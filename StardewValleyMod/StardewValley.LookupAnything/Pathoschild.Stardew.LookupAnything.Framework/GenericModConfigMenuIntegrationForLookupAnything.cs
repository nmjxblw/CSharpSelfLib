using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal class GenericModConfigMenuIntegrationForLookupAnything : IGenericModConfigMenuIntegrationFor<ModConfig>
{
	public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
	{
		menu.Register().AddSectionTitle(I18n.Config_Title_MainOptions).AddCheckbox(I18n.Config_ForceFullScreen_Name, I18n.Config_ForceFullScreen_Desc, (ModConfig config) => config.ForceFullScreen, delegate(ModConfig config, bool value)
		{
			config.ForceFullScreen = value;
		})
			.AddSectionTitle(I18n.Config_Title_Progression)
			.AddCheckbox(I18n.Config_Progression_ShowUncaughtFishSpawnRules_Name, I18n.Config_Progression_ShowUncaughtFishSpawnRules_Desc, (ModConfig config) => config.ShowUncaughtFishSpawnRules, delegate(ModConfig config, bool value)
			{
				config.ShowUncaughtFishSpawnRules = value;
			})
			.AddCheckbox(I18n.Config_Progression_ShowUnknownGiftTastes_Name, I18n.Config_Progression_ShowUnknownGiftTastes_Desc, (ModConfig config) => config.ShowUnknownGiftTastes, delegate(ModConfig config, bool value)
			{
				config.ShowUnknownGiftTastes = value;
			})
			.AddCheckbox(I18n.Config_Progression_ShowUnknownRecipes_Name, I18n.Config_Progression_ShowUnknownRecipes_Desc, (ModConfig config) => config.ShowUnknownRecipes, delegate(ModConfig config, bool value)
			{
				config.ShowUnknownRecipes = value;
			})
			.AddCheckbox(I18n.Config_Progression_ShowPuzzleSolutions_Name, I18n.Config_Progression_ShowPuzzleSolutions_Desc, (ModConfig config) => config.ShowPuzzleSolutions, delegate(ModConfig config, bool value)
			{
				config.ShowPuzzleSolutions = value;
			})
			.AddSectionTitle(I18n.Config_Title_GiftTastes)
			.AddCheckbox(I18n.Config_ShowGiftTastes_Loved_Name, I18n.Config_ShowGiftTastes_Loved_Desc, (ModConfig config) => config.ShowGiftTastes.Loved, delegate(ModConfig config, bool value)
			{
				config.ShowGiftTastes.Loved = value;
			})
			.AddCheckbox(I18n.Config_ShowGiftTastes_Liked_Name, I18n.Config_ShowGiftTastes_Liked_Desc, (ModConfig config) => config.ShowGiftTastes.Liked, delegate(ModConfig config, bool value)
			{
				config.ShowGiftTastes.Liked = value;
			})
			.AddCheckbox(I18n.Config_ShowGiftTastes_Neutral_Name, I18n.Config_ShowGiftTastes_Neutral_Desc, (ModConfig config) => config.ShowGiftTastes.Neutral, delegate(ModConfig config, bool value)
			{
				config.ShowGiftTastes.Neutral = value;
			})
			.AddCheckbox(I18n.Config_ShowGiftTastes_Disliked_Name, I18n.Config_ShowGiftTastes_Disliked_Desc, (ModConfig config) => config.ShowGiftTastes.Disliked, delegate(ModConfig config, bool value)
			{
				config.ShowGiftTastes.Disliked = value;
			})
			.AddCheckbox(I18n.Config_ShowGiftTastes_Hated_Name, I18n.Config_ShowGiftTastes_Hated_Desc, (ModConfig config) => config.ShowGiftTastes.Hated, delegate(ModConfig config, bool value)
			{
				config.ShowGiftTastes.Hated = value;
			})
			.AddCheckbox(I18n.Config_ShowUnownedGifts_Name, I18n.Config_ShowUnownedGifts_Desc, (ModConfig config) => config.ShowUnownedGifts, delegate(ModConfig config, bool value)
			{
				config.ShowUnownedGifts = value;
			})
			.AddCheckbox(I18n.Config_HighlightUnrevealedGiftTastes_Name, I18n.Config_HighlightUnrevealedGiftTastes_Desc, (ModConfig config) => config.HighlightUnrevealedGiftTastes, delegate(ModConfig config, bool value)
			{
				config.HighlightUnrevealedGiftTastes = value;
			})
			.AddSectionTitle(I18n.Config_Title_CollapseFields)
			.AddCheckbox(I18n.Config_CollapseFields_Enabled_Name, I18n.Config_CollapseFields_Enabled_Desc, (ModConfig config) => config.CollapseLargeFields.Enabled, delegate(ModConfig config, bool value)
			{
				config.CollapseLargeFields.Enabled = value;
			})
			.AddNumberField(I18n.Config_CollapseFields_BuildingRecipes_Name, I18n.Config_CollapseFields_Any_Desc, (ModConfig config) => config.CollapseLargeFields.BuildingRecipes, delegate(ModConfig config, int value)
			{
				config.CollapseLargeFields.BuildingRecipes = value;
			}, 1, 1000)
			.AddNumberField(I18n.Config_CollapseFields_ItemRecipes_Name, I18n.Config_CollapseFields_Any_Desc, (ModConfig config) => config.CollapseLargeFields.ItemRecipes, delegate(ModConfig config, int value)
			{
				config.CollapseLargeFields.ItemRecipes = value;
			}, 1, 1000)
			.AddNumberField(I18n.Config_CollapseFields_NpcGiftTastes_Name, I18n.Config_CollapseFields_Any_Desc, (ModConfig config) => config.CollapseLargeFields.NpcGiftTastes, delegate(ModConfig config, int value)
			{
				config.CollapseLargeFields.NpcGiftTastes = value;
			}, 1, 1000)
			.AddSectionTitle(I18n.Config_Title_AdvancedOptions)
			.AddCheckbox(I18n.Config_TileLookups_Name, I18n.Config_TileLookups_Desc, (ModConfig config) => config.EnableTileLookups, delegate(ModConfig config, bool value)
			{
				config.EnableTileLookups = value;
			})
			.AddCheckbox(I18n.Config_DataMiningFields_Name, I18n.Config_DataMiningFields_Desc, (ModConfig config) => config.ShowDataMiningFields, delegate(ModConfig config, bool value)
			{
				config.ShowDataMiningFields = value;
			})
			.AddCheckbox(I18n.Config_ShowInvalidRecipes_Name, I18n.Config_ShowInvalidRecipes_Desc, (ModConfig config) => config.ShowInvalidRecipes, delegate(ModConfig config, bool value)
			{
				config.ShowInvalidRecipes = value;
			})
			.AddCheckbox(I18n.Config_TargetRedirection_Name, I18n.Config_TargetRedirection_Desc, (ModConfig config) => config.EnableTargetRedirection, delegate(ModConfig config, bool value)
			{
				config.EnableTargetRedirection = value;
			})
			.AddNumberField(I18n.Config_ScrollAmount_Name, I18n.Config_ScrollAmount_Desc, (ModConfig config) => config.ScrollAmount, delegate(ModConfig config, int value)
			{
				config.ScrollAmount = value;
			}, 1, 500)
			.AddSectionTitle(I18n.Config_Title_Controls)
			.AddCheckbox(I18n.Config_HideOnKeyUp_Name, I18n.Config_HideOnKeyUp_Desc, (ModConfig config) => config.HideOnKeyUp, delegate(ModConfig config, bool value)
			{
				config.HideOnKeyUp = value;
			})
			.AddKeyBinding(I18n.Config_ToggleLookup_Name, I18n.Config_ToggleLookup_Desc, (ModConfig config) => config.Controls.ToggleLookup, delegate(ModConfig config, KeybindList value)
			{
				config.Controls.ToggleLookup = value;
			})
			.AddKeyBinding(I18n.Config_ToggleSearch_Name, I18n.Config_ToggleSearch_Desc, (ModConfig config) => config.Controls.ToggleSearch, delegate(ModConfig config, KeybindList value)
			{
				config.Controls.ToggleSearch = value;
			})
			.AddKeyBinding(I18n.Config_ScrollUp_Name, I18n.Config_ScrollUp_Desc, (ModConfig config) => config.Controls.ScrollUp, delegate(ModConfig config, KeybindList value)
			{
				config.Controls.ScrollUp = value;
			})
			.AddKeyBinding(I18n.Config_ScrollDown_Name, I18n.Config_ScrollDown_Desc, (ModConfig config) => config.Controls.ScrollDown, delegate(ModConfig config, KeybindList value)
			{
				config.Controls.ScrollDown = value;
			})
			.AddKeyBinding(I18n.Config_PageUp_Name, I18n.Config_PageUp_Desc, (ModConfig config) => config.Controls.PageUp, delegate(ModConfig config, KeybindList value)
			{
				config.Controls.PageUp = value;
			})
			.AddKeyBinding(I18n.Config_PageDown_Name, I18n.Config_PageDown_Desc, (ModConfig config) => config.Controls.PageDown, delegate(ModConfig config, KeybindList value)
			{
				config.Controls.PageDown = value;
			})
			.AddKeyBinding(I18n.Config_ToggleDebug_Name, I18n.Config_ToggleDebug_Desc, (ModConfig config) => config.Controls.ToggleDebug, delegate(ModConfig config, KeybindList value)
			{
				config.Controls.ToggleDebug = value;
			});
	}
}
