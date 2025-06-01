using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley;

public class Farm : GameLocation
{
	public class LightningStrikeEvent : NetEventArg
	{
		public Vector2 boltPosition;

		public bool createBolt;

		public bool bigFlash;

		public bool smallFlash;

		public bool destroyedTerrainFeature;

		public void Read(BinaryReader reader)
		{
			this.createBolt = reader.ReadBoolean();
			this.bigFlash = reader.ReadBoolean();
			this.smallFlash = reader.ReadBoolean();
			this.destroyedTerrainFeature = reader.ReadBoolean();
			this.boltPosition.X = reader.ReadInt32();
			this.boltPosition.Y = reader.ReadInt32();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(this.createBolt);
			writer.Write(this.bigFlash);
			writer.Write(this.smallFlash);
			writer.Write(this.destroyedTerrainFeature);
			writer.Write((int)this.boltPosition.X);
			writer.Write((int)this.boltPosition.Y);
		}
	}

	[XmlIgnore]
	[NonInstancedStatic]
	public static Texture2D houseTextures = Game1.content.Load<Texture2D>("Buildings\\houses");

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Buildings.Building.netBuildingPaintColor" /> instead.</summary>
	[NotNetField]
	public NetRef<BuildingPaintColor> housePaintColor = new NetRef<BuildingPaintColor>();

	public const int default_layout = 0;

	public const int riverlands_layout = 1;

	public const int forest_layout = 2;

	public const int mountains_layout = 3;

	public const int combat_layout = 4;

	public const int fourCorners_layout = 5;

	public const int beach_layout = 6;

	public const int mod_layout = 7;

	public const int layout_max = 7;

	[XmlElement("grandpaScore")]
	public readonly NetInt grandpaScore = new NetInt(0);

	[XmlElement("farmCaveReady")]
	public NetBool farmCaveReady = new NetBool(value: false);

	private TemporaryAnimatedSprite shippingBinLid;

	private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle(4480, 832, 256, 192);

	[XmlIgnore]
	private readonly NetRef<Inventory> sharedShippingBin = new NetRef<Inventory>(new Inventory());

	[XmlIgnore]
	public Item lastItemShipped;

	public bool hasSeenGrandpaNote;

	protected Dictionary<string, Dictionary<Point, Tile>> _baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();

	[XmlIgnore]
	public bool hasMatureFairyRoseTonight;

	[XmlElement("greenhouseUnlocked")]
	public readonly NetBool greenhouseUnlocked = new NetBool();

	[XmlElement("greenhouseMoved")]
	public readonly NetBool greenhouseMoved = new NetBool();

	private readonly NetEvent1Field<Vector2, NetVector2> spawnCrowEvent = new NetEvent1Field<Vector2, NetVector2>();

	public readonly NetEvent1<LightningStrikeEvent> lightningStrikeEvent = new NetEvent1<LightningStrikeEvent>();

	[XmlIgnore]
	public Point? mapGrandpaShrinePosition;

	[XmlIgnore]
	public Point? mapMainMailboxPosition;

	[XmlIgnore]
	public Point? mainFarmhouseEntry;

	[XmlIgnore]
	public Vector2? mapSpouseAreaCorner;

	[XmlIgnore]
	public Vector2? mapShippingBinPosition;

	protected Microsoft.Xna.Framework.Rectangle? _mountainForageRectangle;

	protected bool? _shouldSpawnForestFarmForage;

	protected bool? _shouldSpawnBeachFarmForage;

	protected bool? _oceanCrabPotOverride;

	protected string _fishLocationOverride;

	protected float _fishChanceOverride;

	public Point spousePatioSpot;

	public const int numCropsForCrow = 16;

	public Farm()
	{
	}

	public Farm(string mapPath, string name)
		: base(mapPath, name)
	{
		base.isAlwaysActive.Value = true;
	}

	public override bool IsBuildableLocation()
	{
		return true;
	}

	/// <inheritdoc />
	public override void AddDefaultBuildings(bool load = true)
	{
		this.AddDefaultBuilding("Farmhouse", this.GetStarterFarmhouseLocation(), load);
		this.AddDefaultBuilding("Greenhouse", this.GetGreenhouseStartLocation(), load);
		this.AddDefaultBuilding("Shipping Bin", this.GetStarterShippingBinLocation(), load);
		this.AddDefaultBuilding("Pet Bowl", this.GetStarterPetBowlLocation(), load);
		base.BuildStartingCabins();
	}

	public override string GetDisplayName()
	{
		return base.GetDisplayName() ?? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", Game1.player.farmName.Value);
	}

	/// <summary>Get the tile position at which the shipping bin should be created when it's missing.</summary>
	public virtual Vector2 GetStarterShippingBinLocation()
	{
		if (!this.mapShippingBinPosition.HasValue)
		{
			if (!base.TryGetMapPropertyAs("ShippingBinLocation", out Vector2 position, required: false))
			{
				position = new Vector2(71f, 14f);
			}
			this.mapShippingBinPosition = position;
		}
		return this.mapShippingBinPosition.Value;
	}

	/// <summary>Get the tile position at which the pet bowl should be created when it's missing.</summary>
	public virtual Vector2 GetStarterPetBowlLocation()
	{
		if (!base.TryGetMapPropertyAs("PetBowlLocation", out Vector2 tile, required: false))
		{
			return new Vector2(53f, 7f);
		}
		return tile;
	}

	/// <summary>Get the tile position at which the farmhouse should be created when it's missing.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Farm.GetMainFarmHouseEntry" />.</remarks>
	public virtual Vector2 GetStarterFarmhouseLocation()
	{
		Point entry = this.GetMainFarmHouseEntry();
		return new Vector2(entry.X - 5, entry.Y - 3);
	}

