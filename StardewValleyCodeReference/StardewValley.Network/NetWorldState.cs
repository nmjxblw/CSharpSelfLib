using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Quests;
using StardewValley.Util;

namespace StardewValley.Network;

public class NetWorldState : INetObject<NetFields>
{
	protected readonly NetLong uniqueIDForThisGame = new NetLong();

	protected readonly NetEnum<ServerPrivacy> serverPrivacy = new NetEnum<ServerPrivacy>();

	protected readonly NetInt whichFarm = new NetInt();

	protected readonly NetString whichModFarm = new NetString();

	protected string _oldModFarmType;

	public readonly NetEnum<Game1.MineChestType> shuffleMineChests = new NetEnum<Game1.MineChestType>(Game1.MineChestType.Default);

	public readonly NetInt minesDifficulty = new NetInt();

	public readonly NetInt skullCavesDifficulty = new NetInt();

	public readonly NetInt highestPlayerLimit = new NetInt(-1);

	public readonly NetInt currentPlayerLimit = new NetInt(-1);

	protected readonly NetInt year = new NetInt(1);

	protected readonly NetEnum<Season> season = new NetEnum<Season>(Season.Spring);

	protected readonly NetInt dayOfMonth = new NetInt(0);

	protected readonly NetInt timeOfDay = new NetInt();

	protected readonly NetInt daysPlayed = new NetInt();

	public readonly NetInt visitsUntilY1Guarantee = new NetInt(-1);

	protected readonly NetBool isPaused = new NetBool();

	protected readonly NetBool isTimePaused = new NetBool
	{
		InterpolationWait = false
	};

	protected readonly NetStringDictionary<LocationWeather, NetRef<LocationWeather>> locationWeather = new NetStringDictionary<LocationWeather, NetRef<LocationWeather>>();

	protected readonly NetBool isRaining = new NetBool();

	protected readonly NetBool isSnowing = new NetBool();

	protected readonly NetBool isLightning = new NetBool();

	protected readonly NetBool isDebrisWeather = new NetBool();

	public readonly NetString weatherForTomorrow = new NetString();

	protected readonly NetBundles bundles = new NetBundles();

	protected readonly NetIntDictionary<bool, NetBool> bundleRewards = new NetIntDictionary<bool, NetBool>();

	protected readonly NetStringDictionary<string, NetString> netBundleData = new NetStringDictionary<string, NetString>();

	protected Dictionary<string, string> _bundleData;

	protected bool _bundleDataDirty = true;

	public readonly NetArray<bool, NetBool> raccoonBundles = new NetArray<bool, NetBool>(2);

	public readonly NetInt seasonOfCurrentRacconBundle = new NetInt(-1);

	public readonly NetBool parrotPlatformsUnlocked = new NetBool();

	public readonly NetBool goblinRemoved = new NetBool();

	public readonly NetBool submarineLocked = new NetBool();

	public readonly NetInt lowestMineLevel = new NetInt();

	public readonly NetInt lowestMineLevelForOrder = new NetInt(-1);

	protected readonly NetVector2Dictionary<string, NetString> museumPieces = new NetVector2Dictionary<string, NetString>();

	protected readonly NetIntDelta lostBooksFound = new NetIntDelta
	{
		Minimum = 0,
		Maximum = 21
	};

	protected readonly NetIntDelta goldenWalnuts = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta goldenWalnutsFound = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetBool goldenCoconutCracked = new NetBool();

	protected readonly NetStringHashSet foundBuriedNuts = new NetStringHashSet();

	protected readonly NetIntDelta miniShippingBinsObtained = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta perfectionWaivers = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta timesFedRaccoons = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta treasureTotemsUsed = new NetIntDelta
	{
		Minimum = 0
	};

	public NetLongDictionary<Farmer, NetRef<Farmer>> farmhandData = new NetLongDictionary<Farmer, NetRef<Farmer>>();

	/// <summary>The backing field for <see cref="P:StardewValley.Network.NetWorldState.LocationsWithBuildings" />.</summary>
	public readonly NetStringHashSet locationsWithBuildings = new NetStringHashSet();

	public NetStringDictionary<BuilderData, NetRef<BuilderData>> builders = new NetStringDictionary<BuilderData, NetRef<BuilderData>>();

	public NetStringHashSet activePassiveFestivals = new NetStringHashSet();

