namespace Microsoft.Xna.Framework.Audio;

/// <summary>Controls how Cue objects should cease playback when told to stop.</summary>
public enum AudioStopOptions
{
	/// <summary>Stop normally, playing any pending release phases or transitions.</summary>
	AsAuthored,
	/// <summary>Immediately stops the cue, ignoring any pending release phases or transitions.</summary>
	Immediate
}
