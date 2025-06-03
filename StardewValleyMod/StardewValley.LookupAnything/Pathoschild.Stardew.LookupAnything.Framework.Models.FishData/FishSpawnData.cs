using System.Linq;
using StardewValley.ItemTypeDefinitions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;

internal record FishSpawnData(ParsedItemData FishItem, FishSpawnLocationData[]? Locations, FishSpawnTimeOfDayData[]? TimesOfDay, FishSpawnWeather Weather, int MinFishingLevel, bool IsUnique, bool IsLegendaryFamily)
{
	public bool MatchesLocation(string locationName)
	{
		return this.Locations?.Any((FishSpawnLocationData p) => p.MatchesLocation(locationName)) ?? false;
	}
}
