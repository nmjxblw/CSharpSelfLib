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
	public class MainApplication
	{
		/// <summary>
		/// 程序主方法
		/// </summary>
		public void Start()
		{
			//"任务开始".ShowInConsole(true);
			//Stopwatch stopwatch = Stopwatch.StartNew();
			//Thread.Sleep(3000);
			//stopwatch.Stop();
			//Thread.Sleep(1000);
			//stopwatch.ElapsedMilliseconds.ToString().ShowInConsole(true);
			//int[][] test = new int[3][];
			//int con = test.Length;
			//con.ShowInConsole(true);
			int[][] dungeon = new int[][]
			{
				new int[] { -2, -3, 3 },
				new int[] { -5, -10, 1 },
				new int[] { 10, 30, -5 }
			};
			string dungeonString = string.Empty;
			foreach (var item in dungeon)
			{
				foreach (var i in item)
				{
					dungeonString += $"|\t{i}\t";
				}
				dungeonString += "|\n";
			}
			Console.WriteLine(dungeonString);
			CalculateMinimumHP(dungeon);
		}
		/// <summary>
		/// 计算最小生命值
		/// </summary>
		/// <param name="dungeon"></param>
		/// <returns></returns>
		public int CalculateMinimumHP(int[][] dungeon)
		{
			int m = dungeon.Length;
			int n = dungeon[0].Length;
			int[][] dp = dungeon;
			dp[m - 1][n - 1] = Math.Max(1, 1 - dungeon[m - 1][n - 1]);
			for (int x = m - 1; x >= 0; x--)
			{
				for (int y = n - 1; y >= 0; y--)
				{
					if (x == m - 1 && y == n - 1)
						continue;
					if (x == m - 1)
					{
						dp[x][y] = Math.Max(1, dp[x][y + 1] - dungeon[x][y]);
					}
					else if (y == n - 1)
					{
						dp[x][y] = Math.Max(1, dp[x + 1][y] - dungeon[x][y]);
					}
					else
					{
						dp[x][y] = Math.Min(
							Math.Max(1, dp[x][y + 1] - dungeon[x][y]),
							Math.Max(1, dp[x + 1][y] - dungeon[x][y])
						);
					}
				}
			}
			foreach(var item in dp)
			{
				foreach (var i in item)
				{
					Console.Write($"|\t{i}\t");
				}
				Console.WriteLine("|\n");
			}
			return dp[0][0];
		}
	}
}
