namespace MainProgram;
/// <summary>
/// 主程序
/// </summary>
public sealed class App
{
	/// <summary>
	/// 自定义客户端
	/// </summary>
	private DopamineHttpClient Client { get; set; } = new DopamineHttpClient();
	public void Start()
	{
		string getString = Client.GetAsync($"{ConfigManager.Data.Url.Beat}").GetAwaiter().GetResult() ?? string.Empty;
		getString.ShowInConsole(true);
	}
}