	protected readonly NetStringHashSet worldStateIDs = new NetStringHashSet();

	protected readonly NetStringHashSet islandVisitors = new NetStringHashSet();

	protected readonly NetStringHashSet checkedGarbage = new NetStringHashSet();

	public readonly NetRef<Object> dishOfTheDay = new NetRef<Object>();

	private readonly NetBool activatedGoldenParrot = new NetBool();

	private readonly NetInt daysPlayedWhenLastRaccoonBundleWasFinished = new NetInt();

	public readonly NetBool canDriveYourselfToday = new NetBool();

	public readonly NetBool goldenClocksTurnedOff = new NetBool();

	/// <summary>The backing field for <see cref="P:StardewValley.Network.NetWorldState.QuestOfTheDay" />.</summary>
	protected readonly NetRef<Quest> netQuestOfTheDay = new NetRef<Quest>();

	public NetFields NetFields { get; } = new NetFields("NetWorldState");

	public ServerPrivacy ServerPrivacy
	{
		get
		{
			return this.serverPrivacy.Value;
		}
		set
		{
			this.serverPrivacy.Value = value;
		}
	}

	public Game1.MineChestType ShuffleMineChests
	{
		get
		{
			return this.shuffleMineChests.Value;
		}
		set
		{
			this.shuffleMineChests.Value = value;
		}
	}

	public int MinesDifficulty
	{
		get
		{
			return this.minesDifficulty.Value;
		}
		set
		{
			this.minesDifficulty.Value = value;
		}
	}

	public int SkullCavesDifficulty
	{
		get
		{
			return this.skullCavesDifficulty.Value;
		}
		set
		{
			this.skullCavesDifficulty.Value = value;
		}
	}

	public int HighestPlayerLimit
	{
		get
		{
			return this.highestPlayerLimit.Value;
		}
		set
		{
			this.highestPlayerLimit.Value = value;
		}
	}

	public int CurrentPlayerLimit
	{
		get
		{
			return this.currentPlayerLimit.Value;
		}
		set
		{
			this.currentPlayerLimit.Value = value;
		}
	}

	public WorldDate Date => WorldDate.Now();

	public int VisitsUntilY1Guarantee
	{
		get
		{
			return this.visitsUntilY1Guarantee.Value;
		}
		set
		{
			this.visitsUntilY1Guarantee.Value = value;
		}
	}

	public bool IsPaused
	{
		get
		{
			return this.isPaused.Value;
		}
		set
		{
			this.isPaused.Value = value;
		}
	}

	public bool IsTimePaused
	{
		get
		{
			return this.isTimePaused.Value;
		}
		set
		{
			this.isTimePaused.Value = value;
		}
	}

	public NetStringDictionary<LocationWeather, NetRef<LocationWeather>> LocationWeather => this.locationWeather;

	public string WeatherForTomorrow
	{
		get
		{
			return this.weatherForTomorrow.Value;
		}
		set
		{
			this.weatherForTomorrow.Value = value;
		}
	}

	public NetBundles Bundles => this.bundles;

	public NetIntDictionary<bool, NetBool> BundleRewards => this.bundleRewards;

	public Dictionary<string, string> BundleData
	{
		get
		{
			if (this.netBundleData.Length == 0)
			{
				this.SetBundleData(DataLoader.Bundles(Game1.content));
			}
			if (this._bundleDataDirty)
			{
				this._bundleDataDirty = false;
				this._bundleData = new Dictionary<string, string>();
				foreach (string key in this.netBundleData.Keys)
				{
					this._bundleData[key] = this.netBundleData[key];
				}
				this.UpdateBundleDisplayNames();
			}
			return this._bundleData;
		}
	}

	public bool ParrotPlatformsUnlocked
	{
		get
		{
			return this.parrotPlatformsUnlocked.Value;
		}
		set
		{
			this.parrotPlatformsUnlocked.Value = value;
		}
	}

	public bool IsGoblinRemoved
	{
		get
		{
			return this.goblinRemoved.Value;
		}
		set
		{
			this.goblinRemoved.Value = value;
		}
	}

	public bool IsSubmarineLocked
	{
		get
		{
			return this.submarineLocked.Value;
		}
		set
		{
			this.submarineLocked.Value = value;
		}
	}

