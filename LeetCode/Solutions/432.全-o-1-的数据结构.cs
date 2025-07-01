/*
 * @lc app=leetcode.cn id=432 lang=csharp
 *
 * [432] 全 O(1) 的数据结构
 *
 * https://leetcode.cn/problems/all-oone-data-structure/description/
 *
 * algorithms
 * Hard (46.32%)
 * Likes:    331
 * Dislikes: 0
 * Total Accepted:    32.3K
 * Total Submissions: 69.7K
 * Testcase Example:  '["AllOne","inc","inc","getMaxKey","getMinKey","inc","getMaxKey","getMinKey"]\n' +
  '[[],["hello"],["hello"],[],[],["leet"],[],[]]'
 *
 * 请你设计一个用于存储字符串计数的数据结构，并能够返回计数最小和最大的字符串。
 *
 * 实现 AllOne 类：
 *
 *
 * AllOne() 初始化数据结构的对象。
 * inc(String key) 字符串 key 的计数增加 1 。如果数据结构中尚不存在 key ，那么插入计数为 1 的 key 。
 * dec(String key) 字符串 key 的计数减少 1 。如果 key 的计数在减少后为 0 ，那么需要将这个 key
 * 从数据结构中删除。测试用例保证：在减少计数前，key 存在于数据结构中。
 * getMaxKey() 返回任意一个计数最大的字符串。如果没有元素存在，返回一个空字符串 "" 。
 * getMinKey() 返回任意一个计数最小的字符串。如果没有元素存在，返回一个空字符串 "" 。
 *
 *
 * 注意：每个函数都应当满足 O(1) 平均时间复杂度。
 *
 *
 *
 * 示例：
 *
 *
 * 输入
 * ["AllOne", "inc", "inc", "getMaxKey", "getMinKey", "inc", "getMaxKey",
 * "getMinKey"]
 * [[], ["hello"], ["hello"], [], [], ["leet"], [], []]
 * 输出
 * [null, null, null, "hello", "hello", null, "hello", "leet"]
 *
 * 解释
 * AllOne allOne = new AllOne();
 * allOne.inc("hello");
 * allOne.inc("hello");
 * allOne.getMaxKey(); // 返回 "hello"
 * allOne.getMinKey(); // 返回 "hello"
 * allOne.inc("leet");
 * allOne.getMaxKey(); // 返回 "hello"
 * allOne.getMinKey(); // 返回 "leet"
 *
 *
 *
 *
 * 提示：
 *
 *
 * 1 <= key.length <= 10
 * key 由小写英文字母组成
 * 测试用例保证：在每次调用 dec 时，数据结构中总存在 key
 * 最多调用 inc、dec、getMaxKey 和 getMinKey 方法 5 * 10^4 次
 *
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeetCode
{
    // @lc code=start
    /// <summary>
    ///  全 O(1) 的数据结构
    /// </summary>
    /// <remarks>
    /// 以次数为索引的
    /// </remarks>
    public class AllOne
    {
        /// <summary>
        /// 字符串计数的字典
        /// </summary>
        /// <remarks>
        /// 键为字符串，值为该字符串的计数
        /// </remarks>
        private readonly Dictionary<string, int> _privateDic = new Dictionary<string, int>();

        /// <summary>
        /// 最大计数字符串
        /// </summary>
        private string _maxKey = string.Empty;

        /// <summary>
        /// 最小计数字符串
        /// </summary>
        private string _minKey = string.Empty;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public AllOne()
        {
            this._privateDic.Clear();
        }

        /// <summary>
        /// 字符串 key 的计数增加 1 。如果数据结构中尚不存在 key ，那么插入计数为 1 的 key 。
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Inc(string key)
        {
            try
            {
                int value = (_privateDic[key] = _privateDic[key] + 1);
            }
            catch (KeyNotFoundException)
            {
                // 如果 key 不存在，则添加 key 并设置计数为 1
                _privateDic.Add(key, 1);
            }
            UpdateMaxMinKeys();
        }

        /// <summary>
        /// 字符串 key 的计数减少 1 。如果 key 的计数在减少后为 0 ，那么需要将这个 key 从数据结构中删除。测试用例保证：在减少计数前，key 存在于数据结构中。
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Dec(string key)
        {
            try
            {
                int value = (_privateDic[key] -= 1);
                if (value <= 0)
                {
                    _privateDic.Remove(key);
                }
            }
            catch (KeyNotFoundException)
            {
                return;
            }
            UpdateMaxMinKeys();
        }

        /// <summary>
        /// 返回任意一个计数最大的字符串。如果没有元素存在，返回一个空字符串 "" 。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetMaxKey()
        {
            return _maxKey;
        }

        /// <summary>
        /// 返回任意一个计数最小的字符串。如果没有元素存在，返回一个空字符串 "" 。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetMinKey()
        {
            return _minKey;
        }

        private void UpdateMaxMinKeys()
        {
            if (_privateDic.Count == 0)
            {
                _maxKey = string.Empty;
                _minKey = string.Empty;
                return;
            }
            _maxKey = _privateDic.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            _minKey = _privateDic.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
        }
    }

    /**
     * Your AllOne object will be instantiated and called as such:
     * AllOne obj = new AllOne();
     * obj.Inc(key);
     * obj.Dec(key);
     * string param_3 = obj.GetMaxKey();
     * string param_4 = obj.GetMinKey();
     */
    // @lc code=end
}
