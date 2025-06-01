using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// Defines a manager that constructs and keeps track of <see cref="T:Microsoft.Xna.Framework.Content.ContentTypeReader" /> objects.
/// </summary>
public sealed class ContentTypeReaderManager
{
	private static readonly object _locker;

	private static readonly Dictionary<Type, ContentTypeReader> _contentReadersCache;

	private Dictionary<Type, ContentTypeReader> _contentReaders;

	private static readonly string _assemblyName;

	private static readonly bool _isRunningOnNetCore;

	private static bool falseflag;

	private static Dictionary<string, Func<ContentTypeReader>> typeCreators;

	static ContentTypeReaderManager()
	{
		ContentTypeReaderManager._isRunningOnNetCore = typeof(object).Assembly.GetName().Name == "System.Private.CoreLib";
		ContentTypeReaderManager.falseflag = false;
		ContentTypeReaderManager.typeCreators = new Dictionary<string, Func<ContentTypeReader>>();
		ContentTypeReaderManager._locker = new object();
		ContentTypeReaderManager._contentReadersCache = new Dictionary<Type, ContentTypeReader>(255);
		ContentTypeReaderManager._assemblyName = ReflectionHelpers.GetAssembly(typeof(ContentTypeReaderManager)).FullName;
	}

	/// <summary>
	/// Creates a new instance of the <see cref="T:Microsoft.Xna.Framework.Content.ContentReader" /> class initialized for the specified type.
	/// </summary>
	/// <param name="targetType">The type the <see cref="T:Microsoft.Xna.Framework.Content.ContentReader" /> will handle.</param>
	/// <returns>
	/// The <see cref="T:Microsoft.Xna.Framework.Content.ContentReader" /> created by this method if a content reader of the specified type has been
	/// registered with this content manager; otherwise, null.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">If the <paramref name="targetType" /> parameter is null.</exception>
	public ContentTypeReader GetTypeReader(Type targetType)
	{
		if (targetType.IsArray && targetType.GetArrayRank() > 1)
		{
			targetType = typeof(Array);
		}
		if (this._contentReaders.TryGetValue(targetType, out var reader))
		{
			return reader;
		}
		return null;
	}

	internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
	{
		if (ContentTypeReaderManager.falseflag)
		{
			new ByteReader();
			new SByteReader();
			new DateTimeReader();
			new DecimalReader();
			new BoundingSphereReader();
			new BoundingFrustumReader();
			new RayReader();
			new ListReader<char>();
			new ListReader<Rectangle>();
			new ArrayReader<Rectangle>();
			new ListReader<Vector3>();
			new ListReader<StringReader>();
			new ListReader<int>();
			new SpriteFontReader();
			new Texture2DReader();
			new CharReader();
			new RectangleReader();
			new StringReader();
			new Vector2Reader();
			new Vector3Reader();
			new Vector4Reader();
			new CurveReader();
			new IndexBufferReader();
			new BoundingBoxReader();
			new MatrixReader();
			new BasicEffectReader();
			new VertexBufferReader();
			new AlphaTestEffectReader();
			new EnumReader<SpriteEffects>();
			new ArrayReader<float>();
			new ArrayReader<Vector2>();
			new ListReader<Vector2>();
			new ArrayReader<Matrix>();
			new EnumReader<Blend>();
			new NullableReader<Rectangle>();
			new EffectMaterialReader();
			new ExternalReferenceReader();
			new SoundEffectReader();
			new SongReader();
			new ModelReader();
			new Int32Reader();
			new EffectReader();
			new SingleReader();
		}
		int numberOfReaders = reader.Read7BitEncodedInt();
		ContentTypeReader[] contentReaders = new ContentTypeReader[numberOfReaders];
		BitArray needsInitialize = new BitArray(numberOfReaders);
		this._contentReaders = new Dictionary<Type, ContentTypeReader>(numberOfReaders);
		lock (ContentTypeReaderManager._locker)
		{
			for (int i = 0; i < numberOfReaders; i++)
			{
				string originalReaderTypeString = reader.ReadString();
				if (ContentTypeReaderManager.typeCreators.TryGetValue(originalReaderTypeString, out var readerFunc))
				{
					contentReaders[i] = readerFunc();
					needsInitialize[i] = true;
				}
				else
				{
					string readerTypeString = originalReaderTypeString;
					readerTypeString = ContentTypeReaderManager.PrepareType(readerTypeString);
					Type l_readerType = Type.GetType(readerTypeString);
					if (!(l_readerType != null))
					{
						throw new ContentLoadException("Could not find ContentTypeReader Type. Please ensure the name of the Assembly that contains the Type matches the assembly in the full type name: " + originalReaderTypeString + " (" + readerTypeString + ")");
					}
					if (!ContentTypeReaderManager._contentReadersCache.TryGetValue(l_readerType, out var typeReader))
					{
						try
						{
							typeReader = l_readerType.GetDefaultConstructor().Invoke(null) as ContentTypeReader;
						}
						catch (TargetInvocationException innerException)
						{
							throw new InvalidOperationException("Failed to get default constructor for ContentTypeReader. To work around, add a creation function to ContentTypeReaderManager.AddTypeCreator() with the following failed type string: " + originalReaderTypeString, innerException);
						}
						needsInitialize[i] = true;
						ContentTypeReaderManager._contentReadersCache.Add(l_readerType, typeReader);
					}
					contentReaders[i] = typeReader;
				}
				Type targetType = contentReaders[i].TargetType;
				if (targetType != null && !this._contentReaders.ContainsKey(targetType))
				{
					this._contentReaders.Add(targetType, contentReaders[i]);
				}
				reader.ReadInt32();
			}
			for (int j = 0; j < contentReaders.Length; j++)
			{
				if (needsInitialize.Get(j))
				{
					contentReaders[j].Initialize(this);
				}
			}
			return contentReaders;
		}
	}

