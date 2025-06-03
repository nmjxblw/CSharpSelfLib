using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pathoschild.Stardew.Common;

internal static class ListExtensions
{
	public static bool IsInRange<T>(this IList<T> list, int index)
	{
		if (index >= 0)
		{
			return index < list.Count;
		}
		return false;
	}

	public static bool TryGetIndex<T>(this IList<T> list, int index, [NotNullWhen(true)] out T? value)
	{
		if (!list.IsInRange(index))
		{
			value = default(T);
			return false;
		}
		value = list[index];
		return true;
	}
}
