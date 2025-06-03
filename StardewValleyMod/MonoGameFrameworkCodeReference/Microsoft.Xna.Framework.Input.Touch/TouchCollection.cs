using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Touch;

/// <summary>
/// Provides state information for a touch screen enabled device.
/// </summary>
public struct TouchCollection : IList<TouchLocation>, ICollection<TouchLocation>, IEnumerable<TouchLocation>, IEnumerable
{
	/// <summary>
	/// Provides the ability to iterate through the TouchLocations in an TouchCollection.
	/// </summary>
	public struct Enumerator : IEnumerator<TouchLocation>, IEnumerator, IDisposable
	{
		private readonly TouchCollection _collection;

		private int _position;

		/// <summary>
		/// Gets the current element in the TouchCollection.
		/// </summary>
		public TouchLocation Current => this._collection[this._position];

		object IEnumerator.Current => this._collection[this._position];

		internal Enumerator(TouchCollection collection)
		{
			this._collection = collection;
			this._position = -1;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the TouchCollection.
		/// </summary>
		public bool MoveNext()
		{
			this._position++;
			return this._position < this._collection.Count;
		}

		/// <summary>
		/// Immediately releases the unmanaged resources used by this object.
		/// </summary>
		public void Dispose()
		{
		}

		public void Reset()
		{
			this._position = -1;
		}
	}

	private readonly TouchLocation[] _collection;

	private static readonly TouchLocation[] EmptyLocationArray = new TouchLocation[0];

	internal static readonly TouchCollection Empty = new TouchCollection(TouchCollection.EmptyLocationArray);

	private TouchLocation[] Collection => this._collection ?? TouchCollection.EmptyLocationArray;

	/// <summary>
	/// States if a touch screen is available.
	/// </summary>
	public bool IsConnected => TouchPanel.GetCapabilities().IsConnected;

	/// <summary>
	/// States if touch collection is read only.
	/// </summary>
	public bool IsReadOnly => true;

	/// <summary>
	/// Gets or sets the item at the specified index of the collection.
	/// </summary>
	/// <param name="index">Position of the item.</param>
	/// <returns><see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /></returns>
	public TouchLocation this[int index]
	{
		get
		{
			return this.Collection[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>
	/// Returns the number of <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> items that exist in the collection.
	/// </summary>
	public int Count => this.Collection.Length;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchCollection" /> with a pre-determined set of touch locations.
	/// </summary>
	/// <param name="touches">Array of <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> items to initialize with.</param>
	public TouchCollection(TouchLocation[] touches)
	{
		if (touches == null)
		{
			throw new ArgumentNullException("touches");
		}
		this._collection = touches;
	}

	/// <summary>
	/// Returns <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> specified by ID.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="touchLocation"></param>
	/// <returns></returns>
	public bool FindById(int id, out TouchLocation touchLocation)
	{
		for (int i = 0; i < this.Collection.Length; i++)
		{
			TouchLocation location = this.Collection[i];
			if (location.Id == id)
			{
				touchLocation = location;
				return true;
			}
		}
		touchLocation = default(TouchLocation);
		return false;
	}

	/// <summary>
	/// Returns the index of the first occurrence of specified <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item in the collection.
	/// </summary>
	/// <param name="item"><see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> to query.</param>
	/// <returns></returns>
	public int IndexOf(TouchLocation item)
	{
		for (int i = 0; i < this.Collection.Length; i++)
		{
			if (item == this.Collection[i])
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Inserts a <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item into the indicated position.
	/// </summary>
	/// <param name="index">The position to insert into.</param>
	/// <param name="item">The <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item to insert.</param>
	public void Insert(int index, TouchLocation item)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Removes the <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item at specified index.
	/// </summary>
	/// <param name="index">Index of the item that will be removed from collection.</param>
	public void RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Adds a <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> to the collection.
	/// </summary>
	/// <param name="item">The <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item to be added. </param>
	public void Add(TouchLocation item)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Clears all the items in collection.
	/// </summary>
	public void Clear()
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Returns true if specified <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item exists in the collection, false otherwise./&gt;
	/// </summary>
	/// <param name="item">The <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item to query for.</param>
	/// <returns>Returns true if queried item is found, false otherwise.</returns>
	public bool Contains(TouchLocation item)
	{
		for (int i = 0; i < this.Collection.Length; i++)
		{
			if (item == this.Collection[i])
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Copies the <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" />collection to specified array starting from the given index.
	/// </summary>
	/// <param name="array">The array to copy <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> items.</param>
	/// <param name="arrayIndex">The starting index of the copy operation.</param>
	public void CopyTo(TouchLocation[] array, int arrayIndex)
	{
		this.Collection.CopyTo(array, arrayIndex);
	}

	/// <summary>
	/// Removes the specified <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item from the collection.
	/// </summary>
	/// <param name="item">The <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> item to remove.</param>
	/// <returns></returns>
	public bool Remove(TouchLocation item)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// Returns an enumerator for the <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchCollection" />.
	/// </summary>
	/// <returns>Enumerable list of <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> objects.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>
	/// Returns an enumerator for the <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchCollection" />.
	/// </summary>
	/// <returns>Enumerable list of <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchLocation" /> objects.</returns>
	IEnumerator<TouchLocation> IEnumerable<TouchLocation>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>
	/// Returns an enumerator for the <see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchCollection" />.
	/// </summary>
	/// <returns>Enumerable list of objects.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}
}