	public int LowestMineLevel
	{
		get
		{
			return this.lowestMineLevel.Value;
		}
		set
		{
			this.lowestMineLevel.Value = value;
		}
	}

	public int LowestMineLevelForOrder
	{
		get
		{
			return this.lowestMineLevelForOrder.Value;
		}
		set
		{
			this.lowestMineLevelForOrder.Value = value;
		}
	}

	public NetVector2Dictionary<string, NetString> MuseumPieces => this.museumPieces;

	public int LostBooksFound
	{
		get
		{
			return this.lostBooksFound.Value;
		}
		set
		{
			this.lostBooksFound.Value = value;
		}
	}

	public int GoldenWalnuts
	{
		get
		{
			return this.goldenWalnuts.Value;
		}
		set
		{
			this.goldenWalnuts.Value = value;
		}
	}

	public int GoldenWalnutsFound
	{
		get
		{
			return this.goldenWalnutsFound.Value;
		}
		set
		{
			this.goldenWalnutsFound.Value = value;
		}
	}

	public bool GoldenCoconutCracked
	{
		get
		{
			return this.goldenCoconutCracked.Value;
		}
		set
		{
			this.goldenCoconutCracked.Value = value;
		}
	}

	public bool ActivatedGoldenParrot
	{
		get
		{
			return this.activatedGoldenParrot.Value;
		}
		set
		{
			this.activatedGoldenParrot.Value = value;
		}
	}

	public ISet<string> FoundBuriedNuts => this.foundBuriedNuts;

	public int MiniShippingBinsObtained
	{
		get
		{
			return this.miniShippingBinsObtained.Value;
		}
		set
		{
			this.miniShippingBinsObtained.Value = value;
		}
	}

	public int PerfectionWaivers
	{
		get
		{
			return this.perfectionWaivers.Value;
		}
		set
		{
			this.perfectionWaivers.Value = value;
		}
	}

	public int TimesFedRaccoons
	{
		get
		{
			return this.timesFedRaccoons.Value;
		}
		set
		{
			this.timesFedRaccoons.Value = value;
		}
	}

	public int TreasureTotemsUsed
	{
		get
		{
			return this.treasureTotemsUsed.Value;
		}
		set
		{
			this.treasureTotemsUsed.Value = value;
		}
	}

	public int SeasonOfCurrentRacconBundle
	{
		get
		{
			return this.seasonOfCurrentRacconBundle.Value;
		}
		set
		{
			this.seasonOfCurrentRacconBundle.Value = value;
		}
	}

	public int DaysPlayedWhenLastRaccoonBundleWasFinished
	{
		get
		{
			return this.daysPlayedWhenLastRaccoonBundleWasFinished.Value;
		}
		set
		{
			this.daysPlayedWhenLastRaccoonBundleWasFinished.Value = value;
		}
	}

	/// <summary>The unique names for locations which contain at least one constructed building.</summary>
	public ISet<string> LocationsWithBuildings => this.locationsWithBuildings;

	public NetStringDictionary<BuilderData, NetRef<BuilderData>> Builders => this.builders;

	public ISet<string> ActivePassiveFestivals => this.activePassiveFestivals;

	public ISet<string> IslandVisitors => this.islandVisitors;

	public ISet<string> CheckedGarbage => this.checkedGarbage;

	public Object DishOfTheDay
	{
		get
		{
			return this.dishOfTheDay.Value;
		}
		set
		{
			this.dishOfTheDay.Value = value;
		}
	}

	/// <summary>The daily quest that's shown on the billboard, if any.</summary>
	/// <remarks>This is synchronized from the host in multiplayer. See <see cref="M:StardewValley.Network.NetWorldState.SetQuestOfTheDay(StardewValley.Quests.Quest)" /> to set it.</remarks>
	public Quest QuestOfTheDay { get; private set; }

