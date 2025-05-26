using Newtonsoft.Json;
using System.Collections.Concurrent;
using DeepSeekApi;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using LLama.Batched;
namespace MainProgram;
/// <summary>
/// 主程序
/// </summary>
/// <remarks>
/// <para>注：<b>使用了sealed字段，不允许继承</b></para>
/// </remarks>
public sealed class App
{
	/// <summary>
	/// 运行主方法
	/// </summary>
	public void Start()
	{
		//LLamaManager.OutputTextChangeEvent += (text) =>
		//{
		//	Console.ForegroundColor = (ConsoleColor)11;
		//	Console.Write(text);
		//};
		//Console.ForegroundColor = ConsoleColor.Yellow;
		//Console.Write("\nThe chat session has started.");
		//while (true)
		//{
		//	Console.ForegroundColor = ConsoleColor.Yellow;
		//	Console.Write("\nUser: ");
		//	Console.ForegroundColor = ConsoleColor.Green;
		//	string input = Console.ReadLine() ?? string.Empty;
		//	if (input.ToLower().Equals("exit") || input.ToLower().Equals("quit") || input.ToLower().Equals("q"))
		//	{
		//		break;
		//	}
		//	else
		//	{
		//		LLamaManager.AskAsync(input).GetAwaiter().GetResult();
		//	}
		//}
		//LLamaApplication.Run().GetAwaiter().GetResult();
	}
}