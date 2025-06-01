using System;
using MonoGame.OpenAL;
using RTAudioProcessing;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>Represents a single instance of a playing, paused, or stopped sound.</summary>
/// <remarks>
/// <para>SoundEffectInstances are created through SoundEffect.CreateInstance() and used internally by SoundEffect.Play()</para>
/// </remarks>
public class SoundEffectInstance : IDisposable
{
	private bool _isDisposed;

	internal bool _isPooled = true;

	public bool _isXAct;

	internal bool _isDynamic;

	internal SoundEffect _effect;

	private float _pan;

	private float _volume;

	private float _pitch;

	private const int DefaultBufferCount = 3;

	private const int DefaultBufferSize = 32768;

	internal SoundState SoundState = SoundState.Stopped;

	private uint _loopCount;

	private float _alVolume = 1f;

	internal int SourceId;

	private float reverb;

	private bool applyFilter;

	private EfxFilterType filterType;

	private float filterQ;

	private float frequency;

	private int pauseCount;

	private bool _filterEnabled;

	private int[] BufferIds;

	private int[] BuffersAvailable;

	private bool HasBufferIds;

	private byte[] BufferData;

	private int BufferHead;

	private bool BufferFinished;

	private int TimesPlayed;

	private RtapRiver River = new RtapRiver();

	private readonly float[] FilterState = new float[4];

	internal readonly object sourceMutex = new object();

	internal OpenALSoundController controller;

	internal bool HasSourceId;

	private const int FHIGH = 2;

	private const int FBAND = 1;

	private const int FLOW = 0;

	/// <summary>Sets the number of times the track should repeat after playback.</summary>
	/// <remarks>This value has no effect on an already-playing sound.</remarks>
	public virtual uint LoopCount
	{
		get
		{
			return this.PlatformGetLoopCount();
		}
		set
		{
			this.PlatformSetLoopCount(value);
		}
	}

	/// <summary>Gets or sets the pan, or speaker balance..</summary>
	/// <value>Pan value ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker). Values outside of this range will throw an exception.</value>
	public float Pan
	{
		get
		{
			return this._pan;
		}
		set
		{
			if (value < -1f || value > 1f)
			{
				throw new ArgumentOutOfRangeException();
			}
			this._pan = value;
			this.PlatformSetPan(value);
		}
	}

	/// <summary>Gets or sets the pitch adjustment.</summary>
	/// <value>Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave). Values outside of this range will throw an Exception.</value>
	public float Pitch
	{
		get
		{
			return this._pitch;
		}
		set
		{
			if (!this._isXAct && (value < -1f || value > 1f))
			{
				throw new ArgumentOutOfRangeException();
			}
			this._pitch = value;
			this.PlatformSetPitch(value);
		}
	}

	/// <summary>Gets or sets the volume of the SoundEffectInstance.</summary>
	/// <value>Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled by SoundEffect.MasterVolume.</value>
	/// <remarks>
	/// This is the volume relative to SoundEffect.MasterVolume. Before playback, this Volume property is multiplied by SoundEffect.MasterVolume when determining the final mix volume.
	/// </remarks>
	public float Volume
	{
		get
		{
			return this._volume;
		}
		set
		{
			if (!this._isXAct && (value < 0f || value > 1f))
			{
				throw new ArgumentOutOfRangeException();
			}
			this._volume = value;
			if (this._isXAct)
			{
				this.PlatformSetVolume(value);
			}
			else
			{
				this.PlatformSetVolume(value * SoundEffect.MasterVolume);
			}
		}
	}

	/// <summary>Gets the SoundEffectInstance's current playback state.</summary>
	public virtual SoundState State => this.PlatformGetState();

	/// <summary>Indicates whether the object is disposed.</summary>
	public bool IsDisposed => this._isDisposed;

	internal FilterMode _filterMode => this.filterType switch
	{
		EfxFilterType.Highpass => FilterMode.HighPass, 
		EfxFilterType.Bandpass => FilterMode.BandPass, 
		_ => FilterMode.LowPass, 
	};

	internal float _filterQ => this.filterQ;

	internal float _filterFrequency => this.frequency;

