/*
 
 * 初始化端口由端口自己完成。
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;



namespace ZH.SocketModule.Sock
{
    /// <summary>
    /// UDP端口
    /// </summary>
    internal class UDPClient : IConnection
    {
        private int UdpBindPort;
        private UdpClient Client;
        private UdpClient settingClient;
        private string szBlt = "1200,e,8,1";
        private IPEndPoint Point = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 10003);
        private IPEndPoint localPoint = null;
        private string m_2018IpAddress = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="BindPort">com</param>
        /// <param name="RemotePort">10003,10004</param>
        /// <param name="BasePort">本地起始端口</param>
        public UDPClient(string Ip, int BindPort, int RemotePort, int BasePort)
        {
            m_2018IpAddress = Ip;
            Point.Address = IPAddress.Parse(Ip);
            Point.Port = RemotePort;
            UdpBindPort = LocalPortTo2011Port(BindPort, BasePort);//转换端口成2018端口
            localPoint = new IPEndPoint(IPAddress.Parse(GetHostIp()), UdpBindPort);

        }

        public string GetHostIp()
        {
            string Tip;
            Tip = Dns.GetHostName();
            IPHostEntry Tipentry = Dns.GetHostEntry(Tip);
            string[] str2018Ips = m_2018IpAddress.Split('.');
            bool bChaZhao = false;
            string strResult = string.Empty;
            for (int i = 0; i < Tipentry.AddressList.Length; i++)
            {
                Tip = Tipentry.AddressList[i].ToString();
                string[] Tipg;
                Tipg = Tip.Split('.');
                if (Tipg.Length == 4)
                {
                    if (str2018Ips.Length == 4)
                    {
                        if (Tipg[0] == str2018Ips[0]
                            && Tipg[1] == str2018Ips[1]
                            && Tipg[2] == str2018Ips[2])
                        {
                            strResult = Tip;
                            break;
                        }


                    }
                    else
                    {
                        if (!bChaZhao)
                        {
                            bChaZhao = true;
                            strResult = Tip;
                        }
                    }

                }
            }
            return strResult;

        }

        public static uint IPToUint(string ipAddress)
        {
            string[] strs = ipAddress.Trim().Split('.');
            byte[] buf = new byte[4];

            for (int i = 0; i < strs.Length; i++)
            {
                buf[i] = byte.Parse(strs[i]);
            }
            Array.Reverse(buf);

            return BitConverter.ToUInt32(buf, 0);
        }

        public static string GetSubnetworkIP(string targetIP)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\Tcpip\Parameters\Interfaces", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
            uint iTarget = IPToUint(targetIP);
            foreach (string keyName in key.GetSubKeyNames())
            {
                try
                {
                    RegistryKey tmpKey = key.OpenSubKey(keyName);
                    string[] ip = tmpKey.GetValue("IPAddress") as string[];
                    if (ip == null)
                    {
                        continue;
                    }
                    string[] subnet = tmpKey.GetValue("SubnetMask") as string[];
                    for (int i = 0; i < ip.Length; i++)
                    {
                        IPAddress local = IPAddress.Parse(ip[i]);
                        if (local.IsIPv6SiteLocal)
                            continue;

                        uint iIP = IPToUint(ip[i]);
                        uint iSub = IPToUint(subnet[i]);

                        if ((iIP & iSub) == (iTarget & iSub))
                        {
                            return ip[i];
                        }
                    }
                }
                catch
                {
                }
            }
            return "127.0.0.1";
            
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="vData"></param>
        /// <param name="IsReturn"></param>
        /// <param name="WaiteTime"></param>
        /// <returns></returns>
        public bool SendData(ref byte[] vData, bool IsReturn, int WaiteTime)
        {
            try
            {
                lock (this)
                {

                    {
                        Client = new UdpClient();
                        Client.Client.Bind(this.localPoint);
                    }

                }
                Client.Connect(Point);
            }
            catch { return false; }
            Client.Send(vData, vData.Length);
#if CW_BYTES
            Console.WriteLine(UdpBindPort.ToString());
            Console.WriteLine("┏SendData:{0}", BitConverter.ToString(vData));
#endif
            if (!IsReturn)
            {
                Console.WriteLine("┗本包不需要回复");
                Client.Close();
                return true;
            }
            Thread.Sleep(WaiteTime);
            byte[] BytReceived = new byte[0];
            bool IsReveive = false;     //标志是否返回
            List<byte> RevItems = new List<byte>();     //接收的数据集合
            DateTime Dt;            //等待时间变量
            Dt = DateTime.Now;
            while (TimeSub(DateTime.Now, Dt) < MaxWaitSeconds)
            {
                Thread.Sleep(1);
                try
                {
                    if (Client.Available > 0)
                    {
                        BytReceived = Client.Receive(ref Point);
                        IsReveive = true;
                        break;
                    }
                }
                catch
                {
                    Client.Close();
                    return false;
                }
            }

            if (!IsReveive)
            {
                vData = new byte[0];
            }
            else
            {
                RevItems.AddRange(BytReceived);
                Dt = DateTime.Now;
                while (TimeSub(DateTime.Now, Dt) < WaitSecondsPerByte)
                {
                    if (Client.Available > 0)
                    {
                        BytReceived = Client.Receive(ref Point);
                        RevItems.AddRange(BytReceived);
                        Dt = DateTime.Now;
                    }
                }
                vData = RevItems.ToArray();
            }
#if CW_BYTES
            Console.WriteLine("┗RecvData:{0}", BitConverter.ToString(vData));
#endif
            Client.Close();
            return true;
        }


        private long TimeSub(DateTime Time1, DateTime Time2)
        {
            TimeSpan tsSub = Time1.Subtract(Time2);
            return tsSub.Hours * 60 * 60 * 1000 + tsSub.Minutes * 60 * 1000 + tsSub.Seconds * 1000 + tsSub.Milliseconds;
        }

        /// <summary>
        /// 本地通道转换成2018端口:20000 + 2 * (port - 1);
        /// 数据端口，设置端口在数据端口的基础上+1
        /// </summary>
        /// <param name="port"></param>
        /// <param name="BasePort"></param>
        /// <returns></returns>
        private int LocalPortTo2011Port(int port, int BasePort)
        {
            return BasePort + 2 * (port - 1);
        }

        #region IConnection 成员

        public string ConnectName
        {
            get
            {
                return Point.ToString();
            }
            set
            {
            }
        }

        public int MaxWaitSeconds
        {
            get;
            set;
        }

        public int WaitSecondsPerByte
        {
            get;
            set;
        }

        public bool Open()
        {
            return true;
        }

        public bool Close()
        {
            return true;
        }

        /// <summary>
        /// 更新232串口波特率
        /// </summary>
        /// <param name="szSetting"></param>
        /// <returns></returns>
        public bool UpdateBltSetting(string szSetting)
        {
            //if (szBlt == szSetting) return true;//与上次相同，则不用初始化
            szBlt = szSetting;
            int settingPort = UdpBindPort + 1;

            try
            {
                try
                {
                    //if (settingClient == null)
                    {
                        settingClient = new UdpClient(settingPort);
                    }
                    settingClient.Connect(Point);
                }
                catch { }

                string str_Data = "reset";
                byte[] byt_Data = ASCIIEncoding.ASCII.GetBytes(str_Data);
                int sendlen = settingClient.Send(byt_Data, byt_Data.Length);

                System.Threading.Thread.Sleep(10);
                str_Data = "init " + szBlt.Replace(',', ' ');
                
                byt_Data = ASCIIEncoding.ASCII.GetBytes(str_Data);
                sendlen = settingClient.Send(byt_Data, byt_Data.Length);
                settingClient.Close();
                return sendlen == byt_Data.Length;
            }
            catch { }
            finally
            {
            }
            return false;
        }

        #endregion
    }
}
