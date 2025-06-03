using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal static class HumanReadableContextTagParser
{
	public static string Format(string contextTag)
	{
		return HumanReadableContextTagParser.Format(contextTag, null) ?? I18n.Condition_RawContextTag(contextTag);
	}

	[return: NotNullIfNotNull("defaultValue")]
	public static string? Format(string contextTag, string? defaultValue)
	{
		if (string.IsNullOrWhiteSpace(contextTag))
		{
			return defaultValue;
		}
		bool negate = contextTag.StartsWith('!');
		string text;
		if (!negate)
		{
			text = contextTag;
		}
		else
		{
			text = contextTag.Substring(1, contextTag.Length - 1);
		}
		string actualTag = text;
		if (HumanReadableContextTagParser.TryParseCategory(actualTag, out string parsed) || HumanReadableContextTagParser.TryParseItemId(actualTag, out parsed) || HumanReadableContextTagParser.TryParsePreservedItemId(actualTag, out parsed) || HumanReadableContextTagParser.TryParseSpecial(actualTag, out parsed))
		{
			if (!negate)
			{
				return parsed;
			}
			return I18n.ConditionOrContextTag_Negate(parsed);
		}
		return defaultValue;
	}

	private static bool TryParseCategory(string tag, [NotNullWhen(true)] out string? parsed)
	{
		int? category = tag switch
		{
			"category_artisan_goods" => -26, 
			"category_bait" => -21, 
			"category_big_craftable" => -9, 
			"category_boots" => -97, 
			"category_clothing" => -100, 
			"category_cooking" => -7, 
			"category_crafting" => -8, 
			"category_egg" => -5, 
			"category_equipment" => -29, 
			"category_fertilizer" => -19, 
			"category_fish" => -4, 
			"category_flowers" => -80, 
			"category_fruits" => -79, 
			"category_furniture" => -24, 
			"category_gem" => -2, 
			"category_greens" => -81, 
			"category_hat" => -95, 
			"category_ingredients" => -25, 
			"category_junk" => -20, 
			"category_litter" => -999, 
			"category_meat" => -14, 
			"category_milk" => -6, 
			"category_minerals" => -12, 
			"category_monster_loot" => -28, 
			"category_ring" => -96, 
			"category_seeds" => -74, 
			"category_sell_at_fish_shop" => -23, 
			"category_syrup" => -27, 
			"category_tackle" => -22, 
			"category_tool" => -99, 
			"category_vegetable" => -75, 
			"category_weapon" => -98, 
			"category_sell_at_pierres" => -17, 
			"category_sell_at_pierres_and_marnies" => -18, 
			"category_metal_resources" => -15, 
			"category_building_resources" => -16, 
			"category_trinket" => -101, 
			_ => null, 
		};
		if (category.HasValue)
		{
			string displayName = Object.GetCategoryDisplayName(category.Value);
			if (!string.IsNullOrWhiteSpace(displayName))
			{
				parsed = displayName;
				return true;
			}
		}
		parsed = null;
		return false;
	}

	private static bool TryParseItemId(string tag, [NotNullWhen(true)] out string? parsed)
	{
		if (MachineDataHelper.TryGetUniqueItemFromContextTag(tag, out ParsedItemData data))
		{
			string name = data.DisplayName;
			if (!string.IsNullOrWhiteSpace(name))
			{
				parsed = name;
				return true;
			}
		}
		parsed = null;
		return false;
	}

	private static bool TryParsePreservedItemId(string tag, [NotNullWhen(true)] out string? parsed)
	{
		if (tag.StartsWith("preserve_sheet_index_"))
		{
			string name = ItemRegistry.GetData(tag.Substring(21, tag.Length - 21))?.DisplayName;
			if (!string.IsNullOrWhiteSpace(name))
			{
				parsed = I18n.ContextTag_PreservedItem(name);
				return true;
			}
		}
		parsed = null;
		return false;
	}

	private static bool TryParseSpecial(string tag, [NotNullWhen(true)] out string? parsed)
	{
		parsed = tag switch
		{
			"bone_item" => I18n.ContextTag_Bone(), 
			"egg_item" => I18n.ContextTag_Egg(), 
			"large_egg_item" => I18n.ContextTag_LargeEgg(), 
			_ => null, 
		};
		return parsed != null;
	}
}