	internal SoundEffectInstance()
	{
		this._pan = 0f;
		this._volume = 1f;
		this._pitch = 0f;
	}

	internal SoundEffectInstance(byte[] buffer, int sampleRate, int channels)
		: this()
	{
		this.PlatformInitialize(buffer, sampleRate, channels);
	}

	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the
	/// <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffectInstance" /> is reclaimed by garbage collection.
	/// </summary>
	~SoundEffectInstance()
	{
		this.Dispose(disposing: false);
	}

	/// <summary>Applies 3D positioning to the SoundEffectInstance using a single listener.</summary>
	/// <param name="listener">Data about the listener.</param>
	/// <param name="emitter">Data about the source of emission.</param>
	public void Apply3D(AudioListener listener, AudioEmitter emitter)
	{
		this.PlatformApply3D(listener, emitter);
	}

	/// <summary>Applies 3D positioning to the SoundEffectInstance using multiple listeners.</summary>
	/// <param name="listeners">Data about each listener.</param>
	/// <param name="emitter">Data about the source of emission.</param>
	public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
	{
		foreach (AudioListener l in listeners)
		{
			this.PlatformApply3D(l, emitter);
		}
	}

	/// <summary>Pauses playback of a SoundEffectInstance.</summary>
	/// <remarks>Paused instances can be resumed with SoundEffectInstance.Play() or SoundEffectInstance.Resume().</remarks>
	public virtual void Pause()
	{
		this.PlatformPause();
	}

	/// <summary>Plays or resumes a SoundEffectInstance.</summary>
	/// <remarks>Throws an exception if more sounds are playing than the platform allows.</remarks>
	public virtual void Play()
	{
		if (this._isDisposed)
		{
			throw new ObjectDisposedException("SoundEffectInstance");
		}
		if (this.State == SoundState.Playing)
		{
			return;
		}
		if (this.State == SoundState.Paused)
		{
			this.Resume();
			return;
		}
		if (this.State != SoundState.Paused && !SoundEffectInstancePool.SoundsAvailable)
		{
			throw new InstancePlayLimitException();
		}
		if (!this._isXAct)
		{
			this.PlatformSetVolume(this._volume * SoundEffect.MasterVolume);
		}
		this.PlatformPlay();
		SoundEffectInstancePool.Remove(this);
	}

	/// <summary>Resumes playback for a SoundEffectInstance.</summary>
	/// <remarks>Only has effect on a SoundEffectInstance in a paused state.</remarks>
	public virtual void Resume()
	{
		this.PlatformResume();
	}

	/// <summary>Immediately stops playing a SoundEffectInstance.</summary>
	public virtual void Stop()
	{
		this.PlatformStop(immediate: true);
	}

	/// <summary>Stops playing a SoundEffectInstance, either immediately or as authored.</summary>
	/// <param name="immediate">Determined whether the sound stops immediately, or after playing its release phase and/or transitions.</param>
	/// <remarks>Stopping a sound with the immediate argument set to false will allow it to play any release phases, such as fade, before coming to a stop.</remarks>
	public virtual void Stop(bool immediate)
	{
		this.PlatformStop(immediate);
	}

	/// <summary>Releases the resources held by this <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffectInstance" />.</summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases the resources held by this <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffectInstance" />.
	/// </summary>
	/// <param name="disposing">If set to <c>true</c>, Dispose was called explicitly.</param>
	/// <remarks>If the disposing parameter is true, the Dispose method was called explicitly. This
	/// means that managed objects referenced by this instance should be disposed or released as
	/// required.  If the disposing parameter is false, Dispose was called by the finalizer and
	/// no managed objects should be touched because we do not know if they are still valid or
	/// not at that time.  Unmanaged resources should always be released.</remarks>
	protected virtual void Dispose(bool disposing)
	{
		if (!this._isDisposed)
		{
			this.PlatformDispose(disposing);
			this._isDisposed = true;
		}
	}

	internal virtual void UpdateQueue()
	{
		this.PlatformUpdateQueue();
	}

