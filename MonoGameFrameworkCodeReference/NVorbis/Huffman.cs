using System;
using System.Collections.Generic;

namespace NVorbis;

internal static class Huffman
{
	private const int MAX_TABLE_BITS = 10;

	internal static List<HuffmanListNode> BuildPrefixedLinkedList(int[] values, int[] lengthList, int[] codeList, out int tableBits, out HuffmanListNode firstOverflowNode)
	{
		HuffmanListNode[] list = new HuffmanListNode[lengthList.Length];
		int maxLen = 0;
		for (int i = 0; i < list.Length; i++)
		{
			list[i] = new HuffmanListNode
			{
				Value = values[i],
				Length = ((lengthList[i] <= 0) ? 99999 : lengthList[i]),
				Bits = codeList[i],
				Mask = (1 << lengthList[i]) - 1
			};
			if (lengthList[i] > 0 && maxLen < lengthList[i])
			{
				maxLen = lengthList[i];
			}
		}
		Array.Sort(list, SortCallback);
		tableBits = ((maxLen > 10) ? 10 : maxLen);
		List<HuffmanListNode> prefixList = new List<HuffmanListNode>(1 << tableBits);
		firstOverflowNode = null;
		for (int j = 0; j < list.Length && list[j].Length < 99999; j++)
		{
			if (firstOverflowNode == null)
			{
				int itemBits = list[j].Length;
				if (itemBits > tableBits)
				{
					firstOverflowNode = list[j];
					continue;
				}
				int maxVal = 1 << tableBits - itemBits;
				HuffmanListNode item = list[j];
				for (int k = 0; k < maxVal; k++)
				{
					int idx = (k << itemBits) | item.Bits;
					while (prefixList.Count <= idx)
					{
						prefixList.Add(null);
					}
					prefixList[idx] = item;
				}
			}
			else
			{
				list[j - 1].Next = list[j];
			}
		}
		while (prefixList.Count < 1 << tableBits)
		{
			prefixList.Add(null);
		}
		return prefixList;
	}

	private static int SortCallback(HuffmanListNode i1, HuffmanListNode i2)
	{
		int len = i1.Length - i2.Length;
		if (len == 0)
		{
			return i1.Bits - i2.Bits;
		}
		return len;
	}
}
