using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Locations;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SaveSerialization;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.Util;

namespace StardewValley;

public class SaveGame
{
	/// <summary>The filename suffix for a save file that's currently being written.</summary>
	public const string TempNameSuffix = "_STARDEWVALLEYSAVETMP";

	/// <summary>The filename suffix for a previous save file.</summary>
	public const string BackupNameSuffix = "_old";

	/// <summary>Whether the current platform supports save backup files with the <see cref="F:StardewValley.SaveGame.TempNameSuffix" /> and <see cref="F:StardewValley.SaveGame.BackupNameSuffix" /> suffixes.</summary>
	public const bool PlatformSupportsBackups = true;

	[InstancedStatic]
	public static bool IsProcessing;

	[InstancedStatic]
	public static bool CancelToTitle;

	public Farmer player;

	public List<Farmer> farmhands;

	public List<GameLocation> locations;

	public string currentSeason;

	public string samBandName;

	public string elliottBookName;

	/// <summary>Obsolete. This is only kept to preserve data from old save files.</summary>
	[XmlArray("mailbox")]
	public List<string> obsolete_mailbox;

	public HashSet<string> broadcastedMail;

	public HashSet<string> constructedBuildings;

	public HashSet<string> worldStateIDs;

	public int lostBooksFound = -1;

	public int goldenWalnuts = -1;

	public int goldenWalnutsFound;

	public int miniShippingBinsObtained;

	public bool mineShrineActivated;

	public bool skullShrineActivated;

	public bool goldenCoconutCracked;

	public bool parrotPlatformsUnlocked;

	public bool farmPerfect;

	public List<string> foundBuriedNuts = new List<string>();

	public List<string> checkedGarbage = new List<string>();

	public int visitsUntilY1Guarantee = -1;

	public Game1.MineChestType shuffleMineChests;

	public int dayOfMonth;

	public int year;

	public int? countdownToWedding;

	public double dailyLuck;

	public ulong uniqueIDForThisGame;

	public bool weddingToday;

	public bool isRaining;

	public bool isDebrisWeather;

	public bool isLightning;

	public bool isSnowing;

	public bool shouldSpawnMonsters;

	public bool hasApplied1_3_UpdateChanges;

	public bool hasApplied1_4_UpdateChanges;

	public List<long> weddingsToday;

	/// <summary>Obsolete. This is only kept to preserve data from old save files.</summary>
	[XmlElement("stats")]
	public Stats obsolete_stats;

	[InstancedStatic]
	public static SaveGame loaded;

	public float musicVolume;

	public float soundVolume;

	public Object dishOfTheDay;

	public int highestPlayerLimit = -1;

	public int moveBuildingPermissionMode;

	public bool useLegacyRandom;

	public bool allowChatCheats;

	public bool hasDedicatedHost;

	public SerializableDictionary<string, LocationWeather> locationWeather;

	[XmlArrayItem("item")]
	public SaveablePair<string, BuilderData>[] builders;

	[XmlArrayItem("item")]
	public SaveablePair<string, string>[] bannedUsers = LegacyShims.EmptyArray<SaveablePair<string, string>>();

	[XmlArrayItem("item")]
	public SaveablePair<string, string>[] bundleData = LegacyShims.EmptyArray<SaveablePair<string, string>>();

	[XmlArrayItem("item")]
	public SaveablePair<string, int>[] limitedNutDrops = LegacyShims.EmptyArray<SaveablePair<string, int>>();

	public long latestID;

	public Options options;

	[XmlArrayItem("item")]
	public SaveablePair<long, Options>[] splitscreenOptions = LegacyShims.EmptyArray<SaveablePair<long, Options>>();

	public SerializableDictionary<string, string> CustomData = new SerializableDictionary<string, string>();

	[XmlArrayItem("item")]
	public SaveablePair<int, MineInfo>[] mine_permanentMineChanges;

	public int mine_lowestLevelReached;

	public string weatherForTomorrow;

	public string whichFarm;

	public int mine_lowestLevelReachedForOrder = -1;

	public int skullCavesDifficulty;

	public int minesDifficulty;

	public int currentGemBirdIndex;

	public NetLeaderboards junimoKartLeaderboards;

	public List<SpecialOrder> specialOrders;

	public List<SpecialOrder> availableSpecialOrders;

	public List<string> completedSpecialOrders;

	public List<string> acceptedSpecialOrderTypes = new List<string>();

	public List<Item> returnedDonations;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.SaveGame.globalInventories" /> instead.</summary>
	public List<Item> junimoChest;

	public Item[] shippingBin = LegacyShims.EmptyArray<Item>();

	/// <inheritdoc cref="F:StardewValley.FarmerTeam.globalInventories" />
	[XmlArrayItem("item")]
	public SaveablePair<string, Item[]>[] globalInventories = LegacyShims.EmptyArray<SaveablePair<string, Item[]>>();

	public List<string> collectedNutTracker = new List<string>();

	[XmlArrayItem("item")]
	public SaveablePair<FarmerPair, Friendship>[] farmerFriendships = LegacyShims.EmptyArray<SaveablePair<FarmerPair, Friendship>>();

	[XmlArrayItem("item")]
	public SaveablePair<int, long>[] cellarAssignments = LegacyShims.EmptyArray<SaveablePair<int, long>>();

	public int timesFedRaccoons;

	public int treasureTotemsUsed;

	public int perfectionWaivers;

	public int seasonOfCurrentRaccoonBundle;

	public bool[] raccoonBundles = new bool[2];

	public bool activatedGoldenParrot;

	public int daysPlayedWhenLastRaccoonBundleWasFinished;

	public int lastAppliedSaveFix;

	public string gameVersion = Game1.version;

	public string gameVersionLabel;

	public static XmlSerializer serializer = new XmlSerializer(typeof(SaveGame), new Type[5]
	{
		typeof(Character),
		typeof(GameLocation),
		typeof(Item),
		typeof(Quest),
		typeof(TerrainFeature)
	});

	public static XmlSerializer farmerSerializer = new XmlSerializer(typeof(Farmer), new Type[1] { typeof(Item) });

	public static XmlSerializer locationSerializer = new XmlSerializer(typeof(GameLocation), new Type[3]
	{
		typeof(Character),
		typeof(Item),
		typeof(TerrainFeature)
	});

	public static XmlSerializer descriptionElementSerializer = new XmlSerializer(typeof(DescriptionElement), new Type[2]
	{
		typeof(Character),
		typeof(Item)
	});

	public static XmlSerializer legacyDescriptionElementSerializer = new XmlSerializer(typeof(SaveMigrator_1_6.LegacyDescriptionElement), new Type[3]
	{
		typeof(DescriptionElement),
		typeof(Character),
		typeof(Item)
	});

	/// <summary>Get whether a fix was applied to the loaded data before it was last saved.</summary>
	/// <param name="fix">The save fix to check.</param>
	public bool HasSaveFix(SaveFixes fix)
	{
		return this.lastAppliedSaveFix >= (int)fix;
	}

