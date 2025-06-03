namespace StardewValley.Tests;

/// <summary>An issue found when validating a translation.</summary>
public class TranslationValidatorResult
{
	/// <summary>The issue type that was detected.</summary>
	public TranslationValidatorIssue Issue { get; }

	/// <summary>The translation key for which an issue was found.</summary>
	public string Key { get; }

	/// <summary>The syntactic representation of the base text produced by <see cref="T:StardewValley.Tests.SyntaxAbstractor" />.</summary>
	public string BaseSyntax { get; }

	/// <summary>The raw text from the base asset.</summary>
	public string BaseText { get; }

	/// <summary>The syntactic representation of the translated text produced by <see cref="T:StardewValley.Tests.SyntaxAbstractor" />.</summary>
	public string TranslationSyntax { get; }

	/// <summary>The raw text from the translated asset.</summary>
	public string TranslationText { get; }

	/// <summary>A human-readable error indicating what the issue is.</summary>
	public string SuggestedError { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="issue"><inheritdoc cref="P:StardewValley.Tests.TranslationValidatorResult.Issue" path="/summary" /></param>
	/// <param name="key"><inheritdoc cref="P:StardewValley.Tests.TranslationValidatorResult.Key" path="/summary" /></param>
	/// <param name="baseSyntax"><inheritdoc cref="P:StardewValley.Tests.TranslationValidatorResult.BaseSyntax" path="/summary" /></param>
	/// <param name="baseText"><inheritdoc cref="P:StardewValley.Tests.TranslationValidatorResult.BaseText" path="/summary" /></param>
	/// <param name="translationSyntax"><inheritdoc cref="P:StardewValley.Tests.TranslationValidatorResult.TranslationSyntax" path="/summary" /></param>
	/// <param name="translationText"><inheritdoc cref="P:StardewValley.Tests.TranslationValidatorResult.TranslationText" path="/summary" /></param>
	/// <param name="suggestedError"><inheritdoc cref="P:StardewValley.Tests.TranslationValidatorResult.SuggestedError" path="/summary" /></param>
	public TranslationValidatorResult(TranslationValidatorIssue issue, string key, string baseSyntax, string baseText, string translationSyntax, string translationText, string suggestedError)
	{
		this.Issue = issue;
		this.Key = key;
		this.BaseSyntax = baseSyntax;
		this.BaseText = baseText;
		this.TranslationSyntax = translationSyntax;
		this.TranslationText = translationText;
		this.SuggestedError = suggestedError;
	}
}
