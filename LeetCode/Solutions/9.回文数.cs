/*
 * @lc app=leetcode.cn id=9 lang=csharp
 *
 * [9] 回文数
 *
 * https://leetcode.cn/problems/palindrome-number/description/
 *
 * algorithms
 * Easy (56.37%)
 * Likes:    2992
 * Dislikes: 0
 * Total Accepted:    1.8M
 * Total Submissions: 3.2M
 * Testcase Example:  '121'
 *
 * 给你一个整数 x ，如果 x 是一个回文整数，返回 true ；否则，返回 false 。
 *
 * 回文数是指正序（从左向右）和倒序（从右向左）读都是一样的整数。
 *
 *
 * 例如，121 是回文，而 123 不是。
 *
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：x = 121
 * 输出：true
 *
 *
 * 示例 2：
 *
 *
 * 输入：x = -121
 * 输出：false
 * 解释：从左向右读, 为 -121 。 从右向左读, 为 121- 。因此它不是一个回文数。
 *
 *
 * 示例 3：
 *
 *
 * 输入：x = 10
 * 输出：false
 * 解释：从右向左读, 为 01 。因此它不是一个回文数。
 *
 *
 *
 *
 * 提示：
 *
 *
 * -2^31 <= x <= 2^31 - 1
 *
 *
 *
 *
 * 进阶：你能不将整数转为字符串来解决这个问题吗？
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace LeetCode
{
    // @lc code=start


    public partial class Solution
    {
        /// <summary>
        /// 判断一个整数是否是回文数
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool IsPalindrome(int x)
        {
            string s = x.ToString();
            for (int i = 0; i < s.Length / 2; i++)
            {
                if (s[i] != s[s.Length - 1 - i])
                    return false;
            }
            return true;
        }
    }
    // @lc code=end
}
// 11511/11511 cases passed (2 ms)
// Your runtime beats 76.55 % of csharp submissions
// Your memory usage beats 36.88 % of csharp submissions (35.9 MB)
