using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine
{
	/// <summary>
	/// Access管理器
	/// </summary>
	public class AccessManager
	{
		/// <summary>
		/// 延迟实例化
		/// </summary>
		private static Lazy<AccessManager> _lazy = new Lazy<AccessManager>(() => new AccessManager(), LazyThreadSafetyMode.ExecutionAndPublication);
		private AccessManager() { }
		/// <summary>
		/// 单例实例化
		/// </summary>
		public static AccessManager Instance => _lazy.Value;
	}
}
