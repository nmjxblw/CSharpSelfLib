using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal static class HumanReadableConditionParser
{
	public static string Format(string condition)
	{
		return HumanReadableConditionParser.Format(condition, null) ?? I18n.Condition_RawCondition(condition);
	}

	[return: NotNullIfNotNull("defaultValue")]
	public static string? Format(string condition, string? defaultValue)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		ParsedGameStateQuery[] queries = GameStateQuery.Parse(condition);
		if (queries.Length != 1)
		{
			return defaultValue;
		}
		ParsedGameStateQuery query = queries[0];
		if (query.Error != null || query.Query.Length == 0 || string.IsNullOrWhiteSpace(query.Query[0]))
		{
			return defaultValue;
		}
		string parsed = null;
		switch (query.Query[0].ToUpperInvariant().Trim())
		{
		case "DAY_OF_MONTH":
			parsed = HumanReadableConditionParser.ParseDayOfMonth(query.Query);
			break;
		case "ITEM_CONTEXT_TAG":
			parsed = HumanReadableConditionParser.ParseItemContextTag(query.Query);
			break;
		case "ITEM_EDIBILITY":
			parsed = HumanReadableConditionParser.ParseItemEdibility(query.Query);
			break;
		}
		if (parsed != null)
		{
			if (!query.Negated)
			{
				return parsed;
			}
			return I18n.ConditionOrContextTag_Negate(parsed);
		}
		return defaultValue;
	}

	private static string ParseDayOfMonth(string[] query)
	{
		string[] days = query.Skip(1).ToArray();
		for (int i = 0; i < days.Length; i++)
		{
			string day = days[i];
			if (string.Equals(day, "even", StringComparison.OrdinalIgnoreCase))
			{
				days[i] = I18n.Condition_DayOfMonth_Even();
			}
			else if (string.Equals(day, "odd", StringComparison.OrdinalIgnoreCase))
			{
				days[i] = I18n.Condition_DayOfMonth_Odd();
			}
		}
		return I18n.Condition_DayOfMonth(I18n.List(days));
	}

	private static string? ParseItemContextTag(string[] query)
	{
		if (!HumanReadableConditionParser.TryTranslateItemTarget(ArgUtility.Get(query, 1, (string)null, true), out string itemType))
		{
			return null;
		}
		string[] rawTags = ArgUtility.GetSubsetOf<string>(query, 2, -1);
		for (int i = 0; i < rawTags.Length; i++)
		{
			rawTags[i] = I18n.Condition_ItemContextTag_Value(rawTags[i]);
		}
		string contextTags = I18n.List(rawTags);
		if (query.Length <= 3)
		{
			return I18n.Condition_ItemContextTag(itemType, contextTags);
		}
		return I18n.Condition_ItemContextTags(itemType, contextTags);
	}

	private static string? ParseItemEdibility(string[] query)
	{
		if (!HumanReadableConditionParser.TryTranslateItemTarget(ArgUtility.Get(query, 1, (string)null, true), out string itemType))
		{
			return null;
		}
		if (!ArgUtility.HasIndex<string>(query, 2))
		{
			return I18n.Condition_ItemEdibility_Edible(itemType);
		}
		int min = ArgUtility.GetInt(query, 2, 0);
		int max = ArgUtility.GetInt(query, 3, int.MaxValue);
		if (max == int.MaxValue)
		{
			return I18n.Condition_ItemEdibility_Min(itemType, min);
		}
		return I18n.Condition_ItemEdibility_Range(itemType, min, max);
	}

	private static bool TryTranslateItemTarget(string? itemType, [NotNullWhen(true)] out string? translated)
	{
		if (string.Equals(itemType, "Input", StringComparison.OrdinalIgnoreCase))
		{
			translated = I18n.Condition_ItemType_Input();
			return true;
		}
		if (string.Equals(itemType, "Target", StringComparison.OrdinalIgnoreCase))
		{
			translated = I18n.Condition_ItemType_Target();
			return true;
		}
		translated = null;
		return false;
	}
}
