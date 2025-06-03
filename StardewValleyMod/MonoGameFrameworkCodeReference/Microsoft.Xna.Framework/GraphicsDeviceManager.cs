using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Used to initialize and control the presentation of the graphics device.
/// </summary>
public class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable, IGraphicsDeviceManager
{
	private readonly Game _game;

	private GraphicsDevice _graphicsDevice;

	private bool _initialized;

	private int _preferredBackBufferHeight;

	private int _preferredBackBufferWidth;

	private SurfaceFormat _preferredBackBufferFormat;

	private DepthFormat _preferredDepthStencilFormat;

	private bool _preferMultiSampling;

	private DisplayOrientation _supportedOrientations;

	private bool _synchronizedWithVerticalRetrace = true;

	private bool _drawBegun;

	private bool _disposed;

	private bool _hardwareModeSwitch = true;

	private bool _preferHalfPixelOffset;

	private bool _wantFullScreen;

	private GraphicsProfile _graphicsProfile;

	private bool _shouldApplyChanges;

	/// <summary>
	/// The default back buffer width.
	/// </summary>
	public static readonly int DefaultBackBufferWidth = 800;

	/// <summary>
	/// The default back buffer height.
	/// </summary>
	public static readonly int DefaultBackBufferHeight = 480;

	/// <summary>
	/// The profile which determines the graphics feature level.
	/// </summary>
	public GraphicsProfile GraphicsProfile
	{
		get
		{
			return this._graphicsProfile;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._graphicsProfile = value;
		}
	}

	/// <summary>
	/// Returns the graphics device for this manager.
	/// </summary>
	public GraphicsDevice GraphicsDevice => this._graphicsDevice;

	/// <summary>
	/// Indicates the desire to switch into fullscreen mode.
	/// </summary>
	/// <remarks>
	/// When called at startup this will automatically set fullscreen mode during initialization.  If
	/// set after startup you must call ApplyChanges() for the fullscreen mode to be changed.
	/// Note that for some platforms that do not support windowed modes this property has no affect.
	/// </remarks>
	public bool IsFullScreen
	{
		get
		{
			return this._wantFullScreen;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._wantFullScreen = value;
		}
	}

	/// <summary>
	/// Gets or sets the boolean which defines how window switches from windowed to fullscreen state.
	/// "Hard" mode(true) is slow to switch, but more effecient for performance, while "soft" mode(false) is vice versa.
	/// The default value is <c>true</c>.
	/// </summary>
	public bool HardwareModeSwitch
	{
		get
		{
			return this._hardwareModeSwitch;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._hardwareModeSwitch = value;
		}
	}

	/// <summary>
	/// Indicates if DX9 style pixel addressing or current standard
	/// pixel addressing should be used. This flag is set to
	/// <c>false</c> by default. It should be set to <c>true</c>
	/// for XNA compatibility. It is recommended to leave this flag
	/// set to <c>false</c> for projects that are not ported from
	/// XNA. This value is passed to <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.UseHalfPixelOffset" />.
	/// </summary>
	/// <remarks>
	/// XNA uses DirectX9 for its graphics. DirectX9 interprets UV
	/// coordinates differently from other graphics API's. This is
	/// typically referred to as the half-pixel offset. MonoGame
	/// replicates XNA behavior if this flag is set to <c>true</c>.
	/// </remarks>
	public bool PreferHalfPixelOffset
	{
		get
		{
			return this._preferHalfPixelOffset;
		}
		set
		{
			if (this.GraphicsDevice != null)
			{
				throw new InvalidOperationException("Setting PreferHalfPixelOffset is not allowed after the creation of GraphicsDevice.");
			}
			this._preferHalfPixelOffset = value;
		}
	}

	/// <summary>
	/// Indicates the desire for a multisampled back buffer.
	/// </summary>
	/// <remarks>
	/// When called at startup this will automatically set the MSAA mode during initialization.  If
	/// set after startup you must call ApplyChanges() for the MSAA mode to be changed.
	/// </remarks>
	public bool PreferMultiSampling
	{
		get
		{
			return this._preferMultiSampling;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._preferMultiSampling = value;
		}
	}

