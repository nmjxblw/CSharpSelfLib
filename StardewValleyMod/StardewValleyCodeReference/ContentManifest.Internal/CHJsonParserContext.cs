using System;

namespace ContentManifest.Internal;

internal class CHJsonParserContext
{
	public int ReadHead;

	public string JsonText = "";

	public CHJsonParserContext(string jsonText)
	{
		this.JsonText = jsonText;
	}

	public void SkipWhitespace()
	{
		while (this.ReadHead < this.JsonText.Length)
		{
			switch (this.JsonText[this.ReadHead])
			{
			case '\t':
			case '\n':
			case '\r':
			case ' ':
				break;
			default:
				return;
			}
			this.ReadHead++;
		}
	}

	public void AssertReadHeadIsValid()
	{
		if (this.ReadHead < 0 || this.ReadHead >= this.JsonText.Length)
		{
			throw new InvalidOperationException();
		}
	}
}
