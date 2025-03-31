using System.Diagnostics;

namespace CompanyCode
{
	/// <summary>
	/// 主程序
	/// </summary>
	public sealed class CompanyApp
	{
		public void Start()
		{
			byte resultByte = 0x7F;
			for (int i = 0; i < 8; i++)
			{
				((resultByte & (1 << i)) == (1 << i)).ShowInConsole(true);
			}
		}
	}
}
