namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Defines a culling mode for faces in rasterization process.
/// </summary>
public enum CullMode
{
	/// <summary>
	/// Do not cull faces.
	/// </summary>
	None,
	/// <summary>
	/// Cull faces with clockwise order.
	/// </summary>
	CullClockwiseFace,
	/// <summary>
	/// Cull faces with counter clockwise order.
	/// </summary>
	CullCounterClockwiseFace
}
