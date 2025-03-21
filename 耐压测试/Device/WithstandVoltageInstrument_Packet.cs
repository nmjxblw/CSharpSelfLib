using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH;
using ZH.Struct;

namespace ZH
{
	#region 耐压仪发送

	/// <summary>
	///耐压仪发送包
	/// </summary>
	internal class WithstandVoltageInstrument_SendPacket : SendPacket
	{
		/// <summary>
		/// 固定格式帧头
		/// </summary>
		public byte PacketHead { get; } = 0x7B;
		/// <summary>
		/// 固定格式帧尾
		/// </summary>
		public byte PacketTail { get; } = 0x7D;
		/// <summary>
		/// 数据帧总长度
		/// </summary>
		public byte Length => (byte)(6 + InputParameter.Length);
		/// <summary>
		/// 耐压仪地址，默认0x00
		/// </summary>
		public byte Address = 0x00;
		/// <summary>
		/// 控制码
		/// </summary>
		public byte ControlCode = 0x11;
		/// <summary>
		/// 输入参数
		/// </summary>
		public byte[] InputParameter = new byte[0];
		/// <summary>
		/// 数据帧解析
		/// </summary>
		public string Resolve { get; set; } = string.Empty;
		/// <summary>
		/// 全帧
		/// </summary>
		public byte[] FullFrame { get; set; } = new byte[0];
		/// <summary>
		/// 构造体
		/// </summary>
		/// <param name="ControlCode">控制码</param>
		/// <param name="Input">参数</param>
		public WithstandVoltageInstrument_SendPacket(byte ControlCode, byte[] Input)
			: base()
		{
			this.ControlCode = ControlCode;
			this.InputParameter = Input;
		}

		public override string GetPacketResolving()
		{
			return Resolve;
		}
		/// <summary>
		/// 获得数据帧
		/// </summary>
		/// <returns></returns>
		public override byte[] GetPacketData()
		{
			ByteBuffer buffer = new ByteBuffer();
			buffer.Initialize();
			// 字头(1 个字节) + 总字节数 + 地址(1 个字节) + 命令字(1 个字节) + [参数] + 校验和(1 个字节) + 字尾(1 个字节)。
			buffer.Put(PacketHead);
			buffer.Put(Length);
			buffer.Put(Address);
			buffer.Put(ControlCode);
			if (InputParameter.Length > 0)
			{
				buffer.Put(InputParameter);
			}
			byte[] temp = buffer.ToByteArray();
			buffer.Put(temp.GetSum(1));
			buffer.Put(PacketTail);
			return buffer.ToByteArray();
		}

	}
	/// <summary>
	/// 耐压仪返回包
	/// </summary>
	internal class WithstandVoltageInstrument_ReceivePacket : ReceivePacket
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public WithstandVoltageInstrument_ReceivePacket() : base()
		{

		}
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="controlCode"></param>
		public WithstandVoltageInstrument_ReceivePacket(byte controlCode)
		{
			ControlCode = controlCode;
		}

		/// <summary>
		/// 发送包的控制码，用于解析接收包
		/// </summary>
		public byte ControlCode { get; set; } = 0x00;
		/// <summary>
		/// 固定格式帧头
		/// </summary>
		public byte PacketHead { get; } = 0x7B;
		/// <summary>
		/// 固定格式帧尾
		/// </summary>
		public byte PacketTail { get; } = 0x7D;
		public string OutData { get; set; } = "";
		/// <summary>
		/// 字节数组转16进制字符串
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public string byteToHexStr(byte[] bytes)
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
		/// 拆帧解析
		/// </summary>
		/// <param name="InputFrame"></param>
		/// <returns></returns>
		public override bool ParsePacket(byte[] InputFrame)
		{
			try
			{
				ByteBuffer buffer = new ByteBuffer(InputFrame);
				// 0x7B 帧头
				if (buffer.Get() != 0x7B)
				{
					ReceiveResult = ReceiveResult.FrameError;
					return false;
				}
				// 长度
				if (buffer.Get() != InputFrame.Length)
				{
					ReceiveResult = ReceiveResult.FrameError;
					return false;
				}
				// 数据域
				if (ControlCode == 0x04)
				{
					// 读取当前测试方式下的参数设置
					short voltage = Convert.ToInt16(BitConverter.ToString(buffer.GetByteArray(2)).Replace("-", ""), 16);
					float upperCurrent = Convert.ToInt16(BitConverter.ToString(buffer.GetByteArray(2)).Replace("-", ""), 16) / 1000f;
					float lowerCurrent = Convert.ToInt16(BitConverter.ToString(buffer.GetByteArray(2)).Replace("-", ""), 16) / 1000f;
					float testTime = Convert.ToInt16(BitConverter.ToString(buffer.GetByteArray(2)).Replace("-", ""), 16) / 10f;
					short frequency = Convert.ToInt16(BitConverter.ToString(buffer.GetByteArray(1)).Replace("-", ""), 16);
					float smoothUpTime = Convert.ToInt16(BitConverter.ToString(buffer.GetByteArray(2)).Replace("-", ""), 16) / 10f;
					float smoothDownTime = Convert.ToInt16(BitConverter.ToString(buffer.GetByteArray(2)).Replace("-", ""), 16) / 10f;
					byte[] keeper = buffer.GetByteArray(2);
					OutData = string.Format(
					"耐压仪设置ACW测试电压{0}V；电流上限{1}mA；电流下限{2}mA；测试时间{3}s；频率{4}Hz；缓升{5}s；缓降{6}s",
					voltage,
					upperCurrent,
					lowerCurrent,
					testTime,
					frequency,
					smoothUpTime,
					smoothDownTime);
				}
				else
				{
					// Ascii码解析
					string reply = Encoding.ASCII.GetString(buffer.GetByteArray(2));
					OutData = reply;
				}
				// 异或校验
				if (InputFrame.GetSum(1, InputFrame.Length - 3) != buffer.Get())
				{
					ReceiveResult = ReceiveResult.CSError;
					return false;
				}
				// 帧尾
				if (buffer.Get() != 0x7D)
				{
					ReceiveResult = ReceiveResult.FrameError;
					return false;
				}
				ReceiveResult = ReceiveResult.OK;
				return true;
			}
			catch
			{
				ReceiveResult = ReceiveResult.Unknow;
				return false;
			}
		}
	}


	#endregion




}
