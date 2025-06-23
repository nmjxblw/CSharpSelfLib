using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, a light effect shown around the machine.</summary>
public class MachineLight
{
	/// <summary>The radius of the light emitted.</summary>
	[ContentSerializer(Optional = true)]
	public float Radius = 1f;

	/// <summary>A tint color to apply to the light. This can be a MonoGame property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>). Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string Color;
}
