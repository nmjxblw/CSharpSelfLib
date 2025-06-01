using System.IO;

namespace Microsoft.Xna.Framework.Graphics;

internal static class DxtUtil
{
	internal static byte[] DecompressDxt1(byte[] imageData, int width, int height)
	{
		using MemoryStream imageStream = new MemoryStream(imageData);
		return DxtUtil.DecompressDxt1(imageStream, width, height);
	}

	internal static byte[] DecompressDxt1(Stream imageStream, int width, int height)
	{
		byte[] imageData = new byte[width * height * 4];
		using BinaryReader imageReader = new BinaryReader(imageStream);
		int blockCountX = (width + 3) / 4;
		int blockCountY = (height + 3) / 4;
		for (int y = 0; y < blockCountY; y++)
		{
			for (int x = 0; x < blockCountX; x++)
			{
				DxtUtil.DecompressDxt1Block(imageReader, x, y, blockCountX, width, height, imageData);
			}
		}
		return imageData;
	}

	private static void DecompressDxt1Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
	{
		ushort c0 = imageReader.ReadUInt16();
		ushort c1 = imageReader.ReadUInt16();
		DxtUtil.ConvertRgb565ToRgb888(c0, out var r0, out var g0, out var b0);
		DxtUtil.ConvertRgb565ToRgb888(c1, out var r1, out var g1, out var b1);
		uint lookupTable = imageReader.ReadUInt32();
		for (int blockY = 0; blockY < 4; blockY++)
		{
			for (int blockX = 0; blockX < 4; blockX++)
			{
				byte r2 = 0;
				byte g2 = 0;
				byte b2 = 0;
				byte a = byte.MaxValue;
				uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 3;
				if (c0 > c1)
				{
					switch (index)
					{
					case 0u:
						r2 = r0;
						g2 = g0;
						b2 = b0;
						break;
					case 1u:
						r2 = r1;
						g2 = g1;
						b2 = b1;
						break;
					case 2u:
						r2 = (byte)((2 * r0 + r1) / 3);
						g2 = (byte)((2 * g0 + g1) / 3);
						b2 = (byte)((2 * b0 + b1) / 3);
						break;
					case 3u:
						r2 = (byte)((r0 + 2 * r1) / 3);
						g2 = (byte)((g0 + 2 * g1) / 3);
						b2 = (byte)((b0 + 2 * b1) / 3);
						break;
					}
				}
				else
				{
					switch (index)
					{
					case 0u:
						r2 = r0;
						g2 = g0;
						b2 = b0;
						break;
					case 1u:
						r2 = r1;
						g2 = g1;
						b2 = b1;
						break;
					case 2u:
						r2 = (byte)((r0 + r1) / 2);
						g2 = (byte)((g0 + g1) / 2);
						b2 = (byte)((b0 + b1) / 2);
						break;
					case 3u:
						r2 = 0;
						g2 = 0;
						b2 = 0;
						a = 0;
						break;
					}
				}
				int px = (x << 2) + blockX;
				int py = (y << 2) + blockY;
				if (px < width && py < height)
				{
					int offset = py * width + px << 2;
					imageData[offset] = r2;
					imageData[offset + 1] = g2;
					imageData[offset + 2] = b2;
					imageData[offset + 3] = a;
				}
			}
		}
	}

	internal static byte[] DecompressDxt3(byte[] imageData, int width, int height)
	{
		using MemoryStream imageStream = new MemoryStream(imageData);
		return DxtUtil.DecompressDxt3(imageStream, width, height);
	}

	internal static byte[] DecompressDxt3(Stream imageStream, int width, int height)
	{
		byte[] imageData = new byte[width * height * 4];
		using BinaryReader imageReader = new BinaryReader(imageStream);
		int blockCountX = (width + 3) / 4;
		int blockCountY = (height + 3) / 4;
		for (int y = 0; y < blockCountY; y++)
		{
			for (int x = 0; x < blockCountX; x++)
			{
				DxtUtil.DecompressDxt3Block(imageReader, x, y, blockCountX, width, height, imageData);
			}
		}
		return imageData;
	}

