namespace Microsoft.Xna.Framework.Content;

internal class Int32Reader : ContentTypeReader<int>
{
	protected internal override int Read(ContentReader input, int existingInstance)
	{
		return input.ReadInt32();
	}
}
