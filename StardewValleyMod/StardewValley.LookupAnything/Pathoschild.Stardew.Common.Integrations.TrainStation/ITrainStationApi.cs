using System.Collections.Generic;

namespace Pathoschild.Stardew.Common.Integrations.TrainStation;

public interface ITrainStationApi
{
	IEnumerable<ITrainStationStopModel> GetAvailableStops(bool isBoat);
}
