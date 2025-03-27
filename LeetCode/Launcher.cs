using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetCode
{
	/// <summary>
	/// LeetCode启动器
	/// </summary>
	public static class Launcher
	{
		/// <summary>
		/// Application实例化
		/// </summary>
		private static MainApplication App { get; } = new MainApplication();
		/// <summary>
		/// 主方法，用于启动程序
		/// </summary>
		public static void Main()
		{
			Console.WriteLine("LeetCode训练程序启动。");
			App.Start();
		}
	}
}
