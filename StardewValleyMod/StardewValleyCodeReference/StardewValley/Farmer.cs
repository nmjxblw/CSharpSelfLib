using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Companions;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.LocationContexts;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.Tiles;

namespace StardewValley;

public class Farmer : Character, IComparable
{
	public class EmoteType
	{
		public string emoteString = "";

		public int emoteIconIndex = -1;

		public FarmerSprite.AnimationFrame[] animationFrames;

		public bool hidden;

		public int facingDirection = 2;

		public string displayNameKey;

		public string displayName => Game1.content.LoadString(this.displayNameKey);

		public EmoteType(string emote_string = "", string display_name_key = "", int icon_index = -1, FarmerSprite.AnimationFrame[] frames = null, int facing_direction = 2, bool is_hidden = false)
		{
			this.emoteString = emote_string;
			this.emoteIconIndex = icon_index;
			this.animationFrames = frames;
			this.facingDirection = facing_direction;
			this.hidden = is_hidden;
			this.displayNameKey = "Strings\\UI:" + display_name_key;
		}
	}

	public const int millisecondsPerSpeedUnit = 64;

	public const byte halt = 64;

	public const byte up = 1;

	public const byte right = 2;

	public const byte down = 4;

	public const byte left = 8;

	public const byte run = 16;

	public const byte release = 32;

	public const int farmingSkill = 0;

	public const int miningSkill = 3;

	public const int fishingSkill = 1;

	public const int foragingSkill = 2;

	public const int combatSkill = 4;

	public const int luckSkill = 5;

	public const float interpolationConstant = 0.5f;

	public const int runningSpeed = 5;

	public const int walkingSpeed = 2;

	public const int caveNothing = 0;

	public const int caveBats = 1;

	public const int caveMushrooms = 2;

	public const int millisecondsInvincibleAfterDamage = 1200;

	public const int millisecondsPerFlickerWhenInvincible = 50;

	public const int startingStamina = 270;

	public const int totalLevels = 35;

	public const int maxInventorySpace = 36;

	public const int hotbarSize = 12;

	public const int eyesOpen = 0;

	public const int eyesHalfShut = 4;

	public const int eyesClosed = 1;

	public const int eyesRight = 2;

	public const int eyesLeft = 3;

	public const int eyesWide = 5;

	public const int rancher = 0;

	public const int tiller = 1;

	public const int butcher = 2;

	public const int shepherd = 3;

	public const int artisan = 4;

	public const int agriculturist = 5;

	public const int fisher = 6;

	public const int trapper = 7;

	public const int angler = 8;

	public const int pirate = 9;

	/// <summary>The Mariner profession, which makes crab pots no longer produce junk items.</summary>
	/// <remarks>For legacy reasons, the Luremaster and Mariner profession constants are swapped (i.e. <see cref="F:StardewValley.Farmer.mariner" /> is Luremaster and <see cref="F:StardewValley.Farmer.baitmaster" /> is Mariner).</remarks>
	public const int baitmaster = 10;

	/// <summary>The Luremaster profession, which makes crab pots no longer require bait.</summary>
	/// <inheritdoc cref="F:StardewValley.Farmer.baitmaster" path="/remarks" />
	public const int mariner = 11;

	public const int forester = 12;

	public const int gatherer = 13;

	public const int lumberjack = 14;

	public const int tapper = 15;

	public const int botanist = 16;

	public const int tracker = 17;

	public const int miner = 18;

	public const int geologist = 19;

	public const int blacksmith = 20;

	public const int burrower = 21;

	public const int excavator = 22;

	public const int gemologist = 23;

	public const int fighter = 24;

	public const int scout = 25;

	public const int brute = 26;

	public const int defender = 27;

	public const int acrobat = 28;

	public const int desperado = 29;

	public static int MaximumTrinkets = 1;

	public readonly NetObjectList<Quest> questLog = new NetObjectList<Quest>();

	public readonly NetIntHashSet professions = new NetIntHashSet();

	public readonly NetList<Point, NetPoint> newLevels = new NetList<Point, NetPoint>();

	[XmlIgnore]
	public Queue<int> newLevelSparklingTexts = new Queue<int>();

	[XmlIgnore]
	public SparklingText sparklingText;

	public readonly NetArray<int, NetInt> experiencePoints = new NetArray<int, NetInt>(6);

	/// <summary>The backing field for <see cref="P:StardewValley.Farmer.Items" />.</summary>
	[XmlElement("items")]
	public readonly NetRef<Inventory> netItems = new NetRef<Inventory>(new Inventory());

	[XmlArrayItem("int")]
	public readonly NetStringHashSet dialogueQuestionsAnswered = new NetStringHashSet();

	[XmlElement("cookingRecipes")]
	public readonly NetStringDictionary<int, NetInt> cookingRecipes = new NetStringDictionary<int, NetInt>();

	[XmlElement("craftingRecipes")]
	public readonly NetStringDictionary<int, NetInt> craftingRecipes = new NetStringDictionary<int, NetInt>();

	[XmlElement("activeDialogueEvents")]
	public readonly NetStringDictionary<int, NetInt> activeDialogueEvents = new NetStringDictionary<int, NetInt>();

	[XmlElement("previousActiveDialogueEvents")]
	public readonly NetStringDictionary<int, NetInt> previousActiveDialogueEvents = new NetStringDictionary<int, NetInt>();

	/// <summary>The trigger actions which have been run for the player.</summary>
	public readonly NetStringHashSet triggerActionsRun = new NetStringHashSet();

	/// <summary>The event IDs which the player has seen.</summary>
	[XmlArrayItem("int")]
	public readonly NetStringHashSet eventsSeen = new NetStringHashSet();

	public readonly NetIntHashSet secretNotesSeen = new NetIntHashSet();

	public HashSet<string> songsHeard = new HashSet<string>();

	public readonly NetIntHashSet achievements = new NetIntHashSet();

	[XmlArrayItem("int")]
	public readonly NetStringList specialItems = new NetStringList();

	[XmlArrayItem("int")]
	public readonly NetStringList specialBigCraftables = new NetStringList();

	/// <summary>The mail flags set on the player. This includes both actual mail letter IDs matching <c>Data/mail</c>, and non-mail flags used to track game state like <c>ccIsComplete</c> (community center complete).</summary>
	/// <remarks>See also <see cref="F:StardewValley.Farmer.mailForTomorrow" /> and <see cref="F:StardewValley.Farmer.mailbox" />.</remarks>
	public readonly NetStringHashSet mailReceived = new NetStringHashSet();

	/// <summary>The mail flags that will be added to the <see cref="F:StardewValley.Farmer.mailbox" /> tomorrow.</summary>
	public readonly NetStringHashSet mailForTomorrow = new NetStringHashSet();

	/// <summary>The mail IDs matching <c>Data/mail</c> in the player's mailbox, if any. Each time the player checks their mailbox, one letter from this set will be displayed and moved into <see cref="F:StardewValley.Farmer.mailReceived" />.</summary>
	public readonly NetStringList mailbox = new NetStringList();

	/// <summary>The internal names of locations which the player has previously visited.</summary>
	/// <remarks>This contains the <see cref="P:StardewValley.GameLocation.Name" /> field, not <see cref="P:StardewValley.GameLocation.NameOrUniqueName" />. They're equivalent for most locations, but building interiors will use their common name (like <c>Barn</c> instead of <c>Barn{unique ID}</c> for barns).</remarks>
	public readonly NetStringHashSet locationsVisited = new NetStringHashSet();

	public readonly NetInt timeWentToBed = new NetInt();

	[XmlIgnore]
	public readonly NetList<Companion, NetRef<Companion>> companions = new NetList<Companion, NetRef<Companion>>();

	/// <summary>Whether the local player has moved today.</summary>
	[XmlIgnore]
	public bool hasMoved;

	/// <summary>Whether the local player has interacted with a statue of blessings today.</summary>
	[XmlIgnore]
	public bool hasBeenBlessedByStatueToday;

	/// <summary>Whether the player slept using an item bed in a place that doesn't have an actual bed furniture item.</summary>
	public readonly NetBool sleptInTemporaryBed = new NetBool();

	[XmlIgnore]
	public readonly NetBool requestingTimePause = new NetBool
	{
		InterpolationWait = false
	};

	public Stats stats = new Stats();

	[XmlIgnore]
	public readonly NetRef<Inventory> personalShippingBin = new NetRef<Inventory>(new Inventory());

	[XmlIgnore]
	public IList<Item> displayedShippedItems = new List<Item>();

	[XmlElement("biteChime")]
	public NetInt biteChime = new NetInt(-1);

	[XmlIgnore]
	public float usernameDisplayTime;

	[XmlIgnore]
	protected NetRef<Item> _recoveredItem = new NetRef<Item>();

	public NetObjectList<Item> itemsLostLastDeath = new NetObjectList<Item>();

	public List<int> movementDirections = new List<int>();

	[XmlElement("farmName")]
	public readonly NetString farmName = new NetString("");

	[XmlElement("favoriteThing")]
	public readonly NetString favoriteThing = new NetString();

	[XmlElement("horseName")]
	public readonly NetString horseName = new NetString();

	public string slotName;

	public bool slotCanHost;

	[XmlIgnore]
	public readonly NetString tempFoodItemTextureName = new NetString();

	[XmlIgnore]
	public readonly NetRectangle tempFoodItemSourceRect = new NetRectangle();

	[XmlIgnore]
	public bool hasReceivedToolUpgradeMessageYet;

	[XmlIgnore]
	public readonly BuffManager buffs = new BuffManager();

	[XmlIgnore]
	public IList<OutgoingMessage> messageQueue = new List<OutgoingMessage>();

	[XmlIgnore]
	public readonly NetLong uniqueMultiplayerID = new NetLong(Utility.RandomLong());

	[XmlElement("userID")]
	public readonly NetString userID = new NetString("");

	[XmlIgnore]
	public string previousLocationName = "";

	[XmlIgnore]
	public readonly NetString platformType = new NetString("");

	[XmlIgnore]
	public readonly NetString platformID = new NetString("");

	[XmlIgnore]
	public readonly NetBool hasMenuOpen = new NetBool(value: false);

	[XmlIgnore]
	public readonly Color DEFAULT_SHIRT_COLOR = Color.White;

	public string defaultChatColor;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.whichPetType" /> instead.</summary>
	[XmlElement("catPerson")]
	public bool? obsolete_catPerson;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.canUnderstandDwarves" /> instead.</summary>
	[XmlElement("canUnderstandDwarves")]
	public bool? obsolete_canUnderstandDwarves;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasClubCard" /> instead.</summary>
	[XmlElement("hasClubCard")]
	public bool? obsolete_hasClubCard;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasDarkTalisman" /> instead.</summary>
	[XmlElement("hasDarkTalisman")]
	public bool? obsolete_hasDarkTalisman;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasMagicInk" /> instead.</summary>
	[XmlElement("hasMagicInk")]
	public bool? obsolete_hasMagicInk;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasMagnifyingGlass" /> instead.</summary>
	[XmlElement("hasMagnifyingGlass")]
	public bool? obsolete_hasMagnifyingGlass;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasRustyKey" /> instead.</summary>
	[XmlElement("hasRustyKey")]
	public bool? obsolete_hasRustyKey;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasSkullKey" /> instead.</summary>
	[XmlElement("hasSkullKey")]
	public bool? obsolete_hasSkullKey;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasSpecialCharm" /> instead.</summary>
	[XmlElement("hasSpecialCharm")]
	public bool? obsolete_hasSpecialCharm;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.HasTownKey" /> instead.</summary>
	[XmlElement("HasTownKey")]
	public bool? obsolete_hasTownKey;

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.hasUnlockedSkullDoor" /> instead.</summary>
	[XmlElement("hasUnlockedSkullDoor")]
	public bool? obsolete_hasUnlockedSkullDoor;

	/// <summary>Obsolete since 1.3. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.friendshipData" /> for NPC friendships or <see cref="F:StardewValley.FarmerTeam.friendshipData" /> for farmhands instead.</summary>
	[XmlElement("friendships")]
	public SerializableDictionary<string, int[]> obsolete_friendships;

	/// <summary>Obsolete since 1.3. This is only kept to preserve data from old save files. Use <see cref="M:StardewValley.Farmer.GetDaysMarried" /> instead.</summary>
	[XmlElement("daysMarried")]
	public int? obsolete_daysMarried;

	/// <summary>The preferred pet type, matching an ID in <c>Data/Pets</c>. The vanilla pet types are <see cref="F:StardewValley.Characters.Pet.type_cat" /> and <see cref="F:StardewValley.Characters.Pet.type_dog" />.</summary>
	public string whichPetType = "Cat";

	/// <summary>The selected breed ID in <c>Data/Pets</c> for the <see cref="F:StardewValley.Farmer.whichPetType" />.</summary>
	public string whichPetBreed = "0";

	[XmlIgnore]
	public bool isAnimatingMount;

	[XmlElement("acceptedDailyQuest")]
	public readonly NetBool acceptedDailyQuest = new NetBool(value: false);

	[XmlIgnore]
	public Item mostRecentlyGrabbedItem;

	[XmlIgnore]
	public Item itemToEat;

	[XmlElement("farmerRenderer")]
	private readonly NetRef<FarmerRenderer> farmerRenderer = new NetRef<FarmerRenderer>();

	[XmlIgnore]
	public readonly NetInt toolPower = new NetInt();

	[XmlIgnore]
	public readonly NetInt toolHold = new NetInt();

	public Vector2 mostRecentBed;

	public static Dictionary<int, string> hairStyleMetadataFile = null;

	public static List<int> allHairStyleIndices = null;

	[XmlIgnore]
	public static Dictionary<int, HairStyleMetadata> hairStyleMetadata = new Dictionary<int, HairStyleMetadata>();

	[XmlElement("emoteFavorites")]
	public readonly List<string> emoteFavorites = new List<string>();

	[XmlElement("performedEmotes")]
	public readonly SerializableDictionary<string, bool> performedEmotes = new SerializableDictionary<string, bool>();

	/// <summary>If set, the unqualified item ID of the <see cref="F:StardewValley.ItemRegistry.type_shirt" /> item to show this player wearing instead of the equipped <see cref="F:StardewValley.Farmer.shirtItem" />.</summary>
	[XmlElement("shirt")]
	public readonly NetString shirt = new NetString("1000");

	[XmlElement("hair")]
	public readonly NetInt hair = new NetInt(0);

	[XmlElement("skin")]
	public readonly NetInt skin = new NetInt(0);

	[XmlElement("shoes")]
	public readonly NetString shoes = new NetString("2");

	[XmlElement("accessory")]
	public readonly NetInt accessory = new NetInt(-1);

	[XmlElement("facialHair")]
	public readonly NetInt facialHair = new NetInt(-1);

	/// <summary>If set, the unqualified item ID of the <see cref="F:StardewValley.ItemRegistry.type_pants" /> item to show this player wearing instead of the equipped <see cref="F:StardewValley.Farmer.pantsItem" />.</summary>
	[XmlElement("pants")]
	public readonly NetString pants = new NetString("0");

	[XmlIgnore]
	public int currentEyes;

	[XmlIgnore]
	public int blinkTimer;

	[XmlIgnore]
	public readonly NetInt netFestivalScore = new NetInt();

	/// <summary>The last date that we submitted the Calico Egg Rating to Gil.</summary>
	public readonly NetRef<WorldDate> lastGotPrizeFromGil = new NetRef<WorldDate>();

	/// <summary>The last date that we accepted a Desert Festival fishing quest.</summary>
	public readonly NetRef<WorldDate> lastDesertFestivalFishingQuest = new NetRef<WorldDate>();

	[XmlIgnore]
	public float temporarySpeedBuff;

	[XmlElement("hairstyleColor")]
	public readonly NetColor hairstyleColor = new NetColor(new Color(193, 90, 50));

	[XmlIgnore]
	public NetBool prismaticHair = new NetBool();

	/// <summary>The color to apply when rendering <see cref="F:StardewValley.Farmer.pants" />. Most code should use <see cref="M:StardewValley.Farmer.GetPantsColor" /> instead.</summary>
	[XmlElement("pantsColor")]
	public readonly NetColor pantsColor = new NetColor(new Color(46, 85, 183));

	[XmlElement("newEyeColor")]
	public readonly NetColor newEyeColor = new NetColor(new Color(122, 68, 52));

	[XmlElement("hat")]
	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	[XmlElement("boots")]
	public readonly NetRef<Boots> boots = new NetRef<Boots>();

	[XmlElement("leftRing")]
	public readonly NetRef<Ring> leftRing = new NetRef<Ring>();

	[XmlElement("rightRing")]
	public readonly NetRef<Ring> rightRing = new NetRef<Ring>();

	[XmlElement("shirtItem")]
	public readonly NetRef<Clothing> shirtItem = new NetRef<Clothing>();

	[XmlElement("pantsItem")]
	public readonly NetRef<Clothing> pantsItem = new NetRef<Clothing>();

	[XmlIgnore]
	public readonly NetDancePartner dancePartner = new NetDancePartner();

	[XmlIgnore]
	public bool ridingMineElevator;

	[XmlIgnore]
	public readonly NetBool exhausted = new NetBool();

	[XmlElement("divorceTonight")]
	public readonly NetBool divorceTonight = new NetBool();

	[XmlElement("changeWalletTypeTonight")]
	public readonly NetBool changeWalletTypeTonight = new NetBool();

	[XmlIgnore]
	public AnimatedSprite.endOfAnimationBehavior toolOverrideFunction;

	[XmlIgnore]
	public NetBool onBridge = new NetBool();

	[XmlIgnore]
	public SuspensionBridge bridge;

	private readonly NetInt netDeepestMineLevel = new NetInt();

	[XmlElement("currentToolIndex")]
	private readonly NetInt currentToolIndex = new NetInt(0);

	[XmlIgnore]
	private readonly NetRef<Item> temporaryItem = new NetRef<Item>();

	[XmlIgnore]
	private readonly NetRef<Item> cursorSlotItem = new NetRef<Item>();

	[XmlIgnore]
	public readonly NetBool netItemStowed = new NetBool(value: false);

	protected bool _itemStowed;

	public string gameVersion = "-1";

	public string gameVersionLabel;

	[XmlIgnore]
	public bool isFakeEventActor;

	[XmlElement("bibberstyke")]
	public readonly NetInt bobberStyle = new NetInt(0);

	public bool usingRandomizedBobber;

	[XmlElement("caveChoice")]
	public readonly NetInt caveChoice = new NetInt();

	[XmlElement("farmingLevel")]
	public readonly NetInt farmingLevel = new NetInt();

	[XmlElement("miningLevel")]
	public readonly NetInt miningLevel = new NetInt();

	[XmlElement("combatLevel")]
	public readonly NetInt combatLevel = new NetInt();

	[XmlElement("foragingLevel")]
	public readonly NetInt foragingLevel = new NetInt();

	[XmlElement("fishingLevel")]
	public readonly NetInt fishingLevel = new NetInt();

	[XmlElement("luckLevel")]
	public readonly NetInt luckLevel = new NetInt();

	[XmlElement("maxStamina")]
	public readonly NetInt maxStamina = new NetInt(270);

	[XmlElement("maxItems")]
	public readonly NetInt maxItems = new NetInt(12);

	[XmlElement("lastSeenMovieWeek")]
	public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

	[XmlIgnore]
	public readonly NetString viewingLocation = new NetString();

	private readonly NetFloat netStamina = new NetFloat(270f);

	[XmlIgnore]
	public bool ignoreItemConsumptionThisFrame;

	[XmlIgnore]
	[NotNetField]
	public NetRoot<FarmerTeam> teamRoot = new NetRoot<FarmerTeam>(new FarmerTeam());

	public int clubCoins;

	public int trashCanLevel;

	private NetLong netMillisecondsPlayed = new NetLong
	{
		DeltaAggregateTicks = (ushort)(60 * (Game1.realMilliSecondsPerGameTenMinutes / 1000))
	};

	[XmlElement("toolBeingUpgraded")]
	public readonly NetRef<Tool> toolBeingUpgraded = new NetRef<Tool>();

	[XmlElement("daysLeftForToolUpgrade")]
	public readonly NetInt daysLeftForToolUpgrade = new NetInt();

	[XmlElement("houseUpgradeLevel")]
	public readonly NetInt houseUpgradeLevel = new NetInt(0);

	[XmlElement("daysUntilHouseUpgrade")]
	public readonly NetInt daysUntilHouseUpgrade = new NetInt(-1);

	public bool showChestColorPicker = true;

	public bool hasWateringCanEnchantment;

	[XmlIgnore]
	public List<BaseEnchantment> enchantments = new List<BaseEnchantment>();

	public readonly int BaseMagneticRadius = 128;

	public int temporaryInvincibilityTimer;

	public int currentTemporaryInvincibilityDuration = 1200;

	[XmlIgnore]
	public float rotation;

	private int craftingTime = 1000;

	private int raftPuddleCounter = 250;

	private int raftBobCounter = 1000;

	public int health = 100;

	public int maxHealth = 100;

	private readonly NetInt netTimesReachedMineBottom = new NetInt(0);

	public float difficultyModifier = 1f;

	[XmlIgnore]
	public Vector2 jitter = Vector2.Zero;

	[XmlIgnore]
	public Vector2 lastPosition;

	[XmlIgnore]
	public Vector2 lastGrabTile = Vector2.Zero;

	[XmlIgnore]
	public float jitterStrength;

	[XmlIgnore]
	public float xOffset;

	/// <summary>The net-synchronized backing field for <see cref="P:StardewValley.Farmer.Gender" />.</summary>
	[XmlElement("gender")]
	public readonly NetEnum<Gender> netGender = new NetEnum<Gender>();

	[XmlIgnore]
	public bool canMove = true;

	[XmlIgnore]
	public bool running;

	[XmlIgnore]
	public bool ignoreCollisions;

	[XmlIgnore]
	public readonly NetBool usingTool = new NetBool(value: false);

	[XmlIgnore]
	public bool isEating;

	[XmlIgnore]
	public readonly NetBool isInBed = new NetBool(value: false);

	[XmlIgnore]
	public bool forceTimePass;

	[XmlIgnore]
	public bool isRafting;

	[XmlIgnore]
	public bool usingSlingshot;

	[XmlIgnore]
	public readonly NetBool bathingClothes = new NetBool(value: false);

	[XmlIgnore]
	public bool canOnlyWalk;

	[XmlIgnore]
	public bool temporarilyInvincible;

	[XmlIgnore]
	public bool flashDuringThisTemporaryInvincibility = true;

	private readonly NetBool netCanReleaseTool = new NetBool(value: false);

	[XmlIgnore]
	public bool isCrafting;

	[XmlIgnore]
	public bool isEmoteAnimating;

	[XmlIgnore]
	public bool passedOut;

	[XmlIgnore]
	protected int _emoteGracePeriod;

	[XmlIgnore]
	private BoundingBoxGroup temporaryPassableTiles = new BoundingBoxGroup();

	[XmlIgnore]
	public readonly NetBool hidden = new NetBool();

	[XmlElement("basicShipped")]
	public readonly NetStringDictionary<int, NetInt> basicShipped = new NetStringDictionary<int, NetInt>();

	[XmlElement("mineralsFound")]
	public readonly NetStringDictionary<int, NetInt> mineralsFound = new NetStringDictionary<int, NetInt>();

	[XmlElement("recipesCooked")]
	public readonly NetStringDictionary<int, NetInt> recipesCooked = new NetStringDictionary<int, NetInt>();

	[XmlElement("fishCaught")]
	public readonly NetStringIntArrayDictionary fishCaught = new NetStringIntArrayDictionary();

	[XmlElement("archaeologyFound")]
	public readonly NetStringIntArrayDictionary archaeologyFound = new NetStringIntArrayDictionary();

	[XmlElement("callsReceived")]
	public readonly NetStringDictionary<int, NetInt> callsReceived = new NetStringDictionary<int, NetInt>();

	public SerializableDictionary<string, SerializableDictionary<string, int>> giftedItems;

	[XmlElement("tailoredItems")]
	public readonly NetStringDictionary<int, NetInt> tailoredItems = new NetStringDictionary<int, NetInt>();

	[XmlElement("friendshipData")]
	public readonly NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetStringDictionary<Friendship, NetRef<Friendship>>();

	[XmlIgnore]
	public NetString locationBeforeForcedEvent = new NetString(null);

	[XmlIgnore]
	public Vector2 positionBeforeEvent;

	[XmlIgnore]
	public int orientationBeforeEvent;

	[XmlIgnore]
	public int swimTimer;

	[XmlIgnore]
	public int regenTimer;

	[XmlIgnore]
	public int timerSinceLastMovement;

	[XmlIgnore]
	public int noMovementPause;

	[XmlIgnore]
	public int freezePause;

	[XmlIgnore]
	public float yOffset;

	/// <summary>The backing field for <see cref="P:StardewValley.Farmer.spouse" />.</summary>
	protected readonly NetString netSpouse = new NetString();

	public string dateStringForSaveGame;

	public int? dayOfMonthForSaveGame;

	public int? seasonForSaveGame;

	public int? yearForSaveGame;

	[XmlIgnore]
	public Vector2 armOffset;

	[XmlIgnore]
	public readonly NetRef<Horse> netMount = new NetRef<Horse>();

	[XmlIgnore]
	public ISittable sittingFurniture;

	[XmlIgnore]
	public NetBool isSitting = new NetBool();

	[XmlIgnore]
	public NetVector2 mapChairSitPosition = new NetVector2(new Vector2(-1f, -1f));

	[XmlIgnore]
	public NetBool hasCompletedAllMonsterSlayerQuests = new NetBool(value: false);

	[XmlIgnore]
	public bool isStopSitting;

	[XmlIgnore]
	protected bool _wasSitting;

	[XmlIgnore]
	public Vector2 lerpStartPosition;

	[XmlIgnore]
	public Vector2 lerpEndPosition;

	[XmlIgnore]
	public float lerpPosition = -1f;

	[XmlIgnore]
	public float lerpDuration = -1f;

	[XmlIgnore]
	protected Item _lastSelectedItem;

	[XmlIgnore]
	protected internal Tool _lastEquippedTool;

	[XmlElement("qiGems")]
	public NetIntDelta netQiGems = new NetIntDelta
	{
		Minimum = 0
	};

	[XmlElement("JOTPKProgress")]
	public NetRef<AbigailGame.JOTPKProgress> jotpkProgress = new NetRef<AbigailGame.JOTPKProgress>();

	[XmlIgnore]
	public NetBool hasUsedDailyRevive = new NetBool(value: false);

	[XmlElement("trinketItem")]
	public readonly NetList<Trinket, NetRef<Trinket>> trinketItems = new NetList<Trinket, NetRef<Trinket>>();

	private readonly NetEvent0 fireToolEvent = new NetEvent0(interpolate: true);

	private readonly NetEvent0 beginUsingToolEvent = new NetEvent0(interpolate: true);

	private readonly NetEvent0 endUsingToolEvent = new NetEvent0(interpolate: true);

	private readonly NetEvent0 sickAnimationEvent = new NetEvent0();

	private readonly NetEvent0 passOutEvent = new NetEvent0();

	private readonly NetEvent0 haltAnimationEvent = new NetEvent0();

	private readonly NetEvent1Field<Object, NetRef<Object>> drinkAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

	private readonly NetEvent1Field<Object, NetRef<Object>> eatAnimationEvent = new NetEvent1Field<Object, NetRef<Object>>();

	private readonly NetEvent1Field<string, NetString> doEmoteEvent = new NetEvent1Field<string, NetString>();

	private readonly NetEvent1Field<long, NetLong> kissFarmerEvent = new NetEvent1Field<long, NetLong>();

	private readonly NetEvent1Field<float, NetFloat> synchronizedJumpEvent = new NetEvent1Field<float, NetFloat>();

	public readonly NetEvent1Field<string, NetString> renovateEvent = new NetEvent1Field<string, NetString>();

	[XmlElement("chestConsumedLevels")]
	public readonly NetIntDictionary<bool, NetBool> chestConsumedMineLevels = new NetIntDictionary<bool, NetBool>();

	public int saveTime;

	[XmlIgnore]
	public float drawLayerDisambiguator;

	[XmlElement("isCustomized")]
	public readonly NetBool isCustomized = new NetBool(value: false);

	[XmlElement("homeLocation")]
	public readonly NetString homeLocation = new NetString("FarmHouse");

	[XmlElement("lastSleepLocation")]
	public readonly NetString lastSleepLocation = new NetString();

	[XmlElement("lastSleepPoint")]
	public readonly NetPoint lastSleepPoint = new NetPoint();

	[XmlElement("disconnectDay")]
	public readonly NetInt disconnectDay = new NetInt(-1);

	[XmlElement("disconnectLocation")]
	public readonly NetString disconnectLocation = new NetString();

	[XmlElement("disconnectPosition")]
	public readonly NetVector2 disconnectPosition = new NetVector2();

