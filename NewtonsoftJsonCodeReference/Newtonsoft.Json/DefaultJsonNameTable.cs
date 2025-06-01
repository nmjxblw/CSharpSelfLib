using System;
using System.Threading;

namespace Newtonsoft.Json;

/// <summary>
/// The default JSON name table implementation.
/// </summary>
public class DefaultJsonNameTable : JsonNameTable
{
	private class Entry
	{
		internal readonly string Value;

		internal readonly int HashCode;

		internal Entry Next;

		internal Entry(string value, int hashCode, Entry next)
		{
			this.Value = value;
			this.HashCode = hashCode;
			this.Next = next;
		}
	}

	private static readonly int HashCodeRandomizer;

	private int _count;

	private Entry[] _entries;

	private int _mask = 31;

	static DefaultJsonNameTable()
	{
		DefaultJsonNameTable.HashCodeRandomizer = Environment.TickCount;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.DefaultJsonNameTable" /> class.
	/// </summary>
	public DefaultJsonNameTable()
	{
		this._entries = new Entry[this._mask + 1];
	}

	/// <summary>
	/// Gets a string containing the same characters as the specified range of characters in the given array.
	/// </summary>
	/// <param name="key">The character array containing the name to find.</param>
	/// <param name="start">The zero-based index into the array specifying the first character of the name.</param>
	/// <param name="length">The number of characters in the name.</param>
	/// <returns>A string containing the same characters as the specified range of characters in the given array.</returns>
	public override string? Get(char[] key, int start, int length)
	{
		if (length == 0)
		{
			return string.Empty;
		}
		int num = length + DefaultJsonNameTable.HashCodeRandomizer;
		num += (num << 7) ^ key[start];
		int num2 = start + length;
		for (int i = start + 1; i < num2; i++)
		{
			num += (num << 7) ^ key[i];
		}
		num -= num >> 17;
		num -= num >> 11;
		num -= num >> 5;
		int num3 = Volatile.Read(ref this._mask);
		int num4 = num & num3;
		for (Entry entry = this._entries[num4]; entry != null; entry = entry.Next)
		{
			if (entry.HashCode == num && DefaultJsonNameTable.TextEquals(entry.Value, key, start, length))
			{
				return entry.Value;
			}
		}
		return null;
	}

	/// <summary>
	/// Adds the specified string into name table.
	/// </summary>
	/// <param name="key">The string to add.</param>
	/// <remarks>This method is not thread-safe.</remarks>
	/// <returns>The resolved string.</returns>
	public string Add(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		int length = key.Length;
		if (length == 0)
		{
			return string.Empty;
		}
		int num = length + DefaultJsonNameTable.HashCodeRandomizer;
		for (int i = 0; i < key.Length; i++)
		{
			num += (num << 7) ^ key[i];
		}
		num -= num >> 17;
		num -= num >> 11;
		num -= num >> 5;
		for (Entry entry = this._entries[num & this._mask]; entry != null; entry = entry.Next)
		{
			if (entry.HashCode == num && entry.Value.Equals(key, StringComparison.Ordinal))
			{
				return entry.Value;
			}
		}
		return this.AddEntry(key, num);
	}

	private string AddEntry(string str, int hashCode)
	{
		int num = hashCode & this._mask;
		Entry entry = new Entry(str, hashCode, this._entries[num]);
		this._entries[num] = entry;
		if (this._count++ == this._mask)
		{
			this.Grow();
		}
		return entry.Value;
	}

	private void Grow()
	{
		Entry[] entries = this._entries;
		int num = this._mask * 2 + 1;
		Entry[] array = new Entry[num + 1];
		for (int i = 0; i < entries.Length; i++)
		{
			Entry entry = entries[i];
			while (entry != null)
			{
				int num2 = entry.HashCode & num;
				Entry next = entry.Next;
				entry.Next = array[num2];
				array[num2] = entry;
				entry = next;
			}
		}
		this._entries = array;
		Volatile.Write(ref this._mask, num);
	}

	private static bool TextEquals(string str1, char[] str2, int str2Start, int str2Length)
	{
		if (str1.Length != str2Length)
		{
			return false;
		}
		for (int i = 0; i < str1.Length; i++)
		{
			if (str1[i] != str2[str2Start + i])
			{
				return false;
			}
		}
		return true;
	}
}
