using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Dopamine.Framework
{

    /// <summary>
    /// 控制台管理类
    /// </summary>
    public static class ConsoleManager
    {
        /// <summary>
        /// 获取当前控制台窗口句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        /// <summary>
        /// 获取当前控制台窗口句柄
        /// </summary>
        public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;
        /// <summary>
        /// 分配一个新的控制台窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        /// <summary>
        /// 释放当前控制台窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
        /// <summary>
        /// 切换控制台窗口的显示状态
        /// </summary>
        public static void Toggle()
        {
            if (HasConsole) Hide();
            else Show();
        }
        /// <summary>
        /// 显示控制台窗口
        /// </summary>
        public static void Show()
        {
            if (AllocConsole())
            {
                Console.WriteLine("控制台已激活");
            }
            else
            {
                Recorder.Write("控制台激活失败");
            }
        }
        /// <summary>
        /// 隐藏控制台窗口
        /// </summary>
        public static void Hide()
        {
            if (FreeConsole())
            {
                Recorder.Write("控制台已隐藏");
            }
            else
            {
                Recorder.Write("控制台隐藏失败");
            }
        }

    }
}
