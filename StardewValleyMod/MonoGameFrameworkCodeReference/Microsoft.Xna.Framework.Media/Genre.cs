using System;

namespace Microsoft.Xna.Framework.Media;

public sealed class Genre : IDisposable
{
	private string genre;

	/// <summary>
	/// Gets the AlbumCollection for the Genre.
	/// </summary>
	public AlbumCollection Albums
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Gets a value indicating whether the object is disposed.
	/// </summary>
	public bool IsDisposed => true;

	/// <summary>
	/// Gets the name of the Genre.
	/// </summary>
	public string Name => this.genre;

	/// <summary>
	/// Gets the SongCollection for the Genre.
	/// </summary>
	public SongCollection Songs
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Genre(string genre)
	{
		this.genre = genre;
	}

	/// <summary>
	/// Immediately releases the unmanaged resources used by this object.
	/// </summary>
	public void Dispose()
	{
	}

	/// <summary>
	/// Returns a String representation of the Genre.
	/// </summary>
	public override string ToString()
	{
		return this.genre;
	}

	/// <summary>
	/// Gets the hash code for this instance.
	/// </summary>
	public override int GetHashCode()
	{
		return this.genre.GetHashCode();
	}
}
