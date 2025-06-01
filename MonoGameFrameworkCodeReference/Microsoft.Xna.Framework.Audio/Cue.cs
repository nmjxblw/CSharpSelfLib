using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>Manages the playback of a sound or set of sounds.</summary>
/// <remarks>
/// <para>Cues are comprised of one or more sounds.</para>
/// <para>Cues also define specific properties such as pitch or volume.</para>
/// <para>Cues are referenced through SoundBank objects.</para>
/// </remarks>
public class Cue : IDisposable
{
	protected AudioEngine _engine;

	protected string _name;

	protected List<XactSoundBankSound> _xactSounds;

	protected int _instanceLimit = 255;

	protected int _limitBehavior;

	private RpcVariable[] _variables;

	protected bool _applied3D;

	protected bool _played;

	protected XactSoundBankSound _currentXactSound;

	protected int _variantIndex = -1;

	protected SoundEffectInstance _soundEffect;

	protected AudioCategory _playingCategory;

	protected float _cueVolume = 1f;

	protected float _cuePitch;

	protected float _rpcVolume = 1f;

	protected float _rpcPitch;

	protected float _rpcReverbMix = 1f;

	protected float? _rpcFilterFrequency;

	protected float? _rpcFilterQFactor;

	protected float _time = -1f;

	protected bool? _pitchControlledByRPC;

	protected CueDefinition _cueDefinition;

	public bool IsPitchBeingControlledByRPC
	{
		get
		{
			if (!this._pitchControlledByRPC.HasValue)
			{
				XactSoundBankSound sound = this._currentXactSound;
				if (sound == null && this._xactSounds.Count > 0)
				{
					sound = this._xactSounds[0];
				}
				if (sound == null)
				{
					return false;
				}
				int[] curves = sound.rpcCurves;
				if (curves.Length != 0)
				{
					for (int i = 0; i < curves.Length; i++)
					{
						if (this._engine.RpcCurves[curves[i]].Parameter == RpcParameter.Pitch)
						{
							this._pitchControlledByRPC = true;
							break;
						}
					}
				}
				if (!this._pitchControlledByRPC.HasValue)
				{
					this._pitchControlledByRPC = false;
				}
			}
			return this._pitchControlledByRPC.Value;
		}
	}

	/// <summary>Indicates whether or not the cue is currently paused.</summary>
	/// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
	public bool IsPaused
	{
		get
		{
			if (this._soundEffect != null)
			{
				return this._soundEffect.State == SoundState.Paused;
			}
			return false;
		}
	}

	public float Pitch
	{
		get
		{
			return this._cuePitch;
		}
		set
		{
			if (this._cuePitch != value)
			{
				this._cuePitch = value;
				this._UpdateSoundParameters();
			}
		}
	}

	public float Volume
	{
		get
		{
			return this._cueVolume;
		}
		set
		{
			if (this._cueVolume != value)
			{
				this._cueVolume = value;
				this._UpdateSoundParameters();
			}
		}
	}

	/// <summary>Indicates whether or not the cue is currently playing.</summary>
	/// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
	public bool IsPlaying => this._time >= 0f;

	/// <summary>Indicates whether or not the cue is currently stopped.</summary>
	public bool IsStopped
	{
		get
		{
			if (this._soundEffect != null)
			{
				return this._soundEffect.State == SoundState.Stopped;
			}
			if (!this.IsDisposed)
			{
				return !this.IsPrepared;
			}
			return false;
		}
	}

	public bool IsStopping => false;

	public bool IsPreparing => false;

	public bool IsPrepared { get; internal set; }

	public bool IsCreated { get; internal set; }

	/// <summary>Gets the friendly name of the cue.</summary>
	/// <remarks>The friendly name is a value set from the designer.</remarks>
	public string Name => this._name;

	public int VariantIndex => this._variantIndex;

	/// <summary>
	/// Is true if the Cue has been disposed.
	/// </summary>
	public bool IsDisposed { get; internal set; }

