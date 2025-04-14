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
			"任务开始".ShowInConsole(true);
			Stopwatch stopwatch = Stopwatch.StartNew();
			Thread.Sleep(3000);
			stopwatch.Stop();
			Thread.Sleep(1000);
			stopwatch.ElapsedMilliseconds.ToString().ShowInConsole(true);
		}
	}
}
