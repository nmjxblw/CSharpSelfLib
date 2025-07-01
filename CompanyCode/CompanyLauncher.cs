namespace CompanyCode;
/// <summary>
/// 启动器
/// </summary>
sealed class CompanyLauncher
{
    /// <summary>
    /// 主程序类
    /// </summary>
    private static CompanyApp App { get; } = new CompanyApp();
    /// <summary>
    /// 启动参数
    /// </summary>
    public static string[] Args { get; private set; } = [];
    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        "正在启动 CompanyApp ".ShowInConsole();
        Args = args;
        App.Start();
    }
}
