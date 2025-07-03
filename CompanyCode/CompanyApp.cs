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
            string hexString = "68 13 FE 0B 01 FF 08 00 01 00";
            byte[] bytes = hexString.HexStringToBytes();
            bytes = bytes.AppendXor(false);
            BitConverter.ToString(bytes).Replace("-", " ").ShowInConsole(true);
        }
    }
}
