using log4net;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

public class FrameManager
{
	#region Log4net集成
	private static readonly ILog _frameLogger = LogManager.GetLogger("FrameFileAppender");
	private static readonly ILog _errorLogger = LogManager.GetLogger("ErrorFileAppender");

	static FrameManager()
	{
		log4net.Config.XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()));
	}
	#endregion

	#region 单例构造
	private static readonly Lazy<FrameManager> _instance =
		new Lazy<FrameManager>(() => new FrameManager(), LazyThreadSafetyMode.ExecutionAndPublication);

	public static FrameManager Instance => _instance.Value;
	#endregion

	#region 线程安全缓存实现
	private const int MaxCacheSize = 1000;
	private readonly ConcurrentQueue<FrameData> _frameCache = new ConcurrentQueue<FrameData>();
	private readonly object _cacheLock = new object();
	#endregion

	public void AddFrame<T>(T data, bool direction, DateTime? time = null)
	{
		var frameData = new FrameData
		{
			Content = Convert.ToString(data),
			Timestamp = time ?? DateTime.Now,
			Direction = direction
		};
		if (data is byte[] byteArray)
		{
			frameData.Content = BitConverter.ToString(byteArray).Replace("-", " ");
		}
		UpdateCache(frameData);
		LogFrame(frameData);
	}

	private void UpdateCache(FrameData data)
	{
		lock (_cacheLock)
		{
			if (_frameCache.Count >= MaxCacheSize)
			{
				_frameCache.TryDequeue(out _);
			}
			_frameCache.Enqueue(data);
		}
	}

	private void LogFrame(FrameData data)
	{
		try
		{
			// 使用log4net上下文传递方向信息
			ThreadContext.Properties["direction"] = data.Direction ? "local >>> remote" : "local <<< remote";

			// 使用log4net的格式化日志
			_frameLogger.Info(data.Content);
		}
		catch (Exception ex)
		{
			_errorLogger.Error($"Frame logging failed: {ex.Message}", ex);
		}
		finally
		{
			ThreadContext.Properties["direction"] = null;
		}
	}

	#region 数据实体
	private class FrameData
	{
		public string Content { get; set; }
		public DateTime Timestamp { get; set; }
		public bool Direction { get; set; }
	}
	#endregion
}

