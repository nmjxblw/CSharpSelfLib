using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Markup;
using ZH.SocketModule.Packet;
using ZH.Struct;

namespace ZH
{

	/// <summary>
	///耐压仪
	/// </summary>
	public class WithstandVoltageInstrument : IClass_Interface
	{
		/// <summary>
		/// 联机标识符
		/// </summary>
		public bool Connected { get; set; } = false;
		/// <summary>
		/// 重试次数
		/// </summary>
		public static int RetryTimes { get; } = 1;
		/// <summary>
		/// 耐压仪测试方式
		/// </summary>
		/// <remarks>
		///	共4种测试方式：
		///	0x00-ACW 测试；
		///	0x01-IR 测试；
		///	0x02-ACW——IR 测试；
		///	0x03-IR——ACW 测试；
		/// </remarks>
		public byte TestType { get; set; } = 0x00;
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

		public int InitSetting(int ComNumber, int MaxWaitTime, int WaitSecondsPerByte, string IP, int RemotePort, int LocalStartPort)
		{
			PortInfo.m_Exist = 1;
			PortInfo.m_IP = IP;
			PortInfo.m_Port = ComNumber;
			PortInfo.m_Port_IsUDPorCom = true;
			PortInfo.m_Port_Setting = "9600,n,8,1";
			try
			{
				DriverBase.RegisterPort(ComNumber, PortInfo.m_Port_Setting, PortInfo.m_IP, RemotePort, LocalStartPort, MaxWaitTime, WaitSecondsPerByte);
			}
			catch (Exception)
			{
				Connected = false;
				return 1;
			}
			Connected = true;
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
				Connected = false;
				return 1;
			}
			Connected = true;
			return 0;
		}



		/// <summary>
		/// 耐压仪升降源
		/// </summary>
		/// <remarks>升源True；关源False</remarks>
		/// <param name="powerOn">升源True；关源False</param>
		/// <returns></returns>
		public bool PowerOn(bool powerOn = false)
		{
			//组帧
			//发送数据给设备
			//解析值

			byte[] data = new byte[0];
			WithstandVoltageInstrument_SendPacket sendPacket = new WithstandVoltageInstrument_SendPacket((byte)(powerOn ? 0x01 : 0x02), data);
			WithstandVoltageInstrument_ReceivePacket receivePacket = new WithstandVoltageInstrument_ReceivePacket((byte)(powerOn ? 0x01 : 0x02));

			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				return receivePacket.ReceiveResult == ReceiveResult.OK;
			}
			return false;
		}
		/// <summary>
		/// 设置耐压仪的测试方式
		/// </summary>
		/// <returns></returns>
		public bool SetTestType(byte testType)
		{
			byte[] data = new byte[] { testType };
			WithstandVoltageInstrument_SendPacket sendPacket = new WithstandVoltageInstrument_SendPacket(0x03, data);
			WithstandVoltageInstrument_ReceivePacket receivePacket = new WithstandVoltageInstrument_ReceivePacket(0x03);

			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				this.TestType = testType;
				return receivePacket.ReceiveResult == ReceiveResult.OK;
			}
			return false;
		}

		/// <summary>
		///  设置当前测试方式下的参数
		/// </summary>
		/// <param name="Voltage">电压</param>
		/// <param name="UpperCurrentLimit">电流上限(毫安)</param>
		/// <param name="LowerCurrentLimit">电流下限(毫安)</param>
		/// <param name="TestingTime">测试时间(秒)</param>
		/// <param name="Frequency">频率</param>
		/// <param name="SmoothUpTime">缓升时间</param>
		/// <param name="SmoothDownTime">缓降时间</param>
		/// <returns></returns>
		public bool SetModelValue(float Voltage, float UpperCurrentLimit, float LowerCurrentLimit, float TestingTime, int Frequency, float SmoothUpTime, float SmoothDownTime)
		{
			//组帧
			//发送数据给设备
			//解析值
			string str = "";
			str += ((short)Math.Floor(Voltage)).ToString("x4");//转4位16进制
			str += ((short)Math.Floor(UpperCurrentLimit * 1000)).ToString("x6"); //电流乘以1000在下发
			str += ((short)Math.Floor(LowerCurrentLimit * 1000)).ToString("x6");
			str += ((short)Math.Floor(TestingTime * 10)).ToString("x4");
			str += ((short)Frequency).ToString("x2");
			str += ((short)Math.Floor(SmoothUpTime * 10)).ToString("x4");
			str += ((short)Math.Floor(SmoothDownTime * 10)).ToString("x4");
			str += "0000";//保留
			byte[] data = StrToHexByte(str);
			WithstandVoltageInstrument_SendPacket sendPacket = new WithstandVoltageInstrument_SendPacket(0x06, data);
			sendPacket.Resolve = string.Format("电压：{0}V，电流报警上限{1}mA，电流报警下限{2}mA，试验时间{3}s，频率{4}Hz，电压缓升{5}s，电压缓降{6}s",
				Voltage, UpperCurrentLimit, LowerCurrentLimit, TestingTime, Frequency, SmoothUpTime, SmoothDownTime);
			WithstandVoltageInstrument_ReceivePacket receivePacket = new WithstandVoltageInstrument_ReceivePacket(0x06);

			if (SendPacketWithRetry(PortInfo, sendPacket, receivePacket))
			{
				return receivePacket.ReceiveResult == ReceiveResult.OK;
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
			for (int i = 0; i < RetryTimes; i++)
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
