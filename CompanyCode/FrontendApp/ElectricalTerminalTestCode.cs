using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyCode
{
	/// <summary>
	/// 电能终端测试代码
	/// </summary>
    public class ElectricalTerminalTestCode
    {
		/// <summary>
		/// 获取地址域
		/// </summary>
		/// <returns></returns>
		public static byte[] GetAddress()
		{
			List<byte> address = new List<byte>();
			#region 计算流水编号
			ConfigManager.Data.AssemblyID = Regex.Replace(
				ConfigManager.Data.AssemblyID, @"[^0-9]", "", RegexOptions.IgnoreCase)
				.PadLeft(6, '0').Substring(0, 6);
			string AssemblyID = ConfigManager.Data.AssemblyID;
			#endregion
			#region 计算台体编号
			ConfigManager.Data.BenchID = Regex.Replace(
				ConfigManager.Data.BenchID, @"[^0-9]", "", RegexOptions.IgnoreCase)
				.PadLeft(6, '0').Substring(0, 6);
			string BenchID = ConfigManager.Data.BenchID;
			#endregion
			byte[] benchNoBytes = ByteStringToBytes(BenchID);
			address.AddRange(benchNoBytes.Reverse());
			byte[] assemblyNoBytes = ByteStringToBytes(AssemblyID);
			address.AddRange(assemblyNoBytes.Reverse());
			// 终端地址
			address.Add(0x01);

			return address.ToArray();
		}
		/// <summary>
		/// 将十六进制字符串转换为字节数组
		/// </summary>
		/// <param name="byteString"></param>
		/// <returns></returns>
		public static byte[] ByteStringToBytes(string byteString)
		{
			int byteCount = byteString.Length % 2 + byteString.Length / 2;
			byte[] bytes = new byte[byteCount];
			for (int i = 0; i < byteCount; i++)
			{
				bytes[i] = Convert.ToByte(byteString.Substring(i * 2, 2), 16);
			}
			return bytes;
		}
	}
}
