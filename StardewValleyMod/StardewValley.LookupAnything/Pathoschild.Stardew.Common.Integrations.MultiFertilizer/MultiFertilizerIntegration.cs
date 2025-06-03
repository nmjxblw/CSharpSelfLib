using System.Collections.Generic;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Integrations.MultiFertilizer;

internal class MultiFertilizerIntegration : BaseIntegration
{
	public MultiFertilizerIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("MultiFertilizer", "spacechase0.MultiFertilizer", "1.0.2", modRegistry, monitor)
	{
	}

	public IEnumerable<string> GetAppliedFertilizers(HoeDirt dirt)
	{
		if (!this.IsLoaded)
		{
			yield break;
		}
		if (CommonHelper.IsItemId(((NetFieldBase<string, NetString>)(object)dirt.fertilizer).Value, allowZero: false))
		{
			yield return ((NetFieldBase<string, NetString>)(object)dirt.fertilizer).Value;
		}
		string[] array = new string[3] { "FertilizerLevel", "SpeedGrowLevel", "WaterRetainLevel" };
		string rawValue = default(string);
		foreach (string key in array)
		{
			if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)((TerrainFeature)dirt).modData).TryGetValue("spacechase0.MultiFertilizer/" + key, ref rawValue) && int.TryParse(rawValue, out var level))
			{
				string fertilizer = $"{key}:{level}" switch
				{
					"FertilizerLevel:1" => "(O)368", 
					"FertilizerLevel:2" => "(O)369", 
					"FertilizerLevel:3" => "(O)919", 
					"SpeedGrowLevel:1" => "(O)465", 
					"SpeedGrowLevel:2" => "(O)466", 
					"SpeedGrowLevel:3" => "(O)918", 
					"WaterRetainLevel:1" => "(O)370", 
					"WaterRetainLevel:2" => "(O)371", 
					"WaterRetainLevel:3" => "(O)920", 
					_ => null, 
				};
				if (fertilizer != null)
				{
					yield return fertilizer;
				}
			}
		}
	}
}
