using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH.Struct;

namespace ZH
{


	/// <summary>
	/// 测试板
	/// </summary>
	public class WithstandVoltagePlate : IClass_Interface
	{
		/// <summary>
		/// 联机标识符
		/// </summary>
		public bool IsConnected => DriverBase.IsConnected;
		/// <summary>
		/// 重试次数
		/// </summary>
		public static int RETRYTIEMS = 1;
		/// <summary>
		/// 耐压板控制端口
		/// </summary>
		private StPortInfo PortInfo { get; set; } = new StPortInfo();

		private DriverBase DriverBase { get; set; } = new DriverBase();

		//是否发送数据标志
		//private bool sendFlag = true;
		public int DisConnect(out string[] FrameAry)
		{
			//throw new NotImplementedException();
			FrameAry = new string[0];
			return 0;
		}

		/// <summary>
		/// 初始化端口
		/// </summary>
		/// <param name="ComNumber"></param>
		/// <param name="MaxWaitTime"></param>
		/// <param name="WaitSecondsPerByte"></param>
		/// <param name="IP"></param>
		/// <param name="RemotePort"></param>
		/// <param name="LocalStartPort"></param>
		/// <returns></returns>
		public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSecondsPerByte, string IP, int RemotePort, int LocalStartPort)
		{
			PortInfo.m_Exist = 1;
			PortInfo.m_IP = IP;
			PortInfo.m_Port = ComNumber;
			PortInfo.m_Port_IsUDPorCom = true;
			PortInfo.m_Port_Setting = "19200,n,8,1";
			try
			{
				DriverBase.RegisterPort(ComNumber, PortInfo.m_Port_Setting, PortInfo.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSecondsPerByte);
			}
			catch (Exception)
			{
				return 1;
			}
			return 0;
		}

