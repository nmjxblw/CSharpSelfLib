namespace Microsoft.Xna.Framework.Content;

internal class UInt64Reader : ContentTypeReader<ulong>
{
	protected internal override ulong Read(ContentReader input, ulong existingInstance)
	{
		return input.ReadUInt64();
	}
}
