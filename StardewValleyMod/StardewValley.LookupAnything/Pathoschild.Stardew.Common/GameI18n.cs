using System;
using StardewValley;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.Common;

internal static class GameI18n
{
	public static string GetBuildingName(string id)
	{
		if (Game1.buildingData == null)
		{
			return "(missing translation: game hasn't loaded object data yet)";
		}
		try
		{
			if (Game1.buildingData.TryGetValue(id, out var data))
			{
				return TokenParser.ParseText(data?.Name, (Random)null, (TokenParserDelegate)null, (Farmer)null) ?? id;
			}
			return "(missing translation: no building with ID '" + id + "')";
		}
		catch
		{
			return "(missing translation: building with ID '" + id + "' has an invalid format)";
		}
	}

	public static string GetObjectName(string id)
	{
		if (Game1.objectData == null)
		{
			return "(missing translation: game hasn't loaded object data yet)";
		}
		try
		{
			return ItemRegistry.GetData(ItemRegistry.ManuallyQualifyItemId(id, "(O)", false))?.DisplayName ?? ("(missing translation: no object with ID '" + id + "')");
		}
		catch
		{
			return "(missing translation: object with ID '" + id + "' has an invalid format)";
		}
	}

	public static string GetString(string key, params object[] substitutions)
	{
		return Game1.content.LoadString(key, substitutions);
	}
}
