using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Media;

public sealed class Song : IEquatable<Song>, IDisposable
{
	internal delegate void FinishedPlayingHandler(object sender, EventArgs args);

	private string _name;

	private int _playCount;

	private TimeSpan _duration = TimeSpan.Zero;

	private bool disposed;

	private OggStream stream;

	private float _volume = 1f;

	private readonly object _sourceMutex = new object();

	/// <summary>
	/// Gets the Album on which the Song appears.
	/// </summary>
	public Album Album => this.PlatformGetAlbum();

	/// <summary>
	/// Gets the Artist of the Song.
	/// </summary>
	public Artist Artist => this.PlatformGetArtist();

	/// <summary>
	/// Gets the Genre of the Song.
	/// </summary>
	public Genre Genre => this.PlatformGetGenre();

	public bool IsDisposed => this.disposed;

	internal string FilePath => this._name;

	public TimeSpan Duration => this.PlatformGetDuration();

	public bool IsProtected => this.PlatformIsProtected();

	public bool IsRated => this.PlatformIsRated();

	public string Name => this.PlatformGetName();

	public int PlayCount => this.PlatformGetPlayCount();

	public int Rating => this.PlatformGetRating();

	public int TrackNumber => this.PlatformGetTrackNumber();

	internal float Volume
	{
		get
		{
			if (this.stream == null)
			{
				return 0f;
			}
			return this._volume;
		}
		set
		{
			this._volume = value;
			if (this.stream != null)
			{
				this.stream.Volume = this._volume;
			}
		}
	}

	public TimeSpan Position
	{
		get
		{
			if (this.stream == null)
			{
				return TimeSpan.FromSeconds(0.0);
			}
			return this.stream.GetPosition();
		}
	}

	internal Song(string fileName, int durationMS)
		: this(fileName)
	{
		this._duration = TimeSpan.FromMilliseconds(durationMS);
	}

	internal Song(string fileName)
	{
		this._name = fileName;
		this.PlatformInitialize(fileName);
	}

	~Song()
	{
		this.Dispose(disposing: false);
	}

	/// <summary>
	/// Returns a song that can be played via <see cref="T:Microsoft.Xna.Framework.Media.MediaPlayer" />.
	/// </summary>
	/// <param name="name">The name for the song. See <see cref="P:Microsoft.Xna.Framework.Media.Song.Name" />.</param>
	/// <param name="uri">The path to the song file.</param>
	/// <returns></returns>
	public static Song FromUri(string name, Uri uri)
	{
		return new Song(uri.OriginalString)
		{
			_name = name
		};
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			if (disposing)
			{
				this.PlatformDispose(disposing);
			}
			this.disposed = true;
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public bool Equals(Song song)
	{
		if ((object)song != null)
		{
			return this.Name == song.Name;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return this.Equals(obj as Song);
	}

	public static bool operator ==(Song song1, Song song2)
	{
		return song1?.Equals(song2) ?? ((object)song2 == null);
	}

	public static bool operator !=(Song song1, Song song2)
	{
		return !(song1 == song2);
	}

	private void PlatformInitialize(string fileName)
	{
		OpenALSoundController.EnsureInitialized();
		this.stream = new OggStream(fileName, OnFinishedPlaying);
		this.stream.Prepare();
		this._duration = this.stream.GetLength();
	}

	internal void SetEventHandler(FinishedPlayingHandler handler)
	{
	}

	internal void OnFinishedPlaying()
	{
		MediaPlayer.OnSongFinishedPlaying(null, null);
	}

	private void PlatformDispose(bool disposing)
	{
		lock (this._sourceMutex)
		{
			if (this.stream != null)
			{
				this.stream.Dispose();
				this.stream = null;
			}
		}
	}

	internal void Play(TimeSpan? startPosition)
	{
		if (this.stream != null)
		{
			this.stream.Play();
			if (startPosition.HasValue)
			{
				this.stream.SeekToPosition(startPosition.Value);
			}
			this._playCount++;
		}
	}

	internal void Resume()
	{
		if (this.stream != null)
		{
			this.stream.Resume();
		}
	}

	internal void Pause()
	{
		if (this.stream != null)
		{
			this.stream.Pause();
		}
	}

	internal void Stop()
	{
		if (this.stream != null)
		{
			this.stream.Stop();
			this._playCount = 0;
		}
	}

	private Album PlatformGetAlbum()
	{
		return null;
	}

	private Artist PlatformGetArtist()
	{
		return null;
	}

	private Genre PlatformGetGenre()
	{
		return null;
	}

	private TimeSpan PlatformGetDuration()
	{
		return this._duration;
	}

	private bool PlatformIsProtected()
	{
		return false;
	}

	private bool PlatformIsRated()
	{
		return false;
	}

	private string PlatformGetName()
	{
		return Path.GetFileNameWithoutExtension(this._name);
	}

	private int PlatformGetPlayCount()
	{
		return this._playCount;
	}

	private int PlatformGetRating()
	{
		return 0;
	}

	private int PlatformGetTrackNumber()
	{
		return 0;
	}
}
