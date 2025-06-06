/*
 * @lc app=leetcode.cn id=1032 lang=csharp
 *
 * [1032] 字符流
 *
 * https://leetcode.cn/problems/stream-of-characters/description/
 *
 * algorithms
 * Hard (56.91%)
 * Likes:    183
 * Dislikes: 0
 * Total Accepted:    21K
 * Total Submissions: 37K
 * Testcase Example:  '["StreamChecker","query","query","query","query","query","query","query","query","query","query","query","query"]\n' +
  '[[["cd","f","kl"]],["a"],["b"],["c"],["d"],["e"],["f"],["g"],["h"],["i"],["j"],["k"],["l"]]'
 *
 * 设计一个算法：接收一个字符流，并检查这些字符的后缀是否是字符串数组 words 中的一个字符串。
 * 
 * 例如，words = ["abc", "xyz"] 且字符流中逐个依次加入 4 个字符 'a'、'x'、'y' 和 'z'
 * ，你所设计的算法应当可以检测到 "axyz" 的后缀 "xyz" 与 words 中的字符串 "xyz" 匹配。
 * 
 * 按下述要求实现 StreamChecker 类：
 * 
 * 
 * StreamChecker(String[] words) ：构造函数，用字符串数组 words 初始化数据结构。
 * boolean query(char letter)：从字符流中接收一个新字符，如果字符流中的任一非空后缀能匹配 words 中的某一字符串，返回
 * true ；否则，返回 false。
 * 
 * 
 * 
 * 
 * 示例：
 * 
 * 
 * 输入：
 * ["StreamChecker", "query", "query", "query", "query", "query", "query",
 * "query", "query", "query", "query", "query", "query"]
 * [[["cd", "f", "kl"]], ["a"], ["b"], ["c"], ["d"], ["e"], ["f"], ["g"],
 * ["h"], ["i"], ["j"], ["k"], ["l"]]
 * 输出：
 * [null, false, false, false, true, false, true, false, false, false, false,
 * false, true]
 * 
 * 解释：
 * StreamChecker streamChecker = new StreamChecker(["cd", "f", "kl"]);
 * streamChecker.query("a"); // 返回 False
 * streamChecker.query("b"); // 返回 False
 * streamChecker.query("c"); // 返回n False
 * streamChecker.query("d"); // 返回 True ，因为 'cd' 在 words 中
 * streamChecker.query("e"); // 返回 False
 * streamChecker.query("f"); // 返回 True ，因为 'f' 在 words 中
 * streamChecker.query("g"); // 返回 False
 * streamChecker.query("h"); // 返回 False
 * streamChecker.query("i"); // 返回 False
 * streamChecker.query("j"); // 返回 False
 * streamChecker.query("k"); // 返回 False
 * streamChecker.query("l"); // 返回 True ，因为 'kl' 在 words 中
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 <= words.length <= 2000
 * 1 <= words[i].length <= 200
 * words[i] 由小写英文字母组成
 * letter 是一个小写英文字母
 * 最多调用查询 4 * 10^4 次
 * 
 * 
 */
namespace LeetCode
{
    // @lc code=start
    /// <summary>
    /// 字符流
    /// </summary>
    public partial class StreamChecker
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="words"></param>
        public StreamChecker(string[] words)
        {

        }
        /// <summary>
        /// 查询字符
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public bool Query(char letter)
        {
            return default;
        }
    }

    /**
     * Your StreamChecker object will be instantiated and called as such:
     * StreamChecker obj = new StreamChecker(words);
     * bool param_1 = obj.Query(letter);
     */
    // @lc code=end
}
