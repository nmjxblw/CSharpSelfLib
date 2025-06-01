using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a texture cube that can be used as a render target.
/// </summary>
public class RenderTargetCube : TextureCube, IRenderTarget
{
	private static Action<RenderTargetCube> DisposeAction = delegate(RenderTargetCube t)
	{
		t.GraphicsDevice.PlatformDeleteRenderTarget(t);
	};

	/// <summary>
	/// Gets the depth-stencil buffer format of this render target.
	/// </summary>
	/// <value>The format of the depth-stencil buffer.</value>
	public DepthFormat DepthStencilFormat { get; private set; }

	/// <summary>
	/// Gets the number of multisample locations.
	/// </summary>
	/// <value>The number of multisample locations.</value>
	public int MultiSampleCount { get; private set; }

	/// <summary>
	/// Gets the usage mode of this render target.
	/// </summary>
	/// <value>The usage mode of the render target.</value>
	public RenderTargetUsage RenderTargetUsage { get; private set; }

	/// <inheritdoc />
	int IRenderTarget.Width => base.size;

	/// <inheritdoc />
	int IRenderTarget.Height => base.size;

	public bool IsContentLost => false;

	int IRenderTarget.GLTexture => base.glTexture;

	TextureTarget IRenderTarget.GLTarget => base.glTarget;

	int IRenderTarget.GLColorBuffer { get; set; }

	int IRenderTarget.GLDepthBuffer { get; set; }

	int IRenderTarget.GLStencilBuffer { get; set; }

	public event EventHandler<EventArgs> ContentLost;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.RenderTargetCube" /> class.
	/// </summary>
	/// <param name="graphicsDevice">The graphics device.</param>
	/// <param name="size">The width and height of a texture cube face in pixels.</param>
	/// <param name="mipMap"><see langword="true" /> to generate a full mipmap chain; otherwise <see langword="false" />.</param>
	/// <param name="preferredFormat">The preferred format of the surface.</param>
	/// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
	public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
		: this(graphicsDevice, size, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Graphics.RenderTargetCube" /> class.
	/// </summary>
	/// <param name="graphicsDevice">The graphics device.</param>
	/// <param name="size">The width and height of a texture cube face in pixels.</param>
	/// <param name="mipMap"><see langword="true" /> to generate a full mipmap chain; otherwise <see langword="false" />.</param>
	/// <param name="preferredFormat">The preferred format of the surface.</param>
	/// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
	/// <param name="preferredMultiSampleCount">The preferred number of multisample locations.</param>
	/// <param name="usage">The usage mode of the render target.</param>
	public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
		: base(graphicsDevice, size, mipMap, RenderTargetCube.QuerySelectedFormat(graphicsDevice, preferredFormat), renderTarget: true)
	{
		this.DepthStencilFormat = preferredDepthFormat;
		this.MultiSampleCount = preferredMultiSampleCount;
		this.RenderTargetUsage = usage;
		this.PlatformConstruct(graphicsDevice, mipMap, preferredDepthFormat, preferredMultiSampleCount, usage);
	}

	protected static SurfaceFormat QuerySelectedFormat(GraphicsDevice graphicsDevice, SurfaceFormat preferredFormat)
	{
		SurfaceFormat selectedFormat = preferredFormat;
		graphicsDevice?.Adapter.QueryRenderTargetFormat(graphicsDevice.GraphicsProfile, preferredFormat, DepthFormat.None, 0, out selectedFormat, out var _, out var _);
		return selectedFormat;
	}

	TextureTarget IRenderTarget.GetFramebufferTarget(RenderTargetBinding renderTargetBinding)
	{
		return (TextureTarget)(34069 + renderTargetBinding.ArraySlice);
	}

	private void PlatformConstruct(GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
	{
		Threading.BlockOnUIThread(delegate
		{
			graphicsDevice.PlatformCreateRenderTarget(this, base.size, base.size, mipMap, base.Format, preferredDepthFormat, preferredMultiSampleCount, usage);
		});
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && base.GraphicsDevice != null)
		{
			Threading.BlockOnUIThread(RenderTargetCube.DisposeAction, this);
		}
		base.Dispose(disposing);
	}
}
