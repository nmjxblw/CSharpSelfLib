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
        /// Represents the constant value for the "OK" button in a message box.
        /// </summary>
        /// <remarks>This value is used to specify the "OK" button when configuring message box
        /// options.</remarks>
        public const uint MB_OK = 0x00000000;
        /// <summary>
        /// Represents the constant value for the "Cancel", "Try Again", and "Continue" button combination  used in
        /// message box configurations.
        /// </summary>
        /// <remarks>This constant can be used with message box APIs to specify a button configuration
        /// that includes  "Cancel", "Try Again", and "Continue" options.</remarks>
        public const uint MB_CANCELTRYCONTINUE = 0x00000006;
        /// <summary>
        /// Represents the constant value for the Help button flag in a message box.
        /// </summary>
        /// <remarks>This constant can be used to specify that a Help button should be displayed in a
        /// message box. It corresponds to the <c>MB_HELP</c> flag in the Windows API.</remarks>
        public const uint MB_HELP = 0x00004000;
        /// <summary>
        /// Represents the value used to specify the "Hand" icon in a message box.
        /// </summary>
        /// <remarks>This constant is typically used with message box functions to display a "Hand" icon, 
        /// which is often associated with error or critical messages.</remarks>
        public const uint MB_ICONHAND = 0x00000010;
        /// <summary>
        /// Represents the constant value for the "Question" icon used in message box displays.
        /// </summary>
        /// <remarks>This value corresponds to the <see
        /// href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messagebox">MessageBox</see>
        /// API in the Windows API. It is used to specify the "Question" icon when displaying a message box.</remarks>
        public const uint MB_ICONQUESTION = 0x00000020;
        /// <summary>
        /// Represents the value for the MB_ICONEXCLAMATION constant used in Windows API message box functions.
        /// </summary>
        /// <remarks>This constant specifies that an exclamation mark icon should be displayed in the
        /// message box. It is typically used with functions such as MessageBox in the Windows API.</remarks>
        public const uint MB_ICONEXCLAMATION = 0x00000030;
        /// <summary>
        /// Represents the value for the "Asterisk" icon in a message box, as defined by the Windows API.
        /// </summary>
        /// <remarks>This constant can be used to specify the "Asterisk" icon when displaying a message
        /// box using native Windows API calls. The value corresponds to the MB_ICONASTERISK flag.</remarks>
        public const uint MB_ICONASTERISK = 0x00000040;
        /// <summary>
        /// Specifies that a user-defined icon should be displayed in a message box.
        /// </summary>
        /// <remarks>This constant can be used with message box functions to indicate that a custom icon, 
        /// defined by the user, should be displayed instead of the default icons.</remarks>
        public const uint MB_USERICON = 0x00000080;
        /// <summary>
        /// Represents a constant value used to display a warning icon in a message box.
        /// </summary>
        /// <remarks>This constant is equivalent to <see cref="MB_ICONEXCLAMATION"/> and can be used with
        /// message box functions  to specify a warning icon.</remarks>
        public const uint MB_ICONWARNING = MB_ICONEXCLAMATION;
        /// <summary>
        /// Represents the constant value for an error icon used in message boxes.
        /// </summary>
        /// <remarks>This constant is equivalent to <see cref="MB_ICONHAND"/> and can be used to specify
        /// an error icon when displaying a message box.</remarks>
        public const uint MB_ICONERROR = MB_ICONHAND;
        /// <summary>
        /// Represents the constant value for the "Information" icon used in message boxes.
        /// </summary>
        /// <remarks>This constant is equivalent to <see cref="MB_ICONASTERISK"/> and can be used to
        /// specify the "Information" icon  when displaying a message box using Windows API functions.</remarks>
        public const uint MB_ICONINFORMATION = MB_ICONASTERISK;
        /// <summary>
        /// Represents the value for the "Stop" icon used in message boxes.
        /// </summary>
        /// <remarks>This constant is equivalent to <see cref="MB_ICONHAND"/> and can be used to specify
        /// the "Stop" icon in message box configurations.</remarks>
        public const uint MB_ICONSTOP = MB_ICONHAND;
        /// <summary>
        /// Represents the identifier for the "OK" button in a dialog box.
        /// </summary>
        /// <remarks>This constant is typically used in dialog box implementations to identify the "OK"
        /// button. The value is set to <see langword="1"/>.</remarks>
        public const int IDOK = 1;
        /// <summary>
        /// Represents the identifier for a cancel operation.
        /// </summary>
        /// <remarks>This constant is typically used to indicate that a user has canceled an operation,
        /// such as closing a dialog box or aborting a process.</remarks>
        public const int IDCANCEL = 2; // 取消
        /// <summary>
        /// Represents the constant value for a "Yes" response.
        /// </summary>
        /// <remarks>This constant is typically used in scenarios where a "Yes" response is represented by
        /// an integer value.</remarks>
        public const int IDYES = 6;    // 是
        /// <summary>
        /// Represents the constant value for "No" in a decision or confirmation context.
        /// </summary>
        public const int IDNO = 7;     // 否

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

        /// <summary>
        /// Retrieves a handle to the window that is currently in the foreground.
        /// </summary>
        /// <remarks>The foreground window is the window that is currently receiving user input. This
        /// method is typically used in scenarios where interaction with the active window is required.</remarks>
        /// <returns>An <see cref="IntPtr"/> representing the handle to the foreground window. If no window is in the foreground,
        /// the return value is <see langword="IntPtr.Zero"/>.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
    }
}
