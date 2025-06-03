namespace Pathoschild.Stardew.Common.Integrations.TrainStation;

public interface ITrainStationStopModel
{
	string Id { get; }

	string DisplayName { get; }

	string TargetMapName { get; }

	int TargetX { get; }

	int TargetY { get; }

	int FacingDirectionAfterWarp { get; }

	int Cost { get; }

	bool IsBoat { get; }

	string[] Conditions { get; }
}
