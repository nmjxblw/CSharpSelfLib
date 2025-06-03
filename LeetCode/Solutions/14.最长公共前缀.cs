/*
 * @lc app=leetcode.cn id=14 lang=csharp
 *
 * [14] 最长公共前缀
 *
 * https://leetcode.cn/problems/longest-common-prefix/description/
 *
 * algorithms
 * Easy (44.77%)
 * Likes:    3322
 * Dislikes: 0
 * Total Accepted:    1.5M
 * Total Submissions: 3.3M
 * Testcase Example:  '["flower","flow","flight"]'
 *
 * 编写一个函数来查找字符串数组中的最长公共前缀。
 *
 * 如果不存在公共前缀，返回空字符串 ""。
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：strs = ["flower","flow","flight"]
 * 输出："fl"
 *
 *
 * 示例 2：
 *
 *
 * 输入：strs = ["dog","racecar","car"]
 * 输出：""
 * 解释：输入不存在公共前缀。
 *
 *
 *
 * 提示：
 *
 *
 * 1 <= strs.length <= 200
 * 0 <= strs[i].length <= 200
 * strs[i] 如果非空，则仅由小写英文字母组成
 *
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
// @lc code=start
namespace LeetCode
{

    public partial class Solution
    {
        /// <summary>
        /// 查找字符串数组中的最长公共前缀
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public string LongestCommonPrefix(string[] strs)
        {
            if (strs.Length == 0)
                return "";
            if (strs.Length == 1)
                return strs[0];
            string shortestStr = strs[0];
            foreach (string str in strs)
            {
                if (str.Length <= 0)
                    return str;
                else if (shortestStr.Length > str.Length)
                    shortestStr = str;
            }
            string result = shortestStr;
            for (int i = 0; i < shortestStr.Length; i++)
            {
                foreach (string str in strs)
                {
                    if (str[i] != shortestStr[i])
                    {
                        result = shortestStr.Substring(0, i);
                        return result;
                    }
                }
            }
            return result;
        }
    }
    // @lc code=end
}
// 126/126 cases passed (1 ms)
// Your runtime beats 88.09 % of csharp submissions
// Your memory usage beats 90.31 % of csharp submissions (42.4 MB)
