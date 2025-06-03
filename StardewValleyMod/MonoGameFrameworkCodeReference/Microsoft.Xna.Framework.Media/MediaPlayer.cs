using System;

namespace Microsoft.Xna.Framework.Media;

public static class MediaPlayer
{
	private static int _numSongsInQueuePlayed;

	private static MediaState _state;

	private static float _volume;

	private static bool _isMuted;

	private static bool _isRepeating;

	private static bool _isShuffled;

	private static readonly MediaQueue _queue;

	public static MediaQueue Queue => MediaPlayer._queue;

	public static bool IsMuted
	{
		get
		{
			return MediaPlayer.PlatformGetIsMuted();
		}
		set
		{
			MediaPlayer.PlatformSetIsMuted(value);
		}
	}

	public static bool IsRepeating
	{
		get
		{
			return MediaPlayer.PlatformGetIsRepeating();
		}
		set
		{
			MediaPlayer.PlatformSetIsRepeating(value);
		}
	}

	public static bool IsShuffled
	{
		get
		{
			return MediaPlayer.PlatformGetIsShuffled();
		}
		set
		{
			MediaPlayer.PlatformSetIsShuffled(value);
		}
	}

	public static bool IsVisualizationEnabled => false;

	public static TimeSpan PlayPosition => MediaPlayer.PlatformGetPlayPosition();

	public static MediaState State
	{
		get
		{
			return MediaPlayer.PlatformGetState();
		}
		private set
		{
			if (MediaPlayer._state != value)
			{
				MediaPlayer._state = value;
				EventHelpers.Raise(null, MediaPlayer.MediaStateChanged, EventArgs.Empty);
			}
		}
	}

	public static bool GameHasControl => MediaPlayer.PlatformGetGameHasControl();

	public static float Volume
	{
		get
		{
			return MediaPlayer.PlatformGetVolume();
		}
		set
		{
			MediaPlayer.PlatformSetVolume(MathHelper.Clamp(value, 0f, 1f));
		}
	}

	public static event EventHandler<EventArgs> ActiveSongChanged;

	public static event EventHandler<EventArgs> MediaStateChanged;

	static MediaPlayer()
	{
		MediaPlayer._numSongsInQueuePlayed = 0;
		MediaPlayer._state = MediaState.Stopped;
		MediaPlayer._volume = 1f;
		MediaPlayer._queue = new MediaQueue();
		MediaPlayer.PlatformInitialize();
	}

	public static void Pause()
	{
		if (MediaPlayer.State == MediaState.Playing && !(MediaPlayer._queue.ActiveSong == null))
		{
			MediaPlayer.PlatformPause();
			MediaPlayer.State = MediaState.Paused;
		}
	}

	/// <summary>
	/// Play clears the current playback queue, and then queues up the specified song for playback. 
	/// Playback starts immediately at the beginning of the song.
	/// </summary>
	public static void Play(Song song)
	{
		MediaPlayer.Play(song, null);
	}

	/// <summary>
	/// Play clears the current playback queue, and then queues up the specified song for playback. 
	/// Playback starts immediately at the given position of the song.
	/// </summary>
	public static void Play(Song song, TimeSpan? startPosition)
	{
		Song obj = ((MediaPlayer._queue.Count > 0) ? MediaPlayer._queue[0] : null);
		MediaPlayer._queue.Clear();
		MediaPlayer._numSongsInQueuePlayed = 0;
		MediaPlayer._queue.Add(song);
		MediaPlayer._queue.ActiveSongIndex = 0;
		MediaPlayer.PlaySong(song, startPosition);
		if (obj != song)
		{
			EventHelpers.Raise(null, MediaPlayer.ActiveSongChanged, EventArgs.Empty);
		}
	}

	public static void Play(SongCollection collection, int index = 0)
	{
		MediaPlayer._queue.Clear();
		MediaPlayer._numSongsInQueuePlayed = 0;
		foreach (Song song in collection)
		{
			MediaPlayer._queue.Add(song);
		}
		MediaPlayer._queue.ActiveSongIndex = index;
		MediaPlayer.PlaySong(MediaPlayer._queue.ActiveSong, null);
	}

	private static void PlaySong(Song song, TimeSpan? startPosition)
	{
		if (song != null && song.IsDisposed)
		{
			throw new ObjectDisposedException("song");
		}
		MediaPlayer.PlatformPlaySong(song, startPosition);
		MediaPlayer.State = MediaState.Playing;
	}

