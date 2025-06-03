using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.BetterGameMenu;

public interface IBetterGameMenuApi
{
	IClickableMenu? GetCurrentPage(IClickableMenu menu);
}
