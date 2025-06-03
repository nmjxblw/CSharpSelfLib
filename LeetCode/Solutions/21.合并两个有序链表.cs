/*
 * @lc app=leetcode.cn id=21 lang=csharp
 *
 * [21] 合并两个有序链表
 *
 * https://leetcode.cn/problems/merge-two-sorted-lists/description/
 *
 * algorithms
 * Easy (67.70%)
 * Likes:    3778
 * Dislikes: 0
 * Total Accepted:    2.1M
 * Total Submissions: 3M
 * Testcase Example:  '[1,2,4]\n[1,3,4]'
 *
 * 将两个升序链表合并为一个新的 升序 链表并返回。新链表是通过拼接给定的两个链表的所有节点组成的。
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：l1 = [1,2,4], l2 = [1,3,4]
 * 输出：[1,1,2,3,4,4]
 *
 *
 * 示例 2：
 *
 *
 * 输入：l1 = [], l2 = []
 * 输出：[]
 *
 *
 * 示例 3：
 *
 *
 * 输入：l1 = [], l2 = [0]
 * 输出：[0]
 *
 *
 *
 *
 * 提示：
 *
 *
 * 两个链表的节点数目范围是 [0, 50]
 * -100
 * l1 和 l2 均按 非递减顺序 排列
 *
 *
 */
using System;
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
        /// 合并两个有序链表
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public ListNode MergeTwoLists(ListNode node1, ListNode node2)
        {
            if (node1 == null)
                return node2;
            if (node2 == null)
                return node1;
            if (node1.val <= node2.val)
            {
                node1.next = MergeTwoLists(node1.next, node2);
                return node1;
            }
            else
            {
                node2.next = MergeTwoLists(node1, node2.next);
                return node2;
            }
        }
    }
    // @lc code=end
}