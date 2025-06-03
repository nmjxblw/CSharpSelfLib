using System;
using System.Diagnostics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics;

internal static class GraphicsExtensions
{
	private const SurfaceFormat InvalidFormat = (SurfaceFormat)2147483647;

	public static int OpenGLNumberOfElements(this VertexElementFormat elementFormat)
	{
		return elementFormat switch
		{
			VertexElementFormat.Single => 1, 
			VertexElementFormat.Vector2 => 2, 
			VertexElementFormat.Vector3 => 3, 
			VertexElementFormat.Vector4 => 4, 
			VertexElementFormat.Color => 4, 
			VertexElementFormat.Byte4 => 4, 
			VertexElementFormat.Short2 => 2, 
			VertexElementFormat.Short4 => 4, 
			VertexElementFormat.NormalizedShort2 => 2, 
			VertexElementFormat.NormalizedShort4 => 4, 
			VertexElementFormat.HalfVector2 => 2, 
			VertexElementFormat.HalfVector4 => 4, 
			_ => throw new ArgumentException(), 
		};
	}

	public static VertexPointerType OpenGLVertexPointerType(this VertexElementFormat elementFormat)
	{
		return elementFormat switch
		{
			VertexElementFormat.Single => VertexPointerType.Float, 
			VertexElementFormat.Vector2 => VertexPointerType.Float, 
			VertexElementFormat.Vector3 => VertexPointerType.Float, 
			VertexElementFormat.Vector4 => VertexPointerType.Float, 
			VertexElementFormat.Color => VertexPointerType.Short, 
			VertexElementFormat.Byte4 => VertexPointerType.Short, 
			VertexElementFormat.Short2 => VertexPointerType.Short, 
			VertexElementFormat.Short4 => VertexPointerType.Short, 
			VertexElementFormat.NormalizedShort2 => VertexPointerType.Short, 
			VertexElementFormat.NormalizedShort4 => VertexPointerType.Short, 
			VertexElementFormat.HalfVector2 => VertexPointerType.Float, 
			VertexElementFormat.HalfVector4 => VertexPointerType.Float, 
			_ => throw new ArgumentException(), 
		};
	}

	public static VertexAttribPointerType OpenGLVertexAttribPointerType(this VertexElementFormat elementFormat)
	{
		return elementFormat switch
		{
			VertexElementFormat.Single => VertexAttribPointerType.Float, 
			VertexElementFormat.Vector2 => VertexAttribPointerType.Float, 
			VertexElementFormat.Vector3 => VertexAttribPointerType.Float, 
			VertexElementFormat.Vector4 => VertexAttribPointerType.Float, 
			VertexElementFormat.Color => VertexAttribPointerType.UnsignedByte, 
			VertexElementFormat.Byte4 => VertexAttribPointerType.UnsignedByte, 
			VertexElementFormat.Short2 => VertexAttribPointerType.Short, 
			VertexElementFormat.Short4 => VertexAttribPointerType.Short, 
			VertexElementFormat.NormalizedShort2 => VertexAttribPointerType.Short, 
			VertexElementFormat.NormalizedShort4 => VertexAttribPointerType.Short, 
			VertexElementFormat.HalfVector2 => VertexAttribPointerType.HalfFloat, 
			VertexElementFormat.HalfVector4 => VertexAttribPointerType.HalfFloat, 
			_ => throw new ArgumentException(), 
		};
	}

