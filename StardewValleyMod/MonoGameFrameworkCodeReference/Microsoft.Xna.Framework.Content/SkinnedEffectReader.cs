using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class SkinnedEffectReader : ContentTypeReader<SkinnedEffect>
{
	protected internal override SkinnedEffect Read(ContentReader input, SkinnedEffect existingInstance)
	{
		return new SkinnedEffect(input.GetGraphicsDevice())
		{
			Texture = (input.ReadExternalReference<Texture>() as Texture2D),
			WeightsPerVertex = input.ReadInt32(),
			DiffuseColor = input.ReadVector3(),
			EmissiveColor = input.ReadVector3(),
			SpecularColor = input.ReadVector3(),
			SpecularPower = input.ReadSingle(),
			Alpha = input.ReadSingle()
		};
	}
}
