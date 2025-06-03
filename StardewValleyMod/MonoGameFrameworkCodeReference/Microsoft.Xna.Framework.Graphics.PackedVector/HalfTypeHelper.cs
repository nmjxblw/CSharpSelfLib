using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

internal class HalfTypeHelper
{
	[StructLayout(LayoutKind.Explicit)]
	private struct uif
	{
		[FieldOffset(0)]
		public float f;

		[FieldOffset(0)]
		public int i;

		[FieldOffset(0)]
		public uint u;
	}

	internal static ushort Convert(float f)
	{
		uif uif = new uif
		{
			f = f
		};
		return HalfTypeHelper.Convert(uif.i);
	}

	internal static ushort Convert(int i)
	{
		int s = (i >> 16) & 0x8000;
		int e = ((i >> 23) & 0xFF) - 112;
		int m = i & 0x7FFFFF;
		if (e <= 0)
		{
			if (e < -10)
			{
				return (ushort)s;
			}
			m |= 0x800000;
			int t = 14 - e;
			int a = (1 << t - 1) - 1;
			int b = (m >> t) & 1;
			m = m + a + b >> t;
			return (ushort)(s | m);
		}
		if (e == 143)
		{
			if (m == 0)
			{
				return (ushort)(s | 0x7C00);
			}
			m >>= 13;
			return (ushort)((uint)(s | 0x7C00 | m) | ((m == 0) ? 1u : 0u));
		}
		m = m + 4095 + ((m >> 13) & 1);
		if ((m & 0x800000) != 0)
		{
			m = 0;
			e++;
		}
		if (e > 30)
		{
			return (ushort)(s | 0x7C00);
		}
		return (ushort)(s | (e << 10) | (m >> 13));
	}

	internal static float Convert(ushort value)
	{
		uint mantissa = (uint)(value & 0x3FF);
		uint exp = 4294967282u;
		uint rst;
		if ((value & -33792) == 0)
		{
			if (mantissa != 0)
			{
				while ((mantissa & 0x400) == 0)
				{
					exp--;
					mantissa <<= 1;
				}
				mantissa &= 0xFFFFFBFFu;
				rst = (uint)((value & 0x8000) << 16) | (exp + 127 << 23) | (mantissa << 13);
			}
			else
			{
				rst = (uint)((value & 0x8000) << 16);
			}
		}
		else
		{
			rst = (uint)(((value & 0x8000) << 16) | (((value >>> 10) & 0x1F) - 15 + 127 << 23)) | (mantissa << 13);
		}
		return new uif
		{
			u = rst
		}.f;
	}
}
