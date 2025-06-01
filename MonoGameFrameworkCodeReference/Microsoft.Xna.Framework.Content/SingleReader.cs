namespace Microsoft.Xna.Framework.Content;

internal class SingleReader : ContentTypeReader<float>
{
	protected internal override float Read(ContentReader input, float existingInstance)
	{
		return input.ReadSingle();
	}
}
