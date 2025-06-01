using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Defines the buffers for clearing when calling <see cref="M:Microsoft.Xna.Framework.Graphics.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.ClearOptions,Microsoft.Xna.Framework.Color,System.Single,System.Int32)" /> operation.
/// </summary>
[Flags]
public enum ClearOptions
{
	/// <summary>
	/// Color buffer.
	/// </summary>
	Target = 1,
	/// <summary>
	/// Depth buffer.
	/// </summary>
	DepthBuffer = 2,
	/// <summary>
	/// Stencil buffer.
	/// </summary>
	Stencil = 4
}
