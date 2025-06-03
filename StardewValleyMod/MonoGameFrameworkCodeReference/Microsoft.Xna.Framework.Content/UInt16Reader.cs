namespace Microsoft.Xna.Framework.Content;

internal class UInt16Reader : ContentTypeReader<ushort>
{
	protected internal override ushort Read(ContentReader input, ushort existingInstance)
	{
		return input.ReadUInt16();
	}
}
