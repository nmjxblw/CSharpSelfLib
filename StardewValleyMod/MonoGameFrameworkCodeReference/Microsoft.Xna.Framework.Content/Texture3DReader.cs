using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class Texture3DReader : ContentTypeReader<Texture3D>
{
	protected internal override Texture3D Read(ContentReader reader, Texture3D existingInstance)
	{
		Texture3D texture = null;
		SurfaceFormat format = (SurfaceFormat)reader.ReadInt32();
		int width = reader.ReadInt32();
		int height = reader.ReadInt32();
		int depth = reader.ReadInt32();
		int levelCount = reader.ReadInt32();
		if (existingInstance == null)
		{
			texture = new Texture3D(reader.GetGraphicsDevice(), width, height, depth, levelCount > 1, format);
		}
		else
		{
			texture = existingInstance;
		}
		Threading.BlockOnUIThread(delegate
		{
			for (int i = 0; i < levelCount; i++)
			{
				int num = reader.ReadInt32();
				byte[] array = ContentManager.ScratchBufferPool.Get(num);
				reader.Read(array, 0, num);
				texture.SetData(i, 0, 0, width, height, 0, depth, array, 0, num);
				width = Math.Max(width >> 1, 1);
				height = Math.Max(height >> 1, 1);
				depth = Math.Max(depth >> 1, 1);
				ContentManager.ScratchBufferPool.Return(array);
			}
		});
		return texture;
	}
}
