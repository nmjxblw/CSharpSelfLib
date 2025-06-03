using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Network;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class Cabin : FarmHouse
{
	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Locations.Cabin.owner" /> instead.</summary>
	[XmlElement("farmhand")]
	public Farmer obsolete_farmhand;

	/// <summary>A net reference to the farmhand who owns this cabin. Most code should use <see cref="P:StardewValley.Locations.Cabin.owner" /> instead.</summary>
	[XmlElement("farmhandReference")]
	public readonly NetFarmerRef farmhandReference = new NetFarmerRef();

	[XmlIgnore]
	public readonly NetMutex inventoryMutex = new NetMutex();

	/// <inheritdoc />
	[XmlIgnore]
	public override Farmer owner => this.farmhandReference.Value;

	public Cabin()
	{
	}

	public Cabin(string map)
		: base(map, "Cabin")
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.farmhandReference.NetFields, "farmhandReference.NetFields").AddField(this.inventoryMutex.NetFields, "inventoryMutex.NetFields");
	}

	public void CreateFarmhand()
	{
		if (!this.HasOwner)
		{
			long newId;
			do
			{
				newId = Utility.RandomLong();
			}
			while (Game1.GetPlayer(newId) != null);
			Farmer newFarmer = new Farmer(new FarmerSprite(null), new Vector2(0f, 0f), 1, "", Farmer.initialTools(), isMale: true)
			{
				UniqueMultiplayerID = newId
			};
			newFarmer.addQuest("9");
			newFarmer.homeLocation.Value = base.NameOrUniqueName;
			Game1.netWorldState.Value.farmhandData[newFarmer.UniqueMultiplayerID] = newFarmer;
			this.AssignFarmhand(newFarmer);
			Game1.netWorldState.Value.ResetFarmhandState(newFarmer);
		}
	}

	/// <summary>Fully delete the farmhand associated with this cabin. This will permanently remove their data if the game is saved.</summary>
	public void DeleteFarmhand()
	{
		if (this.HasOwner)
		{
			Game1.player.team.DeleteFarmhand(this.owner);
			this.farmhandReference.Value = null;
		}
	}

	/// <summary>Get whether this cabin is available to assign to a farmhand.</summary>
	/// <param name="farmhand">The farmhand to check.</param>
	public bool CanAssignTo(Farmer farmhand)
	{
		if (this.HasOwner && this.OwnerId != farmhand.UniqueMultiplayerID)
		{
			return this.owner.isUnclaimedFarmhand;
		}
		return true;
	}

	/// <summary>Assign a farmhand to this cabin.</summary>
	/// <param name="farmhand">The farmhand to assign to this cabin.</param>
	/// <exception cref="T:System.InvalidOperationException">The farmhand can't be assigned to this cabin because an existing player is already assigned. You must call <see cref="M:StardewValley.Locations.Cabin.DeleteFarmhand" /> first in that case.</exception>
	public void AssignFarmhand(Farmer farmhand)
	{
		if (this.HasOwner && this.OwnerId != farmhand.UniqueMultiplayerID)
		{
			if (!this.owner.isUnclaimedFarmhand)
			{
				throw new InvalidOperationException($"Can't assign cabin to {farmhand.Name} ({farmhand.UniqueMultiplayerID}) because it's already assigned to {this.owner.Name} ({this.owner.UniqueMultiplayerID}).");
			}
			this.DeleteFarmhand();
		}
		this.farmhandReference.Value = farmhand;
		farmhand.homeLocation.Value = base.NameOrUniqueName;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "indoor");
		if ((uint)(tileIndexAt - 647) <= 1u && !base.IsOwnerActivated)
		{
			this.inventoryMutex.RequestLock(delegate
			{
				base.playSound("Ship");
				this.openFarmhandInventory();
			});
			return true;
		}
		if (base.checkAction(tileLocation, viewport, who))
		{
			return true;
		}
		return false;
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
		this.inventoryMutex.Update(Game1.getOnlineFarmers());
		if (this.inventoryMutex.IsLockHeld() && !(Game1.activeClickableMenu is ItemGrabMenu))
		{
			this.inventoryMutex.ReleaseLock();
		}
	}

	public IInventory getInventory()
	{
		return this.owner?.Items;
	}

	public void openFarmhandInventory()
	{
		Game1.activeClickableMenu = new ItemGrabMenu(this.getInventory(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromPlayerInventory, null, grabItemFromFarmhandInventory, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, this);
	}

	public bool isInventoryOpen()
	{
		return this.inventoryMutex.IsLocked();
	}

	private void grabItemFromPlayerInventory(Item item, Farmer who)
	{
		if (this.HasOwner)
		{
			item.FixStackSize();
			Item tmp = this.owner.addItemToInventory(item);
			if (tmp == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				who.addItemToInventory(tmp);
			}
			int oldID = ((Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1));
			this.openFarmhandInventory();
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	private void grabItemFromFarmhandInventory(Item item, Farmer who)
	{
		if (who.couldInventoryAcceptThisItem(item))
		{
			this.getInventory().Remove(item);
			this.openFarmhandInventory();
		}
	}

	public override void updateWarps()
	{
		if (!Game1.IsClient)
		{
			base.updateWarps();
		}
	}

	public List<Item> demolish()
	{
		List<Item> items = new List<Item>(this.getInventory()).Where((Item item) => item != null).ToList();
		this.getInventory().Clear();
		Farmer.removeInitialTools(items);
		foreach (NPC npc in new List<NPC>(base.characters))
		{
			if (npc.IsVillager && Game1.characterData.ContainsKey(npc.Name))
			{
				npc.reloadDefaultLocation();
				npc.ClearSchedule();
				Game1.warpCharacter(npc, npc.DefaultMap, npc.DefaultPosition / 64f);
			}
			if (npc is Pet pet)
			{
				pet.warpToFarmHouse(Game1.MasterPlayer);
			}
		}
		Cellar cellar = base.GetCellar();
		if (cellar != null)
		{
			cellar.objects.Clear();
			cellar.setUpAgingBoards();
		}
		if (this.HasOwner)
		{
			Game1.player.team.DeleteFarmhand(this.owner);
		}
		Game1.updateCellarAssignments();
		return items;
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		if (this.HasOwner)
		{
			this.owner.stamina = this.owner.MaxStamina;
		}
	}

	public override Point getPorchStandingSpot()
	{
		return base.ParentBuilding?.getPorchStandingSpot() ?? base.getPorchStandingSpot();
	}
}
