using System;
using System.Collections.Generic;
using System.Text;

namespace ZH.SocketModule.Packet
{
    /// <summary>
    /// 数据包基类，提供最基本的数据包操作
    /// </summary>
    abstract class Packet
    {

        /// <summary>
        /// 默认实现 ，返回类命名空间名称
        /// </summary>
        /// <returns></returns>
        public virtual string GetPacketName()
        {
            return this.GetType().FullName;
        }

        /// <summary>
        /// 获取包的解析
        /// </summary>
        /// <returns></returns>
        public virtual string GetPacketResolving()
        {
            return "没有实现解析";
        }
        /// <summary>
        /// 发送后等待返回时间（ms）
        /// </summary>
        /// <returns></returns>
        public virtual int WaiteTime()
        {
            return 200;
        }

        /// <summary>
        /// 获取包的类型，2018的带协议否、、、默认实现0端口数据发送,、、、1握手联接2端口设置3端口打开4端口关闭
        /// </summary>
        /// <returns></returns>
        public virtual int GetPacketType()
        {
            return 0;
        }

    }
}
