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
        public static string ProjectileDisplayName(this string weaponID) => ModEntry.Instance.GetTranslation("Object.Weapon_Projectile.Display", new { weaponName = ModEntry.Instance.GetTranslation($"Weapon.{weaponID}.Config.DisplayName") + " " });
    }

    /// <summary>
    /// 血龙牙投射物
    /// </summary>
    public sealed class BloodFangWeaponProjectile : WeaponProjectile
    {
        /// <summary>
        /// 数据配置类
        /// </summary>
        public class DataConfig
        {
            /// <summary>
            /// 默认配置
            /// </summary>
            public static DataConfig Default => new DataConfig();
            /// <summary>
            /// 伤害
            /// </summary>
            public int Damage { get; set; } = 50;
            /// <summary>
            /// 是否会爆炸
            /// </summary>
            public bool Explodes { get; set; } = true;
            /// <summary>
            /// 投射物在销毁前，在墙面上弹射的次数
            /// </summary>
            public int Bounces { get; set; } = 0;
            /// <summary>
            /// 投射物最大飞行距离
            /// </summary>
            public int MaxDistance { get; set; } = 10;
            /// <summary>
            /// 投射物飞行速度
            /// </summary>
            public int Velocity { get; set; } = 10;
            /// <summary>
            /// 投射物旋转速度
            /// </summary>
            public int RotationVelocity { get; set; } = 0;
            /// <summary>
            /// 投射物轨迹长度
            /// </summary>
            public int TailLength { get; set; } = 1;
        }
        /// <summary>
        /// 投射物数据配置
        /// </summary>
        public DataConfig Data { get; private set; } = ModEntry.Instance.Helper.ReadConfig<DataConfig>() ?? DataConfig.Default;
        /// <summary>
        /// 构造函数
        /// </summary>
        public BloodFangWeaponProjectile()
        {
            Id = "Blood_Fang_Projectile";
            Damage = Data.Damage;
            Explodes = Data.Explodes;
            Bounces = Data.Bounces;
            MaxDistance = Data.MaxDistance;
            Velocity = Data.Velocity;
            RotationVelocity = Data.RotationVelocity;
            TailLength = Data.TailLength;
            Item = new GenericSpawnItemData() { Id = "Blood_Fang_Projectile" };
        }
    }
    /// <summary>
    /// 投射物物件
    /// </summary>
    public class BloodFangProjectileObjectData : ObjectData
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public BloodFangProjectileObjectData() : base()
        {
            Name = "Blood_Fang_Projectile";
            DisplayName = "Blood_Fang".ProjectileDisplayName();
            Description = "Blood_Fang".ProjectileDisplayName();
            Texture = "Blood_Fang_Projectile";
        }
    }
}
