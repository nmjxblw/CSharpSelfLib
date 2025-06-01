using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace StardewValley;

public class Dialogue
{
	public delegate bool onAnswerQuestion(int whichResponse);

	public const string dialogueHappy = "$h";

	public const string dialogueSad = "$s";

	public const string dialogueUnique = "$u";

	public const string dialogueNeutral = "$neutral";

	public const string dialogueLove = "$l";

	public const string dialogueAngry = "$a";

	public const string dialogueEnd = "$e";

	/// <summary>The character which begins a command name.</summary>
	public const char dialogueCommandPrefix = '$';

	/// <summary>A dialogue code which splits the subsequent text into a separate dialogue box shown after the player clicks.</summary>
	public const string dialogueBreak = "$b";

	/// <summary>Equivalent to <see cref="F:StardewValley.Dialogue.dialogueBreak" />, but wrapped with command delimiters so it can be added directly to dialogue text.</summary>
	public const string dialogueBreakDelimited = "#$b#";

	public const string multipleDialogueDelineator = "||";

	public const string dialogueKill = "$k";

	public const string dialogueChance = "$c";

	public const string dialogueDependingOnWorldState = "$d";

	public const string dialogueEvent = "$v";

	public const string dialogueQuickResponse = "$y";

	public const string dialoguePrerequisite = "$p";

	public const string dialogueSingle = "$1";

	/// <summary>A command which toggles between two dialogues depending on the result of a game state query.</summary>
	public const string dialogueGameStateQuery = "$query";

	/// <summary>A command which switches between gendered text based on the player gender.</summary>
	public const string dialogueGenderSwitch_startBlock = "${";

	/// <summary>The end token for a <see cref="F:StardewValley.Dialogue.dialogueGenderSwitch_startBlock" /> command.</summary>
	public const string dialogueGenderSwitch_endBlock = "}$";

	/// <summary>A command which runs an action.</summary>
	public const string dialogueRunAction = "$action";

	/// <summary>A command which begins a conversation topic.</summary>
	public const string dialogueStartConversationTopic = "$t";

	/// <summary>A command which begins a question.</summary>
	public const string dialogueQuestion = "$q";

	/// <summary>A command which starts an inquiry initiated by the player or an answer to an NPC's question.</summary>
	public const string dialogueResponse = "$r";

	/// <summary>A special character added to dialogues to signify that they are part of a broken up series of dialogues.</summary>
	public const string breakSpecialCharacter = "{";

	public const string playerNameSpecialCharacter = "@";

	public const char genderDialogueSplitCharacter = '^';

	public const char genderDialogueSplitCharacter2 = '¦';

	public const string quickResponseDelineator = "*";

	public const string randomAdjectiveSpecialCharacter = "%adj";

	public const string randomNounSpecialCharacter = "%noun";

	public const string randomPlaceSpecialCharacter = "%place";

	public const string spouseSpecialCharacter = "%spouse";

	public const string randomNameSpecialCharacter = "%name";

	public const string firstNameLettersSpecialCharacter = "%firstnameletter";

	public const string timeSpecialCharacter = "%time";

	public const string bandNameSpecialCharacter = "%band";

	public const string bookNameSpecialCharacter = "%book";

	public const string petSpecialCharacter = "%pet";

	public const string farmNameSpecialCharacter = "%farm";

	public const string favoriteThingSpecialCharacter = "%favorite";

	public const string eventForkSpecialCharacter = "%fork";

	public const string yearSpecialCharacter = "%year";

	public const string kid1specialCharacter = "%kid1";

	public const string kid2SpecialCharacter = "%kid2";

	public const string revealTasteCharacter = "%revealtaste";

	public const string seasonCharacter = "%season";

	public const string dontfacefarmer = "%noturn";

	/// <summary>A prefix added to a dialogue line to indicate it should be drawn as a small dialogue box with no portrait.</summary>
	/// <remarks>This is only applied if it's not part of another token like <c>%year</c>.</remarks>
	public const char noPortraitPrefix = '%';

	/// <summary>The translation key for <see cref="M:StardewValley.Dialogue.GetFallbackForError(StardewValley.NPC)" />.</summary>
	public const string FallbackDialogueForErrorKey = "Strings\\Characters:FallbackDialogueForError";

	/// <summary>The tokens like <see cref="F:StardewValley.Dialogue.spouseSpecialCharacter" /> which begin with a <c>%</c> symbol.</summary>
	public static readonly string[] percentTokens = new string[18]
	{
		"%adj", "%noun", "%place", "%spouse", "%name", "%firstnameletter", "%time", "%band", "%book", "%pet",
		"%farm", "%favorite", "%fork", "%year", "%kid1", "%kid2", "%revealtaste", "%season"
	};

	private static bool nameArraysTranslated = false;

	public static string[] adjectives = new string[20]
	{
		"Purple", "Gooey", "Chalky", "Green", "Plush", "Chunky", "Gigantic", "Greasy", "Gloomy", "Practical",
		"Lanky", "Dopey", "Crusty", "Fantastic", "Rubbery", "Silly", "Courageous", "Reasonable", "Lonely", "Bitter"
	};

	public static string[] nouns = new string[23]
	{
		"Dragon", "Buffet", "Biscuit", "Robot", "Planet", "Pepper", "Tomb", "Hyena", "Lip", "Quail",
		"Cheese", "Disaster", "Raincoat", "Shoe", "Castle", "Elf", "Pump", "Chip", "Wig", "Mermaid",
		"Drumstick", "Puppet", "Submarine"
	};

	public static string[] verbs = new string[13]
	{
		"ran", "danced", "spoke", "galloped", "ate", "floated", "stood", "flowed", "smelled", "swam",
		"grilled", "cracked", "melted"
	};

	public static string[] positional = new string[13]
	{
		"atop", "near", "with", "alongside", "away from", "too close to", "dangerously close to", "far, far away from", "uncomfortably close to", "way above the",
		"miles below", "on a different planet from", "in a different century than"
	};

	public static string[] places = new string[12]
	{
		"Castle Village", "Basket Town", "Pine Mesa City", "Point Drake", "Minister Valley", "Grampleton", "Zuzu City", "a small island off the coast", "Fort Josa", "Chestervale",
		"Fern Islands", "Tanker Grove"
	};

	public static string[] colors = new string[16]
	{
		"/crimson", "/green", "/tan", "/purple", "/deep blue", "/neon pink", "/pale/yellow", "/chocolate/brown", "/sky/blue", "/bubblegum/pink",
		"/blood/red", "/bright/orange", "/aquamarine", "/silvery", "/glimmering/gold", "/rainbow"
	};

	/// <summary>The dialogues to show in their own message boxes, and/or actions to perform when selected.</summary>
	public List<DialogueLine> dialogues = new List<DialogueLine>();

	/// <summary>The <see cref="F:StardewValley.Dialogue.currentDialogueIndex" /> values for which to disable the portrait due to <see cref="F:StardewValley.Dialogue.noPortraitPrefix" />.</summary>
	public HashSet<int> indexesWithoutPortrait = new HashSet<int>();

	/// <summary>The responses which the player can choose from, if any.</summary>
	private List<NPCDialogueResponse> playerResponses;

	private List<string> quickResponses;

	private bool isLastDialogueInteractive;

	private bool quickResponse;

	public bool isCurrentStringContinuedOnNextScreen;

	private bool finishedLastDialogue;

	public bool showPortrait;

	public bool removeOnNextMove;

	public bool dontFaceFarmer;

	public string temporaryDialogueKey;

	public int currentDialogueIndex;

