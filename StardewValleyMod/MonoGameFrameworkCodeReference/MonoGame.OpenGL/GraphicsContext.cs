using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.OpenGL;

internal class GraphicsContext : IGraphicsContext, IDisposable
{
	private IntPtr _context;

	private IntPtr _winHandle;

	private bool _disposed;

	public int SwapInterval
	{
		get
		{
			return Sdl.GL.GetSwapInterval();
		}
		set
		{
			Sdl.GL.SetSwapInterval(value);
		}
	}

	public bool IsDisposed => this._disposed;

	public bool IsCurrent => true;

	public GraphicsContext(IWindowInfo info)
	{
		if (this._disposed)
		{
			return;
		}
		this.SetWindowHandle(info);
		this._context = Sdl.GL.CreateContext(this._winHandle);
		try
		{
			GL.LoadEntryPoints();
		}
		catch (EntryPointNotFoundException)
		{
			throw new PlatformNotSupportedException("MonoGame requires OpenGL 3.0 compatible drivers, or either ARB_framebuffer_object or EXT_framebuffer_object extensions. Try updating your graphics drivers.");
		}
	}

	public void MakeCurrent(IWindowInfo info)
	{
		if (!this._disposed)
		{
			this.SetWindowHandle(info);
			Sdl.GL.MakeCurrent(this._winHandle, this._context);
		}
	}

	public void SwapBuffers()
	{
		if (!this._disposed)
		{
			Sdl.GL.SwapWindow(this._winHandle);
		}
	}

	public void Dispose()
	{
		if (!this._disposed)
		{
			GraphicsDevice.DisposeContext(this._context);
			this._context = IntPtr.Zero;
			this._disposed = true;
		}
	}

	private void SetWindowHandle(IWindowInfo info)
	{
		if (info == null)
		{
			this._winHandle = IntPtr.Zero;
		}
		else
		{
			this._winHandle = info.Handle;
		}
	}
}
