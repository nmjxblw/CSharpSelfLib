using System;
using System.Collections.Generic;
using System.Text;

namespace ZH.SocketModule.Packet
{
    /// <summary>
    /// 接收数据包基类
    /// </summary>
    abstract class RecvPacket : Packet
    {
        /// <summary>
        /// 侦解析结果
        /// </summary>
        public RecvResult ReciveResult
        {
            get;
            set;
        }

        /// <summary>
        /// 解析一个接收数据包。
        /// </summary>
        /// <param name="buf">接收到的缓冲区数据</param>
        /// <returns>解析是否成功，如果失败，当前交互无效</returns>
        public abstract bool ParsePacket(byte[] buf);

        
    }
}
