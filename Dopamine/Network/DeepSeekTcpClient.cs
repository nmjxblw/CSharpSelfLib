namespace Dopamine;
/// <summary>
/// DeepSeek 写的 Tcp 客户端
/// </summary>
public class DeepSeekTcpClient
{
	private const int HeartbeatInterval = 1000;
	private const int ReconnectInterval = 5000;
	private TcpClient? client;
	private NetworkStream? stream;
	private bool isRunning;
	/// <summary>
	/// 联机
	/// </summary>
	/// <param name="server"></param>
	/// <param name="port"></param>
	public void Connect(string server, int port)
	{
		isRunning = true;
		new Thread(() =>
		{
			while (isRunning)
			{
				try
				{
					client = new TcpClient(server, port);
					stream = client.GetStream();
					Console.WriteLine("Connected to server.");

					// 心跳定时器
					Timer? heartbeatTimer = null;
					heartbeatTimer = new Timer(_ =>
					{
						try
						{
							byte[] heartbeat = { 0x11, 0x11 };
							stream.Write(heartbeat, 0, heartbeat.Length);
						}
						catch
						{
							heartbeatTimer?.Dispose();
						}
					}, null, HeartbeatInterval, HeartbeatInterval);

					byte[] buffer = new byte[1024];
					while (true)
					{
						int bytesRead = stream.Read(buffer, 0, buffer.Length);
						if (bytesRead == 0) break;

						// 处理服务器指令
						ProcessCommand(buffer, bytesRead);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Connection error: {ex.Message}");
				}
				finally
				{
					client?.Close();
					Console.WriteLine("Attempting to reconnect...");
					Thread.Sleep(ReconnectInterval);
				}
			}
		}).Start();
	}

	private void ProcessCommand(byte[] command, int length)
	{
		try
		{
			// 示例处理逻辑
			byte[] response = { 0x00, 0x00 }; // 处理成功
			stream?.Write(response, 0, response.Length);
		}
		catch
		{
			byte[] response = { 0x00, 0x01 }; // 处理失败
			stream?.Write(response, 0, response.Length);
		}
	}
	/// <summary>
	/// 
	/// </summary>
	public void Disconnect()
	{
		isRunning = false;
		client?.Close();
	}
}
