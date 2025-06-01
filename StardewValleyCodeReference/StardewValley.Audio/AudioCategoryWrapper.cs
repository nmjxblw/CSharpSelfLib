using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio;

public class AudioCategoryWrapper : IAudioCategory
{
	private AudioCategory audioCategory;

	public AudioCategoryWrapper(AudioCategory category)
	{
		this.audioCategory = category;
	}

	public void SetVolume(float volume)
	{
		this.audioCategory.SetVolume(volume);
	}
}
