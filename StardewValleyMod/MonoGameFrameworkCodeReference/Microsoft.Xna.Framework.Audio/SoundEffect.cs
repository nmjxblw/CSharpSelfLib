using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.OpenAL;
using NVorbis;
using RTAudioProcessing;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>Represents a loaded sound resource.</summary>
/// <remarks>
/// <para>A SoundEffect represents the buffer used to hold audio data and metadata. SoundEffectInstances are used to play from SoundEffects. Multiple SoundEffectInstance objects can be created and played from the same SoundEffect object.</para>
/// <para>The only limit on the number of loaded SoundEffects is restricted by available memory. When a SoundEffect is disposed, all SoundEffectInstances created from it will become invalid.</para>
/// <para>SoundEffect.Play() can be used for 'fire and forget' sounds. If advanced playback controls like volume or pitch is required, use SoundEffect.CreateInstance().</para>
/// </remarks>
public class SoundEffect : IDisposable
{
	internal enum SoundSystemState
	{
		NotInitialized,
		Initialized,
		FailedToInitialized
	}

	private string _name = string.Empty;

	protected bool _isDisposed;

	protected TimeSpan _duration;

	public static HashSet<SoundEffect> EffectsToRemove = new HashSet<SoundEffect>();

	protected int _dependencies;

	protected bool _waveBankSound;

	internal static SoundSystemState _systemState = SoundSystemState.NotInitialized;

	private static float _masterVolume = 1f;

	private static float _distanceScale = 1f;

	private static float _dopplerScale = 1f;

	private static float speedOfSound = 343.5f;

	internal const int MAX_PLAYING_INSTANCES = 256;

	internal static uint ReverbSlot = 0u;

	internal static uint ReverbEffect = 0u;

	internal RtapSpring Spring;

	internal ALFormat SpringFormat = ALFormat.Mono8;

	/// <summary>Gets the duration of the SoundEffect.</summary>
	public TimeSpan Duration => this._duration;

	/// <summary>Gets or sets the asset name of the SoundEffect.</summary>
	public string Name
	{
		get
		{
			return this._name;
		}
		set
		{
			this._name = value;
		}
	}

	/// <summary>
	/// Gets or sets the master volume scale applied to all SoundEffectInstances.
	/// </summary>
	/// <remarks>
	/// <para>Each SoundEffectInstance has its own Volume property that is independent to SoundEffect.MasterVolume. During playback SoundEffectInstance.Volume is multiplied by SoundEffect.MasterVolume.</para>
	/// <para>This property is used to adjust the volume on all current and newly created SoundEffectInstances. The volume of an individual SoundEffectInstance can be adjusted on its own.</para>
	/// </remarks>
	public static float MasterVolume
	{
		get
		{
			return SoundEffect._masterVolume;
		}
		set
		{
			if (value < 0f || value > 1f)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (SoundEffect._masterVolume != value)
			{
				SoundEffect._masterVolume = value;
				SoundEffectInstancePool.UpdateMasterVolume();
			}
		}
	}

	/// <summary>
	/// Gets or sets the scale of distance calculations.
	/// </summary>
	/// <remarks> 
	/// <para>DistanceScale defaults to 1.0 and must be greater than 0.0.</para>
	/// <para>Higher values reduce the rate of falloff between the sound and listener.</para>
	/// </remarks>
	public static float DistanceScale
	{
		get
		{
			return SoundEffect._distanceScale;
		}
		set
		{
			if (value <= 0f)
			{
				throw new ArgumentOutOfRangeException("value", "value of DistanceScale");
			}
			SoundEffect._distanceScale = value;
		}
	}

