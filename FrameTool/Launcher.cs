namespace FrameTool
{
	/// <summary>
	/// 启动器
	/// </summary>
	internal class Launcher
	{
		private static MainApplication App { get; } = new MainApplication();
        [STAThread]
        static void Main(string[] args)
		{
			Console.WriteLine("[帧工具助手]");
			App.Run();
		}
	}
}
