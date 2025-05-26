using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Search;

namespace Dopamine
{
    /// <summary>
    /// DataTime API 示例
    /// </summary>
    public class DateTimeAPIExample
    {
        /// <summary>
        /// 在控制台显示当前时间的各种格式
        /// </summary>
        public static void Show()
        {
            Process.Start("cmd.exe", "/c cls"); // 清除控制台

            ($"DateTime.Now.ToBinary():{DateTime.Now.ToBinary()}\n" +
                $"DateTime.Now.ToFileTime():{DateTime.Now.ToFileTime()}\n" +
                $"DateTime.Now.ToLocalTime():{DateTime.Now.ToLocalTime()}\n" +
                $"DateTime.Now.ToLongDateString():{DateTime.Now.ToLongDateString()}\n" +
                $"DateTime.Now.ToLongTimeString():{DateTime.Now.ToLongTimeString()}\n" +
                $"DateTime.Now.ToOADate():{DateTime.Now.ToOADate()}\n" +
                $"DateTime.Now.ToShortDateString():{DateTime.Now.ToShortDateString()}\n" +
                $"DateTime.Now.ToShortTimeString():{DateTime.Now.ToShortTimeString()}\n" +
                $"DateTime.Now.ToUniversalTime():{DateTime.Now.ToUniversalTime()}\n" +
                $"DateTime.Now.ToFileTimeUtc():{DateTime.Now.ToFileTimeUtc()}\n").ShowInConsole(false);
        }
    }
}
