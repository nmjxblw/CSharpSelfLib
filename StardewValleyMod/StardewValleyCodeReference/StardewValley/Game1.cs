using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SkiaSharp;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.Hashing;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.Dedicated;
using StardewValley.Network.NetReady;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SaveSerialization;
using StardewValley.SDKs.Steam;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley;

/// <summary>
/// This is the main type for your game
/// </summary>
[InstanceStatics]
public class Game1 : InstanceGame
{
	public enum BundleType
	{
		Default,
		Remixed
	}

	public enum MineChestType
	{
		Default,
		Remixed
	}

	public delegate void afterFadeFunction();

	public const bool IncrementalLoadEnabled = false;

	public const int defaultResolutionX = 1280;

	public const int defaultResolutionY = 720;

	public const int pixelZoom = 4;

	public const int tileSize = 64;

	public const int smallestTileSize = 16;

	public const int up = 0;

	public const int right = 1;

	public const int down = 2;

	public const int left = 3;

	public const int dialogueBoxTileHeight = 5;

	public static int realMilliSecondsPerGameMinute;

	public static int realMilliSecondsPerGameTenMinutes;

	public const int rainDensity = 70;

	public const int rainLoopLength = 70;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a value indicating the cursor should be hidden.</summary>
	public static readonly int cursor_none;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a default pointer icon.</summary>
	public static readonly int cursor_default;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a wait icon.</summary>
	public static readonly int cursor_wait;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a hand icon indicating that an item can be picked up.</summary>
	public static readonly int cursor_grab;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a gift box icon indicating that an NPC on this tile can accept a gift.</summary>
	public static readonly int cursor_gift;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a speech bubble icon indicating that an NPC can be talked to.</summary>
	public static readonly int cursor_talk;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a magnifying glass icon indicating that something can be examined.</summary>
	public static readonly int cursor_look;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, an icon indicating that something can be harvested.</summary>
	public static readonly int cursor_harvest;

	/// <summary>For <see cref="F:StardewValley.Game1.mouseCursor" />, a pointer icon used when hovering elements with gamepad controls.</summary>
	public static readonly int cursor_gamepad_pointer;

	public static readonly string asianSpacingRegexString;

	public const int legacy_weather_sunny = 0;

	public const int legacy_weather_rain = 1;

	public const int legacy_weather_debris = 2;

	public const int legacy_weather_lightning = 3;

	public const int legacy_weather_festival = 4;

	public const int legacy_weather_snow = 5;

	public const int legacy_weather_wedding = 6;

	public const string weather_sunny = "Sun";

	public const string weather_rain = "Rain";

	public const string weather_green_rain = "GreenRain";

	public const string weather_debris = "Wind";

	public const string weather_lightning = "Storm";

	public const string weather_festival = "Festival";

	public const string weather_snow = "Snow";

	public const string weather_wedding = "Wedding";

	/// <summary>The builder name for Robin's carpenter shop.</summary>
	public const string builder_robin = "Robin";

	/// <summary>The builder name for Wizard's magical construction shop.</summary>
	public const string builder_wizard = "Wizard";

	/// <summary>The shop ID for the Adventurer's Guild shop.</summary>
	public const string shop_adventurersGuild = "AdventureShop";

	/// <summary>The shop ID for the Adventurer's Guild item recovery shop.</summary>
	public const string shop_adventurersGuildItemRecovery = "AdventureGuildRecovery";

	/// <summary>The shop ID for Marnie's animal supply shop.</summary>
	public const string shop_animalSupplies = "AnimalShop";

	/// <summary>The shop ID for Clint's blacksmithery.</summary>
	public const string shop_blacksmith = "Blacksmith";

	/// <summary>The shop ID for Clint's tool upgrade shop.</summary>
	public const string shop_blacksmithUpgrades = "ClintUpgrade";

	/// <summary>The shop ID for the movie theater box office.</summary>
	public const string shop_boxOffice = "BoxOffice";

	/// <summary>The 'shop' ID for the floorpaper/wallpaper catalogue.</summary>
	public const string shop_catalogue = "Catalogue";

	/// <summary>The shop ID for Robin's carpenter supplies.</summary>
	public const string shop_carpenter = "Carpenter";

	/// <summary>The shop ID for the casino club shop.</summary>
	public const string shop_casino = "Casino";

	/// <summary>The shop ID for the desert trader.</summary>
	public const string shop_desertTrader = "DesertTrade";

	/// <summary>The shop ID for Dwarf's shop.</summary>
	public const string shop_dwarf = "Dwarf";

	/// <summary>The shop ID for Willy's fish shop.</summary>
	public const string shop_fish = "FishShop";

	/// <summary>The 'shop' ID for the furniture catalogue.</summary>
	public const string shop_furnitureCatalogue = "Furniture Catalogue";

	/// <summary>The shop ID for Pierre's General Store.</summary>
	public const string shop_generalStore = "SeedShop";

	/// <summary>The shop ID for the Hat Mouse shop.</summary>
	public const string shop_hatMouse = "HatMouse";

	/// <summary>The shop ID for Harvey's clinic.</summary>
	public const string shop_hospital = "Hospital";

	/// <summary>The shop ID for the ice-cream stand.</summary>
	public const string shop_iceCreamStand = "IceCreamStand";

	/// <summary>The shop ID for the island trader.</summary>
	public const string shop_islandTrader = "IslandTrade";

	/// <summary>The shop ID for Joja Mart.</summary>
	public const string shop_jojaMart = "Joja";

	/// <summary>The shop ID for Krobus' shop.</summary>
	public const string shop_krobus = "ShadowShop";

	/// <summary>The shop ID for Qi's gem shop.</summary>
	public const string shop_qiGemShop = "QiGemShop";

	/// <summary>The shop ID for the Ginger Island resort bar.</summary>
	public const string shop_resortBar = "ResortBar";

	/// <summary>The shop ID for Sandy's Oasis.</summary>
	public const string shop_sandy = "Sandy";

	/// <summary>The shop ID for the Stardrop Saloon.</summary>
	public const string shop_saloon = "Saloon";

	/// <summary>The shop ID for the traveling cart shop.</summary>
	public const string shop_travelingCart = "Traveler";

	/// <summary>The shop ID for the Volcano Dungeon shop.</summary>
	public const string shop_volcanoShop = "VolcanoShop";

	/// <summary>The shop ID for the bookseller.</summary>
	public const string shop_bookseller = "Bookseller";

	/// <summary>The shop ID for the bookseller trade-ins.</summary>
	public const string shop_bookseller_trade = "BooksellerTrade";

	/// <summary>The 'shop' ID for the joja furniture catalogue.</summary>
	public const string shop_jojaCatalogue = "JojaFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_wizardCatalogue = "WizardFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_junimoCatalogue = "JunimoFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_retroCatalogue = "RetroFurnitureCatalogue";

	/// <summary>The 'shop' ID for the wizard furniture catalogue.</summary>
	public const string shop_trashCatalogue = "TrashFurnitureCatalogue";

	/// <summary>The shop ID for Marnie's pet adoption shop.</summary>
	public const string shop_petAdoption = "PetAdoption";

	public const byte singlePlayer = 0;

	public const byte multiplayerClient = 1;

	public const byte multiplayerServer = 2;

	public const byte logoScreenGameMode = 4;

	public const byte titleScreenGameMode = 0;

	public const byte loadScreenGameMode = 1;

	public const byte newGameMode = 2;

	public const byte playingGameMode = 3;

	public const byte loadingMode = 6;

	public const byte saveMode = 7;

	public const byte saveCompleteMode = 8;

	public const byte selectGameScreen = 9;

	public const byte creditsMode = 10;

	public const byte errorLogMode = 11;

	/// <summary>The name of the game's main assembly.</summary>
	public static readonly string GameAssemblyName;

	/// <summary>The semantic game version, like <c>1.6.0</c>.</summary>
	/// <remarks>
	///   <para>
	///     This mostly follows semantic versioning format with three or four numbers (without leading zeros), so
	///     1.6.7 comes before 1.6.10. The first three numbers are consistent across all platforms, while some
	///     platforms may add a fourth number for the port version. This doesn't include tags like <c>-alpha</c>
	///     or <c>-beta</c>; see <see cref="F:StardewValley.Game1.versionLabel" /> or <see cref="M:StardewValley.Game1.GetVersionString" /> for that.
	///   </para>
	///
	///   <para>Game versions can be compared using <see cref="M:StardewValley.Utility.CompareGameVersions(System.String,System.String,System.Boolean)" />.</para>
	/// </remarks>
	public static readonly string version;

	/// <summary>A human-readable label for the update, like 'modding update' or 'hotfix #3', if any.</summary>
	public static readonly string versionLabel;

	/// <summary>The game build number used to distinguish different builds with the same version number, like <c>26055</c>.</summary>
	/// <remarks>This value is platform-dependent.</remarks>
	public static readonly int versionBuildNumber;

	public const float keyPollingThreshold = 650f;

	public const float toolHoldPerPowerupLevel = 600f;

	public const float startingMusicVolume = 1f;

	/// <summary>
	/// ContentManager specifically for loading xTile.Map(s).
	/// Will be unloaded when returning to title.
	/// </summary>
	public LocalizedContentManager xTileContent;

	public static DelayedAction morningSongPlayAction;

	private static LocalizedContentManager _temporaryContent;

	[NonInstancedStatic]
	private static bool FinishedIncrementalLoad;

	[NonInstancedStatic]
	private static bool FinishedFirstLoadContent;

	[NonInstancedStatic]
	private static volatile bool FinishedFirstInitSounds;

	[NonInstancedStatic]
	private static volatile bool FinishedFirstInitSerializers;

	[NonInstancedStatic]
	private static IEnumerator<int> LoadContentEnumerator;

	[NonInstancedStatic]
	public static GraphicsDeviceManager graphics;

	[NonInstancedStatic]
	public static LocalizedContentManager content;

	public static SpriteBatch spriteBatch;

	public static float MusicDuckTimer;

	public static GamePadState oldPadState;

	public static float thumbStickSensitivity;

	public static float runThreshold;

	public static int rightStickHoldTime;

	public static int emoteMenuShowTime;

	public static int nextFarmerWarpOffsetX;

	public static int nextFarmerWarpOffsetY;

	public static KeyboardState oldKBState;

	public static MouseState oldMouseState;

	[NonInstancedStatic]
	public static Game1 keyboardFocusInstance;

	private static Farmer _player;

	public static NetFarmerRoot serverHost;

	protected static bool _isWarping;

	[NonInstancedStatic]
	public static bool hasLocalClientsOnly;

	protected bool _instanceIsPlayingBackgroundMusic;

	protected bool _instanceIsPlayingOutdoorsAmbience;

	protected bool _instanceIsPlayingNightAmbience;

	protected bool _instanceIsPlayingTownMusic;

	protected bool _instanceIsPlayingMorningSong;

	public static bool isUsingBackToFrontSorting;

	protected static StringBuilder _debugStringBuilder;

	[NonInstancedStatic]
	internal static readonly DebugTimings debugTimings;

	public static Dictionary<string, GameLocation> _locationLookup;

	public IList<GameLocation> _locations = new List<GameLocation>();

	public static Regex asianSpacingRegex;

	public static Viewport defaultDeviceViewport;

	public static LocationRequest locationRequest;

	public static bool warpingForForcedRemoteEvent;

	protected static GameLocation _PreviousNonNullLocation;

	public GameLocation instanceGameLocation;

	public static IDisplayDevice mapDisplayDevice;

	public static xTile.Dimensions.Rectangle viewport;

	public static xTile.Dimensions.Rectangle uiViewport;

	public static Texture2D objectSpriteSheet;

	public static Texture2D cropSpriteSheet;

	public static Texture2D emoteSpriteSheet;

	public static Texture2D debrisSpriteSheet;

	public static Texture2D rainTexture;

	public static Texture2D bigCraftableSpriteSheet;

	public static Texture2D buffsIcons;

	public static Texture2D daybg;

	public static Texture2D nightbg;

	public static Texture2D menuTexture;

	public static Texture2D uncoloredMenuTexture;

	public static Texture2D lantern;

	public static Texture2D windowLight;

	public static Texture2D sconceLight;

	public static Texture2D cauldronLight;

	public static Texture2D shadowTexture;

	public static Texture2D mouseCursors;

	public static Texture2D mouseCursors2;

	public static Texture2D mouseCursors_1_6;

	public static Texture2D giftboxTexture;

	public static Texture2D controllerMaps;

	public static Texture2D indoorWindowLight;

	public static Texture2D animations;

	public static Texture2D concessionsSpriteSheet;

	public static Texture2D birdsSpriteSheet;

	public static Texture2D objectSpriteSheet_2;

	public static Texture2D bobbersTexture;

	public static Dictionary<string, Stack<Dialogue>> npcDialogues;

	protected readonly List<Farmer> _farmerShadows = new List<Farmer>();

	/// <summary>Actions that are called after waking up in the morning. These aren't saved, so they're only use for "fluff".</summary>
	public static Queue<Action> morningQueue;

	[NonInstancedStatic]
	protected internal static ModHooks hooks;

	public static InputState input;

	protected internal static IInputSimulator inputSimulator;

	public const string concessionsSpriteSheetName = "LooseSprites\\Concessions";

	public const string cropSpriteSheetName = "TileSheets\\crops";

	public const string objectSpriteSheetName = "Maps\\springobjects";

	public const string animationsName = "TileSheets\\animations";

	public const string mouseCursorsName = "LooseSprites\\Cursors";

	public const string mouseCursors2Name = "LooseSprites\\Cursors2";

	public const string mouseCursors1_6Name = "LooseSprites\\Cursors_1_6";

	public const string giftboxName = "LooseSprites\\Giftbox";

	public const string toolSpriteSheetName = "TileSheets\\tools";

	public const string bigCraftableSpriteSheetName = "TileSheets\\Craftables";

	public const string debrisSpriteSheetName = "TileSheets\\debris";

	public const string parrotSheetName = "LooseSprites\\parrots";

	public const string hatsSheetName = "Characters\\Farmer\\hats";

	public const string bobbersTextureName = "TileSheets\\bobbers";

	private static Texture2D _toolSpriteSheet;

	public static Dictionary<Vector2, int> crabPotOverlayTiles;

	protected static bool _setSaveName;

	protected static string _currentSaveName;

	public static List<string> mailDeliveredFromMailForTomorrow;

	private static RenderTarget2D _lightmap;

	public static Texture2D[] dynamicPixelRects;

	public static Texture2D fadeToBlackRect;

	public static Texture2D staminaRect;

	public static Texture2D lightingRect;

	public static SpriteFont dialogueFont;

	public static SpriteFont smallFont;

	public static SpriteFont tinyFont;

	public static float screenGlowAlpha;

	public static float flashAlpha;

	public static float noteBlockTimer;

	public static int currentGemBirdIndex;

	public Dictionary<string, object> newGameSetupOptions = new Dictionary<string, object>();

	public static bool dialogueUp;

	public static bool dialogueTyping;

	public static bool isQuestion;

	public static bool newDay;

	public static bool eventUp;

	public static bool viewportFreeze;

	public static bool eventOver;

	public static bool screenGlow;

	public static bool screenGlowHold;

	public static bool screenGlowUp;

	public static bool killScreen;

	public static bool messagePause;

	public static bool weddingToday;

	public static bool exitToTitle;

	public static bool debugMode;

	public static bool displayHUD;

	public static bool displayFarmer;

	public static bool dialogueButtonShrinking;

	public static bool drawLighting;

	public static bool quit;

	public static bool drawGrid;

	public static bool freezeControls;

	public static bool saveOnNewDay;

	public static bool panMode;

	public static bool showingEndOfNightStuff;

	public static bool wasRainingYesterday;

	public static bool hasLoadedGame;

	public static bool isActionAtCurrentCursorTile;

	public static bool isInspectionAtCurrentCursorTile;

	public static bool isSpeechAtCurrentCursorTile;

	public static bool paused;

	public static bool isTimePaused;

	public static bool frameByFrame;

	public static bool lastCursorMotionWasMouse;

	public static bool showingHealth;

	public static bool cabinsSeparate;

	public static bool showingHealthBar;

	/// <summary>Whether <see cref="M:StardewValley.Game1.OnDayStarted" /> has been called at least once since this save was loaded or joined.</summary>
	public static bool hasStartedDay;

	/// <summary>The event IDs which the current player has seen since entering the location.</summary>
	public static HashSet<string> eventsSeenSinceLastLocationChange;

	internal static bool hasApplied1_3_UpdateChanges;

	internal static bool hasApplied1_4_UpdateChanges;

	private static Action postExitToTitleCallback;

	protected int _lastUsedDisplay = -1;

	public bool wasAskedLeoMemory;

	public float controllerSlingshotSafeTime;

	public static BundleType bundleType;

	public static bool isRaining;

	public static bool isSnowing;

	public static bool isLightning;

	public static bool isDebrisWeather;

	/// <summary>Internal state that tracks whether today's weather state is a green rain day.</summary>
	private static bool _isGreenRain;

	/// <summary>Whether today's weather state was green rain at any point.</summary>
	internal static bool wasGreenRain;

	/// <summary>Whether the locations affected by green rain still need cleanup. This should only be set by <see cref="M:StardewValley.Game1._newDayAfterFade" />.</summary>
	internal static bool greenRainNeedsCleanup;

	/// <summary>The season for which the debris weather fields like <see cref="F:StardewValley.Game1.debrisWeather" /> were last generated.</summary>
	public static Season? debrisWeatherSeason;

	public static string weatherForTomorrow;

	public float zoomModifier = 1f;

	private static ScreenFade screenFade;

	/// <summary>The current season of the year.</summary>
	public static Season season;

	public static SerializableDictionary<string, string> bannedUsers;

	private static object _debugOutputLock;

	private static string _debugOutput;

	public static string requestedMusicTrack;

	public static string messageAfterPause;

	public static string samBandName;

	public static string loadingMessage;

	public static string errorMessage;

	protected Dictionary<MusicContext, KeyValuePair<string, bool>> _instanceRequestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>();

	protected MusicContext _instanceActiveMusicContext;

	public static bool requestedMusicTrackOverrideable;

	public static bool currentTrackOverrideable;

	public static bool requestedMusicDirty;

	protected bool _useUnscaledLighting;

	protected bool _didInitiateItemStow;

	public bool instanceIsOverridingTrack;

	private static string[] _shortDayDisplayName;

	public static Queue<string> currentObjectDialogue;

	public static HashSet<string> worldStateIDs;

	public static List<Response> questionChoices;

	public static int xLocationAfterWarp;

	public static int yLocationAfterWarp;

	public static int gameTimeInterval;

	public static int currentQuestionChoice;

	public static int currentDialogueCharacterIndex;

	public static int dialogueTypingInterval;

	/// <summary>The calendar day of month.</summary>
	public static int dayOfMonth;

	/// <summary>The calendar year.</summary>
	public static int year;

	public static int timeOfDay;

	public static int timeOfDayAfterFade;

	public static int dialogueWidth;

	public static int facingDirectionAfterWarp;

	public static int mouseClickPolling;

	public static int gamePadXButtonPolling;

	public static int gamePadAButtonPolling;

	public static int weatherIcon;

	public static int hitShakeTimer;

	public static int staminaShakeTimer;

	public static int pauseThenDoFunctionTimer;

	public static int cursorTileHintCheckTimer;

	public static int timerUntilMouseFade;

	public static int whichFarm;

	public static int startingCabins;

	public static ModFarmType whichModFarm;

	public static ulong? startingGameSeed;

	public static int elliottPiano;

	public static Microsoft.Xna.Framework.Rectangle viewportClampArea;

	public static SaveFixes lastAppliedSaveFix;

	public static Color eveningColor;

	public static Color unselectedOptionColor;

	public static Color screenGlowColor;

	public static NPC currentSpeaker;

	/// <summary>A default random number generator used for a wide variety of randomization in the game. This provides non-repeatable randomization (e.g. reloading the save will produce different results).</summary>
	public static Random random;

	public static Random recentMultiplayerRandom;

	/// <summary>The cached data for achievements from <c>Data/Achievements</c>.</summary>
	public static Dictionary<int, string> achievements;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_bigCraftable" />-type items from <c>Data/BigCraftables</c>.</summary>
	public static IDictionary<string, BigCraftableData> bigCraftableData;

	/// <summary>The cached data for buildings from <c>Data/Buildings</c>.</summary>
	public static IDictionary<string, BuildingData> buildingData;

	/// <summary>The cached data for NPCs from <c>Data/Characters</c>.</summary>
	public static IDictionary<string, CharacterData> characterData;

	/// <summary>The cached data for crops from <c>Data/Crops</c>.</summary>
	public static IDictionary<string, CropData> cropData;

	/// <summary>The cached data for farm animals from <c>Data/FarmAnimals</c>.</summary>
	public static IDictionary<string, FarmAnimalData> farmAnimalData;

	/// <summary>The cached data for flooring and path items from <c>Data/FloorsAndPaths</c>.</summary>
	public static IDictionary<string, FloorPathData> floorPathData;

	/// <summary>The cached data for fruit trees from <c>Data/FruitTrees</c>.</summary>
	public static IDictionary<string, FruitTreeData> fruitTreeData;

	/// <summary>The cached data for jukebox tracks from <c>Data/JukeboxTracks</c>.</summary>
	public static IDictionary<string, JukeboxTrackData> jukeboxTrackData;

	/// <summary>The cached data for locations from <c>Data/Locations</c>.</summary>
	public static IDictionary<string, LocationData> locationData;

	/// <summary>The cached data for location contexts from <c>Data/LocationContexts</c>.</summary>
	public static IDictionary<string, LocationContextData> locationContextData;

	/// <summary>The cached data for <c>Data/NPCGiftTastes</c>.</summary>
	public static IDictionary<string, string> NPCGiftTastes;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_object" />-type items from <c>Data/Objects</c>.</summary>
	public static IDictionary<string, ObjectData> objectData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_pants" />-type items from <c>Data/Pants</c>.</summary>
	public static IDictionary<string, PantsData> pantsData;

	/// <summary>The cached data for pets from <c>Data/Pets</c>.</summary>
	public static IDictionary<string, PetData> petData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_shirt" />-type items from <c>Data/Shirts</c>.</summary>
	public static IDictionary<string, ShirtData> shirtData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_tool" />-type items from <c>Data/Tools</c>.</summary>
	public static IDictionary<string, ToolData> toolData;

	/// <summary>The cached data for <see cref="F:StardewValley.ItemRegistry.type_weapon" />-type items from <c>Data/Weapons</c>.</summary>
	public static IDictionary<string, WeaponData> weaponData;

	public static List<HUDMessage> hudMessages;

	public static float musicPlayerVolume;

	public static float ambientPlayerVolume;

	public static float pauseAccumulator;

	public static float pauseTime;

	public static float upPolling;

	public static float downPolling;

	public static float rightPolling;

	public static float leftPolling;

	public static float debrisSoundInterval;

	public static float windGust;

	public static float dialogueButtonScale;

	public ICue instanceCurrentSong;

	public static IAudioCategory musicCategory;

	public static IAudioCategory soundCategory;

	public static IAudioCategory ambientCategory;

	public static IAudioCategory footstepCategory;

	public PlayerIndex instancePlayerOneIndex;

	[NonInstancedStatic]
	public static IAudioEngine audioEngine;

	[NonInstancedStatic]
	public static WaveBank waveBank;

	[NonInstancedStatic]
	public static WaveBank waveBank1_4;

	[NonInstancedStatic]
	public static ISoundBank soundBank;

	public static Vector2 previousViewportPosition;

	public static Vector2 currentCursorTile;

	public static Vector2 lastCursorTile;

	public static Vector2 snowPos;

	public Microsoft.Xna.Framework.Rectangle localMultiplayerWindow;

	public static RainDrop[] rainDrops;

	public static ICue chargeUpSound;

	public static ICue wind;

	/// <summary>The audio cues for the current location which are continuously looping until they're stopped.</summary>
	public static LoopingCueManager loopingLocationCues;

	/// <summary>Encapsulates the game logic for playing sound effects (excluding music and background ambience).</summary>
	public static ISoundsHelper sounds;

	[NonInstancedStatic]
	public static AudioCueModificationManager CueModification;

	public static List<WeatherDebris> debrisWeather;

	public static TemporaryAnimatedSpriteList screenOverlayTempSprites;

	public static TemporaryAnimatedSpriteList uiOverlayTempSprites;

	private static byte _gameMode;

	private bool _isSaving;

	/// <summary>Handles writing game messages to the log output.</summary>
	[NonInstancedStatic]
	protected internal static IGameLogger log;

	/// <summary>Combines hash codes in a deterministic way that's consistent between both sessions and players.</summary>
	[NonInstancedStatic]
	public static IHashUtility hash;

	protected internal static Multiplayer multiplayer;

	public static byte multiplayerMode;

	public static IEnumerator<int> currentLoader;

	public static ulong uniqueIDForThisGame;

	public static int[] directionKeyPolling;

	/// <summary>The light sources to draw in the current location.</summary>
	public static Dictionary<string, LightSource> currentLightSources;

	public static Color ambientLight;

	public static Color outdoorLight;

	public static Color textColor;

	/// <summary>The default color for shadows drawn under text.</summary>
	public static Color textShadowColor;

	/// <summary>A darker version of <see cref="F:StardewValley.Game1.textShadowColor" /> used in some cases.</summary>
	public static Color textShadowDarkerColor;

	public static IClickableMenu overlayMenu;

	private static IClickableMenu _activeClickableMenu;

	/// <summary>The queue of menus to open when the <see cref="P:StardewValley.Game1.activeClickableMenu" /> is closed.</summary>
	/// <remarks>See also <see cref="P:StardewValley.Game1.activeClickableMenu" />, <see cref="F:StardewValley.Game1.onScreenMenus" />, and <see cref="F:StardewValley.Game1.overlayMenu" />.</remarks>
	public static List<IClickableMenu> nextClickableMenu;

	/// <summary>A queue of actions to perform when <see cref="M:StardewValley.Farmer.IsBusyDoingSomething" /> is false.</summary>
	/// <remarks>Most code should call <see cref="M:StardewValley.Game1.PerformActionWhenPlayerFree(System.Action)" /> instead of using this field directly.</remarks>
	public static List<Action> actionsWhenPlayerFree;

	public static bool isCheckingNonMousePlacement;

	private static IMinigame _currentMinigame;

	public static IList<IClickableMenu> onScreenMenus;

	public static BuffsDisplay buffsDisplay;

	public static DayTimeMoneyBox dayTimeMoneyBox;

	public static NetRootDictionary<long, Farmer> otherFarmers;

	private static readonly FarmerCollection _onlineFarmers;

	public static IGameServer server;

	public static Client client;

	public KeyboardDispatcher instanceKeyboardDispatcher;

	public static Background background;

	public static FarmEvent farmEvent;

	/// <summary>The farm event to play next, if a regular farm event doesn't play via <see cref="F:StardewValley.Game1.farmEvent" /> instead.</summary>
	/// <remarks>This is set via the <see cref="M:StardewValley.DebugCommands.DefaultHandlers.SetFarmEvent(System.String[],StardewValley.Logging.IGameLogger)" /> debug command.</remarks>
	public static FarmEvent farmEventOverride;

	public static afterFadeFunction afterFade;

	public static afterFadeFunction afterDialogues;

	public static afterFadeFunction afterViewport;

	public static afterFadeFunction viewportReachedTarget;

	public static afterFadeFunction afterPause;

	public static GameTime currentGameTime;

	public static IList<DelayedAction> delayedActions;

	public static Stack<IClickableMenu> endOfNightMenus;

	public Options instanceOptions;

	[NonInstancedStatic]
	public static SerializableDictionary<long, Options> splitscreenOptions;

	public static Game1 game1;

	public static Point lastMousePositionBeforeFade;

	public static int ticks;

	public static EmoteMenu emoteMenu;

	[NonInstancedStatic]
	public static SerializableDictionary<string, string> CustomData;

	/// <summary>Manages and synchronizes ready checks, which ensure all players are ready before proceeding (e.g. before sleeping).</summary>
	public static ReadySynchronizer netReady;

	/// <summary>Manages updating a fake host player when <see cref="!:Game1.IsDedicatedServer" /> is true.</summary>
	public static DedicatedServer dedicatedServer;

	public static NetRoot<NetWorldState> netWorldState;

	public static ChatBox chatBox;

	public TextEntryMenu instanceTextEntry;

	public static SpecialCurrencyDisplay specialCurrencyDisplay;

	private static string debugPresenceString;

	public static List<Action> remoteEventQueue;

	public static List<long> weddingsToday;

	public int instanceIndex;

	public int instanceId;

	public static bool overrideGameMenuReset;

	protected bool _windowResizing;

	protected Point _oldMousePosition;

	protected bool _oldGamepadConnectedState;

	protected int _oldScrollWheelValue;

	public static Point viewportCenter;

	public static Vector2 viewportTarget;

	public static float viewportSpeed;

	public static int viewportHold;

	private static bool _cursorDragEnabled;

	private static bool _cursorDragPrevEnabled;

	private static bool _cursorSpeedDirty;

	private const float CursorBaseSpeed = 16f;

	private static float _cursorSpeed;

	private static float _cursorSpeedScale;

	private static float _cursorUpdateElapsedSec;

	private static int thumbstickPollingTimer;

	public static bool toggleFullScreen;

	public static string whereIsTodaysFest;

	public const string NO_LETTER_MAIL = "%&NL&%";

	public const string BROADCAST_MAIL_FOR_TOMORROW_PREFIX = "%&MFT&%";

	public const string BROADCAST_SEEN_MAIL_PREFIX = "%&SM&%";

	public const string BROADCAST_MAILBOX_PREFIX = "%&MB&%";

	public bool isLocalMultiplayerNewDayActive;

	protected static Task _newDayTask;

	private static Action _afterNewDayAction;

	public static NewDaySynchronizer newDaySync;

	public static bool forceSnapOnNextViewportUpdate;

	public static Vector2 currentViewportTarget;

	public static Vector2 viewportPositionLerp;

	public static float screenGlowRate;

	public static float screenGlowMax;

	public static bool haltAfterCheck;

	public static bool uiMode;

	public static RenderTarget2D nonUIRenderTarget;

	public static int uiModeCount;

	protected static int _oldUIModeCount;

	internal string panModeString;

	public static bool conventionMode;

	internal static EventTest eventTest;

	internal bool panFacingDirectionWait;

	public static bool isRunningMacro;

	public static int thumbstickMotionMargin;

	public static float thumbstickMotionAccell;

	public static int triggerPolling;

	public static int rightClickPolling;

	private RenderTarget2D _screen;

	private RenderTarget2D _uiScreen;

	public static Color bgColor;

	protected readonly BlendState lightingBlend = new BlendState
	{
		ColorBlendFunction = BlendFunction.ReverseSubtract,
		ColorDestinationBlend = Blend.One,
		ColorSourceBlend = Blend.SourceColor
	};

	public bool isDrawing;

	[NonInstancedStatic]
	public static bool isRenderingScreenBuffer;

	protected bool _lastDrewMouseCursor;

	protected static int _activatedTick;

	/// <summary>The cursor icon to show, usually matching a constant like <see cref="F:StardewValley.Game1.cursor_default" />.</summary>
	public static int mouseCursor;

	private static float _mouseCursorTransparency;

	public static bool wasMouseVisibleThisFrame;

	public static NPC objectDialoguePortraitPerson;

	protected static StringBuilder _ParseTextStringBuilder;

	protected static StringBuilder _ParseTextStringBuilderLine;

	protected static StringBuilder _ParseTextStringBuilderWord;

	public bool ScreenshotBusy;

	public bool takingMapScreenshot;

	public bool IsActiveNoOverlay
	{
		get
		{
			if (!base.IsActive)
			{
				return false;
			}
			if (Program.sdk.HasOverlay)
			{
				return false;
			}
			return true;
		}
	}

	public static LocalizedContentManager temporaryContent
	{
		get
		{
			if (Game1._temporaryContent == null)
			{
				Game1._temporaryContent = Game1.content.CreateTemporary();
			}
			return Game1._temporaryContent;
		}
	}

	private bool ShouldLoadIncrementally => false;

	/// <summary>The local character controlled by the current player.</summary>
	/// <remarks>Setting this field will dispose the previous value, if any. The new value must be a completely new instance which doesn't share any texture instances with the previous one to avoid errors.</remarks>
	public static Farmer player
	{
		get
		{
			return Game1._player;
		}
		internal set
		{
			Game1._player?.unload();
			Game1._player = value;
			Game1._player.Items.IsLocalPlayerInventory = true;
		}
	}

	public static bool IsPlayingBackgroundMusic
	{
		get
		{
			return Game1.game1._instanceIsPlayingBackgroundMusic;
		}
		set
		{
			Game1.game1._instanceIsPlayingBackgroundMusic = value;
		}
	}

	public static bool IsPlayingOutdoorsAmbience
	{
		get
		{
			return Game1.game1._instanceIsPlayingOutdoorsAmbience;
		}
		set
		{
			Game1.game1._instanceIsPlayingOutdoorsAmbience = value;
		}
	}

	public static bool IsPlayingNightAmbience
	{
		get
		{
			return Game1.game1._instanceIsPlayingNightAmbience;
		}
		set
		{
			Game1.game1._instanceIsPlayingNightAmbience = value;
		}
	}

	public static bool IsPlayingTownMusic
	{
		get
		{
			return Game1.game1._instanceIsPlayingTownMusic;
		}
		set
		{
			Game1.game1._instanceIsPlayingTownMusic = value;
		}
	}

	public static bool IsPlayingMorningSong
	{
		get
		{
			return Game1.game1._instanceIsPlayingMorningSong;
		}
		set
		{
			Game1.game1._instanceIsPlayingMorningSong = value;
		}
	}

	public static bool isWarping => Game1._isWarping;

	public static IList<GameLocation> locations => Game1.game1._locations;

	public static GameLocation currentLocation
	{
		get
		{
			return Game1.game1.instanceGameLocation;
		}
		set
		{
			if (Game1.game1.instanceGameLocation != value)
			{
				if (Game1._PreviousNonNullLocation == null)
				{
					Game1._PreviousNonNullLocation = Game1.game1.instanceGameLocation;
				}
				Game1.game1.instanceGameLocation = value;
				if (Game1.game1.instanceGameLocation != null)
				{
					GameLocation previousNonNullLocation = Game1._PreviousNonNullLocation;
					Game1._PreviousNonNullLocation = null;
					Game1.OnLocationChanged(previousNonNullLocation, Game1.game1.instanceGameLocation);
				}
			}
		}
	}

	public static Texture2D toolSpriteSheet
	{
		get
		{
			if (Game1._toolSpriteSheet == null)
			{
				Game1.ResetToolSpriteSheet();
			}
			return Game1._toolSpriteSheet;
		}
	}

	public static RenderTarget2D lightmap => Game1._lightmap;

	/// <summary>Get whether HUD menus like the toolbar are currently being drawn.</summary>
	public static bool IsHudDrawn
	{
		get
		{
			if ((Game1.displayHUD || Game1.eventUp) && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode && !Game1.HostPaused)
			{
				return !Game1.game1.takingMapScreenshot;
			}
			return false;
		}
	}

	/// <summary>Whether today's weather state is a green rain day.</summary>
	public static bool isGreenRain
	{
		get
		{
			return Game1._isGreenRain;
		}
		set
		{
			Game1._isGreenRain = value;
			Game1.wasGreenRain |= value;
		}
	}

	public static bool spawnMonstersAtNight
	{
		get
		{
			return Game1.player.team.spawnMonstersAtNight.Value;
		}
		set
		{
			Game1.player.team.spawnMonstersAtNight.Value = value;
		}
	}

	/// <summary>When the game makes a random choice, whether to use a simpler method that's prone to repeating patterns.</summary>
	/// <remarks>This is mainly intended for speedrunning, where full randomization might be undesirable.</remarks>
	public static bool UseLegacyRandom
	{
		get
		{
			return Game1.player.team.useLegacyRandom.Value;
		}
		set
		{
			Game1.player.team.useLegacyRandom.Value = value;
		}
	}

	public static bool fadeToBlack
	{
		get
		{
			return Game1.screenFade.fadeToBlack;
		}
		set
		{
			Game1.screenFade.fadeToBlack = value;
		}
	}

	public static bool fadeIn
	{
		get
		{
			return Game1.screenFade.fadeIn;
		}
		set
		{
			Game1.screenFade.fadeIn = value;
		}
	}

	public static bool globalFade
	{
		get
		{
			return Game1.screenFade.globalFade;
		}
		set
		{
			Game1.screenFade.globalFade = value;
		}
	}

	public static bool nonWarpFade
	{
		get
		{
			return Game1.screenFade.nonWarpFade;
		}
		set
		{
			Game1.screenFade.nonWarpFade = value;
		}
	}

	public static float fadeToBlackAlpha
	{
		get
		{
			return Game1.screenFade.fadeToBlackAlpha;
		}
		set
		{
			Game1.screenFade.fadeToBlackAlpha = value;
		}
	}

	public static float globalFadeSpeed
	{
		get
		{
			return Game1.screenFade.globalFadeSpeed;
		}
		set
		{
			Game1.screenFade.globalFadeSpeed = value;
		}
	}

	public static string CurrentSeasonDisplayName => Game1.content.LoadString("Strings\\StringsFromCSFiles:" + Game1.currentSeason);

	/// <summary>The current season of the year as a string (one of <c>spring</c>, <c>summer</c>, <c>fall</c>, or <c>winter</c>).</summary>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.season" /> instead.</remarks>
	public static string currentSeason
	{
		get
		{
			return Utility.getSeasonKey(Game1.season);
		}
		set
		{
			if (Utility.TryParseEnum<Season>(value, out var seasonValue))
			{
				Game1.season = seasonValue;
				return;
			}
			throw new ArgumentException("Can't parse value '" + value + "' as a season name.");
		}
	}

	/// <summary>The current season of the year as a numeric index.</summary>
	/// <remarks>Most code should use <see cref="F:StardewValley.Game1.season" /> instead.</remarks>
	public static int seasonIndex => (int)Game1.season;

	public static string debugOutput
	{
		get
		{
			return Game1._debugOutput;
		}
		set
		{
			lock (Game1._debugOutputLock)
			{
				if (Game1._debugOutput != value)
				{
					Game1._debugOutput = value;
					if (!string.IsNullOrEmpty(Game1._debugOutput))
					{
						Game1.log.Debug("DebugOutput: " + Game1._debugOutput);
					}
				}
			}
		}
	}

	public static string elliottBookName
	{
		get
		{
			if (Game1.player != null && Game1.player.DialogueQuestionsAnswered.Contains("958699"))
			{
				return Game1.content.LoadString("Strings\\Events:ElliottBook_mystery");
			}
			if (Game1.player != null && Game1.player.DialogueQuestionsAnswered.Contains("958700"))
			{
				return Game1.content.LoadString("Strings\\Events:ElliottBook_romance");
			}
			return Game1.content.LoadString("Strings\\Events:ElliottBook_default");
		}
		set
		{
		}
	}

	protected static Dictionary<MusicContext, KeyValuePair<string, bool>> _requestedMusicTracks
	{
		get
		{
			return Game1.game1._instanceRequestedMusicTracks;
		}
		set
		{
			Game1.game1._instanceRequestedMusicTracks = value;
		}
	}

	protected static MusicContext _activeMusicContext
	{
		get
		{
			return Game1.game1._instanceActiveMusicContext;
		}
		set
		{
			Game1.game1._instanceActiveMusicContext = value;
		}
	}

	public static bool isOverridingTrack
	{
		get
		{
			return Game1.game1.instanceIsOverridingTrack;
		}
		set
		{
			Game1.game1.instanceIsOverridingTrack = value;
		}
	}

	public bool useUnscaledLighting
	{
		get
		{
			return this._useUnscaledLighting;
		}
		set
		{
			if (this._useUnscaledLighting != value)
			{
				this._useUnscaledLighting = value;
				Game1.allocateLightmap(this.localMultiplayerWindow.Width, this.localMultiplayerWindow.Height);
			}
		}
	}

	/// <inheritdoc cref="F:StardewValley.Farmer.mailbox" />
	public static IList<string> mailbox => Game1.player.mailbox;

	public static ICue currentSong
	{
		get
		{
			return Game1.game1.instanceCurrentSong;
		}
		set
		{
			Game1.game1.instanceCurrentSong = value;
		}
	}

	public static PlayerIndex playerOneIndex
	{
		get
		{
			return Game1.game1.instancePlayerOneIndex;
		}
		set
		{
			Game1.game1.instancePlayerOneIndex = value;
		}
	}

	/// <summary>The number of ticks since <see cref="P:StardewValley.Game1.gameMode" /> changed.</summary>
	public static int gameModeTicks { get; private set; }

	public static byte gameMode
	{
		get
		{
			return Game1._gameMode;
		}
		set
		{
			if (Game1._gameMode != value)
			{
				Game1.log.Verbose("gameMode was '" + Game1.GameModeToString(Game1._gameMode) + "', set to '" + Game1.GameModeToString(value) + "'.");
				Game1._gameMode = value;
				Game1.gameModeTicks = 0;
			}
		}
	}

	public bool IsSaving
	{
		get
		{
			return this._isSaving;
		}
		set
		{
			this._isSaving = value;
		}
	}

	public static Multiplayer Multiplayer => Game1.multiplayer;

	public static Stats stats => Game1.player.stats;

	/// <summary>The daily quest that's shown on the billboard, if any.</summary>
	public static Quest questOfTheDay => Game1.netWorldState.Value.QuestOfTheDay;

	/// <summary>The menu which is currently handling player interactions (e.g. a letter viewer, dialogue box, inventory, etc).</summary>
	/// <remarks>See also <see cref="F:StardewValley.Game1.nextClickableMenu" />, <see cref="F:StardewValley.Game1.onScreenMenus" />, and <see cref="F:StardewValley.Game1.overlayMenu" />.</remarks>
	public static IClickableMenu activeClickableMenu
	{
		get
		{
			return Game1._activeClickableMenu;
		}
		set
		{
			bool num = (Game1.activeClickableMenu is SaveGameMenu || Game1.activeClickableMenu is ShippingMenu) && !(value is SaveGameMenu) && !(value is ShippingMenu);
			if (Game1._activeClickableMenu is IDisposable disposable && !Game1._activeClickableMenu.HasDependencies())
			{
				disposable.Dispose();
			}
			if (Game1.textEntry != null && Game1._activeClickableMenu != value)
			{
				Game1.closeTextEntry();
			}
			if (Game1._activeClickableMenu != null && value == null)
			{
				Game1.timerUntilMouseFade = 0;
			}
			Game1._activeClickableMenu = value;
			if (num)
			{
				Game1.OnDayStarted();
			}
			if (Game1._activeClickableMenu != null)
			{
				if (!Game1.eventUp || (Game1.CurrentEvent != null && Game1.CurrentEvent.playerControlSequence && !Game1.player.UsingTool))
				{
					Game1.player.Halt();
				}
			}
			else if (Game1.nextClickableMenu.Count > 0)
			{
				Game1.activeClickableMenu = Game1.nextClickableMenu[0];
				Game1.nextClickableMenu.RemoveAt(0);
			}
		}
	}

	public static IMinigame currentMinigame
	{
		get
		{
			return Game1._currentMinigame;
		}
		set
		{
			Game1._currentMinigame = value;
			if (value == null)
			{
				if (Game1.currentLocation != null)
				{
					Game1.setRichPresence("location", Game1.currentLocation.Name);
				}
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
				Game1.randomizeRainPositions();
			}
			else if (value.minigameId() != null)
			{
				Game1.setRichPresence("minigame", value.minigameId());
			}
		}
	}

	public static Object dishOfTheDay
	{
		get
		{
			return Game1.netWorldState.Value.DishOfTheDay;
		}
		set
		{
			Game1.netWorldState.Value.DishOfTheDay = value;
		}
	}

	public static KeyboardDispatcher keyboardDispatcher
	{
		get
		{
			return Game1.game1.instanceKeyboardDispatcher;
		}
		set
		{
			Game1.game1.instanceKeyboardDispatcher = value;
		}
	}

	public static Options options
	{
		get
		{
			return Game1.game1.instanceOptions;
		}
		set
		{
			Game1.game1.instanceOptions = value;
		}
	}

	public static TextEntryMenu textEntry
	{
		get
		{
			return Game1.game1.instanceTextEntry;
		}
		set
		{
			Game1.game1.instanceTextEntry = value;
		}
	}

	public static WorldDate Date => Game1.netWorldState.Value.Date;

	public static bool NetTimePaused => Game1.netWorldState.Get().IsTimePaused;

	public static bool HostPaused => Game1.netWorldState.Get().IsPaused;

	/// <summary>Whether the game is currently in multiplayer mode with at least one other player connected.</summary>
	public static bool IsMultiplayer => Game1.otherFarmers.Count > 0;

	/// <summary>Whether this game instance is a farmhand connected to a remote host in multiplayer.</summary>
	public static bool IsClient => Game1.multiplayerMode == 1;

	/// <summary>Whether this game instance is the host in multiplayer.</summary>
	public static bool IsServer => Game1.multiplayerMode == 2;

	/// <summary>Whether this game instance is the main or host player.</summary>
	public static bool IsMasterGame
	{
		get
		{
			if (Game1.multiplayerMode != 0)
			{
				return Game1.multiplayerMode == 2;
			}
			return true;
		}
	}

	/// <summary>Whether this game is hosted by an automated dedicated host</summary>
	public static bool HasDedicatedHost
	{
		get
		{
			if (Game1.multiplayerMode != 0)
			{
				return Game1.player?.team?.hasDedicatedHost.Value == true;
			}
			return false;
		}
	}

	/// <summary>Whether this game instance is acting as the host of a dedicated server.</summary>
	public static bool IsDedicatedHost
	{
		get
		{
			if (Game1.IsServer)
			{
				return Game1.HasDedicatedHost;
			}
			return false;
		}
	}

	/// <summary>The main or host player instance.</summary>
	public static Farmer MasterPlayer
	{
		get
		{
			if (!Game1.IsMasterGame)
			{
				return Game1.serverHost.Value;
			}
			return Game1.player;
		}
	}

	public static bool IsChatting
	{
		get
		{
			if (Game1.chatBox != null)
			{
				return Game1.chatBox.isActive();
			}
			return false;
		}
		set
		{
			if (value != Game1.chatBox.isActive())
			{
				if (value)
				{
					Game1.chatBox.activate();
				}
				else
				{
					Game1.chatBox.clickAway();
				}
			}
		}
	}

	public static Event CurrentEvent
	{
		get
		{
			if (Game1.currentLocation == null)
			{
				return null;
			}
			return Game1.currentLocation.currentEvent;
		}
	}

	public static MineShaft mine => (Game1.locationRequest?.Location as MineShaft) ?? (Game1.currentLocation as MineShaft);

	public static int CurrentMineLevel => (Game1.currentLocation as MineShaft)?.mineLevel ?? 0;

	public static int CurrentPlayerLimit
	{
		get
		{
			if (Game1.netWorldState?.Value != null)
			{
				_ = Game1.netWorldState.Value.CurrentPlayerLimit;
				return Game1.netWorldState.Value.CurrentPlayerLimit;
			}
			return Game1.multiplayer.playerLimit;
		}
	}

	private static float thumbstickToMouseModifier
	{
		get
		{
			if (Game1._cursorSpeedDirty)
			{
				Game1.ComputeCursorSpeed();
			}
			return Game1._cursorSpeed / 720f * (float)Game1.viewport.Height * (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
		}
	}

	public static bool isFullscreen => Game1.graphics.IsFullScreen;

	/// <summary>Get whether it's summer in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsSummerHere" /> to handle local seasons.</remarks>
	public static bool IsSummer => Game1.season == Season.Summer;

	/// <summary>Get whether it's spring in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsSpringHere" /> to handle local seasons.</remarks>
	public static bool IsSpring => Game1.season == Season.Spring;

	/// <summary>Get whether it's fall in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsFallHere" /> to handle local seasons.</remarks>
	public static bool IsFall => Game1.season == Season.Fall;

	/// <summary>Get whether it's winter in the valley.</summary>
	/// <remarks>See <see cref="M:StardewValley.GameLocation.IsWinterHere" /> to handle local seasons.</remarks>
	public static bool IsWinter => Game1.season == Season.Winter;

	public RenderTarget2D screen
	{
		get
		{
			return this._screen;
		}
		set
		{
			if (this._screen != null)
			{
				this._screen.Dispose();
				this._screen = null;
			}
			this._screen = value;
		}
	}

	public RenderTarget2D uiScreen
	{
		get
		{
			return this._uiScreen;
		}
		set
		{
			if (this._uiScreen != null)
			{
				this._uiScreen.Dispose();
				this._uiScreen = null;
			}
			this._uiScreen = value;
		}
	}

	public static float mouseCursorTransparency
	{
		get
		{
			return Game1._mouseCursorTransparency;
		}
		set
		{
			Game1._mouseCursorTransparency = value;
		}
	}

	public static void GetHasRoomAnotherFarmAsync(ReportHasRoomAnotherFarmDelegate callback)
	{
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			bool yes = Game1.GetHasRoomAnotherFarm();
			callback(yes);
			return;
		}
		Task task = new Task(delegate
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			bool hasRoomAnotherFarm = Game1.GetHasRoomAnotherFarm();
			callback(hasRoomAnotherFarm);
		});
		Game1.hooks.StartTask(task, "Farm_SpaceCheck");
	}

	private static string GameModeToString(byte mode)
	{
		return mode switch
		{
			4 => $"logoScreenGameMode ({mode})", 
			0 => $"titleScreenGameMode ({mode})", 
			1 => $"loadScreenGameMode ({mode})", 
			2 => $"newGameMode ({mode})", 
			3 => $"playingGameMode ({mode})", 
			6 => $"loadingMode ({mode})", 
			7 => $"saveMode ({mode})", 
			8 => $"saveCompleteMode ({mode})", 
			9 => $"selectGameScreen ({mode})", 
			10 => $"creditsMode ({mode})", 
			11 => $"errorLogMode ({mode})", 
			_ => $"unknown ({mode})", 
		};
	}

	/// <summary>Get a human-readable game version which includes the <see cref="F:StardewValley.Game1.version" />, <see cref="F:StardewValley.Game1.versionLabel" />, and <see cref="F:StardewValley.Game1.versionBuildNumber" />.</summary>
	public static string GetVersionString()
	{
		string label = Game1.version;
		if (!string.IsNullOrEmpty(Game1.versionLabel))
		{
			label = label + " '" + Game1.versionLabel + "'";
		}
		if (Game1.versionBuildNumber > 0)
		{
			label = label + " build " + Game1.versionBuildNumber;
		}
		return label;
	}

	public static void ResetToolSpriteSheet()
	{
		if (Game1._toolSpriteSheet != null)
		{
			Game1._toolSpriteSheet.Dispose();
			Game1._toolSpriteSheet = null;
		}
		Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\tools");
		int w = texture.Width;
		int h = texture.Height;
		Texture2D obj = new Texture2D(Game1.game1.GraphicsDevice, w, h, mipmap: false, SurfaceFormat.Color)
		{
			Name = texture.Name
		};
		Color[] data = new Color[w * h];
		texture.GetData(data);
		obj.SetData(data);
		Game1._toolSpriteSheet = obj;
	}

	public static void SetSaveName(string new_save_name)
	{
		if (new_save_name == null)
		{
			new_save_name = "";
		}
		Game1._currentSaveName = new_save_name;
		Game1._setSaveName = true;
	}

	public static string GetSaveGameName(bool set_value = true)
	{
		if (!Game1._setSaveName && set_value)
		{
			string base_name = Game1.MasterPlayer.farmName.Value;
			string save_name = base_name;
			int collision_index = 2;
			while (SaveGame.IsNewGameSaveNameCollision(save_name))
			{
				save_name = base_name + collision_index;
				collision_index++;
			}
			Game1.SetSaveName(save_name);
		}
		return Game1._currentSaveName;
	}

	private static void allocateLightmap(int width, int height)
	{
		int quality = 8;
		float zoom = 1f;
		if (Game1.options != null)
		{
			quality = Game1.options.lightingQuality;
			zoom = ((!Game1.game1.useUnscaledLighting) ? Game1.options.zoomLevel : 1f);
		}
		int w = (int)((float)width * (1f / zoom) + 64f) / (quality / 2);
		int h = (int)((float)height * (1f / zoom) + 64f) / (quality / 2);
		RenderTarget2D renderTarget2D = Game1.lightmap;
		if (renderTarget2D == null || renderTarget2D.Width != w || Game1.lightmap.Height != h)
		{
			Game1._lightmap?.Dispose();
			Game1._lightmap = new RenderTarget2D(Game1.graphics.GraphicsDevice, w, h, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}

	public static bool canHaveWeddingOnDay(int day, Season season)
	{
		if (!Utility.isFestivalDay(day, season))
		{
			return !Utility.isGreenRainDay(day, season);
		}
		return false;
	}

	/// <summary>Reset the <see cref="P:StardewValley.Game1.questOfTheDay" /> for today and synchronize it to other player. In multiplayer, this can only be called on the host instance.</summary>
	public static void RefreshQuestOfTheDay()
	{
		Quest quest = ((!Utility.isFestivalDay() && !Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season)) ? Utility.getQuestOfTheDay() : null);
		quest?.dailyQuest.Set(newValue: true);
		quest?.reloadObjective();
		quest?.reloadDescription();
		Game1.netWorldState.Value.SetQuestOfTheDay(quest);
	}

	public static void ExitToTitle(Action postExitCallback = null)
	{
		Game1.currentMinigame?.unload();
		Game1._requestedMusicTracks.Clear();
		Game1.UpdateRequestedMusicTrack();
		Game1.changeMusicTrack("none");
		Game1.setGameMode(0);
		Game1.exitToTitle = true;
		Game1.postExitToTitleCallback = postExitCallback;
	}

	static Game1()
	{
		Game1.realMilliSecondsPerGameMinute = 700;
		Game1.realMilliSecondsPerGameTenMinutes = Game1.realMilliSecondsPerGameMinute * 10;
		Game1.cursor_none = -1;
		Game1.cursor_default = 0;
		Game1.cursor_wait = 1;
		Game1.cursor_grab = 2;
		Game1.cursor_gift = 3;
		Game1.cursor_talk = 4;
		Game1.cursor_look = 5;
		Game1.cursor_harvest = 6;
		Game1.cursor_gamepad_pointer = 44;
		Game1.asianSpacingRegexString = "\\s|[（《“‘「『(](?:[\\w,%％]+|[^…—])[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[）》”’」』)，。、？！：；·～,.?!:;~…]*|.[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[·・].[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*|(?:[\\w,%％]+|[^…—])[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[）》”’」』)]?(?:[，。、？！：；·～,.?!:;~…]{1,2}[）》”’」』)]?)?|[\\w,%％]+|.[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]+|……|——|.";
		Game1.FinishedIncrementalLoad = false;
		Game1.FinishedFirstLoadContent = false;
		Game1.FinishedFirstInitSounds = false;
		Game1.FinishedFirstInitSerializers = false;
		Game1.MusicDuckTimer = 0f;
		Game1.thumbStickSensitivity = 0.1f;
		Game1.runThreshold = 0.5f;
		Game1.rightStickHoldTime = 0;
		Game1.emoteMenuShowTime = 250;
		Game1.nextFarmerWarpOffsetX = 0;
		Game1.nextFarmerWarpOffsetY = 0;
		Game1.keyboardFocusInstance = null;
		Game1._isWarping = false;
		Game1.hasLocalClientsOnly = false;
		Game1.isUsingBackToFrontSorting = false;
		Game1._debugStringBuilder = new StringBuilder();
		Game1.debugTimings = new DebugTimings();
		Game1._locationLookup = new Dictionary<string, GameLocation>(StringComparer.OrdinalIgnoreCase);
		Game1.asianSpacingRegex = new Regex(Game1.asianSpacingRegexString, RegexOptions.ECMAScript);
		Game1.warpingForForcedRemoteEvent = false;
		Game1._PreviousNonNullLocation = null;
		Game1.npcDialogues = new Dictionary<string, Stack<Dialogue>>();
		Game1.morningQueue = new Queue<Action>();
		Game1.hooks = new ModHooks();
		Game1.input = new InputState();
		Game1.inputSimulator = null;
		Game1._toolSpriteSheet = null;
		Game1.crabPotOverlayTiles = new Dictionary<Vector2, int>();
		Game1._setSaveName = false;
		Game1._currentSaveName = "";
		Game1.mailDeliveredFromMailForTomorrow = new List<string>();
		Game1.dynamicPixelRects = new Texture2D[3];
		Game1.screenGlowAlpha = 0f;
		Game1.flashAlpha = 0f;
		Game1.currentGemBirdIndex = 0;
		Game1.dialogueUp = false;
		Game1.dialogueTyping = false;
		Game1.isQuestion = false;
		Game1.newDay = false;
		Game1.eventUp = false;
		Game1.viewportFreeze = false;
		Game1.eventOver = false;
		Game1.screenGlow = false;
		Game1.screenGlowHold = false;
		Game1.killScreen = false;
		Game1.displayHUD = true;
		Game1.displayFarmer = true;
		Game1.showingHealth = false;
		Game1.cabinsSeparate = false;
		Game1.showingHealthBar = false;
		Game1.hasStartedDay = false;
		Game1.eventsSeenSinceLastLocationChange = new HashSet<string>();
		Game1.hasApplied1_3_UpdateChanges = false;
		Game1.hasApplied1_4_UpdateChanges = false;
		Game1.postExitToTitleCallback = null;
		Game1.bundleType = BundleType.Default;
		Game1.isRaining = false;
		Game1.isSnowing = false;
		Game1.isLightning = false;
		Game1.isDebrisWeather = false;
		Game1._isGreenRain = false;
		Game1.wasGreenRain = false;
		Game1.greenRainNeedsCleanup = false;
		Game1.season = Season.Spring;
		Game1.bannedUsers = new SerializableDictionary<string, string>();
		Game1._debugOutputLock = new object();
		Game1.requestedMusicTrack = "";
		Game1.messageAfterPause = "";
		Game1.samBandName = "The Alfalfas";
		Game1.loadingMessage = "";
		Game1.errorMessage = "";
		Game1.requestedMusicDirty = false;
		Game1._shortDayDisplayName = new string[7];
		Game1.currentObjectDialogue = new Queue<string>();
		Game1.worldStateIDs = new HashSet<string>();
		Game1.questionChoices = new List<Response>();
		Game1.dayOfMonth = 0;
		Game1.year = 1;
		Game1.timeOfDay = 600;
		Game1.timeOfDayAfterFade = -1;
		Game1.whichModFarm = null;
		Game1.startingGameSeed = null;
		Game1.elliottPiano = 0;
		Game1.viewportClampArea = Microsoft.Xna.Framework.Rectangle.Empty;
		Game1.eveningColor = new Color(255, 255, 0);
		Game1.unselectedOptionColor = new Color(100, 100, 100);
		Game1.random = new Random();
		Game1.recentMultiplayerRandom = new Random();
		Game1.hudMessages = new List<HUDMessage>();
		Game1.dialogueButtonScale = 1f;
		Game1.lastCursorTile = Vector2.Zero;
		Game1.rainDrops = new RainDrop[70];
		Game1.loopingLocationCues = new LoopingCueManager();
		Game1.sounds = new SoundsHelper();
		Game1.CueModification = new AudioCueModificationManager();
		Game1.debrisWeather = new List<WeatherDebris>();
		Game1.screenOverlayTempSprites = new TemporaryAnimatedSpriteList();
		Game1.uiOverlayTempSprites = new TemporaryAnimatedSpriteList();
		Game1.log = new DefaultLogger(!Program.releaseBuild, shouldWriteToLogFile: false);
		Game1.hash = new HashUtility();
		Game1.multiplayer = new Multiplayer();
		Game1.uniqueIDForThisGame = Utility.NewUniqueIdForThisGame();
		Game1.directionKeyPolling = new int[4];
		Game1.currentLightSources = new Dictionary<string, LightSource>();
		Game1.outdoorLight = new Color(255, 255, 0);
		Game1.textColor = new Color(34, 17, 34);
		Game1.textShadowColor = new Color(206, 156, 95);
		Game1.textShadowDarkerColor = new Color(221, 148, 84);
		Game1.nextClickableMenu = new List<IClickableMenu>();
		Game1.actionsWhenPlayerFree = new List<Action>();
		Game1.isCheckingNonMousePlacement = false;
		Game1._currentMinigame = null;
		Game1.onScreenMenus = new List<IClickableMenu>();
		Game1._onlineFarmers = new FarmerCollection();
		Game1.delayedActions = new List<DelayedAction>();
		Game1.endOfNightMenus = new Stack<IClickableMenu>();
		Game1.splitscreenOptions = new SerializableDictionary<long, Options>();
		Game1.CustomData = new SerializableDictionary<string, string>();
		Game1.netReady = new ReadySynchronizer();
		Game1.dedicatedServer = new DedicatedServer();
		Game1.specialCurrencyDisplay = null;
		Game1.remoteEventQueue = new List<Action>();
		Game1.weddingsToday = new List<long>();
		Game1.viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
		Game1.viewportSpeed = 2f;
		Game1._cursorDragEnabled = false;
		Game1._cursorDragPrevEnabled = false;
		Game1._cursorSpeedDirty = true;
		Game1._cursorSpeed = 16f;
		Game1._cursorSpeedScale = 1f;
		Game1._cursorUpdateElapsedSec = 0f;
		Game1.newDaySync = new NewDaySynchronizer();
		Game1.forceSnapOnNextViewportUpdate = false;
		Game1.screenGlowRate = 0.005f;
		Game1.haltAfterCheck = false;
		Game1.uiMode = false;
		Game1.nonUIRenderTarget = null;
		Game1.uiModeCount = 0;
		Game1._oldUIModeCount = 0;
		Game1.conventionMode = false;
		Game1.isRunningMacro = false;
		Game1.thumbstickMotionAccell = 1f;
		Game1.bgColor = new Color(5, 3, 4);
		Game1.isRenderingScreenBuffer = false;
		Game1._activatedTick = 0;
		Game1.mouseCursor = Game1.cursor_default;
		Game1._mouseCursorTransparency = 1f;
		Game1.wasMouseVisibleThisFrame = true;
		Game1._ParseTextStringBuilder = new StringBuilder(2408);
		Game1._ParseTextStringBuilderLine = new StringBuilder(1024);
		Game1._ParseTextStringBuilderWord = new StringBuilder(256);
		Game1.GameAssemblyName = typeof(Game1).Assembly.GetName().Name;
		AssemblyInformationalVersionAttribute attribute = typeof(Game1).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (!string.IsNullOrWhiteSpace(attribute?.InformationalVersion))
		{
			string[] parts = attribute.InformationalVersion.Split(',');
			if (parts.Length == 4)
			{
				Game1.version = parts[0].Trim();
				if (!string.IsNullOrWhiteSpace(parts[1]))
				{
					Game1.versionLabel = parts[1].Trim();
				}
				if (!string.IsNullOrWhiteSpace(parts[2]))
				{
					if (!int.TryParse(parts[2], out var buildNumber))
					{
						throw new InvalidOperationException("Can't parse game build number value '" + parts[2] + "' as a number.");
					}
					Game1.versionBuildNumber = buildNumber;
				}
				if (!string.IsNullOrWhiteSpace(parts[3]))
				{
					Multiplayer.protocolVersionOverride = parts[3].Trim();
				}
			}
		}
		if (string.IsNullOrWhiteSpace(Game1.version))
		{
			throw new InvalidOperationException("No game version found in assembly info.");
		}
	}

	public Game1(PlayerIndex player_index, int index)
		: this()
	{
		this.instancePlayerOneIndex = player_index;
		this.instanceIndex = index;
	}

	public Game1()
	{
		this.instanceId = GameRunner.instance.GetNewInstanceID();
		if (Program.gamePtr == null)
		{
			Program.gamePtr = this;
		}
		Game1._temporaryContent = this.CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
	}

	public void TranslateFields()
	{
		LocalizedContentManager.localizedAssetNames.Clear();
		BaseEnchantment.ResetEnchantments();
		Game1.samBandName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
		Game1.elliottBookName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157");
		Game1.objectSpriteSheet = Game1.content.Load<Texture2D>("Maps\\springobjects");
		Game1.objectSpriteSheet_2 = Game1.content.Load<Texture2D>("TileSheets\\Objects_2");
		Game1.bobbersTexture = Game1.content.Load<Texture2D>("TileSheets\\bobbers");
		Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
		Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
		Game1.smallFont.LineSpacing = 28;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.ko:
			Game1.smallFont.LineSpacing += 16;
			break;
		case LocalizedContentManager.LanguageCode.tr:
			Game1.smallFont.LineSpacing += 4;
			break;
		case LocalizedContentManager.LanguageCode.mod:
			Game1.smallFont.LineSpacing = LocalizedContentManager.CurrentModLanguage.SmallFontLineSpacing;
			break;
		}
		Game1.tinyFont = Game1.content.Load<SpriteFont>("Fonts\\tinyFont");
		Game1.objectData = DataLoader.Objects(Game1.content);
		Game1.bigCraftableData = DataLoader.BigCraftables(Game1.content);
		Game1.achievements = DataLoader.Achievements(Game1.content);
		CraftingRecipe.craftingRecipes = DataLoader.CraftingRecipes(Game1.content);
		CraftingRecipe.cookingRecipes = DataLoader.CookingRecipes(Game1.content);
		ItemRegistry.ResetCache();
		MovieTheater.ClearCachedLocalizedData();
		Game1.mouseCursors = Game1.content.Load<Texture2D>("LooseSprites\\Cursors");
		Game1.mouseCursors2 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors2");
		Game1.mouseCursors_1_6 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors_1_6");
		Game1.giftboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\Giftbox");
		Game1.controllerMaps = Game1.content.Load<Texture2D>("LooseSprites\\ControllerMaps");
		Game1.NPCGiftTastes = DataLoader.NpcGiftTastes(Game1.content);
		Game1._shortDayDisplayName[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
		Game1._shortDayDisplayName[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
		Game1._shortDayDisplayName[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
		Game1._shortDayDisplayName[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
		Game1._shortDayDisplayName[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
		Game1._shortDayDisplayName[5] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
		Game1._shortDayDisplayName[6] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
	}

	public void exitEvent(object sender, EventArgs e)
	{
		Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ClosedGame);
		Game1.keyboardDispatcher.Cleanup();
	}

	public void refreshWindowSettings()
	{
		GameRunner.instance.OnWindowSizeChange(null, null);
	}

	public void Window_ClientSizeChanged(object sender, EventArgs e)
	{
		if (this._windowResizing)
		{
			return;
		}
		Game1.log.Verbose("Window_ClientSizeChanged(); Window.ClientBounds=" + base.Window.ClientBounds.ToString());
		if (Game1.options == null)
		{
			Game1.log.Verbose("Window_ClientSizeChanged(); options is null, returning.");
			return;
		}
		this._windowResizing = true;
		int w = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferWidth : base.Window.ClientBounds.Width);
		int h = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferHeight : base.Window.ClientBounds.Height);
		GameRunner.instance.ExecuteForInstances(delegate(Game1 instance)
		{
			instance.SetWindowSize(w, h);
		});
		this._windowResizing = false;
	}

	public virtual void SetWindowSize(int w, int h)
	{
		Microsoft.Xna.Framework.Rectangle oldWindow = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			if (w < 1280 && !Game1.graphics.IsFullScreen)
			{
				w = 1280;
			}
			if (h < 720 && !Game1.graphics.IsFullScreen)
			{
				h = 720;
			}
		}
		if (!Game1.graphics.IsFullScreen && base.Window.AllowUserResizing)
		{
			Game1.graphics.PreferredBackBufferWidth = w;
			Game1.graphics.PreferredBackBufferHeight = h;
		}
		if (base.IsMainInstance && Game1.graphics.SynchronizeWithVerticalRetrace != Game1.options.vsyncEnabled)
		{
			Game1.graphics.SynchronizeWithVerticalRetrace = Game1.options.vsyncEnabled;
			Game1.log.Verbose("Vsync toggled: " + Game1.graphics.SynchronizeWithVerticalRetrace);
		}
		Game1.graphics.ApplyChanges();
		try
		{
			if (Game1.graphics.IsFullScreen)
			{
				this.localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
			}
			else
			{
				this.localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, w, h);
			}
		}
		catch (Exception)
		{
		}
		Game1.defaultDeviceViewport = new Viewport(this.localMultiplayerWindow);
		List<Vector4> screen_splits = new List<Vector4>();
		if (GameRunner.instance.gameInstances.Count <= 1)
		{
			screen_splits.Add(new Vector4(0f, 0f, 1f, 1f));
		}
		else
		{
			switch (GameRunner.instance.gameInstances.Count)
			{
			case 2:
				screen_splits.Add(new Vector4(0f, 0f, 0.5f, 1f));
				screen_splits.Add(new Vector4(0.5f, 0f, 0.5f, 1f));
				break;
			case 3:
				screen_splits.Add(new Vector4(0f, 0f, 1f, 0.5f));
				screen_splits.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
				break;
			case 4:
				screen_splits.Add(new Vector4(0f, 0f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0.5f, 0f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
				screen_splits.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
				break;
			}
		}
		if (GameRunner.instance.gameInstances.Count <= 1)
		{
			this.zoomModifier = 1f;
		}
		else
		{
			this.zoomModifier = 0.5f;
		}
		Vector4 current_screen_split = screen_splits[Game1.game1.instanceIndex];
		Vector2? old_ui_dimensions = null;
		if (this.uiScreen != null)
		{
			old_ui_dimensions = new Vector2(this.uiScreen.Width, this.uiScreen.Height);
		}
		this.localMultiplayerWindow.X = (int)((float)w * current_screen_split.X);
		this.localMultiplayerWindow.Y = (int)((float)h * current_screen_split.Y);
		this.localMultiplayerWindow.Width = (int)Math.Ceiling((float)w * current_screen_split.Z);
		this.localMultiplayerWindow.Height = (int)Math.Ceiling((float)h * current_screen_split.W);
		try
		{
			int sw = (int)Math.Ceiling((float)this.localMultiplayerWindow.Width * (1f / Game1.options.zoomLevel));
			int sh = (int)Math.Ceiling((float)this.localMultiplayerWindow.Height * (1f / Game1.options.zoomLevel));
			this.screen = new RenderTarget2D(Game1.graphics.GraphicsDevice, sw, sh, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents)
			{
				Name = "@Game1.screen"
			};
			int uw = (int)Math.Ceiling((float)this.localMultiplayerWindow.Width / Game1.options.uiScale);
			int uh = (int)Math.Ceiling((float)this.localMultiplayerWindow.Height / Game1.options.uiScale);
			this.uiScreen = new RenderTarget2D(Game1.graphics.GraphicsDevice, uw, uh, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents)
			{
				Name = "@Game1.uiScreen"
			};
		}
		catch (Exception)
		{
		}
		Game1.updateViewportForScreenSizeChange(fullscreenChange: false, this.localMultiplayerWindow.Width, this.localMultiplayerWindow.Height);
		if (old_ui_dimensions.HasValue && old_ui_dimensions.Value.X == (float)this.uiScreen.Width && old_ui_dimensions.Value.Y == (float)this.uiScreen.Height)
		{
			return;
		}
		Game1.PushUIMode();
		Game1.textEntry?.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
		foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
		{
			onScreenMenu.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
		}
		Game1.currentMinigame?.changeScreenSize();
		Game1.activeClickableMenu?.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
		if (Game1.activeClickableMenu?.GetType() == typeof(GameMenu))
		{
			GameMenu obj = Game1.activeClickableMenu as GameMenu;
			IClickableMenu oldPage = obj.GetCurrentPage();
			IClickableMenu currentPage = ((GameMenu)(Game1.activeClickableMenu = new GameMenu(obj.currentTab))).GetCurrentPage();
			if (!(currentPage is CollectionsPage collectionsPage))
			{
				if (!(currentPage is OptionsPage optionsPage))
				{
					if (currentPage is SocialPage socialPage)
					{
						socialPage.postWindowSizeChange(oldPage);
					}
				}
				else
				{
					optionsPage.postWindowSizeChange(oldPage);
				}
			}
			else
			{
				collectionsPage.postWindowSizeChange(oldPage);
			}
		}
		Game1.PopUIMode();
	}

	private void Game1_Exiting(object sender, EventArgs e)
	{
		Program.sdk.Shutdown();
	}

	public static void setGameMode(byte mode)
	{
		Game1.log.Verbose("setGameMode( '" + Game1.GameModeToString(mode) + "' )");
		Game1._gameMode = mode;
		Game1.temporaryContent?.Unload();
		if (mode != 0)
		{
			return;
		}
		bool skip = false;
		if (Game1.activeClickableMenu != null)
		{
			GameTime gameTime = Game1.currentGameTime;
			if (gameTime != null && gameTime.TotalGameTime.TotalSeconds > 10.0)
			{
				skip = true;
			}
		}
		if (Game1.game1.instanceIndex <= 0)
		{
			TitleMenu titleMenu = (TitleMenu)(Game1.activeClickableMenu = new TitleMenu());
			if (skip)
			{
				titleMenu.skipToTitleButtons();
			}
		}
	}

	public static void updateViewportForScreenSizeChange(bool fullscreenChange, int width, int height)
	{
		Game1.forceSnapOnNextViewportUpdate = true;
		if (Game1.graphics.GraphicsDevice != null)
		{
			Game1.allocateLightmap(width, height);
		}
		width = (int)Math.Ceiling((float)width / Game1.options.zoomLevel);
		height = (int)Math.Ceiling((float)height / Game1.options.zoomLevel);
		Point center = new Point(Game1.viewport.X + Game1.viewport.Width / 2, Game1.viewport.Y + Game1.viewport.Height / 2);
		bool sizeDirty = Game1.viewport.Width != width || Game1.viewport.Height != height;
		Game1.viewport = new xTile.Dimensions.Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
		if (Game1.currentLocation == null)
		{
			return;
		}
		if (Game1.eventUp)
		{
			if (!Game1.IsFakedBlackScreen() && Game1.currentLocation.IsOutdoors)
			{
				Game1.clampViewportToGameMap();
			}
			return;
		}
		if (Game1.viewport.X >= 0 || !Game1.currentLocation.IsOutdoors || fullscreenChange)
		{
			center = new Point(Game1.viewport.X + Game1.viewport.Width / 2, Game1.viewport.Y + Game1.viewport.Height / 2);
			Game1.viewport = new xTile.Dimensions.Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
			Game1.UpdateViewPort(overrideFreeze: true, center);
		}
		if (sizeDirty)
		{
			Game1.forceSnapOnNextViewportUpdate = true;
			Game1.randomizeRainPositions();
			Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
		}
	}

	public void Instance_Initialize()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		this.Initialize();
		stopwatch.Stop();
		Game1.log.Verbose($"Instance_Initialize() finished, elapsed = '{stopwatch.Elapsed}'");
	}

	public static bool IsFading()
	{
		if (!Game1.globalFade && (!Game1.fadeIn || !(Game1.fadeToBlackAlpha > 0f)))
		{
			if (Game1.fadeToBlack)
			{
				return Game1.fadeToBlackAlpha < 1f;
			}
			return false;
		}
		return true;
	}

	public static bool IsFakedBlackScreen()
	{
		if (Game1.currentMinigame != null)
		{
			return false;
		}
		if (Game1.CurrentEvent != null && Game1.CurrentEvent.currentCustomEventScript != null)
		{
			return false;
		}
		if (!Game1.eventUp)
		{
			return false;
		}
		return (float)(int)Math.Floor((float)new Point(Game1.viewport.X + Game1.viewport.Width / 2, Game1.viewport.Y + Game1.viewport.Height / 2).X / 64f) <= -200f;
	}

	private void DoThreadedInitTask(ThreadStart initTask)
	{
		if (this.ShouldLoadIncrementally)
		{
			Thread thread = new Thread(initTask);
			thread.CurrentCulture = CultureInfo.InvariantCulture;
			thread.Priority = ThreadPriority.Highest;
			thread.Start();
		}
		else
		{
			initTask();
		}
	}

	/// <summary>
	/// Allows the game to perform any initialization it needs to before starting to run.
	/// This is where it can query for any required services and load any non-graphic
	/// related content.  Calling base.Initialize will enumerate through any components
	/// and initialize them as well.
	/// </summary>
	protected override void Initialize()
	{
		Game1.keyboardDispatcher = new KeyboardDispatcher(base.Window);
		Game1.screenFade = new ScreenFade(onFadeToBlackComplete, onFadedBackInComplete);
		Game1.options = new Options();
		Game1.options.musicVolumeLevel = 1f;
		Game1.options.soundVolumeLevel = 1f;
		Game1.otherFarmers = new NetRootDictionary<long, Farmer>();
		this.DoThreadedInitTask(InitializeSerializers);
		Game1.viewport = new xTile.Dimensions.Rectangle(new Size(Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight));
		Game1.currentSong = null;
		this.DoThreadedInitTask(InitializeSounds);
		int width = Game1.graphics.GraphicsDevice.Viewport.Width;
		int height = Game1.graphics.GraphicsDevice.Viewport.Height;
		this.screen = new RenderTarget2D(Game1.graphics.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		Game1.allocateLightmap(width, height);
		Game1.previousViewportPosition = Vector2.Zero;
		Game1.PushUIMode();
		Game1.PopUIMode();
		Game1.setRichPresence("menus");
	}

	private void InitializeSounds()
	{
		if (base.IsMainInstance)
		{
			try
			{
				string rootpath = base.Content.RootDirectory;
				AudioEngine obj = new AudioEngine(Path.Combine(rootpath, "XACT", "FarmerSounds.xgs"));
				obj.GetReverbSettings()[18] = 4f;
				obj.GetReverbSettings()[17] = -12f;
				Game1.audioEngine = new AudioEngineWrapper(obj);
				Game1.waveBank = new WaveBank(Game1.audioEngine.Engine, Path.Combine(rootpath, "XACT", "Wave Bank.xwb"));
				Game1.waveBank1_4 = new WaveBank(Game1.audioEngine.Engine, Path.Combine(rootpath, "XACT", "Wave Bank(1.4).xwb"));
				Game1.soundBank = new SoundBankWrapper(new SoundBank(Game1.audioEngine.Engine, Path.Combine(rootpath, "XACT", "Sound Bank.xsb")));
			}
			catch (Exception exception)
			{
				Game1.log.Error("Game.Initialize() caught exception initializing XACT.", exception);
				Game1.audioEngine = new DummyAudioEngine();
				Game1.soundBank = new DummySoundBank();
			}
		}
		Game1.audioEngine.Update();
		Game1.musicCategory = Game1.audioEngine.GetCategory("Music");
		Game1.soundCategory = Game1.audioEngine.GetCategory("Sound");
		Game1.ambientCategory = Game1.audioEngine.GetCategory("Ambient");
		Game1.footstepCategory = Game1.audioEngine.GetCategory("Footsteps");
		Game1.wind = Game1.soundBank.GetCue("wind");
		Game1.chargeUpSound = Game1.soundBank.GetCue("toolCharge");
		AmbientLocationSounds.InitShared();
		Game1.FinishedFirstInitSounds = true;
	}

	private void InitializeSerializers()
	{
		Game1.otherFarmers.Serializer = SaveSerializer.GetSerializer(typeof(Farmer));
		if (StartupPreferences.serializer == null)
		{
			StartupPreferences.serializer = SaveSerializer.GetSerializer(typeof(StartupPreferences));
		}
		Game1.FinishedFirstInitSerializers = true;
	}

	public static void pauseThenDoFunction(int pauseTime, afterFadeFunction function)
	{
		Game1.afterPause = function;
		Game1.pauseThenDoFunctionTimer = pauseTime;
	}

	/// <summary>Construct a content manager to read game content files.</summary>
	/// <param name="serviceProvider">The service provider to use to locate services.</param>
	/// <param name="rootDirectory">The root directory to search for content.</param>
	protected internal virtual LocalizedContentManager CreateContentManager(IServiceProvider serviceProvider, string rootDirectory)
	{
		return new LocalizedContentManager(serviceProvider, rootDirectory);
	}

	/// <summary>Create an xTile map display device.</summary>
	/// <param name="content">The content manager through which it should load tilesheet textures.</param>
	/// <param name="graphicsDevice">The XNA graphics device.</param>
	protected internal virtual IDisplayDevice CreateDisplayDevice(ContentManager content, GraphicsDevice graphicsDevice)
	{
		return new XnaDisplayDevice(content, graphicsDevice);
	}

	public void Instance_LoadContent()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		this.LoadContent();
		stopwatch.Stop();
		Game1.log.Verbose($"Instance_LoadContent() finished, elapsed = '{stopwatch.Elapsed}'");
	}

	/// <summary>LoadContent will be called once per game and is the place to load all of your content.</summary>
	protected override void LoadContent()
	{
		Game1.content = this.CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
		this.xTileContent = this.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
		Game1.mapDisplayDevice = this.CreateDisplayDevice(Game1.content, base.GraphicsDevice);
		Game1.spriteBatch = new SpriteBatch(base.GraphicsDevice);
		Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
		Game1.LoadContentEnumerator = this.GetLoadContentEnumerator();
		if (!this.ShouldLoadIncrementally)
		{
			while (Game1.LoadContentEnumerator.MoveNext())
			{
			}
			Game1.LoadContentEnumerator = null;
			this.AfterLoadContent();
		}
	}

	private void AfterLoadContent()
	{
		Game1.saveOnNewDay = true;
		if (Game1.gameMode == 4)
		{
			Game1.fadeToBlackAlpha = -0.5f;
			Game1.fadeIn = true;
		}
		if (Game1.random.NextDouble() < 0.7)
		{
			Game1.isDebrisWeather = true;
			Game1.populateDebrisWeatherArray();
		}
		Game1.resetPlayer();
		Game1.CueModification.OnStartup();
		Game1.setGameMode(0);
	}

	private IEnumerator<int> GetLoadContentEnumerator()
	{
		int step = 0;
		Game1.bigCraftableData = DataLoader.BigCraftables(Game1.content);
		int num = step + 1;
		step = num;
		yield return num;
		Game1.objectData = DataLoader.Objects(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.cropData = DataLoader.Crops(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.characterData = DataLoader.Characters(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.pantsData = DataLoader.Pants(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.shirtData = DataLoader.Shirts(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.toolData = DataLoader.Tools(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.weaponData = DataLoader.Weapons(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.achievements = DataLoader.Achievements(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.buildingData = DataLoader.Buildings(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.farmAnimalData = DataLoader.FarmAnimals(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.floorPathData = DataLoader.FloorsAndPaths(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.fruitTreeData = DataLoader.FruitTrees(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.locationData = DataLoader.Locations(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.locationContextData = DataLoader.LocationContexts(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.petData = DataLoader.Pets(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		Game1.NPCGiftTastes = DataLoader.NpcGiftTastes(Game1.content);
		num = step + 1;
		step = num;
		yield return num;
		CraftingRecipe.InitShared();
		num = step + 1;
		step = num;
		yield return num;
		ItemRegistry.ResetCache();
		num = step + 1;
		step = num;
		yield return num;
		Game1.jukeboxTrackData = new Dictionary<string, JukeboxTrackData>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, JukeboxTrackData> pair in DataLoader.JukeboxTracks(Game1.content))
		{
			if (!Game1.jukeboxTrackData.TryAdd(pair.Key, pair.Value))
			{
				Game1.log.Warn("Ignored duplicate ID '" + pair.Key + "' in Data/JukeboxTracks.");
			}
		}
		num = step + 1;
		step = num;
		yield return num;
		Game1.concessionsSpriteSheet = Game1.content.Load<Texture2D>("LooseSprites\\Concessions");
		num = step + 1;
		step = num;
		yield return num;
		Game1.birdsSpriteSheet = Game1.content.Load<Texture2D>("LooseSprites\\birds");
		num = step + 1;
		step = num;
		yield return num;
		Game1.daybg = Game1.content.Load<Texture2D>("LooseSprites\\daybg");
		num = step + 1;
		step = num;
		yield return num;
		Game1.nightbg = Game1.content.Load<Texture2D>("LooseSprites\\nightbg");
		num = step + 1;
		step = num;
		yield return num;
		Game1.menuTexture = Game1.content.Load<Texture2D>("Maps\\MenuTiles");
		num = step + 1;
		step = num;
		yield return num;
		Game1.uncoloredMenuTexture = Game1.content.Load<Texture2D>("Maps\\MenuTilesUncolored");
		num = step + 1;
		step = num;
		yield return num;
		Game1.lantern = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\lantern");
		num = step + 1;
		step = num;
		yield return num;
		Game1.windowLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\windowLight");
		num = step + 1;
		step = num;
		yield return num;
		Game1.sconceLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\sconceLight");
		num = step + 1;
		step = num;
		yield return num;
		Game1.cauldronLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\greenLight");
		num = step + 1;
		step = num;
		yield return num;
		Game1.indoorWindowLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\indoorWindowLight");
		num = step + 1;
		step = num;
		yield return num;
		Game1.shadowTexture = Game1.content.Load<Texture2D>("LooseSprites\\shadow");
		num = step + 1;
		step = num;
		yield return num;
		Game1.mouseCursors = Game1.content.Load<Texture2D>("LooseSprites\\Cursors");
		num = step + 1;
		step = num;
		yield return num;
		Game1.mouseCursors2 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors2");
		num = step + 1;
		step = num;
		yield return num;
		Game1.mouseCursors_1_6 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors_1_6");
		num = step + 1;
		step = num;
		yield return num;
		Game1.giftboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\Giftbox");
		num = step + 1;
		step = num;
		yield return num;
		Game1.controllerMaps = Game1.content.Load<Texture2D>("LooseSprites\\ControllerMaps");
		num = step + 1;
		step = num;
		yield return num;
		Game1.animations = Game1.content.Load<Texture2D>("TileSheets\\animations");
		num = step + 1;
		step = num;
		yield return num;
		Game1.objectSpriteSheet = Game1.content.Load<Texture2D>("Maps\\springobjects");
		num = step + 1;
		step = num;
		yield return num;
		Game1.objectSpriteSheet_2 = Game1.content.Load<Texture2D>("TileSheets\\Objects_2");
		num = step + 1;
		step = num;
		yield return num;
		Game1.bobbersTexture = Game1.content.Load<Texture2D>("TileSheets\\bobbers");
		num = step + 1;
		step = num;
		yield return num;
		Game1.cropSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\crops");
		num = step + 1;
		step = num;
		yield return num;
		Game1.emoteSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\emotes");
		num = step + 1;
		step = num;
		yield return num;
		Game1.debrisSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\debris");
		num = step + 1;
		step = num;
		yield return num;
		Game1.bigCraftableSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\Craftables");
		num = step + 1;
		step = num;
		yield return num;
		Game1.rainTexture = Game1.content.Load<Texture2D>("TileSheets\\rain");
		num = step + 1;
		step = num;
		yield return num;
		Game1.buffsIcons = Game1.content.Load<Texture2D>("TileSheets\\BuffsIcons");
		num = step + 1;
		step = num;
		yield return num;
		Tool.weaponsTexture = Game1.content.Load<Texture2D>("TileSheets\\weapons");
		num = step + 1;
		step = num;
		yield return num;
		FarmerRenderer.hairStylesTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\hairstyles");
		num = step + 1;
		step = num;
		yield return num;
		FarmerRenderer.shirtsTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\shirts");
		num = step + 1;
		step = num;
		yield return num;
		FarmerRenderer.pantsTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\pants");
		num = step + 1;
		step = num;
		yield return num;
		FarmerRenderer.hatsTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\hats");
		num = step + 1;
		step = num;
		yield return num;
		FarmerRenderer.accessoriesTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\accessories");
		num = step + 1;
		step = num;
		yield return num;
		MapSeat.mapChairTexture = Game1.content.Load<Texture2D>("TileSheets\\ChairTiles");
		num = step + 1;
		step = num;
		yield return num;
		SpriteText.spriteTexture = Game1.content.Load<Texture2D>("LooseSprites\\font_bold");
		num = step + 1;
		step = num;
		yield return num;
		SpriteText.coloredTexture = Game1.content.Load<Texture2D>("LooseSprites\\font_colored");
		num = step + 1;
		step = num;
		yield return num;
		Projectile.projectileSheet = Game1.content.Load<Texture2D>("TileSheets\\Projectiles");
		num = step + 1;
		step = num;
		yield return num;
		Color[] white = new Color[1] { Color.White };
		for (int i = 0; i < Game1.dynamicPixelRects.Length; i++)
		{
			Game1.dynamicPixelRects[i] = new Texture2D(base.GraphicsDevice, 1, 1, mipmap: false, SurfaceFormat.Color)
			{
				Name = $"@{"Game1"}.{"dynamicPixelRects"}[{i}]"
			};
			Game1.dynamicPixelRects[i].SetData(white);
		}
		Game1.fadeToBlackRect = Game1.dynamicPixelRects[0];
		Game1.staminaRect = Game1.dynamicPixelRects[1];
		Game1.lightingRect = Game1.dynamicPixelRects[2];
		num = step + 1;
		step = num;
		yield return num;
		Game1.onScreenMenus.Clear();
		Game1.onScreenMenus.Add(Game1.dayTimeMoneyBox = new DayTimeMoneyBox());
		Game1.onScreenMenus.Add(new Toolbar());
		Game1.onScreenMenus.Add(Game1.buffsDisplay = new BuffsDisplay());
		num = step + 1;
		step = num;
		yield return num;
		for (int j = 0; j < 70; j++)
		{
			Game1.rainDrops[j] = new RainDrop(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height), Game1.random.Next(4), Game1.random.Next(70));
		}
		num = step + 1;
		step = num;
		yield return num;
		Game1.dialogueWidth = Math.Min(1024, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 256);
		Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
		Game1.dialogueFont.LineSpacing = 42;
		num = step + 1;
		step = num;
		yield return num;
		Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
		Game1.smallFont.LineSpacing = 28;
		num = step + 1;
		step = num;
		yield return num;
		Game1.tinyFont = Game1.content.Load<SpriteFont>("Fonts\\tinyFont");
		num = step + 1;
		step = num;
		yield return num;
		Game1._shortDayDisplayName[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
		num = step + 1;
		step = num;
		yield return num;
		Game1._shortDayDisplayName[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
		num = step + 1;
		step = num;
		yield return num;
		Game1._shortDayDisplayName[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
		num = step + 1;
		step = num;
		yield return num;
		Game1._shortDayDisplayName[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
		num = step + 1;
		step = num;
		yield return num;
		Game1._shortDayDisplayName[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
		num = step + 1;
		step = num;
		yield return num;
		Game1._shortDayDisplayName[5] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
		num = step + 1;
		step = num;
		yield return num;
		Game1._shortDayDisplayName[6] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
		yield return step + 1;
	}

	public static void resetPlayer()
	{
		List<Item> farmersInitialTools = Farmer.initialTools();
		Game1.player = new Farmer(new FarmerSprite(null), new Vector2(192f, 192f), 1, "", farmersInitialTools, isMale: true);
	}

	public static void resetVariables()
	{
		Game1.xLocationAfterWarp = 0;
		Game1.yLocationAfterWarp = 0;
		Game1.gameTimeInterval = 0;
		Game1.currentQuestionChoice = 0;
		Game1.currentDialogueCharacterIndex = 0;
		Game1.dialogueTypingInterval = 0;
		Game1.dayOfMonth = 0;
		Game1.year = 1;
		Game1.timeOfDay = 600;
		Game1.timeOfDayAfterFade = -1;
		Game1.facingDirectionAfterWarp = 0;
		Game1.dialogueWidth = 0;
		Game1.facingDirectionAfterWarp = 0;
		Game1.mouseClickPolling = 0;
		Game1.weatherIcon = 0;
		Game1.hitShakeTimer = 0;
		Game1.staminaShakeTimer = 0;
		Game1.pauseThenDoFunctionTimer = 0;
		Game1.weatherForTomorrow = "Sun";
	}

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <returns>Returns whether the cue exists and was started successfully.</returns>
	/// <remarks>To play audio in a specific location, see <see cref="M:StardewValley.GameLocation.playSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> or <see cref="M:StardewValley.GameLocation.localSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> instead.</remarks>
	public static bool playSound(string cueName, int? pitch = null)
	{
		ICue cue;
		return Game1.sounds.PlayLocal(cueName, null, null, pitch, SoundContext.Default, out cue);
	}

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="cue">The cue instance that was started, or a no-op cue if it failed.</param>
	/// <returns>Returns whether the cue exists and was started successfully.</returns>
	/// <remarks>To play audio in a specific location, see <see cref="M:StardewValley.GameLocation.playSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> or <see cref="M:StardewValley.GameLocation.localSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> instead.</remarks>
	public static bool playSound(string cueName, out ICue cue)
	{
		return Game1.sounds.PlayLocal(cueName, null, null, null, SoundContext.Default, out cue);
	}

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply.</param>
	/// <param name="cue">The cue instance that was started, or a no-op cue if it failed.</param>
	/// <returns>Returns whether the cue exists and was started successfully.</returns>
	/// <remarks>To play audio in a specific location, see <see cref="M:StardewValley.GameLocation.playSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> or <see cref="M:StardewValley.GameLocation.localSound(System.String,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> instead.</remarks>
	public static bool playSound(string cueName, int pitch, out ICue cue)
	{
		return Game1.sounds.PlayLocal(cueName, null, null, pitch, SoundContext.Default, out cue);
	}

	public static void setRichPresence(string friendlyName, object argument = null)
	{
		if (friendlyName == null)
		{
			return;
		}
		switch (friendlyName.Length)
		{
		case 8:
			switch (friendlyName[0])
			{
			case 'l':
				if (friendlyName == "location")
				{
					Game1.debugPresenceString = $"At {argument}";
				}
				break;
			case 'f':
				if (friendlyName == "festival")
				{
					Game1.debugPresenceString = $"At {argument}";
				}
				break;
			case 'm':
				if (friendlyName == "minigame")
				{
					Game1.debugPresenceString = $"Playing {argument}";
				}
				break;
			case 'e':
				if (friendlyName == "earnings")
				{
					Game1.debugPresenceString = $"Made {argument}g last night";
				}
				break;
			}
			break;
		case 7:
			switch (friendlyName[0])
			{
			case 'f':
				if (friendlyName == "fishing")
				{
					Game1.debugPresenceString = $"Fishing at {argument}";
				}
				break;
			case 'w':
				if (friendlyName == "wedding")
				{
					Game1.debugPresenceString = $"Getting married to {argument}";
				}
				break;
			}
			break;
		case 5:
			if (friendlyName == "menus")
			{
				Game1.debugPresenceString = "In menus";
			}
			break;
		case 9:
			if (friendlyName == "giantcrop")
			{
				Game1.debugPresenceString = $"Just harvested a Giant {argument}";
			}
			break;
		case 6:
			break;
		}
	}

	public static void GenerateBundles(BundleType bundle_type, bool use_seed = true)
	{
		if (bundle_type == BundleType.Remixed)
		{
			Random r = (use_seed ? Utility.CreateRandom((double)Game1.uniqueIDForThisGame * 9.0) : new Random());
			Dictionary<string, string> bundle_data = new BundleGenerator().Generate(DataLoader.RandomBundles(Game1.content), r);
			Game1.netWorldState.Value.SetBundleData(bundle_data);
		}
		else
		{
			Game1.netWorldState.Value.SetBundleData(DataLoader.Bundles(Game1.content));
		}
	}

	public void SetNewGameOption<T>(string key, T val)
	{
		this.newGameSetupOptions[key] = val;
	}

	public T GetNewGameOption<T>(string key)
	{
		if (!this.newGameSetupOptions.TryGetValue(key, out var value))
		{
			return default(T);
		}
		return (T)value;
	}

	public virtual void loadForNewGame(bool loadedGame = false)
	{
		if (Game1.startingGameSeed.HasValue)
		{
			Game1.uniqueIDForThisGame = Game1.startingGameSeed.Value;
		}
		Game1.specialCurrencyDisplay = new SpecialCurrencyDisplay();
		Game1.flushLocationLookup();
		Game1.locations.Clear();
		Game1.mailbox.Clear();
		Game1.currentLightSources.Clear();
		Game1.questionChoices.Clear();
		Game1.hudMessages.Clear();
		Game1.weddingToday = false;
		Game1.timeOfDay = 600;
		Game1.season = Season.Spring;
		if (!loadedGame)
		{
			Game1.year = 1;
		}
		Game1.dayOfMonth = 0;
		Game1.isQuestion = false;
		Game1.nonWarpFade = false;
		Game1.newDay = false;
		Game1.eventUp = false;
		Game1.viewportFreeze = false;
		Game1.eventOver = false;
		Game1.screenGlow = false;
		Game1.screenGlowHold = false;
		Game1.screenGlowUp = false;
		Game1.isRaining = false;
		Game1.wasGreenRain = false;
		Game1.killScreen = false;
		Game1.messagePause = false;
		Game1.isDebrisWeather = false;
		Game1.weddingToday = false;
		Game1.exitToTitle = false;
		Game1.dialogueUp = false;
		Game1.postExitToTitleCallback = null;
		Game1.displayHUD = true;
		Game1.messageAfterPause = "";
		Game1.samBandName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
		Game1.background = null;
		Game1.currentCursorTile = Vector2.Zero;
		if (!loadedGame)
		{
			Game1.lastAppliedSaveFix = SaveMigrator.LatestSaveFix;
		}
		Game1.resetVariables();
		Game1.player.team.sharedDailyLuck.Value = 0.001;
		if (!loadedGame)
		{
			Game1.options = new Options();
			Game1.options.LoadDefaultOptions();
			Game1.initializeVolumeLevels();
		}
		Game1.game1.CheckGamepadMode();
		Game1.onScreenMenus.Add(Game1.chatBox = new ChatBox());
		Game1.outdoorLight = Color.White;
		Game1.ambientLight = Color.White;
		Game1.UpdateDishOfTheDay();
		Game1.locations.Clear();
		Farm farm = new Farm("Maps\\" + Farm.getMapNameFromTypeInt(Game1.whichFarm), "Farm");
		Game1.locations.Add(farm);
		Game1.AddLocations();
		foreach (GameLocation location in Game1.locations)
		{
			location.AddDefaultBuildings();
		}
		Game1.forceSnapOnNextViewportUpdate = true;
		farm.onNewGame();
		if (!loadedGame)
		{
			foreach (GameLocation location2 in Game1.locations)
			{
				if (location2 is IslandLocation islandLocation)
				{
					islandLocation.AddAdditionalWalnutBushes();
				}
			}
		}
		if (!loadedGame)
		{
			Game1.hooks.CreatedInitialLocations();
		}
		else
		{
			Game1.hooks.SaveAddedLocations();
		}
		if (!loadedGame)
		{
			Game1.AddNPCs();
		}
		WarpPathfindingCache.PopulateCache();
		if (!loadedGame)
		{
			Game1.GenerateBundles(Game1.bundleType);
			foreach (string value in Game1.netWorldState.Value.BundleData.Values)
			{
				string[] item_split = ArgUtility.SplitBySpace(value.Split('/')[2]);
				if (!Game1.game1.GetNewGameOption<bool>("YearOneCompletable"))
				{
					continue;
				}
				for (int i = 0; i < item_split.Length; i += 3)
				{
					if (item_split[i] == "266")
					{
						int visits = (16 - 2) * 2;
						visits += 3;
						Random r = Utility.CreateRandom((double)Game1.uniqueIDForThisGame * 12.0);
						Game1.netWorldState.Value.VisitsUntilY1Guarantee = r.Next(2, visits);
					}
				}
			}
			Game1.netWorldState.Value.ShuffleMineChests = Game1.game1.GetNewGameOption<MineChestType>("MineChests");
			if (Game1.game1.newGameSetupOptions.ContainsKey("SpawnMonstersAtNight"))
			{
				Game1.spawnMonstersAtNight = Game1.game1.GetNewGameOption<bool>("SpawnMonstersAtNight");
			}
		}
		Game1.player.ConvertClothingOverrideToClothesItems();
		Game1.player.addQuest("9");
		Game1.RefreshQuestOfTheDay();
		Game1.player.currentLocation = Game1.RequireLocation("FarmHouse");
		Game1.player.gameVersion = Game1.version;
		Game1.hudMessages.Clear();
		Game1.hasLoadedGame = true;
		Game1.setGraphicsForSeason(onLoad: true);
		if (!loadedGame)
		{
			Game1._setSaveName = false;
		}
		Game1.game1.newGameSetupOptions.Clear();
		Game1.updateCellarAssignments();
		if (!loadedGame && Game1.netWorldState != null && Game1.netWorldState.Value != null)
		{
			Game1.netWorldState.Value.RegisterSpecialCurrencies();
		}
	}

	public bool IsLocalCoopJoinable()
	{
		if (GameRunner.instance.gameInstances.Count >= GameRunner.instance.GetMaxSimultaneousPlayers())
		{
			return false;
		}
		if (Game1.IsClient)
		{
			return false;
		}
		return true;
	}

	public static void StartLocalMultiplayerIfNecessary()
	{
		if (Game1.multiplayerMode == 0)
		{
			Game1.log.Verbose("Starting multiplayer server for local multiplayer...");
			Game1.multiplayerMode = 2;
			if (Game1.server == null)
			{
				Game1.multiplayer.StartLocalMultiplayerServer();
			}
		}
	}

	public static void EndLocalMultiplayer()
	{
	}

	public static void UpdatePassiveFestivalStates()
	{
		Game1.netWorldState.Value.ActivePassiveFestivals.Clear();
		foreach (KeyValuePair<string, PassiveFestivalData> pair in DataLoader.PassiveFestivals(Game1.content))
		{
			string id = pair.Key;
			PassiveFestivalData festival = pair.Value;
			if (Game1.dayOfMonth >= festival.StartDay && Game1.dayOfMonth <= festival.EndDay && Game1.season == festival.Season && GameStateQuery.CheckConditions(festival.Condition))
			{
				Game1.netWorldState.Value.ActivePassiveFestivals.Add(id);
			}
		}
	}

	public void Instance_UnloadContent()
	{
		this.UnloadContent();
	}

	/// <summary>
	/// UnloadContent will be called once per game and is the place to unload
	/// all content.
	/// </summary>
	protected override void UnloadContent()
	{
		base.UnloadContent();
		Game1.spriteBatch.Dispose();
		Game1.content.Unload();
		this.xTileContent.Unload();
		Game1.server?.stopServer();
	}

	public static void showRedMessage(string message, bool playSound = true)
	{
		Game1.addHUDMessage(new HUDMessage(message, 3));
		if (!message.Contains("Inventory") && playSound)
		{
			Game1.playSound("cancel");
		}
		else if (Game1.player.mailReceived.Add("BackpackTip"))
		{
			Game1.addMailForTomorrow("pierreBackpack");
		}
	}

	public static void showRedMessageUsingLoadString(string loadString, bool playSound = true)
	{
		Game1.showRedMessage(Game1.content.LoadString(loadString), playSound);
	}

	public static bool didPlayerJustLeftClick(bool ignoreNonMouseHeldInput = false)
	{
		if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton != ButtonState.Pressed)
		{
			return true;
		}
		if (Game1.input.GetGamePadState().Buttons.X == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !Game1.oldPadState.IsButtonDown(Buttons.X)))
		{
			return true;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.useToolButton) && (!ignoreNonMouseHeldInput || Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.useToolButton)))
		{
			return true;
		}
		return false;
	}

	public static bool didPlayerJustRightClick(bool ignoreNonMouseHeldInput = false)
	{
		if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton != ButtonState.Pressed)
		{
			return true;
		}
		if (Game1.input.GetGamePadState().Buttons.A == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !Game1.oldPadState.IsButtonDown(Buttons.A)))
		{
			return true;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && (!ignoreNonMouseHeldInput || !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton)))
		{
			return true;
		}
		return false;
	}

	public static bool didPlayerJustClickAtAll(bool ignoreNonMouseHeldInput = false)
	{
		if (!Game1.didPlayerJustLeftClick(ignoreNonMouseHeldInput))
		{
			return Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput);
		}
		return true;
	}

	public static void showGlobalMessage(string message)
	{
		Game1.addHUDMessage(HUDMessage.ForCornerTextbox(message));
	}

	public static void globalFadeToBlack(afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
	{
		Game1.screenFade.GlobalFadeToBlack(afterFade, fadeSpeed);
	}

	public static void globalFadeToClear(afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
	{
		Game1.screenFade.GlobalFadeToClear(afterFade, fadeSpeed);
	}

	public void CheckGamepadMode()
	{
		bool old_gamepad_active_state = Game1.options.gamepadControls;
		switch (Game1.options.gamepadMode)
		{
		case Options.GamepadModes.ForceOn:
			Game1.options.gamepadControls = true;
			return;
		case Options.GamepadModes.ForceOff:
			Game1.options.gamepadControls = false;
			return;
		}
		MouseState mouseState = Game1.input.GetMouseState();
		KeyboardState keyState = Game1.GetKeyboardState();
		GamePadState padState = Game1.input.GetGamePadState();
		bool non_gamepad_control_was_used = false;
		if ((mouseState.LeftButton == ButtonState.Pressed || mouseState.MiddleButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed || mouseState.ScrollWheelValue != this._oldScrollWheelValue || ((mouseState.X != this._oldMousePosition.X || mouseState.Y != this._oldMousePosition.Y) && Game1.lastCursorMotionWasMouse) || keyState.GetPressedKeys().Length != 0) && (keyState.GetPressedKeys().Length != 1 || keyState.GetPressedKeys()[0] != Keys.Pause))
		{
			non_gamepad_control_was_used = true;
			if (Program.sdk is SteamHelper steamHelper && steamHelper.IsRunningOnSteamDeck())
			{
				non_gamepad_control_was_used = false;
			}
		}
		this._oldScrollWheelValue = mouseState.ScrollWheelValue;
		this._oldMousePosition.X = mouseState.X;
		this._oldMousePosition.Y = mouseState.Y;
		bool gamepad_control_was_used = Game1.isAnyGamePadButtonBeingPressed() || Game1.isDPadPressed() || Game1.isGamePadThumbstickInMotion() || padState.Triggers.Left != 0f || padState.Triggers.Right != 0f;
		if (this._oldGamepadConnectedState != padState.IsConnected)
		{
			this._oldGamepadConnectedState = padState.IsConnected;
			if (this._oldGamepadConnectedState)
			{
				Game1.options.gamepadControls = true;
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574"));
			}
			else
			{
				Game1.options.gamepadControls = false;
				if (this.instancePlayerOneIndex != (PlayerIndex)(-1))
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
					if (Game1.CanShowPauseMenu() && Game1.activeClickableMenu == null)
					{
						Game1.activeClickableMenu = new GameMenu();
					}
				}
			}
		}
		if (non_gamepad_control_was_used && Game1.options.gamepadControls)
		{
			Game1.options.gamepadControls = false;
		}
		if (!Game1.options.gamepadControls && gamepad_control_was_used)
		{
			Game1.options.gamepadControls = true;
		}
		if (old_gamepad_active_state == Game1.options.gamepadControls || !Game1.options.gamepadControls)
		{
			return;
		}
		Game1.lastMousePositionBeforeFade = new Point(this.localMultiplayerWindow.Width / 2, this.localMultiplayerWindow.Height / 2);
		if (Game1.activeClickableMenu != null)
		{
			Game1.activeClickableMenu.setUpForGamePadMode();
			if (Game1.options.SnappyMenus)
			{
				Game1.activeClickableMenu.populateClickableComponentList();
				Game1.activeClickableMenu.snapToDefaultClickableComponent();
			}
		}
		Game1.timerUntilMouseFade = 0;
	}

	public void Instance_Update(GameTime gameTime)
	{
		this.Update(gameTime);
	}

	protected override void Update(GameTime gameTime)
	{
		GameTime time = gameTime;
		DebugTools.BeforeGameUpdate(this, ref time);
		Game1.input.UpdateStates();
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
		{
			Game1.rightStickHoldTime += gameTime.ElapsedGameTime.Milliseconds;
		}
		GameMenu.bundleItemHovered = false;
		this._update(time);
		if (Game1.IsMultiplayer && Game1.player != null)
		{
			Game1.player.requestingTimePause.Value = !Game1.shouldTimePass(LocalMultiplayer.IsLocalMultiplayer(is_local_only: true));
			if (Game1.IsMasterGame)
			{
				bool should_time_pause = false;
				if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
				{
					should_time_pause = true;
					foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
					{
						if (!onlineFarmer.requestingTimePause.Value)
						{
							should_time_pause = false;
							break;
						}
					}
				}
				Game1.netWorldState.Value.IsTimePaused = should_time_pause;
			}
		}
		Rumble.update(gameTime.ElapsedGameTime.Milliseconds);
		if (Game1.options.gamepadControls && Game1.thumbstickMotionMargin > 0)
		{
			Game1.thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
		}
		if (!Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
		{
			Game1.rightStickHoldTime = 0;
		}
		base.Update(gameTime);
	}

	public void Instance_OnActivated(object sender, EventArgs args)
	{
		this.OnActivated(sender, args);
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		base.OnActivated(sender, args);
		Game1._activatedTick = Game1.ticks + 1;
		Game1.input.IgnoreKeys(Game1.GetKeyboardState().GetPressedKeys());
	}

	public bool HasKeyboardFocus()
	{
		if (Game1.keyboardFocusInstance == null)
		{
			return base.IsMainInstance;
		}
		return Game1.keyboardFocusInstance == this;
	}

	/// <summary>
	/// Allows the game to run logic such as updating the world,
	/// checking for collisions, gathering input, and playing audio.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	private void _update(GameTime gameTime)
	{
		if (Game1.graphics.GraphicsDevice == null)
		{
			return;
		}
		bool zoom_dirty = false;
		Game1.gameModeTicks++;
		if (Game1.options != null && !this.takingMapScreenshot)
		{
			if (Game1.options.baseUIScale != Game1.options.desiredUIScale)
			{
				if (Game1.options.desiredUIScale < 0f)
				{
					Game1.options.desiredUIScale = Game1.options.desiredBaseZoomLevel;
				}
				Game1.options.baseUIScale = Game1.options.desiredUIScale;
				zoom_dirty = true;
			}
			if (Game1.options.desiredBaseZoomLevel != Game1.options.baseZoomLevel)
			{
				Game1.options.baseZoomLevel = Game1.options.desiredBaseZoomLevel;
				Game1.forceSnapOnNextViewportUpdate = true;
				zoom_dirty = true;
			}
		}
		if (zoom_dirty)
		{
			this.refreshWindowSettings();
		}
		if (!this.ShouldLoadIncrementally)
		{
			this.CheckGamepadMode();
		}
		FarmAnimal.NumPathfindingThisTick = 0;
		Game1.options.reApplySetOptions();
		if (Game1.toggleFullScreen)
		{
			Game1.toggleFullscreen();
			Game1.toggleFullScreen = false;
		}
		Game1.input.Update();
		if (Game1.frameByFrame)
		{
			if (Game1.GetKeyboardState().IsKeyDown(Keys.Escape) && Game1.oldKBState.IsKeyUp(Keys.Escape))
			{
				Game1.frameByFrame = false;
			}
			if (!Game1.GetKeyboardState().IsKeyDown(Keys.G) || !Game1.oldKBState.IsKeyUp(Keys.G))
			{
				Game1.oldKBState = Game1.GetKeyboardState();
				return;
			}
		}
		if (Game1.client != null && Game1.client.timedOut)
		{
			Game1.multiplayer.clientRemotelyDisconnected(Game1.client.pendingDisconnect);
		}
		if (Game1._newDayTask != null)
		{
			if (Game1._newDayTask.Status == TaskStatus.Created)
			{
				Game1.hooks.StartTask(Game1._newDayTask, "NewDay");
			}
			if (Game1._newDayTask.Status >= TaskStatus.RanToCompletion)
			{
				if (Game1._newDayTask.IsFaulted)
				{
					Exception e = Game1._newDayTask.Exception.GetBaseException();
					if (!Game1.IsMasterGame)
					{
						if (e is AbortNetSynchronizerException)
						{
							Game1.log.Verbose("_newDayTask failed: client lost connection to the server");
						}
						else
						{
							Game1.log.Error("Client _newDayTask failed with an exception:", e);
						}
						Game1.multiplayer.clientRemotelyDisconnected(Multiplayer.DisconnectType.ClientTimeout);
						Game1._newDayTask = null;
						Utility.CollectGarbage();
						return;
					}
					Game1.log.Error("_newDayTask failed with an exception:", e);
					throw new Exception($"Error on new day: \n---------------\n{e}\n---------------\n");
				}
				Game1._newDayTask = null;
				Utility.CollectGarbage();
			}
			Game1.UpdateChatBox();
			return;
		}
		if (this.isLocalMultiplayerNewDayActive)
		{
			Game1.UpdateChatBox();
			return;
		}
		if (this.IsSaving)
		{
			Game1.PushUIMode();
			Game1.activeClickableMenu?.update(gameTime);
			if (Game1.overlayMenu != null)
			{
				Game1.overlayMenu.update(gameTime);
				if (Game1.overlayMenu == null)
				{
					Game1.PopUIMode();
					return;
				}
			}
			Game1.PopUIMode();
			Game1.UpdateChatBox();
			return;
		}
		if (Game1.exitToTitle)
		{
			Game1.exitToTitle = false;
			this.CleanupReturningToTitle();
			Utility.CollectGarbage();
			Game1.postExitToTitleCallback?.Invoke();
		}
		Game1.SetFreeCursorElapsed((float)gameTime.ElapsedGameTime.TotalSeconds);
		Program.sdk.Update();
		if (Game1.game1.IsMainInstance)
		{
			Game1.keyboardFocusInstance = Game1.game1;
			foreach (Game1 instance in GameRunner.instance.gameInstances)
			{
				if (instance.instanceKeyboardDispatcher.Subscriber != null && instance.instanceTextEntry != null)
				{
					Game1.keyboardFocusInstance = instance;
					break;
				}
			}
		}
		if (base.IsMainInstance)
		{
			int current_display_index = base.Window.GetDisplayIndex();
			if (this._lastUsedDisplay != -1 && this._lastUsedDisplay != current_display_index)
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, applyLanguage: false);
				startupPreferences.displayIndex = current_display_index;
				startupPreferences.savePreferences(async: false);
			}
			this._lastUsedDisplay = current_display_index;
		}
		if (this.HasKeyboardFocus())
		{
			Game1.keyboardDispatcher.Poll();
		}
		else
		{
			Game1.keyboardDispatcher.Discard();
		}
		if (Game1.gameMode == 6)
		{
			Game1.multiplayer.UpdateLoading();
		}
		if (Game1.gameMode == 3)
		{
			Game1.multiplayer.UpdateEarly();
			Game1.dedicatedServer.Tick();
			if (Game1.player?.team != null)
			{
				Game1.player.team.Update();
			}
		}
		if ((Game1.paused || (!this.IsActiveNoOverlay && Program.releaseBuild)) && (Game1.options == null || Game1.options.pauseWhenOutOfFocus || Game1.paused) && Game1.multiplayerMode == 0)
		{
			Game1.UpdateChatBox();
			return;
		}
		if (Game1.quit)
		{
			base.Exit();
		}
		Game1.currentGameTime = gameTime;
		if (Game1.gameMode != 11 && !this.ShouldLoadIncrementally)
		{
			Game1.ticks++;
			if (this.IsActiveNoOverlay)
			{
				this.checkForEscapeKeys();
			}
			Game1.updateMusic();
			Game1.updateRaindropPosition();
			if (Game1.globalFade)
			{
				Game1.screenFade.UpdateGlobalFade();
			}
			else if (Game1.pauseThenDoFunctionTimer > 0)
			{
				Game1.freezeControls = true;
				Game1.pauseThenDoFunctionTimer -= gameTime.ElapsedGameTime.Milliseconds;
				if (Game1.pauseThenDoFunctionTimer <= 0)
				{
					Game1.freezeControls = false;
					Game1.afterPause?.Invoke();
				}
			}
			if (Game1.options.gamepadControls && (Game1.activeClickableMenu?.shouldClampGamePadCursor() ?? false))
			{
				Point pos = Game1.getMousePositionRaw();
				Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, this.localMultiplayerWindow.Width, this.localMultiplayerWindow.Height);
				if (pos.X < rect.X)
				{
					pos.X = rect.X;
				}
				else if (pos.X > rect.Right)
				{
					pos.X = rect.Right;
				}
				if (pos.Y < rect.Y)
				{
					pos.Y = rect.Y;
				}
				else if (pos.Y > rect.Bottom)
				{
					pos.Y = rect.Bottom;
				}
				Game1.setMousePositionRaw(pos.X, pos.Y);
			}
			if (Game1.gameMode == 3 || Game1.gameMode == 2)
			{
				if (!Game1.warpingForForcedRemoteEvent && !Game1.eventUp && !Game1.dialogueUp && Game1.remoteEventQueue.Count > 0 && Game1.player != null && Game1.player.isCustomized.Value && (!Game1.fadeIn || !(Game1.fadeToBlackAlpha > 0f)))
				{
					if (Game1.activeClickableMenu != null)
					{
						Game1.activeClickableMenu.emergencyShutDown();
						Game1.exitActiveMenu();
					}
					else if (Game1.currentMinigame != null && Game1.currentMinigame.forceQuit())
					{
						Game1.currentMinigame = null;
					}
					if (Game1.activeClickableMenu == null && Game1.currentMinigame == null && Game1.player.freezePause <= 0)
					{
						Action action = Game1.remoteEventQueue[0];
						Game1.remoteEventQueue.RemoveAt(0);
						action();
					}
				}
				Game1.player.millisecondsPlayed += (uint)gameTime.ElapsedGameTime.Milliseconds;
				bool doMainGameUpdates = true;
				if (Game1.currentMinigame != null && !Game1.HostPaused)
				{
					if (Game1.pauseTime > 0f)
					{
						Game1.updatePause(gameTime);
					}
					if (Game1.fadeToBlack)
					{
						Game1.screenFade.UpdateFadeAlpha(gameTime);
						if (Game1.fadeToBlackAlpha >= 1f)
						{
							Game1.fadeToBlack = false;
						}
					}
					else
					{
						if (Game1.thumbstickMotionMargin > 0)
						{
							Game1.thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
						}
						KeyboardState currentKBState = default(KeyboardState);
						MouseState currentMouseState = default(MouseState);
						GamePadState currentPadState = default(GamePadState);
						if (base.IsActive)
						{
							currentKBState = Game1.GetKeyboardState();
							currentMouseState = Game1.input.GetMouseState();
							currentPadState = Game1.input.GetGamePadState();
							if ((Game1.chatBox?.isActive() ?? false) || Game1.textEntry != null)
							{
								currentKBState = default(KeyboardState);
								currentPadState = default(GamePadState);
							}
							else
							{
								Keys[] pressedKeys = currentKBState.GetPressedKeys();
								foreach (Keys k in pressedKeys)
								{
									if (!Game1.oldKBState.IsKeyDown(k) && Game1.currentMinigame != null)
									{
										Game1.currentMinigame.receiveKeyPress(k);
									}
								}
								if (Game1.options.gamepadControls)
								{
									if (Game1.currentMinigame == null)
									{
										Game1.oldMouseState = currentMouseState;
										Game1.oldKBState = currentKBState;
										Game1.oldPadState = currentPadState;
										Game1.UpdateChatBox();
										return;
									}
									ButtonCollection.ButtonEnumerator enumerator2 = Utility.getPressedButtons(currentPadState, Game1.oldPadState).GetEnumerator();
									while (enumerator2.MoveNext())
									{
										Buttons b = enumerator2.Current;
										Game1.currentMinigame?.receiveKeyPress(Utility.mapGamePadButtonToKey(b));
									}
									if (Game1.currentMinigame == null)
									{
										Game1.oldMouseState = currentMouseState;
										Game1.oldKBState = currentKBState;
										Game1.oldPadState = currentPadState;
										Game1.UpdateChatBox();
										return;
									}
									if (currentPadState.ThumbSticks.Right.Y < -0.2f && Game1.oldPadState.ThumbSticks.Right.Y >= -0.2f)
									{
										Game1.currentMinigame.receiveKeyPress(Keys.Down);
									}
									if (currentPadState.ThumbSticks.Right.Y > 0.2f && Game1.oldPadState.ThumbSticks.Right.Y <= 0.2f)
									{
										Game1.currentMinigame.receiveKeyPress(Keys.Up);
									}
									if (currentPadState.ThumbSticks.Right.X < -0.2f && Game1.oldPadState.ThumbSticks.Right.X >= -0.2f)
									{
										Game1.currentMinigame.receiveKeyPress(Keys.Left);
									}
									if (currentPadState.ThumbSticks.Right.X > 0.2f && Game1.oldPadState.ThumbSticks.Right.X <= 0.2f)
									{
										Game1.currentMinigame.receiveKeyPress(Keys.Right);
									}
									if (Game1.oldPadState.ThumbSticks.Right.Y < -0.2f && currentPadState.ThumbSticks.Right.Y >= -0.2f)
									{
										Game1.currentMinigame.receiveKeyRelease(Keys.Down);
									}
									if (Game1.oldPadState.ThumbSticks.Right.Y > 0.2f && currentPadState.ThumbSticks.Right.Y <= 0.2f)
									{
										Game1.currentMinigame.receiveKeyRelease(Keys.Up);
									}
									if (Game1.oldPadState.ThumbSticks.Right.X < -0.2f && currentPadState.ThumbSticks.Right.X >= -0.2f)
									{
										Game1.currentMinigame.receiveKeyRelease(Keys.Left);
									}
									if (Game1.oldPadState.ThumbSticks.Right.X > 0.2f && currentPadState.ThumbSticks.Right.X <= 0.2f)
									{
										Game1.currentMinigame.receiveKeyRelease(Keys.Right);
									}
									if (Game1.isGamePadThumbstickInMotion() && Game1.currentMinigame != null && !Game1.currentMinigame.overrideFreeMouseMovement())
									{
										Game1.setMousePosition(Game1.getMouseX() + (int)(currentPadState.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier), Game1.getMouseY() - (int)(currentPadState.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier));
									}
									else if (Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY())
									{
										Game1.lastCursorMotionWasMouse = true;
									}
								}
								pressedKeys = Game1.oldKBState.GetPressedKeys();
								foreach (Keys k2 in pressedKeys)
								{
									if (!currentKBState.IsKeyDown(k2) && Game1.currentMinigame != null)
									{
										Game1.currentMinigame.receiveKeyRelease(k2);
									}
								}
								if (Game1.options.gamepadControls)
								{
									if (Game1.currentMinigame == null)
									{
										Game1.oldMouseState = currentMouseState;
										Game1.oldKBState = currentKBState;
										Game1.oldPadState = currentPadState;
										Game1.UpdateChatBox();
										return;
									}
									if (currentPadState.IsConnected)
									{
										if (currentPadState.IsButtonDown(Buttons.X) && !Game1.oldPadState.IsButtonDown(Buttons.X))
										{
											Game1.currentMinigame.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
										}
										else if (currentPadState.IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))
										{
											Game1.currentMinigame.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
										}
										else if (!currentPadState.IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonDown(Buttons.X))
										{
											Game1.currentMinigame.releaseRightClick(Game1.getMouseX(), Game1.getMouseY());
										}
										else if (!currentPadState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
										{
											Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
										}
									}
									ButtonCollection.ButtonEnumerator enumerator2 = Utility.getPressedButtons(Game1.oldPadState, currentPadState).GetEnumerator();
									while (enumerator2.MoveNext())
									{
										Buttons b2 = enumerator2.Current;
										Game1.currentMinigame?.receiveKeyRelease(Utility.mapGamePadButtonToKey(b2));
									}
									if (currentPadState.IsConnected && currentPadState.IsButtonDown(Buttons.A) && Game1.currentMinigame != null)
									{
										Game1.currentMinigame.leftClickHeld(0, 0);
									}
								}
								if (Game1.currentMinigame == null)
								{
									Game1.oldMouseState = currentMouseState;
									Game1.oldKBState = currentKBState;
									Game1.oldPadState = currentPadState;
									Game1.UpdateChatBox();
									return;
								}
								if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton != ButtonState.Pressed)
								{
									Game1.currentMinigame.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
								}
								if (Game1.currentMinigame != null && currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton != ButtonState.Pressed)
								{
									Game1.currentMinigame.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
								}
								if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
								{
									Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
								}
								if (Game1.currentMinigame != null && currentMouseState.RightButton == ButtonState.Released && Game1.oldMouseState.RightButton == ButtonState.Pressed)
								{
									Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
								}
								if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
								{
									Game1.currentMinigame.leftClickHeld(Game1.getMouseX(), Game1.getMouseY());
								}
							}
						}
						if (Game1.currentMinigame != null && Game1.currentMinigame.tick(gameTime))
						{
							Game1.oldMouseState = currentMouseState;
							Game1.oldKBState = currentKBState;
							Game1.oldPadState = currentPadState;
							Game1.currentMinigame?.unload();
							Game1.currentMinigame = null;
							Game1.fadeIn = true;
							Game1.fadeToBlackAlpha = 1f;
							Game1.UpdateChatBox();
							return;
						}
						if (Game1.currentMinigame == null && Game1.IsMusicContextActive(MusicContext.MiniGame))
						{
							Game1.stopMusicTrack(MusicContext.MiniGame);
						}
						Game1.oldMouseState = currentMouseState;
						Game1.oldKBState = currentKBState;
						Game1.oldPadState = currentPadState;
					}
					doMainGameUpdates = Game1.IsMultiplayer || Game1.currentMinigame == null || Game1.currentMinigame.doMainGameUpdates();
				}
				else if (Game1.farmEvent != null && !Game1.HostPaused && Game1.farmEvent.tickUpdate(gameTime))
				{
					Game1.farmEvent.makeChangesToLocation();
					Game1.timeOfDay = 600;
					Game1.outdoorLight = Color.White;
					Game1.displayHUD = true;
					Game1.farmEvent = null;
					Game1.netWorldState.Value.WriteToGame1();
					Game1.currentLocation = Game1.player.currentLocation;
					LocationRequest obj = Game1.getLocationRequest(Game1.currentLocation.Name);
					obj.OnWarp += delegate
					{
						if (Game1.currentLocation is FarmHouse farmHouse)
						{
							Game1.player.Position = Utility.PointToVector2(farmHouse.GetPlayerBedSpot()) * 64f;
							BedFurniture.ShiftPositionForBed(Game1.player);
						}
						else
						{
							BedFurniture.ApplyWakeUpPosition(Game1.player);
						}
						if (Game1.player.IsSitting())
						{
							Game1.player.StopSitting(animate: false);
						}
						Game1.changeMusicTrack("none", track_interruptable: true);
						Game1.player.forceCanMove();
						Game1.freezeControls = false;
						Game1.displayFarmer = true;
						Game1.viewportFreeze = false;
						Game1.fadeToBlackAlpha = 0f;
						Game1.fadeToBlack = false;
						Game1.globalFadeToClear();
						Game1.RemoveDeliveredMailForTomorrow();
						Game1.handlePostFarmEventActions();
						Game1.showEndOfNightStuff();
					};
					Game1.warpFarmer(obj, 5, 9, Game1.player.FacingDirection);
					Game1.fadeToBlackAlpha = 1.1f;
					Game1.fadeToBlack = true;
					Game1.nonWarpFade = false;
					Game1.UpdateOther(gameTime);
				}
				if (doMainGameUpdates)
				{
					if (Game1.endOfNightMenus.Count > 0 && Game1.activeClickableMenu == null)
					{
						Game1.activeClickableMenu = Game1.endOfNightMenus.Pop();
						if (Game1.activeClickableMenu != null && Game1.options.SnappyMenus)
						{
							Game1.activeClickableMenu.snapToDefaultClickableComponent();
						}
					}
					Game1.specialCurrencyDisplay?.Update(gameTime);
					if (Game1.currentLocation != null && Game1.currentMinigame == null)
					{
						if (Game1.emoteMenu != null)
						{
							Game1.emoteMenu.update(gameTime);
							if (Game1.emoteMenu != null)
							{
								Game1.PushUIMode();
								Game1.emoteMenu.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
								KeyboardState currentState = Game1.GetKeyboardState();
								if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
								{
									Game1.emoteMenu.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
								}
								else if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released)
								{
									Game1.emoteMenu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
								}
								else if (Game1.isOneOfTheseKeysDown(currentState, Game1.options.menuButton) || (Game1.isOneOfTheseKeysDown(currentState, Game1.options.emoteButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.emoteButton)))
								{
									Game1.emoteMenu.exitThisMenu(playSound: false);
								}
								Game1.PopUIMode();
								Game1.oldKBState = currentState;
								Game1.oldMouseState = Game1.input.GetMouseState();
							}
						}
						else if (Game1.textEntry != null)
						{
							Game1.PushUIMode();
							Game1.updateTextEntry(gameTime);
							Game1.PopUIMode();
						}
						else if (Game1.activeClickableMenu != null)
						{
							Game1.PushUIMode();
							Game1.updateActiveMenu(gameTime);
							Game1.PopUIMode();
						}
						else
						{
							if (Game1.pauseTime > 0f)
							{
								Game1.updatePause(gameTime);
							}
							if (!Game1.globalFade && !Game1.freezeControls && Game1.activeClickableMenu == null && (this.IsActiveNoOverlay || Game1.inputSimulator != null))
							{
								this.UpdateControlInput(gameTime);
							}
						}
					}
					if (Game1.showingEndOfNightStuff && Game1.endOfNightMenus.Count == 0 && Game1.activeClickableMenu == null)
					{
						Game1.newDaySync.destroy();
						Game1.player.team.endOfNightStatus.WithdrawState();
						Game1.showingEndOfNightStuff = false;
						Action afterAction = Game1._afterNewDayAction;
						if (afterAction != null)
						{
							Game1._afterNewDayAction = null;
							afterAction();
						}
						Game1.player.ReequipEnchantments();
						Game1.globalFadeToClear(doMorningStuff);
					}
					if (Game1.currentLocation != null)
					{
						if (!Game1.HostPaused && !Game1.showingEndOfNightStuff)
						{
							if (Game1.IsMultiplayer || (Game1.activeClickableMenu == null && Game1.currentMinigame == null) || Game1.player.viewingLocation.Value != null)
							{
								Game1.UpdateGameClock(gameTime);
							}
							this.UpdateCharacters(gameTime);
							this.UpdateLocations(gameTime);
							if (Game1.currentMinigame == null)
							{
								Game1.UpdateViewPort(overrideFreeze: false, this.getViewportCenter());
							}
							else
							{
								Game1.previousViewportPosition.X = Game1.viewport.X;
								Game1.previousViewportPosition.Y = Game1.viewport.Y;
							}
							Game1.UpdateOther(gameTime);
						}
						if (Game1.messagePause)
						{
							KeyboardState tmp = Game1.GetKeyboardState();
							MouseState tmp2 = Game1.input.GetMouseState();
							GamePadState tmp3 = Game1.input.GetGamePadState();
							if (Game1.isOneOfTheseKeysDown(tmp, Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton))
							{
								Game1.pressActionButton(tmp, tmp2, tmp3);
							}
							Game1.oldKBState = tmp;
							Game1.oldPadState = tmp3;
						}
					}
				}
				else if (Game1.textEntry != null)
				{
					Game1.PushUIMode();
					Game1.updateTextEntry(gameTime);
					Game1.PopUIMode();
				}
			}
			else
			{
				this.UpdateTitleScreen(gameTime);
				if (Game1.textEntry != null)
				{
					Game1.PushUIMode();
					Game1.updateTextEntry(gameTime);
					Game1.PopUIMode();
				}
				else if (Game1.activeClickableMenu != null)
				{
					Game1.PushUIMode();
					Game1.updateActiveMenu(gameTime);
					Game1.PopUIMode();
				}
				if (Game1.gameMode == 10)
				{
					Game1.UpdateOther(gameTime);
				}
			}
			Game1.audioEngine?.Update();
			Game1.UpdateChatBox();
			if (Game1.gameMode != 6)
			{
				Game1.multiplayer.UpdateLate();
			}
		}
		else if (this.ShouldLoadIncrementally)
		{
			Stopwatch loadTimer = Stopwatch.StartNew();
			do
			{
				if (!Game1.LoadContentEnumerator.MoveNext())
				{
					Game1.FinishedFirstLoadContent = true;
					break;
				}
			}
			while (loadTimer.Elapsed.TotalMilliseconds < 25.0);
			if (Game1.FinishedFirstLoadContent && Game1.FinishedFirstInitSounds && Game1.FinishedFirstInitSerializers)
			{
				Game1.FinishedIncrementalLoad = true;
				this.AfterLoadContent();
			}
		}
		if (Game1.gameMode == 3 && Game1.gameModeTicks == 1)
		{
			Game1.OnDayStarted();
		}
	}

	/// <summary>Handle the new day starting after the player saves, loads, or connects.</summary>
	public static void OnDayStarted()
	{
		TriggerActionManager.Raise("DayStarted");
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.OnDayStarted();
			return true;
		});
		Utility.fixAllAnimals();
		foreach (NPC allCharacter in Utility.getAllCharacters())
		{
			allCharacter.OnDayStarted();
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			foreach (FarmAnimal value in location.animals.Values)
			{
				value.OnDayStarted();
			}
			return true;
		});
		Game1.player.currentLocation.resetForPlayerEntry();
		if (!Game1.hasStartedDay)
		{
			foreach (string buildingType in Game1.player.team.constructedBuildings)
			{
				Game1.player.NotifyQuests((Quest quest) => quest.OnBuildingExists(buildingType));
			}
			if (Stats.AllowRetroactiveAchievements)
			{
				foreach (int achievement in Game1.player.achievements)
				{
					Game1.getPlatformAchievement(achievement.ToString());
				}
			}
			Game1.hasStartedDay = true;
		}
		if (Game1.IsMasterGame)
		{
			Woods.ResetLostItemsShop();
		}
	}

	public static void PerformPassiveFestivalSetup()
	{
		foreach (string festival_id in Game1.netWorldState.Value.ActivePassiveFestivals)
		{
			if (Utility.TryGetPassiveFestivalData(festival_id, out var data) && data.DailySetupMethod != null)
			{
				if (StaticDelegateBuilder.TryCreateDelegate<FestivalDailySetupDelegate>(data.DailySetupMethod, out var method, out var error))
				{
					method();
					continue;
				}
				Game1.log.Warn($"Passive festival '{festival_id}' has invalid daily setup method '{data.DailySetupMethod}': {error}");
			}
		}
	}

	public static void showTextEntry(TextBox text_box)
	{
		Game1.timerUntilMouseFade = 0;
		Game1.PushUIMode();
		Game1.textEntry = new TextEntryMenu(text_box);
		Game1.PopUIMode();
	}

	public static void closeTextEntry()
	{
		if (Game1.textEntry != null)
		{
			Game1.textEntry = null;
		}
		if (Game1.activeClickableMenu != null && Game1.options.SnappyMenus)
		{
			if (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu != null)
			{
				TitleMenu.subMenu.snapCursorToCurrentSnappedComponent();
			}
			else
			{
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public static bool isDarkOut(GameLocation location)
	{
		return Game1.timeOfDay >= Game1.getTrulyDarkTime(location);
	}

	public static bool isTimeToTurnOffLighting(GameLocation location)
	{
		return Game1.timeOfDay >= Game1.getTrulyDarkTime(location) - 100;
	}

	public static bool isStartingToGetDarkOut(GameLocation location)
	{
		return Game1.timeOfDay >= Game1.getStartingToGetDarkTime(location);
	}

	public static int getStartingToGetDarkTime(GameLocation location)
	{
		if (location != null && location.InIslandContext())
		{
			return 1800;
		}
		return Game1.season switch
		{
			Season.Fall => 1700, 
			Season.Winter => 1500, 
			_ => 1800, 
		};
	}

	public static void updateCellarAssignments()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Game1.player.team.cellarAssignments[1] = Game1.MasterPlayer.UniqueMultiplayerID;
		for (int i = 2; i <= Game1.netWorldState.Value.HighestPlayerLimit; i++)
		{
			string cellar_name = "Cellar" + i;
			if (i == 1 || Game1.getLocationFromName(cellar_name) == null)
			{
				continue;
			}
			if (Game1.player.team.cellarAssignments.TryGetValue(i, out var assignedFarmerId))
			{
				if (Game1.GetPlayer(assignedFarmerId) != null)
				{
					continue;
				}
				Game1.player.team.cellarAssignments.Remove(i);
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (!Game1.player.team.cellarAssignments.Values.Contains(farmer.UniqueMultiplayerID))
				{
					Game1.player.team.cellarAssignments[i] = farmer.UniqueMultiplayerID;
					break;
				}
			}
		}
	}

	public static int getModeratelyDarkTime(GameLocation location)
	{
		return (Game1.getTrulyDarkTime(location) + Game1.getStartingToGetDarkTime(location)) / 2;
	}

	public static int getTrulyDarkTime(GameLocation location)
	{
		return Game1.getStartingToGetDarkTime(location) + 200;
	}

	public static void playMorningSong(bool ignoreDelay = false)
	{
		LocationContextData context;
		if (!Game1.eventUp && Game1.dayOfMonth > 0)
		{
			LocationData data = Game1.currentLocation.GetData();
			if (Game1.currentLocation.GetLocationSpecificMusic() != null && (data == null || !data.MusicIsTownTheme))
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
				GameLocation.HandleMusicChange(null, Game1.currentLocation);
				return;
			}
			if (Game1.IsRainingHere())
			{
				if (ignoreDelay)
				{
					PlayRain();
				}
				else
				{
					Game1.morningSongPlayAction = DelayedAction.functionAfterDelay(PlayRain, 500);
				}
				return;
			}
			context = Game1.currentLocation?.GetLocationContext();
			if (context?.DefaultMusic != null)
			{
				if (context.DefaultMusicCondition == null || GameStateQuery.CheckConditions(context.DefaultMusicCondition))
				{
					if (ignoreDelay)
					{
						PlayLocationSong();
					}
					else
					{
						Game1.morningSongPlayAction = DelayedAction.functionAfterDelay(PlayLocationSong, 500);
					}
				}
			}
			else if (ignoreDelay)
			{
				PlayDefault();
			}
			else
			{
				Game1.morningSongPlayAction = DelayedAction.functionAfterDelay(PlayDefault, 500);
			}
		}
		else if (Game1.getMusicTrackName() == "silence")
		{
			Game1.changeMusicTrack("none", track_interruptable: true);
		}
		static void PlayDefault()
		{
			Game1.changeMusicTrack(Game1.currentLocation.GetMorningSong(), track_interruptable: true);
			Game1.IsPlayingBackgroundMusic = true;
			Game1.IsPlayingMorningSong = true;
		}
		void PlayLocationSong()
		{
			if (Game1.currentLocation == null)
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			else
			{
				Game1.changeMusicTrack(context.DefaultMusic, track_interruptable: true);
				Game1.IsPlayingBackgroundMusic = true;
			}
		}
		static void PlayRain()
		{
			Game1.changeMusicTrack("rain", track_interruptable: true);
		}
	}

	public static void doMorningStuff()
	{
		Game1.playMorningSong();
		DelayedAction.functionAfterDelay(delegate
		{
			while (Game1.morningQueue.Count > 0)
			{
				Game1.morningQueue.Dequeue()();
			}
		}, 1000);
		if (Game1.player.hasPendingCompletedQuests)
		{
			Game1.dayTimeMoneyBox.PingQuestLog();
		}
	}

	/// <summary>Add an action that will be called one second after fully waking up in the morning. This won't be saved, so it should only be used for "fluff" functions like sending multiplayer chat messages, etc.</summary>
	/// <param name="action">The action to perform.</param>
	public static void addMorningFluffFunction(Action action)
	{
		Game1.morningQueue.Enqueue(action);
	}

	private Point getViewportCenter()
	{
		if (Game1.viewportTarget.X != -2.1474836E+09f)
		{
			if (!(Math.Abs((float)Game1.viewportCenter.X - Game1.viewportTarget.X) <= Game1.viewportSpeed) || !(Math.Abs((float)Game1.viewportCenter.Y - Game1.viewportTarget.Y) <= Game1.viewportSpeed))
			{
				Vector2 velocity = Utility.getVelocityTowardPoint(Game1.viewportCenter, Game1.viewportTarget, Game1.viewportSpeed);
				Game1.viewportCenter.X += (int)Math.Round(velocity.X);
				Game1.viewportCenter.Y += (int)Math.Round(velocity.Y);
			}
			else
			{
				if (Game1.viewportReachedTarget != null)
				{
					Game1.viewportReachedTarget();
					Game1.viewportReachedTarget = null;
				}
				Game1.viewportHold -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (Game1.viewportHold <= 0)
				{
					Game1.viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
					Game1.afterViewport?.Invoke();
				}
			}
		}
		else
		{
			Game1.viewportCenter = Game1.getPlayerOrEventFarmer().StandingPixel;
		}
		return Game1.viewportCenter;
	}

	public static void afterFadeReturnViewportToPlayer()
	{
		Game1.viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
		Game1.viewportHold = 0;
		Game1.viewportFreeze = false;
		Game1.viewportCenter = Game1.player.StandingPixel;
		Game1.globalFadeToClear();
	}

	public static bool isViewportOnCustomPath()
	{
		return Game1.viewportTarget.X != -2.1474836E+09f;
	}

	public static void moveViewportTo(Vector2 target, float speed, int holdTimer = 0, afterFadeFunction reachedTarget = null, afterFadeFunction endFunction = null)
	{
		Game1.viewportTarget = target;
		Game1.viewportSpeed = speed;
		Game1.viewportHold = holdTimer;
		Game1.afterViewport = endFunction;
		Game1.viewportReachedTarget = reachedTarget;
	}

	public static Farm getFarm()
	{
		return Game1.RequireLocation<Farm>("Farm");
	}

	public static void setMousePosition(int x, int y, bool ui_scale)
	{
		if (ui_scale)
		{
			Game1.setMousePositionRaw((int)((float)x * Game1.options.uiScale), (int)((float)y * Game1.options.uiScale));
		}
		else
		{
			Game1.setMousePositionRaw((int)((float)x * Game1.options.zoomLevel), (int)((float)y * Game1.options.zoomLevel));
		}
	}

	public static void setMousePosition(int x, int y)
	{
		Game1.setMousePosition(x, y, Game1.uiMode);
	}

	public static void setMousePosition(Point position, bool ui_scale)
	{
		Game1.setMousePosition(position.X, position.Y, ui_scale);
	}

	public static void setMousePosition(Point position)
	{
		Game1.setMousePosition(position, Game1.uiMode);
	}

	public static void setMousePositionRaw(int x, int y)
	{
		Game1.input.SetMousePosition(x, y);
		Game1.InvalidateOldMouseMovement();
		Game1.lastCursorMotionWasMouse = false;
	}

	public static Point getMousePositionRaw()
	{
		return new Point(Game1.getMouseXRaw(), Game1.getMouseYRaw());
	}

	public static Point getMousePosition(bool ui_scale)
	{
		return new Point(Game1.getMouseX(ui_scale), Game1.getMouseY(ui_scale));
	}

	public static Point getMousePosition()
	{
		return Game1.getMousePosition(Game1.uiMode);
	}

	private static void ComputeCursorSpeed()
	{
		Game1._cursorSpeedDirty = false;
		GamePadState p = Game1.input.GetGamePadState();
		float accellTol = 0.9f;
		bool isAccell = false;
		float num = p.ThumbSticks.Left.Length();
		float rlen = p.ThumbSticks.Right.Length();
		if (num > accellTol || rlen > accellTol)
		{
			isAccell = true;
		}
		float min = 0.7f;
		float max = 2f;
		float rate = 1f;
		if (Game1._cursorDragEnabled)
		{
			min = 0.5f;
			max = 2f;
			rate = 1f;
		}
		if (!isAccell)
		{
			rate = -5f;
		}
		if (Game1._cursorDragPrevEnabled != Game1._cursorDragEnabled)
		{
			Game1._cursorSpeedScale *= 0.5f;
		}
		Game1._cursorDragPrevEnabled = Game1._cursorDragEnabled;
		Game1._cursorSpeedScale += Game1._cursorUpdateElapsedSec * rate;
		Game1._cursorSpeedScale = MathHelper.Clamp(Game1._cursorSpeedScale, min, max);
		float num2 = 16f / (float)Game1.game1.TargetElapsedTime.TotalSeconds * Game1._cursorSpeedScale;
		float deltaSpeed = num2 - Game1._cursorSpeed;
		Game1._cursorSpeed = num2;
		Game1._cursorUpdateElapsedSec = 0f;
		if (Game1.debugMode)
		{
			Game1.log.Verbose("_cursorSpeed=" + Game1._cursorSpeed.ToString("0.0") + ", _cursorSpeedScale=" + Game1._cursorSpeedScale.ToString("0.0") + ", deltaSpeed=" + deltaSpeed.ToString("0.0"));
		}
	}

	private static void SetFreeCursorElapsed(float elapsedSec)
	{
		if (elapsedSec != Game1._cursorUpdateElapsedSec)
		{
			Game1._cursorUpdateElapsedSec = elapsedSec;
			Game1._cursorSpeedDirty = true;
		}
	}

	public static void ResetFreeCursorDrag()
	{
		if (Game1._cursorDragEnabled)
		{
			Game1._cursorSpeedDirty = true;
		}
		Game1._cursorDragEnabled = false;
	}

	public static void SetFreeCursorDrag()
	{
		if (!Game1._cursorDragEnabled)
		{
			Game1._cursorSpeedDirty = true;
		}
		Game1._cursorDragEnabled = true;
	}

	public static void updateActiveMenu(GameTime gameTime)
	{
		IClickableMenu active_menu = Game1.activeClickableMenu;
		while (active_menu.GetChildMenu() != null)
		{
			active_menu = active_menu.GetChildMenu();
		}
		if (!Program.gamePtr.IsActiveNoOverlay && Program.releaseBuild)
		{
			if (active_menu != null && active_menu.IsActive())
			{
				active_menu.update(gameTime);
			}
			return;
		}
		MouseState mouseState = Game1.input.GetMouseState();
		KeyboardState keyState = Game1.GetKeyboardState();
		GamePadState padState = Game1.input.GetGamePadState();
		if (Game1.CurrentEvent != null)
		{
			if ((mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonUp(Buttons.A)))
			{
				Game1.CurrentEvent.receiveMouseClick(Game1.getMouseX(), Game1.getMouseY());
			}
			else if (Game1.options.gamepadControls && padState.IsButtonDown(Buttons.Back) && Game1.oldPadState.IsButtonUp(Buttons.Back) && !Game1.CurrentEvent.skipped && Game1.CurrentEvent.skippable)
			{
				Game1.CurrentEvent.skipped = true;
				Game1.CurrentEvent.skipEvent();
				Game1.freezeControls = false;
			}
			if (Game1.CurrentEvent != null && Game1.CurrentEvent.skipped)
			{
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldKBState = keyState;
				Game1.oldPadState = padState;
				return;
			}
		}
		if (Game1.options.gamepadControls && active_menu != null && active_menu.IsActive())
		{
			if (Game1.isGamePadThumbstickInMotion() && (!Game1.options.snappyMenus || active_menu.overrideSnappyMenuCursorMovementBan()))
			{
				Game1.setMousePositionRaw((int)((float)mouseState.X + padState.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier), (int)((float)mouseState.Y - padState.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier));
			}
			if (active_menu != null && active_menu.IsActive() && (Game1.chatBox == null || !Game1.chatBox.isActive()))
			{
				ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, Game1.oldPadState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons b = enumerator.Current;
					active_menu.receiveGamePadButton(b);
					if (active_menu == null || !active_menu.IsActive())
					{
						break;
					}
				}
				enumerator = Utility.getHeldButtons(padState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons b2 = enumerator.Current;
					if (active_menu != null && active_menu.IsActive())
					{
						active_menu.gamePadButtonHeld(b2);
					}
					if (active_menu == null || !active_menu.IsActive())
					{
						break;
					}
				}
			}
		}
		if ((Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY()) && !Game1.isGamePadThumbstickInMotion() && !Game1.isDPadPressed())
		{
			Game1.lastCursorMotionWasMouse = true;
		}
		Game1.ResetFreeCursorDrag();
		if (active_menu != null && active_menu.IsActive())
		{
			active_menu.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
		}
		if (active_menu != null && active_menu.IsActive())
		{
			active_menu.update(gameTime);
		}
		if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
		{
			if (Game1.chatBox != null && Game1.chatBox.isActive() && Game1.chatBox.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
			{
				Game1.chatBox.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
			}
			else
			{
				active_menu.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
			}
		}
		else if (active_menu != null && active_menu.IsActive() && mouseState.RightButton == ButtonState.Pressed && (Game1.oldMouseState.RightButton == ButtonState.Released || ((float)Game1.mouseClickPolling > 650f && !(active_menu is DialogueBox))))
		{
			active_menu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
			if ((float)Game1.mouseClickPolling > 650f)
			{
				Game1.mouseClickPolling = 600;
			}
			if ((active_menu == null || !active_menu.IsActive()) && Game1.activeClickableMenu == null)
			{
				Game1.rightClickPolling = 500;
				Game1.mouseClickPolling = 0;
			}
		}
		if (mouseState.ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue && active_menu != null && active_menu.IsActive())
		{
			if (Game1.chatBox != null && Game1.chatBox.choosingEmoji && Game1.chatBox.emojiMenu.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				Game1.chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
			}
			else
			{
				active_menu.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
			}
		}
		if (Game1.options.gamepadControls && active_menu != null && active_menu.IsActive())
		{
			Game1.thumbstickPollingTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (Game1.thumbstickPollingTimer <= 0)
			{
				if (padState.ThumbSticks.Right.Y > 0.2f)
				{
					active_menu.receiveScrollWheelAction(1);
				}
				else if (padState.ThumbSticks.Right.Y < -0.2f)
				{
					active_menu.receiveScrollWheelAction(-1);
				}
			}
			if (Game1.thumbstickPollingTimer <= 0)
			{
				Game1.thumbstickPollingTimer = 220 - (int)(Math.Abs(padState.ThumbSticks.Right.Y) * 170f);
			}
			if (Math.Abs(padState.ThumbSticks.Right.Y) < 0.2f)
			{
				Game1.thumbstickPollingTimer = 0;
			}
		}
		if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
		{
			active_menu.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
		}
		else if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
		{
			active_menu.leftClickHeld(Game1.getMouseX(), Game1.getMouseY());
		}
		Keys[] pressedKeys = keyState.GetPressedKeys();
		foreach (Keys k in pressedKeys)
		{
			if (active_menu != null && active_menu.IsActive() && !Game1.oldKBState.GetPressedKeys().Contains(k))
			{
				active_menu.receiveKeyPress(k);
			}
		}
		if (Game1.chatBox == null || !Game1.chatBox.isActive())
		{
			if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y || padState.IsButtonDown(Buttons.DPadUp))))
			{
				Game1.directionKeyPolling[0] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadRight))))
			{
				Game1.directionKeyPolling[1] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadDown))))
			{
				Game1.directionKeyPolling[2] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadLeft))))
			{
				Game1.directionKeyPolling[3] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveUpButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y < 0.1 && padState.IsButtonUp(Buttons.DPadUp))))
			{
				Game1.directionKeyPolling[0] = 250;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveRightButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X < 0.1 && padState.IsButtonUp(Buttons.DPadRight))))
			{
				Game1.directionKeyPolling[1] = 250;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveDownButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y > -0.1 && padState.IsButtonUp(Buttons.DPadDown))))
			{
				Game1.directionKeyPolling[2] = 250;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveLeftButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X > -0.1 && padState.IsButtonUp(Buttons.DPadLeft))))
			{
				Game1.directionKeyPolling[3] = 250;
			}
			if (Game1.directionKeyPolling[0] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton));
				Game1.directionKeyPolling[0] = 70;
			}
			if (Game1.directionKeyPolling[1] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton));
				Game1.directionKeyPolling[1] = 70;
			}
			if (Game1.directionKeyPolling[2] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton));
				Game1.directionKeyPolling[2] = 70;
			}
			if (Game1.directionKeyPolling[3] <= 0 && active_menu != null && active_menu.IsActive())
			{
				active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton));
				Game1.directionKeyPolling[3] = 70;
			}
			if (Game1.options.gamepadControls && active_menu != null && active_menu.IsActive())
			{
				if (!active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && (!Game1.oldPadState.IsButtonDown(Buttons.A) || ((float)Game1.gamePadAButtonPolling > 650f && !(active_menu is DialogueBox))))
				{
					active_menu.receiveLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
					if ((float)Game1.gamePadAButtonPolling > 650f)
					{
						Game1.gamePadAButtonPolling = 600;
					}
				}
				else if (!active_menu.areGamePadControlsImplemented() && !padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
				{
					active_menu.releaseLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
				}
				else if (!active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.X) && (!Game1.oldPadState.IsButtonDown(Buttons.X) || ((float)Game1.gamePadXButtonPolling > 650f && !(active_menu is DialogueBox))))
				{
					active_menu.receiveRightClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
					if ((float)Game1.gamePadXButtonPolling > 650f)
					{
						Game1.gamePadXButtonPolling = 600;
					}
				}
				ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, Game1.oldPadState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons b3 = enumerator.Current;
					if (active_menu == null || !active_menu.IsActive())
					{
						break;
					}
					Keys key = Utility.mapGamePadButtonToKey(b3);
					if (!(active_menu is FarmhandMenu) || Game1.game1.IsMainInstance || !Game1.options.doesInputListContain(Game1.options.menuButton, key))
					{
						active_menu.receiveKeyPress(key);
					}
				}
				if (active_menu != null && active_menu.IsActive() && !active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
				{
					active_menu.leftClickHeld(Game1.getMousePosition().X, Game1.getMousePosition().Y);
				}
				if (padState.IsButtonDown(Buttons.X))
				{
					Game1.gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					Game1.gamePadXButtonPolling = 0;
				}
				if (padState.IsButtonDown(Buttons.A))
				{
					Game1.gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					Game1.gamePadAButtonPolling = 0;
				}
				if (!active_menu.IsActive() && Game1.activeClickableMenu == null)
				{
					Game1.rightClickPolling = 500;
					Game1.gamePadXButtonPolling = 0;
					Game1.gamePadAButtonPolling = 0;
				}
			}
		}
		if (mouseState.RightButton == ButtonState.Pressed)
		{
			Game1.mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
		}
		else
		{
			Game1.mouseClickPolling = 0;
		}
		Game1.oldMouseState = Game1.input.GetMouseState();
		Game1.oldKBState = keyState;
		Game1.oldPadState = padState;
	}

	public bool ShowLocalCoopJoinMenu()
	{
		if (!base.IsMainInstance)
		{
			return false;
		}
		if (Game1.gameMode != 3)
		{
			return false;
		}
		int free_farmhands = 0;
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			if (location is Cabin cabin && (!cabin.HasOwner || !cabin.IsOwnerActivated))
			{
				free_farmhands++;
			}
			return true;
		});
		if (free_farmhands == 0)
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:CoopMenu_NoSlots"));
			return false;
		}
		if (Game1.currentMinigame != null)
		{
			return false;
		}
		if (Game1.activeClickableMenu != null)
		{
			return false;
		}
		if (!this.IsLocalCoopJoinable())
		{
			return false;
		}
		Game1.playSound("bigSelect");
		Game1.activeClickableMenu = new LocalCoopJoinMenu();
		return true;
	}

	public static void updateTextEntry(GameTime gameTime)
	{
		MouseState mouseState = Game1.input.GetMouseState();
		KeyboardState keyState = Game1.GetKeyboardState();
		GamePadState padState = Game1.input.GetGamePadState();
		if (Game1.options.gamepadControls && Game1.textEntry != null && Game1.textEntry != null)
		{
			ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, Game1.oldPadState).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Buttons b = enumerator.Current;
				Game1.textEntry.receiveGamePadButton(b);
				if (Game1.textEntry == null)
				{
					break;
				}
			}
			enumerator = Utility.getHeldButtons(padState).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Buttons b2 = enumerator.Current;
				Game1.textEntry?.gamePadButtonHeld(b2);
				if (Game1.textEntry == null)
				{
					break;
				}
			}
		}
		Game1.textEntry?.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
		Game1.textEntry?.update(gameTime);
		if (Game1.textEntry != null && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
		{
			Game1.textEntry.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
		}
		else if (Game1.textEntry != null && mouseState.RightButton == ButtonState.Pressed && (Game1.oldMouseState.RightButton == ButtonState.Released || (float)Game1.mouseClickPolling > 650f))
		{
			Game1.textEntry.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
			if ((float)Game1.mouseClickPolling > 650f)
			{
				Game1.mouseClickPolling = 600;
			}
			if (Game1.textEntry == null)
			{
				Game1.rightClickPolling = 500;
				Game1.mouseClickPolling = 0;
			}
		}
		if (mouseState.ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue && Game1.textEntry != null)
		{
			if (Game1.chatBox != null && Game1.chatBox.choosingEmoji && Game1.chatBox.emojiMenu.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				Game1.chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
			}
			else
			{
				Game1.textEntry.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
			}
		}
		if (Game1.options.gamepadControls && Game1.textEntry != null)
		{
			Game1.thumbstickPollingTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (Game1.thumbstickPollingTimer <= 0)
			{
				if (padState.ThumbSticks.Right.Y > 0.2f)
				{
					Game1.textEntry.receiveScrollWheelAction(1);
				}
				else if (padState.ThumbSticks.Right.Y < -0.2f)
				{
					Game1.textEntry.receiveScrollWheelAction(-1);
				}
			}
			if (Game1.thumbstickPollingTimer <= 0)
			{
				Game1.thumbstickPollingTimer = 220 - (int)(Math.Abs(padState.ThumbSticks.Right.Y) * 170f);
			}
			if (Math.Abs(padState.ThumbSticks.Right.Y) < 0.2f)
			{
				Game1.thumbstickPollingTimer = 0;
			}
		}
		if (Game1.textEntry != null && mouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
		{
			Game1.textEntry.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
		}
		else if (Game1.textEntry != null && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
		{
			Game1.textEntry.leftClickHeld(Game1.getMouseX(), Game1.getMouseY());
		}
		Keys[] pressedKeys = keyState.GetPressedKeys();
		foreach (Keys k in pressedKeys)
		{
			if (Game1.textEntry != null && !Game1.oldKBState.GetPressedKeys().Contains(k))
			{
				Game1.textEntry.receiveKeyPress(k);
			}
		}
		if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y || padState.IsButtonDown(Buttons.DPadUp))))
		{
			Game1.directionKeyPolling[0] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
		else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadRight))))
		{
			Game1.directionKeyPolling[1] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
		else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadDown))))
		{
			Game1.directionKeyPolling[2] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
		else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadLeft))))
		{
			Game1.directionKeyPolling[3] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
		if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveUpButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y < 0.1 && padState.IsButtonUp(Buttons.DPadUp))))
		{
			Game1.directionKeyPolling[0] = 250;
		}
		if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveRightButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X < 0.1 && padState.IsButtonUp(Buttons.DPadRight))))
		{
			Game1.directionKeyPolling[1] = 250;
		}
		if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveDownButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y > -0.1 && padState.IsButtonUp(Buttons.DPadDown))))
		{
			Game1.directionKeyPolling[2] = 250;
		}
		if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveLeftButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X > -0.1 && padState.IsButtonUp(Buttons.DPadLeft))))
		{
			Game1.directionKeyPolling[3] = 250;
		}
		if (Game1.directionKeyPolling[0] <= 0 && Game1.textEntry != null)
		{
			Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton));
			Game1.directionKeyPolling[0] = 70;
		}
		if (Game1.directionKeyPolling[1] <= 0 && Game1.textEntry != null)
		{
			Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton));
			Game1.directionKeyPolling[1] = 70;
		}
		if (Game1.directionKeyPolling[2] <= 0 && Game1.textEntry != null)
		{
			Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton));
			Game1.directionKeyPolling[2] = 70;
		}
		if (Game1.directionKeyPolling[3] <= 0 && Game1.textEntry != null)
		{
			Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton));
			Game1.directionKeyPolling[3] = 70;
		}
		if (Game1.options.gamepadControls && Game1.textEntry != null)
		{
			if (!Game1.textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && (!Game1.oldPadState.IsButtonDown(Buttons.A) || (float)Game1.gamePadAButtonPolling > 650f))
			{
				Game1.textEntry.receiveLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
				if ((float)Game1.gamePadAButtonPolling > 650f)
				{
					Game1.gamePadAButtonPolling = 600;
				}
			}
			else if (!Game1.textEntry.areGamePadControlsImplemented() && !padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
			{
				Game1.textEntry.releaseLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
			}
			else if (!Game1.textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.X) && (!Game1.oldPadState.IsButtonDown(Buttons.X) || (float)Game1.gamePadXButtonPolling > 650f))
			{
				Game1.textEntry.receiveRightClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
				if ((float)Game1.gamePadXButtonPolling > 650f)
				{
					Game1.gamePadXButtonPolling = 600;
				}
			}
			ButtonCollection.ButtonEnumerator enumerator = Utility.getPressedButtons(padState, Game1.oldPadState).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Buttons b3 = enumerator.Current;
				if (Game1.textEntry == null)
				{
					break;
				}
				Game1.textEntry.receiveKeyPress(Utility.mapGamePadButtonToKey(b3));
			}
			if (Game1.textEntry != null && !Game1.textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
			{
				Game1.textEntry.leftClickHeld(Game1.getMousePosition().X, Game1.getMousePosition().Y);
			}
			if (padState.IsButtonDown(Buttons.X))
			{
				Game1.gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				Game1.gamePadXButtonPolling = 0;
			}
			if (padState.IsButtonDown(Buttons.A))
			{
				Game1.gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				Game1.gamePadAButtonPolling = 0;
			}
			if (Game1.textEntry == null)
			{
				Game1.rightClickPolling = 500;
				Game1.gamePadAButtonPolling = 0;
				Game1.gamePadXButtonPolling = 0;
			}
		}
		if (mouseState.RightButton == ButtonState.Pressed)
		{
			Game1.mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
		}
		else
		{
			Game1.mouseClickPolling = 0;
		}
		Game1.oldMouseState = Game1.input.GetMouseState();
		Game1.oldKBState = keyState;
		Game1.oldPadState = padState;
	}

	public static string DateCompiled()
	{
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
	}

	public static void updatePause(GameTime gameTime)
	{
		if (Game1.IsDedicatedHost)
		{
			Game1.pauseTime = 0f;
		}
		Game1.pauseTime -= gameTime.ElapsedGameTime.Milliseconds;
		if (Game1.player.isCrafting && Game1.random.NextDouble() < 0.007)
		{
			Game1.playSound("crafting");
		}
		if (!(Game1.pauseTime <= 0f))
		{
			return;
		}
		if (Game1.currentObjectDialogue.Count == 0)
		{
			Game1.messagePause = false;
		}
		Game1.pauseTime = 0f;
		if (!string.IsNullOrEmpty(Game1.messageAfterPause))
		{
			Game1.player.isCrafting = false;
			Game1.drawObjectDialogue(Game1.messageAfterPause);
			Game1.messageAfterPause = "";
			if (Game1.killScreen)
			{
				Game1.killScreen = false;
				Game1.player.health = 10;
			}
		}
		else if (Game1.killScreen)
		{
			Game1.multiplayer.globalChatInfoMessage("PlayerDeath", Game1.player.Name);
			Game1.screenGlow = false;
			bool handledRevive = false;
			if (Game1.currentLocation.GetLocationContext().ReviveLocations != null)
			{
				foreach (ReviveLocation revive_location in Game1.currentLocation.GetLocationContext().ReviveLocations)
				{
					if (GameStateQuery.CheckConditions(revive_location.Condition, null, Game1.player))
					{
						Game1.warpFarmer(revive_location.Location, revive_location.Position.X, revive_location.Position.Y, flip: false);
						handledRevive = true;
						break;
					}
				}
			}
			else
			{
				foreach (ReviveLocation revive_location2 in LocationContexts.Default.ReviveLocations)
				{
					if (GameStateQuery.CheckConditions(revive_location2.Condition, null, Game1.player))
					{
						Game1.warpFarmer(revive_location2.Location, revive_location2.Position.X, revive_location2.Position.Y, flip: false);
						handledRevive = true;
						break;
					}
				}
			}
			if (!handledRevive)
			{
				Game1.warpFarmer("Hospital", 20, 12, flip: false);
			}
		}
		if (Game1.currentLocation.currentEvent != null)
		{
			Game1.currentLocation.currentEvent.CurrentCommand++;
		}
	}

	public static void CheckValidFullscreenResolution(ref int width, ref int height)
	{
		int preferredW = width;
		int preferredH = height;
		foreach (DisplayMode v in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
		{
			if (v.Width >= 1280 && v.Width == preferredW && v.Height == preferredH)
			{
				width = preferredW;
				height = preferredH;
				return;
			}
		}
		foreach (DisplayMode v2 in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
		{
			if (v2.Width >= 1280 && v2.Width == Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width && v2.Height == Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height)
			{
				width = Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
				height = Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
				return;
			}
		}
		bool found_resolution = false;
		foreach (DisplayMode v3 in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
		{
			if (v3.Width >= 1280 && preferredW > v3.Width)
			{
				width = v3.Width;
				height = v3.Height;
				found_resolution = true;
			}
		}
		if (!found_resolution)
		{
			Game1.log.Warn("Requested fullscreen resolution not valid, switching to windowed.");
			width = 1280;
			height = 720;
			Game1.options.fullscreen = false;
		}
	}

	public static void toggleNonBorderlessWindowedFullscreen()
	{
		int width = Game1.options.preferredResolutionX;
		int height = Game1.options.preferredResolutionY;
		Game1.graphics.HardwareModeSwitch = Game1.options.fullscreen && !Game1.options.windowedBorderlessFullscreen;
		if (Game1.options.fullscreen && !Game1.options.windowedBorderlessFullscreen)
		{
			Game1.CheckValidFullscreenResolution(ref width, ref height);
		}
		if (!Game1.options.fullscreen && !Game1.options.windowedBorderlessFullscreen)
		{
			width = 1280;
			height = 720;
		}
		Game1.graphics.PreferredBackBufferWidth = width;
		Game1.graphics.PreferredBackBufferHeight = height;
		if (Game1.options.fullscreen != Game1.graphics.IsFullScreen)
		{
			Game1.graphics.ToggleFullScreen();
		}
		Game1.graphics.ApplyChanges();
		Game1.updateViewportForScreenSizeChange(fullscreenChange: true, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
		GameRunner.instance.OnWindowSizeChange(null, null);
	}

	public static void toggleFullscreen()
	{
		if (Game1.options.windowedBorderlessFullscreen)
		{
			Game1.graphics.HardwareModeSwitch = false;
			Game1.graphics.IsFullScreen = true;
			Game1.graphics.ApplyChanges();
			Game1.graphics.PreferredBackBufferWidth = Program.gamePtr.Window.ClientBounds.Width;
			Game1.graphics.PreferredBackBufferHeight = Program.gamePtr.Window.ClientBounds.Height;
		}
		else
		{
			Game1.toggleNonBorderlessWindowedFullscreen();
		}
		GameRunner.instance.OnWindowSizeChange(null, null);
	}

	private void checkForEscapeKeys()
	{
		KeyboardState kbState = Game1.input.GetKeyboardState();
		if (!base.IsMainInstance)
		{
			return;
		}
		if (kbState.IsKeyDown(Keys.LeftAlt) && kbState.IsKeyDown(Keys.Enter) && (Game1.oldKBState.IsKeyUp(Keys.LeftAlt) || Game1.oldKBState.IsKeyUp(Keys.Enter)))
		{
			if (Game1.options.isCurrentlyFullscreen() || Game1.options.isCurrentlyWindowedBorderless())
			{
				Game1.options.setWindowedOption(1);
			}
			else
			{
				Game1.options.setWindowedOption(0);
			}
		}
		if ((Game1.player.UsingTool || Game1.freezeControls) && kbState.IsKeyDown(Keys.RightShift) && kbState.IsKeyDown(Keys.R) && kbState.IsKeyDown(Keys.Delete))
		{
			Game1.freezeControls = false;
			Game1.player.forceCanMove();
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.player.UsingTool = false;
		}
	}

	public static bool IsPressEvent(ref KeyboardState state, Keys key)
	{
		if (state.IsKeyDown(key) && !Game1.oldKBState.IsKeyDown(key))
		{
			Game1.oldKBState = state;
			return true;
		}
		return false;
	}

	public static bool IsPressEvent(ref GamePadState state, Buttons btn)
	{
		if (state.IsConnected && state.IsButtonDown(btn) && !Game1.oldPadState.IsButtonDown(btn))
		{
			Game1.oldPadState = state;
			return true;
		}
		return false;
	}

	public static bool isOneOfTheseKeysDown(KeyboardState state, InputButton[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			InputButton k = keys[i];
			if (k.key != Keys.None && state.IsKeyDown(k.key))
			{
				return true;
			}
		}
		return false;
	}

	public static bool areAllOfTheseKeysUp(KeyboardState state, InputButton[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			InputButton k = keys[i];
			if (k.key != Keys.None && !state.IsKeyUp(k.key))
			{
				return false;
			}
		}
		return true;
	}

	internal void UpdateTitleScreen(GameTime time)
	{
		if (Game1.quit)
		{
			base.Exit();
			Game1.changeMusicTrack("none");
		}
		switch (Game1.gameMode)
		{
		case 6:
			this.UpdateTitleScreenDuringLoadingMode();
			return;
		case 7:
			Game1.currentLoader.MoveNext();
			return;
		case 8:
			Game1.pauseAccumulator -= time.ElapsedGameTime.Milliseconds;
			if (Game1.pauseAccumulator <= 0f)
			{
				Game1.pauseAccumulator = 0f;
				Game1.setGameMode(3);
				if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.messagePause = true;
					Game1.pauseTime = 1E+10f;
					Game1.fadeToBlackAlpha = 1f;
					Game1.player.CanMove = false;
				}
			}
			return;
		}
		if (Game1.game1.instanceIndex > 0)
		{
			if (Game1.activeClickableMenu == null && Game1.ticks > 1)
			{
				Game1.activeClickableMenu = new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient("localhost")));
				Game1.activeClickableMenu.populateClickableComponentList();
				if (Game1.options.SnappyMenus)
				{
					Game1.activeClickableMenu.snapToDefaultClickableComponent();
				}
			}
			return;
		}
		if (Game1.fadeToBlackAlpha < 1f && Game1.fadeIn)
		{
			Game1.fadeToBlackAlpha += 0.02f;
		}
		else if (Game1.fadeToBlackAlpha > 0f && Game1.fadeToBlack)
		{
			Game1.fadeToBlackAlpha -= 0.02f;
		}
		if (Game1.pauseTime > 0f)
		{
			Game1.pauseTime = Math.Max(0f, Game1.pauseTime - (float)time.ElapsedGameTime.Milliseconds);
		}
		if (Game1.fadeToBlackAlpha >= 1f)
		{
			switch (Game1.gameMode)
			{
			case 4:
				if (!Game1.fadeToBlack)
				{
					Game1.fadeIn = false;
					Game1.fadeToBlack = true;
					Game1.fadeToBlackAlpha = 2.5f;
				}
				break;
			case 0:
				if (Game1.currentSong == null && Game1.pauseTime <= 0f && base.IsMainInstance)
				{
					Game1.playSound("spring_day_ambient", out var cue);
					Game1.currentSong = cue;
				}
				if (Game1.activeClickableMenu == null && !Game1.quit)
				{
					Game1.activeClickableMenu = new TitleMenu();
				}
				break;
			}
			return;
		}
		if (!(Game1.fadeToBlackAlpha <= 0f))
		{
			return;
		}
		switch (Game1.gameMode)
		{
		case 4:
			if (Game1.fadeToBlack)
			{
				Game1.fadeIn = true;
				Game1.fadeToBlack = false;
				Game1.setGameMode(0);
				Game1.pauseTime = 2000f;
			}
			break;
		case 0:
			if (Game1.fadeToBlack)
			{
				Game1.currentLoader = Utility.generateNewFarm(Game1.IsClient);
				Game1.setGameMode(6);
				Game1.loadingMessage = (Game1.IsClient ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574", Game1.client.serverName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
				Game1.exitActiveMenu();
			}
			break;
		}
	}

	/// <summary>Update while the <see cref="P:StardewValley.Game1.gameMode" /> is <see cref="F:StardewValley.Game1.loadingMode" /> and the <see cref="F:StardewValley.Game1.currentLoader" /> is running.</summary>
	/// <remarks>This is a low-level method; most code should call <see cref="M:StardewValley.Game1.UpdateTitleScreen(Microsoft.Xna.Framework.GameTime)" /> instead.</remarks>
	internal void UpdateTitleScreenDuringLoadingMode()
	{
		if (Game1._requestedMusicTracks.Count > 0)
		{
			Game1._requestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>();
		}
		Game1.requestedMusicTrack = "none";
		Game1.requestedMusicTrackOverrideable = false;
		Game1.requestedMusicDirty = true;
		if (Game1.currentLoader != null && !Game1.currentLoader.MoveNext())
		{
			Game1.currentLoader = null;
			if (Game1.gameMode == 3)
			{
				Game1.setGameMode(3);
				Game1.fadeIn = true;
				Game1.fadeToBlackAlpha = 0.99f;
			}
			else
			{
				Game1.ExitToTitle();
			}
		}
	}

	/// <summary>Get whether the given NPC is currently constructing a building anywhere in the world.</summary>
	/// <param name="builder">The NPC constructing the building, usually <see cref="F:StardewValley.Game1.builder_robin" /> or <see cref="F:StardewValley.Game1.builder_wizard" />.</param>
	public static bool IsThereABuildingUnderConstruction(string builder = "Robin")
	{
		if (Game1.netWorldState.Value.GetBuilderData(builder) != null)
		{
			return true;
		}
		return false;
	}

	/// <summary>Get the building currently being constructed by a given builder.</summary>
	/// <param name="builder">The NPC constructing the building, usually <see cref="F:StardewValley.Game1.builder_robin" /> or <see cref="F:StardewValley.Game1.builder_wizard" />.</param>
	public static Building GetBuildingUnderConstruction(string builder = "Robin")
	{
		BuilderData builder_data = Game1.netWorldState.Value.GetBuilderData(builder);
		if (builder_data == null)
		{
			return null;
		}
		GameLocation location = Game1.getLocationFromName(builder_data.buildingLocation.Value);
		if (location == null)
		{
			return null;
		}
		if (Game1.client != null && !Game1.multiplayer.isActiveLocation(location))
		{
			return null;
		}
		return location.getBuildingAt(Utility.PointToVector2(builder_data.buildingTile.Value));
	}

	/// <summary>Get whether a building type was constructed anywhere in the world.</summary>
	/// <param name="name">The building type's ID in <c>Data/Buildings</c>.</param>
	public static bool IsBuildingConstructed(string name)
	{
		return Game1.GetNumberBuildingsConstructed(name) > 0;
	}

	/// <summary>Get the number of buildings of all types constructed anywhere in the world.</summary>
	/// <param name="includeUnderConstruction">Whether to count buildings that haven't finished construction yet.</param>
	public static int GetNumberBuildingsConstructed(bool includeUnderConstruction = false)
	{
		int count = 0;
		foreach (string locationName in Game1.netWorldState.Value.LocationsWithBuildings)
		{
			count += Game1.getLocationFromName(locationName)?.getNumberBuildingsConstructed(includeUnderConstruction) ?? 0;
		}
		return count;
	}

	/// <summary>Get the number of buildings of a given type constructed anywhere in the world.</summary>
	/// <param name="name">The building type's ID in <c>Data/Buildings</c>.</param>
	/// <param name="includeUnderConstruction">Whether to count buildings that haven't finished construction yet.</param>
	public static int GetNumberBuildingsConstructed(string name, bool includeUnderConstruction = false)
	{
		int count = 0;
		foreach (string locationName in Game1.netWorldState.Value.LocationsWithBuildings)
		{
			count += Game1.getLocationFromName(locationName)?.getNumberBuildingsConstructed(name, includeUnderConstruction) ?? 0;
		}
		return count;
	}

	private void UpdateLocations(GameTime time)
	{
		Game1.loopingLocationCues.Update(Game1.currentLocation);
		if (Game1.IsClient)
		{
			Game1.currentLocation.UpdateWhenCurrentLocation(time);
			{
				foreach (GameLocation item in Game1.multiplayer.activeLocations())
				{
					item.updateEvenIfFarmerIsntHere(time);
				}
				return;
			}
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			this._UpdateLocation(location, time);
			return true;
		});
		if (Game1.currentLocation.IsTemporary)
		{
			this._UpdateLocation(Game1.currentLocation, time);
		}
		MineShaft.UpdateMines(time);
		VolcanoDungeon.UpdateLevels(time);
	}

	protected void _UpdateLocation(GameLocation location, GameTime time)
	{
		bool shouldUpdate = location.farmers.Any();
		if (!shouldUpdate && location.CanBeRemotedlyViewed())
		{
			if (Game1.player.currentLocation == location)
			{
				shouldUpdate = true;
			}
			else
			{
				foreach (Farmer who in Game1.otherFarmers.Values)
				{
					if (who.viewingLocation.Value != null && who.viewingLocation.Value.Equals(location.NameOrUniqueName))
					{
						shouldUpdate = true;
						break;
					}
				}
			}
		}
		if (shouldUpdate)
		{
			location.UpdateWhenCurrentLocation(time);
		}
		location.updateEvenIfFarmerIsntHere(time);
		if (location.wasInhabited != shouldUpdate)
		{
			location.wasInhabited = shouldUpdate;
			if (Game1.IsMasterGame)
			{
				location.cleanupForVacancy();
			}
		}
	}

	public static void performTenMinuteClockUpdate()
	{
		Game1.hooks.OnGame1_PerformTenMinuteClockUpdate(delegate
		{
			int num = Game1.getTrulyDarkTime(Game1.currentLocation) - 100;
			Game1.gameTimeInterval = 0;
			if (Game1.IsMasterGame)
			{
				Game1.timeOfDay += 10;
			}
			if (Game1.timeOfDay % 100 >= 60)
			{
				Game1.timeOfDay = Game1.timeOfDay - Game1.timeOfDay % 100 + 100;
			}
			Game1.timeOfDay = Math.Min(Game1.timeOfDay, 2600);
			if (Game1.isLightning && Game1.timeOfDay < 2400 && Game1.IsMasterGame)
			{
				Utility.performLightningUpdate(Game1.timeOfDay);
			}
			if (Game1.timeOfDay == num)
			{
				Game1.currentLocation.switchOutNightTiles();
			}
			else if (Game1.timeOfDay == Game1.getModeratelyDarkTime(Game1.currentLocation) && Game1.currentLocation.IsOutdoors && !Game1.currentLocation.IsRainingHere())
			{
				Game1.ambientLight = Color.White;
			}
			if (!Game1.eventUp && Game1.isDarkOut(Game1.currentLocation) && Game1.IsPlayingBackgroundMusic)
			{
				Game1.changeMusicTrack("none", track_interruptable: true);
			}
			if (Game1.weatherIcon == 1)
			{
				Dictionary<string, string> dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth);
				string[] array = dictionary["conditions"].Split('/');
				int num2 = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(array[1], 0));
				if (Game1.whereIsTodaysFest == null)
				{
					Game1.whereIsTodaysFest = array[0];
				}
				if (Game1.timeOfDay == num2)
				{
					if (dictionary.TryGetValue("startedMessage", out var value))
					{
						Game1.showGlobalMessage(TokenParser.ParseText(value));
					}
					else
					{
						if (!dictionary.TryGetValue("locationDisplayName", out var value2))
						{
							value2 = array[0];
							value2 = value2 switch
							{
								"Forest" => Game1.IsWinter ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2634") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2635"), 
								"Town" => Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2637"), 
								"Beach" => Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2639"), 
								_ => TokenParser.ParseText(GameLocation.GetData(value2)?.DisplayName) ?? value2, 
							};
						}
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2640", dictionary["name"]) + value2);
					}
				}
			}
			Game1.player.performTenMinuteUpdate();
			switch (Game1.timeOfDay)
			{
			case 1200:
				if (Game1.currentLocation.isOutdoors.Value && !Game1.currentLocation.IsRainingHere() && (Game1.IsPlayingOutdoorsAmbience || Game1.currentSong == null || Game1.isMusicContextActiveButNotPlaying()))
				{
					Game1.playMorningSong();
				}
				break;
			case 2000:
				if (Game1.IsPlayingTownMusic)
				{
					Game1.changeMusicTrack("none", track_interruptable: true);
				}
				break;
			case 2400:
				Game1.dayTimeMoneyBox.timeShakeTimer = 2000;
				Game1.player.doEmote(24);
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2652"));
				break;
			case 2500:
				Game1.dayTimeMoneyBox.timeShakeTimer = 2000;
				Game1.player.doEmote(24);
				break;
			case 2600:
				Game1.dayTimeMoneyBox.timeShakeTimer = 2000;
				Game1.player.mount?.dismount();
				if (Game1.player.IsSitting())
				{
					Game1.player.StopSitting(animate: false);
				}
				if (Game1.player.UsingTool && (!(Game1.player.CurrentTool is FishingRod fishingRod) || (!fishingRod.isReeling && !fishingRod.pullingOutOfWater)))
				{
					if (Game1.player.UsingTool && Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod { fishCaught: not false } fishingRod2)
					{
						fishingRod2.doneHoldingFish(Game1.player, endOfNight: true);
					}
					else
					{
						Game1.player.completelyStopAnimatingOrDoingAction();
					}
				}
				break;
			case 2800:
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.emergencyShutDown();
					Game1.exitActiveMenu();
				}
				Game1.player.startToPassOut();
				Game1.player.mount?.dismount();
				break;
			}
			foreach (string current in Game1.netWorldState.Value.ActivePassiveFestivals)
			{
				if (Utility.TryGetPassiveFestivalData(current, out var data) && Game1.timeOfDay == data.StartTime && (!data.OnlyShowMessageOnFirstDay || Utility.GetDayOfPassiveFestival(current) == 1))
				{
					Game1.showGlobalMessage(TokenParser.ParseText(data.StartMessage));
				}
			}
			foreach (GameLocation location in Game1.locations)
			{
				GameLocation current2 = location;
				if (current2.NameOrUniqueName == Game1.currentLocation.NameOrUniqueName)
				{
					current2 = Game1.currentLocation;
				}
				current2.performTenMinuteUpdate(Game1.timeOfDay);
				current2.timeUpdate(10);
			}
			MineShaft.UpdateMines10Minutes(Game1.timeOfDay);
			VolcanoDungeon.UpdateLevels10Minutes(Game1.timeOfDay);
			if (Game1.IsMasterGame && Game1.farmEvent == null)
			{
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			Game1.currentLightSources.RemoveWhere((KeyValuePair<string, LightSource> p) => p.Value.color.A <= 0);
		});
	}

	public static bool shouldPlayMorningSong(bool loading_game = false)
	{
		if (Game1.eventUp)
		{
			return false;
		}
		if ((double)Game1.options.musicVolumeLevel <= 0.025)
		{
			return false;
		}
		if (Game1.timeOfDay >= 1200)
		{
			return false;
		}
		if (!loading_game)
		{
			if (Game1.currentSong != null)
			{
				return Game1.IsPlayingOutdoorsAmbience;
			}
			return false;
		}
		return true;
	}

	public static void UpdateGameClock(GameTime time)
	{
		if (Game1.shouldTimePass() && !Game1.IsClient)
		{
			Game1.gameTimeInterval += time.ElapsedGameTime.Milliseconds;
		}
		if (Game1.timeOfDay >= Game1.getTrulyDarkTime(Game1.currentLocation))
		{
			int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
			float transparency = Math.Min(0.93f, 0.75f + ((float)(adjustedTime - Game1.getTrulyDarkTime(Game1.currentLocation)) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f) * 0.000625f);
			Game1.outdoorLight = (Game1.IsRainingHere() ? Game1.ambientLight : Game1.eveningColor) * transparency;
		}
		else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime(Game1.currentLocation))
		{
			int adjustedTime2 = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
			float transparency2 = Math.Min(0.93f, 0.3f + ((float)(adjustedTime2 - Game1.getStartingToGetDarkTime(Game1.currentLocation)) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f) * 0.00225f);
			Game1.outdoorLight = (Game1.IsRainingHere() ? Game1.ambientLight : Game1.eveningColor) * transparency2;
		}
		else if (Game1.IsRainingHere())
		{
			Game1.outdoorLight = Game1.ambientLight * 0.3f;
		}
		else
		{
			Game1.outdoorLight = Game1.ambientLight;
		}
		int num = Game1.gameTimeInterval;
		int num2 = Game1.realMilliSecondsPerGameTenMinutes;
		GameLocation gameLocation = Game1.currentLocation;
		if (num > num2 + ((gameLocation != null) ? new int?(gameLocation.ExtraMillisecondsPerInGameMinute * 10) : ((int?)null)))
		{
			if (Game1.panMode)
			{
				Game1.gameTimeInterval = 0;
			}
			else
			{
				Game1.performTenMinuteClockUpdate();
			}
		}
	}

	public static Event getAvailableWeddingEvent()
	{
		if (Game1.weddingsToday.Count > 0)
		{
			long id = Game1.weddingsToday[0];
			Game1.weddingsToday.RemoveAt(0);
			Farmer farmer = Game1.GetPlayer(id);
			if (farmer == null)
			{
				return null;
			}
			if (farmer.hasRoommate())
			{
				return null;
			}
			if (farmer.spouse != null)
			{
				return Utility.getWeddingEvent(farmer);
			}
			long? spouseID = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
			Farmer spouse = Game1.GetPlayer(spouseID.Value);
			if (spouse == null)
			{
				return null;
			}
			if (!Game1.getOnlineFarmers().Contains(farmer) || !Game1.getOnlineFarmers().Contains(spouse))
			{
				return null;
			}
			Game1.player.team.GetFriendship(farmer.UniqueMultiplayerID, spouseID.Value).Status = FriendshipStatus.Married;
			Game1.player.team.GetFriendship(farmer.UniqueMultiplayerID, spouseID.Value).WeddingDate = new WorldDate(Game1.Date);
			return Utility.getWeddingEvent(farmer);
		}
		return null;
	}

	public static void exitActiveMenu()
	{
		Game1.activeClickableMenu = null;
	}

	/// <summary>Perform an action when <see cref="M:StardewValley.Farmer.IsBusyDoingSomething" /> becomes false for the current player (or do it immediately if it's already false).</summary>
	/// <param name="action">The action to perform.</param>
	public static void PerformActionWhenPlayerFree(Action action)
	{
		if (Game1.player.IsBusyDoingSomething())
		{
			Game1.actionsWhenPlayerFree.Add(action);
		}
		else
		{
			action();
		}
	}

	public static void fadeScreenToBlack()
	{
		Game1.screenFade.FadeScreenToBlack();
	}

	public static void fadeClear()
	{
		Game1.screenFade.FadeClear();
	}

	private bool onFadeToBlackComplete()
	{
		bool should_halt = false;
		if (Game1.killScreen)
		{
			Game1.viewportFreeze = true;
			Game1.viewport.X = -10000;
		}
		if (Game1.exitToTitle)
		{
			Game1.setGameMode(4);
			Game1.fadeIn = false;
			Game1.fadeToBlack = true;
			Game1.fadeToBlackAlpha = 0.01f;
			Game1.exitToTitle = false;
			Game1.changeMusicTrack("none");
			Game1.debrisWeather.Clear();
			return true;
		}
		if (Game1.timeOfDayAfterFade != -1)
		{
			Game1.timeOfDay = Game1.timeOfDayAfterFade;
			Game1.timeOfDayAfterFade = -1;
		}
		if (!Game1.nonWarpFade && Game1.locationRequest != null)
		{
			if (Game1.IsMasterGame && Game1.locationRequest.Location == null)
			{
				Game1.log.Error("Warp to " + Game1.locationRequest.Name + " failed: location wasn't found or couldn't be loaded.");
				Game1.locationRequest = null;
			}
			if (Game1.locationRequest != null)
			{
				GameLocation previousLocation = Game1.currentLocation;
				Game1.emoteMenu?.exitThisMenuNoSound();
				if (Game1.client != null)
				{
					Game1.currentLocation?.StoreCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
				}
				Game1.currentLocation.cleanupBeforePlayerExit();
				Game1.multiplayer.broadcastLocationDelta(Game1.currentLocation);
				bool hasResetLocation = false;
				Game1.displayFarmer = true;
				if (Game1.eventOver)
				{
					Game1.eventFinished();
					if (Game1.dayOfMonth == 0)
					{
						Game1.newDayAfterFade(delegate
						{
							Game1.player.Position = new Vector2(320f, 320f);
						});
					}
					return true;
				}
				if (Game1.locationRequest.IsRequestFor(Game1.currentLocation) && Game1.player.previousLocationName != "" && !Game1.eventUp && !MineShaft.IsGeneratedLevel(Game1.currentLocation))
				{
					Game1.player.Position = new Vector2(Game1.xLocationAfterWarp * 64, Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16);
					Game1.viewportFreeze = false;
					Game1.currentLocation.resetForPlayerEntry();
					hasResetLocation = true;
				}
				else
				{
					if (MineShaft.IsGeneratedLevel(Game1.locationRequest.Name))
					{
						MineShaft mine = Game1.locationRequest.Location as MineShaft;
						if (Game1.player.IsSitting())
						{
							Game1.player.StopSitting(animate: false);
						}
						Game1.player.Halt();
						Game1.player.forceCanMove();
						if (!Game1.IsClient || Game1.locationRequest.Location?.Root != null)
						{
							Game1.currentLocation = mine;
							mine.resetForPlayerEntry();
							hasResetLocation = true;
						}
						Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);
						Game1.checkForRunButton(Game1.GetKeyboardState());
					}
					if (!Game1.eventUp)
					{
						Game1.player.Position = new Vector2(Game1.xLocationAfterWarp * 64, Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16);
					}
					if (!MineShaft.IsGeneratedLevel(Game1.locationRequest.Name) && Game1.locationRequest.Location != null)
					{
						Game1.currentLocation = Game1.locationRequest.Location;
						if (!Game1.IsClient)
						{
							Game1.locationRequest.Loaded(Game1.locationRequest.Location);
							Game1.currentLocation.resetForPlayerEntry();
							hasResetLocation = true;
						}
						Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);
						if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayWidth <= Game1.viewport.Width)
						{
							Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
						}
						if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayHeight <= Game1.viewport.Height)
						{
							Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
						}
						Game1.checkForRunButton(Game1.GetKeyboardState(), ignoreKeyPressQualifier: true);
					}
					if (!Game1.eventUp)
					{
						Game1.viewportFreeze = false;
					}
				}
				Game1.forceSnapOnNextViewportUpdate = true;
				Game1.player.FarmerSprite.PauseForSingleAnimation = false;
				Game1.player.faceDirection(Game1.facingDirectionAfterWarp);
				Game1._isWarping = false;
				if (Game1.player.ActiveObject != null)
				{
					Game1.player.showCarrying();
				}
				else
				{
					Game1.player.showNotCarrying();
				}
				if (Game1.IsClient)
				{
					if (Game1.locationRequest.Location != null && Game1.locationRequest.Location.Root != null && Game1.multiplayer.isActiveLocation(Game1.locationRequest.Location))
					{
						if (Game1.HasDedicatedHost)
						{
							Game1.notifyServerOfWarp(needsLocationInfo: false);
						}
						Game1.currentLocation = Game1.locationRequest.Location;
						Game1.locationRequest.Loaded(Game1.locationRequest.Location);
						if (!hasResetLocation)
						{
							Game1.currentLocation.resetForPlayerEntry();
						}
						Game1.player.currentLocation = Game1.currentLocation;
						Game1.locationRequest.Warped(Game1.currentLocation);
						Game1.currentLocation.updateSeasonalTileSheets();
						if (Game1.IsDebrisWeatherHere())
						{
							Game1.populateDebrisWeatherArray();
						}
						Game1.warpingForForcedRemoteEvent = false;
						Game1.locationRequest = null;
					}
					else
					{
						Game1.requestLocationInfoFromServer();
						if (Game1.currentLocation == null)
						{
							return true;
						}
					}
				}
				else
				{
					Game1.player.currentLocation = Game1.locationRequest.Location;
					Game1.locationRequest.Warped(Game1.locationRequest.Location);
					Game1.locationRequest = null;
				}
				if (Game1.locationRequest == null && Game1.currentLocation.Name == "Farm" && !Game1.eventUp)
				{
					if (Game1.player.position.X / 64f >= (float)(Game1.currentLocation.map.Layers[0].LayerWidth - 1))
					{
						Game1.player.position.X -= 64f;
					}
					else if (Game1.player.position.Y / 64f >= (float)(Game1.currentLocation.map.Layers[0].LayerHeight - 1))
					{
						Game1.player.position.Y -= 32f;
					}
					if (Game1.player.position.Y / 64f >= (float)(Game1.currentLocation.map.Layers[0].LayerHeight - 2))
					{
						Game1.player.position.X -= 48f;
					}
				}
				if (MineShaft.IsGeneratedLevel(previousLocation) && Game1.currentLocation != null && !MineShaft.IsGeneratedLevel(Game1.currentLocation))
				{
					MineShaft.OnLeftMines();
				}
				Game1.player.OnWarp();
				should_halt = true;
			}
		}
		if (Game1.newDay)
		{
			Game1.newDayAfterFade(AfterNewDay);
			return true;
		}
		if (Game1.eventOver)
		{
			Game1.eventFinished();
			if (Game1.dayOfMonth == 0)
			{
				Game1.newDayAfterFade(AfterEventOver);
			}
			return true;
		}
		if (Game1.currentSong?.Name == "rain" && Game1.currentLocation.IsRainingHere())
		{
			if (Game1.currentLocation.IsOutdoors)
			{
				Game1.currentSong.SetVariable("Frequency", 100f);
			}
			else if (!MineShaft.IsGeneratedLevel(Game1.currentLocation.Name))
			{
				Game1.currentSong.SetVariable("Frequency", 15f);
			}
		}
		return should_halt;
		static void AfterEventOver()
		{
			Game1.currentLocation.resetForPlayerEntry();
			Game1.nonWarpFade = false;
			Game1.fadeIn = false;
		}
		static void AfterNewDay()
		{
			if (Game1.eventOver)
			{
				Game1.eventFinished();
				if (Game1.dayOfMonth == 0)
				{
					Game1.newDayAfterFade(delegate
					{
						Game1.player.Position = new Vector2(320f, 320f);
					});
				}
			}
			Game1.nonWarpFade = false;
			Game1.fadeIn = false;
		}
	}

	/// <summary>Update game state when the current player finishes warping to a new location.</summary>
	/// <param name="oldLocation">The location which the player just left (or <c>null</c> for the first location after loading the save).</param>
	/// <param name="newLocation">The location which the player just arrived in.</param>
	public static void OnLocationChanged(GameLocation oldLocation, GameLocation newLocation)
	{
		if (!Game1.hasLoadedGame)
		{
			return;
		}
		Game1.eventsSeenSinceLastLocationChange.Clear();
		if (newLocation.Name != null && !MineShaft.IsGeneratedLevel(newLocation) && !VolcanoDungeon.IsGeneratedLevel(newLocation.Name))
		{
			Game1.player.locationsVisited.Add(newLocation.Name);
		}
		if (newLocation.IsOutdoors && !newLocation.ignoreDebrisWeather.Value && newLocation.IsDebrisWeatherHere() && Game1.GetSeasonForLocation(newLocation) != Game1.debrisWeatherSeason)
		{
			Game1.windGust = 0f;
			WeatherDebris.globalWind = 0f;
			Game1.populateDebrisWeatherArray();
			if (Game1.wind != null)
			{
				Game1.wind.Stop(AudioStopOptions.AsAuthored);
				Game1.wind = null;
			}
		}
		GameLocation.HandleMusicChange(oldLocation, newLocation);
		TriggerActionManager.Raise("LocationChanged");
	}

	private static void onFadedBackInComplete()
	{
		if (Game1.killScreen)
		{
			Game1.pauseThenMessage(1500, "..." + Game1.player.Name + "?");
		}
		else if (!Game1.eventUp)
		{
			Game1.player.CanMove = true;
		}
		Game1.checkForRunButton(Game1.oldKBState, ignoreKeyPressQualifier: true);
	}

	public static void UpdateOther(GameTime time)
	{
		if (Game1.currentLocation == null || (!Game1.player.passedOut && Game1.screenFade.UpdateFade(time)))
		{
			return;
		}
		if (Game1.dialogueUp)
		{
			Game1.player.CanMove = false;
		}
		for (int i = Game1.delayedActions.Count - 1; i >= 0; i--)
		{
			DelayedAction action = Game1.delayedActions[i];
			if (action.update(time) && Game1.delayedActions.Contains(action))
			{
				Game1.delayedActions.Remove(action);
			}
		}
		if (Game1.timeOfDay >= 2600 || Game1.player.stamina <= -15f)
		{
			if (Game1.currentMinigame != null && Game1.currentMinigame.forceQuit())
			{
				Game1.currentMinigame = null;
			}
			if (Game1.currentMinigame == null && Game1.player.canMove && Game1.player.freezePause <= 0 && !Game1.player.UsingTool && !Game1.eventUp && (Game1.IsMasterGame || Game1.player.isCustomized.Value) && Game1.locationRequest == null && Game1.activeClickableMenu == null)
			{
				Game1.player.startToPassOut();
				Game1.player.freezePause = 7000;
			}
		}
		Game1.screenOverlayTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		Game1.uiOverlayTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		if ((Game1.player.CanMove || Game1.player.UsingTool) && Game1.shouldTimePass())
		{
			Game1.buffsDisplay.update(time);
		}
		Game1.player.CurrentItem?.actionWhenBeingHeld(Game1.player);
		float tmp = Game1.dialogueButtonScale;
		Game1.dialogueButtonScale = (float)(16.0 * Math.Sin(time.TotalGameTime.TotalMilliseconds % 1570.0 / 500.0));
		if (tmp > Game1.dialogueButtonScale && !Game1.dialogueButtonShrinking)
		{
			Game1.dialogueButtonShrinking = true;
		}
		else if (tmp < Game1.dialogueButtonScale && Game1.dialogueButtonShrinking)
		{
			Game1.dialogueButtonShrinking = false;
		}
		if (Game1.screenGlow)
		{
			if (Game1.screenGlowUp || Game1.screenGlowHold)
			{
				if (Game1.screenGlowHold)
				{
					Game1.screenGlowAlpha = Math.Min(Game1.screenGlowAlpha + Game1.screenGlowRate, Game1.screenGlowMax);
				}
				else
				{
					Game1.screenGlowAlpha = Math.Min(Game1.screenGlowAlpha + 0.03f, 0.6f);
					if (Game1.screenGlowAlpha >= 0.6f)
					{
						Game1.screenGlowUp = false;
					}
				}
			}
			else
			{
				Game1.screenGlowAlpha -= 0.01f;
				if (Game1.screenGlowAlpha <= 0f)
				{
					Game1.screenGlow = false;
				}
			}
		}
		Game1.hudMessages.RemoveAll((HUDMessage hudMessage) => hudMessage.update(time));
		Game1.updateWeather(time);
		if (!Game1.fadeToBlack)
		{
			Game1.currentLocation.checkForMusic(time);
		}
		if (Game1.debrisSoundInterval > 0f)
		{
			Game1.debrisSoundInterval -= time.ElapsedGameTime.Milliseconds;
		}
		Game1.noteBlockTimer += time.ElapsedGameTime.Milliseconds;
		if (Game1.noteBlockTimer > 1000f)
		{
			Game1.noteBlockTimer = 0f;
			if (Game1.player.health < 20 && Game1.CurrentEvent == null)
			{
				Game1.hitShakeTimer = 250;
				if (Game1.player.health <= 10)
				{
					Game1.hitShakeTimer = 500;
					if (Game1.showingHealthBar && Game1.fadeToBlackAlpha <= 0f)
					{
						for (int i2 = 0; i2 < 3; i2++)
						{
							Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2(Game1.random.Next(32) + Game1.uiViewport.Width - 112, Game1.uiViewport.Height - 224 - (Game1.player.maxHealth - 100) - 16 + 4), flipped: false, 0.017f, Color.Red)
							{
								motion = new Vector2(-1.5f, -8 + Game1.random.Next(-1, 2)),
								acceleration = new Vector2(0f, 0.5f),
								local = true,
								scale = 4f,
								delayBeforeAnimationStart = i2 * 150
							});
						}
					}
				}
			}
		}
		Game1.drawLighting = (Game1.currentLocation.IsOutdoors && !Game1.outdoorLight.Equals(Color.White)) || !Game1.ambientLight.Equals(Color.White) || (Game1.currentLocation is MineShaft && !((MineShaft)Game1.currentLocation).getLightingColor(time).Equals(Color.White));
		if (Game1.player.hasBuff("26"))
		{
			Game1.drawLighting = true;
		}
		if (Game1.hitShakeTimer > 0)
		{
			Game1.hitShakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (Game1.staminaShakeTimer > 0)
		{
			Game1.staminaShakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		Game1.background?.update(Game1.viewport);
		Game1.cursorTileHintCheckTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		Game1.currentCursorTile.X = (Game1.viewport.X + Game1.getOldMouseX()) / 64;
		Game1.currentCursorTile.Y = (Game1.viewport.Y + Game1.getOldMouseY()) / 64;
		if (Game1.cursorTileHintCheckTimer <= 0 || !Game1.currentCursorTile.Equals(Game1.lastCursorTile))
		{
			Game1.cursorTileHintCheckTimer = 250;
			Game1.updateCursorTileHint();
			if (Game1.player.CanMove)
			{
				Game1.checkForRunButton(Game1.oldKBState, ignoreKeyPressQualifier: true);
			}
		}
		if (!MineShaft.IsGeneratedLevel(Game1.currentLocation.Name))
		{
			MineShaft.timeSinceLastMusic = 200000;
		}
		if (Game1.activeClickableMenu == null && Game1.farmEvent == null && Game1.keyboardDispatcher != null && !Game1.IsChatting)
		{
			Game1.keyboardDispatcher.Subscriber = null;
		}
	}

	public static void updateWeather(GameTime time)
	{
		if (Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsSnowingHere())
		{
			Game1.snowPos = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: Game1.snowPos, previous: Game1.previousViewportPosition, speed: -1f);
			return;
		}
		if (Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsRainingHere())
		{
			for (int i = 0; i < Game1.rainDrops.Length; i++)
			{
				if (Game1.rainDrops[i].frame == 0)
				{
					Game1.rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
					if (Game1.rainDrops[i].accumulator < 70)
					{
						continue;
					}
					Game1.rainDrops[i].position += new Vector2(-16 + i * 8 / Game1.rainDrops.Length, 32 - i * 8 / Game1.rainDrops.Length);
					Game1.rainDrops[i].accumulator = 0;
					if (Game1.random.NextDouble() < 0.1)
					{
						Game1.rainDrops[i].frame++;
					}
					if (Game1.currentLocation is IslandNorth || Game1.currentLocation is Caldera)
					{
						Point p = new Point((int)(Game1.rainDrops[i].position.X + (float)Game1.viewport.X) / 64, (int)(Game1.rainDrops[i].position.Y + (float)Game1.viewport.Y) / 64);
						p.Y--;
						if (Game1.currentLocation.isTileOnMap(p.X, p.Y) && !Game1.currentLocation.hasTileAt(p, "Back") && !Game1.currentLocation.hasTileAt(p, "Buildings"))
						{
							Game1.rainDrops[i].frame = 0;
						}
					}
					if (Game1.rainDrops[i].position.Y > (float)(Game1.viewport.Height + 64))
					{
						Game1.rainDrops[i].position.Y = -64f;
					}
					continue;
				}
				Game1.rainDrops[i].accumulator += time.ElapsedGameTime.Milliseconds;
				if (Game1.rainDrops[i].accumulator > 70)
				{
					Game1.rainDrops[i].frame = (Game1.rainDrops[i].frame + 1) % 4;
					Game1.rainDrops[i].accumulator = 0;
					if (Game1.rainDrops[i].frame == 0)
					{
						Game1.rainDrops[i].position = new Vector2(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height));
					}
				}
			}
			return;
		}
		if (Game1.currentLocation.IsOutdoors && !Game1.currentLocation.ignoreDebrisWeather.Value && Game1.currentLocation.IsDebrisWeatherHere())
		{
			if (Game1.currentLocation.GetSeason() == Season.Fall)
			{
				if (WeatherDebris.globalWind == 0f)
				{
					WeatherDebris.globalWind = -0.5f;
				}
				if (Game1.random.NextDouble() < 0.001 && Game1.windGust == 0f && WeatherDebris.globalWind >= -0.5f)
				{
					Game1.windGust += (float)Game1.random.Next(-10, -1) / 100f;
					Game1.playSound("wind", out Game1.wind);
				}
				else if (Game1.windGust != 0f)
				{
					Game1.windGust = Math.Max(-5f, Game1.windGust * 1.02f);
					WeatherDebris.globalWind = -0.5f + Game1.windGust;
					if (Game1.windGust < -0.2f && Game1.random.NextDouble() < 0.007)
					{
						Game1.windGust = 0f;
					}
				}
				if (WeatherDebris.globalWind < -0.5f)
				{
					WeatherDebris.globalWind = Math.Min(-0.5f, WeatherDebris.globalWind + 0.015f);
					if (Game1.wind != null)
					{
						Game1.wind.SetVariable("Volume", (0f - WeatherDebris.globalWind) * 20f);
						Game1.wind.SetVariable("Frequency", (0f - WeatherDebris.globalWind) * 20f);
						if (WeatherDebris.globalWind == -0.5f)
						{
							Game1.wind.Stop(AudioStopOptions.AsAuthored);
						}
					}
				}
			}
			else
			{
				if (WeatherDebris.globalWind == 0f)
				{
					WeatherDebris.globalWind = -0.25f;
				}
				if (Game1.wind != null)
				{
					Game1.wind.Stop(AudioStopOptions.AsAuthored);
					Game1.wind = null;
				}
			}
			{
				foreach (WeatherDebris item in Game1.debrisWeather)
				{
					item.update();
				}
				return;
			}
		}
		if (Game1.wind != null)
		{
			Game1.wind.Stop(AudioStopOptions.AsAuthored);
			Game1.wind = null;
		}
	}

	public static void updateCursorTileHint()
	{
		if (Game1.activeClickableMenu != null)
		{
			return;
		}
		Game1.mouseCursorTransparency = 1f;
		Game1.isActionAtCurrentCursorTile = false;
		Game1.isInspectionAtCurrentCursorTile = false;
		Game1.isSpeechAtCurrentCursorTile = false;
		int xTile = (Game1.viewport.X + Game1.getOldMouseX()) / 64;
		int yTile = (Game1.viewport.Y + Game1.getOldMouseY()) / 64;
		if (Game1.currentLocation != null)
		{
			Game1.isActionAtCurrentCursorTile = Game1.currentLocation.isActionableTile(xTile, yTile, Game1.player);
			if (!Game1.isActionAtCurrentCursorTile)
			{
				Game1.isActionAtCurrentCursorTile = Game1.currentLocation.isActionableTile(xTile, yTile + 1, Game1.player);
			}
		}
		Game1.lastCursorTile = Game1.currentCursorTile;
	}

	public static void updateMusic()
	{
		if (Game1.game1.IsMainInstance)
		{
			Game1 important_music_instance = null;
			string important_instance_music = null;
			int sub_location_priority = 1;
			int non_ambient_world_priority = 2;
			int minigame_priority = 5;
			int event_priority = 6;
			int mermaid_show = 7;
			int priority = 0;
			float default_context_priority = Game1.GetDefaultSongPriority(Game1.getMusicTrackName(), Game1.game1.instanceIsOverridingTrack, Game1.game1);
			MusicContext primary_music_context = MusicContext.Default;
			foreach (Game1 instance in GameRunner.instance.gameInstances)
			{
				MusicContext active_context = instance._instanceActiveMusicContext;
				if (instance.IsMainInstance)
				{
					primary_music_context = active_context;
				}
				string track_name = null;
				string actual_track_name = null;
				if (instance._instanceRequestedMusicTracks.TryGetValue(active_context, out var trackData))
				{
					track_name = trackData.Key;
				}
				if (instance.instanceIsOverridingTrack && instance.instanceCurrentSong != null)
				{
					actual_track_name = instance.instanceCurrentSong.Name;
				}
				switch (active_context)
				{
				case MusicContext.Event:
					if (priority < event_priority && track_name != null)
					{
						priority = event_priority;
						important_music_instance = instance;
						important_instance_music = track_name;
					}
					break;
				case MusicContext.MiniGame:
					if (priority < minigame_priority && track_name != null)
					{
						priority = minigame_priority;
						important_music_instance = instance;
						important_instance_music = track_name;
					}
					break;
				case MusicContext.SubLocation:
					if (priority < sub_location_priority && track_name != null)
					{
						priority = sub_location_priority;
						important_music_instance = instance;
						important_instance_music = ((actual_track_name == null) ? track_name : actual_track_name);
					}
					break;
				case MusicContext.Default:
					if (track_name == "mermaidSong")
					{
						priority = mermaid_show;
						important_music_instance = instance;
						important_instance_music = track_name;
					}
					if (primary_music_context <= active_context && track_name != null)
					{
						float instance_default_context_priority = Game1.GetDefaultSongPriority(track_name, instance.instanceIsOverridingTrack, instance);
						if (default_context_priority < instance_default_context_priority)
						{
							default_context_priority = instance_default_context_priority;
							priority = non_ambient_world_priority;
							important_music_instance = instance;
							important_instance_music = ((actual_track_name == null) ? track_name : actual_track_name);
						}
					}
					break;
				}
			}
			if (important_music_instance == null || important_music_instance == Game1.game1)
			{
				if (Game1.doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
				{
					Game1.stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
				}
			}
			else if (important_instance_music == null && Game1.doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
			{
				Game1.stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
			}
			else if (important_instance_music != null && Game1.getMusicTrackName(MusicContext.ImportantSplitScreenMusic) != important_instance_music)
			{
				Game1.changeMusicTrack(important_instance_music, track_interruptable: false, MusicContext.ImportantSplitScreenMusic);
			}
		}
		string song_to_play = null;
		bool track_overrideable = false;
		bool song_overridden = false;
		if (Game1.currentLocation != null && Game1.currentLocation.IsMiniJukeboxPlaying() && (!Game1.requestedMusicDirty || Game1.requestedMusicTrackOverrideable) && Game1.currentTrackOverrideable)
		{
			song_to_play = null;
			song_overridden = true;
			string mini_jukebox_track = Game1.currentLocation.miniJukeboxTrack.Value;
			if (mini_jukebox_track == "random")
			{
				mini_jukebox_track = ((Game1.currentLocation.randomMiniJukeboxTrack.Value != null) ? Game1.currentLocation.randomMiniJukeboxTrack.Value : "");
			}
			if (Game1.currentSong == null || !Game1.currentSong.IsPlaying || Game1.currentSong.Name != mini_jukebox_track)
			{
				if (!Game1.soundBank.Exists(mini_jukebox_track))
				{
					Game1.log.Error($"Location {Game1.currentLocation.NameOrUniqueName} has invalid jukebox track '{mini_jukebox_track}' selected, turning off jukebox.");
					Game1.player.currentLocation.miniJukeboxTrack.Value = "";
				}
				else
				{
					song_to_play = mini_jukebox_track;
					Game1.requestedMusicDirty = false;
					track_overrideable = true;
				}
			}
		}
		if (Game1.isOverridingTrack != song_overridden)
		{
			Game1.isOverridingTrack = song_overridden;
			if (!Game1.isOverridingTrack)
			{
				Game1.requestedMusicDirty = true;
			}
		}
		if (Game1.requestedMusicDirty)
		{
			song_to_play = Game1.requestedMusicTrack;
			track_overrideable = Game1.requestedMusicTrackOverrideable;
		}
		if (!string.IsNullOrEmpty(song_to_play))
		{
			Game1.musicPlayerVolume = Math.Max(0f, Math.Min(Game1.options.musicVolumeLevel, Game1.musicPlayerVolume - 0.01f));
			Game1.ambientPlayerVolume = Math.Max(0f, Math.Min(Game1.options.musicVolumeLevel, Game1.ambientPlayerVolume - 0.01f));
			if (Game1.game1.IsMainInstance)
			{
				Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
				Game1.ambientCategory.SetVolume(Game1.ambientPlayerVolume);
			}
			if (Game1.musicPlayerVolume != 0f || Game1.ambientPlayerVolume != 0f)
			{
				return;
			}
			if (song_to_play == "none" || song_to_play == "silence")
			{
				if (Game1.game1.IsMainInstance && Game1.currentSong != null)
				{
					Game1.currentSong.Stop(AudioStopOptions.Immediate);
					Game1.currentSong.Dispose();
					Game1.currentSong = null;
				}
			}
			else if ((Game1.options.musicVolumeLevel != 0f || Game1.options.ambientVolumeLevel != 0f) && (song_to_play != "rain" || Game1.endOfNightMenus.Count == 0))
			{
				if (Game1.game1.IsMainInstance && Game1.currentSong != null)
				{
					Game1.currentSong.Stop(AudioStopOptions.Immediate);
					Game1.currentSong.Dispose();
					Game1.currentSong = null;
				}
				Game1.currentSong = Game1.soundBank.GetCue(song_to_play);
				if (Game1.game1.IsMainInstance)
				{
					Game1.currentSong.Play();
				}
				if (Game1.game1.IsMainInstance && Game1.currentSong != null && Game1.currentSong.Name == "rain" && Game1.currentLocation != null)
				{
					if (Game1.IsRainingHere())
					{
						if (Game1.currentLocation.IsOutdoors)
						{
							Game1.currentSong.SetVariable("Frequency", 100f);
						}
						else if (!MineShaft.IsGeneratedLevel(Game1.currentLocation))
						{
							Game1.currentSong.SetVariable("Frequency", 15f);
						}
					}
					else if (Game1.eventUp)
					{
						Game1.currentSong.SetVariable("Frequency", 100f);
					}
				}
			}
			else
			{
				Game1.currentSong?.Stop(AudioStopOptions.Immediate);
			}
			Game1.currentTrackOverrideable = track_overrideable;
			Game1.requestedMusicDirty = false;
		}
		else if (Game1.MusicDuckTimer > 0f)
		{
			Game1.MusicDuckTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			Game1.musicPlayerVolume = Math.Max(Game1.musicPlayerVolume - Game1.options.musicVolumeLevel / 33f, Game1.options.musicVolumeLevel / 12f);
			if (Game1.game1.IsMainInstance)
			{
				Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
			}
		}
		else if (Game1.musicPlayerVolume < Game1.options.musicVolumeLevel || Game1.ambientPlayerVolume < Game1.options.ambientVolumeLevel)
		{
			if (Game1.musicPlayerVolume < Game1.options.musicVolumeLevel)
			{
				Game1.musicPlayerVolume = Math.Min(1f, Game1.musicPlayerVolume += 0.01f);
				if (Game1.game1.IsMainInstance)
				{
					Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
				}
			}
			if (Game1.ambientPlayerVolume < Game1.options.ambientVolumeLevel)
			{
				Game1.ambientPlayerVolume = Math.Min(1f, Game1.ambientPlayerVolume += 0.015f);
				if (Game1.game1.IsMainInstance)
				{
					Game1.ambientCategory.SetVolume(Game1.ambientPlayerVolume);
				}
			}
		}
		else if (Game1.currentSong != null && !Game1.currentSong.IsPlaying && !Game1.currentSong.IsStopped)
		{
			Game1.currentSong = Game1.soundBank.GetCue(Game1.currentSong.Name);
			if (Game1.game1.IsMainInstance)
			{
				Game1.currentSong.Play();
			}
		}
	}

	public static int GetDefaultSongPriority(string song_name, bool is_playing_override, Game1 instance)
	{
		if (is_playing_override)
		{
			return 9;
		}
		if (song_name == "none")
		{
			return 0;
		}
		if (instance._instanceIsPlayingOutdoorsAmbience || instance._instanceIsPlayingNightAmbience || song_name == "rain")
		{
			return 1;
		}
		if (instance._instanceIsPlayingMorningSong)
		{
			return 2;
		}
		if (instance._instanceIsPlayingTownMusic)
		{
			return 3;
		}
		if (song_name == "jungle_ambience")
		{
			return 7;
		}
		if (instance._instanceIsPlayingBackgroundMusic)
		{
			return 8;
		}
		if (instance.instanceGameLocation is MineShaft)
		{
			if (song_name.Contains("Ambient"))
			{
				return 7;
			}
			if (song_name.EndsWith("Mine"))
			{
				return 20;
			}
		}
		return 10;
	}

	public static void updateRainDropPositionForPlayerMovement(int direction, float speed)
	{
		if (Game1.currentLocation.IsRainingHere())
		{
			for (int i = 0; i < Game1.rainDrops.Length; i++)
			{
				switch (direction)
				{
				case 0:
					Game1.rainDrops[i].position.Y += speed;
					if (Game1.rainDrops[i].position.Y > (float)(Game1.viewport.Height + 64))
					{
						Game1.rainDrops[i].position.Y = -64f;
					}
					break;
				case 1:
					Game1.rainDrops[i].position.X -= speed;
					if (Game1.rainDrops[i].position.X < -64f)
					{
						Game1.rainDrops[i].position.X = Game1.viewport.Width;
					}
					break;
				case 2:
					Game1.rainDrops[i].position.Y -= speed;
					if (Game1.rainDrops[i].position.Y < -64f)
					{
						Game1.rainDrops[i].position.Y = Game1.viewport.Height;
					}
					break;
				case 3:
					Game1.rainDrops[i].position.X += speed;
					if (Game1.rainDrops[i].position.X > (float)(Game1.viewport.Width + 64))
					{
						Game1.rainDrops[i].position.X = -64f;
					}
					break;
				}
			}
		}
		else
		{
			Game1.updateDebrisWeatherForMovement(Game1.debrisWeather, direction, speed);
		}
	}

	public static void initializeVolumeLevels()
	{
		if (!LocalMultiplayer.IsLocalMultiplayer() || Game1.game1.IsMainInstance)
		{
			Game1.soundCategory.SetVolume(Game1.options.soundVolumeLevel);
			Game1.musicCategory.SetVolume(Game1.options.musicVolumeLevel);
			Game1.ambientCategory.SetVolume(Game1.options.ambientVolumeLevel);
			Game1.footstepCategory.SetVolume(Game1.options.footstepVolumeLevel);
		}
	}

	public static void updateDebrisWeatherForMovement(List<WeatherDebris> debris, int direction, float speed)
	{
		if (!(Game1.fadeToBlackAlpha <= 0f) || debris == null)
		{
			return;
		}
		foreach (WeatherDebris w in debris)
		{
			switch (direction)
			{
			case 0:
				w.position.Y += speed;
				if (w.position.Y > (float)(Game1.viewport.Height + 64))
				{
					w.position.Y = -64f;
				}
				break;
			case 1:
				w.position.X -= speed;
				if (w.position.X < -64f)
				{
					w.position.X = Game1.viewport.Width;
				}
				break;
			case 2:
				w.position.Y -= speed;
				if (w.position.Y < -64f)
				{
					w.position.Y = Game1.viewport.Height;
				}
				break;
			case 3:
				w.position.X += speed;
				if (w.position.X > (float)(Game1.viewport.Width + 64))
				{
					w.position.X = -64f;
				}
				break;
			}
		}
	}

	public static Vector2 updateFloatingObjectPositionForMovement(Vector2 w, Vector2 current, Vector2 previous, float speed)
	{
		if (current.Y < previous.Y)
		{
			w.Y -= Math.Abs(current.Y - previous.Y) * speed;
		}
		else if (current.Y > previous.Y)
		{
			w.Y += Math.Abs(current.Y - previous.Y) * speed;
		}
		if (current.X > previous.X)
		{
			w.X += Math.Abs(current.X - previous.X) * speed;
		}
		else if (current.X < previous.X)
		{
			w.X -= Math.Abs(current.X - previous.X) * speed;
		}
		return w;
	}

	public static void updateRaindropPosition()
	{
		if (Game1.HostPaused)
		{
			return;
		}
		if (Game1.IsRainingHere())
		{
			int xOffset = Game1.viewport.X - (int)Game1.previousViewportPosition.X;
			int yOffset = Game1.viewport.Y - (int)Game1.previousViewportPosition.Y;
			for (int i = 0; i < Game1.rainDrops.Length; i++)
			{
				Game1.rainDrops[i].position.X -= (float)xOffset * 1f;
				Game1.rainDrops[i].position.Y -= (float)yOffset * 1f;
				if (Game1.rainDrops[i].position.Y > (float)(Game1.viewport.Height + 64))
				{
					Game1.rainDrops[i].position.Y = -64f;
				}
				else if (Game1.rainDrops[i].position.X < -64f)
				{
					Game1.rainDrops[i].position.X = Game1.viewport.Width;
				}
				else if (Game1.rainDrops[i].position.Y < -64f)
				{
					Game1.rainDrops[i].position.Y = Game1.viewport.Height;
				}
				else if (Game1.rainDrops[i].position.X > (float)(Game1.viewport.Width + 64))
				{
					Game1.rainDrops[i].position.X = -64f;
				}
			}
		}
		else
		{
			Game1.updateDebrisWeatherForMovement(Game1.debrisWeather);
		}
	}

	public static void updateDebrisWeatherForMovement(List<WeatherDebris> debris)
	{
		if (Game1.HostPaused || debris == null || !(Game1.fadeToBlackAlpha < 1f))
		{
			return;
		}
		int xOffset = Game1.viewport.X - (int)Game1.previousViewportPosition.X;
		int yOffset = Game1.viewport.Y - (int)Game1.previousViewportPosition.Y;
		if (Math.Abs(xOffset) > 100 || Math.Abs(yOffset) > 80)
		{
			return;
		}
		int wrapBuffer = 16;
		foreach (WeatherDebris w in debris)
		{
			w.position.X -= (float)xOffset * 1f;
			w.position.Y -= (float)yOffset * 1f;
			if (w.position.Y > (float)(Game1.viewport.Height + 64 + wrapBuffer))
			{
				w.position.Y = -64f;
			}
			else if (w.position.X < (float)(-64 - wrapBuffer))
			{
				w.position.X = Game1.viewport.Width;
			}
			else if (w.position.Y < (float)(-64 - wrapBuffer))
			{
				w.position.Y = Game1.viewport.Height;
			}
			else if (w.position.X > (float)(Game1.viewport.Width + 64 + wrapBuffer))
			{
				w.position.X = -64f;
			}
		}
	}

	public static void randomizeRainPositions()
	{
		for (int i = 0; i < 70; i++)
		{
			Game1.rainDrops[i] = new RainDrop(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height), Game1.random.Next(4), Game1.random.Next(70));
		}
	}

	public static void randomizeDebrisWeatherPositions(List<WeatherDebris> debris)
	{
		if (debris == null)
		{
			return;
		}
		foreach (WeatherDebris debri in debris)
		{
			debri.position = Utility.getRandomPositionOnScreen();
		}
	}

	public static void eventFinished()
	{
		Game1.player.canOnlyWalk = false;
		if (Game1.player.bathingClothes.Value)
		{
			Game1.player.canOnlyWalk = true;
		}
		Game1.eventOver = false;
		Game1.eventUp = false;
		Game1.player.CanMove = true;
		Game1.displayHUD = true;
		Game1.player.faceDirection(Game1.player.orientationBeforeEvent);
		Game1.player.completelyStopAnimatingOrDoingAction();
		Game1.viewportFreeze = false;
		Action callback = null;
		if (Game1.currentLocation.currentEvent?.onEventFinished != null)
		{
			callback = Game1.currentLocation.currentEvent.onEventFinished;
			Game1.currentLocation.currentEvent.onEventFinished = null;
		}
		LocationRequest exitLocation = null;
		if (Game1.currentLocation.currentEvent != null)
		{
			exitLocation = Game1.currentLocation.currentEvent.exitLocation;
			Game1.currentLocation.currentEvent.cleanup();
			Game1.currentLocation.currentEvent = null;
		}
		if (Game1.player.ActiveObject != null)
		{
			Game1.player.showCarrying();
		}
		if (Game1.dayOfMonth != 0)
		{
			Game1.currentLightSources.Clear();
		}
		if (exitLocation == null && Game1.currentLocation != null && Game1.locationRequest == null)
		{
			exitLocation = new LocationRequest(Game1.currentLocation.NameOrUniqueName, Game1.currentLocation.isStructure.Value, Game1.currentLocation);
		}
		if (exitLocation != null)
		{
			if (exitLocation.Location is Farm && Game1.player.positionBeforeEvent.Y == 64f)
			{
				Game1.player.positionBeforeEvent.X += 1f;
			}
			exitLocation.OnWarp += delegate
			{
				Game1.player.locationBeforeForcedEvent.Value = null;
			};
			if (exitLocation.Location == Game1.currentLocation)
			{
				GameLocation.HandleMusicChange(Game1.currentLocation, Game1.currentLocation);
			}
			Game1.warpFarmer(exitLocation, (int)Game1.player.positionBeforeEvent.X, (int)Game1.player.positionBeforeEvent.Y, Game1.player.orientationBeforeEvent);
		}
		else
		{
			GameLocation.HandleMusicChange(Game1.currentLocation, Game1.currentLocation);
			Game1.player.setTileLocation(Game1.player.positionBeforeEvent);
			Game1.player.locationBeforeForcedEvent.Value = null;
		}
		Game1.nonWarpFade = false;
		Game1.fadeToBlackAlpha = 1f;
		callback?.Invoke();
	}

	public static void populateDebrisWeatherArray()
	{
		Season season = Game1.GetSeasonForLocation(Game1.currentLocation);
		int debrisToMake = Game1.random.Next(16, 64);
		int baseIndex = season switch
		{
			Season.Fall => 2, 
			Season.Winter => 3, 
			Season.Summer => 1, 
			_ => 0, 
		};
		Game1.isDebrisWeather = true;
		Game1.debrisWeatherSeason = season;
		Game1.debrisWeather.Clear();
		for (int i = 0; i < debrisToMake; i++)
		{
			Game1.debrisWeather.Add(new WeatherDebris(new Vector2(Game1.random.Next(0, Game1.viewport.Width), Game1.random.Next(0, Game1.viewport.Height)), baseIndex, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
		}
	}

	private static void OnNewSeason()
	{
		Game1.setGraphicsForSeason();
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.seasonUpdate();
			return true;
		});
	}

	public static void prepareSpouseForWedding(Farmer farmer)
	{
		NPC nPC = Game1.RequireCharacter(farmer.spouse);
		nPC.ClearSchedule();
		nPC.DefaultMap = farmer.homeLocation.Value;
		nPC.DefaultPosition = Utility.PointToVector2(Game1.RequireLocation<FarmHouse>(farmer.homeLocation.Value).getSpouseBedSpot(farmer.spouse)) * 64f;
		nPC.DefaultFacingDirection = 2;
	}

	public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
	{
		if (!NPC.TryGetData(characterId, out var data))
		{
			return false;
		}
		bool characterAdded = false;
		if (Game1.getCharacterFromName(characterId) == null)
		{
			if (!bypassConditions && !GameStateQuery.CheckConditions(data.UnlockConditions))
			{
				return false;
			}
			NPC.ReadNpcHomeData(data, null, out var homeName, out var homeTile, out var direction);
			bool datable = data.CanBeRomanced;
			Point size = data.Size;
			GameLocation homeLocation = Game1.getLocationFromNameInLocationsList(homeName);
			if (homeLocation == null)
			{
				return false;
			}
			string characterTextureName = NPC.getTextureNameForCharacter(characterId);
			NPC character;
			try
			{
				character = new NPC(new AnimatedSprite("Characters\\" + characterTextureName, 0, size.X, size.Y), new Vector2(homeTile.X * 64, homeTile.Y * 64), homeName, direction, characterId, datable, Game1.content.Load<Texture2D>("Portraits\\" + characterTextureName));
			}
			catch (Exception exception)
			{
				Game1.log.Error("Failed to spawn NPC '" + characterId + "'.", exception);
				return false;
			}
			character.Breather = data.Breather;
			homeLocation.addCharacter(character);
			characterAdded = true;
		}
		if (data.SocialTab == SocialTabBehavior.AlwaysShown && !Game1.player.friendshipData.ContainsKey(characterId))
		{
			Game1.player.friendshipData.Add(characterId, new Friendship());
		}
		return characterAdded;
	}

	public static GameLocation CreateGameLocation(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return null;
		}
		LocationData locationData;
		CreateLocationData createData = (Game1.locationData.TryGetValue(id, out locationData) ? locationData.CreateOnLoad : null);
		return Game1.CreateGameLocation(id, createData);
	}

	public static GameLocation CreateGameLocation(string id, CreateLocationData createData)
	{
		if (createData == null)
		{
			return null;
		}
		GameLocation location = ((createData.Type == null) ? new GameLocation(createData.MapPath, id) : ((GameLocation)Activator.CreateInstance(Type.GetType(createData.Type) ?? throw new Exception("Invalid type for location " + id + ": " + createData.Type), createData.MapPath, id)));
		location.isAlwaysActive.Value = createData.AlwaysActive;
		return location;
	}

	public static void AddLocations()
	{
		bool currentLocationSet = false;
		foreach (KeyValuePair<string, LocationData> pair in Game1.locationData)
		{
			if (pair.Value.CreateOnLoad == null)
			{
				continue;
			}
			GameLocation location;
			try
			{
				location = Game1.CreateGameLocation(pair.Key, pair.Value.CreateOnLoad);
			}
			catch (Exception exception)
			{
				Game1.log.Error("Couldn't create the '" + pair.Key + "' location. Is its data in Data/Locations invalid?", exception);
				continue;
			}
			if (location == null)
			{
				Game1.log.Error("Couldn't create the '" + pair.Key + "' location. Is its data in Data/Locations invalid?");
				continue;
			}
			if (!currentLocationSet)
			{
				try
				{
					location.map.LoadTileSheets(Game1.mapDisplayDevice);
					Game1.currentLocation = location;
					currentLocationSet = true;
				}
				catch (Exception exception2)
				{
					Game1.log.Error("Couldn't load tilesheets for the '" + pair.Key + "' location.", exception2);
				}
			}
			Game1.locations.Add(location);
		}
		for (int i = 1; i < Game1.netWorldState.Value.HighestPlayerLimit; i++)
		{
			GameLocation cellar = Game1.CreateGameLocation("Cellar");
			cellar.name.Value += i + 1;
			Game1.locations.Add(cellar);
		}
	}

	public static void AddNPCs()
	{
		foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
		{
			if (entry.Value.SpawnIfMissing)
			{
				Game1.AddCharacterIfNecessary(entry.Key);
			}
		}
		GameLocation location = Game1.getLocationFromNameInLocationsList("QiNutRoom");
		if (location.getCharacterFromName("Mister Qi") == null)
		{
			AnimatedSprite sprite = new AnimatedSprite("Characters\\MrQi", 0, 16, 32);
			location.addCharacter(new NPC(sprite, new Vector2(448f, 256f), "QiNutRoom", 0, "Mister Qi", datable: false, Game1.content.Load<Texture2D>("Portraits\\MrQi")));
		}
	}

	public static void AddModNPCs()
	{
	}

	public static void fixProblems()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.LearnDefaultRecipes();
			allFarmer.AddMissedMailAndRecipes();
			LevelUpMenu.RevalidateHealth(allFarmer);
			LevelUpMenu.AddMissedProfessionChoices(allFarmer);
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.characters.RemoveWhere(delegate(NPC npc)
			{
				if (npc == null)
				{
					Game1.log.Warn("Removed broken NPC in " + location.NameOrUniqueName + ": null instance.");
					return true;
				}
				if (npc.IsVillager && npc.GetData() == null)
				{
					try
					{
						if (npc.Sprite.Texture == null)
						{
							Game1.log.Warn($"Removed broken NPC '{npc.Name}' in {location.NameOrUniqueName}: villager with no data or sprites.");
							return true;
						}
					}
					catch
					{
						Game1.log.Warn($"Removed broken NPC '{npc.Name}' in {location.NameOrUniqueName}: villager with no data or sprites.");
						return true;
					}
				}
				return false;
			});
			return true;
		});
		Game1.AddNPCs();
		List<NPC> divorced = null;
		Utility.ForEachVillager(delegate(NPC nPC)
		{
			if (!nPC.datable.Value || nPC.getSpouse() != null)
			{
				return true;
			}
			if (nPC.DefaultMap == null || !nPC.DefaultMap.ContainsIgnoreCase("cabin") || nPC.DefaultMap != "FarmHouse")
			{
				return true;
			}
			CharacterData data = nPC.GetData();
			if (data == null)
			{
				return true;
			}
			NPC.ReadNpcHomeData(data, nPC.currentLocation, out var locationName, out var _, out var _);
			if (nPC.DefaultMap != locationName)
			{
				if (divorced == null)
				{
					divorced = new List<NPC>();
				}
				divorced.Add(nPC);
			}
			return true;
		});
		if (divorced != null)
		{
			foreach (NPC n in divorced)
			{
				Game1.log.Warn("Fixing " + n.Name + " who was improperly divorced and left stranded");
				n.PerformDivorce();
			}
		}
		foreach (Farmer player in Game1.getAllFarmers())
		{
			if (player.hasQuest("130"))
			{
				HashSet<string> requiredQuestItems = new HashSet<string> { "(O)864", "(O)865", "(O)866", "(O)867", "(O)868", "(O)869", "(O)870" };
				bool found = false;
				foreach (string itemId in requiredQuestItems)
				{
					if (player.Items.ContainsId(itemId))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					Utility.ForEachItem(delegate(Item item2)
					{
						found = requiredQuestItems.Contains(item2.QualifiedItemId);
						return !found;
					});
				}
				if (!found)
				{
					Object item = ItemRegistry.Create<Object>("(O)864");
					item.specialItem = true;
					item.questItem.Value = true;
					if (!player.addItemToInventoryBool(item))
					{
						player.team.returnedDonations.Add(item);
						player.team.newLostAndFoundItems.Value = true;
					}
				}
			}
			else if (!player.craftingRecipes.ContainsKey("Fairy Dust") && player.mailReceived.Contains("birdieQuestBegun"))
			{
				player.mailReceived.Remove("birdieQuestBegun");
			}
		}
		int playerCount = Game1.getAllFarmers().Count();
		Dictionary<Type, int> missingTools = new Dictionary<Type, int>
		{
			[typeof(Axe)] = playerCount,
			[typeof(Pickaxe)] = playerCount,
			[typeof(Hoe)] = playerCount,
			[typeof(WateringCan)] = playerCount,
			[typeof(Wand)] = 0
		};
		foreach (Farmer allFarmer2 in Game1.getAllFarmers())
		{
			if (allFarmer2.hasOrWillReceiveMail("ReturnScepter"))
			{
				missingTools[typeof(Wand)]++;
			}
		}
		int missingScythes = playerCount;
		foreach (Farmer who in Game1.getAllFarmers())
		{
			if (who.toolBeingUpgraded.Value != null)
			{
				who.toolBeingUpgraded.Value.FixStackSize();
				Type key = who.toolBeingUpgraded.Value.GetType();
				if (missingTools.TryGetValue(key, out var count))
				{
					missingTools[key] = count - 1;
				}
			}
			for (int i = 0; i < who.Items.Count; i++)
			{
				if (who.Items[i] != null)
				{
					Game1.checkIsMissingTool(missingTools, ref missingScythes, who.Items[i]);
				}
			}
		}
		bool allFound = true;
		foreach (int value in missingTools.Values)
		{
			if (value > 0)
			{
				allFound = false;
				break;
			}
		}
		if (missingScythes > 0)
		{
			allFound = false;
		}
		if (allFound)
		{
			return;
		}
		Utility.ForEachLocation(delegate(GameLocation l)
		{
			List<Debris> list = new List<Debris>();
			foreach (Debris current2 in l.debris)
			{
				Item item2 = current2.item;
				if (item2 != null)
				{
					foreach (Type current3 in missingTools.Keys)
					{
						if (item2.GetType() == current3)
						{
							list.Add(current2);
						}
					}
					if (item2.QualifiedItemId == "(W)47")
					{
						list.Add(current2);
					}
				}
			}
			foreach (Debris current4 in list)
			{
				l.debris.Remove(current4);
			}
			return true;
		});
		Utility.iterateChestsAndStorage(delegate(Item item2)
		{
			Game1.checkIsMissingTool(missingTools, ref missingScythes, item2);
		});
		List<string> toAdd = new List<string>();
		foreach (KeyValuePair<Type, int> pair in missingTools)
		{
			if (pair.Value > 0)
			{
				for (int i2 = 0; i2 < pair.Value; i2++)
				{
					toAdd.Add(pair.Key.ToString());
				}
			}
		}
		for (int i3 = 0; i3 < missingScythes; i3++)
		{
			toAdd.Add("Scythe");
		}
		if (toAdd.Count > 0)
		{
			Game1.addMailForTomorrow("foundLostTools");
		}
		for (int i4 = 0; i4 < toAdd.Count; i4++)
		{
			Item tool = null;
			switch (toAdd[i4])
			{
			case "StardewValley.Tools.Axe":
				tool = ItemRegistry.Create("(T)Axe");
				break;
			case "StardewValley.Tools.Hoe":
				tool = ItemRegistry.Create("(T)Hoe");
				break;
			case "StardewValley.Tools.WateringCan":
				tool = ItemRegistry.Create("(T)WateringCan");
				break;
			case "Scythe":
				tool = ItemRegistry.Create("(W)47");
				break;
			case "StardewValley.Tools.Pickaxe":
				tool = ItemRegistry.Create("(T)Pickaxe");
				break;
			case "StardewValley.Tools.Wand":
				tool = ItemRegistry.Create("(T)ReturnScepter");
				break;
			}
			if (tool != null)
			{
				if (Game1.newDaySync.hasInstance())
				{
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
				Game1.player.team.returnedDonations.Add(tool);
			}
		}
	}

	private static void checkIsMissingTool(Dictionary<Type, int> missingTools, ref int missingScythes, Item item)
	{
		foreach (Type key in missingTools.Keys)
		{
			if (item.GetType() == key)
			{
				missingTools[key]--;
			}
		}
		if (item.QualifiedItemId == "(W)47")
		{
			missingScythes--;
		}
	}

	public static void newDayAfterFade(Action after)
	{
		if (Game1.player.currentLocation != null)
		{
			if (Game1.player.rightRing.Value != null)
			{
				Game1.player.rightRing.Value.onLeaveLocation(Game1.player, Game1.player.currentLocation);
			}
			if (Game1.player.leftRing.Value != null)
			{
				Game1.player.leftRing.Value.onLeaveLocation(Game1.player, Game1.player.currentLocation);
			}
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			Game1.hooks.OnGame1_NewDayAfterFade(delegate
			{
				Game1.game1.isLocalMultiplayerNewDayActive = true;
				Game1._afterNewDayAction = after;
				GameRunner.instance.activeNewDayProcesses.Add(new KeyValuePair<Game1, IEnumerator<int>>(Game1.game1, Game1._newDayAfterFade()));
			});
			return;
		}
		Game1.hooks.OnGame1_NewDayAfterFade(delegate
		{
			Game1._afterNewDayAction = after;
			if (Game1._newDayTask != null)
			{
				Game1.log.Warn("Warning: There is already a _newDayTask; unusual code path.\n" + StackTraceHelper.StackTrace);
			}
			else
			{
				Game1._newDayTask = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					IEnumerator<int> enumerator = Game1._newDayAfterFade();
					while (enumerator.MoveNext())
					{
					}
				});
			}
		});
	}

	public static bool CanAcceptDailyQuest()
	{
		if (Game1.questOfTheDay == null)
		{
			return false;
		}
		if (Game1.player.acceptedDailyQuest.Value)
		{
			return false;
		}
		if (string.IsNullOrEmpty(Game1.questOfTheDay.questDescription))
		{
			return false;
		}
		return true;
	}

	private static IEnumerator<int> _newDayAfterFade()
	{
		TriggerActionManager.Raise("DayEnding");
		Game1.newDaySync.start();
		while (!Game1.newDaySync.hasStarted())
		{
			yield return 0;
		}
		int timeWentToSleep = Game1.timeOfDay;
		Game1.newDaySync.barrier("start");
		while (!Game1.newDaySync.isBarrierReady("start"))
		{
			yield return 0;
		}
		int overnightMinutesElapsed = Utility.CalculateMinutesUntilMorning(timeWentToSleep);
		Game1.stats.AverageBedtime = (uint)timeWentToSleep;
		if (Game1.IsMasterGame)
		{
			Game1.dayOfMonth++;
			Game1.stats.DaysPlayed++;
			if (Game1.dayOfMonth > 28)
			{
				Game1.dayOfMonth = 1;
				switch (Game1.season)
				{
				case Season.Spring:
					Game1.season = Season.Summer;
					break;
				case Season.Summer:
					Game1.season = Season.Fall;
					break;
				case Season.Fall:
					Game1.season = Season.Winter;
					break;
				case Season.Winter:
					Game1.season = Season.Spring;
					Game1.year++;
					MineShaft.yearUpdate();
					break;
				}
			}
			Game1.timeOfDay = 600;
			Game1.netWorldState.Value.UpdateFromGame1();
		}
		Game1.newDaySync.barrier("date");
		while (!Game1.newDaySync.isBarrierReady("date"))
		{
			yield return 0;
		}
		Game1.player.dayOfMonthForSaveGame = Game1.dayOfMonth;
		Game1.player.seasonForSaveGame = Game1.seasonIndex;
		Game1.player.yearForSaveGame = Game1.year;
		Game1.flushLocationLookup();
		Event.OnNewDay();
		try
		{
			Game1.fixProblems();
		}
		catch (Exception)
		{
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.FarmerSprite.PauseForSingleAnimation = false;
		}
		Game1.whereIsTodaysFest = null;
		if (Game1.wind != null)
		{
			Game1.wind.Stop(AudioStopOptions.Immediate);
			Game1.wind = null;
		}
		Game1.player.chestConsumedMineLevels.RemoveWhere((KeyValuePair<int, bool> keyValuePair) => keyValuePair.Key > 120);
		Game1.player.currentEyes = 0;
		int seed;
		if (Game1.IsMasterGame)
		{
			Game1.player.team.announcedSleepingFarmers.Clear();
			seed = Utility.CreateRandomSeed(Game1.uniqueIDForThisGame / 100, Game1.stats.DaysPlayed * 10 + 1, Game1.stats.StepsTaken);
			Game1.newDaySync.sendVar<NetInt, int>("seed", seed);
		}
		else
		{
			while (!Game1.newDaySync.isVarReady("seed"))
			{
				yield return 0;
			}
			seed = Game1.newDaySync.waitForVar<NetInt, int>("seed");
		}
		Game1.random = Utility.CreateRandom(seed);
		for (int i = 0; i < Game1.dayOfMonth; i++)
		{
			Game1.random.Next();
		}
		Game1.player.team.endOfNightStatus.UpdateState("sleep");
		Game1.newDaySync.barrier("sleep");
		while (!Game1.newDaySync.isBarrierReady("sleep"))
		{
			yield return 0;
		}
		Game1.gameTimeInterval = 0;
		Game1.game1.wasAskedLeoMemory = false;
		Game1.player.team.Update();
		Game1.player.team.NewDay();
		Game1.player.passedOut = false;
		Game1.player.CanMove = true;
		Game1.player.FarmerSprite.PauseForSingleAnimation = false;
		Game1.player.FarmerSprite.StopAnimation();
		Game1.player.completelyStopAnimatingOrDoingAction();
		Game1.changeMusicTrack("silence");
		if (Game1.IsMasterGame)
		{
			Game1.UpdateDishOfTheDay();
		}
		Game1.newDaySync.barrier("dishOfTheDay");
		while (!Game1.newDaySync.isBarrierReady("dishOfTheDay"))
		{
			yield return 0;
		}
		Game1.npcDialogues = null;
		Utility.ForEachCharacter(delegate(NPC nPC)
		{
			nPC.updatedDialogueYet = false;
			return true;
		});
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			location.currentEvent = null;
			if (Game1.IsMasterGame)
			{
				location.passTimeForObjects(overnightMinutesElapsed);
			}
			return true;
		});
		Game1.outdoorLight = Color.White;
		Game1.ambientLight = Color.White;
		if (Game1.isLightning && Game1.IsMasterGame)
		{
			Utility.overnightLightning(timeWentToSleep);
		}
		if (Game1.MasterPlayer.hasOrWillReceiveMail("ccBulletinThankYou") && !Game1.player.hasOrWillReceiveMail("ccBulletinThankYou"))
		{
			Game1.addMailForTomorrow("ccBulletinThankYou");
		}
		Game1.ReceiveMailForTomorrow();
		if (Utility.TryGetRandom(Game1.player.friendshipData, out var whichFriend, out var friendship) && Game1.random.NextBool((double)(friendship.Points / 250) * 0.1) && Game1.player.spouse != whichFriend && DataLoader.Mail(Game1.content).ContainsKey(whichFriend))
		{
			Game1.mailbox.Add(whichFriend);
		}
		MineShaft.clearActiveMines();
		VolcanoDungeon.ClearAllLevels();
		Game1.netWorldState.Value.CheckedGarbage.Clear();
		for (int i2 = Game1.player.enchantments.Count - 1; i2 >= 0; i2--)
		{
			Game1.player.enchantments[i2].OnUnequip(Game1.player);
		}
		Game1.player.dayupdate(timeWentToSleep);
		if (Game1.IsMasterGame)
		{
			Game1.player.team.sharedDailyLuck.Value = Math.Min(0.10000000149011612, (double)Game1.random.Next(-100, 101) / 1000.0);
		}
		Game1.player.showToolUpgradeAvailability();
		if (Game1.IsMasterGame)
		{
			Game1.queueWeddingsForToday();
			Game1.newDaySync.sendVar<NetRef<NetLongList>, NetLongList>("weddingsToday", new NetLongList(Game1.weddingsToday));
		}
		else
		{
			while (!Game1.newDaySync.isVarReady("weddingsToday"))
			{
				yield return 0;
			}
			Game1.weddingsToday = new List<long>(Game1.newDaySync.waitForVar<NetRef<NetLongList>, NetLongList>("weddingsToday"));
		}
		Game1.weddingToday = false;
		foreach (long item4 in Game1.weddingsToday)
		{
			Farmer spouse_farmer = Game1.GetPlayer(item4);
			if (spouse_farmer != null && !spouse_farmer.hasCurrentOrPendingRoommate())
			{
				Game1.weddingToday = true;
				break;
			}
		}
		if (Game1.player.spouse != null && Game1.player.isEngaged() && Game1.weddingsToday.Contains(Game1.player.UniqueMultiplayerID))
		{
			Friendship friendship2 = Game1.player.friendshipData[Game1.player.spouse];
			friendship2.Status = FriendshipStatus.Married;
			friendship2.WeddingDate = new WorldDate(Game1.Date);
			Game1.prepareSpouseForWedding(Game1.player);
			if (!Game1.player.getSpouse().isRoommate())
			{
				Game1.player.autoGenerateActiveDialogueEvent("married_" + Game1.player.spouse);
				if (!Game1.player.autoGenerateActiveDialogueEvent("married"))
				{
					Game1.player.autoGenerateActiveDialogueEvent("married_twice");
				}
			}
			else
			{
				Game1.player.autoGenerateActiveDialogueEvent("roommates_" + Game1.player.spouse);
			}
		}
		NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>> additional_shipped_items = new NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>();
		if (Game1.IsMasterGame)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (Object value in location.objects.Values)
				{
					if (value is Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } chest)
					{
						chest.clearNulls();
						if (Game1.player.team.useSeparateWallets.Value)
						{
							foreach (long current2 in chest.separateWalletItems.Keys)
							{
								if (!additional_shipped_items.ContainsKey(current2))
								{
									additional_shipped_items[current2] = new NetList<Item, NetRef<Item>>();
								}
								List<Item> list = new List<Item>(chest.separateWalletItems[current2]);
								chest.separateWalletItems[current2].Clear();
								foreach (Item current3 in list)
								{
									current3.onDetachedFromParent();
									additional_shipped_items[current2].Add(current3);
								}
							}
						}
						else
						{
							IInventory shippingBin2 = Game1.getFarm().getShippingBin(Game1.player);
							shippingBin2.RemoveEmptySlots();
							foreach (Item current4 in chest.Items)
							{
								current4.onDetachedFromParent();
								shippingBin2.Add(current4);
							}
						}
						chest.Items.Clear();
						chest.separateWalletItems.Clear();
					}
				}
				return true;
			});
		}
		if (Game1.IsMasterGame)
		{
			Game1.newDaySync.sendVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items", additional_shipped_items);
		}
		else
		{
			while (!Game1.newDaySync.isVarReady("additional_shipped_items"))
			{
				yield return 0;
			}
			additional_shipped_items = Game1.newDaySync.waitForVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items");
		}
		if (Game1.player.team.useSeparateWallets.Value)
		{
			IInventory shipping_bin = Game1.getFarm().getShippingBin(Game1.player);
			if (additional_shipped_items.TryGetValue(Game1.player.UniqueMultiplayerID, out var item_list))
			{
				foreach (Item item in item_list)
				{
					shipping_bin.Add(item);
				}
			}
		}
		Game1.newDaySync.barrier("handleMiniShippingBins");
		while (!Game1.newDaySync.isBarrierReady("handleMiniShippingBins"))
		{
			yield return 0;
		}
		IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);
		shippingBin.RemoveEmptySlots();
		foreach (Item i3 in shippingBin)
		{
			Game1.player.displayedShippedItems.Add(i3);
		}
		if (Game1.player.useSeparateWallets || Game1.player.IsMainPlayer)
		{
			int total = 0;
			foreach (Item item2 in shippingBin)
			{
				int item_value = 0;
				if (item2 is Object obj)
				{
					item_value = obj.sellToStorePrice(-1L) * obj.Stack;
					total += item_value;
				}
				if (Game1.player.team.specialOrders == null)
				{
					continue;
				}
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					specialOrder.onItemShipped?.Invoke(Game1.player, item2, item_value);
				}
			}
			Game1.player.Money += total;
		}
		if (Game1.IsMasterGame)
		{
			if (Game1.IsWinter && Game1.dayOfMonth == 18)
			{
				GameLocation source = Game1.RequireLocation("Submarine");
				if (source.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2(20f, 20f), Game1.getLocationFromName("Beach"));
				}
				source = Game1.RequireLocation("MermaidHouse");
				if (source.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2(21f, 20f), Game1.getLocationFromName("Beach"));
				}
			}
			if (Game1.player.hasOrWillReceiveMail("pamHouseUpgrade") && !Game1.player.hasOrWillReceiveMail("transferredObjectsPamHouse"))
			{
				Game1.addMailForTomorrow("transferredObjectsPamHouse", noLetter: true);
				GameLocation source2 = Game1.RequireLocation("Trailer");
				GameLocation destination = Game1.getLocationFromName("Trailer_Big");
				if (source2.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source2, destination, new Vector2(14f, 23f));
				}
			}
			if (Utility.HasAnyPlayerSeenEvent("191393") && !Game1.player.hasOrWillReceiveMail("transferredObjectsJojaMart"))
			{
				Game1.addMailForTomorrow("transferredObjectsJojaMart", noLetter: true);
				GameLocation source3 = Game1.RequireLocation("JojaMart");
				if (source3.objects.Length >= 0)
				{
					Utility.transferPlacedObjectsFromOneLocationToAnother(source3, null, new Vector2(89f, 51f), Game1.getLocationFromName("Town"));
				}
			}
		}
		if (Game1.player.useSeparateWallets && Game1.player.IsMainPlayer)
		{
			foreach (Farmer who in Game1.getOfflineFarmhands())
			{
				if (who.isUnclaimedFarmhand)
				{
					continue;
				}
				int total2 = 0;
				IInventory farmhandShippingBin = Game1.getFarm().getShippingBin(who);
				farmhandShippingBin.RemoveEmptySlots();
				foreach (Item item3 in farmhandShippingBin)
				{
					int item_value2 = 0;
					if (item3 is Object obj2)
					{
						item_value2 = obj2.sellToStorePrice(who.UniqueMultiplayerID) * obj2.Stack;
						total2 += item_value2;
					}
					if (Game1.player.team.specialOrders == null)
					{
						continue;
					}
					foreach (SpecialOrder specialOrder2 in Game1.player.team.specialOrders)
					{
						specialOrder2.onItemShipped?.Invoke(Game1.player, item3, item_value2);
					}
				}
				Game1.player.team.AddIndividualMoney(who, total2);
				farmhandShippingBin.Clear();
			}
		}
		List<NPC> divorceNPCs = new List<NPC>();
		if (Game1.IsMasterGame)
		{
			foreach (Farmer who2 in Game1.getAllFarmers())
			{
				if (who2.isActive() && who2.divorceTonight.Value && who2.getSpouse() != null)
				{
					divorceNPCs.Add(who2.getSpouse());
				}
			}
		}
		Game1.newDaySync.barrier("player.dayupdate");
		while (!Game1.newDaySync.isBarrierReady("player.dayupdate"))
		{
			yield return 0;
		}
		if (Game1.player.divorceTonight.Value)
		{
			Game1.player.doDivorce();
		}
		Game1.newDaySync.barrier("player.divorce");
		while (!Game1.newDaySync.isBarrierReady("player.divorce"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			foreach (NPC npc in divorceNPCs)
			{
				if (npc.getSpouse() == null)
				{
					npc.PerformDivorce();
				}
			}
		}
		Game1.newDaySync.barrier("player.finishDivorce");
		while (!Game1.newDaySync.isBarrierReady("player.finishDivorce"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building?.GetIndoors() is Cabin cabin)
				{
					cabin.updateFarmLayout();
				}
				return true;
			});
		}
		Game1.newDaySync.barrier("updateFarmLayout");
		while (!Game1.newDaySync.isBarrierReady("updateFarmLayout"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame && Game1.player.changeWalletTypeTonight.Value)
		{
			if (Game1.player.useSeparateWallets)
			{
				ManorHouse.MergeWallets();
			}
			else
			{
				ManorHouse.SeparateWallets();
			}
		}
		Game1.newDaySync.barrier("player.wallets");
		while (!Game1.newDaySync.isBarrierReady("player.wallets"))
		{
			yield return 0;
		}
		Game1.getFarm().lastItemShipped = null;
		Game1.getFarm().getShippingBin(Game1.player).Clear();
		Game1.newDaySync.barrier("clearShipping");
		while (!Game1.newDaySync.isBarrierReady("clearShipping"))
		{
			yield return 0;
		}
		if (Game1.IsClient)
		{
			Game1.multiplayer.sendFarmhand();
			Game1.newDaySync.processMessages();
		}
		Game1.newDaySync.barrier("sendFarmhands");
		while (!Game1.newDaySync.isBarrierReady("sendFarmhands"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			Game1.multiplayer.saveFarmhands();
		}
		Game1.newDaySync.barrier("saveFarmhands");
		while (!Game1.newDaySync.isBarrierReady("saveFarmhands"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			Game1.UpdatePassiveFestivalStates();
			if (Utility.IsPassiveFestivalDay("NightMarket") && Game1.IsMasterGame && Game1.netWorldState.Value.VisitsUntilY1Guarantee >= 0)
			{
				Game1.netWorldState.Value.VisitsUntilY1Guarantee--;
			}
		}
		if (Game1.dayOfMonth == 1)
		{
			Game1.OnNewSeason();
		}
		if (Game1.IsMasterGame && (Game1.dayOfMonth == 1 || Game1.dayOfMonth == 8 || Game1.dayOfMonth == 15 || Game1.dayOfMonth == 22))
		{
			SpecialOrder.UpdateAvailableSpecialOrders("", forceRefresh: true);
			SpecialOrder.UpdateAvailableSpecialOrders("Qi", forceRefresh: true);
		}
		if (Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.UpdateFromGame1();
		}
		Game1.newDaySync.barrier("specialOrders");
		while (!Game1.newDaySync.isBarrierReady("specialOrders"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			Game1.player.team.specialOrders.RemoveWhere(delegate(SpecialOrder specialOrder)
			{
				if (specialOrder.questState.Value != SpecialOrderStatus.Complete && specialOrder.GetDaysLeft() <= 0)
				{
					specialOrder.OnFail();
					return true;
				}
				return false;
			});
		}
		Game1.newDaySync.barrier("processOrders");
		while (!Game1.newDaySync.isBarrierReady("processOrders"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			foreach (string item5 in Game1.player.team.specialRulesRemovedToday)
			{
				SpecialOrder.RemoveSpecialRuleAtEndOfDay(item5);
			}
		}
		Game1.player.team.specialRulesRemovedToday.Clear();
		if (DataLoader.Mail(Game1.content).ContainsKey(Game1.currentSeason + "_" + Game1.dayOfMonth + "_" + Game1.year))
		{
			Game1.mailbox.Add(Game1.currentSeason + "_" + Game1.dayOfMonth + "_" + Game1.year);
		}
		else if (DataLoader.Mail(Game1.content).ContainsKey(Game1.currentSeason + "_" + Game1.dayOfMonth))
		{
			Game1.mailbox.Add(Game1.currentSeason + "_" + Game1.dayOfMonth);
		}
		if (Game1.MasterPlayer.mailReceived.Contains("ccVault") && Game1.IsSpring && Game1.dayOfMonth == 14)
		{
			Game1.mailbox.Add("DesertFestival");
		}
		if (Game1.IsMasterGame)
		{
			if (Game1.player.team.toggleMineShrineOvernight.Value)
			{
				Game1.player.team.toggleMineShrineOvernight.Value = false;
				Game1.player.team.mineShrineActivated.Value = !Game1.player.team.mineShrineActivated.Value;
				if (Game1.player.team.mineShrineActivated.Value)
				{
					Game1.netWorldState.Value.MinesDifficulty++;
				}
				else
				{
					Game1.netWorldState.Value.MinesDifficulty--;
				}
			}
			if (Game1.player.team.toggleSkullShrineOvernight.Value)
			{
				Game1.player.team.toggleSkullShrineOvernight.Value = false;
				Game1.player.team.skullShrineActivated.Value = !Game1.player.team.skullShrineActivated.Value;
				if (Game1.player.team.skullShrineActivated.Value)
				{
					Game1.netWorldState.Value.SkullCavesDifficulty++;
				}
				else
				{
					Game1.netWorldState.Value.SkullCavesDifficulty--;
				}
			}
		}
		if (Game1.IsMasterGame)
		{
			if (!Game1.player.team.SpecialOrderRuleActive("MINE_HARD") && Game1.netWorldState.Value.MinesDifficulty > 1)
			{
				Game1.netWorldState.Value.MinesDifficulty = 1;
			}
			if (!Game1.player.team.SpecialOrderRuleActive("SC_HARD") && Game1.netWorldState.Value.SkullCavesDifficulty > 1)
			{
				Game1.netWorldState.Value.SkullCavesDifficulty = 1;
			}
		}
		if (Game1.IsMasterGame)
		{
			Game1.RefreshQuestOfTheDay();
		}
		Game1.newDaySync.barrier("questOfTheDay");
		while (!Game1.newDaySync.isBarrierReady("questOfTheDay"))
		{
			yield return 0;
		}
		bool yesterdayWasGreenRain = Game1.wasGreenRain;
		Game1.wasGreenRain = false;
		Game1.UpdateWeatherForNewDay();
		Game1.newDaySync.barrier("updateWeather");
		while (!Game1.newDaySync.isBarrierReady("updateWeather"))
		{
			yield return 0;
		}
		Game1.ApplyWeatherForNewDay();
		if (Game1.isGreenRain)
		{
			Game1.morningQueue.Enqueue(delegate
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:greenrainmessage"));
			});
			if (Game1.year == 1 && !Game1.player.hasOrWillReceiveMail("GreenRainGus"))
			{
				Game1.mailbox.Add("GreenRainGus");
			}
			if (Game1.IsMasterGame)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					location.performGreenRainUpdate();
					return true;
				});
			}
		}
		else if (yesterdayWasGreenRain)
		{
			if (Game1.IsMasterGame)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					location.performDayAfterGreenRainUpdate();
					return true;
				});
			}
			if (Game1.year == 1)
			{
				Game1.player.activeDialogueEvents.TryAdd("GreenRainFinished", 1);
			}
		}
		if (Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
		{
			Game1.addMorningFluffFunction(delegate
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:BooksellerInTown"));
			});
		}
		WeatherDebris.globalWind = 0f;
		Game1.windGust = 0f;
		Game1.AddNPCs();
		Utility.ForEachVillager(delegate(NPC nPC)
		{
			Game1.player.mailReceived.Remove(nPC.Name);
			Game1.player.mailReceived.Remove(nPC.Name + "Cooking");
			nPC.drawOffset = Vector2.Zero;
			if (!Game1.IsMasterGame)
			{
				nPC.ChooseAppearance();
			}
			return true;
		});
		FarmAnimal.reservedGrass.Clear();
		if (Game1.IsMasterGame)
		{
			NPC.hasSomeoneRepairedTheFences = false;
			NPC.hasSomeoneFedTheAnimals = false;
			NPC.hasSomeoneFedThePet = false;
			NPC.hasSomeoneWateredCrops = false;
			foreach (GameLocation location in Game1.locations)
			{
				location.ResetCharacterDialogues();
				location.DayUpdate(Game1.dayOfMonth);
			}
			Game1.netWorldState.Value.UpdateUnderConstruction();
			Game1.UpdateHorseOwnership();
			foreach (NPC n in Utility.getAllCharacters())
			{
				if (n.IsVillager)
				{
					n.islandScheduleName.Value = null;
					n.currentScheduleDelay = 0f;
				}
				n.dayUpdate(Game1.dayOfMonth);
			}
			IslandSouth.SetupIslandSchedules();
			HashSet<NPC> purchased_item_npcs = new HashSet<NPC>();
			Game1.UpdateShopPlayerItemInventory("SeedShop", purchased_item_npcs);
			Game1.UpdateShopPlayerItemInventory("FishShop", purchased_item_npcs);
		}
		if (Game1.IsMasterGame && Game1.netWorldState.Value.GetWeatherForLocation("Island").IsRaining)
		{
			Vector2 tile_location = new Vector2(0f, 0f);
			IslandLocation island_location = null;
			List<int> order = new List<int>();
			for (int i4 = 0; i4 < 4; i4++)
			{
				order.Add(i4);
			}
			Utility.Shuffle(Utility.CreateRandom(Game1.uniqueIDForThisGame), order);
			switch (order[Game1.currentGemBirdIndex])
			{
			case 0:
				island_location = Game1.getLocationFromName("IslandSouth") as IslandLocation;
				tile_location = new Vector2(10f, 30f);
				break;
			case 1:
				island_location = Game1.getLocationFromName("IslandNorth") as IslandLocation;
				tile_location = new Vector2(56f, 56f);
				break;
			case 2:
				island_location = Game1.getLocationFromName("Islandwest") as IslandLocation;
				tile_location = new Vector2(53f, 51f);
				break;
			case 3:
				island_location = Game1.getLocationFromName("IslandEast") as IslandLocation;
				tile_location = new Vector2(21f, 35f);
				break;
			}
			Game1.currentGemBirdIndex = (Game1.currentGemBirdIndex + 1) % 4;
			if (island_location != null)
			{
				island_location.locationGemBird.Value = new IslandGemBird(tile_location, IslandGemBird.GetBirdTypeForLocation(island_location.Name));
			}
		}
		if (Game1.IsMasterGame)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				if (location.IsOutdoors && location.IsRainingHere())
				{
					foreach (Building building in location.buildings)
					{
						if (building is PetBowl petBowl)
						{
							petBowl.watered.Value = true;
						}
					}
					foreach (KeyValuePair<Vector2, TerrainFeature> pair2 in location.terrainFeatures.Pairs)
					{
						if (pair2.Value is HoeDirt hoeDirt && hoeDirt.state.Value != 2)
						{
							hoeDirt.state.Value = 1;
						}
					}
				}
				return true;
			});
		}
		WorldDate yesterday = new WorldDate(Game1.Date);
		yesterday.TotalDays--;
		foreach (KeyValuePair<string, PassiveFestivalData> pair in DataLoader.PassiveFestivals(Game1.content))
		{
			string id = pair.Key;
			PassiveFestivalData festival = pair.Value;
			if (yesterday.DayOfMonth == festival.EndDay && yesterday.Season == festival.Season && GameStateQuery.CheckConditions(festival.Condition) && festival != null && festival.CleanupMethod != null)
			{
				if (StaticDelegateBuilder.TryCreateDelegate<FestivalCleanupDelegate>(festival.CleanupMethod, out var method, out var error))
				{
					method();
					continue;
				}
				Game1.log.Warn($"Passive festival '{id}' has invalid cleanup method '{festival.CleanupMethod}': {error}");
			}
		}
		Game1.PerformPassiveFestivalSetup();
		Game1.newDaySync.barrier("buildingUpgrades");
		while (!Game1.newDaySync.isBarrierReady("buildingUpgrades"))
		{
			yield return 0;
		}
		List<string> mailToRemoveOvernight = new List<string>(Game1.player.team.mailToRemoveOvernight);
		foreach (string index in new List<string>(Game1.player.team.itemsToRemoveOvernight))
		{
			if (Game1.IsMasterGame)
			{
				Game1.game1._PerformRemoveNormalItemFromWorldOvernight(index);
				foreach (Farmer farmer in Game1.getOfflineFarmhands())
				{
					Game1.game1._PerformRemoveNormalItemFromFarmerOvernight(farmer, index);
				}
			}
			Game1.game1._PerformRemoveNormalItemFromFarmerOvernight(Game1.player, index);
		}
		foreach (string mail_key in mailToRemoveOvernight)
		{
			if (Game1.IsMasterGame)
			{
				foreach (Farmer farmer2 in Game1.getAllFarmers())
				{
					farmer2.RemoveMail(mail_key, farmer2 == Game1.MasterPlayer);
				}
			}
			else
			{
				Game1.player.RemoveMail(mail_key);
			}
		}
		Game1.newDaySync.barrier("removeItemsFromWorld");
		while (!Game1.newDaySync.isBarrierReady("removeItemsFromWorld"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			Game1.player.team.itemsToRemoveOvernight.Clear();
			Game1.player.team.mailToRemoveOvernight.Clear();
		}
		Game1.newDay = false;
		if (Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.UpdateFromGame1();
		}
		if (Game1.player.currentLocation != null)
		{
			Game1.player.currentLocation.resetForPlayerEntry();
			BedFurniture.ApplyWakeUpPosition(Game1.player);
			Game1.forceSnapOnNextViewportUpdate = true;
			Game1.UpdateViewPort(overrideFreeze: false, Game1.player.StandingPixel);
			Game1.previousViewportPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
		}
		Game1.displayFarmer = true;
		Game1.updateWeatherIcon();
		Game1.freezeControls = false;
		if (Game1.stats.DaysPlayed > 1 || !Game1.IsMasterGame)
		{
			Game1.farmEvent = null;
			if (Game1.IsMasterGame)
			{
				Game1.farmEvent = Utility.pickFarmEvent() ?? Game1.farmEventOverride;
				Game1.farmEventOverride = null;
				Game1.newDaySync.sendVar<NetRef<FarmEvent>, FarmEvent>("farmEvent", Game1.farmEvent);
			}
			else
			{
				while (!Game1.newDaySync.isVarReady("farmEvent"))
				{
					yield return 0;
				}
				Game1.farmEvent = Game1.newDaySync.waitForVar<NetRef<FarmEvent>, FarmEvent>("farmEvent");
			}
			if (Game1.farmEvent == null)
			{
				Game1.farmEvent = Utility.pickPersonalFarmEvent();
			}
			if (Game1.farmEvent != null && Game1.farmEvent.setUp())
			{
				Game1.farmEvent = null;
			}
		}
		if (Game1.farmEvent == null)
		{
			Game1.RemoveDeliveredMailForTomorrow();
		}
		if (Game1.player.team.newLostAndFoundItems.Value)
		{
			Game1.morningQueue.Enqueue(delegate
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
			});
		}
		Game1.newDaySync.barrier("mail");
		while (!Game1.newDaySync.isBarrierReady("mail"))
		{
			yield return 0;
		}
		if (Game1.IsMasterGame)
		{
			Game1.player.team.newLostAndFoundItems.Value = false;
		}
		Utility.ForEachBuilding(delegate(Building building)
		{
			if (building.GetIndoors() is Cabin)
			{
				Game1.player.slotCanHost = true;
				return false;
			}
			return true;
		});
		if (Utility.percentGameComplete() + (float)Game1.netWorldState.Value.PerfectionWaivers * 0.01f >= 1f)
		{
			Game1.player.team.farmPerfect.Value = true;
		}
		Game1.newDaySync.barrier("checkcompletion");
		while (!Game1.newDaySync.isBarrierReady("checkcompletion"))
		{
			yield return 0;
		}
		Game1.UpdateFarmPerfection();
		if (Game1.farmEvent == null)
		{
			Game1.handlePostFarmEventActions();
			Game1.showEndOfNightStuff();
		}
		if (Game1.server != null)
		{
			Game1.server.updateLobbyData();
		}
	}

	/// <summary>Reset the Saloon's dish of the day.</summary>
	public static void UpdateDishOfTheDay()
	{
		string itemId;
		do
		{
			itemId = Game1.random.Next(194, 240).ToString();
		}
		while (Utility.IsForbiddenDishOfTheDay(itemId));
		int count = Game1.random.Next(1, 4 + ((Game1.random.NextDouble() < 0.08) ? 10 : 0));
		Game1.dishOfTheDay = ItemRegistry.Create<Object>("(O)" + itemId, count);
	}

	/// <summary>Apply updates overnight if this save has completed perfection.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Utility.percentGameComplete" /> to check if the save has reached perfection.</remarks>
	public static void UpdateFarmPerfection()
	{
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") || (!Game1.MasterPlayer.hasCompletedCommunityCenter() && !Utility.hasFinishedJojaRoute()) || !Game1.player.team.farmPerfect.Value)
		{
			return;
		}
		Game1.addMorningFluffFunction(delegate
		{
			Game1.changeMusicTrack("none", track_interruptable: true);
			if (Game1.IsMasterGame)
			{
				Game1.multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal1");
			}
			Game1.playSound("discoverMineral");
			if (Game1.IsMasterGame)
			{
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal2", Game1.MasterPlayer.farmName.Value);
				}, 4000);
			}
			Game1.player.mailReceived.Add("Farm_Eternal");
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playSound("thunder_small");
				if (Game1.IsMultiplayer)
				{
					if (Game1.IsMasterGame)
					{
						Game1.multiplayer.globalChatInfoMessage("Eternal3");
					}
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:Chat_Eternal3"));
				}
			}, 12000);
		});
	}

	/// <summary>Get whether it's green raining in the given location's context (regardless of whether the player is currently indoors and sheltered from the green rain).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsGreenRainingHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		if (location != null && Game1.netWorldState != null)
		{
			return location.IsGreenRainingHere();
		}
		return false;
	}

	/// <summary>Get whether it's raining in the given location's context (regardless of whether the player is currently indoors and sheltered from the rain).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsRainingHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		if (location != null && Game1.netWorldState != null)
		{
			return location.IsRainingHere();
		}
		return false;
	}

	/// <summary>Get whether it's storming in the given location's context (regardless of whether the player is currently indoors and sheltered from the storm).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsLightningHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		if (location != null && Game1.netWorldState != null)
		{
			return location.IsLightningHere();
		}
		return false;
	}

	/// <summary>Get whether it's snowing in the given location's context (regardless of whether the player is currently indoors and sheltered from the snow).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsSnowingHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		if (location != null && Game1.netWorldState != null)
		{
			return location.IsSnowingHere();
		}
		return false;
	}

	/// <summary>Get whether it's blowing debris like leaves in the given location's context (regardless of whether the player is currently indoors and sheltered from the wind).</summary>
	/// <param name="location">The location to check, or <c>null</c> to use <see cref="P:StardewValley.Game1.currentLocation" />.</param>
	public static bool IsDebrisWeatherHere(GameLocation location = null)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		if (location != null && Game1.netWorldState != null)
		{
			return location.IsDebrisWeatherHere();
		}
		return false;
	}

	public static string getWeatherModificationsForDate(WorldDate date, string default_weather)
	{
		string weather = default_weather;
		int day_offset = date.TotalDays - Game1.Date.TotalDays;
		if (date.DayOfMonth == 1 || Game1.stats.DaysPlayed + day_offset <= 4)
		{
			weather = "Sun";
		}
		if (Game1.stats.DaysPlayed + day_offset == 3)
		{
			weather = "Rain";
		}
		if (Utility.isGreenRainDay(date.DayOfMonth, date.Season))
		{
			weather = "GreenRain";
		}
		if (date.Season == Season.Summer && date.DayOfMonth % 13 == 0)
		{
			weather = "Storm";
		}
		if (Utility.isFestivalDay(date.DayOfMonth, date.Season))
		{
			weather = "Festival";
		}
		foreach (PassiveFestivalData festival in DataLoader.PassiveFestivals(Game1.content).Values)
		{
			if (date.DayOfMonth < festival.StartDay || date.DayOfMonth > festival.EndDay || date.Season != festival.Season || !GameStateQuery.CheckConditions(festival.Condition) || festival.MapReplacements == null)
			{
				continue;
			}
			foreach (string key in festival.MapReplacements.Keys)
			{
				GameLocation replacedLocation = Game1.getLocationFromName(key);
				if (replacedLocation != null && replacedLocation.InValleyContext())
				{
					weather = "Sun";
					break;
				}
			}
		}
		return weather;
	}

	public static void UpdateWeatherForNewDay()
	{
		Game1.weatherForTomorrow = Game1.getWeatherModificationsForDate(Game1.Date, Game1.weatherForTomorrow);
		if (Game1.weddingToday)
		{
			Game1.weatherForTomorrow = "Wedding";
		}
		if (Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.GetWeatherForLocation("Default").WeatherForTomorrow = Game1.weatherForTomorrow;
		}
		Game1.wasRainingYesterday = Game1.isRaining || Game1.isLightning;
		Game1.debrisWeather.Clear();
		if (!Game1.IsMasterGame)
		{
			return;
		}
		foreach (KeyValuePair<string, LocationContextData> pair in Game1.locationContextData)
		{
			Game1.netWorldState.Value.GetWeatherForLocation(pair.Key).UpdateDailyWeather(pair.Key, pair.Value, Game1.random);
		}
		foreach (KeyValuePair<string, LocationContextData> pair2 in Game1.locationContextData)
		{
			string contextToCopy = pair2.Value.CopyWeatherFromLocation;
			if (contextToCopy != null)
			{
				try
				{
					LocationWeather weatherForLocation = Game1.netWorldState.Value.GetWeatherForLocation(pair2.Key);
					LocationWeather otherLocationWeather = Game1.netWorldState.Value.GetWeatherForLocation(contextToCopy);
					weatherForLocation.CopyFrom(otherLocationWeather);
				}
				catch
				{
				}
			}
		}
	}

	public static void ApplyWeatherForNewDay()
	{
		LocationWeather weatherForLocation = Game1.netWorldState.Value.GetWeatherForLocation("Default");
		Game1.weatherForTomorrow = weatherForLocation.WeatherForTomorrow;
		Game1.isRaining = weatherForLocation.IsRaining;
		Game1.isSnowing = weatherForLocation.IsSnowing;
		Game1.isLightning = weatherForLocation.IsLightning;
		Game1.isDebrisWeather = weatherForLocation.IsDebrisWeather;
		Game1.isGreenRain = weatherForLocation.IsGreenRain;
		if (Game1.isDebrisWeather)
		{
			Game1.populateDebrisWeatherArray();
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		foreach (string key in Game1.netWorldState.Value.LocationWeather.Keys)
		{
			LocationWeather locationWeather = Game1.netWorldState.Value.LocationWeather[key];
			if (Game1.dayOfMonth == 1)
			{
				locationWeather.monthlyNonRainyDayCount.Value = 0;
			}
			if (!locationWeather.IsRaining)
			{
				locationWeather.monthlyNonRainyDayCount.Value++;
			}
		}
	}

	public static void UpdateShopPlayerItemInventory(string location_name, HashSet<NPC> purchased_item_npcs)
	{
		if (!(Game1.getLocationFromName(location_name) is ShopLocation { itemsFromPlayerToSell: var items } shopLocation))
		{
			return;
		}
		for (int i = items.Count - 1; i >= 0; i--)
		{
			if (!(items[i] is Object item))
			{
				items.RemoveAt(i);
			}
			else
			{
				for (int j = 0; j < item.Stack; j++)
				{
					bool soldItem = false;
					if (item.edibility.Value != -300 && Game1.random.NextDouble() < 0.04)
					{
						NPC n = Utility.GetRandomNpc((string name, CharacterData data) => data.CanCommentOnPurchasedShopItems ?? (data.HomeRegion == "Town"));
						if (n.Age != 2 && n.getSpouse() == null)
						{
							if (!purchased_item_npcs.Contains(n))
							{
								Dialogue dialogue = shopLocation.getPurchasedItemDialogueForNPC(item, n);
								if (dialogue != null)
								{
									n.addExtraDialogue(dialogue);
									purchased_item_npcs.Add(n);
								}
							}
							items[i] = item.ConsumeStack(1);
							soldItem = true;
						}
					}
					if (!soldItem && Game1.random.NextDouble() < 0.15)
					{
						items[i] = item.ConsumeStack(1);
					}
					if (items[i] == null)
					{
						items.RemoveAt(i);
						break;
					}
				}
			}
		}
	}

	private static void handlePostFarmEventActions()
	{
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			foreach (Action postFarmEventOvernightAction in location.postFarmEventOvernightActions)
			{
				postFarmEventOvernightAction();
			}
			location.postFarmEventOvernightActions.Clear();
			return true;
		});
		if (Game1.IsMasterGame)
		{
			Mountain mountain = Game1.RequireLocation<Mountain>("Mountain");
			mountain.ApplyTreehouseIfNecessary();
			if (mountain.treehouseDoorDirty)
			{
				mountain.treehouseDoorDirty = false;
				WarpPathfindingCache.PopulateCache();
			}
		}
	}

	public static void ReceiveMailForTomorrow(string mail_to_transfer = null)
	{
		foreach (string s in Game1.player.mailForTomorrow)
		{
			if (s == null)
			{
				continue;
			}
			string stripped = s.Replace("%&NL&%", "");
			if (mail_to_transfer == null || !(mail_to_transfer != s) || !(mail_to_transfer != stripped))
			{
				Game1.mailDeliveredFromMailForTomorrow.Add(s);
				if (s.Contains("%&NL&%"))
				{
					Game1.player.mailReceived.Add(stripped);
				}
				else
				{
					Game1.mailbox.Add(s);
				}
			}
		}
	}

	public static void RemoveDeliveredMailForTomorrow()
	{
		Game1.ReceiveMailForTomorrow("abandonedJojaMartAccessible");
		foreach (string s in Game1.mailDeliveredFromMailForTomorrow)
		{
			Game1.player.mailForTomorrow.Remove(s);
		}
		Game1.mailDeliveredFromMailForTomorrow.Clear();
	}

	public static void queueWeddingsForToday()
	{
		Game1.weddingsToday.Clear();
		Game1.weddingToday = false;
		if (!Game1.canHaveWeddingOnDay(Game1.dayOfMonth, Game1.season))
		{
			return;
		}
		foreach (Farmer farmer in from farmer2 in Game1.getOnlineFarmers()
			orderby farmer2.UniqueMultiplayerID
			select farmer2)
		{
			if (farmer.spouse != null && farmer.isEngaged() && farmer.friendshipData[farmer.spouse].CountdownToWedding < 1)
			{
				Game1.weddingsToday.Add(farmer.UniqueMultiplayerID);
			}
			if (!farmer.team.IsEngaged(farmer.UniqueMultiplayerID))
			{
				continue;
			}
			long? spouse = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
			if (spouse.HasValue && !Game1.weddingsToday.Contains(spouse.Value))
			{
				Farmer spouse_farmer = Game1.GetPlayer(spouse.Value);
				if (spouse_farmer != null && Game1.getOnlineFarmers().Contains(spouse_farmer) && Game1.getOnlineFarmers().Contains(farmer) && Game1.player.team.GetFriendship(farmer.UniqueMultiplayerID, spouse.Value).CountdownToWedding < 1)
				{
					Game1.weddingsToday.Add(farmer.UniqueMultiplayerID);
				}
			}
		}
	}

	public static bool PollForEndOfNewDaySync()
	{
		if (!Game1.IsMultiplayer)
		{
			Game1.newDaySync.destroy();
			Game1.currentLocation.resetForPlayerEntry();
			return true;
		}
		if (Game1.newDaySync.readyForFinish())
		{
			if (Game1.IsMasterGame && Game1.newDaySync.hasInstance() && !Game1.newDaySync.hasFinished())
			{
				Game1.newDaySync.finish();
			}
			if (Game1.IsClient)
			{
				Game1.player.sleptInTemporaryBed.Value = false;
			}
			if (Game1.newDaySync.hasInstance() && Game1.newDaySync.hasFinished())
			{
				Game1.newDaySync.destroy();
				Game1.currentLocation.resetForPlayerEntry();
				return true;
			}
		}
		return false;
	}

	public static void updateWeatherIcon()
	{
		if (Game1.IsSnowingHere())
		{
			Game1.weatherIcon = 7;
		}
		else if (Game1.IsRainingHere())
		{
			Game1.weatherIcon = 4;
		}
		else if (Game1.IsDebrisWeatherHere() && Game1.IsSpring)
		{
			Game1.weatherIcon = 3;
		}
		else if (Game1.IsDebrisWeatherHere() && Game1.IsFall)
		{
			Game1.weatherIcon = 6;
		}
		else if (Game1.IsDebrisWeatherHere() && Game1.IsWinter)
		{
			Game1.weatherIcon = 7;
		}
		else if (Game1.weddingToday)
		{
			Game1.weatherIcon = 0;
		}
		else
		{
			Game1.weatherIcon = 2;
		}
		if (Game1.IsLightningHere())
		{
			Game1.weatherIcon = 5;
		}
		if (Utility.isFestivalDay())
		{
			Game1.weatherIcon = 1;
		}
		if (Game1.IsGreenRainingHere())
		{
			Game1.weatherIcon = 999;
		}
	}

	public static void showEndOfNightStuff()
	{
		Game1.hooks.OnGame1_ShowEndOfNightStuff(delegate
		{
			if (!Game1.IsDedicatedHost)
			{
				bool flag = false;
				if (Game1.player.displayedShippedItems.Count > 0)
				{
					Game1.endOfNightMenus.Push(new ShippingMenu(Game1.player.displayedShippedItems));
					Game1.player.displayedShippedItems.Clear();
					flag = true;
				}
				bool flag2 = false;
				if (Game1.player.newLevels.Count > 0 && !flag)
				{
					Game1.endOfNightMenus.Push(new SaveGameMenu());
				}
				for (int num = Game1.player.newLevels.Count - 1; num >= 0; num--)
				{
					Game1.endOfNightMenus.Push(new LevelUpMenu(Game1.player.newLevels[num].X, Game1.player.newLevels[num].Y));
					flag2 = true;
				}
				if (Game1.player.farmingLevel.Value == 10 && Game1.player.miningLevel.Value == 10 && Game1.player.fishingLevel.Value == 10 && Game1.player.foragingLevel.Value == 10 && Game1.player.combatLevel.Value == 10 && Game1.player.mailReceived.Add("gotMasteryHint") && !Game1.player.locationsVisited.Contains("MasteryCave"))
				{
					Game1.morningQueue.Enqueue(delegate
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryHint"));
					});
				}
				if (flag2)
				{
					Game1.playSound("newRecord");
				}
				if (Game1.client != null && Game1.client.timedOut)
				{
					return;
				}
			}
			if (Game1.endOfNightMenus.Count > 0)
			{
				Game1.showingEndOfNightStuff = true;
				Game1.activeClickableMenu = Game1.endOfNightMenus.Pop();
			}
			else
			{
				Game1.showingEndOfNightStuff = true;
				Game1.activeClickableMenu = new SaveGameMenu();
			}
		});
	}

	/// <summary>Update the game state when the season changes. Despite the name, this may update more than graphics (e.g. it'll remove grass in winter).</summary>
	/// <param name="onLoad">Whether the season is being initialized as part of loading the save, instead of an actual in-game season change.</param>
	public static void setGraphicsForSeason(bool onLoad = false)
	{
		foreach (GameLocation l in Game1.locations)
		{
			Season season = l.GetSeason();
			l.seasonUpdate(onLoad);
			l.updateSeasonalTileSheets();
			if (!l.IsOutdoors)
			{
				continue;
			}
			switch (season)
			{
			case Season.Spring:
				Game1.eveningColor = new Color(255, 255, 0);
				break;
			case Season.Summer:
				foreach (Object o3 in l.Objects.Values)
				{
					if (!o3.IsWeeds())
					{
						continue;
					}
					switch (o3.QualifiedItemId)
					{
					case "(O)792":
						o3.SetIdAndSprite(o3.ParentSheetIndex + 1);
						continue;
					case "(O)882":
					case "(O)883":
					case "(O)884":
						continue;
					}
					if (Game1.random.NextDouble() < 0.3)
					{
						o3.SetIdAndSprite(676);
					}
					else if (Game1.random.NextDouble() < 0.3)
					{
						o3.SetIdAndSprite(677);
					}
				}
				Game1.eveningColor = new Color(255, 255, 0);
				break;
			case Season.Fall:
				foreach (Object o2 in l.Objects.Values)
				{
					if (o2.IsWeeds())
					{
						switch (o2.QualifiedItemId)
						{
						case "(O)793":
							o2.SetIdAndSprite(o2.ParentSheetIndex + 1);
							break;
						default:
							o2.SetIdAndSprite(Game1.random.Choose(678, 679));
							break;
						case "(O)882":
						case "(O)883":
						case "(O)884":
							break;
						}
					}
				}
				Game1.eveningColor = new Color(255, 255, 0);
				foreach (WeatherDebris item in Game1.debrisWeather)
				{
					item.which = 2;
				}
				break;
			case Season.Winter:
			{
				KeyValuePair<Vector2, Object>[] array = l.Objects.Pairs.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<Vector2, Object> pair = array[i];
					Object o = pair.Value;
					if (o.IsWeeds())
					{
						switch (o.QualifiedItemId)
						{
						case "(O)882":
						case "(O)883":
						case "(O)884":
							continue;
						}
						l.Objects.Remove(pair.Key);
					}
				}
				foreach (WeatherDebris item2 in Game1.debrisWeather)
				{
					item2.which = 3;
				}
				Game1.eveningColor = new Color(245, 225, 170);
				break;
			}
			}
		}
	}

	public static void pauseThenMessage(int millisecondsPause, string message)
	{
		Game1.messageAfterPause = message;
		Game1.pauseTime = millisecondsPause;
	}

	public static bool IsVisitingIslandToday(string npc_name)
	{
		return Game1.netWorldState.Value.IslandVisitors.Contains(npc_name);
	}

	public static bool shouldTimePass(bool ignore_multiplayer = false)
	{
		if (Game1.isFestival())
		{
			return false;
		}
		if (Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding)
		{
			return false;
		}
		if (Game1.farmEvent != null)
		{
			return false;
		}
		if (Game1.IsMultiplayer && !ignore_multiplayer)
		{
			return !Game1.netWorldState.Value.IsTimePaused;
		}
		if (Game1.paused || Game1.freezeControls || Game1.overlayMenu != null || Game1.isTimePaused)
		{
			return false;
		}
		if (Game1.eventUp)
		{
			return false;
		}
		if (Game1.activeClickableMenu != null && !(Game1.activeClickableMenu is BobberBar))
		{
			return false;
		}
		if (!Game1.player.CanMove && !Game1.player.UsingTool)
		{
			return Game1.player.forceTimePass;
		}
		return true;
	}

	public static Farmer getPlayerOrEventFarmer()
	{
		if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.isFestival && Game1.CurrentEvent.farmer != null)
		{
			return Game1.CurrentEvent.farmer;
		}
		return Game1.player;
	}

	public static void UpdateViewPort(bool overrideFreeze, Point centerPoint)
	{
		Game1.previousViewportPosition.X = Game1.viewport.X;
		Game1.previousViewportPosition.Y = Game1.viewport.Y;
		Farmer farmer = Game1.getPlayerOrEventFarmer();
		if (Game1.currentLocation == null)
		{
			return;
		}
		if (!Game1.viewportFreeze || overrideFreeze)
		{
			Microsoft.Xna.Framework.Rectangle viewportBounds = ((Game1.viewportClampArea == Microsoft.Xna.Framework.Rectangle.Empty) ? new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.currentLocation.Map.DisplayWidth, Game1.currentLocation.Map.DisplayHeight) : Game1.viewportClampArea);
			Point playerPixel = farmer.StandingPixel;
			bool snapBack = Game1.forceSnapOnNextViewportUpdate || Math.Abs(Game1.currentViewportTarget.X + (float)(Game1.viewport.Width / 2) + (float)viewportBounds.X - (float)playerPixel.X) > 64f || Math.Abs(Game1.currentViewportTarget.Y + (float)(Game1.viewport.Height / 2) + (float)viewportBounds.Y - (float)playerPixel.Y) > 64f;
			if (centerPoint.X >= viewportBounds.X + Game1.viewport.Width / 2 && centerPoint.X <= viewportBounds.X + viewportBounds.Width - Game1.viewport.Width / 2)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.X = centerPoint.X - Game1.viewport.Width / 2;
				}
				else if (Math.Abs(Game1.currentViewportTarget.X - (Game1.currentViewportTarget.X = centerPoint.X - Game1.viewport.Width / 2 + viewportBounds.X)) > farmer.getMovementSpeed())
				{
					Game1.currentViewportTarget.X += (float)Math.Sign(Game1.currentViewportTarget.X - (Game1.currentViewportTarget.X = centerPoint.X - Game1.viewport.Width / 2 + viewportBounds.X)) * farmer.getMovementSpeed();
				}
			}
			else if (centerPoint.X < Game1.viewport.Width / 2 + viewportBounds.X && Game1.viewport.Width <= viewportBounds.Width)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.X = viewportBounds.X;
				}
				else if (Math.Abs(Game1.currentViewportTarget.X - (float)viewportBounds.X) > farmer.getMovementSpeed())
				{
					Game1.currentViewportTarget.X -= (float)Math.Sign(Game1.currentViewportTarget.X - (float)viewportBounds.X) * farmer.getMovementSpeed();
				}
			}
			else if (Game1.viewport.Width <= viewportBounds.Width)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.X = viewportBounds.X + viewportBounds.Width - Game1.viewport.Width;
				}
				else if (!(Math.Abs(Game1.currentViewportTarget.X - (float)(viewportBounds.Width - Game1.viewport.Width)) > farmer.getMovementSpeed()))
				{
				}
			}
			else if (viewportBounds.Width < Game1.viewport.Width)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.X = (viewportBounds.Width - Game1.viewport.Width) / 2 + viewportBounds.X;
				}
				else
				{
					Math.Abs(Game1.currentViewportTarget.X - (float)((viewportBounds.Width + viewportBounds.X - Game1.viewport.Width) / 2));
					farmer.getMovementSpeed();
				}
			}
			if (centerPoint.Y >= Game1.viewport.Height / 2 && centerPoint.Y <= Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height / 2)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.Y = centerPoint.Y - Game1.viewport.Height / 2;
				}
				else if (Math.Abs(Game1.currentViewportTarget.Y - (float)(centerPoint.Y - Game1.viewport.Height / 2)) >= farmer.getMovementSpeed())
				{
					Game1.currentViewportTarget.Y -= (float)Math.Sign(Game1.currentViewportTarget.Y - (float)(centerPoint.Y - Game1.viewport.Height / 2)) * farmer.getMovementSpeed();
				}
			}
			else if (centerPoint.Y < Game1.viewport.Height / 2 && Game1.viewport.Height <= Game1.currentLocation.Map.DisplayHeight)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.Y = 0f;
				}
				else if (Math.Abs(Game1.currentViewportTarget.Y - 0f) > farmer.getMovementSpeed())
				{
					Game1.currentViewportTarget.Y -= (float)Math.Sign(Game1.currentViewportTarget.Y - 0f) * farmer.getMovementSpeed();
				}
				Game1.currentViewportTarget.Y = 0f;
			}
			else if (Game1.viewport.Height <= Game1.currentLocation.Map.DisplayHeight)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.Y = Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height;
				}
				else if (Math.Abs(Game1.currentViewportTarget.Y - (float)(Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) > farmer.getMovementSpeed())
				{
					Game1.currentViewportTarget.Y -= (float)Math.Sign(Game1.currentViewportTarget.Y - (float)(Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) * farmer.getMovementSpeed();
				}
			}
			else if (Game1.currentLocation.Map.DisplayHeight < Game1.viewport.Height)
			{
				if (farmer.isRafting || snapBack)
				{
					Game1.currentViewportTarget.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
				}
				else if (Math.Abs(Game1.currentViewportTarget.Y - (float)((Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2)) > farmer.getMovementSpeed())
				{
					Game1.currentViewportTarget.Y -= (float)Math.Sign(Game1.currentViewportTarget.Y - (float)((Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2)) * farmer.getMovementSpeed();
				}
			}
		}
		if (Game1.currentLocation.forceViewportPlayerFollow)
		{
			Game1.currentViewportTarget.X = farmer.Position.X - (float)(Game1.viewport.Width / 2);
			Game1.currentViewportTarget.Y = farmer.Position.Y - (float)(Game1.viewport.Height / 2);
		}
		bool forceSnap = Game1.forceSnapOnNextViewportUpdate;
		Game1.forceSnapOnNextViewportUpdate = false;
		if (Game1.currentViewportTarget.X != -2.1474836E+09f && (!Game1.viewportFreeze || overrideFreeze))
		{
			int difference = (int)(Game1.currentViewportTarget.X - (float)Game1.viewport.X);
			if (Math.Abs(difference) > 128)
			{
				Game1.viewportPositionLerp.X = Game1.currentViewportTarget.X;
			}
			else
			{
				Game1.viewportPositionLerp.X += (float)difference * farmer.getMovementSpeed() * 0.03f;
			}
			difference = (int)(Game1.currentViewportTarget.Y - (float)Game1.viewport.Y);
			if (Math.Abs(difference) > 128)
			{
				Game1.viewportPositionLerp.Y = (int)Game1.currentViewportTarget.Y;
			}
			else
			{
				Game1.viewportPositionLerp.Y += (float)difference * farmer.getMovementSpeed() * 0.03f;
			}
			if (forceSnap)
			{
				Game1.viewportPositionLerp.X = (int)Game1.currentViewportTarget.X;
				Game1.viewportPositionLerp.Y = (int)Game1.currentViewportTarget.Y;
			}
			Game1.viewport.X = (int)Game1.viewportPositionLerp.X;
			Game1.viewport.Y = (int)Game1.viewportPositionLerp.Y;
		}
	}

	private void UpdateCharacters(GameTime time)
	{
		if (Game1.CurrentEvent?.farmer != null && Game1.CurrentEvent.farmer != Game1.player)
		{
			Game1.CurrentEvent.farmer.Update(time, Game1.currentLocation);
		}
		Game1.player.Update(time, Game1.currentLocation);
		foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
		{
			if (v.Key != Game1.player.UniqueMultiplayerID)
			{
				v.Value.UpdateIfOtherPlayer(time);
			}
		}
	}

	public static void addMail(string mailName, bool noLetter = false, bool sendToEveryone = false)
	{
		if (sendToEveryone)
		{
			Game1.multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.SeenMail, noLetter);
			return;
		}
		mailName = mailName.Trim();
		mailName = mailName.Replace(Environment.NewLine, "");
		if (!Game1.player.hasOrWillReceiveMail(mailName))
		{
			if (noLetter)
			{
				Game1.player.mailReceived.Add(mailName);
			}
			else
			{
				Game1.player.mailbox.Add(mailName);
			}
		}
	}

	public static void addMailForTomorrow(string mailName, bool noLetter = false, bool sendToEveryone = false)
	{
		if (sendToEveryone)
		{
			Game1.multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.MailForTomorrow, noLetter);
			return;
		}
		mailName = mailName.Trim();
		mailName = mailName.Replace(Environment.NewLine, "");
		if (Game1.player.hasOrWillReceiveMail(mailName))
		{
			return;
		}
		if (noLetter)
		{
			mailName += "%&NL&%";
		}
		Game1.player.mailForTomorrow.Add(mailName);
		if (!sendToEveryone || !Game1.IsMultiplayer)
		{
			return;
		}
		foreach (Farmer farmer in Game1.otherFarmers.Values)
		{
			if (farmer != Game1.player && !Game1.player.hasOrWillReceiveMail(mailName))
			{
				farmer.mailForTomorrow.Add(mailName);
			}
		}
	}

	public static void drawDialogue(NPC speaker)
	{
		if (speaker.CurrentDialogue.Count == 0)
		{
			return;
		}
		Game1.activeClickableMenu = new DialogueBox(speaker.CurrentDialogue.Peek());
		if (Game1.activeClickableMenu is DialogueBox { dialogueFinished: not false })
		{
			Game1.activeClickableMenu = null;
			return;
		}
		Game1.dialogueUp = true;
		if (!Game1.eventUp)
		{
			Game1.player.Halt();
			Game1.player.CanMove = false;
		}
		if (speaker != null)
		{
			Game1.currentSpeaker = speaker;
		}
	}

	public static void multipleDialogues(string[] messages)
	{
		Game1.activeClickableMenu = new DialogueBox(messages.ToList());
		Game1.dialogueUp = true;
		Game1.player.CanMove = false;
	}

	public static void drawDialogueNoTyping(string dialogue)
	{
		Game1.drawObjectDialogue(dialogue);
		if (Game1.activeClickableMenu is DialogueBox dialogueBox)
		{
			dialogueBox.showTyping = false;
		}
	}

	public static void drawDialogueNoTyping(List<string> dialogues)
	{
		Game1.drawObjectDialogue(dialogues);
		if (Game1.activeClickableMenu is DialogueBox dialogueBox)
		{
			dialogueBox.showTyping = false;
		}
	}

	/// <summary>Show a dialogue box with text from an NPC's answering machine.</summary>
	/// <param name="npc">The NPC whose answering machine to display.</param>
	/// <param name="translationKey">The translation key for the message text.</param>
	/// <param name="substitutions">The token substitutions for placeholders in the translation text, if any.</param>
	public static void DrawAnsweringMachineDialogue(NPC npc, string translationKey, params object[] substitutions)
	{
		Dialogue dialogue = Dialogue.FromTranslation(npc, translationKey, substitutions);
		dialogue.overridePortrait = Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine");
		Game1.DrawDialogue(dialogue);
	}

	/// <summary>Show a dialogue box with text from an NPC.</summary>
	/// <param name="npc">The NPC whose dialogue to display.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	public static void DrawDialogue(NPC npc, string translationKey)
	{
		Game1.DrawDialogue(new Dialogue(npc, translationKey));
	}

	/// <summary>Show a dialogue box with text from an NPC.</summary>
	/// <param name="npc">The NPC whose dialogue to display.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="substitutions">The values with which to replace placeholders like <c>{0}</c> in the loaded text.</param>
	public static void DrawDialogue(NPC npc, string translationKey, params object[] substitutions)
	{
		Game1.DrawDialogue(Dialogue.FromTranslation(npc, translationKey, substitutions));
	}

	/// <summary>Show a dialogue box with text from an NPC.</summary>
	/// <param name="dialogue">The dialogue to display.</param>
	public static void DrawDialogue(Dialogue dialogue)
	{
		if (dialogue.speaker != null)
		{
			dialogue.speaker.CurrentDialogue.Push(dialogue);
			Game1.drawDialogue(dialogue.speaker);
			return;
		}
		Game1.activeClickableMenu = new DialogueBox(dialogue);
		Game1.dialogueUp = true;
		if (!Game1.eventUp)
		{
			Game1.player.Halt();
			Game1.player.CanMove = false;
		}
	}

	private static void checkIfDialogueIsQuestion()
	{
		if (Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0 && Game1.currentSpeaker.CurrentDialogue.Peek().isCurrentDialogueAQuestion())
		{
			Game1.questionChoices.Clear();
			Game1.isQuestion = true;
			List<NPCDialogueResponse> questions = Game1.currentSpeaker.CurrentDialogue.Peek().getNPCResponseOptions();
			for (int i = 0; i < questions.Count; i++)
			{
				Game1.questionChoices.Add(questions[i]);
			}
		}
	}

	public static void drawLetterMessage(string message)
	{
		Game1.activeClickableMenu = new LetterViewerMenu(message);
	}

	public static void drawObjectDialogue(string dialogue)
	{
		Game1.activeClickableMenu?.emergencyShutDown();
		Game1.activeClickableMenu = new DialogueBox(dialogue);
		Game1.player.CanMove = false;
		Game1.dialogueUp = true;
	}

	public static void drawObjectDialogue(List<string> dialogue)
	{
		Game1.activeClickableMenu?.emergencyShutDown();
		Game1.activeClickableMenu = new DialogueBox(dialogue);
		Game1.player.CanMove = false;
		Game1.dialogueUp = true;
	}

	public static void drawObjectQuestionDialogue(string dialogue, Response[] choices, int width)
	{
		Game1.activeClickableMenu = new DialogueBox(dialogue, choices, width);
		Game1.dialogueUp = true;
		Game1.player.CanMove = false;
	}

	public static void drawObjectQuestionDialogue(string dialogue, Response[] choices)
	{
		Game1.activeClickableMenu = new DialogueBox(dialogue, choices);
		Game1.dialogueUp = true;
		Game1.player.CanMove = false;
	}

	public static void warpCharacter(NPC character, string targetLocationName, Point position)
	{
		Game1.warpCharacter(character, targetLocationName, new Vector2(position.X, position.Y));
	}

	public static void warpCharacter(NPC character, string targetLocationName, Vector2 position)
	{
		Game1.warpCharacter(character, Game1.RequireLocation(targetLocationName), position);
	}

	public static void warpCharacter(NPC character, GameLocation targetLocation, Vector2 position)
	{
		foreach (string activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
		{
			if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var festival) && Game1.dayOfMonth >= festival.StartDay && Game1.dayOfMonth <= festival.EndDay && festival.Season == Game1.season && festival.MapReplacements != null && festival.MapReplacements.TryGetValue(targetLocation.name.Value, out var newName))
			{
				targetLocation = Game1.RequireLocation(newName);
			}
		}
		if (targetLocation.name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
		{
			targetLocation = Game1.RequireLocation("Trailer_Big");
			if (position.X == 12f && position.Y == 9f)
			{
				position.X = 13f;
				position.Y = 24f;
			}
		}
		if (Game1.IsClient)
		{
			Game1.multiplayer.requestCharacterWarp(character, targetLocation, position);
			return;
		}
		if (!targetLocation.characters.Contains(character))
		{
			character.currentLocation?.characters.Remove(character);
			targetLocation.addCharacter(character);
		}
		character.isCharging = false;
		character.speed = 2;
		character.blockedInterval = 0;
		NPC.getTextureNameForCharacter(character.Name);
		character.position.X = position.X * 64f;
		character.position.Y = position.Y * 64f;
		if (character.CurrentDialogue.Count > 0 && character.CurrentDialogue.Peek().removeOnNextMove && character.Tile != character.DefaultPosition / 64f)
		{
			character.CurrentDialogue.Pop();
		}
		if (targetLocation is FarmHouse farmHouse)
		{
			character.arriveAtFarmHouse(farmHouse);
		}
		else
		{
			character.arriveAt(targetLocation);
		}
		if (character.currentLocation != null && !character.currentLocation.Equals(targetLocation))
		{
			character.currentLocation.characters.Remove(character);
		}
		character.currentLocation = targetLocation;
	}

	public static LocationRequest getLocationRequest(string locationName, bool isStructure = false)
	{
		if (locationName == null)
		{
			throw new ArgumentException();
		}
		return new LocationRequest(locationName, isStructure, Game1.getLocationFromName(locationName, isStructure));
	}

	public static void warpHome()
	{
		LocationRequest obj = Game1.getLocationRequest(Game1.player.homeLocation.Value);
		obj.OnWarp += delegate
		{
			Game1.player.position.Set(Utility.PointToVector2((Game1.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f);
		};
		Game1.warpFarmer(obj, 5, 9, Game1.player.FacingDirection);
	}

	public static void warpFarmer(string locationName, int tileX, int tileY, bool flip)
	{
		Game1.warpFarmer(Game1.getLocationRequest(locationName), tileX, tileY, flip ? ((Game1.player.FacingDirection + 2) % 4) : Game1.player.FacingDirection);
	}

	public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp)
	{
		Game1.warpFarmer(Game1.getLocationRequest(locationName), tileX, tileY, facingDirectionAfterWarp);
	}

	public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp, bool isStructure)
	{
		Game1.warpFarmer(Game1.getLocationRequest(locationName, isStructure), tileX, tileY, facingDirectionAfterWarp);
	}

	public virtual bool ShouldDismountOnWarp(Horse mount, GameLocation old_location, GameLocation new_location)
	{
		if (mount == null)
		{
			return false;
		}
		if (Game1.currentLocation != null && Game1.currentLocation.IsOutdoors && new_location != null)
		{
			return !new_location.IsOutdoors;
		}
		return false;
	}

	public static void warpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
	{
		int warp_offset_x = Game1.nextFarmerWarpOffsetX;
		int warp_offset_y = Game1.nextFarmerWarpOffsetY;
		Game1.nextFarmerWarpOffsetX = 0;
		Game1.nextFarmerWarpOffsetY = 0;
		foreach (string activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
		{
			if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var festival) && Game1.dayOfMonth >= festival.StartDay && Game1.dayOfMonth <= festival.EndDay && festival.Season == Game1.season && festival.MapReplacements != null && festival.MapReplacements.TryGetValue(locationRequest.Name, out var newName))
			{
				locationRequest = Game1.getLocationRequest(newName);
			}
		}
		switch (locationRequest.Name)
		{
		case "BusStop":
			if (tileX < 10)
			{
				tileX = 10;
			}
			break;
		case "Farm":
			switch (Game1.currentLocation?.NameOrUniqueName)
			{
			case "FarmCave":
			{
				if (tileX != 34 || tileY != 6)
				{
					break;
				}
				if (Game1.getFarm().TryGetMapPropertyAs("FarmCaveEntry", out Point tile3, required: false))
				{
					tileX = tile3.X;
					tileY = tile3.Y;
					break;
				}
				switch (Game1.whichFarm)
				{
				case 6:
					tileX = 34;
					tileY = 16;
					break;
				case 5:
					tileX = 30;
					tileY = 36;
					break;
				}
				break;
			}
			case "Forest":
			{
				if (tileX != 41 || tileY != 64)
				{
					break;
				}
				if (Game1.getFarm().TryGetMapPropertyAs("ForestEntry", out Point tile4, required: false))
				{
					tileX = tile4.X;
					tileY = tile4.Y;
					break;
				}
				switch (Game1.whichFarm)
				{
				case 6:
					tileX = 82;
					tileY = 103;
					break;
				case 5:
					tileX = 40;
					tileY = 64;
					break;
				}
				break;
			}
			case "BusStop":
			{
				if (tileX == 79 && tileY == 17 && Game1.getFarm().TryGetMapPropertyAs("BusStopEntry", out Point tile2, required: false))
				{
					tileX = tile2.X;
					tileY = tile2.Y;
				}
				break;
			}
			case "Backwoods":
			{
				if (tileX == 40 && tileY == 0 && Game1.getFarm().TryGetMapPropertyAs("BackwoodsEntry", out Point tile, required: false))
				{
					tileX = tile.X;
					tileY = tile.Y;
				}
				break;
			}
			}
			break;
		case "IslandSouth":
			if (tileX <= 15 && tileY <= 6)
			{
				tileX = 21;
				tileY = 43;
			}
			break;
		case "Trailer":
			if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				locationRequest = Game1.getLocationRequest("Trailer_Big");
				tileX = 13;
				tileY = 24;
			}
			break;
		case "Club":
			if (Game1.player.hasClubCard)
			{
				break;
			}
			locationRequest = Game1.getLocationRequest("SandyHouse");
			locationRequest.OnWarp += delegate
			{
				NPC characterFromName = Game1.currentLocation.getCharacterFromName("Bouncer");
				if (characterFromName != null)
				{
					Vector2 vector = new Vector2(17f, 4f);
					characterFromName.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:Club_Bouncer_TextAboveHead" + (Game1.random.Next(2) + 1)));
					int num = Game1.random.Next();
					Game1.currentLocation.playSound("thudStep");
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(288, 100f, 1, 24, vector * 64f, flicker: true, flipped: false, Game1.currentLocation, Game1.player)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = num,
						endFunction = Game1.currentLocation.removeTemporarySpritesWithID
					}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, 0.0263f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = num
					}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, 0.0263f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = num
					}, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, vector * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, 0.0263f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = num
					});
					Game1.currentLocation.netAudio.StartPlaying("fuse");
				}
			};
			tileX = 17;
			tileY = 4;
			break;
		}
		if (VolcanoDungeon.IsGeneratedLevel(locationRequest.Name))
		{
			warp_offset_x = 0;
			warp_offset_y = 0;
		}
		if (Game1.player.isRidingHorse() && Game1.currentLocation != null)
		{
			GameLocation next_location = locationRequest.Location;
			if (next_location == null)
			{
				next_location = Game1.getLocationFromName(locationRequest.Name);
			}
			if (Game1.game1.ShouldDismountOnWarp(Game1.player.mount, Game1.currentLocation, next_location))
			{
				Game1.player.mount.dismount();
				warp_offset_x = 0;
				warp_offset_y = 0;
			}
		}
		if (Game1.weatherIcon == 1 && Game1.whereIsTodaysFest != null && locationRequest.Name.Equals(Game1.whereIsTodaysFest) && !Game1.warpingForForcedRemoteEvent)
		{
			string[] timeParts = ArgUtility.SplitBySpace(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1]);
			if (Game1.timeOfDay <= Convert.ToInt32(timeParts[1]))
			{
				if (Game1.timeOfDay < Convert.ToInt32(timeParts[0]))
				{
					if (!(Game1.currentLocation?.Name == "Hospital"))
					{
						Game1.player.Position = Game1.player.lastPosition;
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2973"));
						return;
					}
					locationRequest = Game1.getLocationRequest("BusStop");
					tileX = 34;
					tileY = 23;
				}
				else
				{
					if (Game1.IsMultiplayer)
					{
						Game1.netReady.SetLocalReady("festivalStart", ready: true);
						Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", allowCancel: true, delegate
						{
							Game1.exitActiveMenu();
							if (Game1.player.mount != null)
							{
								Game1.player.mount.dismount();
								warp_offset_x = 0;
								warp_offset_y = 0;
							}
							Game1.performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
						});
						return;
					}
					if (Game1.player.mount != null)
					{
						Game1.player.mount.dismount();
						warp_offset_x = 0;
						warp_offset_y = 0;
					}
				}
			}
		}
		tileX += warp_offset_x;
		tileY += warp_offset_y;
		Game1.performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
	}

	private static void performWarpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
	{
		if (locationRequest.Location != null)
		{
			if (tileX >= locationRequest.Location.Map.Layers[0].LayerWidth - 1)
			{
				tileX--;
			}
			if (Game1.IsMasterGame)
			{
				locationRequest.Location.hostSetup();
			}
		}
		Game1.log.Verbose("Warping to " + locationRequest.Name);
		if (Game1.player.IsSitting())
		{
			Game1.player.StopSitting(animate: false);
		}
		if (Game1.player.UsingTool)
		{
			Game1.player.completelyStopAnimatingOrDoingAction();
		}
		Game1.player.previousLocationName = ((Game1.player.currentLocation != null) ? Game1.player.currentLocation.name.Value : "");
		Game1.locationRequest = locationRequest;
		Game1.xLocationAfterWarp = tileX;
		Game1.yLocationAfterWarp = tileY;
		Game1._isWarping = true;
		Game1.facingDirectionAfterWarp = facingDirectionAfterWarp;
		Game1.fadeScreenToBlack();
		Game1.setRichPresence("location", locationRequest.Name);
		if (Game1.IsDedicatedHost)
		{
			Game1.fadeToBlackAlpha = 1.1f;
			Game1.fadeToBlack = true;
			Game1.nonWarpFade = false;
		}
	}

	private static void notifyServerOfWarp(bool needsLocationInfo)
	{
		if (Game1.locationRequest != null)
		{
			byte flags = (byte)((Game1.locationRequest.IsStructure ? 1u : 0u) | (uint)(Game1.warpingForForcedRemoteEvent ? 2 : 0) | (uint)(needsLocationInfo ? 4 : 0));
			flags = Game1.facingDirectionAfterWarp switch
			{
				1 => (byte)(flags | 0x10), 
				2 => (byte)(flags | 0x20), 
				3 => (byte)(flags | 0x40), 
				_ => (byte)(flags | 8), 
			};
			Game1.client.sendMessage(5, (short)Game1.xLocationAfterWarp, (short)Game1.yLocationAfterWarp, Game1.locationRequest.Name, flags);
		}
	}

	public static void requestLocationInfoFromServer()
	{
		Game1.notifyServerOfWarp(needsLocationInfo: true);
		Game1.currentLocation = null;
		Game1.player.Position = new Vector2(Game1.xLocationAfterWarp * 64, Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16);
		Game1.player.faceDirection(Game1.facingDirectionAfterWarp);
	}

	/// <summary>Get the first NPC which matches a condition.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="check">The condition to check on each NPC.</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static T GetCharacterWhere<T>(Func<T, bool> check, bool includeEventActors = false) where T : NPC
	{
		T match = null;
		T fallback = null;
		Utility.ForEachCharacter(delegate(NPC rawNpc)
		{
			if (rawNpc is T val && check(val))
			{
				if (val.currentLocation?.IsActiveLocation() ?? false)
				{
					match = val;
					return false;
				}
				fallback = val;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get the first NPC of the given type.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static T GetCharacterOfType<T>(bool includeEventActors = false) where T : NPC
	{
		T match = null;
		T fallback = null;
		Utility.ForEachCharacter(delegate(NPC rawNpc)
		{
			if (rawNpc is T val)
			{
				if (rawNpc.currentLocation?.IsActiveLocation() ?? false)
				{
					match = val;
					return false;
				}
				fallback = val;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get an NPC by its name.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static T getCharacterFromName<T>(string name, bool mustBeVillager = true, bool includeEventActors = false) where T : NPC
	{
		T match = null;
		T fallback = null;
		Utility.ForEachCharacter(delegate(NPC rawNpc)
		{
			if (rawNpc is T val && val.Name == name && (!mustBeVillager || val.IsVillager))
			{
				if (val.currentLocation?.IsActiveLocation() ?? false)
				{
					match = val;
					return false;
				}
				fallback = val;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get an NPC by its name.</summary>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <returns>Returns the matching NPC if found, else <c>null</c>.</returns>
	public static NPC getCharacterFromName(string name, bool mustBeVillager = true, bool includeEventActors = false)
	{
		NPC match = null;
		NPC fallback = null;
		Utility.ForEachCharacter(delegate(NPC npc)
		{
			if (npc.Name == name && (!mustBeVillager || npc.IsVillager))
			{
				if (npc.currentLocation?.IsActiveLocation() ?? false)
				{
					match = npc;
					return false;
				}
				fallback = npc;
			}
			return true;
		}, includeEventActors);
		return match ?? fallback;
	}

	/// <summary>Get an NPC by its name, or throw an exception if it's not found.</summary>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	public static NPC RequireCharacter(string name, bool mustBeVillager = true)
	{
		return Game1.getCharacterFromName(name, mustBeVillager) ?? throw new KeyNotFoundException($"Required {(mustBeVillager ? "villager" : "NPC")} '{name}' not found.");
	}

	/// <summary>Get an NPC by its name, or throw an exception if it's not found.</summary>
	/// <typeparam name="T">The expected NPC type.</typeparam>
	/// <param name="name">The NPC name.</param>
	/// <param name="mustBeVillager">Whether to only match NPCs which return true for <see cref="P:StardewValley.NPC.IsVillager" />.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">There's no NPC matching the given arguments.</exception>
	/// <exception cref="T:System.InvalidCastException">The NPC found can't be converted to <typeparamref name="T" />.</exception>
	public static T RequireCharacter<T>(string name, bool mustBeVillager = true) where T : NPC
	{
		NPC npc = Game1.getCharacterFromName(name, mustBeVillager);
		if (!(npc is T cast))
		{
			if (npc == null)
			{
				throw new KeyNotFoundException($"Required {(mustBeVillager ? "villager" : "NPC")} '{name}' not found.");
			}
			throw new InvalidCastException($"Can't convert NPC '{name}' from '{npc?.GetType().FullName}' to the required '{typeof(T).FullName}'.");
		}
		return cast;
	}

	/// <summary>Get a location by its name, or throw an exception if it's not found.</summary>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">There's no location matching the given arguments.</exception>
	public static GameLocation RequireLocation(string name, bool isStructure = false)
	{
		return Game1.getLocationFromName(name, isStructure) ?? throw new KeyNotFoundException($"Required {(isStructure ? "structure " : "")}location '{name}' not found.");
	}

	/// <summary>Get a location by its name, or throw an exception if it's not found.</summary>
	/// <typeparam name="TLocation">The expected location type.</typeparam>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">There's no location matching the given arguments.</exception>
	/// <exception cref="T:System.InvalidCastException">The location found can't be converted to <typeparamref name="TLocation" />.</exception>
	public static TLocation RequireLocation<TLocation>(string name, bool isStructure = false) where TLocation : GameLocation
	{
		GameLocation location = Game1.getLocationFromName(name, isStructure);
		if (!(location is TLocation cast))
		{
			if (location == null)
			{
				throw new KeyNotFoundException($"Required {(isStructure ? "structure " : "")}location '{name}' not found.");
			}
			throw new InvalidCastException($"Can't convert location {name} from '{location?.GetType().FullName}' to the required '{typeof(TLocation).FullName}'.");
		}
		return cast;
	}

	/// <summary>Get a location by its name, or <c>null</c> if it's not found.</summary>
	/// <param name="name">The location name.</param>
	public static GameLocation getLocationFromName(string name)
	{
		return Game1.getLocationFromName(name, isStructure: false);
	}

	/// <summary>Get a location by its name, or <c>null</c> if it's not found.</summary>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	public static GameLocation getLocationFromName(string name, bool isStructure)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (Game1.currentLocation != null)
		{
			if (!isStructure)
			{
				if (Game1.currentLocation.name.Value.EqualsIgnoreCase(name))
				{
					return Game1.currentLocation;
				}
				if (Game1.currentLocation.isStructure.Value && Game1.currentLocation.Root != null && Game1.currentLocation.Root.Value.NameOrUniqueName.EqualsIgnoreCase(name))
				{
					return Game1.currentLocation.Root.Value;
				}
			}
			else if (Game1.currentLocation.NameOrUniqueName == name)
			{
				return Game1.currentLocation;
			}
		}
		if (Game1._locationLookup.TryGetValue(name, out var cached_location))
		{
			return cached_location;
		}
		return Game1.getLocationFromNameInLocationsList(name, isStructure);
	}

	/// <summary>Get a location by its name (ignoring the cache and current location), or <c>null</c> if it's not found.</summary>
	/// <param name="name">The location name.</param>
	/// <param name="isStructure">Whether the location is an interior structure.</param>
	public static GameLocation getLocationFromNameInLocationsList(string name, bool isStructure = false)
	{
		for (int i = 0; i < Game1.locations.Count; i++)
		{
			GameLocation location = Game1.locations[i];
			if (!isStructure)
			{
				if (location.Name.EqualsIgnoreCase(name))
				{
					Game1._locationLookup[location.Name] = location;
					return location;
				}
				continue;
			}
			GameLocation buildingIndoors = Game1.findStructure(location, name);
			if (buildingIndoors != null)
			{
				Game1._locationLookup[name] = buildingIndoors;
				return buildingIndoors;
			}
		}
		if (MineShaft.IsGeneratedLevel(name))
		{
			return MineShaft.GetMine(name);
		}
		if (VolcanoDungeon.IsGeneratedLevel(name))
		{
			return VolcanoDungeon.GetLevel(name);
		}
		if (!isStructure)
		{
			return Game1.getLocationFromName(name, isStructure: true);
		}
		return null;
	}

	public static void flushLocationLookup()
	{
		Game1._locationLookup.Clear();
	}

	public static void removeLocationFromLocationLookup(string nameOrUniqueName)
	{
		Game1._locationLookup.RemoveWhere((KeyValuePair<string, GameLocation> p) => p.Value.NameOrUniqueName == nameOrUniqueName);
	}

	public static void removeLocationFromLocationLookup(GameLocation location)
	{
		Game1._locationLookup.RemoveWhere((KeyValuePair<string, GameLocation> p) => p.Value == location);
	}

	public static GameLocation findStructure(GameLocation parentLocation, string name)
	{
		foreach (Building building in parentLocation.buildings)
		{
			if (building.HasIndoorsName(name))
			{
				return building.GetIndoors();
			}
		}
		return null;
	}

	public static void addNewFarmBuildingMaps()
	{
		FarmHouse home = Utility.getHomeOfFarmer(Game1.player);
		if (Game1.player.HouseUpgradeLevel >= 1 && home.Map.Id.Equals("FarmHouse"))
		{
			home.updateMap();
		}
	}

	public static void PassOutNewDay()
	{
		Game1.player.lastSleepLocation.Value = Game1.currentLocation.NameOrUniqueName;
		Game1.player.lastSleepPoint.Value = Game1.player.TilePoint;
		if (!Game1.IsMultiplayer)
		{
			Game1.NewDay(0f);
			return;
		}
		Game1.player.FarmerSprite.setCurrentSingleFrame(5, 3000);
		Game1.player.FarmerSprite.PauseForSingleAnimation = true;
		Game1.player.passedOut = true;
		if (Game1.activeClickableMenu != null)
		{
			Game1.activeClickableMenu.emergencyShutDown();
			Game1.exitActiveMenu();
		}
		Game1.activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: false, delegate
		{
			Game1.NewDay(0f);
		});
	}

	public static void NewDay(float timeToPause)
	{
		if (Game1.activeClickableMenu is ReadyCheckDialog { checkName: "sleep" } readyCheckDialog && !readyCheckDialog.isCancelable())
		{
			readyCheckDialog.confirm();
		}
		Game1.currentMinigame = null;
		Game1.newDay = true;
		Game1.newDaySync.create();
		if (Game1.player.isInBed.Value || Game1.player.passedOut)
		{
			Game1.nonWarpFade = true;
			Game1.screenFade.FadeScreenToBlack(Game1.player.passedOut ? 1.1f : 0f);
			Game1.player.Halt();
			Game1.player.currentEyes = 1;
			Game1.player.blinkTimer = -4000;
			Game1.player.CanMove = false;
			Game1.player.passedOut = false;
			Game1.pauseTime = timeToPause;
		}
		if (Game1.activeClickableMenu != null && !Game1.dialogueUp)
		{
			Game1.activeClickableMenu.emergencyShutDown();
			Game1.exitActiveMenu();
		}
	}

	public static void screenGlowOnce(Color glowColor, bool hold, float rate = 0.005f, float maxAlpha = 0.3f)
	{
		Game1.screenGlowMax = maxAlpha;
		Game1.screenGlowRate = rate;
		Game1.screenGlowAlpha = 0f;
		Game1.screenGlowUp = true;
		Game1.screenGlowColor = glowColor;
		Game1.screenGlow = true;
		Game1.screenGlowHold = hold;
	}

	public static string shortDayNameFromDayOfSeason(int dayOfSeason)
	{
		return (dayOfSeason % 7) switch
		{
			0 => "Sun", 
			1 => "Mon", 
			2 => "Tue", 
			3 => "Wed", 
			4 => "Thu", 
			5 => "Fri", 
			6 => "Sat", 
			_ => "", 
		};
	}

	public static string shortDayDisplayNameFromDayOfSeason(int dayOfSeason)
	{
		if (dayOfSeason < 0)
		{
			return string.Empty;
		}
		return Game1._shortDayDisplayName[dayOfSeason % 7];
	}

	public static void runTestEvent()
	{
		StreamReader file = new StreamReader("test_event.txt");
		string? locationName = file.ReadLine();
		string event_string = file.ReadToEnd();
		event_string = event_string.Replace("\r\n", "/").Replace("\n", "/");
		Game1.log.Verbose("Running test event: " + event_string);
		LocationRequest location_request = Game1.getLocationRequest(locationName);
		location_request.OnWarp += delegate
		{
			Game1.currentLocation.currentEvent = new Event(event_string);
			Game1.currentLocation.checkForEvents();
		};
		int x = 8;
		int y = 8;
		Utility.getDefaultWarpLocation(locationName, ref x, ref y);
		Game1.warpFarmer(location_request, x, y, Game1.player.FacingDirection);
	}

	public static bool isMusicContextActiveButNotPlaying(MusicContext music_context = MusicContext.Default)
	{
		if (Game1._activeMusicContext != music_context)
		{
			return false;
		}
		if (Game1.morningSongPlayAction != null)
		{
			return false;
		}
		string currentTrack = Game1.getMusicTrackName(music_context);
		if (currentTrack == "none")
		{
			return true;
		}
		if (Game1.currentSong != null && Game1.currentSong.Name == currentTrack && !Game1.currentSong.IsPlaying)
		{
			return true;
		}
		return false;
	}

	public static bool IsMusicContextActive(MusicContext music_context = MusicContext.Default)
	{
		if (Game1._activeMusicContext != music_context)
		{
			return true;
		}
		return false;
	}

	public static bool doesMusicContextHaveTrack(MusicContext music_context = MusicContext.Default)
	{
		return Game1._requestedMusicTracks.ContainsKey(music_context);
	}

	public static string getMusicTrackName(MusicContext music_context = MusicContext.Default)
	{
		if (Game1._requestedMusicTracks.TryGetValue(music_context, out var trackData))
		{
			return trackData.Key;
		}
		if (music_context == MusicContext.Default)
		{
			return Game1.getMusicTrackName(MusicContext.SubLocation);
		}
		return "none";
	}

	public static void stopMusicTrack(MusicContext music_context)
	{
		if (Game1._requestedMusicTracks.Remove(music_context))
		{
			if (music_context == MusicContext.Default)
			{
				Game1.stopMusicTrack(MusicContext.SubLocation);
			}
			Game1.UpdateRequestedMusicTrack();
		}
	}

	public static void changeMusicTrack(string newTrackName, bool track_interruptable = false, MusicContext music_context = MusicContext.Default)
	{
		if (newTrackName == null)
		{
			return;
		}
		if (music_context == MusicContext.Default)
		{
			if (Game1.morningSongPlayAction != null)
			{
				if (Game1.delayedActions.Contains(Game1.morningSongPlayAction))
				{
					Game1.delayedActions.Remove(Game1.morningSongPlayAction);
				}
				Game1.morningSongPlayAction = null;
			}
			if (Game1.IsGreenRainingHere() && !Game1.currentLocation.InIslandContext() && Game1.IsRainingHere(Game1.currentLocation) && !newTrackName.Equals("rain"))
			{
				return;
			}
		}
		if (music_context == MusicContext.Default || music_context == MusicContext.SubLocation)
		{
			Game1.IsPlayingBackgroundMusic = false;
			Game1.IsPlayingOutdoorsAmbience = false;
			Game1.IsPlayingNightAmbience = false;
			Game1.IsPlayingTownMusic = false;
			Game1.IsPlayingMorningSong = false;
		}
		if (music_context != MusicContext.ImportantSplitScreenMusic && !Game1.player.songsHeard.Contains(newTrackName))
		{
			Utility.farmerHeardSong(newTrackName);
		}
		Game1._requestedMusicTracks[music_context] = new KeyValuePair<string, bool>(newTrackName, track_interruptable);
		Game1.UpdateRequestedMusicTrack();
	}

	public static void UpdateRequestedMusicTrack()
	{
		Game1._activeMusicContext = MusicContext.Default;
		KeyValuePair<string, bool> requested_track_data = new KeyValuePair<string, bool>("none", value: true);
		for (int i = 0; i < 6; i++)
		{
			MusicContext context = (MusicContext)i;
			if (Game1._requestedMusicTracks.TryGetValue(context, out var trackData))
			{
				if (context != MusicContext.ImportantSplitScreenMusic)
				{
					Game1._activeMusicContext = context;
				}
				requested_track_data = trackData;
			}
		}
		if (requested_track_data.Key != Game1.requestedMusicTrack || requested_track_data.Value != Game1.requestedMusicTrackOverrideable)
		{
			Game1.requestedMusicDirty = true;
			Game1.requestedMusicTrack = requested_track_data.Key;
			Game1.requestedMusicTrackOverrideable = requested_track_data.Value;
		}
	}

	/// <summary>Warp the player into a generated mine level.</summary>
	/// <param name="whatLevel">The mine level.</param>
	/// <param name="forceLayout">The layout in <c>Maps/Mines</c> to use, or <c>null</c> to choose a random one based on the level.</param>
	public static void enterMine(int whatLevel, int? forceLayout = null)
	{
		Game1.warpFarmer(MineShaft.GetLevelName(whatLevel, forceLayout), 6, 6, 2);
		Game1.player.temporarilyInvincible = true;
		Game1.player.temporaryInvincibilityTimer = 0;
		Game1.player.flashDuringThisTemporaryInvincibility = false;
		Game1.player.currentTemporaryInvincibilityDuration = 1000;
	}

	/// <summary>Get the season which currently applies to a location.</summary>
	/// <param name="location">The location to check, or <c>null</c> for the global season.</param>
	public static Season GetSeasonForLocation(GameLocation location)
	{
		return location?.GetSeason() ?? Game1.season;
	}

	/// <summary>Get the season which currently applies to a location as a numeric index.</summary>
	/// <param name="location">The location to check, or <c>null</c> for the global season.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Game1.GetSeasonForLocation(StardewValley.GameLocation)" /> instead.</remarks>
	public static int GetSeasonIndexForLocation(GameLocation location)
	{
		return location?.GetSeasonIndex() ?? Game1.seasonIndex;
	}

	/// <summary>Get the season which currently applies to a location as a string.</summary>
	/// <param name="location">The location to check, or <c>null</c> for the global season.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Game1.GetSeasonForLocation(StardewValley.GameLocation)" /> instead.</remarks>
	public static string GetSeasonKeyForLocation(GameLocation location)
	{
		return location?.GetSeasonKey() ?? Game1.currentSeason;
	}

	/// <summary>Unlock an achievement for the current platform.</summary>
	/// <param name="which">The achievement to unlock.</param>
	public static void getPlatformAchievement(string which)
	{
		Program.sdk.GetAchievement(which);
	}

	public static void getSteamAchievement(string which)
	{
		if (which.Equals("0"))
		{
			which = "a0";
		}
		Game1.getPlatformAchievement(which);
	}

	public static void getAchievement(int which, bool allowBroadcasting = true)
	{
		if (Game1.player.achievements.Contains(which) || Game1.gameMode != 3 || !Game1.achievements.TryGetValue(which, out var rawData))
		{
			return;
		}
		string achievementName = rawData.Split('^')[0];
		Game1.player.achievements.Add(which);
		if (which < 32 && allowBroadcasting)
		{
			if (Game1.stats.isSharedAchievement(which))
			{
				Game1.multiplayer.sendSharedAchievementMessage(which);
			}
			else
			{
				string farmerName = Game1.player.Name;
				if (farmerName == "")
				{
					farmerName = TokenStringBuilder.LocalizedText("Strings\\UI:Chat_PlayerJoinedNewName");
				}
				Game1.multiplayer.globalChatInfoMessage("Achievement", farmerName, TokenStringBuilder.AchievementName(which));
			}
		}
		Game1.playSound("achievement");
		Game1.addHUDMessage(HUDMessage.ForAchievement(achievementName));
		Game1.player.autoGenerateActiveDialogueEvent("achievement_" + which);
		Game1.getPlatformAchievement(which.ToString());
		if (!Game1.player.hasOrWillReceiveMail("hatter"))
		{
			Game1.addMailForTomorrow("hatter");
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number)
	{
		for (int i = 0; i < number; i++)
		{
			Game1.createObjectDebris(id, xTile, yTile);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, GameLocation location)
	{
		for (int i = 0; i < number; i++)
		{
			Game1.createObjectDebris(id, xTile, yTile, -1, 0, 1f, location);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, float velocityMultiplier)
	{
		for (int i = 0; i < number; i++)
		{
			Game1.createObjectDebris(id, xTile, yTile, -1, 0, velocityMultiplier);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, long who)
	{
		for (int i = 0; i < number; i++)
		{
			Game1.createObjectDebris(id, xTile, yTile, who);
		}
	}

	public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, long who, GameLocation location)
	{
		for (int i = 0; i < number; i++)
		{
			Game1.createObjectDebris(id, xTile, yTile, who, location);
		}
	}

	public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks)
	{
		Game1.createDebris(debrisType, xTile, yTile, numberOfChunks, Game1.currentLocation);
	}

	public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks, GameLocation location)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		location.debris.Add(new Debris(debrisType, numberOfChunks, new Vector2(xTile * 64 + 32, yTile * 64 + 32), Game1.player.getStandingPosition()));
	}

	public static Debris createItemDebris(Item item, Vector2 pixelOrigin, int direction, GameLocation location = null, int groundLevel = -1, bool flopFish = false)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		Vector2 targetLocation = new Vector2(pixelOrigin.X, pixelOrigin.Y);
		switch (direction)
		{
		case 0:
			pixelOrigin.Y -= 16f + (float)Game1.recentMultiplayerRandom.Next(32);
			targetLocation.Y -= 35.2f;
			break;
		case 1:
			pixelOrigin.X += 16f;
			pixelOrigin.Y -= 32 - Game1.recentMultiplayerRandom.Next(8);
			targetLocation.X += 128f;
			break;
		case 2:
			pixelOrigin.Y += Game1.recentMultiplayerRandom.Next(16);
			targetLocation.Y += 64f;
			break;
		case 3:
			pixelOrigin.X -= 16f;
			pixelOrigin.Y -= 32 - Game1.recentMultiplayerRandom.Next(8);
			targetLocation.X -= 128f;
			break;
		case -1:
			targetLocation = Game1.player.getStandingPosition();
			break;
		}
		Debris d = new Debris(item, pixelOrigin, targetLocation);
		if (flopFish && item.Category == -4)
		{
			d.floppingFish.Value = true;
		}
		if (groundLevel != -1)
		{
			d.chunkFinalYLevel = groundLevel;
		}
		location.debris.Add(d);
		return d;
	}

	public static void createMultipleItemDebris(Item item, Vector2 pixelOrigin, int direction, GameLocation location = null, int groundLevel = -1, bool flopFish = false)
	{
		int stack = item.Stack;
		item.Stack = 1;
		Game1.createItemDebris(item, pixelOrigin, (direction == -1) ? Game1.random.Next(4) : direction, location, groundLevel, flopFish);
		for (int i = 1; i < stack; i++)
		{
			Game1.createItemDebris(item.getOne(), pixelOrigin, (direction == -1) ? Game1.random.Next(4) : direction, location, groundLevel, flopFish);
		}
	}

	public static void createRadialDebris(GameLocation location, int debrisType, int xTile, int yTile, int numberOfChunks, bool resource, int groundLevel = -1, bool item = false, Color? color = null)
	{
		if (groundLevel == -1)
		{
			groundLevel = yTile * 64 + 32;
		}
		Vector2 debrisOrigin = new Vector2(xTile * 64 + 64, yTile * 64 + 64);
		if (item)
		{
			while (numberOfChunks > 0)
			{
				Vector2 offset = Game1.random.Next(4) switch
				{
					0 => new Vector2(-64f, 0f), 
					1 => new Vector2(64f, 0f), 
					2 => new Vector2(0f, 64f), 
					_ => new Vector2(0f, -64f), 
				};
				Item debris = ItemRegistry.Create("(O)" + debrisType);
				location.debris.Add(new Debris(debris, debrisOrigin, debrisOrigin + offset));
				numberOfChunks--;
			}
		}
		if (resource)
		{
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f)));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f)));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f)));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f)));
		}
		else
		{
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f), groundLevel, color));
		}
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks)
	{
		Game1.createRadialDebris(location, texture, sourcerectangle, xTile, yTile, numberOfChunks, yTile);
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks, int groundLevelTile)
	{
		Game1.createRadialDebris(location, texture, sourcerectangle, 8, xTile * 64 + 32 + Game1.random.Next(32), yTile * 64 + 32 + Game1.random.Next(32), numberOfChunks, groundLevelTile);
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile)
	{
		Vector2 debrisOrigin = new Vector2(xPosition, yPosition);
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f), groundLevelTile * 64, sizeOfSourceRectSquares));
		location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f), groundLevelTile * 64, sizeOfSourceRectSquares));
	}

	public static void createRadialDebris_MoreNatural(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevel)
	{
		Vector2 debrisOrigin = new Vector2(xPosition, yPosition);
		for (int i = 0; i < numberOfChunks; i++)
		{
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(Game1.random.Next(-64, 64), Game1.random.Next(-64, 64)), groundLevel + Game1.random.Next(-32, 32), sizeOfSourceRectSquares));
		}
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color)
	{
		Game1.createRadialDebris(location, texture, sourcerectangle, sizeOfSourceRectSquares, xPosition, yPosition, numberOfChunks, groundLevelTile, color, 1f);
	}

	public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color, float scale)
	{
		Vector2 debrisOrigin = new Vector2(xPosition, yPosition);
		while (numberOfChunks > 0)
		{
			switch (Game1.random.Next(4))
			{
			case 0:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			case 1:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			case 2:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(Game1.random.Next(-64, 64), -64f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			case 3:
			{
				Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(Game1.random.Next(-64, 64), 64f), groundLevelTile * 64, sizeOfSourceRectSquares);
				d.nonSpriteChunkColor.Value = color;
				location?.debris.Add(d);
				d.Chunks[0].scale = scale;
				break;
			}
			}
			numberOfChunks--;
		}
	}

	public static void createObjectDebris(string id, int xTile, int yTile, long whichPlayer)
	{
		Farmer forPlayer = Game1.GetPlayer(whichPlayer) ?? Game1.player;
		Game1.currentLocation.debris.Add(new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), forPlayer.getStandingPosition()));
	}

	public static void createObjectDebris(string id, int xTile, int yTile, long whichPlayer, GameLocation location)
	{
		Farmer forPlayer = Game1.GetPlayer(whichPlayer) ?? Game1.player;
		location.debris.Add(new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), forPlayer.getStandingPosition()));
	}

	public static void createObjectDebris(string id, int xTile, int yTile, GameLocation location)
	{
		Game1.createObjectDebris(id, xTile, yTile, -1, 0, 1f, location);
	}

	public static void createObjectDebris(string id, int xTile, int yTile, int groundLevel = -1, int itemQuality = 0, float velocityMultiplyer = 1f, GameLocation location = null)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		Debris d = new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), Game1.player.getStandingPosition())
		{
			itemQuality = itemQuality
		};
		foreach (Chunk chunk in d.Chunks)
		{
			chunk.xVelocity.Value *= velocityMultiplyer;
			chunk.yVelocity.Value *= velocityMultiplyer;
		}
		if (groundLevel != -1)
		{
			d.chunkFinalYLevel = groundLevel;
		}
		location.debris.Add(d);
	}

	[Obsolete("Use GetPlayer instead. Equivalent usage: `GetPlayer(id, onlineOnly: true) ?? Game1.MasterPlayer`.")]
	public static Farmer getFarmer(long id)
	{
		return Game1.GetPlayer(id, onlyOnline: true) ?? Game1.MasterPlayer;
	}

	[Obsolete("Use GetPlayer instead.")]
	public static Farmer getFarmerMaybeOffline(long id)
	{
		return Game1.GetPlayer(id);
	}

	/// <summary>Get the player matching a unique multiplayer ID, if it's valid.</summary>
	/// <param name="id">The unique multiplayer ID.</param>
	/// <param name="onlyOnline">Whether to only check online players.</param>
	/// <returns>Returns the matching player, or <c>null</c> if the ID isn't valid.</returns>
	public static Farmer? GetPlayer(long id, bool onlyOnline = false)
	{
		if (Game1.MasterPlayer.UniqueMultiplayerID == id)
		{
			return Game1.MasterPlayer;
		}
		if (Game1.otherFarmers.TryGetValue(id, out var onlineFarmhand))
		{
			return onlineFarmhand;
		}
		if (!onlyOnline && Game1.netWorldState.Value.farmhandData.TryGetValue(id, out var offlineFarmhand))
		{
			return offlineFarmhand;
		}
		return null;
	}

	/// <summary>Get all players including the host, online farmhands, and offline farmhands.</summary>
	public static IEnumerable<Farmer> getAllFarmers()
	{
		return Enumerable.Repeat(Game1.MasterPlayer, 1).Concat(Game1.getAllFarmhands());
	}

	/// <summary>Get all players who are currently connected, including the host player.</summary>
	public static FarmerCollection getOnlineFarmers()
	{
		return Game1._onlineFarmers;
	}

	/// <summary>Get online and offline farmhands.</summary>
	public static IEnumerable<Farmer> getAllFarmhands()
	{
		foreach (Farmer farmer in Game1.netWorldState.Value.farmhandData.Values)
		{
			if (farmer.isActive())
			{
				yield return Game1.otherFarmers[farmer.UniqueMultiplayerID];
			}
			else
			{
				yield return farmer;
			}
		}
	}

	/// <summary>Get farmhands which aren't currently connected.</summary>
	public static IEnumerable<Farmer> getOfflineFarmhands()
	{
		foreach (Farmer farmer in Game1.netWorldState.Value.farmhandData.Values)
		{
			if (!farmer.isActive())
			{
				yield return farmer;
			}
		}
	}

	public static void farmerFindsArtifact(string itemId)
	{
		Item item = ItemRegistry.Create(itemId);
		Game1.player.addItemToInventoryBool(item);
	}

	public static bool doesHUDMessageExist(string s)
	{
		for (int i = 0; i < Game1.hudMessages.Count; i++)
		{
			if (s.Equals(Game1.hudMessages[i].message))
			{
				return true;
			}
		}
		return false;
	}

	public static void addHUDMessage(HUDMessage message)
	{
		if (message.type != null || message.whatType != 0)
		{
			for (int i = 0; i < Game1.hudMessages.Count; i++)
			{
				if (message.type != null && message.type == Game1.hudMessages[i].type)
				{
					Game1.hudMessages[i].number = Game1.hudMessages[i].number + message.number;
					Game1.hudMessages[i].timeLeft = 3500f;
					Game1.hudMessages[i].transparency = 1f;
					if (Game1.hudMessages[i].number > 50000)
					{
						HUDMessage.numbersEasterEgg(Game1.hudMessages[i].number);
					}
					return;
				}
				if (message.whatType == Game1.hudMessages[i].whatType && message.whatType != 1 && message.message != null && message.message.Equals(Game1.hudMessages[i].message))
				{
					Game1.hudMessages[i].timeLeft = message.timeLeft;
					Game1.hudMessages[i].transparency = 1f;
					return;
				}
			}
		}
		Game1.hudMessages.Add(message);
		for (int i2 = Game1.hudMessages.Count - 1; i2 >= 0; i2--)
		{
			if (Game1.hudMessages[i2].noIcon)
			{
				HUDMessage tmp = Game1.hudMessages[i2];
				Game1.hudMessages.RemoveAt(i2);
				Game1.hudMessages.Add(tmp);
			}
		}
	}

	public static void showSwordswipeAnimation(int direction, Vector2 source, float animationSpeed, bool flip)
	{
		switch (direction)
		{
		case 0:
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y), flicker: false, flipped: false, !flip, -(float)Math.PI / 2f));
			break;
		case 1:
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 96f + 16f, source.Y + 48f), flicker: false, flip, verticalFlipped: false, flip ? (-(float)Math.PI) : 0f));
			break;
		case 2:
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y + 128f), flicker: false, flipped: false, !flip, (float)Math.PI / 2f));
			break;
		case 3:
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X - 32f - 16f, source.Y + 48f), flicker: false, !flip, verticalFlipped: false, flip ? (-(float)Math.PI) : 0f));
			break;
		}
	}

	public static void removeDebris(Debris.DebrisType type)
	{
		Game1.currentLocation.debris.RemoveWhere((Debris debris) => debris.debrisType.Value == type);
	}

	public static void toolAnimationDone(Farmer who)
	{
		float oldStamina = Game1.player.Stamina;
		if (who.CurrentTool == null)
		{
			return;
		}
		if (who.Stamina > 0f)
		{
			int powerupLevel = 1;
			Vector2 actionTile = who.GetToolLocation();
			if (who.CurrentTool is FishingRod { isFishing: not false })
			{
				who.canReleaseTool = false;
			}
			else if (!(who.CurrentTool is FishingRod))
			{
				who.UsingTool = false;
				if (who.CurrentTool.QualifiedItemId == "(T)WateringCan")
				{
					switch (who.FacingDirection)
					{
					case 0:
					case 2:
						who.CurrentTool.DoFunction(Game1.currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
						break;
					case 1:
					case 3:
						who.CurrentTool.DoFunction(Game1.currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
						break;
					}
				}
				else if (who.CurrentTool is MeleeWeapon)
				{
					who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
				}
				else
				{
					if (who.CurrentTool.QualifiedItemId == "(T)ReturnScepter")
					{
						who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
					}
					who.CurrentTool.DoFunction(Game1.currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
				}
			}
			else
			{
				who.UsingTool = false;
			}
		}
		else if (who.CurrentTool.instantUse.Value)
		{
			who.CurrentTool.DoFunction(Game1.currentLocation, 0, 0, 0, who);
		}
		else
		{
			who.UsingTool = false;
		}
		who.lastClick = Vector2.Zero;
		if (who.IsLocalPlayer && !Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
		{
			who.setRunning(Game1.options.autoRun);
		}
		if (!who.UsingTool && who.FarmerSprite.PauseForSingleAnimation)
		{
			who.FarmerSprite.StopAnimation();
		}
		if (Game1.player.Stamina <= 0f && oldStamina > 0f)
		{
			Game1.player.doEmote(36);
		}
	}

	public static bool pressActionButton(KeyboardState currentKBState, MouseState currentMouseState, GamePadState currentPadState)
	{
		if (Game1.IsChatting)
		{
			currentKBState = default(KeyboardState);
		}
		if (Game1.dialogueTyping)
		{
			bool consume = true;
			Game1.dialogueTyping = false;
			if (Game1.currentSpeaker != null)
			{
				Game1.currentDialogueCharacterIndex = Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length;
			}
			else if (Game1.currentObjectDialogue.Count > 0)
			{
				Game1.currentDialogueCharacterIndex = Game1.currentObjectDialogue.Peek().Length;
			}
			else
			{
				consume = false;
			}
			Game1.dialogueTypingInterval = 0;
			Game1.oldKBState = currentKBState;
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldPadState = currentPadState;
			if (consume)
			{
				Game1.playSound("dialogueCharacterClose");
				return false;
			}
		}
		if (Game1.dialogueUp)
		{
			if (Game1.isQuestion)
			{
				Game1.isQuestion = false;
				if (Game1.currentSpeaker != null)
				{
					if (Game1.currentSpeaker.CurrentDialogue.Peek().chooseResponse(Game1.questionChoices[Game1.currentQuestionChoice]))
					{
						Game1.currentDialogueCharacterIndex = 1;
						Game1.dialogueTyping = true;
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
						return false;
					}
				}
				else
				{
					Game1.dialogueUp = false;
					if (Game1.eventUp && Game1.currentLocation.afterQuestion == null)
					{
						Game1.currentLocation.currentEvent.answerDialogue(Game1.currentLocation.lastQuestionKey, Game1.currentQuestionChoice);
						Game1.currentQuestionChoice = 0;
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
					}
					else if (Game1.currentLocation.answerDialogue(Game1.questionChoices[Game1.currentQuestionChoice]))
					{
						Game1.currentQuestionChoice = 0;
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
						return false;
					}
					if (Game1.dialogueUp)
					{
						Game1.currentDialogueCharacterIndex = 1;
						Game1.dialogueTyping = true;
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
						return false;
					}
				}
				Game1.currentQuestionChoice = 0;
			}
			string exitDialogue = null;
			if (Game1.currentSpeaker != null)
			{
				if (Game1.currentSpeaker.immediateSpeak)
				{
					Game1.currentSpeaker.immediateSpeak = false;
					return false;
				}
				exitDialogue = ((Game1.currentSpeaker.CurrentDialogue.Count > 0) ? Game1.currentSpeaker.CurrentDialogue.Peek().exitCurrentDialogue() : null);
			}
			if (exitDialogue == null)
			{
				if (Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0 && Game1.currentSpeaker.CurrentDialogue.Peek().isOnFinalDialogue() && Game1.currentSpeaker.CurrentDialogue.Count > 0)
				{
					Game1.currentSpeaker.CurrentDialogue.Pop();
				}
				Game1.dialogueUp = false;
				if (Game1.messagePause)
				{
					Game1.pauseTime = 500f;
				}
				if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.currentObjectDialogue.Dequeue();
				}
				Game1.currentDialogueCharacterIndex = 0;
				if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.dialogueUp = true;
					Game1.questionChoices.Clear();
					Game1.oldKBState = currentKBState;
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldPadState = currentPadState;
					Game1.dialogueTyping = true;
					return false;
				}
				if (Game1.currentSpeaker != null && !Game1.currentSpeaker.Name.Equals("Gunther") && !Game1.eventUp && !Game1.currentSpeaker.doingEndOfRouteAnimation.Value)
				{
					Game1.currentSpeaker.doneFacingPlayer(Game1.player);
				}
				Game1.currentSpeaker = null;
				if (!Game1.eventUp)
				{
					Game1.player.CanMove = true;
				}
				else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
				{
					if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
					{
						Game1.currentLocation.currentEvent.CurrentCommand++;
					}
					else
					{
						Game1.player.CanMove = true;
					}
				}
				Game1.questionChoices.Clear();
				Game1.playSound("smallSelect");
			}
			else
			{
				Game1.playSound("smallSelect");
				Game1.currentDialogueCharacterIndex = 0;
				Game1.dialogueTyping = true;
				Game1.checkIfDialogueIsQuestion();
			}
			Game1.oldKBState = currentKBState;
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldPadState = currentPadState;
			return false;
		}
		if (!Game1.player.UsingTool && (!Game1.eventUp || (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.playerControlSequence)) && !Game1.fadeToBlack)
		{
			if (Game1.wasMouseVisibleThisFrame && Game1.currentLocation.animals.Length > 0)
			{
				Vector2 mousePosition = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y);
				if (Utility.withinRadiusOfPlayer((int)mousePosition.X, (int)mousePosition.Y, 1, Game1.player))
				{
					if (Game1.currentLocation.CheckPetAnimal(mousePosition, Game1.player))
					{
						return true;
					}
					if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && Game1.currentLocation.CheckInspectAnimal(mousePosition, Game1.player))
					{
						return true;
					}
				}
			}
			Vector2 grabTile = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / 64f;
			Vector2 cursorTile = grabTile;
			bool non_directed_tile = false;
			if (!Game1.wasMouseVisibleThisFrame || Game1.mouseCursorTransparency == 0f || !Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
			{
				grabTile = Game1.player.GetGrabTile();
				non_directed_tile = true;
			}
			bool was_character_at_grab_tile = false;
			if (Game1.eventUp && !Game1.isFestival())
			{
				Game1.CurrentEvent?.receiveActionPress((int)grabTile.X, (int)grabTile.Y);
				Game1.oldKBState = currentKBState;
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldPadState = currentPadState;
				return false;
			}
			if (Game1.tryToCheckAt(grabTile, Game1.player))
			{
				return false;
			}
			if (Game1.player.isRidingHorse())
			{
				Game1.player.mount.checkAction(Game1.player, Game1.player.currentLocation);
				return false;
			}
			if (!Game1.player.canMove)
			{
				return false;
			}
			if (!was_character_at_grab_tile && Game1.player.currentLocation.isCharacterAtTile(grabTile) != null)
			{
				was_character_at_grab_tile = true;
			}
			bool isPlacingObject = false;
			if (Game1.player.ActiveObject != null && !(Game1.player.ActiveObject is Furniture))
			{
				if (Game1.player.ActiveObject.performUseAction(Game1.currentLocation))
				{
					Game1.player.reduceActiveItemByOne();
					Game1.oldKBState = currentKBState;
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldPadState = currentPadState;
					return false;
				}
				int stack = Game1.player.ActiveObject.Stack;
				Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
				if (non_directed_tile)
				{
					Game1.isCheckingNonMousePlacement = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.actionButton))
				{
					Game1.isCheckingNonMousePlacement = true;
				}
				Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(Game1.player, Game1.currentLocation, Game1.player.ActiveObject, (int)grabTile.X * 64 + 32, (int)grabTile.Y * 64 + 32);
				if (!Game1.isCheckingNonMousePlacement && Game1.player.ActiveObject is Wallpaper && Utility.tryToPlaceItem(Game1.currentLocation, Game1.player.ActiveObject, (int)cursorTile.X * 64, (int)cursorTile.Y * 64))
				{
					Game1.isCheckingNonMousePlacement = false;
					return true;
				}
				if (Utility.tryToPlaceItem(Game1.currentLocation, Game1.player.ActiveObject, (int)valid_position.X, (int)valid_position.Y))
				{
					Game1.isCheckingNonMousePlacement = false;
					return true;
				}
				if (!Game1.eventUp && (Game1.player.ActiveObject == null || Game1.player.ActiveObject.Stack < stack || Game1.player.ActiveObject.isPlaceable()))
				{
					isPlacingObject = true;
				}
				Game1.isCheckingNonMousePlacement = false;
			}
			if (!isPlacingObject && !was_character_at_grab_tile)
			{
				grabTile.Y += 1f;
				if (Game1.player.FacingDirection >= 0 && Game1.player.FacingDirection <= 3)
				{
					Vector2 normalized_offset = grabTile - Game1.player.Tile;
					if (normalized_offset.X > 0f || normalized_offset.Y > 0f)
					{
						normalized_offset.Normalize();
					}
					if (Vector2.Dot(Utility.DirectionsTileVectors[Game1.player.FacingDirection], normalized_offset) >= 0f && Game1.tryToCheckAt(grabTile, Game1.player))
					{
						return false;
					}
				}
				if (!Game1.eventUp && Game1.player.ActiveObject is Furniture furniture)
				{
					furniture.rotate();
					Game1.playSound("dwoop");
					Game1.oldKBState = currentKBState;
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldPadState = currentPadState;
					return false;
				}
				grabTile.Y -= 2f;
				if (Game1.player.FacingDirection >= 0 && Game1.player.FacingDirection <= 3 && !was_character_at_grab_tile)
				{
					Vector2 normalized_offset2 = grabTile - Game1.player.Tile;
					if (normalized_offset2.X > 0f || normalized_offset2.Y > 0f)
					{
						normalized_offset2.Normalize();
					}
					if (Vector2.Dot(Utility.DirectionsTileVectors[Game1.player.FacingDirection], normalized_offset2) >= 0f && Game1.tryToCheckAt(grabTile, Game1.player))
					{
						return false;
					}
				}
				if (!Game1.eventUp && Game1.player.ActiveObject is Furniture furniture2)
				{
					furniture2.rotate();
					Game1.playSound("dwoop");
					Game1.oldKBState = currentKBState;
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldPadState = currentPadState;
					return false;
				}
				grabTile = Game1.player.Tile;
				if (Game1.tryToCheckAt(grabTile, Game1.player))
				{
					return false;
				}
				if (!Game1.eventUp && Game1.player.ActiveObject is Furniture furniture3)
				{
					furniture3.rotate();
					Game1.playSound("dwoop");
					Game1.oldKBState = currentKBState;
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldPadState = currentPadState;
					return false;
				}
			}
			if (!Game1.player.isEating && Game1.player.ActiveObject != null && !Game1.dialogueUp && !Game1.eventUp && !Game1.player.canOnlyWalk && !Game1.player.FarmerSprite.PauseForSingleAnimation && !Game1.fadeToBlack && Game1.player.ActiveObject.Edibility != -300 && Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				if (Game1.player.team.SpecialOrderRuleActive("SC_NO_FOOD"))
				{
					MineShaft obj = Game1.player.currentLocation as MineShaft;
					if (obj != null && obj.getMineArea() == 121)
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), 3));
						return false;
					}
				}
				if (Game1.player.hasBuff("25") && Game1.player.ActiveObject != null && !Game1.player.ActiveObject.HasContextTag("ginger_item"))
				{
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Nauseous_CantEat"), 3));
					return false;
				}
				Game1.player.faceDirection(2);
				Game1.player.itemToEat = Game1.player.ActiveObject;
				Game1.player.FarmerSprite.setCurrentSingleAnimation(304);
				if (Game1.objectData.TryGetValue(Game1.player.ActiveObject.ItemId, out var objectData))
				{
					Game1.currentLocation.createQuestionDialogue((objectData.IsDrink && Game1.player.ActiveObject.preserve.Value != Object.PreserveType.Pickle) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", Game1.player.ActiveObject.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3160", Game1.player.ActiveObject.DisplayName), Game1.currentLocation.createYesNoResponses(), "Eat");
				}
				Game1.oldKBState = currentKBState;
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldPadState = currentPadState;
				return false;
			}
		}
		if (Game1.player.CurrentTool is MeleeWeapon && Game1.player.CanMove && !Game1.player.canOnlyWalk && !Game1.eventUp && !Game1.player.onBridge.Value && Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
		{
			((MeleeWeapon)Game1.player.CurrentTool).animateSpecialMove(Game1.player);
			return false;
		}
		return true;
	}

	public static bool IsPerformingMousePlacement()
	{
		if (Game1.mouseCursorTransparency == 0f || !Game1.wasMouseVisibleThisFrame || (!Game1.lastCursorMotionWasMouse && (Game1.player.ActiveObject == null || (!Game1.player.ActiveObject.isPlaceable() && Game1.player.ActiveObject.Category != -74 && !Game1.player.ActiveObject.isSapling()))))
		{
			return false;
		}
		return true;
	}

	public static Vector2 GetPlacementGrabTile()
	{
		if (!Game1.IsPerformingMousePlacement())
		{
			return Game1.player.GetGrabTile();
		}
		return new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / 64f;
	}

	public static bool tryToCheckAt(Vector2 grabTile, Farmer who)
	{
		if (Game1.player.onBridge.Value)
		{
			return false;
		}
		Game1.haltAfterCheck = true;
		if (Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player) && Game1.hooks.OnGameLocation_CheckAction(Game1.currentLocation, new Location((int)grabTile.X, (int)grabTile.Y), Game1.viewport, who, () => Game1.currentLocation.checkAction(new Location((int)grabTile.X, (int)grabTile.Y), Game1.viewport, who)))
		{
			Game1.updateCursorTileHint();
			who.lastGrabTile = grabTile;
			if (who.CanMove && Game1.haltAfterCheck)
			{
				who.faceGeneralDirection(grabTile * 64f);
				who.Halt();
			}
			Game1.oldKBState = Game1.GetKeyboardState();
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldPadState = Game1.input.GetGamePadState();
			return true;
		}
		return false;
	}

	public static void pressSwitchToolButton()
	{
		if (Game1.player.netItemStowed.Value)
		{
			Game1.player.netItemStowed.Set(newValue: false);
			Game1.player.UpdateItemStow();
		}
		int whichWay = ((Game1.input.GetMouseState().ScrollWheelValue > Game1.oldMouseState.ScrollWheelValue) ? (-1) : ((Game1.input.GetMouseState().ScrollWheelValue < Game1.oldMouseState.ScrollWheelValue) ? 1 : 0));
		if (Game1.options.gamepadControls && whichWay == 0)
		{
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftTrigger))
			{
				whichWay = -1;
			}
			else if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightTrigger))
			{
				whichWay = 1;
			}
		}
		if (Game1.options.invertScrollDirection)
		{
			whichWay *= -1;
		}
		if (whichWay == 0)
		{
			return;
		}
		Game1.player.CurrentToolIndex = (Game1.player.CurrentToolIndex + whichWay) % 12;
		if (Game1.player.CurrentToolIndex < 0)
		{
			Game1.player.CurrentToolIndex = 11;
		}
		for (int i = 0; i < 12; i++)
		{
			if (Game1.player.CurrentItem != null)
			{
				break;
			}
			Game1.player.CurrentToolIndex = (whichWay + Game1.player.CurrentToolIndex) % 12;
			if (Game1.player.CurrentToolIndex < 0)
			{
				Game1.player.CurrentToolIndex = 11;
			}
		}
		Game1.playSound("toolSwap");
		if (Game1.player.ActiveObject != null)
		{
			Game1.player.showCarrying();
		}
		else
		{
			Game1.player.showNotCarrying();
		}
	}

	public static bool pressUseToolButton()
	{
		bool stow_was_initialized = Game1.game1._didInitiateItemStow;
		Game1.game1._didInitiateItemStow = false;
		if (Game1.fadeToBlack)
		{
			return false;
		}
		Game1.player.toolPower.Value = 0;
		Game1.player.toolHold.Value = 0;
		bool did_attempt_object_removal = false;
		if (Game1.player.CurrentTool == null && Game1.player.ActiveObject == null)
		{
			Vector2 c = Game1.player.GetToolLocation() / 64f;
			c.X = (int)c.X;
			c.Y = (int)c.Y;
			if (Game1.currentLocation.Objects.TryGetValue(c, out var o) && !o.readyForHarvest.Value && o.heldObject.Value == null && !(o is Fence) && !(o is CrabPot) && (o.Type == "Crafting" || o.Type == "interactive") && !o.IsTwig())
			{
				did_attempt_object_removal = true;
				o.setHealth(o.getHealth() - 1);
				o.shakeTimer = 300;
				o.playNearbySoundAll("hammer");
				if (o.getHealth() < 2)
				{
					o.playNearbySoundAll("hammer");
					if (o.getHealth() < 1)
					{
						Tool t = ItemRegistry.Create<Tool>("(T)Pickaxe");
						t.DoFunction(Game1.currentLocation, -1, -1, 0, Game1.player);
						if (o.performToolAction(t))
						{
							o.performRemoveAction();
							if (o.Type == "Crafting" && o.fragility.Value != 2)
							{
								Game1.currentLocation.debris.Add(new Debris(o.QualifiedItemId, Game1.player.GetToolLocation(), Utility.PointToVector2(Game1.player.StandingPixel)));
							}
							Game1.currentLocation.Objects.Remove(c);
							return true;
						}
					}
				}
			}
		}
		if (Game1.currentMinigame == null && !Game1.player.UsingTool && (Game1.player.IsSitting() || Game1.player.isRidingHorse() || Game1.player.onBridge.Value || Game1.dialogueUp || (Game1.eventUp && !Game1.CurrentEvent.canPlayerUseTool() && (!Game1.currentLocation.currentEvent.playerControlSequence || (Game1.activeClickableMenu == null && Game1.currentMinigame == null))) || (Game1.player.CurrentTool != null && (Game1.currentLocation.doesPositionCollideWithCharacter(Utility.getRectangleCenteredAt(Game1.player.GetToolLocation(), 64), ignoreMonsters: true)?.IsVillager ?? false))))
		{
			Game1.pressActionButton(Game1.GetKeyboardState(), Game1.input.GetMouseState(), Game1.input.GetGamePadState());
			return false;
		}
		if (Game1.player.canOnlyWalk)
		{
			return true;
		}
		Vector2 position = ((!Game1.wasMouseVisibleThisFrame) ? Game1.player.GetToolLocation() : new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y));
		if (Utility.canGrabSomethingFromHere((int)position.X, (int)position.Y, Game1.player))
		{
			Vector2 tile = new Vector2(position.X / 64f, position.Y / 64f);
			if (Game1.hooks.OnGameLocation_CheckAction(Game1.currentLocation, new Location((int)tile.X, (int)tile.Y), Game1.viewport, Game1.player, () => Game1.currentLocation.checkAction(new Location((int)tile.X, (int)tile.Y), Game1.viewport, Game1.player)))
			{
				Game1.updateCursorTileHint();
				return true;
			}
			if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var terrainFeature))
			{
				terrainFeature.performUseAction(tile);
				return true;
			}
			return false;
		}
		if (Game1.currentLocation.leftClick((int)position.X, (int)position.Y, Game1.player))
		{
			return true;
		}
		Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
		if (Game1.player.ActiveObject != null)
		{
			if (Game1.options.allowStowing && Game1.CanPlayerStowItem(Game1.GetPlacementGrabTile()))
			{
				if (Game1.didPlayerJustLeftClick() || stow_was_initialized)
				{
					Game1.game1._didInitiateItemStow = true;
					Game1.playSound("stoneStep");
					Game1.player.netItemStowed.Set(newValue: true);
					return true;
				}
				return true;
			}
			if (Utility.withinRadiusOfPlayer((int)position.X, (int)position.Y, 1, Game1.player) && Game1.hooks.OnGameLocation_CheckAction(Game1.currentLocation, new Location((int)position.X / 64, (int)position.Y / 64), Game1.viewport, Game1.player, () => Game1.currentLocation.checkAction(new Location((int)position.X / 64, (int)position.Y / 64), Game1.viewport, Game1.player)))
			{
				return true;
			}
			Vector2 grabTile = Game1.GetPlacementGrabTile();
			Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(Game1.player, Game1.currentLocation, Game1.player.ActiveObject, (int)grabTile.X * 64, (int)grabTile.Y * 64);
			if (Utility.tryToPlaceItem(Game1.currentLocation, Game1.player.ActiveObject, (int)valid_position.X, (int)valid_position.Y))
			{
				Game1.isCheckingNonMousePlacement = false;
				return true;
			}
			Game1.isCheckingNonMousePlacement = false;
		}
		if (Game1.currentLocation.LowPriorityLeftClick((int)position.X, (int)position.Y, Game1.player))
		{
			return true;
		}
		if (Game1.options.allowStowing && Game1.player.netItemStowed.Value && !did_attempt_object_removal && (stow_was_initialized || Game1.didPlayerJustLeftClick(ignoreNonMouseHeldInput: true)))
		{
			Game1.game1._didInitiateItemStow = true;
			Game1.playSound("toolSwap");
			Game1.player.netItemStowed.Set(newValue: false);
			return true;
		}
		if (Game1.player.UsingTool)
		{
			Game1.player.lastClick = new Vector2((int)position.X, (int)position.Y);
			Game1.player.CurrentTool.DoFunction(Game1.player.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, 1, Game1.player);
			return true;
		}
		if (Game1.player.ActiveObject == null && !Game1.player.isEating && Game1.player.CurrentTool != null)
		{
			if (Game1.player.Stamina <= 20f && Game1.player.CurrentTool != null && !(Game1.player.CurrentTool is MeleeWeapon) && !Game1.eventUp)
			{
				Game1.staminaShakeTimer = 1000;
				for (int i = 0; i < 4; i++)
				{
					Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2(Game1.random.Next(32) + Game1.uiViewport.Width - 56, Game1.uiViewport.Height - 224 - 16 - (int)((double)(Game1.player.MaxStamina - 270) * 0.715)), flipped: false, 0.012f, Color.SkyBlue)
					{
						motion = new Vector2(-2f, -10f),
						acceleration = new Vector2(0f, 0.5f),
						local = true,
						scale = 4 + Game1.random.Next(-1, 0),
						delayBeforeAnimationStart = i * 30
					});
				}
			}
			if (!(Game1.player.CurrentTool is MeleeWeapon) || Game1.didPlayerJustLeftClick(ignoreNonMouseHeldInput: true))
			{
				int old_direction = Game1.player.FacingDirection;
				Vector2 tool_location = Game1.player.GetToolLocation(position);
				Game1.player.FacingDirection = Game1.player.getGeneralDirectionTowards(new Vector2((int)tool_location.X, (int)tool_location.Y));
				Game1.player.lastClick = new Vector2((int)position.X, (int)position.Y);
				Game1.player.BeginUsingTool();
				if (!Game1.player.usingTool.Value)
				{
					Game1.player.FacingDirection = old_direction;
				}
				else if (Game1.player.FarmerSprite.IsPlayingBasicAnimation(old_direction, carrying: true) || Game1.player.FarmerSprite.IsPlayingBasicAnimation(old_direction, carrying: false))
				{
					Game1.player.FarmerSprite.StopAnimation();
				}
			}
		}
		return false;
	}

	public static bool CanPlayerStowItem(Vector2 position)
	{
		if (Game1.player.ActiveObject == null)
		{
			return false;
		}
		if (Game1.player.ActiveObject.bigCraftable.Value)
		{
			return false;
		}
		Object activeObject = Game1.player.ActiveObject;
		if (!(activeObject is Furniture))
		{
			if (activeObject != null && (Game1.player.ActiveObject.Category == -74 || Game1.player.ActiveObject.Category == -19))
			{
				Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(Game1.player, Game1.currentLocation, Game1.player.ActiveObject, (int)position.X * 64, (int)position.Y * 64);
				if (Utility.playerCanPlaceItemHere(Game1.player.currentLocation, Game1.player.ActiveObject, (int)valid_position.X, (int)valid_position.Y, Game1.player) && (!Game1.player.ActiveObject.isSapling() || Game1.IsPerformingMousePlacement()))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static int getMouseXRaw()
	{
		return Game1.input.GetMouseState().X;
	}

	public static int getMouseYRaw()
	{
		return Game1.input.GetMouseState().Y;
	}

	public static bool IsOnMainThread()
	{
		if (Thread.CurrentThread != null)
		{
			return !Thread.CurrentThread.IsBackground;
		}
		return false;
	}

	public static void PushUIMode()
	{
		if (!Game1.IsOnMainThread())
		{
			return;
		}
		Game1.uiModeCount++;
		if (Game1.uiModeCount <= 0 || Game1.uiMode)
		{
			return;
		}
		Game1.uiMode = true;
		if (Game1.game1.isDrawing && Game1.IsOnMainThread())
		{
			if (Game1.game1.uiScreen != null && !Game1.game1.uiScreen.IsDisposed)
			{
				RenderTargetBinding[] render_targets = Game1.graphics.GraphicsDevice.GetRenderTargets();
				if (render_targets.Length != 0)
				{
					Game1.nonUIRenderTarget = render_targets[0].RenderTarget as RenderTarget2D;
				}
				else
				{
					Game1.nonUIRenderTarget = null;
				}
				Game1.SetRenderTarget(Game1.game1.uiScreen);
			}
			if (Game1.isRenderingScreenBuffer)
			{
				Game1.SetRenderTarget(null);
			}
		}
		xTile.Dimensions.Rectangle ui_viewport_rect = new xTile.Dimensions.Rectangle(0, 0, (int)Math.Ceiling((float)Game1.viewport.Width * Game1.options.zoomLevel / Game1.options.uiScale), (int)Math.Ceiling((float)Game1.viewport.Height * Game1.options.zoomLevel / Game1.options.uiScale));
		ui_viewport_rect.X = Game1.viewport.X;
		ui_viewport_rect.Y = Game1.viewport.Y;
		Game1.uiViewport = ui_viewport_rect;
	}

	public static void PopUIMode()
	{
		if (!Game1.IsOnMainThread())
		{
			return;
		}
		Game1.uiModeCount--;
		if (Game1.uiModeCount > 0 || !Game1.uiMode)
		{
			return;
		}
		if (Game1.game1.isDrawing)
		{
			if (Game1.graphics.GraphicsDevice.GetRenderTargets().Length != 0 && Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget == Game1.game1.uiScreen)
			{
				if (Game1.nonUIRenderTarget != null && !Game1.nonUIRenderTarget.IsDisposed)
				{
					Game1.SetRenderTarget(Game1.nonUIRenderTarget);
				}
				else
				{
					Game1.SetRenderTarget(null);
				}
			}
			if (Game1.isRenderingScreenBuffer)
			{
				Game1.SetRenderTarget(null);
			}
		}
		Game1.nonUIRenderTarget = null;
		Game1.uiMode = false;
	}

	public static void SetRenderTarget(RenderTarget2D target)
	{
		if (!Game1.isRenderingScreenBuffer && Game1.IsOnMainThread())
		{
			Game1.graphics.GraphicsDevice.SetRenderTarget(target);
		}
	}

	public static void InUIMode(Action action)
	{
		Game1.PushUIMode();
		try
		{
			action();
		}
		finally
		{
			Game1.PopUIMode();
		}
	}

	public static void StartWorldDrawInUI(SpriteBatch b)
	{
		Game1._oldUIModeCount = 0;
		if (Game1.uiMode)
		{
			Game1._oldUIModeCount = Game1.uiModeCount;
			b?.End();
			while (Game1.uiModeCount > 0)
			{
				Game1.PopUIMode();
			}
			b?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}
	}

	public static void EndWorldDrawInUI(SpriteBatch b)
	{
		if (Game1._oldUIModeCount > 0)
		{
			b?.End();
			for (int i = 0; i < Game1._oldUIModeCount; i++)
			{
				Game1.PushUIMode();
			}
			b?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}
		Game1._oldUIModeCount = 0;
	}

	public static int getMouseX()
	{
		return Game1.getMouseX(Game1.uiMode);
	}

	public static int getMouseX(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)Game1.input.GetMouseState().X / Game1.options.uiScale);
		}
		return (int)((float)Game1.input.GetMouseState().X * (1f / Game1.options.zoomLevel));
	}

	public static int getOldMouseX()
	{
		return Game1.getOldMouseX(Game1.uiMode);
	}

	public static int getOldMouseX(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)Game1.oldMouseState.X / Game1.options.uiScale);
		}
		return (int)((float)Game1.oldMouseState.X * (1f / Game1.options.zoomLevel));
	}

	public static int getMouseY()
	{
		return Game1.getMouseY(Game1.uiMode);
	}

	public static int getMouseY(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)Game1.input.GetMouseState().Y / Game1.options.uiScale);
		}
		return (int)((float)Game1.input.GetMouseState().Y * (1f / Game1.options.zoomLevel));
	}

	public static int getOldMouseY()
	{
		return Game1.getOldMouseY(Game1.uiMode);
	}

	public static int getOldMouseY(bool ui_scale)
	{
		if (ui_scale)
		{
			return (int)((float)Game1.oldMouseState.Y / Game1.options.uiScale);
		}
		return (int)((float)Game1.oldMouseState.Y * (1f / Game1.options.zoomLevel));
	}

	public static bool PlayEvent(string eventId, GameLocation location, out bool validEvent, bool checkPreconditions = true, bool checkSeen = true)
	{
		string eventAssetName;
		Dictionary<string, string> locationEvents;
		try
		{
			if (!location.TryGetLocationEvents(out eventAssetName, out locationEvents))
			{
				validEvent = false;
				return false;
			}
		}
		catch
		{
			validEvent = false;
			return false;
		}
		if (locationEvents == null)
		{
			validEvent = false;
			return false;
		}
		foreach (string key in locationEvents.Keys)
		{
			if (!(key.Split('/')[0] == eventId))
			{
				continue;
			}
			validEvent = true;
			if (checkSeen && (Game1.player.eventsSeen.Contains(eventId) || Game1.eventsSeenSinceLastLocationChange.Contains(eventId)))
			{
				return false;
			}
			string id = eventId;
			if (checkPreconditions)
			{
				id = location.checkEventPrecondition(key, check_seen: false);
			}
			if (!string.IsNullOrEmpty(id) && id != "-1")
			{
				if (location.Name != Game1.currentLocation.Name)
				{
					LocationRequest obj2 = Game1.getLocationRequest(location.Name);
					obj2.OnLoad += delegate
					{
						Game1.currentLocation.currentEvent = new Event(locationEvents[key], eventAssetName, id);
					};
					int x = 8;
					int y = 8;
					Utility.getDefaultWarpLocation(obj2.Name, ref x, ref y);
					Game1.warpFarmer(obj2, x, y, Game1.player.FacingDirection);
				}
				else
				{
					Game1.globalFadeToBlack(delegate
					{
						Game1.forceSnapOnNextViewportUpdate = true;
						Game1.currentLocation.startEvent(new Event(locationEvents[key], eventAssetName, id));
						Game1.globalFadeToClear();
					});
				}
				return true;
			}
			return false;
		}
		validEvent = false;
		return false;
	}

	public static bool PlayEvent(string eventId, bool checkPreconditions = true, bool checkSeen = true)
	{
		if (checkSeen && (Game1.player.eventsSeen.Contains(eventId) || Game1.eventsSeenSinceLastLocationChange.Contains(eventId)))
		{
			return false;
		}
		if (Game1.PlayEvent(eventId, Game1.currentLocation, out var validEvent, checkPreconditions, checkSeen))
		{
			return true;
		}
		if (validEvent)
		{
			return false;
		}
		foreach (GameLocation location in Game1.locations)
		{
			if (location != Game1.currentLocation)
			{
				if (Game1.PlayEvent(eventId, location, out validEvent, checkPreconditions, checkSeen))
				{
					return true;
				}
				if (validEvent)
				{
					return false;
				}
			}
		}
		return false;
	}

	public static int numberOfPlayers()
	{
		return Game1._onlineFarmers.Count;
	}

	public static bool isFestival()
	{
		return Game1.currentLocation?.currentEvent?.isFestival ?? false;
	}

	/// <summary>Parse a raw debug command and run it if it's valid.</summary>
	/// <param name="debugInput">The full debug command, including the command name and arguments.</param>
	/// <param name="log">The log to which to write command output, or <c>null</c> to use <see cref="F:StardewValley.Game1.log" />.</param>
	/// <returns>Returns whether the command was found and executed, regardless of whether the command logic succeeded.</returns>
	public bool parseDebugInput(string debugInput, IGameLogger log = null)
	{
		debugInput = debugInput.Trim();
		string[] command = ArgUtility.SplitBySpaceQuoteAware(debugInput);
		try
		{
			return DebugCommands.TryHandle(command, log);
		}
		catch (Exception ex)
		{
			Game1.log.Error("Debug command error.", ex);
			Game1.debugOutput = ex.Message;
			return false;
		}
	}

	public void RecountWalnuts()
	{
		if (!Game1.IsMasterGame || Game1.netWorldState.Value.ActivatedGoldenParrot || !(Game1.getLocationFromName("IslandHut") is IslandHut hut))
		{
			return;
		}
		int missing_nuts = hut.ShowNutHint();
		int current_nut_count = 130 - missing_nuts;
		Game1.netWorldState.Value.GoldenWalnutsFound = current_nut_count;
		foreach (GameLocation location in Game1.locations)
		{
			if (!(location is IslandLocation island_location))
			{
				continue;
			}
			foreach (ParrotUpgradePerch perch in island_location.parrotUpgradePerches)
			{
				if (perch.currentState.Value == ParrotUpgradePerch.UpgradeState.Complete)
				{
					current_nut_count -= perch.requiredNuts.Value;
				}
			}
		}
		if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoShortcutOut"))
		{
			current_nut_count -= 5;
		}
		if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge"))
		{
			current_nut_count -= 5;
		}
		Game1.netWorldState.Value.GoldenWalnuts = current_nut_count;
	}

	public void ResetIslandLocations()
	{
		Game1.netWorldState.Value.GoldenWalnutsFound = 0;
		Game1.player.team.collectedNutTracker.Clear();
		NetStringHashSet[] array = new NetStringHashSet[3]
		{
			Game1.player.mailReceived,
			Game1.player.mailForTomorrow,
			Game1.player.team.broadcastedMail
		};
		foreach (NetStringHashSet obj in array)
		{
			obj.Remove("birdieQuestBegun");
			obj.Remove("birdieQuestFinished");
			obj.Remove("tigerSlimeNut");
			obj.Remove("Island_W_BuriedTreasureNut");
			obj.Remove("Island_W_BuriedTreasure");
			obj.Remove("islandNorthCaveOpened");
			obj.Remove("Saw_Flame_Sprite_North_North");
			obj.Remove("Saw_Flame_Sprite_North_South");
			obj.Remove("Island_N_BuriedTreasureNut");
			obj.Remove("Island_W_BuriedTreasure");
			obj.Remove("Saw_Flame_Sprite_South");
			obj.Remove("Visited_Island");
			obj.Remove("Island_FirstParrot");
			obj.Remove("gotBirdieReward");
			obj.RemoveWhere((string key) => key.StartsWith("Island_Upgrade"));
		}
		Game1.player.secretNotesSeen.RemoveWhere((int id) => id >= GameLocation.JOURNAL_INDEX);
		Game1.player.team.limitedNutDrops.Clear();
		Game1.netWorldState.Value.GoldenCoconutCracked = false;
		Game1.netWorldState.Value.GoldenWalnuts = 0;
		Game1.netWorldState.Value.ParrotPlatformsUnlocked = false;
		Game1.netWorldState.Value.FoundBuriedNuts.Clear();
		for (int i2 = 0; i2 < Game1.locations.Count; i2++)
		{
			GameLocation location = Game1.locations[i2];
			if (location.InIslandContext())
			{
				Game1._locationLookup.Clear();
				string map_path = location.mapPath.Value;
				string location_name = location.name.Value;
				object[] args = new object[2] { map_path, location_name };
				try
				{
					Game1.locations[i2] = Activator.CreateInstance(location.GetType(), args) as GameLocation;
				}
				catch
				{
					Game1.locations[i2] = Activator.CreateInstance(location.GetType()) as GameLocation;
				}
				Game1._locationLookup.Clear();
			}
		}
		Game1.AddCharacterIfNecessary("Birdie");
	}

	public void ShowTelephoneMenu()
	{
		Game1.playSound("openBox");
		if (Game1.IsGreenRainingHere())
		{
			Game1.drawObjectDialogue("...................");
			return;
		}
		List<KeyValuePair<string, string>> responses = new List<KeyValuePair<string, string>>();
		foreach (IPhoneHandler handler in Phone.PhoneHandlers)
		{
			responses.AddRange(handler.GetOutgoingNumbers());
		}
		responses.Add(new KeyValuePair<string, string>("HangUp", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
		Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\Characters:Phone_SelectNumber"), responses, delegate(string callId)
		{
			if (callId == "HangUp")
			{
				Phone.HangUp();
			}
			else
			{
				foreach (IPhoneHandler phoneHandler in Phone.PhoneHandlers)
				{
					if (phoneHandler.TryHandleOutgoingCall(callId))
					{
						return;
					}
				}
				Phone.HangUp();
			}
		}, auto_select_single_choice: false, addCancel: false, 6);
	}

	public void requestDebugInput()
	{
		Game1.chatBox.activate();
		Game1.chatBox.setText("/");
	}

	private void panModeSuccess(KeyboardState currentKBState)
	{
		this.panFacingDirectionWait = false;
		Game1.playSound("smallSelect");
		if (currentKBState.IsKeyDown(Keys.LeftShift))
		{
			this.panModeString += " (animation_name_here)";
		}
		Game1.debugOutput = this.panModeString;
	}

	private void updatePanModeControls(MouseState currentMouseState, KeyboardState currentKBState)
	{
		if (currentKBState.IsKeyDown(Keys.F8) && !Game1.oldKBState.IsKeyDown(Keys.F8))
		{
			this.requestDebugInput();
			return;
		}
		if (!this.panFacingDirectionWait)
		{
			if (currentKBState.IsKeyDown(Keys.W))
			{
				Game1.viewport.Y -= 16;
			}
			if (currentKBState.IsKeyDown(Keys.A))
			{
				Game1.viewport.X -= 16;
			}
			if (currentKBState.IsKeyDown(Keys.S))
			{
				Game1.viewport.Y += 16;
			}
			if (currentKBState.IsKeyDown(Keys.D))
			{
				Game1.viewport.X += 16;
			}
		}
		else
		{
			if (currentKBState.IsKeyDown(Keys.W))
			{
				this.panModeString += "0";
				this.panModeSuccess(currentKBState);
			}
			if (currentKBState.IsKeyDown(Keys.A))
			{
				this.panModeString += "3";
				this.panModeSuccess(currentKBState);
			}
			if (currentKBState.IsKeyDown(Keys.S))
			{
				this.panModeString += "2";
				this.panModeSuccess(currentKBState);
			}
			if (currentKBState.IsKeyDown(Keys.D))
			{
				this.panModeString += "1";
				this.panModeSuccess(currentKBState);
			}
		}
		if (Game1.getMouseX(ui_scale: false) < 192)
		{
			Game1.viewport.X -= 8;
			Game1.viewport.X -= (192 - Game1.getMouseX()) / 8;
		}
		if (Game1.getMouseX(ui_scale: false) > Game1.viewport.Width - 192)
		{
			Game1.viewport.X += 8;
			Game1.viewport.X += (Game1.getMouseX() - Game1.viewport.Width + 192) / 8;
		}
		if (Game1.getMouseY(ui_scale: false) < 192)
		{
			Game1.viewport.Y -= 8;
			Game1.viewport.Y -= (192 - Game1.getMouseY()) / 8;
		}
		if (Game1.getMouseY(ui_scale: false) > Game1.viewport.Height - 192)
		{
			Game1.viewport.Y += 8;
			Game1.viewport.Y += (Game1.getMouseY() - Game1.viewport.Height + 192) / 8;
		}
		if (currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
		{
			string text = this.panModeString;
			if (text != null && text.Length > 0)
			{
				int x = (Game1.getMouseX() + Game1.viewport.X) / 64;
				int y = (Game1.getMouseY() + Game1.viewport.Y) / 64;
				this.panModeString = this.panModeString + Game1.currentLocation.Name + " " + x + " " + y + " ";
				this.panFacingDirectionWait = true;
				Game1.currentLocation.playTerrainSound(new Vector2(x, y));
				Game1.debugOutput = this.panModeString;
			}
		}
		if (currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released)
		{
			int x2 = Game1.getMouseX() + Game1.viewport.X;
			int y2 = Game1.getMouseY() + Game1.viewport.Y;
			Warp w = Game1.currentLocation.isCollidingWithWarpOrDoor(new Microsoft.Xna.Framework.Rectangle(x2, y2, 1, 1));
			if (w != null)
			{
				Game1.currentLocation = Game1.RequireLocation(w.TargetName);
				Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
				Game1.viewport.X = w.TargetX * 64 - Game1.viewport.Width / 2;
				Game1.viewport.Y = w.TargetY * 64 - Game1.viewport.Height / 2;
				Game1.playSound("dwop");
			}
		}
		if (currentKBState.IsKeyDown(Keys.Escape) && !Game1.oldKBState.IsKeyDown(Keys.Escape))
		{
			Warp w2 = Game1.currentLocation.warps[0];
			Game1.currentLocation = Game1.RequireLocation(w2.TargetName);
			Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
			Game1.viewport.X = w2.TargetX * 64 - Game1.viewport.Width / 2;
			Game1.viewport.Y = w2.TargetY * 64 - Game1.viewport.Height / 2;
			Game1.playSound("dwop");
		}
		if (Game1.viewport.X < -64)
		{
			Game1.viewport.X = -64;
		}
		if (Game1.viewport.X + Game1.viewport.Width > Game1.currentLocation.Map.Layers[0].LayerWidth * 64 + 128)
		{
			Game1.viewport.X = Game1.currentLocation.Map.Layers[0].LayerWidth * 64 + 128 - Game1.viewport.Width;
		}
		if (Game1.viewport.Y < -64)
		{
			Game1.viewport.Y = -64;
		}
		if (Game1.viewport.Y + Game1.viewport.Height > Game1.currentLocation.Map.Layers[0].LayerHeight * 64 + 128)
		{
			Game1.viewport.Y = Game1.currentLocation.Map.Layers[0].LayerHeight * 64 + 128 - Game1.viewport.Height;
		}
		Game1.oldMouseState = Game1.input.GetMouseState();
		Game1.oldKBState = currentKBState;
	}

	public static bool isLocationAccessible(string locationName)
	{
		switch (locationName)
		{
		case "Desert":
			if (Game1.MasterPlayer.mailReceived.Contains("ccVault"))
			{
				return true;
			}
			break;
		case "CommunityCenter":
			if (Game1.player.eventsSeen.Contains("191393"))
			{
				return true;
			}
			break;
		case "JojaMart":
			if (!Utility.HasAnyPlayerSeenEvent("191393"))
			{
				return true;
			}
			break;
		case "Railroad":
			if (Game1.stats.DaysPlayed > 31)
			{
				return true;
			}
			break;
		default:
			return true;
		}
		return false;
	}

	public static bool isDPadPressed()
	{
		return Game1.isDPadPressed(Game1.input.GetGamePadState());
	}

	public static bool isDPadPressed(GamePadState pad_state)
	{
		if (pad_state.DPad.Up == ButtonState.Pressed || pad_state.DPad.Down == ButtonState.Pressed || pad_state.DPad.Left == ButtonState.Pressed || pad_state.DPad.Right == ButtonState.Pressed)
		{
			return true;
		}
		return false;
	}

	public static bool isGamePadThumbstickInMotion(double threshold = 0.2)
	{
		bool inMotion = false;
		GamePadState p = Game1.input.GetGamePadState();
		if ((double)p.ThumbSticks.Left.X < 0.0 - threshold || p.IsButtonDown(Buttons.LeftThumbstickLeft))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Left.X > threshold || p.IsButtonDown(Buttons.LeftThumbstickRight))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Left.Y < 0.0 - threshold || p.IsButtonDown(Buttons.LeftThumbstickUp))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Left.Y > threshold || p.IsButtonDown(Buttons.LeftThumbstickDown))
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.X < 0.0 - threshold)
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.X > threshold)
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.Y < 0.0 - threshold)
		{
			inMotion = true;
		}
		if ((double)p.ThumbSticks.Right.Y > threshold)
		{
			inMotion = true;
		}
		if (inMotion)
		{
			Game1.thumbstickMotionMargin = 50;
		}
		return Game1.thumbstickMotionMargin > 0;
	}

	public static bool isAnyGamePadButtonBeingPressed()
	{
		return Utility.getPressedButtons(Game1.input.GetGamePadState(), Game1.oldPadState).Count > 0;
	}

	public static bool isAnyGamePadButtonBeingHeld()
	{
		return Utility.getHeldButtons(Game1.input.GetGamePadState()).Count > 0;
	}

	private static void UpdateChatBox()
	{
		if (Game1.chatBox == null)
		{
			return;
		}
		KeyboardState keyState = Game1.input.GetKeyboardState();
		GamePadState padState = Game1.input.GetGamePadState();
		if (Game1.IsChatting)
		{
			if (Game1.textEntry != null)
			{
				return;
			}
			if (padState.IsButtonDown(Buttons.A))
			{
				MouseState mouse = Game1.input.GetMouseState();
				if (Game1.chatBox != null && Game1.chatBox.isActive() && !Game1.chatBox.isHoveringOverClickable(mouse.X, mouse.Y))
				{
					Game1.oldPadState = padState;
					Game1.oldKBState = keyState;
					Game1.showTextEntry(Game1.chatBox.chatBox);
				}
			}
			if (keyState.IsKeyDown(Keys.Escape) || padState.IsButtonDown(Buttons.B) || padState.IsButtonDown(Buttons.Back))
			{
				Game1.chatBox.clickAway();
				Game1.oldKBState = keyState;
			}
		}
		else if (Game1.keyboardDispatcher.Subscriber == null && ((Game1.isOneOfTheseKeysDown(keyState, Game1.options.chatButton) && Game1.game1.HasKeyboardFocus()) || (!padState.IsButtonDown(Buttons.RightStick) && Game1.rightStickHoldTime > 0 && Game1.rightStickHoldTime < Game1.emoteMenuShowTime)))
		{
			Game1.chatBox.activate();
			if (keyState.IsKeyDown(Keys.OemQuestion))
			{
				Game1.chatBox.setText("/");
			}
		}
	}

	public static KeyboardState GetKeyboardState()
	{
		KeyboardState keyState = Game1.input.GetKeyboardState();
		if (Game1.chatBox != null)
		{
			if (Game1.IsChatting)
			{
				return default(KeyboardState);
			}
			if (Game1.keyboardDispatcher.Subscriber == null && Game1.isOneOfTheseKeysDown(keyState, Game1.options.chatButton) && Game1.game1.HasKeyboardFocus())
			{
				return default(KeyboardState);
			}
		}
		return keyState;
	}

	private void UpdateControlInput(GameTime time)
	{
		KeyboardState currentKBState = Game1.GetKeyboardState();
		MouseState currentMouseState = Game1.input.GetMouseState();
		GamePadState currentPadState = Game1.input.GetGamePadState();
		if (Game1.ticks < Game1._activatedTick + 2 && Game1.oldKBState.IsKeyDown(Keys.Tab) != currentKBState.IsKeyDown(Keys.Tab))
		{
			List<Keys> keys = Game1.oldKBState.GetPressedKeys().ToList();
			if (currentKBState.IsKeyDown(Keys.Tab))
			{
				keys.Add(Keys.Tab);
			}
			else
			{
				keys.Remove(Keys.Tab);
			}
			Game1.oldKBState = new KeyboardState(keys.ToArray());
		}
		Game1.hooks.OnGame1_UpdateControlInput(ref currentKBState, ref currentMouseState, ref currentPadState, delegate
		{
			if (Game1.options.gamepadControls)
			{
				bool flag = false;
				if (Math.Abs(currentPadState.ThumbSticks.Right.X) > 0f || Math.Abs(currentPadState.ThumbSticks.Right.Y) > 0f)
				{
					Game1.setMousePositionRaw((int)((float)currentMouseState.X + currentPadState.ThumbSticks.Right.X * Game1.thumbstickToMouseModifier), (int)((float)currentMouseState.Y - currentPadState.ThumbSticks.Right.Y * Game1.thumbstickToMouseModifier));
					flag = true;
				}
				if (Game1.IsChatting)
				{
					flag = true;
				}
				if (((Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY()) && Game1.getMouseX() != 0 && Game1.getMouseY() != 0) || flag)
				{
					if (flag)
					{
						if (Game1.timerUntilMouseFade <= 0)
						{
							Game1.lastMousePositionBeforeFade = new Point(this.localMultiplayerWindow.Width / 2, this.localMultiplayerWindow.Height / 2);
						}
					}
					else
					{
						Game1.lastCursorMotionWasMouse = true;
					}
					if (Game1.timerUntilMouseFade <= 0 && !Game1.lastCursorMotionWasMouse)
					{
						Game1.setMousePositionRaw(Game1.lastMousePositionBeforeFade.X, Game1.lastMousePositionBeforeFade.Y);
					}
					Game1.timerUntilMouseFade = 4000;
				}
			}
			else if (Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY())
			{
				Game1.lastCursorMotionWasMouse = true;
			}
			bool actionButtonPressed = false;
			bool switchToolButtonPressed = false;
			bool useToolButtonPressed = false;
			bool useToolButtonReleased = false;
			bool addItemToInventoryButtonPressed = false;
			bool cancelButtonPressed = false;
			bool moveUpPressed = false;
			bool moveRightPressed = false;
			bool moveLeftPressed = false;
			bool moveDownPressed = false;
			bool moveUpReleased = false;
			bool moveRightReleased = false;
			bool moveDownReleased = false;
			bool moveLeftReleased = false;
			bool moveUpHeld = false;
			bool moveRightHeld = false;
			bool moveDownHeld = false;
			bool moveLeftHeld = false;
			bool flag2 = false;
			if ((Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.actionButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.actionButton)) || (currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released))
			{
				actionButtonPressed = true;
				Game1.rightClickPolling = 250;
			}
			if ((Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.useToolButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.useToolButton)) || (currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released))
			{
				useToolButtonPressed = true;
			}
			if ((Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.useToolButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton)) || (currentMouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed))
			{
				useToolButtonReleased = true;
			}
			if (currentMouseState.ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue)
			{
				switchToolButtonPressed = true;
			}
			if ((Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.cancelButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.cancelButton)) || (currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released))
			{
				cancelButtonPressed = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveUpButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveUpButton))
			{
				moveUpPressed = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveRightButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveRightButton))
			{
				moveRightPressed = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveDownButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveDownButton))
			{
				moveDownPressed = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveLeftButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveLeftButton))
			{
				moveLeftPressed = true;
			}
			if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveUpButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
			{
				moveUpReleased = true;
			}
			if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveRightButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
			{
				moveRightReleased = true;
			}
			if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveDownButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
			{
				moveDownReleased = true;
			}
			if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveLeftButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
			{
				moveLeftReleased = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveUpButton))
			{
				moveUpHeld = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveRightButton))
			{
				moveRightHeld = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveDownButton))
			{
				moveDownHeld = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveLeftButton))
			{
				moveLeftHeld = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.useToolButton) || currentMouseState.LeftButton == ButtonState.Pressed)
			{
				flag2 = true;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.actionButton) || currentMouseState.RightButton == ButtonState.Pressed)
			{
				Game1.rightClickPolling -= time.ElapsedGameTime.Milliseconds;
				if (Game1.rightClickPolling <= 0)
				{
					Game1.rightClickPolling = 100;
					actionButtonPressed = true;
				}
			}
			if (Game1.options.gamepadControls)
			{
				if (currentKBState.GetPressedKeys().Length != 0 || currentMouseState.LeftButton == ButtonState.Pressed || currentMouseState.RightButton == ButtonState.Pressed)
				{
					Game1.timerUntilMouseFade = 4000;
				}
				if (currentPadState.IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))
				{
					actionButtonPressed = true;
					Game1.lastCursorMotionWasMouse = false;
					Game1.rightClickPolling = 250;
				}
				if (currentPadState.IsButtonDown(Buttons.X) && !Game1.oldPadState.IsButtonDown(Buttons.X))
				{
					useToolButtonPressed = true;
					Game1.lastCursorMotionWasMouse = false;
				}
				if (!currentPadState.IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonDown(Buttons.X))
				{
					useToolButtonReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.RightTrigger) && !Game1.oldPadState.IsButtonDown(Buttons.RightTrigger))
				{
					switchToolButtonPressed = true;
					Game1.triggerPolling = 300;
				}
				else if (currentPadState.IsButtonDown(Buttons.LeftTrigger) && !Game1.oldPadState.IsButtonDown(Buttons.LeftTrigger))
				{
					switchToolButtonPressed = true;
					Game1.triggerPolling = 300;
				}
				if (currentPadState.IsButtonDown(Buttons.X))
				{
					flag2 = true;
				}
				if (currentPadState.IsButtonDown(Buttons.A))
				{
					Game1.rightClickPolling -= time.ElapsedGameTime.Milliseconds;
					if (Game1.rightClickPolling <= 0)
					{
						Game1.rightClickPolling = 100;
						actionButtonPressed = true;
					}
				}
				if (currentPadState.IsButtonDown(Buttons.RightTrigger) || currentPadState.IsButtonDown(Buttons.LeftTrigger))
				{
					Game1.triggerPolling -= time.ElapsedGameTime.Milliseconds;
					if (Game1.triggerPolling <= 0)
					{
						Game1.triggerPolling = 100;
						switchToolButtonPressed = true;
					}
				}
				if (currentPadState.IsButtonDown(Buttons.RightShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.RightShoulder) && Game1.IsHudDrawn)
				{
					Game1.player.shiftToolbar(right: true);
				}
				if (currentPadState.IsButtonDown(Buttons.LeftShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.LeftShoulder) && Game1.IsHudDrawn)
				{
					Game1.player.shiftToolbar(right: false);
				}
				if (currentPadState.IsButtonDown(Buttons.DPadUp) && !Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveUpPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadUp) && Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveUpReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadRight) && !Game1.oldPadState.IsButtonDown(Buttons.DPadRight))
				{
					moveRightPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadRight) && Game1.oldPadState.IsButtonDown(Buttons.DPadRight))
				{
					moveRightReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadDown) && !Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
				{
					moveDownPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadDown) && Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
				{
					moveDownReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadLeft) && !Game1.oldPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveLeftPressed = true;
				}
				else if (!currentPadState.IsButtonDown(Buttons.DPadLeft) && Game1.oldPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveLeftReleased = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadUp))
				{
					moveUpHeld = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadRight))
				{
					moveRightHeld = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadDown))
				{
					moveDownHeld = true;
				}
				if (currentPadState.IsButtonDown(Buttons.DPadLeft))
				{
					moveLeftHeld = true;
				}
				if ((double)currentPadState.ThumbSticks.Left.X < -0.2)
				{
					moveLeftPressed = true;
					moveLeftHeld = true;
				}
				else if ((double)currentPadState.ThumbSticks.Left.X > 0.2)
				{
					moveRightPressed = true;
					moveRightHeld = true;
				}
				if ((double)currentPadState.ThumbSticks.Left.Y < -0.2)
				{
					moveDownPressed = true;
					moveDownHeld = true;
				}
				else if ((double)currentPadState.ThumbSticks.Left.Y > 0.2)
				{
					moveUpPressed = true;
					moveUpHeld = true;
				}
				if ((double)Game1.oldPadState.ThumbSticks.Left.X < -0.2 && !moveLeftHeld)
				{
					moveLeftReleased = true;
				}
				if ((double)Game1.oldPadState.ThumbSticks.Left.X > 0.2 && !moveRightHeld)
				{
					moveRightReleased = true;
				}
				if ((double)Game1.oldPadState.ThumbSticks.Left.Y < -0.2 && !moveDownHeld)
				{
					moveDownReleased = true;
				}
				if ((double)Game1.oldPadState.ThumbSticks.Left.Y > 0.2 && !moveUpHeld)
				{
					moveUpReleased = true;
				}
				if (this.controllerSlingshotSafeTime > 0f)
				{
					if (!currentPadState.IsButtonDown(Buttons.DPadUp) && !currentPadState.IsButtonDown(Buttons.DPadDown) && !currentPadState.IsButtonDown(Buttons.DPadLeft) && !currentPadState.IsButtonDown(Buttons.DPadRight) && (double)Math.Abs(currentPadState.ThumbSticks.Left.X) < 0.04 && (double)Math.Abs(currentPadState.ThumbSticks.Left.Y) < 0.04)
					{
						this.controllerSlingshotSafeTime = 0f;
					}
					if (this.controllerSlingshotSafeTime <= 0f)
					{
						this.controllerSlingshotSafeTime = 0f;
					}
					else
					{
						this.controllerSlingshotSafeTime -= (float)time.ElapsedGameTime.TotalSeconds;
						moveUpPressed = false;
						moveDownPressed = false;
						moveLeftPressed = false;
						moveRightPressed = false;
						moveUpHeld = false;
						moveDownHeld = false;
						moveLeftHeld = false;
						moveRightHeld = false;
					}
				}
			}
			else
			{
				this.controllerSlingshotSafeTime = 0f;
			}
			Game1.ResetFreeCursorDrag();
			if (flag2)
			{
				Game1.mouseClickPolling += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				Game1.mouseClickPolling = 0;
			}
			if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.toolbarSwap) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.toolbarSwap) && Game1.IsHudDrawn)
			{
				Game1.player.shiftToolbar(!currentKBState.IsKeyDown(Keys.LeftControl));
			}
			if (Game1.mouseClickPolling > 250 && (!(Game1.player.CurrentTool is FishingRod) || Game1.player.CurrentTool.upgradeLevel.Value <= 0))
			{
				useToolButtonPressed = true;
				Game1.mouseClickPolling = 100;
			}
			Game1.PushUIMode();
			foreach (IClickableMenu current in Game1.onScreenMenus)
			{
				if ((Game1.IsHudDrawn || current == Game1.chatBox) && Game1.wasMouseVisibleThisFrame && current.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
				{
					current.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
				}
			}
			Game1.PopUIMode();
			if (Game1.chatBox != null && Game1.chatBox.chatBox.Selected && Game1.oldMouseState.ScrollWheelValue != currentMouseState.ScrollWheelValue)
			{
				Game1.chatBox.receiveScrollWheelAction(currentMouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
			}
			if (Game1.panMode)
			{
				this.updatePanModeControls(currentMouseState, currentKBState);
			}
			else
			{
				if (Game1.inputSimulator != null)
				{
					if (currentKBState.IsKeyDown(Keys.Escape))
					{
						Game1.inputSimulator = null;
					}
					else
					{
						Game1.inputSimulator.SimulateInput(ref actionButtonPressed, ref switchToolButtonPressed, ref useToolButtonPressed, ref useToolButtonReleased, ref addItemToInventoryButtonPressed, ref cancelButtonPressed, ref moveUpPressed, ref moveRightPressed, ref moveLeftPressed, ref moveDownPressed, ref moveUpReleased, ref moveRightReleased, ref moveLeftReleased, ref moveDownReleased, ref moveUpHeld, ref moveRightHeld, ref moveLeftHeld, ref moveDownHeld);
					}
				}
				if (useToolButtonReleased && Game1.player.CurrentTool != null && Game1.CurrentEvent == null && Game1.pauseTime <= 0f && Game1.player.CurrentTool.onRelease(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(), Game1.player))
				{
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldKBState = currentKBState;
					Game1.oldPadState = currentPadState;
					Game1.player.usingSlingshot = false;
					Game1.player.canReleaseTool = true;
					Game1.player.UsingTool = false;
					Game1.player.CanMove = true;
				}
				else
				{
					if (((useToolButtonPressed && !Game1.isAnyGamePadButtonBeingPressed()) || (actionButtonPressed && Game1.isAnyGamePadButtonBeingPressed())) && Game1.pauseTime <= 0f && Game1.wasMouseVisibleThisFrame)
					{
						if (Game1.debugMode)
						{
							Console.WriteLine(Game1.getMouseX() + Game1.viewport.X + ", " + (Game1.getMouseY() + Game1.viewport.Y));
						}
						Game1.PushUIMode();
						foreach (IClickableMenu current2 in Game1.onScreenMenus)
						{
							if (Game1.IsHudDrawn || current2 == Game1.chatBox)
							{
								if ((!Game1.IsChatting || current2 == Game1.chatBox) && !(current2 is LevelUpMenu { informationUp: false }) && current2.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
								{
									current2.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
									Game1.PopUIMode();
									Game1.oldMouseState = Game1.input.GetMouseState();
									Game1.oldKBState = currentKBState;
									Game1.oldPadState = currentPadState;
									return;
								}
								if (current2 == Game1.chatBox && Game1.options.gamepadControls && Game1.IsChatting)
								{
									Game1.oldMouseState = Game1.input.GetMouseState();
									Game1.oldKBState = currentKBState;
									Game1.oldPadState = currentPadState;
									Game1.PopUIMode();
									return;
								}
								current2.clickAway();
							}
						}
						Game1.PopUIMode();
					}
					if (Game1.IsChatting || Game1.player.freezePause > 0)
					{
						if (Game1.IsChatting)
						{
							ButtonCollection.ButtonEnumerator enumerator2 = Utility.getPressedButtons(currentPadState, Game1.oldPadState).GetEnumerator();
							while (enumerator2.MoveNext())
							{
								Buttons current3 = enumerator2.Current;
								Game1.chatBox.receiveGamePadButton(current3);
							}
						}
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldKBState = currentKBState;
						Game1.oldPadState = currentPadState;
					}
					else
					{
						if (Game1.paused || Game1.HostPaused)
						{
							if (!Game1.HostPaused || !Game1.IsMasterGame || (!Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.menuButton) && !currentPadState.IsButtonDown(Buttons.B) && !currentPadState.IsButtonDown(Buttons.Back)))
							{
								Game1.oldMouseState = Game1.input.GetMouseState();
								return;
							}
							Game1.netWorldState.Value.IsPaused = false;
							Game1.chatBox?.globalInfoMessage("Resumed");
						}
						if (Game1.eventUp)
						{
							if (Game1.currentLocation.currentEvent == null && Game1.locationRequest == null)
							{
								Game1.eventUp = false;
							}
							else if (actionButtonPressed || useToolButtonPressed)
							{
								Game1.CurrentEvent?.receiveMouseClick(Game1.getMouseX(), Game1.getMouseY());
							}
						}
						bool flag3 = Game1.eventUp || Game1.farmEvent != null;
						if (actionButtonPressed || (Game1.dialogueUp && useToolButtonPressed))
						{
							Game1.PushUIMode();
							foreach (IClickableMenu current4 in Game1.onScreenMenus)
							{
								if (Game1.wasMouseVisibleThisFrame && (Game1.IsHudDrawn || current4 == Game1.chatBox) && current4.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) && !(current4 is LevelUpMenu { informationUp: false }))
								{
									current4.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
									Game1.oldMouseState = Game1.input.GetMouseState();
									if (!Game1.isAnyGamePadButtonBeingPressed())
									{
										Game1.PopUIMode();
										Game1.oldKBState = currentKBState;
										Game1.oldPadState = currentPadState;
										return;
									}
								}
							}
							Game1.PopUIMode();
							if (!Game1.pressActionButton(currentKBState, currentMouseState, currentPadState))
							{
								Game1.oldKBState = currentKBState;
								Game1.oldMouseState = Game1.input.GetMouseState();
								Game1.oldPadState = currentPadState;
								return;
							}
						}
						if (useToolButtonPressed && (!Game1.player.UsingTool || Game1.player.CurrentTool is MeleeWeapon) && !Game1.player.isEating && !Game1.dialogueUp && Game1.farmEvent == null && (Game1.player.CanMove || Game1.player.CurrentTool is MeleeWeapon))
						{
							if (Game1.player.CurrentTool != null && (!(Game1.player.CurrentTool is MeleeWeapon) || Game1.didPlayerJustLeftClick(ignoreNonMouseHeldInput: true)))
							{
								Game1.player.FireTool();
							}
							if (!Game1.pressUseToolButton() && Game1.player.canReleaseTool && Game1.player.UsingTool)
							{
								_ = Game1.player.CurrentTool;
							}
							if (Game1.player.UsingTool)
							{
								Game1.oldMouseState = Game1.input.GetMouseState();
								Game1.oldKBState = currentKBState;
								Game1.oldPadState = currentPadState;
								return;
							}
						}
						if (useToolButtonReleased && this._didInitiateItemStow)
						{
							this._didInitiateItemStow = false;
						}
						if (useToolButtonReleased && Game1.player.canReleaseTool && Game1.player.UsingTool && Game1.player.CurrentTool != null)
						{
							Game1.player.EndUsingTool();
						}
						if (switchToolButtonPressed && !Game1.player.UsingTool && !Game1.dialogueUp && Game1.player.CanMove && Game1.player.Items.HasAny() && !flag3)
						{
							Game1.pressSwitchToolButton();
						}
						if (Game1.player.CurrentTool != null && flag2 && Game1.player.canReleaseTool && !flag3 && !Game1.dialogueUp && Game1.player.Stamina >= 1f && !(Game1.player.CurrentTool is FishingRod))
						{
							int num = (Game1.player.CurrentTool.hasEnchantmentOfType<ReachingToolEnchantment>() ? 1 : 0);
							if (Game1.player.toolHold.Value <= 0 && Game1.player.CurrentTool.upgradeLevel.Value + num > Game1.player.toolPower.Value)
							{
								float num2 = 1f;
								if (Game1.player.CurrentTool != null)
								{
									num2 = Game1.player.CurrentTool.AnimationSpeedModifier;
								}
								Game1.player.toolHold.Value = (int)(600f * num2);
								Game1.player.toolHoldStartTime.Value = Game1.player.toolHold.Value;
							}
							else if (Game1.player.CurrentTool.upgradeLevel.Value + num > Game1.player.toolPower.Value)
							{
								Game1.player.toolHold.Value -= time.ElapsedGameTime.Milliseconds;
								if (Game1.player.toolHold.Value <= 0)
								{
									Game1.player.toolPowerIncrease();
								}
							}
						}
						if (Game1.upPolling >= 650f)
						{
							moveUpPressed = true;
							Game1.upPolling -= 100f;
						}
						else if (Game1.downPolling >= 650f)
						{
							moveDownPressed = true;
							Game1.downPolling -= 100f;
						}
						else if (Game1.rightPolling >= 650f)
						{
							moveRightPressed = true;
							Game1.rightPolling -= 100f;
						}
						else if (Game1.leftPolling >= 650f)
						{
							moveLeftPressed = true;
							Game1.leftPolling -= 100f;
						}
						else if (Game1.pauseTime <= 0f && Game1.locationRequest == null && (!Game1.player.UsingTool || Game1.player.canStrafeForToolUse()) && (!flag3 || (Game1.CurrentEvent != null && Game1.CurrentEvent.playerControlSequence)))
						{
							if (Game1.player.movementDirections.Count < 2)
							{
								if (moveUpHeld)
								{
									Game1.player.setMoving(1);
								}
								if (moveRightHeld)
								{
									Game1.player.setMoving(2);
								}
								if (moveDownHeld)
								{
									Game1.player.setMoving(4);
								}
								if (moveLeftHeld)
								{
									Game1.player.setMoving(8);
								}
							}
							if (moveUpReleased || (Game1.player.movementDirections.Contains(0) && !moveUpHeld))
							{
								Game1.player.setMoving(33);
								if (Game1.player.movementDirections.Count == 0)
								{
									Game1.player.setMoving(64);
								}
							}
							if (moveRightReleased || (Game1.player.movementDirections.Contains(1) && !moveRightHeld))
							{
								Game1.player.setMoving(34);
								if (Game1.player.movementDirections.Count == 0)
								{
									Game1.player.setMoving(64);
								}
							}
							if (moveDownReleased || (Game1.player.movementDirections.Contains(2) && !moveDownHeld))
							{
								Game1.player.setMoving(36);
								if (Game1.player.movementDirections.Count == 0)
								{
									Game1.player.setMoving(64);
								}
							}
							if (moveLeftReleased || (Game1.player.movementDirections.Contains(3) && !moveLeftHeld))
							{
								Game1.player.setMoving(40);
								if (Game1.player.movementDirections.Count == 0)
								{
									Game1.player.setMoving(64);
								}
							}
							if ((!moveUpHeld && !moveRightHeld && !moveDownHeld && !moveLeftHeld && !Game1.player.UsingTool) || Game1.activeClickableMenu != null)
							{
								Game1.player.Halt();
							}
						}
						else if (Game1.isQuestion)
						{
							if (moveUpPressed)
							{
								Game1.currentQuestionChoice = Math.Max(Game1.currentQuestionChoice - 1, 0);
								Game1.playSound("toolSwap");
							}
							else if (moveDownPressed)
							{
								Game1.currentQuestionChoice = Math.Min(Game1.currentQuestionChoice + 1, Game1.questionChoices.Count - 1);
								Game1.playSound("toolSwap");
							}
						}
						if (moveUpHeld && !Game1.player.CanMove)
						{
							Game1.upPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveDownHeld && !Game1.player.CanMove)
						{
							Game1.downPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveRightHeld && !Game1.player.CanMove)
						{
							Game1.rightPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveLeftHeld && !Game1.player.CanMove)
						{
							Game1.leftPolling += time.ElapsedGameTime.Milliseconds;
						}
						else if (moveUpReleased)
						{
							Game1.upPolling = 0f;
						}
						else if (moveDownReleased)
						{
							Game1.downPolling = 0f;
						}
						else if (moveRightReleased)
						{
							Game1.rightPolling = 0f;
						}
						else if (moveLeftReleased)
						{
							Game1.leftPolling = 0f;
						}
						if (Game1.debugMode)
						{
							if (currentKBState.IsKeyDown(Keys.Q))
							{
								Game1.oldKBState.IsKeyDown(Keys.Q);
							}
							if (currentKBState.IsKeyDown(Keys.P) && !Game1.oldKBState.IsKeyDown(Keys.P))
							{
								Game1.NewDay(0f);
							}
							if (currentKBState.IsKeyDown(Keys.M) && !Game1.oldKBState.IsKeyDown(Keys.M))
							{
								Game1.dayOfMonth = 28;
								Game1.NewDay(0f);
							}
							if (currentKBState.IsKeyDown(Keys.T) && !Game1.oldKBState.IsKeyDown(Keys.T))
							{
								Game1.addHour();
							}
							if (currentKBState.IsKeyDown(Keys.Y) && !Game1.oldKBState.IsKeyDown(Keys.Y))
							{
								Game1.addMinute();
							}
							if (currentKBState.IsKeyDown(Keys.D1) && !Game1.oldKBState.IsKeyDown(Keys.D1))
							{
								Game1.warpFarmer("Mountain", 15, 35, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D2) && !Game1.oldKBState.IsKeyDown(Keys.D2))
							{
								Game1.warpFarmer("Town", 35, 35, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D3) && !Game1.oldKBState.IsKeyDown(Keys.D3))
							{
								Game1.warpFarmer("Farm", 64, 15, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D4) && !Game1.oldKBState.IsKeyDown(Keys.D4))
							{
								Game1.warpFarmer("Forest", 34, 13, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D5) && !Game1.oldKBState.IsKeyDown(Keys.D4))
							{
								Game1.warpFarmer("Beach", 34, 10, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D6) && !Game1.oldKBState.IsKeyDown(Keys.D6))
							{
								Game1.warpFarmer("Mine", 18, 12, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.D7) && !Game1.oldKBState.IsKeyDown(Keys.D7))
							{
								Game1.warpFarmer("SandyHouse", 16, 3, flip: false);
							}
							if (currentKBState.IsKeyDown(Keys.K) && !Game1.oldKBState.IsKeyDown(Keys.K))
							{
								Game1.enterMine(Game1.mine.mineLevel + 1);
							}
							if (currentKBState.IsKeyDown(Keys.H) && !Game1.oldKBState.IsKeyDown(Keys.H))
							{
								Game1.player.changeHat(Game1.random.Next(FarmerRenderer.hatsTexture.Height / 80 * 12));
							}
							if (currentKBState.IsKeyDown(Keys.I) && !Game1.oldKBState.IsKeyDown(Keys.I))
							{
								Game1.player.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
							}
							if (currentKBState.IsKeyDown(Keys.J) && !Game1.oldKBState.IsKeyDown(Keys.J))
							{
								Game1.player.changeShirt(Game1.random.Next(1000, 1040).ToString());
								Game1.player.changePantsColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
							}
							if (currentKBState.IsKeyDown(Keys.L) && !Game1.oldKBState.IsKeyDown(Keys.L))
							{
								Game1.player.changeShirt(Game1.random.Next(1000, 1040).ToString());
								Game1.player.changePantsColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
								Game1.player.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
								if (Game1.random.NextBool())
								{
									Game1.player.changeHat(Game1.random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
								}
								else
								{
									Game1.player.changeHat(-1);
								}
								Game1.player.changeHairColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
								Game1.player.changeSkinColor(Game1.random.Next(16));
							}
							if (currentKBState.IsKeyDown(Keys.U) && !Game1.oldKBState.IsKeyDown(Keys.U))
							{
								FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>("FarmHouse");
								farmHouse.SetWallpaper(Game1.random.Next(112).ToString(), null);
								farmHouse.SetFloor(Game1.random.Next(40).ToString(), null);
							}
							if (currentKBState.IsKeyDown(Keys.F2))
							{
								Game1.oldKBState.IsKeyDown(Keys.F2);
							}
							if (currentKBState.IsKeyDown(Keys.F5) && !Game1.oldKBState.IsKeyDown(Keys.F5))
							{
								Game1.displayFarmer = !Game1.displayFarmer;
							}
							if (currentKBState.IsKeyDown(Keys.F6))
							{
								Game1.oldKBState.IsKeyDown(Keys.F6);
							}
							if (currentKBState.IsKeyDown(Keys.F7) && !Game1.oldKBState.IsKeyDown(Keys.F7))
							{
								Game1.drawGrid = !Game1.drawGrid;
							}
							if (currentKBState.IsKeyDown(Keys.B) && !Game1.oldKBState.IsKeyDown(Keys.B) && Game1.IsHudDrawn)
							{
								Game1.player.shiftToolbar(right: false);
							}
							if (currentKBState.IsKeyDown(Keys.N) && !Game1.oldKBState.IsKeyDown(Keys.N) && Game1.IsHudDrawn)
							{
								Game1.player.shiftToolbar(right: true);
							}
							if (currentKBState.IsKeyDown(Keys.F10) && !Game1.oldKBState.IsKeyDown(Keys.F10) && Game1.server == null)
							{
								Game1.multiplayer.StartServer();
							}
						}
						else if (!Game1.player.UsingTool)
						{
							if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot1) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot1))
							{
								Game1.player.CurrentToolIndex = 0;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot2) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot2))
							{
								Game1.player.CurrentToolIndex = 1;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot3) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot3))
							{
								Game1.player.CurrentToolIndex = 2;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot4) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot4))
							{
								Game1.player.CurrentToolIndex = 3;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot5) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot5))
							{
								Game1.player.CurrentToolIndex = 4;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot6) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot6))
							{
								Game1.player.CurrentToolIndex = 5;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot7) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot7))
							{
								Game1.player.CurrentToolIndex = 6;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot8) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot8))
							{
								Game1.player.CurrentToolIndex = 7;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot9) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot9))
							{
								Game1.player.CurrentToolIndex = 8;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot10) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot10))
							{
								Game1.player.CurrentToolIndex = 9;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot11) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot11))
							{
								Game1.player.CurrentToolIndex = 10;
							}
							else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot12) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot12))
							{
								Game1.player.CurrentToolIndex = 11;
							}
						}
						if (((Game1.options.gamepadControls && Game1.rightStickHoldTime >= Game1.emoteMenuShowTime && Game1.activeClickableMenu == null) || (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.emoteButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.emoteButton))) && !Game1.debugMode && Game1.player.CanEmote())
						{
							if (Game1.player.CanMove)
							{
								Game1.player.Halt();
							}
							Game1.emoteMenu = new EmoteMenu();
							Game1.emoteMenu.gamepadMode = Game1.options.gamepadControls && Game1.rightStickHoldTime >= Game1.emoteMenuShowTime;
							Game1.timerUntilMouseFade = 0;
						}
						if (!Program.releaseBuild)
						{
							if (Game1.IsPressEvent(ref currentKBState, Keys.F3) || Game1.IsPressEvent(ref currentPadState, Buttons.LeftStick))
							{
								Game1.debugMode = !Game1.debugMode;
								if (Game1.gameMode == 11)
								{
									Game1.gameMode = 3;
								}
							}
							if (Game1.IsPressEvent(ref currentKBState, Keys.F8))
							{
								this.requestDebugInput();
							}
						}
						if (currentKBState.IsKeyDown(Keys.F4) && !Game1.oldKBState.IsKeyDown(Keys.F4))
						{
							Game1.displayHUD = !Game1.displayHUD;
							Game1.playSound("smallSelect");
							if (!Game1.displayHUD)
							{
								Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3666"));
							}
						}
						bool flag4 = Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.menuButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.menuButton);
						bool flag5 = Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.journalButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.journalButton);
						bool flag6 = Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.mapButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.mapButton);
						if (Game1.options.gamepadControls && !flag4)
						{
							flag4 = (currentPadState.IsButtonDown(Buttons.Start) && !Game1.oldPadState.IsButtonDown(Buttons.Start)) || (currentPadState.IsButtonDown(Buttons.B) && !Game1.oldPadState.IsButtonDown(Buttons.B));
						}
						if (Game1.options.gamepadControls && !flag5)
						{
							flag5 = currentPadState.IsButtonDown(Buttons.Back) && !Game1.oldPadState.IsButtonDown(Buttons.Back);
						}
						if (Game1.options.gamepadControls && !flag6)
						{
							flag6 = currentPadState.IsButtonDown(Buttons.Y) && !Game1.oldPadState.IsButtonDown(Buttons.Y);
						}
						if (flag4 && Game1.CanShowPauseMenu())
						{
							if (Game1.activeClickableMenu == null)
							{
								Game1.PushUIMode();
								Game1.activeClickableMenu = new GameMenu();
								Game1.PopUIMode();
							}
							else if (Game1.activeClickableMenu.readyToClose())
							{
								Game1.exitActiveMenu();
							}
						}
						if (Game1.dayOfMonth > 0 && Game1.player.CanMove && flag5 && !Game1.dialogueUp && !flag3)
						{
							if (Game1.activeClickableMenu == null)
							{
								Game1.activeClickableMenu = new QuestLog();
							}
						}
						else if (flag3 && Game1.CurrentEvent != null && flag5 && !Game1.CurrentEvent.skipped && Game1.CurrentEvent.skippable)
						{
							Game1.CurrentEvent.skipped = true;
							Game1.CurrentEvent.skipEvent();
							Game1.freezeControls = false;
						}
						if (Game1.options.gamepadControls && Game1.dayOfMonth > 0 && Game1.player.CanMove && Game1.isAnyGamePadButtonBeingPressed() && flag6 && !Game1.dialogueUp && !flag3)
						{
							if (Game1.activeClickableMenu == null)
							{
								Game1.PushUIMode();
								Game1.activeClickableMenu = new GameMenu(GameMenu.craftingTab);
								Game1.PopUIMode();
							}
						}
						else if (Game1.dayOfMonth > 0 && Game1.player.CanMove && flag6 && !Game1.dialogueUp && !flag3 && Game1.activeClickableMenu == null)
						{
							Game1.PushUIMode();
							Game1.activeClickableMenu = new GameMenu(GameMenu.mapTab);
							Game1.PopUIMode();
						}
						Game1.checkForRunButton(currentKBState);
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
					}
				}
			}
		});
	}

	public static bool CanShowPauseMenu()
	{
		if (Game1.dayOfMonth > 0 && Game1.player.CanMove && !Game1.dialogueUp && (!Game1.eventUp || (Game1.isFestival() && Game1.CurrentEvent.festivalTimer <= 0)) && Game1.currentMinigame == null)
		{
			return Game1.farmEvent == null;
		}
		return false;
	}

	internal static void addHour()
	{
		Game1.timeOfDay += 100;
		foreach (GameLocation g in Game1.locations)
		{
			for (int i = 0; i < g.characters.Count; i++)
			{
				NPC nPC = g.characters[i];
				nPC.checkSchedule(Game1.timeOfDay);
				nPC.checkSchedule(Game1.timeOfDay - 50);
				nPC.checkSchedule(Game1.timeOfDay - 60);
				nPC.checkSchedule(Game1.timeOfDay - 70);
				nPC.checkSchedule(Game1.timeOfDay - 80);
				nPC.checkSchedule(Game1.timeOfDay - 90);
			}
		}
		switch (Game1.timeOfDay)
		{
		case 1900:
			Game1.currentLocation.switchOutNightTiles();
			break;
		case 2000:
			if (!Game1.currentLocation.IsRainingHere())
			{
				Game1.changeMusicTrack("none");
			}
			break;
		}
	}

	internal static void addMinute()
	{
		if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
		{
			Game1.timeOfDay -= 10;
		}
		else
		{
			Game1.timeOfDay += 10;
		}
		if (Game1.timeOfDay % 100 == 60)
		{
			Game1.timeOfDay += 40;
		}
		if (Game1.timeOfDay % 100 == 90)
		{
			Game1.timeOfDay -= 40;
		}
		Game1.currentLocation.performTenMinuteUpdate(Game1.timeOfDay);
		foreach (GameLocation g in Game1.locations)
		{
			for (int i = 0; i < g.characters.Count; i++)
			{
				g.characters[i].checkSchedule(Game1.timeOfDay);
			}
		}
		if (Game1.isLightning && Game1.IsMasterGame)
		{
			Utility.performLightningUpdate(Game1.timeOfDay);
		}
		switch (Game1.timeOfDay)
		{
		case 1750:
			Game1.outdoorLight = Color.White;
			break;
		case 1900:
			Game1.currentLocation.switchOutNightTiles();
			break;
		case 2000:
			if (!Game1.currentLocation.IsRainingHere())
			{
				Game1.changeMusicTrack("none");
			}
			break;
		}
	}

	public static void checkForRunButton(KeyboardState kbState, bool ignoreKeyPressQualifier = false)
	{
		bool wasRunning = Game1.player.running;
		bool runPressed = Game1.isOneOfTheseKeysDown(kbState, Game1.options.runButton) && (!Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.runButton) || ignoreKeyPressQualifier);
		bool runReleased = !Game1.isOneOfTheseKeysDown(kbState, Game1.options.runButton) && (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.runButton) || ignoreKeyPressQualifier);
		if (Game1.options.gamepadControls)
		{
			if (!Game1.options.autoRun && Math.Abs(Vector2.Distance(Game1.input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) > 0.9f)
			{
				runPressed = true;
			}
			else if (Math.Abs(Vector2.Distance(Game1.oldPadState.ThumbSticks.Left, Vector2.Zero)) > 0.9f && Math.Abs(Vector2.Distance(Game1.input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) <= 0.9f)
			{
				runReleased = true;
			}
		}
		if (runPressed && !Game1.player.canOnlyWalk)
		{
			Game1.player.setRunning(!Game1.options.autoRun);
			Game1.player.setMoving((byte)(Game1.player.running ? 16u : 48u));
		}
		else if (runReleased && !Game1.player.canOnlyWalk)
		{
			Game1.player.setRunning(Game1.options.autoRun);
			Game1.player.setMoving((byte)(Game1.player.running ? 16u : 48u));
		}
		if (Game1.player.running != wasRunning && !Game1.player.UsingTool)
		{
			Game1.player.Halt();
		}
	}

	public static Vector2 getMostRecentViewportMotion()
	{
		return new Vector2((float)Game1.viewport.X - Game1.previousViewportPosition.X, (float)Game1.viewport.Y - Game1.previousViewportPosition.Y);
	}

	protected virtual void DrawOverlays(GameTime time, RenderTarget2D target_screen)
	{
		if (this.takingMapScreenshot)
		{
			return;
		}
		Game1.PushUIMode();
		Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (Game1.hooks.OnRendering(RenderSteps.Overlays, Game1.spriteBatch, time, target_screen))
		{
			Game1.specialCurrencyDisplay?.Draw(Game1.spriteBatch);
			Game1.emoteMenu?.draw(Game1.spriteBatch);
			Game1.currentLocation?.drawOverlays(Game1.spriteBatch);
			if (Game1.HostPaused && !this.takingMapScreenshot)
			{
				string msg = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
				SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, msg, 96, 32);
			}
			if (Game1.overlayMenu != null)
			{
				if (Game1.hooks.OnRendering(RenderSteps.Overlays_OverlayMenu, Game1.spriteBatch, time, target_screen))
				{
					Game1.overlayMenu.draw(Game1.spriteBatch);
				}
				Game1.hooks.OnRendered(RenderSteps.Overlays_OverlayMenu, Game1.spriteBatch, time, target_screen);
			}
			if (Game1.chatBox != null)
			{
				if (Game1.hooks.OnRendering(RenderSteps.Overlays_Chatbox, Game1.spriteBatch, time, target_screen))
				{
					Game1.chatBox.update(Game1.currentGameTime);
					Game1.chatBox.draw(Game1.spriteBatch);
				}
				Game1.hooks.OnRendered(RenderSteps.Overlays_Chatbox, Game1.spriteBatch, time, target_screen);
			}
			if (Game1.textEntry != null)
			{
				if (Game1.hooks.OnRendering(RenderSteps.Overlays_OnscreenKeyboard, Game1.spriteBatch, time, target_screen))
				{
					Game1.textEntry.draw(Game1.spriteBatch);
				}
				Game1.hooks.OnRendered(RenderSteps.Overlays_OnscreenKeyboard, Game1.spriteBatch, time, target_screen);
			}
			if ((Game1.displayHUD || Game1.eventUp || Game1.currentLocation is Summit) && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode)
			{
				this.drawMouseCursor();
			}
		}
		Game1.hooks.OnRendered(RenderSteps.Overlays, Game1.spriteBatch, time, target_screen);
		Game1.spriteBatch.End();
		Game1.PopUIMode();
	}

	public static void setBGColor(byte r, byte g, byte b)
	{
		Game1.bgColor.R = r;
		Game1.bgColor.G = g;
		Game1.bgColor.B = b;
	}

	public void Instance_Draw(GameTime gameTime)
	{
		this.Draw(gameTime);
	}

	/// <summary>
	/// This is called when the game should draw itself.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	protected override void Draw(GameTime gameTime)
	{
		this.isDrawing = true;
		RenderTarget2D target_screen = null;
		if (this.ShouldDrawOnBuffer())
		{
			target_screen = this.screen;
		}
		if (this.uiScreen != null)
		{
			Game1.SetRenderTarget(this.uiScreen);
			base.GraphicsDevice.Clear(Color.Transparent);
			Game1.SetRenderTarget(target_screen);
		}
		GameTime time = gameTime;
		DebugTools.BeforeGameDraw(this, ref time);
		this._draw(time, target_screen);
		Game1.isRenderingScreenBuffer = true;
		this.renderScreenBuffer(target_screen);
		Game1.isRenderingScreenBuffer = false;
		if (Game1.uiModeCount != 0)
		{
			Game1.log.Warn("WARNING: Mismatched UI Mode Push/Pop counts. Correcting.");
			while (Game1.uiModeCount < 0)
			{
				Game1.PushUIMode();
			}
			while (Game1.uiModeCount > 0)
			{
				Game1.PopUIMode();
			}
		}
		base.Draw(gameTime);
		this.isDrawing = false;
	}

	public virtual bool ShouldDrawOnBuffer()
	{
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			return true;
		}
		if (Game1.options.zoomLevel != 1f)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldShowOnscreenUsernames()
	{
		return false;
	}

	public virtual bool checkCharacterTilesForShadowDrawFlag(Character character)
	{
		if (character is Farmer farmer && farmer.onBridge.Value)
		{
			return true;
		}
		Microsoft.Xna.Framework.Rectangle bounding_box = character.GetBoundingBox();
		bounding_box.Height += 8;
		int right = bounding_box.Right / 64;
		int bottom = bounding_box.Bottom / 64;
		int num = bounding_box.Left / 64;
		int top = bounding_box.Top / 64;
		for (int x = num; x <= right; x++)
		{
			for (int y = top; y <= bottom; y++)
			{
				if (Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(new Vector2(x, y)))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void _draw(GameTime gameTime, RenderTarget2D target_screen)
	{
		Game1.debugTimings.StartDrawTimer();
		Game1.showingHealthBar = false;
		if (Game1._newDayTask != null || this.isLocalMultiplayerNewDayActive || this.ShouldLoadIncrementally)
		{
			base.GraphicsDevice.Clear(Game1.bgColor);
			return;
		}
		if (target_screen != null)
		{
			Game1.SetRenderTarget(target_screen);
		}
		if (this.IsSaving)
		{
			base.GraphicsDevice.Clear(Game1.bgColor);
			this.DrawMenu(gameTime, target_screen);
			Game1.PushUIMode();
			if (Game1.overlayMenu != null)
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.overlayMenu.draw(Game1.spriteBatch);
				Game1.spriteBatch.End();
			}
			Game1.PopUIMode();
			return;
		}
		base.GraphicsDevice.Clear(Game1.bgColor);
		if (Game1.hooks.OnRendering(RenderSteps.FullScene, Game1.spriteBatch, gameTime, target_screen))
		{
			if (Game1.gameMode == 11)
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
				Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, 255, 0));
				Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
				Game1.spriteBatch.End();
				return;
			}
			bool draw_world = true;
			if (Game1.activeClickableMenu != null && Game1.options.showMenuBackground && Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !this.takingMapScreenshot)
			{
				Game1.PushUIMode();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (Game1.hooks.OnRendering(RenderSteps.MenuBackground, Game1.spriteBatch, gameTime, target_screen))
				{
					Game1.activeClickableMenu.drawBackground(Game1.spriteBatch);
					draw_world = false;
				}
				Game1.hooks.OnRendered(RenderSteps.MenuBackground, Game1.spriteBatch, gameTime, target_screen);
				Game1.spriteBatch.End();
				Game1.PopUIMode();
			}
			if (Game1.currentMinigame != null)
			{
				if (Game1.hooks.OnRendering(RenderSteps.Minigame, Game1.spriteBatch, gameTime, target_screen))
				{
					Game1.currentMinigame.draw(Game1.spriteBatch);
					draw_world = false;
				}
				Game1.hooks.OnRendered(RenderSteps.Minigame, Game1.spriteBatch, gameTime, target_screen);
			}
			if (Game1.gameMode == 6 || (Game1.gameMode == 3 && Game1.currentLocation == null))
			{
				if (Game1.hooks.OnRendering(RenderSteps.LoadingScreen, Game1.spriteBatch, gameTime, target_screen))
				{
					this.DrawLoadScreen(gameTime, target_screen);
				}
				Game1.hooks.OnRendered(RenderSteps.LoadingScreen, Game1.spriteBatch, gameTime, target_screen);
				draw_world = false;
			}
			if (Game1.showingEndOfNightStuff)
			{
				draw_world = false;
			}
			else if (Game1.gameMode == 0)
			{
				draw_world = false;
			}
			if (Game1.gameMode == 3 && Game1.dayOfMonth == 0 && Game1.newDay)
			{
				base.Draw(gameTime);
				return;
			}
			if (draw_world)
			{
				this.DrawWorld(gameTime, target_screen);
				Game1.PushUIMode();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (Game1.hooks.OnRendering(RenderSteps.HUD, Game1.spriteBatch, gameTime, target_screen))
				{
					if (Game1.IsHudDrawn)
					{
						this.drawHUD();
					}
					if (Game1.hudMessages.Count > 0 && !this.takingMapScreenshot)
					{
						int heightUsed = 0;
						for (int i = Game1.hudMessages.Count - 1; i >= 0; i--)
						{
							Game1.hudMessages[i].draw(Game1.spriteBatch, i, ref heightUsed);
						}
					}
				}
				Game1.hooks.OnRendered(RenderSteps.HUD, Game1.spriteBatch, gameTime, target_screen);
				Game1.debugTimings.Draw();
				Game1.spriteBatch.End();
				Game1.PopUIMode();
			}
			bool draw_dialogue_box_after_fade = false;
			if (!this.takingMapScreenshot)
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.PushUIMode();
				if ((Game1.messagePause || Game1.globalFade) && Game1.dialogueUp)
				{
					draw_dialogue_box_after_fade = true;
				}
				else if (Game1.dialogueUp && !Game1.messagePause && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox)))
				{
					if (Game1.hooks.OnRendering(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen))
					{
						this.drawDialogueBox();
					}
					Game1.hooks.OnRendered(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen);
				}
				Game1.spriteBatch.End();
				Game1.PopUIMode();
				this.DrawGlobalFade(gameTime, target_screen);
				if (draw_dialogue_box_after_fade)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					Game1.PushUIMode();
					if (Game1.hooks.OnRendering(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen))
					{
						this.drawDialogueBox();
					}
					Game1.hooks.OnRendered(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen);
					Game1.spriteBatch.End();
					Game1.PopUIMode();
				}
				this.DrawScreenOverlaySprites(gameTime, target_screen);
				if (Game1.debugMode)
				{
					this.DrawDebugUIs(gameTime, target_screen);
				}
				this.DrawMenu(gameTime, target_screen);
			}
			Game1.farmEvent?.drawAboveEverything(Game1.spriteBatch);
			this.DrawOverlays(gameTime, target_screen);
		}
		Game1.hooks.OnRendered(RenderSteps.FullScene, Game1.spriteBatch, gameTime, target_screen);
		Game1.debugTimings.StopDrawTimer();
	}

	public virtual void DrawLoadScreen(GameTime time, RenderTarget2D target_screen)
	{
		Game1.PushUIMode();
		base.GraphicsDevice.Clear(Game1.bgColor);
		Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		string addOn = "".PadRight((int)Math.Ceiling(time.TotalGameTime.TotalMilliseconds % 999.0 / 333.0), '.');
		string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
		string msg = text + addOn;
		string largestMessage = text + "... ";
		int msgw = SpriteText.getWidthOfString(largestMessage);
		int msgh = 64;
		int msgx = 64;
		int msgy = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - msgh;
		SpriteText.drawString(Game1.spriteBatch, msg, msgx, msgy, 999999, msgw, msgh, 1f, 0.88f, junimoText: false, 0, largestMessage);
		Game1.spriteBatch.End();
		Game1.PopUIMode();
	}

	public virtual void DrawMenu(GameTime time, RenderTarget2D target_screen)
	{
		Game1.PushUIMode();
		Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (Game1.hooks.OnRendering(RenderSteps.Menu, Game1.spriteBatch, time, target_screen))
		{
			IClickableMenu menu = Game1.activeClickableMenu;
			while (menu != null && Game1.hooks.TryDrawMenu(menu, delegate
			{
				menu.draw(Game1.spriteBatch);
			}))
			{
				menu = menu.GetChildMenu();
			}
		}
		Game1.hooks.OnRendered(RenderSteps.Menu, Game1.spriteBatch, time, target_screen);
		Game1.spriteBatch.End();
		Game1.PopUIMode();
	}

	public virtual void DrawScreenOverlaySprites(GameTime time, RenderTarget2D target_screen)
	{
		if (Game1.hooks.OnRendering(RenderSteps.OverlayTemporarySprites, Game1.spriteBatch, time, target_screen))
		{
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			foreach (TemporaryAnimatedSprite screenOverlayTempSprite in Game1.screenOverlayTempSprites)
			{
				screenOverlayTempSprite.draw(Game1.spriteBatch, localPosition: true);
			}
			Game1.spriteBatch.End();
			Game1.PushUIMode();
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			foreach (TemporaryAnimatedSprite uiOverlayTempSprite in Game1.uiOverlayTempSprites)
			{
				uiOverlayTempSprite.draw(Game1.spriteBatch, localPosition: true);
			}
			Game1.spriteBatch.End();
			Game1.PopUIMode();
		}
		Game1.hooks.OnRendered(RenderSteps.OverlayTemporarySprites, Game1.spriteBatch, time, target_screen);
	}

	public virtual void DrawWorld(GameTime time, RenderTarget2D target_screen)
	{
		if (Game1.hooks.OnRendering(RenderSteps.World, Game1.spriteBatch, time, target_screen))
		{
			Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
			if (Game1.drawLighting)
			{
				this.DrawLighting(time, target_screen);
			}
			base.GraphicsDevice.Clear(Game1.bgColor);
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (Game1.hooks.OnRendering(RenderSteps.World_Background, Game1.spriteBatch, time, target_screen))
			{
				Game1.background?.draw(Game1.spriteBatch);
				Game1.currentLocation.drawBackground(Game1.spriteBatch);
				Game1.spriteBatch.End();
				for (int i = 0; i < Game1.currentLocation.backgroundLayers.Count; i++)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
					Game1.currentLocation.backgroundLayers[i].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4, -1f);
					Game1.spriteBatch.End();
				}
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.currentLocation.drawWater(Game1.spriteBatch);
				Game1.spriteBatch.End();
				Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.currentLocation.drawFloorDecorations(Game1.spriteBatch);
				Game1.spriteBatch.End();
			}
			Game1.hooks.OnRendered(RenderSteps.World_Background, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			this._farmerShadows.Clear();
			if (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.isFestival && Game1.currentLocation.currentEvent.farmerActors.Count > 0)
			{
				foreach (Farmer f in Game1.currentLocation.currentEvent.farmerActors)
				{
					if ((f.IsLocalPlayer && Game1.displayFarmer) || !f.hidden.Value)
					{
						this._farmerShadows.Add(f);
					}
				}
			}
			else
			{
				foreach (Farmer f2 in Game1.currentLocation.farmers)
				{
					if ((f2.IsLocalPlayer && Game1.displayFarmer) || !f2.hidden.Value)
					{
						this._farmerShadows.Add(f2);
					}
				}
			}
			if (!Game1.currentLocation.shouldHideCharacters())
			{
				if (Game1.CurrentEvent == null)
				{
					foreach (NPC n in Game1.currentLocation.characters)
					{
						if (!n.swimming.Value && !n.HideShadow && !n.IsInvisible && !this.checkCharacterTilesForShadowDrawFlag(n))
						{
							n.DrawShadow(Game1.spriteBatch);
						}
					}
				}
				else
				{
					foreach (NPC n2 in Game1.CurrentEvent.actors)
					{
						if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(n2)) && !n2.swimming.Value && !n2.HideShadow && !this.checkCharacterTilesForShadowDrawFlag(n2))
						{
							n2.DrawShadow(Game1.spriteBatch);
						}
					}
				}
				foreach (Farmer f3 in this._farmerShadows)
				{
					if (!Game1.multiplayer.isDisconnecting(f3.UniqueMultiplayerID) && !f3.swimming.Value && !f3.isRidingHorse() && !f3.IsSitting() && (Game1.currentLocation == null || !this.checkCharacterTilesForShadowDrawFlag(f3)))
					{
						f3.DrawShadow(Game1.spriteBatch);
					}
				}
			}
			float layer_sub_sort = 0.1f;
			for (int j = 0; j < Game1.currentLocation.buildingLayers.Count; j++)
			{
				float layer = 0f;
				if (Game1.currentLocation.buildingLayers.Count > 1)
				{
					layer = (float)j / (float)(Game1.currentLocation.buildingLayers.Count - 1);
				}
				Game1.currentLocation.buildingLayers[j].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4, layer_sub_sort * layer);
			}
			Layer building_layer = Game1.currentLocation.Map.RequireLayer("Buildings");
			Game1.spriteBatch.End();
			Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (Game1.hooks.OnRendering(RenderSteps.World_Sorted, Game1.spriteBatch, time, target_screen))
			{
				if (!Game1.currentLocation.shouldHideCharacters())
				{
					if (Game1.CurrentEvent == null)
					{
						foreach (NPC n3 in Game1.currentLocation.characters)
						{
							if (!n3.swimming.Value && !n3.HideShadow && !n3.isInvisible.Value && this.checkCharacterTilesForShadowDrawFlag(n3))
							{
								n3.DrawShadow(Game1.spriteBatch);
							}
						}
					}
					else
					{
						foreach (NPC n4 in Game1.CurrentEvent.actors)
						{
							if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(n4)) && !n4.swimming.Value && !n4.HideShadow && this.checkCharacterTilesForShadowDrawFlag(n4))
							{
								n4.DrawShadow(Game1.spriteBatch);
							}
						}
					}
					foreach (Farmer f4 in this._farmerShadows)
					{
						if (!f4.swimming.Value && !f4.isRidingHorse() && !f4.IsSitting() && Game1.currentLocation != null && this.checkCharacterTilesForShadowDrawFlag(f4))
						{
							f4.DrawShadow(Game1.spriteBatch);
						}
					}
				}
				if ((Game1.eventUp || Game1.killScreen) && !Game1.killScreen && Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
				}
				Game1.currentLocation.draw(Game1.spriteBatch);
				foreach (Vector2 tile_position in Game1.crabPotOverlayTiles.Keys)
				{
					Tile tile = building_layer.Tiles[(int)tile_position.X, (int)tile_position.Y];
					if (tile != null)
					{
						Vector2 vector_draw_position = Game1.GlobalToLocal(Game1.viewport, tile_position * 64f);
						Location draw_location = new Location((int)vector_draw_position.X, (int)vector_draw_position.Y);
						Game1.mapDisplayDevice.DrawTile(tile, draw_location, (tile_position.Y * 64f - 1f) / 10000f);
					}
				}
				if (Game1.player.ActiveObject == null && Game1.player.UsingTool && Game1.player.CurrentTool != null)
				{
					Game1.drawTool(Game1.player);
				}
				if (Game1.panMode)
				{
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(Game1.getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(Game1.getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Color.Lime * 0.75f);
					foreach (Warp w in Game1.currentLocation.warps)
					{
						Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(w.X * 64 - Game1.viewport.X, w.Y * 64 - Game1.viewport.Y, 64, 64), Color.Red * 0.75f);
					}
				}
				for (int k = 0; k < Game1.currentLocation.frontLayers.Count; k++)
				{
					float layer2 = 0f;
					if (Game1.currentLocation.frontLayers.Count > 1)
					{
						layer2 = (float)k / (float)(Game1.currentLocation.frontLayers.Count - 1);
					}
					Game1.currentLocation.frontLayers[k].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4, 64f + layer_sub_sort * layer2);
				}
				Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
			}
			Game1.hooks.OnRendered(RenderSteps.World_Sorted, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.End();
			if (Game1.hooks.OnRendering(RenderSteps.World_AlwaysFront, Game1.spriteBatch, time, target_screen))
			{
				for (int l = 0; l < Game1.currentLocation.alwaysFrontLayers.Count; l++)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
					Game1.currentLocation.alwaysFrontLayers[l].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4, -1f);
					Game1.spriteBatch.End();
				}
			}
			Game1.hooks.OnRendered(RenderSteps.World_AlwaysFront, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (Game1.currentLocation.LightLevel > 0f && Game1.timeOfDay < 2000)
			{
				Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
			}
			if (Game1.screenGlow)
			{
				Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
			}
			Game1.spriteBatch.End();
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
			if (!Game1.IsFakedBlackScreen())
			{
				Game1.spriteBatch.End();
				this.drawWeather(time, target_screen);
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (Game1.player.CurrentTool is FishingRod rod && (rod.isTimingCast || rod.castingChosenCountdown > 0f || rod.fishCaught || rod.showingTreasure))
			{
				Game1.player.CurrentTool.draw(Game1.spriteBatch);
			}
			Game1.spriteBatch.End();
			this.DrawCharacterEmotes(time, target_screen);
			Game1.mapDisplayDevice.EndScene();
			if (Game1.drawLighting && !Game1.IsFakedBlackScreen())
			{
				this.DrawLightmapOnScreen(time, target_screen);
			}
			if (!Game1.eventUp && Game1.farmEvent == null && Game1.gameMode == 3 && !this.takingMapScreenshot && Game1.isOutdoorMapSmallerThanViewport())
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, -Game1.viewport.X, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
				Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64, 0, Game1.graphics.GraphicsDevice.Viewport.Width - (-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64), Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
				Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, -Game1.viewport.Y), Color.Black);
				Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, -Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height - (-Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64)), Color.Black);
				Game1.spriteBatch.End();
			}
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (Game1.currentLocation != null && Game1.currentLocation.isOutdoors.Value && !Game1.IsFakedBlackScreen() && Game1.currentLocation.IsRainingHere())
			{
				bool isGreenRain = Game1.IsGreenRainingHere();
				Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, isGreenRain ? (new Color(0, 120, 150) * 0.22f) : (Color.Blue * 0.2f));
			}
			Game1.spriteBatch.End();
			if (Game1.farmEvent != null)
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.farmEvent.draw(Game1.spriteBatch);
				Game1.spriteBatch.End();
			}
			if (Game1.eventUp && Game1.currentLocation?.currentEvent != null)
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Game1.currentLocation.currentEvent.drawAfterMap(Game1.spriteBatch);
				Game1.spriteBatch.End();
			}
			if (!this.takingMapScreenshot)
			{
				if (Game1.drawGrid)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					int startingX = -Game1.viewport.X % 64;
					float startingY = -Game1.viewport.Y % 64;
					for (int x = startingX; x < Game1.graphics.GraphicsDevice.Viewport.Width; x += 64)
					{
						Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(x, (int)startingY, 1, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
					}
					for (float y = startingY; y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 64f)
					{
						Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(startingX, (int)y, Game1.graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
					}
					Game1.spriteBatch.End();
				}
				if (Game1.ShouldShowOnscreenUsernames() && Game1.currentLocation != null)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
					Game1.currentLocation.DrawFarmerUsernames(Game1.spriteBatch);
					Game1.spriteBatch.End();
				}
				if (Game1.flashAlpha > 0f)
				{
					if (Game1.options.screenFlash)
					{
						Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
						Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, Game1.flashAlpha));
						Game1.spriteBatch.End();
					}
					Game1.flashAlpha -= 0.1f;
				}
			}
		}
		Game1.hooks.OnRendered(RenderSteps.World, Game1.spriteBatch, time, target_screen);
	}

	public virtual void DrawCharacterEmotes(GameTime time, RenderTarget2D target_screen)
	{
		if (!Game1.eventUp || Game1.currentLocation.currentEvent == null)
		{
			return;
		}
		Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (NPC actor in Game1.currentLocation.currentEvent.actors)
		{
			actor.DrawEmote(Game1.spriteBatch);
		}
		Game1.spriteBatch.End();
	}

	public virtual void DrawLightmapOnScreen(GameTime time, RenderTarget2D target_screen)
	{
		if (Game1.hooks.OnRendering(RenderSteps.World_DrawLightmapOnScreen, Game1.spriteBatch, time, target_screen))
		{
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, this.lightingBlend, SamplerState.LinearClamp);
			Viewport vp = base.GraphicsDevice.Viewport;
			vp.Bounds = target_screen?.Bounds ?? base.GraphicsDevice.PresentationParameters.Bounds;
			base.GraphicsDevice.Viewport = vp;
			float render_zoom = Game1.options.lightingQuality / 2;
			if (this.useUnscaledLighting)
			{
				render_zoom /= Game1.options.zoomLevel;
			}
			Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, Game1.lightmap.Bounds, Color.White, 0f, Vector2.Zero, render_zoom, SpriteEffects.None, 1f);
			if (Game1.currentLocation.isOutdoors.Value && Game1.currentLocation.IsRainingHere())
			{
				Game1.spriteBatch.Draw(Game1.lightingRect, vp.Bounds, Color.OrangeRed * 0.45f);
			}
		}
		Game1.hooks.OnRendered(RenderSteps.World_DrawLightmapOnScreen, Game1.spriteBatch, time, target_screen);
		Game1.spriteBatch.End();
	}

	public virtual void DrawDebugUIs(GameTime time, RenderTarget2D target_screen)
	{
		StringBuilder sb = Game1._debugStringBuilder;
		sb.Clear();
		if (Game1.panMode)
		{
			sb.Append((Game1.getOldMouseX() + Game1.viewport.X) / 64);
			sb.Append(",");
			sb.Append((Game1.getOldMouseY() + Game1.viewport.Y) / 64);
		}
		else
		{
			Point playerPixel = Game1.player.StandingPixel;
			sb.Append("player: ");
			sb.Append(playerPixel.X / 64);
			sb.Append(", ");
			sb.Append(playerPixel.Y / 64);
		}
		sb.Append(" mouseTransparency: ");
		sb.Append(Game1.mouseCursorTransparency);
		sb.Append(" mousePosition: ");
		sb.Append(Game1.getMouseX());
		sb.Append(",");
		sb.Append(Game1.getMouseY());
		sb.Append(Environment.NewLine);
		sb.Append(" mouseWorldPosition: ");
		sb.Append(Game1.getMouseX() + Game1.viewport.X);
		sb.Append(",");
		sb.Append(Game1.getMouseY() + Game1.viewport.Y);
		sb.Append("  debugOutput: ");
		sb.Append(Game1.debugOutput);
		Game1.PushUIMode();
		Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		Game1.spriteBatch.DrawString(Game1.smallFont, sb, new Vector2(base.GraphicsDevice.Viewport.GetTitleSafeArea().X, base.GraphicsDevice.Viewport.GetTitleSafeArea().Y + Game1.smallFont.LineSpacing * 8), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
		Game1.spriteBatch.End();
		Game1.PopUIMode();
	}

	public virtual void DrawGlobalFade(GameTime time, RenderTarget2D target_screen)
	{
		if ((Game1.fadeToBlack || Game1.globalFade) && !this.takingMapScreenshot)
		{
			Game1.PushUIMode();
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (Game1.hooks.OnRendering(RenderSteps.GlobalFade, Game1.spriteBatch, time, target_screen))
			{
				Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
			}
			Game1.hooks.OnRendered(RenderSteps.GlobalFade, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.End();
			Game1.PopUIMode();
		}
	}

	public virtual void DrawLighting(GameTime time, RenderTarget2D target_screen)
	{
		Game1.SetRenderTarget(Game1.lightmap);
		base.GraphicsDevice.Clear(Color.White * 0f);
		Matrix lighting_matrix = Matrix.Identity;
		if (this.useUnscaledLighting)
		{
			lighting_matrix = Matrix.CreateScale(Game1.options.zoomLevel);
		}
		Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, lighting_matrix);
		if (Game1.hooks.OnRendering(RenderSteps.World_RenderLightmap, Game1.spriteBatch, time, target_screen))
		{
			Color lighting = ((!(Game1.currentLocation is MineShaft mine)) ? ((Game1.ambientLight.Equals(Color.White) || (Game1.currentLocation.isOutdoors.Value && Game1.currentLocation.IsRainingHere())) ? Game1.outdoorLight : Game1.ambientLight) : mine.getLightingColor(time));
			float light_multiplier = 1f;
			if (Game1.player.hasBuff("26"))
			{
				if (lighting == Color.White)
				{
					lighting = new Color(0.75f, 0.75f, 0.75f);
				}
				else
				{
					lighting.R = (byte)Utility.Lerp((int)lighting.R, 255f, 0.5f);
					lighting.G = (byte)Utility.Lerp((int)lighting.G, 255f, 0.5f);
					lighting.B = (byte)Utility.Lerp((int)lighting.B, 255f, 0.5f);
				}
				light_multiplier = 0.33f;
			}
			if (Game1.IsGreenRainingHere())
			{
				lighting.R = (byte)Utility.Lerp((int)lighting.R, 255f, 0.25f);
				lighting.G = (byte)Utility.Lerp((int)lighting.R, 0f, 0.25f);
			}
			Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, lighting);
			foreach (KeyValuePair<string, LightSource> currentLightSource in Game1.currentLightSources)
			{
				currentLightSource.Value.Draw(Game1.spriteBatch, Game1.currentLocation, light_multiplier);
			}
		}
		Game1.hooks.OnRendered(RenderSteps.World_RenderLightmap, Game1.spriteBatch, time, target_screen);
		Game1.spriteBatch.End();
		Game1.SetRenderTarget(target_screen);
	}

	public virtual void drawWeather(GameTime time, RenderTarget2D target_screen)
	{
		Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (Game1.hooks.OnRendering(RenderSteps.World_Weather, Game1.spriteBatch, time, target_screen) && Game1.currentLocation.IsOutdoors)
		{
			if (Game1.currentLocation.IsSnowingHere())
			{
				Game1.snowPos.X %= 64f;
				Vector2 v = default(Vector2);
				for (float x = -64f + Game1.snowPos.X % 64f; x < (float)Game1.viewport.Width; x += 64f)
				{
					for (float y = -64f + Game1.snowPos.Y % 64f; y < (float)Game1.viewport.Height; y += 64f)
					{
						v.X = (int)x;
						v.Y = (int)y;
						Game1.spriteBatch.Draw(Game1.mouseCursors, v, new Microsoft.Xna.Framework.Rectangle(368 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0) / 75 * 16, 192, 16, 16), Color.White * 0.8f * Game1.options.snowTransparency, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
					}
				}
			}
			if (!Game1.currentLocation.ignoreDebrisWeather.Value && Game1.currentLocation.IsDebrisWeatherHere())
			{
				if (this.takingMapScreenshot)
				{
					if (Game1.debrisWeather != null)
					{
						foreach (WeatherDebris w in Game1.debrisWeather)
						{
							Vector2 position = w.position;
							w.position = new Vector2(Game1.random.Next(Game1.viewport.Width - w.sourceRect.Width * 3), Game1.random.Next(Game1.viewport.Height - w.sourceRect.Height * 3));
							w.draw(Game1.spriteBatch);
							w.position = position;
						}
					}
				}
				else if (Game1.viewport.X > -Game1.viewport.Width)
				{
					foreach (WeatherDebris item in Game1.debrisWeather)
					{
						item.draw(Game1.spriteBatch);
					}
				}
			}
			if (Game1.currentLocation.IsRainingHere() && !(Game1.currentLocation is Summit) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2(Game1.viewport.X / 64, Game1.viewport.Y / 64))))
			{
				bool isGreenRain = Game1.IsGreenRainingHere();
				Color rainColor = (isGreenRain ? Color.LimeGreen : Color.White);
				int vibrancy = ((!isGreenRain) ? 1 : 2);
				for (int i = 0; i < Game1.rainDrops.Length; i++)
				{
					for (int j = 0; j < vibrancy; j++)
					{
						Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[i].position, Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[i].frame + (isGreenRain ? 4 : 0), 16, 16), rainColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
				}
			}
		}
		Game1.hooks.OnRendered(RenderSteps.World_Weather, Game1.spriteBatch, time, target_screen);
		Game1.spriteBatch.End();
	}

	protected virtual void renderScreenBuffer(RenderTarget2D target_screen)
	{
		Game1.graphics.GraphicsDevice.SetRenderTarget(null);
		if (!this.takingMapScreenshot && !LocalMultiplayer.IsLocalMultiplayer() && (target_screen == null || !target_screen.IsContentLost))
		{
			if (this.ShouldDrawOnBuffer() && target_screen != null)
			{
				base.GraphicsDevice.Clear(Game1.bgColor);
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
				Game1.spriteBatch.Draw(target_screen, new Vector2(0f, 0f), target_screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
				Game1.spriteBatch.End();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
				Game1.spriteBatch.Draw(this.uiScreen, new Vector2(0f, 0f), this.uiScreen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.uiScale, SpriteEffects.None, 1f);
				Game1.spriteBatch.End();
			}
			else
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
				Game1.spriteBatch.Draw(this.uiScreen, new Vector2(0f, 0f), this.uiScreen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.uiScale, SpriteEffects.None, 1f);
				Game1.spriteBatch.End();
			}
		}
	}

	public virtual void DrawSplitScreenWindow()
	{
		if (!LocalMultiplayer.IsLocalMultiplayer())
		{
			return;
		}
		Game1.graphics.GraphicsDevice.SetRenderTarget(null);
		if (this.screen == null || !this.screen.IsContentLost)
		{
			Viewport old_viewport = base.GraphicsDevice.Viewport;
			GraphicsDevice graphicsDevice = base.GraphicsDevice;
			Viewport viewport = (base.GraphicsDevice.Viewport = Game1.defaultDeviceViewport);
			graphicsDevice.Viewport = viewport;
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
			Game1.spriteBatch.Draw(this.screen, new Vector2(this.localMultiplayerWindow.X, this.localMultiplayerWindow.Y), this.screen.Bounds, Color.White, 0f, Vector2.Zero, this.instanceOptions.zoomLevel, SpriteEffects.None, 1f);
			if (this.uiScreen != null)
			{
				Game1.spriteBatch.Draw(this.uiScreen, new Vector2(this.localMultiplayerWindow.X, this.localMultiplayerWindow.Y), this.uiScreen.Bounds, Color.White, 0f, Vector2.Zero, this.instanceOptions.uiScale, SpriteEffects.None, 1f);
			}
			Game1.spriteBatch.End();
			base.GraphicsDevice.Viewport = old_viewport;
		}
	}

	/// ###########################
	/// METHODS FOR DRAWING THINGS.
	/// ############################
	public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position)
	{
		Game1.drawWithBorder(message, borderColor, insideColor, position, 0f, 1f, 1f, tiny: false);
	}

	public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth)
	{
		Game1.drawWithBorder(message, borderColor, insideColor, position, rotate, scale, layerDepth, tiny: false);
	}

	public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth, bool tiny)
	{
		string[] words = ArgUtility.SplitBySpace(message);
		int offset = 0;
		for (int i = 0; i < words.Length; i++)
		{
			if (words[i].Contains('='))
			{
				Game1.spriteBatch.DrawString(tiny ? Game1.tinyFont : Game1.dialogueFont, words[i], new Vector2(position.X + (float)offset, position.Y), Color.Purple, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				offset += (int)((tiny ? Game1.tinyFont : Game1.dialogueFont).MeasureString(words[i]).X + 8f);
			}
			else
			{
				Game1.spriteBatch.DrawString(tiny ? Game1.tinyFont : Game1.dialogueFont, words[i], new Vector2(position.X + (float)offset, position.Y), insideColor, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				offset += (int)((tiny ? Game1.tinyFont : Game1.dialogueFont).MeasureString(words[i]).X + 8f);
			}
		}
	}

	public static bool isOutdoorMapSmallerThanViewport()
	{
		if (Game1.uiMode)
		{
			return false;
		}
		if (Game1.currentLocation != null && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Summit))
		{
			if (Game1.currentLocation.map.Layers[0].LayerWidth * 64 >= Game1.viewport.Width)
			{
				return Game1.currentLocation.map.Layers[0].LayerHeight * 64 < Game1.viewport.Height;
			}
			return true;
		}
		return false;
	}

	protected virtual void drawHUD()
	{
		if (Game1.eventUp || Game1.farmEvent != null)
		{
			return;
		}
		float modifier = 0.625f;
		Vector2 topOfBar = new Vector2(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Right - 48 - 8, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (int)((float)(Game1.player.MaxStamina - 270) * modifier));
		if (Game1.isOutdoorMapSmallerThanViewport())
		{
			topOfBar.X = Math.Min(topOfBar.X, -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 48);
		}
		if (Game1.staminaShakeTimer > 0)
		{
			topOfBar.X += Game1.random.Next(-3, 4);
			topOfBar.Y += Game1.random.Next(-3, 4);
		}
		Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar, new Microsoft.Xna.Framework.Rectangle(256, 408, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		Game1.spriteBatch.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f - 8f)), new Microsoft.Xna.Framework.Rectangle(256, 424, 12, 16), Color.White);
		Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)(int)((float)(Game1.player.MaxStamina - 270) * modifier) - 64f), new Microsoft.Xna.Framework.Rectangle(256, 448, 12, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + (int)((float)Game1.player.MaxStamina * modifier) - (int)(Math.Max(0f, Game1.player.Stamina) * modifier), 24, (int)(Game1.player.Stamina * modifier) - 1);
		if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y)
		{
			Game1.drawWithBorder((int)Math.Max(0f, Game1.player.Stamina) + "/" + Game1.player.MaxStamina, Color.Black * 0f, Color.White, topOfBar + new Vector2(0f - Game1.dialogueFont.MeasureString("999/999").X - 16f - (float)(Game1.showingHealth ? 64 : 0), 64f));
		}
		Color c = Utility.getRedToGreenLerpColor(Game1.player.stamina / (float)Game1.player.maxStamina.Value);
		Game1.spriteBatch.Draw(Game1.staminaRect, r, c);
		r.Height = 4;
		c.R = (byte)Math.Max(0, c.R - 50);
		c.G = (byte)Math.Max(0, c.G - 50);
		Game1.spriteBatch.Draw(Game1.staminaRect, r, c);
		if (Game1.player.exhausted.Value)
		{
			Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar - new Vector2(0f, 11f) * 4f, new Microsoft.Xna.Framework.Rectangle(191, 406, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y - 44f)
			{
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747"), Color.Black * 0f, Color.White, topOfBar + new Vector2(0f - Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747")).X - 16f - (float)(Game1.showingHealth ? 64 : 0), 96f));
			}
		}
		if (Game1.currentLocation is MineShaft || Game1.currentLocation is Woods || Game1.currentLocation is SlimeHutch || Game1.currentLocation is VolcanoDungeon || Game1.player.health < Game1.player.maxHealth)
		{
			Game1.showingHealthBar = true;
			Game1.showingHealth = true;
			int bar_full_height = 168 + (Game1.player.maxHealth - 100);
			int height = (int)((float)Game1.player.health / (float)Game1.player.maxHealth * (float)bar_full_height);
			topOfBar.X -= 56 + ((Game1.hitShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0);
			topOfBar.Y = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (Game1.player.maxHealth - 100);
			Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar, new Microsoft.Xna.Framework.Rectangle(268, 408, 12, 16), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Game1.spriteBatch.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f)), new Microsoft.Xna.Framework.Rectangle(268, 424, 12, 16), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White);
			Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)(Game1.player.maxHealth - 100) - 64f), new Microsoft.Xna.Framework.Rectangle(268, 448, 12, 16), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Microsoft.Xna.Framework.Rectangle health_bar_rect = new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + bar_full_height - height, 24, height);
			c = Utility.getRedToGreenLerpColor((float)Game1.player.health / (float)Game1.player.maxHealth);
			Game1.spriteBatch.Draw(Game1.staminaRect, health_bar_rect, Game1.staminaRect.Bounds, c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			c.R = (byte)Math.Max(0, c.R - 50);
			c.G = (byte)Math.Max(0, c.G - 50);
			if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y && (float)Game1.getOldMouseX() < topOfBar.X + 32f)
			{
				Game1.drawWithBorder(Math.Max(0, Game1.player.health) + "/" + Game1.player.maxHealth, Color.Black * 0f, Color.Red, topOfBar + new Vector2(0f - Game1.dialogueFont.MeasureString("999/999").X - 32f, 64f));
			}
			health_bar_rect.Height = 4;
			Game1.spriteBatch.Draw(Game1.staminaRect, health_bar_rect, Game1.staminaRect.Bounds, c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
		}
		else
		{
			Game1.showingHealth = false;
		}
		foreach (IClickableMenu menu in Game1.onScreenMenus)
		{
			if (menu != Game1.chatBox)
			{
				menu.update(Game1.currentGameTime);
				menu.draw(Game1.spriteBatch);
			}
		}
		if (!Game1.player.professions.Contains(17) || !Game1.currentLocation.IsOutdoors)
		{
			return;
		}
		foreach (KeyValuePair<Vector2, Object> v in Game1.currentLocation.objects.Pairs)
		{
			if ((v.Value.isSpawnedObject.Value || v.Value.QualifiedItemId == "(O)590") && !Utility.isOnScreen(v.Key * 64f + new Vector2(32f, 32f), 64))
			{
				Microsoft.Xna.Framework.Rectangle vpbounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
				Vector2 onScreenPosition = default(Vector2);
				float rotation = 0f;
				if (v.Key.X * 64f > (float)(Game1.viewport.MaxCorner.X - 64))
				{
					onScreenPosition.X = vpbounds.Right - 8;
					rotation = (float)Math.PI / 2f;
				}
				else if (v.Key.X * 64f < (float)Game1.viewport.X)
				{
					onScreenPosition.X = 8f;
					rotation = -(float)Math.PI / 2f;
				}
				else
				{
					onScreenPosition.X = v.Key.X * 64f - (float)Game1.viewport.X;
				}
				if (v.Key.Y * 64f > (float)(Game1.viewport.MaxCorner.Y - 64))
				{
					onScreenPosition.Y = vpbounds.Bottom - 8;
					rotation = (float)Math.PI;
				}
				else if (v.Key.Y * 64f < (float)Game1.viewport.Y)
				{
					onScreenPosition.Y = 8f;
				}
				else
				{
					onScreenPosition.Y = v.Key.Y * 64f - (float)Game1.viewport.Y;
				}
				if (onScreenPosition.X == 8f && onScreenPosition.Y == 8f)
				{
					rotation += (float)Math.PI / 4f;
				}
				if (onScreenPosition.X == 8f && onScreenPosition.Y == (float)(vpbounds.Bottom - 8))
				{
					rotation += (float)Math.PI / 4f;
				}
				if (onScreenPosition.X == (float)(vpbounds.Right - 8) && onScreenPosition.Y == 8f)
				{
					rotation -= (float)Math.PI / 4f;
				}
				if (onScreenPosition.X == (float)(vpbounds.Right - 8) && onScreenPosition.Y == (float)(vpbounds.Bottom - 8))
				{
					rotation -= (float)Math.PI / 4f;
				}
				Microsoft.Xna.Framework.Rectangle srcRect = new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4);
				float renderScale = 4f;
				Vector2 safePos = Utility.makeSafe(renderSize: new Vector2((float)srcRect.Width * renderScale, (float)srcRect.Height * renderScale), renderPos: onScreenPosition);
				Game1.spriteBatch.Draw(Game1.mouseCursors, safePos, srcRect, Color.White, rotation, new Vector2(2f, 2f), renderScale, SpriteEffects.None, 1f);
			}
		}
		if (!Game1.currentLocation.orePanPoint.Equals(Point.Zero) && !Utility.isOnScreen(Utility.PointToVector2(Game1.currentLocation.orePanPoint.Value) * 64f + new Vector2(32f, 32f), 64))
		{
			Vector2 onScreenPosition2 = default(Vector2);
			float rotation2 = 0f;
			if (Game1.currentLocation.orePanPoint.X * 64 > Game1.viewport.MaxCorner.X - 64)
			{
				onScreenPosition2.X = Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8;
				rotation2 = (float)Math.PI / 2f;
			}
			else if (Game1.currentLocation.orePanPoint.X * 64 < Game1.viewport.X)
			{
				onScreenPosition2.X = 8f;
				rotation2 = -(float)Math.PI / 2f;
			}
			else
			{
				onScreenPosition2.X = Game1.currentLocation.orePanPoint.X * 64 - Game1.viewport.X;
			}
			if (Game1.currentLocation.orePanPoint.Y * 64 > Game1.viewport.MaxCorner.Y - 64)
			{
				onScreenPosition2.Y = Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8;
				rotation2 = (float)Math.PI;
			}
			else if (Game1.currentLocation.orePanPoint.Y * 64 < Game1.viewport.Y)
			{
				onScreenPosition2.Y = 8f;
			}
			else
			{
				onScreenPosition2.Y = Game1.currentLocation.orePanPoint.Y * 64 - Game1.viewport.Y;
			}
			if (onScreenPosition2.X == 8f && onScreenPosition2.Y == 8f)
			{
				rotation2 += (float)Math.PI / 4f;
			}
			if (onScreenPosition2.X == 8f && onScreenPosition2.Y == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
			{
				rotation2 += (float)Math.PI / 4f;
			}
			if (onScreenPosition2.X == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition2.Y == 8f)
			{
				rotation2 -= (float)Math.PI / 4f;
			}
			if (onScreenPosition2.X == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition2.Y == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
			{
				rotation2 -= (float)Math.PI / 4f;
			}
			Game1.spriteBatch.Draw(Game1.mouseCursors, onScreenPosition2, new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4), Color.Cyan, rotation2, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
		}
	}

	public static void InvalidateOldMouseMovement()
	{
		MouseState input = Game1.input.GetMouseState();
		Game1.oldMouseState = new MouseState(input.X, input.Y, Game1.oldMouseState.ScrollWheelValue, Game1.oldMouseState.LeftButton, Game1.oldMouseState.MiddleButton, Game1.oldMouseState.RightButton, Game1.oldMouseState.XButton1, Game1.oldMouseState.XButton2);
	}

	public static bool IsRenderingNonNativeUIScale()
	{
		return Game1.options.uiScale != Game1.options.zoomLevel;
	}

	public virtual void drawMouseCursor()
	{
		if (Game1.activeClickableMenu == null && Game1.timerUntilMouseFade > 0)
		{
			Game1.timerUntilMouseFade -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			Game1.lastMousePositionBeforeFade = Game1.getMousePosition();
		}
		if (Game1.options.gamepadControls && Game1.timerUntilMouseFade <= 0 && Game1.activeClickableMenu == null && (Game1.emoteMenu == null || Game1.emoteMenu.gamepadMode))
		{
			Game1.mouseCursorTransparency = 0f;
		}
		if (Game1.activeClickableMenu == null && Game1.mouseCursor > Game1.cursor_none && Game1.currentLocation != null)
		{
			if (Game1.IsRenderingNonNativeUIScale())
			{
				Game1.spriteBatch.End();
				Game1.PopUIMode();
				if (this.ShouldDrawOnBuffer())
				{
					Game1.SetRenderTarget(this.screen);
				}
				else
				{
					Game1.SetRenderTarget(null);
				}
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (!(Game1.mouseCursorTransparency > 0f) || !Utility.canGrabSomethingFromHere(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, Game1.player) || Game1.mouseCursor == Game1.cursor_gift)
			{
				if (Game1.player.ActiveObject != null && Game1.mouseCursor != Game1.cursor_gift && !Game1.eventUp && Game1.currentMinigame == null && !Game1.player.isRidingHorse() && Game1.player.CanMove && Game1.displayFarmer)
				{
					if (Game1.mouseCursorTransparency > 0f || Game1.options.showPlacementTileForGamepad)
					{
						Game1.player.ActiveObject.drawPlacementBounds(Game1.spriteBatch, Game1.currentLocation);
						if (Game1.mouseCursorTransparency > 0f)
						{
							Game1.spriteBatch.End();
							Game1.PushUIMode();
							Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
							bool canPlace = Utility.playerCanPlaceItemHere(Game1.currentLocation, Game1.player.CurrentItem, Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y, Game1.player) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(Game1.currentLocation, Game1.player.CurrentItem, Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y) && Utility.withinRadiusOfPlayer(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y, 1, Game1.player));
							Game1.player.CurrentItem?.drawInMenu(Game1.spriteBatch, new Vector2(Game1.getMouseX() + 16, Game1.getMouseY() + 16), canPlace ? (Game1.dialogueButtonScale / 75f + 1f) : 1f, canPlace ? 1f : 0.5f, 0.999f);
							Game1.spriteBatch.End();
							Game1.PopUIMode();
							Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
						}
					}
				}
				else if (Game1.mouseCursor == Game1.cursor_default && Game1.isActionAtCurrentCursorTile && Game1.currentMinigame == null)
				{
					Game1.mouseCursor = (Game1.isSpeechAtCurrentCursorTile ? Game1.cursor_talk : (Game1.isInspectionAtCurrentCursorTile ? Game1.cursor_look : Game1.cursor_grab));
				}
				else if (Game1.mouseCursorTransparency > 0f)
				{
					NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = Game1.currentLocation.animals;
					if (animals != null)
					{
						Vector2 mousePos = new Vector2(Game1.getOldMouseX() + Game1.uiViewport.X, Game1.getOldMouseY() + Game1.uiViewport.Y);
						bool mouseWithinRadiusOfPlayer = Utility.withinRadiusOfPlayer((int)mousePos.X, (int)mousePos.Y, 1, Game1.player);
						foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
						{
							Microsoft.Xna.Framework.Rectangle animalBounds = kvp.Value.GetCursorPetBoundingBox();
							if (!kvp.Value.wasPet.Value && animalBounds.Contains((int)mousePos.X, (int)mousePos.Y))
							{
								Game1.mouseCursor = Game1.cursor_grab;
								if (!mouseWithinRadiusOfPlayer)
								{
									Game1.mouseCursorTransparency = 0.5f;
								}
								break;
							}
						}
					}
				}
			}
			if (Game1.IsRenderingNonNativeUIScale())
			{
				Game1.spriteBatch.End();
				Game1.PushUIMode();
				Game1.SetRenderTarget(this.uiScreen);
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			if (Game1.currentMinigame != null)
			{
				Game1.mouseCursor = Game1.cursor_default;
			}
			if (!Game1.freezeControls && !Game1.options.hardwareCursor)
			{
				Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.mouseCursor, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			Game1.wasMouseVisibleThisFrame = Game1.mouseCursorTransparency > 0f;
			this._lastDrewMouseCursor = Game1.wasMouseVisibleThisFrame;
		}
		Game1.mouseCursor = Game1.cursor_default;
		if (!Game1.isActionAtCurrentCursorTile && Game1.activeClickableMenu == null)
		{
			Game1.mouseCursorTransparency = 1f;
		}
	}

	public static void panScreen(int x, int y)
	{
		int old_ui_mode_count = Game1.uiModeCount;
		while (Game1.uiModeCount > 0)
		{
			Game1.PopUIMode();
		}
		Game1.previousViewportPosition.X = Game1.viewport.Location.X;
		Game1.previousViewportPosition.Y = Game1.viewport.Location.Y;
		Game1.viewport.X += x;
		Game1.viewport.Y += y;
		Game1.clampViewportToGameMap();
		Game1.updateRaindropPosition();
		for (int i = 0; i < old_ui_mode_count; i++)
		{
			Game1.PushUIMode();
		}
	}

	public static void clampViewportToGameMap()
	{
		if (Game1.viewport.X < 0)
		{
			Game1.viewport.X = 0;
		}
		if (Game1.viewport.X > Game1.currentLocation.map.DisplayWidth - Game1.viewport.Width)
		{
			Game1.viewport.X = Game1.currentLocation.map.DisplayWidth - Game1.viewport.Width;
		}
		if (Game1.viewport.Y < 0)
		{
			Game1.viewport.Y = 0;
		}
		if (Game1.viewport.Y > Game1.currentLocation.map.DisplayHeight - Game1.viewport.Height)
		{
			Game1.viewport.Y = Game1.currentLocation.map.DisplayHeight - Game1.viewport.Height;
		}
	}

	protected void drawDialogueBox()
	{
		if (Game1.currentSpeaker != null)
		{
			int messageHeight = (int)Game1.dialogueFont.MeasureString(Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).Y;
			messageHeight = Math.Max(messageHeight, 320);
			Game1.drawDialogueBox((base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Math.Min(1280, base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 128)) / 2, base.GraphicsDevice.Viewport.GetTitleSafeArea().Height - messageHeight, Math.Min(1280, base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 128), messageHeight, speaker: true, drawOnlyBox: false, null, Game1.objectDialoguePortraitPerson != null && Game1.currentSpeaker == null);
		}
	}

	public static void drawDialogueBox(string message)
	{
		Game1.drawDialogueBox(Game1.viewport.Width / 2, Game1.viewport.Height / 2, speaker: false, drawOnlyBox: false, message);
	}

	public static void drawDialogueBox(int centerX, int centerY, bool speaker, bool drawOnlyBox, string message)
	{
		string text = null;
		if (speaker && Game1.currentSpeaker != null)
		{
			text = Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue();
		}
		else if (message != null)
		{
			text = message;
		}
		else if (Game1.currentObjectDialogue.Count > 0)
		{
			text = Game1.currentObjectDialogue.Peek();
		}
		if (text != null)
		{
			Vector2 vector = Game1.dialogueFont.MeasureString(text);
			int width = (int)vector.X + 128;
			int height = (int)vector.Y + 128;
			int x = centerX - width / 2;
			int y = centerY - height / 2;
			Game1.drawDialogueBox(x, y, width, height, speaker, drawOnlyBox, message, Game1.objectDialoguePortraitPerson != null && !speaker);
		}
	}

	public static void DrawBox(int x, int y, int width, int height, Color? color = null)
	{
		Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
		sourceRect.X = 64;
		sourceRect.Y = 128;
		Texture2D menu_texture = Game1.menuTexture;
		Color draw_color = Color.White;
		Color inner_color = Color.White;
		if (color.HasValue)
		{
			draw_color = color.Value;
			menu_texture = Game1.uncoloredMenuTexture;
			inner_color = new Color((int)Utility.Lerp((int)draw_color.R, Math.Min(255, draw_color.R + 150), 0.65f), (int)Utility.Lerp((int)draw_color.G, Math.Min(255, draw_color.G + 150), 0.65f), (int)Utility.Lerp((int)draw_color.B, Math.Min(255, draw_color.B + 150), 0.65f));
		}
		Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x, y, width, height), sourceRect, inner_color);
		sourceRect.Y = 0;
		Vector2 offset = new Vector2((float)(-sourceRect.Width) * 0.5f, (float)(-sourceRect.Height) * 0.5f);
		sourceRect.X = 0;
		Game1.spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X, (float)y + offset.Y), sourceRect, draw_color);
		sourceRect.X = 192;
		Game1.spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X + (float)width, (float)y + offset.Y), sourceRect, draw_color);
		sourceRect.Y = 192;
		Game1.spriteBatch.Draw(menu_texture, new Vector2((float)(x + width) + offset.X, (float)(y + height) + offset.Y), sourceRect, draw_color);
		sourceRect.X = 0;
		Game1.spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X, (float)(y + height) + offset.Y), sourceRect, draw_color);
		sourceRect.X = 128;
		sourceRect.Y = 0;
		Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)offset.X, y + (int)offset.Y, width - 64, 64), sourceRect, draw_color);
		sourceRect.Y = 192;
		Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)offset.X, y + (int)offset.Y + height, width - 64, 64), sourceRect, draw_color);
		sourceRect.Y = 128;
		sourceRect.X = 0;
		Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x + (int)offset.X, y + (int)offset.Y + 64, 64, height - 64), sourceRect, draw_color);
		sourceRect.X = 192;
		Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x + width + (int)offset.X, y + (int)offset.Y + 64, 64, height - 64), sourceRect, draw_color);
	}

	public static void drawDialogueBox(int x, int y, int width, int height, bool speaker, bool drawOnlyBox, string message = null, bool objectDialogueWithPortrait = false, bool ignoreTitleSafe = true, int r = -1, int g = -1, int b = -1)
	{
		if (!drawOnlyBox)
		{
			return;
		}
		Microsoft.Xna.Framework.Rectangle titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
		int screenHeight = titleSafeArea.Height;
		int screenWidth = titleSafeArea.Width;
		int dialogueX = 0;
		int dialogueY = 0;
		if (!ignoreTitleSafe)
		{
			dialogueY = ((y <= titleSafeArea.Y) ? (titleSafeArea.Y - y) : 0);
		}
		int everythingYOffset = 0;
		width = Math.Min(titleSafeArea.Width, width);
		if (!Game1.isQuestion && Game1.currentSpeaker == null && Game1.currentObjectDialogue.Count > 0 && !drawOnlyBox)
		{
			width = (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).X + 128;
			height = (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y + 64;
			x = screenWidth / 2 - width / 2;
			everythingYOffset = ((height > 256) ? (-(height - 256)) : 0);
		}
		Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
		int addedTileHeightForQuestions = -1;
		if (Game1.questionChoices.Count >= 3)
		{
			addedTileHeightForQuestions = Game1.questionChoices.Count - 3;
		}
		if (!drawOnlyBox && Game1.currentObjectDialogue.Count > 0)
		{
			if (Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y >= (float)(height - 128))
			{
				addedTileHeightForQuestions -= (int)(((float)(height - 128) - Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y) / 64f) - 1;
			}
			else
			{
				height += (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y / 2;
				everythingYOffset -= (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y / 2;
				if ((int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y / 2 > 64)
				{
					addedTileHeightForQuestions = 0;
				}
			}
		}
		if (Game1.currentSpeaker != null && Game1.isQuestion && Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, Game1.currentDialogueCharacterIndex)
			.Contains(Environment.NewLine))
		{
			addedTileHeightForQuestions++;
		}
		sourceRect.Width = 64;
		sourceRect.Height = 64;
		sourceRect.X = 64;
		sourceRect.Y = 128;
		Color tint = ((r == -1) ? Color.White : new Color(r, g, b));
		Texture2D texture = ((r == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture);
		Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(28 + x + dialogueX, 28 + y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset, width - 64, height - 64 + addedTileHeightForQuestions * 64), sourceRect, (r == -1) ? tint : new Color((int)Utility.Lerp(r, Math.Min(255, r + 150), 0.65f), (int)Utility.Lerp(g, Math.Min(255, g + 150), 0.65f), (int)Utility.Lerp(b, Math.Min(255, b + 150), 0.65f)));
		sourceRect.Y = 0;
		sourceRect.X = 0;
		Game1.spriteBatch.Draw(texture, new Vector2(x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset), sourceRect, tint);
		sourceRect.X = 192;
		Game1.spriteBatch.Draw(texture, new Vector2(x + width + dialogueX - 64, y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset), sourceRect, tint);
		sourceRect.Y = 192;
		Game1.spriteBatch.Draw(texture, new Vector2(x + width + dialogueX - 64, y + height + dialogueY - 64 + everythingYOffset), sourceRect, tint);
		sourceRect.X = 0;
		Game1.spriteBatch.Draw(texture, new Vector2(x + dialogueX, y + height + dialogueY - 64 + everythingYOffset), sourceRect, tint);
		sourceRect.X = 128;
		sourceRect.Y = 0;
		Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset, width - 128, 64), sourceRect, tint);
		sourceRect.Y = 192;
		Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + dialogueX, y + height + dialogueY - 64 + everythingYOffset, width - 128, 64), sourceRect, tint);
		sourceRect.Y = 128;
		sourceRect.X = 0;
		Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + 64 + everythingYOffset, 64, height - 128 + addedTileHeightForQuestions * 64), sourceRect, tint);
		sourceRect.X = 192;
		Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + width + dialogueX - 64, y - 64 * addedTileHeightForQuestions + dialogueY + 64 + everythingYOffset, 64, height - 128 + addedTileHeightForQuestions * 64), sourceRect, tint);
		NPC theSpeaker;
		string text;
		Microsoft.Xna.Framework.Rectangle portraitRect;
		if ((objectDialogueWithPortrait && Game1.objectDialoguePortraitPerson != null) || (speaker && Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0 && Game1.currentSpeaker.CurrentDialogue.Peek().showPortrait))
		{
			theSpeaker = (objectDialogueWithPortrait ? Game1.objectDialoguePortraitPerson : Game1.currentSpeaker);
			text = ((!objectDialogueWithPortrait) ? theSpeaker.CurrentDialogue.Peek().CurrentEmotion : ((Game1.objectDialoguePortraitPerson.Name == Game1.player.spouse) ? "$l" : "$neutral"));
			if (text != null)
			{
				int length = text.Length;
				if (length != 2)
				{
					if (length == 8 && text == "$neutral")
					{
						goto IL_0705;
					}
				}
				else
				{
					switch (text[1])
					{
					case 'h':
						break;
					case 's':
						goto IL_063e;
					case 'u':
						goto IL_0651;
					case 'l':
						goto IL_0664;
					case 'a':
						goto IL_0677;
					case 'k':
						goto IL_068a;
					default:
						goto IL_0714;
					}
					if (text == "$h")
					{
						portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 0, 64, 64);
						goto IL_0740;
					}
				}
			}
			goto IL_0714;
		}
		goto IL_0a07;
		IL_0714:
		portraitRect = Game1.getSourceRectForStandardTileSheet(theSpeaker.Portrait, Convert.ToInt32(theSpeaker.CurrentDialogue.Peek().CurrentEmotion.Substring(1)));
		goto IL_0740;
		IL_0651:
		if (!(text == "$u"))
		{
			goto IL_0714;
		}
		portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 64, 64, 64);
		goto IL_0740;
		IL_0a07:
		if (drawOnlyBox)
		{
			return;
		}
		string text2 = "";
		if (Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0)
		{
			if (Game1.currentSpeaker.CurrentDialogue.Peek() == null || Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length < Game1.currentDialogueCharacterIndex - 1)
			{
				Game1.dialogueUp = false;
				Game1.currentDialogueCharacterIndex = 0;
				Game1.playSound("dialogueCharacterClose");
				Game1.player.forceCanMove();
				return;
			}
			text2 = Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, Game1.currentDialogueCharacterIndex);
		}
		else if (message != null)
		{
			text2 = message;
		}
		else if (Game1.currentObjectDialogue.Count > 0)
		{
			text2 = ((Game1.currentObjectDialogue.Peek().Length <= 1) ? "" : Game1.currentObjectDialogue.Peek().Substring(0, Game1.currentDialogueCharacterIndex));
		}
		Vector2 textPosition = ((Game1.dialogueFont.MeasureString(text2).X > (float)(screenWidth - 256 - dialogueX)) ? new Vector2(128 + dialogueX, screenHeight - 64 * addedTileHeightForQuestions - 256 - 16 + dialogueY + everythingYOffset) : ((Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0) ? new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString(Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).X / 2f + (float)dialogueX, screenHeight - 64 * addedTileHeightForQuestions - 256 - 16 + dialogueY + everythingYOffset) : ((message != null) ? new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString(text2).X / 2f + (float)dialogueX, y + 96 + 4) : ((!Game1.isQuestion) ? new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString((Game1.currentObjectDialogue.Count == 0) ? "" : Game1.currentObjectDialogue.Peek()).X / 2f + (float)dialogueX, y + 4 + everythingYOffset) : new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString((Game1.currentObjectDialogue.Count == 0) ? "" : Game1.currentObjectDialogue.Peek()).X / 2f + (float)dialogueX, screenHeight - 64 * addedTileHeightForQuestions - 256 - (16 + (Game1.questionChoices.Count - 2) * 64) + dialogueY + everythingYOffset)))));
		if (!drawOnlyBox)
		{
			Game1.spriteBatch.DrawString(Game1.dialogueFont, text2, textPosition + new Vector2(3f, 0f), Game1.textShadowColor);
			Game1.spriteBatch.DrawString(Game1.dialogueFont, text2, textPosition + new Vector2(3f, 3f), Game1.textShadowColor);
			Game1.spriteBatch.DrawString(Game1.dialogueFont, text2, textPosition + new Vector2(0f, 3f), Game1.textShadowColor);
			Game1.spriteBatch.DrawString(Game1.dialogueFont, text2, textPosition, Game1.textColor);
		}
		if (Game1.dialogueFont.MeasureString(text2).Y <= 64f)
		{
			dialogueY += 64;
		}
		if (Game1.isQuestion && !Game1.dialogueTyping)
		{
			for (int i = 0; i < Game1.questionChoices.Count; i++)
			{
				if (Game1.currentQuestionChoice == i)
				{
					textPosition.X = 80 + dialogueX + x;
					textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text2.Trim().Length > 0) ? Game1.dialogueFont.MeasureString(text2).Y : 0f) + 128f + (float)(48 * i) - (float)(16 + (Game1.questionChoices.Count - 2) * 64) + (float)dialogueY + (float)everythingYOffset;
					Game1.spriteBatch.End();
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
					Game1.spriteBatch.Draw(Game1.objectSpriteSheet, textPosition + new Vector2((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) * 3f, 0f), GameLocation.getSourceRectForObject(26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Game1.spriteBatch.End();
					Game1.spriteBatch.Begin();
					textPosition.X = 160 + dialogueX + x;
					textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text2.Trim().Length > 1) ? Game1.dialogueFont.MeasureString(text2).Y : 0f) + 128f - (float)((Game1.questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)dialogueY + (float)everythingYOffset;
					Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.questionChoices[i].responseText, textPosition, Game1.textColor);
				}
				else
				{
					textPosition.X = 128 + dialogueX + x;
					textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text2.Trim().Length > 1) ? Game1.dialogueFont.MeasureString(text2).Y : 0f) + 128f - (float)((Game1.questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)dialogueY + (float)everythingYOffset;
					Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.questionChoices[i].responseText, textPosition, Game1.unselectedOptionColor);
				}
			}
		}
		if (!drawOnlyBox && !Game1.dialogueTyping && message == null)
		{
			Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(x + dialogueX + width - 96, (float)(y + height + dialogueY + everythingYOffset - 96) - Game1.dialogueButtonScale), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (!Game1.dialogueButtonShrinking && Game1.dialogueButtonScale < 8f) ? 3 : 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
		}
		return;
		IL_063e:
		if (!(text == "$s"))
		{
			goto IL_0714;
		}
		portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64);
		goto IL_0740;
		IL_0705:
		portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
		goto IL_0740;
		IL_0664:
		if (!(text == "$l"))
		{
			goto IL_0714;
		}
		portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 128, 64, 64);
		goto IL_0740;
		IL_0740:
		Game1.spriteBatch.End();
		Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
		if (theSpeaker.Portrait != null)
		{
			Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(dialogueX + x + 768, screenHeight - 320 - 64 * addedTileHeightForQuestions - 256 + dialogueY + 16 - 60 + everythingYOffset), new Microsoft.Xna.Framework.Rectangle(333, 305, 80, 87), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.98f);
			Game1.spriteBatch.Draw(theSpeaker.Portrait, new Vector2(dialogueX + x + 768 + 32, screenHeight - 320 - 64 * addedTileHeightForQuestions - 256 + dialogueY + 16 - 60 + everythingYOffset), portraitRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
		}
		Game1.spriteBatch.End();
		Game1.spriteBatch.Begin();
		if (Game1.isQuestion)
		{
			Game1.spriteBatch.DrawString(Game1.dialogueFont, theSpeaker.displayName, new Vector2(928f - Game1.dialogueFont.MeasureString(theSpeaker.displayName).X / 2f + (float)dialogueX + (float)x, (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions) - Game1.dialogueFont.MeasureString(theSpeaker.displayName).Y + (float)dialogueY + 21f + (float)everythingYOffset) + new Vector2(2f, 2f), new Color(150, 150, 150));
		}
		Game1.spriteBatch.DrawString(Game1.dialogueFont, theSpeaker.Name.Equals("Lewis") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName, new Vector2((float)(dialogueX + x + 896 + 32) - Game1.dialogueFont.MeasureString(theSpeaker.Name.Equals("Lewis") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName).X / 2f, (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions) - Game1.dialogueFont.MeasureString(theSpeaker.Name.Equals("Lewis") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName).Y + (float)dialogueY + 21f + 8f + (float)everythingYOffset), Game1.textColor);
		goto IL_0a07;
		IL_068a:
		if (text == "$k")
		{
			goto IL_0705;
		}
		goto IL_0714;
		IL_0677:
		if (!(text == "$a"))
		{
			goto IL_0714;
		}
		portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 128, 64, 64);
		goto IL_0740;
	}

	public static void drawPlayerHeldObject(Farmer f)
	{
		if ((!Game1.eventUp || (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.showActiveObject)) && !f.FarmerSprite.PauseForSingleAnimation && !f.isRidingHorse() && !f.bathingClothes.Value && !f.onBridge.Value)
		{
			float xPosition = f.getLocalPosition(Game1.viewport).X + (float)((f.rotation < 0f) ? (-8) : ((f.rotation > 0f) ? 8 : 0)) + (float)(f.FarmerSprite.CurrentAnimationFrame.xOffset * 4);
			float objectYLoc = f.getLocalPosition(Game1.viewport).Y - 128f + (float)(f.FarmerSprite.CurrentAnimationFrame.positionOffset * 4) + (float)(FarmerRenderer.featureYOffsetPerFrame[f.FarmerSprite.CurrentFrame] * 4);
			if (f.ActiveObject.bigCraftable.Value)
			{
				objectYLoc -= 64f;
			}
			if (f.isEating)
			{
				xPosition = f.getLocalPosition(Game1.viewport).X - 21f;
				objectYLoc = f.getLocalPosition(Game1.viewport).Y - 128f + 12f;
			}
			if (!f.isEating || (f.isEating && f.Sprite.currentFrame <= 218))
			{
				f.ActiveObject.drawWhenHeld(Game1.spriteBatch, new Vector2((int)xPosition, (int)objectYLoc), f);
			}
		}
	}

	public static void drawTool(Farmer f)
	{
		Game1.drawTool(f, f.CurrentTool.CurrentParentTileIndex);
	}

	public static void drawTool(Farmer f, int currentToolIndex)
	{
		Vector2 fPosition = f.getLocalPosition(Game1.viewport) + f.jitter + f.armOffset;
		FarmerSprite farmerSprite = (FarmerSprite)f.Sprite;
		if (f.CurrentTool is MeleeWeapon weapon)
		{
			weapon.drawDuringUse(farmerSprite.currentAnimationIndex, f.FacingDirection, Game1.spriteBatch, fPosition, f);
			return;
		}
		if (f.FarmerSprite.isUsingWeapon())
		{
			MeleeWeapon.drawDuringUse(farmerSprite.currentAnimationIndex, f.FacingDirection, Game1.spriteBatch, fPosition, f, f.FarmerSprite.CurrentToolIndex.ToString(), f.FarmerSprite.getWeaponTypeFromAnimation(), isOnSpecial: false);
			return;
		}
		Tool currentTool = f.CurrentTool;
		if (!(currentTool is Slingshot) && !(currentTool is Shears) && !(currentTool is MilkPail) && !(currentTool is Pan))
		{
			if (!(currentTool is FishingRod) && !(currentTool is WateringCan) && f != Game1.player)
			{
				if (farmerSprite.currentSingleAnimation < 160 || farmerSprite.currentSingleAnimation >= 192)
				{
					return;
				}
				if (f.CurrentTool != null)
				{
					f.CurrentTool.Update(f.FacingDirection, 0, f);
					currentToolIndex = f.CurrentTool.CurrentParentTileIndex;
				}
			}
			Texture2D spritesheet = ItemRegistry.GetData(f.CurrentTool?.QualifiedItemId)?.GetTexture() ?? Game1.toolSpriteSheet;
			Microsoft.Xna.Framework.Rectangle sourceRectangleForTool = new Microsoft.Xna.Framework.Rectangle(currentToolIndex * 16 % spritesheet.Width, currentToolIndex * 16 / spritesheet.Width * 16, 16, 32);
			float base_layer_depth = f.getDrawLayer();
			if (f.CurrentTool is FishingRod rod)
			{
				if (rod.fishCaught || rod.showingTreasure)
				{
					f.CurrentTool.draw(Game1.spriteBatch);
					return;
				}
				sourceRectangleForTool = new Microsoft.Xna.Framework.Rectangle(farmerSprite.currentAnimationIndex * 48, 288, 48, 48);
				if (f.FacingDirection == 2 || f.FacingDirection == 0)
				{
					sourceRectangleForTool.Y += 48;
				}
				else if (rod.isFishing && (!rod.isReeling || rod.hit))
				{
					fPosition.Y += 8f;
				}
				if (rod.isFishing)
				{
					sourceRectangleForTool.X += (5 - farmerSprite.currentAnimationIndex) * 48;
				}
				if (rod.isReeling)
				{
					if (f.FacingDirection == 2 || f.FacingDirection == 0)
					{
						sourceRectangleForTool.X = 288;
						if (f.IsLocalPlayer && Game1.didPlayerJustClickAtAll())
						{
							sourceRectangleForTool.X = 0;
						}
					}
					else
					{
						sourceRectangleForTool.X = 288;
						sourceRectangleForTool.Y = 240;
						if (f.IsLocalPlayer && Game1.didPlayerJustClickAtAll())
						{
							sourceRectangleForTool.Y += 48;
						}
					}
				}
				if (f.FarmerSprite.CurrentFrame == 57)
				{
					sourceRectangleForTool.Height = 0;
				}
				if (f.FacingDirection == 0)
				{
					fPosition.X += 16f;
				}
			}
			f.CurrentTool?.draw(Game1.spriteBatch);
			int toolYOffset = 0;
			int toolXOffset = 0;
			if (f.CurrentTool is WateringCan)
			{
				toolYOffset += 80;
				toolXOffset = ((f.FacingDirection == 1) ? 32 : ((f.FacingDirection == 3) ? (-32) : 0));
				if (farmerSprite.currentAnimationIndex == 0 || farmerSprite.currentAnimationIndex == 1)
				{
					toolXOffset = toolXOffset * 3 / 2;
				}
			}
			toolYOffset += f.yJumpOffset;
			float layerDepth = FarmerRenderer.GetLayerDepth(base_layer_depth, f.FacingDirection switch
			{
				0 => FarmerRenderer.FarmerSpriteLayers.ToolUp, 
				2 => FarmerRenderer.FarmerSpriteLayers.ToolDown, 
				_ => FarmerRenderer.FarmerSpriteLayers.TOOL_IN_USE_SIDE, 
			});
			switch (f.FacingDirection)
			{
			case 1:
			{
				if (farmerSprite.currentAnimationIndex > 2)
				{
					Point tileLocation3 = f.TilePoint;
					tileLocation3.X++;
					tileLocation3.Y--;
					if (!(f.CurrentTool is WateringCan) && f.currentLocation.hasTileAt(tileLocation3, "Front"))
					{
						return;
					}
					tileLocation3.Y++;
				}
				currentTool = f.CurrentTool;
				if (!(currentTool is FishingRod rod3))
				{
					if (currentTool is WateringCan)
					{
						if (farmerSprite.currentAnimationIndex == 1)
						{
							Point tileLocation4 = f.TilePoint;
							tileLocation4.X--;
							tileLocation4.Y--;
							if (f.currentLocation.hasTileAt(tileLocation4, "Front") && f.Position.Y % 64f < 32f)
							{
								return;
							}
						}
						switch (farmerSprite.currentAnimationIndex)
						{
						case 0:
						case 1:
							Game1.spriteBatch.Draw(spritesheet, new Vector2((int)(fPosition.X + (float)toolXOffset - 4f), (int)(fPosition.Y - 128f + 8f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
							break;
						case 2:
							Game1.spriteBatch.Draw(spritesheet, new Vector2((int)fPosition.X + toolXOffset + 24, (int)(fPosition.Y - 128f - 8f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
							break;
						case 3:
							sourceRectangleForTool.X += 16;
							Game1.spriteBatch.Draw(spritesheet, new Vector2((int)(fPosition.X + (float)toolXOffset + 8f), (int)(fPosition.Y - 128f - 24f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
							break;
						}
						return;
					}
					switch (farmerSprite.currentAnimationIndex)
					{
					case 0:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 32f - 4f + (float)toolXOffset - (float)Math.Min(8, f.toolPower.Value * 4), fPosition.Y - 128f + 24f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), sourceRectangleForTool, Color.White, -(float)Math.PI / 12f - (float)Math.Min(f.toolPower.Value, 2) * ((float)Math.PI / 64f), new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 1:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f - 24f + (float)toolXOffset, fPosition.Y - 124f + (float)toolYOffset + 64f)), sourceRectangleForTool, Color.White, (float)Math.PI / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 2:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + (float)toolXOffset - 4f, fPosition.Y - 132f + (float)toolYOffset + 64f)), sourceRectangleForTool, Color.White, (float)Math.PI / 4f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 3:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 28f + (float)toolXOffset, fPosition.Y - 64f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * 7f / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 4:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 28f + (float)toolXOffset, fPosition.Y - 64f + 4f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * 7f / 12f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 5:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 64f + 12f + (float)toolXOffset, fPosition.Y - 128f + 32f + (float)toolYOffset + 128f)), sourceRectangleForTool, Color.White, (float)Math.PI / 4f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 6:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 42f + 8f + (float)toolXOffset, fPosition.Y - 64f + 24f + (float)toolYOffset + 128f)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 128f), 4f, SpriteEffects.None, layerDepth);
						break;
					}
					return;
				}
				Color color2 = rod3.getColor();
				switch (farmerSprite.currentAnimationIndex)
				{
				case 0:
					if (rod3.isReeling || rod3.isFishing || rod3.doneWithAnimation || !rod3.hasDoneFucntionYet || rod3.pullingOutOfWater)
					{
						Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), sourceRectangleForTool, color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 1:
					Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 8f + (float)toolYOffset), sourceRectangleForTool, color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				case 2:
					Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 96f + 32f + (float)toolXOffset, fPosition.Y - 128f - 24f + (float)toolYOffset), sourceRectangleForTool, color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				case 3:
					Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 96f + 24f + (float)toolXOffset, fPosition.Y - 128f - 32f + (float)toolYOffset), sourceRectangleForTool, color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				case 4:
					if (rod3.isFishing || rod3.doneWithAnimation)
					{
						Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), sourceRectangleForTool, color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					}
					else
					{
						Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 4f + (float)toolYOffset), sourceRectangleForTool, color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 5:
					Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), sourceRectangleForTool, color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
					break;
				}
				return;
			}
			case 3:
			{
				if (farmerSprite.currentAnimationIndex > 2)
				{
					Point tileLocation = f.TilePoint;
					tileLocation.X--;
					tileLocation.Y--;
					if (!(f.CurrentTool is WateringCan) && f.currentLocation.hasTileAt(tileLocation, "Front") && f.Position.Y % 64f < 32f)
					{
						return;
					}
					tileLocation.Y++;
				}
				currentTool = f.CurrentTool;
				if (!(currentTool is FishingRod rod2))
				{
					if (currentTool is WateringCan)
					{
						if (farmerSprite.currentAnimationIndex == 1)
						{
							Point tileLocation2 = f.TilePoint;
							tileLocation2.X--;
							tileLocation2.Y--;
							if (f.currentLocation.hasTileAt(tileLocation2, "Front") && f.Position.Y % 64f < 32f)
							{
								return;
							}
						}
						switch (farmerSprite.currentAnimationIndex)
						{
						case 0:
						case 1:
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 4f, fPosition.Y - 128f + 8f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 2:
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 16f, fPosition.Y - 128f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 3:
							sourceRectangleForTool.X += 16;
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 16f, fPosition.Y - 128f - 24f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						}
					}
					else
					{
						switch (farmerSprite.currentAnimationIndex)
						{
						case 0:
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 8f + (float)toolXOffset + (float)Math.Min(8, f.toolPower.Value * 4), fPosition.Y - 128f + 8f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), sourceRectangleForTool, Color.White, (float)Math.PI / 12f + (float)Math.Min(f.toolPower.Value, 2) * ((float)Math.PI / 64f), new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 1:
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 16f + (float)toolXOffset, fPosition.Y - 128f + 16f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 2:
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 4f + (float)toolXOffset, fPosition.Y - 128f + 60f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 4f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 3:
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 20f + (float)toolXOffset, fPosition.Y - 64f + 76f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * -7f / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						case 4:
							Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 24f + (float)toolXOffset, fPosition.Y + 24f + (float)toolYOffset)), sourceRectangleForTool, Color.White, (float)Math.PI * -7f / 12f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
							break;
						}
					}
					return;
				}
				Color color = rod2.getColor();
				switch (farmerSprite.currentAnimationIndex)
				{
				case 0:
					if (rod2.isReeling || rod2.isFishing || rod2.doneWithAnimation || !rod2.hasDoneFucntionYet || rod2.pullingOutOfWater)
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					}
					break;
				case 1:
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 8f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				case 2:
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 96f + 32f + (float)toolXOffset, fPosition.Y - 128f - 24f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				case 3:
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 96f + 24f + (float)toolXOffset, fPosition.Y - 128f - 32f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				case 4:
					if (rod2.isFishing || rod2.doneWithAnimation)
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					}
					else
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 4f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					}
					break;
				case 5:
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), sourceRectangleForTool, color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
					break;
				}
				return;
			}
			}
			if (farmerSprite.currentAnimationIndex > 2 && !(f.CurrentTool is FishingRod { isCasting: false, castedButBobberStillInAir: false, isTimingCast: false }))
			{
				Point tileLocation5 = f.TilePoint;
				if (f.currentLocation.hasTileAt(tileLocation5, "Front") && f.Position.Y % 64f < 32f && f.Position.Y % 64f > 16f)
				{
					return;
				}
			}
			currentTool = f.CurrentTool;
			if (!(currentTool is FishingRod fishingRod))
			{
				if (currentTool is WateringCan)
				{
					switch (farmerSprite.currentAnimationIndex)
					{
					case 0:
					case 1:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f + 16f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 2:
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f - (float)((f.FacingDirection == 2) ? (-4) : 32) + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					case 3:
						if (f.FacingDirection == 2)
						{
							sourceRectangleForTool.X += 16;
						}
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - (float)((f.FacingDirection == 2) ? 4 : 0), fPosition.Y - 128f - (float)((f.FacingDirection == 2) ? (-24) : 64) + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
						break;
					}
					return;
				}
				switch (farmerSprite.currentAnimationIndex)
				{
				case 0:
					if (f.FacingDirection == 0)
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f - 8f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					else
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 20f, fPosition.Y - 128f + 12f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 1:
					if (f.FacingDirection == 0)
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset + 4f, fPosition.Y - 128f + 40f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					else
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 12f, fPosition.Y - 128f + 32f + (float)toolYOffset)), sourceRectangleForTool, Color.White, -(float)Math.PI / 24f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 2:
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f + 64f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					break;
				case 3:
					if (f.FacingDirection != 0)
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 44f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 4:
					if (f.FacingDirection != 0)
					{
						Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 48f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					}
					break;
				case 5:
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 32f + (float)toolYOffset)), sourceRectangleForTool, Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
					break;
				}
				return;
			}
			if (farmerSprite.currentAnimationIndex <= 2)
			{
				Point tileLocation6 = f.TilePoint;
				tileLocation6.Y--;
				if (f.currentLocation.hasTileAt(tileLocation6, "Front"))
				{
					return;
				}
			}
			if (f.FacingDirection == 2)
			{
				layerDepth += 0.01f;
			}
			Color color3 = fishingRod.getColor();
			switch (farmerSprite.currentAnimationIndex)
			{
			case 0:
				if (!fishingRod.showingTreasure && !fishingRod.fishCaught && (f.FacingDirection != 0 || !fishingRod.isFishing || fishingRod.isReeling))
				{
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			case 1:
				Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				break;
			case 2:
				Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				break;
			case 3:
				if (f.FacingDirection == 2)
				{
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			case 4:
				if (f.FacingDirection == 0 && fishingRod.isFishing)
				{
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 80f, fPosition.Y - 96f)), sourceRectangleForTool, color3, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, layerDepth);
				}
				else if (f.FacingDirection == 2)
				{
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			case 5:
				if (f.FacingDirection == 2 && !fishingRod.showingTreasure && !fishingRod.fishCaught)
				{
					Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), sourceRectangleForTool, color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
				break;
			}
		}
		else
		{
			f.CurrentTool.draw(Game1.spriteBatch);
		}
	}

	/// ####################
	/// OTHER HELPER METHODS
	/// ####################
	public static Vector2 GlobalToLocal(xTile.Dimensions.Rectangle viewport, Vector2 globalPosition)
	{
		return new Vector2(globalPosition.X - (float)viewport.X, globalPosition.Y - (float)viewport.Y);
	}

	public static bool IsEnglish()
	{
		return Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en;
	}

	public static Vector2 GlobalToLocal(Vector2 globalPosition)
	{
		return new Vector2(globalPosition.X - (float)Game1.viewport.X, globalPosition.Y - (float)Game1.viewport.Y);
	}

	public static Microsoft.Xna.Framework.Rectangle GlobalToLocal(xTile.Dimensions.Rectangle viewport, Microsoft.Xna.Framework.Rectangle globalPosition)
	{
		return new Microsoft.Xna.Framework.Rectangle(globalPosition.X - viewport.X, globalPosition.Y - viewport.Y, globalPosition.Width, globalPosition.Height);
	}

	public static string parseText(string text, SpriteFont whichFont, int width)
	{
		if (text == null)
		{
			return "";
		}
		text = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
		Game1._ParseTextStringBuilder.Clear();
		Game1._ParseTextStringBuilderLine.Clear();
		Game1._ParseTextStringBuilderWord.Clear();
		float current_width = 0f;
		LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
		if (currentLanguageCode == LocalizedContentManager.LanguageCode.ja || currentLanguageCode == LocalizedContentManager.LanguageCode.zh || currentLanguageCode == LocalizedContentManager.LanguageCode.th)
		{
			foreach (object item in Game1.asianSpacingRegex.Matches(text))
			{
				string s = item.ToString();
				float character_width = whichFont.MeasureString(s).X + whichFont.Spacing;
				if (current_width + character_width > (float)width || s.Equals(Environment.NewLine) || s.Equals("\n"))
				{
					Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
					Game1._ParseTextStringBuilder.Append(Environment.NewLine);
					Game1._ParseTextStringBuilderLine.Clear();
					current_width = 0f;
				}
				if (!s.Equals(Environment.NewLine) && !s.Equals("\n"))
				{
					Game1._ParseTextStringBuilderLine.Append(s);
					current_width += character_width;
				}
			}
			Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
			return Game1._ParseTextStringBuilder.ToString();
		}
		current_width = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			bool check_width;
			if (c != '\n')
			{
				if (c == '\r')
				{
					continue;
				}
				if (c == ' ')
				{
					check_width = true;
				}
				else
				{
					Game1._ParseTextStringBuilderWord.Append(c);
					check_width = i == text.Length - 1;
				}
			}
			else
			{
				check_width = true;
			}
			if (!check_width)
			{
				continue;
			}
			try
			{
				float word_width = whichFont.MeasureString(Game1._ParseTextStringBuilderWord).X + whichFont.Spacing;
				if (current_width + word_width > (float)width)
				{
					Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
					Game1._ParseTextStringBuilder.Append(Environment.NewLine);
					Game1._ParseTextStringBuilderLine.Clear();
					current_width = 0f;
				}
				if (c == '\n')
				{
					Game1._ParseTextStringBuilderLine.Append(Game1._ParseTextStringBuilderWord);
					Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
					Game1._ParseTextStringBuilder.Append(Environment.NewLine);
					Game1._ParseTextStringBuilderLine.Clear();
					Game1._ParseTextStringBuilderWord.Clear();
					current_width = 0f;
					continue;
				}
				Game1._ParseTextStringBuilderLine.Append(Game1._ParseTextStringBuilderWord);
				Game1._ParseTextStringBuilderLine.Append(" ");
				float space_width = whichFont.MeasureString(" ").X + whichFont.Spacing;
				current_width += word_width + space_width;
			}
			catch (Exception exception)
			{
				Game1.log.Error("Exception measuring string: ", exception);
			}
			Game1._ParseTextStringBuilderWord.Clear();
		}
		Game1._ParseTextStringBuilderLine.Append(Game1._ParseTextStringBuilderWord);
		Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
		return Game1._ParseTextStringBuilder.ToString();
	}

	public static void UpdateHorseOwnership()
	{
		bool verbose = false;
		Dictionary<long, Horse> horse_lookup = new Dictionary<long, Horse>();
		HashSet<Horse> claimed_horses = new HashSet<Horse>();
		List<Stable> stables = new List<Stable>();
		Utility.ForEachBuilding(delegate(Stable item)
		{
			stables.Add(item);
			return true;
		});
		foreach (Stable stable in stables)
		{
			if (stable.owner.Value == -6666666 && Game1.GetPlayer(-6666666L) == null)
			{
				stable.owner.Value = Game1.player.UniqueMultiplayerID;
			}
			stable.grabHorse();
		}
		foreach (Stable item in stables)
		{
			Horse horse = item.getStableHorse();
			if (horse != null && !claimed_horses.Contains(horse) && horse.getOwner() != null && !horse_lookup.ContainsKey(horse.getOwner().UniqueMultiplayerID) && horse.getOwner().horseName.Value != null && horse.getOwner().horseName.Value.Length > 0 && horse.Name == horse.getOwner().horseName.Value)
			{
				horse_lookup[horse.getOwner().UniqueMultiplayerID] = horse;
				claimed_horses.Add(horse);
				if (verbose)
				{
					Game1.log.Verbose("Assigned horse " + horse.Name + " to " + horse.getOwner().Name + " (Exact match)");
				}
			}
		}
		Dictionary<string, Farmer> horse_name_lookup = new Dictionary<string, Farmer>();
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (string.IsNullOrEmpty(farmer?.horseName.Value))
			{
				continue;
			}
			bool fail = false;
			foreach (Horse item2 in claimed_horses)
			{
				if (item2.getOwner() == farmer)
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				horse_name_lookup[farmer.horseName.Value] = farmer;
			}
		}
		foreach (Stable stable2 in stables)
		{
			Horse horse2 = stable2.getStableHorse();
			if (horse2 != null && !claimed_horses.Contains(horse2) && horse2.getOwner() != null && horse2.Name != null && horse2.Name.Length > 0 && horse_name_lookup.TryGetValue(horse2.Name, out var owner) && !horse_lookup.ContainsKey(owner.UniqueMultiplayerID))
			{
				stable2.owner.Value = owner.UniqueMultiplayerID;
				stable2.updateHorseOwnership();
				horse_lookup[horse2.getOwner().UniqueMultiplayerID] = horse2;
				claimed_horses.Add(horse2);
				if (verbose)
				{
					Game1.log.Verbose("Assigned horse " + horse2.Name + " to " + horse2.getOwner().Name + " (Name match from different owner.)");
				}
			}
		}
		foreach (Stable stable3 in stables)
		{
			Horse horse3 = stable3.getStableHorse();
			if (horse3 != null && !claimed_horses.Contains(horse3) && horse3.getOwner() != null && !horse_lookup.ContainsKey(horse3.getOwner().UniqueMultiplayerID))
			{
				horse_lookup[horse3.getOwner().UniqueMultiplayerID] = horse3;
				claimed_horses.Add(horse3);
				stable3.updateHorseOwnership();
				if (verbose)
				{
					Game1.log.Verbose("Assigned horse " + horse3.Name + " to " + horse3.getOwner().Name + " (Owner's only stable)");
				}
			}
		}
		foreach (Stable stable4 in stables)
		{
			Horse horse4 = stable4.getStableHorse();
			if (horse4 == null || claimed_horses.Contains(horse4))
			{
				continue;
			}
			foreach (Horse claimed_horse in claimed_horses)
			{
				if (horse4.ownerId == claimed_horse.ownerId)
				{
					stable4.owner.Value = 0L;
					stable4.updateHorseOwnership();
					if (verbose)
					{
						Game1.log.Verbose("Unassigned horse (stable owner already has a horse).");
					}
					break;
				}
			}
		}
	}

	public static string LoadStringByGender(Gender npcGender, string key)
	{
		if (npcGender == Gender.Male)
		{
			return Game1.content.LoadString(key).Split('/')[0];
		}
		return Game1.content.LoadString(key).Split('/').Last();
	}

	public static string LoadStringByGender(Gender npcGender, string key, params object[] substitutions)
	{
		string sentence;
		if (npcGender == Gender.Male)
		{
			sentence = Game1.content.LoadString(key).Split('/')[0];
			if (substitutions.Length != 0)
			{
				try
				{
					return string.Format(sentence, substitutions);
				}
				catch
				{
					return sentence;
				}
			}
		}
		sentence = Game1.content.LoadString(key).Split('/').Last();
		if (substitutions.Length != 0)
		{
			try
			{
				return string.Format(sentence, substitutions);
			}
			catch
			{
				return sentence;
			}
		}
		return sentence;
	}

	public static string parseText(string text)
	{
		return Game1.parseText(text, Game1.dialogueFont, Game1.dialogueWidth);
	}

	public static Microsoft.Xna.Framework.Rectangle getSourceRectForStandardTileSheet(Texture2D tileSheet, int tilePosition, int width = -1, int height = -1)
	{
		if (width == -1)
		{
			width = 64;
		}
		if (height == -1)
		{
			height = 64;
		}
		return new Microsoft.Xna.Framework.Rectangle(tilePosition * width % tileSheet.Width, tilePosition * width / tileSheet.Width * height, width, height);
	}

	public static Microsoft.Xna.Framework.Rectangle getSquareSourceRectForNonStandardTileSheet(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
	{
		return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
	}

	public static Microsoft.Xna.Framework.Rectangle getArbitrarySourceRect(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
	{
		if (tileSheet != null)
		{
			return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
		}
		return Microsoft.Xna.Framework.Rectangle.Empty;
	}

	public static string getTimeOfDayString(int time)
	{
		string zeroPad = ((time % 100 == 0) ? "0" : string.Empty);
		string hours;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		default:
			hours = ((time / 100 % 12 == 0) ? "12" : (time / 100 % 12).ToString());
			break;
		case LocalizedContentManager.LanguageCode.ja:
			hours = ((time / 100 % 12 == 0) ? "0" : (time / 100 % 12).ToString());
			break;
		case LocalizedContentManager.LanguageCode.zh:
			hours = (time / 100 % 24).ToString();
			break;
		case LocalizedContentManager.LanguageCode.ru:
		case LocalizedContentManager.LanguageCode.pt:
		case LocalizedContentManager.LanguageCode.es:
		case LocalizedContentManager.LanguageCode.de:
		case LocalizedContentManager.LanguageCode.th:
		case LocalizedContentManager.LanguageCode.fr:
		case LocalizedContentManager.LanguageCode.tr:
		case LocalizedContentManager.LanguageCode.hu:
			hours = (time / 100 % 24).ToString();
			hours = ((time / 100 % 24 <= 9) ? ("0" + hours) : hours);
			break;
		}
		string timeText = hours + ":" + time % 100 + zeroPad;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.en:
			return timeText + " " + ((time < 1200 || time >= 2400) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
		case LocalizedContentManager.LanguageCode.ja:
			if (time >= 1200 && time < 2400)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + timeText;
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + timeText;
		case LocalizedContentManager.LanguageCode.fr:
			if (time % 100 != 0)
			{
				return hours + "h" + time % 100;
			}
			return hours + "h";
		case LocalizedContentManager.LanguageCode.mod:
			return LocalizedContentManager.FormatTimeString(time, LocalizedContentManager.CurrentModLanguage.TimeFormat).ToString();
		default:
			return timeText;
		}
	}

	public static bool[,] getCircleOutlineGrid(int radius)
	{
		bool[,] circleGrid = new bool[radius * 2 + 1, radius * 2 + 1];
		int f = 1 - radius;
		int ddF_x = 1;
		int ddF_y = -2 * radius;
		int x = 0;
		int y = radius;
		circleGrid[radius, radius + radius] = true;
		circleGrid[radius, radius - radius] = true;
		circleGrid[radius + radius, radius] = true;
		circleGrid[radius - radius, radius] = true;
		while (x < y)
		{
			if (f >= 0)
			{
				y--;
				ddF_y += 2;
				f += ddF_y;
			}
			x++;
			ddF_x += 2;
			f += ddF_x;
			circleGrid[radius + x, radius + y] = true;
			circleGrid[radius - x, radius + y] = true;
			circleGrid[radius + x, radius - y] = true;
			circleGrid[radius - x, radius - y] = true;
			circleGrid[radius + y, radius + x] = true;
			circleGrid[radius - y, radius + x] = true;
			circleGrid[radius + y, radius - x] = true;
			circleGrid[radius - y, radius - x] = true;
		}
		return circleGrid;
	}

	/// <summary>Get the internal identifier for the current farm type. This is either the numeric index for a vanilla farm, or the <see cref="F:StardewValley.GameData.ModFarmType.Id" /> field for a custom type.</summary>
	public static string GetFarmTypeID()
	{
		if (Game1.whichFarm != 7 || Game1.whichModFarm == null)
		{
			return Game1.whichFarm.ToString();
		}
		return Game1.whichModFarm.Id;
	}

	/// <summary>Get the human-readable identifier for the current farm type. For a custom farm type, this is equivalent to <see cref="M:StardewValley.Game1.GetFarmTypeID" />.</summary>
	public static string GetFarmTypeKey()
	{
		return Game1.whichFarm switch
		{
			0 => "Standard", 
			1 => "Riverland", 
			2 => "Forest", 
			3 => "Hilltop", 
			4 => "Wilderness", 
			5 => "FourCorners", 
			6 => "Beach", 
			_ => Game1.GetFarmTypeID(), 
		};
	}

	public void _PerformRemoveNormalItemFromWorldOvernight(string itemId)
	{
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			this._RecursiveRemoveThisNormalItemLocation(location, itemId);
			return true;
		}, includeInteriors: true, includeGenerated: true);
		Game1.player.team.returnedDonations.RemoveWhere((Item this_item) => this._RecursiveRemoveThisNormalItemItem(this_item, itemId));
		foreach (Inventory value in Game1.player.team.globalInventories.Values)
		{
			value.RemoveWhere((Item this_item) => this._RecursiveRemoveThisNormalItemItem(this_item, itemId));
		}
		foreach (SpecialOrder order in Game1.player.team.specialOrders)
		{
			for (int i = 0; i < order.donatedItems.Count; i++)
			{
				Item item = order.donatedItems[i];
				if (this._RecursiveRemoveThisNormalItemItem(item, itemId))
				{
					order.donatedItems[i] = null;
				}
			}
		}
	}

	protected virtual void _PerformRemoveNormalItemFromFarmerOvernight(Farmer farmer, string itemId)
	{
		for (int i = 0; i < farmer.Items.Count; i++)
		{
			if (this._RecursiveRemoveThisNormalItemItem(farmer.Items[i], itemId))
			{
				farmer.Items[i] = null;
			}
		}
		farmer.itemsLostLastDeath.RemoveWhere((Item item) => this._RecursiveRemoveThisNormalItemItem(item, itemId));
		if (farmer.recoveredItem != null && this._RecursiveRemoveThisNormalItemItem(farmer.recoveredItem, itemId))
		{
			farmer.recoveredItem = null;
			farmer.mailbox.Remove("MarlonRecovery");
			farmer.mailForTomorrow.Remove("MarlonRecovery");
		}
		if (farmer.toolBeingUpgraded.Value != null && this._RecursiveRemoveThisNormalItemItem(farmer.toolBeingUpgraded.Value, itemId))
		{
			farmer.toolBeingUpgraded.Value = null;
		}
	}

	protected virtual bool _RecursiveRemoveThisNormalItemItem(Item this_item, string itemId)
	{
		if (this_item != null)
		{
			if (this_item is Object o)
			{
				if (o.heldObject.Value != null && this._RecursiveRemoveThisNormalItemItem(o.heldObject.Value, itemId))
				{
					o.ResetParentSheetIndex();
					o.heldObject.Value = null;
					o.readyForHarvest.Value = false;
					o.showNextIndex.Value = false;
				}
				if (!(o is StorageFurniture furniture))
				{
					if (!(o is IndoorPot pot))
					{
						if (o is Chest chest)
						{
							bool removed_item = false;
							IInventory items = chest.Items;
							for (int i = 0; i < items.Count; i++)
							{
								Item item = items[i];
								if (item != null && this._RecursiveRemoveThisNormalItemItem(item, itemId))
								{
									items[i] = null;
									removed_item = true;
								}
							}
							if (removed_item)
							{
								chest.clearNulls();
							}
						}
					}
					else if (pot.hoeDirt.Value != null)
					{
						this._RecursiveRemoveThisNormalItemDirt(pot.hoeDirt.Value, null, Vector2.Zero, itemId);
					}
				}
				else
				{
					bool removed_item2 = false;
					for (int j = 0; j < furniture.heldItems.Count; j++)
					{
						Item item2 = furniture.heldItems[j];
						if (item2 != null && this._RecursiveRemoveThisNormalItemItem(item2, itemId))
						{
							furniture.heldItems[j] = null;
							removed_item2 = true;
						}
					}
					if (removed_item2)
					{
						furniture.ClearNulls();
					}
				}
				if (o.heldObject.Value != null && this._RecursiveRemoveThisNormalItemItem(o.heldObject.Value, itemId))
				{
					o.heldObject.Value = null;
				}
			}
			return Utility.IsNormalObjectAtParentSheetIndex(this_item, itemId);
		}
		return false;
	}

	protected virtual void _RecursiveRemoveThisNormalItemDirt(HoeDirt dirt, GameLocation location, Vector2 coord, string itemId)
	{
		if (dirt.crop != null && dirt.crop.indexOfHarvest.Value == itemId)
		{
			dirt.destroyCrop(showAnimation: false);
		}
	}

	protected virtual void _RecursiveRemoveThisNormalItemLocation(GameLocation l, string itemId)
	{
		if (l == null)
		{
			return;
		}
		List<Guid> removed_items = new List<Guid>();
		foreach (Furniture furniture in l.furniture)
		{
			if (this._RecursiveRemoveThisNormalItemItem(furniture, itemId))
			{
				removed_items.Add(l.furniture.GuidOf(furniture));
			}
		}
		foreach (Guid guid in removed_items)
		{
			l.furniture.Remove(guid);
		}
		foreach (NPC character in l.characters)
		{
			if (character is Monster monster)
			{
				monster.objectsToDrop?.RemoveWhere((string id) => id == itemId);
			}
		}
		Chest fridge = l.GetFridge(onlyUnlocked: false);
		if (fridge != null)
		{
			IInventory fridgeItems = fridge.Items;
			for (int i = 0; i < fridgeItems.Count; i++)
			{
				Item item = fridgeItems[i];
				if (item != null && this._RecursiveRemoveThisNormalItemItem(item, itemId))
				{
					fridgeItems[i] = null;
				}
			}
		}
		foreach (Vector2 coord in l.terrainFeatures.Keys)
		{
			if (l.terrainFeatures[coord] is HoeDirt dirt)
			{
				this._RecursiveRemoveThisNormalItemDirt(dirt, l, coord, itemId);
			}
		}
		foreach (Building building in l.buildings)
		{
			foreach (Chest chest in building.buildingChests)
			{
				bool anyRemoved = false;
				for (int i2 = 0; i2 < chest.Items.Count; i2++)
				{
					Item item2 = chest.Items[i2];
					if (item2 != null && this._RecursiveRemoveThisNormalItemItem(item2, itemId))
					{
						chest.Items[i2] = null;
						anyRemoved = true;
					}
				}
				if (anyRemoved)
				{
					chest.clearNulls();
				}
			}
		}
		Vector2[] array = l.objects.Keys.ToArray();
		foreach (Vector2 key in array)
		{
			Object obj = l.objects[key];
			if (obj != fridge && this._RecursiveRemoveThisNormalItemItem(obj, itemId))
			{
				l.objects.Remove(key);
			}
		}
		l.debris.RemoveWhere((Debris debris) => debris.item != null && this._RecursiveRemoveThisNormalItemItem(debris.item, itemId));
		if (l is ShopLocation shopLocation)
		{
			shopLocation.itemsFromPlayerToSell.RemoveWhere((Item this_item) => this._RecursiveRemoveThisNormalItemItem(this_item, itemId));
			shopLocation.itemsToStartSellingTomorrow.RemoveWhere((Item this_item) => this._RecursiveRemoveThisNormalItemItem(this_item, itemId));
		}
	}

	public static bool GetHasRoomAnotherFarm()
	{
		return true;
	}

	/// <summary>Reset most game state when returning to the title screen from a title sub-menu.</summary>
	/// <remarks>
	///   <para>This is specialized code that normally shouldn't be called directly. This differs from <see cref="M:StardewValley.Game1.CleanupReturningToTitle" /> in that it doesn't exit split screens, reset the title menu, or reset the game window.</para>
	///   <para>This is called automatically from <see cref="M:StardewValley.Game1.CleanupReturningToTitle" />.</para>
	/// </remarks>
	public virtual void ResetGameStateOnTitleScreen()
	{
		LocalizedContentManager.localizedAssetNames.Clear();
		Event.invalidFestivals.Clear();
		NPC.invalidDialogueFiles.Clear();
		SaveGame.CancelToTitle = false;
		Game1.overlayMenu = null;
		Game1.multiplayer.cachedMultiplayerMaps.Clear();
		Game1.keyboardFocusInstance = null;
		BuildingPaintMenu.savedColors = null;
		Game1.startingGameSeed = null;
		Game1.UseLegacyRandom = false;
		Game1._afterNewDayAction = null;
		Game1._currentMinigame = null;
		Game1.gameMode = 0;
		this._isSaving = false;
		Game1._mouseCursorTransparency = 1f;
		Game1._newDayTask = null;
		Game1.newDaySync.destroy();
		Game1.netReady.Reset();
		Game1.dedicatedServer.Reset();
		Game1.resetPlayer();
		Game1.afterDialogues = null;
		Game1.afterFade = null;
		Game1.afterPause = null;
		Game1.afterViewport = null;
		Game1.ambientLight = new Color(0, 0, 0, 0);
		Game1.background = null;
		Game1.chatBox = null;
		Game1.specialCurrencyDisplay?.Cleanup();
		GameLocation.PlayedNewLocationContextMusic = false;
		Game1.IsPlayingBackgroundMusic = false;
		Game1.IsPlayingNightAmbience = false;
		Game1.IsPlayingOutdoorsAmbience = false;
		Game1.IsPlayingMorningSong = false;
		Game1.IsPlayingTownMusic = false;
		Game1.specialCurrencyDisplay = null;
		Game1.conventionMode = false;
		Game1.currentCursorTile = Vector2.Zero;
		Game1.currentDialogueCharacterIndex = 0;
		Game1.currentLightSources.Clear();
		Game1.currentLoader = null;
		Game1.currentLocation = null;
		Game1._PreviousNonNullLocation = null;
		Game1.currentObjectDialogue.Clear();
		Game1.currentQuestionChoice = 0;
		Game1.season = Season.Spring;
		Game1.currentSpeaker = null;
		Game1.currentViewportTarget = Vector2.Zero;
		Game1.cursorTileHintCheckTimer = 0;
		Game1.CustomData = new SerializableDictionary<string, string>();
		Game1.player.team.sharedDailyLuck.Value = 0.001;
		Game1.dayOfMonth = 0;
		Game1.debrisSoundInterval = 0f;
		Game1.debrisWeather.Clear();
		Game1.debugMode = false;
		Game1.debugOutput = null;
		Game1.debugPresenceString = "In menus";
		Game1.delayedActions.Clear();
		Game1.morningSongPlayAction = null;
		Game1.dialogueButtonScale = 1f;
		Game1.dialogueButtonShrinking = false;
		Game1.dialogueTyping = false;
		Game1.dialogueTypingInterval = 0;
		Game1.dialogueUp = false;
		Game1.dialogueWidth = 1024;
		Game1.displayFarmer = true;
		Game1.displayHUD = true;
		Game1.downPolling = 0f;
		Game1.drawGrid = false;
		Game1.drawLighting = false;
		Game1.elliottBookName = "Blue Tower";
		Game1.endOfNightMenus.Clear();
		Game1.errorMessage = "";
		Game1.eveningColor = new Color(255, 255, 0, 255);
		Game1.eventOver = false;
		Game1.eventUp = false;
		Game1.exitToTitle = false;
		Game1.facingDirectionAfterWarp = 0;
		Game1.fadeIn = true;
		Game1.fadeToBlack = false;
		Game1.fadeToBlackAlpha = 1.02f;
		Game1.farmEvent = null;
		Game1.flashAlpha = 0f;
		Game1.freezeControls = false;
		Game1.gamePadAButtonPolling = 0;
		Game1.gameTimeInterval = 0;
		Game1.globalFade = false;
		Game1.globalFadeSpeed = 0f;
		Game1.haltAfterCheck = false;
		Game1.hasLoadedGame = false;
		Game1.hasStartedDay = false;
		Game1.hitShakeTimer = 0;
		Game1.hudMessages.Clear();
		Game1.isActionAtCurrentCursorTile = false;
		Game1.isDebrisWeather = false;
		Game1.isInspectionAtCurrentCursorTile = false;
		Game1.isLightning = false;
		Game1.isQuestion = false;
		Game1.isRaining = false;
		Game1.wasGreenRain = false;
		Game1.isSnowing = false;
		Game1.killScreen = false;
		Game1.lastCursorMotionWasMouse = true;
		Game1.lastCursorTile = Vector2.Zero;
		Game1.lastMousePositionBeforeFade = Point.Zero;
		Game1.leftPolling = 0f;
		Game1.loadingMessage = "";
		Game1.locationRequest = null;
		Game1.warpingForForcedRemoteEvent = false;
		Game1.locations.Clear();
		Game1.mailbox.Clear();
		Game1.mapDisplayDevice = this.CreateDisplayDevice(Game1.content, base.GraphicsDevice);
		Game1.messageAfterPause = "";
		Game1.messagePause = false;
		Game1.mouseClickPolling = 0;
		Game1.mouseCursor = Game1.cursor_default;
		Game1.multiplayerMode = 0;
		Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
		Game1.newDay = false;
		Game1.nonWarpFade = false;
		Game1.noteBlockTimer = 0f;
		Game1.npcDialogues = null;
		Game1.objectDialoguePortraitPerson = null;
		Game1.hasApplied1_3_UpdateChanges = false;
		Game1.hasApplied1_4_UpdateChanges = false;
		Game1.remoteEventQueue.Clear();
		Game1.bannedUsers?.Clear();
		Game1.nextClickableMenu.Clear();
		Game1.actionsWhenPlayerFree.Clear();
		Game1.onScreenMenus.Clear();
		Game1.onScreenMenus.Add(new Toolbar());
		Game1.dayTimeMoneyBox = new DayTimeMoneyBox();
		Game1.onScreenMenus.Add(Game1.dayTimeMoneyBox);
		Game1.buffsDisplay = new BuffsDisplay();
		Game1.onScreenMenus.Add(Game1.buffsDisplay);
		bool gamepad_controls = Game1.options.gamepadControls;
		bool snappy_menus = Game1.options.snappyMenus;
		Game1.options = new Options();
		Game1.options.gamepadControls = gamepad_controls;
		Game1.options.snappyMenus = snappy_menus;
		foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
		{
			otherFarmer.Value.unload();
		}
		Game1.otherFarmers.Clear();
		Game1.outdoorLight = new Color(255, 255, 0, 255);
		Game1.overlayMenu = null;
		this.panFacingDirectionWait = false;
		Game1.panMode = false;
		this.panModeString = null;
		Game1.pauseAccumulator = 0f;
		Game1.paused = false;
		Game1.pauseThenDoFunctionTimer = 0;
		Game1.pauseTime = 0f;
		Game1.previousViewportPosition = Vector2.Zero;
		Game1.questionChoices.Clear();
		Game1.quit = false;
		Game1.rightClickPolling = 0;
		Game1.rightPolling = 0f;
		Game1.runThreshold = 0.5f;
		Game1.samBandName = "The Alfalfas";
		Game1.saveOnNewDay = true;
		Game1.startingCabins = 0;
		Game1.cabinsSeparate = false;
		Game1.screenGlow = false;
		Game1.screenGlowAlpha = 0f;
		Game1.screenGlowColor = new Color(0, 0, 0, 0);
		Game1.screenGlowHold = false;
		Game1.screenGlowMax = 0f;
		Game1.screenGlowRate = 0.005f;
		Game1.screenGlowUp = false;
		Game1.screenOverlayTempSprites.Clear();
		Game1.uiOverlayTempSprites.Clear();
		this.newGameSetupOptions.Clear();
		Game1.showingEndOfNightStuff = false;
		Game1.spawnMonstersAtNight = false;
		Game1.staminaShakeTimer = 0;
		Game1.textColor = new Color(34, 17, 34, 255);
		Game1.textShadowColor = new Color(206, 156, 95, 255);
		Game1.thumbstickMotionAccell = 1f;
		Game1.thumbstickMotionMargin = 0;
		Game1.thumbstickPollingTimer = 0;
		Game1.thumbStickSensitivity = 0.1f;
		Game1.timeOfDay = 600;
		Game1.timeOfDayAfterFade = -1;
		Game1.timerUntilMouseFade = 0;
		Game1.toggleFullScreen = false;
		Game1.ResetToolSpriteSheet();
		Game1.triggerPolling = 0;
		Game1.uniqueIDForThisGame = (ulong)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalSeconds;
		Game1.upPolling = 0f;
		Game1.viewportFreeze = false;
		Game1.viewportHold = 0;
		Game1.viewportPositionLerp = Vector2.Zero;
		Game1.viewportReachedTarget = null;
		Game1.viewportSpeed = 2f;
		Game1.viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
		Game1.wasMouseVisibleThisFrame = true;
		Game1.wasRainingYesterday = false;
		Game1.weatherForTomorrow = "Sun";
		Game1.elliottPiano = 0;
		Game1.weatherIcon = 0;
		Game1.weddingToday = false;
		Game1.whereIsTodaysFest = null;
		Game1.worldStateIDs.Clear();
		Game1.whichFarm = 0;
		Game1.whichModFarm = null;
		Game1.windGust = 0f;
		Game1.xLocationAfterWarp = 0;
		Game1.game1.xTileContent.Dispose();
		Game1.game1.xTileContent = this.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
		Game1.year = 1;
		Game1.yLocationAfterWarp = 0;
		Game1.mailDeliveredFromMailForTomorrow.Clear();
		Game1.bundleType = BundleType.Default;
		JojaMart.Morris = null;
		AmbientLocationSounds.onLocationLeave();
		WeatherDebris.globalWind = -0.25f;
		Utility.killAllStaticLoopingSoundCues();
		OptionsDropDown.selected = null;
		JunimoNoteMenu.tempSprites.Clear();
		JunimoNoteMenu.screenSwipe = null;
		JunimoNoteMenu.canClick = true;
		GameMenu.forcePreventClose = false;
		Club.timesPlayedCalicoJack = 0;
		MineShaft.activeMines.RemoveAll(delegate(MineShaft level)
		{
			level.OnRemoved();
			return true;
		});
		MineShaft.permanentMineChanges.Clear();
		MineShaft.numberOfCraftedStairsUsedThisRun = 0;
		MineShaft.mushroomLevelsGeneratedToday.Clear();
		VolcanoDungeon.activeLevels.RemoveAll(delegate(VolcanoDungeon level)
		{
			level.OnRemoved();
			return true;
		});
		ItemRegistry.ResetCache();
		Rumble.stopRumbling();
	}

	/// <summary>Reset all game state when returning to the title screen from a loaded save.</summary>
	/// <remarks>This is specialized code that should normally not be called directly. See also <see cref="M:StardewValley.Game1.ResetGameStateOnTitleScreen" />.</remarks>
	public virtual void CleanupReturningToTitle()
	{
		if (Game1.game1.IsMainInstance)
		{
			foreach (Game1 instance in GameRunner.instance.gameInstances)
			{
				if (instance != this)
				{
					GameRunner.instance.RemoveGameInstance(instance);
				}
			}
		}
		else
		{
			GameRunner.instance.RemoveGameInstance(this);
		}
		Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ExitedToMainMenu);
		this.ResetGameStateOnTitleScreen();
		Game1.serverHost = null;
		Game1.client = null;
		Game1.server = null;
		TitleMenu.subMenu = null;
		Game1.game1.refreshWindowSettings();
		if (Game1.activeClickableMenu is TitleMenu titleMenu)
		{
			titleMenu.applyPreferences();
			Game1.activeClickableMenu.gameWindowSizeChanged(Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.graphics.GraphicsDevice.Viewport.Bounds);
		}
	}

	public bool CanTakeScreenshots()
	{
		return true;
	}

	/// <summary>Get the absolute path to the folder containing screenshots.</summary>
	/// <param name="createIfMissing">Whether to create the folder if it doesn't exist already.</param>
	public string GetScreenshotFolder(bool createIfMissing = true)
	{
		return Program.GetLocalAppDataFolder("Screenshots", createIfMissing);
	}

	public bool CanBrowseScreenshots()
	{
		return Directory.Exists(this.GetScreenshotFolder(createIfMissing: false));
	}

	public bool CanZoomScreenshots()
	{
		return true;
	}

	public void BrowseScreenshots()
	{
		string folderPath = this.GetScreenshotFolder(createIfMissing: false);
		if (Directory.Exists(folderPath))
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = folderPath,
					UseShellExecute = true,
					Verb = "open"
				});
			}
			catch (Exception exception)
			{
				Game1.log.Error("Failed to open screenshot folder.", exception);
			}
		}
	}

	public string takeMapScreenshot(float? in_scale, string screenshot_name, Action onDone)
	{
		if (Game1.currentLocation == null)
		{
			return null;
		}
		float scale = 1f;
		if (in_scale.HasValue)
		{
			scale = in_scale.Value;
		}
		string screenshotName = screenshot_name;
		if (string.IsNullOrWhiteSpace(screenshot_name))
		{
			DateTime now = DateTime.UtcNow;
			screenshotName = SaveGame.FilterFileName(Game1.player.name.Value) + $"_{now.Month}-{now.Day}-{now.Year}_{(int)now.TimeOfDay.TotalMilliseconds}";
		}
		return this.takeMapScreenshot(Game1.currentLocation, scale, screenshotName, onDone);
	}

	private unsafe string takeMapScreenshot(GameLocation screenshotLocation, float scale, string screenshot_name, Action onDone)
	{
		string filename = screenshot_name + ".png";
		Game1.GetScreenshotRegion(screenshotLocation, out var start_x, out var start_y, out var width, out var height);
		SKSurface map_bitmap = null;
		bool failed;
		int scaled_width;
		int scaled_height;
		do
		{
			failed = false;
			scaled_width = (int)((float)width * scale);
			scaled_height = (int)((float)height * scale);
			try
			{
				map_bitmap = SKSurface.Create(scaled_width, scaled_height, SKColorType.Rgb888x, SKAlphaType.Opaque);
			}
			catch (Exception exception)
			{
				Game1.log.Error("Map Screenshot: Error trying to create Bitmap.", exception);
				failed = true;
			}
			if (failed)
			{
				scale -= 0.25f;
			}
			if (scale <= 0f)
			{
				return null;
			}
		}
		while (failed);
		int chunk_size = 2048;
		int scaled_chunk_size = (int)((float)chunk_size * scale);
		xTile.Dimensions.Rectangle old_viewport = Game1.viewport;
		bool old_display_hud = Game1.displayHUD;
		this.takingMapScreenshot = true;
		float old_zoom_level = Game1.options.baseZoomLevel;
		Game1.options.baseZoomLevel = 1f;
		RenderTarget2D cached_lightmap = Game1._lightmap;
		Game1._lightmap = null;
		bool fail = false;
		try
		{
			Game1.allocateLightmap(chunk_size, chunk_size);
			int chunks_wide = (int)Math.Ceiling((float)scaled_width / (float)scaled_chunk_size);
			int chunks_high = (int)Math.Ceiling((float)scaled_height / (float)scaled_chunk_size);
			for (int y_offset = 0; y_offset < chunks_high; y_offset++)
			{
				for (int x_offset = 0; x_offset < chunks_wide; x_offset++)
				{
					int current_width = scaled_chunk_size;
					int current_height = scaled_chunk_size;
					int current_x = x_offset * scaled_chunk_size;
					int current_y = y_offset * scaled_chunk_size;
					if (current_x + scaled_chunk_size > scaled_width)
					{
						current_width += scaled_width - (current_x + scaled_chunk_size);
					}
					if (current_y + scaled_chunk_size > scaled_height)
					{
						current_height += scaled_height - (current_y + scaled_chunk_size);
					}
					if (current_height <= 0 || current_width <= 0)
					{
						continue;
					}
					Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(current_x, current_y, current_width, current_height);
					RenderTarget2D render_target = new RenderTarget2D(Game1.graphics.GraphicsDevice, chunk_size, chunk_size, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
					Game1.viewport = new xTile.Dimensions.Rectangle(x_offset * chunk_size + start_x, y_offset * chunk_size + start_y, chunk_size, chunk_size);
					this._draw(Game1.currentGameTime, render_target);
					RenderTarget2D scaled_render_target = new RenderTarget2D(Game1.graphics.GraphicsDevice, current_width, current_height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
					base.GraphicsDevice.SetRenderTarget(scaled_render_target);
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
					Color color = Color.White;
					Game1.spriteBatch.Draw(render_target, Vector2.Zero, render_target.Bounds, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
					Game1.spriteBatch.End();
					render_target.Dispose();
					base.GraphicsDevice.SetRenderTarget(null);
					Color[] colors = new Color[current_width * current_height];
					scaled_render_target.GetData(colors);
					SKBitmap portion_bitmap = new SKBitmap(rect.Width, rect.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
					byte* ptr = (byte*)portion_bitmap.GetPixels().ToPointer();
					for (int row = 0; row < current_height; row++)
					{
						for (int col = 0; col < current_width; col++)
						{
							*(ptr++) = colors[col + row * current_width].R;
							*(ptr++) = colors[col + row * current_width].G;
							*(ptr++) = colors[col + row * current_width].B;
							*(ptr++) = byte.MaxValue;
						}
					}
					SKPaint paint = new SKPaint();
					map_bitmap.Canvas.DrawBitmap(portion_bitmap, SKRect.Create(rect.X, rect.Y, current_width, current_height), paint);
					portion_bitmap.Dispose();
					scaled_render_target.Dispose();
				}
			}
			string fullFilePath = Path.Combine(this.GetScreenshotFolder(), filename);
			map_bitmap.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(new FileStream(fullFilePath, FileMode.OpenOrCreate));
			map_bitmap.Dispose();
		}
		catch (Exception exception2)
		{
			Game1.log.Error("Map Screenshot: Error taking screenshot.", exception2);
			base.GraphicsDevice.SetRenderTarget(null);
			fail = true;
		}
		if (Game1._lightmap != null)
		{
			Game1._lightmap.Dispose();
			Game1._lightmap = null;
		}
		Game1._lightmap = cached_lightmap;
		Game1.options.baseZoomLevel = old_zoom_level;
		this.takingMapScreenshot = false;
		Game1.displayHUD = old_display_hud;
		Game1.viewport = old_viewport;
		if (fail)
		{
			return null;
		}
		return filename;
	}

	private static void GetScreenshotRegion(GameLocation screenshotLocation, out int startX, out int startY, out int width, out int height)
	{
		startX = 0;
		startY = 0;
		width = screenshotLocation.map.DisplayWidth;
		height = screenshotLocation.map.DisplayHeight;
		try
		{
			string[] fields = screenshotLocation.GetMapPropertySplitBySpaces("ScreenshotRegion");
			if (fields.Length != 0)
			{
				if (!ArgUtility.TryGetInt(fields, 0, out var topLeftX, out var error, "int topLeftX") || !ArgUtility.TryGetInt(fields, 1, out var topLeftY, out error, "int topLeftY") || !ArgUtility.TryGetInt(fields, 2, out var bottomRightX, out error, "int bottomRightX") || !ArgUtility.TryGetInt(fields, 3, out var bottomRightY, out error, "int bottomRightY"))
				{
					screenshotLocation.LogMapPropertyError("ScreenshotRegion", fields, error);
					return;
				}
				startX = topLeftX * 64;
				startY = topLeftY * 64;
				width = (bottomRightX + 1) * 64 - startX;
				height = (bottomRightY + 1) * 64 - startY;
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error("GetScreenshotRegion failed with exception:", exception);
		}
	}
}
