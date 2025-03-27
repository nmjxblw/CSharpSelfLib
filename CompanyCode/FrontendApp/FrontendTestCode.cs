namespace CompanyCode;
/// <summary>
/// 前置机测试软件
/// </summary>
public class FrontendTestCode
{
	/// <summary>
	/// 解析传入的指令字符串
	/// </summary>
	/// <param name="cmd"></param>
	/// <param name="code"></param>
	/// <param name="target"></param>
	/// <param name="data"></param>
	/// <returns></returns>
	public static bool DeserializationCmd(string cmd, out string code, out string target, out string data)
	{
		code = string.Empty;
		target = string.Empty;
		data = string.Empty;
		var regex = new Regex(@$"{ConfigManager.Data.Format.Receive}",
			RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);

		Match match = regex.Match(cmd);

		if (!match.Success)
		{
			Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss,ffff} PC->ME :{cmd}\n该指令是无效指令！正则表达式匹配失败，请检测输入的命令是否符合配置文件中的正则表达式。");
			return false;
		}

		code = match.Groups["cmd"].Value;
		data = match.Groups["data"].Value;

		if (!int.TryParse(match.Groups["sn"].Value, out var temp))
		{
			Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss,ffff} PC->ME :{cmd}\n该指令是无效指令！尝试转换Sn为整型失败,请确认Sn是否为整型字符串。");
			return false;
		}
		target = temp.ToString();
		return true;
	}
	static List<UInt64> SocketState { get; } = new List<UInt64>() { (UInt64)0, (UInt64)0 };
	/// <summary>
	/// 5000H—设置载波模块工作电压
	/// </summary>
	/// <returns></returns>
	public static int ControlRelay(List<List<int>> socketList, bool on = true)
	{
		int channelIndex = 0;
		foreach (List<int> sockets in socketList)
		{
			foreach (int socket in sockets)
			{
				// 无效表位跳过
				if (socket <= 0 || socket > 64)
				{
					continue;
				}
				// 通过位运算获得
				if(on)
					SocketState[channelIndex] |=  (UInt64)1 << (socket - 1);
				else
					SocketState[channelIndex] &= ~(UInt64)1 << (socket - 1);
			}
			channelIndex++;
		}

		return 0;
	}
}
