using System.Collections.Generic;
using System.IO;

namespace Netcode;

public class NetRoot<T> : NetRef<T>, INetRoot where T : class, INetObject<INetSerializable>
{
	private Dictionary<long, int> connections = new Dictionary<long, int>();

	public NetClock Clock { get; } = new NetClock();

	public override bool Dirty => base.DirtyTick <= this.Clock.GetLocalTick();

	public NetRoot()
	{
		base.Root = this;
	}

	public NetRoot(T value)
		: this()
	{
		base.cleanSet(value);
	}

	public void TickTree()
	{
		this.Clock.Tick();
		base.Tick();
	}

	public override void Read(BinaryReader reader, NetVersion _)
	{
		NetVersion remoteVersion = default(NetVersion);
		remoteVersion.Read(reader);
		base.Read(reader, remoteVersion);
		this.Clock.netVersion.Merge(remoteVersion);
	}

	public void Read(BinaryReader reader)
	{
		NetVersion remoteVersion = default(NetVersion);
		remoteVersion.Read(reader);
		base.Read(reader, remoteVersion);
		this.Clock.netVersion.Merge(remoteVersion);
	}

	public override void Write(BinaryWriter writer)
	{
		this.Clock.netVersion.Write(writer);
		base.Write(writer);
		base.MarkClean();
	}

	public override void ReadFull(BinaryReader reader, NetVersion _)
	{
		base.ReadFull(reader, this.Clock.netVersion);
	}

	public static NetRoot<T> Connect(BinaryReader reader)
	{
		NetRoot<T> netRoot = new NetRoot<T>();
		netRoot.ReadConnectionPacket(reader);
		return netRoot;
	}

	public void ReadConnectionPacket(BinaryReader reader)
	{
		this.Clock.LocalId = reader.ReadByte();
		this.Clock.netVersion.Read(reader);
		base.ReadFull(reader, this.Clock.netVersion);
	}

	public void CreateConnectionPacket(BinaryWriter writer, long? connection)
	{
		if (!connection.HasValue || !this.connections.TryGetValue(connection.Value, out var peerId))
		{
			peerId = this.Clock.AddNewPeer();
			if (connection.HasValue)
			{
				this.connections[connection.Value] = peerId;
			}
		}
		writer.Write((byte)peerId);
		this.Clock.netVersion.Write(writer);
		this.WriteFull(writer);
	}

	public void Disconnect(long connection)
	{
		if (this.connections.TryGetValue(connection, out var peerId))
		{
			this.Clock.RemovePeer(peerId);
		}
	}

	public virtual NetRoot<T> Clone()
	{
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);
		using BinaryReader reader = new BinaryReader(stream);
		this.WriteFull(writer);
		stream.Seek(0L, SeekOrigin.Begin);
		NetRoot<T> netRoot = new NetRoot<T>();
		netRoot.Serializer = base.Serializer;
		netRoot.ReadFull(reader, this.Clock.netVersion);
		netRoot.reassigned.Set(default(NetVersion));
		netRoot.MarkClean();
		return netRoot;
	}

	public void CloneInto(NetRef<T> netref)
	{
		NetRoot<T> netRoot = this.Clone();
		T copy = netRoot.Value;
		netRoot.Value = null;
		netref.Value = copy;
	}
}
