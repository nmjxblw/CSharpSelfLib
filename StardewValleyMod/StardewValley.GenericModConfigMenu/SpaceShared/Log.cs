using System.Diagnostics;
using StardewModdingAPI;

namespace SpaceShared;

internal class Log
{
	public static IMonitor Monitor;

	public static bool IsVerbose => Log.Monitor.IsVerbose;

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public static void DebugOnlyLog(string str)
	{
		Log.Monitor.Log(str, (LogLevel)1);
	}

	[DebuggerHidden]
	[Conditional("DEBUG")]
	public static void DebugOnlyLog(string str, bool pred)
	{
		if (pred)
		{
			Log.Monitor.Log(str, (LogLevel)1);
		}
	}

	[DebuggerHidden]
	public static void Verbose(string str)
	{
		Log.Monitor.VerboseLog(str);
	}

	public static void Trace(string str)
	{
		Log.Monitor.Log(str, (LogLevel)0);
	}

	public static void Debug(string str)
	{
		Log.Monitor.Log(str, (LogLevel)1);
	}

	public static void Info(string str)
	{
		Log.Monitor.Log(str, (LogLevel)2);
	}

	public static void Warn(string str)
	{
		Log.Monitor.Log(str, (LogLevel)3);
	}

	public static void Error(string str)
	{
		Log.Monitor.Log(str, (LogLevel)4);
	}
}
