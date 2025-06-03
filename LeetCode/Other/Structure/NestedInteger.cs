using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LeetCode
{
    /// <summary>
    /// 嵌套整型接口
    /// </summary>
    public interface INestedInteger
    {

        /// <summary>
        /// return true if this NestedInteger holds a single integer, rather than a nested list.
        /// </summary>
        /// <returns></returns>
        bool IsInteger();

        /// <summary>
        /// return the single integer that this NestedInteger holds, if it holds a single integer
        /// Return null if this NestedInteger holds a nested list
        /// </summary>
        /// <returns></returns>

        int GetInteger();

        /// <summary>
        /// return the nested list that this NestedInteger holds, if it holds a nested list, Return null if this NestedInteger holds a single integer
        /// </summary>
        /// <returns></returns>
        IList<INestedInteger> GetList();
    }
    /// <summary>
    /// 嵌套整型
    /// </summary>
    public class NestedInteger : INestedInteger
    {
        private object _holdValue = new object();
        /// <summary>
        /// 嵌套整型构造函数
        /// </summary>
        /// <param name="input"></param>
        public NestedInteger(object input)
        {
            input = this._holdValue;
        }
        /// <summary>
        /// 返回整型
        /// </summary>
        /// <returns></returns>
        public int GetInteger()
        {
            return default;
        }
        /// <summary>
        /// 返回列表
        /// </summary>
        /// <returns></returns>
        public IList<INestedInteger> GetList()
        {
            return default;
        }
        /// <summary>
        /// 是否为整数
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsInteger()
        {
            return false;
        }
    }
}