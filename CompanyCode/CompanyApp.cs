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
			string frameString = " 68 13 FE 0A 01 09 00 00 00";
			byte[] frame = frameString.ByteStringToBytes().AppendXor(false);
			BitConverter.ToString(frame).Replace("-", " ").ShowInConsole(true);
		}
	}
}
