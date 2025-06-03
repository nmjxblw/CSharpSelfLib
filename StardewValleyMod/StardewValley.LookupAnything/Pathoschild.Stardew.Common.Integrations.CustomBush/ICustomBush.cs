using System.Collections.Generic;
using StardewValley;
using StardewValley.GameData;

namespace Pathoschild.Stardew.Common.Integrations.CustomBush;

public interface ICustomBush
{
	int AgeToProduce { get; }

	int DayToBeginProducing { get; }

	string Description { get; }

	string DisplayName { get; }

	string IndoorTexture { get; }

	List<Season> Seasons { get; }

	List<PlantableRule> PlantableLocationRules { get; }

	string Texture { get; }

	int TextureSpriteRow { get; }
}
