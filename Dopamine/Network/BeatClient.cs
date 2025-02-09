namespace Dopamine;
/// <summary>
/// 客户端，向服务器发送心跳包
/// </summary>
public class BeatClient : IDisposable
{
	/// <summary>
	/// 初始化
	/// </summary>
	public BeatClient(int port, int interval = 1000)
	{
		Port = port;
		Interval = interval;
	}
	/// <summary>
	/// 启动方法
	/// </summary>
	public void Active() { Task.Run(async () => await ListenAsync()); }
	/// <summary>
	/// 监听端口
	/// </summary>
	private int Port { get; set; } = 10001;
	private int _interval = 1000;
	/// <summary>
	/// 心跳间隔,单位毫秒（ms）
	/// </summary>
	/// <remarks>区间[100,int.MaxValue]</remarks>
	public int Interval { get { return _interval; } set { _interval = Math.Max(value, 100); } }
	/// <summary>
	/// 监听器
	/// </summary>
	public TcpListener? Listener { get; set; }
	/// <summary>
	/// 远程客户端
	/// </summary>
	public TcpClient? RemoteClient { get; set; }
	/// <summary>
	/// 远程端口
	/// </summary>
	public IPEndPoint? RemoteEndPoint { get; protected set; }
	/// <summary>
	/// 数据流
	/// </summary>
	private NetworkStream? Stream { get; set; }
	/// <summary>
	/// 断连信号源
	/// </summary>
	private CancellationTokenSource? CancellationTokenSource { get; set; }
	/// <summary>
	/// 运行标识符
	/// </summary>
	public bool IsRunning { get; set; } = true;
	/// <summary>
	/// 异步监听服务器
	/// </summary>
	/// <returns></returns>
	private async Task ListenAsync()
	{
		Listener = Listener ?? new TcpListener(IPAddress.Any, Port);
		Listener.Start();
		CancellationTokenSource = new CancellationTokenSource();
		"监听器启动".ShowInTrace(true);
		while (IsRunning)
		{
			try
			{
				RemoteClient = await Listener.AcceptTcpClientAsync();
				while (RemoteClient.Connected)
				{
					// 等待直到可读取的数据流进入
					if (RemoteClient.Available <= 0) { await Task.Delay(100); continue; }

					byte[] temp = new byte[1024];
					Stream = RemoteClient.GetStream();
					int len = await Stream.ReadAsync(temp, 0, temp.Length);
					if (len > 0)
					{
						byte[] frame = new byte[len];
						Array.Copy(frame, 0, temp, 0, len);
						// 判断传入数据是否为心跳启动数据帧
						if (IsBeatStartFrame(frame))
						{
							// 记录当前远程客户端IP
							RemoteEndPoint = (IPEndPoint)RemoteClient!.Client.RemoteEndPoint!;
							await SendHeartbeatAsync(CancellationTokenSource.Token);
						}
					}

				}
			}
			catch (Exception e)
			{
				$"监听异常\t{e.Message}".ShowInTrace(true);
				break;
			}
		}
	}
	/// <summary>
	/// 在这里判断是否为心跳包启动数据帧
	/// </summary>
	/// <param name="frame"></param>
	/// <returns></returns>
	private static bool IsBeatStartFrame(byte[] frame)
	{
		return true;
	}
	/// <summary>
	/// 心跳包内容
	/// </summary>
	// TODO:改成南网《计量自动化终端技术规范 第5部分：上行通信规约》格式的心跳帧
	public byte[] HeartbeatMessage { get; set; } = Encoding.UTF8.GetBytes($"[{DateTime.Now:HH:mm:ss}] HEARTBEAT");
	/// <summary>
	/// 异步发送心跳包
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested && IsRunning)
		{
			try
			{
				if (RemoteClient!.Connected)
				{
					// 发送心跳包
					await Stream!.WriteAsync(HeartbeatMessage, cancellationToken);
					"发送心跳包".ShowInTrace(true);
				}
				else
				{
					"连接中断,等待连接".ShowInTrace(true);
					break; // 连接断开，退出循环
				}
				// 等待
				await Task.Delay(Interval, cancellationToken);
			}
			catch (Exception ex)
			{
				$"心跳包发送异常\t{ex.Message}".ShowInTrace(true);
				break; // 如果发生错误，退出循环
			}
		}
		// 清理资源
		Cleanup();
		"数据清理完毕".ShowInTrace(true);
		if (IsRunning)
			await ListenAsync();
	}

	private void Cleanup()
	{
		CancellationTokenSource?.Cancel();
		Stream?.Close();
		RemoteClient?.Close();
		Listener?.Stop();
	}
	/// <summary>
	/// 手动释放
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	/// <summary>
	/// 内部释放
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			IsRunning = false;
			Cleanup();
			CancellationTokenSource = null;
			Stream = null;
			RemoteClient?.Dispose();
			Listener = null;
		}
	}
	/// <summary>
	/// 当程序关闭时运行
	/// </summary>
	public void OnApplicationExit()
	{
		Dispose();
	}
	/// <summary>
	/// 析构
	/// </summary>
	~BeatClient()
	{
		Dispose(false);
	}
}