	/// <summary>Get the tile position at which the greenhouse should be created when it's missing.</summary>
	public virtual Vector2 GetGreenhouseStartLocation()
	{
		if (base.TryGetMapPropertyAs("GreenhouseLocation", out Vector2 position, required: false))
		{
			return position;
		}
		return Game1.whichFarm switch
		{
			5 => new Vector2(36f, 29f), 
			6 => new Vector2(14f, 14f), 
			_ => new Vector2(25f, 10f), 
		};
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.sharedShippingBin, "sharedShippingBin").AddField(this.spawnCrowEvent, "spawnCrowEvent").AddField(this.lightningStrikeEvent, "lightningStrikeEvent")
			.AddField(this.grandpaScore, "grandpaScore")
			.AddField(this.greenhouseUnlocked, "greenhouseUnlocked")
			.AddField(this.greenhouseMoved, "greenhouseMoved")
			.AddField(this.farmCaveReady, "farmCaveReady");
		this.spawnCrowEvent.onEvent += doSpawnCrow;
		this.lightningStrikeEvent.onEvent += doLightningStrike;
		this.greenhouseMoved.fieldChangeVisibleEvent += delegate
		{
			this.ClearGreenhouseGrassTiles();
		};
	}

	public virtual void ClearGreenhouseGrassTiles()
	{
		if (base.map != null && Game1.gameMode != 6 && this.greenhouseMoved.Value)
		{
			switch (Game1.whichFarm)
			{
			case 0:
			case 3:
			case 4:
				base.ApplyMapOverride("Farm_Greenhouse_Dirt", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)this.GetGreenhouseStartLocation().X, (int)this.GetGreenhouseStartLocation().Y, 9, 6));
				break;
			case 5:
				base.ApplyMapOverride("Farm_Greenhouse_Dirt_FourCorners", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)this.GetGreenhouseStartLocation().X, (int)this.GetGreenhouseStartLocation().Y, 9, 6));
				break;
			case 1:
			case 2:
				break;
			}
		}
	}

	public static string getMapNameFromTypeInt(int type)
	{
		switch (type)
		{
		case 0:
			return "Farm";
		case 1:
			return "Farm_Fishing";
		case 2:
			return "Farm_Foraging";
		case 3:
			return "Farm_Mining";
		case 4:
			return "Farm_Combat";
		case 5:
			return "Farm_FourCorners";
		case 6:
			return "Farm_Island";
		case 7:
			if (Game1.whichModFarm != null)
			{
				return Game1.whichModFarm.MapName;
			}
			break;
		}
		return "Farm";
	}

	public void onNewGame()
	{
		if (Game1.whichFarm == 3 || this.ShouldSpawnMountainOres())
		{
			for (int i = 0; i < 28; i++)
			{
				this.doDailyMountainFarmUpdate();
			}
		}
		else if (Game1.whichFarm == 5)
		{
			for (int j = 0; j < 10; j++)
			{
				this.doDailyMountainFarmUpdate();
			}
		}
		else if (Game1.GetFarmTypeID() == "MeadowlandsFarm")
		{
			for (int x = 47; x < 63; x++)
			{
				base.objects.Add(new Vector2(x, 20f), new Fence(new Vector2(x, 20f), "322", isGate: false));
			}
			for (int y = 16; y < 20; y++)
			{
				base.objects.Add(new Vector2(47f, y), new Fence(new Vector2(47f, y), "322", isGate: false));
			}
			for (int k = 7; k < 20; k++)
			{
				base.objects.Add(new Vector2(62f, k), new Fence(new Vector2(62f, k), "322", k == 13));
			}
			Building b = new Building("Coop", new Vector2(54f, 9f));
			b.FinishConstruction(onGameStart: true);
			b.LoadFromBuildingData(b.GetData(), forUpgrade: false, forConstruction: true);
			b.load();
			FarmAnimal starterChicken = new FarmAnimal("White Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
			FarmAnimal starterChicken2 = new FarmAnimal("Brown Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
			string[] chickenSplit = Game1.content.LoadString("Strings\\1_6_Strings:StarterChicken_Names").Split('|');
			string chickenNames = chickenSplit[Game1.random.Next(chickenSplit.Length)];
			starterChicken.Name = chickenNames.Split(',')[0].Trim();
			starterChicken2.Name = chickenNames.Split(',')[1].Trim();
			(b.GetIndoors() as AnimalHouse).adoptAnimal(starterChicken);
			(b.GetIndoors() as AnimalHouse).adoptAnimal(starterChicken2);
			base.buildings.Add(b);
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		this.UpdatePatio();
		for (int i = base.characters.Count - 1; i >= 0; i--)
		{
			if (base.characters[i] is Pet pet && (base.hasTileAt(pet.TilePoint, "Buildings") || base.hasTileAt(pet.TilePoint.X + 1, pet.TilePoint.Y, "Buildings") || !this.CanSpawnCharacterHere(pet.Tile) || !this.CanSpawnCharacterHere(new Vector2(pet.TilePoint.X + 1, pet.TilePoint.Y))))
			{
				pet.WarpToPetBowl();
			}
		}
		this.lastItemShipped = null;
		if (base.characters.Count > 5)
		{
			int slimesEscaped = base.characters.RemoveWhere((NPC npc) => npc is GreenSlime && Game1.random.NextDouble() < 0.035);
			if (slimesEscaped > 0)
			{
				Game1.multiplayer.broadcastGlobalMessage((slimesEscaped == 1) ? "Strings\\Locations:Farm_1SlimeEscaped" : "Strings\\Locations:Farm_NSlimesEscaped", false, null, slimesEscaped.ToString() ?? "");
			}
		}
		Vector2 key;
		if (Game1.whichFarm == 5)
		{
			if (this.CanItemBePlacedHere(new Vector2(5f, 32f), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && this.CanItemBePlacedHere(new Vector2(6f, 32f), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && this.CanItemBePlacedHere(new Vector2(6f, 33f), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && this.CanItemBePlacedHere(new Vector2(5f, 33f), itemIsPassable: false, CollisionMask.All, CollisionMask.None))
			{
				base.resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(5f, 32f)));
			}
			if (base.objects.Length > 0)
			{
				for (int i2 = 0; i2 < 6; i2++)
				{
					if (Utility.TryGetRandom(base.objects, out key, out var o) && o.IsWeeds() && o.tileLocation.X < 36f && o.tileLocation.Y < 34f)
					{
						o.SetIdAndSprite(792 + Game1.seasonIndex);
					}
				}
			}
		}
		if (this.ShouldSpawnBeachFarmForage())
		{
			while (Game1.random.NextDouble() < 0.9)
			{
				Vector2 v = base.getRandomTile();
				if (!this.CanItemBePlacedHere(v) || base.hasTileAt((int)v.X, (int)v.Y, "AlwaysFront"))
				{
					continue;
				}
				string whichItem = null;
				if (this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "BeachSpawn", "Back") != "")
				{
					whichItem = "372";
					Game1.stats.Increment("beachFarmSpawns");
					switch (Game1.random.Next(6))
					{
					case 0:
						whichItem = "393";
						break;
					case 1:
						whichItem = "719";
						break;
					case 2:
						whichItem = "718";
						break;
					case 3:
						whichItem = "723";
						break;
					case 4:
					case 5:
						whichItem = "152";
						break;
					}
					if (Game1.stats.DaysPlayed > 1)
					{
						if (Game1.random.NextDouble() < 0.15 || Game1.stats.Get("beachFarmSpawns") % 4 == 0)
						{
							whichItem = Game1.random.Next(922, 925).ToString();
							base.objects.Add(v, new Object(whichItem, 1)
							{
								Fragility = 2,
								MinutesUntilReady = 3
							});
							whichItem = null;
						}
						else if (Game1.random.NextDouble() < 0.1)
						{
							whichItem = "397";
						}
						else if (Game1.random.NextDouble() < 0.05)
						{
							whichItem = "392";
						}
						else if (Game1.random.NextDouble() < 0.02)
						{
							whichItem = "394";
						}
					}
				}
				else if (Game1.season != Season.Winter && new Microsoft.Xna.Framework.Rectangle(20, 66, 33, 18).Contains((int)v.X, (int)v.Y) && this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back") == "Grass")
				{
					whichItem = Utility.getRandomBasicSeasonalForageItem(Game1.season, (int)Game1.stats.DaysPlayed);
				}
				if (whichItem != null)
				{
					Object obj = ItemRegistry.Create<Object>("(O)" + whichItem);
					obj.CanBeSetDown = false;
					obj.IsSpawnedObject = true;
					this.dropObject(obj, v * 64f, Game1.viewport, initialPlacement: true);
				}
			}
		}
		if (Game1.whichFarm == 2)
		{
			for (int x = 0; x < 20; x++)
			{
				for (int y = 0; y < base.map.Layers[0].LayerHeight; y++)
				{
					if (base.getTileIndexAt(x, y, "Paths") == 21 && this.CanItemBePlacedHere(new Vector2(x, y), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && this.CanItemBePlacedHere(new Vector2(x + 1, y), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && this.CanItemBePlacedHere(new Vector2(x + 1, y + 1), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && this.CanItemBePlacedHere(new Vector2(x, y + 1), itemIsPassable: false, CollisionMask.All, CollisionMask.None))
					{
						base.resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(x, y)));
					}
				}
			}
		}
		if (this.ShouldSpawnForestFarmForage() && !Game1.IsWinter)
		{
			while (Game1.random.NextDouble() < 0.75)
			{
				Vector2 v2 = new Vector2(Game1.random.Next(18), Game1.random.Next(base.map.Layers[0].LayerHeight));
				if (Game1.random.NextBool() || Game1.whichFarm != 2)
				{
					v2 = base.getRandomTile();
				}
				if (this.CanItemBePlacedHere(v2, itemIsPassable: false, CollisionMask.All, CollisionMask.None) && !base.hasTileAt((int)v2.X, (int)v2.Y, "AlwaysFront") && ((Game1.whichFarm == 2 && v2.X < 18f) || this.doesTileHavePropertyNoNull((int)v2.X, (int)v2.Y, "Type", "Back").Equals("Grass")))
				{
					Object obj2 = ItemRegistry.Create<Object>(Game1.season switch
					{
						Season.Spring => Game1.random.Next(4) switch
						{
							0 => "(O)" + 16, 
							1 => "(O)" + 22, 
							2 => "(O)" + 20, 
							_ => "(O)257", 
						}, 
						Season.Summer => Game1.random.Next(4) switch
						{
							0 => "(O)402", 
							1 => "(O)396", 
							2 => "(O)398", 
							_ => "(O)404", 
						}, 
						Season.Fall => Game1.random.Next(4) switch
						{
							0 => "(O)281", 
							1 => "(O)420", 
							2 => "(O)422", 
							_ => "(O)404", 
						}, 
						_ => "(O)792", 
					});
					obj2.CanBeSetDown = false;
					obj2.IsSpawnedObject = true;
					this.dropObject(obj2, v2 * 64f, Game1.viewport, initialPlacement: true);
				}
			}
			if (base.objects.Length > 0)
			{
				for (int i3 = 0; i3 < 6; i3++)
				{
					if (Utility.TryGetRandom(base.objects, out key, out var o2) && o2.IsWeeds())
					{
						o2.SetIdAndSprite(792 + Game1.seasonIndex);
					}
				}
			}
		}
		if (Game1.whichFarm == 3 || Game1.whichFarm == 5 || this.ShouldSpawnMountainOres())
		{
			this.doDailyMountainFarmUpdate();
		}
		if (base.terrainFeatures.Length > 0 && Game1.season == Season.Fall && Game1.dayOfMonth > 1 && Game1.random.NextDouble() < 0.05)
		{
			for (int tries = 0; tries < 10; tries++)
			{
				if (Utility.TryGetRandom(base.terrainFeatures, out var _, out var feature) && feature is Tree tree && tree.growthStage.Value >= 5 && !tree.tapped.Value && !tree.isTemporaryGreenRainTree.Value)
				{
					tree.treeType.Value = "7";
					tree.loadSprite();
					break;
				}
			}
		}
		this.addCrows();
		if (Game1.season != Season.Winter)
		{
			base.spawnWeedsAndStones((Game1.season == Season.Summer) ? 30 : 20);
		}
		base.spawnWeeds(weedsOnly: false);
		this.HandleGrassGrowth(dayOfMonth);
	}

	public void doDailyMountainFarmUpdate()
	{
		double chance = 1.0;
		while (Game1.random.NextDouble() < chance)
		{
			Vector2 v = (this.ShouldSpawnMountainOres() ? Utility.getRandomPositionInThisRectangle(this._mountainForageRectangle.Value, Game1.random) : ((Game1.whichFarm == 5) ? Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(51, 67, 11, 3), Game1.random) : Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(5, 37, 22, 8), Game1.random)));
			if (this.doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back").Equals("Dirt") && this.CanItemBePlacedHere(v, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
			{
				string stone_id = "668";
				int health = 2;
				if (Game1.random.NextDouble() < 0.15)
				{
					base.objects.Add(v, ItemRegistry.Create<Object>("(O)590"));
					continue;
				}
				if (Game1.random.NextBool())
				{
					stone_id = "670";
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					if (Game1.player.MiningLevel >= 8 && Game1.random.NextDouble() < 0.33)
					{
						stone_id = "77";
						health = 7;
					}
					else if (Game1.player.MiningLevel >= 5 && Game1.random.NextBool())
					{
						stone_id = "76";
						health = 5;
					}
					else
					{
						stone_id = "75";
						health = 3;
					}
				}
				if (Game1.random.NextDouble() < 0.21)
				{
					stone_id = "751";
					health = 3;
				}
				if (Game1.player.MiningLevel >= 4 && Game1.random.NextDouble() < 0.15)
				{
					stone_id = "290";
					health = 4;
				}
				if (Game1.player.MiningLevel >= 7 && Game1.random.NextDouble() < 0.1)
				{
					stone_id = "764";
					health = 8;
				}
				if (Game1.player.MiningLevel >= 10 && Game1.random.NextDouble() < 0.01)
				{
					stone_id = "765";
					health = 16;
				}
				base.objects.Add(v, new Object(stone_id, 10)
				{
					MinutesUntilReady = health
				});
			}
			chance *= 0.75;
		}
	}

	/// <inheritdoc />
	public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
	{
		if (base.map != null)
		{
			if (!this._oceanCrabPotOverride.HasValue)
			{
				this._oceanCrabPotOverride = base.map.Properties.ContainsKey("FarmOceanCrabPotOverride");
			}
			if (this._oceanCrabPotOverride.Value)
			{
				return true;
			}
		}
		return base.catchOceanCrabPotFishFromThisSpot(x, y);
	}

	public void addCrows()
	{
		int numCrops = 0;
		foreach (KeyValuePair<Vector2, TerrainFeature> pair in base.terrainFeatures.Pairs)
		{
			if (pair.Value is HoeDirt { crop: not null })
			{
				numCrops++;
			}
		}
		List<Vector2> scarecrowPositions = new List<Vector2>();
		foreach (KeyValuePair<Vector2, Object> v in base.objects.Pairs)
		{
			if (v.Value.IsScarecrow())
			{
				scarecrowPositions.Add(v.Key);
			}
		}
		int potentialCrows = Math.Min(4, numCrops / 16);
		for (int i = 0; i < potentialCrows; i++)
		{
			if (!(Game1.random.NextDouble() < 0.3))
			{
				continue;
			}
			for (int attempts = 0; attempts < 10; attempts++)
			{
				if (!Utility.TryGetRandom(base.terrainFeatures, out var tile, out var feature) || !(feature is HoeDirt dirt2))
				{
					continue;
				}
				Crop crop = dirt2.crop;
				if (crop == null || crop.currentPhase.Value <= 1)
				{
					continue;
				}
				bool scarecrow = false;
				foreach (Vector2 s in scarecrowPositions)
				{
					int radius = base.objects[s].GetRadiusForScarecrow();
					if (Vector2.Distance(s, tile) < (float)radius)
					{
						scarecrow = true;
						base.objects[s].SpecialVariable++;
						break;
					}
				}
				if (!scarecrow)
				{
					dirt2.destroyCrop(showAnimation: false);
					this.spawnCrowEvent.Fire(tile);
				}
				break;
			}
		}
	}

	private void doSpawnCrow(Vector2 v)
	{
		if (base.critters == null && base.isOutdoors.Value)
		{
			base.critters = new List<Critter>();
		}
		base.critters.Add(new Crow((int)v.X, (int)v.Y));
	}

	public static Point getFrontDoorPositionForFarmer(Farmer who)
	{
		Point entry_point = Game1.getFarm().GetMainFarmHouseEntry();
		entry_point.Y--;
		return entry_point;
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		if (timeOfDay >= 1300 && Game1.IsMasterGame)
		{
			foreach (NPC n in new List<Character>(base.characters))
			{
				if (n.isMarried())
				{
					n.returnHomeFromFarmPosition(this);
				}
			}
		}
		foreach (NPC c in base.characters)
		{
			if (c.getSpouse() == Game1.player)
			{
				c.checkForMarriageDialogue(timeOfDay, this);
			}
			if (c is Child child)
			{
				child.tenMinuteUpdate();
			}
		}
		if (!Game1.spawnMonstersAtNight || Game1.farmEvent != null || Game1.timeOfDay < 1900 || !(Game1.random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() / 2.0))
		{
			return;
		}
		if (Game1.random.NextDouble() < 0.25)
		{
			if (base.Equals(Game1.currentLocation))
			{
				this.spawnFlyingMonstersOffScreen();
			}
		}
		else
		{
			this.spawnGroundMonsterOffScreen();
		}
	}

	public void spawnGroundMonsterOffScreen()
	{
		for (int i = 0; i < 15; i++)
		{
			Vector2 spawnLocation = base.getRandomTile();
			if (Utility.isOnScreen(Utility.Vector2ToPoint(spawnLocation), 64, this))
			{
				spawnLocation.X -= Game1.viewport.Width / 64;
			}
			if (!this.CanItemBePlacedHere(spawnLocation))
			{
				continue;
			}
			int combatLevel = Game1.player.CombatLevel;
			bool success;
			if (combatLevel >= 8 && Game1.random.NextDouble() < 0.15)
			{
				base.characters.Add(new ShadowBrute(spawnLocation * 64f)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			else if (Game1.random.NextDouble() < ((Game1.whichFarm == 4) ? 0.66 : 0.33))
			{
				base.characters.Add(new RockGolem(spawnLocation * 64f, combatLevel)
				{
					wildernessFarmMonster = true
				});
				success = true;
			}
			else
			{
				int virtualMineLevel = 1;
				if (combatLevel >= 10)
				{
					virtualMineLevel = 140;
				}
				else if (combatLevel >= 8)
				{
					virtualMineLevel = 100;
				}
				else if (combatLevel >= 4)
				{
					virtualMineLevel = 41;
				}
				base.characters.Add(new GreenSlime(spawnLocation * 64f, virtualMineLevel)
				{
					wildernessFarmMonster = true
				});
				success = true;
			}
			if (!success || !Game1.currentLocation.Equals(this))
			{
				break;
			}
			{
				foreach (KeyValuePair<Vector2, Object> v in base.objects.Pairs)
				{
					if (v.Value?.QualifiedItemId == "(BC)83")
					{
						v.Value.shakeTimer = 1000;
						v.Value.showNextIndex.Value = true;
						Game1.currentLightSources.Add(new LightSource(v.Value.GenerateLightSourceId(v.Value.TileLocation), 4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
					}
				}
				break;
			}
		}
	}

	public void spawnFlyingMonstersOffScreen()
	{
		Vector2 spawnLocation = Vector2.Zero;
		switch (Game1.random.Next(4))
		{
		case 0:
			spawnLocation.X = Game1.random.Next(base.map.Layers[0].LayerWidth);
			break;
		case 3:
			spawnLocation.Y = Game1.random.Next(base.map.Layers[0].LayerHeight);
			break;
		case 1:
			spawnLocation.X = base.map.Layers[0].LayerWidth - 1;
			spawnLocation.Y = Game1.random.Next(base.map.Layers[0].LayerHeight);
			break;
		case 2:
			spawnLocation.Y = base.map.Layers[0].LayerHeight - 1;
			spawnLocation.X = Game1.random.Next(base.map.Layers[0].LayerWidth);
			break;
		}
		if (Utility.isOnScreen(spawnLocation * 64f, 64))
		{
			spawnLocation.X -= Game1.viewport.Width;
		}
		int combatLevel = Game1.player.CombatLevel;
		bool success;
		if (combatLevel >= 10 && Game1.random.NextDouble() < 0.01 && Game1.player.Items.ContainsId("(W)4"))
		{
			base.characters.Add(new Bat(spawnLocation * 64f, 9999)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
		{
			base.characters.Add(new Bat(spawnLocation * 64f, 172)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
		{
			base.characters.Add(new Serpent(spawnLocation * 64f)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 8 && Game1.random.NextBool())
		{
			base.characters.Add(new Bat(spawnLocation * 64f, 81)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 5 && Game1.random.NextBool())
		{
			base.characters.Add(new Bat(spawnLocation * 64f, 41)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else
		{
			base.characters.Add(new Bat(spawnLocation * 64f, 1)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		if (!success || !Game1.currentLocation.Equals(this))
		{
			return;
		}
		foreach (KeyValuePair<Vector2, Object> v in base.objects.Pairs)
		{
			if (v.Value?.QualifiedItemId == "(BC)83")
			{
				v.Value.shakeTimer = 1000;
				v.Value.showNextIndex.Value = true;
				Game1.currentLightSources.Add(new LightSource(v.Value.GenerateLightSourceId(v.Value.TileLocation), 4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			}
		}
	}

	public virtual void requestGrandpaReevaluation()
	{
		this.grandpaScore.Value = 0;
		if (Game1.IsMasterGame)
		{
			Game1.player.eventsSeen.Remove("558292");
			Game1.player.eventsSeen.Add("321777");
		}
		base.removeTemporarySpritesWithID(6666);
	}

	public override void OnMapLoad(Map map)
	{
		this.CacheOffBasePatioArea();
		base.OnMapLoad(map);
	}

	/// <inheritdoc />
	public override void OnBuildingMoved(Building building)
	{
		base.OnBuildingMoved(building);
		if (building.HasIndoorsName("FarmHouse"))
		{
			this.UnsetFarmhouseValues();
		}
		if (building is GreenhouseBuilding)
		{
			this.greenhouseMoved.Value = true;
		}
		if (building.GetIndoors() is FarmHouse house && house.HasNpcSpouseOrRoommate())
		{
			NPC npc = base.getCharacterFromName(house.owner.spouse);
			if (npc != null && !npc.shouldPlaySpousePatioAnimation.Value)
			{
				Game1.player.team.requestNPCGoHome.Fire(npc.Name);
			}
		}
	}

	/// <inheritdoc />
	public override bool ShouldExcludeFromNpcPathfinding()
	{
		return true;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		Point shrine_position = this.GetGrandpaShrinePosition();
		if (tileLocation.X >= shrine_position.X - 1 && tileLocation.X <= shrine_position.X + 1 && tileLocation.Y == shrine_position.Y)
		{
			if (!this.hasSeenGrandpaNote)
			{
				Game1.addMail("hasSeenGrandpaNote", noLetter: true);
				this.hasSeenGrandpaNote = true;
				Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaNote", Game1.player.Name).Replace('\n', '^'));
				return true;
			}
			if (Game1.year >= 3 && this.grandpaScore.Value > 0 && this.grandpaScore.Value < 4)
			{
				if (who.ActiveObject?.QualifiedItemId == "(O)72" && this.grandpaScore.Value < 4)
				{
					who.reduceActiveItemByOne();
					base.playSound("stoneStep");
					base.playSound("fireball");
					DelayedAction.playSoundAfterDelay("yoba", 800, this);
					DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_PlaceDiamond"), 1200);
					Game1.multiplayer.broadcastGrandpaReevaluation();
					Game1.player.freezePause = 1200;
					return true;
				}
				if (who.ActiveObject?.QualifiedItemId != "(O)72")
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_DiamondSlot"));
					return true;
				}
			}
			else
			{
				if (this.grandpaScore.Value >= 4 && !Utility.doesItemExistAnywhere("(BC)160"))
				{
					who.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)160"), grandpaStatueCallback);
					return true;
				}
				if (this.grandpaScore.Value == 0 && Game1.year >= 3)
				{
					Game1.player.eventsSeen.Remove("558292");
					Game1.player.eventsSeen.Add("321777");
				}
			}
		}
		if (base.checkAction(tileLocation, viewport, who))
		{
			return true;
		}
		return false;
	}

	public void grandpaStatueCallback(Item item, Farmer who)
	{
		if (item is Object { QualifiedItemId: "(BC)160" })
		{
			who?.mailReceived.Add("grandpaPerfect");
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		Farm fromFarm = (Farm)l;
		base.TransferDataFromSavedLocation(l);
		this.housePaintColor.Value = fromFarm.housePaintColor.Value;
		this.farmCaveReady.Value = fromFarm.farmCaveReady.Value;
		if (fromFarm.hasSeenGrandpaNote)
		{
			Game1.addMail("hasSeenGrandpaNote", noLetter: true);
		}
		this.UnsetFarmhouseValues();
	}

	public IInventory getShippingBin(Farmer who)
	{
		if (Game1.player.team.useSeparateWallets.Value)
		{
			return who.personalShippingBin.Value;
		}
		return this.sharedShippingBin.Value;
	}

	public void shipItem(Item i, Farmer who)
	{
		if (i != null)
		{
			who.removeItemFromInventory(i);
			this.getShippingBin(who).Add(i);
			this.showShipment(i, playThrowSound: false);
			this.lastItemShipped = i;
			if (Game1.player.ActiveItem == null)
			{
				Game1.player.showNotCarrying();
				Game1.player.Halt();
			}
		}
	}

	public void UnsetFarmhouseValues()
	{
		this.mainFarmhouseEntry = null;
		this.mapMainMailboxPosition = null;
	}

	public void showShipment(Item item, bool playThrowSound = true)
	{
		if (playThrowSound)
		{
			base.localSound("backpackIN");
		}
		DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0);
		int id = Game1.random.Next();
		base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(71f, 13f) * 64f + new Vector2(0f, 5f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 100f,
			totalNumberOfLoops = 1,
			animationLength = 3,
			pingPong = true,
			scale = 4f,
			layerDepth = 0.09601f,
			id = id,
			extraInfoForEndBehavior = id,
			endFunction = base.removeTemporarySpritesWithID
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(71f, 13f) * 64f + new Vector2(0f, 17f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 100f,
			totalNumberOfLoops = 1,
			animationLength = 3,
			pingPong = true,
			scale = 4f,
			layerDepth = 0.0963f,
			id = id,
			extraInfoForEndBehavior = id
		});
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
		ColoredObject coloredObj = item as ColoredObject;
		Vector2 initialPosition = new Vector2(71f, 13f) * 64f + new Vector2(8 + Game1.random.Next(6), 2f) * 4f;
		bool[] array = new bool[2] { false, true };
		foreach (bool isColorOverlay in array)
		{
			if (!isColorOverlay || (coloredObj != null && !coloredObj.ColorSameIndexAsParentSheetIndex))
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(isColorOverlay ? 1 : 0), initialPosition, flipped: false, 0f, Color.White)
				{
					interval = 9999f,
					scale = 4f,
					alphaFade = 0.045f,
					layerDepth = 0.096225f,
					motion = new Vector2(0f, 0.3f),
					acceleration = new Vector2(0f, 0.2f),
					scaleChange = -0.05f,
					color = (coloredObj?.color.Value ?? Color.White)
				});
			}
		}
	}

	public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string location = null)
	{
		if (this._fishLocationOverride == null)
		{
			this._fishLocationOverride = "";
			string[] fields = base.GetMapPropertySplitBySpaces("FarmFishLocationOverride");
			if (fields.Length != 0)
			{
				if (!ArgUtility.TryGet(fields, 0, out var targetLocation, out var error, allowBlank: true, "string targetLocation") || !ArgUtility.TryGetFloat(fields, 1, out var chance, out error, "float chance"))
				{
					base.LogMapPropertyError("FarmFishLocationOverride", fields, error);
				}
				else
				{
					this._fishLocationOverride = targetLocation;
					this._fishChanceOverride = chance;
				}
			}
		}
		if (this._fishChanceOverride > 0f && Game1.random.NextDouble() < (double)this._fishChanceOverride)
		{
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, this._fishLocationOverride);
		}
		return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (!this.greenhouseUnlocked.Value && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccPantry"))
		{
			this.greenhouseUnlocked.Value = true;
		}
		for (int i = base.characters.Count - 1; i >= 0; i--)
		{
			if (Game1.timeOfDay >= 1300 && base.characters[i].isMarried() && base.characters[i].controller == null)
			{
				base.characters[i].Halt();
				base.characters[i].drawOffset = Vector2.Zero;
				base.characters[i].Sprite.StopAnimation();
				FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(base.characters[i].getSpouse().homeLocation.Value);
				Game1.warpCharacter(base.characters[i], base.characters[i].getSpouse().homeLocation.Value, farmHouse.getKitchenStandingSpot());
				break;
			}
		}
	}

	public virtual void UpdatePatio()
	{
		if (Game1.MasterPlayer.isMarriedOrRoommates() && Game1.MasterPlayer.spouse != null)
		{
			this.addSpouseOutdoorArea(Game1.MasterPlayer.spouse);
		}
		else
		{
			this.addSpouseOutdoorArea("");
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		this.ClearGreenhouseGrassTiles();
		this.UpdatePatio();
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		this.hasSeenGrandpaNote = Game1.player.hasOrWillReceiveMail("hasSeenGrandpaNote");
		if (Game1.player.mailReceived.Add("button_tut_2"))
		{
			Game1.onScreenMenus.Add(new ButtonTutorialMenu(1));
		}
		for (int i = base.characters.Count - 1; i >= 0; i--)
		{
			if (base.characters[i] is Child child)
			{
				child.resetForPlayerEntry(this);
			}
		}
		this.addGrandpaCandles();
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("Farm_Eternal_Parrots") && !base.IsRainingHere())
		{
			for (int j = 0; j < 20; j++)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(49, 24 * Game1.random.Next(4), 24, 24), new Vector2(Game1.viewport.MaxCorner.X, Game1.viewport.Location.Y + Game1.random.Next(64, Game1.viewport.Height / 2)), flipped: false, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(-5f + (float)Game1.random.Next(-10, 11) / 10f, 4f + (float)Game1.random.Next(-10, 11) / 10f),
					acceleration = new Vector2(0f, -0.02f),
					animationLength = 3,
					interval = 100f,
					pingPong = true,
					totalNumberOfLoops = 999,
					delayBeforeAnimationStart = j * 250,
					drawAboveAlwaysFront = true,
					startSound = "batFlap"
				});
			}
			DelayedAction.playSoundAfterDelay("parrot_squawk", 1000);
			DelayedAction.playSoundAfterDelay("parrot_squawk", 4000);
			DelayedAction.playSoundAfterDelay("parrot", 3000);
			DelayedAction.playSoundAfterDelay("parrot", 5500);
			DelayedAction.playSoundAfterDelay("parrot_squawk", 7000);
			for (int k = 0; k < 20; k++)
			{
				DelayedAction.playSoundAfterDelay("batFlap", 5000 + k * 250);
			}
			Game1.player.mailReceived.Add("Farm_Eternal_Parrots");
		}
	}

	public virtual Vector2 GetSpouseOutdoorAreaCorner()
	{
		if (!this.mapSpouseAreaCorner.HasValue)
		{
			if (!base.TryGetMapPropertyAs("SpouseAreaLocation", out Vector2 position, required: false))
			{
				position = new Vector2(69f, 6f);
			}
			this.mapSpouseAreaCorner = position;
		}
		return this.mapSpouseAreaCorner.Value;
	}

	public virtual void CacheOffBasePatioArea()
	{
		this._baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();
		List<string> layers_to_cache = new List<string>();
		foreach (Layer layer in base.map.Layers)
		{
			layers_to_cache.Add(layer.Id);
		}
		foreach (string layer_name in layers_to_cache)
		{
			Layer original_layer = base.map.GetLayer(layer_name);
			Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
			this._baseSpouseAreaTiles[layer_name] = tiles;
			Vector2 spouse_area_corner = this.GetSpouseOutdoorAreaCorner();
			for (int x = (int)spouse_area_corner.X; x < (int)spouse_area_corner.X + 4; x++)
			{
				for (int y = (int)spouse_area_corner.Y; y < (int)spouse_area_corner.Y + 4; y++)
				{
					if (original_layer == null)
					{
						tiles[new Point(x, y)] = null;
					}
					else
					{
						tiles[new Point(x, y)] = original_layer.Tiles[x, y];
					}
				}
			}
		}
	}

	public virtual void ReapplyBasePatioArea()
	{
		foreach (string layer in this._baseSpouseAreaTiles.Keys)
		{
			Layer map_layer = base.map.GetLayer(layer);
			foreach (Point location in this._baseSpouseAreaTiles[layer].Keys)
			{
				Tile base_tile = this._baseSpouseAreaTiles[layer][location];
				if (map_layer != null)
				{
					map_layer.Tiles[location.X, location.Y] = base_tile;
				}
			}
		}
	}

	public void addSpouseOutdoorArea(string spouseName)
	{
		this.ReapplyBasePatioArea();
		Point patio_corner = Utility.Vector2ToPoint(this.GetSpouseOutdoorAreaCorner());
		this.spousePatioSpot = new Point(patio_corner.X + 2, patio_corner.Y + 3);
		CharacterData spouseData;
		CharacterSpousePatioData patioData = (NPC.TryGetData(spouseName, out spouseData) ? spouseData.SpousePatio : null);
		if (patioData == null)
		{
			return;
		}
		string assetName = patioData.MapAsset ?? "spousePatios";
		Microsoft.Xna.Framework.Rectangle sourceArea = patioData.MapSourceRect;
		int width = Math.Min(sourceArea.Width, 4);
		int height = Math.Min(sourceArea.Height, 4);
		Point corner = patio_corner;
		Microsoft.Xna.Framework.Rectangle areaToRefurbish = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, width, height);
		Point fromOrigin = sourceArea.Location;
		if (base._appliedMapOverrides.Contains("spouse_patio"))
		{
			base._appliedMapOverrides.Remove("spouse_patio");
		}
		base.ApplyMapOverride(assetName, "spouse_patio", new Microsoft.Xna.Framework.Rectangle(fromOrigin.X, fromOrigin.Y, areaToRefurbish.Width, areaToRefurbish.Height), areaToRefurbish);
		foreach (Point tile in areaToRefurbish.GetPoints())
		{
			if (base.getTileIndexAt(tile, "Paths") == 7)
			{
				this.spousePatioSpot = tile;
				break;
			}
		}
	}

	public void addGrandpaCandles()
	{
		Point grandpa_shrine_location = this.GetGrandpaShrinePosition();
		if (this.grandpaScore.Value > 0)
		{
			Microsoft.Xna.Framework.Rectangle candleSource = new Microsoft.Xna.Framework.Rectangle(577, 1985, 2, 5);
			base.removeTemporarySpritesWithIDLocal(6666);
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X - 1) * 64 + 20, (grandpa_shrine_location.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X - 1) * 64 + 12, (grandpa_shrine_location.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 7,
				lightId = "Farm_GrandpaCandles_1",
				id = 6666,
				lightRadius = 1f,
				scale = 3f,
				layerDepth = 0.038500004f,
				delayBeforeAnimationStart = 0
			});
			if (this.grandpaScore.Value > 1)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X - 1) * 64 + 40, (grandpa_shrine_location.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X - 1) * 64 + 36, (grandpa_shrine_location.Y - 2) * 64), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					lightId = "Farm_GrandpaCandles_2",
					id = 6666,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 50
				});
			}
			if (this.grandpaScore.Value > 2)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X + 1) * 64 + 20, (grandpa_shrine_location.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X + 1) * 64 + 16, (grandpa_shrine_location.Y - 2) * 64), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					lightId = "Farm_GrandpaCandles_3",
					id = 6666,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 100
				});
			}
			if (this.grandpaScore.Value > 3)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X + 1) * 64 + 40, (grandpa_shrine_location.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X + 1) * 64 + 36, (grandpa_shrine_location.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					lightId = "Farm_GrandpaCandles_4",
					id = 6666,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 150
				});
			}
		}
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(176, 157, 15, 16), 99999f, 1, 9999, new Vector2(grandpa_shrine_location.X * 64 + 4, (grandpa_shrine_location.Y - 2) * 64 - 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
		}
	}

	private void openShippingBinLid()
	{
		if (this.shippingBinLid != null)
		{
			if (this.shippingBinLid.pingPongMotion != 1 && Game1.currentLocation == this)
			{
				base.localSound("doorCreak");
			}
			this.shippingBinLid.pingPongMotion = 1;
			this.shippingBinLid.paused = false;
		}
	}

	private void closeShippingBinLid()
	{
		if (this.shippingBinLid != null && this.shippingBinLid.currentParentTileIndex > 0)
		{
			if (this.shippingBinLid.pingPongMotion != -1 && Game1.currentLocation == this)
			{
				base.localSound("doorCreakReverse");
			}
			this.shippingBinLid.pingPongMotion = -1;
			this.shippingBinLid.paused = false;
		}
	}

	private void updateShippingBinLid(GameTime time)
	{
		if (this.isShippingBinLidOpen(requiredToBeFullyOpen: true) && this.shippingBinLid.pingPongMotion == 1)
		{
			this.shippingBinLid.paused = true;
		}
		else if (this.shippingBinLid.currentParentTileIndex == 0 && this.shippingBinLid.pingPongMotion == -1)
		{
			if (!this.shippingBinLid.paused && Game1.currentLocation == this)
			{
				base.localSound("woodyStep");
			}
			this.shippingBinLid.paused = true;
		}
		this.shippingBinLid.update(time);
	}

	private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
	{
		if (this.shippingBinLid != null && this.shippingBinLid.currentParentTileIndex >= ((!requiredToBeFullyOpen) ? 1 : (this.shippingBinLid.animationLength - 1)))
		{
			return true;
		}
		return false;
	}

	public override void pokeTileForConstruction(Vector2 tile)
	{
		base.pokeTileForConstruction(tile);
		foreach (NPC character in base.characters)
		{
			if (character is Pet pet && pet.Tile == tile)
			{
				pet.FacingDirection = Game1.random.Next(0, 4);
				pet.faceDirection(pet.FacingDirection);
				pet.CurrentBehavior = "Walk";
				pet.forceUpdateTimer = 2000;
				pet.setMovingInFacingDirection();
			}
		}
	}

	public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
	{
		if (this.doesTileHaveProperty((int)p.X, (int)p.Y, "NoSpawn", "Back") == "All" && this.doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back") == "Wood")
		{
			return true;
		}
		return base.shouldShadowBeDrawnAboveBuildingsLayer(p);
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (Game1.mailbox.Count > 0)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			Point mailbox_position = Game1.player.getMailboxPosition();
			float draw_layer = (float)((mailbox_position.X + 1) * 64) / 10000f + (float)(mailbox_position.Y * 64) / 10000f;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64, (float)(mailbox_position.Y * 64 - 96 - 48) + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64 + 32 + 4, (float)(mailbox_position.Y * 64 - 64 - 24 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
		}
		this.shippingBinLid?.draw(b);
		if (!this.hasSeenGrandpaNote)
		{
			Point grandpa_shrine = this.GetGrandpaShrinePosition();
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((grandpa_shrine.X + 1) * 64, grandpa_shrine.Y * 64)), new Microsoft.Xna.Framework.Rectangle(575, 1972, 11, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(grandpa_shrine.Y * 64) / 10000f + 1E-06f);
		}
	}

	public virtual Point GetMainMailboxPosition()
	{
		if (!this.mapMainMailboxPosition.HasValue)
		{
			if (!base.TryGetMapPropertyAs("MailboxLocation", out Point position, required: false))
			{
				position = new Point(68, 16);
			}
			this.mapMainMailboxPosition = position;
			Building farmhouse = this.GetMainFarmHouse();
			BuildingData buildingData = farmhouse?.GetData();
			if (buildingData?.ActionTiles != null)
			{
				foreach (BuildingActionTile action in buildingData.ActionTiles)
				{
					if (action.Action == "Mailbox")
					{
						this.mapMainMailboxPosition = new Point(farmhouse.tileX.Value + action.Tile.X, farmhouse.tileY.Value + action.Tile.Y);
						break;
					}
				}
			}
		}
		return this.mapMainMailboxPosition.Value;
	}

	public virtual Point GetGrandpaShrinePosition()
	{
		if (!this.mapGrandpaShrinePosition.HasValue)
		{
			if (!base.TryGetMapPropertyAs("GrandpaShrineLocation", out Point position, required: false))
			{
				position = new Point(8, 7);
			}
			this.mapGrandpaShrinePosition = position;
		}
		return this.mapGrandpaShrinePosition.Value;
	}

	/// <summary>Get the door tile position for the farmhouse.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Farm.GetStarterFarmhouseLocation" />.</remarks>
	public virtual Point GetMainFarmHouseEntry()
	{
		if (!this.mainFarmhouseEntry.HasValue)
		{
			if (!base.TryGetMapPropertyAs("FarmHouseEntry", out Point position, required: false))
			{
				position = new Point(64, 15);
			}
			this.mainFarmhouseEntry = position;
			Building farmhouse = this.GetMainFarmHouse();
			if (farmhouse != null)
			{
				this.mainFarmhouseEntry = new Point(farmhouse.tileX.Value + farmhouse.humanDoor.X, farmhouse.tileY.Value + farmhouse.humanDoor.Y + 1);
			}
		}
		return this.mainFarmhouseEntry.Value;
	}

	/// <summary>Get the main player's farmhouse, if found.</summary>
	public virtual Building GetMainFarmHouse()
	{
		return base.getBuildingByType("Farmhouse");
	}

	public override void ResetForEvent(Event ev)
	{
		base.ResetForEvent(ev);
		if (ev.id != "-2")
		{
			Point main_farmhouse_entry = Farm.getFrontDoorPositionForFarmer(ev.farmer);
			main_farmhouse_entry.Y++;
			int offset_x = main_farmhouse_entry.X - 64;
			int offset_y = main_farmhouse_entry.Y - 15;
			ev.eventPositionTileOffset = new Vector2(offset_x, offset_y);
		}
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
	{
		this.spawnCrowEvent.Poll();
		this.lightningStrikeEvent.Poll();
		base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
	}

	public bool isTileOpenBesidesTerrainFeatures(Vector2 tile)
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
		foreach (Building building in base.buildings)
		{
			if (building.intersects(boundingBox))
			{
				return false;
			}
		}
		foreach (ResourceClump resourceClump in base.resourceClumps)
		{
			if (resourceClump.getBoundingBox().Intersects(boundingBox))
			{
				return false;
			}
		}
		foreach (KeyValuePair<long, FarmAnimal> pair in base.animals.Pairs)
		{
			if (pair.Value.Tile == tile)
			{
				return true;
			}
		}
		if (!base.objects.ContainsKey(tile))
		{
			return base.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
		}
		return false;
	}

	private void doLightningStrike(LightningStrikeEvent lightning)
	{
		if (lightning.smallFlash)
		{
			if (Game1.currentLocation.IsOutdoors && !Game1.newDay && Game1.currentLocation.IsLightningHere())
			{
				Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
				if (Game1.random.NextBool())
				{
					DelayedAction.screenFlashAfterDelay((float)(0.3 + Game1.random.NextDouble()), Game1.random.Next(500, 1000));
				}
				DelayedAction.playSoundAfterDelay("thunder_small", Game1.random.Next(500, 1500));
			}
		}
		else if (lightning.bigFlash && Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsLightningHere() && !Game1.newDay)
		{
			Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
			Game1.playSound("thunder");
		}
		if (lightning.createBolt && Game1.currentLocation.name.Equals("Farm"))
		{
			if (lightning.destroyedTerrainFeature)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, lightning.boltPosition, flicker: false, flipped: false));
			}
			Utility.drawLightningBolt(lightning.boltPosition, this);
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (base.wasUpdated && Game1.gameMode != 0)
		{
			return;
		}
		base.UpdateWhenCurrentLocation(time);
		if (this.shippingBinLid == null)
		{
			return;
		}
		bool opening = false;
		foreach (Farmer farmer in base.farmers)
		{
			if (farmer.GetBoundingBox().Intersects(this.shippingBinLidOpenArea))
			{
				this.openShippingBinLid();
				opening = true;
			}
		}
		if (!opening)
		{
			this.closeShippingBinLid();
		}
		this.updateShippingBinLid(time);
	}

	public bool ShouldSpawnMountainOres()
	{
		if (!this._mountainForageRectangle.HasValue)
		{
			this._mountainForageRectangle = (base.TryGetMapPropertyAs("SpawnMountainFarmOreRect", out Microsoft.Xna.Framework.Rectangle area, required: false) ? area : Microsoft.Xna.Framework.Rectangle.Empty);
		}
		return this._mountainForageRectangle.Value.Width > 0;
	}

	public bool ShouldSpawnForestFarmForage()
	{
		if (base.map != null)
		{
			if (!this._shouldSpawnForestFarmForage.HasValue)
			{
				this._shouldSpawnForestFarmForage = base.map.Properties.ContainsKey("SpawnForestFarmForage");
			}
			if (this._shouldSpawnForestFarmForage.Value)
			{
				return true;
			}
		}
		return Game1.whichFarm == 2;
	}

	public bool ShouldSpawnBeachFarmForage()
	{
		if (base.map != null)
		{
			if (!this._shouldSpawnBeachFarmForage.HasValue)
			{
				this._shouldSpawnBeachFarmForage = base.map.Properties.ContainsKey("SpawnBeachFarmForage");
			}
			if (this._shouldSpawnBeachFarmForage.Value)
			{
				return true;
			}
		}
		return Game1.whichFarm == 6;
	}

	public bool SpawnsForage()
	{
		if (!this.ShouldSpawnForestFarmForage())
		{
			return this.ShouldSpawnBeachFarmForage();
		}
		return true;
	}

	public bool doesFarmCaveNeedHarvesting()
	{
		return this.farmCaveReady.Value;
	}
}
