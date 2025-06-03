using System.Collections.Generic;
using System.Reflection;
using StardewModdingAPI;

namespace BirbCore.Attributes;

public class Log
{
	private static readonly Dictionary<string, IMonitor> Monitors = new Dictionary<string, IMonitor>();

	internal static void Init(IMonitor monitor, Assembly caller)
	{
		string assembly = caller.FullName;
		if (!Monitors.TryAdd(assembly, monitor))
		{
			monitor.Log("Assembly " + assembly + " has already initialized Log...Are there two dlls with the same assembly name?", (LogLevel)4);
		}
	}

	public static void Debug(string str)
	{
		Monitors[GetKey(Assembly.GetCallingAssembly())].Log(str, (LogLevel)1);
	}

	public static void Trace(string str)
	{
		Monitors[GetKey(Assembly.GetCallingAssembly())].Log(str, (LogLevel)0);
	}

	public static void Info(string str)
	{
		Monitors[GetKey(Assembly.GetCallingAssembly())].Log(str, (LogLevel)2);
	}

	public static void Warn(string str)
	{
		Monitors[GetKey(Assembly.GetCallingAssembly())].Log(str, (LogLevel)3);
	}

	public static void Error(string str)
	{
		Monitors[GetKey(Assembly.GetCallingAssembly())].Log(str, (LogLevel)4);
	}

	public static void Alert(string str)
	{
		Monitors[GetKey(Assembly.GetCallingAssembly())].Log(str, (LogLevel)5);
	}

	private static string GetKey(Assembly assembly)
	{
		if (assembly.FullName != null && Monitors.ContainsKey(assembly.FullName))
		{
			return assembly.FullName;
		}
		return Assembly.GetExecutingAssembly().FullName;
	}
}
