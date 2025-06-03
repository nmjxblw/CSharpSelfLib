using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio;

public class CueDefinition
{
	public enum LimitBehavior
	{
		FailToPlay = 0,
		ReplaceOldest = 2
	}

	public string name;

	public List<XactSoundBankSound> sounds = new List<XactSoundBankSound>();

	public int instanceLimit = 255;

	public Action OnModified;

	public LimitBehavior limitBehavior;

	public CueDefinition()
	{
	}

	public CueDefinition(string cue_name, SoundEffect sound_effect, int category_id, bool loop = false, bool use_reverb = false)
	{
		this.name = cue_name;
		this.SetSound(sound_effect, category_id, loop, use_reverb);
	}

	public CueDefinition(string cue_name, SoundEffect[] sound_effects, int category_id, bool loop = false, bool use_reverb = false)
	{
		this.name = cue_name;
		this.SetSound(sound_effects, category_id, loop, use_reverb);
	}

	public virtual void SetSound(SoundEffect sound_effect, int category_id, bool loop = false, bool use_reverb = false)
	{
		this.SetSound(new SoundEffect[1] { sound_effect }, category_id, loop, use_reverb);
	}

	public virtual void SetSound(SoundEffect[] sound_effects, int category_id, bool loop = false, bool use_reverb = false)
	{
		foreach (XactSoundBankSound sound in this.sounds)
		{
			if (sound.soundClips == null)
			{
				sound.soundBank.GetSoundEffect(sound.waveBankIndex, sound.trackIndex).RemoveDependency();
			}
			else
			{
				if (sound.soundClips == null)
				{
					continue;
				}
				XactClip[] soundClips = sound.soundClips;
				for (int i = 0; i < soundClips.Length; i++)
				{
					ClipEvent[] clipEvents = soundClips[i].clipEvents;
					foreach (ClipEvent clip_event in clipEvents)
					{
						if (!(clip_event is PlayWaveEvent))
						{
							continue;
						}
						foreach (PlayWaveVariant variant in (clip_event as PlayWaveEvent).GetVariants())
						{
							variant.GetSoundEffect().RemoveDependency();
						}
					}
				}
			}
		}
		this.sounds.Clear();
		this.sounds.Add(new XactSoundBankSound(sound_effects, category_id, loop, use_reverb));
		for (int i = 0; i < sound_effects.Length; i++)
		{
			sound_effects[i].AddDependency();
		}
		foreach (XactSoundBankSound sound2 in this.sounds)
		{
			if (sound2.soundClips == null)
			{
				continue;
			}
			XactClip[] soundClips = sound2.soundClips;
			for (int i = 0; i < soundClips.Length; i++)
			{
				ClipEvent[] clipEvents = soundClips[i].clipEvents;
				foreach (ClipEvent clip_event2 in clipEvents)
				{
					_ = clip_event2 is PlayWaveEvent;
				}
			}
		}
	}
}