	/// <summary>
	/// Creates a standalone SoundEffectInstance from given wavedata.
	/// </summary>
	internal void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
	{
		this.InitializeSound();
	}

	/// <summary>
	/// Gets the OpenAL sound controller, constructs the sound buffer, and sets up the event delegates for
	/// the reserved and recycled events.
	/// </summary>
	internal void InitializeSound()
	{
		this.controller = OpenALSoundController.Instance;
	}

	/// <summary>
	/// Converts the XNA [-1, 1] pitch range to OpenAL pitch (0, INF) or Android SoundPool playback rate [0.5, 2].
	/// <param name="xnaPitch">The pitch of the sound in the Microsoft XNA range.</param>
	/// </summary>
	private static float XnaPitchToAlPitch(float xnaPitch)
	{
		return (float)Math.Pow(2.0, xnaPitch);
	}

	private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
	{
		if (this.HasSourceId)
		{
			AL.GetListener(ALListener3f.Position, out var x, out var y, out var z);
			Vector3 posOffset = emitter.Position - listener.Position;
			Matrix orientation = Matrix.CreateWorld(Vector3.Zero, listener.Forward, listener.Up);
			Vector3 finalPos = new Vector3(x + posOffset.X, y + posOffset.Y, z + posOffset.Z);
			finalPos = Vector3.Transform(finalPos, orientation);
			Vector3 finalVel = emitter.Velocity;
			finalVel = Vector3.Transform(finalVel, orientation);
			AL.Source(this.SourceId, ALSource3f.Position, finalPos.X, finalPos.Y, finalPos.Z);
			AL.Source(this.SourceId, ALSource3f.Velocity, finalVel.X, finalVel.Y, finalVel.Z);
			AL.Source(this.SourceId, ALSourcef.ReferenceDistance, SoundEffect.DistanceScale);
			AL.DopplerFactor(SoundEffect.DopplerScale);
		}
	}

	private void PlatformPause()
	{
		if (this.HasSourceId && this.SoundState == SoundState.Playing)
		{
			if (this.pauseCount == 0)
			{
				AL.SourcePause(this.SourceId);
			}
			this.pauseCount++;
			this.SoundState = SoundState.Paused;
		}
	}

	private void PlatformPlay()
	{
		if (!this.HasBufferIds)
		{
			this.BufferIds = AL.GenBuffers(3);
			this.HasBufferIds = true;
			this.BufferData = new byte[32768];
		}
		for (int i = 0; i < 4; i++)
		{
			this.FilterState[i] = 0f;
		}
		this.BuffersAvailable = new int[3];
		for (int j = 0; j < 3; j++)
		{
			this.BuffersAvailable[j] = this.BufferIds[j];
		}
		this.BufferHead = 0;
		this.BufferFinished = false;
		this.TimesPlayed = 0;
		this.SourceId = 0;
		this.HasSourceId = false;
		this.SourceId = this.controller.ReserveSource();
		this.HasSourceId = true;
		if (this.HasSourceId)
		{
			AL.Source(this.SourceId, ALSourcei.SourceRelative, 1);
			AL.DistanceModel(ALDistanceModel.InverseDistanceClamped);
			AL.Source(this.SourceId, ALSource3f.Position, this._pan, 0f, 0f);
			AL.Source(this.SourceId, ALSource3f.Velocity, 0f, 0f, 0f);
			AL.Source(this.SourceId, ALSourcef.Gain, this._alVolume);
			AL.Source(this.SourceId, ALSourcei.Buffer, 0);
			this.River.SetSpring(this._effect.Spring);
			this.QueueBuffers();
			AL.Source(this.SourceId, ALSourcef.Pitch, SoundEffectInstance.XnaPitchToAlPitch(this._pitch));
			this.ApplyReverb();
			this.ApplyFilter();
			AL.SourcePlay(this.SourceId);
			this.SoundState = SoundState.Playing;
		}
	}

	private void PlatformResume()
	{
		if (!this.HasSourceId)
		{
			this.Play();
			return;
		}
		if (this.SoundState == SoundState.Paused)
		{
			this.pauseCount--;
			if (this.pauseCount == 0)
			{
				AL.SourcePlay(this.SourceId);
			}
		}
		this.SoundState = SoundState.Playing;
	}

