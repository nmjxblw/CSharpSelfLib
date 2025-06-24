using StardewModdingAPI;

namespace AdvancedMeleeFramework;

public class ModConfig
{
	public bool EnableMod { get; set; } = true;

	public SButton ReloadButton { get; set; } = (SButton)96;

	public bool RequireModKey { get; set; }

	public SButton ModKey { get; set; } = (SButton)160;
}
