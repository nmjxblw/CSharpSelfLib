namespace ContentManifest.Internal;

internal class CHJson : CHParsable
{
	public CHElement Element;

	public void Parse(CHJsonParserContext context)
	{
		this.Element = new CHElement();
		this.Element.Parse(context);
	}
}
