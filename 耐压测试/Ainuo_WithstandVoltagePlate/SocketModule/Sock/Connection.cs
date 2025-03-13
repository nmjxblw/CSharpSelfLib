using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

namespace ZH.SocketModule.Sock
{

    /// <summary>
    /// 与台体的通讯连接
    /// </summary>
    internal class Connection
    {
        private object objSendLock = new object();
        private object objPackLock = new object();

        /// <summary>
        /// 连接对象
        /// </summary>
        IConnection connection = null;

        /// <summary>
        /// 初始化为UDP连接，并打开连接
        /// </summary>
        /// <param name="remoteIp">远程服务器IP</param>
        /// <param name="remotePort">远程服务器端口</param>
        /// <param name="localPort">本地监听端口</param>
        /// <param name="intBasePort">本地监听端口</param>
        /// <param name="MaxWaitMSecond">指示最大等待时间</param>
        /// <param name="WaitSecondsPerByte">单字节最大等等时间</param>
        public Connection(IPAddress remoteIp, int remotePort, int localPort, int intBasePort, int MaxWaitMSecond, int WaitSecondsPerByte)
        {
            connection = new UDPClient(remoteIp.ToString().Split(':')[0], localPort, remotePort, intBasePort) as IConnection;
            connection.MaxWaitSeconds = MaxWaitMSecond;
            connection.WaitSecondsPerByte = WaitSecondsPerByte;
        }

        /// <summary>
        /// 初始化为COM连接，并打开连接
        /// </summary>
        /// <param name="commPort">COM端口</param>
        /// <param name="szBtl">波特率字符串，如：1200,e,8,1</param>
        /// <param name="WaitSecondsPerByte">单字节最大等等时间</param>
        /// <param name="MaxWaitMSecond">指示最大等待时间</param>
        public Connection(int commPort, string szBtl, int MaxWaitMSecond, int WaitSecondsPerByte)
        {
            connection = new COM32(szBtl, commPort);
            connection.MaxWaitSeconds = MaxWaitMSecond;
            connection.WaitSecondsPerByte = WaitSecondsPerByte;
        }

        /// <summary>
        /// 更新端口对应的COMM口波特率参数
        /// </summary>
        /// <param name="szBlt">要更新的波特率</param>
        /// <returns>更新是否成功</returns>
        public bool UpdatePortSetting(string szBlt)
        {
            if (connection != null) connection.UpdateBltSetting(szBlt);
            return true;
        }

        /// <summary>
        /// 发送并且接收返回数据
        /// </summary>
        /// <param name="sendPack">发送数据包</param>
        /// <param name="recvPack">接收数据包</param>
        /// <param name="PortName">仅用于报文日志，无实际端口作用</param>
        /// <returns></returns>
        public bool Send(Packet.SendPacket sendPack, Packet.RecvPacket recvPack, string PortName)
        {

            Monitor.Enter(objPackLock);

            LogModel.LogFrameInfo Log = new LogModel.LogFrameInfo();
            Log.Port = PortName;
            byte[] vData = sendPack.GetPacketData();
            Log.SendTime = DateTime.Now;
            if (vData == null)
            {
                Log.SendMeaning = "发送数据包为空";
                Monitor.Exit(objPackLock);
                return false;
            }

            Log.SendData = vData;
            Log.WriteLog(true);

            if (!Send(ref vData, sendPack.IsNeedReturn, sendPack.WaiteTime()))
            {
                Log.RecvMeaning = "发送失败";
                Log.RecvData = vData;
                Log.RecvTime = DateTime.Now;
                Monitor.Exit(objPackLock);
                Log.WriteLog(false);
                return false;
            }
            Log.RecvData = vData;
            Log.RecvTime = DateTime.Now;
            if (sendPack.IsNeedReturn == false)//  && vData.Length < 1
            {
                Log.RecvMeaning = "不需要回复数据";
                Monitor.Exit(objPackLock);
                Log.WriteLog(false);
                return true;
            }

            if (sendPack.IsNeedReturn == true && (vData == null || vData.Length < 1))
            {
                Log.RecvMeaning = "无回复数据";
                Monitor.Exit(objPackLock);
                Log.WriteLog(false);
                return false;
            }
            if (sendPack.IsNeedReturn == true && recvPack == null)
            {
                Log.RecvMeaning = "没有传入解析包";
                Monitor.Exit(objPackLock);
                Log.WriteLog(false);
                return false;
            }
            if (sendPack.IsNeedReturn == true && recvPack != null)
            {
                bool ret = recvPack.ParsePacket(vData);
                Log.RecvMeaning = recvPack.GetPacketResolving();
                Monitor.Exit(objPackLock);
                Log.WriteLog(false);
                return ret;
            }
            else
            {
                Log.RecvMeaning = "其他发送接收错误";
                Monitor.Exit(objPackLock);
                Log.WriteLog(false);
                return false;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="vData"></param>
        /// <param name="isNeedReturn"></param>
        /// <param name="WaiteTime"></param>
        /// <returns></returns>
        private bool Send(ref byte[] vData, bool isNeedReturn, int WaiteTime)
        {
            if (connection == null) return false;
            lock (objSendLock)
            {
                if (connection != null)
                {
                    if (!connection.SendData(ref vData, isNeedReturn, WaiteTime))
                        return false;
                }
                if (isNeedReturn && vData.Length < 1)
                {
                    return false;
                }
                return true;
            }
        }
        public bool Close()
        {
            if (connection == null) return true;
            return connection.Close();
        }
    }
}
