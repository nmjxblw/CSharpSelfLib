using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Dopamine.Framework
{
    // 拓展方法第一部分，常用调试代码
    /// <summary>
    /// 拓展域名下的额外静态方法
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// 在【输出】中将字符串打印出来
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="showDateTime">添加时间戳</param>
        public static string ShowInTrace(this object input, bool showDateTime = false)
        {
            string result = $"{(showDateTime ? DateTime.Now.ToString("[HH:mm:ss]\t") : string.Empty)}{input}";
            Trace.WriteLine(result);
            return result;
        }
        /// <summary>
        /// 在终端中打印文本信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="showDateTime"></param>
        public static string ShowInConsole(this object input, bool showDateTime = false)
        {
            string result = $"{(showDateTime ? DateTime.Now.ToString("[HH:mm:ss]\t") : string.Empty)}{input}";
            Console.WriteLine(result);
            return result;
        }
        /// <summary>
        /// 数组转字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToString<T>(this T[] input)
        {
            return string.Join(",", input);
        }
        /// <summary>
        /// 获取最大值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T Max<T>(this IEnumerable<T> input) where T : IComparable
        {
            if (!input.Any()) return default;
            T result = input.GetEnumerator().Current;
            foreach (T v in input)
            {
                result = v.CompareTo(result) > 0 ? v : result;
            }
            return result;
        }
        /// <summary>
        /// 泛型钳夹
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="LowerLimit"></param>
        /// <param name="UpperLimit"></param>
        /// <returns></returns>
        public static T Clamp<T>(this T input, T LowerLimit, T UpperLimit) where T : IComparable
        {
            T low = LowerLimit.CompareTo(UpperLimit) > 0 ? UpperLimit : LowerLimit;
            T high = UpperLimit.CompareTo(LowerLimit) > 0 ? UpperLimit : LowerLimit;
            input = input.CompareTo(low) > 0 ? input : low;
            input = input.CompareTo(high) < 0 ? input : high;
            return input;
        }

        /// <summary>
        /// 将文本从原始编码格式转化为目标编码格式
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="original"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string ConvertEncoding(this string inputString, Encoding original, Encoding target)
        {
            byte[] bytes = original.GetBytes(inputString);
            byte[] convertedBytes = Encoding.Convert(original, target, bytes);
            return target.GetString(convertedBytes);
        }

        /// <summary>
        /// 呼出控制台执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        public static void CallConsole(this string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo("powershell", args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Process process = Process.Start(psi);
            if (process == null) throw new Exception($"{args}不是有效指令。");
            string output = process.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(output))
            {
                output.ShowInConsole();
            }
            else
            {
                "没有输出结果".ShowInConsole();
            }
            Console.ReadLine();
            process.WaitForExit();
        }
    }
}