	public NetWorldState()
	{
		this.RegisterSpecialCurrencies();
		this.NetFields.SetOwner(this).AddField(this.uniqueIDForThisGame, "uniqueIDForThisGame").AddField(this.serverPrivacy, "serverPrivacy")
			.AddField(this.whichFarm, "whichFarm")
			.AddField(this.whichModFarm, "whichModFarm")
			.AddField(this.shuffleMineChests, "shuffleMineChests")
			.AddField(this.minesDifficulty, "minesDifficulty")
			.AddField(this.skullCavesDifficulty, "skullCavesDifficulty")
			.AddField(this.highestPlayerLimit, "highestPlayerLimit")
			.AddField(this.currentPlayerLimit, "currentPlayerLimit")
			.AddField(this.year, "year")
			.AddField(this.season, "season")
			.AddField(this.dayOfMonth, "dayOfMonth")
			.AddField(this.timeOfDay, "timeOfDay")
			.AddField(this.daysPlayed, "daysPlayed")
			.AddField(this.visitsUntilY1Guarantee, "visitsUntilY1Guarantee")
			.AddField(this.isPaused, "isPaused")
			.AddField(this.isTimePaused, "isTimePaused")
			.AddField(this.locationWeather, "locationWeather")
			.AddField(this.isRaining, "isRaining")
			.AddField(this.isSnowing, "isSnowing")
			.AddField(this.isLightning, "isLightning")
			.AddField(this.isDebrisWeather, "isDebrisWeather")
			.AddField(this.weatherForTomorrow, "weatherForTomorrow")
			.AddField(this.bundles, "bundles")
			.AddField(this.bundleRewards, "bundleRewards")
			.AddField(this.netBundleData, "netBundleData")
			.AddField(this.raccoonBundles, "raccoonBundles")
			.AddField(this.seasonOfCurrentRacconBundle, "seasonOfCurrentRacconBundle")
			.AddField(this.parrotPlatformsUnlocked, "parrotPlatformsUnlocked")
			.AddField(this.goblinRemoved, "goblinRemoved")
			.AddField(this.submarineLocked, "submarineLocked")
			.AddField(this.lowestMineLevel, "lowestMineLevel")
			.AddField(this.lowestMineLevelForOrder, "lowestMineLevelForOrder")
			.AddField(this.museumPieces, "museumPieces")
			.AddField(this.lostBooksFound, "lostBooksFound")
			.AddField(this.goldenWalnuts, "goldenWalnuts")
			.AddField(this.goldenWalnutsFound, "goldenWalnutsFound")
			.AddField(this.goldenCoconutCracked, "goldenCoconutCracked")
			.AddField(this.foundBuriedNuts, "foundBuriedNuts")
			.AddField(this.miniShippingBinsObtained, "miniShippingBinsObtained")
			.AddField(this.perfectionWaivers, "perfectionWaivers")
			.AddField(this.timesFedRaccoons, "timesFedRaccoons")
			.AddField(this.treasureTotemsUsed, "treasureTotemsUsed")
			.AddField(this.farmhandData, "farmhandData")
			.AddField(this.locationsWithBuildings, "locationsWithBuildings")
			.AddField(this.builders, "builders")
			.AddField(this.activePassiveFestivals, "activePassiveFestivals")
			.AddField(this.worldStateIDs, "worldStateIDs")
			.AddField(this.islandVisitors, "islandVisitors")
			.AddField(this.checkedGarbage, "checkedGarbage")
			.AddField(this.dishOfTheDay, "dishOfTheDay")
			.AddField(this.netQuestOfTheDay, "netQuestOfTheDay")
			.AddField(this.activatedGoldenParrot, "activatedGoldenParrot")
			.AddField(this.daysPlayedWhenLastRaccoonBundleWasFinished, "daysPlayedWhenLastRaccoonBundleWasFinished")
			.AddField(this.canDriveYourselfToday, "canDriveYourselfToday")
			.AddField(this.goldenClocksTurnedOff, "goldenClocksTurnedOff");
		this.netBundleData.OnConflictResolve += delegate
		{
			this._bundleDataDirty = true;
		};
		this.netBundleData.OnValueAdded += delegate
		{
			this._bundleDataDirty = true;
		};
		this.netBundleData.OnValueRemoved += delegate
		{
			this._bundleDataDirty = true;
		};
		this.netQuestOfTheDay.fieldChangeVisibleEvent += delegate(NetRef<Quest> field, Quest oldQuest, Quest newQuest)
		{
			if (newQuest == null)
			{
				this.QuestOfTheDay = null;
				return;
			}
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter writer = new BinaryWriter(memoryStream);
			NetRef<Quest> netRef = new NetRef<Quest>();
			netRef.Value = newQuest;
			netRef.WriteFull(writer);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			using BinaryReader reader = new BinaryReader(memoryStream);
			NetRef<Quest> netRef2 = new NetRef<Quest>();
			netRef2.ReadFull(reader, default(NetVersion));
			this.QuestOfTheDay = netRef2.Value;
		};
	}