	private void PlatformStop(bool immediate)
	{
		this.FreeSource();
		this.SoundState = SoundState.Stopped;
		this._filterEnabled = false;
		this.BufferFinished = true;
	}

	private void FreeSource()
	{
		if (!this.HasSourceId)
		{
			return;
		}
		lock (this.sourceMutex)
		{
			if (this.HasSourceId && AL.IsSource(this.SourceId))
			{
				AL.Source(this.SourceId, ALSourceb.Looping, a: false);
				AL.Source(this.SourceId, ALSource3f.Position, 0f, 0f, 0.1f);
				AL.Source(this.SourceId, ALSourcef.Pitch, 1f);
				AL.Source(this.SourceId, ALSourcef.Gain, 1f);
				AL.SourceStop(this.SourceId);
				this.UnqueueProcessedBuffers();
				if (OpenALSoundController.Efx.IsInitialized)
				{
					OpenALSoundController.Efx.BindSourceToAuxiliarySlot(this.SourceId, 0, 0, 0);
					AL.Source(this.SourceId, ALSourcei.EfxDirectFilter, 0);
				}
				this.controller.FreeSource(this);
			}
		}
	}

	private void PlatformSetIsLooped(bool value)
	{
		this.PlatformSetLoopCount(value ? 255u : 0u);
	}

	private bool PlatformGetIsLooped()
	{
		return this.LoopCount != 0;
	}

	private void PlatformSetLoopCount(uint value)
	{
		this._loopCount = value;
	}

	private uint PlatformGetLoopCount()
	{
		return this._loopCount;
	}

	private void PlatformSetPan(float value)
	{
		if (this.HasSourceId)
		{
			AL.Source(this.SourceId, ALSource3f.Position, value, 0f, 0.1f);
		}
	}

	private void PlatformSetPitch(float value)
	{
		if (this.HasSourceId)
		{
			AL.Source(this.SourceId, ALSourcef.Pitch, SoundEffectInstance.XnaPitchToAlPitch(value));
		}
	}

	private SoundState PlatformGetState()
	{
		if (!this.HasSourceId)
		{
			return SoundState.Stopped;
		}
		switch (AL.GetSourceState(this.SourceId))
		{
		case ALSourceState.Initial:
			this.SoundState = SoundState.Stopped;
			break;
		case ALSourceState.Stopped:
			if (this.SoundState != SoundState.Playing || this.BufferFinished)
			{
				this.SoundState = SoundState.Stopped;
			}
			break;
		case ALSourceState.Paused:
			this.SoundState = SoundState.Paused;
			break;
		case ALSourceState.Playing:
			this.SoundState = SoundState.Playing;
			break;
		}
		return this.SoundState;
	}

	private void PlatformSetVolume(float value)
	{
		this._alVolume = value;
		if (this.HasSourceId)
		{
			AL.Source(this.SourceId, ALSourcef.Gain, this._alVolume);
		}
	}

	internal void PlatformSetReverbMix(float mix)
	{
		if (OpenALSoundController.Efx.IsInitialized)
		{
			this.reverb = mix;
			if (this.State == SoundState.Playing)
			{
				this.ApplyReverb();
				this.reverb = 0f;
			}
		}
	}

	private void ApplyReverb()
	{
		if (this.reverb > 0f && SoundEffect.ReverbSlot != 0)
		{
			OpenALSoundController.Efx.BindSourceToAuxiliarySlot(this.SourceId, (int)SoundEffect.ReverbSlot, 0, 0);
		}
	}

	private void ApplyFilter()
	{
	}

	internal void PlatformSetFilter(FilterMode mode, float filterQ, float frequency)
	{
		this._filterEnabled = true;
		this.applyFilter = true;
		switch (mode)
		{
		case FilterMode.BandPass:
			this.filterType = EfxFilterType.Bandpass;
			break;
		case FilterMode.LowPass:
			this.filterType = EfxFilterType.Lowpass;
			break;
		case FilterMode.HighPass:
			this.filterType = EfxFilterType.Highpass;
			break;
		}
		this.filterQ = filterQ;
		this.frequency = frequency;
		if (this.State == SoundState.Playing)
		{
			this.ApplyFilter();
			this.applyFilter = false;
		}
	}

