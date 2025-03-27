#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Dopamine
{
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配
	/// <summary>
	/// 拓展域名下的Json配置管理器
	/// </summary>
	public static class ConfigManager
	{
		private static Assembly Assembly => Assembly.GetExecutingAssembly();
		private static readonly string ConfigName = "Dopamine.Config.RuntimeConfig.json";
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
		/// 配置管理器使用的Json序列化选项
		/// </summary>
		public static JsonSerializerOptions Options => opts;
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
			string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Json", "test.json");
			string? path = Path.GetDirectoryName(filePath);
			if (string.IsNullOrEmpty(path)) return;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			FileStream stream = File.Create(filePath);
			JsonSerializer.Serialize(stream, Data, opts);
			stream.Dispose();
		}
	}
}
