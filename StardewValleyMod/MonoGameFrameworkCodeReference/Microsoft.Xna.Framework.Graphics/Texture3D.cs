using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class Texture3D : Texture
{
	private int _width;

	private int _height;

	private int _depth;

	public int Width => this._width;

	public int Height => this._height;

	public int Depth => this._depth;

	public Texture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
		: this(graphicsDevice, width, height, depth, mipMap, format, renderTarget: false)
	{
	}

	protected Texture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
		}
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException("width", "Texture width must be greater than zero");
		}
		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException("height", "Texture height must be greater than zero");
		}
		if (depth <= 0)
		{
			throw new ArgumentOutOfRangeException("depth", "Texture depth must be greater than zero");
		}
		base.GraphicsDevice = graphicsDevice;
		this._width = width;
		this._height = height;
		this._depth = depth;
		base._levelCount = 1;
		base._format = format;
		this.PlatformConstruct(graphicsDevice, width, height, depth, mipMap, format, renderTarget);
	}

	public void SetData<T>(T[] data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		this.SetData(data, 0, data.Length);
	}

	public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.SetData(0, 0, 0, this.Width, this.Height, 0, this.Depth, data, startIndex, elementCount);
	}

	public void SetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
		int width = right - left;
		int height = bottom - top;
		int depth = back - front;
		this.PlatformSetData(level, left, top, right, bottom, front, back, data, startIndex, elementCount, width, height, depth);
	}

	/// <summary>
	/// Gets a copy of 3D texture data, specifying a mipmap level, source box, start index, and number of elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the array.</typeparam>
	/// <param name="level">Mipmap level.</param>
	/// <param name="left">Position of the left side of the box on the x-axis.</param>
	/// <param name="top">Position of the top of the box on the y-axis.</param>
	/// <param name="right">Position of the right side of the box on the x-axis.</param>
	/// <param name="bottom">Position of the bottom of the box on the y-axis.</param>
	/// <param name="front">Position of the front of the box on the z-axis.</param>
	/// <param name="back">Position of the back of the box on the z-axis.</param>
	/// <param name="data">Array of data.</param>
	/// <param name="startIndex">Index of the first element to get.</param>
	/// <param name="elementCount">Number of elements to get.</param>
	public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
		this.PlatformGetData(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
	}

	/// <summary>
	/// Gets a copy of 3D texture data, specifying a start index and number of elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the array.</typeparam>
	/// <param name="data">Array of data.</param>
	/// <param name="startIndex">Index of the first element to get.</param>
	/// <param name="elementCount">Number of elements to get.</param>
	public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.GetData(0, 0, 0, this._width, this._height, 0, this._depth, data, startIndex, elementCount);
	}

	/// <summary>
	/// Gets a copy of 3D texture data.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the array.</typeparam>
	/// <param name="data">Array of data.</param>
	public void GetData<T>(T[] data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		this.GetData(data, 0, data.Length);
	}

	private void ValidateParams<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
	{
		int texWidth = Math.Max(this.Width >> level, 1);
		int texHeight = Math.Max(this.Height >> level, 1);
		int texDepth = Math.Max(this.Depth >> level, 1);
		int num = right - left;
		int height = bottom - top;
		int depth = back - front;
		if (left < 0 || top < 0 || back < 0 || right > texWidth || bottom > texHeight || front > texDepth)
		{
			throw new ArgumentException("Area must remain inside texture bounds");
		}
		if (left >= right || top >= bottom || front >= back)
		{
			throw new ArgumentException("Neither box size nor box position can be negative");
		}
		if (level < 0 || level >= base.LevelCount)
		{
			throw new ArgumentException("level must be smaller than the number of levels in this texture.");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int tSize = ReflectionHelpers.SizeOf<T>.Get();
		int fSize = base.Format.GetSize();
		if (tSize > fSize || fSize % tSize != 0)
		{
			throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
		}
		if (startIndex < 0 || startIndex >= data.Length)
		{
			throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
		}
		if (data.Length < startIndex + elementCount)
		{
			throw new ArgumentException("The data array is too small.");
		}
		int dataByteSize = num * height * depth * fSize;
		if (elementCount * tSize != dataByteSize)
		{
			throw new ArgumentException($"elementCount is not the right size, elementCount * sizeof(T) is {elementCount * tSize}, but data size is {dataByteSize}.", "elementCount");
		}
	}

	private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
	{
		base.glTarget = TextureTarget.Texture3D;
		Threading.BlockOnUIThread(delegate
		{
			GL.GenTextures(1, out base.glTexture);
			GL.BindTexture(base.glTarget, base.glTexture);
			format.GetGLFormat(base.GraphicsDevice, out base.glInternalFormat, out base.glFormat, out base.glType);
			GL.TexImage3D(base.glTarget, 0, base.glInternalFormat, width, height, depth, 0, base.glFormat, base.glType, IntPtr.Zero);
		});
		if (mipMap)
		{
			throw new NotImplementedException("Texture3D does not yet support mipmaps.");
		}
	}

	private void PlatformSetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount, int width, int height, int depth)
	{
		Threading.BlockOnUIThread(delegate
		{
			int num = Marshal.SizeOf<T>();
			GCHandle gCHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				IntPtr data2 = (IntPtr)(gCHandle.AddrOfPinnedObject().ToInt64() + startIndex * num);
				GL.BindTexture(base.glTarget, base.glTexture);
				GL.TexSubImage3D(base.glTarget, level, left, top, front, width, height, depth, base.glFormat, base.glType, data2);
			}
			finally
			{
				gCHandle.Free();
			}
		});
	}

	private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
	{
		throw new NotImplementedException();
	}
}
