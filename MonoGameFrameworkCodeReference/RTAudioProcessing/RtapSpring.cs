using System;
using System.Runtime.InteropServices;

namespace RTAudioProcessing;

public sealed class RtapSpring : IDisposable
{
	private static readonly int SpringSizeInBytes;

	private bool disposed;

	internal IntPtr SpringPtr;

	internal IntPtr DataPtr;

	public readonly int Length;

	public readonly RtapFormat Format;

	public readonly int SampleRate;

	public readonly double Duration;

	static RtapSpring()
	{
		RtapSpring.SpringSizeInBytes = RTAP.rtap_alloc_size_for_spring();
	}

	public RtapSpring(byte[] data, RtapFormat format, int sampleRate, int blockAlignment)
	{
		this.Format = format;
		this.SampleRate = sampleRate;
		this.DataPtr = Marshal.AllocHGlobal(data.Length);
		Marshal.Copy(data, 0, this.DataPtr, data.Length);
		this.SpringPtr = Marshal.AllocHGlobal(RtapSpring.SpringSizeInBytes);
		RTAP.rtap_spring_init(this.SpringPtr, this.DataPtr, data.Length, (int)format, sampleRate, blockAlignment);
		this.Length = RTAP.rtap_spring_get_length(this.SpringPtr);
		this.Duration = RTAP.rtap_spring_get_duration(this.SpringPtr);
	}

	~RtapSpring()
	{
		this.Dispose(disposing: false);
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
			Marshal.FreeHGlobal(this.SpringPtr);
			Marshal.FreeHGlobal(this.DataPtr);
			this.SpringPtr = IntPtr.Zero;
			this.DataPtr = IntPtr.Zero;
			this.disposed = true;
		}
	}
}
