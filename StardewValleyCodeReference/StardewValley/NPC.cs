using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley;

[XmlInclude(typeof(Cat))]
[XmlInclude(typeof(Child))]
[XmlInclude(typeof(Dog))]
[XmlInclude(typeof(Horse))]
[XmlInclude(typeof(Junimo))]
[XmlInclude(typeof(JunimoHarvester))]
[XmlInclude(typeof(Pet))]
[XmlInclude(typeof(TrashBear))]
[XmlInclude(typeof(Raccoon))]
[XmlInclude(typeof(Monster))]
public class NPC : Character, IComparable
{
	public const int minimum_square_pause = 6000;

	public const int maximum_square_pause = 12000;

	public const int portrait_width = 64;

	public const int portrait_height = 64;

	public const int portrait_neutral_index = 0;

	public const int portrait_happy_index = 1;

	public const int portrait_sad_index = 2;

	public const int portrait_custom_index = 3;

	public const int portrait_blush_index = 4;

	public const int portrait_angry_index = 5;

	public const int startingFriendship = 0;

	public const int defaultSpeed = 2;

	public const int maxGiftsPerWeek = 2;

	public const int friendshipPointsPerHeartLevel = 250;

	public const int maxFriendshipPoints = 2500;

	public const int gift_taste_love = 0;

	public const int gift_taste_like = 2;

	public const int gift_taste_neutral = 8;

	public const int gift_taste_dislike = 4;

	public const int gift_taste_hate = 6;

	public const int gift_taste_stardroptea = 7;

	public const int textStyle_shake = 0;

	public const int textStyle_none = 2;

	public const int adult = 0;

	public const int teen = 1;

	public const int child = 2;

	public const int neutral = 0;

	public const int polite = 1;

	public const int rude = 2;

	public const int outgoing = 0;

	public const int shy = 1;

	public const int positive = 0;

	public const int negative = 1;

	public const string region_desert = "Desert";

	public const string region_town = "Town";

	public const string region_other = "Other";

	/// <summary>The default pixel width for the <see cref="P:StardewValley.Character.Sprite" />.</summary>
	public const int defaultSpriteWidth = 16;

	/// <summary>The default pixel height for the <see cref="P:StardewValley.Character.Sprite" />.</summary>
	public const int defaultSpriteHeight = 32;

	private Dictionary<string, string> dialogue;

	private SchedulePathDescription directionsToNewLocation;

	private int lengthOfWalkingSquareX;

	private int lengthOfWalkingSquareY;

	private int squarePauseAccumulation;

	private int squarePauseTotal;

	private int squarePauseOffset;

	public Microsoft.Xna.Framework.Rectangle lastCrossroad;

	/// <summary>The loaded portrait asset.</summary>
	/// <remarks>This is normally set via <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" />.</remarks>
	private Texture2D portrait;

	/// <summary>The last location for which <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" /> was applied.</summary>
	private string LastLocationNameForAppearance;

	/// <summary>The appearance ID from <c>Data/Characters</c> chosen by the last <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" /> call, or <c>null</c> if the last call didn't apply an appearance entry. This may not match their current textures if they were manually overridden after calling <see cref="M:StardewValley.NPC.ChooseAppearance(StardewValley.LocalizedContentManager)" />.</summary>
	[XmlIgnore]
	public string LastAppearanceId;

	private Vector2 nextSquarePosition;

	[XmlIgnore]
	public int shakeTimer;

	private bool isWalkingInSquare;

	private readonly NetBool isWalkingTowardPlayer = new NetBool();

	protected string textAboveHead;

	protected int textAboveHeadPreTimer;

	protected int textAboveHeadTimer;

	protected int textAboveHeadStyle;

	protected Color? textAboveHeadColor;

	protected float textAboveHeadAlpha;

	public int daysAfterLastBirth = -1;

	protected Dialogue extraDialogueMessageToAddThisMorning;

	[XmlElement("birthday_Season")]
	public readonly NetString birthday_Season = new NetString();

	[XmlElement("birthday_Day")]
	public readonly NetInt birthday_Day = new NetInt();

	[XmlElement("age")]
	public readonly NetInt age = new NetInt();

	[XmlElement("manners")]
	public readonly NetInt manners = new NetInt();

	[XmlElement("socialAnxiety")]
	public readonly NetInt socialAnxiety = new NetInt();

	[XmlElement("optimism")]
	public readonly NetInt optimism = new NetInt();

	/// <summary>The net-synchronized backing field for <see cref="P:StardewValley.NPC.Gender" />.</summary>
	[XmlElement("gender")]
	public readonly NetEnum<Gender> gender = new NetEnum<Gender>();

	[XmlIgnore]
	public readonly NetBool breather = new NetBool(value: true);

	[XmlIgnore]
	public readonly NetBool isSleeping = new NetBool(value: false);

	[XmlElement("sleptInBed")]
	public readonly NetBool sleptInBed = new NetBool(value: true);

	[XmlIgnore]
	public readonly NetBool hideShadow = new NetBool();

	[XmlElement("isInvisible")]
	public readonly NetBool isInvisible = new NetBool(value: false);

	[XmlElement("lastSeenMovieWeek")]
	public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.friendshipData" /> instead.</summary>
	public bool? datingFarmer;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Farmer.friendshipData" /> instead.</summary>
	public bool? divorcedFromFarmer;

	[XmlElement("datable")]
	public readonly NetBool datable = new NetBool();

	[XmlIgnore]
	public bool updatedDialogueYet;

	[XmlIgnore]
	public bool immediateSpeak;

	[XmlIgnore]
	public bool ignoreScheduleToday;

	protected int defaultFacingDirection;

	private readonly NetVector2 defaultPosition = new NetVector2();

	[XmlElement("defaultMap")]
	public readonly NetString defaultMap = new NetString();

	public string loveInterest;

	public int id = -1;

	public int daysUntilNotInvisible;

	public bool followSchedule = true;

	[XmlIgnore]
	public PathFindController temporaryController;

	[XmlElement("moveTowardPlayerThreshold")]
	public readonly NetInt moveTowardPlayerThreshold = new NetInt();

	[XmlIgnore]
	public float rotation;

	[XmlIgnore]
	public float yOffset;

	[XmlIgnore]
	public float swimTimer;

	[XmlIgnore]
	public float timerSinceLastMovement;

	[XmlIgnore]
	public string mapBeforeEvent;

	[XmlIgnore]
	public Vector2 positionBeforeEvent;

	[XmlIgnore]
	public Vector2 lastPosition;

	[XmlIgnore]
	public float currentScheduleDelay;

	[XmlIgnore]
	public float scheduleDelaySeconds;

	[XmlIgnore]
	public bool layingDown;

	[XmlIgnore]
	public Vector2 appliedRouteAnimationOffset = Vector2.Zero;

	[XmlIgnore]
	public string[] routeAnimationMetadata;

	[XmlElement("hasSaidAfternoonDialogue")]
	private NetBool hasSaidAfternoonDialogue = new NetBool(value: false);

	[XmlIgnore]
	public static bool hasSomeoneWateredCrops;

	[XmlIgnore]
	public static bool hasSomeoneFedThePet;

	[XmlIgnore]
	public static bool hasSomeoneFedTheAnimals;

	[XmlIgnore]
	public static bool hasSomeoneRepairedTheFences = false;

	[XmlIgnore]
	protected bool _skipRouteEndIntro;

	[NonInstancedStatic]
	public static HashSet<string> invalidDialogueFiles = new HashSet<string>();

	[XmlIgnore]
	protected bool _hasLoadedMasterScheduleData;

	[XmlIgnore]
	protected Dictionary<string, string> _masterScheduleData;

	protected static Stack<Dialogue> _EmptyDialogue = new Stack<Dialogue>();

	/// <summary>If set to a non-null value, the dialogue to return for <see cref="P:StardewValley.NPC.CurrentDialogue" /> instead of reading <see cref="F:StardewValley.Game1.npcDialogues" />.</summary>
	[XmlIgnore]
	public Stack<Dialogue> TemporaryDialogue;

	[XmlIgnore]
	public readonly NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>> currentMarriageDialogue = new NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>>();

	public readonly NetBool hasBeenKissedToday = new NetBool(value: false);

	[XmlIgnore]
	public readonly NetRef<MarriageDialogueReference> marriageDefaultDialogue = new NetRef<MarriageDialogueReference>(null);

	[XmlIgnore]
	public readonly NetBool shouldSayMarriageDialogue = new NetBool(value: false);

	public readonly NetEvent0 removeHenchmanEvent = new NetEvent0();

	private bool isPlayingSleepingAnimation;

	public readonly NetBool shouldPlayRobinHammerAnimation = new NetBool();

	private bool isPlayingRobinHammerAnimation;

	public readonly NetBool shouldPlaySpousePatioAnimation = new NetBool();

	private bool isPlayingSpousePatioAnimation;

	public readonly NetBool shouldWearIslandAttire = new NetBool();

	private bool isWearingIslandAttire;

	public readonly NetBool isMovingOnPathFindPath = new NetBool();

	/// <summary>Whether the NPC's portrait has been explicitly overridden (e.g. using the <c>changePortrait</c> event command) and shouldn't be changed automatically.</summary>
	[XmlIgnore]
	public bool portraitOverridden;

	/// <summary>Whether the NPC's sprite has been explicitly overridden (e.g. using the <c>changeSprite</c> event command) and shouldn't be changed automatically.</summary>
	[XmlIgnore]
	public bool spriteOverridden;

	[XmlIgnore]
	public List<SchedulePathDescription> queuedSchedulePaths = new List<SchedulePathDescription>();

	[XmlIgnore]
	public int lastAttemptedSchedule = -1;

	[XmlIgnore]
	public readonly NetBool doingEndOfRouteAnimation = new NetBool();

	private bool currentlyDoingEndOfRouteAnimation;

	[XmlIgnore]
	public readonly NetBool goingToDoEndOfRouteAnimation = new NetBool();

	[XmlIgnore]
	public readonly NetString endOfRouteMessage = new NetString();

	/// <summary>The backing field for <see cref="P:StardewValley.NPC.ScheduleKey" />. Most code should use that property instead.</summary>
	[XmlElement("dayScheduleName")]
	public readonly NetString dayScheduleName = new NetString();

	[XmlElement("islandScheduleName")]
	public readonly NetString islandScheduleName = new NetString();

	private int[] routeEndIntro;

	private int[] routeEndAnimation;

	private int[] routeEndOutro;

	[XmlIgnore]
	public string nextEndOfRouteMessage;

	private string loadedEndOfRouteBehavior;

	[XmlIgnore]
	protected string _startedEndOfRouteBehavior;

	[XmlIgnore]
	protected string _finishingEndOfRouteBehavior;

	[XmlIgnore]
	protected int _beforeEndOfRouteAnimationFrame;

	public readonly NetString endOfRouteBehaviorName = new NetString();

	public Point previousEndPoint;

	public int squareMovementFacingPreference;

	protected bool returningToEndPoint;

	private bool wasKissedYesterday;

	[XmlIgnore]
	public SchedulePathDescription DirectionsToNewLocation
	{
		get
		{
			return this.directionsToNewLocation;
		}
		set
		{
			this.directionsToNewLocation = value;
		}
	}

	public int DefaultFacingDirection
	{
		get
		{
			return this.defaultFacingDirection;
		}
		set
		{
			this.defaultFacingDirection = value;
		}
	}

	/// <summary>The main dialogue data for this NPC, if available.</summary>
	[XmlIgnore]
	public Dictionary<string, string> Dialogue
	{
		get
		{
			if (this is Monster || this is Pet || this is Horse || this is Child)
			{
				this.LoadedDialogueKey = null;
				return null;
			}
			if (this.dialogue == null)
			{
				string dialogue_file = "Characters\\Dialogue\\" + this.GetDialogueSheetName();
				if (NPC.invalidDialogueFiles.Contains(dialogue_file))
				{
					this.LoadedDialogueKey = null;
					this.dialogue = new Dictionary<string, string>();
				}
				try
				{
					this.dialogue = Game1.content.Load<Dictionary<string, string>>(dialogue_file).Select(delegate(KeyValuePair<string, string> pair)
					{
						string key = pair.Key;
						string value = StardewValley.Dialogue.applyGenderSwitch(str: pair.Value, gender: Game1.player.Gender, altTokenOnly: true);
						return new KeyValuePair<string, string>(key, value);
					}).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
					this.LoadedDialogueKey = dialogue_file;
				}
				catch (ContentLoadException)
				{
					NPC.invalidDialogueFiles.Add(dialogue_file);
					this.dialogue = new Dictionary<string, string>();
					this.LoadedDialogueKey = null;
				}
			}
			return this.dialogue;
		}
	}

	/// <summary>The dialogue key that was loaded via <see cref="P:StardewValley.NPC.Dialogue" />, if any.</summary>
	[XmlIgnore]
	public string LoadedDialogueKey { get; private set; }

	[XmlIgnore]
	public string DefaultMap
	{
		get
		{
			return this.defaultMap.Value;
		}
		set
		{
			this.defaultMap.Value = value;
		}
	}

	public Vector2 DefaultPosition
	{
		get
		{
			return this.defaultPosition.Value;
		}
		set
		{
			this.defaultPosition.Value = value;
		}
	}

	[XmlIgnore]
	public Texture2D Portrait
	{
		get
		{
			if (this.portrait == null && this.IsVillager)
			{
				this.ChooseAppearance();
			}
			return this.portrait;
		}
		set
		{
			this.portrait = value;
		}
	}

	/// <summary>Whether this NPC can dynamically change appearance based on their data in <c>Data/Characters</c>. This can be disabled for temporary NPCs and event actors.</summary>
	[XmlIgnore]
	public bool AllowDynamicAppearance { get; set; } = true;

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => true;

	/// <summary>The schedule of this NPC's movements and actions today, if loaded. The key is the time of departure, and the value is a list of directions to reach the new position.</summary>
	/// <remarks>You can set the schedule using <see cref="M:StardewValley.NPC.TryLoadSchedule" /> or one of its overloads.</remarks>
	[XmlIgnore]
	public Dictionary<int, SchedulePathDescription> Schedule { get; private set; }

	/// <summary>The <see cref="P:StardewValley.NPC.Schedule" />'s key in the original data asset, if loaded.</summary>
	[XmlIgnore]
	public string ScheduleKey => this.dayScheduleName.Value;

	public bool IsWalkingInSquare
	{
		get
		{
			return this.isWalkingInSquare;
		}
		set
		{
			this.isWalkingInSquare = value;
		}
	}

	public bool IsWalkingTowardPlayer
	{
		get
		{
			return this.isWalkingTowardPlayer.Value;
		}
		set
		{
			this.isWalkingTowardPlayer.Value = value;
		}
	}

	[XmlIgnore]
	public virtual Stack<Dialogue> CurrentDialogue
	{
		get
		{
			if (this.TemporaryDialogue != null)
			{
				return this.TemporaryDialogue;
			}
			if (Game1.npcDialogues == null)
			{
				Game1.npcDialogues = new Dictionary<string, Stack<Dialogue>>();
			}
			if (!this.IsVillager)
			{
				return NPC._EmptyDialogue;
			}
			Game1.npcDialogues.TryGetValue(base.Name, out var currentDialogue);
			if (currentDialogue == null)
			{
				return Game1.npcDialogues[base.Name] = this.loadCurrentDialogue();
			}
			return currentDialogue;
		}
		set
		{
			if (this.TemporaryDialogue != null)
			{
				this.TemporaryDialogue = value;
			}
			else if (Game1.npcDialogues != null)
			{
				Game1.npcDialogues[base.Name] = value;
			}
		}
	}

	[XmlIgnore]
	public string Birthday_Season
	{
		get
		{
			return this.birthday_Season.Value;
		}
		set
		{
			this.birthday_Season.Value = value;
		}
	}

	[XmlIgnore]
	public int Birthday_Day
	{
		get
		{
			return this.birthday_Day.Value;
		}
		set
		{
			this.birthday_Day.Value = value;
		}
	}

	[XmlIgnore]
	public int Age
	{
		get
		{
			return this.age.Value;
		}
		set
		{
			this.age.Value = value;
		}
	}

	[XmlIgnore]
	public int Manners
	{
		get
		{
			return this.manners.Value;
		}
		set
		{
			this.manners.Value = value;
		}
	}

	[XmlIgnore]
	public int SocialAnxiety
	{
		get
		{
			return this.socialAnxiety.Value;
		}
		set
		{
			this.socialAnxiety.Value = value;
		}
	}

	[XmlIgnore]
	public int Optimism
	{
		get
		{
			return this.optimism.Value;
		}
		set
		{
			this.optimism.Value = value;
		}
	}

	/// <summary>The character's gender identity.</summary>
	[XmlIgnore]
	public override Gender Gender
	{
		get
		{
			return this.gender.Value;
		}
		set
		{
			this.gender.Value = value;
		}
	}

	[XmlIgnore]
	public bool Breather
	{
		get
		{
			return this.breather.Value;
		}
		set
		{
			this.breather.Value = value;
		}
	}

	[XmlIgnore]
	public bool HideShadow
	{
		get
		{
			return this.hideShadow.Value;
		}
		set
		{
			this.hideShadow.Value = value;
		}
	}

