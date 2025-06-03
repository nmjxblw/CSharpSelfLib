using System;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Provides functionality for opening a stream in the title storage area.
/// </summary>
public static class TitleContainer
{
	internal static string Location { get; private set; }

	private static void PlatformInit()
	{
		if (CurrentPlatform.OS == OS.MacOSX)
		{
			TitleContainer.Location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
		}
		if (!Directory.Exists(TitleContainer.Location))
		{
			TitleContainer.Location = AppDomain.CurrentDomain.BaseDirectory;
		}
	}

	static TitleContainer()
	{
		TitleContainer.Location = string.Empty;
		TitleContainer.PlatformInit();
	}

	/// <summary>
	/// Returns an open stream to an existing file in the title storage area.
	/// </summary>
	/// <param name="name">The filepath relative to the title storage area.</param>
	/// <returns>A open stream or null if the file is not found.</returns>
	public static Stream OpenStream(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (Path.IsPathRooted(name))
		{
			throw new ArgumentException("Invalid filename. TitleContainer.OpenStream requires a relative path.", name);
		}
		string safeName = TitleContainer.NormalizeRelativePath(name);
		try
		{
			Stream stream = TitleContainer.PlatformOpenStream(safeName);
			if (stream == null)
			{
				throw TitleContainer.FileNotFoundException(name, null);
			}
			return stream;
		}
		catch (FileNotFoundException)
		{
			throw;
		}
		catch (Exception innerException)
		{
			throw new FileNotFoundException(name, innerException);
		}
	}

	private static Exception FileNotFoundException(string name, Exception inner)
	{
		return new FileNotFoundException("Error loading \"" + name + "\". File not found.", inner);
	}

	internal static string NormalizeRelativePath(string name)
	{
		return new Uri("file:///" + FileHelpers.UrlEncode(name)).LocalPath.Substring(1).Replace(FileHelpers.NotSeparator, FileHelpers.Separator);
	}

	private static Stream PlatformOpenStream(string safeName)
	{
		return File.OpenRead(Path.Combine(TitleContainer.Location, safeName));
	}
}
