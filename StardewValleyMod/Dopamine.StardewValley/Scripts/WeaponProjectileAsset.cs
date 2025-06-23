using StardewValley.GameData.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.GameData;
using StardewValley.GameData.Objects;

namespace Dopamine.StardewValley
{
    /// <summary>
    /// 武器投射物静态资源
    /// </summary>
    public static class WeaponProjectileAsset
    {
        /// <summary>
        /// 血龙牙投射物静态类
        /// </summary>
        public static BloodFangWeaponProjectile BloodFang => new BloodFangWeaponProjectile();
        /// <summary>
        /// 投射物显示名字
        /// </summary>
        /// <param name="weaponID">武器ID</param>
        /// <returns>投射物翻译名称文本</returns>
        public static string ProjectileDisplayName(this string weaponID) => ModEntry.GetTranslation("Object.Weapon_Projectile.Display", new { weaponName = ModEntry.GetTranslation($"Weapon.{weaponID}.Config.DisplayName") + " " });
    }

    /// <summary>
    /// 血龙牙投射物
    /// </summary>
    public sealed class BloodFangWeaponProjectile : WeaponProjectile
    {
        /// <summary>
        /// 投射物数据配置
        /// </summary>
        public ModConfig Config => ModEntry.Config;
        /// <summary>
        /// 构造函数
        /// </summary>
        public BloodFangWeaponProjectile()
        {
            Id = Config.BloodFang_Projectile_Name;
            Damage = Config.BloodFang_Projectile_Damage;
            Explodes = Config.BloodFang_Projectile_Explodes;
            Bounces = Config.BloodFang_Projectile_Bounces;
            MaxDistance = Config.BloodFang_Projectile_MaxDistance;
            Velocity = Config.BloodFang_Projectile_Velocity;
            RotationVelocity = Config.BloodFang_Projectile_RotationVelocity;
            TailLength = Config.BloodFang_Projectile_TailLength;
            Item = new GenericSpawnItemData()
            {
                ItemId = Config.BloodFang_Projectile_Name,
                Id = Config.BloodFang_Projectile_Name,
            };
        }
    }

    /// <summary>
    /// 投射物物件
    /// </summary>
    public class BloodFangProjectileObjectData : ObjectData
    {
        /// <summary>
        /// 配置
        /// </summary>
        public static ModConfig Config => ModEntry.Config;
        /// <summary>
        /// 构造函数
        /// </summary>
        public BloodFangProjectileObjectData() : base()
        {
            Name = Config.BloodFang_Projectile_Name;
            DisplayName = Config.BloodFang_Projectile_DisplayName;
            Description = Config.BloodFang_Projectile_Description;
            Texture = Config.BloodFang_Projectile_Texture;
        }
    }
}
