using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class BooleanQueryExpression : QueryExpression
{
	public readonly object Left;

	public readonly object? Right;

	public BooleanQueryExpression(QueryOperator @operator, object left, object? right)
		: base(@operator)
	{
		this.Left = left;
		this.Right = right;
	}

	private IEnumerable<JToken> GetResult(JToken root, JToken t, object? o)
	{
		if (o is JToken jToken)
		{
			return new JToken[1] { jToken };
		}
		if (o is List<PathFilter> filters)
		{
			return JPath.Evaluate(filters, root, t, null);
		}
		return CollectionUtils.ArrayEmpty<JToken>();
	}

	public override bool IsMatch(JToken root, JToken t, JsonSelectSettings? settings)
	{
		if (base.Operator == QueryOperator.Exists)
		{
			return this.GetResult(root, t, this.Left).Any();
		}
		using (IEnumerator<JToken> enumerator = this.GetResult(root, t, this.Left).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				IEnumerable<JToken> result = this.GetResult(root, t, this.Right);
				ICollection<JToken> collection = (result as ICollection<JToken>) ?? result.ToList();
				do
				{
					JToken current = enumerator.Current;
					foreach (JToken item in collection)
					{
						if (this.MatchTokens(current, item, settings))
						{
							return true;
						}
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return false;
	}

	private bool MatchTokens(JToken leftResult, JToken rightResult, JsonSelectSettings? settings)
	{
		if (leftResult is JValue jValue && rightResult is JValue jValue2)
		{
			switch (base.Operator)
			{
			case QueryOperator.RegexEquals:
				if (BooleanQueryExpression.RegexEquals(jValue, jValue2, settings))
				{
					return true;
				}
				break;
			case QueryOperator.Equals:
				if (BooleanQueryExpression.EqualsWithStringCoercion(jValue, jValue2))
				{
					return true;
				}
				break;
			case QueryOperator.StrictEquals:
				if (BooleanQueryExpression.EqualsWithStrictMatch(jValue, jValue2))
				{
					return true;
				}
				break;
			case QueryOperator.NotEquals:
				if (!BooleanQueryExpression.EqualsWithStringCoercion(jValue, jValue2))
				{
					return true;
				}
				break;
			case QueryOperator.StrictNotEquals:
				if (!BooleanQueryExpression.EqualsWithStrictMatch(jValue, jValue2))
				{
					return true;
				}
				break;
			case QueryOperator.GreaterThan:
				if (jValue.CompareTo(jValue2) > 0)
				{
					return true;
				}
				break;
			case QueryOperator.GreaterThanOrEquals:
				if (jValue.CompareTo(jValue2) >= 0)
				{
					return true;
				}
				break;
			case QueryOperator.LessThan:
				if (jValue.CompareTo(jValue2) < 0)
				{
					return true;
				}
				break;
			case QueryOperator.LessThanOrEquals:
				if (jValue.CompareTo(jValue2) <= 0)
				{
					return true;
				}
				break;
			case QueryOperator.Exists:
				return true;
			}
		}
		else
		{
			QueryOperator queryOperator = base.Operator;
			if ((uint)(queryOperator - 2) <= 1u)
			{
				return true;
			}
		}
		return false;
	}

	private static bool RegexEquals(JValue input, JValue pattern, JsonSelectSettings? settings)
	{
		if (input.Type != JTokenType.String || pattern.Type != JTokenType.String)
		{
			return false;
		}
		string obj = (string)pattern.Value;
		int num = obj.LastIndexOf('/');
		string pattern2 = obj.Substring(1, num - 1);
		string optionsText = obj.Substring(num + 1);
		TimeSpan matchTimeout = settings?.RegexMatchTimeout ?? Regex.InfiniteMatchTimeout;
		return Regex.IsMatch((string)input.Value, pattern2, MiscellaneousUtils.GetRegexOptions(optionsText), matchTimeout);
	}

	internal static bool EqualsWithStringCoercion(JValue value, JValue queryValue)
	{
		if (value.Equals(queryValue))
		{
			return true;
		}
		if ((value.Type == JTokenType.Integer && queryValue.Type == JTokenType.Float) || (value.Type == JTokenType.Float && queryValue.Type == JTokenType.Integer))
		{
			return JValue.Compare(value.Type, value.Value, queryValue.Value) == 0;
		}
		if (queryValue.Type != JTokenType.String)
		{
			return false;
		}
		string b = (string)queryValue.Value;
		string a;
		switch (value.Type)
		{
		case JTokenType.Date:
		{
			using (StringWriter stringWriter = StringUtils.CreateStringWriter(64))
			{
				if (value.Value is DateTimeOffset value2)
				{
					DateTimeUtils.WriteDateTimeOffsetString(stringWriter, value2, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
				}
				else
				{
					DateTimeUtils.WriteDateTimeString(stringWriter, (DateTime)value.Value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
				}
				a = stringWriter.ToString();
			}
			break;
		}
		case JTokenType.Bytes:
			a = Convert.ToBase64String((byte[])value.Value);
			break;
		case JTokenType.Guid:
		case JTokenType.TimeSpan:
			a = value.Value.ToString();
			break;
		case JTokenType.Uri:
			a = ((Uri)value.Value).OriginalString;
			break;
		default:
			return false;
		}
		return string.Equals(a, b, StringComparison.Ordinal);
	}

	internal static bool EqualsWithStrictMatch(JValue value, JValue queryValue)
	{
		if ((value.Type == JTokenType.Integer && queryValue.Type == JTokenType.Float) || (value.Type == JTokenType.Float && queryValue.Type == JTokenType.Integer))
		{
			return JValue.Compare(value.Type, value.Value, queryValue.Value) == 0;
		}
		if (value.Type != queryValue.Type)
		{
			return false;
		}
		return value.Equals(queryValue);
	}
}
