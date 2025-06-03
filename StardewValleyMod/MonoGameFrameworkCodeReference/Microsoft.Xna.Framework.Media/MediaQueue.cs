using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media;

public sealed class MediaQueue
{
	private List<Song> songs = new List<Song>();

	private int _activeSongIndex = -1;

	private Random random = new Random();

	public Song ActiveSong
	{
		get
		{
			if (this.songs.Count == 0 || this._activeSongIndex < 0)
			{
				return null;
			}
			return this.songs[this._activeSongIndex];
		}
	}

	public int ActiveSongIndex
	{
		get
		{
			return this._activeSongIndex;
		}
		set
		{
			this._activeSongIndex = value;
		}
	}

	internal int Count => this.songs.Count;

	public Song this[int index] => this.songs[index];

	internal IEnumerable<Song> Songs => this.songs;

	internal Song GetNextSong(int direction, bool shuffle)
	{
		if (shuffle)
		{
			this._activeSongIndex = this.random.Next(this.songs.Count);
		}
		else
		{
			this._activeSongIndex = MathHelper.Clamp(this._activeSongIndex + direction, 0, this.songs.Count - 1);
		}
		return this.songs[this._activeSongIndex];
	}

	internal void Clear()
	{
		while (this.songs.Count > 0)
		{
			Song song = this.songs[0];
			song.Stop();
			this.songs.Remove(song);
		}
	}

	internal void SetVolume(float volume)
	{
		int count = this.songs.Count;
		for (int i = 0; i < count; i++)
		{
			this.songs[i].Volume = volume;
		}
	}

	internal void Add(Song song)
	{
		this.songs.Add(song);
	}

	internal void Stop()
	{
		int count = this.songs.Count;
		for (int i = 0; i < count; i++)
		{
			this.songs[i].Stop();
		}
	}
}
