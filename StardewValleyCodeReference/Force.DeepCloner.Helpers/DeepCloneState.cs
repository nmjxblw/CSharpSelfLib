using System;
using System.Runtime.CompilerServices;

namespace Force.DeepCloner.Helpers;

internal class DeepCloneState
{
	private class MiniDictionary
	{
		private struct Entry
		{
			public int HashCode;

			public int Next;

			public object Key;

			public object Value;
		}

		private int[] _buckets;

		private Entry[] _entries;

		private int _count;

		private static readonly int[] _primes = new int[72]
		{
			3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
			89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
			631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
			4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
			25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
			156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
			968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
			5999471, 7199369
		};

		public MiniDictionary()
			: this(5)
		{
		}

		public MiniDictionary(int capacity)
		{
			if (capacity > 0)
			{
				this.Initialize(capacity);
			}
		}

		public object FindEntry(object key)
		{
			if (this._buckets != null)
			{
				int hashCode = RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF;
				Entry[] entries1 = this._entries;
				for (int i = this._buckets[hashCode % this._buckets.Length]; i >= 0; i = entries1[i].Next)
				{
					if (entries1[i].HashCode == hashCode && entries1[i].Key == key)
					{
						return entries1[i].Value;
					}
				}
			}
			return null;
		}

		private static int GetPrime(int min)
		{
			for (int i = 0; i < MiniDictionary._primes.Length; i++)
			{
				int prime = MiniDictionary._primes[i];
				if (prime >= min)
				{
					return prime;
				}
			}
			for (int j = min | 1; j < int.MaxValue; j += 2)
			{
				if (MiniDictionary.IsPrime(j) && (j - 1) % 101 != 0)
				{
					return j;
				}
			}
			return min;
		}

		private static bool IsPrime(int candidate)
		{
			if ((candidate & 1) != 0)
			{
				int limit = (int)Math.Sqrt(candidate);
				for (int divisor = 3; divisor <= limit; divisor += 2)
				{
					if (candidate % divisor == 0)
					{
						return false;
					}
				}
				return true;
			}
			return candidate == 2;
		}

		private static int ExpandPrime(int oldSize)
		{
			int newSize = 2 * oldSize;
			if ((uint)newSize > 2146435069u && 2146435069 > oldSize)
			{
				return 2146435069;
			}
			return MiniDictionary.GetPrime(newSize);
		}

		private void Initialize(int size)
		{
			this._buckets = new int[size];
			for (int i = 0; i < this._buckets.Length; i++)
			{
				this._buckets[i] = -1;
			}
			this._entries = new Entry[size];
		}

		public void Insert(object key, object value)
		{
			if (this._buckets == null)
			{
				this.Initialize(0);
			}
			int hashCode = RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF;
			int targetBucket = hashCode % this._buckets.Length;
			Entry[] entries1 = this._entries;
			if (this._count == entries1.Length)
			{
				this.Resize();
				entries1 = this._entries;
				targetBucket = hashCode % this._buckets.Length;
			}
			int index = this._count;
			this._count++;
			entries1[index].HashCode = hashCode;
			entries1[index].Next = this._buckets[targetBucket];
			entries1[index].Key = key;
			entries1[index].Value = value;
			this._buckets[targetBucket] = index;
		}

		private void Resize()
		{
			this.Resize(MiniDictionary.ExpandPrime(this._count));
		}

		private void Resize(int newSize)
		{
			int[] newBuckets = new int[newSize];
			for (int i = 0; i < newBuckets.Length; i++)
			{
				newBuckets[i] = -1;
			}
			Entry[] newEntries = new Entry[newSize];
			Array.Copy(this._entries, 0, newEntries, 0, this._count);
			for (int j = 0; j < this._count; j++)
			{
				if (newEntries[j].HashCode >= 0)
				{
					int bucket = newEntries[j].HashCode % newSize;
					newEntries[j].Next = newBuckets[bucket];
					newBuckets[bucket] = j;
				}
			}
			this._buckets = newBuckets;
			this._entries = newEntries;
		}
	}

	private MiniDictionary _loops;

	private readonly object[] _baseFromTo = new object[6];

	private int _idx;

	public object GetKnownRef(object from)
	{
		object[] baseFromTo = this._baseFromTo;
		if (from == baseFromTo[0])
		{
			return baseFromTo[3];
		}
		if (from == baseFromTo[1])
		{
			return baseFromTo[4];
		}
		if (from == baseFromTo[2])
		{
			return baseFromTo[5];
		}
		return this._loops?.FindEntry(from);
	}

	public void AddKnownRef(object from, object to)
	{
		if (this._idx < 3)
		{
			this._baseFromTo[this._idx] = from;
			this._baseFromTo[this._idx + 3] = to;
			this._idx++;
			return;
		}
		if (this._loops == null)
		{
			this._loops = new MiniDictionary();
		}
		this._loops.Insert(from, to);
	}
}
