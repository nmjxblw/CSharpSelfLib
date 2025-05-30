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
            string testString = "2|||||";
            testString.TrimEnd('|').ShowInConsole();
        }
    }
}
