using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The metadata for a mannequin which can be placed in the world and used to store and display clothing.</summary>
public class MannequinData
{
	/// <summary>A tokenizable string for the item's translated display name.</summary>
	public string DisplayName;

	/// <summary>A tokenizable string for the item's translated description.</summary>
	public string Description;

	/// <summary>The asset name for the texture containing the item sprite, or <c>null</c> for <c>TileSheets/Mannequins</c>.</summary>
	public string Texture;

	/// <summary>The asset name for the texture containing the placed world sprite.</summary>
	public string FarmerTexture;

	/// <summary>The sprite's index in the <see cref="F:StardewValley.GameData.MannequinData.Texture" /> spritesheet.</summary>
	public int SheetIndex;

	/// <summary>For clothing with gender variants, whether to display the male (true) or female (false) variant.</summary>
	public bool DisplaysClothingAsMale = true;

	/// <summary>Whether to enable rare Easter egg 'cursed' behavior.</summary>
	[ContentSerializer(Optional = true)]
	public bool Cursed;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