	public virtual void RegisterSpecialCurrencies()
	{
		if (Game1.specialCurrencyDisplay != null)
		{
			Game1.specialCurrencyDisplay.Register("walnuts", this.goldenWalnuts);
			Game1.specialCurrencyDisplay.Register("qiGems", Game1.player.netQiGems);
		}
	}

	/// <summary>Sets the quest of the day and synchronizes it to other players. In multiplayer, this can only be called on the host instance.</summary>
	/// <param name="quest">The daily quest to set.</param>
	public void SetQuestOfTheDay(Quest quest)
	{
		if (!Game1.IsMasterGame)
		{
			Game1.log.Warn("Can't set the daily quest from a farmhand instance.");
			Game1.log.Verbose(new StackTraceHelper().ToString());
		}
		else
		{
			this.netQuestOfTheDay.Value = quest;
		}
	}

	public void SetBundleData(Dictionary<string, string> data)
	{
		this._bundleDataDirty = true;
		this.netBundleData.CopyFrom(data);
		foreach (KeyValuePair<string, string> pair in this.netBundleData.Pairs)
		{
			string key = pair.Key;
			string value = pair.Value;
			int index = Convert.ToInt32(key.Split('/')[1]);
			int count = ArgUtility.SplitBySpace(value.Split('/')[2]).Length;
			if (!this.bundles.ContainsKey(index))
			{
				this.bundles.Add(index, new NetArray<bool, NetBool>(count));
			}
			else if (this.bundles[index].Length < count)
			{
				NetArray<bool, NetBool> new_array = new NetArray<bool, NetBool>(count);
				for (int i = 0; i < Math.Min(this.bundles[index].Length, count); i++)
				{
					new_array[i] = this.bundles[index][i];
				}
				this.bundles.Remove(index);
				this.bundles.Add(index, new_array);
			}
			if (!this.bundleRewards.ContainsKey(index))
			{
				this.bundleRewards.Add(index, new NetBool(value: false));
			}
		}
	}

	public static bool checkAnywhereForWorldStateID(string id)
	{
		if (!Game1.worldStateIDs.Contains(id))
		{
			return Game1.netWorldState.Value.hasWorldStateID(id);
		}
		return true;
	}

	public static void addWorldStateIDEverywhere(string id)
	{
		Game1.netWorldState.Value.addWorldStateID(id);
		if (!Game1.worldStateIDs.Contains(id))
		{
			Game1.worldStateIDs.Add(id);
		}
	}

	public virtual void UpdateBundleDisplayNames()
	{
		List<string> list = new List<string>(this._bundleData.Keys);
		Dictionary<string, string> localizedBundleData = DataLoader.Bundles(Game1.content);
		foreach (string key in list)
		{
			string[] fields = this._bundleData[key].Split('/');
			string bundleName = fields[0];
			if (!ArgUtility.HasIndex(fields, 6))
			{
				Array.Resize(ref fields, 7);
			}
			string displayName = null;
			foreach (string value in localizedBundleData.Values)
			{
				string[] localizedFields = value.Split('/');
				if (ArgUtility.Get(localizedFields, 0) == bundleName)
				{
					displayName = ArgUtility.Get(localizedFields, 6);
					break;
				}
			}
			if (displayName == null)
			{
				displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\BundleNames:" + bundleName);
			}
			fields[6] = displayName ?? bundleName;
			this._bundleData[key] = string.Join("/", fields);
		}
	}

	public bool hasWorldStateID(string id)
	{
		return this.worldStateIDs.Contains(id);
	}

	public void addWorldStateID(string id)
	{
		this.worldStateIDs.Add(id);
	}

	public void removeWorldStateID(string id)
	{
		this.worldStateIDs.Remove(id);
	}

	public void SaveFarmhand(NetFarmerRoot farmhand)
	{
		if (Game1.netWorldState.Value.farmhandData.FieldDict.TryGetValue(farmhand.Value.UniqueMultiplayerID, out var farmhandData))
		{
			farmhand.CloneInto(farmhandData);
		}
		this.ResetFarmhandState(farmhand.Value);
	}

