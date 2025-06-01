using System;

namespace Newtonsoft.Json.Utilities;

internal readonly struct StructMultiKey<T1, T2> : IEquatable<StructMultiKey<T1, T2>>
{
	public readonly T1 Value1;

	public readonly T2 Value2;

	public StructMultiKey(T1 v1, T2 v2)
	{
		this.Value1 = v1;
		this.Value2 = v2;
	}

	public override int GetHashCode()
	{
		T1 value = this.Value1;
		int num = ((value != null) ? value.GetHashCode() : 0);
		T2 value2 = this.Value2;
		return num ^ ((value2 != null) ? value2.GetHashCode() : 0);
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is StructMultiKey<T1, T2> other))
		{
			return false;
		}
		return this.Equals(other);
	}

	public bool Equals(StructMultiKey<T1, T2> other)
	{
		if (object.Equals(this.Value1, other.Value1))
		{
			return object.Equals(this.Value2, other.Value2);
		}
		return false;
	}
}