	public static IEnumerator<int> Save()
	{
		SaveGame.IsProcessing = true;
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			IEnumerator<int> save = SaveGame.getSaveEnumerator();
			while (save.MoveNext())
			{
				yield return save.Current;
			}
			yield return 100;
			yield break;
		}
		SaveGame.LogVerbose("SaveGame.Save() called.");
		yield return 1;
		IEnumerator<int> loader = SaveGame.getSaveEnumerator();
		Task saveTask = new Task(delegate
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (loader != null)
			{
				while (loader.MoveNext() && loader.Current < 100)
				{
				}
			}
		});
		Game1.hooks.StartTask(saveTask, "Save");
		while (!saveTask.IsCanceled && !saveTask.IsCompleted && !saveTask.IsFaulted)
		{
			yield return 1;
		}
		SaveGame.IsProcessing = false;
		if (saveTask.IsFaulted)
		{
			Exception e = saveTask.Exception.GetBaseException();
			SaveGame.LogError("saveTask failed with an exception", e);
			if (!(e is TaskCanceledException))
			{
				throw e;
			}
			Game1.ExitToTitle();
		}
		else
		{
			SaveGame.LogVerbose("SaveGame.Save() completed without exceptions.");
			yield return 100;
		}
	}

	public static string FilterFileName(string fileName)
	{
		StringBuilder sb = new StringBuilder(fileName.Length);
		string text = fileName;
		foreach (char c in text)
		{
			if (char.IsLetterOrDigit(c))
			{
				sb.Append(c);
			}
		}
		fileName = sb.ToString();
		return fileName;
	}

	/// <summary>Get an enumerator which writes the loaded save to a save file.</summary>
	/// <returns>Returns an enumeration of incrementing progress values between 0 and 100.</returns>
	public static IEnumerator<int> getSaveEnumerator()
	{
		if (SaveGame.CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 1;
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.UnapplyAllTrinketEffects();
		}
		Game1.player.gameVersion = Game1.version;
		Game1.player.gameVersionLabel = Game1.versionLabel;
		foreach (GameLocation location in Game1.locations)
		{
			location.cleanupBeforeSave();
		}
		Game1.player.team.globalInventories.RemoveWhere((KeyValuePair<string, Inventory> p) => !p.Value.HasAny());
		SaveGame saveData = new SaveGame
		{
			player = Game1.player,
			farmhands = new List<Farmer>(Game1.netWorldState.Value.farmhandData.Values),
			locations = new List<GameLocation>(Game1.locations),
			currentSeason = Game1.currentSeason,
			samBandName = Game1.samBandName,
			broadcastedMail = new HashSet<string>(Game1.player.team.broadcastedMail),
			constructedBuildings = new HashSet<string>(Game1.player.team.constructedBuildings),
			bannedUsers = Game1.bannedUsers.ToSaveableArray(),
			skullCavesDifficulty = Game1.netWorldState.Value.SkullCavesDifficulty,
			minesDifficulty = Game1.netWorldState.Value.MinesDifficulty,
			visitsUntilY1Guarantee = Game1.netWorldState.Value.VisitsUntilY1Guarantee,
			shuffleMineChests = Game1.netWorldState.Value.ShuffleMineChests,
			elliottBookName = Game1.elliottBookName,
			dayOfMonth = Game1.dayOfMonth,
			year = Game1.year,
			dailyLuck = Game1.player.team.sharedDailyLuck.Value,
			isRaining = Game1.isRaining,
			isLightning = Game1.isLightning,
			isSnowing = Game1.isSnowing,
			isDebrisWeather = Game1.isDebrisWeather,
			shouldSpawnMonsters = Game1.spawnMonstersAtNight,
			specialOrders = Game1.player.team.specialOrders.ToList(),
			availableSpecialOrders = Game1.player.team.availableSpecialOrders.ToList(),
			completedSpecialOrders = Game1.player.team.completedSpecialOrders.ToList(),
			collectedNutTracker = Game1.player.team.collectedNutTracker.ToList(),
			acceptedSpecialOrderTypes = Game1.player.team.acceptedSpecialOrderTypes.ToList(),
			returnedDonations = Game1.player.team.returnedDonations.ToList(),
			weddingToday = Game1.weddingToday,
			weddingsToday = Game1.weddingsToday.ToList(),
			shippingBin = Game1.getFarm().getShippingBin(Game1.player).ToArray(),
			globalInventories = DictionarySaver<string, Item[]>.ArrayFrom(Game1.player.team.globalInventories.FieldDict, (NetRef<Inventory> value) => value.Value.ToArray()),
			whichFarm = Game1.GetFarmTypeID(),
			junimoKartLeaderboards = Game1.player.team.junimoKartScores,
			lastAppliedSaveFix = 98,
			locationWeather = SerializableDictionary<string, LocationWeather>.BuildFrom(Game1.netWorldState.Value.LocationWeather.FieldDict, (NetRef<LocationWeather> value) => value.Value),
			builders = DictionarySaver<string, BuilderData>.ArrayFrom(Game1.netWorldState.Value.Builders.FieldDict, (NetRef<BuilderData> value) => value.Value),
			cellarAssignments = DictionarySaver<int, long>.ArrayFrom(Game1.player.team.cellarAssignments.FieldDict, (NetLong value) => value.Value),
			uniqueIDForThisGame = Game1.uniqueIDForThisGame,
			musicVolume = Game1.options.musicVolumeLevel,
			soundVolume = Game1.options.soundVolumeLevel,
			mine_lowestLevelReached = Game1.netWorldState.Value.LowestMineLevel,
			mine_lowestLevelReachedForOrder = Game1.netWorldState.Value.LowestMineLevelForOrder,
			currentGemBirdIndex = Game1.currentGemBirdIndex,
			mine_permanentMineChanges = MineShaft.permanentMineChanges.ToSaveableArray(),
			dishOfTheDay = Game1.dishOfTheDay,
			latestID = (long)Game1.multiplayer.latestID,
			highestPlayerLimit = Game1.netWorldState.Value.HighestPlayerLimit,
			options = Game1.options,
			splitscreenOptions = Game1.splitscreenOptions.ToSaveableArray(),
			CustomData = Game1.CustomData,
			worldStateIDs = Game1.worldStateIDs,
			weatherForTomorrow = Game1.weatherForTomorrow,
			goldenWalnuts = Game1.netWorldState.Value.GoldenWalnuts,
			goldenWalnutsFound = Game1.netWorldState.Value.GoldenWalnutsFound,
			miniShippingBinsObtained = Game1.netWorldState.Value.MiniShippingBinsObtained,
			goldenCoconutCracked = Game1.netWorldState.Value.GoldenCoconutCracked,
			parrotPlatformsUnlocked = Game1.netWorldState.Value.ParrotPlatformsUnlocked,
			farmPerfect = Game1.player.team.farmPerfect.Value,
			lostBooksFound = Game1.netWorldState.Value.LostBooksFound,
			foundBuriedNuts = Game1.netWorldState.Value.FoundBuriedNuts.ToList(),
			checkedGarbage = Game1.netWorldState.Value.CheckedGarbage.ToList(),
			mineShrineActivated = Game1.player.team.mineShrineActivated.Value,
			skullShrineActivated = Game1.player.team.skullShrineActivated.Value,
			timesFedRaccoons = Game1.netWorldState.Value.TimesFedRaccoons,
			treasureTotemsUsed = Game1.netWorldState.Value.TreasureTotemsUsed,
			perfectionWaivers = Game1.netWorldState.Value.PerfectionWaivers,
			seasonOfCurrentRaccoonBundle = Game1.netWorldState.Value.SeasonOfCurrentRacconBundle,
			raccoonBundles = Game1.netWorldState.Value.raccoonBundles.ToArray(),
			activatedGoldenParrot = Game1.netWorldState.Value.ActivatedGoldenParrot,
			daysPlayedWhenLastRaccoonBundleWasFinished = Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished,
			gameVersion = Game1.version,
			gameVersionLabel = Game1.versionLabel,
			limitedNutDrops = DictionarySaver<string, int>.ArrayFrom(Game1.player.team.limitedNutDrops.FieldDict, (NetInt value) => value.Value),
			bundleData = Game1.netWorldState.Value.BundleData.ToSaveableArray(),
			moveBuildingPermissionMode = (int)Game1.player.team.farmhandsCanMoveBuildings.Value,
			useLegacyRandom = Game1.player.team.useLegacyRandom.Value,
			allowChatCheats = Game1.player.team.allowChatCheats.Value,
			hasDedicatedHost = Game1.player.team.hasDedicatedHost.Value,
			hasApplied1_3_UpdateChanges = true,
			hasApplied1_4_UpdateChanges = true,
			farmerFriendships = DictionarySaver<FarmerPair, Friendship>.ArrayFrom(Game1.player.team.friendshipData.FieldDict, (NetRef<Friendship> value) => value.Value)
		};
		string finalDataName = SaveGame.FilterFileName(Game1.GetSaveGameName()) + "_" + Game1.uniqueIDForThisGame;
		string saveDirPath = Path.Combine(Program.GetSavesFolder(), finalDataName + Path.DirectorySeparatorChar);
		string finalFarmerPath = Path.Combine(saveDirPath, "SaveGameInfo");
		string finalDataPath = Path.Combine(saveDirPath, finalDataName);
		string tempFarmerPath = finalFarmerPath + "_STARDEWVALLEYSAVETMP";
		string tempDataPath = finalDataPath + "_STARDEWVALLEYSAVETMP";
		SaveGame.ensureFolderStructureExists();
		Stream fstream = null;
		try
		{
			fstream = File.Open(tempDataPath, FileMode.Create);
		}
		catch (IOException ex)
		{
			if (fstream != null)
			{
				fstream.Close();
				fstream.Dispose();
			}
			Game1.gameMode = 9;
			Game1.debugOutput = Game1.parseText(ex.Message);
			yield break;
		}
		MemoryStream mstream1 = new MemoryStream(1024);
		new MemoryStream(1024);
		if (SaveGame.CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 2;
		SaveGame.LogVerbose("Saving without compression...");
		XmlWriterSettings settings = new XmlWriterSettings
		{
			CloseOutput = false
		};
		XmlWriter xmlWriter = XmlWriter.Create(mstream1, settings);
		xmlWriter.WriteStartDocument();
		SaveSerializer.Serialize(xmlWriter, saveData);
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();
		xmlWriter.Close();
		mstream1.Close();
		byte[] buffer1 = mstream1.ToArray();
		if (SaveGame.CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 2;
		fstream.Write(buffer1, 0, buffer1.Length);
		fstream.Close();
		Game1.player.saveTime = (int)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalMinutes;
		try
		{
			fstream = File.Open(tempFarmerPath, FileMode.Create);
		}
		catch (IOException ex2)
		{
			fstream?.Close();
			Game1.gameMode = 9;
			Game1.debugOutput = Game1.parseText(ex2.Message);
			yield break;
		}
		Stream stream2 = fstream;
		XmlWriter xmlWriter2 = XmlWriter.Create(stream2, settings);
		xmlWriter2.WriteStartDocument();
		SaveSerializer.Serialize(xmlWriter2, Game1.player);
		xmlWriter2.WriteEndDocument();
		xmlWriter2.Flush();
		xmlWriter2.Close();
		stream2.Close();
		fstream.Close();
		if (SaveGame.CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 2;
		string oldDataPath = finalDataPath + "_old";
		string oldFarmerPath = finalFarmerPath + "_old";
		try
		{
			LegacyShims.MoveFileWithOverwrite(finalDataPath, oldDataPath);
			LegacyShims.MoveFileWithOverwrite(finalFarmerPath, oldFarmerPath);
		}
		catch
		{
		}
		LegacyShims.MoveFileWithOverwrite(tempDataPath, finalDataPath);
		LegacyShims.MoveFileWithOverwrite(tempFarmerPath, finalFarmerPath);
		foreach (Farmer allFarmer2 in Game1.getAllFarmers())
		{
			allFarmer2.resetAllTrinketEffects();
		}
		Game1.player.sleptInTemporaryBed.Value = false;
		if (SaveGame.CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 100;
	}

	public static bool IsNewGameSaveNameCollision(string save_name)
	{
		string filename = SaveGame.FilterFileName(save_name) + "_" + Game1.uniqueIDForThisGame;
		return Directory.Exists(Path.Combine(Program.GetSavesFolder(), filename));
	}

	public static void ensureFolderStructureExists()
	{
		string folderName = SaveGame.FilterFileName(Game1.GetSaveGameName()) + "_" + Game1.uniqueIDForThisGame;
		Directory.CreateDirectory(Path.Combine(Program.GetSavesFolder(), folderName));
	}

	public static void Load(string filename)
	{
		Game1.gameMode = 6;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4690");
		Game1.currentLoader = SaveGame.getLoadEnumerator(filename);
	}

	public static void LoadFarmType()
	{
		List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
		Game1.whichFarm = -1;
		if (farm_types != null)
		{
			foreach (ModFarmType farm_type in farm_types)
			{
				if (farm_type.Id == SaveGame.loaded.whichFarm)
				{
					Game1.whichModFarm = farm_type;
					Game1.whichFarm = 7;
					break;
				}
			}
		}
		if (SaveGame.loaded.whichFarm == null)
		{
			Game1.whichFarm = 0;
		}
		if (Game1.whichFarm < 0)
		{
			if (int.TryParse(SaveGame.loaded.whichFarm, out var farmType))
			{
				Game1.whichFarm = farmType;
				return;
			}
			SaveGame.LogWarn("Ignored unknown farm type '" + SaveGame.loaded.whichFarm + "' which no longer exists in the data.");
			Game1.whichFarm = 0;
			Game1.whichModFarm = null;
		}
	}

	/// <summary>Read a raw save file, if it's valid.</summary>
	/// <param name="file">The save folder name to load, in the form <c>{farmer name}_{unique id}</c>.</param>
	/// <param name="fileNameSuffix">The suffix for the filename within the save folder to load, if supported by the platform. This should usually be <c>null</c> (main file), <see cref="F:StardewValley.SaveGame.BackupNameSuffix" />, or <see cref="F:StardewValley.SaveGame.TempNameSuffix" />.</param>
	/// <param name="error">An error indicating why loading the save file failed, if applicable.</param>
	/// <remarks>This is a low-level method. Most code should use <see cref="M:StardewValley.SaveGame.getLoadEnumerator(System.String)" /> instead.</remarks>
	public static SaveGame TryReadSaveFile(string file, string fileNameSuffix, out string error)
	{
		string fullFilePath = Path.Combine(Program.GetSavesFolder(), file, file + fileNameSuffix);
		if (!File.Exists(fullFilePath))
		{
			fullFilePath += ".xml";
			if (!File.Exists(fullFilePath))
			{
				return FileDoesNotExist(out error);
			}
		}
		Stream stream = null;
		try
		{
			stream = new MemoryStream(File.ReadAllBytes(fullFilePath), writable: false);
		}
		catch (IOException ex)
		{
			error = ex.Message;
			stream?.Close();
			return null;
		}
		byte firstByte = (byte)stream.ReadByte();
		stream.Position--;
		if (firstByte == 120)
		{
			SaveGame.LogVerbose("zlib stream detected...");
			stream = new ZlibStream(stream, CompressionMode.Decompress);
		}
		try
		{
			error = null;
			return SaveSerializer.Deserialize<SaveGame>(stream);
		}
		catch (Exception ex2)
		{
			error = ex2.Message;
			return null;
		}
		finally
		{
			stream.Dispose();
		}
		static SaveGame FileDoesNotExist(out string outError)
		{
			outError = "File does not exist";
			return null;
		}
	}

	/// <summary>Read a raw save file with automatic fallback to the backup files, if any of them are valid.</summary>
	/// <param name="file">The save folder name to load, in the form <c>{farmer name}_{unique id}</c>.</param>
	/// <param name="error">An error indicating why loading the save file failed, if applicable.</param>
	/// <param name="autoRecovered">Whether the save was auto-recovered by loading a backup.</param>
	/// <remarks>This is a low-level method. Most code should use <see cref="M:StardewValley.SaveGame.getLoadEnumerator(System.String)" /> instead.</remarks>
	public static SaveGame TryReadSaveFileWithFallback(string file, out string error, out bool autoRecovered)
	{
		SaveGame data = SaveGame.TryReadSaveFile(file, null, out error);
		if (data != null)
		{
			error = null;
			autoRecovered = false;
			return data;
		}
		data = SaveGame.TryReadSaveFile(file, "_old", out var error2);
		if (data != null)
		{
			error = null;
			autoRecovered = true;
			return data;
		}
		data = SaveGame.TryReadSaveFile(file, "_STARDEWVALLEYSAVETMP", out error2);
		if (data != null)
		{
			error = null;
			autoRecovered = true;
			return data;
		}
		error = error ?? "Save could not be loaded";
		autoRecovered = false;
		return null;
	}

	/// <summary>Get an enumerator which loads a save file.</summary>
	/// <param name="file">The save folder name to load, in the form <c>{farmer name}_{unique id}</c>.</param>
	/// <returns>Returns an enumeration of incrementing progress values between 0 and 100.</returns>
	public static IEnumerator<int> getLoadEnumerator(string file)
	{
		SaveGame.LogVerbose("getLoadEnumerator('" + file + "')");
		Stopwatch stopwatch = Stopwatch.StartNew();
		Game1.SetSaveName(Path.GetFileNameWithoutExtension(file).Split('_').FirstOrDefault());
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4690");
		SaveGame.IsProcessing = true;
		if (SaveGame.CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 1;
		string error = null;
		bool autoRecovered = false;
		Task readSaveTask = new Task(delegate
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			SaveGame.loaded = SaveGame.TryReadSaveFileWithFallback(file, out error, out autoRecovered);
		});
		Game1.hooks.StartTask(readSaveTask, "Load_ReadSave");
		while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
		{
			yield return 15;
		}
		if (SaveGame.loaded == null)
		{
			Game1.gameMode = 9;
			Game1.debugOutput = Game1.parseText(error);
			yield break;
		}
		if (autoRecovered)
		{
			SaveGame.LogWarn("Save file " + file + " was corrupted; auto-recovered it from the backup.");
		}
		yield return 19;
		Game1.hasApplied1_3_UpdateChanges = SaveGame.loaded.hasApplied1_3_UpdateChanges;
		Game1.hasApplied1_4_UpdateChanges = SaveGame.loaded.hasApplied1_4_UpdateChanges;
		Game1.lastAppliedSaveFix = (SaveFixes)SaveGame.loaded.lastAppliedSaveFix;
		Game1.player.team.useLegacyRandom.Value = SaveGame.loaded.useLegacyRandom;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4697");
		if (SaveGame.CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 20;
		SaveGame.LoadFarmType();
		Game1.year = SaveGame.loaded.year;
		Game1.netWorldState.Value.CurrentPlayerLimit = Game1.multiplayer.playerLimit;
		if (SaveGame.loaded.highestPlayerLimit >= 0)
		{
			Game1.netWorldState.Value.HighestPlayerLimit = SaveGame.loaded.highestPlayerLimit;
		}
		else
		{
			Game1.netWorldState.Value.HighestPlayerLimit = Math.Max(Game1.netWorldState.Value.HighestPlayerLimit, Game1.multiplayer.MaxPlayers);
		}
		Game1.uniqueIDForThisGame = SaveGame.loaded.uniqueIDForThisGame;
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			Game1.game1.loadForNewGame(loadedGame: true);
		}
		else
		{
			readSaveTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				Game1.game1.loadForNewGame(loadedGame: true);
			});
			Game1.hooks.StartTask(readSaveTask, "Load_LoadForNewGame");
			while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
			{
				yield return 24;
			}
			if (readSaveTask.IsFaulted)
			{
				Exception e = readSaveTask.Exception.GetBaseException();
				SaveGame.LogError("loadNewGameTask failed with an exception.", e);
				throw e;
			}
			if (SaveGame.CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 25;
		}
		Game1.weatherForTomorrow = (int.TryParse(SaveGame.loaded.weatherForTomorrow, out var legacyWeather) ? Utility.LegacyWeatherToWeather(legacyWeather) : SaveGame.loaded.weatherForTomorrow);
		Game1.dayOfMonth = SaveGame.loaded.dayOfMonth;
		Game1.year = SaveGame.loaded.year;
		Game1.currentSeason = SaveGame.loaded.currentSeason;
		Game1.worldStateIDs = SaveGame.loaded.worldStateIDs;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4698");
		if (SaveGame.loaded.mine_permanentMineChanges != null)
		{
			MineShaft.permanentMineChanges = new SerializableDictionary<int, MineInfo>(SaveGame.loaded.mine_permanentMineChanges.ToDictionary());
			Game1.netWorldState.Value.LowestMineLevel = SaveGame.loaded.mine_lowestLevelReached;
			Game1.netWorldState.Value.LowestMineLevelForOrder = SaveGame.loaded.mine_lowestLevelReachedForOrder;
		}
		Game1.currentGemBirdIndex = SaveGame.loaded.currentGemBirdIndex;
		if (SaveGame.loaded.bundleData.Length != 0)
		{
			Dictionary<string, string> bundleData = SaveGame.loaded.bundleData.ToDictionary();
			if (!SaveGame.loaded.HasSaveFix(SaveFixes.StandardizeBundleFields))
			{
				SaveMigrator_1_6.StandardizeBundleFields(bundleData);
			}
			Game1.netWorldState.Value.SetBundleData(bundleData);
		}
		if (SaveGame.CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 26;
		Game1.isRaining = SaveGame.loaded.isRaining;
		Game1.isLightning = SaveGame.loaded.isLightning;
		Game1.isSnowing = SaveGame.loaded.isSnowing;
		Game1.isGreenRain = Utility.isGreenRainDay();
		if (Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.UpdateFromGame1();
		}
		if (SaveGame.loaded.locationWeather != null)
		{
			Game1.netWorldState.Value.LocationWeather.Clear();
			foreach (KeyValuePair<string, LocationWeather> pair in SaveGame.loaded.locationWeather)
			{
				Game1.netWorldState.Value.LocationWeather[pair.Key] = pair.Value;
			}
		}
		if (SaveGame.loaded.builders != null)
		{
			SaveablePair<string, BuilderData>[] array = SaveGame.loaded.builders;
			for (int num = 0; num < array.Length; num++)
			{
				SaveablePair<string, BuilderData> pair2 = array[num];
				Game1.netWorldState.Value.Builders[pair2.Key] = pair2.Value;
			}
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			SaveGame.loadDataToFarmer(SaveGame.loaded.player);
		}
		else
		{
			readSaveTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				SaveGame.loadDataToFarmer(SaveGame.loaded.player);
			});
			Game1.hooks.StartTask(readSaveTask, "Load_Farmer");
			while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
			{
				yield return 1;
			}
			if (readSaveTask.IsFaulted)
			{
				Exception e2 = readSaveTask.Exception.GetBaseException();
				SaveGame.LogError("loadFarmerTask failed with an exception", e2);
				throw e2;
			}
		}
		Game1.player = SaveGame.loaded.player;
		Game1.player.team.useLegacyRandom.Value = SaveGame.loaded.useLegacyRandom;
		Game1.player.team.allowChatCheats.Value = SaveGame.loaded.allowChatCheats;
		Game1.player.team.hasDedicatedHost.Value = SaveGame.loaded.hasDedicatedHost;
		Game1.netWorldState.Value.farmhandData.Clear();
		if (Game1.lastAppliedSaveFix < SaveFixes.MigrateFarmhands)
		{
			SaveMigrator_1_6.MigrateFarmhands(SaveGame.loaded.locations);
		}
		if (SaveGame.loaded.farmhands != null)
		{
			foreach (Farmer farmhand in SaveGame.loaded.farmhands)
			{
				Game1.netWorldState.Value.farmhandData[farmhand.UniqueMultiplayerID] = farmhand;
			}
		}
		foreach (Farmer value in Game1.netWorldState.Value.farmhandData.Values)
		{
			SaveGame.loadDataToFarmer(value);
		}
		if (Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved") && Game1.getLocationFromName("Mountain") is Mountain mountain)
		{
			mountain.reloadMap();
			mountain.ApplyTreehouseIfNecessary();
			if (mountain.treehouseDoorDirty)
			{
				mountain.treehouseDoorDirty = false;
				WarpPathfindingCache.PopulateCache();
			}
		}
		if (SaveGame.loaded.farmerFriendships != null)
		{
			SaveablePair<FarmerPair, Friendship>[] array2 = SaveGame.loaded.farmerFriendships;
			for (int num = 0; num < array2.Length; num++)
			{
				SaveablePair<FarmerPair, Friendship> pair3 = array2[num];
				Game1.player.team.friendshipData[pair3.Key] = pair3.Value;
			}
		}
		Game1.spawnMonstersAtNight = SaveGame.loaded.shouldSpawnMonsters;
		Game1.player.team.limitedNutDrops.Clear();
		if (Game1.netWorldState != null && Game1.netWorldState.Value != null)
		{
			Game1.netWorldState.Value.RegisterSpecialCurrencies();
		}
		if (SaveGame.loaded.limitedNutDrops != null)
		{
			SaveablePair<string, int>[] array3 = SaveGame.loaded.limitedNutDrops;
			for (int num = 0; num < array3.Length; num++)
			{
				SaveablePair<string, int> pair4 = array3[num];
				if (pair4.Value > 0)
				{
					Game1.player.team.limitedNutDrops[pair4.Key] = pair4.Value;
				}
			}
		}
		Game1.player.team.completedSpecialOrders.Clear();
		Game1.player.team.completedSpecialOrders.AddRange(SaveGame.loaded.completedSpecialOrders);
		Game1.player.team.specialOrders.Clear();
		foreach (SpecialOrder order in SaveGame.loaded.specialOrders)
		{
			if (order != null)
			{
				Game1.player.team.specialOrders.Add(order);
			}
		}
		Game1.player.team.availableSpecialOrders.Clear();
		foreach (SpecialOrder order2 in SaveGame.loaded.availableSpecialOrders)
		{
			if (order2 != null)
			{
				Game1.player.team.availableSpecialOrders.Add(order2);
			}
		}
		Game1.player.team.acceptedSpecialOrderTypes.Clear();
		Game1.player.team.acceptedSpecialOrderTypes.AddRange(SaveGame.loaded.acceptedSpecialOrderTypes);
		Game1.player.team.collectedNutTracker.Clear();
		Game1.player.team.collectedNutTracker.AddRange(SaveGame.loaded.collectedNutTracker);
		Game1.player.team.globalInventories.Clear();
		if (SaveGame.loaded.globalInventories != null)
		{
			SaveablePair<string, Item[]>[] array4 = SaveGame.loaded.globalInventories;
			for (int num = 0; num < array4.Length; num++)
			{
				SaveablePair<string, Item[]> pair5 = array4[num];
				Game1.player.team.GetOrCreateGlobalInventory(pair5.Key).AddRange(pair5.Value);
			}
		}
		List<Item> list = SaveGame.loaded.junimoChest;
		if (list != null && list.Count > 0)
		{
			Game1.player.team.GetOrCreateGlobalInventory("JunimoChests").AddRange(SaveGame.loaded.junimoChest);
		}
		Game1.player.team.returnedDonations.Clear();
		foreach (Item donatedItem in SaveGame.loaded.returnedDonations)
		{
			Game1.player.team.returnedDonations.Add(donatedItem);
		}
		if (SaveGame.loaded.obsolete_stats != null)
		{
			Game1.player.stats = SaveGame.loaded.obsolete_stats;
		}
		if (SaveGame.loaded.obsolete_mailbox != null && !Game1.player.mailbox.Any())
		{
			Game1.player.mailbox.AddRange(SaveGame.loaded.obsolete_mailbox);
		}
		Game1.random = Utility.CreateDaySaveRandom(1.0);
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4699");
		if (SaveGame.CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 36;
		Game1.UpdatePassiveFestivalStates();
		if (SaveGame.loaded.cellarAssignments != null)
		{
			SaveablePair<int, long>[] array5 = SaveGame.loaded.cellarAssignments;
			for (int num = 0; num < array5.Length; num++)
			{
				SaveablePair<int, long> pair6 = array5[num];
				Game1.player.team.cellarAssignments[pair6.Key] = pair6.Value;
			}
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			SaveGame.loadDataToLocations(SaveGame.loaded.locations);
		}
		else
		{
			readSaveTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				SaveGame.loadDataToLocations(SaveGame.loaded.locations);
			});
			Game1.hooks.StartTask(readSaveTask, "Load_Locations");
			while (!readSaveTask.IsCanceled && !readSaveTask.IsCompleted && !readSaveTask.IsFaulted)
			{
				yield return 1;
			}
			if (readSaveTask.IsFaulted)
			{
				Exception e3 = readSaveTask.Exception.GetBaseException();
				SaveGame.LogError("loadLocationsTask failed with an exception", e3);
				throw readSaveTask.Exception.GetBaseException();
			}
		}
		if (SaveGame.loaded.shippingBin != null)
		{
			Game1.getFarm().getShippingBin(Game1.player).Clear();
			Game1.getFarm().getShippingBin(Game1.player).AddRange(SaveGame.loaded.shippingBin);
		}
		if (Game1.getLocationFromName("Railroad") is Railroad railroad)
		{
			railroad.ResetTrainForNewDay();
		}
		HashSet<long> validFarmhands = new HashSet<long>();
		Utility.ForEachBuilding(delegate(Building building)
		{
			if (building?.GetIndoors() is Cabin cabin)
			{
				validFarmhands.Add(cabin.farmhandReference.UID);
			}
			return true;
		});
		List<Farmer> orphanedFarmhands = new List<Farmer>();
		foreach (Farmer farmer in Game1.netWorldState.Value.farmhandData.Values)
		{
			if (!farmer.isCustomized.Value && !validFarmhands.Contains(farmer.UniqueMultiplayerID))
			{
				orphanedFarmhands.Add(farmer);
			}
		}
		foreach (Farmer farmer2 in orphanedFarmhands)
		{
			Game1.player.team.DeleteFarmhand(farmer2);
		}
		foreach (Farmer farmer3 in Game1.getAllFarmers())
		{
			int farmerMoney = farmer3.Money;
			if (!Game1.player.team.individualMoney.TryGetValue(farmer3.UniqueMultiplayerID, out var moneyField))
			{
				moneyField = (Game1.player.team.individualMoney[farmer3.UniqueMultiplayerID] = new NetIntDelta(farmerMoney));
			}
			moneyField.Value = farmerMoney;
		}
		Game1.updateCellarAssignments();
		foreach (GameLocation location in Game1.locations)
		{
			foreach (Building building in location.buildings)
			{
				GameLocation indoors = building.GetIndoors();
				if (indoors != null)
				{
					if (indoors is FarmHouse house)
					{
						house.updateCellarWarps();
					}
					indoors.parentLocationName.Value = location.NameOrUniqueName;
				}
			}
			if (location is FarmHouse farmHouse)
			{
				farmHouse.updateCellarWarps();
			}
		}
		foreach (Farmer farmhand2 in Game1.netWorldState.Value.farmhandData.Values)
		{
			Game1.netWorldState.Value.ResetFarmhandState(farmhand2);
		}
		if (SaveGame.CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 50;
		yield return 51;
		Game1.isDebrisWeather = SaveGame.loaded.isDebrisWeather;
		if (Game1.isDebrisWeather)
		{
			Game1.populateDebrisWeatherArray();
		}
		else
		{
			Game1.debrisWeather.Clear();
		}
		yield return 53;
		Game1.player.team.sharedDailyLuck.Value = SaveGame.loaded.dailyLuck;
		yield return 54;
		yield return 55;
		Game1.setGraphicsForSeason(onLoad: true);
		yield return 56;
		Game1.samBandName = SaveGame.loaded.samBandName;
		Game1.elliottBookName = SaveGame.loaded.elliottBookName;
		yield return 63;
		Game1.weddingToday = SaveGame.loaded.weddingToday;
		Game1.weddingsToday = SaveGame.loaded.weddingsToday.ToList();
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4700");
		yield return 64;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4701");
		if (SaveGame.CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 79;
		Game1.options.musicVolumeLevel = SaveGame.loaded.musicVolume;
		Game1.options.soundVolumeLevel = SaveGame.loaded.soundVolume;
		yield return 83;
		if (SaveGame.loaded.countdownToWedding.HasValue && SaveGame.loaded.countdownToWedding.Value != 0 && !string.IsNullOrEmpty(SaveGame.loaded.player.spouse))
		{
			WorldDate weddingDate = WorldDate.Now();
			weddingDate.TotalDays += SaveGame.loaded.countdownToWedding.Value;
			Friendship friendship = SaveGame.loaded.player.friendshipData[SaveGame.loaded.player.spouse];
			friendship.Status = FriendshipStatus.Engaged;
			friendship.WeddingDate = weddingDate;
		}
		yield return 85;
		yield return 87;
		yield return 88;
		yield return 95;
		Game1.fadeToBlack = true;
		Game1.fadeIn = false;
		Game1.fadeToBlackAlpha = 0.99f;
		if (Game1.player.mostRecentBed.X <= 0f)
		{
			Game1.player.Position = new Vector2(192f, 384f);
		}
		Game1.addNewFarmBuildingMaps();
		GameLocation last_sleep_location = null;
		if (Game1.player.lastSleepLocation.Value != null && Game1.isLocationAccessible(Game1.player.lastSleepLocation.Value))
		{
			last_sleep_location = Game1.getLocationFromName(Game1.player.lastSleepLocation.Value);
		}
		bool apply_default_bed_position = true;
		if (last_sleep_location != null && last_sleep_location.CanWakeUpHere(Game1.player))
		{
			Game1.currentLocation = last_sleep_location;
			Game1.player.currentLocation = Game1.currentLocation;
			Game1.player.Position = Utility.PointToVector2(Game1.player.lastSleepPoint.Value) * 64f;
			apply_default_bed_position = false;
		}
		if (apply_default_bed_position)
		{
			Game1.currentLocation = Game1.RequireLocation("FarmHouse");
		}
		Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
		Game1.player.CanMove = true;
		Game1.player.ReequipEnchantments();
		if (SaveGame.loaded.junimoKartLeaderboards != null)
		{
			Game1.player.team.junimoKartScores.LoadScores(SaveGame.loaded.junimoKartLeaderboards.GetScores());
		}
		Game1.options = SaveGame.loaded.options;
		Game1.splitscreenOptions = new SerializableDictionary<long, Options>(SaveGame.loaded.splitscreenOptions.ToDictionary());
		Game1.CustomData = SaveGame.loaded.CustomData;
		Game1.player.team.broadcastedMail.Clear();
		if (SaveGame.loaded.broadcastedMail != null)
		{
			Game1.player.team.broadcastedMail.AddRange(SaveGame.loaded.broadcastedMail);
		}
		Game1.player.team.constructedBuildings.Clear();
		if (SaveGame.loaded.constructedBuildings != null)
		{
			Game1.player.team.constructedBuildings.AddRange(SaveGame.loaded.constructedBuildings);
		}
		if (Game1.options == null)
		{
			Game1.options = new Options();
			Game1.options.LoadDefaultOptions();
		}
		else
		{
			if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
			{
				Game1.options.loadChineseFonts();
			}
			else
			{
				Game1.options.dialogueFontScale = 1f;
			}
			Game1.options.platformClampValues();
			Game1.options.SaveDefaultOptions();
		}
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			Game1.options.gamepadMode = startupPreferences.gamepadMode;
		}
		catch
		{
		}
		Game1.initializeVolumeLevels();
		Game1.multiplayer.latestID = (ulong)SaveGame.loaded.latestID;
		Game1.netWorldState.Value.SkullCavesDifficulty = SaveGame.loaded.skullCavesDifficulty;
		Game1.netWorldState.Value.MinesDifficulty = SaveGame.loaded.minesDifficulty;
		Game1.netWorldState.Value.VisitsUntilY1Guarantee = SaveGame.loaded.visitsUntilY1Guarantee;
		Game1.netWorldState.Value.ShuffleMineChests = SaveGame.loaded.shuffleMineChests;
		Game1.netWorldState.Value.DishOfTheDay = SaveGame.loaded.dishOfTheDay;
		if (Game1.IsRainingHere())
		{
			Game1.changeMusicTrack("rain", track_interruptable: true);
		}
		Game1.updateWeatherIcon();
		Game1.netWorldState.Value.MiniShippingBinsObtained = SaveGame.loaded.miniShippingBinsObtained;
		Game1.netWorldState.Value.LostBooksFound = SaveGame.loaded.lostBooksFound;
		Game1.netWorldState.Value.GoldenWalnuts = SaveGame.loaded.goldenWalnuts;
		Game1.netWorldState.Value.GoldenWalnutsFound = SaveGame.loaded.goldenWalnutsFound;
		Game1.netWorldState.Value.GoldenCoconutCracked = SaveGame.loaded.goldenCoconutCracked;
		Game1.netWorldState.Value.FoundBuriedNuts.Clear();
		Game1.netWorldState.Value.FoundBuriedNuts.AddRange(SaveGame.loaded.foundBuriedNuts);
		Game1.netWorldState.Value.CheckedGarbage.Clear();
		Game1.netWorldState.Value.CheckedGarbage.AddRange(SaveGame.loaded.checkedGarbage);
		IslandSouth.SetupIslandSchedules();
		Game1.netWorldState.Value.TimesFedRaccoons = SaveGame.loaded.timesFedRaccoons;
		Game1.netWorldState.Value.TreasureTotemsUsed = SaveGame.loaded.treasureTotemsUsed;
		Game1.netWorldState.Value.PerfectionWaivers = SaveGame.loaded.perfectionWaivers;
		Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = SaveGame.loaded.seasonOfCurrentRaccoonBundle;
		Game1.netWorldState.Value.raccoonBundles.Set(SaveGame.loaded.raccoonBundles);
		Game1.netWorldState.Value.ActivatedGoldenParrot = SaveGame.loaded.activatedGoldenParrot;
		Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished = SaveGame.loaded.daysPlayedWhenLastRaccoonBundleWasFinished;
		Game1.PerformPassiveFestivalSetup();
		Game1.player.team.farmhandsCanMoveBuildings.Value = (FarmerTeam.RemoteBuildingPermissions)SaveGame.loaded.moveBuildingPermissionMode;
		Game1.player.team.mineShrineActivated.Value = SaveGame.loaded.mineShrineActivated;
		Game1.player.team.skullShrineActivated.Value = SaveGame.loaded.skullShrineActivated;
		if (Game1.multiplayerMode == 2)
		{
			if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.InviteOnly)
			{
				Game1.options.setServerMode("invite");
			}
			else if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.FriendsOnly)
			{
				Game1.options.setServerMode("friends");
			}
			else
			{
				Game1.options.setServerMode("friends");
			}
		}
		Game1.bannedUsers = new SerializableDictionary<string, string>(SaveGame.loaded.bannedUsers.ToDictionary());
		bool num2 = SaveGame.loaded.lostBooksFound < 0;
		SaveGame.loaded = null;
		Game1.currentLocation.lastTouchActionLocation = Game1.player.Tile;
		if (Game1.player.horseName.Value == null)
		{
			Horse horse = Utility.findHorse(Guid.Empty);
			if (horse != null && horse.displayName != "")
			{
				Game1.player.horseName.Value = horse.displayName;
				horse.ownerId.Value = Game1.player.UniqueMultiplayerID;
			}
		}
		SaveMigrator.ApplySaveFixes();
		if (num2)
		{
			SaveMigrator_1_4.RecalculateLostBookCount();
		}
		foreach (Item item in Game1.player.Items)
		{
			(item as Object)?.reloadSprite();
		}
		foreach (Trinket trinketItem in Game1.player.trinketItems)
		{
			trinketItem.reloadSprite();
		}
		Game1.gameMode = 3;
		Game1.AddNPCs();
		Game1.AddModNPCs();
		Game1.RefreshQuestOfTheDay();
		try
		{
			Game1.fixProblems();
		}
		catch (Exception exception)
		{
			Game1.log.Error("Failed to fix problems.", exception);
		}
		Utility.ForEachBuilding(delegate(Building building)
		{
			if (building is Stable stable)
			{
				stable.grabHorse();
			}
			else
			{
				GameLocation indoors2 = building.GetIndoors();
				if (!(indoors2 is Cabin cabin))
				{
					if (indoors2 is Shed shed)
					{
						shed.updateLayout();
						building.updateInteriorWarps(shed);
					}
				}
				else
				{
					cabin.updateFarmLayout();
				}
			}
			return true;
		});
		Game1.UpdateHorseOwnership();
		Game1.UpdateFarmPerfection();
		Game1.doMorningStuff();
		if (apply_default_bed_position && Game1.player.currentLocation is FarmHouse farmhouse)
		{
			Game1.player.Position = Utility.PointToVector2(farmhouse.GetPlayerBedSpot()) * 64f;
		}
		BedFurniture.ShiftPositionForBed(Game1.player);
		Game1.stats.checkForAchievements();
		if (Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.UpdateFromGame1();
		}
		SaveGame.LogVerbose("getLoadEnumerator() exited, elapsed = '" + stopwatch.Elapsed.ToString() + "'");
		if (SaveGame.CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		SaveGame.IsProcessing = false;
		Game1.player.currentLocation.lastTouchActionLocation = Game1.player.Tile;
		if (Game1.IsMasterGame)
		{
			Game1.player.currentLocation.hostSetup();
			Game1.player.currentLocation.interiorDoors.ResetSharedState();
		}
		Game1.player.currentLocation.resetForPlayerEntry();
		Game1.player.sleptInTemporaryBed.Value = false;
		Game1.player.showToolUpgradeAvailability();
		Game1.player.resetAllTrinketEffects();
		Game1.dayTimeMoneyBox.questsDirty = true;
		yield return 100;
	}

	public static void loadDataToFarmer(Farmer target)
	{
		target.gameVersion = target.gameVersion;
		target.Items.OverwriteWith(target.Items);
		target.canMove = true;
		target.Sprite = new FarmerSprite(null);
		target.songsHeard.Add("title_day");
		target.songsHeard.Add("title_night");
		target.maxItems.Value = target.maxItems.Value;
		for (int i = 0; i < target.maxItems.Value; i++)
		{
			if (target.Items.Count <= i)
			{
				target.Items.Add(null);
			}
		}
		if (target.FarmerRenderer == null)
		{
			target.FarmerRenderer = new FarmerRenderer(target.getTexture(), target);
		}
		target.changeGender(target.IsMale);
		target.changeAccessory(target.accessory.Value);
		target.changeShirt(target.shirt.Value);
		target.changePantsColor(target.GetPantsColor());
		target.changeSkinColor(target.skin.Value);
		target.changeHairColor(target.hairstyleColor.Value);
		target.changeHairStyle(target.hair.Value);
		target.changeShoeColor(target.shoes.Value);
		target.changeEyeColor(target.newEyeColor.Value);
		target.Stamina = target.Stamina;
		target.health = target.health;
		target.maxStamina.Value = target.maxStamina.Value;
		target.mostRecentBed = target.mostRecentBed;
		target.Position = target.mostRecentBed;
		target.position.X -= 64f;
		if (!Game1.hasApplied1_3_UpdateChanges)
		{
			SaveMigrator_1_3.MigrateFriendshipData(target);
		}
		target.questLog.RemoveWhere((Quest quest) => quest == null);
		target.ConvertClothingOverrideToClothesItems();
		target.UpdateClothing();
		target._lastEquippedTool = target.CurrentTool;
	}

	public static void loadDataToLocations(List<GameLocation> fromLocations)
	{
		Dictionary<string, string> formerLocationNames = SaveGame.GetFormerLocationNames();
		if (formerLocationNames.Count > 0)
		{
			foreach (GameLocation fromLocation2 in fromLocations)
			{
				foreach (NPC npc in fromLocation2.characters)
				{
					string curHome = npc.DefaultMap;
					if (curHome != null && formerLocationNames.TryGetValue(curHome, out var newHome))
					{
						SaveGame.LogDebug($"Updated {npc.Name}'s home from '{curHome}' to '{newHome}'.");
						npc.DefaultMap = newHome;
					}
				}
			}
		}
		Game1.netWorldState.Value.ParrotPlatformsUnlocked = SaveGame.loaded.parrotPlatformsUnlocked;
		Game1.player.team.farmPerfect.Value = SaveGame.loaded.farmPerfect;
		List<GameLocation> loadedLocations = new List<GameLocation>();
		Dictionary<string, Tuple<NPC, GameLocation>> lostVillagers = new Dictionary<string, Tuple<NPC, GameLocation>>();
		foreach (GameLocation fromLocation in fromLocations)
		{
			GameLocation realLocation = Game1.getLocationFromName(fromLocation.name.Value);
			if (realLocation == null)
			{
				if (fromLocation is Cellar)
				{
					realLocation = Game1.CreateGameLocation("Cellar");
					if (realLocation == null)
					{
						SaveGame.LogError("Couldn't create 'Cellar' location. Was it removed from Data/Locations?");
						continue;
					}
					realLocation.name.Value = fromLocation.name.Value;
					Game1.locations.Add(realLocation);
				}
				if (realLocation == null && formerLocationNames.TryGetValue(fromLocation.name.Value, out var realLocationName))
				{
					SaveGame.LogDebug($"Mapped legacy location '{fromLocation.Name}' to '{realLocationName}'.");
					realLocation = Game1.getLocationFromName(realLocationName);
				}
				if (realLocation == null)
				{
					List<string> npcNames = new List<string>();
					foreach (NPC npc2 in fromLocation.characters)
					{
						if (npc2.IsVillager && npc2.Name != null)
						{
							npcNames.Add(npc2.Name);
							lostVillagers[npc2.Name] = Tuple.Create(npc2, fromLocation);
						}
					}
					Game1.log.Warn($"Ignored unknown location '{fromLocation.NameOrUniqueName}' in save data{((npcNames.Count > 0) ? $", including NPC{((npcNames.Count > 1) ? "s" : "")} '{string.Join("', '", npcNames.OrderBy((string p) => p))}'" : "")}.");
					continue;
				}
			}
			if (!(realLocation is Farm farm))
			{
				if (!(realLocation is FarmHouse farmHouse))
				{
					if (!(realLocation is Forest forest))
					{
						if (!(realLocation is MovieTheater theater))
						{
							if (!(realLocation is Town town))
							{
								if (!(realLocation is Beach beach))
								{
									if (!(realLocation is Woods woods))
									{
										if (!(realLocation is CommunityCenter communityCenter))
										{
											if (realLocation is ShopLocation shopLocation && fromLocation is ShopLocation fromShopLocation)
											{
												shopLocation.itemsFromPlayerToSell.MoveFrom(fromShopLocation.itemsFromPlayerToSell);
												shopLocation.itemsToStartSellingTomorrow.MoveFrom(fromShopLocation.itemsToStartSellingTomorrow);
											}
										}
										else if (fromLocation is CommunityCenter fromCommunityCenter)
										{
											communityCenter.areasComplete.Set(fromCommunityCenter.areasComplete);
										}
									}
									else if (fromLocation is Woods fromWoods)
									{
										woods.hasUnlockedStatue.Value = fromWoods.hasUnlockedStatue.Value;
									}
								}
								else if (fromLocation is Beach fromBeach)
								{
									beach.bridgeFixed.Value = fromBeach.bridgeFixed.Value;
								}
							}
							else if (fromLocation is Town fromTown)
							{
								town.daysUntilCommunityUpgrade.Value = fromTown.daysUntilCommunityUpgrade.Value;
							}
						}
						else if (fromLocation is MovieTheater fromTheater)
						{
							theater.dayFirstEntered.Set(fromTheater.dayFirstEntered.Value);
						}
					}
					else if (fromLocation is Forest fromForest)
					{
						forest.stumpFixed.Value = fromForest.stumpFixed.Value;
						forest.obsolete_log = fromForest.obsolete_log;
					}
				}
				else if (fromLocation is FarmHouse fromFarmHouse)
				{
					farmHouse.setMapForUpgradeLevel(farmHouse.upgradeLevel);
					farmHouse.fridge.Value = fromFarmHouse.fridge.Value;
					farmHouse.ReadWallpaperAndFloorTileData();
				}
			}
			else if (fromLocation is Farm fromFarm)
			{
				farm.greenhouseUnlocked.Value = fromFarm.greenhouseUnlocked.Value;
				farm.greenhouseMoved.Value = fromFarm.greenhouseMoved.Value;
				farm.hasSeenGrandpaNote = fromFarm.hasSeenGrandpaNote;
				farm.grandpaScore.Value = fromFarm.grandpaScore.Value;
				farm.UpdatePatio();
			}
			realLocation.TransferDataFromSavedLocation(fromLocation);
			realLocation.animals.MoveFrom(fromLocation.animals);
			realLocation.buildings.Set(fromLocation.buildings);
			realLocation.characters.Set(fromLocation.characters);
			realLocation.furniture.Set(fromLocation.furniture);
			realLocation.largeTerrainFeatures.Set(fromLocation.largeTerrainFeatures);
			realLocation.miniJukeboxCount.Value = fromLocation.miniJukeboxCount.Value;
			realLocation.miniJukeboxTrack.Value = fromLocation.miniJukeboxTrack.Value;
			realLocation.netObjects.Set(fromLocation.netObjects.Pairs);
			realLocation.numberOfSpawnedObjectsOnMap = fromLocation.numberOfSpawnedObjectsOnMap;
			realLocation.piecesOfHay.Value = fromLocation.piecesOfHay.Value;
			realLocation.resourceClumps.Set(new List<ResourceClump>(fromLocation.resourceClumps));
			realLocation.terrainFeatures.Set(fromLocation.terrainFeatures.Pairs);
			if (!SaveGame.loaded.HasSaveFix(SaveFixes.MigrateBuildingsToData))
			{
				SaveMigrator_1_6.ConvertBuildingsToData(realLocation);
			}
			loadedLocations.Add(realLocation);
		}
		SaveGame.MigrateLostVillagers(lostVillagers);
		foreach (GameLocation realLocation2 in loadedLocations)
		{
			realLocation2.AddDefaultBuildings(load: false);
			foreach (Building b in realLocation2.buildings)
			{
				b.load();
				if (b.GetIndoorsType() == IndoorsType.Instanced)
				{
					b.GetIndoors()?.addLightGlows();
				}
			}
			foreach (FarmAnimal value in realLocation2.animals.Values)
			{
				value.reload((GameLocation)null);
			}
			foreach (Furniture item in realLocation2.furniture)
			{
				item.updateDrawPosition();
			}
			foreach (LargeTerrainFeature largeTerrainFeature in realLocation2.largeTerrainFeatures)
			{
				largeTerrainFeature.Location = realLocation2;
				largeTerrainFeature.loadSprite();
			}
			foreach (TerrainFeature value2 in realLocation2.terrainFeatures.Values)
			{
				value2.Location = realLocation2;
				value2.loadSprite();
				if (value2 is HoeDirt hoe_dirt)
				{
					hoe_dirt.updateNeighbors();
				}
			}
			foreach (KeyValuePair<Vector2, Object> v in realLocation2.objects.Pairs)
			{
				v.Value.initializeLightSource(v.Key);
				v.Value.reloadSprite();
			}
			realLocation2.addLightGlows();
			if (!(realLocation2 is IslandLocation islandLocation))
			{
				if (realLocation2 is FarmCave farmCave)
				{
					farmCave.UpdateReadyFlag();
				}
			}
			else
			{
				islandLocation.AddAdditionalWalnutBushes();
			}
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			if (location.characters.Count > 0)
			{
				NPC[] array = location.characters.ToArray();
				foreach (NPC obj in array)
				{
					SaveGame.initializeCharacter(obj, location);
					obj.reloadSprite();
				}
			}
			return true;
		});
		Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
	}

	public static void initializeCharacter(NPC c, GameLocation location)
	{
		c.currentLocation = location;
		c.reloadData();
		if (!c.DefaultPosition.Equals(Vector2.Zero))
		{
			c.Position = c.DefaultPosition;
		}
	}

	/// <summary>Migrate villager NPCs from the save file based on their <see cref="T:StardewValley.GameData.Characters.CharacterData" /> data.</summary>
	/// <param name="lostVillagers">The villager NPCs from the save data which were in a location that no longer exists.</param>
	public static void MigrateLostVillagers(Dictionary<string, Tuple<NPC, GameLocation>> lostVillagers)
	{
		Dictionary<string, string> npcNamesByFormerName = SaveGame.GetFormerNpcNames((string name, CharacterData _) => Game1.getCharacterFromName(name) == null);
		foreach (KeyValuePair<string, Tuple<NPC, GameLocation>> pair in lostVillagers)
		{
			NPC npc = pair.Value.Item1;
			GameLocation lostLocation = pair.Value.Item2;
			if (Game1.getCharacterFromName(npc.Name) == null && (!npcNamesByFormerName.TryGetValue(npc.Name, out var newName) || Game1.getCharacterFromName(newName) == null) && NPC.TryGetData(newName ?? npc.Name, out var _))
			{
				GameLocation home = null;
				string oldName = npc.Name;
				npc.Name = newName ?? oldName;
				_ = npc.DefaultMap;
				npc.reloadDefaultLocation();
				try
				{
					home = npc.getHome();
				}
				catch (Exception)
				{
					continue;
				}
				npc.Name = oldName;
				if (home != null)
				{
					home.characters.Add(npc);
					npc.currentLocation = home;
					npc.position.Value = npc.DefaultPosition * 64f;
					Game1.log.Debug($"Moved NPC '{npc.Name}' from deleted location '{lostLocation.Name}' to their new home in '{npc.currentLocation.Name}'.");
				}
			}
		}
		foreach (KeyValuePair<string, string> pair2 in npcNamesByFormerName)
		{
			string oldName2 = pair2.Key;
			string newName2 = pair2.Value;
			NPC npc2 = Game1.getCharacterFromName(oldName2);
			if (npc2 == null)
			{
				continue;
			}
			npc2.Name = newName2;
			foreach (Farmer player in Game1.getAllFarmers())
			{
				if (player.spouse == oldName2)
				{
					player.spouse = newName2;
				}
				if (player.friendshipData.TryGetValue(oldName2, out var friendship))
				{
					player.friendshipData.Remove(oldName2);
					player.friendshipData.TryAdd(newName2, friendship);
				}
				if (player.giftedItems.TryGetValue(oldName2, out var giftedItems))
				{
					player.giftedItems.Remove(oldName2);
					player.giftedItems.TryAdd(newName2, giftedItems);
				}
			}
			Game1.log.Debug($"Migrated legacy NPC '{oldName2}' in save data to '{newName2}'.");
		}
	}

	/// <summary>Get a lookup of former → new location names based on their <see cref="F:StardewValley.GameData.Locations.LocationData.FormerLocationNames" /> field.</summary>
	public static Dictionary<string, string> GetFormerLocationNames()
	{
		Dictionary<string, string> formerNames = new Dictionary<string, string>();
		foreach (KeyValuePair<string, LocationData> pair in Game1.locationData)
		{
			LocationData data = pair.Value;
			List<string> formerLocationNames = data.FormerLocationNames;
			if (formerLocationNames == null || formerLocationNames.Count <= 0)
			{
				continue;
			}
			foreach (string formerName in data.FormerLocationNames)
			{
				string conflictingId;
				if (Game1.locationData.ContainsKey(formerName))
				{
					SaveGame.LogError($"Location '{pair.Key}' in Data/Locations has former name '{formerName}', which can't be added because there's a location with that ID in Data/Locations.");
				}
				else if (formerNames.TryGetValue(formerName, out conflictingId))
				{
					if (conflictingId != pair.Key)
					{
						SaveGame.LogError($"Location '{pair.Key}' in Data/Locations has former name '{formerName}', which can't be added because that name is already mapped to '{conflictingId}'.");
					}
				}
				else
				{
					formerNames[formerName] = pair.Key;
				}
			}
		}
		return formerNames;
	}

	/// <summary>Get a lookup of former → new NPC names based on their <see cref="F:StardewValley.GameData.Characters.CharacterData.FormerCharacterNames" /> field.</summary>
	/// <param name="filter">A filter to apply to the list of NPCs with former names.</param>
	public static Dictionary<string, string> GetFormerNpcNames(Func<string, CharacterData, bool> filter)
	{
		Dictionary<string, string> formerNames = new Dictionary<string, string>();
		foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
		{
			CharacterData data = pair.Value;
			List<string> formerCharacterNames = data.FormerCharacterNames;
			if (formerCharacterNames == null || formerCharacterNames.Count <= 0 || !filter(pair.Key, data))
			{
				continue;
			}
			foreach (string formerName in data.FormerCharacterNames)
			{
				string conflictingId;
				if (Game1.characterData.ContainsKey(formerName))
				{
					SaveGame.LogError($"NPC '{pair.Key}' in Data/Characters has former name '{formerName}', which can't be added because there's an NPC with that ID in Data/Characters.");
				}
				else if (formerNames.TryGetValue(formerName, out conflictingId))
				{
					if (conflictingId != pair.Key)
					{
						SaveGame.LogError($"NPC '{pair.Key}' in Data/Characters has former name '{formerName}', which can't be added because that name is already mapped to '{conflictingId}'.");
					}
				}
				else
				{
					formerNames[formerName] = pair.Key;
				}
			}
		}
		return formerNames;
	}

	/// <inheritdoc cref="M:StardewValley.Logging.IGameLogger.Verbose(System.String)" />
	private static void LogVerbose(string message)
	{
		Game1.log.Verbose(message);
	}

	/// <inheritdoc cref="M:StardewValley.Logging.IGameLogger.Debug(System.String)" />
	private static void LogDebug(string message)
	{
		Game1.log.Debug(message);
	}

	/// <inheritdoc cref="M:StardewValley.Logging.IGameLogger.Warn(System.String)" />
	private static void LogWarn(string message)
	{
		Game1.log.Warn(message);
	}

	/// <inheritdoc cref="M:StardewValley.Logging.IGameLogger.Error(System.String,System.Exception)" />
	private static void LogError(string message, Exception exception = null)
	{
		Game1.log.Error(message, exception);
	}
}
