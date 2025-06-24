using Newtonsoft.Json;
using StardewValley.GameData.Weapons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Dopamine.StardewValley
{
    /// <summary>
    /// 武器静态资源类
    /// </summary>
    public static class WeaponAsset
    {
        /// <summary>
        /// 血龙牙静态配置
        /// </summary>
        public static BloodFangWeaponData BloodFang => new BloodFangWeaponData();
    }
    #region ---血龙牙---
    /// <summary>
    /// 血龙牙配置
    /// </summary>
    public sealed class BloodFangWeaponData : WeaponData
    {
        /// <summary>
        /// 武器数据配置
        /// </summary>
        public static ModConfig Config => ModEntry.Config;
        /// <summary>
        /// 实例化构造函数
        /// </summary>
        public BloodFangWeaponData()
        {
            // 导入配置表信息
            Name = Config.BloodFang_Name;
            DisplayName = Config.BloodFang_DisplayName;
            Description = Config.BloodFang_Description;
            Type = Config.BloodFang_Type;
            Speed = Config.BloodFang_Speed;
            Precision = Config.BloodFang_Precision;
            MinDamage = Config.BloodFang_MinDamage;
            MaxDamage = Config.BloodFang_MaxDamage;
            Knockback = Config.BloodFang_Knockback;
            CritChance = Config.BloodFang_CritChance;
            CritMultiplier = Config.BloodFang_CritMultiplier;
            Defense = Config.BloodFang_Defense;
            MineBaseLevel = Config.BloodFang_MineBaseLevel;
            MineMinLevel = Config.BloodFang_MineMinLevel;
            AreaOfEffect = Config.BloodFang_AreaOfEffect;
            CanBeLostOnDeath = Config.BloodFang_CanBeLostOnDeath;
            Projectiles = new List<WeaponProjectile>() {
                new BloodFangWeaponProjectile()
            };
            Texture = Config.BloodFang_Texture;
        }
    }
    #endregion
}
