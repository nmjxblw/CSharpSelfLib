namespace Microsoft.Xna.Framework.Content;

internal class StringReader : ContentTypeReader<string>
{
	protected internal override string Read(ContentReader input, string existingInstance)
	{
		return input.ReadString();
	}
}
