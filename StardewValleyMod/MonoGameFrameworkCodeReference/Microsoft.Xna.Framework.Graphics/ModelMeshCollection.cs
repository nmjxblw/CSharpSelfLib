using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a collection of ModelMesh objects.
/// </summary>
public sealed class ModelMeshCollection : ReadOnlyCollection<ModelMesh>
{
	/// <summary>
	/// Provides the ability to iterate through the bones in an ModelMeshCollection.
	/// </summary>
	public struct Enumerator : IEnumerator<ModelMesh>, IEnumerator, IDisposable
	{
		private readonly ModelMeshCollection _collection;

		private int _position;

		/// <summary>
		/// Gets the current element in the ModelMeshCollection.
		/// </summary>
		public ModelMesh Current => this._collection[this._position];

		object IEnumerator.Current => this._collection[this._position];

		internal Enumerator(ModelMeshCollection collection)
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
	/// Retrieves a ModelMesh from the collection, given the name of the mesh.
	/// </summary>
	/// <param name="meshName">The name of the mesh to retrieve.</param>
	public ModelMesh this[string meshName]
	{
		get
		{
			if (!this.TryGetValue(meshName, out var ret))
			{
				throw new KeyNotFoundException();
			}
			return ret;
		}
	}

	internal ModelMeshCollection(IList<ModelMesh> list)
		: base(list)
	{
	}

	/// <summary>
	/// Finds a mesh with a given name if it exists in the collection.
	/// </summary>
	/// <param name="meshName">The name of the mesh to find.</param>
	/// <param name="value">The mesh named meshName, if found.</param>
	/// <returns>true if a mesh was found</returns>
	public bool TryGetValue(string meshName, out ModelMesh value)
	{
		if (string.IsNullOrEmpty(meshName))
		{
			throw new ArgumentNullException("meshName");
		}
		foreach (ModelMesh mesh in this)
		{
			if (string.Compare(mesh.Name, meshName, StringComparison.Ordinal) == 0)
			{
				value = mesh;
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
