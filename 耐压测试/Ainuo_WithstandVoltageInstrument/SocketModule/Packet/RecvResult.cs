using System;
using System.Collections.Generic;
using System.Text;

namespace ZH.SocketModule.Packet
{
    /// <summary>
    /// 接收侦结果
    /// </summary>
    enum RecvResult
    {
        /// <summary>
        ///  未解析过侦
        /// </summary>
        None,

        /// <summary>
        /// 未知
        /// </summary>
        Unknow,

        /// <summary>
        /// 无此命令
        /// </summary>
        NOCOMMAND,

        /// <summary>
        /// 正确
        /// </summary>
        OK,

        /// <summary>
        /// 帧结构错误
        /// </summary>
        FrameError,

        /// <summary>
        /// 校验错误
        /// </summary>
        CSError,

        /// <summary>
        /// 数据区域逻辑错误
        /// </summary>
        DataError
    }
}