	/// <summary>The backing field for <see cref="P:StardewValley.Dialogue.CurrentEmotion" />.</summary>
	/// <remarks>Most code shouldn't use this directly.</remarks>
	private string currentEmotion;

	public NPC speaker;

	public onAnswerQuestion answerQuestionBehavior;

	public Texture2D overridePortrait;

	public Action onFinish;

	/// <summary>The translation key from which the <see cref="F:StardewValley.Dialogue.dialogues" /> were taken, if known, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>. This is informational only, and has no effect on the dialogue text. The displayed text may not match the translation text exactly (e.g. due to token substitutions or dialogue parsing).</summary>
	public readonly string TranslationKey;

	/// <summary>The portrait command for the current dialogue, usually matching a constant like <see cref="F:StardewValley.Dialogue.dialogueHappy" /> or a numeric index like <c>$1</c>.</summary>
	public string CurrentEmotion
	{
		get
		{
			return this.currentEmotion ?? "$neutral";
		}
		set
		{
			this.currentEmotion = value;
		}
	}

	/// <summary>Whether the <see cref="P:StardewValley.Dialogue.CurrentEmotion" /> was set explicitly (e.g. via a dialogue command like <see cref="F:StardewValley.Dialogue.dialogueNeutral" />), instead of being the default value.</summary>
	public bool CurrentEmotionSetExplicitly => this.currentEmotion != null;

	public Farmer farmer
	{
		get
		{
			if (Game1.CurrentEvent != null)
			{
				return Game1.CurrentEvent.farmer;
			}
			return Game1.player;
		}
	}

