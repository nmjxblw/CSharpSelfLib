namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// External reference reader, provided for compatibility with XNA Framework built content
/// </summary>
internal class ExternalReferenceReader : ContentTypeReader
{
	public ExternalReferenceReader()
		: base(null)
	{
	}

	protected internal override object Read(ContentReader input, object existingInstance)
	{
		return input.ReadExternalReference<object>();
	}
}
