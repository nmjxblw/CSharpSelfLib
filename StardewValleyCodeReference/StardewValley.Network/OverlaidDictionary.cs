using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network;

/// <summary>A hybrid synchronized/unsynchronized dictionary where values either come from a synchronized NetDictionary, or from a locally maintained overlay dictionary.</summary>
public class OverlaidDictionary : IEnumerable<SerializableDictionary<Vector2, Object>>, IEnumerable
{
	private NetVector2Dictionary<Object, NetRef<Object>> baseDict;

	private OverlayDictionary<Vector2, Object> overlayDict;

	private Dictionary<Vector2, Object> compositeDict;

	private bool _locked;

	private Dictionary<Vector2, Object> _changes = new Dictionary<Vector2, Object>();

	/// <summary>The number of key/value pairs in the dictionary.</summary>
	public int Length => this.compositeDict.Count;

	public Object this[Vector2 key]
	{
		get
		{
			if (this.overlayDict.TryGetValue(key, out var overlaid))
			{
				return overlaid;
			}
			if (this._locked && this._changes.TryGetValue(key, out var o))
			{
				if (o == null)
				{
					throw new KeyNotFoundException();
				}
				return o;
			}
			return this.baseDict[key];
		}
		set
		{
			if (this._locked)
			{
				this._changes[key] = value;
			}
			else
			{
				this.baseDict[key] = value;
			}
		}
	}

	public Dictionary<Vector2, Object>.KeyCollection Keys => this.compositeDict.Keys;

	public Dictionary<Vector2, Object>.ValueCollection Values => this.compositeDict.Values;

	public IEnumerable<KeyValuePair<Vector2, Object>> Pairs => this.compositeDict;

	public void OnValueAdded(Vector2 key, Object value)
	{
		if (this.overlayDict.TryGetValue(key, out var o))
		{
			this.compositeDict[key] = o;
		}
		else if (this.baseDict.TryGetValue(key, out o))
		{
			this.compositeDict[key] = o;
		}
	}

	public void OnValueRemoved(Vector2 key, Object value)
	{
		if (this.overlayDict.TryGetValue(key, out var o))
		{
			this.compositeDict[key] = o;
		}
		else if (this.baseDict.TryGetValue(key, out o))
		{
			this.compositeDict[key] = o;
		}
		else
		{
			this.compositeDict.Remove(key);
		}
	}

	public void SetEqualityComparer(IEqualityComparer<Vector2> comparer, ref NetVector2Dictionary<Object, NetRef<Object>> base_dict, ref OverlayDictionary<Vector2, Object> overlay_dict)
	{
		this.baseDict.SetEqualityComparer(comparer);
		this.overlayDict.onValueAdded -= OnValueAdded;
		this.overlayDict.onValueRemoved -= OnValueRemoved;
		this.overlayDict = new OverlayDictionary<Vector2, Object>(this.overlayDict, comparer);
		this.compositeDict = new Dictionary<Vector2, Object>(this.compositeDict, comparer);
		this.overlayDict.onValueAdded += OnValueAdded;
		this.overlayDict.onValueRemoved += OnValueRemoved;
		this.overlayDict.onValueAdded += OnValueAdded;
		this.overlayDict.onValueRemoved += OnValueRemoved;
		base_dict = this.baseDict;
		overlay_dict = this.overlayDict;
	}

	public OverlaidDictionary(NetVector2Dictionary<Object, NetRef<Object>> baseDict, OverlayDictionary<Vector2, Object> overlayDict)
	{
		this.baseDict = baseDict;
		this.overlayDict = overlayDict;
		this.compositeDict = new Dictionary<Vector2, Object>();
		foreach (KeyValuePair<Vector2, Object> pair in overlayDict)
		{
			this.OnValueAdded(pair.Key, pair.Value);
		}
		foreach (KeyValuePair<Vector2, Object> pair2 in baseDict.Pairs)
		{
			this.OnValueAdded(pair2.Key, pair2.Value);
		}
		baseDict.OnValueAdded += OnValueAdded;
		baseDict.OnConflictResolve += delegate(Vector2 key, NetRef<Object> rejected, NetRef<Object> accepted)
		{
			this.OnValueRemoved(key, rejected.Value);
			this.OnValueAdded(key, accepted.Value);
		};
		baseDict.OnValueRemoved += OnValueRemoved;
	}

