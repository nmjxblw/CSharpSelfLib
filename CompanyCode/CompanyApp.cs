using Microsoft.AspNetCore.Mvc.Internal;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
			float floatValue = -123.45f * 10000;
			Convert.ToInt32(GetReversedByteString(floatValue, 4), 16).ShowInConsole(true);
		}

		/// <summary>
		/// 获取浮点型四舍五入后的小端序表示字符串
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		private string GetReversedByteString(float number, int bitCount = 3)
		{
			int roundNumber = (int)Math.Round(number);
			int limit = bitCount < 4 ? (1 << (8 * bitCount)) - 1 : int.MaxValue;
			roundNumber = Math.Min(Math.Max(roundNumber, -limit), limit);
			byte[] temp = BitConverter.GetBytes(roundNumber);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(temp);
			byte[] result = new byte[bitCount];
			Array.Copy(temp, result, bitCount);
			StringBuilder reultStringBuilder = new StringBuilder();
			foreach (byte tempByte in result)
			{
				reultStringBuilder.Append(tempByte.ToString("X2"));
			}
			return reultStringBuilder.ToString();
		}
	}
}
