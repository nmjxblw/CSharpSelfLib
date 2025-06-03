namespace Microsoft.Xna.Framework.Audio;

internal class VolumeEvent : ClipEvent
{
	private readonly float _volume;

	public VolumeEvent(XactClip clip, float timeStamp, float randomOffset, float volume)
		: base(clip, timeStamp, randomOffset)
	{
		this._volume = volume;
	}

	public override void Fire(Cue cue)
	{
		cue.Volume = this._volume;
	}
}
