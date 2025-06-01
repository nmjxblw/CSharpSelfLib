using System.IO;
using Microsoft.Xna.Framework.Media;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content;

internal class SongReader : ContentTypeReader<Song>
{
	protected internal override Song Read(ContentReader input, Song existingInstance)
	{
		string path = input.ReadString();
		if (!string.IsNullOrEmpty(path))
		{
			path = FileHelpers.ResolveRelativePath(Path.Combine(input.ContentManager.RootDirectoryFullPath, input.AssetName), path);
		}
		int durationMs = input.ReadObject<int>();
		return new Song(path, durationMs);
	}
}
