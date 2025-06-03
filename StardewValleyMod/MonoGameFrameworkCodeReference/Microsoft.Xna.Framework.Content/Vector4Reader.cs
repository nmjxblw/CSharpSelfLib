namespace Microsoft.Xna.Framework.Content;

internal class Vector4Reader : ContentTypeReader<Vector4>
{
	protected internal override Vector4 Read(ContentReader input, Vector4 existingInstance)
	{
		return input.ReadVector4();
	}
}
