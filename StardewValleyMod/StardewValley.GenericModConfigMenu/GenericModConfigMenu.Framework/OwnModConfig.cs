using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace GenericModConfigMenu.Framework;

internal class OwnModConfig
{
	public KeybindList OpenMenuKey = new KeybindList((SButton)0);

	public int ScrollSpeed { get; set; } = 120;
}
