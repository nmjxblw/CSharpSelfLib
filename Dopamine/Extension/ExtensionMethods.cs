namespace Dopamine;
/// <summary>
/// 拓展域名下的额外静态方法
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	/// 在【输出】中将字符串打印出来
	/// </summary>
	/// <param name="input">输入的字符串</param>
	/// <param name="showDateTime">添加时间戳</param>
	public static void ShowInTrace(this string? input, bool showDateTime = false)
	{
		Trace.WriteLine($"{(showDateTime ? DateTime.Now.ToString("[HH:mm:ss]\t") : string.Empty)}{input}");
	}
	/// <summary>
	/// 在终端中打印文本信息
	/// </summary>
	/// <param name="input"></param>
	/// <param name="showDateTime"></param>
	public static void ShowInConsole(this string? input, bool showDateTime = false)
	{
		Console.WriteLine($"{(showDateTime ? DateTime.Now.ToString("[HH:mm:ss]\t") : string.Empty)}{input}");
	}
	/// <summary>
	/// 数组转字符串
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string ToString<T>(this T[] input)
	{
		return string.Join(",", input.ToString());
	}
	/// <summary>
	/// 从流中获取数据帧
	/// </summary>
	/// <param name="stream">输入流</param>
	/// <param name="capacity">容量</param>
	/// <returns>流数据帧</returns>
	public static byte[] GetFrame(this NetworkStream stream, int capacity = 0x100)
	{
		// 流数据容器
		byte[] InitFrame = new byte[capacity];
		// 获取数据帧长度
		int len = stream.Read(InitFrame, 0, InitFrame.Length);
		byte[] TrimFrame = new byte[len];
		Array.Copy(InitFrame, 0, TrimFrame, 0, len);
		return TrimFrame;
	}

	/// <summary>
	/// 和校验
	/// </summary>
	/// <param name="UserDataArea"></param>
	/// <returns></returns>
	public static byte GetSum(this IEnumerable<byte> UserDataArea)
	{
		byte result = 0x00;
		foreach (var data in UserDataArea)
		{
			result += data;
		}
		return result;
	}
	/// <summary>
	/// 计算异或和
	/// </summary>
	/// <param name="UserDataArea"></param>
	/// <returns></returns>
	public static byte GetXor(this IEnumerable<byte> UserDataArea)
	{
		byte result = 0x00;
		foreach (var data in UserDataArea)
		{
			result ^= data;
		}
		return result;
	}
}
