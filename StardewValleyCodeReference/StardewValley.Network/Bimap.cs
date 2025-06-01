using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network;

public class Bimap<L, R> : IEnumerable<KeyValuePair<L, R>>, IEnumerable
{
	private Dictionary<L, R> leftToRight = new Dictionary<L, R>();

	private Dictionary<R, L> rightToLeft = new Dictionary<R, L>();

	public R this[L l]
	{
		get
		{
			return this.leftToRight[l];
		}
		set
		{
			if (this.leftToRight.TryGetValue(l, out var rightKey))
			{
				this.rightToLeft.Remove(rightKey);
			}
			if (this.rightToLeft.TryGetValue(value, out var leftKey))
			{
				this.leftToRight.Remove(leftKey);
			}
			this.leftToRight[l] = value;
			this.rightToLeft[value] = l;
		}
	}

	public L this[R r]
	{
		get
		{
			return this.rightToLeft[r];
		}
		set
		{
			if (this.rightToLeft.TryGetValue(r, out var leftKey))
			{
				this.leftToRight.Remove(leftKey);
			}
			if (this.leftToRight.TryGetValue(value, out var rightKey))
			{
				this.rightToLeft.Remove(rightKey);
			}
			this.rightToLeft[r] = value;
			this.leftToRight[value] = r;
		}
	}

	public ICollection<L> LeftValues => this.leftToRight.Keys;

	public ICollection<R> RightValues => this.rightToLeft.Keys;

	public int Count => this.rightToLeft.Count;

	public void Clear()
	{
		this.leftToRight.Clear();
		this.rightToLeft.Clear();
	}

	public void Add(L l, R r)
	{
		if (this.leftToRight.ContainsKey(l) || this.rightToLeft.ContainsKey(r))
		{
			throw new ArgumentException();
		}
		this.leftToRight.Add(l, r);
		this.rightToLeft.Add(r, l);
	}

	public bool ContainsLeft(L l)
	{
		return this.leftToRight.ContainsKey(l);
	}

	public bool ContainsRight(R r)
	{
		return this.rightToLeft.ContainsKey(r);
	}

	public void RemoveLeft(L l)
	{
		if (this.leftToRight.TryGetValue(l, out var rightKey))
		{
			this.rightToLeft.Remove(rightKey);
		}
		this.leftToRight.Remove(l);
	}

	public void RemoveRight(R r)
	{
		if (this.rightToLeft.TryGetValue(r, out var leftKey))
		{
			this.leftToRight.Remove(leftKey);
		}
		this.rightToLeft.Remove(r);
	}

	public L GetLeft(R r)
	{
		return this.rightToLeft[r];
	}

	public R GetRight(L l)
	{
		return this.leftToRight[l];
	}

	public IEnumerator<KeyValuePair<L, R>> GetEnumerator()
	{
		return this.leftToRight.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}
}
