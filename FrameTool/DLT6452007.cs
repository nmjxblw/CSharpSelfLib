using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrameTool
{
    class DLT645_2007
    {
        /// <summary>
        /// 覆写ToString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "DLT645-2007电能表通讯协议";
        }
        /// <summary>
        /// 帧头
        /// </summary>
        public byte FrameHead { get; } = 0x68;
        /// <summary>
        /// 帧尾
        /// </summary>
        public byte FrameTail { get; } = 0x16;
        /// <summary>
        /// 被检表地址
        /// </summary>
        /// <remarks>6位，小端序表示</remarks>
        public byte[] MeterAddress { get; set; } = new byte[6];
        /// <summary>
        /// 强制地址
        /// </summary>
        public byte[] DefaultAddress { get; } = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
        /// <summary>
        /// 控制码
        /// </summary>
        /// <remarks>
        /// <para>D7 传送方向 0-主站发出的命令帧 1-从站发出的应答帧</para>
        /// <para>D6 从站应答标志 0-从站正确应答 1-从站异常应答</para>
        /// <para>D5 后续帧标志 0-无后续数据帧 1-有后续数据帧</para>
        /// <para>D4-0 功能码 00000:保留 01000:广播校时 10001:读数据 10100:写数据 11010:电表清零</para>
        /// </remarks>
        public byte ControlCode { get; set; } = 0b000_10001;
        /// <summary>
        /// 数据域
        /// </summary>
        public byte[] Data { get; set; } = new byte[0];
        /// <summary>
        /// 序列化数据域
        /// </summary>
        public static void SerializData(ref byte[] inputData)
        {
            if (inputData == null) return;
            for (int i = 0; i < inputData.Length; i++)
            {
                inputData[i] += 0x33;
            }
        }
        /// <summary>
        /// 将Hex String转化为字节，并以小端序表示
        /// </summary>
        /// <param name="hexString"></param>
        /// <param name="outData"></param>
        public static void ToLittleEndian(string hexString, out byte[] outData)
        {
            outData = new byte[0];
            if (string.IsNullOrWhiteSpace(hexString)) return;
            hexString = Regex.Replace(hexString.ToUpper(), @"[^0-9A-F]", "", RegexOptions.IgnoreCase);
            int totalByteCount = hexString.Length / 2 + hexString.Length % 2;
            // 补位
            hexString = hexString.PadLeft(totalByteCount * 2, '0');
            outData = new byte[totalByteCount];
            for (int i = 0; i < totalByteCount; i++)
            {
                outData[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            // 逆序
            Array.Reverse(outData);
        }
        /// <summary>
        /// 将浮点型化为整型，然后转化为字节并以小端序表示
        /// </summary>
        public static void ToLittleEndian(float inputFloat, out byte[] outData, int byteCount = 3)
        {
            outData = new byte[0];
            if (inputFloat == default) return;
            byteCount = byteCount.Clamp(1, 4);
            int roundNumber = (int)Math.Round(inputFloat);
            int limit = byteCount < 4 ? (1 << (8 * byteCount)) - 1 : int.MaxValue;
            roundNumber = roundNumber.Clamp(-limit, limit);
            byte[] temp = BitConverter.GetBytes(roundNumber);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(temp);
            outData = new byte[byteCount];
            Array.Copy(temp, outData, byteCount);
            //Array.Reverse(outData);
        }
    }
}
