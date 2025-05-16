using System;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace Dopamine
{
    /// <summary>
    /// 剪切板助手类
    /// </summary>
    /// <example>
    /// [STAThread]
    ///static void Main(string[] args)
    ///{
    ///    // 业务逻辑
    ///}
    ///public static void SetText(string text)
    ///{
    ///    if (!OpenClipboard(IntPtr.Zero))
    ///    {
    ///        throw new Exception("无法打开剪切板");
    ///     }
    ///     try
    ///     {
    ///         EmptyClipboard();
    ///         IntPtr hGlobal = Marshal.StringToHGlobalUni(text); // 将字符串转为非托管内存指针
    ///         SetClipboardData(CF_UNICODETEXT, hGlobal);
    ///     }
    ///     finally
    ///     {
    ///         CloseClipboard();
    ///         Marshal.FreeHGlobal(hGlobal); // 释放非托管内存（需确保在关闭剪切板后执行）
    ///     }
    /// }
    /// </example>
    public static class Clipboard
    {
        /// <summary>
        /// 打开剪切板
        /// </summary>
        /// <param name="hWndNewOwner"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);
        /// <summary>
        /// 关闭剪切板
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool CloseClipboard();
        /// <summary>
        /// 清空剪切板
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();
        /// <summary>
        /// 设置剪切板内容
        /// </summary>
        /// <param name="uFormat"></param>
        /// <param name="hMem"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
        /// <summary>
        /// Unicode 文本格式标识
        /// </summary>
        public const uint CF_UNICODETEXT = 13;
    }
}