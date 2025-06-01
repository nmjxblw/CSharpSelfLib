using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

internal class AlphaTestEffectReader : ContentTypeReader<AlphaTestEffect>
{
	protected internal override AlphaTestEffect Read(ContentReader input, AlphaTestEffect existingInstance)
	{
		return new AlphaTestEffect(input.GetGraphicsDevice())
		{
			Texture = (input.ReadExternalReference<Texture>() as Texture2D),
			AlphaFunction = (CompareFunction)input.ReadInt32(),
			ReferenceAlpha = (int)input.ReadUInt32(),
			DiffuseColor = input.ReadVector3(),
			Alpha = input.ReadSingle(),
			VertexColorEnabled = input.ReadBoolean()
		};
	}
}
