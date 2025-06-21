using Newtonsoft.Json;
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
    #region ---血龙牙---
    /// <summary>
    /// 血龙牙配置
    /// </summary>
    public sealed class BloodFangConfig : WeaponData
    {
        /// <summary>
        /// 数据配置类
        /// </summary>
        public class DataConfig
        {
            /// <summary>
            /// 武器速度
            /// </summary>
            /// <remarks>
            /// 武器速度指的是用武器击中敌人所需的时间，以及再次击中敌人所需的恢复时间。
            /// 默认为+0武器速度，剑每次行动需要400毫秒，棍棒每次行动需要720毫秒（相当于-8 buff）。
            /// 匕首比剑快得多。每个动作300毫秒[3]，不能修改。
            /// 每增加一点武器速度就会减少40毫秒的时间，这意味着剑的最大有效buff是+10，尽管实际性能限制可能会更低（剑的最大有效buff大约是+4，棍棒的最大有效buff大约是+12）。
            /// 这也意味着一开始，武器速度对俱乐部的影响要小得多，因为每秒的伤害增加（当连续攻击时）的百分比要小得多：一把+1武器速度的剑每秒的伤害增加11%，而一个+1武器速度的俱乐部每秒的伤害只增加6%。
            /// 随着buff的增加，剑会更快地达到实际性能极限，而棍棒则会随着buff的增加而不断提高。
            /// 玩家的移动速度buff也适用于武器，[5]，并有效地与武器速度buff叠加在一起。
            /// 例如，速度+2的武器将与速度+1的武器一样快，但在咖啡的作用下，武器的速度将增加+1。
            /// </remarks>
            public int Speed { get; set; } = 5;
            /// <summary>
            /// 
            /// </summary>
            public int Precision { get; set; } = 50;
            public int MinDamage { get; set; } = 50;
            public int MaxDamage { get; set; } = 75;
            public float Knockback { get; set; } = 1.5f;
            public float CritChance { get; set; } = 0.01f;
            public float CritMultiplier { get; set; } = 100f;
            public int Defense { get; set; } = 5;
            public int MineBaseLevel { get; set; } = -1;// 不可掉落
            public int MineMinLevel { get; set; } = -1;
            public bool CanBeLostOnDeath { get; set; } = false;
        }
        /// <summary>
        /// 实例化构造函数
        /// </summary>
        public BloodFangConfig()
        {
            BloodFangConfig.DataConfig DataConfig = ModEntry.Instance.Helper.ReadConfig<BloodFangConfig.DataConfig>();
            Name = "Blood_Fang";
            DisplayName = "[LocalizedText Strings\\Weapon.Blood_Fang.Config.DisplayName]";
            Description = "[LocalizedText Strings\\Weapon.Blood_Fang.Config.Description]";
            Type = 0;
            Speed = DataConfig.Speed;
            Precision = 50;
            MinDamage = 50;
            MaxDamage = 75;
            Knockback = 1.5f;
            CritChance = 0.01f;
            CritMultiplier = 100f;
            Defense = 5;
            MineBaseLevel = -1;// 不可掉落
            MineMinLevel = -1;
            CanBeLostOnDeath = false;
            Projectiles = new List<WeaponProjectile>() {
                new BloodFangProjectile()
            };
            Texture = "Blood_Fang";
        }
    }
    #endregion
}
