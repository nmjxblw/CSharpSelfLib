using System;
using System.IO;

namespace Microsoft.Xna.Framework.Media;

public sealed class Album : IDisposable
{
	private Artist artist;

	private Genre genre;

	private string album;

	private SongCollection songCollection;

	public Artist Artist => this.artist;

	/// <summary>
	/// Gets the duration of the Album.
	/// </summary>
	public TimeSpan Duration => TimeSpan.Zero;

	/// <summary>
	/// Gets the Genre of the Album.
	/// </summary>
	public Genre Genre => this.genre;

	/// <summary>
	/// Gets a value indicating whether the Album has associated album art.
	/// </summary>
	public bool HasArt => false;

	/// <summary>
	/// Gets a value indicating whether the object is disposed.
	/// </summary>
	public bool IsDisposed => false;

	/// <summary>
	/// Gets the name of the Album.
	/// </summary>
	public string Name => this.album;

	/// <summary>
	/// Gets a SongCollection that contains the songs on the album.
	/// </summary>
	public SongCollection Songs => this.songCollection;

	private Album(SongCollection songCollection, string name, Artist artist, Genre genre)
	{
		this.songCollection = songCollection;
		this.album = name;
		this.artist = artist;
		this.genre = genre;
	}

	/// <summary>
	/// Immediately releases the unmanaged resources used by this object.
	/// </summary>
	public void Dispose()
	{
	}

	/// <summary>
	/// Returns the stream that contains the album art image data.
	/// </summary>
	public Stream GetAlbumArt()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Returns the stream that contains the album thumbnail image data.
	/// </summary>
	public Stream GetThumbnail()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Returns a String representation of this Album.
	/// </summary>
	public override string ToString()
	{
		return this.album.ToString();
	}

	/// <summary>
	/// Gets the hash code for this instance.
	/// </summary>
	public override int GetHashCode()
	{
		return this.album.GetHashCode();
	}
}
