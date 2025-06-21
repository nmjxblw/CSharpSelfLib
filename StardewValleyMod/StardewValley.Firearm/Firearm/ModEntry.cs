using System;
using System.Collections.Generic;
using Firearm.Framework;
using Firearm.Framework.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Weapons;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.SaveSerialization;
using static StardewValley.LocalizedContentManager;

namespace Firearm;

public class ModEntry : Mod
{
	private static ModEntry _instance;

	public Config Config;

	public ModEntry()
	{
		_instance = this;
	}

	public override void Entry(IModHelper helper)
	{
		Config = helper.ReadConfig<Config>();
		helper.Events.Content.AssetRequested += OnAssetRequested;
		Harmony harmony = new Harmony(((Mod)this).ModManifest.UniqueID);
		harmony.Patch(AccessTools.Method(typeof(SaveSerializer), "GetSerializer"), new HarmonyMethod(typeof(SaveSerializerPatches), "GetSerializer"));
		harmony.Patch(AccessTools.Method(typeof(WeaponDataDefinition), "CreateItem"), null, new HarmonyMethod(typeof(WeaponDataDefinitionPatches), "CreateItem"));
		harmony.Patch(AccessTools.Method(typeof(Game1), "drawTool", new Type[1] { typeof(Farmer) }), new HarmonyMethod(typeof(Game1Patches), "DrawTool"));
		harmony.Patch(AccessTools.Method(typeof(CraftingPage), "layoutRecipes"), null, new HarmonyMethod(typeof(CraftingPagePatches), "LayoutRecipes"));
	}

	private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		if (e.Name.IsEquivalentTo("Data/Weapons", false))
		{
			e.Edit((Action<IAssetData>)delegate(IAssetData assets)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Expected O, but got Unknown
				//IL_005c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0077: Unknown result type (might be due to invalid IL or missing references)
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_0099: Expected O, but got Unknown
				IAssetDataForDictionary<string, WeaponData> val = assets.AsDictionary<string, WeaponData>();
				((IAssetData<IDictionary<string, WeaponData>>)(object)val).Data["Firearm_AK47"] = new WeaponData
				{
					Name = "Firearm_AK47",
					DisplayName = "[LocalizedText Strings\\Firearm:Firearm.Weapon.Ak47.DisplayName]",
					Description = "[LocalizedText Strings\\Firearm:Firearm.Weapon.Ak47.Description]",
					CanBeLostOnDeath = false,
					Texture = "Firearm_AK47"
				};
				((IAssetData<IDictionary<string, WeaponData>>)(object)val).Data["Firearm_M16"] = new WeaponData
				{
					Name = "Firearm_M16",
					DisplayName = "[LocalizedText Strings\\Firearm:Firearm.Weapon.M16.DisplayName]",
					Description = "[LocalizedText Strings\\Firearm:Firearm.Weapon.M16.Description]",
					CanBeLostOnDeath = false,
					Texture = "Firearm_M16"
				};
			}, (AssetEditPriority)0, (string)null);
		}
		else if (e.Name.IsEquivalentTo("Data/Objects", false))
		{
			e.Edit((Action<IAssetData>)delegate(IAssetData assets)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Expected O, but got Unknown
				IAssetDataForDictionary<string, ObjectData> val = assets.AsDictionary<string, ObjectData>();
				((IAssetData<IDictionary<string, ObjectData>>)(object)val).Data["Firearm_Ammo_Assault_Rifle"] = new ObjectData
				{
					Name = "Firearm_Ammo_Assault_Rifle",
					DisplayName = "[LocalizedText Strings\\Firearm:Firearm.Object.AssaultRifleAmmo.DisplayName]",
					Description = "[LocalizedText Strings\\Firearm:Firearm.Object.AssaultRifleAmmo.Description]",
					Texture = "Firearm_Ammo"
				};
			}, (AssetEditPriority)0, (string)null);
		}
		else if (e.Name.IsEquivalentTo("Data/CraftingRecipes", false))
		{
			e.Edit((Action<IAssetData>)delegate(IAssetData assets)
			{
				IAssetDataForDictionary<string, string> val = assets.AsDictionary<string, string>();
				((IAssetData<IDictionary<string, string>>)(object)val).Data["Firearm_AK47"] = "335 20/Field/Firearm_AK47/false/default/";
				((IAssetData<IDictionary<string, string>>)(object)val).Data["Firearm_M16"] = "335 20/Field/Firearm_M16/false/default/";
				((IAssetData<IDictionary<string, string>>)(object)val).Data["Firearm_Ammo_Assault_Rifle"] = "382 1 378 1/Field/Firearm_Ammo_Assault_Rifle/false/default/";
			}, (AssetEditPriority)0, (string)null);
		}
		else if (e.Name.IsEquivalentTo("Firearm_AK47", false))
		{
			e.LoadFromModFile<Texture2D>("assets/assault_rifle/ak47.png", (AssetLoadPriority)0);
		}
		else if (e.Name.IsEquivalentTo("Firearm_M16", false))
		{
			e.LoadFromModFile<Texture2D>("assets/assault_rifle/m16.png", (AssetLoadPriority)0);
		}
		else if (e.Name.IsEquivalentTo("Firearm_Ammo", false))
		{
			e.LoadFromModFile<Texture2D>("assets/ammo.png", (AssetLoadPriority)0);
		}
		else if (e.Name.IsEquivalentTo("Strings/Firearm", false))
		{
			LanguageCode localeEnum = ((Mod)this).Helper.Translation.LocaleEnum;
			if (1 == 0)
			{
			}
			string text = (((int)localeEnum != 0) ? ((object)((Mod)this).Helper.Translation.LocaleEnum/*cast due to .constrained prefix*/).ToString() : "default");
			if (1 == 0)
			{
			}
			string locale = text;
			e.LoadFromModFile<Dictionary<string, string>>("i18n/" + locale + ".json", (AssetLoadPriority)0);
		}
		else if (e.Name.IsEquivalentTo("Data/Shops", false))
		{
			e.Edit((Action<IAssetData>)delegate(IAssetData asset)
			{
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Expected O, but got Unknown
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_005c: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Expected O, but got Unknown
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected O, but got Unknown
				IAssetDataForDictionary<string, ShopData> val = asset.AsDictionary<string, ShopData>();
				((IAssetData<IDictionary<string, ShopData>>)(object)val).Data["AdventureShop"].Items.AddRange((IEnumerable<ShopItemData>)(object)new ShopItemData[3]
				{
					new ShopItemData
					{
						ItemId = "Firearm_AK47",
						Price = Config.Ak47Price
					},
					new ShopItemData
					{
						ItemId = "Firearm_M16",
						Price = Config.M16Price
					},
					new ShopItemData
					{
						ItemId = "Firearm_Ammo_Assault_Rifle",
						Price = Config.AssaultRiflePrice
					}
				});
			}, (AssetEditPriority)0, (string)null);
		}
	}

	public static ModEntry GetInstance()
	{
		return _instance;
	}
}
