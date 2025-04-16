using Newtonsoft.Json;
using System.Collections.Concurrent;
using DeepSeekApi;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.Formatters;
namespace MainProgram;
/// <summary>
/// 主程序
/// </summary>
public sealed class App
{
	public void Start()
	{
		DeepSeekControl.Run();
	}
}
