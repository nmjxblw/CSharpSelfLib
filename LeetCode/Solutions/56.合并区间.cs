/*
 * @lc app=leetcode.cn id=56 lang=csharp
 *
 * [56] 合并区间
 *
 * https://leetcode.cn/problems/merge-intervals/description/
 *
 * algorithms
 * Medium (51.39%)
 * Likes:    2555
 * Dislikes: 0
 * Total Accepted:    1.1M
 * Total Submissions: 2.2M
 * Testcase Example:  '[[1,3],[2,6],[8,10],[15,18]]'
 *
 * 以数组 intervals 表示若干个区间的集合，其中单个区间为 intervals[i] = [starti, endi]
 * 。请你合并所有重叠的区间，并返回 一个不重叠的区间数组，该数组需恰好覆盖输入中的所有区间 。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：intervals = [[1,3],[2,6],[8,10],[15,18]]
 * 输出：[[1,6],[8,10],[15,18]]
 * 解释：区间 [1,3] 和 [2,6] 重叠, 将它们合并为 [1,6].
 * 
 * 
 * 示例 2：
 * 
 * 
 * 输入：intervals = [[1,4],[4,5]]
 * 输出：[[1,5]]
 * 解释：区间 [1,4] 和 [4,5] 可被视为重叠区间。
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 <= intervals.length <= 10^4
 * intervals[i].length == 2
 * 0 <= starti <= endi <= 10^4
 * 
 * 
 */
namespace LeetCode
{
    // @lc code=start
    public partial class Solution
    {
        /// <summary>
        /// 合并区间
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int[][] Merge(int[][] intervals)
        {
            throw new System.NotImplementedException();
        }
    }
    // @lc code=end
}

