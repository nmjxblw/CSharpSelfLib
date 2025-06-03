using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// The collection of the <see cref="T:Microsoft.Xna.Framework.CurveKey" /> elements and a part of the <see cref="T:Microsoft.Xna.Framework.Curve" /> class.
/// </summary>
[DataContract]
public class CurveKeyCollection : ICollection<CurveKey>, IEnumerable<CurveKey>, IEnumerable
{
	private readonly List<CurveKey> _keys;

	/// <summary>
	/// Indexer.
	/// </summary>
	/// <param name="index">The index of key in this collection.</param>
	/// <returns><see cref="T:Microsoft.Xna.Framework.CurveKey" /> at <paramref name="index" /> position.</returns>
	[DataMember(Name = "Items")]
	public CurveKey this[int index]
	{
		get
		{
			return this._keys[index];
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (index >= this._keys.Count)
			{
				throw new IndexOutOfRangeException();
			}
			if (this._keys[index].Position == value.Position)
			{
				this._keys[index] = value;
				return;
			}
			this._keys.RemoveAt(index);
			this._keys.Add(value);
		}
	}

	/// <summary>
	/// Returns the count of keys in this collection.
	/// </summary>
	[DataMember]
	public int Count => this._keys.Count;

	/// <summary>
	/// Returns false because it is not a read-only collection.
	/// </summary>
	[DataMember]
	public bool IsReadOnly => false;

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.CurveKeyCollection" /> class.
	/// </summary>
	public CurveKeyCollection()
	{
		this._keys = new List<CurveKey>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._keys.GetEnumerator();
	}

	/// <summary>
	/// Adds a key to this collection.
	/// </summary>
	/// <param name="item">New key for the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">Throws if <paramref name="item" /> is null.</exception>
	/// <remarks>The new key would be added respectively to a position of that key and the position of other keys.</remarks>
	public void Add(CurveKey item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (this._keys.Count == 0)
		{
			this._keys.Add(item);
			return;
		}
		for (int i = 0; i < this._keys.Count; i++)
		{
			if (item.Position < this._keys[i].Position)
			{
				this._keys.Insert(i, item);
				return;
			}
		}
		this._keys.Add(item);
	}

	/// <summary>
	/// Removes all keys from this collection.
	/// </summary>
	public void Clear()
	{
		this._keys.Clear();
	}

	/// <summary>
	/// Creates a copy of this collection.
	/// </summary>
	/// <returns>A copy of this collection.</returns>
	public CurveKeyCollection Clone()
	{
		CurveKeyCollection ckc = new CurveKeyCollection();
		foreach (CurveKey key in this._keys)
		{
			ckc.Add(key);
		}
		return ckc;
	}

	/// <summary>
	/// Determines whether this collection contains a specific key.
	/// </summary>
	/// <param name="item">The key to locate in this collection.</param>
	/// <returns><c>true</c> if the key is found; <c>false</c> otherwise.</returns>
	public bool Contains(CurveKey item)
	{
		return this._keys.Contains(item);
	}

	/// <summary>
	/// Copies the keys of this collection to an array, starting at the array index provided.
	/// </summary>
	/// <param name="array">Destination array where elements will be copied.</param>
	/// <param name="arrayIndex">The zero-based index in the array to start copying from.</param>
	public void CopyTo(CurveKey[] array, int arrayIndex)
	{
		this._keys.CopyTo(array, arrayIndex);
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator for the <see cref="T:Microsoft.Xna.Framework.CurveKeyCollection" />.</returns>
	public IEnumerator<CurveKey> GetEnumerator()
	{
		return this._keys.GetEnumerator();
	}

	/// <summary>
	/// Finds element in the collection and returns its index.
	/// </summary>
	/// <param name="item">Element for the search.</param>
	/// <returns>Index of the element; or -1 if item is not found.</returns>
	public int IndexOf(CurveKey item)
	{
		return this._keys.IndexOf(item);
	}

	/// <summary>
	/// Removes element at the specified index.
	/// </summary>
	/// <param name="index">The index which element will be removed.</param>
	public void RemoveAt(int index)
	{
		this._keys.RemoveAt(index);
	}

	/// <summary>
	/// Removes specific element.
	/// </summary>
	/// <param name="item">The element</param>
	/// <returns><c>true</c> if item is successfully removed; <c>false</c> otherwise. This method also returns <c>false</c> if item was not found.</returns>
	public bool Remove(CurveKey item)
	{
		return this._keys.Remove(item);
	}
}
