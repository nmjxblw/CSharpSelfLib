using System;
using System.Runtime.InteropServices;

namespace StbImageWriteSharp;

internal static class CRuntime
{
	public const long DBL_EXP_MASK = 9218868437227405312L;

	public const int DBL_MANT_BITS = 52;

	public const long DBL_SGN_MASK = long.MinValue;

	public const long DBL_MANT_MASK = 4503599627370495L;

	public const long DBL_EXP_CLR_MASK = -9218868437227405313L;

	public unsafe static void* malloc(ulong size)
	{
		return CRuntime.malloc((long)size);
	}

	public unsafe static void* malloc(long size)
	{
		return Marshal.AllocHGlobal((int)size).ToPointer();
	}

	public unsafe static void memcpy(void* a, void* b, long size)
	{
		byte* ap = (byte*)a;
		byte* bp = (byte*)b;
		for (long i = 0L; i < size; i++)
		{
			*(ap++) = *(bp++);
		}
	}

	public unsafe static void memcpy(void* a, void* b, ulong size)
	{
		CRuntime.memcpy(a, b, (long)size);
	}

	public unsafe static void memmove(void* a, void* b, long size)
	{
		void* temp = null;
		try
		{
			temp = CRuntime.malloc(size);
			CRuntime.memcpy(temp, b, size);
			CRuntime.memcpy(a, temp, size);
		}
		finally
		{
			if (temp != null)
			{
				CRuntime.free(temp);
			}
		}
	}

	public unsafe static void memmove(void* a, void* b, ulong size)
	{
		CRuntime.memmove(a, b, (long)size);
	}

	public unsafe static int memcmp(void* a, void* b, long size)
	{
		int result = 0;
		byte* ap = (byte*)a;
		byte* bp = (byte*)b;
		for (long i = 0L; i < size; i++)
		{
			if (*ap != *bp)
			{
				result++;
			}
			ap++;
			bp++;
		}
		return result;
	}

	public unsafe static int memcmp(void* a, void* b, ulong size)
	{
		return CRuntime.memcmp(a, b, (long)size);
	}

	public unsafe static int memcmp(byte* a, byte[] b, ulong size)
	{
		fixed (byte* ptr = b)
		{
			void* bptr = ptr;
			return CRuntime.memcmp(a, bptr, (long)size);
		}
	}

	public unsafe static void free(void* a)
	{
		Marshal.FreeHGlobal(new IntPtr(a));
	}

	public unsafe static void memset(void* ptr, int value, long size)
	{
		byte* bptr = (byte*)ptr;
		byte bval = (byte)value;
		for (long i = 0L; i < size; i++)
		{
			*(bptr++) = bval;
		}
	}

	public unsafe static void memset(void* ptr, int value, ulong size)
	{
		CRuntime.memset(ptr, value, (long)size);
	}

	public static uint _lrotl(uint x, int y)
	{
		return (x << y) | (x >> 32 - y);
	}

	public unsafe static void* realloc(void* a, long newSize)
	{
		if (a == null)
		{
			return CRuntime.malloc(newSize);
		}
		return Marshal.ReAllocHGlobal(new IntPtr(a), new IntPtr(newSize)).ToPointer();
	}

	public unsafe static void* realloc(void* a, ulong newSize)
	{
		return CRuntime.realloc(a, (long)newSize);
	}

	public static int abs(int v)
	{
		return Math.Abs(v);
	}

	/// <summary>
	/// This code had been borrowed from here: https://github.com/MachineCognitis/C.math.NET
	/// </summary>
	/// <param name="number"></param>
	/// <param name="exponent"></param>
	/// <returns></returns>
	public unsafe static double frexp(double number, int* exponent)
	{
		long bits = BitConverter.DoubleToInt64Bits(number);
		int exp = (int)((bits & 0x7FF0000000000000L) >> 52);
		*exponent = 0;
		if (exp == 2047 || number == 0.0)
		{
			number += number;
		}
		else
		{
			*exponent = exp - 1022;
			if (exp == 0)
			{
				number *= BitConverter.Int64BitsToDouble(4850376798678024192L);
				bits = BitConverter.DoubleToInt64Bits(number);
				exp = (int)((bits & 0x7FF0000000000000L) >> 52);
				*exponent = exp - 1022 - 54;
			}
			number = BitConverter.Int64BitsToDouble((bits & -9218868437227405313L) | 0x3FE0000000000000L);
		}
		return number;
	}
}
