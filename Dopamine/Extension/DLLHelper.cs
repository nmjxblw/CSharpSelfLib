using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Dopamine
{
    /// <summary>
    /// DLL帮助类
    /// </summary>
    public static class DLLHelper
    {
        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MessageBox(IntPtr hWnd, string text, string caption, int options);
    }
}
