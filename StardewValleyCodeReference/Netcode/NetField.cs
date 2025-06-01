using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netcode;

public abstract class NetField<T, TSelf> : NetFieldBase<T, TSelf>, IEnumerable<T>, IEnumerable where TSelf : NetField<T, TSelf>
{
	private bool xmlInitialized;

	public NetField()
	{
	}

	public NetField(T value)
		: base(value)
	{
	}

	public IEnumerator<T> GetEnumerator()
	{
		return Enumerable.Repeat(base.Get(), 1).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public void Add(T value)
	{
		if (this.xmlInitialized || base.Parent != null)
		{
			throw new InvalidOperationException(base.GetType().Name + " already has value " + this.ToString());
		}
		base.cleanSet(value);
		this.xmlInitialized = true;
	}
}
