namespace Dopamine;
/// <summary>
/// 基于tcp协议创建的服务器
/// </summary>
public class TcpServer : IDisposable
{
	/// <summary>
	/// 构造函数
	/// </summary>
	public TcpServer(string localIp, int localPort)
	{
		LocalIPEndPoint = new IPEndPoint(IPAddress.Parse(localIp), localPort);
		Initialization();
	}
	/// <summary>
	/// 监听器
	/// </summary>
	public TcpListener? Listener { get; protected set; }
	/// <summary>
	/// Tcp客户端
	/// </summary>
	public TcpClient? RemoteClient { get; protected set; }	
	/// <summary>
	/// 本机IP终结点
	/// </summary>
	public IPEndPoint? LocalIPEndPoint { get; protected set; }
	/// <summary>
	/// 远端IP终结点
	/// </summary>
	public IPEndPoint? RemoteIPEndPoint { get; protected set; }
	/// <summary>
	/// 网络流
	/// </summary>
	public NetworkStream? Stream { get; protected set; }
	/// <summary>
	/// 取消操作令牌源
	/// </summary>
	public CancellationTokenSource? CancellationTokenSource { get; protected set; }
	/// <summary>
	/// 运行标识符
	/// </summary>
	public bool IsRunning { get; set; } = true;
	/// <summary>
	/// 释放标识符
	/// </summary>
	public bool Disposed { get; protected set; } = false;
	/// <summary>
	/// 初始化
	/// </summary>
	public void Initialization()
	{
		Listener = new TcpListener(LocalIPEndPoint!);
		Listener.Start();
	}
	/// <summary>
	/// 启动服务器
	/// </summary>
	public void Active()
	{
		IsRunning = true;
		Task.Run(StartListen);
	}
	/// <summary>
	/// 终止服务器
	/// </summary>
	public void Terminate()
	{
		IsRunning = false;
	}
	/// <summary>
	/// 发送
	/// </summary>
	public void Send(byte[] buffer)
	{
		if (IsRunning)
		{
			Stream!.Write(buffer, 0, buffer.Length);
		}
	}
	/// <summary>
	/// 异步等待客户端接入
	/// </summary>
	/// <returns></returns>
	public async Task StartListen()
	{
		Listener!.Start();
		RemoteClient = await Listener.AcceptTcpClientAsync();
		RemoteIPEndPoint = (IPEndPoint)RemoteClient!.Client.RemoteEndPoint!;
		
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
	/// 私有手动释放
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if (Disposed) return;

		if (disposing)
		{
			Listener!.Stop();
			Stream!.Close();
			Stream!.Dispose();
			Stream = null;
			Listener = null;
		}
		Disposed = true;
	}
	/// <summary>
	/// 析构
	/// </summary>
	~TcpServer()
	{
		Dispose(false);
	}
}

