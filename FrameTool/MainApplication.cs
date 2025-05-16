using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO.Ports;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
namespace FrameTool
{
    /// <summary>
    /// 主程序
    /// </summary>
    public class MainApplication
    {
        private static dynamic ReadAddressParams => ConfigManager.Data.ReadAddressParameters;
        private static dynamic WriteAddressParams => ConfigManager.Data.WriteAddressParameters;
        private static dynamic EnergyClearParams => ConfigManager.Data.EnergyClearParameters;
        private static dynamic PowerAndErrorCalibrationParams => ConfigManager.Data.PowerAndErrorCalibrationParameters;
        private static dynamic ResetDefaultParameters => ConfigManager.Data.ResetDefaultParameters;
        /// <summary>
        /// 运行主方法
        /// </summary>
        [STAThread]
        public void Run()
        {
            while (true)
            {
                Console.Clear();
                if (!ConfigManager.ReloadJson())
                {
                    "重新载入Json文件失败！".ShowInConsole();
                    break;
                }
                string.Format("输入编号以获取数据帧\n" +
                "[1] - 功率/误差校准\n" +
                "[2] - 读地址\n" +
                "[3] - 写地址\n" +
                "[4] - 时间校准\n" +
                "[5] - 电量清零\n" +
                "[6] - 电能表参数重置\n").ShowInConsole();
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input)) continue;
                if (input.Equals("quit", StringComparison.CurrentCultureIgnoreCase)
                    || input.Equals("q", StringComparison.CurrentCultureIgnoreCase))
                    break;
                input = Regex.Replace(input, @"[^0-9]", "", RegexOptions.IgnoreCase);
                if (string.IsNullOrEmpty(input)) continue;
                if (!int.TryParse(input, out int code)) continue;
                byte[] frame = new byte[0];
                try
                {
                    switch (code)
                    {
                        case 1:
                            {
                                frame = GetPowerAndErrorCalibrationFrame();
                                break;
                            }
                        case 2:
                            {
                                frame = DLT645_2007.GetReadAddressFrame(ReadAddressParams.InitialAddress);
                                break;
                            }
                        case 3:
                            {
                                frame = DLT645_2007.GetWirteAddressFrame(WriteAddressParams.NewAddress);
                                break;
                            }
                        case 4:
                            {
                                frame = DLT645_2007.GetTimeCalibrationFrame();
                                break;
                            }
                        case 5:
                            {
                                frame = DLT645_2007.GetEnergyClearFrame(EnergyClearParams.Password, EnergyClearParams.Usercode);
                                break;
                            }
                        case 6:
                            {
                                frame = DLT645_2007.GetResetDefaultFrame(ResetDefaultParameters.Password, ResetDefaultParameters.Usercode);
                                break;
                            }
                        default:
                            {
                                continue;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Recorder.RecordError(ex.StackTrace);
                    Environment.Exit(1);
                    break;
                }
                string frameString = BitConverter.ToString(frame).Replace("-", " ");
                if (!Clipboard.OpenClipboard(IntPtr.Zero))
                {
                    throw new Exception("无法打开剪切板");
                }
                IntPtr hGlobal = new IntPtr();
                try
                {
                    Clipboard.EmptyClipboard();
                    hGlobal = Marshal.StringToHGlobalUni(frameString); // 将字符串转为非托管内存指针
                    Clipboard.SetClipboardData(Clipboard.CF_UNICODETEXT, hGlobal);
                }
                finally
                {
                    Clipboard.CloseClipboard();
                    Marshal.FreeHGlobal(hGlobal); // 释放非托管内存（需确保在关闭剪切板后执行）
                }
                string msg = string.Format("功能[{0}]输出帧：{1}", code, frameString);
                Recorder.Record(msg);
                msg.ShowInConsole();
                Console.ReadKey();
            }
        }

        private byte[] GetPowerAndErrorCalibrationFrame()
        {
            return DLT645_2007.GetPowerAndErrorCalibrationFrame(
                PowerAndErrorCalibrationParams.Password, PowerAndErrorCalibrationParams.Usercode, (byte)PowerAndErrorCalibrationParams.Phase, (byte)PowerAndErrorCalibrationParams.Method, (byte)PowerAndErrorCalibrationParams.Type,
                (float)PowerAndErrorCalibrationParams.VoltageA, (float)PowerAndErrorCalibrationParams.VoltageB, (float)PowerAndErrorCalibrationParams.VoltageC, (float)PowerAndErrorCalibrationParams.CurrentA, (float)PowerAndErrorCalibrationParams.CurrentB, (float)PowerAndErrorCalibrationParams.CurrentC,
                (float)PowerAndErrorCalibrationParams.ActivePowerA, (float)PowerAndErrorCalibrationParams.ActivePowerB, (float)PowerAndErrorCalibrationParams.ActivePowerC, (float)PowerAndErrorCalibrationParams.ReactivePowerA, (float)PowerAndErrorCalibrationParams.ReactivePowerB, (float)PowerAndErrorCalibrationParams.ReactivePowerC,
                (float)PowerAndErrorCalibrationParams.ErrorA, (float)PowerAndErrorCalibrationParams.ErrorB, (float)PowerAndErrorCalibrationParams.ErrorC);
        }
    }
}
