using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

public class ContentManager : IDisposable
{
	private const byte ContentCompressedLzx = 128;

	private const byte ContentCompressedLz4 = 64;

	private string _rootDirectory = string.Empty;

	private IServiceProvider serviceProvider;

	private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	private List<IDisposable> disposableAssets = new List<IDisposable>();

	private bool disposed;

	private static object ContentManagerLock;

	private static List<WeakReference> ContentManagers;

	internal static readonly ByteBufferPool ScratchBufferPool;

	private static readonly List<char> targetPlatformIdentifiers;

	/// <summary>
	/// Virtual property to allow a derived ContentManager to have it's assets reloaded
	/// </summary>
	protected virtual Dictionary<string, object> LoadedAssets => this.loadedAssets;

	public string RootDirectory
	{
		get
		{
			return this._rootDirectory;
		}
		set
		{
			this._rootDirectory = value;
		}
	}

	internal string RootDirectoryFullPath => Path.Combine(TitleContainer.Location, this.RootDirectory);

	public IServiceProvider ServiceProvider => this.serviceProvider;

	static ContentManager()
	{
		ContentManager.ContentManagerLock = new object();
		ContentManager.ContentManagers = new List<WeakReference>();
		ContentManager.ScratchBufferPool = new ByteBufferPool(1048576, Environment.ProcessorCount);
		ContentManager.targetPlatformIdentifiers = new List<char>
		{
			'w', 'x', 'm', 'i', 'a', 'd', 'X', 'W', 'n', 'M',
			'r', 'P', 'v', 'O', 'S', 'G', 'b', 'p', 'g', 'l'
		};
	}

	private static void AddContentManager(ContentManager contentManager)
	{
		lock (ContentManager.ContentManagerLock)
		{
			bool contains = false;
			for (int i = ContentManager.ContentManagers.Count - 1; i >= 0; i--)
			{
				WeakReference weakReference = ContentManager.ContentManagers[i];
				if (weakReference.Target == contentManager)
				{
					contains = true;
				}
				if (!weakReference.IsAlive)
				{
					ContentManager.ContentManagers.RemoveAt(i);
				}
			}
			if (!contains)
			{
				ContentManager.ContentManagers.Add(new WeakReference(contentManager));
			}
		}
	}

	private static void RemoveContentManager(ContentManager contentManager)
	{
		lock (ContentManager.ContentManagerLock)
		{
			for (int i = ContentManager.ContentManagers.Count - 1; i >= 0; i--)
			{
				WeakReference contentRef = ContentManager.ContentManagers[i];
				if (!contentRef.IsAlive || contentRef.Target == contentManager)
				{
					ContentManager.ContentManagers.RemoveAt(i);
				}
			}
		}
	}

	internal static void ReloadGraphicsContent()
	{
		lock (ContentManager.ContentManagerLock)
		{
			for (int i = ContentManager.ContentManagers.Count - 1; i >= 0; i--)
			{
				WeakReference contentRef = ContentManager.ContentManagers[i];
				if (contentRef.IsAlive)
				{
					((ContentManager)contentRef.Target)?.ReloadGraphicsAssets();
				}
				else
				{
					ContentManager.ContentManagers.RemoveAt(i);
				}
			}
		}
	}

	~ContentManager()
	{
		this.Dispose(disposing: false);
	}

	public ContentManager(IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
		{
			throw new ArgumentNullException("serviceProvider");
		}
		this.serviceProvider = serviceProvider;
		ContentManager.AddContentManager(this);
	}

	public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
	{
		if (serviceProvider == null)
		{
			throw new ArgumentNullException("serviceProvider");
		}
		if (rootDirectory == null)
		{
			throw new ArgumentNullException("rootDirectory");
		}
		this.RootDirectory = rootDirectory;
		this.serviceProvider = serviceProvider;
		ContentManager.AddContentManager(this);
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
		ContentManager.RemoveContentManager(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			if (disposing)
			{
				this.Unload();
			}
			this.disposed = true;
		}
	}

	public virtual T LoadLocalized<T>(string assetName)
	{
		string[] array = new string[2]
		{
			CultureInfo.CurrentCulture.Name,
			CultureInfo.CurrentCulture.TwoLetterISOLanguageName
		};
		foreach (string cultureName in array)
		{
			string localizedAssetName = assetName + "." + cultureName;
			try
			{
				return this.Load<T>(localizedAssetName);
			}
			catch (ContentLoadException)
			{
			}
		}
		return this.Load<T>(assetName);
	}

	public virtual T Load<T>(string assetName)
	{
		if (string.IsNullOrEmpty(assetName))
		{
			throw new ArgumentNullException("assetName");
		}
		if (this.disposed)
		{
			throw new ObjectDisposedException("ContentManager");
		}
		T result = default(T);
		string key = assetName.Replace('\\', '/');
		object asset = null;
		if (this.loadedAssets.TryGetValue(key, out asset) && asset is T)
		{
			return (T)asset;
		}
		result = this.ReadAsset<T>(assetName, null);
		this.loadedAssets[key] = result;
		return result;
	}

