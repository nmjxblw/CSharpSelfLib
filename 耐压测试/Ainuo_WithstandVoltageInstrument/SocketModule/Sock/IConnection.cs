using System;
using System.Collections.Generic;
using System.Text;

namespace ZH.SocketModule.Sock
{
    interface IConnection
    {


        /// <summary>
        /// 连接名称
        /// </summary>
        string ConnectName { get; set; }
        /// <summary>
        /// 最大等待时间
        /// </summary>
        int MaxWaitSeconds { set;  get; }

        /// <summary>
        /// 每二字节点最大等等时间
        /// </summary>
        int WaitSecondsPerByte { set;  get; }
        bool Open();


        bool Close();

        /// <summary>
        /// 更新端口信息
        /// </summary>
        /// <param name="szSetting"></param>
        /// <returns></returns>
        bool UpdateBltSetting(string szSetting);



        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="vData">要发送的数据</param>
        /// <param name="IsReturn">量否需要回复</param>
        /// <param name="WaiteTime">发送后等待时间</param>
        /// <returns>发送是否成功</returns>
        bool SendData(ref byte[] vData, bool IsReturn,int WaiteTime);
    }
}
