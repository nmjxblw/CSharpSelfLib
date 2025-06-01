namespace Microsoft.Xna.Framework.Content;

internal class BooleanReader : ContentTypeReader<bool>
{
	protected internal override bool Read(ContentReader input, bool existingInstance)
	{
		return input.ReadBoolean();
	}
}
