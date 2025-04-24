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
			stopwatch.Start();
			Solution solution = new Solution();
			// 运行测试
			int input = 5;
			IList<IList<string>> solutionList = solution.SolveNQueens(input);
			int count = 1;
			foreach(IList<string> solutionStringList in solutionList)
			{
				Console.WriteLine(count + ".");
				foreach(string rowString in solutionStringList)
				{
					foreach(char pos in rowString)
					{
						Console.Write('|'+pos+'|');
					}
					Console.WriteLine();
				}
				Console.WriteLine();
			}
			stopwatch.Stop();
			Console.WriteLine($"执行时间：{stopwatch.ElapsedMilliseconds}ms");
			Console.ReadKey();
		}
	}
}
