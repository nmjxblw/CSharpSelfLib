using System;
using System.Threading;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public abstract class Texture : GraphicsResource
{
	internal SurfaceFormat _format;

	internal int _levelCount;

	private readonly int _sortingKey = Interlocked.Increment(ref Texture._lastSortingKey);

	private static int _lastSortingKey;

	internal int glTexture = -1;

	internal TextureTarget glTarget;

	internal TextureUnit glTextureUnit = TextureUnit.Texture0;

	internal PixelInternalFormat glInternalFormat;

	internal PixelFormat glFormat;

	internal PixelType glType;

	internal SamplerState glLastSamplerState;

	/// <summary>
	/// Gets a unique identifier of this texture for sorting purposes.
	/// </summary>
	/// <remarks>
	/// <para>For example, this value is used by <see cref="T:Microsoft.Xna.Framework.Graphics.SpriteBatch" /> when drawing with <see cref="F:Microsoft.Xna.Framework.Graphics.SpriteSortMode.Texture" />.</para>
	/// <para>The value is an implementation detail and may change between application launches or MonoGame versions.
	/// It is only guaranteed to stay consistent during application lifetime.</para>
	/// </remarks>
	internal int SortingKey => this._sortingKey;

	public SurfaceFormat Format => this._format;

	public int LevelCount => this._levelCount;

	internal static int CalculateMipLevels(int width, int height = 0, int depth = 0)
	{
		int levels = 1;
		int size = Math.Max(Math.Max(width, height), depth);
		while (size > 1)
		{
			size /= 2;
			levels++;
		}
		return levels;
	}

	internal static void GetSizeForLevel(int width, int height, int level, out int w, out int h)
	{
		w = width;
		h = height;
		while (level > 0)
		{
			level--;
			w /= 2;
			h /= 2;
		}
		if (w == 0)
		{
			w = 1;
		}
		if (h == 0)
		{
			h = 1;
		}
	}

	internal static void GetSizeForLevel(int width, int height, int depth, int level, out int w, out int h, out int d)
	{
		w = width;
		h = height;
		d = depth;
		while (level > 0)
		{
			level--;
			w /= 2;
			h /= 2;
			d /= 2;
		}
		if (w == 0)
		{
			w = 1;
		}
		if (h == 0)
		{
			h = 1;
		}
		if (d == 0)
		{
			d = 1;
		}
	}

	internal int GetPitch(int width)
	{
		switch (this._format)
		{
		case SurfaceFormat.Dxt1:
		case SurfaceFormat.Dxt3:
		case SurfaceFormat.Dxt5:
		case SurfaceFormat.Dxt1SRgb:
		case SurfaceFormat.Dxt3SRgb:
		case SurfaceFormat.Dxt5SRgb:
		case SurfaceFormat.RgbPvrtc2Bpp:
		case SurfaceFormat.RgbPvrtc4Bpp:
		case SurfaceFormat.RgbaPvrtc2Bpp:
		case SurfaceFormat.RgbaPvrtc4Bpp:
		case SurfaceFormat.RgbEtc1:
		case SurfaceFormat.Dxt1a:
		case SurfaceFormat.Rgb8Etc2:
		case SurfaceFormat.Srgb8Etc2:
		case SurfaceFormat.Rgb8A1Etc2:
		case SurfaceFormat.Srgb8A1Etc2:
			return (width + 3) / 4 * this._format.GetSize();
		default:
			return width * this._format.GetSize();
		}
	}

	protected internal override void GraphicsDeviceResetting()
	{
		this.PlatformGraphicsDeviceResetting();
	}

	private void PlatformGraphicsDeviceResetting()
	{
		this.DeleteGLTexture();
		this.glLastSamplerState = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed)
		{
			this.DeleteGLTexture();
			this.glLastSamplerState = null;
		}
		base.Dispose(disposing);
	}

	protected void DeleteGLTexture()
	{
		if (this.glTexture > 0)
		{
			base.GraphicsDevice.DisposeTexture(this.glTexture);
		}
		this.glTexture = -1;
	}
}