	/// <summary>
	/// Indicates the desired back buffer color format.
	/// </summary>
	/// <remarks>
	/// When called at startup this will automatically set the format during initialization.  If
	/// set after startup you must call ApplyChanges() for the format to be changed.
	/// </remarks>
	public SurfaceFormat PreferredBackBufferFormat
	{
		get
		{
			return this._preferredBackBufferFormat;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._preferredBackBufferFormat = value;
		}
	}

	/// <summary>
	/// Indicates the desired back buffer height in pixels.
	/// </summary>
	/// <remarks>
	/// When called at startup this will automatically set the height during initialization.  If
	/// set after startup you must call ApplyChanges() for the height to be changed.
	/// </remarks>
	public int PreferredBackBufferHeight
	{
		get
		{
			return this._preferredBackBufferHeight;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._preferredBackBufferHeight = value;
		}
	}

	/// <summary>
	/// Indicates the desired back buffer width in pixels.
	/// </summary>
	/// <remarks>
	/// When called at startup this will automatically set the width during initialization.  If
	/// set after startup you must call ApplyChanges() for the width to be changed.
	/// </remarks>
	public int PreferredBackBufferWidth
	{
		get
		{
			return this._preferredBackBufferWidth;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._preferredBackBufferWidth = value;
		}
	}

	/// <summary>
	/// Indicates the desired depth-stencil buffer format.
	/// </summary>
	/// <remarks>
	/// The depth-stencil buffer format defines the scene depth precision and stencil bits available for effects during rendering.
	/// When called at startup this will automatically set the format during initialization.  If
	/// set after startup you must call ApplyChanges() for the format to be changed.
	/// </remarks>
	public DepthFormat PreferredDepthStencilFormat
	{
		get
		{
			return this._preferredDepthStencilFormat;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._preferredDepthStencilFormat = value;
		}
	}

	/// <summary>
	/// Indicates the desire for vsync when presenting the back buffer.
	/// </summary>
	/// <remarks>
	/// Vsync limits the frame rate of the game to the monitor referesh rate to prevent screen tearing.
	/// When called at startup this will automatically set the vsync mode during initialization.  If
	/// set after startup you must call ApplyChanges() for the vsync mode to be changed.
	/// </remarks>
	public bool SynchronizeWithVerticalRetrace
	{
		get
		{
			return this._synchronizedWithVerticalRetrace;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._synchronizedWithVerticalRetrace = value;
		}
	}

	/// <summary>
	/// Indicates the desired allowable display orientations when the device is rotated.
	/// </summary>
	/// <remarks>
	/// This property only applies to mobile platforms with automatic display rotation.
	/// When called at startup this will automatically apply the supported orientations during initialization.  If
	/// set after startup you must call ApplyChanges() for the supported orientations to be changed.
	/// </remarks>
	public DisplayOrientation SupportedOrientations
	{
		get
		{
			return this._supportedOrientations;
		}
		set
		{
			this._shouldApplyChanges = true;
			this._supportedOrientations = value;
		}
	}

	/// <inheritdoc />
	public event EventHandler<EventArgs> DeviceCreated;

	/// <inheritdoc />
	public event EventHandler<EventArgs> DeviceDisposing;

	/// <inheritdoc />
	public event EventHandler<EventArgs> DeviceResetting;

	/// <inheritdoc />
	public event EventHandler<EventArgs> DeviceReset;

	/// <summary>
	/// Raised by <see cref="M:Microsoft.Xna.Framework.GraphicsDeviceManager.CreateDevice" /> or <see cref="M:Microsoft.Xna.Framework.GraphicsDeviceManager.ApplyChanges" />. Allows users
	/// to override the <see cref="T:Microsoft.Xna.Framework.Graphics.PresentationParameters" /> to pass to the
	/// <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />.
	/// </summary>
	public event EventHandler<PreparingDeviceSettingsEventArgs> PreparingDeviceSettings;

