namespace CompanyCode;
/// <summary>
/// 主程序
/// </summary>
public sealed class CompanyApp
{
	public static string TestString { get; } = "cmd={0},sn={1},data={2}";
	public string Code { get; } = "0301";
	public int Target { get; } = 001;
	public string Para { get; } = "测试代码";
	public void Start()
	{
		string input = string.Format(TestString, Code, Target, Para);
		if (FrontendTestCode.DeserializationCmd(input, out var code, out var target, out var data))
		{
			$"code={code},target={target},data={data}".ShowInConsole(true);
		}
		string stringAfterFormat = string.Format(ConfigManager.Data.Format.Send, Code, Target, Para + ";data");
		stringAfterFormat.ShowInConsole(true);
		if (FrontendTestCode.DeserializationCmd(stringAfterFormat, out code, out target, out data))
		{
			$"code={code},target={target},data={data}".ShowInConsole(true);
		}
	}
}
