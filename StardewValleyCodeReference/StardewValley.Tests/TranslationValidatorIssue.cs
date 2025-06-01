namespace StardewValley.Tests;

/// <summary>An issue type detected by the translation validator.</summary>
public enum TranslationValidatorIssue
{
	/// <summary>The translations are missing a key found in the base data.</summary>
	MissingKey,
	/// <summary>The translations have an extra key that doesn't exist in the base data.</summary>
	UnknownKey,
	/// <summary>The translation produces a different <see cref="T:StardewValley.Tests.SyntaxAbstractor">syntactic representation</see> than the base key.</summary>
	SyntaxMismatch,
	/// <summary>A syntax block is malformed or invalid.</summary>
	MalformedSyntax
}
