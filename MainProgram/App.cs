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
	public void Start()
	{
		string bs = "4401F060F004";
		bs = bs.PadLeft(12, '0');
		byte[] bytes = bs.ByteStringToBytes();
		BitConverter.ToString(bytes).ShowInConsole(true);
	}
}