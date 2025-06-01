namespace ContentManifest.Internal;

internal class CHElement : CHParsable
{
	public CHValue Value;

	public void Parse(CHJsonParserContext context)
	{
		context.SkipWhitespace();
		this.Value = new CHValue();
		this.Value.Parse(context);
		context.SkipWhitespace();
	}
}
