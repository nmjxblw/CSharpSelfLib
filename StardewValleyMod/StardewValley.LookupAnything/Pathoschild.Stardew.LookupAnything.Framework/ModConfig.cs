using System.Runtime.Serialization;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal class ModConfig
{
	public ModConfigKeys Controls { get; set; } = new ModConfigKeys();

	public bool ShowUnknownGiftTastes { get; set; } = true;

	public bool ShowUncaughtFishSpawnRules { get; set; } = true;

	public bool ShowUnknownRecipes { get; set; } = true;

	public bool ShowPuzzleSolutions { get; set; } = true;

	public bool HighlightUnrevealedGiftTastes { get; set; }

	public ModGiftTasteConfig ShowGiftTastes { get; set; } = new ModGiftTasteConfig();

	public ModCollapseLargeFieldsConfig CollapseLargeFields { get; set; } = new ModCollapseLargeFieldsConfig();

	public bool ShowUnownedGifts { get; set; } = true;

	public bool HideOnKeyUp { get; set; }

	public bool EnableTargetRedirection { get; set; } = true;

	public bool EnableTileLookups { get; set; }

	public bool ForceFullScreen { get; set; }

	public int ScrollAmount { get; set; } = 160;

	public bool ShowDataMiningFields { get; set; }

	public bool ShowInvalidRecipes { get; set; }

	[OnDeserialized]
	public void OnDeserialized(StreamingContext context)
	{
		if (this.Controls == null)
		{
			ModConfigKeys modConfigKeys = (this.Controls = new ModConfigKeys());
		}
		if (this.ShowGiftTastes == null)
		{
			ModGiftTasteConfig modGiftTasteConfig = (this.ShowGiftTastes = new ModGiftTasteConfig());
		}
		if (this.CollapseLargeFields == null)
		{
			ModCollapseLargeFieldsConfig modCollapseLargeFieldsConfig = (this.CollapseLargeFields = new ModCollapseLargeFieldsConfig());
		}
	}
}
