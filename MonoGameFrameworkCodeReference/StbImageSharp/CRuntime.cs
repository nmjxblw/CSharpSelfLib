using System;
using System.Runtime.InteropServices;

namespace StbImageSharp;

internal static class CRuntime
{
	public unsafe static void* malloc(ulong size)
	{
		return CRuntime.malloc((long)size);
	}

	public unsafe static void* malloc(long size)
	{
		IntPtr ptr = Marshal.AllocHGlobal((int)size);
		MemoryStats.Allocated();
		return ptr.ToPointer();
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
		if (a != null)
		{
			Marshal.FreeHGlobal(new IntPtr(a));
			MemoryStats.Freed();
		}
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

	public static void SetArray<T>(T[] data, T value)
	{
		for (int i = 0; i < data.Length; i++)
		{
			data[i] = value;
		}
	}
}
