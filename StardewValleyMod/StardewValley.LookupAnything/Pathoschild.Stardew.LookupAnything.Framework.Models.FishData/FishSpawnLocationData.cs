using System;
using System.Collections.Generic;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;

internal record FishSpawnLocationData(string LocationId, string? Area, HashSet<string> Seasons)
{
	internal FishSpawnLocationData(string locationId, int? area, string[] seasons)
		: this(locationId, (area >= 0) ? area.ToString() : null, seasons)
	{
	}

	internal FishSpawnLocationData(string locationId, string? area, string[] seasons)
		: this(locationId, area, new HashSet<string>(seasons, StringComparer.OrdinalIgnoreCase))
	{
	}

	public bool MatchesLocation(string locationId)
	{
		if (this.LocationId == "UndergroundMine")
		{
			if (!string.IsNullOrWhiteSpace(this.Area))
			{
				return locationId == this.LocationId + this.Area;
			}
			return MineShaft.IsGeneratedLevel(locationId);
		}
		return locationId == this.LocationId;
	}
}