	internal static void OnSongFinishedPlaying(object sender, EventArgs args)
	{
		MediaPlayer._numSongsInQueuePlayed++;
		if (MediaPlayer._numSongsInQueuePlayed >= MediaPlayer._queue.Count)
		{
			MediaPlayer._numSongsInQueuePlayed = 0;
			if (!MediaPlayer.IsRepeating)
			{
				MediaPlayer.Stop();
				EventHelpers.Raise(null, MediaPlayer.ActiveSongChanged, EventArgs.Empty);
				return;
			}
		}
		MediaPlayer.MoveNext();
	}

	public static void Resume()
	{
		if (MediaPlayer.State == MediaState.Paused)
		{
			MediaPlayer.PlatformResume();
			MediaPlayer.State = MediaState.Playing;
		}
	}

	public static void Stop()
	{
		if (MediaPlayer.State != MediaState.Stopped)
		{
			MediaPlayer.PlatformStop();
			MediaPlayer.State = MediaState.Stopped;
		}
	}

	public static void MoveNext()
	{
		MediaPlayer.NextSong(1);
	}

	public static void MovePrevious()
	{
		MediaPlayer.NextSong(-1);
	}

	private static void NextSong(int direction)
	{
		MediaPlayer.Stop();
		if (MediaPlayer.IsRepeating && MediaPlayer._queue.ActiveSongIndex >= MediaPlayer._queue.Count - 1)
		{
			MediaPlayer._queue.ActiveSongIndex = 0;
			direction = 0;
		}
		Song nextSong = MediaPlayer._queue.GetNextSong(direction, MediaPlayer.IsShuffled);
		if (nextSong != null)
		{
			MediaPlayer.PlaySong(nextSong, null);
		}
		EventHelpers.Raise(null, MediaPlayer.ActiveSongChanged, EventArgs.Empty);
	}

	private static void PlatformInitialize()
	{
	}

	private static bool PlatformGetIsMuted()
	{
		return MediaPlayer._isMuted;
	}

	private static void PlatformSetIsMuted(bool muted)
	{
		MediaPlayer._isMuted = muted;
		if (MediaPlayer._queue.Count != 0)
		{
			float newVolume = (MediaPlayer._isMuted ? 0f : MediaPlayer._volume);
			MediaPlayer._queue.SetVolume(newVolume);
		}
	}

	private static bool PlatformGetIsRepeating()
	{
		return MediaPlayer._isRepeating;
	}

	private static void PlatformSetIsRepeating(bool repeating)
	{
		MediaPlayer._isRepeating = repeating;
	}

	private static bool PlatformGetIsShuffled()
	{
		return MediaPlayer._isShuffled;
	}

	private static void PlatformSetIsShuffled(bool shuffled)
	{
		MediaPlayer._isShuffled = shuffled;
	}

	private static TimeSpan PlatformGetPlayPosition()
	{
		if (MediaPlayer._queue.ActiveSong == null)
		{
			return TimeSpan.Zero;
		}
		return MediaPlayer._queue.ActiveSong.Position;
	}

	private static MediaState PlatformGetState()
	{
		return MediaPlayer._state;
	}

	private static float PlatformGetVolume()
	{
		return MediaPlayer._volume;
	}

	private static void PlatformSetVolume(float volume)
	{
		MediaPlayer._volume = volume;
		if (!(MediaPlayer._queue.ActiveSong == null))
		{
			MediaPlayer._queue.SetVolume(MediaPlayer._isMuted ? 0f : MediaPlayer._volume);
		}
	}

	private static bool PlatformGetGameHasControl()
	{
		return true;
	}

	private static void PlatformPause()
	{
		if (!(MediaPlayer._queue.ActiveSong == null))
		{
			MediaPlayer._queue.ActiveSong.Pause();
		}
	}

	private static void PlatformPlaySong(Song song, TimeSpan? startPosition)
	{
		if (!(MediaPlayer._queue.ActiveSong == null))
		{
			song.SetEventHandler(OnSongFinishedPlaying);
			song.Volume = (MediaPlayer._isMuted ? 0f : MediaPlayer._volume);
			song.Play(startPosition);
		}
	}

	private static void PlatformResume()
	{
		if (!(MediaPlayer._queue.ActiveSong == null))
		{
			MediaPlayer._queue.ActiveSong.Resume();
		}
	}

	private static void PlatformStop()
	{
		foreach (Song song in MediaPlayer.Queue.Songs)
		{
			_ = song;
			MediaPlayer._queue.ActiveSong.Stop();
		}
	}
}
