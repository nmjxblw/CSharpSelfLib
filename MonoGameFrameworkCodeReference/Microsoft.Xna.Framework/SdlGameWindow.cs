using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework;

internal class SdlGameWindow : GameWindow, IDisposable
{
	public static GameWindow Instance;

	public uint? Id;

	public bool IsFullScreen;

	internal readonly Game _game;

	private IntPtr _handle;

	private IntPtr _icon;

	private bool _disposed;

	private bool _resizable;

	private bool _borderless;

	private bool _willBeFullScreen;

	private bool _mouseVisible;

	private bool _hardwareSwitch;

	private string _screenDeviceName;

	private int _width;

	private int _height;

	private bool _wasMoved;

	private bool _supressMoved;

	protected bool _applyingFullScreenMove;

	public override bool AllowUserResizing
	{
		get
		{
			if (!this.IsBorderless)
			{
				return this._resizable;
			}
			return false;
		}
		set
		{
			Sdl.Version nonResizeableVersion = new Sdl.Version
			{
				Major = 2,
				Minor = 0,
				Patch = 4
			};
			if (Sdl.version > nonResizeableVersion)
			{
				Sdl.Window.SetResizable(this._handle, value);
				this._resizable = value;
				return;
			}
			Sdl.Version version = nonResizeableVersion;
			throw new Exception("SDL " + version.ToString() + " does not support changing resizable parameter of the window after it's already been created, please use a newer version of it.");
		}
	}

	public override Rectangle ClientBounds
	{
		get
		{
			int x = 0;
			int y = 0;
			Sdl.Window.GetPosition(this.Handle, out x, out y);
			return new Rectangle(x, y, this._width, this._height);
		}
	}

	public override Point Position
	{
		get
		{
			int x = 0;
			int y = 0;
			if (!this.IsFullScreen)
			{
				Sdl.Window.GetPosition(this.Handle, out x, out y);
			}
			return new Point(x, y);
		}
		set
		{
			Sdl.Window.SetPosition(this.Handle, value.X, value.Y);
			this._wasMoved = true;
		}
	}

	public override DisplayOrientation CurrentOrientation => DisplayOrientation.Default;

	public override IntPtr Handle => this._handle;

	public override string ScreenDeviceName => this._screenDeviceName;

	public override bool IsBorderless
	{
		get
		{
			return this._borderless;
		}
		set
		{
			Sdl.Window.SetBordered(this._handle, (!value) ? 1 : 0);
			this._borderless = value;
		}
	}

	public SdlGameWindow(Game game)
	{
		this._game = game;
		this._screenDeviceName = "";
		SdlGameWindow.Instance = this;
		this._width = GraphicsDeviceManager.DefaultBackBufferWidth;
		this._height = GraphicsDeviceManager.DefaultBackBufferHeight;
		Sdl.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
		Sdl.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");
		if (Assembly.GetEntryAssembly() != null)
		{
			using Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream(Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace + ".Icon.bmp") ?? Assembly.GetEntryAssembly().GetManifestResourceStream("Icon.bmp") ?? Assembly.GetExecutingAssembly().GetManifestResourceStream("MonoGame.bmp");
			if (stream != null)
			{
				using BinaryReader br = new BinaryReader(stream);
				try
				{
					IntPtr src = Sdl.RwFromMem(br.ReadBytes((int)stream.Length), (int)stream.Length);
					this._icon = Sdl.LoadBMP_RW(src, 1);
				}
				catch
				{
				}
			}
		}
		this._handle = Sdl.Window.Create("", 0, 0, GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight, 4105);
	}

	internal void CreateWindow()
	{
		int initflags = 1546;
		if (this._handle != IntPtr.Zero)
		{
			Sdl.Window.Destroy(this._handle);
		}
		int winx = 805240832;
		int winy = 805240832;
		if (CurrentPlatform.OS == OS.Linux)
		{
			winx |= SdlGameWindow.GetMouseDisplay();
			winy |= SdlGameWindow.GetMouseDisplay();
		}
		this._handle = Sdl.Window.Create(AssemblyHelper.GetDefaultWindowTitle(), winx, winy, this._width, this._height, initflags);
		this.Id = Sdl.Window.GetWindowId(this._handle);
		if (this._icon != IntPtr.Zero)
		{
			Sdl.Window.SetIcon(this._handle, this._icon);
		}
		Sdl.Window.SetBordered(this._handle, (!this._borderless) ? 1 : 0);
		Sdl.Window.SetResizable(this._handle, this._resizable);
		this.SetCursorVisible(this._mouseVisible);
	}

	~SdlGameWindow()
	{
		this.Dispose(disposing: false);
	}

	private static int GetMouseDisplay()
	{
		Sdl.Rectangle rect = default(Sdl.Rectangle);
		Sdl.Mouse.GetGlobalState(out var x, out var y);
		int displayCount = Sdl.Display.GetNumVideoDisplays();
		for (int i = 0; i < displayCount; i++)
		{
			Sdl.Display.GetBounds(i, out rect);
			if (x >= rect.X && x < rect.X + rect.Width && y >= rect.Y && y < rect.Y + rect.Height)
			{
				return i;
			}
		}
		return 0;
	}

	public void SetCursorVisible(bool visible)
	{
		this._mouseVisible = visible;
		Sdl.Mouse.ShowCursor(visible ? 1 : 0);
	}

	public override void BeginScreenDeviceChange(bool willBeFullScreen)
	{
		this._willBeFullScreen = willBeFullScreen;
	}

