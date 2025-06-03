namespace Microsoft.Xna.Framework.Audio;

public class PlayWaveVariant
{
	public SoundEffect overrideSoundEffect;

	public SoundBank soundBank;

	public int waveBank = -1;

	public int track = -1;

	public byte weight = 1;

	public SoundEffect GetSoundEffect()
	{
		if (this.overrideSoundEffect != null)
		{
			return this.overrideSoundEffect;
		}
		if (this.soundBank != null)
		{
			return this.soundBank.GetSoundEffect(this.waveBank, this.track);
		}
		return null;
	}

	public SoundEffectInstance GetSoundEffectInstance()
	{
		if (this.overrideSoundEffect != null)
		{
			return this.overrideSoundEffect.GetPooledInstance(forXAct: true);
		}
		bool streaming;
		if (this.soundBank != null)
		{
			return this.soundBank.GetSoundEffectInstance(this.waveBank, this.track, out streaming);
		}
		return null;
	}
}