		public int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSecondsPerByte)
		{
			PortInfo.m_Exist = 1;
			PortInfo.m_IP = "";
			PortInfo.m_Port = ComNumber;
			PortInfo.m_Port_IsUDPorCom = false;
			PortInfo.m_Port_Setting = "19200,n,8,1";
			try
			{
				DriverBase.RegisterPort(ComNumber, "19200,n,8,1", MaxWaitTime, WaitSecondsPerByte);
			}
			catch (Exception)
			{
				return 1;
			}
			return 0;
		}
		/// <summary>
		/// 读取当前测试中漏电流采样值
		/// </summary>
		/// <param name="ValueType">30最小值，31最大值，32实时值</param>
		///<param name="SamplingValue">读取到采样值</param>
		///<param name="BwNum">表位0x00广播</param>
		/// <returns></returns>
		public bool GetSamplingValue(byte ValueType, out float SamplingValue, byte BwNum = 0x00)
		{
			SamplingValue = new float();
			//组帧
			//发送数据给设备
			//解析值

			byte[] data = new byte[1];
			data[0] = ValueType;

			WithstandVoltagePlate_SendPacket sendPacket = new WithstandVoltagePlate_SendPacket(0x16, data, BwNum);
			WithstandVoltagePlate_ReceivePacket receivePacket = new WithstandVoltagePlate_ReceivePacket();

			//sendPacket.SetPara(ValueType);
			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				bool result = receivePacket.ReceiveResult == ReceiveResult.OK;
				if (float.TryParse(receivePacket.OutData, out var temp))
				{
					SamplingValue = temp;
				}
				else
				{
					SamplingValue = float.MinValue;
				}
				return result;
			}
			return false;
		}

		/// <summary>
		/// 设置耐压仪功能状态
		/// </summary>
		/// <param name="ValueType">31开始测试,30停止测试，32复位(继电器释放),显示清零。该命令发送后，需延时2秒再发送后续命令，33加标准采样信号，执行AD校准功能。/param>
		/// <returns></returns>
		public bool SetStrat(byte ValueType, byte Socket = 0x00)
		{
			//组帧
			//发送数据给设备
			//解析值

			byte[] data = new byte[1];
			data[0] = ValueType;

			WithstandVoltagePlate_SendPacket sendPacket = new WithstandVoltagePlate_SendPacket(0x13, data, Socket);
			WithstandVoltagePlate_ReceivePacket receivePacket = new WithstandVoltagePlate_ReceivePacket();

			//sendPacket.SetPara(ValueType);
			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				return receivePacket.ReceiveResult == ReceiveResult.OK;
			}
			return false;
		}


		/// <summary>
		/// 设置耐压仪测试组合
		/// </summary>
		/// <param name="ValueType">30 对地，31电压对电流，32电流对电流<param>
		/// <returns></returns>
		public bool SetTestType(byte ValueType)
		{
			//组帧
			//发送数据给设备
			//解析值

			byte[] data = new byte[1];
			data[0] = ValueType;

			WithstandVoltagePlate_SendPacket sendPacket = new WithstandVoltagePlate_SendPacket(0x20, data, 0xEA);
			WithstandVoltagePlate_ReceivePacket receivePacket = new WithstandVoltagePlate_ReceivePacket();

			//sendPacket.SetPara(ValueType);
			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				return receivePacket.ReceiveResult == ReceiveResult.OK;
			}
			return false;
		}

		/// <summary>
		/// <zh>获得表位漏电流采样值</zh>
		/// <en>Get target leaking sample.</en>
		/// </summary>
		/// <param name="Socket">表位编号</param>
		///<param name="valueType">值类型26最大，27最小，28实时值</param>
		/// <returns></returns>
		public float GetSocketValue(byte Socket, byte valueType)
		{
			byte[] data = new byte[2];
			WithstandVoltagePlate_SendPacket sendPacket = new WithstandVoltagePlate_SendPacket(valueType, data, Socket);
			WithstandVoltagePlate_ReceivePacket receivePacket = new WithstandVoltagePlate_ReceivePacket();
			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				if (receivePacket.ReceiveResult != ReceiveResult.OK)
				{
					float s = 0f;
					float.TryParse(receivePacket.OutData, out s);
					return s;
				}
			}
			return 0f;
		}
		/// <summary>
		/// 测试时间设置（范围：1-9999秒）0000: 连续测试
		/// </summary>
		/// <param name="time">测试时间,单位秒<param>
		/// <returns></returns>
		public bool SetTestTime(int time, byte Socket = 0x00)
		{
			//组帧
			//发送数据给设备
			//解析值

			//byte[] data = new byte[2];
			//data[0] = ValueType;
			byte[] data = StrToHexByte(time.ToString("x4"));
			WithstandVoltagePlate_SendPacket sgh = new WithstandVoltagePlate_SendPacket(0x11, data, Socket);
			WithstandVoltagePlate_ReceivePacket rgh = new WithstandVoltagePlate_ReceivePacket();
			if (SendPacketWithRetry(PortInfo, sgh, rgh))
			{
				return rgh.ReceiveResult == ReceiveResult.OK;
			}
			return false;
		}
		/// <summary>
		///漏电流设置（范围：0.01-99.99mA数据扩大100倍）
		/// </summary>
		/// <param name="value"><param>
		/// <returns></returns>
		public bool SetLeakageI(int value, byte BwNum = 0x00)
		{
			byte[] data = StrToHexByte(value.ToString("x4"));   //TODO可能需要×100在下发
																//byte[] data = StrToHexByte((value*100).ToString("x4"));   //TODO可能需要×100在下发

			WithstandVoltagePlate_SendPacket sgh = new WithstandVoltagePlate_SendPacket(0x12, data, BwNum);
			WithstandVoltagePlate_ReceivePacket rgh = new WithstandVoltagePlate_ReceivePacket();
			if (SendPacketWithRetry(PortInfo, sgh, rgh))
			{
				return rgh.ReceiveResult == ReceiveResult.OK;
			}
			return false;
		}
		/// <summary>
		/// 将16进制的字符串转为byte[]
		/// </summary>
		/// <param name="hexString"></param>
		/// <returns></returns>
		public static byte[] StrToHexByte(string hexString)
		{
			hexString = hexString.Replace(" ", "");
			if ((hexString.Length % 2) != 0)
				hexString += " ";
			byte[] returnBytes = new byte[hexString.Length / 2];
			for (int i = 0; i < returnBytes.Length; i++)
				returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			return returnBytes;
		}
		/// <summary>
		/// 发送命令
		/// </summary>
		/// <param name="stPort">端口号</param>
		/// <param name="sp">发送包</param>
		/// <param name="rp">接收包</param>
		/// <returns></returns>
		private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, ReceivePacket rp)
		{
			for (int i = 0; i < RETRYTIEMS; i++)
			{

				if (DriverBase.SendData(stPort, sp, rp) == true)
				{
					return true;
				}
				Thread.Sleep(100);
			}
			return false;
		}
	}
}
