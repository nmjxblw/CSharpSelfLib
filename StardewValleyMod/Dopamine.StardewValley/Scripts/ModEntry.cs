using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Weapons;
using StardewValley.Tools;
using StardewValley.Menus;
using StardewValley.GameData.Shops;

namespace Dopamine.StardewValley
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Content.AssetRequested += OnContentAssetRequested;
            helper.Events.Display.MenuChanged += OnDisplayMenuChanged;
        }
        /// <summary>
        /// 注册资产请求事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContentAssetRequested(object? sender, AssetRequestedEventArgs e)
        {

            #region ---注册武器：血龙牙---
            if (e.Name.IsEquivalentTo("Data/Weapons"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, WeaponData>();
                    dict.Data["Blood_Fang"] = WeaponAsset.BloodFang;
                });
            }
            if (e.Name.IsEquivalentTo("Blood_Fang"))
            {
                Monitor.Log("加载血龙牙贴图", LogLevel.Debug);
                e.LoadFromModFile<Texture2D>("Assets/Blood_Fang.png", AssetLoadPriority.Medium);
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
            if (Game1.player.IsLocalPlayer && Game1.activeClickableMenu is ShopMenu shopMenu)
            {
                if (shopMenu.ShopId == "AdventureShop"||shopMenu.ShopId == "SeedShop")
                {
                    shopMenu.itemPriceAndStock.Add(ItemRegistry.Create("(W)Blood_Fang"), new ItemStockInformation(5000, 1, default, default, LimitedStockMode.None));
                    shopMenu.forSale.Add(ItemRegistry.Create("(W)Blood_Fang"));
                }
            }
        }
    }
}