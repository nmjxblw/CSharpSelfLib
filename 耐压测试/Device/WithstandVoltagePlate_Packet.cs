using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH;
using ZH.Struct;

namespace ZH
{

	#region 耐压板发送

	/// <summary>
	///耐压板子联机/脱机请求包
	/// </summary>
	internal class WithstandVoltagePlate_SendPacket : SendPacket
	{
		public bool IsLink { get; set; } = true;
		/// <summary>
		/// 表位编号0x80广播
		/// </summary>
		public byte SocketIndex { get; set; } = 0x80;
		/// <summary>
		/// 控制码
		/// </summary>
		public byte ControlCode { get; set; } = 0x11;
		/// <summary>
		/// 数据域的长度位
		/// </summary>
		public byte ParameterLength { get; set; } = 0x01;
		/// <summary>
		/// 数据位
		/// </summary>
		public byte[] Parameter { get; set; } = new byte[0];
		/// <summary>
		/// 耐压板发送包固定帧头
		/// </summary>
		public byte PacketHead { get; } = 0x4E;
		/// <summary>
		/// 耐压板发送包固定帧尾
		/// </summary>
		public byte PacketTail { get; } = 0x45;
		public WithstandVoltagePlate_SendPacket(byte controlCode, byte[] inputParameter, byte socketIndex)
			: base()
		{
			ControlCode = controlCode;
			ParameterLength = (byte)inputParameter.Length;
			Parameter = inputParameter;
			SocketIndex = controlCode == 0x20 ? (byte)0xEA : (byte)(socketIndex + 0x80);
		}

		public override string GetPacketResolving()
		{
			string strResolve = "没有解析";
			return strResolve;
		}
		/// <summary>
		/// 组装数据帧并返回发送字节数组
		/// </summary>
		/// <returns></returns>
		public override byte[] GetPacketData()
		{
			ByteBuffer byteBuffer = new ByteBuffer();
			byteBuffer.Put(PacketHead);// 帧头
			byteBuffer.Put(SocketIndex); // 地址
			byteBuffer.Put(ControlCode);// 控制码
			byteBuffer.Put(ParameterLength);// 数据域长度
			byteBuffer.Put(Parameter);// 数据域
			byte[] temp = byteBuffer.ToByteArray();
			byte sum = temp.GetSum();
			byteBuffer.Put(sum);// 和校验
			byteBuffer.Put(PacketTail);// 帧尾
			return byteBuffer.ToByteArray();
		}
	}
	/// <summary>
	/// 耐压板联机返回指令
	/// </summary>
	internal class WithstandVoltagePlate_ReceivePacket : ReceivePacket
	{
		/// <summary>
		/// 返回数据
		/// </summary>
		public string OutData { get; set; } = "";

		/// <summary>
		/// 字节数组转16进制字符串
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public string ByteToHexStr(byte[] bytes)
		{
			string returnStr = "";
			if (bytes != null)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					returnStr += bytes[i].ToString("X2");
				}
			}
			return returnStr;
		}
		public override string GetPacketResolving()
		{
			string strResolve = "返回：" + ReceiveResult.ToString();
			return strResolve;
		}
		/// <summary>
		/// 解析数据帧，转化为有效信息
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public override bool ParsePacket(byte[] bytes)
		{
			ByteBuffer byteBuffer = new ByteBuffer(bytes);
			if (byteBuffer.Get() != 0x4F)
			{
				ReceiveResult = ReceiveResult.FrameError;
				return false;
			}
			byte socket = byteBuffer.Get();
			if (socket < 0x80)
			{
				ReceiveResult = ReceiveResult.FrameError;
				return false;
			}
			byte controlCode = byteBuffer.Get();
			byte parameterLength = byteBuffer.Get();
			if (parameterLength + 6 != bytes.Length)
			{
				ReceiveResult = ReceiveResult.FrameError;
				return false;
			}
			// 只处理读值，其他数据帧不处理
			switch (controlCode)
			{
				case 0x26:
				case 0x27:
				case 0x28:
					{
						// 实际值被放大100倍
						// 需要转成10进制后除以100
						// 先转字符串再转ushort，避免程序使用小端序解析
						OutData = (Convert.ToUInt16(BitConverter.ToString(byteBuffer.GetByteArray(2)).Replace("-", ""), 16) / 100f).ToString("F2");
						ReceiveResult = ReceiveResult.OK;
						return true;
					}
				default:
					{
						return true;
					}
			}
		}
	}


	#endregion


}
