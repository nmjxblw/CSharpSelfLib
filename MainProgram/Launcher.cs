namespace MainProgram;
/// <summary>
/// 启动器
/// </summary>
sealed class Launcher
{
	/// <summary>
	/// 主程序类
	/// </summary>
	private static App App { get; } = new App();
	/// <summary>
	/// 启动
	/// </summary>
	/// <param name="args"></param>
	static void Main(string[] args)
	{
		App.Start();
	}
}
