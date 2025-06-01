using System.IO;

namespace MonoGame.Framework.Utilities;

internal static class Hash
{
	/// <summary>
	/// Compute a hash from a byte array.
	/// </summary>
	/// <remarks>
	/// Modified FNV Hash in C#
	/// http://stackoverflow.com/a/468084
	/// </remarks>
	internal static int ComputeHash(params byte[] data)
	{
		int hash = -2128831035;
		for (int i = 0; i < data.Length; i++)
		{
			hash = (hash ^ data[i]) * 16777619;
		}
		hash += hash << 13;
		hash ^= hash >> 7;
		hash += hash << 3;
		hash ^= hash >> 17;
		return hash + (hash << 5);
	}

	/// <summary>
	/// Compute a hash from the content of a stream and restore the position.
	/// </summary>
	/// <remarks>
	/// Modified FNV Hash in C#
	/// http://stackoverflow.com/a/468084
	/// </remarks>
	internal static int ComputeHash(Stream stream)
	{
		int hash = -2128831035;
		long prevPosition = stream.Position;
		stream.Position = 0L;
		byte[] data = new byte[1024];
		int length;
		while ((length = stream.Read(data, 0, data.Length)) != 0)
		{
			for (int i = 0; i < length; i++)
			{
				hash = (hash ^ data[i]) * 16777619;
			}
		}
		stream.Position = prevPosition;
		hash += hash << 13;
		hash ^= hash >> 7;
		hash += hash << 3;
		hash ^= hash >> 17;
		return hash + (hash << 5);
	}
}
