using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class JPath
{
	private static readonly char[] FloatCharacters = new char[3] { '.', 'E', 'e' };

	private readonly string _expression;

	private int _currentIndex;

	public List<PathFilter> Filters { get; }

	public JPath(string expression)
	{
		ValidationUtils.ArgumentNotNull(expression, "expression");
		this._expression = expression;
		this.Filters = new List<PathFilter>();
		this.ParseMain();
	}

	private void ParseMain()
	{
		int currentIndex = this._currentIndex;
		this.EatWhitespace();
		if (this._expression.Length == this._currentIndex)
		{
			return;
		}
		if (this._expression[this._currentIndex] == '$')
		{
			if (this._expression.Length == 1)
			{
				return;
			}
			char c = this._expression[this._currentIndex + 1];
			if (c == '.' || c == '[')
			{
				this._currentIndex++;
				currentIndex = this._currentIndex;
			}
		}
		if (!this.ParsePath(this.Filters, currentIndex, query: false))
		{
			int currentIndex2 = this._currentIndex;
			this.EatWhitespace();
			if (this._currentIndex < this._expression.Length)
			{
				throw new JsonException("Unexpected character while parsing path: " + this._expression[currentIndex2]);
			}
		}
	}

	private bool ParsePath(List<PathFilter> filters, int currentPartStartIndex, bool query)
	{
		bool scan = false;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		while (this._currentIndex < this._expression.Length && !flag3)
		{
			char c = this._expression[this._currentIndex];
			switch (c)
			{
			case '(':
			case '[':
				if (this._currentIndex > currentPartStartIndex)
				{
					string text = this._expression.Substring(currentPartStartIndex, this._currentIndex - currentPartStartIndex);
					if (text == "*")
					{
						text = null;
					}
					filters.Add(JPath.CreatePathFilter(text, scan));
					scan = false;
				}
				filters.Add(this.ParseIndexer(c, scan));
				scan = false;
				this._currentIndex++;
				currentPartStartIndex = this._currentIndex;
				flag = true;
				flag2 = false;
				break;
			case ')':
			case ']':
				flag3 = true;
				break;
			case ' ':
				if (this._currentIndex < this._expression.Length)
				{
					flag3 = true;
				}
				break;
			case '.':
				if (this._currentIndex > currentPartStartIndex)
				{
					string text2 = this._expression.Substring(currentPartStartIndex, this._currentIndex - currentPartStartIndex);
					if (text2 == "*")
					{
						text2 = null;
					}
					filters.Add(JPath.CreatePathFilter(text2, scan));
					scan = false;
				}
				if (this._currentIndex + 1 < this._expression.Length && this._expression[this._currentIndex + 1] == '.')
				{
					scan = true;
					this._currentIndex++;
				}
				this._currentIndex++;
				currentPartStartIndex = this._currentIndex;
				flag = false;
				flag2 = true;
				break;
			default:
				if (query && (c == '=' || c == '<' || c == '!' || c == '>' || c == '|' || c == '&'))
				{
					flag3 = true;
					break;
				}
				if (flag)
				{
					throw new JsonException("Unexpected character following indexer: " + c);
				}
				this._currentIndex++;
				break;
			}
		}
		bool flag4 = this._currentIndex == this._expression.Length;
		if (this._currentIndex > currentPartStartIndex)
		{
			string text3 = this._expression.Substring(currentPartStartIndex, this._currentIndex - currentPartStartIndex).TrimEnd();
			if (text3 == "*")
			{
				text3 = null;
			}
			filters.Add(JPath.CreatePathFilter(text3, scan));
		}
		else if (flag2 && (flag4 || query))
		{
			throw new JsonException("Unexpected end while parsing path.");
		}
		return flag4;
	}

	private static PathFilter CreatePathFilter(string? member, bool scan)
	{
		if (!scan)
		{
			return new FieldFilter(member);
		}
		return new ScanFilter(member);
	}

	private PathFilter ParseIndexer(char indexerOpenChar, bool scan)
	{
		this._currentIndex++;
		char indexerCloseChar = ((indexerOpenChar == '[') ? ']' : ')');
		this.EnsureLength("Path ended with open indexer.");
		this.EatWhitespace();
		if (this._expression[this._currentIndex] == '\'')
		{
			return this.ParseQuotedField(indexerCloseChar, scan);
		}
		if (this._expression[this._currentIndex] == '?')
		{
			return this.ParseQuery(indexerCloseChar, scan);
		}
		return this.ParseArrayIndexer(indexerCloseChar);
	}

	private PathFilter ParseArrayIndexer(char indexerCloseChar)
	{
		int currentIndex = this._currentIndex;
		int? num = null;
		List<int> list = null;
		int num2 = 0;
		int? start = null;
		int? end = null;
		int? step = null;
		while (this._currentIndex < this._expression.Length)
		{
			char c = this._expression[this._currentIndex];
			if (c == ' ')
			{
				num = this._currentIndex;
				this.EatWhitespace();
				continue;
			}
			if (c == indexerCloseChar)
			{
				int num3 = (num ?? this._currentIndex) - currentIndex;
				if (list != null)
				{
					if (num3 == 0)
					{
						throw new JsonException("Array index expected.");
					}
					int item = Convert.ToInt32(this._expression.Substring(currentIndex, num3), CultureInfo.InvariantCulture);
					list.Add(item);
					return new ArrayMultipleIndexFilter(list);
				}
				if (num2 > 0)
				{
					if (num3 > 0)
					{
						int value = Convert.ToInt32(this._expression.Substring(currentIndex, num3), CultureInfo.InvariantCulture);
						if (num2 == 1)
						{
							end = value;
						}
						else
						{
							step = value;
						}
					}
					return new ArraySliceFilter
					{
						Start = start,
						End = end,
						Step = step
					};
				}
				if (num3 == 0)
				{
					throw new JsonException("Array index expected.");
				}
				int value2 = Convert.ToInt32(this._expression.Substring(currentIndex, num3), CultureInfo.InvariantCulture);
				return new ArrayIndexFilter
				{
					Index = value2
				};
			}
			switch (c)
			{
			case ',':
			{
				int num5 = (num ?? this._currentIndex) - currentIndex;
				if (num5 == 0)
				{
					throw new JsonException("Array index expected.");
				}
				if (list == null)
				{
					list = new List<int>();
				}
				string value4 = this._expression.Substring(currentIndex, num5);
				list.Add(Convert.ToInt32(value4, CultureInfo.InvariantCulture));
				this._currentIndex++;
				this.EatWhitespace();
				currentIndex = this._currentIndex;
				num = null;
				break;
			}
			case '*':
				this._currentIndex++;
				this.EnsureLength("Path ended with open indexer.");
				this.EatWhitespace();
				if (this._expression[this._currentIndex] != indexerCloseChar)
				{
					throw new JsonException("Unexpected character while parsing path indexer: " + c);
				}
				return new ArrayIndexFilter();
			case ':':
			{
				int num4 = (num ?? this._currentIndex) - currentIndex;
				if (num4 > 0)
				{
					int value3 = Convert.ToInt32(this._expression.Substring(currentIndex, num4), CultureInfo.InvariantCulture);
					switch (num2)
					{
					case 0:
						start = value3;
						break;
					case 1:
						end = value3;
						break;
					default:
						step = value3;
						break;
					}
				}
				num2++;
				this._currentIndex++;
				this.EatWhitespace();
				currentIndex = this._currentIndex;
				num = null;
				break;
			}
			default:
				if (!char.IsDigit(c) && c != '-')
				{
					throw new JsonException("Unexpected character while parsing path indexer: " + c);
				}
				if (num.HasValue)
				{
					throw new JsonException("Unexpected character while parsing path indexer: " + c);
				}
				this._currentIndex++;
				break;
			}
		}
		throw new JsonException("Path ended with open indexer.");
	}

	private void EatWhitespace()
	{
		while (this._currentIndex < this._expression.Length && this._expression[this._currentIndex] == ' ')
		{
			this._currentIndex++;
		}
	}

	private PathFilter ParseQuery(char indexerCloseChar, bool scan)
	{
		this._currentIndex++;
		this.EnsureLength("Path ended with open indexer.");
		if (this._expression[this._currentIndex] != '(')
		{
			throw new JsonException("Unexpected character while parsing path indexer: " + this._expression[this._currentIndex]);
		}
		this._currentIndex++;
		QueryExpression expression = this.ParseExpression();
		this._currentIndex++;
		this.EnsureLength("Path ended with open indexer.");
		this.EatWhitespace();
		if (this._expression[this._currentIndex] != indexerCloseChar)
		{
			throw new JsonException("Unexpected character while parsing path indexer: " + this._expression[this._currentIndex]);
		}
		if (!scan)
		{
			return new QueryFilter(expression);
		}
		return new QueryScanFilter(expression);
	}

	private bool TryParseExpression(out List<PathFilter>? expressionPath)
	{
		if (this._expression[this._currentIndex] == '$')
		{
			expressionPath = new List<PathFilter> { RootFilter.Instance };
		}
		else
		{
			if (this._expression[this._currentIndex] != '@')
			{
				expressionPath = null;
				return false;
			}
			expressionPath = new List<PathFilter>();
		}
		this._currentIndex++;
		if (this.ParsePath(expressionPath, this._currentIndex, query: true))
		{
			throw new JsonException("Path ended with open query.");
		}
		return true;
	}

	private JsonException CreateUnexpectedCharacterException()
	{
		return new JsonException("Unexpected character while parsing path query: " + this._expression[this._currentIndex]);
	}

	private object ParseSide()
	{
		this.EatWhitespace();
		if (this.TryParseExpression(out List<PathFilter> expressionPath))
		{
			this.EatWhitespace();
			this.EnsureLength("Path ended with open query.");
			return expressionPath;
		}
		if (this.TryParseValue(out object value))
		{
			this.EatWhitespace();
			this.EnsureLength("Path ended with open query.");
			return new JValue(value);
		}
		throw this.CreateUnexpectedCharacterException();
	}

	private QueryExpression ParseExpression()
	{
		QueryExpression queryExpression = null;
		CompositeExpression compositeExpression = null;
		while (this._currentIndex < this._expression.Length)
		{
			object left = this.ParseSide();
			object right = null;
			QueryOperator queryOperator;
			if (this._expression[this._currentIndex] == ')' || this._expression[this._currentIndex] == '|' || this._expression[this._currentIndex] == '&')
			{
				queryOperator = QueryOperator.Exists;
			}
			else
			{
				queryOperator = this.ParseOperator();
				right = this.ParseSide();
			}
			BooleanQueryExpression booleanQueryExpression = new BooleanQueryExpression(queryOperator, left, right);
			if (this._expression[this._currentIndex] == ')')
			{
				if (compositeExpression != null)
				{
					compositeExpression.Expressions.Add(booleanQueryExpression);
					return queryExpression;
				}
				return booleanQueryExpression;
			}
			if (this._expression[this._currentIndex] == '&')
			{
				if (!this.Match("&&"))
				{
					throw this.CreateUnexpectedCharacterException();
				}
				if (compositeExpression == null || compositeExpression.Operator != QueryOperator.And)
				{
					CompositeExpression compositeExpression2 = new CompositeExpression(QueryOperator.And);
					compositeExpression?.Expressions.Add(compositeExpression2);
					compositeExpression = compositeExpression2;
					if (queryExpression == null)
					{
						queryExpression = compositeExpression;
					}
				}
				compositeExpression.Expressions.Add(booleanQueryExpression);
			}
			if (this._expression[this._currentIndex] != '|')
			{
				continue;
			}
			if (!this.Match("||"))
			{
				throw this.CreateUnexpectedCharacterException();
			}
			if (compositeExpression == null || compositeExpression.Operator != QueryOperator.Or)
			{
				CompositeExpression compositeExpression3 = new CompositeExpression(QueryOperator.Or);
				compositeExpression?.Expressions.Add(compositeExpression3);
				compositeExpression = compositeExpression3;
				if (queryExpression == null)
				{
					queryExpression = compositeExpression;
				}
			}
			compositeExpression.Expressions.Add(booleanQueryExpression);
		}
		throw new JsonException("Path ended with open query.");
	}

	private bool TryParseValue(out object? value)
	{
		char c = this._expression[this._currentIndex];
		if (c == '\'')
		{
			value = this.ReadQuotedString();
			return true;
		}
		if (char.IsDigit(c) || c == '-')
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(c);
			this._currentIndex++;
			while (this._currentIndex < this._expression.Length)
			{
				c = this._expression[this._currentIndex];
				if (c == ' ' || c == ')')
				{
					string text = stringBuilder.ToString();
					if (text.IndexOfAny(JPath.FloatCharacters) != -1)
					{
						double result2;
						bool result = double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result2);
						value = result2;
						return result;
					}
					long result4;
					bool result3 = long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result4);
					value = result4;
					return result3;
				}
				stringBuilder.Append(c);
				this._currentIndex++;
			}
		}
		else
		{
			switch (c)
			{
			case 't':
				if (this.Match("true"))
				{
					value = true;
					return true;
				}
				break;
			case 'f':
				if (this.Match("false"))
				{
					value = false;
					return true;
				}
				break;
			case 'n':
				if (this.Match("null"))
				{
					value = null;
					return true;
				}
				break;
			case '/':
				value = this.ReadRegexString();
				return true;
			}
		}
		value = null;
		return false;
	}

	private string ReadQuotedString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		this._currentIndex++;
		while (this._currentIndex < this._expression.Length)
		{
			char c = this._expression[this._currentIndex];
			if (c == '\\' && this._currentIndex + 1 < this._expression.Length)
			{
				this._currentIndex++;
				c = this._expression[this._currentIndex];
				char value;
				switch (c)
				{
				case 'b':
					value = '\b';
					break;
				case 't':
					value = '\t';
					break;
				case 'n':
					value = '\n';
					break;
				case 'f':
					value = '\f';
					break;
				case 'r':
					value = '\r';
					break;
				case '"':
				case '\'':
				case '/':
				case '\\':
					value = c;
					break;
				default:
					throw new JsonException("Unknown escape character: \\" + c);
				}
				stringBuilder.Append(value);
				this._currentIndex++;
			}
			else
			{
				if (c == '\'')
				{
					this._currentIndex++;
					return stringBuilder.ToString();
				}
				this._currentIndex++;
				stringBuilder.Append(c);
			}
		}
		throw new JsonException("Path ended with an open string.");
	}

	private string ReadRegexString()
	{
		int currentIndex = this._currentIndex;
		this._currentIndex++;
		while (this._currentIndex < this._expression.Length)
		{
			char c = this._expression[this._currentIndex];
			if (c == '\\' && this._currentIndex + 1 < this._expression.Length)
			{
				this._currentIndex += 2;
				continue;
			}
			if (c == '/')
			{
				this._currentIndex++;
				while (this._currentIndex < this._expression.Length)
				{
					c = this._expression[this._currentIndex];
					if (!char.IsLetter(c))
					{
						break;
					}
					this._currentIndex++;
				}
				return this._expression.Substring(currentIndex, this._currentIndex - currentIndex);
			}
			this._currentIndex++;
		}
		throw new JsonException("Path ended with an open regex.");
	}

	private bool Match(string s)
	{
		int num = this._currentIndex;
		for (int i = 0; i < s.Length; i++)
		{
			if (num < this._expression.Length && this._expression[num] == s[i])
			{
				num++;
				continue;
			}
			return false;
		}
		this._currentIndex = num;
		return true;
	}

	private QueryOperator ParseOperator()
	{
		if (this._currentIndex + 1 >= this._expression.Length)
		{
			throw new JsonException("Path ended with open query.");
		}
		if (this.Match("==="))
		{
			return QueryOperator.StrictEquals;
		}
		if (this.Match("=="))
		{
			return QueryOperator.Equals;
		}
		if (this.Match("=~"))
		{
			return QueryOperator.RegexEquals;
		}
		if (this.Match("!=="))
		{
			return QueryOperator.StrictNotEquals;
		}
		if (this.Match("!=") || this.Match("<>"))
		{
			return QueryOperator.NotEquals;
		}
		if (this.Match("<="))
		{
			return QueryOperator.LessThanOrEquals;
		}
		if (this.Match("<"))
		{
			return QueryOperator.LessThan;
		}
		if (this.Match(">="))
		{
			return QueryOperator.GreaterThanOrEquals;
		}
		if (this.Match(">"))
		{
			return QueryOperator.GreaterThan;
		}
		throw new JsonException("Could not read query operator.");
	}

	private PathFilter ParseQuotedField(char indexerCloseChar, bool scan)
	{
		List<string> list = null;
		while (this._currentIndex < this._expression.Length)
		{
			string text = this.ReadQuotedString();
			this.EatWhitespace();
			this.EnsureLength("Path ended with open indexer.");
			if (this._expression[this._currentIndex] == indexerCloseChar)
			{
				if (list != null)
				{
					list.Add(text);
					if (!scan)
					{
						return new FieldMultipleFilter(list);
					}
					return new ScanMultipleFilter(list);
				}
				return JPath.CreatePathFilter(text, scan);
			}
			if (this._expression[this._currentIndex] == ',')
			{
				this._currentIndex++;
				this.EatWhitespace();
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(text);
				continue;
			}
			throw new JsonException("Unexpected character while parsing path indexer: " + this._expression[this._currentIndex]);
		}
		throw new JsonException("Path ended with open indexer.");
	}

	private void EnsureLength(string message)
	{
		if (this._currentIndex >= this._expression.Length)
		{
			throw new JsonException(message);
		}
	}

	internal IEnumerable<JToken> Evaluate(JToken root, JToken t, JsonSelectSettings? settings)
	{
		return JPath.Evaluate(this.Filters, root, t, settings);
	}

	internal static IEnumerable<JToken> Evaluate(List<PathFilter> filters, JToken root, JToken t, JsonSelectSettings? settings)
	{
		IEnumerable<JToken> enumerable = new JToken[1] { t };
		foreach (PathFilter filter in filters)
		{
			enumerable = filter.ExecuteFilter(root, enumerable, settings);
		}
		return enumerable;
	}
}
