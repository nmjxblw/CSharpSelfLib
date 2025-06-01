using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Network.ChestHit;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Objects;

public class Chest : Object
{
	public enum SpecialChestTypes
	{
		None,
		MiniShippingBin,
		JunimoChest,
		AutoLoader,
		Enricher,
		[Obsolete("This value is only used in mobile versions of the game.")]
		Mill,
		BigChest
	}

	public const int capacity = 36;

	/// <summary>The underlying <see cref="T:StardewValley.Network.ChestHit.ChestHitTimer" /> instance used by <see cref="P:StardewValley.Objects.Chest.HitTimerInstance" />.</summary>
	internal ChestHitTimer hitTimerInstance;

	[XmlElement("currentLidFrame")]
	public readonly NetInt startingLidFrame = new NetInt(501);

	public readonly NetInt lidFrameCount = new NetInt(5);

	private int currentLidFrame;

	[XmlElement("frameCounter")]
	public readonly NetInt frameCounter = new NetInt(-1);

	/// <summary>The backing field for <see cref="P:StardewValley.Objects.Chest.Items" />.</summary>
	[XmlElement("items")]
	public NetRef<Inventory> netItems = new NetRef<Inventory>(new Inventory());

	public readonly NetLongDictionary<Inventory, NetRef<Inventory>> separateWalletItems = new NetLongDictionary<Inventory, NetRef<Inventory>>();

	[XmlElement("tint")]
	public readonly NetColor tint = new NetColor(Color.White);

	[XmlElement("playerChoiceColor")]
	public readonly NetColor playerChoiceColor = new NetColor(Color.Black);

	[XmlElement("playerChest")]
	public readonly NetBool playerChest = new NetBool();

	[XmlElement("fridge")]
	public readonly NetBool fridge = new NetBool();

	/// <summary>Whether this is a gift box. This changes the chest's appearance, and when the player interacts with the chest they'll receive all the items directly and the chest will disappear.</summary>
	[XmlElement("giftbox")]
	public readonly NetBool giftbox = new NetBool();

	/// <summary>If <see cref="F:StardewValley.Objects.Chest.giftbox" /> is true, the sprite index to draw from the <see cref="F:StardewValley.Game1.giftboxName" /> texture.</summary>
	[XmlElement("giftboxIndex")]
	public readonly NetInt giftboxIndex = new NetInt();

	/// <summary>If <see cref="F:StardewValley.Objects.Chest.giftbox" /> is true, whether this is the starter gift for a player in their cabin or farmhouse.</summary>
	public readonly NetBool giftboxIsStarterGift = new NetBool();

	[XmlElement("spriteIndexOverride")]
	public readonly NetInt bigCraftableSpriteIndex = new NetInt(-1);

	[XmlElement("dropContents")]
	public readonly NetBool dropContents = new NetBool(value: false);

	[XmlIgnore]
	public string mailToAddOnItemDump;

	[XmlElement("synchronized")]
	public readonly NetBool synchronized = new NetBool(value: false);

	[XmlIgnore]
	public int _shippingBinFrameCounter;

	[XmlIgnore]
	public bool _farmerNearby;

	[XmlIgnore]
	public NetVector2 kickStartTile = new NetVector2(new Vector2(-1000f, -1000f));

	[XmlIgnore]
	public Vector2? localKickStartTile;

	[XmlIgnore]
	public float kickProgress = -1f;

	[XmlIgnore]
	public readonly NetEvent0 openChestEvent = new NetEvent0();

	[XmlElement("specialChestType")]
	public readonly NetEnum<SpecialChestTypes> specialChestType = new NetEnum<SpecialChestTypes>();

