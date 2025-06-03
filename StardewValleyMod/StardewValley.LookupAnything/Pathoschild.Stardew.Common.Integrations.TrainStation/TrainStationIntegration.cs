using System.Collections.Generic;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.TrainStation;

internal class TrainStationIntegration : BaseIntegration<ITrainStationApi>
{
	public TrainStationIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Train Station", "Cherry.TrainStation", "2.2.0", modRegistry, monitor)
	{
	}

	public IEnumerable<ITrainStationStopModel> GetAvailableStops(bool isBoat)
	{
		this.AssertLoaded();
		return base.ModApi.GetAvailableStops(isBoat);
	}
}
