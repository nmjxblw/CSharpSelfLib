using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FishPonds;

/// <summary>As part of <see cref="T:StardewValley.GameData.FishPonds.FishPondData" />, a color to apply to the water if its fields match.</summary>
public class FishPondWaterColor
{
	public string Id;

	/// <summary>A tint color to apply to the water. This can be <c>CopyFromInput</c> (to use the input item's color), a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
	public string Color;

	/// <summary>The minimum population before this color applies.</summary>
	[ContentSerializer(Optional = true)]
	public int MinPopulation = 1;

	/// <summary>The minimum population gate that was unlocked, or 0 for any value.</summary>
	[ContentSerializer(Optional = true)]
	public int MinUnlockedPopulationGate;

	/// <summary>A game state query which indicates whether this color should be applied. Defaults to always added.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;
}