	/// <summary>
	/// This event is triggered when the Cue is disposed.
	/// </summary>
	public event EventHandler<EventArgs> Disposing;

	internal Cue(AudioEngine engine, CueDefinition cue)
	{
		this._cueDefinition = cue;
		this._engine = engine;
		this._name = cue.name;
		this._xactSounds = cue.sounds;
		this._instanceLimit = cue.instanceLimit;
		this._limitBehavior = (int)cue.limitBehavior;
		this._variables = engine.CreateCueVariables();
	}

	internal Cue(AudioEngine engine, string cuename, XactSoundBankSound sound)
	{
		this._engine = engine;
		this._name = cuename;
		this._currentXactSound = sound;
		this._variables = engine.CreateCueVariables();
	}

	internal Cue(AudioEngine engine, string cuename, List<XactSoundBankSound> sounds, float[] probs)
	{
		this._engine = engine;
		this._name = cuename;
		this._xactSounds = sounds;
		this._variables = engine.CreateCueVariables();
	}

	internal void Prepare()
	{
		this.IsDisposed = false;
		this.IsCreated = false;
		this.IsPrepared = true;
		this._currentXactSound = null;
	}

	/// <summary>Pauses playback.</summary>
	public void Pause()
	{
		lock (this._engine.UpdateLock)
		{
			if (this._soundEffect != null)
			{
				this._soundEffect.Pause();
			}
		}
	}

	/// <summary>Requests playback of a prepared or preparing Cue.</summary>
	/// <remarks>Calling Play when the Cue already is playing can result in an InvalidOperationException.</remarks>
	public void Play()
	{
		lock (this._engine.UpdateLock)
		{
			if (this._instanceLimit < 255 && this._instanceLimit > 0)
			{
				Cue oldest_cue = null;
				int current_count = 0;
				foreach (Cue cue in this._engine.ActiveCues)
				{
					if (cue.Name == this.Name)
					{
						if (oldest_cue == null)
						{
							oldest_cue = cue;
						}
						current_count++;
						if (current_count >= this._instanceLimit)
						{
							break;
						}
					}
				}
				if (current_count >= this._instanceLimit)
				{
					if (this._limitBehavior == 0)
					{
						return;
					}
					if (this._limitBehavior == 1)
					{
						oldest_cue.Stop(AudioStopOptions.Immediate);
					}
					else if (this._limitBehavior == 2)
					{
						oldest_cue.Stop(AudioStopOptions.Immediate);
					}
					else if (this._limitBehavior == 3)
					{
						oldest_cue.Stop(AudioStopOptions.Immediate);
					}
					else if (this._limitBehavior == 4)
					{
						oldest_cue.Stop(AudioStopOptions.Immediate);
					}
				}
			}
			if (!this._engine.ActiveCues.Contains(this))
			{
				this._engine.ActiveCues.Add(this);
			}
			if (this._xactSounds != null)
			{
				int index = XactHelpers.Random.Next(this._xactSounds.Count);
				this._currentXactSound = this._xactSounds[index];
				if (this._currentXactSound == null)
				{
					return;
				}
			}
			this.UpdateRpcCurves();
			AudioCategory category = this._engine.Categories[this._currentXactSound.categoryID];
			if (category.GetPlayingInstanceCount() >= category.maxInstances)
			{
				category.GetOldestInstance()?.Stop(AudioStopOptions.Immediate);
			}
			SoundEffectInstance wave = this._currentXactSound.GetSimpleSoundInstance();
			this._time = 0f;
			if (wave != null)
			{
				this.PlaySoundInstance(wave);
			}
			else if (this._currentXactSound.soundClips != null)
			{
				XactClip[] soundClips = this._currentXactSound.soundClips;
				for (int i = 0; i < soundClips.Length; i++)
				{
					soundClips[i].Update(this, -1f, this._time);
				}
			}
		}
		if (this._cueDefinition != null)
		{
			CueDefinition cueDefinition = this._cueDefinition;
			cueDefinition.OnModified = (Action)Delegate.Combine(cueDefinition.OnModified, new Action(_OnCueDefinitionModified));
		}
		this._played = true;
		this.IsPrepared = false;
	}

