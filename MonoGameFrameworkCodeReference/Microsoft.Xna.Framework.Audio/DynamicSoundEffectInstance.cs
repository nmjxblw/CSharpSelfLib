using System;
using System.Collections.Generic;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>
/// A <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffectInstance" /> for which the audio buffer is provided by the game at run time.
/// </summary>
public class DynamicSoundEffectInstance : SoundEffectInstance
{
	private const int TargetPendingBufferCount = 3;

	private int _buffersNeeded;

	private int _sampleRate;

	private AudioChannels _channels;

	private SoundState _state;

	private int _sampleAlignment;

	private bool _msadpcm;

	private bool _looped;

	private readonly object _queueLocker = new object();

	private Queue<OALSoundBuffer> _queuedBuffers;

	private ALFormat _format;

	private bool _finishedQueueing;

	public override SoundState State
	{
		get
		{
			this.AssertNotDisposed();
			return this._state;
		}
	}

	/// <summary>
	/// Returns the number of audio buffers queued for playback.
	/// </summary>
	public int PendingBufferCount
	{
		get
		{
			this.AssertNotDisposed();
			return this.PlatformGetPendingBufferCount();
		}
	}

	/// <summary>
	/// The event that occurs when the number of queued audio buffers is less than or equal to 2.
	/// </summary>
	/// <remarks>
	/// This event may occur when <see cref="M:Microsoft.Xna.Framework.Audio.DynamicSoundEffectInstance.Play" /> is called or during playback when a buffer is completed.
	/// </remarks>
	public event EventHandler<EventArgs> BufferNeeded;

	/// <param name="sampleRate">Sample rate, in Hertz (Hz).</param>
	/// <param name="channels">Number of channels (mono or stereo).</param>
	public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
	{
		this.Construct(msadpcm: false, 0, sampleRate, channels);
	}

	internal DynamicSoundEffectInstance(bool msadpcm, int blockAlignment, int sampleRate, AudioChannels channels)
	{
		this.Construct(msadpcm, blockAlignment, sampleRate, channels);
	}

	private void Construct(bool msadpcm, int blockAlignment, int sampleRate, AudioChannels channels)
	{
		if (sampleRate < 8000 || sampleRate > 48000)
		{
			throw new ArgumentOutOfRangeException("sampleRate");
		}
		if (channels != AudioChannels.Mono && channels != AudioChannels.Stereo)
		{
			throw new ArgumentOutOfRangeException("channels");
		}
		this._msadpcm = msadpcm;
		this._sampleRate = sampleRate;
		this._channels = channels;
		this._state = SoundState.Stopped;
		this._sampleAlignment = blockAlignment;
		this.PlatformCreate();
		base._isPooled = false;
		base._isDynamic = true;
	}

	/// <summary>
	/// Returns the duration of an audio buffer of the specified size, based on the settings of this instance.
	/// </summary>
	/// <param name="sizeInBytes">Size of the buffer, in bytes.</param>
	/// <returns>The playback length of the buffer.</returns>
	public TimeSpan GetSampleDuration(int sizeInBytes)
	{
		this.AssertNotDisposed();
		return SoundEffect.GetSampleDuration(sizeInBytes, this._sampleRate, this._channels);
	}

	/// <summary>
	/// Returns the size, in bytes, of a buffer of the specified duration, based on the settings of this instance.
	/// </summary>
	/// <param name="duration">The playback length of the buffer.</param>
	/// <returns>The data size of the buffer, in bytes.</returns>
	public int GetSampleSizeInBytes(TimeSpan duration)
	{
		this.AssertNotDisposed();
		return SoundEffect.GetSampleSizeInBytes(duration, this._sampleRate, this._channels);
	}

	/// <summary>
	/// Plays or resumes the DynamicSoundEffectInstance.
	/// </summary>
	public override void Play()
	{
		this.AssertNotDisposed();
		if (this._state == SoundState.Playing)
		{
			return;
		}
		if (this._state == SoundState.Paused)
		{
			this.Resume();
			return;
		}
		base.Volume = base.Volume;
		if (!SoundEffectInstancePool.SoundsAvailable)
		{
			throw new InstancePlayLimitException();
		}
		SoundEffectInstancePool.Remove(this);
		this.PlatformPlay();
		this._state = SoundState.Playing;
		this.CheckBufferCount();
		DynamicSoundEffectInstanceManager.AddInstance(this);
	}

