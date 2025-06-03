using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media;

public sealed class AlbumCollection : IDisposable
{
	private List<Album> albumCollection;

	/// <summary>
	/// Gets the number of Album objects in the AlbumCollection.
	/// </summary>
	public int Count => this.albumCollection.Count;

	/// <summary>
	/// Gets a value indicating whether the object is disposed.
	/// </summary>
	public bool IsDisposed => false;

	/// <summary>
	/// Gets the Album at the specified index in the AlbumCollection.
	/// </summary>
	/// <param name="index">Index of the Album to get.</param>
	public Album this[int index] => this.albumCollection[index];

	public AlbumCollection(List<Album> albums)
	{
		this.albumCollection = albums;
	}

	/// <summary>
	/// Immediately releases the unmanaged resources used by this object.
	/// </summary>
	public void Dispose()
	{
		foreach (Album item in this.albumCollection)
		{
			item.Dispose();
		}
	}
}
