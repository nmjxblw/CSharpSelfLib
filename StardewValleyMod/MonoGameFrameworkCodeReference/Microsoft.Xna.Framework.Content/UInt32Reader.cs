namespace Microsoft.Xna.Framework.Content;

internal class UInt32Reader : ContentTypeReader<uint>
{
	protected internal override uint Read(ContentReader input, uint existingInstance)
	{
		return input.ReadUInt32();
	}
}
