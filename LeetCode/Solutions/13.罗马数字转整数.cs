/*
 * @lc app=leetcode.cn id=13 lang=csharp
 *
 * [13] 罗马数字转整数
 *
 * https://leetcode.cn/problems/roman-to-integer/description/
 *
 * algorithms
 * Easy (64.08%)
 * Likes:    2930
 * Dislikes: 0
 * Total Accepted:    1.2M
 * Total Submissions: 1.8M
 * Testcase Example:  '"III"'
 *
 * 罗马数字包含以下七种字符: I， V， X， L，C，D 和 M。
 *
 *
 * 字符          数值
 * I             1
 * V             5
 * X             10
 * L             50
 * C             100
 * D             500
 * M             1000
 *
 * 例如， 罗马数字 2 写做 II ，即为两个并列的 1 。12 写做 XII ，即为 X + II 。 27 写做  XXVII, 即为 XX + V
 * + II 。
 *
 * 通常情况下，罗马数字中小的数字在大的数字的右边。但也存在特例，例如 4 不写做 IIII，而是 IV。数字 1 在数字 5 的左边，所表示的数等于大数
 * 5 减小数 1 得到的数值 4 。同样地，数字 9 表示为 IX。这个特殊的规则只适用于以下六种情况：
 *
 *
 * I 可以放在 V (5) 和 X (10) 的左边，来表示 4 和 9。
 * X 可以放在 L (50) 和 C (100) 的左边，来表示 40 和 90。
 * C 可以放在 D (500) 和 M (1000) 的左边，来表示 400 和 900。
 *
 *
 * 给定一个罗马数字，将其转换成整数。
 *
 *
 *
 * 示例 1:
 *
 *
 * 输入: s = "III"
 * 输出: 3
 *
 * 示例 2:
 *
 *
 * 输入: s = "IV"
 * 输出: 4
 *
 * 示例 3:
 *
 *
 * 输入: s = "IX"
 * 输出: 9
 *
 * 示例 4:
 *
 *
 * 输入: s = "LVIII"
 * 输出: 58
 * 解释: L = 50, V= 5, III = 3.
 *
 *
 * 示例 5:
 *
 *
 * 输入: s = "MCMXCIV"
 * 输出: 1994
 * 解释: M = 1000, CM = 900, XC = 90, IV = 4.
 *
 *
 *
 * 提示：
 *
 *
 * 1 <= s.length <= 15
 * s 仅含字符 ('I', 'V', 'X', 'L', 'C', 'D', 'M')
 * 题目数据保证 s 是一个有效的罗马数字，且表示整数在范围 [1, 3999] 内
 * 题目所给测试用例皆符合罗马数字书写规则，不会出现跨位等情况。
 * IL 和 IM 这样的例子并不符合题目要求，49 应该写作 XLIX，999 应该写作 CMXCIX 。
 * 关于罗马数字的详尽书写规则，可以参考 罗马数字 - 百度百科。
 *
 *
 */
using System;
namespace LeetCode
{
    // @lc code=start
    public partial class Solution
    {
        /// <summary>
        /// 给定一个罗马数字，将其转换成整数。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int RomanToInt(string s)
        {
            int sum = 0;
            s = s.ToUpper().Trim();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == 'M')
                {
                    sum += 1000;
                }
                else if (s[i] == 'D')
                {
                    sum += 500;
                }
                else if (s[i] == 'C')
                {
                    if (i < s.Length - 1)
                    {
                        if (s[i + 1] == 'D')
                        {
                            sum += 400;
                            i++;
                            continue;
                        }
                        else if (s[i + 1] == 'M')
                        {
                            sum += 900;
                            i++;
                            continue;
                        }
                    }
                    sum += 100;
                }
                else if (s[i] == 'L')
                {
                    sum += 50;
                }
                else if (s[i] == 'X')
                {
                    if (i < s.Length - 1)
                    {
                        if (s[i + 1] == 'L')
                        {
                            sum += 40;
                            i++;
                            continue;
                        }
                        else if (s[i + 1] == 'C')
                        {
                            sum += 90;
                            i++;
                            continue;
                        }
                    }
                    sum += 10;
                }
                else if (s[i] == 'V')
                {
                    sum += 5;
                }
                else
                {
                    if (i < s.Length - 1)
                    {
                        if (s[i + 1] == 'V')
                        {
                            sum += 4;
                            i++;
                            continue;
                        }
                        else if (s[i + 1] == 'X')
                        {
                            sum += 9;
                            i++;
                            continue;
                        }
                    }
                    sum += 1;
                }
            }
            return sum;
        }
    }
    // @lc code=end
}
// 3999/3999 cases passed (10 ms)
// Your runtime beats 6.02 % of csharp submissions
// Your memory usage beats 13.26 % of csharp submissions (50.5 MB)
