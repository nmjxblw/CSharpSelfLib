using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetCode
{
	/// <summary>
	/// 
	/// </summary>
	public class ListNode
	{
		/// <summary>
		/// 
		/// </summary>
		public int val;
		/// <summary>
		/// 
		/// </summary>
		public ListNode next;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <param name="next"></param>
		public ListNode(int val = 0, ListNode next = null)
		{
			this.val = val;
			this.next = next;
		}
	}
}
