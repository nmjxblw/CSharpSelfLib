using System.Diagnostics;
using System.Net.NetworkInformation;

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
	/// 内置计时器
	/// </summary>
	private static Stopwatch sw { get; } = new Stopwatch();
	/// <summary>
	/// 启动
	/// </summary>
	/// <param name="args"></param>
	static void Main(string[] args)
	{
		sw.Restart();
		App.Start();
		sw.Stop();
		var process = Process.GetCurrentProcess();
		long managedMem = GC.GetTotalMemory(forceFullCollection: false);
		// 查看内存压力
		long memory = GC.GetTotalMemory(true);
		// 获取GC统计信息
		System.GCMemoryInfo GCInfo = GC.GetGCMemoryInfo();
		Console.WriteLine($"\n【总结】\n物理内存占用：{process.WorkingSet64 / 1024} KB\n托管堆内存：{managedMem/1024} KB\n堆分配：{memory/1024} KB\n耗时：{sw.ElapsedMilliseconds}ms\n刻度计数：{sw.ElapsedTicks}\n计数频率:{Stopwatch.Frequency}Hz");
	}
}
