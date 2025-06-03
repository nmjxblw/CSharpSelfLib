using System.Collections.Generic;

namespace SpaceShared.APIs;

public interface ITrainStationApi
{
	void OpenTrainMenu();

	void OpenBoatMenu();

	void RegisterTrainStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName);

	void RegisterBoatStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName);
}
