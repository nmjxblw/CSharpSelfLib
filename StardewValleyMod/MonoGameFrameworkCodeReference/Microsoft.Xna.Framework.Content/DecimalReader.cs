namespace Microsoft.Xna.Framework.Content;

internal class DecimalReader : ContentTypeReader<decimal>
{
	protected internal override decimal Read(ContentReader input, decimal existingInstance)
	{
		return input.ReadDecimal();
	}
}
