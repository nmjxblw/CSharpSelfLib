using System;
using System.Collections.Generic;
using System.Text;

namespace ZH.Struct
{
    /// <summary>
    /// 端口信息
    /// </summary>
    public class StPortInfo
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public int m_Port = 0;
        /// <summary>
        /// 通讯是否为串口,true UDP,false COM
        /// </summary>
        public bool m_Port_IsUDPorCom = false;
        /// <summary>
        /// IP
        /// </summary>
        public string m_IP = "";
        /// <summary>
        /// 波特率
        /// </summary>
        public string m_Port_Setting = "38400,n,8,1";
        /// <summary>
        /// 0无，1有
        /// </summary>
        public int m_Exist = 0;
    }
}
