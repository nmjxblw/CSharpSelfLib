using System;
using System.Collections.Generic;

namespace SpaceShared;

internal static class CommonExtensions
{
	public static void Shuffle<T>(this List<T> list, Random r = null)
	{
		if (r == null)
		{
			r = new Random();
		}
		for (int i = 0; i < list.Count; i++)
		{
			int ri = r.Next(list.Count);
			T tmp = list[i];
			list[i] = list[ri];
			list[ri] = tmp;
		}
	}

	public static int GetDeterministicHashCode(this string str)
	{
		int hash1 = 352654597;
		int hash2 = hash1;
		for (int i = 0; i < str.Length; i += 2)
		{
			hash1 = ((hash1 << 5) + hash1) ^ str[i];
			if (i == str.Length - 1)
			{
				break;
			}
			hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
		}
		return hash1 + hash2 * 1566083941;
	}

	public static bool TryGetIndex<T>(this T[] array, int index, out T value)
	{
		if (array == null || index < 0 || index >= array.Length)
		{
			value = default(T);
			return false;
		}
		value = array[index];
		return true;
	}

	public static T GetOrDefault<T>(this T[] array, int index, T defaultValue = default(T))
	{
		if (!array.TryGetIndex(index, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public static TParsed GetOrDefault<TRaw, TParsed>(this TRaw[] array, int index, Func<TRaw, TParsed> tryParse, TParsed defaultValue = default(TParsed))
	{
		if (!array.TryGetIndex(index, out var value))
		{
			return defaultValue;
		}
		try
		{
			return tryParse(value);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public static TParsed GetOrDefault<TKey, TValue, TParsed>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue, TParsed> tryParse, TParsed defaultValue = default(TParsed))
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		try
		{
			return tryParse(value);
		}
		catch
		{
			return defaultValue;
		}
	}
}