	/// <summary>
	/// Raised when this <see cref="T:Microsoft.Xna.Framework.GraphicsDeviceManager" /> is disposed.
	/// </summary>
	public event EventHandler<EventArgs> Disposed;

	/// <summary>
	/// Associates this graphics device manager to a game instances.
	/// </summary>
	/// <param name="game">The game instance to attach.</param>
	public GraphicsDeviceManager(Game game)
	{
		if (game == null)
		{
			throw new ArgumentNullException("game", "Game cannot be null.");
		}
		this._game = game;
		this._supportedOrientations = DisplayOrientation.Default;
		this._preferredBackBufferFormat = SurfaceFormat.Color;
		this._preferredDepthStencilFormat = DepthFormat.Depth24;
		this._synchronizedWithVerticalRetrace = true;
		Rectangle clientBounds = this._game.Window.ClientBounds;
		if (clientBounds.Width >= clientBounds.Height)
		{
			this._preferredBackBufferWidth = clientBounds.Width;
			this._preferredBackBufferHeight = clientBounds.Height;
		}
		else
		{
			this._preferredBackBufferWidth = clientBounds.Height;
			this._preferredBackBufferHeight = clientBounds.Width;
		}
		this._wantFullScreen = false;
		this.GraphicsProfile = GraphicsProfile.Reach;
		if (this._game.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
		{
			throw new ArgumentException("A graphics device manager is already registered.  The graphics device manager cannot be changed once it is set.");
		}
		this._game.graphicsDeviceManager = this;
		this._game.Services.AddService(typeof(IGraphicsDeviceManager), this);
		this._game.Services.AddService(typeof(IGraphicsDeviceService), this);
	}

	~GraphicsDeviceManager()
	{
		this.Dispose(disposing: false);
	}

	private void CreateDevice()
	{
		if (this._graphicsDevice != null)
		{
			return;
		}
		try
		{
			if (!this._initialized)
			{
				this.Initialize();
			}
			GraphicsDeviceInformation gdi = this.DoPreparingDeviceSettings();
			this.CreateDevice(gdi);
		}
		catch (NoSuitableGraphicsDeviceException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new NoSuitableGraphicsDeviceException("Failed to create graphics device!", inner);
		}
	}

	private void CreateDevice(GraphicsDeviceInformation gdi)
	{
		if (this._graphicsDevice == null)
		{
			this._graphicsDevice = new GraphicsDevice(gdi.Adapter, gdi.GraphicsProfile, this.PreferHalfPixelOffset, gdi.PresentationParameters);
			this._shouldApplyChanges = false;
			this.GraphicsDevice.DeviceReset += delegate(object? sender, EventArgs args)
			{
				this.OnDeviceReset(args);
			};
			this.GraphicsDevice.DeviceResetting += delegate(object? sender, EventArgs args)
			{
				this.OnDeviceResetting(args);
			};
			this._graphicsDevice.DeviceReset += UpdateTouchPanel;
			this._graphicsDevice.PresentationChanged += OnPresentationChanged;
			this.OnDeviceCreated(EventArgs.Empty);
		}
	}

	void IGraphicsDeviceManager.CreateDevice()
	{
		this.CreateDevice();
	}

	public bool BeginDraw()
	{
		if (this._graphicsDevice == null)
		{
			return false;
		}
		this._drawBegun = true;
		return true;
	}

	public void EndDraw()
	{
		if (this._graphicsDevice != null && this._drawBegun)
		{
			this._drawBegun = false;
			this._graphicsDevice.Present();
		}
	}

	/// <summary>
	/// Called when a <see cref="P:Microsoft.Xna.Framework.GraphicsDeviceManager.GraphicsDevice" /> is created. Raises the <see cref="E:Microsoft.Xna.Framework.GraphicsDeviceManager.DeviceCreated" /> event.
	/// </summary>
	/// <param name="e"></param>
	protected void OnDeviceCreated(EventArgs e)
	{
		EventHelpers.Raise(this, this.DeviceCreated, e);
	}

	/// <summary>
	/// Called when a <see cref="P:Microsoft.Xna.Framework.GraphicsDeviceManager.GraphicsDevice" /> is disposed. Raises the <see cref="E:Microsoft.Xna.Framework.GraphicsDeviceManager.DeviceDisposing" /> event.
	/// </summary>
	/// <param name="e"></param>
	protected void OnDeviceDisposing(EventArgs e)
	{
		EventHelpers.Raise(this, this.DeviceDisposing, e);
	}

	/// <summary>
	/// Called before a <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> is reset.
	/// Raises the <see cref="E:Microsoft.Xna.Framework.GraphicsDeviceManager.DeviceResetting" /> event.
	/// </summary>
	/// <param name="e"></param>
	protected void OnDeviceResetting(EventArgs e)
	{
		EventHelpers.Raise(this, this.DeviceResetting, e);
	}

	/// <summary>
	/// Called after a <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> is reset.
	/// Raises the <see cref="E:Microsoft.Xna.Framework.GraphicsDeviceManager.DeviceReset" /> event.
	/// </summary>
	/// <param name="e"></param>
	protected void OnDeviceReset(EventArgs e)
	{
		EventHelpers.Raise(this, this.DeviceReset, e);
	}

	/// <summary>
	/// This populates a GraphicsDeviceInformation instance and invokes PreparingDeviceSettings to
	/// allow users to change the settings. Then returns that GraphicsDeviceInformation.
	/// Throws NullReferenceException if users set GraphicsDeviceInformation.PresentationParameters to null.
	/// </summary>
	private GraphicsDeviceInformation DoPreparingDeviceSettings()
	{
		GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
		this.PrepareGraphicsDeviceInformation(gdi);
		EventHandler<PreparingDeviceSettingsEventArgs> preparingDeviceSettingsHandler = this.PreparingDeviceSettings;
		if (preparingDeviceSettingsHandler != null)
		{
			PreparingDeviceSettingsEventArgs args = new PreparingDeviceSettingsEventArgs(gdi);
			preparingDeviceSettingsHandler(this, args);
			if (gdi.PresentationParameters == null || gdi.Adapter == null)
			{
				throw new NullReferenceException("Members should not be set to null in PreparingDeviceSettingsEventArgs");
			}
		}
		return gdi;
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!this._disposed)
		{
			if (disposing && this._graphicsDevice != null)
			{
				this._graphicsDevice.Dispose();
				this._graphicsDevice = null;
			}
			this._disposed = true;
			EventHelpers.Raise(this, this.Disposed, EventArgs.Empty);
		}
	}

