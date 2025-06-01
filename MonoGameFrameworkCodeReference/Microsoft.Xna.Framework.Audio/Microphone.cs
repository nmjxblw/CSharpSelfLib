using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>
/// Provides microphones capture features. 
/// </summary>
/// <summary>
/// Provides microphones capture features.  
/// </summary>
public sealed class Microphone
{
	/// <summary>
	/// Returns the friendly name of the microphone.
	/// </summary>
	public readonly string Name;

	private TimeSpan _bufferDuration = TimeSpan.FromMilliseconds(1000.0);

	private const bool _isHeadset = false;

	private int _sampleRate = 44100;

	private MicrophoneState _state = MicrophoneState.Stopped;

	private static List<Microphone> _allMicrophones;

	private static Microphone _default;

	private IntPtr _captureDevice = IntPtr.Zero;

	/// <summary>
	/// Gets or sets the capture buffer duration. This value must be greater than 100 milliseconds, lower than 1000 milliseconds, and must be 10 milliseconds aligned (BufferDuration % 10 == 10).
	/// </summary>
	public TimeSpan BufferDuration
	{
		get
		{
			return this._bufferDuration;
		}
		set
		{
			if (value.TotalMilliseconds < 100.0 || value.TotalMilliseconds > 1000.0)
			{
				throw new ArgumentOutOfRangeException("Buffer duration must be a value between 100 and 1000 milliseconds.");
			}
			if (value.TotalMilliseconds % 10.0 != 0.0)
			{
				throw new ArgumentOutOfRangeException("Buffer duration must be 10ms aligned (BufferDuration % 10 == 0)");
			}
			this._bufferDuration = value;
		}
	}

	/// <summary>
	/// Determines if the microphone is a wired headset.
	/// Note: XNA could know if a headset microphone was plugged in an Xbox 360 controller but MonoGame can't.
	/// Hence, this is always true on mobile platforms, and always false otherwise.
	/// </summary>
	public bool IsHeadset => false;

	/// <summary>
	/// Returns the sample rate of the captured audio.
	/// Note: default value is 44100hz
	/// </summary>
	public int SampleRate => this._sampleRate;

	/// <summary>
	/// Returns the state of the Microphone. 
	/// </summary>
	public MicrophoneState State => this._state;

	/// <summary>
	/// Returns all compatible microphones.
	/// </summary>
	public static ReadOnlyCollection<Microphone> All
	{
		get
		{
			SoundEffect.Initialize();
			if (Microphone._allMicrophones == null)
			{
				Microphone._allMicrophones = new List<Microphone>();
			}
			return new ReadOnlyCollection<Microphone>(Microphone._allMicrophones);
		}
	}

	/// <summary>
	/// Returns the default microphone.
	/// </summary>
	public static Microphone Default
	{
		get
		{
			SoundEffect.Initialize();
			return Microphone._default;
		}
	}

	/// <summary>
	/// Event fired when the audio data are available.
	/// </summary>
	public event EventHandler<EventArgs> BufferReady;

	internal Microphone()
	{
	}

	internal Microphone(string name)
	{
		this.Name = name;
	}

	/// <summary>
	/// Returns the duration based on the size of the buffer (assuming 16-bit PCM data).
	/// </summary>
	/// <param name="sizeInBytes">Size, in bytes</param>
	/// <returns>TimeSpan of the duration.</returns>
	public TimeSpan GetSampleDuration(int sizeInBytes)
	{
		return SoundEffect.GetSampleDuration(sizeInBytes, this._sampleRate, AudioChannels.Mono);
	}

	/// <summary>
	/// Returns the size, in bytes, of the array required to hold the specified duration of 16-bit PCM data. 
	/// </summary>
	/// <param name="duration">TimeSpan of the duration of the sample.</param>
	/// <returns>Size, in bytes, of the buffer.</returns>
	public int GetSampleSizeInBytes(TimeSpan duration)
	{
		return SoundEffect.GetSampleSizeInBytes(duration, this._sampleRate, AudioChannels.Mono);
	}

	/// <summary>
	/// Starts microphone capture.
	/// </summary>
	public void Start()
	{
		this.PlatformStart();
	}

	/// <summary>
	/// Stops microphone capture.
	/// </summary>
	public void Stop()
	{
		this.PlatformStop();
	}

	/// <summary>
	/// Gets the latest available data from the microphone.
	/// </summary>
	/// <param name="buffer">Buffer, in bytes, of the captured data (16-bit PCM).</param>
	/// <returns>The buffer size, in bytes, of the captured data.</returns>
	public int GetData(byte[] buffer)
	{
		return this.GetData(buffer, 0, buffer.Length);
	}

