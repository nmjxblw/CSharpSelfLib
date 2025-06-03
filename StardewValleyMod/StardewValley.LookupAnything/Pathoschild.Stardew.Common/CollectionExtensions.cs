using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Common;

internal static class CollectionExtensions
{
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> values) where T : class
	{
		return values.Where((T p) => p != null);
	}

	public static HashSet<string> ToNonNullCaseInsensitive(this HashSet<string>? collection)
	{
		if (collection == null)
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		if (collection.Comparer != StringComparer.OrdinalIgnoreCase)
		{
			return new HashSet<string>(collection, StringComparer.OrdinalIgnoreCase);
		}
		return collection;
	}

	public static Dictionary<string, TValue> ToNonNullCaseInsensitive<TValue>(this Dictionary<string, TValue>? collection)
	{
		if (collection == null)
		{
			return new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
		}
		if (collection.Comparer != StringComparer.OrdinalIgnoreCase)
		{
			return new Dictionary<string, TValue>(collection, StringComparer.OrdinalIgnoreCase);
		}
		return collection;
	}
}