	internal void PlaySoundInstance(SoundEffectInstance sound_instance, int variant_index = -1)
	{
		if (this._soundEffect != null)
		{
			this._soundEffect.Stop(immediate: true);
			this._soundEffect._isXAct = false;
			if (!this._soundEffect._isPooled && !(this._soundEffect is OggStreamSoundEffectInstance))
			{
				this._soundEffect.Dispose();
			}
			this._soundEffect = null;
		}
		this._soundEffect = sound_instance;
		if (this._soundEffect != null)
		{
			this._soundEffect.Play();
			this._playingCategory = this._engine.Categories[this._currentXactSound.categoryID];
			this._playingCategory.AddSound(this);
			this._UpdateSoundParameters();
		}
		this._variantIndex = variant_index;
	}

	/// <summary>Resumes playback of a paused Cue.</summary>
	public void Resume()
	{
		lock (this._engine.UpdateLock)
		{
			if (this._soundEffect != null)
			{
				this._soundEffect.Resume();
			}
		}
	}

	/// <summary>Stops playback of a Cue.</summary>
	/// <param name="options">Specifies if the sound should play any pending release phases or transitions before stopping.</param>
	public void Stop(AudioStopOptions options)
	{
		lock (this._engine.UpdateLock)
		{
			this._time = -1f;
			this._engine.ActiveCues.Remove(this);
			if (this._playingCategory != null)
			{
				this._playingCategory.RemoveSound(this);
				this._playingCategory = null;
			}
			if (this._soundEffect != null)
			{
				this._soundEffect.Stop(options == AudioStopOptions.Immediate);
				this._soundEffect._isXAct = false;
				if (!this._soundEffect._isPooled && !(this._soundEffect is OggStreamSoundEffectInstance))
				{
					this._soundEffect.Dispose();
				}
				this._soundEffect = null;
			}
		}
		if (this._cueDefinition != null)
		{
			CueDefinition cueDefinition = this._cueDefinition;
			cueDefinition.OnModified = (Action)Delegate.Remove(cueDefinition.OnModified, new Action(_OnCueDefinitionModified));
		}
		this.IsPrepared = false;
	}

	protected void _OnCueDefinitionModified()
	{
		if (this.IsPaused)
		{
			this._soundEffect.Stop();
			this._soundEffect = null;
			this.Play();
			if (this._soundEffect != null)
			{
				this._soundEffect.Pause();
			}
		}
		else if (this.IsPlaying)
		{
			this._soundEffect.Stop();
			this._soundEffect = null;
			this.Play();
		}
		else if (this._soundEffect != null)
		{
			this._soundEffect.Stop();
			this._soundEffect = null;
		}
	}

