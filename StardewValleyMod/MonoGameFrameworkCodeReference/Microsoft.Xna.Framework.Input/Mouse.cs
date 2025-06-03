using System;

namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Allows reading position and button click information from mouse.
/// </summary>
public static class Mouse
{
	internal static GameWindow PrimaryWindow;

	private static readonly MouseState _defaultState;

	internal static int ScrollX;

	internal static int ScrollY;

	/// <summary>
	/// Gets or sets the window handle for current mouse processing.
	/// </summary> 
	public static IntPtr WindowHandle
	{
		get
		{
			return Mouse.PlatformGetWindowHandle();
		}
		set
		{
			Mouse.PlatformSetWindowHandle(value);
		}
	}

	/// <summary>
	/// This API is an extension to XNA.
	/// Gets mouse state information that includes position and button
	/// presses for the provided window
	/// </summary>
	/// <returns>Current state of the mouse.</returns>
	public static MouseState GetState(GameWindow window)
	{
		return Mouse.PlatformGetState(window);
	}

	/// <summary>
	/// Gets mouse state information that includes position and button presses
	/// for the primary window
	/// </summary>
	/// <returns>Current state of the mouse.</returns>
	public static MouseState GetState()
	{
		if (Mouse.PrimaryWindow != null)
		{
			return Mouse.GetState(Mouse.PrimaryWindow);
		}
		return Mouse._defaultState;
	}

	/// <summary>
	/// Sets mouse cursor's relative position to game-window.
	/// </summary>
	/// <param name="x">Relative horizontal position of the cursor.</param>
	/// <param name="y">Relative vertical position of the cursor.</param>
	public static void SetPosition(int x, int y)
	{
		Mouse.PlatformSetPosition(x, y);
	}

	/// <summary>
	/// Sets the cursor image to the specified MouseCursor.
	/// </summary>
	/// <param name="cursor">Mouse cursor to use for the cursor image.</param>
	public static void SetCursor(MouseCursor cursor)
	{
		Mouse.PlatformSetCursor(cursor);
	}

	private static IntPtr PlatformGetWindowHandle()
	{
		return Mouse.PrimaryWindow.Handle;
	}

	private static void PlatformSetWindowHandle(IntPtr windowHandle)
	{
	}

	private static MouseState PlatformGetState(GameWindow window)
	{
		int num = Sdl.Window.GetWindowFlags(window.Handle);
		int x;
		int y;
		Sdl.Mouse.Button state = Sdl.Mouse.GetGlobalState(out x, out y);
		if ((num & 0x400) != 0)
		{
			window.MouseState.LeftButton = (((state & Sdl.Mouse.Button.Left) != 0) ? ButtonState.Pressed : ButtonState.Released);
			window.MouseState.MiddleButton = (((state & Sdl.Mouse.Button.Middle) != 0) ? ButtonState.Pressed : ButtonState.Released);
			window.MouseState.RightButton = (((state & Sdl.Mouse.Button.Right) != 0) ? ButtonState.Pressed : ButtonState.Released);
			window.MouseState.XButton1 = (((state & Sdl.Mouse.Button.X1Mask) != 0) ? ButtonState.Pressed : ButtonState.Released);
			window.MouseState.XButton2 = (((state & Sdl.Mouse.Button.X2Mask) != 0) ? ButtonState.Pressed : ButtonState.Released);
			window.MouseState.HorizontalScrollWheelValue = Mouse.ScrollX;
			window.MouseState.ScrollWheelValue = Mouse.ScrollY;
		}
		Rectangle clientBounds = window.ClientBounds;
		window.MouseState.X = x - clientBounds.X;
		window.MouseState.Y = y - clientBounds.Y;
		return window.MouseState;
	}

	private static void PlatformSetPosition(int x, int y)
	{
		Mouse.PrimaryWindow.MouseState.X = x;
		Mouse.PrimaryWindow.MouseState.Y = y;
		Sdl.Mouse.WarpInWindow(Mouse.PrimaryWindow.Handle, x, y);
	}

	private static void PlatformSetCursor(MouseCursor cursor)
	{
		Sdl.Mouse.SetCursor(cursor.Handle);
	}
}