	/// <summary>The backing field for <see cref="P:StardewValley.Objects.Chest.GlobalInventoryId" />.</summary>
	public readonly NetString globalInventoryId = new NetString();

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	/// <summary>A read-only <see cref="T:StardewValley.Network.ChestHit.ChestHitTimer" /> that is automatically created or fetched from <see cref="F:StardewValley.Network.ChestHit.ChestHitSynchronizer.SavedTimers" />.</summary>
	private ChestHitTimer HitTimerInstance
	{
		get
		{
			if (this.hitTimerInstance != null)
			{
				return this.hitTimerInstance;
			}
			this.hitTimerInstance = new ChestHitTimer();
			if (Game1.IsMasterGame || this.Location == null)
			{
				return this.hitTimerInstance;
			}
			if (!Game1.player.team.chestHit.SavedTimers.TryGetValue(this.Location.NameOrUniqueName, out var localTimers))
			{
				return this.hitTimerInstance;
			}
			ulong tileHash = ChestHitSynchronizer.HashPosition((int)this.TileLocation.X, (int)this.TileLocation.Y);
			if (localTimers.TryGetValue(tileHash, out var timer))
			{
				this.hitTimerInstance = timer;
				localTimers.Remove(tileHash);
				if (timer.SavedTime >= 0 && Game1.currentGameTime != null)
				{
					timer.Milliseconds -= (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - timer.SavedTime;
					timer.SavedTime = -1;
				}
			}
			return this.hitTimerInstance;
		}
	}

	[XmlIgnore]
	public SpecialChestTypes SpecialChestType
	{
		get
		{
			return this.specialChestType.Value;
		}
		set
		{
			this.specialChestType.Value = value;
		}
	}

	/// <summary>If set, the inventory ID in <see cref="F:StardewValley.FarmerTeam.globalInventories" /> to use for this chest instead of its local item list.</summary>
	[XmlIgnore]
	public string GlobalInventoryId
	{
		get
		{
			return this.globalInventoryId.Value;
		}
		set
		{
			this.globalInventoryId.Value = value;
		}
	}

	[XmlIgnore]
	public Color Tint
	{
		get
		{
			return this.tint.Value;
		}
		set
		{
			this.tint.Value = value;
		}
	}

	[XmlIgnore]
	public Inventory Items => this.netItems.Value;

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.startingLidFrame, "startingLidFrame").AddField(this.frameCounter, "frameCounter").AddField(this.netItems, "netItems")
			.AddField(this.tint, "tint")
			.AddField(this.playerChoiceColor, "playerChoiceColor")
			.AddField(this.playerChest, "playerChest")
			.AddField(this.fridge, "fridge")
			.AddField(this.giftbox, "giftbox")
			.AddField(this.giftboxIndex, "giftboxIndex")
			.AddField(this.giftboxIsStarterGift, "giftboxIsStarterGift")
			.AddField(this.mutex.NetFields, "mutex.NetFields")
			.AddField(this.lidFrameCount, "lidFrameCount")
			.AddField(this.bigCraftableSpriteIndex, "bigCraftableSpriteIndex")
			.AddField(this.dropContents, "dropContents")
			.AddField(this.openChestEvent.NetFields, "openChestEvent.NetFields")
			.AddField(this.synchronized, "synchronized")
			.AddField(this.specialChestType, "specialChestType")
			.AddField(this.kickStartTile, "kickStartTile")
			.AddField(this.separateWalletItems, "separateWalletItems")
			.AddField(this.globalInventoryId, "globalInventoryId");
		this.openChestEvent.onEvent += performOpenChest;
		this.kickStartTile.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 old_value, Vector2 new_value)
		{
			if (Game1.gameMode != 6 && new_value.X != -1000f && new_value.Y != -1000f)
			{
				this.localKickStartTile = this.kickStartTile.Value;
				this.kickProgress = 0f;
			}
		};
	}

	public Chest()
	{
		this.Name = "Chest";
		base.type.Value = "interactive";
	}

	public Chest(bool playerChest, Vector2 tileLocation, string itemId = "130")
		: base(tileLocation, itemId)
	{
		this.Name = "Chest";
		base.type.Value = "Crafting";
		if (playerChest)
		{
			this.playerChest.Value = playerChest;
			this.startingLidFrame.Value = base.ParentSheetIndex + 1;
			base.bigCraftable.Value = true;
			base.canBeSetDown.Value = true;
		}
		else
		{
			this.lidFrameCount.Value = 3;
		}
		this.SetSpecialChestType();
	}

	public Chest(bool playerChest, string itemId = "130")
		: base(Vector2.Zero, itemId)
	{
		this.Name = "Chest";
		base.type.Value = "Crafting";
		if (playerChest)
		{
			this.playerChest.Value = playerChest;
			this.startingLidFrame.Value = base.ParentSheetIndex + 1;
			base.bigCraftable.Value = true;
			base.canBeSetDown.Value = true;
		}
		else
		{
			this.lidFrameCount.Value = 3;
		}
	}

	public Chest(string itemId, Vector2 tile_location, int starting_lid_frame, int lid_frame_count)
		: base(tile_location, itemId)
	{
		this.playerChest.Value = true;
		this.startingLidFrame.Value = starting_lid_frame;
		this.lidFrameCount.Value = lid_frame_count;
		base.bigCraftable.Value = true;
		base.canBeSetDown.Value = true;
	}

	public Chest(List<Item> items, Vector2 location, bool giftbox = false, int giftboxIndex = 0, bool giftboxIsStarterGift = false)
	{
		base.name = "Chest";
		base.type.Value = "interactive";
		this.giftbox.Value = giftbox;
		this.giftboxIndex.Value = giftboxIndex;
		this.giftboxIsStarterGift.Value = giftboxIsStarterGift;
		if (!this.giftbox.Value)
		{
			this.lidFrameCount.Value = 3;
		}
		if (items != null)
		{
			this.Items.OverwriteWith(items);
		}
		this.TileLocation = location;
	}

	public void resetLidFrame()
	{
		this.currentLidFrame = this.startingLidFrame.Value;
	}

	public void fixLidFrame()
	{
		if (this.currentLidFrame == 0)
		{
			this.currentLidFrame = this.startingLidFrame.Value;
		}
		if (this.SpecialChestType == SpecialChestTypes.MiniShippingBin)
		{
			return;
		}
		if (this.playerChest.Value)
		{
			if (this.GetMutex().IsLocked() && !this.GetMutex().IsLockHeld())
			{
				this.currentLidFrame = this.getLastLidFrame();
			}
			else if (!this.GetMutex().IsLocked())
			{
				this.currentLidFrame = this.startingLidFrame.Value;
			}
		}
		else if (this.currentLidFrame == this.startingLidFrame.Value && this.GetMutex().IsLocked() && !this.GetMutex().IsLockHeld())
		{
			this.currentLidFrame = this.getLastLidFrame();
		}
	}

	public int getLastLidFrame()
	{
		return this.startingLidFrame.Value + this.lidFrameCount.Value - 1;
	}

	/// <summary>Handles a player hitting this chest.</summary>
	/// <param name="args">The arguments for the chest hit event.</param>
	public void HandleChestHit(ChestHitArgs args)
	{
		if (!Game1.IsMasterGame)
		{
			Game1.log.Warn("Attempted to call Chest::HandleChestHit as a farmhand.");
			return;
		}
		if (this.TileLocation.X == 0f && this.TileLocation.Y == 0f)
		{
			this.TileLocation = Utility.PointToVector2(args.ChestTile);
		}
		this.GetMutex().RequestLock(delegate
		{
			this.clearNulls();
			if (this.isEmpty())
			{
				this.performRemoveAction();
				if (this.Location.Objects.Remove(Utility.PointToVector2(args.ChestTile)) && base.Type == "Crafting" && base.fragility.Value != 2)
				{
					this.Location.debris.Add(new Debris(base.QualifiedItemId, args.ToolPosition, Utility.PointToVector2(args.StandingPixel)));
				}
				Game1.player.team.chestHit.SignalDelete(this.Location, args.ChestTile.X, args.ChestTile.Y);
			}
			else if (args.ToolCanHit)
			{
				if (args.HoldDownClick || args.RecentlyHit)
				{
					if (this.kickStartTile.Value == this.TileLocation)
					{
						this.kickStartTile.Value = new Vector2(-1000f, -1000f);
					}
					this.TryMoveToSafePosition(args.Direction);
					Game1.player.team.chestHit.SignalMove(this.Location, args.ChestTile.X, args.ChestTile.Y, (int)this.TileLocation.X, (int)this.TileLocation.Y);
				}
				else
				{
					this.kickStartTile.Value = this.TileLocation;
				}
			}
			this.GetMutex().ReleaseLock();
		});
	}

	public override bool performToolAction(Tool t)
	{
		if (t?.getLastFarmerToUse() != null && t.getLastFarmerToUse() != Game1.player)
		{
			return false;
		}
		if (this.playerChest.Value)
		{
			if (t == null)
			{
				return false;
			}
			if (t is MeleeWeapon || !t.isHeavyHitter())
			{
				return false;
			}
			if (base.performToolAction(t))
			{
				GameLocation location = this.Location;
				Farmer player = t.getLastFarmerToUse();
				if (player != null)
				{
					Vector2 c = this.TileLocation;
					if (c.X == 0f && c.Y == 0f)
					{
						bool found = false;
						foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
						{
							if (pair.Value == this)
							{
								c.X = (int)pair.Key.X;
								c.Y = (int)pair.Key.Y;
								found = true;
								break;
							}
						}
						if (!found)
						{
							c = player.GetToolLocation() / 64f;
							c.X = (int)c.X;
							c.Y = (int)c.Y;
						}
					}
					if (!this.GetMutex().IsLocked())
					{
						ChestHitArgs args = new ChestHitArgs();
						args.Location = location;
						args.ChestTile = new Point((int)this.TileLocation.X, (int)this.TileLocation.Y);
						args.ToolPosition = player.GetToolLocation();
						args.StandingPixel = player.StandingPixel;
						args.Direction = player.FacingDirection;
						args.HoldDownClick = t != player.CurrentTool;
						args.ToolCanHit = t.isHeavyHitter() && !(t is MeleeWeapon);
						args.RecentlyHit = this.HitTimerInstance.Milliseconds > 0;
						if (args.ToolCanHit)
						{
							base.shakeTimer = 100;
							this.HitTimerInstance.Milliseconds = 10000;
						}
						if (args.ChestTile.X == 0 && args.ChestTile.Y == 0)
						{
							if (location.getObjectAtTile((int)c.X, (int)c.Y) != this)
							{
								return false;
							}
							args.ChestTile = new Point((int)c.X, (int)c.Y);
						}
						Game1.player.team.chestHit.Sync(args);
					}
				}
			}
			return false;
		}
		if (t is Pickaxe && this.currentLidFrame == this.getLastLidFrame() && this.frameCounter.Value == -1 && this.isEmpty())
		{
			this.Location.playSound("woodWhack");
			for (int i = 0; i < 8; i++)
			{
				Game1.multiplayer.broadcastSprites(this.Location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4) : new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4), 999f, 1, 0, base.tileLocation.Value * 64f + new Vector2(32f, 64f), flicker: false, Game1.random.NextDouble() < 0.5, (base.tileLocation.Y * 64f + 64f) / 10000f, 0.01f, new Color(204, 132, 87), 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
				{
					motion = new Vector2((float)Game1.random.Next(-25, 26) / 10f, Game1.random.Next(-11, -8)),
					acceleration = new Vector2(0f, 0.3f)
				});
			}
			Game1.createRadialDebris(this.Location, 12, (int)base.tileLocation.X, (int)base.tileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, new Color(204, 132, 87));
			return true;
		}
		return false;
	}

	/// <summary>Try to shove this chest onto an unoccupied nearby tile.</summary>
	/// <param name="preferDirection">The direction in which to move the chest if possible, matching a constant like <see cref="F:StardewValley.Game1.up" />.</param>
	/// <returns>Returns whether the chest was successfully moved to an unoccupied space.</returns>
	public bool TryMoveToSafePosition(int? preferDirection = null)
	{
		GameLocation location = this.Location;
		Vector2? prioritizeDirection = preferDirection switch
		{
			1 => new Vector2(1f, 0f), 
			3 => new Vector2(-1f, 0f), 
			0 => new Vector2(0f, -1f), 
			_ => new Vector2(0f, 1f), 
		};
		return TryMoveRecursively(base.tileLocation.Value, 0, prioritizeDirection);
		bool TryMoveRecursively(Vector2 tile_position, int depth, Vector2? prioritize_direction)
		{
			List<Vector2> offsets = new List<Vector2>();
			offsets.AddRange(new Vector2[4]
			{
				new Vector2(1f, 0f),
				new Vector2(-1f, 0f),
				new Vector2(0f, -1f),
				new Vector2(0f, 1f)
			});
			Utility.Shuffle(Game1.random, offsets);
			if (prioritize_direction.HasValue)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			foreach (Vector2 offset in offsets)
			{
				Vector2 new_position = tile_position + offset;
				if (this.canBePlacedHere(location, new_position) && location.CanItemBePlacedHere(new_position))
				{
					if (!location.objects.ContainsKey(new_position) && location.objects.Remove(this.TileLocation))
					{
						this.kickStartTile.Value = this.TileLocation;
						this.TileLocation = new_position;
						location.objects[new_position] = this;
					}
					return true;
				}
			}
			Utility.Shuffle(Game1.random, offsets);
			if (prioritize_direction.HasValue)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			if (depth < 3)
			{
				foreach (Vector2 offset2 in offsets)
				{
					Vector2 new_position2 = tile_position + offset2;
					if (location.isPointPassable(new Location((int)(new_position2.X + 0.5f) * 64, (int)(new_position2.Y + 0.5f) * 64), Game1.viewport) && TryMoveRecursively(new_position2, depth + 1, prioritize_direction))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	/// <inheritdoc />
	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		this.localKickStartTile = null;
		this.kickProgress = -1f;
		return base.placementAction(location, x, y, who);
	}

	/// <summary>Set the special chest type based on the chest's item ID.</summary>
	public void SetSpecialChestType()
	{
		switch (base.QualifiedItemId)
		{
		case "(BC)BigChest":
		case "(BC)BigStoneChest":
			this.SpecialChestType = SpecialChestTypes.BigChest;
			break;
		case "(BC)248":
			this.SpecialChestType = SpecialChestTypes.MiniShippingBin;
			break;
		case "(BC)256":
			this.SpecialChestType = SpecialChestTypes.JunimoChest;
			break;
		case "(BC)275":
			this.SpecialChestType = SpecialChestTypes.AutoLoader;
			break;
		}
	}

	public void destroyAndDropContents(Vector2 pointToDropAt)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return;
		}
		List<Item> item_list = new List<Item>();
		item_list.AddRange(this.Items);
		if (this.SpecialChestType == SpecialChestTypes.MiniShippingBin)
		{
			foreach (Inventory separate_wallet_item_list in this.separateWalletItems.Values)
			{
				item_list.AddRange(separate_wallet_item_list);
			}
		}
		if (item_list.Count > 0)
		{
			location.playSound("throwDownITem");
		}
		foreach (Item item in item_list)
		{
			if (item != null)
			{
				Game1.createItemDebris(item, pointToDropAt, Game1.random.Next(4), location);
			}
		}
		this.Items.Clear();
		this.separateWalletItems.Clear();
		this.clearNulls();
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		if (dropInItem != null && dropInItem.QualifiedItemId != base.QualifiedItemId && dropInItem.HasContextTag("swappable_chest") && base.HasContextTag("swappable_chest") && this.Location != null)
		{
			if (!probe)
			{
				if (this.GetMutex().IsLocked())
				{
					return false;
				}
				Chest newChest = new Chest(playerChest: true, this.TileLocation, dropInItem.ItemId);
				int newCapacity = newChest.GetActualCapacity();
				if (newCapacity < this.GetActualCapacity() && newCapacity < this.Items.CountItemStacks())
				{
					return false;
				}
				if (newCapacity < this.Items.Count)
				{
					this.clearNulls();
				}
				newChest.netItems.Value = this.netItems.Value;
				newChest.playerChoiceColor.Value = this.playerChoiceColor.Value;
				newChest.Tint = this.Tint;
				newChest.modData.CopyFrom(base.modData);
				GameLocation location = this.Location;
				location.Objects.Remove(this.TileLocation);
				location.Objects.Add(this.TileLocation, newChest);
				Game1.createMultipleItemDebris(ItemRegistry.Create(base.QualifiedItemId), this.TileLocation * 64f + new Vector2(32f), -1);
				this.Location.playSound("axchop");
			}
			return true;
		}
		return base.performObjectDropInAction(dropInItem, probe, who);
	}

	public void dumpContents()
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return;
		}
		IInventory items = this.Items;
		if (this.synchronized.Value && (this.GetMutex().IsLocked() || !Game1.IsMasterGame) && !this.GetMutex().IsLockHeld())
		{
			return;
		}
		if (items.Count > 0 && (this.GetMutex().IsLockHeld() || !this.playerChest.Value))
		{
			if (this.giftbox.Value && this.giftboxIsStarterGift.Value && location is FarmHouse house)
			{
				if (!house.IsOwnedByCurrentPlayer)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Objects:ParsnipSeedPackage_SomeoneElse"));
					return;
				}
				Game1.player.addQuest((Game1.GetFarmTypeID() == "MeadowlandsFarm") ? "132" : "6");
				Game1.dayTimeMoneyBox.PingQuestLog();
			}
			foreach (Item item in items)
			{
				if (item == null)
				{
					continue;
				}
				item.SetTempData("FromStarterGiftBox", value: true);
				if (item.QualifiedItemId == "(O)434")
				{
					if (Game1.player.mailReceived.Add((location is FarmHouse) ? "CF_Spouse" : "CF_Mines"))
					{
						Game1.player.eatObject(items[0] as Object, overrideFullness: true);
					}
				}
				else if (this.dropContents.Value)
				{
					Game1.createItemDebris(item, base.tileLocation.Value * 64f, -1, location);
					if (location is VolcanoDungeon)
					{
						switch (this.bigCraftableSpriteIndex.Value)
						{
						case 223:
							Game1.player.team.RequestLimitedNutDrops("VolcanoNormalChest", location, (int)base.tileLocation.Value.X * 64, (int)base.tileLocation.Value.Y * 64, 1);
							break;
						case 227:
							Game1.player.team.RequestLimitedNutDrops("VolcanoRareChest", location, (int)base.tileLocation.Value.X * 64, (int)base.tileLocation.Value.Y * 64, 1);
							break;
						}
					}
				}
				else if (!this.synchronized.Value || this.GetMutex().IsLockHeld())
				{
					item.onDetachedFromParent();
					if (Game1.activeClickableMenu is ItemGrabMenu grabMenu)
					{
						grabMenu.ItemsToGrabMenu.actualInventory.Add(item);
					}
					else
					{
						Game1.player.addItemByMenuIfNecessaryElseHoldUp(item);
					}
					if (this.mailToAddOnItemDump != null)
					{
						Game1.player.mailReceived.Add(this.mailToAddOnItemDump);
					}
					if (location is Caldera || Game1.player.currentLocation is Caldera)
					{
						Game1.player.mailReceived.Add("CalderaTreasure");
					}
				}
			}
			items.Clear();
			this.clearNulls();
			Game1.mine?.chestConsumed();
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			ItemGrabMenu grabMenu2 = activeClickableMenu as ItemGrabMenu;
			if (grabMenu2 != null)
			{
				ItemGrabMenu itemGrabMenu = grabMenu2;
				itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
				{
					grabMenu2.DropRemainingItems();
				});
			}
		}
		Game1.player.gainExperience(5, 25 + Game1.CurrentMineLevel);
		if (this.giftbox.Value)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("LooseSprites\\Giftbox", new Microsoft.Xna.Framework.Rectangle(0, this.giftboxIndex.Value * 32, 16, 32), 80f, 11, 1, base.tileLocation.Value * 64f - new Vector2(0f, 52f), flicker: false, flipped: false, base.tileLocation.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				destroyable = false,
				holdLastFrame = true
			};
			if (location.netObjects.TryGetValue(base.tileLocation.Value, out var tileObj) && tileObj == this)
			{
				Game1.multiplayer.broadcastSprites(location, sprite);
				location.removeObject(base.tileLocation.Value, showDestroyedObject: false);
			}
			else
			{
				location.temporarySprites.Add(sprite);
			}
		}
	}

	public NetMutex GetMutex()
	{
		if (this.GlobalInventoryId != null)
		{
			return Game1.player.team.GetOrCreateGlobalInventoryMutex(this.GlobalInventoryId);
		}
		if (this.specialChestType.Value == SpecialChestTypes.JunimoChest)
		{
			return Game1.player.team.GetOrCreateGlobalInventoryMutex("JunimoChests");
		}
		return this.mutex;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		GameLocation location = this.Location;
		IInventory items = this.GetItemsForPlayer();
		if (this.giftbox.Value)
		{
			Game1.player.Halt();
			Game1.player.freezePause = 1000;
			location.playSound("Ship");
			this.dumpContents();
		}
		else if (this.playerChest.Value)
		{
			if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				return false;
			}
			this.GetMutex().RequestLock(delegate
			{
				if (this.SpecialChestType == SpecialChestTypes.MiniShippingBin)
				{
					this.OpenMiniShippingMenu();
				}
				else
				{
					this.frameCounter.Value = 5;
					Game1.playSound(this.fridge.Value ? "doorCreak" : "openChest");
					Game1.player.Halt();
					Game1.player.freezePause = 1000;
				}
			});
		}
		else if (!this.playerChest.Value)
		{
			if (this.currentLidFrame == this.startingLidFrame.Value && this.frameCounter.Value <= -1)
			{
				location.playSound("openChest");
				if (this.synchronized.Value)
				{
					this.GetMutex().RequestLock(this.openChestEvent.Fire);
				}
				else
				{
					this.performOpenChest();
				}
			}
			else if (this.currentLidFrame == this.getLastLidFrame() && items.Count > 0 && !this.synchronized.Value)
			{
				Item item = items[0];
				items.RemoveAt(0);
				if (Game1.mine != null)
				{
					Game1.mine.chestConsumed();
				}
				who.addItemByMenuIfNecessaryElseHoldUp(item);
				IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
				ItemGrabMenu grab_menu = activeClickableMenu as ItemGrabMenu;
				if (grab_menu != null)
				{
					ItemGrabMenu itemGrabMenu = grab_menu;
					itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
					{
						grab_menu.DropRemainingItems();
					});
				}
			}
		}
		if (items.Count == 0 && (!this.playerChest.Value || this.giftbox.Value))
		{
			location.removeObject(this.TileLocation, showDestroyedObject: false);
			location.playSound("woodWhack");
			for (int i = 0; i < 8; i++)
			{
				Game1.multiplayer.broadcastSprites(this.Location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4) : new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4), 999f, 1, 0, base.tileLocation.Value * 64f + new Vector2(32f, 64f), flicker: false, Game1.random.NextDouble() < 0.5, (base.tileLocation.Y * 64f + 64f) / 10000f, 0.01f, new Color(204, 132, 87), 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
				{
					motion = new Vector2((float)Game1.random.Next(-25, 26) / 10f, Game1.random.Next(-11, -8)),
					acceleration = new Vector2(0f, 0.3f)
				});
			}
			Game1.createRadialDebris(location, 12, (int)base.tileLocation.X, (int)base.tileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, new Color(204, 132, 87));
		}
		return true;
	}

	public virtual void OpenMiniShippingMenu()
	{
		Game1.playSound("shwip");
		this.ShowMenu();
	}

	public virtual void performOpenChest()
	{
		this.frameCounter.Value = 5;
	}

	public virtual void grabItemFromChest(Item item, Farmer who)
	{
		if (who.couldInventoryAcceptThisItem(item))
		{
			this.GetItemsForPlayer().Remove(item);
			this.clearNulls();
			this.ShowMenu();
		}
	}

	public virtual Item addItem(Item item)
	{
		item.resetState();
		this.clearNulls();
		IInventory item_list = this.GetItemsForPlayer();
		for (int i = 0; i < item_list.Count; i++)
		{
			if (item_list[i] != null && item_list[i].canStackWith(item))
			{
				int toRemove = item.Stack - item_list[i].addToStack(item);
				if (item.ConsumeStack(toRemove) == null)
				{
					return null;
				}
			}
		}
		if (item_list.Count < this.GetActualCapacity())
		{
			item_list.Add(item);
			return null;
		}
		return item;
	}

	public virtual int GetActualCapacity()
	{
		switch (this.SpecialChestType)
		{
		case SpecialChestTypes.MiniShippingBin:
		case SpecialChestTypes.JunimoChest:
			return 9;
		case SpecialChestTypes.Enricher:
			return 1;
		case SpecialChestTypes.BigChest:
			return 70;
		default:
			return 36;
		}
	}

	/// <summary>If there's an object below this chest, try to auto-load its inventory from this chest.</summary>
	/// <param name="who">The player who interacted with the chest.</param>
	public virtual void CheckAutoLoad(Farmer who)
	{
		GameLocation location = this.Location;
		Vector2 tile = this.TileLocation;
		if (location != null && location.objects.TryGetValue(new Vector2(tile.X, tile.Y + 1f), out var beneath_object))
		{
			beneath_object?.AttemptAutoLoad(who);
		}
	}

	public virtual void ShowMenu()
	{
		ItemGrabMenu oldMenu = Game1.activeClickableMenu as ItemGrabMenu;
		switch (this.SpecialChestType)
		{
		case SpecialChestTypes.MiniShippingBin:
			Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, Utility.highlightShippableObjects, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 1, this, -1, this);
			break;
		case SpecialChestTypes.JunimoChest:
			Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
			break;
		case SpecialChestTypes.AutoLoader:
		{
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(this.GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
			itemGrabMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(itemGrabMenu.exitFunction, (IClickableMenu.onExit)delegate
			{
				this.CheckAutoLoad(Game1.player);
			});
			Game1.activeClickableMenu = itemGrabMenu;
			break;
		}
		case SpecialChestTypes.Enricher:
			Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, Object.HighlightFertilizers, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
			break;
		default:
			Game1.activeClickableMenu = new ItemGrabMenu(this.GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
			break;
		}
		if (oldMenu != null && Game1.activeClickableMenu is ItemGrabMenu newMenu)
		{
			newMenu.inventory.moveItemSound = oldMenu.inventory.moveItemSound;
			newMenu.inventory.highlightMethod = oldMenu.inventory.highlightMethod;
		}
	}

	public virtual void grabItemFromInventory(Item item, Farmer who)
	{
		if (item.Stack == 0)
		{
			item.Stack = 1;
		}
		Item tmp = this.addItem(item);
		if (tmp == null)
		{
			who.removeItemFromInventory(item);
		}
		else
		{
			tmp = who.addItemToInventory(tmp);
		}
		this.clearNulls();
		int oldID = ((Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1));
		this.ShowMenu();
		(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
		if (oldID != -1)
		{
			Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
			Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
		}
	}

	public IInventory GetItemsForPlayer()
	{
		return this.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
	}

	public IInventory GetItemsForPlayer(long id)
	{
		if (this.GlobalInventoryId != null)
		{
			return Game1.player.team.GetOrCreateGlobalInventory(this.GlobalInventoryId);
		}
		switch (this.SpecialChestType)
		{
		case SpecialChestTypes.MiniShippingBin:
			if (Game1.player.team.useSeparateWallets.Value && this.SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				if (!this.separateWalletItems.TryGetValue(id, out var items))
				{
					items = (this.separateWalletItems[id] = new Inventory());
				}
				return items;
			}
			break;
		case SpecialChestTypes.JunimoChest:
			return Game1.player.team.GetOrCreateGlobalInventory("JunimoChests");
		}
		return this.Items;
	}

	public virtual bool isEmpty()
	{
		if (this.SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
		{
			foreach (Inventory value in this.separateWalletItems.Values)
			{
				if (value.HasAny())
				{
					return false;
				}
			}
			return true;
		}
		return !this.GetItemsForPlayer().HasAny();
	}

	public virtual void clearNulls()
	{
		this.GetItemsForPlayer().RemoveEmptySlots();
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = this.Location;
		if (environment == null)
		{
			return;
		}
		if (this.synchronized.Value)
		{
			this.openChestEvent.Poll();
		}
		if (this.localKickStartTile.HasValue)
		{
			if (Game1.currentLocation == environment)
			{
				if (this.kickProgress == 0f)
				{
					if (Utility.isOnScreen((this.localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * 64f, 64))
					{
						Game1.playSound("clubhit");
					}
					base.shakeTimer = 100;
				}
			}
			else
			{
				this.localKickStartTile = null;
				this.kickProgress = -1f;
			}
			if (this.kickProgress >= 0f)
			{
				float move_duration = 0.25f;
				this.kickProgress += (float)(time.ElapsedGameTime.TotalSeconds / (double)move_duration);
				if (this.kickProgress >= 1f)
				{
					this.kickProgress = -1f;
					this.localKickStartTile = null;
				}
			}
		}
		else
		{
			this.kickProgress = -1f;
		}
		this.fixLidFrame();
		this.mutex.Update(environment);
		if (base.shakeTimer > 0)
		{
			base.shakeTimer -= time.ElapsedGameTime.Milliseconds;
			if (base.shakeTimer <= 0)
			{
				base.health = 10;
			}
		}
		this.hitTimerInstance?.Update(time);
		if (this.playerChest.Value)
		{
			if (this.SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				this.UpdateFarmerNearby();
				if (this._shippingBinFrameCounter > -1)
				{
					this._shippingBinFrameCounter--;
					if (this._shippingBinFrameCounter <= 0)
					{
						this._shippingBinFrameCounter = 5;
						if (this._farmerNearby && this.currentLidFrame < this.getLastLidFrame())
						{
							this.currentLidFrame++;
						}
						else if (!this._farmerNearby && this.currentLidFrame > this.startingLidFrame.Value)
						{
							this.currentLidFrame--;
						}
						else
						{
							this._shippingBinFrameCounter = -1;
						}
					}
				}
				if (Game1.activeClickableMenu == null && this.GetMutex().IsLockHeld())
				{
					this.GetMutex().ReleaseLock();
				}
			}
			else if (this.frameCounter.Value > -1 && this.currentLidFrame < this.getLastLidFrame() + 1)
			{
				this.frameCounter.Value--;
				if (this.frameCounter.Value <= 0 && this.GetMutex().IsLockHeld())
				{
					if (this.currentLidFrame == this.getLastLidFrame())
					{
						this.ShowMenu();
						this.frameCounter.Value = -1;
					}
					else
					{
						this.frameCounter.Value = 5;
						this.currentLidFrame++;
					}
				}
			}
			else if (((this.frameCounter.Value == -1 && this.currentLidFrame > this.startingLidFrame.Value) || this.currentLidFrame >= this.getLastLidFrame()) && Game1.activeClickableMenu == null && this.GetMutex().IsLockHeld())
			{
				this.GetMutex().ReleaseLock();
				this.currentLidFrame = this.getLastLidFrame();
				this.frameCounter.Value = 2;
				environment.localSound("doorCreakReverse");
			}
		}
		else
		{
			if (this.frameCounter.Value <= -1 || this.currentLidFrame > this.getLastLidFrame())
			{
				return;
			}
			this.frameCounter.Value--;
			if (this.frameCounter.Value > 0)
			{
				return;
			}
			if (this.currentLidFrame == this.getLastLidFrame())
			{
				this.dumpContents();
				this.frameCounter.Value = -1;
				return;
			}
			this.frameCounter.Value = 10;
			this.currentLidFrame++;
			if (this.currentLidFrame == this.getLastLidFrame())
			{
				this.frameCounter.Value += 5;
			}
		}
	}

	public virtual void UpdateFarmerNearby(bool animate = true)
	{
		GameLocation location = this.Location;
		bool should_open = false;
		Vector2 curTile = base.tileLocation.Value;
		foreach (Farmer farmer in location.farmers)
		{
			Point playerTile = farmer.TilePoint;
			if (Math.Abs((float)playerTile.X - curTile.X) <= 1f && Math.Abs((float)playerTile.Y - curTile.Y) <= 1f)
			{
				should_open = true;
				break;
			}
		}
		if (should_open == this._farmerNearby)
		{
			return;
		}
		this._farmerNearby = should_open;
		this._shippingBinFrameCounter = 5;
		if (!animate)
		{
			this._shippingBinFrameCounter = -1;
			if (this._farmerNearby)
			{
				this.currentLidFrame = this.getLastLidFrame();
			}
			else
			{
				this.currentLidFrame = this.startingLidFrame.Value;
			}
		}
		else if (Game1.gameMode != 6)
		{
			if (this._farmerNearby)
			{
				location.localSound("doorCreak");
			}
			else
			{
				location.localSound("doorCreakReverse");
			}
		}
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		base.actionOnPlayerEntry();
		this.fixLidFrame();
		if (this.specialChestType.Value == SpecialChestTypes.MiniShippingBin)
		{
			this.UpdateFarmerNearby(animate: false);
		}
		this.kickProgress = -1f;
		this.localKickStartTile = null;
		if (!this.playerChest.Value && this.GetItemsForPlayer().Count == 0)
		{
			this.currentLidFrame = this.getLastLidFrame();
		}
	}

	public virtual void SetBigCraftableSpriteIndex(int sprite_index, int starting_lid_frame = -1, int lid_frame_count = 3)
	{
		this.bigCraftableSpriteIndex.Value = sprite_index;
		if (starting_lid_frame >= 0)
		{
			this.startingLidFrame.Value = starting_lid_frame;
		}
		else
		{
			this.startingLidFrame.Value = sprite_index + 1;
		}
		this.lidFrameCount.Value = lid_frame_count;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		float draw_x = x;
		float draw_y = y;
		if (this.localKickStartTile.HasValue)
		{
			draw_x = Utility.Lerp(this.localKickStartTile.Value.X, draw_x, this.kickProgress);
			draw_y = Utility.Lerp(this.localKickStartTile.Value.Y, draw_y, this.kickProgress);
		}
		float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
		if (this.localKickStartTile.HasValue)
		{
			spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((draw_x + 0.5f) * 64f, (draw_y + 0.5f) * 64f)), Game1.shadowTexture.Bounds, Color.Black * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
			draw_y -= (float)Math.Sin((double)this.kickProgress * Math.PI) * 0.5f;
		}
		if (this.playerChest.Value && (base.QualifiedItemId == "(BC)130" || base.QualifiedItemId == "(BC)232" || base.QualifiedItemId.Equals("(BC)BigChest") || base.QualifiedItemId.Equals("(BC)BigStoneChest")))
		{
			if (this.playerChoiceColor.Value.Equals(Color.Black))
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				Texture2D texture = itemData.GetTexture();
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData.GetSourceRect(), this.tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData.GetSourceRect(0, this.currentLidFrame), this.tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				return;
			}
			ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture2 = itemData2.GetTexture();
			int spriteIndex = base.ParentSheetIndex;
			int lidIndex = this.currentLidFrame + 8;
			int coloredLidIndex = this.currentLidFrame;
			string qualifiedItemId = base.QualifiedItemId;
			if (!(qualifiedItemId == "(BC)130"))
			{
				if (qualifiedItemId == "(BC)BigChest")
				{
					spriteIndex = 312;
					lidIndex = this.currentLidFrame + 16;
					coloredLidIndex = this.currentLidFrame + 8;
				}
			}
			else
			{
				spriteIndex = 168;
				lidIndex = this.currentLidFrame + 46;
				coloredLidIndex = this.currentLidFrame + 38;
			}
			Microsoft.Xna.Framework.Rectangle drawRect = itemData2.GetSourceRect(0, spriteIndex);
			Microsoft.Xna.Framework.Rectangle lidRect = itemData2.GetSourceRect(0, lidIndex);
			Microsoft.Xna.Framework.Rectangle coloredLidRect = itemData2.GetSourceRect(0, coloredLidIndex);
			spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), drawRect, this.playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + 20f)), new Microsoft.Xna.Framework.Rectangle(0, spriteIndex / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
			spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), lidRect, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
			spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), coloredLidRect, this.playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
			return;
		}
		if (this.playerChest.Value)
		{
			ParsedItemData itemData3 = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture3 = itemData3.GetTexture();
			spriteBatch.Draw(texture3, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData3.GetSourceRect(), this.tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			spriteBatch.Draw(texture3, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData3.GetSourceRect(0, this.currentLidFrame), this.tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
			return;
		}
		if (this.giftbox.Value)
		{
			spriteBatch.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
			if (this.GetItemsForPlayer().Count > 0)
			{
				int textureY = this.giftboxIndex.Value * 32;
				spriteBatch.Draw(Game1.giftboxTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), draw_y * 64f - 52f)), new Microsoft.Xna.Framework.Rectangle(0, textureY, 16, 32), this.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			}
			return;
		}
		int sprite_index = 500;
		Texture2D sprite_sheet = Game1.objectSpriteSheet;
		int sprite_sheet_height = 16;
		int y_offset = 0;
		if (this.bigCraftableSpriteIndex.Value >= 0)
		{
			sprite_index = this.bigCraftableSpriteIndex.Value;
			sprite_sheet = Game1.bigCraftableSpriteSheet;
			sprite_sheet_height = 32;
			y_offset = -64;
		}
		if (this.bigCraftableSpriteIndex.Value < 0)
		{
			spriteBatch.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
		}
		spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset)), Game1.getSourceRectForStandardTileSheet(sprite_sheet, sprite_index, 16, sprite_sheet_height), this.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
		Vector2 lidPosition = new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset);
		if (this.bigCraftableSpriteIndex.Value < 0)
		{
			switch (this.currentLidFrame)
			{
			case 501:
				lidPosition.Y -= 32f;
				break;
			case 502:
				lidPosition.Y -= 40f;
				break;
			case 503:
				lidPosition.Y -= 60f;
				break;
			}
		}
		spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, lidPosition), Game1.getSourceRectForStandardTileSheet(sprite_sheet, this.currentLidFrame, 16, sprite_sheet_height), this.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
	}

	public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f, bool local = false)
	{
		if (!this.playerChest.Value)
		{
			return;
		}
		if (this.playerChoiceColor.Equals(Color.Black))
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), itemData.GetSourceRect(), this.tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.89f : ((float)(y * 64 + 4) / 10000f));
			return;
		}
		ParsedItemData itemData2 = ItemRegistry.GetData(base.QualifiedItemId);
		if (itemData2 != null)
		{
			int drawIndex = base.ParentSheetIndex;
			int overlayIndex = this.currentLidFrame + 8;
			int coloredLidIndex = this.currentLidFrame;
			switch (base.QualifiedItemId)
			{
			case "(BC)130":
				drawIndex = 168;
				overlayIndex = this.currentLidFrame + 46;
				coloredLidIndex = this.currentLidFrame + 38;
				break;
			case "(BC)BigChest":
				drawIndex = 312;
				overlayIndex = this.currentLidFrame + 16;
				coloredLidIndex = this.currentLidFrame + 8;
				break;
			case "(BC)BigStoneChest":
				overlayIndex = this.currentLidFrame + 8;
				coloredLidIndex = this.currentLidFrame;
				break;
			}
			Microsoft.Xna.Framework.Rectangle drawRect = itemData2.GetSourceRect(0, drawIndex);
			Microsoft.Xna.Framework.Rectangle lidRect = itemData2.GetSourceRect(0, overlayIndex);
			Microsoft.Xna.Framework.Rectangle coloredLidRect = itemData2.GetSourceRect(0, coloredLidIndex);
			Texture2D texture = itemData2.GetTexture();
			spriteBatch.Draw(texture, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), drawRect, this.playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 4) / 10000f));
			spriteBatch.Draw(texture, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), coloredLidRect, this.playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 5) / 10000f));
			spriteBatch.Draw(texture, local ? new Vector2(x, y + 20) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 20)), new Microsoft.Xna.Framework.Rectangle(0, drawIndex / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
			spriteBatch.Draw(texture, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), lidRect, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
		}
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
	{
		if (base.ForEachItem(handler, getPath))
		{
			return ForEachItemHelper.ApplyToList(this.Items, handler, getPath);
		}
		return false;
	}
}
