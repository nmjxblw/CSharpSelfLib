using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Objects;

public class Workbench : Object
{
	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.mutex.NetFields, "mutex.NetFields");
	}

	public Workbench()
	{
	}

	public Workbench(Vector2 position)
		: base(position, "208")
	{
		this.Name = "Workbench";
		base.type.Value = "Crafting";
		base.bigCraftable.Value = true;
		base.canBeSetDown.Value = true;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		if (justCheckingForActivity)
		{
			return true;
		}
		List<Chest> nearby_chests = new List<Chest>();
		Point? fridgePosition = location.GetFridgePosition();
		Vector2[] neighbor_tiles = new Vector2[8]
		{
			new Vector2(-1f, 1f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(-1f, 0f),
			new Vector2(1f, 0f),
			new Vector2(-1f, -1f),
			new Vector2(0f, -1f),
			new Vector2(1f, -1f)
		};
		for (int i = 0; i < neighbor_tiles.Length; i++)
		{
			Vector2 tile_location = new Vector2((int)(base.tileLocation.X + neighbor_tiles[i].X), (int)(base.tileLocation.Y + neighbor_tiles[i].Y));
			if ((int)base.tileLocation.X == fridgePosition?.X && (int)base.tileLocation.Y == fridgePosition.Value.Y)
			{
				Chest fridge = location.GetFridge();
				if (fridge != null)
				{
					nearby_chests.Add(fridge);
				}
			}
			if (location.objects.TryGetValue(tile_location, out var neighbor_object) && neighbor_object is Chest chest && (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
			{
				nearby_chests.Add(chest);
			}
		}
		List<NetMutex> muticies = new List<NetMutex>();
		List<IInventory> inventories = new List<IInventory>();
		foreach (Chest chest2 in nearby_chests)
		{
			muticies.Add(chest2.mutex);
			inventories.Add(chest2.Items);
		}
		if (!this.mutex.IsLocked())
		{
			new MultipleMutexRequest(muticies, delegate(MultipleMutexRequest request)
			{
				this.mutex.RequestLock(delegate
				{
					Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
					Game1.activeClickableMenu = new CraftingPage((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: false, standaloneMenu: true, inventories);
					Game1.activeClickableMenu.exitFunction = delegate
					{
						this.mutex.ReleaseLock();
						request.ReleaseLocks();
					};
				}, request.ReleaseLocks);
			}, delegate
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
			});
		}
		return true;
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = this.Location;
		if (environment != null)
		{
			this.mutex.Update(environment);
		}
		base.updateWhenCurrentLocation(time);
	}
}