	private static void TranslateArraysOfStrings()
	{
		Dialogue.colors = new string[16]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.795"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.796"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.797"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.798"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.799"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.800"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.801"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.802"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.803"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.804"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.805"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.806"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.807"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.808"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.809"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.810")
		};
		Dialogue.adjectives = new string[20]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.679"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.680"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.681"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.682"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.683"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.684"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.685"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.686"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.687"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.688"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.689"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.690"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.691"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.692"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.693"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.694"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.695"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.696"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.697"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.698")
		};
		Dialogue.nouns = new string[23]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.699"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.700"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.701"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.702"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.703"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.704"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.705"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.706"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.707"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.708"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.709"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.710"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.711"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.712"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.713"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.714"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.715"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.716"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.717"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.718"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.719"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.720"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.721")
		};
		Dialogue.verbs = new string[13]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.722"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.723"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.724"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.725"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.726"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.727"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.728"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.729"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.730"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.731"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.732"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.733"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.734")
		};
		Dialogue.positional = new string[13]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.735"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.736"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.737"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.738"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.739"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.740"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.741"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.742"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.743"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.744"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.745"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.746"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.747")
		};
		Dialogue.places = new string[12]
		{
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.748"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.749"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.750"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.751"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.752"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.753"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.754"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.755"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.756"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.757"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.758"),
			Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.759")
		};
		Dialogue.nameArraysTranslated = true;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which the <paramref name="dialogueText" /> was taken, if known, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>. This is informational only, and has no effect on the dialogue text.</param>
	/// <param name="dialogueText">The literal dialogue text to display.</param>
	/// <remarks>This constructor allows setting literal text. To use a translation as-is, see the other constructor.</remarks>
	public Dialogue(NPC speaker, string translationKey, string dialogueText)
	{
		if (!Dialogue.nameArraysTranslated)
		{
			Dialogue.TranslateArraysOfStrings();
		}
		this.speaker = speaker;
		this.TranslationKey = translationKey;
		try
		{
			this.parseDialogueString(dialogueText, translationKey);
			this.checkForSpecialDialogueAttributes();
		}
		catch (Exception exception)
		{
			Game1.log.Error($"Failed parsing dialogue string for NPC {speaker?.Name} (key: {translationKey}, text: {dialogueText}).", exception);
			this.parseDialogueString(Dialogue.GetFallbackTextForError(), "Strings\\Characters:FallbackDialogueForError");
			this.checkForSpecialDialogueAttributes();
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="isGendered">Whether the <paramref name="translationKey" /> matches a gendered translation.</param>
	/// <remarks>This matches the most common convention, i.e. a translation with no format placeholders. For more advanced cases, see <c>FromTranslation</c> or the constructor which takes a <c>dialogueText</c> parameter.</remarks>
	public Dialogue(NPC speaker, string translationKey, bool isGendered = false)
		: this(speaker, translationKey, isGendered ? Game1.LoadStringByGender(speaker.Gender, translationKey) : Game1.content.LoadString(translationKey))
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="other">The data to copy.</param>
	public Dialogue(Dialogue other)
	{
		foreach (DialogueLine line in other.dialogues)
		{
			this.dialogues.Add(new DialogueLine(line.Text, line.SideEffects));
		}
		this.indexesWithoutPortrait = new HashSet<int>(other.indexesWithoutPortrait);
		if (other.playerResponses != null)
		{
			this.playerResponses = new List<NPCDialogueResponse>();
			foreach (NPCDialogueResponse response in other.playerResponses)
			{
				this.playerResponses.Add(new NPCDialogueResponse(response));
			}
		}
		if (other.quickResponses != null)
		{
			this.quickResponses = new List<string>(other.quickResponses);
		}
		this.isLastDialogueInteractive = other.isLastDialogueInteractive;
		this.quickResponse = other.quickResponse;
		this.isCurrentStringContinuedOnNextScreen = other.isCurrentStringContinuedOnNextScreen;
		this.finishedLastDialogue = other.finishedLastDialogue;
		this.showPortrait = other.showPortrait;
		this.removeOnNextMove = other.removeOnNextMove;
		this.dontFaceFarmer = other.dontFaceFarmer;
		this.temporaryDialogueKey = other.temporaryDialogueKey;
		this.currentDialogueIndex = other.currentDialogueIndex;
		this.currentEmotion = other.currentEmotion;
		this.speaker = other.speaker;
		this.answerQuestionBehavior = other.answerQuestionBehavior;
		this.overridePortrait = other.overridePortrait;
		this.onFinish = other.onFinish;
		this.TranslationKey = other.TranslationKey;
	}

	/// <summary>Get a dialogue instance if the given translation key exists.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	public static Dialogue TryGetDialogue(NPC speaker, string translationKey)
	{
		string text = Game1.content.LoadStringReturnNullIfNotFound(translationKey);
		if (text == null)
		{
			return null;
		}
		return new Dialogue(speaker, translationKey, text);
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey)
	{
		return new Dialogue(speaker, translationKey);
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="sub1">The value with which to replace the <c>{0}</c> placeholder in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1));
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="sub1">The value with which to replace the <c>{0}</c> placeholder in the loaded text.</param>
	/// <param name="sub2">The value with which to replace the <c>{1}</c> placeholder in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1, object sub2)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1, sub2));
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="sub1">The value with which to replace the <c>{0}</c> placeholder in the loaded text.</param>
	/// <param name="sub2">The value with which to replace the <c>{1}</c> placeholder in the loaded text.</param>
	/// <param name="sub3">The value with which to replace the <c>{2}</c> placeholder in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, object sub1, object sub2, object sub3)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, sub1, sub2, sub3));
	}

	/// <summary>Get a dialogue instance for a translation key.</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	/// <param name="translationKey">The translation from which to take the dialogue text, in the form <c>assetName:fieldKey</c> like <c>Strings/UI:Confirm</c>.</param>
	/// <param name="substitutions">The values with which to replace placeholders like <c>{0}</c> in the loaded text.</param>
	public static Dialogue FromTranslation(NPC speaker, string translationKey, params object[] substitutions)
	{
		return new Dialogue(speaker, translationKey, Game1.content.LoadString(translationKey, substitutions));
	}

	/// <summary>Get a fallback dialogue to show when an error happens and a suitable dialogue can't be loaded. This is usually some variation of "...".</summary>
	/// <param name="speaker">The NPC saying the line.</param>
	public static Dialogue GetFallbackForError(NPC speaker)
	{
		return Dialogue.TryGetDialogue(speaker, "Strings\\Characters:FallbackDialogueForError") ?? new Dialogue(speaker, "Strings\\Characters:FallbackDialogueForError", "...");
	}

	/// <summary>Get a fallback dialogue text to show when an error happens and a suitable dialogue can't be loaded. This is usually some variation of "...".</summary>
	/// <remarks>Most code should use <see cref="M:StardewValley.Dialogue.GetFallbackForError(StardewValley.NPC)" /> instead.</remarks>
	public static string GetFallbackTextForError()
	{
		return Game1.content.LoadStringReturnNullIfNotFound("Strings\\Characters:FallbackDialogueForError") ?? "...";
	}

	public static string getRandomVerb()
	{
		if (!Dialogue.nameArraysTranslated)
		{
			Dialogue.TranslateArraysOfStrings();
		}
		return Game1.random.Choose(Dialogue.verbs);
	}

	public static string getRandomAdjective()
	{
		if (!Dialogue.nameArraysTranslated)
		{
			Dialogue.TranslateArraysOfStrings();
		}
		return Game1.random.Choose(Dialogue.adjectives);
	}

	public static string getRandomNoun()
	{
		if (!Dialogue.nameArraysTranslated)
		{
			Dialogue.TranslateArraysOfStrings();
		}
		return Game1.random.Choose(Dialogue.nouns);
	}

	public static string getRandomPositional()
	{
		if (!Dialogue.nameArraysTranslated)
		{
			Dialogue.TranslateArraysOfStrings();
		}
		return Game1.random.Choose(Dialogue.positional);
	}

	public int getPortraitIndex()
	{
		if (this.speaker != null && Game1.isGreenRain && this.speaker.Name.Equals("Demetrius") && Game1.year == 1)
		{
			return 7;
		}
		switch (this.CurrentEmotion)
		{
		case "$neutral":
			return 0;
		case "$h":
			return 1;
		case "$s":
			return 2;
		case "$u":
			return 3;
		case "$l":
			return 4;
		case "$a":
			return 5;
		default:
		{
			if (!int.TryParse(this.CurrentEmotion.Substring(1), out var index))
			{
				return 0;
			}
			return index;
		}
		}
	}

	/// <summary>Parse raw dialogue text.</summary>
	/// <param name="masterString">The raw dialogue text to parse.</param>
	/// <param name="translationKey">The translation key from which the dialogue was loaded, if known.</param>
	protected virtual void parseDialogueString(string masterString, string translationKey)
	{
		masterString = TokenParser.ParseText(masterString ?? "...");
		string[] multipleWeeklyDialogueSplit = masterString.Split("||");
		if (multipleWeeklyDialogueSplit.Length > 1)
		{
			masterString = multipleWeeklyDialogueSplit[Game1.stats.DaysPlayed / 7 % multipleWeeklyDialogueSplit.Length];
		}
		this.playerResponses?.Clear();
		string[] masterDialogueSplit = masterString.Split('#');
		for (int i = 0; i < masterDialogueSplit.Length; i++)
		{
			string curDialogue = masterDialogueSplit[i];
			if (curDialogue.Length < 2)
			{
				continue;
			}
			curDialogue = (masterDialogueSplit[i] = this.checkForSpecialCharacters(curDialogue));
			bool handledCommand = false;
			string commandToken;
			string commandArgs;
			if (curDialogue.StartsWith('$'))
			{
				string[] parts = ArgUtility.SplitBySpace(curDialogue, 2);
				commandToken = parts[0];
				commandArgs = ArgUtility.Get(parts, 1);
				handledCommand = true;
				if (commandToken == null)
				{
					goto IL_08a2;
				}
				int length = commandToken.Length;
				if (length != 2)
				{
					if (length != 6)
					{
						if (length != 7 || !(commandToken == "$action"))
						{
							goto IL_08a2;
						}
						this.dialogues.Add(new DialogueLine("", delegate
						{
							if (!TriggerActionManager.TryRunAction(commandArgs, out var error, out var exception))
							{
								error = $"Failed to parse {"$action"} token for {translationKey ?? this.speaker?.Name ?? ("\"" + masterString + "\"")}: {error}.";
								if (exception == null)
								{
									Game1.log.Warn(error);
								}
								else
								{
									Game1.log.Error(error, exception);
								}
							}
						}));
					}
					else
					{
						if (!(commandToken == "$query"))
						{
							goto IL_08a2;
						}
						string queryString = commandArgs;
						string[] dialogueOptions = ArgUtility.Get(masterString.Split('#', 2), 1)?.Split('|') ?? LegacyShims.EmptyArray<string>();
						masterDialogueSplit = (GameStateQuery.CheckConditions(queryString) ? dialogueOptions[0].Split('#') : ArgUtility.Get(dialogueOptions, 1, dialogueOptions[0]).Split('#'));
						i--;
					}
				}
				else
				{
					switch (commandToken[1])
					{
					case 'e':
						break;
					case 'b':
						goto IL_01a0;
					case 'k':
						goto IL_01b6;
					case '1':
						goto IL_01cc;
					case 'c':
						goto IL_01e2;
					case 't':
						goto IL_01f8;
					case 'q':
						goto IL_020e;
					case 'r':
						goto IL_0224;
					case 'p':
						goto IL_023a;
					case 'd':
						goto IL_0250;
					case 'y':
						goto IL_0266;
					default:
						goto IL_08a2;
					}
					if (!(commandToken == "$e"))
					{
						goto IL_08a2;
					}
				}
			}
			goto IL_08a5;
			IL_01b6:
			if (!(commandToken == "$k"))
			{
				goto IL_08a2;
			}
			goto IL_08a5;
			IL_08a2:
			handledCommand = false;
			goto IL_08a5;
			IL_020e:
			if (!(commandToken == "$q"))
			{
				goto IL_08a2;
			}
			if (this.dialogues.Count > 0)
			{
				this.dialogues[this.dialogues.Count - 1].Text += "{";
			}
			string[] questionSplit = ArgUtility.SplitBySpace(commandArgs);
			string[] answerIDs = questionSplit[0].Split('/');
			bool alreadySeenAnswer = false;
			for (int j = 0; j < answerIDs.Length; j++)
			{
				if (this.farmer.DialogueQuestionsAnswered.Contains(answerIDs[j]))
				{
					alreadySeenAnswer = true;
					break;
				}
			}
			if (alreadySeenAnswer && answerIDs[0] != "-1")
			{
				if (!questionSplit[1].Equals("null"))
				{
					masterDialogueSplit = masterDialogueSplit.Take(i).Concat(this.speaker.Dialogue[questionSplit[1]].Split('#')).ToArray();
					i--;
				}
			}
			else
			{
				this.isLastDialogueInteractive = true;
			}
			goto IL_08a5;
			IL_0224:
			if (!(commandToken == "$r"))
			{
				goto IL_08a2;
			}
			string[] responseSplit = ArgUtility.SplitBySpace(commandArgs);
			if (this.playerResponses == null)
			{
				this.playerResponses = new List<NPCDialogueResponse>();
			}
			this.isLastDialogueInteractive = true;
			this.playerResponses.Add(new NPCDialogueResponse(responseSplit[0], Convert.ToInt32(responseSplit[1]), responseSplit[2], masterDialogueSplit[i + 1]));
			i++;
			goto IL_08a5;
			IL_01cc:
			if (commandToken == "$1")
			{
				string messageId = ArgUtility.SplitBySpaceAndGet(commandArgs, 0);
				if (messageId != null)
				{
					if (this.farmer.mailReceived.Contains(messageId))
					{
						i += 3;
						if (i < masterDialogueSplit.Length)
						{
							masterDialogueSplit[i] = this.checkForSpecialCharacters(masterDialogueSplit[i]);
							this.dialogues.Add(new DialogueLine(masterDialogueSplit[i]));
						}
					}
					else
					{
						masterDialogueSplit[i + 1] = this.checkForSpecialCharacters(masterDialogueSplit[i + 1]);
						this.dialogues.Add(new DialogueLine(messageId + "}" + masterDialogueSplit[i + 1]));
						i = 99999;
					}
					goto IL_08a5;
				}
			}
			goto IL_08a2;
			IL_0266:
			if (!(commandToken == "$y"))
			{
				goto IL_08a2;
			}
			this.quickResponse = true;
			this.isLastDialogueInteractive = true;
			if (this.quickResponses == null)
			{
				this.quickResponses = new List<string>();
			}
			if (this.playerResponses == null)
			{
				this.playerResponses = new List<NPCDialogueResponse>();
			}
			string raw = curDialogue.Substring(curDialogue.IndexOf('\'') + 1);
			raw = raw.Substring(0, raw.Length - 1);
			string[] rawSplit = raw.Split('_');
			this.dialogues.Add(new DialogueLine(rawSplit[0]));
			for (int j2 = 1; j2 < rawSplit.Length; j2 += 2)
			{
				string choice = rawSplit[j2];
				string response = rawSplit[j2 + 1];
				if (response.Contains("*"))
				{
					response = response.Replace("**", "<<<<asterisk>>>>").Replace("*", "#$b#").Replace("<<<<asterisk>>>>", "*");
				}
				this.playerResponses.Add(new NPCDialogueResponse(null, -1, "quickResponse" + j2, Game1.parseText(choice)));
				this.quickResponses.Add(response);
			}
			goto IL_08a5;
			IL_023a:
			if (!(commandToken == "$p"))
			{
				goto IL_08a2;
			}
			string[] prerequisiteSplit = ArgUtility.SplitBySpace(commandArgs);
			string[] prerequisiteDialogueSplit = masterDialogueSplit[i + 1].Split('|');
			bool choseOne = false;
			for (int j3 = 0; j3 < prerequisiteSplit.Length; j3++)
			{
				if (this.farmer.DialogueQuestionsAnswered.Contains(prerequisiteSplit[j3]))
				{
					choseOne = true;
					break;
				}
			}
			if (choseOne)
			{
				masterDialogueSplit = prerequisiteDialogueSplit[0].Split('#');
				i = -1;
			}
			else
			{
				masterDialogueSplit[i + 1] = masterDialogueSplit[i + 1].Split('|').Last();
			}
			goto IL_08a5;
			IL_01a0:
			if (!(commandToken == "$b"))
			{
				goto IL_08a2;
			}
			if (this.dialogues.Count > 0)
			{
				this.dialogues[this.dialogues.Count - 1].Text += "{";
			}
			goto IL_08a5;
			IL_01f8:
			if (!(commandToken == "$t"))
			{
				goto IL_08a2;
			}
			this.dialogues.Add(new DialogueLine("", delegate
			{
				string[] array2 = ArgUtility.SplitBySpace(commandArgs);
				if (!ArgUtility.TryGet(array2, 0, out var value, out var error, allowBlank: false, "string topicId") || !ArgUtility.TryGetOptionalInt(array2, 1, out var value2, out error, 4, "int daysDuration"))
				{
					Game1.log.Warn($"Failed to parse {"$t"} token for {translationKey ?? this.speaker?.Name ?? ("\"" + masterString + "\"")}: {error}.");
				}
				else
				{
					Game1.player.activeDialogueEvents.TryAdd(value, value2);
				}
			}));
			goto IL_08a5;
			IL_08a5:
			if (!handledCommand)
			{
				curDialogue = this.applyGenderSwitch(curDialogue);
				this.dialogues.Add(new DialogueLine(curDialogue));
			}
			continue;
			IL_0250:
			if (!(commandToken == "$d"))
			{
				goto IL_08a2;
			}
			string[] array = ArgUtility.SplitBySpace(commandArgs);
			string prerequisiteDialogue = masterString.Substring(masterString.IndexOf('#') + 1);
			bool worldStateConfirmed = false;
			switch (array[0].ToLower())
			{
			case "joja":
				worldStateConfirmed = Game1.isLocationAccessible("JojaMart");
				break;
			case "cc":
			case "communitycenter":
				worldStateConfirmed = Game1.isLocationAccessible("CommunityCenter");
				break;
			case "bus":
				worldStateConfirmed = Game1.MasterPlayer.mailReceived.Contains("ccVault");
				break;
			case "kent":
				worldStateConfirmed = Game1.year >= 2;
				break;
			}
			char toLookFor = (prerequisiteDialogue.Contains('|') ? '|' : '#');
			masterDialogueSplit = ((!worldStateConfirmed) ? prerequisiteDialogue.Split(toLookFor)[1].Split('#') : prerequisiteDialogue.Split(toLookFor)[0].Split('#'));
			i--;
			goto IL_08a5;
			IL_01e2:
			if (commandToken == "$c")
			{
				string rawChance = ArgUtility.SplitBySpaceAndGet(commandArgs, 0);
				if (rawChance != null)
				{
					double chance = Convert.ToDouble(rawChance);
					if (!Game1.random.NextBool(chance))
					{
						i++;
					}
					else
					{
						this.dialogues.Add(new DialogueLine(masterDialogueSplit[i + 1]));
						i += 3;
					}
					goto IL_08a5;
				}
			}
			goto IL_08a2;
		}
	}

	public virtual void prepareDialogueForDisplay()
	{
		if (this.dialogues.Count > 0 && this.speaker != null && this.speaker.shouldWearIslandAttire.Value && Game1.player.friendshipData.TryGetValue(this.speaker.Name, out var friendship) && friendship.IsDivorced() && this.CurrentEmotion == "$u")
		{
			this.CurrentEmotion = "$neutral";
		}
	}

	/// <summary>Parse dialogue commands and tokens in the current dialogue (i.e. the <see cref="F:StardewValley.Dialogue.currentDialogueIndex" /> entry in <see cref="F:StardewValley.Dialogue.dialogues" />).</summary>
	public virtual void prepareCurrentDialogueForDisplay()
	{
		this.applyAndSkipPlainSideEffects();
		if (this.currentDialogueIndex >= this.dialogues.Count)
		{
			return;
		}
		string currentDialogue = this.dialogues[this.currentDialogueIndex].Text;
		currentDialogue = Utility.ParseGiftReveals(currentDialogue);
		this.showPortrait = true;
		if (currentDialogue.StartsWith("$v"))
		{
			string[] split = ArgUtility.SplitBySpace(currentDialogue);
			string eventId = split[1];
			bool checkPrecondition = true;
			bool checkSeen = true;
			if (split.Length > 2 && split[2] == "false")
			{
				checkPrecondition = false;
			}
			if (split.Length > 3 && split[3] == "false")
			{
				checkSeen = false;
			}
			if (Game1.PlayEvent(eventId, checkPrecondition, checkSeen))
			{
				this.dialogues.Clear();
				this.exitCurrentDialogue();
				return;
			}
			this.exitCurrentDialogue();
			if (!this.isDialogueFinished())
			{
				this.prepareCurrentDialogueForDisplay();
			}
			return;
		}
		if (currentDialogue.Contains('}'))
		{
			this.farmer.mailReceived.Add(currentDialogue.Split('}')[0]);
			currentDialogue = currentDialogue.Substring(currentDialogue.IndexOf("}") + 1);
			currentDialogue = currentDialogue.Replace("$k", "");
		}
		if (currentDialogue.Contains("$k"))
		{
			currentDialogue = currentDialogue.Replace("$k", "");
			this.dialogues.RemoveRange(this.currentDialogueIndex + 1, this.dialogues.Count - 1 - this.currentDialogueIndex);
			if (currentDialogue.Length < 2)
			{
				this.finishedLastDialogue = true;
			}
		}
		if (currentDialogue.StartsWith('%'))
		{
			bool isToken = false;
			string[] array = Dialogue.percentTokens;
			foreach (string token in array)
			{
				if (currentDialogue.StartsWith(token))
				{
					isToken = true;
					break;
				}
			}
			if (!isToken)
			{
				this.indexesWithoutPortrait.Add(this.currentDialogueIndex);
				this.showPortrait = false;
				currentDialogue = currentDialogue.Substring(1);
			}
		}
		else if (this.indexesWithoutPortrait.Contains(this.currentDialogueIndex))
		{
			this.showPortrait = false;
		}
		currentDialogue = this.ReplacePlayerEnteredStrings(currentDialogue);
		if (currentDialogue.Contains('['))
		{
			int open_index = -1;
			do
			{
				open_index = currentDialogue.IndexOf('[', Math.Max(open_index, 0));
				if (open_index < 0)
				{
					continue;
				}
				int close_index = currentDialogue.IndexOf(']', open_index);
				if (close_index < 0)
				{
					break;
				}
				string[] split2 = ArgUtility.SplitBySpace(currentDialogue.Substring(open_index + 1, close_index - open_index - 1));
				bool fail = false;
				string[] array = split2;
				for (int i = 0; i < array.Length; i++)
				{
					if (ItemRegistry.GetData(array[i]) == null)
					{
						fail = true;
						break;
					}
				}
				if (fail)
				{
					open_index++;
					continue;
				}
				Item item = ItemRegistry.Create(Game1.random.Choose(split2));
				if (item != null)
				{
					if (this.farmer.addItemToInventoryBool(item, makeActiveObject: true))
					{
						this.farmer.showCarrying();
					}
					else
					{
						this.farmer.addItemByMenuIfNecessary(item, null, forceQueue: true);
					}
				}
				currentDialogue = currentDialogue.Remove(open_index, close_index - open_index + 1);
			}
			while (open_index >= 0 && open_index < currentDialogue.Length);
		}
		currentDialogue = currentDialogue.Replace("%time", Game1.getTimeOfDayString(Game1.timeOfDay));
		bool? flag = this.speaker?.SpeaksDwarvish();
		if (flag.HasValue && flag == true && !this.farmer.canUnderstandDwarves)
		{
			currentDialogue = Dialogue.convertToDwarvish(currentDialogue);
		}
		this.dialogues[this.currentDialogueIndex].Text = currentDialogue;
	}

	public virtual string getCurrentDialogue()
	{
		if (this.currentDialogueIndex >= this.dialogues.Count || this.finishedLastDialogue)
		{
			return "";
		}
		if (this.dialogues.Count <= 0)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.792");
		}
		return this.dialogues[this.currentDialogueIndex].Text;
	}

	public bool isItemGrabDialogue()
	{
		if (this.currentDialogueIndex < this.dialogues.Count)
		{
			return this.dialogues[this.currentDialogueIndex].Text.Contains('[');
		}
		return false;
	}

	/// <summary>Whether we're currently displaying the last entry in <see cref="F:StardewValley.Dialogue.dialogues" /> which has text to display.</summary>
	public bool isOnFinalDialogue()
	{
		for (int i = this.currentDialogueIndex + 1; i < this.dialogues.Count; i++)
		{
			if (this.dialogues[i].HasText)
			{
				return false;
			}
		}
		return true;
	}

	public bool isDialogueFinished()
	{
		return this.finishedLastDialogue;
	}

	public string ReplacePlayerEnteredStrings(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		string farmer_name = Utility.FilterUserName(this.farmer.Name);
		str = str.Replace("@", farmer_name);
		if (str.Contains('%'))
		{
			str = str.Replace("%firstnameletter", farmer_name.Substring(0, Math.Max(0, farmer_name.Length / 2)));
			if (str.Contains("%spouse"))
			{
				if (this.farmer.spouse != null)
				{
					string spouseName = NPC.GetDisplayName(this.farmer.spouse);
					str = str.Replace("%spouse", spouseName);
				}
				else
				{
					long? spouseId = this.farmer.team.GetSpouse(this.farmer.UniqueMultiplayerID);
					if (spouseId.HasValue)
					{
						Farmer spouse = Game1.GetPlayer(spouseId.Value);
						str = str.Replace("%spouse", spouse.Name);
					}
				}
			}
			string farmName = Utility.FilterUserName(this.farmer.farmName.Value);
			str = str.Replace("%farm", farmName);
			string favoriteThing = Utility.FilterUserName(this.farmer.favoriteThing.Value);
			str = str.Replace("%favorite", favoriteThing);
			int kids = this.farmer.getNumberOfChildren();
			str = str.Replace("%kid1", (kids > 0) ? this.farmer.getChildren()[0].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.793"));
			str = str.Replace("%kid2", (kids > 1) ? this.farmer.getChildren()[1].displayName : Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.794"));
			str = str.Replace("%pet", this.farmer.getPetDisplayName());
		}
		return str;
	}

	public string checkForSpecialCharacters(string str)
	{
		str = this.applyGenderSwitch(str, altTokenOnly: true);
		if (str.Contains('%'))
		{
			str = str.Replace("%adj", Game1.random.Choose(Dialogue.adjectives).ToLower());
			if (str.Contains("%noun"))
			{
				str = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de) ? (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns)) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns))) : (str.Substring(0, str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns).ToLower()) + str.Substring(str.IndexOf("%noun") + "%noun".Length).Replace("%noun", Game1.random.Choose(Dialogue.nouns).ToLower())));
			}
			str = str.Replace("%place", Game1.random.Choose(Dialogue.places));
			str = str.Replace("%name", Dialogue.randomName());
			str = str.Replace("%band", Game1.samBandName);
			if (str.Contains("%book"))
			{
				str = str.Replace("%book", Game1.elliottBookName);
			}
			str = str.Replace("%year", Game1.year.ToString() ?? "");
			str = str.Replace("%season", Game1.CurrentSeasonDisplayName);
			if (str.Contains("%fork"))
			{
				str = str.Replace("%fork", "");
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.specialEventVariable1 = true;
				}
			}
		}
		return str;
	}

	/// <summary>Get the gender-appropriate dialogue from a dialogue string which may contain a gender-switch token.</summary>
	/// <param name="str">The dialogue string to parse.</param>
	/// <param name="altTokenOnly">Only apply the <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter2" /> token, and ignore <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter" />.</param>
	public string applyGenderSwitch(string str, bool altTokenOnly = false)
	{
		return Dialogue.applyGenderSwitch(this.farmer.Gender, str, altTokenOnly);
	}

	/// <summary>Get the gender-appropriate dialogue from a dialogue string which may contain gender-switch tokens.</summary>
	/// <param name="gender">The gender for which to apply tokens.</param>
	/// <param name="str">The dialogue string to parse.</param>
	/// <param name="altTokenOnly">Only apply the <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter2" /> token, and ignore <see cref="F:StardewValley.Dialogue.genderDialogueSplitCharacter" />.</param>
	public static string applyGenderSwitch(Gender gender, string str, bool altTokenOnly = false)
	{
		str = Dialogue.applyGenderSwitchBlocks(gender, str);
		int splitIndex = ((!altTokenOnly) ? str.IndexOf('^') : (-1));
		if (splitIndex == -1)
		{
			splitIndex = str.IndexOf('¦');
		}
		if (splitIndex != -1)
		{
			str = ((gender == Gender.Male) ? str.Substring(0, splitIndex) : str.Substring(splitIndex + 1));
		}
		return str;
	}

	/// <summary>Replace gender-switch blocks like <c>${male^female}$</c> or <c>${male¦female}$</c> in the input string with the gender-appropriate text.</summary>
	/// <param name="gender">The gender for which to apply tokens.</param>
	/// <param name="str">The dialogue string to parse.</param>
	/// <remarks>This should only be called directly in cases where <see cref="M:StardewValley.Dialogue.applyGenderSwitch(StardewValley.Gender,System.String,System.Boolean)" /> isn't applied, since that includes gender-switch blocks.</remarks>
	public static string applyGenderSwitchBlocks(Gender gender, string str)
	{
		int startIndex = 0;
		while (true)
		{
			int index = str.IndexOf("${", startIndex, StringComparison.Ordinal);
			if (index == -1)
			{
				return str;
			}
			int endIndex = str.IndexOf("}$", index, StringComparison.Ordinal);
			if (endIndex == -1)
			{
				break;
			}
			string originalSubstr = str.Substring(index + 2, endIndex - index - 2);
			string[] parts = (originalSubstr.Contains('¦') ? originalSubstr.Split('¦') : originalSubstr.Split('^'));
			string newSubstr = gender switch
			{
				Gender.Male => parts[0], 
				Gender.Female => ArgUtility.Get(parts, 1, parts[0]), 
				_ => ArgUtility.Get(parts, 2, parts[0]), 
			};
			str = str.Substring(0, index) + newSubstr + str.Substring(endIndex + "}$".Length);
			startIndex = index + newSubstr.Length;
		}
		return str;
	}

	/// <summary>If the next dialogue(s) in <see cref="F:StardewValley.Dialogue.dialogues" /> have side-effects without text, apply them and set <see cref="F:StardewValley.Dialogue.currentDialogueIndex" /> to the next dialogue which has text.</summary>
	public void applyAndSkipPlainSideEffects()
	{
		while (this.currentDialogueIndex < this.dialogues.Count)
		{
			DialogueLine entry = this.dialogues[this.currentDialogueIndex];
			if (!entry.HasText)
			{
				entry.SideEffects?.Invoke();
				this.currentDialogueIndex++;
				continue;
			}
			break;
		}
	}

	public static string randomName()
	{
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.ja:
		{
			string[] names3 = new string[38]
			{
				"ローゼン", "ミルド", "ココ", "ナミ", "こころ", "サルコ", "ハンゾー", "クッキー", "ココナツ", "せん",
				"ハル", "ラン", "オサム", "ヨシ", "ソラ", "ホシ", "まこと", "マサ", "ナナ", "リオ",
				"リン", "フジ", "うどん", "ミント", "さくら", "ボンボン", "レオ", "モリ", "コーヒー", "ミルク",
				"マロン", "クルミ", "サムライ", "カミ", "ゴロ", "マル", "チビ", "ユキダマ"
			};
			return Game1.random.Choose(names3);
		}
		case LocalizedContentManager.LanguageCode.zh:
		{
			string[] names = new string[183]
			{
				"雨果", "蛋挞", "小百合", "毛毛", "小雨", "小溪", "精灵", "安琪儿", "小糕", "玫瑰",
				"小黄", "晓雨", "阿江", "铃铛", "马琪", "果粒", "郁金香", "小黑", "雨露", "小江",
				"灵力", "萝拉", "豆豆", "小莲", "斑点", "小雾", "阿川", "丽丹", "玛雅", "阿豆",
				"花花", "琉璃", "滴答", "阿山", "丹麦", "梅西", "橙子", "花儿", "晓璃", "小夕",
				"山大", "咪咪", "卡米", "红豆", "花朵", "洋洋", "太阳", "小岩", "汪汪", "玛利亚",
				"小菜", "花瓣", "阳阳", "小夏", "石头", "阿狗", "邱洁", "苹果", "梨花", "小希",
				"天天", "浪子", "阿猫", "艾薇儿", "雪梨", "桃花", "阿喜", "云朵", "风儿", "狮子",
				"绮丽", "雪莉", "樱花", "小喜", "朵朵", "田田", "小红", "宝娜", "梅子", "小樱",
				"嘻嘻", "云儿", "小草", "小黄", "纳香", "阿梅", "茶花", "哈哈", "芸儿", "东东",
				"小羽", "哈豆", "桃子", "茶叶", "双双", "沫沫", "楠楠", "小爱", "麦当娜", "杏仁",
				"椰子", "小王", "泡泡", "小林", "小灰", "马格", "鱼蛋", "小叶", "小李", "晨晨",
				"小琳", "小慧", "布鲁", "晓梅", "绿叶", "甜豆", "小雪", "晓林", "康康", "安妮",
				"樱桃", "香板", "甜甜", "雪花", "虹儿", "美美", "葡萄", "薇儿", "金豆", "雪玲",
				"瑶瑶", "龙眼", "丁香", "晓云", "雪豆", "琪琪", "麦子", "糖果", "雪丽", "小艺",
				"小麦", "小圆", "雨佳", "小火", "麦茶", "圆圆", "春儿", "火灵", "板子", "黑点",
				"冬冬", "火花", "米粒", "喇叭", "晓秋", "跟屁虫", "米果", "欢欢", "爱心", "松子",
				"丫头", "双子", "豆芽", "小子", "彤彤", "棉花糖", "阿贵", "仙儿", "冰淇淋", "小彬",
				"贤儿", "冰棒", "仔仔", "格子", "水果", "悠悠", "莹莹", "巧克力", "梦洁", "汤圆",
				"静香", "茄子", "珍珠"
			};
			return Game1.random.Choose(names);
		}
		case LocalizedContentManager.LanguageCode.ru:
		{
			string[] names2 = new string[50]
			{
				"Августина", "Альф", "Анфиса", "Ариша", "Афоня", "Баламут", "Балкан", "Бандит", "Бланка", "Бобик",
				"Боня", "Борька", "Буренка", "Бусинка", "Вася", "Гаврюша", "Глаша", "Гоша", "Дуня", "Дуся",
				"Зорька", "Ивонна", "Игнат", "Кеша", "Клара", "Кузя", "Лада", "Максимус", "Маня", "Марта",
				"Маруся", "Моня", "Мотя", "Мурзик", "Мурка", "Нафаня", "Ника", "Нюша", "Проша", "Пятнушка",
				"Сеня", "Сивка", "Тихон", "Тоша", "Фунтик", "Шайтан", "Юнона", "Юпитер", "Ягодка", "Яшка"
			};
			return Game1.random.Choose(names2);
		}
		default:
		{
			int nameLength = Game1.random.Next(3, 6);
			string[] startingConsonants = new string[24]
			{
				"B", "Br", "J", "F", "S", "M", "C", "Ch", "L", "P",
				"K", "W", "G", "Z", "Tr", "T", "Gr", "Fr", "Pr", "N",
				"Sn", "R", "Sh", "St"
			};
			string[] consonants = new string[12]
			{
				"ll", "tch", "l", "m", "n", "p", "r", "s", "t", "c",
				"rt", "ts"
			};
			string[] vowels = new string[5] { "a", "e", "i", "o", "u" };
			string[] consonantEndings = new string[5] { "ie", "o", "a", "ers", "ley" };
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
			dictionary["a"] = new string[6] { "nie", "bell", "bo", "boo", "bella", "s" };
			dictionary["e"] = new string[4] { "ll", "llo", "", "o" };
			dictionary["i"] = new string[18]
			{
				"ck", "e", "bo", "ba", "lo", "la", "to", "ta", "no", "na",
				"ni", "a", "o", "zor", "que", "ca", "co", "mi"
			};
			dictionary["o"] = new string[12]
			{
				"nie", "ze", "dy", "da", "o", "ver", "la", "lo", "s", "ny",
				"mo", "ra"
			};
			dictionary["u"] = new string[4] { "rt", "mo", "", "s" };
			Dictionary<string, string[]> endings = dictionary;
			dictionary = new Dictionary<string, string[]>();
			dictionary["a"] = new string[12]
			{
				"nny", "sper", "trina", "bo", "-bell", "boo", "lbert", "sko", "sh", "ck",
				"ishe", "rk"
			};
			dictionary["e"] = new string[9] { "lla", "llo", "rnard", "cardo", "ffe", "ppo", "ppa", "tch", "x" };
			dictionary["i"] = new string[18]
			{
				"llard", "lly", "lbo", "cky", "card", "ne", "nnie", "lbert", "nono", "nano",
				"nana", "ana", "nsy", "msy", "skers", "rdo", "rda", "sh"
			};
			dictionary["o"] = new string[17]
			{
				"nie", "zzy", "do", "na", "la", "la", "ver", "ng", "ngus", "ny",
				"-mo", "llo", "ze", "ra", "ma", "cco", "z"
			};
			dictionary["u"] = new string[11]
			{
				"ssie", "bbie", "ffy", "bba", "rt", "s", "mby", "mbo", "mbus", "ngus",
				"cky"
			};
			Dictionary<string, string[]> endingsForShortNames = dictionary;
			string name = startingConsonants[Game1.random.Next(startingConsonants.Length - 1)];
			for (int i = 1; i < nameLength - 1; i++)
			{
				name = ((i % 2 != 0) ? (name + Game1.random.Choose(vowels)) : (name + Game1.random.Choose(consonants)));
				if (name.Length >= nameLength)
				{
					break;
				}
			}
			string lastLetter = name[name.Length - 1].ToString();
			if (Game1.random.NextBool() && !vowels.Contains(lastLetter))
			{
				name += Game1.random.Choose(consonantEndings);
			}
			else if (vowels.Contains(lastLetter))
			{
				if (Game1.random.NextDouble() < 0.8)
				{
					name = ((name.Length > 3) ? (name + Game1.random.ChooseFrom(endings[lastLetter])) : (name + Game1.random.ChooseFrom(endingsForShortNames[lastLetter])));
				}
			}
			else
			{
				name += Game1.random.Choose(vowels);
			}
			for (int i2 = name.Length - 1; i2 > 2; i2--)
			{
				if (vowels.Contains(name[i2].ToString()) && vowels.Contains(name[i2 - 2].ToString()))
				{
					switch (name[i2 - 1])
					{
					case 'c':
						name = name.Substring(0, i2) + "k" + name.Substring(i2);
						i2--;
						break;
					case 'r':
						name = name.Substring(0, i2 - 1) + "k" + name.Substring(i2);
						i2--;
						break;
					case 'l':
						name = name.Substring(0, i2 - 1) + "n" + name.Substring(i2);
						i2--;
						break;
					}
				}
			}
			if (name.Length <= 3 && Game1.random.NextDouble() < 0.1)
			{
				name = (Game1.random.NextBool() ? (name + name) : (name + "-" + name));
			}
			if (name.Length <= 2 && name.Last() == 'e')
			{
				name += Game1.random.Choose('m', 'p', 'b');
			}
			return Dialogue.ReplaceBadRandomName(name);
		}
		}
	}

	/// <summary>Get an alternative name if the input name contains bad words.</summary>
	/// <param name="name">The random name </param>
	public static string ReplaceBadRandomName(string name)
	{
		string lowerName = name.ToLower();
		if (lowerName.Contains("bitch") || lowerName.Contains("cock") || lowerName.Contains("cum") || lowerName.Contains("fuck") || lowerName.Contains("goock") || lowerName.Contains("gook") || lowerName.Contains("kike") || lowerName.Contains("nigg") || lowerName.Contains("pusie") || lowerName.Contains("puss") || lowerName.Contains("puta") || lowerName.Contains("rape") || lowerName.Contains("sex") || lowerName.Contains("shart") || lowerName.Contains("shit") || lowerName.Contains("taboo") || lowerName.Contains("trann") || lowerName.Contains("willy"))
		{
			return Game1.random.Choose("Bobo", "Wumbus");
		}
		switch (lowerName)
		{
		case "boner":
		case "boners":
			return "Boneo";
		case "bussie":
			return "Busu";
		case "cucka":
		case "cucke":
		case "cucko":
		case "cucky":
		case "cuckas":
		case "cuckie":
		case "cuckos":
		case "cuckers":
			return "Cubbie";
		case "grope":
		case "gropers":
			return "Gropello";
		case "natsi":
			return "Natsia";
		case "packi":
		case "packie":
			return "Packina";
		case "penos":
		case "penus":
			return "Penono";
		case "rapie":
			return "Rapimi";
		case "trapi":
		case "trani":
		case "tranie":
		case "trapie":
		case "trananie":
			return "Tranello";
		default:
			return name;
		}
	}

	public virtual string exitCurrentDialogue()
	{
		if (this.isOnFinalDialogue())
		{
			this.currentDialogueIndex++;
			this.applyAndSkipPlainSideEffects();
			this.onFinish?.Invoke();
		}
		bool num = this.isCurrentStringContinuedOnNextScreen;
		if (this.currentDialogueIndex < this.dialogues.Count - 1)
		{
			this.currentDialogueIndex++;
			this.applyAndSkipPlainSideEffects();
			this.checkForSpecialDialogueAttributes();
		}
		else
		{
			this.finishedLastDialogue = true;
		}
		if (num)
		{
			return this.getCurrentDialogue();
		}
		return null;
	}

	private void checkForSpecialDialogueAttributes()
	{
		this.CurrentEmotion = null;
		this.isCurrentStringContinuedOnNextScreen = false;
		this.dontFaceFarmer = false;
		if (this.currentDialogueIndex < this.dialogues.Count)
		{
			DialogueLine dialogueLine = this.dialogues[this.currentDialogueIndex];
			if (dialogueLine.Text.Contains("{"))
			{
				dialogueLine.Text = dialogueLine.Text.Replace("{", "");
				this.isCurrentStringContinuedOnNextScreen = true;
			}
			if (dialogueLine.Text.Contains("%noturn"))
			{
				dialogueLine.Text = dialogueLine.Text.Replace("%noturn", "");
				this.dontFaceFarmer = true;
			}
			this.checkEmotions();
		}
	}

	private void checkEmotions()
	{
		this.CurrentEmotion = null;
		if (this.currentDialogueIndex >= this.dialogues.Count)
		{
			return;
		}
		DialogueLine dialogueLine = this.dialogues[this.currentDialogueIndex];
		string text = dialogueLine.Text;
		int emoteIndex = text.IndexOf('$');
		if (emoteIndex == -1 || this.dialogues.Count <= 0)
		{
			return;
		}
		if (text.Contains("$h"))
		{
			this.CurrentEmotion = "$h";
			dialogueLine.Text = text.Replace("$h", "");
			return;
		}
		if (text.Contains("$s"))
		{
			this.CurrentEmotion = "$s";
			dialogueLine.Text = text.Replace("$s", "");
			return;
		}
		if (text.Contains("$u"))
		{
			this.CurrentEmotion = "$u";
			dialogueLine.Text = text.Replace("$u", "");
			return;
		}
		if (text.Contains("$l"))
		{
			this.CurrentEmotion = "$l";
			dialogueLine.Text = text.Replace("$l", "");
			return;
		}
		if (text.Contains("$a"))
		{
			this.CurrentEmotion = "$a";
			dialogueLine.Text = text.Replace("$a", "");
			return;
		}
		int digits = 0;
		for (int i = emoteIndex + 1; i < text.Length && char.IsDigit(text[i]); i++)
		{
			digits++;
		}
		if (digits > 0)
		{
			string emote = (this.CurrentEmotion = text.Substring(emoteIndex, digits + 1));
			dialogueLine.Text = text.Replace(emote, "");
		}
	}

	public List<NPCDialogueResponse> getNPCResponseOptions()
	{
		return this.playerResponses;
	}

	public Response[] getResponseOptions()
	{
		return this.playerResponses.Cast<Response>().ToArray();
	}

	public bool isCurrentDialogueAQuestion()
	{
		if (this.isLastDialogueInteractive)
		{
			return this.currentDialogueIndex == this.dialogues.Count - 1;
		}
		return false;
	}

	public virtual bool chooseResponse(Response response)
	{
		for (int i = 0; i < this.playerResponses.Count; i++)
		{
			if (this.playerResponses[i].responseKey == null || response.responseKey == null || !this.playerResponses[i].responseKey.Equals(response.responseKey))
			{
				continue;
			}
			if (this.answerQuestionBehavior != null)
			{
				if (this.answerQuestionBehavior(i))
				{
					Game1.currentSpeaker = null;
				}
				this.isLastDialogueInteractive = false;
				this.finishedLastDialogue = true;
				this.answerQuestionBehavior = null;
				return true;
			}
			if (this.quickResponse)
			{
				this.isLastDialogueInteractive = false;
				this.finishedLastDialogue = true;
				this.isCurrentStringContinuedOnNextScreen = true;
				this.speaker.setNewDialogue(new Dialogue(this.speaker, null, this.quickResponses[i]));
				Game1.drawDialogue(this.speaker);
				this.speaker.faceTowardFarmerForPeriod(4000, 3, faceAway: false, this.farmer);
				return true;
			}
			if (Game1.isFestival())
			{
				Game1.currentLocation.currentEvent.answerDialogueQuestion(this.speaker, this.playerResponses[i].responseKey);
				this.isLastDialogueInteractive = false;
				this.finishedLastDialogue = true;
				return false;
			}
			this.farmer.changeFriendship(this.playerResponses[i].friendshipChange, this.speaker);
			if (this.playerResponses[i].id != null)
			{
				this.farmer.addSeenResponse(this.playerResponses[i].id);
			}
			if (this.playerResponses[i].extraArgument != null)
			{
				try
				{
					this.performDialogueResponseExtraArgument(this.farmer, this.playerResponses[i].extraArgument);
				}
				catch (Exception)
				{
				}
			}
			this.isLastDialogueInteractive = false;
			this.finishedLastDialogue = false;
			this.parseDialogueString(this.speaker.Dialogue[this.playerResponses[i].responseKey], this.speaker.LoadedDialogueKey + ":" + this.playerResponses[i].responseKey);
			this.isCurrentStringContinuedOnNextScreen = true;
			return false;
		}
		return false;
	}

	public void performDialogueResponseExtraArgument(Farmer farmer, string argument)
	{
		string[] split = argument.Split("_");
		if (split[0].EqualsIgnoreCase("friend"))
		{
			farmer.changeFriendship(Convert.ToInt32(split[2]), Game1.getCharacterFromName(split[1]));
		}
	}

	/// <summary>Convert the current dialogue text into Dwarvish, as spoken by Dwarf when the player doesn't have the Dwarvish Translation Guide.</summary>
	public void convertToDwarvish()
	{
		for (int i = 0; i < this.dialogues.Count; i++)
		{
			this.dialogues[i].Text = Dialogue.convertToDwarvish(this.dialogues[i].Text);
		}
	}

	/// <summary>Convert dialogue text into Dwarvish, as spoken by Dwarf when the player doesn't have the Dwarvish Translation Guide.</summary>
	/// <param name="str">The text to translate.</param>
	public static string convertToDwarvish(string str)
	{
		if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
		{
			string charset1 = "bcdfghjklmnpqrstvwxyz";
			string charset2 = "bcd fghj klmn pqrst vwxy z";
			StringBuilder result = new StringBuilder();
			bool nextCapital = true;
			foreach (char cjk in str)
			{
				int code = cjk;
				if ((19968 <= code && code <= 40959) || (12352 <= code && code <= 12543) || cjk == '々' || (44032 <= code && code <= 55215))
				{
					char char1 = charset1[code % charset1.Length];
					if (nextCapital)
					{
						char1 = char.ToUpper(char1);
						nextCapital = false;
					}
					result.Append(char1);
					char char2 = charset2[(code >> 1) % charset2.Length];
					result.Append(char2);
				}
				else
				{
					result.Append(cjk);
					if (cjk != ' ')
					{
						nextCapital = true;
					}
				}
			}
			return result.ToString();
		}
		StringBuilder translated = new StringBuilder();
		for (int j = 0; j < str.Length; j++)
		{
			switch (str[j])
			{
			case 'a':
				translated.Append('o');
				continue;
			case 'e':
				translated.Append('u');
				continue;
			case 'i':
				translated.Append("e");
				continue;
			case 'o':
				translated.Append('a');
				continue;
			case 'u':
				translated.Append("i");
				continue;
			case 'y':
				translated.Append("ol");
				continue;
			case 'z':
				translated.Append('b');
				continue;
			case 'A':
				translated.Append('O');
				continue;
			case 'E':
				translated.Append('U');
				continue;
			case 'I':
				translated.Append("E");
				continue;
			case 'O':
				translated.Append('A');
				continue;
			case 'U':
				translated.Append("I");
				continue;
			case 'Y':
				translated.Append("Ol");
				continue;
			case 'Z':
				translated.Append('B');
				continue;
			case '1':
				translated.Append('M');
				continue;
			case '5':
				translated.Append('X');
				continue;
			case '9':
				translated.Append('V');
				continue;
			case '0':
				translated.Append('Q');
				continue;
			case 'g':
				translated.Append('l');
				continue;
			case 'c':
				translated.Append('t');
				continue;
			case 't':
				translated.Append('n');
				continue;
			case 'd':
				translated.Append('p');
				continue;
			case ' ':
			case '!':
			case '"':
			case '\'':
			case ',':
			case '.':
			case '?':
			case 'h':
			case 'm':
			case 's':
				translated.Append(str[j]);
				continue;
			case '\n':
			case 'n':
			case 'p':
				continue;
			}
			if (char.IsLetterOrDigit(str[j]))
			{
				translated.Append((char)(str[j] + 2));
			}
		}
		return translated.ToString().Replace("nhu", "doo");
	}
}
