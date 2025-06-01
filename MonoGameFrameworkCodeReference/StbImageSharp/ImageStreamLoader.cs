using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace StbImageSharp;

internal class ImageStreamLoader
{
	private Stream _stream;

	private byte[] _buffer = new byte[1024];

	private readonly StbImage.stbi_io_callbacks _callbacks;

	public unsafe ImageStreamLoader()
	{
		this._callbacks = new StbImage.stbi_io_callbacks
		{
			read = ReadCallback,
			skip = SkipCallback,
			eof = Eof
		};
	}

	private unsafe int SkipCallback(void* user, int i)
	{
		return (int)this._stream.Seek(i, SeekOrigin.Current);
	}

	private unsafe int Eof(void* user)
	{
		return this._stream.CanRead ? 1 : 0;
	}

	private unsafe int ReadCallback(void* user, sbyte* data, int size)
	{
		if (size > this._buffer.Length)
		{
			this._buffer = new byte[size * 2];
		}
		int result = this._stream.Read(this._buffer, 0, size);
		Marshal.Copy(this._buffer, 0, new IntPtr(data), size);
		return result;
	}

	public unsafe ImageResult Load(Stream stream, ColorComponents requiredComponents = ColorComponents.Default)
	{
		byte* result = null;
		this._stream = stream;
		try
		{
			int x = default(int);
			int y = default(int);
			int comp = default(int);
			result = StbImage.stbi_load_from_callbacks(this._callbacks, null, &x, &y, &comp, (int)requiredComponents);
			return ImageResult.FromResult(result, x, y, (ColorComponents)comp, requiredComponents);
		}
		finally
		{
			if (result != null)
			{
				CRuntime.free(result);
			}
			this._stream = null;
		}
	}

	public unsafe AnimatedFrameResult[] ReadAnimatedGif(Stream stream, ColorComponents requiredComponents = ColorComponents.Default)
	{
		try
		{
			this._stream = stream;
			List<AnimatedFrameResult> res = new List<AnimatedFrameResult>();
			StbImage.stbi__context context = new StbImage.stbi__context();
			StbImage.stbi__start_callbacks(context, this._callbacks, null);
			if (StbImage.stbi__gif_test(context) == 0)
			{
				throw new Exception("Input stream is not GIF file.");
			}
			StbImage.stbi__gif g = new StbImage.stbi__gif();
			int comp = default(int);
			while (true)
			{
				byte* result = StbImage.stbi__gif_load_next(context, g, &comp, (int)requiredComponents, null);
				if (result == null)
				{
					break;
				}
				AnimatedFrameResult frame = new AnimatedFrameResult
				{
					Width = g.w,
					Height = g.h,
					SourceComp = (ColorComponents)comp,
					Comp = ((requiredComponents == ColorComponents.Default) ? ((ColorComponents)comp) : requiredComponents),
					Delay = g.delay
				};
				frame.Data = new byte[g.w * g.h * (int)frame.Comp];
				Marshal.Copy(new IntPtr(result), frame.Data, 0, frame.Data.Length);
				CRuntime.free(result);
				res.Add(frame);
			}
			CRuntime.free(g._out_);
			return res.ToArray();
		}
		finally
		{
			this._stream = null;
		}
	}
}
