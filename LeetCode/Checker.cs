using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeetCode
{
	/// <summary>
	/// 检测器
	/// </summary>
	internal class Checker
	{
		/// <summary>
		/// 起始时间
		/// </summary>
		private DateTime StartTime { get; set; } = DateTime.Now;
		/// <summary>
		/// 运行时间
		/// </summary>
		private TimeSpan TimeSpend => EndTime - StartTime;
		/// <summary>
		/// 结束时间
		/// </summary>
		private DateTime EndTime { get; set; } = DateTime.Now;
		/// <summary>
		/// 构造检测器
		/// </summary>
		public Checker(DateTime startTime)
		{
			this.StartTime = startTime;
		}
		/// <summary>
		/// 记录时间
		/// </summary>
		public string TimeRecord()
		{
			EndTime = DateTime.Now;
			return TimeSpend.ToString(@"ss\.ffff") + "s";
		}
		/// <summary>
		/// 成员缓存
		/// </summary>
		private ConcurrentDictionary<Type, MemberInfo[]> MemberCache { get; } = new ConcurrentDictionary<Type, MemberInfo[]>();
		/// <summary>
		/// 深度比较
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool DeepCompare<T>(T x, T y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			return CompareMembers(x, y, typeof(T));
		}
		/// <summary>
		/// 比较成员
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private bool CompareMembers(object x, object y, Type type)
		{
			MemberInfo[] members = MemberCache.GetOrAdd(type, t =>
				t.GetProperties().Cast<MemberInfo>()
				.Concat(t.GetFields()).ToArray());

			foreach (MemberInfo member in members)
			{
				object xVal, yVal;
				if (member is PropertyInfo prop)
				{
					xVal = prop.GetValue(x);
					yVal = prop.GetValue(y);
				}
				else if (member is FieldInfo field)
				{
					xVal = field.GetValue(x);
					yVal = field.GetValue(y);
				}
				else continue;

				if (!CompareValues(xVal, yVal)) return false;
			}
			return true;
		}
		/// <summary>
		/// 比较值
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private bool CompareValues(object x, object y)
		{
			if (x == null || y == null) return x == y;
			Type type = x.GetType();
			if (type != y.GetType()) return false;

			if (type.IsPrimitive || type == typeof(string))
				return x.Equals(y);

			if (type.IsArray)
			{
				Array arrX = (Array)x;
				Array arrY = (Array)y;
				if (arrX.Length != arrY.Length) return false;
				return arrX.Cast<object>()
					.Zip(arrY.Cast<object>(), (a, b) => CompareValues(a, b))
					.All(result => result);
			}

			return CompareMembers(x, y, type);
		}
	}
}
