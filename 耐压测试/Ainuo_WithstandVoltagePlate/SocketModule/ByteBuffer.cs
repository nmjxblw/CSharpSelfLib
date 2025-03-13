
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// 创建一个可变长的Byte数组方便Push数据和Pop数据
    /// 数组的最大长度为1024,超过会产生溢出
    /// 数组的最大长度由常量MAX_LENGTH设定
    /// 
    /// 注:由于实际需要,可能要从左到右取数据,所以这里
    /// 定义的Pop函数并不是先进后出的函数,而是从0开始.
    /// 
    /// </summary>
    [Serializable]
     class ByteBuffer : IDisposable
    {
        private byte[] _buffer;
        private Encoder _encoder;
        private Encoding _encoding;

        /// <summary>
        /// 无后被存储区的ByteBuffer
        /// </summary>
        public static readonly ByteBuffer Null = new ByteBuffer();
        /// <summary>
        /// 获取同后备存储区连接的基础流
        /// </summary>
        protected Stream BaseStream;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ByteBuffer()
        {
            this.BaseStream = new MemoryStream();
            this._buffer = new byte[0x10];
            this._encoding = Encoding.Default;//(false, true);
            this._encoder = this._encoding.GetEncoder();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encoding">编码</param>
        public ByteBuffer(Encoding encoding)
        {
            this.BaseStream = new MemoryStream();
            this._buffer = new byte[0x10];
            this._encoding = encoding;
            this._encoder = this._encoding.GetEncoder();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer">初始数组</param>
        public ByteBuffer(byte[] buffer)
            : this(buffer, 0, buffer.Length)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBuffer"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        public ByteBuffer(byte[] buffer, int start, int count)
        {
            this.BaseStream = new MemoryStream(buffer, start, count, false);
            this.BaseStream.Position = 0;
            this._buffer = new byte[0x10];
            this._encoding = Encoding.Default;
            this._encoder = this._encoding.GetEncoder();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer">初始数组</param>
        /// <param name="encoding">编码</param>
        public ByteBuffer(byte[] buffer, Encoding encoding)
        {
            this.BaseStream = new MemoryStream(buffer);
            this.BaseStream.Position = 0;
            this._buffer = new byte[0x10];
            this._encoding = encoding;
            this._encoder = this._encoding.GetEncoder();
        }
        /// <summary>初始化流
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            Close();
            this.BaseStream = new MemoryStream();
        }
        #region "基础属性方法"
        /// <summary>
        /// 关闭当前流并释放与之关联的相关资源
        /// </summary>
        public virtual void Close()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// 释放由ByteBuffer使用得所有资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseStream.Close();
            }
        }

        /// <summary>
        /// 将清除该流所有缓冲区,并使得所有缓冲数据被写入到基础设备.
        /// </summary>
        public virtual void Flush()
        {
            this.BaseStream.Flush();
        }

        /// <summary>
        /// 设置当前流中的位置
        /// </summary>
        /// <param name="offset">相对于origin参数字节偏移量</param>
        /// <param name="origin">System.IO.SeekOrigin类型值,指示用于获取新位置的参考点</param>
        /// <returns></returns>
        public virtual long Seek(int offset, SeekOrigin origin)
        {
            return this.BaseStream.Seek((long)offset, origin);
        }

        /// <summary>
        /// 设置当前流长度
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLength(long value)
        {
            this.BaseStream.SetLength(value);
        }

        /// <summary>
        /// 检测是否还有可用字节
        /// </summary>
        /// <returns></returns>
        public bool Peek()
        {
            return BaseStream.Position >= BaseStream.Length ? false : true;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// 将整个流内容写入字节数组，而与 Position 属性无关。
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            long org = BaseStream.Position;
            BaseStream.Position = 0;
            byte[] ret = new byte[BaseStream.Length];
            BaseStream.Read(ret, 0, ret.Length);
            BaseStream.Position = org;
            return ret;
        }
        #endregion

        #region "写流方法"
        /// <summary>
        /// 压入一个布尔值,并将流中当前位置提升1
        /// </summary>
        /// <param name="value"></param>
        public void Put(bool value)
        {
            this._buffer[0] = value ? (byte)1 : (byte)0;
            this.BaseStream.Write(_buffer, 0, 1);
        }

        /// <summary>
        /// 压入一个Byte,并将流中当前位置提升1
        /// </summary>
        /// <param name="value"></param>
        public void Put(Byte value)
        {
            this.BaseStream.WriteByte(value);
        }
        /// <summary>
        /// Puts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void Put(int index, byte value)
        {
            int pos = (int)this.BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            Put(value);
            Seek(pos, SeekOrigin.Begin);
        }
        /// <summary>
        /// 压入Byte数组,并将流中当前位置提升数组长度
        /// </summary>
        /// <param name="value">字节数组</param>
        public void Put(Byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.BaseStream.Write(value, 0, value.Length);
        }

        /// <summary>
        /// 压入Byte数组,并将流中当前位置提升数组长度
        /// </summary>
        /// <param name="value">字节数组</param>
        /// <param name="index">value中从零开始的字节偏移量</param>
        /// <param name="count">要写入当前流的字节数</param>
        public void Put(Byte[] value, int index, int count)
        {
            this.BaseStream.Write(value, index, count);
        }

        /// <summary>
        /// 压入一个Char,并将流中当前位置提升1
        /// </summary>
        /// <param name="ch"></param>
        public void PutChar(char ch)
        {
            //if (char.IsSurrogate(ch))
            //{
            //    throw new ArgumentException("Arg_SurrogatesNotAllowedAsSingleChar");
            //}
            PutUShort((ushort)ch);
        }
        /// <summary>
        /// Puts the char.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="ch">The ch.</param>
        public void PutChar(int index, char ch)
        {
            int pos = (int)this.BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            PutChar(ch);
            Seek(pos, SeekOrigin.Begin);
        }
        /// <summary>
        /// 压入一个Short,并将流中当前位置提升2
        /// </summary>
        /// <param name="value"></param>
        public void PutUShort(ushort value)
        {
            this._buffer[0] = (byte)(value >> 8);
            this._buffer[1] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 2);
        }
        /// <summary>在指定位置压入一个shor值
        /// Puts the U short.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void PutUShort(int index, ushort value)
        {
            int pos = (int)this.BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            this.PutUShort(value);
            Seek(pos, SeekOrigin.Begin);
        }
        /// <summary>
        /// 压入一个Short,并将流中当前位置提升2
        /// </summary>
        /// <param name="value"></param>
        public void PutUShort_S(ushort value)
        {
            this._buffer[1] = (byte)(value >> 8);
            this._buffer[0] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 2);
        }

        /// <summary>
        /// Puts the int.
        /// </summary>
        /// <param name="value">The value.</param>
        public void PutInt(int value)
        {
            PutInt((uint)value);
        }
        /// <summary>
        /// 压入一个int,并将流中当前位置提升4
        /// </summary>
        /// <param name="value"></param>
        public void PutInt(uint value)
        {
            this._buffer[0] = (byte)(value >> 0x18);
            this._buffer[1] = (byte)(value >> 0x10);
            this._buffer[2] = (byte)(value >> 8);
            this._buffer[3] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 4);
        }
        /// <summary>
        /// 压入一个int(反转),并将流中当前位置提升4
        /// </summary>
        /// <param name="value"></param>
        public void PutInt_S(int value)
        {
            this._buffer[3] = (byte)(value >> 0x18);
            this._buffer[2] = (byte)(value >> 0x10);
            this._buffer[1] = (byte)(value >> 8);
            this._buffer[0] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 4);

        }
        /// <summary>
        /// 压入一个2字节int(反转),并将流中当前位置提升2
        /// </summary>
        /// <param name="value"></param>
        public void PutInt2_S(int value)
        {
            this._buffer[1] = (byte)(value >> 8);
            this._buffer[0] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 2);
        }
        /// <summary>
        /// 压入一个int,并将流中当前位置提升2
        /// </summary>
        /// <param name="value"></param>
        public void PutInt2(int value)
        {
            this._buffer[0] = (byte)(value >> 8);
            this._buffer[1] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 2);
        }
        /// <summary>
        /// Puts the int.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void PutInt(int index, uint value)
        {
            int pos = (int)this.BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            PutInt(value);
            Seek(pos, SeekOrigin.Begin);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pMaxLength"></param>
        /// <param name="pintOrder"></param>
        public void PutLong_S(long value, int pMaxLength, int pintOrder)
        {
            string hexValue = Convert.ToString(value, 16); //把长整型数转化成十六进制字符串

            if (hexValue.Length < pMaxLength * 2)
            {
                int intAdditional = pMaxLength * 2 - hexValue.Length;
                for (int i = 0; i < intAdditional; i++)
                    hexValue = "0" + hexValue;
            }
            string[] TmpValue = new string[pMaxLength];
            for (int i = 0; i < pMaxLength; i++)
            {
                if (pintOrder == 0)
                    TmpValue[i] = hexValue.Substring(i * 2, 2);
                else
                    TmpValue[pMaxLength - 1 - i] = hexValue.Substring(i * 2, 2);
            }
            byte[] retValue = new byte[pMaxLength];
            for (int i = 0; i < pMaxLength; i++)
            {
                int intTmpValue = Convert.ToInt32(TmpValue[i].Substring(0, 1), 16) * 0x10 + Convert.ToInt32(TmpValue[i].Substring(1, 1), 16) * 0x01;
                retValue[i] = (byte)intTmpValue;
            }
            this.BaseStream.Write(retValue, 0, pMaxLength);
        }

        /// <summary>
        /// 压入一个Long,并将流中当前位置提升8
        /// </summary>
        /// <param name="value"></param>
        public void PutLong(long value)
        {
            this._buffer[0] = (byte)(value >> 0x38);
            this._buffer[1] = (byte)(value >> 0x30);
            this._buffer[2] = (byte)(value >> 0x28);
            this._buffer[3] = (byte)(value >> 0x20);
            this._buffer[4] = (byte)(value >> 0x18);
            this._buffer[5] = (byte)(value >> 0x10);
            this._buffer[6] = (byte)(value >> 8);
            this._buffer[7] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 8);
        }
        /// <summary>
        /// Puts the long.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void PutLong(int index, long value)
        {
            int pos = (int)this.BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            PutLong(value);
            Seek(pos, SeekOrigin.Begin);
        }
        #endregion

        #region "读流方法"
        /// <summary>
        /// 读取布尔值,并将流中当前位置提升1
        /// </summary>
        /// <returns></returns>
        public bool GetBoolean()
        {
            return Get() == 0 ? false : true;
        }

        /// <summary>
        /// 读取Byte值,并将流中当前位置提升1
        /// </summary>
        /// <returns></returns>
        public byte Get()
        {
            return (byte)BaseStream.ReadByte();
        }
        /// <summary>
        /// Gets the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public byte Get(int index)
        {
            int current = (int)this.BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            byte ret = Get();
            Seek(current, SeekOrigin.Begin);
            return ret;
        }
        /// <summary>
        /// 读取count长度的Byte数组,并将流中当前位置提升count
        /// </summary>
        /// <param name="count">要从当前流中最多读取的字节数</param>
        /// <returns></returns>
        public byte[] GetByteArray(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            byte[] buffer = new byte[count];
            int num = BaseStream.Read(buffer, 0, count);
            return buffer;
        }

        /// <summary>
        /// 读取一个Char值,并将流中当前位置提升1
        /// </summary>
        /// <returns></returns>
        public char GetChar()
        {
            return (char)GetUShort();
        }
        /// <summary>
        /// Gets the char.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public char GetChar(int index)
        {
            int current = (int)BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            char c = GetChar();
            Seek(current, SeekOrigin.Begin);
            return c;
        }

        /// <summary>
        /// 读取一个short值,并将流中当前位置提升2
        /// </summary>
        /// <returns></returns>
        public ushort GetUShort()
        {
            ushort ret = (ushort)(Get() << 8 | Get());
            return ret;
        }
        /// <summary>
        /// 读取一个short值,并将流中当前位置提升2
        /// </summary>
        /// <returns></returns>
        public ushort GetUShort_S()
        {
            ushort ret = (ushort)(Get() | Get() << 8);
            return ret;
        }
        /// <summary>
        /// Gets the U short.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public ushort GetUShort(int index)
        {
            int current = (int)BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            ushort ret = GetUShort();
            Seek(current, SeekOrigin.Begin);
            return ret;
        }
        /// <summary>
        /// 读取一个Int值,并将流中当前位置提升4
        /// </summary>
        /// <returns></returns>
        public int GetInt()
        {
            int ret = (int)(Get() << 0x18 | Get() << 0x10 | Get() << 8 | Get());
            return ret;
        }
        /// <summary>
        /// 反转读取一个Int值,并将流中当前位置提升4
        /// </summary>
        /// <returns></returns>

        public int GetInt_S()
        {
            int ret = (int)(Get() | Get() << 0x8 | Get() << 0x10 | Get() << 0x18);
            return ret;
        }
        /// <summary>
        /// 读取一个float值,并将流中当前位置提升5
        /// 其中四字节数据，一字节指数
        /// </summary>
        /// <returns></returns>
        public float GetIntE1()
        {
            int ret = GetInt_S();
            sbyte power = (sbyte)Get();
            return (float)(ret * Math.Pow(10, power));
        }

        /// <summary>
        /// Gets the U int.
        /// </summary>
        /// <returns></returns>
        public uint GetUInt()
        {
            return (uint)GetInt();
        }

        /// <summary>
        /// 读取一个Long值,并将流中当前位置提升8
        /// </summary>
        /// <returns></returns>
        public long GetLong()
        {
            uint num1 = (uint)GetInt();
            uint num2 = (uint)GetInt();
            return (long)((num1 << 0x20) | num2);
        }
        /// <summary>
        /// 读取一个Long值,并将流中当前位置提升8[低位在前]
        /// </summary>
        /// <returns></returns>
        public long GetLong_S()
        {
            uint num1 = (uint)GetInt_S();
            uint num2 = (uint)GetInt_S();
            return (long)((num1 | (num2 << 0x20)));
        }

        #endregion

        #region 属性
        /// <summary>
        /// 获取用字节表示的流长度
        /// </summary>
        public int Length
        {
            get
            {
                return (int)this.BaseStream.Length;
            }
        }

        /// <summary>
        /// 获取或设置当前流中的位置
        /// </summary>
        public int Position
        {
            get
            {
                return (int)this.BaseStream.Position;
            }
            set
            {
                this.BaseStream.Position = value;
            }
        }

        /// <summary>
        /// 获取可用字节个数
        /// </summary>
        public int Remaining()
        {
            return (int)(BaseStream.Length - BaseStream.Position);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HasRemaining()
        {
            return Remaining() > 0;
        }
        /// <summary>
        /// 重绕此缓冲区。将位置设置为 0 并丢弃标记。
        /// </summary>
        public void Rewind()
        {
            Seek(0, SeekOrigin.Begin);
        }
        #endregion

        /// <summary>
        /// 读取count长度的Byte数组,并将流中当前位置提升count------排序
        /// </summary>
        /// <param name="count">要从当前流中最多读取的字节数</param>
        /// <returns></returns>
        public byte[] GetByteArrayRev(int count, bool boolX)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            byte[] buffer = new byte[count];
            int num = BaseStream.Read(buffer, 0, count);

            if (boolX)
            {
                Array.Reverse(buffer);
            }
            return buffer;
        }
    }
}

