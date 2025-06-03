using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.PelicanFiber;

internal class PelicanFiberIntegration : BaseIntegration
{
	private readonly string MenuTypeName = "PelicanFiber.Framework.ConstructionMenu";

	private readonly IReflectionHelper Reflection;

	public PelicanFiberIntegration(IModRegistry modRegistry, IReflectionHelper reflection, IMonitor monitor)
		: base("Pelican Fiber", "jwdred.PelicanFiber", "3.1.1-unofficial.7.1-pathoschild", modRegistry, monitor)
	{
		this.Reflection = reflection;
	}

	public bool IsBuildMenuOpen()
	{
		this.AssertLoaded();
		return ((object)Game1.activeClickableMenu)?.GetType().FullName == this.MenuTypeName;
	}

	public BlueprintEntry? GetBuildMenuBlueprint()
	{
		this.AssertLoaded();
		if (!this.IsBuildMenuOpen())
		{
			return null;
		}
		return this.Reflection.GetProperty<BlueprintEntry>((object)Game1.activeClickableMenu, "Blueprint", true).GetValue();
	}
}
