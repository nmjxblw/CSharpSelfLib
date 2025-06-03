namespace Microsoft.Xna.Framework.Content;

internal class Int64Reader : ContentTypeReader<long>
{
	protected internal override long Read(ContentReader input, long existingInstance)
	{
		return input.ReadInt64();
	}
}
