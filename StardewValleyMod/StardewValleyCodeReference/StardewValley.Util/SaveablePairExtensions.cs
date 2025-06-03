using System.Collections.Generic;

namespace StardewValley.Util;

/// <summary>Provides utility extension methods for converting between <see cref="T:StardewValley.Util.SaveablePair`2" /> arrays and dictionaries.</summary>
public static class SaveablePairExtensions
{
	/// <summary>Create a dictionary from an array of <see cref="T:StardewValley.Util.SaveablePair`2" />.</summary>
	/// <param name="pairs">The array of pairs.</param>
	public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this SaveablePair<TKey, TValue>[] pairs)
	{
		Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
		if (pairs != null)
		{
			for (int i = 0; i < pairs.Length; i++)
			{
				SaveablePair<TKey, TValue> pair = pairs[i];
				result[pair.Key] = pair.Value;
			}
		}
		return result;
	}

	/// <summary>Create an array of <see cref="T:StardewValley.Util.SaveablePair`2" /> from a dictionary.</summary>
	/// <param name="data">The data to copy.</param>
	public static SaveablePair<TKey, TValue>[] ToSaveableArray<TKey, TValue>(this IDictionary<TKey, TValue> data)
	{
		return DictionarySaver<TKey, TValue>.ArrayFrom(data);
	}
}
