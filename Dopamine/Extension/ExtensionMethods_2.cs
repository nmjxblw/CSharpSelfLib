using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Dopamine
{
	// 拓展方法第二部分，数据帧相关
	public static partial class ExtensionMethods
	{
		/// <summary>
		/// 从流中获取数据帧
		/// </summary>
		/// <param name="stream">输入流</param>
		/// <param name="capacity">容量</param>
		/// <returns>流数据帧</returns>
		public static byte[] GetFrame(this NetworkStream stream, int capacity = 0x100)
		{
			// 流数据容器
			byte[] InitFrame = new byte[capacity];
			// 获取数据帧长度
			int len = stream.Read(InitFrame, 0, InitFrame.Length);
			byte[] TrimFrame = new byte[len];
			Array.Copy(InitFrame, 0, TrimFrame, 0, len);
			return TrimFrame;
		}

		/// <summary>
		/// 和校验
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static byte GetSumCheck(this IEnumerable<byte> input)
		{
			byte result = 0x00;
			foreach (byte data in input)
			{
				result += data;
			}
			return result;
		}
		/// <summary>
		/// 计算检验码[帧头不进入检验范围]
		/// </summary>
		public static byte GetXor(this IEnumerable<byte> InputBytes, int startIndex = 1, int length = default)
		{
			if (startIndex > InputBytes.Count() - 1 || startIndex < 0) throw new IndexOutOfRangeException("异或校验码计算时，起始索引超出数据帧的范围");
			byte Xor = 0;
			int count = InputBytes.Count();
			int index = -1;
			if (length == default) length = count - startIndex;
			length = Math.Max(0, Math.Min(length, count - startIndex));
			foreach (byte b in InputBytes)
			{
				index++;
				if (index < startIndex) continue;
				if (index > startIndex + length) break;
				Xor ^= b;
			}
			return Xor;
		}
		/// <summary>
		/// 将BCC校验码填入数据帧最后一位（如果最后一位不为0x00,则自动补位）
		/// </summary>
		public static byte[] FillBBCAtLast(this IEnumerable<byte> Input, bool FillInTail = false)
		{
			List<byte> result = Input.ToList();
			if (result.Count <= 0) return result.ToArray();
			if (!FillInTail) result.Add(0x00);
			byte BBC = 0x00;
			// 除去帧头，其余字节进行异或运算
			for (int i = 1; i < result.Count - 1; i++)
			{
				BBC ^= result[i];
			}
			result[result.Count - 1] = BBC;
			return result.ToArray();
		}
		/// <summary>
		/// 判断目标 byte[] 是否在集合中 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool HasBytes(this IEnumerable<byte[]> Input, byte[] item)
		{
			return Input.Any(bytes => bytes.SequenceEqual(item));
		}
		/// <summary>
		/// 在帧末尾添加异或校验
		/// </summary>
		/// <returns></returns>
		public static byte[] AppendXor(this byte[] inputFrame, bool IncludingHead = false)
		{
			List<byte> ck = new List<byte>(inputFrame);
			ck.Add(0x00);
			for (int i = IncludingHead ? 0 : 1; i < inputFrame.Length; i++)
			{
				ck[inputFrame.Length] ^= inputFrame[i];
			}
			return ck.ToArray();
		}
		/// <summary>
		/// 将hex-string转化成byte[]
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static byte[] FromHexString(this string hex)
		{
			hex = Regex.Replace(hex, @"[^0-9A-F]", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (string.IsNullOrWhiteSpace(hex)) return Array.Empty<byte>();
			int length = hex.Length;
			if (length % 2 != 0)
				hex += " ";

			byte[] bytes = new byte[length / 2];
			for (int i = 0; i < length; i += 2)
			{
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}
			return bytes;
		}

		/// <summary>
		/// 获取指定位置开始，目标长度的数据帧
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static byte[] SubFrame(this IEnumerable<byte> input, int startIndex = 0, int length = default)
		{
			startIndex = Math.Max(0, Math.Min(startIndex, input.Count()));
			if (length == default) length = input.Count();
			else { length = Math.Max(0, Math.Min(length, input.Count() - startIndex)); }
			return input.Skip(startIndex).Take(length).ToArray();
		}
		/// <summary>
		/// 输入2bytes，获得一个short型
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static short GetInt16(this IEnumerable<byte> input)
		{
			byte[] temp = new byte[2];
			Array.Copy(input.ToArray(), temp, input.Count());
			if (BitConverter.IsLittleEndian) Array.Copy(input.Reverse().ToArray(), temp, input.Count());
			return BitConverter.ToInt16(temp, 0);
		}
		/// <summary>
		/// 输入4bytes，获得一个int型
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static int GetInt32(this IEnumerable<byte> input)
		{
			byte[] temp = new byte[4];
			Array.Copy(input.ToArray(), temp, input.Count());
			if (BitConverter.IsLittleEndian) Array.Copy(input.Reverse().ToArray(), temp, input.Count());
			return BitConverter.ToInt32(temp, 0);
		}
		/// <summary>
		/// 输入8bytes，获得一个long型
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static long GetInt64(this IEnumerable<byte> input)
		{
			byte[] temp = new byte[8];
			Array.Copy(input.ToArray(), temp, input.Count());
			if (BitConverter.IsLittleEndian) Array.Copy(input.Reverse().ToArray(), temp, input.Count());
			return BitConverter.ToInt64(temp, 0);
		}
		/// <summary>
		/// 输入任意字节，转化为合适的数字类型并返回值
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		[return: System.Diagnostics.CodeAnalysis.NotNull]
		public static object ToFitNumber(this IEnumerable<byte> input)
		{
			if (input == null || !input.Any())
				throw new ArgumentException("输入字节序列不能为空");
			if (input.Count() <= 2)
				return Convert.ToInt16(BitConverter.ToString(input.ToArray()).Replace("-", ""), 16);
			else if (input.Count() <= 4)
				return Convert.ToInt32(BitConverter.ToString(input.ToArray()).Replace("-", ""), 16);
			return Convert.ToInt64(BitConverter.ToString(input.ToArray()).Replace("-", ""), 16);
		}

		/// <summary>
		/// 将字节数组转换为合适的数值类型（自动适配 Int16/Int32/Int64）
		/// </summary>
		public static object ToFitNumber1(this IEnumerable<byte> input)
		{
			if (input == null || !input.Any())
				throw new ArgumentException("输入字节序列不能为空");

			byte[] bytes = input.ToArray();

			// 确保字节序为小端（如需大端序，可反转字节数组）
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			try
			{
				switch (bytes.Length)
				{
					case 1:
						return (short)bytes[0]; // 单字节转为 Int16
					case 2:
						return BitConverter.ToInt16(bytes, 0);
					case 4:
						return BitConverter.ToInt32(bytes, 0);
					case 8:
						return BitConverter.ToInt64(bytes, 0);
					default:
						throw new ArgumentException("不支持的非标准字节长度");
				}
			}
			catch (ArgumentException)
			{
				// 自定义扩展逻辑：处理非标准长度（如截断或填充）
				// 示例：截断为最大8字节并转为Int64
				byte[] truncated = bytes.Take(8).ToArray();
				Array.Resize(ref truncated, 8); // 填充0至8字节
				return BitConverter.ToInt64(truncated, 0);
			}
		}
		/// <summary>
		/// 将字节字符串转换为十六进制数组
		/// </summary>
		/// <param name="byteString"></param>
		/// <returns></returns>
		public static byte[] ByteStringToBytes(this string byteString)
		{

			int byteCount = byteString.Length % 2 + byteString.Length / 2;
			byte[] bytes = new byte[byteCount];
			for (int i = 0; i < byteCount; i++)
			{
				try
				{
					bytes[i] = Convert.ToByte(byteString.Substring(i * 2, 2), 16);
				}
				catch
				{
					bytes[i] = 0;
				}
			}
			return bytes;
		}
	}
}
