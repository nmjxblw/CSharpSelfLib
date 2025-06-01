using System;
using System.Collections.Generic;

namespace StardewValley.Tests;

/// <summary>Provides methods to compare and validate translations, used in the game's internal unit tests.</summary>
public class TranslationValidator
{
	/// <summary>Converts raw text into language-independent syntax representations, which can be compared between languages.</summary>
	private readonly SyntaxAbstractor Abstractor = new SyntaxAbstractor();

	/// <summary>Compare the base and translated variants of an asset and return a list of keys which are missing, unknown, or have a different syntax.</summary>
	/// <typeparam name="TValue">The value type in the asset data.</typeparam>
	/// <param name="baseData">The original untranslated data.</param>
	/// <param name="translatedData">The translated data.</param>
	/// <param name="getText">Get the text to compare for an entry.</param>
	/// <param name="baseAssetName">The asset name without the locale suffix, like <c>Data/Achievements</c>.</param>
	public IEnumerable<TranslationValidatorResult> Compare<TValue>(Dictionary<string, TValue> baseData, Dictionary<string, TValue> translatedData, Func<TValue, string> getText, string baseAssetName)
	{
		return this.Compare(baseData, translatedData, getText, (string key, string text) => this.Abstractor.ExtractSyntaxFor(baseAssetName, key, text));
	}

	/// <summary>Compare the base and translated variants of an asset and return a list of keys which are missing, unknown, or have a different syntax.</summary>
	/// <typeparam name="TValue">The value type in the asset data.</typeparam>
	/// <param name="baseData">The original untranslated data.</param>
	/// <param name="translatedData">The translated data.</param>
	/// <param name="getText">Get the text to compare for an entry.</param>
	/// <param name="getSyntax">Get the syntax for a data entry, given its key and value.</param>
	public IEnumerable<TranslationValidatorResult> Compare<TValue>(Dictionary<string, TValue> baseData, Dictionary<string, TValue> translatedData, Func<TValue, string> getText, Func<string, string, string> getSyntax)
	{
		foreach (KeyValuePair<string, TValue> basePair in baseData)
		{
			string key = basePair.Key;
			string baseText = getText(basePair.Value);
			if (!translatedData.TryGetValue(key, out var translationEntry))
			{
				yield return new TranslationValidatorResult(TranslationValidatorIssue.MissingKey, key, getSyntax(key, baseText), baseText, null, null, "Key not found in the translated asset.");
				continue;
			}
			string translationText = getText(translationEntry);
			TranslationValidatorResult syntaxResult = this.CompareEntry(key, baseText, translationText, (string value) => getSyntax(key, value));
			if (syntaxResult != null)
			{
				yield return syntaxResult;
			}
		}
		foreach (KeyValuePair<string, TValue> translatedPair in translatedData)
		{
			string key2 = translatedPair.Key;
			if (!baseData.ContainsKey(key2))
			{
				string translationText2 = getText(translatedPair.Value);
				string translationSyntax = getSyntax(key2, translationText2);
				yield return new TranslationValidatorResult(TranslationValidatorIssue.UnknownKey, key2, null, null, translationSyntax, translationText2, "Unknown key in translation which isn't in the base asset.");
			}
		}
	}

