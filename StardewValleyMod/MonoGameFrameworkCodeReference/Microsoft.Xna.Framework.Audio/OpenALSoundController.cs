using System;
using System.Collections.Generic;
using System.Linq;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio;

internal sealed class OpenALSoundController : IDisposable
{
	private static OpenALSoundController _instance;

	private static EffectsExtension _efx;

	private IntPtr _device;

	private IntPtr _context;

	private IntPtr NullContext = IntPtr.Zero;

	private int[] allSourcesArray;

	internal const int MAX_NUMBER_OF_SOURCES = 256;

	private static OggStreamer _oggstreamer;

	private static OpenALSoundEffectInstanceManager _openALSoundEffectInstanceManager;

	private List<int> availableSourcesCollection;

	private List<int> inUseSourcesCollection;

	private bool _isDisposed;

	public bool SupportsIma4 { get; private set; }

	public bool SupportsAdpcm { get; private set; }

	public bool SupportsEfx { get; private set; }

	public bool SupportsIeee { get; private set; }

	public static OpenALSoundController Instance
	{
		get
		{
			if (OpenALSoundController._instance == null)
			{
				throw new NoAudioHardwareException("OpenAL context has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
			}
			return OpenALSoundController._instance;
		}
	}

	public static EffectsExtension Efx
	{
		get
		{
			if (OpenALSoundController._efx == null)
			{
				OpenALSoundController._efx = new EffectsExtension();
			}
			return OpenALSoundController._efx;
		}
	}

	public int Filter { get; private set; }

	/// <summary>
	/// Sets up the hardware resources used by the controller.
	/// </summary>
	private OpenALSoundController()
	{
		if (AL.NativeLibrary == IntPtr.Zero)
		{
			throw new DllNotFoundException("Couldn't initialize OpenAL because the native binaries couldn't be found.");
		}
		if (!this.OpenSoundController())
		{
			throw new NoAudioHardwareException("OpenAL device could not be initialized, see console output for details.");
		}
		if (Alc.IsExtensionPresent(this._device, "ALC_EXT_CAPTURE"))
		{
			Microphone.PopulateCaptureDevices();
		}
		this.allSourcesArray = new int[256];
		AL.GenSources(this.allSourcesArray);
		this.Filter = 0;
		if (OpenALSoundController.Efx.IsInitialized)
		{
			this.Filter = OpenALSoundController.Efx.GenFilter();
		}
		this.availableSourcesCollection = new List<int>(this.allSourcesArray);
		this.inUseSourcesCollection = new List<int>();
	}

	~OpenALSoundController()
	{
		this.Dispose(disposing: false);
	}

	/// <summary>
	/// Open the sound device, sets up an audio context, and makes the new context
	/// the current context. Note that this method will stop the playback of
	/// music that was running prior to the game start. If any error occurs, then
	/// the state of the controller is reset.
	/// </summary>
	/// <returns>True if the sound controller was setup, and false if not.</returns>
	private bool OpenSoundController()
	{
		try
		{
			this._device = Alc.OpenDevice(string.Empty);
			EffectsExtension.device = this._device;
		}
		catch (Exception innerException)
		{
			throw new NoAudioHardwareException("OpenAL device could not be initialized.", innerException);
		}
		if (this._device != IntPtr.Zero)
		{
			int[] attribute = new int[3] { 6546, 0, 0 };
			this._context = Alc.CreateContext(this._device, attribute);
			OpenALSoundController._oggstreamer = new OggStreamer();
			if (this._context != this.NullContext)
			{
				Alc.MakeContextCurrent(this._context);
				this.SupportsIma4 = AL.IsExtensionPresent("AL_EXT_IMA4");
				this.SupportsAdpcm = AL.IsExtensionPresent("AL_SOFT_MSADPCM");
				this.SupportsEfx = AL.IsExtensionPresent("AL_EXT_EFX");
				this.SupportsIeee = AL.IsExtensionPresent("AL_EXT_float32");
				return true;
			}
		}
		return false;
	}

	public static void EnsureInitialized()
	{
		if (OpenALSoundController._instance == null)
		{
			try
			{
				OpenALSoundController._instance = new OpenALSoundController();
			}
			catch (DllNotFoundException)
			{
				throw;
			}
			catch (NoAudioHardwareException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				throw new NoAudioHardwareException("Failed to init OpenALSoundController", innerException);
			}
		}
		if (OpenALSoundController._openALSoundEffectInstanceManager == null)
		{
			OpenALSoundController._openALSoundEffectInstanceManager = new OpenALSoundEffectInstanceManager();
		}
	}

	public static void DestroyInstance()
	{
		if (OpenALSoundController._openALSoundEffectInstanceManager != null)
		{
			OpenALSoundController._openALSoundEffectInstanceManager.Dispose();
			OpenALSoundController._openALSoundEffectInstanceManager = null;
		}
		if (OpenALSoundController._instance != null)
		{
			OpenALSoundController._instance.Dispose();
			OpenALSoundController._instance = null;
		}
	}

	/// <summary>
	/// Destroys the AL context and closes the device, when they exist.
	/// </summary>
	private void CleanUpOpenAL()
	{
		Alc.MakeContextCurrent(this.NullContext);
		if (this._context != this.NullContext)
		{
			Alc.DestroyContext(this._context);
			this._context = this.NullContext;
		}
		if (this._device != IntPtr.Zero)
		{
			Alc.CloseDevice(this._device);
			this._device = IntPtr.Zero;
		}
	}

	/// <summary>
	/// Dispose of the OpenALSoundCOntroller.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose of the OpenALSoundCOntroller.
	/// </summary>
	/// <param name="disposing">If true, the managed resources are to be disposed.</param>
	private void Dispose(bool disposing)
	{
		if (this._isDisposed)
		{
			return;
		}
		if (disposing)
		{
			if (OpenALSoundController._oggstreamer != null)
			{
				OpenALSoundController._oggstreamer.Dispose();
			}
			for (int i = 0; i < this.allSourcesArray.Length; i++)
			{
				AL.DeleteSource(this.allSourcesArray[i]);
			}
			if (this.Filter != 0 && OpenALSoundController.Efx.IsInitialized)
			{
				OpenALSoundController.Efx.DeleteFilter(this.Filter);
			}
			Microphone.StopMicrophones();
			this.CleanUpOpenAL();
		}
		this._isDisposed = true;
	}

	/// <summary>
	/// Reserves a sound buffer and return its identifier. If there are no available sources
	/// or the controller was not able to setup the hardware then an
	/// <see cref="T:Microsoft.Xna.Framework.Audio.InstancePlayLimitException" /> is thrown.
	/// </summary>
	/// <returns>The source number of the reserved sound buffer.</returns>
	public int ReserveSource()
	{
		lock (this.availableSourcesCollection)
		{
			if (this.availableSourcesCollection.Count == 0)
			{
				throw new InstancePlayLimitException();
			}
			int sourceNumber = this.availableSourcesCollection.Last();
			this.inUseSourcesCollection.Add(sourceNumber);
			this.availableSourcesCollection.Remove(sourceNumber);
			return sourceNumber;
		}
	}

	public void RecycleSource(int sourceId)
	{
		AL.Source(sourceId, ALSourcei.Buffer, 0);
		lock (this.availableSourcesCollection)
		{
			if (this.inUseSourcesCollection.Remove(sourceId))
			{
				this.availableSourcesCollection.Add(sourceId);
			}
		}
	}

	public void FreeSource(SoundEffectInstance inst)
	{
		this.RecycleSource(inst.SourceId);
		inst.SourceId = 0;
		inst.HasSourceId = false;
		inst.SoundState = SoundState.Stopped;
	}

	public double SourceCurrentPosition(int sourceId)
	{
		AL.GetSource(sourceId, ALGetSourcei.SampleOffset, out var pos);
		return pos;
	}
}
