using System;
using System.Runtime.InteropServices;

namespace RTAudioProcessing;

public sealed class RtapRiver : IDisposable
{
	private static readonly int RiverSizeInBytes;

	private bool disposed;

	internal IntPtr RiverPtr;

	public RtapSpring Spring { get; private set; }

	static RtapRiver()
	{
		RtapRiver.RiverSizeInBytes = RTAP.rtap_alloc_size_for_river();
	}

	public RtapRiver()
	{
		this.RiverPtr = Marshal.AllocHGlobal(RtapRiver.RiverSizeInBytes);
		RTAP.rtap_river_reset(this.RiverPtr);
	}

	~RtapRiver()
	{
		this.Dispose(disposing: false);
	}

	public void Reset()
	{
		if (!(this.RiverPtr == IntPtr.Zero))
		{
			RTAP.rtap_river_reset(this.RiverPtr);
		}
	}

	public void SetSpring(RtapSpring spring)
	{
		if (!(this.RiverPtr == IntPtr.Zero))
		{
			RTAP.rtap_river_set_spring(this.RiverPtr, spring?.SpringPtr ?? IntPtr.Zero);
			this.Spring = spring;
		}
	}

	public void ReadInto(IntPtr destination, int startIndex, int length)
	{
		if (this.RiverPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException("River::ReadInto(...) called with RiverPtr == IntPtr.Zero (likely already disposed)");
		}
		RTAP.rtap_river_read_into(this.RiverPtr, destination, startIndex, length);
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			Marshal.FreeHGlobal(this.RiverPtr);
			this.RiverPtr = IntPtr.Zero;
			this.disposed = true;
		}
	}
}
