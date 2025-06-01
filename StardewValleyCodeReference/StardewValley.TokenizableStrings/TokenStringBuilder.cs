namespace StardewValley.TokenizableStrings;

/// <summary>Creates tokenized strings in the format recognized by <see cref="T:StardewValley.TokenizableStrings.TokenParser" />.</summary>
public static class TokenStringBuilder
{
	/// <summary>Build a tokenized string which wraps the input in [EscapedText] if it contains spaces.</summary>
	/// <param name="value">The value to escape.</param>
	/// <param name="skipIfNotNeeded">Whether to keep the input as-is if it likely doesn't need to be escaped.</param>
	public static string EscapedText(string value, bool skipIfNotNeeded = true)
	{
		if (!skipIfNotNeeded || (value.IndexOfAny(TokenParser.HeuristicCharactersForEscapableStrings) != -1 && !value.StartsWith("[EscapedText")))
		{
			value = "[EscapedText " + value + "]";
		}
		return value;
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.AchievementName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="achievementId">The achievement ID.</param>
	public static string AchievementName(int achievementId)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("AchievementName", achievementId.ToString());
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ArticleFor(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="word">The noun for which to get the article.</param>
	public static string ArticleFor(string word)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("ArticleFor", word);
	}

	/// <summary>Build a <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.CapitalizeFirstLetter(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="text">The text whose first letter to capitalize.</param>
	public static string CapitalizeFirstLetter(string text)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("CapitalizeFirstLetter", text);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ItemName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string for an item ID (ignoring any preserved flavor).</summary>
	/// <param name="itemId">The qualified item ID.</param>
	/// <param name="fallbackItemName">The item name to use if the item doesn't exist; defaults to "Error Item (id)".</param>
	/// <remarks>Consider using <see cref="M:StardewValley.TokenizableStrings.TokenStringBuilder.ItemNameFor(StardewValley.Item,System.String)" /> instead if you have an item instance, which accounts for variations like flavored items automatically.</remarks>
	public static string ItemName(string itemId, string fallbackItemName = null)
	{
		if (fallbackItemName == null)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("ItemName", itemId);
		}
		return TokenStringBuilder.BuildTokenWithArgumentString("ItemName", itemId, fallbackItemName);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ItemNameWithFlavor(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string for a flavored item.</summary>
	/// <param name="preserveType">The preserve type.</param>
	/// <param name="preservedId">The item ID for the flavor item, like <c>(O)258</c> for the blueberry in "Blueberry Wine".</param>
	/// <param name="fallbackItemName">The base item name to use if the item doesn't exist; defaults to "Error Item (id)".</param>
	/// <remarks>Consider using <see cref="M:StardewValley.TokenizableStrings.TokenStringBuilder.ItemNameFor(StardewValley.Item,System.String)" /> instead if you have an item instance, which accounts for variations like flavored items automatically.</remarks>
	public static string ItemNameWithFlavor(Object.PreserveType preserveType, string preservedId, string fallbackItemName = null)
	{
		if (fallbackItemName == null)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("ItemNameWithFlavor", preserveType.ToString(), preservedId);
		}
		return TokenStringBuilder.BuildTokenWithArgumentString("ItemNameWithFlavor", preserveType.ToString(), preservedId, fallbackItemName);
	}

	/// <summary>Build a token which produces an item's translated display name using <see cref="P:StardewValley.Object.displayNameFormat" />, <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ItemNameWithFlavor(System.String[],System.String@,System.Random,StardewValley.Farmer)" />, or <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ItemName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> depending on the item data.</summary>
	/// <param name="item">The item instance.</param>
	/// <param name="fallbackItemName">The item name to use if the item doesn't exist; defaults to "Error Item (id)".</param>
	public static string ItemNameFor(Item item, string fallbackItemName = null)
	{
		if (item is Object obj)
		{
			if (!string.IsNullOrWhiteSpace(obj.displayNameFormat))
			{
				return obj.displayNameFormat;
			}
			if (obj.preserve.Value.HasValue)
			{
				return TokenStringBuilder.ItemNameWithFlavor(obj.preserve.Value.Value, obj.preservedParentSheetIndex.Value, fallbackItemName);
			}
		}
		return TokenStringBuilder.ItemName(item?.QualifiedItemId, fallbackItemName);
	}

	/// <summary>Build a <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.LocalizedText(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="translationKey">The translation key containing the asset name and entry key, like <c>Strings\Lexicon:Pronoun_Female</c>.</param>
	public static string LocalizedText(string translationKey)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("LocalizedText", translationKey);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.MonsterName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="monsterId">The monster ID.</param>
	/// <param name="fallbackText">The text to display if a localized name isn't found in data; defaults to the monster ID.</param>
	public static string MonsterName(string monsterId, string fallbackText = null)
	{
		if (fallbackText == null)
		{
			return TokenStringBuilder.BuildTokenWithArgumentString("MonsterName", monsterId);
		}
		return TokenStringBuilder.BuildTokenWithArgumentString("MonsterName", monsterId, fallbackText);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.MovieName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="movieId">The movie ID.</param>
	public static string MovieName(string movieId)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("MovieName", movieId);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.NumberWithSeparators(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="number">The number to format.</param>
	public static string NumberWithSeparators(int number)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("NumberWithSeparators", number.ToString());
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.SpecialOrderName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="orderId">The special order ID.</param>
	public static string SpecialOrderName(string orderId)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("SpecialOrderName", orderId);
	}

	/// <summary>Build an <see cref="M:StardewValley.TokenizableStrings.TokenParser.DefaultResolvers.ToolName(System.String[],System.String@,System.Random,StardewValley.Farmer)" /> token string.</summary>
	/// <param name="itemId">The qualified tool ID.</param>
	/// <param name="upgradeLevel">The tool upgrade level.</param>
	public static string ToolName(string itemId, int upgradeLevel)
	{
		return TokenStringBuilder.BuildTokenWithArgumentString("ToolName", itemId, upgradeLevel.ToString());
	}

	/// <summary>Build a tokenized string in the form <c>[token [EscapedText argument]]</c>.</summary>
	/// <param name="tokenName">The literal token name, like <c>LocalizedText</c>.</param>
	/// <param name="argument">The tokenized string passed as an argument to the token.</param>
	public static string BuildTokenWithArgumentString(string tokenName, string argument)
	{
		return "[" + tokenName + " " + TokenStringBuilder.EscapedText(argument) + "]";
	}

	/// <summary>Build a tokenized string in the form <c>[token [EscapedText argument]]</c>.</summary>
	/// <param name="tokenName">The literal token name, like <c>LocalizedText</c>.</param>
	/// <param name="arg1">The tokenized string passed as the first argument to the token.</param>
	/// <param name="arg2">The tokenized string passed as the second argument to the token.</param>
	public static string BuildTokenWithArgumentString(string tokenName, string arg1, string arg2)
	{
		return "[" + tokenName + " " + TokenStringBuilder.EscapedText(arg1) + " " + TokenStringBuilder.EscapedText(arg2) + "]";
	}

	/// <summary>Build a tokenized string in the form <c>[token [EscapedText argument]]</c>.</summary>
	/// <param name="tokenName">The literal token name, like <c>LocalizedText</c>.</param>
	/// <param name="arg1">The tokenized string passed as the first argument to the token.</param>
	/// <param name="arg2">The tokenized string passed as the second argument to the token.</param>
	/// <param name="arg3">The tokenized string passed as the third argument to the token.</param>
	public static string BuildTokenWithArgumentString(string tokenName, string arg1, string arg2, string arg3)
	{
		return "[" + tokenName + " " + TokenStringBuilder.EscapedText(arg1) + " " + TokenStringBuilder.EscapedText(arg2) + " " + TokenStringBuilder.EscapedText(arg3) + "]";
	}
}