	public void ResetFarmhandState(Farmer farmhand)
	{
		farmhand.farmName.Value = Game1.MasterPlayer.farmName.Value;
		if (this.TryAssignFarmhandHome(farmhand))
		{
			FarmHouse farmhandHome = Utility.getHomeOfFarmer(farmhand);
			if (farmhand.lastSleepLocation.Value == null || farmhand.lastSleepLocation.Value == farmhandHome.NameOrUniqueName)
			{
				farmhand.currentLocation = farmhandHome;
				farmhand.Position = Utility.PointToVector2(farmhandHome.GetPlayerBedSpot()) * 64f;
			}
		}
		else
		{
			farmhand.userID.Value = "";
			farmhand.homeLocation.Value = null;
			Game1.otherFarmers.Remove(farmhand.UniqueMultiplayerID);
		}
		farmhand.resetState();
	}

	/// <summary>Assign a farmhand to a cabin if their current home is invalid.</summary>
	/// <param name="farmhand">The farmhand instance.</param>
	/// <returns>Returns whether the farmhand has a valid home (either already assigned or just assigned).</returns>
	public bool TryAssignFarmhandHome(Farmer farmhand)
	{
		if (farmhand.IsMainPlayer || Game1.getLocationFromName(farmhand.homeLocation.Value) is Cabin)
		{
			return true;
		}
		if (farmhand.currentLocation is Cabin curLocation && curLocation.CanAssignTo(farmhand))
		{
			curLocation.AssignFarmhand(farmhand);
			return true;
		}
		if (Game1.getLocationFromName(farmhand.lastSleepLocation.Value) is Cabin lastSleptCabin && lastSleptCabin.CanAssignTo(farmhand))
		{
			lastSleptCabin.AssignFarmhand(farmhand);
			return true;
		}
		bool found = false;
		Utility.ForEachBuilding(delegate(Building building)
		{
			if (building.GetIndoors() is Cabin cabin && cabin.CanAssignTo(farmhand))
			{
				cabin.AssignFarmhand(farmhand);
				found = true;
				return false;
			}
			return true;
		});
		return found;
	}

	public void UpdateFromGame1()
	{
		this.year.Value = Game1.year;
		this.season.Value = Game1.season;
		this.dayOfMonth.Value = Game1.dayOfMonth;
		this.timeOfDay.Value = Game1.timeOfDay;
		LocationWeather weatherForLocation = this.GetWeatherForLocation("Default");
		weatherForLocation.WeatherForTomorrow = Game1.weatherForTomorrow;
		weatherForLocation.IsRaining = Game1.isRaining;
		weatherForLocation.IsSnowing = Game1.isSnowing;
		weatherForLocation.IsDebrisWeather = Game1.isDebrisWeather;
		weatherForLocation.IsGreenRain = Game1.isGreenRain;
		this.isDebrisWeather.Value = Game1.isDebrisWeather;
		this.whichFarm.Value = Game1.whichFarm;
		this.weatherForTomorrow.Value = Game1.weatherForTomorrow;
		this.daysPlayed.Value = (int)Game1.stats.DaysPlayed;
		this.uniqueIDForThisGame.Value = (long)Game1.uniqueIDForThisGame;
		if (Game1.whichFarm != 7 || Game1.whichModFarm == null)
		{
			this.whichModFarm.Value = null;
		}
		else
		{
			this.whichModFarm.Value = Game1.whichModFarm.Id;
		}
		this.currentPlayerLimit.Value = Game1.multiplayer.playerLimit;
		this.highestPlayerLimit.Value = Math.Max(this.highestPlayerLimit.Value, Game1.multiplayer.playerLimit);
		this.worldStateIDs.Clear();
		this.worldStateIDs.AddRange(Game1.worldStateIDs);
	}

	public LocationWeather GetWeatherForLocation(string locationContextId)
	{
		if (!this.locationWeather.TryGetValue(locationContextId, out var weather))
		{
			weather = (this.locationWeather[locationContextId] = new LocationWeather());
			if (Game1.locationContextData.TryGetValue(locationContextId, out var contextData))
			{
				weather.UpdateDailyWeather(locationContextId, contextData, Game1.random);
				weather.UpdateDailyWeather(locationContextId, contextData, Game1.random);
			}
		}
		return weather;
	}

