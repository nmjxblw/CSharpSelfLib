using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;

namespace SpaceShared.APIs;

public interface ICookingSkillApi
{
	bool ModifyCookedItem(CraftingRecipe recipe, Item craftedItem, List<Chest> additionalIngredients);
}
