using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a render target.
/// </summary>
internal interface IRenderTarget
{
	/// <summary>
	/// Gets the width of the render target in pixels
	/// </summary>
	/// <value>The width of the render target in pixels.</value>
	int Width { get; }

	/// <summary>
	/// Gets the height of the render target in pixels
	/// </summary>
	/// <value>The height of the render target in pixels.</value>
	int Height { get; }

	/// <summary>
	/// Gets the usage mode of the render target.
	/// </summary>
	/// <value>The usage mode of the render target.</value>
	RenderTargetUsage RenderTargetUsage { get; }

	int GLTexture { get; }

	TextureTarget GLTarget { get; }

	int GLColorBuffer { get; set; }

	int GLDepthBuffer { get; set; }

	int GLStencilBuffer { get; set; }

	int MultiSampleCount { get; }

	int LevelCount { get; }

	TextureTarget GetFramebufferTarget(RenderTargetBinding renderTargetBinding);
}
