/*
 * @lc app=leetcode.cn id=321 lang=csharp
 *
 * [321] 拼接最大数
 *
 * https://leetcode.cn/problems/create-maximum-number/description/
 *
 * algorithms
 * Hard (42.40%)
 * Likes:    606
 * Dislikes: 0
 * Total Accepted:    45.7K
 * Total Submissions: 107.7K
 * Testcase Example:  '[3,4,6,5]\n[9,1,2,5,8,3]\n5'
 *
 * 给你两个整数数组 nums1 和 nums2，它们的长度分别为 m 和 n。数组 nums1 和 nums2
 * 分别代表两个数各位上的数字。同时你也会得到一个整数 k。
 * 
 * 请你利用这两个数组中的数字创建一个长度为 k <= m + n 的最大数。同一数组中数字的相对顺序必须保持不变。
 * 
 * 返回代表答案的长度为 k 的数组。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：nums1 = [3,4,6,5], nums2 = [9,1,2,5,8,3], k = 5
 * 输出：[9,8,6,5,3]
 * 
 * 
 * 示例 2：
 * 
 * 
 * 输入：nums1 = [6,7], nums2 = [6,0,4], k = 5
 * 输出：[6,7,6,0,4]
 * 
 * 
 * 示例 3：
 * 
 * 
 * 输入：nums1 = [3,9], nums2 = [8,9], k = 3
 * 输出：[9,8,9]
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * m == nums1.length
 * n == nums2.length
 * 1 <= m, n <= 500
 * 0 <= nums1[i], nums2[i] <= 9
 * 1 <= k <= m + n
 * nums1 和 nums2 没有前导 0。
 * 
 * 
 */
namespace LeetCode
{
    // @lc code=start
    public partial class Solution
    {
        /// <summary>
        /// Combines two arrays to create the largest possible number of a specified length.
        /// </summary>
        /// <remarks>The method selects elements from <paramref name="nums1"/> and <paramref
        /// name="nums2"/> in such a way that the resulting array is lexicographically the largest possible.</remarks>
        /// <param name="nums1">The first array of integers to be considered for the result.</param>
        /// <param name="nums2">The second array of integers to be considered for the result.</param>
        /// <param name="k">The length of the resulting array. Must be a non-negative integer less than or equal to the combined length
        /// of <paramref name="nums1"/> and <paramref name="nums2"/>.</param>
        /// <returns>An array of integers representing the largest possible number of length <paramref name="k"/> formed by
        /// combining elements from <paramref name="nums1"/> and <paramref name="nums2"/> while maintaining their
        /// relative order.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int[] MaxNumber(int[] nums1, int[] nums2, int k)
        {
            throw new System.NotImplementedException();
        }
    }
    // @lc code=end
}
