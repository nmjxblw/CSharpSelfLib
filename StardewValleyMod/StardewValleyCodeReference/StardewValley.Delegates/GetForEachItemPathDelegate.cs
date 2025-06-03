using System.Collections.Generic;

namespace StardewValley.Delegates;

/// <summary>Get the contextual path leading to an item in the world. For example, an item inside a chest would have the location and chest as path vlaues.</summary>
public delegate IList<object> GetForEachItemPathDelegate();
