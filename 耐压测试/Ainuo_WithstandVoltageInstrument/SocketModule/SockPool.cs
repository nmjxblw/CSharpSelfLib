using System;
using System.Collections.Generic;
using System.Text;
using ZH.SocketModule.Sock;
using System.Net;

namespace ZH.SocketModule
{
    /// <summary>
    /// 通讯连接池、使用连接名称获取或设置、使用连接对象
    /// </summary>
     class SockPool
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        public static SockPool Instance { get; private set; }
        /// <summary>
        /// 连接对象列表
        /// </summary>
        private Dictionary<string, Connection> DicSock = new Dictionary<string, Connection>();
        /// <summary>
        /// 线程锁,每个端口一个线程锁
        /// </summary>
        private Dictionary<string, object> dicLock = new Dictionary<string, object>();

        private object objLock = new object();
        static SockPool()
        {
            Instance = new SockPool();
        }

        /// <summary>
        /// 
        /// </summary>
        public SockPool()
        {
            if (Instance != null)
                throw new Exception("单例构造模式，不允许多次实例化对象");
        }

        /// <summary>
        /// 清除所有连接
        /// </summary>
        public void Clear()
        {
            lock (objLock)
            {
                foreach (string szKey in DicSock.Keys)
                {
                    DicSock[szKey].Close();
                }
                DicSock.Clear();
                dicLock.Clear();
            }
        }

        /// <summary>
        /// 添加一个使用UDP的连接
        /// </summary>
        /// <param name="szSockName">连接名称：任意字符串名称，用于表示此连接的唯一标识</param>
        /// <param name="remoteIp">远程服务器IP</param>
        /// <param name="remotePort">远程服务器端口</param>
        /// <param name="localPort">本地数据端口号【对于2018来说取值范围为1-33】</param>
        /// <param name="BasePort">本地数据端口号【对于2018来说取值范围为1-33】</param>
        /// <param name="MaxWaitMSecond">最大等待时间</param>
        /// <param name="WaitSecondPerByte">最大等待时间</param>
        public void AddUdpSock(string szSockName, IPAddress remoteIp, int remotePort, int localPort,int BasePort, int MaxWaitMSecond,int WaitSecondPerByte)
        {
            lock (objLock)
            {
                if (DicSock.ContainsKey(szSockName))
                {
                    return;
                }
                Connection s = new Connection(remoteIp, remotePort, localPort, BasePort, MaxWaitMSecond, WaitSecondPerByte);
                DicSock.Add(szSockName, s);
                dicLock.Add(szSockName, new object());
            }
        }

        /// <summary>
        /// 添加一个使用串口通信的连接
        /// </summary>
        /// <param name="szSockName">连接名称：任意字符串名称，用于表示此连接的唯一标识,建议按以下格式：fjk
        /// 格式：port_comX_2018-1/5_BTL
        /// 例如：20000_com1_2018-1_9600,e,8,1
        /// </param>
        /// <param name="commPort">端口号</param>
        /// <param name="szBtl">波特率，通信参数，如： 1200,e,8,1</param>
        /// <param name="MaxWaitMSecond"></param>
        /// <param name="WaitSecondPerByte"></param>
        public void AddComSock(string szSockName, int commPort, string szBtl, int MaxWaitMSecond, int WaitSecondPerByte)
        {
            lock (objLock)
            {
                if (DicSock.ContainsKey(szSockName))
                {
                    return;
                }
                Connection s = new Connection(commPort, szBtl, MaxWaitMSecond,WaitSecondPerByte);
                DicSock.Add(szSockName, s);
                dicLock.Add(szSockName, new object());
            }
        }

        /// <summary>
        /// 更新端口串口配置参数
        /// </summary>
        /// <param name="szSockName"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool UpdatePortSetting(string szSockName,string setting)
        {
            if (DicSock.ContainsKey(szSockName))
            {
               return  DicSock[szSockName].UpdatePortSetting(setting);
            }
            return false;
        }

        /// <summary>
        /// 移除一个网络连接
        /// </summary>
        /// <param name="szSockName"></param>
        public void RemoveSock(string szSockName)
        {
            lock (objLock)
            {
                if (DicSock.ContainsKey(szSockName))
                    DicSock.Remove(szSockName);
                if (dicLock.ContainsKey(szSockName))
                    dicLock.Remove(szSockName);
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="szSockName">端口名称</param>
        /// <param name="sendPacket">发送包</param>
        /// <param name="recvPacket">接收包</param>
        /// <returns>发送是否成功</returns>
        public bool Send(string szSockName, Packet.SendPacket sendPacket, Packet.RecvPacket recvPacket)
        {
            bool ret = false;

            if (dicLock.ContainsKey(szSockName))
            {
                lock (dicLock[szSockName])
                {
                    if (!DicSock.ContainsKey(szSockName))
                    {
                        throw new Exception(string.Format("未找到名称为：{0} 的网络连接", szSockName));
                    }
                    ret = DicSock[szSockName].Send(sendPacket, recvPacket, szSockName);
                }
            }

            return ret;
        }
    }
}
