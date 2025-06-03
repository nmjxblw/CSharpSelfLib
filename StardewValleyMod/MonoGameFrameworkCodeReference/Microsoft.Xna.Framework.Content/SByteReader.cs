namespace Microsoft.Xna.Framework.Content;

internal class SByteReader : ContentTypeReader<sbyte>
{
	protected internal override sbyte Read(ContentReader input, sbyte existingInstance)
	{
		return input.ReadSByte();
	}
}
