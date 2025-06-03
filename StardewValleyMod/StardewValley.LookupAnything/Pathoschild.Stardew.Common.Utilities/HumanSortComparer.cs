using System;
using System.Collections.Generic;

namespace Pathoschild.Stardew.Common.Utilities;

internal class HumanSortComparer : IComparer<string?>
{
	private readonly IComparer<string?> AlphaComparer;

	public static readonly HumanSortComparer DefaultIgnoreCase = new HumanSortComparer(ignoreCase: true);

	public HumanSortComparer(bool ignoreCase = false)
	{
		this.AlphaComparer = (ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
	}

	public int Compare(string? a, string? b)
	{
		if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
		{
			return this.AlphaComparer.Compare(a, b);
		}
		int indexA = 0;
		int indexB = 0;
		int result;
		do
		{
			this.GetNextPart(a, ref indexA, out string rawA, out long? numericA);
			this.GetNextPart(b, ref indexB, out string rawB, out long? numericB);
			bool isNumeric = numericA.HasValue && numericB.HasValue;
			if (rawA == null && rawB == null)
			{
				return 0;
			}
			if (rawA == null)
			{
				return -1;
			}
			if (rawB == null)
			{
				return 1;
			}
			if (isNumeric)
			{
				for (int i = 0; i < rawA.Length && i < rawB.Length; i++)
				{
					bool zeroA = rawA[i] == '0';
					bool zeroB = rawB[i] == '0';
					if (!(zeroA && zeroB))
					{
						if (zeroA)
						{
							return -1;
						}
						if (!zeroB)
						{
							break;
						}
						return 1;
					}
				}
			}
			result = (isNumeric ? numericA.Value.CompareTo(numericB.Value) : this.AlphaComparer.Compare(rawA, rawB));
			if (result < 0)
			{
				return -1;
			}
		}
		while (result <= 0);
		return 1;
	}

	private void GetNextPart(string str, ref int position, out string? raw, out long? numeric)
	{
		if (position >= str.Length)
		{
			raw = null;
			numeric = null;
			return;
		}
		int start = position;
		bool isNumeric = char.IsNumber(str[start]);
		position++;
		while (position < str.Length && char.IsNumber(str[position]) == isNumeric)
		{
			position++;
		}
		raw = str.Substring(start, position - start);
		numeric = ((isNumeric && long.TryParse(raw, out var parsedNumeric)) ? new long?(parsedNumeric) : ((long?)null));
	}
}
