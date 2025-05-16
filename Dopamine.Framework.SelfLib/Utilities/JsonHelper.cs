using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
namespace Dopamine.Framework
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
		public readonly static string MainJsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "JsonFiles");

		// Newtonsoft 序列化配置
		private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver
			{
				// 配置属性名称策略
				NamingStrategy = new CamelCaseNamingStrategy
				{
					ProcessDictionaryKeys = true,
					OverrideSpecifiedNames = false
				}
			},
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Include,
			Converters = { new StringEnumConverter() }, // 枚举转字符串
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
			DateTimeZoneHandling = DateTimeZoneHandling.Utc,
			// 注释处理配置（需配合读取器）
			DateParseHandling = DateParseHandling.None // 防止日期解析冲突
		};

		// 线程安全锁
		private static readonly object LockObj = new object();

		static JsonHelper()
		{
			Directory.CreateDirectory(MainJsonFilePath); // 自动创建目录
		}

		public static string JsonFilter { get; } = "Json Files (*.json)|*.json|All Files (*.*)|*.*";

		/// <summary>
		/// 加载 JSON 文件（支持注释和尾随逗号）
		/// </summary>
		public static T Load<T>(string fileName = null)
		{
			if (string.IsNullOrEmpty(fileName)) return default;

			// 自动补全路径和扩展名
			if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
				fileName += ".json";

			if (!Path.IsPathRooted(fileName))
				fileName = Path.Combine(MainJsonFilePath, fileName);

			if (!File.Exists(fileName)) return default;

			// 创建支持注释的读取器
			using (StreamReader streamReader = new StreamReader(fileName))
			using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
			{
				// 配置读取行为
				jsonReader.CloseInput = true;
				jsonReader.SupportMultipleContent = true;

				// 应用注释处理设置
				JsonLoadSettings loadSettings = new JsonLoadSettings
				{
					CommentHandling = CommentHandling.Ignore,
					LineInfoHandling = LineInfoHandling.Ignore
				};

				JToken jToken = JToken.Load(jsonReader, loadSettings);
				return jToken.ToObject<T>(JsonSerializer.Create(JsonSettings));
			}
		}

		/// <summary>
		/// 保存 JSON 文件
		/// </summary>
		public static bool Save<T>(T data, params string[] fileNames)
		{
			if (data == null) return false;

			lock (LockObj)
			{
				try
				{
					// 构建文件名
					string fileName;
					if (fileNames.Length == 0)
					{
						fileName = $"{data.GetType().Name}.json";
					}
					else
					{
						fileName = Path.Combine(fileNames);
						if (!fileName.EndsWith(".json"))
							fileName += ".json";
					}

					// 确定保存路径
					var savePath = Path.IsPathRooted(fileName)
						? Path.GetDirectoryName(fileName)
						: MainJsonFilePath;

					Directory.CreateDirectory(savePath);

					// 序列化数据
					var jsonString = JsonConvert.SerializeObject(data, JsonSettings);
					var fullPath = Path.Combine(savePath, fileName);

					File.WriteAllText(fullPath, jsonString, Encoding.UTF8);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}
	}
}
