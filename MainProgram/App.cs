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
	private DopamineTcpClient? Client { get; set; }
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
		//byte[] frame = new byte[] { 0x68, 0x01, 0xFE, 0x08, 0x10, 0x30, 0x00, 0x00 };
		//Console.WriteLine($"{CheckSum(frame).ToString("x")}");
		List<byte[]> TestStartDataIdentityCollection =
			new List<byte[]> {
				new byte[] { 0xa0, 0x20, 0x00, 0x00 },
				new byte[] { 0xA0, 0x20, 0x01, 0x00 },
				new byte[] { 0xa0, 0x20, 0xff, 0x00 }
			};
		byte[] CallbackTestStartDataIdentity = new byte[] { 0xa0, 0x20, 0xff, 0x00 };
		bool exists = TestStartDataIdentityCollection
	.Any(bytes => bytes.SequenceEqual(CallbackTestStartDataIdentity));
		Console.WriteLine(exists);
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
	/// <summary>
	/// 和校验
	/// </summary>
	/// <param name="UserDataArea"></param>
	/// <returns></returns>
	public static byte CheckSum(IEnumerable<byte> UserDataArea)
	{
		byte result = 0x00;
		foreach (var data in UserDataArea)
		{
			result += data;
		}
		return result;
	}
}
