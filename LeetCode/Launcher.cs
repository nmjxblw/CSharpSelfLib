using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeetCode
{
    /// <summary>
    /// LeetCode启动器
    /// </summary>
    public static class Launcher
    {
        /// <summary>
        /// Application实例化
        /// </summary>
        private static LeetCodeApp App { get; } = new LeetCodeApp();

        /// <summary>
        /// 主方法，用于启动程序
        /// </summary>
        [STAThread]
        public static void Main()
        {
            #region 进程锁
            new Mutex(true, "LeetCodeTraining", out bool ret);
            if (!ret)
            {
                Environment.Exit(0);
            }
            #endregion
            Console.WriteLine("LeetCode训练程序启动。");
            App.Start();
        }
    }
}