	private static void DecompressDxt3Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
	{
		byte a0 = imageReader.ReadByte();
		byte a1 = imageReader.ReadByte();
		byte a2 = imageReader.ReadByte();
		byte a3 = imageReader.ReadByte();
		byte a4 = imageReader.ReadByte();
		byte a5 = imageReader.ReadByte();
		byte a6 = imageReader.ReadByte();
		byte a7 = imageReader.ReadByte();
		ushort color = imageReader.ReadUInt16();
		ushort c1 = imageReader.ReadUInt16();
		DxtUtil.ConvertRgb565ToRgb888(color, out var r0, out var g0, out var b0);
		DxtUtil.ConvertRgb565ToRgb888(c1, out var r1, out var g1, out var b1);
		uint lookupTable = imageReader.ReadUInt32();
		int alphaIndex = 0;
		for (int blockY = 0; blockY < 4; blockY++)
		{
			for (int blockX = 0; blockX < 4; blockX++)
			{
				byte r2 = 0;
				byte g2 = 0;
				byte b2 = 0;
				byte a8 = 0;
				uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 3;
				switch (alphaIndex)
				{
				case 0:
					a8 = (byte)((a0 & 0xF) | ((a0 & 0xF) << 4));
					break;
				case 1:
					a8 = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4));
					break;
				case 2:
					a8 = (byte)((a1 & 0xF) | ((a1 & 0xF) << 4));
					break;
				case 3:
					a8 = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4));
					break;
				case 4:
					a8 = (byte)((a2 & 0xF) | ((a2 & 0xF) << 4));
					break;
				case 5:
					a8 = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4));
					break;
				case 6:
					a8 = (byte)((a3 & 0xF) | ((a3 & 0xF) << 4));
					break;
				case 7:
					a8 = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4));
					break;
				case 8:
					a8 = (byte)((a4 & 0xF) | ((a4 & 0xF) << 4));
					break;
				case 9:
					a8 = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4));
					break;
				case 10:
					a8 = (byte)((a5 & 0xF) | ((a5 & 0xF) << 4));
					break;
				case 11:
					a8 = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4));
					break;
				case 12:
					a8 = (byte)((a6 & 0xF) | ((a6 & 0xF) << 4));
					break;
				case 13:
					a8 = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4));
					break;
				case 14:
					a8 = (byte)((a7 & 0xF) | ((a7 & 0xF) << 4));
					break;
				case 15:
					a8 = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4));
					break;
				}
				alphaIndex++;
				switch (index)
				{
				case 0u:
					r2 = r0;
					g2 = g0;
					b2 = b0;
					break;
				case 1u:
					r2 = r1;
					g2 = g1;
					b2 = b1;
					break;
				case 2u:
					r2 = (byte)((2 * r0 + r1) / 3);
					g2 = (byte)((2 * g0 + g1) / 3);
					b2 = (byte)((2 * b0 + b1) / 3);
					break;
				case 3u:
					r2 = (byte)((r0 + 2 * r1) / 3);
					g2 = (byte)((g0 + 2 * g1) / 3);
					b2 = (byte)((b0 + 2 * b1) / 3);
					break;
				}
				int px = (x << 2) + blockX;
				int py = (y << 2) + blockY;
				if (px < width && py < height)
				{
					int offset = py * width + px << 2;
					imageData[offset] = r2;
					imageData[offset + 1] = g2;
					imageData[offset + 2] = b2;
					imageData[offset + 3] = a8;
				}
			}
		}
	}

	internal static byte[] DecompressDxt5(byte[] imageData, int width, int height)
	{
		using MemoryStream imageStream = new MemoryStream(imageData);
		return DxtUtil.DecompressDxt5(imageStream, width, height);
	}

	internal static byte[] DecompressDxt5(Stream imageStream, int width, int height)
	{
		byte[] imageData = new byte[width * height * 4];
		using BinaryReader imageReader = new BinaryReader(imageStream);
		int blockCountX = (width + 3) / 4;
		int blockCountY = (height + 3) / 4;
		for (int y = 0; y < blockCountY; y++)
		{
			for (int x = 0; x < blockCountX; x++)
			{
				DxtUtil.DecompressDxt5Block(imageReader, x, y, blockCountX, width, height, imageData);
			}
		}
		return imageData;
	}

	private static void DecompressDxt5Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
	{
		byte alpha0 = imageReader.ReadByte();
		byte alpha1 = imageReader.ReadByte();
		ulong alphaMask = imageReader.ReadByte();
		alphaMask += (ulong)imageReader.ReadByte() << 8;
		alphaMask += (ulong)imageReader.ReadByte() << 16;
		alphaMask += (ulong)imageReader.ReadByte() << 24;
		alphaMask += (ulong)imageReader.ReadByte() << 32;
		alphaMask += (ulong)imageReader.ReadByte() << 40;
		ushort color = imageReader.ReadUInt16();
		ushort c1 = imageReader.ReadUInt16();
		DxtUtil.ConvertRgb565ToRgb888(color, out var r0, out var g0, out var b0);
		DxtUtil.ConvertRgb565ToRgb888(c1, out var r1, out var g1, out var b1);
		uint lookupTable = imageReader.ReadUInt32();
		for (int blockY = 0; blockY < 4; blockY++)
		{
			for (int blockX = 0; blockX < 4; blockX++)
			{
				byte r2 = 0;
				byte g2 = 0;
				byte b2 = 0;
				byte a = byte.MaxValue;
				uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 3;
				uint alphaIndex = (uint)((alphaMask >> 3 * (4 * blockY + blockX)) & 7);
				a = alphaIndex switch
				{
					0u => alpha0, 
					1u => alpha1, 
					_ => (alpha0 <= alpha1) ? (alphaIndex switch
					{
						6u => 0, 
						7u => byte.MaxValue, 
						_ => (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5), 
					}) : ((byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7)), 
				};
				switch (index)
				{
				case 0u:
					r2 = r0;
					g2 = g0;
					b2 = b0;
					break;
				case 1u:
					r2 = r1;
					g2 = g1;
					b2 = b1;
					break;
				case 2u:
					r2 = (byte)((2 * r0 + r1) / 3);
					g2 = (byte)((2 * g0 + g1) / 3);
					b2 = (byte)((2 * b0 + b1) / 3);
					break;
				case 3u:
					r2 = (byte)((r0 + 2 * r1) / 3);
					g2 = (byte)((g0 + 2 * g1) / 3);
					b2 = (byte)((b0 + 2 * b1) / 3);
					break;
				}
				int px = (x << 2) + blockX;
				int py = (y << 2) + blockY;
				if (px < width && py < height)
				{
					int offset = py * width + px << 2;
					imageData[offset] = r2;
					imageData[offset + 1] = g2;
					imageData[offset + 2] = b2;
					imageData[offset + 3] = a;
				}
			}
		}
	}

	private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
	{
		int temp = (color >> 11) * 255 + 16;
		r = (byte)((temp / 32 + temp) / 32);
		temp = ((color & 0x7E0) >> 5) * 255 + 32;
		g = (byte)((temp / 64 + temp) / 64);
		temp = (color & 0x1F) * 255 + 16;
		b = (byte)((temp / 32 + temp) / 32);
	}
}
