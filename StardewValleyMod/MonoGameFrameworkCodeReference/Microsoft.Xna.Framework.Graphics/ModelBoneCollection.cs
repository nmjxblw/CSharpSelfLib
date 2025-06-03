using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a set of bones associated with a model.
/// </summary>
public class ModelBoneCollection : ReadOnlyCollection<ModelBone>
{
	/// <summary>
	/// Provides the ability to iterate through the bones in an ModelMeshCollection.
	/// </summary>
	public struct Enumerator : IEnumerator<ModelBone>, IEnumerator, IDisposable
	{
		private readonly ModelBoneCollection _collection;

		private int _position;

		/// <summary>
		/// Gets the current element in the ModelMeshCollection.
		/// </summary>
		public ModelBone Current => this._collection[this._position];

		object IEnumerator.Current => this._collection[this._position];

		internal Enumerator(ModelBoneCollection collection)
		{
			this._collection = collection;
			this._position = -1;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the ModelMeshCollection.
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

	/// <summary>
	/// Retrieves a ModelBone from the collection, given the name of the bone.
	/// </summary>
	/// <param name="boneName">The name of the bone to retrieve.</param>
	public ModelBone this[string boneName]
	{
		get
		{
			if (!this.TryGetValue(boneName, out var ret))
			{
				throw new KeyNotFoundException();
			}
			return ret;
		}
	}

	public ModelBoneCollection(IList<ModelBone> list)
		: base(list)
	{
	}

	/// <summary>
	/// Finds a bone with a given name if it exists in the collection.
	/// </summary>
	/// <param name="boneName">The name of the bone to find.</param>
	/// <param name="value">The bone named boneName, if found.</param>
	/// <returns>true if the bone was found</returns>
	public bool TryGetValue(string boneName, out ModelBone value)
	{
		if (string.IsNullOrEmpty(boneName))
		{
			throw new ArgumentNullException("boneName");
		}
		foreach (ModelBone bone in this)
		{
			if (bone.Name == boneName)
			{
				value = bone;
				return true;
			}
		}
		value = null;
		return false;
	}

	/// <summary>
	/// Returns a ModelMeshCollection.Enumerator that can iterate through a ModelMeshCollection.
	/// </summary>
	/// <returns></returns>
	public new Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
