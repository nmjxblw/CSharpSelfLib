namespace StardewValley.Tests;

/// <summary>Get a syntactic representation from a specific asset type for <see cref="T:StardewValley.Tests.SyntaxAbstractor" />.</summary>
/// <param name="syntaxAbstractor">The syntax abstractor instance to use.</param>
/// <param name="baseAssetName">The asset name without the locale suffix, like <c>Data/Achievements</c>.</param>
/// <param name="key">The entry key.</param>
/// <param name="text">The entry value to parse.</param>
public delegate string ExtractSyntaxDelegate(SyntaxAbstractor syntaxAbstractor, string baseAssetName, string key, string text);
