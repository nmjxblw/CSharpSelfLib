/*
 * @lc app=leetcode.cn id=316 lang=csharp
 *
 * [316] 去除重复字母
 *
 * https://leetcode.cn/problems/remove-duplicate-letters/description/
 *
 * algorithms
 * Medium (49.78%)
 * Likes:    1143
 * Dislikes: 0
 * Total Accepted:    159.3K
 * Total Submissions: 319.4K
 * Testcase Example:  '"bcabc"'
 *
 * 给你一个字符串 s ，请你去除字符串中重复的字母，使得每个字母只出现一次。需保证 返回结果的字典序最小（要求不能打乱其他字符的相对位置）。
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：s = "bcabc"
 * 输出："abc"
 *
 *
 * 示例 2：
 *
 *
 * 输入：s = "cbacdcbc"
 * 输出："acdb"
 *
 *
 *
 * 提示：
 *
 *
 * 1 <= s.length <= 10^4
 * s 由小写英文字母组成
 *
 *
 *
 *
 * 注意：该题与 1081
 * https://leetcode-cn.com/problems/smallest-subsequence-of-distinct-characters
 * 相同
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
        /// 去除重复字母
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string RemoveDuplicateLetters(string s)
        {
            string result = s;
            return result;
        }
    }
    // @lc code=end
}