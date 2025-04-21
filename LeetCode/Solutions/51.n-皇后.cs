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
using System.Collections.Generic;
using System.ComponentModel;

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
		List<List<string>> result = new List<List<string>>();
		// 处理特殊情况
		if (n <= 1)
		{
			if (n == 1)
			{
				result.Add(new List<string>() { "Q" });
			}
			return (IList<IList<string>>)result;
		}
		// 计算对称轴
		int count = n / 2 + n % 2;
		for (int i = 0; i < count; i++)
		{
			// 初始解法
			bool[][] board1 = GetEmptyBoard(n);
			// 镜像解
			bool[][] board2 = GetEmptyBoard(n);
			bool hasSoultion = true;
			board1[i][0] = board2[n - 1 - i][0] = true;
			for (int row = 1; row < n; row++)
			{
				for (int col = 0; col < n; col++)
				{
					bool isValid = true;
					for(int j = 0; j < n; j++)
					{

					}
				}

				if (!hasSoultion)
				{
					continue;
				}
			}
		}
		return (IList<IList<string>>)result;
	}
	/// <summary>
	/// 获取一个空的棋盘
	/// </summary>
	/// <param name="n"></param>
	/// <returns></returns>
	public bool[][] GetEmptyBoard(int n)
	{
		bool[][] emptyBoard = new bool[n][];
		for (int i = 0; i < n; i++)
		{
			emptyBoard[i] = new bool[n];
		}
		return emptyBoard;
	}
}
// @lc code=end

