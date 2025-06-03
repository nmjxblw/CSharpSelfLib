using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceShared;

public static class WeightedExtensions
{
	public static T Choose<T>(this Weighted<T>[] choices, Random r = null)
	{
		if (choices.Length == 0)
		{
			return default(T);
		}
		if (choices.Length == 1)
		{
			return choices[0].Value;
		}
		if (r == null)
		{
			r = new Random();
		}
		double sum = choices.Sum((Weighted<T> weighted) => weighted.Weight);
		double chosenWeight = r.NextDouble() * sum;
		foreach (Weighted<T> choice in choices)
		{
			if (chosenWeight < choice.Weight)
			{
				return choice.Value;
			}
			chosenWeight -= choice.Weight;
		}
		throw new Exception("This should never happen");
	}

	public static T Choose<T>(this List<Weighted<T>> choices, Random r = null)
	{
		return choices.ToArray().Choose(r);
	}
}
