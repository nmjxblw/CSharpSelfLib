using StardewValley.GameData.Weapons;
using System;
using System.Collections.Generic;
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
        public static BloodFangConfig BloodFang => new BloodFangConfig();
    }
    /// <summary>
    /// 血龙牙配置
    /// </summary>
    public class BloodFangConfig : WeaponData
    {
        /// <summary>
        /// 实例化构造函数
        /// </summary>
        public BloodFangConfig()
        {
            Name = "Blood_Fang";
            DisplayName = "Blood Fang";
            Description = "A fang that has absorbed the blood of its victims.";
            Type = 0;
            Speed = 4;
            MinDamage = 50;
            MaxDamage = 75;
            Knockback = 1.5f;
            CritChance = 0.01f;
            CritMultiplier = 100f;
            Defense = 5;
            MineBaseLevel = 3;
            MineMinLevel = 1;
            CanBeLostOnDeath = false;
            Projectiles = new List<WeaponProjectile>() {
                new WeaponProjectile()
                {
                    Id = "Dopamine_BloodFangProjectile",
                    Damage = 50,
                    Explodes = true
                }
            };
            Texture = "Blood_Fang";
        }
    }
}
