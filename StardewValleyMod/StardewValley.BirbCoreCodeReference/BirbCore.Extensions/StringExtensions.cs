using System.Text;

namespace BirbCore.Extensions;

public static class StringExtensions
{
	public static string ToSnakeCase(this string str)
	{
		if (str == null)
		{
			return null;
		}
		if (str.Length < 2)
		{
			return str.ToLowerInvariant();
		}
		StringBuilder sb = new StringBuilder();
		sb.Append(char.ToLowerInvariant(str[0]));
		for (int i = 1; i < str.Length; i++)
		{
			char c = str[i];
			if (char.IsUpper(c))
			{
				sb.Append('_');
				sb.Append(char.ToLowerInvariant(c));
			}
			else
			{
				sb.Append(c);
			}
		}
		return sb.ToString();
	}

	public static string ToPascalCase(this string str)
	{
		if (str == null)
		{
			return null;
		}
		bool nextUpper = true;
		StringBuilder sb = new StringBuilder();
		foreach (char c in str)
		{
			if ((c == ' ' || c == '-' || c == '_') ? true : false)
			{
				nextUpper = true;
			}
			else if (nextUpper)
			{
				nextUpper = false;
				sb.Append(char.ToUpperInvariant(c));
			}
			else
			{
				sb.Append(char.ToLowerInvariant(c));
			}
		}
		return sb.ToString();
	}

	public static string ToCamelCase(this string str)
	{
		if (str == null)
		{
			return null;
		}
		bool nextUpper = false;
		StringBuilder sb = new StringBuilder();
		foreach (char c in str)
		{
			if ((c == ' ' || c == '-' || c == '_') ? true : false)
			{
				nextUpper = true;
			}
			else if (nextUpper)
			{
				nextUpper = false;
				sb.Append(char.ToUpperInvariant(c));
			}
			else
			{
				sb.Append(char.ToLowerInvariant(c));
			}
		}
		return sb.ToString();
	}
}
