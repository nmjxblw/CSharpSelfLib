namespace Microsoft.Xna.Framework.Content;

internal class RayReader : ContentTypeReader<Ray>
{
	protected internal override Ray Read(ContentReader input, Ray existingInstance)
	{
		Vector3 position = input.ReadVector3();
		Vector3 direction = input.ReadVector3();
		return new Ray(position, direction);
	}
}