	/// <summary>
	/// Pauses playback of the DynamicSoundEffectInstance.
	/// </summary>
	public override void Pause()
	{
		this.AssertNotDisposed();
		this.PlatformPause();
		this._state = SoundState.Paused;
	}

	/// <summary>
	/// Resumes playback of the DynamicSoundEffectInstance.
	/// </summary>
	public override void Resume()
	{
		this.AssertNotDisposed();
		if (this._state != SoundState.Playing && this._state != SoundState.Paused)
		{
			base.Volume = base.Volume;
			if (!SoundEffectInstancePool.SoundsAvailable)
			{
				throw new InstancePlayLimitException();
			}
			SoundEffectInstancePool.Remove(this);
		}
		this.PlatformResume();
		this._state = SoundState.Playing;
	}

	/// <summary>
	/// Immediately stops playing the DynamicSoundEffectInstance.
	/// </summary>
	/// <remarks>
	/// Calling this also releases all queued buffers.
	/// </remarks>
	public override void Stop()
	{
		this.Stop(immediate: true);
	}

	/// <summary>
	/// Stops playing the DynamicSoundEffectInstance.
	/// If the <paramref name="immediate" /> parameter is false, this call has no effect.
	/// </summary>
	/// <remarks>
	/// Calling this also releases all queued buffers.
	/// </remarks>
	/// <param name="immediate">When set to false, this call has no effect.</param>
	public override void Stop(bool immediate)
	{
		this.AssertNotDisposed();
		if (immediate)
		{
			DynamicSoundEffectInstanceManager.RemoveInstance(this);
			lock (this._queueLocker)
			{
				this.PlatformStop();
			}
			this._state = SoundState.Stopped;
			SoundEffectInstancePool.Add(this);
		}
	}

	/// <summary>
	/// Queues an audio buffer for playback.
	/// </summary>
	/// <remarks>
	/// The buffer length must conform to alignment requirements for the audio format.
	/// </remarks>
	/// <param name="buffer">The buffer containing PCM audio data.</param>
	public void SubmitBuffer(byte[] buffer)
	{
		this.AssertNotDisposed();
		if (buffer.Length == 0)
		{
			throw new ArgumentException("Buffer may not be empty.");
		}
		int sampleSize = 2 * (int)this._channels;
		if (buffer.Length % sampleSize != 0)
		{
			throw new ArgumentException("Buffer length does not match format alignment.");
		}
		this.SubmitBuffer(buffer, 0, buffer.Length);
	}

	/// <summary>
	/// Queues an audio buffer for playback.
	/// </summary>
	/// <remarks>
	/// The buffer length must conform to alignment requirements for the audio format.
	/// </remarks>
	/// <param name="buffer">The buffer containing PCM audio data.</param>
	/// <param name="offset">The starting position of audio data.</param>
	/// <param name="count">The amount of bytes to use.</param>
	public void SubmitBuffer(byte[] buffer, int offset, int count)
	{
		this.AssertNotDisposed();
		if (buffer == null || buffer.Length == 0)
		{
			throw new ArgumentException("Buffer may not be null or empty.");
		}
		if (count <= 0)
		{
			throw new ArgumentException("Number of bytes must be greater than zero.");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentException("Buffer is shorter than the specified number of bytes from the offset.");
		}
		int sampleSize = 2 * (int)this._channels;
		if (count % sampleSize != 0)
		{
			throw new ArgumentException("Number of bytes does not match format alignment.");
		}
		if (offset % sampleSize != 0)
		{
			throw new ArgumentException("Offset into the buffer does not match format alignment.");
		}
		lock (this._queueLocker)
		{
			this.PlatformSubmitBuffer(buffer, offset, count);
		}
	}

