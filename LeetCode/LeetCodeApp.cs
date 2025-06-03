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
            solution.PrintMineSweeperBoard(solution.UpdateBoard(new char[][] { new char[] { 'E', 'E', 'E', 'E', 'E' }, new char[] { 'E', 'E', 'M', 'E', 'E' }, new char[] { 'E', 'E', 'E', 'E', 'E' }, new char[] { 'E', 'E', 'E', 'E', 'E' } }, new int[] { 3, 0 }));
            stopwatch.Stop();
            Console.WriteLine($"执行时间：{((double)stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000f):F3}ms");
        }
    }
}
