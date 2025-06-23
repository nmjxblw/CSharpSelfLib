using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.MakeoverOutfits;

/// <summary>A hat, shirt, or pants that should be equipped on the player as part of a <see cref="T:StardewValley.GameData.MakeoverOutfits.MakeoverOutfit" />.</summary>
public class MakeoverItem
{
	/// <summary>A unique ID for this entry within the list.</summary>
	public string Id;

	/// <summary>The qualified item ID for the hat, shirt, or pants to equip.</summary>
	public string ItemId;

	/// <summary>A tint color to apply to the item. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string Color;

	/// <summary>The player gender for which the outfit part applies, or <c>null</c> for any gender.</summary>
	[ContentSerializer(Optional = true)]
	public Gender? Gender;

	/// <summary>Get whether this item applies to the given player gender.</summary>
	/// <param name="gender">The player gender to check.</param>
	public bool MatchesGender(Gender gender)
	{
		Gender? gender2 = Gender;
		if (gender2.HasValue)
		{
			return Gender == gender;
		}
		return true;
	}
}
