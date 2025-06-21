using StardewValley.GameData.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.GameData;

namespace Dopamine.StardewValley
{
    /// <summary>
    /// 武器投射物静态资源
    /// </summary>
    public static class WeaponProjectileAsset
    {
        /// <summary>
        /// 血龙牙投射物静态资源
        /// </summary>
        public static BloodFangProjectile BloodFangProjectile = new BloodFangProjectile();
    }

    /// <summary>
    /// 血龙牙投射物
    /// </summary>
    public class BloodFangProjectile : WeaponProjectile
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public BloodFangProjectile()
        {

            Id = "Blood_Fang_Projectile";
            Damage = 50;
            Explodes = true;
            Bounces = 0;
            MaxDistance = 10;
            Velocity = 10;
            RotationVelocity = 0;
            TailLength = 1;
            Item = new GenericSpawnItemData();
        }
    }
}
