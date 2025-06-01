using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode;

public class NetObjectShrinkList<T> : AbstractNetSerializable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<NetObjectShrinkList<T>> where T : class, INetObject<INetSerializable>
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly NetArray<T, NetRef<T>> _array;

		private int _index;

		private T _current;

		private bool _done;

		public T Current => this._current;

		object IEnumerator.Current
		{
			get
			{
				if (this._done)
				{
					throw new InvalidOperationException();
				}
				return this._current;
			}
		}

		public Enumerator(NetArray<T, NetRef<T>> array)
		{
			this._array = array;
			this._index = 0;
			this._current = null;
			this._done = false;
		}

		public bool MoveNext()
		{
			while (this._index < this._array.Count)
			{
				T v = this._array[this._index];
				this._index++;
				if (v != null)
				{
					this._current = v;
					return true;
				}
			}
			this._done = true;
			this._current = null;
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			this._index = 0;
			this._current = null;
			this._done = false;
		}
	}

	private NetArray<T, NetRef<T>> array = new NetArray<T, NetRef<T>>();

	public T this[int index]
	{
		get
		{
			int count = 0;
			for (int i = 0; i < this.array.Count; i++)
			{
				T v = this.array[i];
				if (v != null)
				{
					if (index == count)
					{
						return v;
					}
					count++;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
		set
		{
			int count = 0;
			for (int i = 0; i < this.array.Count; i++)
			{
				if (this.array[i] != null)
				{
					if (index == count)
					{
						this.array[i] = value;
						return;
					}
					count++;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public int Count
	{
		get
		{
			int count = 0;
			for (int i = 0; i < this.array.Count; i++)
			{
				if (this.array[i] != null)
				{
					count++;
				}
			}
			return count;
		}
	}

	public bool IsReadOnly => false;

	public NetObjectShrinkList()
	{
	}

	public NetObjectShrinkList(IEnumerable<T> values)
		: this()
	{
		foreach (T value in values)
		{
			this.array.Add(value);
		}
	}

	public void Add(T item)
	{
		this.array.Add(item);
	}

	public void Clear()
	{
		for (int i = 0; i < this.array.Count; i++)
		{
			this.array[i] = null;
		}
	}

	public void CopyFrom(IList<T> list)
	{
		if (list == this)
		{
			return;
		}
		if (list.Count > this.array.Count)
		{
			throw new InvalidOperationException();
		}
		for (int i = 0; i < this.array.Count; i++)
		{
			if (i < list.Count)
			{
				this.array[i] = list[i];
			}
			else
			{
				this.array[i] = null;
			}
		}
	}

	public void Set(IList<T> list)
	{
		this.CopyFrom(list);
	}

	public void MoveFrom(IList<T> list)
	{
		List<T> values = new List<T>(list);
		list.Clear();
		this.Set(values);
	}

	public bool Contains(T item)
	{
		foreach (T item2 in this)
		{
			if (item2 == item)
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException();
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (this.Count - arrayIndex > array.Length)
		{
			throw new ArgumentException();
		}
		foreach (T value in this)
		{
			array[arrayIndex++] = value;
		}
	}

	public List<T> GetRange(int index, int count)
	{
		List<T> result = new List<T>();
		for (int i = index; i < index + count; i++)
		{
			result.Add(this[i]);
		}
		return result;
	}

	public void AddRange(IEnumerable<T> collection)
	{
		foreach (T value in collection)
		{
			this.Add(value);
		}
	}

	public void RemoveRange(int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			this.RemoveAt(index);
		}
	}

	public bool Equals(NetObjectShrinkList<T> other)
	{
		if (this.Count != other.Count)
		{
			return false;
		}
		for (int i = 0; i < this.Count; i++)
		{
			if (this[i] != other[i])
			{
				return false;
			}
		}
		return true;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this.array);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this.array);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this.array);
	}

	public int IndexOf(T item)
	{
		int index = 0;
		for (int i = 0; i < this.array.Count; i++)
		{
			T v = this.array[i];
			if (v != null)
			{
				if (v == item)
				{
					return index;
				}
				index++;
			}
		}
		return -1;
	}

	public void Insert(int index, T item)
	{
		int count = 0;
		for (int i = 0; i < this.array.Count; i++)
		{
			if (this.array[i] != null)
			{
				if (count == index)
				{
					this.array.Insert(i, item);
					return;
				}
				count++;
			}
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public override void Read(BinaryReader reader, NetVersion version)
	{
		this.array.Read(reader, version);
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.array.ReadFull(reader, version);
	}

	public bool Remove(T item)
	{
		for (int i = 0; i < this.array.Count; i++)
		{
			if (this.array[i] == item)
			{
				this.array[i] = null;
				return true;
			}
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		int count = 0;
		for (int i = 0; i < this.array.Count; i++)
		{
			if (this.array[i] != null)
			{
				if (count == index)
				{
					this.array[i] = null;
					break;
				}
				count++;
			}
		}
	}

	public override void Write(BinaryWriter writer)
	{
		this.array.Write(writer);
	}

	public override void WriteFull(BinaryWriter writer)
	{
		this.array.WriteFull(writer);
	}

	protected override void ForEachChild(Action<INetSerializable> childAction)
	{
		childAction(this.array);
	}

	public override string ToString()
	{
		return string.Join(",", this);
	}
}