	protected virtual Stream OpenStream(string assetName)
	{
		try
		{
			string assetPath = Path.Combine(this.RootDirectory, assetName) + ".xnb";
			if (Path.IsPathRooted(assetPath))
			{
				return File.OpenRead(assetPath);
			}
			return TitleContainer.OpenStream(assetPath);
		}
		catch (FileNotFoundException innerException)
		{
			throw new ContentLoadException("The content file was not found.", innerException);
		}
		catch (DirectoryNotFoundException innerException2)
		{
			throw new ContentLoadException("The directory was not found.", innerException2);
		}
		catch (Exception innerException3)
		{
			throw new ContentLoadException("Opening stream error.", innerException3);
		}
	}

	protected T ReadAsset<T>(string assetName, Action<IDisposable> recordDisposableObject)
	{
		if (string.IsNullOrEmpty(assetName))
		{
			throw new ArgumentNullException("assetName");
		}
		if (this.disposed)
		{
			throw new ObjectDisposedException("ContentManager");
		}
		object result = null;
		Stream stream = this.OpenStream(assetName);
		using (BinaryReader xnbReader = new BinaryReader(stream))
		{
			using ContentReader reader = this.GetContentReaderFromXnb(assetName, stream, xnbReader, recordDisposableObject);
			result = reader.ReadAsset<T>();
			if (result is GraphicsResource)
			{
				((GraphicsResource)result).Name = assetName;
			}
		}
		if (result == null)
		{
			throw new ContentLoadException("Could not load " + assetName + " asset!");
		}
		return (T)result;
	}

	private ContentReader GetContentReaderFromXnb(string originalAssetName, Stream stream, BinaryReader xnbReader, Action<IDisposable> recordDisposableObject)
	{
		byte num = xnbReader.ReadByte();
		byte n = xnbReader.ReadByte();
		byte b = xnbReader.ReadByte();
		byte platform = xnbReader.ReadByte();
		if (num != 88 || n != 78 || b != 66 || !ContentManager.targetPlatformIdentifiers.Contains((char)platform))
		{
			throw new ContentLoadException("Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
		}
		byte version = xnbReader.ReadByte();
		byte num2 = xnbReader.ReadByte();
		bool compressedLzx = (num2 & 0x80) != 0;
		bool compressedLz4 = (num2 & 0x40) != 0;
		if (version != 5 && version != 4)
		{
			throw new ContentLoadException("Invalid XNB version");
		}
		int xnbLength = xnbReader.ReadInt32();
		Stream decompressedStream = null;
		if (compressedLzx || compressedLz4)
		{
			int decompressedSize = xnbReader.ReadInt32();
			if (compressedLzx)
			{
				int compressedSize = xnbLength - 14;
				decompressedStream = new LzxDecoderStream(stream, decompressedSize, compressedSize);
			}
			else if (compressedLz4)
			{
				decompressedStream = new Lz4DecoderStream(stream);
			}
		}
		else
		{
			decompressedStream = stream;
		}
		return new ContentReader(this, decompressedStream, originalAssetName, version, recordDisposableObject);
	}

	internal void RecordDisposable(IDisposable disposable)
	{
		if (!this.disposableAssets.Contains(disposable))
		{
			this.disposableAssets.Add(disposable);
		}
	}

	protected virtual void ReloadGraphicsAssets()
	{
		foreach (KeyValuePair<string, object> asset in this.LoadedAssets)
		{
			if (asset.Key == null)
			{
				this.ReloadAsset(asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()));
			}
			ReflectionHelpers.GetMethodInfo(typeof(ContentManager), "ReloadAsset").MakeGenericMethod(asset.Value.GetType()).Invoke(this, new object[2]
			{
				asset.Key,
				Convert.ChangeType(asset.Value, asset.Value.GetType())
			});
		}
	}

	protected virtual void ReloadAsset<T>(string originalAssetName, T currentAsset)
	{
		if (string.IsNullOrEmpty(originalAssetName))
		{
			throw new ArgumentNullException("assetName");
		}
		if (this.disposed)
		{
			throw new ObjectDisposedException("ContentManager");
		}
		Stream stream = this.OpenStream(originalAssetName);
		using BinaryReader xnbReader = new BinaryReader(stream);
		using ContentReader reader = this.GetContentReaderFromXnb(originalAssetName, stream, xnbReader, null);
		reader.ReadAsset(currentAsset);
	}

	public virtual void Unload()
	{
		foreach (IDisposable disposableAsset in this.disposableAssets)
		{
			disposableAsset?.Dispose();
		}
		this.disposableAssets.Clear();
		this.loadedAssets.Clear();
	}
}
