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
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Solution solution = new Solution();
			// 运行测试
			int input = -142241;
			solution.IsPalindrome(input).ShowInConsole(true);
			stopwatch.Stop();
			Console.WriteLine($"执行时间：{stopwatch.ElapsedMilliseconds}ms");
		}
	}
}
