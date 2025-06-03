using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Pathoschild.Stardew.Common.Utilities;

internal class MutableInvariantSet : ISet<string>, ICollection<string>, IEnumerable<string>, IEnumerable
{
	private HashSet<string>? Set;

	private IInvariantSet? CachedImmutableSet;

	public int Count => this.Set?.Count ?? 0;

	public bool IsReadOnly { get; private set; }

	[MemberNotNullWhen(false, "Set")]
	public bool IsEmpty
	{
		[MemberNotNullWhen(false, "Set")]
		get
		{
			int? num = this.Set?.Count;
			if (!num.HasValue || num.GetValueOrDefault() == 0)
			{
				return true;
			}
			return false;
		}
	}

	public MutableInvariantSet()
	{
	}

	public MutableInvariantSet(IEnumerable<string> values)
	{
		this.Set = this.CreateSet(values);
	}

	public IEnumerator<string> GetEnumerator()
	{
		HashSet<string>.Enumerator? enumerator = this.Set?.GetEnumerator();
		if (!enumerator.HasValue)
		{
			return Enumerable.Empty<string>().GetEnumerator();
		}
		return enumerator.GetValueOrDefault();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		HashSet<string>.Enumerator? enumerator = this.Set?.GetEnumerator();
		if (!enumerator.HasValue)
		{
			return Enumerable.Empty<string>().GetEnumerator();
		}
		return enumerator.GetValueOrDefault();
	}

	public bool IsProperSubsetOf(IEnumerable<string> other)
	{
		if (this.IsEmpty && other is ICollection<string> list)
		{
			return list.Count > 0;
		}
		return this.EnsureSet().IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<string> other)
	{
		if (this.IsEmpty)
		{
			return false;
		}
		return this.Set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<string> other)
	{
		if (this.IsEmpty)
		{
			return true;
		}
		return this.Set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<string> other)
	{
		if (this.IsEmpty && other is ICollection<string> list)
		{
			return list.Count == 0;
		}
		return this.EnsureSet().IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<string> other)
	{
		if (this.IsEmpty)
		{
			return false;
		}
		return this.Set.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<string> other)
	{
		if (this.IsEmpty && other is ICollection<string> list)
		{
			return list.Count == 0;
		}
		return this.EnsureSet().SetEquals(other);
	}

	public bool Contains(string item)
	{
		if (this.IsEmpty)
		{
			return false;
		}
		return this.Set.Contains(item);
	}

	public void CopyTo(string[] array, int arrayIndex)
	{
		if (!this.IsEmpty)
		{
			this.Set.CopyTo(array, arrayIndex);
		}
	}

	void ICollection<string>.Add(string item)
	{
		this.AssertNotLocked();
		if (this.EnsureSet().Add(item))
		{
			this.ClearCache();
		}
	}

	public bool Add(string item)
	{
		this.AssertNotLocked();
		if (this.EnsureSet().Add(item))
		{
			this.ClearCache();
			return true;
		}
		return false;
	}

	public void Clear()
	{
		this.AssertNotLocked();
		if (!this.IsEmpty)
		{
			this.Set.Clear();
			this.ClearCache();
		}
	}

	public bool Remove(string item)
	{
		this.AssertNotLocked();
		if (this.IsEmpty)
		{
			return false;
		}
		if (this.Set.Remove(item))
		{
			this.ClearCache();
			return true;
		}
		return false;
	}

	public void ExceptWith(IEnumerable<string> other)
	{
		this.AssertNotLocked();
		if (!this.IsEmpty)
		{
			int wasCount = this.Count;
			this.EnsureSet().ExceptWith(other);
			this.ClearCacheUnlessCount(wasCount);
		}
	}

	public void IntersectWith(IEnumerable<string> other)
	{
		this.AssertNotLocked();
		if (!this.IsEmpty)
		{
			int wasCount = this.Count;
			this.EnsureSet().IntersectWith(other);
			this.ClearCacheUnlessCount(wasCount);
		}
	}

	public void SymmetricExceptWith(IEnumerable<string> other)
	{
		this.AssertNotLocked();
		if (!this.IsEmpty || !(other is ICollection<string> { Count: 0 }))
		{
			this.EnsureSet().SymmetricExceptWith(other);
			this.ClearCache();
		}
	}

	public void UnionWith(IEnumerable<string> other)
	{
		this.AssertNotLocked();
		if (this.IsEmpty)
		{
			if (!(other is ICollection<string> { Count: 0 }))
			{
				this.Set = this.CreateSet(other);
				this.ClearCacheUnlessCount(0);
			}
		}
		else
		{
			int wasCount = this.Count;
			this.Set.UnionWith(other);
			this.ClearCacheUnlessCount(wasCount);
		}
	}

	public IInvariantSet Lock()
	{
		this.IsReadOnly = true;
		return this.GetImmutable();
	}

	public IInvariantSet GetImmutable()
	{
		if (this.CachedImmutableSet == null)
		{
			int? num = this.Set?.Count;
			if (!num.HasValue || num.GetValueOrDefault() <= 0)
			{
				this.CachedImmutableSet = InvariantSet.Empty;
			}
			else
			{
				if (!this.IsReadOnly)
				{
					HashSet<string> hashSet = new HashSet<string>();
					foreach (string item in this.Set)
					{
						hashSet.Add(item);
					}
					HashSet<string> copy = hashSet;
					return new InvariantSet(copy);
				}
				this.CachedImmutableSet = new InvariantSet(this.Set);
			}
		}
		return this.CachedImmutableSet;
	}

	private void AssertNotLocked()
	{
		if (this.IsReadOnly)
		{
			throw new NotSupportedException("This set is locked and doesn't allow further changes.");
		}
	}

	[MemberNotNull("Set")]
	private HashSet<string> EnsureSet()
	{
		return this.Set ?? (this.Set = this.CreateSet());
	}

	private void ClearCache()
	{
		this.CachedImmutableSet = null;
	}

	private void ClearCacheUnlessCount(int count)
	{
		if (count != this.Count)
		{
			this.CachedImmutableSet = null;
		}
	}

	private HashSet<string> CreateSet()
	{
		return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private HashSet<string> CreateSet(IEnumerable<string> values)
	{
		return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
	}
}
