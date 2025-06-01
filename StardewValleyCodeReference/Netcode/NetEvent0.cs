using System;
using System.IO;

namespace Netcode;

public class NetEvent0 : AbstractNetSerializable
{
	public delegate void Event();

	public readonly NetInt Counter = new NetInt();

	private int currentCount;

	public event Event onEvent;

	public NetEvent0(bool interpolate = false)
	{
		this.Counter.InterpolationEnabled = interpolate;
	}

	public void Fire()
	{
		NetInt counter = this.Counter;
		int value = counter.Value + 1;
		counter.Value = value;
		this.Poll();
	}

	public void Poll()
	{
		if (this.Counter.Value != this.currentCount)
		{
			this.currentCount = this.Counter.Value;
			if (this.onEvent != null)
			{
				this.onEvent();
			}
		}
	}

	public void Clear()
	{
		this.Counter.Set(0);
		this.currentCount = 0;
	}

	public override void Read(BinaryReader reader, NetVersion version)
	{
		this.Counter.Read(reader, version);
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.Counter.ReadFull(reader, version);
		this.currentCount = this.Counter.Value;
	}

	public override void Write(BinaryWriter writer)
	{
		this.Counter.Write(writer);
	}

	public override void WriteFull(BinaryWriter writer)
	{
		this.Counter.WriteFull(writer);
	}

	protected override void ForEachChild(Action<INetSerializable> childAction)
	{
		childAction(this.Counter);
	}
}
