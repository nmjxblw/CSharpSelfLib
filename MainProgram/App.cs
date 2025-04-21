using Newtonsoft.Json;
using System.Collections.Concurrent;
using DeepSeekApi;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Linq;
namespace MainProgram;
/// <summary>
/// 主程序
/// </summary>
public sealed class App
{
	public void Start()
	{
		string bs = "440100600004";
		bs = bs.PadLeft(12, '0');
		byte[] bytes = ByteStringToBytes(bs);
		BitConverter.ToString(bytes).ShowInConsole(true);
	}
	public byte[] ByteStringToBytes(string byteString)
	{
		int byteCount = byteString.Length % 2 + byteString.Length / 2;
		byte[] bytes = new byte[byteCount];
		for (int i = 0; i < byteCount; i++)
		{
			bytes[i] = Convert.ToByte(byteString.Substring(i * 2, 2), 16);
		}
		return bytes;
	}
}