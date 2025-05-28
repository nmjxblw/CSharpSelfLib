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
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n（输入exit/quit/q退出）校验码: ");
                Console.ForegroundColor = ConsoleColor.Green;
                string input = Console.ReadLine() ?? string.Empty;
                if (input.ToLower().Equals("exit") || input.ToLower().Equals("quit") || input.ToLower().Equals("q"))
                {
                    break;
                }
                else
                {
                    byte[] inputByte = input.ByteStringToBytes();
                    byte[] crc = inputByte.GetCRCCheck();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"CRC16校验码: {BitConverter.ToString(crc).Replace("-", " ").ToUpper()}");
                }
            }
        }
    }
}
