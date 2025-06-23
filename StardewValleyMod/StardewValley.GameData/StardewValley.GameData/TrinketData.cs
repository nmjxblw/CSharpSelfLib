using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The data for a trinket item.</summary>
public class TrinketData
{
	/// <summary>A tokenizable string for the item display name.</summary>
	public string DisplayName;

	/// <summary>A tokenizable string for the item description.</summary>
	public string Description;

	/// <summary>The asset name for the texture containing the item sprite. This should contain a grid of 16x16 sprites.</summary>
	public string Texture;

	/// <summary>The sprite index for this trinket within the <see cref="F:StardewValley.GameData.TrinketData.Texture" />.</summary>
	public int SheetIndex;

	/// <summary>The full name of the C# <c>TrinketEffect</c> subclass which implements the trinket behavior. This can safely be a mod class.</summary>
	public string TrinketEffectClass;

	/// <summary>Whether this trinket can be spawned randomly (e.g. in mine treasure chests).</summary>
	[ContentSerializer(Optional = true)]
	public bool DropsNaturally = true;

	/// <summary>Whether players can re-roll this trinket's stats using an anvil.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeReforged = true;

	/// <summary>Custom fields which may be used by the <see cref="F:StardewValley.GameData.TrinketData.TrinketEffectClass" /> or mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>A lookup of arbitrary <c>modData</c> values to attach to the trinket when it's constructed.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> ModData;
}
