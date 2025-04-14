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
			string Barcode = "313000920019999999901";
			//string Barcode = "011000100101010101010";
			Barcode = Regex.Replace(Barcode, @"[^0-9]", "");
			int sum = 0;
			for (int i = 0; i < Barcode.Length; i++)
			{
				int currentcode = int.Parse(Barcode[i].ToString());
				sum += currentcode * (i % 2 == 0 ? 3 : 1);
			}
			int r = sum % 10;
			int result = 10 - r;
			result.ToString().ShowInConsole(true);
		}

	}
}
