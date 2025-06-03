using System;
using StardewValley.Extensions;

namespace StardewValley;

/// <summary>An implementation of <see cref="T:StardewValley.SerializableDictionary`2" /> specialized for storing <see cref="T:StardewValley.Stats" /> values.</summary>
/// <typeparam name="TValue">The numeric stat value type. This must be <see cref="T:System.Int32" />, <see cref="T:System.Int64" />, or <see cref="T:System.UInt32" />.</typeparam>
public class StatsDictionary<TValue> : SerializableDictionaryWithCaseInsensitiveKeys<TValue>
{
	/// <inheritdoc />
	protected override void AddDuringDeserialization(string key, TValue value)
	{
		if (!base.TryGetValue(key, out var oldValue))
		{
			base.AddDuringDeserialization(key, value);
			return;
		}
		long valueLong = Convert.ToInt64(value);
		long oldValueLong = Convert.ToInt64(oldValue);
		if (key.EqualsIgnoreCase("averageBedtime"))
		{
			if (oldValueLong == 0L)
			{
				base[key] = value;
			}
		}
		else
		{
			base[key] = (TValue)Convert.ChangeType(oldValueLong + valueLong, typeof(TValue));
		}
	}
}
