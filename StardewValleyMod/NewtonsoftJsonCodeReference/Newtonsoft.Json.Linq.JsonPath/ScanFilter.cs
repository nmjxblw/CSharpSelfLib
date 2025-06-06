using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class ScanFilter : PathFilter
{
	internal string? Name;

	public ScanFilter(string? name)
	{
		this.Name = name;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
	{
		foreach (JToken c in current)
		{
			if (this.Name == null)
			{
				yield return c;
			}
			JToken value = c;
			while (true)
			{
				JContainer container = value as JContainer;
				value = PathFilter.GetNextScanValue(c, container, value);
				if (value == null)
				{
					break;
				}
				if (value is JProperty jProperty)
				{
					if (jProperty.Name == this.Name)
					{
						yield return jProperty.Value;
					}
				}
				else if (this.Name == null)
				{
					yield return value;
				}
			}
		}
	}
}
