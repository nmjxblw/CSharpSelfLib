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
using System.Data;
using System.Linq;
using System.Text;

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
			List<StringBuilder> board1 = GetEmptyBoard(n);
			// 镜像解
			List<StringBuilder> board2 = GetEmptyBoard(n);
			bool hasSolution = true;
			board1[0][i] = board2[0][n - 1 - i] = 'Q';
			// 从第二行开始枚举
			for (int row = 1; row < n; row++)
			{
				// 从第row行第col列开始枚举
				for (int col = 0; col < n; col++)
				{
					// 皇后棋子位置合法标识符
					bool isValid = true;
					#region 合法性检测算法
					// 与第一行皇后在同一列上，跳过
					if (col == i) continue;
					// 从第0列开始检测合法性
					for (int j = 0; j < col; j++)
					{
						// 计算偏移量
						int offset = Math.Abs(col - j);
						// 以棋子为原点建立坐标系，第三以及第四象限默认合法。
						// 检测第二象限是否合法
						if (row - offset >= 0 && board1[row - offset][j] == 'Q')
						{
							isValid = false;
							break;
						}
						// 检测第一象限是否合法
						if (row + offset <= n - 1 && board1[row + offset][n - 1 - j] == 'Q')
						{
							isValid = false;
							break;
						}
					}
					// 检测y轴是否合法
					for (int k = 0; k < row; k++)
					{
						if (board1[k][col] == 'Q')
						{
							isValid = false;
							break;
						}
					}
					// 默认x轴没有棋子，x轴默认合法
					#endregion
					#region 处理合法性结果
					// 合法性通过，当前位置可以放置皇后
					// 计算下一行皇后的位置
					if (isValid)
					{
						if (row == n - 1)
						{
							// 当前皇后为最后一行
							// 找到了一组解，将其记录到结果中
							board1[row][col] = 'Q';
							board2[row][n - 1 - col] = 'Q';
							List<string> boardResult1 = new List<string>();
							List<string> boardResult2 = new List<string>();

							for (int s = 0; s < n; s++)
							{
								boardResult1.Add(board1[s].ToString());
								boardResult2.Add(board2[s].ToString());
							}
							result.Add(boardResult1);
							result.Add(boardResult2);
							// 记录完以后寻找下一组解
							board1[row] = new StringBuilder(new string('.', n));
							board2[row] = new StringBuilder(new string('.', n));
							row = row - 1;
							col = board1[row].ToString().IndexOf('Q');
							board1[row] = new StringBuilder(new string('.', n));
							board2[row] = new StringBuilder(new string('.', n));
							col++;
							continue;
						}
						else
						{
							// 找到了当前行的一个解
							// 记录棋盘，并移动到下一行
							board1[row][col] = 'Q';
							board2[row][n - 1 - col] = 'Q';
							break;
						}
					}
					else
					{
						// 检测是否为最终列
						if (col < n - 1)
						{
							// 不为最终列，说明本行还存在潜在解
							continue;
						}
						// 当前无解，即上一行皇后的位置不合法
						// 需要回溯到上一个皇后的位置
						if (row - 1 >= 1)
						{
							board1[row] = new StringBuilder(new string('.', n));
							board2[row] = new StringBuilder(new string('.', n));
							row = row - 1;
							col = board1[row].ToString().IndexOf('Q');
							board1[row] = new StringBuilder(new string('.', n));
							board2[row] = new StringBuilder(new string('.', n));
							col++;
							continue;
						}
						// 不可回溯，无解，跳出循环
						break;
					}
					#endregion
				}
				//// 当前行无解，说明上一行皇后的位置不对，
				//// 回溯上一个答案
				//if (!hasSolution)
				//{
				//	if (row - 1 >= 1)
				//	{
				//		board1[row] = new StringBuilder(new string('.', n));
				//		board2[row] = new StringBuilder(new string('.', n));
				//		row = row - 1;
				//		board1[row] = new StringBuilder(new string('.', n));
				//		board2[row] = new StringBuilder(new string('.', n));
				//		hasSolution = true;
				//		continue;
				//	}
				//	// 已经是第二行了，
				//	// 说明起始皇后落子无解，跳出循环
				//	break;
				//}
			}
		}
		return (IList<IList<string>>)result;
	}
	/// <summary>
	/// 获取一个空的棋盘
	/// </summary>
	/// <param name="n"></param>
	/// <returns></returns>
	public List<StringBuilder> GetEmptyBoard(int n)
	{
		List<StringBuilder> emptyBoard = new List<StringBuilder>();
		for (int i = 0; i < n; i++)
		{
			StringBuilder rowSb = new StringBuilder(new string('.', n));
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
}
// @lc code=end

