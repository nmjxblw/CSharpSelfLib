using System.Collections.Generic;

namespace BirbCore.Extensions;

public static class CustomFieldsExtensions
{
	public static int? TryGetInt(this Dictionary<string, string>? customFields, string key)
	{
		if (customFields == null)
		{
			return null;
		}
		if (!customFields.TryGetValue(key, out string value))
		{
			return null;
		}
		if (!int.TryParse(value, out var result))
		{
			return null;
		}
		return result;
	}
}
