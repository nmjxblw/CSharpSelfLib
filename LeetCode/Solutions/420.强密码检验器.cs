/*
 * @lc app=leetcode.cn id=420 lang=csharp
 *
 * [420] 强密码检验器
 *
 * https://leetcode.cn/problems/strong-password-checker/description/
 *
 * algorithms
 * Hard (38.40%)
 * Likes:    226
 * Dislikes: 0
 * Total Accepted:    19.6K
 * Total Submissions: 51.1K
 * Testcase Example:  '"a"'
 *
 * 满足以下条件的密码被认为是强密码：
 * 
 * 
 * 由至少 6 个，至多 20 个字符组成。
 * 包含至少 一个小写 字母，至少 一个大写 字母，和至少 一个数字 。
 * 不包含连续三个重复字符 (比如 "Baaabb0" 是弱密码, 但是 "Baaba0" 是强密码)。
 * 
 * 
 * 给你一个字符串 password ，返回 将 password 修改到满足强密码条件需要的最少修改步数。如果 password 已经是强密码，则返回 0
 * 。
 * 
 * 在一步修改操作中，你可以：
 * 
 * 
 * 插入一个字符到 password ，
 * 从 password 中删除一个字符，或
 * 用另一个字符来替换 password 中的某个字符。
 * 
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：password = "a"
 * 输出：5
 * 
 * 
 * 示例 2：
 * 
 * 
 * 输入：password = "aA1"
 * 输出：3
 * 
 * 
 * 示例 3：
 * 
 * 
 * 输入：password = "1337C0d3"
 * 输出：0
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 <= password.length <= 50
 * password 由字母、数字、点 '.' 或者感叹号 '!' 组成
 * 
 * 
 */

// @lc code=start
public class Solution {
    public int StrongPasswordChecker(string password) {
        
    }
}
// @lc code=end

