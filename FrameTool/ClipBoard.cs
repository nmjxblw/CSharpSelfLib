using System.Runtime.InteropServices;
/// <summary>
/// 剪切板助手类
/// </summary>
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