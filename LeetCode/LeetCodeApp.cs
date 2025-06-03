using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dopamine.Framework;

namespace LeetCode
{
    /// <summary>
    /// LeetCode的主程序
    /// </summary>
    public class LeetCodeApp
    {
        /// <summary>
        /// 程序主方法
        /// </summary>
        public void Start()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            Solution solution = new Solution();
            // 运行测试
            object type = ConfigManager.Data.SudokuBoard;
            type.GetType().ToString().ShowInConsole(true);
            //char[][] boardCharFormat = new char[boardStringFormat.Count][];
            //for (int i = 0; i < boardStringFormat.Count; i++)
            //{
            //    boardCharFormat[i] = new char[(boardStringFormat[i] as List<string>).Count];
            //    for (int j = 0; j < (boardStringFormat[i] as List<string>).Count; j++)
            //    {
            //        boardCharFormat[i][j] = (boardStringFormat[i] as List<string>)[j][0]; // 只取第一个字符
            //    }
            //}
            //solution.PrintSudokuBoard(boardCharFormat);
            stopwatch.Stop();
            Console.WriteLine($"执行时间：{((double)stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000f):F3}ms");
        }
    }
}
