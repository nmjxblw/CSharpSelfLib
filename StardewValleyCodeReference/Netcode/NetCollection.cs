using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode;

public sealed class NetCollection<T> : AbstractNetSerializable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<NetCollection<T>> where T : class, INetObject<INetSerializable>
{
	public delegate void ContentsChangeEvent(T value);

	private List<Guid> guids = new List<Guid>();

	private List<T> list = new List<T>();

	private NetGuidDictionary<T, NetRef<T>> elements = new NetGuidDictionary<T, NetRef<T>>();

	public int Count => this.list.Count;

	public bool IsReadOnly => false;

	public bool InterpolationWait
	{
		get
		{
			return this.elements.InterpolationWait;
		}
		set
		{
			this.elements.InterpolationWait = value;
		}
	}

	public T this[int index]
	{
		get
		{
			return this.list[index];
		}
		set
		{
			this.elements[this.guids[index]] = value;
		}
	}

	public T this[Guid guid] => this.elements[guid];

	public event ContentsChangeEvent OnValueAdded;

	public event ContentsChangeEvent OnValueRemoved;

	public NetCollection()
	{
		this.elements.OnValueTargetUpdated += delegate(Guid guid, T old_target_value, T new_target_value)
		{
			if (old_target_value != new_target_value)
			{
				int num = this.guids.IndexOf(guid);
				if (num == -1)
				{
					this.guids.Add(guid);
					this.list.Add(new_target_value);
				}
				else
				{
					this.list[num] = new_target_value;
				}
			}
		};
		this.elements.OnValueAdded += delegate(Guid guid, T value)
		{
			int num = this.guids.IndexOf(guid);
			if (num == -1)
			{
				this.guids.Add(guid);
				this.list.Add(value);
			}
			else
			{
				this.list[num] = value;
			}
			this.OnValueAdded?.Invoke(value);
		};
		this.elements.OnValueRemoved += delegate(Guid guid, T value)
		{
			int num = this.guids.IndexOf(guid);
			if (num != -1)
			{
				this.guids.RemoveAt(num);
				this.list.RemoveAt(num);
			}
			this.OnValueRemoved?.Invoke(value);
		};
	}

	public NetCollection(IEnumerable<T> values)
		: this()
	{
		foreach (T value in values)
		{
			this.Add(value);
		}
	}

	/// <summary>Try to get a value from the collection by its ID.</summary>
	/// <param name="id">The entry ID.</param>
	/// <param name="value">The entry value, if found.</param>
	/// <returns>Returns whether a matching entry was found.</returns>
	public bool TryGetValue(Guid id, out T value)
	{
		return this.elements.TryGetValue(id, out value);
	}

	public void Add(T item)
	{
		Guid key = Guid.NewGuid();
		this.elements.Add(key, item);
	}

	public bool Equals(NetCollection<T> other)
	{
		return this.elements.Equals(other.elements);
	}

	public List<T>.Enumerator GetEnumerator()
	{
		return this.list.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return this.list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public void Clear()
	{
		this.elements.Clear();
	}

	public void Set(ICollection<T> other)
	{
		this.Clear();
		foreach (T elem in other)
		{
			this.Add(elem);
		}
	}

	public bool Contains(T item)
	{
		return this.list.Contains(item);
	}

	public bool ContainsGuid(Guid guid)
	{
		return this.elements.ContainsKey(guid);
	}

	public Guid GuidOf(T item)
	{
		for (int i = 0; i < this.list.Count; i++)
		{
			if (this.list[i] == item)
			{
				return this.guids[i];
			}
		}
		return Guid.Empty;
	}

	public int IndexOf(T item)
	{
		return this.list.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		throw new NotSupportedException();
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

	public bool Remove(T item)
	{
		foreach (Guid key in this.guids)
		{
			if (this.elements[key] == item)
			{
				this.elements.Remove(key);
				return true;
			}
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		this.elements.Remove(this.guids[index]);
	}

	public void Remove(Guid guid)
	{
		this.elements.Remove(guid);
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of values removed from the collection.</returns>
	public int RemoveWhere(Func<T, bool> match)
	{
		int count = 0;
		for (int i = this.list.Count - 1; i >= 0; i--)
		{
			if (match(this.list[i]))
			{
				this.elements.Remove(this.guids[i]);
				count++;
			}
		}
		return count;
	}

	[Obsolete("Use RemoveWhere instead.")]
	public void Filter(Func<T, bool> f)
	{
		this.RemoveWhere((T pair) => !f(pair));
	}

	protected override void ForEachChild(Action<INetSerializable> childAction)
	{
		childAction(this.elements);
	}

	public override void Read(BinaryReader reader, NetVersion version)
	{
		this.elements.Read(reader, version);
	}

	public override void Write(BinaryWriter writer)
	{
		this.elements.Write(writer);
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.elements.ReadFull(reader, version);
	}

	public override void WriteFull(BinaryWriter writer)
	{
		this.elements.WriteFull(writer);
	}
}
