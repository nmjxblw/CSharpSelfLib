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
	/// 1兆字节
	/// </summary>
	private const double MegaBytes = (double)(1 << 20);
	/// <summary>
	/// 启动
	/// </summary>
	/// <param name="args"></param>
	[Dopamine]
	static void Main(string[] args)
	{
		sw.Restart();
		App.Start();
		sw.Stop();
		System.Diagnostics.Process process = Process.GetCurrentProcess();
		double workingSet = process.WorkingSet64 / MegaBytes;
		double virtualMem = process.VirtualMemorySize64 / MegaBytes;
		double managedMem = GC.GetTotalMemory(forceFullCollection: false) / MegaBytes;
		// 查看内存压力
		double memory = GC.GetTotalMemory(true) / MegaBytes;
		// 获取GC统计信息
		System.GCMemoryInfo GCInfo = GC.GetGCMemoryInfo();
		double heapSizeBytes = GCInfo.HeapSizeBytes / MegaBytes;
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine($"\n【内存统计】\n物理内存占用：{workingSet:F2} MB\n虚拟内存占用：{virtualMem:F2}MB\n托管堆内存：{managedMem:F2} MB\n堆分配：{memory:F2} MB\n堆最终分配：{heapSizeBytes:F2} MB\n【计时统计】\n实际耗时{((double)sw.ElapsedTicks / Stopwatch.Frequency * 1000).ToString("F5")}ms");
	}
}
