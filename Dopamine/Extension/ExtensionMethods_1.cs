namespace Dopamine
{
	// 拓展方法第一部分，常用调试代码
	/// <summary>
	/// 拓展域名下的额外静态方法
	/// </summary>
	public static partial class ExtensionMethods
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
	}
}
