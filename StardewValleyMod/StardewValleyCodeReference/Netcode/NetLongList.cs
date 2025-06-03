using System.Collections.Generic;

namespace Netcode;

public sealed class NetLongList : NetList<long, NetLong>
{
	public NetLongList()
	{
	}

	public NetLongList(IEnumerable<long> values)
		: base(values)
	{
	}

	public NetLongList(int capacity)
		: base(capacity)
	{
	}

	public override bool Contains(long item)
	{
		foreach (long item2 in this)
		{
			if (item2 == item)
			{
				return true;
			}
		}
		return false;
	}

	public override int IndexOf(long item)
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
