using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// Represents a trace writer that writes to memory. When the trace message limit is
/// reached then old trace messages will be removed as new messages are added.
/// </summary>
public class MemoryTraceWriter : ITraceWriter
{
	private readonly Queue<string> _traceMessages;

	private readonly object _lock;

	/// <summary>
	/// Gets the <see cref="T:System.Diagnostics.TraceLevel" /> that will be used to filter the trace messages passed to the writer.
	/// For example a filter level of <see cref="F:System.Diagnostics.TraceLevel.Info" /> will exclude <see cref="F:System.Diagnostics.TraceLevel.Verbose" /> messages and include <see cref="F:System.Diagnostics.TraceLevel.Info" />,
	/// <see cref="F:System.Diagnostics.TraceLevel.Warning" /> and <see cref="F:System.Diagnostics.TraceLevel.Error" /> messages.
	/// </summary>
	/// <value>
	/// The <see cref="T:System.Diagnostics.TraceLevel" /> that will be used to filter the trace messages passed to the writer.
	/// </value>
	public TraceLevel LevelFilter { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.MemoryTraceWriter" /> class.
	/// </summary>
	public MemoryTraceWriter()
	{
		this.LevelFilter = TraceLevel.Verbose;
		this._traceMessages = new Queue<string>();
		this._lock = new object();
	}

	/// <summary>
	/// Writes the specified trace level, message and optional exception.
	/// </summary>
	/// <param name="level">The <see cref="T:System.Diagnostics.TraceLevel" /> at which to write this trace.</param>
	/// <param name="message">The trace message.</param>
	/// <param name="ex">The trace exception. This parameter is optional.</param>
	public void Trace(TraceLevel level, string message, Exception? ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
		stringBuilder.Append(" ");
		stringBuilder.Append(level.ToString("g"));
		stringBuilder.Append(" ");
		stringBuilder.Append(message);
		string item = stringBuilder.ToString();
		lock (this._lock)
		{
			if (this._traceMessages.Count >= 1000)
			{
				this._traceMessages.Dequeue();
			}
			this._traceMessages.Enqueue(item);
		}
	}

	/// <summary>
	/// Returns an enumeration of the most recent trace messages.
	/// </summary>
	/// <returns>An enumeration of the most recent trace messages.</returns>
	public IEnumerable<string> GetTraceMessages()
	{
		return this._traceMessages;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> of the most recent trace messages.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.String" /> of the most recent trace messages.
	/// </returns>
	public override string ToString()
	{
		lock (this._lock)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string traceMessage in this._traceMessages)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(traceMessage);
			}
			return stringBuilder.ToString();
		}
	}
}
