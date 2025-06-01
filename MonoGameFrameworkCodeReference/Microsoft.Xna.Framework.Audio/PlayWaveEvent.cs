using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio;

public class PlayWaveEvent : ClipEvent
{
	private SoundBank _soundBank;

	public VariationType variationType;

	private uint _loopCount;

	private bool _newWaveOnLoop;

	protected List<PlayWaveVariant> _variants;

	protected int _totalWeights;

	protected float _trackVolume;

	protected float _trackPitch;

	protected float _trackFilterFrequency;

	protected float _trackFilterQFactor;

	protected float _clipReverbMix;

	public readonly Vector4? randomFilterRange;

	public readonly Vector2? randomVolumeRange;

	public readonly Vector2? randomPitchRange;

	private bool _streaming;

	public bool Loop
	{
		get
		{
			return this._loopCount == 255;
		}
		set
		{
			if (value)
			{
				this._loopCount = 255u;
			}
			else
			{
				this._loopCount = 0u;
			}
		}
	}

	public PlayWaveEvent(XactClip clip, List<PlayWaveVariant> variants, float time_stamp = 0f, float random_offset = 0f, bool loop = false)
		: base(clip, time_stamp, random_offset)
	{
		this._variants = variants;
		if (loop)
		{
			this._loopCount = 255u;
		}
		this._totalWeights = 0;
		for (int i = 0; i < this._variants.Count; i++)
		{
			PlayWaveVariant variant = variants[i];
			this._totalWeights += variant.weight;
		}
	}

	public PlayWaveEvent(XactClip clip, float timeStamp, float randomOffset, SoundBank soundBank, int[] waveBanks, int[] tracks, byte[] weights, int totalWeights, VariationType variation, Vector2? volumeVar, Vector2? pitchVar, Vector4? filterVar, uint loopCount, bool newWaveOnLoop)
		: base(clip, timeStamp, randomOffset)
	{
		this._soundBank = soundBank;
		this._variants = new List<PlayWaveVariant>();
		this._totalWeights = 0;
		for (int i = 0; i < tracks.Length; i++)
		{
			PlayWaveVariant variant = new PlayWaveVariant
			{
				soundBank = this._soundBank,
				waveBank = waveBanks[i],
				track = tracks[i]
			};
			if (weights != null)
			{
				variant.weight = weights[i];
			}
			else
			{
				variant.weight = 1;
			}
			this._totalWeights += variant.weight;
			this._variants.Add(variant);
		}
		this.randomVolumeRange = volumeVar;
		this.randomPitchRange = pitchVar;
		this.randomFilterRange = filterVar;
		this._trackVolume = 1f;
		this._trackPitch = 0f;
		this._trackFilterFrequency = 0f;
		this._trackFilterQFactor = 0f;
		if (base._clip.UseReverb)
		{
			this._clipReverbMix = 1f;
		}
		else
		{
			this._clipReverbMix = 0f;
		}
		this.variationType = variation;
		this._loopCount = loopCount;
		this._newWaveOnLoop = newWaveOnLoop;
	}

	public override void Fire(Cue cue)
	{
		this.Play(pickNewWav: true, cue);
	}

	private void Play(bool pickNewWav, Cue cue)
	{
		int trackCount = this._variants.Count;
		int variant_index = cue.VariantIndex;
		if (variant_index < 0)
		{
			variant_index = 0;
		}
		if (pickNewWav)
		{
			switch (this.variationType)
			{
			case VariationType.Ordered:
				variant_index = (variant_index + 1) % trackCount;
				break;
			case VariationType.OrderedFromRandom:
				variant_index = (variant_index + 1) % trackCount;
				break;
			case VariationType.Random:
			{
				int sum2 = XactHelpers.Random.Next(this._totalWeights + 1);
				for (int j = 0; j < trackCount; j++)
				{
					sum2 -= this._variants[j].weight;
					if (sum2 <= 0)
					{
						variant_index = j;
						break;
					}
				}
				break;
			}
			case VariationType.RandomNoImmediateRepeats:
			{
				int last = variant_index;
				int sum = XactHelpers.Random.Next(this._totalWeights + 1);
				for (int i = 0; i < trackCount; i++)
				{
					sum -= this._variants[i].weight;
					if (sum <= 0)
					{
						variant_index = i;
						break;
					}
				}
				if (variant_index == last)
				{
					variant_index = (variant_index + 1) % trackCount;
				}
				break;
			}
			case VariationType.Shuffle:
				variant_index = XactHelpers.Random.Next() % trackCount;
				break;
			}
		}
		SoundEffectInstance new_wave = this._variants[variant_index].GetSoundEffectInstance();
		if (new_wave == null)
		{
			return;
		}
		this._trackVolume = base._clip.DefaultVolume;
		if (this.randomVolumeRange.HasValue)
		{
			this._trackVolume = this.randomVolumeRange.Value.X + (float)XactHelpers.Random.NextDouble() * this.randomVolumeRange.Value.Y;
		}
		if (this.randomPitchRange.HasValue)
		{
			this._trackPitch = this.randomPitchRange.Value.X + (float)XactHelpers.Random.NextDouble() * this.randomPitchRange.Value.Y;
		}
		if (base._clip.FilterEnabled)
		{
			if (this.randomFilterRange.HasValue)
			{
				this._trackFilterFrequency = this.randomFilterRange.Value.X + (float)XactHelpers.Random.NextDouble() * this.randomFilterRange.Value.Y;
				this._trackFilterQFactor = this.randomFilterRange.Value.Z + (float)XactHelpers.Random.NextDouble() * this.randomFilterRange.Value.W;
			}
			else
			{
				this._trackFilterFrequency = (int)base._clip.FilterFrequency;
				this._trackFilterQFactor = base._clip.FilterQ;
			}
		}
		new_wave.LoopCount = this._loopCount;
		new_wave.Volume = this._trackVolume;
		new_wave.Pitch = this._trackPitch;
		if (base._clip.UseReverb)
		{
			new_wave.PlatformSetReverbMix(this._clipReverbMix);
		}
		if (base._clip.FilterEnabled)
		{
			new_wave.PlatformSetFilter(base._clip.FilterMode, this._trackFilterQFactor, this._trackFilterFrequency);
		}
		cue.Volume = this._trackVolume;
		cue.Pitch = this._trackPitch;
		cue.PlaySoundInstance(new_wave, variant_index);
	}

	public List<PlayWaveVariant> GetVariants()
	{
		return this._variants;
	}

	public void SetVariants(List<PlayWaveVariant> variants)
	{
		this._variants = variants;
		this._totalWeights = 0;
		foreach (PlayWaveVariant variant in this._variants)
		{
			this._totalWeights += variant.weight;
		}
	}
}
