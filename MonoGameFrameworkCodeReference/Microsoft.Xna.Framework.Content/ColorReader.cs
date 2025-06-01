namespace Microsoft.Xna.Framework.Content;

internal class ColorReader : ContentTypeReader<Color>
{
	protected internal override Color Read(ContentReader input, Color existingInstance)
	{
		byte r = input.ReadByte();
		byte g = input.ReadByte();
		byte b = input.ReadByte();
		byte a = input.ReadByte();
		return new Color(r, g, b, a);
	}
}