	private void AssertNotDisposed()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(null);
		}
	}

	protected override void Dispose(bool disposing)
	{
		lock (this._queueLocker)
		{
			this.PlatformDispose(disposing);
			base.Dispose(disposing);
		}
	}

	private void CheckBufferCount()
	{
		if (this.PendingBufferCount < 3 && this._state == SoundState.Playing)
		{
			this._buffersNeeded = 3 - this.PendingBufferCount;
		}
	}

	internal override void UpdateQueue()
	{
		lock (this._queueLocker)
		{
			if (base.IsDisposed)
			{
				return;
			}
			this.PlatformUpdateQueue();
			EventHandler<EventArgs> bufferNeededHandler = this.BufferNeeded;
			if (bufferNeededHandler != null)
			{
				int eventCount = ((this._buffersNeeded < 3) ? this._buffersNeeded : 3);
				for (int i = 0; i < eventCount; i++)
				{
					bufferNeededHandler(this, EventArgs.Empty);
				}
			}
			this._buffersNeeded = 0;
		}
	}

	public void FinishedQueueing()
	{
		this._finishedQueueing = true;
	}

	private void PlatformCreate()
	{
		if (this._msadpcm)
		{
			this._format = ((this._channels == AudioChannels.Mono) ? ALFormat.MonoMSAdpcm : ALFormat.StereoMSAdpcm);
			this._sampleAlignment = AudioLoader.SampleAlignment(this._format, this._sampleAlignment);
		}
		else
		{
			this._format = ((this._channels == AudioChannels.Mono) ? ALFormat.Mono16 : ALFormat.Stereo16);
			this._sampleAlignment = 0;
		}
		base.InitializeSound();
		base.SourceId = base.controller.ReserveSource();
		base.HasSourceId = true;
		this._queuedBuffers = new Queue<OALSoundBuffer>();
	}

	private int PlatformGetPendingBufferCount()
	{
		return this._queuedBuffers?.Count ?? 0;
	}

	private void PlatformPlay()
	{
		this._finishedQueueing = false;
		AL.GetError();
		AL.Source(base.SourceId, ALSourceb.Looping, a: false);
		AL.SourcePlay(base.SourceId);
	}

	private void PlatformPause()
	{
		AL.GetError();
		AL.SourcePause(base.SourceId);
	}

	private void PlatformResume()
	{
		AL.GetError();
		AL.SourcePlay(base.SourceId);
	}

	private void PlatformStop()
	{
		this._finishedQueueing = true;
		AL.GetError();
		AL.SourceStop(base.SourceId);
		AL.Source(base.SourceId, ALSourcei.Buffer, 0);
		while (true)
		{
			Queue<OALSoundBuffer> queuedBuffers = this._queuedBuffers;
			if (queuedBuffers != null && queuedBuffers.Count > 0)
			{
				this._queuedBuffers.Dequeue()?.Dispose();
				continue;
			}
			break;
		}
	}

	private void PlatformSubmitBuffer(byte[] buffer, int offset, int count)
	{
		OALSoundBuffer oalBuffer = new OALSoundBuffer();
		byte[] offsetBuffer = buffer;
		if (offset != 0)
		{
			offsetBuffer = new byte[count];
			Array.Copy(buffer, offset, offsetBuffer, 0, count);
		}
		if (base.IsFilterEnabled())
		{
			switch (this._format)
			{
			case ALFormat.Mono8:
			case ALFormat.Stereo8:
				base.FilterBuffer8(offsetBuffer, (int)this._channels, this._sampleRate);
				break;
			case ALFormat.Mono16:
			case ALFormat.Stereo16:
				base.FilterBuffer16(offsetBuffer, (int)this._channels, this._sampleRate);
				break;
			}
		}
		oalBuffer.BindDataBuffer(offsetBuffer, this._format, count, this._sampleRate, this._sampleAlignment);
		if (this._queuedBuffers != null)
		{
			this._queuedBuffers.Enqueue(oalBuffer);
			AL.SourceQueueBuffer(base.SourceId, oalBuffer.OpenALDataBuffer);
		}
		ALSourceState sourceState = AL.GetSourceState(base.SourceId);
		if (this._state == SoundState.Playing && sourceState == ALSourceState.Stopped)
		{
			AL.SourcePlay(base.SourceId);
		}
	}

	private void PlatformDispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!disposing)
		{
			return;
		}
		while (true)
		{
			Queue<OALSoundBuffer> queuedBuffers = this._queuedBuffers;
			if (queuedBuffers == null || queuedBuffers.Count <= 0)
			{
				break;
			}
			this._queuedBuffers.Dequeue()?.Dispose();
		}
		DynamicSoundEffectInstanceManager.RemoveInstance(this);
	}

	private void PlatformUpdateQueue()
	{
		AL.GetError();
		AL.GetSource(base.SourceId, ALGetSourcei.BuffersProcessed, out var numBuffers);
		if (numBuffers > 0)
		{
			AL.SourceUnqueueBuffers(base.SourceId, numBuffers);
			for (int i = 0; i < numBuffers; i++)
			{
				(this._queuedBuffers?.Dequeue())?.Dispose();
			}
		}
		for (int j = 0; j < numBuffers; j++)
		{
			this.CheckBufferCount();
		}
		if (this._queuedBuffers.Count == 0 && this._finishedQueueing)
		{
			this.Stop(immediate: true);
		}
	}
}
