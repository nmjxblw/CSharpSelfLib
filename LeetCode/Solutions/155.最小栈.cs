/*
 * @lc app=leetcode.cn id=155 lang=csharp
 *
 * [155] 最小栈
 *
 * https://leetcode.cn/problems/min-stack/description/
 *
 * algorithms
 * Medium (61.27%)
 * Likes:    1912
 * Dislikes: 0
 * Total Accepted:    729.1K
 * Total Submissions: 1.2M
 * Testcase Example:  '["MinStack","push","push","push","getMin","pop","top","getMin"]\n' +
  '[[],[-2],[0],[-3],[],[],[],[]]'
 *
 * 设计一个支持 push ，pop ，top 操作，并能在常数时间内检索到最小元素的栈。
 * 
 * 实现 MinStack 类:
 * 
 * 
 * MinStack() 初始化堆栈对象。
 * void push(int val) 将元素val推入堆栈。
 * void pop() 删除堆栈顶部的元素。
 * int top() 获取堆栈顶部的元素。
 * int getMin() 获取堆栈中的最小元素。
 * 
 * 
 * 
 * 
 * 示例 1:
 * 
 * 
 * 输入：
 * ["MinStack","push","push","push","getMin","pop","top","getMin"]
 * [[],[-2],[0],[-3],[],[],[],[]]
 * 
 * 输出：
 * [null,null,null,null,-3,null,0,-2]
 * 
 * 解释：
 * MinStack minStack = new MinStack();
 * minStack.push(-2);
 * minStack.push(0);
 * minStack.push(-3);
 * minStack.getMin();   --> 返回 -3.
 * minStack.pop();
 * minStack.top();      --> 返回 0.
 * minStack.getMin();   --> 返回 -2.
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * -2^31 <= val <= 2^31 - 1
 * pop、top 和 getMin 操作总是在 非空栈 上调用
 * push, pop, top, and getMin最多被调用 3 * 10^4 次
 * 
 * 
 */
using System;
namespace LeetCode
{
    // @lc code=start
    /// <summary>
    /// Represents a stack data structure that supports retrieving the minimum element in constant time.
    /// </summary>
    /// <remarks>In addition to standard stack operations such as <see cref="Push(int)"/> and <see
    /// cref="Pop()"/>,  this class provides a method to retrieve the minimum value in the stack using <see
    /// cref="GetMin()"/>.  The minimum value is updated dynamically as elements are added or removed.</remarks>
    public class MinStack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinStack"/> class, which represents a stack that supports
        /// retrieving the minimum element in constant time.
        /// </summary>
        /// <remarks>The <see cref="MinStack"/> class provides standard stack operations (push, pop, and
        /// peek) along with a method to retrieve the minimum element in the stack.</remarks>
        public MinStack()
        {

        }
        /// <summary>
        /// Adds the specified value to the top of the stack.
        /// </summary>
        /// <remarks>The stack will grow dynamically to accommodate the new value if necessary.</remarks>
        /// <param name="val">The value to push onto the stack.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Push(int val)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Removes the top element from the stack.
        /// </summary>
        /// <remarks>This method modifies the state of the stack by removing its topmost element.  If the
        /// stack is empty, calling this method will result in an exception.</remarks>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Pop()
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Retrieves the top element of the stack without removing it.
        /// </summary>
        /// <returns>The value of the top element in the stack.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int Top()
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Retrieves the minimum value from the collection.
        /// </summary>
        /// <returns>The smallest integer in the collection. If the collection is empty, the behavior is undefined.</returns>
        /// <exception cref="System.NotImplementedException">Thrown when the method is not implemented.</exception>
        public int GetMin()
        {
            throw new System.NotImplementedException();
        }
    }

    /**
     * Your MinStack object will be instantiated and called as such:
     * MinStack obj = new MinStack();
     * obj.Push(val);
     * obj.Pop();
     * int param_3 = obj.Top();
     * int param_4 = obj.GetMin();
     */
    // @lc code=end
}
