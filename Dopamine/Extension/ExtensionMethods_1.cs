using static System.Net.Mime.MediaTypeNames;

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
		public static string ShowInTrace(this object? input, bool showDateTime = false)
		{
			string result = $"{(showDateTime ? DateTime.Now.ToString("[HH:mm:ss]\t") : string.Empty)}{input}";
			Trace.WriteLine(result);
			return result;
		}
		/// <summary>
		/// 在终端中打印文本信息
		/// </summary>
		/// <param name="input"></param>
		/// <param name="showDateTime"></param>
		public static string ShowInConsole(this object? input, bool showDateTime = false)
		{
			string result = $"{(showDateTime ? DateTime.Now.ToString("[HH:mm:ss]\t") : string.Empty)}{input}";
			Console.WriteLine(result);
			return result;
		}
		/// <summary>
		/// 数组转字符串
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string ToString<T>(this T[] input)
		{
			return string.Join(",", input);
		}
		/// <summary>
		/// 复制文本到剪切板
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string CopyToClipboard(this string text)
		{
			try
			{
				TextCopy.ClipboardService.SetText(text);
			}
			catch (Exception ex)
			{
				// 处理平台兼容异常
				throw new PlatformNotSupportedException(
					"当前操作系统不支持剪切板操作", ex);
			}
			return text;
		}
		/// <summary>
		/// 获取最大值
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input"></param>
		/// <returns></returns>
		public static T Max<T>(this IEnumerable<T> input) where T : IComparable
		{
			if (!input.Any()) return default;
			T result = input.GetEnumerator().Current;
			foreach (T v in input)
			{
				result = v.CompareTo(result) > 0 ? v : result;
			}
			return result;
		}
	}
}
