using System;
using System.Diagnostics;

namespace StardewValley.Util;

public class StackTraceHelper
{
	/// <summary>The underlying stack trace object.</summary>
	private object _StackTrace;

	/// <summary>The current stack trace information.</summary>
	public static string StackTrace => Environment.StackTrace;

	/// <summary>The number of frames in the stack trace.</summary>
	public int FrameCount => (this._StackTrace as StackTrace)?.FrameCount ?? 0;

	public static string FromException(Exception ex)
	{
		return ex?.StackTrace ?? "";
	}

	/// <summary>Construct an instance.</summary>
	public StackTraceHelper()
	{
		this._StackTrace = new StackTrace();
	}

	/// <summary>Gets the specified stack frame.</summary>
	public StackFrame GetFrame(int index)
	{
		return (this._StackTrace as StackTrace)?.GetFrame(index);
	}

	/// <summary>Returns a copy of all stack frames in the current stack trace.</summary>
	public StackFrame[] GetFrames()
	{
		return (this._StackTrace as StackTrace)?.GetFrames() ?? LegacyShims.EmptyArray<StackFrame>();
	}

	/// <summary>Builds a readable representation of the stack trace.</summary>
	public new string ToString()
	{
		return (this._StackTrace as StackTrace)?.ToString() ?? "";
	}
}
