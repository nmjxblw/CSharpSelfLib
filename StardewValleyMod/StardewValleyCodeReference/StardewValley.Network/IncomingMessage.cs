using System;
using System.IO;
using Netcode;

namespace StardewValley.Network;

public class IncomingMessage : IDisposable
{
	private byte messageType;

	private long farmerID;

	private byte[] data;

	private MemoryStream stream;

	private BinaryReader reader;

	public byte MessageType => this.messageType;

	public long FarmerID => this.farmerID;

	public Farmer SourceFarmer => Game1.GetPlayer(this.farmerID) ?? Game1.MasterPlayer;

	public byte[] Data => this.data;

	public BinaryReader Reader => this.reader;

	public void Read(BinaryReader reader)
	{
		this.Dispose();
		this.messageType = reader.ReadByte();
		this.farmerID = reader.ReadInt64();
		this.data = reader.ReadSkippableBytes();
		this.stream = new MemoryStream(this.data);
		this.reader = new BinaryReader(this.stream);
	}

	public void Dispose()
	{
		this.reader?.Dispose();
		this.stream?.Dispose();
		this.stream = null;
		this.reader = null;
	}
}
