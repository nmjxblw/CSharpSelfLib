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
			string id = "34 32 B3 35";
			string pwdAndUser = "33 33 33 33 33 33 33 33";
			byte phase = 0x33;
			byte method = 0x34;
			byte type = 0x33;
			string frameString = "54 BF 52 33 33 33 33 33 33 33 33 33 33 F6 CC 33 33 33 33 33 33 33 33 33 DA 2E 75 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33 33";
			int framStringCount = frameString.Split(" ").Count();
			framStringCount.ShowInConsole(true);
		}
	}
}
