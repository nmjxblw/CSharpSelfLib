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
            float[] test = new float[] { 1f };
            string.Join(";", test).ShowInConsole();
        }
    }
}
