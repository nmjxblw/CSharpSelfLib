using System;

namespace Microsoft.Xna.Framework.Graphics;

public class PresentationParameters
{
	public const int DefaultPresentRate = 60;

	private DepthFormat depthStencilFormat;

	private SurfaceFormat backBufferFormat;

	private int backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;

	private int backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;

	private IntPtr deviceWindowHandle;

	private int multiSampleCount;

	private bool disposed;

	private bool isFullScreen;

	private bool hardwareModeSwitch = true;

	/// <summary>
	/// Get or set the format of the back buffer.
	/// </summary>
	public SurfaceFormat BackBufferFormat
	{
		get
		{
			return this.backBufferFormat;
		}
		set
		{
			this.backBufferFormat = value;
		}
	}

	/// <summary>
	/// Get or set the height of the back buffer.
	/// </summary>
	public int BackBufferHeight
	{
		get
		{
			return this.backBufferHeight;
		}
		set
		{
			this.backBufferHeight = value;
		}
	}

	/// <summary>
	/// Get or set the width of the back buffer.
	/// </summary>
	public int BackBufferWidth
	{
		get
		{
			return this.backBufferWidth;
		}
		set
		{
			this.backBufferWidth = value;
		}
	}

	/// <summary>
	/// Get the bounds of the back buffer.
	/// </summary>
	public Rectangle Bounds => new Rectangle(0, 0, this.backBufferWidth, this.backBufferHeight);

	/// <summary>
	/// Get or set the handle of the window that will present the back buffer.
	/// </summary>
	public IntPtr DeviceWindowHandle
	{
		get
		{
			return this.deviceWindowHandle;
		}
		set
		{
			this.deviceWindowHandle = value;
		}
	}

	/// <summary>
	/// Get or set the depth stencil format for the back buffer.
	/// </summary>
	public DepthFormat DepthStencilFormat
	{
		get
		{
			return this.depthStencilFormat;
		}
		set
		{
			this.depthStencilFormat = value;
		}
	}

	/// <summary>
	/// Get or set a value indicating if we are in full screen mode.
	/// </summary>
	public bool IsFullScreen
	{
		get
		{
			return this.isFullScreen;
		}
		set
		{
			this.isFullScreen = value;
		}
	}

	/// <summary>
	/// If <code>true</code> the <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> will do a mode switch
	/// when going to full screen mode. If <code>false</code> it will instead do a
	/// soft full screen by maximizing the window and making it borderless.
	/// </summary>
	public bool HardwareModeSwitch
	{
		get
		{
			return this.hardwareModeSwitch;
		}
		set
		{
			this.hardwareModeSwitch = value;
		}
	}

	/// <summary>
	/// Get or set the multisample count for the back buffer.
	/// </summary>
	public int MultiSampleCount
	{
		get
		{
			return this.multiSampleCount;
		}
		set
		{
			this.multiSampleCount = value;
		}
	}

	/// <summary>
	/// Get or set the presentation interval.
	/// </summary>
	public PresentInterval PresentationInterval { get; set; }

	/// <summary>
	/// Get or set the display orientation.
	/// </summary>
	public DisplayOrientation DisplayOrientation { get; set; }

	/// <summary>
	/// Get or set the RenderTargetUsage for the back buffer.
	/// Determines if the back buffer is cleared when it is set as the
	/// render target by the <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />.
	/// <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> target.
	/// </summary>
	public RenderTargetUsage RenderTargetUsage { get; set; }

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.Graphics.PresentationParameters" /> instance with default values for all properties.
	/// </summary>
	public PresentationParameters()
	{
		this.Clear();
	}

	/// <summary>
	/// Reset all properties to their default values.
	/// </summary>
	public void Clear()
	{
		this.backBufferFormat = SurfaceFormat.Color;
		this.backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
		this.backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
		this.deviceWindowHandle = IntPtr.Zero;
		this.depthStencilFormat = DepthFormat.None;
		this.multiSampleCount = 0;
		this.PresentationInterval = PresentInterval.Default;
		this.DisplayOrientation = DisplayOrientation.Default;
	}

	/// <summary>
	/// Create a copy of this <see cref="T:Microsoft.Xna.Framework.Graphics.PresentationParameters" /> instance.
	/// </summary>
	/// <returns></returns>
	public PresentationParameters Clone()
	{
		return new PresentationParameters
		{
			backBufferFormat = this.backBufferFormat,
			backBufferHeight = this.backBufferHeight,
			backBufferWidth = this.backBufferWidth,
			deviceWindowHandle = this.deviceWindowHandle,
			depthStencilFormat = this.depthStencilFormat,
			IsFullScreen = this.IsFullScreen,
			HardwareModeSwitch = this.HardwareModeSwitch,
			multiSampleCount = this.multiSampleCount,
			PresentationInterval = this.PresentationInterval,
			DisplayOrientation = this.DisplayOrientation,
			RenderTargetUsage = this.RenderTargetUsage
		};
	}
}
