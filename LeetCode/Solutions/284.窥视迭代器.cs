/*
 * @lc app=leetcode.cn id=284 lang=csharp
 *
 * [284] 窥视迭代器
 *
 * https://leetcode.cn/problems/peeking-iterator/description/
 *
 * algorithms
 * Medium (76.44%)
 * Likes:    216
 * Dislikes: 0
 * Total Accepted:    36.8K
 * Total Submissions: 48.2K
 * Testcase Example:  '["PeekingIterator","next","peek","next","next","hasNext"]\n' +
  '[[[1,2,3]],[],[],[],[],[]]'
 *
 * 请你在设计一个迭代器，在集成现有迭代器拥有的 hasNext 和 next 操作的基础上，还额外支持 peek 操作。
 * 
 * 实现 PeekingIterator 类：
 * 
 * 
 * PeekingIterator(Iterator<int> nums) 使用指定整数迭代器 nums 初始化迭代器。
 * int next() 返回数组中的下一个元素，并将指针移动到下个元素处。
 * bool hasNext() 如果数组中存在下一个元素，返回 true ；否则，返回 false 。
 * int peek() 返回数组中的下一个元素，但 不 移动指针。
 * 
 * 
 * 注意：每种语言可能有不同的构造函数和迭代器 Iterator，但均支持 int next() 和 boolean hasNext() 函数。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：
 * ["PeekingIterator", "next", "peek", "next", "next", "hasNext"]
 * [[[1, 2, 3]], [], [], [], [], []]
 * 输出：
 * [null, 1, 2, 2, 3, false]
 * 
 * 解释：
 * PeekingIterator peekingIterator = new PeekingIterator([1, 2, 3]); // [1,2,3]
 * peekingIterator.next();    // 返回 1 ，指针移动到下一个元素 [1,2,3]
 * peekingIterator.peek();    // 返回 2 ，指针未发生移动 [1,2,3]
 * peekingIterator.next();    // 返回 2 ，指针移动到下一个元素 [1,2,3]
 * peekingIterator.next();    // 返回 3 ，指针移动到下一个元素 [1,2,3]
 * peekingIterator.hasNext(); // 返回 False
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 <= nums.length <= 1000
 * 1 <= nums[i] <= 1000
 * 对 next 和 peek 的调用均有效
 * next、hasNext 和 peek 最多调用  1000 次
 * 
 * 
 * 
 * 
 * 进阶：你将如何拓展你的设计？使之变得通用化，从而适应所有的类型，而不只是整数型？
 * 
 */
using System;
using System.Collections.Generic;
namespace LeetCode
{
    // @lc code=start
    // C# IEnumerator interface reference:
    // https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerator?view=netframework-4.8
    /// <summary>
    /// 窥视迭代器
    /// </summary>
    class PeekingIterator
    {
        // iterators refers to the first element of the array.
        public PeekingIterator(System.Collections.Generic.IEnumerator<int> iterator)
        {
            // initialize any member here.
        }

        // Returns the next element in the iteration without advancing the iterator.
        public int Peek()
        {
            throw new System.NotImplementedException();
        }

        // Returns the next element in the iteration and advances the iterator.
        public int Next()
        {
            throw new System.NotImplementedException();
        }

        // Returns false if the iterator is refering to the end of the array of true otherwise.
        public bool HasNext()
        {
            throw new System.NotImplementedException();
        }
    }
    // @lc code=end
}
