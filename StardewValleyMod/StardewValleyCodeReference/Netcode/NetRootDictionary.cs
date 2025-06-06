using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Netcode;

public class NetRootDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TValue : class, INetObject<INetSerializable>
{
	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
	{
		private Dictionary<TKey, NetRoot<TValue>> _roots;

		private Dictionary<TKey, NetRoot<TValue>>.Enumerator _enumerator;

		private KeyValuePair<TKey, TValue> _current;

		private bool _done;

		public KeyValuePair<TKey, TValue> Current => this._current;

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

		public Enumerator(Dictionary<TKey, NetRoot<TValue>> roots)
		{
			this._roots = roots;
			this._enumerator = this._roots.GetEnumerator();
			this._current = default(KeyValuePair<TKey, TValue>);
			this._done = false;
		}

		public bool MoveNext()
		{
			if (this._enumerator.MoveNext())
			{
				KeyValuePair<TKey, NetRoot<TValue>> pair = this._enumerator.Current;
				this._current = new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Get());
				return true;
			}
			this._done = true;
			this._current = default(KeyValuePair<TKey, TValue>);
			return false;
		}

		public void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			this._enumerator = this._roots.GetEnumerator();
			this._current = default(KeyValuePair<TKey, TValue>);
			this._done = false;
		}
	}

	public XmlSerializer Serializer;

	public Dictionary<TKey, NetRoot<TValue>> Roots = new Dictionary<TKey, NetRoot<TValue>>();

	public TValue this[TKey key]
	{
		get
		{
			return this.Roots[key].Get();
		}
		set
		{
			if (!this.ContainsKey(key))
			{
				this.Add(key, value);
			}
			else
			{
				this.Roots[key].Set(value);
			}
		}
	}

	public int Count => this.Roots.Count;

	public bool IsReadOnly => ((IDictionary)this.Roots).IsReadOnly;

	public ICollection<TKey> Keys => this.Roots.Keys;

	public ICollection<TValue> Values => this.Roots.Values.Select((NetRoot<TValue> root) => root.Get()).ToList();

	public NetRootDictionary()
	{
	}

	public NetRootDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values)
	{
		foreach (KeyValuePair<TKey, TValue> pair in values)
		{
			this.Add(pair.Key, pair.Value);
		}
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		this.Add(item.Key, item.Value);
	}

	public void Add(TKey key, TValue value)
	{
		NetRoot<TValue> root = new NetRoot<TValue>(value);
		root.Serializer = this.Serializer;
		this.Roots.Add(key, root);
	}

	public void Clear()
	{
		this.Roots.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (!this.Roots.TryGetValue(item.Key, out var root))
		{
			return false;
		}
		return (object)root == item.Value;
	}

	public bool ContainsKey(TKey key)
	{
		return this.Roots.ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException();
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (array.Length < this.Count - arrayIndex)
		{
			throw new ArgumentException();
		}
		foreach (KeyValuePair<TKey, TValue> pair in this)
		{
			array[arrayIndex++] = pair;
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this.Roots);
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new Enumerator(this.Roots);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this.Roots);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (this.Contains(item))
		{
			return this.Remove(item.Key);
		}
		return false;
	}

	public bool Remove(TKey key)
	{
		return this.Roots.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (this.Roots.TryGetValue(key, out var root))
		{
			value = root.Get();
			return true;
		}
		value = null;
		return false;
	}
}
