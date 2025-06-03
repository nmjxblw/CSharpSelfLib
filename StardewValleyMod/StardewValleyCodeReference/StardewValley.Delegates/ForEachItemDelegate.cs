using StardewValley.Internal;

namespace StardewValley.Delegates;

/// <summary>A callback invoked when iterating all items in the game.</summary>
/// <returns>Returns whether to continue iterating items in the game.</returns>
public delegate bool ForEachItemDelegate(in ForEachItemContext context);
