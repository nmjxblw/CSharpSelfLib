namespace Microsoft.Xna.Framework.Content;

internal class BoundingFrustumReader : ContentTypeReader<BoundingFrustum>
{
	protected internal override BoundingFrustum Read(ContentReader input, BoundingFrustum existingInstance)
	{
		return new BoundingFrustum(input.ReadMatrix());
	}
}
