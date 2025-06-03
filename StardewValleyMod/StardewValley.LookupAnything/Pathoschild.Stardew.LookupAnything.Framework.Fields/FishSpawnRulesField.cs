using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.SpecialOrders;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class FishSpawnRulesField : CheckboxListField
{
	private static readonly string[] Seasons = new string[4] { "spring", "summer", "fall", "winter" };

	public FishSpawnRulesField(GameHelper gameHelper, string label, ParsedItemData fish, bool showUncaughtFishSpawnRules)
		: this(label, new CheckboxList(FishSpawnRulesField.GetConditions(gameHelper, fish), !showUncaughtFishSpawnRules && !FishSpawnRulesField.HasPlayerCaughtFish(fish)))
	{
	}

	public FishSpawnRulesField(GameHelper gameHelper, string label, GameLocation location, Vector2 tile, string fishAreaId, bool showUncaughtFishSpawnRules)
		: this(label, FishSpawnRulesField.GetConditions(gameHelper, location, tile, fishAreaId, showUncaughtFishSpawnRules).ToArray())
	{
	}//IL_0004: Unknown result type (might be due to invalid IL or missing references)


	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		float topOffset = 0f;
		int hiddenSpawnRulesCount = 0;
		CheckboxList[] checkboxLists = base.CheckboxLists;
		foreach (CheckboxList checkboxList in checkboxLists)
		{
			if (checkboxList.IsHidden)
			{
				hiddenSpawnRulesCount++;
			}
			else
			{
				topOffset += base.DrawCheckboxList(checkboxList, spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth).Y;
			}
		}
		if (hiddenSpawnRulesCount > 0)
		{
			topOffset += base.LineHeight + base.DrawIconText(spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, I18n.Item_UncaughtFish(hiddenSpawnRulesCount), Color.Gray).Y;
		}
		return new Vector2(wrapWidth, topOffset - base.LineHeight);
	}

	private FishSpawnRulesField(string label, params CheckboxList[] spawnConditions)
		: base(label)
	{
		base.CheckboxLists = spawnConditions;
		base.HasValue = base.CheckboxLists.Any((CheckboxList checkboxList) => checkboxList.Checkboxes.Length != 0);
	}

	private static IEnumerable<CheckboxList> GetConditions(GameHelper gameHelper, GameLocation location, Vector2 tile, string fishAreaId, bool showUncaughtFishSpawnRules)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		foreach (FishSpawnData spawnRules in gameHelper.GetFishSpawnRules(location, tile, fishAreaId))
		{
			ParsedItemData fishItemData = ItemRegistry.GetDataOrErrorItem(spawnRules.FishItem.QualifiedItemId);
			bool isCheckboxListHidden = !showUncaughtFishSpawnRules && !FishSpawnRulesField.HasPlayerCaughtFish(fishItemData);
			CheckboxList checkboxList = new CheckboxList(FishSpawnRulesField.GetConditions(gameHelper, fishItemData), isCheckboxListHidden);
			checkboxList.AddIntro(fishItemData.DisplayName, new SpriteInfo(fishItemData.GetTexture(), fishItemData.GetSourceRect(0, (int?)null)));
			yield return checkboxList;
		}
	}

	private static IEnumerable<Checkbox> GetConditions(GameHelper gameHelper, ParsedItemData fish)
	{
		FishSpawnData spawnRules = gameHelper.GetFishSpawnRules(fish);
		FishSpawnLocationData[]? locations = spawnRules.Locations;
		if (locations == null || !locations.Any())
		{
			yield break;
		}
		if (spawnRules.IsUnique)
		{
			yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_NotCaughtYet(), !FishSpawnRulesField.HasPlayerCaughtFish(fish));
		}
		if (spawnRules.MinFishingLevel > 0)
		{
			yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_MinFishingLevel(spawnRules.MinFishingLevel), Game1.player.FishingLevel >= spawnRules.MinFishingLevel);
		}
		if (spawnRules.IsLegendaryFamily)
		{
			yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_ExtendedFamilyQuestActive(), Game1.player.team.SpecialOrderRuleActive("LEGENDARY_FAMILY", (SpecialOrder)null));
		}
		if (spawnRules.Weather == FishSpawnWeather.Sunny)
		{
			yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_WeatherSunny(), !Game1.isRaining);
		}
		else if (spawnRules.Weather == FishSpawnWeather.Rainy)
		{
			yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_WeatherRainy(), Game1.isRaining);
		}
		if (spawnRules.TimesOfDay?.Any() ?? false)
		{
			yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_Time(I18n.List(spawnRules.TimesOfDay.Select((FishSpawnTimeOfDayData p) => I18n.Generic_Range(CommonHelper.FormatTime(p.MinTime), CommonHelper.FormatTime(p.MaxTime)).ToString()))), spawnRules.TimesOfDay.Any((FishSpawnTimeOfDayData p) => Game1.timeOfDay >= p.MinTime && Game1.timeOfDay <= p.MaxTime));
		}
		if (FishSpawnRulesField.HaveSameSeasons(spawnRules.Locations))
		{
			FishSpawnLocationData firstLocation = spawnRules.Locations[0];
			if (firstLocation.Seasons.Count == 4)
			{
				yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_SeasonAny(), isMet: true);
			}
			else
			{
				yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_SeasonList(I18n.List(firstLocation.Seasons.Select(gameHelper.TranslateSeason))), firstLocation.Seasons.Contains(Game1.currentSeason));
			}
			yield return FishSpawnRulesField.GetCondition(I18n.Item_FishSpawnRules_Locations(I18n.List(from p in spawnRules.Locations.Select(gameHelper.GetLocationDisplayName)
				orderby p
				select p)), spawnRules.MatchesLocation(Game1.currentLocation.Name));
			yield break;
		}
		Dictionary<string, string[]> locationsBySeason = (from location in spawnRules.Locations
			from season2 in location.Seasons
			select new
			{
				Season = season2,
				LocationName = gameHelper.GetLocationDisplayName(location)
			}).GroupBy(p => p.Season, p => p.LocationName, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, string>, string, string[]>((IGrouping<string, string> p) => p.Key, (IGrouping<string, string> p) => p.ToArray(), StringComparer.OrdinalIgnoreCase);
		List<IFormattedText> summary = new List<IFormattedText>
		{
			new FormattedText(I18n.Item_FishSpawnRules_LocationsBySeason_Label())
		};
		string[] seasons = FishSpawnRulesField.Seasons;
		foreach (string season in seasons)
		{
			if (locationsBySeason.TryGetValue(season, out var locationNames))
			{
				summary.Add(new FormattedText(Environment.NewLine + I18n.Item_FishSpawnRules_LocationsBySeason_SeasonLocations(gameHelper.TranslateSeason(season), I18n.List(locationNames)), (season == Game1.currentSeason) ? Color.Black : Color.Gray));
			}
		}
		bool hasMatch = spawnRules.Locations.Any((FishSpawnLocationData p) => p.LocationId == Game1.currentLocation.Name && p.Seasons.Contains(Game1.currentSeason));
		yield return FishSpawnRulesField.GetCondition(summary, hasMatch);
	}

	private static Checkbox GetCondition(string label, bool isMet)
	{
		return new Checkbox(isMet, label);
	}

	private static Checkbox GetCondition(IEnumerable<IFormattedText> label, bool isMet)
	{
		return new Checkbox(isMet, label.ToArray());
	}

	private static bool HaveSameSeasons(IEnumerable<FishSpawnLocationData> locations)
	{
		ISet<string> seasons = null;
		foreach (FishSpawnLocationData location in locations)
		{
			if (seasons == null)
			{
				seasons = location.Seasons;
			}
			else if (seasons.Count != location.Seasons.Count || !location.Seasons.All(seasons.Contains))
			{
				return false;
			}
		}
		return true;
	}

	private static bool HasPlayerCaughtFish(ParsedItemData fish)
	{
		return ((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)Game1.player.fishCaught).ContainsKey(fish.QualifiedItemId);
	}
}
