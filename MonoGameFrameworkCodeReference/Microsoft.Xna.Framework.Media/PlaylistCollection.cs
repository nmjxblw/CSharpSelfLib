using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media;

public sealed class PlaylistCollection : ICollection<Playlist>, IEnumerable<Playlist>, IEnumerable, IDisposable
{
	private bool isReadOnly;

	private List<Playlist> innerlist = new List<Playlist>();

	public int Count => this.innerlist.Count;

	public bool IsReadOnly => this.isReadOnly;

	public Playlist this[int index] => this.innerlist[index];

	public void Dispose()
	{
	}

	public IEnumerator<Playlist> GetEnumerator()
	{
		return this.innerlist.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.innerlist.GetEnumerator();
	}

	public void Add(Playlist item)
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
			if (item.Duration < this.innerlist[i].Duration)
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

	public PlaylistCollection Clone()
	{
		PlaylistCollection plc = new PlaylistCollection();
		foreach (Playlist playlist in this.innerlist)
		{
			plc.Add(playlist);
		}
		return plc;
	}

	public bool Contains(Playlist item)
	{
		return this.innerlist.Contains(item);
	}

	public void CopyTo(Playlist[] array, int arrayIndex)
	{
		this.innerlist.CopyTo(array, arrayIndex);
	}

	public int IndexOf(Playlist item)
	{
		return this.innerlist.IndexOf(item);
	}

	public bool Remove(Playlist item)
	{
		return this.innerlist.Remove(item);
	}
}
