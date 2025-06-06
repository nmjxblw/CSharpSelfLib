namespace Newtonsoft.Json.Linq.JsonPath;

internal abstract class QueryExpression
{
	internal QueryOperator Operator;

	public QueryExpression(QueryOperator @operator)
	{
		this.Operator = @operator;
	}

	public bool IsMatch(JToken root, JToken t)
	{
		return this.IsMatch(root, t, null);
	}

	public abstract bool IsMatch(JToken root, JToken t, JsonSelectSettings? settings);
}
