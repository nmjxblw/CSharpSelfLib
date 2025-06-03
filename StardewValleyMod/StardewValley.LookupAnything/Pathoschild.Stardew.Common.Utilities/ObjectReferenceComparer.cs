using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pathoschild.Stardew.Common.Utilities;

internal class ObjectReferenceComparer<T> : IEqualityComparer<T>
{
	public bool Equals(T? x, T? y)
	{
		return (object)x == (object)y;
	}

	public int GetHashCode(T obj)
	{
		return RuntimeHelpers.GetHashCode(obj);
	}
}