	public void WriteToGame1(bool onLoad = false)
	{
		if (Game1.farmEvent != null)
		{
			return;
		}
		LocationWeather weatherForLocation = this.GetWeatherForLocation("Default");
		Game1.weatherForTomorrow = weatherForLocation.WeatherForTomorrow;
		Game1.isRaining = weatherForLocation.IsRaining;
		Game1.isSnowing = weatherForLocation.IsSnowing;
		Game1.isLightning = weatherForLocation.IsLightning;
		Game1.isDebrisWeather = weatherForLocation.IsDebrisWeather;
		Game1.isGreenRain = weatherForLocation.IsGreenRain;
		Game1.weatherForTomorrow = this.weatherForTomorrow.Value;
		Game1.worldStateIDs = new HashSet<string>(this.worldStateIDs);
		if (!Game1.IsServer)
		{
			bool newSeason = Game1.season != this.season.Value;
			Game1.year = this.year.Value;
			Game1.season = this.season.Value;
			Game1.dayOfMonth = this.dayOfMonth.Value;
			Game1.timeOfDay = this.timeOfDay.Value;
			Game1.whichFarm = this.whichFarm.Value;
			if (Game1.whichFarm != 7)
			{
				Game1.whichModFarm = null;
			}
			else if (this._oldModFarmType != this.whichModFarm.Value)
			{
				this._oldModFarmType = this.whichModFarm.Value;
				Game1.whichModFarm = null;
				List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
				if (farm_types != null)
				{
					foreach (ModFarmType farm_type in farm_types)
					{
						if (farm_type.Id == this.whichModFarm.Value)
						{
							Game1.whichModFarm = farm_type;
							break;
						}
					}
				}
				if (Game1.whichModFarm == null)
				{
					throw new Exception(this.whichModFarm.Value + " is not a valid farm type.");
				}
			}
			Game1.stats.DaysPlayed = (uint)this.daysPlayed.Value;
			Game1.uniqueIDForThisGame = (ulong)this.uniqueIDForThisGame.Value;
			if (newSeason)
			{
				Game1.setGraphicsForSeason(onLoad);
			}
		}
		Game1.updateWeatherIcon();
		if (this.IsGoblinRemoved)
		{
			Game1.player.removeQuest("27");
		}
	}

	/// <summary>Get cached info about the building being constructed by an NPC.</summary>
	/// <param name="builderName">The internal name of the NPC constructing buildings.</param>
	public BuilderData GetBuilderData(string builderName)
	{
		if (!this.builders.TryGetValue(builderName, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Mark a building as being under construction.</summary>
	/// <param name="builderName">The internal name of the NPC constructing it.</param>
	/// <param name="building">The building being constructed.</param>
	public void MarkUnderConstruction(string builderName, Building building)
	{
		int buildDays = building.daysOfConstructionLeft.Value;
		int upgradeDays = building.daysUntilUpgrade.Value;
		int daysUntilFinished = Math.Max(buildDays, upgradeDays);
		if (daysUntilFinished != 0)
		{
			this.builders[builderName] = new BuilderData(building.buildingType.Value, daysUntilFinished, building.parentLocationName.Value, new Point(building.tileX.Value, building.tileY.Value), upgradeDays > 0 && buildDays <= 0);
		}
	}

	/// <summary>Remove constructed buildings from the cached list of buildings under construction.</summary>
	public void UpdateUnderConstruction()
	{
		KeyValuePair<string, BuilderData>[] array = this.builders.Pairs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, BuilderData> pair = array[i];
			string builderName = pair.Key;
			BuilderData data = pair.Value;
			GameLocation location = Game1.getLocationFromName(data.buildingLocation.Value);
			if (location == null)
			{
				this.builders.Remove(builderName);
				continue;
			}
			Building building = location.getBuildingAt(Utility.PointToVector2(data.buildingTile.Value));
			if (building == null || !building.isUnderConstruction(ignoreUpgrades: false))
			{
				this.builders.Remove(builderName);
			}
		}
	}

	/// <summary>Add or remove the location from the <see cref="P:StardewValley.Network.NetWorldState.LocationsWithBuildings" /> cache.</summary>
	/// <param name="location">The location to update.</param>
	public void UpdateBuildingCache(GameLocation location)
	{
		string name = location.NameOrUniqueName;
		if (location.buildings.Count > 0)
		{
			this.locationsWithBuildings.Add(name);
		}
		else
		{
			this.locationsWithBuildings.Remove(name);
		}
	}
}
