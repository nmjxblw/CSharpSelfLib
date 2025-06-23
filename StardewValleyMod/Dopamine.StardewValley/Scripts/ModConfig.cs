using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Dopamine.StardewValley
{
    /// <summary>
    /// Mod配置类
    /// </summary>
    public class ModConfig
    {
        #region ---血龙牙配置---

        #region ---武器本体配置---

        /// <summary>
        /// 血龙牙ID
        /// </summary>
        public string BloodFang_Name { get; set; } = "BloodFang";
        /// <summary>
        /// 血龙牙显示名字
        /// </summary>
        public string BloodFang_DisplayName => $"[LocalizedText Strings\\{ModEntry.AssemblyName}:Weapon.BloodFang.Config.DisplayName]";/*ModEntry.GetTranslation("Weapon.BloodFang.Config.DisplayName");*/
        /// <summary>
        /// 血龙牙武器描述
        /// </summary>
        public string BloodFang_Description => $"[LocalizedText Strings\\{ModEntry.AssemblyName}:Weapon.BloodFang.Config.Description]"; /* ModEntry.GetTranslation("Weapon.BloodFang.Config.Description");*/
        /// <summary>
        /// 血龙牙武器类型
        /// </summary>
        public int BloodFang_Type { get; set; } = 2;
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
        public int BloodFang_Speed { get; set; } = 5;
        /// <summary>
        /// 降低攻击未命中的概率
        /// </summary>
        public int BloodFang_Precision { get; set; } = 50;
        /// <summary>
        /// 武器最小伤害
        /// </summary>
        public int BloodFang_MinDamage { get; set; } = 50;
        /// <summary>
        /// 武器最大伤害
        /// </summary>
        public int BloodFang_MaxDamage { get; set; } = 75;
        /// <summary>
        /// 目标被击退的距离
        /// </summary>
        /// <remarks>武器重量每+1，击退系数+0.1。如重量+5，击退系数为1.5</remarks>
        public float BloodFang_Knockback { get; set; } = 1f;
        /// <summary>
        /// 暴击几率
        /// </summary>
        /// <remarks>取值范围0（永不暴击）到1f（总是暴击）</remarks>
        public float BloodFang_CritChance { get; set; } = 0.1f;
        /// <summary>
        /// 暴击乘数
        /// </summary>
        /// <remarks>暴击伤害 = 基础伤害 × (3 + 武器暴击力量 / 50)，即100的伤害乘数，在游戏中显示4850的暴击力量</remarks>
        public float BloodFang_CritMultiplier { get; set; } = 3f;
        /// <summary>
        /// 每次收到攻击时的伤害减免
        /// </summary>
        /// <remarks>玩家收到的伤害值至少为1</remarks>
        public int BloodFang_Defense { get; set; } = 3;
        /// <summary>
        /// 矿井宝箱基础掉落等级
        /// </summary>
        /// <remarks>-1意味着永远不可能掉落</remarks>
        public int BloodFang_MineBaseLevel { get; set; } = -1;// 不可掉落
        /// <summary>
        /// 矿井宝箱最低掉落等级
        /// </summary>
        /// <remarks>-1意味着永远不可能掉落</remarks>
        public int BloodFang_MineMinLevel { get; set; } = -1;
        /// <summary>
        /// 玩家死亡后掉落该武器
        /// </summary>
        public bool BloodFang_CanBeLostOnDeath { get; set; } = false;
        /// <summary>
        /// 售卖价格
        /// </summary>
        public int BloodFang_Price { get; set; } = 5000;
        /// <summary>
        /// 材质
        /// </summary>
        public string BloodFang_Texture { get; set; } = "BloodFang";

        #endregion

        #region ---血龙牙投射物配置---

        /// <summary>
        /// 投射物ID
        /// </summary>
        public string BloodFang_Projectile_Name { get; set; } = "BloodFang_Projectile";
        /// <summary>
        /// 投射物显示名称
        /// </summary>
        public string BloodFang_Projectile_DisplayName => "BloodFang".ProjectileDisplayName();
        /// <summary>
        /// 投射物描述
        /// </summary>
        public string BloodFang_Projectile_Description => "BloodFang".ProjectileDisplayName();
        /// <summary>
        /// 投射物材质图
        /// </summary>
        public string BloodFang_Projectile_Texture { get; set; } = "BloodFang_Projectile";
        /// <summary>
        /// 伤害
        /// </summary>        
        public int BloodFang_Projectile_Damage { get; set; } = 50;
        /// <summary>
        /// 是否会爆炸
        /// </summary>
        public bool BloodFang_Projectile_Explodes { get; set; } = true;
        /// <summary>
        /// 投射物在销毁前，在墙面上弹射的次数
        /// </summary>
        public int BloodFang_Projectile_Bounces { get; set; } = 0;
        /// <summary>
        /// 投射物最大飞行距离
        /// </summary>
        public int BloodFang_Projectile_MaxDistance { get; set; } = 10;
        /// <summary>
        /// 投射物飞行速度
        /// </summary>
        public int BloodFang_Projectile_Velocity { get; set; } = 10;
        /// <summary>
        /// 投射物旋转速度
        /// </summary>
        public int BloodFang_Projectile_RotationVelocity { get; set; } = 32;
        /// <summary>
        /// 投射物轨迹长度
        /// </summary>
        public int BloodFang_Projectile_TailLength { get; set; } = 1;

        #endregion

        #endregion
    }
}