	[XmlIgnore]
	public bool HasPartnerForDance
	{
		get
		{
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				if (onlineFarmer.dancePartner.TryGetVillager() == this)
				{
					return true;
				}
			}
			return false;
		}
	}

	[XmlIgnore]
	public bool IsInvisible
	{
		get
		{
			return this.isInvisible.Value;
		}
		set
		{
			this.isInvisible.Value = value;
		}
	}

	/// <summary>Whether social features (like birthdays, gift giving, friendship, and an entry in the social tab) are enabled for this NPC.</summary>
	public virtual bool CanSocialize
	{
		get
		{
			if (this.IsVillager)
			{
				return NPC.CanSocializePerData(base.Name, base.currentLocation);
			}
			return false;
		}
	}

	public NPC()
	{
	}

	public NPC(AnimatedSprite sprite, Vector2 position, int facingDir, string name, LocalizedContentManager content = null)
		: base(sprite, position, 2, name)
	{
		this.faceDirection(facingDir);
		this.defaultPosition.Value = position;
		this.defaultFacingDirection = facingDir;
		this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
		if (content != null)
		{
			try
			{
				this.portrait = content.Load<Texture2D>("Portraits\\" + name);
			}
			catch (Exception)
			{
			}
		}
	}

	public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Texture2D portrait)
		: this(sprite, position, defaultMap, facingDirection, name, portrait, eventActor: false)
	{
		this.datable.Value = datable;
	}

	public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDir, string name, Texture2D portrait, bool eventActor)
		: base(sprite, position, 2, name)
	{
		this.portrait = portrait;
		this.faceDirection(facingDir);
		if (!eventActor)
		{
			this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
		}
		this.reloadData();
		this.defaultPosition.Value = position;
		this.defaultMap.Value = defaultMap;
		base.currentLocation = Game1.getLocationFromName(defaultMap);
		this.defaultFacingDirection = facingDir;
	}

	public virtual void reloadData()
	{
		if (this is Child)
		{
			return;
		}
		CharacterData data = this.GetData();
		if (data != null)
		{
			this.Age = (int)Utility.GetEnumOrDefault(data.Age, NpcAge.Adult);
			this.Manners = (int)Utility.GetEnumOrDefault(data.Manner, NpcManner.Neutral);
			this.SocialAnxiety = (int)Utility.GetEnumOrDefault(data.SocialAnxiety, NpcSocialAnxiety.Outgoing);
			this.Optimism = (int)Utility.GetEnumOrDefault(data.Optimism, NpcOptimism.Positive);
			this.Gender = Utility.GetEnumOrDefault(data.Gender, Gender.Male);
			this.datable.Value = data.CanBeRomanced;
			this.loveInterest = data.LoveInterest;
			this.Birthday_Season = (data.BirthSeason.HasValue ? Utility.getSeasonKey(data.BirthSeason.Value) : null);
			this.Birthday_Day = data.BirthDay;
			this.id = ((data.FestivalVanillaActorIndex > -1) ? data.FestivalVanillaActorIndex : Game1.hash.GetDeterministicHashCode(base.name.Value));
			this.breather.Value = data.Breather;
			if (!this.isMarried())
			{
				this.reloadDefaultLocation();
			}
			this.displayName = this.translateName();
		}
	}

	public virtual void reloadDefaultLocation()
	{
		CharacterData data = this.GetData();
		if (data != null && NPC.ReadNpcHomeData(data, base.currentLocation, out var locationName, out var tile, out var direction))
		{
			this.DefaultMap = locationName;
			this.DefaultPosition = new Vector2(tile.X * 64, tile.Y * 64);
			this.DefaultFacingDirection = direction;
		}
	}

	/// <summary>Get an NPC's home location from its data, or fallback values if it doesn't exist.</summary>
	/// <param name="data">The character data for the NPC.</param>
	/// <param name="currentLocation">The NPC's current location, if applicable.</param>
	/// <param name="locationName">The internal name of the NPC's default map.</param>
	/// <param name="tile">The NPC's default tile position within the <paramref name="locationName" />.</param>
	/// <param name="direction">The default facing direction.</param>
	/// <returns>Returns whether a valid home was found in the given character data.</returns>
	public static bool ReadNpcHomeData(CharacterData data, GameLocation currentLocation, out string locationName, out Point tile, out int direction)
	{
		if (data?.Home != null)
		{
			foreach (CharacterHomeData home in data.Home)
			{
				if (home.Condition == null || GameStateQuery.CheckConditions(home.Condition, currentLocation))
				{
					locationName = home.Location;
					tile = home.Tile;
					direction = (Utility.TryParseDirection(home.Direction, out var parsedDirection) ? parsedDirection : 0);
					return true;
				}
			}
		}
		locationName = "Town";
		tile = new Point(29, 67);
		direction = 2;
		return false;
	}

	public virtual bool canTalk()
	{
		return true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.birthday_Season, "birthday_Season").AddField(this.birthday_Day, "birthday_Day").AddField(this.datable, "datable")
			.AddField(this.shouldPlayRobinHammerAnimation, "shouldPlayRobinHammerAnimation")
			.AddField(this.shouldPlaySpousePatioAnimation, "shouldPlaySpousePatioAnimation")
			.AddField(this.isWalkingTowardPlayer, "isWalkingTowardPlayer")
			.AddField(this.moveTowardPlayerThreshold, "moveTowardPlayerThreshold")
			.AddField(this.age, "age")
			.AddField(this.manners, "manners")
			.AddField(this.socialAnxiety, "socialAnxiety")
			.AddField(this.optimism, "optimism")
			.AddField(this.gender, "gender")
			.AddField(this.breather, "breather")
			.AddField(this.isSleeping, "isSleeping")
			.AddField(this.hideShadow, "hideShadow")
			.AddField(this.isInvisible, "isInvisible")
			.AddField(this.defaultMap, "defaultMap")
			.AddField(this.defaultPosition, "defaultPosition")
			.AddField(this.removeHenchmanEvent, "removeHenchmanEvent")
			.AddField(this.doingEndOfRouteAnimation, "doingEndOfRouteAnimation")
			.AddField(this.goingToDoEndOfRouteAnimation, "goingToDoEndOfRouteAnimation")
			.AddField(this.endOfRouteMessage, "endOfRouteMessage")
			.AddField(this.endOfRouteBehaviorName, "endOfRouteBehaviorName")
			.AddField(this.lastSeenMovieWeek, "lastSeenMovieWeek")
			.AddField(this.currentMarriageDialogue, "currentMarriageDialogue")
			.AddField(this.marriageDefaultDialogue, "marriageDefaultDialogue")
			.AddField(this.shouldSayMarriageDialogue, "shouldSayMarriageDialogue")
			.AddField(this.hasBeenKissedToday, "hasBeenKissedToday")
			.AddField(this.hasSaidAfternoonDialogue, "hasSaidAfternoonDialogue")
			.AddField(this.dayScheduleName, "dayScheduleName")
			.AddField(this.islandScheduleName, "islandScheduleName")
			.AddField(this.sleptInBed, "sleptInBed")
			.AddField(this.shouldWearIslandAttire, "shouldWearIslandAttire")
			.AddField(this.isMovingOnPathFindPath, "isMovingOnPathFindPath");
		base.position.Field.AxisAlignedMovement = true;
		this.removeHenchmanEvent.onEvent += performRemoveHenchman;
	}

	/// <summary>Reload the NPC's sprite or portrait based on their character data within the current context.</summary>
	/// <param name="content">The content manager from which to load assets, or <c>null</c> for the default content manager.</param>
	public virtual void ChooseAppearance(LocalizedContentManager content = null)
	{
		this.LastAppearanceId = null;
		if (base.SimpleNonVillagerNPC)
		{
			return;
		}
		content = content ?? Game1.content;
		GameLocation location = base.currentLocation;
		if (location == null)
		{
			return;
		}
		this.LastLocationNameForAppearance = location.NameOrUniqueName;
		bool appliedLegacyUniquePortraits = false;
		if (location.TryGetMapProperty("UniquePortrait", out var uniquePortraitsProperty) && ArgUtility.SplitBySpace(uniquePortraitsProperty).Contains(base.Name))
		{
			string assetName = "Portraits\\" + this.getTextureName() + "_" + location.Name;
			appliedLegacyUniquePortraits = this.TryLoadPortraits(assetName, out var errorPhrase, content);
			if (!appliedLegacyUniquePortraits)
			{
				Game1.log.Warn($"NPC {base.Name} can't load portraits from '{assetName}' (per the {"UniquePortrait"} map property in '{location.NameOrUniqueName}'): {errorPhrase}. Falling back to default portraits.");
			}
		}
		bool appliedLegacyUniqueSprites = false;
		if (location.TryGetMapProperty("UniqueSprite", out var uniqueSpritesProperty) && ArgUtility.SplitBySpace(uniqueSpritesProperty).Contains(base.Name))
		{
			string assetName2 = "Characters\\" + this.getTextureName() + "_" + location.Name;
			appliedLegacyUniqueSprites = this.TryLoadSprites(assetName2, out var errorPhrase2, content);
			if (!appliedLegacyUniqueSprites)
			{
				Game1.log.Warn($"NPC {base.Name} can't load sprites from '{assetName2}' (per the {"UniqueSprite"} map property in '{location.NameOrUniqueName}'): {errorPhrase2}. Falling back to default sprites.");
			}
		}
		if (appliedLegacyUniquePortraits && appliedLegacyUniqueSprites)
		{
			return;
		}
		CharacterData data = null;
		CharacterAppearanceData appearance = null;
		if (!this.IsMonster)
		{
			data = this.GetData();
			if (data != null && data.Appearance?.Count > 0)
			{
				List<CharacterAppearanceData> possibleOptions = new List<CharacterAppearanceData>();
				int totalWeight = 0;
				Random random = Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(base.Name));
				Season season = location.GetSeason();
				bool isOutdoors = location.IsOutdoors;
				int precedence = int.MaxValue;
				foreach (CharacterAppearanceData option in data.Appearance)
				{
					if (option.Precedence > precedence || option.IsIslandAttire != this.isWearingIslandAttire)
					{
						continue;
					}
					Season? season2 = option.Season;
					if ((!season2.HasValue || option.Season.Value == season) && (isOutdoors ? option.Outdoors : option.Indoors) && GameStateQuery.CheckConditions(option.Condition, location, null, null, null, random))
					{
						if (option.Precedence < precedence)
						{
							precedence = option.Precedence;
							possibleOptions.Clear();
							totalWeight = 0;
						}
						possibleOptions.Add(option);
						totalWeight += option.Weight;
					}
				}
				switch (possibleOptions.Count)
				{
				case 1:
					appearance = possibleOptions[0];
					break;
				default:
				{
					appearance = possibleOptions[possibleOptions.Count - 1];
					int cursor = Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode(base.Name)).Next(totalWeight + 1);
					foreach (CharacterAppearanceData option2 in possibleOptions)
					{
						cursor -= option2.Weight;
						if (cursor <= 0)
						{
							appearance = option2;
							break;
						}
					}
					break;
				}
				case 0:
					break;
				}
			}
		}
		if (!appliedLegacyUniquePortraits)
		{
			string defaultAsset = "Portraits/" + this.getTextureName();
			bool loaded = false;
			if (appearance != null && appearance.Portrait != null && appearance.Portrait != defaultAsset)
			{
				loaded = this.TryLoadPortraits(appearance.Portrait, out var errorPhrase3, content);
				if (!loaded)
				{
					Game1.log.Warn($"NPC {base.Name} can't load portraits from '{appearance.Portrait}' (per appearance entry '{appearance.Id}' in Data/Characters): {errorPhrase3}. Falling back to default portraits.");
				}
			}
			if (!loaded && this.isWearingIslandAttire)
			{
				string beachAsset = defaultAsset + "_Beach";
				if (content.DoesAssetExist<Texture2D>(beachAsset))
				{
					loaded = this.TryLoadPortraits(beachAsset, out var errorPhrase4, content);
					if (!loaded)
					{
						Game1.log.Warn($"NPC {base.Name} can't load portraits from '{beachAsset}' for island attire: {errorPhrase4}. Falling back to default portraits.");
					}
				}
			}
			if (!loaded && !this.TryLoadPortraits(defaultAsset, out var errorPhrase5, content))
			{
				Game1.log.Warn($"NPC {base.Name} can't load portraits from '{defaultAsset}': {errorPhrase5}.");
			}
			if (loaded)
			{
				this.LastAppearanceId = appearance?.Id;
			}
		}
		if (!appliedLegacyUniqueSprites)
		{
			string defaultAsset2 = "Characters/" + this.getTextureName();
			bool loaded2 = false;
			if (appearance != null && appearance.Sprite != null && appearance.Sprite != defaultAsset2)
			{
				loaded2 = this.TryLoadSprites(appearance.Sprite, out var errorPhrase6, content);
				if (!loaded2)
				{
					Game1.log.Warn($"NPC {base.Name} can't load sprites from '{appearance.Sprite}' (per appearance entry '{appearance.Id}' in Data/Characters): {errorPhrase6}. Falling back to default sprites.");
				}
			}
			if (!loaded2 && this.isWearingIslandAttire)
			{
				string beachAsset2 = defaultAsset2 + "_Beach";
				if (content.DoesAssetExist<Texture2D>(beachAsset2))
				{
					loaded2 = this.TryLoadSprites(beachAsset2, out var errorPhrase7, content);
					if (!loaded2)
					{
						Game1.log.Warn($"NPC {base.Name} can't load sprites from '{beachAsset2}' for island attire: {errorPhrase7}. Falling back to default sprites.");
					}
				}
			}
			if (!loaded2 && !this.TryLoadSprites(defaultAsset2, out var errorPhrase8, content))
			{
				Game1.log.Warn($"NPC {base.Name} can't load sprites from '{defaultAsset2}': {errorPhrase8}.");
			}
			if (loaded2)
			{
				this.LastAppearanceId = appearance?.Id;
			}
		}
		if (data != null && this.Sprite != null)
		{
			this.Sprite.SpriteWidth = data.Size.X;
			this.Sprite.SpriteHeight = data.Size.Y;
			this.Sprite.ignoreSourceRectUpdates = false;
		}
	}

	protected override string translateName()
	{
		return NPC.GetDisplayName(base.name.Value);
	}

	public string getName()
	{
		if (this.displayName != null && this.displayName.Length > 0)
		{
			return this.displayName;
		}
		return base.Name;
	}

	public virtual string getTextureName()
	{
		return NPC.getTextureNameForCharacter(base.Name);
	}

	public static string getTextureNameForCharacter(string character_name)
	{
		NPC.TryGetData(character_name, out var data);
		string textureName = data?.TextureName;
		if (string.IsNullOrEmpty(textureName))
		{
			return character_name;
		}
		return textureName;
	}

	public void resetSeasonalDialogue()
	{
		this.dialogue = null;
	}

	public void performSpecialScheduleChanges()
	{
		if (this.Schedule == null || !base.Name.Equals("Pam") || !Game1.MasterPlayer.mailReceived.Contains("ccVault"))
		{
			return;
		}
		bool foundBus = false;
		foreach (KeyValuePair<int, SchedulePathDescription> v in this.Schedule)
		{
			bool pamGone = false;
			switch (v.Value.targetLocationName)
			{
			case "BusStop":
				foundBus = true;
				break;
			case "DesertFestival":
			case "Desert":
			case "IslandSouth":
			{
				BusStop busStop = Game1.RequireLocation<BusStop>("BusStop");
				Game1.netWorldState.Value.canDriveYourselfToday.Value = true;
				Object sign = ItemRegistry.Create<Object>("(BC)TextSign");
				sign.signText.Value = TokenStringBuilder.LocalizedText((v.Value.targetLocationName == "IslandSouth") ? "Strings\\1_6_Strings:Pam_busSign_resort" : "Strings\\1_6_Strings:Pam_busSign");
				sign.SpecialVariable = 987659;
				busStop.tryPlaceObject(new Vector2(25f, 10f), sign);
				foundBus = true;
				pamGone = true;
				break;
			}
			}
			if (pamGone)
			{
				break;
			}
		}
		if (!foundBus && !Game1.isGreenRain)
		{
			BusStop obj = Game1.getLocationFromName("BusStop") as BusStop;
			Game1.netWorldState.Value.canDriveYourselfToday.Value = true;
			Object sign2 = (Object)ItemRegistry.Create("(BC)TextSign");
			sign2.signText.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:Pam_busSign_generic");
			sign2.SpecialVariable = 987659;
			obj.tryPlaceObject(new Vector2(25f, 10f), sign2);
		}
	}

	/// <summary>Update the NPC state (including sprite, dialogue, facing direction, schedules, etc). Despite the name, this doesn't only affect the sprite.</summary>
	/// <param name="onlyAppearance">Only reload the NPC's appearance (e.g. sprite, portraits, or breather/shadow fields), don't change any other data.</param>
	public virtual void reloadSprite(bool onlyAppearance = false)
	{
		if (base.SimpleNonVillagerNPC)
		{
			return;
		}
		this.ChooseAppearance();
		if (onlyAppearance || (!Game1.newDay && Game1.gameMode != 6))
		{
			return;
		}
		this.faceDirection(this.DefaultFacingDirection);
		this.previousEndPoint = new Point((int)this.defaultPosition.X / 64, (int)this.defaultPosition.Y / 64);
		this.TryLoadSchedule();
		this.performSpecialScheduleChanges();
		this.resetSeasonalDialogue();
		this.resetCurrentDialogue();
		this.updateConstructionAnimation();
		try
		{
			this.displayName = this.translateName();
		}
		catch (Exception)
		{
		}
	}

	/// <summary>Try to load a portraits texture, or keep the current texture if the load fails.</summary>
	/// <param name="assetName">The asset name to load.</param>
	/// <param name="error">If loading the portrait failed, an error phrase indicating why it failed.</param>
	/// <param name="content">The content manager from which to load the asset, or <c>null</c> for the default content manager.</param>
	/// <returns>Returns whether the texture was successfully loaded.</returns>
	public bool TryLoadPortraits(string assetName, out string error, LocalizedContentManager content = null)
	{
		if (base.Name == "Raccoon" || base.Name == "MrsRaccoon")
		{
			error = null;
			return true;
		}
		if (this.portraitOverridden)
		{
			error = null;
			return true;
		}
		if (string.IsNullOrWhiteSpace(assetName))
		{
			error = "the asset name is empty";
			return false;
		}
		if (this.portrait?.Name == assetName && !this.portrait.IsDisposed)
		{
			error = null;
			return true;
		}
		if (content == null)
		{
			content = Game1.content;
		}
		try
		{
			this.portrait = content.Load<Texture2D>(assetName);
			this.portrait.Name = assetName;
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.ToString();
			return false;
		}
	}

	/// <summary>Try to load a sprite texture, or keep the current texture if the load fails.</summary>
	/// <param name="assetName">The asset name to load.</param>
	/// <param name="error">If loading the portrait failed, an error phrase indicating why it failed.</param>
	/// <param name="content">The content manager from which to load the asset, or <c>null</c> for the default content manager.</param>
	/// <param name="logOnFail">Whether to log a warning if the texture can't be loaded.</param>
	/// <returns>Returns whether the texture was successfully loaded.</returns>
	public bool TryLoadSprites(string assetName, out string error, LocalizedContentManager content = null)
	{
		if (this.spriteOverridden)
		{
			error = null;
			return true;
		}
		if (string.IsNullOrWhiteSpace(assetName))
		{
			error = "the asset name is empty";
			return false;
		}
		if (this.Sprite?.spriteTexture != null && ((this.Sprite.overrideTextureName ?? this.Sprite.textureName.Value) == assetName || this.Sprite.spriteTexture.Name == assetName) && !this.Sprite.spriteTexture.IsDisposed)
		{
			error = null;
			return true;
		}
		if (content == null)
		{
			content = Game1.content;
		}
		try
		{
			if (this.Sprite == null)
			{
				this.Sprite = new AnimatedSprite(content, assetName);
			}
			else
			{
				this.Sprite.LoadTexture(assetName, Game1.IsMasterGame);
			}
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.ToString();
			return false;
		}
	}

	private void updateConstructionAnimation()
	{
		bool isFestivalDay = Utility.isFestivalDay();
		if (Game1.IsMasterGame && base.Name == "Robin" && !isFestivalDay && (!Game1.isGreenRain || Game1.year > 1))
		{
			if (Game1.player.daysUntilHouseUpgrade.Value > 0)
			{
				Farm farm = Game1.getFarm();
				Game1.warpCharacter(this, farm.NameOrUniqueName, new Vector2(farm.GetMainFarmHouseEntry().X + 4, farm.GetMainFarmHouseEntry().Y - 1));
				this.isPlayingRobinHammerAnimation = false;
				this.shouldPlayRobinHammerAnimation.Value = true;
				return;
			}
			if (Game1.IsThereABuildingUnderConstruction())
			{
				Building b = Game1.GetBuildingUnderConstruction();
				if (b == null)
				{
					return;
				}
				GameLocation indoors = b.GetIndoors();
				if (b.daysUntilUpgrade.Value > 0 && indoors != null)
				{
					base.currentLocation?.characters.Remove(this);
					base.currentLocation = indoors;
					if (base.currentLocation != null && !base.currentLocation.characters.Contains(this))
					{
						base.currentLocation.addCharacter(this);
					}
					string indoorsName = b.GetIndoorsName();
					if (indoorsName != null && indoorsName.StartsWith("Shed"))
					{
						this.setTilePosition(2, 2);
						base.position.X -= 28f;
					}
					else
					{
						this.setTilePosition(1, 5);
					}
				}
				else
				{
					Game1.warpCharacter(this, b.parentLocationName.Value, new Vector2(b.tileX.Value + b.tilesWide.Value / 2, b.tileY.Value + b.tilesHigh.Value / 2));
					base.position.X += 16f;
					base.position.Y -= 32f;
				}
				this.isPlayingRobinHammerAnimation = false;
				this.shouldPlayRobinHammerAnimation.Value = true;
				return;
			}
			if (Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value > 0)
			{
				if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Game1.warpCharacter(this, "Backwoods", new Vector2(41f, 23f));
					this.isPlayingRobinHammerAnimation = false;
					this.shouldPlayRobinHammerAnimation.Value = true;
				}
				else if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Game1.warpCharacter(this, "Town", new Vector2(77f, 68f));
					this.isPlayingRobinHammerAnimation = false;
					this.shouldPlayRobinHammerAnimation.Value = true;
				}
				return;
			}
		}
		this.shouldPlayRobinHammerAnimation.Value = false;
	}

	private void doPlayRobinHammerAnimation()
	{
		this.Sprite.ClearAnimation();
		this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(24, 75));
		this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(25, 75));
		this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(26, 300, secondaryArm: false, flip: false, robinHammerSound));
		this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(27, 1000, secondaryArm: false, flip: false, robinVariablePause));
		this.ignoreScheduleToday = true;
		bool oneDayLeft = Game1.player.daysUntilHouseUpgrade.Value == 1 || Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value == 1;
		this.CurrentDialogue.Clear();
		this.CurrentDialogue.Push(new Dialogue(this, oneDayLeft ? "Strings\\StringsFromCSFiles:NPC.cs.3927" : "Strings\\StringsFromCSFiles:NPC.cs.3926"));
	}

	public void showTextAboveHead(string text, Color? spriteTextColor = null, int style = 2, int duration = 3000, int preTimer = 0)
	{
		if (!this.IsInvisible)
		{
			this.textAboveHeadAlpha = 0f;
			this.textAboveHead = StardewValley.Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
			this.textAboveHeadPreTimer = preTimer;
			this.textAboveHeadTimer = duration;
			this.textAboveHeadStyle = style;
			this.textAboveHeadColor = spriteTextColor;
		}
	}

	public virtual bool hitWithTool(Tool t)
	{
		return false;
	}

	/// <summary>Get whether this NPC can receive gifts from the player (regardless of whether they've already received one today).</summary>
	public bool CanReceiveGifts()
	{
		if (this.CanSocialize && !base.SimpleNonVillagerNPC && Game1.NPCGiftTastes.ContainsKey(base.Name))
		{
			return this.GetData()?.CanReceiveGifts ?? true;
		}
		return false;
	}

	/// <summary>Get how much the NPC likes receiving an item as a gift.</summary>
	/// <param name="item">The item to check.</param>
	/// <returns>Returns one of <see cref="F:StardewValley.NPC.gift_taste_hate" />, <see cref="F:StardewValley.NPC.gift_taste_dislike" />, <see cref="F:StardewValley.NPC.gift_taste_neutral" />, <see cref="F:StardewValley.NPC.gift_taste_like" />, or <see cref="F:StardewValley.NPC.gift_taste_love" />.</returns>
	public int getGiftTasteForThisItem(Item item)
	{
		if (item.QualifiedItemId == "(O)StardropTea")
		{
			return 7;
		}
		int tasteForItem = 8;
		if (item is Object { Category: var categoryNumber } obj)
		{
			string categoryNumberString = categoryNumber.ToString() ?? "";
			string[] universalLoves = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Love"]);
			string[] universalHates = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Hate"]);
			string[] universalLikes = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Like"]);
			string[] universalDislikes = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Dislike"]);
			string[] universalNeutrals = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Neutral"]);
			if (universalLoves.Contains(categoryNumberString))
			{
				tasteForItem = 0;
			}
			else if (universalHates.Contains(categoryNumberString))
			{
				tasteForItem = 6;
			}
			else if (universalLikes.Contains(categoryNumberString))
			{
				tasteForItem = 2;
			}
			else if (universalDislikes.Contains(categoryNumberString))
			{
				tasteForItem = 4;
			}
			if (this.CheckTasteContextTags(obj, universalLoves))
			{
				tasteForItem = 0;
			}
			else if (this.CheckTasteContextTags(obj, universalHates))
			{
				tasteForItem = 6;
			}
			else if (this.CheckTasteContextTags(obj, universalLikes))
			{
				tasteForItem = 2;
			}
			else if (this.CheckTasteContextTags(obj, universalDislikes))
			{
				tasteForItem = 4;
			}
			bool wasIndividualUniversal = false;
			bool skipDefaultValueRules = false;
			if (this.CheckTaste(universalLoves, obj))
			{
				tasteForItem = 0;
				wasIndividualUniversal = true;
			}
			else if (this.CheckTaste(universalHates, obj))
			{
				tasteForItem = 6;
				wasIndividualUniversal = true;
			}
			else if (this.CheckTaste(universalLikes, obj))
			{
				tasteForItem = 2;
				wasIndividualUniversal = true;
			}
			else if (this.CheckTaste(universalDislikes, obj))
			{
				tasteForItem = 4;
				wasIndividualUniversal = true;
			}
			else if (this.CheckTaste(universalNeutrals, obj))
			{
				tasteForItem = 8;
				wasIndividualUniversal = true;
				skipDefaultValueRules = true;
			}
			if (obj.Type == "Arch")
			{
				tasteForItem = 4;
				if (base.Name.Equals("Penny") || base.name.Equals("Dwarf"))
				{
					tasteForItem = 2;
				}
			}
			if (tasteForItem == 8 && !skipDefaultValueRules)
			{
				if (obj.edibility.Value != -300 && obj.edibility.Value < 0)
				{
					tasteForItem = 6;
				}
				else if (obj.price.Value < 20)
				{
					tasteForItem = 4;
				}
			}
			if (Game1.NPCGiftTastes.TryGetValue(base.Name, out var dispositionData))
			{
				string[] split = dispositionData.Split('/');
				List<string[]> items = new List<string[]>();
				for (int i = 0; i < 10; i += 2)
				{
					string[] splitItems = ArgUtility.SplitBySpace(split[i + 1]);
					string[] thisItems = new string[splitItems.Length];
					for (int j = 0; j < splitItems.Length; j++)
					{
						if (splitItems[j].Length > 0)
						{
							thisItems[j] = splitItems[j];
						}
					}
					items.Add(thisItems);
				}
				if (this.CheckTaste(items[0], obj))
				{
					return 0;
				}
				if (this.CheckTaste(items[3], obj))
				{
					return 6;
				}
				if (this.CheckTaste(items[1], obj))
				{
					return 2;
				}
				if (this.CheckTaste(items[2], obj))
				{
					return 4;
				}
				if (this.CheckTaste(items[4], obj))
				{
					return 8;
				}
				if (this.CheckTasteContextTags(obj, items[0]))
				{
					return 0;
				}
				if (this.CheckTasteContextTags(obj, items[3]))
				{
					return 6;
				}
				if (this.CheckTasteContextTags(obj, items[1]))
				{
					return 2;
				}
				if (this.CheckTasteContextTags(obj, items[2]))
				{
					return 4;
				}
				if (this.CheckTasteContextTags(obj, items[4]))
				{
					return 8;
				}
				if (!wasIndividualUniversal)
				{
					if (categoryNumber != 0 && items[0].Contains(categoryNumberString))
					{
						return 0;
					}
					if (categoryNumber != 0 && items[3].Contains(categoryNumberString))
					{
						return 6;
					}
					if (categoryNumber != 0 && items[1].Contains(categoryNumberString))
					{
						return 2;
					}
					if (categoryNumber != 0 && items[2].Contains(categoryNumberString))
					{
						return 4;
					}
					if (categoryNumber != 0 && items[4].Contains(categoryNumberString))
					{
						return 8;
					}
				}
			}
		}
		return tasteForItem;
	}

	public bool CheckTaste(IEnumerable<string> list, Item item)
	{
		foreach (string item_entry in list)
		{
			if (item_entry != null && !item_entry.StartsWith('-'))
			{
				ParsedItemData data = ItemRegistry.GetData(item_entry);
				if (data?.ItemType != null && item.QualifiedItemId == data.QualifiedItemId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool CheckTasteContextTags(Item item, string[] list)
	{
		foreach (string entry in list)
		{
			if (entry != null && entry.Length > 0 && !char.IsNumber(entry[0]) && entry[0] != '-' && item.HasContextTag(entry))
			{
				return true;
			}
		}
		return false;
	}

	private void goblinDoorEndBehavior(Character c, GameLocation l)
	{
		l.characters.Remove(this);
		l.playSound("doorClose");
	}

	private void performRemoveHenchman()
	{
		this.Sprite.CurrentFrame = 4;
		Game1.netWorldState.Value.IsGoblinRemoved = true;
		Game1.player.removeQuest("27");
		Stack<Point> p = new Stack<Point>();
		p.Push(new Point(20, 21));
		p.Push(new Point(20, 22));
		p.Push(new Point(20, 23));
		p.Push(new Point(20, 24));
		p.Push(new Point(20, 25));
		p.Push(new Point(20, 26));
		p.Push(new Point(20, 27));
		p.Push(new Point(20, 28));
		this.addedSpeed = 2f;
		base.controller = new PathFindController(p, this, base.currentLocation);
		base.controller.endBehaviorFunction = goblinDoorEndBehavior;
		this.showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Henchman6"));
		Game1.player.mailReceived.Add("henchmanGone");
		base.currentLocation.removeTile(20, 29, "Buildings");
	}

	private void engagementResponse(Farmer who, bool asRoommate = false)
	{
		Game1.changeMusicTrack("silence");
		who.spouse = base.Name;
		if (!asRoommate)
		{
			Game1.multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, this.GetTokenizedDisplayName());
		}
		Friendship friendship = who.friendshipData[base.Name];
		friendship.Status = FriendshipStatus.Engaged;
		friendship.RoommateMarriage = asRoommate;
		WorldDate weddingDate = new WorldDate(Game1.Date);
		weddingDate.TotalDays += 3;
		who.removeDatingActiveDialogueEvents(Game1.player.spouse);
		while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
		{
			weddingDate.TotalDays++;
		}
		friendship.WeddingDate = weddingDate;
		this.CurrentDialogue.Clear();
		if (asRoommate && DataLoader.EngagementDialogue(Game1.content).ContainsKey(base.Name + "Roommate0"))
		{
			this.CurrentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + "Roommate0"));
			Dialogue attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_EngagedRoommate");
			if (attemptDialogue != null)
			{
				this.CurrentDialogue.Push(attemptDialogue);
			}
			else
			{
				attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
				if (attemptDialogue != null)
				{
					this.CurrentDialogue.Push(attemptDialogue);
				}
				else
				{
					this.CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3980"));
				}
			}
		}
		else
		{
			Dialogue attemptDialogue2 = StardewValley.Dialogue.TryGetDialogue(this, "Data\\EngagementDialogue:" + base.Name + "0");
			if (attemptDialogue2 != null)
			{
				this.CurrentDialogue.Push(attemptDialogue2);
			}
			attemptDialogue2 = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
			if (attemptDialogue2 != null)
			{
				this.CurrentDialogue.Push(attemptDialogue2);
			}
			else
			{
				this.CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3980"));
			}
		}
		Dialogue obj = this.CurrentDialogue.Peek();
		obj.onFinish = (Action)Delegate.Combine(obj.onFinish, (Action)delegate
		{
			Game1.changeMusicTrack("none", track_interruptable: true);
			GameLocation.HandleMusicChange(null, who.currentLocation);
		});
		who.changeFriendship(1, this);
		who.reduceActiveItemByOne();
		who.completelyStopAnimatingOrDoingAction();
		Game1.drawDialogue(this);
	}

	/// <summary>Try to receive an item from the player.</summary>
	/// <param name="who">The player whose active object to receive.</param>
	/// <param name="probe">Whether to return what the method would return if called normally, but without actually accepting the item or making any changes to the NPC. This is used to accurately predict whether the NPC would accept or react to the offer.</param>
	/// <returns>Returns true if the NPC accepted the item or reacted to the offer, else false.</returns>
	public virtual bool tryToReceiveActiveObject(Farmer who, bool probe = false)
	{
		if (base.SimpleNonVillagerNPC)
		{
			return false;
		}
		Object activeObj = who.ActiveObject;
		if (activeObj == null)
		{
			return false;
		}
		if (!probe)
		{
			who.Halt();
			who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
		}
		if (base.Name == "Henchman" && Game1.currentLocation.NameOrUniqueName == "WitchSwamp")
		{
			if (activeObj.QualifiedItemId == "(O)308")
			{
				if (base.controller != null)
				{
					return false;
				}
				if (!probe)
				{
					who.currentLocation.localSound("coin");
					who.reduceActiveItemByOne();
					this.CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman5"));
					Game1.drawDialogue(this);
					who.freezePause = 2000;
					this.removeHenchmanEvent.Fire();
				}
			}
			else if (!probe)
			{
				this.CurrentDialogue.Push(new Dialogue(this, (activeObj.QualifiedItemId == "(O)684") ? "Strings\\Characters:Henchman4" : "Strings\\Characters:Henchman3"));
				Game1.drawDialogue(this);
			}
			return true;
		}
		if (Game1.player.team.specialOrders != null)
		{
			foreach (SpecialOrder order in Game1.player.team.specialOrders)
			{
				if (order.onItemDelivered == null)
				{
					continue;
				}
				Delegate[] invocationList = order.onItemDelivered.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					if (((Func<Farmer, NPC, Item, bool, int>)invocationList[i])(Game1.player, this, activeObj, probe) > 0)
					{
						if (!probe && activeObj.Stack <= 0)
						{
							who.ActiveObject = null;
							who.showNotCarrying();
						}
						return true;
					}
				}
			}
		}
		if (who.NotifyQuests((Quest quest) => quest.OnItemOfferedToNpc(this, activeObj, probe), onlyOneQuest: true))
		{
			if (!probe)
			{
				who.completelyStopAnimatingOrDoingAction();
				if (Game1.random.NextDouble() < 0.3 && base.Name != "Wizard")
				{
					base.doEmote(32);
				}
			}
			return true;
		}
		switch (who.ActiveObject?.QualifiedItemId)
		{
		case "(O)233":
			if (base.name.Value == "Jas" && Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is Desert && !who.mailReceived.Contains("Jas_IceCream_DF_" + Game1.year))
			{
				if (!probe)
				{
					who.reduceActiveItemByOne();
					this.jump();
					base.doEmote(16);
					this.CurrentDialogue.Clear();
					this.setNewDialogue("Strings\\1_6_Strings:Jas_IceCream", add: true);
					Game1.drawDialogue(this);
					who.mailReceived.Add("Jas_IceCream_DF_" + Game1.year);
					who.changeFriendship(200, this);
				}
				return true;
			}
			break;
		case "(O)897":
			if (!probe)
			{
				if (base.Name == "Pierre" && !Game1.player.hasOrWillReceiveMail("PierreStocklist"))
				{
					Game1.addMail("PierreStocklist", noLetter: true, sendToEveryone: true);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					who.currentLocation.localSound("give_gift");
					Game1.player.team.itemsToRemoveOvernight.Add("897");
					this.setNewDialogue("Strings\\Characters:PierreStockListDialogue", add: true);
					Game1.drawDialogue(this);
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						Game1.multiplayer.globalChatInfoMessage("StockList");
					});
				}
				else
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
				}
			}
			return true;
		case "(O)71":
			if (base.Name == "Lewis" && who.hasQuest("102"))
			{
				if (!probe)
				{
					if (who.currentLocation?.NameOrUniqueName == "IslandSouth")
					{
						Game1.player.activeDialogueEvents["lucky_pants_lewis"] = 28;
					}
					who.completeQuest("102");
					string[] questFields = Quest.GetRawQuestFields("102");
					Dialogue thankYou = new Dialogue(this, null, ArgUtility.Get(questFields, 9, "Data\\ExtraDialogue:LostItemQuest_DefaultThankYou", allowBlank: false));
					this.setNewDialogue(thankYou);
					Game1.drawDialogue(this);
					Game1.player.changeFriendship(250, this);
					who.ActiveObject = null;
				}
				return true;
			}
			return false;
		case "(O)867":
		case "(O)864":
		case "(O)865":
		case "(O)866":
		case "(O)868":
		case "(O)869":
		case "(O)870":
			if (who.hasQuest("130"))
			{
				Dialogue dialogue = this.TryGetDialogue("accept_" + activeObj.ItemId);
				if (dialogue != null)
				{
					if (!probe)
					{
						this.setNewDialogue(dialogue);
						Game1.drawDialogue(this);
						this.CurrentDialogue.Peek().onFinish = delegate
						{
							Object o = ItemRegistry.Create<Object>("(O)" + (activeObj.ParentSheetIndex + 1));
							o.specialItem = true;
							o.questItem.Value = true;
							who.reduceActiveItemByOne();
							DelayedAction.playSoundAfterDelay("coin", 200);
							DelayedAction.functionAfterDelay(delegate
							{
								who.addItemByMenuIfNecessary(o);
							}, 200);
							Game1.player.freezePause = 550;
							DelayedAction.functionAfterDelay(delegate
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", o.DisplayName, Lexicon.getProperArticleForWord(o.DisplayName)));
							}, 550);
						};
						base.faceTowardFarmerForPeriod(6000, 4, faceAway: false, who);
					}
					return true;
				}
				dialogue = this.TryGetDialogue("reject_" + activeObj.ItemId);
				if (dialogue != null)
				{
					if (!probe)
					{
						this.setNewDialogue(dialogue);
						Game1.drawDialogue(this);
					}
					return true;
				}
			}
			return false;
		}
		if (activeObj.questItem.Value)
		{
			return false;
		}
		Dialogue dialogue2 = this.TryGetDialogue("RejectItem_" + activeObj.QualifiedItemId) ?? (from tag in activeObj.GetContextTags()
			select this.TryGetDialogue("RejectItem_" + tag)).FirstOrDefault((Dialogue p) => p != null) ?? (activeObj.HasTypeObject() ? this.TryGetDialogue("reject_" + activeObj.ItemId) : null);
		if (dialogue2 != null)
		{
			if (!probe)
			{
				this.setNewDialogue(dialogue2);
				Game1.drawDialogue(this);
			}
			return true;
		}
		who.friendshipData.TryGetValue(base.Name, out var friendship);
		bool canReceiveGifts = this.CanReceiveGifts();
		switch (activeObj.QualifiedItemId)
		{
		case "(O)809":
		{
			if (!Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
				}
				return true;
			}
			if (this.SpeaksDwarvish() && !who.canUnderstandDwarves)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
				}
				return true;
			}
			string text = base.Name;
			if (!(text == "Krobus"))
			{
				if (text == "Leo" && !Game1.MasterPlayer.mailReceived.Contains("leoMoved"))
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
					}
					return true;
				}
			}
			else if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
				}
				return true;
			}
			if (!this.IsVillager || !this.CanSocialize)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_CantInvite", this.displayName)));
				}
				return true;
			}
			if (friendship == null)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
				}
				return true;
			}
			if (friendship.IsDivorced())
			{
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, this.GetTokenizedDisplayName());
					}
					this.CurrentDialogue.Push(this.TryGetDialogue("RejectMovieTicket_Divorced") ?? this.TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:Divorced_gift"));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (who.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_FarmerAlreadySeen")));
				}
				return true;
			}
			if (Utility.isFestivalDay())
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Festival")));
				}
				return true;
			}
			if (Game1.timeOfDay > 2100)
			{
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Closed")));
				}
				return true;
			}
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who)
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_AlreadyInvitedSomeone", invitation.invitedNPC.displayName)));
					}
					return true;
				}
			}
			if (!probe)
			{
				base.faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
			}
			foreach (MovieInvitation invitation2 in who.team.movieInvitations)
			{
				if (invitation2.invitedNPC != this)
				{
					continue;
				}
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, this.GetTokenizedDisplayName());
					}
					this.CurrentDialogue.Push(this.TryGetDialogue("RejectMovieTicket_AlreadyInvitedBySomeoneElse", invitation2.farmer.displayName) ?? this.TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_InvitedBySomeoneElse", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_InvitedBySomeoneElse", invitation2.farmer.displayName)));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (this.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
			{
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, this.GetTokenizedDisplayName());
					}
					this.CurrentDialogue.Push(this.TryGetDialogue("RejectMovieTicket_AlreadyWatchedThisWeek") ?? this.TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_AlreadySeen", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_AlreadySeen")));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (MovieTheater.GetResponseForMovie(this) == "reject")
			{
				if (!probe)
				{
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", Game1.player.displayName, this.GetTokenizedDisplayName());
					}
					this.CurrentDialogue.Push(this.TryGetDialogue("RejectMovieTicket_DontWantToSeeThatMovie") ?? this.TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_Reject", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_Reject")));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (!probe)
			{
				this.CurrentDialogue.Push(((this.getSpouse() == who) ? StardewValley.Dialogue.TryGetDialogue(this, "Strings\\Characters:MovieInvite_Spouse_" + base.name.Value) : null) ?? this.TryGetDialogue("MovieInvitation") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_Invited", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_Invited")));
				Game1.drawDialogue(this);
				who.reduceActiveItemByOne();
				who.completelyStopAnimatingOrDoingAction();
				who.currentLocation.localSound("give_gift");
				MovieTheater.Invite(who, this);
				if (who == Game1.player)
				{
					Game1.multiplayer.globalChatInfoMessage("MovieInviteAccept", Game1.player.displayName, this.GetTokenizedDisplayName());
				}
			}
			return true;
		}
		case "(O)458":
			if (canReceiveGifts)
			{
				bool npcMarriedToSomeoneElse = who.spouse != base.Name && this.isMarriedOrEngaged();
				if (!this.datable.Value || npcMarriedToSomeoneElse)
				{
					if (!probe)
					{
						if (Game1.random.NextBool())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", this.displayName));
						}
						else
						{
							this.CurrentDialogue.Push(((!this.datable.Value) ? this.TryGetDialogue("RejectBouquet_NotDatable") : null) ?? (npcMarriedToSomeoneElse ? this.TryGetDialogue("RejectBouquet_NpcAlreadyMarried", this.getSpouse()?.Name) : null) ?? this.TryGetDialogue("RejectBouquet") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3956") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3957", isGendered: true)));
							Game1.drawDialogue(this);
						}
					}
					return true;
				}
				if (friendship == null)
				{
					friendship = (who.friendshipData[base.Name] = new Friendship());
				}
				if (friendship.IsDating())
				{
					if (!probe)
					{
						Dialogue dialogue5 = this.TryGetDialogue($"RejectBouquet_AlreadyAccepted_{friendship.Status}") ?? this.TryGetDialogue("RejectBouquet_AlreadyAccepted");
						if (dialogue5 != null)
						{
							this.CurrentDialogue.Push(dialogue5);
							Game1.drawDialogue(this);
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", this.displayName));
						}
					}
					return true;
				}
				if (friendship.IsDivorced())
				{
					if (!probe)
					{
						this.CurrentDialogue.Push(this.TryGetDialogue("RejectBouquet_Divorced") ?? this.TryGetDialogue("RejectBouquet") ?? new Dialogue(this, "Strings\\Characters:Divorced_bouquet"));
						Game1.drawDialogue(this);
					}
					return true;
				}
				if (friendship.Points < 1000)
				{
					if (!probe)
					{
						this.CurrentDialogue.Push(this.TryGetDialogue("RejectBouquet_VeryLowHearts") ?? this.TryGetDialogue("RejectBouquet") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3958") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3959", isGendered: true)));
						Game1.drawDialogue(this);
					}
					return true;
				}
				if (friendship.Points < 2000)
				{
					if (!probe)
					{
						this.CurrentDialogue.Push(this.TryGetDialogue("RejectBouquet_LowHearts") ?? this.TryGetDialogue("RejectBouquet") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3960", "3961")));
						Game1.drawDialogue(this);
					}
					return true;
				}
				if (!probe)
				{
					friendship.Status = FriendshipStatus.Dating;
					Game1.multiplayer.globalChatInfoMessage("Dating", Game1.player.Name, this.GetTokenizedDisplayName());
					this.CurrentDialogue.Push(this.TryGetDialogue("AcceptBouquet") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3962", "3963"), isGendered: true));
					who.autoGenerateActiveDialogueEvent("dating_" + base.Name);
					who.autoGenerateActiveDialogueEvent("dating");
					who.changeFriendship(25, this);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					base.doEmote(20);
					Game1.drawDialogue(this);
				}
				return true;
			}
			return false;
		case "(O)277":
			if (canReceiveGifts)
			{
				if (!probe)
				{
					if (!this.datable.Value || friendship == null || !friendship.IsDating() || (friendship != null && friendship.IsMarried()))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Meaningless", this.displayName));
					}
					else
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Effect", this.displayName));
						Game1.multiplayer.globalChatInfoMessage("BreakUp", Game1.player.Name, this.GetTokenizedDisplayName());
						who.removeDatingActiveDialogueEvents(base.Name);
						who.reduceActiveItemByOne();
						friendship.Status = FriendshipStatus.Friendly;
						if (who.spouse == base.Name)
						{
							who.spouse = null;
						}
						friendship.WeddingDate = null;
						who.completelyStopAnimatingOrDoingAction();
						friendship.Points = Math.Min(friendship.Points, 1250);
						switch (base.name.Value)
						{
						case "Maru":
						case "Haley":
							base.doEmote(12);
							break;
						default:
							base.doEmote(28);
							break;
						case "Shane":
						case "Alex":
							break;
						}
						this.CurrentDialogue.Clear();
						this.CurrentDialogue.Push(new Dialogue(this, "Characters\\Dialogue\\" + this.GetDialogueSheetName() + ":breakUp"));
						Game1.drawDialogue(this);
					}
				}
				return true;
			}
			return false;
		case "(O)460":
			if (canReceiveGifts)
			{
				bool isDivorced = friendship?.IsDivorced() ?? false;
				if (who.spouse == base.Name)
				{
					Dialogue dialogue4 = this.TryGetDialogue($"RejectMermaidPendant_AlreadyAccepted_{friendship?.Status}") ?? this.TryGetDialogue("RejectMermaidPendant_AlreadyAccepted");
					if (!probe && dialogue4 != null)
					{
						this.CurrentDialogue.Push(dialogue4);
						Game1.drawDialogue(this);
					}
					return dialogue4 != null;
				}
				if (who.isMarriedOrRoommates() || who.isEngaged())
				{
					if (!probe)
					{
						if (who.hasCurrentOrPendingRoommate())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TriedToMarryButKrobus"));
						}
						else if (who.isEngaged())
						{
							this.CurrentDialogue.Push(this.TryGetDialogue("RejectMermaidPendant_PlayerWithSomeoneElse", who.getSpouse()?.displayName ?? who.spouse) ?? this.TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3965", "3966"), isGendered: true));
							Game1.drawDialogue(this);
						}
						else
						{
							this.CurrentDialogue.Push(this.TryGetDialogue("RejectMermaidPendant_PlayerWithSomeoneElse") ?? this.TryGetDialogue("RejectMermaidPendant") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3967") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3968", isGendered: true)));
							Game1.drawDialogue(this);
						}
					}
					return true;
				}
				if (!this.datable.Value || this.isMarriedOrEngaged() || isDivorced || (friendship != null && friendship.Points < 1500))
				{
					if (!probe)
					{
						if (Game1.random.NextBool())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", this.displayName));
						}
						else
						{
							this.CurrentDialogue.Push(((!this.datable.Value) ? this.TryGetDialogue("RejectMermaidPendant_NotDatable") : null) ?? (isDivorced ? this.TryGetDialogue("RejectMermaidPendant_Divorced") : null) ?? (this.isMarriedOrEngaged() ? this.TryGetDialogue("RejectMermaidPendant_NpcWithSomeoneElse", this.getSpouse()?.Name) : null) ?? ((this.datable.Value && friendship != null && friendship.Points < 1500) ? this.TryGetDialogue("RejectMermaidPendant_Under8Hearts") : null) ?? this.TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + ((this.Gender == Gender.Female) ? "3970" : "3971")));
							Game1.drawDialogue(this);
						}
					}
					return true;
				}
				if (this.datable.Value && friendship != null && friendship.Points < 2500)
				{
					if (!probe)
					{
						if (!friendship.ProposalRejected)
						{
							this.CurrentDialogue.Push(this.TryGetDialogue("RejectMermaidPendant_Under10Hearts") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3972", "3973")));
							Game1.drawDialogue(this);
							who.changeFriendship(-20, this);
							friendship.ProposalRejected = true;
						}
						else
						{
							this.CurrentDialogue.Push(this.TryGetDialogue("RejectMermaidPendant_Under10Hearts_AskedAgain") ?? this.TryGetDialogue("RejectMermaidPendant_Under10Hearts") ?? this.TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3974", "3975"), isGendered: true));
							Game1.drawDialogue(this);
							who.changeFriendship(-50, this);
						}
					}
					return true;
				}
				if (this.datable.Value && who.houseUpgradeLevel.Value < 1)
				{
					if (!probe)
					{
						if (Game1.random.NextBool())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", this.displayName));
						}
						else
						{
							this.CurrentDialogue.Push(this.TryGetDialogue("RejectMermaidPendant_NeedHouseUpgrade") ?? this.TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3972"));
							Game1.drawDialogue(this);
						}
					}
					return true;
				}
				if (!probe)
				{
					this.engagementResponse(who);
				}
				return true;
			}
			return false;
		default:
		{
			if (canReceiveGifts && activeObj.HasContextTag(ItemContextTagManager.SanitizeContextTag("propose_roommate_" + base.Name)))
			{
				string failedKey = null;
				object[] substitutions = null;
				bool defaultToPuzzledMessage = base.Name != "Krobus";
				if (who.spouse == base.Name)
				{
					failedKey = "RejectRoommateProposal_AlreadyAccepted";
					defaultToPuzzledMessage = false;
				}
				else if (this.isMarriedOrEngaged())
				{
					failedKey = "RejectRoommateProposal_NpcWithSomeoneElse";
				}
				else if (who.isMarriedOrRoommates() || who.isEngaged())
				{
					failedKey = "RejectRoommateProposal_PlayerWithSomeoneElse";
					substitutions = new object[1] { who.getSpouse()?.displayName ?? who.spouse };
				}
				else if (who.getFriendshipHeartLevelForNPC(base.Name) < 10)
				{
					failedKey = "RejectRoommateProposal_LowFriendship";
				}
				else if (who.houseUpgradeLevel.Value < 1)
				{
					failedKey = "RejectRoommateProposal_SmallHouse";
				}
				if (failedKey != null)
				{
					Dialogue dialogue3 = ((substitutions != null) ? this.TryGetDialogue(failedKey, substitutions) : this.TryGetDialogue(failedKey)) ?? this.TryGetDialogue("RejectRoommateProposal");
					if (!probe)
					{
						if (dialogue3 != null)
						{
							this.CurrentDialogue.Push(dialogue3);
							Game1.drawDialogue(this);
						}
						else if (defaultToPuzzledMessage)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
						}
					}
					return dialogue3 != null || defaultToPuzzledMessage;
				}
				if (!probe)
				{
					this.engagementResponse(who, asRoommate: true);
				}
				return true;
			}
			bool obsoleteNotGiftable = ItemContextTagManager.HasBaseTag(activeObj.QualifiedItemId, "not_giftable");
			if (canReceiveGifts && activeObj.canBeGivenAsGift() && !obsoleteNotGiftable)
			{
				foreach (string activeKey in who.activeDialogueEvents.Keys)
				{
					if (activeKey.Contains("dumped") && this.Dialogue.ContainsKey(activeKey))
					{
						if (!probe)
						{
							base.doEmote(12);
						}
						return true;
					}
				}
				if (!probe)
				{
					who.completeQuest("25");
				}
				if (Game1.IsGreenRainingHere() && Game1.year == 1 && !this.isMarried())
				{
					if (!probe)
					{
						Game1.showRedMessage(".........");
					}
					return false;
				}
				if ((friendship != null && friendship.GiftsThisWeek < 2) || who.spouse == base.Name || this is Child || this.isBirthday() || who.ActiveObject.QualifiedItemId == "(O)StardropTea")
				{
					if (!probe)
					{
						if (friendship == null)
						{
							friendship = (who.friendshipData[base.Name] = new Friendship());
						}
						if (friendship.IsDivorced())
						{
							this.CurrentDialogue.Push(this.TryGetDialogue("RejectGift_Divorced") ?? new Dialogue(this, "Strings\\Characters:Divorced_gift"));
							Game1.drawDialogue(this);
							return true;
						}
						if (friendship.GiftsToday == 1 && who.ActiveObject.QualifiedItemId != "(O)StardropTea")
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3981", this.displayName)));
							return true;
						}
						this.receiveGift(who.ActiveObject, who, who.ActiveObject.QualifiedItemId != "(O)StardropTea");
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						base.faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
						if (this.datable.Value && who.spouse != null && who.spouse != base.Name && !who.hasCurrentOrPendingRoommate() && Utility.isMale(who.spouse) == Utility.isMale(base.Name) && Game1.random.NextDouble() < 0.3 - (double)((float)who.LuckLevel / 100f) - who.DailyLuck && !this.isBirthday() && friendship.IsDating())
						{
							NPC spouse = Game1.getCharacterFromName(who.spouse);
							CharacterData spouseData = spouse?.GetData();
							if (spouse != null && GameStateQuery.CheckConditions(spouseData?.SpouseGiftJealousy, null, who, activeObj))
							{
								who.changeFriendship(spouseData?.SpouseGiftJealousyFriendshipChange ?? (-30), spouse);
								spouse.CurrentDialogue.Clear();
								spouse.CurrentDialogue.Push(spouse.TryGetDialogue("SpouseGiftJealous", this.displayName, activeObj.DisplayName) ?? StardewValley.Dialogue.FromTranslation(spouse, "Strings\\StringsFromCSFiles:NPC.cs.3985", this.displayName));
							}
						}
					}
					return true;
				}
				if (!probe)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3987", this.displayName, 2)));
				}
				return true;
			}
			return false;
		}
		}
	}

	public string GetDispositionModifiedString(string path, params object[] substitutions)
	{
		List<string> disposition_tags = new List<string>();
		disposition_tags.Add(base.name.Value);
		if (Game1.player.isMarriedOrRoommates() && Game1.player.getSpouse() == this)
		{
			disposition_tags.Add("spouse");
		}
		CharacterData npcData = this.GetData();
		if (npcData != null)
		{
			disposition_tags.Add(npcData.Manner.ToString().ToLower());
			disposition_tags.Add(npcData.SocialAnxiety.ToString().ToLower());
			disposition_tags.Add(npcData.Optimism.ToString().ToLower());
			disposition_tags.Add(npcData.Age.ToString().ToLower());
		}
		foreach (string tag in disposition_tags)
		{
			string current_path = path + "_" + Utility.capitalizeFirstLetter(tag);
			string found_string = Game1.content.LoadString(current_path, substitutions);
			if (!(found_string == current_path))
			{
				return found_string;
			}
		}
		return Game1.content.LoadString(path, substitutions);
	}

	public void haltMe(Farmer who)
	{
		this.Halt();
	}

	public virtual bool checkAction(Farmer who, GameLocation l)
	{
		if (this.IsInvisible)
		{
			return false;
		}
		if (this.isSleeping.Value)
		{
			if (!base.isEmoting)
			{
				base.doEmote(24);
			}
			this.shake(250);
			return false;
		}
		if (!who.CanMove)
		{
			return false;
		}
		Game1.player.friendshipData.TryGetValue(base.Name, out var friendship);
		if (base.Name.Equals("Henchman") && l.Name.Equals("WitchSwamp"))
		{
			if (Game1.player.mailReceived.Add("Henchman1"))
			{
				this.CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman1"));
				Game1.drawDialogue(this);
				Game1.player.addQuest("27");
				if (!Game1.player.friendshipData.ContainsKey("Henchman"))
				{
					Game1.player.friendshipData.Add("Henchman", friendship = new Friendship());
				}
			}
			else
			{
				if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who))
				{
					return true;
				}
				if (base.controller == null)
				{
					this.CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman2"));
					Game1.drawDialogue(this);
				}
			}
			return true;
		}
		bool reactingToShorts = who.pantsItem.Value?.QualifiedItemId == "(P)15" && (base.Name == "Lewis" || base.Name == "Marnie");
		if (this.CanReceiveGifts() && friendship == null)
		{
			Game1.player.friendshipData.Add(base.Name, friendship = new Friendship(0));
			if (base.Name.Equals("Krobus"))
			{
				this.CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3990"));
				Game1.drawDialogue(this);
				return true;
			}
		}
		if (who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this)) && Game1.dialogueUp)
		{
			base.faceTowardFarmerForPeriod(6000, 3, faceAway: false, who);
			return true;
		}
		if (base.Name.Equals("Krobus") && who.hasQuest("28"))
		{
			this.CurrentDialogue.Push(new Dialogue(this, (l is Sewer) ? "Strings\\Characters:KrobusDarkTalisman" : "Strings\\Characters:KrobusDarkTalisman_elsewhere"));
			Game1.drawDialogue(this);
			who.removeQuest("28");
			who.mailReceived.Add("krobusUnseal");
			if (l is Sewer)
			{
				DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
				{
					scale = 4f,
					delayBeforeAnimationStart = 1,
					startSound = "debuffSpell",
					motion = new Vector2(-9f, 1f),
					rotationChange = (float)Math.PI / 64f,
					lightId = "Krobus_Unseal_1",
					lightRadius = 1f,
					lightcolor = new Color(150, 0, 50),
					layerDepth = 1f,
					alphaFade = 0.003f
				}, l, 200, waitUntilMenusGone: true);
				DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, flicker: false, flipped: false)
				{
					startSound = "debuffSpell",
					delayBeforeAnimationStart = 1,
					scale = 4f,
					motion = new Vector2(-9f, 1f),
					rotationChange = (float)Math.PI / 64f,
					lightId = "Krobus_Unseal_2",
					lightRadius = 1f,
					lightcolor = new Color(150, 0, 50),
					layerDepth = 1f,
					alphaFade = 0.003f
				}, l, 700, waitUntilMenusGone: true);
			}
			return true;
		}
		if (base.name.Value == "Jas" && base.currentLocation is Desert && who.mailReceived.Contains("Jas_IceCream_DF_" + Game1.year))
		{
			base.doEmote(32);
			return true;
		}
		if (base.Name == who.spouse && who.IsLocalPlayer && this.Sprite.CurrentAnimation == null)
		{
			this.faceDirection(-3);
			if (friendship != null && friendship.Points >= 3125 && who.mailReceived.Add("CF_Spouse"))
			{
				this.CurrentDialogue.Push(this.TryGetDialogue("SpouseStardrop") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4001"));
				Object stardrop = ItemRegistry.Create<Object>("(O)434");
				stardrop.CanBeSetDown = false;
				stardrop.CanBeGrabbed = false;
				Game1.player.addItemByMenuIfNecessary(stardrop);
				this.shouldSayMarriageDialogue.Value = false;
				this.currentMarriageDialogue.Clear();
				return true;
			}
			if (!this.hasTemporaryMessageAvailable() && this.currentMarriageDialogue.Count == 0 && this.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !this.isMoving() && who.ActiveObject == null)
			{
				if (base.faceTowardFarmerTimer <= 0)
				{
					base.facingDirectionBeforeSpeakingToPlayer.Value = this.FacingDirection;
				}
				base.faceGeneralDirection(who.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				if (this.FacingDirection == 3 || this.FacingDirection == 1)
				{
					CharacterData data = this.GetData();
					int spouseFrame = data?.KissSpriteIndex ?? 28;
					bool facingRight = data?.KissSpriteFacingRight ?? true;
					bool flip = facingRight != (this.FacingDirection == 1);
					if (who.getFriendshipHeartLevelForNPC(base.Name) > 9 && this.sleptInBed.Value)
					{
						int delay = (base.movementPause = (Game1.IsMultiplayer ? 1000 : 10));
						base.faceTowardFarmerForPeriod(3000, 3, faceAway: false, who);
						this.Sprite.ClearAnimation();
						this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(spouseFrame, delay, secondaryArm: false, flip, haltMe, behaviorAtEndOfFrame: true));
						if (!this.hasBeenKissedToday.Value)
						{
							who.changeFriendship(10, this);
							if (who.hasCurrentOrPendingRoommate())
							{
								Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									motion = new Vector2(0f, -0.5f),
									alphaFade = 0.01f
								});
							}
							else
							{
								Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
								{
									motion = new Vector2(0f, -0.5f),
									alphaFade = 0.01f
								});
							}
							l.playSound("dwop", null, null, SoundContext.NPC);
							who.exhausted.Value = false;
						}
						else if (Game1.random.NextDouble() < 0.1)
						{
							base.doEmote(20);
						}
						this.hasBeenKissedToday.Value = true;
						this.Sprite.UpdateSourceRect();
					}
					else
					{
						this.faceDirection(Game1.random.Choose(2, 0));
						base.doEmote(12);
					}
					int playerFaceDirection = 1;
					if ((facingRight && !flip) || (!facingRight && flip))
					{
						playerFaceDirection = 3;
					}
					who.PerformKiss(playerFaceDirection);
					return true;
				}
				if (base.faceTowardFarmerTimer <= 0 && Game1.random.NextDouble() < 0.1)
				{
					Game1.playSound("dwop");
					if (who.getFriendshipHeartLevelForNPC(base.Name) > 9)
					{
						base.doEmote((Game1.random.NextDouble() < 0.5) ? 32 : 20);
					}
					else if (who.getFriendshipHeartLevelForNPC(base.Name) > 7)
					{
						base.doEmote((Game1.random.NextDouble() < 0.5) ? 40 : 8);
					}
					else
					{
						base.doEmote((Game1.random.NextDouble() < 0.5) ? 28 : 12);
					}
				}
				else if (base.facingDirectionBeforeSpeakingToPlayer.Value >= 0 && Math.Abs(base.facingDirectionBeforeSpeakingToPlayer.Value - this.FacingDirection) == 2 && Game1.random.NextDouble() < 0.1)
				{
					this.jump();
					base.doEmote(16);
				}
				base.faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
			}
		}
		if (base.SimpleNonVillagerNPC)
		{
			if (base.name.Value == "Fizz")
			{
				int waivers = Game1.netWorldState.Value.PerfectionWaivers;
				if (Utility.percentGameComplete() + (float)waivers * 0.01f >= 1f)
				{
					base.doEmote(56);
					this.shakeTimer = 250;
				}
				else
				{
					this.CurrentDialogue.Clear();
					if (!Game1.player.mailReceived.Contains("FizzFirstDialogue"))
					{
						Game1.player.mailReceived.Add("FizzFirstDialogue");
						this.CurrentDialogue.Push(new Dialogue(this, "Strings\\1_6_Strings:Fizz_Intro_1"));
						Game1.drawDialogue(this);
					}
					else
					{
						this.CurrentDialogue.Push(new Dialogue(this, "Strings\\1_6_Strings:Fizz_Intro_2"));
						Game1.drawDialogue(this);
						Game1.afterDialogues = delegate
						{
							Game1.currentLocation.createQuestionDialogue("", new Response[2]
							{
								new Response("Yes", Game1.content.LoadString("Strings\\1_6_Strings:Fizz_Yes")).SetHotKey(Keys.Y),
								new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
							}, "Fizz");
						};
					}
				}
			}
			else
			{
				string path = "Strings\\SimpleNonVillagerDialogues:" + base.Name;
				string s = Game1.content.LoadString(path);
				if (s != path)
				{
					string[] split = s.Split("||");
					if (base.nonVillagerNPCTimesTalked != -1 && base.nonVillagerNPCTimesTalked < split.Length)
					{
						Game1.drawObjectDialogue(split[base.nonVillagerNPCTimesTalked]);
						base.nonVillagerNPCTimesTalked++;
						if (base.nonVillagerNPCTimesTalked >= split.Length)
						{
							base.nonVillagerNPCTimesTalked = -1;
						}
					}
				}
			}
			return true;
		}
		bool newCurrentDialogue = false;
		if (friendship != null)
		{
			if (this.getSpouse() == Game1.player && this.shouldSayMarriageDialogue.Value && this.currentMarriageDialogue.Count > 0 && this.currentMarriageDialogue.Count > 0)
			{
				while (this.currentMarriageDialogue.Count > 0)
				{
					MarriageDialogueReference dialogue_reference = this.currentMarriageDialogue[this.currentMarriageDialogue.Count - 1];
					if (dialogue_reference == this.marriageDefaultDialogue.Value)
					{
						this.marriageDefaultDialogue.Value = null;
					}
					this.currentMarriageDialogue.RemoveAt(this.currentMarriageDialogue.Count - 1);
					this.CurrentDialogue.Push(dialogue_reference.GetDialogue(this));
				}
				newCurrentDialogue = true;
			}
			if (!newCurrentDialogue)
			{
				newCurrentDialogue = this.checkForNewCurrentDialogue(friendship.Points / 250);
				if (!newCurrentDialogue)
				{
					newCurrentDialogue = this.checkForNewCurrentDialogue(friendship.Points / 250, noPreface: true);
				}
			}
		}
		if (who.IsLocalPlayer && friendship != null && (this.endOfRouteMessage.Value != null || newCurrentDialogue || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
		{
			if (!newCurrentDialogue && this.setTemporaryMessages(who))
			{
				who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this));
				return false;
			}
			Texture2D texture = this.Sprite.Texture;
			if (texture != null && texture.Bounds.Height > 32 && (this.CurrentDialogue.Count <= 0 || !this.CurrentDialogue.Peek().dontFaceFarmer))
			{
				base.faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
			}
			if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who))
			{
				who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this));
				base.faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
				return true;
			}
			this.grantConversationFriendship(who);
			Game1.drawDialogue(this);
			return true;
		}
		if (this.canTalk() && who.hasClubCard && base.Name.Equals("Bouncer") && who.IsLocalPlayer)
		{
			Response[] responses = new Response[2]
			{
				new Response("Yes.", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4018")),
				new Response("That's", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4020"))
			};
			l.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4021"), responses, "ClubCard");
		}
		else if (this.canTalk() && this.CurrentDialogue.Count > 0)
		{
			if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who, probe: true))
			{
				if (who.IsLocalPlayer)
				{
					this.tryToReceiveActiveObject(who);
				}
				else
				{
					base.faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
				}
				return true;
			}
			if (this.CurrentDialogue.Count >= 1 || this.endOfRouteMessage.Value != null || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this)))
			{
				if (this.setTemporaryMessages(who))
				{
					who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this));
					return false;
				}
				Texture2D texture2 = this.Sprite.Texture;
				if (texture2 != null && texture2.Bounds.Height > 32 && !this.CurrentDialogue.Peek().dontFaceFarmer)
				{
					base.faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
				}
				if (who.IsLocalPlayer)
				{
					this.grantConversationFriendship(who);
					if (!reactingToShorts)
					{
						Game1.drawDialogue(this);
						return true;
					}
				}
			}
			else if (!this.doingEndOfRouteAnimation.Value)
			{
				try
				{
					if (friendship != null)
					{
						base.faceTowardFarmerForPeriod(friendship.Points / 125 * 1000 + 1000, 4, faceAway: false, who);
					}
				}
				catch (Exception)
				{
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					base.doEmote(8);
				}
			}
		}
		else if (this.canTalk() && !Game1.game1.wasAskedLeoMemory && Game1.CurrentEvent == null && base.name.Value == "Leo" && base.currentLocation != null && (base.currentLocation.NameOrUniqueName == "LeoTreeHouse" || base.currentLocation.NameOrUniqueName == "Mountain") && Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved") && this.GetUnseenLeoEvent().HasValue && this.CanRevisitLeoMemory(this.GetUnseenLeoEvent()))
		{
			Game1.DrawDialogue(this, "Strings\\Characters:Leo_Memory");
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(AskLeoMemoryPrompt));
		}
		else
		{
			if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who))
			{
				base.faceTowardFarmerForPeriod(3000, 4, faceAway: false, who);
				return true;
			}
			string text = base.Name;
			if (!(text == "Krobus"))
			{
				if (text == "Dwarf" && who.canUnderstandDwarves && l is Mine)
				{
					Utility.TryOpenShopMenu("Dwarf", base.Name);
					return true;
				}
			}
			else if (l is Sewer)
			{
				Utility.TryOpenShopMenu("ShadowShop", "Krobus");
				return true;
			}
		}
		if (reactingToShorts)
		{
			if (base.yJumpVelocity != 0f || this.Sprite.CurrentAnimation != null)
			{
				return true;
			}
			string text = base.Name;
			if (!(text == "Lewis"))
			{
				if (text == "Marnie")
				{
					base.faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
					this.Sprite.ClearAnimation();
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 150, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, secondaryArm: false, flip: false, delegate
					{
						l.playSound("dustMeep");
					}));
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
					this.Sprite.loop = false;
				}
			}
			else
			{
				base.faceTowardFarmerForPeriod(1000, 3, faceAway: false, who);
				this.jump();
				this.Sprite.ClearAnimation();
				this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(26, 1000, secondaryArm: false, flip: false, delegate
				{
					base.doEmote(12);
				}, behaviorAtEndOfFrame: true));
				this.Sprite.loop = false;
				this.shakeTimer = 1000;
				l.playSound("batScreech");
			}
			return true;
		}
		if (this.setTemporaryMessages(who))
		{
			return false;
		}
		if ((this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value) && this.endOfRouteMessage.Value != null)
		{
			Game1.drawDialogue(this);
			return true;
		}
		return false;
	}

	public void grantConversationFriendship(Farmer who, int amount = 20)
	{
		if (who.hasPlayerTalkedToNPC(base.Name) || !who.friendshipData.TryGetValue(base.Name, out var friendship))
		{
			return;
		}
		friendship.TalkedToToday = true;
		who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this));
		if (!this.isDivorcedFrom(who))
		{
			if (who.hasBuff("statue_of_blessings_4"))
			{
				amount = 60;
			}
			who.changeFriendship(amount, this);
		}
	}

	public virtual void AskLeoMemoryPrompt()
	{
		GameLocation l = base.currentLocation;
		Response[] responses = new Response[2]
		{
			new Response("Yes", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_Yes")),
			new Response("No", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_No"))
		};
		string question = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Characters:Leo_Memory_" + this.GetUnseenLeoEvent().Value.Value);
		if (question == null)
		{
			question = "";
		}
		l.createQuestionDialogue(question, responses, OnLeoMemoryResponse, this);
	}

	public bool CanRevisitLeoMemory(KeyValuePair<string, string>? event_data)
	{
		if (!event_data.HasValue)
		{
			return false;
		}
		string location_name = event_data.Value.Key;
		string event_id = event_data.Value.Value;
		Dictionary<string, string> location_events;
		try
		{
			location_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + location_name);
		}
		catch
		{
			return false;
		}
		if (location_events == null)
		{
			return false;
		}
		foreach (string key in location_events.Keys)
		{
			if (Event.SplitPreconditions(key)[0] == event_id)
			{
				GameLocation locationFromName = Game1.getLocationFromName(location_name);
				string event_key = key;
				event_key = event_key.Replace("/e 1039573", "");
				event_key = event_key.Replace("/Hl leoMoved", "");
				string condition = locationFromName?.checkEventPrecondition(event_key);
				if (locationFromName != null && string.IsNullOrEmpty(condition) && condition != "-1")
				{
					return true;
				}
			}
		}
		return false;
	}

	public KeyValuePair<string, string>? GetUnseenLeoEvent()
	{
		if (!Game1.player.eventsSeen.Contains("6497423"))
		{
			return new KeyValuePair<string, string>("IslandWest", "6497423");
		}
		if (!Game1.player.eventsSeen.Contains("6497421"))
		{
			return new KeyValuePair<string, string>("IslandNorth", "6497421");
		}
		if (!Game1.player.eventsSeen.Contains("6497428"))
		{
			return new KeyValuePair<string, string>("IslandSouth", "6497428");
		}
		return null;
	}

	public void OnLeoMemoryResponse(Farmer who, string whichAnswer)
	{
		if (whichAnswer.EqualsIgnoreCase("yes"))
		{
			KeyValuePair<string, string>? event_data = this.GetUnseenLeoEvent();
			if (!event_data.HasValue)
			{
				return;
			}
			string location_name = event_data.Value.Key;
			string event_id = event_data.Value.Value;
			string eventAssetName = "Data\\Events\\" + location_name;
			Dictionary<string, string> location_events;
			try
			{
				location_events = Game1.content.Load<Dictionary<string, string>>(eventAssetName);
			}
			catch
			{
				return;
			}
			if (location_events == null)
			{
				return;
			}
			Point oldTile = Game1.player.TilePoint;
			string oldLocation = Game1.player.currentLocation.NameOrUniqueName;
			int oldDirection = Game1.player.FacingDirection;
			{
				foreach (string key in location_events.Keys)
				{
					if (Event.SplitPreconditions(key)[0] == event_id)
					{
						LocationRequest location_request = Game1.getLocationRequest(location_name);
						Game1.warpingForForcedRemoteEvent = true;
						location_request.OnWarp += delegate
						{
							Event obj2 = new Event(location_events[key], eventAssetName, "event_id");
							obj2.isMemory = true;
							obj2.setExitLocation(oldLocation, oldTile.X, oldTile.Y);
							Game1.player.orientationBeforeEvent = oldDirection;
							location_request.Location.currentEvent = obj2;
							location_request.Location.startEvent(obj2);
							Game1.warpingForForcedRemoteEvent = false;
						};
						int x = 8;
						int y = 8;
						Utility.getDefaultWarpLocation(location_request.Name, ref x, ref y);
						Game1.warpFarmer(location_request, x, y, Game1.player.FacingDirection);
					}
				}
				return;
			}
		}
		Game1.game1.wasAskedLeoMemory = true;
	}

	public bool isDivorcedFrom(Farmer who)
	{
		return NPC.IsDivorcedFrom(who, base.Name);
	}

	public static bool IsDivorcedFrom(Farmer player, string npcName)
	{
		if (player != null && player.friendshipData.TryGetValue(npcName, out var friendship))
		{
			return friendship.IsDivorced();
		}
		return false;
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (base.movementPause <= 0)
		{
			base.faceTowardFarmerTimer = 0;
			base.MovePosition(time, viewport, currentLocation);
		}
	}

	public GameLocation getHome()
	{
		if (this.isMarried() && this.getSpouse() != null)
		{
			return Utility.getHomeOfFarmer(this.getSpouse());
		}
		return Game1.RequireLocation(this.defaultMap.Value);
	}

	public override bool canPassThroughActionTiles()
	{
		return true;
	}

	public virtual void behaviorOnFarmerPushing()
	{
	}

	public virtual void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
	{
		if (this.Sprite != null && this.Sprite.CurrentAnimation == null && this.Sprite.SourceRect.Height > 32 && !base.SimpleNonVillagerNPC)
		{
			this.Sprite.SpriteWidth = 16;
			this.Sprite.SpriteHeight = 16;
			this.Sprite.currentFrame = 0;
		}
	}

	public virtual void behaviorOnLocalFarmerLocationEntry(GameLocation location)
	{
		this.shouldPlayRobinHammerAnimation.CancelInterpolation();
		this.shouldPlaySpousePatioAnimation.CancelInterpolation();
		this.shouldWearIslandAttire.CancelInterpolation();
		this.isSleeping.CancelInterpolation();
		this.doingEndOfRouteAnimation.CancelInterpolation();
		if (this.doingEndOfRouteAnimation.Value)
		{
			this._skipRouteEndIntro = true;
		}
		else
		{
			this._skipRouteEndIntro = false;
		}
		this.endOfRouteBehaviorName.CancelInterpolation();
		if (this.isSleeping.Value)
		{
			base.position.Field.CancelInterpolation();
		}
	}

	public override void updateMovement(GameLocation location, GameTime time)
	{
		this.lastPosition = base.Position;
		if (this.DirectionsToNewLocation != null && !Game1.newDay)
		{
			Point standingPixel = base.StandingPixel;
			if (standingPixel.X < -64 || standingPixel.X > location.map.DisplayWidth + 64 || standingPixel.Y < -64 || standingPixel.Y > location.map.DisplayHeight + 64)
			{
				this.IsWalkingInSquare = false;
				Game1.warpCharacter(this, this.DefaultMap, this.DefaultPosition);
				location.characters.Remove(this);
			}
			else if (this.IsWalkingInSquare)
			{
				this.returnToEndPoint();
				this.MovePosition(time, Game1.viewport, location);
			}
		}
		else if (this.IsWalkingInSquare)
		{
			this.randomSquareMovement(time);
			this.MovePosition(time, Game1.viewport, location);
		}
	}

	public void facePlayer(Farmer who)
	{
		if (base.facingDirectionBeforeSpeakingToPlayer.Value == -1)
		{
			base.facingDirectionBeforeSpeakingToPlayer.Value = base.getFacingDirection();
		}
		this.faceDirection((who.FacingDirection + 2) % 4);
	}

	public void doneFacingPlayer(Farmer who)
	{
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (this.AllowDynamicAppearance && base.currentLocation != null && base.currentLocation.NameOrUniqueName != this.LastLocationNameForAppearance)
		{
			this.ChooseAppearance();
		}
		if (Game1.IsMasterGame && this.currentScheduleDelay > 0f)
		{
			this.currentScheduleDelay -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.currentScheduleDelay <= 0f)
			{
				this.currentScheduleDelay = -1f;
				this.checkSchedule(Game1.timeOfDay);
				this.currentScheduleDelay = 0f;
			}
		}
		this.removeHenchmanEvent.Poll();
		if (Game1.IsMasterGame && this.shouldWearIslandAttire.Value && (base.currentLocation == null || base.currentLocation.InValleyContext()))
		{
			this.shouldWearIslandAttire.Value = false;
		}
		if (this._startedEndOfRouteBehavior == null && this._finishingEndOfRouteBehavior == null && this.loadedEndOfRouteBehavior != this.endOfRouteBehaviorName.Value)
		{
			this.loadEndOfRouteBehavior(this.endOfRouteBehaviorName.Value);
		}
		if (this.doingEndOfRouteAnimation.Value != this.currentlyDoingEndOfRouteAnimation)
		{
			if (!this.currentlyDoingEndOfRouteAnimation)
			{
				if (string.Equals(this.loadedEndOfRouteBehavior, this.endOfRouteBehaviorName.Value, StringComparison.Ordinal))
				{
					this.reallyDoAnimationAtEndOfScheduleRoute();
				}
			}
			else
			{
				this.finishEndOfRouteAnimation();
			}
			this.currentlyDoingEndOfRouteAnimation = this.doingEndOfRouteAnimation.Value;
		}
		if (this.shouldWearIslandAttire.Value != this.isWearingIslandAttire)
		{
			if (!this.isWearingIslandAttire)
			{
				this.wearIslandAttire();
			}
			else
			{
				this.wearNormalClothes();
			}
		}
		if (this.isSleeping.Value != this.isPlayingSleepingAnimation)
		{
			if (!this.isPlayingSleepingAnimation)
			{
				this.playSleepingAnimation();
			}
			else
			{
				this.Sprite.StopAnimation();
				this.isPlayingSleepingAnimation = false;
			}
		}
		if (this.shouldPlayRobinHammerAnimation.Value != this.isPlayingRobinHammerAnimation)
		{
			if (!this.isPlayingRobinHammerAnimation)
			{
				this.doPlayRobinHammerAnimation();
				this.isPlayingRobinHammerAnimation = true;
			}
			else
			{
				this.Sprite.StopAnimation();
				this.isPlayingRobinHammerAnimation = false;
			}
		}
		if (this.shouldPlaySpousePatioAnimation.Value != this.isPlayingSpousePatioAnimation)
		{
			if (!this.isPlayingSpousePatioAnimation)
			{
				this.doPlaySpousePatioAnimation();
				this.isPlayingSpousePatioAnimation = true;
			}
			else
			{
				this.Sprite.StopAnimation();
				this.isPlayingSpousePatioAnimation = false;
			}
		}
		if (this.returningToEndPoint)
		{
			this.returnToEndPoint();
			this.MovePosition(time, Game1.viewport, location);
		}
		else if (this.temporaryController != null)
		{
			if (this.temporaryController.update(time))
			{
				bool nPCSchedule = this.temporaryController.NPCSchedule;
				this.temporaryController = null;
				if (nPCSchedule)
				{
					this.currentScheduleDelay = -1f;
					this.checkSchedule(Game1.timeOfDay);
					this.currentScheduleDelay = 0f;
				}
			}
			base.updateEmote(time);
		}
		else
		{
			base.update(time, location);
		}
		if (this.textAboveHeadTimer > 0)
		{
			if (this.textAboveHeadPreTimer > 0)
			{
				this.textAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				this.textAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.textAboveHeadTimer > 500)
				{
					this.textAboveHeadAlpha = Math.Min(1f, this.textAboveHeadAlpha + 0.1f);
				}
				else
				{
					this.textAboveHeadAlpha = Math.Max(0f, this.textAboveHeadAlpha - 0.04f);
				}
			}
		}
		if (this.isWalkingInSquare && !this.returningToEndPoint)
		{
			this.randomSquareMovement(time);
		}
		if (this.Sprite?.CurrentAnimation != null && !Game1.eventUp && Game1.IsMasterGame && this.Sprite.animateOnce(time))
		{
			this.Sprite.CurrentAnimation = null;
		}
		if (base.movementPause > 0 && (!Game1.dialogueUp || base.controller != null))
		{
			base.freezeMotion = true;
			base.movementPause -= time.ElapsedGameTime.Milliseconds;
			if (base.movementPause <= 0)
			{
				base.freezeMotion = false;
			}
		}
		if (this.shakeTimer > 0)
		{
			this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.lastPosition.Equals(base.Position))
		{
			this.timerSinceLastMovement += time.ElapsedGameTime.Milliseconds;
		}
		else
		{
			this.timerSinceLastMovement = 0f;
		}
		if (base.swimming.Value)
		{
			this.yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
			float oldSwimTimer = this.swimTimer;
			this.swimTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.timerSinceLastMovement == 0f)
			{
				if (oldSwimTimer > 400f && this.swimTimer <= 400f && location.Equals(Game1.currentLocation))
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(base.xVelocity) + Math.Abs(base.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					location.playSound("slosh", null, null, SoundContext.NPC);
				}
				if (this.swimTimer < 0f)
				{
					this.swimTimer = 800f;
					if (location.Equals(Game1.currentLocation))
					{
						location.playSound("slosh", null, null, SoundContext.NPC);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(base.xVelocity) + Math.Abs(base.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, base.StandingPixel.Y - 32), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
					}
				}
			}
			else if (this.swimTimer < 0f)
			{
				this.swimTimer = 100f;
			}
		}
		if (Game1.IsMasterGame)
		{
			this.isMovingOnPathFindPath.Value = base.controller != null && this.temporaryController != null;
		}
	}

	public virtual void wearIslandAttire()
	{
		this.isWearingIslandAttire = true;
		this.ChooseAppearance();
	}

	public virtual void wearNormalClothes()
	{
		this.isWearingIslandAttire = false;
		this.ChooseAppearance();
	}

	/// <summary>Runs NPC update logic on ten in-game minute intervals (e.g. greeting players or other NPCs)</summary>
	/// <param name="timeOfDay">The new in-game time.</param>
	/// <param name="location">The location where the update is occurring.</param>
	public virtual void performTenMinuteUpdate(int timeOfDay, GameLocation location)
	{
		if (Game1.eventUp || location == null)
		{
			return;
		}
		if (Game1.random.NextDouble() < 0.1 && this.Dialogue != null && this.Dialogue.TryGetValue(location.Name + "_Ambient", out var rawText))
		{
			CharacterData data = this.GetData();
			if (data == null || data.CanGreetNearbyCharacters)
			{
				string[] split = rawText.Split('/');
				int extraTime = Game1.random.Next(4) * 1000;
				this.showTextAboveHead(Game1.random.Choose(split), null, 2, 3000, extraTime);
				return;
			}
		}
		if (!this.isMoving() || !location.IsOutdoors || timeOfDay >= 1800 || !(Game1.random.NextDouble() < 0.3 + ((this.SocialAnxiety == 0) ? 0.25 : ((this.SocialAnxiety != 1) ? 0.0 : ((this.Manners == 2) ? (-1.0) : (-0.2))))) || (this.Age == 1 && (this.Manners != 1 || this.SocialAnxiety != 0)) || this.isMarried())
		{
			return;
		}
		CharacterData data2 = this.GetData();
		if (data2 == null || !data2.CanGreetNearbyCharacters)
		{
			return;
		}
		Character c = Utility.isThereAFarmerOrCharacterWithinDistance(base.Tile, 4, location);
		if (c == null || c.Name == base.Name || c is Horse)
		{
			return;
		}
		NPC obj = c as NPC;
		if (obj != null && obj.GetData()?.CanGreetNearbyCharacters == false)
		{
			return;
		}
		NPC obj2 = c as NPC;
		if (obj2 == null || !obj2.SimpleNonVillagerNPC)
		{
			Dictionary<string, string> friendsAndFamily = data2.FriendsAndFamily;
			if ((friendsAndFamily == null || !friendsAndFamily.ContainsKey(c.Name)) && this.isFacingToward(c.Tile))
			{
				this.sayHiTo(c);
			}
		}
	}

	public void sayHiTo(Character c)
	{
		if (this.getHi(c.displayName) != null)
		{
			this.showTextAboveHead(this.getHi(c.displayName));
			if (c is NPC npc && Game1.random.NextDouble() < 0.66 && npc.getHi(this.displayName) != null)
			{
				npc.showTextAboveHead(npc.getHi(this.displayName), null, 2, 3000, 1000 + Game1.random.Next(500));
			}
		}
	}

	public string getHi(string nameToGreet)
	{
		if (this.Age == 2)
		{
			if (this.SocialAnxiety != 1)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4059");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4058");
		}
		switch (this.SocialAnxiety)
		{
		case 1:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("4060", "4061"));
		case 0:
			if (!(Game1.random.NextDouble() < 0.33))
			{
				if (!Game1.random.NextBool())
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4068", nameToGreet);
				}
				return ((Game1.timeOfDay < 1200) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4063") : ((Game1.timeOfDay < 1700) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4064") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4065"))) + ", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4066", nameToGreet);
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4062");
		default:
			if (!(Game1.random.NextDouble() < 0.33))
			{
				if (!Game1.random.NextBool())
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4072");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4071", nameToGreet);
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4060");
		}
	}

	public bool isFacingToward(Vector2 tileLocation)
	{
		return this.FacingDirection switch
		{
			0 => (float)base.TilePoint.Y > tileLocation.Y, 
			1 => (float)base.TilePoint.X < tileLocation.X, 
			2 => (float)base.TilePoint.Y < tileLocation.Y, 
			3 => (float)base.TilePoint.X > tileLocation.X, 
			_ => false, 
		};
	}

	public virtual void arriveAt(GameLocation l)
	{
		if (!Game1.eventUp && Game1.random.NextBool() && this.Dialogue != null && this.Dialogue.TryGetValue(l.name.Value + "_Entry", out var rawText))
		{
			this.showTextAboveHead(Game1.random.Choose(rawText.Split('/')));
		}
	}

	public override void Halt()
	{
		base.Halt();
		this.shouldPlaySpousePatioAnimation.Value = false;
		this.isPlayingSleepingAnimation = false;
		base.isCharging = false;
		base.speed = 2;
		this.addedSpeed = 0f;
		if (this.isSleeping.Value)
		{
			this.playSleepingAnimation();
			this.Sprite.UpdateSourceRect();
		}
	}

	public void addExtraDialogue(Dialogue dialogue)
	{
		if (this.updatedDialogueYet)
		{
			if (dialogue != null)
			{
				this.CurrentDialogue.Push(dialogue);
			}
		}
		else
		{
			this.extraDialogueMessageToAddThisMorning = dialogue;
		}
	}

	public void PerformDivorce()
	{
		this.reloadDefaultLocation();
		Game1.warpCharacter(this, this.defaultMap.Value, this.DefaultPosition / 64f);
	}

	public Dialogue tryToGetMarriageSpecificDialogue(string dialogueKey)
	{
		Dictionary<string, string> marriageDialogues = null;
		string assetName = null;
		bool skip_married_dialogue = false;
		if (this.isRoommate())
		{
			try
			{
				assetName = "Characters\\Dialogue\\MarriageDialogue" + this.GetDialogueSheetName() + "Roommate";
				Dictionary<string, string> rawData = Game1.content.Load<Dictionary<string, string>>(assetName);
				if (rawData != null)
				{
					skip_married_dialogue = true;
					marriageDialogues = rawData;
					if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out var rawText))
					{
						return new Dialogue(this, assetName + ":" + dialogueKey, rawText);
					}
				}
			}
			catch (Exception)
			{
				assetName = null;
			}
		}
		if (!skip_married_dialogue)
		{
			try
			{
				assetName = "Characters\\Dialogue\\MarriageDialogue" + this.GetDialogueSheetName();
				marriageDialogues = Game1.content.Load<Dictionary<string, string>>(assetName);
			}
			catch (Exception)
			{
				assetName = null;
			}
		}
		if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out var rawText2))
		{
			return new Dialogue(this, assetName + ":" + dialogueKey, rawText2);
		}
		assetName = "Characters\\Dialogue\\MarriageDialogue";
		marriageDialogues = Game1.content.Load<Dictionary<string, string>>(assetName);
		if (this.isRoommate())
		{
			string key = dialogueKey + "Roommate";
			if (marriageDialogues != null && marriageDialogues.TryGetValue(key, out var rawText3))
			{
				return new Dialogue(this, assetName + ":" + dialogueKey, rawText3);
			}
		}
		if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out var rawText4))
		{
			return new Dialogue(this, assetName + ":" + dialogueKey, rawText4);
		}
		return null;
	}

	public void resetCurrentDialogue()
	{
		this.CurrentDialogue = null;
		this.shouldSayMarriageDialogue.Value = false;
		this.currentMarriageDialogue.Clear();
	}

	private Stack<Dialogue> loadCurrentDialogue()
	{
		this.updatedDialogueYet = true;
		Stack<Dialogue> currentDialogue = new Stack<Dialogue>();
		try
		{
			Friendship friends;
			int heartLevel = (Game1.player.friendshipData.TryGetValue(base.Name, out friends) ? (friends.Points / 250) : 0);
			Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed * 77, 2f + this.defaultPosition.X * 77f, this.defaultPosition.Y * 777f);
			if (base.currentLocation != null && base.currentLocation.IsGreenRainingHere())
			{
				Dialogue dialogue = null;
				if (Game1.year >= 2)
				{
					dialogue = this.TryGetDialogue("GreenRain_2");
				}
				if (dialogue == null)
				{
					dialogue = this.TryGetDialogue("GreenRain");
				}
				if (dialogue != null)
				{
					currentDialogue.Clear();
					currentDialogue.Push(dialogue);
					return currentDialogue;
				}
			}
			if (r.NextDouble() < 0.025 && heartLevel >= 1)
			{
				CharacterData npcData = this.GetData();
				if (npcData?.FriendsAndFamily != null && Utility.TryGetRandom(npcData.FriendsAndFamily, out var relativeName, out var relativeTitle))
				{
					NPC relative = Game1.getCharacterFromName(relativeName);
					string relativeDisplayName = relative?.displayName ?? NPC.GetDisplayName(relativeName);
					CharacterData relativeData;
					bool relativeIsMale = ((relative != null) ? (relative.gender.Value == Gender.Male) : (NPC.TryGetData(relativeName, out relativeData) && relativeData.Gender == Gender.Male));
					relativeTitle = TokenParser.ParseText(relativeTitle);
					if (string.IsNullOrWhiteSpace(relativeTitle))
					{
						relativeTitle = null;
					}
					IDictionary<string, string> npcGiftTastes = Game1.NPCGiftTastes;
					if (npcGiftTastes.TryGetValue(relativeName, out var rawGiftTasteData))
					{
						string[] rawGiftTasteFields = rawGiftTasteData.Split('/');
						string item = null;
						string itemName = null;
						string nameAndTitle = ((relativeTitle == null || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja) ? relativeDisplayName : (relativeIsMale ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", relativeTitle) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", relativeTitle)));
						string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);
						if (r.NextBool())
						{
							int tries = 0;
							string[] lovedItems = ArgUtility.SplitBySpace(ArgUtility.Get(rawGiftTasteFields, 1));
							while ((item == null || item.StartsWith("-")) && tries < 30)
							{
								item = r.Choose(lovedItems);
								tries++;
							}
							if (base.Name == "Penny" && relativeName == "Pam")
							{
								while (true)
								{
									switch (item)
									{
									case "303":
									case "346":
									case "348":
									case "459":
										goto IL_027b;
									}
									break;
									IL_027b:
									item = r.Choose(lovedItems);
								}
							}
							if (item != null)
							{
								ParsedItemData itemData = ItemRegistry.GetData(item);
								if (itemData != null)
								{
									itemName = itemData.DisplayName;
									message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4084", itemName);
									if (this.Age == 2)
									{
										message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4086", nameAndTitle, itemName) + (relativeIsMale ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4088") : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4089"));
									}
									else
									{
										switch (r.Next(5))
										{
										case 0:
											message = Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4091", nameAndTitle, itemName);
											break;
										case 1:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4094", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4097", nameAndTitle, itemName));
											break;
										case 2:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4103", nameAndTitle, itemName));
											break;
										case 3:
											message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4106", nameAndTitle, itemName);
											break;
										}
										if (r.NextDouble() < 0.65)
										{
											switch (r.Next(5))
											{
											case 0:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4109") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4111"));
												break;
											case 1:
												message += ((!relativeIsMale) ? (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4115") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4116")) : (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4113") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4114")));
												break;
											case 2:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4118") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4120"));
												break;
											case 3:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4125");
												break;
											case 4:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128"));
												break;
											}
											if (relativeName.Equals("Abigail") && r.NextBool())
											{
												message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128", relativeDisplayName, itemName);
											}
										}
									}
								}
							}
						}
						else
						{
							string[] hatedItems = ArgUtility.SplitBySpace(ArgUtility.Get(rawGiftTasteFields, 7));
							if (hatedItems.Count() > 0)
							{
								int tries2 = 0;
								while ((item == null || item.StartsWith("-")) && tries2 < 30)
								{
									item = r.Choose(hatedItems);
									tries2++;
								}
							}
							if (item == null)
							{
								int tries3 = 0;
								while ((item == null || item.StartsWith("-")) && tries3 < 30)
								{
									item = r.Choose(ArgUtility.SplitBySpace(npcGiftTastes["Universal_Hate"]));
									tries3++;
								}
							}
							if (item != null)
							{
								ParsedItemData itemData2 = ItemRegistry.GetData(item);
								if (itemData2 != null)
								{
									itemName = itemData2.DisplayName;
									message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", itemName, Lexicon.getRandomNegativeFoodAdjective()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", itemName, Lexicon.getRandomNegativeFoodAdjective()));
									if (this.Age == 2)
									{
										message = (relativeIsMale ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4141", relativeDisplayName, itemName) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4144", relativeDisplayName, itemName));
									}
									else
									{
										switch (r.Next(4))
										{
										case 0:
											message = (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4146") : "") + Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4147", nameAndTitle, itemName);
											break;
										case 1:
											message = ((!relativeIsMale) ? (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4153", nameAndTitle, itemName) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4154", nameAndTitle, itemName)) : (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4149", nameAndTitle, itemName) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4152", nameAndTitle, itemName)));
											break;
										case 2:
											message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4161", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4164", nameAndTitle, itemName));
											break;
										}
										if (r.NextDouble() < 0.65)
										{
											switch (r.Next(5))
											{
											case 0:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4170");
												break;
											case 1:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4171");
												break;
											case 2:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4172") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4174"));
												break;
											case 3:
												message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4176") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4178"));
												break;
											case 4:
												message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4180");
												break;
											}
											if (base.Name.Equals("Lewis") && r.NextBool())
											{
												message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4182", relativeDisplayName, itemName);
											}
										}
									}
								}
							}
						}
						if (itemName != null)
						{
							if (Game1.getCharacterFromName(relativeName) != null)
							{
								message = message + "%revealtaste:" + relativeName + ":" + item;
							}
							currentDialogue.Clear();
							if (message.Length > 0)
							{
								try
								{
									message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
								}
								catch (Exception)
								{
								}
							}
							currentDialogue.Push(new Dialogue(this, null, message));
							return currentDialogue;
						}
					}
				}
			}
			if (this.Dialogue != null && this.Dialogue.Count != 0)
			{
				currentDialogue.Clear();
				if (Game1.player.spouse != null && Game1.player.spouse == base.Name)
				{
					if (Game1.player.isEngaged())
					{
						Dictionary<string, string> engagementDialogue = Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue");
						if (Game1.player.hasCurrentOrPendingRoommate() && engagementDialogue.ContainsKey(base.Name + "Roommate0"))
						{
							currentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + "Roommate" + r.Next(2)));
						}
						else if (engagementDialogue.ContainsKey(base.Name + "0"))
						{
							currentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + r.Next(2)));
						}
					}
					else if (!Game1.newDay && this.marriageDefaultDialogue.Value != null && !this.shouldSayMarriageDialogue.Value)
					{
						currentDialogue.Push(this.marriageDefaultDialogue.Value.GetDialogue(this));
						this.marriageDefaultDialogue.Value = null;
					}
				}
				else
				{
					if (Game1.player.friendshipData.TryGetValue(base.Name, out var friendship) && friendship.IsDivorced())
					{
						Dialogue dialogue2 = StardewValley.Dialogue.TryGetDialogue(this, "Characters\\Dialogue\\" + this.GetDialogueSheetName() + ":divorced");
						if (dialogue2 != null)
						{
							currentDialogue.Push(dialogue2);
							return currentDialogue;
						}
					}
					if (Game1.isRaining && r.NextBool() && (base.currentLocation == null || base.currentLocation.InValleyContext()) && (!base.Name.Equals("Krobus") || !(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")) && (!base.Name.Equals("Penny") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && (!base.Name.Equals("Emily") || !Game1.IsFall || Game1.dayOfMonth != 15))
					{
						Dialogue dialogue3 = StardewValley.Dialogue.TryGetDialogue(this, "Characters\\Dialogue\\rainy:" + this.GetDialogueSheetName());
						if (dialogue3 != null)
						{
							currentDialogue.Push(dialogue3);
							return currentDialogue;
						}
					}
					Dialogue d = this.tryToRetrieveDialogue(Game1.currentSeason + "_", heartLevel);
					if (d == null)
					{
						d = this.tryToRetrieveDialogue("", heartLevel);
					}
					if (d != null)
					{
						currentDialogue.Push(d);
					}
				}
			}
			else if (base.Name.Equals("Bouncer"))
			{
				currentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4192"));
			}
			if (this.extraDialogueMessageToAddThisMorning != null)
			{
				currentDialogue.Push(this.extraDialogueMessageToAddThisMorning);
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error("NPC '" + base.Name + "' failed loading their current dialogue.", exception);
		}
		return currentDialogue;
	}

	public bool checkForNewCurrentDialogue(int heartLevel, bool noPreface = false)
	{
		if (Game1.IsGreenRainingHere())
		{
			return false;
		}
		foreach (string eventMessageKey in Game1.player.activeDialogueEvents.Keys)
		{
			if (eventMessageKey == "")
			{
				continue;
			}
			Dialogue dialogue = this.TryGetDialogue(eventMessageKey);
			if (dialogue == null)
			{
				continue;
			}
			string mailKey = base.Name + "_" + eventMessageKey;
			if (dialogue != null && !Game1.player.mailReceived.Contains(mailKey))
			{
				this.CurrentDialogue.Clear();
				this.CurrentDialogue.Push(dialogue);
				if (!eventMessageKey.Contains("dumped"))
				{
					Game1.player.mailReceived.Add(mailKey);
				}
				return true;
			}
		}
		string preface = ((Game1.season != Season.Spring && !noPreface) ? Game1.currentSeason : "");
		Dialogue dialogue2 = this.TryGetDialogue(preface + Game1.currentLocation.name.Value + "_" + base.TilePoint.X + "_" + base.TilePoint.Y) ?? this.TryGetDialogue(preface + Game1.currentLocation.name.Value + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
		int hearts = 10;
		while (dialogue2 == null && hearts >= 2)
		{
			if (heartLevel >= hearts)
			{
				dialogue2 = this.TryGetDialogue(preface + Game1.currentLocation.name.Value + hearts);
			}
			hearts -= 2;
		}
		dialogue2 = dialogue2 ?? this.TryGetDialogue(preface + Game1.currentLocation.Name);
		if (dialogue2 != null)
		{
			dialogue2.removeOnNextMove = true;
			this.CurrentDialogue.Push(dialogue2);
			return true;
		}
		return false;
	}

	/// <summary>Try to get a specific dialogue from the loaded <see cref="P:StardewValley.NPC.Dialogue" />.</summary>
	/// <param name="key">The dialogue key.</param>
	/// <returns>Returns the matched dialogue if found, else <c>null</c>.</returns>
	public Dialogue TryGetDialogue(string key)
	{
		Dictionary<string, string> dialogue = this.Dialogue;
		if (dialogue != null && dialogue.TryGetValue(key, out var text))
		{
			return new Dialogue(this, this.LoadedDialogueKey + ":" + key, text);
		}
		return null;
	}

	/// <summary>Try to get a specific dialogue from the loaded <see cref="P:StardewValley.NPC.Dialogue" /> using a matching gift taste tag like 'Positive'.</summary>
	/// <param name="giftTaste">The gift taste, matching a value lke <see cref="F:StardewValley.NPC.gift_taste_love" />.</param>
	/// <param name="key">Get the dialogue key to try, given a gift taste tag like 'Positive'.</param>
	/// <returns>Returns the matched dialogue if found, else <c>null</c>.</returns>
	public Dialogue TryGetDialogueByGiftTaste(int giftTaste, Func<string, string> getKey)
	{
		switch (giftTaste)
		{
		case 0:
		case 7:
			return this.TryGetDialogue(getKey("Loved")) ?? this.TryGetDialogue(getKey("Positive"));
		case 2:
			return this.TryGetDialogue(getKey("Liked")) ?? this.TryGetDialogue(getKey("Positive"));
		case 4:
			return this.TryGetDialogue(getKey("Disliked")) ?? this.TryGetDialogue(getKey("Negative"));
		case 6:
			return this.TryGetDialogue(getKey("Hated")) ?? this.TryGetDialogue(getKey("Negative"));
		default:
			return this.TryGetDialogue(getKey("Neutral")) ?? this.TryGetDialogue(getKey("Positive"));
		}
	}

	/// <summary>Try to get a specific dialogue from the loaded <see cref="P:StardewValley.NPC.Dialogue" />.</summary>
	/// <param name="key">The dialogue key.</param>
	/// <param name="substitutions">The values with which to replace placeholders like <c>{0}</c> in the loaded text.</param>
	/// <returns>Returns the matched dialogue if found, else <c>null</c>.</returns>
	public Dialogue TryGetDialogue(string key, params object[] substitutions)
	{
		Dictionary<string, string> dialogue = this.Dialogue;
		if (dialogue != null && dialogue.TryGetValue(key, out var text))
		{
			return new Dialogue(this, this.LoadedDialogueKey + ":" + key, string.Format(text, substitutions));
		}
		return null;
	}

	/// <summary>Try to get a dialogue from the loaded <see cref="P:StardewValley.NPC.Dialogue" />, applying variant rules for roommates, marriage, inlaws, dates, etc.</summary>
	/// <param name="preface">A prefix added to the translation keys to look up.</param>
	/// <param name="heartLevel">The NPC's heart level with the player.</param>
	/// <param name="appendToEnd">A suffix added to the translation keys to look up.</param>
	/// <returns>Returns the best matched dialogue if found, else <c>null</c>.</returns>
	public Dialogue tryToRetrieveDialogue(string preface, int heartLevel, string appendToEnd = "")
	{
		int year = Game1.year;
		if (Game1.year > 2)
		{
			year = 2;
		}
		if (!string.IsNullOrEmpty(Game1.player.spouse) && appendToEnd.Equals(""))
		{
			if (Game1.player.hasCurrentOrPendingRoommate())
			{
				Dialogue s = this.tryToRetrieveDialogue(preface, heartLevel, "_roommate_" + Game1.player.spouse);
				if (s != null)
				{
					return s;
				}
			}
			else
			{
				Dialogue s2 = this.tryToRetrieveDialogue(preface, heartLevel, "_inlaw_" + Game1.player.spouse);
				if (s2 != null)
				{
					return s2;
				}
			}
		}
		string day_name = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
		if (year == 1)
		{
			Dialogue dialogue = this.TryGetDialogue(preface + Game1.dayOfMonth + appendToEnd);
			if (dialogue != null)
			{
				return dialogue;
			}
		}
		Dialogue dialogue2 = this.TryGetDialogue(preface + Game1.dayOfMonth + "_" + year + appendToEnd);
		if (dialogue2 != null)
		{
			return dialogue2;
		}
		Dialogue dialogue3 = this.TryGetDialogue(preface + Game1.dayOfMonth + "_*" + appendToEnd);
		if (dialogue3 != null)
		{
			return dialogue3;
		}
		for (int hearts = 10; hearts >= 2; hearts -= 2)
		{
			if (heartLevel >= hearts)
			{
				Dialogue dialogue4 = this.TryGetDialogue(preface + day_name + hearts + "_" + year + appendToEnd) ?? this.TryGetDialogue(preface + day_name + hearts + appendToEnd);
				if (dialogue4 != null)
				{
					if (hearts == 4 && preface == "fall_" && day_name == "Mon" && base.Name.Equals("Penny") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						return this.TryGetDialogue(preface + day_name + "_" + year + appendToEnd) ?? this.TryGetDialogue("fall_Mon");
					}
					return dialogue4;
				}
			}
		}
		Dialogue dialogue5 = this.TryGetDialogue(preface + day_name + appendToEnd);
		if (dialogue5 != null)
		{
			Dialogue specificDialogue = this.TryGetDialogue(preface + day_name + "_" + year + appendToEnd);
			if (specificDialogue != null)
			{
				dialogue5 = specificDialogue;
			}
		}
		if (dialogue5 != null && base.Name.Equals("Caroline") && Game1.isLocationAccessible("CommunityCenter") && preface == "summer_" && day_name == "Mon")
		{
			dialogue5 = this.TryGetDialogue("summer_Wed");
		}
		if (dialogue5 != null)
		{
			return dialogue5;
		}
		return null;
	}

	public virtual void checkSchedule(int timeOfDay)
	{
		if (this.currentScheduleDelay == 0f && this.scheduleDelaySeconds > 0f)
		{
			this.currentScheduleDelay = this.scheduleDelaySeconds;
		}
		else
		{
			if (this.returningToEndPoint)
			{
				return;
			}
			this.updatedDialogueYet = false;
			this.extraDialogueMessageToAddThisMorning = null;
			if (this.ignoreScheduleToday || this.Schedule == null)
			{
				return;
			}
			SchedulePathDescription possibleNewDirections = null;
			if (this.lastAttemptedSchedule < timeOfDay)
			{
				this.lastAttemptedSchedule = timeOfDay;
				this.Schedule.TryGetValue(timeOfDay, out possibleNewDirections);
				if (possibleNewDirections != null)
				{
					this.queuedSchedulePaths.Add(possibleNewDirections);
				}
				possibleNewDirections = null;
			}
			PathFindController pathFindController = base.controller;
			if (pathFindController != null && pathFindController.pathToEndPoint?.Count > 0)
			{
				return;
			}
			if (this.queuedSchedulePaths.Count > 0 && timeOfDay >= this.queuedSchedulePaths[0].time)
			{
				possibleNewDirections = this.queuedSchedulePaths[0];
			}
			if (possibleNewDirections == null)
			{
				return;
			}
			this.prepareToDisembarkOnNewSchedulePath();
			if (!this.returningToEndPoint && this.temporaryController == null)
			{
				this.directionsToNewLocation = possibleNewDirections;
				if (this.queuedSchedulePaths.Count > 0)
				{
					this.queuedSchedulePaths.RemoveAt(0);
				}
				base.controller = new PathFindController(this.directionsToNewLocation.route, this, Utility.getGameLocationOfCharacter(this))
				{
					finalFacingDirection = this.directionsToNewLocation.facingDirection,
					endBehaviorFunction = this.getRouteEndBehaviorFunction(this.directionsToNewLocation.endOfRouteBehavior, this.directionsToNewLocation.endOfRouteMessage)
				};
				if (base.controller.pathToEndPoint == null || base.controller.pathToEndPoint.Count == 0)
				{
					base.controller.endBehaviorFunction?.Invoke(this, base.currentLocation);
					base.controller = null;
				}
				if (this.directionsToNewLocation?.route != null)
				{
					this.previousEndPoint = this.directionsToNewLocation.route.LastOrDefault();
				}
			}
		}
	}

	private void finishEndOfRouteAnimation()
	{
		this._finishingEndOfRouteBehavior = this._startedEndOfRouteBehavior;
		this._startedEndOfRouteBehavior = null;
		string finishingEndOfRouteBehavior = this._finishingEndOfRouteBehavior;
		if (!(finishingEndOfRouteBehavior == "change_beach"))
		{
			if (finishingEndOfRouteBehavior == "change_normal")
			{
				this.shouldWearIslandAttire.Value = false;
				this.currentlyDoingEndOfRouteAnimation = false;
			}
		}
		else
		{
			this.shouldWearIslandAttire.Value = true;
			this.currentlyDoingEndOfRouteAnimation = false;
		}
		while (this.CurrentDialogue.Count > 0 && this.CurrentDialogue.Peek().removeOnNextMove)
		{
			this.CurrentDialogue.Pop();
		}
		this.shouldSayMarriageDialogue.Value = false;
		this.currentMarriageDialogue.Clear();
		this.nextEndOfRouteMessage = null;
		this.endOfRouteMessage.Value = null;
		if (this.currentlyDoingEndOfRouteAnimation && this.routeEndOutro != null)
		{
			bool addedFrame = false;
			for (int i = 0; i < this.routeEndOutro.Length; i++)
			{
				if (!addedFrame)
				{
					this.Sprite.ClearAnimation();
					addedFrame = true;
				}
				if (i == this.routeEndOutro.Length - 1)
				{
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndOutro[i], 100, 0, secondaryArm: false, flip: false, routeEndAnimationFinished, behaviorAtEndOfFrame: true));
				}
				else
				{
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndOutro[i], 100, 0, secondaryArm: false, flip: false));
				}
			}
			if (!addedFrame)
			{
				this.routeEndAnimationFinished(null);
			}
			if (this._finishingEndOfRouteBehavior != null)
			{
				this.finishRouteBehavior(this._finishingEndOfRouteBehavior);
			}
		}
		else
		{
			this.routeEndAnimationFinished(null);
		}
	}

	protected virtual void prepareToDisembarkOnNewSchedulePath()
	{
		this.finishEndOfRouteAnimation();
		this.doingEndOfRouteAnimation.Value = false;
		this.currentlyDoingEndOfRouteAnimation = false;
		if (!this.isMarried())
		{
			return;
		}
		if (this.temporaryController == null && Utility.getGameLocationOfCharacter(this) is FarmHouse)
		{
			this.temporaryController = new PathFindController(this, this.getHome(), new Point(this.getHome().warps[0].X, this.getHome().warps[0].Y), 2, clearMarriageDialogues: true)
			{
				NPCSchedule = true
			};
			if (this.temporaryController.pathToEndPoint == null || this.temporaryController.pathToEndPoint.Count <= 0)
			{
				this.temporaryController = null;
				this.ClearSchedule();
			}
			else
			{
				this.followSchedule = true;
			}
		}
		else if (Utility.getGameLocationOfCharacter(this) is Farm)
		{
			this.temporaryController = null;
			this.ClearSchedule();
		}
	}

	public void checkForMarriageDialogue(int timeOfDay, GameLocation location)
	{
		if (base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
		{
			return;
		}
		switch (timeOfDay)
		{
		case 1100:
			this.setRandomAfternoonMarriageDialogue(1100, location);
			break;
		case 1800:
			if (location is FarmHouse)
			{
				int which = Utility.CreateDaySaveRandom(timeOfDay, this.getSpouse().UniqueMultiplayerID).Next(Game1.isRaining ? 7 : 6) - 1;
				string suffix = ((which >= 0) ? (which.ToString() ?? "") : base.Name);
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", (Game1.isRaining ? "Rainy" : "Indoor") + "_Night_" + suffix, false);
			}
			break;
		}
	}

	private void routeEndAnimationFinished(Farmer who)
	{
		this.doingEndOfRouteAnimation.Value = false;
		base.freezeMotion = false;
		CharacterData data = this.GetData();
		this.Sprite.SpriteWidth = data?.Size.X ?? 16;
		this.Sprite.SpriteHeight = data?.Size.Y ?? 32;
		this.Sprite.UpdateSourceRect();
		this.Sprite.oldFrame = this._beforeEndOfRouteAnimationFrame;
		this.Sprite.StopAnimation();
		this.endOfRouteMessage.Value = null;
		base.isCharging = false;
		base.speed = 2;
		this.addedSpeed = 0f;
		this.goingToDoEndOfRouteAnimation.Value = false;
		if (this.isWalkingInSquare)
		{
			this.returningToEndPoint = true;
		}
		if (this._finishingEndOfRouteBehavior == "penny_dishes")
		{
			base.drawOffset = Vector2.Zero;
		}
		if (this.appliedRouteAnimationOffset != Vector2.Zero)
		{
			base.drawOffset = Vector2.Zero;
			this.appliedRouteAnimationOffset = Vector2.Zero;
		}
		this._finishingEndOfRouteBehavior = null;
	}

	public bool isOnSilentTemporaryMessage()
	{
		if ((this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value) && this.endOfRouteMessage.Value != null && this.endOfRouteMessage.Value.EqualsIgnoreCase("silent"))
		{
			return true;
		}
		return false;
	}

	public bool hasTemporaryMessageAvailable()
	{
		if (this.isDivorcedFrom(Game1.player))
		{
			return false;
		}
		if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
		{
			return true;
		}
		if (this.endOfRouteMessage.Value != null && (this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value))
		{
			return true;
		}
		return false;
	}

	public bool setTemporaryMessages(Farmer who)
	{
		if (this.isOnSilentTemporaryMessage())
		{
			return true;
		}
		if (this.endOfRouteMessage.Value != null && (this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value))
		{
			if (!this.isDivorcedFrom(Game1.player) && (!this.endOfRouteMessage.Value.Contains("marriage") || this.getSpouse() == Game1.player))
			{
				this._PushTemporaryDialogue(this.endOfRouteMessage.Value);
				return false;
			}
		}
		else if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
		{
			this._PushTemporaryDialogue(base.currentLocation.GetLocationOverrideDialogue(this));
			return false;
		}
		return false;
	}

	/// <summary>Add a dialogue to the NPC's queue which is shown before other dialogues, and replaced if another temporary dialogue is added later.</summary>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	protected void _PushTemporaryDialogue(string translationKey)
	{
		string oldTranslationKey = translationKey;
		try
		{
			if (Game1.player.friendshipData.TryGetValue(base.Name, out var friendship))
			{
				string relationshipKey = $"{translationKey}_{friendship.Status}";
				if (Game1.content.LoadStringReturnNullIfNotFound(relationshipKey) != null)
				{
					translationKey = relationshipKey;
				}
			}
			if (this.CurrentDialogue.Count == 0 || this.CurrentDialogue.Peek().temporaryDialogueKey != translationKey)
			{
				Dialogue temporaryDialogue = new Dialogue(this, translationKey)
				{
					removeOnNextMove = true,
					temporaryDialogueKey = translationKey
				};
				this.CurrentDialogue.Push(temporaryDialogue);
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error($"NPC '{base.Name}' failed setting temporary dialogue key '{translationKey}'{((translationKey != oldTranslationKey) ? (" (from dialogue key '" + oldTranslationKey + "')") : "")}", exception);
		}
	}

	private void walkInSquareAtEndOfRoute(Character c, GameLocation l)
	{
		this.startRouteBehavior(this.endOfRouteBehaviorName.Value);
	}

	private void doAnimationAtEndOfScheduleRoute(Character c, GameLocation l)
	{
		this.doingEndOfRouteAnimation.Value = true;
		this.reallyDoAnimationAtEndOfScheduleRoute();
		this.currentlyDoingEndOfRouteAnimation = true;
	}

	private void reallyDoAnimationAtEndOfScheduleRoute()
	{
		this._startedEndOfRouteBehavior = this.loadedEndOfRouteBehavior;
		bool is_special_route_behavior = false;
		string startedEndOfRouteBehavior = this._startedEndOfRouteBehavior;
		if (startedEndOfRouteBehavior == "change_beach" || startedEndOfRouteBehavior == "change_normal")
		{
			is_special_route_behavior = true;
		}
		if (!is_special_route_behavior)
		{
			if (this._startedEndOfRouteBehavior == "penny_dishes")
			{
				base.drawOffset = new Vector2(0f, 16f);
			}
			if (this._startedEndOfRouteBehavior.EndsWith("_sleep"))
			{
				this.layingDown = true;
				this.HideShadow = true;
			}
			if (this.routeAnimationMetadata != null)
			{
				for (int i = 0; i < this.routeAnimationMetadata.Length; i++)
				{
					string[] metadata = ArgUtility.SplitBySpace(this.routeAnimationMetadata[i]);
					startedEndOfRouteBehavior = metadata[0];
					if (!(startedEndOfRouteBehavior == "laying_down"))
					{
						if (startedEndOfRouteBehavior == "offset")
						{
							this.appliedRouteAnimationOffset = new Vector2(int.Parse(metadata[1]), int.Parse(metadata[2]));
						}
					}
					else
					{
						this.layingDown = true;
						this.HideShadow = true;
					}
				}
			}
			if (this.appliedRouteAnimationOffset != Vector2.Zero)
			{
				base.drawOffset = this.appliedRouteAnimationOffset;
			}
			if (this._skipRouteEndIntro)
			{
				this.doMiddleAnimation(null);
			}
			else
			{
				this.Sprite.ClearAnimation();
				for (int j = 0; j < this.routeEndIntro.Length; j++)
				{
					if (j == this.routeEndIntro.Length - 1)
					{
						this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndIntro[j], 100, 0, secondaryArm: false, flip: false, doMiddleAnimation, behaviorAtEndOfFrame: true));
					}
					else
					{
						this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndIntro[j], 100, 0, secondaryArm: false, flip: false));
					}
				}
			}
		}
		this._skipRouteEndIntro = false;
		this.doingEndOfRouteAnimation.Value = true;
		base.freezeMotion = true;
		this._beforeEndOfRouteAnimationFrame = this.Sprite.oldFrame;
	}

	private void doMiddleAnimation(Farmer who)
	{
		this.Sprite.ClearAnimation();
		for (int i = 0; i < this.routeEndAnimation.Length; i++)
		{
			this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndAnimation[i], 100, 0, secondaryArm: false, flip: false));
		}
		this.Sprite.loop = true;
		if (this._startedEndOfRouteBehavior != null)
		{
			this.startRouteBehavior(this._startedEndOfRouteBehavior);
		}
	}

	private void startRouteBehavior(string behaviorName)
	{
		if (behaviorName.Length > 0 && behaviorName[0] == '"')
		{
			if (Game1.IsMasterGame)
			{
				this.endOfRouteMessage.Value = behaviorName.Replace("\"", "");
			}
			return;
		}
		if (behaviorName.Contains("square_") && Game1.IsMasterGame)
		{
			this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64);
			string[] squareSplit = behaviorName.Split('_');
			this.walkInSquare(Convert.ToInt32(squareSplit[1]), Convert.ToInt32(squareSplit[2]), 6000);
			if (squareSplit.Length > 3)
			{
				this.squareMovementFacingPreference = Convert.ToInt32(squareSplit[3]);
			}
			else
			{
				this.squareMovementFacingPreference = -1;
			}
		}
		if (behaviorName.Contains("sleep"))
		{
			this.isPlayingSleepingAnimation = true;
			this.playSleepingAnimation();
		}
		switch (behaviorName)
		{
		case "abigail_videogames":
			if (Game1.IsMasterGame)
			{
				Game1.multiplayer.broadcastSprites(Utility.getGameLocationOfCharacter(this), new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 999999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 688
				});
				base.doEmote(52);
			}
			break;
		case "dick_fish":
			base.extendSourceRect(0, 32);
			this.Sprite.tempSpriteHeight = 64;
			base.drawOffset = new Vector2(0f, 96f);
			this.Sprite.ignoreSourceRectUpdates = false;
			if (Utility.isOnScreen(Utility.Vector2ToPoint(base.Position), 64, base.currentLocation))
			{
				base.currentLocation.playSound("slosh", base.Tile);
			}
			break;
		case "clint_hammer":
			base.extendSourceRect(16, 0);
			this.Sprite.SpriteWidth = 32;
			this.Sprite.ignoreSourceRectUpdates = false;
			this.Sprite.currentFrame = 8;
			this.Sprite.CurrentAnimation[14] = new FarmerSprite.AnimationFrame(9, 100, 0, secondaryArm: false, flip: false, clintHammerSound);
			break;
		case "birdie_fish":
			base.extendSourceRect(16, 0);
			this.Sprite.SpriteWidth = 32;
			this.Sprite.ignoreSourceRectUpdates = false;
			this.Sprite.currentFrame = 8;
			break;
		}
	}

	public void playSleepingAnimation()
	{
		this.isSleeping.Value = true;
		Vector2 draw_offset = new Vector2(0f, base.name.Equals("Sebastian") ? 12 : (-4));
		if (this.isMarried())
		{
			draw_offset.X = -12f;
		}
		base.drawOffset = draw_offset;
		if (!this.isPlayingSleepingAnimation)
		{
			if (DataLoader.AnimationDescriptions(Game1.content).TryGetValue(base.name.Value.ToLower() + "_sleep", out var animationData))
			{
				int sleep_frame = Convert.ToInt32(animationData.Split('/')[0]);
				this.Sprite.ClearAnimation();
				this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(sleep_frame, 100, secondaryArm: false, flip: false));
				this.Sprite.loop = true;
			}
			this.isPlayingSleepingAnimation = true;
		}
	}

	private void finishRouteBehavior(string behaviorName)
	{
		switch (behaviorName)
		{
		case "abigail_videogames":
			Utility.getGameLocationOfCharacter(this).removeTemporarySpritesWithID(688);
			break;
		case "birdie_fish":
		case "clint_hammer":
		case "dick_fish":
		{
			this.reloadSprite();
			CharacterData data = this.GetData();
			this.Sprite.SpriteWidth = data?.Size.X ?? 16;
			this.Sprite.SpriteHeight = data?.Size.Y ?? 32;
			this.Sprite.UpdateSourceRect();
			base.drawOffset = Vector2.Zero;
			this.Halt();
			base.movementPause = 1;
			break;
		}
		}
		if (this.layingDown)
		{
			this.layingDown = false;
			this.HideShadow = false;
		}
	}

	public bool IsReturningToEndPoint()
	{
		return this.returningToEndPoint;
	}

	public void StartActivityWalkInSquare(int square_width, int square_height, int pause_offset)
	{
		Point tile = base.TilePoint;
		this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(tile.X * 64, tile.Y * 64, 64, 64);
		this.walkInSquare(square_height, square_height, pause_offset);
	}

	public void EndActivityRouteEndBehavior()
	{
		this.finishEndOfRouteAnimation();
	}

	public void StartActivityRouteEndBehavior(string behavior_name, string end_message)
	{
		this.getRouteEndBehaviorFunction(behavior_name, end_message)?.Invoke(this, base.currentLocation);
	}

	protected PathFindController.endBehavior getRouteEndBehaviorFunction(string behaviorName, string endMessage)
	{
		if (endMessage != null || (behaviorName != null && behaviorName.Length > 0 && behaviorName[0] == '"'))
		{
			this.nextEndOfRouteMessage = endMessage.Replace("\"", "");
		}
		if (behaviorName != null)
		{
			if (behaviorName.Length > 0 && behaviorName.Contains("square_"))
			{
				this.endOfRouteBehaviorName.Value = behaviorName;
				return walkInSquareAtEndOfRoute;
			}
			Dictionary<string, string> animationDescriptions = DataLoader.AnimationDescriptions(Game1.content);
			if (behaviorName == "change_beach" || behaviorName == "change_normal")
			{
				this.endOfRouteBehaviorName.Value = behaviorName;
				this.goingToDoEndOfRouteAnimation.Value = true;
			}
			else
			{
				if (!animationDescriptions.ContainsKey(behaviorName))
				{
					return null;
				}
				this.endOfRouteBehaviorName.Value = behaviorName;
				this.loadEndOfRouteBehavior(this.endOfRouteBehaviorName.Value);
				this.goingToDoEndOfRouteAnimation.Value = true;
			}
			return doAnimationAtEndOfScheduleRoute;
		}
		return null;
	}

	private void loadEndOfRouteBehavior(string name)
	{
		this.loadedEndOfRouteBehavior = name;
		if (name.Length > 0 && name.Contains("square_"))
		{
			return;
		}
		string rawData = null;
		try
		{
			if (DataLoader.AnimationDescriptions(Game1.content).TryGetValue(name, out rawData))
			{
				string[] fields = rawData.Split('/');
				this.routeEndIntro = Utility.parseStringToIntArray(fields[0]);
				this.routeEndAnimation = Utility.parseStringToIntArray(fields[1]);
				this.routeEndOutro = Utility.parseStringToIntArray(fields[2]);
				if (fields.Length > 3 && fields[3] != "")
				{
					this.nextEndOfRouteMessage = fields[3];
				}
				if (fields.Length > 4)
				{
					this.routeAnimationMetadata = fields.Skip(4).ToArray();
				}
				else
				{
					this.routeAnimationMetadata = null;
				}
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error($"NPC {base.Name} failed to apply end-of-route behavior '{name}'{((rawData != null) ? (" with raw data '" + rawData + "'") : "")}.", exception);
		}
	}

	public void shake(int duration)
	{
		this.shakeTimer = duration;
	}

	public void setNewDialogue(string translationKey, bool add = false, bool clearOnMovement = false)
	{
		this.setNewDialogue(new Dialogue(this, translationKey), add, clearOnMovement);
	}

	public void setNewDialogue(Dialogue dialogue, bool add = false, bool clearOnMovement = false)
	{
		if (!add)
		{
			this.CurrentDialogue.Clear();
		}
		dialogue.removeOnNextMove = clearOnMovement;
		this.CurrentDialogue.Push(dialogue);
	}

	private void setNewDialogue(string dialogueSheetName, string dialogueSheetKey, bool clearOnMovement = false)
	{
		this.CurrentDialogue.Clear();
		string translationKey = dialogueSheetKey + base.Name;
		if (dialogueSheetName.Contains("Marriage"))
		{
			if (this.getSpouse() == Game1.player)
			{
				Dialogue dialogue = this.tryToGetMarriageSpecificDialogue(translationKey);
				if (dialogue == null)
				{
					Game1.log.Warn($"NPC '{base.Name}' couldn't set marriage dialogue key '{translationKey}': not found.");
					dialogue = StardewValley.Dialogue.GetFallbackForError(this);
				}
				dialogue.removeOnNextMove = clearOnMovement;
				this.CurrentDialogue.Push(dialogue);
			}
			return;
		}
		string translationPath = "Characters\\Dialogue\\" + dialogueSheetName + ":" + translationKey;
		Dialogue dialogue2 = StardewValley.Dialogue.TryGetDialogue(this, translationPath);
		if (dialogue2 == null)
		{
			Game1.log.Warn($"NPC '{base.Name}' couldn't set dialogue key '{translationPath}': not found.");
			dialogue2 = StardewValley.Dialogue.GetFallbackForError(this);
		}
		if (dialogue2 != null)
		{
			dialogue2.removeOnNextMove = clearOnMovement;
			this.CurrentDialogue.Push(dialogue2);
		}
	}

	public string GetDialogueSheetName()
	{
		if (base.Name == "Leo" && this.DefaultMap != "IslandHut")
		{
			return base.Name + "Mainland";
		}
		return base.Name;
	}

	public void setSpouseRoomMarriageDialogue()
	{
		this.currentMarriageDialogue.Clear();
		this.addMarriageDialogue("MarriageDialogue", "spouseRoom_" + base.Name, false);
	}

	public void setRandomAfternoonMarriageDialogue(int time, GameLocation location, bool countAsDailyAfternoon = false)
	{
		if ((base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri") || this.hasSaidAfternoonDialogue.Value)
		{
			return;
		}
		if (countAsDailyAfternoon)
		{
			this.hasSaidAfternoonDialogue.Value = true;
		}
		Random r = Utility.CreateDaySaveRandom(time);
		int hearts = this.getSpouse().getFriendshipHeartLevelForNPC(base.Name);
		if (!(location is FarmHouse))
		{
			if (location is Farm)
			{
				this.currentMarriageDialogue.Clear();
				if (r.NextDouble() < 0.2)
				{
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + base.Name, false);
				}
				else
				{
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
			}
		}
		else if (r.NextBool())
		{
			if (hearts < 9)
			{
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", ((r.NextDouble() < (double)((float)hearts / 11f)) ? "Neutral_" : "Bad_") + r.Next(10), false);
			}
			else if (r.NextDouble() < 0.05)
			{
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + base.Name, false);
			}
			else if ((hearts >= 10 && r.NextBool()) || (hearts >= 11 && r.NextDouble() < 0.75) || (hearts >= 12 && r.NextDouble() < 0.95))
			{
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", "Good_" + r.Next(10), false);
			}
			else
			{
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", "Neutral_" + r.Next(10), false);
			}
		}
	}

	/// <summary>Get whether it's the NPC's birthday today.</summary>
	public bool isBirthday()
	{
		if (this.Birthday_Season == Game1.currentSeason)
		{
			return this.Birthday_Day == Game1.dayOfMonth;
		}
		return false;
	}

	/// <summary>Get the NPC's first loved item for the Statue of Endless Fortune.</summary>
	public Item getFavoriteItem()
	{
		if (Game1.NPCGiftTastes.TryGetValue(base.Name, out var rawData))
		{
			Item item = (from id in ArgUtility.SplitBySpace(rawData.Split('/')[1])
				select ItemRegistry.ResolveMetadata(id)?.CreateItem()).FirstOrDefault((Item p) => p != null);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	/// <summary>Get the NPC's data from <see cref="F:StardewValley.Game1.characterData" />, if found.</summary>
	public CharacterData GetData()
	{
		if (!this.IsVillager || !NPC.TryGetData(base.name.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get an NPC's data from <see cref="F:StardewValley.Game1.characterData" />.</summary>
	/// <param name="name">The NPC's internal name (i.e. the key in <see cref="F:StardewValley.Game1.characterData" />).</param>
	/// <param name="data">The NPC data, if found.</param>
	/// <returns>Returns whether the NPC data was found.</returns>
	public static bool TryGetData(string name, out CharacterData data)
	{
		if (name == null)
		{
			data = null;
			return false;
		}
		return Game1.characterData.TryGetValue(name, out data);
	}

	/// <summary>Get the translated display name for an NPC from the underlying data, if any.</summary>
	/// <param name="name">The NPC's internal name.</param>
	public static string GetDisplayName(string name)
	{
		NPC.TryGetData(name, out var data);
		return TokenParser.ParseText(data?.DisplayName) ?? name;
	}

	/// <summary>Get whether social features (like birthdays, gift giving, friendship, and an entry in the social tab) should be enabled for an NPC based on their entry in <c>Data/Characters</c>.</summary>
	/// <param name="name">The NPC name to check.</param>
	/// <param name="location">The location to assume they're in when parsing conditions.</param>
	/// <remarks>Most code should use <see cref="P:StardewValley.NPC.CanSocialize" /> instead.</remarks>
	public static bool CanSocializePerData(string name, GameLocation location)
	{
		if (NPC.TryGetData(name, out var data))
		{
			return GameStateQuery.CheckConditions(data.CanSocialize, location);
		}
		return false;
	}

	/// <summary>Get a tokenized string for the NPC's display name.</summary>
	public string GetTokenizedDisplayName()
	{
		return this.GetData()?.DisplayName ?? this.displayName;
	}

	/// <summary>Get whether this NPC speaks Dwarvish, which the player can only understand after finding the Dwarvish Translation Guide.</summary>
	public bool SpeaksDwarvish()
	{
		CharacterData data = this.GetData();
		if (data == null)
		{
			return false;
		}
		return data.Language == NpcLanguage.Dwarvish;
	}

	public virtual void receiveGift(Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
	{
		if (this.CanReceiveGifts())
		{
			float qualityChangeMultipler = 1f;
			switch (o.Quality)
			{
			case 1:
				qualityChangeMultipler = 1.1f;
				break;
			case 2:
				qualityChangeMultipler = 1.25f;
				break;
			case 4:
				qualityChangeMultipler = 1.5f;
				break;
			}
			if (this.isBirthday())
			{
				friendshipChangeMultiplier = 8f;
			}
			if (this.getSpouse() != null && this.getSpouse().Equals(giver))
			{
				friendshipChangeMultiplier /= 2f;
			}
			giver.onGiftGiven(this, o);
			Game1.stats.GiftsGiven++;
			giver.currentLocation.localSound("give_gift");
			if (updateGiftLimitInfo)
			{
				giver.friendshipData[base.Name].GiftsToday++;
				giver.friendshipData[base.Name].GiftsThisWeek++;
				giver.friendshipData[base.Name].LastGiftDate = new WorldDate(Game1.Date);
			}
			switch (giver.FacingDirection)
			{
			case 0:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(80, 50f);
				break;
			case 1:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(72, 50f);
				break;
			case 2:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(64, 50f);
				break;
			case 3:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(88, 50f);
				break;
			}
			int tasteForItem = this.getGiftTasteForThisItem(o);
			switch (tasteForItem)
			{
			case 7:
				giver.changeFriendship(Math.Min(750, (int)(250f * friendshipChangeMultiplier)), this);
				base.doEmote(56);
				base.faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
				break;
			case 0:
				giver.changeFriendship((int)(80f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				base.doEmote(20);
				base.faceTowardFarmerForPeriod(15000, 4, faceAway: false, giver);
				break;
			case 6:
				giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), this);
				base.doEmote(12);
				base.faceTowardFarmerForPeriod(15000, 4, faceAway: true, giver);
				break;
			case 2:
				giver.changeFriendship((int)(45f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				base.faceTowardFarmerForPeriod(7000, 3, faceAway: true, giver);
				break;
			case 4:
				giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), this);
				break;
			default:
				giver.changeFriendship((int)(20f * friendshipChangeMultiplier), this);
				break;
			}
			if (showResponse)
			{
				Game1.DrawDialogue(this.GetGiftReaction(giver, o, tasteForItem));
			}
		}
	}

	/// <summary>Get the NPC's reaction dialogue for receiving an item as a gift.</summary>
	/// <param name="giver">The player giving the gift.</param>
	/// <param name="gift">The item being gifted.</param>
	/// <param name="taste">The NPC's gift taste for this item, as returned by <see cref="M:StardewValley.NPC.getGiftTasteForThisItem(StardewValley.Item)" />.</param>
	/// <returns>Returns the dialogue if the NPC can receive gifts, else <c>null</c>.</returns>
	public virtual Dialogue GetGiftReaction(Farmer giver, Object gift, int taste)
	{
		if (!this.CanReceiveGifts() || !Game1.NPCGiftTastes.TryGetValue(base.Name, out var rawData))
		{
			return null;
		}
		Dialogue dialogue = null;
		string portrait = null;
		if (base.Name == "Krobus" && Game1.Date.DayOfWeek == DayOfWeek.Friday)
		{
			dialogue = this.TryGetDialogue("Fri") ?? StardewValley.Dialogue.GetFallbackForError(this);
		}
		else if (this.isBirthday())
		{
			dialogue = this.TryGetDialogue("AcceptBirthdayGift_" + gift.QualifiedItemId) ?? (from tag in gift.GetContextTags()
				select this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptBirthdayGift_" + tasteTag + "_" + tag)).FirstOrDefault((Dialogue p) => p != null) ?? (from tag in gift.GetContextTags()
				select this.TryGetDialogue("AcceptBirthdayGift_" + tag)).FirstOrDefault((Dialogue p) => p != null) ?? this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptBirthdayGift_" + tasteTag) ?? this.TryGetDialogue("AcceptBirthdayGift");
			switch (taste)
			{
			case 0:
			case 2:
			case 7:
				portrait = "$h";
				dialogue = dialogue ?? ((!Game1.random.NextBool()) ? ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4274", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4275")) : ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4276", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4277", isGendered: true)));
				break;
			case 4:
			case 6:
				portrait = "$s";
				dialogue = dialogue ?? ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4278", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4279", isGendered: true));
				break;
			default:
				dialogue = dialogue ?? ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4280") : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4281", isGendered: true));
				break;
			}
		}
		else
		{
			dialogue = this.TryGetDialogue("AcceptGift_" + gift.QualifiedItemId) ?? (from tag in gift.GetContextTags()
				select this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptGift_" + tasteTag + "_" + tag)).FirstOrDefault((Dialogue p) => p != null) ?? (from tag in gift.GetContextTags()
				select this.TryGetDialogue("AcceptGift_" + tag)).FirstOrDefault((Dialogue p) => p != null) ?? this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptGift_" + tasteTag) ?? this.TryGetDialogue("AcceptGift");
			string[] rawFields = rawData.Split('/');
			switch (taste)
			{
			case 7:
				portrait = "$h";
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, 0));
				break;
			case 0:
			case 2:
				if (dialogue == null)
				{
					portrait = "$h";
				}
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, taste));
				break;
			case 4:
			case 6:
				portrait = "$s";
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, taste));
				break;
			default:
				dialogue = dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, 8));
				break;
			}
		}
		if (!giver.canUnderstandDwarves && this.SpeaksDwarvish())
		{
			dialogue.convertToDwarvish();
		}
		else if (portrait != null && !dialogue.CurrentEmotionSetExplicitly)
		{
			dialogue.CurrentEmotion = portrait;
		}
		return dialogue;
	}

	public override void draw(SpriteBatch b, float alpha = 1f)
	{
		int standingY = base.StandingPixel.Y;
		float mainLayerDepth = Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f));
		if (this.Sprite.Texture == null)
		{
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, base.Position);
			Microsoft.Xna.Framework.Rectangle spriteArea = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y - this.Sprite.SpriteWidth * 4, this.Sprite.SpriteWidth * 4, this.Sprite.SpriteHeight * 4);
			Utility.DrawErrorTexture(b, spriteArea, mainLayerDepth);
		}
		else if (!this.IsInvisible && (Utility.isOnScreen(base.Position, 128) || (this.EventActor && base.currentLocation is Summit)))
		{
			if (base.swimming.Value)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 80 + base.yJumpOffset * 2) + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero) - new Vector2(0f, this.yOffset), new Microsoft.Xna.Framework.Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, this.Sprite.SourceRect.Width, this.Sprite.SourceRect.Height / 2 - (int)(this.yOffset / 4f)), Color.White, this.rotation, new Vector2(32f, 96f) / 4f, Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, mainLayerDepth);
				Vector2 localPosition = base.getLocalPosition(Game1.viewport);
				b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)localPosition.X + (int)this.yOffset + 8, (int)localPosition.Y - 128 + this.Sprite.SourceRect.Height * 4 + 48 + base.yJumpOffset * 2 - (int)this.yOffset, this.Sprite.SourceRect.Width * 4 - (int)this.yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, (float)standingY / 10000f + 0.001f);
			}
			else
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(base.GetSpriteWidthForPositioning() * 4 / 2, this.GetBoundingBox().Height / 2) + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), this.Sprite.SourceRect, Color.White * alpha, this.rotation, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.scale.Value) * 4f, (base.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, mainLayerDepth);
			}
			this.DrawBreathing(b, alpha);
			this.DrawGlow(b);
			if (!Game1.eventUp)
			{
				this.DrawEmote(b);
			}
		}
	}

	public virtual void DrawBreathing(SpriteBatch b, float alpha = 1f)
	{
		if (!this.Breather || this.shakeTimer > 0 || base.swimming.Value || base.farmerPassesThrough)
		{
			return;
		}
		AnimatedSprite animatedSprite = this.Sprite;
		if (animatedSprite != null && animatedSprite.SpriteHeight > 32)
		{
			return;
		}
		AnimatedSprite animatedSprite2 = this.Sprite;
		if (animatedSprite2 != null && animatedSprite2.SpriteWidth > 16)
		{
			return;
		}
		AnimatedSprite sprite = this.Sprite;
		if (sprite.currentFrame >= 16)
		{
			return;
		}
		CharacterData data = this.GetData();
		Microsoft.Xna.Framework.Rectangle spriteRect = sprite.SourceRect;
		Microsoft.Xna.Framework.Rectangle chestBox;
		if (data != null && data.BreathChestRect.HasValue)
		{
			Microsoft.Xna.Framework.Rectangle dataRect = data.BreathChestRect.Value;
			chestBox = new Microsoft.Xna.Framework.Rectangle(spriteRect.X + dataRect.X, spriteRect.Y + dataRect.Y, dataRect.Width, dataRect.Height);
		}
		else
		{
			chestBox = new Microsoft.Xna.Framework.Rectangle(spriteRect.X + sprite.SpriteWidth / 4, spriteRect.Y + sprite.SpriteHeight / 2 + sprite.SpriteHeight / 32, sprite.SpriteHeight / 4, sprite.SpriteWidth / 2);
			if (this.Age == 2)
			{
				chestBox.Y += sprite.SpriteHeight / 6 + 1;
				chestBox.Height /= 2;
			}
			else if (this.Gender == Gender.Female)
			{
				chestBox.Y++;
				chestBox.Height /= 2;
			}
		}
		Vector2 chestPosition;
		if (data != null && data.BreathChestPosition.HasValue)
		{
			chestPosition = Utility.PointToVector2(data.BreathChestPosition.Value);
		}
		else
		{
			chestPosition = new Vector2(sprite.SpriteWidth * 4 / 2, 8f);
			if (this.Age == 2)
			{
				chestPosition.Y += sprite.SpriteHeight / 8 * 4;
				if (this is Child { Age: var num })
				{
					switch (num)
					{
					case 0:
						chestPosition.X -= 12f;
						break;
					case 1:
						chestPosition.X -= 4f;
						break;
					}
				}
			}
			else if (this.Gender == Gender.Female)
			{
				chestPosition.Y -= 4f;
			}
		}
		float breathScale = Math.Max(0f, (float)Math.Ceiling(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 600.0 + (double)(this.defaultPosition.X * 20f))) / 4f);
		int standingY = base.StandingPixel.Y;
		b.Draw(sprite.Texture, base.getLocalPosition(Game1.viewport) + chestPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), chestBox, Color.White * alpha, this.rotation, new Vector2(chestBox.Width / 2, chestBox.Height / 2 + 1), Math.Max(0.2f, base.scale.Value) * 4f + breathScale, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.992f : (((float)standingY + 0.01f) / 10000f)));
	}

	public virtual void DrawGlow(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		if (base.isGlowing)
		{
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(base.GetSpriteWidthForPositioning() * 4 / 2, this.GetBoundingBox().Height / 2) + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, this.rotation, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
		}
	}

	public virtual void DrawEmote(SpriteBatch b)
	{
		if (base.IsEmoting && !(this is Child) && !(this is Pet))
		{
			Point dataOffset = this.GetData()?.EmoteOffset ?? Point.Zero;
			Vector2 emotePosition = base.getLocalPosition(Game1.viewport);
			emotePosition = new Vector2(emotePosition.X + (float)dataOffset.X + ((float)this.Sprite.SourceRect.Width / 2f - 8f) * 4f, emotePosition.Y + (float)dataOffset.Y + (float)base.emoteYOffset - (float)((this.Sprite.SpriteHeight + 3) * 4));
			if (this.NeedsBirdieEmoteHack())
			{
				emotePosition.X += 64f;
			}
			if (this.Age == 2)
			{
				emotePosition.Y += 32f;
			}
			else if (this.Gender == Gender.Female)
			{
				emotePosition.Y += 10f;
			}
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)base.StandingPixel.Y / 10000f);
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (this.textAboveHeadTimer > 0 && this.textAboveHead != null)
		{
			Point standingPixel = base.StandingPixel;
			Vector2 local = Game1.GlobalToLocal(new Vector2(standingPixel.X, standingPixel.Y - this.Sprite.SpriteHeight * 4 - 64 + base.yJumpOffset));
			if (this.textAboveHeadStyle == 0)
			{
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			if (this.NeedsBirdieEmoteHack())
			{
				local.X += -this.GetBoundingBox().Width / 4 + 64;
			}
			if (base.shouldShadowBeOffset)
			{
				local += base.drawOffset;
			}
			Point tile = base.TilePoint;
			SpriteText.drawStringWithScrollCenteredAt(b, this.textAboveHead, (int)local.X, (int)local.Y, "", this.textAboveHeadAlpha, this.textAboveHeadColor, 1, (float)(tile.Y * 64) / 10000f + 0.001f + (float)tile.X / 10000f);
		}
	}

	public bool NeedsBirdieEmoteHack()
	{
		if (Game1.eventUp && this.Sprite.SpriteWidth == 32 && base.Name == "Birdie")
		{
			return true;
		}
		return false;
	}

	public void warpToPathControllerDestination()
	{
		if (base.controller != null)
		{
			while (base.controller.pathToEndPoint.Count > 2)
			{
				base.controller.pathToEndPoint.Pop();
				base.controller.handleWarps(new Microsoft.Xna.Framework.Rectangle(base.controller.pathToEndPoint.Peek().X * 64, base.controller.pathToEndPoint.Peek().Y * 64, 64, 64));
				base.Position = new Vector2(base.controller.pathToEndPoint.Peek().X * 64, base.controller.pathToEndPoint.Peek().Y * 64 + 16);
				this.Halt();
			}
		}
	}

	/// <summary>Get the pixel area in the <see cref="P:StardewValley.Character.Sprite" /> texture to show as the NPC's icon in contexts like the calendar and social menu.</summary>
	public virtual Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
	{
		return this.GetData()?.MugShotSourceRect ?? new Microsoft.Xna.Framework.Rectangle(0, (this.Age == 2) ? 4 : 0, 16, 24);
	}

	public void getHitByPlayer(Farmer who, GameLocation location)
	{
		base.doEmote(12);
		if (who == null)
		{
			if (Game1.IsMultiplayer)
			{
				return;
			}
			who = Game1.player;
		}
		if (who.friendshipData.ContainsKey(base.Name))
		{
			who.changeFriendship(-30, this);
			if (who.IsLocalPlayer)
			{
				this.CurrentDialogue.Clear();
				this.CurrentDialogue.Push(this.TryGetDialogue("HitBySlingshot") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4293", isGendered: true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4294")));
			}
			if (this.Sprite.Texture != null)
			{
				location.debris.Add(new Debris(this.Sprite.textureName.Value, Game1.random.Next(3, 8), Utility.PointToVector2(base.StandingPixel)));
			}
		}
		if (base.Name.Equals("Bouncer"))
		{
			location.localSound("crafting");
		}
		else
		{
			location.localSound("hitEnemy");
		}
	}

	public void walkInSquare(int squareWidth, int squareHeight, int squarePauseOffset)
	{
		this.isWalkingInSquare = true;
		this.lengthOfWalkingSquareX = squareWidth;
		this.lengthOfWalkingSquareY = squareHeight;
		this.squarePauseOffset = squarePauseOffset;
	}

	public void moveTowardPlayer(int threshold)
	{
		this.isWalkingTowardPlayer.Value = true;
		this.moveTowardPlayerThreshold.Value = threshold;
	}

	protected virtual Farmer findPlayer()
	{
		return Game1.MasterPlayer;
	}

	public virtual bool withinPlayerThreshold()
	{
		return this.withinPlayerThreshold(this.moveTowardPlayerThreshold.Value);
	}

	public virtual bool withinPlayerThreshold(int threshold)
	{
		if (base.currentLocation != null && !base.currentLocation.farmers.Any())
		{
			return false;
		}
		Vector2 tileLocationOfPlayer = this.findPlayer().Tile;
		Vector2 tileLocationOfMonster = base.Tile;
		if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold)
		{
			return true;
		}
		return false;
	}

	private Stack<Point> addToStackForSchedule(Stack<Point> original, Stack<Point> toAdd)
	{
		if (toAdd == null)
		{
			return original;
		}
		original = new Stack<Point>(original);
		while (original.Count > 0)
		{
			toAdd.Push(original.Pop());
		}
		return toAdd;
	}

	public virtual SchedulePathDescription pathfindToNextScheduleLocation(string scheduleKey, string startingLocation, int startingX, int startingY, string endingLocation, int endingX, int endingY, int finalFacingDirection, string endBehavior, string endMessage)
	{
		Stack<Point> path = new Stack<Point>();
		Point locationStartPoint = new Point(startingX, startingY);
		if (locationStartPoint == Point.Zero)
		{
			throw new Exception($"NPC {base.Name} has an invalid schedule with key '{scheduleKey}': start position in {startingLocation} is at tile (0, 0), which isn't valid.");
		}
		string[] locationsRoute = ((!startingLocation.Equals(endingLocation, StringComparison.Ordinal)) ? this.getLocationRoute(startingLocation, endingLocation) : null);
		if (locationsRoute != null)
		{
			for (int i = 0; i < locationsRoute.Length; i++)
			{
				string targetLocationName = locationsRoute[i];
				foreach (string activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
				{
					if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var data) && data.MapReplacements != null && data.MapReplacements.TryGetValue(targetLocationName, out var newName))
					{
						targetLocationName = newName;
						break;
					}
				}
				GameLocation currentLocation = Game1.RequireLocation(targetLocationName);
				if (currentLocation.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					currentLocation = Game1.RequireLocation("Trailer_Big");
				}
				if (i < locationsRoute.Length - 1)
				{
					Point target = currentLocation.getWarpPointTo(locationsRoute[i + 1]);
					if (target == Point.Zero)
					{
						throw new Exception($"NPC {base.Name} has an invalid schedule with key '{scheduleKey}': it requires a warp from {currentLocation.NameOrUniqueName} to {locationsRoute[i + 1]}, but none was found.");
					}
					path = this.addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, target, currentLocation, 30000, this));
					locationStartPoint = currentLocation.getWarpPointTarget(target, this);
				}
				else
				{
					path = this.addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), currentLocation, 30000, this));
				}
			}
		}
		else if (startingLocation.Equals(endingLocation, StringComparison.Ordinal))
		{
			string targetLocationName2 = startingLocation;
			foreach (string activePassiveFestival2 in Game1.netWorldState.Value.ActivePassiveFestivals)
			{
				if (Utility.TryGetPassiveFestivalData(activePassiveFestival2, out var data2) && data2.MapReplacements != null && data2.MapReplacements.TryGetValue(targetLocationName2, out var newName2))
				{
					targetLocationName2 = newName2;
					break;
				}
			}
			GameLocation location = Game1.RequireLocation(targetLocationName2);
			if (location.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				location = Game1.RequireLocation("Trailer_Big");
			}
			path = PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), location, 30000, this);
		}
		return new SchedulePathDescription(path, finalFacingDirection, endBehavior, endMessage, endingLocation, new Point(endingX, endingY));
	}

	private string[] getLocationRoute(string startingLocation, string endingLocation)
	{
		return WarpPathfindingCache.GetLocationRoute(startingLocation, endingLocation, this.Gender);
	}

	/// <summary>
	/// returns true if location is inaccessable and should use "Default" instead.
	///
	///
	/// </summary>
	/// <param name="locationName"></param>
	/// <param name="tileX"></param>
	/// <param name="tileY"></param>
	/// <param name="facingDirection"></param>
	/// <returns></returns>
	private bool changeScheduleForLocationAccessibility(ref string locationName, ref int tileX, ref int tileY, ref int facingDirection)
	{
		switch (locationName)
		{
		case "JojaMart":
		case "Railroad":
			if (!Game1.isLocationAccessible(locationName))
			{
				if (!this.hasMasterScheduleEntry(locationName + "_Replacement"))
				{
					return true;
				}
				string[] split = ArgUtility.SplitBySpace(this.getMasterScheduleEntry(locationName + "_Replacement"));
				locationName = split[0];
				tileX = Convert.ToInt32(split[1]);
				tileY = Convert.ToInt32(split[2]);
				facingDirection = Convert.ToInt32(split[3]);
			}
			break;
		case "CommunityCenter":
			return !Game1.isLocationAccessible(locationName);
		}
		return false;
	}

	/// <inheritdoc cref="M:StardewValley.NPC.parseMasterScheduleImpl(System.String,System.String,System.Collections.Generic.List{System.String})" />
	public virtual Dictionary<int, SchedulePathDescription> parseMasterSchedule(string scheduleKey, string rawData)
	{
		return this.parseMasterScheduleImpl(scheduleKey, rawData, new List<string>());
	}

	/// <summary>Parse a schedule script into its component commands, handling redirection like <c>GOTO</c> automatically.</summary>
	/// <param name="scheduleKey">The schedule key being parsed.</param>
	/// <param name="rawData">The raw schedule script to parse.</param>
	/// <param name="visited">The schedule keys which led to this parse (if any).</param>
	/// <remarks>This is a low-level method. Most code should call <see cref="M:StardewValley.NPC.TryLoadSchedule(System.String)" /> instead.</remarks>
	protected virtual Dictionary<int, SchedulePathDescription> parseMasterScheduleImpl(string scheduleKey, string rawData, List<string> visited)
	{
		if (visited.Contains<string>(scheduleKey, StringComparer.OrdinalIgnoreCase))
		{
			Game1.log.Warn($"NPC {base.Name} can't load schedules because they led to an infinite loop ({string.Join(" -> ", visited)} -> {scheduleKey}).");
			return new Dictionary<int, SchedulePathDescription>();
		}
		visited.Add(scheduleKey);
		try
		{
			string[] split = NPC.SplitScheduleCommands(rawData);
			Dictionary<int, SchedulePathDescription> oneDaySchedule = new Dictionary<int, SchedulePathDescription>();
			int routesToSkip = 0;
			if (split[0].Contains("GOTO"))
			{
				string newKey = ArgUtility.SplitBySpaceAndGet(split[0], 1);
				Dictionary<string, string> allSchedules = this.getMasterScheduleRawData();
				if (newKey.EqualsIgnoreCase("season"))
				{
					newKey = Game1.currentSeason;
					if (!allSchedules.ContainsKey(newKey))
					{
						newKey = "spring";
					}
				}
				try
				{
					if (allSchedules.TryGetValue(newKey, out var newScript))
					{
						return this.parseMasterScheduleImpl(newKey, newScript, visited);
					}
					Game1.log.Error($"Failed to load schedule '{scheduleKey}' for NPC '{base.Name}': GOTO references schedule '{newKey}' which doesn't exist. Falling back to 'spring'.");
				}
				catch (Exception exception)
				{
					Game1.log.Error($"Failed to load schedule '{scheduleKey}' for NPC '{base.Name}': GOTO references schedule '{newKey}' which couldn't be parsed. Falling back to 'spring'.", exception);
				}
				return this.parseMasterScheduleImpl("spring", this.getMasterScheduleEntry("spring"), visited);
			}
			if (split[0].Contains("NOT"))
			{
				string[] commandSplit = ArgUtility.SplitBySpace(split[0]);
				if (commandSplit[1].ToLower() == "friendship")
				{
					int index = 2;
					bool conditionMet = false;
					for (; index < commandSplit.Length; index += 2)
					{
						string who = commandSplit[index];
						if (int.TryParse(commandSplit[index + 1], out var level))
						{
							foreach (Farmer allFarmer in Game1.getAllFarmers())
							{
								if (allFarmer.getFriendshipHeartLevelForNPC(who) >= level)
								{
									conditionMet = true;
									break;
								}
							}
						}
						if (conditionMet)
						{
							break;
						}
					}
					if (conditionMet)
					{
						return this.parseMasterScheduleImpl("spring", this.getMasterScheduleEntry("spring"), visited);
					}
					routesToSkip++;
				}
			}
			else if (split[0].Contains("MAIL"))
			{
				string mailID = ArgUtility.SplitBySpace(split[0])[1];
				routesToSkip = ((!Game1.MasterPlayer.mailReceived.Contains(mailID) && !NetWorldState.checkAnywhereForWorldStateID(mailID)) ? (routesToSkip + 1) : (routesToSkip + 2));
			}
			if (split[routesToSkip].Contains("GOTO"))
			{
				string newKey2 = ArgUtility.SplitBySpaceAndGet(split[routesToSkip], 1);
				string text = newKey2.ToLower();
				if (!(text == "season"))
				{
					if (text == "no_schedule")
					{
						this.followSchedule = false;
						return null;
					}
				}
				else
				{
					newKey2 = Game1.currentSeason;
				}
				return this.parseMasterScheduleImpl(newKey2, this.getMasterScheduleEntry(newKey2), visited);
			}
			Point previousPosition = (this.isMarried() ? new Point(10, 23) : new Point((int)this.defaultPosition.X / 64, (int)this.defaultPosition.Y / 64));
			string previousGameLocation = (this.isMarried() ? "BusStop" : this.defaultMap.Value);
			int previousTime = 610;
			string default_map = this.DefaultMap;
			int default_x = (int)(this.defaultPosition.X / 64f);
			int default_y = (int)(this.defaultPosition.Y / 64f);
			bool default_map_dirty = false;
			for (int i = routesToSkip; i < split.Length; i++)
			{
				int index2 = 0;
				string[] newDestinationDescription = ArgUtility.SplitBySpace(split[i]);
				bool time_is_arrival_time = false;
				string time_string = newDestinationDescription[index2];
				if (time_string.Length > 0 && newDestinationDescription[index2][0] == 'a')
				{
					time_is_arrival_time = true;
					time_string = time_string.Substring(1);
				}
				int time = Convert.ToInt32(time_string);
				index2++;
				string location = newDestinationDescription[index2];
				string endOfRouteAnimation = null;
				string endOfRouteMessage = null;
				int xLocation = 0;
				int yLocation = 0;
				int localFacingDirection = 2;
				if (location == "bed")
				{
					if (this.isMarried())
					{
						location = "BusStop";
						xLocation = 9;
						yLocation = 23;
						localFacingDirection = 3;
					}
					else
					{
						string default_schedule = null;
						if (this.hasMasterScheduleEntry("default"))
						{
							default_schedule = this.getMasterScheduleEntry("default");
						}
						else if (this.hasMasterScheduleEntry("spring"))
						{
							default_schedule = this.getMasterScheduleEntry("spring");
						}
						if (default_schedule != null)
						{
							try
							{
								string[] last_schedule_split = ArgUtility.SplitBySpace(NPC.SplitScheduleCommands(default_schedule)[^1]);
								location = last_schedule_split[1];
								if (last_schedule_split.Length > 3)
								{
									if (!int.TryParse(last_schedule_split[2], out xLocation) || !int.TryParse(last_schedule_split[3], out yLocation))
									{
										default_schedule = null;
									}
								}
								else
								{
									default_schedule = null;
								}
							}
							catch (Exception)
							{
								default_schedule = null;
							}
						}
						if (default_schedule == null)
						{
							location = default_map;
							xLocation = default_x;
							yLocation = default_y;
						}
					}
					index2++;
					Dictionary<string, string> dictionary = DataLoader.AnimationDescriptions(Game1.content);
					string sleep_behavior = base.name.Value.ToLower() + "_sleep";
					if (dictionary.ContainsKey(sleep_behavior))
					{
						endOfRouteAnimation = sleep_behavior;
					}
				}
				else
				{
					if (int.TryParse(location, out var _))
					{
						location = previousGameLocation;
						index2--;
					}
					index2++;
					xLocation = Convert.ToInt32(newDestinationDescription[index2]);
					index2++;
					yLocation = Convert.ToInt32(newDestinationDescription[index2]);
					index2++;
					try
					{
						if (newDestinationDescription.Length > index2)
						{
							if (int.TryParse(newDestinationDescription[index2], out localFacingDirection))
							{
								index2++;
							}
							else
							{
								localFacingDirection = 2;
							}
						}
					}
					catch (Exception)
					{
						localFacingDirection = 2;
					}
				}
				if (this.changeScheduleForLocationAccessibility(ref location, ref xLocation, ref yLocation, ref localFacingDirection))
				{
					string newKey3 = (this.getMasterScheduleRawData().ContainsKey("default") ? "default" : "spring");
					return this.parseMasterScheduleImpl(newKey3, this.getMasterScheduleEntry(newKey3), visited);
				}
				if (index2 < newDestinationDescription.Length)
				{
					if (newDestinationDescription[index2].Length > 0 && newDestinationDescription[index2][0] == '"')
					{
						endOfRouteMessage = split[i].Substring(split[i].IndexOf('"'));
					}
					else
					{
						endOfRouteAnimation = newDestinationDescription[index2];
						index2++;
						if (index2 < newDestinationDescription.Length && newDestinationDescription[index2].Length > 0 && newDestinationDescription[index2][0] == '"')
						{
							endOfRouteMessage = split[i].Substring(split[i].IndexOf('"')).Replace("\"", "");
						}
					}
				}
				if (time == 0)
				{
					default_map_dirty = true;
					default_map = location;
					default_x = xLocation;
					default_y = yLocation;
					previousGameLocation = location;
					previousPosition.X = xLocation;
					previousPosition.Y = yLocation;
					this.faceDirection(localFacingDirection);
					this.previousEndPoint = new Point(xLocation, yLocation);
					continue;
				}
				SchedulePathDescription path_description = this.pathfindToNextScheduleLocation(scheduleKey, previousGameLocation, previousPosition.X, previousPosition.Y, location, xLocation, yLocation, localFacingDirection, endOfRouteAnimation, endOfRouteMessage);
				if (time_is_arrival_time)
				{
					int distance_traveled = 0;
					Point? last_point = null;
					foreach (Point point in path_description.route)
					{
						if (!last_point.HasValue)
						{
							last_point = point;
							continue;
						}
						if (Math.Abs(last_point.Value.X - point.X) + Math.Abs(last_point.Value.Y - point.Y) == 1)
						{
							distance_traveled += 64;
						}
						last_point = point;
					}
					int num = distance_traveled / 2;
					int ticks_per_ten_minutes = Game1.realMilliSecondsPerGameTenMinutes / 1000 * 60;
					int travel_time = (int)Math.Round((float)num / (float)ticks_per_ten_minutes) * 10;
					time = Math.Max(Utility.ConvertMinutesToTime(Utility.ConvertTimeToMinutes(time) - travel_time), previousTime);
				}
				path_description.time = time;
				oneDaySchedule.Add(time, path_description);
				previousPosition.X = xLocation;
				previousPosition.Y = yLocation;
				previousGameLocation = location;
				previousTime = time;
			}
			if (Game1.IsMasterGame && default_map_dirty)
			{
				Game1.warpCharacter(this, default_map, new Point(default_x, default_y));
			}
			return oneDaySchedule;
		}
		catch (Exception exception2)
		{
			Game1.log.Error($"NPC '{base.Name}' failed to parse master schedule '{scheduleKey}' with raw data '{rawData}'.", exception2);
			return new Dictionary<int, SchedulePathDescription>();
		}
	}

	/// <summary>Split a raw schedule script into its component commands.</summary>
	/// <param name="rawScript">The raw schedule script to split.</param>
	public static string[] SplitScheduleCommands(string rawScript)
	{
		return LegacyShims.SplitAndTrim(rawScript, '/', StringSplitOptions.RemoveEmptyEntries);
	}

	/// <summary>Try to load a schedule that applies today, or disable the schedule if none is found.</summary>
	/// <returns>Returns whether a schedule was successfully loaded.</returns>
	public bool TryLoadSchedule()
	{
		string season = Game1.currentSeason;
		int day = Game1.dayOfMonth;
		string dayName = Game1.shortDayNameFromDayOfSeason(day);
		int heartLevel = Math.Max(0, Utility.GetAllPlayerFriendshipLevel(this)) / 250;
		if (this.getMasterScheduleRawData() == null)
		{
			this.ClearSchedule();
			return false;
		}
		if (Game1.isGreenRain && Game1.year == 1 && this.TryLoadSchedule("GreenRain"))
		{
			return true;
		}
		if (!string.IsNullOrWhiteSpace(this.islandScheduleName.Value))
		{
			this.TryLoadSchedule(this.islandScheduleName.Value, this.Schedule);
			return true;
		}
		foreach (string festivalId in Game1.netWorldState.Value.ActivePassiveFestivals)
		{
			int dayOfPassiveFestival = Utility.GetDayOfPassiveFestival(festivalId);
			if (this.isMarried())
			{
				if (this.TryLoadSchedule("marriage_" + festivalId + "_" + dayOfPassiveFestival))
				{
					return true;
				}
				if (this.TryLoadSchedule("marriage_" + festivalId))
				{
					return true;
				}
			}
			else
			{
				if (this.TryLoadSchedule(festivalId + "_" + dayOfPassiveFestival))
				{
					return true;
				}
				if (this.TryLoadSchedule(festivalId))
				{
					return true;
				}
			}
		}
		if (this.isMarried())
		{
			if (this.TryLoadSchedule("marriage_" + season + "_" + day))
			{
				return true;
			}
			if (base.Name == "Penny")
			{
				switch (dayName)
				{
				case "Tue":
				case "Wed":
				case "Fri":
					goto IL_0205;
				}
			}
			if ((base.Name == "Maru" && (dayName == "Tue" || dayName == "Thu")) || (base.Name == "Harvey" && (dayName == "Tue" || dayName == "Thu")))
			{
				goto IL_0205;
			}
			goto IL_0214;
		}
		if (this.TryLoadSchedule(season + "_" + day))
		{
			return true;
		}
		for (int tryHearts = heartLevel; tryHearts > 0; tryHearts--)
		{
			if (this.TryLoadSchedule(day + "_" + tryHearts))
			{
				return true;
			}
		}
		if (this.TryLoadSchedule(day.ToString()))
		{
			return true;
		}
		if (base.Name == "Pam" && Game1.player.mailReceived.Contains("ccVault") && this.TryLoadSchedule("bus"))
		{
			return true;
		}
		if (base.currentLocation?.IsRainingHere() ?? false)
		{
			if (Game1.random.NextBool() && this.TryLoadSchedule("rain2"))
			{
				return true;
			}
			if (this.TryLoadSchedule("rain"))
			{
				return true;
			}
		}
		int tryHearts2;
		for (tryHearts2 = heartLevel; tryHearts2 > 0; tryHearts2--)
		{
			if (this.TryLoadSchedule(season + "_" + dayName + "_" + tryHearts2))
			{
				return true;
			}
			tryHearts2--;
		}
		if (this.TryLoadSchedule(season + "_" + dayName))
		{
			return true;
		}
		int tryHearts3;
		for (tryHearts3 = heartLevel; tryHearts3 > 0; tryHearts3--)
		{
			if (this.TryLoadSchedule(dayName + "_" + tryHearts3))
			{
				return true;
			}
			tryHearts3--;
		}
		if (this.TryLoadSchedule(dayName))
		{
			return true;
		}
		if (this.TryLoadSchedule(season))
		{
			return true;
		}
		if (this.TryLoadSchedule("spring_" + dayName))
		{
			return true;
		}
		if (this.TryLoadSchedule("spring"))
		{
			return true;
		}
		this.ClearSchedule();
		return false;
		IL_0205:
		if (this.TryLoadSchedule("marriageJob"))
		{
			return true;
		}
		goto IL_0214;
		IL_0214:
		if (!Game1.isRaining && this.TryLoadSchedule("marriage_" + dayName))
		{
			return true;
		}
		this.ClearSchedule();
		return false;
	}

	/// <summary>Try to load a schedule matching the the given key, or disable the schedule if it's missing or invalid.</summary>
	/// <param name="key">The key for the schedule to load.</param>
	/// <returns>Returns whether the schedule was successfully loaded.</returns>
	public bool TryLoadSchedule(string key)
	{
		try
		{
			if (this.hasMasterScheduleEntry(key))
			{
				this.TryLoadSchedule(key, this.parseMasterSchedule(key, this.getMasterScheduleEntry(key)));
				return true;
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error($"Failed to load schedule key '{key}' for NPC '{base.Name}'.", exception);
		}
		this.ClearSchedule();
		return false;
	}

	/// <summary>Try to load a raw schedule script, or disable the schedule if it's invalid.</summary>
	/// <param name="key">The schedule's key in the data asset.</param>
	/// <param name="rawSchedule">The schedule script to load.</param>
	public bool TryLoadSchedule(string key, string rawSchedule)
	{
		Dictionary<int, SchedulePathDescription> schedule;
		try
		{
			schedule = this.parseMasterSchedule(key, rawSchedule);
		}
		catch (Exception exception)
		{
			Game1.log.Error($"Failed to load schedule key '{key}' from raw string for NPC '{base.Name}'.", exception);
			this.ClearSchedule();
			return false;
		}
		return this.TryLoadSchedule(key, schedule);
	}

	/// <summary>Try to load raw schedule data, or disable the schedule if it's invalid.</summary>
	/// <param name="key">The schedule's key in the data asset.</param>
	/// <param name="schedule">The schedule data to load.</param>
	public bool TryLoadSchedule(string key, Dictionary<int, SchedulePathDescription> schedule)
	{
		if (schedule == null)
		{
			this.ClearSchedule();
			return false;
		}
		this.Schedule = schedule;
		if (Game1.IsMasterGame)
		{
			this.dayScheduleName.Value = key;
		}
		this.followSchedule = true;
		return true;
	}

	/// <summary>Disable the schedule for today.</summary>
	public void ClearSchedule()
	{
		this.Schedule = null;
		if (Game1.IsMasterGame)
		{
			this.dayScheduleName.Value = null;
		}
		this.followSchedule = false;
	}

	public virtual void handleMasterScheduleFileLoadError(Exception e)
	{
		Game1.log.Error("NPC '" + base.Name + "' failed loading schedule file.", e);
	}

	public virtual void InvalidateMasterSchedule()
	{
		this._hasLoadedMasterScheduleData = false;
	}

	public Dictionary<string, string> getMasterScheduleRawData()
	{
		if (!this._hasLoadedMasterScheduleData)
		{
			this._hasLoadedMasterScheduleData = true;
			string assetName = "Characters\\schedules\\" + base.Name;
			if (base.Name == "Leo" && this.DefaultMap != "IslandHut")
			{
				assetName += "Mainland";
			}
			try
			{
				if (Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
				{
					this._masterScheduleData = Game1.content.Load<Dictionary<string, string>>(assetName);
					this._masterScheduleData = new Dictionary<string, string>(this._masterScheduleData, StringComparer.OrdinalIgnoreCase);
				}
			}
			catch (Exception e)
			{
				this.handleMasterScheduleFileLoadError(e);
			}
		}
		return this._masterScheduleData;
	}

	public string getMasterScheduleEntry(string schedule_key)
	{
		if (this.getMasterScheduleRawData() == null)
		{
			throw new KeyNotFoundException("The schedule file for NPC '" + base.Name + "' could not be loaded...");
		}
		if (this._masterScheduleData.TryGetValue(schedule_key, out var data))
		{
			return data;
		}
		throw new KeyNotFoundException($"The schedule file for NPC '{base.Name}' has no schedule named '{schedule_key}'.");
	}

	public bool hasMasterScheduleEntry(string key)
	{
		if (this.getMasterScheduleRawData() == null)
		{
			return false;
		}
		return this.getMasterScheduleRawData().ContainsKey(key);
	}

	public virtual bool isRoommate()
	{
		if (!this.IsVillager)
		{
			return false;
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name && !f.isEngaged() && f.isRoommate(base.Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool isMarried()
	{
		if (!this.IsVillager)
		{
			return false;
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name && !f.isEngaged())
			{
				return true;
			}
		}
		return false;
	}

	public bool isMarriedOrEngaged()
	{
		if (!this.IsVillager)
		{
			return false;
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Update the NPC state when setting up the new day, before the game saves overnight.</summary>
	/// <param name="dayOfMonth">The current day of month.</param>
	/// <remarks>See also <see cref="M:StardewValley.NPC.OnDayStarted" />, which happens after saving when the day has started.</remarks>
	public virtual void dayUpdate(int dayOfMonth)
	{
		bool villager = this.IsVillager;
		this.isMovingOnPathFindPath.Value = false;
		this.queuedSchedulePaths.Clear();
		this.lastAttemptedSchedule = -1;
		base.drawOffset = Vector2.Zero;
		this.appliedRouteAnimationOffset = Vector2.Zero;
		this.shouldWearIslandAttire.Value = false;
		if (this.layingDown)
		{
			this.layingDown = false;
			this.HideShadow = false;
		}
		if (this.isWearingIslandAttire)
		{
			this.wearNormalClothes();
		}
		if (base.currentLocation != null && this.defaultMap.Value != null)
		{
			try
			{
				Game1.warpCharacter(this, this.defaultMap.Value, this.defaultPosition.Value / 64f);
			}
			catch (Exception exception)
			{
				Game1.log.Error($"NPC '{base.Name}' failed to warp home to '{this.defaultMap}' overnight.", exception);
			}
		}
		if (villager)
		{
			string text = base.Name;
			if (!(text == "Willy"))
			{
				if (text == "Elliott" && Game1.IsMasterGame && Game1.netWorldState.Value.hasWorldStateID("elliottGone"))
				{
					this.daysUntilNotInvisible = 7;
					Game1.netWorldState.Value.removeWorldStateID("elliottGone");
					Game1.worldStateIDs.Remove("elliottGone");
				}
			}
			else
			{
				this.IsInvisible = false;
			}
		}
		this.UpdateInvisibilityOnNewDay();
		this.resetForNewDay(dayOfMonth);
		this.ChooseAppearance();
		if (villager)
		{
			this.updateConstructionAnimation();
		}
		this.clearTextAboveHead();
	}

	/// <summary>Handle the new day starting after the player saves, loads, or connects.</summary>
	/// <remarks>See also <see cref="M:StardewValley.NPC.dayUpdate(System.Int32)" />, which happens while setting up the day before saving.</remarks>
	public void OnDayStarted()
	{
		if (Game1.IsMasterGame && this.isMarried() && !this.getSpouse().divorceTonight.Value && !this.IsInvisible)
		{
			this.marriageDuties();
		}
	}

	protected void UpdateInvisibilityOnNewDay()
	{
		if (Game1.IsMasterGame && (this.IsInvisible || this.daysUntilNotInvisible > 0))
		{
			this.daysUntilNotInvisible--;
			this.IsInvisible = this.daysUntilNotInvisible > 0;
			if (!this.IsInvisible)
			{
				this.daysUntilNotInvisible = 0;
			}
		}
	}

	public virtual void resetForNewDay(int dayOfMonth)
	{
		this.sleptInBed.Value = true;
		if (this.isMarried() && !this.isRoommate())
		{
			FarmHouse house = Utility.getHomeOfFarmer(this.getSpouse());
			if (house != null && house.GetSpouseBed() == null)
			{
				this.sleptInBed.Value = false;
			}
		}
		if (this.doingEndOfRouteAnimation.Value)
		{
			this.routeEndAnimationFinished(null);
		}
		this.Halt();
		this.wasKissedYesterday = this.hasBeenKissedToday.Value;
		this.hasBeenKissedToday.Value = false;
		this.currentMarriageDialogue.Clear();
		this.marriageDefaultDialogue.Value = null;
		this.shouldSayMarriageDialogue.Value = false;
		this.isSleeping.Value = false;
		base.drawOffset = Vector2.Zero;
		base.faceTowardFarmer = false;
		base.faceTowardFarmerTimer = 0;
		base.drawOffset = Vector2.Zero;
		this.hasSaidAfternoonDialogue.Value = false;
		this.isPlayingSleepingAnimation = false;
		this.ignoreScheduleToday = false;
		this.Halt();
		base.controller = null;
		this.temporaryController = null;
		this.directionsToNewLocation = null;
		this.faceDirection(this.DefaultFacingDirection);
		this.Sprite.oldFrame = this.Sprite.CurrentFrame;
		this.previousEndPoint = new Point((int)this.defaultPosition.X / 64, (int)this.defaultPosition.Y / 64);
		this.isWalkingInSquare = false;
		this.returningToEndPoint = false;
		this.lastCrossroad = Microsoft.Xna.Framework.Rectangle.Empty;
		this._startedEndOfRouteBehavior = null;
		this._finishingEndOfRouteBehavior = null;
		this.loadedEndOfRouteBehavior = null;
		this._beforeEndOfRouteAnimationFrame = this.Sprite.CurrentFrame;
		if (this.IsVillager)
		{
			if (base.Name == "Willy" && Game1.stats.DaysPlayed < 2)
			{
				this.IsInvisible = true;
				this.daysUntilNotInvisible = 1;
			}
			this.TryLoadSchedule();
			this.performSpecialScheduleChanges();
		}
		this.endOfRouteMessage.Value = null;
	}

	public void returnHomeFromFarmPosition(Farm farm)
	{
		Farmer farmer = this.getSpouse();
		if (farmer != null)
		{
			FarmHouse farm_house = Utility.getHomeOfFarmer(farmer);
			Point porchPoint = farm_house.getPorchStandingSpot();
			if (base.TilePoint == porchPoint)
			{
				base.drawOffset = Vector2.Zero;
				string nameOfHome = this.getHome().NameOrUniqueName;
				base.willDestroyObjectsUnderfoot = true;
				Point destination = farm.getWarpPointTo(nameOfHome, this);
				base.controller = new PathFindController(this, farm, destination, 0)
				{
					NPCSchedule = true
				};
			}
			else if (!this.shouldPlaySpousePatioAnimation.Value || !farm.farmers.Any())
			{
				base.drawOffset = Vector2.Zero;
				this.Halt();
				base.controller = null;
				this.temporaryController = null;
				this.ignoreScheduleToday = true;
				Game1.warpCharacter(this, farm_house, Utility.PointToVector2(farm_house.getKitchenStandingSpot()));
			}
		}
	}

	public virtual Vector2 GetSpousePatioPosition()
	{
		return Utility.PointToVector2(Game1.getFarm().spousePatioSpot);
	}

	public void setUpForOutdoorPatioActivity()
	{
		Vector2 patio_location = this.GetSpousePatioPosition();
		if (!NPC.checkTileOccupancyForSpouse(Game1.getFarm(), patio_location))
		{
			Game1.warpCharacter(this, "Farm", patio_location);
			this.popOffAnyNonEssentialItems();
			this.currentMarriageDialogue.Clear();
			this.addMarriageDialogue("MarriageDialogue", "patio_" + base.Name, false);
			this.setTilePosition((int)patio_location.X, (int)patio_location.Y);
			this.shouldPlaySpousePatioAnimation.Value = true;
		}
	}

	private void doPlaySpousePatioAnimation()
	{
		CharacterSpousePatioData patioData = this.GetData()?.SpousePatio;
		if (patioData == null)
		{
			return;
		}
		List<int[]> frames = patioData.SpriteAnimationFrames;
		if (frames == null || frames.Count <= 0)
		{
			return;
		}
		base.drawOffset = Utility.PointToVector2(patioData.SpriteAnimationPixelOffset);
		this.Sprite.ClearAnimation();
		for (int i = 0; i < frames.Count; i++)
		{
			int[] frame = frames[i];
			if (frame != null && frame.Length != 0)
			{
				int index = frame[0];
				int duration = (ArgUtility.HasIndex(frame, 1) ? frame[1] : 100);
				this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(index, duration, 0, secondaryArm: false, flip: false));
			}
		}
	}

	/// <summary>Whether this character has dark skin for the purposes of child genetics.</summary>
	public virtual bool hasDarkSkin()
	{
		if (this.IsVillager)
		{
			return this.GetData()?.IsDarkSkinned ?? false;
		}
		return false;
	}

	/// <summary>Whether the player will need to adopt children with this spouse, instead of either the player or NPC giving birth.</summary>
	public bool isAdoptionSpouse()
	{
		Farmer spouse = this.getSpouse();
		if (spouse == null)
		{
			return false;
		}
		string isAdoptionSpouse = this.GetData()?.SpouseAdopts;
		if (isAdoptionSpouse != null)
		{
			return GameStateQuery.CheckConditions(isAdoptionSpouse, base.currentLocation, spouse);
		}
		return this.Gender == spouse.Gender;
	}

	public bool canGetPregnant()
	{
		if (this is Horse || base.Name.Equals("Krobus") || this.isRoommate() || this.IsInvisible)
		{
			return false;
		}
		Farmer spouse = this.getSpouse();
		if (spouse == null || spouse.divorceTonight.Value)
		{
			return false;
		}
		int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
		Friendship friendship = spouse.GetSpouseFriendship();
		List<Child> kids = spouse.getChildren();
		this.defaultMap.Value = spouse.homeLocation.Value;
		FarmHouse farmHouse = Utility.getHomeOfFarmer(spouse);
		if (farmHouse.cribStyle.Value <= 0)
		{
			return false;
		}
		if (farmHouse.upgradeLevel >= 2 && friendship.DaysUntilBirthing < 0 && heartsWithSpouse >= 10 && spouse.GetDaysMarried() >= 7)
		{
			if (kids.Count != 0)
			{
				if (kids.Count < 2)
				{
					return kids[0].Age > 2;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void marriageDuties()
	{
		Farmer spouse = this.getSpouse();
		if (spouse == null)
		{
			return;
		}
		this.shouldSayMarriageDialogue.Value = true;
		this.DefaultMap = spouse.homeLocation.Value;
		FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(spouse.homeLocation.Value);
		Random r = Utility.CreateDaySaveRandom(spouse.UniqueMultiplayerID);
		int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
		if (Game1.IsMasterGame && (base.currentLocation == null || !base.currentLocation.Equals(farmHouse)))
		{
			Game1.warpCharacter(this, spouse.homeLocation.Value, farmHouse.getSpouseBedSpot(base.Name));
		}
		if (Game1.isRaining)
		{
			this.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + r.Next(5), false);
		}
		else
		{
			this.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + r.Next(5), false);
		}
		this.currentMarriageDialogue.Add(new MarriageDialogueReference(this.marriageDefaultDialogue.Value.DialogueFile, this.marriageDefaultDialogue.Value.DialogueKey, this.marriageDefaultDialogue.Value.IsGendered, this.marriageDefaultDialogue.Value.Substitutions));
		if (spouse.GetSpouseFriendship().DaysUntilBirthing == 0)
		{
			this.setTilePosition(farmHouse.getKitchenStandingSpot());
			this.currentMarriageDialogue.Clear();
			return;
		}
		if (this.daysAfterLastBirth >= 0)
		{
			this.daysAfterLastBirth--;
			switch (this.getSpouse().getChildrenCount())
			{
			case 1:
				this.setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
				}
				return;
			case 2:
				this.setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse))
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
				}
				return;
			}
		}
		this.setTilePosition(farmHouse.getKitchenStandingSpot());
		if (!this.sleptInBed.Value)
		{
			this.currentMarriageDialogue.Clear();
			this.addMarriageDialogue("MarriageDialogue", "NoBed_" + r.Next(4), false);
			return;
		}
		if (this.tryToGetMarriageSpecificDialogue(Game1.currentSeason + "_" + Game1.dayOfMonth) != null)
		{
			if (spouse != null)
			{
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + Game1.dayOfMonth, false);
			}
			return;
		}
		if (this.Schedule != null)
		{
			if (this.ScheduleKey == "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
			{
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", "funLeave_" + base.Name, false);
			}
			else if (this.ScheduleKey == "marriageJob")
			{
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", "jobLeave_" + base.Name, false);
			}
			return;
		}
		if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && spouse == Game1.MasterPlayer && !base.Name.Equals("Krobus"))
		{
			this.setUpForOutdoorPatioActivity();
			return;
		}
		int minHeartLevelForNegativeDialogue = 12;
		if (Game1.Date.TotalDays - spouse.GetSpouseFriendship().LastGiftDate?.TotalDays <= 1)
		{
			minHeartLevelForNegativeDialogue--;
		}
		if (this.wasKissedYesterday)
		{
			minHeartLevelForNegativeDialogue--;
		}
		if (spouse.GetDaysMarried() > 7 && r.NextDouble() < (double)(1f - (float)Math.Max(1, heartsWithSpouse) / (float)minHeartLevelForNegativeDialogue))
		{
			Furniture f = farmHouse.getRandomFurniture(r);
			if (f != null && f.isGroundFurniture() && f.furniture_type.Value != 15 && f.furniture_type.Value != 12)
			{
				Point p = new Point((int)f.tileLocation.X - 1, (int)f.tileLocation.Y);
				if (farmHouse.CanItemBePlacedHere(new Vector2(p.X, p.Y)))
				{
					this.setTilePosition(p);
					this.faceDirection(1);
					switch (r.Next(10))
					{
					case 0:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4420", false);
						break;
					case 1:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4421", false);
						break;
					case 2:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4422", true);
						break;
					case 3:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4423", false);
						break;
					case 4:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4424", false);
						break;
					case 5:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4425", false);
						break;
					case 6:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4426", false);
						break;
					case 7:
						if (this.Gender == Gender.Female)
						{
							this.currentMarriageDialogue.Clear();
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", r.Choose("NPC.cs.4427", "NPC.cs.4429"), false);
						}
						else
						{
							this.currentMarriageDialogue.Clear();
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4431", false);
						}
						break;
					case 8:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4432", false);
						break;
					case 9:
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4433", false);
						break;
					}
					return;
				}
			}
			this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false), farmHouse, force: true);
			return;
		}
		Friendship friendship = spouse.GetSpouseFriendship();
		if (friendship.DaysUntilBirthing != -1 && friendship.DaysUntilBirthing <= 7 && r.NextBool())
		{
			if (this.isAdoptionSpouse())
			{
				this.setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4439", false), farmHouse))
				{
					if (r.NextBool())
					{
						this.currentMarriageDialogue.Clear();
					}
					if (r.NextBool())
					{
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4440", false, this.getSpouse().displayName);
					}
					else
					{
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4441", false, "%endearment");
					}
				}
				return;
			}
			if (this.Gender == Gender.Female)
			{
				this.setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!this.spouseObstacleCheck(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4442", false) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4443", false), farmHouse))
				{
					if (r.NextBool())
					{
						this.currentMarriageDialogue.Clear();
					}
					this.currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4444", false, this.getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4445", false, "%endearment"));
				}
				return;
			}
			this.setTilePosition(farmHouse.getKitchenStandingSpot());
			if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true), farmHouse))
			{
				if (r.NextBool())
				{
					this.currentMarriageDialogue.Clear();
				}
				this.currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, this.getSpouse().displayName) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, "%endearment"));
			}
			return;
		}
		if (r.NextDouble() < 0.07)
		{
			switch (this.getSpouse().getChildrenCount())
			{
			case 1:
				this.setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4449", true), farmHouse))
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false);
				}
				return;
			case 2:
				this.setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4452", true), farmHouse))
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false);
				}
				return;
			}
		}
		Farm farm = Game1.getFarm();
		if (this.currentMarriageDialogue.Count > 0 && this.currentMarriageDialogue[0].IsItemGrabDialogue(this))
		{
			this.setTilePosition(farmHouse.getKitchenStandingSpot());
			this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4455", true), farmHouse);
		}
		else if (!Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())) && !base.Name.Equals("Krobus"))
		{
			bool filledBowl = false;
			if (!NPC.hasSomeoneFedThePet)
			{
				foreach (Building building in farm.buildings)
				{
					if (building is PetBowl bowl && !bowl.watered.Value)
					{
						filledBowl = true;
						bowl.watered.Value = true;
						NPC.hasSomeoneFedThePet = true;
					}
				}
			}
			if (r.NextDouble() < 0.6 && Game1.season != Season.Winter && !NPC.hasSomeoneWateredCrops)
			{
				Vector2 origin = Vector2.Zero;
				int tries = 0;
				bool foundWatered = false;
				for (; tries < Math.Min(50, farm.terrainFeatures.Length); tries++)
				{
					if (!origin.Equals(Vector2.Zero))
					{
						break;
					}
					if (Utility.TryGetRandom(farm.terrainFeatures, out var tile, out var feature) && feature is HoeDirt dirt && dirt.needsWatering())
					{
						if (!dirt.isWatered())
						{
							origin = tile;
						}
						else
						{
							foundWatered = true;
						}
					}
				}
				if (!origin.Equals(Vector2.Zero))
				{
					foreach (Vector2 currentPosition in new Microsoft.Xna.Framework.Rectangle((int)origin.X - 30, (int)origin.Y - 30, 60, 60).GetVectors())
					{
						if (farm.isTileOnMap(currentPosition) && farm.terrainFeatures.TryGetValue(currentPosition, out var terrainFeature) && terrainFeature is HoeDirt dirt2 && Game1.IsMasterGame && dirt2.needsWatering())
						{
							dirt2.state.Value = 1;
						}
					}
					this.faceDirection(2);
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
					if (filledBowl)
					{
						if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
						{
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
						}
						else
						{
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
					}
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					NPC.hasSomeoneWateredCrops = true;
				}
				else
				{
					this.faceDirection(2);
					if (foundWatered)
					{
						this.currentMarriageDialogue.Clear();
						if (Game1.gameMode == 6)
						{
							if (r.NextBool())
							{
								this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4465", false, "%endearment");
							}
							else
							{
								this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4466", false, "%endearment");
								this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true);
								if (filledBowl)
								{
									if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
									{
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
									}
									else
									{
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
									}
								}
							}
						}
						else
						{
							this.currentMarriageDialogue.Clear();
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4470", true);
						}
					}
					else
					{
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					}
				}
			}
			else if (r.NextDouble() < 0.6 && !NPC.hasSomeoneFedTheAnimals)
			{
				bool fedAnything = false;
				foreach (Building b in farm.buildings)
				{
					if (b.GetIndoors() is AnimalHouse animalHouse && b.daysOfConstructionLeft.Value <= 0 && Game1.IsMasterGame)
					{
						animalHouse.feedAllAnimals();
						fedAnything = true;
					}
				}
				this.faceDirection(2);
				if (fedAnything)
				{
					NPC.hasSomeoneFedTheAnimals = true;
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4474", true);
					if (filledBowl)
					{
						if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
						{
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
						}
						else
						{
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
					}
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
				else
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
				if (Game1.IsMasterGame)
				{
					foreach (Building building2 in farm.buildings)
					{
						if (building2 is PetBowl bowl2 && !bowl2.watered.Value)
						{
							filledBowl = true;
							bowl2.watered.Value = true;
							NPC.hasSomeoneFedThePet = true;
						}
					}
				}
			}
			else if (!NPC.hasSomeoneRepairedTheFences)
			{
				int i = 0;
				this.faceDirection(2);
				Vector2 origin2 = Vector2.Zero;
				for (; i < Math.Min(50, farm.objects.Length); i++)
				{
					if (!origin2.Equals(Vector2.Zero))
					{
						break;
					}
					if (Utility.TryGetRandom(farm.objects, out var tile2, out var obj) && obj is Fence)
					{
						origin2 = tile2;
					}
				}
				if (!origin2.Equals(Vector2.Zero))
				{
					foreach (Vector2 currentPosition2 in new Microsoft.Xna.Framework.Rectangle((int)origin2.X - 10, (int)origin2.Y - 10, 20, 20).GetVectors())
					{
						if (farm.isTileOnMap(currentPosition2) && farm.objects.TryGetValue(currentPosition2, out var obj2) && obj2 is Fence fence && Game1.IsMasterGame)
						{
							fence.repair();
						}
					}
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4481", true);
					if (filledBowl)
					{
						if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
						{
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, Game1.player.getPetDisplayName());
						}
						else
						{
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, Game1.player.getPetDisplayName());
						}
					}
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
					NPC.hasSomeoneRepairedTheFences = true;
				}
				else
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
				}
			}
			Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
			this.popOffAnyNonEssentialItems();
			this.faceDirection(2);
		}
		else if (base.Name.Equals("Krobus") && Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot())))
		{
			this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false);
			Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
			this.popOffAnyNonEssentialItems();
			this.faceDirection(2);
		}
		else if (spouse.GetDaysMarried() >= 1 && r.NextDouble() < 0.045)
		{
			if (r.NextDouble() < 0.75)
			{
				Point spot = farmHouse.getRandomOpenPointInHouse(r, 1);
				Furniture new_furniture;
				try
				{
					new_furniture = ItemRegistry.Create<Furniture>(Utility.getRandomSingleTileFurniture(r)).SetPlacement(spot);
				}
				catch
				{
					new_furniture = null;
				}
				if (new_furniture != null && spot.X > 0 && farmHouse.CanItemBePlacedHere(new Vector2(spot.X - 1, spot.Y)))
				{
					farmHouse.furniture.Add(new_furniture);
					this.setTilePosition(spot.X - 1, spot.Y);
					this.faceDirection(1);
					this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4486", false, "%endearmentlower");
					if (Game1.random.NextBool())
					{
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4488", true);
					}
					else
					{
						this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4489", false);
					}
				}
				else
				{
					this.setTilePosition(farmHouse.getKitchenStandingSpot());
					this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4490", false), farmHouse);
				}
				return;
			}
			Point p2 = farmHouse.getRandomOpenPointInHouse(r);
			if (p2.X <= 0)
			{
				return;
			}
			this.setTilePosition(p2.X, p2.Y);
			this.faceDirection(0);
			if (r.NextBool())
			{
				string wall = farmHouse.GetWallpaperID(p2.X, p2.Y);
				if (wall != null)
				{
					string wallpaperId = r.ChooseFrom(this.GetData()?.SpouseWallpapers) ?? r.Next(112).ToString();
					farmHouse.SetWallpaper(wallpaperId, wall);
					this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4496", false);
				}
			}
			else
			{
				string floor = farmHouse.getFloorRoomIdAt(p2);
				if (floor != null)
				{
					string floorId = r.ChooseFrom(this.GetData()?.SpouseFloors) ?? r.Next(40).ToString();
					farmHouse.SetFloor(floorId, floor);
					this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4497", false);
				}
			}
		}
		else if (Game1.isRaining && r.NextDouble() < 0.08 && heartsWithSpouse < 11 && spouse.GetDaysMarried() > 7 && base.Name != "Krobus")
		{
			foreach (Furniture f2 in farmHouse.furniture)
			{
				if (f2.furniture_type.Value == 13 && farmHouse.CanItemBePlacedHere(new Vector2((int)f2.tileLocation.X, (int)f2.tileLocation.Y + 1)))
				{
					this.setTilePosition((int)f2.tileLocation.X, (int)f2.tileLocation.Y + 1);
					this.faceDirection(0);
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4498", true);
					return;
				}
			}
			this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4499", false), farmHouse, force: true);
		}
		else if (r.NextDouble() < 0.45)
		{
			Vector2 spot2 = Utility.PointToVector2(farmHouse.GetSpouseRoomSpot());
			this.setTilePosition((int)spot2.X, (int)spot2.Y);
			this.faceDirection(0);
			this.setSpouseRoomMarriageDialogue();
			if (base.name.Value == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
			{
				Point frog_spot = farmHouse.GetSpouseRoomCorner();
				frog_spot.X += 2;
				frog_spot.Y += 5;
				this.setTilePosition(frog_spot);
				this.faceDirection(2);
			}
		}
		else
		{
			this.setTilePosition(farmHouse.getKitchenStandingSpot());
			this.faceDirection(0);
			if (r.NextDouble() < 0.2)
			{
				this.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse);
			}
		}
	}

	public virtual void popOffAnyNonEssentialItems()
	{
		if (Game1.IsMasterGame && base.currentLocation != null)
		{
			Point tile = base.TilePoint;
			Object tile_object = base.currentLocation.getObjectAtTile(tile.X, tile.Y);
			if (tile_object != null && (tile_object.QualifiedItemId == "(O)93" || tile_object is Torch))
			{
				Vector2 tile_position = tile_object.TileLocation;
				tile_object.performRemoveAction();
				base.currentLocation.objects.Remove(tile_position);
				tile_object.dropItem(base.currentLocation, tile_position * 64f, tile_position * 64f);
			}
		}
	}

	public static bool checkTileOccupancyForSpouse(GameLocation location, Vector2 point, string characterToIgnore = "")
	{
		return location?.IsTileOccupiedBy(point, ~(CollisionMask.Characters | CollisionMask.Farmers), CollisionMask.All) ?? true;
	}

	public void addMarriageDialogue(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
	{
		this.shouldSayMarriageDialogue.Value = true;
		this.currentMarriageDialogue.Add(new MarriageDialogueReference(dialogue_file, dialogue_key, gendered, substitutions));
	}

	public void clearTextAboveHead()
	{
		this.textAboveHead = null;
		this.textAboveHeadPreTimer = -1;
		this.textAboveHeadTimer = -1;
	}

	/// <summary>Get whether this is a villager NPC, regardless of whether they're present in <c>Data/Characters</c>.</summary>
	[Obsolete("Use IsVillager instead.")]
	public bool isVillager()
	{
		return this.IsVillager;
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		if (this.isMarried() && (this.Schedule == null || location is FarmHouse))
		{
			return true;
		}
		return base.shouldCollideWithBuildingLayer(location);
	}

	public virtual void arriveAtFarmHouse(FarmHouse farmHouse)
	{
		if (Game1.newDay || !this.isMarried() || Game1.timeOfDay <= 630 || !(base.TilePoint != farmHouse.getSpouseBedSpot(base.name.Value)))
		{
			return;
		}
		this.setTilePosition(farmHouse.getEntryLocation());
		this.ignoreScheduleToday = true;
		this.temporaryController = null;
		base.controller = null;
		if (Game1.timeOfDay >= 2130)
		{
			Point bed_spot = farmHouse.getSpouseBedSpot(base.name.Value);
			bool found_bed = farmHouse.GetSpouseBed() != null;
			PathFindController.endBehavior end_behavior = null;
			if (found_bed)
			{
				end_behavior = FarmHouse.spouseSleepEndFunction;
			}
			base.controller = new PathFindController(this, farmHouse, bed_spot, 0, end_behavior);
			if (base.controller.pathToEndPoint != null && found_bed)
			{
				foreach (Furniture furniture in farmHouse.furniture)
				{
					if (furniture is BedFurniture bed && furniture.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(bed_spot.X * 64, bed_spot.Y * 64, 64, 64)))
					{
						bed.ReserveForNPC();
						break;
					}
				}
			}
		}
		else
		{
			base.controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
		}
		if (base.controller.pathToEndPoint == null)
		{
			base.willDestroyObjectsUnderfoot = true;
			base.controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
			this.setNewDialogue(this.TryGetDialogue("SpouseFarmhouseClutter") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4500", isGendered: true));
		}
		else if (Game1.timeOfDay > 1300)
		{
			if (this.ScheduleKey == "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
			{
				this.setNewDialogue("MarriageDialogue", "funReturn_", clearOnMovement: true);
			}
			else if (this.ScheduleKey == "marriageJob")
			{
				this.setNewDialogue("MarriageDialogue", "jobReturn_");
			}
			else if (Game1.timeOfDay < 1800)
			{
				this.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, base.currentLocation, countAsDailyAfternoon: true);
			}
		}
		if (Game1.currentLocation == farmHouse)
		{
			Game1.currentLocation.playSound("doorClose", null, null, SoundContext.NPC);
		}
	}

	public Farmer getSpouse()
	{
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.spouse != null && f.spouse == base.Name)
			{
				return f;
			}
		}
		return null;
	}

	public string getTermOfSpousalEndearment(bool happy = true)
	{
		Farmer spouse = this.getSpouse();
		if (spouse != null)
		{
			if (this.isRoommate())
			{
				return spouse.displayName;
			}
			if (spouse.getFriendshipHeartLevelForNPC(base.Name) < 9)
			{
				return spouse.displayName;
			}
			if (!happy)
			{
				return Game1.random.Next(2) switch
				{
					0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517"), 
					1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518"), 
					_ => spouse.displayName, 
				};
			}
			if (Game1.random.NextDouble() < 0.08)
			{
				switch (Game1.random.Next(8))
				{
				case 0:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4507");
				case 1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4508");
				case 2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4509");
				case 3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4510");
				case 4:
					if (!spouse.IsMale)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4512");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4511");
				case 5:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4513");
				case 6:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4514");
				default:
					if (!spouse.IsMale)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4516");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4515");
				}
			}
			return Game1.random.Next(5) switch
			{
				0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4519"), 
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4522"), 
				_ => Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4523"), 
			};
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
	}

	/// <summary>
	/// return true if spouse encountered obstacle.
	/// if force == true then the obstacle check will be ignored and spouse will absolutely be put into bed.
	/// </summary>
	/// <param name="backToBedMessage"></param>
	/// <param name="currentLocation"></param>
	/// <returns></returns>
	public bool spouseObstacleCheck(MarriageDialogueReference backToBedMessage, GameLocation currentLocation, bool force = false)
	{
		if (force || NPC.checkTileOccupancyForSpouse(currentLocation, base.Tile, base.Name))
		{
			Game1.warpCharacter(this, this.defaultMap.Value, Game1.RequireLocation<FarmHouse>(this.defaultMap.Value).getSpouseBedSpot(base.name.Value));
			this.faceDirection(1);
			this.currentMarriageDialogue.Clear();
			this.currentMarriageDialogue.Add(backToBedMessage);
			this.shouldSayMarriageDialogue.Value = true;
			return true;
		}
		return false;
	}

	public void setTilePosition(Point p)
	{
		this.setTilePosition(p.X, p.Y);
	}

	public void setTilePosition(int x, int y)
	{
		base.Position = new Vector2(x * 64, y * 64);
	}

	private void clintHammerSound(Farmer who)
	{
		base.currentLocation.playSound("hammer", base.Tile);
	}

	private void robinHammerSound(Farmer who)
	{
		if (Game1.currentLocation.Equals(base.currentLocation) && Utility.isOnScreen(base.Position, 256))
		{
			Game1.playSound((Game1.random.NextDouble() < 0.1) ? "clank" : "axchop");
			this.shakeTimer = 250;
		}
	}

	private void robinVariablePause(Farmer who)
	{
		if (Game1.random.NextDouble() < 0.4)
		{
			this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, 300, secondaryArm: false, flip: false, robinVariablePause);
		}
		else if (Game1.random.NextDouble() < 0.25)
		{
			this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(23, Game1.random.Next(500, 4000), secondaryArm: false, flip: false, robinVariablePause);
		}
		else
		{
			this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, Game1.random.Next(1000, 4000), secondaryArm: false, flip: false, robinVariablePause);
		}
	}

	public void randomSquareMovement(GameTime time)
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
		boundingBox.Inflate(2, 2);
		Microsoft.Xna.Framework.Rectangle endRect = new Microsoft.Xna.Framework.Rectangle((int)this.nextSquarePosition.X * 64, (int)this.nextSquarePosition.Y * 64, 64, 64);
		if (this.nextSquarePosition.Equals(Vector2.Zero))
		{
			this.squarePauseAccumulation = 0;
			this.squarePauseTotal = Game1.random.Next(6000 + this.squarePauseOffset, 12000 + this.squarePauseOffset);
			this.nextSquarePosition = new Vector2(this.lastCrossroad.X / 64 - this.lengthOfWalkingSquareX / 2 + Game1.random.Next(this.lengthOfWalkingSquareX), this.lastCrossroad.Y / 64 - this.lengthOfWalkingSquareY / 2 + Game1.random.Next(this.lengthOfWalkingSquareY));
		}
		else if (endRect.Contains(boundingBox))
		{
			this.Halt();
			if (this.squareMovementFacingPreference != -1)
			{
				this.faceDirection(this.squareMovementFacingPreference);
			}
			base.isCharging = false;
			base.speed = 2;
		}
		else if (boundingBox.Left <= endRect.Left)
		{
			this.SetMovingOnlyRight();
		}
		else if (boundingBox.Right >= endRect.Right)
		{
			this.SetMovingOnlyLeft();
		}
		else if (boundingBox.Top <= endRect.Top)
		{
			this.SetMovingOnlyDown();
		}
		else if (boundingBox.Bottom >= endRect.Bottom)
		{
			this.SetMovingOnlyUp();
		}
		this.squarePauseAccumulation += time.ElapsedGameTime.Milliseconds;
		if (this.squarePauseAccumulation >= this.squarePauseTotal && endRect.Contains(boundingBox))
		{
			this.nextSquarePosition = Vector2.Zero;
			base.isCharging = false;
			base.speed = 2;
		}
	}

	public void returnToEndPoint()
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
		boundingBox.Inflate(2, 2);
		if (boundingBox.Left <= this.lastCrossroad.Left)
		{
			this.SetMovingOnlyRight();
		}
		else if (boundingBox.Right >= this.lastCrossroad.Right)
		{
			this.SetMovingOnlyLeft();
		}
		else if (boundingBox.Top <= this.lastCrossroad.Top)
		{
			this.SetMovingOnlyDown();
		}
		else if (boundingBox.Bottom >= this.lastCrossroad.Bottom)
		{
			this.SetMovingOnlyUp();
		}
		boundingBox.Inflate(-2, -2);
		if (this.lastCrossroad.Contains(boundingBox))
		{
			this.isWalkingInSquare = false;
			this.nextSquarePosition = Vector2.Zero;
			this.returningToEndPoint = false;
			this.Halt();
		}
	}

	public void SetMovingOnlyUp()
	{
		base.moveUp = true;
		base.moveDown = false;
		base.moveLeft = false;
		base.moveRight = false;
	}

	public void SetMovingOnlyRight()
	{
		base.moveUp = false;
		base.moveDown = false;
		base.moveLeft = false;
		base.moveRight = true;
	}

	public void SetMovingOnlyDown()
	{
		base.moveUp = false;
		base.moveDown = true;
		base.moveLeft = false;
		base.moveRight = false;
	}

	public void SetMovingOnlyLeft()
	{
		base.moveUp = false;
		base.moveDown = false;
		base.moveLeft = true;
		base.moveRight = false;
	}

	public virtual int getTimeFarmerMustPushBeforePassingThrough()
	{
		return 1500;
	}

	public virtual int getTimeFarmerMustPushBeforeStartShaking()
	{
		return 400;
	}

	public int CompareTo(object obj)
	{
		if (obj is NPC npc)
		{
			return npc.id - this.id;
		}
		return 0;
	}

	public virtual void Removed()
	{
	}
}
