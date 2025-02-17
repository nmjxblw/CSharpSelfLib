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
	private DopamineHttpClient? Client { get; set; }
	public void Start()
	{
		//HttpMessage message = new HttpMessage()
		//{
		//	Message = @$"{DateTime.Now:g}\tHEARTBEAT",
		//	Flag = new
		//	{
		//		Error = "None",
		//		Sender = new
		//		{
		//			IP = "127.0.0.1",
		//			Port = 10001,
		//		}
		//	}
		//};
		//string getString = Client.PostStringAsync(JsonConvert.SerializeObject(message),
		//	$"{ConfigManager.Data.RemoteIP.Address}:{ConfigManager.Data.RemoteIP.Port.Beat}/api/heartbeat")
		//	.GetAwaiter().GetResult() ?? string.Empty;
		//getString.ShowInConsole(true);
		//Client = new DopamineHttpClient();
		//string getString = Client.GetAPIAsync("self").GetAwaiter().GetResult()??"Null";
		//Client.Dispose();
		//getString.ShowInConsole(true);
		byte[] frame = new byte[] { 0x68, 0x01, 0xFE, 0x0A, 0x13, 0x60, 0x01, 0x1B, 0x01, };
		Console.WriteLine($"{GetFullAfterChkSum(frame).ToString<byte>()}");
	}
	public static byte[] GetFullAfterChkSum(byte[] data)
	{
		List<byte> ck = [.. data];
		ck.Add(0x00);
		for (int i = 1; i < data.Length; i++)
		{
			ck[data.Length] ^= data[i];
		}
		return ck.ToArray();
	}
}
