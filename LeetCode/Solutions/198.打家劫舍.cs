/*
 * @lc app=leetcode.cn id=198 lang=csharp
 *
 * [198] 打家劫舍
 *
 * https://leetcode.cn/problems/house-robber/description/
 *
 * algorithms
 * Medium (55.74%)
 * Likes:    3252
 * Dislikes: 0
 * Total Accepted:    1.3M
 * Total Submissions: 2.3M
 * Testcase Example:  '[1,2,3,1]'
 *
 * 
 * 你是一个专业的小偷，计划偷窃沿街的房屋。每间房内都藏有一定的现金，影响你偷窃的唯一制约因素就是相邻的房屋装有相互连通的防盗系统，如果两间相邻的房屋在同一晚上被小偷闯入，系统会自动报警。
 * 
 * 给定一个代表每个房屋存放金额的非负整数数组，计算你 不触动警报装置的情况下 ，一夜之内能够偷窃到的最高金额。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：[1,2,3,1]
 * 输出：4
 * 解释：偷窃 1 号房屋 (金额 = 1) ，然后偷窃 3 号房屋 (金额 = 3)。
 * 偷窃到的最高金额 = 1 + 3 = 4 。
 * 
 * 示例 2：
 * 
 * 
 * 输入：[2,7,9,3,1]
 * 输出：12
 * 解释：偷窃 1 号房屋 (金额 = 2), 偷窃 3 号房屋 (金额 = 9)，接着偷窃 5 号房屋 (金额 = 1)。
 * 偷窃到的最高金额 = 2 + 9 + 1 = 12 。
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 
 * 0 
 * 
 * 
 */
using System;
using System.Collections.Generic;
namespace LeetCode
{
	// @lc code=start

	public partial class Solution
	{
		/// <summary>
		/// 打家劫舍
		/// </summary>
		/// <param name="nums"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public int Rob(int[] nums)
		{
			if (nums.Length == 0) return 0;
			if (nums.Length == 1) return nums[0];
			int[] dp = new int[nums.Length + 1];
			dp[0] = nums[0];
			dp[1] = Math.Max(nums[0], nums[1]);
			for (int index = 1; index < nums.Length; index++)
			{
				dp[index + 1] = Math.Max(dp[index], dp[index - 1] + nums[index]);
			}
			return dp[nums.Length];
		}
	}
	// @lc code=end
}
