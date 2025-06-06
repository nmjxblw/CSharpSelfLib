/*
 * @lc app=leetcode.cn id=140 lang=csharp
 *
 * [140] 单词拆分 II
 *
 * https://leetcode.cn/problems/word-break-ii/description/
 *
 * algorithms
 * Hard (60.25%)
 * Likes:    781
 * Dislikes: 0
 * Total Accepted:    109.4K
 * Total Submissions: 181.5K
 * Testcase Example:  '"catsanddog"\n["cat","cats","and","sand","dog"]'
 *
 * 给定一个字符串 s 和一个字符串字典 wordDict ，在字符串 s 中增加空格来构建一个句子，使得句子中所有的单词都在词典中。以任意顺序
 * 返回所有这些可能的句子。
 * 
 * 注意：词典中的同一个单词可能在分段中被重复使用多次。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入:s = "catsanddog", wordDict = ["cat","cats","and","sand","dog"]
 * 输出:["cats and dog","cat sand dog"]
 * 
 * 
 * 示例 2：
 * 
 * 
 * 输入:s = "pineapplepenapple", wordDict =
 * ["apple","pen","applepen","pine","pineapple"]
 * 输出:["pine apple pen apple","pineapple pen apple","pine applepen apple"]
 * 解释: 注意你可以重复使用字典中的单词。
 * 
 * 
 * 示例 3：
 * 
 * 
 * 输入:s = "catsandog", wordDict = ["cats","dog","sand","and","cat"]
 * 输出:[]
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 
 * 
 * 1 <= s.length <= 20
 * 1 <= wordDict.length <= 1000
 * 1 <= wordDict[i].length <= 10
 * s 和 wordDict[i] 仅有小写英文字母组成
 * wordDict 中所有字符串都 不同
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
        /// 单词拆分 II
        /// </summary>
        /// <param name="s"></param>
        /// <param name="wordDict"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public System.Collections.Generic.IList<string> WordBreak(string s, System.Collections.Generic.IList<string> wordDict)
        {
            throw new System.NotImplementedException();
        }
    }
    // @lc code=end

}