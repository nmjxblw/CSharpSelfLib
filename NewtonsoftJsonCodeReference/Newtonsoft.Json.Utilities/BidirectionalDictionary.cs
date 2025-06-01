using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Newtonsoft.Json.Utilities;

internal class BidirectionalDictionary<TFirst, TSecond> where TFirst : notnull where TSecond : notnull
{
	private readonly IDictionary<TFirst, TSecond> _firstToSecond;

	private readonly IDictionary<TSecond, TFirst> _secondToFirst;

	private readonly string _duplicateFirstErrorMessage;

	private readonly string _duplicateSecondErrorMessage;

	public BidirectionalDictionary()
		: this((IEqualityComparer<TFirst>)EqualityComparer<TFirst>.Default, (IEqualityComparer<TSecond>)EqualityComparer<TSecond>.Default)
	{
	}

	public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer)
		: this(firstEqualityComparer, secondEqualityComparer, "Duplicate item already exists for '{0}'.", "Duplicate item already exists for '{0}'.")
	{
	}

	public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer, string duplicateFirstErrorMessage, string duplicateSecondErrorMessage)
	{
		this._firstToSecond = new Dictionary<TFirst, TSecond>(firstEqualityComparer);
		this._secondToFirst = new Dictionary<TSecond, TFirst>(secondEqualityComparer);
		this._duplicateFirstErrorMessage = duplicateFirstErrorMessage;
		this._duplicateSecondErrorMessage = duplicateSecondErrorMessage;
	}

	public void Set(TFirst first, TSecond second)
	{
		if (this._firstToSecond.TryGetValue(first, out var value))
		{
			object obj = second;
			if (!value.Equals(obj))
			{
				throw new ArgumentException(this._duplicateFirstErrorMessage.FormatWith(CultureInfo.InvariantCulture, first));
			}
		}
		if (this._secondToFirst.TryGetValue(second, out var value2))
		{
			object obj2 = first;
			if (!value2.Equals(obj2))
			{
				throw new ArgumentException(this._duplicateSecondErrorMessage.FormatWith(CultureInfo.InvariantCulture, second));
			}
		}
		this._firstToSecond.Add(first, second);
		this._secondToFirst.Add(second, first);
	}

	public bool TryGetByFirst(TFirst first, [NotNullWhen(true)] out TSecond? second)
	{
		return this._firstToSecond.TryGetValue(first, out second);
	}

	public bool TryGetBySecond(TSecond second, [NotNullWhen(true)] out TFirst? first)
	{
		return this._secondToFirst.TryGetValue(second, out first);
	}
}
