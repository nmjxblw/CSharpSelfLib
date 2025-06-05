using Microsoft.AspNetCore.Mvc.Internal;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CompanyCode
{
    /// <summary>
    /// 主程序
    /// </summary>
    public sealed class CompanyApp
    {
        /// <summary>
        /// 运行主方法
        /// </summary>
        public void Start()
        {
            //string parameter = "1.0c";
            ////Regex.Replace(parameter, @"[^0-9.]", "").TrimEnd('.').ShowInConsole(true);
            //Regex.IsMatch(parameter, @"^((1(\.0)?)|(0?\.\d))[Lr|Cc]$").ShowInConsole(true);
            int testLong = (int)Math.Round(2.23566);
           testLong.ShowInConsole(true);
        }
    }
}
