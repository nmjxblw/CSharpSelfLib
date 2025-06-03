using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LeetCode
{
    /// <summary>
    /// 树节点
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// 节点值
        /// </summary>
        public int val;
        /// <summary>
        /// 左子节点
        /// </summary>
        public TreeNode left;
        /// <summary>
        /// 右子节点
        /// </summary>
        public TreeNode right;
        /// <summary>
        /// 树节点构造函数
        /// </summary>
        /// <param name="val"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public TreeNode(int val = 0, TreeNode left = null, TreeNode right = null)
        {
            this.val = val;
            this.left = left;
            this.right = right;
        }
    }
}