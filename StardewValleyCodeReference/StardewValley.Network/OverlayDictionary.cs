using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network;

public class OverlayDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	protected Dictionary<TKey, TValue> _dictionary;

	protected List<KeyValuePair<TKey, TValue>> _removedPairs = new List<KeyValuePair<TKey, TValue>>();

	public TValue this[TKey key]
	{
		get
		{
			return this._dictionary[key];
		}
		set
		{
			this._dictionary[key] = value;
			this.onValueAdded?.Invoke(key, value);
		}
	}

	public ICollection<TKey> Keys => this._dictionary.Keys;

	public ICollection<TValue> Values => this._dictionary.Values;

	public int Count => this._dictionary.Count;

	public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).IsReadOnly;

	public event Action<TKey, TValue> onValueAdded;

	public event Action<TKey, TValue> onValueRemoved;

	public OverlayDictionary()
	{
		this._dictionary = new Dictionary<TKey, TValue>();
	}

	public OverlayDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
	{
		this._dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
	}

	public OverlayDictionary(IEqualityComparer<TKey> comparer)
	{
		this._dictionary = new Dictionary<TKey, TValue>(comparer);
	}

	public void Add(TKey key, TValue value)
	{
		this._dictionary.Add(key, value);
		this.onValueAdded?.Invoke(key, value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		this.Add(item.Key, item.Value);
	}

	public void Clear()
	{
		this._removedPairs.AddRange(this._dictionary);
		((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Clear();
		foreach (KeyValuePair<TKey, TValue> pair in this._removedPairs)
		{
			this.onValueRemoved(pair.Key, pair.Value);
		}
		this._removedPairs.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Contains(item);
	}

	public bool ContainsKey(TKey key)
	{
		return this._dictionary.ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return this._dictionary.GetEnumerator();
	}

	public bool Remove(TKey key)
	{
		if (this._dictionary.TryGetValue(key, out var value))
		{
			this._dictionary.Remove(key);
			this.onValueRemoved?.Invoke(key, value);
			return true;
		}
		return false;
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (this.Contains(item))
		{
			return this.Remove(item.Key);
		}
		return false;
	}

	/// <summary>Get the value with a given key if it exists.</summary>
	/// <param name="key">The key to check.</param>
	/// <param name="value">The value that was found, if applicable.</param>
	/// <returns>Returns whether the value was found.</returns>
	public bool TryGetValue(TKey key, out TValue value)
	{
		return this._dictionary.TryGetValue(key, out value);
	}

	/// <summary>Get the value with a given key if it exists, else a default value.</summary>
	/// <param name="key">The key to check.</param>
	/// <param name="defaultValue">The value to return if the element isn't found.</param>
	public TValue GetValueOrDefault(TKey key, TValue defaultValue = default(TValue))
	{
		return this._dictionary.GetValueOrDefault(key, defaultValue);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._dictionary.GetEnumerator();
	}
}
