using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StardewValley.Tests;

/// <summary>Converts raw text from dialogue, event, mail, and data assets into language-independent syntax representations, which can be compared between languages to make sure they have the same sequence of commands, portraits, unlocalized metadata and delimiters, etc.</summary>
/// <remarks>
///   <para><strong>This is highly specialized.</strong> It's meant for vanilla unit tests, so it may not correctly handle non-vanilla text and may change at any time.</para>
///
///   For example, this converts a dialogue string like this:
///   <code>$c 0.5#Wow... Thanks, @!$h#Thank you! It's so pretty.</code>
///
///   Into a language-independent representation like this:
///   <code>$c 0.5#text$h#text</code>
/// </remarks>
public class SyntaxAbstractor
{
	/// <summary>The placeholder in <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractSyntaxFor(System.String,System.String,System.String)" /> for localizable text.</summary>
	public const string TextMarker = "text";

	/// <summary>The implementations which extract syntax from specific assets, indexed by exact match or prefix.</summary>
	public readonly Dictionary<string, ExtractSyntaxDelegate> SyntaxHandlers = new Dictionary<string, ExtractSyntaxDelegate>(StringComparer.OrdinalIgnoreCase)
	{
		["Characters/Dialogue/*"] = DialogueSyntaxHandler,
		["Data/EngagementDialogue"] = DialogueSyntaxHandler,
		["Data/ExtraDialogue"] = DialogueSyntaxHandler,
		["Strings/animationDescriptions"] = DialogueSyntaxHandler,
		["Strings/Buildings"] = DialogueSyntaxHandler,
		["Strings/Characters"] = DialogueSyntaxHandler,
		["Strings/Events"] = DialogueSyntaxHandler,
		["Strings/Locations"] = DialogueSyntaxHandler,
		["Strings/MovieReactions"] = DialogueSyntaxHandler,
		["Strings/Objects"] = DialogueSyntaxHandler,
		["Strings/Quests"] = DialogueSyntaxHandler,
		["Strings/schedules/*"] = DialogueSyntaxHandler,
		["Strings/SimpleNonVillagerDialogues"] = DialogueSyntaxHandler,
		["Strings/SpecialOrderStrings"] = DialogueSyntaxHandler,
		["Strings/SpeechBubbles"] = DialogueSyntaxHandler,
		["Strings/StringsFromCSFiles"] = DialogueSyntaxHandler,
		["Strings/StringsFromMaps"] = DialogueSyntaxHandler,
		["Strings/BigCraftables"] = PlainTextSyntaxHandler,
		["Strings/BundleNames"] = PlainTextSyntaxHandler,
		["Strings/EnchantmentNames"] = PlainTextSyntaxHandler,
		["Strings/FarmAnimals"] = PlainTextSyntaxHandler,
		["Strings/Furniture"] = PlainTextSyntaxHandler,
		["Strings/MovieConcessions"] = PlainTextSyntaxHandler,
		["Strings/Movies"] = PlainTextSyntaxHandler,
		["Strings/NPCNames"] = PlainTextSyntaxHandler,
		["Strings/Pants"] = PlainTextSyntaxHandler,
		["Strings/Shirts"] = PlainTextSyntaxHandler,
		["Strings/Tools"] = PlainTextSyntaxHandler,
		["Strings/TV/TipChannel"] = PlainTextSyntaxHandler,
		["Strings/UI"] = PlainTextSyntaxHandler,
		["Strings/Weapons"] = PlainTextSyntaxHandler,
		["Strings/WorldMap"] = PlainTextSyntaxHandler,
		["Data/Events/*"] = EventSyntaxHandler,
		["Data/Festivals/*"] = FestivalSyntaxHandler,
		["Data/Achievements"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '^', 0, 1),
		["Data/Boots"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', 1, 6),
		["Data/Bundles"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', 6),
		["Data/hats"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', 1, 5),
		["Data/Monsters"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', 14),
		["Data/NPCGiftTastes"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => (!key.StartsWith("Universal_")) ? syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', 0, 2, 4, 6, 8) : text,
		["Data/Quests"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', new int[3] { 1, 2, 3 }, new int[1] { 9 }),
		["Data/TV/CookingChannel"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractDelimitedDataSyntax(text, '/', 1),
		["Data/mail"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractMailSyntax(text),
		["Data/Notes"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractMailSyntax(text),
		["Data/SecretNotes"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractMailSyntax(text),
		["Strings/credits"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractCreditsSyntax(text),
		["Strings/1_6_Strings"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.Extract16StringsSyntax(key, text),
		["Strings/Lexicon"] = (SyntaxAbstractor syntaxBuilder, string _, string key, string text) => syntaxBuilder.ExtractLexiconSyntax(key, text)
	};

	/// <summary>Get a handler which can extract syntactic representations for a given asset.</summary>
	/// <param name="baseAssetName">The asset name without the locale suffix, like <c>Data/Achievements</c>.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractSyntaxFor(System.String,System.String,System.String)" /> or a specific method like <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractDialogueSyntax(System.String)" /> instead.</remarks>
	public ExtractSyntaxDelegate GetSyntaxHandler(string baseAssetName)
	{
		if (this.SyntaxHandlers.TryGetValue(baseAssetName, out var handler))
		{
			return handler;
		}
		int splitIndex = baseAssetName.LastIndexOf('/');
		if (splitIndex != -1 && this.SyntaxHandlers.TryGetValue(baseAssetName.Substring(0, splitIndex) + "/*", out handler))
		{
			return handler;
		}
		return null;
	}

	/// <summary>Get a syntactic representation of an arbitrary asset entry, if it's a known asset.</summary>
	/// <param name="baseAssetName">The asset name without the locale suffix, like <c>Data/Achievements</c>.</param>
	/// <param name="key">The key within the asset for the text value.</param>
	/// <param name="value">The text to represent.</param>
	public string ExtractSyntaxFor(string baseAssetName, string key, string value)
	{
		if (value.Contains("${"))
		{
			value = Regex.Replace(value, "\\$\\{.+?\\}\\$", "text");
		}
		return this.GetSyntaxHandler(baseAssetName)?.Invoke(this, baseAssetName, key, value) ?? value;
	}

	/// <summary>Get a syntactic representation of plain text which has no special syntax.</summary>
	/// <param name="value">The text to represent.</param>
	public string ExtractPlainTextSyntax(string value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return "text";
		}
		return string.Empty;
	}

	/// <summary>Get a syntactic representation of a dialogue string.</summary>
	/// <param name="value">The text to represent.</param>
	/// <remarks>This handles the general syntax format. For asset-specific formats, see <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractDialogueSyntax(System.String,System.String,System.String)" /> instead.</remarks>
	public string ExtractDialogueSyntax(string value)
	{
		StringBuilder syntax = new StringBuilder();
		int index = 0;
		this.ExtractDialogueSyntaxImpl(value, '#', ref index, syntax);
		return syntax.ToString();
	}

	/// <summary>Get a syntactic representation of a dialogue string.</summary>
	/// <param name="baseAssetName">The asset name without the locale suffix, like <c>Data/Achievements</c>.</param>
	/// <param name="key">The key within the asset for the text value.</param>
	/// <param name="value">The text to represent.</param>
	/// <remarks>This supports asset-specific dialogue formats. In particular, some translations are loaded via <see cref="M:StardewValley.Game1.LoadStringByGender(StardewValley.Gender,System.String)" /> which supports a special <c>male/female</c> format based on the NPC's gender (not the player's gender).</remarks>
	public string ExtractDialogueSyntax(string baseAssetName, string key, string value)
	{
		switch (baseAssetName)
		{
		case "Data/ExtraDialogue":
			switch (key)
			{
			case "NewChild_Adoption":
			case "NewChild_FirstChild":
			case "NewChild_SecondChild1":
			case "NewChild_SecondChild2":
				return this.ExtractNpcGenderedDialogueSyntax(value);
			}
			break;
		case "Strings/Locations":
			if (key == "FarmHouse_SpouseAttacked3")
			{
				return "text";
			}
			break;
		case "Strings/StringsFromCSFiles":
			switch (key)
			{
			case "Pipe":
				return "text";
			case "Event.cs.1497":
			case "Event.cs.1498":
			case "Event.cs.1499":
			case "Event.cs.1500":
			case "Event.cs.1501":
			case "Event.cs.1504":
			case "NPC.cs.3957":
			case "NPC.cs.3959":
			case "NPC.cs.3962":
			case "NPC.cs.3963":
			case "NPC.cs.3965":
			case "NPC.cs.3966":
			case "NPC.cs.3968":
			case "NPC.cs.3974":
			case "NPC.cs.3975":
			case "NPC.cs.4079":
			case "NPC.cs.4080":
			case "NPC.cs.4088":
			case "NPC.cs.4089":
			case "NPC.cs.4091":
			case "NPC.cs.4113":
			case "NPC.cs.4115":
			case "NPC.cs.4141":
			case "NPC.cs.4144":
			case "NPC.cs.4146":
			case "NPC.cs.4147":
			case "NPC.cs.4149":
			case "NPC.cs.4152":
			case "NPC.cs.4153":
			case "NPC.cs.4154":
			case "NPC.cs.4274":
			case "NPC.cs.4276":
			case "NPC.cs.4277":
			case "NPC.cs.4278":
			case "NPC.cs.4279":
			case "NPC.cs.4293":
			case "NPC.cs.4422":
			case "NPC.cs.4446":
			case "NPC.cs.4447":
			case "NPC.cs.4449":
			case "NPC.cs.4452":
			case "NPC.cs.4455":
			case "NPC.cs.4462":
			case "NPC.cs.4470":
			case "NPC.cs.4474":
			case "NPC.cs.4481":
			case "NPC.cs.4488":
			case "NPC.cs.4498":
			case "NPC.cs.4500":
				return this.ExtractNpcGenderedDialogueSyntax(value);
			case "OptionsPage.cs.11289":
			case "OptionsPage.cs.11290":
			case "OptionsPage.cs.11291":
			case "OptionsPage.cs.11292":
			case "OptionsPage.cs.11293":
			case "OptionsPage.cs.11294":
			case "OptionsPage.cs.11295":
			case "OptionsPage.cs.11296":
			case "OptionsPage.cs.11297":
			case "OptionsPage.cs.11298":
			case "OptionsPage.cs.11299":
			case "OptionsPage.cs.11300":
				if (!string.IsNullOrWhiteSpace(value))
				{
					return "text";
				}
				break;
			}
			break;
		}
		return this.ExtractDialogueSyntax(value);
	}

	/// <summary>Get a syntactic representation of a dialogue string.</summary>
	/// <param name="value">The text to represent.</param>
	public string ExtractEventSyntax(string value)
	{
		StringBuilder syntax = new StringBuilder();
		int index = 0;
		this.ExtractEventSyntaxImpl(value, ref index, syntax);
		return syntax.ToString();
	}

	/// <summary>Get a syntactic representation of a festival string.</summary>
	/// <param name="baseAssetName">The asset name without the locale suffix, like <c>Data/Achievements</c>.</param>
	/// <param name="key">The key within the asset for the text value.</param>
	/// <param name="value">The text to represent.</param>
	public string ExtractFestivalSyntax(string baseAssetName, string key, string value)
	{
		switch (key)
		{
		case "mainEvent":
		case "set-up_y2":
		case "mainEvent_y2":
		case "conditions":
		case "set-up":
			return this.ExtractEventSyntax(value);
		case "afterEggHunt":
		case "AbbyWin":
		case "afterEggHunt_y2":
			if (baseAssetName == "Data/Festivals/spring13")
			{
				return this.ExtractEventSyntax(value);
			}
			break;
		case "governorReaction0":
		case "governorReaction1":
		case "governorReaction2":
		case "governorReaction3":
		case "governorReaction4":
		case "governorReaction5":
		case "governorReaction6":
			if (baseAssetName == "Data/Festivals/summer11")
			{
				return this.ExtractEventSyntax(value);
			}
			break;
		}
		return this.ExtractDialogueSyntax(value);
	}

	/// <summary>Get a syntactic representation of a <c>Strings/credits</c> entry.</summary>
	/// <param name="text">The text to represent.</param>
	/// <remarks>See parsing logic in <see cref="M:StardewValley.Menus.AboutMenu.SetUpCredits" />.</remarks>
	public string ExtractCreditsSyntax(string text)
	{
		if (text.Length == 0)
		{
			return text;
		}
		if (text.StartsWith('['))
		{
			if (text.StartsWith("[image]"))
			{
				return text;
			}
			if (text.StartsWith("[link]"))
			{
				string[] parts = text.Split(' ', 3);
				parts[2] = "text";
				return string.Join(" ", parts);
			}
		}
		StringBuilder syntax = new StringBuilder();
		int index = 0;
		bool hasText = false;
		for (; index < text.Length; index++)
		{
			if (text[index] == '[')
			{
				this.EndTextContext(ref hasText, syntax);
				this.ExtractTagSyntax(text, ref index, syntax);
			}
			else
			{
				hasText = true;
			}
		}
		this.EndTextContext(ref hasText, syntax);
		return syntax.ToString();
	}

	/// <summary>Get a syntactic representation of a mail string.</summary>
	/// <param name="text">The text to represent.</param>
	/// <remarks>This handles the general syntax format. For asset-specific formats, see <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractDialogueSyntax(System.String,System.String,System.String)" /> instead.</remarks>
	public string ExtractMailSyntax(string text)
	{
		text = text.Replace("%secretsanta", "text");
		StringBuilder syntax = new StringBuilder();
		int index = 0;
		bool hasText = false;
		for (; index < text.Length; index++)
		{
			char ch = text[index];
			switch (ch)
			{
			case '¦':
				this.EndTextContext(ref hasText, syntax);
				syntax.Append(ch);
				break;
			case '[':
				this.EndTextContext(ref hasText, syntax);
				this.ExtractTagSyntax(text, ref index, syntax);
				break;
			case '%':
				if (index >= text.Length || char.IsWhiteSpace(text[index + 1]) || char.IsDigit(text[index + 1]))
				{
					hasText = true;
					break;
				}
				this.EndTextContext(ref hasText, syntax);
				this.ExtractMailCommandSyntax(text, ref index, syntax);
				break;
			default:
				if (!hasText)
				{
					hasText = true;
				}
				break;
			case ' ':
				break;
			}
		}
		this.EndTextContext(ref hasText, syntax);
		return syntax.ToString();
	}

	/// <summary>Get a syntactic representation of a data entry containing delimited fields.</summary>
	/// <param name="text">The dialogue entry.</param>
	/// <param name="delimiter">The delimiter between fields.</param>
	/// <param name="textFields">The field indices containing localized text, which should be replaced by <see cref="F:StardewValley.Tests.SyntaxAbstractor.TextMarker" />.</param>
	public string ExtractDelimitedDataSyntax(string text, char delimiter, params int[] textFields)
	{
		return this.ExtractDelimitedDataSyntax(text, delimiter, textFields, null);
	}

	/// <summary>Get a syntactic representation of a data entry containing delimited fields.</summary>
	/// <param name="text">The dialogue entry.</param>
	/// <param name="delimiter">The delimiter between fields.</param>
	/// <param name="textFields">The field indices containing localized text, which should be replaced by <see cref="F:StardewValley.Tests.SyntaxAbstractor.TextMarker" />.</param>
	/// <param name="dialogueFields">The field indices containing dialogue text.</param>
	public string ExtractDelimitedDataSyntax(string text, char delimiter, int[] textFields, int[] dialogueFields)
	{
		string[] parts = text.Split(delimiter);
		int[] array = textFields;
		foreach (int index in array)
		{
			if (ArgUtility.HasIndex(parts, index))
			{
				parts[index] = "text";
			}
		}
		if (dialogueFields != null)
		{
			array = dialogueFields;
			foreach (int index2 in array)
			{
				if (ArgUtility.HasIndex(parts, index2))
				{
					parts[index2] = this.ExtractDialogueSyntax(parts[index2]);
				}
			}
		}
		return string.Join(delimiter.ToString(), parts);
	}

	/// <summary>Get a syntactic representation of a string from <c>Strings/1_6_Strings</c>.</summary>
	/// <param name="key">The key within the asset for the text value.</param>
	/// <param name="text">The text to represent.</param>
	public string Extract16StringsSyntax(string key, string text)
	{
		if (key.StartsWith("Renovation_"))
		{
			return this.ExtractDelimitedDataSyntax(text, '/', LegacyShims.EmptyArray<int>(), new int[3] { 0, 1, 2 });
		}
		if (!(key == "ForestPylonEvent"))
		{
			if (key == "StarterChicken_Names")
			{
				string[] array = text.Split('|');
				StringBuilder syntax = new StringBuilder();
				bool omittedPairs = false;
				string[] array2 = array;
				foreach (string entry in array2)
				{
					if (entry.Split(',', 3).Length == 2)
					{
						if (syntax.Length == 0)
						{
							syntax.Append("name,name");
						}
						else
						{
							omittedPairs = true;
						}
						continue;
					}
					if (syntax.Length > 0)
					{
						syntax.Append(" | ");
					}
					StringBuilder stringBuilder = syntax;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder);
					handler.AppendLiteral("<invalid pair: ");
					handler.AppendFormatted(entry.Trim());
					handler.AppendLiteral(">");
					stringBuilder.Append(ref handler);
				}
				if (omittedPairs)
				{
					return $"{syntax} | ...";
				}
				if (syntax.Length > 0)
				{
					return syntax.ToString();
				}
				return string.Empty;
			}
			return this.ExtractDialogueSyntax(text);
		}
		return this.ExtractEventSyntax(text);
	}

	/// <summary>Get a syntactic representation of a string from <c>Strings/Lexicon</c>.</summary>
	/// <param name="key">The key within the asset for the text value.</param>
	/// <param name="text">The text to represent.</param>
	public string ExtractLexiconSyntax(string key, string text)
	{
		string[] parts = text.Split('#');
		for (int i = 0; i < parts.Length; i++)
		{
			if (!string.IsNullOrWhiteSpace(parts[i]))
			{
				string raw = parts[i];
				int prefixSpaces = raw.Length - raw.TrimStart().Length;
				int suffixSpaces = raw.Length - raw.TrimEnd().Length;
				parts[i] = ((prefixSpaces > 0 || suffixSpaces > 0) ? ("".PadRight(prefixSpaces) + "text" + "".PadRight(suffixSpaces)) : "text");
			}
		}
		if (key.StartsWith("Random") && parts.Length > 2)
		{
			return parts[0] + "#" + parts[1] + "#...";
		}
		return string.Join("#", parts);
	}

	/// <summary>A shortcut for calling <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractDialogueSyntax(System.String,System.String,System.String)" /> in <see cref="F:StardewValley.Tests.SyntaxAbstractor.SyntaxHandlers" />.</summary>
	/// <inheritdoc cref="T:StardewValley.Tests.ExtractSyntaxDelegate" />
	private static string DialogueSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
	{
		return syntaxAbstractor.ExtractDialogueSyntax(baseAssetName, key, text);
	}

	/// <summary>A shortcut for calling <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractPlainTextSyntax(System.String)" /> in <see cref="F:StardewValley.Tests.SyntaxAbstractor.SyntaxHandlers" />.</summary>
	/// <inheritdoc cref="T:StardewValley.Tests.ExtractSyntaxDelegate" />
	private static string PlainTextSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
	{
		return syntaxAbstractor.ExtractPlainTextSyntax(text);
	}

	/// <summary>A shortcut for calling <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractEventSyntax(System.String)" /> in <see cref="F:StardewValley.Tests.SyntaxAbstractor.SyntaxHandlers" />.</summary>
	/// <inheritdoc cref="T:StardewValley.Tests.ExtractSyntaxDelegate" />
	private static string EventSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
	{
		return syntaxAbstractor.ExtractEventSyntax(text);
	}

	/// <summary>A shortcut for calling <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractFestivalSyntax(System.String,System.String,System.String)" /> in <see cref="F:StardewValley.Tests.SyntaxAbstractor.SyntaxHandlers" />.</summary>
	/// <inheritdoc cref="T:StardewValley.Tests.ExtractSyntaxDelegate" />
	private static string FestivalSyntaxHandler(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text)
	{
		return syntaxAbstractor.ExtractFestivalSyntax(baseAssetName, key, text);
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractSyntaxFor(System.String,System.String,System.String)" />, read a syntax representation of an event script.</summary>
	/// <param name="text">The event script.</param>
	/// <param name="index">The index at which to read the next character. After this method runs, it will be set to the last character in the event script.</param>
	/// <param name="syntax">The string builder to extend with the current command's syntax.</param>
	/// <param name="maxIndex">If set, the index at which to stop reading the string.</param>
	private void ExtractEventSyntaxImpl(string text, ref int index, StringBuilder syntax, int maxIndex = -1)
	{
		string[] array = ArgUtility.SplitQuoteAware((index == 0 && maxIndex < 0) ? text : text.Substring(index, maxIndex - index + 1), '/', StringSplitOptions.TrimEntries, keepQuotesAndEscapes: true);
		bool isFirstCommand = true;
		string[] array2 = array;
		foreach (string rawCommand in array2)
		{
			if (!isFirstCommand)
			{
				syntax.Append('/');
			}
			if (!string.IsNullOrWhiteSpace(rawCommand))
			{
				string[] args = ArgUtility.SplitBySpaceQuoteAware(rawCommand);
				string commandName = args[0];
				syntax.Append(commandName);
				int nextArg = 1;
				if (Event.TryResolveCommandName(commandName, out var actualName))
				{
					switch (actualName)
					{
					case "End":
					{
						string action = ArgUtility.Get(args, 1);
						if (action == "dialogue" || action == "dialogueWarpOut")
						{
							this.AppendEventCommandArg(syntax, args, 1);
							this.AppendEventCommandArg(syntax, args, 2);
							this.AppendEventCommandDialogueArg(syntax, args, 3);
							nextArg = 4;
						}
						break;
					}
					case "Message":
						this.AppendEventCommandDialogueArg(syntax, args, 1);
						nextArg = 2;
						break;
					case "Question":
						this.AppendEventCommandArg(syntax, args, 1);
						this.AppendEventCommandDialogueArg(syntax, args, 2);
						nextArg = 3;
						break;
					case "QuickQuestion":
					{
						string[] masterSplit = LegacyShims.SplitAndTrim(rawCommand.Substring(rawCommand.IndexOf(' ')), "(break)");
						string[] questionAndAnswerSplit = LegacyShims.SplitAndTrim(masterSplit[0], '#');
						syntax.Append(" \"");
						this.AppendEventCommandDialogueArg(syntax, questionAndAnswerSplit, 0, prependSpace: true, quote: false);
						for (int k = 1; k < questionAndAnswerSplit.Length; k++)
						{
							syntax.Append('#');
							this.AppendEventCommandDialogueArg(syntax, questionAndAnswerSplit, k, prependSpace: false, quote: false);
						}
						for (int l = 1; l < masterSplit.Length; l++)
						{
							masterSplit[l] = masterSplit[l].Replace('\\', '/');
							syntax.Append("(break)");
							int tempIndex = 0;
							this.ExtractEventSyntaxImpl(masterSplit[l], ref tempIndex, syntax);
						}
						syntax.Append('"');
						nextArg = args.Length;
						break;
					}
					case "Speak":
						this.AppendEventCommandArg(syntax, args, 1);
						this.AppendEventCommandDialogueArg(syntax, args, 2);
						nextArg = 3;
						break;
					case "SplitSpeak":
					{
						string[] dialogues = ArgUtility.Get(args, 2)?.Split('~');
						this.AppendEventCommandArg(syntax, args, 1);
						if (dialogues != null)
						{
							syntax.Append(" \"");
							for (int j = 0; j < dialogues.Length; j++)
							{
								if (j > 0)
								{
									syntax.Append('~');
								}
								this.AppendEventCommandDialogueArg(syntax, dialogues, j, prependSpace: false, quote: false);
							}
							syntax.Append('"');
						}
						nextArg = 3;
						break;
					}
					case "SpriteText":
						this.AppendEventCommandArg(syntax, args, 1);
						this.AppendEventCommandDialogueArg(syntax, args, 2);
						nextArg = 3;
						break;
					case "TextAboveHead":
						this.AppendEventCommandArg(syntax, args, 1);
						this.AppendEventCommandDialogueArg(syntax, args, 2);
						nextArg = 3;
						break;
					}
				}
				for (; nextArg < args.Length; nextArg++)
				{
					this.AppendEventCommandArg(syntax, args, nextArg);
				}
			}
			isFirstCommand = false;
		}
		index = ((maxIndex > 0) ? maxIndex : (text.Length - 1));
	}

	/// <summary>Append an event command argument to a syntax string being built, including the preceding space.</summary>
	/// <param name="syntax">The syntax string being built.</param>
	/// <param name="args">The command arguments.</param>
	/// <param name="index">The index of the argument in <paramref name="args" /> to append.</param>
	/// <param name="prependSpace">Whether to prepend a space before the argument.</param>
	private void AppendEventCommandArg(StringBuilder syntax, string[] args, int index, bool prependSpace = true)
	{
		if (ArgUtility.HasIndex(args, index))
		{
			string text = args[index];
			bool num = text.Contains(' ');
			if (prependSpace)
			{
				syntax.Append(' ');
			}
			if (num)
			{
				syntax.Append('"');
			}
			syntax.Append(text);
			if (num)
			{
				syntax.Append('"');
			}
		}
	}

	/// <summary>Append an event command argument containing dialogue syntax to a syntax string being built, including the preceding space.</summary>
	/// <param name="syntax">The syntax string being built.</param>
	/// <param name="args">The command arguments.</param>
	/// <param name="index">The index of the argument in <paramref name="args" /> to append.</param>
	/// <param name="prependSpace">Whether to prepend a space before the argument.</param>
	/// <param name="quote">Whether to quote the dialogue string.</param>
	private void AppendEventCommandDialogueArg(StringBuilder syntax, string[] args, int index, bool prependSpace = true, bool quote = true)
	{
		if (ArgUtility.HasIndex(args, index))
		{
			string text = args[index];
			int tempIndex = 0;
			if (prependSpace)
			{
				syntax.Append(' ');
			}
			if (quote)
			{
				syntax.Append('"');
			}
			this.ExtractDialogueSyntaxImpl(text, '/', ref tempIndex, syntax);
			if (quote)
			{
				syntax.Append('"');
			}
		}
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractDialogueSyntax(System.String,System.String,System.String)" />, extract the syntax for a dialogue which is NPC-gendered via <see cref="M:StardewValley.Game1.LoadStringByGender(StardewValley.Gender,System.String)" />.</summary>
	/// <param name="text">The dialogue entry.</param>
	private string ExtractNpcGenderedDialogueSyntax(string text)
	{
		if (!text.Contains('/'))
		{
			return this.ExtractDialogueSyntax(text);
		}
		string[] parts = text.Split('/');
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i] = this.ExtractDialogueSyntax(parts[i]);
		}
		if (parts.Length != 2 || !(parts[0] == parts[1]))
		{
			return string.Join("/", parts);
		}
		return parts[0];
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractSyntaxFor(System.String,System.String,System.String)" />, read a syntax representation of a dialogue entry.</summary>
	/// <param name="text">The dialogue entry.</param>
	/// <param name="commandDelimiter">Within the larger asset, the character which delimits commands. This is usually <c>#</c> for dialogue, or <c>/</c> for event data. This is used in certain specialized cases like <see cref="F:StardewValley.Dialogue.dialogueQuickResponse" />, which extends to the end of the command.</param>
	/// <param name="index">The index at which to read the next character. After this method runs, it will be set to the last character in the dialogue string.</param>
	/// <param name="syntax">The string builder to extend with the current command's syntax.</param>
	/// <param name="maxIndex">If set, the index at which to stop reading the string.</param>
	private void ExtractDialogueSyntaxImpl(string text, char commandDelimiter, ref int index, StringBuilder syntax, int maxIndex = -1)
	{
		bool hasText = false;
		bool hasSpaces = false;
		if (maxIndex < 0 || maxIndex > text.Length - 1)
		{
			maxIndex = text.Length - 1;
		}
		while (index <= maxIndex)
		{
			char ch = text[index];
			switch (ch)
			{
			case '#':
			case '$':
			case '|':
				if (ch == '$' && hasSpaces && !hasText)
				{
					syntax.Append("text");
				}
				this.EndTextContext(ref hasText, syntax);
				hasSpaces = false;
				if (ch == '$')
				{
					this.ExtractDialogueCommandSyntax(text, ref index, syntax, commandDelimiter);
				}
				else
				{
					syntax.Append(ch);
				}
				break;
			case '[':
				this.EndTextContext(ref hasText, syntax);
				this.ExtractDialogueItemSpawnSyntax(text, ref index, syntax);
				hasSpaces = false;
				break;
			case ']':
				this.EndTextContext(ref hasText, syntax);
				syntax.Append(']');
				hasSpaces = false;
				break;
			default:
				if (ch == ' ')
				{
					hasSpaces = true;
				}
				else
				{
					hasText = true;
				}
				break;
			}
			index++;
		}
		this.EndTextContext(ref hasText, syntax);
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractDialogueSyntaxImpl(System.String,System.Char,System.Int32@,System.Text.StringBuilder,System.Int32)" />, read a syntax representation of a single command from the input string.</summary>
	/// <param name="text">The dialogue or event text.</param>
	/// <param name="index">The index at which to read the next character. After this method runs, it will be set to the last character in the command.</param>
	/// <param name="syntax">The string builder to extend with the current command's syntax.</param>
	/// <param name="commandDelimiter">Within the larger asset, the character which delimits commands. This is usually <c>#</c> for dialogue, or <c>/</c> for event data. This is used in certain specialized cases like <see cref="F:StardewValley.Dialogue.dialogueQuickResponse" />, which extends to the end of the command.</param>
	private void ExtractDialogueCommandSyntax(string text, ref int index, StringBuilder syntax, char commandDelimiter)
	{
		int startIndex = index;
		index++;
		while (index < text.Length && (char.IsLetter(text[index]) || char.IsNumber(text[index])))
		{
			index++;
		}
		string commandName = text.Substring(startIndex, index - startIndex);
		syntax.Append(commandName);
		if (commandName != null)
		{
			int length = commandName.Length;
			if (length != 2)
			{
				if (length == 6 && commandName == "$query")
				{
					goto IL_01a8;
				}
			}
			else
			{
				int startIndex2;
				switch (commandName[1])
				{
				case 'c':
					if (commandName == "$c")
					{
						goto IL_0166;
					}
					goto IL_03a0;
				case 'q':
					if (commandName == "$q")
					{
						goto IL_0166;
					}
					goto IL_03a0;
				case 'r':
					if (commandName == "$r")
					{
						goto IL_0166;
					}
					goto IL_03a0;
				case '1':
					if (commandName == "$1")
					{
						goto IL_0166;
					}
					goto IL_03a0;
				case 't':
					if (commandName == "$t")
					{
						goto IL_0166;
					}
					goto IL_03a0;
				case 'd':
					break;
				case 'p':
					goto IL_012d;
				case 'y':
					goto IL_013f;
				default:
					goto IL_03a0;
					IL_0166:
					startIndex2 = index;
					while (index < text.Length && text[index] != '#')
					{
						index++;
					}
					syntax.Append(text.Substring(startIndex2, index - startIndex2).TrimEnd(' '));
					goto IL_03a0;
				}
				if (commandName == "$d")
				{
					goto IL_01a8;
				}
			}
		}
		goto IL_03a0;
		IL_012d:
		if (commandName == "$p")
		{
			goto IL_01a8;
		}
		goto IL_03a0;
		IL_013f:
		if (commandName == "$y")
		{
			int startIndex3 = index;
			while (index < text.Length && text[index] == ' ')
			{
				index++;
			}
			if (text[index] != '\'')
			{
				index = startIndex3;
				return;
			}
			index++;
			syntax.Append(text.Substring(startIndex3, index - startIndex3).TrimEnd(' '));
			int endIndex = index;
			int maxIndex = text.IndexOf(commandDelimiter, index);
			if (maxIndex == -1)
			{
				maxIndex = text.Length;
			}
			while (true)
			{
				int nextIndex = text.IndexOf('\'', endIndex + 1);
				if (nextIndex == -1 || nextIndex > maxIndex)
				{
					break;
				}
				endIndex = nextIndex;
			}
			if (endIndex <= index)
			{
				return;
			}
			bool hasText = false;
			while (index < endIndex - 1)
			{
				char ch = text[index];
				if (ch == '_')
				{
					if (hasText)
					{
						syntax.Append("text");
						hasText = false;
					}
					syntax.Append(ch);
				}
				else
				{
					hasText = true;
				}
				index++;
			}
			if (hasText)
			{
				syntax.Append("text");
			}
			index++;
			syntax.Append(text[index]);
			index++;
		}
		goto IL_03a0;
		IL_01a8:
		int startIndex4 = index;
		while (index < text.Length && text[index] != '#')
		{
			index++;
		}
		index++;
		syntax.Append(text.Substring(startIndex4, index - startIndex4).TrimEnd(' '));
		int i;
		for (i = index; i < text.Length && text[i] != '#' && text[i] != '|'; i++)
		{
		}
		this.ExtractDialogueSyntaxImpl(text, commandDelimiter, ref index, syntax, i - 1);
		if (index < text.Length && text[index] == '|')
		{
			syntax.Append(text[index]);
			index++;
			int j;
			for (j = index; j < text.Length && text[j] != '#' && text[j] != '|'; j++)
			{
			}
			this.ExtractDialogueSyntaxImpl(text, commandDelimiter, ref index, syntax, j - 1);
			goto IL_03a0;
		}
		return;
		IL_03a0:
		index--;
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractDialogueSyntaxImpl(System.String,System.Char,System.Int32@,System.Text.StringBuilder,System.Int32)" />, read a syntax representation of an item spawn list like <c>[128 129]</c>.</summary>
	/// <param name="text">The dialogue or event text.</param>
	/// <param name="index">The index at which to read the next character. After this method runs, it will be set to the last character in the command.</param>
	/// <param name="syntax">The string builder to extend with the current command's syntax.</param>
	private void ExtractDialogueItemSpawnSyntax(string text, ref int index, StringBuilder syntax)
	{
		int startIndex = index;
		int endIndex = index;
		endIndex++;
		bool foundEnd = false;
		for (; endIndex < text.Length; endIndex++)
		{
			char ch = text[endIndex];
			if (ch != ' ' && ch != '.' && !char.IsLetter(ch) && !char.IsNumber(ch))
			{
				if (ch == ']')
				{
					foundEnd = true;
				}
				break;
			}
		}
		if (foundEnd)
		{
			syntax.Append(text.Substring(startIndex, endIndex - startIndex + 1).TrimEnd(' '));
			index = endIndex;
		}
		else
		{
			syntax.Append(text[index]);
			index++;
		}
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractMailSyntax(System.String)" />, read a syntax representation of a single <c>%</c> mail command from the input string.</summary>
	/// <param name="text">The dialogue or event text.</param>
	/// <param name="index">The index at which to read the next character. After this method runs, it will be set to the last character in the command.</param>
	/// <param name="syntax">The string builder to extend with the current command's syntax.</param>
	private void ExtractMailCommandSyntax(string text, ref int index, StringBuilder syntax)
	{
		int startIndex = index;
		index++;
		while (index < text.Length && (char.IsLetter(text[index]) || char.IsNumber(text[index])))
		{
			index++;
		}
		string commandName = text.Substring(startIndex, index - startIndex);
		if (!(commandName == "%item"))
		{
			if (commandName == "%revealtaste")
			{
				index -= "%revealtaste".Length;
				this.ExtractRevealTasteCommandSyntax(text, ref index, syntax);
			}
			else
			{
				syntax.Append(commandName);
				index--;
			}
			return;
		}
		syntax.Append(commandName);
		int startIndex2 = index;
		while (index < text.Length)
		{
			index++;
			if (index > 1 && text[index] == '%' && text[index - 1] == '%')
			{
				break;
			}
		}
		string command = ((text[index] == '%' && text[index - 1] == '%' && char.IsWhiteSpace(text[index - 2])) ? (text.Substring(startIndex2, index - startIndex2 - 1).TrimEnd() + "%%") : text.Substring(startIndex2, index - startIndex2 + 1));
		syntax.Append(command);
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractMailSyntax(System.String)" /> or <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractCreditsSyntax(System.String)" />, read a syntax representation of a single <c>[...]</c> tag from the input string.</summary>
	/// <param name="text">The dialogue or event text.</param>
	/// <param name="index">The index at which to read the next character. After this method runs, it will be set to the last character in the command.</param>
	/// <param name="syntax">The string builder to extend with the current command's syntax.</param>
	private void ExtractTagSyntax(string text, ref int index, StringBuilder syntax)
	{
		int startIndex = index;
		index++;
		while (index < text.Length - 1 && text[index] != ']')
		{
			index++;
		}
		syntax.Append(text.Substring(startIndex, index - startIndex + 1));
	}

	/// <summary>As part of <see cref="M:StardewValley.Tests.SyntaxAbstractor.ExtractMailSyntax(System.String)" />, read a syntax representation of a single <c>%</c> mail command from the input string.</summary>
	/// <param name="text">The dialogue or event text.</param>
	/// <param name="index">The index at which to read the next character. After this method runs, it will be set to the last character in the command.</param>
	/// <param name="syntax">The string builder to extend with the current command's syntax.</param>
	/// <remarks>Derived from <see cref="M:StardewValley.Utility.ParseGiftReveals(System.String)" />.</remarks>
	private void ExtractRevealTasteCommandSyntax(string text, ref int index, StringBuilder syntax)
	{
		int startIndex = index;
		while (index < text.Length - 1)
		{
			char next = text[index + 1];
			if (char.IsWhiteSpace(next) || next == '#' || next == '%' || next == '$' || next == '{' || next == '^' || next == '*' || next == '[')
			{
				break;
			}
			index++;
		}
		syntax.Append(text.Substring(startIndex, index - startIndex + 1));
	}

	/// <summary>If we're in the text portion of a dialogue/data string, output a <c>text</c> token now and end the text portion.</summary>
	/// <param name="hasText">Whether we're in a text portion of the input string. This will be set to false.</param>
	/// <param name="syntax">The syntax string being compiled.</param>
	private void EndTextContext(ref bool hasText, StringBuilder syntax)
	{
		if (hasText)
		{
			syntax.Append("text");
			hasText = false;
		}
	}
}
