/*
 * @lc app=leetcode.cn id=188 lang=csharp
 *
 * [188] 买卖股票的最佳时机 IV
 *
 * https://leetcode.cn/problems/best-time-to-buy-and-sell-stock-iv/description/
 *
 * algorithms
 * Hard (52.65%)
 * Likes:    1268
 * Dislikes: 0
 * Total Accepted:    320.7K
 * Total Submissions: 609K
 * Testcase Example:  '2\n[2,4,1]'
 *
 * 给你一个整数数组 prices 和一个整数 k ，其中 prices[i] 是某支给定的股票在第 i 天的价格。
 * 
 * 设计一个算法来计算你所能获取的最大利润。你最多可以完成 k 笔交易。也就是说，你最多可以买 k 次，卖 k 次。
 * 
 * 注意：你不能同时参与多笔交易（你必须在再次购买前出售掉之前的股票）。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：k = 2, prices = [2,4,1]
 * 输出：2
 * 解释：在第 1 天 (股票价格 = 2) 的时候买入，在第 2 天 (股票价格 = 4) 的时候卖出，这笔交易所能获得利润 = 4-2 = 2 。
 * 
 * 示例 2：
 * 
 * 
 * 输入：k = 2, prices = [3,2,6,5,0,3]
 * 输出：7
 * 解释：在第 2 天 (股票价格 = 2) 的时候买入，在第 3 天 (股票价格 = 6) 的时候卖出, 这笔交易所能获得利润 = 6-2 = 4 。
 * ⁠    随后，在第 5 天 (股票价格 = 0) 的时候买入，在第 6 天 (股票价格 = 3) 的时候卖出, 这笔交易所能获得利润 = 3-0 =
 * 3 。
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 <= k <= 100
 * 1 <= prices.length <= 1000
 * 0 <= prices[i] <= 1000
 * 
 * 
 */
using System;
using System.Linq;
namespace LeetCode
{
    // @lc code=start
    public partial class Solution
    {
        /// <summary>
        /// Calculates the maximum profit that can be achieved from at most <paramref name="k"/> transactions on the
        /// given array of stock prices.
        /// </summary>
        /// <param name="k">The maximum number of transactions allowed. Must be a non-negative integer.</param>
        /// <param name="prices">An array of integers representing the stock prices on consecutive days. Cannot be null.</param>
        /// <returns>The maximum profit that can be achieved. Returns 0 if no transactions can be made or if the input array is
        /// empty.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int MaxProfit(int k, int[] prices)
        {
            throw new System.NotImplementedException();
        }
    }
    // @lc code=end
}
