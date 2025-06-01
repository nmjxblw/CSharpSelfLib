namespace Microsoft.Xna.Framework.Content;

internal class CharReader : ContentTypeReader<char>
{
	protected internal override char Read(ContentReader input, char existingInstance)
	{
		return input.ReadChar();
	}
}
