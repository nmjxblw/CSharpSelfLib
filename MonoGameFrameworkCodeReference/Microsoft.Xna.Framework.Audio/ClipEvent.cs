namespace Microsoft.Xna.Framework.Audio;

public abstract class ClipEvent
{
	protected XactClip _clip;

	public float RandomOffset { get; private set; }

	public float TimeStamp { get; private set; }

	protected ClipEvent(XactClip clip, float timeStamp, float randomOffset)
	{
		this._clip = clip;
		this.TimeStamp = timeStamp;
		this.RandomOffset = randomOffset;
	}

	public abstract void Fire(Cue cue);
}
