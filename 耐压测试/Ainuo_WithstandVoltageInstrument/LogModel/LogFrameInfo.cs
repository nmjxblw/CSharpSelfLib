using System;

namespace ZH.LogModel
{
    public class LogFrameInfo
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public static string DeviceName = "";

        /// <summary>
        /// 端口号
        /// </summary>
        public string Port = "";

        /// <summary>
        /// 发送的数据
        /// </summary>
        public byte[] SendData;

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime;

        /// <summary>
        /// 提示信息
        /// </summary>
        public string SendMeaning = "";

        /// <summary>
        /// 接收的数据
        /// </summary>
        public byte[] RecvData;

        /// <summary>
        /// 接收的时间
        /// </summary>
        public DateTime RecvTime;

        /// <summary>
        /// 返回帧解析
        /// </summary>
        public string RecvMeaning = "";

        ///// <summary>
        ///// 提示信息
        ///// </summary>
        //public string Tips = "";

        /// <summary>
        /// 是否保存日志
        /// </summary>
        public bool IsSaveLog = false;


        /// <summary>
        /// 是否在控制台输出日志
        /// </summary>
#if CW_BYTES
        public static bool IsDebugLog = true;
#else
        public static bool IsDebugLog = false;
#endif
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="IsSend">是否是发送true发送日志,false 返回日志</param>
        public void WriteLog(bool IsSend)
        {
            string log;//日志内容
            //日志格式【端口号】时间：内容--暂时不写解析
            if (IsSend) //发送的数据
            {
                log = $"{SendTime:HH:mm:ss fff} 【{Port}】 >>> ";

                if (SendData != null || SendData.Length > 0)
                {
                    log += BitConverter.ToString(SendData, 0).Replace("-"," ");
                }
                if (SendMeaning != "") log += $" [{SendMeaning}]";

            }
            else //返回的数据
            {
                log = $"{RecvTime:HH:mm:ss fff} 【{Port}】 <<< ";

                if (RecvData != null || RecvData.Length > 0)
                {
                    log += BitConverter.ToString(RecvData, 0).Replace("-", " ");
                }
                if (RecvMeaning != "") log += $" [{RecvMeaning}]";
            }
            if (IsDebugLog) Console.WriteLine(log);
            if (IsSaveLog) SaveLog(log);
        }


        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="MessageLog">日志内容</param>
        public void SaveLog(string MessageLog)
        {

            try
            {
                if (DeviceName == "")
                {
                    DeviceName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;//获取程序集的名称
                }
            }
            catch (Exception) { }
            try
            {

                string DirectoryPath = System.IO.Directory.GetCurrentDirectory() + $"Log\\设备日志\\{DeviceName}";  //文件夹路径
                if (!System.IO.Directory.Exists(DirectoryPath))  //创建目录
                {
                    System.IO.Directory.CreateDirectory(DirectoryPath);
                    System.Threading.Thread.Sleep(500);//创建目录稍等一点延迟，以防创建失败
                }

                string FileName = DirectoryPath + $"\\{DateTime.Now:yyyy-MM-dd}.txt";
                System.IO.File.AppendAllText(FileName, MessageLog + "\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
