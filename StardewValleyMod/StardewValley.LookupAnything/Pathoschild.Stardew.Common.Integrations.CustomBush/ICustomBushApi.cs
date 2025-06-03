using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Integrations.CustomBush;

public interface ICustomBushApi
{
	bool TryGetCustomBush(Bush bush, out ICustomBush? customBush);

	bool TryGetCustomBush(Bush bush, out ICustomBush? customBush, [NotNullWhen(true)] out string? id);

	bool TryGetDrops(string id, [NotNullWhen(true)] out IList<ICustomBushDrop>? drops);
}
