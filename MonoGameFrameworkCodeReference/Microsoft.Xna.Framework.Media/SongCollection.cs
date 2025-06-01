using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media;

public class SongCollection : ICollection<Song>, IEnumerable<Song>, IEnumerable, IDisposable
{
	private bool isReadOnly;

	private List<Song> innerlist = new List<Song>();

	public int Count => this.innerlist.Count;

	public bool IsReadOnly => this.isReadOnly;

	public Song this[int index] => this.innerlist[index];

	internal SongCollection()
	{
	}

	internal SongCollection(List<Song> songs)
	{
		this.innerlist = songs;
	}

	public void Dispose()
	{
	}

	public IEnumerator<Song> GetEnumerator()
	{
		return this.innerlist.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.innerlist.GetEnumerator();
	}

	public void Add(Song item)
	{
		if (item == null)
		{
			throw new ArgumentNullException();
		}
		if (this.innerlist.Count == 0)
		{
			this.innerlist.Add(item);
			return;
		}
		for (int i = 0; i < this.innerlist.Count; i++)
		{
			if (item.TrackNumber < this.innerlist[i].TrackNumber)
			{
				this.innerlist.Insert(i, item);
				return;
			}
		}
		this.innerlist.Add(item);
	}

	public void Clear()
	{
		this.innerlist.Clear();
	}

	public SongCollection Clone()
	{
		SongCollection sc = new SongCollection();
		foreach (Song song in this.innerlist)
		{
			sc.Add(song);
		}
		return sc;
	}

	public bool Contains(Song item)
	{
		return this.innerlist.Contains(item);
	}

	public void CopyTo(Song[] array, int arrayIndex)
	{
		this.innerlist.CopyTo(array, arrayIndex);
	}

	public int IndexOf(Song item)
	{
		return this.innerlist.IndexOf(item);
	}

	public bool Remove(Song item)
	{
		return this.innerlist.Remove(item);
	}
}
