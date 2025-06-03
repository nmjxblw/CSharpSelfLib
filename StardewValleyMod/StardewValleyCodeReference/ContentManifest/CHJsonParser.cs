using ContentManifest.Internal;

namespace ContentManifest;

public class CHJsonParser
{
	public static object ParseJson(string text)
	{
		CHJsonParserContext context = new CHJsonParserContext(text);
		CHJson cHJson = new CHJson();
		cHJson.Parse(context);
		return cHJson.Element.Value.GetManagedObject();
	}
}
