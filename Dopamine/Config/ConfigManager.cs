namespace Dopamine;
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
}
