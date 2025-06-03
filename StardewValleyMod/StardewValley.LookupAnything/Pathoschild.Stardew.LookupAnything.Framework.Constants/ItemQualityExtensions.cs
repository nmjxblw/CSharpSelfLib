using System;

namespace Pathoschild.Stardew.LookupAnything.Framework.Constants;

internal static class ItemQualityExtensions
{
	public static string GetName(this ItemQuality current)
	{
		return current.ToString().ToLower();
	}

	public static ItemQuality GetNext(this ItemQuality current)
	{
		return current switch
		{
			ItemQuality.Normal => ItemQuality.Silver, 
			ItemQuality.Silver => ItemQuality.Gold, 
			ItemQuality.Gold => ItemQuality.Iridium, 
			ItemQuality.Iridium => ItemQuality.Iridium, 
			_ => throw new NotSupportedException($"Unknown quality '{current}'."), 
		};
	}
}
