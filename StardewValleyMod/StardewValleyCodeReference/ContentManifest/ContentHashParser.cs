using System.Collections.Generic;
using System.IO;

namespace ContentManifest;

public class ContentHashParser
{
	public static Dictionary<string, object> ParseFromFile(string contentHashPath)
	{
		return CHJsonParser.ParseJson(File.ReadAllText(contentHashPath)) as Dictionary<string, object>;
	}
}
