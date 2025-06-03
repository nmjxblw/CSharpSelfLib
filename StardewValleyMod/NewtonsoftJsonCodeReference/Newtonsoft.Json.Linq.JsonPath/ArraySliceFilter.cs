using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class ArraySliceFilter : PathFilter
{
	public int? Start { get; set; }

	public int? End { get; set; }

	public int? Step { get; set; }

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
	{
		if (this.Step == 0)
		{
			throw new JsonException("Step cannot be zero.");
		}
		foreach (JToken item in current)
		{
			if (item is JArray a)
			{
				int stepCount = this.Step ?? 1;
				int num = this.Start ?? ((stepCount <= 0) ? (a.Count - 1) : 0);
				int stopIndex = this.End ?? ((stepCount > 0) ? a.Count : (-1));
				if (this.Start < 0)
				{
					num = a.Count + num;
				}
				if (this.End < 0)
				{
					stopIndex = a.Count + stopIndex;
				}
				num = Math.Max(num, (stepCount <= 0) ? int.MinValue : 0);
				num = Math.Min(num, (stepCount > 0) ? a.Count : (a.Count - 1));
				stopIndex = Math.Max(stopIndex, -1);
				stopIndex = Math.Min(stopIndex, a.Count);
				bool positiveStep = stepCount > 0;
				if (this.IsValid(num, stopIndex, positiveStep))
				{
					for (int i = num; this.IsValid(i, stopIndex, positiveStep); i += stepCount)
					{
						yield return a[i];
					}
				}
				else if (settings?.ErrorWhenNoMatch ?? false)
				{
					throw new JsonException("Array slice of {0} to {1} returned no results.".FormatWith(CultureInfo.InvariantCulture, this.Start.HasValue ? this.Start.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "*", this.End.HasValue ? this.End.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "*"));
				}
			}
			else if (settings?.ErrorWhenNoMatch ?? false)
			{
				throw new JsonException("Array slice is not valid on {0}.".FormatWith(CultureInfo.InvariantCulture, item.GetType().Name));
			}
		}
	}

	private bool IsValid(int index, int stopIndex, bool positiveStep)
	{
		if (positiveStep)
		{
			return index < stopIndex;
		}
		return index > stopIndex;
	}
}
