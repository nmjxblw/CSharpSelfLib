using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace GenericModConfigMenu.Framework;

internal static class AssetManager
{
	private const string BasePath = "Mods/GenericModConfigMenu";

	internal static string ConfigButton { get; } = PathUtilities.NormalizeAssetName("Mods/GenericModConfigMenu/ConfigButton");

	internal static string KeyboardButton { get; } = PathUtilities.NormalizeAssetName("Mods/GenericModConfigMenu/KeyboardButton");

	internal static void Apply(AssetRequestedEventArgs e)
	{
		if (e.Name.IsEquivalentTo(AssetManager.ConfigButton, false))
		{
			e.LoadFromModFile<Texture2D>("assets/config-button.png", (AssetLoadPriority)int.MaxValue);
		}
		else if (e.Name.IsEquivalentTo(AssetManager.KeyboardButton, false))
		{
			e.LoadFromModFile<Texture2D>("assets/keybinds-button.png", (AssetLoadPriority)int.MaxValue);
		}
	}
}
