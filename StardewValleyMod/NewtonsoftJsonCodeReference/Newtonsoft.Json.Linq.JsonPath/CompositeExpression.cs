using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class CompositeExpression : QueryExpression
{
	public List<QueryExpression> Expressions { get; set; }

	public CompositeExpression(QueryOperator @operator)
		: base(@operator)
	{
		this.Expressions = new List<QueryExpression>();
	}

	public override bool IsMatch(JToken root, JToken t, JsonSelectSettings? settings)
	{
		switch (base.Operator)
		{
		case QueryOperator.And:
			foreach (QueryExpression expression in this.Expressions)
			{
				if (!expression.IsMatch(root, t, settings))
				{
					return false;
				}
			}
			return true;
		case QueryOperator.Or:
			foreach (QueryExpression expression2 in this.Expressions)
			{
				if (expression2.IsMatch(root, t, settings))
				{
					return true;
				}
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
