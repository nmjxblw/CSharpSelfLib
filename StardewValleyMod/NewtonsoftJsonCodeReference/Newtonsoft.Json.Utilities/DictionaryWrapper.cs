using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Newtonsoft.Json.Utilities;

internal class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IWrappedDictionary, IDictionary, ICollection
{
	private readonly struct DictionaryEnumerator<TEnumeratorKey, TEnumeratorValue> : IDictionaryEnumerator, IEnumerator
	{
		private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

		public DictionaryEntry Entry => (DictionaryEntry)this.Current;

		public object Key => this.Entry.Key;

		public object? Value => this.Entry.Value;

		public object Current => new DictionaryEntry(this._e.Current.Key, this._e.Current.Value);

		public DictionaryEnumerator(IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
		{
			ValidationUtils.ArgumentNotNull(e, "e");
			this._e = e;
		}

		public bool MoveNext()
		{
			return this._e.MoveNext();
		}

		public void Reset()
		{
			this._e.Reset();
		}
	}

	private readonly IDictionary? _dictionary;

	private readonly IDictionary<TKey, TValue>? _genericDictionary;

	private readonly IReadOnlyDictionary<TKey, TValue>? _readOnlyDictionary;

	private object? _syncRoot;

	internal IDictionary<TKey, TValue> GenericDictionary => this._genericDictionary;

	public ICollection<TKey> Keys
	{
		get
		{
			if (this._dictionary != null)
			{
				return this._dictionary.Keys.Cast<TKey>().ToList();
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary.Keys.ToList();
			}
			return this.GenericDictionary.Keys;
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			if (this._dictionary != null)
			{
				return this._dictionary.Values.Cast<TValue>().ToList();
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary.Values.ToList();
			}
			return this.GenericDictionary.Values;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			if (this._dictionary != null)
			{
				return (TValue)this._dictionary[key];
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary[key];
			}
			return this.GenericDictionary[key];
		}
		set
		{
			if (this._dictionary != null)
			{
				this._dictionary[key] = value;
				return;
			}
			if (this._readOnlyDictionary != null)
			{
				throw new NotSupportedException();
			}
			this.GenericDictionary[key] = value;
		}
	}

	public int Count
	{
		get
		{
			if (this._dictionary != null)
			{
				return this._dictionary.Count;
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary.Count;
			}
			return this.GenericDictionary.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			if (this._dictionary != null)
			{
				return this._dictionary.IsReadOnly;
			}
			if (this._readOnlyDictionary != null)
			{
				return true;
			}
			return this.GenericDictionary.IsReadOnly;
		}
	}

	object? IDictionary.this[object key]
	{
		get
		{
			if (this._dictionary != null)
			{
				return this._dictionary[key];
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary[(TKey)key];
			}
			return this.GenericDictionary[(TKey)key];
		}
		set
		{
			if (this._dictionary != null)
			{
				this._dictionary[key] = value;
				return;
			}
			if (this._readOnlyDictionary != null)
			{
				throw new NotSupportedException();
			}
			this.GenericDictionary[(TKey)key] = (TValue)value;
		}
	}

	bool IDictionary.IsFixedSize
	{
		get
		{
			if (this._genericDictionary != null)
			{
				return false;
			}
			if (this._readOnlyDictionary != null)
			{
				return true;
			}
			return this._dictionary.IsFixedSize;
		}
	}

	ICollection IDictionary.Keys
	{
		get
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.Keys.ToList();
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary.Keys.ToList();
			}
			return this._dictionary.Keys;
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			if (this._genericDictionary != null)
			{
				return this._genericDictionary.Values.ToList();
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary.Values.ToList();
			}
			return this._dictionary.Values;
		}
	}

	bool ICollection.IsSynchronized
	{
		get
		{
			if (this._dictionary != null)
			{
				return this._dictionary.IsSynchronized;
			}
			return false;
		}
	}

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

	public object UnderlyingDictionary
	{
		get
		{
			if (this._dictionary != null)
			{
				return this._dictionary;
			}
			if (this._readOnlyDictionary != null)
			{
				return this._readOnlyDictionary;
			}
			return this.GenericDictionary;
		}
	}

	public DictionaryWrapper(IDictionary dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		this._dictionary = dictionary;
	}

	public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		this._genericDictionary = dictionary;
	}

	public DictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
	{
		ValidationUtils.ArgumentNotNull(dictionary, "dictionary");
		this._readOnlyDictionary = dictionary;
	}

	public void Add(TKey key, TValue value)
	{
		if (this._dictionary != null)
		{
			this._dictionary.Add(key, value);
			return;
		}
		if (this._genericDictionary != null)
		{
			this._genericDictionary.Add(key, value);
			return;
		}
		throw new NotSupportedException();
	}

	public bool ContainsKey(TKey key)
	{
		if (this._dictionary != null)
		{
			return this._dictionary.Contains(key);
		}
		if (this._readOnlyDictionary != null)
		{
			return this._readOnlyDictionary.ContainsKey(key);
		}
		return this.GenericDictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		if (this._dictionary != null)
		{
			if (this._dictionary.Contains(key))
			{
				this._dictionary.Remove(key);
				return true;
			}
			return false;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return this.GenericDictionary.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue? value)
	{
		if (this._dictionary != null)
		{
			if (!this._dictionary.Contains(key))
			{
				value = default(TValue);
				return false;
			}
			value = (TValue)this._dictionary[key];
			return true;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return this.GenericDictionary.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		if (this._dictionary != null)
		{
			((IList)this._dictionary).Add(item);
			return;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		this._genericDictionary?.Add(item);
	}

	public void Clear()
	{
		if (this._dictionary != null)
		{
			this._dictionary.Clear();
			return;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		this.GenericDictionary.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (this._dictionary != null)
		{
			return ((IList)this._dictionary).Contains(item);
		}
		if (this._readOnlyDictionary != null)
		{
			return this._readOnlyDictionary.Contains(item);
		}
		return this.GenericDictionary.Contains(item);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (this._dictionary != null)
		{
			foreach (DictionaryEntry item in this._dictionary)
			{
				array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)item.Key, (TValue)item.Value);
			}
			return;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		this.GenericDictionary.CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (this._dictionary != null)
		{
			if (this._dictionary.Contains(item.Key))
			{
				if (object.Equals(this._dictionary[item.Key], item.Value))
				{
					this._dictionary.Remove(item.Key);
					return true;
				}
				return false;
			}
			return true;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		return this.GenericDictionary.Remove(item);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		if (this._dictionary != null)
		{
			return (from DictionaryEntry de in this._dictionary
				select new KeyValuePair<TKey, TValue>((TKey)de.Key, (TValue)de.Value)).GetEnumerator();
		}
		if (this._readOnlyDictionary != null)
		{
			return this._readOnlyDictionary.GetEnumerator();
		}
		return this.GenericDictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	void IDictionary.Add(object key, object? value)
	{
		if (this._dictionary != null)
		{
			this._dictionary.Add(key, value);
			return;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		this.GenericDictionary.Add((TKey)key, (TValue)value);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		if (this._dictionary != null)
		{
			return this._dictionary.GetEnumerator();
		}
		if (this._readOnlyDictionary != null)
		{
			return new DictionaryEnumerator<TKey, TValue>(this._readOnlyDictionary.GetEnumerator());
		}
		return new DictionaryEnumerator<TKey, TValue>(this.GenericDictionary.GetEnumerator());
	}

	bool IDictionary.Contains(object key)
	{
		if (this._genericDictionary != null)
		{
			return this._genericDictionary.ContainsKey((TKey)key);
		}
		if (this._readOnlyDictionary != null)
		{
			return this._readOnlyDictionary.ContainsKey((TKey)key);
		}
		return this._dictionary.Contains(key);
	}

	public void Remove(object key)
	{
		if (this._dictionary != null)
		{
			this._dictionary.Remove(key);
			return;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		this.GenericDictionary.Remove((TKey)key);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (this._dictionary != null)
		{
			this._dictionary.CopyTo(array, index);
			return;
		}
		if (this._readOnlyDictionary != null)
		{
			throw new NotSupportedException();
		}
		this.GenericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
	}
}
