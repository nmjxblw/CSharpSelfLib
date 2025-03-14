﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine
{
	// 拓展方法第二部分，数据帧相关
	public static partial class ExtensionMethods
	{
		/// <summary>
		/// 从流中获取数据帧
		/// </summary>
		/// <param name="stream">输入流</param>
		/// <param name="capacity">容量</param>
		/// <returns>流数据帧</returns>
		public static byte[] GetFrame(this NetworkStream stream, int capacity = 0x100)
		{
			// 流数据容器
			byte[] InitFrame = new byte[capacity];
			// 获取数据帧长度
			int len = stream.Read(InitFrame, 0, InitFrame.Length);
			byte[] TrimFrame = new byte[len];
			Array.Copy(InitFrame, 0, TrimFrame, 0, len);
			return TrimFrame;
		}

		/// <summary>
		/// 和校验
		/// </summary>
		/// <param name="UserDataArea"></param>
		/// <returns></returns>
		public static byte GetSum(this IEnumerable<byte> UserDataArea)
		{
			byte result = 0x00;
			foreach (var data in UserDataArea)
			{
				result += data;
			}
			return result;
		}
		/// <summary>
		/// 计算异或和
		/// </summary>
		/// <param name="UserDataArea"></param>
		/// <returns></returns>
		public static byte GetXor(this IEnumerable<byte> UserDataArea)
		{
			byte result = 0x00;
			foreach (var data in UserDataArea)
			{
				result ^= data;
			}
			return result;
		}
		/// <summary>
		/// 将BCC校验码填入数据帧最后一位（如果最后一位不为0x00,则自动补位）
		/// </summary>
		public static byte[] FillBBCAtLast(this IEnumerable<byte> Input, bool FillInTail = false)
		{
			List<byte> result = Input.ToList();
			if (result.Count <= 0) return result.ToArray();
			if (!FillInTail) result.Add(0x00);
			byte BBC = 0x00;
			// 除去帧头，其余字节进行异或运算
			for (int i = 1; i < result.Count - 1; i++)
			{
				BBC ^= result[i];
			}
			result[result.Count - 1] = BBC;
			return result.ToArray();
		}
		/// <summary>
		/// 判断目标 byte[] 是否在集合中 
		/// </summary>
		/// <param name="Input"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool HasBytes(this IEnumerable<byte[]> Input, byte[] item)
		{
			return Input.Any(bytes => bytes.SequenceEqual(item));
		}
		/// <summary>
		/// 在帧末尾添加异或校验
		/// </summary>
		/// <returns></returns>
		public static byte[] AppendXor(this byte[] inputFrame, bool IncludingHead = false)
		{
			List<byte> ck = new List<byte>(inputFrame);
			ck.Add(0x00);
			for (int i = IncludingHead ? 0 : 1; i < inputFrame.Length; i++)
			{
				ck[inputFrame.Length] ^= inputFrame[i];
			}
			return ck.ToArray();
		}
	}
}
