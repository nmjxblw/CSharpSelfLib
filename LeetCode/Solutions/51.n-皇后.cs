/*
 * @lc app=leetcode.cn id=51 lang=csharp
 *
 * [51] N 皇后
 *
 * https://leetcode.cn/problems/n-queens/description/
 *
 * algorithms
 * Hard (75.29%)
 * Likes:    2289
 * Dislikes: 0
 * Total Accepted:    524.5K
 * Total Submissions: 696.7K
 * Testcase Example:  '4'
 *
 * 按照国际象棋的规则，皇后可以攻击与之处在同一行或同一列或同一斜线上的棋子。
 * 
 * n 皇后问题 研究的是如何将 n 个皇后放置在 n×n 的棋盘上，并且使皇后彼此之间不能相互攻击。
 * 
 * 给你一个整数 n ，返回所有不同的 n 皇后问题 的解决方案。
 * 
 * 
 * 
 * 每一种解法包含一个不同的 n 皇后问题 的棋子放置方案，该方案中 'Q' 和 '.' 分别代表了皇后和空位。
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：n = 4
 * 输出：[[".Q..","...Q","Q...","..Q."],["..Q.","Q...","...Q",".Q.."]]
 * 解释：如上图所示，4 皇后问题存在两个不同的解法。
 * 
 * 
 * 示例 2：
 * 
 * 
 * 输入：n = 1
 * 输出：[["Q"]]
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * 1 <= n <= 9
 * 
 * 
 * 
 * 
 */

// @lc code=start
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

