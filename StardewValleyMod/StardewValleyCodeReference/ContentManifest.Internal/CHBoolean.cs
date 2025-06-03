using System;

namespace ContentManifest.Internal;

internal class CHBoolean : CHParsable
{
	public bool RawBoolean;

	public void Parse(CHJsonParserContext context)
	{
		string jsonText = context.JsonText;
		int readHead = context.ReadHead;
		switch (jsonText[readHead])
		{
		case 't':
			if (readHead + 3 >= jsonText.Length)
			{
				throw new InvalidOperationException();
			}
			if (jsonText[readHead + 1] != 'r' || jsonText[readHead + 2] != 'u' || jsonText[readHead + 3] != 'e')
			{
				throw new InvalidOperationException();
			}
			context.ReadHead += 4;
			this.RawBoolean = true;
			break;
		case 'f':
			if (readHead + 4 >= jsonText.Length)
			{
				throw new InvalidOperationException();
			}
			if (jsonText[readHead + 1] != 'a' || jsonText[readHead + 2] != 'l' || jsonText[readHead + 3] != 's' || jsonText[readHead + 4] != 'e')
			{
				throw new InvalidOperationException();
			}
			context.ReadHead += 5;
			this.RawBoolean = false;
			break;
		default:
			throw new NotImplementedException();
		}
	}
}