	public bool Any()
	{
		return this.compositeDict.Count > 0;
	}

	public int Count()
	{
		return this.compositeDict.Count;
	}

	/// <summary>Freeze the object list, so changes will be queued until <see cref="M:StardewValley.Network.OverlaidDictionary.Unlock" /> is called.</summary>
	public void Lock()
	{
		this._locked = true;
	}

	/// <summary>Remove the freeze added by <see cref="M:StardewValley.Network.OverlaidDictionary.Lock" /> and apply all changes that were queued while it was locked.</summary>
	public void Unlock()
	{
		if (!this._locked)
		{
			return;
		}
		this._locked = false;
		if (this._changes.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<Vector2, Object> kvp in this._changes)
		{
			if (kvp.Value != null)
			{
				this.baseDict[kvp.Key] = kvp.Value;
			}
			else
			{
				this.baseDict.Remove(kvp.Key);
			}
		}
		this._changes.Clear();
	}

	/// <summary>Add an object to the dictionary.</summary>
	/// <param name="key">The tile position.</param>
	/// <param name="value">The object instance.</param>
	/// <exception cref="T:System.ArgumentException">The key is already present in the dictionary.</exception>
	public void Add(Vector2 key, Object value)
	{
		if (this._locked)
		{
			if (this._changes.TryGetValue(key, out var existingValue))
			{
				if (existingValue != null)
				{
					throw new ArgumentException();
				}
				this._changes[key] = value;
			}
			else
			{
				if (this.baseDict.ContainsKey(key))
				{
					throw new ArgumentException();
				}
				this._changes[key] = value;
			}
		}
		else
		{
			this.baseDict.Add(key, value);
		}
	}

	/// <summary>Add an object to the dictionary if the key isn't already present.</summary>
	/// <param name="key">The tile position.</param>
	/// <param name="value">The object instance.</param>
	/// <returns>Returns whether the object was successfully added.</returns>
	public bool TryAdd(Vector2 key, Object value)
	{
		if (this.ContainsKey(key))
		{
			return false;
		}
		this.Add(key, value);
		return true;
	}

	public void Clear()
	{
		if (this._locked)
		{
			throw new NotImplementedException();
		}
		this.baseDict.Clear();
		this.overlayDict.Clear();
		this.compositeDict.Clear();
	}

	public bool ContainsKey(Vector2 key)
	{
		if (this._locked && this._changes.TryGetValue(key, out var value))
		{
			return value != null;
		}
		return this.compositeDict.ContainsKey(key);
	}

	public bool Remove(Vector2 key)
	{
		if (this.overlayDict.Remove(key))
		{
			return true;
		}
		if (this._locked)
		{
			if (this._changes.TryGetValue(key, out var value))
			{
				this._changes[key] = null;
				return value != null;
			}
			if (this.baseDict.ContainsKey(key))
			{
				this._changes[key] = null;
				return true;
			}
			return false;
		}
		return this.baseDict.Remove(key);
	}

	/// <summary>Get the object on a given tile if it exists.</summary>
	/// <param name="key">The tile position to check.</param>
	/// <param name="value">The object that was found, if applicable.</param>
	/// <returns>Returns whether the object was found.</returns>
	public bool TryGetValue(Vector2 key, out Object value)
	{
		return this.compositeDict.TryGetValue(key, out value);
	}

	/// <summary>Get the object on a given tile if it exists, else a default value.</summary>
	/// <param name="key">The tile position to check.</param>
	/// <param name="defaultValue">The value to return if the element isn't found.</param>
	public Object GetValueOrDefault(Vector2 key, Object defaultValue = null)
	{
		return this.compositeDict.GetValueOrDefault(key, defaultValue);
	}

	public IEnumerator<SerializableDictionary<Vector2, Object>> GetEnumerator()
	{
		return this.baseDict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.baseDict.GetEnumerator();
	}

	public void Add(SerializableDictionary<Vector2, Object> dict)
	{
		foreach (KeyValuePair<Vector2, Object> pair in dict)
		{
			if (pair.Value != null)
			{
				this.Add(pair.Key, pair.Value);
			}
		}
	}
}
