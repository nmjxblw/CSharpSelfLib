namespace Microsoft.Xna.Framework.Content;

internal class QuaternionReader : ContentTypeReader<Quaternion>
{
	protected internal override Quaternion Read(ContentReader input, Quaternion existingInstance)
	{
		return input.ReadQuaternion();
	}
}
