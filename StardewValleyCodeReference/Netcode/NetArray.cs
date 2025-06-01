using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode;

public class NetArray<T, TField> : AbstractNetSerializable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IEquatable<NetArray<T, TField>> where TField : NetField<T, TField>, new()
{
	public delegate void FieldCreateEvent(int index, TField field);

	private int appendPosition;

	private readonly List<TField> elements = new List<TField>();

	public List<TField> Fields => this.elements;

	public T this[int index]
	{
		get
		{
			return this.elements[index].Get();
		}
		set
		{
			this.elements[index].Set(value);
		}
	}

	public int Count => this.elements.Count;

	public int Length => this.elements.Count;

	public bool IsReadOnly => false;

	public bool IsFixedSize => base.Parent != null;

	public event FieldCreateEvent OnFieldCreate;

	public NetArray()
	{
	}

	public NetArray(IEnumerable<T> values)
		: this()
	{
		int i = 0;
		foreach (T value in values)
		{
			TField field = this.createField(i++);
			field.Set(value);
			this.elements.Add(field);
		}
	}

	public NetArray(int size)
		: this()
	{
		for (int i = 0; i < size; i++)
		{
			this.elements.Add(this.createField(i));
		}
	}

	private TField createField(int index)
	{
		TField field = new TField().Interpolated(interpolate: false, wait: false);
		this.OnFieldCreate?.Invoke(index, field);
		return field;
	}

	public void Add(T item)
	{
		if (this.IsFixedSize)
		{
			throw new InvalidOperationException();
		}
		while (this.appendPosition >= this.elements.Count)
		{
			this.elements.Add(this.createField(this.elements.Count));
		}
		this.elements[this.appendPosition].Set(item);
		this.appendPosition++;
	}

	public void Clear()
	{
		if (this.IsFixedSize)
		{
			throw new InvalidOperationException();
		}
		this.elements.Clear();
	}

	public bool Contains(T item)
	{
		foreach (TField element in this.elements)
		{
			if (object.Equals(element.Get(), item))
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

	private void ensureCapacity(int size)
	{
		if (this.IsFixedSize && size != this.Count)
		{
			throw new InvalidOperationException();
		}
		while (this.Count < size)
		{
			this.elements.Add(this.createField(this.Count));
		}
	}

	public void SetCount(int size)
	{
		this.ensureCapacity(size);
	}

	public void Set(IList<T> values)
	{
		this.ensureCapacity(values.Count);
		for (int i = 0; i < this.Count; i++)
		{
			this[i] = values[i];
		}
	}

	public bool Equals(NetArray<T, TField> other)
	{
		return object.Equals(this.elements, other.elements);
	}

	public override bool Equals(object obj)
	{
		if (obj is NetArray<T, TField> otherArray)
		{
			return this.Equals(otherArray);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.elements.GetHashCode() ^ 0x300A5A8D;
	}

	public IEnumerator<T> GetEnumerator()
	{
		foreach (TField elementField in this.elements)
		{
			yield return elementField.Get();
		}
	}

	public int IndexOf(T item)
	{
		for (int i = 0; i < this.Count; i++)
		{
			if (object.Equals(this.elements[i].Get(), item))
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, T item)
	{
		if (this.IsFixedSize)
		{
			throw new InvalidOperationException();
		}
		TField field = this.createField(index);
		field.Set(item);
		this.elements.Insert(index, field);
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

	public void RemoveAt(int index)
	{
		if (this.IsFixedSize)
		{
			throw new InvalidOperationException();
		}
		this.elements.RemoveAt(index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public override void Read(BinaryReader reader, NetVersion version)
	{
		BitArray dirtyBits = reader.ReadBitArray();
		for (int i = 0; i < this.elements.Count; i++)
		{
			if (dirtyBits[i])
			{
				this.elements[i].Read(reader, version);
			}
		}
	}

	public override void Write(BinaryWriter writer)
	{
		BitArray dirtyBits = new BitArray(this.elements.Count);
		for (int i = 0; i < this.elements.Count; i++)
		{
			dirtyBits[i] = this.elements[i].Dirty;
		}
		writer.WriteBitArray(dirtyBits);
		for (int j = 0; j < this.elements.Count; j++)
		{
			if (dirtyBits[j])
			{
				this.elements[j].Write(writer);
			}
		}
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		int size = reader.ReadInt32();
		this.elements.Clear();
		for (int i = 0; i < size; i++)
		{
			TField element = this.createField(this.elements.Count);
			element.ReadFull(reader, version);
			if (base.Parent != null)
			{
				element.Parent = this;
			}
			this.elements.Add(element);
		}
	}

	public override void WriteFull(BinaryWriter writer)
	{
		writer.Write(this.Count);
		foreach (TField element in this.elements)
		{
			element.WriteFull(writer);
		}
	}

	protected override void ForEachChild(Action<INetSerializable> childAction)
	{
		foreach (TField elementField in this.elements)
		{
			childAction(elementField);
		}
	}

	public override string ToString()
	{
		return string.Join(",", this);
	}
}
