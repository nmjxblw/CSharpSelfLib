using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class EnvironmentMapEffectReader : ContentTypeReader<EnvironmentMapEffect>
{
	protected internal override EnvironmentMapEffect Read(ContentReader input, EnvironmentMapEffect existingInstance)
	{
		return new EnvironmentMapEffect(input.GetGraphicsDevice())
		{
			Texture = (input.ReadExternalReference<Texture>() as Texture2D),
			EnvironmentMap = input.ReadExternalReference<TextureCube>(),
			EnvironmentMapAmount = input.ReadSingle(),
			EnvironmentMapSpecular = input.ReadVector3(),
			FresnelFactor = input.ReadSingle(),
			DiffuseColor = input.ReadVector3(),
			EmissiveColor = input.ReadVector3(),
			Alpha = input.ReadSingle()
		};
	}
}
