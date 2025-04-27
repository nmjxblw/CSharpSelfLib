using System.Diagnostics;

namespace CompanyCode
{
	/// <summary>
	/// 主程序
	/// </summary>
	public sealed class CompanyApp
	{
		/// <summary>
		/// 运行主方法
		/// </summary>
		public void Start()
		{
			DLT645_2007.GetControlCode(true, true, false, DLT645_2007.FunctionCode.读数据).ToString("B8").ShowInConsole(true);
		}
	}
}