	/// <summary>Compare the base and translated variants of a single entry in an asset and return a result if the entries have a different syntax.</summary>
	/// <param name="key">The key for this entry in the asset.</param>
	/// <param name="baseText">The original untranslated text.</param>
	/// <param name="translationText">The translated text.</param>
	/// <param name="getSyntax">Get the syntax for an entry, given its value.</param>
	/// <returns>Returns the validator result if an issue was found, else <c>null</c>.</returns>
	public TranslationValidatorResult CompareEntry(string key, string baseText, string translationText, Func<string, string> getSyntax)
	{
		string baseSyntax = getSyntax(baseText);
		string translationSyntax = getSyntax(translationText);
		if (baseSyntax != translationSyntax)
		{
			return new TranslationValidatorResult(TranslationValidatorIssue.SyntaxMismatch, key, baseSyntax, baseText, translationSyntax, translationText, $"The translation has a different syntax than the base text.\nSyntax:\n    base:  {baseSyntax}\n    local: {translationSyntax}\n           {"".PadRight(this.GetDiffIndex(baseSyntax, translationSyntax), ' ')}^\nText:\n    base:  {baseText}\n    local: {translationText}\n\n           {"".PadRight(this.GetDiffIndex(baseText, translationText), ' ')}^\n");
		}
		if (!this.ValidateGenderSwitchBlocks(baseText, out var error, out var errorBlock))
		{
			return new TranslationValidatorResult(TranslationValidatorIssue.MalformedSyntax, key, baseSyntax, baseText, translationSyntax, translationText, $"Base text has invalid gender switch block: {error}.\nAffected block: {errorBlock}.");
		}
		if (!this.ValidateGenderSwitchBlocks(baseText, out error, out errorBlock))
		{
			return new TranslationValidatorResult(TranslationValidatorIssue.MalformedSyntax, key, baseSyntax, baseText, translationSyntax, translationText, $"Translated text has invalid gender switch block: {error}.\nAffected block: {errorBlock}.");
		}
		return null;
	}

	/// <summary>Validate that all gender-switch blocks in a given text are correctly formatted.</summary>
	/// <param name="text">The text which may contain gender-switch blocks to validate.</param>
	/// <param name="error">If applicable, a human-readable phrase indicating why the gender-switch blocks are invalid.</param>
	/// <param name="errorBlock">The gender-switch block which is invalid.</param>
	public bool ValidateGenderSwitchBlocks(string text, out string error, out string errorBlock)
	{
		int minIndex = 0;
		while (true)
		{
			int start = text.IndexOf("${", minIndex, StringComparison.OrdinalIgnoreCase);
			if (start == -1)
			{
				break;
			}
			int end = text.IndexOf("}$", start, StringComparison.OrdinalIgnoreCase);
			if (end == -1)
			{
				error = "closing '}$' not found";
				errorBlock = text.Substring(start);
				return false;
			}
			errorBlock = text.Substring(start, end - start);
			string text2 = text.Substring(start + 2, end - start - 2);
			char splitCharacter = (text2.Contains('^') ? '^' : '¦');
			string[] branches = text2.Split(splitCharacter);
			if (text2.Contains("${"))
			{
				error = "can't start a new gender-switch block inside another";
				return false;
			}
			if (branches.Length < 2)
			{
				error = $"must have at least two branches delimited by {94} or {166}";
				return false;
			}
			if (branches.Length > 3)
			{
				error = $"found {branches.Length} branches delimited by {splitCharacter}, must be two (male{splitCharacter}female) or three (male{splitCharacter}female{splitCharacter}other)";
				return false;
			}
			string firstSyntax = this.Abstractor.ExtractDialogueSyntax(branches[0]);
			for (int i = 1; i < branches.Length; i++)
			{
				string curSyntax = this.Abstractor.ExtractDialogueSyntax(branches[1]);
				if (firstSyntax != curSyntax)
				{
					error = $"branches have different syntax (0: `{firstSyntax}`, {i}: `{curSyntax}`)";
					return false;
				}
			}
			minIndex = end + 2;
		}
		error = null;
		errorBlock = null;
		return true;
	}

	/// <summary>Get the index at which two strings first differ.</summary>
	/// <param name="baseText">The base text being compare to.</param>
	/// <param name="translatedText">The translated text to compare with the base text.</param>
	public int GetDiffIndex(string baseText, string translatedText)
	{
		int minLength = Math.Min(baseText.Length, translatedText.Length);
		for (int i = 0; i < minLength; i++)
		{
			if (baseText[i] != translatedText[i])
			{
				return i;
			}
		}
		return minLength;
	}
}
