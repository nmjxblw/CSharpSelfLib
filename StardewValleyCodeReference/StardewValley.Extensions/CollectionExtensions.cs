using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on .NET collection types.</summary>
public static class CollectionExtensions
{
	/// <summary>Remove all elements that match a condition.</summary>
	/// <typeparam name="TKey">The dictionary key type.</typeparam>
	/// <typeparam name="TValue">The dictionary value type.</typeparam>
	/// <param name="dictionary">The dictionary to update.</param>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of entries removed.</returns>
	public static int RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> match)
	{
		if (dictionary.Count == 0)
		{
			return 0;
		}
		int removed = 0;
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			if (match(pair))
			{
				dictionary.Remove(pair.Key);
				removed++;
			}
		}
		return removed;
	}

	/// <summary>Add multiple key/value pairs to the dictionary, skipping keys which already exist in the dictionary.</summary>
	/// <typeparam name="TKey">The dictionary key type.</typeparam>
	/// <typeparam name="TValue">The dictionary value type.</typeparam>
	/// <param name="dict">The dictionary to update.</param>
	/// <param name="values">The key/value pairs to add.</param>
	/// <returns>Returns the number of pairs added to the dictionary.</returns>
	public static int TryAddMany<TKey, TValue>(this IDictionary<TKey, TValue> dict, Dictionary<TKey, TValue> values)
	{
		if (values == null)
		{
			return 0;
		}
		int added = 0;
		foreach (KeyValuePair<TKey, TValue> pair in values)
		{
			if (dict.TryAdd(pair.Key, pair.Value))
			{
				added++;
			}
		}
		return added;
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <typeparam name="T">The set item type.</typeparam>
	/// <param name="list">The list to update.</param>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of values removed from the list.</returns>
	public static int RemoveWhere<T>(this IList<T> list, Predicate<T> match)
	{
		if (list is List<T> concreteList)
		{
			return concreteList.RemoveAll(match);
		}
		int count = 0;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (match(list[i]))
			{
				list.RemoveAt(i);
				count++;
			}
		}
		return count;
	}

	/// <summary>Add or remove value to the set.</summary>
	/// <typeparam name="T">The set item type.</typeparam>
	/// <param name="set">The set to update.</param>
	/// <param name="value">The value to add or remove.</param>
	/// <param name="add">Whether to add the value; else it's removed.</param>
	public static void Toggle<T>(this ISet<T> set, T value, bool add)
	{
		if (add)
		{
			set.Add(value);
		}
		else
		{
			set.Remove(value);
		}
	}

	/// <summary>Add a list of values to the set.</summary>
	/// <typeparam name="T">The set item type.</typeparam>
	/// <param name="set">The set to update.</param>
	/// <param name="values">The values to add to the set.</param>
	/// <returns>Returns the number of values added to the set.</returns>
	public static int AddRange<T>(this ISet<T> set, IEnumerable<T> values)
	{
		if (values == null)
		{
			return 0;
		}
		int added = 0;
		foreach (T value in values)
		{
			if (set.Add(value))
			{
				added++;
			}
		}
		return added;
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <typeparam name="T">The set item type.</typeparam>
	/// <param name="set">The set to update.</param>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of values removed from the set.</returns>
	public static int RemoveWhere<T>(this ISet<T> set, Predicate<T> match)
	{
		if (!(set is HashSet<T> hashSet))
		{
			if (set is NetHashSet<T> netHashSet)
			{
				return netHashSet.RemoveWhere(match);
			}
			List<T> removed = null;
			foreach (T value in set)
			{
				if (match(value))
				{
					if (removed == null)
					{
						removed = new List<T>();
					}
					removed.Add(value);
				}
			}
			if (removed != null)
			{
				foreach (T value2 in removed)
				{
					set.Remove(value2);
				}
				return removed.Count;
			}
			return 0;
		}
		return hashSet.RemoveWhere(match);
	}
}
