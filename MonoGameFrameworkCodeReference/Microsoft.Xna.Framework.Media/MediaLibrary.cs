using System;

namespace Microsoft.Xna.Framework.Media;

public class MediaLibrary : IDisposable
{
	public AlbumCollection Albums => this.PlatformGetAlbums();

	public bool IsDisposed { get; private set; }

	public MediaSource MediaSource => null;

	public SongCollection Songs => this.PlatformGetSongs();

	public MediaLibrary()
	{
	}

	/// <summary>
	/// Load the contents of MediaLibrary. This blocking call might take up to a few minutes depending on the platform and the size of the user's music library.
	/// </summary>
	/// <param name="progressCallback">Callback that reports back the progress of the music library loading in percents (0-100).</param>
	public void Load(Action<int> progressCallback = null)
	{
		this.PlatformLoad(progressCallback);
	}

	public MediaLibrary(MediaSource mediaSource)
	{
		throw new NotSupportedException("Initializing from MediaSource is not supported");
	}

	public void Dispose()
	{
		this.PlatformDispose();
		this.IsDisposed = true;
	}

	private void PlatformLoad(Action<int> progressCallback)
	{
	}

	private AlbumCollection PlatformGetAlbums()
	{
		return null;
	}

	private SongCollection PlatformGetSongs()
	{
		return null;
	}

	private void PlatformDispose()
	{
	}
}
