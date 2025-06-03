using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NVorbis;

internal static class Utils
{
	[StructLayout(LayoutKind.Explicit)]
	private struct FloatBits
	{
		[FieldOffset(0)]
		public float Float;

		[FieldOffset(0)]
		public uint Bits;
	}

	internal static int ilog(int x)
	{
		int cnt = 0;
		while (x > 0)
		{
			cnt++;
			x >>= 1;
		}
		return cnt;
	}

	internal static uint BitReverse(uint n)
	{
		return Utils.BitReverse(n, 32);
	}

	internal static uint BitReverse(uint n, int bits)
	{
		n = ((n & 0xAAAAAAAAu) >> 1) | ((n & 0x55555555) << 1);
		n = ((n & 0xCCCCCCCCu) >> 2) | ((n & 0x33333333) << 2);
		n = ((n & 0xF0F0F0F0u) >> 4) | ((n & 0xF0F0F0F) << 4);
		n = ((n & 0xFF00FF00u) >> 8) | ((n & 0xFF00FF) << 8);
		return ((n >> 16) | (n << 16)) >> 32 - bits;
	}

	internal static float ClipValue(float value, ref bool clipped)
	{
		FloatBits fb = default(FloatBits);
		fb.Bits = 0u;
		fb.Float = value;
		if ((fb.Bits & 0x7FFFFFFF) > 1065353215)
		{
			clipped = true;
			fb.Bits = 0x3F7FFFFF | (fb.Bits & 0x80000000u);
		}
		return fb.Float;
	}

	internal static float ConvertFromVorbisFloat32(uint bits)
	{
		int sign = (int)bits >> 31;
		double exponent = (int)(((bits & 0x7FE00000) >> 21) - 788);
		return (float)(((bits & 0x1FFFFF) ^ sign) + (sign & 1)) * (float)Math.Pow(2.0, exponent);
	}

	internal static int Sum(Queue<int> queue)
	{
		int value = 0;
		for (int i = 0; i < queue.Count; i++)
		{
			int temp = queue.Dequeue();
			value += temp;
			queue.Enqueue(temp);
		}
		return value;
	}
}