	private void PreparePresentationParameters(PresentationParameters presentationParameters)
	{
		presentationParameters.BackBufferFormat = this._preferredBackBufferFormat;
		presentationParameters.BackBufferWidth = this._preferredBackBufferWidth;
		presentationParameters.BackBufferHeight = this._preferredBackBufferHeight;
		presentationParameters.DepthStencilFormat = this._preferredDepthStencilFormat;
		presentationParameters.IsFullScreen = this._wantFullScreen;
		presentationParameters.HardwareModeSwitch = this._hardwareModeSwitch;
		presentationParameters.PresentationInterval = (this._synchronizedWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate);
		presentationParameters.DisplayOrientation = this._game.Window.CurrentOrientation;
		presentationParameters.DeviceWindowHandle = this._game.Window.Handle;
		if (this._preferMultiSampling)
		{
			presentationParameters.MultiSampleCount = ((this.GraphicsDevice != null) ? this.GraphicsDevice.GraphicsCapabilities.MaxMultiSampleCount : 32);
		}
		else
		{
			presentationParameters.MultiSampleCount = 0;
		}
	}

	private void PrepareGraphicsDeviceInformation(GraphicsDeviceInformation gdi)
	{
		gdi.Adapter = GraphicsAdapter.DefaultAdapter;
		gdi.GraphicsProfile = this.GraphicsProfile;
		PresentationParameters pp = new PresentationParameters();
		this.PreparePresentationParameters(pp);
		gdi.PresentationParameters = pp;
	}

