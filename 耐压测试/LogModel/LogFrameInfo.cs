using System;
/// <summary>
/// 记录数据帧静态类
/// </summary>
public static class LogFrameInfo
{
	static LogFrameInfo()
	{
		OnLogWriteEvent += (System.Windows.Application.Current.MainWindow as VoltageInsulationTest.MainWindow).HandleLogWriteEvent;
		IsSaveLog = true;
	}

	/// <summary>
	/// 是否保存日志
	/// </summary>
	public static bool IsSaveLog = false;
	/// <summary>
	/// 日志写入事件
	/// </summary>
	public static event Action<string> OnLogWriteEvent;
	/// <summary>
	/// 是否在控制台输出日志
	/// </summary>
	public static bool IsDebugLog = false;
	/// <summary>
	/// 是否在Trace中输出
	/// </summary>
	public static bool IsTraceLog = false;
	/// <summary>
	/// 写入日志
	/// </summary>
	/// <param name="IsSend">是否是发送true发送日志,false 返回日志</param>
	public static void WriteLog(this byte[] Data, DateTime Time = default, string Port = default, bool IsSend = true, string remark = default)
	{
		if (Time == default) Time = DateTime.Now;
		string log = "[{0}] {1}{2}{3}{4}";// 时间 端口 发送方向 数据帧 注释
		string DataString = string.Empty;
		if (Data != null || Data.Length > 0)
		{
			DataString = BitConverter.ToString(Data, 0).Replace("-", " ");
		}
		log = string.Format(log,
			Time.ToString("T"),
			Port == default ? string.Empty : $"[{Port}] ",
			IsSend ? ">>> " : "<<< ",
			DataString,
			remark == default ? string.Empty : $"[{remark}]");
		if (IsDebugLog) Console.WriteLine(log);
		if (IsTraceLog) System.Diagnostics.Trace.WriteLine(log);
		if (IsSaveLog) SaveLog(log);
		OnLogWriteEvent?.Invoke(log);
	}


	/// <summary>
	/// 保存日志
	/// </summary>
	/// <param name="MessageLog">日志内容</param>
	public static void SaveLog(string MessageLog)
	{
		try
		{

			string DirectoryPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "日志", "设备日志", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);  //文件夹路径
			if (!System.IO.Directory.Exists(DirectoryPath))  //创建目录
			{
				System.IO.Directory.CreateDirectory(DirectoryPath);
				System.Threading.Thread.Sleep(500);//创建目录稍等一点延迟，以防创建失败
			}

			string FileName = System.IO.Path.Combine(DirectoryPath, $"{DateTime.Now:yyyy-MM-dd}.txt");
			System.IO.File.AppendAllText(FileName, MessageLog + "\r\n", System.Text.Encoding.UTF8);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}
}