using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class TextureCubeReader : ContentTypeReader<TextureCube>
{
	protected internal override TextureCube Read(ContentReader reader, TextureCube existingInstance)
	{
		TextureCube textureCube = null;
		SurfaceFormat surfaceFormat = (SurfaceFormat)reader.ReadInt32();
		int size = reader.ReadInt32();
		int levels = reader.ReadInt32();
		if (existingInstance == null)
		{
			textureCube = new TextureCube(reader.GetGraphicsDevice(), size, levels > 1, surfaceFormat);
		}
		else
		{
			textureCube = existingInstance;
		}
		Threading.BlockOnUIThread(delegate
		{
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < levels; j++)
				{
					int num = reader.ReadInt32();
					byte[] array = ContentManager.ScratchBufferPool.Get(num);
					reader.Read(array, 0, num);
					textureCube.SetData((CubeMapFace)i, j, null, array, 0, num);
					ContentManager.ScratchBufferPool.Return(array);
				}
			}
		});
		return textureCube;
	}
}
