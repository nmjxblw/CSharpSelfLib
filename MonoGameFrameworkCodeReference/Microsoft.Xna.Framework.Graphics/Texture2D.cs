using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;
using StbImageSharp;
using StbImageWriteSharp;

namespace Microsoft.Xna.Framework.Graphics;

public class Texture2D : Texture
{
	protected internal enum SurfaceType
	{
		Texture,
		RenderTarget,
		SwapChainRenderTarget
	}

	private enum ImageWriterFormat
	{
		Jpg,
		Png
	}

	private struct SetDataState<T> where T : struct
	{
		public Texture2D texture;

		public int level;

		public T[] data;

		public int startIndex;

		public int elementCount;

		internal static Action<SetDataState<T>> Action = delegate(SetDataState<T> s)
		{
			s.texture.PlatformSetDataBody(s.level, s.data, s.startIndex, s.elementCount);
		};
	}

	private struct SetDataRectState<T> where T : struct
	{
		public Texture2D texture;

		public int level;

		public int arraySlice;

		public Rectangle rect;

		public T[] data;

		public int startIndex;

		public int elementCount;

		public static Action<SetDataRectState<T>> Action = delegate(SetDataRectState<T> s)
		{
			s.texture.PlatformSetDataBody(s.level, s.arraySlice, s.rect, s.data, s.startIndex, s.elementCount);
		};
	}

	internal int width;

	internal int height;

	internal int ArraySize;

	internal float TexelWidth { get; private set; }

	internal float TexelHeight { get; private set; }

	/// <summary>
	/// Gets the dimensions of the texture
	/// </summary>
	public Rectangle Bounds => new Rectangle(0, 0, this.width, this.height);

	/// <summary>
	/// Gets the width of the texture image data in pixels.
	/// </summary>
	public int Width => this.width;

	/// <summary>
	/// Gets the height of the texture image data in pixels.
	/// </summary>
	public int Height => this.height;

	/// <summary>
	/// Gets the width of the allocated texture data in pixels.
	/// </summary>
	public int ActualWidth { get; private set; }

	/// <summary>
	/// Gets the height of the allocated texture data in pixels.
	/// </summary>
	public int ActualHeight { get; private set; }

	/// <summary>
	/// Creates a new texture of the given size
	/// </summary>
	/// <param name="graphicsDevice"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
		: this(graphicsDevice, width, height, mipmap: false, SurfaceFormat.Color, SurfaceType.Texture, shared: false, 1)
	{
	}

