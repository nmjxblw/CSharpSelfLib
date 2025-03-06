using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine
{
	/// <summary>
	/// 
	/// </summary>
	public class DopamineTcpClient : TcpClient
	{
		/// <summary>
		/// 监听器
		/// </summary>
		public TcpListener Listener { get; set; } = new TcpListener(IPAddress.Any, 60000);
		/// <summary>
		/// 运行标识符
		/// </summary>
		public bool IsRunning { get; set; } = false;
		/// <summary>
		/// 启动
		/// </summary>
		public void Start()
		{
			Listener ??= new TcpListener(IPAddress.Any, 60000);
			IsRunning = true;
			Listener.Start();
			Listener.BeginAcceptTcpClient(HandleClientConnected, null);
		}
		/// <summary>
		/// 处理连结
		/// </summary>
		/// <param name="asyncResult"></param>
		public void HandleClientConnected(IAsyncResult asyncResult)
		{
			while (IsRunning)
			{

				try
				{
					// 获取客户端对象
					TcpClient client = Listener.EndAcceptTcpClient(asyncResult);

					// 继续监听新连接
					Listener.BeginAcceptTcpClient(HandleClientConnected, null);

					// 处理客户端通信（新线程中执行）
					Thread clientThread = new Thread(() => ProcessClient());
					clientThread.Start();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
		/// <summary>
		/// 客户端回复线程
		/// </summary>
		public void ProcessClient()
		{
			string? clientEndPoint = this.Client?.RemoteEndPoint?.ToString();
			try
			{
				using (NetworkStream stream = this.GetStream())
				{
					byte[] buffer = new byte[1024];
					int bytesRead;

					// 持续接收数据
					while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
					{
						// 将接收的字节转换为字符串
						string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
						Console.WriteLine($"[接收来自 {clientEndPoint}]: {receivedData}");

						// 生成响应数据（示例：原样返回+时间戳）
						Console.Write($"回复：");
						byte[] bytes = (Console.ReadLine() ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
						   .Select(s => Convert.ToByte(s, 16))
						   .ToArray();

						// 发送响应
						stream.Write(bytes, 0, bytes.Length);
						Console.WriteLine($"[发送至 {clientEndPoint}]: {string.Join(" ", bytes.Select(b => b.ToString("x")))}");
					}
				}
			}
			catch (IOException ex)
			{
				Console.WriteLine($"客户端 {clientEndPoint} 异常断开: {ex.Message}");
			}
			finally
			{
				this.Close();
				Console.WriteLine($"客户端 {clientEndPoint} 连接已关闭");
			}
		}

	}
}
