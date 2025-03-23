using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Encodings;
using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
namespace DeepSeekApi
{
	internal static class ConfigManager
	{
		private static Assembly Assembly => Assembly.GetExecutingAssembly();
		private static readonly string ConfigName = "DeepSeekApi.Config.RuntimeConfig.json";
		private static readonly JsonSerializerOptions opts = new JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
		};
		/// <summary>
		/// 动态解析后的 json 文件
		/// </summary>
		public static dynamic Data { get; set; } = new DynamicClass();
		static ConfigManager()
		{
			try
			{
				using (Stream? stream = Assembly.GetManifestResourceStream(ConfigName))
				using (StreamReader reader = new StreamReader(stream!))
				{
					string json = reader.ReadToEnd();
					if (string.IsNullOrEmpty(json)) throw new Exception("json is null");
					Data = JsonSerializer.Deserialize<DynamicClass>(json, opts)!;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
			}
		}
		/// <summary>
		/// 存储
		/// </summary>
		public static void Save()
		{
			string filepath = Path.Combine(Directory.GetCurrentDirectory(), "Json", "test.json");
			string? path = Path.GetDirectoryName(filepath);
			if (string.IsNullOrEmpty(path)) return;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			FileStream stream = File.Create(filepath);
			JsonSerializer.Serialize(stream, Data, opts);
			stream.Dispose();
		}
	}
}