	/// <summary>
	/// Creates a new texture of a given size with a surface format and optional mipmaps 
	/// </summary>
	/// <param name="graphicsDevice"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <param name="mipmap"></param>
	/// <param name="format"></param>
	public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
		: this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, shared: false, 1)
	{
	}

	/// <summary>
	/// Creates a new texture array of a given size with a surface format and optional mipmaps.
	/// Throws ArgumentException if the current GraphicsDevice can't work with texture arrays
	/// </summary>
	/// <param name="graphicsDevice"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <param name="mipmap"></param>
	/// <param name="format"></param>
	/// <param name="arraySize"></param>
	public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize)
		: this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, shared: false, arraySize)
	{
	}

	/// <summary>
	///  Creates a new texture of a given size with a surface format and optional mipmaps.
	/// </summary>
	/// <param name="graphicsDevice"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	/// <param name="mipmap"></param>
	/// <param name="format"></param>
	/// <param name="type"></param>
	internal Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type)
		: this(graphicsDevice, width, height, mipmap, format, type, shared: false, 1)
	{
	}

	protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize)
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
		if (arraySize > 1 && !graphicsDevice.GraphicsCapabilities.SupportsTextureArrays)
		{
			throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySize");
		}
		base.GraphicsDevice = graphicsDevice;
		this.width = (this.ActualWidth = width);
		this.height = (this.ActualHeight = height);
		this.TexelWidth = 1f / (float)width;
		this.TexelHeight = 1f / (float)height;
		base._format = format;
		base._levelCount = ((!mipmap) ? 1 : Texture.CalculateMipLevels(width, height));
		this.ArraySize = arraySize;
		if (type != SurfaceType.SwapChainRenderTarget)
		{
			this.PlatformConstruct(width, height, mipmap, format, type, shared);
		}
	}

	/// <summary>
	/// Gets the image data size of the texture which could
	/// be smaller than the allocated texture data size.
	/// </summary>
	public void SetImageSize(int width, int height)
	{
		this.ActualWidth = this.width;
		this.ActualHeight = this.height;
		this.width = width;
		this.height = height;
	}

	/// <summary>
	/// Changes the pixels of the texture
	/// Throws ArgumentNullException if data is null
	/// Throws ArgumentException if arraySlice is greater than 0, and the GraphicsDevice does not support texture arrays
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="level">Layer of the texture to modify</param>
	/// <param name="arraySlice">Index inside the texture array</param>
	/// <param name="rect">Area to modify</param>
	/// <param name="data">New data for the texture</param>
	/// <param name="startIndex">Start position of data</param>
	/// <param name="elementCount"></param>
	public void SetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(level, arraySlice, rect, data, startIndex, elementCount, out var checkedRect);
		this.PlatformSetData(level, arraySlice, checkedRect, data, startIndex, elementCount);
	}

	/// <summary>
	/// Changes the pixels of the texture
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="level">Layer of the texture to modify</param>
	/// <param name="rect">Area to modify</param>
	/// <param name="data">New data for the texture</param>
	/// <param name="startIndex">Start position of data</param>
	/// <param name="elementCount"></param>
	public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(level, 0, rect, data, startIndex, elementCount, out var checkedRect);
		if (rect.HasValue)
		{
			this.PlatformSetData(level, 0, checkedRect, data, startIndex, elementCount);
		}
		else
		{
			this.PlatformSetData(level, data, startIndex, elementCount);
		}
	}

	/// <summary>
	/// Changes the texture's pixels
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data">New data for the texture</param>
	/// <param name="startIndex">Start position of data</param>
	/// <param name="elementCount"></param>
	public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(0, 0, null, data, startIndex, elementCount, out var _);
		this.PlatformSetData(0, data, startIndex, elementCount);
	}

	/// <summary>
	/// Changes the texture's pixels
	/// </summary>
	/// <typeparam name="T">New data for the texture</typeparam>
	/// <param name="data"></param>
	public void SetData<T>(T[] data) where T : struct
	{
		this.ValidateParams(0, 0, null, data, 0, data.Length, out var _);
		this.PlatformSetData(0, data, 0, data.Length);
	}

	/// <summary>
	/// Retrieves the contents of the texture
	/// Throws ArgumentException if data is null, data.length is too short or
	/// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="level">Layer of the texture</param>
	/// <param name="arraySlice">Index inside the texture array</param>
	/// <param name="rect">Area of the texture to retrieve</param>
	/// <param name="data">Destination array for the data</param>
	/// <param name="startIndex">Starting index of data where to write the pixel data</param>
	/// <param name="elementCount">Number of pixels to read</param>
	public void GetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.ValidateParams(level, arraySlice, rect, data, startIndex, elementCount, out var checkedRect);
		this.PlatformGetData(level, arraySlice, checkedRect, data, startIndex, elementCount);
	}

	/// <summary>
	/// Retrieves the contents of the texture
	/// Throws ArgumentException if data is null, data.length is too short or
	/// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="level">Layer of the texture</param>
	/// <param name="rect">Area of the texture</param>
	/// <param name="data">Destination array for the texture data</param>
	/// <param name="startIndex">First position in data where to write the pixel data</param>
	/// <param name="elementCount">Number of pixels to read</param>
	public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		this.GetData(level, 0, rect, data, startIndex, elementCount);
	}

	/// <summary>
	/// Retrieves the contents of the texture
	/// Throws ArgumentException if data is null, data.length is too short or
	/// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data">Destination array for the texture data</param>
	/// <param name="startIndex">First position in data where to write the pixel data</param>
	/// <param name="elementCount">Number of pixels to read</param>
	public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
	{
		this.GetData(0, null, data, startIndex, elementCount);
	}

	/// <summary>
	/// Retrieves the contents of the texture
	/// Throws ArgumentException if data is null, data.length is too short or
	/// if arraySlice is greater than 0 and the GraphicsDevice doesn't support texture arrays
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data">Destination array for the texture data</param>
	public void GetData<T>(T[] data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		this.GetData(0, null, data, 0, data.Length);
	}

	public void CopyFromTexture(Texture2D other)
	{
		Color[] pixel_data = new Color[other.width * other.height];
		other.GetData(pixel_data);
		this.width = other.width;
		this.height = other.height;
		this.ActualWidth = other.ActualWidth;
		this.ActualHeight = other.ActualHeight;
		this.TexelWidth = other.TexelWidth;
		this.TexelHeight = other.TexelHeight;
		this.SetData(pixel_data);
	}

	/// <summary>
	/// Creates a <see cref="T:Microsoft.Xna.Framework.Graphics.Texture2D" /> from a file, supported formats bmp, gif, jpg, png, tif and dds (only for simple textures).
	/// May work with other formats, but will not work with tga files.
	/// This internally calls <see cref="M:Microsoft.Xna.Framework.Graphics.Texture2D.FromStream(Microsoft.Xna.Framework.Graphics.GraphicsDevice,System.IO.Stream)" />.
	/// </summary>
	/// <param name="graphicsDevice">The graphics device to use to create the texture.</param>
	/// <param name="path">The path to the image file.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Graphics.Texture2D" /> created from the given file.</returns>
	/// <remarks>Note that different image decoders may generate slight differences between platforms, but perceptually 
	/// the images should be identical.  This call does not premultiply the image alpha, but areas of zero alpha will
	/// result in black color data.
	/// </remarks>
	public static Texture2D FromFile(GraphicsDevice graphicsDevice, string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		using FileStream stream = File.OpenRead(path);
		return Texture2D.FromStream(graphicsDevice, stream);
	}

	/// <summary>
	/// Creates a <see cref="T:Microsoft.Xna.Framework.Graphics.Texture2D" /> from a stream, supported formats bmp, gif, jpg, png, tif and dds (only for simple textures).
	/// May work with other formats, but will not work with tga files.
	/// </summary>
	/// <param name="graphicsDevice">The graphics device to use to create the texture.</param>
	/// <param name="stream">The stream from which to read the image data.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Graphics.Texture2D" /> created from the image stream.</returns>
	/// <remarks>Note that different image decoders may generate slight differences between platforms, but perceptually 
	/// the images should be identical.  This call does not premultiply the image alpha, but areas of zero alpha will
	/// result in black color data.
	/// </remarks>
	public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
	{
		if (graphicsDevice == null)
		{
			throw new ArgumentNullException("graphicsDevice");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		try
		{
			return Texture2D.PlatformFromStream(graphicsDevice, stream);
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("This image format is not supported", innerException);
		}
	}

	/// <summary>
	/// Converts the texture to a JPG image
	/// </summary>
	/// <param name="stream">Destination for the image</param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	public void SaveAsJpeg(Stream stream, int width, int height)
	{
		this.PlatformSaveAsJpeg(stream, width, height);
	}

	/// <summary>
	/// Converts the texture to a PNG image
	/// </summary>
	/// <param name="stream">Destination for the image</param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	public void SaveAsPng(Stream stream, int width, int height)
	{
		this.PlatformSaveAsPng(stream, width, height);
	}

	public void Reload(Stream textureStream)
	{
		this.PlatformReload(textureStream);
	}

	private static void ConvertToABGR(int pixelHeight, int pixelWidth, int[] pixels)
	{
		int pixelCount = pixelWidth * pixelHeight;
		for (int i = 0; i < pixelCount; i++)
		{
			uint pixel = (uint)pixels[i];
			pixels[i] = (int)((pixel & 0xFF00FF00u) | ((pixel & 0xFF0000) >> 16) | ((pixel & 0xFF) << 16));
		}
	}

	private void ValidateParams<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount, out Rectangle checkedRect) where T : struct
	{
		Rectangle textureBounds = new Rectangle(0, 0, Math.Max(this.ActualWidth >> level, 1), Math.Max(this.ActualHeight >> level, 1));
		checkedRect = rect ?? textureBounds;
		if (level < 0 || level >= base.LevelCount)
		{
			throw new ArgumentException("level must be smaller than the number of levels in this texture.", "level");
		}
		if (arraySlice > 0 && !base.GraphicsDevice.GraphicsCapabilities.SupportsTextureArrays)
		{
			throw new ArgumentException("Texture arrays are not supported on this graphics device", "arraySlice");
		}
		if (arraySlice < 0 || arraySlice >= this.ArraySize)
		{
			throw new ArgumentException("arraySlice must be smaller than the ArraySize of this texture and larger than 0.", "arraySlice");
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
			base.Format.GetBlockSize(out var blockWidth, out var blockHeight);
			int blockWidthMinusOne = blockWidth - 1;
			int blockHeightMinusOne = blockHeight - 1;
			int roundedWidth = (checkedRect.Width + blockWidthMinusOne) & ~blockWidthMinusOne;
			int roundedHeight = (checkedRect.Height + blockHeightMinusOne) & ~blockHeightMinusOne;
			checkedRect = new Rectangle(checkedRect.X & ~blockWidthMinusOne, checkedRect.Y & ~blockHeightMinusOne, (checkedRect.Width < blockWidth && textureBounds.Width < blockWidth) ? textureBounds.Width : roundedWidth, (checkedRect.Height < blockHeight && textureBounds.Height < blockHeight) ? textureBounds.Height : roundedHeight);
			dataByteSize = ((base.Format != SurfaceFormat.RgbPvrtc2Bpp && base.Format != SurfaceFormat.RgbaPvrtc2Bpp) ? ((base.Format != SurfaceFormat.RgbPvrtc4Bpp && base.Format != SurfaceFormat.RgbaPvrtc4Bpp) ? (roundedWidth * roundedHeight * fSize / (blockWidth * blockHeight)) : ((Math.Max(checkedRect.Width, 8) * Math.Max(checkedRect.Height, 8) * 4 + 7) / 8)) : ((Math.Max(checkedRect.Width, 16) * Math.Max(checkedRect.Height, 8) * 2 + 7) / 8));
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

	internal Color[] GetColorData()
	{
		int colorDataLength = this.Width * this.Height;
		Color[] colorData = new Color[colorDataLength];
		switch (base.Format)
		{
		case SurfaceFormat.Single:
		{
			float[] floatData = new float[colorDataLength];
			this.GetData(floatData);
			for (int num5 = 0; num5 < colorDataLength; num5++)
			{
				float brightness = floatData[num5];
				colorData[num5] = new Color(brightness, brightness, brightness);
			}
			break;
		}
		case SurfaceFormat.Color:
			this.GetData(colorData);
			break;
		case SurfaceFormat.Alpha8:
		{
			Alpha8[] alpha8Data = new Alpha8[colorDataLength];
			this.GetData(alpha8Data);
			for (int num = 0; num < colorDataLength; num++)
			{
				colorData[num] = new Color(alpha8Data[num].ToVector4());
			}
			break;
		}
		case SurfaceFormat.Bgr565:
		{
			Bgr565[] bgr565Data = new Bgr565[colorDataLength];
			this.GetData(bgr565Data);
			for (int num7 = 0; num7 < colorDataLength; num7++)
			{
				colorData[num7] = new Color(bgr565Data[num7].ToVector4());
			}
			break;
		}
		case SurfaceFormat.Bgra4444:
		{
			Bgra4444[] bgra4444Data = new Bgra4444[colorDataLength];
			this.GetData(bgra4444Data);
			for (int num3 = 0; num3 < colorDataLength; num3++)
			{
				colorData[num3] = new Color(bgra4444Data[num3].ToVector4());
			}
			break;
		}
		case SurfaceFormat.Bgra5551:
		{
			Bgra5551[] bgra5551Data = new Bgra5551[colorDataLength];
			this.GetData(bgra5551Data);
			for (int m = 0; m < colorDataLength; m++)
			{
				colorData[m] = new Color(bgra5551Data[m].ToVector4());
			}
			break;
		}
		case SurfaceFormat.HalfSingle:
		{
			HalfSingle[] halfSingleData = new HalfSingle[colorDataLength];
			this.GetData(halfSingleData);
			for (int j = 0; j < colorDataLength; j++)
			{
				colorData[j] = new Color(halfSingleData[j].ToVector4());
			}
			break;
		}
		case SurfaceFormat.HalfVector2:
		{
			HalfVector2[] halfVector2Data = new HalfVector2[colorDataLength];
			this.GetData(halfVector2Data);
			for (int num6 = 0; num6 < colorDataLength; num6++)
			{
				colorData[num6] = new Color(halfVector2Data[num6].ToVector4());
			}
			break;
		}
		case SurfaceFormat.HalfVector4:
		{
			HalfVector4[] halfVector4Data = new HalfVector4[colorDataLength];
			this.GetData(halfVector4Data);
			for (int num4 = 0; num4 < colorDataLength; num4++)
			{
				colorData[num4] = new Color(halfVector4Data[num4].ToVector4());
			}
			break;
		}
		case SurfaceFormat.NormalizedByte2:
		{
			NormalizedByte2[] normalizedByte2Data = new NormalizedByte2[colorDataLength];
			this.GetData(normalizedByte2Data);
			for (int num2 = 0; num2 < colorDataLength; num2++)
			{
				colorData[num2] = new Color(normalizedByte2Data[num2].ToVector4());
			}
			break;
		}
		case SurfaceFormat.NormalizedByte4:
		{
			NormalizedByte4[] normalizedByte4Data = new NormalizedByte4[colorDataLength];
			this.GetData(normalizedByte4Data);
			for (int n = 0; n < colorDataLength; n++)
			{
				colorData[n] = new Color(normalizedByte4Data[n].ToVector4());
			}
			break;
		}
		case SurfaceFormat.Rg32:
		{
			Rg32[] rg32Data = new Rg32[colorDataLength];
			this.GetData(rg32Data);
			for (int l = 0; l < colorDataLength; l++)
			{
				colorData[l] = new Color(rg32Data[l].ToVector4());
			}
			break;
		}
		case SurfaceFormat.Rgba64:
		{
			Rgba64[] rgba64Data = new Rgba64[colorDataLength];
			this.GetData(rgba64Data);
			for (int k = 0; k < colorDataLength; k++)
			{
				colorData[k] = new Color(rgba64Data[k].ToVector4());
			}
			break;
		}
		case SurfaceFormat.Rgba1010102:
		{
			Rgba1010102[] rgba1010102Data = new Rgba1010102[colorDataLength];
			this.GetData(rgba1010102Data);
			for (int i = 0; i < colorDataLength; i++)
			{
				colorData[i] = new Color(rgba1010102Data[i].ToVector4());
			}
			break;
		}
		default:
			throw new Exception("Texture surface format not supported");
		}
		return colorData;
	}

	private unsafe static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
	{
		if (stream.CanSeek && stream.Length == stream.Position)
		{
			stream.Seek(0L, SeekOrigin.Begin);
		}
		byte[] bytes;
		using (MemoryStream ms = new MemoryStream())
		{
			stream.CopyTo(ms);
			bytes = ms.ToArray();
		}
		ImageResult result = ImageResult.FromMemory(bytes, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
		fixed (byte* b = &result.Data[0])
		{
			for (int i = 0; i < result.Data.Length; i += 4)
			{
				if (b[i + 3] == 0)
				{
					b[i] = 0;
					b[i + 1] = 0;
					b[i + 2] = 0;
				}
			}
		}
		Texture2D texture2D = new Texture2D(graphicsDevice, result.Width, result.Height);
		texture2D.SetData(result.Data);
		return texture2D;
	}

	private void PlatformSaveAsJpeg(Stream stream, int width, int height)
	{
		this.SaveAsImage(stream, width, height, ImageWriterFormat.Jpg);
	}

	private void PlatformSaveAsPng(Stream stream, int width, int height)
	{
		this.SaveAsImage(stream, width, height, ImageWriterFormat.Png);
	}

	private unsafe void SaveAsImage(Stream stream, int width, int height, ImageWriterFormat format)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream", "'stream' cannot be null (Nothing in Visual Basic)");
		}
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
		}
		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");
		}
		Color[] data = null;
		try
		{
			data = this.GetColorData();
			fixed (Color* ptr = &data[0])
			{
				ImageWriter writer = new ImageWriter();
				switch (format)
				{
				case ImageWriterFormat.Jpg:
					writer.WriteJpg(ptr, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, 90);
					break;
				case ImageWriterFormat.Png:
					writer.WritePng(ptr, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
					break;
				}
			}
		}
		finally
		{
			if (data != null)
			{
				data = null;
			}
		}
	}

	private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
	{
		base.glTarget = TextureTarget.Texture2D;
		format.GetGLFormat(base.GraphicsDevice, out base.glInternalFormat, out base.glFormat, out base.glType);
		Threading.BlockOnUIThread(delegate
		{
			this.GenerateGLTextureIfRequired();
			int num = width;
			int num2 = height;
			int num3 = 0;
			while (true)
			{
				if (base.glFormat == PixelFormat.CompressedTextureFormats)
				{
					int num4 = 0;
					if (format == SurfaceFormat.RgbPvrtc2Bpp || format == SurfaceFormat.RgbaPvrtc2Bpp)
					{
						num4 = (Math.Max(num, 16) * Math.Max(num2, 8) * 2 + 7) / 8;
					}
					else if (format == SurfaceFormat.RgbPvrtc4Bpp || format == SurfaceFormat.RgbaPvrtc4Bpp)
					{
						num4 = (Math.Max(num, 8) * Math.Max(num2, 8) * 4 + 7) / 8;
					}
					else
					{
						int size = format.GetSize();
						format.GetBlockSize(out var num5, out var num6);
						int num7 = (num + (num5 - 1)) / num5;
						int num8 = (num2 + (num6 - 1)) / num6;
						num4 = num7 * num8 * size;
					}
					GL.CompressedTexImage2D(TextureTarget.Texture2D, num3, base.glInternalFormat, num, num2, 0, num4, IntPtr.Zero);
				}
				else
				{
					GL.TexImage2D(TextureTarget.Texture2D, num3, base.glInternalFormat, num, num2, 0, base.glFormat, base.glType, IntPtr.Zero);
				}
				if ((num == 1 && num2 == 1) || !mipmap)
				{
					break;
				}
				if (num > 1)
				{
					num /= 2;
				}
				if (num2 > 1)
				{
					num2 /= 2;
				}
				num3++;
			}
		});
	}

	private void PlatformSetDataBody<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
	{
		Texture.GetSizeForLevel(this.ActualWidth, this.ActualHeight, level, out var w, out var h);
		int elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
		GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
		try
		{
			int startBytes = startIndex * elementSizeInByte;
			IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
			int prevTexture = GraphicsExtensions.GetBoundTexture2D();
			if (prevTexture != base.glTexture)
			{
				GL.BindTexture(TextureTarget.Texture2D, base.glTexture);
			}
			this.GenerateGLTextureIfRequired();
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(base._format.GetSize(), 8));
			if (base.glFormat == PixelFormat.CompressedTextureFormats)
			{
				GL.CompressedTexImage2D(TextureTarget.Texture2D, level, base.glInternalFormat, w, h, 0, elementCount * elementSizeInByte, dataPtr);
			}
			else
			{
				GL.TexImage2D(TextureTarget.Texture2D, level, base.glInternalFormat, w, h, 0, base.glFormat, base.glType, dataPtr);
			}
			GL.Finish();
			if (prevTexture != base.glTexture)
			{
				GL.BindTexture(TextureTarget.Texture2D, prevTexture);
			}
		}
		finally
		{
			dataHandle.Free();
		}
	}

	private void PlatformSetDataBody<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		int elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
		GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
		try
		{
			int startBytes = startIndex * elementSizeInByte;
			IntPtr dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
			int prevTexture = GraphicsExtensions.GetBoundTexture2D();
			if (prevTexture != base.glTexture)
			{
				GL.BindTexture(TextureTarget.Texture2D, base.glTexture);
			}
			this.GenerateGLTextureIfRequired();
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, Math.Min(base._format.GetSize(), 8));
			if (base.glFormat == PixelFormat.CompressedTextureFormats)
			{
				GL.CompressedTexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height, base.glInternalFormat, elementCount * elementSizeInByte, dataPtr);
			}
			else
			{
				GL.TexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height, base.glFormat, base.glType, dataPtr);
			}
			GL.Finish();
			if (prevTexture != base.glTexture)
			{
				GL.BindTexture(TextureTarget.Texture2D, prevTexture);
			}
		}
		finally
		{
			dataHandle.Free();
		}
	}

	private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
	{
		Threading.BlockOnUIThread(SetDataState<T>.Action, new SetDataState<T>
		{
			texture = this,
			level = level,
			data = data,
			startIndex = startIndex,
			elementCount = elementCount
		});
	}

	private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		Threading.BlockOnUIThread(SetDataRectState<T>.Action, new SetDataRectState<T>
		{
			texture = this,
			level = level,
			arraySlice = arraySlice,
			rect = rect,
			data = data,
			startIndex = startIndex,
			elementCount = elementCount
		});
	}

	private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
	{
		Threading.EnsureUIThread();
		int tSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
		GL.BindTexture(TextureTarget.Texture2D, base.glTexture);
		GL.PixelStore(PixelStoreParameter.PackAlignment, Math.Min(tSizeInByte, 8));
		if (base.glFormat == PixelFormat.CompressedTextureFormats)
		{
			int pixelToT = base.Format.GetSize() / tSizeInByte;
			int tFullWidth = Math.Max(this.ActualWidth >> level, 1) / 4 * pixelToT;
			T[] temp = new T[Math.Max(this.ActualHeight >> level, 1) / 4 * tFullWidth];
			GL.GetCompressedTexImage(TextureTarget.Texture2D, level, temp);
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
			int tFullWidth2 = Math.Max(this.ActualWidth >> level, 1) * base.Format.GetSize() / tSizeInByte;
			T[] temp2 = new T[Math.Max(this.ActualHeight >> level, 1) * tFullWidth2];
			GL.GetTexImage(TextureTarget.Texture2D, level, base.glFormat, base.glType, temp2);
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

	private void FillTextureFromStream(Stream stream)
	{
	}

	private void PlatformReload(Stream textureStream)
	{
		int prev = GraphicsExtensions.GetBoundTexture2D();
		this.GenerateGLTextureIfRequired();
		this.FillTextureFromStream(textureStream);
		GL.BindTexture(TextureTarget.Texture2D, prev);
	}

	private void GenerateGLTextureIfRequired()
	{
		if (base.glTexture >= 0)
		{
			return;
		}
		GL.GenTextures(1, out base.glTexture);
		TextureWrapMode wrap = TextureWrapMode.Repeat;
		if ((this.width & (this.width - 1)) != 0 || (this.height & (this.height - 1)) != 0)
		{
			wrap = TextureWrapMode.ClampToEdge;
		}
		GL.BindTexture(TextureTarget.Texture2D, base.glTexture);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (base._levelCount > 1) ? 9987 : 9729);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 9729);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
		if (base.GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
		{
			if (base._levelCount > 0)
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, base._levelCount - 1);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 1000);
			}
		}
	}
}
