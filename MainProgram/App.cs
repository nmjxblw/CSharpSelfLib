using Newtonsoft.Json;
using System.Collections.Concurrent;
using DeepSeekApi;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
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
		string testString = "\n\t";
		Dopamine.Recorder.Write("Test" + testString);
	}
}