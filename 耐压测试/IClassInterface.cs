using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
}
