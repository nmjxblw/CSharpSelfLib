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
        /// <summary>
        /// 运行主方法
        /// </summary>
        public void Run()
        {
            float test = -3450000f, low = 0f, hight = 10f;
            DLT645_2007.ToLittleEndian(test, out byte[] frame, 3);
            BitConverter.ToString(frame).Replace("-", " ").ShowInConsole(true);
        }
    }
}