	/// <summary>
	/// Removes the Version, Culture, and PublicKeyToken from a fully-qualified type name string.
	/// </summary>
	/// <remarks>
	/// Supports multiple generic types (e.g. Dictionary&lt;TKey,TValue&gt;) and nested generic types (e.g. List&lt;List&lt;int&gt;&gt;).
	/// </remarks>
	/// <param name="type">A string containing the fully-qualified type name to prepare.</param>
	/// <returns>A new string with the Version, Culture and PublicKeyToken removed.</returns>
	public static string PrepareType(string type)
	{
		int count = type.Split(new string[1] { "[[" }, StringSplitOptions.None).Length - 1;
		string preparedType = type;
		for (int i = 0; i < count; i++)
		{
			preparedType = Regex.Replace(preparedType, "\\[(.+?), Version=.+?\\]", "[$1]");
		}
		if (preparedType.Contains("PublicKeyToken"))
		{
			preparedType = Regex.Replace(preparedType, "(.+?), Version=.+?$", "$1");
		}
		preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Graphics", $", {ContentTypeReaderManager._assemblyName}");
		preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Video", $", {ContentTypeReaderManager._assemblyName}");
		preparedType = preparedType.Replace(", Microsoft.Xna.Framework", $", {ContentTypeReaderManager._assemblyName}");
		if (ContentTypeReaderManager._isRunningOnNetCore)
		{
			return preparedType.Replace("mscorlib", "System.Private.CoreLib");
		}
		return preparedType.Replace("System.Private.CoreLib", "mscorlib");
	}

	/// <summary>
	/// Registers a function to create a <see cref="T:Microsoft.Xna.Framework.Content.ContentTypeReader" /> instance used to read an object of the
	/// type specified.
	/// </summary>
	/// <param name="typeString">A string containing the fully-qualified type name of the object type.</param>
	/// <param name="createFunction">The function responsible for creating an instance of the <see cref="T:Microsoft.Xna.Framework.Content.ContentTypeReader" /> class.</param>
	/// <exception cref="T:System.ArgumentNullException">If the <paramref name="typeString" /> parameter is null or an empty string.</exception>
	public static void AddTypeCreator(string typeString, Func<ContentTypeReader> createFunction)
	{
		if (!ContentTypeReaderManager.typeCreators.ContainsKey(typeString))
		{
			ContentTypeReaderManager.typeCreators.Add(typeString, createFunction);
		}
	}

	/// <summary>
	/// Clears all content type creators that were registered with <see cref="M:Microsoft.Xna.Framework.Content.ContentTypeReaderManager.AddTypeCreator(System.String,System.Func{Microsoft.Xna.Framework.Content.ContentTypeReader})" />
	/// </summary>
	public static void ClearTypeCreators()
	{
		ContentTypeReaderManager.typeCreators.Clear();
	}
}
