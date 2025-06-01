using System;
using System.Runtime.InteropServices;
using System.Text;
using MonoGame.Framework.Utilities;

internal static class Sdl
{
	[Flags]
	public enum InitFlags
	{
		Video = 0x20,
		Joystick = 0x200,
		Haptic = 0x1000,
		GameController = 0x2000
	}

	public enum EventType : uint
	{
		First = 0u,
		Quit = 256u,
		WindowEvent = 512u,
		SysWM = 513u,
		KeyDown = 768u,
		KeyUp = 769u,
		TextEditing = 770u,
		TextInput = 771u,
		MouseMotion = 1024u,
		MouseButtonDown = 1025u,
		MouseButtonup = 1026u,
		MouseWheel = 1027u,
		JoyAxisMotion = 1536u,
		JoyBallMotion = 1537u,
		JoyHatMotion = 1538u,
		JoyButtonDown = 1539u,
		JoyButtonUp = 1540u,
		JoyDeviceAdded = 1541u,
		JoyDeviceRemoved = 1542u,
		ControllerAxisMotion = 1616u,
		ControllerButtonDown = 1617u,
		ControllerButtonUp = 1618u,
		ControllerDeviceAdded = 1619u,
		ControllerDeviceRemoved = 1620u,
		ControllerDeviceRemapped = 1620u,
		FingerDown = 1792u,
		FingerUp = 1793u,
		FingerMotion = 1794u,
		DollarGesture = 2048u,
		DollarRecord = 2049u,
		MultiGesture = 2050u,
		ClipboardUpdate = 2304u,
		DropFile = 4096u,
		DropText = 4097u,
		DropBegin = 4098u,
		DropComplete = 4099u,
		AudioDeviceAdded = 4352u,
		AudioDeviceRemoved = 4353u,
		RenderTargetsReset = 8192u,
		RenderDeviceReset = 8193u,
		UserEvent = 32768u,
		Last = 65535u
	}

	public enum EventAction
	{
		AddEvent,
		PeekEvent,
		GetEvent
	}

	[StructLayout(LayoutKind.Explicit, Size = 56)]
	public struct Event
	{
		[FieldOffset(0)]
		public EventType Type;

		[FieldOffset(0)]
		public Window.Event Window;

		[FieldOffset(0)]
		public Keyboard.Event Key;

		[FieldOffset(0)]
		public Mouse.MotionEvent Motion;

		[FieldOffset(0)]
		public Keyboard.TextEditingEvent Edit;

		[FieldOffset(0)]
		public Keyboard.TextInputEvent Text;

		[FieldOffset(0)]
		public Mouse.WheelEvent Wheel;

		[FieldOffset(0)]
		public Joystick.DeviceEvent JoystickDevice;

		[FieldOffset(0)]
		public GameController.DeviceEvent ControllerDevice;

		[FieldOffset(0)]
		public Drop.Event Drop;
	}

	public struct Rectangle
	{
		public int X;

		public int Y;

		public int Width;

		public int Height;
	}

	public struct Version
	{
		public byte Major;

		public byte Minor;

		public byte Patch;

		public static bool operator >(Version version1, Version version2)
		{
			return Version.ConcatenateVersion(version1) > Version.ConcatenateVersion(version2);
		}

		public static bool operator <(Version version1, Version version2)
		{
			return Version.ConcatenateVersion(version1) < Version.ConcatenateVersion(version2);
		}

		public static bool operator ==(Version version1, Version version2)
		{
			if (version1.Major == version2.Major && version1.Minor == version2.Minor)
			{
				return version1.Patch == version2.Patch;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Version))
			{
				return false;
			}
			return Sdl.version == (Version)obj;
		}

		public override int GetHashCode()
		{
			return ((17 * 23 + this.Major.GetHashCode()) * 23 + this.Minor.GetHashCode()) * 23 + this.Patch.GetHashCode();
		}

		public static bool operator !=(Version version1, Version version2)
		{
			return !(version1 == version2);
		}

		public static bool operator >=(Version version1, Version version2)
		{
			if (!(version1 == version2))
			{
				return version1 > version2;
			}
			return true;
		}

		public static bool operator <=(Version version1, Version version2)
		{
			if (!(version1 == version2))
			{
				return version1 < version2;
			}
			return true;
		}

		public override string ToString()
		{
			return this.Major + "." + this.Minor + "." + this.Patch;
		}

