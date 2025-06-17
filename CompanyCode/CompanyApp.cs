using Aspose.Cells;
using Microsoft.AspNetCore.Mvc.Internal;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;

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
            var test = 0;
            TestFunction(ref test);
            test.ShowInConsole(true);
        }

        public void TestFunction(ref int temp)
        {
            temp = 10;
        }
    }
}
