using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO.Ports;
namespace FrameTool
{
    /// <summary>
    /// 主程序
    /// </summary>
    public class MainApplication
    {
        private dynamic Params => ConfigManager.Data.PowerAndErrorCalibrationParameters;
        /// <summary>
        /// 运行主方法
        /// </summary>
        public void Run()
        {
            while (true)
            {
                "[1] - 功率/误差校准".ShowInConsole();
                "[2] - 读地址".ShowInConsole();
                "[3] - 写地址".ShowInConsole();
                "[4] - 时间校准".ShowInConsole();
                "[5] - 电量清零".ShowInConsole();
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                if (input.Equals("quit", StringComparison.CurrentCultureIgnoreCase) || input.Equals("q", StringComparison.CurrentCultureIgnoreCase)) break;
                byte[] test = GetPowerAndErrorCalibrationFrame();
                BitConverter.ToString(test).Replace("-", " ").ShowInConsole(true);
            }
        }

        private byte[] GetPowerAndErrorCalibrationFrame()
        {
            return DLT645_2007.GetPowerAndErrorCalibrationFrame(
                Params.Password, Params.Usercode, (byte)Params.Phase, (byte)Params.Method, (byte)Params.Type,
                (float)Params.VoltageA, (float)Params.VoltageB, (float)Params.VoltageC, (float)Params.CurrentA, (float)Params.CurrentB, (float)Params.CurrentC,
                (float)Params.ActivePowerA, (float)Params.ActivePowerB, (float)Params.ActivePowerC, (float)Params.ReactivePowerA, (float)Params.ReactivePowerB, (float)Params.ReactivePowerC,
                (float)Params.ErrorA, (float)Params.ErrorB, (float)Params.ErrorC);
        }
    }
}
