using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Track which effect parameters need to be recomputed during the next OnApply.
/// </summary>
[Flags]
internal enum EffectDirtyFlags
{
	WorldViewProj = 1,
	World = 2,
	EyePosition = 4,
	MaterialColor = 8,
	Fog = 0x10,
	FogEnable = 0x20,
	AlphaTest = 0x40,
	ShaderIndex = 0x80,
	All = -1
}