	/// <summary>
	/// Gets or sets the scale of Doppler calculations applied to sounds.
	/// </summary>
	/// <remarks>
	/// <para>DopplerScale defaults to 1.0 and must be greater or equal to 0.0</para>
	/// <para>Affects the relative velocity of emitters and listeners.</para>
	/// <para>Higher values more dramatically shift the pitch for the given relative velocity of the emitter and listener.</para>
	/// </remarks>
	public static float DopplerScale
	{
		get
		{
			return SoundEffect._dopplerScale;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("value", "value of DopplerScale");
			}
			SoundEffect._dopplerScale = value;
		}
	}

	/// <summary>Returns the speed of sound used when calculating the Doppler effect..</summary>
	/// <remarks>
	/// <para>Defaults to 343.5. Value is measured in meters per second.</para>
	/// <para>Has no effect on distance attenuation.</para>
	/// </remarks>
	public static float SpeedOfSound
	{
		get
		{
			return SoundEffect.speedOfSound;
		}
		set
		{
			if (value <= 0f)
			{
				throw new ArgumentOutOfRangeException();
			}
			SoundEffect.speedOfSound = value;
		}
	}

	/// <summary>Indicates whether the object is disposed.</summary>
	public bool IsDisposed => this._isDisposed;

	internal SoundEffect()
	{
		SoundEffect.Initialize();
		if (SoundEffect._systemState != SoundSystemState.Initialized)
		{
			throw new NoAudioHardwareException("Audio has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
		}
	}

	internal SoundEffect(Stream stream, bool vorbis = false)
	{
		SoundEffect.Initialize();
		if (SoundEffect._systemState != SoundSystemState.Initialized)
		{
			throw new NoAudioHardwareException("Audio has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
		}
		if (vorbis)
		{
			using (VorbisReader vorbis_reader = new VorbisReader(stream, closeStreamOnDispose: true))
			{
				int bytes_per_sample = 2;
				float[] float_buffer = new float[vorbis_reader.TotalSamples * vorbis_reader.Channels];
				short[] cast_buffer = new short[float_buffer.Length];
				byte[] xna_buffer = new byte[float_buffer.Length * bytes_per_sample];
				int read_samples = vorbis_reader.ReadSamples(float_buffer, 0, float_buffer.Length);
				OggStream.CastBuffer(float_buffer, cast_buffer, read_samples);
				Buffer.BlockCopy(cast_buffer, 0, xna_buffer, 0, read_samples * bytes_per_sample);
				this._duration = vorbis_reader.TotalTime;
				this.PlatformInitializePcm(xna_buffer, 0, xna_buffer.Length, 16, vorbis_reader.SampleRate, (AudioChannels)vorbis_reader.Channels, 0, (int)vorbis_reader.TotalSamples);
				return;
			}
		}
		this.PlatformLoadAudioStream(stream, out this._duration);
	}

	internal SoundEffect(byte[] header, byte[] buffer, int bufferSize, int durationMs, int loopStart, int loopLength)
	{
		SoundEffect.Initialize();
		if (SoundEffect._systemState != SoundSystemState.Initialized)
		{
			throw new NoAudioHardwareException("Audio has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
		}
		using MemoryStream stream = new MemoryStream(buffer);
		this.PlatformLoadAudioStream(stream, out this._duration);
	}

	internal SoundEffect(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength)
	{
		SoundEffect.Initialize();
		this._waveBankSound = true;
		if (SoundEffect._systemState != SoundSystemState.Initialized)
		{
			throw new NoAudioHardwareException("Audio has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
		}
		if (codec == MiniFormatTag.Pcm)
		{
			this._duration = TimeSpan.FromSeconds((float)buffer.Length / (float)(sampleRate * blockAlignment));
			this.PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, (AudioChannels)channels, loopStart, loopLength);
		}
		else
		{
			this.PlatformInitializeXact(codec, buffer, channels, sampleRate, blockAlignment, loopStart, loopLength, out this._duration);
		}
	}

	/// <summary>
	/// Initializes the sound system for SoundEffect support.
	/// This method is automatically called when a SoundEffect is loaded, a DynamicSoundEffectInstance is created, or Microphone.All is queried.
	/// You can however call this method manually (preferably in, or before the Game constructor) to catch any Exception that may occur during the sound system initialization (and act accordingly).
	/// </summary>
	public static void Initialize()
	{
		if (SoundEffect._systemState != SoundSystemState.NotInitialized)
		{
			return;
		}
		try
		{
			SoundEffect.PlatformInitialize();
			SoundEffect._systemState = SoundSystemState.Initialized;
		}
		catch (Exception)
		{
			SoundEffect._systemState = SoundSystemState.FailedToInitialized;
			throw;
		}
	}

	/// <summary>
	/// Create a sound effect.
	/// </summary>
	/// <param name="buffer">The buffer with the sound data.</param>
	/// <param name="sampleRate">The sound data sample rate in hertz.</param>
	/// <param name="channels">The number of channels in the sound data.</param>
	/// <remarks>This only supports uncompressed 16bit PCM wav data.</remarks>
	public SoundEffect(byte[] buffer, int sampleRate, AudioChannels channels)
		: this(buffer, 0, (buffer != null) ? buffer.Length : 0, sampleRate, channels, 0, 0)
	{
	}

	/// <summary>
	/// Create a sound effect.
	/// </summary>
	/// <param name="buffer">The buffer with the sound data.</param>
	/// <param name="offset">The offset to the start of the sound data in bytes.</param>
	/// <param name="count">The length of the sound data in bytes.</param>
	/// <param name="sampleRate">The sound data sample rate in hertz.</param>
	/// <param name="channels">The number of channels in the sound data.</param>
	/// <param name="loopStart">The position where the sound should begin looping in samples.</param>
	/// <param name="loopLength">The duration of the sound data loop in samples.</param>
	/// <remarks>This only supports uncompressed 16bit PCM wav data.</remarks>
	public SoundEffect(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
	{
		SoundEffect.Initialize();
		if (SoundEffect._systemState != SoundSystemState.Initialized)
		{
			throw new NoAudioHardwareException("Audio has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
		}
		if (sampleRate < 8000 || sampleRate > 48000)
		{
			throw new ArgumentOutOfRangeException("sampleRate");
		}
		if (channels != AudioChannels.Mono && channels != AudioChannels.Stereo)
		{
			throw new ArgumentOutOfRangeException("channels");
		}
		if (buffer == null || buffer.Length == 0)
		{
			throw new ArgumentException("Ensure that the buffer length is non-zero.", "buffer");
		}
		int blockAlign = (int)channels * 2;
		if (count % blockAlign != 0)
		{
			throw new ArgumentException("Ensure that the buffer meets the block alignment requirements for the number of channels.", "buffer");
		}
		if (count <= 0)
		{
			throw new ArgumentException("Ensure that the count is greater than zero.", "count");
		}
		if (count % blockAlign != 0)
		{
			throw new ArgumentException("Ensure that the count meets the block alignment requirements for the number of channels.", "count");
		}
		if (offset < 0)
		{
			throw new ArgumentException("The offset cannot be negative.", "offset");
		}
		if ((ulong)((long)count + (long)offset) > (ulong)buffer.Length)
		{
			throw new ArgumentException("Ensure that the offset+count region lines within the buffer.", "offset");
		}
		int totalSamples = count / blockAlign;
		if (loopStart < 0)
		{
			throw new ArgumentException("The loopStart cannot be negative.", "loopStart");
		}
		if (loopStart > totalSamples)
		{
			throw new ArgumentException("The loopStart cannot be greater than the total number of samples.", "loopStart");
		}
		if (loopLength == 0)
		{
			loopLength = totalSamples - loopStart;
		}
		if (loopLength < 0)
		{
			throw new ArgumentException("The loopLength cannot be negative.", "loopLength");
		}
		if ((ulong)((long)loopStart + (long)loopLength) > (ulong)totalSamples)
		{
			throw new ArgumentException("Ensure that the loopStart+loopLength region lies within the sample range.", "loopLength");
		}
		this._duration = SoundEffect.GetSampleDuration(count, sampleRate, channels);
		this.PlatformInitializePcm(buffer, offset, count, 16, sampleRate, channels, loopStart, loopLength);
	}

	/// <summary>
	/// Releases unmanaged resources and performs other cleanup operations before the
	/// <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffect" /> is reclaimed by garbage collection.
	/// </summary>
	~SoundEffect()
	{
		this.Dispose(disposing: false);
	}

	/// <summary>
	/// Creates a new SoundEffectInstance for this SoundEffect.
	/// </summary>
	/// <returns>A new SoundEffectInstance for this SoundEffect.</returns>
	/// <remarks>Creating a SoundEffectInstance before calling SoundEffectInstance.Play() allows you to access advanced playback features, such as volume, pitch, and 3D positioning.</remarks>
	public SoundEffectInstance CreateInstance()
	{
		SoundEffectInstance inst = new SoundEffectInstance();
		this.PlatformSetupInstance(inst);
		inst._isPooled = false;
		inst._effect = this;
		return inst;
	}

	public void AddDependency()
	{
		if (!this._waveBankSound)
		{
			this._dependencies++;
		}
	}

	public void RemoveDependency()
	{
		if (!this._waveBankSound)
		{
			this._dependencies--;
			SoundEffect.EffectsToRemove.Add(this);
		}
	}

	public bool ShouldBeRemoved()
	{
		if (this._waveBankSound)
		{
			return false;
		}
		return this._dependencies == 0;
	}

	/// <summary>
	/// Creates a new SoundEffect object based on the specified data stream.
	/// This internally calls <see cref="M:Microsoft.Xna.Framework.Audio.SoundEffect.FromStream(System.IO.Stream,System.Boolean)" />.
	/// </summary>
	/// <param name="path">The path to the audio file.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffect" /> loaded from the given file.</returns>
	/// <remarks>The stream must point to the head of a valid wave file in the RIFF bitstream format.  The formats supported are:
	/// <list type="bullet">
	/// <item>
	/// <description>8-bit unsigned PCM</description>
	/// <description>16-bit signed PCM</description>
	/// <description>24-bit signed PCM</description>
	/// <description>32-bit IEEE float PCM</description>
	/// <description>MS-ADPCM 4-bit compressed</description>
	/// <description>IMA/ADPCM (IMA4) 4-bit compressed</description>
	/// </item>
	/// </list>
	/// </remarks>
	public static SoundEffect FromFile(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		using FileStream stream = File.OpenRead(path);
		return SoundEffect.FromStream(stream);
	}

	/// <summary>
	/// Creates a new SoundEffect object based on the specified data stream.
	/// </summary>
	/// <param name="stream">A stream containing the wave data.</param>
	/// <returns>A new SoundEffect object.</returns>
	/// <remarks>The stream must point to the head of a valid wave file in the RIFF bitstream format.  The formats supported are:
	/// <list type="bullet">
	/// <item>
	/// <description>8-bit unsigned PCM</description>
	/// <description>16-bit signed PCM</description>
	/// <description>24-bit signed PCM</description>
	/// <description>32-bit IEEE float PCM</description>
	/// <description>MS-ADPCM 4-bit compressed</description>
	/// <description>IMA/ADPCM (IMA4) 4-bit compressed</description>
	/// </item>
	/// </list>
	/// </remarks>
	public static SoundEffect FromStream(Stream stream, bool vorbis = false)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return new SoundEffect(stream, vorbis);
	}

	/// <summary>
	/// Returns the duration for 16-bit PCM audio.
	/// </summary>
	/// <param name="sizeInBytes">The length of the audio data in bytes.</param>
	/// <param name="sampleRate">Sample rate, in Hertz (Hz). Must be between 8000 Hz and 48000 Hz</param>
	/// <param name="channels">Number of channels in the audio data.</param>
	/// <returns>The duration of the audio data.</returns>
	public static TimeSpan GetSampleDuration(int sizeInBytes, int sampleRate, AudioChannels channels)
	{
		if (sizeInBytes < 0)
		{
			throw new ArgumentException("Buffer size cannot be negative.", "sizeInBytes");
		}
		if (sampleRate < 8000 || sampleRate > 48000)
		{
			throw new ArgumentOutOfRangeException("sampleRate");
		}
		if (channels != AudioChannels.Mono && channels != AudioChannels.Stereo)
		{
			throw new ArgumentOutOfRangeException("channels");
		}
		if (sizeInBytes == 0)
		{
			return TimeSpan.Zero;
		}
		return TimeSpan.FromSeconds((float)sizeInBytes / ((float)(sampleRate * (int)channels) * 16f / 8f));
	}

	/// <summary>
	/// Returns the data size in bytes for 16bit PCM audio.
	/// </summary>
	/// <param name="duration">The total duration of the audio data.</param>
	/// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio data. Must be between 8,000 and 48,000 Hz.</param>
	/// <param name="channels">Number of channels in the audio data.</param>
	/// <returns>The size in bytes of a single sample of audio data.</returns>
	public static int GetSampleSizeInBytes(TimeSpan duration, int sampleRate, AudioChannels channels)
	{
		if (duration < TimeSpan.Zero || duration > TimeSpan.FromMilliseconds(134217727.0))
		{
			throw new ArgumentOutOfRangeException("duration");
		}
		if (sampleRate < 8000 || sampleRate > 48000)
		{
			throw new ArgumentOutOfRangeException("sampleRate");
		}
		if (channels != AudioChannels.Mono && channels != AudioChannels.Stereo)
		{
			throw new ArgumentOutOfRangeException("channels");
		}
		return (int)(duration.TotalSeconds * (double)((float)(sampleRate * (int)channels) * 16f / 8f));
	}

	/// <summary>Gets an internal SoundEffectInstance and plays it.</summary>
	/// <returns>True if a SoundEffectInstance was successfully played, false if not.</returns>
	/// <remarks>
	/// <para>Play returns false if more SoundEffectInstances are currently playing then the platform allows.</para>
	/// <para>To loop a sound or apply 3D effects, call SoundEffect.CreateInstance() and SoundEffectInstance.Play() instead.</para>
	/// <para>SoundEffectInstances used by SoundEffect.Play() are pooled internally.</para>
	/// </remarks>
	public bool Play()
	{
		SoundEffectInstance inst = this.GetPooledInstance(forXAct: false);
		if (inst == null)
		{
			return false;
		}
		inst.Play();
		return true;
	}

	/// <summary>Gets an internal SoundEffectInstance and plays it with the specified volume, pitch, and panning.</summary>
	/// <returns>True if a SoundEffectInstance was successfully created and played, false if not.</returns>
	/// <param name="volume">Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled by SoundEffect.MasterVolume.</param>
	/// <param name="pitch">Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).</param>
	/// <param name="pan">Panning, ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).</param>
	/// <remarks>
	/// <para>Play returns false if more SoundEffectInstances are currently playing then the platform allows.</para>
	/// <para>To apply looping or simulate 3D audio, call SoundEffect.CreateInstance() and SoundEffectInstance.Play() instead.</para>
	/// <para>SoundEffectInstances used by SoundEffect.Play() are pooled internally.</para>
	/// </remarks>
	public bool Play(float volume, float pitch, float pan)
	{
		SoundEffectInstance inst = this.GetPooledInstance(forXAct: false);
		if (inst == null)
		{
			return false;
		}
		inst.Volume = volume;
		inst.Pitch = pitch;
		inst.Pan = pan;
		inst.Play();
		return true;
	}

	/// <summary>
	/// Returns a sound effect instance from the pool or null if none are available.
	/// </summary>
	public virtual SoundEffectInstance GetPooledInstance(bool forXAct)
	{
		if (!SoundEffectInstancePool.SoundsAvailable)
		{
			return null;
		}
		SoundEffectInstance inst = SoundEffectInstancePool.GetInstance(forXAct);
		inst._effect = this;
		this.PlatformSetupInstance(inst);
		return inst;
	}

	/// <summary>Releases the resources held by this <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffect" />.</summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases the resources held by this <see cref="T:Microsoft.Xna.Framework.Audio.SoundEffect" />.
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
			SoundEffectInstancePool.StopPooledInstances(this);
			this.PlatformDispose(disposing);
			this._isDisposed = true;
		}
	}

	private static ALFormat RtapToAlFormat(RtapFormat rtapFormat)
	{
		switch (rtapFormat)
		{
		case RtapFormat.Mono16:
		case RtapFormat.MonoMSAdpcm:
			return ALFormat.Mono16;
		case RtapFormat.Stereo8:
			return ALFormat.Stereo8;
		case RtapFormat.Stereo16:
		case RtapFormat.StereoMSAdpcm:
			return ALFormat.Stereo16;
		default:
			return ALFormat.Mono8;
		}
	}

	private void PlatformLoadAudioStream(Stream stream, out TimeSpan duration)
	{
		ALFormat format;
		int freq;
		int channels;
		int blockAlignment;
		int bitsPerSample;
		int samplesPerBlock;
		int sampleCount;
		byte[] buffer = AudioLoader.Load(stream, out format, out freq, out channels, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);
		duration = TimeSpan.FromSeconds((float)sampleCount / (float)freq);
		this.PlatformInitializeBuffer(buffer, buffer.Length, format, channels, freq, blockAlignment, bitsPerSample, 0, 0);
	}

	private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
	{
		if (sampleBits == 24)
		{
			buffer = AudioLoader.Convert24To16(buffer, offset, count);
			offset = 0;
			count = buffer.Length;
			sampleBits = 16;
		}
		bool stereo = channels == AudioChannels.Stereo;
		RtapFormat rtapFormat = RtapFormat.Mono8;
		if (sampleBits == 16)
		{
			rtapFormat = ((!stereo) ? RtapFormat.Mono16 : RtapFormat.Stereo16);
		}
		else if (stereo)
		{
			rtapFormat = RtapFormat.Stereo8;
		}
		this.SpringFormat = SoundEffect.RtapToAlFormat(rtapFormat);
		this.Spring = new RtapSpring(buffer, rtapFormat, sampleRate, sampleBits / 8 * (int)(channels + 1));
	}

	private void PlatformInitializeIeeeFloat(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
	{
		buffer = AudioLoader.ConvertFloatTo16(buffer, offset, count);
		this.PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
	}

	private void PlatformInitializeAdpcm(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
	{
		RtapFormat rtapFormat = ((channels == AudioChannels.Stereo) ? RtapFormat.StereoMSAdpcm : RtapFormat.MonoMSAdpcm);
		this.SpringFormat = SoundEffect.RtapToAlFormat(rtapFormat);
		this.Spring = new RtapSpring(buffer, rtapFormat, sampleRate, blockAlignment);
	}

	private void PlatformInitializeIma4(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int blockAlignment, int loopStart, int loopLength)
	{
		buffer = AudioLoader.ConvertIma4ToPcm(buffer, offset, count, (int)channels, blockAlignment);
		this.PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
	}

	private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength)
	{
		short format = BitConverter.ToInt16(header, 0);
		short channels = BitConverter.ToInt16(header, 2);
		int sampleRate = BitConverter.ToInt32(header, 4);
		short blockAlignment = BitConverter.ToInt16(header, 12);
		short bitsPerSample = BitConverter.ToInt16(header, 14);
		ALFormat format2 = AudioLoader.GetSoundFormat(format, channels, bitsPerSample);
		this.PlatformInitializeBuffer(buffer, bufferSize, format2, channels, sampleRate, blockAlignment, bitsPerSample, loopStart, loopLength);
	}

	private void PlatformInitializeBuffer(byte[] buffer, int bufferSize, ALFormat format, int channels, int sampleRate, int blockAlignment, int bitsPerSample, int loopStart, int loopLength)
	{
		switch (format)
		{
		case ALFormat.Mono8:
		case ALFormat.Mono16:
		case ALFormat.Stereo8:
		case ALFormat.Stereo16:
			this.PlatformInitializePcm(buffer, 0, bufferSize, bitsPerSample, sampleRate, (AudioChannels)channels, loopStart, loopLength);
			break;
		case ALFormat.MonoMSAdpcm:
		case ALFormat.StereoMSAdpcm:
			this.PlatformInitializeAdpcm(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
			break;
		case ALFormat.MonoFloat32:
		case ALFormat.StereoFloat32:
			this.PlatformInitializeIeeeFloat(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, loopStart, loopLength);
			break;
		case ALFormat.MonoIma4:
		case ALFormat.StereoIma4:
			this.PlatformInitializeIma4(buffer, 0, bufferSize, sampleRate, (AudioChannels)channels, blockAlignment, loopStart, loopLength);
			break;
		default:
			throw new NotSupportedException("Unsupported wave format!");
		}
	}

	private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration)
	{
		if (codec == MiniFormatTag.Adpcm)
		{
			this.PlatformInitializeAdpcm(buffer, 0, buffer.Length, sampleRate, (AudioChannels)channels, (blockAlignment + 22) * channels, loopStart, loopLength);
			duration = TimeSpan.FromSeconds(this.Spring.Duration);
			return;
		}
		throw new NotSupportedException("Unsupported sound format!");
	}

	private void PlatformSetupInstance(SoundEffectInstance inst)
	{
		inst.InitializeSound();
	}

	internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings)
	{
		if (OpenALSoundController.Efx.IsInitialized && SoundEffect.ReverbEffect == 0)
		{
			EffectsExtension efx = OpenALSoundController.Efx;
			efx.GenAuxiliaryEffectSlots(1, out SoundEffect.ReverbSlot);
			efx.GenEffect(out SoundEffect.ReverbEffect);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffecti.EffectType, 32768);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbReflectionsDelay, reverbSettings.ReflectionsDelayMs / 1000f);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.LateReverbDelay, reverbSettings.ReverbDelayMs / 1000f);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbDiffusion, reverbSettings.EarlyDiffusion / 15f);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbDiffusion, reverbSettings.LateDiffusion / 15f);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbGainLF, Math.Min(XactHelpers.ParseVolumeFromDecibels(reverbSettings.LowEqGain - 8f), 1f));
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbLFReference, reverbSettings.LowEqCutoff * 50f + 50f);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbGainHF, XactHelpers.ParseVolumeFromDecibels(reverbSettings.HighEqGain - 8f));
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbHFReference, reverbSettings.HighEqCutoff * 500f + 1000f);
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbReflectionsGain, Math.Min(XactHelpers.ParseVolumeFromDecibels(reverbSettings.ReflectionsGainDb), 3.16f));
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbGain, Math.Min(XactHelpers.ParseVolumeFromDecibels(reverbSettings.ReverbGainDb), 1f));
			efx.Effect(SoundEffect.ReverbEffect, EfxEffectf.EaxReverbDensity, reverbSettings.DensityPct / 100f);
			efx.AuxiliaryEffectSlot(SoundEffect.ReverbSlot, EfxEffectSlotf.EffectSlotGain, reverbSettings.WetDryMixPct / 200f);
			efx.BindEffectToAuxiliarySlot(SoundEffect.ReverbSlot, SoundEffect.ReverbEffect);
		}
	}

	private void PlatformDispose(bool disposing)
	{
		this.Spring?.Dispose();
	}

	internal static void PlatformInitialize()
	{
		OpenALSoundController.EnsureInitialized();
	}

	internal static void PlatformShutdown()
	{
		if (SoundEffect.ReverbEffect != 0)
		{
			OpenALSoundController.Efx.DeleteAuxiliaryEffectSlot((int)SoundEffect.ReverbSlot);
			OpenALSoundController.Efx.DeleteEffect((int)SoundEffect.ReverbEffect);
		}
		OpenALSoundController.DestroyInstance();
	}
}
