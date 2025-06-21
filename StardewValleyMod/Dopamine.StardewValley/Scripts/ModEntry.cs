using System;
using System.Collections.Generic;
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

namespace Dopamine.StardewValley
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private static ModEntry _instance = new ModEntry();
        /// <summary>
        /// Mod Instance
        /// </summary>
        public static ModEntry Instance => _instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            #region 事件注册
            helper.Events.Content.AssetRequested += OnContentAssetRequested;
            helper.Events.Display.MenuChanged += OnDisplayMenuChanged;
            #endregion
        }
        /// <summary>
        /// 注册资产请求事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContentAssetRequested(object? sender, AssetRequestedEventArgs e)
        {

            #region ---注册武器：血龙牙---
            if (e.Name.IsEquivalentTo("Data/Weapons", false))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, WeaponData>();
                    dict.Data["Blood_Fang"] = WeaponAsset.BloodFang;
                });
            }
            else if (e.Name.IsEquivalentTo("Data/Objects", false))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, WeaponProjectile>();
                    dict.Data["Blood_Fang_Projectile"] = WeaponProjectileAsset.BloodFangProjectile;
                });
            }
            else if (e.Name.IsEquivalentTo("Blood_Fang", false))
            {
                Monitor.Log("加载血龙牙贴图", LogLevel.Debug);
                e.LoadFromModFile<Texture2D>("Assets/Blood_Fang.png", AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo("Blood_Fang_Projectile", false)) {
                Monitor.Log("加载血龙牙投射物贴图", LogLevel.Debug);
                e.LoadFromModFile<Texture2D>("Assets/Blood_Fang_Projectile.png", AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo("Data/Shops", false))
            {
                e.Edit(asset =>
                {
                    IAssetDataForDictionary<string, ShopData> dict = asset.AsDictionary<string, ShopData>();
                    dict.Data["AdventureShop"].Items.Add(new ShopItemData()
                    {
                        ItemId = "Blood_Fang",
                        Price = 5000,
                    });
                });
            }
            #endregion
        }
        /// <summary>
        /// 注册显示菜单更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDisplayMenuChanged(object? sender, MenuChangedEventArgs e)
        {
        }
    }
}