using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations;

public class FarmHouse : DecoratableLocation
{
	[XmlElement("fridge")]
	public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));

	[XmlIgnore]
	public readonly NetInt synchronizedDisplayedLevel = new NetInt(-1);

	/// <summary>The tile position containing the map fridge, or <see cref="P:Microsoft.Xna.Framework.Point.Zero" /> if not found.</summary>
	public Point fridgePosition = Point.Zero;

	[XmlIgnore]
	public Point spouseRoomSpot = Point.Zero;

	private string lastSpouseRoom;

	[XmlIgnore]
	private LocalizedContentManager mapLoader;

	public List<Warp> cellarWarps;

	[XmlElement("cribStyle")]
	public readonly NetInt cribStyle = new NetInt(1)
	{
		InterpolationEnabled = false
	};

	[XmlIgnore]
	public int previousUpgradeLevel = -1;

	private int currentlyDisplayedUpgradeLevel;

	private bool displayingSpouseRoom;

	private Color nightLightingColor = new Color(180, 180, 0);

	private Color rainLightingColor = new Color(90, 90, 0);

	/// <summary>The player who owns this home.</summary>
	[XmlIgnore]
	public virtual Farmer owner => Game1.MasterPlayer;

	/// <summary>Whether the home has an assigned player, regardless of whether they've finished creating their character..</summary>
	/// <remarks>See also <see cref="P:StardewValley.Locations.FarmHouse.IsOwnerActivated" />.</remarks>
	[XmlIgnore]
	[MemberNotNullWhen(true, "owner")]
	public virtual bool HasOwner
	{
		[MemberNotNullWhen(true, "owner")]
		get
		{
			return this.owner != null;
		}
	}

	/// <summary>The unique ID of the player who owns this home, if any.</summary>
	public virtual long OwnerId => this.owner?.UniqueMultiplayerID ?? 0;

	/// <summary>Whether the home has an assigned player and they've finished creating their character.</summary>
	/// <remarks>See also <see cref="P:StardewValley.Locations.FarmHouse.HasOwner" />.</remarks>
	[MemberNotNullWhen(true, "owner")]
	public bool IsOwnerActivated
	{
		[MemberNotNullWhen(true, "owner")]
		get
		{
			return this.owner?.isActive() ?? false;
		}
	}

	/// <summary>Whether the home is owned by the current player.</summary>
	[MemberNotNullWhen(true, "owner")]
	public bool IsOwnedByCurrentPlayer
	{
		[MemberNotNullWhen(true, "owner")]
		get
		{
			return this.owner?.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID;
		}
	}

	[XmlIgnore]
	public virtual int upgradeLevel
	{
		get
		{
			return this.owner?.HouseUpgradeLevel ?? 0;
		}
		set
		{
			if (this.HasOwner)
			{
				this.owner.houseUpgradeLevel.Value = value;
			}
		}
	}

	public FarmHouse()
	{
		this.fridge.Value.Location = this;
	}

	public FarmHouse(string m, string name)
		: base(m, name)
	{
		this.fridge.Value.Location = this;
		this.ReadWallpaperAndFloorTileData();
		Farm farm = Game1.getFarm();
		this.AddStarterGiftBox(farm);
		this.AddStarterFurniture(farm);
		this.SetStarterFlooring(farm);
		this.SetStarterWallpaper(farm);
	}

	/// <summary>Place the starter gift box when the farmhouse is first created.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	private void AddStarterGiftBox(Farm farm)
	{
		Chest box = new Chest(null, Vector2.Zero, giftbox: true, 0, giftboxIsStarterGift: true);
		string[] fields = farm.GetMapPropertySplitBySpaces("FarmHouseStarterGift");
		for (int i = 0; i < fields.Length; i += 2)
		{
			if (!ArgUtility.TryGet(fields, i, out var giftId, out var error, allowBlank: false, "string giftId") || !ArgUtility.TryGetOptionalInt(fields, i + 1, out var count, out error, 0, "int count"))
			{
				farm.LogMapPropertyError("FarmHouseStarterGift", fields, error);
			}
			else
			{
				box.Items.Add(ItemRegistry.Create(giftId, count));
			}
		}
		if (!box.Items.Any())
		{
			Item parsnipSeeds = ItemRegistry.Create("(O)472", 15);
			box.Items.Add(parsnipSeeds);
		}
		if (!farm.TryGetMapPropertyAs("FarmHouseStarterSeedsPosition", out Vector2 tile, required: false))
		{
			switch (Game1.whichFarm)
			{
			case 1:
			case 2:
			case 4:
				tile = new Vector2(4f, 7f);
				break;
			case 3:
				tile = new Vector2(2f, 9f);
				break;
			case 6:
				tile = new Vector2(8f, 6f);
				break;
			default:
				tile = new Vector2(3f, 7f);
				break;
			}
		}
		base.objects.Add(tile, box);
	}

	/// <summary>Place the starter furniture when the farmhouse is first created.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	private void AddStarterFurniture(Farm farm)
	{
		base.furniture.Add(new BedFurniture(BedFurniture.DEFAULT_BED_INDEX, new Vector2(9f, 8f)));
		string[] fields = farm.GetMapPropertySplitBySpaces("FarmHouseFurniture");
		if (fields.Any())
		{
			for (int i = 0; i < fields.Length; i += 4)
			{
				if (!ArgUtility.TryGetInt(fields, i, out var index, out var error, "int index") || !ArgUtility.TryGetVector2(fields, i + 1, out var tile, out error, integerOnly: false, "Vector2 tile") || !ArgUtility.TryGetInt(fields, i + 3, out var rotations, out error, "int rotations"))
				{
					farm.LogMapPropertyError("FarmHouseFurniture", fields, error);
					continue;
				}
				Furniture newFurniture = ItemRegistry.Create<Furniture>("(F)" + index);
				newFurniture.InitializeAtTile(tile);
				newFurniture.isOn.Value = true;
				for (int rotation = 0; rotation < rotations; rotation++)
				{
					newFurniture.rotate();
				}
				Furniture targetFurniture = base.GetFurnitureAt(tile);
				if (targetFurniture != null)
				{
					targetFurniture.heldObject.Value = newFurniture;
				}
				else
				{
					base.furniture.Add(newFurniture);
				}
			}
			return;
		}
		switch (Game1.whichFarm)
		{
		case 0:
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1120").SetPlacement(5, 4).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1364")));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1376").SetPlacement(1, 10));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)0").SetPlacement(4, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1466").SetPlacement(1, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(3, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(6, 8));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1602").SetPlacement(5, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(this.getFireplacePoint()));
			break;
		case 1:
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1122").SetPlacement(1, 6).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1367")));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)3").SetPlacement(1, 5));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(5, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(1, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(3, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1676").SetPlacement(5, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1737").SetPlacement(6, 8));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1742").SetPlacement(5, 5));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1675").SetPlacement(10, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(this.getFireplacePoint()));
			base.objects.Add(new Vector2(4f, 4f), ItemRegistry.Create<Object>("(BC)FishSmoker"));
			break;
		case 2:
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1134").SetPlacement(1, 7).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1748")));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)3").SetPlacement(1, 6));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(6, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(1, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1682").SetPlacement(3, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1777").SetPlacement(6, 5));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1745").SetPlacement(6, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(5, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(10, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(this.getFireplacePoint()));
			break;
		case 3:
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1218").SetPlacement(1, 6).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1368")));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1755").SetPlacement(1, 5));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1755").SetPlacement(3, 6, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(5, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1751").SetPlacement(5, 10));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1749").SetPlacement(3, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1753").SetPlacement(5, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1742").SetPlacement(5, 5));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1794").SetPlacement(this.getFireplacePoint()));
			break;
		case 4:
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(1, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1628").SetPlacement(1, 5));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1393").SetPlacement(3, 4).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1369")));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1678").SetPlacement(10, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1812").SetPlacement(3, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1630").SetPlacement(1, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1811").SetPlacement(6, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1389").SetPlacement(10, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(1, 10));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1794").SetPlacement(this.getFireplacePoint()));
			break;
		case 5:
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1466").SetPlacement(1, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(3, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(6, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1601").SetPlacement(10, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)202").SetPlacement(3, 4, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(4, 4, 1).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1379")));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)202").SetPlacement(6, 4, 3));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1378").SetPlacement(10, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1377").SetPlacement(1, 9));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1445").SetPlacement(1, 10));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(2, 9));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(this.getFireplacePoint()));
			break;
		case 6:
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(4, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(7, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(3, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1283").SetPlacement(1, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(8, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)202").SetPlacement(7, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(10, 4));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)6").SetPlacement(2, 6, 1));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)6").SetPlacement(5, 7, 3));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(3, 6).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1362")));
			base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1228").SetPlacement(2, 9));
			break;
		}
	}

	/// <summary>Gets the initial flooring type for a farmhouse being created, if any.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	public static string GetStarterFlooring(Farm farm)
	{
		string id = farm?.getMapProperty("FarmHouseFlooring");
		if (id != null)
		{
			return id;
		}
		return Game1.whichFarm switch
		{
			1 => "1", 
			2 => "34", 
			3 => "18", 
			4 => "4", 
			5 => "5", 
			6 => "35", 
			_ => null, 
		};
	}

	/// <summary>Gets the initial wallpaper type for a farmhouse being created, if any.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	public static string GetStarterWallpaper(Farm farm)
	{
		string id = farm?.getMapProperty("FarmHouseWallpaper");
		if (id != null)
		{
			return id;
		}
		return Game1.whichFarm switch
		{
			1 => "11", 
			2 => "92", 
			3 => "12", 
			4 => "95", 
			5 => "65", 
			6 => "106", 
			_ => null, 
		};
	}

	/// <summary>Set the initial flooring when the farmhouse is first created, if any.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	/// <param name="styleToOverride">Unused.</param>
	private void SetStarterFlooring(Farm farm, string styleToOverride = null)
	{
		string id = FarmHouse.GetStarterFlooring(farm);
		if (id != null)
		{
			base.SetFloor(id, null);
		}
	}

	/// <summary>Set the initial wallpaper when the farmhouse is first created, if any.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	/// <param name="styleToOverride">Unused.</param>
	private void SetStarterWallpaper(Farm farm, string styleToOverride = null)
	{
		string id = FarmHouse.GetStarterWallpaper(farm);
		if (id != null)
		{
			base.SetWallpaper(id, null);
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.fridge, "fridge").AddField(this.cribStyle, "cribStyle").AddField(this.synchronizedDisplayedLevel, "synchronizedDisplayedLevel");
		this.cribStyle.fieldChangeVisibleEvent += delegate
		{
			if (base.map != null)
			{
				if (base._appliedMapOverrides != null && base._appliedMapOverrides.Contains("crib"))
				{
					base._appliedMapOverrides.Remove("crib");
				}
				this.UpdateChildRoom();
				this.ReadWallpaperAndFloorTileData();
				this.setWallpapers();
				this.setFloors();
			}
		};
		this.fridge.fieldChangeEvent += delegate(NetRef<Chest> field, Chest oldValue, Chest newValue)
		{
			newValue.Location = this;
		};
	}

	public List<Child> getChildren()
	{
		return base.characters.OfType<Child>().ToList();
	}

	public int getChildrenCount()
	{
		int count = 0;
		foreach (NPC character in base.characters)
		{
			if (character is Child)
			{
				count++;
			}
		}
		return count;
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
	{
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding);
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		foreach (NPC c in base.characters)
		{
			if (c.isMarried())
			{
				if (c.getSpouse() == Game1.player)
				{
					c.checkForMarriageDialogue(timeOfDay, this);
				}
				if (Game1.IsMasterGame && Game1.timeOfDay >= 2200 && Game1.IsMasterGame && c.TilePoint != this.getSpouseBedSpot(c.Name) && (timeOfDay == 2200 || (c.controller == null && timeOfDay % 100 % 30 == 0)))
				{
					Point bed_spot = this.getSpouseBedSpot(c.Name);
					c.controller = null;
					PathFindController.endBehavior end_behavior = null;
					bool found_bed = this.GetSpouseBed() != null;
					if (found_bed)
					{
						end_behavior = spouseSleepEndFunction;
					}
					c.controller = new PathFindController(c, this, bed_spot, 0, end_behavior);
					if (c.controller.pathToEndPoint == null || !base.isTileOnMap(c.controller.pathToEndPoint.Last()))
					{
						c.controller = null;
					}
					else if (found_bed)
					{
						foreach (Furniture item in base.furniture)
						{
							if (item is BedFurniture bed && bed.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(bed_spot.X * 64, bed_spot.Y * 64, 64, 64)))
							{
								bed.ReserveForNPC();
								break;
							}
						}
					}
				}
			}
			if (c is Child child)
			{
				child.tenMinuteUpdate();
			}
		}
	}

	public static void spouseSleepEndFunction(Character c, GameLocation location)
	{
		if (!(c is NPC npc))
		{
			return;
		}
		if (DataLoader.AnimationDescriptions(Game1.content).ContainsKey(npc.name.Value.ToLower() + "_sleep"))
		{
			npc.playSleepingAnimation();
		}
		Microsoft.Xna.Framework.Rectangle npcBounds = npc.GetBoundingBox();
		foreach (Furniture item in location.furniture)
		{
			if (item is BedFurniture bed && bed.GetBoundingBox().Intersects(npcBounds))
			{
				bed.ReserveForNPC();
				break;
			}
		}
		if (Game1.random.NextDouble() < 0.1)
		{
			if (Game1.random.NextDouble() < 0.8)
			{
				npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Spouse_Goodnight0", npc.getTermOfSpousalEndearment(Game1.random.NextDouble() < 0.1)));
			}
			else
			{
				npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Spouse_Goodnight1"));
			}
		}
	}

	public virtual Point getFrontDoorSpot()
	{
		foreach (Warp warp in base.warps)
		{
			if (warp.TargetName == "Farm")
			{
				if (this is Cabin)
				{
					return new Point(warp.TargetX, warp.TargetY);
				}
				if (warp.TargetX == 64 && warp.TargetY == 15)
				{
					return Game1.getFarm().GetMainFarmHouseEntry();
				}
				return new Point(warp.TargetX, warp.TargetY);
			}
		}
		return Game1.getFarm().GetMainFarmHouseEntry();
	}

	public virtual Point getPorchStandingSpot()
	{
		Point p = Game1.getFarm().GetMainFarmHouseEntry();
		p.X += 2;
		return p;
	}

	public Point getKitchenStandingSpot()
	{
		if (base.TryGetMapPropertyAs("KitchenStandingLocation", out Point position, required: false))
		{
			return position;
		}
		switch (this.upgradeLevel)
		{
		case 1:
			return new Point(4, 5);
		case 2:
		case 3:
			return new Point(22, 24);
		default:
			return new Point(-1000, -1000);
		}
	}

	public virtual BedFurniture GetSpouseBed()
	{
		if (this.HasOwner)
		{
			if (this.owner.getSpouse()?.Name == "Krobus")
			{
				return null;
			}
			if (this.owner.hasCurrentOrPendingRoommate() && this.GetBed(BedFurniture.BedType.Single) != null)
			{
				return this.GetBed(BedFurniture.BedType.Single);
			}
		}
		return this.GetBed(BedFurniture.BedType.Double);
	}

	public Point getSpouseBedSpot(string spouseName)
	{
		if (spouseName == "Krobus")
		{
			NPC characterFromName = Game1.getCharacterFromName(base.name.Value);
			if (characterFromName != null && characterFromName.isRoommate())
			{
				goto IL_0035;
			}
		}
		if (this.GetSpouseBed() != null)
		{
			BedFurniture spouseBed = this.GetSpouseBed();
			Point bed_spot = this.GetSpouseBed().GetBedSpot();
			if (spouseBed.bedType == BedFurniture.BedType.Double)
			{
				bed_spot.X++;
			}
			return bed_spot;
		}
		goto IL_0035;
		IL_0035:
		return this.GetSpouseRoomSpot();
	}

	public Point GetSpouseRoomSpot()
	{
		if (this.upgradeLevel == 0)
		{
			return new Point(-1000, -1000);
		}
		return this.spouseRoomSpot;
	}

	public BedFurniture GetBed(BedFurniture.BedType bed_type = BedFurniture.BedType.Any, int index = 0)
	{
		foreach (Furniture item in base.furniture)
		{
			if (item is BedFurniture bed && (bed_type == BedFurniture.BedType.Any || bed.bedType == bed_type))
			{
				if (index == 0)
				{
					return bed;
				}
				index--;
			}
		}
		return null;
	}

	public Point GetPlayerBedSpot()
	{
		return this.GetPlayerBed()?.GetBedSpot() ?? this.getEntryLocation();
	}

	public BedFurniture GetPlayerBed()
	{
		if (this.upgradeLevel == 0)
		{
			return this.GetBed(BedFurniture.BedType.Single);
		}
		return this.GetBed(BedFurniture.BedType.Double);
	}

	public Point getBedSpot(BedFurniture.BedType bed_type = BedFurniture.BedType.Any)
	{
		return this.GetBed(bed_type)?.GetBedSpot() ?? new Point(-1000, -1000);
	}

	public Point getEntryLocation()
	{
		if (base.TryGetMapPropertyAs("EntryLocation", out Point position, required: false))
		{
			return position;
		}
		switch (this.upgradeLevel)
		{
		case 0:
			return new Point(3, 11);
		case 1:
			return new Point(9, 11);
		case 2:
		case 3:
			return new Point(27, 30);
		default:
			return new Point(-1000, -1000);
		}
	}

	public BedFurniture GetChildBed(int index)
	{
		return this.GetBed(BedFurniture.BedType.Child, index);
	}

	public Point GetChildBedSpot(int index)
	{
		return this.GetChildBed(index)?.GetBedSpot() ?? Point.Zero;
	}

	public override bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
	{
		if (base.isTileOnMap(v) && base.getTileIndexAt((int)v.X, (int)v.Y, "Back", "indoor") == 0)
		{
			return false;
		}
		return base.isTilePlaceable(v, itemIsPassable);
	}

	public Point getRandomOpenPointInHouse(Random r, int buffer = 0, int tries = 30)
	{
		for (int numTries = 0; numTries < tries; numTries++)
		{
			Point point = new Point(r.Next(base.map.Layers[0].LayerWidth), r.Next(base.map.Layers[0].LayerHeight));
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(point.X - buffer, point.Y - buffer, 1 + buffer * 2, 1 + buffer * 2);
			bool obstacleFound = false;
			foreach (Point point2 in rect.GetPoints())
			{
				int x = point2.X;
				int y = point2.Y;
				obstacleFound = !base.hasTileAt(x, y, "Back") || !this.CanItemBePlacedHere(new Vector2(x, y)) || this.isTileOnWall(x, y);
				if (base.getTileIndexAt(x, y, "Back", "indoor") == 0)
				{
					obstacleFound = true;
				}
				if (obstacleFound)
				{
					break;
				}
			}
			if (!obstacleFound)
			{
				return point;
			}
		}
		return Point.Zero;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet") == 173)
		{
			this.fridge.Value.fridge.Value = true;
			this.fridge.Value.checkForAction(who);
			return true;
		}
		if (base.getTileIndexAt(tileLocation, "Buildings", "indoor") == 2173)
		{
			if (Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse == "Emily" && base.getTemporarySpriteByID(5858585) is EmilysParrot parrot)
			{
				parrot.doAction();
			}
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
		if (!this.HasOwner || !Game1.IsMasterGame)
		{
			return;
		}
		foreach (NPC spouse in base.characters)
		{
			if (spouse.getSpouse()?.UniqueMultiplayerID != this.OwnerId || Game1.timeOfDay >= 1500 || !(Game1.random.NextDouble() < 0.0006) || spouse.controller != null || spouse.Schedule != null || !(spouse.TilePoint != this.getSpouseBedSpot(Game1.player.spouse)) || base.furniture.Count <= 0)
			{
				continue;
			}
			Furniture f = base.furniture[Game1.random.Next(base.furniture.Count)];
			Microsoft.Xna.Framework.Rectangle b = f.boundingBox.Value;
			Vector2 possibleLocation = new Vector2(b.X / 64, b.Y / 64);
			if (f.furniture_type.Value == 15 || f.furniture_type.Value == 12)
			{
				continue;
			}
			int tries = 0;
			int facingDirection = -3;
			for (; tries < 3; tries++)
			{
				int xMove = Game1.random.Next(-1, 2);
				int yMove = Game1.random.Next(-1, 2);
				possibleLocation.X += xMove;
				if (xMove == 0)
				{
					possibleLocation.Y += yMove;
				}
				switch (xMove)
				{
				case -1:
					facingDirection = 1;
					break;
				case 1:
					facingDirection = 3;
					break;
				default:
					switch (yMove)
					{
					case -1:
						facingDirection = 2;
						break;
					case 1:
						facingDirection = 0;
						break;
					}
					break;
				}
				if (this.CanItemBePlacedHere(possibleLocation))
				{
					break;
				}
			}
			if (tries < 3)
			{
				spouse.controller = new PathFindController(spouse, this, new Point((int)possibleLocation.X, (int)possibleLocation.Y), facingDirection, clearMarriageDialogues: false);
			}
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (base.wasUpdated)
		{
			return;
		}
		base.UpdateWhenCurrentLocation(time);
		this.fridge.Value.updateWhenCurrentLocation(time);
		if (!Game1.player.isMarriedOrRoommates() || Game1.player.spouse == null)
		{
			return;
		}
		NPC spouse = base.getCharacterFromName(Game1.player.spouse);
		if (spouse == null || spouse.isEmoting)
		{
			return;
		}
		Vector2 spousePos = spouse.Tile;
		Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
		foreach (Vector2 offset in adjacentTilesOffsets)
		{
			Vector2 v = spousePos + offset;
			if (base.isCharacterAtTile(v) is Monster monster)
			{
				Microsoft.Xna.Framework.Rectangle monsterBounds = monster.GetBoundingBox();
				Point centerPixel = monsterBounds.Center;
				spouse.faceGeneralDirection(v * new Vector2(64f, 64f));
				Game1.showSwordswipeAnimation(spouse.FacingDirection, spouse.Position, 60f, flip: false);
				base.localSound("swordswipe");
				spouse.shake(500);
				spouse.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:FarmHouse_SpouseAttacked" + (Game1.random.Next(12) + 1)));
				monster.takeDamage(50, (int)Utility.getAwayFromPositionTrajectory(monsterBounds, spouse.Position).X, (int)Utility.getAwayFromPositionTrajectory(monsterBounds, spouse.Position).Y, isBomb: false, 1.0, Game1.player);
				if (monster.Health <= 0)
				{
					base.debris.Add(new Debris(monster.Sprite.textureName.Value, Game1.random.Next(6, 16), Utility.PointToVector2(centerPixel)));
					this.monsterDrop(monster, centerPixel.X, centerPixel.Y, this.owner);
					base.characters.Remove(monster);
					Game1.stats.MonstersKilled++;
					Game1.player.changeFriendship(-10, spouse);
				}
				else
				{
					monster.shedChunks(4);
				}
				spouse.CurrentDialogue.Clear();
				spouse.CurrentDialogue.Push(spouse.TryGetDialogue("Spouse_MonstersInHouse") ?? new Dialogue(spouse, "Data\\ExtraDialogue:Spouse_MonstersInHouse"));
			}
		}
	}

	public Point getFireplacePoint()
	{
		switch (this.upgradeLevel)
		{
		case 0:
			return new Point(8, 4);
		case 1:
			return new Point(26, 4);
		case 2:
		case 3:
			return new Point(17, 23);
		default:
			return new Point(-50, -50);
		}
	}

	/// <summary>Get whether the player who owns this home is married to or roommates with an NPC.</summary>
	public bool HasNpcSpouseOrRoommate()
	{
		if (this.owner?.spouse != null)
		{
			return this.owner.isMarriedOrRoommates();
		}
		return false;
	}

	/// <summary>Get whether the player who owns this home is married to or roommates with the given NPC.</summary>
	/// <param name="spouseName">The NPC name.</param>
	public bool HasNpcSpouseOrRoommate(string spouseName)
	{
		if (spouseName != null && this.owner?.spouse == spouseName)
		{
			return this.owner.isMarriedOrRoommates();
		}
		return false;
	}

	public virtual void showSpouseRoom()
	{
		bool showSpouse = this.HasNpcSpouseOrRoommate();
		bool num = this.displayingSpouseRoom;
		this.displayingSpouseRoom = showSpouse;
		this.updateMap();
		if (num && !this.displayingSpouseRoom)
		{
			Point corner = this.GetSpouseRoomCorner();
			Microsoft.Xna.Framework.Rectangle sourceArea = CharacterSpouseRoomData.DefaultMapSourceRect;
			if (NPC.TryGetData(this.owner.spouse, out var spouseData))
			{
				sourceArea = spouseData.SpouseRoom?.MapSourceRect ?? sourceArea;
			}
			Microsoft.Xna.Framework.Rectangle spouseRoomBounds = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
			spouseRoomBounds.X--;
			List<Item> collected_items = new List<Item>();
			Microsoft.Xna.Framework.Rectangle room_bounds = new Microsoft.Xna.Framework.Rectangle(spouseRoomBounds.X * 64, spouseRoomBounds.Y * 64, spouseRoomBounds.Width * 64, spouseRoomBounds.Height * 64);
			foreach (Furniture placed_furniture in new List<Furniture>(base.furniture))
			{
				if (placed_furniture.GetBoundingBox().Intersects(room_bounds))
				{
					if (placed_furniture is StorageFurniture storage_furniture)
					{
						collected_items.AddRange(storage_furniture.heldItems);
						storage_furniture.heldItems.Clear();
					}
					if (placed_furniture.heldObject.Value != null)
					{
						collected_items.Add(placed_furniture.heldObject.Value);
						placed_furniture.heldObject.Value = null;
					}
					collected_items.Add(placed_furniture);
					base.furniture.Remove(placed_furniture);
				}
			}
			for (int x = spouseRoomBounds.X; x <= spouseRoomBounds.Right; x++)
			{
				for (int y = spouseRoomBounds.Y; y <= spouseRoomBounds.Bottom; y++)
				{
					Object tile_object = base.getObjectAtTile(x, y);
					if (tile_object == null || tile_object is Furniture)
					{
						continue;
					}
					tile_object.performRemoveAction();
					if (!(tile_object is Fence fence))
					{
						if (!(tile_object is IndoorPot garden_pot))
						{
							if (tile_object is Chest chest)
							{
								collected_items.AddRange(chest.Items);
								chest.Items.Clear();
							}
						}
						else if (garden_pot.hoeDirt.Value?.crop != null)
						{
							garden_pot.hoeDirt.Value.destroyCrop(showAnimation: false);
						}
					}
					else
					{
						tile_object = new Object(fence.ItemId, 1);
					}
					tile_object.heldObject.Value = null;
					tile_object.minutesUntilReady.Value = -1;
					tile_object.readyForHarvest.Value = false;
					collected_items.Add(tile_object);
					base.objects.Remove(new Vector2(x, y));
				}
			}
			if (this.upgradeLevel >= 2)
			{
				Utility.createOverflowChest(this, new Vector2(39f, 32f), collected_items);
			}
			else
			{
				Utility.createOverflowChest(this, new Vector2(21f, 10f), collected_items);
			}
		}
		base.loadObjects();
		if (this.upgradeLevel == 3)
		{
			this.AddCellarTiles();
			this.createCellarWarps();
			Game1.player.craftingRecipes.TryAdd("Cask", 0);
		}
		if (showSpouse)
		{
			this.loadSpouseRoom();
		}
		this.lastSpouseRoom = this.owner?.spouse;
	}

	public virtual void AddCellarTiles()
	{
		if (base._appliedMapOverrides.Contains("cellar"))
		{
			base._appliedMapOverrides.Remove("cellar");
		}
		base.ApplyMapOverride("FarmHouse_Cellar", "cellar");
	}

	/// <summary>Get the cellar location linked to this cabin, or <c>null</c> if there is none.</summary>
	public Cellar GetCellar()
	{
		string cellarName = this.GetCellarName();
		if (cellarName == null)
		{
			return null;
		}
		return Game1.RequireLocation<Cellar>(cellarName);
	}

	/// <summary>Get the name of the cellar location linked to this cabin, or <c>null</c> if there is none.</summary>
	public string GetCellarName()
	{
		int cellar_number = -1;
		if (this.HasOwner)
		{
			foreach (int i in Game1.player.team.cellarAssignments.Keys)
			{
				if (Game1.player.team.cellarAssignments[i] == this.OwnerId)
				{
					cellar_number = i;
				}
			}
		}
		switch (cellar_number)
		{
		case 0:
		case 1:
			return "Cellar";
		case -1:
			return null;
		default:
			return "Cellar" + cellar_number;
		}
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (this.HasOwner)
		{
			if (Game1.timeOfDay >= 2200 && this.owner.spouse != null && base.getCharacterFromName(this.owner.spouse) != null && !this.owner.isEngaged())
			{
				Game1.player.team.requestSpouseSleepEvent.Fire(this.owner.UniqueMultiplayerID);
			}
			if (Game1.timeOfDay >= 2000 && this.IsOwnedByCurrentPlayer && Game1.getFarm().farmers.Count <= 1)
			{
				Game1.player.team.requestPetWarpHomeEvent.Fire(this.owner.UniqueMultiplayerID);
			}
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Farm farm = Game1.getFarm();
		for (int i = base.characters.Count - 1; i >= 0; i--)
		{
			if (base.characters[i] is Pet { TilePoint: var tile } pet)
			{
				Microsoft.Xna.Framework.Rectangle bounds = pet.GetBoundingBox();
				if (!base.isTileOnMap(tile.X, tile.Y) || base.hasTileAt(bounds.Left / 64, tile.Y, "Buildings") || base.hasTileAt(bounds.Right / 64, tile.Y, "Buildings"))
				{
					pet.WarpToPetBowl();
					break;
				}
			}
		}
		for (int i2 = base.characters.Count - 1; i2 >= 0; i2--)
		{
			for (int j = i2 - 1; j >= 0; j--)
			{
				if (i2 < base.characters.Count && j < base.characters.Count && (base.characters[j].Equals(base.characters[i2]) || (base.characters[j].Name.Equals(base.characters[i2].Name) && base.characters[j].IsVillager && base.characters[i2].IsVillager)) && j != i2)
				{
					base.characters.RemoveAt(j);
				}
			}
			for (int j2 = farm.characters.Count - 1; j2 >= 0; j2--)
			{
				if (i2 < base.characters.Count && j2 < base.characters.Count && farm.characters[j2].Equals(base.characters[i2]))
				{
					farm.characters.RemoveAt(j2);
				}
			}
		}
	}

	public void UpdateForRenovation()
	{
		this.updateFarmLayout();
		this.setWallpapers();
		this.setFloors();
	}

	public void updateFarmLayout()
	{
		if (this.currentlyDisplayedUpgradeLevel != this.upgradeLevel)
		{
			this.setMapForUpgradeLevel(this.upgradeLevel);
		}
		this._ApplyRenovations();
		if (this.displayingSpouseRoom != this.HasNpcSpouseOrRoommate() || this.lastSpouseRoom != this.owner?.spouse)
		{
			this.showSpouseRoom();
		}
		this.UpdateChildRoom();
		this.ReadWallpaperAndFloorTileData();
	}

	protected virtual void _ApplyRenovations()
	{
		bool hasOwner = this.HasOwner;
		if (this.upgradeLevel >= 2)
		{
			if (base._appliedMapOverrides.Contains("bedroom_open"))
			{
				base._appliedMapOverrides.Remove("bedroom_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_bedroom_open"))
			{
				base.ApplyMapOverride("FarmHouse_Bedroom_Open", "bedroom_open");
			}
			else
			{
				base.ApplyMapOverride("FarmHouse_Bedroom_Normal", "bedroom_open");
			}
			if (base._appliedMapOverrides.Contains("southernroom_open"))
			{
				base._appliedMapOverrides.Remove("southernroom_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_southern_open"))
			{
				base.ApplyMapOverride("FarmHouse_SouthernRoom_Add", "southernroom_open");
			}
			else
			{
				base.ApplyMapOverride("FarmHouse_SouthernRoom_Remove", "southernroom_open");
			}
			if (base._appliedMapOverrides.Contains("cornerroom_open"))
			{
				base._appliedMapOverrides.Remove("cornerroom_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_corner_open"))
			{
				base.ApplyMapOverride("FarmHouse_CornerRoom_Add", "cornerroom_open");
				if (this.displayingSpouseRoom)
				{
					base.setMapTile(49, 19, 229, "Front", "untitled tile sheet");
				}
			}
			else
			{
				base.ApplyMapOverride("FarmHouse_CornerRoom_Remove", "cornerroom_open");
				if (this.displayingSpouseRoom)
				{
					base.setMapTile(49, 19, 87, "Front", "untitled tile sheet");
				}
			}
			if (base._appliedMapOverrides.Contains("diningroom_open"))
			{
				base._appliedMapOverrides.Remove("diningroom_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_dining_open"))
			{
				base.ApplyMapOverride("FarmHouse_DiningRoom_Add", "diningroom_open");
			}
			else
			{
				base.ApplyMapOverride("FarmHouse_DiningRoom_Remove", "diningroom_open");
			}
			if (base._appliedMapOverrides.Contains("cubby_open"))
			{
				base._appliedMapOverrides.Remove("cubby_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_cubby_open"))
			{
				base.ApplyMapOverride("FarmHouse_Cubby_Add", "cubby_open");
			}
			else
			{
				base.ApplyMapOverride("FarmHouse_Cubby_Remove", "cubby_open");
			}
			if (base._appliedMapOverrides.Contains("farupperroom_open"))
			{
				base._appliedMapOverrides.Remove("farupperroom_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_farupperroom_open"))
			{
				base.ApplyMapOverride("FarmHouse_FarUpperRoom_Add", "farupperroom_open");
			}
			else
			{
				base.ApplyMapOverride("FarmHouse_FarUpperRoom_Remove", "farupperroom_open");
			}
			if (base._appliedMapOverrides.Contains("extendedcorner_open"))
			{
				base._appliedMapOverrides.Remove("extendedcorner_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_extendedcorner_open"))
			{
				base.ApplyMapOverride("FarmHouse_ExtendedCornerRoom_Add", "extendedcorner_open");
			}
			else if (hasOwner && this.owner.mailReceived.Contains("renovation_corner_open"))
			{
				base.ApplyMapOverride("FarmHouse_ExtendedCornerRoom_Remove", "extendedcorner_open");
			}
			if (base._appliedMapOverrides.Contains("diningroomwall_open"))
			{
				base._appliedMapOverrides.Remove("diningroomwall_open");
			}
			if (hasOwner && this.owner.mailReceived.Contains("renovation_diningroomwall_open"))
			{
				base.ApplyMapOverride("FarmHouse_DiningRoomWall_Add", "diningroomwall_open");
			}
			else if (hasOwner && this.owner.mailReceived.Contains("renovation_dining_open"))
			{
				base.ApplyMapOverride("FarmHouse_DiningRoomWall_Remove", "diningroomwall_open");
			}
		}
		if (!base.TryGetMapProperty("AdditionalRenovations", out var propertyValue))
		{
			return;
		}
		string[] array = propertyValue.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string[] data_split = ArgUtility.SplitBySpace(array[i]);
			if (data_split.Length < 4)
			{
				continue;
			}
			string map_patch_id = data_split[0];
			string required_mail = data_split[1];
			string add_map_override = data_split[2];
			string remove_map_override = data_split[3];
			Microsoft.Xna.Framework.Rectangle? destination_rect = null;
			if (data_split.Length >= 8)
			{
				try
				{
					destination_rect = new Microsoft.Xna.Framework.Rectangle
					{
						X = int.Parse(data_split[4]),
						Y = int.Parse(data_split[5]),
						Width = int.Parse(data_split[6]),
						Height = int.Parse(data_split[7])
					};
				}
				catch (Exception)
				{
					destination_rect = null;
				}
			}
			if (base._appliedMapOverrides.Contains(map_patch_id))
			{
				base._appliedMapOverrides.Remove(map_patch_id);
			}
			if (hasOwner && this.owner.mailReceived.Contains(required_mail))
			{
				base.ApplyMapOverride(add_map_override, map_patch_id, null, destination_rect);
			}
			else
			{
				base.ApplyMapOverride(remove_map_override, map_patch_id, null, destination_rect);
			}
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		this.updateFarmLayout();
		this.setWallpapers();
		this.setFloors();
		if (this.HasNpcSpouseOrRoommate("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
		{
			Point frog_spot = this.GetSpouseRoomCorner();
			frog_spot.X++;
			frog_spot.Y += 6;
			Vector2 spot = Utility.PointToVector2(frog_spot);
			base.removeTile((int)spot.X, (int)spot.Y - 1, "Front");
			base.removeTile((int)spot.X + 1, (int)spot.Y - 1, "Front");
			base.removeTile((int)spot.X + 2, (int)spot.Y - 1, "Front");
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (this.HasNpcSpouseOrRoommate("Emily") && Game1.player.eventsSeen.Contains("463391"))
		{
			Vector2 parrotSpot = new Vector2(2064f, 160f);
			int num = this.upgradeLevel;
			if ((uint)(num - 2) <= 1u)
			{
				parrotSpot = new Vector2(3408f, 1376f);
			}
			base.temporarySprites.Add(new EmilysParrot(parrotSpot));
		}
		if (Game1.player.currentLocation == null || (!Game1.player.currentLocation.Equals(this) && !Game1.player.currentLocation.name.Value.StartsWith("Cellar")))
		{
			Game1.player.Position = Utility.PointToVector2(this.getEntryLocation()) * 64f;
			Game1.xLocationAfterWarp = Game1.player.TilePoint.X;
			Game1.yLocationAfterWarp = Game1.player.TilePoint.Y;
			Game1.player.currentLocation = this;
		}
		foreach (NPC n in base.characters)
		{
			if (n is Child child)
			{
				child.resetForPlayerEntry(this);
			}
			if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && !(n is Pet))
			{
				n.controller = null;
				n.Halt();
			}
		}
		if (this.IsOwnedByCurrentPlayer && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue && Game1.player.team.IsMarried(Game1.player.UniqueMultiplayerID) && !Game1.player.mailReceived.Contains("CF_Spouse"))
		{
			Vector2 chestPosition = Utility.PointToVector2(this.getEntryLocation()) + new Vector2(0f, -1f);
			Chest chest = new Chest(new List<Item> { ItemRegistry.Create("(O)434") }, chestPosition, giftbox: true, 1);
			base.overlayObjects[chestPosition] = chest;
		}
		if (this.IsOwnedByCurrentPlayer && !Game1.player.activeDialogueEvents.ContainsKey("pennyRedecorating"))
		{
			int whichQuilt = -1;
			if (Game1.player.mailReceived.Contains("pennyQuilt0"))
			{
				whichQuilt = 0;
			}
			else if (Game1.player.mailReceived.Contains("pennyQuilt1"))
			{
				whichQuilt = 1;
			}
			else if (Game1.player.mailReceived.Contains("pennyQuilt2"))
			{
				whichQuilt = 2;
			}
			if (whichQuilt != -1 && !Game1.player.mailReceived.Contains("pennyRefurbished"))
			{
				List<Object> objectsPickedUp = new List<Object>();
				foreach (Furniture item in base.furniture)
				{
					if (item is BedFurniture { bedType: BedFurniture.BedType.Double } bed_furniture)
					{
						string bedId = null;
						if (this.owner.mailReceived.Contains("pennyQuilt0"))
						{
							bedId = "2058";
						}
						if (this.owner.mailReceived.Contains("pennyQuilt1"))
						{
							bedId = "2064";
						}
						if (this.owner.mailReceived.Contains("pennyQuilt2"))
						{
							bedId = "2070";
						}
						if (bedId != null)
						{
							Vector2 tile_location = bed_furniture.TileLocation;
							bed_furniture.performRemoveAction();
							objectsPickedUp.Add(bed_furniture);
							Guid guid = base.furniture.GuidOf(bed_furniture);
							base.furniture.Remove(guid);
							base.furniture.Add(new BedFurniture(bedId, new Vector2(tile_location.X, tile_location.Y)));
						}
						break;
					}
				}
				Game1.player.mailReceived.Add("pennyRefurbished");
				Microsoft.Xna.Framework.Rectangle roomToRedecorate = ((this.upgradeLevel >= 2) ? new Microsoft.Xna.Framework.Rectangle(38, 20, 11, 13) : new Microsoft.Xna.Framework.Rectangle(20, 1, 8, 10));
				for (int x = roomToRedecorate.X; x <= roomToRedecorate.Right; x++)
				{
					for (int y = roomToRedecorate.Y; y <= roomToRedecorate.Bottom; y++)
					{
						if (base.getObjectAtTile(x, y) == null)
						{
							continue;
						}
						Object o = base.getObjectAtTile(x, y);
						if (o != null && !(o is Chest) && !(o is StorageFurniture) && !(o is IndoorPot) && !(o is BedFurniture))
						{
							if (o.heldObject.Value != null && ((o as Furniture)?.IsTable() ?? false))
							{
								Object held_object = o.heldObject.Value;
								o.heldObject.Value = null;
								objectsPickedUp.Add(held_object);
							}
							o.performRemoveAction();
							if (o is Fence fence)
							{
								o = new Object(fence.ItemId, 1);
							}
							objectsPickedUp.Add(o);
							base.objects.Remove(new Vector2(x, y));
							if (o is Furniture curFurniture)
							{
								base.furniture.Remove(curFurniture);
							}
						}
					}
				}
				this.decoratePennyRoom(whichQuilt, objectsPickedUp);
			}
		}
		if (!this.HasNpcSpouseOrRoommate("Sebastian") || !Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
		{
			return;
		}
		Point frog_spot = this.GetSpouseRoomCorner();
		frog_spot.X++;
		frog_spot.Y += 6;
		Vector2 spot = Utility.PointToVector2(frog_spot);
		base.temporarySprites.Add(new TemporaryAnimatedSprite
		{
			texture = Game1.mouseCursors,
			sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
			animationLength = 1,
			sourceRectStartingPos = new Vector2(641f, 1534f),
			interval = 5000f,
			totalNumberOfLoops = 9999,
			position = spot * 64f + new Vector2(0f, -5f) * 4f,
			scale = 4f,
			layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
		});
		if (Game1.random.NextDouble() < 0.85)
		{
			Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
			base.TemporarySprites.Add(new SebsFrogs
			{
				texture = crittersText2,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 224, 16, 16),
				animationLength = 1,
				sourceRectStartingPos = new Vector2(64f, 224f),
				interval = 100f,
				totalNumberOfLoops = 9999,
				position = spot * 64f + new Vector2(Game1.random.Choose(22, 25), Game1.random.Choose(2, 1)) * 4f,
				scale = 4f,
				flipped = Game1.random.NextBool(),
				layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
				Parent = this
			});
		}
		if (!Game1.player.activeDialogueEvents.ContainsKey("sebastianFrog2") && Game1.random.NextBool())
		{
			Texture2D crittersText3 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
			base.TemporarySprites.Add(new SebsFrogs
			{
				texture = crittersText3,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 240, 16, 16),
				animationLength = 1,
				sourceRectStartingPos = new Vector2(64f, 240f),
				interval = 150f,
				totalNumberOfLoops = 9999,
				position = spot * 64f + new Vector2(8f, 3f) * 4f,
				scale = 4f,
				layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
				flipped = Game1.random.NextBool(),
				pingPong = false,
				Parent = this
			});
			if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
			{
				DelayedAction.playSoundAfterDelay("croak", 1000);
			}
		}
	}

	private void addFurnitureIfSpaceIsFreePenny(List<Object> objectsToStoreInChests, Furniture f, Furniture heldObject = null)
	{
		bool fail = false;
		foreach (Furniture furniture in base.furniture)
		{
			if (f.GetBoundingBox().Intersects(furniture.GetBoundingBox()))
			{
				fail = true;
				break;
			}
		}
		if (base.objects.ContainsKey(f.TileLocation))
		{
			fail = true;
		}
		if (!fail)
		{
			base.furniture.Add(f);
			if (heldObject != null)
			{
				f.heldObject.Value = heldObject;
			}
		}
		else
		{
			objectsToStoreInChests.Add(f);
			if (heldObject != null)
			{
				objectsToStoreInChests.Add(heldObject);
			}
		}
	}

	private void decoratePennyRoom(int whichStyle, List<Object> objectsToStoreInChests)
	{
		List<Chest> chests = new List<Chest>();
		List<Vector2> chest_positions = new List<Vector2>();
		Color chest_color = default(Color);
		switch (whichStyle)
		{
		case 0:
			if (this.upgradeLevel == 1)
			{
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(20, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(21, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1915").SetPlacement(22, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(23, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(24, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1682").SetPlacement(26, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(25, 4));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1395").SetPlacement(26, 4), ItemRegistry.Create<Furniture>("(F)1363"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443").SetPlacement(27, 4));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664").SetPlacement(27, 5, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1978").SetPlacement(21, 6));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(26, 9), ItemRegistry.Create<Furniture>("(F)1368"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)6").SetPlacement(25, 10, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(28, 10));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(24, 10));
				base.SetWallpaper("107", "Bedroom");
				base.SetFloor("2", "Bedroom");
				chest_color = new Color(85, 85, 255);
				chest_positions.Add(new Vector2(21f, 10f));
				chest_positions.Add(new Vector2(22f, 10f));
			}
			else
			{
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(38, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(39, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1604").SetPlacement(41, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1915").SetPlacement(43, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(45, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(47, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(48, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443").SetPlacement(38, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(39, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1395").SetPlacement(40, 23), ItemRegistry.Create<Furniture>("(F)1363"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)714").SetPlacement(46, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443").SetPlacement(48, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1978").SetPlacement(42, 25));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664").SetPlacement(47, 25, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664").SetPlacement(38, 27, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(46, 31), ItemRegistry.Create<Furniture>("(F)1368"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)416").SetPlacement(40, 32, 2));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(38, 32));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)6").SetPlacement(45, 32, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(48, 32));
				base.SetWallpaper("107", "Bedroom");
				base.SetFloor("2", "Bedroom");
				chest_color = new Color(85, 85, 255);
				chest_positions.Add(new Vector2(38f, 24f));
				chest_positions.Add(new Vector2(39f, 24f));
			}
			break;
		case 1:
			if (this.upgradeLevel == 1)
			{
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1678").SetPlacement(20, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(21, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(22, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(23, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907").SetPlacement(24, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1400").SetPlacement(25, 4), ItemRegistry.Create<Furniture>("(F)1365"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1866").SetPlacement(26, 4));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909").SetPlacement(27, 6, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1451").SetPlacement(21, 6));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1138").SetPlacement(27, 9), ItemRegistry.Create<Furniture>("(F)1378"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)12").SetPlacement(26, 10, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(24, 10));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(21, 9));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1390").SetPlacement(22, 10));
				base.SetWallpaper("84", "Bedroom");
				base.SetFloor("35", "Bedroom");
				chest_color = new Color(255, 85, 85);
				chest_positions.Add(new Vector2(21f, 10f));
				chest_positions.Add(new Vector2(23f, 10f));
			}
			else
			{
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1678").SetPlacement(39, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907").SetPlacement(40, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(42, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(43, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(44, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907").SetPlacement(45, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(48, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(38, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1400").SetPlacement(40, 23), ItemRegistry.Create<Furniture>("(F)1365"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1390").SetPlacement(46, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1866").SetPlacement(47, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1387").SetPlacement(38, 24));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909").SetPlacement(47, 24, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)719").SetPlacement(38, 25, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1451").SetPlacement(42, 25));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909").SetPlacement(38, 27, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1389").SetPlacement(47, 29));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1377").SetPlacement(48, 29));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(41, 30));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)424").SetPlacement(42, 30, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(44, 30));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)536").SetPlacement(47, 30, 3));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1138").SetPlacement(38, 31), ItemRegistry.Create<Furniture>("(F)1378"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1383").SetPlacement(41, 31));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1449").SetPlacement(48, 32));
				base.SetWallpaper("84", "Bedroom");
				base.SetFloor("35", "Bedroom");
				chest_color = new Color(255, 85, 85);
				chest_positions.Add(new Vector2(39f, 23f));
				chest_positions.Add(new Vector2(43f, 25f));
			}
			break;
		case 2:
			if (this.upgradeLevel == 1)
			{
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(20, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1547").SetPlacement(21, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1675").SetPlacement(24, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1900").SetPlacement(25, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1393").SetPlacement(25, 4), ItemRegistry.Create<Furniture>("(F)1367"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1798").SetPlacement(26, 4));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1902").SetPlacement(25, 5));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1751").SetPlacement(22, 6));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1122").SetPlacement(26, 9), ItemRegistry.Create<Furniture>("(F)1378"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)197").SetPlacement(28, 9, 3));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3").SetPlacement(25, 10, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(20, 10));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(24, 10));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1964").SetPlacement(21, 8));
				base.SetWallpaper("95", "Bedroom");
				base.SetFloor("1", "Bedroom");
				chest_color = new Color(85, 85, 85);
				chest_positions.Add(new Vector2(22f, 10f));
				chest_positions.Add(new Vector2(23f, 10f));
			}
			else
			{
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(38, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1675").SetPlacement(40, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1547").SetPlacement(42, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1900").SetPlacement(45, 20));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1751").SetPlacement(38, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1393").SetPlacement(40, 23), ItemRegistry.Create<Furniture>("(F)1367"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1798").SetPlacement(47, 23));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1902").SetPlacement(46, 24));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1964").SetPlacement(42, 25));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(38, 26));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3").SetPlacement(46, 29));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(38, 30));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1122").SetPlacement(46, 30), ItemRegistry.Create<Furniture>("(F)1369"));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)197").SetPlacement(48, 30, 3));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)709").SetPlacement(38, 31, 1));
				this.addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3").SetPlacement(47, 32, 2));
				base.SetWallpaper("95", "Bedroom");
				base.SetFloor("1", "Bedroom");
				chest_color = new Color(85, 85, 85);
				chest_positions.Add(new Vector2(39f, 23f));
				chest_positions.Add(new Vector2(46f, 23f));
			}
			break;
		}
		if (objectsToStoreInChests != null)
		{
			foreach (Object o in objectsToStoreInChests)
			{
				if (chests.Count == 0)
				{
					chests.Add(new Chest(playerChest: true));
				}
				bool found_chest_to_stash_in = false;
				foreach (Chest item in chests)
				{
					if (item.addItem(o) == null)
					{
						found_chest_to_stash_in = true;
					}
				}
				if (!found_chest_to_stash_in)
				{
					Chest new_chest = new Chest(playerChest: true);
					chests.Add(new_chest);
					new_chest.addItem(o);
				}
			}
		}
		for (int i = 0; i < chests.Count; i++)
		{
			Chest chest = chests[i];
			chest.playerChoiceColor.Value = chest_color;
			Vector2 chest_position = chest_positions[Math.Min(i, chest_positions.Count - 1)];
			this.PlaceInNearbySpace(chest_position, chest);
		}
	}

	public void PlaceInNearbySpace(Vector2 tileLocation, Object o)
	{
		if (o == null || tileLocation.Equals(Vector2.Zero))
		{
			return;
		}
		int attempts = 0;
		Queue<Vector2> open_list = new Queue<Vector2>();
		HashSet<Vector2> closed_list = new HashSet<Vector2>();
		open_list.Enqueue(tileLocation);
		Vector2 current = Vector2.Zero;
		for (; attempts < 100; attempts++)
		{
			current = open_list.Dequeue();
			if (this.CanItemBePlacedHere(current))
			{
				break;
			}
			closed_list.Add(current);
			foreach (Vector2 v in Utility.getAdjacentTileLocations(current))
			{
				if (!closed_list.Contains(v))
				{
					open_list.Enqueue(v);
				}
			}
		}
		if (!current.Equals(Vector2.Zero) && this.CanItemBePlacedHere(current))
		{
			o.TileLocation = current;
			base.objects.Add(current, o);
		}
	}

	public virtual void RefreshFloorObjectNeighbors()
	{
		foreach (Vector2 key in base.terrainFeatures.Keys)
		{
			if (base.terrainFeatures[key] is Flooring flooring)
			{
				flooring.OnAdded(this, key);
			}
		}
	}

	public void moveObjectsForHouseUpgrade(int whichUpgrade)
	{
		this.previousUpgradeLevel = this.upgradeLevel;
		base.overlayObjects.Clear();
		switch (whichUpgrade)
		{
		case 0:
			if (this.upgradeLevel == 1)
			{
				this.shiftContents(-6, 0);
			}
			break;
		case 1:
			switch (this.upgradeLevel)
			{
			case 0:
				this.shiftContents(6, 0);
				break;
			case 2:
				this.shiftContents(-3, 0);
				break;
			}
			break;
		case 2:
		case 3:
			switch (this.upgradeLevel)
			{
			case 1:
				this.shiftContents(18, 19);
				foreach (Furniture v in base.furniture)
				{
					if (v.tileLocation.X >= 25f && v.tileLocation.X <= 28f && v.tileLocation.Y >= 20f && v.tileLocation.Y <= 21f)
					{
						v.TileLocation = new Vector2(v.tileLocation.X - 3f, v.tileLocation.Y - 9f);
					}
				}
				base.moveFurniture(42, 23, 16, 14);
				base.moveFurniture(43, 23, 17, 14);
				base.moveFurniture(44, 23, 18, 14);
				base.moveFurniture(43, 24, 22, 14);
				base.moveFurniture(44, 24, 23, 14);
				base.moveFurniture(42, 24, 19, 14);
				base.moveFurniture(43, 25, 20, 14);
				base.moveFurniture(44, 26, 21, 14);
				break;
			case 0:
				this.shiftContents(24, 19);
				break;
			}
			break;
		}
	}

	protected override LocalizedContentManager getMapLoader()
	{
		if (this.mapLoader == null)
		{
			this.mapLoader = Game1.game1.xTileContent.CreateTemporary();
		}
		return this.mapLoader;
	}

	protected override void _updateAmbientLighting()
	{
		if (Game1.isStartingToGetDarkOut(this) || base.lightLevel.Value > 0f)
		{
			int time = Game1.timeOfDay + Game1.gameTimeInterval / (Game1.realMilliSecondsPerGameMinute + base.ExtraMillisecondsPerInGameMinute);
			float lerp = 1f - Utility.Clamp((float)Utility.CalculateMinutesBetweenTimes(time, Game1.getTrulyDarkTime(this)) / 120f, 0f, 1f);
			Game1.ambientLight = new Color((byte)Utility.Lerp(Game1.isRaining ? this.rainLightingColor.R : 0, (int)this.nightLightingColor.R, lerp), (byte)Utility.Lerp(Game1.isRaining ? this.rainLightingColor.G : 0, (int)this.nightLightingColor.G, lerp), (byte)Utility.Lerp(0f, (int)this.nightLightingColor.B, lerp));
		}
		else
		{
			Game1.ambientLight = (Game1.isRaining ? this.rainLightingColor : Color.White);
		}
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		base.drawAboveFrontLayer(b);
		if (this.fridge.Value.mutex.IsLocked())
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.fridgePosition.X, this.fridgePosition.Y - 1) * 64f), new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.fridgePosition.Y + 1) * 64 + 1) / 10000f);
		}
	}

	public override void updateMap()
	{
		bool showSpouse = this.HasNpcSpouseOrRoommate();
		base.mapPath.Value = "Maps\\FarmHouse" + ((this.upgradeLevel == 0) ? "" : ((this.upgradeLevel == 3) ? "2" : (this.upgradeLevel.ToString() ?? ""))) + (showSpouse ? "_marriage" : "");
		base.updateMap();
	}

	public virtual void setMapForUpgradeLevel(int level)
	{
		this.upgradeLevel = level;
		int previous_synchronized_displayed_level = this.synchronizedDisplayedLevel.Value;
		this.currentlyDisplayedUpgradeLevel = level;
		this.synchronizedDisplayedLevel.Value = level;
		bool showSpouse = this.HasNpcSpouseOrRoommate();
		if (this.displayingSpouseRoom && !showSpouse)
		{
			this.displayingSpouseRoom = false;
		}
		this.updateMap();
		this.RefreshFloorObjectNeighbors();
		if (showSpouse)
		{
			this.showSpouseRoom();
		}
		base.loadObjects();
		if (level == 3)
		{
			this.AddCellarTiles();
			this.createCellarWarps();
			Game1.player.craftingRecipes.TryAdd("Cask", 0);
		}
		bool need_bed_upgrade = this.previousUpgradeLevel == 0 && this.upgradeLevel >= 0;
		if (this.previousUpgradeLevel >= 0)
		{
			if (this.previousUpgradeLevel < 2 && this.upgradeLevel >= 2)
			{
				for (int x = 0; x < base.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < base.map.Layers[0].LayerHeight; y++)
					{
						if (this.doesTileHaveProperty(x, y, "DefaultChildBedPosition", "Back") != null)
						{
							string bedId = BedFurniture.CHILD_BED_INDEX;
							base.furniture.Add(new BedFurniture(bedId, new Vector2(x, y)));
							break;
						}
					}
				}
			}
			Furniture bed_furniture = null;
			if (this.previousUpgradeLevel == 0)
			{
				foreach (Furniture item in base.furniture)
				{
					if (item is BedFurniture { bedType: BedFurniture.BedType.Single } bed)
					{
						bed_furniture = bed;
						break;
					}
				}
			}
			else
			{
				foreach (Furniture item2 in base.furniture)
				{
					if (item2 is BedFurniture { bedType: BedFurniture.BedType.Double } bed2)
					{
						bed_furniture = bed2;
						break;
					}
				}
			}
			if (this.upgradeLevel != 3 || need_bed_upgrade)
			{
				for (int i = 0; i < base.map.Layers[0].LayerWidth; i++)
				{
					for (int j = 0; j < base.map.Layers[0].LayerHeight; j++)
					{
						if (this.doesTileHaveProperty(i, j, "DefaultBedPosition", "Back") == null)
						{
							continue;
						}
						string bedId2 = BedFurniture.DEFAULT_BED_INDEX;
						if (this.previousUpgradeLevel != 1 || bed_furniture == null || (bed_furniture.tileLocation.X == 39f && bed_furniture.tileLocation.Y == 22f))
						{
							if (bed_furniture != null)
							{
								bedId2 = bed_furniture.ItemId;
							}
							if (this.previousUpgradeLevel == 0 && bed_furniture != null)
							{
								bed_furniture.performRemoveAction();
								Guid guid = base.furniture.GuidOf(bed_furniture);
								base.furniture.Remove(guid);
								bedId2 = Utility.GetDoubleWideVersionOfBed(bedId2);
								base.furniture.Add(new BedFurniture(bedId2, new Vector2(i, j)));
							}
							else if (bed_furniture != null)
							{
								bed_furniture.performRemoveAction();
								Guid guid2 = base.furniture.GuidOf(bed_furniture);
								base.furniture.Remove(guid2);
								base.furniture.Add(new BedFurniture(bed_furniture.ItemId, new Vector2(i, j)));
							}
						}
						break;
					}
				}
			}
			this.previousUpgradeLevel = -1;
		}
		if (previous_synchronized_displayed_level != level)
		{
			base.lightGlows.Clear();
		}
		this.fridgePosition = this.GetFridgePositionFromMap() ?? Point.Zero;
	}

	/// <summary>Get the fridge position by scanning the map tiles for the sprite index.</summary>
	/// <remarks>This is relatively expensive. Most code should use the cached <see cref="F:StardewValley.Locations.FarmHouse.fridgePosition" /> instead.</remarks>
	public Point? GetFridgePositionFromMap()
	{
		Layer layer = base.map.RequireLayer("Buildings");
		for (int y = 0; y < layer.LayerHeight; y++)
		{
			for (int x = 0; x < layer.LayerWidth; x++)
			{
				if (layer.GetTileIndexAt(x, y, "untitled tile sheet") == 173)
				{
					return new Point(x, y);
				}
			}
		}
		return null;
	}

	public void createCellarWarps()
	{
		this.updateCellarWarps();
	}

	public void updateCellarWarps()
	{
		Layer back_layer = base.map.RequireLayer("Back");
		string cellarName = this.GetCellarName();
		if (cellarName == null)
		{
			return;
		}
		for (int x = 0; x < back_layer.LayerWidth; x++)
		{
			for (int y = 0; y < back_layer.LayerHeight; y++)
			{
				string[] touchAction = base.GetTilePropertySplitBySpaces("TouchAction", "Back", x, y);
				if (ArgUtility.Get(touchAction, 0) == "Warp" && ArgUtility.Get(touchAction, 1, "").StartsWith("Cellar"))
				{
					touchAction[1] = cellarName;
					base.setTileProperty(x, y, "Back", "TouchAction", string.Join(" ", touchAction));
				}
			}
		}
		if (this.cellarWarps == null)
		{
			return;
		}
		foreach (Warp warp in this.cellarWarps)
		{
			if (!base.warps.Contains(warp))
			{
				base.warps.Add(warp);
			}
			warp.TargetName = cellarName;
		}
	}

	public virtual Point GetSpouseRoomCorner()
	{
		if (base.TryGetMapPropertyAs("SpouseRoomPosition", out Point position, required: false))
		{
			return position;
		}
		if (this.upgradeLevel != 1)
		{
			return new Point(50, 20);
		}
		return new Point(29, 1);
	}

	public virtual void loadSpouseRoom()
	{
		string obj = ((this.owner?.spouse != null && this.owner.isMarriedOrRoommates()) ? this.owner.spouse : null);
		CharacterData spouseData;
		CharacterSpouseRoomData roomData = ((!NPC.TryGetData(obj, out spouseData)) ? null : spouseData?.SpouseRoom);
		this.spouseRoomSpot = this.GetSpouseRoomCorner();
		this.spouseRoomSpot.X += 3;
		this.spouseRoomSpot.Y += 4;
		if (obj == null)
		{
			return;
		}
		string assetName = roomData?.MapAsset ?? "spouseRooms";
		Microsoft.Xna.Framework.Rectangle sourceArea = roomData?.MapSourceRect ?? CharacterSpouseRoomData.DefaultMapSourceRect;
		Point corner = this.GetSpouseRoomCorner();
		Microsoft.Xna.Framework.Rectangle areaToRefurbish = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
		Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\" + assetName);
		Point fromOrigin = sourceArea.Location;
		base.map.Properties.Remove("Light");
		base.map.Properties.Remove("DayTiles");
		base.map.Properties.Remove("NightTiles");
		List<KeyValuePair<Point, Tile>> bottom_row_tiles = new List<KeyValuePair<Point, Tile>>();
		Layer front_layer = base.map.RequireLayer("Front");
		for (int x = areaToRefurbish.Left; x < areaToRefurbish.Right; x++)
		{
			Point point = new Point(x, areaToRefurbish.Bottom - 1);
			Tile tile = front_layer.Tiles[point.X, point.Y];
			if (tile != null)
			{
				bottom_row_tiles.Add(new KeyValuePair<Point, Tile>(point, tile));
			}
		}
		if (base._appliedMapOverrides.Contains("spouse_room"))
		{
			base._appliedMapOverrides.Remove("spouse_room");
		}
		base.ApplyMapOverride(assetName, "spouse_room", new Microsoft.Xna.Framework.Rectangle(fromOrigin.X, fromOrigin.Y, areaToRefurbish.Width, areaToRefurbish.Height), areaToRefurbish);
		Layer refurbishedBuildingsLayer = refurbishedMap.RequireLayer("Buildings");
		Layer refurbishedFrontLayer = refurbishedMap.RequireLayer("Front");
		for (int i = 0; i < areaToRefurbish.Width; i++)
		{
			for (int y = 0; y < areaToRefurbish.Height; y++)
			{
				int tileIndex = refurbishedBuildingsLayer.GetTileIndexAt(fromOrigin.X + i, fromOrigin.Y + y);
				if (tileIndex != -1)
				{
					base.adjustMapLightPropertiesForLamp(tileIndex, areaToRefurbish.X + i, areaToRefurbish.Y + y, "Buildings");
				}
				if (y < areaToRefurbish.Height - 1)
				{
					tileIndex = refurbishedFrontLayer.GetTileIndexAt(fromOrigin.X + i, fromOrigin.Y + y);
					if (tileIndex != -1)
					{
						base.adjustMapLightPropertiesForLamp(tileIndex, areaToRefurbish.X + i, areaToRefurbish.Y + y, "Front");
					}
				}
			}
		}
		foreach (Point tile2 in areaToRefurbish.GetPoints())
		{
			if (base.getTileIndexAt(tile2, "Paths") == 7)
			{
				this.spouseRoomSpot = tile2;
				break;
			}
		}
		Point spouse_room_spot = this.GetSpouseRoomSpot();
		base.setTileProperty(spouse_room_spot.X, spouse_room_spot.Y, "Back", "NoFurniture", "T");
		foreach (KeyValuePair<Point, Tile> kvp in bottom_row_tiles)
		{
			front_layer.Tiles[kvp.Key.X, kvp.Key.Y] = kvp.Value;
		}
	}

	public virtual Microsoft.Xna.Framework.Rectangle? GetCribBounds()
	{
		if (this.upgradeLevel < 2)
		{
			return null;
		}
		return new Microsoft.Xna.Framework.Rectangle(30, 12, 3, 4);
	}

	public virtual void UpdateChildRoom()
	{
		Microsoft.Xna.Framework.Rectangle? crib_location = this.GetCribBounds();
		if (crib_location.HasValue)
		{
			if (base._appliedMapOverrides.Contains("crib"))
			{
				base._appliedMapOverrides.Remove("crib");
			}
			base.ApplyMapOverride("FarmHouse_Crib_" + this.cribStyle.Value, "crib", null, crib_location);
		}
	}

	public void playerDivorced()
	{
		this.displayingSpouseRoom = false;
	}

	public virtual List<Microsoft.Xna.Framework.Rectangle> getForbiddenPetWarpTiles()
	{
		List<Microsoft.Xna.Framework.Rectangle> forbidden_tiles = new List<Microsoft.Xna.Framework.Rectangle>();
		switch (this.upgradeLevel)
		{
		case 0:
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(2, 8, 3, 4));
			break;
		case 1:
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(8, 8, 3, 4));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(17, 8, 4, 3));
			break;
		case 2:
		case 3:
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(26, 27, 3, 4));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(35, 27, 4, 3));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(27, 15, 4, 3));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(26, 17, 2, 6));
			break;
		}
		return forbidden_tiles;
	}

	public bool canPetWarpHere(Vector2 tile_position)
	{
		foreach (Microsoft.Xna.Framework.Rectangle forbiddenPetWarpTile in this.getForbiddenPetWarpTiles())
		{
			if (forbiddenPetWarpTile.Contains((int)tile_position.X, (int)tile_position.Y))
			{
				return false;
			}
		}
		return true;
	}

	public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
	{
		List<Microsoft.Xna.Framework.Rectangle> walls = new List<Microsoft.Xna.Framework.Rectangle>();
		switch (this.upgradeLevel)
		{
		case 0:
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3));
			break;
		case 1:
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 17, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(18, 6, 2, 2));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(20, 1, 9, 3));
			break;
		case 2:
		case 3:
		{
			bool hasOwner = this.HasOwner;
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 12, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(15, 1, 13, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 3, 2, 2));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 10, 10, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 10, 8, 3));
			int bedroomWidthReduction = ((hasOwner && this.owner.hasOrWillReceiveMail("renovation_corner_open")) ? (-3) : 0);
			if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_bedroom_open"))
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 0, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 10, 13 + bedroomWidthReduction, 3));
			}
			else
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 2, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(23, 10, 11 + bedroomWidthReduction, 3));
			}
			if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_southern_open"))
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(23, 24, 3, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(31, 24, 3, 3));
			}
			else
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_corner_open"))
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(30, 1, 9, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(28, 3, 2, 2));
			}
			else
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			foreach (Microsoft.Xna.Framework.Rectangle item in walls)
			{
				item.Offset(15, 10);
			}
			break;
		}
		}
		return walls;
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is FarmHouse farmhouse)
		{
			this.cribStyle.Value = farmhouse.cribStyle.Value;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
	{
		List<Microsoft.Xna.Framework.Rectangle> floors = new List<Microsoft.Xna.Framework.Rectangle>();
		switch (this.upgradeLevel)
		{
		case 0:
			floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 10, 9));
			break;
		case 1:
			floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 6, 9));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(7, 3, 11, 9));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(18, 8, 2, 2));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(20, 3, 9, 8));
			break;
		case 2:
		case 3:
		{
			bool hasOwner = this.HasOwner;
			floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 12, 6));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(15, 3, 13, 6));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(13, 5, 2, 2));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 12, 10, 11));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(10, 12, 11, 9));
			if (hasOwner && this.owner.mailReceived.Contains("renovation_bedroom_open"))
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 0, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 12, 14, 11));
			}
			else
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 2, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(23, 12, 12, 11));
			}
			if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_southern_open"))
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(23, 26, 11, 8));
			}
			else
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			if (hasOwner && this.owner.hasOrWillReceiveMail("renovation_corner_open"))
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(28, 5, 2, 3));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(30, 3, 9, 6));
			}
			else
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			foreach (Microsoft.Xna.Framework.Rectangle item in floors)
			{
				item.Offset(15, 10);
			}
			break;
		}
		}
		return floors;
	}

	public virtual bool CanModifyCrib()
	{
		if (!this.HasOwner)
		{
			return false;
		}
		if (this.owner.isMarriedOrRoommates() && this.owner.GetSpouseFriendship().DaysUntilBirthing != -1)
		{
			return false;
		}
		foreach (Child child in this.owner.getChildren())
		{
			if (child.Age < 3)
			{
				return false;
			}
		}
		return true;
	}
}
