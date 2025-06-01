using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoGame.Framework.Utilities;

internal static class FileHelpers
{
	private static readonly char[] UrlSafeChars = new char[8] { '.', '_', '-', ';', '/', '?', '\\', ':' };

	public static readonly char ForwardSlash = '/';

	public static readonly string ForwardSlashString = new string(FileHelpers.ForwardSlash, 1);

	public static readonly char BackwardSlash = '\\';

	public static readonly char NotSeparator = ((Path.DirectorySeparatorChar == FileHelpers.BackwardSlash) ? FileHelpers.ForwardSlash : FileHelpers.BackwardSlash);

	public static readonly char Separator = Path.DirectorySeparatorChar;

	public static string NormalizeFilePathSeparators(string name)
	{
		return name.Replace(FileHelpers.NotSeparator, FileHelpers.Separator);
	}

	/// <summary>
	/// Combines the filePath and relativeFile based on relativeFile being a file in the same location as filePath.
	/// Relative directory operators (..) are also resolved
	/// </summary>
	/// <example>"A\B\C.txt","D.txt" becomes "A\B\D.txt"</example>
	/// <example>"A\B\C.txt","..\D.txt" becomes "A\D.txt"</example>
	/// <param name="filePath">Path to the file we are starting from</param>
	/// <param name="relativeFile">Relative location of another file to resolve the path to</param>
	public static string ResolveRelativePath(string filePath, string relativeFile)
	{
		filePath = filePath.Replace(FileHelpers.BackwardSlash, FileHelpers.ForwardSlash);
		relativeFile = relativeFile.Replace(FileHelpers.BackwardSlash, FileHelpers.ForwardSlash);
		while (filePath.Contains("//"))
		{
			filePath = filePath.Replace("//", "/");
		}
		bool num = filePath.StartsWith(FileHelpers.ForwardSlashString);
		if (!num)
		{
			filePath = FileHelpers.ForwardSlashString + filePath;
		}
		string localPath = new Uri(new Uri("file://" + FileHelpers.UrlEncode(filePath)), FileHelpers.UrlEncode(relativeFile)).LocalPath;
		if (!num && localPath.StartsWith("/"))
		{
			localPath = localPath.Substring(1);
		}
		return FileHelpers.TrimPath(FileHelpers.NormalizeFilePathSeparators(localPath));
	}

	internal static string UrlEncode(string url)
	{
		UTF8Encoding encoder = new UTF8Encoding();
		StringBuilder safeline = new StringBuilder(encoder.GetByteCount(url) * 3);
		for (int i = 0; i < url.Length; i++)
		{
			char c = url[i];
			if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || Array.IndexOf(FileHelpers.UrlSafeChars, c) != -1)
			{
				safeline.Append(c);
				continue;
			}
			byte[] bytes = encoder.GetBytes(c.ToString());
			foreach (byte num in bytes)
			{
				safeline.Append("%");
				safeline.Append(num.ToString("X"));
			}
		}
		return safeline.ToString();
	}

	private static string TrimPath(string filePath)
	{
		while (filePath.Contains("/./"))
		{
			filePath = filePath.Replace("/./", "/");
		}
		while (filePath.Contains("\\.\\"))
		{
			filePath = filePath.Replace("\\.\\", "\\");
		}
		filePath = Regex.Replace(filePath, "^\\.(\\/|\\\\)", string.Empty);
		filePath = Regex.Replace(filePath, "[^\\/\\\\]+(\\/|\\\\)\\.\\.(\\/|\\\\)", string.Empty);
		return filePath;
	}
}
