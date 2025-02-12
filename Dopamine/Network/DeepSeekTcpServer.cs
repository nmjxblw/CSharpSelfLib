namespace Dopamine;
/// <summary>
/// DeepSeek 的 Tcp 服务器
/// </summary>
public class DeepSeekTcpServer
{
	private const int HeartbeatInterval = 1000;
	private const int Timeout = 10000;
	private TcpListener? listener;
	/// <summary>
	/// 
	/// </summary>
	/// <param name="port"></param>
	public void Start(int port)
	{
		listener = new TcpListener(IPAddress.Any, port);
		listener.Start();
		Console.WriteLine("Server started. Waiting for connections...");

		new Thread(() =>
		{
			while (true)
			{
				var client = listener.AcceptTcpClient();
				Console.WriteLine("Client connected.");
				HandleClient(client);
			}
		}).Start();
	}

	private void HandleClient(TcpClient client)
	{
		var stream = client.GetStream();
		var lastActivity = DateTime.Now;
		var heartbeatCommand = new byte[] { 0x11, 0x11 };

		// 发送心跳指令
		stream.Write(heartbeatCommand, 0, heartbeatCommand.Length);

		// 超时检测定时器
		var timer = new Timer(_ =>
		{
			if ((DateTime.Now - lastActivity).TotalSeconds > Timeout / 1000)
			{
				Console.WriteLine("Connection timeout. Closing connection.");
				client.Close();
			}
		}, null, Timeout, Timeout);

		try
		{
			byte[] buffer = new byte[1024];
			while (true)
			{
				int bytesRead = stream.Read(buffer, 0, buffer.Length);
				if (bytesRead == 0) break;

				lastActivity = DateTime.Now;

				// 处理客户端响应
				if (buffer[0] == 0x11 && buffer[1] == 0x11)
				{
					Console.WriteLine("Received heartbeat");
				}
				else
				{
					Console.WriteLine($"Received response: {BitConverter.ToString(buffer, 0, bytesRead)}");
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Connection error: {ex.Message}");
		}
		finally
		{
			timer.Dispose();
			client.Close();
			Console.WriteLine("Client disconnected.");
		}
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="command"></param>
	public void SendCommand(byte[] command)
	{
		// 实际应用中需要管理多个客户端连接
		// 这里简化为向最近连接的客户端发送
		// 需要根据实际需求修改客户端管理逻辑
	}
}
