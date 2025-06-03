using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;

public interface IProducerFrameworkModApi
{
	List<Dictionary<string, object?>> GetRecipes();

	List<Dictionary<string, object?>> GetRecipes(Object machine);
}
