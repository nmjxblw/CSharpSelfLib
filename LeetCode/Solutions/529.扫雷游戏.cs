/*
 * @lc app=leetcode.cn id=529 lang=csharp
 *
 * [529] 扫雷游戏
 *
 * https://leetcode.cn/problems/minesweeper/description/
 *
 * algorithms
 * Medium (64.31%)
 * Likes:    413
 * Dislikes: 0
 * Total Accepted:    62.1K
 * Total Submissions: 96.4K
 * Testcase Example:  '[["E","E","E","E","E"],["E","E","M","E","E"],["E","E","E","E","E"],["E","E","E","E","E"]]\n' +
  '[3,0]'
 *
 * 让我们一起来玩扫雷游戏！
 *
 * 给你一个大小为 m x n 二维字符矩阵 board ，表示扫雷游戏的盘面，其中：
 *
 *
 * 'M' 代表一个 未挖出的 地雷，
 * 'E' 代表一个 未挖出的 空方块，
 * 'B' 代表没有相邻（上，下，左，右，和所有4个对角线）地雷的 已挖出的 空白方块，
 * 数字（'1' 到 '8'）表示有多少地雷与这块 已挖出的 方块相邻，
 * 'X' 则表示一个 已挖出的 地雷。
 *
 *
 * 给你一个整数数组 click ，其中 click = [clickr, clickc] 表示在所有 未挖出的 方块（'M' 或者
 * 'E'）中的下一个点击位置（clickr 是行下标，clickc 是列下标）。
 *
 * 根据以下规则，返回相应位置被点击后对应的盘面：
 *
 *
 * 如果一个地雷（'M'）被挖出，游戏就结束了- 把它改为 'X' 。
 * 如果一个 没有相邻地雷 的空方块（'E'）被挖出，修改它为（'B'），并且所有和其相邻的 未挖出 方块都应该被递归地揭露。
 * 如果一个 至少与一个地雷相邻 的空方块（'E'）被挖出，修改它为数字（'1' 到 '8' ），表示相邻地雷的数量。
 * 如果在此次点击中，若无更多方块可被揭露，则返回盘面。
 *
 *
 *
 *
 * 示例 1：
 *
 *
 * 输入：board =
 * [["E","E","E","E","E"],["E","E","M","E","E"],["E","E","E","E","E"],["E","E","E","E","E"]],
 * click = [3,0]
 *
 * 输出：[["B","1","E","1","B"],["B","1","M","1","B"],["B","1","1","1","B"],["B","B","B","B","B"]]
 *
 *
 * 示例 2：
 *
 *
 * 输入：board =
 * [["B","1","E","1","B"],["B","1","M","1","B"],["B","1","1","1","B"],["B","B","B","B","B"]],
 * click = [1,2]
 *
 * 输出：[["B","1","E","1","B"],["B","1","X","1","B"],["B","1","1","1","B"],["B","B","B","B","B"]]
 *
 *
 *
 *
 * 提示：
 *
 *
 * m == board.length
 * n == board[i].length
 * 1 <= m, n <= 50
 * board[i][j] 为 'M'、'E'、'B' 或数字 '1' 到 '8' 中的一个
 * click.length == 2
 * 0 <= clickr < m
 * 0 <= clickc < n
 * board[clickr][clickc] 为 'M' 或 'E'
 *
 *
 */

// @lc code=start
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Solution
{
    /// <summary>
    /// 扫雷游戏
    /// </summary>
    /// <param name="board"></param>
    /// <param name="click"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public char[][] UpdateBoard(char[][] board, int[] click)
    {
        if (board[click[0]][click[1]].Equals('M'))
        {
            board[click[0]][click[1]] = 'X';
            return board;
        }
        if (
            board[click[0]][click[1]].Equals('B')
            || int.TryParse(board[click[0]][click[1]].ToString(), out _)
        )
        {
            return board;
        }
        /// <summary>
        /// 递归遍历空白方块
        /// <summary/>
        void IterateEmpty(int[] newClick)
        {
            List<int[]> emptyBlockList = new List<int[]>();
            int mCount = 0;
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <= 1; c++)
                {
                    if (r == 0 && c == 0)
                        continue;
                    int nr = newClick[0] + r,
                        nc = newClick[1] + c;
                    if (nr < 0 || nr >= board.Length || nc < 0 || nc >= board[0].Length)
                        continue;
                    if (board[nr][nc].Equals('M'))
                    {
                        mCount++;
                    }
                    else if (board[nr][nc].Equals('E'))
                    {
                        emptyBlockList.Add(new int[] { nr, nc });
                    }
                }
            }
            if (mCount > 0)
            {
                board[newClick[0]][newClick[1]] = mCount.ToString("D1")[0];
            }
            else
            {
                board[newClick[0]][newClick[1]] = 'B';
                foreach (int[] emptyBlock in emptyBlockList)
                {
                    IterateEmpty(emptyBlock);
                }
            }
        }
        IterateEmpty(click);
        return board;
    }

    /// <summary>
    /// 打印扫雷结果
    /// </summary>
    /// <param name="board"></param>
    public void PrintMineSweeperBoard(char[][] board)
    {
        Console.WriteLine(new string('-', board[0].Length * 2 - 1));
        foreach (char[] row in board)
        {
            Console.WriteLine(string.Join("|", row));
            Console.WriteLine(new string('-', row.Length * 2 - 1));
        }
    }
}
// @lc code=end
// 54/54 cases passed (5 ms) [WARN] Failed to get runtime percentile.
