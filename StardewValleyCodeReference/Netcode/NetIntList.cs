using System.Collections.Generic;

namespace Netcode;

public sealed class NetIntList : NetList<int, NetInt>
{
	public NetIntList()
	{
	}

	public NetIntList(IEnumerable<int> values)
		: base(values)
	{
	}

	public NetIntList(int capacity)
		: base(capacity)
	{
	}

	public override bool Contains(int item)
	{
		foreach (int item2 in this)
		{
			if (item2 == item)
			{
				return true;
			}
		}
		return false;
	}

	public override int IndexOf(int item)
	{
		NetInt count = base.count;
		for (int i = 0; i < count.Value; i++)
		{
			if (base.array.Value[i] == item)
			{
				return i;
			}
		}
		return -1;
	}
}
