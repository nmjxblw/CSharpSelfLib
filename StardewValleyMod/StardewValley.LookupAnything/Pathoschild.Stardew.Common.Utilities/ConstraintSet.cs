using System.Collections.Generic;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.Common.Utilities;

internal class ConstraintSet<T>
{
	public HashSet<T> RestrictToValues { get; }

	public HashSet<T> ExcludeValues { get; }

	public bool IsBounded => this.RestrictToValues.Count != 0;

	public bool IsInfinite => !this.IsBounded;

	public bool IsConstrained
	{
		get
		{
			if (this.RestrictToValues.Count == 0)
			{
				return this.ExcludeValues.Count != 0;
			}
			return true;
		}
	}

	public ConstraintSet()
		: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public ConstraintSet(IEqualityComparer<T> comparer)
	{
		this.RestrictToValues = new HashSet<T>(comparer);
		this.ExcludeValues = new HashSet<T>(comparer);
	}

	public bool AddBound(T value)
	{
		return this.RestrictToValues.Add(value);
	}

	public bool AddBound(IEnumerable<T> values)
	{
		return CollectionExtensions.AddRange<T>((ISet<T>)this.RestrictToValues, values) > 0;
	}

	public bool Exclude(T value)
	{
		return this.ExcludeValues.Add(value);
	}

	public bool Exclude(IEnumerable<T> values)
	{
		return CollectionExtensions.AddRange<T>((ISet<T>)this.ExcludeValues, values) > 0;
	}

	public bool Intersects(ConstraintSet<T> other)
	{
		if (this.IsInfinite && other.IsInfinite)
		{
			return true;
		}
		if (this.IsBounded)
		{
			foreach (T value in this.RestrictToValues)
			{
				if (this.Allows(value) && other.Allows(value))
				{
					return true;
				}
			}
		}
		if (other.IsBounded)
		{
			foreach (T value2 in other.RestrictToValues)
			{
				if (other.Allows(value2) && this.Allows(value2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Allows(T value)
	{
		if (this.ExcludeValues.Contains(value))
		{
			return false;
		}
		if (!this.IsInfinite)
		{
			return this.RestrictToValues.Contains(value);
		}
		return true;
	}
}
