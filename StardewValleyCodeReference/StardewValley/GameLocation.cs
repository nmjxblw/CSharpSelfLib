using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.GarbageCans;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Minecarts;
using StardewValley.GameData.Movies;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Util;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley;

[XmlInclude(typeof(AbandonedJojaMart))]
[XmlInclude(typeof(AdventureGuild))]
[XmlInclude(typeof(AnimalHouse))]
[XmlInclude(typeof(BathHousePool))]
[XmlInclude(typeof(Beach))]
[XmlInclude(typeof(BeachNightMarket))]
[XmlInclude(typeof(BoatTunnel))]
[XmlInclude(typeof(BugLand))]
[XmlInclude(typeof(BusStop))]
[XmlInclude(typeof(Cabin))]
[XmlInclude(typeof(Caldera))]
[XmlInclude(typeof(Cellar))]
[XmlInclude(typeof(Club))]
[XmlInclude(typeof(CommunityCenter))]
[XmlInclude(typeof(DecoratableLocation))]
[XmlInclude(typeof(Desert))]
[XmlInclude(typeof(DesertFestival))]
[XmlInclude(typeof(Farm))]
[XmlInclude(typeof(FarmCave))]
[XmlInclude(typeof(FarmHouse))]
[XmlInclude(typeof(FishShop))]
[XmlInclude(typeof(Forest))]
[XmlInclude(typeof(IslandEast))]
[XmlInclude(typeof(IslandFarmCave))]
[XmlInclude(typeof(IslandFarmHouse))]
[XmlInclude(typeof(IslandFieldOffice))]
[XmlInclude(typeof(IslandForestLocation))]
[XmlInclude(typeof(IslandHut))]
[XmlInclude(typeof(IslandLocation))]
[XmlInclude(typeof(IslandNorth))]
[XmlInclude(typeof(IslandSecret))]
[XmlInclude(typeof(IslandShrine))]
[XmlInclude(typeof(IslandSouth))]
[XmlInclude(typeof(IslandSouthEast))]
[XmlInclude(typeof(IslandSouthEastCave))]
[XmlInclude(typeof(IslandWest))]
[XmlInclude(typeof(IslandWestCave1))]
[XmlInclude(typeof(JojaMart))]
[XmlInclude(typeof(LibraryMuseum))]
[XmlInclude(typeof(ManorHouse))]
[XmlInclude(typeof(MermaidHouse))]
[XmlInclude(typeof(Mine))]
[XmlInclude(typeof(MineShaft))]
[XmlInclude(typeof(Mountain))]
[XmlInclude(typeof(MovieTheater))]
[XmlInclude(typeof(Railroad))]
[XmlInclude(typeof(SeedShop))]
[XmlInclude(typeof(Sewer))]
[XmlInclude(typeof(Shed))]
[XmlInclude(typeof(ShopLocation))]
[XmlInclude(typeof(SlimeHutch))]
[XmlInclude(typeof(Submarine))]
[XmlInclude(typeof(Summit))]
[XmlInclude(typeof(Town))]
[XmlInclude(typeof(WizardHouse))]
[XmlInclude(typeof(Woods))]
[InstanceStatics]
[NotImplicitNetField]
public class GameLocation : INetObject<NetFields>, IEquatable<GameLocation>, IAnimalLocation, IHaveModData
{
	public delegate void afterQuestionBehavior(Farmer who, string whichAnswer);

	/// <summary>A request to damage players who overlap a bounding box within the current location.</summary>
	private struct DamagePlayersEventArg : NetEventArg
	{
		/// <summary>The location pixel area where players will take damage.</summary>
		public Microsoft.Xna.Framework.Rectangle Area;

		/// <summary>The amount of damage the player should take.</summary>
		public int Damage;

		/// <summary>Whether the damage source was a bomb.</summary>
		public bool IsBomb;

		/// <summary>Reads the request data from a net-sync stream.</summary>
		/// <param name="reader">The binary stream to read.</param>
		public void Read(BinaryReader reader)
		{
			this.Area = reader.ReadRectangle();
			this.Damage = reader.ReadInt32();
			this.IsBomb = reader.ReadBoolean();
		}

		/// <summary>Writes the request data to a net-sync stream.</summary>
		/// <param name="writer">The binary stream to write to.</param>
		public void Write(BinaryWriter writer)
		{
			writer.WriteRectangle(this.Area);
			writer.Write(this.Damage);
			writer.Write(this.IsBomb);
		}
	}

	public const int maxTriesForDebrisPlacement = 3;

	/// <summary>The default ID for a map tile sheet. This often (but not always) matches the main tile sheet for Stardew Valley maps.</summary>
	public const string DefaultTileSheetId = "untitled tile sheet";

	public const string OVERRIDE_MAP_TILESHEET_PREFIX = "zzzzz";

	public const string PHONE_DIAL_SOUND = "telephone_buttonPush";

	public const int PHONE_RING_DURATION = 4950;

	public const string PHONE_PICKUP_SOUND = "bigSelect";

	public const string PHONE_HANGUP_SOUND = "openBox";

	/// <summary>The ocean fish types.</summary>
	public static readonly IList<string> OceanCrabPotFishTypes = new string[1] { "ocean" };

	/// <summary>The default fish types caught by crab pots in all locations which don't have a specific value in <c>Data/Locations</c>.</summary>
	public static readonly IList<string> DefaultCrabPotFishTypes = new string[1] { "freshwater" };

	/// <summary>The cached value for <see cref="M:StardewValley.GameLocation.GetSeason" />.</summary>
	/// <remarks>Most code should use <see cref="M:StardewValley.GameLocation.GetSeason" /> instead.</remarks>
	[XmlIgnore]
	private Lazy<Season?> seasonOverride;

	[XmlIgnore]
	public bool? isMusicTownMusic;

	/// <summary>The cached location context ID for <see cref="M:StardewValley.GameLocation.GetLocationContextId" />.</summary>
	/// <remarks>Most code should use <see cref="M:StardewValley.GameLocation.GetLocationContextId" /> or <see cref="M:StardewValley.GameLocation.GetLocationContext" /> instead.</remarks>
	[XmlIgnore]
	public string locationContextId;

	public readonly NetCollection<Building> buildings = new NetCollection<Building>
	{
		InterpolationWait = false
	};

	[XmlElement("animals")]
	public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>();

	[XmlElement("piecesOfHay")]
	public readonly NetInt piecesOfHay = new NetInt(0);

	private readonly List<KeyValuePair<long, FarmAnimal>> tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();

	/// <summary>The unique name of the parent location, if applicable.</summary>
	[XmlIgnore]
	public readonly NetString parentLocationName = new NetString();

	/// <summary>The building which contains this location, if applicable.</summary>
	[XmlIgnore]
	public Building ParentBuilding;

	[XmlIgnore]
	public List<KeyValuePair<Layer, int>> backgroundLayers = new List<KeyValuePair<Layer, int>>();

	[XmlIgnore]
	public List<KeyValuePair<Layer, int>> buildingLayers = new List<KeyValuePair<Layer, int>>();

	[XmlIgnore]
	public List<KeyValuePair<Layer, int>> frontLayers = new List<KeyValuePair<Layer, int>>();

	[XmlIgnore]
	public List<KeyValuePair<Layer, int>> alwaysFrontLayers = new List<KeyValuePair<Layer, int>>();

	[NonInstancedStatic]
	[XmlIgnore]
	protected static Dictionary<string, Action<GameLocation, string[], Farmer, Vector2>> registeredTouchActions = new Dictionary<string, Action<GameLocation, string[], Farmer, Vector2>>();

	[NonInstancedStatic]
	[XmlIgnore]
	protected static Dictionary<string, Func<GameLocation, string[], Farmer, Point, bool>> registeredTileActions = new Dictionary<string, Func<GameLocation, string[], Farmer, Point, bool>>();

	/// <summary>Whether this location should always be synchronized in multiplayer. </summary>
	/// <remarks>
	///   <para>This value should only be set when the location is instantiated, it shouldn't be modified during gameplay.</para>
	///
	///   <para>Most code should call <see cref="M:StardewValley.Multiplayer.isAlwaysActiveLocation(StardewValley.GameLocation)" /> instead.</para>
	/// </remarks>
	[XmlIgnore]
	public NetBool isAlwaysActive = new NetBool();

	[XmlIgnore]
	public afterQuestionBehavior afterQuestion;

	[XmlIgnore]
	public Map map;

	[XmlIgnore]
	public readonly NetString mapPath = new NetString().Interpolated(interpolate: false, wait: false);

	[XmlIgnore]
	protected string loadedMapPath;

	public readonly NetCollection<NPC> characters = new NetCollection<NPC>();

	[XmlIgnore]
	public readonly NetVector2Dictionary<Object, NetRef<Object>> netObjects = new NetVector2Dictionary<Object, NetRef<Object>>();

	[XmlIgnore]
	public readonly OverlayDictionary<Vector2, Object> overlayObjects = new OverlayDictionary<Vector2, Object>(GameLocation.tilePositionComparer);

	[XmlElement("objects")]
	public readonly OverlaidDictionary objects;

	[XmlIgnore]
	public NetList<MapSeat, NetRef<MapSeat>> mapSeats = new NetList<MapSeat, NetRef<MapSeat>>();

	protected bool _mapSeatsDirty;

	[XmlIgnore]
	public TemporaryAnimatedSpriteList temporarySprites = new TemporaryAnimatedSpriteList();

	[XmlIgnore]
	public List<Action> postFarmEventOvernightActions = new List<Action>();

	[XmlIgnore]
	public readonly NetObjectList<Warp> warps = new NetObjectList<Warp>();

	[XmlIgnore]
	public readonly NetPointDictionary<string, NetString> doors = new NetPointDictionary<string, NetString>();

	[XmlIgnore]
	public readonly InteriorDoorDictionary interiorDoors;

	[XmlIgnore]
	public readonly FarmerCollection farmers;

	[XmlIgnore]
	public readonly NetCollection<Projectile> projectiles = new NetCollection<Projectile>();

	public readonly NetCollection<ResourceClump> resourceClumps = new NetCollection<ResourceClump>();

	public readonly NetCollection<LargeTerrainFeature> largeTerrainFeatures = new NetCollection<LargeTerrainFeature>();

	/// <summary>The terrain features whose <see cref="M:StardewValley.TerrainFeatures.TerrainFeature.tickUpdate(Microsoft.Xna.Framework.GameTime)" /> method should be called on each tick.</summary>
	[XmlIgnore]
	public List<TerrainFeature> _activeTerrainFeatures = new List<TerrainFeature>();

	[XmlIgnore]
	public List<Critter> critters;

	[XmlElement("terrainFeatures")]
	public readonly NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = new NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>();

	[XmlIgnore]
	public readonly NetCollection<Debris> debris = new NetCollection<Debris>();

	[XmlIgnore]
	public readonly NetPoint fishSplashPoint = new NetPoint(Point.Zero);

	private int fishSplashPointTime;

	[XmlIgnore]
	public readonly NetString fishFrenzyFish = new NetString();

	[XmlIgnore]
	public readonly NetPoint orePanPoint = new NetPoint(Point.Zero);

	[XmlIgnore]
	public TemporaryAnimatedSprite fishSplashAnimation;

	[XmlIgnore]
	public TemporaryAnimatedSprite orePanAnimation;

	[XmlIgnore]
	public WaterTiles waterTiles;

	[XmlIgnore]
	protected HashSet<string> _appliedMapOverrides;

	[XmlElement("uniqueName")]
	public readonly NetString uniqueName = new NetString();

	[XmlIgnore]
	protected string _displayName;

	[XmlElement("name")]
	public readonly NetString name = new NetString();

	[XmlElement("waterColor")]
	public readonly NetColor waterColor = new NetColor(Color.White * 0.33f);

	[XmlIgnore]
	public string lastQuestionKey;

	[XmlIgnore]
	public Vector2 lastTouchActionLocation = Vector2.Zero;

	[XmlElement("lightLevel")]
	protected readonly NetFloat lightLevel = new NetFloat(0f);

	[XmlElement("isFarm")]
	public readonly NetBool isFarm = new NetBool();

	[XmlElement("isOutdoors")]
	public readonly NetBool isOutdoors = new NetBool();

	[XmlIgnore]
	public readonly NetBool isGreenhouse = new NetBool();

	[XmlElement("isStructure")]
	public readonly NetBool isStructure = new NetBool();

	[XmlElement("ignoreDebrisWeather")]
	public readonly NetBool ignoreDebrisWeather = new NetBool();

	[XmlElement("ignoreOutdoorLighting")]
	public readonly NetBool ignoreOutdoorLighting = new NetBool();

	[XmlElement("ignoreLights")]
	public readonly NetBool ignoreLights = new NetBool();

	[XmlElement("treatAsOutdoors")]
	public readonly NetBool treatAsOutdoors = new NetBool();

	[XmlIgnore]
	public bool wasUpdated;

	public int numberOfSpawnedObjectsOnMap;

	[XmlIgnore]
	public bool showDropboxIndicator;

	[XmlIgnore]
	public Vector2 dropBoxIndicatorLocation;

	[XmlElement("miniJukeboxCount")]
	public readonly NetInt miniJukeboxCount = new NetInt();

	[XmlElement("miniJukeboxTrack")]
	public readonly NetString miniJukeboxTrack = new NetString("");

	[XmlIgnore]
	public readonly NetString randomMiniJukeboxTrack = new NetString();

	[XmlIgnore]
	public Event currentEvent;

	[XmlIgnore]
	public Object actionObjectForQuestionDialogue;

	[XmlIgnore]
	public int waterAnimationIndex;

	[XmlIgnore]
	public int waterAnimationTimer;

	[XmlIgnore]
	public bool waterTileFlip;

	[XmlIgnore]
	public bool forceViewportPlayerFollow;

	[XmlIgnore]
	public bool forceLoadPathLayerLights;

	[XmlIgnore]
	public float waterPosition;

	[XmlIgnore]
	public readonly NetAudio netAudio;

	/// <summary>The light sources to draw for players in this location.</summary>
	[XmlIgnore]
	public readonly NetStringDictionary<LightSource, NetRef<LightSource>> sharedLights = new NetStringDictionary<LightSource, NetRef<LightSource>>();

	private readonly NetEvent1Field<int, NetInt> removeTemporarySpritesWithIDEvent = new NetEvent1Field<int, NetInt>();

	private readonly NetEvent1Field<int, NetInt> rumbleAndFadeEvent = new NetEvent1Field<int, NetInt>();

	/// <summary>An event raised to damage players within the current location.</summary>
	private readonly NetEvent1<DamagePlayersEventArg> damagePlayersEvent = new NetEvent1<DamagePlayersEventArg>();

	[XmlIgnore]
	public NetVector2HashSet lightGlows = new NetVector2HashSet();

	public static readonly int JOURNAL_INDEX = 1000;

	public static readonly float FIRST_SECRET_NOTE_CHANCE = 0.8f;

	public static readonly float LAST_SECRET_NOTE_CHANCE = 0.12f;

	public static readonly int NECKLACE_SECRET_NOTE_INDEX = 25;

	public static readonly string CAROLINES_NECKLACE_ITEM_QID = "(O)191";

	public static readonly string CAROLINES_NECKLACE_MAIL = "carolinesNecklace";

	public static TilePositionComparer tilePositionComparer = new TilePositionComparer();

	protected List<Vector2> _startingCabinLocations = new List<Vector2>();

	[XmlIgnore]
	public bool wasInhabited;

	[XmlIgnore]
	protected bool _madeMapModifications;

	public readonly NetCollection<Furniture> furniture = new NetCollection<Furniture>
	{
		InterpolationWait = false
	};

	protected readonly NetMutexQueue<Guid> furnitureToRemove = new NetMutexQueue<Guid>();

	protected bool _mapPathDirty = true;

	protected LocalizedContentManager _structureMapLoader;

	protected bool ignoreWarps;

	protected HashSet<Vector2> _visitedCollisionTiles = new HashSet<Vector2>();

	protected bool _looserBuildRestrictions;

	protected Microsoft.Xna.Framework.Rectangle? _buildableTileRect;

	private bool showedBuildableButNotAlwaysActiveWarning;

	public static bool PlayedNewLocationContextMusic = false;

	private const int fireIDBase = 944468;

	protected Color indoorLightingColor = new Color(100, 120, 30);

	protected Color indoorLightingNightColor = new Color(150, 150, 30);

	protected static List<KeyValuePair<string, string>> _PagedResponses = new List<KeyValuePair<string, string>>();

	protected static int _PagedResponsePage = 0;

	protected static int _PagedResponseItemsPerPage;

	public static bool _PagedResponseAddCancel;

	protected static string _PagedResponsePrompt;

	protected static Action<string> _OnPagedResponse;

	protected string _constructLocationBuilderName;

	protected List<Farmer> _currentLocationFarmersForDisambiguating = new List<Farmer>();

	[XmlIgnore]
	public Dictionary<Vector2, float> lightGlowLayerCache = new Dictionary<Vector2, float>();

	public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals => this.animals;

	[XmlIgnore]
	public NetFields NetFields { get; }

	[XmlIgnore]
	public NetRoot<GameLocation> Root => this.NetFields.Root as NetRoot<GameLocation>;

	/// <summary>The number of milliseconds to add to <see cref="F:StardewValley.Game1.realMilliSecondsPerGameMinute" /> when calculating the flow of time within this location.</summary>
	[XmlIgnore]
	public int ExtraMillisecondsPerInGameMinute { get; set; }

	[XmlIgnore]
	public string DisplayName
	{
		get
		{
			if (this._displayName == null)
			{
				this._displayName = this.GetDisplayName();
			}
			if (this._displayName == null)
			{
				string parentName = this.GetParentLocation()?.DisplayName;
				if (parentName != null)
				{
					return parentName;
				}
				return this.Name;
			}
			return this._displayName;
		}
		set
		{
			this._displayName = value;
		}
	}

	[XmlIgnore]
	public string NameOrUniqueName
	{
		get
		{
			if (this.uniqueName.Value != null)
			{
				return this.uniqueName.Value;
			}
			return this.name.Value;
		}
	}

	/// <summary>Whether this is a temporary location for a festival or event.</summary>
	/// <remarks>This is set automatically based on <see cref="M:StardewValley.GameLocation.IsTemporaryName(System.String)" />.</remarks>
	[XmlIgnore]
	public bool IsTemporary { get; protected set; }

	[XmlIgnore]
	public float LightLevel
	{
		get
		{
			return this.lightLevel.Value;
		}
		set
		{
			this.lightLevel.Value = value;
		}
	}

	[XmlIgnore]
	public Map Map
	{
		get
		{
			this.updateMap();
			return this.map;
		}
		set
		{
			this.map = value;
		}
	}

	[XmlIgnore]
	public OverlaidDictionary Objects => this.objects;

	[XmlIgnore]
	public TemporaryAnimatedSpriteList TemporarySprites => this.temporarySprites;

	public string Name => this.name.Value;

	[XmlIgnore]
	public bool IsFarm
	{
		get
		{
			return this.isFarm.Value;
		}
		set
		{
			this.isFarm.Value = value;
		}
	}

	[XmlIgnore]
	public bool IsOutdoors
	{
		get
		{
			return this.isOutdoors.Value;
		}
		set
		{
			this.isOutdoors.Value = value;
		}
	}

	public bool IsGreenhouse
	{
		get
		{
			return this.isGreenhouse.Value;
		}
		set
		{
			this.isGreenhouse.Value = value;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();

	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return this.modData.GetForSerialization();
		}
		set
		{
			this.modData.SetFromSerialization(value);
		}
	}

	public virtual string GetDisplayName()
	{
		string displayName = this.GetData()?.DisplayName;
		if (displayName == null)
		{
			return null;
		}
		return TokenParser.ParseText(displayName);
	}

	/// <summary>Whether seeds and sapling can be planted and grown in any season here.</summary>
	public virtual bool SeedsIgnoreSeasonsHere()
	{
		return this.IsGreenhouse;
	}

	/// <summary>Get whether crop seeds can be planted in this location.</summary>
	/// <param name="itemId">The qualified or unqualified item ID for the seed being planted.</param>
	/// <param name="tileX">The X tile position for which to apply location-specific overrides.</param>
	/// <param name="tileY">The Y tile position for which to apply location-specific overrides.</param>
	/// <param name="isGardenPot">Whether the item is being planted in a garden pot.</param>
	/// <param name="deniedMessage">The translated message to show to the user indicating why it can't be planted, if applicable.</param>
	public virtual bool CanPlantSeedsHere(string itemId, int tileX, int tileY, bool isGardenPot, out string deniedMessage)
	{
		return this.CheckItemPlantRules(itemId, isGardenPot, this.GetData()?.CanPlantHere ?? this.IsFarm, out deniedMessage);
	}

	/// <summary>Get whether tree saplings can be planted in this location.</summary>
	/// <param name="itemId">The qualified or unqualified item ID for the sapling being planted.</param>
	/// <param name="tileX">The X tile position for which to apply location-specific overrides.</param>
	/// <param name="tileY">The Y tile position for which to apply location-specific overrides.</param>
	/// <param name="deniedMessage">The translated message to show to the user indicating why it can't be planted, if applicable.</param>
	public virtual bool CanPlantTreesHere(string itemId, int tileX, int tileY, out string deniedMessage)
	{
		return this.CheckItemPlantRules(itemId, isGardenPot: false, this.IsGreenhouse || this.IsFarm || (this.GetData()?.CanPlantHere ?? false) || (Object.isWildTreeSeed(itemId) && this.IsOutdoors && this.doesTileHavePropertyNoNull(tileX, tileY, "Type", "Back") == "Dirt") || (this.map?.Properties.ContainsKey("ForceAllowTreePlanting") ?? false), out deniedMessage);
	}

	/// <summary>Get whether a crop or tree can be planted here according to the planting rules in its data.</summary>
	/// <param name="itemId">The qualified or unqualified item ID for the seed or sapling being planted.</param>
	/// <param name="isGardenPot">Whether the item is being planted in a garden pot.</param>
	/// <param name="defaultAllowed">The result to return when no rules apply, or the selected rule uses <see cref="F:StardewValley.GameData.PlantableResult.Default" />.</param>
	/// <param name="deniedMessage">The translated message to show to the user indicating why it can't be planted, if applicable.</param>
	/// <remarks>This is a low-level method which doesn't check higher-level requirements. Most code should call <see cref="M:StardewValley.GameLocation.CanPlantSeedsHere(System.String,System.Int32,System.Int32,System.Boolean,System.String@)" /> or <see cref="M:StardewValley.GameLocation.CanPlantTreesHere(System.String,System.Int32,System.Int32,System.String@)" /> instead.</remarks>
	public bool CheckItemPlantRules(string itemId, bool isGardenPot, bool defaultAllowed, out string deniedMessage)
	{
		ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
		if (metadata != null && metadata.TypeIdentifier == "(O)")
		{
			itemId = metadata.LocalItemId;
			if (Crop.TryGetData(itemId, out var cropData))
			{
				return this.CheckItemPlantRules(cropData.PlantableLocationRules, isGardenPot, defaultAllowed, out deniedMessage);
			}
			string wildTreeType = Tree.ResolveTreeTypeFromSeed(metadata.QualifiedItemId);
			if (wildTreeType != null && Tree.TryGetData(wildTreeType, out var wildTreeData))
			{
				return this.CheckItemPlantRules(wildTreeData.PlantableLocationRules, isGardenPot, defaultAllowed, out deniedMessage);
			}
			if (FruitTree.TryGetData(itemId, out var fruitTreeData))
			{
				return this.CheckItemPlantRules(fruitTreeData.PlantableLocationRules, isGardenPot, defaultAllowed, out deniedMessage);
			}
		}
		deniedMessage = null;
		return defaultAllowed;
	}

	/// <summary>Get whether a crop or tree can be planted here according to the planting rules in its data.</summary>
	/// <param name="rules">The plantable rules to check.</param>
	/// <param name="isGardenPot">Whether the item is being planted in a garden pot.</param>
	/// <param name="defaultAllowed">The result to return when no rules apply, or the selected rule uses <see cref="F:StardewValley.GameData.PlantableResult.Default" />.</param>
	/// <param name="deniedMessage">The translated message to show to the user indicating why it can't be planted, if applicable.</param>
	/// <remarks>This is a low-level method which doesn't check higher-level requirements. Most code should call <see cref="M:StardewValley.GameLocation.CanPlantSeedsHere(System.String,System.Int32,System.Int32,System.Boolean,System.String@)" /> or <see cref="M:StardewValley.GameLocation.CanPlantTreesHere(System.String,System.Int32,System.Int32,System.String@)" /> instead.</remarks>
	private bool CheckItemPlantRules(List<PlantableRule> rules, bool isGardenPot, bool defaultAllowed, out string deniedMessage)
	{
		if (rules != null && rules.Count > 0)
		{
			foreach (PlantableRule rule in rules)
			{
				if (rule.ShouldApplyWhen(isGardenPot) && GameStateQuery.CheckConditions(rule.Condition, this))
				{
					switch (rule.Result)
					{
					case PlantableResult.Allow:
						deniedMessage = null;
						return true;
					case PlantableResult.Deny:
						deniedMessage = TokenParser.ParseText(rule.DeniedMessage);
						return false;
					default:
						deniedMessage = ((!defaultAllowed) ? TokenParser.ParseText(rule.DeniedMessage) : null);
						return defaultAllowed;
					}
				}
			}
		}
		deniedMessage = null;
		return defaultAllowed;
	}

	protected virtual void initNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.mapPath, "mapPath").AddField(this.uniqueName, "uniqueName")
			.AddField(this.name, "name")
			.AddField(this.lightLevel, "lightLevel")
			.AddField(this.sharedLights, "sharedLights")
			.AddField(this.isFarm, "isFarm")
			.AddField(this.isOutdoors, "isOutdoors")
			.AddField(this.isStructure, "isStructure")
			.AddField(this.ignoreDebrisWeather, "ignoreDebrisWeather")
			.AddField(this.ignoreOutdoorLighting, "ignoreOutdoorLighting")
			.AddField(this.ignoreLights, "ignoreLights")
			.AddField(this.treatAsOutdoors, "treatAsOutdoors")
			.AddField(this.warps, "warps")
			.AddField(this.doors, "doors")
			.AddField(this.interiorDoors, "interiorDoors")
			.AddField(this.waterColor, "waterColor")
			.AddField(this.netObjects, "netObjects")
			.AddField(this.projectiles, "projectiles")
			.AddField(this.largeTerrainFeatures, "largeTerrainFeatures")
			.AddField(this.terrainFeatures, "terrainFeatures")
			.AddField(this.characters, "characters")
			.AddField(this.debris, "debris")
			.AddField(this.netAudio.NetFields, "netAudio.NetFields")
			.AddField(this.removeTemporarySpritesWithIDEvent, "removeTemporarySpritesWithIDEvent")
			.AddField(this.rumbleAndFadeEvent, "rumbleAndFadeEvent")
			.AddField(this.damagePlayersEvent, "damagePlayersEvent")
			.AddField(this.lightGlows, "lightGlows")
			.AddField(this.fishSplashPoint, "fishSplashPoint")
			.AddField(this.fishFrenzyFish, "fishFrenzyFish")
			.AddField(this.orePanPoint, "orePanPoint")
			.AddField(this.isGreenhouse, "isGreenhouse")
			.AddField(this.miniJukeboxCount, "miniJukeboxCount")
			.AddField(this.miniJukeboxTrack, "miniJukeboxTrack")
			.AddField(this.randomMiniJukeboxTrack, "randomMiniJukeboxTrack")
			.AddField(this.resourceClumps, "resourceClumps")
			.AddField(this.isAlwaysActive, "isAlwaysActive")
			.AddField(this.furniture, "furniture")
			.AddField(this.furnitureToRemove.NetFields, "furnitureToRemove.NetFields")
			.AddField(this.parentLocationName, "parentLocationName")
			.AddField(this.buildings, "buildings")
			.AddField(this.animals, "animals")
			.AddField(this.piecesOfHay, "piecesOfHay")
			.AddField(this.mapSeats, "mapSeats")
			.AddField(this.modData, "modData");
		this.mapPath.fieldChangeVisibleEvent += delegate
		{
			this._mapPathDirty = true;
		};
		this.name.fieldChangeVisibleEvent += delegate
		{
			this.OnNameChanged();
		};
		this.uniqueName.fieldChangeVisibleEvent += delegate
		{
			this.OnNameChanged();
		};
		this.parentLocationName.fieldChangeVisibleEvent += delegate
		{
			this.OnParentLocationChanged();
		};
		this.buildings.OnValueAdded += delegate(Building b)
		{
			if (b != null)
			{
				b.parentLocationName.Value = this.NameOrUniqueName;
				b.updateInteriorWarps();
			}
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateBuildingCache(this);
			}
		};
		this.buildings.OnValueRemoved += delegate(Building b)
		{
			if (b != null)
			{
				b.parentLocationName.Value = null;
			}
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateBuildingCache(this);
			}
		};
		this.isStructure.fieldChangeVisibleEvent += delegate
		{
			if (this.mapPath.Value != null)
			{
				this.InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
				this.reloadMap();
			}
		};
		this.sharedLights.OnValueAdded += delegate(string identifier, LightSource light)
		{
			if (Game1.currentLocation == this)
			{
				Game1.currentLightSources.Add(light);
			}
		};
		this.sharedLights.OnValueRemoved += delegate(string identifier, LightSource light)
		{
			if (Game1.currentLocation == this)
			{
				Game1.currentLightSources.Remove(light?.Id);
			}
		};
		this.netObjects.OnConflictResolve += delegate(Vector2 pos, NetRef<Object> rejected, NetRef<Object> accepted)
		{
			if (Game1.IsMasterGame)
			{
				Object value = rejected.Value;
				if (value != null)
				{
					value.onDetachedFromParent();
					value.dropItem(this, pos * 64f, pos * 64f);
				}
			}
		};
		this.netObjects.OnValueAdded += OnObjectAdded;
		this.overlayObjects.onValueAdded += OnObjectAdded;
		this.removeTemporarySpritesWithIDEvent.onEvent += removeTemporarySpritesWithIDLocal;
		this.rumbleAndFadeEvent.onEvent += performRumbleAndFade;
		this.damagePlayersEvent.onEvent += performDamagePlayers;
		this.fishSplashPoint.fieldChangeVisibleEvent += delegate
		{
			this.updateFishSplashAnimation();
		};
		this.orePanPoint.fieldChangeVisibleEvent += delegate
		{
			this.updateOrePanAnimation();
		};
		this.characters.OnValueRemoved += delegate(NPC npc)
		{
			npc.Removed();
		};
		this.terrainFeatures.OnValueAdded += delegate(Vector2 tile, TerrainFeature feature)
		{
			this.OnTerrainFeatureAdded(feature, tile);
		};
		this.terrainFeatures.OnValueRemoved += delegate(Vector2 tile, TerrainFeature feature)
		{
			this.OnTerrainFeatureRemoved(feature);
		};
		this.largeTerrainFeatures.OnValueAdded += delegate(LargeTerrainFeature feature)
		{
			this.OnTerrainFeatureAdded(feature, feature.Tile);
		};
		this.largeTerrainFeatures.OnValueRemoved += OnTerrainFeatureRemoved;
		this.resourceClumps.OnValueAdded += OnResourceClumpAdded;
		this.resourceClumps.OnValueRemoved += OnResourceClumpRemoved;
		this.furniture.OnValueAdded += delegate(Furniture f)
		{
			f.Location = this;
			f.OnAdded(this, f.TileLocation);
		};
		this.furniture.OnValueRemoved += delegate(Furniture f)
		{
			f.OnRemoved(this, f.TileLocation);
		};
		this.furnitureToRemove.Processor = removeQueuedFurniture;
	}

	public virtual void InvalidateCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
	{
		if (!Game1.IsMasterGame)
		{
			cached_data.Remove(this.NameOrUniqueName);
		}
	}

	public virtual void MakeMapModifications(bool force = false)
	{
		if (force)
		{
			this._appliedMapOverrides.Clear();
		}
		this.interiorDoors.MakeMapModifications();
		string value = this.name.Value;
		if (value == null)
		{
			return;
		}
		switch (value.Length)
		{
		case 10:
			switch (value[0])
			{
			case 'W':
				if (value == "WitchSwamp")
				{
					if (Game1.MasterPlayer.mailReceived.Contains("henchmanGone"))
					{
						this.removeTile(20, 29, "Buildings");
					}
					else
					{
						this.setMapTile(20, 29, 10, "Buildings", "wt");
					}
				}
				break;
			case 'H':
				if (value == "HaleyHouse" && Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse != "Emily")
				{
					this.setMapTile(14, 4, 2173, "Buildings", "1");
					this.setMapTile(14, 3, 2141, "Buildings", "1");
					this.setMapTile(14, 3, 219, "Back", "1");
				}
				break;
			}
			break;
		case 9:
			switch (value[0])
			{
			case 'B':
				if (!(value == "Backwoods"))
				{
					break;
				}
				if (Game1.netWorldState.Value.hasWorldStateID("golemGrave"))
				{
					this.ApplyMapOverride("Backwoods_GraveSite");
				}
				if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts") && !this._appliedMapOverrides.Contains("Backwoods_Staircase"))
				{
					this.ApplyMapOverride("Backwoods_Staircase");
					LargeTerrainFeature blockingBush = null;
					foreach (LargeTerrainFeature t in this.largeTerrainFeatures)
					{
						if (t.Tile == new Vector2(37f, 16f))
						{
							blockingBush = t;
							break;
						}
					}
					if (blockingBush != null)
					{
						this.largeTerrainFeatures.Remove(blockingBush);
					}
				}
				if (!Game1.player.mailReceived.Contains("asdlkjfg1") || Game1.random.NextDouble() < 0.01)
				{
					this.setTileProperty(13, 29, "Back", "TouchAction", "asdlfkjg");
					this.setTileProperty(14, 29, "Back", "TouchAction", "asdlfkjg");
					this.setTileProperty(15, 29, "Back", "TouchAction", "asdlfkjg");
				}
				else if (Utility.doesAnyFarmerHaveMail("asdlkjfg1") && Utility.CreateDaySaveRandom(1244.0).NextDouble() < 0.02)
				{
					if (!this.IsTileOccupiedBy(new Vector2(13f, 26f)))
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(13f, 26f) * 64f, flipped: false, 0.003f, Color.White)
						{
							scale = 4f,
							layerDepth = 0f
						});
					}
					if (!this.IsTileOccupiedBy(new Vector2(12f, 25f)))
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(12f, 25f) * 64f, flipped: true, 0.003f, Color.White)
						{
							scale = 4f,
							layerDepth = 0f
						});
					}
					if (!this.IsTileOccupiedBy(new Vector2(13f, 24f)))
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(13f, 24f) * 64f, flipped: false, 0.003f, Color.White)
						{
							scale = 4f,
							layerDepth = 0f
						});
					}
					if (!this.IsTileOccupiedBy(new Vector2(13f, 23f)))
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(12f, 23f) * 64f, flipped: true, 0.003f, Color.White * 0.66f)
						{
							scale = 4f,
							layerDepth = 0f
						});
					}
					if (!this.IsTileOccupiedBy(new Vector2(13f, 22f)))
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(495, 412, 16, 16), new Vector2(13f, 22f) * 64f, flipped: false, 0.003f, Color.White * 0.33f)
						{
							scale = 4f,
							layerDepth = 0f
						});
					}
				}
				if (Game1.timeOfDay >= 2400)
				{
					Random asdfaTime = Utility.CreateDaySaveRandom(124.0);
					int time = Utility.ModifyTime(2400, asdfaTime.Next(12) * 10);
					if (Game1.timeOfDay == time && asdfaTime.NextDouble() < 0.33)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 48, 32, 48), new Vector2(60f, -260f), flipped: true, 0f, Color.White)
						{
							animationLength = 8,
							totalNumberOfLoops = 99,
							interval = 120f,
							scale = 4f,
							motion = new Vector2(0.5f, 1f),
							yStopCoordinate = 256,
							xStopCoordinate = 256,
							delayBeforeAnimationStart = 1000
						});
					}
				}
				break;
			case 'S':
				if (value == "SkullCave")
				{
					bool showShrineActivated = Game1.player.team.skullShrineActivated.Value || Game1.player.team.SpecialOrderRuleActive("SC_HARD");
					if (Game1.player.team.toggleSkullShrineOvernight.Value)
					{
						showShrineActivated = !showShrineActivated;
					}
					if (showShrineActivated)
					{
						this._appliedMapOverrides.Remove("SkullCaveAltarDeactivated");
						this.ApplyMapOverride("SkullCaveAltar", new Microsoft.Xna.Framework.Rectangle(0, 0, 5, 4), new Microsoft.Xna.Framework.Rectangle(10, 1, 5, 4));
						Game1.currentLightSources.Add(new LightSource("SkullCaveAltar", 4, new Vector2(12f, 3f) * 64f, 1f, LightSource.LightContext.MapLight, 0L, this.NameOrUniqueName));
						AmbientLocationSounds.addSound(new Vector2(12f, 3f), 1);
					}
					else
					{
						this._appliedMapOverrides.Remove("SkullCaveAltar");
						this.ApplyMapOverride(Game1.temporaryContent.Load<Map>("Maps\\SkullCave"), "SkullCaveAltarDeactivated", new Microsoft.Xna.Framework.Rectangle(10, 1, 5, 4), new Microsoft.Xna.Framework.Rectangle(10, 1, 5, 4));
						Game1.currentLightSources.Remove("SkullCaveAltar");
						AmbientLocationSounds.removeSound(new Vector2(12f, 3f));
					}
				}
				break;
			}
			break;
		case 16:
			if (value == "IslandNorthCave1" && Game1.player.mailReceived.Contains("FizzIntro"))
			{
				if (this.getCharacterFromName("Fizz") == null)
				{
					this.characters.Add(new NPC(new AnimatedSprite("Characters\\Fizz", 0, 16, 32), new Vector2(6f, 3f) * 64f, 2, "Fizz")
					{
						SimpleNonVillagerNPC = true,
						Portrait = Game1.content.Load<Texture2D>("Portraits\\Fizz"),
						displayName = Game1.content.LoadString("Strings\\NPCNames:Fizz")
					});
					this.removeObjectsAndSpawned(6, 3, 1, 1);
				}
				else
				{
					this.getCharacterFromName("Fizz").SimpleNonVillagerNPC = true;
					this.getCharacterFromName("Fizz").Sprite.SpriteHeight = 32;
					this.getCharacterFromName("Fizz").Sprite.UpdateSourceRect();
				}
				Game1.currentLightSources.Add(new LightSource("IslandNorthCave1", 1, new Vector2(6f, 3f) * 64f + new Vector2(32f), 2f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
			}
			break;
		case 11:
			if (value == "MasteryCave")
			{
				Game1.stats.Get("MasteryExp");
				int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
				int levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");
				ShowSkillMastery(4, new Vector2(54f, 98f));
				ShowSkillMastery(2, new Vector2(84f, 82f));
				ShowSkillMastery(0, new Vector2(116f, 82f));
				ShowSkillMastery(0, new Vector2(116f, 82f));
				ShowSkillMastery(1, new Vector2(148f, 82f));
				ShowSkillMastery(3, new Vector2(179f, 98f));
				if (MasteryTrackerMenu.hasCompletedAllMasteryPlaques())
				{
					MasteryTrackerMenu.addSpiritCandles(instant: true);
					Game1.changeMusicTrack("grandpas_theme");
				}
			}
			break;
		case 19:
			if (value == "WizardHouseBasement" && Game1.player.mailReceived.Contains("hasActivatedForestPylon"))
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 106, 14, 22), new Vector2(16.6f, 2.5f) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 8,
					interval = 100f,
					totalNumberOfLoops = 9999,
					scale = 4f
				});
			}
			break;
		case 7:
			if (value == "Sunroom")
			{
				TileSheet tileSheet = this.map.RequireTileSheet(1, "2");
				string imageDir = Path.GetDirectoryName(tileSheet.ImageSource);
				if (string.IsNullOrWhiteSpace(imageDir))
				{
					imageDir = "Maps";
				}
				tileSheet.ImageSource = Path.Combine(imageDir, "CarolineGreenhouseTiles" + ((this.IsRainingHere() || Game1.timeOfDay > Game1.getTrulyDarkTime(this)) ? "_rainy" : ""));
				this.map.DisposeTileSheets(Game1.mapDisplayDevice);
				this.map.LoadTileSheets(Game1.mapDisplayDevice);
			}
			break;
		case 17:
			if (value == "AbandonedJojaMart")
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					StaticTile[] tileFrames = CommunityCenter.getJunimoNoteTileFrames(0, this.map);
					string layer = "Buildings";
					Point position = new Point(8, 8);
					this.map.RequireLayer(layer).Tiles[position.X, position.Y] = new AnimatedTile(this.map.RequireLayer(layer), tileFrames, 70L);
				}
				else
				{
					this.removeTile(8, 8, "Buildings");
				}
			}
			break;
		case 8:
			if (value == "WitchHut" && Game1.player.mailReceived.Contains("hasPickedUpMagicInk"))
			{
				this.setMapTile(4, 11, 113, "Buildings", "untitled tile sheet").Properties.Remove("Action");
			}
			break;
		case 6:
			if (value == "Saloon" && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
			{
				this.ApplyMapOverride("RefurbishedSaloonRoom", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(32, 1, 6, 8));
				Game1.currentLightSources.Add(new LightSource("Saloon_1", 1, new Vector2(33f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("Saloon_2", 1, new Vector2(36f, 7f) * 64f, 4f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("Saloon_3", 1, new Vector2(34f, 5f) * 64f, 4f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
			}
			break;
		case 12:
		case 13:
		case 14:
		case 15:
		case 18:
			break;
		}
	}

	public virtual bool ApplyCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data, string requested_map_path)
	{
		if (Game1.IsMasterGame)
		{
			return false;
		}
		if (cached_data.TryGetValue(this.NameOrUniqueName, out var data))
		{
			if (data.mapPath == requested_map_path)
			{
				this._appliedMapOverrides = data.appliedMapOverrides;
				this.map = data.map;
				this.loadedMapPath = data.loadedMapPath;
				return true;
			}
			cached_data.Remove(this.NameOrUniqueName);
			return false;
		}
		return false;
	}

	public virtual void StoreCachedMultiplayerMap(Dictionary<string, CachedMultiplayerMap> cached_data)
	{
		if (!Game1.IsMasterGame && !(this is VolcanoDungeon) && !(this is MineShaft))
		{
			CachedMultiplayerMap data = new CachedMultiplayerMap();
			data.map = this.map;
			data.appliedMapOverrides = this._appliedMapOverrides;
			data.mapPath = this.mapPath.Value;
			data.loadedMapPath = this.loadedMapPath;
			cached_data[this.NameOrUniqueName] = data;
		}
	}

	public virtual void TransferDataFromSavedLocation(GameLocation l)
	{
		this.modData.Clear();
		if (l.modData != null)
		{
			foreach (string key in l.modData.Keys)
			{
				this.modData[key] = l.modData[key];
			}
		}
		this.miniJukeboxCount.Value = l.miniJukeboxCount.Value;
		this.miniJukeboxTrack.Value = l.miniJukeboxTrack.Value;
		this.SelectRandomMiniJukeboxTrack();
		this.UpdateMapSeats();
	}

	/// <summary>Reset cached data when the name or unique name changes.</summary>
	private void OnNameChanged()
	{
		this.IsTemporary = GameLocation.IsTemporaryName(this.Name);
	}

	/// <summary>Reset cached data when the parent location changes.</summary>
	private void OnParentLocationChanged()
	{
		this.locationContextId = null;
		if (this.seasonOverride == null || this.seasonOverride.IsValueCreated)
		{
			this.seasonOverride = new Lazy<Season?>(LoadSeasonOverride);
		}
	}

	/// <summary>Update when the building containing this location is upgraded, if applicable.</summary>
	/// <param name="building">The building containing this location.</param>
	public virtual void OnParentBuildingUpgraded(Building building)
	{
	}

	/// <summary>Update when this location is removed from the game (e.g. a mine level that was unloaded).</summary>
	public virtual void OnRemoved()
	{
		for (int i = this.characters.Count - 1; i >= 0; i--)
		{
			this.characters[i].OnLocationRemoved();
		}
	}

	/// <summary>Handle an object added to the location.</summary>
	/// <param name="tile">The tile position.</param>
	/// <param name="obj">The object that was added.</param>
	protected virtual void OnObjectAdded(Vector2 tile, Object obj)
	{
		obj.Location = this;
		obj.TileLocation = tile;
	}

	/// <summary>Handle a resource clump added to the location.</summary>
	/// <param name="obj">The resource clump that was added.</param>
	public virtual void OnResourceClumpAdded(ResourceClump resourceClump)
	{
		resourceClump.Location = this;
		resourceClump.OnAddedToLocation(this, resourceClump.Tile);
	}

	/// <summary>Handle a resource clump removed from the location.</summary>
	/// <param name="tile">The tile position.</param>
	/// <param name="obj">The resource clump that was removed.</param>
	public virtual void OnResourceClumpRemoved(ResourceClump resourceClump)
	{
		resourceClump.Location = null;
	}

	/// <summary>Handle a terrain feature added to the location.</summary>
	/// <param name="tile">The tile position.</param>
	/// <param name="obj">The terrain feature that was added.</param>
	public virtual void OnTerrainFeatureAdded(TerrainFeature feature, Vector2 location)
	{
		if (feature == null)
		{
			return;
		}
		if (!(feature is Flooring flooring))
		{
			if (feature is HoeDirt dirt)
			{
				dirt.OnAdded(this, location);
			}
		}
		else
		{
			flooring.OnAdded(this, location);
		}
		feature.Location = this;
		feature.Tile = location;
		feature.OnAddedToLocation(this, location);
		this.UpdateTerrainFeatureUpdateSubscription(feature);
	}

	/// <summary>Handle a terrain feature removed from the location.</summary>
	/// <param name="tile">The tile position.</param>
	/// <param name="obj">The terrain feature that was removed.</param>
	public virtual void OnTerrainFeatureRemoved(TerrainFeature feature)
	{
		if (feature == null)
		{
			return;
		}
		if (!(feature is Flooring flooring))
		{
			if (!(feature is HoeDirt dirt))
			{
				if (feature is LargeTerrainFeature largeFeature)
				{
					largeFeature.onDestroy();
				}
			}
			else
			{
				dirt.OnRemoved();
			}
		}
		else
		{
			flooring.OnRemoved();
		}
		if (feature.NeedsUpdate)
		{
			this._activeTerrainFeatures.Remove(feature);
		}
		feature.Location = null;
	}

	public virtual void UpdateTerrainFeatureUpdateSubscription(TerrainFeature feature)
	{
		if (feature.NeedsUpdate)
		{
			this._activeTerrainFeatures.Add(feature);
		}
		else
		{
			this._activeTerrainFeatures.Remove(feature);
		}
	}

	/// <summary>Get the season which currently applies to this location as a numeric index.</summary>
	/// <remarks>Most code should use <see cref="M:StardewValley.GameLocation.GetSeason" /> instead.</remarks>
	public int GetSeasonIndex()
	{
		return (int)this.GetSeason();
	}

	/// <summary>Read the override season from the map or location context.</summary>
	private Season? LoadSeasonOverride()
	{
		if (this.map == null && this.mapPath.Value != null)
		{
			this.reloadMap();
		}
		if (this.map != null && this.map.Properties.TryGetValue("SeasonOverride", out var propertyValue) && !string.IsNullOrWhiteSpace(propertyValue))
		{
			if (Utility.TryParseEnum<Season>(propertyValue, out var season))
			{
				return season;
			}
			Game1.log.Error($"Unable to read SeasonOverride map property value '{propertyValue}' for location '{this.NameOrUniqueName}', not a valid season name.");
		}
		return this.GetLocationContext()?.SeasonOverride;
	}

	/// <summary>Get the season which currently applies to this location.</summary>
	public Season GetSeason()
	{
		return this.seasonOverride.Value ?? this.GetParentLocation()?.GetSeason() ?? Game1.season;
	}

	/// <summary>Get the season which currently applies to this location as a string.</summary>
	/// <remarks>Most code should use <see cref="M:StardewValley.GameLocation.GetSeason" /> instead.</remarks>
	public string GetSeasonKey()
	{
		return Utility.getSeasonKey(this.GetSeason());
	}

	/// <summary>Get whether it's spring in this location's context.</summary>
	/// <remarks>This is a shortcut for convenience. When checking multiple season, consider caching the result from <see cref="M:StardewValley.GameLocation.GetSeason" /> instead.</remarks>
	public bool IsSpringHere()
	{
		return this.GetSeason() == Season.Spring;
	}

	/// <summary>Get whether it's summer in this location's context.</summary>
	/// <inheritdoc cref="M:StardewValley.GameLocation.IsSpringHere" path="/remarks" />
	public bool IsSummerHere()
	{
		return this.GetSeason() == Season.Summer;
	}

	/// <summary>Get whether it's fall in this location's context.</summary>
	/// <inheritdoc cref="M:StardewValley.GameLocation.IsSpringHere" path="/remarks" />
	public bool IsFallHere()
	{
		return this.GetSeason() == Season.Fall;
	}

	/// <summary>Get whether it's winter in this location's context.</summary>
	/// <inheritdoc cref="M:StardewValley.GameLocation.IsSpringHere" path="/remarks" />
	public bool IsWinterHere()
	{
		return this.GetSeason() == Season.Winter;
	}

	/// <summary>Get the weather which applies in this location's context.</summary>
	public LocationWeather GetWeather()
	{
		return Game1.netWorldState.Value.GetWeatherForLocation(this.GetLocationContextId());
	}

	/// <summary>Get whether it's raining in this location's context (regardless of whether the player is currently indoors and sheltered from the rain).</summary>
	public bool IsRainingHere()
	{
		return this.GetWeather().IsRaining;
	}

	/// <summary>Get whether it's green raining in this location's context (regardless of whether the player is currently indoors and sheltered from the green rain).</summary>
	public bool IsGreenRainingHere()
	{
		if (this.IsRainingHere())
		{
			return this.GetWeather().IsGreenRain;
		}
		return false;
	}

	/// <summary>Get whether it's storming in this location's context (regardless of whether the player is currently indoors and sheltered from the storm).</summary>
	public bool IsLightningHere()
	{
		return this.GetWeather().IsLightning;
	}

	/// <summary>Get whether it's snowing in this location's context (regardless of whether the player is currently indoors and sheltered from the snow).</summary>
	public bool IsSnowingHere()
	{
		return this.GetWeather().IsSnowing;
	}

	/// <summary>Get whether it's blowing debris like leaves in this location's context (regardless of whether the player is currently indoors and sheltered from the wind).</summary>
	public bool IsDebrisWeatherHere()
	{
		return this.GetWeather().IsDebrisWeather;
	}

	/// <summary>Get whether a location name matches the pattern used by temporary locations for events or minigames.</summary>
	/// <param name="name">The location name to check.</param>
	public static bool IsTemporaryName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (!name.StartsWith("Temp", StringComparison.Ordinal) && !(name == "fishingGame"))
		{
			return name == "tent";
		}
		return true;
	}

	private void updateFishSplashAnimation()
	{
		if (this.fishSplashPoint.Value == Point.Zero)
		{
			this.fishSplashAnimation = null;
			return;
		}
		this.fishSplashAnimation = new TemporaryAnimatedSprite(51, new Vector2(this.fishSplashPoint.X * 64, this.fishSplashPoint.Y * 64), Color.White, 10, flipped: false, 80f, 999999)
		{
			layerDepth = (float)(this.fishSplashPoint.Y * 64 - 64 - 1) / 10000f
		};
	}

	private void updateOrePanAnimation()
	{
		if (this.orePanPoint.Value == Point.Zero)
		{
			this.orePanAnimation = null;
			return;
		}
		this.orePanAnimation = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), new Vector2(this.orePanPoint.X * 64 + 32, this.orePanPoint.Y * 64 + 32), flipped: false, 0f, Color.White)
		{
			totalNumberOfLoops = 9999999,
			interval = 100f,
			scale = 3f,
			animationLength = 6
		};
	}

	public GameLocation()
	{
		this.NetFields = new NetFields(NetFields.GetNameForInstance(this));
		this.farmers = new FarmerCollection(this);
		this.interiorDoors = new InteriorDoorDictionary(this);
		this.netAudio = new NetAudio(this);
		this.objects = new OverlaidDictionary(this.netObjects, this.overlayObjects);
		this._appliedMapOverrides = new HashSet<string>();
		this.terrainFeatures.SetEqualityComparer(GameLocation.tilePositionComparer);
		this.netObjects.SetEqualityComparer(GameLocation.tilePositionComparer);
		this.objects.SetEqualityComparer(GameLocation.tilePositionComparer, ref this.netObjects, ref this.overlayObjects);
		this.seasonOverride = new Lazy<Season?>(LoadSeasonOverride);
		this.initNetFields();
	}

	public GameLocation(string mapPath, string name)
		: this()
	{
		this.mapPath.Set(mapPath);
		this.name.Value = name;
		if (name.Contains("Farm") || name.Contains("Coop") || name.Contains("Barn") || name.Equals("SlimeHutch"))
		{
			this.isFarm.Value = true;
		}
		if (name == "Greenhouse")
		{
			this.IsGreenhouse = true;
		}
		this.reloadMap();
		this.loadObjects();
	}

	/// <summary>Add the default buildings which should always exist on the farm, if missing.</summary>
	/// <param name="load">Whether to call <see cref="M:StardewValley.Buildings.Building.load" />. This should be true unless you'll be calling it separately.</param>
	public virtual void AddDefaultBuildings(bool load = true)
	{
	}

	/// <summary>Add a default building which should always exist on the farm, if it's missing.</summary>
	/// <param name="id">The building ID in <c>Data/Buildings</c>.</param>
	/// <param name="tile">The tile position at which to construct it.</param>
	/// <param name="load">Whether to call <see cref="M:StardewValley.Buildings.Building.load" />. This should be true unless you'll be calling it separately.</param>
	public virtual void AddDefaultBuilding(string id, Vector2 tile, bool load = true)
	{
		foreach (Building building2 in this.buildings)
		{
			if (building2.buildingType.Value == id)
			{
				return;
			}
		}
		Building building = Building.CreateInstanceFromId(id, tile);
		if (load)
		{
			building.load();
		}
		this.buildings.Add(building);
	}

	/// <summary>Play a sound for each online player in the location if they can hear it.</summary>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="position">The tile position from which to play the sound, or <c>null</c> if it should be played throughout the location.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> to keep it as-is.</param>
	/// <param name="context">The source which triggered the sound.</param>
	public void playSound(string audioName, Vector2? position = null, int? pitch = null, SoundContext context = SoundContext.Default)
	{
		Game1.sounds.PlayAll(audioName, this, position, pitch, context);
	}

	/// <summary>Play a sound for the current player only, if they can hear it.</summary>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="position">The tile position from which to play the sound, or <c>null</c> if not applicable.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> to keep it as-is.</param>
	/// <param name="context">The source which triggered the sound.</param>
	public void localSound(string audioName, Vector2? position = null, int? pitch = null, SoundContext context = SoundContext.Default)
	{
		Game1.sounds.PlayLocal(audioName, this, position, pitch, context, out var _);
	}

	protected virtual LocalizedContentManager getMapLoader()
	{
		if (this.isStructure.Value)
		{
			if (this._structureMapLoader == null)
			{
				this._structureMapLoader = Game1.game1.xTileContent.CreateTemporary();
			}
			return this._structureMapLoader;
		}
		return Game1.game1.xTileContent;
	}

	/// <summary>Destroy any organic material like weeds or twigs, and send any player items to the lost and found. Used to clean up areas before map overrides.</summary>
	/// <param name="tile">The tile position to clean up.</param>
	public void cleanUpTileForMapOverride(Point tile)
	{
		this.cleanUpTileForMapOverride(tile, null);
	}

	/// <summary>Destroy any organic material like weeds or twigs, and send any player items to the lost and found. Used to clean up areas before map overrides.</summary>
	/// <param name="tile">The tile position to clean up.</param>
	/// <param name="exceptItemId">If set, an item on this spot won't be moved if its item ID matches this one.</param>
	public void cleanUpTileForMapOverride(Point tile, string exceptItemId)
	{
		Vector2 tileVector = Utility.PointToVector2(tile);
		Point tileCenterPoint = Utility.Vector2ToPoint(tileVector * new Vector2(64f) + new Vector2(32f, 32f));
		NetCollection<Item> lostAndFound = Game1.player.team.returnedDonations;
		if (this.Objects.TryGetValue(tileVector, out var o) && (exceptItemId == null || !ItemRegistry.HasItemId(o, exceptItemId)))
		{
			if (o != null && (o.HasBeenInInventory || (!o.isDebrisOrForage() && o.QualifiedItemId != "(O)590" && o.QualifiedItemId != "(O)SeedSpot")))
			{
				if (o is Chest chest)
				{
					foreach (Item i in chest.Items)
					{
						lostAndFound.Add(i);
					}
					chest.Items.Clear();
				}
				else if (o.readyForHarvest.Value && o.heldObject != null)
				{
					lostAndFound.Add(o.heldObject.Value);
					o.heldObject.Value = null;
				}
				lostAndFound.Add(o);
				Game1.player.team.newLostAndFoundItems.Value = true;
			}
			this.objects.Remove(tileVector);
		}
		this.furniture.RemoveWhere(delegate(Furniture item)
		{
			if (!item.GetBoundingBox().Contains(tileCenterPoint) || (exceptItemId != null && ItemRegistry.HasItemId(item, exceptItemId)))
			{
				return false;
			}
			if (item.heldObject.Value != null)
			{
				lostAndFound.Add(item.heldObject.Value);
				item.heldObject.Value = null;
			}
			lostAndFound.Add(item);
			return true;
		});
		this.terrainFeatures.Remove(tileVector);
		this.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature.getBoundingBox().Contains(tileCenterPoint));
		this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.getBoundingBox().Contains(tileCenterPoint));
	}

	public void ApplyMapOverride(Map override_map, string override_key, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? dest_rect = null, Action<Point> perTileCustomAction = null)
	{
		if (this._appliedMapOverrides.Contains(override_key))
		{
			return;
		}
		this._appliedMapOverrides.Add(override_key);
		this.updateSeasonalTileSheets(override_map);
		Dictionary<TileSheet, TileSheet> tilesheet_lookup = new Dictionary<TileSheet, TileSheet>();
		foreach (TileSheet override_tile_sheet in override_map.TileSheets)
		{
			TileSheet map_tilesheet = this.map.GetTileSheet(override_tile_sheet.Id);
			string source_image_source = "";
			string dest_image_source = "";
			if (map_tilesheet != null)
			{
				source_image_source = map_tilesheet.ImageSource;
			}
			if (dest_image_source != null)
			{
				dest_image_source = override_tile_sheet.ImageSource;
			}
			if (map_tilesheet == null || dest_image_source != source_image_source)
			{
				map_tilesheet = new TileSheet(GameLocation.GetAddedMapOverrideTilesheetId(override_key, override_tile_sheet.Id), this.map, override_tile_sheet.ImageSource, override_tile_sheet.SheetSize, override_tile_sheet.TileSize);
				for (int i = 0; i < override_tile_sheet.TileCount; i++)
				{
					map_tilesheet.TileIndexProperties[i].CopyFrom(override_tile_sheet.TileIndexProperties[i]);
				}
				this.map.AddTileSheet(map_tilesheet);
			}
			else if (map_tilesheet.TileCount < override_tile_sheet.TileCount)
			{
				int tileCount = map_tilesheet.TileCount;
				map_tilesheet.SheetWidth = override_tile_sheet.SheetWidth;
				map_tilesheet.SheetHeight = override_tile_sheet.SheetHeight;
				for (int j = tileCount; j < override_tile_sheet.TileCount; j++)
				{
					map_tilesheet.TileIndexProperties[j].CopyFrom(override_tile_sheet.TileIndexProperties[j]);
				}
			}
			tilesheet_lookup[override_tile_sheet] = map_tilesheet;
		}
		Dictionary<Layer, Layer> layer_lookup = new Dictionary<Layer, Layer>();
		int map_width = 0;
		int map_height = 0;
		for (int layer_index = 0; layer_index < override_map.Layers.Count; layer_index++)
		{
			map_width = Math.Max(map_width, override_map.Layers[layer_index].LayerWidth);
			map_height = Math.Max(map_height, override_map.Layers[layer_index].LayerHeight);
		}
		if (!source_rect.HasValue)
		{
			source_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, map_width, map_height);
		}
		map_width = 0;
		map_height = 0;
		for (int k = 0; k < this.map.Layers.Count; k++)
		{
			map_width = Math.Max(map_width, this.map.Layers[k].LayerWidth);
			map_height = Math.Max(map_height, this.map.Layers[k].LayerHeight);
		}
		bool layersDirty = false;
		for (int l = 0; l < override_map.Layers.Count; l++)
		{
			Layer original_layer = this.map.GetLayer(override_map.Layers[l].Id);
			if (original_layer == null)
			{
				original_layer = new Layer(override_map.Layers[l].Id, this.map, new Size(map_width, map_height), override_map.Layers[l].TileSize);
				this.map.AddLayer(original_layer);
				layersDirty = true;
			}
			layer_lookup[override_map.Layers[l]] = original_layer;
		}
		if (layersDirty)
		{
			this.SortLayers();
		}
		if (!dest_rect.HasValue)
		{
			dest_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, map_width, map_height);
		}
		int source_rect_x = source_rect.Value.X;
		int source_rect_y = source_rect.Value.Y;
		int dest_rect_x = dest_rect.Value.X;
		int dest_rect_y = dest_rect.Value.Y;
		for (int x = 0; x < source_rect.Value.Width; x++)
		{
			for (int y = 0; y < source_rect.Value.Height; y++)
			{
				Point source_tile_pos = new Point(source_rect_x + x, source_rect_y + y);
				Point dest_tile_pos = new Point(dest_rect_x + x, dest_rect_y + y);
				perTileCustomAction?.Invoke(dest_tile_pos);
				bool lower_layer_overridden = false;
				for (int m = 0; m < override_map.Layers.Count; m++)
				{
					Layer override_layer = override_map.Layers[m];
					Layer target_layer = layer_lookup[override_layer];
					if (target_layer == null || dest_tile_pos.X >= target_layer.LayerWidth || dest_tile_pos.Y >= target_layer.LayerHeight || (!lower_layer_overridden && override_map.Layers[m].Tiles[source_tile_pos.X, source_tile_pos.Y] == null))
					{
						continue;
					}
					lower_layer_overridden = true;
					if (source_tile_pos.X >= override_layer.LayerWidth || source_tile_pos.Y >= override_layer.LayerHeight)
					{
						continue;
					}
					if (override_layer.Tiles[source_tile_pos.X, source_tile_pos.Y] == null)
					{
						target_layer.Tiles[dest_tile_pos.X, dest_tile_pos.Y] = null;
						continue;
					}
					Tile override_tile = override_layer.Tiles[source_tile_pos.X, source_tile_pos.Y];
					Tile new_tile = null;
					if (!(override_tile is StaticTile))
					{
						if (override_tile is AnimatedTile override_animated_tile)
						{
							StaticTile[] tiles = new StaticTile[override_animated_tile.TileFrames.Length];
							for (int n = 0; n < override_animated_tile.TileFrames.Length; n++)
							{
								StaticTile frame_tile = override_animated_tile.TileFrames[n];
								tiles[n] = new StaticTile(target_layer, tilesheet_lookup[frame_tile.TileSheet], frame_tile.BlendMode, frame_tile.TileIndex);
							}
							new_tile = new AnimatedTile(target_layer, tiles, override_animated_tile.FrameInterval);
						}
					}
					else
					{
						new_tile = new StaticTile(target_layer, tilesheet_lookup[override_tile.TileSheet], override_tile.BlendMode, override_tile.TileIndex);
					}
					new_tile?.Properties.CopyFrom(override_tile.Properties);
					target_layer.Tiles[dest_tile_pos.X, dest_tile_pos.Y] = new_tile;
				}
			}
		}
		this.map.LoadTileSheets(Game1.mapDisplayDevice);
		if (Game1.IsMasterGame || this.IsTemporary)
		{
			this._mapSeatsDirty = true;
		}
	}

	/// <summary>Get the generated tilesheet ID for a tilesheet added to the map via <see cref="M:StardewValley.GameLocation.ApplyMapOverride(xTile.Map,System.String,System.Nullable{Microsoft.Xna.Framework.Rectangle},System.Nullable{Microsoft.Xna.Framework.Rectangle},System.Action{Microsoft.Xna.Framework.Point})" />.</summary>
	/// <param name="overrideKey">The map override ID.</param>
	/// <param name="tilesheetId">The tilesheet ID in the applied override map.</param>
	/// <remarks>Note that this tilesheet ID is only used when adding a new tilesheet to the map. If the tilesheet already exists in the base map with the same asset path, it's reused as-is.</remarks>
	public static string GetAddedMapOverrideTilesheetId(string overrideKey, string tilesheetId)
	{
		return $"{"zzzzz"}_{overrideKey}_{tilesheetId}";
	}

	public virtual bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
	{
		return true;
	}

	public bool hasActiveFireplace()
	{
		for (int i = 0; i < this.furniture.Count; i++)
		{
			if (this.furniture[i].furniture_type.Value == 14 && this.furniture[i].isOn.Value)
			{
				return true;
			}
		}
		return false;
	}

	public void ApplyMapOverride(string map_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
	{
		if (!this._appliedMapOverrides.Contains(map_name))
		{
			Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
			this.ApplyMapOverride(override_map, map_name, source_rect, destination_rect);
		}
	}

	public void ApplyMapOverride(string map_name, string override_key_name, Microsoft.Xna.Framework.Rectangle? source_rect = null, Microsoft.Xna.Framework.Rectangle? destination_rect = null)
	{
		if (!this._appliedMapOverrides.Contains(override_key_name))
		{
			Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_name);
			this.ApplyMapOverride(override_map, override_key_name, source_rect, destination_rect);
		}
	}

	public virtual void UpdateMapSeats()
	{
		this._mapSeatsDirty = false;
		if (!Game1.IsMasterGame && !this.IsTemporary)
		{
			return;
		}
		Dictionary<string, string> base_tilesheet_paths = new Dictionary<string, string>();
		Dictionary<string, string> chair_tile_data = DataLoader.ChairTiles(Game1.content);
		this.mapSeats.Clear();
		Layer buildings_layer = this.map.GetLayer("Buildings");
		if (buildings_layer == null)
		{
			return;
		}
		for (int x = 0; x < buildings_layer.LayerWidth; x++)
		{
			for (int y = 0; y < buildings_layer.LayerHeight; y++)
			{
				Tile tile = buildings_layer.Tiles[x, y];
				if (tile == null)
				{
					continue;
				}
				string path = Path.GetFileNameWithoutExtension(tile.TileSheet.ImageSource);
				if (base_tilesheet_paths.TryGetValue(path, out var overridePath))
				{
					path = overridePath;
				}
				else
				{
					if (path.StartsWith("summer_") || path.StartsWith("winter_") || path.StartsWith("fall_"))
					{
						path = "spring_" + path.Substring(path.IndexOf('_') + 1);
					}
					base_tilesheet_paths[path] = path;
				}
				int tiles_per_row = tile.TileSheet.SheetWidth;
				int tile_x = tile.TileIndex % tiles_per_row;
				int tile_y = tile.TileIndex / tiles_per_row;
				string key = path + "/" + tile_x + "/" + tile_y;
				if (chair_tile_data.TryGetValue(key, out var data))
				{
					MapSeat seat = MapSeat.FromData(data, x, y);
					if (seat != null)
					{
						this.mapSeats.Add(seat);
					}
				}
			}
		}
	}

	public virtual void SortLayers()
	{
		this.backgroundLayers.Clear();
		this.buildingLayers.Clear();
		this.frontLayers.Clear();
		this.alwaysFrontLayers.Clear();
		Dictionary<string, List<KeyValuePair<Layer, int>>> layerNameLookup = new Dictionary<string, List<KeyValuePair<Layer, int>>>();
		layerNameLookup["Back"] = this.backgroundLayers;
		layerNameLookup["Buildings"] = this.buildingLayers;
		layerNameLookup["Front"] = this.frontLayers;
		layerNameLookup["AlwaysFront"] = this.alwaysFrontLayers;
		foreach (Layer layer in this.map.Layers)
		{
			foreach (string key in layerNameLookup.Keys)
			{
				if (layer.Id.StartsWith(key))
				{
					int sortIndex = 0;
					string sortString = layer.Id.Substring(key.Length);
					if (sortString.Length <= 0 || int.TryParse(sortString, out sortIndex))
					{
						layerNameLookup[key].Add(new KeyValuePair<Layer, int>(layer, sortIndex));
						break;
					}
				}
			}
		}
		foreach (List<KeyValuePair<Layer, int>> value in layerNameLookup.Values)
		{
			value.Sort((KeyValuePair<Layer, int> a, KeyValuePair<Layer, int> b) => a.Value.CompareTo(b.Value));
		}
	}

	public virtual void OnMapLoad(Map map)
	{
	}

	public void loadMap(string mapPath, bool force_reload = false)
	{
		if (force_reload)
		{
			LocalizedContentManager loader = Program.gamePtr.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
			this.map = loader.Load<Map>(mapPath);
			loader.Unload();
			this.InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
		}
		else if (!this.ApplyCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps, mapPath))
		{
			this.map = this.getMapLoader().Load<Map>(mapPath);
		}
		this.loadedMapPath = mapPath;
		this.OnMapLoad(this.map);
		this.SortLayers();
		if (this.map.Properties.ContainsKey("Outdoors"))
		{
			this.isOutdoors.Value = true;
		}
		if (this.map.Properties.ContainsKey("IsFarm"))
		{
			this.isFarm.Value = true;
		}
		if (this.map.Properties.ContainsKey("IsGreenhouse"))
		{
			this.isGreenhouse.Value = true;
		}
		if (this.HasMapPropertyWithValue("forceLoadPathLayerLights"))
		{
			this.forceLoadPathLayerLights = true;
		}
		if (this.HasMapPropertyWithValue("TreatAsOutdoors"))
		{
			this.treatAsOutdoors.Value = true;
		}
		this.updateSeasonalTileSheets(this.map);
		this.map.LoadTileSheets(Game1.mapDisplayDevice);
		if (Game1.IsMasterGame || this.IsTemporary)
		{
			this._mapSeatsDirty = true;
		}
		if ((this.isOutdoors.Value || this.HasMapPropertyWithValue("indoorWater") || this is Sewer || this is Submarine) && !(this is Desert))
		{
			this.waterTiles = new WaterTiles(this.map.Layers[0].LayerWidth, this.map.Layers[0].LayerHeight);
			bool foundAnyWater = false;
			for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
				{
					string water_property = this.doesTileHaveProperty(x, y, "Water", "Back");
					if (water_property != null)
					{
						foundAnyWater = true;
						if (water_property == "I")
						{
							this.waterTiles.waterTiles[x, y] = new WaterTiles.WaterTileData(is_water: true, is_visible: false);
						}
						else
						{
							this.waterTiles[x, y] = true;
						}
					}
				}
			}
			if (!foundAnyWater)
			{
				this.waterTiles = null;
			}
		}
		if (this.isOutdoors.Value)
		{
			this.critters = new List<Critter>();
		}
		this.loadLights();
	}

	public virtual void HandleGrassGrowth(int dayOfMonth)
	{
		if (dayOfMonth == 1)
		{
			if (this is Farm || this.HasMapPropertyWithValue("ClearEmptyDirtOnNewMonth"))
			{
				this.terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value is HoeDirt { crop: null } && Game1.random.NextDouble() < 0.8);
			}
			if (this is Farm || this.HasMapPropertyWithValue("SpawnDebrisOnNewMonth"))
			{
				this.spawnWeedsAndStones(20, weedsOnly: false, spawnFromOldWeeds: false);
			}
			if (Game1.IsSpring && Game1.stats.DaysPlayed > 1)
			{
				if (this is Farm || this.HasMapPropertyWithValue("SpawnDebrisOnNewYear"))
				{
					this.spawnWeedsAndStones(40, weedsOnly: false, spawnFromOldWeeds: false);
					this.spawnWeedsAndStones(40, weedsOnly: true, spawnFromOldWeeds: false);
				}
				if (this is Farm || this.HasMapPropertyWithValue("SpawnRandomGrassOnNewYear"))
				{
					for (int i = 0; i < 15; i++)
					{
						int xCoord = Game1.random.Next(this.map.DisplayWidth / 64);
						int yCoord = Game1.random.Next(this.map.DisplayHeight / 64);
						Vector2 location = new Vector2(xCoord, yCoord);
						this.objects.TryGetValue(location, out var o);
						if (o == null && this.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null && !this.IsNoSpawnTile(location) && this.isTileLocationOpen(new Location(xCoord, yCoord)) && !this.IsTileOccupiedBy(location) && !this.isWaterTile(xCoord, yCoord))
						{
							int grassType = 1;
							if (Game1.GetFarmTypeID() == "MeadowlandsFarm" && Game1.random.NextDouble() < 0.2)
							{
								grassType = 7;
							}
							this.terrainFeatures.Add(location, new Grass(grassType, 4));
						}
					}
					this.growWeedGrass(40);
				}
				if (this.HasMapPropertyWithValue("SpawnGrassFromPathsOnNewYear"))
				{
					Layer paths = this.map.GetLayer("Paths");
					if (paths != null)
					{
						for (int x = 0; x < paths.LayerWidth; x++)
						{
							for (int y = 0; y < paths.LayerHeight; y++)
							{
								Vector2 location2 = new Vector2(x, y);
								this.objects.TryGetValue(location2, out var o2);
								if (o2 == null && this.getTileIndexAt(x, y, "Paths") == 22 && this.isTileLocationOpen(location2) && !this.IsTileOccupiedBy(location2))
								{
									this.terrainFeatures.Add(location2, new Grass(1, 4));
								}
							}
						}
					}
				}
			}
		}
		if ((this is Farm || this.HasMapPropertyWithValue("EnableGrassSpread")) && (!this.IsWinterHere() || this.HasMapPropertyWithValue("AllowGrassGrowInWinter")))
		{
			this.growWeedGrass(1);
		}
	}

	public void reloadMap()
	{
		if (this.mapPath.Value != null)
		{
			this.loadMap(this.mapPath.Value);
		}
		else
		{
			this.map = null;
		}
		this.loadedMapPath = this.mapPath.Value;
	}

	public virtual bool canSlimeMateHere()
	{
		return true;
	}

	public virtual bool canSlimeHatchHere()
	{
		return true;
	}

	public void addCharacter(NPC character)
	{
		this.characters.Add(character);
	}

	public static Microsoft.Xna.Framework.Rectangle getSourceRectForObject(int tileIndex)
	{
		return new Microsoft.Xna.Framework.Rectangle(tileIndex * 16 % Game1.objectSpriteSheet.Width, tileIndex * 16 / Game1.objectSpriteSheet.Width * 16, 16, 16);
	}

	public Warp isCollidingWithWarp(Microsoft.Xna.Framework.Rectangle position, Character character)
	{
		if (this.ignoreWarps)
		{
			return null;
		}
		foreach (Warp w in this.warps)
		{
			if ((!(character is NPC) && w.npcOnly.Value) || (w.X != (int)Math.Floor((double)position.Left / 64.0) && w.X != (int)Math.Floor((double)position.Right / 64.0)) || (w.Y != (int)Math.Floor((double)position.Top / 64.0) && w.Y != (int)Math.Floor((double)position.Bottom / 64.0)))
			{
				continue;
			}
			string targetName = w.TargetName;
			if (!(targetName == "BoatTunnel"))
			{
				if (targetName == "VolcanoEntrance")
				{
					return new Warp(w.X, w.Y, VolcanoDungeon.GetLevelName(0), w.TargetX, w.TargetY, flipFarmer: false);
				}
			}
			else if (character is NPC)
			{
				return new Warp(w.X, w.Y, "IslandSouth", 17, 43, flipFarmer: false);
			}
			return w;
		}
		return null;
	}

	public Warp isCollidingWithWarpOrDoor(Microsoft.Xna.Framework.Rectangle position, Character character = null)
	{
		Warp w = this.isCollidingWithWarp(position, character);
		if (w == null)
		{
			w = this.isCollidingWithDoors(position, character);
		}
		return w;
	}

	public virtual Warp isCollidingWithDoors(Microsoft.Xna.Framework.Rectangle position, Character character = null)
	{
		for (int i = 0; i < 4; i++)
		{
			Vector2 v = Utility.getCornersOfThisRectangle(ref position, i);
			Point rectangleCorner = new Point((int)v.X / 64, (int)v.Y / 64);
			foreach (KeyValuePair<Point, string> pair2 in this.doors.Pairs)
			{
				Point door = pair2.Key;
				if (rectangleCorner == door)
				{
					Warp warp = this.getWarpFromDoor(door, character);
					if (warp != null)
					{
						return warp;
					}
				}
			}
			foreach (Building building in this.buildings)
			{
				if (!building.HasIndoors())
				{
					continue;
				}
				Point point = building.getPointForHumanDoor();
				if (rectangleCorner == point)
				{
					Warp warp2 = this.getWarpFromDoor(point, character);
					if (warp2 != null)
					{
						return warp2;
					}
				}
			}
		}
		return null;
	}

	public virtual Warp getWarpFromDoor(Point door, Character character = null)
	{
		foreach (Building building in this.buildings)
		{
			if (door == building.getPointForHumanDoor())
			{
				GameLocation interior = building.GetIndoors();
				if (interior != null)
				{
					return new Warp(door.X, door.Y, interior.NameOrUniqueName, interior.warps[0].X, interior.warps[0].Y - 1, flipFarmer: false);
				}
			}
		}
		string[] split = this.GetTilePropertySplitBySpaces("Action", "Buildings", door.X, door.Y);
		string propertyName = ArgUtility.Get(split, 0, "");
		switch (propertyName)
		{
		case "WarpCommunityCenter":
			return new Warp(door.X, door.Y, "CommunityCenter", 32, 23, flipFarmer: false);
		case "Warp_Sunroom_Door":
			return new Warp(door.X, door.Y, "Sunroom", 5, 13, flipFarmer: false);
		case "WarpBoatTunnel":
			if (!(character is NPC))
			{
				return new Warp(door.X, door.Y, "BoatTunnel", 6, 11, flipFarmer: false);
			}
			return new Warp(door.X, door.Y, "IslandSouth", 17, 43, flipFarmer: false);
		case "WarpMensLocker":
		case "LockedDoorWarp":
		case "Warp":
		case "WarpWomensLocker":
		{
			if (!ArgUtility.TryGetPoint(split, 1, out var tile, out var error, "Point tile") || !ArgUtility.TryGet(split, 3, out var locationName, out error, allowBlank: true, "string locationName"))
			{
				this.LogTileActionError(split, door.X, door.Y, error);
				return null;
			}
			if (!(locationName == "BoatTunnel") || !(character is NPC))
			{
				return new Warp(door.X, door.Y, locationName, tile.X, tile.Y, flipFarmer: false);
			}
			return new Warp(door.X, door.Y, "IslandSouth", 17, 43, flipFarmer: false);
		}
		default:
			if (propertyName.Contains("Warp"))
			{
				Game1.log.Warn($"Door in {this.NameOrUniqueName} ({door}) has unknown warp property '{string.Join(" ", split)}', parsing with legacy logic.");
				goto case "WarpMensLocker";
			}
			return null;
		}
	}

	/// <summary>Get the first warp which the player can use to leave the location, accounting for any gender restrictions and NPC-only flags if possible.</summary>
	public Warp GetFirstPlayerWarp()
	{
		Warp warpIgnoringGender = null;
		foreach (Warp warp in this.warps)
		{
			if (!warp.npcOnly.Value)
			{
				if (!WarpPathfindingCache.GenderRestrictions.TryGetValue(warp.TargetName, out var gender) || gender == Game1.player.Gender)
				{
					return warp;
				}
				if (warpIgnoringGender == null)
				{
					warpIgnoringGender = warp;
				}
			}
		}
		return warpIgnoringGender ?? this.warps.FirstOrDefault();
	}

	public void addResourceClumpAndRemoveUnderlyingTerrain(int resourceClumpIndex, int width, int height, Vector2 tile)
	{
		this.removeObjectsAndSpawned((int)tile.X, (int)tile.Y, width, height);
		this.resourceClumps.Add(new ResourceClump(resourceClumpIndex, width, height, tile));
	}

	public virtual bool canFishHere()
	{
		return true;
	}

	/// <summary>Get whether a player can resume in this location after waking up, instead of being warped home.</summary>
	/// <param name="who">The player who's waking up here.</param>
	/// <param name="tile">The tile at which they're waking up, or <c>null</c> to use <see cref="F:StardewValley.Farmer.lastSleepPoint" />.</param>
	public virtual bool CanWakeUpHere(Farmer who, Point? tile = null)
	{
		Point wakeUpTile = tile ?? who.lastSleepPoint.Value;
		bool allowWakeUpWithoutBed;
		if (!BedFurniture.IsBedHere(this, wakeUpTile.X, wakeUpTile.Y) && !who.sleptInTemporaryBed.Value && !(this is IslandFarmHouse))
		{
			return this.TryGetMapPropertyAs("AllowWakeUpWithoutBed", out allowWakeUpWithoutBed, required: false) && allowWakeUpWithoutBed;
		}
		return true;
	}

	public virtual bool CanRefillWateringCanOnTile(int tileX, int tileY)
	{
		Vector2 tile = new Vector2(tileX, tileY);
		Building buildingAt = this.getBuildingAt(tile);
		if (buildingAt != null && buildingAt.CanRefillWateringCan())
		{
			return true;
		}
		if (!this.isWaterTile(tileX, tileY) && this.doesTileHaveProperty(tileX, tileY, "WaterSource", "Back") == null)
		{
			if (!this.isOutdoors.Value && this.doesTileHaveProperty(tileX, tileY, "Action", "Buildings") == "kitchen")
			{
				if (this.getTileIndexAt(tileX, tileY, "Buildings", "untitled tile sheet") != 172)
				{
					return this.getTileIndexAt(tileX, tileY, "Buildings", "untitled tile sheet") == 257;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public virtual bool isTileBuildingFishable(int tileX, int tileY)
	{
		Vector2 tile = new Vector2(tileX, tileY);
		foreach (Building building in this.buildings)
		{
			if (building.isTileFishable(tile))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool isTileFishable(int tileX, int tileY)
	{
		if (this.isTileBuildingFishable(tileX, tileY))
		{
			return true;
		}
		if (!this.isWaterTile(tileX, tileY) || this.doesTileHaveProperty(tileX, tileY, "NoFishing", "Back") != null || this.hasTileAt(tileX, tileY, "Buildings"))
		{
			return this.doesTileHaveProperty(tileX, tileY, "Water", "Buildings") != null;
		}
		return true;
	}

	public bool isFarmerCollidingWithAnyCharacter()
	{
		if (this.characters.Count > 0)
		{
			Microsoft.Xna.Framework.Rectangle playerBounds = Game1.player.GetBoundingBox();
			foreach (NPC n in this.characters)
			{
				if (n != null && playerBounds.Intersects(n.GetBoundingBox()))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, Character character)
	{
		return this.isCollidingPosition(position, viewport, character is Farmer, 0, glider: false, character, pathfinding: false);
	}

	public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
	{
		return this.isCollidingPosition(position, viewport, character is Farmer, damagesFarmer, glider, character, pathfinding: false);
	}

	protected bool _TestCornersWorld(int top, int bottom, int left, int right, Func<int, int, bool> action)
	{
		if (action(right, top))
		{
			return true;
		}
		if (action(right, bottom))
		{
			return true;
		}
		if (action(left, top))
		{
			return true;
		}
		if (action(left, bottom))
		{
			return true;
		}
		return false;
	}

	protected bool _TestCornersTiles(Vector2 top_right, Vector2 top_left, Vector2 bottom_right, Vector2 bottom_left, Vector2 top_mid, Vector2 bottom_mid, Vector2? player_top_right, Vector2? player_top_left, Vector2? player_bottom_right, Vector2? player_bottom_left, Vector2? player_top_mid, Vector2? player_bottom_mid, bool bigger_than_tile, Func<Vector2, bool> action)
	{
		this._visitedCollisionTiles.Clear();
		if (player_top_right != top_right && this._visitedCollisionTiles.Add(top_right) && action(top_right))
		{
			return true;
		}
		if (player_top_left != top_left && this._visitedCollisionTiles.Add(top_left) && action(top_left))
		{
			return true;
		}
		if (bottom_left != player_bottom_left && this._visitedCollisionTiles.Add(bottom_left) && action(bottom_left))
		{
			return true;
		}
		if (bottom_right != player_bottom_right && this._visitedCollisionTiles.Add(bottom_right) && action(bottom_right))
		{
			return true;
		}
		if (bigger_than_tile)
		{
			if (player_top_mid != top_mid && this._visitedCollisionTiles.Add(top_mid) && action(top_mid))
			{
				return true;
			}
			if (player_bottom_mid != bottom_mid && this._visitedCollisionTiles.Add(bottom_mid) && action(bottom_mid))
			{
				return true;
			}
		}
		return false;
	}

	public Furniture GetFurnitureAt(Vector2 tile_position)
	{
		Point position = new Point
		{
			X = (int)((float)(int)tile_position.X + 0.5f) * 64,
			Y = (int)((float)(int)tile_position.Y + 0.5f) * 64
		};
		foreach (Furniture f in this.furniture)
		{
			if (!f.isPassable() && f.GetBoundingBox().Contains(position))
			{
				return f;
			}
		}
		foreach (Furniture f2 in this.furniture)
		{
			if (f2.isPassable() && f2.GetBoundingBox().Contains(position))
			{
				return f2;
			}
		}
		return null;
	}

	public virtual Microsoft.Xna.Framework.Rectangle GetBuildableRectangle()
	{
		if (!this._buildableTileRect.HasValue)
		{
			this._buildableTileRect = (this.TryGetMapPropertyAs("ValidBuildRect", out Microsoft.Xna.Framework.Rectangle area, required: false) ? area : Microsoft.Xna.Framework.Rectangle.Empty);
			this._looserBuildRestrictions = this.HasMapPropertyWithValue("LooserBuildRestrictions");
		}
		return this._buildableTileRect.Value;
	}

	public virtual bool IsBuildableLocation()
	{
		if (this.HasMapPropertyWithValue("CanBuildHere"))
		{
			if (!Game1.multiplayer.isAlwaysActiveLocation(this))
			{
				if (!this.showedBuildableButNotAlwaysActiveWarning)
				{
					Game1.log.Warn($"Location {this.NameOrUniqueName} has the CanBuildHere map property set, but its {"AlwaysActive"} option is disabled, so building is disabled here.");
					this.showedBuildableButNotAlwaysActiveWarning = true;
				}
				return false;
			}
			string conditions = this.getMapProperty("BuildConditions");
			if (string.IsNullOrEmpty(conditions) || GameStateQuery.CheckConditions(conditions, this))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Get whether a pixel area is fully outside the bounds of the map.</summary>
	/// <param name="pixelPosition">The pixel position.</param>
	public virtual bool IsOutOfBounds(Microsoft.Xna.Framework.Rectangle pixelPosition)
	{
		if (pixelPosition.Right < 0 || pixelPosition.Bottom < 0)
		{
			return true;
		}
		Layer layer = this.map.Layers[0];
		if (pixelPosition.X <= layer.DisplayWidth)
		{
			return pixelPosition.Top > layer.DisplayHeight;
		}
		return true;
	}

	public virtual bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
	{
		bool is_event_up = Game1.eventUp;
		if (is_event_up && Game1.CurrentEvent != null && !Game1.CurrentEvent.ignoreObjectCollisions)
		{
			is_event_up = false;
		}
		this.updateMap();
		if (this.IsOutOfBounds(position))
		{
			if (isFarmer && Game1.eventUp)
			{
				bool? flag = this.currentEvent?.isFestival;
				if (flag.HasValue && flag == true && this.currentEvent.checkForCollision(position, (character as Farmer) ?? Game1.player))
				{
					return true;
				}
			}
			return false;
		}
		if (character == null && !ignoreCharacterRequirement)
		{
			return true;
		}
		Vector2 nextTopRight = new Vector2(position.Right / 64, position.Top / 64);
		Vector2 nextTopLeft = new Vector2(position.Left / 64, position.Top / 64);
		Vector2 nextBottomRight = new Vector2(position.Right / 64, position.Bottom / 64);
		Vector2 nextBottomLeft = new Vector2(position.Left / 64, position.Bottom / 64);
		bool nextLargerThanTile = position.Width > 64;
		Vector2 nextBottomMid = new Vector2(position.Center.X / 64, position.Bottom / 64);
		Vector2 nextTopMid = new Vector2(position.Center.X / 64, position.Top / 64);
		BoundingBoxGroup passableTiles = null;
		Farmer farmer = character as Farmer;
		Microsoft.Xna.Framework.Rectangle? currentBounds;
		if (farmer != null)
		{
			isFarmer = true;
			currentBounds = farmer.GetBoundingBox();
			passableTiles = farmer.TemporaryPassableTiles;
		}
		else
		{
			farmer = null;
			isFarmer = false;
			currentBounds = null;
		}
		Vector2? currentTopRight = null;
		Vector2? currentTopLeft = null;
		Vector2? currentBottomRight = null;
		Vector2? currentBottomLeft = null;
		Vector2? currentBottomMid = null;
		Vector2? currentTopMid = null;
		if (currentBounds.HasValue)
		{
			currentTopRight = new Vector2((currentBounds.Value.Right - 1) / 64, currentBounds.Value.Top / 64);
			currentTopLeft = new Vector2(currentBounds.Value.Left / 64, currentBounds.Value.Top / 64);
			currentBottomRight = new Vector2((currentBounds.Value.Right - 1) / 64, (currentBounds.Value.Bottom - 1) / 64);
			currentBottomLeft = new Vector2(currentBounds.Value.Left / 64, (currentBounds.Value.Bottom - 1) / 64);
			currentBottomMid = new Vector2(currentBounds.Value.Center.X / 64, (currentBounds.Value.Bottom - 1) / 64);
			currentTopMid = new Vector2(currentBounds.Value.Center.X / 64, currentBounds.Value.Top / 64);
		}
		if (farmer?.bridge != null && farmer.onBridge.Value && position.Right >= farmer.bridge.bridgeBounds.X && position.Left <= farmer.bridge.bridgeBounds.Right)
		{
			if (this._TestCornersWorld(position.Top, position.Bottom, position.Left, position.Right, (int x, int y) => (y > farmer.bridge.bridgeBounds.Bottom || y < farmer.bridge.bridgeBounds.Top) ? true : false))
			{
				return true;
			}
			return false;
		}
		if (!glider)
		{
			if (character != null && this.animals.FieldDict.Count > 0 && !(character is FarmAnimal))
			{
				foreach (FarmAnimal animal in this.animals.Values)
				{
					Microsoft.Xna.Framework.Rectangle animalBounds = animal.GetBoundingBox();
					if (position.Intersects(animalBounds) && (!currentBounds.HasValue || !currentBounds.Value.Intersects(animalBounds)) && (passableTiles == null || !passableTiles.Intersects(position)))
					{
						if (!skipCollisionEffects)
						{
							animal.farmerPushing();
						}
						return true;
					}
				}
			}
			if (this.buildings.Count > 0)
			{
				foreach (Building b in this.buildings)
				{
					if (!b.intersects(position) || (currentBounds.HasValue && b.intersects(currentBounds.Value)))
					{
						continue;
					}
					if (!(character is FarmAnimal) && !(character is JunimoHarvester))
					{
						if (!(character is NPC))
						{
							return true;
						}
						Microsoft.Xna.Framework.Rectangle door = b.getRectForHumanDoor();
						door.Height += 64;
						if (!door.Contains(position))
						{
							return true;
						}
					}
					else
					{
						Microsoft.Xna.Framework.Rectangle door2 = b.getRectForAnimalDoor();
						door2.Height += 64;
						if (!door2.Contains(position))
						{
							return true;
						}
						if (character is FarmAnimal animal2 && !animal2.CanLiveIn(b))
						{
							return true;
						}
					}
				}
			}
			if (this.resourceClumps.Count > 0)
			{
				foreach (ResourceClump resourceClump in this.resourceClumps)
				{
					Microsoft.Xna.Framework.Rectangle bounds = resourceClump.getBoundingBox();
					if (bounds.Intersects(position) && (!currentBounds.HasValue || !bounds.Intersects(currentBounds.Value)))
					{
						return true;
					}
				}
			}
			if (!is_event_up && this.furniture.Count > 0)
			{
				foreach (Furniture f in this.furniture)
				{
					if (f.furniture_type.Value != 12 && f.IntersectsForCollision(position) && (!currentBounds.HasValue || !f.IntersectsForCollision(currentBounds.Value)))
					{
						return true;
					}
				}
			}
			NetCollection<LargeTerrainFeature> netCollection = this.largeTerrainFeatures;
			if (netCollection != null && netCollection.Count > 0)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
				{
					Microsoft.Xna.Framework.Rectangle bounds2 = largeTerrainFeature.getBoundingBox();
					if (bounds2.Intersects(position) && (!currentBounds.HasValue || !bounds2.Intersects(currentBounds.Value)))
					{
						return true;
					}
				}
			}
		}
		if (!glider)
		{
			TerrainFeature value;
			if ((!is_event_up || (character != null && !isFarmer && (!pathfinding || !character.willDestroyObjectsUnderfoot))) && this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 corner)
			{
				if (this.objects.TryGetValue(corner, out value) && value != null)
				{
					if (value.isPassable())
					{
						return false;
					}
					Microsoft.Xna.Framework.Rectangle boundingBox = value.GetBoundingBox();
					if (boundingBox.Intersects(position) && (character == null || character.collideWith(value)))
					{
						if (character is FarmAnimal && value.isAnimalProduct())
						{
							return false;
						}
						if (passableTiles != null && passableTiles.Intersects(boundingBox))
						{
							return false;
						}
						return true;
					}
				}
				return false;
			}))
			{
				return true;
			}
			this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, null, null, null, null, null, null, nextLargerThanTile, delegate(Vector2 corner)
			{
				if (this.terrainFeatures.TryGetValue(corner, out value) && value != null && value.getBoundingBox().Intersects(position) && !pathfinding && character != null && !skipCollisionEffects)
				{
					value.doCollisionAction(position, (int)((float)character.speed + character.addedSpeed), corner, character);
				}
				return false;
			});
			if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, (Vector2 corner) => (this.terrainFeatures.TryGetValue(corner, out value) && value != null && value.getBoundingBox().Intersects(position) && !value.isPassable(character)) ? true : false))
			{
				return true;
			}
		}
		if (character != null && character.hasSpecialCollisionRules() && (character.isColliding(this, nextTopRight) || character.isColliding(this, nextTopLeft) || character.isColliding(this, nextBottomRight) || character.isColliding(this, nextBottomLeft)))
		{
			return true;
		}
		if (((isFarmer && (this.currentEvent == null || this.currentEvent.playerControlSequence)) || (character != null && character.collidesWithOtherCharacters.Value)) && !pathfinding)
		{
			for (int i = this.characters.Count - 1; i >= 0; i--)
			{
				NPC other = this.characters[i];
				if (other != null && (character == null || !character.Equals(other)))
				{
					Microsoft.Xna.Framework.Rectangle bounding_box = other.GetBoundingBox();
					if (other.layingDown)
					{
						bounding_box.Y -= 64;
						bounding_box.Height += 64;
					}
					if (bounding_box.Intersects(position) && !Game1.player.temporarilyInvincible && !skipCollisionEffects)
					{
						other.behaviorOnFarmerPushing();
					}
					if (isFarmer)
					{
						if (!is_event_up && !other.farmerPassesThrough && bounding_box.Intersects(position) && !Game1.player.temporarilyInvincible && Game1.player.TemporaryPassableTiles.IsEmpty() && (!other.IsMonster || (!((Monster)other).isGlider.Value && !Game1.player.GetBoundingBox().Intersects(other.GetBoundingBox()))) && !other.IsInvisible && !Game1.player.GetBoundingBox().Intersects(bounding_box))
						{
							return true;
						}
					}
					else if (bounding_box.Intersects(position))
					{
						return true;
					}
				}
			}
		}
		Layer back_layer = this.map.RequireLayer("Back");
		Layer buildings_layer = this.map.RequireLayer("Buildings");
		Tile t;
		if (isFarmer)
		{
			Event obj = this.currentEvent;
			if (obj != null && obj.checkForCollision(position, (character as Farmer) ?? Game1.player))
			{
				return true;
			}
		}
		else
		{
			if (!pathfinding && !(character is Monster) && damagesFarmer == 0 && !glider)
			{
				foreach (Farmer otherFarmer in this.farmers)
				{
					if (position.Intersects(otherFarmer.GetBoundingBox()))
					{
						return true;
					}
				}
			}
			if ((this.isFarm.Value || MineShaft.IsGeneratedLevel(this) || this is IslandLocation) && character != null && !character.Name.Contains("NPC") && !character.EventActor && !glider)
			{
				if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 vector)
				{
					t = back_layer.Tiles[(int)vector.X, (int)vector.Y];
					return (t != null && t.Properties.ContainsKey("NPCBarrier")) ? true : false;
				}))
				{
					return true;
				}
			}
			if (glider && !projectile)
			{
				return false;
			}
		}
		if (!isFarmer || !Game1.player.isRafting)
		{
			if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 vector)
			{
				t = back_layer.Tiles[(int)vector.X, (int)vector.Y];
				return (t != null && t.Properties.ContainsKey("TemporaryBarrier")) ? true : false;
			}))
			{
				return true;
			}
		}
		if (!isFarmer || !Game1.player.isRafting)
		{
			if ((!(character is FarmAnimal animal3) || !animal3.IsActuallySwimming()) && this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 vector)
			{
				Tile tile2 = back_layer.Tiles[(int)vector.X, (int)vector.Y];
				if (tile2 != null)
				{
					bool flag2 = tile2.TileIndexProperties.ContainsKey("Passable");
					if (!flag2)
					{
						flag2 = tile2.Properties.ContainsKey("Passable");
					}
					if (flag2)
					{
						if (passableTiles != null && passableTiles.Contains((int)vector.X, (int)vector.Y))
						{
							return false;
						}
						return true;
					}
				}
				return false;
			}))
			{
				return true;
			}
			if (character == null || character.shouldCollideWithBuildingLayer(this))
			{
				Tile tmp;
				if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 vector)
				{
					tmp = buildings_layer.Tiles[(int)vector.X, (int)vector.Y];
					if (tmp != null)
					{
						if (projectile && this is VolcanoDungeon)
						{
							Tile tile2 = back_layer.Tiles[(int)vector.X, (int)vector.Y];
							if (tile2 != null)
							{
								if (tile2.TileIndexProperties.ContainsKey("Water"))
								{
									return false;
								}
								if (tile2.Properties.ContainsKey("Water"))
								{
									return false;
								}
							}
						}
						if (!tmp.TileIndexProperties.ContainsKey("Shadow") && !tmp.TileIndexProperties.ContainsKey("Passable") && !tmp.Properties.ContainsKey("Passable") && (!projectile || (!tmp.TileIndexProperties.ContainsKey("ProjectilePassable") && !tmp.Properties.ContainsKey("ProjectilePassable"))))
						{
							if (isFarmer)
							{
								goto IL_01c9;
							}
							if (!tmp.TileIndexProperties.ContainsKey("NPCPassable") && !tmp.Properties.ContainsKey("NPCPassable"))
							{
								bool? flag2 = character?.canPassThroughActionTiles();
								if (!flag2.HasValue || flag2 != true || !tmp.Properties.ContainsKey("Action"))
								{
									goto IL_01c9;
								}
							}
						}
					}
					return false;
					IL_01c9:
					if (passableTiles != null)
					{
						return !passableTiles.Contains((int)vector.X, (int)vector.Y);
					}
					return true;
				}))
				{
					return true;
				}
			}
			if (!isFarmer && character?.controller != null && !skipCollisionEffects)
			{
				Point tileLocation = new Point(position.Center.X / 64, position.Bottom / 64);
				Tile tile = buildings_layer.Tiles[tileLocation.X, tileLocation.Y];
				if (tile != null && tile.Properties.ContainsKey("Action"))
				{
					this.openDoor(new Location(tileLocation.X, tileLocation.Y), Game1.currentLocation.Equals(this));
				}
				else
				{
					tileLocation = new Point(position.Center.X / 64, position.Top / 64);
					tile = buildings_layer.Tiles[tileLocation.X, tileLocation.Y];
					if (tile != null && tile.Properties.ContainsKey("Action"))
					{
						this.openDoor(new Location(tileLocation.X, tileLocation.Y), Game1.currentLocation.Equals(this));
					}
				}
			}
			return false;
		}
		if (this._TestCornersTiles(nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate(Vector2 vector)
		{
			t = back_layer.Tiles[(int)vector.X, (int)vector.Y];
			if ((!(t?.TileIndexProperties.ContainsKey("Water"))) ?? true)
			{
				int num = (int)vector.X;
				int num2 = (int)vector.Y;
				if (this.IsTileBlockedBy(new Vector2(num, num2)))
				{
					Game1.player.isRafting = false;
					Game1.player.Position = new Vector2(num * 64, num2 * 64 - 32);
					Game1.player.setTrajectory(0, 0);
				}
				return true;
			}
			return false;
		}))
		{
			return true;
		}
		return false;
	}

	public bool isTilePassable(Vector2 tileLocation)
	{
		Tile backTile = this.map.RequireLayer("Back").Tiles[(int)tileLocation.X, (int)tileLocation.Y];
		if (backTile != null && backTile.TileIndexProperties.ContainsKey("Passable"))
		{
			return false;
		}
		Tile buildingsTile = this.map.RequireLayer("Buildings").Tiles[(int)tileLocation.X, (int)tileLocation.Y];
		if (buildingsTile != null && !buildingsTile.TileIndexProperties.ContainsKey("Shadow") && !buildingsTile.TileIndexProperties.ContainsKey("Passable"))
		{
			return false;
		}
		return true;
	}

	public bool isTilePassable(Location tileLocation, xTile.Dimensions.Rectangle viewport)
	{
		return this.isTilePassable(new Vector2(tileLocation.X, tileLocation.Y));
	}

	public bool isPointPassable(Location location, xTile.Dimensions.Rectangle viewport)
	{
		return this.isTilePassable(new Location(location.X / 64, location.Y / 64), viewport);
	}

	public bool isTilePassable(Microsoft.Xna.Framework.Rectangle nextPosition, xTile.Dimensions.Rectangle viewport)
	{
		if (this.isPointPassable(new Location(nextPosition.Left, nextPosition.Top), viewport) && this.isPointPassable(new Location(nextPosition.Right, nextPosition.Bottom), viewport) && this.isPointPassable(new Location(nextPosition.Left, nextPosition.Bottom), viewport))
		{
			return this.isPointPassable(new Location(nextPosition.Right, nextPosition.Top), viewport);
		}
		return false;
	}

	public bool isTileOnMap(Vector2 position)
	{
		if (position.X >= 0f && position.X < (float)this.map.Layers[0].LayerWidth && position.Y >= 0f)
		{
			return position.Y < (float)this.map.Layers[0].LayerHeight;
		}
		return false;
	}

	public bool isTileOnMap(Point tile)
	{
		return this.isTileOnMap(tile.X, tile.Y);
	}

	public bool isTileOnMap(int x, int y)
	{
		if (x >= 0 && x < this.map.Layers[0].LayerWidth && y >= 0)
		{
			return y < this.map.Layers[0].LayerHeight;
		}
		return false;
	}

	public int numberOfObjectsWithName(string name)
	{
		int number = 0;
		foreach (Object value in this.objects.Values)
		{
			if (value.Name.Equals(name))
			{
				number++;
			}
		}
		return number;
	}

	public virtual Point getWarpPointTo(string location, Character character = null)
	{
		foreach (Building building in this.buildings)
		{
			if (building.HasIndoorsName(location))
			{
				return building.getPointForHumanDoor();
			}
		}
		foreach (Warp w in this.warps)
		{
			if (w.TargetName.Equals(location))
			{
				return new Point(w.X, w.Y);
			}
			if (w.TargetName.Equals("BoatTunnel") && location == "IslandSouth")
			{
				return new Point(w.X, w.Y);
			}
		}
		foreach (KeyValuePair<Point, string> v in this.doors.Pairs)
		{
			if (v.Value.Equals("BoatTunnel") && location == "IslandSouth")
			{
				return v.Key;
			}
			if (v.Value.Equals(location))
			{
				return v.Key;
			}
		}
		return Point.Zero;
	}

	public Point getWarpPointTarget(Point warpPointLocation, Character character = null)
	{
		foreach (Warp w in this.warps)
		{
			if (w.X == warpPointLocation.X && w.Y == warpPointLocation.Y)
			{
				return new Point(w.TargetX, w.TargetY);
			}
		}
		foreach (KeyValuePair<Point, string> v in this.doors.Pairs)
		{
			if (!v.Key.Equals(warpPointLocation))
			{
				continue;
			}
			string[] action = this.GetTilePropertySplitBySpaces("Action", "Buildings", warpPointLocation.X, warpPointLocation.Y);
			string propertyName = ArgUtility.Get(action, 0, "");
			switch (propertyName)
			{
			case "WarpCommunityCenter":
				return new Point(32, 23);
			case "Warp_Sunroom_Door":
				return new Point(5, 13);
			case "WarpBoatTunnel":
				return new Point(17, 43);
			case "WarpMensLocker":
			case "LockedDoorWarp":
			case "Warp":
			case "WarpWomensLocker":
				break;
			default:
				if (!propertyName.Contains("Warp"))
				{
					continue;
				}
				Game1.log.Warn($"Door in {this.NameOrUniqueName} ({v.Key}) has unknown warp property '{string.Join(" ", action)}', parsing with legacy logic.");
				break;
			}
			if (!ArgUtility.TryGetPoint(action, 1, out var tile, out var error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName, out error, allowBlank: true, "string locationName"))
			{
				this.LogTileActionError(action, warpPointLocation.X, warpPointLocation.Y, error);
				continue;
			}
			if (!(locationName == "BoatTunnel"))
			{
				if (locationName == "Trailer" && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					return new Point(13, 24);
				}
				return new Point(tile.X, tile.Y);
			}
			return new Point(17, 43);
		}
		return Point.Zero;
	}

	public virtual bool HasLocationOverrideDialogue(NPC character)
	{
		return false;
	}

	public virtual string GetLocationOverrideDialogue(NPC character)
	{
		if (!this.HasLocationOverrideDialogue(character))
		{
			return null;
		}
		return "";
	}

	public NPC doesPositionCollideWithCharacter(Microsoft.Xna.Framework.Rectangle r, bool ignoreMonsters = false)
	{
		foreach (NPC n in this.characters)
		{
			if (n.GetBoundingBox().Intersects(r) && (!n.IsMonster || !ignoreMonsters))
			{
				return n;
			}
		}
		return null;
	}

	public void switchOutNightTiles()
	{
		string[] split = this.GetMapPropertySplitBySpaces("NightTiles");
		for (int i = 0; i < split.Length; i += 4)
		{
			if (!ArgUtility.TryGet(split, i, out var layerId, out var error, allowBlank: true, "string layerId") || !ArgUtility.TryGetPoint(split, i + 1, out var position, out error, "Point position") || !ArgUtility.TryGetInt(split, i + 3, out var tileIndex, out error, "int tileIndex"))
			{
				this.LogMapPropertyError("NightTiles", split, error);
			}
			else if ((tileIndex != 726 && tileIndex != 720) || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				Tile tile = this.map.RequireLayer(layerId).Tiles[position.X, position.Y];
				if (tile == null)
				{
					this.LogMapPropertyError("NightTiles", split, $"there's no tile at position ({position})");
				}
				else
				{
					tile.TileIndex = tileIndex;
				}
			}
		}
		if (!(this is MineShaft) && !(this is Woods))
		{
			this.lightGlows.Clear();
		}
	}

	public string GetMorningSong()
	{
		LocationWeather locationWeather = this.GetWeather();
		if (locationWeather.IsRaining)
		{
			return "rain";
		}
		List<string> songList = new List<string>();
		List<LocationMusicData> entries = this.GetLocationContext().Music;
		if (entries == null || entries.Count <= 0)
		{
			entries = LocationContexts.Default.Music ?? new List<LocationMusicData>();
		}
		foreach (LocationMusicData entry in entries)
		{
			if (GameStateQuery.CheckConditions(entry.Condition, this))
			{
				songList.Add(entry.Track);
			}
		}
		if (songList.Count == 0)
		{
			return "none";
		}
		int songIndex = locationWeather.monthlyNonRainyDayCount.Value - 1;
		if (songIndex < 0)
		{
			songIndex = 0;
		}
		return songList[songIndex % songList.Count];
	}

	/// <summary>Update the music when the player changes location.</summary>
	/// <param name="oldLocation">The location the player just left.</param>
	/// <param name="newLocation">The location the player just arrived in.</param>
	/// <remarks>For changes to music while a location is active, see <see cref="M:StardewValley.GameLocation.checkForMusic(Microsoft.Xna.Framework.GameTime)" />.</remarks>
	public static void HandleMusicChange(GameLocation oldLocation, GameLocation newLocation)
	{
		string currentTrack = Game1.getMusicTrackName();
		if (!newLocation.IsOutdoors && Game1.IsPlayingOutdoorsAmbience)
		{
			Game1.changeMusicTrack("none", track_interruptable: true);
		}
		if (currentTrack == "rain")
		{
			if (!Game1.IsRainingHere(newLocation))
			{
				Game1.stopMusicTrack(MusicContext.Default);
			}
			else if (newLocation is MineShaft && !(oldLocation is MineShaft))
			{
				Game1.stopMusicTrack(MusicContext.Default);
			}
		}
		if (Game1.getMusicTrackName() == "sam_acoustic1")
		{
			Game1.stopMusicTrack(MusicContext.Default);
		}
		if (newLocation is MineShaft)
		{
			return;
		}
		string oldLocationContextId = oldLocation?.GetLocationContextId();
		LocationContextData oldLocationContext = oldLocation?.GetLocationContext();
		LocationData newLocationData = newLocation?.GetData();
		string newLocationContextId = newLocation?.GetLocationContextId();
		LocationContextData newLocationContext = newLocation?.GetLocationContext();
		string newLocationMusic = newLocation?.GetLocationSpecificMusic();
		MusicContext newMusicContext = newLocationData?.MusicContext ?? MusicContext.Default;
		bool newLocationIsTownTheme = false;
		if (newLocation != null)
		{
			if (newLocationMusic != null)
			{
				newLocationIsTownTheme = newLocationData?.MusicIsTownTheme ?? false;
				newLocation.isMusicTownMusic = newLocationIsTownTheme;
			}
			else
			{
				newLocation.isMusicTownMusic = false;
			}
		}
		if (newLocationMusic == null || newMusicContext == MusicContext.Default)
		{
			Game1.stopMusicTrack(MusicContext.SubLocation);
		}
		if (newLocationMusic == null && Game1.IsRainingHere(newLocation))
		{
			newLocationMusic = "rain";
		}
		else if (Game1.IsPlayingMorningSong && oldLocation != null && oldLocation.GetMorningSong() != newLocation.GetMorningSong() && Game1.shouldPlayMorningSong(loading_game: true))
		{
			Game1.playMorningSong(ignoreDelay: true);
			return;
		}
		if (newLocationMusic == null && !Game1.IsPlayingBackgroundMusic && newLocation.isOutdoors.Value && Game1.shouldPlayMorningSong())
		{
			Game1.playMorningSong();
			return;
		}
		if (oldLocationContextId != newLocationContextId)
		{
			GameLocation.PlayedNewLocationContextMusic = false;
		}
		if (!newLocationContext.DefaultMusicDelayOneScreen)
		{
			GameLocation.PlayedNewLocationContextMusic = false;
		}
		if (Game1.IsPlayingTownMusic && newLocation.IsOutdoors && (!newLocationIsTownTheme || newLocationMusic != currentTrack))
		{
			Game1.IsPlayingTownMusic = false;
			Game1.changeMusicTrack("none", track_interruptable: true);
		}
		if (newLocationIsTownTheme)
		{
			if (newLocationMusic == currentTrack)
			{
				return;
			}
			newLocationMusic = null;
		}
		if (newLocationMusic == null)
		{
			if (oldLocationContext != null && newLocationContext.DefaultMusic != oldLocationContext.DefaultMusic)
			{
				Game1.stopMusicTrack(MusicContext.Default);
			}
			if (!GameLocation.PlayedNewLocationContextMusic)
			{
				if (newLocationContext.DefaultMusic != null)
				{
					if (Game1.isDarkOut(newLocation) || Game1.isStartingToGetDarkOut(newLocation) || Game1.IsRainingHere(newLocation))
					{
						GameLocation.PlayedNewLocationContextMusic = true;
					}
					else if (newLocationContext.DefaultMusicCondition == null || GameStateQuery.CheckConditions(newLocationContext.DefaultMusicCondition, newLocation))
					{
						Game1.changeMusicTrack(newLocationContext.DefaultMusic, track_interruptable: true);
						Game1.IsPlayingBackgroundMusic = true;
						GameLocation.PlayedNewLocationContextMusic = true;
					}
				}
				else
				{
					GameLocation.PlayedNewLocationContextMusic = true;
					if (!newLocationIsTownTheme && Game1.shouldPlayMorningSong(loading_game: true))
					{
						Game1.playMorningSong();
						return;
					}
				}
			}
		}
		if (!(currentTrack != newLocationMusic))
		{
			return;
		}
		if (newLocationMusic == null)
		{
			if (!Game1.IsPlayingBackgroundMusic && !Game1.IsPlayingOutdoorsAmbience)
			{
				Game1.stopMusicTrack(MusicContext.Default);
			}
		}
		else
		{
			Game1.changeMusicTrack(newLocationMusic, track_interruptable: true, newMusicContext);
		}
	}

	/// <summary>Check for music changes while the level is active.</summary>
	/// <param name="time">The current game time.</param>
	/// <remarks>This should only be used for music changes while a location is active. Other music changes should be in <see cref="M:StardewValley.GameLocation.HandleMusicChange(StardewValley.GameLocation,StardewValley.GameLocation)" />.</remarks>
	public virtual void checkForMusic(GameTime time)
	{
		if (Game1.getMusicTrackName() == "sam_acoustic1" && Game1.isMusicContextActiveButNotPlaying())
		{
			Game1.changeMusicTrack("none", track_interruptable: true);
		}
		if (this.isMusicTownMusic.HasValue && this.isMusicTownMusic.Value && !Game1.eventUp && Game1.timeOfDay < 1800 && (Game1.isMusicContextActiveButNotPlaying() || Game1.IsPlayingOutdoorsAmbience))
		{
			string townMusicTrack = this.GetLocationSpecificMusic();
			if (townMusicTrack != null)
			{
				MusicContext context = this.GetData()?.MusicContext ?? MusicContext.Default;
				Game1.changeMusicTrack(townMusicTrack, track_interruptable: false, context);
				Game1.IsPlayingBackgroundMusic = true;
				Game1.IsPlayingTownMusic = true;
			}
		}
		if (this.IsOutdoors && !this.IsRainingHere() && !Game1.eventUp)
		{
			bool isNight = Game1.isDarkOut(this);
			if (isNight && Game1.IsPlayingOutdoorsAmbience && !Game1.IsPlayingNightAmbience)
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (!Game1.isMusicContextActiveButNotPlaying())
			{
				return;
			}
			if (!isNight)
			{
				LocationContextData context2 = this.GetLocationContext();
				if (context2.DayAmbience != null)
				{
					Game1.changeMusicTrack(context2.DayAmbience, track_interruptable: true);
				}
				else
				{
					switch (this.GetSeason())
					{
					case Season.Spring:
						Game1.changeMusicTrack("spring_day_ambient", track_interruptable: true);
						break;
					case Season.Summer:
						Game1.changeMusicTrack("summer_day_ambient", track_interruptable: true);
						break;
					case Season.Fall:
						Game1.changeMusicTrack("fall_day_ambient", track_interruptable: true);
						break;
					case Season.Winter:
						Game1.changeMusicTrack("winter_day_ambient", track_interruptable: true);
						break;
					}
				}
				Game1.IsPlayingOutdoorsAmbience = true;
			}
			else
			{
				if (Game1.timeOfDay >= 2500)
				{
					return;
				}
				LocationContextData context3 = this.GetLocationContext();
				if (context3.NightAmbience != null)
				{
					Game1.changeMusicTrack(context3.NightAmbience, track_interruptable: true);
				}
				else
				{
					switch (this.GetSeason())
					{
					case Season.Spring:
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case Season.Summer:
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case Season.Fall:
						Game1.changeMusicTrack("spring_night_ambient", track_interruptable: true);
						break;
					case Season.Winter:
						Game1.changeMusicTrack("none", track_interruptable: true);
						break;
					}
				}
				Game1.IsPlayingNightAmbience = true;
				Game1.IsPlayingOutdoorsAmbience = true;
			}
		}
		else if (this.IsRainingHere() && !Game1.showingEndOfNightStuff && Game1.isMusicContextActiveButNotPlaying())
		{
			Game1.changeMusicTrack("rain", track_interruptable: true);
		}
	}

	public virtual string GetLocationSpecificMusic()
	{
		LocationData data = this.GetData();
		if (data != null)
		{
			if (data.MusicIgnoredInRain && this.IsRainingHere())
			{
				return null;
			}
			Season season = this.GetSeason();
			bool ignoreInSeason = false;
			switch (season)
			{
			case Season.Spring:
				ignoreInSeason = data.MusicIgnoredInSpring;
				break;
			case Season.Summer:
				ignoreInSeason = data.MusicIgnoredInSummer;
				break;
			case Season.Fall:
				ignoreInSeason = data.MusicIgnoredInFall;
				break;
			case Season.Winter:
				ignoreInSeason = data.MusicIgnoredInWinter;
				break;
			}
			if (ignoreInSeason)
			{
				return null;
			}
			if (season == Season.Fall && this.IsDebrisWeatherHere() && data.MusicIgnoredInFallDebris)
			{
				return null;
			}
			List<LocationMusicData> music = data.Music;
			if (music != null && music.Count > 0)
			{
				foreach (LocationMusicData music2 in data.Music)
				{
					if (GameStateQuery.CheckConditions(music2.Condition, this))
					{
						return music2.Track;
					}
				}
			}
			if (data.MusicDefault != null)
			{
				return data.MusicDefault;
			}
		}
		string[] musicFields = this.GetMapPropertySplitBySpaces("Music");
		if (musicFields.Length != 0)
		{
			if (musicFields.Length > 1)
			{
				if (!ArgUtility.TryGetInt(musicFields, 0, out var startTime, out var error, "int startTime") || !ArgUtility.TryGetInt(musicFields, 1, out var endTime, out error, "int endTime") || !ArgUtility.TryGet(musicFields, 2, out var musicId, out error, allowBlank: true, "string musicId"))
				{
					this.LogMapPropertyError("Music", musicFields, error);
					return null;
				}
				if (Game1.timeOfDay < startTime || (endTime != 0 && Game1.timeOfDay >= endTime))
				{
					return null;
				}
				return musicId;
			}
			return musicFields[0];
		}
		return null;
	}

	public NPC isCollidingWithCharacter(Microsoft.Xna.Framework.Rectangle box)
	{
		if (Game1.isFestival() && this.currentEvent != null)
		{
			foreach (NPC n in this.currentEvent.actors)
			{
				if (n.GetBoundingBox().Intersects(box))
				{
					return n;
				}
			}
		}
		foreach (NPC n2 in this.characters)
		{
			if (n2.GetBoundingBox().Intersects(box))
			{
				return n2;
			}
		}
		return null;
	}

	public virtual void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (this.critters != null && Game1.farmEvent == null)
		{
			for (int i = 0; i < this.critters.Count; i++)
			{
				this.critters[i].drawAboveFrontLayer(b);
			}
		}
		foreach (NPC character in this.characters)
		{
			character.drawAboveAlwaysFrontLayer(b);
		}
		if (!(this is MineShaft))
		{
			foreach (NPC character2 in this.characters)
			{
				(character2 as Monster)?.drawAboveAllLayers(b);
			}
		}
		if (this.TemporarySprites.Count > 0)
		{
			foreach (TemporaryAnimatedSprite s in this.TemporarySprites)
			{
				if (s.drawAboveAlwaysFront)
				{
					s.draw(b);
				}
			}
		}
		if (this.projectiles.Count <= 0)
		{
			return;
		}
		foreach (Projectile projectile in this.projectiles)
		{
			projectile.draw(b);
		}
	}

	/// <summary>Move objects and furniture covering a tile to another position.</summary>
	/// <param name="oldX">The X tile coordinate from which to move items.</param>
	/// <param name="oldY">The Y tile coordinate from which to move items.</param>
	/// <param name="newX">The X tile coordinate at which to place moved items.</param>
	/// <param name="newY">The Y tile coordinate at which to place moved items.</param>
	/// <param name="unlessItemId">If set, an item won't be moved if its item ID matches this one.</param>
	/// <returns>Returns whether any items were moved.</returns>
	/// <remarks>Multi-tile furniture which cover the old tile will be placed at an equivalent position relative to the new tile.</remarks>
	public bool moveContents(int oldX, int oldY, int newX, int newY, string unlessItemId)
	{
		Vector2 oldTile = new Vector2(oldX, oldY);
		Vector2 newTile = new Vector2(newX, newY);
		Microsoft.Xna.Framework.Rectangle oldPixelArea = new Microsoft.Xna.Framework.Rectangle(oldX * 64, oldY * 64, 64, 64);
		Microsoft.Xna.Framework.Rectangle newPixelArea = new Microsoft.Xna.Framework.Rectangle(newX * 64, newY * 64, 64, 64);
		bool movedAny = false;
		if (this.objects.TryGetValue(oldTile, out var o) && !this.objects.ContainsKey(newTile) && (unlessItemId == null || !ItemRegistry.HasItemId(o, unlessItemId)))
		{
			this.objects.Remove(oldTile);
			this.objects.Add(newTile, o);
			movedAny = true;
		}
		for (int i = this.furniture.Count - 1; i >= 0; i--)
		{
			Furniture f = this.furniture[i];
			if (f.boundingBox.Value.Intersects(oldPixelArea) && (unlessItemId == null || !ItemRegistry.HasItemId(f, unlessItemId)) && !this.furniture.Any((Furniture p) => p.boundingBox.Value.Intersects(newPixelArea)))
			{
				Vector2 offset = f.TileLocation - oldTile;
				this.furniture.RemoveAt(i);
				f.TileLocation = newTile + offset;
				this.furniture.Add(f);
				movedAny = true;
			}
		}
		return movedAny;
	}

	private void getGalaxySword()
	{
		Item galaxySword = ItemRegistry.Create("(W)4");
		Game1.flashAlpha = 1f;
		Game1.player.holdUpItemThenMessage(galaxySword);
		Game1.player.reduceActiveItemByOne();
		if (!Game1.player.addItemToInventoryBool(galaxySword))
		{
			Game1.createItemDebris(galaxySword, Game1.player.getStandingPosition(), 1);
		}
		Game1.player.mailReceived.Add("galaxySword");
		Game1.player.jitterStrength = 0f;
		Game1.screenGlowHold = false;
		Game1.multiplayer.globalChatInfoMessage("GalaxySword", Game1.player.Name);
	}

	public static void RegisterTouchAction(string key, Action<GameLocation, string[], Farmer, Vector2> action)
	{
		if (action == null)
		{
			GameLocation.registeredTouchActions.Remove(key);
		}
		else
		{
			GameLocation.registeredTouchActions[key] = action;
		}
	}

	public static void RegisterTileAction(string key, Func<GameLocation, string[], Farmer, Point, bool> action)
	{
		if (action == null)
		{
			GameLocation.registeredTileActions.Remove(key);
		}
		else
		{
			GameLocation.registeredTileActions[key] = action;
		}
	}

	/// <summary>Whether to ignore any touch actions the player walks over.</summary>
	public virtual bool IgnoreTouchActions()
	{
		return Game1.eventUp;
	}

	/// <summary>Handle a <c>TouchAction</c> property from a <c>Back</c> map tile in the location when a player steps on the tile.</summary>
	/// <param name="fullActionString">The full action string to parse, including the <c>TouchAction</c> prefix.</param>
	/// <param name="playerStandingPosition">The tile coordinate containing the tile which was stepped on.</param>
	public virtual void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
	{
		string[] split = ArgUtility.SplitBySpace(fullActionString);
		this.performTouchAction(split, playerStandingPosition);
	}

	/// <summary>Handle a <c>TouchAction</c> property from a <c>Back</c> map tile in the location when a player steps on the tile.</summary>
	/// <param name="action">The action arguments to parse, including the <c>TouchAction</c> prefix.</param>
	/// <param name="playerStandingPosition">The tile coordinate containing the tile which was stepped on.</param>
	public virtual void performTouchAction(string[] action, Vector2 playerStandingPosition)
	{
		if (this.IgnoreTouchActions())
		{
			return;
		}
		try
		{
			Action<GameLocation, string[], Farmer, Vector2> actionHandler;
			if (!ArgUtility.TryGet(action, 0, out var actionType, out var error, allowBlank: true, "string actionType"))
			{
				LogError(error);
			}
			else if (GameLocation.registeredTouchActions.TryGetValue(actionType, out actionHandler))
			{
				actionHandler(this, action, Game1.player, playerStandingPosition);
			}
			else
			{
				if (actionType == null)
				{
					return;
				}
				switch (actionType.Length)
				{
				case 9:
					switch (actionType[0])
					{
					case 'P':
						if (actionType == "PlayEvent")
						{
							if (!ArgUtility.TryGet(action, 1, out var eventId, out error, allowBlank: true, "string eventId") || !ArgUtility.TryGetOptionalBool(action, 2, out var checkPreconditions, out error, defaultValue: true, "bool checkPreconditions") || !ArgUtility.TryGetOptionalBool(action, 3, out var checkSeen, out error, defaultValue: true, "bool checkSeen") || !ArgUtility.TryGetOptionalRemainder(action, 4, out var fallbackAction))
							{
								LogError(error);
							}
							else if (!Game1.PlayEvent(eventId, checkPreconditions, checkSeen) && fallbackAction != null)
							{
								this.performAction(fallbackAction, Game1.player, new Location((int)playerStandingPosition.X, (int)playerStandingPosition.Y));
							}
						}
						break;
					case 'M':
					{
						if (!(actionType == "MagicWarp"))
						{
							break;
						}
						if (!ArgUtility.TryGet(action, 1, out var locationToWarp, out error, allowBlank: true, "string locationToWarp") || !ArgUtility.TryGetPoint(action, 2, out var tile, out error, "Point tile") || !ArgUtility.TryGetOptional(action, 4, out var mailRequired, out error, null, allowBlank: true, "string mailRequired"))
						{
							LogError(error);
						}
						else if (mailRequired == null || Game1.player.mailReceived.Contains(mailRequired))
						{
							for (int j = 0; j < 12; j++)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)Game1.player.position.X - 256, (int)Game1.player.position.X + 192), Game1.random.Next((int)Game1.player.position.Y - 256, (int)Game1.player.position.Y + 192)), flicker: false, Game1.random.NextBool()));
							}
							this.playSound("wand");
							Game1.freezeControls = true;
							Game1.displayFarmer = false;
							Game1.player.CanMove = false;
							Game1.flashAlpha = 1f;
							DelayedAction.fadeAfterDelay(delegate
							{
								Game1.warpFarmer(locationToWarp, tile.X, tile.Y, flip: false);
								Game1.fadeToBlackAlpha = 0.99f;
								Game1.screenGlow = false;
								Game1.displayFarmer = true;
								Game1.player.CanMove = true;
								Game1.freezeControls = false;
							}, 1000);
							Microsoft.Xna.Framework.Rectangle playerBounds = Game1.player.GetBoundingBox();
							new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
							int j2 = 0;
							Point playerTile = Game1.player.TilePoint;
							for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
							{
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
								{
									layerDepth = 1f,
									delayBeforeAnimationStart = j2 * 25,
									motion = new Vector2(-0.25f, 0f)
								});
								j2++;
							}
						}
						break;
					}
					}
					break;
				case 4:
					switch (actionType[0])
					{
					case 'W':
						if (actionType == "Warp")
						{
							if (!ArgUtility.TryGet(action, 1, out var locationToWarp2, out error, allowBlank: true, "string locationToWarp") || !ArgUtility.TryGetPoint(action, 2, out var tile2, out error, "Point tile") || !ArgUtility.TryGetOptional(action, 4, out var mailRequired2, out error, null, allowBlank: true, "string mailRequired"))
							{
								LogError(error);
							}
							else if (mailRequired2 == null || Game1.player.mailReceived.Contains(mailRequired2))
							{
								Game1.warpFarmer(locationToWarp2, tile2.X, tile2.Y, flip: false);
							}
						}
						break;
					case 'D':
					{
						if (!(actionType == "Door"))
						{
							break;
						}
						for (int i2 = 1; i2 < action.Length && (!(action[i2] == "Sebastian") || !this.IsGreenRainingHere() || Game1.year != 1); i2++)
						{
							if (Game1.player.getFriendshipHeartLevelForNPC(action[i2]) < 2 && i2 == action.Length - 1)
							{
								Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
								Game1.player.yVelocity = 0f;
								Game1.player.Halt();
								Game1.player.TemporaryPassableTiles.Clear();
								if (Game1.player.Tile == this.lastTouchActionLocation)
								{
									if (Game1.player.Position.Y > this.lastTouchActionLocation.Y * 64f + 32f)
									{
										Game1.player.position.Y += 4f;
									}
									else
									{
										Game1.player.position.Y -= 4f;
									}
									this.lastTouchActionLocation = Vector2.Zero;
								}
								if ((!Game1.player.mailReceived.Contains("doorUnlock" + action[1]) || (action.Length != 2 && !Game1.player.mailReceived.Contains("doorUnlock" + action[2]))) && (action.Length != 3 || !Game1.player.mailReceived.Contains("doorUnlock" + action[2])))
								{
									this.ShowLockedDoorMessage(action);
								}
								break;
							}
							if (i2 != action.Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(action[i2]) >= 2)
							{
								Game1.player.mailReceived.Add("doorUnlock" + action[i2]);
								break;
							}
							if (i2 == action.Length - 1 && Game1.player.getFriendshipHeartLevelForNPC(action[i2]) >= 2)
							{
								Game1.player.mailReceived.Add("doorUnlock" + action[i2]);
								break;
							}
						}
						break;
					}
					}
					break;
				case 5:
					switch (actionType[0])
					{
					case 'S':
						if (actionType == "Sleep" && !Game1.newDay && Game1.shouldTimePass() && Game1.player.hasMoved && !Game1.player.passedOut)
						{
							this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), this.createYesNoResponses(), "Sleep", null);
						}
						break;
					case 'E':
						if (actionType == "Emote")
						{
							if (!ArgUtility.TryGet(action, 1, out var npcName, out error, allowBlank: true, "string npcName") || !ArgUtility.TryGetInt(action, 2, out var emote, out error, "int emote"))
							{
								LogError(error);
							}
							else
							{
								this.getCharacterFromName(npcName)?.doEmote(emote);
							}
						}
						break;
					}
					break;
				case 12:
					switch (actionType[0])
					{
					case 'W':
						if (actionType == "WomensLocker" && Game1.player.IsMale)
						{
							Game1.player.position.Y += ((float)Game1.player.Speed + Game1.player.addedSpeed) * 2f;
							Game1.player.Halt();
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
						}
						break;
					case 'P':
						if (actionType == "PoolEntrance")
						{
							if (!Game1.player.swimming.Value)
							{
								Game1.player.swimTimer = 800;
								Game1.player.swimming.Value = true;
								Game1.player.position.Y += 16f;
								Game1.player.yVelocity = -8f;
								this.playSound("pullItemFromWater");
								Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(27, 100f, 4, 0, new Vector2(Game1.player.Position.X, Game1.player.StandingPixel.Y - 40), flicker: false, flipped: false)
								{
									layerDepth = 1f,
									motion = new Vector2(0f, 2f)
								});
							}
							else
							{
								Game1.player.jump();
								Game1.player.swimTimer = 800;
								Game1.player.position.X = playerStandingPosition.X * 64f;
								this.playSound("pullItemFromWater");
								Game1.player.yVelocity = 8f;
								Game1.player.swimming.Value = false;
							}
							Game1.player.noMovementPause = 500;
						}
						break;
					}
					break;
				case 8:
					if (actionType == "asdlfkjg")
					{
						this.removeTileProperty(13, 29, "Back", "TouchAction");
						this.removeTileProperty(14, 29, "Back", "TouchAction");
						this.removeTileProperty(15, 29, "Back", "TouchAction");
						if (Game1.timeOfDay >= 1920 && Game1.timeOfDay < 2020 && this.farmers.Count == 1 && Game1.stats.DaysPlayed > 3 && !Game1.isRaining && Game1.random.NextDouble() < 0.025)
						{
							Game1.player.mailReceived.Add("asdlkjfg1");
							this.playSound("shadowDie");
							DelayedAction.playSoundAfterDelay("grassyStep", 500, this);
							DelayedAction.playSoundAfterDelay("grassyStep", 1000, this);
							DelayedAction.playSoundAfterDelay("grassyStep", 1500, this);
							this.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 48), new Vector2(390f, 1980f), flipped: true, 0f, Color.White)
							{
								animationLength = 8,
								totalNumberOfLoops = 99,
								interval = 100f,
								motion = new Vector2(-5f, -1f),
								scale = 5.5f
							});
						}
					}
					break;
				case 11:
				{
					if (!(actionType == "MagicalSeal") || Game1.player.mailReceived.Contains("krobusUnseal"))
					{
						break;
					}
					Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
					Game1.player.yVelocity = 0f;
					Game1.player.Halt();
					Game1.player.TemporaryPassableTiles.Clear();
					if (Game1.player.Tile == this.lastTouchActionLocation)
					{
						if (Game1.player.position.Y > this.lastTouchActionLocation.Y * 64f + 32f)
						{
							Game1.player.position.Y += 4f;
						}
						else
						{
							Game1.player.position.Y -= 4f;
						}
						this.lastTouchActionLocation = Vector2.Zero;
					}
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_MagicSeal"));
					for (int i = 0; i < 40; i++)
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2(-8 + i % 4 * 16, -(i / 4) * 64 / 4), flicker: false, flipped: false)
						{
							layerDepth = 0.1152f + (float)i / 10000f,
							color = new Color(100 + i * 4, i * 5, 120 + i * 4),
							pingPong = true,
							delayBeforeAnimationStart = i * 10,
							scale = 4f,
							alphaFade = 0.01f
						});
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 17f) * 64f + new Vector2(-8 + i % 4 * 16, i / 4 * 64 / 4), flicker: false, flipped: false)
						{
							layerDepth = 0.1152f + (float)i / 10000f,
							color = new Color(232 - i * 4, 192 - i * 6, 255 - i * 4),
							pingPong = true,
							delayBeforeAnimationStart = 320 + i * 10,
							scale = 4f,
							alphaFade = 0.01f
						});
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 25f, 4, 2, new Vector2(3f, 19f) * 64f + new Vector2(-8 + i % 4 * 16, -(i / 4) * 64 / 4), flicker: false, flipped: false)
						{
							layerDepth = 0.1152f + (float)i / 10000f,
							color = new Color(100 + i * 4, i * 6, 120 + i * 4),
							pingPong = true,
							delayBeforeAnimationStart = 640 + i * 10,
							scale = 4f,
							alphaFade = 0.01f
						});
					}
					Game1.player.jitterStrength = 2f;
					Game1.player.freezePause = 500;
					this.playSound("debuffHit");
					break;
				}
				case 15:
				{
					if (!(actionType == "ConditionalDoor") || action.Length <= 1 || Game1.eventUp || GameStateQuery.CheckConditions(ArgUtility.UnsplitQuoteAware(action, ' ', 1)))
					{
						break;
					}
					Game1.player.Position -= Game1.player.getMostRecentMovementVector() * 2f;
					Game1.player.yVelocity = 0f;
					Game1.player.Halt();
					Game1.player.TemporaryPassableTiles.Clear();
					if (Game1.player.Tile == this.lastTouchActionLocation)
					{
						if (Game1.player.Position.Y > this.lastTouchActionLocation.Y * 64f + 32f)
						{
							Game1.player.position.Y += 4f;
						}
						else
						{
							Game1.player.position.Y -= 4f;
						}
						this.lastTouchActionLocation = Vector2.Zero;
					}
					string message = this.doesTileHaveProperty((int)playerStandingPosition.X / 64, (int)playerStandingPosition.Y / 64, "LockedDoorMessage", "Back");
					if (message != null)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString(TokenParser.ParseText(message)));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
					}
					break;
				}
				case 13:
					if (actionType == "FaceDirection")
					{
						if (!ArgUtility.TryGet(action, 1, out var npcName2, out error, allowBlank: true, "string npcName") || !ArgUtility.TryGetInt(action, 2, out var direction, out error, "int direction"))
						{
							LogError(error);
						}
						else
						{
							this.getCharacterFromName(npcName2)?.faceDirection(direction);
						}
					}
					break;
				case 14:
					if (!(actionType == "legendarySword"))
					{
						break;
					}
					if (Game1.player.ActiveObject?.QualifiedItemId == "(O)74" && !Game1.player.mailReceived.Contains("galaxySword"))
					{
						Game1.player.Halt();
						Game1.player.faceDirection(2);
						Game1.player.showCarrying();
						Game1.player.jitterStrength = 1f;
						Game1.pauseThenDoFunction(7000, getGalaxySword);
						Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.Event);
						this.playSound("crit");
						Game1.screenGlowOnce(new Color(30, 0, 150), hold: true, 0.01f, 0.999f);
						DelayedAction.playSoundAfterDelay("stardrop", 1500);
						Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							Game1.stopMusicTrack(MusicContext.Event);
						});
					}
					else if (!Game1.player.mailReceived.Contains("galaxySword"))
					{
						this.localSound("SpringBirds");
					}
					break;
				case 10:
					if (actionType == "MensLocker" && !Game1.player.IsMale)
					{
						Game1.player.position.Y += ((float)Game1.player.Speed + Game1.player.addedSpeed) * 2f;
						Game1.player.Halt();
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
					}
					break;
				case 18:
					if (actionType == "ChangeIntoSwimsuit")
					{
						Game1.player.changeIntoSwimsuit();
					}
					break;
				case 19:
					if (actionType == "ChangeOutOfSwimsuit")
					{
						Game1.player.changeOutOfSwimSuit();
					}
					break;
				case 6:
				case 7:
				case 16:
				case 17:
					break;
				}
			}
		}
		catch (Exception)
		{
		}
		void LogError(string errorPhrase)
		{
			this.LogTileTouchActionError(action, playerStandingPosition, errorPhrase);
		}
	}

	public virtual void updateMap()
	{
		if (this._mapPathDirty)
		{
			this._mapPathDirty = false;
			if (!string.Equals(this.mapPath.Value, this.loadedMapPath, StringComparison.Ordinal))
			{
				this.reloadMap();
				this.updateLayout();
			}
		}
	}

	public virtual void updateLayout()
	{
		if (Game1.IsMasterGame)
		{
			this.updateDoors();
			this.updateWarps();
		}
	}

	public LargeTerrainFeature getLargeTerrainFeatureAt(int tileX, int tileY)
	{
		foreach (LargeTerrainFeature ltf in this.largeTerrainFeatures)
		{
			if (ltf.getBoundingBox().Contains(tileX * 64 + 32, tileY * 64 + 32))
			{
				return ltf;
			}
		}
		return null;
	}

	public virtual void UpdateWhenCurrentLocation(GameTime time)
	{
		this.updateMap();
		if (this.wasUpdated)
		{
			return;
		}
		this.wasUpdated = true;
		if (this._mapSeatsDirty)
		{
			this.UpdateMapSeats();
		}
		this.furnitureToRemove.Update(this);
		if (Game1.player.currentLocation.Equals(this))
		{
			this._updateAmbientLighting();
		}
		for (int i = 0; i < this.furniture.Count; i++)
		{
			this.furniture[i].updateWhenCurrentLocation(time);
		}
		AmbientLocationSounds.update(time);
		this.critters?.RemoveAll((Critter critter) => critter.update(time, this));
		if (this.fishSplashAnimation != null)
		{
			this.fishSplashAnimation.update(time);
			bool frenzy = this.fishFrenzyFish.Value != null && !this.fishFrenzyFish.Value.Equals("");
			double rate = (frenzy ? 0.1 : 0.02);
			ICue cue;
			if (Game1.random.NextDouble() < rate)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite(0, this.fishSplashAnimation.position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), Color.White * 0.3f)
				{
					layerDepth = (this.fishSplashAnimation.position.Y - 64f) / 10000f
				});
				if (frenzy)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite(0, this.fishSplashAnimation.position + new Vector2(Game1.random.Next(-64, 64), Game1.random.Next(-64, 64)), Color.White * 0.3f)
					{
						layerDepth = (this.fishSplashAnimation.position.Y - 64f) / 10000f
					});
					if (Game1.random.NextDouble() < 0.1)
					{
						Game1.sounds.PlayLocal("slosh", this, this.fishSplashAnimation.Position / 64f, null, SoundContext.Default, out cue);
					}
				}
			}
			if (frenzy && Game1.random.NextDouble() < 0.005)
			{
				Vector2 position = this.fishSplashAnimation.position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32));
				Action<Vector2> splashAnimation = delegate(Vector2 pos)
				{
					this.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, pos, flicker: false, flipped: false)
					{
						delayBeforeAnimationStart = 0,
						layerDepth = (pos.Y + 1f) / 10000f
					});
				};
				Game1.sounds.PlayLocal("slosh", this, this.fishSplashAnimation.Position / 64f, null, SoundContext.Default, out cue);
				splashAnimation(position);
				ParsedItemData fishData = ItemRegistry.GetData(this.fishFrenzyFish.Value);
				int spriteID = 982648 + Game1.random.Next(99999);
				bool flip = Game1.random.NextDouble() < 0.5;
				float intensity = (float)Game1.random.Next(10, 20) / 10f;
				if (Game1.random.NextDouble() < 0.9)
				{
					intensity *= 0.75f;
				}
				this.TemporarySprites.Add(new TemporaryAnimatedSprite(fishData.GetTextureName(), fishData.GetSourceRect(), position, flip, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2((float)((!flip) ? 1 : (-1)) * ((float)Game1.random.Next(11) * intensity + intensity * 5f) / 20f, (0f - (float)Game1.random.Next(30, 41) * intensity) / 10f),
					acceleration = new Vector2(0f, 0.1f),
					rotationChange = (float)((!flip) ? 1 : (-1)) * ((float)Game1.random.Next(5, 10) * intensity) / 800f,
					yStopCoordinate = (int)position.Y + 1,
					id = spriteID,
					layerDepth = position.Y / 10000f,
					reachedStopCoordinateSprite = delegate(TemporaryAnimatedSprite x)
					{
						this.removeTemporarySpritesWithID(spriteID);
						Game1.sounds.PlayLocal("dropItemInWater", this, position / 64f, null, SoundContext.Default, out var _);
						splashAnimation(x.Position);
					}
				});
			}
		}
		if (this.orePanAnimation != null)
		{
			this.orePanAnimation.update(time);
			if (Game1.random.NextDouble() < 0.05)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), this.orePanAnimation.position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flipped: false, 0.02f, Color.White * 0.8f)
				{
					scale = 2f,
					animationLength = 6,
					interval = 100f
				});
			}
		}
		this.interiorDoors.Update(time);
		this.updateWater(time);
		this.Map.Update(time.ElapsedGameTime.Milliseconds);
		this.debris.RemoveWhere((Debris d) => d.updateChunks(time, this));
		if (Game1.shouldTimePass() || Game1.isFestival())
		{
			this.projectiles.RemoveWhere((Projectile projectile) => projectile.update(time, this));
		}
		for (int i2 = this._activeTerrainFeatures.Count - 1; i2 >= 0; i2--)
		{
			TerrainFeature feature = this._activeTerrainFeatures[i2];
			if (feature.tickUpdate(time))
			{
				this.terrainFeatures.Remove(feature.Tile);
			}
		}
		this.largeTerrainFeatures?.RemoveWhere((LargeTerrainFeature largeTerrainFeature) => largeTerrainFeature.tickUpdate(time));
		foreach (ResourceClump resourceClump in this.resourceClumps)
		{
			resourceClump.tickUpdate(time);
		}
		if (this.currentEvent != null)
		{
			bool continue_execution;
			do
			{
				int last_command_index = this.currentEvent.CurrentCommand;
				this.currentEvent.Update(this, time);
				if (this.currentEvent != null)
				{
					continue_execution = this.currentEvent.simultaneousCommand;
					if (last_command_index == this.currentEvent.CurrentCommand)
					{
						continue_execution = false;
					}
				}
				else
				{
					continue_execution = false;
				}
			}
			while (continue_execution);
		}
		this.objects.Lock();
		foreach (Object value in this.objects.Values)
		{
			value.updateWhenCurrentLocation(time);
		}
		this.objects.Unlock();
		Vector2 player_position;
		if (Game1.gameMode == 3 && this == Game1.currentLocation)
		{
			if (Game1.currentLocation.GetLocationContext().PlayRandomAmbientSounds && this.isOutdoors.Value)
			{
				if (!this.IsRainingHere())
				{
					if (Game1.timeOfDay < 2000)
					{
						if (Game1.isMusicContextActiveButNotPlaying() && !this.IsWinterHere() && Game1.random.NextDouble() < 0.002)
						{
							this.localSound("SpringBirds");
						}
					}
					else if (Game1.timeOfDay > 2100 && !(this is Beach) && this.IsSummerHere() && !this.IsTemporary && Game1.random.NextDouble() < 0.0005)
					{
						this.localSound("crickets");
					}
				}
				else if (!Game1.eventUp && Game1.options.musicVolumeLevel > 0f && Game1.random.NextDouble() < 0.00015 && !this.name.Equals("Town"))
				{
					this.localSound("rainsound");
				}
			}
			Vector2 playerTile = Game1.player.Tile;
			if (this.lastTouchActionLocation.Equals(Vector2.Zero))
			{
				string touchActionProperty = this.doesTileHaveProperty((int)playerTile.X, (int)playerTile.Y, "TouchAction", "Back");
				this.lastTouchActionLocation = playerTile;
				if (touchActionProperty != null)
				{
					this.performTouchAction(touchActionProperty, playerTile);
				}
			}
			else if (!this.lastTouchActionLocation.Equals(playerTile))
			{
				this.lastTouchActionLocation = Vector2.Zero;
			}
			foreach (Farmer farmer in this.farmers)
			{
				Vector2 playerPos = farmer.Tile;
				Vector2[] directionsTileVectorsWithDiagonals = Utility.DirectionsTileVectorsWithDiagonals;
				for (int num = 0; num < directionsTileVectorsWithDiagonals.Length; num++)
				{
					Vector2 offset = directionsTileVectorsWithDiagonals[num];
					Vector2 v = playerPos + offset;
					if (this.objects.TryGetValue(v, out var obj))
					{
						obj.farmerAdjacentAction(farmer, offset.X != 0f && offset.Y != 0f);
					}
				}
			}
			if (Game1.player != null)
			{
				int direction = Game1.player.facingDirection.Value;
				player_position = Game1.player.Tile;
				Object sign = null;
				if (direction >= 0 && direction < 4)
				{
					Vector2 offset2 = Utility.DirectionsTileVectors[direction];
					sign = CheckForSign((int)offset2.X, (int)offset2.Y);
				}
				if (sign == null)
				{
					sign = CheckForSign(0, -1) ?? CheckForSign(0, 1) ?? CheckForSign(-1, 0) ?? CheckForSign(1, 0) ?? CheckForSign(-1, -1) ?? CheckForSign(1, -1) ?? CheckForSign(-1, 1) ?? CheckForSign(1, 1);
				}
				if (sign != null)
				{
					sign.shouldShowSign = true;
				}
			}
		}
		foreach (KeyValuePair<long, FarmAnimal> kvp in this.animals.Pairs)
		{
			this.tempAnimals.Add(kvp);
		}
		foreach (KeyValuePair<long, FarmAnimal> kvp2 in this.tempAnimals)
		{
			if (kvp2.Value.updateWhenCurrentLocation(time, this))
			{
				this.animals.Remove(kvp2.Key);
			}
		}
		this.tempAnimals.Clear();
		foreach (Building building in this.buildings)
		{
			building.Update(time);
		}
		Object CheckForSign(int offsetX, int offsetY)
		{
			if (!this.objects.TryGetValue(player_position + new Vector2(offsetX, offsetY), out var tileObject) || !tileObject.IsTextSign())
			{
				return null;
			}
			return tileObject;
		}
	}

	public void updateWater(GameTime time)
	{
		this.waterAnimationTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.waterAnimationTimer <= 0)
		{
			this.waterAnimationIndex = (this.waterAnimationIndex + 1) % 10;
			this.waterAnimationTimer = 200;
		}
		this.waterPosition += ((!this.isFarm.Value) ? ((float)((Math.Sin((float)time.TotalGameTime.Milliseconds / 1000f) + 1.0) * 0.15000000596046448)) : 0.1f);
		if (this.waterPosition >= 64f)
		{
			this.waterPosition -= 64f;
			this.waterTileFlip = !this.waterTileFlip;
		}
	}

	public NPC getCharacterFromName(string name)
	{
		NPC character = null;
		foreach (NPC n in this.characters)
		{
			if (n.Name.Equals(name))
			{
				return n;
			}
		}
		return character;
	}

	protected virtual void updateCharacters(GameTime time)
	{
		bool shouldTimePass = Game1.shouldTimePass();
		for (int i = this.characters.Count - 1; i >= 0; i--)
		{
			NPC character = this.characters[i];
			if (character != null && (shouldTimePass || character is Horse || character.forceUpdateTimer > 0))
			{
				character.currentLocation = this;
				character.update(time, this);
				if (i < this.characters.Count && character is Monster monster && monster.ShouldMonsterBeRemoved())
				{
					this.characters.RemoveAt(i);
				}
			}
			else if (character != null)
			{
				if (character.hasJustStartedFacingPlayer)
				{
					character.updateFaceTowardsFarmer(time, this);
				}
				character.updateEmote(time);
			}
		}
	}

	public Projectile getProjectileFromID(int uniqueID)
	{
		foreach (Projectile p in this.projectiles)
		{
			if (p.uniqueID.Value == uniqueID)
			{
				return p;
			}
		}
		return null;
	}

	public virtual void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
	{
		this.netAudio.Update();
		this.removeTemporarySpritesWithIDEvent.Poll();
		this.rumbleAndFadeEvent.Poll();
		this.damagePlayersEvent.Poll();
		if (!ignoreWasUpdatedFlush)
		{
			this.wasUpdated = false;
		}
		this.updateCharacters(time);
		for (int i = this.temporarySprites.Count - 1; i >= 0; i--)
		{
			TemporaryAnimatedSprite sprite = ((i < this.temporarySprites.Count) ? this.temporarySprites[i] : null);
			if (i < this.temporarySprites.Count && sprite != null && sprite.update(time) && i < this.temporarySprites.Count)
			{
				this.temporarySprites.RemoveAt(i);
			}
		}
		foreach (Building building in this.buildings)
		{
			building.updateWhenFarmNotCurrentLocation(time);
		}
		if (!Game1.currentLocation.Equals(this) && this.animals.Length > 0)
		{
			Building containingBuilding = this.ParentBuilding;
			FarmAnimal[] array = this.animals.Values.ToArray();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].updateWhenNotCurrentLocation(containingBuilding, time, this);
			}
		}
	}

	/// <summary>Get the location which contains this one, if applicable.</summary>
	/// <remarks>
	///   <para>For example, the interior for a farm building will have the farm as its root location.</para>
	///   <para>See also <see cref="M:StardewValley.GameLocation.GetRootLocation" />.</para>
	/// </remarks>
	public GameLocation GetParentLocation()
	{
		if (this.parentLocationName.Value == null)
		{
			return null;
		}
		return Game1.getLocationFromName(this.parentLocationName.Value);
	}

	/// <summary>Get the parent location which contains this one, or the current location if it has no parent.</summary>
	/// <remarks>See also <see cref="M:StardewValley.GameLocation.GetParentLocation" />.</remarks>
	public GameLocation GetRootLocation()
	{
		return this.GetParentLocation() ?? this;
	}

	public Response[] createYesNoResponses()
	{
		return new Response[2]
		{
			new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
			new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
		};
	}

	public virtual void customQuestCompleteBehavior(string questId)
	{
	}

	public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey)
	{
		this.lastQuestionKey = dialogKey;
		Game1.drawObjectQuestionDialogue(question, answerChoices);
	}

	public void createQuestionDialogueWithCustomWidth(string question, Response[] answerChoices, string dialogKey)
	{
		int width = SpriteText.getWidthOfString(question) + 64;
		this.lastQuestionKey = dialogKey;
		Game1.drawObjectQuestionDialogue(question, answerChoices, width);
	}

	public void createQuestionDialogue(string question, Response[] answerChoices, afterQuestionBehavior afterDialogueBehavior, NPC speaker = null)
	{
		this.lastQuestionKey = null;
		this.afterQuestion = afterDialogueBehavior;
		Game1.drawObjectQuestionDialogue(question, answerChoices);
		if (speaker != null)
		{
			Game1.objectDialoguePortraitPerson = speaker;
		}
	}

	public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey, Object actionObject)
	{
		this.lastQuestionKey = dialogKey;
		Game1.drawObjectQuestionDialogue(question, answerChoices);
		this.actionObjectForQuestionDialogue = actionObject;
	}

	public virtual void monsterDrop(Monster monster, int x, int y, Farmer who)
	{
		IList<string> objects = monster.objectsToDrop;
		Vector2 playerPosition = Utility.PointToVector2(who.StandingPixel);
		List<Item> extraDrops = monster.getExtraDropItems();
		if (who.isWearingRing("526") && DataLoader.Monsters(Game1.content).TryGetValue(monster.Name, out var result))
		{
			string[] objectsSplit = ArgUtility.SplitBySpace(result.Split('/')[6]);
			for (int i = 0; i < objectsSplit.Length; i += 2)
			{
				if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
				{
					objects.Add(objectsSplit[i]);
				}
			}
		}
		List<Debris> debrisToAdd = new List<Debris>();
		for (int j = 0; j < objects.Count; j++)
		{
			string objectToAdd = objects[j];
			if (objectToAdd != null && objectToAdd.StartsWith('-') && int.TryParse(objectToAdd, out var parsedIndex))
			{
				debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(Math.Abs(parsedIndex), Game1.random.Next(1, 4), new Vector2(x, y), playerPosition)));
			}
			else
			{
				debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(objectToAdd, new Vector2(x, y), playerPosition)));
			}
		}
		for (int k = 0; k < extraDrops.Count; k++)
		{
			debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(extraDrops[k], new Vector2(x, y), playerPosition)));
		}
		Trinket.TrySpawnTrinket(this, monster, monster.getStandingPosition());
		if (who.isWearingRing("526"))
		{
			extraDrops = monster.getExtraDropItems();
			for (int l = 0; l < extraDrops.Count; l++)
			{
				Item tmp = extraDrops[l].getOne();
				tmp.Stack = extraDrops[l].Stack;
				tmp.HasBeenInInventory = false;
				debrisToAdd.Add(monster.ModifyMonsterLoot(new Debris(tmp, new Vector2(x, y), playerPosition)));
			}
		}
		foreach (Debris d in debrisToAdd)
		{
			this.debris.Add(d);
		}
		if (who.stats.Get("Book_Void") != 0 && Game1.random.NextDouble() < 0.03 && debrisToAdd != null && monster != null)
		{
			foreach (Debris d2 in debrisToAdd)
			{
				if (d2.item != null)
				{
					Item tmp2 = d2.item.getOne();
					if (tmp2 != null)
					{
						tmp2.Stack = d2.item.Stack;
						tmp2.HasBeenInInventory = false;
						this.debris.Add(monster.ModifyMonsterLoot(new Debris(tmp2, new Vector2(x, y), playerPosition)));
					}
				}
				else if (d2.itemId.Value != null && d2.itemId.Value.Length > 0)
				{
					Item tmp3 = ItemRegistry.Create(d2.itemId.Value);
					tmp3.HasBeenInInventory = false;
					this.debris.Add(monster.ModifyMonsterLoot(new Debris(tmp3, new Vector2(x, y), playerPosition)));
				}
			}
		}
		if (this.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.033)
		{
			Object o = this.tryToCreateUnseenSecretNote(who);
			if (o != null)
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(o, new Vector2(x, y), -1, this));
			}
		}
		Utility.trySpawnRareObject(who, new Vector2(x, y), this, 1.5);
		if (Utility.tryRollMysteryBox(0.01 + who.team.AverageDailyLuck() / 10.0 + (double)who.LuckLevel * 0.008))
		{
			monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create((who.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(x, y), -1, this));
		}
		if (who.stats.MonstersKilled > 10 && Game1.random.NextDouble() < 0.0001 + ((!who.mailReceived.Contains("voidBookDropped")) ? ((double)who.stats.MonstersKilled * 1.5E-05) : 0.0004))
		{
			monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)Book_Void"), new Vector2(x, y), -1, this));
			who.mailReceived.Add("voidBookDropped");
		}
		if (this is Woods && Game1.random.NextDouble() < 0.1)
		{
			monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)292"), new Vector2(x, y), -1, this));
		}
		if (Game1.netWorldState.Value.GoldenWalnutsFound >= 100)
		{
			if (monster.isHardModeMonster.Value && Game1.stats.Get("hardModeMonstersKilled") > 50 && Game1.random.NextDouble() < 0.001 + (double)((float)who.LuckLevel * 0.0002f))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)896"), new Vector2(x, y), -1, this));
			}
			else if (monster.isHardModeMonster.Value && Game1.random.NextDouble() < 0.008 + (double)((float)who.LuckLevel * 0.002f))
			{
				monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)858"), new Vector2(x, y), -1, this));
			}
		}
	}

	public virtual bool HasUnlockedAreaSecretNotes(Farmer who)
	{
		if (!this.InIslandContext())
		{
			return who.hasMagnifyingGlass;
		}
		return true;
	}

	public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, Farmer who, bool isProjectile = false)
	{
		return this.damageMonster(areaOfEffect, minDamage, maxDamage, isBomb, 1f, 0, 0f, 1f, triggerMonsterInvincibleTimer: false, who, isProjectile);
	}

	private bool isMonsterDamageApplicable(Farmer who, Monster monster, bool horizontalBias = true)
	{
		if (!monster.isGlider.Value && !(who.CurrentTool is Slingshot) && !monster.ignoreDamageLOS.Value)
		{
			Point farmerStandingPoint = who.TilePoint;
			Point monsterStandingPoint = monster.TilePoint;
			if (Math.Abs(farmerStandingPoint.X - monsterStandingPoint.X) + Math.Abs(farmerStandingPoint.Y - monsterStandingPoint.Y) > 1)
			{
				int xDif = monsterStandingPoint.X - farmerStandingPoint.X;
				int yDif = monsterStandingPoint.Y - farmerStandingPoint.Y;
				Vector2 pointInQuestion = new Vector2(farmerStandingPoint.X, farmerStandingPoint.Y);
				while (xDif != 0 || yDif != 0)
				{
					if (horizontalBias)
					{
						if (Math.Abs(xDif) >= Math.Abs(yDif))
						{
							pointInQuestion.X += Math.Sign(xDif);
							xDif -= Math.Sign(xDif);
						}
						else
						{
							pointInQuestion.Y += Math.Sign(yDif);
							yDif -= Math.Sign(yDif);
						}
					}
					else if (Math.Abs(yDif) >= Math.Abs(xDif))
					{
						pointInQuestion.Y += Math.Sign(yDif);
						yDif -= Math.Sign(yDif);
					}
					else
					{
						pointInQuestion.X += Math.Sign(xDif);
						xDif -= Math.Sign(xDif);
					}
					if ((this.objects.TryGetValue(pointInQuestion, out var obj) && !obj.isPassable()) || this.BlocksDamageLOS((int)pointInQuestion.X, (int)pointInQuestion.Y))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public virtual bool BlocksDamageLOS(int x, int y)
	{
		if (this.hasTileAt(x, y, "Buildings") && this.doesTileHaveProperty(x, y, "Passable", "Buildings") == null)
		{
			return true;
		}
		return false;
	}

	public bool damageMonster(Microsoft.Xna.Framework.Rectangle areaOfEffect, int minDamage, int maxDamage, bool isBomb, float knockBackModifier, int addedPrecision, float critChance, float critMultiplier, bool triggerMonsterInvincibleTimer, Farmer who, bool isProjectile = false)
	{
		bool didAnyDamage = false;
		for (int j = this.characters.Count - 1; j >= 0; j--)
		{
			if (j < this.characters.Count && this.characters[j] is Monster { IsMonster: not false, Health: >0 } monster && monster.TakesDamageFromHitbox(areaOfEffect))
			{
				if (monster.currentLocation == null)
				{
					monster.currentLocation = this;
				}
				if (!monster.IsInvisible && !monster.isInvincible() && (isBomb || isProjectile || this.isMonsterDamageApplicable(who, monster) || this.isMonsterDamageApplicable(who, monster, horizontalBias: false)))
				{
					bool isDagger = !isBomb && who?.CurrentTool is MeleeWeapon weapon && weapon.type.Value == 1;
					bool isDaggerSpecial = false;
					if (isDagger && MeleeWeapon.daggerHitsLeft > 1)
					{
						isDaggerSpecial = true;
					}
					if (isDaggerSpecial)
					{
						triggerMonsterInvincibleTimer = false;
					}
					didAnyDamage = true;
					if (Game1.currentLocation == this)
					{
						Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 8.0), 200 + Game1.random.Next(-50, 50));
					}
					Microsoft.Xna.Framework.Rectangle monsterBox = monster.GetBoundingBox();
					Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(monsterBox, who);
					if (knockBackModifier > 0f)
					{
						trajectory *= knockBackModifier;
					}
					else
					{
						trajectory = new Vector2(monster.xVelocity, monster.yVelocity);
					}
					if (monster.Slipperiness == -1)
					{
						trajectory = Vector2.Zero;
					}
					bool crit = false;
					if (who?.CurrentTool != null && monster.hitWithTool(who.CurrentTool))
					{
						return false;
					}
					if (who.hasBuff("statue_of_blessings_5"))
					{
						critChance += 0.1f;
					}
					if (who.professions.Contains(25))
					{
						critChance += critChance * 0.5f;
					}
					int damageAmount;
					if (maxDamage >= 0)
					{
						damageAmount = Game1.random.Next(minDamage, maxDamage + 1);
						if (who != null && Game1.random.NextDouble() < (double)(critChance + (float)who.LuckLevel * (critChance / 40f)))
						{
							crit = true;
							this.playSound("crit");
							if (who.hasTrinketWithID("IridiumSpur"))
							{
								BuffEffects b = new BuffEffects();
								b.Speed.Value = 1f;
								who.applyBuff(new Buff("iridiumspur", null, Game1.content.LoadString("Strings\\1_6_Strings:IridiumSpur_Name"), who.getFirstTrinketWithID("IridiumSpur").GetEffect().GeneralStat * 1000, Game1.objectSpriteSheet_2, 76, b, false));
							}
						}
						damageAmount = (crit ? ((int)((float)damageAmount * critMultiplier)) : damageAmount);
						damageAmount = Math.Max(1, damageAmount + ((who != null) ? (who.Attack * 3) : 0));
						if (who != null && who.professions.Contains(24))
						{
							damageAmount = (int)Math.Ceiling((float)damageAmount * 1.1f);
						}
						if (who != null && who.professions.Contains(26))
						{
							damageAmount = (int)Math.Ceiling((float)damageAmount * 1.15f);
						}
						if (who != null && crit && who.professions.Contains(29))
						{
							damageAmount = (int)((float)damageAmount * 2f);
						}
						if (who != null)
						{
							foreach (BaseEnchantment enchantment in who.enchantments)
							{
								enchantment.OnCalculateDamage(monster, this, who, isBomb, ref damageAmount);
							}
						}
						damageAmount = monster.takeDamage(damageAmount, (int)trajectory.X, (int)trajectory.Y, isBomb, (double)addedPrecision / 10.0, who);
						if (isDaggerSpecial)
						{
							if (monster.stunTime.Value < 50)
							{
								monster.stunTime.Value = 50;
							}
						}
						else if (monster.stunTime.Value < 50)
						{
							monster.stunTime.Value = 0;
						}
						if (damageAmount == -1)
						{
							string missText = Game1.content.LoadString("Strings\\StringsFromCSFiles:Attack_Miss");
							this.debris.Add(new Debris(missText, 1, new Vector2(monsterBox.Center.X, monsterBox.Center.Y), Color.LightGray, 1f, 0f));
						}
						else
						{
							this.removeDamageDebris(monster);
							this.debris.Add(new Debris(damageAmount, new Vector2(monsterBox.Center.X + 16, monsterBox.Center.Y), crit ? Color.Yellow : new Color(255, 130, 0), crit ? (1f + (float)damageAmount / 300f) : 1f, monster));
							if (who != null)
							{
								foreach (BaseEnchantment enchantment2 in who.enchantments)
								{
									enchantment2.OnDealtDamage(monster, this, who, isBomb, damageAmount);
								}
							}
						}
						if (triggerMonsterInvincibleTimer)
						{
							monster.setInvincibleCountdown(450 / (isDagger ? 3 : 2));
						}
						if (who != null)
						{
							foreach (Trinket trinketItem in who.trinketItems)
							{
								trinketItem?.OnDamageMonster(who, monster, damageAmount, isBomb, crit);
							}
						}
					}
					else
					{
						damageAmount = -2;
						monster.setTrajectory(trajectory);
						if (monster.Slipperiness > 10)
						{
							monster.xVelocity /= 2f;
							monster.yVelocity /= 2f;
						}
					}
					if (who?.CurrentTool?.QualifiedItemId == "(W)4")
					{
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(50, 120), 6, 1, new Vector2(monsterBox.Center.X - 32, monsterBox.Center.Y - 32), flicker: false, flipped: false));
					}
					if (monster.Health <= 0)
					{
						this.onMonsterKilled(who, monster, monsterBox, isBomb);
					}
					else if (damageAmount > 0)
					{
						monster.shedChunks(Game1.random.Next(1, 3));
						if (crit)
						{
							Vector2 standPos = monster.getStandingPosition();
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32f, 32f), flicker: false, Game1.random.NextBool())
							{
								scale = 0.75f,
								alpha = (crit ? 0.75f : 0.5f)
							});
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextBool())
							{
								scale = 0.5f,
								delayBeforeAnimationStart = 50,
								alpha = (crit ? 0.75f : 0.5f)
							});
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextBool())
							{
								scale = 0.5f,
								delayBeforeAnimationStart = 100,
								alpha = (crit ? 0.75f : 0.5f)
							});
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) + 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextBool())
							{
								scale = 0.5f,
								delayBeforeAnimationStart = 150,
								alpha = (crit ? 0.75f : 0.5f)
							});
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(362, Game1.random.Next(15, 50), 6, 1, standPos - new Vector2(32 + Game1.random.Next(-21, 21) - 32, 32 + Game1.random.Next(-21, 21)), flicker: false, Game1.random.NextBool())
							{
								scale = 0.5f,
								delayBeforeAnimationStart = 200,
								alpha = (crit ? 0.75f : 0.5f)
							});
						}
					}
				}
			}
		}
		return didAnyDamage;
	}

	/// <summary>Handle a monster reaching zero health after being hit by the player.</summary>
	/// <param name="who">The player who damaged the monster.</param>
	/// <param name="monster">The monster whose health reached zero.</param>
	/// <param name="monsterBox">The monster's pixel hitbox.</param>
	/// <param name="killedByBomb">Whether the monster was killed by a bomb placed by the player.</param>
	private void onMonsterKilled(Farmer who, Monster monster, Microsoft.Xna.Framework.Rectangle monsterBox, bool killedByBomb)
	{
		bool isHutchSlime = false;
		bool isBabySlime = false;
		if (monster is GreenSlime slime)
		{
			isHutchSlime = this is SlimeHutch;
			isBabySlime = !slime.firstGeneration.Value;
		}
		who.NotifyQuests((Quest quest) => quest.OnMonsterSlain(this, monster, killedByBomb, isHutchSlime));
		if (!isHutchSlime && Game1.player.team.specialOrders != null)
		{
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				specialOrder.onMonsterSlain?.Invoke(Game1.player, monster);
			}
		}
		if (who != null)
		{
			foreach (BaseEnchantment enchantment in who.enchantments)
			{
				enchantment.OnMonsterSlay(monster, this, who, killedByBomb);
			}
		}
		who?.leftRing.Value?.onMonsterSlay(monster, this, who);
		who?.rightRing.Value?.onMonsterSlay(monster, this, who);
		if (who != null && !isHutchSlime && !isBabySlime)
		{
			if (who.IsLocalPlayer)
			{
				Game1.stats.monsterKilled(monster.Name);
			}
			else if (Game1.IsMasterGame)
			{
				who.queueMessage(25, Game1.player, monster.Name);
			}
		}
		if (monster.isHardModeMonster.Value)
		{
			Game1.stats.Increment("hardModeMonstersKilled");
		}
		Game1.stats.MonstersKilled++;
		this.monsterDrop(monster, monsterBox.Center.X, monsterBox.Center.Y, who);
		if (!isHutchSlime)
		{
			who?.gainExperience(4, this.isFarm.Value ? Math.Max(1, monster.ExperienceGained / 3) : monster.ExperienceGained);
		}
		if (monster.ShouldMonsterBeRemoved())
		{
			this.characters.Remove(monster);
		}
		this.removeTemporarySpritesWithID((int)(monster.position.X * 777f + monster.position.Y * 77777f));
		if (who?.CurrentTool is MeleeWeapon weapon && (weapon.QualifiedItemId == "(W)65" || (weapon.appearance.Value != null && weapon.appearance.Value.Equals("(W)65"))))
		{
			Utility.addRainbowStarExplosion(this, new Vector2(monsterBox.Center.X - 32, monsterBox.Center.Y - 32), Game1.random.Next(6, 9));
		}
	}

	public void growWeedGrass(int iterations)
	{
		for (int i = 0; i < iterations; i++)
		{
			KeyValuePair<Vector2, TerrainFeature>[] array = this.terrainFeatures.Pairs.ToArray();
			for (int j = 0; j < array.Length; j++)
			{
				KeyValuePair<Vector2, TerrainFeature> pair = array[j];
				if (!(pair.Value is Grass grass) || !(Game1.random.NextDouble() < 0.65))
				{
					continue;
				}
				if (grass.numberOfWeeds.Value < 4)
				{
					grass.numberOfWeeds.Value = Math.Max(0, Math.Min(4, grass.numberOfWeeds.Value + Game1.random.Next(3)));
				}
				else
				{
					if (grass.numberOfWeeds.Value < 4)
					{
						continue;
					}
					int xCoord = (int)pair.Key.X;
					int yCoord = (int)pair.Key.Y;
					Vector2[] adjacentTileLocationsArray = Utility.getAdjacentTileLocationsArray(pair.Key);
					for (int k = 0; k < adjacentTileLocationsArray.Length; k++)
					{
						Vector2 tile = adjacentTileLocationsArray[k];
						if (this.isTileOnMap(xCoord, yCoord) && !this.IsTileBlockedBy(tile) && this.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null && !this.IsNoSpawnTile(tile) && Game1.random.NextDouble() < 0.25)
						{
							this.terrainFeatures.Add(tile, new Grass(grass.grassType.Value, Game1.random.Next(1, 3)));
						}
					}
				}
			}
		}
	}

	public bool tryPlaceObject(Vector2 tile, Object o)
	{
		if (this.CanItemBePlacedHere(tile))
		{
			o.initializeLightSource(tile);
			this.objects.Add(tile, o);
			return true;
		}
		return false;
	}

	public void removeDamageDebris(Monster monster)
	{
		this.debris.RemoveWhere((Debris d) => d.toHover != null && d.toHover.Equals(monster) && !d.nonSpriteChunkColor.Equals(Color.Yellow) && d.timeSinceDoneBouncing > 900f);
	}

	public void spawnWeeds(bool weedsOnly)
	{
		LocationData data = this.GetData();
		int numberOfNewWeeds = Game1.random.Next(data?.MinDailyWeeds ?? 1, (data?.MaxDailyWeeds ?? 5) + 1);
		if (Game1.dayOfMonth == 1 && Game1.IsSpring)
		{
			numberOfNewWeeds *= data?.FirstDayWeedMultiplier ?? 15;
		}
		for (int i = 0; i < numberOfNewWeeds; i++)
		{
			int numberOfTries = 0;
			while (numberOfTries < 3)
			{
				int xCoord = Game1.random.Next(this.map.DisplayWidth / 64);
				int yCoord = Game1.random.Next(this.map.DisplayHeight / 64);
				Vector2 location = new Vector2(xCoord, yCoord);
				this.objects.TryGetValue(location, out var o);
				int grass = -1;
				int tree = -1;
				if (Game1.random.NextDouble() < 0.15 + (weedsOnly ? 0.05 : 0.0))
				{
					grass = 1;
				}
				else if (!weedsOnly)
				{
					if (Game1.random.NextDouble() < 0.35)
					{
						tree = 1;
					}
					else if (!this.isFarm.Value && Game1.random.NextDouble() < 0.35)
					{
						tree = 2;
					}
				}
				if (tree != -1)
				{
					if (this is Farm && Game1.random.NextDouble() < 0.25)
					{
						return;
					}
				}
				else if (o == null && this.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null && this.isTileLocationOpen(new Location(xCoord, yCoord)) && !this.IsTileOccupiedBy(location) && !this.isWaterTile(xCoord, yCoord))
				{
					if (this.IsNoSpawnTile(location, "Grass"))
					{
						continue;
					}
					if (grass != -1 && this.GetSeason() != Season.Winter && this.name.Value == "Farm")
					{
						if (Game1.GetFarmTypeID() == "MeadowlandsFarm" && Game1.random.NextDouble() < 0.1)
						{
							grass = 7;
						}
						int numberOfWeeds = Game1.random.Next(1, 3);
						this.terrainFeatures.Add(location, new Grass(grass, numberOfWeeds));
					}
				}
				numberOfTries++;
			}
		}
	}

	public virtual void OnMiniJukeboxAdded()
	{
		this.miniJukeboxCount.Value = this.miniJukeboxCount.Value + 1;
		this.UpdateMiniJukebox();
	}

	public virtual void OnMiniJukeboxRemoved()
	{
		this.miniJukeboxCount.Value = this.miniJukeboxCount.Value - 1;
		this.UpdateMiniJukebox();
	}

	public virtual void UpdateMiniJukebox()
	{
		if (this.miniJukeboxCount.Value <= 0)
		{
			this.miniJukeboxCount.Set(0);
			this.miniJukeboxTrack.Set("");
		}
	}

	public virtual bool IsMiniJukeboxPlaying()
	{
		if (this.miniJukeboxCount.Value > 0 && this.miniJukeboxTrack.Value != "" && (!this.IsOutdoors || !this.IsRainingHere()))
		{
			return !Game1.isGreenRain;
		}
		return false;
	}

	/// <summary>Update the location state when setting up the new day, before the game saves overnight.</summary>
	/// <param name="dayOfMonth">The current day of month.</param>
	/// <remarks>See also <see cref="M:StardewValley.GameLocation.OnDayStarted" />, which happens after saving when the day has started.</remarks>
	public virtual void DayUpdate(int dayOfMonth)
	{
		this.isMusicTownMusic = null;
		this.netAudio.StopPlaying("fuse");
		this.SelectRandomMiniJukeboxTrack();
		this.critters?.Clear();
		this.characters.RemoveWhere((NPC npc) => npc is JunimoHarvester || (npc is Monster monster && monster.wildernessFarmMonster));
		FarmAnimal[] array = this.animals.Values.ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			array[num].dayUpdate(this);
		}
		for (int i = this.debris.Count - 1; i >= 0; i--)
		{
			Debris d = this.debris[i];
			if (d.isEssentialItem() && Game1.IsMasterGame)
			{
				if (d.item?.QualifiedItemId == "(O)73")
				{
					d.collect(Game1.player);
				}
				else
				{
					Item item = d.item;
					d.item = null;
					Game1.player.team.returnedDonations.Add(item);
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
				this.debris.RemoveAt(i);
			}
		}
		this.updateMap();
		this.temporarySprites.Clear();
		KeyValuePair<Vector2, TerrainFeature>[] map_features = this.terrainFeatures.Pairs.ToArray();
		KeyValuePair<Vector2, TerrainFeature>[] array2 = map_features;
		for (int num = 0; num < array2.Length; num++)
		{
			KeyValuePair<Vector2, TerrainFeature> pair = array2[num];
			if (!this.isTileOnMap(pair.Key))
			{
				this.terrainFeatures.Remove(pair.Key);
			}
			else
			{
				pair.Value.dayUpdate();
			}
		}
		array2 = map_features;
		foreach (KeyValuePair<Vector2, TerrainFeature> pair2 in array2)
		{
			if (pair2.Value is HoeDirt hoe_dirt)
			{
				hoe_dirt.updateNeighbors();
			}
		}
		if (this.largeTerrainFeatures != null)
		{
			LargeTerrainFeature[] array3 = this.largeTerrainFeatures.ToArray();
			for (int num = 0; num < array3.Length; num++)
			{
				array3[num].dayUpdate();
			}
		}
		this.objects.Lock();
		foreach (KeyValuePair<Vector2, Object> pair3 in this.objects.Pairs)
		{
			pair3.Value.DayUpdate();
			if (pair3.Value.destroyOvernight)
			{
				pair3.Value.performRemoveAction();
				this.objects.Remove(pair3.Key);
			}
		}
		this.objects.Unlock();
		this.RespawnStumpsFromMapProperty();
		if (!(this is FarmHouse))
		{
			this.debris.RemoveWhere((Debris debris) => debris.item == null && debris.itemId.Value == null);
		}
		if (this.map != null && (this.isOutdoors.Value || this.map.Properties.ContainsKey("ForceSpawnForageables")) && !this.map.Properties.ContainsKey("skipWeedGrowth"))
		{
			if (Game1.dayOfMonth % 7 == 0 && !(this is Farm))
			{
				Microsoft.Xna.Framework.Rectangle ignoreRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0);
				if (this is IslandWest)
				{
					ignoreRectangle = new Microsoft.Xna.Framework.Rectangle(31, 3, 77, 70);
				}
				KeyValuePair<Vector2, Object>[] array4 = this.objects.Pairs.ToArray();
				for (int num = 0; num < array4.Length; num++)
				{
					KeyValuePair<Vector2, Object> pair4 = array4[num];
					if (pair4.Value.isSpawnedObject.Value && pair4.Value.SpecialVariable != 724519 && !ignoreRectangle.Contains(Utility.Vector2ToPoint(pair4.Key)))
					{
						this.objects.Remove(pair4.Key);
					}
				}
				this.numberOfSpawnedObjectsOnMap = 0;
				this.spawnObjects();
				this.spawnObjects();
			}
			this.spawnObjects();
			if (Game1.dayOfMonth == 1)
			{
				this.spawnObjects();
			}
			if (Game1.stats.DaysPlayed < 4)
			{
				this.spawnObjects();
			}
			Layer pathsLayer = this.map.GetLayer("Paths");
			if (pathsLayer != null && !(this is Farm))
			{
				for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
				{
					for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
					{
						if (!this.TryGetTreeIdForTile(pathsLayer.Tiles[x, y], out var treeId, out var _, out var growthStageOnRegrow, out var isFruitTree) || !Game1.random.NextBool())
						{
							continue;
						}
						Vector2 tile = new Vector2(x, y);
						if (this.GetFurnitureAt(tile) == null && !this.terrainFeatures.ContainsKey(tile) && !this.objects.ContainsKey(tile) && this.getBuildingAt(tile) == null)
						{
							if (isFruitTree)
							{
								this.terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnRegrow ?? 2));
							}
							else
							{
								this.terrainFeatures.Add(tile, new Tree(treeId, growthStageOnRegrow ?? 2));
							}
						}
					}
				}
			}
		}
		this.terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> keyValuePair) => keyValuePair.Value is HoeDirt hoeDirt && (hoeDirt.crop == null || hoeDirt.crop.forageCrop.Value) && (!this.objects.TryGetValue(keyValuePair.Key, out var value) || value == null || !value.IsSpawnedObject || !value.isForage()) && Game1.random.NextBool(this.GetDirtDecayChance(keyValuePair.Key)));
		this.lightLevel.Value = 0f;
		foreach (Furniture item2 in this.furniture)
		{
			item2.minutesElapsed(Utility.CalculateMinutesUntilMorning(Game1.timeOfDay));
			item2.DayUpdate();
		}
		this.addLightGlows();
		if (!(this is Farm))
		{
			this.HandleGrassGrowth(dayOfMonth);
		}
		foreach (Building building2 in this.buildings)
		{
			building2.dayUpdate(dayOfMonth);
		}
		foreach (string builder in new List<string>(Game1.netWorldState.Value.Builders.Keys))
		{
			BuilderData builderData = Game1.netWorldState.Value.Builders[builder];
			if (builderData.buildingLocation.Value == this.NameOrUniqueName)
			{
				Building building = this.getBuildingAt(Utility.PointToVector2(builderData.buildingTile.Value));
				if (building == null || (building.daysUntilUpgrade.Value == 0 && building.daysOfConstructionLeft.Value == 0))
				{
					Game1.netWorldState.Value.Builders.Remove(builder);
				}
				else
				{
					Game1.netWorldState.Value.MarkUnderConstruction(builder, building);
				}
			}
		}
		if (dayOfMonth == 9 && this.Name.Equals("Backwoods"))
		{
			if (this.terrainFeatures.GetValueOrDefault(new Vector2(18f, 18f)) is HoeDirt)
			{
				this.terrainFeatures.Remove(new Vector2(18f, 18f));
			}
			this.tryPlaceObject(new Vector2(18f, 18f), ItemRegistry.Create<Object>("(O)SeedSpot"));
		}
		this.fishSplashPointTime = 0;
		this.fishFrenzyFish.Value = "";
		this.fishSplashPoint.Value = Point.Zero;
		this.orePanPoint.Value = Point.Zero;
	}

	/// <summary>Get the probability that a hoed dirt tile decays overnight, as a value between 0 (never) and 1 (always).</summary>
	/// <param name="tile">The dirt tile position.</param>
	public virtual double GetDirtDecayChance(Vector2 tile)
	{
		if (this.TryGetMapPropertyAs("DirtDecayChance", out double chance, required: false))
		{
			return chance;
		}
		if (this.IsGreenhouse)
		{
			return 0.0;
		}
		if (this is Farm || this is IslandWest || this.isFarm.Value)
		{
			return 0.1;
		}
		return 1.0;
	}

	/// <summary>If the location's map has the <c>Stumps</c> map property, respawn any missing stumps. This will destroy any objects placed on the same tile.</summary>
	public void RespawnStumpsFromMapProperty()
	{
		string[] stumpData = this.GetMapPropertySplitBySpaces("Stumps");
		for (int i = 0; i < stumpData.Length; i += 3)
		{
			if (!ArgUtility.TryGetVector2(stumpData, i, out var tile, out var error, integerOnly: false, "Vector2 tile"))
			{
				this.LogMapPropertyError("Stumps", stumpData, error);
				continue;
			}
			bool foundStump = false;
			foreach (ResourceClump resourceClump in this.resourceClumps)
			{
				if (resourceClump.Tile == tile)
				{
					foundStump = true;
					break;
				}
			}
			if (!foundStump)
			{
				this.resourceClumps.Add(new ResourceClump(600, 2, 2, tile));
				this.removeObject(tile, showDestroyedObject: false);
				this.removeObject(tile + new Vector2(1f, 0f), showDestroyedObject: false);
				this.removeObject(tile + new Vector2(1f, 1f), showDestroyedObject: false);
				this.removeObject(tile + new Vector2(0f, 1f), showDestroyedObject: false);
			}
		}
	}

	public void addLightGlows()
	{
		int night_tiles_time = Game1.getTrulyDarkTime(this) - 100;
		if (this.isOutdoors.Value || (Game1.timeOfDay >= night_tiles_time && !Game1.newDay))
		{
			return;
		}
		this.lightGlows.Clear();
		string[] split = this.GetMapPropertySplitBySpaces("DayTiles");
		for (int i = 0; i < split.Length; i += 4)
		{
			if (!ArgUtility.TryGet(split, i, out var layerId, out var error, allowBlank: true, "string layerId") || !ArgUtility.TryGetVector2(split, i + 1, out var position, out error, integerOnly: false, "Vector2 position") || !ArgUtility.TryGetInt(split, i + 3, out var tileIndex, out error, "int tileIndex"))
			{
				this.LogMapPropertyError("DayTiles", split, error);
				continue;
			}
			Tile tile = this.map.RequireLayer(layerId).Tiles[(int)position.X, (int)position.Y];
			if (tile != null)
			{
				tile.TileIndex = tileIndex;
				switch (tileIndex)
				{
				case 257:
					this.lightGlows.Add(position * 64f + new Vector2(32f, -4f));
					break;
				case 256:
					this.lightGlows.Add(position * 64f + new Vector2(32f, 64f));
					break;
				case 405:
					this.lightGlows.Add(position * 64f + new Vector2(32f, 32f));
					this.lightGlows.Add(position * 64f + new Vector2(96f, 32f));
					break;
				case 469:
					this.lightGlows.Add(position * 64f + new Vector2(32f, 36f));
					break;
				case 1224:
					this.lightGlows.Add(position * 64f + new Vector2(32f, 32f));
					break;
				}
			}
		}
	}

	public NPC isCharacterAtTile(Vector2 tileLocation)
	{
		NPC c = null;
		tileLocation.X = (int)tileLocation.X;
		tileLocation.Y = (int)tileLocation.Y;
		Microsoft.Xna.Framework.Rectangle tileBoundingBox = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		if (this.currentEvent == null)
		{
			foreach (NPC n in this.characters)
			{
				if (n.GetBoundingBox().Intersects(tileBoundingBox))
				{
					return n;
				}
			}
		}
		else
		{
			foreach (NPC n2 in this.currentEvent.actors)
			{
				if (n2.GetBoundingBox().Intersects(tileBoundingBox))
				{
					return n2;
				}
			}
		}
		return c;
	}

	public void ResetCharacterDialogues()
	{
		for (int i = this.characters.Count - 1; i >= 0; i--)
		{
			this.characters[i].resetCurrentDialogue();
		}
	}

	/// <summary>Get the value of a map property, if it's defined.</summary>
	/// <param name="propertyName">The property name to check.</param>
	/// <returns>Returns the map property value, or null if it's not set.</returns>
	public string getMapProperty(string propertyName)
	{
		if (!this.TryGetMapProperty(propertyName, out var value))
		{
			return null;
		}
		return value;
	}

	/// <summary>Get the value of a map property, if it's defined.</summary>
	/// <param name="propertyName">The property name to check.</param>
	/// <param name="propertyValue">The map property value, if it's set.</param>
	/// <returns>Returns whether the map property is set.</returns>
	public bool TryGetMapProperty(string propertyName, out string propertyValue)
	{
		Map map = this.Map;
		if (map == null)
		{
			Game1.log.Warn($"Can't read map property '{propertyName}' for location '{this.NameOrUniqueName}' because the map is null.");
			propertyValue = null;
			return false;
		}
		if (map.Properties.TryGetValue(propertyName, out propertyValue))
		{
			return propertyValue != null;
		}
		return false;
	}

	/// <summary>Get the space-delimited values defined by a map property.</summary>
	/// <param name="propertyName">The property name to read.</param>
	/// <returns>Returns the map property value, or an empty array if it's empty or unset.</returns>
	public string[] GetMapPropertySplitBySpaces(string propertyName)
	{
		if (!this.TryGetMapProperty(propertyName, out var value) || value == null)
		{
			return LegacyShims.EmptyArray<string>();
		}
		return ArgUtility.SplitBySpace(value);
	}

	/// <summary>Get a map property which defines a boolean value.</summary>
	/// <param name="key">The property name to read.</param>
	/// <param name="parsed">The parsed boolean value, if the map property was present and valid.</param>
	/// <param name="required">Whether to log an error if the map property isn't defined.</param>
	public bool TryGetMapPropertyAs(string key, out bool parsed, bool required = false)
	{
		if (!this.TryGetMapProperty(key, out var raw))
		{
			if (required)
			{
				this.LogMapPropertyError(key, "", "required map property isn't defined");
			}
			parsed = false;
			return false;
		}
		switch (raw)
		{
		case "T":
		case "t":
			parsed = true;
			return true;
		case "F":
		case "f":
			parsed = false;
			return true;
		default:
			if (bool.TryParse(raw, out parsed))
			{
				return true;
			}
			this.LogMapPropertyError(key, raw, "not a valid boolean value");
			return false;
		}
	}

	/// <summary>Get a map property which defines a space-delimited <see cref="T:System.Double" /> value.</summary>
	/// <param name="key">The property name to read.</param>
	/// <param name="parsed">The parsed value, if the map property was present and valid.</param>
	/// <param name="required">Whether to log an error if the map property isn't defined.</param>
	public bool TryGetMapPropertyAs(string key, out double parsed, bool required = false)
	{
		if (!this.TryGetMapProperty(key, out var raw))
		{
			if (required)
			{
				this.LogMapPropertyError(key, "", "required map property isn't defined");
			}
			parsed = 0.0;
			return false;
		}
		if (!double.TryParse(raw, out parsed))
		{
			this.LogMapPropertyError(key, raw, "value '" + raw + "' can't be parsed as a decimal value");
			return false;
		}
		return true;
	}

	/// <summary>Get a map property which defines a space-delimited <see cref="T:Microsoft.Xna.Framework.Point" /> position.</summary>
	/// <param name="key">The property name to read.</param>
	/// <param name="parsed">The parsed position value, if the map property was present and valid.</param>
	/// <param name="required">Whether to log an error if the map property isn't defined.</param>
	public bool TryGetMapPropertyAs(string key, out Point parsed, bool required = false)
	{
		string[] fields = this.GetMapPropertySplitBySpaces(key);
		if (fields.Length == 0)
		{
			if (required)
			{
				this.LogMapPropertyError(key, "", "required map property isn't defined");
			}
			parsed = Point.Zero;
			return false;
		}
		if (!ArgUtility.TryGetPoint(fields, 0, out parsed, out var error, "parsed"))
		{
			this.LogMapPropertyError(key, fields, error);
			parsed = Point.Zero;
			return false;
		}
		return true;
	}

	/// <summary>Get a map property which defines a space-delimited <see cref="T:Microsoft.Xna.Framework.Vector2" /> position.</summary>
	/// <param name="key">The property name to read.</param>
	/// <param name="parsed">The parsed position value, if the map property was present and valid.</param>
	/// <param name="required">Whether to log an error if the map property isn't defined.</param>
	public bool TryGetMapPropertyAs(string key, out Vector2 parsed, bool required = false)
	{
		string[] fields = this.GetMapPropertySplitBySpaces(key);
		if (fields.Length == 0)
		{
			if (required)
			{
				this.LogMapPropertyError(key, "", "required map property isn't defined");
			}
			parsed = Vector2.Zero;
			return false;
		}
		if (!ArgUtility.TryGetVector2(fields, 0, out parsed, out var error, integerOnly: false, "parsed"))
		{
			this.LogMapPropertyError(key, fields, error);
			parsed = Vector2.Zero;
			return false;
		}
		return true;
	}

	/// <summary>Get a map property which defines a space-delimited position and size.</summary>
	/// <param name="key">The property name to read.</param>
	/// <param name="parsed">The parsed position value, if the map property was present and valid.</param>
	/// <param name="required">Whether to log an error if the map property isn't defined.</param>
	public bool TryGetMapPropertyAs(string key, out Microsoft.Xna.Framework.Rectangle parsed, bool required = false)
	{
		string[] fields = this.GetMapPropertySplitBySpaces(key);
		if (fields.Length == 0)
		{
			if (required)
			{
				this.LogMapPropertyError(key, "", "required map property isn't defined");
			}
			parsed = Microsoft.Xna.Framework.Rectangle.Empty;
			return false;
		}
		if (!ArgUtility.TryGetRectangle(fields, 0, out parsed, out var error, "parsed"))
		{
			this.LogMapPropertyError(key, fields, error);
			parsed = Microsoft.Xna.Framework.Rectangle.Empty;
			return false;
		}
		return true;
	}

	/// <summary>Get whether a map property is defined and has a non-empty value.</summary>
	/// <param name="propertyName">The property name to check.</param>
	public bool HasMapPropertyWithValue(string propertyName)
	{
		if (this.map != null && this.Map.Properties.TryGetValue(propertyName, out var rawValue))
		{
			if (rawValue == null)
			{
				return false;
			}
			return rawValue.Length > 0;
		}
		return false;
	}

	public virtual void tryToAddCritters(bool onlyIfOnScreen = false)
	{
		if (Game1.CurrentEvent != null)
		{
			return;
		}
		double mapArea = this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight;
		double baseChance = Math.Max(0.15, Math.Min(0.5, mapArea / 15000.0));
		double birdieChance = baseChance;
		double butterflyChance = baseChance;
		double bunnyChance = baseChance / 2.0;
		double squirrelChance = baseChance / 2.0;
		double woodPeckerChance = baseChance / 8.0;
		double cloudChange = baseChance * 2.0;
		if (this.IsRainingHere())
		{
			return;
		}
		this.addClouds(cloudChange / (double)(onlyIfOnScreen ? 2f : 1f), onlyIfOnScreen);
		if (!(this is Beach) && this.critters != null && this.critters.Count <= (this.IsSummerHere() ? 20 : 10))
		{
			this.addBirdies(birdieChance, onlyIfOnScreen);
			this.addButterflies(butterflyChance, onlyIfOnScreen);
			this.addBunnies(bunnyChance, onlyIfOnScreen);
			this.addSquirrels(squirrelChance, onlyIfOnScreen);
			this.addWoodpecker(woodPeckerChance, onlyIfOnScreen);
			if (Game1.isDarkOut(this) && Game1.random.NextDouble() < 0.01)
			{
				this.addOwl();
			}
			if (Game1.isDarkOut(this))
			{
				this.addOpossums(baseChance / 10.0, onlyIfOnScreen);
			}
		}
	}

	public void addClouds(double chance, bool onlyIfOnScreen = false)
	{
		if (!this.IsSummerHere() || this.IsRainingHere() || Game1.weatherIcon == 4 || Game1.timeOfDay >= Game1.getStartingToGetDarkTime(this) - 100)
		{
			return;
		}
		while (Game1.random.NextDouble() < Math.Min(0.9, chance))
		{
			Vector2 v = this.getRandomTile();
			if (onlyIfOnScreen)
			{
				v = (Game1.random.NextBool() ? new Vector2(this.map.Layers[0].LayerWidth, Game1.random.Next(this.map.Layers[0].LayerHeight)) : new Vector2(Game1.random.Next(this.map.Layers[0].LayerWidth), this.map.Layers[0].LayerHeight));
			}
			if (!onlyIfOnScreen && Utility.isOnScreen(v * 64f, 1280))
			{
				continue;
			}
			Cloud cloud = new Cloud(v);
			bool freeToAdd = true;
			if (this.critters != null)
			{
				foreach (Critter c in this.critters)
				{
					if (c is Cloud && c.getBoundingBox(0, 0).Intersects(cloud.getBoundingBox(0, 0)))
					{
						freeToAdd = false;
						break;
					}
				}
			}
			if (freeToAdd)
			{
				this.addCritter(cloud);
			}
		}
	}

	public void addOwl()
	{
		this.critters.Add(new Owl(new Vector2(Game1.random.Next(64, this.map.Layers[0].LayerWidth * 64 - 64), -128f)));
	}

	public void setFireplace(bool on, int tileLocationX, int tileLocationY, bool playSound = true, int xOffset = 0, int yOffset = 0)
	{
		int fireid = 944468 + tileLocationX * 1000 + tileLocationY;
		string lightSourceId = $"{this.NameOrUniqueName}_Fireplace_{tileLocationX}_{tileLocationY}";
		if (on)
		{
			if (this.getTemporarySpriteByID(fireid) == null)
			{
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(tileLocationX, tileLocationY) * 64f + new Vector2(32f, -32f) + new Vector2(xOffset, yOffset), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = lightSourceId + "_1",
					id = fireid,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(tileLocationX + 1, tileLocationY) * 64f + new Vector2(-16f, -32f) + new Vector2(xOffset, yOffset), flipped: false, 0f, Color.White)
				{
					delayBeforeAnimationStart = 10,
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					lightId = lightSourceId + "_2",
					id = fireid,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = ((float)tileLocationY + 1.1f) * 64f / 10000f
				});
				if (playSound && Game1.gameMode != 6)
				{
					this.localSound("fireball");
				}
				AmbientLocationSounds.addSound(new Vector2(tileLocationX, tileLocationY), 1);
			}
		}
		else
		{
			this.removeTemporarySpritesWithID(fireid);
			Game1.currentLightSources.Remove(lightSourceId + "_1");
			Game1.currentLightSources.Remove(lightSourceId + "_2");
			if (playSound)
			{
				this.localSound("fireball");
			}
			AmbientLocationSounds.removeSound(new Vector2(tileLocationX, tileLocationY));
		}
	}

	public void addWoodpecker(double chance, bool onlyIfOnScreen = false)
	{
		if (Game1.isStartingToGetDarkOut(this) || onlyIfOnScreen || this is Town || this is Desert || !(Game1.random.NextDouble() < chance) || this.terrainFeatures.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			if (Utility.TryGetRandom(this.terrainFeatures, out var tile, out var feature) && feature is Tree tree)
			{
				WildTreeData data = tree.GetData();
				if (data != null && data.AllowWoodpeckers && tree.growthStage.Value >= 5)
				{
					this.critters.Add(new Woodpecker(tree, tile));
					break;
				}
			}
		}
	}

	public void addSquirrels(double chance, bool onlyIfOnScreen = false)
	{
		if (Game1.isStartingToGetDarkOut(this) || onlyIfOnScreen || this is Farm || this is Town || this is Desert || !(Game1.random.NextDouble() < chance) || this.terrainFeatures.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			if (!Utility.TryGetRandom(this.terrainFeatures, out var pos, out var feature) || !(feature is Tree tree) || tree.growthStage.Value < 5 || tree.stump.Value)
			{
				continue;
			}
			int distance = Game1.random.Next(4, 7);
			bool flip = Game1.random.NextBool();
			bool success = true;
			for (int j = 0; j < distance; j++)
			{
				pos.X += (flip ? 1 : (-1));
				if (!this.CanSpawnCharacterHere(pos))
				{
					success = false;
					break;
				}
			}
			if (success)
			{
				this.critters.Add(new Squirrel(pos, flip));
				break;
			}
		}
	}

	public void addBunnies(double chance, bool onlyIfOnScreen = false)
	{
		if (onlyIfOnScreen || this is Farm || this is Desert || !(Game1.random.NextDouble() < chance) || this.largeTerrainFeatures == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			int index = Game1.random.Next(this.largeTerrainFeatures.Count);
			if (this.largeTerrainFeatures.Count <= 0 || !(this.largeTerrainFeatures[index] is Bush))
			{
				continue;
			}
			Vector2 pos = this.largeTerrainFeatures[index].Tile;
			int distance = Game1.random.Next(5, 12);
			bool flip = Game1.random.NextBool();
			bool success = true;
			for (int j = 0; j < distance; j++)
			{
				pos.X += (flip ? 1 : (-1));
				if (!this.largeTerrainFeatures[index].getBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)pos.X * 64, (int)pos.Y * 64, 64, 64)) && !this.CanSpawnCharacterHere(pos))
				{
					success = false;
					break;
				}
			}
			if (success)
			{
				this.critters.Add(new Rabbit(this, pos, flip));
				break;
			}
		}
	}

	public void addOpossums(double chance, bool onlyIfOnScreen = false)
	{
		if (onlyIfOnScreen || this is Farm || this is Desert || !(Game1.random.NextDouble() < chance) || this.largeTerrainFeatures == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			int index = Game1.random.Next(this.largeTerrainFeatures.Count);
			if (this.largeTerrainFeatures.Count <= 0 || !(this.largeTerrainFeatures[index] is Bush))
			{
				continue;
			}
			Vector2 pos = this.largeTerrainFeatures[index].Tile;
			int distance = Game1.random.Next(5, 12);
			bool flip = Game1.player.Position.X > (float)((this is BusStop) ? 704 : 64);
			bool success = true;
			for (int j = 0; j < distance; j++)
			{
				pos.X += (flip ? 1 : (-1));
				if (!this.largeTerrainFeatures[index].getBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)pos.X * 64, (int)pos.Y * 64, 64, 64)) && !this.CanSpawnCharacterHere(pos))
				{
					success = false;
					break;
				}
			}
			if (success)
			{
				if (this is BusStop && Game1.random.NextDouble() < 0.5)
				{
					pos = new Vector2((Game1.player.Tile.X < 26f) ? 36 : 16, 23 + Game1.random.Next(2));
				}
				this.critters.Add(new Opossum(this, pos, flip));
				break;
			}
		}
	}

	public void instantiateCrittersList()
	{
		if (this.critters == null)
		{
			this.critters = new List<Critter>();
		}
	}

	public void addCritter(Critter c)
	{
		this.critters?.Add(c);
	}

	public void addButterflies(double chance, bool onlyIfOnScreen = false)
	{
		Season season = this.GetSeason();
		bool island_location = this.InIslandContext();
		bool firefly = season == Season.Summer && Game1.isDarkOut(this);
		if (Game1.timeOfDay >= 1500 && !firefly && season != Season.Winter)
		{
			return;
		}
		if (season == Season.Spring || season == Season.Summer || (season == Season.Winter && Game1.dayOfMonth % 7 == 0 && Game1.isDarkOut(this)))
		{
			chance = Math.Min(0.8, chance * 1.5);
			while (Game1.random.NextDouble() < chance)
			{
				Vector2 v = this.getRandomTile();
				if (onlyIfOnScreen && Utility.isOnScreen(v * 64f, 64))
				{
					continue;
				}
				if (firefly)
				{
					this.critters.Add(new Firefly(v));
				}
				else
				{
					this.critters.Add(new Butterfly(this, v, island_location));
				}
				while (Game1.random.NextDouble() < 0.4)
				{
					if (firefly)
					{
						this.critters.Add(new Firefly(v + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3))));
					}
					else
					{
						this.critters.Add(new Butterfly(this, v + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3)), island_location));
					}
				}
			}
		}
		if (Game1.timeOfDay < 1700)
		{
			this.tryAddPrismaticButterfly();
		}
	}

	public void tryAddPrismaticButterfly()
	{
		if (!Game1.player.hasBuff("statue_of_blessings_6"))
		{
			return;
		}
		foreach (Critter critter in this.critters)
		{
			if (critter is Butterfly { isPrismatic: not false })
			{
				return;
			}
		}
		Random r = Utility.CreateDaySaveRandom(Game1.player.UniqueMultiplayerID % 10000);
		string[] possibleLocations = new string[7] { "Forest", "Town", "Beach", "Mountain", "Woods", "BusStop", "Backwoods" };
		string locationChoice = possibleLocations[r.Next(possibleLocations.Length)];
		if (locationChoice.Equals("Beach") && this.Name.Equals("BeachNightMarket"))
		{
			locationChoice = "BeachNightMarket";
		}
		if (!this.Name.Equals(locationChoice))
		{
			return;
		}
		Vector2 prism_v = this.getRandomTile(r);
		for (int i = 0; i < 32; i++)
		{
			if (this.isTileLocationOpen(prism_v))
			{
				break;
			}
			prism_v = this.getRandomTile(r);
		}
		this.critters.Add(new Butterfly(this, prism_v, islandButterfly: false, forceSummerButterfly: false, 394, prismatic: true)
		{
			stayInbounds = true
		});
	}

	public void addBirdies(double chance, bool onlyIfOnScreen = false)
	{
		if (Game1.timeOfDay >= 1500 || this is Desert || this is Railroad || this is Farm)
		{
			return;
		}
		Season season = this.GetSeason();
		if (season == Season.Summer)
		{
			return;
		}
		while (Game1.random.NextDouble() < chance)
		{
			int birdiesToAdd = Game1.random.Next(1, 4);
			bool success = false;
			int tries = 0;
			while (!success && tries < 5)
			{
				Vector2 randomTile = this.getRandomTile();
				if (!onlyIfOnScreen || !Utility.isOnScreen(randomTile * 64f, 64))
				{
					Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 2, (int)randomTile.Y - 2, 5, 5);
					if (this.isAreaClear(area))
					{
						List<Critter> crittersToAdd = new List<Critter>();
						int whichBird = ((season == Season.Fall) ? 45 : 25);
						if (Game1.random.NextBool() && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
						{
							whichBird = ((season == Season.Fall) ? 135 : 125);
						}
						if (whichBird == 25 && Game1.random.NextDouble() < 0.05)
						{
							whichBird = 165;
						}
						for (int i = 0; i < birdiesToAdd; i++)
						{
							crittersToAdd.Add(new Birdie(-100, -100, whichBird));
						}
						this.addCrittersStartingAtTile(randomTile, crittersToAdd);
						success = true;
					}
				}
				tries++;
			}
		}
	}

	public void addJumperFrog(Vector2 tileLocation)
	{
		this.critters?.Add(new Frog(tileLocation));
	}

	public void addFrog()
	{
		if (!this.IsRainingHere() || this.IsWinterHere())
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			Vector2 v = this.getRandomTile();
			if (!this.isWaterTile((int)v.X, (int)v.Y) || !this.isWaterTile((int)v.X, (int)v.Y - 1) || this.doesTileHaveProperty((int)v.X, (int)v.Y, "Passable", "Buildings") != null)
			{
				continue;
			}
			int distanceToCheck = 10;
			bool flip = Game1.random.NextBool();
			for (int j = 0; j < distanceToCheck; j++)
			{
				v.X += (flip ? 1 : (-1));
				if (this.isTileOnMap((int)v.X, (int)v.Y) && !this.isWaterTile((int)v.X, (int)v.Y))
				{
					this.critters.Add(new Frog(v, waterLeaper: true, flip));
					return;
				}
			}
		}
	}

	public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
	{
		this.currentEvent?.checkForSpecialCharacterIconAtThisTile(tileLocation);
	}

	private void addCrittersStartingAtTile(Vector2 tile, List<Critter> crittersToAdd)
	{
		if (crittersToAdd == null)
		{
			return;
		}
		int tries = 0;
		HashSet<Vector2> tried_tiles = new HashSet<Vector2>();
		while (crittersToAdd.Count > 0 && tries < 20)
		{
			if (tried_tiles.Contains(tile))
			{
				tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
			}
			else
			{
				if (this.CanItemBePlacedHere(tile))
				{
					Critter critter = crittersToAdd.Last();
					critter.position = tile * 64f;
					critter.startingPosition = tile * 64f;
					this.critters.Add(critter);
					crittersToAdd.RemoveAt(crittersToAdd.Count - 1);
				}
				tile = Utility.getTranslatedVector2(tile, Game1.random.Next(4), 1f);
				tried_tiles.Add(tile);
			}
			tries++;
		}
	}

	public bool isAreaClear(Microsoft.Xna.Framework.Rectangle area)
	{
		foreach (Vector2 tile in area.GetVectors())
		{
			if (!this.CanItemBePlacedHere(tile))
			{
				return false;
			}
		}
		return true;
	}

	public void performGreenRainUpdate()
	{
		if (!this.IsGreenRainingHere() || !this.IsOutdoors || !(this.GetData()?.CanHaveGreenRainSpawns ?? true))
		{
			return;
		}
		Layer pathsLayer = this.map.GetLayer("Paths");
		if (pathsLayer != null)
		{
			for (int x = 0; x < pathsLayer.LayerWidth; x++)
			{
				for (int y = 0; y < pathsLayer.LayerHeight; y++)
				{
					Tile tile = pathsLayer.Tiles[x, y];
					if (tile != null && tile.TileIndexProperties.ContainsKey("GreenRain"))
					{
						Vector2 tilePos = new Vector2(x, y);
						if (!this.IsTileOccupiedBy(tilePos))
						{
							this.terrainFeatures.Add(tilePos, (this is Forest) ? new Tree("12", 5, isGreenRainTemporaryTree: true) : new Tree((10 + (Game1.random.NextBool(0.1) ? 2 : Game1.random.Choose(1, 0))).ToString(), 5, isGreenRainTemporaryTree: true));
						}
					}
				}
			}
		}
		if (this is Town)
		{
			return;
		}
		string[] trees = this.GetMapPropertySplitBySpaces("Trees");
		for (int i = 0; i < trees.Length; i += 3)
		{
			if (!ArgUtility.TryGetVector2(trees, i, out var position, out var error, integerOnly: false, "Vector2 position") || !ArgUtility.TryGetInt(trees, i + 2, out var treeType, out error, "int treeType"))
			{
				this.LogMapPropertyError("Trees", trees, error);
				continue;
			}
			float chance = (this.IsFarm ? 0.5f : 1f);
			if (Game1.random.NextBool(chance) && !this.IsTileOccupiedBy(position))
			{
				this.terrainFeatures.Add(position, new Tree((treeType + 1).ToString(), 5));
			}
		}
		TerrainFeature[] array = this.terrainFeatures.Values.ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] is Tree tree)
			{
				tree.onGreenRainDay();
			}
		}
		int mapArea = this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight;
		this.spawnWeedsAndStones(mapArea / 16, weedsOnly: true, spawnFromOldWeeds: false);
		this.spawnWeedsAndStones(mapArea / 8, weedsOnly: true);
		for (int k = 0; k < mapArea / 4; k++)
		{
			Vector2 v = this.getRandomTile();
			if (this.objects.TryGetValue(v, out var topLeft) && topLeft.IsWeeds() && this.objects.TryGetValue(v + new Vector2(1f, 0f), out var topRight) && topRight.IsWeeds() && this.objects.TryGetValue(v + new Vector2(1f, 1f), out var bottomRight) && bottomRight.IsWeeds() && this.objects.TryGetValue(v + new Vector2(0f, 1f), out var bottomLeft) && bottomLeft.IsWeeds())
			{
				this.objects.Remove(v);
				this.objects.Remove(v + new Vector2(1f, 0f));
				this.objects.Remove(v + new Vector2(1f, 1f));
				this.objects.Remove(v + new Vector2(0f, 1f));
				this.resourceClumps.Add(new ResourceClump(44 + Game1.random.Choose(2, 0), 2, 2, v, 4, "TileSheets\\Objects_2"));
			}
		}
	}

	public void performDayAfterGreenRainUpdate()
	{
		KeyValuePair<Vector2, Object>[] array = this.objects.Pairs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<Vector2, Object> pair = array[i];
			if (pair.Value.Name.Contains("GreenRainWeeds"))
			{
				this.objects.Remove(pair.Key);
			}
		}
		this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.IsGreenRainBush());
		KeyValuePair<Vector2, TerrainFeature>[] array2 = this.terrainFeatures.Pairs.ToArray();
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<Vector2, TerrainFeature> pair2 = array2[i];
			if (!(pair2.Value is Tree tree))
			{
				continue;
			}
			if (this is Town)
			{
				if (tree.isTemporaryGreenRainTree.Value)
				{
					this.terrainFeatures.Remove(pair2.Key);
				}
			}
			else
			{
				tree.onGreenRainDay(undo: true);
			}
		}
	}

	public Vector2 getRandomTile(Random r = null)
	{
		if (r == null)
		{
			r = Game1.random;
		}
		return new Vector2(r.Next(this.Map.Layers[0].LayerWidth), r.Next(this.Map.Layers[0].LayerHeight));
	}

	public void setUpLocationSpecificFlair()
	{
		this.indoorLightingColor = new Color(100, 120, 30);
		this.indoorLightingNightColor = new Color(150, 150, 30);
		if (this.TryGetAmbientLightFromMap(out var c))
		{
			if (c == Color.White)
			{
				c = Color.Black;
			}
			this.indoorLightingColor = c;
			if (this.TryGetAmbientLightFromMap(out var night, "AmbientNightLight"))
			{
				this.indoorLightingNightColor = night;
			}
			else
			{
				this.indoorLightingNightColor = this.indoorLightingColor;
			}
		}
		if (!this.isOutdoors.Value && !(this is FarmHouse) && !(this is IslandFarmHouse))
		{
			Game1.ambientLight = this.indoorLightingColor;
		}
		Game1.screenGlow = false;
		if (!this.IsOutdoors && this.IsGreenRainingHere() && !this.InIslandContext() && this.IsRainingHere())
		{
			this.indoorLightingColor = new Color(123, 0, 96);
			this.indoorLightingNightColor = new Color(185, 40, 119);
			Game1.screenGlowOnce(new Color(0, 255, 50) * 0.5f, hold: true, 1f);
		}
		string value = this.name.Value;
		if (value == null)
		{
			return;
		}
		switch (value.Length)
		{
		case 9:
			switch (value[0])
			{
			case 'J':
				if (value == "JoshHouse" && Game1.isGreenRain)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(386, 334, 36, 28), 40f, 3, 999999, new Vector2(246.5f, 317f) * 4f, flicker: false, flipped: false, 0.136001f, 0f, Color.White, 2f, 0f, 0f, 0f));
				}
				break;
			case 'Q':
				if (value == "QiNutRoom")
				{
					Game1.ambientLight = this.indoorLightingColor;
				}
				break;
			case 'L':
				if (value == "LeahHouse")
				{
					NPC l = Game1.getCharacterFromName("Leah");
					if (this.IsFallHere() || this.IsWinterHere() || this.IsRainingHere())
					{
						this.setFireplace(on: true, 11, 4, playSound: false);
					}
					if (l != null && l.currentLocation == this && !l.isDivorcedFrom(Game1.player))
					{
						string toSay10 = Game1.random.Next(3) switch
						{
							0 => "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting1", 
							1 => "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting2", 
							_ => "Strings\\SpeechBubbles:LeahHouse_Leah_Greeting3", 
						};
						l.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
						l.showTextAboveHead(Game1.content.LoadString(toSay10, Game1.player.Name));
					}
				}
				break;
			}
			break;
		case 6:
			switch (value[1])
			{
			case 'u':
				if (value == "Summit")
				{
					Game1.ambientLight = Color.Black;
				}
				break;
			case 'a':
				if (!(value == "Saloon"))
				{
					break;
				}
				if (Game1.timeOfDay >= 1700 || this.IsGreenRainingHere())
				{
					this.setFireplace(on: true, 22, 17, playSound: false);
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					NPC p8 = Game1.getCharacterFromName("Gus");
					if (p8 != null && p8.TilePoint.Y == 18 && p8.currentLocation == this)
					{
						string toSay12 = Game1.random.Next(5) switch
						{
							0 => "Greeting", 
							1 => this.IsSummerHere() ? "Summer" : "NotSummer", 
							2 => this.IsSnowingHere() ? "Snowing1" : "NotSnowing1", 
							3 => this.IsRainingHere() ? "Raining" : "NotRaining", 
							_ => this.IsSnowingHere() ? "Snowing2" : "NotSnowing2", 
						};
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay12 = "RareGreeting";
						}
						p8.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:Saloon_Gus_" + toSay12));
					}
				}
				if (this.getCharacterFromName("Gus") == null && Game1.IsVisitingIslandToday("Gus"))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(11f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1281f,
						id = 777
					});
				}
				if (Game1.dayOfMonth % 7 == 0 && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom") && Game1.timeOfDay < 1500)
				{
					Texture2D tempTxture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
					this.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
						animationLength = 7,
						sourceRectStartingPos = new Vector2(368f, 336f),
						interval = 5000f,
						totalNumberOfLoops = 99999,
						position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
						scale = 4f,
						layerDepth = 0.0401f,
						id = 2400
					});
				}
				break;
			}
			break;
		case 12:
			switch (value[0])
			{
			case 'L':
				if (value == "LeoTreeHouse")
				{
					this.temporarySprites.Add(new EmilysParrot(new Vector2(88f, 224f))
					{
						layerDepth = 1f,
						id = 5858585
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(304f, 32f), flipped: false, 0f, Color.White)
					{
						layerDepth = 0.001f,
						interval = 700f,
						animationLength = 3,
						totalNumberOfLoops = 999999,
						scale = 4f
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(47, 334, 12, 11), new Vector2(112f, -25.6f), flipped: true, 0f, Color.White)
					{
						layerDepth = 0.001f,
						interval = 300f,
						animationLength = 3,
						totalNumberOfLoops = 999999,
						scale = 4f
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(71, 334, 12, 11), new Vector2(224f, -25.6f), flipped: false, 0f, Color.White)
					{
						layerDepth = 0.001f,
						interval = 800f,
						animationLength = 3,
						totalNumberOfLoops = 999999,
						scale = 4f
					});
				}
				break;
			case 'S':
				if (!(value == "ScienceHouse"))
				{
					break;
				}
				if (Game1.random.NextBool() && Game1.player.currentLocation != null && Game1.player.currentLocation.isOutdoors.Value)
				{
					NPC p = Game1.getCharacterFromName("Robin");
					if (p != null && p.TilePoint.Y == 18)
					{
						string toSay3 = Game1.random.Next(5) switch
						{
							0 => this.IsRainingHere() ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining1" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining1", 
							1 => this.IsSnowingHere() ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Snowing" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotSnowing", 
							2 => (Game1.player.getFriendshipHeartLevelForNPC("Robin") > 4) ? "Strings\\SpeechBubbles:ScienceHouse_Robin_CloseFriends" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotCloseFriends", 
							3 => this.IsRainingHere() ? "Strings\\SpeechBubbles:ScienceHouse_Robin_Raining2" : "Strings\\SpeechBubbles:ScienceHouse_Robin_NotRaining2", 
							_ => "Strings\\SpeechBubbles:ScienceHouse_Robin_Greeting", 
						};
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay3 = "Strings\\SpeechBubbles:ScienceHouse_Robin_RareGreeting";
						}
						p.showTextAboveHead(Game1.content.LoadString(toSay3, Game1.player.Name));
					}
				}
				if (this.getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(7f, 18f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1281f,
						id = 777
					});
				}
				break;
			case 'E':
				if (value == "ElliottHouse")
				{
					NPC e = Game1.getCharacterFromName("Elliott");
					if (e != null && e.currentLocation == this && !e.isDivorcedFrom(Game1.player))
					{
						string toSay2 = Game1.random.Next(3) switch
						{
							0 => "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting1", 
							1 => "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting2", 
							_ => "Strings\\SpeechBubbles:ElliottHouse_Elliott_Greeting3", 
						};
						e.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
						e.showTextAboveHead(Game1.content.LoadString(toSay2, Game1.player.Name));
					}
				}
				break;
			}
			break;
		case 7:
			switch (value[0])
			{
			case 'S':
				if (!(value == "Sunroom"))
				{
					break;
				}
				this.indoorLightingColor = new Color(0, 0, 0);
				AmbientLocationSounds.addSound(new Vector2(3f, 4f), 0);
				if (this.largeTerrainFeatures.Count == 0)
				{
					Bush b = new Bush(new Vector2(6f, 7f), 3, this, -999);
					b.loadSprite();
					b.health = 99f;
					this.largeTerrainFeatures.Add(b);
				}
				if (!this.IsRainingHere())
				{
					this.critters = new List<Critter>();
					this.critters.Add(new Butterfly(this, this.getRandomTile()).setStayInbounds(stayInbounds: true));
					while (Game1.random.NextBool())
					{
						this.critters.Add(new Butterfly(this, this.getRandomTile()).setStayInbounds(stayInbounds: true));
					}
				}
				break;
			case 'B':
				if (!(value == "BugLand"))
				{
					break;
				}
				if (!Game1.player.hasDarkTalisman && this.CanItemBePlacedHere(new Vector2(31f, 5f)))
				{
					this.overlayObjects.Add(new Vector2(31f, 5f), new Chest(new List<Item>
					{
						new SpecialItem(6)
					}, new Vector2(31f, 5f))
					{
						Tint = Color.Gray
					});
				}
				{
					foreach (NPC n in this.characters)
					{
						if (!(n is Grub grub))
						{
							if (n is Fly fly)
							{
								fly.setHard();
							}
						}
						else
						{
							grub.setHard();
						}
					}
					break;
				}
			}
			break;
		case 8:
			switch (value[0])
			{
			case 'W':
				if (value == "WitchHut" && Game1.player.mailReceived.Contains("cursed_doll") && !this.farmers.Any())
				{
					this.characters.Clear();
					uint childrenTurnedToDoves = Game1.stats.Get("childrenTurnedToDoves");
					this.addCharacter(new Bat(new Vector2(7f, 6f) * 64f, -666));
					if (childrenTurnedToDoves > 1)
					{
						this.addCharacter(new Bat(new Vector2(4f, 7f) * 64f, -666));
					}
					if (childrenTurnedToDoves > 2)
					{
						this.addCharacter(new Bat(new Vector2(10f, 7f) * 64f, -666));
					}
					for (int i = 4; i <= childrenTurnedToDoves; i++)
					{
						this.addCharacter(new Bat(Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(1, 4, 13, 4), Game1.random) * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), -666));
					}
				}
				break;
			case 'H':
			{
				if (!(value == "Hospital"))
				{
					break;
				}
				this.indoorLightingColor = new Color(100, 100, 60);
				if (!Game1.random.NextBool())
				{
					break;
				}
				NPC p5 = Game1.getCharacterFromName("Maru");
				if (p5 != null && p5.currentLocation == this && !p5.isDivorcedFrom(Game1.player))
				{
					string toSay8 = Game1.random.Next(5) switch
					{
						0 => "Strings\\SpeechBubbles:Hospital_Maru_Greeting1", 
						1 => "Strings\\SpeechBubbles:Hospital_Maru_Greeting2", 
						2 => "Strings\\SpeechBubbles:Hospital_Maru_Greeting3", 
						3 => "Strings\\SpeechBubbles:Hospital_Maru_Greeting4", 
						_ => "Strings\\SpeechBubbles:Hospital_Maru_Greeting5", 
					};
					if (Game1.player.spouse == "Maru")
					{
						toSay8 = "Strings\\SpeechBubbles:Hospital_Maru_Spouse";
						p5.showTextAboveHead(Game1.content.LoadString(toSay8), SpriteText.color_Red);
					}
					else
					{
						p5.showTextAboveHead(Game1.content.LoadString(toSay8));
					}
				}
				break;
			}
			case 'J':
				if (!(value == "JojaMart"))
				{
					break;
				}
				this.indoorLightingColor = new Color(0, 0, 0);
				if (Game1.random.NextBool())
				{
					NPC p6 = Game1.getCharacterFromName("Morris");
					if (p6 != null && p6.currentLocation == this)
					{
						string toSay9 = "Strings\\SpeechBubbles:JojaMart_Morris_Greeting";
						p6.showTextAboveHead(Game1.content.LoadString(toSay9));
					}
				}
				break;
			case 'S':
				if (!(value == "SeedShop"))
				{
					break;
				}
				this.setFireplace(on: true, 25, 13, playSound: false);
				if (Game1.random.NextBool() && Game1.player.TilePoint.Y > 10)
				{
					NPC p4 = Game1.getCharacterFromName("Pierre");
					if (p4 != null && p4.TilePoint.Y == 17 && p4.currentLocation == this)
					{
						string toSay7 = Game1.random.Next(5) switch
						{
							0 => this.IsWinterHere() ? "Winter" : "NotWinter", 
							1 => this.IsSummerHere() ? "Summer" : "NotSummer", 
							2 => "Greeting1", 
							3 => "Greeting2", 
							_ => this.IsRainingHere() ? "Raining" : "NotRaining", 
						};
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay7 = "RareGreeting";
						}
						string dialogue = Game1.content.LoadString("Strings\\SpeechBubbles:SeedShop_Pierre_" + toSay7);
						p4.showTextAboveHead(string.Format(dialogue, Game1.player.Name));
					}
				}
				if (this.getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(5f, 17f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1217f,
						id = 777
					});
				}
				if (this.getCharacterFromName("Abigail") != null && this.getCharacterFromName("Abigail").TilePoint.Equals(new Point(3, 6)))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 999999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						id = 688
					});
				}
				break;
			}
			break;
		case 10:
			switch (value[0])
			{
			case 'H':
				if (value == "HaleyHouse" && Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse != "Emily")
				{
					this.temporarySprites.Add(new EmilysParrot(new Vector2(912f, 160f)));
				}
				break;
			case 'A':
				if (!(value == "AnimalShop"))
				{
					break;
				}
				this.setFireplace(on: true, 3, 14, playSound: false);
				if (Game1.random.NextBool())
				{
					NPC p3 = Game1.getCharacterFromName("Marnie");
					if (p3 != null && p3.TilePoint.Y == 14)
					{
						string toSay6 = Game1.random.Next(5) switch
						{
							0 => "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting1", 
							1 => "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting2", 
							2 => (Game1.player.getFriendshipHeartLevelForNPC("Marnie") > 4) ? "Strings\\SpeechBubbles:AnimalShop_Marnie_CloseFriends" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotCloseFriends", 
							3 => this.IsRainingHere() ? "Strings\\SpeechBubbles:AnimalShop_Marnie_Raining" : "Strings\\SpeechBubbles:AnimalShop_Marnie_NotRaining", 
							_ => "Strings\\SpeechBubbles:AnimalShop_Marnie_Greeting3", 
						};
						if (Game1.random.NextDouble() < 0.001)
						{
							toSay6 = "Strings\\SpeechBubbles:AnimalShop_Marnie_RareGreeting";
						}
						p3.showTextAboveHead(Game1.content.LoadString(toSay6, Game1.player.Name, Game1.player.farmName));
					}
				}
				if (this.getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors2,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(129, 210, 13, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(129f, 210f),
						interval = 50000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(13f, 14f) * 64f + new Vector2(3f, 0f) * 4f,
						scale = 4f,
						layerDepth = 0.1025f,
						id = 777
					});
				}
				if (Game1.netWorldState.Value.hasWorldStateID("m_painting0"))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(25, 1925, 25, 23),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(25f, 1925f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 777
					});
				}
				else if (Game1.netWorldState.Value.hasWorldStateID("m_painting1"))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1925, 25, 23),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(0f, 1925f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 777
					});
				}
				else if (Game1.netWorldState.Value.hasWorldStateID("m_painting2"))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.mouseCursors,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 1948, 25, 24),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(0f, 1948f),
						interval = 5000f,
						totalNumberOfLoops = 9999,
						position = new Vector2(16f, 1f) * 64f + new Vector2(3f, 1f) * 4f,
						scale = 4f,
						layerDepth = 0.1f,
						id = 777
					});
				}
				break;
			case 'B':
				if (value == "Blacksmith")
				{
					AmbientLocationSounds.addSound(new Vector2(9f, 10f), 2);
					AmbientLocationSounds.changeSpecificVariable("Frequency", 2f, 2);
				}
				break;
			case 'S':
				if (!(value == "SandyHouse"))
				{
					break;
				}
				this.indoorLightingColor = new Color(0, 0, 0);
				if (Game1.random.NextBool())
				{
					NPC p2 = Game1.getCharacterFromName("Sandy");
					if (p2 != null && p2.currentLocation == this)
					{
						string toSay4 = Game1.random.Next(5) switch
						{
							0 => "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting1", 
							1 => "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting2", 
							2 => "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting3", 
							3 => "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting4", 
							_ => "Strings\\SpeechBubbles:SandyHouse_Sandy_Greeting5", 
						};
						p2.showTextAboveHead(Game1.content.LoadString(toSay4));
					}
				}
				break;
			case 'M':
				if (value == "ManorHouse")
				{
					this.indoorLightingColor = new Color(150, 120, 50);
					NPC le = Game1.getCharacterFromName("Lewis");
					if (le != null && le.currentLocation == this)
					{
						string toSay5 = ((Game1.timeOfDay < 1200) ? "Morning" : ((Game1.timeOfDay < 1700) ? "Afternoon" : "Evening"));
						le.faceTowardFarmerForPeriod(3000, 15, faceAway: false, Game1.player);
						le.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ManorHouse_Lewis_" + toSay5));
					}
				}
				break;
			case 'G':
				if (value == "Greenhouse" && Game1.isDarkOut(this))
				{
					Game1.ambientLight = Game1.outdoorLight;
				}
				break;
			}
			break;
		case 13:
			if (value == "LewisBasement")
			{
				if (this.farmers.Count == 0)
				{
					this.characters.Clear();
				}
				Vector2 shortsTile = new Vector2(17f, 15f);
				this.overlayObjects.Remove(shortsTile);
				Object o = ItemRegistry.Create<Object>("(O)789");
				o.questItem.Value = true;
				o.TileLocation = shortsTile;
				o.IsSpawnedObject = true;
				this.overlayObjects.Add(shortsTile, o);
			}
			break;
		case 17:
			if (value == "AbandonedJojaMart" && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
			{
				Point position = new Point(8, 8);
				Game1.currentLightSources.Add(new LightSource("AbandonedJojaMart", 4, new Vector2(position.X * 64, position.Y * 64), 1f, LightSource.LightContext.None, 0L, this.NameOrUniqueName));
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64, position.Y * 64), Color.White)
				{
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f)
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64 - 12, position.Y * 64 - 12), Color.White)
				{
					scale = 0.75f,
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 50
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64 - 12, position.Y * 64 + 12), Color.White)
				{
					layerDepth = 1f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 100
				});
				this.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(position.X * 64, position.Y * 64), Color.White)
				{
					layerDepth = 1f,
					scale = 0.75f,
					interval = 50f,
					motion = new Vector2(1f, 0f),
					acceleration = new Vector2(-0.005f, 0f),
					delayBeforeAnimationStart = 150
				});
				if (this.characters.Count == 0)
				{
					this.characters.Add(new Junimo(new Vector2(8f, 7f) * 64f, 6));
				}
			}
			break;
		case 15:
			if (value == "CommunityCenter" && this is CommunityCenter && (Game1.isLocationAccessible("CommunityCenter") || this.currentEvent?.id == "191393"))
			{
				this.setFireplace(on: true, 31, 8, playSound: false);
				this.setFireplace(on: true, 32, 8, playSound: false);
				this.setFireplace(on: true, 33, 8, playSound: false);
			}
			break;
		case 14:
			if (!(value == "AdventureGuild"))
			{
				break;
			}
			this.setFireplace(on: true, 9, 11, playSound: false);
			if (Game1.random.NextBool())
			{
				NPC p7 = Game1.getCharacterFromName("Marlon");
				if (p7 != null)
				{
					string toSay11 = Game1.random.Next(5) switch
					{
						0 => "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting_" + (Game1.player.IsMale ? "Male" : "Female"), 
						1 => "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting1", 
						2 => "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting2", 
						3 => "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting3", 
						_ => "Strings\\SpeechBubbles:AdventureGuild_Marlon_Greeting4", 
					};
					p7.showTextAboveHead(Game1.content.LoadString(toSay11));
				}
			}
			break;
		case 16:
		{
			if (!(value == "ArchaeologyHouse"))
			{
				break;
			}
			this.setFireplace(on: true, 43, 4, playSound: false);
			if (!Game1.random.NextBool() || !Game1.player.hasOrWillReceiveMail("artifactFound"))
			{
				break;
			}
			NPC g = Game1.getCharacterFromName("Gunther");
			if (g != null && g.currentLocation == this)
			{
				string toSay = Game1.random.Next(5) switch
				{
					0 => "Greeting1", 
					1 => "Greeting2", 
					2 => "Greeting3", 
					3 => "Greeting4", 
					_ => "Greeting5", 
				};
				if (Game1.random.NextDouble() < 0.001)
				{
					toSay = "RareGreeting";
				}
				g.showTextAboveHead(Game1.content.LoadString("Strings\\SpeechBubbles:ArchaeologyHouse_Gunther_" + toSay));
			}
			break;
		}
		case 11:
			break;
		}
	}

	public virtual void hostSetup()
	{
		if (Game1.IsMasterGame && !this.farmers.Any() && !this.HasFarmerWatchingBroadcastEventReturningHere())
		{
			this.interiorDoors.ResetSharedState();
		}
	}

	public virtual void ResetForEvent(Event ev)
	{
		ev.eventPositionTileOffset = Vector2.Zero;
		if (this.IsOutdoors)
		{
			Game1.ambientLight = (this.IsRainingHere() ? new Color(255, 200, 80) : Color.White);
		}
	}

	public virtual bool HasFarmerWatchingBroadcastEventReturningHere()
	{
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer.locationBeforeForcedEvent.Value != null && farmer.locationBeforeForcedEvent.Value == this.NameOrUniqueName)
			{
				return true;
			}
		}
		return false;
	}

	public void resetForPlayerEntry()
	{
		Game1.updateWeatherIcon();
		Game1.hooks.OnGameLocation_ResetForPlayerEntry(this, delegate
		{
			this._madeMapModifications = false;
			if ((!this.farmers.Any() && !this.HasFarmerWatchingBroadcastEventReturningHere()) || Game1.player.sleptInTemporaryBed.Value)
			{
				this.resetSharedState();
			}
			this.resetLocalState();
			if (!this._madeMapModifications)
			{
				this._madeMapModifications = true;
				this.MakeMapModifications();
			}
		});
		Microsoft.Xna.Framework.Rectangle player_bounds = Game1.player.GetBoundingBox();
		foreach (Furniture f in this.furniture)
		{
			Microsoft.Xna.Framework.Rectangle furnitureBounds = f.GetBoundingBox();
			if (furnitureBounds.Intersects(player_bounds) && f.IntersectsForCollision(player_bounds) && !f.isPassable())
			{
				Game1.player.TemporaryPassableTiles.Add(furnitureBounds);
			}
		}
	}

	protected virtual void resetLocalState()
	{
		bool isUpdatingForNewDay = Game1.newDaySync.hasInstance();
		if (this.TryGetMapProperty("ViewportClamp", out var clamp))
		{
			try
			{
				int[] bounds = Utility.parseStringToIntArray(clamp);
				Game1.viewportClampArea = new Microsoft.Xna.Framework.Rectangle(bounds[0] * 64, bounds[1] * 64, bounds[2] * 64, bounds[3] * 64);
			}
			catch (Exception)
			{
				Game1.viewportClampArea = Microsoft.Xna.Framework.Rectangle.Empty;
			}
		}
		else
		{
			Game1.viewportClampArea = Microsoft.Xna.Framework.Rectangle.Empty;
		}
		Game1.elliottPiano = 0;
		Game1.crabPotOverlayTiles.Clear();
		Utility.killAllStaticLoopingSoundCues();
		Game1.player.bridge = null;
		Game1.player.SetOnBridge(val: false);
		if (Game1.CurrentEvent == null && !this.Name.ContainsIgnoreCase("bath"))
		{
			Game1.player.canOnlyWalk = false;
		}
		if (!(this is Farm))
		{
			this.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.clearOnAreaEntry());
		}
		if (Game1.options != null)
		{
			if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton))
			{
				Game1.player.setRunning(!Game1.options.autoRun, force: true);
			}
			else
			{
				Game1.player.setRunning(Game1.options.autoRun, force: true);
			}
		}
		Game1.player.mount?.SyncPositionToRider();
		Game1.UpdateViewPort(overrideFreeze: false, Game1.player.StandingPixel);
		Game1.previousViewportPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
		Game1.PushUIMode();
		foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
		{
			onScreenMenu.gameWindowSizeChanged(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height), new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height));
		}
		Game1.PopUIMode();
		this.ignoreWarps = false;
		if (!isUpdatingForNewDay || Game1.newDaySync.hasFinished())
		{
			if (Game1.player.rightRing.Value != null)
			{
				Game1.player.rightRing.Value.onNewLocation(Game1.player, this);
			}
			if (Game1.player.leftRing.Value != null)
			{
				Game1.player.leftRing.Value.onNewLocation(Game1.player, this);
			}
		}
		this.forceViewportPlayerFollow = this.Map.Properties.ContainsKey("ViewportFollowPlayer");
		this.lastTouchActionLocation = Game1.player.Tile;
		Game1.player.NotifyQuests((Quest quest) => quest.OnWarped(this));
		if (!this.isOutdoors.Value)
		{
			Game1.player.FarmerSprite.currentStep = "thudStep";
		}
		this.setUpLocationSpecificFlair();
		this._updateAmbientLighting();
		if (!this.ignoreLights.Value)
		{
			string lightIdPrefix = this.NameOrUniqueName + "_MapLight_";
			Game1.currentLightSources.RemoveWhere((KeyValuePair<string, LightSource> p) => p.Key.StartsWith(lightIdPrefix));
			string[] lights = this.GetMapPropertySplitBySpaces("Light");
			for (int i = 0; i < lights.Length; i += 3)
			{
				if (!ArgUtility.TryGetPoint(lights, i, out var tile, out var error, "Point tile") || !ArgUtility.TryGetInt(lights, i + 2, out var textureIndex, out error, "int textureIndex"))
				{
					this.LogMapPropertyError("Light", lights, error);
					continue;
				}
				Game1.currentLightSources.Add(new LightSource($"{lightIdPrefix}_{tile.X}_{tile.Y}", textureIndex, new Vector2(tile.X * 64 + 32, tile.Y * 64 + 32), 1f, LightSource.LightContext.MapLight, 0L, this.NameOrUniqueName));
			}
			if (!Game1.isTimeToTurnOffLighting(this) && !Game1.isRaining)
			{
				string[] windowLights = this.GetMapPropertySplitBySpaces("WindowLight");
				for (int i2 = 0; i2 < windowLights.Length; i2 += 3)
				{
					if (!ArgUtility.TryGetPoint(windowLights, i2, out var tile2, out var error2, "Point tile") || !ArgUtility.TryGetInt(windowLights, i2 + 2, out var textureIndex2, out error2, "int textureIndex"))
					{
						this.LogMapPropertyError("WindowLight", windowLights, error2);
						continue;
					}
					Game1.currentLightSources.Add(new LightSource($"{lightIdPrefix}_{tile2.X}_{tile2.Y}_Window", textureIndex2, new Vector2(tile2.X * 64 + 32, tile2.Y * 64 + 32), 1f, LightSource.LightContext.WindowLight, 0L, this.NameOrUniqueName));
				}
				foreach (Vector2 v in this.lightGlows)
				{
					Game1.currentLightSources.Add(new LightSource($"{lightIdPrefix}_{v.X}_{v.Y}_Glow", 6, v, 1f, LightSource.LightContext.WindowLight, 0L, this.NameOrUniqueName));
				}
			}
		}
		if (this.isOutdoors.Value || this.treatAsOutdoors.Value)
		{
			string[] sounds = this.GetMapPropertySplitBySpaces("BrookSounds");
			for (int i3 = 0; i3 < sounds.Length; i3 += 3)
			{
				if (!ArgUtility.TryGetVector2(sounds, i3, out var tile3, out var error3, integerOnly: false, "Vector2 tile") || !ArgUtility.TryGetInt(sounds, i3 + 2, out var soundId, out error3, "int soundId"))
				{
					this.LogMapPropertyError("BrookSounds", sounds, error3);
				}
				else
				{
					AmbientLocationSounds.addSound(tile3, soundId);
				}
			}
			Game1.randomizeRainPositions();
			Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
		}
		foreach (KeyValuePair<Vector2, TerrainFeature> pair2 in this.terrainFeatures.Pairs)
		{
			pair2.Value.performPlayerEntryAction();
		}
		if (this.largeTerrainFeatures != null)
		{
			foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
			{
				largeTerrainFeature.performPlayerEntryAction();
			}
		}
		foreach (KeyValuePair<Vector2, Object> pair3 in this.objects.Pairs)
		{
			pair3.Value.actionOnPlayerEntry();
		}
		if (this.isOutdoors.Value)
		{
			((FarmerSprite)Game1.player.Sprite).currentStep = "sandyStep";
			this.tryToAddCritters();
		}
		this.interiorDoors.ResetLocalState();
		int night_tiles_time = Game1.getTrulyDarkTime(this) - 100;
		if (Game1.timeOfDay < night_tiles_time && (!this.IsRainingHere() || this.name.Equals("SandyHouse")))
		{
			string[] dayTiles = this.GetMapPropertySplitBySpaces("DayTiles");
			for (int i4 = 0; i4 < dayTiles.Length; i4 += 4)
			{
				if (!ArgUtility.TryGet(dayTiles, i4, out var layerId, out var error4, allowBlank: true, "string layerId") || !ArgUtility.TryGetPoint(dayTiles, i4 + 1, out var position, out error4, "Point position") || !ArgUtility.TryGetInt(dayTiles, i4 + 3, out var tileIndex, out error4, "int tileIndex"))
				{
					this.LogMapPropertyError("DayTiles", dayTiles, error4);
				}
				else if (tileIndex != 720 || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Tile tile4 = this.map.RequireLayer(layerId).Tiles[position.X, position.Y];
					if (tile4 != null)
					{
						tile4.TileIndex = tileIndex;
					}
				}
			}
		}
		else if (Game1.timeOfDay >= night_tiles_time || (this.IsRainingHere() && !this.name.Equals("SandyHouse")))
		{
			this.switchOutNightTiles();
		}
		if (Game1.killScreen && Game1.activeClickableMenu != null && !Game1.dialogueUp)
		{
			Game1.activeClickableMenu.emergencyShutDown();
			Game1.exitActiveMenu();
		}
		if (Game1.activeClickableMenu == null && !Game1.warpingForForcedRemoteEvent && !isUpdatingForNewDay)
		{
			this.checkForEvents();
		}
		foreach (KeyValuePair<string, LightSource> pair in this.sharedLights.Pairs)
		{
			Game1.currentLightSources[pair.Key] = pair.Value;
		}
		foreach (NPC character in this.characters)
		{
			character.behaviorOnLocalFarmerLocationEntry(this);
		}
		foreach (Furniture item in this.furniture)
		{
			item.actionOnPlayerEntry();
		}
		this.updateFishSplashAnimation();
		this.updateOrePanAnimation();
		this.showDropboxIndicator = false;
		foreach (SpecialOrder s in Game1.player.team.specialOrders)
		{
			if (s.ShouldDisplayAsComplete())
			{
				continue;
			}
			foreach (OrderObjective objective in s.objectives)
			{
				if (objective is DonateObjective donateObjective && !string.IsNullOrEmpty(donateObjective.dropBoxGameLocation.Value) && donateObjective.GetDropboxLocationName() == this.Name)
				{
					this.showDropboxIndicator = true;
					this.dropBoxIndicatorLocation = donateObjective.dropBoxTileLocation.Value * 64f + new Vector2(7f, 0f) * 4f;
				}
			}
		}
		if (Game1.timeOfDay >= 1830)
		{
			FarmAnimal[] array = this.animals.Values.ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				array[num].warpHome();
			}
		}
		foreach (Building building in this.buildings)
		{
			building.resetLocalState();
		}
		if (this.isThereABuildingUnderConstruction())
		{
			foreach (string builder in Game1.netWorldState.Value.Builders.Keys)
			{
				BuilderData builderData = Game1.netWorldState.Value.Builders[builder];
				if (builderData.buildingLocation.Value == this.NameOrUniqueName && builderData.daysUntilBuilt.Value > 0)
				{
					NPC buildCharacter = Game1.getCharacterFromName(builder);
					if (buildCharacter != null && buildCharacter.currentLocation.Equals(this))
					{
						Building b = this.getBuildingAt(Utility.PointToVector2(builderData.buildingTile.Value));
						if (b != null)
						{
							this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(399, 262, (b.daysOfConstructionLeft.Value == 1) ? 29 : 9, 43), new Vector2(b.tileX.Value + b.tilesWide.Value / 2, b.tileY.Value + b.tilesHigh.Value / 2) * 64f + new Vector2(-16f, -144f), flipped: false, 0f, Color.White)
							{
								id = 16846,
								scale = 4f,
								interval = 999999f,
								animationLength = 1,
								totalNumberOfLoops = 99999,
								layerDepth = (float)((b.tileY.Value + b.tilesHigh.Value / 2) * 64 + 32) / 10000f
							});
						}
					}
				}
			}
			return;
		}
		this.removeTemporarySpritesWithIDLocal(16846);
	}

	protected virtual void _updateAmbientLighting()
	{
		if (Game1.eventUp || (Game1.player.viewingLocation.Value != null && !Game1.player.viewingLocation.Value.Equals(this.Name)))
		{
			return;
		}
		if (!this.isOutdoors.Value || this.ignoreOutdoorLighting.Value)
		{
			if (Game1.isStartingToGetDarkOut(this) || this.lightLevel.Value > 0f)
			{
				int time = Game1.timeOfDay + Game1.gameTimeInterval / (Game1.realMilliSecondsPerGameMinute + this.ExtraMillisecondsPerInGameMinute);
				float lerp = 1f - Utility.Clamp((float)Utility.CalculateMinutesBetweenTimes(time, Game1.getTrulyDarkTime(this)) / 120f, 0f, 1f);
				Game1.ambientLight = new Color((byte)Utility.Lerp((int)this.indoorLightingColor.R, (int)this.indoorLightingNightColor.R, lerp), (byte)Utility.Lerp((int)this.indoorLightingColor.G, (int)this.indoorLightingNightColor.G, lerp), (byte)Utility.Lerp((int)this.indoorLightingColor.B, (int)this.indoorLightingNightColor.B, lerp));
			}
			else
			{
				Game1.ambientLight = this.indoorLightingColor;
			}
		}
		else
		{
			Game1.ambientLight = (this.IsRainingHere() ? new Color(255, 200, 80) : Color.White);
		}
	}

	private bool TryGetAmbientLightFromMap(out Color color, string propertyName = "AmbientLight")
	{
		string[] fields = this.GetMapPropertySplitBySpaces(propertyName);
		if (fields.Length != 0)
		{
			if (ArgUtility.TryGetInt(fields, 0, out var r, out var error, "int r") && ArgUtility.TryGetInt(fields, 1, out var g, out error, "int g") && ArgUtility.TryGetInt(fields, 2, out var b, out error, "int b"))
			{
				color = new Color(r, g, b);
				return true;
			}
			this.LogMapPropertyError(propertyName, fields, error);
		}
		color = Color.White;
		return false;
	}

	public void SelectRandomMiniJukeboxTrack()
	{
		if (!(this.miniJukeboxTrack.Value != "random"))
		{
			Farmer farmer = Game1.player;
			if (this is FarmHouse { HasOwner: not false } farmhouse)
			{
				farmer = farmhouse.owner;
			}
			List<string> song_options = Utility.GetJukeboxTracks(farmer, this);
			string song = Game1.random.ChooseFrom(song_options);
			this.randomMiniJukeboxTrack.Value = song;
		}
	}

	protected virtual void resetSharedState()
	{
		this.SelectRandomMiniJukeboxTrack();
		for (int i = this.characters.Count - 1; i >= 0; i--)
		{
			this.characters[i].behaviorOnFarmerLocationEntry(this, Game1.player);
		}
		if (!(this is MineShaft))
		{
			switch (this.GetSeason())
			{
			case Season.Spring:
				this.waterColor.Value = new Color(120, 200, 255) * 0.5f;
				break;
			case Season.Summer:
				this.waterColor.Value = new Color(60, 240, 255) * 0.5f;
				break;
			case Season.Fall:
				this.waterColor.Value = new Color(255, 130, 200) * 0.5f;
				break;
			case Season.Winter:
				this.waterColor.Value = new Color(130, 80, 255) * 0.5f;
				break;
			}
		}
	}

	public LightSource getLightSource([NotNullWhen(true)] string identifier)
	{
		if (identifier == null || !this.sharedLights.TryGetValue(identifier, out var light))
		{
			return null;
		}
		return light;
	}

	public bool hasLightSource([NotNullWhen(true)] string identifier)
	{
		if (identifier != null)
		{
			return this.sharedLights.ContainsKey(identifier);
		}
		return false;
	}

	public void removeLightSource([NotNullWhen(true)] string identifier)
	{
		if (identifier != null)
		{
			this.sharedLights.Remove(identifier);
		}
	}

	public void repositionLightSource([NotNullWhen(true)] string identifier, Vector2 position)
	{
		if (identifier != null && this.sharedLights.TryGetValue(identifier, out var light))
		{
			light.position.Value = position;
		}
	}

	public virtual bool CanSpawnCharacterHere(Vector2 tileLocation)
	{
		if (this.isTileOnMap(tileLocation) && this.isTilePlaceable(tileLocation))
		{
			return !this.IsTileBlockedBy(tileLocation);
		}
		return false;
	}

	/// <summary>Get whether items in general can be placed on a tile.</summary>
	/// <param name="tile">The tile position within the location.</param>
	/// <param name="itemIsPassable">Whether the item being placed can be walked over by players and characters.</param>
	/// <param name="collisionMask">The collision types to look for. This should usually be kept default.</param>
	/// <param name="ignorePassables">The collision types to ignore when they don't block movement (e.g. tilled dirt).</param>
	/// <param name="useFarmerTile">When checking collisions with farmers, whether to check their tile position instead of their bounding box.</param>
	/// <param name="ignorePassablesExactly">Whether to use the exact <paramref name="ignorePassables" /> value provided, without adjusting it for <paramref name="itemIsPassable" />. This should only be true in specialized cases.</param>
	public virtual bool CanItemBePlacedHere(Vector2 tile, bool itemIsPassable = false, CollisionMask collisionMask = CollisionMask.All, CollisionMask ignorePassables = ~CollisionMask.Objects, bool useFarmerTile = false, bool ignorePassablesExactly = false)
	{
		if (!ignorePassablesExactly)
		{
			ignorePassables &= ~CollisionMask.Objects;
			if (!itemIsPassable)
			{
				ignorePassables &= ~(CollisionMask.Characters | CollisionMask.Farmers);
			}
		}
		if (!this.isTileOnMap(tile))
		{
			return false;
		}
		if (!this.isTilePlaceable(tile, itemIsPassable))
		{
			return false;
		}
		if (this.GetHoeDirtAtTile(tile)?.crop != null)
		{
			return false;
		}
		if (this.IsTileBlockedBy(tile, collisionMask, ignorePassables, useFarmerTile))
		{
			return false;
		}
		if (itemIsPassable && this.getBuildingAt(tile) != null && this.getBuildingAt(tile).GetData() != null && !this.getBuildingAt(tile).GetData().AllowsFlooringUnderneath)
		{
			return false;
		}
		return true;
	}

	/// <summary>Get whether a tile is either occupied by an object or is a non-passable tile.</summary>
	/// <param name="tile">The tile position within the location.</param>
	/// <param name="collisionMask">The collision types to look for. This should usually be kept default.</param>
	/// <param name="ignorePassables">The collision types to ignore when they don't block movement (e.g. tilled dirt).</param>
	/// <param name="useFarmerTile">When checking collisions with farmers, whether to check their tile position instead of their bounding box.</param>
	public virtual bool IsTileBlockedBy(Vector2 tile, CollisionMask collisionMask = CollisionMask.All, CollisionMask ignorePassables = CollisionMask.None, bool useFarmerTile = false)
	{
		if (!this.IsTileOccupiedBy(tile, collisionMask, ignorePassables, useFarmerTile))
		{
			return !this.isTilePassable(tile);
		}
		return true;
	}

	/// <summary>Get whether a tile is occupied.</summary>
	/// <param name="tile">The tile position within the location.</param>
	/// <param name="collisionMask">The collision types to look for. This should usually be kept default.</param>
	/// <param name="ignorePassables">The collision types to ignore when they don't block movement (e.g. tilled dirt).</param>
	/// <param name="useFarmerTile">When checking collisions with farmers, whether to check their tile position instead of their bounding box.</param>
	public virtual bool IsTileOccupiedBy(Vector2 tile, CollisionMask collisionMask = CollisionMask.All, CollisionMask ignorePassables = CollisionMask.None, bool useFarmerTile = false)
	{
		Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
		if (collisionMask.HasFlag(CollisionMask.Farmers) && !ignorePassables.HasFlag(CollisionMask.Farmers))
		{
			foreach (Farmer f in this.farmers)
			{
				if (useFarmerTile ? (f.Tile == tile) : f.GetBoundingBox().Intersects(tileRect))
				{
					return true;
				}
			}
		}
		if (collisionMask.HasFlag(CollisionMask.Objects) && this.objects.TryGetValue(tile, out var o) && (!ignorePassables.HasFlag(CollisionMask.Objects) || !o.isPassable()))
		{
			return true;
		}
		if (collisionMask.HasFlag(CollisionMask.Furniture))
		{
			Furniture f2 = this.GetFurnitureAt(tile);
			if (f2 != null && (!ignorePassables.HasFlag(CollisionMask.Furniture) || !f2.isPassable()))
			{
				return true;
			}
		}
		if (collisionMask.HasFlag(CollisionMask.Characters))
		{
			foreach (NPC character in this.characters)
			{
				if (character != null && character.GetBoundingBox().Intersects(tileRect) && !character.IsInvisible && (!ignorePassables.HasFlag(CollisionMask.Characters) || !character.farmerPassesThrough))
				{
					return true;
				}
			}
			if (this.animals.Length > 0)
			{
				foreach (FarmAnimal animal in this.animals.Values)
				{
					if (animal.Tile == tile && (!ignorePassables.HasFlag(CollisionMask.Characters) || !animal.farmerPassesThrough))
					{
						return true;
					}
				}
			}
		}
		if (collisionMask.HasFlag(CollisionMask.TerrainFeatures))
		{
			foreach (ResourceClump resourceClump in this.resourceClumps)
			{
				if (resourceClump.occupiesTile((int)tile.X, (int)tile.Y) && (!ignorePassables.HasFlag(CollisionMask.TerrainFeatures) || !resourceClump.isPassable()))
				{
					return true;
				}
			}
			if (this.largeTerrainFeatures != null)
			{
				foreach (LargeTerrainFeature t in this.largeTerrainFeatures)
				{
					if (t.getBoundingBox().Intersects(tileRect) && (!ignorePassables.HasFlag(CollisionMask.TerrainFeatures) || !t.isPassable()))
					{
						return true;
					}
				}
			}
		}
		if ((collisionMask.HasFlag(CollisionMask.TerrainFeatures) || collisionMask.HasFlag(CollisionMask.Flooring)) && this.terrainFeatures.TryGetValue(tile, out var feature) && feature.getBoundingBox().Intersects(tileRect))
		{
			CollisionMask relevantMask = ((feature is Flooring) ? CollisionMask.Flooring : CollisionMask.TerrainFeatures);
			if (collisionMask.HasFlag(relevantMask) && (!ignorePassables.HasFlag(relevantMask) || !feature.isPassable()))
			{
				return true;
			}
		}
		if (collisionMask.HasFlag(CollisionMask.LocationSpecific) && this.IsLocationSpecificOccupantOnTile(tile))
		{
			return true;
		}
		if (collisionMask.HasFlag(CollisionMask.Buildings))
		{
			foreach (Building b in this.buildings)
			{
				if (!b.isMoving && (ignorePassables.HasFlag(CollisionMask.Buildings) ? (!b.isTilePassable(tile)) : b.occupiesTile(tile)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool IsLocationSpecificOccupantOnTile(Vector2 tileLocation)
	{
		return false;
	}

	public virtual bool IsLocationSpecificPlacementRestriction(Vector2 tileLocation)
	{
		return false;
	}

	public Farmer isTileOccupiedByFarmer(Vector2 tileLocation)
	{
		foreach (Farmer f in this.farmers)
		{
			if (f.Tile == tileLocation)
			{
				return f;
			}
		}
		return null;
	}

	/// <summary>Get any tilled dirt at a tile position, whether it's on the ground or in a garden pot.</summary>
	/// <param name="tile">The tile position to check.</param>
	/// <returns>Returns the tilled dirt found, else <c>null</c>.</returns>
	public HoeDirt GetHoeDirtAtTile(Vector2 tile)
	{
		if (this.objects.TryGetValue(tile, out var obj) && obj is IndoorPot pot)
		{
			return pot.hoeDirt.Value;
		}
		if (this.terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt dirt)
		{
			return dirt;
		}
		return null;
	}

	/// <summary>Get whether a tile contains a hoe dirt, or an object that should behave like a hoe dirt, such as a Garden Pot.</summary>
	public bool isTileHoeDirt(Vector2 tile)
	{
		return this.GetHoeDirtAtTile(tile) != null;
	}

	/// <summary>Get whether a tile is not on the water, and is unobstructed by a tile on the Buildings layer or higher. This can be used to ensure items don't spawn behind high walls, etc.</summary>
	public bool isTileLocationOpen(Location location)
	{
		return this.isTileLocationOpen(new Vector2(location.X, location.Y));
	}

	/// <summary>Get whether a tile is not on the water, and is unobstructed by a tile on the Buildings layer or higher. This can be used to ensure items don't spawn behind high walls, etc.</summary>
	public bool isTileLocationOpen(Vector2 location)
	{
		if (this.map.RequireLayer("Buildings").Tiles[(int)location.X, (int)location.Y] == null && !this.isWaterTile((int)location.X, (int)location.Y) && this.map.RequireLayer("Front").Tiles[(int)location.X, (int)location.Y] == null)
		{
			return this.map.GetLayer("AlwaysFront")?.Tiles[(int)location.X, (int)location.Y] == null;
		}
		return false;
	}

	public virtual bool CanPlaceThisFurnitureHere(Furniture furniture)
	{
		if (furniture == null)
		{
			return false;
		}
		bool isIndoor = this is DecoratableLocation || !this.IsOutdoors;
		if (furniture.furniture_type.Value == 15)
		{
			if (!this.TryGetMapPropertyAs("AllowBeds", out bool allowBedsHere, required: false))
			{
				allowBedsHere = this is FarmHouse || this is IslandFarmHouse || (isIndoor && this.ParentBuilding != null);
			}
			if (!allowBedsHere)
			{
				return false;
			}
		}
		switch (furniture.placementRestriction)
		{
		case 0:
			return isIndoor;
		case 1:
			return !isIndoor;
		case 2:
			if (!isIndoor)
			{
				return !isIndoor;
			}
			return true;
		default:
			return false;
		}
	}

	/// <summary>Get whether a tile is allowed to have an object placed on it. Note that this function does not factor in the tile's current occupancy.</summary>
	public virtual bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
	{
		if (this.IsLocationSpecificPlacementRestriction(v))
		{
			return false;
		}
		if (!this.hasTileAt((int)v.X, (int)v.Y, "Back"))
		{
			return false;
		}
		if (this.isWaterTile((int)v.X, (int)v.Y))
		{
			return false;
		}
		string noFurniture = this.doesTileHaveProperty((int)v.X, (int)v.Y, "NoFurniture", "Back");
		if (noFurniture != null)
		{
			if (noFurniture == "total")
			{
				return false;
			}
			if (!itemIsPassable || !Game1.currentLocation.IsOutdoors)
			{
				return false;
			}
		}
		return true;
	}

	public void playTerrainSound(Vector2 tileLocation, Character who = null, bool showTerrainDisturbAnimation = true)
	{
		string currentStep = "thudStep";
		if (this.IsOutdoors || this.treatAsOutdoors.Value || this.Name.ContainsIgnoreCase("mine"))
		{
			switch (this.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back"))
			{
			case "Dirt":
				currentStep = "sandyStep";
				break;
			case "Stone":
				currentStep = "stoneStep";
				break;
			case "Grass":
				currentStep = ((this.GetSeason() == Season.Winter) ? "snowyStep" : "grassyStep");
				break;
			case "Wood":
				currentStep = "woodyStep";
				break;
			case null:
				if (this.isWaterTile((int)tileLocation.X, (int)tileLocation.Y))
				{
					currentStep = "waterSlosh";
				}
				break;
			}
		}
		if (this.terrainFeatures.TryGetValue(tileLocation, out var terrainFeature) && terrainFeature is Flooring)
		{
			currentStep = ((Flooring)this.terrainFeatures[tileLocation]).getFootstepSound();
		}
		if (who != null && showTerrainDisturbAnimation && currentStep == "sandyStep")
		{
			Vector2 offset = Vector2.Zero;
			if (who.shouldShadowBeOffset)
			{
				offset = who.drawOffset;
			}
			this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64), 50f, 4, 1, new Vector2(who.Position.X + (float)Game1.random.Next(-8, 8), who.Position.Y + (float)Game1.random.Next(-16, 0)) + offset, flicker: false, Game1.random.NextBool(), 0.0001f, 0f, Color.White, 1f, 0.01f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 128f));
		}
		else if (who != null && showTerrainDisturbAnimation && this.GetSeason() == Season.Winter && currentStep == "grassyStep")
		{
			Vector2 offset2 = Vector2.Zero;
			if (who.shouldShadowBeOffset)
			{
				offset2 = who.drawOffset;
			}
			this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(who.Position.X, who.Position.Y) + offset2, flicker: false, flipped: false, 0.0001f, 0.001f, Color.White, 1f, 0.01f, 0f, 0f));
		}
		if ((who as Farmer)?.boots.Value?.ItemId == "853")
		{
			this.localSound("jingleBell");
		}
		if (currentStep.Length > 0)
		{
			this.localSound(currentStep);
		}
	}

	public bool checkTileIndexAction(int tileIndex)
	{
		if ((tileIndex == 1799 || (uint)(tileIndex - 1824) <= 9u) && this.Name.Equals("AbandonedJojaMart"))
		{
			Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart").checkBundle();
			return true;
		}
		return false;
	}

	public bool checkForTerrainFeaturesAndObjectsButDestroyNonPlayerItems(int x, int y)
	{
		Vector2 v = new Vector2(x, y);
		if (this.objects.TryGetValue(v, out var tileObj))
		{
			if (!tileObj.IsSpawnedObject || tileObj is Chest || tileObj.Type == "Crafting")
			{
				return false;
			}
			this.objects.Remove(v);
		}
		this.terrainFeatures.Remove(v);
		return true;
	}

	public virtual bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		who.ignoreItemConsumptionThisFrame = false;
		Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
		if (!this.objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)) && this.CheckPetAnimal(tileRect, who))
		{
			return true;
		}
		foreach (Building building in this.buildings)
		{
			if (building.doAction(new Vector2(tileLocation.X, tileLocation.Y), who))
			{
				return true;
			}
		}
		if (who.IsSitting())
		{
			who.StopSitting();
			return true;
		}
		foreach (Farmer farmer in this.farmers)
		{
			if (farmer != Game1.player && farmer.GetBoundingBox().Intersects(tileRect) && farmer.checkAction(who, this))
			{
				return true;
			}
		}
		if (this.currentEvent != null && this.currentEvent.isFestival)
		{
			return this.currentEvent.checkAction(tileLocation, viewport, who);
		}
		foreach (NPC n in this.characters)
		{
			if (n != null && !n.IsMonster && (!who.isRidingHorse() || !(n is Horse)) && n.GetBoundingBox().Intersects(tileRect) && n.checkAction(who, this))
			{
				if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, carrying: false) || who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, carrying: true))
				{
					who.faceGeneralDirection(n.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				}
				return true;
			}
		}
		int tileIndex = this.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet");
		if (this.NameOrUniqueName == "SkullCave" && (tileIndex == 344 || tileIndex == 349))
		{
			if (Game1.player.team.SpecialOrderActive("QiChallenge10"))
			{
				who.doEmote(40);
				return false;
			}
			if (!Game1.player.team.completedSpecialOrders.Contains("QiChallenge10"))
			{
				who.doEmote(8);
				return false;
			}
			if (!Game1.player.team.toggleSkullShrineOvernight.Value)
			{
				if (!Game1.player.team.skullShrineActivated.Value)
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_NotYetHard"), this.createYesNoResponses(), "ShrineOfSkullChallenge");
				}
				else
				{
					Game1.player.team.toggleSkullShrineOvernight.Value = true;
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
					Game1.multiplayer.globalChatInfoMessage(Game1.player.team.skullShrineActivated.Value ? "HardModeSkullCaveDeactivated" : "HardModeSkullCaveActivated", who.name.Value);
					this.playSound(Game1.player.team.skullShrineActivated.Value ? "skeletonStep" : "serpentDie");
				}
			}
			else if (Game1.player.team.toggleSkullShrineOvernight.Value && Game1.player.team.skullShrineActivated.Value)
			{
				Game1.player.team.toggleSkullShrineOvernight.Value = false;
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:PendingProposal_Canceling"));
				this.playSound("skeletonStep");
			}
			return true;
		}
		foreach (ResourceClump stump in this.resourceClumps)
		{
			if (stump.getBoundingBox().Intersects(tileRect) && stump.performUseAction(new Vector2(tileLocation.X, tileLocation.Y)))
			{
				return true;
			}
		}
		Vector2 tilePos = new Vector2(tileLocation.X, tileLocation.Y);
		if (this.objects.TryGetValue(tilePos, out var obj))
		{
			bool isErrorItem = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId).IsErrorItem;
			if (obj.Type != null || isErrorItem)
			{
				if (who.isRidingHorse() && !(obj is Fence))
				{
					return false;
				}
				if (tilePos == who.Tile && !obj.isPassable() && (!(obj is Fence fence) || !fence.isGate.Value))
				{
					Tool t = ItemRegistry.Create<Tool>("(T)Pickaxe");
					t.DoFunction(Game1.currentLocation, -1, -1, 0, who);
					if (obj.performToolAction(t))
					{
						obj.performRemoveAction();
						obj.dropItem(this, who.GetToolLocation(), Utility.PointToVector2(who.StandingPixel));
						Game1.currentLocation.Objects.Remove(tilePos);
						return true;
					}
					t = ItemRegistry.Create<Tool>("(T)Axe");
					t.DoFunction(Game1.currentLocation, -1, -1, 0, who);
					if (this.objects.TryGetValue(tilePos, out obj) && obj.performToolAction(t))
					{
						obj.performRemoveAction();
						obj.dropItem(this, who.GetToolLocation(), Utility.PointToVector2(who.StandingPixel));
						Game1.currentLocation.Objects.Remove(tilePos);
						return true;
					}
					if (!this.objects.TryGetValue(tilePos, out obj))
					{
						return true;
					}
				}
				if (this.objects.TryGetValue(tilePos, out obj) && (obj.Type == "Crafting" || obj.Type == "interactive"))
				{
					if (who.ActiveObject == null && obj.checkForAction(who))
					{
						return true;
					}
					if (this.objects.TryGetValue(tilePos, out obj))
					{
						if (who.CurrentItem != null)
						{
							Object old_held_object = obj.heldObject.Value;
							obj.heldObject.Value = null;
							bool probe_returned_true = obj.performObjectDropInAction(who.CurrentItem, probe: true, who);
							obj.heldObject.Value = old_held_object;
							bool perform_returned_true = obj.performObjectDropInAction(who.CurrentItem, probe: false, who, returnFalseIfItemConsumed: true);
							if ((probe_returned_true || perform_returned_true) && who.isMoving())
							{
								Game1.haltAfterCheck = false;
							}
							if (who.ignoreItemConsumptionThisFrame)
							{
								return true;
							}
							if (perform_returned_true)
							{
								who.reduceActiveItemByOne();
								return true;
							}
							return obj.checkForAction(who) || probe_returned_true;
						}
						return obj.checkForAction(who);
					}
				}
				else if (this.objects.TryGetValue(tilePos, out obj) && (obj.isSpawnedObject.Value || isErrorItem))
				{
					int oldQuality = obj.quality.Value;
					Random r = Utility.CreateDaySaveRandom(tilePos.X, tilePos.Y * 777f);
					if (obj.isForage())
					{
						obj.Quality = this.GetHarvestSpawnedObjectQuality(who, obj.isForage(), obj.TileLocation, r);
					}
					if (obj.questItem.Value && obj.questId.Value != null && obj.questId.Value != "0" && !who.hasQuest(obj.questId.Value))
					{
						return false;
					}
					if (who.couldInventoryAcceptThisItem(obj))
					{
						if (who.IsLocalPlayer)
						{
							this.localSound("pickUpItem");
							DelayedAction.playSoundAfterDelay("coin", 300);
						}
						who.animateOnce(279 + who.FacingDirection);
						if (!this.isFarmBuildingInterior())
						{
							if (obj.isForage())
							{
								this.OnHarvestedForage(who, obj);
							}
							if (obj.ItemId.Equals("789") && this.Name.Equals("LewisBasement"))
							{
								Bat b = new Bat(Vector2.Zero, -789);
								b.focusedOnFarmers = true;
								Game1.changeMusicTrack("none");
								this.playSound("cursed_mannequin");
								this.characters.Add(b);
							}
						}
						else
						{
							who.gainExperience(0, 5);
						}
						who.addItemToInventoryBool(obj.getOne());
						Game1.stats.ItemsForaged++;
						if (who.professions.Contains(13) && r.NextDouble() < 0.2 && !obj.questItem.Value && who.couldInventoryAcceptThisItem(obj) && !this.isFarmBuildingInterior())
						{
							who.addItemToInventoryBool(obj.getOne());
							who.gainExperience(2, 7);
						}
						this.objects.Remove(tilePos);
						return true;
					}
					obj.Quality = oldQuality;
				}
			}
		}
		if (who.isRidingHorse())
		{
			who.mount.checkAction(who, this);
			return true;
		}
		foreach (KeyValuePair<Vector2, TerrainFeature> v in this.terrainFeatures.Pairs)
		{
			if (v.Value.getBoundingBox().Intersects(tileRect) && v.Value.performUseAction(v.Key))
			{
				Game1.haltAfterCheck = false;
				return true;
			}
		}
		if (this.largeTerrainFeatures != null)
		{
			foreach (LargeTerrainFeature f in this.largeTerrainFeatures)
			{
				if (f.getBoundingBox().Intersects(tileRect) && f.performUseAction(f.Tile))
				{
					Game1.haltAfterCheck = false;
					return true;
				}
			}
		}
		Tile tile = this.map.RequireLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
		if (tile == null || !tile.Properties.TryGetValue("Action", out var action))
		{
			action = this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
		}
		if (action != null)
		{
			NPC characterAtTile = this.isCharacterAtTile(tilePos + new Vector2(0f, 1f));
			if (this.currentEvent == null && characterAtTile != null && !characterAtTile.IsInvisible && !characterAtTile.IsMonster && (!who.isRidingHorse() || !(characterAtTile is Horse)))
			{
				Point characterPixel = characterAtTile.StandingPixel;
				if (Utility.withinRadiusOfPlayer(characterPixel.X, characterPixel.Y, 1, who) && characterAtTile.checkAction(who, this))
				{
					if (who.FarmerSprite.IsPlayingBasicAnimation(who.FacingDirection, who.IsCarrying()))
					{
						who.faceGeneralDirection(Utility.PointToVector2(characterPixel), 0, opposite: false, useTileCalculations: false);
					}
					return true;
				}
			}
			return this.performAction(action, who, tileLocation);
		}
		if (tile != null && this.checkTileIndexAction(tile.TileIndex))
		{
			return true;
		}
		foreach (MapSeat seat in this.mapSeats)
		{
			if (seat.OccupiesTile(tileLocation.X, tileLocation.Y) && !seat.IsBlocked(this))
			{
				who.BeginSitting(seat);
				return true;
			}
		}
		Point vectOnWall = new Point(tileLocation.X * 64, (tileLocation.Y - 1) * 64);
		bool didRightClick = Game1.didPlayerJustRightClick();
		Furniture paintingFound = null;
		foreach (Furniture f2 in this.furniture)
		{
			if (f2.boundingBox.Value.Contains((int)(tilePos.X * 64f), (int)(tilePos.Y * 64f)) && f2.furniture_type.Value != 12)
			{
				if (didRightClick)
				{
					if (who.ActiveObject != null && f2.performObjectDropInAction(who.ActiveObject, probe: false, who))
					{
						return true;
					}
					return f2.checkForAction(who);
				}
				return f2.clicked(who);
			}
			if (f2.furniture_type.Value == 6 && f2.boundingBox.Value.Contains(vectOnWall))
			{
				paintingFound = f2;
			}
		}
		if (paintingFound != null)
		{
			if (didRightClick)
			{
				if (who.ActiveObject != null && paintingFound.performObjectDropInAction(who.ActiveObject, probe: false, who))
				{
					return true;
				}
				return paintingFound.checkForAction(who);
			}
			return paintingFound.clicked(who);
		}
		if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && this.animals.Length > 0 && this.CheckInspectAnimal(tileRect, who))
		{
			return true;
		}
		return false;
	}

	/// <summary>Get the quality for a newly harvested spawned object.</summary>
	/// <param name="who">The player harvesting the object.</param>
	/// <param name="isForage">Whether the object is a forage item.</param>
	/// <param name="tile">The tile position.</param>
	/// <param name="random">The RNG to use if needed, or <c>null</c> to create a new one.</param>
	public int GetHarvestSpawnedObjectQuality(Farmer who, bool isForage, Vector2 tile, Random random = null)
	{
		if (who.professions.Contains(16) && isForage)
		{
			return 4;
		}
		if (isForage)
		{
			if (random == null)
			{
				random = Utility.CreateDaySaveRandom(tile.X, tile.Y * 777f);
			}
			if (random.NextBool((float)who.ForagingLevel / 30f))
			{
				return 2;
			}
			if (random.NextBool((float)who.ForagingLevel / 15f))
			{
				return 1;
			}
		}
		return 0;
	}

	/// <summary>Handle a player harvesting a spawned forage object.</summary>
	/// <param name="who">The player who harvested the forage.</param>
	/// <param name="forage">The forage object that was harvested.</param>
	public void OnHarvestedForage(Farmer who, Object forage)
	{
		if (forage.SpecialVariable == 724519)
		{
			who.gainExperience(2, 2);
			who.gainExperience(0, 3);
		}
		else
		{
			who.gainExperience(2, 7);
		}
	}

	public virtual bool CanFreePlaceFurniture()
	{
		return false;
	}

	public virtual bool LowPriorityLeftClick(int x, int y, Farmer who)
	{
		if (Game1.activeClickableMenu != null)
		{
			return false;
		}
		for (int i = this.furniture.Count - 1; i >= 0; i--)
		{
			Furniture furnitureItem = this.furniture[i];
			if (this.CanFreePlaceFurniture() || furnitureItem.IsCloseEnoughToFarmer(who))
			{
				if (!furnitureItem.isPassable() && furnitureItem.boundingBox.Value.Contains(x, y) && furnitureItem.canBeRemoved(who))
				{
					furnitureItem.AttemptRemoval(delegate(Furniture f)
					{
						Guid job = this.furniture.GuidOf(f);
						if (!this.furnitureToRemove.Contains(job))
						{
							this.furnitureToRemove.Add(job);
						}
					});
					return true;
				}
				if (furnitureItem.boundingBox.Value.Contains(x, y) && furnitureItem.heldObject.Value != null)
				{
					furnitureItem.clicked(who);
					return true;
				}
				if (!furnitureItem.isGroundFurniture() && furnitureItem.canBeRemoved(who))
				{
					int wall_y = y;
					if (this is DecoratableLocation decoratableLocation)
					{
						wall_y = decoratableLocation.GetWallTopY(x / 64, y / 64);
						wall_y = ((wall_y != -1) ? (wall_y * 64) : (y * 64));
					}
					if (furnitureItem.boundingBox.Value.Contains(x, wall_y))
					{
						furnitureItem.AttemptRemoval(delegate(Furniture f)
						{
							Guid job = this.furniture.GuidOf(f);
							if (!this.furnitureToRemove.Contains(job))
							{
								this.furnitureToRemove.Add(job);
							}
						});
						return true;
					}
				}
			}
		}
		for (int i2 = this.furniture.Count - 1; i2 >= 0; i2--)
		{
			Furniture furnitureItem2 = this.furniture[i2];
			if ((this.CanFreePlaceFurniture() || furnitureItem2.IsCloseEnoughToFarmer(who)) && furnitureItem2.isPassable() && furnitureItem2.boundingBox.Value.Contains(x, y) && furnitureItem2.canBeRemoved(who))
			{
				furnitureItem2.AttemptRemoval(delegate(Furniture f)
				{
					Guid job = this.furniture.GuidOf(f);
					if (!this.furnitureToRemove.Contains(job))
					{
						this.furnitureToRemove.Add(job);
					}
				});
				return true;
			}
		}
		Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
		if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && this.CheckInspectAnimal(tileRect, who))
		{
			return true;
		}
		return false;
	}

	[Obsolete("These values returned by this function are no longer used by the game (except for rare, backwards compatibility related cases.) Check DecoratableLocation's wallpaper/flooring related functionality instead.")]
	public virtual List<Microsoft.Xna.Framework.Rectangle> getWalls()
	{
		return new List<Microsoft.Xna.Framework.Rectangle>();
	}

	protected virtual void removeQueuedFurniture(Guid guid)
	{
		Farmer who = Game1.player;
		if (!this.furniture.TryGetValue(guid, out var furnitureItem) || !who.couldInventoryAcceptThisItem(furnitureItem))
		{
			return;
		}
		furnitureItem.performRemoveAction();
		this.furniture.Remove(guid);
		bool foundInToolbar = false;
		for (int j = 0; j < 12; j++)
		{
			if (who.Items[j] == null)
			{
				who.Items[j] = furnitureItem;
				who.CurrentToolIndex = j;
				foundInToolbar = true;
				break;
			}
		}
		if (!foundInToolbar)
		{
			Item item = who.addItemToInventory(furnitureItem, 11);
			who.addItemToInventory(item);
			who.CurrentToolIndex = 11;
		}
		this.localSound("coin");
	}

	public virtual bool leftClick(int x, int y, Farmer who)
	{
		Vector2 clickTile = new Vector2(x / 64, y / 64);
		foreach (Building building in this.buildings)
		{
			if (building.CanLeftClick(x, y) && building.leftClicked())
			{
				return true;
			}
		}
		if (this.objects.TryGetValue(clickTile, out var clickedObj) && clickedObj.clicked(who))
		{
			this.objects.Remove(clickTile);
			return true;
		}
		return false;
	}

	public virtual bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
	{
		if (this.doesTileHaveProperty((int)p.X, (int)p.Y, "Passable", "Buildings") != null)
		{
			return true;
		}
		if (this.terrainFeatures.TryGetValue(p, out var feature) && feature is HoeDirt)
		{
			return true;
		}
		if (this.isWaterTile((int)p.X, (int)p.Y))
		{
			int tileIndex = this.getTileIndexAt((int)p.X, (int)p.Y, "Buildings", "Town");
			if (tileIndex < 1004 || tileIndex > 1013)
			{
				return true;
			}
		}
		foreach (Building building in this.buildings)
		{
			if (building.occupiesTile(p) && building.isTilePassable(p))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Get the fridge that's part of this map, if it has one.</summary>
	/// <param name="onlyUnlocked">Whether to only return the fridge if it's available to the player (e.g. they've unlocked the required house upgrade).</param>
	public virtual Chest GetFridge(bool onlyUnlocked = true)
	{
		if (!(this is FarmHouse home))
		{
			if (this is IslandFarmHouse islandHome && (!onlyUnlocked || islandHome.fridgePosition != Point.Zero))
			{
				return islandHome.fridge.Value;
			}
		}
		else if (!onlyUnlocked || home.fridgePosition != Point.Zero)
		{
			return home.fridge.Value;
		}
		return null;
	}

	/// <summary>Get the tile position of the fridge that's part of this map, if it has one and it's available to the player (e.g. they've unlocked the required house upgrade).</summary>
	public virtual Point? GetFridgePosition()
	{
		if (!(this is FarmHouse home))
		{
			if (this is IslandFarmHouse islandHome && islandHome.fridgePosition != Point.Zero)
			{
				return islandHome.fridgePosition;
			}
		}
		else if (home.fridgePosition != Point.Zero)
		{
			return home.fridgePosition;
		}
		return null;
	}

	/// <summary>Open the cooking menu, with ingredients available from any <see cref="M:StardewValley.GameLocation.GetFridge(System.Boolean)" /> or mini-fridges in the location.</summary>
	public void ActivateKitchen()
	{
		List<NetMutex> muticies = new List<NetMutex>();
		List<Chest> mini_fridges = new List<Chest>();
		foreach (Object item in this.objects.Values)
		{
			if (item != null && item.bigCraftable.Value && item is Chest chest && chest.fridge.Value)
			{
				mini_fridges.Add(chest);
				muticies.Add(chest.mutex);
			}
		}
		Chest fridge = this.GetFridge();
		if (fridge != null)
		{
			muticies.Add(fridge.mutex);
		}
		new MultipleMutexRequest(muticies, delegate(MultipleMutexRequest request)
		{
			List<IInventory> list = new List<IInventory>();
			if (fridge != null)
			{
				list.Add(fridge.Items);
			}
			foreach (Chest current in mini_fridges)
			{
				list.Add(current.Items);
			}
			Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
			Game1.activeClickableMenu = new CraftingPage((int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: true, standaloneMenu: true, list);
			Game1.activeClickableMenu.exitFunction = request.ReleaseLocks;
		}, delegate
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Kitchen_InUse"));
		});
	}

	public void openDoor(Location tileLocation, bool playSound)
	{
		try
		{
			int tileIndex = this.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "1");
			Point point = new Point(tileLocation.X, tileLocation.Y);
			if (!this.interiorDoors.ContainsKey(point))
			{
				return;
			}
			this.interiorDoors[point] = true;
			if (playSound)
			{
				Vector2 pos = new Vector2(tileLocation.X, tileLocation.Y);
				if (tileIndex == 120)
				{
					this.playSound("doorOpen", pos);
				}
				else
				{
					this.playSound("doorCreak", pos);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void doStarpoint(string which)
	{
		if (!(which == "3"))
		{
			if (which == "4" && Game1.player.ActiveObject != null && Game1.player.ActiveObject.QualifiedItemId == "(O)203")
			{
				Object reward = ItemRegistry.Create<Object>("(BC)162");
				if (!Game1.player.couldInventoryAcceptThisItem(reward) && Game1.player.ActiveObject.stack.Value > 1)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					return;
				}
				Game1.player.reduceActiveItemByOne();
				Game1.player.makeThisTheActiveObject(reward);
				this.localSound("croak");
				Game1.flashAlpha = 1f;
			}
		}
		else if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.QualifiedItemId == "(O)307")
		{
			Object reward2 = ItemRegistry.Create<Object>("(BC)161");
			if (!Game1.player.couldInventoryAcceptThisItem(reward2) && Game1.player.ActiveObject.stack.Value > 1)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				return;
			}
			Game1.player.reduceActiveItemByOne();
			Game1.player.makeThisTheActiveObject(reward2);
			this.localSound("discoverMineral");
			Game1.flashAlpha = 1f;
		}
	}

	public virtual string FormatCompletionLine(Func<Farmer, float> check)
	{
		KeyValuePair<Farmer, float> kvp = Utility.GetFarmCompletion(check);
		if (kvp.Key == Game1.player)
		{
			return kvp.Value.ToString();
		}
		return "(" + kvp.Key.Name + ") " + kvp.Value;
	}

	public virtual string FormatCompletionLine(Func<Farmer, bool> check, string true_value, string false_value)
	{
		KeyValuePair<Farmer, bool> kvp = Utility.GetFarmCompletion(check);
		if (kvp.Key == Game1.player)
		{
			if (!kvp.Value)
			{
				return false_value;
			}
			return true_value;
		}
		return "(" + kvp.Key.Name + ") " + (kvp.Value ? true_value : false_value);
	}

	public virtual void ShowQiCat()
	{
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.MasterPlayer.mailReceived.Contains("GotPerfectionStatue"))
		{
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "GotPerfectionStatue", MailType.Received, add: true);
			Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)280"));
			return;
		}
		if (!Game1.player.hasOrWillReceiveMail("FizzIntro"))
		{
			Game1.addMailForTomorrow("FizzIntro", noLetter: false, sendToEveryone: true);
		}
		Game1.playSound("qi_shop");
		int totalWaivers = Game1.netWorldState.Value.PerfectionWaivers;
		double totalPercent = Math.Floor(Utility.percentGameComplete() * 100f);
		if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ja || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ko || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
		{
			totalPercent += (double)totalWaivers;
		}
		string totalStr = totalWaivers switch
		{
			0 => Game1.content.LoadString("Strings\\UI:PT_Total_Value", totalPercent), 
			1 => Game1.content.LoadString("Strings\\UI:PT_Total_ValueWithWaiver", totalPercent), 
			_ => Game1.content.LoadString("Strings\\UI:PT_Total_ValueWithWaivers", totalPercent, totalWaivers), 
		};
		List<string> brokenUp = SpriteText.getStringBrokenIntoSectionsOfHeight(string.Concat(Utility.loadStringShort("UI", "PT_Title") + "^", "----------------^", Utility.loadStringShort("UI", "PT_Shipped") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getFarmerItemsShippedPercent(farmer) * 100f)) + "%^", Utility.loadStringShort("UI", "PT_Obelisks") + ": " + Math.Min(Utility.GetObeliskTypesBuilt(), 4) + "/4^", Utility.loadStringShort("UI", "PT_GoldClock") + ": " + (Game1.IsBuildingConstructed("Gold Clock") ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^", Utility.loadStringShort("UI", "PT_MonsterSlayer") + ": " + this.FormatCompletionLine((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value, Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^", Utility.loadStringShort("UI", "PT_GreatFriends") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getMaxedFriendshipPercent(farmer) * 100f)) + "%^", Utility.loadStringShort("UI", "PT_FarmerLevel") + ": " + this.FormatCompletionLine((Farmer farmer) => Math.Min(farmer.Level, 25)) + "/25^", Utility.loadStringShort("UI", "PT_Stardrops") + ": " + this.FormatCompletionLine((Farmer farmer) => Utility.foundAllStardrops(farmer), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"), Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "^", Utility.loadStringShort("UI", "PT_Cooking") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getCookedRecipesPercent(farmer) * 100f)) + "%^", Utility.loadStringShort("UI", "PT_Crafting") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getCraftedRecipesPercent(farmer) * 100f)) + "%^", Utility.loadStringShort("UI", "PT_Fish") + ": " + this.FormatCompletionLine((Farmer farmer) => (float)Math.Floor(Utility.getFishCaughtPercent(farmer) * 100f)) + "%^", Utility.loadStringShort("UI", "PT_GoldenWalnut") + ": " + Math.Min(Game1.netWorldState.Value.GoldenWalnutsFound, 130) + "/" + 130 + "^", "----------------^", Utility.loadStringShort("UI", "PT_Total") + ": " + totalStr), 9999, Game1.uiViewport.Height - 100);
		for (int i = 0; i < brokenUp.Count - 1; i++)
		{
			brokenUp[i] += "...\n";
		}
		Game1.drawDialogueNoTyping(brokenUp);
	}

	/// <summary>Search a garbage can for a player if they haven't searched it today, and give or drop the resulting item (if any).</summary>
	/// <param name="id">The unique ID for the garbage can to search.</param>
	/// <param name="tile">The tile position for the garbage can being searched.</param>
	/// <param name="who">The player performing the search.</param>
	/// <param name="playAnimations">Whether to play animations and sounds.</param>
	/// <param name="reactNpcs">Whether nearby NPCs should react to the search (e.g. friendship point impact or dialogue).</param>
	/// <param name="logError">Log an error if the search fails due to invalid data, or <c>null</c> to fail silently.</param>
	/// <returns>Returns whether the garbage can was searched successfully, regardless of whether an item was found.</returns>
	public virtual bool CheckGarbage(string id, Vector2 tile, Farmer who, bool playAnimations = true, bool reactNpcs = true, Action<string> logError = null)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			logError?.Invoke("must specify a garbage can ID");
			return false;
		}
		switch (id)
		{
		case "0":
			id = "JodiAndKent";
			break;
		case "1":
			id = "EmilyAndHaley";
			break;
		case "2":
			id = "Mayor";
			break;
		case "3":
			id = "Museum";
			break;
		case "4":
			id = "Blacksmith";
			break;
		case "5":
			id = "Saloon";
			break;
		case "6":
			id = "Evelyn";
			break;
		case "7":
			id = "JojaMart";
			break;
		}
		if (!Game1.netWorldState.Value.CheckedGarbage.Add(id))
		{
			Game1.haltAfterCheck = false;
			return true;
		}
		this.TryGetGarbageItem(id, who.DailyLuck, out var item, out var selected, out var garbageRandom, logError);
		if (playAnimations)
		{
			bool doubleMega = selected?.IsDoubleMegaSuccess ?? false;
			bool mega = !doubleMega && (selected?.IsMegaSuccess ?? false);
			if (doubleMega)
			{
				this.playSound("explosion");
			}
			else if (mega)
			{
				this.playSound("crit");
			}
			this.playSound("trashcan");
			int tileY = (int)tile.Y;
			int xSourceOffset = this.GetSeasonIndex() * 17;
			TemporaryAnimatedSprite lidSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + xSourceOffset, 0, 16, 10), tile * 64f + new Vector2(0f, -6f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = (doubleMega ? 4000 : 1000),
				motion = (doubleMega ? new Vector2(4f, -20f) : new Vector2(0f, -8f + (mega ? (-7f) : ((float)(garbageRandom.Next(-1, 3) + ((garbageRandom.NextDouble() < 0.1) ? (-2) : 0)))))),
				rotationChange = (doubleMega ? 0.4f : 0f),
				acceleration = new Vector2(0f, 0.7f),
				yStopCoordinate = tileY * 64 + -24,
				layerDepth = (doubleMega ? 1f : ((float)((tileY + 1) * 64 + 2) / 10000f)),
				scale = 4f,
				Parent = this,
				shakeIntensity = (doubleMega ? 0f : 1f),
				reachedStopCoordinate = delegate
				{
					this.removeTemporarySpritesWithID(97654);
					this.playSound("thudStep");
					for (int j = 0; j < 3; j++)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), tile * 64f + new Vector2(j * 6, -3 + garbageRandom.Next(3)) * 4f, flipped: false, 0.02f, Color.DimGray)
						{
							alpha = 0.85f,
							motion = new Vector2(-0.6f + (float)j * 0.3f, -1f),
							acceleration = new Vector2(0.002f, 0f),
							interval = 99999f,
							layerDepth = (float)((tileY + 1) * 64 + 3) / 10000f,
							scale = 3f,
							scaleChange = 0.02f,
							rotationChange = (float)garbageRandom.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = 50
						});
					}
				},
				id = 97654
			};
			TemporaryAnimatedSprite bodySprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + xSourceOffset, 11, 16, 16), tile * 64f + new Vector2(0f, -5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = (doubleMega ? 999999 : 1000),
				layerDepth = (float)((tileY + 1) * 64 + 1) / 10000f,
				scale = 4f,
				id = 97654
			};
			if (doubleMega)
			{
				lidSprite.reachedStopCoordinate = lidSprite.bounce;
			}
			TemporaryAnimatedSpriteList trashCanSprites = new TemporaryAnimatedSpriteList { lidSprite, bodySprite };
			for (int i = 0; i < 5; i++)
			{
				TemporaryAnimatedSprite particleSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(22 + garbageRandom.Next(4) * 4, 32, 4, 4), tile * 64f + new Vector2(Game1.random.Next(13), -3 + Game1.random.Next(3)) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 500f,
					motion = new Vector2(garbageRandom.Next(-2, 3), -5f),
					acceleration = new Vector2(0f, 0.4f),
					layerDepth = (float)((tileY + 1) * 64 + 3) / 10000f,
					scale = 4f,
					color = Utility.getRandomRainbowColor(garbageRandom),
					delayBeforeAnimationStart = garbageRandom.Next(100)
				};
				trashCanSprites.Add(particleSprite);
			}
			Game1.multiplayer.broadcastSprites(this, trashCanSprites);
		}
		if (reactNpcs)
		{
			foreach (NPC npc in Utility.GetNpcsWithinDistance(tile, 7, this))
			{
				if (!(npc is Horse))
				{
					Game1.multiplayer.globalChatInfoMessage("TrashCan", who.Name, npc.GetTokenizedDisplayName());
					if (npc.Name == "Linus")
					{
						Game1.multiplayer.globalChatInfoMessage("LinusTrashCan");
					}
					CharacterData data = npc.GetData();
					int friendshipChange = data?.DumpsterDiveFriendshipEffect ?? (-25);
					int? emote = data?.DumpsterDiveEmote;
					Dialogue dialogue = npc.TryGetDialogue("DumpsterDiveComment");
					switch (npc.Age)
					{
					case 2:
						emote = emote ?? 28;
						dialogue = dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Child");
						break;
					case 1:
						emote = emote ?? 8;
						dialogue = dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen");
						break;
					default:
						emote = emote ?? 12;
						dialogue = dialogue ?? new Dialogue(npc, "Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult");
						break;
					}
					npc.doEmote(emote.Value);
					who.changeFriendship(friendshipChange, npc);
					npc.setNewDialogue(dialogue, add: true, clearOnMovement: true);
					Game1.drawDialogue(npc);
					break;
				}
			}
		}
		Game1.stats.Increment("trashCansChecked");
		if (selected != null)
		{
			if (selected.AddToInventoryDirectly)
			{
				who.addItemByMenuIfNecessary(item);
			}
			else
			{
				Vector2 origin = new Vector2(tile.X + 0.5f, tile.Y - 1f) * 64f;
				if (selected.CreateMultipleDebris)
				{
					Game1.createMultipleItemDebris(item, origin, 2, this, (int)origin.Y + 64);
				}
				else
				{
					Game1.createItemDebris(item, origin, 2, this, (int)origin.Y + 64);
				}
			}
		}
		return true;
	}

	/// <summary>Try to get the item that would be produced by checking a garbage can in the location, without marking it checked or playing animations or sounds.</summary>
	/// <param name="id">The garbage can ID in <c>Data/GarbageCans</c>.</param>
	/// <param name="dailyLuck">The daily luck of the player checking the garbage can.</param>
	/// <param name="item">The item produced by the garbage can, if any.</param>
	/// <param name="selected">The data entry which produced the <paramref name="item" />, if applicable.</param>
	/// <param name="garbageRandom">The RNG used to select the item, and which would normally be used for subsequent effects like animations.</param>
	/// <param name="logError">Log an error if the search fails due to invalid data, or <c>null</c> to fail silently.</param>
	/// <returns>Returns whether an item was produced.</returns>
	public virtual bool TryGetGarbageItem(string id, double dailyLuck, out Item item, out GarbageCanItemData selected, out Random garbageRandom, Action<string> logError = null)
	{
		GarbageCanData allData = DataLoader.GarbageCans(Game1.content);
		GarbageCanEntryData data = allData.GarbageCans.GetValueOrDefault(id);
		float baseChance = ((data != null && data.BaseChance > 0f) ? data.BaseChance : allData.DefaultBaseChance);
		baseChance += (float)dailyLuck;
		if (Game1.player.stats.Get("Book_Trash") != 0)
		{
			baseChance += 0.2f;
		}
		garbageRandom = Utility.CreateDaySaveRandom(777 + Game1.hash.GetDeterministicHashCode(id));
		int prewarm = garbageRandom.Next(0, 100);
		for (int i = 0; i < prewarm; i++)
		{
			garbageRandom.NextDouble();
		}
		prewarm = garbageRandom.Next(0, 100);
		for (int j = 0; j < prewarm; j++)
		{
			garbageRandom.NextDouble();
		}
		selected = null;
		item = null;
		bool baseChancePassed = garbageRandom.NextDouble() < (double)baseChance;
		ItemQueryContext itemQueryContext = new ItemQueryContext(this, Game1.player, garbageRandom, "garbage data '" + id + "'");
		List<GarbageCanItemData>[] array = new List<GarbageCanItemData>[3]
		{
			allData.BeforeAll,
			data?.Items,
			allData.AfterAll
		};
		foreach (List<GarbageCanItemData> itemList in array)
		{
			if (itemList == null)
			{
				continue;
			}
			foreach (GarbageCanItemData entry in itemList)
			{
				if (string.IsNullOrWhiteSpace(entry.Id))
				{
					logError("ignored item entry with no Id field.");
				}
				else if ((baseChancePassed || entry.IgnoreBaseChance) && GameStateQuery.CheckConditions(entry.Condition, this, null, null, null, garbageRandom))
				{
					bool error = false;
					Item result = ItemQueryResolver.TryResolveRandomItem(entry, itemQueryContext, avoidRepeat: false, null, null, null, delegate(string query, string message)
					{
						error = true;
						logError("failed parsing item query '" + query + "': " + message);
					});
					if (!error)
					{
						selected = entry;
						item = result;
						break;
					}
				}
			}
			if (selected != null)
			{
				break;
			}
		}
		return item != null;
	}

	/// <summary>Handle an <c>Action</c> property from a <c>Buildings</c> map tile in the location when the player interacts with the tile.</summary>
	/// <param name="fullActionString">The full action string to parse, <strong>excluding</strong> the <c>Action</c> prefix.</param>
	/// <param name="who">The player performing the action.</param>
	/// <param name="tileLocation">The tile coordinate of the action to handle.</param>
	public virtual bool performAction(string fullActionString, Farmer who, Location tileLocation)
	{
		if (fullActionString == null)
		{
			return false;
		}
		string[] action = ArgUtility.SplitBySpace(fullActionString);
		return this.performAction(action, who, tileLocation);
	}

	/// <summary>Get whether an <c>Action</c> property from a <c>Buildings</c> map tile in the location should be ignored, so it doesn't show an action cursor and isn't triggered on click.</summary>
	/// <param name="action">The action arguments to parse, including the <c>Action</c> prefix.</param>
	/// <param name="who">The player performing the action.</param>
	/// <param name="tileLocation">The tile coordinate of the action to handle.</param>
	public virtual bool ShouldIgnoreAction(string[] action, Farmer who, Location tileLocation)
	{
		string actionType = ArgUtility.Get(action, 0);
		if (string.IsNullOrWhiteSpace(actionType))
		{
			return true;
		}
		if (!(actionType == "DropBox"))
		{
			if (actionType == "MonsterGrave")
			{
				return !who.eventsSeen.Contains("6963327");
			}
			return false;
		}
		if (Game1.player.team.specialOrders != null)
		{
			string boxId = ArgUtility.Get(action, 1);
			if (boxId != null)
			{
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					if (specialOrder.UsesDropBox(boxId))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	/// <summary>Displays a message when a player is not good enough friends with a villager to enter their room.</summary>
	/// <param name="action">The action arguments from <see cref="M:StardewValley.GameLocation.performAction(System.String,StardewValley.Farmer,xTile.Dimensions.Location)" />.</param>
	public virtual void ShowLockedDoorMessage(string[] action)
	{
		Gender ownerGender = Gender.Female;
		string ownerName = null;
		string[] ownerDisplayNames = new string[(action.Length == 2) ? 1 : 2];
		for (int i = 0; i < ownerDisplayNames.Length; i++)
		{
			string ownerKey = action[i + 1];
			NPC owner = Game1.getCharacterFromName(ownerKey);
			if (owner != null)
			{
				ownerName = owner.Name;
				ownerGender = owner.Gender;
				ownerDisplayNames[i] = owner.displayName;
				continue;
			}
			if (NPC.TryGetData(ownerKey, out var data))
			{
				ownerName = ownerKey;
				ownerGender = data.Gender;
				ownerDisplayNames[i] = TokenParser.ParseText(data.DisplayName);
				continue;
			}
			return;
		}
		string lockedDoorMessage;
		if (ownerDisplayNames.Length > 1)
		{
			lockedDoorMessage = Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Couple", ownerDisplayNames[0], ownerDisplayNames[1]);
		}
		else
		{
			string ownerDisplayName = ownerDisplayNames[0];
			lockedDoorMessage = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:DoorUnlock_NotFriend_" + ownerName) ?? Game1.content.LoadStringReturnNullIfNotFound($"Strings\\Locations:DoorUnlock_NotFriend_{ownerGender}", ownerDisplayName) ?? Game1.content.LoadString("Strings\\Locations:DoorUnlock_NotFriend_Female", ownerDisplayName);
		}
		Game1.drawObjectDialogue(lockedDoorMessage);
	}

	/// <summary>Handle an <c>Action</c> property from a <c>Buildings</c> map tile in the location when the player interacts with the tile.</summary>
	/// <param name="action">The action arguments to parse, <strong>excluding</strong> the <c>Action</c> prefix.</param>
	/// <param name="who">The player performing the action.</param>
	/// <param name="tileLocation">The tile coordinate of the action to handle.</param>
	public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (this.ShouldIgnoreAction(action, who, tileLocation))
		{
			return false;
		}
		if (!ArgUtility.TryGet(action, 0, out var actionType, out var error, allowBlank: true, "string actionType"))
		{
			return LogError(error);
		}
		if (who.IsLocalPlayer)
		{
			if (GameLocation.registeredTileActions.TryGetValue(actionType, out var actionHandler))
			{
				return actionHandler(this, action, who, new Point(tileLocation.X, tileLocation.Y));
			}
			switch (actionType)
			{
			case "None":
				return true;
			case "BuildingGoldClock":
			{
				bool clockOn = !Game1.netWorldState.Value.goldenClocksTurnedOff.Value;
				who.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:GoldClock_" + (clockOn ? "Off" : "On")), who.currentLocation.createYesNoResponses(), "GoldClock");
				break;
			}
			case "Bobbers":
				Game1.activeClickableMenu = new ChooseFromIconsMenu("bobbers");
				break;
			case "GrandpaMasteryNote":
				Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\1_6_Strings:GrandpaMasteryNote", Game1.player.Name, Game1.player.farmName));
				break;
			case "Bookseller":
				if (Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
				{
					if (Game1.player.mailReceived.Contains("read_a_book"))
					{
						this.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:books_welcome"), new Response[3]
						{
							new Response("Buy", Game1.content.LoadString("Strings\\1_6_Strings:buy_books")),
							new Response("Trade", Game1.content.LoadString("Strings\\1_6_Strings:trade_books")),
							new Response("Leave", Game1.content.LoadString("Strings\\1_6_Strings:Leave"))
						}, "Bookseller");
					}
					else
					{
						Utility.TryOpenShopMenu("Bookseller", null, playOpenSound: true);
					}
				}
				break;
			case "MasteryCave_Pedestal":
				Game1.activeClickableMenu = new MasteryTrackerMenu();
				break;
			case "MasteryCave_Farming":
				if (Game1.player.stats.Get(StatKeys.Mastery(0)) >= 0)
				{
					Game1.activeClickableMenu = new MasteryTrackerMenu(0);
				}
				break;
			case "MasteryCave_Fishing":
				if (Game1.player.stats.Get(StatKeys.Mastery(1)) >= 0)
				{
					Game1.activeClickableMenu = new MasteryTrackerMenu(1);
				}
				break;
			case "MasteryCave_Foraging":
				if (Game1.player.stats.Get(StatKeys.Mastery(2)) >= 0)
				{
					Game1.activeClickableMenu = new MasteryTrackerMenu(2);
				}
				break;
			case "MasteryCave_Combat":
				if (Game1.player.stats.Get(StatKeys.Mastery(4)) >= 0)
				{
					Game1.activeClickableMenu = new MasteryTrackerMenu(4);
				}
				break;
			case "MasteryCave_Mining":
				if (Game1.player.stats.Get(StatKeys.Mastery(3)) >= 0)
				{
					Game1.activeClickableMenu = new MasteryTrackerMenu(3);
				}
				break;
			case "MasteryRoom":
			{
				int totalSkills = Game1.player.farmingLevel.Value / 10 + Game1.player.fishingLevel.Value / 10 + Game1.player.foragingLevel.Value / 10 + Game1.player.miningLevel.Value / 10 + Game1.player.combatLevel.Value / 10;
				if (totalSkills >= 5)
				{
					Game1.playSound("doorClose");
					Game1.warpFarmer("MasteryCave", 7, 11, 0);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", totalSkills));
				}
				break;
			}
			case "PrizeMachine":
				Game1.activeClickableMenu = new PrizeTicketMenu();
				break;
			case "SquidFestBooth":
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SquidFestBooth_Intro"), new Response[3]
				{
					new Response("Rewards", Game1.content.LoadString("Strings\\1_6_Strings:GetRewards")),
					new Response("Explanation", Game1.content.LoadString("Strings\\1_6_Strings:Explanation")),
					new Response("Leave", Game1.content.LoadString("Strings\\1_6_Strings:Leave"))
				}, "SquidFestBooth");
				break;
			case "TroutDerbyBooth":
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_Intro"), new Response[3]
				{
					new Response("Rewards", Game1.content.LoadString("Strings\\1_6_Strings:GetRewards")),
					new Response("Explanation", Game1.content.LoadString("Strings\\1_6_Strings:Explanation")),
					new Response("Leave", Game1.content.LoadString("Strings\\1_6_Strings:Leave"))
				}, "TroutDerbyBooth");
				break;
			case "FishingDerbySign":
				Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString(Game1.IsSummer ? "Strings\\1_6_Strings:FishingDerbySign" : "Strings\\1_6_Strings:SquidFestSign"));
				break;
			case "SpecialWaterDroppable":
				if (!(this is MineShaft) || (this as MineShaft).mineLevel == 100)
				{
					if (who?.ActiveObject?.QualifiedItemId == "(O)103")
					{
						this.localSound("throwDownITem");
						who.reduceActiveItemByOne();
						TemporaryAnimatedSprite tempSprite = new TemporaryAnimatedSprite(103, 9999f, 1, 1, who.position.Value + new Vector2(0f, -128f), flicker: false, bigCraftable: false, flipped: false)
						{
							motion = new Vector2(4f, -4f),
							acceleration = new Vector2(0f, 0.3f),
							yStopCoordinate = (int)who.position.Y,
							id = 777
						};
						who.freezePause = 4000;
						tempSprite.reachedStopCoordinate = delegate
						{
							this.removeTemporarySpritesWithID(777);
							this.temporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, tempSprite.position, flicker: false, flipped: false)
							{
								color = Color.OrangeRed
							});
							this.localSound("dropItemInWater");
							DelayedAction.functionAfterDelay(delegate
							{
								this.localSound("terraria_boneSerpent");
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(128, 96, 32, 32), 70f, 4, 5, tempSprite.position + new Vector2(-5f, -3f) * 4f, flicker: false, flipped: true, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f));
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(128, 96, 32, 32), 60f, 4, 5, tempSprite.position + new Vector2(-5f, 7f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(134, 2, 21, 38), 9999f, 1, 1, tempSprite.position, flicker: false, flipped: false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									xPeriodic = true,
									xPeriodicLoopTime = 500f,
									xPeriodicRange = 2f,
									motion = new Vector2(0f, -8f)
								});
								for (int j = 0; j < 13; j++)
								{
									this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(134, (j == 12) ? 54 : 41, 21, 12), 9999f, 1, 1, tempSprite.position, flicker: false, flipped: false, 0.97f - (float)j * 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
									{
										xPeriodic = true,
										xPeriodicLoopTime = 500 + Game1.random.Next(-50, 50),
										xPeriodicRange = 2f,
										motion = new Vector2(0f, -8f),
										delayBeforeAnimationStart = 220 + 80 * j
									});
								}
								TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(935, 9999f, 1, 1, tempSprite.position + new Vector2(0f, -128f), flicker: false, bigCraftable: false, flipped: false)
								{
									motion = new Vector2(-4f, -4f),
									acceleration = new Vector2(0f, 0.3f),
									yStopCoordinate = (int)(who.position.Y - 128f + 12f),
									id = 888
								};
								temporaryAnimatedSprite.reachedStopCoordinate = delegate
								{
									who.addItemByMenuIfNecessary(new Object("FarAwayStone", 1));
									who.currentLocation.removeTemporarySpritesWithID(888);
									this.localSound("coin");
								};
								who.currentLocation.temporarySprites.Add(temporaryAnimatedSprite);
							}, 1000);
						};
						this.temporarySprites.Add(tempSprite);
						return true;
					}
					if (who?.ActiveObject != null && !who.ActiveObject.questItem.Value && who.ActiveObject.QualifiedItemId != "(O)FarAwayStone" && who.ActiveObject.Edibility <= 0 && !who.ActiveObject.Name.Contains("Totem"))
					{
						ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(who?.ActiveObject.QualifiedItemId);
						if (itemData != null)
						{
							this.localSound("throwDownITem");
							int _id = Game1.random.Next();
							TemporaryAnimatedSprite tempSprite2 = new TemporaryAnimatedSprite(itemData.GetTextureName(), itemData.GetSourceRect(), 9999f, 1, 1, who.position.Value + new Vector2(0f, -128f), flicker: false, flipped: false)
							{
								motion = new Vector2(4f, -4f),
								acceleration = new Vector2(0f, 0.3f),
								yStopCoordinate = (int)who.position.Y,
								id = _id,
								scale = 4f * ((itemData.GetSourceRect().Height > 32) ? 0.5f : 1f)
							};
							who.reduceActiveItemByOne();
							tempSprite2.reachedStopCoordinate = delegate
							{
								this.removeTemporarySpritesWithID(_id);
								this.temporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, tempSprite2.position, flicker: false, flipped: false)
								{
									color = Color.OrangeRed
								});
								this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), tempSprite2.position + new Vector2(2f, 0f) * 4f, flipped: false, 0f, Color.White)
								{
									interval = 50f,
									totalNumberOfLoops = 99999,
									animationLength = 4,
									scale = 4f,
									layerDepth = 0.99f,
									alphaFade = 0.02f
								});
								for (int j = 0; j < 4; j++)
								{
									this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1965, 8, 8), tempSprite2.position + new Vector2(2f, 0f) * 4f, flipped: false, 0f, Color.White)
									{
										motion = new Vector2((float)Game1.random.Next(-15, 26) / 10f, -4f),
										acceleration = new Vector2(0f, (float)Game1.random.Next(3, 7) / 30f),
										interval = 50f,
										totalNumberOfLoops = 99999,
										animationLength = 7,
										scale = 4f,
										layerDepth = 0.99f,
										alphaFade = 0.02f,
										delayBeforeAnimationStart = j * 30
									});
								}
								this.localSound("dropItemInWater");
								this.localSound("fireball");
							};
							this.temporarySprites.Add(tempSprite2);
						}
						return true;
					}
				}
				return false;
			case "ForestPylon":
				if (who?.ActiveObject?.QualifiedItemId == "(O)FarAwayStone")
				{
					who.reduceActiveItemByOne();
					Game1.playSound("openBox");
					Game1.player.mailReceived.Add("hasActivatedForestPylon");
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\terraria_cat", new Microsoft.Xna.Framework.Rectangle(0, 106, 14, 22), new Vector2(16.6f, 2.5f) * 64f, flipped: false, 0f, Color.White)
					{
						animationLength = 8,
						interval = 100f,
						totalNumberOfLoops = 9999,
						scale = 4f
					});
					Game1.player.freezePause = 3000;
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.globalFadeToBlack(delegate
						{
							this.startEvent(new Event(Game1.content.LoadString("Strings\\1_6_Strings:ForestPylonEvent")));
						});
					}, 1000);
				}
				else if (who.mailReceived.Contains("hasActivatedForestPylon"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:ForestPylonActivated"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:ForestPylon"));
				}
				break;
			case "Garbage":
			{
				if (!ArgUtility.TryGet(action, 1, out var id, out error, allowBlank: true, "string id"))
				{
					return LogError(error);
				}
				this.CheckGarbage(id, new Vector2(tileLocation.X, tileLocation.Y), who, playAnimations: true, reactNpcs: true, delegate(string garbageError)
				{
					Game1.log.Warn($"Ignored invalid 'Action Garbage {id}' property: {garbageError}.");
				});
				Game1.haltAfterCheck = false;
				return true;
			}
			case "kitchen":
			case "Kitchen":
				this.ActivateKitchen();
				return true;
			case "Forge":
				Game1.activeClickableMenu = new ForgeMenu();
				return true;
			case "SummitBoulder":
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SummitBoulder"));
				break;
			case "QiCat":
				this.ShowQiCat();
				break;
			case "QiGemShop":
				return Utility.TryOpenShopMenu("QiGemShop", null, playOpenSound: true);
			case "QiChallengeBoard":
				Game1.player.team.qiChallengeBoardMutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new SpecialOrdersBoard("Qi")
					{
						behaviorBeforeCleanup = delegate
						{
							Game1.player.team.qiChallengeBoardMutex.ReleaseLock();
						}
					};
				});
				break;
			case "SpecialOrdersPrizeTickets":
				if (Game1.player.stats.Get("specialOrderPrizeTickets") != 0)
				{
					if (Game1.player.couldInventoryAcceptThisItem(ItemRegistry.Create("(O)PrizeTicket")))
					{
						Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)PrizeTicket"));
						Game1.player.stats.Decrement("specialOrderPrizeTickets");
						Game1.playSound("coin");
					}
					else
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					}
				}
				break;
			case "SpecialOrders":
				Game1.player.team.ordersBoardMutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new SpecialOrdersBoard
					{
						behaviorBeforeCleanup = delegate
						{
							Game1.player.team.ordersBoardMutex.ReleaseLock();
						}
					};
				});
				break;
			case "MonsterGrave":
				Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Backwoods_MonsterGrave").Split('#'));
				break;
			case "ObeliskWarp":
			{
				if (!ArgUtility.TryGet(action, 1, out var targetLocation, out error, allowBlank: true, "string targetLocation") || !ArgUtility.TryGetPoint(action, 2, out var targetTile, out error, "Point targetTile") || !ArgUtility.TryGetOptionalBool(action, 4, out var forceDismount, out error, defaultValue: false, "bool forceDismount"))
				{
					return LogError(error);
				}
				Building.PerformObeliskWarp(targetLocation, targetTile.X, targetTile.Y, forceDismount, who);
				return true;
			}
			case "PlayEvent":
			{
				if (!ArgUtility.TryGet(action, 1, out var eventId, out error, allowBlank: true, "string eventId") || !ArgUtility.TryGetOptionalBool(action, 2, out var checkPreconditions, out error, defaultValue: true, "bool checkPreconditions") || !ArgUtility.TryGetOptionalBool(action, 3, out var checkSeen, out error, defaultValue: true, "bool checkSeen") || !ArgUtility.TryGetOptionalRemainder(action, 4, out var fallbackAction))
				{
					return LogError(error);
				}
				if (Game1.PlayEvent(eventId, checkPreconditions, checkSeen))
				{
					return true;
				}
				if (fallbackAction != null)
				{
					return this.performAction(fallbackAction, who, tileLocation);
				}
				return false;
			}
			case "OpenShop":
			{
				if (!ArgUtility.TryGet(action, 1, out var shopId, out error, allowBlank: true, "string shopId") || !ArgUtility.TryGetOptional(action, 2, out var direction, out error, null, allowBlank: true, "string direction") || !ArgUtility.TryGetOptionalInt(action, 3, out var openTime2, out error, -1, "int openTime") || !ArgUtility.TryGetOptionalInt(action, 4, out var closeTime2, out error, -1, "int closeTime") || !ArgUtility.TryGetOptionalInt(action, 5, out var shopAreaX, out error, -1, "int shopAreaX") || !ArgUtility.TryGetOptionalInt(action, 6, out var shopAreaY, out error, -1, "int shopAreaY") || !ArgUtility.TryGetOptionalInt(action, 7, out var shopAreaWidth, out error, -1, "int shopAreaWidth") || !ArgUtility.TryGetOptionalInt(action, 8, out var shopAreaHeight, out error, -1, "int shopAreaHeight"))
				{
					return LogError(error);
				}
				Microsoft.Xna.Framework.Rectangle? ownerSearchArea2 = null;
				if (shopAreaX != -1 || shopAreaY != -1 || shopAreaWidth != -1 || shopAreaHeight != -1)
				{
					if (shopAreaX == -1 || shopAreaY == -1 || shopAreaWidth == -1 || shopAreaHeight == -1)
					{
						return LogError("when specifying any of the shop area 'x y width height' arguments (indexes 5-8), all four must be specified");
					}
					ownerSearchArea2 = new Microsoft.Xna.Framework.Rectangle(shopAreaX, shopAreaY, shopAreaWidth, shopAreaHeight);
				}
				switch (direction)
				{
				case "down":
					if (who.TilePoint.Y < tileLocation.Y)
					{
						return false;
					}
					break;
				case "up":
					if (who.TilePoint.Y > tileLocation.Y)
					{
						return false;
					}
					break;
				case "left":
					if (who.TilePoint.X > tileLocation.X)
					{
						return false;
					}
					break;
				case "right":
					if (who.TilePoint.X < tileLocation.X)
					{
						return false;
					}
					break;
				}
				if ((openTime2 >= 0 && Game1.timeOfDay < openTime2) || (closeTime2 >= 0 && Game1.timeOfDay >= closeTime2))
				{
					return false;
				}
				string shopId2 = shopId;
				Microsoft.Xna.Framework.Rectangle? ownerArea = ownerSearchArea2;
				bool forceOpen = !ownerSearchArea2.HasValue;
				return Utility.TryOpenShopMenu(shopId2, this, ownerArea, null, forceOpen);
			}
			case "Warp_Sunroom_Door":
				if (who.getFriendshipHeartLevelForNPC("Caroline") >= 2)
				{
					this.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
					Game1.warpFarmer("Sunroom", 5, 13, flip: false);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Caroline_Sunroom_Door"));
				}
				break;
			case "DogStatue":
			{
				if (GameLocation.canRespec(0) || GameLocation.canRespec(3) || GameLocation.canRespec(2) || GameLocation.canRespec(4) || GameLocation.canRespec(1))
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue"), this.createYesNoResponses(), "dogStatue");
					break;
				}
				string displayed_text = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
				displayed_text = displayed_text.Substring(0, displayed_text.LastIndexOf('^'));
				Game1.drawObjectDialogue(displayed_text);
				break;
			}
			case "WizardBook":
				if (who.mailReceived.Contains("hasPickedUpMagicInk") || who.hasMagicInk)
				{
					this.ShowConstructOptions("Wizard");
				}
				break;
			case "EvilShrineLeft":
				if (who.getChildrenCount() == 0)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeftInactive"));
				}
				else
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineLeft"), this.createYesNoResponses(), "evilShrineLeft");
				}
				break;
			case "EvilShrineCenter":
				if (who.isDivorced())
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenter"), this.createYesNoResponses(), "evilShrineCenter");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineCenterInactive"));
				}
				break;
			case "EvilShrineRight":
				if (Game1.spawnMonstersAtNight)
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightDeActivate"), this.createYesNoResponses(), "evilShrineRightDeActivate");
				}
				else
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_EvilShrineRightActivate"), this.createYesNoResponses(), "evilShrineRightActivate");
				}
				break;
			case "Tailoring":
				if (who.eventsSeen.Contains("992559"))
				{
					Game1.activeClickableMenu = new TailoringMenu();
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_SewingMachine"));
				}
				break;
			case "DyePot":
				if (who.eventsSeen.Contains("992559"))
				{
					if (!DyeMenu.IsWearingDyeable())
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable"));
					}
					else
					{
						Game1.activeClickableMenu = new DyeMenu();
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_DyePot"));
				}
				break;
			case "MagicInk":
				if (who.mailReceived.Add("hasPickedUpMagicInk"))
				{
					who.hasMagicInk = true;
					this.setMapTile(4, 11, 113, "Buildings", "untitled tile sheet");
					who.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(7));
				}
				break;
			case "LeoParrot":
				(this.getTemporarySpriteByID(5858585) as EmilysParrot)?.doAction();
				break;
			case "EmilyRoomObject":
				if (Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse != "Emily")
				{
					(this.getTemporarySpriteByID(5858585) as EmilysParrot)?.doAction();
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:HaleyHouse_EmilyRoomObject"));
				}
				break;
			case "Starpoint":
			{
				if (!ArgUtility.TryGet(action, 1, out var which, out error, allowBlank: true, "string which"))
				{
					return LogError(error);
				}
				this.doStarpoint(which);
				break;
			}
			case "JojaShop":
				Utility.TryOpenShopMenu("Joja", null, playOpenSound: true);
				break;
			case "ColaMachine":
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_ColaMachine_Question"), this.createYesNoResponses(), "buyJojaCola");
				break;
			case "IceCreamStand":
			{
				Microsoft.Xna.Framework.Rectangle npcArea = new Microsoft.Xna.Framework.Rectangle(tileLocation.X, tileLocation.Y - 3, 1, 3);
				Utility.TryOpenShopMenu("IceCreamStand", this, npcArea);
				break;
			}
			case "WizardShrine":
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WizardTower_WizardShrine").Replace('\n', '^'), this.createYesNoResponses(), "WizardShrine");
				break;
			case "HMTGF":
				if (who.ActiveObject != null && who.ActiveObject.QualifiedItemId == "(O)155")
				{
					Object reward = ItemRegistry.Create<Object>("(BC)155");
					if (!Game1.player.couldInventoryAcceptThisItem(reward) && Game1.player.ActiveObject.stack.Value > 1)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						break;
					}
					Game1.player.reduceActiveItemByOne();
					Game1.player.makeThisTheActiveObject(reward);
					this.localSound("discoverMineral");
					Game1.flashAlpha = 1f;
				}
				break;
			case "HospitalShop":
			{
				Point playerTile = who.TilePoint;
				Microsoft.Xna.Framework.Rectangle ownerSearchArea = new Microsoft.Xna.Framework.Rectangle(playerTile.X - 1, playerTile.Y - 2, 2, 1);
				Utility.TryOpenShopMenu("Hospital", this, ownerSearchArea);
				break;
			}
			case "BuyBackpack":
			{
				Response purchase2000 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000"));
				Response purchase10000 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000"));
				Response notNow = new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
				if (Game1.player.maxItems.Value == 12)
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24"), new Response[2] { purchase2000, notNow }, "Backpack");
				}
				else if (Game1.player.maxItems.Value < 36)
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36"), new Response[2] { purchase10000, notNow }, "Backpack");
				}
				break;
			}
			case "BuyQiCoins":
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_Buy100Coins"), this.createYesNoResponses(), "BuyQiCoins");
				break;
			case "LumberPile":
				if (!who.hasOrWillReceiveMail("TH_LumberPile") && who.hasOrWillReceiveMail("TH_SandDragon"))
				{
					Game1.player.hasClubCard = true;
					Game1.player.CanMove = false;
					Game1.player.mailReceived.Add("TH_LumberPile");
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(new SpecialItem(2));
					Game1.player.removeQuest("5");
				}
				break;
			case "SandDragon":
				if (who.ActiveObject?.QualifiedItemId == "(O)768" && !who.hasOrWillReceiveMail("TH_SandDragon") && who.hasOrWillReceiveMail("TH_MayorFridge"))
				{
					who.reduceActiveItemByOne();
					Game1.player.CanMove = false;
					this.localSound("eat");
					Game1.player.mailReceived.Add("TH_SandDragon");
					Game1.multipleDialogues(new string[2]
					{
						Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_ConsumeEssence"),
						Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote")
					});
					Game1.player.removeQuest("4");
					Game1.player.addQuest("5");
				}
				else if (who.hasOrWillReceiveMail("TH_SandDragon"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_MrQiNote"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Desert_SandDragon_Initial"));
				}
				break;
			case "RailroadBox":
				if (who.ActiveObject?.QualifiedItemId == "(O)394" && !who.hasOrWillReceiveMail("TH_Railroad") && who.hasOrWillReceiveMail("TH_Tunnel"))
				{
					who.reduceActiveItemByOne();
					Game1.player.CanMove = false;
					this.localSound("Ship");
					Game1.player.mailReceived.Add("TH_Railroad");
					Game1.multipleDialogues(new string[2]
					{
						Game1.content.LoadString("Strings\\Locations:Railroad_Box_ConsumeShell"),
						Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote")
					});
					Game1.player.removeQuest("2");
					Game1.player.addQuest("3");
				}
				else if (who.hasOrWillReceiveMail("TH_Railroad"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_MrQiNote"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Railroad_Box_Initial"));
				}
				break;
			case "TunnelSafe":
				if (who.ActiveObject?.QualifiedItemId == "(O)787" && !who.hasOrWillReceiveMail("TH_Tunnel"))
				{
					who.reduceActiveItemByOne();
					Game1.player.CanMove = false;
					this.playSound("openBox");
					DelayedAction.playSoundAfterDelay("doorCreakReverse", 500);
					Game1.player.mailReceived.Add("TH_Tunnel");
					Game1.multipleDialogues(new string[2]
					{
						Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_ConsumeBattery"),
						Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote")
					});
					Game1.player.addQuest("2");
				}
				else if (who.hasOrWillReceiveMail("TH_Tunnel"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_MrQiNote"));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Tunnel_TunnelSafe_Initial"));
				}
				break;
			case "SkullDoor":
				if (who.hasSkullKey || Utility.IsPassiveFestivalDay("DesertFestival"))
				{
					if (!who.hasUnlockedSkullDoor && !Utility.IsPassiveFestivalDay("DesertFestival"))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Unlock")));
						DelayedAction.playSoundAfterDelay("openBox", 500);
						DelayedAction.playSoundAfterDelay("openBox", 700);
						Game1.addMailForTomorrow("skullCave");
						who.hasUnlockedSkullDoor = true;
						who.completeQuest("19");
					}
					else
					{
						who.completelyStopAnimatingOrDoingAction();
						this.playSound("doorClose");
						DelayedAction.playSoundAfterDelay("stairsdown", 500, this);
						Game1.enterMine(121);
						MineShaft.numberOfCraftedStairsUsedThisRun = 0;
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SkullCave_SkullDoor_Locked"));
				}
				break;
			case "Crib":
				foreach (NPC n in this.characters)
				{
					if (!(n is Child child))
					{
						continue;
					}
					switch (child.Age)
					{
					case 1:
						child.toss(who);
						return true;
					case 0:
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FarmHouse_Crib_NewbornSleeping", n.displayName)));
						return true;
					case 2:
						if (child.isInCrib())
						{
							return n.checkAction(who, this);
						}
						break;
					}
				}
				return false;
			case "WarpGreenhouse":
				if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
				{
					who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
					this.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
					GameLocation greenhouse = Game1.getLocationFromName("Greenhouse");
					int destination_x = 10;
					int destination_y = 23;
					if (greenhouse != null)
					{
						foreach (Warp warp in greenhouse.warps)
						{
							if (warp.TargetName == "Farm")
							{
								destination_x = warp.X;
								destination_y = warp.Y - 1;
								break;
							}
						}
					}
					Game1.warpFarmer("Greenhouse", destination_x, destination_y, flip: false);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GreenhouseRuins"));
				}
				break;
			case "Arcade_Prairie":
				this.showPrairieKingMenu();
				break;
			case "Arcade_Minecart":
				if (who.hasSkullKey)
				{
					Response[] junimoKartOptions = new Response[3]
					{
						new Response("Progress", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_ProgressMode")),
						new Response("Endless", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_EndlessMode")),
						new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
					};
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), junimoKartOptions, "MinecartGame");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Inactive"));
				}
				break;
			case "WarpCommunityCenter":
				if (Game1.MasterPlayer.mailReceived.Contains("ccDoorUnlock") || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
				{
					this.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
					Game1.warpFarmer("CommunityCenter", 32, 23, flip: false);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8175"));
				}
				break;
			case "AdventureShop":
				this.adventureShop();
				break;
			case "Warp":
			{
				if (!ArgUtility.TryGetPoint(action, 1, out var tile2, out error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName2, out error, allowBlank: true, "string locationName"))
				{
					return LogError(error);
				}
				bool num = action.Length < 5;
				who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
				Rumble.rumble(0.15f, 200f);
				if (num)
				{
					this.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
				}
				Game1.warpFarmer(locationName2, tile2.X, tile2.Y, flip: false);
				break;
			}
			case "WarpWomensLocker":
			{
				if (!ArgUtility.TryGetPoint(action, 1, out var tile, out error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName, out error, allowBlank: true, "string locationName"))
				{
					return LogError(error);
				}
				bool playDoorSound = action.Length < 5;
				if (who.IsMale)
				{
					if (who.IsLocalPlayer)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WomensLocker_WrongGender"));
					}
					return true;
				}
				who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
				if (playDoorSound)
				{
					this.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
				}
				Game1.warpFarmer(locationName, tile.X, tile.Y, flip: false);
				break;
			}
			case "WarpMensLocker":
			{
				if (!ArgUtility.TryGetPoint(action, 1, out var tile4, out error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName4, out error, allowBlank: true, "string locationName"))
				{
					return LogError(error);
				}
				bool playDoorSound2 = action.Length < 5;
				if (!who.IsMale)
				{
					if (who.IsLocalPlayer)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:MensLocker_WrongGender"));
					}
					return true;
				}
				who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
				if (playDoorSound2)
				{
					this.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
				}
				Game1.warpFarmer(locationName4, tile4.X, tile4.Y, flip: false);
				break;
			}
			case "LockedDoorWarp":
			{
				if (!ArgUtility.TryGetPoint(action, 1, out var tile3, out error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName3, out error, allowBlank: true, "string locationName") || !ArgUtility.TryGetInt(action, 4, out var openTime3, out error, "int openTime") || !ArgUtility.TryGetInt(action, 5, out var closeTime3, out error, "int closeTime") || !ArgUtility.TryGetOptional(action, 6, out var npcName3, out error, null, allowBlank: true, "string npcName") || !ArgUtility.TryGetOptionalInt(action, 7, out var minFriendship, out error, 0, "int minFriendship"))
				{
					return LogError(error);
				}
				who.faceGeneralDirection(new Vector2(tileLocation.X, tileLocation.Y) * 64f);
				this.lockedDoorWarp(tile3, locationName3, openTime3, closeTime3, npcName3, minFriendship);
				break;
			}
			case "ConditionalDoor":
				if (action.Length > 1 && !Game1.eventUp)
				{
					if (GameStateQuery.CheckConditions(ArgUtility.UnsplitQuoteAware(action, ' ', 1)))
					{
						this.openDoor(tileLocation, playSound: true);
						return true;
					}
					string message2 = this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "LockedDoorMessage", "Buildings");
					if (message2 != null)
					{
						Game1.drawObjectDialogue(TokenParser.ParseText(Game1.content.LoadString(message2)));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
					}
				}
				break;
			case "Door":
				if (action.Length > 1 && !Game1.eventUp)
				{
					for (int i = 1; i < action.Length; i++)
					{
						string name = action[i];
						string mailKey = "doorUnlock" + name;
						if (who.getFriendshipHeartLevelForNPC(name) >= 2 || Game1.player.mailReceived.Contains(mailKey))
						{
							Rumble.rumble(0.1f, 100f);
							Game1.player.mailReceived.Add(mailKey);
							this.openDoor(tileLocation, playSound: true);
							return true;
						}
						if (name == "Sebastian" && this.IsGreenRainingHere() && Game1.year == 1)
						{
							Rumble.rumble(0.1f, 100f);
							this.openDoor(tileLocation, playSound: true);
							return true;
						}
					}
					this.ShowLockedDoorMessage(action);
					break;
				}
				this.openDoor(tileLocation, playSound: true);
				return true;
			case "Tutorial":
				Game1.activeClickableMenu = new TutorialMenu();
				break;
			case "Message":
			case "MessageSpeech":
			{
				if (!ArgUtility.TryGet(action, 1, out var translationKey3, out error, allowBlank: true, "string translationKey"))
				{
					return LogError(error);
				}
				string s = null;
				try
				{
					s = Game1.content.LoadStringReturnNullIfNotFound(translationKey3);
				}
				catch (Exception)
				{
					s = null;
				}
				if (s != null)
				{
					Game1.drawDialogueNoTyping(s);
				}
				else
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:" + translationKey3.Replace("\"", "")));
				}
				break;
			}
			case "Dialogue":
			{
				if (!ArgUtility.TryGetRemainder(action, 1, out var dialogue2, out error, ' ', "string dialogue"))
				{
					return LogError(error);
				}
				dialogue2 = TokenParser.ParseText(dialogue2);
				Game1.drawDialogueNoTyping(dialogue2);
				break;
			}
			case "NPCSpeechMessageNoRadius":
			{
				if (!ArgUtility.TryGet(action, 1, out var npcName2, out error, allowBlank: true, "string npcName") || !ArgUtility.TryGet(action, 2, out var translationKey2, out error, allowBlank: true, "string translationKey"))
				{
					return LogError(error);
				}
				NPC npc2 = Game1.getCharacterFromName(npcName2);
				if (npc2 == null)
				{
					try
					{
						npc2 = new NPC(null, Vector2.Zero, "", 0, npcName2, datable: false, Game1.temporaryContent.Load<Texture2D>("Portraits\\" + npcName2));
					}
					catch (Exception)
					{
						return LogError("couldn't find or create a matching NPC");
					}
				}
				try
				{
					npc2.setNewDialogue("Strings\\StringsFromMaps:" + translationKey2, add: true);
					Game1.drawDialogue(npc2);
					return true;
				}
				catch (Exception value)
				{
					return LogError($"unhandled exception drawing dialogue: {value}");
				}
			}
			case "NPCMessage":
			{
				if (!ArgUtility.TryGet(action, 1, out var npcName, out error, allowBlank: true, "string npcName") || !ArgUtility.TryGetRemainder(action, 2, out var rawMessage, out error, ' ', "string rawMessage"))
				{
					return LogError(error);
				}
				string message = rawMessage.Replace("\"", "");
				NPC npc = Game1.getCharacterFromName(npcName);
				if (npc != null && npc.currentLocation == who.currentLocation && Utility.tileWithinRadiusOfPlayer(npc.TilePoint.X, npc.TilePoint.Y, 14, who))
				{
					try
					{
						string str_name = message.Split('/')[0];
						string str_name_no_filePath = str_name.Substring(str_name.IndexOf(':') + 1);
						npc.setNewDialogue(str_name, add: true);
						Game1.drawDialogue(npc);
						switch (str_name_no_filePath)
						{
						case "AnimalShop.20":
						case "JoshHouse_Alex_Trash":
						case "SamHouse_Sam_Trash":
						case "SeedShop_Abigail_Drawers":
							if (who != null)
							{
								Game1.multiplayer.globalChatInfoMessage("Caught_Snooping", who.name.Value, npc.GetTokenizedDisplayName());
							}
							break;
						}
						return true;
					}
					catch (Exception)
					{
						return false;
					}
				}
				try
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString(message.Split('/')[1]));
					return false;
				}
				catch (Exception)
				{
					return false;
				}
			}
			case "ElliottPiano":
			{
				if (!ArgUtility.TryGetInt(action, 1, out var key, out error, "int key"))
				{
					return LogError(error);
				}
				this.playElliottPiano(key);
				break;
			}
			case "DropBox":
			{
				if (!ArgUtility.TryGet(action, 1, out var box_id, out error, allowBlank: true, "string box_id"))
				{
					return LogError(error);
				}
				int minimum_capacity = 0;
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.UsesDropBox(box_id))
					{
						minimum_capacity = Math.Max(minimum_capacity, order.GetMinimumDropBoxCapacity(box_id));
					}
				}
				foreach (SpecialOrder order2 in Game1.player.team.specialOrders)
				{
					if (!order2.UsesDropBox(box_id))
					{
						continue;
					}
					order2.donateMutex.RequestLock(delegate
					{
						while (order2.donatedItems.Count < minimum_capacity)
						{
							order2.donatedItems.Add(null);
						}
						Game1.activeClickableMenu = new QuestContainerMenu(order2.donatedItems, 3, order2.HighlightAcceptableItems, order2.GetAcceptCount, order2.UpdateDonationCounts, order2.ConfirmCompleteDonations);
					});
					return true;
				}
				return false;
			}
			case "playSound":
			{
				if (!ArgUtility.TryGet(action, 1, out var audioName, out error, allowBlank: true, "string audioName"))
				{
					return LogError(error);
				}
				this.localSound(audioName);
				break;
			}
			case "Letter":
			{
				if (!ArgUtility.TryGet(action, 1, out var translationKey, out error, allowBlank: true, "string translationKey"))
				{
					return LogError(error);
				}
				Game1.drawLetterMessage(Game1.content.LoadString("Strings\\StringsFromMaps:" + translationKey.Replace("\"", "")));
				break;
			}
			case "MessageOnce":
			{
				if (!ArgUtility.TryGet(action, 1, out var eventFlag, out error, allowBlank: true, "string eventFlag") || !ArgUtility.TryGetRemainder(action, 2, out var dialogue, out error, ' ', "string dialogue"))
				{
					return LogError(error);
				}
				if (who.eventsSeen.Add(eventFlag))
				{
					Game1.drawObjectDialogue(Game1.parseText(dialogue));
				}
				break;
			}
			case "Lamp":
				if (this.lightLevel.Value == 0f)
				{
					this.lightLevel.Value = 0.6f;
				}
				else
				{
					this.lightLevel.Value = 0f;
				}
				this.playSound("openBox");
				break;
			case "Billboard":
				Game1.activeClickableMenu = new Billboard(ArgUtility.Get(action, 1) == "3");
				break;
			case "MinecartTransport":
			{
				string networkId = ArgUtility.Get(action, 1) ?? "Default";
				string excludeDestinationId = ArgUtility.Get(action, 2);
				this.ShowMineCartMenu(networkId, excludeDestinationId);
				return true;
			}
			case "MineElevator":
				if (MineShaft.lowestLevelReached < 5)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_MineElevator_NotWorking")));
				}
				else
				{
					Game1.activeClickableMenu = new MineElevatorMenu();
				}
				break;
			case "Mine":
			case "NextMineLevel":
			{
				if (!ArgUtility.TryGetOptionalInt(action, 1, out var mineLevel, out error, 1, "int mineLevel"))
				{
					return LogError(error);
				}
				this.playSound("stairsdown");
				Game1.enterMine(mineLevel);
				break;
			}
			case "ExitMine":
			{
				Response[] responses = new Response[3]
				{
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")),
					new Response("Go", Game1.content.LoadString("Strings\\Locations:Mines_GoUp")),
					new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing"))
				};
				this.createQuestionDialogue(" ", responses, "ExitMine");
				break;
			}
			case "GoldenScythe":
				if (!Game1.player.mailReceived.Contains("gotGoldenScythe"))
				{
					if (!Game1.player.isInventoryFull())
					{
						Game1.playSound("parry");
						Game1.player.mailReceived.Add("gotGoldenScythe");
						this.setMapTile(29, 4, 245, "Front", "mine");
						this.setMapTile(30, 4, 246, "Front", "mine");
						this.setMapTile(29, 5, 261, "Front", "mine");
						this.setMapTile(30, 5, 262, "Front", "mine");
						this.setMapTile(29, 6, 277, "Buildings", "mine");
						this.setMapTile(30, 56, 278, "Buildings", "mine");
						Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(W)53"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					}
				}
				else
				{
					Game1.changeMusicTrack("silence");
					this.performTouchAction("MagicWarp Mine 67 10", Game1.player.getStandingPosition());
				}
				break;
			case "Saloon":
				if (who.TilePoint.Y > tileLocation.Y)
				{
					return this.saloon(tileLocation);
				}
				return false;
			case "Carpenter":
				if (who.TilePoint.Y > tileLocation.Y)
				{
					return this.carpenters(tileLocation);
				}
				return false;
			case "AnimalShop":
				if (who.TilePoint.Y > tileLocation.Y)
				{
					return this.animalShop(tileLocation);
				}
				return false;
			case "Blacksmith":
				if (who.TilePoint.Y > tileLocation.Y)
				{
					return this.blacksmith(tileLocation);
				}
				return false;
			case "Jukebox":
				Game1.activeClickableMenu = new ChooseFromListMenu(Utility.GetJukeboxTracks(Game1.player, Game1.player.currentLocation), ChooseFromListMenu.playSongAction, isJukebox: true);
				break;
			case "Buy":
			{
				if (!ArgUtility.TryGet(action, 1, out var which2, out error, allowBlank: true, "string which"))
				{
					return LogError(error);
				}
				if (who.TilePoint.Y >= tileLocation.Y)
				{
					return this.HandleBuyAction(which2);
				}
				return false;
			}
			case "Craft":
				GameLocation.openCraftingMenu();
				break;
			case "MineSign":
			{
				if (!ArgUtility.TryGetRemainder(action, 1, out var dialogue3, out error, ' ', "string dialogue"))
				{
					return LogError(error);
				}
				Game1.drawObjectDialogue(Game1.parseText(dialogue3));
				break;
			}
			case "ClubSlots":
				Game1.currentMinigame = new Slots();
				break;
			case "ClubShop":
				Utility.TryOpenShopMenu("Casino", null, playOpenSound: true);
				break;
			case "ClubCards":
			case "BlackJack":
				if (ArgUtility.Get(action, 1) == "1000")
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_HS"), new Response[2]
					{
						new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave"))
					}, "CalicoJackHS");
				}
				else
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack"), new Response[3]
					{
						new Response("Play", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Play")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Leave")),
						new Response("Rules", Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules"))
					}, "CalicoJack");
				}
				break;
			case "QiCoins":
				if (who.clubCoins > 0)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins", who.clubCoins));
				}
				else
				{
					this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_QiCoins_BuyStarter"), this.createYesNoResponses(), "BuyClubCoins");
				}
				break;
			case "FarmerFile":
			case "ClubComputer":
				this.farmerFile();
				break;
			case "ClubSeller":
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller"), new Response[2]
				{
					new Response("I'll", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_Yes")),
					new Response("No", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_No"))
				}, "ClubSeller");
				break;
			case "Mailbox":
				if (this is Farm && this.getBuildingAt(new Vector2(tileLocation.X, tileLocation.Y))?.GetIndoors() is FarmHouse { IsOwnedByCurrentPlayer: false })
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
				}
				else
				{
					this.mailbox();
				}
				break;
			case "Notes":
			{
				if (!ArgUtility.TryGetInt(action, 1, out var noteId, out error, "int noteId"))
				{
					return LogError(error);
				}
				this.readNote(noteId);
				break;
			}
			case "SpiritAltar":
				if (who.ActiveObject != null && Game1.player.team.sharedDailyLuck.Value != -0.12 && Game1.player.team.sharedDailyLuck.Value != 0.12)
				{
					if (who.ActiveObject.Price >= 60)
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite(352, 70f, 2, 2, new Vector2(tileLocation.X * 64, tileLocation.Y * 64), flicker: false, flipped: false));
						Game1.player.team.sharedDailyLuck.Value = 0.12;
						this.playSound("money");
					}
					else
					{
						this.temporarySprites.Add(new TemporaryAnimatedSprite(362, 50f, 6, 1, new Vector2(tileLocation.X * 64, tileLocation.Y * 64), flicker: false, flipped: false));
						Game1.player.team.sharedDailyLuck.Value = -0.12;
						this.playSound("thunder");
					}
					who.ActiveObject = null;
					who.showNotCarrying();
				}
				break;
			case "WizardHatch":
			{
				if (who.friendshipData.TryGetValue("Wizard", out var friendship) && friendship.Points >= 1000)
				{
					this.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
					Game1.warpFarmer("WizardHouseBasement", 4, 4, flip: true);
				}
				else
				{
					NPC wizard = this.characters[0];
					wizard.CurrentDialogue.Push(new Dialogue(wizard, "Data\\ExtraDialogue:Wizard_Hatch"));
					Game1.drawDialogue(wizard);
				}
				break;
			}
			case "EnterSewer":
				if (who.mailReceived.Contains("OpenedSewer"))
				{
					this.playSound("stairsdown", new Vector2(tileLocation.X, tileLocation.Y));
					Game1.warpFarmer("Sewer", 16, 11, 2);
				}
				else if (who.hasRustyKey)
				{
					this.playSound("openBox");
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Forest_OpenedSewer")));
					who.mailReceived.Add("OpenedSewer");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
				}
				break;
			case "DwarfGrave":
				if (who.canUnderstandDwarves)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_DwarfGrave_Translated").Replace('\n', '^'));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8214"));
				}
				break;
			case "Yoba":
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_Yoba"));
				break;
			case "ElliottBook":
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ElliottHouse_ElliottBook_Blank"));
				break;
			case "Theater_Poster":
				if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					MovieData currentMovie = MovieTheater.GetMovieToday();
					if (currentMovie != null)
					{
						Game1.multipleDialogues(new string[2]
						{
							Game1.content.LoadString("Strings\\Locations:Theater_Poster_0", TokenParser.ParseText(currentMovie.Title)),
							Game1.content.LoadString("Strings\\Locations:Theater_Poster_1", TokenParser.ParseText(currentMovie.Description))
						});
					}
				}
				break;
			case "Theater_PosterComingSoon":
				if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					MovieData upcomingMovie = MovieTheater.GetUpcomingMovie();
					if (upcomingMovie != null)
					{
						Game1.multipleDialogues(new string[1] { Game1.content.LoadString("Strings\\Locations:Theater_Poster_Coming_Soon", TokenParser.ParseText(upcomingMovie.Title)) });
					}
				}
				break;
			case "Theater_Entrance":
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					break;
				}
				if (Game1.player.team.movieMutex.IsLocked())
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_CurrentlyShowing")));
					break;
				}
				if (Game1.isFestival())
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
					break;
				}
				if (Game1.timeOfDay > 2100 || Game1.timeOfDay < 900)
				{
					string openTime = Game1.getTimeOfDayString(900).Replace(" ", "");
					string closeTime = Game1.getTimeOfDayString(2100).Replace(" ", "");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", openTime, closeTime));
					break;
				}
				if (Game1.player.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AlreadySeen"));
					break;
				}
				NPC invited_npc = null;
				foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
				{
					if (invitation.farmer == Game1.player && !invitation.fulfilled && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
					{
						invited_npc = invitation.invitedNPC;
						break;
					}
				}
				if (Game1.player.Items.ContainsId("(O)809"))
				{
					string question = ((invited_npc != null) ? Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchWithFriendPrompt", invited_npc.displayName) : Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchAlonePrompt"));
					Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(), "EnterTheaterSpendTicket");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_NoTicket")));
				}
				break;
			}
			case "Theater_BoxOffice":
				if (Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					if (Game1.isFestival())
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_ClosedFestival")));
					}
					else if (Game1.timeOfDay > 2100)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_BoxOfficeClosed"));
					}
					else if (MovieTheater.GetMovieToday() != null)
					{
						Utility.TryOpenShopMenu("BoxOffice", null, playOpenSound: true);
					}
				}
				break;
			case "BuildingChest":
			{
				if (!ArgUtility.TryGet(action, 1, out var buildingAction, out error, allowBlank: true, "string buildingAction"))
				{
					return LogError(error);
				}
				_ = this.getBuildingAt(new Vector2(tileLocation.X, tileLocation.Y))?.PerformBuildingChestAction(buildingAction, who) ?? false;
				return true;
			}
			case "BuildingToggleAnimalDoor":
			{
				Building building = this.getBuildingAt(new Vector2(tileLocation.X, tileLocation.Y));
				if (building != null)
				{
					if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
					{
						building.ToggleAnimalDoor(who);
					}
					return true;
				}
				break;
			}
			case "BuildingSilo":
			{
				if (!who.IsLocalPlayer)
				{
					break;
				}
				Object activeObj = who.ActiveObject;
				if (activeObj?.QualifiedItemId == "(O)178")
				{
					activeObj.FixStackSize();
					int stored = activeObj.Stack - this.tryToAddHay(activeObj.Stack);
					if (stored > 0)
					{
						if (activeObj.ConsumeStack(stored) == null)
						{
							who.ActiveObject = null;
						}
						Game1.playSound("Ship");
						DelayedAction.playSoundAfterDelay("grassyStep", 100);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:AddedHay", stored));
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PiecesOfHay", this.piecesOfHay, this.GetHayCapacity()));
				}
				break;
			}
			default:
				return false;
			}
			return true;
		}
		if (actionType == "Door")
		{
			this.openDoor(tileLocation, playSound: true);
		}
		return false;
		bool LogError(string errorPhrase)
		{
			this.LogTileActionError(action, tileLocation.X, tileLocation.Y, errorPhrase);
			return false;
		}
	}

	public void showPrairieKingMenu()
	{
		if (Game1.player.jotpkProgress.Value == null)
		{
			Game1.currentMinigame = new AbigailGame();
			return;
		}
		Response[] junimoKartOptions = new Response[3]
		{
			new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue")),
			new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame")),
			new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
		};
		this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu"), junimoKartOptions, "CowboyGame");
	}

	/// <summary>Show a minecart destination menu if the network is unlocked.</summary>
	/// <param name="networkId">The network whose destinations to show.</param>
	/// <param name="excludeDestinationId">The destination to hide from the list (usually the ID of the minecart we're using), or <c>null</c> to show all of them.</param>
	public void ShowMineCartMenu(string networkId, string excludeDestinationId)
	{
		if (Game1.player.mount != null)
		{
			return;
		}
		Dictionary<string, MinecartNetworkData> networks = DataLoader.Minecarts(Game1.content);
		if (networkId == null || !networks.TryGetValue(networkId, out var network))
		{
			Game1.log.Warn("Can't show minecart menu for unknown network ID '" + networkId + "'.");
			return;
		}
		if (!GameStateQuery.CheckConditions(network.UnlockCondition, this))
		{
			Game1.drawObjectDialogue(TokenParser.ParseText(network.LockedMessage) ?? Game1.content.LoadString("Strings\\Locations:MineCart_OutOfOrder"));
			return;
		}
		MinecartNetworkData minecartNetworkData = network;
		if (minecartNetworkData == null || !(minecartNetworkData.Destinations?.Count > 0))
		{
			Game1.log.Warn("Can't show minecart menu for network ID '" + networkId + "' with missing destination data.");
			return;
		}
		List<KeyValuePair<string, string>> destinations = new List<KeyValuePair<string, string>>();
		Dictionary<string, MinecartDestinationData> destinationLookup = new Dictionary<string, MinecartDestinationData>();
		foreach (MinecartDestinationData destination in network.Destinations)
		{
			if (string.IsNullOrWhiteSpace(destination?.Id) || string.IsNullOrWhiteSpace(destination?.TargetLocation))
			{
				Game1.log.Warn($"Ignored invalid minecart destination '{destination?.Id}' in network '{networkId}' because its ID or location isn't specified.");
			}
			else
			{
				if (destination.Id.EqualsIgnoreCase(excludeDestinationId) || !GameStateQuery.CheckConditions(destination.Condition, this))
				{
					continue;
				}
				if (destinationLookup.TryAdd(destination.Id, destination))
				{
					string label = TokenParser.ParseText(destination.DisplayName) ?? destination.TargetLocation;
					if (destination.Price > 0)
					{
						label = Game1.content.LoadString("Strings\\Locations:MineCart_DestinationWithPrice", label, destination.Price);
					}
					destinations.Add(new KeyValuePair<string, string>(destination.Id, label));
				}
				else
				{
					Game1.log.Warn($"Ignored minecart destination with duplicate ID '{destination.Id}' in network '{networkId}'.");
				}
			}
		}
		this.ShowPagedResponses(TokenParser.ParseText(network.ChooseDestinationMessage) ?? Game1.content.LoadString("Strings\\Locations:MineCart_ChooseDestination"), destinations, delegate(string destinationId)
		{
			if (destinationLookup.TryGetValue(destinationId, out var destination2))
			{
				int price = destination2.Price;
				if (price < 1)
				{
					this.MinecartWarp(destination2);
				}
				else
				{
					string numberWithCommas = Utility.getNumberWithCommas(price);
					string text = destination2.BuyTicketMessage ?? network.BuyTicketMessage;
					text = ((text != null) ? string.Format(TokenParser.ParseText(network.BuyTicketMessage), numberWithCommas) : Game1.content.LoadString("Strings\\Locations:BuyTicket", numberWithCommas));
					this.createQuestionDialogue(text, this.createYesNoResponses(), delegate(Farmer who, string whichAnswer)
					{
						if (whichAnswer == "Yes")
						{
							if (who.Money >= price)
							{
								who.Money -= price;
								this.MinecartWarp(destination2);
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
							}
						}
					});
				}
			}
		});
	}

	/// <summary>Warp to a minecart destination.</summary>
	/// <param name="destination">The minecart destination data.</param>
	public void MinecartWarp(MinecartDestinationData destination)
	{
		GameLocation targetLocation = Game1.RequireLocation(destination.TargetLocation);
		Point targetTile = destination.TargetTile;
		if (!Utility.TryParseDirection(destination.TargetDirection, out var direction))
		{
			direction = 2;
		}
		Game1.player.Halt();
		Game1.player.freezePause = 700;
		Game1.warpFarmer(targetLocation.NameOrUniqueName, targetTile.X, targetTile.Y, direction);
		if (Game1.IsPlayingTownMusic && !targetLocation.IsOutdoors)
		{
			Game1.changeMusicTrack("none");
		}
	}

	public void lockedDoorWarp(Point tile, string locationName, int openTime, int closeTime, string npcName, int minFriendship)
	{
		bool town_key_applies = Game1.player.HasTownKey;
		if (GameLocation.AreStoresClosedForFestival() && this.InValleyContext())
		{
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FestivalDay_DoorLocked")));
			return;
		}
		if (locationName == "SeedShop" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent("191393") && !town_key_applies)
		{
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SeedShop_LockedWed")));
			return;
		}
		if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
		{
			openTime = 800;
		}
		if (town_key_applies)
		{
			if (town_key_applies && !this.InValleyContext())
			{
				town_key_applies = false;
			}
			if (town_key_applies && this is BeachNightMarket && locationName != "FishShop")
			{
				town_key_applies = false;
			}
		}
		Friendship friendship;
		bool canOpenDoor = (town_key_applies || (Game1.timeOfDay >= openTime && Game1.timeOfDay < closeTime)) && (minFriendship <= 0 || this.IsWinterHere() || (Game1.player.friendshipData.TryGetValue(npcName, out friendship) && friendship.Points >= minFriendship));
		if (this.IsGreenRainingHere() && Game1.year == 1 && !(this is Beach) && !(this is Forest) && !locationName.Equals("AdventureGuild"))
		{
			canOpenDoor = true;
		}
		if (canOpenDoor)
		{
			Rumble.rumble(0.15f, 200f);
			Game1.player.completelyStopAnimatingOrDoingAction();
			this.playSound("doorClose", Game1.player.Tile);
			Game1.warpFarmer(locationName, tile.X, tile.Y, flip: false);
		}
		else if (minFriendship <= 0)
		{
			string openTimeString = Game1.getTimeOfDayString(openTime).Replace(" ", "");
			if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
			{
				openTimeString = Game1.getTimeOfDayString(800).Replace(" ", "");
			}
			string closeTimeString = Game1.getTimeOfDayString(closeTime).Replace(" ", "");
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", openTimeString, closeTimeString));
		}
		else if (Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
		}
		else
		{
			NPC character = Game1.getCharacterFromName(npcName);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_FriendsOnly", character.displayName));
		}
	}

	public void playElliottPiano(int key)
	{
		if (Game1.IsMultiplayer && Game1.player.UniqueMultiplayerID % 111 == 0L)
		{
			switch (key)
			{
			case 1:
			{
				int? pitch = 500;
				this.playSound("toyPiano", null, pitch);
				break;
			}
			case 2:
			{
				int? pitch = 1200;
				this.playSound("toyPiano", null, pitch);
				break;
			}
			case 3:
			{
				int? pitch = 1400;
				this.playSound("toyPiano", null, pitch);
				break;
			}
			case 4:
			{
				int? pitch = 2000;
				this.playSound("toyPiano", null, pitch);
				break;
			}
			}
			return;
		}
		switch (key)
		{
		case 1:
		{
			int? pitch = 1100;
			this.playSound("toyPiano", null, pitch);
			break;
		}
		case 2:
		{
			int? pitch = 1500;
			this.playSound("toyPiano", null, pitch);
			break;
		}
		case 3:
		{
			int? pitch = 1600;
			this.playSound("toyPiano", null, pitch);
			break;
		}
		case 4:
		{
			int? pitch = 1800;
			this.playSound("toyPiano", null, pitch);
			break;
		}
		}
		switch (Game1.elliottPiano)
		{
		case 0:
			if (key == 2)
			{
				Game1.elliottPiano++;
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		case 1:
			if (key == 4)
			{
				Game1.elliottPiano++;
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		case 2:
			if (key == 3)
			{
				Game1.elliottPiano++;
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		case 3:
			if (key == 2)
			{
				Game1.elliottPiano++;
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		case 4:
			if (key == 3)
			{
				Game1.elliottPiano++;
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		case 5:
			if (key == 4)
			{
				Game1.elliottPiano++;
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		case 6:
			if (key == 2)
			{
				Game1.elliottPiano++;
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		case 7:
			if (key == 1)
			{
				Game1.elliottPiano = 0;
				NPC elliott = this.getCharacterFromName("Elliott");
				if (!Game1.eventUp && elliott != null && !elliott.isMoving())
				{
					elliott.faceTowardFarmerForPeriod(1000, 100, faceAway: false, Game1.player);
					elliott.doEmote(20);
				}
			}
			else
			{
				Game1.elliottPiano = 0;
			}
			break;
		}
	}

	public void readNote(int which)
	{
		if (Game1.netWorldState.Value.LostBooksFound >= which)
		{
			string message = Game1.content.LoadString("Strings\\Notes:" + which).Replace('\n', '^');
			Game1.player.mailReceived.Add("lb_" + which);
			this.removeTemporarySpritesWithIDLocal(which);
			Game1.drawLetterMessage(message);
		}
		else
		{
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Notes:Missing")));
		}
	}

	public void mailbox()
	{
		if (Game1.mailbox.Count > 0)
		{
			string mailTitle = Game1.mailbox[0];
			if (!mailTitle.Contains("passedOut") && !mailTitle.Contains("Cooking"))
			{
				Game1.player.mailReceived.Add(mailTitle);
			}
			Game1.mailbox.RemoveAt(0);
			Dictionary<string, string> mails = DataLoader.Mail(Game1.content);
			string mail = mails.GetValueOrDefault(mailTitle, "");
			if (mailTitle.StartsWith("passedOut"))
			{
				if (mailTitle.StartsWith("passedOut "))
				{
					string[] split = ArgUtility.SplitBySpace(mailTitle);
					int moneyTaken = ((split.Length > 1) ? Convert.ToInt32(split[1]) : 0);
					mail = Dialogue.applyGenderSwitchBlocks(str: mails[Utility.CreateDaySaveRandom(moneyTaken).Next((Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey")) ? 2 : 3) switch
					{
						0 => (Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember")) ? "passedOut4" : ("passedOut1_" + ((moneyTaken > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")), 
						1 => "passedOut2", 
						_ => "passedOut3_" + ((moneyTaken > 0) ? "Billed" : "NotBilled"), 
					}], gender: Game1.player.Gender);
					mail = string.Format(mail, moneyTaken);
				}
				else
				{
					string[] split2 = ArgUtility.SplitBySpace(mailTitle);
					if (split2.Length > 1)
					{
						int moneyTaken2 = Convert.ToInt32(split2[1]);
						mail = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, mails[split2[0]]);
						mail = string.Format(mail, moneyTaken2);
					}
				}
			}
			if (mail.Length > 0)
			{
				Game1.activeClickableMenu = new LetterViewerMenu(mail, mailTitle);
			}
		}
		else if (Game1.mailbox.Count == 0)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8429"));
		}
	}

	public void farmerFile()
	{
		Game1.multipleDialogues(new string[2]
		{
			Game1.content.LoadString("Strings\\UI:FarmerFile_1", Game1.player.Name, Game1.stats.StepsTaken, Game1.stats.GiftsGiven, Game1.stats.DaysPlayed, Game1.stats.DirtHoed, Game1.stats.ItemsCrafted, Game1.stats.ItemsCooked, Game1.stats.PiecesOfTrashRecycled).Replace('\n', '^'),
			Game1.content.LoadString("Strings\\UI:FarmerFile_2", Game1.stats.MonstersKilled, Game1.stats.FishCaught, Game1.stats.TimesFished, Game1.stats.SeedsSown, Game1.stats.ItemsShipped).Replace('\n', '^')
		});
	}

	/// <summary>Get the number of crops currently planted in this location.</summary>
	public int getTotalCrops()
	{
		int amount = 0;
		foreach (TerrainFeature value in this.terrainFeatures.Values)
		{
			if (value is HoeDirt { crop: not null } dirt && !dirt.crop.dead.Value)
			{
				amount++;
			}
		}
		return amount;
	}

	/// <summary>Get the number of crops currently planted in this location which are ready to harvest.</summary>
	public int getTotalCropsReadyForHarvest()
	{
		int amount = 0;
		foreach (TerrainFeature value in this.terrainFeatures.Values)
		{
			if (value is HoeDirt dirt && dirt.readyForHarvest())
			{
				amount++;
			}
		}
		return amount;
	}

	/// <summary>Get the number of crops currently planted in this location which need to be watered.</summary>
	public int getTotalUnwateredCrops()
	{
		int amount = 0;
		foreach (TerrainFeature value in this.terrainFeatures.Values)
		{
			if (value is HoeDirt { crop: not null } dirt && dirt.needsWatering() && !dirt.isWatered())
			{
				amount++;
			}
		}
		return amount;
	}

	/// <summary>Get the number of crops currently planted in a greenhouse within this location.</summary>
	public int? getTotalGreenhouseCropsReadyForHarvest()
	{
		if (Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
		{
			int amount = 0;
			foreach (TerrainFeature value in Game1.RequireLocation("Greenhouse").terrainFeatures.Values)
			{
				if (value is HoeDirt dirt && dirt.readyForHarvest())
				{
					amount++;
				}
			}
			return amount;
		}
		return null;
	}

	/// <summary>Get the number of tiles currently tilled in this location which don't contain a crop.</summary>
	public int getTotalOpenHoeDirt()
	{
		int amount = 0;
		foreach (TerrainFeature t in this.terrainFeatures.Values)
		{
			if (t is HoeDirt { crop: null } && !this.objects.ContainsKey(t.Tile))
			{
				amount++;
			}
		}
		return amount;
	}

	/// <summary>Get the number of forage items currently in this location.</summary>
	public int getTotalForageItems()
	{
		int amount = 0;
		foreach (Object value in this.objects.Values)
		{
			if (value.isSpawnedObject.Value)
			{
				amount++;
			}
		}
		return amount;
	}

	/// <summary>Get the number of machines within this location with output ready to collect.</summary>
	public int getNumberOfMachinesReadyForHarvest()
	{
		int num = 0;
		foreach (Object value in this.objects.Values)
		{
			if (value.IsConsideredReadyMachineForComputer())
			{
				num++;
			}
		}
		string houseName = null;
		if (!(this is Farm))
		{
			if (this is IslandWest islandWest && islandWest.farmhouseRestored.Value)
			{
				houseName = "IslandFarmHouse";
			}
		}
		else
		{
			houseName = "FarmHouse";
		}
		if (houseName != null)
		{
			foreach (Object value2 in Game1.RequireLocation(houseName).objects.Values)
			{
				if (value2.IsConsideredReadyMachineForComputer())
				{
					num++;
				}
			}
		}
		foreach (Building building in this.buildings)
		{
			GameLocation indoors = building.GetIndoors();
			if (indoors == null)
			{
				continue;
			}
			foreach (Object value3 in indoors.objects.Values)
			{
				if (value3.IsConsideredReadyMachineForComputer())
				{
					num++;
				}
			}
		}
		return num;
	}

	public static void openCraftingMenu()
	{
		Game1.activeClickableMenu = new GameMenu(GameMenu.craftingTab);
	}

	/// <summary>Handle an <c>Action Buy</c> tile property in this location.</summary>
	/// <param name="which">The legacy shop ID. This is not necessarily the same ID used in <c>Data/ShopData</c>.</param>
	/// <remarks>This is used to apply hardcoded game logic (like showing a message when Pierre is visiting the island). Most code should use <c>Action OpenShop</c> or <see cref="M:StardewValley.Utility.TryOpenShopMenu(System.String,System.String,System.Boolean)" /> instead.</remarks>
	public virtual bool HandleBuyAction(string which)
	{
		if (which.Equals("Fish"))
		{
			int? maxOwnerY = Game1.player.TilePoint.Y - 1;
			return Utility.TryOpenShopMenu("FishShop", this, null, maxOwnerY);
		}
		if (this is SeedShop)
		{
			if (this.getCharacterFromName("Pierre") == null && Game1.IsVisitingIslandToday("Pierre"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_MoneyBox"));
				Game1.afterDialogues = delegate
				{
					Utility.TryOpenShopMenu("SeedShop", null, playOpenSound: true);
				};
			}
			else
			{
				Utility.TryOpenShopMenu("SeedShop", this, new Microsoft.Xna.Framework.Rectangle(4, 17, 1, 1), Game1.player.TilePoint.Y - 1);
			}
			return true;
		}
		if (this.name.Equals("SandyHouse"))
		{
			Utility.TryOpenShopMenu("Sandy", this);
			return true;
		}
		return false;
	}

	public virtual bool isObjectAt(int x, int y)
	{
		Vector2 v = new Vector2(x / 64, y / 64);
		foreach (Furniture item in this.furniture)
		{
			if (item.boundingBox.Value.Contains(x, y))
			{
				return true;
			}
		}
		return this.objects.ContainsKey(v);
	}

	public virtual bool isObjectAtTile(int tileX, int tileY)
	{
		Vector2 v = new Vector2(tileX, tileY);
		foreach (Furniture item in this.furniture)
		{
			if (item.boundingBox.Value.Contains(tileX * 64, tileY * 64))
			{
				return true;
			}
		}
		return this.objects.ContainsKey(v);
	}

	public virtual Object getObjectAt(int x, int y, bool ignorePassables = false)
	{
		Vector2 v = new Vector2(x / 64, y / 64);
		foreach (Furniture f in this.furniture)
		{
			if (f.boundingBox.Value.Contains(x, y) && (!ignorePassables || !f.isPassable()))
			{
				return f;
			}
		}
		Object obj = null;
		this.objects.TryGetValue(v, out obj);
		if (ignorePassables && obj != null && obj.isPassable())
		{
			obj = null;
		}
		return obj;
	}

	public Object getObjectAtTile(int x, int y, bool ignorePassables = false)
	{
		return this.getObjectAt(x * 64, y * 64, ignorePassables);
	}

	public virtual bool saloon(Location tileLocation)
	{
		NPC gus = this.getCharacterFromName("Gus");
		Microsoft.Xna.Framework.Rectangle shopOwnerArea = new Microsoft.Xna.Framework.Rectangle(9, 17, 10, 2);
		if (Utility.TryOpenShopMenu("Saloon", this, shopOwnerArea))
		{
			gus?.facePlayer(Game1.player);
			return true;
		}
		if (gus == null && Game1.IsVisitingIslandToday("Gus"))
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_MoneyBox"));
			Game1.afterDialogues = delegate
			{
				Utility.TryOpenShopMenu("Saloon", null, playOpenSound: true);
			};
			return true;
		}
		return false;
	}

	private void adventureShop()
	{
		if (Game1.player.itemsLostLastDeath.Count > 0)
		{
			List<Response> options = new List<Response>
			{
				new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")),
				new Response("Recovery", Game1.content.LoadString("Strings\\Locations:AdventureGuild_ItemRecovery")),
				new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave"))
			};
			this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:AdventureGuild_Greeting"), options.ToArray(), "adventureGuild");
		}
		else
		{
			Utility.TryOpenShopMenu("AdventureShop", "Marlon");
		}
	}

	public virtual bool carpenters(Location tileLocation)
	{
		foreach (NPC n in this.characters)
		{
			if (!n.Name.Equals("Robin"))
			{
				continue;
			}
			if (Vector2.Distance(n.Tile, new Vector2(tileLocation.X, tileLocation.Y)) > 3f)
			{
				return false;
			}
			n.faceDirection(2);
			if (Game1.player.daysUntilHouseUpgrade.Value < 0 && !Game1.IsThereABuildingUnderConstruction())
			{
				List<Response> options = new List<Response>();
				options.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
				if (Game1.IsMasterGame)
				{
					if (Game1.player.houseUpgradeLevel.Value < 3)
					{
						options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")));
					}
					else if ((Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter()) && Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value <= 0)
					{
						if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
						{
							options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
						}
						else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
						{
							options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
						}
					}
				}
				else if (Game1.player.houseUpgradeLevel.Value < 3)
				{
					options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeCabin")));
				}
				if (Game1.player.houseUpgradeLevel.Value >= 2)
				{
					if (Game1.IsMasterGame)
					{
						options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateHouse")));
					}
					else
					{
						options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateCabin")));
					}
				}
				options.Add(new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")));
				options.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")));
				this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), options.ToArray(), "carpenter");
			}
			else
			{
				Utility.TryOpenShopMenu("Carpenter", "Robin");
			}
			return true;
		}
		if (this.getCharacterFromName("Robin") == null && Game1.IsVisitingIslandToday("Robin"))
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_MoneyBox"));
			Game1.afterDialogues = delegate
			{
				Utility.TryOpenShopMenu("Carpenter", null, playOpenSound: true);
			};
			return true;
		}
		if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_RobinAbsent").Replace('\n', '^'));
			return true;
		}
		return false;
	}

	public virtual bool blacksmith(Location tileLocation)
	{
		foreach (NPC n in this.characters)
		{
			if (!n.Name.Equals("Clint"))
			{
				continue;
			}
			if (n.Tile != new Vector2(tileLocation.X, tileLocation.Y - 1))
			{
				_ = n.Tile != new Vector2(tileLocation.X - 1, tileLocation.Y - 1);
			}
			n.faceDirection(2);
			if (Game1.player.toolBeingUpgraded.Value != null && Game1.player.daysLeftForToolUpgrade.Value <= 0)
			{
				if (Game1.player.freeSpotsInInventory() > 0 || Game1.player.toolBeingUpgraded.Value is GenericTool)
				{
					Tool tool = Game1.player.toolBeingUpgraded.Value;
					Game1.player.toolBeingUpgraded.Value = null;
					Game1.player.hasReceivedToolUpgradeMessageYet = false;
					Game1.player.holdUpItemThenMessage(tool);
					if (tool is GenericTool)
					{
						tool.actionWhenClaimed();
					}
					else
					{
						Game1.player.addItemToInventoryBool(tool);
					}
					if (Game1.player.team.useSeparateWallets.Value && tool.UpgradeLevel == 4)
					{
						Game1.multiplayer.globalChatInfoMessage("IridiumToolUpgrade", Game1.player.Name, TokenStringBuilder.ToolName(tool.QualifiedItemId, tool.UpgradeLevel));
					}
				}
				else
				{
					Game1.DrawDialogue(n, "Data\\ExtraDialogue:Clint_NoInventorySpace");
				}
			}
			else
			{
				bool hasGeode = false;
				foreach (Item item in Game1.player.Items)
				{
					if (Utility.IsGeode(item))
					{
						hasGeode = true;
						break;
					}
				}
				Response[] responses = ((!hasGeode) ? new Response[3]
				{
					new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
					new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
				} : new Response[4]
				{
					new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
					new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
					new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
				});
				this.createQuestionDialogue("", responses, "Blacksmith");
			}
			return true;
		}
		return false;
	}

	public virtual bool animalShop(Location tileLocation)
	{
		foreach (NPC n in this.characters)
		{
			if (!n.Name.Equals("Marnie"))
			{
				continue;
			}
			if (n.Tile != new Vector2(tileLocation.X, tileLocation.Y - 1) && n.Tile != new Vector2(tileLocation.X - 1, tileLocation.Y - 1))
			{
				if (Game1.player.stats.Get("Book_AnimalCatalogue") != 0)
				{
					break;
				}
				return false;
			}
			n.faceDirection(2);
			List<Response> options = new List<Response>
			{
				new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
				new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
				new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
			};
			if ((Utility.getAllPets().Count == 0 && Game1.year >= 2) || Game1.player.mailReceived.Contains("MarniePetAdoption") || Game1.player.mailReceived.Contains("MarniePetRejectedAdoption"))
			{
				options.Insert(2, new Response("Adopt", Game1.content.LoadString("Strings\\1_6_Strings:AdoptPets")));
			}
			this.createQuestionDialogue("", options.ToArray(), "Marnie");
			return true;
		}
		if (this.getCharacterFromName("Marnie") == null && Game1.IsVisitingIslandToday("Marnie"))
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_MoneyBox"));
			Game1.afterDialogues = delegate
			{
				Utility.TryOpenShopMenu("AnimalShop", null, playOpenSound: true);
			};
			return true;
		}
		if (Game1.player.stats.Get("Book_AnimalCatalogue") != 0)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Marnie_Counter"));
			Game1.afterDialogues = delegate
			{
				List<Response> list = new List<Response>
				{
					new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
					new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
				};
				if ((Utility.getAllPets().Count == 0 && Game1.year >= 2) || Game1.player.mailReceived.Contains("MarniePetAdoption") || Game1.player.mailReceived.Contains("MarniePetRejectedAdoption"))
				{
					list.Insert(2, new Response("Adopt", Game1.content.LoadString("Strings\\1_6_Strings:AdoptPets")));
				}
				this.createQuestionDialogue("", list.ToArray(), "Marnie");
			};
			return true;
		}
		if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Absent").Replace('\n', '^'));
			return true;
		}
		return false;
	}

	public void removeTile(Location tileLocation, string layer)
	{
		this.Map.RequireLayer(layer).Tiles[tileLocation.X, tileLocation.Y] = null;
	}

	public void removeTile(int x, int y, string layer)
	{
		this.Map.RequireLayer(layer).Tiles[x, y] = null;
	}

	public void characterTrampleTile(Vector2 tile)
	{
		if (!(this is FarmHouse) && !(this is IslandFarmHouse) && !(this is Farm))
		{
			this.terrainFeatures.TryGetValue(tile, out var tf);
			if (tf is Tree tree && tree.growthStage.Value < 1 && tree.instantDestroy(tile))
			{
				this.terrainFeatures.Remove(tile);
			}
		}
	}

	public bool characterDestroyObjectWithinRectangle(Microsoft.Xna.Framework.Rectangle rect, bool showDestroyedObject)
	{
		if (this is FarmHouse || this is IslandFarmHouse)
		{
			return false;
		}
		foreach (Farmer farmer in this.farmers)
		{
			if (rect.Intersects(farmer.GetBoundingBox()))
			{
				return false;
			}
		}
		Vector2 tilePositionToTry = new Vector2(rect.X / 64, rect.Y / 64);
		this.objects.TryGetValue(tilePositionToTry, out var o);
		if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
		{
			return true;
		}
		this.terrainFeatures.TryGetValue(tilePositionToTry, out var tf);
		if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
		{
			return true;
		}
		tilePositionToTry.X = rect.Right / 64;
		this.objects.TryGetValue(tilePositionToTry, out o);
		if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
		{
			return true;
		}
		this.terrainFeatures.TryGetValue(tilePositionToTry, out tf);
		if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
		{
			return true;
		}
		tilePositionToTry.X = rect.X / 64;
		tilePositionToTry.Y = rect.Bottom / 64;
		this.objects.TryGetValue(tilePositionToTry, out o);
		if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
		{
			return true;
		}
		this.terrainFeatures.TryGetValue(tilePositionToTry, out tf);
		if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
		{
			return true;
		}
		tilePositionToTry.X = rect.Right / 64;
		this.objects.TryGetValue(tilePositionToTry, out o);
		if (this.checkDestroyItem(o, tilePositionToTry, showDestroyedObject))
		{
			return true;
		}
		this.terrainFeatures.TryGetValue(tilePositionToTry, out tf);
		if (this.checkDestroyTerrainFeature(tf, tilePositionToTry))
		{
			return true;
		}
		for (int i = this.largeTerrainFeatures.Count - 1; i >= 0; i--)
		{
			LargeTerrainFeature feature = this.largeTerrainFeatures[i];
			if (feature.isDestroyedByNPCTrample && feature.getBoundingBox().Intersects(rect))
			{
				feature.onDestroy();
				this.largeTerrainFeatures.RemoveAt(i);
				return true;
			}
		}
		for (int i2 = this.resourceClumps.Count - 1; i2 >= 0; i2--)
		{
			ResourceClump clump = this.resourceClumps[i2];
			if (clump.IsGreenRainBush() && clump.getBoundingBox().Intersects(rect) && clump.destroy(null, this, clump.Tile))
			{
				this.resourceClumps.RemoveAt(i2);
			}
		}
		return false;
	}

	private bool checkDestroyTerrainFeature(TerrainFeature tf, Vector2 tilePositionToTry)
	{
		if (tf is Tree tree && tree.instantDestroy(tilePositionToTry))
		{
			this.terrainFeatures.Remove(tilePositionToTry);
		}
		return false;
	}

	private bool checkDestroyItem(Object o, Vector2 tilePositionToTry, bool showDestroyedObject)
	{
		if (o != null && !o.isPassable() && !this.map.RequireLayer("Back").Tiles[(int)tilePositionToTry.X, (int)tilePositionToTry.Y].Properties.ContainsKey("NPCBarrier"))
		{
			if (o.IsSpawnedObject)
			{
				this.numberOfSpawnedObjectsOnMap--;
			}
			if (showDestroyedObject && !o.bigCraftable.Value)
			{
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, new Vector2(tilePositionToTry.X * 64f, tilePositionToTry.Y * 64f), flicker: false, o.flipped.Value)
				{
					alphaFade = 0.01f
				};
				sprite.CopyAppearanceFromItemId(o.QualifiedItemId);
				Game1.multiplayer.broadcastSprites(this, sprite);
			}
			o.performToolAction(null);
			if (this.objects.ContainsKey(tilePositionToTry))
			{
				if (o is Chest chest)
				{
					if (chest.TryMoveToSafePosition())
					{
						return true;
					}
					chest.destroyAndDropContents(tilePositionToTry * 64f);
				}
				this.objects.Remove(tilePositionToTry);
			}
			return true;
		}
		return false;
	}

	public Object removeObject(Vector2 location, bool showDestroyedObject)
	{
		this.objects.TryGetValue(location, out var o);
		if (o != null && (o.CanBeGrabbed || showDestroyedObject))
		{
			if (o.IsSpawnedObject)
			{
				this.numberOfSpawnedObjectsOnMap--;
			}
			Object tmp = this.objects[location];
			this.objects.Remove(location);
			if (showDestroyedObject)
			{
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, new Vector2(location.X * 64f, location.Y * 64f), flicker: true, tmp.bigCraftable.Value, tmp.flipped.Value);
				sprite.CopyAppearanceFromItemId(tmp.QualifiedItemId, (!(tmp.Type == "Crafting")) ? 1 : 0);
				Game1.multiplayer.broadcastSprites(this, sprite);
			}
			if (o.IsWeeds())
			{
				Game1.stats.WeedsEliminated++;
			}
			return tmp;
		}
		return null;
	}

	public void removeTileProperty(int tileX, int tileY, string layer, string key)
	{
		try
		{
			(this.map?.GetLayer(layer)?.Tiles[tileX, tileY])?.Properties.Remove(key);
		}
		catch (Exception)
		{
		}
	}

	public void setTileProperty(int tileX, int tileY, string layer, string key, string value)
	{
		try
		{
			Tile tile = this.map?.GetLayer(layer)?.Tiles[tileX, tileY];
			if (tile != null)
			{
				tile.Properties[key] = value;
			}
		}
		catch (Exception)
		{
		}
	}

	public void setObjectAt(float x, float y, Object o)
	{
		Vector2 v = new Vector2(x, y);
		this.objects[v] = o;
	}

	public virtual void cleanupBeforeSave()
	{
		this.characters.RemoveWhere((NPC npc) => npc is Junimo);
		if (this.name.Equals("WitchHut"))
		{
			this.characters.Clear();
		}
		this.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature is Tent);
		foreach (Building building in this.buildings)
		{
			building.indoors.Value?.cleanupBeforeSave();
		}
	}

	public virtual void cleanupForVacancy()
	{
		if (Game1.IsMasterGame)
		{
			this.debris.RemoveWhere((Debris d) => d.isEssentialItem() && d.collect(Game1.player));
		}
	}

	public virtual void cleanupBeforePlayerExit()
	{
		this.debris.RemoveWhere((Debris d) => d.isEssentialItem() && d.player.Value != null && d.player.Value == Game1.player && d.collect(d.player.Value));
		Game1.currentLightSources.Clear();
		this.critters?.Clear();
		Game1.onScreenMenus.RemoveWhere(delegate(IClickableMenu menu)
		{
			if (menu.destroy)
			{
				(menu as IDisposable)?.Dispose();
				return true;
			}
			return false;
		});
		AmbientLocationSounds.onLocationLeave();
		Game1.player.rightRing.Value?.onLeaveLocation(Game1.player, this);
		Game1.player.leftRing.Value?.onLeaveLocation(Game1.player, this);
		if (this.name.Equals("AbandonedJojaMart") && this.farmers.Count <= 1)
		{
			this.characters.RemoveWhere((NPC npc) => npc is Junimo);
		}
		this.furnitureToRemove.Clear();
		this.interiorDoors.CleanUpLocalState();
		Game1.temporaryContent.Unload();
		Utility.CollectGarbage();
	}

	public static string getWeedForSeason(Random r, Season season)
	{
		return season switch
		{
			Season.Spring => r.Choose("(O)784", "(O)674", "(O)675"), 
			Season.Summer => r.Choose("(O)785", "(O)676", "(O)677"), 
			Season.Fall => r.Choose("(O)786", "(O)678", "(O)679"), 
			_ => "(O)674", 
		};
	}

	private void startSleep()
	{
		Game1.player.timeWentToBed.Value = Game1.timeOfDay;
		if (Game1.IsMultiplayer)
		{
			Game1.netReady.SetLocalReady("sleep", ready: true);
			Game1.dialogueUp = false;
			Game1.activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: true, delegate
			{
				this.doSleep();
			}, delegate(Farmer who)
			{
				if (Game1.activeClickableMenu is ReadyCheckDialog readyCheckDialog)
				{
					readyCheckDialog.closeDialog(who);
				}
				who.timeWentToBed.Value = 0;
			});
		}
		else
		{
			this.doSleep();
		}
		if (Game1.IsDedicatedHost || Game1.player.team.announcedSleepingFarmers.Contains(Game1.player))
		{
			return;
		}
		Game1.player.team.announcedSleepingFarmers.Add(Game1.player);
		if (!Game1.IsMultiplayer || (Game1.player.team.sleepAnnounceMode.Value != FarmerTeam.SleepAnnounceModes.All && (Game1.player.team.sleepAnnounceMode.Value != FarmerTeam.SleepAnnounceModes.First || Game1.player.team.announcedSleepingFarmers.Count != 1)))
		{
			return;
		}
		string key = "GoneToBed";
		if (Game1.random.NextDouble() < 0.75)
		{
			if (Game1.timeOfDay < 1800)
			{
				key += "Early";
			}
			else if (Game1.timeOfDay > 2530)
			{
				key += "Late";
			}
		}
		int key_index = 0;
		for (int i = 0; i < 2; i++)
		{
			if (Game1.random.NextDouble() < 0.25)
			{
				key_index++;
			}
		}
		Game1.multiplayer.globalChatInfoMessage(key + key_index, Game1.player.displayName);
	}

	protected virtual void _CleanupPagedResponses()
	{
		GameLocation._PagedResponses.Clear();
		GameLocation._OnPagedResponse = null;
		GameLocation._PagedResponsePrompt = null;
	}

	public virtual void ShowPagedResponses(string prompt, List<KeyValuePair<string, string>> responses, Action<string> on_response, bool auto_select_single_choice = false, bool addCancel = true, int itemsPerPage = 5)
	{
		GameLocation._PagedResponses.Clear();
		GameLocation._PagedResponses.AddRange(responses);
		GameLocation._PagedResponsePage = 0;
		GameLocation._PagedResponseAddCancel = addCancel;
		GameLocation._PagedResponseItemsPerPage = itemsPerPage;
		GameLocation._PagedResponsePrompt = prompt;
		GameLocation._OnPagedResponse = on_response;
		if (GameLocation._PagedResponses.Count == 1 && auto_select_single_choice)
		{
			on_response(GameLocation._PagedResponses[0].Key);
		}
		else if (GameLocation._PagedResponses.Count > 0)
		{
			this._ShowPagedResponses(GameLocation._PagedResponsePage);
		}
	}

	protected virtual void _ShowPagedResponses(int page = -1)
	{
		GameLocation._PagedResponsePage = page;
		int itemsPerPage = GameLocation._PagedResponseItemsPerPage;
		int pages = (GameLocation._PagedResponses.Count - 1) / itemsPerPage;
		int itemsOnCurPage = itemsPerPage;
		if (GameLocation._PagedResponsePage == pages - 1 && GameLocation._PagedResponses.Count % itemsPerPage == 1)
		{
			itemsOnCurPage++;
			pages--;
		}
		List<Response> locationResponses = new List<Response>();
		for (int i = 0; i < itemsOnCurPage; i++)
		{
			int index = i + GameLocation._PagedResponsePage * itemsPerPage;
			if (index < GameLocation._PagedResponses.Count)
			{
				KeyValuePair<string, string> response = GameLocation._PagedResponses[index];
				locationResponses.Add(new Response(response.Key, response.Value));
			}
		}
		if (GameLocation._PagedResponsePage < pages)
		{
			locationResponses.Add(new Response("nextPage", Game1.content.LoadString("Strings\\UI:NextPage")));
		}
		if (GameLocation._PagedResponsePage > 0)
		{
			locationResponses.Add(new Response("previousPage", Game1.content.LoadString("Strings\\UI:PreviousPage")));
		}
		if (GameLocation._PagedResponseAddCancel)
		{
			locationResponses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
		}
		this.createQuestionDialogue(GameLocation._PagedResponsePrompt, locationResponses.ToArray(), "pagedResponse");
	}

	/// <summary>Show a dialogue menu to choose where to construct buildings.</summary>
	/// <param name="builder">The name of the NPC whose building menu is being shown (the vanilla values are <see cref="F:StardewValley.Game1.builder_robin" /> and <see cref="F:StardewValley.Game1.builder_wizard" />).</param>
	/// <param name="page">The page of location names to show, if there are multiple pages.</param>
	public virtual void ShowConstructOptions(string builder, int page = -1)
	{
		if (builder != null)
		{
			this._constructLocationBuilderName = builder;
		}
		List<KeyValuePair<string, string>> buildableLocations = new List<KeyValuePair<string, string>>();
		foreach (GameLocation location in Game1.locations)
		{
			if (location.IsBuildableLocation())
			{
				buildableLocations.Add(new KeyValuePair<string, string>(location.NameOrUniqueName, location.DisplayName));
			}
		}
		if (!buildableLocations.Any())
		{
			Farm farm = Game1.getFarm();
			buildableLocations.Add(new KeyValuePair<string, string>(farm.NameOrUniqueName, farm.DisplayName));
		}
		this.ShowPagedResponses(Game1.content.LoadString("Strings\\Buildings:Construction_ChooseLocation"), buildableLocations, delegate(string value)
		{
			GameLocation locationFromName = Game1.getLocationFromName(value);
			if (locationFromName != null)
			{
				Game1.activeClickableMenu = new CarpenterMenu(this._constructLocationBuilderName, locationFromName);
			}
			else
			{
				Game1.log.Error("Can't find location '" + value + "' for construct menu.");
			}
		}, auto_select_single_choice: true);
	}

	/// <summary>Show a shop menu to select a location (if multiple have animal buildings) and purchase animals.</summary>
	/// <param name="onMenuOpened">An callback to invoke when the purchase menu is opened.</param>
	public void ShowAnimalShopMenu(Action<PurchaseAnimalsMenu> onMenuOpened = null)
	{
		List<KeyValuePair<string, string>> validLocations = new List<KeyValuePair<string, string>>();
		foreach (GameLocation location in Game1.locations)
		{
			if (location.buildings.Any((Building p) => p.GetIndoors() is AnimalHouse) && (!Game1.IsClient || location.CanBeRemotedlyViewed()))
			{
				validLocations.Add(new KeyValuePair<string, string>(location.NameOrUniqueName, location.DisplayName));
			}
		}
		if (!validLocations.Any())
		{
			Farm farm = Game1.getFarm();
			validLocations.Add(new KeyValuePair<string, string>(farm.NameOrUniqueName, farm.DisplayName));
		}
		Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.ChooseLocation"), validLocations, delegate(string value)
		{
			GameLocation locationFromName = Game1.getLocationFromName(value);
			if (locationFromName != null)
			{
				PurchaseAnimalsMenu purchaseAnimalsMenu = new PurchaseAnimalsMenu(Utility.getPurchaseAnimalStock(locationFromName), locationFromName);
				onMenuOpened?.Invoke(purchaseAnimalsMenu);
				Game1.activeClickableMenu = purchaseAnimalsMenu;
			}
			else
			{
				Game1.log.Error("Can't find location '" + value + "' for animal purchase menu.");
			}
		}, auto_select_single_choice: true);
	}

	private void doSleep()
	{
		if (this.lightLevel.Value == 0f && Game1.timeOfDay < 2000)
		{
			if (!this.isOutdoors.Value)
			{
				this.lightLevel.Value = 0.6f;
				this.localSound("openBox");
			}
			if (Game1.IsMasterGame)
			{
				Game1.NewDay(600f);
			}
		}
		else if (this.lightLevel.Value > 0f && Game1.timeOfDay >= 2000)
		{
			if (!this.isOutdoors.Value)
			{
				this.lightLevel.Value = 0f;
				this.localSound("openBox");
			}
			if (Game1.IsMasterGame)
			{
				Game1.NewDay(600f);
			}
		}
		else if (Game1.IsMasterGame)
		{
			Game1.NewDay(0f);
		}
		Game1.player.lastSleepLocation.Value = Game1.currentLocation.NameOrUniqueName;
		Game1.player.lastSleepPoint.Value = Game1.player.TilePoint;
		Game1.player.mostRecentBed = Game1.player.Position;
		Game1.player.doEmote(24);
		Game1.player.freezePause = 2000;
	}

	public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		switch (questionAndAnswer)
		{
		case null:
			return false;
		case "GoldClock_Yes":
			Game1.netWorldState.Value.goldenClocksTurnedOff.Value = !Game1.netWorldState.Value.goldenClocksTurnedOff.Value;
			Game1.playSound("yoba");
			break;
		case "Bookseller_Buy":
			Utility.TryOpenShopMenu("Bookseller", null, playOpenSound: true);
			break;
		case "Bookseller_Trade":
			Utility.TryOpenShopMenu("BooksellerTrade", null, playOpenSound: true);
			break;
		case "SquidFestBooth_Rewards":
		{
			if (Game1.player.mailReceived.Contains("GotSquidFestReward_" + Game1.year + "_" + Game1.dayOfMonth + "_3") || Game1.player.mailReceived.Contains("GotSquidFestReward_" + Game1.year + "_" + Game1.dayOfMonth + "_3"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SquidFest_GotAllRewardsToday"));
				break;
			}
			List<string> availableRewards = new List<string>();
			int[] squidTargets = ((Game1.dayOfMonth != 12) ? new int[4] { 2, 5, 7, 10 } : new int[4] { 1, 3, 5, 8 });
			int currentSquid = (int)Game1.stats.Get(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year));
			bool alreadyReceivedAllRewards = false;
			bool alreadyGotCrabbingBook = Game1.player.mailReceived.Contains("GotCrabbingBook");
			for (int i3 = 0; i3 < squidTargets.Length; i3++)
			{
				if (currentSquid < squidTargets[i3])
				{
					continue;
				}
				if (!Game1.player.mailReceived.Contains("GotSquidFestReward_" + Game1.year + "_" + Game1.dayOfMonth + "_" + i3))
				{
					availableRewards.Add(Game1.dayOfMonth + "_" + i3);
					Game1.player.mailReceived.Add("GotSquidFestReward_" + Game1.year + "_" + Game1.dayOfMonth + "_" + i3);
					alreadyReceivedAllRewards = false;
					if (!alreadyGotCrabbingBook && i3 >= 3)
					{
						Game1.player.mailReceived.Add("GotCrabbingBook");
					}
				}
				else
				{
					alreadyReceivedAllRewards = true;
				}
			}
			if (availableRewards.Count > 0)
			{
				List<Item> rewards = new List<Item>();
				Random r = Utility.CreateDaySaveRandom(Game1.year * 2000, Game1.dayOfMonth * 10);
				using (List<string>.Enumerator enumerator = availableRewards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						switch (enumerator.Current)
						{
						case "12_0":
							rewards.Add(ItemRegistry.Create("(O)DeluxeBait", 20));
							break;
						case "12_1":
							rewards.Add((r.NextDouble() < 0.5) ? ItemRegistry.Create("(O)498", 10) : ItemRegistry.Create("(O)MysteryBox", 2));
							rewards.Add(ItemRegistry.Create("(O)242"));
							break;
						case "12_2":
							rewards.Add(ItemRegistry.Create("(O)797"));
							rewards.Add(ItemRegistry.Create("(O)395", 3));
							break;
						case "12_3":
							rewards.Add(new Furniture("SquidKid_Painting", Vector2.Zero));
							if (!alreadyGotCrabbingBook)
							{
								rewards.Add(ItemRegistry.Create("(O)Book_Crabbing"));
								break;
							}
							rewards.Add(ItemRegistry.Create("(O)MysteryBox", 3));
							rewards.Add(ItemRegistry.Create("(O)265"));
							break;
						case "13_0":
							rewards.Add(ItemRegistry.Create("(O)694"));
							break;
						case "13_1":
							rewards.Add((r.NextDouble() < 0.5) ? ItemRegistry.Create("(O)498", 15) : ItemRegistry.Create("(O)MysteryBox", 3));
							rewards.Add(ItemRegistry.Create("(O)242"));
							break;
						case "13_2":
							rewards.Add(ItemRegistry.Create("(O)166"));
							rewards.Add(ItemRegistry.Create("(O)253", 3));
							break;
						case "13_3":
							rewards.Add(new Hat("SquidHat"));
							if (!alreadyGotCrabbingBook)
							{
								rewards.Add(ItemRegistry.Create("(O)Book_Crabbing"));
								break;
							}
							rewards.Add(ItemRegistry.Create("(O)MysteryBox", 3));
							rewards.Add(ItemRegistry.Create("(O)265"));
							break;
						}
					}
				}
				if (rewards.Count > 0)
				{
					ItemGrabMenu itemGrabMenu = new ItemGrabMenu(rewards).setEssential(essential: true, superEssential: true);
					itemGrabMenu.inventory.showGrayedOutSlots = true;
					itemGrabMenu.source = 2;
					Game1.activeClickableMenu = itemGrabMenu;
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString(alreadyReceivedAllRewards ? "Strings\\1_6_Strings:SquidFest_AlreadyGotAvailableRewards" : "Strings\\1_6_Strings:SquidFestBooth_NoRewards"));
			}
			break;
		}
		case "SquidFestBooth_Explanation":
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:SquidFestBooth_Explanation"));
			break;
		case "TroutDerbyBooth_Rewards":
			if (Game1.player.Items.CountId("TroutDerbyTag") > 0)
			{
				Item reward = null;
				int rewardIndex = (int)(Utility.CreateRandom(Game1.uniqueIDForThisGame).Next(10) + Game1.stats.Get("GoldenTagsTurnedIn")) % 10;
				if (Game1.stats.Get("GoldenTagsTurnedIn") == 0)
				{
					reward = ItemRegistry.Create("(O)TentKit");
				}
				else
				{
					switch (rewardIndex)
					{
					case 0:
						reward = ItemRegistry.Create("(H)BucketHat");
						break;
					case 1:
						reward = ItemRegistry.Create("(O)710");
						break;
					case 2:
						reward = ItemRegistry.Create("(O)MysteryBox", 3);
						break;
					case 3:
						reward = ItemRegistry.Create("(O)72");
						break;
					case 4:
						reward = ItemRegistry.Create("(F)MountedTrout_Painting");
						break;
					case 5:
						reward = ItemRegistry.Create("(O)DeluxeBait", 20);
						break;
					case 6:
						reward = ItemRegistry.Create("(O)253", 2);
						break;
					case 7:
						reward = ItemRegistry.Create("(O)621");
						break;
					case 8:
						reward = ItemRegistry.Create("(O)688", 3);
						break;
					case 9:
						reward = ItemRegistry.Create("(O)749", 3);
						break;
					}
				}
				if (reward != null && (Game1.player.couldInventoryAcceptThisItem(reward) || Game1.player.Items.CountId("TroutDerbyTag") == 1))
				{
					Game1.stats.Increment("GoldenTagsTurnedIn");
					Game1.player.Items.ReduceId("TroutDerbyTag", 1);
					Game1.player.holdUpItemThenMessage(reward);
					Game1.player.addItemToInventoryBool(reward);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_BagFull"));
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_NoTags"));
			}
			break;
		case "TroutDerbyBooth_Explanation":
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FishingDerbyBooth_Explanation"));
			break;
		case "pagedResponse_cancel":
			this._CleanupPagedResponses();
			break;
		case "pagedResponse_nextPage":
			this._ShowPagedResponses(GameLocation._PagedResponsePage + 1);
			break;
		case "pagedResponse_previousPage":
			this._ShowPagedResponses(GameLocation._PagedResponsePage - 1);
			break;
		case "Fizz_Yes":
			if (Game1.player.Money >= 500000)
			{
				Game1.player.Money -= 500000;
				Game1.netWorldState.Value.PerfectionWaivers++;
				DelayedAction.playSoundAfterDelay("qi_shop_purchase", 500);
				this.getCharacterFromName("Fizz")?.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Fizz_Sweet"));
				this.getCharacterFromName("Fizz")?.shake(500);
				if (Game1.IsMultiplayer)
				{
					Game1.Multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:Waiver_Note_Multiplayer", false, null, Game1.player.Name);
				}
				else
				{
					Game1.showGlobalMessage(string.Format(Game1.content.LoadString("Strings\\1_6_Strings:Waiver_Note", Game1.netWorldState.Value.PerfectionWaivers.ToString() ?? "")));
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
			break;
		case "EnterTheaterSpendTicket_Yes":
			Game1.player.Items.ReduceId("(O)809", 1);
			Rumble.rumble(0.15f, 200f);
			Game1.player.completelyStopAnimatingOrDoingAction();
			this.playSound("doorClose", Game1.player.Tile);
			Game1.warpFarmer("MovieTheater", 13, 15, 0);
			break;
		case "EnterTheater_Yes":
			Rumble.rumble(0.15f, 200f);
			Game1.player.completelyStopAnimatingOrDoingAction();
			this.playSound("doorClose", Game1.player.Tile);
			Game1.warpFarmer("MovieTheater", 13, 15, 0);
			break;
		case "dogStatue_Yes":
		{
			if (Game1.player.Money < 10000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
				break;
			}
			List<Response> skill_responses = new List<Response>();
			if (GameLocation.canRespec(0))
			{
				skill_responses.Add(new Response("farming", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604")));
			}
			if (GameLocation.canRespec(3))
			{
				skill_responses.Add(new Response("mining", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605")));
			}
			if (GameLocation.canRespec(2))
			{
				skill_responses.Add(new Response("foraging", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606")));
			}
			if (GameLocation.canRespec(1))
			{
				skill_responses.Add(new Response("fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607")));
			}
			if (GameLocation.canRespec(4))
			{
				skill_responses.Add(new Response("combat", Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608")));
			}
			skill_responses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueCancel")));
			this.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueQuestion"), skill_responses.ToArray(), "professionForget");
			break;
		}
		case "professionForget_farming":
		{
			if (Game1.player.newLevels.Contains(new Point(0, 5)) || Game1.player.newLevels.Contains(new Point(0, 10)))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
				break;
			}
			Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
			GameLocation.RemoveProfession(0);
			GameLocation.RemoveProfession(1);
			GameLocation.RemoveProfession(3);
			GameLocation.RemoveProfession(5);
			GameLocation.RemoveProfession(2);
			GameLocation.RemoveProfession(4);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
			int num5 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[0]);
			if (num5 >= 5)
			{
				Game1.player.newLevels.Add(new Point(0, 5));
			}
			if (num5 >= 10)
			{
				Game1.player.newLevels.Add(new Point(0, 10));
			}
			DelayedAction.playSoundAfterDelay("dog_bark", 300);
			DelayedAction.playSoundAfterDelay("dog_bark", 900);
			break;
		}
		case "professionForget_mining":
		{
			if (Game1.player.newLevels.Contains(new Point(3, 5)) || Game1.player.newLevels.Contains(new Point(3, 10)))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
				break;
			}
			Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
			GameLocation.RemoveProfession(23);
			GameLocation.RemoveProfession(21);
			GameLocation.RemoveProfession(18);
			GameLocation.RemoveProfession(19);
			GameLocation.RemoveProfession(22);
			GameLocation.RemoveProfession(20);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
			int num2 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[3]);
			if (num2 >= 5)
			{
				Game1.player.newLevels.Add(new Point(3, 5));
			}
			if (num2 >= 10)
			{
				Game1.player.newLevels.Add(new Point(3, 10));
			}
			DelayedAction.playSoundAfterDelay("dog_bark", 300);
			DelayedAction.playSoundAfterDelay("dog_bark", 900);
			break;
		}
		case "professionForget_foraging":
		{
			if (Game1.player.newLevels.Contains(new Point(2, 5)) || Game1.player.newLevels.Contains(new Point(2, 10)))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
				break;
			}
			Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
			GameLocation.RemoveProfession(16);
			GameLocation.RemoveProfession(14);
			GameLocation.RemoveProfession(17);
			GameLocation.RemoveProfession(12);
			GameLocation.RemoveProfession(13);
			GameLocation.RemoveProfession(15);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
			int num6 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[2]);
			if (num6 >= 5)
			{
				Game1.player.newLevels.Add(new Point(2, 5));
			}
			if (num6 >= 10)
			{
				Game1.player.newLevels.Add(new Point(2, 10));
			}
			DelayedAction.playSoundAfterDelay("dog_bark", 300);
			DelayedAction.playSoundAfterDelay("dog_bark", 900);
			break;
		}
		case "professionForget_fishing":
		{
			if (Game1.player.newLevels.Contains(new Point(1, 5)) || Game1.player.newLevels.Contains(new Point(1, 10)))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
				break;
			}
			Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
			GameLocation.RemoveProfession(8);
			GameLocation.RemoveProfession(11);
			GameLocation.RemoveProfession(10);
			GameLocation.RemoveProfession(6);
			GameLocation.RemoveProfession(9);
			GameLocation.RemoveProfession(7);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
			int num4 = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[1]);
			if (num4 >= 5)
			{
				Game1.player.newLevels.Add(new Point(1, 5));
			}
			if (num4 >= 10)
			{
				Game1.player.newLevels.Add(new Point(1, 10));
			}
			DelayedAction.playSoundAfterDelay("dog_bark", 300);
			DelayedAction.playSoundAfterDelay("dog_bark", 900);
			break;
		}
		case "professionForget_combat":
		{
			if (Game1.player.newLevels.Contains(new Point(4, 5)) || Game1.player.newLevels.Contains(new Point(4, 10)))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
				break;
			}
			Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
			GameLocation.RemoveProfession(26);
			GameLocation.RemoveProfession(27);
			GameLocation.RemoveProfession(29);
			GameLocation.RemoveProfession(25);
			GameLocation.RemoveProfession(28);
			GameLocation.RemoveProfession(24);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
			int num = Farmer.checkForLevelGain(0, Game1.player.experiencePoints[4]);
			if (num >= 5)
			{
				Game1.player.newLevels.Add(new Point(4, 5));
			}
			if (num >= 10)
			{
				Game1.player.newLevels.Add(new Point(4, 10));
			}
			DelayedAction.playSoundAfterDelay("dog_bark", 300);
			DelayedAction.playSoundAfterDelay("dog_bark", 900);
			break;
		}
		case "specialCharmQuestion_Yes":
			if (Game1.player.Items.ContainsId("(O)446"))
			{
				Game1.player.holdUpItemThenMessage(new SpecialItem(3));
				Game1.player.removeFirstOfThisItemFromInventory("446");
				Game1.player.hasSpecialCharm = true;
				Game1.player.mailReceived.Add("SecretNote20_done");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_specialCharmNoFoot"));
			}
			break;
		case "evilShrineLeft_Yes":
			if (Game1.player.Items.ReduceId("(O)74", 1) > 0)
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(156f, 388f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					layerDepth = 0.038500004f,
					scale = 4f
				});
				for (int i7 = 0; i7 < 20; i7++)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.LightGray)
					{
						alpha = 0.75f,
						motion = new Vector2(1f, -0.5f),
						acceleration = new Vector2(-0.002f, 0f),
						interval = 99999f,
						layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = i7 * 25
					});
				}
				this.playSound("fireball");
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(4f, -2f)
				});
				if (Game1.player.getChildrenCount() > 1)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(4f, -1.5f),
						delayBeforeAnimationStart = 50
					});
				}
				string message = "";
				foreach (Child n in Game1.player.getChildren())
				{
					message += Game1.content.LoadString("Strings\\Locations:WitchHut_Goodbye", n.getName());
				}
				Game1.showGlobalMessage(message);
				Game1.player.getRidOfChildren();
				Game1.multiplayer.globalChatInfoMessage("EvilShrine", Game1.player.name.Value);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
			}
			break;
		case "evilShrineCenter_Yes":
			if (Game1.player.Money >= 30000)
			{
				Game1.player.Money -= 30000;
				Game1.player.wipeExMemories();
				Game1.multiplayer.globalChatInfoMessage("EvilShrine", Game1.player.name.Value);
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(468f, 328f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					layerDepth = 0.038500004f,
					scale = 4f
				});
				this.playSound("fireball");
				DelayedAction.playSoundAfterDelay("debuffHit", 500, this);
				int count = 0;
				Game1.player.faceDirection(2);
				Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
				{
					new FarmerSprite.AnimationFrame(94, 1500),
					new FarmerSprite.AnimationFrame(0, 1)
				});
				Game1.player.freezePause = 1500;
				Game1.player.jitterStrength = 1f;
				for (int i5 = 0; i5 < 20; i5++)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(7f, 5f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.SlateGray)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(-0.002f, 0f),
						interval = 99999f,
						layerDepth = 0.032f + (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = i5 * 25
					});
				}
				for (int i6 = 0; i6 < 16; i6++)
				{
					foreach (Vector2 v2 in Utility.getBorderOfThisRectangle(Utility.getRectangleCenteredAt(new Vector2(7f, 5f), 2 + i6 * 2)))
					{
						if (count % 2 == 0)
						{
							Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(692, 1853, 4, 4), 25f, 1, 16, v2 * 64f + new Vector2(32f, 32f), flicker: false, flipped: false)
							{
								layerDepth = 1f,
								delayBeforeAnimationStart = i6 * 50,
								scale = 4f,
								scaleChange = 1f,
								color = new Color(255 - Utility.getRedToGreenLerpColor(1f / (float)(i6 + 1)).R, 255 - Utility.getRedToGreenLerpColor(1f / (float)(i6 + 1)).G, 255 - Utility.getRedToGreenLerpColor(1f / (float)(i6 + 1)).B),
								acceleration = new Vector2(-0.1f, 0f)
							});
						}
						count++;
					}
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
			}
			break;
		case "evilShrineRightActivate_Yes":
			if (Game1.player.Items.ReduceId("(O)203", 1) > 0)
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					layerDepth = 0.038500004f,
					scale = 4f
				});
				this.playSound("fireball");
				DelayedAction.playSoundAfterDelay("batScreech", 500, this);
				for (int i = 0; i < 20; i++)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.DarkSlateBlue)
					{
						alpha = 0.75f,
						motion = new Vector2(-0.1f, -0.5f),
						acceleration = new Vector2(-0.002f, 0f),
						interval = 99999f,
						layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = i * 60
					});
				}
				Game1.player.freezePause = 1501;
				for (int i2 = 0; i2 < 28; i2++)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 347, 13, 13), 50f, 4, 9999, new Vector2(12f, 5f) * 64f, flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 500 + i2 * 25,
						motion = new Vector2(Game1.random.Next(1, 5) * Game1.random.Choose(-1, 1), Game1.random.Next(1, 5) * Game1.random.Choose(-1, 1))
					});
				}
				Game1.spawnMonstersAtNight = true;
				Game1.multiplayer.globalChatInfoMessage("MonstersActivated", Game1.player.name.Value);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
			}
			break;
		case "evilShrineRightDeActivate_Yes":
			if (Game1.player.Items.ReduceId("(O)203", 1) > 0)
			{
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(780f, 388f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					layerDepth = 0.038500004f,
					scale = 4f
				});
				this.playSound("fireball");
				for (int i8 = 0; i8 < 20; i8++)
				{
					Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(12f, 6f) * 64f + new Vector2(Game1.random.Next(-32, 64), Game1.random.Next(16)), flipped: false, 0.002f, Color.DarkSlateBlue)
					{
						alpha = 0.75f,
						motion = new Vector2(0f, -0.5f),
						acceleration = new Vector2(-0.002f, 0f),
						interval = 99999f,
						layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = i8 * 25
					});
				}
				Game1.spawnMonstersAtNight = false;
				Game1.multiplayer.globalChatInfoMessage("MonstersDeActivated", Game1.player.name.Value);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:WitchHut_NoOffering"));
			}
			break;
		case "buyJojaCola_Yes":
			if (Game1.player.Money >= 75)
			{
				Game1.player.Money -= 75;
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(O)167"));
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
			break;
		case "WizardShrine_Yes":
			if (Game1.player.Money >= 500)
			{
				Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);
				Game1.player.Money -= 500;
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
			}
			break;
		case "Backpack_Purchase":
			if (Game1.player.maxItems.Value == 12 && Game1.player.Money >= 2000)
			{
				Game1.player.Money -= 2000;
				Game1.player.increaseBackpackSize(12);
				Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708")));
				Game1.multiplayer.globalChatInfoMessage("BackpackLarge", Game1.player.Name);
			}
			else if (Game1.player.maxItems.Value < 36 && Game1.player.Money >= 10000)
			{
				Game1.player.Money -= 10000;
				Game1.player.maxItems.Value += 12;
				Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709")));
				for (int i4 = 0; i4 < Game1.player.maxItems.Value; i4++)
				{
					if (Game1.player.Items.Count <= i4)
					{
						Game1.player.Items.Add(null);
					}
				}
				Game1.multiplayer.globalChatInfoMessage("BackpackDeluxe", Game1.player.Name);
			}
			else if (Game1.player.maxItems.Value != 36)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
			}
			break;
		case "ClubSeller_I'll":
			if (Game1.player.Money >= 1000000)
			{
				Game1.player.Money -= 1000000;
				Game1.exitActiveMenu();
				Game1.player.forceCanMove();
				Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)127"));
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_NotEnoughMoney"));
			}
			break;
		case "BuyQiCoins_Yes":
			if (Game1.player.Money >= 1000)
			{
				Game1.player.Money -= 1000;
				this.localSound("Pickup_Coin15");
				Game1.player.clubCoins += 100;
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8715"));
			}
			break;
		case "Shaft_Jump":
			if (this is MineShaft mineShaft)
			{
				mineShaft.enterMineShaft();
			}
			break;
		case "mariner_Buy":
			if (Game1.player.Money >= 5000)
			{
				Game1.player.Money -= 5000;
				Item mermaidPendant = ItemRegistry.Create("(O)460");
				mermaidPendant.specialItem = true;
				Game1.player.addItemByMenuIfNecessary(mermaidPendant);
				if (Game1.activeClickableMenu == null)
				{
					Game1.player.holdUpItemThenMessage(ItemRegistry.Create("(O)460"));
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
			break;
		case "upgrade_Yes":
			this.houseUpgradeAccept();
			break;
		case "communityUpgrade_Yes":
			this.communityUpgradeAccept();
			break;
		case "adventureGuild_Shop":
			Game1.player.forceCanMove();
			Utility.TryOpenShopMenu("AdventureShop", "Marlon");
			break;
		case "adventureGuild_Recovery":
			Game1.player.forceCanMove();
			Utility.TryOpenShopMenu("AdventureGuildRecovery", "Marlon");
			break;
		case "carpenter_Shop":
			Game1.player.forceCanMove();
			Utility.TryOpenShopMenu("Carpenter", "Robin");
			break;
		case "carpenter_Upgrade":
			this.houseUpgradeOffer();
			break;
		case "carpenter_Renovate":
			Game1.player.forceCanMove();
			HouseRenovation.ShowRenovationMenu();
			break;
		case "carpenter_CommunityUpgrade":
			this.communityUpgradeOffer();
			break;
		case "carpenter_Construct":
			this.ShowConstructOptions("Robin");
			break;
		case "Eat_Yes":
			Game1.player.isEating = false;
			Game1.player.eatHeldObject();
			break;
		case "Eat_No":
			Game1.player.isEating = false;
			Game1.player.completelyStopAnimatingOrDoingAction();
			break;
		case "Marnie_Supplies":
			Utility.TryOpenShopMenu("AnimalShop", "Marnie");
			break;
		case "Marnie_Adopt":
			Utility.TryOpenShopMenu("PetAdoption", "Marnie");
			break;
		case "Marnie_Purchase":
			Game1.player.forceCanMove();
			Game1.currentLocation.ShowAnimalShopMenu();
			break;
		case "Blacksmith_Shop":
			Utility.TryOpenShopMenu("Blacksmith", "Clint");
			break;
		case "Blacksmith_Upgrade":
			if (Game1.player.daysLeftForToolUpgrade.Value > 0)
			{
				NPC n2 = this.getCharacterFromName("Clint");
				if (n2 != null)
				{
					Game1.DrawDialogue(n2, "Data\\ExtraDialogue:Clint_StillWorking", Game1.player.toolBeingUpgraded.Value.DisplayName);
				}
			}
			else
			{
				Utility.TryOpenShopMenu("ClintUpgrade", "Clint");
			}
			break;
		case "Blacksmith_Process":
			Game1.activeClickableMenu = new GeodeMenu();
			break;
		case "Dungeon_Go":
			Game1.enterMine(Game1.CurrentMineLevel + 1);
			break;
		case "Mine_Return":
			Game1.enterMine(Game1.player.deepestMineLevel);
			break;
		case "Mine_Enter":
			Game1.enterMine(1);
			break;
		case "Sleep_Yes":
			this.startSleep();
			break;
		case "SleepTent_Yes":
			Game1.player.isInBed.Value = true;
			Game1.player.sleptInTemporaryBed.Value = true;
			Game1.displayFarmer = false;
			Game1.playSound("sandyStep");
			DelayedAction.playSoundAfterDelay("sandyStep", 500);
			this.startSleep();
			break;
		case "Mine_Yes":
			if (Game1.CurrentMineLevel > 120)
			{
				Game1.warpFarmer("SkullCave", 3, 4, 2);
			}
			else
			{
				Game1.warpFarmer("UndergroundMine", 16, 16, flip: false);
			}
			break;
		case "Mine_No":
		{
			Response[] noYesResponses = new Response[2]
			{
				new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")),
				new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes"))
			};
			this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_ResetMine")), noYesResponses, "ResetMine");
			break;
		}
		case "ExitMine_Leave":
		case "ExitMine_Yes":
			if (Game1.CurrentMineLevel == 77377)
			{
				Game1.warpFarmer("Mine", 67, 10, flip: true);
			}
			else if (Game1.CurrentMineLevel > 120)
			{
				Game1.warpFarmer("SkullCave", 3, 4, 2);
			}
			else
			{
				Game1.warpFarmer("Mine", 23, 8, flip: false);
			}
			break;
		case "ExitMine_Go":
			Game1.enterMine(Game1.CurrentMineLevel - 1);
			break;
		case "MinecartGame_Endless":
			Game1.currentMinigame = new MineCart(0, 2);
			break;
		case "MinecartGame_Progress":
			Game1.currentMinigame = new MineCart(0, 3);
			break;
		case "CowboyGame_NewGame":
			Game1.player.jotpkProgress.Value = null;
			Game1.currentMinigame = new AbigailGame();
			break;
		case "CowboyGame_Continue":
			Game1.currentMinigame = new AbigailGame();
			break;
		case "ClubCard_Yes.":
		case "ClubCard_That's":
		{
			Game1.addMail("bouncerGone", noLetter: true, sendToEveryone: true);
			this.playSound("explosion");
			Game1.flashAlpha = 5f;
			this.characters.Remove(this.getCharacterFromName("Bouncer"));
			NPC sandy = this.getCharacterFromName("Sandy");
			if (sandy != null)
			{
				sandy.faceDirection(1);
				sandy.setNewDialogue("Data\\ExtraDialogue:Sandy_PlayerClubMember");
				sandy.doEmote(16);
			}
			Game1.pauseThenMessage(500, Game1.content.LoadString("Strings\\Locations:Club_Bouncer_PlayerClubMember"));
			Game1.player.Halt();
			Game1.getCharacterFromName("Mister Qi")?.setNewDialogue("Data\\ExtraDialogue:MisterQi_PlayerClubMember");
			break;
		}
		case "CalicoJack_Rules":
			Game1.multipleDialogues(new string[2]
			{
				Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules1"),
				Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_Rules2")
			});
			break;
		case "CalicoJackHS_Play":
			if (Game1.player.clubCoins >= 1000)
			{
				Game1.currentMinigame = new CalicoJack(-1, highStakes: true);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJackHS_NotEnoughCoins"));
			}
			break;
		case "CalicoJack_Play":
			if (Game1.player.clubCoins >= 100)
			{
				Game1.currentMinigame = new CalicoJack();
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_CalicoJack_NotEnoughCoins"));
			}
			break;
		case "BuyClubCoins_Yes":
			if (Game1.player.Money >= 1000)
			{
				Game1.player.Money -= 1000;
				Game1.player.clubCoins += 10;
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
			break;
		case "Bouquet_Yes":
			if (Game1.player.Money >= 500)
			{
				if (Game1.player.ActiveObject == null)
				{
					Game1.player.Money -= 500;
					Object bouquet = ItemRegistry.Create<Object>("(O)458");
					bouquet.CanBeSetDown = false;
					Game1.player.grabObject(bouquet);
					return true;
				}
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
			break;
		case "Mariner_Buy":
			if (Game1.player.Money >= 5000)
			{
				Game1.player.Money -= 5000;
				Object mermaidPendant2 = ItemRegistry.Create<Object>("(O)460");
				mermaidPendant2.CanBeSetDown = false;
				Game1.player.grabObject(mermaidPendant2);
				return true;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			break;
		case "ClearHouse_Yes":
		{
			Vector2 playerPos = Game1.player.Tile;
			Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
			foreach (Vector2 offset in adjacentTilesOffsets)
			{
				Vector2 v = playerPos + offset;
				this.objects.Remove(v);
			}
			break;
		}
		case "ExitToTitle_Yes":
			Game1.fadeScreenToBlack();
			Game1.exitToTitle = true;
			break;
		case "telephone_Carpenter_HouseCost":
		{
			NPC characterFromName = Game1.getCharacterFromName("Robin");
			string upgradeTextKey = "Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse" + (Game1.player.houseUpgradeLevel.Value + 1);
			string upgrade_text = Game1.content.LoadString(upgradeTextKey, "65,000", "100");
			if (upgrade_text.Contains('.'))
			{
				upgrade_text = upgrade_text.Substring(0, upgrade_text.LastIndexOf('.') + 1);
			}
			else if (upgrade_text.Contains('。'))
			{
				upgrade_text = upgrade_text.Substring(0, upgrade_text.LastIndexOf('。') + 1);
			}
			Game1.DrawDialogue(new Dialogue(characterFromName, upgradeTextKey, upgrade_text)
			{
				overridePortrait = Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine")
			});
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
			});
			break;
		}
		case "telephone_Carpenter_BuildingCost":
		{
			GameLocation targetLocation = Game1.getFarm();
			if (Game1.currentLocation.IsBuildableLocation())
			{
				targetLocation = Game1.currentLocation;
			}
			Game1.activeClickableMenu = new CarpenterMenu("Robin", targetLocation);
			if (Game1.activeClickableMenu is CarpenterMenu menu4)
			{
				menu4.readOnly = true;
				menu4.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu4.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
				{
					this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
				});
			}
			break;
		}
		case "telephone_Carpenter_ShopStock":
			Utility.TryOpenShopMenu("Carpenter", null, playOpenSound: true);
			if (Game1.activeClickableMenu is ShopMenu menu3)
			{
				menu3.readOnly = true;
				menu3.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu3.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
				{
					this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
				});
			}
			break;
		case "telephone_Blacksmith_UpgradeCost":
			this.answerDialogueAction("Blacksmith_Upgrade", LegacyShims.EmptyArray<string>());
			if (Game1.activeClickableMenu is ShopMenu menu2)
			{
				menu2.readOnly = true;
				menu2.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu2.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
				{
					this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
				});
			}
			break;
		case "telephone_SeedShop_CheckSeedStock":
			if (Game1.getLocationFromName("SeedShop") is SeedShop)
			{
				if (Utility.TryOpenShopMenu("SeedShop", null, playOpenSound: true) && Game1.activeClickableMenu is ShopMenu menu)
				{
					menu.readOnly = true;
					menu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(menu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
					{
						this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
					});
				}
			}
			else
			{
				this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
			}
			break;
		case "telephone_AnimalShop_CheckAnimalPrices":
			Game1.currentLocation.ShowAnimalShopMenu(delegate(PurchaseAnimalsMenu purchaseAnimalsMenu)
			{
				purchaseAnimalsMenu.readOnly = true;
				purchaseAnimalsMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(purchaseAnimalsMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
				{
					this.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
				});
			});
			break;
		case "ShrineOfSkullChallenge_Yes":
			Game1.player.team.toggleSkullShrineOvernight.Value = true;
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
			Game1.multiplayer.globalChatInfoMessage(Game1.player.team.skullShrineActivated.Value ? "HardModeSkullCaveDeactivated" : "HardModeSkullCaveActivated", Game1.player.Name);
			this.playSound(Game1.player.team.skullShrineActivated.Value ? "skeletonStep" : "serpentDie");
			break;
		case "ShrineOfChallenge_Yes":
			Game1.player.team.toggleMineShrineOvernight.Value = true;
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_Activated"));
			Game1.multiplayer.globalChatInfoMessage((!Game1.player.team.mineShrineActivated.Value) ? "HardModeMinesActivated" : "HardModeMinesDeactivated", Game1.player.Name);
			DelayedAction.functionAfterDelay(delegate
			{
				if (!Game1.player.team.mineShrineActivated.Value)
				{
					Game1.playSound("fireball");
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(8.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						lightId = "ShrineOfChallenge_Activation_1",
						id = 888,
						lightRadius = 2f,
						scale = 4f,
						yPeriodic = true,
						lightcolor = new Color(100, 0, 0),
						yPeriodicLoopTime = 1000f,
						yPeriodicRange = 4f,
						layerDepth = 0.04544f
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(10.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						lightId = "ShrineOfChallenge_Activation_2",
						id = 889,
						lightRadius = 2f,
						scale = 4f,
						lightcolor = new Color(100, 0, 0),
						yPeriodic = true,
						yPeriodicLoopTime = 1100f,
						yPeriodicRange = 4f,
						layerDepth = 0.04544f
					});
				}
				else
				{
					this.removeTemporarySpritesWithID(888);
					this.removeTemporarySpritesWithID(889);
					Game1.playSound("fireball");
				}
			}, 500);
			break;
		default:
			if (questionAndAnswer.StartsWith("pagedResponse"))
			{
				string response = questionAndAnswer.Substring("pagedResponse".Length + 1);
				Action<string> onPagedResponse = GameLocation._OnPagedResponse;
				this._CleanupPagedResponses();
				onPagedResponse?.Invoke(response);
			}
			break;
		}
		return true;
	}

	public void playShopPhoneNumberSounds(string whichShop)
	{
		Random r = Utility.CreateRandom(whichShop.GetHashCode());
		DelayedAction.playSoundAfterDelay("telephone_dialtone", 495, null, null, 1200);
		DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1200, null, null, 1200 + r.Next(-4, 5) * 100);
		DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1370, null, null, 1200 + r.Next(-4, 5) * 100);
		DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1600, null, null, 1200 + r.Next(-4, 5) * 100);
		DelayedAction.playSoundAfterDelay("telephone_buttonPush", 1850, null, null, 1200 + r.Next(-4, 5) * 100);
		DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2030, null, null, 1200 + r.Next(-4, 5) * 100);
		DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2250, null, null, 1200 + r.Next(-4, 5) * 100);
		DelayedAction.playSoundAfterDelay("telephone_buttonPush", 2410, null, null, 1200 + r.Next(-4, 5) * 100);
		DelayedAction.playSoundAfterDelay("telephone_ringingInEar", 3150);
	}

	public virtual bool answerDialogue(Response answer)
	{
		string[] questionParams = ((this.lastQuestionKey != null) ? ArgUtility.SplitBySpace(this.lastQuestionKey) : null);
		string questionAndAnswer = ((questionParams != null) ? (questionParams[0] + "_" + answer.responseKey) : null);
		if (answer.responseKey.Equals("Move"))
		{
			Game1.player.grabObject(this.actionObjectForQuestionDialogue);
			this.removeObject(this.actionObjectForQuestionDialogue.TileLocation, showDestroyedObject: false);
			this.actionObjectForQuestionDialogue = null;
			return true;
		}
		if (this.afterQuestion != null)
		{
			this.afterQuestion(Game1.player, answer.responseKey);
			this.afterQuestion = null;
			Game1.objectDialoguePortraitPerson = null;
			return true;
		}
		if (questionAndAnswer == null)
		{
			return false;
		}
		return this.answerDialogueAction(questionAndAnswer, questionParams);
	}

	public static bool AreStoresClosedForFestival()
	{
		if (Utility.isFestivalDay())
		{
			return Utility.getStartTimeOfFestival() < 1900;
		}
		return false;
	}

	public static void RemoveProfession(int profession)
	{
		if (Game1.player.professions.Remove(profession))
		{
			LevelUpMenu.removeImmediateProfessionPerk(profession);
		}
	}

	public static bool canRespec(int skill_index)
	{
		if (Game1.player.GetUnmodifiedSkillLevel(skill_index) < 5)
		{
			return false;
		}
		if (Game1.player.newLevels.Contains(new Point(skill_index, 5)) || Game1.player.newLevels.Contains(new Point(skill_index, 10)))
		{
			return false;
		}
		return true;
	}

	public void setObject(Vector2 v, Object o)
	{
		this.objects[v] = o;
	}

	private void houseUpgradeOffer()
	{
		switch (Game1.player.houseUpgradeLevel.Value)
		{
		case 0:
			this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1")), this.createYesNoResponses(), "upgrade");
			break;
		case 1:
			this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2", "65,000", "100")), this.createYesNoResponses(), "upgrade");
			break;
		case 2:
			this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3")), this.createYesNoResponses(), "upgrade");
			break;
		}
	}

	private void communityUpgradeOffer()
	{
		if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
		{
			this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade1")), this.createYesNoResponses(), "communityUpgrade");
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "pamHouseUpgradeAsked", MailType.Received, add: true);
		}
		else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
		{
			this.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_CommunityUpgrade2")), this.createYesNoResponses(), "communityUpgrade");
		}
	}

	/// <summary>Whether crab pots on a given tile can only catch ocean fish, regardless of the location's crab pot fish areas.</summary>
	/// <param name="x">The X tile position to check.</param>
	/// <param name="y">The Y tile position to check.</param>
	/// <returns>Returns true to only catch ocean fish, or false to apply the normal crab pot behavior based on <c>Data/Locations</c> or <see cref="F:StardewValley.GameLocation.DefaultCrabPotFishTypes" />.</returns>
	public virtual bool catchOceanCrabPotFishFromThisSpot(int x, int y)
	{
		return false;
	}

	private void communityUpgradeAccept()
	{
		if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
		{
			if (Game1.player.Money >= 500000 && Game1.player.Items.ContainsId("(O)388", 950))
			{
				Game1.player.Money -= 500000;
				Game1.player.Items.ReduceId("(O)388", 950);
				Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_PamUpgrade_Accepted");
				Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
				Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value = 3;
				Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
			}
			else if (Game1.player.Money < 500000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood", 950));
			}
		}
		else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
		{
			if (Game1.player.Money >= 300000)
			{
				Game1.player.Money -= 300000;
				Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", add: true);
				Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
				Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value = 3;
				Game1.multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
			}
			else if (Game1.player.Money < 300000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
			}
		}
	}

	private void houseUpgradeAccept()
	{
		switch (Game1.player.houseUpgradeLevel.Value)
		{
		case 0:
			if (Game1.player.Money >= 10000 && Game1.player.Items.ContainsId("(O)388", 450))
			{
				Game1.player.daysUntilHouseUpgrade.Value = 3;
				Game1.player.Money -= 10000;
				Game1.player.Items.ReduceId("(O)388", 450);
				Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", add: true);
				Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
				Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale));
			}
			else if (Game1.player.Money < 10000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood", 450));
			}
			break;
		case 1:
			if (Game1.player.Money >= 65000 && Game1.player.Items.ContainsId("(O)709", 100))
			{
				Game1.player.daysUntilHouseUpgrade.Value = 3;
				Game1.player.Money -= 65000;
				Game1.player.Items.ReduceId("(O)709", 100);
				Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", add: true);
				Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
				Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale));
			}
			else if (Game1.player.Money < 65000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughHardwood", 100));
			}
			break;
		case 2:
			if (Game1.player.Money >= 100000)
			{
				Game1.player.daysUntilHouseUpgrade.Value = 3;
				Game1.player.Money -= 100000;
				Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted", add: true);
				Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
				Game1.multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale));
			}
			else if (Game1.player.Money < 100000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
			}
			break;
		}
	}

	public void destroyObject(Vector2 tileLocation, Farmer who)
	{
		this.destroyObject(tileLocation, hardDestroy: false, who);
	}

	public void destroyObject(Vector2 tileLocation, bool hardDestroy, Farmer who)
	{
		if (!this.objects.TryGetValue(tileLocation, out var obj) || obj.fragility.Value == 2 || obj is Chest || !(obj.QualifiedItemId != "(BC)165"))
		{
			return;
		}
		bool remove = false;
		if (obj.Type == "Fish" || obj.Type == "Cooking" || obj.Type == "Crafting")
		{
			if (!(obj is BreakableContainer))
			{
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), flicker: true, obj.bigCraftable.Value, obj.flipped.Value);
				sprite.CopyAppearanceFromItemId(obj.QualifiedItemId, obj.showNextIndex.Value ? 1 : 0);
				sprite.scale = 4f;
				Game1.multiplayer.broadcastSprites(this, sprite);
			}
			remove = true;
		}
		else if (obj.CanBeGrabbed || hardDestroy)
		{
			remove = true;
		}
		if (obj.IsBreakableStone())
		{
			remove = true;
			this.OnStoneDestroyed(obj.ItemId, (int)tileLocation.X, (int)tileLocation.Y, who);
		}
		if (remove)
		{
			this.objects.Remove(tileLocation);
		}
	}

	public void addOneTimeGiftBox(Item i, int x, int y, int whichGiftBox = 2)
	{
		string id = this.Name + "_giftbox_" + x + "_" + y;
		if (!Game1.player.mailReceived.Contains(id))
		{
			Vector2 v = new Vector2(x, y);
			if (!(this.overlayObjects.GetValueOrDefault(v) is Chest oldChest) || !(oldChest.mailToAddOnItemDump == id))
			{
				this.cleanUpTileForMapOverride(new Point(x, y));
			}
			if (!this.overlayObjects.ContainsKey(v))
			{
				Chest c = new Chest(new List<Item> { i }, v, giftbox: true, whichGiftBox)
				{
					mailToAddOnItemDump = id
				};
				this.overlayObjects.Add(v, c);
			}
		}
	}

	/// <summary>Get the unique ID of the location context in <c>Data/LocationContexts</c> which includes this location.</summary>
	public virtual string GetLocationContextId()
	{
		if (this.locationContextId == null)
		{
			if (this.map == null)
			{
				this.reloadMap();
			}
			if (this.map != null && this.map.Properties.TryGetValue("LocationContext", out var contextId))
			{
				if (Game1.locationContextData.ContainsKey(contextId))
				{
					this.locationContextId = contextId;
				}
				else
				{
					Game1.log.Error($"Location {this.NameOrUniqueName} has invalid LocationContext map property '{contextId}', ignoring value.");
				}
			}
			if (this.locationContextId == null)
			{
				this.locationContextId = this.GetParentLocation()?.GetLocationContextId() ?? "Default";
			}
		}
		return this.locationContextId;
	}

	/// <summary>Get the data for the location context in <c>Data/LocationContexts</c> which includes this location.</summary>
	public virtual LocationContextData GetLocationContext()
	{
		return LocationContexts.Require(this.GetLocationContextId());
	}

	/// <summary>Get whether this location is in the desert context.</summary>
	public bool InDesertContext()
	{
		return this.GetLocationContextId() == "Desert";
	}

	/// <summary>Get whether this location is in the Ginger Island context.</summary>
	public bool InIslandContext()
	{
		return this.GetLocationContextId() == "Island";
	}

	/// <summary>Get whether this location is in the default valley context.</summary>
	public bool InValleyContext()
	{
		return this.GetLocationContextId() == "Default";
	}

	public virtual bool sinkDebris(Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
	{
		if (debris.isEssentialItem())
		{
			return false;
		}
		if (debris.item != null && debris.item.HasContextTag("book_item"))
		{
			return false;
		}
		if (debris.debrisType.Value == Debris.DebrisType.OBJECT && debris.chunkType.Value == 74)
		{
			return false;
		}
		if (debris.floppingFish.Value)
		{
			foreach (Building building in this.buildings)
			{
				if (building.isTileFishable(chunkTile))
				{
					return false;
				}
			}
		}
		if (!debris.isSinking.Value)
		{
			debris.isSinking.Value = true;
			Debris.DebrisType value = debris.debrisType.Value;
			if (value == Debris.DebrisType.OBJECT || value == Debris.DebrisType.RESOURCE)
			{
				if (Game1.random.NextBool())
				{
					this.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 8, 0, chunkPosition + new Vector2(-8f), flicker: false, Game1.random.NextBool(), 0.001f, 0.02f, Color.White, 1f, 0.003f, 0f, 0f)
					{
						delayBeforeAnimationStart = Game1.random.Next(300),
						startSound = "quickSlosh"
					});
				}
				return false;
			}
		}
		else
		{
			bool anySunk = false;
			foreach (Chunk chunk in debris.Chunks)
			{
				if (chunk.sinkTimer.Value <= 0)
				{
					anySunk = true;
					break;
				}
			}
			if (!anySunk)
			{
				return false;
			}
		}
		if (debris.debrisType.Value == Debris.DebrisType.CHUNKS)
		{
			this.localSound("quickSlosh");
			this.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 8, 0, chunkPosition + new Vector2(-8f), flicker: false, Game1.random.NextBool(), 0.001f, 0.02f, Color.White, 1f, 0.003f, 0f, 0f));
			return true;
		}
		this.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 300f, 2, 1, chunkPosition + new Vector2(-8f), flicker: false, flipped: false));
		this.localSound("dropItemInWater");
		return true;
	}

	public virtual bool doesTileSinkDebris(int xTile, int yTile, Debris.DebrisType type)
	{
		if (this.isTileBuildingFishable(xTile, yTile))
		{
			return true;
		}
		if (type == Debris.DebrisType.CHUNKS)
		{
			if (this.isWaterTile(xTile, yTile))
			{
				return !this.hasTileAt(xTile, yTile, "Buildings");
			}
			return false;
		}
		if (this.isWaterTile(xTile, yTile) && !this.isTileUpperWaterBorder(this.getTileIndexAt(xTile, yTile, "Buildings", "untitled tile sheet")))
		{
			return this.doesTileHaveProperty(xTile, yTile, "Passable", "Buildings") == null;
		}
		return false;
	}

	private bool isTileUpperWaterBorder(int index)
	{
		switch (index)
		{
		case 183:
		case 184:
		case 185:
		case 211:
		case 1182:
		case 1183:
		case 1184:
		case 1210:
			return true;
		default:
			return false;
		}
	}

	public virtual bool doesEitherTileOrTileIndexPropertyEqual(int xTile, int yTile, string propertyName, string layerName, string propertyValue)
	{
		Layer layer = this.map?.GetLayer(layerName);
		if (layer != null)
		{
			Tile tmp = layer.PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
			if (tmp != null && tmp.TileIndexProperties.TryGetValue(propertyName, out var property) && property == propertyValue)
			{
				return true;
			}
			if (tmp != null && layer.PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size).Properties.TryGetValue(propertyName, out var property2) && property2 == propertyValue)
			{
				return true;
			}
		}
		return propertyValue == null;
	}

	/// <summary>Get whether the given tile prohibits spawned items.</summary>
	/// <param name="tile">The tile position to check.</param>
	/// <param name="type">The spawn type. This can be <c>Grass</c> (weeds, stones, and other debris), <c>Tree</c> (trees), or <c>All</c> (any other type).</param>
	public virtual bool IsNoSpawnTile(Vector2 tile, string type = "All", bool ignoreTileSheetProperties = false)
	{
		int x = (int)tile.X;
		int y = (int)tile.Y;
		string noSpawn = this.doesTileHaveProperty(x, y, "NoSpawn", "Back", ignoreTileSheetProperties);
		switch (noSpawn)
		{
		case "Grass":
		case "Tree":
			if (type == noSpawn)
			{
				return true;
			}
			break;
		default:
		{
			if (!bool.TryParse(noSpawn, out var isBanned) || isBanned)
			{
				return true;
			}
			break;
		}
		case null:
			break;
		}
		return this.getBuildingAt(tile) != null;
	}

	public virtual string doesTileHaveProperty(int xTile, int yTile, string propertyName, string layerName, bool ignoreTileSheetProperties = false)
	{
		Vector2 tilePos = new Vector2(xTile, yTile);
		bool buildingOnTile = false;
		foreach (Building building in this.buildings)
		{
			if (!building.isMoving && building.occupiesTile(tilePos, applyTilePropertyRadius: true))
			{
				string tileProperty = null;
				if (building.doesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tileProperty))
				{
					return tileProperty;
				}
				buildingOnTile = buildingOnTile || building.occupiesTile(tilePos);
			}
		}
		foreach (Furniture f in this.furniture)
		{
			if ((float)xTile >= f.tileLocation.X - (float)f.GetAdditionalTilePropertyRadius() && (float)xTile < f.tileLocation.X + (float)f.getTilesWide() + (float)f.GetAdditionalTilePropertyRadius() && (float)yTile >= f.tileLocation.Y - (float)f.GetAdditionalTilePropertyRadius() && (float)yTile < f.tileLocation.Y + (float)f.getTilesHigh() + (float)f.GetAdditionalTilePropertyRadius())
			{
				string tile_property = null;
				if (f.DoesTileHaveProperty(xTile, yTile, propertyName, layerName, ref tile_property))
				{
					return tile_property;
				}
			}
		}
		if (!buildingOnTile && this.map != null)
		{
			Tile tile = this.map.GetLayer(layerName)?.Tiles[xTile, yTile];
			if (tile != null)
			{
				if (tile.Properties.TryGetValue(propertyName, out var propertyValue))
				{
					return propertyValue;
				}
				if (!ignoreTileSheetProperties && tile.TileIndexProperties.TryGetValue(propertyName, out propertyValue))
				{
					return propertyValue;
				}
			}
		}
		return null;
	}

	public virtual string doesTileHavePropertyNoNull(int xTile, int yTile, string propertyName, string layerName)
	{
		return this.doesTileHaveProperty(xTile, yTile, propertyName, layerName) ?? "";
	}

	/// <summary>Get the space-delimited values defined by a map property.</summary>
	/// <param name="propertyName">The property name to read.</param>
	/// <param name="layerId">The ID for the layer whose tile to check.</param>
	/// <param name="tileX">The X tile position for the map tile to check.</param>
	/// <param name="tileY">The Y tile position for the map tile to check.</param>
	/// <returns>Returns the map property value, or an empty array if it's empty or unset.</returns>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.doesTileHaveProperty(System.Int32,System.Int32,System.String,System.String,System.Boolean)" /> or <see cref="M:StardewValley.GameLocation.doesTileHavePropertyNoNull(System.Int32,System.Int32,System.String,System.String)" /> to get a tile property without splitting it.</remarks>
	public string[] GetTilePropertySplitBySpaces(string propertyName, string layerId, int tileX, int tileY)
	{
		string raw = this.doesTileHaveProperty(tileX, tileY, propertyName, layerId);
		if (raw == null)
		{
			return LegacyShims.EmptyArray<string>();
		}
		return ArgUtility.SplitBySpace(raw);
	}

	/// <summary>Whether a tile coordinate matches a map water tile.</summary>
	/// <param name="xTile">The X tile position.</param>
	/// <param name="yTile">The Y tile position.</param>
	public bool isWaterTile(int xTile, int yTile)
	{
		return this.doesTileHaveProperty(xTile, yTile, "Water", "Back") != null;
	}

	public bool isOpenWater(int xTile, int yTile)
	{
		if (!this.isWaterTile(xTile, yTile))
		{
			return false;
		}
		int tileIndexAt = this.getTileIndexAt(xTile, yTile, "Buildings", "outdoors");
		if ((uint)(tileIndexAt - 628) <= 1u || tileIndexAt == 734 || tileIndexAt == 759)
		{
			return false;
		}
		return !this.objects.ContainsKey(new Vector2(xTile, yTile));
	}

	public bool isCropAtTile(int tileX, int tileY)
	{
		Vector2 v = new Vector2(tileX, tileY);
		if (this.terrainFeatures.TryGetValue(v, out var terrainFeature) && terrainFeature is HoeDirt dirt)
		{
			return dirt.crop != null;
		}
		return false;
	}

	/// <summary>Try to add an object to the location.</summary>
	/// <param name="obj">The object to place. This must be a new instance or <see cref="M:StardewValley.Item.getOne" /> copy; passing a stack that's stored in an inventory will link their state and cause unexpected behaviors.</param>
	/// <param name="dropLocation">The pixel position at which to place the item.</param>
	/// <param name="viewport">Unused.</param>
	/// <param name="initialPlacement">Whether to place the item regardless of the <see cref="F:StardewValley.Object.canBeSetDown" /> field.</param>
	/// <param name="who">The player placing the object, if applicable.</param>
	/// <returns>Returns whether the object was added to the location.</returns>
	public virtual bool dropObject(Object obj, Vector2 dropLocation, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
	{
		Vector2 tileLocation = new Vector2((int)dropLocation.X / 64, (int)dropLocation.Y / 64);
		obj.Location = this;
		obj.TileLocation = tileLocation;
		obj.isSpawnedObject.Value = true;
		if (!this.isTileOnMap(tileLocation) || this.map.RequireLayer("Back").PickTile(new Location((int)dropLocation.X, (int)dropLocation.Y), Game1.viewport.Size) == null || this.map.RequireLayer("Back").Tiles[(int)tileLocation.X, (int)tileLocation.Y].TileIndexProperties.ContainsKey("Unplaceable"))
		{
			return false;
		}
		if (obj.bigCraftable.Value)
		{
			if (!this.isFarm.Value)
			{
				return false;
			}
			if (!obj.setOutdoors.Value && this.isOutdoors.Value)
			{
				return false;
			}
			if (!obj.setIndoors.Value && !this.isOutdoors.Value)
			{
				return false;
			}
			if (obj.performDropDownAction(who))
			{
				return false;
			}
		}
		else if (obj.Type == "Crafting" && obj.performDropDownAction(who))
		{
			obj.CanBeSetDown = false;
		}
		bool tilePassable = this.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), viewport) && this.CanItemBePlacedHere(tileLocation);
		if ((obj.CanBeSetDown || initialPlacement) && tilePassable && !this.isTileHoeDirt(tileLocation))
		{
			if (!this.objects.TryAdd(tileLocation, obj))
			{
				return false;
			}
		}
		else if (this.isWaterTile((int)tileLocation.X, (int)tileLocation.Y))
		{
			Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(28, 300f, 2, 1, dropLocation, flicker: false, obj.flipped.Value));
			this.playSound("dropItemInWater");
		}
		else
		{
			if (obj.CanBeSetDown && !tilePassable)
			{
				return false;
			}
			if (obj.ParentSheetIndex >= 0 && obj.Type != null)
			{
				if (obj.Type == "Fish" || obj.Type == "Cooking" || obj.Type == "Crafting")
				{
					TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 150f, 1, 3, dropLocation, flicker: true, obj.flipped.Value);
					sprite.CopyAppearanceFromItemId(obj.QualifiedItemId);
					Game1.multiplayer.broadcastSprites(this, sprite);
				}
				else
				{
					TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite(0, 150f, 1, 3, dropLocation, flicker: true, obj.flipped.Value);
					sprite2.CopyAppearanceFromItemId(obj.QualifiedItemId, 1);
					Game1.multiplayer.broadcastSprites(this, sprite2);
				}
			}
		}
		return true;
	}

	private void rumbleAndFade(int milliseconds)
	{
		this.rumbleAndFadeEvent.Fire(milliseconds);
	}

	private void performRumbleAndFade(int milliseconds)
	{
		if (Game1.currentLocation == this)
		{
			Rumble.rumbleAndFade(1f, milliseconds);
		}
	}

	/// <summary>Sends a request to damage players within the current location.</summary>
	/// <param name="area">The location pixel area where players will take damage.</param>
	/// <param name="damage">The amount of damage the player should take.</param>
	/// <param name="isBomb">Whether the damage source was a bomb.</param>
	private void damagePlayers(Microsoft.Xna.Framework.Rectangle area, int damage, bool isBomb = false)
	{
		this.damagePlayersEvent.Fire(new DamagePlayersEventArg
		{
			Area = area,
			Damage = damage,
			IsBomb = isBomb
		});
	}

	private void performDamagePlayers(DamagePlayersEventArg arg)
	{
		if (Game1.player.currentLocation == this && (!arg.IsBomb || !Game1.player.hasBuff("dwarfStatue_3")))
		{
			int damage = arg.Damage;
			if (Game1.player.stats.Get("Book_Bombs") != 0)
			{
				damage = (int)((float)damage * 0.75f);
			}
			if (Game1.player.GetBoundingBox().Intersects(arg.Area) && !Game1.player.onBridge.Value)
			{
				Game1.player.takeDamage(damage, overrideParry: true, null);
			}
		}
	}

	public void explode(Vector2 tileLocation, int radius, Farmer who, bool damageFarmers = true, int damage_amount = -1, bool destroyObjects = true)
	{
		int insideCircle = 0;
		this.updateMap();
		Vector2 currentTile = new Vector2(Math.Min(this.map.Layers[0].LayerWidth - 1, Math.Max(0f, tileLocation.X - (float)radius)), Math.Min(this.map.Layers[0].LayerHeight - 1, Math.Max(0f, tileLocation.Y - (float)radius)));
		bool[,] circleOutline = Game1.getCircleOutlineGrid(radius);
		Microsoft.Xna.Framework.Rectangle areaOfEffect = new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - (float)radius) * 64, (int)(tileLocation.Y - (float)radius) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64);
		if (damage_amount > 0)
		{
			this.damageMonster(areaOfEffect, damage_amount, damage_amount, isBomb: true, who);
		}
		else
		{
			this.damageMonster(areaOfEffect, radius * 6, radius * 8, isBomb: true, who);
		}
		TemporaryAnimatedSpriteList sprites = new TemporaryAnimatedSpriteList
		{
			new TemporaryAnimatedSprite(23, 9999f, 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), flicker: false, Game1.random.NextBool())
			{
				lightId = $"{this.NameOrUniqueName}_{"explode"}_{tileLocation.X}_{tileLocation.Y}_{Game1.random.Next()}",
				lightRadius = radius,
				lightcolor = Color.Black,
				alphaFade = 0.03f - (float)radius * 0.003f,
				Parent = this
			}
		};
		this.rumbleAndFade(300 + radius * 100);
		if (damageFarmers)
		{
			int actualDamage = ((damage_amount > 0) ? damage_amount : (radius * 3));
			this.damagePlayers(areaOfEffect, actualDamage, isBomb: true);
		}
		for (int i = 0; i < radius * 2 + 1; i++)
		{
			for (int j = 0; j < radius * 2 + 1; j++)
			{
				if (i == 0 || j == 0 || i == radius * 2 || j == radius * 2)
				{
					insideCircle = (circleOutline[i, j] ? 1 : 0);
				}
				else if (circleOutline[i, j])
				{
					insideCircle += ((j <= radius) ? 1 : (-1));
					if (insideCircle <= 0)
					{
						if (destroyObjects)
						{
							if (this.objects.TryGetValue(currentTile, out var obj) && obj.onExplosion(who))
							{
								this.destroyObject(currentTile, who);
							}
							if (this.terrainFeatures.TryGetValue(currentTile, out var terrainFeature) && terrainFeature.performToolAction(null, radius / 2, currentTile))
							{
								this.terrainFeatures.Remove(currentTile);
							}
						}
						if (Game1.random.NextDouble() < 0.45)
						{
							if (Game1.random.NextBool())
							{
								sprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), flicker: false, Game1.random.NextBool())
								{
									delayBeforeAnimationStart = Game1.random.Next(700)
								});
							}
							else
							{
								sprites.Add(new TemporaryAnimatedSprite(5, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, flipped: false, 50f)
								{
									delayBeforeAnimationStart = Game1.random.Next(200),
									scale = (float)Game1.random.Next(5, 15) / 10f
								});
							}
						}
					}
				}
				if (insideCircle >= 1)
				{
					this.explosionAt(currentTile.X, currentTile.Y);
					if (destroyObjects)
					{
						if (this.objects.TryGetValue(currentTile, out var obj2) && obj2.onExplosion(who))
						{
							this.destroyObject(currentTile, who);
						}
						if (this.terrainFeatures.TryGetValue(currentTile, out var terrainFeature2) && terrainFeature2.performToolAction(null, radius / 2, currentTile))
						{
							this.terrainFeatures.Remove(currentTile);
						}
					}
					if (Game1.random.NextDouble() < 0.45)
					{
						if (Game1.random.NextBool())
						{
							sprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), flicker: false, Game1.random.NextBool())
							{
								delayBeforeAnimationStart = Game1.random.Next(700)
							});
						}
						else
						{
							sprites.Add(new TemporaryAnimatedSprite(5, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, flipped: false, 50f)
							{
								delayBeforeAnimationStart = Game1.random.Next(200),
								scale = (float)Game1.random.Next(5, 15) / 10f
							});
						}
					}
					sprites.Add(new TemporaryAnimatedSprite(6, new Vector2(currentTile.X * 64f, currentTile.Y * 64f), Color.White, 8, Game1.random.NextBool(), Vector2.Distance(currentTile, tileLocation) * 20f));
				}
				currentTile.Y += 1f;
				currentTile.Y = Math.Min(this.map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
			}
			currentTile.X += 1f;
			currentTile.Y = Math.Min(this.map.Layers[0].LayerWidth - 1, Math.Max(0f, currentTile.X));
			currentTile.Y = tileLocation.Y - (float)radius;
			currentTile.Y = Math.Min(this.map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
		}
		Game1.multiplayer.broadcastSprites(this, sprites);
		radius /= 2;
		circleOutline = Game1.getCircleOutlineGrid(radius);
		currentTile = new Vector2((int)(tileLocation.X - (float)radius), (int)(tileLocation.Y - (float)radius));
		insideCircle = 0;
		for (int k = 0; k < radius * 2 + 1; k++)
		{
			for (int l = 0; l < radius * 2 + 1; l++)
			{
				if (k == 0 || l == 0 || k == radius * 2 || l == radius * 2)
				{
					insideCircle = (circleOutline[k, l] ? 1 : 0);
				}
				else if (circleOutline[k, l])
				{
					insideCircle += ((l <= radius) ? 1 : (-1));
					if (insideCircle <= 0 && !this.objects.ContainsKey(currentTile) && Game1.random.NextDouble() < 0.9 && !this.isTileHoeDirt(currentTile) && this.makeHoeDirt(currentTile))
					{
						this.checkForBuriedItem((int)currentTile.X, (int)currentTile.Y, explosion: true, detectOnly: false, who);
					}
				}
				if (insideCircle >= 1 && !this.objects.ContainsKey(currentTile) && Game1.random.NextDouble() < 0.9 && !this.isTileHoeDirt(currentTile) && this.makeHoeDirt(currentTile))
				{
					this.checkForBuriedItem((int)currentTile.X, (int)currentTile.Y, explosion: true, detectOnly: false, who);
				}
				currentTile.Y += 1f;
				currentTile.Y = Math.Min(this.map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
			}
			currentTile.X += 1f;
			currentTile.Y = Math.Min(this.map.Layers[0].LayerWidth - 1, Math.Max(0f, currentTile.X));
			currentTile.Y = tileLocation.Y - (float)radius;
			currentTile.Y = Math.Min(this.map.Layers[0].LayerHeight - 1, Math.Max(0f, currentTile.Y));
		}
	}

	public virtual void explosionAt(float x, float y)
	{
	}

	public void removeTemporarySpritesWithID(int id)
	{
		this.removeTemporarySpritesWithIDEvent.Fire(id);
	}

	public void removeTemporarySpritesWithIDLocal(int id)
	{
		this.temporarySprites.RemoveWhere(delegate(TemporaryAnimatedSprite sprite)
		{
			if (sprite.id == id)
			{
				if (sprite.hasLit)
				{
					Utility.removeLightSource(sprite.lightId);
				}
				return true;
			}
			return false;
		});
	}

	/// <summary>Till a tile into a <see cref="T:StardewValley.TerrainFeatures.HoeDirt" /> if it's a valid diggable position, and there isn't already a tilled dirt there.</summary>
	/// <param name="tileLocation">The tile position to till.</param>
	/// <param name="ignoreChecks">Whether to till the tile even if it's occupied or non-diggable.</param>
	/// <returns>Returns whether a <see cref="T:StardewValley.TerrainFeatures.HoeDirt" /> instance was successfully added.</returns>
	public bool makeHoeDirt(Vector2 tileLocation, bool ignoreChecks = false)
	{
		if (ignoreChecks || (this.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null && !this.IsTileBlockedBy(tileLocation, ~(CollisionMask.Characters | CollisionMask.Farmers))))
		{
			MineShaft obj = this as MineShaft;
			if (obj == null || obj.getMineArea() != 77377)
			{
				return this.terrainFeatures.TryAdd(tileLocation, new HoeDirt((this.IsRainingHere() && this.isOutdoors.Value) ? 1 : 0, this));
			}
		}
		return false;
	}

	public int numberOfObjectsOfType(string itemId, bool bigCraftable)
	{
		int number = 0;
		string type = (bigCraftable ? "(BC)" : "(O)");
		foreach (Object obj in this.Objects.Values)
		{
			if (obj.HasTypeId(type) && obj.ItemId == itemId)
			{
				number++;
			}
		}
		return number;
	}

	public virtual void timeUpdate(int timeElapsed)
	{
		if (Game1.IsMasterGame)
		{
			foreach (FarmAnimal value in this.animals.Values)
			{
				value.updatePerTenMinutes(Game1.timeOfDay, this);
			}
		}
		foreach (Building b in this.buildings)
		{
			if (b.daysOfConstructionLeft.Value > 0)
			{
				continue;
			}
			b.performTenMinuteAction(timeElapsed);
			if (b.GetIndoorsType() != IndoorsType.Instanced)
			{
				continue;
			}
			GameLocation indoors = b.GetIndoors();
			if (indoors == null)
			{
				continue;
			}
			foreach (FarmAnimal value2 in indoors.animals.Values)
			{
				value2.updatePerTenMinutes(Game1.timeOfDay, indoors);
			}
			if (timeElapsed >= 10)
			{
				indoors.performTenMinuteUpdate(Game1.timeOfDay);
				if (timeElapsed > 10)
				{
					indoors.passTimeForObjects(timeElapsed - 10);
				}
			}
		}
	}

	/// <summary>Update all object when the time of day changes.</summary>
	/// <param name="timeElapsed">The number of minutes that passed.</param>
	public void passTimeForObjects(int timeElapsed)
	{
		this.objects.Lock();
		foreach (KeyValuePair<Vector2, Object> pair in this.objects.Pairs)
		{
			if (pair.Value.minutesElapsed(timeElapsed))
			{
				Vector2 key = pair.Key;
				this.objects.Remove(key);
			}
		}
		this.objects.Unlock();
	}

	public virtual void performTenMinuteUpdate(int timeOfDay)
	{
		for (int i = 0; i < this.furniture.Count; i++)
		{
			this.furniture[i].minutesElapsed(10);
		}
		for (int j = 0; j < this.characters.Count; j++)
		{
			NPC character = this.characters[j];
			if (!character.IsInvisible)
			{
				character.checkSchedule(timeOfDay);
				character.performTenMinuteUpdate(timeOfDay, this);
			}
		}
		this.passTimeForObjects(10);
		if (this.isOutdoors.Value)
		{
			Random r = Utility.CreateDaySaveRandom(timeOfDay, this.Map.Layers[0].LayerWidth);
			if (this.Equals(Game1.currentLocation))
			{
				this.tryToAddCritters(onlyIfOnScreen: true);
			}
			if (Game1.IsMasterGame)
			{
				int splashPointDurationSoFar = Utility.CalculateMinutesBetweenTimes(this.fishSplashPointTime, Game1.timeOfDay);
				bool frenzy = this.fishFrenzyFish.Value != null && !this.fishFrenzyFish.Value.Equals("");
				if (this.fishSplashPoint.Value.Equals(Point.Zero) && r.NextBool() && (!(this is Farm) || Game1.whichFarm == 1))
				{
					for (int tries = 0; tries < 2; tries++)
					{
						Point p = new Point(r.Next(0, this.map.RequireLayer("Back").LayerWidth), r.Next(0, this.map.RequireLayer("Back").LayerHeight));
						if (!this.isOpenWater(p.X, p.Y) || this.doesTileHaveProperty(p.X, p.Y, "NoFishing", "Back") != null)
						{
							continue;
						}
						int toLand = FishingRod.distanceToLand(p.X, p.Y, this);
						if (toLand <= 1 || toLand >= 5)
						{
							continue;
						}
						if (Game1.player.currentLocation.Equals(this))
						{
							this.playSound("waterSlosh");
						}
						if (r.NextDouble() < ((this is Beach) ? 0.008 : 0.01) && Game1.Date.TotalDays > 3 && (this is Town || this is Mountain || this is Forest || this is Beach) && Game1.timeOfDay < 2300 && (Game1.player.fishCaught.Count() > 2 || Game1.Date.TotalDays > 14) && !Utility.isFestivalDay())
						{
							Item f = this.getFish(r.Next(500), "", toLand, Game1.player, 0.0, Utility.PointToVector2(p));
							if (f.Category == -4 && !f.HasContextTag("fish_legendary"))
							{
								this.fishFrenzyFish.Value = f.QualifiedItemId;
								string locationName = ((this is Mountain) ? "mountain" : ((this is Forest) ? "forest" : ((!(this is Town)) ? "beach" : "town")));
								string tokenizedName = TokenStringBuilder.ItemNameFor(f);
								string tokenizedArticle = TokenStringBuilder.CapitalizeFirstLetter(TokenStringBuilder.ArticleFor(tokenizedName));
								Game1.multiplayer.broadcastGlobalMessage("Strings\\1_6_Strings:FishFrenzy_" + locationName, false, null, tokenizedName, tokenizedArticle);
							}
						}
						this.fishSplashPointTime = Game1.timeOfDay;
						this.fishSplashPoint.Value = p;
						break;
					}
				}
				else if (!this.fishSplashPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1 + (double)((float)splashPointDurationSoFar / 1800f) && splashPointDurationSoFar > (frenzy ? 120 : 60))
				{
					this.fishSplashPointTime = 0;
					this.fishFrenzyFish.Value = "";
					this.fishSplashPoint.Value = Point.Zero;
				}
				this.performOrePanTenMinuteUpdate(r);
			}
		}
		if (Game1.dayOfMonth % 7 == 0 && Game1.timeOfDay >= 1200 && Game1.timeOfDay <= 1500 && this.name.Equals("Saloon") && NetWorldState.checkAnywhereForWorldStateID("saloonSportsRoom"))
		{
			if (Game1.timeOfDay == 1500)
			{
				this.removeTemporarySpritesWithID(2400);
			}
			else
			{
				bool goodEvent = Game1.random.NextDouble() < 0.25;
				bool badEvent = Game1.random.NextDouble() < 0.25;
				List<NPC> sportsBoys = new List<NPC>();
				foreach (NPC n in this.characters)
				{
					if (n.TilePoint.Y < 12 && n.TilePoint.X > 26 && Game1.random.NextDouble() < ((goodEvent || badEvent) ? 0.66 : 0.25))
					{
						sportsBoys.Add(n);
					}
				}
				foreach (NPC n2 in sportsBoys)
				{
					n2.showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Saloon_" + (goodEvent ? "goodEvent" : (badEvent ? "badEvent" : "neutralEvent")) + "_" + Game1.random.Next(5)));
					if (goodEvent && Game1.random.NextDouble() < 0.55)
					{
						n2.jump();
					}
				}
			}
		}
		if (Game1.currentLocation.Equals(this) && this.name.Equals("BugLand") && Game1.random.NextDouble() <= 0.2)
		{
			this.characters.Add(new Fly(this.getRandomTile() * 64f, hard: true));
		}
	}

	public virtual bool performOrePanTenMinuteUpdate(Random r)
	{
		if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank") && !(this is Beach) && this.orePanPoint.Value.Equals(Point.Zero) && r.NextBool())
		{
			for (int tries = 0; tries < 8; tries++)
			{
				Point p = new Point(r.Next(0, this.Map.RequireLayer("Back").LayerWidth), r.Next(0, this.Map.RequireLayer("Back").LayerHeight));
				if (this.isOpenWater(p.X, p.Y) && FishingRod.distanceToLand(p.X, p.Y, this, landMustBeAdjacentToWalkableTile: true) <= 1 && !this.hasTileAt(p, "Buildings"))
				{
					if (Game1.player.currentLocation.Equals(this))
					{
						this.playSound("slosh");
					}
					this.orePanPoint.Value = p;
					return true;
				}
			}
		}
		else if (!this.orePanPoint.Value.Equals(Point.Zero) && r.NextDouble() < 0.1)
		{
			this.orePanPoint.Value = Point.Zero;
		}
		return false;
	}

	/// <summary>Get the fish types that can be caught by crab pots on a given tile.</summary>
	/// <param name="tile">The tile position containing the crab pot.</param>
	public virtual IList<string> GetCrabPotFishForTile(Vector2 tile)
	{
		if (this.catchOceanCrabPotFishFromThisSpot((int)tile.X, (int)tile.Y))
		{
			return GameLocation.OceanCrabPotFishTypes;
		}
		if (this.TryGetFishAreaForTile(tile, out var _, out var data))
		{
			List<string> crabPotFishTypes = data.CrabPotFishTypes;
			if (crabPotFishTypes != null && crabPotFishTypes.Count > 0)
			{
				return data.CrabPotFishTypes;
			}
		}
		return GameLocation.DefaultCrabPotFishTypes;
	}

	/// <summary>Get the fish area that applies to the given tile, if any.</summary>
	/// <param name="tile">The tile to check.</param>
	/// <param name="id">The fish area ID which applies, if any.</param>
	/// <param name="data">The fish area data which applies, if any.</param>
	public virtual bool TryGetFishAreaForTile(Vector2 tile, out string id, out FishAreaData data)
	{
		LocationData locationData = this.GetData();
		if (locationData?.FishAreas != null)
		{
			string defaultId = null;
			FishAreaData defaultArea = null;
			foreach (KeyValuePair<string, FishAreaData> pair in locationData.FishAreas)
			{
				FishAreaData area = pair.Value;
				bool? flag = area.Position?.Contains((int)tile.X, (int)tile.Y);
				if (flag.HasValue)
				{
					if (flag == true)
					{
						id = pair.Key;
						data = area;
						return true;
					}
				}
				else if (defaultId == null)
				{
					defaultId = pair.Key;
					defaultArea = pair.Value;
				}
			}
			if (defaultId != null)
			{
				id = defaultId;
				data = defaultArea;
				return true;
			}
		}
		id = null;
		data = null;
		return false;
	}

	/// <summary>Get the display name for a fishing area, if it has one.</summary>
	/// <param name="id">The fishing area ID, as returned by <see cref="M:StardewValley.GameLocation.TryGetFishAreaForTile(Microsoft.Xna.Framework.Vector2,System.String@,StardewValley.GameData.Locations.FishAreaData@)" />.</param>
	public virtual string GetFishingAreaDisplayName(string id)
	{
		LocationData data = this.GetData();
		if (data?.FishAreas == null || !data.FishAreas.TryGetValue(id, out var fishArea) || fishArea.DisplayName == null)
		{
			return null;
		}
		return TokenParser.ParseText(fishArea.DisplayName);
	}

	/// <summary>Get a random fish that can be caught in this location.</summary>
	/// <param name="millisecondsAfterNibble">The number of milliseconds after the fish starting biting before the player reacted and pressed the tool button.</param>
	/// <param name="bait">The qualified item ID for the bait attached to the fishing rod, if any.</param>
	/// <param name="waterDepth">The tile distance from the nearest shore.</param>
	/// <param name="who">The player who's fishing.</param>
	/// <param name="baitPotency">Unused.</param>
	/// <param name="bobberTile">The tile position where the fishing rod's bobber is floating.</param>
	/// <param name="locationName">The name of the location whose fish to get, or <c>null</c> for the current location.</param>
	public virtual Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
	{
		if (locationName != null && locationName != this.Name && (!(locationName == "UndergroundMine") || !(this is MineShaft)))
		{
			GameLocation location = Game1.getLocationFromName(locationName);
			if (location != null && location != this)
			{
				return location.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
			}
		}
		if (bobberTile != Vector2.Zero && who.currentLocation?.NameOrUniqueName == this.NameOrUniqueName)
		{
			foreach (Building building in this.buildings)
			{
				if (building is FishPond pond && pond.isTileFishable(bobberTile))
				{
					return pond.CatchFish();
				}
			}
		}
		if (this.fishFrenzyFish.Value != null && !this.fishFrenzyFish.Value.Equals("") && Vector2.Distance(bobberTile, Utility.PointToVector2(this.fishSplashPoint.Value)) <= 2f)
		{
			return ItemRegistry.Create(this.fishFrenzyFish.Value);
		}
		bool isTutorialCatch = who.fishCaught.Length == 0;
		return GameLocation.GetFishFromLocationData(this.Name, bobberTile, waterDepth, who, isTutorialCatch, isInherited: false, this) ?? ItemRegistry.Create("(O)168");
	}

	/// <summary>Get a random fish that can be caught for a given location based on its <c>Data\Locations</c> entry. This doesn't include global default fish and special cases; most code should call <see cref="M:StardewValley.GameLocation.getFish(System.Single,System.String,System.Int32,StardewValley.Farmer,System.Double,Microsoft.Xna.Framework.Vector2,System.String)" /> instead.</summary>
	/// <param name="locationName">The name of the location whose fish to get.</param>
	/// <param name="bobberTile">The tile position where the fishing rod's bobber is floating.</param>
	/// <param name="waterDepth">The tile distance from the nearest shore.</param>
	/// <param name="player">The player who's fishing.</param>
	/// <param name="isTutorialCatch">Whether this is the player's first catch, so it should be an easy fish for the tutorial.</param>
	/// <param name="isInherited">Whether we're loading fish indirectly (e.g. via the <c>LOCATION_FISH</c> item query), rather than for the actual location.</param>
	/// <param name="location">The location instance from which to get context data. If this is <c>null</c>, it'll be loaded based on the <paramref name="locationName" />; if that fails, generic context info (e.g. current location's weather) will be used instead.</param>
	/// <returns>Returns the fish to catch, or <c>null</c> if no match was found.</returns>
	public static Item GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location = null)
	{
		return GameLocation.GetFishFromLocationData(locationName, bobberTile, waterDepth, player, isTutorialCatch, isInherited, location, null);
	}

	/// <summary>Get a random fish that can be caught for a given location based on its <c>Data\Locations</c> entry. This doesn't include global default fish and special cases; most code should call <see cref="M:StardewValley.GameLocation.getFish(System.Single,System.String,System.Int32,StardewValley.Farmer,System.Double,Microsoft.Xna.Framework.Vector2,System.String)" /> instead.</summary>
	/// <param name="locationName">The name of the location whose fish to get.</param>
	/// <param name="bobberTile">The tile position where the fishing rod's bobber is floating.</param>
	/// <param name="waterDepth">The tile distance from the nearest shore.</param>
	/// <param name="player">The player who's fishing.</param>
	/// <param name="isTutorialCatch">Whether this is the player's first catch, so it should be an easy fish for the tutorial.</param>
	/// <param name="isInherited">Whether we're loading fish indirectly (e.g. via the <c>LOCATION_FISH</c> item query), rather than for the actual location.</param>
	/// <param name="location">The location instance from which to get context data. If this is <c>null</c>, it'll be loaded based on the <paramref name="locationName" />; if that fails, generic context info (e.g. current location's weather) will be used instead.</param>
	/// <param name="itemQueryContext">The context for the item query which led to this call, if applicable. This is used internally to prevent circular loops.</param>
	/// <returns>Returns the fish to catch, or <c>null</c> if no match was found.</returns>
	internal static Item GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location, ItemQueryContext itemQueryContext)
	{
		if (location == null)
		{
			location = Game1.getLocationFromName(locationName);
		}
		LocationData locationData = ((location != null) ? location.GetData() : GameLocation.GetData(locationName));
		Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
		Season season = Game1.GetSeasonForLocation(location);
		if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var fishAreaId, out var _))
		{
			fishAreaId = null;
		}
		bool usingMagicBait = false;
		bool hasCuriosityLure = false;
		string baitTargetFish = null;
		bool usingGoodBait = false;
		if (player?.CurrentTool is FishingRod { isFishing: not false } rod)
		{
			usingMagicBait = rod.HasMagicBait();
			hasCuriosityLure = rod.HasCuriosityLure();
			Object bait = rod.GetBait();
			if (bait != null)
			{
				if (bait.QualifiedItemId == "(O)SpecificBait" && bait.preservedParentSheetIndex.Value != null)
				{
					baitTargetFish = "(O)" + bait.preservedParentSheetIndex.Value;
				}
				if (bait.QualifiedItemId != "(O)685")
				{
					usingGoodBait = true;
				}
			}
		}
		Point playerTile = player.TilePoint;
		if (itemQueryContext == null)
		{
			itemQueryContext = new ItemQueryContext(location, null, Game1.random, "location '" + locationName + "' > fish data");
		}
		IEnumerable<SpawnFishData> possibleFish = Game1.locationData["Default"].Fish;
		if (locationData != null && locationData.Fish?.Count > 0)
		{
			possibleFish = possibleFish.Concat(locationData.Fish);
		}
		possibleFish = from p in possibleFish
			orderby p.Precedence, Game1.random.Next()
			select p;
		int targetedBaitTries = 0;
		HashSet<string> ignoreQueryKeys = (usingMagicBait ? GameStateQuery.MagicBaitIgnoreQueryKeys : null);
		Item firstNonTargetFish = null;
		for (int i = 0; i < 2; i++)
		{
			foreach (SpawnFishData spawn in possibleFish)
			{
				if ((isInherited && !spawn.CanBeInherited) || (spawn.FishAreaId != null && fishAreaId != spawn.FishAreaId) || (spawn.Season.HasValue && !usingMagicBait && spawn.Season != season))
				{
					continue;
				}
				Microsoft.Xna.Framework.Rectangle? playerPosition = spawn.PlayerPosition;
				if (playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains(playerTile.X, playerTile.Y))
				{
					continue;
				}
				playerPosition = spawn.BobberPosition;
				if ((playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y)) || player.FishingLevel < spawn.MinFishingLevel || waterDepth < spawn.MinDistanceFromShore || (spawn.MaxDistanceFromShore > -1 && waterDepth > spawn.MaxDistanceFromShore) || (spawn.RequireMagicBait && !usingMagicBait))
				{
					continue;
				}
				float chance = spawn.GetChance(hasCuriosityLure, player.DailyLuck, player.LuckLevel, (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), spawn.ItemId == baitTargetFish);
				if (spawn.UseFishCaughtSeededRandom)
				{
					if (!Utility.CreateRandom(Game1.uniqueIDForThisGame, player.stats.Get("PreciseFishCaught") * 859).NextBool(chance))
					{
						continue;
					}
				}
				else if (!Game1.random.NextBool(chance))
				{
					continue;
				}
				if (spawn.Condition != null && !GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, ignoreQueryKeys))
				{
					continue;
				}
				Item item = ItemQueryResolver.TryResolveRandomItem(spawn, itemQueryContext, avoidRepeat: false, null, (string query) => query.Replace("BOBBER_X", ((int)bobberTile.X).ToString()).Replace("BOBBER_Y", ((int)bobberTile.Y).ToString()).Replace("WATER_DEPTH", waterDepth.ToString()), null, delegate(string query, string error)
				{
					Game1.log.Error($"Location '{location.NameOrUniqueName}' failed parsing item query '{query}' for fish '{spawn.Id}': {error}");
				});
				if (item == null)
				{
					continue;
				}
				if (!string.IsNullOrWhiteSpace(spawn.SetFlagOnCatch))
				{
					item.SetFlagOnPickup = spawn.SetFlagOnCatch;
				}
				if (spawn.IsBossFish)
				{
					item.SetTempData("IsBossFish", value: true);
				}
				Item fish = item;
				if ((spawn.CatchLimit <= -1 || !player.fishCaught.TryGetValue(fish.QualifiedItemId, out var values) || values[0] < spawn.CatchLimit) && GameLocation.CheckGenericFishRequirements(fish, allFishData, location, player, spawn, waterDepth, usingMagicBait, hasCuriosityLure, spawn.ItemId == baitTargetFish, isTutorialCatch))
				{
					if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= 2)
					{
						return fish;
					}
					if (firstNonTargetFish == null)
					{
						firstNonTargetFish = fish;
					}
					targetedBaitTries++;
				}
			}
			if (!usingGoodBait)
			{
				i++;
			}
		}
		if (firstNonTargetFish != null)
		{
			return firstNonTargetFish;
		}
		if (!isTutorialCatch)
		{
			return null;
		}
		return ItemRegistry.Create("(O)145");
	}

	/// <summary>Get whether a fish can be spawned based on its requirements in Data/Fish, if applicable.</summary>
	/// <param name="fish">The fish being checked.</param>
	/// <param name="allFishData">The Data/Fish data to check.</param>
	/// <param name="location">The location for which fish are being caught.</param>
	/// <param name="player">The player catching fish.</param>
	/// <param name="spawn">The fish spawn rule for which a fish is being checked.</param>
	/// <param name="waterDepth">The current water depth for the fishing bobber.</param>
	/// <param name="usingMagicBait">Whether the player has the magic bait equipped.</param>
	/// <param name="hasCuriosityLure">Whether the player has the curiosity lure equipped.</param>
	/// <param name="usingTargetBait">Whether the player has the target bait equipped.</param>
	/// <param name="isTutorialCatch">Whether this is the player's first catch, so it should be an easy fish for the tutorial.</param>
	internal static bool CheckGenericFishRequirements(Item fish, Dictionary<string, string> allFishData, GameLocation location, Farmer player, SpawnFishData spawn, int waterDepth, bool usingMagicBait, bool hasCuriosityLure, bool usingTargetBait, bool isTutorialCatch)
	{
		if (!fish.HasTypeObject() || !allFishData.TryGetValue(fish.ItemId, out var rawSpecificFishData))
		{
			return !isTutorialCatch;
		}
		string[] specificFishData = rawSpecificFishData.Split('/');
		if (ArgUtility.Get(specificFishData, 1) == "trap")
		{
			return !isTutorialCatch;
		}
		bool isTrainingRod = player?.CurrentTool?.QualifiedItemId == "(T)TrainingRod";
		if (isTrainingRod)
		{
			bool? canUseTrainingRod = spawn.CanUseTrainingRod;
			if (canUseTrainingRod.HasValue)
			{
				if (canUseTrainingRod != true)
				{
					return false;
				}
			}
			else
			{
				if (!ArgUtility.TryGetInt(specificFishData, 1, out var difficulty, out var error, "int difficulty"))
				{
					return LogFormatError(error);
				}
				if (difficulty >= 50)
				{
					return false;
				}
			}
		}
		if (isTutorialCatch)
		{
			if (!ArgUtility.TryGetOptionalBool(specificFishData, 13, out var isTutorialFish, out var error2, defaultValue: false, "bool isTutorialFish"))
			{
				return LogFormatError(error2);
			}
			if (!isTutorialFish)
			{
				return false;
			}
		}
		if (!spawn.IgnoreFishDataRequirements)
		{
			if (!usingMagicBait)
			{
				if (!ArgUtility.TryGet(specificFishData, 5, out var rawTimeSpans, out var error3, allowBlank: true, "string rawTimeSpans"))
				{
					return LogFormatError(error3);
				}
				string[] timeSpans = ArgUtility.SplitBySpace(rawTimeSpans);
				bool found = false;
				for (int i = 0; i < timeSpans.Length; i += 2)
				{
					if (!ArgUtility.TryGetInt(timeSpans, i, out var startTime, out error3, "int startTime") || !ArgUtility.TryGetInt(timeSpans, i + 1, out var endTime, out error3, "int endTime"))
					{
						return LogFormatError("invalid time spans '" + rawTimeSpans + "': " + error3);
					}
					if (Game1.timeOfDay >= startTime && Game1.timeOfDay < endTime)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					return false;
				}
			}
			if (!usingMagicBait)
			{
				if (!ArgUtility.TryGet(specificFishData, 7, out var weather, out var error4, allowBlank: true, "string weather"))
				{
					return LogFormatError(error4);
				}
				if (!(weather == "rainy"))
				{
					if (weather == "sunny" && location.IsRainingHere())
					{
						return false;
					}
				}
				else if (!location.IsRainingHere())
				{
					return false;
				}
			}
			if (!ArgUtility.TryGetInt(specificFishData, 12, out var minFishingLevel, out var error5, "int minFishingLevel"))
			{
				return LogFormatError(error5);
			}
			if (player.FishingLevel < minFishingLevel)
			{
				return false;
			}
			if (!ArgUtility.TryGetInt(specificFishData, 9, out var maxDepth, out var error6, "int maxDepth") || !ArgUtility.TryGetFloat(specificFishData, 10, out var chance, out error6, "float chance") || !ArgUtility.TryGetFloat(specificFishData, 11, out var depthMultiplier, out error6, "float depthMultiplier"))
			{
				return LogFormatError(error6);
			}
			float dropOffAmount = depthMultiplier * chance;
			chance -= (float)Math.Max(0, maxDepth - waterDepth) * dropOffAmount;
			chance += (float)player.FishingLevel / 50f;
			if (isTrainingRod)
			{
				chance *= 1.1f;
			}
			chance = Math.Min(chance, 0.9f);
			if ((double)chance < 0.25 && hasCuriosityLure)
			{
				if (spawn.CuriosityLureBuff > -1f)
				{
					chance += spawn.CuriosityLureBuff;
				}
				else
				{
					float max = 0.25f;
					float min = 0.08f;
					chance = (max - min) / max * chance + (max - min) / 2f;
				}
			}
			if (usingTargetBait)
			{
				chance *= 1.66f;
			}
			if (spawn.ApplyDailyLuck)
			{
				chance += (float)player.DailyLuck;
			}
			List<QuantityModifier> chanceModifiers = spawn.ChanceModifiers;
			if (chanceModifiers != null && chanceModifiers.Count > 0)
			{
				chance = Utility.ApplyQuantityModifiers(chance, spawn.ChanceModifiers, spawn.ChanceModifierMode, location);
			}
			if (!Game1.random.NextBool(chance))
			{
				return false;
			}
		}
		return true;
		bool LogFormatError(string text)
		{
			Game1.log.Warn("Skipped fish '" + fish.ItemId + "' due to invalid requirements in Data/Fish: " + text);
			return false;
		}
	}

	public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
	{
		foreach (Building building in this.buildings)
		{
			if (building.isActionableTile(xTile, yTile, who))
			{
				return true;
			}
		}
		bool isActionable = false;
		string[] action = ArgUtility.SplitBySpace(this.doesTileHaveProperty(xTile, yTile, "Action", "Buildings"));
		if (!this.ShouldIgnoreAction(action, who, new Location(xTile, yTile)))
		{
			switch (action[0])
			{
			case "Dialogue":
			case "Message":
			case "MessageOnce":
			case "NPCMessage":
				isActionable = true;
				Game1.isInspectionAtCurrentCursorTile = true;
				break;
			case "MessageSpeech":
				isActionable = true;
				Game1.isSpeechAtCurrentCursorTile = true;
				break;
			default:
				isActionable = true;
				break;
			}
		}
		if (!isActionable)
		{
			if (this.objects.TryGetValue(new Vector2(xTile, yTile), out var obj) && obj.isActionable(who))
			{
				isActionable = true;
			}
			if (!Game1.isFestival() && this.terrainFeatures.TryGetValue(new Vector2(xTile, yTile), out var terrainFeature) && terrainFeature.isActionable())
			{
				isActionable = true;
			}
		}
		if (isActionable && !Utility.tileWithinRadiusOfPlayer(xTile, yTile, 1, who))
		{
			Game1.mouseCursorTransparency = 0.5f;
		}
		return isActionable;
	}

	public Item tryGetRandomArtifactFromThisLocation(Farmer who, Random r, double chanceMultipler = 1.0)
	{
		LocationData locationData = this.GetData();
		ItemQueryContext itemQueryContext = new ItemQueryContext(this, who, r, "location '" + this.NameOrUniqueName + "' > artifact spots");
		IEnumerable<ArtifactSpotDropData> possibleDrops = Game1.locationData["Default"].ArtifactSpots;
		if (locationData != null && locationData.ArtifactSpots?.Count > 0)
		{
			possibleDrops = possibleDrops.Concat(locationData.ArtifactSpots);
		}
		possibleDrops = possibleDrops.OrderBy((ArtifactSpotDropData p) => p.Precedence);
		foreach (ArtifactSpotDropData drop in possibleDrops)
		{
			if (r.NextBool(drop.Chance * chanceMultipler) && (drop.Condition == null || GameStateQuery.CheckConditions(drop.Condition, this, who, null, null, r)))
			{
				Item item = ItemQueryResolver.TryResolveRandomItem(drop, itemQueryContext, avoidRepeat: false, null, null, null, delegate(string query, string error)
				{
					Game1.log.Error($"Location '{this.NameOrUniqueName}' failed parsing item query '{query}' for artifact spot '{drop.Id}': {error}");
				});
				if (item != null)
				{
					return item;
				}
			}
		}
		return null;
	}

	public virtual void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
	{
		Random r = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation, Game1.netWorldState.Value.TreasureTotemsUsed * 777);
		Vector2 tilePixelPos = new Vector2(xLocation * 64, yLocation * 64);
		bool hasGenerousEnchantment = (who?.CurrentTool as Hoe)?.hasEnchantmentOfType<GenerousEnchantment>() ?? false;
		LocationData locationData = this.GetData();
		ItemQueryContext itemQueryContext = new ItemQueryContext(this, who, r, "location '" + this.NameOrUniqueName + "' > artifact spots");
		IEnumerable<ArtifactSpotDropData> possibleDrops = Game1.locationData["Default"].ArtifactSpots;
		if (locationData != null && locationData.ArtifactSpots?.Count > 0)
		{
			possibleDrops = possibleDrops.Concat(locationData.ArtifactSpots);
		}
		possibleDrops = possibleDrops.OrderBy((ArtifactSpotDropData p) => p.Precedence);
		if (Game1.player.mailReceived.Contains("sawQiPlane") && r.NextDouble() < 0.05 + Game1.player.team.AverageDailyLuck() / 2.0)
		{
			Game1.createMultipleItemDebris(ItemRegistry.Create("(O)MysteryBox", r.Next(1, 3)), tilePixelPos, -1, this);
		}
		Utility.trySpawnRareObject(who, tilePixelPos, this, 9.0, 1.0, -1, r);
		foreach (ArtifactSpotDropData drop in possibleDrops)
		{
			if (!r.NextBool(drop.Chance) || (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, this, who, null, null, r)))
			{
				continue;
			}
			Item item = ItemQueryResolver.TryResolveRandomItem(drop, itemQueryContext, avoidRepeat: false, null, null, null, delegate(string query, string error)
			{
				Game1.log.Error($"Location '{this.NameOrUniqueName}' failed parsing item query '{query}' for artifact spot '{drop.Id}': {error}");
			});
			if (item == null)
			{
				continue;
			}
			if (drop.OneDebrisPerDrop && item.Stack > 1)
			{
				Game1.createMultipleItemDebris(item, tilePixelPos, -1, this);
			}
			else
			{
				Game1.createItemDebris(item, tilePixelPos, Game1.random.Next(4), this);
			}
			if (hasGenerousEnchantment && drop.ApplyGenerousEnchantment && r.NextBool())
			{
				item = item.getOne();
				item = (Item)ItemQueryResolver.ApplyItemFields(item, drop, itemQueryContext);
				if (drop.OneDebrisPerDrop && item.Stack > 1)
				{
					Game1.createMultipleItemDebris(item, tilePixelPos, -1, this);
				}
				else
				{
					Game1.createItemDebris(item, tilePixelPos, -1, this);
				}
			}
			if (!drop.ContinueOnDrop)
			{
				break;
			}
		}
	}

	/// <summary>Get the underlying data from <c>Data/Locations</c> for this location, if available.</summary>
	/// <remarks>If this is a passive festival location and doesn't have its own data, this will return the data matching its <see cref="F:StardewValley.GameData.PassiveFestivalData.MapReplacements" /> field.</remarks>
	public LocationData GetData()
	{
		string name = this.Name;
		if (!(this is MineShaft))
		{
			if (this is Cellar && name.StartsWith("Cellar"))
			{
				name = "Cellar";
			}
		}
		else
		{
			name = "UndergroundMine";
		}
		return GameLocation.GetData(name);
	}

	/// <summary>Get the underlying data from <c>Data/Locations</c> for this location, if available.</summary>
	/// <param name="name">The location name to match.</param>
	/// <remarks>If this is a passive festival location and doesn't have its own data, this will return the data matching its <see cref="F:StardewValley.GameData.PassiveFestivalData.MapReplacements" /> field.</remarks>
	public static LocationData GetData(string name)
	{
		IDictionary<string, LocationData> rawData = Game1.locationData;
		if (name == "Farm")
		{
			return GetImpl("Farm_" + Game1.GetFarmTypeKey()) ?? GetImpl("Farm_Standard");
		}
		return GetImpl(name);
		LocationData GetImpl(string entryName)
		{
			if (rawData.TryGetValue(entryName, out var data))
			{
				return data;
			}
			foreach (string activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
			{
				if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var festivalData) && festivalData.MapReplacements != null)
				{
					foreach (KeyValuePair<string, string> replacement in festivalData.MapReplacements)
					{
						if (replacement.Value == entryName)
						{
							if (!rawData.TryGetValue(replacement.Key, out data))
							{
								break;
							}
							return data;
						}
					}
				}
			}
			return null;
		}
	}

	/// <summary>Get whether NPCs should ignore this location when pathfinding between locations.</summary>
	public virtual bool ShouldExcludeFromNpcPathfinding()
	{
		return this.GetData()?.ExcludeFromNpcPathfinding ?? false;
	}

	public virtual string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
	{
		Random r = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation * 77, Game1.stats.DirtHoed);
		string treasureType = this.HandleTreasureTileProperty(xLocation, yLocation, detectOnly);
		if (treasureType != null)
		{
			return treasureType;
		}
		bool generousEnchant = who?.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
		float generousChance = 0.5f;
		if (!this.isFarm.Value && this.isOutdoors.Value && this.GetSeason() == Season.Winter && r.NextDouble() < 0.08 && !explosion && !detectOnly && !(this is Desert))
		{
			Game1.createObjectDebris(r.Choose("(O)412", "(O)416"), xLocation, yLocation);
			if (generousEnchant && r.NextDouble() < (double)generousChance)
			{
				Game1.createObjectDebris(r.Choose("(O)412", "(O)416"), xLocation, yLocation);
			}
			return "";
		}
		LocationData data = this.GetData();
		if (this.isOutdoors.Value && r.NextBool(data?.ChanceForClay ?? 0.03) && !explosion)
		{
			if (detectOnly)
			{
				this.map.RequireLayer("Back").Tiles[xLocation, yLocation].Properties.Add("Treasure", "Item (O)330");
				return "Item";
			}
			Game1.createObjectDebris("(O)330", xLocation, yLocation);
			if (generousEnchant && r.NextDouble() < (double)generousChance)
			{
				Game1.createObjectDebris("(O)330", xLocation, yLocation);
			}
			return "";
		}
		return "";
	}

	private string HandleTreasureTileProperty(int xLocation, int yLocation, bool detectOnly)
	{
		string property = this.doesTileHaveProperty(xLocation, yLocation, "Treasure", "Back");
		if (property == null)
		{
			return null;
		}
		string[] fields = ArgUtility.SplitBySpace(property);
		if (!ArgUtility.TryGet(fields, 0, out var type, out var error, allowBlank: true, "string type"))
		{
			LogError(property, error);
			return null;
		}
		if (detectOnly)
		{
			return type;
		}
		switch (type)
		{
		case "Arch":
		{
			if (ArgUtility.TryGet(fields, 1, out var itemId2, out error, allowBlank: true, "string itemId"))
			{
				Game1.createObjectDebris(itemId2, xLocation, yLocation);
			}
			else
			{
				LogError(property, error);
			}
			break;
		}
		case "CaveCarrot":
			Game1.createObjectDebris("(O)78", xLocation, yLocation);
			break;
		case "Coins":
			Game1.createObjectDebris("(O)330", xLocation, yLocation);
			break;
		case "Coal":
		case "Gold":
		case "Iron":
		case "Copper":
		case "Iridium":
		{
			int debris = type switch
			{
				"Coal" => 4, 
				"Copper" => 0, 
				"Gold" => 6, 
				"Iridium" => 10, 
				_ => 2, 
			};
			if (ArgUtility.TryGetInt(fields, 1, out var itemId4, out error, "int itemId"))
			{
				Game1.createDebris(debris, xLocation, yLocation, itemId4);
			}
			else
			{
				LogError(property, error);
			}
			break;
		}
		case "Object":
		{
			if (ArgUtility.TryGet(fields, 1, out var itemId3, out error, allowBlank: true, "string itemId"))
			{
				Game1.createObjectDebris(itemId3, xLocation, yLocation);
				if (itemId3 == "78" || itemId3 == "(O)79")
				{
					Game1.stats.CaveCarrotsFound++;
				}
			}
			else
			{
				LogError(property, error);
			}
			break;
		}
		case "Item":
		{
			if (ArgUtility.TryGet(fields, 1, out var itemId, out error, allowBlank: true, "string itemId"))
			{
				Item item = ItemRegistry.Create(itemId);
				Game1.createItemDebris(item, new Vector2(xLocation, yLocation), -1, this);
				if (item.QualifiedItemId == "(O)78")
				{
					Game1.stats.CaveCarrotsFound++;
				}
			}
			else
			{
				LogError(property, error);
			}
			break;
		}
		default:
			type = null;
			LogError(property, "invalid treasure type '" + type + "'");
			break;
		}
		this.map.RequireLayer("Back").Tiles[xLocation, yLocation].Properties["Treasure"] = null;
		return type;
		void LogError(string value, string errorPhrase)
		{
			this.LogTilePropertyError("Treasure", "Back", xLocation, yLocation, value, errorPhrase);
		}
	}

	public virtual bool AllowMapModificationsInResetState()
	{
		return false;
	}

	/// <summary>Remove a tile from the location's map.</summary>
	/// <param name="tileX">The X tile position to set.</param>
	/// <param name="tileY">The Y tile position to set.</param>
	/// <param name="layer">The layer whose tile to set.</param>
	public void removeMapTile(int tileX, int tileY, string layer)
	{
		Layer mapLayer = this.map?.RequireLayer(layer);
		if (mapLayer?.Tiles[tileX, tileY] != null)
		{
			mapLayer.Tiles[tileX, tileY] = null;
		}
	}

	/// <summary>Change the tile at a given map position to use the given tilesheet and tile index, recreating it if needed.</summary>
	/// <param name="tileX">The X tile position to set.</param>
	/// <param name="tileY">The Y tile position to set.</param>
	/// <param name="index">The tile index in the tilesheet to show.</param>
	/// <param name="layer">The layer whose tile to set.</param>
	/// <param name="tileSheetId">The tilesheet ID from which to get the <paramref name="index" />.</param>
	/// <param name="action">The <c>Action</c> tile property to set, or <c>null</c> for none. This is ignored if <paramref name="layer" /> is not <c>Buildings</c>.</param>
	/// <param name="copyProperties">If the tile is recreated, whether to copy any tile properties that were on the previous tile.</param>
	/// <returns>Returns the new or updated tile at the tile position.</returns>
	public StaticTile setMapTile(int tileX, int tileY, int index, string layer, string tileSheetId, string action = null, bool copyProperties = true)
	{
		Layer mapLayer = this.map.RequireLayer(layer);
		Tile oldTile = mapLayer.Tiles[tileX, tileY];
		StaticTile tile = oldTile as StaticTile;
		if (tile != null && tile.TileSheet.Id == tileSheetId)
		{
			tile.TileIndex = index;
		}
		else
		{
			tile = (StaticTile)(mapLayer.Tiles[tileX, tileY] = new StaticTile(mapLayer, this.map.RequireTileSheet(tileSheetId), BlendMode.Alpha, index));
			if (copyProperties && oldTile != null)
			{
				foreach (KeyValuePair<string, PropertyValue> property in oldTile.Properties)
				{
					tile.Properties[property.Key] = property.Value;
				}
			}
		}
		if (action != null && layer == "Buildings")
		{
			tile.Properties["Action"] = action;
		}
		return tile;
	}

	/// <summary>Replace a map tile with an animated tile.</summary>
	/// <param name="tileX">The X tile position to set.</param>
	/// <param name="tileY">The Y tile position to set.</param>
	/// <param name="animationTileIndexes">The tile index in the tilesheet to show for each frame in the animation.</param>
	/// <param name="interval">The number of milliseconds for which to show each frame of the animation.</param>
	/// <param name="layer">The layer whose tile to set.</param>
	/// <param name="tileSheetId">The tilesheet ID from which to get the <paramref name="animationTileIndexes" />.</param>
	/// <param name="action">The <c>Action</c> tile property to set, or <c>null</c> for none. This is ignored if <paramref name="layer" /> is not <c>Buildings</c>.</param>
	/// <param name="copyProperties">Whether to copy any tile properties that were on the previous tile.</param>
	/// <returns>Returns the new tile at the tile position.</returns>
	public AnimatedTile setAnimatedMapTile(int tileX, int tileY, int[] animationTileIndexes, long interval, string layer, string tileSheetId, string action = null, bool copyProperties = true)
	{
		Layer mapLayer = this.map.RequireLayer(layer);
		TileSheet tileSheet = this.map.RequireTileSheet(tileSheetId);
		StaticTile[] tiles = new StaticTile[animationTileIndexes.Length];
		for (int i = 0; i < animationTileIndexes.Length; i++)
		{
			tiles[i] = new StaticTile(mapLayer, tileSheet, BlendMode.Alpha, animationTileIndexes[i]);
		}
		AnimatedTile tile = new AnimatedTile(mapLayer, tiles, interval);
		if (copyProperties)
		{
			Tile oldTile = mapLayer.Tiles[tileX, tileY];
			if (oldTile != null)
			{
				foreach (KeyValuePair<string, PropertyValue> property in oldTile.Properties)
				{
					tile.Properties[property.Key] = property.Value;
				}
			}
		}
		if (action != null && layer == "Buildings")
		{
			tile.Properties["Action"] = action;
		}
		mapLayer.Tiles[tileX, tileY] = tile;
		return tile;
	}

	/// <summary>Move all objects, furniture, terrain features, and large terrain features within the location.</summary>
	/// <param name="dx">The X tile offset to apply.</param>
	/// <param name="dy">The Y tile offset to apply.</param>
	/// <param name="where">If set, a filter which indicates whether something should be moved.</param>
	public virtual void shiftContents(int dx, int dy, Func<Vector2, object, bool> where = null)
	{
		Vector2 offset = new Vector2(dx, dy);
		List<KeyValuePair<Vector2, Object>> list = new List<KeyValuePair<Vector2, Object>>(this.objects.Pairs);
		this.objects.Clear();
		foreach (KeyValuePair<Vector2, Object> v in list)
		{
			if (where == null || where(v.Key, v.Value))
			{
				this.removeLightSource(v.Value.lightSource?.Id);
				Vector2 tile = v.Key + offset;
				this.objects.Add(tile, v.Value);
				v.Value.initializeLightSource(tile);
			}
			else
			{
				this.objects.Add(v.Key, v.Value);
			}
		}
		List<KeyValuePair<Vector2, TerrainFeature>> list2 = new List<KeyValuePair<Vector2, TerrainFeature>>(this.terrainFeatures.Pairs);
		this.terrainFeatures.Clear();
		foreach (KeyValuePair<Vector2, TerrainFeature> v2 in list2)
		{
			Vector2 tile2 = ((where == null || where(v2.Key, v2.Value)) ? (v2.Key + offset) : v2.Key);
			this.terrainFeatures.Add(tile2, v2.Value);
		}
		foreach (LargeTerrainFeature v3 in this.largeTerrainFeatures)
		{
			if (where == null || where(v3.Tile, v3))
			{
				v3.Tile += offset;
			}
		}
		foreach (Furniture v4 in this.furniture)
		{
			if (where == null || where(v4.TileLocation, v4))
			{
				v4.removeLights();
				v4.TileLocation = new Vector2(v4.TileLocation.X + (float)dx, v4.TileLocation.Y + (float)dy);
				v4.updateDrawPosition();
				if (Game1.isDarkOut(this))
				{
					v4.addLights();
				}
			}
		}
	}

	public void moveFurniture(int oldX, int oldY, int newX, int newY)
	{
		Vector2 oldSpot = new Vector2(oldX, oldY);
		foreach (Furniture f in this.furniture)
		{
			if (f.tileLocation.Equals(oldSpot))
			{
				f.removeLights();
				f.TileLocation = new Vector2(newX, newY);
				if (Game1.isDarkOut(this))
				{
					f.addLights();
				}
				return;
			}
		}
		if (this.objects.TryGetValue(oldSpot, out var o))
		{
			this.objects.Remove(oldSpot);
			this.objects.Add(new Vector2(newX, newY), o);
		}
	}

	/// <summary>Get whether a tile exists at the given coordinate.</summary>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID to check, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	public bool hasTileAt(int x, int y, string layer, string tilesheetId = null)
	{
		return this.map?.HasTileAt(x, y, layer, tilesheetId) ?? false;
	}

	/// <summary>Get whether a tile exists at the given coordinate.</summary>
	/// <param name="tile">The tile coordinate.</param>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID to check, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	public bool hasTileAt(Location tile, string layer, string tilesheetId = null)
	{
		return this.map?.HasTileAt(tile.X, tile.Y, layer, tilesheetId) ?? false;
	}

	/// <summary>Get whether a tile exists at the given coordinate.</summary>
	/// <param name="tile">The tile coordinate.</param>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID to check, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	public bool hasTileAt(Point tile, string layer, string tilesheetId = null)
	{
		return this.map?.HasTileAt(tile.X, tile.Y, layer, tilesheetId) ?? false;
	}

	/// <summary>Get the tile index at the given map coordinate.</summary>
	/// <param name="p">The tile coordinate.</param>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public int getTileIndexAt(Location p, string layer, string tilesheetId = null)
	{
		return this.map?.GetTileIndexAt(p.X, p.Y, layer, tilesheetId) ?? (-1);
	}

	/// <summary>Get the tile index at the given map coordinate.</summary>
	/// <param name="p">The tile coordinate.</param>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public int getTileIndexAt(Point p, string layer, string tilesheetId = null)
	{
		return this.map?.GetTileIndexAt(p.X, p.Y, layer, tilesheetId) ?? (-1);
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public int getTileIndexAt(int x, int y, string layer, string tilesheetId = null)
	{
		return this.map?.GetTileIndexAt(x, y, layer, tilesheetId) ?? (-1);
	}

	public string getTileSheetIDAt(int x, int y, string layer)
	{
		return this.map.GetLayer(layer)?.Tiles[x, y]?.TileSheet.Id ?? "";
	}

	/// <summary>Handle a building in this location being constructed by any player.</summary>
	/// <param name="building">The building that was constructed.</param>
	/// <param name="who">The player that constructed the building.</param>
	public virtual void OnBuildingConstructed(Building building, Farmer who)
	{
		building.performActionOnConstruction(this, who);
	}

	/// <summary>Handle a building in this location being moved by any player.</summary>
	/// <param name="building">The building that was moved.</param>
	public virtual void OnBuildingMoved(Building building)
	{
		building.performActionOnBuildingPlacement();
	}

	/// <summary>Handle a building in this location being demolished by the current player.</summary>
	/// <param name="building">The building type that was demolished.</param>
	/// <param name="id">The unique building ID.</param>
	public virtual void OnBuildingDemolished(string type, Guid id)
	{
		if (type == "Stable")
		{
			Horse mount = Game1.player.mount;
			if (mount != null && mount.HorseId == id)
			{
				Game1.player.mount.dismount(from_demolish: true);
			}
		}
	}

	/// <summary>Handle the new day starting after the player saves, loads, or connects.</summary>
	/// <remarks>See also <see cref="M:StardewValley.GameLocation.DayUpdate(System.Int32)" />, which happens while setting up the day before saving.</remarks>
	public virtual void OnDayStarted()
	{
	}

	/// <summary>Handle a breakable mine stone being destroyed.</summary>
	/// <param name="stoneId">The unqualified item ID for the stone object.</param>
	/// <param name="x">The stone's X tile position.</param>
	/// <param name="y">The stone's Y tile position.</param>
	/// <param name="who">The player who broke the stone.</param>
	/// <remarks>This is the entry point for creating item drops when breaking stone.</remarks>
	public void OnStoneDestroyed(string stoneId, int x, int y, Farmer who)
	{
		long farmerId = who?.UniqueMultiplayerID ?? 0;
		if (who?.currentLocation is MineShaft { mineLevel: >120 } mine && !mine.isSideBranch())
		{
			int floor = mine.mineLevel - 121;
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
			{
				float chance = 0.01f;
				chance += (float)floor * 0.0005f;
				if (chance > 0.5f)
				{
					chance = 0.5f;
				}
				if (Game1.random.NextBool(chance))
				{
					Game1.createMultipleObjectDebris("CalicoEgg", x, y, Game1.random.Next(1, 4), who.UniqueMultiplayerID, this);
				}
			}
		}
		if (who != null && Game1.random.NextDouble() <= 0.02 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
		{
			Game1.createMultipleObjectDebris("(O)890", x, y, 1, who.UniqueMultiplayerID, this);
		}
		if (!MineShaft.IsGeneratedLevel(this))
		{
			if (stoneId == "343" || stoneId == "450")
			{
				Random r = Utility.CreateDaySaveRandom(x * 2000, y);
				double geodeChanceMultiplier = ((who != null && who.hasBuff("dwarfStatue_4")) ? 1.25 : 1.0);
				if (r.NextDouble() < 0.035 * geodeChanceMultiplier && Game1.stats.DaysPlayed > 1)
				{
					Game1.createObjectDebris("(O)" + (535 + ((Game1.stats.DaysPlayed > 60 && r.NextDouble() < 0.2) ? 1 : ((Game1.stats.DaysPlayed > 120 && r.NextDouble() < 0.2) ? 2 : 0))), x, y, farmerId, this);
				}
				int burrowerMultiplier = ((who == null || !who.professions.Contains(21)) ? 1 : 2);
				double addedCoalChance = ((who != null && who.hasBuff("dwarfStatue_2")) ? 0.03 : 0.0);
				if (r.NextDouble() < 0.035 * (double)burrowerMultiplier + addedCoalChance && Game1.stats.DaysPlayed > 1)
				{
					Game1.createObjectDebris("(O)382", x, y, farmerId, this);
				}
				if (r.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1)
				{
					Game1.createObjectDebris("(O)390", x, y, farmerId, this);
				}
			}
			this.breakStone(stoneId, x, y, who, Utility.CreateDaySaveRandom(x * 4000, y));
		}
		else
		{
			(this as MineShaft).checkStoneForItems(stoneId, x, y, who);
		}
	}

	protected virtual bool breakStone(string stoneId, int x, int y, Farmer who, Random r)
	{
		int experience = 0;
		int addedOres = ((who != null && who.professions.Contains(18)) ? 1 : 0);
		if (who != null && who.hasBuff("dwarfStatue_0"))
		{
			addedOres++;
		}
		if (stoneId == 44.ToString())
		{
			stoneId = (r.Next(1, 8) * 2).ToString();
		}
		long farmerId = who?.UniqueMultiplayerID ?? 0;
		int farmerLuckLevel = who?.LuckLevel ?? 0;
		double farmerDailyLuck = who?.DailyLuck ?? 0.0;
		int farmerMiningLevel = who?.MiningLevel ?? 0;
		switch (stoneId)
		{
		case "95":
			Game1.createMultipleObjectDebris("(O)909", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 200f)) ? 1 : 0), farmerId, this);
			experience = 18;
			break;
		case "843":
		case "844":
			Game1.createMultipleObjectDebris("(O)848", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 200f)) ? 1 : 0), farmerId, this);
			experience = 12;
			break;
		case "25":
			Game1.createMultipleObjectDebris("(O)719", x, y, r.Next(2, 5), farmerId, this);
			experience = 5;
			if (this is IslandLocation && r.NextDouble() < 0.1)
			{
				Game1.player.team.RequestLimitedNutDrops("MusselStone", this, x * 64, y * 64, 5);
			}
			break;
		case "75":
			Game1.createObjectDebris("(O)535", x, y, farmerId, this);
			experience = 8;
			break;
		case "76":
			Game1.createObjectDebris("(O)536", x, y, farmerId, this);
			experience = 16;
			break;
		case "77":
			Game1.createObjectDebris("(O)537", x, y, farmerId, this);
			experience = 32;
			break;
		case "816":
		case "817":
			if (r.NextDouble() < 0.1)
			{
				Game1.createObjectDebris("(O)823", x, y, farmerId, this);
			}
			else if (r.NextDouble() < 0.015)
			{
				Game1.createObjectDebris("(O)824", x, y, farmerId, this);
			}
			else if (r.NextDouble() < 0.1)
			{
				Game1.createObjectDebris("(O)" + (579 + r.Next(11)), x, y, farmerId, this);
			}
			Game1.createMultipleObjectDebris("(O)881", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			experience = 6;
			break;
		case "818":
			Game1.createMultipleObjectDebris("(O)330", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			experience = 6;
			break;
		case "819":
			Game1.createObjectDebris("(O)749", x, y, farmerId, this);
			experience = 64;
			break;
		case "8":
			Game1.createMultipleObjectDebris("(O)66", x, y, (who == null || who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2, farmerId, this);
			experience = 16;
			break;
		case "10":
			Game1.createMultipleObjectDebris("(O)68", x, y, (who == null || who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2, farmerId, this);
			experience = 16;
			break;
		case "12":
			Game1.createMultipleObjectDebris("(O)60", x, y, (who == null || who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2, farmerId, this);
			experience = 80;
			break;
		case "14":
			Game1.createMultipleObjectDebris("(O)62", x, y, (who == null || who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2, farmerId, this);
			experience = 40;
			break;
		case "6":
			Game1.createMultipleObjectDebris("(O)70", x, y, (who == null || who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2, farmerId, this);
			experience = 40;
			break;
		case "4":
			Game1.createMultipleObjectDebris("(O)64", x, y, (who == null || who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2, farmerId, this);
			experience = 80;
			break;
		case "2":
			Game1.createMultipleObjectDebris("(O)72", x, y, (who == null || who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2, farmerId, this);
			experience = 150;
			break;
		case "846":
		case "847":
		case "668":
		case "845":
		case "670":
			Game1.createMultipleObjectDebris("(O)390", x, y, addedOres + r.Next(1, 3) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			experience = 3;
			if (r.NextDouble() < 0.08)
			{
				Game1.createMultipleObjectDebris("(O)382", x, y, 1 + addedOres, farmerId, this);
				experience = 4;
			}
			break;
		case "849":
		case "751":
			Game1.createMultipleObjectDebris("(O)378", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			experience = 5;
			Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Orange * 0.5f, 175, 100));
			break;
		case "850":
		case "290":
			Game1.createMultipleObjectDebris("(O)380", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			experience = 12;
			Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.White * 0.5f, 175, 100));
			break;
		case "BasicCoalNode0":
		case "BasicCoalNode1":
		case "VolcanoCoalNode0":
		case "VolcanoCoalNode1":
			Game1.createMultipleObjectDebris("(O)382", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			experience = 10;
			Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Black * 0.5f, 175, 100));
			break;
		case "764":
		case "VolcanoGoldNode":
			Game1.createMultipleObjectDebris("(O)384", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			experience = 18;
			Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 3, Color.Yellow * 0.5f, 175, 100));
			break;
		case "765":
			Game1.createMultipleObjectDebris("(O)386", x, y, addedOres + r.Next(1, 4) + ((r.NextDouble() < (double)((float)farmerLuckLevel / 100f)) ? 1 : 0) + ((r.NextDouble() < (double)((float)farmerMiningLevel / 100f)) ? 1 : 0), farmerId, this);
			Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, Color.BlueViolet * 0.5f, 175, 100));
			if (r.NextDouble() < 0.035)
			{
				Game1.createMultipleObjectDebris("(O)74", x, y, 1, farmerId, this);
			}
			experience = 50;
			break;
		case "CalicoEggStone_0":
		case "CalicoEggStone_1":
		case "CalicoEggStone_2":
			Game1.createMultipleObjectDebris("CalicoEgg", x, y, r.Next(1, 4) + (r.NextBool((float)farmerLuckLevel / 100f) ? 1 : 0) + (r.NextBool((float)farmerMiningLevel / 100f) ? 1 : 0), farmerId, this);
			experience = 50;
			Game1.multiplayer.broadcastSprites(this, Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(x * 64, (y - 1) * 64, 32, 96), 6, new Color(255, 120, 0) * 0.5f, 175, 100));
			break;
		}
		if (who != null && who.professions.Contains(19) && r.NextBool())
		{
			int numToDrop = ((who.stats.Get(StatKeys.Mastery(3)) == 0) ? 1 : 2);
			switch (stoneId)
			{
			case "8":
				Game1.createMultipleObjectDebris("(O)66", x, y, numToDrop, who.UniqueMultiplayerID, this);
				experience = 8;
				break;
			case "10":
				Game1.createMultipleObjectDebris("(O)68", x, y, numToDrop, who.UniqueMultiplayerID, this);
				experience = 8;
				break;
			case "12":
				Game1.createMultipleObjectDebris("(O)60", x, y, numToDrop, who.UniqueMultiplayerID, this);
				experience = 50;
				break;
			case "14":
				Game1.createMultipleObjectDebris("(O)62", x, y, numToDrop, who.UniqueMultiplayerID, this);
				experience = 20;
				break;
			case "6":
				Game1.createMultipleObjectDebris("(O)70", x, y, numToDrop, who.UniqueMultiplayerID, this);
				experience = 20;
				break;
			case "4":
				Game1.createMultipleObjectDebris("(O)64", x, y, numToDrop, who.UniqueMultiplayerID, this);
				experience = 50;
				break;
			case "2":
				Game1.createMultipleObjectDebris("(O)72", x, y, numToDrop, who.UniqueMultiplayerID, this);
				experience = 100;
				break;
			}
		}
		if (stoneId == 46.ToString())
		{
			Game1.createDebris(10, x, y, r.Next(1, 4), this);
			Game1.createDebris(6, x, y, r.Next(1, 5), this);
			if (r.NextDouble() < 0.25)
			{
				Game1.createMultipleObjectDebris("(O)74", x, y, 1, farmerId, this);
			}
			experience = 150;
			Game1.stats.MysticStonesCrushed++;
		}
		if ((this.isOutdoors.Value || this.treatAsOutdoors.Value) && experience == 0)
		{
			double chanceModifier = farmerDailyLuck / 2.0 + (double)farmerMiningLevel * 0.005 + (double)farmerLuckLevel * 0.001;
			Random ran = Utility.CreateDaySaveRandom(x * 1000, y);
			Game1.createDebris(14, x, y, 1, this);
			if (who != null)
			{
				who.gainExperience(3, 1);
				double coalChance = 0.0;
				if (who.professions.Contains(21))
				{
					coalChance += 0.05 * (1.0 + chanceModifier);
				}
				if (who.hasBuff("dwarfStatue_2"))
				{
					coalChance += 0.025;
				}
				if (ran.NextDouble() < coalChance)
				{
					Game1.createObjectDebris("(O)382", x, y, who.UniqueMultiplayerID, this);
				}
			}
			if (ran.NextDouble() < 0.05 * (1.0 + chanceModifier))
			{
				Game1.createObjectDebris("(O)382", x, y, farmerId, this);
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextBool(), 80f, 0, -1, -1f, 128));
				who?.gainExperience(3, 5);
			}
		}
		if (who != null && this.HasUnlockedAreaSecretNotes(who) && r.NextDouble() < 0.0075)
		{
			Object o = this.tryToCreateUnseenSecretNote(who);
			if (o != null)
			{
				Game1.createItemDebris(o, new Vector2((float)x + 0.5f, (float)y + 0.75f) * 64f, Game1.player.FacingDirection, this);
			}
		}
		who?.gainExperience(3, experience);
		return experience > 0;
	}

	public bool isBehindBush(Vector2 Tile)
	{
		if (this.largeTerrainFeatures != null)
		{
			Microsoft.Xna.Framework.Rectangle down = new Microsoft.Xna.Framework.Rectangle((int)Tile.X * 64, (int)(Tile.Y + 1f) * 64, 64, 128);
			foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
			{
				if (largeTerrainFeature.getBoundingBox().Intersects(down))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool isBehindTree(Vector2 Tile)
	{
		if (this.terrainFeatures != null)
		{
			Microsoft.Xna.Framework.Rectangle down = new Microsoft.Xna.Framework.Rectangle((int)(Tile.X - 1f) * 64, (int)Tile.Y * 64, 192, 256);
			foreach (KeyValuePair<Vector2, TerrainFeature> l in this.terrainFeatures.Pairs)
			{
				if (l.Value is Tree && l.Value.getBoundingBox().Intersects(down))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void spawnObjects()
	{
		Random r = Utility.CreateDaySaveRandom();
		LocationData data = this.GetData();
		if (data != null && this.numberOfSpawnedObjectsOnMap < data.MaxSpawnedForageAtOnce)
		{
			Season season = this.GetSeason();
			List<SpawnForageData> possibleForage = new List<SpawnForageData>();
			foreach (SpawnForageData spawn in GameLocation.GetData("Default").Forage.Concat(data.Forage))
			{
				if ((spawn.Condition == null || GameStateQuery.CheckConditions(spawn.Condition, this, null, null, null, r)) && (!spawn.Season.HasValue || spawn.Season == season))
				{
					possibleForage.Add(spawn);
				}
			}
			if (possibleForage.Any())
			{
				int numberToSpawn = r.Next(data.MinDailyForageSpawn, data.MaxDailyForageSpawn + 1);
				numberToSpawn = Math.Min(numberToSpawn, data.MaxSpawnedForageAtOnce - this.numberOfSpawnedObjectsOnMap);
				ItemQueryContext itemQueryContext = new ItemQueryContext(this, null, r, "location '" + this.NameOrUniqueName + "' > forage");
				for (int i = 0; i < numberToSpawn; i++)
				{
					for (int attempt = 0; attempt < 11; attempt++)
					{
						int xCoord = r.Next(this.map.DisplayWidth / 64);
						int yCoord = r.Next(this.map.DisplayHeight / 64);
						Vector2 location = new Vector2(xCoord, yCoord);
						if (this.objects.ContainsKey(location) || this.IsNoSpawnTile(location) || this.doesTileHaveProperty(xCoord, yCoord, "Spawnable", "Back") == null || this.doesEitherTileOrTileIndexPropertyEqual(xCoord, yCoord, "Spawnable", "Back", "F") || !this.CanItemBePlacedHere(location) || this.hasTileAt(xCoord, yCoord, "AlwaysFront") || this.hasTileAt(xCoord, yCoord, "AlwaysFront2") || this.hasTileAt(xCoord, yCoord, "AlwaysFront3") || this.hasTileAt(xCoord, yCoord, "Front") || this.isBehindBush(location) || (!r.NextBool(0.1) && this.isBehindTree(location)))
						{
							continue;
						}
						SpawnForageData forage = r.ChooseFrom(possibleForage);
						if (!r.NextBool(forage.Chance))
						{
							continue;
						}
						Item forageItem = ItemQueryResolver.TryResolveRandomItem(forage, itemQueryContext, avoidRepeat: false, null, null, null, delegate(string query, string error)
						{
							Game1.log.Error($"Location '{this.NameOrUniqueName}' failed parsing item query '{query}' for forage '{forage.Id}': {error}");
						});
						if (forageItem == null)
						{
							continue;
						}
						if (!(forageItem is Object forageObj))
						{
							Game1.log.Warn($"Location '{this.Name}' ignored invalid forage data '{forage.Id}': the resulting item '{forageItem.QualifiedItemId}' isn't an {"Object"}-type item.");
						}
						else
						{
							forageObj.IsSpawnedObject = true;
							if (this.dropObject(forageObj, location * 64f, Game1.viewport, initialPlacement: true))
							{
								this.numberOfSpawnedObjectsOnMap++;
								break;
							}
						}
					}
				}
			}
		}
		List<Vector2> positionOfArtifactSpots = new List<Vector2>();
		foreach (KeyValuePair<Vector2, Object> v in this.objects.Pairs)
		{
			if (v.Value.QualifiedItemId == "(O)590" || v.Value.QualifiedItemId == "(O)SeedSpot")
			{
				positionOfArtifactSpots.Add(v.Key);
			}
		}
		if (!(this is Farm) && !(this is IslandWest))
		{
			this.spawnWeedsAndStones();
		}
		for (int i2 = positionOfArtifactSpots.Count - 1; i2 >= 0; i2--)
		{
			if ((!(this is IslandNorth) || !(positionOfArtifactSpots[i2].X < 26f)) && r.NextBool(0.15))
			{
				this.objects.Remove(positionOfArtifactSpots[i2]);
				positionOfArtifactSpots.RemoveAt(i2);
			}
		}
		if (positionOfArtifactSpots.Count > ((!(this is Farm)) ? 1 : 0) && (this.GetSeason() != Season.Winter || positionOfArtifactSpots.Count > 4))
		{
			return;
		}
		double chanceForNewArtifactAttempt = 1.0;
		while (r.NextDouble() < chanceForNewArtifactAttempt)
		{
			int xCoord2 = r.Next(this.map.DisplayWidth / 64);
			int yCoord2 = r.Next(this.map.DisplayHeight / 64);
			Vector2 location2 = new Vector2(xCoord2, yCoord2);
			if (this.CanItemBePlacedHere(location2) && !this.IsTileOccupiedBy(location2) && !this.hasTileAt(xCoord2, yCoord2, "AlwaysFront") && !this.hasTileAt(xCoord2, yCoord2, "Front") && !this.isBehindBush(location2) && (this.doesTileHaveProperty(xCoord2, yCoord2, "Diggable", "Back") != null || (this.GetSeason() == Season.Winter && this.doesTileHaveProperty(xCoord2, yCoord2, "Type", "Back") != null && this.doesTileHaveProperty(xCoord2, yCoord2, "Type", "Back").Equals("Grass"))))
			{
				if (this.name.Equals("Forest") && xCoord2 >= 93 && yCoord2 <= 22)
				{
					continue;
				}
				this.objects.Add(location2, ItemRegistry.Create<Object>(r.NextBool(0.166) ? "(O)SeedSpot" : "(O)590"));
			}
			chanceForNewArtifactAttempt *= 0.75;
			if (this.GetSeason() == Season.Winter)
			{
				chanceForNewArtifactAttempt += 0.10000000149011612;
			}
		}
	}

	public void spawnWeedsAndStones(int numDebris = -1, bool weedsOnly = false, bool spawnFromOldWeeds = true)
	{
		if ((this is Farm || this is IslandWest) && Game1.IsBuildingConstructed("Gold Clock") && !Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
		{
			return;
		}
		bool notified_destruction = false;
		if (this is Beach || this.GetSeason() == Season.Winter || this is Desert)
		{
			return;
		}
		int numWeedsAndStones = ((numDebris != -1) ? numDebris : ((Game1.random.NextDouble() < 0.95) ? ((Game1.random.NextDouble() < 0.25) ? Game1.random.Next(10, 21) : Game1.random.Next(5, 11)) : 0));
		if (this.IsRainingHere())
		{
			numWeedsAndStones *= 2;
		}
		if (Game1.dayOfMonth == 1)
		{
			numWeedsAndStones *= 5;
		}
		if (this.objects.Length <= 0 && spawnFromOldWeeds)
		{
			return;
		}
		if (!(this is Farm))
		{
			numWeedsAndStones /= 2;
		}
		bool greenRain = this.IsGreenRainingHere();
		for (int i = 0; i < numWeedsAndStones; i++)
		{
			Vector2 v = (spawnFromOldWeeds ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : new Vector2(Game1.random.Next(this.map.Layers[0].LayerWidth), Game1.random.Next(this.map.Layers[0].LayerHeight)));
			if (!spawnFromOldWeeds && this is IslandWest)
			{
				v = new Vector2(Game1.random.Next(57, 97), Game1.random.Next(44, 68));
			}
			while (spawnFromOldWeeds && v.Equals(Vector2.Zero))
			{
				v = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			Vector2 fromTile = Vector2.Zero;
			Object fromObj = null;
			if (spawnFromOldWeeds)
			{
				Utility.TryGetRandom(this.objects, out fromTile, out fromObj);
			}
			Vector2 baseVect = (spawnFromOldWeeds ? fromTile : Vector2.Zero);
			if ((this is Mountain && v.X + baseVect.X > 100f) || this is IslandNorth)
			{
				continue;
			}
			bool num = this is Farm || this is IslandWest;
			int checked_tile_x = (int)(v.X + baseVect.X);
			int checked_tile_y = (int)(v.Y + baseVect.Y);
			Vector2 checked_tile = v + baseVect;
			int health = 1;
			bool is_valid_tile = false;
			bool tile_is_diggable = this.doesTileHaveProperty(checked_tile_x, checked_tile_y, "Diggable", "Back") != null;
			if (num == tile_is_diggable && !this.IsNoSpawnTile(checked_tile) && this.doesTileHaveProperty(checked_tile_x, checked_tile_y, "Type", "Back") != "Wood")
			{
				bool is_tile_clear = false;
				if (this.CanItemBePlacedHere(checked_tile) && !this.terrainFeatures.ContainsKey(checked_tile))
				{
					is_tile_clear = true;
				}
				else if (spawnFromOldWeeds)
				{
					if (this.objects.TryGetValue(checked_tile, out var tileObj))
					{
						if (greenRain)
						{
							is_tile_clear = false;
						}
						else if (!tileObj.IsTapper())
						{
							is_tile_clear = true;
						}
					}
					if (!is_tile_clear && this.terrainFeatures.TryGetValue(checked_tile, out var terrainFeature) && (terrainFeature is HoeDirt || terrainFeature is Flooring))
					{
						is_tile_clear = !greenRain && this.getLargeTerrainFeatureAt(checked_tile_x, checked_tile_y) == null;
					}
				}
				if (is_tile_clear)
				{
					if (spawnFromOldWeeds)
					{
						is_valid_tile = true;
					}
					else if (!this.objects.ContainsKey(checked_tile))
					{
						is_valid_tile = true;
					}
				}
			}
			if (!is_valid_tile)
			{
				continue;
			}
			string whatToAdd = null;
			if (this is Desert)
			{
				whatToAdd = "(O)750";
			}
			else
			{
				if (Game1.random.NextBool() && !weedsOnly && (!spawnFromOldWeeds || fromObj.IsBreakableStone() || fromObj.IsTwig()))
				{
					whatToAdd = Game1.random.Choose("(O)294", "(O)295", "(O)343", "(O)450");
				}
				else if (!spawnFromOldWeeds || fromObj.IsWeeds())
				{
					whatToAdd = GameLocation.getWeedForSeason(Game1.random, this.GetSeason());
					if (this.IsGreenRainingHere())
					{
						if (this.doesTileHavePropertyNoNull((int)(v.X + baseVect.X), (int)(v.Y + baseVect.Y), "Type", "Back") == (this.IsFarm ? "Dirt" : "Grass"))
						{
							int which = Game1.random.Next(8);
							whatToAdd = "(O)GreenRainWeeds" + which;
							if (which == 2 || which == 3 || which == 7)
							{
								health = 2;
							}
						}
						else
						{
							whatToAdd = null;
						}
					}
				}
				if (this is Farm && !spawnFromOldWeeds && Game1.random.NextDouble() < 0.05 && !this.terrainFeatures.ContainsKey(checked_tile))
				{
					this.terrainFeatures.Add(checked_tile, new Tree((Game1.random.Next(3) + 1).ToString(), Game1.random.Next(3)));
					continue;
				}
			}
			if (whatToAdd == null)
			{
				continue;
			}
			bool destroyed = false;
			if (this.objects.TryGetValue(v + baseVect, out var removedObj))
			{
				if (greenRain || removedObj is Fence || removedObj is Chest || removedObj.QualifiedItemId == "(O)590" || removedObj.QualifiedItemId == "(BC)MushroomLog")
				{
					continue;
				}
				if (removedObj.name.Length > 0 && removedObj.Category != -999)
				{
					destroyed = true;
					Game1.debugOutput = removedObj.Name + " was destroyed";
				}
				this.objects.Remove(v + baseVect);
			}
			if (this.terrainFeatures.TryGetValue(v + baseVect, out var removedFeature))
			{
				try
				{
					destroyed = removedFeature is HoeDirt || removedFeature is Flooring;
				}
				catch (Exception)
				{
				}
				if (!destroyed || this.IsGreenRainingHere())
				{
					break;
				}
				this.terrainFeatures.Remove(v + baseVect);
			}
			if (destroyed && this is Farm && Game1.stats.DaysPlayed > 1 && !notified_destruction)
			{
				notified_destruction = true;
				Game1.multiplayer.broadcastGlobalMessage("Strings\\Locations:Farm_WeedsDestruction", false, null);
			}
			Object obj = ItemRegistry.Create<Object>(whatToAdd);
			obj.minutesUntilReady.Value = health;
			this.objects.TryAdd(v + baseVect, obj);
		}
	}

	[Obsolete("Use removeObjectsAndSpawned instead.")]
	public virtual void removeEverythingExceptCharactersFromThisTile(int x, int y)
	{
		this.removeObjectsAndSpawned(x, y, 1, 1);
	}

	/// <summary>Remove all objects, bushes, resource clumps, and terrain features within an area.</summary>
	/// <param name="x">The top-left X position of the area to clear.</param>
	/// <param name="y">The top-right X position of the area to clear.</param>
	/// <param name="width">The width of the area to clear.</param>
	/// <param name="height">The height of the area to clear.</param>
	public virtual void removeObjectsAndSpawned(int x, int y, int width, int height)
	{
		Microsoft.Xna.Framework.Rectangle pixelArea = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, width * 64, height * 64);
		int maxX = x + width - 1;
		int maxY = y + height - 1;
		for (int curY = y; curY <= maxY; curY++)
		{
			for (int curX = x; curX <= maxX; curX++)
			{
				Vector2 tile = new Vector2(curX, curY);
				this.terrainFeatures.Remove(tile);
				this.objects.Remove(tile);
			}
		}
		this.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature.getBoundingBox().Intersects(pixelArea));
		this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.getBoundingBox().Intersects(pixelArea));
	}

	public virtual string getFootstepSoundReplacement(string footstep)
	{
		return footstep;
	}

	public virtual void removeEverythingFromThisTile(int x, int y)
	{
		Vector2 tile = new Vector2(x, y);
		Point pixel = Utility.Vector2ToPoint(tile * 64f + new Vector2(32f));
		this.resourceClumps.RemoveWhere((ResourceClump clump) => clump.Tile == tile);
		this.terrainFeatures.Remove(tile);
		this.objects.Remove(tile);
		this.furniture.RemoveWhere((Furniture f) => f.GetBoundingBox().Contains(pixel));
		this.characters.RemoveWhere((NPC npc) => npc.Tile == tile && npc is Monster);
	}

	public virtual bool TryGetLocationEvents(out string assetName, out Dictionary<string, string> events)
	{
		events = null;
		assetName = ((this.NameOrUniqueName == Game1.player.homeLocation.Value) ? "Data\\Events\\FarmHouse" : ("Data\\Events\\" + this.name.Value));
		try
		{
			if (Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
			{
				events = Game1.content.Load<Dictionary<string, string>>(assetName);
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error($"Failed loading events for location '{this.NameOrUniqueName}' from asset '{assetName}'.", exception);
		}
		if (events == null)
		{
			events = new Dictionary<string, string>();
		}
		if (assetName != "Data\\Events\\FarmHouse")
		{
			foreach (KeyValuePair<string, string> @event in Game1.content.Load<Dictionary<string, string>>("Data\\Events\\FarmHouse"))
			{
				if (@event.Key.StartsWith("558291/") || @event.Key.StartsWith("558292/"))
				{
					events.TryAdd(@event.Key, @event.Value);
				}
			}
		}
		if (this.Name == "Trailer_Big")
		{
			events = new Dictionary<string, string>(events);
			Dictionary<string, string> trailer_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Trailer");
			if (trailer_events != null)
			{
				foreach (string trailer_event_key in trailer_events.Keys)
				{
					string event_string = trailer_events[trailer_event_key];
					if (!(this.name.Value == "Trailer_Big") || !events.ContainsKey(trailer_event_key))
					{
						if (trailer_event_key.StartsWith("36/"))
						{
							event_string = event_string.Replace("/farmer -30 30 0", "/farmer 12 19 0");
							event_string = event_string.Replace("/playSound doorClose/warp farmer 12 9", "/move farmer 0 -10 0");
						}
						else if (trailer_event_key.StartsWith("35/"))
						{
							event_string = event_string.Replace("/farmer -30 30 0", "/farmer 12 19 0");
							event_string = event_string.Replace("/warp farmer 12 9/playSound doorClose", "/move farmer 0 -10 0");
							event_string = event_string.Replace("/warp farmer -40 -40/playSound doorClose", "/move farmer 0 10 0/warp farmer -40 -40");
						}
						events[trailer_event_key] = event_string;
					}
				}
			}
		}
		return events.Count > 0;
	}

	public static bool IsValidLocationEvent(string key, string eventScript)
	{
		if (!key.Contains('/') && !int.TryParse(key, out var _))
		{
			return false;
		}
		string[] commands = Event.ParseCommands(eventScript);
		if (commands.Length < 3)
		{
			return false;
		}
		string cameraPosition = commands[1];
		if (cameraPosition.Length == 0 || (cameraPosition != "follow" && !char.IsDigit(cameraPosition[0]) && cameraPosition[0] != '-'))
		{
			return false;
		}
		return true;
	}

	public virtual void checkForEvents()
	{
		if (Game1.killScreen && !Game1.eventUp)
		{
			if (Game1.player.bathingClothes.Value)
			{
				Game1.player.changeOutOfSwimSuit();
			}
			if (this.name.Equals("Mine"))
			{
				string rescuer;
				string uniquemessage;
				switch (Game1.random.Next(7))
				{
				case 0:
					rescuer = "Robin";
					uniquemessage = "Data\\ExtraDialogue:Mines_PlayerKilled_Robin";
					break;
				case 1:
					rescuer = "Clint";
					uniquemessage = "Data\\ExtraDialogue:Mines_PlayerKilled_Clint";
					break;
				case 2:
					rescuer = "Maru";
					uniquemessage = ((Game1.player.spouse == "Maru") ? "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_Spouse" : "Data\\ExtraDialogue:Mines_PlayerKilled_Maru_NotSpouse");
					break;
				default:
					rescuer = "Linus";
					uniquemessage = "Data\\ExtraDialogue:Mines_PlayerKilled_Linus";
					break;
				}
				if (Game1.random.NextDouble() < 0.1 && Game1.player.spouse != null && !Game1.player.isEngaged() && Game1.player.spouse.Length > 1)
				{
					rescuer = Game1.player.spouse;
					uniquemessage = (Game1.player.IsMale ? "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerMale" : "Data\\ExtraDialogue:Mines_PlayerKilled_Spouse_PlayerFemale");
				}
				this.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Mine:PlayerKilled", rescuer, uniquemessage, ArgUtility.EscapeQuotes(Game1.player.Name)));
			}
			else if (this is IslandLocation)
			{
				string rescuer2 = "Willy";
				string uniquemessage2 = "Data\\ExtraDialogue:Island_willy_rescue";
				if (Game1.player.friendshipData.ContainsKey("Leo") && Game1.random.NextBool())
				{
					rescuer2 = "Leo";
					uniquemessage2 = "Data\\ExtraDialogue:Island_leo_rescue";
				}
				this.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:PlayerKilled", rescuer2, uniquemessage2, ArgUtility.EscapeQuotes(Game1.player.Name)));
			}
			else if (this.name.Equals("Hospital"))
			{
				this.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\Hospital:PlayerKilled", ArgUtility.EscapeQuotes(Game1.player.Name)));
			}
			else
			{
				try
				{
					if (this.TryGetLocationEvents(out var assetName, out var events) && events.TryGetValue("PlayerKilled", out var eventScript))
					{
						this.currentEvent = new Event(eventScript, assetName, "PlayerKilled");
					}
				}
				catch (Exception)
				{
				}
			}
			if (this.currentEvent != null)
			{
				Game1.eventUp = true;
			}
			Game1.changeMusicTrack("none", track_interruptable: true);
			Game1.killScreen = false;
			Game1.player.health = 10;
		}
		else if (!Game1.eventUp && Game1.weddingsToday.Count > 0 && (Game1.CurrentEvent == null || Game1.CurrentEvent.id != "-2") && Game1.currentLocation != null && !Game1.currentLocation.IsTemporary)
		{
			this.currentEvent = Game1.getAvailableWeddingEvent();
			if (this.currentEvent != null)
			{
				this.startEvent(this.currentEvent);
			}
		}
		else
		{
			if (Game1.eventUp || Game1.farmEvent != null)
			{
				return;
			}
			string key = $"{Game1.currentSeason}{Game1.dayOfMonth}";
			try
			{
				if (Event.tryToLoadFestival(key, out var festival))
				{
					this.currentEvent = festival;
				}
			}
			catch (Exception)
			{
			}
			if (!Game1.eventUp && this.currentEvent == null && Game1.farmEvent == null && !this.IsGreenRainingHere())
			{
				string eventAssetName;
				Dictionary<string, string> events2;
				try
				{
					if (!this.TryGetLocationEvents(out eventAssetName, out events2))
					{
						return;
					}
				}
				catch
				{
					return;
				}
				if (events2 != null)
				{
					foreach (string eventKey in events2.Keys)
					{
						string eventId = this.checkEventPrecondition(eventKey);
						if (!string.IsNullOrEmpty(eventId) && eventId != "-1" && GameLocation.IsValidLocationEvent(eventKey, events2[eventKey]))
						{
							this.currentEvent = new Event(events2[eventKey], eventAssetName, eventId);
							break;
						}
					}
					if (this.currentEvent == null && Game1.IsMasterGame && Game1.stats.DaysPlayed >= 20 && !Game1.player.mailReceived.Contains("rejectedPet") && !Game1.player.hasPet() && Pet.TryGetData(Game1.player.whichPetType, out var data) && this.Name == data.AdoptionEventLocation && !string.IsNullOrWhiteSpace(data.AdoptionEventId) && !Game1.player.eventsSeen.Contains(data.AdoptionEventId))
					{
						Game1.PlayEvent(data.AdoptionEventId, checkPreconditions: false, checkSeen: false);
					}
				}
			}
			if (this.currentEvent != null)
			{
				this.startEvent(this.currentEvent);
			}
		}
	}

	public Event findEventById(string id, Farmer farmerActor = null)
	{
		if (id == "-2")
		{
			long? spouseFarmer = Game1.player.team.GetSpouse(farmerActor.UniqueMultiplayerID);
			if (farmerActor == null || !spouseFarmer.HasValue)
			{
				return Utility.getWeddingEvent(farmerActor);
			}
			if (Game1.otherFarmers.ContainsKey(spouseFarmer.Value))
			{
				return Utility.getWeddingEvent(farmerActor);
			}
		}
		string eventAssetName;
		Dictionary<string, string> events;
		try
		{
			if (!this.TryGetLocationEvents(out eventAssetName, out events))
			{
				return null;
			}
		}
		catch
		{
			return null;
		}
		foreach (KeyValuePair<string, string> pair in events)
		{
			if (Event.SplitPreconditions(pair.Key)[0] == id)
			{
				return new Event(pair.Value, eventAssetName, id, farmerActor);
			}
		}
		return null;
	}

	public virtual void startEvent(Event evt)
	{
		if (Game1.eventUp || Game1.eventOver)
		{
			return;
		}
		this.currentEvent = evt;
		this.ResetForEvent(evt);
		if (evt.exitLocation == null)
		{
			evt.exitLocation = Game1.getLocationRequest(this.NameOrUniqueName, this.isStructure.Value);
		}
		if (Game1.player.mount != null)
		{
			Horse mount = Game1.player.mount;
			mount.currentLocation = this;
			mount.dismount();
			Microsoft.Xna.Framework.Rectangle bbox = mount.GetBoundingBox();
			Vector2 position = mount.Position;
			if (mount.currentLocation != null && mount.currentLocation.isCollidingPosition(bbox, Game1.viewport, isFarmer: false, 0, glider: false, mount, pathfinding: true))
			{
				bbox.X -= 64;
				if (!mount.currentLocation.isCollidingPosition(bbox, Game1.viewport, isFarmer: false, 0, glider: false, mount, pathfinding: true))
				{
					position.X -= 64f;
					mount.Position = position;
				}
				else
				{
					bbox.X += 128;
					if (!mount.currentLocation.isCollidingPosition(bbox, Game1.viewport, isFarmer: false, 0, glider: false, mount, pathfinding: true))
					{
						position.X += 64f;
						mount.Position = position;
					}
				}
			}
		}
		foreach (NPC character in this.characters)
		{
			character.clearTextAboveHead();
		}
		Game1.eventUp = true;
		Game1.displayHUD = false;
		Game1.player.CanMove = false;
		Game1.player.showNotCarrying();
		this.critters?.Clear();
		if (this.currentEvent != null)
		{
			Game1.player.autoGenerateActiveDialogueEvent("eventSeen_" + this.currentEvent.id);
		}
	}

	public virtual void drawBackground(SpriteBatch b)
	{
	}

	public virtual void drawWater(SpriteBatch b)
	{
		this.currentEvent?.drawUnderWater(b);
		if (this.waterTiles == null)
		{
			return;
		}
		for (int y = Math.Max(0, Game1.viewport.Y / 64 - 1); y < Math.Min(this.map.Layers[0].LayerHeight, (Game1.viewport.Y + Game1.viewport.Height) / 64 + 2); y++)
		{
			for (int x = Math.Max(0, Game1.viewport.X / 64 - 1); x < Math.Min(this.map.Layers[0].LayerWidth, (Game1.viewport.X + Game1.viewport.Width) / 64 + 1); x++)
			{
				if (this.waterTiles.waterTiles[x, y].isWater && this.waterTiles.waterTiles[x, y].isVisible)
				{
					this.drawWaterTile(b, x, y);
				}
			}
		}
	}

	public virtual void drawWaterTile(SpriteBatch b, int x, int y)
	{
		this.drawWaterTile(b, x, y, this.waterColor.Value);
	}

	public void drawWaterTile(SpriteBatch b, int x, int y, Color color)
	{
		bool num = y == this.map.Layers[0].LayerHeight - 1 || !this.waterTiles[x, y + 1];
		bool topY = y == 0 || !this.waterTiles[x, y - 1];
		b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!topY) ? this.waterPosition : 0f))), new Microsoft.Xna.Framework.Rectangle(this.waterAnimationIndex * 64, 2064 + (((x + y) % 2 != 0) ? ((!this.waterTileFlip) ? 128 : 0) : (this.waterTileFlip ? 128 : 0)) + (topY ? ((int)this.waterPosition) : 0), 64, 64 + (topY ? ((int)(0f - this.waterPosition)) : 0)), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
		if (num)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)this.waterPosition)), new Microsoft.Xna.Framework.Rectangle(this.waterAnimationIndex * 64, 2064 + (((x + (y + 1)) % 2 != 0) ? ((!this.waterTileFlip) ? 128 : 0) : (this.waterTileFlip ? 128 : 0)), 64, 64 - (int)(64f - this.waterPosition) - 1), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
		}
	}

	public virtual void drawFloorDecorations(SpriteBatch b)
	{
		int borderBuffer = 1;
		Microsoft.Xna.Framework.Rectangle viewportRect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - borderBuffer, Game1.viewport.Y / 64 - borderBuffer, (int)Math.Ceiling((float)Game1.viewport.Width / 64f) + 2 * borderBuffer, (int)Math.Ceiling((float)Game1.viewport.Height / 64f) + 3 + 2 * borderBuffer);
		Microsoft.Xna.Framework.Rectangle objectRectangle = default(Microsoft.Xna.Framework.Rectangle);
		if (this.buildings.Count > 0)
		{
			foreach (Building building in this.buildings)
			{
				int additionalRadius = building.GetAdditionalTilePropertyRadius();
				Microsoft.Xna.Framework.Rectangle sourceRect = building.getSourceRect();
				objectRectangle.X = building.tileX.Value - additionalRadius;
				objectRectangle.Width = building.tilesWide.Value + additionalRadius * 2;
				int bottomY = building.tileY.Value + building.tilesHigh.Value + additionalRadius;
				objectRectangle.Height = bottomY - (objectRectangle.Y = bottomY - (int)Math.Ceiling((float)sourceRect.Height * 4f / 64f) - additionalRadius);
				if (objectRectangle.Intersects(viewportRect))
				{
					building.drawBackground(b);
				}
			}
		}
		if (!Game1.isFestival() && this.terrainFeatures.Length > 0)
		{
			Vector2 tile = default(Vector2);
			for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
			{
				for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
				{
					tile.X = x;
					tile.Y = y;
					if (this.terrainFeatures.TryGetValue(tile, out var feat) && feat is Flooring)
					{
						feat.draw(b);
					}
				}
			}
		}
		if (Game1.eventUp && !(this is Farm) && !(this is FarmHouse))
		{
			return;
		}
		Furniture.isDrawingLocationFurniture = true;
		foreach (Furniture f in this.furniture)
		{
			if (f.furniture_type.Value == 12)
			{
				f.draw(b, -1, -1);
			}
		}
		Furniture.isDrawingLocationFurniture = false;
	}

	public TemporaryAnimatedSprite getTemporarySpriteByID(int id)
	{
		for (int i = 0; i < this.temporarySprites.Count; i++)
		{
			if (this.temporarySprites[i].id == id)
			{
				return this.temporarySprites[i];
			}
		}
		return null;
	}

	protected void drawDebris(SpriteBatch b)
	{
		int counter = 0;
		foreach (Debris d in this.debris)
		{
			counter++;
			if (d.item != null)
			{
				Vector2 position = d.Chunks[0].GetVisualPosition();
				if (d.item is Object obj && obj.bigCraftable.Value)
				{
					obj.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 32f))), 1.6f, 1f, ((float)(d.chunkFinalYLevel + 64 + 8) + position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
				}
				else
				{
					d.item.drawInMenu(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 32f))), 0.8f + (float)d.itemQuality * 0.1f, 1f, ((float)(d.chunkFinalYLevel + 64 + 8) + position.X / 10000f) / 10000f, StackDrawType.Hide, Color.White, drawShadow: true);
				}
				continue;
			}
			switch (d.debrisType.Value)
			{
			case Debris.DebrisType.LETTERS:
			{
				Chunk chunk = d.Chunks[0];
				Vector2 position2 = chunk.GetVisualPosition();
				Game1.drawWithBorder(d.debrisMessage.Value, Color.Black, d.nonSpriteChunkColor.Value, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position2)), chunk.rotation, chunk.scale, (position2.Y + 64f) / 10000f);
				continue;
			}
			case Debris.DebrisType.NUMBERS:
			{
				Chunk chunk2 = d.Chunks[0];
				Vector2 position3 = chunk2.GetVisualPosition();
				NumberSprite.draw(d.chunkType.Value, b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(new Vector2(position3.X, (float)d.chunkFinalYLevel - ((float)d.chunkFinalYLevel - position3.Y)))), d.nonSpriteChunkColor.Value, chunk2.scale * 0.75f, 0.98f + 0.0001f * (float)counter, chunk2.alpha, -1 * (int)((float)d.chunkFinalYLevel - position3.Y) / 2);
				continue;
			}
			case Debris.DebrisType.SPRITECHUNKS:
			{
				for (int i = 0; i < d.Chunks.Count; i++)
				{
					Chunk chunk3 = d.Chunks[0];
					Vector2 position4 = chunk3.GetVisualPosition();
					b.Draw(d.spriteChunkSheet, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position4)), new Microsoft.Xna.Framework.Rectangle(chunk3.xSpriteSheet.Value, chunk3.ySpriteSheet.Value, Math.Min(d.sizeOfSourceRectSquares.Value, d.spriteChunkSheet.Bounds.Width), Math.Min(d.sizeOfSourceRectSquares.Value, d.spriteChunkSheet.Bounds.Height)), d.nonSpriteChunkColor.Value * chunk3.alpha, chunk3.rotation, new Vector2(d.sizeOfSourceRectSquares.Value / 2, d.sizeOfSourceRectSquares.Value / 2), chunk3.scale, SpriteEffects.None, ((float)(d.chunkFinalYLevel + 16) + position4.X / 10000f) / 10000f);
				}
				continue;
			}
			}
			if (d.itemId.Value != null)
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(d.itemId.Value);
				Texture2D texture = itemData.GetTexture();
				float scale = ((d.debrisType.Value == Debris.DebrisType.RESOURCE || d.floppingFish.Value) ? 4f : (4f * (0.8f + (float)d.itemQuality * 0.1f)));
				for (int j = 0; j < d.Chunks.Count; j++)
				{
					Chunk chunk4 = d.Chunks[j];
					Vector2 position5 = chunk4.GetVisualPosition();
					Microsoft.Xna.Framework.Rectangle sourceRect = ((d.debrisType.Value == Debris.DebrisType.RESOURCE) ? itemData.GetSourceRect(chunk4.randomOffset) : itemData.GetSourceRect());
					SpriteEffects spriteEffect = ((d.floppingFish.Value && chunk4.bounces % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
					b.Draw(texture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, position5)), sourceRect, Color.White, 0f, Vector2.Zero, scale, spriteEffect, ((float)(d.chunkFinalYLevel + 32) + position5.X / 10000f) / 10000f);
					b.Draw(Game1.shadowTexture, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, new Vector2(position5.X + 25.6f, (d.chunksMoveTowardPlayer ? (position5.Y + 8f) : ((float)d.chunkFinalYLevel)) + 32f + (float)(12 * d.itemQuality)))), Game1.shadowTexture.Bounds, Color.White * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Math.Min(3f, 3f - (d.chunksMoveTowardPlayer ? 0f : (((float)d.chunkFinalYLevel - position5.Y) / 96f))), SpriteEffects.None, (float)d.chunkFinalYLevel / 10000f);
				}
			}
			else
			{
				for (int k = 0; k < d.Chunks.Count; k++)
				{
					Vector2 position6 = Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, d.Chunks[k].position.Value));
					Microsoft.Xna.Framework.Rectangle sourceRect2 = Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, d.chunkType.Value + d.Chunks[k].randomOffset, 16, 16);
					float drawLayer = (d.Chunks[k].position.Y + 128f + d.Chunks[k].position.X / 10000f) / 10000f;
					b.Draw(Game1.debrisSpriteSheet, position6, sourceRect2, d.chunksColor.Value, 0f, Vector2.Zero, 4f * d.scale.Value, SpriteEffects.None, drawLayer);
				}
			}
		}
	}

	public virtual bool shouldHideCharacters()
	{
		return false;
	}

	protected virtual void drawCharacters(SpriteBatch b)
	{
		if (this.shouldHideCharacters() || (Game1.eventUp && (Game1.CurrentEvent == null || !Game1.CurrentEvent.showWorldCharacters)))
		{
			return;
		}
		for (int i = 0; i < this.characters.Count; i++)
		{
			if (this.characters[i] != null)
			{
				this.characters[i].draw(b);
			}
		}
	}

	protected virtual void drawFarmers(SpriteBatch b)
	{
		if (this.shouldHideCharacters() || Game1.currentMinigame != null)
		{
			return;
		}
		if (this.currentEvent == null || this.currentEvent.isFestival || this.currentEvent.farmerActors.Count == 0)
		{
			foreach (Farmer farmer in this.farmers)
			{
				if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
				{
					farmer.draw(b);
				}
			}
			return;
		}
		this.currentEvent.drawFarmers(b);
	}

	public virtual void DrawFarmerUsernames(SpriteBatch b)
	{
		if (this.shouldHideCharacters() || Game1.currentMinigame != null || (this.currentEvent != null && !this.currentEvent.isFestival && this.currentEvent.farmerActors.Count != 0))
		{
			return;
		}
		foreach (Farmer farmer in this.farmers)
		{
			if (!Game1.multiplayer.isDisconnecting(farmer.UniqueMultiplayerID))
			{
				farmer.DrawUsername(b);
			}
		}
	}

	public virtual void draw(SpriteBatch b)
	{
		if (this.animals.Length > 0)
		{
			foreach (FarmAnimal value in this.animals.Values)
			{
				value.draw(b);
			}
		}
		if (this.mapSeats.Count > 0)
		{
			foreach (MapSeat mapSeat in this.mapSeats)
			{
				mapSeat.Draw(b);
			}
		}
		Microsoft.Xna.Framework.Rectangle viewportRect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
		viewportRect.Inflate(128, 128);
		if (this is Woods && Game1.eventUp)
		{
			Event obj = this.currentEvent;
			if (obj == null || !obj.showGroundObjects)
			{
				goto IL_014d;
			}
		}
		if (this.resourceClumps.Count > 0)
		{
			foreach (ResourceClump r in this.resourceClumps)
			{
				if (r.getRenderBounds().Intersects(viewportRect))
				{
					r.draw(b);
				}
			}
		}
		goto IL_014d;
		IL_014d:
		this._currentLocationFarmersForDisambiguating.Clear();
		foreach (Farmer farmer in this.farmers)
		{
			farmer.drawLayerDisambiguator = 0f;
			this._currentLocationFarmersForDisambiguating.Add(farmer);
		}
		if (this._currentLocationFarmersForDisambiguating.Contains(Game1.player))
		{
			this._currentLocationFarmersForDisambiguating.Remove(Game1.player);
			this._currentLocationFarmersForDisambiguating.Insert(0, Game1.player);
		}
		float disambiguator_amount = 0.0001f;
		for (int i = 0; i < this._currentLocationFarmersForDisambiguating.Count; i++)
		{
			for (int j = i + 1; j < this._currentLocationFarmersForDisambiguating.Count; j++)
			{
				Farmer farmer2 = this._currentLocationFarmersForDisambiguating[i];
				Farmer other_farmer = this._currentLocationFarmersForDisambiguating[j];
				if (!other_farmer.IsSitting() && Math.Abs(farmer2.getDrawLayer() - other_farmer.getDrawLayer()) < disambiguator_amount && Math.Abs(farmer2.position.X - other_farmer.position.X) < 64f)
				{
					other_farmer.drawLayerDisambiguator += farmer2.getDrawLayer() - disambiguator_amount - other_farmer.getDrawLayer();
				}
			}
		}
		this.drawCharacters(b);
		this.drawFarmers(b);
		if (this.critters != null && Game1.farmEvent == null)
		{
			for (int k = 0; k < this.critters.Count; k++)
			{
				this.critters[k].draw(b);
			}
		}
		this.drawDebris(b);
		if ((!Game1.eventUp || (this.currentEvent != null && this.currentEvent.showGroundObjects)) && this.objects.Length > 0)
		{
			Vector2 tile = default(Vector2);
			for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 3; y++)
			{
				for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; x++)
				{
					tile.X = x;
					tile.Y = y;
					if (this.objects.TryGetValue(tile, out var o))
					{
						o.draw(b, (int)tile.X, (int)tile.Y);
					}
				}
			}
		}
		if (this.TemporarySprites.Count > 0)
		{
			foreach (TemporaryAnimatedSprite s in this.TemporarySprites)
			{
				if (!s.drawAboveAlwaysFront)
				{
					s.draw(b);
				}
			}
		}
		this.interiorDoors.Draw(b);
		NetCollection<LargeTerrainFeature> netCollection = this.largeTerrainFeatures;
		if (netCollection != null && netCollection.Count > 0)
		{
			foreach (LargeTerrainFeature f in this.largeTerrainFeatures)
			{
				if (f.getRenderBounds().Intersects(viewportRect))
				{
					f.draw(b);
				}
			}
		}
		if (this.buildings.Count > 0)
		{
			int borderBuffer = 1;
			viewportRect = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X / 64 - borderBuffer, Game1.viewport.Y / 64 - borderBuffer, (int)Math.Ceiling((float)Game1.viewport.Width / 64f) + 2 * borderBuffer, (int)Math.Ceiling((float)Game1.viewport.Height / 64f) + 3 + 2 * borderBuffer);
			Microsoft.Xna.Framework.Rectangle objectRectangle = default(Microsoft.Xna.Framework.Rectangle);
			foreach (Building building in this.buildings)
			{
				int additionalRadius = building.GetAdditionalTilePropertyRadius();
				Microsoft.Xna.Framework.Rectangle sourceRect = building.getSourceRect();
				objectRectangle.X = building.tileX.Value - additionalRadius;
				objectRectangle.Width = building.tilesWide.Value + additionalRadius * 2;
				int bottomY = building.tileY.Value + building.tilesHigh.Value + additionalRadius;
				objectRectangle.Height = bottomY - (objectRectangle.Y = bottomY - (int)Math.Ceiling((float)sourceRect.Height * 4f / 64f) - additionalRadius);
				if (objectRectangle.Intersects(viewportRect))
				{
					building.draw(b);
				}
			}
		}
		this.fishSplashAnimation?.draw(b);
		this.orePanAnimation?.draw(b);
		if (!Game1.eventUp || this is Farm || this is FarmHouse)
		{
			Furniture.isDrawingLocationFurniture = true;
			foreach (Furniture f2 in this.furniture)
			{
				if (f2.furniture_type.Value != 12)
				{
					f2.draw(b, -1, -1);
				}
			}
			Furniture.isDrawingLocationFurniture = false;
		}
		if (this.showDropboxIndicator && !Game1.eventUp)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.dropBoxIndicatorLocation.X, this.dropBoxIndicatorLocation.Y + yOffset)), new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
		}
		if (this.lightGlows.Count > 0)
		{
			this.drawLightGlows(b);
		}
	}

	public virtual void drawOverlays(SpriteBatch b)
	{
	}

	public virtual void drawAboveFrontLayer(SpriteBatch b)
	{
		Vector2 tile = default(Vector2);
		for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
		{
			for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
			{
				tile.X = x;
				tile.Y = y;
				if (this.terrainFeatures.TryGetValue(tile, out var feat) && !(feat is Flooring))
				{
					feat.draw(b);
				}
			}
		}
	}

	public virtual void drawLightGlows(SpriteBatch b)
	{
		foreach (Vector2 v in this.lightGlows)
		{
			if (!this.lightGlowLayerCache.ContainsKey(v))
			{
				Furniture f = this.GetFurnitureAt(new Vector2((int)(v.X / 64f), (int)(v.Y / 64f) + 2));
				if (f != null && f.sourceRect.Height / 16 - f.getTilesHigh() > 1)
				{
					this.lightGlowLayerCache.Add(v, 2.5f);
				}
				else if (this is FarmHouse { upgradeLevel: >0 } farmhouse)
				{
					Vector2 tileV = new Vector2((int)(v.X / 64f), (int)(v.Y / 64f));
					Vector2 diff = Utility.PointToVector2(farmhouse.getKitchenStandingSpot()) - tileV;
					if (diff.Y == 3f && (diff.X == 2f || diff.X == 3f || diff.X == -1f || diff.X == -2f))
					{
						this.lightGlowLayerCache.Add(v, 1.5f);
					}
					else
					{
						this.lightGlowLayerCache.Add(v, 10f);
					}
				}
				else
				{
					this.lightGlowLayerCache.Add(v, 10f);
				}
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, v), new Microsoft.Xna.Framework.Rectangle(21, 1695, 41, 67), Color.White, 0f, new Vector2(19f, 22f), 4f, SpriteEffects.None, (v.Y + 64f * this.lightGlowLayerCache[v]) / 10000f);
		}
	}

	/// <summary>Try to create a secret note or journal scrap that hasn't been seen by a player, based on the random spawn chance.</summary>
	/// <param name="who">The farmer for whom to create a secret note.</param>
	/// <returns>Returns an unseen secret note/journal scrap, or <see langworld="null" /> if there are none left or the random spawn chance fails.</returns>
	public Object tryToCreateUnseenSecretNote(Farmer who)
	{
		if (this.currentEvent != null && this.currentEvent.isFestival)
		{
			return null;
		}
		bool journal = this.InIslandContext();
		if (!journal && (who == null || !who.hasMagnifyingGlass))
		{
			return null;
		}
		string noteItemId = (journal ? "(O)842" : "(O)79");
		int totalNotes;
		int totalUnseen = Utility.GetUnseenSecretNotes(who, journal, out totalNotes).Length - who.Items.CountId(noteItemId);
		if (totalUnseen <= 0)
		{
			return null;
		}
		float fractionOfNotesRemaining = (float)(totalUnseen - 1) / (float)Math.Max(1, totalNotes - 1);
		float chanceForNewNote = GameLocation.LAST_SECRET_NOTE_CHANCE + (GameLocation.FIRST_SECRET_NOTE_CHANCE - GameLocation.LAST_SECRET_NOTE_CHANCE) * fractionOfNotesRemaining;
		if (!Game1.random.NextBool(chanceForNewNote))
		{
			return null;
		}
		return ItemRegistry.Create<Object>(noteItemId);
	}

	public virtual bool performToolAction(Tool t, int tileX, int tileY)
	{
		if (t is MeleeWeapon weapon)
		{
			foreach (FarmAnimal animal in this.animals.Values)
			{
				if (animal.GetBoundingBox().Intersects(weapon.mostRecentArea))
				{
					animal.hitWithWeapon(weapon);
				}
			}
		}
		foreach (Building building in this.buildings)
		{
			if (building.occupiesTile(new Vector2(tileX, tileY)))
			{
				building.performToolAction(t, tileX, tileY);
			}
		}
		for (int i = this.resourceClumps.Count - 1; i >= 0; i--)
		{
			if (this.resourceClumps[i] != null && this.resourceClumps[i].getBoundingBox().Contains(tileX * 64, tileY * 64) && this.resourceClumps[i].performToolAction(t, 1, this.resourceClumps[i].Tile))
			{
				this.resourceClumps.RemoveAt(i);
				return true;
			}
		}
		Microsoft.Xna.Framework.Rectangle toolArea = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
		foreach (LargeTerrainFeature ltf in this.largeTerrainFeatures)
		{
			if (ltf.getBoundingBox().Intersects(toolArea))
			{
				ltf.performToolAction(t, 1, new Vector2(tileX, tileY));
			}
		}
		return false;
	}

	/// <summary>Update the location when the season changes.</summary>
	/// <param name="onLoad">Whether the season is being initialized as part of loading the save, instead of an actual in-game season change.</param>
	public virtual void seasonUpdate(bool onLoad = false)
	{
		Season season = this.GetSeason();
		this.terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> keyValuePair) => keyValuePair.Value.seasonUpdate(onLoad));
		this.largeTerrainFeatures?.RemoveWhere((LargeTerrainFeature feature) => feature.seasonUpdate(onLoad));
		foreach (NPC n in this.characters)
		{
			if (!n.IsMonster)
			{
				n.resetSeasonalDialogue();
			}
		}
		if (this.IsOutdoors && !onLoad)
		{
			KeyValuePair<Vector2, Object>[] array = this.objects.Pairs.ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				KeyValuePair<Vector2, Object> pair = array[num];
				Vector2 tile = pair.Key;
				Object obj = pair.Value;
				if (obj.IsSpawnedObject && !obj.IsBreakableStone())
				{
					this.objects.Remove(tile);
				}
				else if (obj.QualifiedItemId == "(O)590" && this.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "Diggable", "Back") == "")
				{
					this.objects.Remove(tile);
				}
			}
			this.numberOfSpawnedObjectsOnMap = 0;
		}
		switch (season)
		{
		case Season.Spring:
			this.waterColor.Value = new Color(120, 200, 255) * 0.5f;
			break;
		case Season.Summer:
			this.waterColor.Value = new Color(60, 240, 255) * 0.5f;
			break;
		case Season.Fall:
			this.waterColor.Value = new Color(255, 130, 200) * 0.5f;
			break;
		case Season.Winter:
			this.waterColor.Value = new Color(130, 80, 255) * 0.5f;
			break;
		}
		if (!onLoad && season == Season.Spring && Game1.stats.DaysPlayed > 1 && !(this is Farm))
		{
			this.loadWeeds();
		}
	}

	public List<FarmAnimal> getAllFarmAnimals()
	{
		List<FarmAnimal> farmAnimals = this.animals.Values.ToList();
		foreach (Building building in this.buildings)
		{
			GameLocation interior = building.GetIndoors();
			if (interior != null)
			{
				farmAnimals.AddRange(interior.animals.Values);
			}
		}
		return farmAnimals;
	}

	public virtual int GetHayCapacity()
	{
		int totalCapacity = 0;
		foreach (Building building in this.buildings)
		{
			if (building.hayCapacity.Value > 0 && building.daysOfConstructionLeft.Value <= 0)
			{
				totalCapacity += building.hayCapacity.Value;
			}
		}
		return totalCapacity;
	}

	public bool CheckPetAnimal(Vector2 position, Farmer who)
	{
		foreach (FarmAnimal animal in this.animals.Values)
		{
			if (!animal.wasPet.Value && animal.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
			{
				animal.pet(who);
				return true;
			}
		}
		return false;
	}

	public bool CheckPetAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
	{
		foreach (FarmAnimal animal in this.animals.Values)
		{
			if (!animal.wasPet.Value && animal.GetBoundingBox().Intersects(rect))
			{
				animal.pet(who);
				return true;
			}
		}
		return false;
	}

	public bool CheckInspectAnimal(Vector2 position, Farmer who)
	{
		foreach (FarmAnimal animal in this.animals.Values)
		{
			if (animal.wasPet.Value && animal.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
			{
				animal.pet(who);
				return true;
			}
		}
		return false;
	}

	public bool CheckInspectAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
	{
		foreach (FarmAnimal animal in this.animals.Values)
		{
			if (animal.wasPet.Value && animal.GetBoundingBox().Intersects(rect))
			{
				animal.pet(who);
				return true;
			}
		}
		return false;
	}

	public virtual void updateSeasonalTileSheets(Map map = null)
	{
		if (map == null)
		{
			map = this.Map;
		}
		if (!(this is Summit) && (!this.IsOutdoors || this.Name.Equals("Desert")))
		{
			return;
		}
		map.DisposeTileSheets(Game1.mapDisplayDevice);
		foreach (TileSheet tilesheet in map.TileSheets)
		{
			string prevImageSource = tilesheet.ImageSource;
			try
			{
				tilesheet.ImageSource = GameLocation.GetSeasonalTilesheetName(tilesheet.ImageSource, this.GetSeasonKey());
				Game1.mapDisplayDevice.LoadTileSheet(tilesheet);
			}
			catch (Exception exception)
			{
				Game1.log.Error($"Location '{this.NameOrUniqueName}' failed to load seasonal asset name '{tilesheet.ImageSource}' for tilesheet ID '{tilesheet.Id}'.", exception);
				tilesheet.ImageSource = prevImageSource;
			}
		}
		map.LoadTileSheets(Game1.mapDisplayDevice);
	}

	public static string GetSeasonalTilesheetName(string sheet_path, string current_season)
	{
		string file_name = Path.GetFileName(sheet_path);
		if (file_name.StartsWith("spring_") || file_name.StartsWith("summer_") || file_name.StartsWith("fall_") || file_name.StartsWith("winter_"))
		{
			sheet_path = Path.Combine(Path.GetDirectoryName(sheet_path), current_season + file_name.Substring(file_name.IndexOf('_')));
		}
		return sheet_path;
	}

	public virtual string checkEventPrecondition(string precondition)
	{
		return this.checkEventPrecondition(precondition, check_seen: true);
	}

	public virtual string checkEventPrecondition(string precondition, bool check_seen)
	{
		string[] split = Event.SplitPreconditions(precondition);
		string eventId = split[0];
		if (string.IsNullOrEmpty(eventId) || eventId == "-1")
		{
			return "-1";
		}
		if (check_seen && (Game1.player.eventsSeen.Contains(eventId) || Game1.eventsSeenSinceLastLocationChange.Contains(eventId)))
		{
			return "-1";
		}
		for (int i = 1; i < split.Length; i++)
		{
			if (!string.IsNullOrEmpty(split[i]) && !Event.CheckPrecondition(this, split[0], split[i]))
			{
				return "-1";
			}
		}
		return eventId;
	}

	/// <summary>Get hay from any non-empty silos.</summary>
	/// <param name="currentLocation">The location in which the hay was found.</param>
	public static Object GetHayFromAnySilo(GameLocation currentLocation)
	{
		if (TryGetHayFrom(currentLocation, out var hay))
		{
			return hay;
		}
		if (currentLocation.Name != "Farm" && TryGetHayFrom(Game1.getFarm(), out hay))
		{
			return hay;
		}
		Utility.ForEachLocation((GameLocation location) => !TryGetHayFrom(location, out hay), includeInteriors: false);
		return hay;
		static bool TryGetHayFrom(GameLocation location, out Object foundHay)
		{
			if (location.piecesOfHay.Value < 1)
			{
				foundHay = null;
				return false;
			}
			foundHay = ItemRegistry.Create<Object>("(O)178");
			location.piecesOfHay.Value--;
			return true;
		}
	}

	/// <summary>Store hay in any silos that have available space.</summary>
	/// <param name="count">The number of hay items to store.</param>
	/// <param name="currentLocation">The location in which the hay was found.</param>
	/// <returns>Returns the number of hay that couldn't be stored.</returns>
	public static int StoreHayInAnySilo(int count, GameLocation currentLocation)
	{
		count = currentLocation.tryToAddHay(count);
		if (count > 0 && currentLocation.Name != "Farm")
		{
			count = Game1.getFarm().tryToAddHay(count);
			if (count <= 0)
			{
				return 0;
			}
		}
		if (count > 0)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.buildings.Count > 0)
				{
					count = location.tryToAddHay(count);
					return count > 0;
				}
				return true;
			}, includeInteriors: false);
		}
		if (count <= 0)
		{
			return 0;
		}
		return count;
	}

	/// <summary>Store hay in the current location's silos, if they have space available.</summary>
	/// <param name="num">The number of hay items to store.</param>
	/// <returns>Returns the number of hay that couldn't be stored.</returns>
	public int tryToAddHay(int num)
	{
		int piecesToAdd = Math.Min(this.GetHayCapacity() - this.piecesOfHay.Value, num);
		this.piecesOfHay.Value += piecesToAdd;
		return num - piecesToAdd;
	}

	public Building getBuildingAt(Vector2 tile)
	{
		foreach (Building building in this.buildings)
		{
			if (building.occupiesTile(tile) || !building.isTilePassable(tile))
			{
				return building;
			}
		}
		return null;
	}

	/// <summary>Get a building by its <see cref="F:StardewValley.Buildings.Building.buildingType" /> value.</summary>
	/// <param name="id">The building type key.</param>
	public Building getBuildingByType(string type)
	{
		if (type != null)
		{
			foreach (Building building in this.buildings)
			{
				if (string.Equals(building.buildingType.Value, type, StringComparison.Ordinal))
				{
					return building;
				}
			}
		}
		return null;
	}

	/// <summary>Get a building by its <see cref="F:StardewValley.Buildings.Building.id" /> value.</summary>
	/// <param name="id">The unique building ID.</param>
	public Building getBuildingById(Guid id)
	{
		if (id != Guid.Empty)
		{
			foreach (Building building in this.buildings)
			{
				if (building.id.Value == id)
				{
					return building;
				}
			}
		}
		return null;
	}

	/// <summary>Get a building by the unique name of its interior location.</summary>
	/// <param name="id">The building interior location's unique name.</param>
	public Building getBuildingByName(string name)
	{
		if (name != null)
		{
			foreach (Building building in this.buildings)
			{
				if (building.HasIndoorsName(name))
				{
					return building;
				}
			}
		}
		return null;
	}

	public bool destroyStructure(Vector2 tile)
	{
		Building building = this.getBuildingAt(tile);
		if (building != null)
		{
			return this.destroyStructure(building);
		}
		return false;
	}

	public bool destroyStructure(Building building)
	{
		if (this.buildings.Remove(building))
		{
			building.performActionOnDemolition(this);
			Game1.player.team.SendBuildingDemolishedEvent(this, building);
			return true;
		}
		return false;
	}

	public bool buildStructure(Building building, Vector2 tileLocation, Farmer who, bool skipSafetyChecks = false)
	{
		if (!skipSafetyChecks)
		{
			for (int y = 0; y < building.tilesHigh.Value; y++)
			{
				for (int x = 0; x < building.tilesWide.Value; x++)
				{
					this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y));
				}
			}
			foreach (BuildingPlacementTile additionalPlacementTile in building.GetAdditionalPlacementTiles())
			{
				foreach (Point areaTile in additionalPlacementTile.TileArea.GetPoints())
				{
					this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)areaTile.X, tileLocation.Y + (float)areaTile.Y));
				}
			}
			for (int i = 0; i < building.tilesHigh.Value; i++)
			{
				for (int j = 0; j < building.tilesWide.Value; j++)
				{
					Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)j, tileLocation.Y + (float)i);
					if (this.buildings.Contains(building) && building.occupiesTile(currentGlobalTilePosition))
					{
						continue;
					}
					if (!this.isBuildable(currentGlobalTilePosition))
					{
						return false;
					}
					foreach (Farmer farmer in this.farmers)
					{
						if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(j * 64, i * 64, 64, 64)))
						{
							return false;
						}
					}
				}
			}
			foreach (BuildingPlacementTile additionalPlacementTile2 in building.GetAdditionalPlacementTiles())
			{
				bool onlyNeedsToBePassable = additionalPlacementTile2.OnlyNeedsToBePassable;
				foreach (Point point in additionalPlacementTile2.TileArea.GetPoints())
				{
					int x2 = point.X;
					int y2 = point.Y;
					Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y2);
					if (this.buildings.Contains(building) && building.occupiesTile(currentGlobalTilePosition2))
					{
						continue;
					}
					if (!this.isBuildable(currentGlobalTilePosition2, onlyNeedsToBePassable))
					{
						return false;
					}
					if (onlyNeedsToBePassable)
					{
						continue;
					}
					foreach (Farmer farmer2 in this.farmers)
					{
						if (farmer2.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x2 * 64, y2 * 64, 64, 64)))
						{
							return false;
						}
					}
				}
			}
			if (building.humanDoor.Value != new Point(-1, -1))
			{
				Vector2 doorPos = tileLocation + new Vector2(building.humanDoor.X, building.humanDoor.Y + 1);
				if ((!this.buildings.Contains(building) || !building.occupiesTile(doorPos)) && !this.isBuildable(doorPos) && !this.isPath(doorPos))
				{
					return false;
				}
			}
			string finalCheckResult = building.isThereAnythingtoPreventConstruction(this, tileLocation);
			if (finalCheckResult != null)
			{
				Game1.addHUDMessage(new HUDMessage(finalCheckResult, 3));
				return false;
			}
		}
		building.tileX.Value = (int)tileLocation.X;
		building.tileY.Value = (int)tileLocation.Y;
		for (int k = 0; k < building.tilesHigh.Value; k++)
		{
			for (int l = 0; l < building.tilesWide.Value; l++)
			{
				Vector2 currentGlobalTilePosition3 = new Vector2(tileLocation.X + (float)l, tileLocation.Y + (float)k);
				if (!(this.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition3) is Flooring) || !(building.GetData()?.AllowsFlooringUnderneath ?? false))
				{
					this.terrainFeatures.Remove(currentGlobalTilePosition3);
				}
			}
		}
		if (!this.buildings.Contains(building))
		{
			this.buildings.Add(building);
			who.team.SendBuildingConstructedEvent(this, building, who);
		}
		GameLocation interior = building.GetIndoors();
		if (interior is AnimalHouse animalHouse)
		{
			foreach (long animalId in animalHouse.animalsThatLiveHere)
			{
				FarmAnimal animal = Utility.getAnimal(animalId);
				if (animal != null)
				{
					animal.homeInterior = interior;
				}
				else if (animalHouse.animals.TryGetValue(animalId, out animal))
				{
					animal.homeInterior = interior;
				}
			}
		}
		if (interior != null)
		{
			foreach (Warp warp in interior.warps)
			{
				if (warp.TargetName == this.NameOrUniqueName)
				{
					warp.TargetX = building.humanDoor.X + building.tileX.Value;
					warp.TargetY = building.humanDoor.Y + building.tileY.Value + 1;
				}
			}
		}
		for (int m = 0; m < building.tilesHigh.Value; m++)
		{
			for (int n = 0; n < building.tilesWide.Value; n++)
			{
				RemoveArtifactSpots(new Vector2(tileLocation.X + (float)n, tileLocation.Y + (float)m));
			}
		}
		foreach (BuildingPlacementTile area in building.GetAdditionalPlacementTiles())
		{
			if (area.OnlyNeedsToBePassable)
			{
				continue;
			}
			foreach (Point areaTile2 in area.TileArea.GetPoints())
			{
				RemoveArtifactSpots(new Vector2(tileLocation.X + (float)areaTile2.X, tileLocation.Y + (float)areaTile2.Y));
			}
		}
		return true;
		void RemoveArtifactSpots(Vector2 tile_location)
		{
			if (this.getObjectAtTile((int)tile_location.X, (int)tile_location.Y)?.QualifiedItemId == "(O)590")
			{
				this.removeObject(tile_location, showDestroyedObject: false);
			}
		}
	}

	/// <summary>Construct a building in the location.</summary>
	/// <param name="typeId">The building type ID in <c>Data/Buildings</c>.</param>
	/// <param name="data">The building data from <c>Data/Buildings</c>.</param>
	/// <param name="tileLocation">The top-left tile position of the building.</param>
	/// <param name="who">The player constructing the building.</param>
	/// <param name="magicalConstruction">Whether construction should complete instantly.</param>
	/// <param name="skipSafetyChecks">Whether to ignore safety checks (e.g. making sure the area is clear).</param>
	/// <returns>Returns whether the building was successfully placed.</returns>
	public bool buildStructure(string typeId, BuildingData data, Vector2 tileLocation, Farmer who, out Building constructed, bool magicalConstruction = false, bool skipSafetyChecks = false)
	{
		if (data == null || (!skipSafetyChecks && !this.IsBuildableLocation()))
		{
			constructed = null;
			return false;
		}
		int tilesWide = data.Size.X;
		int tilesHigh = data.Size.Y;
		List<BuildingPlacementTile> additionalPlacementTiles = data.AdditionalPlacementTiles ?? new List<BuildingPlacementTile>(0);
		if (!skipSafetyChecks)
		{
			for (int y = 0; y < tilesHigh; y++)
			{
				for (int x = 0; x < tilesWide; x++)
				{
					this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)x, tileLocation.Y + (float)y));
				}
			}
			foreach (BuildingPlacementTile item in additionalPlacementTiles)
			{
				foreach (Point areaTile in item.TileArea.GetPoints())
				{
					this.pokeTileForConstruction(new Vector2(tileLocation.X + (float)areaTile.X, tileLocation.Y + (float)areaTile.Y));
				}
			}
			for (int i = 0; i < tilesHigh; i++)
			{
				for (int j = 0; j < tilesWide; j++)
				{
					Vector2 currentGlobalTilePosition = new Vector2(tileLocation.X + (float)j, tileLocation.Y + (float)i);
					if (!this.isBuildable(currentGlobalTilePosition))
					{
						constructed = null;
						return false;
					}
					foreach (Farmer farmer in this.farmers)
					{
						if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(j * 64, i * 64, 64, 64)))
						{
							constructed = null;
							return false;
						}
					}
				}
			}
			foreach (BuildingPlacementTile item2 in additionalPlacementTiles)
			{
				bool onlyNeedsToBePassable = item2.OnlyNeedsToBePassable;
				foreach (Point point in item2.TileArea.GetPoints())
				{
					int x2 = point.X;
					int y2 = point.Y;
					Vector2 currentGlobalTilePosition2 = new Vector2(tileLocation.X + (float)x2, tileLocation.Y + (float)y2);
					if (!this.isBuildable(currentGlobalTilePosition2, onlyNeedsToBePassable))
					{
						constructed = null;
						return false;
					}
					if (onlyNeedsToBePassable)
					{
						continue;
					}
					foreach (Farmer farmer2 in this.farmers)
					{
						if (farmer2.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(x2 * 64, y2 * 64, 64, 64)))
						{
							constructed = null;
							return false;
						}
					}
				}
			}
			if (data.HumanDoor != new Point(-1, -1))
			{
				Vector2 doorPos = tileLocation + new Vector2(data.HumanDoor.X, data.HumanDoor.Y + 1);
				if (!this.isBuildable(doorPos, onlyNeedsToBePassable: true) && !this.isPath(doorPos))
				{
					constructed = null;
					return false;
				}
			}
		}
		Building building = Building.CreateInstanceFromId(typeId, tileLocation);
		if (magicalConstruction)
		{
			building.magical.Value = true;
			building.daysOfConstructionLeft.Value = 0;
		}
		building.owner.Value = who.UniqueMultiplayerID;
		if (!skipSafetyChecks)
		{
			string finalCheckResult = building.isThereAnythingtoPreventConstruction(this, tileLocation);
			if (finalCheckResult != null)
			{
				Game1.addHUDMessage(new HUDMessage(finalCheckResult, 3));
				constructed = null;
				return false;
			}
		}
		for (int k = 0; k < building.tilesHigh.Value; k++)
		{
			for (int l = 0; l < building.tilesWide.Value; l++)
			{
				Vector2 currentGlobalTilePosition3 = new Vector2(tileLocation.X + (float)l, tileLocation.Y + (float)k);
				if (!(this.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition3) is Flooring) || !(building.GetData()?.AllowsFlooringUnderneath ?? false))
				{
					this.terrainFeatures.Remove(currentGlobalTilePosition3);
				}
			}
		}
		this.buildings.Add(building);
		who.team.SendBuildingConstructedEvent(this, building, who);
		string chatKey = (magicalConstruction ? "BuildingMagicBuild" : "BuildingBuild");
		Game1.multiplayer.globalChatInfoMessage(chatKey, Game1.player.Name, "aOrAn:" + data.Name, data.Name, Game1.player.farmName.Value);
		constructed = building;
		return true;
	}

	/// <summary>Construct a building in the location.</summary>
	/// <param name="typeId">The building type ID in <c>Data/Buildings</c>.</param>
	/// <param name="tileLocation">The top-left tile position of the building.</param>
	/// <param name="who">The player constructing the building.</param>
	/// <param name="magicalConstruction">Whether construction should complete instantly.</param>
	/// <param name="skipSafetyChecks">Whether to ignore safety checks (e.g. making sure the area is clear).</param>
	/// <returns>Returns whether the building was successfully placed.</returns>
	public bool buildStructure(string typeId, Vector2 tileLocation, Farmer who, out Building constructed, bool magicalConstruction = false, bool skipSafetyChecks = false)
	{
		if (typeId == null || !Game1.buildingData.TryGetValue(typeId, out var buildingData))
		{
			Game1.log.Error("Can't construct building '" + typeId + "', no data found matching that ID.");
			constructed = null;
			return false;
		}
		return this.buildStructure(typeId, buildingData, tileLocation, who, out constructed, magicalConstruction, skipSafetyChecks);
	}

	/// <summary>Get whether the location contains any buildings of the given type.</summary>
	/// <param name="name">The building type's ID in <c>Data/Buildings</c>.</param>
	public bool isBuildingConstructed(string name)
	{
		return this.getNumberBuildingsConstructed(name) > 0;
	}

	/// <summary>Get whether the location has a minimum number of matching buildings.</summary>
	/// <param name="buildingType">The building type to count.</param>
	/// <param name="minCount">The minimum number needed.</param>
	public bool HasMinBuildings(string buildingType, int minCount)
	{
		return this.getNumberBuildingsConstructed(buildingType) >= minCount;
	}

	/// <summary>Get whether the location has a minimum number of matching buildings.</summary>
	/// <param name="match">A filter which matches buildings to count.</param>
	/// <param name="minCount">The minimum number needed.</param>
	public bool HasMinBuildings(Func<Building, bool> match, int minCount)
	{
		if (minCount <= 0)
		{
			return true;
		}
		int count = 0;
		foreach (Building building in this.buildings)
		{
			if (match(building))
			{
				count++;
			}
			if (count >= minCount)
			{
				return true;
			}
		}
		return false;
	}

	public int getNumberBuildingsConstructed(bool includeUnderConstruction = false)
	{
		if (includeUnderConstruction || this.buildings.Count == 0)
		{
			return this.buildings.Count;
		}
		int count = 0;
		foreach (Building building in this.buildings)
		{
			if (!building.isUnderConstruction())
			{
				count++;
			}
		}
		return count;
	}

	public int getNumberBuildingsConstructed(string name, bool includeUnderConstruction = false)
	{
		int count = 0;
		if (this.buildings.Count > 0)
		{
			foreach (Building building in this.buildings)
			{
				if (building.buildingType.Value == name && (includeUnderConstruction || !building.isUnderConstruction()))
				{
					count++;
				}
			}
		}
		return count;
	}

	public bool isThereABuildingUnderConstruction()
	{
		if (this.buildings.Count > 0)
		{
			foreach (Building building in this.buildings)
			{
				if (building.isUnderConstruction())
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Get all building interiors within this location which are instanced to the building (i.e. not in <see cref="P:StardewValley.Game1.locations" /> separately).</summary>
	public IEnumerable<GameLocation> GetInstancedBuildingInteriors()
	{
		List<GameLocation> interiors = null;
		this.ForEachInstancedInterior(delegate(GameLocation location)
		{
			if (interiors == null)
			{
				interiors = new List<GameLocation>();
			}
			interiors.Add(location);
			return true;
		});
		if (interiors == null)
		{
			return LegacyShims.EmptyArray<GameLocation>();
		}
		return interiors;
	}

	/// <summary>Perform an action for each building interior within this location which is instanced to the building (i.e. not in <see cref="P:StardewValley.Game1.locations" /> separately).</summary>
	/// <param name="action">The action to perform for each interior. This should return true (continue iterating) or false (stop).</param>
	public void ForEachInstancedInterior(Func<GameLocation, bool> action)
	{
		foreach (Building building in this.buildings)
		{
			if (building.GetIndoorsType() == IndoorsType.Instanced)
			{
				GameLocation indoors = building.GetIndoors();
				if (indoors != null && !action(indoors))
				{
					break;
				}
			}
		}
	}

	/// <summary>Perform an action for each tilled dirt in the location.</summary>
	/// <param name="action">The action to perform.</param>
	/// <param name="includeGardenPots">Whether to apply the action to dirt in garden pots.</param>
	public void ForEachDirt(Func<HoeDirt, bool> action, bool includeGardenPots = true)
	{
		foreach (TerrainFeature value in this.terrainFeatures.Values)
		{
			if (value is HoeDirt dirt && !action(dirt))
			{
				return;
			}
		}
		if (!includeGardenPots)
		{
			return;
		}
		using Dictionary<Vector2, Object>.ValueCollection.Enumerator enumerator2 = this.objects.Values.GetEnumerator();
		while (enumerator2.MoveNext() && (!(enumerator2.Current is IndoorPot pot) || pot.bush.Value != null || action(pot.hoeDirt.Value)))
		{
		}
	}

	public bool isPath(Vector2 tileLocation)
	{
		if (this.terrainFeatures.TryGetValue(tileLocation, out var terrainFeature) && terrainFeature != null && terrainFeature.isPassable())
		{
			if (this.objects.TryGetValue(tileLocation, out var obj) && obj != null)
			{
				return obj.isPassable();
			}
			return true;
		}
		return false;
	}

	public bool isBuildable(Vector2 tileLocation, bool onlyNeedsToBePassable = false)
	{
		Microsoft.Xna.Framework.Rectangle validRect = this.GetBuildableRectangle();
		if (validRect != Microsoft.Xna.Framework.Rectangle.Empty && !validRect.Contains((int)tileLocation.X, (int)tileLocation.Y))
		{
			return false;
		}
		if (onlyNeedsToBePassable)
		{
			if (this.isTilePassable(tileLocation))
			{
				return !this.IsTileOccupiedBy(tileLocation, CollisionMask.All, CollisionMask.All);
			}
			return false;
		}
		Building buildingAtTile = this.getBuildingAt(tileLocation);
		if (buildingAtTile != null && !buildingAtTile.isMoving)
		{
			return false;
		}
		if (this.CanItemBePlacedHere(tileLocation, itemIsPassable: false, CollisionMask.All, ~CollisionMask.Objects, useFarmerTile: true) || this.getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y)?.QualifiedItemId == "(O)590")
		{
			if (this._looserBuildRestrictions)
			{
				return !Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").EqualsIgnoreCase("f");
			}
			if (Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").EqualsIgnoreCase("t") || Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").ToLower().Equals("true"))
			{
				return true;
			}
			if (Game1.currentLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") != null && !Game1.currentLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Buildable", "Back").EqualsIgnoreCase("f"))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void pokeTileForConstruction(Vector2 tile)
	{
		foreach (FarmAnimal animal in this.animals.Values)
		{
			if (animal.Tile == tile)
			{
				animal.Poke();
			}
		}
	}

	public virtual void updateWarps()
	{
		if (Game1.IsClient)
		{
			return;
		}
		this.warps.Clear();
		string[] array = new string[2] { "NPCWarp", "Warp" };
		foreach (string propertyName in array)
		{
			if (!this.map.Properties.TryGetValue(propertyName, out var warpsUnparsed) || warpsUnparsed == null)
			{
				continue;
			}
			bool npcOnly = propertyName == "NPCWarp";
			string[] fields = ArgUtility.SplitBySpace(warpsUnparsed);
			for (int j = 0; j < fields.Length; j += 5)
			{
				bool hasFields = fields.Length >= j + 5;
				if (!hasFields || !int.TryParse(fields[j], out var fromX) || !int.TryParse(fields[j + 1], out var fromY) || !int.TryParse(fields[j + 3], out var toX) || !int.TryParse(fields[j + 4], out var toY))
				{
					Game1.log.Warn($"Failed parsing {(npcOnly ? "NPC warp" : "warp")} '{string.Join(" ", fields.Skip(j))}' for location '{this.NameOrUniqueName}'. Warps must have five fields in the form 'fromX fromY toLocationName toX toY', but " + ((!hasFields) ? "got insufficient fields." : "got a non-numeric value for one of the X/Y position fields."));
				}
				else
				{
					this.warps.Add(new Warp(fromX, fromY, fields[j + 2], toX, toY, flipFarmer: false, npcOnly));
				}
			}
		}
		if (this.warps.Count > 0)
		{
			this.ParentBuilding?.updateInteriorWarps(this);
		}
	}

	public void loadWeeds()
	{
		if (!this.isOutdoors.Value && !this.treatAsOutdoors.Value)
		{
			return;
		}
		Layer pathsLayer = this.map?.GetLayer("Paths");
		if (pathsLayer == null)
		{
			return;
		}
		for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
		{
			for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
			{
				int tileIndex = pathsLayer.GetTileIndexAt(x, y);
				if (tileIndex == -1)
				{
					continue;
				}
				Vector2 tile = new Vector2(x, y);
				switch (tileIndex)
				{
				case 13:
				case 14:
				case 15:
					if (this.CanLoadPathObjectHere(tile))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, this.GetSeason())));
					}
					break;
				case 16:
					if (this.CanLoadPathObjectHere(tile))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 17:
					if (this.CanLoadPathObjectHere(tile))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 18:
					if (this.CanLoadPathObjectHere(tile))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295")));
					}
					break;
				}
			}
		}
	}

	/// <summary>Get whether a tile is unoccupied for the purposes of spawning weed/stone debris from the <c>Paths</c> layer.</summary>
	/// <param name="tile">The tile position.</param>
	public bool CanLoadPathObjectHere(Vector2 tile)
	{
		if (this.IsTileOccupiedBy(tile, CollisionMask.Buildings | CollisionMask.Flooring | CollisionMask.Objects | CollisionMask.TerrainFeatures))
		{
			return false;
		}
		Vector2 tile_center = tile * 64f;
		tile_center.X += 32f;
		tile_center.Y += 32f;
		foreach (Furniture f in this.furniture)
		{
			if (f.furniture_type.Value != 12 && !f.isPassable() && f.GetBoundingBox().Contains((int)tile_center.X, (int)tile_center.Y) && !f.AllowPlacementOnThisTile((int)tile.X, (int)tile.Y))
			{
				return false;
			}
		}
		return true;
	}

	public void loadObjects()
	{
		this._startingCabinLocations.Clear();
		if (this.map == null)
		{
			return;
		}
		this.updateWarps();
		Layer pathsLayer = this.map.GetLayer("Paths");
		string[] trees = this.GetMapPropertySplitBySpaces("Trees");
		for (int i = 0; i < trees.Length; i += 3)
		{
			if (!ArgUtility.TryGetVector2(trees, i, out var position, out var error, integerOnly: false, "Vector2 position") || !ArgUtility.TryGetInt(trees, i + 2, out var treeType, out error, "int treeType"))
			{
				this.LogMapPropertyError("Trees", trees, error);
			}
			else
			{
				this.terrainFeatures.Add(position, new Tree((treeType + 1).ToString(), 5));
			}
		}
		if (pathsLayer != null && this.TryGetMapProperty("LoadTreesFrom", out var parentTreeLocation))
		{
			GameLocation parentTreeMap = Game1.getLocationFromName(parentTreeLocation);
			if (parentTreeMap != null)
			{
				foreach (KeyValuePair<Vector2, TerrainFeature> pair in parentTreeMap.terrainFeatures.Pairs)
				{
					if (pair.Value is Tree tree)
					{
						Point p = new Point((int)pair.Key.X, (int)pair.Key.Y);
						if (pathsLayer.HasTileAt(p.X, p.Y) && this.TryGetTreeIdForTile(pathsLayer.Tiles[p.X, p.Y], out var _, out var _, out var _, out var _))
						{
							this.terrainFeatures.Add(pair.Key, new Tree(tree.treeType.Value, tree.growthStage.Value));
						}
					}
				}
			}
		}
		if ((this.isOutdoors.Value || this.name.Equals("BathHouse_Entry") || this.treatAsOutdoors.Value || this.map.Properties.ContainsKey("forceLoadObjects")) && pathsLayer != null)
		{
			this.loadPathsLayerObjectsInArea(0, 0, this.map.Layers[0].LayerWidth, this.map.Layers[0].LayerHeight);
			if (!Game1.eventUp && this.HasMapPropertyWithValue(this.GetSeason().ToString() + "_Objects"))
			{
				this.spawnObjects();
			}
		}
		this.updateDoors();
	}

	public void loadPathsLayerObjectsInArea(int startingX, int startingY, int width, int height)
	{
		Layer pathsLayer = this.map.GetLayer("Paths");
		for (int x = startingX; x < startingX + width; x++)
		{
			for (int y = startingY; y < startingY + height; y++)
			{
				Tile t = pathsLayer.Tiles[x, y];
				if (t == null)
				{
					continue;
				}
				Vector2 tile = new Vector2(x, y);
				if (this.TryGetTreeIdForTile(t, out var treeId, out var growthStageOnLoad, out var _, out var isFruitTree))
				{
					if (this.GetFurnitureAt(tile) == null && !this.terrainFeatures.ContainsKey(tile) && !this.objects.ContainsKey(tile))
					{
						if (isFruitTree)
						{
							this.terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnLoad ?? 4));
						}
						else
						{
							this.terrainFeatures.Add(tile, new Tree(treeId, growthStageOnLoad ?? 5));
						}
					}
					continue;
				}
				switch (t.TileIndex)
				{
				case 13:
				case 14:
				case 15:
					if (!this.objects.ContainsKey(tile) && (!this.IsOutdoors || !Game1.IsWinter))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, this.GetSeason())));
					}
					break;
				case 16:
					if (!this.objects.ContainsKey(tile))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 17:
					if (!this.objects.ContainsKey(tile))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450")));
					}
					break;
				case 18:
					if (!this.objects.ContainsKey(tile))
					{
						this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295")));
					}
					break;
				case 19:
					this.addResourceClumpAndRemoveUnderlyingTerrain(602, 2, 2, tile);
					break;
				case 20:
					this.addResourceClumpAndRemoveUnderlyingTerrain(672, 2, 2, tile);
					break;
				case 21:
					this.addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, tile);
					break;
				case 22:
				case 36:
				{
					if (this.terrainFeatures.ContainsKey(tile))
					{
						break;
					}
					Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
					tileRect.Inflate(-1, -1);
					bool fail = false;
					foreach (ResourceClump resourceClump in this.resourceClumps)
					{
						if (resourceClump.getBoundingBox().Intersects(tileRect))
						{
							fail = true;
							break;
						}
					}
					if (!fail)
					{
						this.terrainFeatures.Add(tile, new Grass((t.TileIndex != 36) ? 1 : 7, 3));
					}
					break;
				}
				case 23:
					if (!this.terrainFeatures.ContainsKey(tile))
					{
						this.terrainFeatures.Add(tile, new Tree(Game1.random.Next(1, 4).ToString(), Game1.random.Next(2, 4)));
					}
					break;
				case 24:
					if (!this.terrainFeatures.ContainsKey(tile))
					{
						this.largeTerrainFeatures.Add(new Bush(tile, 2, this));
					}
					break;
				case 25:
					if (!this.terrainFeatures.ContainsKey(tile))
					{
						this.largeTerrainFeatures.Add(new Bush(tile, 1, this));
					}
					break;
				case 26:
					if (!this.terrainFeatures.ContainsKey(tile))
					{
						this.largeTerrainFeatures.Add(new Bush(tile, 0, this));
					}
					break;
				case 33:
					if (!this.terrainFeatures.ContainsKey(tile))
					{
						this.largeTerrainFeatures.Add(new Bush(tile, 4, this));
					}
					break;
				case 27:
					this.changeMapProperties("BrookSounds", tile.X + " " + tile.Y + " 0");
					break;
				case 29:
				case 30:
				{
					if (Game1.startingCabins > 0 && t.Properties.TryGetValue("Order", out var rawOrder) && int.Parse(rawOrder) <= Game1.startingCabins && ((t.TileIndex == 29 && !Game1.cabinsSeparate) || (t.TileIndex == 30 && Game1.cabinsSeparate)))
					{
						this._startingCabinLocations.Add(tile);
					}
					break;
				}
				}
			}
		}
	}

	/// <summary>Get the tree to spawn on a tile based on its tile index on the <c>Paths</c> layer, if any.</summary>
	/// <param name="tileIndex">The tile index on the <c>Paths</c> layer.</param>
	/// <param name="treeId">The tree ID in <c>Data/FruitTrees</c> or <c>Data/WildTrees</c> that should spawn.</param>
	/// <param name="growthStageOnLoad">The preferred tree growth stage when first populating the save, if applicable.</param>
	/// <param name="growthStageOnRegrow">The preferred tree growth stage when regrowing trees on day update, if applicable.</param>
	/// <param name="isFruitTree">Whether to spawn a fruit tree (<c>true</c>) or wild tree (<c>false</c>).</param>
	/// <returns>Returns whether a tree should spawn here.</returns>
	public bool TryGetTreeIdForTile(Tile tile, out string treeId, out int? growthStageOnLoad, out int? growthStageOnRegrow, out bool isFruitTree)
	{
		isFruitTree = false;
		growthStageOnLoad = null;
		growthStageOnRegrow = null;
		if (tile == null)
		{
			treeId = null;
			return false;
		}
		switch (tile.TileIndex)
		{
		case 9:
			treeId = (this.IsWinterHere() ? "4" : "1");
			return true;
		case 10:
			treeId = (this.IsWinterHere() ? "5" : "2");
			return true;
		case 11:
			treeId = "3";
			return true;
		case 12:
			treeId = "6";
			return true;
		case 31:
			treeId = "9";
			return true;
		case 32:
			treeId = "8";
			return true;
		case 34:
		{
			if (!tile.Properties.TryGetValue("SpawnTree", out var property))
			{
				Game1.log.Warn($"Location '{this.NameOrUniqueName}' ignored path tile index 34 (spawn tree) at position {tile} because the tile has no '{"SpawnTree"}' tile property.");
				break;
			}
			string[] args = ArgUtility.SplitBySpace(property);
			if (!ArgUtility.TryGet(args, 0, out var rawType, out var error, allowBlank: true, "string rawType") || !ArgUtility.TryGet(args, 1, out var rawId, out error, allowBlank: true, "string rawId") || !ArgUtility.TryGetOptionalInt(args, 2, out var rawGrowthStageOnLoad, out error, -1, "int rawGrowthStageOnLoad") || !ArgUtility.TryGetOptionalInt(args, 3, out var rawGrowthStageOnRegrow, out error, -1, "int rawGrowthStageOnRegrow"))
			{
				Game1.log.Warn($"Location '{this.NameOrUniqueName}' ignored path tile index 34 (spawn tree) at position {tile} because the '{"SpawnTree"}' tile property is invalid: {error}.");
				break;
			}
			if (rawGrowthStageOnLoad > -1)
			{
				growthStageOnLoad = rawGrowthStageOnLoad;
			}
			if (rawGrowthStageOnRegrow > -1)
			{
				growthStageOnRegrow = rawGrowthStageOnRegrow;
			}
			if (rawType.EqualsIgnoreCase("wild"))
			{
				treeId = rawId;
				return true;
			}
			if (rawType.EqualsIgnoreCase("fruit"))
			{
				treeId = rawId;
				isFruitTree = true;
				return true;
			}
			Game1.log.Warn($"Location '{this.NameOrUniqueName}' ignored path tile index 34 (spawn tree) at position {tile} because the '{"SpawnTree"}' tile property has invalid type '{rawType}' (expected 'fruit' or 'wild').");
			break;
		}
		}
		growthStageOnLoad = null;
		growthStageOnRegrow = null;
		treeId = null;
		return false;
	}

	public void BuildStartingCabins()
	{
		if (this._startingCabinLocations.Count > 0)
		{
			List<string> cabinStyleOrder = new List<string>();
			switch (Game1.whichFarm)
			{
			case 3:
			case 4:
				cabinStyleOrder.Add("Stone Cabin");
				cabinStyleOrder.Add("Log Cabin");
				cabinStyleOrder.Add("Plank Cabin");
				cabinStyleOrder.Add("Rustic Cabin");
				cabinStyleOrder.Add("Trailer Cabin");
				cabinStyleOrder.Add("Neighbor Cabin");
				cabinStyleOrder.Add("Beach Cabin");
				break;
			case 1:
				cabinStyleOrder.Add("Beach Cabin");
				cabinStyleOrder.Add("Plank Cabin");
				cabinStyleOrder.Add("Log Cabin");
				cabinStyleOrder.Add("Neighbor Cabin");
				cabinStyleOrder.Add("Trailer Cabin");
				cabinStyleOrder.Add("Stone Cabin");
				cabinStyleOrder.Add("Rustic Cabin");
				break;
			default:
			{
				bool logFirst = Game1.random.NextBool();
				cabinStyleOrder.Add(logFirst ? "Log Cabin" : "Plank Cabin");
				cabinStyleOrder.Add("Stone Cabin");
				cabinStyleOrder.Add(logFirst ? "Plank Cabin" : "Log Cabin");
				cabinStyleOrder.Add("Trailer Cabin");
				cabinStyleOrder.Add("Neighbor Cabin");
				cabinStyleOrder.Add("Rustic Cabin");
				cabinStyleOrder.Add("Beach Cabin");
				break;
			}
			}
			List<Vector2> startingCabinsInOrder = new List<Vector2>();
			for (int i = 0; i < this._startingCabinLocations.Count; i++)
			{
				for (int j = 0; j < this._startingCabinLocations.Count; j++)
				{
					if (this.doesTileHavePropertyNoNull((int)this._startingCabinLocations[j].X, (int)this._startingCabinLocations[j].Y, "Order", "Paths").Equals((i + 1).ToString() ?? ""))
					{
						startingCabinsInOrder.Add(this._startingCabinLocations[j]);
					}
				}
			}
			for (int k = 0; k < startingCabinsInOrder.Count; k++)
			{
				this.removeObjectsAndSpawned((int)startingCabinsInOrder[k].X, (int)startingCabinsInOrder[k].Y, 5, 3);
				this.removeObjectsAndSpawned((int)startingCabinsInOrder[k].X + 2, (int)startingCabinsInOrder[k].Y + 3, 1, 1);
				Building b = new Building("Cabin", startingCabinsInOrder[k]);
				b.magical.Value = true;
				b.skinId.Value = cabinStyleOrder[k % cabinStyleOrder.Count];
				b.daysOfConstructionLeft.Value = 0;
				b.load();
				this.buildStructure(b, startingCabinsInOrder[k], Game1.player, skipSafetyChecks: true);
				b.removeOverlappingBushes(this);
			}
		}
		this._startingCabinLocations.Clear();
	}

	public void updateDoors()
	{
		if (Game1.IsClient)
		{
			return;
		}
		this.doors.Clear();
		Layer buildingLayer = this.map.RequireLayer("Buildings");
		int y = 0;
		for (int layerHeight = buildingLayer.LayerHeight; y < layerHeight; y++)
		{
			int x = 0;
			for (int layerWidth = buildingLayer.LayerWidth; x < layerWidth; x++)
			{
				Tile tile = buildingLayer.Tiles[x, y];
				if (tile == null || !tile.Properties.TryGetValue("Action", out var door) || !door.Contains("Warp"))
				{
					continue;
				}
				string[] split = ArgUtility.SplitBySpace(door);
				string propertyName = ArgUtility.Get(split, 0);
				switch (propertyName)
				{
				case "WarpBoatTunnel":
					this.doors.Add(new Point(x, y), new NetString("BoatTunnel"));
					continue;
				case "WarpCommunityCenter":
					this.doors.Add(new Point(x, y), new NetString("CommunityCenter"));
					continue;
				case "Warp_Sunroom_Door":
					this.doors.Add(new Point(x, y), new NetString("Sunroom"));
					continue;
				case "WarpMensLocker":
				case "LockedDoorWarp":
				case "Warp":
				case "WarpWomensLocker":
					break;
				default:
					if (!propertyName.Contains("Warp"))
					{
						continue;
					}
					Game1.log.Warn($"{this.NameOrUniqueName} ({x}, {y}) has unknown warp property '{door}', parsing with legacy logic.");
					break;
				}
				if (!(this.name.Value == "Mountain") || x != 8 || y != 20)
				{
					string locationName = ArgUtility.Get(split, 3);
					if (locationName != null)
					{
						this.doors.Add(new Point(x, y), new NetString(locationName));
					}
				}
			}
		}
	}

	[Obsolete("Use removeObjectsAndSpawned instead.")]
	private void clearArea(int startingX, int startingY, int width, int height)
	{
		this.removeObjectsAndSpawned(startingX, startingY, width, height);
	}

	public bool isTerrainFeatureAt(int x, int y)
	{
		Vector2 v = new Vector2(x, y);
		if (this.terrainFeatures.TryGetValue(v, out var terrainFeature) && !terrainFeature.isPassable())
		{
			return true;
		}
		if (this.largeTerrainFeatures != null)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64);
			foreach (LargeTerrainFeature largeTerrainFeature in this.largeTerrainFeatures)
			{
				if (largeTerrainFeature.getBoundingBox().Intersects(tileRect))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void loadLights()
	{
		if ((this.isOutdoors.Value && !Game1.isFestival() && !this.forceLoadPathLayerLights) || this is FarmHouse || this is IslandFarmHouse)
		{
			return;
		}
		Layer pathsLayer = this.map.GetLayer("Paths");
		Layer frontLayer = this.map.RequireLayer("Front");
		Layer buildingsLayer = this.map.RequireLayer("Buildings");
		for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
		{
			for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
			{
				int tileIndex;
				if (!this.isOutdoors.Value && !this.map.Properties.ContainsKey("IgnoreLightingTiles"))
				{
					tileIndex = frontLayer.GetTileIndexAt(x, y);
					if (tileIndex != -1)
					{
						this.adjustMapLightPropertiesForLamp(tileIndex, x, y, "Front");
					}
					tileIndex = buildingsLayer.GetTileIndexAt(x, y);
					if (tileIndex != -1)
					{
						this.adjustMapLightPropertiesForLamp(tileIndex, x, y, "Buildings");
					}
				}
				tileIndex = pathsLayer?.GetTileIndexAt(x, y) ?? (-1);
				if (tileIndex != -1)
				{
					this.adjustMapLightPropertiesForLamp(tileIndex, x, y, "Paths");
				}
			}
		}
	}

	public bool isFarmBuildingInterior()
	{
		return this is AnimalHouse;
	}

	/// <summary>Get whether this location is actively synced to the current player.</summary>
	/// <remarks>This is always true for the main player, and based on <see cref="M:StardewValley.Multiplayer.isActiveLocation(StardewValley.GameLocation)" /> for farmhands.</remarks>
	public bool IsActiveLocation()
	{
		if (Game1.IsMasterGame)
		{
			return true;
		}
		if (this.Root?.Value != null)
		{
			return Game1.multiplayer.isActiveLocation(this);
		}
		return false;
	}

	public virtual bool CanBeRemotedlyViewed()
	{
		return Game1.multiplayer.isAlwaysActiveLocation(this);
	}

	protected void adjustMapLightPropertiesForLamp(int tile, int x, int y, string layer)
	{
		string tilesheet = this.getTileSheetIDAt(x, y, layer);
		if (this.isFarmBuildingInterior())
		{
			if (tilesheet == "Coop" || tilesheet == "barn")
			{
				switch (tile)
				{
				case 24:
					this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 26);
					this.changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
					this.changeMapProperties("WindowLight", x + " " + (y + 3) + " 4");
					break;
				case 25:
					this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 12);
					break;
				case 46:
					this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 53);
					break;
				}
			}
		}
		else if (tile == 8 && layer == "Paths")
		{
			this.changeMapProperties("Light", x + " " + y + " 4");
		}
		else
		{
			if (!(tilesheet == "indoor"))
			{
				return;
			}
			switch (tile)
			{
			case 1346:
				this.changeMapProperties("DayTiles", "Front " + x + " " + y + " " + tile);
				this.changeMapProperties("NightTiles", "Front " + x + " " + y + " " + 1347);
				this.changeMapProperties("DayTiles", "Buildings " + x + " " + (y + 1) + " " + 452);
				this.changeMapProperties("NightTiles", "Buildings " + x + " " + (y + 1) + " " + 453);
				this.changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 480:
				this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 809);
				this.changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 826:
				this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 827);
				this.changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 1344:
				this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1345);
				this.changeMapProperties("Light", x + " " + y + " 4");
				break;
			case 256:
				this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
				this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1253);
				this.changeMapProperties("DayTiles", layer + " " + x + " " + (y + 1) + " " + 288);
				this.changeMapProperties("NightTiles", layer + " " + x + " " + (y + 1) + " " + 1285);
				this.changeMapProperties("WindowLight", x + " " + y + " 4");
				this.changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
				break;
			case 225:
				if (!this.name.Value.Contains("BathHouse") && !this.name.Value.Contains("Club") && (!this.name.Equals("SeedShop") || (x != 36 && x != 37)))
				{
					this.changeMapProperties("DayTiles", layer + " " + x + " " + y + " " + tile);
					this.changeMapProperties("NightTiles", layer + " " + x + " " + y + " " + 1222);
					this.changeMapProperties("DayTiles", layer + " " + x + " " + (y + 1) + " " + 257);
					this.changeMapProperties("NightTiles", layer + " " + x + " " + (y + 1) + " " + 1254);
					this.changeMapProperties("WindowLight", x + " " + y + " 4");
					this.changeMapProperties("WindowLight", x + " " + (y + 1) + " 4");
				}
				break;
			}
		}
	}

	private void changeMapProperties(string propertyName, string toAdd)
	{
		try
		{
			if (!this.map.Properties.TryGetValue(propertyName, out var oldValue))
			{
				this.map.Properties[propertyName] = new PropertyValue(toAdd);
			}
			else if (!oldValue.Contains(toAdd))
			{
				string newValue = oldValue + " " + toAdd;
				this.map.Properties[propertyName] = new PropertyValue(newValue);
			}
		}
		catch
		{
		}
	}

	/// <summary>Log an error indicating that a map property could not be parsed.</summary>
	/// <param name="name">The name of the property that failed to parse.</param>
	/// <param name="value">The property value that failed to parse.</param>
	/// <param name="error">The error phrase indicating why it failed.</param>
	public void LogMapPropertyError(string name, string value, string error)
	{
		Game1.log.Error($"Can't parse map property '{name}' with value '{value}' in location '{this.NameOrUniqueName}': {error}.");
	}

	/// <summary>Log an error indicating that a map property could not be parsed.</summary>
	/// <param name="name">The name of the property that failed to parse.</param>
	/// <param name="value">The property value that failed to parse.</param>
	/// <param name="error">The error phrase indicating why it failed.</param>
	/// <param name="delimiter">The character used to delimit values in the property.</param>
	public void LogMapPropertyError(string name, string[] value, string error, char delimiter = ' ')
	{
		this.LogMapPropertyError(name, string.Join(delimiter, value), error);
	}

	/// <summary>Log an error indicating that a tile property could not be parsed.</summary>
	/// <param name="name">The name of the property that failed to parse.</param>
	/// <param name="layerId">The layer containing the tile.</param>
	/// <param name="x">The X tile position of the tile.</param>
	/// <param name="y">The Y tile position of the tile.</param>
	/// <param name="value">The property value that failed to parse.</param>
	/// <param name="error">The error phrase indicating why it failed.</param>
	public void LogTilePropertyError(string name, string layerId, int x, int y, string value, string error)
	{
		Game1.log.Error($"Can't parse tile property '{name}' at {layerId}:{x},{y} with value '{value}' in location '{this.NameOrUniqueName}': {error}.");
	}

	/// <summary>Log an error indicating that a tile property could not be parsed.</summary>
	/// <param name="name">The name of the property that failed to parse.</param>
	/// <param name="layerId">The layer containing the tile.</param>
	/// <param name="x">The X tile position of the tile.</param>
	/// <param name="y">The Y tile position of the tile.</param>
	/// <param name="value">The property value that failed to parse.</param>
	/// <param name="error">The error phrase indicating why it failed.</param>
	/// <param name="delimiter">The character used to delimit values in the property.</param>
	public void LogTilePropertyError(string name, string layerId, int x, int y, string[] value, string error, char delimiter = ' ')
	{
		this.LogTilePropertyError(name, layerId, x, y, string.Join(delimiter, value), error);
	}

	/// <summary>Log an error indicating that a tile <c>Action</c> property could not be parsed.</summary>
	/// <param name="action">The action arguments, including the <c>Action</c> prefix.</param>
	/// <param name="x">The tile X position containing the action.</param>
	/// <param name="y">The tile Y position containing the action.</param>
	/// <param name="error">The error phrase indicating why it failed.</param>
	public void LogTileActionError(string[] action, int x, int y, string error)
	{
		this.LogTilePropertyError("Action", "Buildings", x, y, action, error);
	}

	/// <summary>Log an error indicating that a tile <c>TouchAction</c> property could not be parsed.</summary>
	/// <param name="action">The action arguments, including the <c>TouchAction</c> prefix.</param>
	/// <param name="tile">The tile position containing the action.</param>
	/// <param name="error">The error phrase indicating why it failed.</param>
	public void LogTileTouchActionError(string[] action, Vector2 tile, string error)
	{
		this.LogTilePropertyError("TouchAction", "Back", (int)tile.X, (int)tile.Y, action, error);
	}

	public override bool Equals(object obj)
	{
		if (obj is GameLocation location)
		{
			return this.Equals(location);
		}
		return false;
	}

	public bool Equals(GameLocation other)
	{
		if (other != null && this.isStructure.Get() == other.isStructure.Get())
		{
			return string.Equals(this.NameOrUniqueName, other.NameOrUniqueName, StringComparison.Ordinal);
		}
		return false;
	}
}
