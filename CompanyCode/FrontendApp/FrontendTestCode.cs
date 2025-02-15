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
}
