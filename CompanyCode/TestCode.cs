using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyCode
{
	internal class TestCode
	{
		private static TestCode? _instance = null;
		public static TestCode Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new TestCode();
				}
				return _instance;
			}
		}
		public byte GetSequence(int frameIndex = 0, bool needCheck = false, bool isTailFrame = false, bool isHeadFrame = true, bool hasTimeTag = false)
		{
			byte Sequence = 0;
			Sequence ^= ReverseBits((byte)(frameIndex % 0x10));
			Sequence ^= needCheck ? (byte)(1 << 4) : (byte)(0);
			Sequence ^= isTailFrame ? (byte)(1 << 5) : (byte)(0);
			Sequence ^= isHeadFrame ? (byte)(1 << 6) : (byte)(0);
			Sequence ^= hasTimeTag ? (byte)(1 << 7) : (byte)(0);
			return Sequence;
		}
		public static byte ReverseBits(byte b, int bitsCount = 4)
		{
			byte result = 0;
			for (int i = 0; i < bitsCount; i++)
			{
				result = (byte)((result << 1) | (b & 1)); // 左移结果，取最低位
				b >>= 1; // 原字节右移
			}
			return result;
		}
	}
}
