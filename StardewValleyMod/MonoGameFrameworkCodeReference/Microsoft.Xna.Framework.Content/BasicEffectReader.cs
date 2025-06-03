using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class BasicEffectReader : ContentTypeReader<BasicEffect>
{
	protected internal override BasicEffect Read(ContentReader input, BasicEffect existingInstance)
	{
		BasicEffect effect = new BasicEffect(input.GetGraphicsDevice());
		if (input.ReadExternalReference<Texture>() is Texture2D texture)
		{
			effect.Texture = texture;
			effect.TextureEnabled = true;
		}
		effect.DiffuseColor = input.ReadVector3();
		effect.EmissiveColor = input.ReadVector3();
		effect.SpecularColor = input.ReadVector3();
		effect.SpecularPower = input.ReadSingle();
		effect.Alpha = input.ReadSingle();
		effect.VertexColorEnabled = input.ReadBoolean();
		return effect;
	}
}
