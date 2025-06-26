using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine
{
	/// <summary>
	/// DL/TClass 645-2007多功能电能表通讯协议
	/// </summary>
	public static class DLT645_2007
	{
		/// <summary>
		/// 帧头
		/// </summary>
		public static byte Header { get; } = 0b0110_1000;
		/// <summary>
		/// 帧尾
		/// </summary>
		public static byte Tail { get; } = 0b0001_0110;
		/// <summary>
		/// 组帧
		/// </summary>
		/// <returns></returns>
		public static byte[] FrameAssembly(byte[]? Address = default, byte ControlCode = default, byte[]? Data = default)
		{
			List<byte> bytes = new List<byte>();
			bytes.Add(Header);
			if (Address == default) Address = [0xAA, 0xAA, 0xAA, 0xAA, 0xAA];
			bytes.AddRange(Address);
			bytes.Add(Header);
			if (ControlCode == default) ControlCode = 0b0000;
			bytes.Add(ControlCode);
			if (Data == default) Data = new byte[0];
			bytes.Add((byte)Data.Length);
			bytes.Add(Tail);
			return bytes.ToArray();
		}
		/// <summary>
		/// 读取表地址
		/// </summary>
		/// <returns></returns>
		public static byte[] ReadMeterAddress()
		{
			return default;
		}
		/// <summary>
		/// 功能码枚举
		/// </summary>
		public enum FunctionCode
		{
			/// <summary>
			/// 保留功能码，0b0_0000
			/// </summary>
			保留 = 0b0_0000,
			/// <summary>
			/// 广播校时功能码，0b0_1000
			/// </summary>
			广播校时 = 0b0_1000,
			/// <summary>
			/// 写数据功能码，0b1_0001
			/// </summary>
			读数据 = 0b1_0001,
			/// <summary>
			/// 写数据功能码，0b1_0100
			/// </summary>
			写数据 = 0b1_0100,
			/// <summary>
			/// 电表清零功能码，0b1_1010
			/// </summary>
			电表清零 = 0b1_1010,
		}
		/// <summary>
		/// 获取控制码
		/// </summary>
		/// <param name="direction">传送方向<para>true - 主站发出的命令帧</para><para>false - 从站发出的应答帧</para></param>
		/// <param name="replyFlag">从站应答标志<para>true - 从站正确应答</para><para>false - 从站异常应答</para></param>
		/// <param name="hasSubsequent">是否有后续帧<para>true - 有后续数据帧</para><para>false - 无后续数据帧</para></param>
		/// <param name="functionCode">功能码<para>保留 = 0b0_0000</para><para>广播校时 = 0b0_1000</para><para>读数据 = 0b1_0001</para><para>写数据 = 0b1_0100</para><para>电表清零 = 0b1_1010</para></param>
		/// <returns></returns>
		public static byte GetControlCode(bool direction, bool replyFlag, bool hasSubsequent, FunctionCode functionCode)
		{
			byte result = (byte)functionCode;
			result |= (byte)((direction ? 0 : 1) << 7);
			result |= (byte)((replyFlag ? 0 : 1) << 6);
			result |= (byte)((hasSubsequent ? 1 : 0) << 5);
			return result;
		}
		/// <summary>
		/// 解析控制码
		/// </summary>
		/// <param name="inputControlCode">输入的控制码</param>
		/// <param name="direction">传送方向<para>true - 主站发出的命令帧</para><para>false - 从站发出的应答帧</para></param>
		/// <param name="replyFlag">从站应答标志<para>true - 从站正确应答</para><para>false - 从站异常应答</para></param>
		/// <param name="hasSubsequent">是否有后续帧<para>true - 有后续数据帧</para><para>false - 无后续数据帧</para></param>
		/// <param name="function">功能概述</param>
		public static void ParseControlCode(byte inputControlCode, out bool direction, out bool replyFlag, out bool hasSubsequent, out string function)
		{
			direction = (inputControlCode >> 7 & 1) == 0;
			replyFlag = (inputControlCode >> 6 & 1) == 0;
			hasSubsequent = (inputControlCode >> 5 & 1) == 1;
			// 提取bit[0]-bit[4]
			inputControlCode &= 0b0001_1111;
			function = ((FunctionCode)inputControlCode).ToString();
		}
	}
}