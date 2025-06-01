using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class DualTextureEffectReader : ContentTypeReader<DualTextureEffect>
{
	protected internal override DualTextureEffect Read(ContentReader input, DualTextureEffect existingInstance)
	{
		return new DualTextureEffect(input.GetGraphicsDevice())
		{
			Texture = (input.ReadExternalReference<Texture>() as Texture2D),
			Texture2 = (input.ReadExternalReference<Texture>() as Texture2D),
			DiffuseColor = input.ReadVector3(),
			Alpha = input.ReadSingle(),
			VertexColorEnabled = input.ReadBoolean()
		};
	}
}
