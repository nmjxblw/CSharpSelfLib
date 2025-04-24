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
		System.Diagnostics.Process process = Process.GetCurrentProcess();
		long workingSet = process.WorkingSet64 / 1024;
		long managedMem = GC.GetTotalMemory(forceFullCollection: false) / 1024;
		// 查看内存压力
		long memory = GC.GetTotalMemory(true) / 1024;
		// 获取GC统计信息
		System.GCMemoryInfo GCInfo = GC.GetGCMemoryInfo();
		long heapSizeBytes = GCInfo.HeapSizeBytes / 1024;
		Console.WriteLine($"\n【内存统计】\n物理内存占用：{workingSet} KB\n托管堆内存：{managedMem} KB\n堆分配：{memory} KB\n堆最终分配：{heapSizeBytes} KB\n【计时统计】\n实际耗时{((double)sw.ElapsedTicks / Stopwatch.Frequency * 1000).ToString("F5")}ms");
	}
}
