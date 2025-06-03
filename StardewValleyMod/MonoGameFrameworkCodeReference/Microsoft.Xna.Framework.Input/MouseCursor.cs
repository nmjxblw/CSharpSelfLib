using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Describes a mouse cursor.
/// </summary>
public class MouseCursor : IDisposable
{
	private bool _disposed;

	/// <summary>
	/// Gets the default arrow cursor.
	/// </summary>
	public static MouseCursor Arrow { get; private set; }

	/// <summary>
	/// Gets the cursor that appears when the mouse is over text editing regions.
	/// </summary>
	public static MouseCursor IBeam { get; private set; }

	/// <summary>
	/// Gets the waiting cursor that appears while the application/system is busy.
	/// </summary>
	public static MouseCursor Wait { get; private set; }

	/// <summary>
	/// Gets the crosshair ("+") cursor.
	/// </summary>
	public static MouseCursor Crosshair { get; private set; }

	/// <summary>
	/// Gets the cross between Arrow and Wait cursors.
	/// </summary>
	public static MouseCursor WaitArrow { get; private set; }

	/// <summary>
	/// Gets the northwest/southeast ("\") cursor.
	/// </summary>
	public static MouseCursor SizeNWSE { get; private set; }

	/// <summary>
	/// Gets the northeast/southwest ("/") cursor.
	/// </summary>
	public static MouseCursor SizeNESW { get; private set; }

	/// <summary>
	/// Gets the horizontal west/east ("-") cursor.
	/// </summary>
	public static MouseCursor SizeWE { get; private set; }

	/// <summary>
	/// Gets the vertical north/south ("|") cursor.
	/// </summary>
	public static MouseCursor SizeNS { get; private set; }

	/// <summary>
	/// Gets the size all cursor which points in all directions.
	/// </summary>
	public static MouseCursor SizeAll { get; private set; }

	/// <summary>
	/// Gets the cursor that points that something is invalid, usually a cross.
	/// </summary>
	public static MouseCursor No { get; private set; }

	/// <summary>
	/// Gets the hand cursor, usually used for web links.
	/// </summary>
	public static MouseCursor Hand { get; private set; }

	public IntPtr Handle { get; private set; }

	/// <summary>
	/// Creates a mouse cursor from the specified texture.
	/// </summary>
	/// <param name="texture">Texture to use as the cursor image.</param>
	/// <param name="originx">X cordinate of the image that will be used for mouse position.</param>
	/// <param name="originy">Y cordinate of the image that will be used for mouse position.</param>
	public static MouseCursor FromTexture2D(Texture2D texture, int originx, int originy)
	{
		if (texture.Format != SurfaceFormat.Color && texture.Format != SurfaceFormat.ColorSRgb)
		{
			throw new ArgumentException("Only Color or ColorSrgb textures are accepted for mouse cursors", "texture");
		}
		return MouseCursor.PlatformFromTexture2D(texture, originx, originy);
	}

	static MouseCursor()
	{
		MouseCursor.PlatformInitalize();
	}

	private MouseCursor(IntPtr handle)
	{
		this.Handle = handle;
	}

	public void Dispose()
	{
		if (!this._disposed)
		{
			this.PlatformDispose();
			this._disposed = true;
		}
	}

	private MouseCursor(Sdl.Mouse.SystemCursor cursor)
	{
		this.Handle = Sdl.Mouse.CreateSystemCursor(cursor);
	}

	private static void PlatformInitalize()
	{
		MouseCursor.Arrow = new MouseCursor(Sdl.Mouse.SystemCursor.Arrow);
		MouseCursor.IBeam = new MouseCursor(Sdl.Mouse.SystemCursor.IBeam);
		MouseCursor.Wait = new MouseCursor(Sdl.Mouse.SystemCursor.Wait);
		MouseCursor.Crosshair = new MouseCursor(Sdl.Mouse.SystemCursor.Crosshair);
		MouseCursor.WaitArrow = new MouseCursor(Sdl.Mouse.SystemCursor.WaitArrow);
		MouseCursor.SizeNWSE = new MouseCursor(Sdl.Mouse.SystemCursor.SizeNWSE);
		MouseCursor.SizeNESW = new MouseCursor(Sdl.Mouse.SystemCursor.SizeNESW);
		MouseCursor.SizeWE = new MouseCursor(Sdl.Mouse.SystemCursor.SizeWE);
		MouseCursor.SizeNS = new MouseCursor(Sdl.Mouse.SystemCursor.SizeNS);
		MouseCursor.SizeAll = new MouseCursor(Sdl.Mouse.SystemCursor.SizeAll);
		MouseCursor.No = new MouseCursor(Sdl.Mouse.SystemCursor.No);
		MouseCursor.Hand = new MouseCursor(Sdl.Mouse.SystemCursor.Hand);
	}

	private static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy)
	{
		IntPtr surface = IntPtr.Zero;
		IntPtr handle = IntPtr.Zero;
		try
		{
			byte[] bytes = new byte[texture.Width * texture.Height * 4];
			texture.GetData(bytes);
			surface = Sdl.CreateRGBSurfaceFrom(bytes, texture.Width, texture.Height, 32, texture.Width * 4, 255u, 65280u, 16711680u, 4278190080u);
			if (surface == IntPtr.Zero)
			{
				throw new InvalidOperationException("Failed to create surface for mouse cursor: " + Sdl.GetError());
			}
			handle = Sdl.Mouse.CreateColorCursor(surface, originx, originy);
			if (handle == IntPtr.Zero)
			{
				throw new InvalidOperationException("Failed to set surface for mouse cursor: " + Sdl.GetError());
			}
		}
		finally
		{
			if (surface != IntPtr.Zero)
			{
				Sdl.FreeSurface(surface);
			}
		}
		return new MouseCursor(handle);
	}

	private void PlatformDispose()
	{
		if (!(this.Handle == IntPtr.Zero))
		{
			Sdl.Mouse.FreeCursor(this.Handle);
			this.Handle = IntPtr.Zero;
		}
	}
}
