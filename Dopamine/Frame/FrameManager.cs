using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine
{
	/// <summary>
	/// 报文处理器
	/// </summary>
	public class FrameManager
	{
		/// <summary>
		/// 延迟实例化
		/// </summary>
		private static Lazy<FrameManager> _lazy = new Lazy<FrameManager>(() => new FrameManager(), LazyThreadSafetyMode.ExecutionAndPublication);
		private FrameManager() { }
		/// <summary>
		/// 单例实例化
		/// </summary>
		public static FrameManager Instance => _lazy.Value;
		/// <summary>
		/// 组装数据帧
		/// </summary>
		/// <returns></returns>
		public static byte[] AssembleFrame<T>(params T[] input)
		{
			List<byte> result = new List<byte>();
			foreach(T temp in input)
			{
				result.Add(Convert.ToByte(temp));
			}
			return result.ToArray();
		}
	}
}
