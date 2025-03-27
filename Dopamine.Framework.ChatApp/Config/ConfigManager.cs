using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Encodings;
using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
namespace Dopamine.ChatApp
{
	public static class ConfigManager
	{
		private static Assembly Assembly => Assembly.GetExecutingAssembly();
		private static readonly string ConfigName = "Dopamine.ChatApp.Config.RuntimeConfig.json";
		// Newtonsoft 序列化配置
		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Include,
			DateParseHandling = DateParseHandling.None, // 禁用自动日期解析
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore
		};

		/// <summary>
		/// 动态解析后的 JSON 数据
		/// </summary>
		public static dynamic Data { get; private set; } = new DynamicClass();

		static ConfigManager()
		{
			try
			{
				using (Stream stream = Assembly.GetManifestResourceStream(ConfigName))
				using (StreamReader reader = new StreamReader(stream, new UTF8Encoding(false)))
				{
					string jsonString = reader.ReadToEnd();
					Data = JsonConvert.DeserializeObject<DynamicClass>(jsonString);
				}

			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Config initialization failed: {ex.Message}");
			}
		}

		/// <summary>
		/// 保存配置到文件
		/// </summary>
		public static void Save()
		{
			string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "JsonFiles", "RuntimeConfig.json");
			string directory = Path.GetDirectoryName(filePath);

			try
			{
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				// 使用原子写入模式
				string tempFile = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
				File.WriteAllText(tempFile, JsonConvert.SerializeObject(Data, Formatting.Indented), Encoding.UTF8);
				File.Move(tempFile, filePath);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Config save failed: {ex.Message}");
			}
		}
	}
}
