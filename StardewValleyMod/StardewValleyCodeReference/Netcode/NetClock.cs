using System.Collections.Generic;

namespace Netcode;

public class NetClock
{
	public NetVersion netVersion;

	public int LocalId;

	public int InterpolationTicks;

	public List<bool> blanks = new List<bool>();

	public NetClock()
	{
		this.netVersion = default(NetVersion);
		this.LocalId = this.AddNewPeer();
	}

	public int AddNewPeer()
	{
		int id = this.blanks.IndexOf(item: true);
		if (id != -1)
		{
			this.blanks[id] = false;
		}
		else
		{
			id = this.netVersion.Size();
			while (this.blanks.Count < this.netVersion.Size())
			{
				this.blanks.Add(item: false);
			}
			this.netVersion[id] = 0u;
		}
		return id;
	}

	public void RemovePeer(int id)
	{
		while (this.blanks.Count <= id)
		{
			this.blanks.Add(item: false);
		}
		this.blanks[id] = true;
	}

	public uint GetLocalTick()
	{
		return this.netVersion[this.LocalId];
	}

	public void Tick()
	{
		ref NetVersion reference = ref this.netVersion;
		int localId = this.LocalId;
		uint value = reference[localId] + 1;
		reference[localId] = value;
	}

	public void Clear()
	{
		this.netVersion.Clear();
		this.LocalId = 0;
	}

	public override string ToString()
	{
		return base.ToString() + ";LocalId=" + this.LocalId;
	}
}
