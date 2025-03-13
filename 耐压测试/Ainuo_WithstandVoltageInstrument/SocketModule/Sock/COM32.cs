using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace ZH.SocketModule.Sock
{
    internal class COM32 : IConnection
    {
        /// <summary>
        /// 波特率
        /// </summary>
        private string BaudRate;
        /// <summary>
        /// 数据位
        /// </summary>
        private string DataBits;
        /// <summary>
        /// 停止位
        /// </summary>
        private string StopBits;
        /// <summary>
        /// 校验位
        /// </summary>
        private string CheckBits;
        /// <summary>
        /// 端口号
        /// </summary>
        private string PortNum;

        private Object ThreadObject = new object();


        private SerialPort spCom;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Settings">通信参数1200,e,8,1</param>
        /// <param name="ComNum">端口号(1,2,3,4)</param>
        public COM32(string Settings, int ComNum)
        {
            UpdatePortInfo(Settings);
            PortNum = "COM" + ComNum;
            spCom = new SerialPort();
            MaxWaitSeconds = 1000;
            WaitSecondsPerByte = 100;

        }
        #region IConnection 成员

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            lock (ThreadObject)
            {
                try
                {
                    if (spCom.IsOpen)
                    {
                        return true;
                        //spCom.Close();
                    }

                    spCom.BaudRate = int.Parse(BaudRate);
                    spCom.StopBits = (StopBits)int.Parse(StopBits);
                    spCom.DataBits = int.Parse(DataBits);
                    spCom.Parity = CheckBits.ToLower() == "n" ? Parity.None : CheckBits.ToLower() == "e" ? Parity.Even : Parity.Mark;
                    spCom.PortName = PortNum;
                    spCom.DtrEnable = true;
                    spCom.Open();
                    return true;
                }
                catch
                {
                    spCom.Close();
                    return false;
                }
            }
        }

        /// <summary>
        /// 关闭端口
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            try
            {
                if (spCom.IsOpen) spCom.Close();
            }
            catch { }
            return spCom.IsOpen == false;
        }
        /// <summary>
        /// 更新串口波特率
        /// </summary>
        /// <param name="szSetting"></param>
        /// <returns></returns>
        public bool UpdateBltSetting(string szSetting)
        {
            UpdatePortInfo(szSetting);
            return true;
        }

        /// <summary>
        /// 连接名称
        /// </summary>
        public string ConnectName
        {
            get
            {
                return PortNum;
            }
            set
            {
                PortNum = value;
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

        /// <summary>
        /// 发送数据\接收数据
        /// </summary>
        /// <param name="vData">发送数据</param>
        /// <param name="IsReturn">是否需要等待返回</param>
        /// <param name="WaiteTime"></param>
        /// <returns></returns>
        public bool SendData(ref byte[] vData, bool IsReturn,int WaiteTime)
        {
            lock (ThreadObject)
            {
                if (!this.Open())
                {
                    this.Close();
                    return false;
                }
                spCom.DiscardOutBuffer();
                spCom.DiscardInBuffer();
                Console.WriteLine("┏SendData:{0}", BitConverter.ToString(vData));
                spCom.WriteTimeout = MaxWaitSeconds;
                try
                {
                    spCom.Write(vData, 0, vData.Length);
                    Console.WriteLine("┗发送数据成功===========================");
                }
                catch (TimeoutException ex)
                {
                    ex.ToString();
                    Console.WriteLine("┗发送数据超时");
                    return false;
                }
                if (!IsReturn)
                {
                    Console.WriteLine("┗关闭端口===========================");
                    //spCom.Close();
                    Console.WriteLine("┗本包不需要回复");
                    return true;     //如果不需要返回
                }
        
                bool IsOut = false;
                //System.Threading.Thread.Sleep(WaiteTime);
                System.Threading.Thread.Sleep(WaiteTime);

                DateTime TmpTime1 = DateTime.Now;
                while (TimeSub(DateTime.Now, TmpTime1) < MaxWaitSeconds)          //1秒超时器，如果超过表示收不到任何数据，直接退出
                {
                    System.Threading.Thread.Sleep(1);

                    IsOut = true;
                    if (spCom.BytesToRead > 0)      //如果缓冲区待接收数据量大于0
                    {
                        IsOut = false;
                        break;
                    }
                }
                if (IsOut)      //如果超时就将需要返回的数组数量设置为0
                {
                    vData = new byte[0];
                    //spCom.Close();
                    Console.WriteLine("┗RecvData:接收超时");
                    return true;
                }

                List<byte> TmpBytes = new List<byte>();

                TmpTime1 = DateTime.Now;
                byte[] buf = new byte[0];
                while (TimeSub(DateTime.Now, TmpTime1) < WaitSecondsPerByte)       //100毫秒超时器，目的是检查最后一个字符后面是否还存在待接受的数据
                {
                    System.Threading.Thread.Sleep(1);
                    if (spCom.BytesToRead != 0)
                    {
                        buf = new byte[spCom.BytesToRead];
                        spCom.Read(buf, 0, buf.Length);
                        TmpBytes.AddRange(buf);
                        TmpTime1 = DateTime.Now;
                    }
                }
                vData = TmpBytes.ToArray();
                Console.WriteLine("┗RecvData:{0}", BitConverter.ToString(vData));
                //this.Close();
                return true;
            }
        }
        #endregion


        private long TimeSub(DateTime Time1, DateTime Time2)
        {
            TimeSpan tsSub = Time1.Subtract(Time2);
            return tsSub.Hours * 60 * 60 * 1000 + tsSub.Minutes * 60 * 1000 + tsSub.Seconds * 1000 + tsSub.Milliseconds;
        }

        /// <summary>
        /// 更新端口信息
        /// </summary>
        /// <param name="Settings"></param>
        private void UpdatePortInfo(string Settings)
        {
            string[] Tmp = Settings.Split(',');

            if (Tmp.Length != 4)
            {

                BaudRate = "1200";
                CheckBits = "n";
                DataBits = "8";
                StopBits = "1";
            }
            else
            {
                BaudRate = Tmp[0];
                CheckBits = Tmp[1];
                DataBits = Tmp[2];
                StopBits = Tmp[3];
            }
        }
    }
}