public partial class Solution
{
    /// <summary>
    /// N 皇后问题
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public IList<IList<string>> SolveNQueens(int n)
    {
        IList<IList<string>> result = new List<IList<string>>();
        // 处理特殊情况
        if (n <= 1)
        {
            if (n == 1)
            {
                result.Add(new List<string>() { "Q" });
            }
            return result;
        }
        string solutionString = string.Empty;
        Stack<(int, int)> QueenStack = new Stack<(int, int)>();
#if true


        //内置递归方法
        void TryAddPoint((int, int) input)
        {
            try
            {
                // 输入了超出最大行数范围的点
                // 一般不太可能发生
                if (input.Item1 >= n) return;
                // 输入了列数不合法的点,
                // 即上一个点的解不正确
                if (input.Item2 >= n)
                {
                    // 判断是否能回退
                    if (QueenStack.Count >= 1)
                    {
                        // 上一个点的列数+1
                        (int, int) lastPoint = QueenStack.Pop();
                        TryAddPoint((lastPoint.Item1, lastPoint.Item2 + 1));
                    }
                    // 当前行为第一行，已经尝试所有列了，无解
                    // 终止当前操作
                    return;
                }
                // 检测行列合法性
                foreach ((int, int) existPoint in QueenStack)
                {
                    if (input.Item1 == existPoint.Item1 ||
                        input.Item2 == existPoint.Item2 ||
                        Math.Abs(input.Item1 - existPoint.Item1) == Math.Abs(input.Item2 - existPoint.Item2))
                    {
                        // 行列不合法，当前点不合法
                        // 移动到下一个点
                        List<int> validCol = new List<int>();
                        for (int i = 0; i < n; i++)
                        {
                            if (i > input.Item2)
                                validCol.Add(i);
                        }
                        foreach ((int, int) p in QueenStack)
                        {
                            // 剔除重复列
                            if (validCol.Contains(p.Item2))
                            {
                                validCol.Remove(p.Item2);
                            }
                            int offset = Math.Abs(input.Item1 - p.Item1);
                            validCol.RemoveAll(x => Math.Abs(x - p.Item2) == offset);
                        }
                        validCol.Sort();
                        if (validCol.Count > 0)
                            TryAddPoint((input.Item1, validCol[0]));
                        else if (QueenStack.Count > 0)
                        {
                            // 上一个点的列数+1
                            (int, int) lastPoint = QueenStack.Pop();
                            TryAddPoint((lastPoint.Item1, lastPoint.Item2 + 1));
                        }
                        else
                            return;
                    }
                }
                // 合法性通过
                // 记录当前点
                QueenStack.Push(input);
                //Console.Clear();
                //IList<StringBuilder> _bordSolution = GetEmptyBoard(n);
                //foreach ((int, int) point in QueenStack)
                //{
                //    _bordSolution[point.Item1][point.Item2] = 'Q';
                //}
                //foreach (StringBuilder rowStringBuilder in _bordSolution)
                //{
                //    Console.Write("|");
                //    foreach (char pos in rowStringBuilder.ToString())
                //    {
                //        Console.Write($"{pos}|");
                //    }
                //    Console.WriteLine();
                //}

                //Thread.Sleep(1000);
                if (QueenStack.Count < n)
                {
                    // 还未找到一组完整的解
                    // 继续寻找下一个点
                    List<int> validCol = new List<int>();
                    for (int i = 0; i < n; i++)
                    {
                        validCol.Add(i);
                    }
                    foreach ((int, int) p in QueenStack)
                    {
                        if (validCol.Contains(p.Item2))
                        {
                            validCol.Remove(p.Item2);
                        }
                    }
                    validCol.Sort();
                    TryAddPoint((input.Item1 + 1, validCol[0]));
                    //TryAddPoint((input.Item1 + 1, 0));
                    return;
                }
                else if (QueenStack.Count == n)
                {
                    // 找到了一组完整的解
                    // 记录当前解
                    IList<StringBuilder> bordSolution = GetEmptyBoard(n);
                    foreach ((int, int) point in QueenStack)
                    {
                        bordSolution[point.Item1][point.Item2] = 'Q';
                    }
                    IList<string> boardResult = new List<string>();
                    foreach (StringBuilder sb in bordSolution)
                    {
                        boardResult.Add(sb.ToString());
                    }
                    result.Add(boardResult);
                    // 继续寻找下一个解
                    if (QueenStack.Count > 0)
                    {
                        // 移除当前的解
                        (int, int) lastPoint = QueenStack.Pop();
                        List<int> validCol = new List<int>();
                        for (int i = 0; i < n; i++)
                        {
                            if (i > input.Item2)
                                validCol.Add(i);
                        }
                        foreach ((int, int) p in QueenStack)
                        {
                            // 剔除重复列
                            if (validCol.Contains(p.Item2))
                            {
                                validCol.Remove(p.Item2);
                            }
                            int offset = Math.Abs(input.Item1 - p.Item1);
                            validCol.RemoveAll(x => Math.Abs(x - p.Item2) == offset);
                        }
                        validCol.Sort();
                        if (validCol.Count > 0)
                        {
                            TryAddPoint((lastPoint.Item1, validCol[0]));
                        }
                        else
                        {
                            TryAddPoint((lastPoint.Item1, lastPoint.Item2 + 1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                solutionString = string.Join(",", QueenStack.ToArray());
                Console.WriteLine(solutionString);
                Console.WriteLine(ex.ToString());
                throw ex;
            }
            return;
        }
        TryAddPoint((0, 0));
#endif
        return result;
    }


    /// <summary>
    /// 获取一个空的棋盘
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public IList<StringBuilder> GetEmptyBoard(int n)
    {
        IList<StringBuilder> emptyBoard = new List<StringBuilder>();
        for (int i = 0; i < n; i++)
        {
            StringBuilder rowSb = new StringBuilder(new string('_', n));
            emptyBoard.Add(rowSb);
        }
        return emptyBoard;
    }
    /// <summary>
    /// 将棋盘转化为字符串列表
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public List<StringBuilder> ToStringList(bool[][] board)
    {
        List<StringBuilder> stringList = new List<StringBuilder>();
        foreach (bool[] row in board)
        {
            StringBuilder sb = new StringBuilder();
            foreach (bool pos in row)
            {
                sb.Append(pos ? "Q" : ".");
            }
            stringList.Append(sb);
        }
        return stringList;
    }
    /// <summary>
    /// 获取一个新的行
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public StringBuilder NewRow(int n)
    {
        return new StringBuilder(new string('.', n));
    }
    /// <summary>
    /// 打印N皇后问题的解
    /// </summary>
    /// <param name="n">棋盘尺寸</param>
    public void PrintSolveNQueens(int n)
    {
        int count = 1;
        foreach (IList<string> solutionStringList in SolveNQueens(n))
        {
            Console.WriteLine($"{count}.");
            foreach (string rowString in solutionStringList)
            {
                Console.Write("|");
                foreach (char pos in rowString)
                {
                    Console.Write($"{pos}|");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            count++;
        }
    }
}
// @lc code=end

