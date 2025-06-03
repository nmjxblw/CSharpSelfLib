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
            int test = 99;
            string result = (test + 1).ToString("X4").ShowInConsole(true);
        }
    }
}
