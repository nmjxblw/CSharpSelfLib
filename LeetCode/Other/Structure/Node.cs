using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 图节点
/// </summary>
public class Node
{
    /// <summary>
    /// 节点值
    /// </summary>
    public int val;
    /// <summary>
    /// 邻接节点
    /// </summary>
    public IList<Node> neighbors;
    /// <summary>
    /// 图节点构造函数
    /// </summary>
    public Node()
    {
        val = 0;
        neighbors = new List<Node>();
    }
    /// <summary>
    /// 图节点构造函数
    /// </summary>
    /// <param name="_val"></param>
    public Node(int _val)
    {
        val = _val;
        neighbors = new List<Node>();
    }
    /// <summary>
    /// 图节点构造函数
    /// </summary>
    /// <param name="_val"></param>
    /// <param name="_neighbors"></param>
    public Node(int _val, List<Node> _neighbors)
    {
        val = _val;
        neighbors = _neighbors;
    }
}
