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
		byte[] newTest = new byte[] { 0x88, 0x9f }.AppendXOR(true); 
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