	private int FindVariable(string name)
	{
		for (int i = 0; i < this._variables.Length; i++)
		{
			if (this._variables[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Sets the value of a cue-instance variable based on its friendly name.
	/// </summary>
	/// <param name="name">Friendly name of the variable to set.</param>
	/// <param name="value">Value to assign to the variable.</param>
	/// <remarks>The friendly name is a value set from the designer.</remarks>
	public void SetVariable(string name, float value)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		int i = this.FindVariable(name);
		if (i == -1 || !this._variables[i].IsPublic)
		{
			throw new IndexOutOfRangeException("The specified variable index is invalid.");
		}
		this._variables[i].SetValue(value);
	}

	/// <summary>Gets a cue-instance variable value based on its friendly name.</summary>
	/// <param name="name">Friendly name of the variable.</param>
	/// <returns>Value of the variable.</returns>
	/// <remarks>
	/// <para>Cue-instance variables are useful when multiple instantiations of a single cue (and its associated sounds) are required (for example, a "car" cue where there may be more than one car at any given time). While a global variable allows multiple audio elements to be controlled in unison, a cue instance variable grants discrete control of each instance of a cue, even for each copy of the same cue.</para>
	/// <para>The friendly name is a value set from the designer.</para>
	/// </remarks>
	public float GetVariable(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		int i = this.FindVariable(name);
		if (i == -1 || !this._variables[i].IsPublic)
		{
			throw new IndexOutOfRangeException("The specified variable index is invalid.");
		}
		return this._variables[i].Value;
	}

	/// <summary>Updates the simulated 3D Audio settings calculated between an AudioEmitter and AudioListener.</summary>
	/// <param name="listener">The listener to calculate.</param>
	/// <param name="emitter">The emitter to calculate.</param>
	/// <remarks>
	/// <para>This must be called before Play().</para>
	/// <para>Calling this method automatically converts the sound to monoaural and sets the speaker mix for any sound played by this cue to a value calculated with the listener's and emitter's positions. Any stereo information in the sound will be discarded.</para>
	/// </remarks>
	public void Apply3D(AudioListener listener, AudioEmitter emitter)
	{
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		if (emitter == null)
		{
			throw new ArgumentNullException("emitter");
		}
		if (this._played && !this._applied3D)
		{
			throw new InvalidOperationException("You must call Apply3D on a Cue before calling Play to be able to call Apply3D after calling Play.");
		}
		Vector3 direction = listener.Position - emitter.Position;
		lock (this._engine.UpdateLock)
		{
			float distance = direction.Length();
			int i = this.FindVariable("Distance");
			this._variables[i].SetValue(distance);
			if (distance > 0f)
			{
				direction /= distance;
			}
			Vector3.Cross(listener.Up, listener.Forward);
			float angle = MathHelper.ToDegrees((float)Math.Acos(Vector3.Dot(direction, listener.Forward)));
			int j = this.FindVariable("OrientationAngle");
			this._variables[j].SetValue(angle);
			_ = this._currentXactSound;
			_ = (emitter.Velocity - listener.Velocity) * emitter.DopplerScale;
		}
		this._applied3D = true;
	}

	public List<PlayWaveEvent> GetPlayWaveEvents()
	{
		List<PlayWaveEvent> events = null;
		if (this._xactSounds == null)
		{
			if (this._currentXactSound.complexSound)
			{
				XactClip[] soundClips = this._currentXactSound.soundClips;
				for (int i = 0; i < soundClips.Length; i++)
				{
					ClipEvent[] clipEvents = soundClips[i].clipEvents;
					foreach (ClipEvent clip_event in clipEvents)
					{
						if (clip_event is PlayWaveEvent)
						{
							if (events == null)
							{
								events = new List<PlayWaveEvent>();
							}
							events.Add(clip_event as PlayWaveEvent);
						}
					}
				}
			}
		}
		else
		{
			foreach (XactSoundBankSound xact_sound in this._xactSounds)
			{
				if (!xact_sound.complexSound)
				{
					continue;
				}
				XactClip[] soundClips = xact_sound.soundClips;
				for (int i = 0; i < soundClips.Length; i++)
				{
					ClipEvent[] clipEvents = soundClips[i].clipEvents;
					foreach (ClipEvent clip_event2 in clipEvents)
					{
						if (clip_event2 is PlayWaveEvent)
						{
							if (events == null)
							{
								events = new List<PlayWaveEvent>();
							}
							events.Add(clip_event2 as PlayWaveEvent);
						}
					}
				}
			}
		}
		return events;
	}

	internal void Update(float dt)
	{
		if (this._currentXactSound == null || this._time < 0f)
		{
			return;
		}
		if (this._soundEffect == null || this._soundEffect.State == SoundState.Playing)
		{
			float old_time = this._time;
			this._time += dt;
			if (this._currentXactSound.soundClips != null)
			{
				XactClip[] soundClips = this._currentXactSound.soundClips;
				for (int i = 0; i < soundClips.Length; i++)
				{
					soundClips[i].Update(this, old_time, this._time);
				}
			}
		}
		this.UpdateRpcCurves();
		if (this._soundEffect != null && this._soundEffect.State == SoundState.Stopped)
		{
			this.Stop(AudioStopOptions.Immediate);
		}
	}

	private float UpdateRpcCurves()
	{
		float volume = 1f;
		int[] rpcCurves = this._currentXactSound.rpcCurves;
		if (rpcCurves.Length != 0)
		{
			float pitch = 0f;
			float reverbMix = 1f;
			float? filterFrequency = null;
			float? filterQFactor = null;
			for (int i = 0; i < rpcCurves.Length; i++)
			{
				RpcCurve rpcCurve = this._engine.RpcCurves[rpcCurves[i]];
				float value = ((!rpcCurve.IsGlobal) ? rpcCurve.Evaluate(this._variables[rpcCurve.Variable].Value) : rpcCurve.Evaluate(this._engine.GetGlobalVariable(rpcCurve.Variable)));
				switch (rpcCurve.Parameter)
				{
				case RpcParameter.Volume:
					volume *= XactHelpers.ParseVolumeFromDecibels(value / 100f);
					break;
				case RpcParameter.Pitch:
					pitch += value / 1200f;
					break;
				case RpcParameter.ReverbSend:
					reverbMix *= XactHelpers.ParseVolumeFromDecibels(value / 100f);
					break;
				case RpcParameter.FilterFrequency:
					filterFrequency = value;
					break;
				case RpcParameter.FilterQFactor:
					filterQFactor = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("rpcCurve.Parameter");
				}
			}
			pitch = MathHelper.Clamp(pitch, -1f, 1f);
			if (volume < 0f)
			{
				volume = 0f;
			}
			this._rpcVolume = volume;
			this._rpcPitch = pitch;
			this._rpcReverbMix = reverbMix;
			this._rpcFilterFrequency = filterFrequency;
			this._rpcFilterQFactor = filterQFactor;
			this._UpdateSoundParameters();
		}
		return volume;
	}

	internal void _UpdateSoundParameters()
	{
		if (this._soundEffect != null)
		{
			this._soundEffect.Volume = this._cueVolume * this._playingCategory._volume * this._rpcVolume * this._currentXactSound.volume;
			this._soundEffect.Pitch = this._rpcPitch + this._cuePitch + this._currentXactSound.pitch;
			if (this._currentXactSound.useReverb)
			{
				this._soundEffect.PlatformSetReverbMix(this._rpcReverbMix);
			}
			if (this._soundEffect.IsFilterEnabled() && (this._rpcFilterQFactor.HasValue || this._rpcFilterFrequency.HasValue))
			{
				this._soundEffect.PlatformSetFilter(this._soundEffect._filterMode, this._rpcFilterQFactor.HasValue ? this._rpcFilterQFactor.Value : this._soundEffect._filterQ, this._rpcFilterFrequency.HasValue ? this._rpcFilterFrequency.Value : this._soundEffect._filterFrequency);
			}
		}
	}

	/// <summary>
	/// Disposes the Cue
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (this.IsDisposed)
		{
			return;
		}
		this.IsDisposed = true;
		if (disposing)
		{
			this.IsCreated = false;
			this.IsPrepared = false;
			EventHelpers.Raise(this, this.Disposing, EventArgs.Empty);
			if (this._cueDefinition != null)
			{
				CueDefinition cueDefinition = this._cueDefinition;
				cueDefinition.OnModified = (Action)Delegate.Remove(cueDefinition.OnModified, new Action(_OnCueDefinitionModified));
				this._cueDefinition = null;
			}
		}
	}
}
