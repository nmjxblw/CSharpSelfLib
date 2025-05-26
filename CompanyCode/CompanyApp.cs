using Microsoft.AspNetCore.Mvc.Internal;
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
           DateTimeAPIExample.Show(); // 显示当前时间的各种格式
        }
    }
}
