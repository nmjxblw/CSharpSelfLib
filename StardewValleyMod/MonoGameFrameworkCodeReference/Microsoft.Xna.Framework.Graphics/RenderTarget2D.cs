using System;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class RenderTarget2D : Texture2D, IRenderTarget
{
	private static Action<RenderTarget2D> DisposeAction = delegate(RenderTarget2D t)
	{
		t.GraphicsDevice.PlatformDeleteRenderTarget(t);
	};

	public DepthFormat DepthStencilFormat { get; private set; }

	public int MultiSampleCount { get; private set; }

	public RenderTargetUsage RenderTargetUsage { get; private set; }

	public bool IsContentLost => false;

	int IRenderTarget.GLTexture => base.glTexture;

	TextureTarget IRenderTarget.GLTarget => base.glTarget;

	int IRenderTarget.GLColorBuffer { get; set; }

	int IRenderTarget.GLDepthBuffer { get; set; }

	int IRenderTarget.GLStencilBuffer { get; set; }

	public event EventHandler<EventArgs> ContentLost;

	private bool SuppressEventHandlerWarningsUntilEventsAreProperlyImplemented()
	{
		return this.ContentLost != null;
	}

	public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize)
		: base(graphicsDevice, width, height, mipMap, RenderTarget2D.QuerySelectedFormat(graphicsDevice, preferredFormat), SurfaceType.RenderTarget, shared, arraySize)
	{
		this.DepthStencilFormat = preferredDepthFormat;
		this.MultiSampleCount = graphicsDevice.GetClampedMultisampleCount(preferredMultiSampleCount);
		this.RenderTargetUsage = usage;
		this.PlatformConstruct(graphicsDevice, width, height, mipMap, preferredDepthFormat, preferredMultiSampleCount, usage, shared);
	}

	protected static SurfaceFormat QuerySelectedFormat(GraphicsDevice graphicsDevice, SurfaceFormat preferredFormat)
	{
		SurfaceFormat selectedFormat = preferredFormat;
		graphicsDevice?.Adapter.QueryRenderTargetFormat(graphicsDevice.GraphicsProfile, preferredFormat, DepthFormat.None, 0, out selectedFormat, out var _, out var _);
		return selectedFormat;
	}

	public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
		: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, 1)
	{
	}

	public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
		: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared: false)
	{
	}

	public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
		: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents)
	{
	}

	public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
		: this(graphicsDevice, width, height, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents)
	{
	}

	/// <summary>
	/// Allows child class to specify the surface type, eg: a swap chain.
	/// </summary>        
	protected RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format, DepthFormat depthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, SurfaceType surfaceType)
		: base(graphicsDevice, width, height, mipMap, format, surfaceType)
	{
		this.DepthStencilFormat = depthFormat;
		this.MultiSampleCount = preferredMultiSampleCount;
		this.RenderTargetUsage = usage;
	}

	protected internal override void GraphicsDeviceResetting()
	{
		this.PlatformGraphicsDeviceResetting();
		base.GraphicsDeviceResetting();
	}

	TextureTarget IRenderTarget.GetFramebufferTarget(RenderTargetBinding renderTargetBinding)
	{
		return base.glTarget;
	}

	private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
	{
		Threading.BlockOnUIThread(delegate
		{
			graphicsDevice.PlatformCreateRenderTarget(this, width, height, mipMap, base.Format, preferredDepthFormat, preferredMultiSampleCount, usage);
		});
	}

	private void PlatformGraphicsDeviceResetting()
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && base.GraphicsDevice != null)
		{
			Threading.BlockOnUIThread(RenderTarget2D.DisposeAction, this);
		}
		base.Dispose(disposing);
	}
}
