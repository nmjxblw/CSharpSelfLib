/*
 * @lc app=leetcode.cn id=23 lang=csharp
 *
 * [23] 合并 K 个升序链表
 *
 * https://leetcode.cn/problems/merge-k-sorted-lists/description/
 *
 * algorithms
 * Hard (61.79%)
 * Likes:    2996
 * Dislikes: 0
 * Total Accepted:    983.3K
 * Total Submissions: 1.6M
 * Testcase Example:  '[[1,4,5],[1,3,4],[2,6]]'
 *
 * 给你一个链表数组，每个链表都已经按升序排列。
 * 
 * 请你将所有链表合并到一个升序链表中，返回合并后的链表。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 输入：lists = [[1,4,5],[1,3,4],[2,6]]
 * 输出：[1,1,2,3,4,4,5,6]
 * 解释：链表数组如下：
 * [
 * ⁠ 1->4->5,
 * ⁠ 1->3->4,
 * ⁠ 2->6
 * ]
 * 将它们合并到一个有序链表中得到。
 * 1->1->2->3->4->4->5->6
 * 
 * 
 * 示例 2：
 * 
 * 输入：lists = []
 * 输出：[]
 * 
 * 
 * 示例 3：
 * 
 * 输入：lists = [[]]
 * 输出：[]
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * k == lists.length
 * 0 <= k <= 10^4
 * 0 <= lists[i].length <= 500
 * -10^4 <= lists[i][j] <= 10^4
 * lists[i] 按 升序 排列
 * lists[i].length 的总和不超过 10^4
 * 
 * 
 */
using System;
using System.Linq;
using System.Collections.Generic;
namespace LeetCode
{
    // @lc code=start
    /**
     * Definition for singly-linked list.
     * public class ListNode {
     *     public int val;
     *     public ListNode next;
     *     public ListNode(int val=0, ListNode next=null) {
     *         this.val = val;
     *         this.next = next;
     *     }
     * }
     */

    public partial class Solution
    {
        /// <summary>
        /// 合并 K 个升序链表
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ListNode MergeKLists(ListNode[] lists)
        {
            ListNode tempMin = lists[0];
            for (int i = 0; i < lists.Length; i++)
            {
                if (lists[i] == null)
                {
                    return lists[i];
                }
                if (tempMin.val <= lists[i].val)
                {
                    tempMin = lists[i];
                }
            }
            tempMin.next = MergeKLists(lists.Where(x => x != tempMin).ToArray());
            return MergeKLists(lists);
        }
    }
    // @lc code=end
}
