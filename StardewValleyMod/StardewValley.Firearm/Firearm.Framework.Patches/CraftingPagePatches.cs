using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Menus;

namespace Firearm.Framework.Patches;

public class CraftingPagePatches
{
	internal static void LayoutRecipes(CraftingPage __instance)
	{
		Dictionary<ClickableTextureComponent, CraftingRecipe> instancePagesOfCraftingRecipe = __instance.pagesOfCraftingRecipes[0];
		foreach (var (clickableTextureComponent, craftingRecipe) in instancePagesOfCraftingRecipe)
		{
			if (new string[2] { "Firearm_AK47", "Firearm_M16" }.Contains(craftingRecipe.name))
			{
				clickableTextureComponent.sourceRect.Width += 16;
				clickableTextureComponent.sourceRect.Height += 16;
			}
		}
	}
}
