using System;
using System.Collections.Generic;
using System.Text;

namespace ZH
{
    public class DataFormart
    {
        /// <summary>
        /// 数组中的数据反转
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public static void ByteReverse(byte[] value, int offset, int len)
        {
            for (int i = 0; i < len / 2; i++)
            {
                byte b = value[offset + i];
                value[offset + i] = value[offset + len - i - 1];
                value[offset + len - i - 1] = b;
            }
        }

        /// <summary>
        /// 数组中的数据反转
        /// </summary>
        /// <param name="value"></param>
        public static void ByteReverse(byte[] value)
        {
            ByteReverse(value, 0, value.Length);
        }

        /// <summary>
        /// 格式化 数据
        /// 返回 4个字节
        /// </summary>
        /// <param name="value">要格式化的数据</param>
        /// <param name="digit">小数位数</param>
        /// <returns></returns>
        public static byte[] Formart(double value, int digit, bool isLittleEndian)
        {
            value *= Math.Pow(10, digit);
            value = Math.Round(value, 0);
            uint uTmp = (uint)value;
            return Formart(uTmp, isLittleEndian);
        }

        public static byte[] Formart(uint value, bool isLittleEndian)
        {
            byte[] bTmp = BitConverter.GetBytes(value);

            if (isLittleEndian == false)
            {
                ByteReverse(bTmp);
            }
            return bTmp;
        }

        /// <summary>
        /// 格式化数据
        /// 返回 两个字节
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] Formart(ushort value, bool isLittleEndian)
        {
            byte[] b = BitConverter.GetBytes(value);

            if (isLittleEndian == false)
            {
                ByteReverse(b);
            }

            return b;
        }

        /// <summary>
        /// 格式化 一个整形数字
        /// 例如 int i=12345;
        /// 返回
        /// byte{49,50,51,52,53}
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isReverse">是否反顺序</param>
        /// <returns></returns>
        public static byte[] FormartToASCIIByte(int value, bool isReverse)
        {
            byte[] buf = Encoding.ASCII.GetBytes(value.ToString());

            if (isReverse == true)
            {
                ByteReverse(buf);
            }
            return buf;
        }

        /// <summary>
        /// 格式化 一个整形数字
        /// 例如 int i=12345;
        /// 返回
        /// byte{49,50,51,52,53}
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] FormartToASCIIByte(int value)
        {
            return FormartToASCIIByte(value, false);
        }

        /// <summary>
        /// 将字符串 
        /// 转换成 float 数组
        /// </summary>
        /// <param name="values"></param>
        /// <param name="spilter">分割符</param>
        /// <returns></returns>
        public static float[] ParseStringToFloat(string values, char spilter)
        {
            string[] strs = values.Split(new char[] { spilter }, StringSplitOptions.RemoveEmptyEntries);
            float[] va = new float[strs.Length];

            for (int i = 0; i < strs.Length; i++)
            {
                va[i] = float.Parse(strs[i]);
            }

            return va;
        }

        /// <summary>
        /// 将字符串 
        /// 转换成 float 数组 默认使用 空格
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float[] ParseStringToFloat(string value)
        {
            return ParseStringToFloat(value, ' ');
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
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

        /// <summary>
        /// 十六进制字符串转十进制
        /// </summary>
        /// <param name="strHex"></param>
        /// <returns></returns>
        public static int HexStrToBin(string strHex)
        {
            return Convert.ToInt32(strHex, 16);
        }
    }
}