	public static bool OpenGLVertexAttribNormalized(this VertexElement element)
	{
		if (element.VertexElementUsage == VertexElementUsage.Color)
		{
			return true;
		}
		VertexElementFormat vertexElementFormat = element.VertexElementFormat;
		if ((uint)(vertexElementFormat - 8) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static ColorPointerType OpenGLColorPointerType(this VertexElementFormat elementFormat)
	{
		return elementFormat switch
		{
			VertexElementFormat.Single => ColorPointerType.Float, 
			VertexElementFormat.Vector2 => ColorPointerType.Float, 
			VertexElementFormat.Vector3 => ColorPointerType.Float, 
			VertexElementFormat.Vector4 => ColorPointerType.Float, 
			VertexElementFormat.Color => ColorPointerType.UnsignedByte, 
			VertexElementFormat.Byte4 => ColorPointerType.UnsignedByte, 
			VertexElementFormat.Short2 => ColorPointerType.Short, 
			VertexElementFormat.Short4 => ColorPointerType.Short, 
			VertexElementFormat.NormalizedShort2 => ColorPointerType.UnsignedShort, 
			VertexElementFormat.NormalizedShort4 => ColorPointerType.UnsignedShort, 
			_ => throw new ArgumentException(), 
		};
	}

	public static NormalPointerType OpenGLNormalPointerType(this VertexElementFormat elementFormat)
	{
		return elementFormat switch
		{
			VertexElementFormat.Single => NormalPointerType.Float, 
			VertexElementFormat.Vector2 => NormalPointerType.Float, 
			VertexElementFormat.Vector3 => NormalPointerType.Float, 
			VertexElementFormat.Vector4 => NormalPointerType.Float, 
			VertexElementFormat.Color => NormalPointerType.Byte, 
			VertexElementFormat.Byte4 => NormalPointerType.Byte, 
			VertexElementFormat.Short2 => NormalPointerType.Short, 
			VertexElementFormat.Short4 => NormalPointerType.Short, 
			VertexElementFormat.NormalizedShort2 => NormalPointerType.Short, 
			VertexElementFormat.NormalizedShort4 => NormalPointerType.Short, 
			_ => throw new ArgumentException(), 
		};
	}

	public static TexCoordPointerType OpenGLTexCoordPointerType(this VertexElementFormat elementFormat)
	{
		return elementFormat switch
		{
			VertexElementFormat.Single => TexCoordPointerType.Float, 
			VertexElementFormat.Vector2 => TexCoordPointerType.Float, 
			VertexElementFormat.Vector3 => TexCoordPointerType.Float, 
			VertexElementFormat.Vector4 => TexCoordPointerType.Float, 
			VertexElementFormat.Color => TexCoordPointerType.Float, 
			VertexElementFormat.Byte4 => TexCoordPointerType.Float, 
			VertexElementFormat.Short2 => TexCoordPointerType.Short, 
			VertexElementFormat.Short4 => TexCoordPointerType.Short, 
			VertexElementFormat.NormalizedShort2 => TexCoordPointerType.Short, 
			VertexElementFormat.NormalizedShort4 => TexCoordPointerType.Short, 
			_ => throw new ArgumentException(), 
		};
	}

	public static BlendEquationMode GetBlendEquationMode(this BlendFunction function)
	{
		return function switch
		{
			BlendFunction.Add => BlendEquationMode.FuncAdd, 
			BlendFunction.Max => BlendEquationMode.Max, 
			BlendFunction.Min => BlendEquationMode.Min, 
			BlendFunction.ReverseSubtract => BlendEquationMode.FuncReverseSubtract, 
			BlendFunction.Subtract => BlendEquationMode.FuncSubtract, 
			_ => throw new ArgumentException(), 
		};
	}

	public static BlendingFactorSrc GetBlendFactorSrc(this Blend blend)
	{
		return blend switch
		{
			Blend.BlendFactor => BlendingFactorSrc.ConstantColor, 
			Blend.DestinationAlpha => BlendingFactorSrc.DstAlpha, 
			Blend.DestinationColor => BlendingFactorSrc.DstColor, 
			Blend.InverseBlendFactor => BlendingFactorSrc.OneMinusConstantColor, 
			Blend.InverseDestinationAlpha => BlendingFactorSrc.OneMinusDstAlpha, 
			Blend.InverseDestinationColor => BlendingFactorSrc.OneMinusDstColor, 
			Blend.InverseSourceAlpha => BlendingFactorSrc.OneMinusSrcAlpha, 
			Blend.InverseSourceColor => BlendingFactorSrc.OneMinusSrcColor, 
			Blend.One => BlendingFactorSrc.One, 
			Blend.SourceAlpha => BlendingFactorSrc.SrcAlpha, 
			Blend.SourceAlphaSaturation => BlendingFactorSrc.SrcAlphaSaturate, 
			Blend.SourceColor => BlendingFactorSrc.SrcColor, 
			Blend.Zero => BlendingFactorSrc.Zero, 
			_ => throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented."), 
		};
	}

	public static BlendingFactorDest GetBlendFactorDest(this Blend blend)
	{
		return blend switch
		{
			Blend.BlendFactor => BlendingFactorDest.ConstantColor, 
			Blend.DestinationAlpha => BlendingFactorDest.DstAlpha, 
			Blend.DestinationColor => BlendingFactorDest.DstColor, 
			Blend.InverseBlendFactor => BlendingFactorDest.OneMinusConstantColor, 
			Blend.InverseDestinationAlpha => BlendingFactorDest.OneMinusDstAlpha, 
			Blend.InverseDestinationColor => BlendingFactorDest.OneMinusDstColor, 
			Blend.InverseSourceAlpha => BlendingFactorDest.OneMinusSrcAlpha, 
			Blend.InverseSourceColor => BlendingFactorDest.OneMinusSrcColor, 
			Blend.One => BlendingFactorDest.One, 
			Blend.SourceAlpha => BlendingFactorDest.SrcAlpha, 
			Blend.SourceAlphaSaturation => BlendingFactorDest.SrcAlphaSaturate, 
			Blend.SourceColor => BlendingFactorDest.SrcColor, 
			Blend.Zero => BlendingFactorDest.Zero, 
			_ => throw new ArgumentOutOfRangeException("blend", "The specified blend function is not implemented."), 
		};
	}

	public static DepthFunction GetDepthFunction(this CompareFunction compare)
	{
		return compare switch
		{
			CompareFunction.Equal => DepthFunction.Equal, 
			CompareFunction.Greater => DepthFunction.Greater, 
			CompareFunction.GreaterEqual => DepthFunction.Gequal, 
			CompareFunction.Less => DepthFunction.Less, 
			CompareFunction.LessEqual => DepthFunction.Lequal, 
			CompareFunction.Never => DepthFunction.Never, 
			CompareFunction.NotEqual => DepthFunction.Notequal, 
			_ => DepthFunction.Always, 
		};
	}

	/// <summary>
	/// Convert a <see cref="T:Microsoft.Xna.Framework.Graphics.SurfaceFormat" /> to an OpenTK.Graphics.ColorFormat.
	/// This is used for setting up the backbuffer format of the OpenGL context.
	/// </summary>
	/// <returns>An OpenTK.Graphics.ColorFormat instance.</returns>
	/// <param name="format">The <see cref="T:Microsoft.Xna.Framework.Graphics.SurfaceFormat" /> to convert.</param>
	internal static ColorFormat GetColorFormat(this SurfaceFormat format)
	{
		switch (format)
		{
		case SurfaceFormat.Alpha8:
			return new ColorFormat(0, 0, 0, 8);
		case SurfaceFormat.Bgr565:
			return new ColorFormat(5, 6, 5, 0);
		case SurfaceFormat.Bgra4444:
			return new ColorFormat(4, 4, 4, 4);
		case SurfaceFormat.Bgra5551:
			return new ColorFormat(5, 5, 5, 1);
		case SurfaceFormat.Bgr32:
			return new ColorFormat(8, 8, 8, 0);
		case SurfaceFormat.Color:
		case SurfaceFormat.Bgra32:
		case SurfaceFormat.ColorSRgb:
			return new ColorFormat(8, 8, 8, 8);
		case SurfaceFormat.Rgba1010102:
			return new ColorFormat(10, 10, 10, 2);
		default:
			throw new NotSupportedException();
		}
	}

	/// <summary>
	/// Converts <see cref="T:Microsoft.Xna.Framework.Graphics.PresentInterval" /> to OpenGL swap interval.
	/// </summary>
	/// <returns>A value according to EXT_swap_control</returns>
	/// <param name="interval">The <see cref="T:Microsoft.Xna.Framework.Graphics.PresentInterval" /> to convert.</param>
	internal static int GetSwapInterval(this PresentInterval interval)
	{
		return interval switch
		{
			PresentInterval.Immediate => 0, 
			PresentInterval.One => 1, 
			PresentInterval.Two => 2, 
			_ => -1, 
		};
	}

	internal static void GetGLFormat(this SurfaceFormat format, GraphicsDevice graphicsDevice, out PixelInternalFormat glInternalFormat, out PixelFormat glFormat, out PixelType glType)
	{
		glInternalFormat = PixelInternalFormat.Rgba;
		glFormat = PixelFormat.Rgba;
		glType = PixelType.UnsignedByte;
		bool supportsSRgb = graphicsDevice.GraphicsCapabilities.SupportsSRgb;
		bool supportsS3tc = graphicsDevice.GraphicsCapabilities.SupportsS3tc;
		bool supportsPvrtc = graphicsDevice.GraphicsCapabilities.SupportsPvrtc;
		bool supportsEtc1 = graphicsDevice.GraphicsCapabilities.SupportsEtc1;
		bool supportsEtc2 = graphicsDevice.GraphicsCapabilities.SupportsEtc2;
		bool supportsAtitc = graphicsDevice.GraphicsCapabilities.SupportsAtitc;
		bool supportsFloat = graphicsDevice.GraphicsCapabilities.SupportsFloatTextures;
		bool supportsHalfFloat = graphicsDevice.GraphicsCapabilities.SupportsHalfFloatTextures;
		bool supportsNormalized = graphicsDevice.GraphicsCapabilities.SupportsNormalized;
		bool isGLES2 = GL.BoundApi == GL.RenderApi.ES && graphicsDevice.glMajorVersion == 2;
		switch (format)
		{
		case SurfaceFormat.Color:
			glInternalFormat = PixelInternalFormat.Rgba;
			glFormat = PixelFormat.Rgba;
			glType = PixelType.UnsignedByte;
			return;
		case SurfaceFormat.ColorSRgb:
			if (supportsSRgb)
			{
				glInternalFormat = PixelInternalFormat.Srgb;
				glFormat = PixelFormat.Rgba;
				glType = PixelType.UnsignedByte;
				return;
			}
			goto case SurfaceFormat.Color;
		case SurfaceFormat.Bgr565:
			glInternalFormat = PixelInternalFormat.Rgb;
			glFormat = PixelFormat.Rgb;
			glType = PixelType.UnsignedShort565;
			return;
		case SurfaceFormat.Bgra4444:
			glInternalFormat = PixelInternalFormat.Rgba4;
			glFormat = PixelFormat.Rgba;
			glType = PixelType.UnsignedShort4444;
			return;
		case SurfaceFormat.Bgra5551:
			glInternalFormat = PixelInternalFormat.Rgba;
			glFormat = PixelFormat.Rgba;
			glType = PixelType.UnsignedShort5551;
			return;
		case SurfaceFormat.Alpha8:
			glInternalFormat = PixelInternalFormat.Luminance;
			glFormat = PixelFormat.Luminance;
			glType = PixelType.UnsignedByte;
			return;
		case SurfaceFormat.Dxt1:
			if (supportsS3tc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Dxt1SRgb:
			if (supportsSRgb)
			{
				glInternalFormat = PixelInternalFormat.CompressedSrgbS3tcDxt1Ext;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			goto case SurfaceFormat.Dxt1;
		case SurfaceFormat.Dxt1a:
			if (supportsS3tc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Dxt3:
			if (supportsS3tc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Dxt3SRgb:
			if (supportsSRgb)
			{
				glInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			goto case SurfaceFormat.Dxt3;
		case SurfaceFormat.Dxt5:
			if (supportsS3tc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Dxt5SRgb:
			if (supportsSRgb)
			{
				glInternalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			goto case SurfaceFormat.Dxt5;
		case SurfaceFormat.Rgba1010102:
			glInternalFormat = PixelInternalFormat.Rgb10A2ui;
			glFormat = PixelFormat.Rgba;
			glType = PixelType.UnsignedInt1010102;
			return;
		case SurfaceFormat.Single:
			if (supportsFloat)
			{
				glInternalFormat = PixelInternalFormat.R32f;
				glFormat = PixelFormat.Red;
				glType = PixelType.Float;
				return;
			}
			break;
		case SurfaceFormat.HalfVector2:
			if (supportsHalfFloat)
			{
				glInternalFormat = PixelInternalFormat.Rg16f;
				glFormat = PixelFormat.Rg;
				glType = PixelType.HalfFloat;
				return;
			}
			break;
		case SurfaceFormat.HalfVector4:
		case SurfaceFormat.HdrBlendable:
			if (supportsHalfFloat)
			{
				glInternalFormat = PixelInternalFormat.Rgba16f;
				glFormat = PixelFormat.Rgba;
				glType = PixelType.HalfFloat;
				return;
			}
			break;
		case SurfaceFormat.HalfSingle:
			if (supportsHalfFloat)
			{
				glInternalFormat = PixelInternalFormat.R16f;
				glFormat = PixelFormat.Red;
				glType = (isGLES2 ? PixelType.HalfFloatOES : PixelType.HalfFloat);
				return;
			}
			break;
		case SurfaceFormat.Vector2:
			if (supportsFloat)
			{
				glInternalFormat = PixelInternalFormat.Rg32f;
				glFormat = PixelFormat.Rg;
				glType = PixelType.Float;
				return;
			}
			break;
		case SurfaceFormat.Vector4:
			if (supportsFloat)
			{
				glInternalFormat = PixelInternalFormat.Rgba32f;
				glFormat = PixelFormat.Rgba;
				glType = PixelType.Float;
				return;
			}
			break;
		case SurfaceFormat.NormalizedByte2:
			glInternalFormat = PixelInternalFormat.Rg8i;
			glFormat = PixelFormat.Rg;
			glType = PixelType.Byte;
			return;
		case SurfaceFormat.NormalizedByte4:
			glInternalFormat = PixelInternalFormat.Rgba8i;
			glFormat = PixelFormat.Rgba;
			glType = PixelType.Byte;
			return;
		case SurfaceFormat.Rg32:
			if (supportsNormalized)
			{
				glInternalFormat = PixelInternalFormat.Rg16ui;
				glFormat = PixelFormat.Rg;
				glType = PixelType.UnsignedShort;
				return;
			}
			break;
		case SurfaceFormat.Rgba64:
			if (supportsNormalized)
			{
				glInternalFormat = PixelInternalFormat.Rgba16;
				glFormat = PixelFormat.Rgba;
				glType = PixelType.UnsignedShort;
				return;
			}
			break;
		case SurfaceFormat.RgbaAtcExplicitAlpha:
			if (supportsAtitc)
			{
				glInternalFormat = PixelInternalFormat.AtcRgbaExplicitAlphaAmd;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.RgbaAtcInterpolatedAlpha:
			if (supportsAtitc)
			{
				glInternalFormat = PixelInternalFormat.AtcRgbaInterpolatedAlphaAmd;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.RgbEtc1:
			if (supportsEtc1)
			{
				glInternalFormat = PixelInternalFormat.Etc1;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Rgb8Etc2:
			if (supportsEtc2)
			{
				glInternalFormat = PixelInternalFormat.Etc2Rgb8;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Srgb8Etc2:
			if (supportsEtc2)
			{
				glInternalFormat = PixelInternalFormat.Etc2Srgb8;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Rgb8A1Etc2:
			if (supportsEtc2)
			{
				glInternalFormat = PixelInternalFormat.Etc2Rgb8A1;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Srgb8A1Etc2:
			if (supportsEtc2)
			{
				glInternalFormat = PixelInternalFormat.Etc2Srgb8A1;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.Rgba8Etc2:
			if (supportsEtc2)
			{
				glInternalFormat = PixelInternalFormat.Etc2Rgba8Eac;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.SRgb8A8Etc2:
			if (supportsEtc2)
			{
				glInternalFormat = PixelInternalFormat.Etc2SRgb8A8Eac;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.RgbPvrtc2Bpp:
			if (supportsPvrtc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc2Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.RgbPvrtc4Bpp:
			if (supportsPvrtc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbPvrtc4Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.RgbaPvrtc2Bpp:
			if (supportsPvrtc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc2Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		case SurfaceFormat.RgbaPvrtc4Bpp:
			if (supportsPvrtc)
			{
				glInternalFormat = PixelInternalFormat.CompressedRgbaPvrtc4Bppv1Img;
				glFormat = PixelFormat.CompressedTextureFormats;
				return;
			}
			break;
		}
		throw new NotSupportedException($"The requested SurfaceFormat `{format}` is not supported.");
	}

	public static int GetSyncInterval(this PresentInterval interval)
	{
		return interval switch
		{
			PresentInterval.Immediate => 0, 
			PresentInterval.Two => 2, 
			_ => 1, 
		};
	}

	public static bool IsCompressedFormat(this SurfaceFormat format)
	{
		switch (format)
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
		case SurfaceFormat.RgbaAtcExplicitAlpha:
		case SurfaceFormat.RgbaAtcInterpolatedAlpha:
		case SurfaceFormat.Rgb8Etc2:
		case SurfaceFormat.Srgb8Etc2:
		case SurfaceFormat.Rgb8A1Etc2:
		case SurfaceFormat.Srgb8A1Etc2:
		case SurfaceFormat.Rgba8Etc2:
		case SurfaceFormat.SRgb8A8Etc2:
			return true;
		default:
			return false;
		}
	}

	public static int GetSize(this SurfaceFormat surfaceFormat)
	{
		switch (surfaceFormat)
		{
		case SurfaceFormat.Dxt1:
		case SurfaceFormat.Dxt1SRgb:
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
			return 8;
		case SurfaceFormat.Dxt3:
		case SurfaceFormat.Dxt5:
		case SurfaceFormat.Dxt3SRgb:
		case SurfaceFormat.Dxt5SRgb:
		case SurfaceFormat.RgbaAtcExplicitAlpha:
		case SurfaceFormat.RgbaAtcInterpolatedAlpha:
		case SurfaceFormat.Rgba8Etc2:
		case SurfaceFormat.SRgb8A8Etc2:
			return 16;
		case SurfaceFormat.Alpha8:
			return 1;
		case SurfaceFormat.Bgr565:
		case SurfaceFormat.Bgra5551:
		case SurfaceFormat.Bgra4444:
		case SurfaceFormat.NormalizedByte2:
		case SurfaceFormat.HalfSingle:
			return 2;
		case SurfaceFormat.Color:
		case SurfaceFormat.NormalizedByte4:
		case SurfaceFormat.Rgba1010102:
		case SurfaceFormat.Rg32:
		case SurfaceFormat.Single:
		case SurfaceFormat.HalfVector2:
		case SurfaceFormat.Bgr32:
		case SurfaceFormat.Bgra32:
		case SurfaceFormat.ColorSRgb:
		case SurfaceFormat.Bgr32SRgb:
		case SurfaceFormat.Bgra32SRgb:
			return 4;
		case SurfaceFormat.Rgba64:
		case SurfaceFormat.Vector2:
		case SurfaceFormat.HalfVector4:
			return 8;
		case SurfaceFormat.Vector4:
			return 16;
		default:
			throw new ArgumentException();
		}
	}

	public static int GetSize(this VertexElementFormat elementFormat)
	{
		return elementFormat switch
		{
			VertexElementFormat.Single => 4, 
			VertexElementFormat.Vector2 => 8, 
			VertexElementFormat.Vector3 => 12, 
			VertexElementFormat.Vector4 => 16, 
			VertexElementFormat.Color => 4, 
			VertexElementFormat.Byte4 => 4, 
			VertexElementFormat.Short2 => 4, 
			VertexElementFormat.Short4 => 8, 
			VertexElementFormat.NormalizedShort2 => 4, 
			VertexElementFormat.NormalizedShort4 => 8, 
			VertexElementFormat.HalfVector2 => 4, 
			VertexElementFormat.HalfVector4 => 8, 
			_ => 0, 
		};
	}

	public static void GetBlockSize(this SurfaceFormat surfaceFormat, out int width, out int height)
	{
		switch (surfaceFormat)
		{
		case SurfaceFormat.RgbPvrtc2Bpp:
		case SurfaceFormat.RgbaPvrtc2Bpp:
			width = 8;
			height = 4;
			break;
		case SurfaceFormat.Dxt1:
		case SurfaceFormat.Dxt3:
		case SurfaceFormat.Dxt5:
		case SurfaceFormat.Dxt1SRgb:
		case SurfaceFormat.Dxt3SRgb:
		case SurfaceFormat.Dxt5SRgb:
		case SurfaceFormat.RgbPvrtc4Bpp:
		case SurfaceFormat.RgbaPvrtc4Bpp:
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
			width = 4;
			height = 4;
			break;
		default:
			width = 1;
			height = 1;
			break;
		}
	}

	public static int GetBoundTexture2D()
	{
		int prevTexture = 0;
		GL.GetInteger(GetPName.TextureBinding2D, out prevTexture);
		return prevTexture;
	}

	[Conditional("DEBUG")]
	[DebuggerHidden]
	public static void CheckGLError()
	{
		ErrorCode error = GL.GetError();
		if (error != ErrorCode.NoError)
		{
			throw new MonoGameGLException("GL.GetError() returned " + error);
		}
	}

	[Conditional("DEBUG")]
	public static void LogGLError(string location)
	{
		try
		{
		}
		catch (MonoGameGLException)
		{
		}
	}
}