	internal bool IsFilterEnabled()
	{
		return this._filterEnabled;
	}

	internal void PlatformClearFilter()
	{
		for (int i = 0; i < 4; i++)
		{
			this.FilterState[i] = 0f;
		}
		this.applyFilter = false;
		this._filterEnabled = false;
	}

	private void PlatformDispose(bool disposing)
	{
		this.FreeSource();
		if (this.HasBufferIds)
		{
			AL.DeleteBuffers(this.BufferIds);
			this.BufferIds = null;
			this.HasBufferIds = false;
		}
		this.River?.Dispose();
	}

	private unsafe bool QueueBuffer(int bufferId)
	{
		int springLength = this._effect.Spring.Length;
		if (this.BufferHead > springLength)
		{
			this.BufferHead = 0;
		}
		int copySize = springLength - this.BufferHead;
		if (copySize > 32768)
		{
			copySize = 32768;
		}
		fixed (byte* bufferPtr = this.BufferData)
		{
			this.River.ReadInto((IntPtr)bufferPtr, this.BufferHead, copySize);
			int copyHead = copySize;
			this.BufferHead += copySize;
			if (this.BufferHead >= springLength)
			{
				this.BufferHead = 0;
				this.TimesPlayed++;
			}
			int unfilled = 32768 - copySize;
			if (this.LoopCount >= 255 || this.TimesPlayed <= this.LoopCount)
			{
				while (unfilled > 0)
				{
					copySize = springLength - this.BufferHead;
					if (copySize > unfilled)
					{
						copySize = unfilled;
					}
					this.River.ReadInto((IntPtr)(bufferPtr + copyHead), this.BufferHead, copySize);
					this.BufferHead += copySize;
					copyHead += copySize;
					unfilled -= copySize;
					if (this.BufferHead >= springLength)
					{
						this.BufferHead = 0;
						this.TimesPlayed++;
					}
					if (this.LoopCount < 255 && this.TimesPlayed > this.LoopCount)
					{
						this.BufferFinished = true;
						break;
					}
				}
			}
			else
			{
				this.BufferFinished = true;
			}
			for (int i = 0; i < unfilled; i++)
			{
				this.BufferData[copyHead + i] = 0;
			}
		}
		if (this._filterEnabled)
		{
			this.FilterBuffer();
		}
		fixed (byte* bufferPtr2 = this.BufferData)
		{
			AL.alBufferData((uint)bufferId, (int)this._effect.SpringFormat, (IntPtr)bufferPtr2, 32768, this._effect.Spring.SampleRate);
		}
		AL.SourceQueueBuffers(this.SourceId, 1, new int[1] { bufferId });
		return true;
	}

	public void FilterBuffer()
	{
		if (this._filterEnabled)
		{
			int channels = 1;
			int bytesPerSample = 1;
			switch (this._effect.SpringFormat)
			{
			case ALFormat.Mono16:
				bytesPerSample = 2;
				break;
			case ALFormat.Stereo8:
				channels = 2;
				break;
			case ALFormat.Stereo16:
				channels = 2;
				bytesPerSample = 2;
				break;
			}
			if (bytesPerSample == 1)
			{
				this.FilterBuffer8(this.BufferData, channels, this._effect.Spring.SampleRate);
			}
			else
			{
				this.FilterBuffer16(this.BufferData, channels, this._effect.Spring.SampleRate);
			}
		}
	}

