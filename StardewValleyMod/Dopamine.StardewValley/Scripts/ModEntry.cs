using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private static ModEntry? _instance;
        public ModEntry()
        {
            if (_instance == null)
                _instance = this;
        }
        /// <summary>
        /// Mod Instance
        /// </summary>
        public static ModEntry? Instance => _instance;
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
                    dict.Data["Blood_Fang"] = new BloodFangWeaponData();
                });
            }
            else if (e.Name.IsEquivalentTo("Data/Objects", false))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, ObjectData>();
                    dict.Data["Blood_Fang_Projectile"] = new BloodFangProjectileObjectData();
                });
            }
            else if (e.Name.IsEquivalentTo("Blood_Fang", false))
            {
                Monitor.Log("加载血龙牙贴图", LogLevel.Debug);
                e.LoadFromModFile<Texture2D>("Assets/Blood_Fang.png", AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo("Blood_Fang_Projectile", false))
            {
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
                        Price = WeaponAsset.BloodFang.Data.Price,
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
        /// <summary>
        /// 获取翻译
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>对应的翻译文本</returns>
        public Translation GetTranslation(string key) => ((Mod)this).Helper.Translation.Get(key).Default("missing translation?");
        /// <summary>
        /// 获取翻译
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="tokens">令牌对象
        /// <para>
        /// An object containing token key/value pairs. 
        /// This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>),
        /// a dictionary, or a class instance.
        /// </para>
        /// </param>
        /// <returns>对应的翻译文本</returns>
        public Translation GetTranslation(string key, object? tokens) => ((Mod)this).Helper.Translation.Get(key, tokens);
        /// <summary>
        /// 读取模组配置文件
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <returns></returns>
        public TConfig ReadConfig<TConfig>() where TConfig : class, new() => ((Mod)this).Helper.ReadConfig<TConfig>();
        /// <summary>
        /// 写入模组配置文件
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="config"></param>
        public void WriteConfig<TConfig>(TConfig config) where TConfig : class, new() => ((Mod)this).Helper.WriteConfig<TConfig>(config);
    }
}