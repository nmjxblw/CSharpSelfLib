using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework;

internal class ReusableItemList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
{
	private readonly List<T> _list = new List<T>();

	private int _listTop;

	private int _iteratorIndex;

	public T this[int index]
	{
		get
		{
			if (index >= this._listTop)
			{
				throw new IndexOutOfRangeException();
			}
			return this._list[index];
		}
		set
		{
			if (index >= this._listTop)
			{
				throw new IndexOutOfRangeException();
			}
			this._list[index] = value;
		}
	}

	public int Count => this._listTop;

	public bool IsReadOnly => false;

	public T Current => this._list[this._iteratorIndex];

	object IEnumerator.Current => this._list[this._iteratorIndex];

	public void Add(T item)
	{
		if (this._list.Count > this._listTop)
		{
			this._list[this._listTop] = item;
		}
		else
		{
			this._list.Add(item);
		}
		this._listTop++;
	}

	public void Sort(IComparer<T> comparison)
	{
		this._list.Sort(comparison);
	}

	public T GetNewItem()
	{
		if (this._listTop < this._list.Count)
		{
			return this._list[this._listTop++];
		}
		return default(T);
	}

	public void Clear()
	{
		this._listTop = 0;
	}

	public void Reset()
	{
		this.Clear();
		this._list.Clear();
	}

	public bool Contains(T item)
	{
		return this._list.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		this._list.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<T> GetEnumerator()
	{
		this._iteratorIndex = -1;
		return this;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		this._iteratorIndex = -1;
		return this;
	}

	public void Dispose()
	{
	}

	public bool MoveNext()
	{
		this._iteratorIndex++;
		return this._iteratorIndex < this._listTop;
	}
}
