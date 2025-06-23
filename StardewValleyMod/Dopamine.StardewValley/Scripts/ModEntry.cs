using System;
using System.Collections.Generic;
using System.Reflection;
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
        /// <summary>
        /// Mod静态配置类
        /// </summary>
        public static ModConfig Config
        {
            get => ReadConfig<ModConfig>();
            set => WriteConfig<ModConfig>(value);
        }
        /// <summary>
        /// 程序集名称
        /// </summary>
        public static string AssemblyName { get; private set; } = Assembly.GetExecutingAssembly().GetName().Name!;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            #region 事件注册
            helper.Events.Content.AssetRequested += OnContentAssetRequested;
            helper.Events.Input.CursorMoved += OnCursorMoved;
            helper.Events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
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
            #region --- 注册字符串 ---

            if (e.Name.IsEquivalentTo($"Strings/{AssemblyName}", false))
            {
                LanguageCode localeEnum = Helper.Translation.LocaleEnum;
                string? locale = (int)localeEnum != 0 ? Helper.Translation.LocaleEnum.ToString() : "default";
                e.LoadFromModFile<Dictionary<string, string>>("i18n/" + locale + ".json", (AssetLoadPriority)0);
            }

            #endregion

            #region ---注册武器：血龙牙---

            #region --- 武器本体 ---
            if (e.Name.IsEquivalentTo("Data/Weapons", false))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, WeaponData>();
                    dict.Data[Config.BloodFang_Name] = new BloodFangWeaponData();
                });
            }
            if (e.Name.IsEquivalentTo(Config.BloodFang_Name, false))
            {
                e.LoadFromModFile<Texture2D>($"Assets/{Config.BloodFang_Name}.png", AssetLoadPriority.Medium);
            }
            #endregion

            #region --- 投射物 ---
            if (e.Name.IsEquivalentTo("Data/Objects", false))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, ObjectData>();
                    dict.Data[Config.BloodFang_Projectile_Name] = new BloodFangProjectileObjectData();
                });
            }
            if (e.Name.IsEquivalentTo(Config.BloodFang_Projectile_Name, false))
            {
                e.LoadFromModFile<Texture2D>($"Assets/{Config.BloodFang_Projectile_Name}.png", AssetLoadPriority.Medium);
            }
            #endregion

            #endregion

            #region --- 商店商品注册 ---
            if (e.Name.IsEquivalentTo("Data/Shops", false))
            {
                e.Edit(asset =>
                {
                    IAssetDataForDictionary<string, ShopData> dict = asset.AsDictionary<string, ShopData>();
                    dict.Data["AdventureShop"].Items.Add(new ShopItemData()
                    {
                        ItemId = Config.BloodFang_Name,
                        Price = Config.BloodFang_Price,
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
        /// 注册鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
        {
        }
        /// <summary>
        /// 注册鼠标滚轮滚动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
        {
        }
        // <summary>
        /// 注册鼠标、控制器或者按键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
        }
        /// <summary>
        /// 注册鼠标、控制器或者按键松开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
        }
        /// <summary>
        /// 注册鼠标、控制器或者按键状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
        }
        /// <summary>
        /// 获取翻译
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>对应的翻译文本</returns>
        public static Translation GetTranslation(string key) => ((Mod)Instance!).Helper.Translation.Get(key).Default("missing translation?");
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
        public static Translation GetTranslation(string key, object? tokens) => ((Mod)Instance!).Helper.Translation.Get(key, tokens);
        /// <summary>
        /// 读取模组配置文件
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <returns></returns>
        public static TConfig ReadConfig<TConfig>() where TConfig : class, new() => ((Mod)Instance!).Helper.ReadConfig<TConfig>();
        /// <summary>
        /// 写入模组配置文件
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="config"></param>
        public static void WriteConfig<TConfig>(TConfig config) where TConfig : class, new() => ((Mod)Instance!).Helper.WriteConfig<TConfig>(config);
    }
}