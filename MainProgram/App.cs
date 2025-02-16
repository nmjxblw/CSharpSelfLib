using Newtonsoft.Json;

namespace MainProgram;
/// <summary>
/// 主程序
/// </summary>
public sealed class App
{
	/// <summary>
	/// 自定义客户端
	/// </summary>
	private DopamineHttpClient Client { get; set; } = new DopamineHttpClient();
	public void Start()
	{
		HttpMessage message = new HttpMessage()
		{
			Message = @$"{DateTime.Now:g}\tHEARTBEAT",
			Flag = new
			{
				Error = "None",
				Sender = new
				{
					IP = "127.0.0.1",
					Port = 10001,
				}
			}
		};
		string getString = Client.PostStringAsync(JsonConvert.SerializeObject(message),
			$"{ConfigManager.Data.RemoteIP.Address}:{ConfigManager.Data.RemoteIP.Port.Beat}/api/heartbeat")
			.GetAwaiter().GetResult() ?? string.Empty;
		getString.ShowInConsole(true);
	}
}
