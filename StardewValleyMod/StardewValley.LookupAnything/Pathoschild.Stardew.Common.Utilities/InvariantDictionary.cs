using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Common.Utilities;

internal class InvariantDictionary<TValue> : Dictionary<string, TValue>
{
	public InvariantDictionary()
		: base((IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase)
	{
	}

	public InvariantDictionary(IDictionary<string, TValue> values)
		: base(values, (IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase)
	{
	}

	public InvariantDictionary(IEnumerable<KeyValuePair<string, TValue>> values)
		: base((IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase)
	{
		foreach (KeyValuePair<string, TValue> entry in values)
		{
			base.Add(entry.Key, entry.Value);
		}
	}

	public InvariantDictionary<TValue> Clone(Func<TValue, TValue>? cloneValue = null)
	{
		if (cloneValue == null)
		{
			return new InvariantDictionary<TValue>(this);
		}
		return new InvariantDictionary<TValue>(this.ToDictionary<KeyValuePair<string, TValue>, string, TValue>((KeyValuePair<string, TValue> p) => p.Key, (KeyValuePair<string, TValue> p) => cloneValue(p.Value)));
	}
}
