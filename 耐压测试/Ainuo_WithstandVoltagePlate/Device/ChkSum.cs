using System;
using System.Collections.Generic;
using System.Text;

namespace ZH
{

    /// <summary>
    /// 效验码计算
    /// </summary>
    class ChkSum
    {
        /// <summary>
        /// 计算异或效验码
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte GetChkSumXor(byte[] buff, int offset, int len)
        {
            byte chkSum = 0;

            for (int i = 0; i < len; i++)
            {
                chkSum ^= buff[i + offset];
            }

            return chkSum;
        }

        /// <summary>
        /// 计算异或效验码
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static byte GetChkSumXor(byte[] buff)
        {
            return GetChkSumXor(buff, 0, buff.Length);
        }
    }
}
