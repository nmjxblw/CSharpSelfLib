using BirbCore.Attributes;
using StardewModdingAPI;

namespace BirbCore;

internal class ModEntry : Mod
{
	internal static ModEntry Instance;

	public override void Entry(IModHelper helper)
	{
		Instance = this;
		Parser.InitEvents();
		Parser.ParseAll((IMod)(object)this);
		Log.Trace("=== Running Priority 0 events ===");
	}
}
