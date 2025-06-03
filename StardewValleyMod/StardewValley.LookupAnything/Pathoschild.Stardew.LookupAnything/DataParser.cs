using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Machines;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything;

internal class DataParser
{
	public const string ComplexRecipeId = "__COMPLEX_RECIPE__";

	public IEnumerable<BundleModel> GetBundles(IMonitor monitor)
	{
		foreach (var (key, value) in ((NetFieldBase<NetWorldState, NetRef<NetWorldState>>)(object)Game1.netWorldState).Value.BundleData)
		{
			if (value == null)
			{
				continue;
			}
			BundleModel bundle;
			try
			{
				string[] keyParts = key.Split('/');
				string area = ArgUtility.Get(keyParts, 0, (string)null, true);
				int id = ArgUtility.GetInt(keyParts, 1, 0);
				string[] valueParts = value.Split('/');
				string name = ArgUtility.Get(valueParts, 0, (string)null, true);
				string reward = ArgUtility.Get(valueParts, 1, (string)null, true);
				string displayName = ArgUtility.Get(valueParts, 6, (string)null, true);
				List<BundleIngredientModel> ingredients = new List<BundleIngredientModel>();
				string[] ingredientData = ArgUtility.SplitBySpace(ArgUtility.Get(valueParts, 2, (string)null, true));
				for (int i = 0; i < ingredientData.Length; i += 3)
				{
					int index = i / 3;
					string itemID = ArgUtility.Get(ingredientData, i, (string)null, true);
					int stack = ArgUtility.GetInt(ingredientData, i + 1, 0);
					ItemQuality quality = ArgUtility.GetEnum<ItemQuality>(ingredientData, i + 2, ItemQuality.Normal);
					ingredients.Add(new BundleIngredientModel(index, itemID, stack, quality));
				}
				bundle = new BundleModel(id, name, displayName, area, reward, ingredients.ToArray());
			}
			catch (Exception value2)
			{
				monitor.LogOnce($"Couldn't parse community center bundle '{key}' due to an invalid format.\nRecipe data: '{value}'\nError: {value2}", (LogLevel)3);
				continue;
			}
			yield return bundle;
		}
	}

	public IEnumerable<FishPondPopulationGateData> GetFishPondPopulationGates(FishPondData data)
	{
		if (data.PopulationGates == null)
		{
			yield break;
		}
		foreach (var (minPopulation, rawData) in data.PopulationGates)
		{
			if (rawData == null)
			{
				continue;
			}
			FishPondPopulationGateQuestItemData[] questItems = rawData.Select(delegate(string entry)
			{
				string[] array = ArgUtility.SplitBySpace(entry);
				int num2 = array.Length;
				if ((num2 < 1 || num2 > 3) ? true : false)
				{
					return (FishPondPopulationGateQuestItemData)null;
				}
				string itemID = ArgUtility.Get(array, 0, (string)null, true);
				int val = ArgUtility.GetInt(array, 1, 1);
				int val2 = ArgUtility.GetInt(array, 2, 1);
				val = Math.Max(1, val);
				val2 = Math.Max(1, val2);
				if (val2 < val)
				{
					val2 = val;
				}
				return new FishPondPopulationGateQuestItemData(itemID, val, val2);
			}).WhereNotNull().ToArray();
			yield return new FishPondPopulationGateData(minPopulation, questItems);
		}
	}

	public IEnumerable<FishPondDropData> GetFishPondDrops(FishPondData data)
	{
		if (data.ProducedItems == null)
		{
			yield break;
		}
		foreach (FishPondReward drop in data.ProducedItems)
		{
			if (drop == null)
			{
				continue;
			}
			IList<ItemQueryResult> itemQueryResults = ItemQueryResolver.TryResolve((ISpawnItemData)(object)drop, new ItemQueryContext(), (ItemQuerySearchMode)1, false, (HashSet<string>)null, (Func<string, string>)null, (Action<string, string>)null, (Item)null);
			float chance = drop.Chance * (1f / (float)itemQueryResults.Count);
			foreach (ItemQueryResult result in itemQueryResults)
			{
				ISalable item = result.Item;
				Item item2 = (Item)(object)((item is Item) ? item : null);
				if (item2 != null)
				{
					yield return new FishPondDropData(drop.RequiredPopulation, drop.Precedence, item2, ((GenericSpawnItemData)drop).MinStack, ((GenericSpawnItemData)drop).MaxStack, chance, ((GenericSpawnItemDataWithCondition)drop).Condition);
				}
			}
		}
	}

