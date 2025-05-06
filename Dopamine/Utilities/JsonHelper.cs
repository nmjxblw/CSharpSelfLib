using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
namespace Dopamine
{
	/// <summary>
	/// Json帮助类
	/// </summary>
	/// <remarks>注意：仅允许序列化属性而不是字段</remarks>
	/// <example>
	/// 这是一个示例类，展示如何使用 Json帮助类：
	/// <code>
	/// public class TestClass
	/// {
	///     public TestClass(string name = "Bob", int age = 15) 
	///     { 
	///         this.Name = name; 
	///         this.Age = age; 
	///     }
	///     
	///     public string Name { get; set; } = "Bob";
	///     public int Age { get; set; } = 15;
	/// }
	/// </code>
	/// </example>
	public static class JsonHelper
	{
		/// <summary>
		/// Json文件主路径
		/// </summary>
		public readonly static string MainJsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "JsonFiles");
		/// <summary>
		/// 序列化设定
		/// </summary>
		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true, // 属性名不区分大小写
			AllowTrailingCommas = true, // 允许尾随逗号
			ReadCommentHandling = JsonCommentHandling.Skip, // 跳过注释
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
			WriteIndented = true, // 增加阅读性
			DefaultIgnoreCondition = JsonIgnoreCondition.Never, // 确保不忽略任何字段
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), }, // 将枚举型转化成字符串
			Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // 创建一个支持特定 Unicode 范围的编码器。
		};
		/// <summary>
		/// 线程锁
		/// </summary>
		private readonly static object LockObj = new object();
		/// <summary>
		/// 静态实例化
		/// </summary>
		static JsonHelper()
		{
			if (!Directory.Exists(MainJsonFilePath))
			{
				Directory.CreateDirectory(MainJsonFilePath);
			}
		}
		/// <summary>
		/// Json文件过滤器
		/// </summary>
		public static string JsonFilter { get; } = "Json Files (*.json)|*.json|All Files (*.*)|*.*";
		/// <summary>
		/// 加载Json文件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName">默认文件名包含绝对路径</param>
		/// <returns></returns>
		public static T? Load<T>(string? fileName = default)
		{
			if (fileName == default)
			{
				if (string.IsNullOrEmpty(fileName))
					return default;
			}
			else
			{
				if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
				{
					fileName += ".json";
				}
				if (!File.Exists(fileName))
				{
					// 有可能是只传入了文件名字，添加一下绝对路径
					if (!fileName.Contains(MainJsonFilePath))
					{
						fileName = Path.Combine(MainJsonFilePath, fileName);
					}
					// 如果文件还是不存在
					if (!File.Exists(fileName))
					{
						return default;
					}
				}
			}
			string jsonString = File.ReadAllText(fileName);
			if (string.IsNullOrEmpty(jsonString))
				return default;
			return JsonSerializer.Deserialize<T>(jsonString, JsonOptions);
		}
		/// <summary>
		/// 存写Json文件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="fileNames"></param>
		/// <returns></returns>
		public static bool Save<T>(T data, params string[] fileNames)
		{
			try
			{
				if (data == null) return false;
				string fileName;
				lock (LockObj) // 确保线程安全
				{
					if (fileNames.Length <= 0)
					{
						fileName = data.GetType().Name;
					}
					else
					{
						fileName = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(fileNames));
					}
					if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
					{
						fileName += ".json";
					}
					// 确定保存路径
					string savePath = fileNames.Length <= 0 ?  MainJsonFilePath : Path.GetDirectoryName(fileName) ?? MainJsonFilePath;

					// 如果自定义路径不存在，则创建它
					if (!Directory.Exists(savePath))
					{
						Directory.CreateDirectory(savePath);
					}

					string jsonString = JsonSerializer.Serialize(data, JsonOptions);
					string filePath = Path.Combine(savePath, fileName);

					// 使用 StreamWriter 写入文件
					using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
					{
						writer.Write(jsonString);
					}
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