		private static int ConcatenateVersion(Version version)
		{
			if (version.Major == 2 && version.Minor == 0 && version.Patch < 23)
			{
				return version.Major * 1000000 + version.Patch * 1000;
			}
			return version.Major * 1000000 + version.Minor * 1000 + version.Patch;
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int d_sdl_init(int flags);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void d_sdl_disablescreensaver();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void d_sdl_getversion(out Version version);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int d_sdl_pollevent(out Event _event);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int d_sdl_pumpevents();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr d_sdl_creatergbsurfacefrom(IntPtr pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void d_sdl_freesurface(IntPtr surface);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr d_sdl_geterror();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void d_sdl_clearerror();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate IntPtr d_sdl_gethint(string name);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr d_sdl_loadbmp_rw(IntPtr src, int freesrc);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void d_sdl_quit();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr d_sdl_rwfrommem(byte[] mem, int size);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int d_sdl_sethint(string name, string value);

	public static class Window
	{
		public enum EventId : byte
		{
			None,
			Shown,
			Hidden,
			Exposed,
			Moved,
			Resized,
			SizeChanged,
			Minimized,
			Maximized,
			Restored,
			Enter,
			Leave,
			FocusGained,
			FocusLost,
			Close
		}

		public static class State
		{
			public const int Fullscreen = 1;

			public const int OpenGL = 2;

			public const int Shown = 4;

			public const int Hidden = 8;

			public const int Borderless = 16;

			public const int Resizable = 32;

			public const int Minimized = 64;

			public const int Maximized = 128;

			public const int Grabbed = 256;

			public const int InputFocus = 512;

			public const int MouseFocus = 1024;

			public const int FullscreenDesktop = 4097;

			public const int Foreign = 2048;

			public const int AllowHighDPI = 8192;

			public const int MouseCapture = 16384;
		}

		public struct Event
		{
			public EventType Type;

			public uint TimeStamp;

			public uint WindowID;

			public EventId EventID;

			private byte padding1;

			private byte padding2;

			private byte padding3;

			public int Data1;

			public int Data2;
		}

		public enum SysWMType
		{
			Unknow,
			Windows,
			X11,
			Directfb,
			Cocoa,
			UiKit,
			Wayland,
			Mir,
			Android
		}

		public struct SDL_SysWMinfo
		{
			public Version version;

			public SysWMType subsystem;

			public IntPtr window;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_createwindow(string title, int x, int y, int w, int h, int flags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_destroywindow(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate uint d_sdl_getwindowid(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getwindowdisplayindex(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_getwindowflags(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_setwindowicon(IntPtr window, IntPtr icon);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_getwindowposition(IntPtr window, out int x, out int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_getwindowsize(IntPtr window, out int w, out int h);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_setwindowbordered(IntPtr window, int bordered);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_setwindowfullscreen(IntPtr window, int flags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_setwindowposition(IntPtr window, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_setwindowresizable(IntPtr window, bool resizable);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_setwindowsize(IntPtr window, int w, int h);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void d_sdl_setwindowtitle(IntPtr window, ref byte value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_showwindow(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool d_sdl_getwindowwminfo(IntPtr window, ref SDL_SysWMinfo sysWMinfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_getwindowborderssize(IntPtr window, out int top, out int left, out int right, out int bottom);

		public const int PosUndefined = 536805376;

		public const int PosCentered = 805240832;

		private static d_sdl_createwindow SDL_CreateWindow = FuncLoader.LoadFunction<d_sdl_createwindow>(Sdl.NativeLibrary, "SDL_CreateWindow");

		public static d_sdl_destroywindow Destroy = FuncLoader.LoadFunction<d_sdl_destroywindow>(Sdl.NativeLibrary, "SDL_DestroyWindow");

		public static d_sdl_getwindowid GetWindowId = FuncLoader.LoadFunction<d_sdl_getwindowid>(Sdl.NativeLibrary, "SDL_GetWindowID");

		private static d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex = FuncLoader.LoadFunction<d_sdl_getwindowdisplayindex>(Sdl.NativeLibrary, "SDL_GetWindowDisplayIndex");

		public static d_sdl_getwindowflags GetWindowFlags = FuncLoader.LoadFunction<d_sdl_getwindowflags>(Sdl.NativeLibrary, "SDL_GetWindowFlags");

		public static d_sdl_setwindowicon SetIcon = FuncLoader.LoadFunction<d_sdl_setwindowicon>(Sdl.NativeLibrary, "SDL_SetWindowIcon");

		public static d_sdl_getwindowposition GetPosition = FuncLoader.LoadFunction<d_sdl_getwindowposition>(Sdl.NativeLibrary, "SDL_GetWindowPosition");

		public static d_sdl_getwindowsize GetSize = FuncLoader.LoadFunction<d_sdl_getwindowsize>(Sdl.NativeLibrary, "SDL_GetWindowSize");

		public static d_sdl_setwindowbordered SetBordered = FuncLoader.LoadFunction<d_sdl_setwindowbordered>(Sdl.NativeLibrary, "SDL_SetWindowBordered");

		private static d_sdl_setwindowfullscreen SDL_SetWindowFullscreen = FuncLoader.LoadFunction<d_sdl_setwindowfullscreen>(Sdl.NativeLibrary, "SDL_SetWindowFullscreen");

		public static d_sdl_setwindowposition SetPosition = FuncLoader.LoadFunction<d_sdl_setwindowposition>(Sdl.NativeLibrary, "SDL_SetWindowPosition");

		public static d_sdl_setwindowresizable SetResizable = FuncLoader.LoadFunction<d_sdl_setwindowresizable>(Sdl.NativeLibrary, "SDL_SetWindowResizable");

		public static d_sdl_setwindowsize SetSize = FuncLoader.LoadFunction<d_sdl_setwindowsize>(Sdl.NativeLibrary, "SDL_SetWindowSize");

		private static d_sdl_setwindowtitle SDL_SetWindowTitle = FuncLoader.LoadFunction<d_sdl_setwindowtitle>(Sdl.NativeLibrary, "SDL_SetWindowTitle");

		public static d_sdl_showwindow Show = FuncLoader.LoadFunction<d_sdl_showwindow>(Sdl.NativeLibrary, "SDL_ShowWindow");

		public static d_sdl_getwindowwminfo GetWindowWMInfo = FuncLoader.LoadFunction<d_sdl_getwindowwminfo>(Sdl.NativeLibrary, "SDL_GetWindowWMInfo");

		public static d_sdl_getwindowborderssize GetBorderSize = FuncLoader.LoadFunction<d_sdl_getwindowborderssize>(Sdl.NativeLibrary, "SDL_GetWindowBordersSize");

		public static IntPtr Create(string title, int x, int y, int w, int h, int flags)
		{
			return Sdl.GetError(Window.SDL_CreateWindow(title, x, y, w, h, flags));
		}

		public static int GetDisplayIndex(IntPtr window)
		{
			return Sdl.GetError(Window.SDL_GetWindowDisplayIndex(window));
		}

		public static void SetFullscreen(IntPtr window, int flags)
		{
			Sdl.GetError(Window.SDL_SetWindowFullscreen(window, flags));
		}

		public static void SetTitle(IntPtr handle, string title)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(title + "\0");
			Window.SDL_SetWindowTitle(handle, ref bytes[0]);
		}
	}

	public static class Display
	{
		public struct Mode
		{
			public uint Format;

			public int Width;

			public int Height;

			public int RefreshRate;

			public IntPtr DriverData;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getdisplaybounds(int displayIndex, out Rectangle rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getcurrentdisplaymode(int displayIndex, out Mode mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getdisplaymode(int displayIndex, int modeIndex, out Mode mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getclosestdisplaymode(int displayIndex, Mode mode, out Mode closest);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_getdisplayname(int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getnumdisplaymodes(int displayIndex);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getnumvideodisplays();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_getwindowdisplayindex(IntPtr window);

		private static d_sdl_getdisplaybounds SDL_GetDisplayBounds = FuncLoader.LoadFunction<d_sdl_getdisplaybounds>(Sdl.NativeLibrary, "SDL_GetDisplayBounds");

		private static d_sdl_getcurrentdisplaymode SDL_GetCurrentDisplayMode = FuncLoader.LoadFunction<d_sdl_getcurrentdisplaymode>(Sdl.NativeLibrary, "SDL_GetCurrentDisplayMode");

		private static d_sdl_getdisplaymode SDL_GetDisplayMode = FuncLoader.LoadFunction<d_sdl_getdisplaymode>(Sdl.NativeLibrary, "SDL_GetDisplayMode");

		private static d_sdl_getclosestdisplaymode SDL_GetClosestDisplayMode = FuncLoader.LoadFunction<d_sdl_getclosestdisplaymode>(Sdl.NativeLibrary, "SDL_GetClosestDisplayMode");

		private static d_sdl_getdisplayname SDL_GetDisplayName = FuncLoader.LoadFunction<d_sdl_getdisplayname>(Sdl.NativeLibrary, "SDL_GetDisplayName");

		private static d_sdl_getnumdisplaymodes SDL_GetNumDisplayModes = FuncLoader.LoadFunction<d_sdl_getnumdisplaymodes>(Sdl.NativeLibrary, "SDL_GetNumDisplayModes");

		private static d_sdl_getnumvideodisplays SDL_GetNumVideoDisplays = FuncLoader.LoadFunction<d_sdl_getnumvideodisplays>(Sdl.NativeLibrary, "SDL_GetNumVideoDisplays");

		private static d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex = FuncLoader.LoadFunction<d_sdl_getwindowdisplayindex>(Sdl.NativeLibrary, "SDL_GetWindowDisplayIndex");

		public static void GetBounds(int displayIndex, out Rectangle rect)
		{
			Sdl.GetError(Display.SDL_GetDisplayBounds(displayIndex, out rect));
		}

		public static void GetCurrentDisplayMode(int displayIndex, out Mode mode)
		{
			Sdl.GetError(Display.SDL_GetCurrentDisplayMode(displayIndex, out mode));
		}

		public static void GetDisplayMode(int displayIndex, int modeIndex, out Mode mode)
		{
			Sdl.GetError(Display.SDL_GetDisplayMode(displayIndex, modeIndex, out mode));
		}

		public static void GetClosestDisplayMode(int displayIndex, Mode mode, out Mode closest)
		{
			Sdl.GetError(Display.SDL_GetClosestDisplayMode(displayIndex, mode, out closest));
		}

		public static string GetDisplayName(int index)
		{
			return InteropHelpers.Utf8ToString(Sdl.GetError(Display.SDL_GetDisplayName(index)));
		}

		public static int GetNumDisplayModes(int displayIndex)
		{
			return Sdl.GetError(Display.SDL_GetNumDisplayModes(displayIndex));
		}

		public static int GetNumVideoDisplays()
		{
			return Sdl.GetError(Display.SDL_GetNumVideoDisplays());
		}

		public static int GetWindowDisplayIndex(IntPtr window)
		{
			return Sdl.GetError(Display.SDL_GetWindowDisplayIndex(window));
		}
	}

	public static class GL
	{
		public enum Attribute
		{
			RedSize,
			GreenSize,
			BlueSize,
			AlphaSize,
			BufferSize,
			DoubleBuffer,
			DepthSize,
			StencilSize,
			AccumRedSize,
			AccumGreenSize,
			AccumBlueSize,
			AccumAlphaSize,
			Stereo,
			MultiSampleBuffers,
			MultiSampleSamples,
			AcceleratedVisual,
			RetainedBacking,
			ContextMajorVersion,
			ContextMinorVersion,
			ContextEgl,
			ContextFlags,
			ContextProfileMAsl,
			ShareWithCurrentContext,
			FramebufferSRGBCapable,
			ContextReleaseBehaviour
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_gl_createcontext(IntPtr window);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_gl_deletecontext(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_gl_getcurrentcontext();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr d_sdl_gl_getprocaddress(string proc);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_gl_getswapinterval();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_gl_makecurrent(IntPtr window, IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_gl_setattribute(Attribute attr, int value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_gl_setswapinterval(int interval);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_gl_swapwindow(IntPtr window);

		private static d_sdl_gl_createcontext SDL_GL_CreateContext = FuncLoader.LoadFunction<d_sdl_gl_createcontext>(Sdl.NativeLibrary, "SDL_GL_CreateContext");

		public static d_sdl_gl_deletecontext DeleteContext = FuncLoader.LoadFunction<d_sdl_gl_deletecontext>(Sdl.NativeLibrary, "SDL_GL_DeleteContext");

		private static d_sdl_gl_getcurrentcontext SDL_GL_GetCurrentContext = FuncLoader.LoadFunction<d_sdl_gl_getcurrentcontext>(Sdl.NativeLibrary, "SDL_GL_GetCurrentContext");

		public static d_sdl_gl_getprocaddress GetProcAddress = FuncLoader.LoadFunction<d_sdl_gl_getprocaddress>(Sdl.NativeLibrary, "SDL_GL_GetProcAddress");

		public static d_sdl_gl_getswapinterval GetSwapInterval = FuncLoader.LoadFunction<d_sdl_gl_getswapinterval>(Sdl.NativeLibrary, "SDL_GL_GetSwapInterval");

		public static d_sdl_gl_makecurrent MakeCurrent = FuncLoader.LoadFunction<d_sdl_gl_makecurrent>(Sdl.NativeLibrary, "SDL_GL_MakeCurrent");

		private static d_sdl_gl_setattribute SDL_GL_SetAttribute = FuncLoader.LoadFunction<d_sdl_gl_setattribute>(Sdl.NativeLibrary, "SDL_GL_SetAttribute");

		public static d_sdl_gl_setswapinterval SetSwapInterval = FuncLoader.LoadFunction<d_sdl_gl_setswapinterval>(Sdl.NativeLibrary, "SDL_GL_SetSwapInterval");

		public static d_sdl_gl_swapwindow SwapWindow = FuncLoader.LoadFunction<d_sdl_gl_swapwindow>(Sdl.NativeLibrary, "SDL_GL_SwapWindow");

		public static IntPtr CreateContext(IntPtr window)
		{
			return Sdl.GetError(GL.SDL_GL_CreateContext(window));
		}

		public static IntPtr GetCurrentContext()
		{
			return Sdl.GetError(GL.SDL_GL_GetCurrentContext());
		}

		public static int SetAttribute(Attribute attr, int value)
		{
			return Sdl.GetError(GL.SDL_GL_SetAttribute(attr, value));
		}
	}

	public static class Mouse
	{
		[Flags]
		public enum Button
		{
			Left = 1,
			Middle = 2,
			Right = 4,
			X1Mask = 8,
			X2Mask = 0x10
		}

		public enum SystemCursor
		{
			Arrow,
			IBeam,
			Wait,
			Crosshair,
			WaitArrow,
			SizeNWSE,
			SizeNESW,
			SizeWE,
			SizeNS,
			SizeAll,
			No,
			Hand
		}

		public struct MotionEvent
		{
			public EventType Type;

			public uint Timestamp;

			public uint WindowID;

			public uint Which;

			public byte State;

			private byte _padding1;

			private byte _padding2;

			private byte _padding3;

			public int X;

			public int Y;

			public int Xrel;

			public int Yrel;
		}

		public struct WheelEvent
		{
			public EventType Type;

			public uint TimeStamp;

			public uint WindowId;

			public uint Which;

			public int X;

			public int Y;

			public uint Direction;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_createcolorcursor(IntPtr surface, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_createsystemcursor(SystemCursor id);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_freecursor(IntPtr cursor);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate Button d_sdl_getglobalmousestate(out int x, out int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate Button d_sdl_getmousestate(out int x, out int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_setcursor(IntPtr cursor);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_showcursor(int toggle);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_warpmouseinwindow(IntPtr window, int x, int y);

		private static d_sdl_createcolorcursor SDL_CreateColorCursor = FuncLoader.LoadFunction<d_sdl_createcolorcursor>(Sdl.NativeLibrary, "SDL_CreateColorCursor");

		private static d_sdl_createsystemcursor SDL_CreateSystemCursor = FuncLoader.LoadFunction<d_sdl_createsystemcursor>(Sdl.NativeLibrary, "SDL_CreateSystemCursor");

		public static d_sdl_freecursor FreeCursor = FuncLoader.LoadFunction<d_sdl_freecursor>(Sdl.NativeLibrary, "SDL_FreeCursor");

		public static d_sdl_getglobalmousestate GetGlobalState = FuncLoader.LoadFunction<d_sdl_getglobalmousestate>(Sdl.NativeLibrary, "SDL_GetGlobalMouseState");

		public static d_sdl_getmousestate GetState = FuncLoader.LoadFunction<d_sdl_getmousestate>(Sdl.NativeLibrary, "SDL_GetMouseState");

		public static d_sdl_setcursor SetCursor = FuncLoader.LoadFunction<d_sdl_setcursor>(Sdl.NativeLibrary, "SDL_SetCursor");

		public static d_sdl_showcursor ShowCursor = FuncLoader.LoadFunction<d_sdl_showcursor>(Sdl.NativeLibrary, "SDL_ShowCursor");

		public static d_sdl_warpmouseinwindow WarpInWindow = FuncLoader.LoadFunction<d_sdl_warpmouseinwindow>(Sdl.NativeLibrary, "SDL_WarpMouseInWindow");

		public static IntPtr CreateColorCursor(IntPtr surface, int x, int y)
		{
			return Sdl.GetError(Mouse.SDL_CreateColorCursor(surface, x, y));
		}

		public static IntPtr CreateSystemCursor(SystemCursor id)
		{
			return Sdl.GetError(Mouse.SDL_CreateSystemCursor(id));
		}
	}

	public static class Keyboard
	{
		public struct Keysym
		{
			public int Scancode;

			public int Sym;

			public Keymod Mod;

			public uint Unicode;
		}

		[Flags]
		public enum Keymod : ushort
		{
			None = 0,
			LeftShift = 1,
			RightShift = 2,
			LeftCtrl = 0x40,
			RightCtrl = 0x80,
			LeftAlt = 0x100,
			RightAlt = 0x200,
			LeftGui = 0x400,
			RightGui = 0x800,
			NumLock = 0x1000,
			CapsLock = 0x2000,
			AltGr = 0x4000,
			Reserved = 0x8000,
			Ctrl = 0xC0,
			Shift = 3,
			Alt = 0x300,
			Gui = 0xC00
		}

		public struct Event
		{
			public EventType Type;

			public uint TimeStamp;

			public uint WindowId;

			public byte State;

			public byte Repeat;

			private byte padding2;

			private byte padding3;

			public Keysym Keysym;
		}

		public struct TextEditingEvent
		{
			public EventType Type;

			public uint Timestamp;

			public uint WindowId;

			public unsafe fixed byte Text[32];

			public int Start;

			public int Length;
		}

		public struct TextInputEvent
		{
			public EventType Type;

			public uint Timestamp;

			public uint WindowId;

			public unsafe fixed byte Text[32];
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate Keymod d_sdl_getmodstate();

		public static d_sdl_getmodstate GetModState = FuncLoader.LoadFunction<d_sdl_getmodstate>(Sdl.NativeLibrary, "SDL_GetModState");
	}

	public static class Joystick
	{
		[Flags]
		public enum Hat : byte
		{
			Centered = 0,
			Up = 1,
			Right = 2,
			Down = 4,
			Left = 8
		}

		public struct DeviceEvent
		{
			public EventType Type;

			public uint TimeStamp;

			public int Which;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_joystickclose(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate short d_sdl_joystickgetaxis(IntPtr joystick, int axis);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate byte d_sdl_joystickgetbutton(IntPtr joystick, int button);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_joystickname(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate Guid d_sdl_joystickgetguid(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate Hat d_sdl_joystickgethat(IntPtr joystick, int hat);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_joystickinstanceid(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_joystickopen(int deviceIndex);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_joysticknumaxes(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_joysticknumbuttons(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_joysticknumhats(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_numjoysticks();

		public static d_sdl_joystickclose Close = FuncLoader.LoadFunction<d_sdl_joystickclose>(Sdl.NativeLibrary, "SDL_JoystickClose");

		private static d_sdl_joystickfrominstanceid SDL_JoystickFromInstanceID = FuncLoader.LoadFunction<d_sdl_joystickfrominstanceid>(Sdl.NativeLibrary, "SDL_JoystickFromInstanceID");

		public static d_sdl_joystickgetaxis GetAxis = FuncLoader.LoadFunction<d_sdl_joystickgetaxis>(Sdl.NativeLibrary, "SDL_JoystickGetAxis");

		public static d_sdl_joystickgetbutton GetButton = FuncLoader.LoadFunction<d_sdl_joystickgetbutton>(Sdl.NativeLibrary, "SDL_JoystickGetButton");

		private static d_sdl_joystickname JoystickName = FuncLoader.LoadFunction<d_sdl_joystickname>(Sdl.NativeLibrary, "SDL_JoystickName");

		public static d_sdl_joystickgetguid GetGUID = FuncLoader.LoadFunction<d_sdl_joystickgetguid>(Sdl.NativeLibrary, "SDL_JoystickGetGUID");

		public static d_sdl_joystickgethat GetHat = FuncLoader.LoadFunction<d_sdl_joystickgethat>(Sdl.NativeLibrary, "SDL_JoystickGetHat");

		public static d_sdl_joystickinstanceid InstanceID = FuncLoader.LoadFunction<d_sdl_joystickinstanceid>(Sdl.NativeLibrary, "SDL_JoystickInstanceID");

		private static d_sdl_joystickopen SDL_JoystickOpen = FuncLoader.LoadFunction<d_sdl_joystickopen>(Sdl.NativeLibrary, "SDL_JoystickOpen");

		private static d_sdl_joysticknumaxes SDL_JoystickNumAxes = FuncLoader.LoadFunction<d_sdl_joysticknumaxes>(Sdl.NativeLibrary, "SDL_JoystickNumAxes");

		private static d_sdl_joysticknumbuttons SDL_JoystickNumButtons = FuncLoader.LoadFunction<d_sdl_joysticknumbuttons>(Sdl.NativeLibrary, "SDL_JoystickNumButtons");

		private static d_sdl_joysticknumhats SDL_JoystickNumHats = FuncLoader.LoadFunction<d_sdl_joysticknumhats>(Sdl.NativeLibrary, "SDL_JoystickNumHats");

		private static d_sdl_numjoysticks SDL_NumJoysticks = FuncLoader.LoadFunction<d_sdl_numjoysticks>(Sdl.NativeLibrary, "SDL_NumJoysticks");

		public static IntPtr FromInstanceID(int joyid)
		{
			return Sdl.GetError(Joystick.SDL_JoystickFromInstanceID(joyid));
		}

		public static string GetJoystickName(IntPtr joystick)
		{
			return InteropHelpers.Utf8ToString(Joystick.JoystickName(joystick));
		}

		public static IntPtr Open(int deviceIndex)
		{
			return Sdl.GetError(Joystick.SDL_JoystickOpen(deviceIndex));
		}

		public static int NumAxes(IntPtr joystick)
		{
			return Sdl.GetError(Joystick.SDL_JoystickNumAxes(joystick));
		}

		public static int NumButtons(IntPtr joystick)
		{
			return Sdl.GetError(Joystick.SDL_JoystickNumButtons(joystick));
		}

		public static int NumHats(IntPtr joystick)
		{
			return Sdl.GetError(Joystick.SDL_JoystickNumHats(joystick));
		}

		public static int NumJoysticks()
		{
			return Sdl.GetError(Joystick.SDL_NumJoysticks());
		}
	}

	public static class GameController
	{
		public enum Axis
		{
			Invalid = -1,
			LeftX,
			LeftY,
			RightX,
			RightY,
			TriggerLeft,
			TriggerRight,
			Max
		}

		public enum Button
		{
			Invalid = -1,
			A,
			B,
			X,
			Y,
			Back,
			Guide,
			Start,
			LeftStick,
			RightStick,
			LeftShoulder,
			RightShoulder,
			DpadUp,
			DpadDown,
			DpadLeft,
			DpadRight,
			Max
		}

		public struct DeviceEvent
		{
			public EventType Type;

			public uint TimeStamp;

			public int Which;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_free(IntPtr ptr);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_gamecontrolleraddmapping(string mappingString);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_gamecontrolleraddmappingsfromrw(IntPtr rw, int freew);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool d_sdl_gamecontrollerhasbutton(IntPtr gamecontroller, Button button);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool d_sdl_gamecontrollerhasaxis(IntPtr gamecontroller, Axis axis);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_gamecontrollerclose(IntPtr gamecontroller);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate short d_sdl_gamecontrollergetaxis(IntPtr gamecontroller, Axis axis);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate byte d_sdl_gamecontrollergetbutton(IntPtr gamecontroller, Button button);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_gamecontrollergetjoystick(IntPtr gamecontroller);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate byte d_sdl_isgamecontroller(int joystickIndex);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_gamecontrollermapping(IntPtr gamecontroller);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_gamecontrolleropen(int joystickIndex);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_gamecontrollername(IntPtr gamecontroller);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_gamecontrollerrumble(IntPtr gamecontroller, ushort left, ushort right, uint duration);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate byte d_sdl_gamecontrollerhasrumble(IntPtr gamecontroller);

		public static d_sdl_free SDL_Free = FuncLoader.LoadFunction<d_sdl_free>(Sdl.NativeLibrary, "SDL_free");

		public static d_sdl_gamecontrolleraddmapping AddMapping = FuncLoader.LoadFunction<d_sdl_gamecontrolleraddmapping>(Sdl.NativeLibrary, "SDL_GameControllerAddMapping");

		public static d_sdl_gamecontrolleraddmappingsfromrw AddMappingFromRw = FuncLoader.LoadFunction<d_sdl_gamecontrolleraddmappingsfromrw>(Sdl.NativeLibrary, "SDL_GameControllerAddMappingsFromRW");

		public static d_sdl_gamecontrollerhasbutton HasButton = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasbutton>(Sdl.NativeLibrary, "SDL_GameControllerHasButton");

		public static d_sdl_gamecontrollerhasaxis HasAxis = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasaxis>(Sdl.NativeLibrary, "SDL_GameControllerHasAxis");

		public static d_sdl_gamecontrollerclose Close = FuncLoader.LoadFunction<d_sdl_gamecontrollerclose>(Sdl.NativeLibrary, "SDL_GameControllerClose");

		private static d_sdl_joystickfrominstanceid SDL_GameControllerFromInstanceID = FuncLoader.LoadFunction<d_sdl_joystickfrominstanceid>(Sdl.NativeLibrary, "SDL_JoystickFromInstanceID");

		public static d_sdl_gamecontrollergetaxis GetAxis = FuncLoader.LoadFunction<d_sdl_gamecontrollergetaxis>(Sdl.NativeLibrary, "SDL_GameControllerGetAxis");

		public static d_sdl_gamecontrollergetbutton GetButton = FuncLoader.LoadFunction<d_sdl_gamecontrollergetbutton>(Sdl.NativeLibrary, "SDL_GameControllerGetButton");

		private static d_sdl_gamecontrollergetjoystick SDL_GameControllerGetJoystick = FuncLoader.LoadFunction<d_sdl_gamecontrollergetjoystick>(Sdl.NativeLibrary, "SDL_GameControllerGetJoystick");

		public static d_sdl_isgamecontroller IsGameController = FuncLoader.LoadFunction<d_sdl_isgamecontroller>(Sdl.NativeLibrary, "SDL_IsGameController");

		private static d_sdl_gamecontrollermapping SDL_GameControllerMapping = FuncLoader.LoadFunction<d_sdl_gamecontrollermapping>(Sdl.NativeLibrary, "SDL_GameControllerMapping");

		private static d_sdl_gamecontrolleropen SDL_GameControllerOpen = FuncLoader.LoadFunction<d_sdl_gamecontrolleropen>(Sdl.NativeLibrary, "SDL_GameControllerOpen");

		private static d_sdl_gamecontrollername SDL_GameControllerName = FuncLoader.LoadFunction<d_sdl_gamecontrollername>(Sdl.NativeLibrary, "SDL_GameControllerName");

		public static d_sdl_gamecontrollerrumble Rumble = FuncLoader.LoadFunction<d_sdl_gamecontrollerrumble>(Sdl.NativeLibrary, "SDL_GameControllerRumble");

		public static d_sdl_gamecontrollerrumble RumbleTriggers = FuncLoader.LoadFunction<d_sdl_gamecontrollerrumble>(Sdl.NativeLibrary, "SDL_GameControllerRumbleTriggers");

		public static d_sdl_gamecontrollerhasrumble HasRumble = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasrumble>(Sdl.NativeLibrary, "SDL_GameControllerHasRumble");

		public static d_sdl_gamecontrollerhasrumble HasRumbleTriggers = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasrumble>(Sdl.NativeLibrary, "SDL_GameControllerHasRumbleTriggers");

		public static IntPtr FromInstanceID(int joyid)
		{
			return Sdl.GetError(GameController.SDL_GameControllerFromInstanceID(joyid));
		}

		public static IntPtr GetJoystick(IntPtr gamecontroller)
		{
			return Sdl.GetError(GameController.SDL_GameControllerGetJoystick(gamecontroller));
		}

		public static string GetMapping(IntPtr gamecontroller)
		{
			IntPtr nativeStr = GameController.SDL_GameControllerMapping(gamecontroller);
			if (nativeStr == IntPtr.Zero)
			{
				return string.Empty;
			}
			string result = InteropHelpers.Utf8ToString(nativeStr);
			GameController.SDL_Free(nativeStr);
			return result;
		}

		public static IntPtr Open(int joystickIndex)
		{
			return Sdl.GetError(GameController.SDL_GameControllerOpen(joystickIndex));
		}

		public static string GetName(IntPtr gamecontroller)
		{
			return InteropHelpers.Utf8ToString(GameController.SDL_GameControllerName(gamecontroller));
		}
	}

	public static class Haptic
	{
		public enum EffectId : ushort
		{
			LeftRight = 4
		}

		public struct LeftRight
		{
			public EffectId Type;

			public uint Length;

			public ushort LargeMagnitude;

			public ushort SmallMagnitude;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Effect
		{
			[FieldOffset(0)]
			public EffectId type;

			[FieldOffset(0)]
			public LeftRight leftright;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_hapticclose(IntPtr haptic);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_hapticeffectsupported(IntPtr haptic, ref Effect effect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int d_sdl_joystickishaptic(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_hapticneweffect(IntPtr haptic, ref Effect effect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr d_sdl_hapticopen(int device_index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr d_sdl_hapticopenfromjoystick(IntPtr joystick);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_hapticrumbleinit(IntPtr haptic);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_hapticrumbleplay(IntPtr haptic, float strength, uint length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_hapticrumblesupported(IntPtr haptic);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_hapticruneffect(IntPtr haptic, int effect, uint iterations);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_hapticstopall(IntPtr haptic);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int d_sdl_hapticupdateeffect(IntPtr haptic, int effect, ref Effect data);

		public const uint Infinity = 1000000u;

		public static d_sdl_hapticclose Close = FuncLoader.LoadFunction<d_sdl_hapticclose>(Sdl.NativeLibrary, "SDL_HapticClose");

		public static d_sdl_hapticeffectsupported EffectSupported = FuncLoader.LoadFunction<d_sdl_hapticeffectsupported>(Sdl.NativeLibrary, "SDL_HapticEffectSupported");

		public static d_sdl_joystickishaptic IsHaptic = FuncLoader.LoadFunction<d_sdl_joystickishaptic>(Sdl.NativeLibrary, "SDL_JoystickIsHaptic");

		private static d_sdl_hapticneweffect SDL_HapticNewEffect = FuncLoader.LoadFunction<d_sdl_hapticneweffect>(Sdl.NativeLibrary, "SDL_HapticNewEffect");

		public static d_sdl_hapticopen Open = FuncLoader.LoadFunction<d_sdl_hapticopen>(Sdl.NativeLibrary, "SDL_HapticOpen");

		private static d_sdl_hapticopenfromjoystick SDL_HapticOpenFromJoystick = FuncLoader.LoadFunction<d_sdl_hapticopenfromjoystick>(Sdl.NativeLibrary, "SDL_HapticOpenFromJoystick");

		private static d_sdl_hapticrumbleinit SDL_HapticRumbleInit = FuncLoader.LoadFunction<d_sdl_hapticrumbleinit>(Sdl.NativeLibrary, "SDL_HapticRumbleInit");

		private static d_sdl_hapticrumbleplay SDL_HapticRumblePlay = FuncLoader.LoadFunction<d_sdl_hapticrumbleplay>(Sdl.NativeLibrary, "SDL_HapticRumblePlay");

		private static d_sdl_hapticrumblesupported SDL_HapticRumbleSupported = FuncLoader.LoadFunction<d_sdl_hapticrumblesupported>(Sdl.NativeLibrary, "SDL_HapticRumbleSupported");

		private static d_sdl_hapticruneffect SDL_HapticRunEffect = FuncLoader.LoadFunction<d_sdl_hapticruneffect>(Sdl.NativeLibrary, "SDL_HapticRunEffect");

		private static d_sdl_hapticstopall SDL_HapticStopAll = FuncLoader.LoadFunction<d_sdl_hapticstopall>(Sdl.NativeLibrary, "SDL_HapticStopAll");

		private static d_sdl_hapticupdateeffect SDL_HapticUpdateEffect = FuncLoader.LoadFunction<d_sdl_hapticupdateeffect>(Sdl.NativeLibrary, "SDL_HapticUpdateEffect");

		public static void NewEffect(IntPtr haptic, ref Effect effect)
		{
			Sdl.GetError(Haptic.SDL_HapticNewEffect(haptic, ref effect));
		}

		public static IntPtr OpenFromJoystick(IntPtr joystick)
		{
			return Sdl.GetError(Haptic.SDL_HapticOpenFromJoystick(joystick));
		}

		public static void RumbleInit(IntPtr haptic)
		{
			Sdl.GetError(Haptic.SDL_HapticRumbleInit(haptic));
		}

		public static void RumblePlay(IntPtr haptic, float strength, uint length)
		{
			Sdl.GetError(Haptic.SDL_HapticRumblePlay(haptic, strength, length));
		}

		public static int RumbleSupported(IntPtr haptic)
		{
			return Sdl.GetError(Haptic.SDL_HapticRumbleSupported(haptic));
		}

		public static void RunEffect(IntPtr haptic, int effect, uint iterations)
		{
			Sdl.GetError(Haptic.SDL_HapticRunEffect(haptic, effect, iterations));
		}

		public static void StopAll(IntPtr haptic)
		{
			Sdl.GetError(Haptic.SDL_HapticStopAll(haptic));
		}

		public static void UpdateEffect(IntPtr haptic, int effect, ref Effect data)
		{
			Sdl.GetError(Haptic.SDL_HapticUpdateEffect(haptic, effect, ref data));
		}
	}

	public static class Drop
	{
		public struct Event
		{
			public EventType Type;

			public uint TimeStamp;

			public IntPtr File;

			public uint WindowId;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void d_sdl_free(IntPtr ptr);

		public static d_sdl_free SDL_Free = FuncLoader.LoadFunction<d_sdl_free>(Sdl.NativeLibrary, "SDL_free");
	}

	public static IntPtr NativeLibrary = Sdl.GetNativeLibrary();

	public static int Major;

	public static int Minor;

	public static int Patch;

	public static Version version;

	public static d_sdl_init SDL_Init = FuncLoader.LoadFunction<d_sdl_init>(Sdl.NativeLibrary, "SDL_Init");

	public static d_sdl_disablescreensaver DisableScreenSaver = FuncLoader.LoadFunction<d_sdl_disablescreensaver>(Sdl.NativeLibrary, "SDL_DisableScreenSaver");

	public static d_sdl_getversion GetVersion = FuncLoader.LoadFunction<d_sdl_getversion>(Sdl.NativeLibrary, "SDL_GetVersion");

	public static d_sdl_pollevent PollEvent = FuncLoader.LoadFunction<d_sdl_pollevent>(Sdl.NativeLibrary, "SDL_PollEvent");

	public static d_sdl_pumpevents PumpEvents = FuncLoader.LoadFunction<d_sdl_pumpevents>(Sdl.NativeLibrary, "SDL_PumpEvents");

	private static d_sdl_creatergbsurfacefrom SDL_CreateRGBSurfaceFrom = FuncLoader.LoadFunction<d_sdl_creatergbsurfacefrom>(Sdl.NativeLibrary, "SDL_CreateRGBSurfaceFrom");

	public static d_sdl_freesurface FreeSurface = FuncLoader.LoadFunction<d_sdl_freesurface>(Sdl.NativeLibrary, "SDL_FreeSurface");

	private static d_sdl_geterror SDL_GetError = FuncLoader.LoadFunction<d_sdl_geterror>(Sdl.NativeLibrary, "SDL_GetError");

	public static d_sdl_clearerror ClearError = FuncLoader.LoadFunction<d_sdl_clearerror>(Sdl.NativeLibrary, "SDL_ClearError");

	public static d_sdl_gethint SDL_GetHint = FuncLoader.LoadFunction<d_sdl_gethint>(Sdl.NativeLibrary, "SDL_GetHint");

	private static d_sdl_loadbmp_rw SDL_LoadBMP_RW = FuncLoader.LoadFunction<d_sdl_loadbmp_rw>(Sdl.NativeLibrary, "SDL_LoadBMP_RW");

	public static d_sdl_quit Quit = FuncLoader.LoadFunction<d_sdl_quit>(Sdl.NativeLibrary, "SDL_Quit");

	private static d_sdl_rwfrommem SDL_RWFromMem = FuncLoader.LoadFunction<d_sdl_rwfrommem>(Sdl.NativeLibrary, "SDL_RWFromMem");

	public static d_sdl_sethint SetHint = FuncLoader.LoadFunction<d_sdl_sethint>(Sdl.NativeLibrary, "SDL_SetHint");

	private static IntPtr GetNativeLibrary()
	{
		if (CurrentPlatform.OS == OS.Windows)
		{
			return FuncLoader.LoadLibraryExt("SDL2.dll");
		}
		if (CurrentPlatform.OS == OS.Linux)
		{
			return FuncLoader.LoadLibraryExt("libSDL2-2.0.so.0");
		}
		if (CurrentPlatform.OS == OS.MacOSX)
		{
			return FuncLoader.LoadLibraryExt("libSDL2-2.0.0.dylib");
		}
		return FuncLoader.LoadLibraryExt("sdl2");
	}

	public static void Init(int flags)
	{
		Sdl.GetError(Sdl.SDL_Init(flags));
	}

	public static IntPtr CreateRGBSurfaceFrom(byte[] pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask)
	{
		GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
		try
		{
			return Sdl.SDL_CreateRGBSurfaceFrom(handle.AddrOfPinnedObject(), width, height, depth, pitch, rMask, gMask, bMask, aMask);
		}
		finally
		{
			handle.Free();
		}
	}

	public static string GetError()
	{
		return InteropHelpers.Utf8ToString(Sdl.SDL_GetError());
	}

	public static int GetError(int value)
	{
		_ = 0;
		return value;
	}

	public static IntPtr GetError(IntPtr pointer)
	{
		_ = pointer == IntPtr.Zero;
		return pointer;
	}

	public static string GetHint(string name)
	{
		return InteropHelpers.Utf8ToString(Sdl.SDL_GetHint(name));
	}

	public static IntPtr LoadBMP_RW(IntPtr src, int freesrc)
	{
		return Sdl.GetError(Sdl.SDL_LoadBMP_RW(src, freesrc));
	}

	public static IntPtr RwFromMem(byte[] mem, int size)
	{
		return Sdl.GetError(Sdl.SDL_RWFromMem(mem, size));
	}
}
