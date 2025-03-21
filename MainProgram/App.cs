using Newtonsoft.Json;
using System.Collections.Concurrent;
namespace MainProgram;
/// <summary>
/// 主程序
/// </summary>
public sealed class App
{
	public void Start()
	{
		System.Collections.Concurrent.ConcurrentDictionary<int, int> keyValuePairs = new System.Collections.Concurrent.ConcurrentDictionary<int, int>();
		
		var random = new Random(Guid.NewGuid().GetHashCode());
		int runtimes = 0;
		while (true)
		{
			Thread.Sleep(100);
			int start = 9;
			int count = 0;
			while (start > 0 && start < 18)
			{
				count++;
				start = GetNewResult(start, random);
			}
			runtimes++;
			keyValuePairs.AddOrUpdate(
				count,
				_ => 1,
				(_, old) => Interlocked.Increment(ref old)
			);
			Console.Clear();
			foreach (var kvp in keyValuePairs.OrderBy(k => k.Value))
			{
				string.Format("count:{0,-40}time{1}", kvp.Key, kvp.Value).ShowInConsole();
			}
			if (Console.KeyAvailable)
			{
				Console.ReadKey(true);
				break;
			}
		}
		float exp = 0f;
		foreach (var kvp in keyValuePairs)
		{
			exp += (float)kvp.Key * kvp.Value / runtimes;
		}
		string.Format("Runtimes:{0,-10} EXP:{1,-10} Minute:{2}", runtimes, exp, (exp / 60f)).ShowInConsole();
	}
	public int GetNewResult(int start, Random random)
	{
		if (start == 9)
		{
			return start + (random.Next(0, 100) < 50 ? -1 : 1);
		}
		else if (start < 9)
		{
			return start + (random.Next(0, 100) < 40 ? -1 : 1);
		}
		else
		{
			return start + (random.Next(0, 100) < 60 ? -1 : 1);
		}
	}
}
