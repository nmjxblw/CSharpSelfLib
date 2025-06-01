using System;
using System.IO;
using System.Runtime.InteropServices;

namespace StbImageWriteSharp;

internal class ImageWriter
{
	private Stream _stream;

	private byte[] _buffer = new byte[1024];

	private unsafe int WriteCallback(void* context, void* data, int size)
	{
		if (data == null || size <= 0)
		{
			return 0;
		}
		if (this._buffer.Length < size)
		{
			this._buffer = new byte[size * 2];
		}
		Marshal.Copy(new IntPtr(data), this._buffer, 0, size);
		this._stream.Write(this._buffer, 0, size);
		return size;
	}

	private void CheckParams(byte[] data, int width, int height, ColorComponents components)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		int requiredDataSize = width * height * (int)components;
		if (data.Length < requiredDataSize)
		{
			throw new ArgumentException($"Not enough data. 'data' variable should contain at least {requiredDataSize} bytes.");
		}
	}

	public unsafe void WriteBmp(void* data, int width, int height, ColorComponents components, Stream dest)
	{
		try
		{
			this._stream = dest;
			StbImageWrite.stbi_write_bmp_to_func(WriteCallback, null, width, height, (int)components, data);
		}
		finally
		{
			this._stream = null;
		}
	}

	public unsafe void WriteBmp(byte[] data, int width, int height, ColorComponents components, Stream dest)
	{
		this.CheckParams(data, width, height, components);
		fixed (byte* b = &data[0])
		{
			this.WriteBmp(b, width, height, components, dest);
		}
	}

	public unsafe void WriteTga(void* data, int width, int height, ColorComponents components, Stream dest)
	{
		try
		{
			this._stream = dest;
			StbImageWrite.stbi_write_tga_to_func(WriteCallback, null, width, height, (int)components, data);
		}
		finally
		{
			this._stream = null;
		}
	}

	public unsafe void WriteTga(byte[] data, int width, int height, ColorComponents components, Stream dest)
	{
		this.CheckParams(data, width, height, components);
		fixed (byte* b = &data[0])
		{
			this.WriteTga(b, width, height, components, dest);
		}
	}

	public unsafe void WriteHdr(byte[] data, int width, int height, ColorComponents components, Stream dest)
	{
		this.CheckParams(data, width, height, components);
		try
		{
			this._stream = dest;
			float[] f = new float[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				f[i] = (float)(int)data[i] / 255f;
			}
			fixed (float* fptr = f)
			{
				StbImageWrite.stbi_write_hdr_to_func(WriteCallback, null, width, height, (int)components, fptr);
			}
		}
		finally
		{
			this._stream = null;
		}
	}

	public unsafe void WritePng(void* data, int width, int height, ColorComponents components, Stream dest)
	{
		try
		{
			this._stream = dest;
			StbImageWrite.stbi_write_png_to_func(WriteCallback, null, width, height, (int)components, data, width * (int)components);
		}
		finally
		{
			this._stream = null;
		}
	}

	public unsafe void WritePng(byte[] data, int width, int height, ColorComponents components, Stream dest)
	{
		this.CheckParams(data, width, height, components);
		fixed (byte* ptr = &data[0])
		{
			this.WritePng(data, width, height, components, dest);
		}
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="data"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <param name="components"></param>
	/// <param name="dest"></param>
	/// <param name="quality">Should be from 1 to 100</param>
	public unsafe void WriteJpg(void* data, int width, int height, ColorComponents components, Stream dest, int quality)
	{
		try
		{
			this._stream = dest;
			StbImageWrite.stbi_write_jpg_to_func(WriteCallback, null, width, height, (int)components, data, quality);
		}
		finally
		{
			this._stream = null;
		}
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="data"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <param name="components"></param>
	/// <param name="dest"></param>
	/// <param name="quality">Should be from 1 to 100</param>
	public unsafe void WriteJpg(byte[] data, int width, int height, ColorComponents components, Stream dest, int quality)
	{
		this.CheckParams(data, width, height, components);
		fixed (byte* b = &data[0])
		{
			this.WriteJpg(b, width, height, components, dest, quality);
		}
	}
}