	public unsafe void FilterBuffer8(byte[] bufferArray, int channels, int sampleRate)
	{
		float filterFrequency = Math.Min((float)(2.0 * Math.Sin(Math.PI * (double)Math.Min(this.frequency / (float)sampleRate, 0.5f))), 1f);
		float oneOverQ = 1f / this.filterQ;
		float* f = stackalloc float[3];
		int stereo = ((channels != 0) ? 1 : 0);
		int clipLength = bufferArray.Length >> stereo;
		int filterMode = (int)this._filterMode;
		fixed (byte* bufferPtr = bufferArray)
		{
			for (int s = 0; s < clipLength; s++)
			{
				int baseIdx = s << stereo;
				for (int c = 0; c <= stereo; c++)
				{
					int sc = baseIdx + c;
					int fc = c << 1;
					int intSample = bufferPtr[sc];
					if ((intSample & 0x80) != 0)
					{
						intSample = -128 + (intSample & 0x7F);
					}
					float sample = (float)intSample / 128f;
					f[2] = sample - this.FilterState[fc + 1] - oneOverQ * this.FilterState[fc];
					f[1] = filterFrequency * f[2] + this.FilterState[fc];
					*f = filterFrequency * f[1] + this.FilterState[fc + 1];
					int final = (int)(f[filterMode] * 127f + 0.5f);
					if (final < -128)
					{
						final = -128;
					}
					else if (final > 127)
					{
						final = 127;
					}
					bufferPtr[sc] = (byte)final;
					this.FilterState[fc] = f[1];
					this.FilterState[fc + 1] = *f;
				}
			}
		}
	}

	public unsafe void FilterBuffer16(byte[] bufferArray, int channels, int sampleRate)
	{
		float filterFrequency = Math.Min((float)(2.0 * Math.Sin(Math.PI * (double)Math.Min(this.frequency / (float)sampleRate, 0.5f))), 1f);
		float oneOverQ = 1f / this.filterQ;
		float* f = stackalloc float[3];
		int stereo = ((channels != 0) ? 1 : 0);
		int clipLength = bufferArray.Length >> stereo + 1;
		int filterMode = (int)this._filterMode;
		fixed (byte* bufferPtr = bufferArray)
		{
			for (int s = 0; s < clipLength; s++)
			{
				int baseIdx = s << stereo + 1;
				for (int c = 0; c <= stereo; c++)
				{
					int sc = baseIdx + (c << 1);
					int fc = c << 1;
					int intSample = (bufferPtr[sc + 1] << 8) | bufferPtr[sc];
					if ((intSample & 0x8000) != 0)
					{
						intSample = -32768 + (intSample & 0x7FFF);
					}
					float sample = (float)intSample / 32768f;
					f[2] = sample - this.FilterState[fc + 1] - oneOverQ * this.FilterState[fc];
					f[1] = filterFrequency * f[2] + this.FilterState[fc];
					*f = filterFrequency * f[1] + this.FilterState[fc + 1];
					int final = (int)(f[filterMode] * 32767f + 0.5f);
					if (final < -32768)
					{
						final = -32768;
					}
					else if (final > 32767)
					{
						final = 32767;
					}
					bufferPtr[sc] = (byte)(final & 0xFF);
					bufferPtr[sc + 1] = (byte)(final >> 8);
					this.FilterState[fc] = f[1];
					this.FilterState[fc + 1] = *f;
				}
			}
		}
	}

	private void QueueBuffers()
	{
		if (!this.HasSourceId || !this.HasBufferIds || this.BufferFinished || this.BuffersAvailable == null)
		{
			return;
		}
		int availableCount = this.BuffersAvailable.Length;
		for (int i = 0; i < availableCount; i++)
		{
			this.QueueBuffer(this.BuffersAvailable[i]);
			if (this.BufferFinished)
			{
				break;
			}
		}
		this.BuffersAvailable = null;
	}

	private void UnqueueProcessedBuffers()
	{
		AL.GetSource(this.SourceId, ALGetSourcei.BuffersProcessed, out var processed);
		if (processed != 0)
		{
			this.BuffersAvailable = AL.SourceUnqueueBuffers(this.SourceId, processed);
		}
	}

	private void PlatformUpdateQueue()
	{
		lock (this.sourceMutex)
		{
			if (this.HasSourceId && this.HasBufferIds)
			{
				this.UnqueueProcessedBuffers();
				this.QueueBuffers();
				if (AL.GetSourceState(this.SourceId) == ALSourceState.Stopped)
				{
					AL.SourcePlay(this.SourceId);
				}
			}
		}
	}
}
