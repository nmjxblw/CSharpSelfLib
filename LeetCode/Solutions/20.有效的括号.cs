/*
 * @lc app=leetcode.cn id=20 lang=csharp
 *
 * [20] 有效的括号
 *
 * https://leetcode.cn/problems/valid-parentheses/description/
 *
 * algorithms
 * Easy (44.74%)
 * Likes:    4720
 * Dislikes: 0
 * Total Accepted:    2.2M
 * Total Submissions: 4.9M
 * Testcase Example:  '"()"'
 *
 * 给定一个只包括 '('，')'，'{'，'}'，'['，']' 的字符串 s ，判断字符串是否有效。
 *
 * 有效字符串需满足：
 *
 *
 * 左括号必须用相同类型的右括号闭合。
 * 左括号必须以正确的顺序闭合。
 * 每个右括号都有一个对应的相同类型的左括号。
 *
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：s = "()"
 *
 * 输出：true
 *
 *
 * 示例 2：
 *
 *
 * 输入：s = "()[]{}"
 *
 * 输出：true
 *
 *
 * 示例 3：
 *
 *
 * 输入：s = "(]"
 *
 * 输出：false
 *
 *
 * 示例 4：
 *
 *
 * 输入：s = "([])"
 *
 * 输出：true
 *
 *
 *
 *
 * 提示：
 *
 *
 * 1 <= s.length <= 10^4
 * s 仅由括号 '()[]{}' 组成
 *
 *
 */

// @lc code=start
using System;
using System.Collections.Generic;

public partial class Solution
{
#if true
    /// <summary>
    /// 给定一个只包括 '('，')'，'{'，'}'，'['，']' 的字符串 s ，判断字符串是否有效。
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool IsValid(string s)
    {
        if (string.IsNullOrEmpty(s))
            return true;
        Stack<char> stack = new Stack<char>();
        List<char> leftparentheses = new List<char> { ')', '}', ']' };
        bool Compare(char a, char b)
        {
            return (a == '(' && b == ')') || (a == '{' && b == '}') || (a == '[' && b == ']');
        }
        foreach (char c in s)
        {
            if (stack.Count > 0)
            {
                if (Compare(stack.Peek(), c))
                {
                    stack.Pop();
                    continue;
                }
                else if (leftparentheses.Contains(c))
                    return false;
            }
            stack.Push(c);
        }
        return stack.Count <= 0;
    }
#endif
}
// @lc code=end
// 100/100 cases passed (2 ms)
// Your runtime beats 68.51 % of csharp submissions
// Your memory usage beats 69.08 % of csharp submissions (41.7 MB)
