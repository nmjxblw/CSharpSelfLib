using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

public sealed class ContentReader : BinaryReader
{
	private ContentManager contentManager;

	private Action<IDisposable> recordDisposableObject;

	private ContentTypeReaderManager typeReaderManager;

	private string assetName;

	private List<KeyValuePair<int, Action<object>>> sharedResourceFixups;

	private ContentTypeReader[] typeReaders;

	internal int version;

	internal int sharedResourceCount;

	internal ContentTypeReader[] TypeReaders => this.typeReaders;

	public ContentManager ContentManager => this.contentManager;

	public string AssetName => this.assetName;

	internal ContentReader(ContentManager manager, Stream stream, string assetName, int version, Action<IDisposable> recordDisposableObject)
		: base(stream)
	{
		this.recordDisposableObject = recordDisposableObject;
		this.contentManager = manager;
		this.assetName = assetName;
		this.version = version;
	}

	internal object ReadAsset<T>()
	{
		this.InitializeTypeReaders();
		object result = this.ReadObject<T>();
		this.ReadSharedResources();
		return result;
	}

	internal object ReadAsset<T>(T existingInstance)
	{
		this.InitializeTypeReaders();
		object result = this.ReadObject(existingInstance);
		this.ReadSharedResources();
		return result;
	}

	internal void InitializeTypeReaders()
	{
		this.typeReaderManager = new ContentTypeReaderManager();
		this.typeReaders = this.typeReaderManager.LoadAssetReaders(this);
		this.sharedResourceCount = this.Read7BitEncodedInt();
		this.sharedResourceFixups = new List<KeyValuePair<int, Action<object>>>();
	}

	internal void ReadSharedResources()
	{
		if (this.sharedResourceCount <= 0)
		{
			return;
		}
		object[] sharedResources = new object[this.sharedResourceCount];
		for (int i = 0; i < this.sharedResourceCount; i++)
		{
			sharedResources[i] = this.InnerReadObject<object>(null);
		}
		foreach (KeyValuePair<int, Action<object>> fixup in this.sharedResourceFixups)
		{
			fixup.Value(sharedResources[fixup.Key]);
		}
	}

	public T ReadExternalReference<T>()
	{
		string externalReference = this.ReadString();
		if (!string.IsNullOrEmpty(externalReference))
		{
			return this.contentManager.Load<T>(FileHelpers.ResolveRelativePath(this.assetName, externalReference));
		}
		return default(T);
	}

	public Matrix ReadMatrix()
	{
		return new Matrix
		{
			M11 = this.ReadSingle(),
			M12 = this.ReadSingle(),
			M13 = this.ReadSingle(),
			M14 = this.ReadSingle(),
			M21 = this.ReadSingle(),
			M22 = this.ReadSingle(),
			M23 = this.ReadSingle(),
			M24 = this.ReadSingle(),
			M31 = this.ReadSingle(),
			M32 = this.ReadSingle(),
			M33 = this.ReadSingle(),
			M34 = this.ReadSingle(),
			M41 = this.ReadSingle(),
			M42 = this.ReadSingle(),
			M43 = this.ReadSingle(),
			M44 = this.ReadSingle()
		};
	}

	private void RecordDisposable<T>(T result)
	{
		if (result is IDisposable disposable)
		{
			if (this.recordDisposableObject != null)
			{
				this.recordDisposableObject(disposable);
			}
			else
			{
				this.contentManager.RecordDisposable(disposable);
			}
		}
	}

	public T ReadObject<T>()
	{
		return this.InnerReadObject(default(T));
	}

	public T ReadObject<T>(ContentTypeReader typeReader)
	{
		T result = (T)typeReader.Read(this, default(T));
		this.RecordDisposable(result);
		return result;
	}

	public T ReadObject<T>(T existingInstance)
	{
		return this.InnerReadObject(existingInstance);
	}

	private T InnerReadObject<T>(T existingInstance)
	{
		int typeReaderIndex = this.Read7BitEncodedInt();
		if (typeReaderIndex == 0)
		{
			return existingInstance;
		}
		if (typeReaderIndex > this.typeReaders.Length)
		{
			throw new ContentLoadException("Incorrect type reader index found!");
		}
		T result = (T)this.typeReaders[typeReaderIndex - 1].Read(this, existingInstance);
		this.RecordDisposable(result);
		return result;
	}

	public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
	{
		if (!ReflectionHelpers.IsValueType(typeReader.TargetType))
		{
			return this.ReadObject(existingInstance);
		}
		T result = (T)typeReader.Read(this, existingInstance);
		this.RecordDisposable(result);
		return result;
	}

	public Quaternion ReadQuaternion()
	{
		return new Quaternion
		{
			X = this.ReadSingle(),
			Y = this.ReadSingle(),
			Z = this.ReadSingle(),
			W = this.ReadSingle()
		};
	}

	public T ReadRawObject<T>()
	{
		return this.ReadRawObject(default(T));
	}

	public T ReadRawObject<T>(ContentTypeReader typeReader)
	{
		return this.ReadRawObject(typeReader, default(T));
	}

	public T ReadRawObject<T>(T existingInstance)
	{
		Type objectType = typeof(T);
		ContentTypeReader[] array = this.typeReaders;
		foreach (ContentTypeReader typeReader in array)
		{
			if (typeReader.TargetType == objectType)
			{
				return this.ReadRawObject(typeReader, existingInstance);
			}
		}
		throw new NotSupportedException();
	}

	public T ReadRawObject<T>(ContentTypeReader typeReader, T existingInstance)
	{
		return (T)typeReader.Read(this, existingInstance);
	}

	public void ReadSharedResource<T>(Action<T> fixup)
	{
		int index = this.Read7BitEncodedInt();
		if (index <= 0)
		{
			return;
		}
		this.sharedResourceFixups.Add(new KeyValuePair<int, Action<object>>(index - 1, delegate(object v)
		{
			if (!(v is T))
			{
				throw new ContentLoadException($"Error loading shared resource. Expected type {typeof(T).Name}, received type {v.GetType().Name}");
			}
			fixup((T)v);
		}));
	}

	public Vector2 ReadVector2()
	{
		return new Vector2
		{
			X = this.ReadSingle(),
			Y = this.ReadSingle()
		};
	}

	public Vector3 ReadVector3()
	{
		return new Vector3
		{
			X = this.ReadSingle(),
			Y = this.ReadSingle(),
			Z = this.ReadSingle()
		};
	}

	public Vector4 ReadVector4()
	{
		return new Vector4
		{
			X = this.ReadSingle(),
			Y = this.ReadSingle(),
			Z = this.ReadSingle(),
			W = this.ReadSingle()
		};
	}

	public Color ReadColor()
	{
		return new Color
		{
			R = this.ReadByte(),
			G = this.ReadByte(),
			B = this.ReadByte(),
			A = this.ReadByte()
		};
	}

	internal new int Read7BitEncodedInt()
	{
		return base.Read7BitEncodedInt();
	}

	internal BoundingSphere ReadBoundingSphere()
	{
		Vector3 center = this.ReadVector3();
		float radius = this.ReadSingle();
		return new BoundingSphere(center, radius);
	}
}
