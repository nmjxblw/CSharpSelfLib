namespace CompanyCode;
/// <summary>
/// 主程序
/// </summary>
public sealed class CompanyApp
{
	//public static string TestString { get; } = "cmd={0},sn={1},data={2}";
	//public string Code { get; } = "0301";
	//public int Target { get; } = 001;
	//public string Para { get; } = "测试代码";
	public void Start()
	{
		byte[] Frame;
		Frame = new byte[] { 0x7B, 0x15, 0x00, 0x06, 0x07, 0xD0, 0x13, 0x88, 0x00, 0x00, 0x02, 0x58, 0x32, 0x00, 0x64, 0x00, 0x64, 0x00, 0x00, 0x00, 0x7D };
		Frame = new byte[] { 0x4E, 0x80, 0x12, 0x02, 0x01, 0xF4, 0x39, 0x45 };
		Frame.GetSum().ToString("X").ShowInConsole(true);
	}
}
