using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class Texture2DReader : ContentTypeReader<Texture2D>
{
	protected internal override Texture2D Read(ContentReader reader, Texture2D existingInstance)
	{
		Texture2D texture = null;
		SurfaceFormat surfaceFormat = (SurfaceFormat)reader.ReadInt32();
		uint packedWidth = reader.ReadUInt32();
		uint packedHeight = reader.ReadUInt32();
		int contentWidth;
		int width;
		if ((packedWidth & 0xFFFF0000u) == 0)
		{
			contentWidth = (width = (int)packedWidth);
		}
		else
		{
			contentWidth = (int)((packedWidth & 0xFFFF0000u) >> 16);
			width = (int)(packedWidth & 0xFFFF);
		}
		int contentHeight;
		int height;
		if ((packedHeight & 0xFFFF0000u) == 0)
		{
			contentHeight = (height = (int)packedHeight);
		}
		else
		{
			contentHeight = (int)((packedHeight & 0xFFFF0000u) >> 16);
			height = (int)(packedHeight & 0xFFFF);
		}
		int levelCount = reader.ReadInt32();
		int levelCountOutput = levelCount;
		if (levelCount > 1 && !reader.GetGraphicsDevice().GraphicsCapabilities.SupportsNonPowerOfTwo && (!MathHelper.IsPowerOfTwo(width) || !MathHelper.IsPowerOfTwo(height)))
		{
			levelCountOutput = 1;
		}
		SurfaceFormat convertedFormat = surfaceFormat;
		switch (surfaceFormat)
		{
		case SurfaceFormat.Dxt1:
		case SurfaceFormat.Dxt1a:
			if (!reader.GetGraphicsDevice().GraphicsCapabilities.SupportsDxt1)
			{
				convertedFormat = SurfaceFormat.Color;
			}
			break;
		case SurfaceFormat.Dxt1SRgb:
			if (!reader.GetGraphicsDevice().GraphicsCapabilities.SupportsDxt1)
			{
				convertedFormat = SurfaceFormat.ColorSRgb;
			}
			break;
		case SurfaceFormat.Dxt3:
		case SurfaceFormat.Dxt5:
			if (!reader.GetGraphicsDevice().GraphicsCapabilities.SupportsS3tc)
			{
				convertedFormat = SurfaceFormat.Color;
			}
			break;
		case SurfaceFormat.Dxt3SRgb:
		case SurfaceFormat.Dxt5SRgb:
			if (!reader.GetGraphicsDevice().GraphicsCapabilities.SupportsS3tc)
			{
				convertedFormat = SurfaceFormat.ColorSRgb;
			}
			break;
		case SurfaceFormat.NormalizedByte4:
			convertedFormat = SurfaceFormat.Color;
			break;
		}
		texture = existingInstance ?? new Texture2D(reader.GetGraphicsDevice(), width, height, levelCountOutput > 1, convertedFormat);
		if (contentWidth != width || contentHeight != height)
		{
			texture.SetImageSize(contentWidth, contentHeight);
		}
		Threading.BlockOnUIThread(delegate
		{
			for (int i = 0; i < levelCount; i++)
			{
				int num = reader.ReadInt32();
				byte[] array = ContentManager.ScratchBufferPool.Get(num);
				reader.Read(array, 0, num);
				int num2 = Math.Max(width >> i, 1);
				int num3 = Math.Max(height >> i, 1);
				if (i < levelCountOutput)
				{
					switch (surfaceFormat)
					{
					case SurfaceFormat.Dxt1:
					case SurfaceFormat.Dxt1SRgb:
					case SurfaceFormat.Dxt1a:
						if (!reader.GetGraphicsDevice().GraphicsCapabilities.SupportsDxt1 && convertedFormat == SurfaceFormat.Color)
						{
							array = DxtUtil.DecompressDxt1(array, num2, num3);
							num = array.Length;
						}
						break;
					case SurfaceFormat.Dxt3:
					case SurfaceFormat.Dxt3SRgb:
						if (!reader.GetGraphicsDevice().GraphicsCapabilities.SupportsS3tc && !reader.GetGraphicsDevice().GraphicsCapabilities.SupportsS3tc && convertedFormat == SurfaceFormat.Color)
						{
							array = DxtUtil.DecompressDxt3(array, num2, num3);
							num = array.Length;
						}
						break;
					case SurfaceFormat.Dxt5:
					case SurfaceFormat.Dxt5SRgb:
						if (!reader.GetGraphicsDevice().GraphicsCapabilities.SupportsS3tc && !reader.GetGraphicsDevice().GraphicsCapabilities.SupportsS3tc && convertedFormat == SurfaceFormat.Color)
						{
							array = DxtUtil.DecompressDxt5(array, num2, num3);
							num = array.Length;
						}
						break;
					case SurfaceFormat.Bgra5551:
					{
						int num8 = 0;
						for (int n = 0; n < num3; n++)
						{
							for (int num9 = 0; num9 < num2; num9++)
							{
								ushort num10 = BitConverter.ToUInt16(array, num8);
								num10 = (ushort)(((num10 & 0x7FFF) << 1) | ((num10 & 0x8000) >> 15));
								array[num8] = (byte)num10;
								array[num8 + 1] = (byte)(num10 >> 8);
								num8 += 2;
							}
						}
						break;
					}
					case SurfaceFormat.Bgra4444:
					{
						int num6 = 0;
						for (int l = 0; l < num3; l++)
						{
							for (int m = 0; m < num2; m++)
							{
								ushort num7 = BitConverter.ToUInt16(array, num6);
								num7 = (ushort)(((num7 & 0xFFF) << 4) | ((num7 & 0xF000) >> 12));
								array[num6] = (byte)num7;
								array[num6 + 1] = (byte)(num7 >> 8);
								num6 += 2;
							}
						}
						break;
					}
					case SurfaceFormat.NormalizedByte4:
					{
						int size = surfaceFormat.GetSize();
						int num4 = num2 * size;
						for (int j = 0; j < num3; j++)
						{
							for (int k = 0; k < num2; k++)
							{
								int num5 = BitConverter.ToInt32(array, j * num4 + k * size);
								array[j * num4 + k * 4] = (byte)((num5 >> 16) & 0xFF);
								array[j * num4 + k * 4 + 1] = (byte)((num5 >> 8) & 0xFF);
								array[j * num4 + k * 4 + 2] = (byte)(num5 & 0xFF);
								array[j * num4 + k * 4 + 3] = (byte)((num5 >> 24) & 0xFF);
							}
						}
						break;
					}
					}
					texture.SetData(i, null, array, 0, num);
					ContentManager.ScratchBufferPool.Return(array);
				}
			}
		});
		return texture;
	}
}
