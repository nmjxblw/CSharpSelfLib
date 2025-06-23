using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.MakeoverOutfits;

/// <summary>An outfit that can be selected at the Desert Festival makeover booth.</summary>
public class MakeoverOutfit
{
	/// <summary>A unique string ID for this entry within the outfit list.</summary>
	public string Id;

	/// <summary>The hat, shirt, and pants that makes up the outfit. Each item is added to the appropriate equipment slot based on its type.</summary>
	/// <remarks>An item can be omitted to leave the player's current item unchanged (e.g. shirt + pants without a hat). If there are multiple items of the same type, the first matching one is applied.</remarks>
	public List<MakeoverItem> OutfitParts;

	/// <summary>The player gender for which the outfit applies, or <c>null</c> for any gender.</summary>
	[ContentSerializer(Optional = true)]
	public Gender? Gender;
}
