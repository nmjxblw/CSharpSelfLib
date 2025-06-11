using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.ChatApp
{
    /// <summary>
    /// 控制程序休眠和屏幕关闭的管理器
    /// </summary>
    public static class SystemSleepManager
    {

        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);

        [Flags]
        enum ExecutionFlag : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,  // 阻止系统休眠
            ES_DISPLAY_REQUIRED = 0x00000002, // 阻止屏幕关闭
            ES_CONTINUOUS = 0x80000000       // 持续生效直至取消
        }
        /// <summary>
        /// 阻止系统休眠和屏幕关闭的调用
        /// </summary>
        public static void PreventSleep()
        {
            // 组合标志位，持续阻止休眠和屏幕关闭
            SetThreadExecutionState(ExecutionFlag.ES_SYSTEM_REQUIRED |
                                    ExecutionFlag.ES_DISPLAY_REQUIRED |
                                    ExecutionFlag.ES_CONTINUOUS);
        }
        /// <summary>
        /// 恢复系统默认休眠策略，允许系统休眠和屏幕关闭
        /// </summary>
        public static void RestoreSleep()
        {
            // 恢复系统默认休眠策略
            SetThreadExecutionState(ExecutionFlag.ES_CONTINUOUS);
        }
    }
}
