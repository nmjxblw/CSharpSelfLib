using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZH.SocketModule.Packet;
using ZH;
using ZH.Struct;

namespace ZH
{
    #region 耐压仪发送

    /// <summary>
    ///耐压仪联机/脱机请求包
    /// </summary>
    internal class WithstandVoltageInstrument_RequestLinkPacket : ZH1104SendPacket
    {
        public bool IsLink = true;
        public byte Address = 0x00;
        /// <summary>
        /// 控制码
        /// </summary>
        public byte ContrnlType = 0x11;
        public byte Len = 0x01;
        public byte[] Data = new byte[0];

        public WithstandVoltageInstrument_RequestLinkPacket(byte contrnlType, byte[] data)
            : base()
        {
            ContrnlType = contrnlType;
            //Len = (byte)((byte)data.Length+0x06); 
            Data = data;
        }
        public WithstandVoltageInstrument_RequestLinkPacket()
        { 
        
        }

        protected override byte[] GetBody()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.Initialize();
            //buf.Put(Len);          //命令 
            buf.Put(Address);          //命令 
            buf.Put(ContrnlType);
            if (Data.Length>0)
            {
                buf.Put(Data);
            }
            return buf.ToByteArray();
        }

        public override string GetPacketResolving()
        {
            string strResolve = "没有解析";
            return strResolve;
        }
    }
    /// <summary>
    /// 耐压仪联机返回指令
    /// </summary>
    internal class WithstandVoltageInstrument_RequestLinkReplyPacket : ZH1104RecvPacket
    {
        public string OutData ="";
        protected override void ParseBody(byte[] data)
        {
            if (data[0] == 0x4E)
            {
                ByteBuffer buf = new ByteBuffer(data);
                //   0x4F(帧头) + 地址  +  控制码  +  数据长度n  +  数据 + 校验码 + 0x45（结束符）
                buf.Get(); //0x87
                buf.Get(); //地址
                buf.Get(); //控制码
                int len= (int)buf.Get(); //长度
                byte[] d = new byte[len];
                for (int i = 0; i < len; i++)  //数据
                {
                    d[i] = buf.Get();
                }
                OutData = byteToHexStr(d);
            }
            else
                ReciveResult = RecvResult.Unknow;



        }
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public  string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        public override string GetPacketResolving()
        {
            string strResolve = "返回：" + ReciveResult.ToString();
            return strResolve;
        }
     
    }


    #endregion




}