	public FishSpawnData GetFishSpawnRules(ParsedItemData fish, Metadata metadata)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		List<FishSpawnLocationData> locations = new List<FishSpawnLocationData>();
		bool isLegendaryFamily = false;
		string key = default(string);
		foreach (KeyValuePair<string, LocationData> locationDatum in Game1.locationData)
		{
			locationDatum.Deconstruct(out key, out var value);
			string locationId = key;
			LocationData data = value;
			if (metadata.IgnoreFishingLocations.Contains(locationId))
			{
				continue;
			}
			List<FishSpawnLocationData> curLocations = new List<FishSpawnLocationData>();
			if (data?.Fish != null)
			{
				foreach (SpawnFishData spawn in data.Fish)
				{
					if (spawn == null)
					{
						continue;
					}
					ParsedItemData spawnItemData = ItemRegistry.GetData(((GenericSpawnItemData)spawn).ItemId);
					if (spawnItemData?.ObjectType != "Fish" || spawnItemData.QualifiedItemId != fish.QualifiedItemId)
					{
						continue;
					}
					if (spawn.Season.HasValue)
					{
						curLocations.Add(new FishSpawnLocationData(locationId, spawn.FishAreaId, new string[1] { ((object)spawn.Season.Value/*cast due to .constrained prefix*/).ToString() }));
					}
					else if (((GenericSpawnItemDataWithCondition)spawn).Condition != null)
					{
						ParsedGameStateQuery[] array = GameStateQuery.Parse(((GenericSpawnItemDataWithCondition)spawn).Condition);
						foreach (ParsedGameStateQuery condition in array)
						{
							if (condition.Query.Length == 0)
							{
								continue;
							}
							if (GameStateQuery.SeasonQueryKeys.Contains(condition.Query[0]))
							{
								List<string> seasons = new List<string>();
								string[] array2 = new string[4] { "spring", "summer", "fall", "winter" };
								foreach (string season in array2)
								{
									if (!condition.Negated && condition.Query.Any((string word) => word.Equals(season, StringComparison.OrdinalIgnoreCase)))
									{
										seasons.Add(season);
									}
								}
								curLocations.Add(new FishSpawnLocationData(locationId, spawn.FishAreaId, seasons.ToArray()));
							}
							else if (!isLegendaryFamily && !condition.Negated)
							{
								string[] query = condition.Query;
								if (query != null && query.Length == 3 && query[0] == "PLAYER_SPECIAL_ORDER_RULE_ACTIVE" && query[1] == "Current" && query[2] == "LEGENDARY_FAMILY")
								{
									isLegendaryFamily = true;
								}
							}
						}
					}
					else
					{
						curLocations.Add(new FishSpawnLocationData(locationId, spawn.FishAreaId, new string[4] { "spring", "summer", "fall", "winter" }));
					}
				}
			}
			if (curLocations.Count <= 0)
			{
				continue;
			}
			locations.AddRange(from p in curLocations
				group p by p.Area into areaGroup
				let seasons = areaGroup.SelectMany((FishSpawnLocationData p) => p.Seasons).Distinct().ToArray()
				select new FishSpawnLocationData(locationId, areaGroup.Key, seasons));
		}
		List<FishSpawnTimeOfDayData> timesOfDay = new List<FishSpawnTimeOfDayData>();
		FishSpawnWeather weather = FishSpawnWeather.Both;
		int minFishingLevel = 0;
		bool isUnique = false;
		if (ItemExtensions.HasTypeObject((IHaveItemTypeId)(object)fish) && locations.Any() && DataLoader.Fish(Game1.content).TryGetValue(fish.ItemId, out var rawData) && rawData != null)
		{
			string[] fishFields = rawData.Split('/');
			string[] timeFields = ArgUtility.SplitBySpace(ArgUtility.Get(fishFields, 5, (string)null, true));
			int i2 = 0;
			int minTime = default(int);
			int maxTime = default(int);
			for (int last = timeFields.Length + 1; i2 + 1 < last; i2 += 2)
			{
				if (ArgUtility.TryGetInt(timeFields, i2, ref minTime, ref key, "int minTime") && ArgUtility.TryGetInt(timeFields, i2 + 1, ref maxTime, ref key, "int maxTime"))
				{
					timesOfDay.Add(new FishSpawnTimeOfDayData(minTime, maxTime));
				}
			}
			if (!ArgUtility.TryGetEnum<FishSpawnWeather>(fishFields, 7, ref weather, ref key, "weather"))
			{
				weather = FishSpawnWeather.Both;
			}
			if (!ArgUtility.TryGetInt(fishFields, 12, ref minFishingLevel, ref key, "minFishingLevel"))
			{
				minFishingLevel = 0;
			}
		}
		if (metadata.CustomFishSpawnRules.TryGetValue(fish.QualifiedItemId, out FishSpawnData customRules))
		{
			if (customRules.MinFishingLevel > minFishingLevel)
			{
				minFishingLevel = customRules.MinFishingLevel;
			}
			if (customRules.Weather != FishSpawnWeather.Unknown)
			{
				weather = customRules.Weather;
			}
			isUnique = isUnique || customRules.IsUnique;
			if (customRules.TimesOfDay != null)
			{
				timesOfDay.AddRange(customRules.TimesOfDay);
			}
			if (customRules.Locations != null)
			{
				locations.AddRange(customRules.Locations);
			}
		}
		return new FishSpawnData(fish, locations.ToArray(), timesOfDay.ToArray(), weather, minFishingLevel, isUnique, isLegendaryFamily);
	}

	public IEnumerable<FishSpawnData> GetFishSpawnRules(GameLocation location, Vector2 tile, string fishAreaId, Metadata metadata)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		HashSet<string> seenFishIds = new HashSet<string>();
		List<SpawnFishData> locationFish = location.GetData()?.Fish;
		if (locationFish != null)
		{
			foreach (SpawnFishData fishData in locationFish)
			{
				if (((fishData != null) ? ((GenericSpawnItemData)fishData).ItemId : null) == null)
				{
					continue;
				}
				seenFishIds.Add(((GenericSpawnItemData)fishData).ItemId);
				if (fishData.FishAreaId != null && fishData.FishAreaId != fishAreaId)
				{
					continue;
				}
				Rectangle? bobberPosition = fishData.BobberPosition;
				bool? obj;
				Rectangle valueOrDefault;
				if (!bobberPosition.HasValue)
				{
					obj = null;
				}
				else
				{
					valueOrDefault = bobberPosition.GetValueOrDefault();
					obj = ((Rectangle)(ref valueOrDefault)).Contains(tile);
				}
				if (!(obj ?? true))
				{
					continue;
				}
				bobberPosition = fishData.PlayerPosition;
				bool? obj2;
				if (!bobberPosition.HasValue)
				{
					obj2 = null;
				}
				else
				{
					valueOrDefault = bobberPosition.GetValueOrDefault();
					obj2 = ((Rectangle)(ref valueOrDefault)).Contains(((Character)Game1.player).TilePoint);
				}
				if (obj2 ?? true)
				{
					ParsedItemData fish = ItemRegistry.GetDataOrErrorItem(((GenericSpawnItemData)fishData).ItemId);
					if (!(fish.ObjectType != "Fish"))
					{
						yield return this.GetFishSpawnRules(fish, metadata);
					}
				}
			}
		}
		foreach (var (fishId, spawnData) in metadata.CustomFishSpawnRules)
		{
			if (!seenFishIds.Contains(fishId) && spawnData.MatchesLocation(location.Name))
			{
				ParsedItemData fish2 = ItemRegistry.GetDataOrErrorItem(fishId);
				yield return this.GetFishSpawnRules(fish2, metadata);
			}
		}
	}

	public FriendshipModel GetFriendshipForVillager(Farmer player, NPC npc, Friendship friendship, Metadata metadata)
	{
		return new FriendshipModel(player, npc, friendship, metadata.Constants);
	}

	public FriendshipModel GetFriendshipForPet(Farmer player, Pet pet)
	{
		return new FriendshipModel(((NetFieldBase<int, NetInt>)(object)pet.friendshipTowardFarmer).Value, 100, 1000);
	}

	public FriendshipModel GetFriendshipForAnimal(Farmer player, FarmAnimal animal, Metadata metadata)
	{
		return new FriendshipModel(((NetFieldBase<int, NetInt>)(object)animal.friendshipTowardFarmer).Value, metadata.Constants.AnimalFriendshipPointsPerLevel, metadata.Constants.AnimalFriendshipMaxPoints);
	}

	public string GetLocationDisplayName(FishSpawnLocationData fishSpawnData)
	{
		if (!Game1.locationData.TryGetValue(fishSpawnData.LocationId, out var locationData))
		{
			locationData = null;
		}
		return this.GetLocationDisplayName(fishSpawnData.LocationId, locationData, fishSpawnData.Area);
	}

	public string GetLocationDisplayName(string id, LocationData? data)
	{
		string name = Translation.op_Implicit(I18n.GetByKey("location." + id).UsePlaceholder(false));
		if (!string.IsNullOrWhiteSpace(name))
		{
			return name;
		}
		if (data != null)
		{
			string name2 = TokenParser.ParseText(data.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null);
			if (!string.IsNullOrWhiteSpace(name2))
			{
				return name2;
			}
		}
		return id;
	}

	public string GetLocationDisplayName(string id, LocationData? data, string? fishAreaId)
	{
		int mineLevel = default(int);
		if (MineShaft.IsGeneratedLevel(id, ref mineLevel))
		{
			string level = fishAreaId ?? mineLevel.ToString();
			if (string.IsNullOrWhiteSpace(level))
			{
				return this.GetLocationDisplayName(id, data);
			}
			return I18n.Location_UndergroundMine_Level(level);
		}
		if (string.IsNullOrWhiteSpace(fishAreaId))
		{
			return this.GetLocationDisplayName(id, data);
		}
		string locationName = this.GetLocationDisplayName(id, data);
		object obj;
		if (data == null)
		{
			obj = null;
		}
		else
		{
			Dictionary<string, FishAreaData> fishAreas = data.FishAreas;
			if (fishAreas == null)
			{
				obj = null;
			}
			else
			{
				FishAreaData? valueOrDefault = fishAreas.GetValueOrDefault(fishAreaId);
				obj = ((valueOrDefault != null) ? valueOrDefault.DisplayName : null);
			}
		}
		string areaName = TokenParser.ParseText((string)obj, (Random)null, (TokenParserDelegate)null, (Farmer)null);
		string displayName = Translation.op_Implicit(I18n.GetByKey("location." + id + "." + fishAreaId, new { locationName }).UsePlaceholder(false));
		if (string.IsNullOrWhiteSpace(displayName))
		{
			displayName = ((!string.IsNullOrWhiteSpace(areaName)) ? I18n.Location_FishArea(locationName, areaName) : I18n.Location_UnknownFishArea(locationName, fishAreaId));
		}
		return displayName;
	}

	public IEnumerable<MonsterData> GetMonsters()
	{
		foreach (var (name, rawData) in DataLoader.Monsters(Game1.content))
		{
			if (rawData == null)
			{
				continue;
			}
			string[] fields = rawData.Split('/');
			int health = ArgUtility.GetInt(fields, 0, 0);
			int damageToFarmer = ArgUtility.GetInt(fields, 1, 0);
			bool isGlider = ArgUtility.GetBool(fields, 4, false);
			int resilience = ArgUtility.GetInt(fields, 7, 0);
			double jitteriness = ArgUtility.GetFloat(fields, 8, 0f);
			int moveTowardsPlayerThreshold = ArgUtility.GetInt(fields, 9, 0);
			int speed = ArgUtility.GetInt(fields, 10, 0);
			double missChance = ArgUtility.GetFloat(fields, 11, 0f);
			bool isMineMonster = ArgUtility.GetBool(fields, 12, false);
			List<ItemDropData> drops = new List<ItemDropData>();
			string[] dropFields = ArgUtility.SplitBySpace(ArgUtility.Get(fields, 6, (string)null, true));
			for (int i = 0; i < dropFields.Length; i += 2)
			{
				string itemId = ArgUtility.Get(dropFields, i, (string)null, true);
				float chance = ArgUtility.GetFloat(dropFields, i + 1, 0f);
				int maxDrops = 1;
				if (int.TryParse(itemId, out var id) && id < 0)
				{
					itemId = (-id).ToString();
					maxDrops = 3;
				}
				if (itemId == 0.ToString())
				{
					itemId = 378.ToString();
				}
				else if (itemId == 2.ToString())
				{
					itemId = 380.ToString();
				}
				else if (itemId == 4.ToString())
				{
					itemId = 382.ToString();
				}
				else if (itemId == 6.ToString())
				{
					itemId = 384.ToString();
				}
				else
				{
					if (itemId == 8.ToString())
					{
						continue;
					}
					if (itemId == 10.ToString())
					{
						itemId = 386.ToString();
					}
					else if (itemId == 12.ToString())
					{
						itemId = 388.ToString();
					}
					else if (itemId == 14.ToString())
					{
						itemId = 390.ToString();
					}
				}
				drops.Add(new ItemDropData(itemId, 1, maxDrops, chance));
			}
			if (isMineMonster && Game1.player.timesReachedMineBottom >= 1)
			{
				drops.Add(new ItemDropData(72.ToString(), 1, 1, 0.008f));
				drops.Add(new ItemDropData(74.ToString(), 1, 1, 0.008f));
			}
			yield return new MonsterData(name, health, damageToFarmer, isGlider, resilience, jitteriness, moveTowardsPlayerThreshold, speed, missChance, isMineMonster, drops.ToArray());
		}
	}

	public RecipeModel[] GetRecipes(Metadata metadata, IMonitor monitor, ExtraMachineConfigIntegration extraMachineConfig)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c1: Expected O, but got Unknown
		//IL_0a72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a82: Expected O, but got Unknown
		//IL_0673: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Expected O, but got Unknown
		List<RecipeModel> recipes = new List<RecipeModel>();
		var craftingRecipes = CraftingRecipe.cookingRecipes.Select(delegate(KeyValuePair<string, string> pair)
		{
			KeyValuePair<string, string> keyValuePair = pair;
			string key2 = keyValuePair.Key;
			keyValuePair = pair;
			return new
			{
				Key = key2,
				Value = keyValuePair.Value,
				IsCookingRecipe = true
			};
		}).Concat(CraftingRecipe.craftingRecipes.Select(delegate(KeyValuePair<string, string> pair)
		{
			KeyValuePair<string, string> keyValuePair = pair;
			string key2 = keyValuePair.Key;
			keyValuePair = pair;
			return new
			{
				Key = key2,
				Value = keyValuePair.Value,
				IsCookingRecipe = false
			};
		}));
		foreach (var entry in craftingRecipes)
		{
			if (entry.Value == null)
			{
				continue;
			}
			try
			{
				CraftingRecipe recipe = new CraftingRecipe(entry.Key, entry.IsCookingRecipe);
				foreach (string itemId in recipe.itemToProduce)
				{
					string qualifiedItemId = RecipeModel.QualifyRecipeOutputId(recipe, itemId) ?? itemId;
					recipes.Add(new RecipeModel(recipe, qualifiedItemId));
				}
			}
			catch (Exception value)
			{
				monitor.Log($"Couldn't parse {(entry.IsCookingRecipe ? "cooking" : "crafting")} recipe '{entry.Key}' due to an invalid format.\nRecipe data: '{entry.Value}'\nError: {value}", (LogLevel)3);
			}
		}
		string key;
		foreach (KeyValuePair<string, MachineData> item in DataLoader.Machines(Game1.content))
		{
			item.Deconstruct(out key, out var value2);
			string entryKey = key;
			MachineData machineData = value2;
			string qualifiedMachineId = entryKey;
			if (!ItemRegistry.Exists(qualifiedMachineId))
			{
				continue;
			}
			int? num = machineData?.OutputRules?.Count;
			if (!num.HasValue || num.GetValueOrDefault() <= 0)
			{
				continue;
			}
			RecipeIngredientModel[] additionalConsumedItems = machineData.AdditionalConsumedItems?.Select((MachineItemAdditionalConsumedItems item) => new RecipeIngredientModel(RecipeType.MachineInput, item.ItemId, item.RequiredCount)).ToArray() ?? Array.Empty<RecipeIngredientModel>();
			bool someRulesTooComplex = false;
			foreach (MachineOutputRule outputRule in machineData.OutputRules)
			{
				num = outputRule?.Triggers?.Count;
				if (!num.HasValue || num.GetValueOrDefault() <= 0)
				{
					continue;
				}
				num = outputRule.OutputItem?.Count;
				if (!num.HasValue || num.GetValueOrDefault() <= 0)
				{
					continue;
				}
				foreach (MachineOutputTriggerRule trigger in outputRule.Triggers)
				{
					if (trigger == null)
					{
						continue;
					}
					foreach (MachineItemOutput mainOutputItem in outputRule.OutputItem)
					{
						if (mainOutputItem == null)
						{
							continue;
						}
						MachineItemOutput[] array2;
						if (extraMachineConfig.IsLoaded)
						{
							MachineItemOutput val = mainOutputItem;
							IList<MachineItemOutput> extraOutputs = extraMachineConfig.ModApi.GetExtraOutputs(mainOutputItem, machineData);
							int num2 = 0;
							MachineItemOutput[] array = (MachineItemOutput[])(object)new MachineItemOutput[1 + extraOutputs.Count];
							array[num2] = val;
							num2++;
							foreach (MachineItemOutput item2 in extraOutputs)
							{
								array[num2] = item2;
								num2++;
							}
							array2 = array;
						}
						else
						{
							array2 = (MachineItemOutput[])(object)new MachineItemOutput[1] { mainOutputItem };
						}
						MachineItemOutput[] allOutputItems = array2;
						MachineItemOutput[] array3 = allOutputItems;
						foreach (MachineItemOutput outputItem in array3)
						{
							List<string> conditions = null;
							string rawConditions = null;
							if (!string.IsNullOrWhiteSpace(trigger.Condition))
							{
								rawConditions = trigger.Condition;
							}
							if (!string.IsNullOrWhiteSpace(((GenericSpawnItemDataWithCondition)mainOutputItem).Condition))
							{
								rawConditions = ((rawConditions != null) ? (rawConditions + ", " + ((GenericSpawnItemDataWithCondition)mainOutputItem).Condition) : ((GenericSpawnItemDataWithCondition)mainOutputItem).Condition);
							}
							if (!string.IsNullOrWhiteSpace(((GenericSpawnItemDataWithCondition)outputItem).Condition) && ((GenericSpawnItemDataWithCondition)outputItem).Condition != ((GenericSpawnItemDataWithCondition)mainOutputItem).Condition)
							{
								rawConditions = ((rawConditions != null) ? (rawConditions + ", " + ((GenericSpawnItemDataWithCondition)outputItem).Condition) : ((GenericSpawnItemDataWithCondition)outputItem).Condition);
							}
							if (rawConditions != null)
							{
								conditions = GameStateQuery.SplitRaw(rawConditions).Distinct().ToList();
							}
							if (!this.TryGetMostSpecificIngredientIds(trigger.RequiredItemId, trigger.RequiredTags, ref conditions, out string inputId, out string[] inputContextTags))
							{
								continue;
							}
							if (outputItem.OutputMethod != null)
							{
								someRulesTooComplex = true;
							}
							List<RecipeIngredientModel> ingredients = new List<RecipeIngredientModel>(1)
							{
								new RecipeIngredientModel(RecipeType.MachineInput, inputId, trigger.RequiredCount, inputContextTags)
							};
							ingredients.AddRange(additionalConsumedItems);
							if (extraMachineConfig.IsLoaded)
							{
								foreach (var (extraItemId, extraCount) in extraMachineConfig.ModApi.GetExtraRequirements(outputItem))
								{
									ingredients.Add(new RecipeIngredientModel(RecipeType.MachineInput, extraItemId, extraCount));
								}
								foreach (var (extraContextTags, extraCount2) in extraMachineConfig.ModApi.GetExtraTagsRequirements(outputItem))
								{
									ingredients.Add(new RecipeIngredientModel(RecipeType.MachineInput, null, extraCount2, extraContextTags.Split(",")));
								}
							}
							IList<ItemQueryResult> itemQueryResults;
							if (((GenericSpawnItemData)outputItem).ItemId != null || ((GenericSpawnItemData)outputItem).RandomItemId != null)
							{
								ItemQueryContext itemQueryContext = new ItemQueryContext();
								itemQueryResults = ItemQueryResolver.TryResolve((ISpawnItemData)(object)outputItem, itemQueryContext, (ItemQuerySearchMode)0, false, (HashSet<string>)null, (Func<string, string>)((string id) => id?.Replace("DROP_IN_ID", "0").Replace("DROP_IN_PRESERVE", "0").Replace("NEARBY_FLOWER_ID", "0")), (Action<string, string>)null, (Item)null);
							}
							else
							{
								itemQueryResults = new List<ItemQueryResult>();
								someRulesTooComplex = true;
							}
							recipes.AddRange(itemQueryResults.Select((ItemQueryResult result) => new RecipeModel(null, RecipeType.MachineInput, ItemRegistry.GetDataOrErrorItem(qualifiedMachineId).DisplayName, ingredients, 0, (Item? _) => ItemRegistry.Create(result.Item.QualifiedItemId, 1, 0, false), () => true, qualifiedMachineId, null, result.Item.QualifiedItemId, (((GenericSpawnItemData)outputItem).MinStack <= 0) ? 1 : ((GenericSpawnItemData)outputItem).MinStack, (((GenericSpawnItemData)outputItem).MaxStack > 0) ? new int?(((GenericSpawnItemData)outputItem).MaxStack) : ((int?)null), quality: ((GenericSpawnItemData)outputItem).Quality, outputChance: 100 / outputRule.OutputItem.Count / itemQueryResults.Count, conditions: conditions?.ToArray())));
						}
					}
				}
			}
			if (someRulesTooComplex)
			{
				recipes.Add(new RecipeModel(null, RecipeType.MachineInput, ItemRegistry.GetDataOrErrorItem(qualifiedMachineId).DisplayName, Array.Empty<RecipeIngredientModel>(), 0, (Item? _) => ItemRegistry.Create("__COMPLEX_RECIPE__", 1, 0, false), () => true, qualifiedMachineId, null, "__COMPLEX_RECIPE__"));
			}
		}
		foreach (KeyValuePair<string, BuildingData> buildingDatum in Game1.buildingData)
		{
			buildingDatum.Deconstruct(out key, out var value3);
			string buildingType = key;
			BuildingData buildingData = value3;
			BuildingData obj = buildingData;
			if (obj == null || obj.BuildCost <= 0)
			{
				BuildingData obj2 = buildingData;
				if (obj2 == null || !(obj2.BuildMaterials?.Count > 0))
				{
					goto IL_08e4;
				}
			}
			RecipeIngredientModel[] ingredients2 = RecipeModel.ParseIngredients(buildingData);
			Building building;
			try
			{
				building = new Building(buildingType, Vector2.Zero);
			}
			catch
			{
				continue;
			}
			recipes.Add(new RecipeModel(building, ingredients2, buildingData.BuildCost));
			goto IL_08e4;
			IL_08e4:
			BuildingData obj4 = buildingData;
			if (obj4 == null || !(obj4.ItemConversions?.Count > 0))
			{
				continue;
			}
			foreach (BuildingItemConversion rule in buildingData.ItemConversions)
			{
				int? num = rule?.ProducedItems?.Count;
				if (!num.HasValue || num.GetValueOrDefault() <= 0)
				{
					continue;
				}
				num = rule.RequiredTags?.Count;
				if (!num.HasValue || num.GetValueOrDefault() <= 0)
				{
					continue;
				}
				List<string> ruleConditions = null;
				if (!this.TryGetMostSpecificIngredientIds(null, rule.RequiredTags, ref ruleConditions, out string ingredientId, out string[] ingredientContextTags))
				{
					continue;
				}
				RecipeIngredientModel[] ingredients3 = new RecipeIngredientModel[1]
				{
					new RecipeIngredientModel(RecipeType.BuildingInput, ingredientId, rule.RequiredCount, ingredientContextTags)
				};
				foreach (GenericSpawnItemDataWithCondition outputItem2 in rule.ProducedItems)
				{
					if (outputItem2 == null)
					{
						continue;
					}
					IList<ItemQueryResult> itemQueryResults2 = ItemQueryResolver.TryResolve((ISpawnItemData)(object)outputItem2, new ItemQueryContext(), (ItemQuerySearchMode)0, false, (HashSet<string>)null, (Func<string, string>)null, (Action<string, string>)null, (Item)null);
					string[] conditions2 = ((!string.IsNullOrWhiteSpace(outputItem2.Condition)) ? GameStateQuery.SplitRaw(outputItem2.Condition).Distinct().ToArray() : null);
					recipes.AddRange(itemQueryResults2.Select((ItemQueryResult result) => new RecipeModel(null, RecipeType.BuildingInput, TokenParser.ParseText(buildingData?.Name, (Random)null, (TokenParserDelegate)null, (Farmer)null) ?? buildingType, ingredients3, 0, (Item? _) => ItemRegistry.Create(result.Item.QualifiedItemId, 1, 0, false), () => true, buildingType, null, result.Item.QualifiedItemId, (((GenericSpawnItemData)outputItem2).MinStack <= 0) ? 1 : ((GenericSpawnItemData)outputItem2).MinStack, (((GenericSpawnItemData)outputItem2).MaxStack > 0) ? new int?(((GenericSpawnItemData)outputItem2).MaxStack) : ((int?)null), quality: ((GenericSpawnItemData)outputItem2).Quality, outputChance: 100 / itemQueryResults2.Count, conditions: conditions2)));
				}
			}
		}
		return recipes.ToArray();
	}

	private bool TryGetMostSpecificIngredientIds(string? fromItemId, List<string?>? fromContextTags, ref List<string>? fromConditions, out string? itemId, out string[] contextTags)
	{
		contextTags = fromContextTags?.WhereNotNull().ToArray() ?? Array.Empty<string>();
		itemId = ((!string.IsNullOrWhiteSpace(fromItemId)) ? fromItemId : null);
		if (contextTags.Length == 1 && MachineDataHelper.TryGetUniqueItemFromContextTag(contextTags[0], out ParsedItemData dataFromTag))
		{
			if (itemId != null && ItemRegistry.QualifyItemId(itemId) != dataFromTag.QualifiedItemId)
			{
				return false;
			}
			itemId = dataFromTag.QualifiedItemId;
			contextTags = Array.Empty<string>();
		}
		if (fromConditions != null)
		{
			for (int i = 0; i < fromConditions.Count; i++)
			{
				if (MachineDataHelper.TryGetUniqueItemFromGameStateQuery(fromConditions[i], out ParsedItemData data))
				{
					if (itemId != null && data.QualifiedItemId != ItemRegistry.QualifyItemId(itemId))
					{
						return false;
					}
					itemId = data.QualifiedItemId;
					fromConditions.RemoveAt(i);
				}
			}
		}
		if (itemId == null)
		{
			return contextTags.Length != 0;
		}
		return true;
	}
}
