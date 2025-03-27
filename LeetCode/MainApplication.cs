using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dopamine;
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
			string input = ConfigManager.Data.Tests[0].Input[0];
			input.ShowInConsole(true);
			Checker checker = new Checker(DateTime.Now);
			checker.DeepCompare("happy", "happy").ToString().ShowInConsole(true);
			checker.TimeRecord().ShowInConsole(true);
		}
	}
}
