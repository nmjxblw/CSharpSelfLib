using System.Threading;

namespace StbImageWriteSharp;

internal static class MemoryStats
{
	private static int _allocations;

	public static int Allocations => MemoryStats._allocations;

	internal static void Allocated()
	{
		Interlocked.Increment(ref MemoryStats._allocations);
	}

	internal static void Freed()
	{
		Interlocked.Decrement(ref MemoryStats._allocations);
	}
}
