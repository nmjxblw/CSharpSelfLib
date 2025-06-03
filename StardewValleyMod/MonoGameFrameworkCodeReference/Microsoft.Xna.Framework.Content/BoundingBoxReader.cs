namespace Microsoft.Xna.Framework.Content;

internal class BoundingBoxReader : ContentTypeReader<BoundingBox>
{
	protected internal override BoundingBox Read(ContentReader input, BoundingBox existingInstance)
	{
		Vector3 min = input.ReadVector3();
		Vector3 max = input.ReadVector3();
		return new BoundingBox(min, max);
	}
}