	public static readonly EmoteType[] EMOTES = new EmoteType[22]
	{
		new EmoteType("happy", "Emote_Happy", 32),
		new EmoteType("sad", "Emote_Sad", 28),
		new EmoteType("heart", "Emote_Heart", 20),
		new EmoteType("exclamation", "Emote_Exclamation", 16),
		new EmoteType("note", "Emote_Note", 56),
		new EmoteType("sleep", "Emote_Sleep", 24),
		new EmoteType("game", "Emote_Game", 52),
		new EmoteType("question", "Emote_Question", 8),
		new EmoteType("x", "Emote_X", 36),
		new EmoteType("pause", "Emote_Pause", 40),
		new EmoteType("blush", "Emote_Blush", 60, null, 2, is_hidden: true),
		new EmoteType("angry", "Emote_Angry", 12),
		new EmoteType("yes", "Emote_Yes", 56, new FarmerSprite.AnimationFrame[7]
		{
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("jingle1");
				}
			}),
			new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(16, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(0, 250, secondaryArm: false, flip: false)
		}),
		new EmoteType("no", "Emote_No", 36, new FarmerSprite.AnimationFrame[5]
		{
			new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("cancel");
				}
			}),
			new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
			new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(27, 250, secondaryArm: true, flip: false),
			new FarmerSprite.AnimationFrame(25, 250, secondaryArm: false, flip: false)
		}),
		new EmoteType("sick", "Emote_Sick", 12, new FarmerSprite.AnimationFrame[8]
		{
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("croak");
				}
			}),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false)
		}),
		new EmoteType("laugh", "Emote_Laugh", 56, new FarmerSprite.AnimationFrame[8]
		{
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("dustMeep");
				}
			}),
			new FarmerSprite.AnimationFrame(103, 150, secondaryArm: false, flip: false)
		}),
		new EmoteType("surprised", "Emote_Surprised", 16, new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(94, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
		{
			if (who.ShouldHandleAnimationSound())
			{
				who.playNearbySoundLocal("batScreech");
			}
			who.jumpWithoutSound(4f);
			who.jitterStrength = 1f;
		}) }),
		new EmoteType("hi", "Emote_Hi", 56, new FarmerSprite.AnimationFrame[4]
		{
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("give_gift");
				}
			}),
			new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(85, 250, secondaryArm: false, flip: false)
		}),
		new EmoteType("taunt", "Emote_Taunt", 12, new FarmerSprite.AnimationFrame[10]
		{
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("hitEnemy");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("hitEnemy");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(102, 50, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(10, 250, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("hitEnemy");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(3, 500, secondaryArm: false, flip: false)
		}, 2, is_hidden: true),
		new EmoteType("uh", "Emote_Uh", 40, new FarmerSprite.AnimationFrame[1] { new FarmerSprite.AnimationFrame(10, 1500, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
		{
			if (who.ShouldHandleAnimationSound())
			{
				who.playNearbySoundLocal("clam_tone");
			}
		}) }),
		new EmoteType("music", "Emote_Music", 56, new FarmerSprite.AnimationFrame[9]
		{
			new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				who.playHarpEmoteSound();
			}),
			new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(98, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(99, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(100, 150, secondaryArm: false, flip: false)
		}, 2, is_hidden: true),
		new EmoteType("jar", "Emote_Jar", -1, new FarmerSprite.AnimationFrame[6]
		{
			new FarmerSprite.AnimationFrame(111, 150, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("fishingRodBend");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(111, 300, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("fishingRodBend");
				}
				who.jitterStrength = 1f;
			}).AddFrameEndAction(delegate(Farmer who)
			{
				who.stopJittering();
			}),
			new FarmerSprite.AnimationFrame(111, 500, secondaryArm: false, flip: false),
			new FarmerSprite.AnimationFrame(112, 1000, secondaryArm: false, flip: false).AddFrameAction(delegate(Farmer who)
			{
				if (who.ShouldHandleAnimationSound())
				{
					who.playNearbySoundLocal("coin");
				}
				who.jumpWithoutSound(4f);
			})
		}, 1, is_hidden: true)
	};

	[XmlIgnore]
	public int emoteFacingDirection = 2;

	private int toolPitchAccumulator;

	[XmlIgnore]
	public readonly NetInt toolHoldStartTime = new NetInt();

	private int charactercollisionTimer;

	private NPC collisionNPC;

	public float movementMultiplier = 0.01f;

	public bool hasVisibleQuests
	{
		get
		{
			foreach (SpecialOrder specialOrder in this.team.specialOrders)
			{
				if (!specialOrder.IsHidden())
				{
					return true;
				}
			}
			foreach (Quest quest in this.questLog)
			{
				if (quest != null && !quest.IsHidden())
				{
					return true;
				}
			}
			return false;
		}
	}

	public Item recoveredItem
	{
		get
		{
			return this._recoveredItem.Value;
		}
		set
		{
			this._recoveredItem.Value = value;
		}
	}

	/// <summary>Obsolete since 1.6. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Farmer.Gender" /> or <see cref="P:StardewValley.Farmer.IsMale" /> instead.</summary>
	[XmlElement("isMale")]
	public bool? obsolete_isMale
	{
		get
		{
			return null;
		}
		set
		{
			if (value.HasValue)
			{
				this.Gender = ((!value.Value) ? Gender.Female : Gender.Male);
			}
		}
	}

	/// <summary>Whether the player's preferred pet type is <see cref="F:StardewValley.Characters.Pet.type_cat" />.</summary>
	/// <remarks>See also <see cref="F:StardewValley.Farmer.whichPetType" />.</remarks>
	[XmlIgnore]
	public bool catPerson => this.whichPetType == "Cat";

	[XmlIgnore]
	public int festivalScore
	{
		get
		{
			return this.netFestivalScore.Value;
		}
		set
		{
			if (this.team?.festivalScoreStatus != null)
			{
				this.team.festivalScoreStatus.UpdateState(this.festivalScore.ToString() ?? "");
			}
			this.netFestivalScore.Value = value;
		}
	}

	public int deepestMineLevel
	{
		get
		{
			return this.netDeepestMineLevel.Value;
		}
		set
		{
			this.netDeepestMineLevel.Value = value;
		}
	}

	public float stamina
	{
		get
		{
			return this.netStamina.Value;
		}
		set
		{
			this.netStamina.Value = value;
		}
	}

	[XmlIgnore]
	public FarmerTeam team
	{
		get
		{
			if (Game1.player != null && this != Game1.player)
			{
				return Game1.player.team;
			}
			return this.teamRoot.Value;
		}
	}

	public uint totalMoneyEarned
	{
		get
		{
			return (uint)this.teamRoot.Value.totalMoneyEarned.Value;
		}
		set
		{
			if (this.teamRoot.Value.totalMoneyEarned.Value != 0)
			{
				if (value >= 15000 && this.teamRoot.Value.totalMoneyEarned.Value < 15000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned15k", this.farmName.Value);
				}
				if (value >= 50000 && this.teamRoot.Value.totalMoneyEarned.Value < 50000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned50k", this.farmName.Value);
				}
				if (value >= 250000 && this.teamRoot.Value.totalMoneyEarned.Value < 250000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned250k", this.farmName.Value);
				}
				if (value >= 1000000 && this.teamRoot.Value.totalMoneyEarned.Value < 1000000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned1m", this.farmName.Value);
				}
				if (value >= 10000000 && this.teamRoot.Value.totalMoneyEarned.Value < 10000000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned10m", this.farmName.Value);
				}
				if (value >= 100000000 && this.teamRoot.Value.totalMoneyEarned.Value < 100000000)
				{
					Game1.multiplayer.globalChatInfoMessage("Earned100m", this.farmName.Value);
				}
			}
			this.teamRoot.Value.totalMoneyEarned.Value = (int)value;
		}
	}

	public ulong millisecondsPlayed
	{
		get
		{
			return (ulong)this.netMillisecondsPlayed.Value;
		}
		set
		{
			this.netMillisecondsPlayed.Value = (long)value;
		}
	}

	/// <summary>Whether <strong>any player</strong> has found the Dwarvish Translation Guide that allows speaking to dwarves.</summary>
	[XmlIgnore]
	public bool canUnderstandDwarves
	{
		get
		{
			return Game1.MasterPlayer.mailReceived.Contains("HasDwarvishTranslationGuide");
		}
		set
		{
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasDwarvishTranslationGuide", MailType.Received, value);
		}
	}

	/// <summary>Whether this player has unlocked access to the casino club.</summary>
	[XmlIgnore]
	public bool hasClubCard
	{
		get
		{
			return this.mailReceived.Contains("HasClubCard");
		}
		set
		{
			this.mailReceived.Toggle("HasClubCard", value);
		}
	}

	/// <summary>Whether this player has found the dark talisman, which unblocks the railroad's northeast path.</summary>
	[XmlIgnore]
	public bool hasDarkTalisman
	{
		get
		{
			return this.mailReceived.Contains("HasDarkTalisman");
		}
		set
		{
			this.mailReceived.Toggle("HasDarkTalisman", value);
		}
	}

	/// <summary>Whether this player has found the magic ink which allows magical building construction by the Wizard.</summary>
	[XmlIgnore]
	public bool hasMagicInk
	{
		get
		{
			return this.mailReceived.Contains("HasMagicInk");
		}
		set
		{
			this.mailReceived.Toggle("HasMagicInk", value);
		}
	}

	/// <summary>Whether this player has found the magnifying glass which allows finding secret notes.</summary>
	[XmlIgnore]
	public bool hasMagnifyingGlass
	{
		get
		{
			return this.mailReceived.Contains("HasMagnifyingGlass");
		}
		set
		{
			this.mailReceived.Toggle("HasMagnifyingGlass", value);
		}
	}

	/// <summary>Whether <strong>any player</strong> has found the Rusty Key which unlocks the sewers.</summary>
	[XmlIgnore]
	public bool hasRustyKey
	{
		get
		{
			return Game1.MasterPlayer.mailReceived.Contains("HasRustyKey");
		}
		set
		{
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasRustyKey", MailType.Received, value);
		}
	}

	/// <summary>Whether <strong>any player</strong> has found the Skull Key which unlocks the skull caverns.</summary>
	[XmlIgnore]
	public bool hasSkullKey
	{
		get
		{
			return Game1.MasterPlayer.mailReceived.Contains("HasSkullKey");
		}
		set
		{
			Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "HasSkullKey", MailType.Received, value);
		}
	}

	/// <summary>Whether this player has the Special Charm which increases daily luck.</summary>
	[XmlIgnore]
	public bool hasSpecialCharm
	{
		get
		{
			return this.mailReceived.Contains("HasSpecialCharm");
		}
		set
		{
			this.mailReceived.Toggle("HasSpecialCharm", value);
		}
	}

	/// <summary>Whether this player has unlocked the 'Key to the Town' item which lets them enter all town buildings.</summary>
	[XmlIgnore]
	public bool HasTownKey
	{
		get
		{
			return this.mailReceived.Contains("HasTownKey");
		}
		set
		{
			this.mailReceived.Toggle("HasTownKey", value);
		}
	}

	/// <summary>Whether the player has unlocked the door to the skull caverns using <see cref="P:StardewValley.Farmer.hasSkullKey" />.</summary>
	[XmlIgnore]
	public bool hasUnlockedSkullDoor
	{
		get
		{
			return this.mailReceived.Contains("HasUnlockedSkullDoor");
		}
		set
		{
			this.mailReceived.Toggle("HasUnlockedSkullDoor", value);
		}
	}

	[XmlIgnore]
	public bool hasPendingCompletedQuests
	{
		get
		{
			foreach (SpecialOrder quest in this.team.specialOrders)
			{
				if (quest.participants.ContainsKey(this.UniqueMultiplayerID) && quest.ShouldDisplayAsComplete())
				{
					return true;
				}
			}
			foreach (Quest quest2 in this.questLog)
			{
				if (!quest2.IsHidden() && quest2.ShouldDisplayAsComplete() && !quest2.destroy.Value)
				{
					return true;
				}
			}
			return false;
		}
	}

	[XmlElement("useSeparateWallets")]
	public bool useSeparateWallets
	{
		get
		{
			return this.teamRoot.Value.useSeparateWallets.Value;
		}
		set
		{
			this.teamRoot.Value.useSeparateWallets.Value = value;
		}
	}

	[XmlElement("theaterBuildDate")]
	public long theaterBuildDate
	{
		get
		{
			return this.teamRoot.Value.theaterBuildDate.Value;
		}
		set
		{
			this.teamRoot.Value.theaterBuildDate.Value = value;
		}
	}

	public int timesReachedMineBottom
	{
		get
		{
			return this.netTimesReachedMineBottom.Value;
		}
		set
		{
			this.netTimesReachedMineBottom.Value = value;
		}
	}

	[XmlIgnore]
	public bool canReleaseTool
	{
		get
		{
			return this.netCanReleaseTool.Value;
		}
		set
		{
			this.netCanReleaseTool.Value = value;
		}
	}

	/// <summary>The player's NPC spouse or roommate.</summary>
	[XmlElement("spouse")]
	public string spouse
	{
		get
		{
			if (!string.IsNullOrEmpty(this.netSpouse.Value))
			{
				return this.netSpouse.Value;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				this.netSpouse.Value = "";
			}
			else
			{
				this.netSpouse.Value = value;
			}
		}
	}

	[XmlIgnore]
	public bool isUnclaimedFarmhand
	{
		get
		{
			if (!this.IsMainPlayer)
			{
				return !this.isCustomized.Value;
			}
			return false;
		}
	}

	[XmlIgnore]
	public Horse mount
	{
		get
		{
			return this.netMount.Value;
		}
		set
		{
			this.setMount(value);
		}
	}

	[XmlIgnore]
	public int MaxItems
	{
		get
		{
			return this.maxItems.Value;
		}
		set
		{
			this.maxItems.Value = value;
		}
	}

	[XmlIgnore]
	public int Level => (this.farmingLevel.Value + this.fishingLevel.Value + this.foragingLevel.Value + this.combatLevel.Value + this.miningLevel.Value + this.luckLevel.Value) / 2;

	[XmlIgnore]
	public int FarmingLevel => Math.Max(this.farmingLevel.Value + this.buffs.FarmingLevel, 0);

	[XmlIgnore]
	public int MiningLevel => Math.Max(this.miningLevel.Value + this.buffs.MiningLevel, 0);

	[XmlIgnore]
	public int CombatLevel => Math.Max(this.combatLevel.Value + this.buffs.CombatLevel, 0);

	[XmlIgnore]
	public int ForagingLevel => Math.Max(this.foragingLevel.Value + this.buffs.ForagingLevel, 0);

	[XmlIgnore]
	public int FishingLevel => Math.Max(this.fishingLevel.Value + this.buffs.FishingLevel, 0);

	[XmlIgnore]
	public int LuckLevel => Math.Max(this.luckLevel.Value + this.buffs.LuckLevel, 0);

	[XmlIgnore]
	public double DailyLuck => Math.Min(Math.Max(this.team.sharedDailyLuck.Value + (double)(this.hasSpecialCharm ? 0.025f : 0f), -0.20000000298023224), 0.20000000298023224);

	[XmlIgnore]
	public int HouseUpgradeLevel
	{
		get
		{
			return this.houseUpgradeLevel.Value;
		}
		set
		{
			this.houseUpgradeLevel.Value = value;
		}
	}

	[XmlIgnore]
	public BoundingBoxGroup TemporaryPassableTiles
	{
		get
		{
			return this.temporaryPassableTiles;
		}
		set
		{
			this.temporaryPassableTiles = value;
		}
	}

	[XmlIgnore]
	public Inventory Items => this.netItems.Value;

	[XmlIgnore]
	public int MagneticRadius => Math.Max(this.BaseMagneticRadius + this.buffs.MagneticRadius, 0);

	[XmlIgnore]
	public Item ActiveItem
	{
		get
		{
			if (this.TemporaryItem != null)
			{
				return this.TemporaryItem;
			}
			if (this._itemStowed)
			{
				return null;
			}
			if (this.currentToolIndex.Value < this.Items.Count && this.Items[this.currentToolIndex.Value] != null)
			{
				return this.Items[this.currentToolIndex.Value];
			}
			return null;
		}
		set
		{
			this.netItemStowed.Set(newValue: false);
			if (value == null)
			{
				this.removeItemFromInventory(this.ActiveItem);
			}
			else
			{
				this.addItemToInventory(value, this.CurrentToolIndex);
			}
		}
	}

	[XmlIgnore]
	public Object ActiveObject
	{
		get
		{
			return this.ActiveItem as Object;
		}
		set
		{
			this.ActiveItem = value;
		}
	}

	/// <summary>The player's gender identity.</summary>
	[XmlIgnore]
	public override Gender Gender
	{
		get
		{
			return this.netGender.Value;
		}
		set
		{
			this.netGender.Value = value;
		}
	}

	[XmlIgnore]
	public bool IsMale => this.netGender.Value == Gender.Male;

	[XmlIgnore]
	public ISet<string> DialogueQuestionsAnswered => this.dialogueQuestionsAnswered;

	[XmlIgnore]
	public bool CanMove
	{
		get
		{
			return this.canMove;
		}
		set
		{
			this.canMove = value;
		}
	}

	[XmlIgnore]
	public bool UsingTool
	{
		get
		{
			return this.usingTool.Value;
		}
		set
		{
			this.usingTool.Set(value);
		}
	}

	[XmlIgnore]
	public Tool CurrentTool
	{
		get
		{
			return this.CurrentItem as Tool;
		}
		set
		{
			while (this.CurrentToolIndex >= this.Items.Count)
			{
				this.Items.Add(null);
			}
			this.Items[this.CurrentToolIndex] = value;
		}
	}

	[XmlIgnore]
	public Item TemporaryItem
	{
		get
		{
			return this.temporaryItem.Value;
		}
		set
		{
			this.temporaryItem.Value = value;
		}
	}

	public Item CursorSlotItem
	{
		get
		{
			return this.cursorSlotItem.Value;
		}
		set
		{
			this.cursorSlotItem.Value = value;
		}
	}

	[XmlIgnore]
	public Item CurrentItem
	{
		get
		{
			if (this.TemporaryItem != null)
			{
				return this.TemporaryItem;
			}
			if (this._itemStowed)
			{
				return null;
			}
			if (this.currentToolIndex.Value >= this.Items.Count)
			{
				return null;
			}
			return this.Items[this.currentToolIndex.Value];
		}
	}

	[XmlIgnore]
	public int CurrentToolIndex
	{
		get
		{
			return this.currentToolIndex.Value;
		}
		set
		{
			this.netItemStowed.Set(newValue: false);
			if (this.currentToolIndex.Value >= 0 && this.CurrentItem != null && value != this.currentToolIndex.Value)
			{
				this.CurrentItem.actionWhenStopBeingHeld(this);
			}
			this.currentToolIndex.Set(value);
		}
	}

	[XmlIgnore]
	public float Stamina
	{
		get
		{
			return this.stamina;
		}
		set
		{
			if (!this.hasBuff("statue_of_blessings_2") || !(value < this.stamina))
			{
				this.stamina = Math.Min(this.MaxStamina, Math.Max(value, -16f));
			}
		}
	}

	[XmlIgnore]
	public int MaxStamina => Math.Max(this.maxStamina.Value + this.buffs.MaxStamina, 0);

	[XmlIgnore]
	public int Attack => this.buffs.Attack;

	[XmlIgnore]
	public int Immunity => this.buffs.Immunity;

	[XmlIgnore]
	public override float addedSpeed
	{
		get
		{
			return this.buffs.Speed + ((this.stats.Get("Book_Speed") != 0 && !this.isRidingHorse()) ? 0.25f : 0f) + ((this.stats.Get("Book_Speed2") != 0 && !this.isRidingHorse()) ? 0.25f : 0f);
		}
		[Obsolete("Player speed can't be changed directly. You can add a speed buff via applyBuff instead (and optionally mark it invisible).")]
		set
		{
		}
	}

	public long UniqueMultiplayerID
	{
		get
		{
			return this.uniqueMultiplayerID.Value;
		}
		set
		{
			this.uniqueMultiplayerID.Value = value;
		}
	}

	/// <summary>Whether this is the farmer controlled by the local player, <strong>or</strong> the main farmer in an event being viewed by the local player (even if that farmer instance is a different player).</summary>
	[XmlIgnore]
	public bool IsLocalPlayer
	{
		get
		{
			if (this.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
			{
				if (Game1.CurrentEvent != null)
				{
					return Game1.CurrentEvent.farmer == this;
				}
				return false;
			}
			return true;
		}
	}

	[XmlIgnore]
	public bool IsMainPlayer
	{
		get
		{
			if (!(Game1.serverHost == null) || !this.IsLocalPlayer)
			{
				if (Game1.serverHost != null)
				{
					return this.UniqueMultiplayerID == Game1.serverHost.Value.UniqueMultiplayerID;
				}
				return false;
			}
			return true;
		}
	}

	[XmlIgnore]
	public bool IsDedicatedPlayer
	{
		get
		{
			if (Game1.HasDedicatedHost && Game1.serverHost != null)
			{
				return this.UniqueMultiplayerID == Game1.serverHost.Value.UniqueMultiplayerID;
			}
			return false;
		}
	}

	[XmlIgnore]
	public override AnimatedSprite Sprite
	{
		get
		{
			return base.Sprite;
		}
		set
		{
			base.Sprite = value;
		}
	}

	[XmlIgnore]
	public FarmerSprite FarmerSprite
	{
		get
		{
			return (FarmerSprite)this.Sprite;
		}
		set
		{
			this.Sprite = value;
		}
	}

	[XmlIgnore]
	public FarmerRenderer FarmerRenderer
	{
		get
		{
			return this.farmerRenderer.Value;
		}
		set
		{
			this.farmerRenderer.Set(value);
		}
	}

	[XmlElement("money")]
	public int _money
	{
		get
		{
			return this.teamRoot.Value.GetMoney(this).Value;
		}
		set
		{
			this.teamRoot.Value.GetMoney(this).Value = value;
		}
	}

	[XmlIgnore]
	public int QiGems
	{
		get
		{
			return this.netQiGems.Value;
		}
		set
		{
			this.netQiGems.Value = value;
		}
	}

	[XmlIgnore]
	public int Money
	{
		get
		{
			return this._money;
		}
		set
		{
			if (Game1.player != this)
			{
				throw new Exception("Cannot change another farmer's money. Use Game1.player.team.SetIndividualMoney");
			}
			int previousMoney = this._money;
			this._money = value;
			if (value > previousMoney)
			{
				uint earned = (uint)(value - previousMoney);
				this.totalMoneyEarned += earned;
				if (this.useSeparateWallets)
				{
					this.stats.IndividualMoneyEarned += earned;
				}
				Game1.stats.checkForMoneyAchievements();
			}
		}
	}

	public override int FacingDirection
	{
		get
		{
			if (!this.IsLocalPlayer && !this.isFakeEventActor && this.UsingTool && this.CurrentTool is FishingRod { CastDirection: >=0 } rod)
			{
				return rod.CastDirection;
			}
			if (this.isEmoteAnimating)
			{
				return this.emoteFacingDirection;
			}
			return base.facingDirection.Value;
		}
		set
		{
			base.facingDirection.Set(value);
		}
	}

	public void addUnearnedMoney(int money)
	{
		this._money += money;
	}

	public List<string> GetEmoteFavorites()
	{
		if (this.emoteFavorites.Count == 0)
		{
			this.emoteFavorites.Add("question");
			this.emoteFavorites.Add("heart");
			this.emoteFavorites.Add("yes");
			this.emoteFavorites.Add("happy");
			this.emoteFavorites.Add("pause");
			this.emoteFavorites.Add("sad");
			this.emoteFavorites.Add("no");
			this.emoteFavorites.Add("angry");
		}
		return this.emoteFavorites;
	}

	public Farmer()
	{
		this.farmerInit();
		this.Sprite = new FarmerSprite(null);
	}

	public Farmer(FarmerSprite sprite, Vector2 position, int speed, string name, List<Item> initialTools, bool isMale)
		: base(sprite, position, speed, name)
	{
		this.farmerInit();
		base.Name = name;
		this.displayName = name;
		this.Gender = ((!isMale) ? Gender.Female : Gender.Male);
		this.stamina = this.maxStamina.Value;
		this.Items.OverwriteWith(initialTools);
		for (int i = this.Items.Count; i < this.maxItems.Value; i++)
		{
			this.Items.Add(null);
		}
		this.activeDialogueEvents["Introduction"] = 6;
		if (base.currentLocation != null)
		{
			this.mostRecentBed = Utility.PointToVector2((base.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f;
		}
		else
		{
			this.mostRecentBed = new Vector2(9f, 9f) * 64f;
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.uniqueMultiplayerID, "uniqueMultiplayerID").AddField(this.userID, "userID").AddField(this.platformType, "platformType")
			.AddField(this.platformID, "platformID")
			.AddField(this.hasMenuOpen, "hasMenuOpen")
			.AddField(this.farmerRenderer, "farmerRenderer")
			.AddField(this.netGender, "netGender")
			.AddField(this.bathingClothes, "bathingClothes")
			.AddField(this.shirt, "shirt")
			.AddField(this.pants, "pants")
			.AddField(this.hair, "hair")
			.AddField(this.skin, "skin")
			.AddField(this.shoes, "shoes")
			.AddField(this.accessory, "accessory")
			.AddField(this.facialHair, "facialHair")
			.AddField(this.hairstyleColor, "hairstyleColor")
			.AddField(this.pantsColor, "pantsColor")
			.AddField(this.newEyeColor, "newEyeColor")
			.AddField(this.netItems, "netItems")
			.AddField(this.currentToolIndex, "currentToolIndex")
			.AddField(this.temporaryItem, "temporaryItem")
			.AddField(this.cursorSlotItem, "cursorSlotItem")
			.AddField(this.fireToolEvent, "fireToolEvent")
			.AddField(this.beginUsingToolEvent, "beginUsingToolEvent")
			.AddField(this.endUsingToolEvent, "endUsingToolEvent")
			.AddField(this.hat, "hat")
			.AddField(this.boots, "boots")
			.AddField(this.leftRing, "leftRing")
			.AddField(this.rightRing, "rightRing")
			.AddField(this.hidden, "hidden")
			.AddField(this.usingTool, "usingTool")
			.AddField(this.isInBed, "isInBed")
			.AddField(this.bobberStyle, "bobberStyle")
			.AddField(this.caveChoice, "caveChoice")
			.AddField(this.houseUpgradeLevel, "houseUpgradeLevel")
			.AddField(this.daysUntilHouseUpgrade, "daysUntilHouseUpgrade")
			.AddField(this.netSpouse, "netSpouse")
			.AddField(this.mailReceived, "mailReceived")
			.AddField(this.mailForTomorrow, "mailForTomorrow")
			.AddField(this.mailbox, "mailbox")
			.AddField(this.triggerActionsRun, "triggerActionsRun")
			.AddField(this.eventsSeen, "eventsSeen")
			.AddField(this.locationsVisited, "locationsVisited")
			.AddField(this.secretNotesSeen, "secretNotesSeen")
			.AddField(this.netMount.NetFields, "netMount.NetFields")
			.AddField(this.dancePartner.NetFields, "dancePartner.NetFields")
			.AddField(this.divorceTonight, "divorceTonight")
			.AddField(this.changeWalletTypeTonight, "changeWalletTypeTonight")
			.AddField(this.isCustomized, "isCustomized")
			.AddField(this.homeLocation, "homeLocation")
			.AddField(this.farmName, "farmName")
			.AddField(this.favoriteThing, "favoriteThing")
			.AddField(this.horseName, "horseName")
			.AddField(this.netMillisecondsPlayed, "netMillisecondsPlayed")
			.AddField(this.netFestivalScore, "netFestivalScore")
			.AddField(this.friendshipData, "friendshipData")
			.AddField(this.drinkAnimationEvent, "drinkAnimationEvent")
			.AddField(this.eatAnimationEvent, "eatAnimationEvent")
			.AddField(this.sickAnimationEvent, "sickAnimationEvent")
			.AddField(this.passOutEvent, "passOutEvent")
			.AddField(this.doEmoteEvent, "doEmoteEvent")
			.AddField(this.questLog, "questLog")
			.AddField(this.professions, "professions")
			.AddField(this.newLevels, "newLevels")
			.AddField(this.experiencePoints, "experiencePoints")
			.AddField(this.dialogueQuestionsAnswered, "dialogueQuestionsAnswered")
			.AddField(this.cookingRecipes, "cookingRecipes")
			.AddField(this.craftingRecipes, "craftingRecipes")
			.AddField(this.activeDialogueEvents, "activeDialogueEvents")
			.AddField(this.previousActiveDialogueEvents, "previousActiveDialogueEvents")
			.AddField(this.achievements, "achievements")
			.AddField(this.specialItems, "specialItems")
			.AddField(this.specialBigCraftables, "specialBigCraftables")
			.AddField(this.farmingLevel, "farmingLevel")
			.AddField(this.miningLevel, "miningLevel")
			.AddField(this.combatLevel, "combatLevel")
			.AddField(this.foragingLevel, "foragingLevel")
			.AddField(this.fishingLevel, "fishingLevel")
			.AddField(this.luckLevel, "luckLevel")
			.AddField(this.maxStamina, "maxStamina")
			.AddField(this.netStamina, "netStamina")
			.AddField(this.maxItems, "maxItems")
			.AddField(this.chestConsumedMineLevels, "chestConsumedMineLevels")
			.AddField(this.toolBeingUpgraded, "toolBeingUpgraded")
			.AddField(this.daysLeftForToolUpgrade, "daysLeftForToolUpgrade")
			.AddField(this.exhausted, "exhausted")
			.AddField(this.netDeepestMineLevel, "netDeepestMineLevel")
			.AddField(this.netTimesReachedMineBottom, "netTimesReachedMineBottom")
			.AddField(this.netItemStowed, "netItemStowed")
			.AddField(this.acceptedDailyQuest, "acceptedDailyQuest")
			.AddField(this.lastSeenMovieWeek, "lastSeenMovieWeek")
			.AddField(this.shirtItem, "shirtItem")
			.AddField(this.pantsItem, "pantsItem")
			.AddField(this.personalShippingBin, "personalShippingBin")
			.AddField(this.viewingLocation, "viewingLocation")
			.AddField(this.kissFarmerEvent, "kissFarmerEvent")
			.AddField(this.haltAnimationEvent, "haltAnimationEvent")
			.AddField(this.synchronizedJumpEvent, "synchronizedJumpEvent")
			.AddField(this.tailoredItems, "tailoredItems")
			.AddField(this.basicShipped, "basicShipped")
			.AddField(this.mineralsFound, "mineralsFound")
			.AddField(this.recipesCooked, "recipesCooked")
			.AddField(this.archaeologyFound, "archaeologyFound")
			.AddField(this.fishCaught, "fishCaught")
			.AddField(this.biteChime, "biteChime")
			.AddField(this._recoveredItem, "_recoveredItem")
			.AddField(this.itemsLostLastDeath, "itemsLostLastDeath")
			.AddField(this.renovateEvent, "renovateEvent")
			.AddField(this.callsReceived, "callsReceived")
			.AddField(this.onBridge, "onBridge")
			.AddField(this.lastSleepLocation, "lastSleepLocation")
			.AddField(this.lastSleepPoint, "lastSleepPoint")
			.AddField(this.sleptInTemporaryBed, "sleptInTemporaryBed")
			.AddField(this.timeWentToBed, "timeWentToBed")
			.AddField(this.hasUsedDailyRevive, "hasUsedDailyRevive")
			.AddField(this.jotpkProgress, "jotpkProgress")
			.AddField(this.requestingTimePause, "requestingTimePause")
			.AddField(this.isSitting, "isSitting")
			.AddField(this.mapChairSitPosition, "mapChairSitPosition")
			.AddField(this.netQiGems, "netQiGems")
			.AddField(this.locationBeforeForcedEvent, "locationBeforeForcedEvent")
			.AddField(this.hasCompletedAllMonsterSlayerQuests, "hasCompletedAllMonsterSlayerQuests")
			.AddField(this.buffs.NetFields, "buffs.NetFields")
			.AddField(this.trinketItems, "trinketItems")
			.AddField(this.companions, "companions")
			.AddField(this.prismaticHair, "prismaticHair")
			.AddField(this.disconnectDay, "disconnectDay")
			.AddField(this.disconnectLocation, "disconnectLocation")
			.AddField(this.disconnectPosition, "disconnectPosition")
			.AddField(this.tempFoodItemTextureName, "tempFoodItemTextureName")
			.AddField(this.tempFoodItemSourceRect, "tempFoodItemSourceRect")
			.AddField(this.toolHoldStartTime, "toolHoldStartTime")
			.AddField(this.toolHold, "toolHold")
			.AddField(this.toolPower, "toolPower")
			.AddField(this.netCanReleaseTool, "netCanReleaseTool")
			.AddField(this.lastGotPrizeFromGil, "lastGotPrizeFromGil")
			.AddField(this.lastDesertFestivalFishingQuest, "lastDesertFestivalFishingQuest");
		this.fireToolEvent.onEvent += performFireTool;
		this.beginUsingToolEvent.onEvent += performBeginUsingTool;
		this.endUsingToolEvent.onEvent += performEndUsingTool;
		this.drinkAnimationEvent.onEvent += performDrinkAnimation;
		this.eatAnimationEvent.onEvent += performEatAnimation;
		this.sickAnimationEvent.onEvent += performSickAnimation;
		this.passOutEvent.onEvent += performPassOut;
		this.doEmoteEvent.onEvent += performPlayerEmote;
		this.kissFarmerEvent.onEvent += performKissFarmer;
		this.haltAnimationEvent.onEvent += performHaltAnimation;
		this.synchronizedJumpEvent.onEvent += performSynchronizedJump;
		this.renovateEvent.onEvent += performRenovation;
		this.netMount.fieldChangeEvent += delegate
		{
			base.ClearCachedPosition();
		};
		this.shirtItem.fieldChangeVisibleEvent += delegate
		{
			this.UpdateClothing();
		};
		this.pantsItem.fieldChangeVisibleEvent += delegate
		{
			this.UpdateClothing();
		};
		this.trinketItems.OnArrayReplaced += OnTrinketArrayReplaced;
		this.trinketItems.OnElementChanged += OnTrinketChange;
		this.netItems.fieldChangeEvent += delegate(NetRef<Inventory> field, Inventory oldValue, Inventory newValue)
		{
			newValue.IsLocalPlayerInventory = this.IsLocalPlayer;
		};
	}

	private void farmerInit()
	{
		this.buffs.SetOwner(this);
		this.FarmerRenderer = new FarmerRenderer("Characters\\Farmer\\farmer_" + (this.IsMale ? "" : "girl_") + "base", this);
		base.currentLocation = Game1.getLocationFromName(this.homeLocation.Value);
		this.Items.Clear();
		this.giftedItems = new SerializableDictionary<string, SerializableDictionary<string, int>>();
		this.LearnDefaultRecipes();
		this.songsHeard.Add("title_day");
		this.songsHeard.Add("title_night");
		this.changeShirt("1000");
		this.changeSkinColor(0);
		this.changeShoeColor("2");
		this.farmName.FilterStringEvent += Utility.FilterDirtyWords;
		base.name.FilterStringEvent += Utility.FilterDirtyWords;
	}

	public virtual void OnWarp()
	{
		foreach (Companion companion in this.companions)
		{
			companion.OnOwnerWarp();
		}
		this.autoGenerateActiveDialogueEvent("firstVisit_" + base.currentLocation.Name);
		if (!Stats.AllowRetroactiveAchievements)
		{
			switch (base.currentLocation.Name)
			{
			case "CommunityCenter":
			case "JojaMart":
				Game1.stats.checkForCommunityCenterOrJojaAchievements(isDirectUnlock: true);
				break;
			case "MasteryCave":
				Game1.stats.checkForSkillAchievements(isDirectUnlock: true);
				Game1.stats.checkForStardropAchievement(isDirectUnlock: true);
				break;
			}
		}
	}

	public Trinket getFirstTrinketWithID(string id)
	{
		foreach (Trinket trinket in this.trinketItems)
		{
			if (trinket != null && trinket.ItemId == id)
			{
				return trinket;
			}
		}
		return null;
	}

	public bool hasTrinketWithID(string id)
	{
		foreach (Trinket trinket in this.trinketItems)
		{
			if (trinket != null && trinket.ItemId == id)
			{
				return true;
			}
		}
		return false;
	}

	public void resetAllTrinketEffects()
	{
		this.UnapplyAllTrinketEffects();
		this.ApplyAllTrinketEffects();
	}

	public virtual void ApplyAllTrinketEffects()
	{
		foreach (Trinket trinket in this.trinketItems)
		{
			if (trinket != null)
			{
				trinket.reloadSprite();
				trinket.Apply(this);
			}
		}
	}

	public virtual void UnapplyAllTrinketEffects()
	{
		foreach (Trinket trinketItem in this.trinketItems)
		{
			trinketItem?.Unapply(this);
		}
	}

	public virtual void OnTrinketArrayReplaced(NetList<Trinket, NetRef<Trinket>> list, IList<Trinket> before, IList<Trinket> after)
	{
		if ((Game1.gameMode != 0 && Utility.ShouldIgnoreValueChangeCallback()) || (!this.IsLocalPlayer && !this.isFakeEventActor && Game1.gameMode != 0))
		{
			return;
		}
		foreach (Trinket item in before)
		{
			item?.Unapply(this);
		}
		foreach (Trinket item2 in after)
		{
			item2?.Apply(this);
		}
	}

	public virtual void OnTrinketChange(NetList<Trinket, NetRef<Trinket>> list, int index, Trinket old_value, Trinket new_value)
	{
		if ((Game1.gameMode == 0 || !Utility.ShouldIgnoreValueChangeCallback()) && (this.IsLocalPlayer || this.isFakeEventActor || Game1.gameMode == 0))
		{
			old_value?.Unapply(this);
			new_value?.Apply(this);
		}
	}

	public bool CanEmote()
	{
		if (Game1.farmEvent != null)
		{
			return false;
		}
		if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence && this.IsLocalPlayer)
		{
			return false;
		}
		if (this.usingSlingshot)
		{
			return false;
		}
		if (this.isEating)
		{
			return false;
		}
		if (this.UsingTool)
		{
			return false;
		}
		if (!this.CanMove && this.IsLocalPlayer)
		{
			return false;
		}
		if (this.IsSitting())
		{
			return false;
		}
		if (this.isRidingHorse())
		{
			return false;
		}
		if (this.bathingClothes.Value)
		{
			return false;
		}
		return true;
	}

	/// <summary>Learn the recipes that have no unlock requirements.</summary>
	public void LearnDefaultRecipes()
	{
		foreach (KeyValuePair<string, string> recipe in CraftingRecipe.craftingRecipes)
		{
			if (!this.craftingRecipes.ContainsKey(recipe.Key) && ArgUtility.Get(recipe.Value.Split('/'), 4) == "default")
			{
				this.craftingRecipes.Add(recipe.Key, 0);
			}
		}
		foreach (KeyValuePair<string, string> recipe2 in CraftingRecipe.cookingRecipes)
		{
			if (!this.cookingRecipes.ContainsKey(recipe2.Key) && ArgUtility.Get(recipe2.Value.Split('/'), 3) == "default")
			{
				this.cookingRecipes.Add(recipe2.Key, 0);
			}
		}
	}

	/// <summary>Add any recipes and mail that should have been unlocked based on criteria like skill levels and stats.</summary>
	public void AddMissedMailAndRecipes()
	{
		bool addRobinKitchenLetter = false;
		foreach (KeyValuePair<string, string> v in CraftingRecipe.cookingRecipes)
		{
			if (CraftingRecipe.TryParseLevelRequirement(v.Key, v.Value, isCooking: true, out var reqSkillNumber, out var minLevel) && this.GetUnmodifiedSkillLevel(reqSkillNumber) >= minLevel)
			{
				this.cookingRecipes.TryAdd(v.Key, 0);
				addRobinKitchenLetter = true;
			}
		}
		foreach (KeyValuePair<string, string> v2 in CraftingRecipe.craftingRecipes)
		{
			if (CraftingRecipe.TryParseLevelRequirement(v2.Key, v2.Value, isCooking: false, out var reqSkillNumber2, out var minLevel2) && this.GetUnmodifiedSkillLevel(reqSkillNumber2) >= minLevel2)
			{
				this.craftingRecipes.TryAdd(v2.Key, 0);
			}
		}
		if (addRobinKitchenLetter && !this.hasOrWillReceiveMail("robinKitchenLetter"))
		{
			this.team.RequestSetMail(PlayerActionTarget.All, "robinKitchenLetter", MailType.Now, add: true, this.uniqueMultiplayerID.Value);
		}
		if (this.farmingLevel.Value >= 10 && !this.hasOrWillReceiveMail("marnieAutoGrabber"))
		{
			this.team.RequestSetMail(PlayerActionTarget.All, "marnieAutoGrabber", MailType.Tomorrow, add: true, this.uniqueMultiplayerID.Value);
		}
		if (this.stats.Get("completedJunimoKart") != 0 && !this.hasOrWillReceiveMail("JunimoKart"))
		{
			this.team.RequestSetMail(PlayerActionTarget.All, "JunimoKart", MailType.Tomorrow, add: true, this.uniqueMultiplayerID.Value);
		}
		if (this.stats.Get("completedPrairieKing") != 0 && !this.hasOrWillReceiveMail("Beat_PK"))
		{
			this.team.RequestSetMail(PlayerActionTarget.All, "Beat_PK", MailType.Tomorrow, add: true, this.uniqueMultiplayerID.Value);
		}
	}

	public void performRenovation(string location_name)
	{
		if (Game1.RequireLocation(location_name) is FarmHouse farmhouse)
		{
			farmhouse.UpdateForRenovation();
		}
	}

	public void performPlayerEmote(string emote_string)
	{
		for (int i = 0; i < Farmer.EMOTES.Length; i++)
		{
			EmoteType emote_type = Farmer.EMOTES[i];
			if (!(emote_type.emoteString == emote_string))
			{
				continue;
			}
			this.performedEmotes[emote_string] = true;
			if (emote_type.animationFrames != null)
			{
				if (!this.CanEmote())
				{
					break;
				}
				if (this.isEmoteAnimating)
				{
					this.EndEmoteAnimation();
				}
				else if (this.FarmerSprite.PauseForSingleAnimation)
				{
					break;
				}
				this.isEmoteAnimating = true;
				this._emoteGracePeriod = 200;
				if (this == Game1.player)
				{
					this.noMovementPause = Math.Max(this.noMovementPause, 200);
				}
				this.emoteFacingDirection = emote_type.facingDirection;
				this.FarmerSprite.animateOnce(emote_type.animationFrames, OnEmoteAnimationEnd);
			}
			if (emote_type.emoteIconIndex >= 0)
			{
				base.isEmoting = false;
				base.doEmote(emote_type.emoteIconIndex, nextEventCommand: false);
			}
		}
	}

	public bool ShouldHandleAnimationSound()
	{
		if (!LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
		{
			return true;
		}
		if (this.IsLocalPlayer)
		{
			return true;
		}
		return false;
	}

	public static List<Item> initialTools()
	{
		return new List<Item>
		{
			ItemRegistry.Create("(T)Axe"),
			ItemRegistry.Create("(T)Hoe"),
			ItemRegistry.Create("(T)WateringCan"),
			ItemRegistry.Create("(T)Pickaxe"),
			ItemRegistry.Create("(W)47")
		};
	}

	private void playHarpEmoteSound()
	{
		int[] notes = new int[4] { 1200, 1600, 1900, 2400 };
		switch (Game1.random.Next(5))
		{
		case 0:
			notes = new int[4] { 1200, 1600, 1900, 2400 };
			break;
		case 1:
			notes = new int[4] { 1200, 1700, 2100, 2400 };
			break;
		case 2:
			notes = new int[4] { 1100, 1400, 1900, 2300 };
			break;
		case 3:
			notes = new int[3] { 1600, 1900, 2400 };
			break;
		case 4:
			notes = new int[3] { 700, 1200, 1900 };
			break;
		}
		if (!this.IsLocalPlayer)
		{
			return;
		}
		if (Game1.IsMultiplayer && this.UniqueMultiplayerID % 111 == 0L)
		{
			notes = new int[4]
			{
				800 + Game1.random.Next(4) * 100,
				1200 + Game1.random.Next(4) * 100,
				1600 + Game1.random.Next(4) * 100,
				2000 + Game1.random.Next(4) * 100
			};
			for (int i = 0; i < notes.Length; i++)
			{
				DelayedAction.playSoundAfterDelay("miniharp_note", Game1.random.Next(60, 150) * i, base.currentLocation, base.Tile, notes[i]);
				if (i > 1 && Game1.random.NextDouble() < 0.25)
				{
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < notes.Length; j++)
			{
				DelayedAction.playSoundAfterDelay("miniharp_note", (j > 0) ? (150 + Game1.random.Next(35, 51) * j) : 0, base.currentLocation, base.Tile, notes[j]);
			}
		}
	}

	private static void removeLowestUpgradeLevelTool(List<Item> items, Type toolType)
	{
		Tool lowestItem = null;
		foreach (Item item in items)
		{
			if (item is Tool tool && tool.GetType() == toolType && (lowestItem == null || tool.upgradeLevel.Value < lowestItem.upgradeLevel.Value))
			{
				lowestItem = tool;
			}
		}
		if (lowestItem != null)
		{
			items.Remove(lowestItem);
		}
	}

	public static void removeInitialTools(List<Item> items)
	{
		Farmer.removeLowestUpgradeLevelTool(items, typeof(Axe));
		Farmer.removeLowestUpgradeLevelTool(items, typeof(Hoe));
		Farmer.removeLowestUpgradeLevelTool(items, typeof(WateringCan));
		Farmer.removeLowestUpgradeLevelTool(items, typeof(Pickaxe));
		Item scythe = items.FirstOrDefault((Item item) => item is MeleeWeapon && item.ItemId == "47");
		if (scythe != null)
		{
			items.Remove(scythe);
		}
	}

	public Point getMailboxPosition()
	{
		foreach (Building b in Game1.getFarm().buildings)
		{
			if (b.isCabin && b.HasIndoorsName(this.homeLocation.Value))
			{
				return b.getMailboxPosition();
			}
		}
		return Game1.getFarm().GetMainMailboxPosition();
	}

	public void ClearBuffs()
	{
		this.buffs.Clear();
		base.stopGlowing();
	}

	public bool isActive()
	{
		if (this != Game1.player)
		{
			return Game1.otherFarmers.ContainsKey(this.UniqueMultiplayerID);
		}
		return true;
	}

	public string getTexture()
	{
		return "Characters\\Farmer\\farmer_" + (this.IsMale ? "" : "girl_") + "base" + (this.isBald() ? "_bald" : "");
	}

	public void unload()
	{
		this.FarmerRenderer?.unload();
	}

	public void setInventory(List<Item> newInventory)
	{
		this.Items.OverwriteWith(newInventory);
		for (int i = this.Items.Count; i < this.maxItems.Value; i++)
		{
			this.Items.Add(null);
		}
	}

	public void makeThisTheActiveObject(Object o)
	{
		if (this.freeSpotsInInventory() > 0)
		{
			Item i = this.CurrentItem;
			this.ActiveObject = o;
			this.addItemToInventory(i);
		}
	}

	public int getNumberOfChildren()
	{
		return this.getChildrenCount();
	}

	private void setMount(Horse mount)
	{
		if (mount != null)
		{
			this.netMount.Value = mount;
			this.xOffset = -11f;
			base.Position = Utility.PointToVector2(mount.GetBoundingBox().Location);
			base.position.Y -= 16f;
			base.position.X -= 8f;
			base.speed = 2;
			this.showNotCarrying();
			return;
		}
		this.netMount.Value = null;
		this.collisionNPC = null;
		this.running = false;
		base.speed = ((Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.runButton) && !Game1.options.autoRun) ? 5 : 2);
		bool isRunning = base.speed == 5;
		this.running = isRunning;
		if (this.running)
		{
			base.speed = 5;
		}
		else
		{
			base.speed = 2;
		}
		this.completelyStopAnimatingOrDoingAction();
		this.xOffset = 0f;
	}

	public bool isRidingHorse()
	{
		if (this.mount != null)
		{
			return !Game1.eventUp;
		}
		return false;
	}

	public List<Child> getChildren()
	{
		return Utility.getHomeOfFarmer(this).getChildren();
	}

	public int getChildrenCount()
	{
		return Utility.getHomeOfFarmer(this).getChildrenCount();
	}

	public Tool getToolFromName(string name)
	{
		foreach (Item item in this.Items)
		{
			if (item is Tool tool && tool.Name.Contains(name))
			{
				return tool;
			}
		}
		return null;
	}

	public override void SetMovingDown(bool b)
	{
		this.setMoving((byte)(4 + ((!b) ? 32 : 0)));
	}

	public override void SetMovingRight(bool b)
	{
		this.setMoving((byte)(2 + ((!b) ? 32 : 0)));
	}

	public override void SetMovingUp(bool b)
	{
		this.setMoving((byte)(1 + ((!b) ? 32 : 0)));
	}

	public override void SetMovingLeft(bool b)
	{
		this.setMoving((byte)(8 + ((!b) ? 32 : 0)));
	}

	public int? tryGetFriendshipLevelForNPC(string name)
	{
		if (this.friendshipData.TryGetValue(name, out var friendship))
		{
			return friendship.Points;
		}
		return null;
	}

	public int getFriendshipLevelForNPC(string name)
	{
		if (this.friendshipData.TryGetValue(name, out var friendship))
		{
			return friendship.Points;
		}
		return 0;
	}

	public int getFriendshipHeartLevelForNPC(string name)
	{
		return this.getFriendshipLevelForNPC(name) / 250;
	}

	/// <summary>Get whether the player is roommates with a given NPC (excluding marriage).</summary>
	/// <param name="npc">The NPC's internal name.</param>
	/// <remarks>See also <see cref="M:StardewValley.Farmer.hasRoommate" />.</remarks>
	public bool isRoommate(string name)
	{
		if (name != null && this.friendshipData.TryGetValue(name, out var friendship))
		{
			return friendship.IsRoommate();
		}
		return false;
	}

	/// <summary>Get whether the player is or will soon be roommates with an NPC (excluding marriage).</summary>
	public bool hasCurrentOrPendingRoommate()
	{
		if (this.spouse != null && this.friendshipData.TryGetValue(this.spouse, out var friendship))
		{
			return friendship.RoommateMarriage;
		}
		return false;
	}

	/// <summary>Get whether the player is roommates with an NPC (excluding marriage).</summary>
	/// <remarks>See also <see cref="M:StardewValley.Farmer.isRoommate(System.String)" />.</remarks>
	public bool hasRoommate()
	{
		return this.isRoommate(this.spouse);
	}

	public bool hasAFriendWithFriendshipPoints(int minPoints, bool datablesOnly, int maxPoints = int.MaxValue)
	{
		bool found = false;
		Utility.ForEachVillager(delegate(NPC n)
		{
			if (!datablesOnly || n.datable.Value)
			{
				int friendshipLevelForNPC = this.getFriendshipLevelForNPC(n.Name);
				if (friendshipLevelForNPC >= minPoints && friendshipLevelForNPC <= maxPoints)
				{
					found = true;
				}
			}
			return !found;
		});
		return found;
	}

	public bool hasAFriendWithHeartLevel(int minHeartLevel, bool datablesOnly, int maxHeartLevel = int.MaxValue)
	{
		int minPoints = minHeartLevel * 250;
		int maxPoints = maxHeartLevel * 250;
		if (maxPoints < maxHeartLevel)
		{
			maxPoints = int.MaxValue;
		}
		return this.hasAFriendWithFriendshipPoints(minPoints, datablesOnly, maxPoints);
	}

	public void shippedBasic(string itemId, int number)
	{
		if (!this.basicShipped.TryGetValue(itemId, out var curValue))
		{
			curValue = 0;
		}
		this.basicShipped[itemId] = curValue + number;
	}

	public void shiftToolbar(bool right)
	{
		if (this.Items == null || this.Items.Count < 12 || this.UsingTool || Game1.dialogueUp || !this.CanMove || !this.Items.HasAny() || Game1.eventUp || Game1.farmEvent != null)
		{
			return;
		}
		Game1.playSound("shwip");
		this.CurrentItem?.actionWhenStopBeingHeld(this);
		if (right)
		{
			IList<Item> toMove = this.Items.GetRange(0, 12);
			this.Items.RemoveRange(0, 12);
			this.Items.AddRange(toMove);
		}
		else
		{
			IList<Item> toMove2 = this.Items.GetRange(this.Items.Count - 12, 12);
			for (int i = 0; i < this.Items.Count - 12; i++)
			{
				toMove2.Add(this.Items[i]);
			}
			this.Items.OverwriteWith(toMove2);
		}
		this.netItemStowed.Set(newValue: false);
		this.CurrentItem?.actionWhenBeingHeld(this);
		for (int j = 0; j < Game1.onScreenMenus.Count; j++)
		{
			if (Game1.onScreenMenus[j] is Toolbar toolbar)
			{
				toolbar.shifted(right);
				break;
			}
		}
	}

	public void foundWalnut(int stack = 1)
	{
		if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
		{
			Game1.netWorldState.Value.GoldenWalnuts += stack;
			Game1.netWorldState.Value.GoldenWalnutsFound += stack;
			Game1.PerformActionWhenPlayerFree(showNutPickup);
		}
	}

	public virtual void RemoveMail(string mail_key, bool from_broadcast_list = false)
	{
		mail_key = mail_key.Replace("%&NL&%", "");
		this.mailReceived.Remove(mail_key);
		this.mailbox.Remove(mail_key);
		this.mailForTomorrow.Remove(mail_key);
		this.mailForTomorrow.Remove(mail_key + "%&NL&%");
		if (from_broadcast_list)
		{
			this.team.broadcastedMail.Remove("%&SM&%" + mail_key);
			this.team.broadcastedMail.Remove("%&MFT&%" + mail_key);
			this.team.broadcastedMail.Remove("%&MB&%" + mail_key);
		}
	}

	public virtual void showNutPickup()
	{
		if (!this.hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
		{
			Game1.addMailForTomorrow("lostWalnutFound", noLetter: true);
			this.completelyStopAnimatingOrDoingAction();
			this.holdUpItemThenMessage(ItemRegistry.Create("(O)73"));
		}
		else if (this.hasOrWillReceiveMail("lostWalnutFound") && !Game1.eventUp)
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 240, 16, 16), 100f, 4, 2, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.2f),
				stopAcceleratingWhenVelocityIsZero = true,
				attachedCharacter = this,
				positionFollowsAttachedCharacter = true
			});
		}
	}

	/// <summary>Handle the player finding an artifact object.</summary>
	/// <param name="itemId">The unqualified item ID for an <see cref="F:StardewValley.ItemRegistry.type_object" />-type item.</param>
	/// <param name="number">The number found.</param>
	public void foundArtifact(string itemId, int number)
	{
		bool shouldHoldUpArtifact = false;
		if (itemId == "102")
		{
			if (!this.hasOrWillReceiveMail("lostBookFound"))
			{
				Game1.addMailForTomorrow("lostBookFound", noLetter: true);
				shouldHoldUpArtifact = true;
			}
			else
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14100"));
			}
			Game1.playSound("newRecipe");
			Game1.netWorldState.Value.LostBooksFound++;
			Game1.multiplayer.globalChatInfoMessage("LostBook", this.displayName);
		}
		if (this.archaeologyFound.TryGetValue(itemId, out var artifactEntry))
		{
			artifactEntry[0] += number;
			artifactEntry[1] += number;
			this.archaeologyFound[itemId] = artifactEntry;
		}
		else
		{
			if (this.archaeologyFound.Length == 0)
			{
				if (!this.eventsSeen.Contains("0") && itemId != "102")
				{
					this.addQuest("23");
				}
				this.mailReceived.Add("artifactFound");
				shouldHoldUpArtifact = true;
			}
			this.archaeologyFound.Add(itemId, new int[2] { number, number });
		}
		if (shouldHoldUpArtifact)
		{
			this.holdUpItemThenMessage(ItemRegistry.Create("(O)" + itemId));
		}
	}

	public void cookedRecipe(string itemId)
	{
		if (!this.recipesCooked.TryGetValue(itemId, out var curValue))
		{
			curValue = 0;
		}
		this.recipesCooked[itemId] = curValue + 1;
	}

	public bool caughtFish(string itemId, int size, bool from_fish_pond = false, int numberCaught = 1)
	{
		ItemMetadata itemData = ItemRegistry.GetMetadata(itemId);
		itemId = itemData.QualifiedItemId;
		bool num = !from_fish_pond && itemData.Exists() && !ItemContextTagManager.HasBaseTag(itemData.QualifiedItemId, "trash_item") && !(itemId == "(O)167") && (itemData.GetParsedData()?.ObjectType == "Fish" || itemData.QualifiedItemId == "(O)372");
		bool sizeRecord = false;
		if (num)
		{
			if (this.fishCaught.TryGetValue(itemId, out var fishEntry))
			{
				fishEntry[0] += numberCaught;
				Game1.stats.checkForFishingAchievements();
				if (size > this.fishCaught[itemId][1])
				{
					fishEntry[1] = size;
					sizeRecord = true;
				}
				this.fishCaught[itemId] = fishEntry;
			}
			else
			{
				this.fishCaught.Add(itemId, new int[2] { numberCaught, size });
				Game1.stats.checkForFishingAchievements();
				this.autoGenerateActiveDialogueEvent("fishCaught_" + itemData.LocalItemId);
			}
			this.NotifyQuests((Quest quest) => quest.OnFishCaught(itemId, numberCaught, size));
			if (Utility.GetDayOfPassiveFestival("SquidFest") > 0 && itemId == "(O)151")
			{
				Game1.stats.Increment(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year), numberCaught);
			}
		}
		return sizeRecord;
	}

	public virtual void gainExperience(int which, int howMuch)
	{
		if (which == 5 || howMuch <= 0)
		{
			return;
		}
		if (!this.IsLocalPlayer && Game1.IsServer)
		{
			this.queueMessage(17, Game1.player, which, howMuch);
			return;
		}
		if (this.Level >= 25)
		{
			int old = MasteryTrackerMenu.getCurrentMasteryLevel();
			Game1.stats.Increment("MasteryExp", Math.Max(1, (which == 0) ? (howMuch / 2) : howMuch));
			if (MasteryTrackerMenu.getCurrentMasteryLevel() > old)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:Mastery_newlevel"));
				Game1.playSound("newArtifact");
			}
		}
		int newLevel = Farmer.checkForLevelGain(this.experiencePoints[which], this.experiencePoints[which] + howMuch);
		this.experiencePoints[which] += howMuch;
		int oldLevel = -1;
		if (newLevel != -1)
		{
			switch (which)
			{
			case 0:
				oldLevel = this.farmingLevel.Value;
				this.farmingLevel.Value = newLevel;
				break;
			case 3:
				oldLevel = this.miningLevel.Value;
				this.miningLevel.Value = newLevel;
				break;
			case 1:
				oldLevel = this.fishingLevel.Value;
				this.fishingLevel.Value = newLevel;
				break;
			case 2:
				oldLevel = this.foragingLevel.Value;
				this.foragingLevel.Value = newLevel;
				break;
			case 5:
				oldLevel = this.luckLevel.Value;
				this.luckLevel.Value = newLevel;
				break;
			case 4:
				oldLevel = this.combatLevel.Value;
				this.combatLevel.Value = newLevel;
				break;
			}
		}
		if (newLevel <= oldLevel)
		{
			return;
		}
		for (int i = oldLevel + 1; i <= newLevel; i++)
		{
			this.newLevels.Add(new Point(which, i));
			if (this.newLevels.Count == 1)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:NewIdeas"));
			}
		}
	}

	public int getEffectiveSkillLevel(int whichSkill)
	{
		if (whichSkill < 0 || whichSkill > 5)
		{
			return -1;
		}
		int[] effectiveSkillLevels = new int[6]
		{
			this.farmingLevel.Value,
			this.fishingLevel.Value,
			this.foragingLevel.Value,
			this.miningLevel.Value,
			this.combatLevel.Value,
			this.luckLevel.Value
		};
		for (int i = 0; i < this.newLevels.Count; i++)
		{
			effectiveSkillLevels[this.newLevels[i].X]--;
		}
		return effectiveSkillLevels[whichSkill];
	}

	public static int checkForLevelGain(int oldXP, int newXP)
	{
		for (int level = 10; level >= 1; level--)
		{
			if (oldXP < Farmer.getBaseExperienceForLevel(level) && newXP >= Farmer.getBaseExperienceForLevel(level))
			{
				return level;
			}
		}
		return -1;
	}

	public static int getBaseExperienceForLevel(int level)
	{
		return level switch
		{
			1 => 100, 
			2 => 380, 
			3 => 770, 
			4 => 1300, 
			5 => 2150, 
			6 => 3300, 
			7 => 4800, 
			8 => 6900, 
			9 => 10000, 
			10 => 15000, 
			_ => -1, 
		};
	}

	/// <summary>Mark a gift as having been revealed to the player, even if it hasn't yet been gifted.</summary>
	/// <param name="npcName">The name of the NPC.</param>
	/// <param name="itemId">The item ID.</param>
	public void revealGiftTaste(string npcName, string itemId)
	{
		if (npcName != null)
		{
			if (!this.giftedItems.TryGetValue(npcName, out var giftData))
			{
				giftData = (this.giftedItems[npcName] = new SerializableDictionary<string, int>());
			}
			giftData.TryAdd(itemId, 0);
		}
	}

	public void onGiftGiven(NPC npc, Object item)
	{
		if (item.bigCraftable.Value)
		{
			return;
		}
		if (!this.giftedItems.TryGetValue(npc.name.Value, out var giftData))
		{
			giftData = (this.giftedItems[npc.name.Value] = new SerializableDictionary<string, int>());
		}
		int curValue = giftData.GetValueOrDefault(item.ItemId);
		giftData[item.ItemId] = curValue + 1;
		if (this.team.specialOrders == null)
		{
			return;
		}
		foreach (SpecialOrder specialOrder in this.team.specialOrders)
		{
			specialOrder.onGiftGiven?.Invoke(this, npc, item);
		}
	}

	public bool hasGiftTasteBeenRevealed(NPC npc, string itemId)
	{
		if (this.hasItemBeenGifted(npc, itemId))
		{
			return true;
		}
		if (!this.giftedItems.TryGetValue(npc.name.Value, out var giftData))
		{
			return false;
		}
		return giftData.ContainsKey(itemId);
	}

	public bool hasItemBeenGifted(NPC npc, string itemId)
	{
		if (!this.giftedItems.TryGetValue(npc.name.Value, out var giftData))
		{
			return false;
		}
		if (!giftData.TryGetValue(itemId, out var value))
		{
			return false;
		}
		return value > 0;
	}

	public void MarkItemAsTailored(Item item)
	{
		if (item != null)
		{
			string item_key = Utility.getStandardDescriptionFromItem(item, 1);
			if (!this.tailoredItems.TryGetValue(item_key, out var curValue))
			{
				curValue = 0;
			}
			this.tailoredItems[item_key] = curValue + 1;
		}
	}

	public bool HasTailoredThisItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		string item_key = Utility.getStandardDescriptionFromItem(item, 1);
		return this.tailoredItems.ContainsKey(item_key);
	}

	/// <summary>Handle the player finding a mineral object.</summary>
	/// <param name="itemId">The unqualified item ID for an <see cref="F:StardewValley.ItemRegistry.type_object" />-type item.</param>
	public void foundMineral(string itemId)
	{
		if (!this.mineralsFound.TryGetValue(itemId, out var curValue))
		{
			curValue = 0;
		}
		this.mineralsFound[itemId] = curValue + 1;
		if (!this.hasOrWillReceiveMail("artifactFound"))
		{
			this.mailReceived.Add("artifactFound");
		}
	}

	public void increaseBackpackSize(int howMuch)
	{
		this.MaxItems += howMuch;
		while (this.Items.Count < this.MaxItems)
		{
			this.Items.Add(null);
		}
	}

	[Obsolete("Most code should use Items.CountId instead. However this method works a bit differently in that the item ID can be 858 (Qi Gems), 73 (Golden Walnuts), a category number, or -777 to match seasonal wild seeds.")]
	public int getItemCount(string itemId)
	{
		return this.getItemCountInList(this.Items, itemId);
	}

	[Obsolete("Most code should use Items.CountId instead. However this method works a bit differently in that the item ID can be a category number, or -777 to match seasonal wild seeds.")]
	public int getItemCountInList(IList<Item> list, string itemId)
	{
		int number_found = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && CraftingRecipe.ItemMatchesForCrafting(list[i], itemId))
			{
				number_found += list[i].Stack;
			}
		}
		return number_found;
	}

	/// <summary>Cause the player to lose a random number of items based on their luck after dying. These will be added to <see cref="F:StardewValley.Farmer.itemsLostLastDeath" /> so they can recover one of them.</summary>
	/// <param name="random">The RNG to use, or <c>null</c> to create one.</param>
	/// <returns>Returns the number of items lost.</returns>
	public int LoseItemsOnDeath(Random random = null)
	{
		if (random == null)
		{
			random = Utility.CreateDaySaveRandom(Game1.timeOfDay);
		}
		double itemLossRate = 0.22 - (double)this.LuckLevel * 0.04 - this.DailyLuck;
		int numberOfItemsLost = 0;
		this.itemsLostLastDeath.Clear();
		for (int i = this.Items.Count - 1; i >= 0; i--)
		{
			Item item = this.Items[i];
			if (item != null && item.CanBeLostOnDeath() && random.NextBool(itemLossRate))
			{
				numberOfItemsLost++;
				this.Items[i] = null;
				this.itemsLostLastDeath.Add(item);
				if (numberOfItemsLost == 3)
				{
					break;
				}
			}
		}
		return numberOfItemsLost;
	}

	public void ShowSitting()
	{
		if (!this.IsSitting())
		{
			return;
		}
		if (this.sittingFurniture != null)
		{
			this.FacingDirection = this.sittingFurniture.GetSittingDirection();
		}
		if (base.yJumpOffset != 0)
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.FarmerSprite.setCurrentSingleFrame(12, 32000);
				break;
			case 1:
				this.FarmerSprite.setCurrentSingleFrame(6, 32000);
				break;
			case 3:
				this.FarmerSprite.setCurrentSingleFrame(6, 32000, secondaryArm: false, flip: true);
				break;
			case 2:
				this.FarmerSprite.setCurrentSingleFrame(0, 32000);
				break;
			}
			return;
		}
		switch (this.FacingDirection)
		{
		case 0:
			this.FarmerSprite.setCurrentSingleFrame(113, 32000);
			this.xOffset = 0f;
			this.yOffset = -40f;
			break;
		case 1:
			this.FarmerSprite.setCurrentSingleFrame(117, 32000);
			this.xOffset = -4f;
			this.yOffset = -32f;
			break;
		case 3:
			this.FarmerSprite.setCurrentSingleFrame(117, 32000, secondaryArm: false, flip: true);
			this.xOffset = 4f;
			this.yOffset = -32f;
			break;
		case 2:
			this.FarmerSprite.setCurrentSingleFrame(107, 32000, secondaryArm: true);
			this.xOffset = 0f;
			this.yOffset = -48f;
			break;
		}
	}

	public void showRiding()
	{
		if (!this.isRidingHorse())
		{
			return;
		}
		this.xOffset = -6f;
		switch (this.FacingDirection)
		{
		case 0:
			this.FarmerSprite.setCurrentSingleFrame(113, 32000);
			break;
		case 1:
			this.FarmerSprite.setCurrentSingleFrame(106, 32000);
			this.xOffset += 2f;
			break;
		case 3:
			this.FarmerSprite.setCurrentSingleFrame(106, 32000, secondaryArm: false, flip: true);
			this.xOffset = -12f;
			break;
		case 2:
			this.FarmerSprite.setCurrentSingleFrame(107, 32000);
			break;
		}
		if (this.isMoving())
		{
			switch (this.mount.Sprite.currentAnimationIndex)
			{
			case 0:
				this.yOffset = 0f;
				break;
			case 1:
				this.yOffset = -4f;
				break;
			case 2:
				this.yOffset = -4f;
				break;
			case 3:
				this.yOffset = 0f;
				break;
			case 4:
				this.yOffset = 4f;
				break;
			case 5:
				this.yOffset = 4f;
				break;
			}
		}
		else
		{
			this.yOffset = 0f;
		}
	}

	public void showCarrying()
	{
		if (Game1.eventUp || this.isRidingHorse() || Game1.killScreen || this.IsSitting())
		{
			return;
		}
		if (this.bathingClothes.Value || this.onBridge.Value)
		{
			this.showNotCarrying();
			return;
		}
		if (!this.FarmerSprite.PauseForSingleAnimation && !this.isMoving())
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.FarmerSprite.setCurrentFrame(144);
				break;
			case 1:
				this.FarmerSprite.setCurrentFrame(136);
				break;
			case 2:
				this.FarmerSprite.setCurrentFrame(128);
				break;
			case 3:
				this.FarmerSprite.setCurrentFrame(152);
				break;
			}
		}
		if (this.ActiveObject != null)
		{
			this.mostRecentlyGrabbedItem = this.ActiveObject;
		}
		if (this.IsLocalPlayer && this.mostRecentlyGrabbedItem?.QualifiedItemId == "(O)434")
		{
			this.eatHeldObject();
		}
	}

	public void showNotCarrying()
	{
		if (!this.FarmerSprite.PauseForSingleAnimation && !this.isMoving())
		{
			bool canOnlyWalk = this.canOnlyWalk || this.bathingClothes.Value || this.onBridge.Value;
			switch (this.FacingDirection)
			{
			case 0:
				this.FarmerSprite.setCurrentFrame(canOnlyWalk ? 16 : 48, canOnlyWalk ? 1 : 0);
				break;
			case 1:
				this.FarmerSprite.setCurrentFrame(canOnlyWalk ? 8 : 40, canOnlyWalk ? 1 : 0);
				break;
			case 2:
				this.FarmerSprite.setCurrentFrame((!canOnlyWalk) ? 32 : 0, canOnlyWalk ? 1 : 0);
				break;
			case 3:
				this.FarmerSprite.setCurrentFrame(canOnlyWalk ? 24 : 56, canOnlyWalk ? 1 : 0);
				break;
			}
		}
	}

	public int GetDaysMarried()
	{
		return this.GetSpouseFriendship()?.DaysMarried ?? 0;
	}

	public Friendship GetSpouseFriendship()
	{
		long? farmerSpouseId = this.team.GetSpouse(this.UniqueMultiplayerID);
		if (farmerSpouseId.HasValue)
		{
			long spouseID = farmerSpouseId.Value;
			return this.team.GetFriendship(this.UniqueMultiplayerID, spouseID);
		}
		if (string.IsNullOrEmpty(this.spouse) || !this.friendshipData.TryGetValue(this.spouse, out var friendship))
		{
			return null;
		}
		return friendship;
	}

	public bool hasDailyQuest()
	{
		for (int i = this.questLog.Count - 1; i >= 0; i--)
		{
			if (this.questLog[i].dailyQuest.Value)
			{
				return true;
			}
		}
		return false;
	}

	public void showToolUpgradeAvailability()
	{
		int day = Game1.dayOfMonth;
		if (!(this.toolBeingUpgraded != null) || this.daysLeftForToolUpgrade.Value > 0 || this.toolBeingUpgraded.Value == null || Utility.isFestivalDay() || (!(Game1.shortDayNameFromDayOfSeason(day) != "Fri") && this.hasCompletedCommunityCenter() && !Game1.isRaining) || this.hasReceivedToolUpgradeMessageYet)
		{
			return;
		}
		if (Game1.newDay)
		{
			Game1.morningQueue.Enqueue(delegate
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", this.toolBeingUpgraded.Value.DisplayName));
			});
		}
		else
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", this.toolBeingUpgraded.Value.DisplayName));
		}
		this.hasReceivedToolUpgradeMessageYet = true;
	}

	public void dayupdate(int timeWentToSleep)
	{
		if (this.IsSitting())
		{
			this.StopSitting(animate: false);
		}
		this.resetFriendshipsForNewDay();
		this.LearnDefaultRecipes();
		this.hasUsedDailyRevive.Value = false;
		this.hasBeenBlessedByStatueToday = false;
		this.acceptedDailyQuest.Set(newValue: false);
		this.dancePartner.Value = null;
		this.festivalScore = 0;
		this.forceTimePass = false;
		if (this.daysLeftForToolUpgrade.Value > 0)
		{
			this.daysLeftForToolUpgrade.Value--;
		}
		if (this.daysUntilHouseUpgrade.Value > 0)
		{
			this.daysUntilHouseUpgrade.Value--;
			if (this.daysUntilHouseUpgrade.Value <= 0)
			{
				FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(this);
				homeOfFarmer.moveObjectsForHouseUpgrade(this.houseUpgradeLevel.Value + 1);
				this.houseUpgradeLevel.Value++;
				this.daysUntilHouseUpgrade.Value = -1;
				homeOfFarmer.setMapForUpgradeLevel(this.houseUpgradeLevel.Value);
				Game1.stats.checkForBuildingUpgradeAchievements();
				this.autoGenerateActiveDialogueEvent("houseUpgrade_" + this.houseUpgradeLevel.Value);
			}
		}
		this.questLog.RemoveWhere(delegate(Quest quest)
		{
			if (quest.IsTimedQuest())
			{
				quest.daysLeft.Value--;
				if (quest.daysLeft.Value <= 0)
				{
					return !quest.completed.Value;
				}
				return false;
			}
			return false;
		});
		this.ClearBuffs();
		if (this.MaxStamina >= 508)
		{
			this.mailReceived.Add("gotMaxStamina");
		}
		float oldStamina = this.Stamina;
		this.Stamina = this.MaxStamina;
		bool wasExhausted = this.exhausted.Value;
		if (wasExhausted)
		{
			this.exhausted.Value = false;
			this.Stamina = this.MaxStamina / 2 + 1;
		}
		int bedTime = ((this.timeWentToBed.Value == 0) ? timeWentToSleep : this.timeWentToBed.Value);
		if (bedTime > 2400)
		{
			float staminaRestorationReduction = (1f - (float)(2600 - Math.Min(2600, bedTime)) / 200f) * (float)(this.MaxStamina / 2);
			this.Stamina -= staminaRestorationReduction;
			if (timeWentToSleep > 2700)
			{
				this.Stamina /= 2f;
			}
		}
		if (timeWentToSleep < 2700 && oldStamina > this.Stamina && !wasExhausted)
		{
			this.Stamina = oldStamina;
		}
		this.health = this.maxHealth;
		this.activeDialogueEvents.RemoveWhere(delegate(KeyValuePair<string, int> pair)
		{
			string key = pair.Key;
			if (!key.Contains("_memory_"))
			{
				this.previousActiveDialogueEvents.TryAdd(key, 0);
			}
			this.activeDialogueEvents[key]--;
			if (this.activeDialogueEvents[key] < 0)
			{
				if (!(key == "pennyRedecorating") || Utility.getHomeOfFarmer(this).GetSpouseBed() != null)
				{
					return true;
				}
				this.activeDialogueEvents[key] = 0;
			}
			return false;
		});
		foreach (string previousEvent in this.previousActiveDialogueEvents.Keys)
		{
			this.previousActiveDialogueEvents[previousEvent]++;
			if (this.previousActiveDialogueEvents[previousEvent] == 1)
			{
				this.activeDialogueEvents[previousEvent + "_memory_oneday"] = 4;
			}
			if (this.previousActiveDialogueEvents[previousEvent] == 7)
			{
				this.activeDialogueEvents[previousEvent + "_memory_oneweek"] = 4;
			}
			if (this.previousActiveDialogueEvents[previousEvent] == 14)
			{
				this.activeDialogueEvents[previousEvent + "_memory_twoweeks"] = 4;
			}
			if (this.previousActiveDialogueEvents[previousEvent] == 28)
			{
				this.activeDialogueEvents[previousEvent + "_memory_fourweeks"] = 4;
			}
			if (this.previousActiveDialogueEvents[previousEvent] == 56)
			{
				this.activeDialogueEvents[previousEvent + "_memory_eightweeks"] = 4;
			}
			if (this.previousActiveDialogueEvents[previousEvent] == 104)
			{
				this.activeDialogueEvents[previousEvent + "_memory_oneyear"] = 4;
			}
		}
		this.hasMoved = false;
		if (Game1.random.NextDouble() < 0.905 && !this.hasOrWillReceiveMail("RarecrowSociety") && Utility.doesItemExistAnywhere("(BC)136") && Utility.doesItemExistAnywhere("(BC)137") && Utility.doesItemExistAnywhere("(BC)138") && Utility.doesItemExistAnywhere("(BC)139") && Utility.doesItemExistAnywhere("(BC)140") && Utility.doesItemExistAnywhere("(BC)126") && Utility.doesItemExistAnywhere("(BC)110") && Utility.doesItemExistAnywhere("(BC)113"))
		{
			this.mailbox.Add("RarecrowSociety");
		}
		this.timeWentToBed.Value = 0;
		this.stats.Set("blessingOfWaters", 0);
		if (this.shirtItem.Value == null || this.pantsItem.Value == null || (!(base.currentLocation is FarmHouse) && !(base.currentLocation is IslandFarmHouse) && !(base.currentLocation is Shed)))
		{
			return;
		}
		foreach (Object value in base.currentLocation.netObjects.Values)
		{
			if (value is Mannequin mannequin && mannequin.GetMannequinData().Cursed && Game1.random.NextDouble() < 0.005 && !mannequin.swappedWithFarmerTonight.Value)
			{
				mannequin.hat.Value = this.Equip(mannequin.hat.Value, this.hat);
				mannequin.shirt.Value = this.Equip(mannequin.shirt.Value, this.shirtItem);
				mannequin.pants.Value = this.Equip(mannequin.pants.Value, this.pantsItem);
				mannequin.boots.Value = this.Equip(mannequin.boots.Value, this.boots);
				mannequin.swappedWithFarmerTonight.Value = true;
				base.currentLocation.playSound("cursed_mannequin");
				mannequin.eyeTimer = 1000;
			}
		}
	}

	public bool hasSeenActiveDialogueEvent(string eventName)
	{
		if (!this.activeDialogueEvents.ContainsKey(eventName))
		{
			return this.previousActiveDialogueEvents.ContainsKey(eventName);
		}
		return true;
	}

	public bool autoGenerateActiveDialogueEvent(string eventName, int duration = 4)
	{
		if (!this.hasSeenActiveDialogueEvent(eventName))
		{
			this.activeDialogueEvents[eventName] = duration;
			return true;
		}
		return false;
	}

	public void removeDatingActiveDialogueEvents(string npcName)
	{
		this.activeDialogueEvents.Remove("dating_" + npcName);
		this.removeActiveDialogMemoryEvents("dating_" + npcName);
		this.previousActiveDialogueEvents.Remove("dating_" + npcName);
	}

	public void removeMarriageActiveDialogueEvents(string npcName)
	{
		this.activeDialogueEvents.Remove("married_" + npcName);
		this.removeActiveDialogMemoryEvents("married_" + npcName);
		this.previousActiveDialogueEvents.Remove("married_" + npcName);
	}

	public void removeActiveDialogMemoryEvents(string activeDialogKey)
	{
		this.activeDialogueEvents.Remove(activeDialogKey + "_memory_oneday");
		this.activeDialogueEvents.Remove(activeDialogKey + "_memory_oneweek");
		this.activeDialogueEvents.Remove(activeDialogKey + "_memory_twoweeks");
		this.activeDialogueEvents.Remove(activeDialogKey + "_memory_fourweeks");
		this.activeDialogueEvents.Remove(activeDialogKey + "_memory_eightweeks");
		this.activeDialogueEvents.Remove(activeDialogKey + "_memory_oneyear");
	}

	public void doDivorce()
	{
		this.divorceTonight.Value = false;
		if (!this.isMarriedOrRoommates())
		{
			return;
		}
		NPC currentSpouse = null;
		if (this.spouse != null)
		{
			currentSpouse = this.getSpouse();
			if (currentSpouse != null)
			{
				this.removeMarriageActiveDialogueEvents(currentSpouse.Name);
				if (!currentSpouse.isRoommate())
				{
					this.autoGenerateActiveDialogueEvent("divorced_" + currentSpouse.Name);
				}
				this.spouse = null;
				this.specialItems.RemoveWhere((string id) => id == "460");
				if (this.friendshipData.TryGetValue(currentSpouse.name.Value, out var friendship))
				{
					friendship.Points = 0;
					friendship.RoommateMarriage = false;
					friendship.Status = FriendshipStatus.Divorced;
				}
				Utility.getHomeOfFarmer(this).showSpouseRoom();
				Game1.getFarm().UpdatePatio();
				this.removeQuest("126");
			}
		}
		else if (this.team.GetSpouse(this.UniqueMultiplayerID).HasValue)
		{
			long spouseID = this.team.GetSpouse(this.UniqueMultiplayerID).Value;
			Friendship friendship2 = this.team.GetFriendship(this.UniqueMultiplayerID, spouseID);
			friendship2.Points = 0;
			friendship2.RoommateMarriage = false;
			friendship2.Status = FriendshipStatus.Divorced;
		}
		if (((!(currentSpouse?.isRoommate())) ?? true) && !this.autoGenerateActiveDialogueEvent("divorced_once"))
		{
			this.autoGenerateActiveDialogueEvent("divorced_twice");
		}
	}

	public static void showReceiveNewItemMessage(Farmer who, Item item, int countAdded)
	{
		string message = item.checkForSpecialItemHoldUpMeessage();
		if (message == null)
		{
			message = ((item.TryGetTempData<bool>("FromStarterGiftBox", out var fromGiftbox) && fromGiftbox && item.QualifiedItemId == "(O)472" && countAdded == 15) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1918") : ((!item.HasContextTag("book_item")) ? ((countAdded > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1922", countAdded, item.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", item.DisplayName, Lexicon.getProperArticleForWord(item.DisplayName))) : Game1.content.LoadString("Strings\\1_6_Strings:FoundABook", item.DisplayName)));
		}
		Game1.drawObjectDialogue(new List<string> { message });
		who.completelyStopAnimatingOrDoingAction();
	}

	public static void showEatingItem(Farmer who)
	{
		TemporaryAnimatedSprite tempSprite = null;
		if (who.itemToEat == null)
		{
			return;
		}
		TemporaryAnimatedSprite coloredTempSprite = null;
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(who.itemToEat.QualifiedItemId);
		string textureName = dataOrErrorItem.TextureName;
		Microsoft.Xna.Framework.Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		Color color = Color.White;
		Color coloredObjectColor = Color.White;
		if (who.tempFoodItemTextureName.Value != null)
		{
			textureName = who.tempFoodItemTextureName.Value;
			sourceRect = who.tempFoodItemSourceRect.Value;
		}
		else if ((who.itemToEat as Object)?.preservedParentSheetIndex.Value != null)
		{
			if (who.itemToEat.ItemId.Equals("SmokedFish"))
			{
				ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem("(O)" + (who.itemToEat as Object).preservedParentSheetIndex.Value);
				textureName = dataOrErrorItem2.TextureName;
				sourceRect = dataOrErrorItem2.GetSourceRect();
				color = new Color(130, 100, 83);
			}
			else if (who.itemToEat is ColoredObject coloredO)
			{
				coloredObjectColor = coloredO.color.Value;
			}
		}
		switch (who.FarmerSprite.currentAnimationIndex)
		{
		case 1:
			if (who.IsLocalPlayer && who.itemToEat.QualifiedItemId == "(O)434")
			{
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 62.75f, 8, 2, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			}
			tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 254f, 1, 0, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, 0f, 0f, 0f);
			if (!coloredObjectColor.Equals(Color.White))
			{
				sourceRect.X += sourceRect.Width;
				coloredTempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 254f, 1, 0, who.Position + new Vector2(-21f, -112f), flicker: false, flipped: false, (float)(who.StandingPixel.Y + 1) / 10000f + 0.01f, 0f, coloredObjectColor, 4f, 0f, 0f, 0f);
			}
			break;
		case 2:
			if (who.IsLocalPlayer && who.itemToEat.QualifiedItemId == "(O)434")
			{
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 81.25f, 8, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, -0.01f, 0f, 0f)
				{
					motion = new Vector2(0.8f, -11f),
					acceleration = new Vector2(0f, 0.5f)
				};
				break;
			}
			if (Game1.currentLocation == who.currentLocation)
			{
				Game1.playSound("dwop");
			}
			tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 650f, 1, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, -0.01f, 0f, 0f)
			{
				motion = new Vector2(0.8f, -11f),
				acceleration = new Vector2(0f, 0.5f)
			};
			if (!coloredObjectColor.Equals(Color.White))
			{
				sourceRect.X += sourceRect.Width;
				coloredTempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 650f, 1, 0, who.Position + new Vector2(-21f, -108f), flicker: false, flipped: false, (float)(who.StandingPixel.Y + 1) / 10000f + 0.01f, 0f, coloredObjectColor, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0.8f, -11f),
					acceleration = new Vector2(0f, 0.5f)
				};
			}
			break;
		case 3:
			who.yJumpVelocity = 6f;
			who.yJumpOffset = 1;
			break;
		case 4:
		{
			if (Game1.currentLocation == who.currentLocation && who.ShouldHandleAnimationSound())
			{
				Game1.playSound("eat");
			}
			for (int i = 0; i < 8; i++)
			{
				int size = Game1.random.Next(2, 4);
				Microsoft.Xna.Framework.Rectangle r = sourceRect.Clone();
				r.X += 8;
				r.Y += 8;
				r.Width = size;
				r.Height = size;
				tempSprite = new TemporaryAnimatedSprite(textureName, r, 400f, 1, 0, who.Position + new Vector2(24f, -48f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, color, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-6, -3)),
					acceleration = new Vector2(0f, 0.5f)
				};
				who.currentLocation.temporarySprites.Add(tempSprite);
			}
			return;
		}
		default:
			who.freezePause = 0;
			break;
		}
		if (tempSprite != null)
		{
			who.currentLocation.temporarySprites.Add(tempSprite);
		}
		if (coloredTempSprite != null)
		{
			who.currentLocation.temporarySprites.Add(coloredTempSprite);
		}
	}

	public static void eatItem(Farmer who)
	{
	}

	/// <summary>Get whether the player has a buff applied.</summary>
	/// <param name="id">The buff ID, like <see cref="F:StardewValley.Buff.tipsy" />.</param>
	public bool hasBuff(string id)
	{
		return this.buffs.IsApplied(id);
	}

	/// <summary>Add a buff to the player, or refresh it if it's already applied.</summary>
	/// <param name="id">The buff ID, like <see cref="F:StardewValley.Buff.tipsy" />.</param>
	public void applyBuff(string id)
	{
		this.buffs.Apply(new Buff(id));
	}

	/// <summary>Add a buff to the player, or refresh it if it's already applied.</summary>
	/// <param name="id">The buff to apply.</param>
	public void applyBuff(Buff buff)
	{
		this.buffs.Apply(buff);
	}

	/// <summary>Get whether the player has a buff with an ID containing the given string.</summary>
	/// <param name="idSubstring">The substring to match in the buff ID.</param>
	public bool hasBuffWithNameContainingString(string idSubstr)
	{
		return this.buffs.HasBuffWithNameContaining(idSubstr);
	}

	public bool hasOrWillReceiveMail(string id)
	{
		if (!this.mailReceived.Contains(id) && !this.mailForTomorrow.Contains(id) && !this.mailbox.Contains(id))
		{
			return this.mailForTomorrow.Contains(id + "%&NL&%");
		}
		return true;
	}

	public static void showHoldingItem(Farmer who, Item item)
	{
		if (!(item is SpecialItem specialItem))
		{
			if (!(item is Slingshot) && !(item is MeleeWeapon) && !(item is Boots))
			{
				if (!(item is Hat))
				{
					if (!(item is Furniture))
					{
						if (!(item is Object) && !(item is Tool))
						{
							if (!(item is Ring))
							{
								if (item == null)
								{
									Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(420, 489, 25, 18), 2500f, 1, 0, who.Position + new Vector2(-20f, -152f), flicker: false, flipped: false)
									{
										motion = new Vector2(0f, -0.1f),
										scale = 4f,
										layerDepth = 1f
									});
								}
								else
								{
									TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false)
									{
										motion = new Vector2(0f, -0.1f),
										layerDepth = 1f
									};
									sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
									Game1.currentLocation.temporarySprites.Add(sprite);
								}
							}
							else
							{
								TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(-4f, -124f), flicker: false, flipped: false)
								{
									motion = new Vector2(0f, -0.1f),
									layerDepth = 1f
								};
								sprite2.CopyAppearanceFromItemId(item.QualifiedItemId);
								Game1.currentLocation.temporarySprites.Add(sprite2);
							}
						}
						else if (item is Object obj && obj.bigCraftable.Value)
						{
							TemporaryAnimatedSprite sprite3 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -188f), flicker: false, flipped: false)
							{
								motion = new Vector2(0f, -0.1f),
								layerDepth = 1f
							};
							sprite3.CopyAppearanceFromItemId(item.QualifiedItemId);
							Game1.currentLocation.temporarySprites.Add(sprite3);
						}
						else
						{
							TemporaryAnimatedSprite sprite4 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false)
							{
								motion = new Vector2(0f, -0.1f),
								layerDepth = 1f
							};
							sprite4.CopyAppearanceFromItemId(item.QualifiedItemId);
							Game1.currentLocation.temporarySprites.Add(sprite4);
							if (who.IsLocalPlayer && item.QualifiedItemId == "(O)434")
							{
								who.eatHeldObject();
							}
						}
					}
					else
					{
						TemporaryAnimatedSprite sprite5 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, Vector2.Zero, flicker: false, flipped: false)
						{
							motion = new Vector2(0f, -0.1f),
							layerDepth = 1f
						};
						sprite5.CopyAppearanceFromItemId(item.QualifiedItemId);
						sprite5.initialPosition = (sprite5.position = who.Position + new Vector2(32 - sprite5.sourceRect.Width / 2 * 4, -188f));
						Game1.currentLocation.temporarySprites.Add(sprite5);
					}
				}
				else
				{
					TemporaryAnimatedSprite sprite6 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(-8f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -0.1f)
					};
					sprite6.CopyAppearanceFromItemId(item.QualifiedItemId);
					Game1.currentLocation.temporarySprites.Add(sprite6);
				}
			}
			else
			{
				TemporaryAnimatedSprite sprite7 = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), 2500f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.1f)
				};
				sprite7.CopyAppearanceFromItemId(item.QualifiedItemId);
				Game1.currentLocation.temporarySprites.Add(sprite7);
			}
		}
		else
		{
			TemporaryAnimatedSprite t = specialItem.getTemporarySpriteForHoldingUp(who.Position + new Vector2(0f, -124f));
			t.motion = new Vector2(0f, -0.1f);
			t.scale = 4f;
			t.interval = 2500f;
			t.totalNumberOfLoops = 0;
			t.animationLength = 1;
			Game1.currentLocation.temporarySprites.Add(t);
		}
	}

	public void holdUpItemThenMessage(Item item, bool showMessage = true)
	{
		this.holdUpItemThenMessage(item, item?.Stack ?? 1, showMessage);
	}

	public void holdUpItemThenMessage(Item item, int countAdded, bool showMessage = true)
	{
		this.completelyStopAnimatingOrDoingAction();
		if (showMessage)
		{
			Game1.MusicDuckTimer = 2000f;
			DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750);
		}
		this.faceDirection(2);
		this.freezePause = 4000;
		this.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[3]
		{
			new FarmerSprite.AnimationFrame(57, 0),
			new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, delegate(Farmer who)
			{
				Farmer.showHoldingItem(who, item);
			}),
			showMessage ? new FarmerSprite.AnimationFrame((short)this.FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false, delegate(Farmer who)
			{
				Farmer.showReceiveNewItemMessage(who, item, countAdded);
			}, behaviorAtEndOfFrame: true) : new FarmerSprite.AnimationFrame((short)this.FarmerSprite.CurrentFrame, 500, secondaryArm: false, flip: false)
		});
		this.mostRecentlyGrabbedItem = item;
		this.canMove = false;
	}

	public void resetState()
	{
		this.mount = null;
		this.ClearBuffs();
		this.TemporaryItem = null;
		base.swimming.Value = false;
		this.bathingClothes.Value = false;
		this.ignoreCollisions = false;
		this.resetItemStates();
		this.fireToolEvent.Clear();
		this.beginUsingToolEvent.Clear();
		this.endUsingToolEvent.Clear();
		this.sickAnimationEvent.Clear();
		this.passOutEvent.Clear();
		this.drinkAnimationEvent.Clear();
		this.eatAnimationEvent.Clear();
	}

	public void resetItemStates()
	{
		for (int i = 0; i < this.Items.Count; i++)
		{
			this.Items[i]?.resetState();
		}
	}

	public void clearBackpack()
	{
		for (int i = 0; i < this.Items.Count; i++)
		{
			this.Items[i] = null;
		}
	}

	public void resetFriendshipsForNewDay()
	{
		foreach (string name in this.friendshipData.Keys)
		{
			bool single = false;
			NPC n = Game1.getCharacterFromName(name);
			if (n == null)
			{
				n = Game1.getCharacterFromName<Child>(name, mustBeVillager: false);
			}
			if (n != null)
			{
				if (n != null && n.datable.Value && !this.friendshipData[name].IsDating() && !n.isMarried())
				{
					single = true;
				}
				if (this.spouse != null && name == this.spouse && !this.hasPlayerTalkedToNPC(name))
				{
					this.changeFriendship(-20, n);
				}
				else if (n != null && this.friendshipData[name].IsDating() && !this.hasPlayerTalkedToNPC(name) && this.friendshipData[name].Points < 2500)
				{
					this.changeFriendship(-8, n);
				}
				if (this.hasPlayerTalkedToNPC(name))
				{
					this.friendshipData[name].TalkedToToday = false;
				}
				else if ((!single && this.friendshipData[name].Points < 2500) || (single && this.friendshipData[name].Points < 2000))
				{
					this.changeFriendship(-2, n);
				}
			}
		}
		this.updateFriendshipGifts(Game1.Date);
	}

	public virtual int GetAppliedMagneticRadius()
	{
		return Math.Max(128, this.MagneticRadius);
	}

	public void updateFriendshipGifts(WorldDate date)
	{
		foreach (string name in this.friendshipData.Keys)
		{
			if (date.TotalDays != this.friendshipData[name].LastGiftDate?.TotalDays)
			{
				this.friendshipData[name].GiftsToday = 0;
			}
			if (date.TotalSundayWeeks != this.friendshipData[name].LastGiftDate?.TotalSundayWeeks)
			{
				if (this.friendshipData[name].GiftsThisWeek >= 2)
				{
					this.changeFriendship(10, Game1.getCharacterFromName(name));
				}
				this.friendshipData[name].GiftsThisWeek = 0;
			}
		}
	}

	public bool hasPlayerTalkedToNPC(string name)
	{
		if (!this.friendshipData.TryGetValue(name, out var friendship) && NPC.CanSocializePerData(name, base.currentLocation))
		{
			friendship = (this.friendshipData[name] = new Friendship());
		}
		return friendship?.TalkedToToday ?? false;
	}

	public void fuelLantern(int units)
	{
		Tool lantern = this.getToolFromName("Lantern");
		if (lantern != null)
		{
			((Lantern)lantern).fuelLeft = Math.Min(100, ((Lantern)lantern).fuelLeft + units);
		}
	}

	public bool IsEquippedItem(Item item)
	{
		if (item != null)
		{
			foreach (Item equippedItem in this.GetEquippedItems())
			{
				if (equippedItem == item)
				{
					return true;
				}
			}
		}
		return false;
	}

	public IEnumerable<Item> GetEquippedItems()
	{
		return new Item[7]
		{
			this.CurrentTool,
			this.hat.Value,
			this.shirtItem.Value,
			this.pantsItem.Value,
			this.boots.Value,
			this.leftRing.Value,
			this.rightRing.Value
		}.Where((Item item) => item != null);
	}

	public override bool collideWith(Object o)
	{
		base.collideWith(o);
		if (this.isRidingHorse() && o is Fence)
		{
			this.mount.squeezeForGate();
			switch (this.FacingDirection)
			{
			case 3:
				if (o.tileLocation.X > base.Tile.X)
				{
					return false;
				}
				break;
			case 1:
				if (o.tileLocation.X < base.Tile.X)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	public void changeIntoSwimsuit()
	{
		this.bathingClothes.Value = true;
		this.Halt();
		this.setRunning(isRunning: false);
		this.canOnlyWalk = true;
	}

	public void changeOutOfSwimSuit()
	{
		this.bathingClothes.Value = false;
		this.canOnlyWalk = false;
		this.Halt();
		this.FarmerSprite.StopAnimation();
		if (Game1.options.autoRun)
		{
			this.setRunning(isRunning: true);
		}
	}

	public void showFrame(int frame, bool flip = false)
	{
		FarmerSprite.AnimationFrame[] animationFrames = new FarmerSprite.AnimationFrame[1]
		{
			new FarmerSprite.AnimationFrame(Convert.ToInt32(frame), 100, secondaryArm: false, flip)
		};
		this.FarmerSprite.setCurrentAnimation(animationFrames);
		this.FarmerSprite.loop = true;
		this.FarmerSprite.PauseForSingleAnimation = true;
		this.Sprite.currentFrame = Convert.ToInt32(frame);
	}

	public void stopShowingFrame()
	{
		this.FarmerSprite.loop = false;
		this.FarmerSprite.PauseForSingleAnimation = false;
		this.completelyStopAnimatingOrDoingAction();
	}

	/// <summary>Add an item to the player's inventory if there's room for it.</summary>
	/// <param name="item">The item to add.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public Item addItemToInventory(Item item)
	{
		return this.addItemToInventory(item, null);
	}

	/// <summary>Add an item to the player's inventory if there's room for it.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="affected_items_list">A list to update with the inventory item stacks it was merged into, or <c>null</c> to ignore it.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public Item addItemToInventory(Item item, List<Item> affected_items_list)
	{
		if (item == null)
		{
			return null;
		}
		this.GetItemReceiveBehavior(item, out var needsInventorySpace, out var _);
		if (!needsInventorySpace)
		{
			this.OnItemReceived(item, item.Stack, null);
			return null;
		}
		int originalStack = item.Stack;
		int stackLeft = originalStack;
		foreach (Item slot in this.Items)
		{
			if (!item.canStackWith(slot))
			{
				continue;
			}
			int stack = item.Stack;
			stackLeft = slot.addToStack(item);
			int added = stack - stackLeft;
			if (added > 0)
			{
				item.Stack = stackLeft;
				this.OnItemReceived(item, added, slot, hideHudNotification: true);
				affected_items_list?.Add(slot);
				if (stackLeft < 1)
				{
					break;
				}
			}
		}
		if (stackLeft > 0)
		{
			for (int i = 0; i < this.maxItems.Value && i < this.Items.Count; i++)
			{
				if (this.Items[i] == null)
				{
					item.onDetachedFromParent();
					this.Items[i] = item;
					stackLeft = 0;
					this.OnItemReceived(item, item.Stack, null, hideHudNotification: true);
					affected_items_list?.Add(this.Items[i]);
					break;
				}
			}
		}
		if (originalStack > stackLeft)
		{
			this.ShowItemReceivedHudMessageIfNeeded(item, originalStack - stackLeft);
		}
		if (stackLeft <= 0)
		{
			return null;
		}
		return item;
	}

	/// <summary>Add an item to the player's inventory at a specific index position. If there's already an item at that position, the stacks are merged (if possible) else they're swapped.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="position">The index position within the list at which to add the item.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. If it replaced an item stack previously at that position, returns the replaced item stack. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public Item addItemToInventory(Item item, int position)
	{
		if (item == null)
		{
			return null;
		}
		this.GetItemReceiveBehavior(item, out var needsInventorySpace, out var _);
		if (!needsInventorySpace)
		{
			this.OnItemReceived(item, item.Stack, null);
			return null;
		}
		if (position >= 0 && position < this.Items.Count)
		{
			if (this.Items[position] == null)
			{
				this.Items[position] = item;
				this.OnItemReceived(item, item.Stack, null);
				return null;
			}
			if (!this.Items[position].canStackWith(item))
			{
				Item result = this.Items[position];
				this.Items[position] = item;
				this.OnItemReceived(item, item.Stack, null);
				return result;
			}
			int stack = item.Stack;
			int stackLeft = this.Items[position].addToStack(item);
			int added = stack - stackLeft;
			if (added > 0)
			{
				item.Stack = stackLeft;
				this.OnItemReceived(item, added, this.Items[position]);
				if (stackLeft <= 0)
				{
					return null;
				}
				return item;
			}
		}
		return item;
	}

	/// <summary>Add an item to the player's inventory if there's room for it.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="makeActiveObject">Legacy option which may behave in unexpected ways; shouldn't be used by most code.</param>
	/// <returns>Returns whether the item was at least partially added to the inventory. The number of items added will be deducted from the <paramref name="item" />'s <see cref="P:StardewValley.Item.Stack" />.</returns>
	public bool addItemToInventoryBool(Item item, bool makeActiveObject = false)
	{
		if (item == null)
		{
			return false;
		}
		if (this.IsLocalPlayer)
		{
			Item remainder = null;
			this.GetItemReceiveBehavior(item, out var needsInventorySpace, out var _);
			if (needsInventorySpace)
			{
				remainder = this.addItemToInventory(item);
			}
			else
			{
				this.OnItemReceived(item, item.Stack, null);
			}
			bool success = remainder?.Stack != item.Stack || item is SpecialItem;
			if (makeActiveObject && success && !(item is SpecialItem) && remainder != null && item.Stack <= 1)
			{
				int newItemPosition = this.getIndexOfInventoryItem(item);
				if (newItemPosition > -1)
				{
					Item i = this.Items[this.currentToolIndex.Value];
					this.Items[this.currentToolIndex.Value] = this.Items[newItemPosition];
					this.Items[newItemPosition] = i;
				}
			}
			return success;
		}
		return false;
	}

	/// <summary>Add an item to the player's inventory if there's room for it, then show an animation of the player holding up the item above their head. If the item can't be fully added to the player's inventory, show (or queue) an item-grab menu to let the player collect the remainder.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="itemSelectedCallback">The callback to invoke when the item is added to the player's inventory.</param>
	/// <param name="forceQueue">For any remainder that can't be added to the inventory directly, whether to add the item-grab menu to <see cref="F:StardewValley.Game1.nextClickableMenu" /> even if there's no active menu currently open.</param>
	public void addItemByMenuIfNecessaryElseHoldUp(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
	{
		int countAdded = item.Stack;
		this.mostRecentlyGrabbedItem = item;
		this.addItemsByMenuIfNecessary(new List<Item> { item }, itemSelectedCallback, forceQueue);
		if (Game1.activeClickableMenu == null && item?.QualifiedItemId != "(O)434")
		{
			this.holdUpItemThenMessage(item, countAdded);
		}
	}

	/// <summary>Add an item to the player's inventory if there's room for it. If the item can't be fully added to the player's inventory, show (or queue) an item-grab menu to let the player collect the remainder.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="itemSelectedCallback">The callback to invoke when the item is added to the player's inventory.</param>
	/// <param name="forceQueue">For any remainder that can't be added to the inventory directly, whether to add the item-grab menu to <see cref="F:StardewValley.Game1.nextClickableMenu" /> even if there's no active menu currently open.</param>
	public void addItemByMenuIfNecessary(Item item, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
	{
		this.addItemsByMenuIfNecessary(new List<Item> { item }, itemSelectedCallback, forceQueue);
	}

	/// <summary>Add items to the player's inventory if there's room for them. If the items can't be fully added to the player's inventory, show (or queue) an item-grab menu to let the player collect the remainder.</summary>
	/// <param name="itemsToAdd">The items to add.</param>
	/// <param name="itemSelectedCallback">The callback to invoke when an item is added to the player's inventory.</param>
	/// <param name="forceQueue">For any items that can't be added to the inventory directly, whether to add the item-grab menu to <see cref="F:StardewValley.Game1.nextClickableMenu" /> even if there's no active menu currently open.</param>
	public void addItemsByMenuIfNecessary(List<Item> itemsToAdd, ItemGrabMenu.behaviorOnItemSelect itemSelectedCallback = null, bool forceQueue = false)
	{
		if (itemsToAdd == null || !this.IsLocalPlayer)
		{
			return;
		}
		if (itemsToAdd.Count > 0 && itemsToAdd[0]?.QualifiedItemId == "(O)434")
		{
			if (Game1.activeClickableMenu == null && !forceQueue)
			{
				this.eatObject(itemsToAdd[0] as Object, overrideFullness: true);
			}
			else
			{
				Game1.nextClickableMenu.Add(ItemGrabMenu.CreateOverflowMenu(itemsToAdd));
			}
			return;
		}
		for (int i = itemsToAdd.Count - 1; i >= 0; i--)
		{
			if (this.addItemToInventoryBool(itemsToAdd[i]))
			{
				itemSelectedCallback?.Invoke(itemsToAdd[i], this);
				itemsToAdd.Remove(itemsToAdd[i]);
			}
		}
		if (itemsToAdd.Count > 0 && (forceQueue || Game1.activeClickableMenu != null))
		{
			for (int menuIndex = 0; menuIndex < Game1.nextClickableMenu.Count; menuIndex++)
			{
				if (Game1.nextClickableMenu[menuIndex] is ItemGrabMenu { source: 4 } menu)
				{
					IList<Item> inventory = menu.ItemsToGrabMenu.actualInventory;
					int capacity = menu.ItemsToGrabMenu.capacity;
					bool anyAdded = false;
					for (int j = 0; j < itemsToAdd.Count; j++)
					{
						Item item = itemsToAdd[j];
						int stack = item.Stack;
						item = (itemsToAdd[j] = Utility.addItemToThisInventoryList(item, inventory, capacity));
						if (stack != item?.Stack)
						{
							anyAdded = true;
							if (item == null)
							{
								itemsToAdd.RemoveAt(j);
								j--;
							}
						}
					}
					if (anyAdded)
					{
						Game1.nextClickableMenu[menuIndex] = ItemGrabMenu.CreateOverflowMenu(inventory);
					}
				}
				if (itemsToAdd.Count == 0)
				{
					break;
				}
			}
		}
		if (itemsToAdd.Count > 0)
		{
			ItemGrabMenu itemGrabMenu = ItemGrabMenu.CreateOverflowMenu(itemsToAdd);
			if (forceQueue || Game1.activeClickableMenu != null)
			{
				Game1.nextClickableMenu.Add(itemGrabMenu);
			}
			else
			{
				Game1.activeClickableMenu = itemGrabMenu;
			}
		}
	}

	public virtual void BeginSitting(ISittable furniture)
	{
		if (furniture == null || this.bathingClothes.Value || base.swimming.Value || this.isRidingHorse() || !this.CanMove || this.UsingTool || base.IsEmoting)
		{
			return;
		}
		Vector2? sitting_position = furniture.AddSittingFarmer(this);
		if (!sitting_position.HasValue)
		{
			return;
		}
		base.playNearbySoundAll("woodyStep");
		this.Halt();
		this.synchronizedJump(4f);
		this.FarmerSprite.StopAnimation();
		this.sittingFurniture = furniture;
		this.mapChairSitPosition.Value = new Vector2(-1f, -1f);
		if (this.sittingFurniture is MapSeat)
		{
			Vector2? seat_position = this.sittingFurniture.GetSittingPosition(this, ignore_offsets: true);
			if (seat_position.HasValue)
			{
				this.mapChairSitPosition.Value = seat_position.Value;
			}
		}
		this.isSitting.Value = true;
		this.LerpPosition(base.Position, new Vector2(sitting_position.Value.X * 64f, sitting_position.Value.Y * 64f), 0.15f);
		this.freezePause += 100;
	}

	public virtual void LerpPosition(Vector2 start_position, Vector2 end_position, float duration)
	{
		this.freezePause = (int)(duration * 1000f);
		this.lerpStartPosition = start_position;
		this.lerpEndPosition = end_position;
		this.lerpPosition = 0f;
		this.lerpDuration = duration;
	}

	public virtual void StopSitting(bool animate = true)
	{
		if (this.sittingFurniture == null)
		{
			return;
		}
		ISittable furniture = this.sittingFurniture;
		if (!animate)
		{
			this.mapChairSitPosition.Value = new Vector2(-1f, -1f);
			furniture.RemoveSittingFarmer(this);
		}
		bool furniture_is_in_this_location = false;
		bool location_found = false;
		Vector2 old_position = base.Position;
		if (furniture.IsSeatHere(base.currentLocation))
		{
			furniture_is_in_this_location = true;
			List<Vector2> exit_positions = new List<Vector2>();
			Vector2 sit_position = new Vector2(furniture.GetSeatBounds().Left, furniture.GetSeatBounds().Top);
			if (furniture.IsSittingHere(this))
			{
				sit_position = furniture.GetSittingPosition(this, ignore_offsets: true).Value;
			}
			if (furniture.GetSittingDirection() == 2)
			{
				exit_positions.Add(sit_position + new Vector2(0f, 1f));
				this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(1f, 0f), sit_position + new Vector2(-1f, 0f), sit_position + new Vector2(0f, -1f));
			}
			else if (furniture.GetSittingDirection() == 1)
			{
				exit_positions.Add(sit_position + new Vector2(1f, 0f));
				this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(0f, -1f), sit_position + new Vector2(0f, 1f), sit_position + new Vector2(-1f, 0f));
			}
			else if (furniture.GetSittingDirection() == 3)
			{
				exit_positions.Add(sit_position + new Vector2(-1f, 0f));
				this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(0f, 1f), sit_position + new Vector2(0f, -1f), sit_position + new Vector2(1f, 0f));
			}
			else if (furniture.GetSittingDirection() == 0)
			{
				exit_positions.Add(sit_position + new Vector2(0f, -1f));
				this.SortSeatExitPositions(exit_positions, sit_position + new Vector2(-1f, 0f), sit_position + new Vector2(1f, 0f), sit_position + new Vector2(0f, 1f));
			}
			Microsoft.Xna.Framework.Rectangle bounds = furniture.GetSeatBounds();
			bounds.Inflate(1, 1);
			foreach (Vector2 v in Utility.getBorderOfThisRectangle(bounds))
			{
				exit_positions.Add(v);
			}
			foreach (Vector2 exit_position in exit_positions)
			{
				base.setTileLocation(exit_position);
				Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
				base.Position = old_position;
				Object tile_object = base.currentLocation.getObjectAtTile((int)exit_position.X, (int)exit_position.Y, ignorePassables: true);
				if (!base.currentLocation.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: true, 0, glider: false, this) && (tile_object == null || tile_object.isPassable()))
				{
					if (animate)
					{
						base.playNearbySoundAll("coin");
						this.synchronizedJump(4f);
						this.LerpPosition(sit_position * 64f, exit_position * 64f, 0.15f);
					}
					location_found = true;
					break;
				}
			}
		}
		if (!location_found)
		{
			if (animate)
			{
				base.playNearbySoundAll("coin");
			}
			base.Position = old_position;
			if (furniture_is_in_this_location)
			{
				Microsoft.Xna.Framework.Rectangle bounds2 = furniture.GetSeatBounds();
				bounds2.X *= 64;
				bounds2.Y *= 64;
				bounds2.Width *= 64;
				bounds2.Height *= 64;
				this.temporaryPassableTiles.Add(bounds2);
			}
		}
		if (!animate)
		{
			this.sittingFurniture = null;
			this.isSitting.Value = false;
			this.Halt();
			this.showNotCarrying();
		}
		else
		{
			this.isStopSitting = true;
		}
		Game1.haltAfterCheck = false;
		this.yOffset = 0f;
		this.xOffset = 0f;
	}

	public void SortSeatExitPositions(List<Vector2> list, Vector2 a, Vector2 b, Vector2 c)
	{
		Vector2 mouse_pos = Utility.PointToVector2(Game1.getMousePosition(ui_scale: false)) + new Vector2(Game1.viewport.X, Game1.viewport.Y);
		Vector2 move_direction = Vector2.Zero;
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveUpButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp))))
		{
			move_direction.Y -= 1f;
		}
		else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveDownButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown))))
		{
			move_direction.Y += 1f;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveLeftButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadLeft))))
		{
			move_direction.X -= 1f;
		}
		else if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveRightButton) || (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadRight))))
		{
			move_direction.X += 1f;
		}
		if (move_direction != Vector2.Zero)
		{
			mouse_pos = base.getStandingPosition() + move_direction * 64f;
		}
		mouse_pos /= 64f;
		List<Vector2> exit_positions = new List<Vector2> { a, b, c };
		exit_positions.Sort((Vector2 d, Vector2 e) => (d + new Vector2(0.5f, 0.5f) - mouse_pos).Length().CompareTo((e + new Vector2(0.5f, 0.5f) - mouse_pos).Length()));
		list.AddRange(exit_positions);
	}

	public virtual bool IsSitting()
	{
		return this.isSitting.Value;
	}

	public bool isInventoryFull()
	{
		for (int i = 0; i < this.maxItems.Value; i++)
		{
			if (this.Items.Count > i && this.Items[i] == null)
			{
				return false;
			}
		}
		return true;
	}

	public bool couldInventoryAcceptThisItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.IsRecipe)
		{
			return true;
		}
		switch (item.QualifiedItemId)
		{
		case "(O)73":
		case "(O)930":
		case "(O)102":
		case "(O)858":
		case "(O)GoldCoin":
			return true;
		default:
		{
			for (int i = 0; i < this.maxItems.Value; i++)
			{
				if (this.Items.Count > i && (this.Items[i] == null || (item is Object && this.Items[i] is Object && this.Items[i].Stack + item.Stack <= this.Items[i].maximumStackSize() && (this.Items[i] as Object).canStackWith(item))))
				{
					return true;
				}
			}
			if (this.IsLocalPlayer && this.isInventoryFull() && Game1.hudMessages.Count == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			return false;
		}
		}
	}

	public bool couldInventoryAcceptThisItem(string id, int stack, int quality = 0)
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
		switch (itemData.QualifiedItemId)
		{
		case "(O)73":
		case "(O)930":
		case "(O)102":
		case "(O)858":
		case "(O)GoldCoin":
			return true;
		default:
		{
			for (int i = 0; i < this.maxItems.Value; i++)
			{
				if (this.Items.Count > i && (this.Items[i] == null || (this.Items[i].Stack + stack <= this.Items[i].maximumStackSize() && this.Items[i].QualifiedItemId == itemData.QualifiedItemId && this.Items[i].quality.Value == quality)))
				{
					return true;
				}
			}
			if (this.IsLocalPlayer && this.isInventoryFull() && Game1.hudMessages.Count == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			return false;
		}
		}
	}

	public NPC getSpouse()
	{
		if (this.isMarriedOrRoommates() && this.spouse != null)
		{
			return Game1.getCharacterFromName(this.spouse);
		}
		return null;
	}

	public int freeSpotsInInventory()
	{
		int slotsUsed = this.Items.CountItemStacks();
		if (slotsUsed >= this.maxItems.Value)
		{
			return 0;
		}
		return this.maxItems.Value - slotsUsed;
	}

	/// <summary>Get the behavior that applies when this item is received.</summary>
	/// <param name="item">The item being received.</param>
	/// <param name="needsInventorySpace">Whether this item takes space in the player inventory. This is false for special items like Qi Gems.</param>
	/// <param name="showNotification">Whether to show a HUD notification when the item is received.</param>
	public void GetItemReceiveBehavior(Item item, out bool needsInventorySpace, out bool showNotification)
	{
		if (item is SpecialItem)
		{
			needsInventorySpace = false;
			showNotification = false;
			return;
		}
		switch (item.QualifiedItemId)
		{
		case "(O)73":
		case "(O)102":
		case "(O)858":
			needsInventorySpace = false;
			showNotification = true;
			break;
		case "(O)GoldCoin":
		case "(O)930":
			needsInventorySpace = false;
			showNotification = false;
			break;
		default:
			needsInventorySpace = true;
			showNotification = true;
			break;
		}
		if (item.IsRecipe)
		{
			needsInventorySpace = false;
			showNotification = true;
		}
	}

	/// <summary>Handle an item being added to the current player's inventory.</summary>
	/// <param name="item">The item that was added. If <see cref="!:mergedIntoStack" /> is set, this is the original item rather than the one actually in the player's inventory.</param>
	/// <param name="countAdded">The number of the item that was added. This may differ from <paramref name="item" />'s stack size if it was only partly added or split across multiple stacks.</param>
	/// <param name="mergedIntoStack">The previous item stack it was merged into, if applicable.</param>
	/// <param name="hideHudNotification">Hide the 'item received' HUD notification even if it would normally be shown. This is used when merging the item into multiple stacks, so the HUD notification is shown once.</param>
	public void OnItemReceived(Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification = false)
	{
		if (!this.IsLocalPlayer)
		{
			return;
		}
		(item as Object)?.reloadSprite();
		if (item.HasBeenInInventory)
		{
			return;
		}
		Item actualItem = mergedIntoStack ?? item;
		if (!hideHudNotification)
		{
			this.GetItemReceiveBehavior(actualItem, out var _, out var showHudNotification);
			if (showHudNotification)
			{
				this.ShowItemReceivedHudMessage(actualItem, countAdded);
			}
		}
		if (this.freezePause <= 0)
		{
			this.mostRecentlyGrabbedItem = actualItem;
		}
		if (item.SetFlagOnPickup != null)
		{
			if (!this.hasOrWillReceiveMail(item.SetFlagOnPickup))
			{
				Game1.addMail(item.SetFlagOnPickup, noLetter: true);
			}
			actualItem.SetFlagOnPickup = null;
		}
		(actualItem as SpecialItem)?.actionWhenReceived(this);
		if (actualItem is Object { specialItem: not false } obj)
		{
			string key = (obj.IsRecipe ? ("-" + obj.ItemId) : obj.ItemId);
			if (obj.bigCraftable.Value || obj is Furniture)
			{
				if (!this.specialBigCraftables.Contains(key))
				{
					this.specialBigCraftables.Add(key);
				}
			}
			else if (!this.specialItems.Contains(key))
			{
				this.specialItems.Add(key);
			}
		}
		if (item.IsRecipe)
		{
			item.LearnRecipe();
			Game1.playSound("newRecipe");
			return;
		}
		int originalStack = actualItem.Stack;
		try
		{
			actualItem.Stack = countAdded;
			this.NotifyQuests((Quest quest) => quest.OnItemReceived(actualItem, countAdded));
			if (this.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in this.team.specialOrders)
				{
					specialOrder.onItemCollected?.Invoke(this, actualItem);
				}
			}
		}
		finally
		{
			actualItem.Stack = originalStack;
		}
		if (actualItem.HasTypeObject() && actualItem is Object obj2)
		{
			if (obj2.Category == -2 || obj2.Type == "Minerals")
			{
				this.foundMineral(obj2.ItemId);
			}
			else if (obj2.Type == "Arch")
			{
				this.foundArtifact(obj2.ItemId, 1);
			}
		}
		this.stats.checkForHeldItemAchievements();
		switch (actualItem.QualifiedItemId)
		{
		case "(O)GoldCoin":
		{
			Game1.playSound("moneyDial");
			int coinAmount = 250 * countAdded;
			if (Game1.IsSpring && Game1.dayOfMonth == 17 && base.currentLocation is Forest && base.Tile.Y > 90f)
			{
				coinAmount = 25;
			}
			this.Money += coinAmount;
			this.removeItemFromInventory(item);
			Game1.dayTimeMoneyBox.gotGoldCoin(coinAmount);
			break;
		}
		case "(O)73":
			this.foundWalnut(countAdded);
			this.removeItemFromInventory(actualItem);
			break;
		case "(O)858":
			this.QiGems += countAdded;
			Game1.playSound("qi_shop_purchase");
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), 100f, 1, 8, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.2f),
				stopAcceleratingWhenVelocityIsZero = true,
				attachedCharacter = this,
				positionFollowsAttachedCharacter = true
			});
			this.removeItemFromInventory(actualItem);
			break;
		case "(O)930":
		{
			int amount = 10 * countAdded;
			this.health = Math.Min(this.maxHealth, this.health + amount);
			base.currentLocation.debris.Add(new Debris(amount, base.getStandingPosition(), Color.Lime, 1f, this));
			Game1.playSound("healSound");
			this.removeItemFromInventory(actualItem);
			break;
		}
		case "(O)875":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop") && this.team.SpecialOrderActive("Wizard"))
			{
				Game1.addMailForTomorrow("ectoplasmDrop", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(O)876":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("prismaticJellyDrop") && this.team.SpecialOrderActive("Wizard2"))
			{
				Game1.addMailForTomorrow("prismaticJellyDrop", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(O)897":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotMissingStocklist"))
			{
				Game1.addMailForTomorrow("gotMissingStocklist", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(BC)256":
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotFirstJunimoChest"))
			{
				Game1.addMailForTomorrow("gotFirstJunimoChest", noLetter: true, sendToEveryone: true);
			}
			break;
		case "(O)535":
			Game1.PerformActionWhenPlayerFree(delegate
			{
				if (!this.hasOrWillReceiveMail("geodeFound"))
				{
					this.mailReceived.Add("geodeFound");
					this.holdUpItemThenMessage(actualItem);
				}
			});
			break;
		case "(O)428":
			if (!this.hasOrWillReceiveMail("clothFound"))
			{
				Game1.addMailForTomorrow("clothFound", noLetter: true);
			}
			break;
		case "(O)102":
			Game1.PerformActionWhenPlayerFree(delegate
			{
				this.foundArtifact(actualItem.ItemId, 1);
			});
			this.removeItemFromInventory(actualItem);
			this.stats.NotesFound++;
			break;
		case "(O)390":
			this.stats.StoneGathered++;
			if (this.stats.StoneGathered >= 100 && !this.hasOrWillReceiveMail("robinWell"))
			{
				Game1.addMailForTomorrow("robinWell");
			}
			break;
		case "(O)384":
			this.stats.GoldFound += (uint)countAdded;
			break;
		case "(O)380":
			this.stats.IronFound += (uint)countAdded;
			break;
		case "(O)386":
			this.stats.IridiumFound += (uint)countAdded;
			break;
		case "(O)378":
			this.stats.CopperFound += (uint)countAdded;
			if (!this.hasOrWillReceiveMail("copperFound"))
			{
				Game1.addMailForTomorrow("copperFound", noLetter: true);
			}
			break;
		case "(O)74":
			this.stats.PrismaticShardsFound++;
			break;
		case "(O)72":
			this.stats.DiamondsFound++;
			break;
		case "(BC)248":
			Game1.netWorldState.Value.MiniShippingBinsObtained++;
			break;
		}
		actualItem.HasBeenInInventory = true;
	}

	/// <summary>Show the item-received HUD message for an item if applicable for the item type.</summary>
	/// <param name="item">The item that was added.</param>
	/// <param name="countAdded">The number of the item that was added. This may differ from <paramref name="item" />'s stack size if it was only partly added or split across multiple stacks.</param>
	public void ShowItemReceivedHudMessageIfNeeded(Item item, int countAdded)
	{
		this.GetItemReceiveBehavior(item, out var _, out var showHudNotification);
		if (showHudNotification)
		{
			this.ShowItemReceivedHudMessage(item, countAdded);
		}
	}

	/// <summary>Show the item-received HUD message for an item.</summary>
	/// <param name="item">The item that was added.</param>
	/// <param name="countAdded">The number of the item that was added. This may differ from <paramref name="item" />'s stack size if it was only partly added or split across multiple stacks.</param>
	public void ShowItemReceivedHudMessage(Item item, int countAdded)
	{
		if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ItemGrabMenu))
		{
			Game1.addHUDMessage(HUDMessage.ForItemGained(item, countAdded));
		}
	}

	public int getIndexOfInventoryItem(Item item)
	{
		for (int i = 0; i < this.Items.Count; i++)
		{
			if (this.Items[i] == item || (this.Items[i] != null && item != null && item.canStackWith(this.Items[i])))
			{
				return i;
			}
		}
		return -1;
	}

	public void reduceActiveItemByOne()
	{
		if (this.CurrentItem != null && --this.CurrentItem.Stack <= 0)
		{
			this.removeItemFromInventory(this.CurrentItem);
			this.showNotCarrying();
		}
	}

	public void ReequipEnchantments()
	{
		Tool tool = this.CurrentTool;
		if (tool == null)
		{
			return;
		}
		foreach (BaseEnchantment enchantment in tool.enchantments)
		{
			enchantment.OnEquip(this);
		}
	}

	public void removeItemFromInventory(Item which)
	{
		if (which != null)
		{
			int i = this.Items.IndexOf(which);
			if (i >= 0 && i < this.Items.Count)
			{
				this.Items[i].actionWhenStopBeingHeld(this);
				this.Items[i] = null;
			}
		}
	}

	/// <summary>Get whether the player is married to or roommates with an NPC or player.</summary>
	public bool isMarriedOrRoommates()
	{
		if (this.team.IsMarried(this.UniqueMultiplayerID))
		{
			return true;
		}
		if (this.spouse != null && this.friendshipData.TryGetValue(this.spouse, out var friendship))
		{
			return friendship.IsMarried();
		}
		return false;
	}

	public bool isEngaged()
	{
		if (this.team.IsEngaged(this.UniqueMultiplayerID))
		{
			return true;
		}
		if (this.spouse != null && this.friendshipData.TryGetValue(this.spouse, out var friendship))
		{
			return friendship.IsEngaged();
		}
		return false;
	}

	public void removeFirstOfThisItemFromInventory(string itemId, int count = 1)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return;
		}
		int remaining = count;
		if (this.ActiveObject?.QualifiedItemId == itemId)
		{
			int toRemove = Math.Min(remaining, this.ActiveObject.Stack);
			remaining -= toRemove;
			if (this.ActiveObject.ConsumeStack(toRemove) == null)
			{
				this.ActiveObject = null;
				this.showNotCarrying();
			}
		}
		if (remaining > 0)
		{
			this.Items.ReduceId(itemId, remaining);
		}
	}

	public void rotateShirt(int direction, List<string> validIds = null)
	{
		string itemId = this.shirt.Value;
		if (validIds == null)
		{
			validIds = new List<string>(Game1.shirtData.Keys);
		}
		int index = validIds.IndexOf(itemId);
		if (index == -1)
		{
			itemId = validIds.FirstOrDefault();
			if (itemId != null)
			{
				this.changeShirt(itemId);
			}
		}
		else
		{
			index = Utility.WrapIndex(index + direction, validIds.Count);
			itemId = validIds[index];
			this.changeShirt(itemId);
		}
	}

	public void changeShirt(string itemId)
	{
		this.shirt.Set(itemId);
		this.FarmerRenderer.changeShirt(itemId);
	}

	public void rotatePantStyle(int direction, List<string> validIds = null)
	{
		string itemId = this.pants.Value;
		if (validIds == null)
		{
			validIds = new List<string>(Game1.pantsData.Keys);
		}
		int index = validIds.IndexOf(itemId);
		if (index == -1)
		{
			itemId = validIds.FirstOrDefault();
			if (itemId != null)
			{
				this.changePantStyle(itemId);
			}
		}
		else
		{
			index = Utility.WrapIndex(index + direction, validIds.Count);
			itemId = validIds[index];
			this.changePantStyle(itemId);
		}
	}

	public void changePantStyle(string itemId)
	{
		this.pants.Set(itemId);
		this.FarmerRenderer.changePants(itemId);
	}

	public void ConvertClothingOverrideToClothesItems()
	{
		if (this.IsOverridingPants(out var pantsId, out var color))
		{
			if (ItemRegistry.Exists("(P)" + pantsId))
			{
				Clothing clothes = new Clothing(pantsId);
				clothes.clothesColor.Value = color ?? Color.White;
				this.Equip(clothes, this.pantsItem);
			}
			this.pants.Value = "-1";
		}
		if (this.IsOverridingShirt(out var shirtId))
		{
			if (int.TryParse(shirtId, out var index) && index < 1000)
			{
				shirtId = (index + 1000).ToString();
			}
			if (ItemRegistry.Exists("(S)" + shirtId))
			{
				Clothing clothes2 = new Clothing(shirtId);
				this.Equip(clothes2, this.shirtItem);
			}
			this.shirt.Value = "-1";
		}
	}

	public static Dictionary<int, string> GetHairStyleMetadataFile()
	{
		if (Farmer.hairStyleMetadataFile == null)
		{
			Farmer.hairStyleMetadataFile = DataLoader.HairData(Game1.content);
		}
		return Farmer.hairStyleMetadataFile;
	}

	public static HairStyleMetadata GetHairStyleMetadata(int hair_index)
	{
		Farmer.GetHairStyleMetadataFile();
		if (Farmer.hairStyleMetadata.TryGetValue(hair_index, out var hair_data))
		{
			return hair_data;
		}
		try
		{
			if (Farmer.hairStyleMetadataFile.TryGetValue(hair_index, out var data))
			{
				string[] split = data.Split('/');
				HairStyleMetadata new_hair_data = new HairStyleMetadata();
				new_hair_data.texture = Game1.content.Load<Texture2D>("Characters\\Farmer\\" + split[0]);
				new_hair_data.tileX = int.Parse(split[1]);
				new_hair_data.tileY = int.Parse(split[2]);
				if (split.Length > 3 && split[3].EqualsIgnoreCase("true"))
				{
					new_hair_data.usesUniqueLeftSprite = true;
				}
				else
				{
					new_hair_data.usesUniqueLeftSprite = false;
				}
				if (split.Length > 4)
				{
					new_hair_data.coveredIndex = int.Parse(split[4]);
				}
				if (split.Length > 5 && split[5].EqualsIgnoreCase("true"))
				{
					new_hair_data.isBaldStyle = true;
				}
				else
				{
					new_hair_data.isBaldStyle = false;
				}
				hair_data = new_hair_data;
			}
		}
		catch (Exception)
		{
		}
		Farmer.hairStyleMetadata[hair_index] = hair_data;
		return hair_data;
	}

	public static List<int> GetAllHairstyleIndices()
	{
		if (Farmer.allHairStyleIndices != null)
		{
			return Farmer.allHairStyleIndices;
		}
		Farmer.GetHairStyleMetadataFile();
		Farmer.allHairStyleIndices = new List<int>();
		int highest_hair = FarmerRenderer.hairStylesTexture.Height / 96 * 8;
		for (int i = 0; i < highest_hair; i++)
		{
			Farmer.allHairStyleIndices.Add(i);
		}
		foreach (int key in Farmer.hairStyleMetadataFile.Keys)
		{
			if (key >= 0 && !Farmer.allHairStyleIndices.Contains(key))
			{
				Farmer.allHairStyleIndices.Add(key);
			}
		}
		Farmer.allHairStyleIndices.Sort();
		return Farmer.allHairStyleIndices;
	}

	public static int GetLastHairStyle()
	{
		return Farmer.GetAllHairstyleIndices()[Farmer.GetAllHairstyleIndices().Count - 1];
	}

	public void changeHairStyle(int whichHair)
	{
		bool num = this.isBald();
		if (Farmer.GetHairStyleMetadata(whichHair) != null)
		{
			this.hair.Set(whichHair);
		}
		else
		{
			if (whichHair < 0)
			{
				whichHair = Farmer.GetLastHairStyle();
			}
			else if (whichHair > Farmer.GetLastHairStyle())
			{
				whichHair = 0;
			}
			this.hair.Set(whichHair);
		}
		if (this.IsBaldHairStyle(whichHair))
		{
			this.FarmerRenderer.textureName.Set(this.getTexture());
		}
		if (num && !this.isBald())
		{
			this.FarmerRenderer.textureName.Set(this.getTexture());
		}
	}

	public virtual bool IsBaldHairStyle(int style)
	{
		if (Farmer.GetHairStyleMetadata(this.hair.Value) != null)
		{
			return Farmer.GetHairStyleMetadata(this.hair.Value).isBaldStyle;
		}
		if ((uint)(style - 49) <= 6u)
		{
			return true;
		}
		return false;
	}

	private bool isBald()
	{
		return this.IsBaldHairStyle(this.getHair());
	}

	/// <summary>Change the color of the player's shoes.</summary>
	/// <param name="color">The new color to set.</param>
	public void changeShoeColor(string which)
	{
		this.FarmerRenderer.recolorShoes(which);
		this.shoes.Set(which);
	}

	/// <summary>Change the color of the player's hair.</summary>
	/// <param name="color">The new color to set.</param>
	public void changeHairColor(Color c)
	{
		this.hairstyleColor.Set(c);
	}

	/// <summary>Change the color of the player's equipped pants.</summary>
	/// <param name="color">The new color to set.</param>
	public void changePantsColor(Color color)
	{
		this.pantsColor.Set(color);
		this.pantsItem.Value?.clothesColor.Set(color);
	}

	public void changeHat(int newHat)
	{
		if (newHat < 0)
		{
			this.Equip(null, this.hat);
		}
		else
		{
			this.Equip(ItemRegistry.Create<Hat>("(H)" + newHat), this.hat);
		}
	}

	public void changeAccessory(int which)
	{
		if (which < -1)
		{
			which = 29;
		}
		if (which >= -1)
		{
			if (which >= 30)
			{
				which = -1;
			}
			this.accessory.Set(which);
		}
	}

	public void changeSkinColor(int which, bool force = false)
	{
		if (which < 0)
		{
			which = 23;
		}
		else if (which >= 24)
		{
			which = 0;
		}
		this.skin.Set(this.FarmerRenderer.recolorSkin(which, force));
	}

	/// <summary>Whether this player has dark skin for the purposes of child genetics.</summary>
	public virtual bool hasDarkSkin()
	{
		if (this.skin.Value < 4 || this.skin.Value > 8 || this.skin.Value == 7)
		{
			return this.skin.Value == 14;
		}
		return true;
	}

	/// <summary>Change the color of the player's eyes.</summary>
	/// <param name="color">The new color to set.</param>
	public void changeEyeColor(Color c)
	{
		this.newEyeColor.Set(c);
		this.FarmerRenderer.recolorEyes(c);
	}

	public int getHair(bool ignore_hat = false)
	{
		if (this.hat.Value != null && !this.bathingClothes.Value && !ignore_hat)
		{
			switch ((Hat.HairDrawType)this.hat.Value.hairDrawType.Value)
			{
			case Hat.HairDrawType.HideHair:
				return -1;
			case Hat.HairDrawType.DrawObscuredHair:
				switch (this.hair.Value)
				{
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
					return this.hair.Value;
				case 48:
					return 6;
				case 49:
					return 52;
				case 3:
					return 11;
				case 1:
				case 5:
				case 6:
				case 9:
				case 11:
				case 17:
				case 20:
				case 23:
				case 24:
				case 25:
				case 27:
				case 28:
				case 29:
				case 30:
				case 32:
				case 33:
				case 34:
				case 36:
				case 39:
				case 41:
				case 43:
				case 44:
				case 45:
				case 46:
				case 47:
					return this.hair.Value;
				case 18:
				case 19:
				case 21:
				case 31:
					return 23;
				case 42:
					return 46;
				default:
					if (this.hair.Value >= 16)
					{
						if (this.hair.Value < 100)
						{
							return 30;
						}
						return this.hair.Value;
					}
					return 7;
				}
			}
		}
		return this.hair.Value;
	}

	public void changeGender(bool male)
	{
		if (male)
		{
			this.Gender = Gender.Male;
			this.FarmerRenderer.textureName.Set(this.getTexture());
			this.FarmerRenderer.heightOffset.Set(0);
		}
		else
		{
			this.Gender = Gender.Female;
			this.FarmerRenderer.heightOffset.Set(4);
			this.FarmerRenderer.textureName.Set(this.getTexture());
		}
		this.changeShirt(this.shirt.Value);
	}

	public void changeFriendship(int amount, NPC n)
	{
		if (n == null || (!(n is Child) && !n.IsVillager))
		{
			return;
		}
		if (amount > 0 && this.stats.Get("Book_Friendship") != 0)
		{
			amount = (int)((float)amount * 1.1f);
		}
		if (amount > 0 && n.SpeaksDwarvish() && !this.canUnderstandDwarves)
		{
			return;
		}
		if (this.friendshipData.TryGetValue(n.Name, out var friendship))
		{
			if (n.isDivorcedFrom(this) && amount > 0)
			{
				return;
			}
			if (n.Equals(this.getSpouse()))
			{
				amount = (int)((float)amount * 0.66f);
			}
			friendship.Points = Math.Max(0, Math.Min(friendship.Points + amount, (Utility.GetMaximumHeartsForCharacter(n) + 1) * 250 - 1));
			if (n.datable.Value && friendship.Points >= 2000 && !this.hasOrWillReceiveMail("Bouquet"))
			{
				Game1.addMailForTomorrow("Bouquet");
			}
			if (n.datable.Value && friendship.Points >= 2500 && !this.hasOrWillReceiveMail("SeaAmulet"))
			{
				Game1.addMailForTomorrow("SeaAmulet");
			}
			if (friendship.Points < 0)
			{
				friendship.Points = 0;
			}
		}
		else
		{
			Game1.debugOutput = "Tried to change friendship for a friend that wasn't there.";
		}
		Game1.stats.checkForFriendshipAchievements();
	}

	public bool knowsRecipe(string name)
	{
		if (name.EndsWith(" Recipe"))
		{
			name = name.Substring(0, name.Length - " Recipe".Length);
		}
		if (!this.craftingRecipes.Keys.Contains(name))
		{
			return this.cookingRecipes.Keys.Contains(name);
		}
		return true;
	}

	public Vector2 getUniformPositionAwayFromBox(int direction, int distance)
	{
		Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBox();
		return this.FacingDirection switch
		{
			0 => new Vector2(bounds.Center.X, bounds.Y - distance), 
			1 => new Vector2(bounds.Right + distance, bounds.Center.Y), 
			2 => new Vector2(bounds.Center.X, bounds.Bottom + distance), 
			3 => new Vector2(bounds.X - distance, bounds.Center.Y), 
			_ => Vector2.Zero, 
		};
	}

	public bool hasTalkedToFriendToday(string npcName)
	{
		if (this.friendshipData.TryGetValue(npcName, out var friendship))
		{
			return friendship.TalkedToToday;
		}
		return false;
	}

	public void talkToFriend(NPC n, int friendshipPointChange = 20)
	{
		if (this.friendshipData.TryGetValue(n.Name, out var friendship) && !friendship.TalkedToToday)
		{
			this.changeFriendship(friendshipPointChange, n);
			friendship.TalkedToToday = true;
		}
	}

	public void moveRaft(GameLocation currentLocation, GameTime time)
	{
		float raftInertia = 0.2f;
		if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
		{
			base.yVelocity = Math.Max(base.yVelocity - raftInertia, -3f + Math.Abs(base.xVelocity) / 2f);
			this.faceDirection(0);
		}
		if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
		{
			base.xVelocity = Math.Min(base.xVelocity + raftInertia, 3f - Math.Abs(base.yVelocity) / 2f);
			this.faceDirection(1);
		}
		if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
		{
			base.yVelocity = Math.Min(base.yVelocity + raftInertia, 3f - Math.Abs(base.xVelocity) / 2f);
			this.faceDirection(2);
		}
		if (this.CanMove && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
		{
			base.xVelocity = Math.Max(base.xVelocity - raftInertia, -3f + Math.Abs(base.yVelocity) / 2f);
			this.faceDirection(3);
		}
		Microsoft.Xna.Framework.Rectangle collidingBox = new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)(base.Position.Y + 64f + 16f), 64, 64);
		collidingBox.X += (int)Math.Ceiling(base.xVelocity);
		if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, this))
		{
			base.position.X += base.xVelocity;
		}
		collidingBox.X -= (int)Math.Ceiling(base.xVelocity);
		collidingBox.Y += (int)Math.Floor(base.yVelocity);
		if (!currentLocation.isCollidingPosition(collidingBox, Game1.viewport, this))
		{
			base.position.Y += base.yVelocity;
		}
		if (base.xVelocity != 0f || base.yVelocity != 0f)
		{
			this.raftPuddleCounter -= time.ElapsedGameTime.Milliseconds;
			if (this.raftPuddleCounter <= 0)
			{
				this.raftPuddleCounter = 250;
				currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(base.xVelocity) + Math.Abs(base.yVelocity)) * 3f, 8, 0, new Vector2(collidingBox.X, collidingBox.Y - 64), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
				if (Game1.random.NextDouble() < 0.6)
				{
					Game1.playSound("wateringCan");
				}
				if (Game1.random.NextDouble() < 0.6)
				{
					this.raftBobCounter /= 2;
				}
			}
		}
		this.raftBobCounter -= time.ElapsedGameTime.Milliseconds;
		if (this.raftBobCounter <= 0)
		{
			this.raftBobCounter = Game1.random.Next(15, 28) * 100;
			if (this.yOffset <= 0f)
			{
				this.yOffset = 4f;
				currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(base.xVelocity) + Math.Abs(base.yVelocity)) * 3f, 8, 0, new Vector2(collidingBox.X, collidingBox.Y - 64), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
			}
			else
			{
				this.yOffset = 0f;
			}
		}
		if (base.xVelocity > 0f)
		{
			base.xVelocity = Math.Max(0f, base.xVelocity - raftInertia / 2f);
		}
		else if (base.xVelocity < 0f)
		{
			base.xVelocity = Math.Min(0f, base.xVelocity + raftInertia / 2f);
		}
		if (base.yVelocity > 0f)
		{
			base.yVelocity = Math.Max(0f, base.yVelocity - raftInertia / 2f);
		}
		else if (base.yVelocity < 0f)
		{
			base.yVelocity = Math.Min(0f, base.yVelocity + raftInertia / 2f);
		}
	}

	public void warpFarmer(Warp w, int warp_collide_direction)
	{
		if (w == null || Game1.eventUp)
		{
			return;
		}
		this.Halt();
		int target_x = w.TargetX;
		int target_y = w.TargetY;
		if (this.isRidingHorse())
		{
			switch (warp_collide_direction)
			{
			case 3:
				Game1.nextFarmerWarpOffsetX = -1;
				break;
			case 0:
				Game1.nextFarmerWarpOffsetY = -1;
				break;
			}
		}
		Game1.warpFarmer(w.TargetName, target_x, target_y, w.flipFarmer.Value);
	}

	public void warpFarmer(Warp w)
	{
		this.warpFarmer(w, -1);
	}

	public void startToPassOut()
	{
		this.passOutEvent.Fire();
	}

	private void performPassOut()
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		if (!base.swimming.Value && this.bathingClothes.Value)
		{
			this.bathingClothes.Value = false;
		}
		if (!this.passedOut && !this.FarmerSprite.isPassingOut())
		{
			this.faceDirection(2);
			this.completelyStopAnimatingOrDoingAction();
			this.animateOnce(293);
		}
	}

	public static void passOutFromTired(Farmer who)
	{
		if (!who.IsLocalPlayer)
		{
			return;
		}
		if (who.IsSitting())
		{
			who.StopSitting(animate: false);
		}
		if (who.isRidingHorse())
		{
			who.mount.dismount();
		}
		if (Game1.activeClickableMenu != null)
		{
			Game1.activeClickableMenu.emergencyShutDown();
			Game1.exitActiveMenu();
		}
		who.completelyStopAnimatingOrDoingAction();
		if (who.bathingClothes.Value)
		{
			who.changeOutOfSwimSuit();
		}
		who.swimming.Value = false;
		who.CanMove = false;
		who.FarmerSprite.setCurrentSingleFrame(5, 3000);
		who.FarmerSprite.PauseForSingleAnimation = true;
		if (!who.IsDedicatedPlayer && who == Game1.player && who.team.sleepAnnounceMode.Value != FarmerTeam.SleepAnnounceModes.Off)
		{
			string key = "PassedOut";
			string possibleLocationKey = "PassedOut_" + who.currentLocation.Name.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
			if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:Chat_" + possibleLocationKey) != null)
			{
				Game1.multiplayer.globalChatInfoMessage(possibleLocationKey, who.displayName);
			}
			else
			{
				int key_index = 0;
				for (int i = 0; i < 2; i++)
				{
					if (Game1.random.NextDouble() < 0.25)
					{
						key_index++;
					}
				}
				Game1.multiplayer.globalChatInfoMessage(key + key_index, who.displayName);
			}
		}
		if (Game1.currentLocation is FarmHouse farmhouse)
		{
			who.lastSleepLocation.Value = farmhouse.NameOrUniqueName;
			who.lastSleepPoint.Value = farmhouse.GetPlayerBedSpot();
		}
		Game1.multiplayer.sendPassoutRequest();
	}

	public static void performPassoutWarp(Farmer who, string bed_location_name, Point bed_point, bool has_bed)
	{
		GameLocation passOutLocation = who.currentLocationRef.Value;
		Vector2 bed = Utility.PointToVector2(bed_point) * 64f;
		Vector2 bed_tile = new Vector2((int)bed.X / 64, (int)bed.Y / 64);
		Vector2 bed_sleep_position = bed;
		if (!who.isInBed.Value)
		{
			LocationRequest locationRequest = Game1.getLocationRequest(bed_location_name);
			Game1.warpFarmer(locationRequest, (int)bed.X / 64, (int)bed.Y / 64, 2);
			locationRequest.OnWarp += ContinuePassOut;
			who.FarmerSprite.setCurrentSingleFrame(5, 3000);
			who.FarmerSprite.PauseForSingleAnimation = true;
		}
		else
		{
			ContinuePassOut();
		}
		void ContinuePassOut()
		{
			who.Position = bed_sleep_position;
			who.currentLocation.lastTouchActionLocation = bed_tile;
			(who.NetFields.Root as NetRoot<Farmer>)?.CancelInterpolation();
			if (!Game1.IsMultiplayer || Game1.timeOfDay >= 2600)
			{
				Game1.PassOutNewDay();
			}
			Game1.changeMusicTrack("none");
			if (!(passOutLocation is FarmHouse) && !(passOutLocation is IslandFarmHouse) && !(passOutLocation is Cellar) && !passOutLocation.HasMapPropertyWithValue("PassOutSafe"))
			{
				Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, who.UniqueMultiplayerID);
				int max_passout_cost = passOutLocation.GetLocationContext().MaxPassOutCost;
				if (max_passout_cost == -1)
				{
					max_passout_cost = LocationContexts.Default.MaxPassOutCost;
				}
				int moneyToTake = Math.Min(max_passout_cost, who.Money / 10);
				List<PassOutMailData> obj = passOutLocation.GetLocationContext().PassOutMail ?? LocationContexts.Default.PassOutMail;
				PassOutMailData selected_mail = null;
				List<PassOutMailData> valid_mails = new List<PassOutMailData>();
				foreach (PassOutMailData mail in obj)
				{
					if (GameStateQuery.CheckConditions(mail.Condition, passOutLocation, null, null, null, r))
					{
						if (mail.SkipRandomSelection)
						{
							selected_mail = mail;
							break;
						}
						valid_mails.Add(mail);
					}
				}
				if (selected_mail == null && valid_mails.Count > 0)
				{
					selected_mail = r.ChooseFrom(valid_mails);
				}
				string mail_to_send = null;
				if (selected_mail != null)
				{
					if (selected_mail.MaxPassOutCost >= 0)
					{
						moneyToTake = Math.Min(moneyToTake, selected_mail.MaxPassOutCost);
					}
					string mailName = selected_mail.Mail;
					if (!string.IsNullOrEmpty(mailName))
					{
						Dictionary<string, string> mails = DataLoader.Mail(Game1.content);
						mail_to_send = (mails.ContainsKey(mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled") + "_" + (who.IsMale ? "Male" : "Female")) ? (mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled") + "_" + (who.IsMale ? "Male" : "Female")) : (mails.ContainsKey(mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled")) ? (mailName + "_" + ((moneyToTake > 0) ? "Billed" : "NotBilled")) : ((!mails.ContainsKey(mailName)) ? "passedOut2" : mailName)));
						if (mail_to_send.StartsWith("passedOut"))
						{
							mail_to_send = mail_to_send + " " + moneyToTake;
						}
					}
				}
				if (moneyToTake > 0)
				{
					who.Money -= moneyToTake;
				}
				if (mail_to_send != null)
				{
					who.mailForTomorrow.Add(mail_to_send);
				}
			}
		}
	}

	public static void doSleepEmote(Farmer who)
	{
		who.doEmote(24);
		who.yJumpVelocity = -2f;
	}

	public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		if (this.mount != null && !this.mount.dismounting.Value)
		{
			return this.mount.GetBoundingBox();
		}
		Vector2 position = base.Position;
		return new Microsoft.Xna.Framework.Rectangle((int)position.X + 8, (int)position.Y + this.Sprite.getHeight() - 32, 48, 32);
	}

	public string getPetName()
	{
		foreach (NPC n in Game1.getFarm().characters)
		{
			if (n is Pet)
			{
				return n.Name;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC n2 in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (n2 is Pet)
				{
					return n2.Name;
				}
			}
		}
		return "your pet";
	}

	public Pet getPet()
	{
		foreach (NPC character in Game1.getFarm().characters)
		{
			if (character is Pet pet)
			{
				return pet;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (character2 is Pet pet2)
				{
					return pet2;
				}
			}
		}
		return null;
	}

	public string getPetDisplayName()
	{
		foreach (NPC n in Game1.getFarm().characters)
		{
			if (n is Pet)
			{
				return n.displayName;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC n2 in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (n2 is Pet)
				{
					return n2.displayName;
				}
			}
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1972");
	}

	public bool hasPet()
	{
		foreach (NPC character in Game1.getFarm().characters)
		{
			if (character is Pet)
			{
				return true;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC character2 in Utility.getHomeOfFarmer(allFarmer).characters)
			{
				if (character2 is Pet)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void UpdateClothing()
	{
		this.FarmerRenderer.MarkSpriteDirty();
	}

	/// <summary>Get whether custom pants should be drawn instead of the equipped pants item.</summary>
	/// <param name="id">The pants ID to draw, if overridden.</param>
	/// <param name="color">The pants color to draw, if overridden.</param>
	public bool IsOverridingPants(out string id, out Color? color)
	{
		if (this.pants.Value != null && this.pants.Value != "-1")
		{
			id = this.pants.Value;
			color = this.pantsColor.Value;
			return true;
		}
		id = null;
		color = null;
		return false;
	}

	/// <summary>Get whether the current pants can be dyed.</summary>
	public bool CanDyePants()
	{
		return this.pantsItem.Value?.dyeable.Value ?? false;
	}

	/// <summary>Get the pants to draw on the farmer.</summary>
	/// <param name="texture">The texture to render.</param>
	/// <param name="spriteIndex">The sprite index in the <paramref name="texture" />.</param>
	public void GetDisplayPants(out Texture2D texture, out int spriteIndex)
	{
		if (this.IsOverridingPants(out var id, out var _))
		{
			ParsedItemData itemData = ItemRegistry.GetData("(P)" + id);
			if (itemData != null && !itemData.IsErrorItem)
			{
				texture = itemData.GetTexture();
				spriteIndex = itemData.SpriteIndex;
				return;
			}
		}
		if (this.pantsItem.Value != null)
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(this.pantsItem.Value.QualifiedItemId);
			if (data != null && !data.IsErrorItem)
			{
				texture = data.GetTexture();
				spriteIndex = this.pantsItem.Value.indexInTileSheet.Value;
				return;
			}
		}
		texture = FarmerRenderer.pantsTexture;
		spriteIndex = 14;
	}

	/// <summary>Get the unqualified item ID for the displayed pants (which aren't necessarily the equipped ones).</summary>
	public string GetPantsId()
	{
		if (this.IsOverridingPants(out var id, out var _))
		{
			return id;
		}
		return this.pantsItem.Value?.ItemId ?? "14";
	}

	public int GetPantsIndex()
	{
		this.GetDisplayPants(out var _, out var index);
		return index;
	}

	/// <summary>Get whether a custom shirt should be drawn instead of the equipped shirt item.</summary>
	/// <param name="id">The shirt ID to draw, if overridden.</param>
	public bool IsOverridingShirt(out string id)
	{
		if (this.shirt.Value != null && this.shirt.Value != "-1")
		{
			id = this.shirt.Value;
			return true;
		}
		id = null;
		return false;
	}

	/// <summary>Get whether the current shirt can be dyed.</summary>
	public bool CanDyeShirt()
	{
		return this.shirtItem.Value?.dyeable.Value ?? false;
	}

	/// <summary>Get the shirt to draw on the farmer.</summary>
	/// <param name="texture">The texture to render.</param>
	/// <param name="spriteIndex">The sprite index in the <paramref name="texture" />.</param>
	public void GetDisplayShirt(out Texture2D texture, out int spriteIndex)
	{
		if (this.IsOverridingShirt(out var id))
		{
			ParsedItemData itemData = ItemRegistry.GetData("(S)" + id);
			if (itemData != null && !itemData.IsErrorItem)
			{
				texture = itemData.GetTexture();
				spriteIndex = itemData.SpriteIndex;
				return;
			}
		}
		if (this.shirtItem.Value != null)
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(this.shirtItem.Value.QualifiedItemId);
			if (data != null && !data.IsErrorItem)
			{
				texture = data.GetTexture();
				spriteIndex = this.shirtItem.Value.indexInTileSheet.Value;
				return;
			}
		}
		texture = FarmerRenderer.shirtsTexture;
		spriteIndex = (this.IsMale ? 209 : 41);
	}

	/// <summary>Get the unqualified item ID for the displayed shirt (which isn't necessarily the equipped one).</summary>
	public string GetShirtId()
	{
		if (this.IsOverridingShirt(out var id))
		{
			return id;
		}
		if (this.shirtItem.Value != null)
		{
			return this.shirtItem.Value.ItemId;
		}
		if (!this.IsMale)
		{
			return "1041";
		}
		return "1209";
	}

	public int GetShirtIndex()
	{
		this.GetDisplayShirt(out var _, out var index);
		return index;
	}

	public bool ShirtHasSleeves()
	{
		if (!this.IsOverridingShirt(out var itemId))
		{
			itemId = this.shirtItem.Value?.ItemId;
		}
		if (itemId != null && Game1.shirtData.TryGetValue(itemId, out var data))
		{
			return data.HasSleeves;
		}
		return true;
	}

	/// <summary>Get the color of the currently worn shirt.</summary>
	public Color GetShirtColor()
	{
		if (this.IsOverridingShirt(out var id) && Game1.shirtData.TryGetValue(id, out var shirtData))
		{
			if (!shirtData.IsPrismatic)
			{
				return Utility.StringToColor(shirtData.DefaultColor) ?? Color.White;
			}
			return Utility.GetPrismaticColor();
		}
		if (this.shirtItem.Value != null)
		{
			if (this.shirtItem.Value.isPrismatic.Value)
			{
				return Utility.GetPrismaticColor();
			}
			return this.shirtItem.Value.clothesColor.Value;
		}
		return this.DEFAULT_SHIRT_COLOR;
	}

	/// <summary>Get the color of the currently worn pants.</summary>
	public Color GetPantsColor()
	{
		if (this.IsOverridingPants(out var _, out var color))
		{
			return color ?? Color.White;
		}
		if (this.pantsItem.Value != null)
		{
			if (this.pantsItem.Value.isPrismatic.Value)
			{
				return Utility.GetPrismaticColor();
			}
			return this.pantsItem.Value.clothesColor.Value;
		}
		return Color.White;
	}

	public bool movedDuringLastTick()
	{
		return !base.Position.Equals(this.lastPosition);
	}

	public int CompareTo(object obj)
	{
		return ((Farmer)obj).saveTime - this.saveTime;
	}

	public virtual void SetOnBridge(bool val)
	{
		if (this.onBridge.Value != val)
		{
			this.onBridge.Value = val;
			if (this.onBridge.Value)
			{
				this.showNotCarrying();
			}
		}
	}

	public float getDrawLayer()
	{
		if (this.onBridge.Value)
		{
			return (float)base.StandingPixel.Y / 10000f + this.drawLayerDisambiguator + 0.0256f;
		}
		if (this.IsSitting() && this.mapChairSitPosition.Value.X != -1f && this.mapChairSitPosition.Value.Y != -1f)
		{
			return (this.mapChairSitPosition.Value.Y + 1f) * 64f / 10000f;
		}
		return (float)base.StandingPixel.Y / 10000f + this.drawLayerDisambiguator;
	}

	public override void draw(SpriteBatch b)
	{
		if (base.currentLocation == null || (!base.currentLocation.Equals(Game1.currentLocation) && !this.IsLocalPlayer && !Game1.currentLocation.IsTemporary && !this.isFakeEventActor) || (this.hidden.Value && (base.currentLocation.currentEvent == null || this != base.currentLocation.currentEvent.farmer) && (!this.IsLocalPlayer || Game1.locationRequest == null)) || (this.viewingLocation.Value != null && this.IsLocalPlayer))
		{
			return;
		}
		float draw_layer = this.getDrawLayer();
		if (this.isRidingHorse())
		{
			this.mount.SyncPositionToRider();
			this.mount.draw(b);
			if (this.FacingDirection == 3 || this.FacingDirection == 1)
			{
				draw_layer += 0.0016f;
			}
		}
		float layerDepth = FarmerRenderer.GetLayerDepth(0f, FarmerRenderer.FarmerSpriteLayers.MAX);
		Vector2 origin = new Vector2(this.xOffset, (this.yOffset + 128f - (float)(this.GetBoundingBox().Height / 2)) / 4f + 4f);
		Point standingPixel = base.StandingPixel;
		Tile shadowTile = Game1.currentLocation.Map.RequireLayer("Buildings").PickTile(new Location(standingPixel.X, standingPixel.Y), Game1.viewport.Size);
		float glow_offset = layerDepth * 1f;
		float shadow_offset = layerDepth * 2f;
		if (base.isGlowing)
		{
			if (base.coloredBorder)
			{
				b.Draw(this.Sprite.Texture, new Vector2(base.getLocalPosition(Game1.viewport).X - 4f, base.getLocalPosition(Game1.viewport).Y - 4f), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, 0f, Vector2.Zero, 1.1f, SpriteEffects.None, draw_layer + glow_offset);
			}
			else
			{
				this.FarmerRenderer.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport) + this.jitter + new Vector2(0f, base.yJumpOffset), origin, draw_layer + glow_offset, base.glowingColor * base.glowingTransparency, this.rotation, this);
			}
		}
		if ((!(shadowTile?.TileIndexProperties.ContainsKey("Shadow"))) ?? true)
		{
			if (this.IsSitting() || !Game1.shouldTimePass() || !this.temporarilyInvincible || !this.flashDuringThisTemporaryInvincibility || this.temporaryInvincibilityTimer % 100 < 50)
			{
				this.farmerRenderer.Value.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport) + this.jitter + new Vector2(0f, base.yJumpOffset), origin, draw_layer, Color.White, this.rotation, this);
			}
		}
		else
		{
			this.farmerRenderer.Value.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport), origin, draw_layer, Color.White, this.rotation, this);
			this.farmerRenderer.Value.draw(b, this.FarmerSprite, this.FarmerSprite.SourceRect, base.getLocalPosition(Game1.viewport), origin, draw_layer + shadow_offset, Color.Black * 0.25f, this.rotation, this);
		}
		if (this.isRafting)
		{
			b.Draw(Game1.toolSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2(0f, this.yOffset), Game1.getSourceRectForStandardTileSheet(Game1.toolSpriteSheet, 1), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, FarmerRenderer.GetLayerDepth(draw_layer, FarmerRenderer.FarmerSpriteLayers.ToolUp));
		}
		if (Game1.activeClickableMenu == null && !Game1.eventUp && this.IsLocalPlayer && this.CurrentTool != null && (Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.options.alwaysShowToolHitLocation) && this.CurrentTool.doesShowTileLocationMarker() && (!Game1.options.hideToolHitLocationWhenInMotion || !this.isMoving()))
		{
			Vector2 mouse_position = Utility.PointToVector2(Game1.getMousePosition()) + new Vector2(Game1.viewport.X, Game1.viewport.Y);
			Vector2 draw_location = Game1.GlobalToLocal(Game1.viewport, Utility.clampToTile(this.GetToolLocation(mouse_position)));
			b.Draw(Game1.mouseCursors, draw_location, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, draw_location.Y / 10000f);
		}
		if (base.IsEmoting)
		{
			Vector2 emotePosition = base.getLocalPosition(Game1.viewport);
			emotePosition.Y -= 160f;
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);
		}
		if (this.ActiveObject != null && this.IsCarrying())
		{
			Game1.drawPlayerHeldObject(this);
		}
		this.sparklingText?.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(32f - this.sparklingText.textWidth / 2f, -128f)));
		if (this.UsingTool && this.CurrentTool != null)
		{
			Game1.drawTool(this);
		}
		foreach (Companion companion in this.companions)
		{
			companion.Draw(b);
		}
	}

	public virtual void DrawUsername(SpriteBatch b)
	{
		if (!Game1.IsMultiplayer || Game1.multiplayer == null || LocalMultiplayer.IsLocalMultiplayer(is_local_only: true) || this.usernameDisplayTime <= 0f)
		{
			return;
		}
		string username = Game1.multiplayer.getUserName(this.UniqueMultiplayerID);
		if (username == null)
		{
			return;
		}
		Vector2 string_size = Game1.smallFont.MeasureString(username);
		Vector2 draw_origin = base.getLocalPosition(Game1.viewport) + new Vector2(32f, -104f) - string_size / 2f;
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x != 0 || y != 0)
				{
					b.DrawString(Game1.smallFont, username, draw_origin + new Vector2(x, y) * 2f, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
				}
			}
		}
		b.DrawString(Game1.smallFont, username, draw_origin, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
	}

	public static void drinkGlug(Farmer who)
	{
		Color c = Color.LightBlue;
		if (who.itemToEat != null)
		{
			switch (ArgUtility.SplitBySpace(who.itemToEat.Name).Last())
			{
			case "Tonic":
				c = Color.Red;
				break;
			case "Remedy":
				c = Color.LimeGreen;
				break;
			case "Coffee":
			case "Cola":
			case "Espresso":
				c = new Color(46, 20, 0);
				break;
			case "Wine":
				c = Color.Purple;
				break;
			case "Beer":
				c = Color.Orange;
				break;
			case "Milk":
				c = Color.White;
				break;
			case "Juice":
			case "Tea":
				c = Color.LightGreen;
				break;
			case "Mayonnaise":
				c = ((who.itemToEat.Name == "Void Mayonnaise") ? Color.Black : Color.White);
				break;
			case "Soup":
				c = Color.LightGreen;
				break;
			}
		}
		if (Game1.currentLocation == who.currentLocation)
		{
			Game1.playSound((who.itemToEat is Object o && o.preserve.Value == Object.PreserveType.Pickle) ? "eat" : "gulp");
		}
		who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(653, 858, 1, 1), 9999f, 1, 1, who.Position + new Vector2(32 + Game1.random.Next(-2, 3) * 4, -48f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.001f, 0.04f, c, 5f, 0f, 0f, 0f)
		{
			acceleration = new Vector2(0f, 0.5f)
		});
	}

	public void handleDisconnect()
	{
		if (base.currentLocation != null)
		{
			this.rightRing.Value?.onLeaveLocation(this, base.currentLocation);
			this.leftRing.Value?.onLeaveLocation(this, base.currentLocation);
		}
		this.UnapplyAllTrinketEffects();
		this.disconnectDay.Value = (int)Game1.stats.DaysPlayed;
		this.disconnectLocation.Value = base.currentLocation?.NameOrUniqueName ?? "";
		this.disconnectPosition.Value = base.Position;
	}

	public bool isDivorced()
	{
		foreach (Friendship value in this.friendshipData.Values)
		{
			if (value.IsDivorced())
			{
				return true;
			}
		}
		return false;
	}

	public void wipeExMemories()
	{
		foreach (string npcName in this.friendshipData.Keys)
		{
			Friendship friendship = this.friendshipData[npcName];
			if (friendship.IsDivorced())
			{
				friendship.Clear();
				NPC n = Game1.getCharacterFromName(npcName);
				if (n != null)
				{
					n.CurrentDialogue.Clear();
					n.CurrentDialogue.Push(n.TryGetDialogue("WipedMemory") ?? new Dialogue(n, "Strings\\Characters:WipedMemory"));
					Game1.stats.Increment("exMemoriesWiped");
				}
			}
		}
	}

	public void getRidOfChildren()
	{
		FarmHouse farmhouse = Utility.getHomeOfFarmer(this);
		for (int i = farmhouse.characters.Count - 1; i >= 0; i--)
		{
			if (farmhouse.characters[i] is Child child)
			{
				farmhouse.GetChildBed((int)child.Gender)?.mutex.ReleaseLock();
				if (child.hat.Value != null)
				{
					Hat hat = child.hat.Value;
					child.hat.Value = null;
					this.team.returnedDonations.Add(hat);
					this.team.newLostAndFoundItems.Value = true;
				}
				farmhouse.characters.RemoveAt(i);
				Game1.stats.Increment("childrenTurnedToDoves");
			}
		}
	}

	public void animateOnce(int whichAnimation)
	{
		this.FarmerSprite.animateOnce(whichAnimation, 100f, 6);
		this.CanMove = false;
	}

	public static void showItemIntake(Farmer who)
	{
		TemporaryAnimatedSprite tempSprite = null;
		Object toShow = ((!(who.mostRecentlyGrabbedItem is Object grabbedObj)) ? ((who.ActiveObject == null) ? null : who.ActiveObject) : grabbedObj);
		if (toShow == null)
		{
			return;
		}
		ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(toShow.QualifiedItemId);
		string textureName = dataOrErrorItem.TextureName;
		Microsoft.Xna.Framework.Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
		switch (who.FacingDirection)
		{
		case 2:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		case 1:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(28f, -64f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(24f, -72f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(4f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		case 0:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -32f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -43f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -120f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f - 0.001f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		case 3:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 1:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-32f, -64f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 2:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-28f, -76f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 3:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 100f, 1, 0, who.Position + new Vector2(-16f, -128f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 4:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f);
				break;
			case 5:
				tempSprite = new TemporaryAnimatedSprite(textureName, sourceRect, 200f, 1, 0, who.Position + new Vector2(0f, -124f), flicker: false, flipped: false, (float)who.StandingPixel.Y / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.02f, 0f, 0f);
				break;
			}
			break;
		}
		if (toShow.QualifiedItemId == who.ActiveObject?.QualifiedItemId && who.FarmerSprite.currentAnimationIndex == 5)
		{
			tempSprite = null;
		}
		if (tempSprite != null)
		{
			who.currentLocation.temporarySprites.Add(tempSprite);
		}
		if (who.mostRecentlyGrabbedItem is ColoredObject coloredObj && tempSprite != null)
		{
			Microsoft.Xna.Framework.Rectangle coloredSourceRect = ItemRegistry.GetDataOrErrorItem(coloredObj.QualifiedItemId).GetSourceRect(1);
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, coloredSourceRect, tempSprite.interval, 1, 0, tempSprite.Position, flicker: false, flipped: false, tempSprite.layerDepth + 0.0001f, tempSprite.alphaFade, coloredObj.color.Value, 4f, tempSprite.scaleChange, 0f, 0f));
		}
		if (who.FarmerSprite.currentAnimationIndex == 5)
		{
			who.Halt();
			who.FarmerSprite.CurrentAnimation = null;
		}
	}

	public virtual void showSwordSwipe(Farmer who)
	{
		TemporaryAnimatedSprite tempSprite = null;
		Vector2 actionTile = who.GetToolLocation(ignoreClick: true);
		bool dagger = false;
		if (who.CurrentTool is MeleeWeapon weapon)
		{
			dagger = weapon.type.Value == 1;
			if (!dagger)
			{
				weapon.DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
			}
		}
		int min_swipe_interval = 20;
		switch (who.FacingDirection)
		{
		case 2:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.yVelocity = -0.6f;
				}
				break;
			case 1:
				who.yVelocity = (dagger ? 0.5f : (-0.5f));
				break;
			case 5:
				who.yVelocity = 0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(503, 256, 42, 17), who.Position + new Vector2(-16f, -2f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					alpha = 0.5f,
					layerDepth = (who.Position.Y + 64f) / 10000f
				};
				break;
			}
			break;
		case 1:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.xVelocity = 0.6f;
				}
				break;
			case 1:
				who.xVelocity = (dagger ? (-0.5f) : 0.5f);
				break;
			case 5:
				who.xVelocity = -0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(4f, -12f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					alpha = 0.5f
				};
				break;
			}
			break;
		case 3:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.xVelocity = -0.6f;
				}
				break;
			case 1:
				who.xVelocity = (dagger ? 0.5f : (-0.5f));
				break;
			case 5:
				who.xVelocity = 0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(-15f, -12f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					flipped = true,
					alpha = 0.5f
				};
				break;
			}
			break;
		case 0:
			switch (who.FarmerSprite.currentAnimationIndex)
			{
			case 0:
				if (dagger)
				{
					who.yVelocity = 0.6f;
				}
				break;
			case 1:
				who.yVelocity = (dagger ? (-0.5f) : 0.5f);
				break;
			case 5:
				who.yVelocity = -0.3f;
				tempSprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(518, 274, 23, 31), who.Position + new Vector2(0f, -32f) * 4f, flipped: false, 0.07f, Color.White)
				{
					scale = 4f,
					animationLength = 1,
					interval = Math.Max(who.FarmerSprite.CurrentAnimationFrame.milliseconds, min_swipe_interval),
					alpha = 0.5f,
					rotation = 3.926991f
				};
				break;
			}
			break;
		}
		if (tempSprite != null)
		{
			if (who.CurrentTool?.QualifiedItemId == "(W)4")
			{
				tempSprite.color = Color.HotPink;
			}
			who.currentLocation.temporarySprites.Add(tempSprite);
		}
	}

	public static void showToolSwipeEffect(Farmer who)
	{
		if (!(who.CurrentTool is WateringCan))
		{
			switch (who.FacingDirection)
			{
			case 1:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(20f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 3:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(15, who.Position + new Vector2(-92f, -132f), Color.White, 4, flipped: true, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 2:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(19, who.Position + new Vector2(-4f, -128f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 80f : 40f, 0, 128, 1f, 128)
				{
					layerDepth = (float)(who.GetBoundingBox().Bottom + 1) / 10000f
				});
				break;
			case 0:
				who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(18, who.Position + new Vector2(0f, -132f), Color.White, 4, flipped: false, (who.stamina <= 0f) ? 100f : 50f, 0, 64, 1f, 64)
				{
					layerDepth = (float)(who.StandingPixel.Y - 9) / 10000f
				});
				break;
			}
		}
	}

	public static void canMoveNow(Farmer who)
	{
		who.CanMove = true;
		who.UsingTool = false;
		who.usingSlingshot = false;
		who.FarmerSprite.PauseForSingleAnimation = false;
		who.yVelocity = 0f;
		who.xVelocity = 0f;
	}

	public void FireTool()
	{
		this.fireToolEvent.Fire();
	}

	public void synchronizedJump(float velocity)
	{
		if (this.IsLocalPlayer)
		{
			this.synchronizedJumpEvent.Fire(velocity);
			this.synchronizedJumpEvent.Poll();
		}
	}

	protected void performSynchronizedJump(float velocity)
	{
		base.yJumpVelocity = velocity;
		base.yJumpOffset = -1;
	}

	private void performFireTool()
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		this.CurrentTool?.leftClick(this);
	}

	public static void useTool(Farmer who)
	{
		if (who.toolOverrideFunction != null)
		{
			who.toolOverrideFunction(who);
		}
		else if (who.CurrentTool != null)
		{
			float oldStamina = who.stamina;
			if (who.IsLocalPlayer)
			{
				who.CurrentTool.DoFunction(who.currentLocation, (int)who.GetToolLocation().X, (int)who.GetToolLocation().Y, 1, who);
			}
			who.lastClick = Vector2.Zero;
			who.checkForExhaustion(oldStamina);
		}
	}

	public void BeginUsingTool()
	{
		this.beginUsingToolEvent.Fire();
	}

	private void performBeginUsingTool()
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		if (this.CurrentTool != null)
		{
			this.CanMove = false;
			this.UsingTool = true;
			this.canReleaseTool = true;
			this.CurrentTool.beginUsing(base.currentLocation, (int)base.lastClick.X, (int)base.lastClick.Y, this);
		}
	}

	public void EndUsingTool()
	{
		if (this == Game1.player)
		{
			this.endUsingToolEvent.Fire();
		}
		else
		{
			this.performEndUsingTool();
		}
	}

	private void performEndUsingTool()
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		this.CurrentTool?.endUsing(base.currentLocation, this);
	}

	public void checkForExhaustion(float oldStamina)
	{
		if (this.stamina <= 0f && oldStamina > 0f)
		{
			if (!this.exhausted.Value && this.IsLocalPlayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1986"));
			}
			this.setRunning(isRunning: false);
			this.doEmote(36);
		}
		else if (this.stamina <= 15f && oldStamina > 15f && this.IsLocalPlayer)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1987"));
		}
		if (this.stamina <= 0f)
		{
			this.exhausted.Value = true;
		}
	}

	public void setMoving(byte command)
	{
		switch (command)
		{
		case 1:
			if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(0) && !this.movementDirections.Contains(2))
			{
				this.movementDirections.Insert(0, 0);
			}
			break;
		case 2:
			if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(1) && !this.movementDirections.Contains(3))
			{
				this.movementDirections.Insert(0, 1);
			}
			break;
		case 4:
			if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(2) && !this.movementDirections.Contains(0))
			{
				this.movementDirections.Insert(0, 2);
			}
			break;
		case 8:
			if (this.movementDirections.Count < 2 && !this.movementDirections.Contains(3) && !this.movementDirections.Contains(1))
			{
				this.movementDirections.Insert(0, 3);
			}
			break;
		case 33:
			this.movementDirections.Remove(0);
			break;
		case 34:
			this.movementDirections.Remove(1);
			break;
		case 36:
			this.movementDirections.Remove(2);
			break;
		case 40:
			this.movementDirections.Remove(3);
			break;
		case 16:
			this.setRunning(isRunning: true);
			break;
		case 48:
			this.setRunning(isRunning: false);
			break;
		}
		if ((command & 0x40) == 64)
		{
			this.Halt();
			this.running = false;
		}
	}

	public void toolPowerIncrease()
	{
		if (this.CurrentTool is Pan)
		{
			return;
		}
		if (this.toolPower.Value == 0)
		{
			this.toolPitchAccumulator = 0;
		}
		this.toolPower.Value++;
		if (this.CurrentTool is Pickaxe && this.toolPower.Value == 1)
		{
			this.toolPower.Value += 2;
		}
		Color powerUpColor = Color.White;
		int frameOffset = ((this.FacingDirection == 0) ? 4 : ((this.FacingDirection == 2) ? 2 : 0));
		switch (this.toolPower.Value)
		{
		case 1:
			powerUpColor = Color.Orange;
			if (!(this.CurrentTool is WateringCan))
			{
				this.FarmerSprite.CurrentFrame = 72 + frameOffset;
			}
			this.jitterStrength = 0.25f;
			break;
		case 2:
			powerUpColor = Color.LightSteelBlue;
			if (!(this.CurrentTool is WateringCan))
			{
				this.FarmerSprite.CurrentFrame++;
			}
			this.jitterStrength = 0.5f;
			break;
		case 3:
			powerUpColor = Color.Gold;
			this.jitterStrength = 1f;
			break;
		case 4:
			powerUpColor = Color.Violet;
			this.jitterStrength = 2f;
			break;
		case 5:
			powerUpColor = Color.BlueViolet;
			this.jitterStrength = 3f;
			break;
		}
		int xAnimation = ((this.FacingDirection == 1) ? 40 : ((this.FacingDirection == 3) ? (-40) : ((this.FacingDirection == 2) ? 32 : 0)));
		int yAnimation = 192;
		if (this.CurrentTool is WateringCan)
		{
			switch (this.FacingDirection)
			{
			case 3:
				xAnimation = 48;
				break;
			case 1:
				xAnimation = -48;
				break;
			case 2:
				xAnimation = 0;
				break;
			}
			yAnimation = 128;
		}
		int standingY = base.StandingPixel.Y;
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(21, base.Position - new Vector2(xAnimation, yAnimation), powerUpColor, 8, flipped: false, 70f, 0, 64, (float)standingY / 10000f + 0.005f, 128));
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(192, 1152, 64, 64), 50f, 4, 0, base.Position - new Vector2((this.FacingDirection != 1) ? (-64) : 0, 128f), flicker: false, this.FacingDirection == 1, (float)standingY / 10000f, 0.01f, Color.White, 1f, 0f, 0f, 0f));
		int pitch = Utility.CreateRandom(Game1.dayOfMonth, (double)base.Position.X * 1000.0, base.Position.Y).Next(12, 16) * 100 + this.toolPower.Value * 100;
		Game1.playSound("toolCharge", pitch);
	}

	public void UpdateIfOtherPlayer(GameTime time)
	{
		if (base.currentLocation == null)
		{
			return;
		}
		base.position.UpdateExtrapolation(this.getMovementSpeed());
		base.position.Field.InterpolationEnabled = !base.currentLocationRef.IsChanging();
		if (Game1.ShouldShowOnscreenUsernames() && Game1.mouseCursorTransparency > 0f && base.currentLocation == Game1.currentLocation && Game1.currentMinigame == null && Game1.activeClickableMenu == null)
		{
			Vector2 local_position = base.getLocalPosition(Game1.viewport);
			Microsoft.Xna.Framework.Rectangle bounding_rect = new Microsoft.Xna.Framework.Rectangle(0, 0, 128, 192);
			bounding_rect.X = (int)(local_position.X + 32f - (float)(bounding_rect.Width / 2));
			bounding_rect.Y = (int)(local_position.Y - (float)bounding_rect.Height + 48f);
			if (bounding_rect.Contains(Game1.getMouseX(ui_scale: false), Game1.getMouseY(ui_scale: false)))
			{
				this.usernameDisplayTime = 1f;
			}
		}
		if (this._lastSelectedItem != this.CurrentItem)
		{
			this._lastSelectedItem?.actionWhenStopBeingHeld(this);
			this._lastSelectedItem = this.CurrentItem;
		}
		this.fireToolEvent.Poll();
		this.beginUsingToolEvent.Poll();
		this.endUsingToolEvent.Poll();
		this.drinkAnimationEvent.Poll();
		this.eatAnimationEvent.Poll();
		this.sickAnimationEvent.Poll();
		this.passOutEvent.Poll();
		this.doEmoteEvent.Poll();
		this.kissFarmerEvent.Poll();
		this.haltAnimationEvent.Poll();
		this.synchronizedJumpEvent.Poll();
		this.renovateEvent.Poll();
		this.FarmerSprite.checkForSingleAnimation(time);
		this.updateCommon(time, base.currentLocation);
	}

	/// <summary>Put an item into an equipment slot with appropriate updates (e.g. calling <see cref="M:StardewValley.Item.onEquip(StardewValley.Farmer)" /> or <see cref="M:StardewValley.Item.onUnequip(StardewValley.Farmer)" />).</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="newItem">The item to place in the equipment slot, or <c>null</c> to just unequip the old item.</param>
	/// <param name="slot">The equipment slot to update.</param>
	/// <returns>Returns the item that was previously in the equipment slot, or <c>null</c> if it was empty.</returns>
	public TItem Equip<TItem>(TItem newItem, NetRef<TItem> slot) where TItem : Item
	{
		TItem oldItem = slot.Value;
		oldItem?.onDetachedFromParent();
		newItem?.onDetachedFromParent();
		this.Equip(oldItem, newItem, delegate(TItem val)
		{
			slot.Value = val;
		});
		return oldItem;
	}

	/// <summary>Place an item into an equipment slot manually with appropriate updates (e.g. calling <see cref="M:StardewValley.Item.onEquip(StardewValley.Farmer)" /> or <see cref="M:StardewValley.Item.onUnequip(StardewValley.Farmer)" />).</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="oldItem">The item previously in the equipment slot, or <c>null</c> if it was empty.</param>
	/// <param name="newItem">The item to place in the equipment slot, or <c>null</c> to just unequip the old item.</param>
	/// <param name="equip">A callback which equips an item in the slot.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Farmer.Equip``1(``0,Netcode.NetRef{``0})" /> instead. When calling this form, you should call <see cref="M:StardewValley.Item.onDetachedFromParent" /> on the old/new items as needed to avoid warnings.</remarks>
	public void Equip<TItem>(TItem oldItem, TItem newItem, Action<TItem> equip) where TItem : Item
	{
		bool raiseEvents = Game1.hasLoadedGame && Game1.dayOfMonth > 0 && this.IsLocalPlayer;
		if (raiseEvents)
		{
			oldItem?.onUnequip(this);
		}
		equip(newItem);
		if (newItem != null)
		{
			newItem.HasBeenInInventory = true;
			if (raiseEvents)
			{
				newItem.onEquip(this);
			}
		}
		if ((oldItem?.HasEquipmentBuffs() ?? false) || !((!(newItem?.HasEquipmentBuffs())) ?? true))
		{
			this.buffs.Dirty = true;
		}
	}

	public void forceCanMove()
	{
		this.forceTimePass = false;
		this.movementDirections.Clear();
		this.isEating = false;
		this.CanMove = true;
		Game1.freezeControls = false;
		this.freezePause = 0;
		this.UsingTool = false;
		this.usingSlingshot = false;
		this.FarmerSprite.PauseForSingleAnimation = false;
		if (this.CurrentTool is FishingRod rod)
		{
			rod.isFishing = false;
		}
	}

	public void dropItem(Item i)
	{
		if (i != null && i.canBeDropped())
		{
			Game1.createItemDebris(i.getOne(), base.getStandingPosition(), this.FacingDirection);
		}
	}

	public bool addEvent(string eventName, int daysActive)
	{
		return this.activeDialogueEvents.TryAdd(eventName, daysActive);
	}

	public Vector2 getMostRecentMovementVector()
	{
		return new Vector2(base.Position.X - this.lastPosition.X, base.Position.Y - this.lastPosition.Y);
	}

	public int GetSkillLevel(int index)
	{
		return index switch
		{
			0 => this.FarmingLevel, 
			3 => this.MiningLevel, 
			1 => this.FishingLevel, 
			2 => this.ForagingLevel, 
			5 => this.LuckLevel, 
			4 => this.CombatLevel, 
			_ => 0, 
		};
	}

	public int GetUnmodifiedSkillLevel(int index)
	{
		return index switch
		{
			0 => this.farmingLevel.Value, 
			3 => this.miningLevel.Value, 
			1 => this.fishingLevel.Value, 
			2 => this.foragingLevel.Value, 
			5 => this.luckLevel.Value, 
			4 => this.combatLevel.Value, 
			_ => 0, 
		};
	}

	public static string getSkillNameFromIndex(int index)
	{
		return index switch
		{
			0 => "Farming", 
			3 => "Mining", 
			1 => "Fishing", 
			2 => "Foraging", 
			5 => "Luck", 
			4 => "Combat", 
			_ => "", 
		};
	}

	public static int getSkillNumberFromName(string name)
	{
		return name.ToLower() switch
		{
			"farming" => 0, 
			"mining" => 3, 
			"fishing" => 1, 
			"foraging" => 2, 
			"luck" => 5, 
			"combat" => 4, 
			_ => -1, 
		};
	}

	public bool setSkillLevel(string nameOfSkill, int level)
	{
		int skillIndex = Farmer.getSkillNumberFromName(nameOfSkill);
		switch (nameOfSkill)
		{
		case "Farming":
			if (this.farmingLevel.Value < level)
			{
				this.newLevels.Add(new Point(skillIndex, level - this.farmingLevel.Value));
				this.farmingLevel.Value = level;
				this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Fishing":
			if (this.fishingLevel.Value < level)
			{
				this.newLevels.Add(new Point(skillIndex, level - this.fishingLevel.Value));
				this.fishingLevel.Value = level;
				this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Foraging":
			if (this.foragingLevel.Value < level)
			{
				this.newLevels.Add(new Point(skillIndex, level - this.foragingLevel.Value));
				this.foragingLevel.Value = level;
				this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Mining":
			if (this.miningLevel.Value < level)
			{
				this.newLevels.Add(new Point(skillIndex, level - this.miningLevel.Value));
				this.miningLevel.Value = level;
				this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
				return true;
			}
			break;
		case "Combat":
			if (this.combatLevel.Value < level)
			{
				this.newLevels.Add(new Point(skillIndex, level - this.combatLevel.Value));
				this.combatLevel.Value = level;
				this.experiencePoints[skillIndex] = Farmer.getBaseExperienceForLevel(level);
				return true;
			}
			break;
		}
		return false;
	}

	public static string getSkillDisplayNameFromIndex(int index)
	{
		return index switch
		{
			0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1991"), 
			3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1992"), 
			1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1993"), 
			2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1994"), 
			5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1995"), 
			4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1996"), 
			_ => "", 
		};
	}

	public bool hasCompletedCommunityCenter()
	{
		if (this.mailReceived.Contains("ccBoilerRoom") && this.mailReceived.Contains("ccCraftsRoom") && this.mailReceived.Contains("ccPantry") && this.mailReceived.Contains("ccFishTank") && this.mailReceived.Contains("ccVault"))
		{
			return this.mailReceived.Contains("ccBulletin");
		}
		return false;
	}

	private bool localBusMoving()
	{
		GameLocation gameLocation = base.currentLocation;
		if (!(gameLocation is Desert desert))
		{
			if (gameLocation is BusStop busStop)
			{
				if (!busStop.drivingOff)
				{
					return busStop.drivingBack;
				}
				return true;
			}
			return false;
		}
		if (!desert.drivingOff)
		{
			return desert.drivingBack;
		}
		return true;
	}

	public virtual bool CanBeDamaged()
	{
		if (!this.IsDedicatedPlayer && !this.temporarilyInvincible && !this.isEating && !Game1.fadeToBlack)
		{
			return !this.hasBuff("21");
		}
		return false;
	}

	public void takeDamage(int damage, bool overrideParry, Monster damager)
	{
		if (Game1.eventUp || this.IsDedicatedPlayer || this.FarmerSprite.isPassingOut() || (this.isInBed.Value && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog))
		{
			return;
		}
		bool num = damager != null && !damager.isInvincible() && !overrideParry;
		bool monsterDamageCapable = (damager == null || !damager.isInvincible()) && (damager == null || (!(damager is GreenSlime) && !(damager is BigSlime)) || !this.isWearingRing("520"));
		bool playerParryable = this.CurrentTool is MeleeWeapon && ((MeleeWeapon)this.CurrentTool).isOnSpecial && ((MeleeWeapon)this.CurrentTool).type.Value == 3;
		bool playerDamageable = this.CanBeDamaged();
		if (num && playerParryable)
		{
			Rumble.rumble(0.75f, 150f);
			base.playNearbySoundAll("parry");
			damager.parried(damage, this);
		}
		else
		{
			if (!(monsterDamageCapable && playerDamageable))
			{
				return;
			}
			damager?.onDealContactDamage(this);
			damage += Game1.random.Next(Math.Min(-1, -damage / 8), Math.Max(1, damage / 8));
			int defense = this.buffs.Defense;
			if (this.stats.Get("Book_Defense") != 0)
			{
				defense++;
			}
			if ((float)defense >= (float)damage * 0.5f)
			{
				defense -= (int)((float)defense * (float)Game1.random.Next(3) / 10f);
			}
			if (damager != null && this.isWearingRing("839"))
			{
				Microsoft.Xna.Framework.Rectangle monsterBox = damager.GetBoundingBox();
				_ = Utility.getAwayFromPlayerTrajectory(monsterBox, this) / 2f;
				int damageToMonster = damage;
				int farmerDamage = Math.Max(1, damage - defense);
				if (farmerDamage < 10)
				{
					damageToMonster = (int)Math.Ceiling((double)(damageToMonster + farmerDamage) / 2.0);
				}
				int multiplier = this.getNumberOfWornRingsWithID("839");
				damageToMonster *= multiplier;
				base.currentLocation?.damageMonster(monsterBox, damageToMonster, damageToMonster + 1, isBomb: false, this);
			}
			if (this.isWearingRing("524") && !this.hasBuff("21") && Game1.random.NextDouble() < (0.9 - (double)((float)this.health / 100f)) / (double)(3 - this.LuckLevel / 10) + ((this.health <= 15) ? 0.2 : 0.0))
			{
				base.playNearbySoundAll("yoba");
				this.applyBuff("21");
				return;
			}
			Rumble.rumble(0.75f, 150f);
			damage = Math.Max(1, damage - defense);
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is MineShaft && Game1.mine.getMineArea() == 121)
			{
				float adjustment = 1f;
				if (this.team.calicoStatueEffects.TryGetValue(8, out var sharpTeethAmount))
				{
					adjustment += (float)sharpTeethAmount * 0.25f;
				}
				if (this.team.calicoStatueEffects.TryGetValue(14, out var toothFileAmount))
				{
					adjustment -= (float)toothFileAmount * 0.25f;
				}
				damage = Math.Max(1, (int)((float)damage * adjustment));
			}
			this.health = Math.Max(0, this.health - damage);
			foreach (Trinket trinketItem in this.trinketItems)
			{
				trinketItem?.OnReceiveDamage(this, damage);
			}
			if (this.health <= 0 && this.GetEffectsOfRingMultiplier("863") > 0 && !this.hasUsedDailyRevive.Value)
			{
				base.startGlowing(new Color(255, 255, 0), border: false, 0.25f);
				DelayedAction.functionAfterDelay(base.stopGlowing, 500);
				Game1.playSound("yoba");
				for (int i = 0; i < 13; i++)
				{
					float xPos = Game1.random.Next(-32, 33);
					base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(114, 46, 2, 2), 200f, 5, 1, new Vector2(xPos + 32f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						attachedCharacter = this,
						positionFollowsAttachedCharacter = true,
						motion = new Vector2(xPos / 32f, -3f),
						delayBeforeAnimationStart = i * 50,
						alphaFade = 0.001f,
						acceleration = new Vector2(0f, 0.1f)
					});
				}
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(157, 280, 28, 19), 2000f, 1, 1, new Vector2(-20f, -16f), flicker: false, flipped: false, 1E-06f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					attachedCharacter = this,
					positionFollowsAttachedCharacter = true,
					alpha = 0.1f,
					alphaFade = -0.01f,
					alphaFadeFade = -0.00025f
				});
				this.health = (int)Math.Min(this.maxHealth, (float)this.maxHealth * 0.5f + (float)this.GetEffectsOfRingMultiplier("863"));
				this.hasUsedDailyRevive.Value = true;
			}
			this.temporarilyInvincible = true;
			this.flashDuringThisTemporaryInvincibility = true;
			this.temporaryInvincibilityTimer = 0;
			this.currentTemporaryInvincibilityDuration = 1200 + this.GetEffectsOfRingMultiplier("861") * 400;
			Point standingPixel = base.StandingPixel;
			base.currentLocation.debris.Add(new Debris(damage, new Vector2(standingPixel.X + 8, standingPixel.Y), Color.Red, 1f, this));
			base.playNearbySoundAll("ow");
			Game1.hitShakeTimer = 100 * damage;
		}
	}

	public int GetEffectsOfRingMultiplier(string ringId)
	{
		int count = 0;
		if (this.leftRing.Value != null)
		{
			count += this.leftRing.Value.GetEffectsOfRingMultiplier(ringId);
		}
		if (this.rightRing.Value != null)
		{
			count += this.rightRing.Value.GetEffectsOfRingMultiplier(ringId);
		}
		return count;
	}

	private void checkDamage(GameLocation location)
	{
		if (Game1.eventUp)
		{
			return;
		}
		for (int i = location.characters.Count - 1; i >= 0; i--)
		{
			if (i < location.characters.Count && location.characters[i] is Monster monster && monster.OverlapsFarmerForDamage(this))
			{
				monster.currentLocation = location;
				monster.collisionWithFarmerBehavior();
				if (monster.DamageToFarmer > 0)
				{
					if (this.CurrentTool is MeleeWeapon && ((MeleeWeapon)this.CurrentTool).isOnSpecial && ((MeleeWeapon)this.CurrentTool).type.Value == 3)
					{
						this.takeDamage(monster.DamageToFarmer, overrideParry: false, monster);
					}
					else
					{
						this.takeDamage(Math.Max(1, monster.DamageToFarmer + Game1.random.Next(-monster.DamageToFarmer / 4, monster.DamageToFarmer / 4)), overrideParry: false, monster);
					}
				}
			}
		}
	}

	public bool checkAction(Farmer who, GameLocation location)
	{
		if (who.isRidingHorse())
		{
			who.Halt();
		}
		if (this.hidden.Value)
		{
			return false;
		}
		if (Game1.CurrentEvent != null)
		{
			if (Game1.CurrentEvent.isSpecificFestival("spring24") && who.dancePartner.Value == null)
			{
				who.Halt();
				who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				string question = Game1.content.LoadString("Strings\\UI:AskToDance_" + (this.IsMale ? "Male" : "Female"), base.Name);
				location.createQuestionDialogue(question, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Dance);
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
			return false;
		}
		if (who.CurrentItem != null && who.CurrentItem.QualifiedItemId == "(O)801" && !this.isMarriedOrRoommates() && !this.isEngaged() && !who.isMarriedOrRoommates() && !who.isEngaged())
		{
			who.Halt();
			who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
			string question2 = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (this.IsMale ? "Male" : "Female"), base.Name);
			location.createQuestionDialogue(question2, location.createYesNoResponses(), delegate(Farmer _, string answer)
			{
				if (answer == "Yes")
				{
					who.team.SendProposal(this, ProposalType.Marriage, who.CurrentItem.getOne());
					Game1.activeClickableMenu = new PendingProposalDialog();
				}
			});
			return true;
		}
		if (who.CanMove)
		{
			bool? flag = who.ActiveObject?.canBeGivenAsGift();
			if (flag.HasValue && flag == true && !who.ActiveObject.questItem.Value)
			{
				who.Halt();
				who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				string question3 = Game1.content.LoadString("Strings\\UI:GiftPlayerItem_" + (this.IsMale ? "Male" : "Female"), who.ActiveObject.DisplayName, base.Name);
				location.createQuestionDialogue(question3, location.createYesNoResponses(), delegate(Farmer _, string answer)
				{
					if (answer == "Yes")
					{
						who.team.SendProposal(this, ProposalType.Gift, who.ActiveObject.getOne());
						Game1.activeClickableMenu = new PendingProposalDialog();
					}
				});
				return true;
			}
		}
		long? playerSpouseID = this.team.GetSpouse(this.UniqueMultiplayerID);
		if ((playerSpouseID.HasValue & (who.UniqueMultiplayerID == playerSpouseID)) && who.CanMove && !who.isMoving() && !this.isMoving() && Utility.IsHorizontalDirection(base.getGeneralDirectionTowards(who.getStandingPosition(), -10, opposite: false, useTileCalculations: false)))
		{
			who.Halt();
			who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
			who.kissFarmerEvent.Fire(this.UniqueMultiplayerID);
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				motion = new Vector2(0f, -0.5f),
				alphaFade = 0.01f
			});
			base.playNearbySoundAll("dwop", null, SoundContext.NPC);
			return true;
		}
		return false;
	}

	public void Update(GameTime time, GameLocation location)
	{
		if (this._lastEquippedTool != this.CurrentTool)
		{
			this.Equip(this._lastEquippedTool, this.CurrentTool, delegate(Tool tool)
			{
				this._lastEquippedTool = tool;
			});
		}
		this.buffs.SetOwner(this);
		this.buffs.Update(time);
		base.position.UpdateExtrapolation(this.getMovementSpeed());
		this.fireToolEvent.Poll();
		this.beginUsingToolEvent.Poll();
		this.endUsingToolEvent.Poll();
		this.drinkAnimationEvent.Poll();
		this.eatAnimationEvent.Poll();
		this.sickAnimationEvent.Poll();
		this.passOutEvent.Poll();
		this.doEmoteEvent.Poll();
		this.kissFarmerEvent.Poll();
		this.synchronizedJumpEvent.Poll();
		this.renovateEvent.Poll();
		if (this.IsLocalPlayer)
		{
			if (base.currentLocation == null)
			{
				return;
			}
			this.hidden.Value = this.IsDedicatedPlayer || this.localBusMoving() || (location.currentEvent != null && !location.currentEvent.isFestival) || (location.currentEvent != null && location.currentEvent.doingSecretSanta) || Game1.locationRequest != null || !Game1.displayFarmer;
			this.isInBed.Value = base.currentLocation.doesTileHaveProperty(base.TilePoint.X, base.TilePoint.Y, "Bed", "Back") != null || this.sleptInTemporaryBed.Value;
			if (!Game1.options.allowStowing)
			{
				this.netItemStowed.Value = false;
			}
			this.hasMenuOpen.Value = Game1.activeClickableMenu != null;
		}
		if (this.IsSitting())
		{
			this.movementDirections.Clear();
			if (this.IsSitting() && !this.isStopSitting)
			{
				if (!this.sittingFurniture.IsSeatHere(base.currentLocation))
				{
					this.StopSitting(animate: false);
				}
				else if (this.sittingFurniture is MapSeat mapSeat)
				{
					if (!base.currentLocation.mapSeats.Contains(this.sittingFurniture))
					{
						this.StopSitting(animate: false);
					}
					else if (mapSeat.IsBlocked(base.currentLocation))
					{
						this.StopSitting();
					}
				}
			}
		}
		if (Game1.CurrentEvent == null && !this.bathingClothes.Value && !this.onBridge.Value)
		{
			this.canOnlyWalk = false;
		}
		if (this.noMovementPause > 0)
		{
			this.CanMove = false;
			this.noMovementPause -= time.ElapsedGameTime.Milliseconds;
			if (this.noMovementPause <= 0)
			{
				this.CanMove = true;
			}
		}
		if (this.freezePause > 0)
		{
			this.CanMove = false;
			this.freezePause -= time.ElapsedGameTime.Milliseconds;
			if (this.freezePause <= 0)
			{
				this.CanMove = true;
			}
		}
		if (this.sparklingText != null && this.sparklingText.update(time))
		{
			this.sparklingText = null;
		}
		if (this.newLevelSparklingTexts.Count > 0 && this.sparklingText == null && !this.UsingTool && this.CanMove && Game1.activeClickableMenu == null)
		{
			this.sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2003", Farmer.getSkillDisplayNameFromIndex(this.newLevelSparklingTexts.Peek())), Color.White, Color.White, rainbow: true);
			this.newLevelSparklingTexts.Dequeue();
		}
		if (this.lerpPosition >= 0f)
		{
			this.lerpPosition += (float)time.ElapsedGameTime.TotalSeconds;
			if (this.lerpPosition >= this.lerpDuration)
			{
				this.lerpPosition = this.lerpDuration;
			}
			base.Position = new Vector2(Utility.Lerp(this.lerpStartPosition.X, this.lerpEndPosition.X, this.lerpPosition / this.lerpDuration), Utility.Lerp(this.lerpStartPosition.Y, this.lerpEndPosition.Y, this.lerpPosition / this.lerpDuration));
			if (this.lerpPosition >= this.lerpDuration)
			{
				this.lerpPosition = -1f;
			}
		}
		if (this.isStopSitting && this.lerpPosition < 0f)
		{
			this.isStopSitting = false;
			if (this.sittingFurniture != null)
			{
				this.mapChairSitPosition.Value = new Vector2(-1f, -1f);
				this.sittingFurniture.RemoveSittingFarmer(this);
				this.sittingFurniture = null;
				this.isSitting.Value = false;
			}
		}
		if (this.isInBed.Value && Game1.IsMultiplayer && Game1.shouldTimePass())
		{
			this.regenTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.regenTimer < 0)
			{
				this.regenTimer = 500;
				if (this.stamina < (float)this.MaxStamina)
				{
					this.stamina++;
				}
				if (this.health < this.maxHealth)
				{
					this.health++;
				}
			}
		}
		this.FarmerSprite.checkForSingleAnimation(time);
		if (this.CanMove)
		{
			this.rotation = 0f;
			if (this.health <= 0 && !Game1.killScreen && Game1.timeOfDay < 2600)
			{
				if (this.IsSitting())
				{
					this.StopSitting(animate: false);
				}
				this.CanMove = false;
				Game1.screenGlowOnce(Color.Red, hold: true);
				Game1.killScreen = true;
				this.faceDirection(2);
				this.FarmerSprite.setCurrentFrame(5);
				this.jitterStrength = 1f;
				Game1.pauseTime = 3000f;
				Rumble.rumbleAndFade(0.75f, 1500f);
				this.freezePause = 8000;
				if (Game1.currentSong != null && Game1.currentSong.IsPlaying)
				{
					Game1.currentSong.Stop(AudioStopOptions.Immediate);
				}
				Game1.changeMusicTrack("silence");
				base.playNearbySoundAll("death");
				Game1.dialogueUp = false;
				Game1.stats.TimesUnconscious++;
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.player.currentLocation is MineShaft && Game1.mine.getMineArea() == 121)
				{
					int eggsRemoved = 0;
					float eggPercentToRemove = 0.2f;
					if (Game1.player.team.calicoStatueEffects.ContainsKey(5))
					{
						eggPercentToRemove = 0.5f;
					}
					eggsRemoved = (int)(eggPercentToRemove * (float)Game1.player.getItemCount("CalicoEgg"));
					Game1.player.Items.ReduceId("CalicoEgg", eggsRemoved);
					this.itemsLostLastDeath.Clear();
					if (eggsRemoved > 0)
					{
						this.itemsLostLastDeath.Add(new Object("CalicoEgg", eggsRemoved));
					}
				}
				if (Game1.activeClickableMenu is GameMenu)
				{
					Game1.activeClickableMenu.emergencyShutDown();
					Game1.activeClickableMenu = null;
				}
			}
			if (this.collisionNPC != null)
			{
				this.collisionNPC.farmerPassesThrough = true;
			}
			NPC collider;
			if (this.movementDirections.Count > 0 && !this.isRidingHorse() && (collider = location.isCollidingWithCharacter(this.nextPosition(this.FacingDirection))) != null)
			{
				this.charactercollisionTimer += time.ElapsedGameTime.Milliseconds;
				if (this.charactercollisionTimer > collider.getTimeFarmerMustPushBeforeStartShaking())
				{
					collider.shake(50);
				}
				if (this.charactercollisionTimer >= collider.getTimeFarmerMustPushBeforePassingThrough() && this.collisionNPC == null)
				{
					this.collisionNPC = collider;
					if (this.collisionNPC.Name.Equals("Bouncer") && base.currentLocation != null && base.currentLocation.name.Equals("SandyHouse"))
					{
						this.collisionNPC.showTextAboveHead(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2010"));
						this.collisionNPC = null;
						this.charactercollisionTimer = 0;
					}
					else if (this.collisionNPC.name.Equals("Henchman") && base.currentLocation != null && base.currentLocation.name.Equals("WitchSwamp"))
					{
						this.collisionNPC = null;
						this.charactercollisionTimer = 0;
					}
					else if (this.collisionNPC is Raccoon)
					{
						this.collisionNPC = null;
						this.charactercollisionTimer = 0;
					}
				}
			}
			else
			{
				this.charactercollisionTimer = 0;
				if (this.collisionNPC != null && location.isCollidingWithCharacter(this.nextPosition(this.FacingDirection)) == null)
				{
					this.collisionNPC.farmerPassesThrough = false;
					this.collisionNPC = null;
				}
			}
		}
		if (Game1.shouldTimePass())
		{
			MeleeWeapon.weaponsTypeUpdate(time);
		}
		if (!Game1.eventUp || this.movementDirections.Count <= 0 || base.currentLocation.currentEvent == null || base.currentLocation.currentEvent.playerControlSequence || (base.controller != null && base.controller.allowPlayerPathingInEvent))
		{
			this.lastPosition = base.Position;
			if (base.controller != null)
			{
				if (base.controller.update(time))
				{
					base.controller = null;
				}
			}
			else if (base.controller == null)
			{
				this.MovePosition(time, Game1.viewport, location);
			}
		}
		if (Game1.actionsWhenPlayerFree.Count > 0 && this.IsLocalPlayer && !this.IsBusyDoingSomething())
		{
			Action action = Game1.actionsWhenPlayerFree[0];
			Game1.actionsWhenPlayerFree.RemoveAt(0);
			action();
		}
		this.updateCommon(time, location);
		base.position.Paused = this.FarmerSprite.PauseForSingleAnimation || (this.UsingTool && !this.canStrafeForToolUse()) || this.isEating;
		this.checkDamage(location);
	}

	private void updateCommon(GameTime time, GameLocation location)
	{
		if (this.usernameDisplayTime > 0f)
		{
			this.usernameDisplayTime -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.usernameDisplayTime < 0f)
			{
				this.usernameDisplayTime = 0f;
			}
		}
		if (this.jitterStrength > 0f)
		{
			this.jitter = new Vector2((float)Game1.random.Next(-(int)(this.jitterStrength * 100f), (int)((this.jitterStrength + 1f) * 100f)) / 100f, (float)Game1.random.Next(-(int)(this.jitterStrength * 100f), (int)((this.jitterStrength + 1f) * 100f)) / 100f);
		}
		if (this._wasSitting != this.isSitting.Value)
		{
			if (this._wasSitting)
			{
				this.yOffset = 0f;
				this.xOffset = 0f;
			}
			this._wasSitting = this.isSitting.Value;
		}
		if (base.yJumpOffset != 0)
		{
			base.yJumpVelocity -= ((this.UsingTool && this.canStrafeForToolUse() && (this.movementDirections.Count > 0 || (!this.IsLocalPlayer && base.IsRemoteMoving()))) ? 0.25f : 0.5f);
			base.yJumpOffset -= (int)base.yJumpVelocity;
			if (base.yJumpOffset >= 0)
			{
				base.yJumpOffset = 0;
				base.yJumpVelocity = 0f;
			}
		}
		this.updateMovementAnimation(time);
		base.updateEmote(time);
		base.updateGlow();
		base.currentLocationRef.Update();
		if (this.exhausted.Value && this.stamina <= 1f)
		{
			this.currentEyes = 4;
			this.blinkTimer = -1000;
		}
		this.blinkTimer += time.ElapsedGameTime.Milliseconds;
		if (this.blinkTimer > 2200 && Game1.random.NextDouble() < 0.01)
		{
			this.blinkTimer = -150;
			this.currentEyes = 4;
		}
		else if (this.blinkTimer > -100)
		{
			if (this.blinkTimer < -50)
			{
				this.currentEyes = 1;
			}
			else if (this.blinkTimer < 0)
			{
				this.currentEyes = 4;
			}
			else
			{
				this.currentEyes = 0;
			}
		}
		if (this.isCustomized.Value && this.isInBed.Value && !Game1.eventUp && ((this.timerSinceLastMovement >= 3000 && Game1.timeOfDay >= 630) || this.timeWentToBed.Value != 0))
		{
			this.currentEyes = 1;
			this.blinkTimer = -10;
		}
		this.UpdateItemStow();
		if (base.swimming.Value)
		{
			this.yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
			int oldSwimTimer = this.swimTimer;
			this.swimTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.timerSinceLastMovement == 0)
			{
				if (oldSwimTimer > 400 && this.swimTimer <= 400 && this.IsLocalPlayer)
				{
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(base.xVelocity) + Math.Abs(base.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
				}
				if (this.swimTimer < 0)
				{
					this.swimTimer = 800;
					if (this.IsLocalPlayer)
					{
						base.playNearbySoundAll("slosh");
						Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(base.xVelocity) + Math.Abs(base.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					}
				}
			}
			else if (!Game1.eventUp && (Game1.activeClickableMenu == null || Game1.IsMultiplayer) && !Game1.paused)
			{
				if (this.timerSinceLastMovement > 800)
				{
					this.currentEyes = 1;
				}
				else if (this.timerSinceLastMovement > 700)
				{
					this.currentEyes = 4;
				}
				if (this.swimTimer < 0)
				{
					this.swimTimer = 100;
					if (this.Stamina < (float)this.MaxStamina)
					{
						this.Stamina++;
					}
					if (this.health < this.maxHealth)
					{
						this.health++;
					}
				}
			}
		}
		if (!this.isMoving())
		{
			this.timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
		}
		else
		{
			this.timerSinceLastMovement = 0;
		}
		for (int i = this.Items.Count - 1; i >= 0; i--)
		{
			if (this.Items[i] is Tool tool)
			{
				tool.tickUpdate(time, this);
			}
		}
		if (this.TemporaryItem is Tool tempTool)
		{
			tempTool.tickUpdate(time, this);
		}
		this.rightRing.Value?.update(time, location, this);
		this.leftRing.Value?.update(time, location, this);
		if (Game1.shouldTimePass() && this.IsLocalPlayer)
		{
			foreach (Trinket trinketItem in this.trinketItems)
			{
				trinketItem?.Update(this, time, location);
			}
		}
		this.mount?.update(time, location);
		this.mount?.SyncPositionToRider();
		foreach (Companion companion in this.companions)
		{
			companion.Update(time, location);
		}
	}

	/// <summary>Get whether the player is engaged in any action and shouldn't be interrupted. This includes viewing a menu or event, fading to black, warping, using a tool, etc. If this returns false, we should be free to interrupt the player.</summary>
	public virtual bool IsBusyDoingSomething()
	{
		if (Game1.eventUp)
		{
			return true;
		}
		if (Game1.fadeToBlack)
		{
			return true;
		}
		if (Game1.currentMinigame != null)
		{
			return true;
		}
		if (Game1.activeClickableMenu != null)
		{
			return true;
		}
		if (Game1.isWarping)
		{
			return true;
		}
		if (this.UsingTool)
		{
			return true;
		}
		if (Game1.killScreen)
		{
			return true;
		}
		if (this.freezePause > 0)
		{
			return true;
		}
		if (!this.CanMove)
		{
			return true;
		}
		if (this.FarmerSprite.PauseForSingleAnimation)
		{
			return true;
		}
		if (this.usingSlingshot)
		{
			return true;
		}
		return false;
	}

	public void UpdateItemStow()
	{
		if (this._itemStowed != this.netItemStowed.Value)
		{
			if (this.netItemStowed.Value && this.ActiveObject != null)
			{
				this.ActiveObject.actionWhenStopBeingHeld(this);
			}
			this._itemStowed = this.netItemStowed.Value;
			if (!this.netItemStowed.Value)
			{
				this.ActiveObject?.actionWhenBeingHeld(this);
			}
		}
	}

	/// <summary>Add a quest to the player's quest log, or log a warning if it doesn't exist.</summary>
	/// <param name="questId">The quest ID in <c>Data/Quests</c>.</param>
	public void addQuest(string questId)
	{
		if (this.hasQuest(questId))
		{
			return;
		}
		Quest quest = Quest.getQuestFromId(questId);
		if (quest == null)
		{
			Game1.log.Warn("Can't add quest with ID '" + questId + "' because no such ID was found.");
			return;
		}
		this.questLog.Add(quest);
		if (!quest.IsHidden())
		{
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2011"), 2));
		}
		foreach (string key in Game1.player.team.constructedBuildings)
		{
			quest.OnBuildingExists(key);
		}
	}

	public void removeQuest(string questID)
	{
		this.questLog.RemoveWhere((Quest quest) => quest.id.Value == questID);
	}

	public void completeQuest(string questID)
	{
		for (int i = this.questLog.Count - 1; i >= 0; i--)
		{
			if (this.questLog[i].id.Value == questID)
			{
				this.questLog[i].questComplete();
			}
		}
	}

	public bool hasQuest(string id)
	{
		for (int i = this.questLog.Count - 1; i >= 0; i--)
		{
			if (this.questLog[i].id.Value == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool hasNewQuestActivity()
	{
		foreach (SpecialOrder o in this.team.specialOrders)
		{
			if (!o.IsHidden() && (o.ShouldDisplayAsNew() || o.ShouldDisplayAsComplete()))
			{
				return true;
			}
		}
		foreach (Quest q in this.questLog)
		{
			if (!q.IsHidden() && (q.showNew.Value || (q.completed.Value && !q.destroy.Value)))
			{
				return true;
			}
		}
		return false;
	}

	public float getMovementSpeed()
	{
		if (this.UsingTool && this.canStrafeForToolUse())
		{
			return 2f;
		}
		if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
		{
			this.movementMultiplier = 0.066f;
			float movementSpeed = 1f;
			movementSpeed = ((!this.isRidingHorse()) ? Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : (this.addedSpeed + this.temporarySpeedBuff))) * this.movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds) : Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : (this.addedSpeed + 4.6f + (this.mount.ateCarrotToday ? 0.4f : 0f) + ((this.stats.Get("Book_Horse") != 0) ? 0.5f : 0f)))) * this.movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds));
			if (this.movementDirections.Count > 1)
			{
				movementSpeed *= 0.707f;
			}
			if (Game1.CurrentEvent == null && this.hasBuff("19"))
			{
				movementSpeed = 0f;
			}
			return movementSpeed;
		}
		float movementSpeed2 = Math.Max(1f, (float)base.speed + (Game1.eventUp ? ((float)Math.Max(0, Game1.CurrentEvent.farmerAddedSpeed - 2)) : (this.addedSpeed + (this.isRidingHorse() ? 5f : this.temporarySpeedBuff))));
		if (this.movementDirections.Count > 1)
		{
			movementSpeed2 = Math.Max(1, (int)Math.Sqrt(2f * (movementSpeed2 * movementSpeed2)) / 2);
		}
		return movementSpeed2;
	}

	public bool isWearingRing(string itemId)
	{
		if (this.rightRing.Value == null || !this.rightRing.Value.GetsEffectOfRing(itemId))
		{
			if (this.leftRing.Value != null)
			{
				return this.leftRing.Value.GetsEffectOfRing(itemId);
			}
			return false;
		}
		return true;
	}

	public int getNumberOfWornRingsWithID(string itemId)
	{
		int num = 0;
		if (this.rightRing.Value != null && this.rightRing.Value.GetsEffectOfRing(itemId))
		{
			num++;
		}
		if (this.leftRing.Value != null && this.leftRing.Value.GetsEffectOfRing(itemId))
		{
			num++;
		}
		return num;
	}

	public override void Halt()
	{
		if (!this.FarmerSprite.PauseForSingleAnimation && !this.isRidingHorse() && !this.UsingTool)
		{
			base.Halt();
		}
		this.movementDirections.Clear();
		if (!this.isEmoteAnimating && !this.UsingTool)
		{
			this.stopJittering();
		}
		this.armOffset = Vector2.Zero;
		if (this.isRidingHorse())
		{
			this.mount.Halt();
			this.mount.Sprite.CurrentAnimation = null;
		}
		if (this.IsSitting())
		{
			this.ShowSitting();
		}
	}

	public void stopJittering()
	{
		this.jitterStrength = 0f;
		this.jitter = Vector2.Zero;
	}

	public override Microsoft.Xna.Framework.Rectangle nextPosition(int direction)
	{
		Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
		switch (direction)
		{
		case 0:
			nextPosition.Y -= (int)Math.Ceiling(this.getMovementSpeed());
			break;
		case 1:
			nextPosition.X += (int)Math.Ceiling(this.getMovementSpeed());
			break;
		case 2:
			nextPosition.Y += (int)Math.Ceiling(this.getMovementSpeed());
			break;
		case 3:
			nextPosition.X -= (int)Math.Ceiling(this.getMovementSpeed());
			break;
		}
		return nextPosition;
	}

	public Microsoft.Xna.Framework.Rectangle nextPositionHalf(int direction)
	{
		Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
		switch (direction)
		{
		case 0:
			nextPosition.Y -= (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
			break;
		case 1:
			nextPosition.X += (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
			break;
		case 2:
			nextPosition.Y += (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
			break;
		case 3:
			nextPosition.X -= (int)Math.Ceiling((double)this.getMovementSpeed() / 2.0);
			break;
		}
		return nextPosition;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="skillType">e.g. farming, fishing, foraging</param>
	/// <param name="skillLevel">5 or 10</param>
	/// <returns></returns>
	public int getProfessionForSkill(int skillType, int skillLevel)
	{
		switch (skillLevel)
		{
		case 5:
			switch (skillType)
			{
			case 0:
				if (this.professions.Contains(0))
				{
					return 0;
				}
				if (this.professions.Contains(1))
				{
					return 1;
				}
				break;
			case 1:
				if (this.professions.Contains(6))
				{
					return 6;
				}
				if (this.professions.Contains(7))
				{
					return 7;
				}
				break;
			case 2:
				if (this.professions.Contains(12))
				{
					return 12;
				}
				if (this.professions.Contains(13))
				{
					return 13;
				}
				break;
			case 3:
				if (this.professions.Contains(18))
				{
					return 18;
				}
				if (this.professions.Contains(19))
				{
					return 19;
				}
				break;
			case 4:
				if (this.professions.Contains(24))
				{
					return 24;
				}
				if (this.professions.Contains(25))
				{
					return 25;
				}
				break;
			}
			break;
		case 10:
			switch (skillType)
			{
			case 0:
				if (this.professions.Contains(1))
				{
					if (this.professions.Contains(4))
					{
						return 4;
					}
					if (this.professions.Contains(5))
					{
						return 5;
					}
				}
				else
				{
					if (this.professions.Contains(2))
					{
						return 2;
					}
					if (this.professions.Contains(3))
					{
						return 3;
					}
				}
				break;
			case 1:
				if (this.professions.Contains(6))
				{
					if (this.professions.Contains(8))
					{
						return 8;
					}
					if (this.professions.Contains(9))
					{
						return 9;
					}
				}
				else
				{
					if (this.professions.Contains(10))
					{
						return 10;
					}
					if (this.professions.Contains(11))
					{
						return 11;
					}
				}
				break;
			case 2:
				if (this.professions.Contains(12))
				{
					if (this.professions.Contains(14))
					{
						return 14;
					}
					if (this.professions.Contains(15))
					{
						return 15;
					}
				}
				else
				{
					if (this.professions.Contains(16))
					{
						return 16;
					}
					if (this.professions.Contains(17))
					{
						return 17;
					}
				}
				break;
			case 3:
				if (this.professions.Contains(18))
				{
					if (this.professions.Contains(20))
					{
						return 20;
					}
					if (this.professions.Contains(21))
					{
						return 21;
					}
				}
				else
				{
					if (this.professions.Contains(23))
					{
						return 23;
					}
					if (this.professions.Contains(22))
					{
						return 22;
					}
				}
				break;
			case 4:
				if (this.professions.Contains(24))
				{
					if (this.professions.Contains(26))
					{
						return 26;
					}
					if (this.professions.Contains(27))
					{
						return 27;
					}
				}
				else
				{
					if (this.professions.Contains(28))
					{
						return 28;
					}
					if (this.professions.Contains(29))
					{
						return 29;
					}
				}
				break;
			}
			break;
		}
		return -1;
	}

	public void behaviorOnMovement(int direction)
	{
		this.hasMoved = true;
	}

	public void OnEmoteAnimationEnd(Farmer farmer)
	{
		if (farmer == this && this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
	}

	public void EndEmoteAnimation()
	{
		if (this.isEmoteAnimating)
		{
			if (this.jitterStrength > 0f)
			{
				this.stopJittering();
			}
			if (base.yJumpOffset != 0)
			{
				base.yJumpOffset = 0;
				base.yJumpVelocity = 0f;
			}
			this.FarmerSprite.PauseForSingleAnimation = false;
			this.FarmerSprite.StopAnimation();
			this.isEmoteAnimating = false;
		}
	}

	private void broadcastHaltAnimation(Farmer who)
	{
		if (this.IsLocalPlayer)
		{
			this.haltAnimationEvent.Fire();
		}
		else
		{
			Farmer.completelyStopAnimating(who);
		}
	}

	private void performHaltAnimation()
	{
		this.completelyStopAnimatingOrDoingAction();
	}

	public void performKissFarmer(long otherPlayerID)
	{
		Farmer spouse = Game1.GetPlayer(otherPlayerID);
		if (spouse != null)
		{
			bool localPlayerOnLeft = base.StandingPixel.X < spouse.StandingPixel.X;
			this.PerformKiss(localPlayerOnLeft ? 1 : 3);
			spouse.PerformKiss((!localPlayerOnLeft) ? 1 : 3);
		}
	}

	public void PerformKiss(int facingDirection)
	{
		if (!Game1.eventUp && !this.UsingTool && (!this.IsLocalPlayer || Game1.activeClickableMenu == null) && !this.isRidingHorse() && !this.IsSitting() && !base.IsEmoting && this.CanMove)
		{
			this.CanMove = false;
			this.FarmerSprite.PauseForSingleAnimation = false;
			this.faceDirection(facingDirection);
			this.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
			{
				new FarmerSprite.AnimationFrame(101, 1000, 0, secondaryArm: false, this.FacingDirection == 3),
				new FarmerSprite.AnimationFrame(6, 1, secondaryArm: false, this.FacingDirection == 3, broadcastHaltAnimation)
			});
			if (!Stats.AllowRetroactiveAchievements)
			{
				Game1.stats.checkForFullHouseAchievement(isDirectUnlock: true);
			}
		}
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (this.IsSitting())
		{
			return;
		}
		if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
		{
			if (Game1.shouldTimePass() && this.temporarilyInvincible)
			{
				if (this.temporaryInvincibilityTimer < 0)
				{
					this.currentTemporaryInvincibilityDuration = 1200;
				}
				this.temporaryInvincibilityTimer += time.ElapsedGameTime.Milliseconds;
				if (this.temporaryInvincibilityTimer > this.currentTemporaryInvincibilityDuration)
				{
					this.temporarilyInvincible = false;
					this.temporaryInvincibilityTimer = 0;
				}
			}
		}
		else if (this.temporarilyInvincible)
		{
			this.temporarilyInvincible = false;
			this.temporaryInvincibilityTimer = 0;
		}
		if (Game1.activeClickableMenu != null && (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
		{
			return;
		}
		if (this.isRafting)
		{
			this.moveRaft(currentLocation, time);
			return;
		}
		if (base.xVelocity != 0f || base.yVelocity != 0f)
		{
			if (double.IsNaN(base.xVelocity) || double.IsNaN(base.yVelocity))
			{
				base.xVelocity = 0f;
				base.yVelocity = 0f;
			}
			Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBox();
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(bounds.X + (int)Math.Floor(base.xVelocity), bounds.Y - (int)Math.Floor(base.yVelocity), bounds.Width, bounds.Height);
			Microsoft.Xna.Framework.Rectangle nextPositionCeil = new Microsoft.Xna.Framework.Rectangle(bounds.X + (int)Math.Ceiling(base.xVelocity), bounds.Y - (int)Math.Ceiling(base.yVelocity), bounds.Width, bounds.Height);
			Microsoft.Xna.Framework.Rectangle nextPosition = Microsoft.Xna.Framework.Rectangle.Union(value, nextPositionCeil);
			if (!currentLocation.isCollidingPosition(nextPosition, viewport, isFarmer: true, -1, glider: false, this))
			{
				base.position.X += base.xVelocity;
				base.position.Y -= base.yVelocity;
				base.xVelocity -= base.xVelocity / 16f;
				base.yVelocity -= base.yVelocity / 16f;
				if (Math.Abs(base.xVelocity) <= 0.05f)
				{
					base.xVelocity = 0f;
				}
				if (Math.Abs(base.yVelocity) <= 0.05f)
				{
					base.yVelocity = 0f;
				}
			}
			else
			{
				base.xVelocity -= base.xVelocity / 16f;
				base.yVelocity -= base.yVelocity / 16f;
				if (Math.Abs(base.xVelocity) <= 0.05f)
				{
					base.xVelocity = 0f;
				}
				if (Math.Abs(base.yVelocity) <= 0.05f)
				{
					base.yVelocity = 0f;
				}
			}
		}
		if (this.CanMove || Game1.eventUp || base.controller != null || this.canStrafeForToolUse())
		{
			this.temporaryPassableTiles.ClearNonIntersecting(this.GetBoundingBox());
			float movementSpeed = this.getMovementSpeed();
			this.temporarySpeedBuff = 0f;
			if ((this.movementDirections.Contains(0) && this.MovePositionImpl(0, 0f, 0f - movementSpeed, time, viewport)) || (this.movementDirections.Contains(2) && this.MovePositionImpl(2, 0f, movementSpeed, time, viewport)) || (this.movementDirections.Contains(1) && this.MovePositionImpl(1, movementSpeed, 0f, time, viewport)) || (this.movementDirections.Contains(3) && this.MovePositionImpl(3, 0f - movementSpeed, 0f, time, viewport)))
			{
				return;
			}
		}
		if (this.movementDirections.Count > 0 && !this.UsingTool)
		{
			this.FarmerSprite.intervalModifier = 1f - (this.running ? 0.0255f : 0.025f) * (Math.Max(1f, ((float)base.speed + (Game1.eventUp ? 0f : ((float)(int)this.addedSpeed + (this.isRidingHorse() ? 4.6f : 0f)))) * this.movementMultiplier * (float)Game1.currentGameTime.ElapsedGameTime.Milliseconds) * 1.25f);
		}
		else
		{
			this.FarmerSprite.intervalModifier = 1f;
		}
		if (currentLocation != null && currentLocation.isFarmerCollidingWithAnyCharacter())
		{
			this.temporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64));
		}
	}

	public bool canStrafeForToolUse()
	{
		if (this.toolHold.Value != 0 && this.canReleaseTool)
		{
			if (this.toolPower.Value < 1)
			{
				return this.toolHoldStartTime.Value - this.toolHold.Value > 150;
			}
			return true;
		}
		return false;
	}

	/// <summary>Handle a player's movement in a specific direction, after the game has already checked whether movement is allowed.</summary>
	/// <param name="direction">The direction the player is moving in, matching a constant like <see cref="F:StardewValley.Game1.up" />.</param>
	/// <param name="movementSpeedX">The player's movement speed along the X axis for this direction.</param>
	/// <param name="movementSpeedY">The player's movement speed along the Y axis for this direction.</param>
	/// <param name="time">The elapsed game time.</param>
	/// <param name="viewport">The pixel area being viewed relative to the top-left corner of the map.</param>
	/// <returns>Returns whether the movement was fully handled (e.g. a warp was activated), so no further movement logic should be applied.</returns>
	protected virtual bool MovePositionImpl(int direction, float movementSpeedX, float movementSpeedY, GameTime time, xTile.Dimensions.Rectangle viewport)
	{
		Microsoft.Xna.Framework.Rectangle targetPos = this.nextPosition(direction);
		Warp warp = Game1.currentLocation.isCollidingWithWarp(targetPos, this);
		if (warp != null && this.IsLocalPlayer)
		{
			if (Game1.eventUp && !((!(Game1.CurrentEvent?.isFestival)) ?? true))
			{
				Game1.CurrentEvent.TryStartEndFestivalDialogue(this);
			}
			else
			{
				this.warpFarmer(warp, direction);
			}
			return true;
		}
		bool isCutscene = Game1.eventUp && !(Game1.CurrentEvent?.isFestival ?? true) && ((!(Game1.CurrentEvent?.playerControlSequence)) ?? false);
		if (!base.currentLocation.isCollidingPosition(targetPos, viewport, isFarmer: true, 0, glider: false, this) || this.ignoreCollisions || isCutscene)
		{
			base.position.X += movementSpeedX;
			base.position.Y += movementSpeedY;
			this.behaviorOnMovement(direction);
			return false;
		}
		if (!base.currentLocation.isCollidingPosition(this.nextPositionHalf(direction), viewport, isFarmer: true, 0, glider: false, this))
		{
			base.position.X += movementSpeedX / 2f;
			base.position.Y += movementSpeedY / 2f;
			this.behaviorOnMovement(direction);
			return false;
		}
		if (this.movementDirections.Count == 1)
		{
			Microsoft.Xna.Framework.Rectangle tmp = targetPos;
			if (direction == 0 || direction == 2)
			{
				tmp.Width /= 4;
				bool leftCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				tmp.X += tmp.Width * 3;
				bool rightCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				if (leftCorner && !rightCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: true, 0, glider: false, this))
				{
					base.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (rightCorner && !leftCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: true, 0, glider: false, this))
				{
					base.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
			}
			else
			{
				tmp.Height /= 4;
				bool topCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				tmp.Y += tmp.Height * 3;
				bool bottomCorner = base.currentLocation.isCollidingPosition(tmp, viewport, isFarmer: true, 0, glider: false, this);
				if (topCorner && !bottomCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: true, 0, glider: false, this))
				{
					base.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (bottomCorner && !topCorner && !base.currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: true, 0, glider: false, this))
				{
					base.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
			}
		}
		return false;
	}

	public void updateMovementAnimation(GameTime time)
	{
		if (this._emoteGracePeriod > 0)
		{
			this._emoteGracePeriod -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.isEmoteAnimating && (((this.IsLocalPlayer ? (this.movementDirections.Count > 0) : base.IsRemoteMoving()) && this._emoteGracePeriod <= 0) || !this.FarmerSprite.PauseForSingleAnimation))
		{
			this.EndEmoteAnimation();
		}
		bool carrying = this.IsCarrying();
		if (!this.isRidingHorse())
		{
			this.xOffset = 0f;
		}
		if (this.CurrentTool is FishingRod rod && (rod.isTimingCast || rod.isCasting))
		{
			rod.setTimingCastAnimation(this);
			return;
		}
		if (this.FarmerSprite.PauseForSingleAnimation || this.UsingTool)
		{
			if (this.UsingTool && this.canStrafeForToolUse() && (this.movementDirections.Count > 0 || (!this.IsLocalPlayer && base.IsRemoteMoving())) && base.yJumpOffset == 0)
			{
				this.jumpWithoutSound(2.5f);
			}
			return;
		}
		if (this.IsSitting())
		{
			this.ShowSitting();
			return;
		}
		if (this.IsLocalPlayer && !this.CanMove && !Game1.eventUp)
		{
			if (this.isRidingHorse() && this.mount != null && !this.isAnimatingMount)
			{
				this.showRiding();
			}
			else if (carrying)
			{
				this.showCarrying();
			}
			return;
		}
		if (this.IsLocalPlayer || this.isFakeEventActor)
		{
			base.moveUp = this.movementDirections.Contains(0);
			base.moveRight = this.movementDirections.Contains(1);
			base.moveDown = this.movementDirections.Contains(2);
			base.moveLeft = this.movementDirections.Contains(3);
			if (base.moveLeft)
			{
				this.FacingDirection = 3;
			}
			else if (base.moveRight)
			{
				this.FacingDirection = 1;
			}
			else if (base.moveUp)
			{
				this.FacingDirection = 0;
			}
			else if (base.moveDown)
			{
				this.FacingDirection = 2;
			}
			if (this.isRidingHorse() && !this.mount.dismounting.Value)
			{
				base.speed = 2;
			}
		}
		else
		{
			base.moveLeft = base.IsRemoteMoving() && this.FacingDirection == 3;
			base.moveRight = base.IsRemoteMoving() && this.FacingDirection == 1;
			base.moveUp = base.IsRemoteMoving() && this.FacingDirection == 0;
			base.moveDown = base.IsRemoteMoving() && this.FacingDirection == 2;
			bool num = base.moveUp || base.moveRight || base.moveDown || base.moveLeft;
			float speed = base.position.CurrentInterpolationSpeed() / ((float)Game1.currentGameTime.ElapsedGameTime.Milliseconds * 0.066f);
			this.running = Math.Abs(speed - 5f) < Math.Abs(speed - 2f) && !this.bathingClothes.Value && !this.onBridge.Value;
			if (!num)
			{
				this.FarmerSprite.StopAnimation();
			}
		}
		if (this.hasBuff("19"))
		{
			this.running = false;
			base.moveUp = false;
			base.moveDown = false;
			base.moveLeft = false;
			base.moveRight = false;
		}
		if (!this.FarmerSprite.PauseForSingleAnimation && !this.UsingTool)
		{
			if (this.isRidingHorse() && !this.mount.dismounting.Value)
			{
				this.showRiding();
			}
			else if (base.moveLeft && this.running && !carrying)
			{
				this.FarmerSprite.animate(56, time);
			}
			else if (base.moveRight && this.running && !carrying)
			{
				this.FarmerSprite.animate(40, time);
			}
			else if (base.moveUp && this.running && !carrying)
			{
				this.FarmerSprite.animate(48, time);
			}
			else if (base.moveDown && this.running && !carrying)
			{
				this.FarmerSprite.animate(32, time);
			}
			else if (base.moveLeft && this.running)
			{
				this.FarmerSprite.animate(152, time);
			}
			else if (base.moveRight && this.running)
			{
				this.FarmerSprite.animate(136, time);
			}
			else if (base.moveUp && this.running)
			{
				this.FarmerSprite.animate(144, time);
			}
			else if (base.moveDown && this.running)
			{
				this.FarmerSprite.animate(128, time);
			}
			else if (base.moveLeft && !carrying)
			{
				this.FarmerSprite.animate(24, time);
			}
			else if (base.moveRight && !carrying)
			{
				this.FarmerSprite.animate(8, time);
			}
			else if (base.moveUp && !carrying)
			{
				this.FarmerSprite.animate(16, time);
			}
			else if (base.moveDown && !carrying)
			{
				this.FarmerSprite.animate(0, time);
			}
			else if (base.moveLeft)
			{
				this.FarmerSprite.animate(120, time);
			}
			else if (base.moveRight)
			{
				this.FarmerSprite.animate(104, time);
			}
			else if (base.moveUp)
			{
				this.FarmerSprite.animate(112, time);
			}
			else if (base.moveDown)
			{
				this.FarmerSprite.animate(96, time);
			}
			else if (carrying)
			{
				this.showCarrying();
			}
			else
			{
				this.showNotCarrying();
			}
		}
	}

	public bool IsCarrying()
	{
		if (this.mount != null || this.isAnimatingMount)
		{
			return false;
		}
		if (this.IsSitting())
		{
			return false;
		}
		if (this.onBridge.Value)
		{
			return false;
		}
		if (this.ActiveObject == null || Game1.eventUp || Game1.killScreen)
		{
			return false;
		}
		if (!this.ActiveObject.IsHeldOverHead())
		{
			return false;
		}
		return true;
	}

	public void doneEating()
	{
		this.isEating = false;
		this.tempFoodItemTextureName.Value = null;
		this.completelyStopAnimatingOrDoingAction();
		this.forceCanMove();
		if (this.mostRecentlyGrabbedItem == null || !this.IsLocalPlayer)
		{
			return;
		}
		Object consumed = this.itemToEat as Object;
		if (consumed.QualifiedItemId == "(O)434")
		{
			Game1.stats.checkForStardropAchievement(isDirectUnlock: true);
			this.yOffset = 0f;
			base.yJumpOffset = 0;
			Game1.changeMusicTrack("none");
			Game1.playSound("stardrop");
			string mid = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs." + Game1.random.Choose("3094", "3095"));
			if (this.favoriteThing.Value != null)
			{
				mid = (this.favoriteThing.Value.Contains("Stardew") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3097") : ((!this.favoriteThing.Equals("ConcernedApe")) ? (mid + this.favoriteThing.Value) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3099")));
			}
			DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3100") + mid + Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3101"), 6000);
			this.maxStamina.Value += 34;
			this.stamina = this.MaxStamina;
			this.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(57, 6000)
			});
			base.startGlowing(new Color(200, 0, 255), border: false, 0.1f);
			this.jitterStrength = 1f;
			Game1.staminaShakeTimer = 12000;
			Game1.screenGlowOnce(new Color(200, 0, 255), hold: true);
			this.CanMove = false;
			this.freezePause = 8000;
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 16, 16, 16), 60f, 8, 40, base.Position + new Vector2(-8f, -128f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0.0075f, 0f, 0f)
			{
				alpha = 0.75f,
				alphaFade = 0.0025f,
				motion = new Vector2(0f, -0.25f)
			});
			if (Game1.displayHUD && !Game1.eventUp)
			{
				for (int i = 0; i < 40; i++)
				{
					Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite(Game1.random.Next(10, 12), new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right / Game1.options.uiScale - 48f - 8f - (float)Game1.random.Next(64), (float)Game1.random.Next(-64, 64) + (float)Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom / Game1.options.uiScale - 224f - 16f - (float)(int)((double)(this.MaxStamina - 270) * 0.715)), Game1.random.Choose(Color.White, Color.Lime), 8, flipped: false, 50f)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = 200 * i,
						interval = 100f,
						local = true
					});
				}
			}
			Point tile = base.TilePoint;
			Utility.addSprinklesToLocation(base.currentLocation, tile.X, tile.Y, 9, 9, 6000, 100, new Color(200, 0, 255), null, motionTowardCenter: true);
			DelayedAction.stopFarmerGlowing(6000);
			Utility.addSprinklesToLocation(base.currentLocation, tile.X, tile.Y, 9, 9, 6000, 300, Color.Cyan, null, motionTowardCenter: true);
			this.mostRecentlyGrabbedItem = null;
		}
		else
		{
			if (consumed.HasContextTag("ginger_item"))
			{
				this.buffs.Remove("25");
			}
			foreach (Buff buff in consumed.GetFoodOrDrinkBuffs())
			{
				this.applyBuff(buff);
			}
			switch (consumed.QualifiedItemId)
			{
			case "(O)773":
				this.health = this.maxHealth;
				break;
			case "(O)351":
				this.exhausted.Value = false;
				break;
			case "(O)349":
				this.Stamina = this.MaxStamina;
				break;
			}
			float oldStam = this.Stamina;
			int oldHealth = this.health;
			int staminaToHeal = consumed.staminaRecoveredOnConsumption();
			int healthToHeal = consumed.healthRecoveredOnConsumption();
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is MineShaft && Game1.mine.getMineArea() == 121 && this.team.calicoStatueEffects.ContainsKey(6))
			{
				staminaToHeal = Math.Max(1, staminaToHeal / 2);
				healthToHeal = Math.Max(1, healthToHeal / 2);
			}
			this.Stamina = Math.Min(this.MaxStamina, this.Stamina + (float)staminaToHeal);
			this.health = Math.Min(this.maxHealth, this.health + healthToHeal);
			if (oldStam < this.Stamina)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(this.Stamina - oldStam)), 4));
			}
			if (oldHealth < this.health)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", this.health - oldHealth), 5));
			}
		}
		if (consumed != null && consumed.Edibility < 0)
		{
			this.CanMove = false;
			this.sickAnimationEvent.Fire();
		}
	}

	/// <summary>Perform an action for each active non-completed quest in the local player's log.</summary>
	/// <param name="check">The action to perform on the quest. This should return whether the quest state changed.</param>
	/// <param name="onlyOneQuest">Whether to stop after any quest is updated.</param>
	/// <returns>Returns whether a quest was updated (e.g. closer to completion).</returns>
	public virtual bool NotifyQuests(Func<Quest, bool> check, bool onlyOneQuest = false)
	{
		bool changed = false;
		for (int i = this.questLog.Count - 1; i >= 0; i--)
		{
			Quest quest = this.questLog[i];
			if (!quest.completed.Value)
			{
				if (quest == null)
				{
					this.questLog.RemoveAt(i);
				}
				else if (check(quest))
				{
					changed = true;
					if (onlyOneQuest)
					{
						break;
					}
				}
			}
		}
		return changed;
	}

	public virtual void AddCompanion(Companion companion)
	{
		if (!this.companions.Contains(companion))
		{
			companion.InitializeCompanion(this);
			this.companions.Add(companion);
		}
	}

	public virtual void RemoveCompanion(Companion companion)
	{
		if (this.companions.Contains(companion))
		{
			this.companions.Remove(companion);
			companion.CleanupCompanion();
		}
	}

	public static void completelyStopAnimating(Farmer who)
	{
		who.completelyStopAnimatingOrDoingAction();
	}

	public void completelyStopAnimatingOrDoingAction()
	{
		this.CanMove = !Game1.eventUp;
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		if (this.UsingTool)
		{
			this.EndUsingTool();
			if (this.CurrentTool is FishingRod rod)
			{
				rod.resetState();
			}
		}
		if (this.usingSlingshot && this.CurrentTool is Slingshot slingshot)
		{
			slingshot.finish();
		}
		this.UsingTool = false;
		this.isEating = false;
		this.FarmerSprite.PauseForSingleAnimation = false;
		this.usingSlingshot = false;
		this.canReleaseTool = false;
		this.Halt();
		this.Sprite.StopAnimation();
		if (this.CurrentTool is MeleeWeapon weapon)
		{
			weapon.isOnSpecial = false;
		}
		this.stopJittering();
	}

	public void doEmote(int whichEmote)
	{
		if (!Game1.eventUp && !base.isEmoting)
		{
			base.isEmoting = true;
			base.currentEmote = whichEmote;
			base.currentEmoteFrame = 0;
			base.emoteInterval = 0f;
		}
	}

	public void performTenMinuteUpdate()
	{
	}

	public void setRunning(bool isRunning, bool force = false)
	{
		if (this.canOnlyWalk || (this.bathingClothes.Value && !this.running) || (Game1.CurrentEvent != null && isRunning && !Game1.CurrentEvent.isFestival && !Game1.CurrentEvent.playerControlSequence && (base.controller == null || !base.controller.allowPlayerPathingInEvent)))
		{
			return;
		}
		if (this.isRidingHorse())
		{
			this.running = true;
		}
		else if (this.stamina <= 0f)
		{
			base.speed = 2;
			if (this.running)
			{
				this.Halt();
			}
			this.running = false;
		}
		else if (force || (this.CanMove && !this.isEating && Game1.currentLocation != null && (Game1.currentLocation.currentEvent == null || Game1.currentLocation.currentEvent.playerControlSequence) && (isRunning || !this.UsingTool) && (this.Sprite == null || !((FarmerSprite)this.Sprite).PauseForSingleAnimation)))
		{
			this.running = isRunning;
			if (this.running)
			{
				base.speed = 5;
			}
			else
			{
				base.speed = 2;
			}
		}
		else if (this.UsingTool)
		{
			this.running = isRunning;
			if (this.running)
			{
				base.speed = 5;
			}
			else
			{
				base.speed = 2;
			}
		}
	}

	public void addSeenResponse(string id)
	{
		this.dialogueQuestionsAnswered.Add(id);
	}

	public void eatObject(Object o, bool overrideFullness = false)
	{
		if (o?.QualifiedItemId == "(O)434")
		{
			Game1.MusicDuckTimer = 10000f;
			Game1.changeMusicTrack("none");
			Game1.multiplayer.globalChatInfoMessage("Stardrop", base.Name);
		}
		if (base.getFacingDirection() != 2)
		{
			this.faceDirection(2);
		}
		this.itemToEat = o;
		this.mostRecentlyGrabbedItem = o;
		this.forceCanMove();
		this.completelyStopAnimatingOrDoingAction();
		if (Game1.objectData.TryGetValue(o.ItemId, out var data) && data.IsDrink)
		{
			if (this.IsLocalPlayer && this.hasBuff("7") && !overrideFullness)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2898")));
				return;
			}
			this.drinkAnimationEvent.Fire(o.getOne() as Object);
		}
		else if (o.Edibility != -300)
		{
			if (this.hasBuff("6") && !overrideFullness)
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2899")));
				return;
			}
			this.eatAnimationEvent.Fire(o.getOne() as Object);
		}
		this.freezePause = 20000;
		this.CanMove = false;
		this.isEating = true;
	}

	/// <inheritdoc />
	public override void DrawShadow(SpriteBatch b)
	{
		float drawLayer = this.getDrawLayer() - 1E-06f;
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(this.GetShadowOffset() + base.Position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - (((this.running || this.UsingTool) && this.FarmerSprite.currentAnimationIndex > 1) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[this.FarmerSprite.CurrentFrame]) * 0.5f) : 0f), SpriteEffects.None, drawLayer);
	}

	private void performDrinkAnimation(Object item)
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		if (!this.IsLocalPlayer)
		{
			this.itemToEat = item;
		}
		this.FarmerSprite.animateOnce(294, 80f, 8);
		this.isEating = true;
		if (item != null && item.HasContextTag("mayo_item") && Utility.isThereAFarmerOrCharacterWithinDistance(base.Tile, 7, base.currentLocation) is NPC { Age: not 2 } npc)
		{
			int whichMessage = Game1.random.Next(3);
			if (npc.Manners == 2 || npc.SocialAnxiety == 1)
			{
				whichMessage = 3;
			}
			if (npc.Name == "Emily" || npc.Name == "Sandy" || npc.Name == "Linus" || (npc.Name == "Krobus" && item.QualifiedItemId == "(O)308"))
			{
				whichMessage = 4;
			}
			else if (npc.Name == "Krobus" || npc.Name == "Dwarf" || npc is Monster || npc is Horse || npc is Pet || npc is Child)
			{
				return;
			}
			npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Mayo_reaction" + whichMessage), null, 2, 3000, 500);
			npc.faceTowardFarmerForPeriod(1500, 7, faceAway: false, this);
		}
	}

	public Farmer CreateFakeEventFarmer()
	{
		Farmer fake_farmer = new Farmer(new FarmerSprite(this.FarmerSprite.textureName.Value), new Vector2(192f, 192f), 1, "", new List<Item>(), this.IsMale);
		fake_farmer.Name = base.Name;
		fake_farmer.displayName = this.displayName;
		fake_farmer.isFakeEventActor = true;
		fake_farmer.changeGender(this.IsMale);
		fake_farmer.changeHairStyle(this.hair.Value);
		fake_farmer.UniqueMultiplayerID = this.UniqueMultiplayerID;
		fake_farmer.shirtItem.Set(this.shirtItem.Value);
		fake_farmer.pantsItem.Set(this.pantsItem.Value);
		fake_farmer.shirt.Set(this.shirt.Value);
		fake_farmer.pants.Set(this.pants.Value);
		foreach (Trinket t in this.trinketItems)
		{
			fake_farmer.trinketItems.Add((Trinket)(t?.getOne()));
		}
		fake_farmer.changeShoeColor(this.shoes.Value);
		fake_farmer.boots.Set(this.boots.Value);
		fake_farmer.leftRing.Set(this.leftRing.Value);
		fake_farmer.rightRing.Set(this.rightRing.Value);
		fake_farmer.hat.Set(this.hat.Value);
		fake_farmer.pantsColor.Set(this.pantsColor.Value);
		fake_farmer.changeHairColor(this.hairstyleColor.Value);
		fake_farmer.changeSkinColor(this.skin.Value);
		fake_farmer.accessory.Set(this.accessory.Value);
		fake_farmer.changeEyeColor(this.newEyeColor.Value);
		fake_farmer.UpdateClothing();
		return fake_farmer;
	}

	private void performEatAnimation(Object item)
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		if (!this.IsLocalPlayer)
		{
			this.itemToEat = item;
		}
		this.FarmerSprite.animateOnce(216, 80f, 8);
		this.isEating = true;
	}

	public void netDoEmote(string emote_type)
	{
		this.doEmoteEvent.Fire(emote_type);
	}

	private void performSickAnimation()
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		this.isEating = false;
		this.FarmerSprite.animateOnce(224, 350f, 4);
		this.doEmote(12);
	}

	public void eatHeldObject()
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		if (Game1.fadeToBlack)
		{
			return;
		}
		Item oldActiveItem = null;
		int oldToolIndex = 0;
		bool swappedActiveObject = false;
		bool wasStowed = false;
		if (this.ActiveItem == null || this.ActiveItem != this.mostRecentlyGrabbedItem)
		{
			if (this.netItemStowed.Value)
			{
				wasStowed = true;
				this.netItemStowed.Value = false;
				this.UpdateItemStow();
			}
			if (this.ActiveItem == null)
			{
				this.ActiveItem = this.mostRecentlyGrabbedItem;
			}
			else if (this.ActiveItem != this.mostRecentlyGrabbedItem)
			{
				oldToolIndex = this.currentToolIndex.Value;
				if (this.currentToolIndex.Value < 0 || this.currentToolIndex.Value >= this.Items.Count)
				{
					this.currentToolIndex.Value = 0;
				}
				oldActiveItem = this.Items[this.currentToolIndex.Value];
				this.Items[this.currentToolIndex.Value] = this.mostRecentlyGrabbedItem;
				this.OnItemReceived(this.mostRecentlyGrabbedItem, this.mostRecentlyGrabbedItem.Stack, null);
				swappedActiveObject = true;
			}
		}
		this.eatObject(this.ActiveObject);
		if (this.isEating)
		{
			this.reduceActiveItemByOne();
			this.CanMove = false;
		}
		if (swappedActiveObject)
		{
			this.Items[this.currentToolIndex.Value] = oldActiveItem;
			this.currentToolIndex.Value = oldToolIndex;
		}
		if (wasStowed)
		{
			this.netItemStowed.Value = true;
			this.UpdateItemStow();
		}
	}

	public void grabObject(Object obj)
	{
		if (this.isEmoteAnimating)
		{
			this.EndEmoteAnimation();
		}
		if (obj != null)
		{
			this.CanMove = false;
			switch (this.FacingDirection)
			{
			case 2:
				((FarmerSprite)this.Sprite).animateOnce(64, 50f, 8);
				break;
			case 1:
				((FarmerSprite)this.Sprite).animateOnce(72, 50f, 8);
				break;
			case 0:
				((FarmerSprite)this.Sprite).animateOnce(80, 50f, 8);
				break;
			case 3:
				((FarmerSprite)this.Sprite).animateOnce(88, 50f, 8);
				break;
			}
			Game1.playSound("pickUpItem");
		}
	}

	public virtual void PlayFishBiteChime()
	{
		int bite_chime = this.biteChime.Value;
		if (bite_chime < 0)
		{
			bite_chime = Game1.game1.instanceIndex;
		}
		if (bite_chime > 3)
		{
			bite_chime = 3;
		}
		if (bite_chime == 0)
		{
			base.playNearbySoundLocal("fishBite");
		}
		else
		{
			base.playNearbySoundLocal("fishBite_alternate_" + (bite_chime - 1));
		}
	}

	public string getTitle()
	{
		int level = this.Level;
		if (level >= 30)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2016");
		}
		switch (level)
		{
		case 29:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2017");
		case 27:
		case 28:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2018");
		case 25:
		case 26:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2019");
		case 23:
		case 24:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2020");
		case 21:
		case 22:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2021");
		case 19:
		case 20:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2022");
		case 17:
		case 18:
			if (!this.IsMale)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2024");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2023");
		case 15:
		case 16:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2025");
		case 13:
		case 14:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2026");
		case 11:
		case 12:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2027");
		case 9:
		case 10:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2028");
		case 7:
		case 8:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2029");
		case 5:
		case 6:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2030");
		case 3:
		case 4:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2031");
		default:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.2032");
		}
	}

	public void queueMessage(byte messageType, Farmer sourceFarmer, params object[] data)
	{
		this.queueMessage(new OutgoingMessage(messageType, sourceFarmer, data));
	}

	public void queueMessage(OutgoingMessage message)
	{
		this.messageQueue.Add(message);
	}
}