	public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
	{
		this._screenDeviceName = screenDeviceName;
		Rectangle prevBounds = this.ClientBounds;
		int displayIndex = Sdl.Window.GetDisplayIndex(this.Handle);
		Sdl.Display.GetBounds(displayIndex, out var displayRect);
		if (this._willBeFullScreen != this.IsFullScreen || this._hardwareSwitch != this._game.graphicsDeviceManager.HardwareModeSwitch)
		{
			int fullscreenFlag = (this._game.graphicsDeviceManager.HardwareModeSwitch ? 1 : 4097);
			Sdl.Window.SetFullscreen(this.Handle, this._willBeFullScreen ? fullscreenFlag : 0);
			this._hardwareSwitch = this._game.graphicsDeviceManager.HardwareModeSwitch;
		}
		if (CurrentPlatform.OS == OS.Windows)
		{
			Sdl.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", (this._willBeFullScreen && this._hardwareSwitch) ? "1" : "0");
		}
		if (!this._willBeFullScreen || this._game.graphicsDeviceManager.HardwareModeSwitch)
		{
			Sdl.Window.SetSize(this.Handle, clientWidth, clientHeight);
			this._width = clientWidth;
			this._height = clientHeight;
		}
		else
		{
			this._width = displayRect.Width;
			this._height = displayRect.Height;
		}
		int minx = 0;
		int miny = 0;
		Sdl.Window.GetBorderSize(this._handle, out miny, out minx, out var ignore, out ignore);
		int centerX = Math.Max(prevBounds.X + (prevBounds.Width - clientWidth) / 2, minx + displayRect.X);
		int centerY = Math.Max(prevBounds.Y + (prevBounds.Height - clientHeight) / 2, miny + displayRect.Y);
		if (this.IsFullScreen && !this._willBeFullScreen)
		{
			Sdl.Display.GetBounds(displayIndex, out displayRect);
			centerX = displayRect.X + displayRect.Width / 2 - clientWidth / 2;
			centerY = displayRect.Y + displayRect.Height / 2 - clientHeight / 2;
		}
		if (Sdl.version > new Sdl.Version
		{
			Major = 2,
			Minor = 0,
			Patch = 4
		} || !this.AllowUserResizing)
		{
			int winFlags = Sdl.Window.GetWindowFlags(this.Handle);
			if (CurrentPlatform.OS == OS.Windows && (winFlags & 0x80) == 0)
			{
				Sdl.Window.SetPosition(this.Handle, centerX, centerY);
			}
		}
		if (this.IsFullScreen != this._willBeFullScreen)
		{
			base.OnClientSizeChanged();
		}
		this.IsFullScreen = this._willBeFullScreen;
		this._supressMoved = true;
	}

	internal void Moved(int x, int y)
	{
		if (this.IsFullScreen && !this._applyingFullScreenMove)
		{
			int display = -1;
			for (int i = 0; i < Sdl.Display.GetNumVideoDisplays(); i++)
			{
				Sdl.Display.GetBounds(i, out var rect);
				if (x >= rect.X && x < rect.X + rect.Width && y >= rect.Y && y < rect.Y + rect.Height)
				{
					display = i;
					break;
				}
			}
			if (this.GetDisplayIndex() != display)
			{
				this._applyingFullScreenMove = true;
				Sdl.Window.SetFullscreen(this.Handle, 0);
				Sdl.Window.SetPosition(this.Handle, x, y);
				int fullscreenFlag = (this._game.graphicsDeviceManager.HardwareModeSwitch ? 1 : 4097);
				Sdl.Window.SetFullscreen(this.Handle, fullscreenFlag);
				if (fullscreenFlag == 4097 && display >= 0)
				{
					Sdl.Display.GetBounds(display, out var new_display_bounds);
					this._game.graphicsDeviceManager.PreferredBackBufferWidth = new_display_bounds.Width;
					this._game.graphicsDeviceManager.PreferredBackBufferHeight = new_display_bounds.Height;
				}
			}
		}
		else if (this._supressMoved)
		{
			this._supressMoved = false;
		}
		else
		{
			this._wasMoved = true;
		}
	}

	public virtual void OnEventsHandled()
	{
		this._applyingFullScreenMove = false;
	}

	public override int GetDisplayIndex()
	{
		return Sdl.Display.GetWindowDisplayIndex(this._handle);
	}

	public override Rectangle GetDisplayBounds(int index)
	{
		if (index >= 0 && index < Sdl.Display.GetNumVideoDisplays())
		{
			Sdl.Display.GetBounds(index, out var rect);
			return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}
		return Rectangle.Empty;
	}

	public override bool CenterOnDisplay(int index)
	{
		if (index >= 0 && index < Sdl.Display.GetNumVideoDisplays())
		{
			Sdl.Display.GetBounds(index, out var rect);
			this.Position = new Point(rect.X + rect.Width / 2 - this.ClientBounds.Width / 2, rect.Y + rect.Height / 2 - this.ClientBounds.Height / 2);
			return true;
		}
		return false;
	}

	public void ClientResize(int width, int height)
	{
		if (this._game.GraphicsDevice.PresentationParameters.BackBufferWidth != width || this._game.GraphicsDevice.PresentationParameters.BackBufferHeight != height)
		{
			this._game.GraphicsDevice.PresentationParameters.BackBufferWidth = width;
			this._game.GraphicsDevice.PresentationParameters.BackBufferHeight = height;
			this._game.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);
			Sdl.Window.GetSize(this.Handle, out this._width, out this._height);
			base.OnClientSizeChanged();
		}
	}

	protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
	{
	}

	protected override void SetTitle(string title)
	{
		Sdl.Window.SetTitle(this._handle, title);
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
			Sdl.Window.Destroy(this._handle);
			this._handle = IntPtr.Zero;
			if (this._icon != IntPtr.Zero)
			{
				Sdl.FreeSurface(this._icon);
			}
			this._disposed = true;
		}
	}
}
