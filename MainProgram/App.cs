﻿using DeepSeekApi;
using LLama.Batched;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
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
        this.Invoke("TestMethod1")?.ToString().ShowInConsole(true);
    }

   static string TestMethod() => "Successed";
}