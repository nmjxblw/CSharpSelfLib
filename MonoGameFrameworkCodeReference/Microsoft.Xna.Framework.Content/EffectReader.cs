using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class EffectReader : ContentTypeReader<Effect>
{
	protected internal override Effect Read(ContentReader input, Effect existingInstance)
	{
		int dataSize = input.ReadInt32();
		byte[] data = ContentManager.ScratchBufferPool.Get(dataSize);
		input.Read(data, 0, dataSize);
		Effect effect = new Effect(input.GetGraphicsDevice(), data, 0, dataSize);
		ContentManager.ScratchBufferPool.Return(data);
		effect.Name = input.AssetName;
		return effect;
	}
}
