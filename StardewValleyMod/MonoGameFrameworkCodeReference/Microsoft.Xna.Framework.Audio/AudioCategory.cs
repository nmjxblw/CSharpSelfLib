using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>
/// Provides functionality for manipulating multiple sounds at a time.
/// </summary>
public class AudioCategory
{
	private readonly string _name;

	private readonly AudioEngine _engine;

	private readonly List<Cue> _sounds;

	internal float _volume;

	internal bool isBackgroundMusic;

	internal bool isPublic;

	internal bool instanceLimit;

	internal int maxInstances;

	internal MaxInstanceBehavior InstanceBehavior;

	internal CrossfadeType fadeType;

	internal float fadeIn;

	internal float fadeOut;

	/// <summary>
	/// Gets the category's friendly name.
	/// </summary>
	public string Name => this._name;

	internal AudioCategory(AudioEngine audioengine, string name, BinaryReader reader)
	{
		this._sounds = new List<Cue>();
		this._name = name;
		this._engine = audioengine;
		this._sounds = new List<Cue>();
		this.maxInstances = reader.ReadByte();
		this.instanceLimit = this.maxInstances != 255;
		this.fadeIn = (float)(int)reader.ReadUInt16() / 1000f;
		this.fadeOut = (float)(int)reader.ReadUInt16() / 1000f;
		byte instanceFlags = reader.ReadByte();
		this.fadeType = (CrossfadeType)(instanceFlags & 7);
		this.InstanceBehavior = (MaxInstanceBehavior)(instanceFlags >> 3);
		reader.ReadUInt16();
		float volume = XactHelpers.ParseVolumeFromDecibels(reader.ReadByte());
		this._volume = volume;
		byte visibilityFlags = reader.ReadByte();
		this.isBackgroundMusic = (visibilityFlags & 1) != 0;
		this.isPublic = (visibilityFlags & 2) != 0;
	}

	internal void AddSound(Cue sound)
	{
		this._sounds.Add(sound);
	}

	internal void RemoveSound(Cue sound)
	{
		this._sounds.Remove(sound);
	}

	internal int GetPlayingInstanceCount()
	{
		int sum = 0;
		for (int i = 0; i < this._sounds.Count; i++)
		{
			if (this._sounds[i].IsPlaying)
			{
				sum++;
			}
		}
		return sum;
	}

	internal Cue GetOldestInstance()
	{
		for (int i = 0; i < this._sounds.Count; i++)
		{
			if (this._sounds[i].IsPlaying)
			{
				return this._sounds[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Pauses all associated sounds.
	/// </summary>
	public void Pause()
	{
		foreach (Cue sound in this._sounds)
		{
			sound.Pause();
		}
	}

	/// <summary>
	/// Resumes all associated paused sounds.
	/// </summary>
	public void Resume()
	{
		foreach (Cue sound in this._sounds)
		{
			sound.Resume();
		}
	}

	/// <summary>
	/// Stops all associated sounds.
	/// </summary>
	public void Stop(AudioStopOptions options)
	{
		foreach (Cue sound in this._sounds)
		{
			sound.Stop(options);
		}
	}

	/// <summary>
	/// Set the volume for this <see cref="T:Microsoft.Xna.Framework.Audio.AudioCategory" />.
	/// </summary>
	/// <param name="volume">The new volume of the category.</param>
	/// <exception cref="T:System.ArgumentException">If the volume is less than zero.</exception>
	public void SetVolume(float volume)
	{
		if (volume < 0f)
		{
			throw new ArgumentException("The volume must be positive.");
		}
		if (this._volume == volume)
		{
			return;
		}
		this._volume = volume;
		foreach (Cue sound in this._sounds)
		{
			sound._UpdateSoundParameters();
		}
	}

	/// <summary>
	/// Returns the name of this AudioCategory
	/// </summary>
	/// <returns>Friendly name of the AudioCategory</returns>
	public override string ToString()
	{
		return this._name;
	}
}
