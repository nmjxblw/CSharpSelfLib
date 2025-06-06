using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class FieldFilter : PathFilter
{
	internal string? Name;

	public FieldFilter(string? name)
	{
		this.Name = name;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
	{
		foreach (JToken item in current)
		{
			if (item is JObject jObject)
			{
				if (this.Name != null)
				{
					JToken jToken = jObject[this.Name];
					if (jToken != null)
					{
						yield return jToken;
					}
					else if (settings?.ErrorWhenNoMatch ?? false)
					{
						throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, this.Name));
					}
					continue;
				}
				foreach (KeyValuePair<string, JToken> item2 in jObject)
				{
					yield return item2.Value;
				}
			}
			else if (settings?.ErrorWhenNoMatch ?? false)
			{
				throw new JsonException("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, this.Name ?? "*", item.GetType().Name));
			}
		}
	}
}
