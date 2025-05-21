using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Solution
{
    /// <summary>
    /// 提莫在《英雄联盟》游戏中是一个非常可爱的角色，他的技能可以让敌人中毒。给你一个整数数组 timeSeries ，表示提莫在每个时间点施放技能的时刻，以及一个整数 duration ，表示提莫的技能在施放后持续多长时间（单位是秒）。返回敌人受到的总中毒时间。
    /// </summary>
    /// <param name="timeSeries"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public int FindPoisonedDuration(int[] timeSeries, int duration)
    {
        if (timeSeries.Length == 0) return 0;
        int totalDuration = 0;
        for (int i = 1; i < timeSeries.Length; i++)
        {
            totalDuration += Math.Min(duration, timeSeries[i] - timeSeries[i - 1]);
        }
        return totalDuration + duration;
    }
}
