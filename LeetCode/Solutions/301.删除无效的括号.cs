/*
 * @lc app=leetcode.cn id=301 lang=csharp
 *
 * [301] 删除无效的括号
 *
 * https://leetcode.cn/problems/remove-invalid-parentheses/description/
 *
 * algorithms
 * Hard (55.66%)
 * Likes:    970
 * Dislikes: 0
 * Total Accepted:    118.7K
 * Total Submissions: 213.3K
 * Testcase Example:  '"()())()"'
 *
 * 给你一个由若干括号和字母组成的字符串 s ，删除最小数量的无效括号，使得输入的字符串有效。
 * 
 * 返回所有可能的结果。答案可以按 任意顺序 返回。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：s = "()())()"
 * 输出：["(())()","()()()"]
 * 
 * 
 * 示例 2：
 * 
 * 
 * 输入：s = "(a)())()"
 * 输出：["(a())()","(a)()()"]
 * 
 * 
 * 示例 3：
 * 
 * 
 * 输入：s = ")("
 * 输出：[""]
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 
 * s 由小写英文字母以及括号 '(' 和 ')' 组成
 * s 中至多含 20 个括号
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
        /// 删除无效的括号
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public System.Collections.Generic.IList<string> RemoveInvalidParentheses(string s)
        {
            throw new System.NotImplementedException();
        }
    }
    // @lc code=end
}
