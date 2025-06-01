using System;
using System.Collections.Generic;
using System.IO;

namespace StardewValley;

public class NetLogger
{
	private Dictionary<string, NetLogRecord> loggedWrites = new Dictionary<string, NetLogRecord>();

	private DateTime timeLastStarted;

	private double priorMillis;

	private bool isLogging;

	public bool IsLogging
	{
		get
		{
			return this.isLogging;
		}
		set
		{
			if (value != this.isLogging)
			{
				this.isLogging = value;
				if (this.isLogging)
				{
					this.timeLastStarted = DateTime.UtcNow;
				}
				else
				{
					this.priorMillis += (DateTime.UtcNow - this.timeLastStarted).TotalMilliseconds;
				}
			}
		}
	}

	public double LogDuration
	{
		get
		{
			if (this.isLogging)
			{
				return this.priorMillis + (DateTime.UtcNow - this.timeLastStarted).TotalMilliseconds;
			}
			return this.priorMillis;
		}
	}

	public void LogWrite(string path, long length)
	{
		if (this.IsLogging)
		{
			this.loggedWrites.TryGetValue(path, out var record);
			record.Path = path;
			record.Count++;
			record.Bytes += length;
			this.loggedWrites[path] = record;
		}
	}

	public void Clear()
	{
		this.loggedWrites.Clear();
		this.priorMillis = 0.0;
		this.timeLastStarted = DateTime.UtcNow;
	}

	public string Dump()
	{
		string path = Path.Combine(Program.GetLocalAppDataFolder("Profiling"), DateTime.UtcNow.Ticks + ".csv");
		using StreamWriter writer = File.CreateText(path);
		double duration = this.LogDuration / 1000.0;
		writer.WriteLine("Profile Duration: {0:F2}", duration);
		writer.WriteLine("Stack,Deltas,Bytes,Deltas/s,Bytes/s,Bytes/Delta");
		foreach (NetLogRecord record in this.loggedWrites.Values)
		{
			writer.WriteLine("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2}", record.Path, record.Count, record.Bytes, (double)record.Count / duration, (double)record.Bytes / duration, (double)record.Bytes / (double)record.Count);
		}
		return path;
	}
}
