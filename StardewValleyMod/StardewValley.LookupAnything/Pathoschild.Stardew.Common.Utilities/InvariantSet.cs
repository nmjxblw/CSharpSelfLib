using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathoschild.Stardew.Common.Utilities;

internal class InvariantSet : IInvariantSet, IReadOnlySet<string>, IEnumerable<string>, IEnumerable, IReadOnlyCollection<string>
{
	private static readonly HashSet<string> EmptyHashSet = new HashSet<string>();

	private readonly HashSet<string> Set;

	public static IInvariantSet Empty { get; } = new InvariantSet();

	public int Count => this.Set.Count;

	public InvariantSet()
	{
		this.Set = InvariantSet.EmptyHashSet;
	}

	public InvariantSet(IEnumerable<string> values)
	{
		this.Set = ((values is InvariantSet set) ? set.Set : ((values is HashSet<string> set2) ? set2.ToNonNullCaseInsensitive() : ((!(values is ICollection<string> { Count: 0 })) ? this.CreateSet(values) : InvariantSet.EmptyHashSet)));
	}

	public InvariantSet(params string[] values)
	{
		this.Set = this.CreateSet(values);
	}

	public IEnumerator<string> GetEnumerator()
	{
		return this.Set.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.Set.GetEnumerator();
	}

	public bool Contains(string item)
	{
		return this.Set.Contains(item);
	}

	public bool IsProperSubsetOf(IEnumerable<string> other)
	{
		return this.Set.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<string> other)
	{
		return this.Set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<string> other)
	{
		return this.Set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<string> other)
	{
		return this.Set.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<string> other)
	{
		return this.Set.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<string> other)
	{
		return this.Set.SetEquals(other);
	}

	public IInvariantSet GetWith(string other)
	{
		if (this.Count == 0)
		{
			return new InvariantSet(other);
		}
		if (this.Contains(other))
		{
			return this;
		}
		HashSet<string> set = this.CreateSet(this.Set);
		set.Add(other);
		return new InvariantSet(set);
	}

	public IInvariantSet GetWith(ICollection<string> other)
	{
		if (other.Count == 0)
		{
			return this;
		}
		if (this.Count == 0)
		{
			return new InvariantSet(this.CreateSet(other));
		}
		bool changed = false;
		HashSet<string> set = this.CreateSet(this.Set);
		foreach (string value in other)
		{
			changed |= set.Add(value);
		}
		if (!changed)
		{
			return this;
		}
		return new InvariantSet(set);
	}

	public IInvariantSet GetWithout(string other)
	{
		if (!this.Contains(other))
		{
			return this;
		}
		HashSet<string> copy = this.CreateSet(this);
		copy.Remove(other);
		return new InvariantSet(copy);
	}

	public IInvariantSet GetWithout(IEnumerable<string> other)
	{
		HashSet<string> copy = null;
		foreach (string value in other)
		{
			if (copy == null)
			{
				if (!this.Contains(value))
				{
					continue;
				}
				copy = this.CreateSet(this);
			}
			copy.Remove(value);
		}
		if (copy == null)
		{
			return this;
		}
		return new InvariantSet(copy);
	}

	private HashSet<string> CreateSet(IEnumerable<string> values)
	{
		return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
	}
}
