using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Newtonsoft.Json.Utilities;

internal class CollectionWrapper<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IWrappedCollection, IList, ICollection
{
	private readonly IList? _list;

	private readonly ICollection<T>? _genericCollection;

	private object? _syncRoot;

	public virtual int Count
	{
		get
		{
			if (this._genericCollection != null)
			{
				return this._genericCollection.Count;
			}
			return this._list.Count;
		}
	}

	public virtual bool IsReadOnly
	{
		get
		{
			if (this._genericCollection != null)
			{
				return this._genericCollection.IsReadOnly;
			}
			return this._list.IsReadOnly;
		}
	}

	bool IList.IsFixedSize
	{
		get
		{
			if (this._genericCollection != null)
			{
				return this._genericCollection.IsReadOnly;
			}
			return this._list.IsFixedSize;
		}
	}

	object? IList.this[int index]
	{
		get
		{
			if (this._genericCollection != null)
			{
				throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
			}
			return this._list[index];
		}
		set
		{
			if (this._genericCollection != null)
			{
				throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
			}
			CollectionWrapper<T>.VerifyValueType(value);
			this._list[index] = (T)value;
		}
	}

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (this._syncRoot == null)
			{
				Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
			}
			return this._syncRoot;
		}
	}

	public object UnderlyingCollection => ((object)this._genericCollection) ?? ((object)this._list);

	public CollectionWrapper(IList list)
	{
		ValidationUtils.ArgumentNotNull(list, "list");
		if (list is ICollection<T> genericCollection)
		{
			this._genericCollection = genericCollection;
		}
		else
		{
			this._list = list;
		}
	}

	public CollectionWrapper(ICollection<T> list)
	{
		ValidationUtils.ArgumentNotNull(list, "list");
		this._genericCollection = list;
	}

	public virtual void Add(T item)
	{
		if (this._genericCollection != null)
		{
			this._genericCollection.Add(item);
		}
		else
		{
			this._list.Add(item);
		}
	}

	public virtual void Clear()
	{
		if (this._genericCollection != null)
		{
			this._genericCollection.Clear();
		}
		else
		{
			this._list.Clear();
		}
	}

	public virtual bool Contains(T item)
	{
		if (this._genericCollection != null)
		{
			return this._genericCollection.Contains(item);
		}
		return this._list.Contains(item);
	}

	public virtual void CopyTo(T[] array, int arrayIndex)
	{
		if (this._genericCollection != null)
		{
			this._genericCollection.CopyTo(array, arrayIndex);
		}
		else
		{
			this._list.CopyTo(array, arrayIndex);
		}
	}

	public virtual bool Remove(T item)
	{
		if (this._genericCollection != null)
		{
			return this._genericCollection.Remove(item);
		}
		bool num = this._list.Contains(item);
		if (num)
		{
			this._list.Remove(item);
		}
		return num;
	}

	public virtual IEnumerator<T> GetEnumerator()
	{
		IEnumerable<T> genericCollection = this._genericCollection;
		return (genericCollection ?? this._list.Cast<T>()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		IEnumerable genericCollection = this._genericCollection;
		return (genericCollection ?? this._list).GetEnumerator();
	}

	int IList.Add(object? value)
	{
		CollectionWrapper<T>.VerifyValueType(value);
		this.Add((T)value);
		return this.Count - 1;
	}

	bool IList.Contains(object? value)
	{
		if (CollectionWrapper<T>.IsCompatibleObject(value))
		{
			return this.Contains((T)value);
		}
		return false;
	}

	int IList.IndexOf(object? value)
	{
		if (this._genericCollection != null)
		{
			throw new InvalidOperationException("Wrapped ICollection<T> does not support IndexOf.");
		}
		if (CollectionWrapper<T>.IsCompatibleObject(value))
		{
			return this._list.IndexOf((T)value);
		}
		return -1;
	}

	void IList.RemoveAt(int index)
	{
		if (this._genericCollection != null)
		{
			throw new InvalidOperationException("Wrapped ICollection<T> does not support RemoveAt.");
		}
		this._list.RemoveAt(index);
	}

	void IList.Insert(int index, object? value)
	{
		if (this._genericCollection != null)
		{
			throw new InvalidOperationException("Wrapped ICollection<T> does not support Insert.");
		}
		CollectionWrapper<T>.VerifyValueType(value);
		this._list.Insert(index, (T)value);
	}

	void IList.Remove(object? value)
	{
		if (CollectionWrapper<T>.IsCompatibleObject(value))
		{
			this.Remove((T)value);
		}
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		this.CopyTo((T[])array, arrayIndex);
	}

	private static void VerifyValueType(object? value)
	{
		if (!CollectionWrapper<T>.IsCompatibleObject(value))
		{
			throw new ArgumentException("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.".FormatWith(CultureInfo.InvariantCulture, value, typeof(T)), "value");
		}
	}

	private static bool IsCompatibleObject(object? value)
	{
		if (!(value is T) && (value != null || (typeof(T).IsValueType() && !ReflectionUtils.IsNullableType(typeof(T)))))
		{
			return false;
		}
		return true;
	}
}