	/// <summary>
	/// Gets the latest available data from the microphone.
	/// </summary>
	/// <param name="buffer">Buffer, in bytes, of the captured data (16-bit PCM).</param>
	/// <param name="offset">Byte offset.</param>
	/// <param name="count">Amount, in bytes.</param>
	/// <returns>The buffer size, in bytes, of the captured data.</returns>
	public int GetData(byte[] buffer, int offset, int count)
	{
		return this.PlatformGetData(buffer, offset, count);
	}

	internal static void UpdateMicrophones()
	{
		if (Microphone._allMicrophones != null)
		{
			for (int i = 0; i < Microphone._allMicrophones.Count; i++)
			{
				Microphone._allMicrophones[i].Update();
			}
		}
	}

	internal static void StopMicrophones()
	{
		if (Microphone._allMicrophones != null)
		{
			for (int i = 0; i < Microphone._allMicrophones.Count; i++)
			{
				Microphone._allMicrophones[i].Stop();
			}
		}
	}

	internal void CheckALCError(string operation)
	{
		AlcError error = Alc.GetErrorForDevice(this._captureDevice);
		if (error == AlcError.NoError)
		{
			return;
		}
		string errorFmt = "OpenAL Error: {0}";
		throw new NoMicrophoneConnectedException($"{operation} - {string.Format(errorFmt, error)}");
	}

	internal static void PopulateCaptureDevices()
	{
		if (Microphone._allMicrophones != null)
		{
			Microphone._allMicrophones.Clear();
		}
		else
		{
			Microphone._allMicrophones = new List<Microphone>();
		}
		Microphone._default = null;
		string defaultDevice = Alc.GetString(IntPtr.Zero, AlcGetString.CaptureDefaultDeviceSpecifier);
		IntPtr deviceList = Alc.alcGetString(IntPtr.Zero, 784);
		while (true)
		{
			string deviceIdentifier = InteropHelpers.Utf8ToString(deviceList);
			if (!string.IsNullOrEmpty(deviceIdentifier))
			{
				Microphone microphone = new Microphone(deviceIdentifier);
				Microphone._allMicrophones.Add(microphone);
				if (deviceIdentifier == defaultDevice)
				{
					Microphone._default = microphone;
				}
				deviceList += deviceIdentifier.Length + 1;
				continue;
			}
			break;
		}
	}

	internal void PlatformStart()
	{
		if (this._state != MicrophoneState.Started)
		{
			this._captureDevice = Alc.CaptureOpenDevice(this.Name, (uint)this._sampleRate, ALFormat.Mono16, this.GetSampleSizeInBytes(this._bufferDuration));
			this.CheckALCError("Failed to open capture device.");
			if (!(this._captureDevice != IntPtr.Zero))
			{
				throw new NoMicrophoneConnectedException("Failed to open capture device.");
			}
			Alc.CaptureStart(this._captureDevice);
			this.CheckALCError("Failed to start capture.");
			this._state = MicrophoneState.Started;
		}
	}

	internal void PlatformStop()
	{
		if (this._state == MicrophoneState.Started)
		{
			Alc.CaptureStop(this._captureDevice);
			this.CheckALCError("Failed to stop capture.");
			Alc.CaptureCloseDevice(this._captureDevice);
			this.CheckALCError("Failed to close capture device.");
			this._captureDevice = IntPtr.Zero;
		}
		this._state = MicrophoneState.Stopped;
	}

	internal int GetQueuedSampleCount()
	{
		if (this._state == MicrophoneState.Stopped || this.BufferReady == null)
		{
			return 0;
		}
		int[] values = new int[1];
		Alc.GetInteger(this._captureDevice, AlcGetInteger.CaptureSamples, 1, values);
		this.CheckALCError("Failed to query capture samples.");
		return values[0];
	}

	internal void Update()
	{
		if (this.GetQueuedSampleCount() > 0)
		{
			this.BufferReady(this, EventArgs.Empty);
		}
	}

	internal int PlatformGetData(byte[] buffer, int offset, int count)
	{
		int sampleCount = this.GetQueuedSampleCount();
		sampleCount = Math.Min(count / 2, sampleCount);
		if (sampleCount > 0)
		{
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			Alc.CaptureSamples(this._captureDevice, handle.AddrOfPinnedObject() + offset, sampleCount);
			handle.Free();
			this.CheckALCError("Failed to capture samples.");
			return sampleCount * 2;
		}
		return 0;
	}
}
