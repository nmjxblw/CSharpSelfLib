using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH.Struct;

namespace ZH
{

	[InterfaceType(ComInterfaceType.InterfaceIsIDispatch), ComVisible(true)]
	public interface IClass_Interface
	{
		/// <summary>
		/// 初始化设备通讯参数UDP
		/// </summary>
		/// <param name="ComNumber">端口号</param>
		/// <param name="MaxWaitTme">最长等待时间</param>
		/// <param name="WaitSecondsPerByte">帧字节间隔时间</param>
		/// <param name="IP">Ip地址</param>
		/// <param name="RemotePort">远程端口</param>
		/// <param name="LocalStartPort">本地端口</param>
		/// <returns>是否注册成功</returns>
		[DispId(1)]
		int InitSetting(int ComNumber, int MaxWaitTime, int WaitSecondsPerByte, string IP, int RemotePort, int LocalStartPort);
		/// <summary>
		/// 注册Com 口
		/// </summary>
		/// <param name="ComNumber"></param>
		/// <param name="strSetting"></param>
		/// <param name="maxWaittime"></param>
		/// <returns></returns>
		[DispId(2)]
		int InitSettingCom(int ComNumber, int MaxWaitTime, int WaitSecondsPerByte);
		/// <summary>
		/// 脱机指令没有
		/// </summary>
		/// <param name="FrameAry">输出上行报文</param>
		/// <returns></returns>
		[DispId(3)]
		int DisConnect(out string[] FrameAry);

	}

	/// <summary>
	///耐压仪
	/// </summary>
	public class Ainuo_WithstandVoltageInstrument : IClass_Interface
	{
		/// <summary>
		/// 重试次数
		/// </summary>
		public static int RETRYTIEMS = 1;
		/// <summary>
		/// 耐压仪控制端口
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

		public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSencondsPerByte, string IP, int RemotePort, int LocalStartPort)
		{
			PortInfo.m_Exist = 1;
			PortInfo.m_IP = IP;
			PortInfo.m_Port = ComNumber;
			PortInfo.m_Port_IsUDPorCom = true;
			PortInfo.m_Port_Setting = "9600,n,8,1";
			try
			{
				DriverBase.RegisterPort(ComNumber, PortInfo.m_Port_Setting, PortInfo.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSencondsPerByte);
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
			PortInfo.m_Port_Setting = "9600,n,8,1";
			try
			{
				DriverBase.RegisterPort(ComNumber, "9600,n,8,1", MaxWaitTime, WaitSecondsPerByte);
			}
			catch (Exception)
			{

				return 1;
			}
			return 0;
		}



		/// <summary>
		/// 启动，停止测试
		/// </summary>
		/// <param name="ValueType">01开始，02结束<param>
		/// <returns></returns>
		public int Start(byte ValueType)
		{
			//组帧
			//发送数据给设备
			//解析值

			byte[] data = new byte[0];
			WithstandVoltageInstrument_RequestLinkPacket sendPacket = new WithstandVoltageInstrument_RequestLinkPacket(ValueType, data);
			WithstandVoltageInstrument_RequestLinkReplyPacket receivePacket = new WithstandVoltageInstrument_RequestLinkReplyPacket();

			//sendPacket.SetPara(ValueType);
			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				if (receivePacket.ReciveResult != RecvResult.OK)
				{
					return 1;
				}
			}
			return 0;
		}

		/// <summary>
		///  设置当前测试方式下的参数
		/// </summary>
		/// <param name="Voltage">电压</param>
		/// <param name="UpperCurrentLimit">电流上限(毫安)</param>
		/// <param name="LowerCurrentLimit">电流下限(毫安)</param>
		/// <param name="TestingTime">测试时间(秒)</param>
		/// <param name="Frequency">频率</param>
		/// <param name="PowerOnTime">缓升时间</param>
		/// <param name="PowerOffTime">缓降时间</param>
		/// <returns></returns>
		public int SetModelValue(float Voltage, float UpperCurrentLimit, float LowerCurrentLimit, int TestingTime, int Frequency, int PowerOnTime, int PowerOffTime)
		{
			//组帧
			//发送数据给设备
			//解析值
			//byte[] data = new byte[17];
			string str = "";
			str += Voltage.ToString("x4");//转4位16进制
			str += (UpperCurrentLimit * 1000).ToString("x6"); //电流乘以1000在下发
			str += (LowerCurrentLimit * 1000).ToString("x6");
			str += (TestingTime).ToString("x4");
			str += (Frequency).ToString("x2");
			str += (PowerOnTime).ToString("x4");
			str += (PowerOffTime).ToString("x4");
			str += "0000";//保留
			byte[] data = StrToHexByte(str);
			WithstandVoltageInstrument_RequestLinkPacket sendPacket = new WithstandVoltageInstrument_RequestLinkPacket(0x06, data);
			WithstandVoltageInstrument_RequestLinkReplyPacket receivePacket = new WithstandVoltageInstrument_RequestLinkReplyPacket();

			//sendPacket.SetPara(ValueType);
			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				if (receivePacket.ReciveResult != RecvResult.OK)
				{
					return 1;
				}
			}
			return 0;
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
		private bool SendPacketWithRetry(StPortInfo stPort, SendPacket sp, RecvPacket rp)
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
