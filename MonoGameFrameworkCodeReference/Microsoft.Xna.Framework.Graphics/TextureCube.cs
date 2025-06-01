using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

public class TextureCube : Texture
{
	internal int size;

	/// <summary>
	/// Gets the width and height of the cube map face in pixels.
	/// </summary>
	/// <value>The width and height of a cube map face in pixels.</value>
	public int Size => this.size;

	public TextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format)
		: this(graphicsDevice, size, mipMap, format, renderTarget: false)
	{
	}

	internal TextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice", "The GraphicsDevice must not be null when creating new resources.");
		}
		if (size <= 0)
		{
			throw new ArgumentOutOfRangeException("size", "Cube size must be greater than zero");
		}
		base.GraphicsDevice = graphicsDevice;
		this.size = size;
		base._format = format;
		base._levelCount = ((!mipMap) ? 1 : Texture.CalculateMipLevels(size));
		this.PlatformConstruct(graphicsDevice, size, mipMap, format, renderTarget);
	}

	/// <summary>
	/// Gets a copy of cube texture data specifying a cubemap face.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="cubeMapFace">The cube map face.</param>
	/// <param name="data">The data.</param>
	public void GetData<T>(CubeMapFace cubeMapFace, T[] data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		this.GetData(cubeMapFace, 0, null, data, 0, data.Length);
	}

	public void GetData<T>(CubeMapFace cubeMapFace, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.GetData(cubeMapFace, 0, null, data, startIndex, elementCount);
	}

	public void GetData<T>(CubeMapFace cubeMapFace, int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(level, rect, data, startIndex, elementCount, out var checkedRect);
		this.PlatformGetData(cubeMapFace, level, checkedRect, data, startIndex, elementCount);
	}

	public void SetData<T>(CubeMapFace face, T[] data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		this.SetData(face, 0, null, data, 0, data.Length);
	}

	public void SetData<T>(CubeMapFace face, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.SetData(face, 0, null, data, startIndex, elementCount);
	}

	public void SetData<T>(CubeMapFace face, int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(level, rect, data, startIndex, elementCount, out var checkedRect);
		this.PlatformSetData(face, level, checkedRect, data, startIndex, elementCount);
	}

	private void ValidateParams<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount, out Rectangle checkedRect) where T : struct
	{
		Rectangle textureBounds = new Rectangle(0, 0, Math.Max(this.Size >> level, 1), Math.Max(this.Size >> level, 1));
		checkedRect = rect ?? textureBounds;
		if (level < 0 || level >= base.LevelCount)
		{
			throw new ArgumentException("level must be smaller than the number of levels in this texture.");
		}
		if (!textureBounds.Contains(checkedRect) || checkedRect.Width <= 0 || checkedRect.Height <= 0)
		{
			throw new ArgumentException("Rectangle must be inside the texture bounds", "rect");
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
		int dataByteSize;
		if (base.Format.IsCompressedFormat())
		{
			int roundedWidth = (checkedRect.Width + 3) & -4;
			int roundedHeight = (checkedRect.Height + 3) & -4;
			checkedRect = new Rectangle(checkedRect.X & -4, checkedRect.Y & -4, (checkedRect.Width < 4 && textureBounds.Width < 4) ? textureBounds.Width : roundedWidth, (checkedRect.Height < 4 && textureBounds.Height < 4) ? textureBounds.Height : roundedHeight);
			dataByteSize = roundedWidth * roundedHeight * fSize / 16;
		}
		else
		{
			dataByteSize = checkedRect.Width * checkedRect.Height * fSize;
		}
		if (elementCount * tSize != dataByteSize)
		{
			throw new ArgumentException($"elementCount is not the right size, elementCount * sizeof(T) is {elementCount * tSize}, but data size is {dataByteSize}.", "elementCount");
		}
	}

	private void PlatformConstruct(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
	{
		base.glTarget = TextureTarget.TextureCubeMap;
		Threading.BlockOnUIThread(delegate
		{
			GL.GenTextures(1, out base.glTexture);
			GL.BindTexture(TextureTarget.TextureCubeMap, base.glTexture);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, mipMap ? 9987 : 9729);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, 9729);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, 33071);
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, 33071);
			format.GetGLFormat(base.GraphicsDevice, out base.glInternalFormat, out base.glFormat, out base.glType);
			for (int i = 0; i < 6; i++)
			{
				TextureTarget gLCubeFace = this.GetGLCubeFace((CubeMapFace)i);
				if (base.glFormat == PixelFormat.CompressedTextureFormats)
				{
					int num = 0;
					switch (format)
					{
					case SurfaceFormat.RgbPvrtc2Bpp:
					case SurfaceFormat.RgbaPvrtc2Bpp:
						num = (Math.Max(size, 16) * Math.Max(size, 8) * 2 + 7) / 8;
						break;
					case SurfaceFormat.RgbPvrtc4Bpp:
					case SurfaceFormat.RgbaPvrtc4Bpp:
						num = (Math.Max(size, 8) * Math.Max(size, 8) * 4 + 7) / 8;
						break;
					case SurfaceFormat.Dxt1:
					case SurfaceFormat.Dxt3:
					case SurfaceFormat.Dxt5:
					case SurfaceFormat.Dxt1SRgb:
					case SurfaceFormat.Dxt3SRgb:
					case SurfaceFormat.Dxt5SRgb:
					case SurfaceFormat.RgbEtc1:
					case SurfaceFormat.Dxt1a:
					case SurfaceFormat.RgbaAtcExplicitAlpha:
					case SurfaceFormat.RgbaAtcInterpolatedAlpha:
					case SurfaceFormat.Rgb8Etc2:
					case SurfaceFormat.Srgb8Etc2:
					case SurfaceFormat.Rgb8A1Etc2:
					case SurfaceFormat.Srgb8A1Etc2:
					case SurfaceFormat.Rgba8Etc2:
					case SurfaceFormat.SRgb8A8Etc2:
						num = (size + 3) / 4 * ((size + 3) / 4) * format.GetSize();
						break;
					default:
						throw new NotSupportedException();
					}
					GL.CompressedTexImage2D(gLCubeFace, 0, base.glInternalFormat, size, size, 0, num, IntPtr.Zero);
				}
				else
				{
					GL.TexImage2D(gLCubeFace, 0, base.glInternalFormat, size, size, 0, base.glFormat, base.glType, IntPtr.Zero);
				}
			}
			if (mipMap)
			{
				GraphicsDevice.FramebufferHelper.Get().GenerateMipmap((int)base.glTarget);
				GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, 1);
			}
		});
	}

	private void PlatformGetData<T>(CubeMapFace cubeMapFace, int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		Threading.EnsureUIThread();
		TextureTarget target = this.GetGLCubeFace(cubeMapFace);
		int tSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
		GL.BindTexture(TextureTarget.TextureCubeMap, base.glTexture);
		if (base.glFormat == PixelFormat.CompressedTextureFormats)
		{
			int pixelToT = base.Format.GetSize() / tSizeInByte;
			int tFullWidth = Math.Max(this.size >> level, 1) / 4 * pixelToT;
			T[] temp = new T[Math.Max(this.size >> level, 1) / 4 * tFullWidth];
			GL.GetCompressedTexImage(target, level, temp);
			int rowCount = rect.Height / 4;
			int tRectWidth = rect.Width / 4 * base.Format.GetSize() / tSizeInByte;
			for (int r = 0; r < rowCount; r++)
			{
				int tempStart = rect.X / 4 * pixelToT + (rect.Top / 4 + r) * tFullWidth;
				int dataStart = startIndex + r * tRectWidth;
				Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
			}
		}
		else
		{
			int tFullWidth2 = Math.Max(this.size >> level, 1) * base.Format.GetSize() / tSizeInByte;
			T[] temp2 = new T[Math.Max(this.size >> level, 1) * tFullWidth2];
			GL.GetTexImage(target, level, base.glFormat, base.glType, temp2);
			int pixelToT2 = base.Format.GetSize() / tSizeInByte;
			int rowCount2 = rect.Height;
			int tRectWidth2 = rect.Width * pixelToT2;
			for (int i = 0; i < rowCount2; i++)
			{
				int tempStart2 = rect.X * pixelToT2 + (i + rect.Top) * tFullWidth2;
				int dataStart2 = startIndex + i * tRectWidth2;
				Array.Copy(temp2, tempStart2, data, dataStart2, tRectWidth2);
			}
		}
	}

	private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount)
	{
		Threading.BlockOnUIThread(delegate
		{
			int num = ReflectionHelpers.SizeOf<T>.Get();
			GCHandle gCHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				int num2 = startIndex * num;
				IntPtr data2 = new IntPtr(gCHandle.AddrOfPinnedObject().ToInt64() + num2);
				GL.BindTexture(TextureTarget.TextureCubeMap, base.glTexture);
				TextureTarget gLCubeFace = this.GetGLCubeFace(face);
				if (base.glFormat == PixelFormat.CompressedTextureFormats)
				{
					GL.CompressedTexSubImage2D(gLCubeFace, level, rect.X, rect.Y, rect.Width, rect.Height, base.glInternalFormat, elementCount * num, data2);
				}
				else
				{
					GL.TexSubImage2D(gLCubeFace, level, rect.X, rect.Y, rect.Width, rect.Height, base.glFormat, base.glType, data2);
				}
			}
			finally
			{
				gCHandle.Free();
			}
		});
	}

	private TextureTarget GetGLCubeFace(CubeMapFace face)
	{
		return face switch
		{
			CubeMapFace.PositiveX => TextureTarget.TextureCubeMapPositiveX, 
			CubeMapFace.NegativeX => TextureTarget.TextureCubeMapNegativeX, 
			CubeMapFace.PositiveY => TextureTarget.TextureCubeMapPositiveY, 
			CubeMapFace.NegativeY => TextureTarget.TextureCubeMapNegativeY, 
			CubeMapFace.PositiveZ => TextureTarget.TextureCubeMapPositiveZ, 
			CubeMapFace.NegativeZ => TextureTarget.TextureCubeMapNegativeZ, 
			_ => throw new ArgumentException(), 
		};
	}
}
