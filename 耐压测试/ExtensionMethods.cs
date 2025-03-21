using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ExtensionMethods
{
	/// <summary>
	/// 将hex-string转化成byte[]
	/// </summary>
	/// <param name="hex"></param>
	/// <returns></returns>
	public static byte[] FromHexString(this string hex)
	{
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
	///获取所有字节和，并返回结果的最低位字节
	/// </summary>
	/// <param name="InputBytes"></param>
	/// <returns></returns>
	public static byte GetSum(this IEnumerable<byte> InputBytes, int startIndex = 0, int length = default)
	{
		if (startIndex > InputBytes.Count() - 1 || startIndex < 0) throw new IndexOutOfRangeException("和校验码计算时，起始索引超出数据帧的范围");
		byte Sum = 0;
		int count = InputBytes.Count();
		int index = -1;
		if (length == default) length = count - startIndex;
		length = Math.Max(0, Math.Min(length, count - startIndex));
		foreach (byte b in InputBytes)
		{
			index++;
			if (index < startIndex) continue;
			if (index >= startIndex + length) break;
			Sum += b;
		}
		return Sum;
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
		byte[] temp = input.ToArray();
		if (BitConverter.IsLittleEndian) temp = input.Reverse().ToArray();
		return BitConverter.ToInt16(temp, 0);
	}
	/// <summary>
	/// 输入4bytes，获得一个int型
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static int GetInt32(this IEnumerable<byte> input)
	{
		byte[] temp = input.ToArray();
		if (BitConverter.IsLittleEndian) temp = input.Reverse().ToArray();
		return BitConverter.ToInt32(temp, 0);
	}
	/// <summary>
	/// 输入8bytes，获得一个long型
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static long GetInt64(this IEnumerable<byte> input)
	{
		byte[] temp = input.ToArray();
		if (BitConverter.IsLittleEndian) temp = input.Reverse().ToArray();
		return BitConverter.ToInt64(temp, 0);
	}
	///// <summary>
	///// 将字节数组转换为合适的数值类型（自动适配 Int16/Int32/Int64）
	///// </summary>
	//public static object ToFitNumber(this IEnumerable<byte> InputBytes)
	//{
	//	if (InputBytes == null || !InputBytes.Any())
	//		throw new ArgumentException("输入字节序列不能为空");

	//	byte[] bytes = InputBytes.ToArray();

	//	// 确保字节序为小端
	//	if (BitConverter.IsLittleEndian)
	//		Array.Reverse(bytes);

	//	try
	//	{
	//		switch (bytes.ParameterLength)
	//		{
	//			case 1:
	//				return (short)bytes[0]; // 单字节转为 Int16
	//			case 2:
	//				return BitConverter.ToInt16(bytes, 0);
	//			case 4:
	//				return BitConverter.ToInt32(bytes, 0);
	//			case 8:
	//				return BitConverter.ToInt64(bytes, 0);
	//			default:
	//				throw new ArgumentException("不支持的非标准字节长度");
	//		}
	//	}
	//	catch (ArgumentException)
	//	{
	//		// 自定义扩展逻辑：处理非标准长度（如截断或填充）
	//		// 示例：截断为最大8字节并转为Int64
	//		byte[] truncated = bytes.Take(8).ToArray();
	//		Array.Resize(ref truncated, 8); // 填充0至8字节
	//		return BitConverter.ToInt64(truncated, 0);
	//	}
	//}
	/// <summary>
	/// 输入任意字节，转化为合适的数字类型并返回值
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static object ToFitNumber(this IEnumerable<byte> input)
	{
		if (input == null || !input.Any())
			throw new ArgumentException("输入字节序列不能为空");
		if (input.Count() <= 2)
			return Convert.ToInt16(BitConverter.ToString(input.ToArray()).Replace("-", ""), 16);
		else if (input.Count() <= 4)
			return Convert.ToInt32(BitConverter.ToString(input.ToArray()).Replace("-", ""), 16);
		else
			return Convert.ToInt64(BitConverter.ToString(input.ToArray()).Replace("-", ""), 16);
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
			if (index >= startIndex + length) break;
			Xor ^= b;
		}
		return Xor;
	}
}
