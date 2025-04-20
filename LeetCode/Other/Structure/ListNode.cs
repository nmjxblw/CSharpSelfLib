using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
///  链表节点
/// </summary>
public class ListNode
{
	/// <summary>
	/// 链表节点值
	/// </summary>
	public int val;
	/// <summary>
	/// 下一节点
	/// </summary>
	public ListNode next;
	/// <summary>
	/// 链表节点构造函数
	/// </summary>
	/// <param name="val"></param>
	/// <param name="next"></param>
	public ListNode(int val = 0, ListNode next = null)
	{
		this.val = val;
		this.next = next;
	}
}
