using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Audio;

/// <summary>Applies audio changes from the <c>Data/AudioChanges</c> asset to the game's soundbank.</summary>
public class AudioCueModificationManager
{
	/// <summary>The audio changes to apply from the <c>Data/AudioChanges</c> asset.</summary>
	public Dictionary<string, AudioCueData> cueModificationData;

	/// <summary>Initialize the manager when the game starts.</summary>
	public void OnStartup()
	{
		this.cueModificationData = DataLoader.AudioChanges(Game1.content);
		this.ApplyAllCueModifications();
	}

	/// <summary>Apply all changes registered through the <c>Data/AudioChanges</c> asset.</summary>
	public virtual void ApplyAllCueModifications()
	{
		foreach (string key in this.cueModificationData.Keys)
		{
			this.ApplyCueModification(key);
		}
	}

	/// <summary>Get the absolute file path for a content-relative path.</summary>
	/// <param name="filePath">The file path relative to the game's <c>Content</c> folder.</param>
	public virtual string GetFilePath(string filePath)
	{
		return Path.Combine(Game1.content.RootDirectory, filePath);
	}

	/// <summary>Apply a change registered through the <c>Data/AudioChanges</c> asset.</summary>
	/// <param name="key">The entry key to apply in the asset.</param>
	public virtual void ApplyCueModification(string key)
	{
		try
		{
			if (!this.cueModificationData.TryGetValue(key, out var modification_data))
			{
				return;
			}
			bool is_modification = false;
			int category_index = Game1.audioEngine.GetCategoryIndex("Default");
			CueDefinition cue_definition;
			if (Game1.soundBank.Exists(modification_data.Id))
			{
				cue_definition = Game1.soundBank.GetCueDefinition(modification_data.Id);
				is_modification = true;
			}
			else
			{
				cue_definition = new CueDefinition();
				cue_definition.name = modification_data.Id;
			}
			if (modification_data.Category != null)
			{
				category_index = Game1.audioEngine.GetCategoryIndex(modification_data.Category);
			}
			if (modification_data.FilePaths != null)
			{
				SoundEffect[] effects = new SoundEffect[modification_data.FilePaths.Count];
				for (int i = 0; i < modification_data.FilePaths.Count; i++)
				{
					string file_path = this.GetFilePath(modification_data.FilePaths[i]);
					bool vorbis = Path.GetExtension(file_path).EqualsIgnoreCase(".ogg");
					int invalid_sounds = 0;
					try
					{
						SoundEffect sound_effect;
						if (vorbis && modification_data.StreamedVorbis)
						{
							sound_effect = new OggStreamSoundEffect(file_path);
						}
						else
						{
							using FileStream stream = new FileStream(file_path, FileMode.Open);
							sound_effect = SoundEffect.FromStream(stream, vorbis);
						}
						effects[i - invalid_sounds] = sound_effect;
					}
					catch (Exception exception)
					{
						Game1.log.Error("Error loading sound: " + file_path, exception);
						invalid_sounds++;
					}
					if (invalid_sounds > 0)
					{
						Array.Resize(ref effects, effects.Length - invalid_sounds);
					}
				}
				cue_definition.SetSound(effects, category_index, modification_data.Looped, modification_data.UseReverb);
				if (is_modification)
				{
					cue_definition.OnModified?.Invoke();
				}
			}
			Game1.soundBank.AddCue(cue_definition);
		}
		catch (NoAudioHardwareException)
		{
			Game1.log.Warn("Can't apply modifications for audio cue '" + key + "' because there's no audio hardware available.");
		}
	}
}
