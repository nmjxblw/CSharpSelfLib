using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class QueryFilter : PathFilter
{
	internal QueryExpression Expression;

	public QueryFilter(QueryExpression expression)
	{
		this.Expression = expression;
	}

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
	{
		foreach (JToken item in current)
		{
			foreach (JToken item2 in (IEnumerable<JToken>)item)
			{
				if (this.Expression.IsMatch(root, item2, settings))
				{
					yield return item2;
				}
			}
		}
	}
}
