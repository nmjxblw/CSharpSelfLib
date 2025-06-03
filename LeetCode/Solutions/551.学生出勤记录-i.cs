/*
 * @lc app=leetcode.cn id=551 lang=csharp
 *
 * [551] 学生出勤记录 I
 *
 * https://leetcode.cn/problems/student-attendance-record-i/description/
 *
 * algorithms
 * Easy (59.11%)
 * Likes:    171
 * Dislikes: 0
 * Total Accepted:    110.4K
 * Total Submissions: 186.7K
 * Testcase Example:  '"PPALLP"'
 *
 * 给你一个字符串 s 表示一个学生的出勤记录，其中的每个字符用来标记当天的出勤情况（缺勤、迟到、到场）。记录中只含下面三种字符：
 *
 *
 * 'A'：Absent，缺勤
 * 'L'：Late，迟到
 * 'P'：Present，到场
 *
 *
 * 如果学生能够 同时 满足下面两个条件，则可以获得出勤奖励：
 *
 *
 * 按 总出勤 计，学生缺勤（'A'）严格 少于两天。
 * 学生 不会 存在 连续 3 天或 连续 3 天以上的迟到（'L'）记录。
 *
 *
 * 如果学生可以获得出勤奖励，返回 true ；否则，返回 false 。
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：s = "PPALLP"
 * 输出：true
 * 解释：学生缺勤次数少于 2 次，且不存在 3 天或以上的连续迟到记录。
 *
 *
 * 示例 2：
 *
 *
 * 输入：s = "PPALLL"
 * 输出：false
 * 解释：学生最后三天连续迟到，所以不满足出勤奖励的条件。
 *
 *
 *
 *
 * 提示：
 *
 *
 * 1 <= s.length <= 1000
 * s[i] 为 'A'、'L' 或 'P'
 *
 *
 */

// @lc code=start
using System;
using System.Collections.Generic;

public partial class Solution
{
    /// <summary>
    /// 学生出勤记录 I
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool CheckRecord(string s)
    {
        int absentCount = 0; // 缺勤计数
        for (int index = 0; index < s.Length; index++)
        {
            if (s[index] == 'A')
            {
                absentCount++;
                if (absentCount >= 2) // 缺勤超过1次
                {
                    return false;
                }
            }
            else if (s[index] == 'L')
            {
                if (index + 2 < s.Length && s[index + 1] == 'L' && s[index + 2] == 'L')
                {
                    return false; // 连续迟到超过2次
                }
            }
        }
        return true; // 满足条件
    }
}
// @lc code=end
// 115/115 cases passed (0 ms)
// Your runtime beats 100 % of csharp submissions
// Your memory usage beats 9.52 % of csharp submissions (41.9 MB)
