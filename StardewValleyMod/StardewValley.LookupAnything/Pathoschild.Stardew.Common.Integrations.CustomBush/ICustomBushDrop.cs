using StardewValley;
using StardewValley.GameData;

namespace Pathoschild.Stardew.Common.Integrations.CustomBush;

public interface ICustomBushDrop : ISpawnItemData
{
	Season? Season { get; }

	float Chance { get; }

	string? Condition { get; }

	string? Id { get; }
}
