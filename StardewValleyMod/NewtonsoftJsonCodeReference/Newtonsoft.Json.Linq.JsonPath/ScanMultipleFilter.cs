using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class ScanMultipleFilter : PathFilter
{
	private List<string> _names;

	public ScanMultipleFilter(List<string> names)
	{
		this._names = names;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
	{
		foreach (JToken c in current)
		{
			JToken value = c;
			while (true)
			{
				JContainer container = value as JContainer;
				value = PathFilter.GetNextScanValue(c, container, value);
				if (value == null)
				{
					break;
				}
				if (!(value is JProperty property))
				{
					continue;
				}
				foreach (string name in this._names)
				{
					if (property.Name == name)
					{
						yield return property.Value;
					}
				}
			}
		}
	}
}