	/// <summary>
	/// Applies any pending property changes to the graphics device.
	/// </summary>
	public void ApplyChanges()
	{
		if (this._graphicsDevice == null)
		{
			this.CreateDevice();
		}
		if (this._shouldApplyChanges)
		{
			this._shouldApplyChanges = false;
			this._game.Window.SetSupportedOrientations(this._supportedOrientations);
			GraphicsDeviceInformation gdi = this.DoPreparingDeviceSettings();
			if (gdi.GraphicsProfile != this.GraphicsDevice.GraphicsProfile)
			{
				this.DisposeGraphicsDevice();
				this.CreateDevice(gdi);
			}
			else
			{
				this.GraphicsDevice.Reset(gdi.PresentationParameters);
			}
		}
	}

	private void DisposeGraphicsDevice()
	{
		this._graphicsDevice.Dispose();
		EventHelpers.Raise(this, this.DeviceDisposing, EventArgs.Empty);
		this._graphicsDevice = null;
	}

	private void PlatformInitialize(PresentationParameters presentationParameters)
	{
		ColorFormat surfaceFormat = this._game.graphicsDeviceManager.PreferredBackBufferFormat.GetColorFormat();
		DepthFormat depthStencilFormat = this._game.graphicsDeviceManager.PreferredDepthStencilFormat;
		Sdl.GL.SetAttribute(Sdl.GL.Attribute.RedSize, surfaceFormat.R);
		Sdl.GL.SetAttribute(Sdl.GL.Attribute.GreenSize, surfaceFormat.G);
		Sdl.GL.SetAttribute(Sdl.GL.Attribute.BlueSize, surfaceFormat.B);
		Sdl.GL.SetAttribute(Sdl.GL.Attribute.AlphaSize, surfaceFormat.A);
		switch (depthStencilFormat)
		{
		case DepthFormat.None:
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 0);
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
			break;
		case DepthFormat.Depth16:
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 16);
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
			break;
		case DepthFormat.Depth24:
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
			break;
		case DepthFormat.Depth24Stencil8:
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
			Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 8);
			break;
		}
		Sdl.GL.SetAttribute(Sdl.GL.Attribute.DoubleBuffer, 1);
		Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
		Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 1);
		((SdlGameWindow)SdlGameWindow.Instance).CreateWindow();
	}

	private void Initialize()
	{
		this._game.Window.SetSupportedOrientations(this._supportedOrientations);
		PresentationParameters presentationParameters = new PresentationParameters();
		this.PreparePresentationParameters(presentationParameters);
		this.PlatformInitialize(presentationParameters);
		this._initialized = true;
	}

	private void UpdateTouchPanel(object sender, EventArgs eventArgs)
	{
		TouchPanel.DisplayWidth = this._graphicsDevice.PresentationParameters.BackBufferWidth;
		TouchPanel.DisplayHeight = this._graphicsDevice.PresentationParameters.BackBufferHeight;
		TouchPanel.DisplayOrientation = this._graphicsDevice.PresentationParameters.DisplayOrientation;
	}

	/// <summary>
	/// Toggles between windowed and fullscreen modes.
	/// </summary>
	/// <remarks>
	/// Note that on platforms that do not support windowed modes this has no affect.
	/// </remarks>
	public void ToggleFullScreen()
	{
		this.IsFullScreen = !this.IsFullScreen;
		this.ApplyChanges();
	}

	private void OnPresentationChanged(object sender, PresentationEventArgs args)
	{
		this._game.Platform.OnPresentationChanged(args.PresentationParameters);
	}
}
