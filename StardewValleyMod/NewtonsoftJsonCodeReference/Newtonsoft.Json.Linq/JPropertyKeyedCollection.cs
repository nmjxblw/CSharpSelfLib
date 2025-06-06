using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

internal class JPropertyKeyedCollection : Collection<JToken>
{
	private static readonly IEqualityComparer<string> Comparer = StringComparer.Ordinal;

	private Dictionary<string, JToken>? _dictionary;

	public JToken this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (this._dictionary != null)
			{
				return this._dictionary[key];
			}
			throw new KeyNotFoundException();
		}
	}

	public ICollection<string> Keys
	{
		get
		{
			this.EnsureDictionary();
			return this._dictionary.Keys;
		}
	}

	public ICollection<JToken> Values
	{
		get
		{
			this.EnsureDictionary();
			return this._dictionary.Values;
		}
	}

	public JPropertyKeyedCollection()
		: base((IList<JToken>)new List<JToken>())
	{
	}

	private void AddKey(string key, JToken item)
	{
		this.EnsureDictionary();
		this._dictionary[key] = item;
	}

	protected void ChangeItemKey(JToken item, string newKey)
	{
		if (!this.ContainsItem(item))
		{
			throw new ArgumentException("The specified item does not exist in this KeyedCollection.");
		}
		string keyForItem = this.GetKeyForItem(item);
		if (!JPropertyKeyedCollection.Comparer.Equals(keyForItem, newKey))
		{
			if (newKey != null)
			{
				this.AddKey(newKey, item);
			}
			if (keyForItem != null)
			{
				this.RemoveKey(keyForItem);
			}
		}
	}

	protected override void ClearItems()
	{
		base.ClearItems();
		this._dictionary?.Clear();
	}

	public bool Contains(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (this._dictionary != null)
		{
			return this._dictionary.ContainsKey(key);
		}
		return false;
	}

	private bool ContainsItem(JToken item)
	{
		if (this._dictionary == null)
		{
			return false;
		}
		string keyForItem = this.GetKeyForItem(item);
		JToken value;
		return this._dictionary.TryGetValue(keyForItem, out value);
	}

	private void EnsureDictionary()
	{
		if (this._dictionary == null)
		{
			this._dictionary = new Dictionary<string, JToken>(JPropertyKeyedCollection.Comparer);
		}
	}

	private string GetKeyForItem(JToken item)
	{
		return ((JProperty)item).Name;
	}

	protected override void InsertItem(int index, JToken item)
	{
		this.AddKey(this.GetKeyForItem(item), item);
		base.InsertItem(index, item);
	}

	public bool Remove(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (this._dictionary != null)
		{
			if (this._dictionary.TryGetValue(key, out JToken value))
			{
				return base.Remove(value);
			}
			return false;
		}
		return false;
	}

	protected override void RemoveItem(int index)
	{
		string keyForItem = this.GetKeyForItem(base.Items[index]);
		this.RemoveKey(keyForItem);
		base.RemoveItem(index);
	}

	private void RemoveKey(string key)
	{
		this._dictionary?.Remove(key);
	}

	protected override void SetItem(int index, JToken item)
	{
		string keyForItem = this.GetKeyForItem(item);
		string keyForItem2 = this.GetKeyForItem(base.Items[index]);
		if (JPropertyKeyedCollection.Comparer.Equals(keyForItem2, keyForItem))
		{
			if (this._dictionary != null)
			{
				this._dictionary[keyForItem] = item;
			}
		}
		else
		{
			this.AddKey(keyForItem, item);
			if (keyForItem2 != null)
			{
				this.RemoveKey(keyForItem2);
			}
		}
		base.SetItem(index, item);
	}

	public bool TryGetValue(string key, [NotNullWhen(true)] out JToken? value)
	{
		if (this._dictionary == null)
		{
			value = null;
			return false;
		}
		return this._dictionary.TryGetValue(key, out value);
	}

	public int IndexOfReference(JToken t)
	{
		return ((List<JToken>)base.Items).IndexOfReference(t);
	}

	public bool Compare(JPropertyKeyedCollection other)
	{
		if (this == other)
		{
			return true;
		}
		Dictionary<string, JToken> dictionary = this._dictionary;
		Dictionary<string, JToken> dictionary2 = other._dictionary;
		if (dictionary == null && dictionary2 == null)
		{
			return true;
		}
		if (dictionary == null)
		{
			return dictionary2.Count == 0;
		}
		if (dictionary2 == null)
		{
			return dictionary.Count == 0;
		}
		if (dictionary.Count != dictionary2.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, JToken> item in dictionary)
		{
			if (!dictionary2.TryGetValue(item.Key, out var value))
			{
				return false;
			}
			JProperty jProperty = (JProperty)item.Value;
			JProperty jProperty2 = (JProperty)value;
			if (jProperty.Value == null)
			{
				return jProperty2.Value == null;
			}
			if (!jProperty.Value.DeepEquals(jProperty2.Value))
			{
				return false;
			}
		}
		return true;
	}
}
