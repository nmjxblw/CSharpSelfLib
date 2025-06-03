using System;

namespace Pathoschild.Stardew.Common;

internal static class ObjectExtensions
{
	public static T AssertNotNull<T>(this T? value, string? paramName = null) where T : class
	{
		if (value == null)
		{
			throw new ArgumentNullException(paramName);
		}
		return value;
	}
}
