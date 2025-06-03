using System;
using System.Collections.Generic;

namespace ContentManifest.Internal;

internal class CHObject : CHParsable
{
	public readonly Dictionary<string, object> Members = new Dictionary<string, object>();

	public void Parse(CHJsonParserContext context)
	{
		if (context.JsonText[context.ReadHead] != '{')
		{
			throw new InvalidOperationException();
		}
		context.ReadHead++;
		bool needsMember = false;
		while (true)
		{
			context.SkipWhitespace();
			context.AssertReadHeadIsValid();
			switch (context.JsonText[context.ReadHead])
			{
			case '}':
				if (needsMember)
				{
					throw new InvalidOperationException();
				}
				context.ReadHead++;
				return;
			case '"':
			{
				CHString memberKey = new CHString();
				memberKey.Parse(context);
				context.SkipWhitespace();
				context.AssertReadHeadIsValid();
				if (context.JsonText[context.ReadHead] != ':')
				{
					throw new InvalidOperationException();
				}
				context.ReadHead++;
				CHElement element = new CHElement();
				element.Parse(context);
				this.Members[memberKey.RawString] = element.Value.GetManagedObject();
				needsMember = false;
				context.SkipWhitespace();
				context.AssertReadHeadIsValid();
				if (context.JsonText[context.ReadHead] == ',')
				{
					context.ReadHead++;
					needsMember = true;
				}
				break;
			}
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
