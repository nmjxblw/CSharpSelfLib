using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode;

public class NetList<T, TField> : AbstractNetSerializable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<NetList<T, TField>> where TField : NetField<T, TField>, new()
{
	public delegate void ElementChangedEvent(NetList<T, TField> list, int index, T oldValue, T newValue);

	public delegate void ArrayReplacedEvent(NetList<T, TField> list, IList<T> before, IList<T> after);

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly NetList<T, TField> _list;

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

		public Enumerator(NetList<T, TField> list)
		{
			this._list = list;
			this._index = 0;
			this._current = default(T);
			this._done = false;
		}

		public bool MoveNext()
		{
			int count = this._list.count.Value;
			if (this._index < count)
			{
				this._current = this._list.array.Value[this._index];
				this._index++;
				return true;
			}
			this._done = true;
			this._current = default(T);
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			this._index = 0;
			this._current = default(T);
			this._done = false;
		}
	}

	private const int initialSize = 10;

	private const double resizeFactor = 1.5;

	protected readonly NetInt count = new NetInt(0).Interpolated(interpolate: false, wait: false);

	protected readonly NetRef<NetArray<T, TField>> array = new NetRef<NetArray<T, TField>>(new NetArray<T, TField>(10)).Interpolated(interpolate: false, wait: false);

	public virtual T this[int index]
	{
		get
		{
			if (index >= this.Count || index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			return this.array.Value[index];
		}
		set
		{
			if (index >= this.Count || index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.array.Value[index] = value;
		}
	}

	public int Count => this.count.Value;

	public int Capacity => this.array.Value.Count;

	public bool IsReadOnly => false;

	public event ElementChangedEvent OnElementChanged;

	public event ArrayReplacedEvent OnArrayReplaced;

	public NetList()
	{
		this.hookArray(this.array.Value);
		this.array.fieldChangeVisibleEvent += delegate(NetRef<NetArray<T, TField>> arrayRef, NetArray<T, TField> oldArray, NetArray<T, TField> newArray)
		{
			if (newArray != null)
			{
				this.hookArray(newArray);
			}
			this.OnArrayReplaced?.Invoke(this, oldArray, newArray);
		};
	}

	public NetList(IEnumerable<T> values)
		: this()
	{
		foreach (T value in values)
		{
			this.Add(value);
		}
	}

	public NetList(int capacity)
		: this()
	{
		this.Resize(capacity);
	}

	private void hookField(int index, TField field)
	{
		if (!(field == null))
		{
			field.fieldChangeVisibleEvent += delegate(TField f, T oldValue, T newValue)
			{
				this.OnElementChanged?.Invoke(this, index, oldValue, newValue);
			};
		}
	}

	private void hookArray(NetArray<T, TField> array)
	{
		for (int i = 0; i < array.Count; i++)
		{
			this.hookField(i, array.Fields[i]);
		}
		array.OnFieldCreate += hookField;
	}

	private void Resize(int capacity)
	{
		this.count.Set(Math.Min(capacity, this.count.Value));
		NetArray<T, TField> oldArray = this.array.Value;
		NetArray<T, TField> newArray = new NetArray<T, TField>(capacity);
		this.array.Value = newArray;
		for (int i = 0; i < capacity && i < this.Count; i++)
		{
			T tmp = oldArray[i];
			oldArray[i] = default(T);
			this.array.Value[i] = tmp;
		}
	}

	private void EnsureCapacity(int neededCapacity)
	{
		if (neededCapacity > this.Capacity)
		{
			int newCapacity = (int)((double)this.Capacity * 1.5);
			while (neededCapacity > newCapacity)
			{
				newCapacity = (int)((double)newCapacity * 1.5);
			}
			this.Resize(newCapacity);
		}
	}

	public virtual void Add(T item)
	{
		this.EnsureCapacity(this.Count + 1);
		this.array.Value[this.Count] = item;
		this.count.Set(this.count.Value + 1);
	}

	public virtual void Clear()
	{
		this.count.Set(0);
		this.Resize(10);
		this.fillNull();
	}

	private void fillNull()
	{
		for (int i = 0; i < this.Capacity; i++)
		{
			this.array.Value[i] = default(T);
		}
	}

	public virtual void CopyFrom(IList<T> list)
	{
		if (list != this)
		{
			this.EnsureCapacity(list.Count);
			this.fillNull();
			this.count.Set(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				this.array.Value[i] = list[i];
			}
		}
	}

	public void Set(IList<T> list)
	{
		this.CopyFrom(list);
	}

	public void MoveFrom(NetList<T, TField> list)
	{
		List<T> values = new List<T>(list);
		list.Clear();
		this.Set(values);
	}

	public bool Any()
	{
		return this.count.Value > 0;
	}

	public virtual bool Contains(T item)
	{
		foreach (T item2 in this)
		{
			if (object.Equals(item2, item))
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

	public bool Equals(NetList<T, TField> other)
	{
		return object.Equals(this.array, other.array);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public virtual int IndexOf(T item)
	{
		for (int i = 0; i < this.Count; i++)
		{
			if (object.Equals(this.array.Value[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	public virtual void Insert(int index, T item)
	{
		if (index > this.Count || index < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		this.EnsureCapacity(this.Count + 1);
		this.count.Set(this.count.Value + 1);
		for (int i = this.Count - 1; i > index; i--)
		{
			T tmp = this.array.Value[i - 1];
			this.array.Value[i - 1] = default(T);
			this.array.Value[i] = tmp;
		}
		this.array.Value[index] = item;
	}

	public override void Read(BinaryReader reader, NetVersion version)
	{
		this.count.Read(reader, version);
		this.array.Read(reader, version);
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.count.ReadFull(reader, version);
		this.array.ReadFull(reader, version);
	}

	public bool Remove(T item)
	{
		int index = this.IndexOf(item);
		if (index != -1)
		{
			this.RemoveAt(index);
			return true;
		}
		return false;
	}

	public virtual void RemoveAt(int index)
	{
		if (index < 0 || index >= this.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		this.count.Set(this.count.Value - 1);
		for (int i = index; i < this.Count; i++)
		{
			T tmp = this.array.Value[i + 1];
			this.array.Value[i + 1] = default(T);
			this.array.Value[i] = tmp;
		}
		this.array.Value[this.Count] = default(T);
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of values removed from the set.</returns>
	public int RemoveWhere(Func<T, bool> match)
	{
		int count = 0;
		for (int i = this.Count - 1; i >= 0; i--)
		{
			if (match(this[i]))
			{
				this.RemoveAt(i);
				count++;
			}
		}
		return count;
	}

	/// <summary>Remove all entries which don't match the filter.</summary>
	/// <param name="f">Get whether to keep the given item.</param>
	[Obsolete("Use RemoveWhere instead.")]
	public void Filter(Func<T, bool> f)
	{
		this.RemoveWhere((T pair) => !f(pair));
	}

	public override void Write(BinaryWriter writer)
	{
		this.count.Write(writer);
		this.array.Write(writer);
	}

	public override void WriteFull(BinaryWriter writer)
	{
		this.count.WriteFull(writer);
		this.array.WriteFull(writer);
	}

	protected override void ForEachChild(Action<INetSerializable> childAction)
	{
		childAction(this.count);
		childAction(this.array);
	}

	public override string ToString()
	{
		return string.Join(",", this);
	}
}
