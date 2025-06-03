/*
 * @lc app=leetcode.cn id=37 lang=csharp
 *
 * [37] 解数独
 *
 * https://leetcode.cn/problems/sudoku-solver/description/
 *
 * algorithms
 * Hard (67.79%)
 * Likes:    1924
 * Dislikes: 0
 * Total Accepted:    288.6K
 * Total Submissions: 425.7K
 * Testcase Example:  '[["5","3",".",".","7",".",".",".","."],["6",".",".","1","9","5",".",".","."],[".","9","8",".",".",".",".","6","."],["8",".",".",".","6",".",".",".","3"],["4",".",".","8",".","3",".",".","1"],["7",".",".",".","2",".",".",".","6"],[".","6",".",".",".",".","2","8","."],[".",".",".","4","1","9",".",".","5"],[".",".",".",".","8",".",".","7","9"]]'
 *
 * 编写一个程序，通过填充空格来解决数独问题。
 * 
 * 数独的解法需 遵循如下规则：
 * 
 * 
 * 数字 1-9 在每一行只能出现一次。
 * 数字 1-9 在每一列只能出现一次。
 * 数字 1-9 在每一个以粗实线分隔的 3x3 宫内只能出现一次。（请参考示例图）
 * 
 * 
 * 数独部分空格内已填入了数字，空白格用 '.' 表示。
 * 
 * 
 * 
 * 
 * 
 * 
 * 示例 1：
 * 
 * 
 * 输入：board =
 * [["5","3",".",".","7",".",".",".","."],["6",".",".","1","9","5",".",".","."],[".","9","8",".",".",".",".","6","."],["8",".",".",".","6",".",".",".","3"],["4",".",".","8",".","3",".",".","1"],["7",".",".",".","2",".",".",".","6"],[".","6",".",".",".",".","2","8","."],[".",".",".","4","1","9",".",".","5"],[".",".",".",".","8",".",".","7","9"]]
 * 
 * 输出：[["5","3","4","6","7","8","9","1","2"],["6","7","2","1","9","5","3","4","8"],["1","9","8","3","4","2","5","6","7"],["8","5","9","7","6","1","4","2","3"],["4","2","6","8","5","3","7","9","1"],["7","1","3","9","2","4","8","5","6"],["9","6","1","5","3","7","2","8","4"],["2","8","7","4","1","9","6","3","5"],["3","4","5","2","8","6","1","7","9"]]
 * 解释：输入的数独如上图所示，唯一有效的解决方案如下所示：
 * 
 * 
 * 
 * 
 * 
 * 
 * 提示：
 * 
 * 
 * board.length == 9
 * board[i].length == 9
 * board[i][j] 是一位数字或者 '.'
 * 题目数据 保证 输入数独仅有一个解
 * 
 * 
 * 
 * 
 * 
 */
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeetCode
{
    // @lc code=start
    public partial class Solution
    {
        /// <summary>
        ///  解数独
        /// </summary>
        /// <param name="board"></param>
        public void SolveSudoku(char[][] board)
        {
            char[][] tempBoard = new char[board.Length][];
            for (int i = 0; i < board.Length; i++)
            {
                tempBoard[i] = new char[board[i].Length];
                for (int j = 0; j < board[i].Length; j++)
                {
                    tempBoard[i][j] = board[i][j];
                }
            }
            char[] fullChars = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
            for (int row = 0; row < board.Length; row++)
            {
                // 获取当前行未使用的字符
                List<char> validChars = fullChars.Where(c => !board[row].Contains(c)).ToList();
                for (int col = 0; col < board[row].Length; col++)
                {

                }
            }
        }
        /// <summary>
        /// 打印数独棋盘
        /// </summary>
        /// <param name="board"></param>
        public void PrintSudokuBoard(char[][] board)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    string block = " ";
                    if (int.TryParse(board[i][j].ToString(),out int temp))
                    {
                        block = temp.ToString();
                    }
                    stringBuilder.Append(block);
                    if (j < board[i].Length - 1)
                    {
                        stringBuilder.Append("|");
                    }
                    if ((j + 1) % 3 == 0 && j < board[i].Length - 1)
                    {
                        stringBuilder.Append("|");
                    }
                }
                stringBuilder.AppendLine(new string('-', board[i].Length * 2 + 1));
                if ((i + 1) % 3 == 0 && i < board.Length - 1)
                {
                    stringBuilder.AppendLine(new string('-', board[i].Length * 2 + 1));
                }
            }
        }
    }
    // @lc code=end
}
