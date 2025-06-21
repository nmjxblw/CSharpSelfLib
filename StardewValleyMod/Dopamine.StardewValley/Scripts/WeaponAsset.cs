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
        public static BloodFangWeaponData BloodFang => new BloodFangWeaponData();
    }
    #region ---血龙牙---
    /// <summary>
    /// 血龙牙配置
    /// </summary>
    public sealed class BloodFangWeaponData : WeaponData
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
            /// 降低攻击未命中的概率
            /// </summary>
            public int Precision { get; set; } = 50;
            /// <summary>
            /// 武器最小伤害
            /// </summary>
            public int MinDamage { get; set; } = 50;
            /// <summary>
            /// 武器最大伤害
            /// </summary>
            public int MaxDamage { get; set; } = 75;
            /// <summary>
            /// 目标被击退的距离
            /// </summary>
            /// <remarks>武器重量每+1，击退系数+0.1。如重量+5，击退系数为1.5</remarks>
            public float Knockback { get; set; } = 1.5f;
            /// <summary>
            /// 暴击几率
            /// </summary>
            /// <remarks>取值范围0（永不暴击）到1f（总是暴击）</remarks>
            public float CritChance { get; set; } = 0.01f;
            /// <summary>
            /// 暴击乘数
            /// </summary>
            /// <remarks>暴击伤害 = 基础伤害 × (3 + 武器暴击力量 / 50)，即100的伤害乘数，在游戏中显示4850的暴击力量</remarks>
            public float CritMultiplier { get; set; } = 100f;
            /// <summary>
            /// 每次收到攻击时的伤害减免
            /// </summary>
            /// <remarks>玩家收到的伤害值至少为1</remarks>
            public int Defense { get; set; } = 5;
            /// <summary>
            /// 矿井宝箱基础掉落等级
            /// </summary>
            /// <remarks>-1意味着永远不可能掉落</remarks>
            public int MineBaseLevel { get; set; } = -1;// 不可掉落
            /// <summary>
            /// 矿井宝箱最低掉落等级
            /// </summary>
            /// <remarks>-1意味着永远不可能掉落</remarks>
            public int MineMinLevel { get; set; } = -1;
            /// <summary>
            /// 玩家死亡后掉落该武器
            /// </summary>
            public bool CanBeLostOnDeath { get; set; } = false;
            /// <summary>
            /// 售卖价格
            /// </summary>
            public int Price { get; set; } = 5000;
        }
        /// <summary>
        /// 武器数据配置
        /// </summary>
        public DataConfig Data { get; private set; }= ModEntry.Instance.ReadConfig<DataConfig>() ?? DataConfig.Default;
        /// <summary>
        /// 实例化构造函数
        /// </summary>
        public BloodFangWeaponData()
        {
            Name = "Blood_Fang";
            DisplayName = "[LocalizedText Strings\\Dopamine.StardewValley:Weapon.Blood_Fang.Config.DisplayName]";
            Description = "[LocalizedText Strings\\Dopamine.StardewValley:Weapon.Blood_Fang.Config.Description]";
            Type = 0;
            Speed = Data.Speed;
            Precision = Data.Precision;
            MinDamage = Data.MinDamage;
            MaxDamage = Data.MaxDamage;
            Knockback = Data.Knockback;
            CritChance = Data.CritChance;
            CritMultiplier = Data.CritChance;
            Defense = Data.Defense;
            MineBaseLevel = Data.MineBaseLevel;
            MineMinLevel = Data.MineMinLevel;
            CanBeLostOnDeath = Data.CanBeLostOnDeath;
            Projectiles = new List<WeaponProjectile>() {
                new BloodFangWeaponProjectile()
            };
            Texture = "Blood_Fang";
        }
    }
    #endregion
}
