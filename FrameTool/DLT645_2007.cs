using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrameTool
{
    /// <summary>
    /// 
    /// </summary>
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
        /// 前导字节，用于唤醒下位机
        /// </summary>
        /// <remarks>由4个FE构成</remarks>
        public static byte[] InvokeFrame { get; } = new byte[] { 0xFE, 0xFE, 0xFE, 0xFE };
        /// <summary>
        /// 帧头
        /// </summary>
        public static byte FrameHead { get; } = 0x68;
        /// <summary>
        /// 帧尾
        /// </summary>
        public static byte FrameTail { get; } = 0x16;
        /// <summary>
        /// 被检表地址
        /// </summary>
        /// <remarks>6位，小端序表示</remarks>
        public byte[] MeterAddress { get; set; } = new byte[6];
        /// <summary>
        /// 强制地址
        /// </summary>
        public static byte[] DefaultAddress { get; } = Enumerable.Repeat((byte)0xAA, AddressByteCount).ToArray();
        /// <summary>
        /// 地址字节数
        /// </summary>
        public static int AddressByteCount => ConfigManager.Data.AddressByteCount;
        /// <summary>
        /// 密码字节数
        /// </summary>
        public static int PasswordByteCount => ConfigManager.Data.PasswordByteCount;
        /// <summary>
        /// 操作者代码字节数
        /// </summary>
        public static int UsercodeByteCount => ConfigManager.Data.UsercodeByteCount;
        /// <summary>
        /// 控制码
        /// </summary>
        /// <remarks>
        /// <para>D7 传送方向 0-主站发出的命令帧 1-从站发出的应答帧</para>
        /// <para>D6 从站应答标志 0-从站正确应答 1-从站异常应答</para>
        /// <para>D5 后续帧标志 0-无后续数据帧 1-有后续数据帧</para>
        /// <para>D4-0 功能码 
        /// <list type="table">
        /// <item>00000:保留</item> 
        /// <item>01000:广播校时 </item>
        /// <item>10001:读数据</item>
        /// <item>10010:读后续数据</item>
        /// <item>10011:读通信地址</item>
        /// <item>10100:写数据</item>
        /// <item>10101:写通信地址</item>
        /// <item>10110:冻结命令</item>
        /// <item>10111:更改通信速率</item>
        /// <item>11000:修改密码</item>
        /// <item>11001:最大需量清零</item>
        /// <item>11010:电表清零</item>
        /// <item>11011:时间清零</item>
        /// </list></para>
        /// </remarks>
        public byte ControlCode { get; set; } = 0b000_10001;
        /// <summary>
        /// 默认控制码，读地址
        /// </summary>
        public static byte DefaultControlCode { get; } = 0b000_10001;
        /// <summary>
        /// 默认数据域，空数据
        /// </summary>
        public static byte[] DefaultData { get; } = new byte[0];
        private static int _ResponseTime = 20;
        /// <summary>
        /// 从站响应时间
        /// </summary>
        /// <remarks>单位ms</remarks>
        public static int ResponseTime
        {
            get { return _ResponseTime; }
            set { _ResponseTime = Math.Max(Math.Min(value, 500), 20); }
        }
        private static int _ByteInterval = 0;
        /// <summary>
        /// 每字节最大间隔时间
        /// </summary>
        /// <remarks>单位ms</remarks>
        public static int ByteInterval
        {
            get { return _ByteInterval; }
            set { _ByteInterval = Math.Max(Math.Min(value, 500), 0); }
        }
        /// <summary>
        /// 加密/解密数据域
        /// </summary>
        public static void ProcessData(ref byte[] inputData, bool encode = true)
        {
            if (inputData == null) return;
            int offset = encode ? 0x33 : -0x33;
            for (int i = 0; i < inputData.Length; i++)
            {
                inputData[i] = (byte)(inputData[i] + offset);
            }
        }
        /// <summary>
        /// 组帧
        /// </summary>
        /// <param name="outFrame">输出帧</param>
        /// <param name="address">表地址<para>自动逆序，无需处理</para></param>
        /// <param name="controlCode">控制码</param>
        /// <param name="data">数据域<para>需要手动处理逆序</para></param>
        public static void AssembleFrame(out byte[] outFrame, out byte[] remainingFrame, byte controlCode = default, byte[]? data = default, byte[]? address = default)
        {
            outFrame = new byte[0];
            remainingFrame = new byte[0];
            List<byte> tempFrame = new List<byte> { FrameHead };
            if (address == default) address = DefaultAddress;
            // 小端序表示
            Array.Reverse(address);
            tempFrame.AddRange(address);
            tempFrame.Add(FrameHead);
            // 须先处理数据域才能确定控制码内容
            if (data == default) data = DefaultData;
            if (controlCode == default) controlCode = DefaultControlCode;
            // 数据域字节数由控制码内容和数据域内容共同决定
            byte Length = GetLength(ref controlCode, ref data, out byte[] temp);
            if (temp.Length > 0)
                remainingFrame = temp;
            tempFrame.Add(controlCode);
            tempFrame.Add(Length);
            ProcessData(ref data, true);
            tempFrame.AddRange(data);
            byte SumCheck = GetSumCheck(tempFrame);
            tempFrame.Add(SumCheck);
            tempFrame.Add(FrameTail);
            outFrame = tempFrame.ToArray();
        }
        /// <summary>
        /// 获取数据域字节数
        /// </summary>
        /// <remarks>LEN 为数据域的字节数。读数据时 LEN≤200，写数据时 LEN≤50，LEN=0 表示无数据域。</remarks>
        /// <param name="controlCode">控制码</param>
        /// <param name="data">数据域</param>
        /// <param name="remainingFrame">剩余帧</param>
        /// <returns>数据长度位应当填写的字节</returns>
        public static byte GetLength(ref byte controlCode, ref byte[] data, out byte[] remainingFrame)
        {
            remainingFrame = new byte[0];
            byte MaxLength = (byte)200;
            bool isWrite = false;
            // 判断是否为写控制码
            if ((byte)((controlCode >> 2) & 1) == 1 && (byte)((controlCode >> 4) & 1) == 1)
            {
                MaxLength = (byte)50;
                isWrite = true;
            }

            // 判断是否超出写入上限
            if (MaxLength < data.Length && isWrite)
            {
                // 写报文需要改为有后续帧
                controlCode |= 1 << 5;
                byte[] modifiedData = new byte[MaxLength];
                Array.Copy(data, modifiedData, MaxLength);
                int remainingLength = data.Length - MaxLength;
                remainingFrame = new byte[remainingLength];
                Array.Copy(data, MaxLength, remainingFrame, 0, remainingLength);
                data = modifiedData;
            }
            return (byte)data.Length;
        }
        /// <summary>
        /// 计算和校验
        /// </summary>
        /// <param name="inputFrame"></param>
        /// <returns></returns>
        public static byte GetSumCheck(IEnumerable<byte> inputFrame)
        {
            byte result = 0x00;
            foreach (byte @byte in inputFrame)
            {
                result += @byte;
            }
            return result;
        }
        /// <summary>
        /// 将Hex String转化为字节，并以小端序表示
        /// </summary>
        /// <param name="hexString"></param>
        /// <param name="outData"></param>
        public static byte[] ToLittleEndian(string? hexString, int totalByteCount = default)
        {
            byte[] outData = new byte[0];
            if (string.IsNullOrWhiteSpace(hexString)) return outData;
            hexString = Regex.Replace(hexString.ToUpper(), @"[^0-9A-F]", "", RegexOptions.IgnoreCase);
            if (totalByteCount == default)
                totalByteCount = hexString.Length / 2 + hexString.Length % 2;
            else
                totalByteCount = Math.Max(totalByteCount, 0);
            // 补位
            hexString = hexString.PadLeft(totalByteCount * 2, '0');
            outData = new byte[totalByteCount];
            for (int i = 0; i < totalByteCount; i++)
            {
                outData[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            // 逆序
            Array.Reverse(outData);
            return outData;
        }
        /// <summary>
        /// 浮点型转化为字节
        /// </summary>
        /// <param name="input"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static byte[] ToLittleEndian(float input, int byteCount = 3)
        {
            // 转换float为int（四舍五入）
            int intValue = (int)Math.Round(input);

            // Clamp字节数到[1,4]范围
            int clampedByteCount = Math.Max(1, Math.Min(4, byteCount));

            // 计算该字节数的补码范围
            int bits = 8 * clampedByteCount;
            long min = -(1L << (bits - 1));
            long max = (1L << (bits - 1)) - 1;
            int minInt = (int)min;
            int maxInt = (int)max;

            // Clamp整数值到补码范围
            int clampedValue = Math.Max(minInt, Math.Min(intValue, maxInt));

            // 转换为大端序字节数组
            byte[] bytes = new byte[clampedByteCount];
            for (int i = 0; i < clampedByteCount; i++)
            {
                int shift = 8 * (clampedByteCount - 1 - i);
                bytes[i] = (byte)((clampedValue >> shift) & 0xFF);
            }
            // 小端序表示
            Array.Reverse(bytes);
            return bytes;
        }

        #region --- 功能实例化 ---

        #region --- 读取表地址 ---
        /// <summary>
        /// 获取读地址指令
        /// </summary>
        /// <param name="userInputAddress"></param>
        /// <returns></returns>
        public static byte[] GetReadAddressFrame(string userInputAddress)
        {
            byte[] initialAddress = ToLittleEndian(userInputAddress, AddressByteCount);
            if (initialAddress.Length <= 0) Array.Copy(DefaultAddress, initialAddress, AddressByteCount);
            Array.Reverse(initialAddress);
            AssembleFrame(out byte[]? outFrame, out _, 0b000_10011, DefaultData, initialAddress);
            if (outFrame == null)
                throw new Exception("数据帧为空");
            return outFrame;
        }
        #endregion

        #region --- 写入表地址 ---
        /// <summary>
        /// 获取写地址指令
        /// </summary>
        /// <param name="newAddress">新地址</param>
        /// <returns></returns>
        public static byte[] GetWirteAddressFrame(string newAddress)
        {
            byte[] newAddressBytes = ToLittleEndian(newAddress, AddressByteCount);
            if (newAddressBytes.Length <= 0) Array.Copy(DefaultAddress, newAddressBytes, AddressByteCount);
            AssembleFrame(out byte[]? outFrame, out _, 0b000_10101, newAddressBytes);
            if (outFrame == null)
                throw new Exception("数据帧为空");
            return outFrame;
        }
        #endregion

        #region --- 电表清零 ---
        /// <summary>
        /// 获得电表清零指令
        /// </summary>
        /// <param name="password"></param>
        /// <param name="usercode"></param>
        /// <returns></returns>
        public static byte[] GetEnergyClearFrame(string? password = default, string? usercode = default)
        {
            if (string.IsNullOrWhiteSpace(password)) password = new string('0', 2 * PasswordByteCount);
            if (string.IsNullOrWhiteSpace(usercode)) usercode = new string('0', 2 * UsercodeByteCount);
            byte[] passwordBytes = ToLittleEndian(password, PasswordByteCount);
            byte[] usercodeBytes = ToLittleEndian(usercode, UsercodeByteCount);
            List<byte> tempData = new List<byte>();
            tempData.AddRange(passwordBytes);
            tempData.AddRange(usercodeBytes);
            AssembleFrame(out byte[]? outFrame, out _, 0b000_10101, tempData.ToArray());
            if (outFrame == null)
                throw new Exception("数据帧为空");
            return outFrame;
        }
        #endregion

        #region --- 功率/误差校准（读命令校表） ---
        /// <summary>
        /// 获取功率/误差校准指令
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="usercode">操作者代码</param>
        /// <param name="phase">升源方式</param>
        /// <param name="method">校准方法</param>
        /// <param name="type">校准类型</param>
        /// <param name="Ua">A相电压</param>
        /// <param name="Ub">B相电压</param>
        /// <param name="Uc">C相电压</param>
        /// <param name="Ia">A相电流</param>
        /// <param name="Ib">B相电流</param>
        /// <param name="Ic">C相电流</param>
        /// <param name="Pa">A相有功功率</param>
        /// <param name="Pb">B相有功功率</param>
        /// <param name="Pc">C相有功功率</param>
        /// <param name="Qa">A相无功功率</param>
        /// <param name="Qb">B相无功功率</param>
        /// <param name="Qc">C相无功功率</param>
        /// <param name="ErrA">A相误差</param>
        /// <param name="ErrB">B相误差</param>
        /// <param name="ErrC">C相误差</param>
        /// <returns></returns>
        public static byte[] GetPowerAndErrorCalibrationFrame(
            string? password, string? usercode, byte phase, byte method, byte type,
            float Ua, float Ub, float Uc, float Ia, float Ib, float Ic, float Pa, float Pb, float Pc, float Qa, float Qb, float Qc,
            float ErrA, float ErrB, float ErrC)
        {
            List<byte> tempData = new List<byte>();
            // 数据标识
            byte[] DI = new byte[] { 0x02, 0x80, 0xFF, 0x01 };
            byte[] passwordBytes = ToLittleEndian(password, PasswordByteCount);
            if (passwordBytes.Length <= 0) passwordBytes = Enumerable.Repeat((byte)0x00, PasswordByteCount).ToArray();
            byte[] usercodeBytes = ToLittleEndian(usercode, UsercodeByteCount);
            if (usercodeBytes.Length <= 0) usercodeBytes = Enumerable.Repeat((byte)0x00, UsercodeByteCount).ToArray();

            byte[] VoltageA, VoltageB, VoltageC, CurrentA, CurrentB, CurrentC, ActivePowerA,
                ActivePowerB, ActivePowerC, ReactivePowerA, ReactivePowerB, ReactivePowerC,
                ErrorA, ErrorB, ErrorC;
            VoltageA = ToLittleEndian(Ua);
            VoltageB = ToLittleEndian(Ub);
            VoltageC = ToLittleEndian(Uc);
            CurrentA = ToLittleEndian(Ia);
            CurrentB = ToLittleEndian(Ib);
            CurrentC = ToLittleEndian(Ic);
            ActivePowerA = ToLittleEndian(Pa, 4);
            ActivePowerB = ToLittleEndian(Pb, 4);
            ActivePowerC = ToLittleEndian(Pc, 4);
            ReactivePowerA = ToLittleEndian(Qa, 4);
            ReactivePowerB = ToLittleEndian(Qb, 4);
            ReactivePowerC = ToLittleEndian(Qc, 4);
            ErrorA = ToLittleEndian(ErrA);
            ErrorB = ToLittleEndian(ErrB);
            ErrorC = ToLittleEndian(ErrC);
            foreach (byte[] bytes in new List<byte[]> { DI,passwordBytes,usercodeBytes,
                new byte[]{phase},new byte[]{method},new byte[]{type},
                VoltageA, VoltageB, VoltageC,
                CurrentA, CurrentB, CurrentC, ActivePowerA,ActivePowerB, ActivePowerC, ReactivePowerA, ReactivePowerB, ReactivePowerC,
                ErrorA, ErrorB, ErrorC})
            {
                Array.Reverse(bytes);
                tempData.AddRange(bytes);
            }
            AssembleFrame(out byte[]? outFrame, out _, 0b000_10001, tempData.ToArray());
            if (outFrame == null)
                throw new Exception("数据帧为空");
            return outFrame;
        }
        #endregion

        #region --- 时间校准（广播校准时间） ---
        public static byte[] GetTimeCalibrationFrame()
        {
            List<byte> tempData = new List<byte>();
            byte[] DI = new byte[] { 0x02, 0x80, 0xFF, 0x02 };
            Array.Reverse(DI);
            tempData.AddRange(DI);
            byte Second, Minute, Hour, Day, Month, Year;
            DateTime currentMoment = DateTime.Now;
            Second = (byte)currentMoment.Second;
            Minute = (byte)currentMoment.Minute;
            Hour = (byte)currentMoment.Hour;
            Day = (byte)currentMoment.Day;
            Month = (byte)currentMoment.Month;
            Year = (byte)(currentMoment.Year % 100);
            foreach (byte @byte in new List<byte> { Second, Minute, Hour, Day, Month, Year })
            {
                tempData.Add(@byte);
            }
            AssembleFrame(out byte[]? outFrame, out _, 0b000_10000, tempData.ToArray());
            if (outFrame == null)
                throw new Exception("数据帧为空");
            return outFrame;
        }
        #endregion

        #region --- 恢复出厂设置 ---
        public static byte[] GetResetDefaultFrame(string? password = default, string? usercode = default)
        {
            if (string.IsNullOrWhiteSpace(password)) password = new string('0', 2 * PasswordByteCount);
            if (string.IsNullOrWhiteSpace(usercode)) usercode = new string('0', 2 * UsercodeByteCount);
            byte[] passwordBytes = ToLittleEndian(password, PasswordByteCount);
            byte[] usercodeBytes = ToLittleEndian(usercode, UsercodeByteCount);
            List<byte> tempData = new List<byte>();
            byte[] DI = new byte[] { 0x02, 0x80, 0xFF, 0x00 };
            Array.Reverse(DI);
            tempData.AddRange(DI);
            tempData.AddRange(passwordBytes);
            tempData.AddRange(usercodeBytes);
            byte[] Data = new byte[] { 0x01 };
            tempData.AddRange(Data);
            AssembleFrame(out byte[]? outFrame, out _, 0b000_10001, tempData.ToArray());
            if (outFrame == null)
                throw new Exception("数据帧为空");
            return outFrame;
        }
        #endregion

        #endregion
    }
}
