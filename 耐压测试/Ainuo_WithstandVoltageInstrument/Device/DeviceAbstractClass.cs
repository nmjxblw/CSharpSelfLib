using System;
using System.Collections.Generic;
using ZH.SocketModule.Packet;
using System.Text;

namespace ZH
{
    #region 2018初始化包
    /// <summary>
    /// 初始化2018数据包
    /// </summary>
    internal class RequestInit2018PortPacket : SendPacket
    {
        private string m_strSetting = "";
        public RequestInit2018PortPacket(string strSetting)
        {
            m_strSetting = strSetting;
        }

        public override byte[] GetPacketData()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            string str_Data = "init " + m_strSetting.Replace(',', ' ');
            byte[] byt_Data = ASCIIEncoding.ASCII.GetBytes(str_Data);
            buf.Put(byt_Data);
            return buf.ToByteArray();
        }

    }
    #endregion

    #region CLT11协议数据包基类
    /// <summary>
    /// 科陆CLT11协议接收数据包基类
    /// </summary>
    internal abstract class ZHRecvPacket_CLT11 : RecvPacket
    {
        /// <summary>
        /// 包头
        /// </summary>
        protected byte PacketHead = 0x7B;
        ///// <summary>
        ///// 发信节点
        ///// </summary>
        //protected byte MyID = 0x80;
        ///// <summary>
        ///// 受信节点
        ///// </summary>
        //protected byte ToID = 0x10;
        /// <summary>
        /// 解析数据包
        /// </summary>
        /// <param name="buf">缓冲区接收到的数据包内容</param>
        /// <returns>解析是否成功</returns>
        public override bool ParsePacket(byte[] buf)
        {
            //第一步，验证包长度
            //第二步，验证包结构
            //第三步，拆帧
            ByteBuffer pack = new ByteBuffer(buf);
            int iLocalSum = 1;
            PacketHead = pack.Get();
            //ToID = pack.Get();
            //MyID = pack.Get();
            byte dataLength = pack.Get();
            if (buf.Length < dataLength || dataLength<5) return false;
            byte[] data = pack.GetByteArray(dataLength - 5);
            byte chkCode = pack.Get();

            while (buf[dataLength - iLocalSum] == 0)
            {
                iLocalSum++;
            }
            //计算校验码
            byte chkSum = GetChkSum(buf, 1, dataLength - iLocalSum);
            
            
            //if (chkCode != chkSum) return false;
            ParseBody(data);
            return true;
        }
        /// <summary>
        /// 计算检验码[帧头不进入检验范围]
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        protected byte GetChkSum(byte[] bytData, int startPos, int length)
        {
            byte bytChkSum = 0;
            for (int int_Inc = startPos; int_Inc < length; int_Inc++)
            {
                bytChkSum ^= bytData[int_Inc];
            }
            return bytChkSum;
        }
        /// <summary>
        /// 解析数据域
        /// </summary>
        /// <param name="data">数据域</param>
        protected abstract void ParseBody(byte[] data);


        /// <summary>
        /// 单个字节由低位向高位取值，
        /// </summary>
        /// <param name="input">单个字节</param>
        /// <param name="index">起始0,1,2..7</param>
        /// <returns></returns>
        protected int GetbitValue(byte input, int index)
        {
            int value;
            value = index > 0 ? input >> index : input;
            return value &= 1;
        }

        /// <summary>
        /// 3字节转换为Float
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="dotLen"></param>
        /// <returns></returns>
        protected float get3ByteValue(byte[] bytData, int dotLen)
        {
            float data = 0F;

            data = bytData[0] << 16;
            data += bytData[1] << 8;
            data += bytData[2];

            data = (float)(data / Math.Pow(10, dotLen));
            return data;
        }

        ///<summary>
        /// 替换byteSource目标位的值
        ///</summary>
        ///<param name="byteSource">源字节</param>
        ///<param name="location">替换位置(0-7)</param>
        ///<param name="value">替换的值(1-true,0-false)</param>
        ///<returns>替换后的值</returns>
        protected byte ReplaceTargetBit(byte byteSource, short location, bool value)
        {
            Byte baseNum = (byte)(Math.Pow(2, location + 1) / 2);
            return ReplaceTargetBit(location, value, byteSource, baseNum);
        }

        ///<summary>
        /// 替换byteSource目标位的值
        ///</summary>
        ///<param name="location"></param>
        ///<param name="value">替换的值(1-true,0-false)</param>
        ///<param name="byteSource"></param>
        ///<param name="baseNum">与 基数(1,2,4,8,16,32,64,128)</param>
        ///<returns></returns>
        private byte ReplaceTargetBit(short location, bool value, byte byteSource, byte baseNum)
        {
            bool locationValue = GetbitValue(byteSource, location) == 1 ? true : false;
            if (locationValue != value)
            {
                return (byte)(value ? byteSource + baseNum : byteSource - baseNum);
            }
            return byteSource;
        }
    }

    /// <summary>
    /// 科陆CLT1.1协议发送包基类
    /// </summary>
    internal abstract class ZHSendPacket_CLT11 : SendPacket
    {
        /// <summary>
        /// 包尾
        /// </summary>
        protected byte PacketTail = 0x7D;
        /// <summary>
        /// 包头
        /// </summary>
        protected byte PacketHead = 0x7B;
        /// <summary>
        /// 发信节点
        /// </summary>
        public byte MyID = 0xFE;
        /// <summary>                                                                
        /// 受信节点
        /// </summary>
        protected byte ToID = 0x13;
        //0xFE, 0x13

        public ZHSendPacket_CLT11() { IsNeedReturn = true; }
        public ZHSendPacket_CLT11(bool needReplay) { IsNeedReturn = needReplay; }

        /// <summary>
        /// 组帧
        /// </summary>
        /// <returns>完整的数据包内容</returns>
        public override byte[] GetPacketData()
        {
            //       01H—切换单相、三相继电器
            //    发送：68H+ID+SD+LEN+01H+NUM+DATA+CS  （DATA=2bytes）
            //DATA内容：01 01—2bytes 输出继电器吸合脉冲
            //            02 01—2bytes输出继电器断开脉冲
            //例如：
            //吸合  68   13  01  09  01  01    01 01  1B
            //断开  68   13  01  09  01  01    02 01  18  
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(PacketHead);        //帧头
            //buf.Put(ToID);              //发信节点
            //buf.Put(MyID);              //受信节点
            byte[] body = GetBody();
            if (body == null)
                return null;
            byte packetLength = (byte)(body.Length + 4);//帧头+帧尾+校验+长度
            buf.Put(packetLength);      //帧长度
            buf.Put(body);              //数据域 
            byte chkSum = GetChkSum(buf.ToByteArray());
            buf.Put(chkSum);
            buf.Put(PacketTail);

            return buf.ToByteArray();
        }

        protected abstract byte[] GetBody();


        /// <summary>
        /// 计算检验码[帧头不进入检验范围]
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        protected byte GetChkSum(byte[] bytData)
        {
            //byte bytChkSum = 0x00;
            //for (int int_Inc = 1; int_Inc < bytData.Length; int_Inc++)
            //{
            //    bytChkSum ^= bytData[int_Inc];
            //}
            //return bytChkSum;
            byte chk = 0x00;
            //for (int i = 1; i < data.Length; i++)
            //    chk ^= data[i];
            for (int i = 1; i < bytData.Length; i++)
            {
                chk += bytData[i];
            }
            return chk;
        }

        /// <summary>
        /// 二进制字符串转换成16进制Hex
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte Str2ToByte(string str2)
        {
            int num = Convert.ToInt32(str2, 2);
            return Convert.ToByte(num);
        }
    }


    /// <summary>
    /// CLT1.1协议发送包基类
    /// </summary>
    internal abstract class CLT11SendPacket : SendPacket
    {
        protected byte PacketHead = 0x7B;
        /// <summary>
        /// 包尾
        /// </summary>
        protected byte PacketTail = 0x7D;
        /// <summary>
        /// 发信节点
        /// </summary>
        public byte MyID { get; set; }

        /// <summary>
        /// 受信节点
        /// </summary>
        protected byte ToID { get; set; }

        public CLT11SendPacket(byte myID, byte toId, bool needReplay)
        {
            MyID = myID;
            ToID = toId;
            IsNeedReturn = needReplay;
        }

        /// <summary>
        /// 组帧
        /// </summary>
        /// <returns>完整的数据包内容</returns>
        public override byte[] GetPacketData()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            buf.Put(PacketHead);              //帧头
            //buf.Put(ToID);              //发信节点
            //buf.Put(MyID);              //受信节点
            //buf.Put(0x80);              //地址
            byte[] body = GetBody();
            if (body == null) return null;
            byte packetLength = (byte)(body.Length + 4);//帧头4字节+CS一字节
            buf.Put(packetLength);      //帧长度
            buf.Put(body);              //数据域 
            byte chkSum = GetChkSum(buf.ToByteArray());
            buf.Put(chkSum);
            buf.Put(PacketTail);

            return buf.ToByteArray();
        }

        protected abstract byte[] GetBody();


        /// <summary>
        /// 计算检验码[帧头不进入检验范围]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected byte GetChkSum(byte[] data)
        {
            byte chk = 0x00;
            //for (int i = 1; i < data.Length; i++)
            //    chk ^= data[i];
            for (int i = 1; i < data.Length; i++)
            {
                chk+=data[i];
            }
            return chk;
        }
    }

    /// <summary>
    /// CLT11协议接收数据包基类
    /// </summary>
    internal abstract class CLT11RecvPacket : RecvPacket
    {
        public CLT11RecvPacket(byte myID, byte toId)
        {
            MyID = myID;
            ToID = toId;
        }

        /// <summary>
        /// 发信节点
        /// </summary>
        protected byte MyID = 0x80;
        /// <summary>
        /// 受信节点
        /// </summary>
        protected byte ToID = 0x10;
        /// <summary>
        /// 解析数据包
        /// </summary>
        /// <param name="buf">缓冲区接收到的数据包内容</param>
        /// <returns>解析是否成功</returns>
        public override bool ParsePacket(byte[] buf)
        {
            //第一步，验证包长度
            //第二步，验证包结构
            //第三步，拆帧
            ByteBuffer pack = new ByteBuffer(buf);
            int headIndex = -1;  //帧头的位置 0x81
            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i] == 0x4F)
                {
                    headIndex = i;
                    break;
                }
            }
            if (headIndex == -1) return false;

            if (MyID != 0xFF && buf[headIndex + 1] != MyID) return false;//目标地址不一致
            if (ToID != 0xFF && buf[headIndex + 2] != ToID) return false;//上位机地址不一致
            int len = buf[headIndex + 3]; //整帧长度
            if (buf.Length < len || len < 6) return false;//长度不够

            byte chk = GetChkSum(buf, 1, len - 2);
            //if (chk != buf[headIndex + len - 1]) return false;//校验码不对,  由于功耗板返回校验码不正确

            byte[] data = new byte[len - 5];
            Array.Copy(buf, 4, data, 0, data.Length);

            ParseBody(data);
            return true;
        }

        /// <summary>
        /// 计算检验码[帧头不进入检验范围]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected byte GetChkSum(byte[] data, int startPos, int length)
        {
            byte chk = 0;
            for (int i = startPos; i < startPos + length; i++)
            {
                chk ^= data[i];
            }
            return chk;
        }
        /// <summary>
        /// 解析数据域
        /// </summary>
        /// <param name="data">数据域</param>
        protected abstract void ParseBody(byte[] data);


        /// <summary>
        /// 获取字节指定索引的位，低位向高位取值，
        /// </summary>
        /// <param name="input">单个字节</param>
        /// <param name="index">0-指定最低位，7-指定最高位</param>
        /// <returns></returns>
        protected int GetbitValue(byte input, int index)
        {
            int value = index > 0 ? input >> index : input;
            return value &= 1;
        }

        ///<summary>
        /// 替换byteSource目标位的值
        ///</summary>
        ///<param name="byteSource">源字节</param>
        ///<param name="location">替换位置(0-7)</param>
        ///<param name="value">替换的值(1-true,0-false)</param>
        ///<returns>替换后的值</returns>
        protected byte ReplaceTargetBit(byte byteSource, short location, bool value)
        {
            byte baseNum = (byte)(Math.Pow(2, location + 1) / 2);
            return ReplaceTargetBit(location, value, byteSource, baseNum);
        }

        ///<summary>
        /// 替换byteSource目标位的值
        ///</summary>
        ///<param name="location"></param>
        ///<param name="value">替换的值(1-true,0-false)</param>
        ///<param name="byteSource"></param>
        ///<param name="baseNum">与 基数(1,2,4,8,16,32,64,128)</param>
        ///<returns></returns>
        private byte ReplaceTargetBit(short location, bool value, byte byteSource, byte baseNum)
        {
            bool locationValue = GetbitValue(byteSource, location) == 1 ? true : false;
            if (locationValue != value)
            {
                return (byte)(value ? byteSource + baseNum : byteSource - baseNum);
            }
            return byteSource;
        }
    }
    #endregion


}
