using System.IO;
using Netcode;

namespace StardewValley.Monsters;

internal class ParryEventArgs : NetEventArg
{
	public int damage;

	private long farmerId;

	public Farmer who
	{
		get
		{
			return Game1.GetPlayer(this.farmerId) ?? Game1.MasterPlayer;
		}
		set
		{
			this.farmerId = value.UniqueMultiplayerID;
		}
	}

	public ParryEventArgs()
	{
	}

	public ParryEventArgs(int damage, Farmer who)
	{
		this.damage = damage;
		this.who = who;
	}

	public void Read(BinaryReader reader)
	{
		this.damage = reader.ReadInt32();
		this.farmerId = reader.ReadInt64();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(this.damage);
		writer.Write(this.farmerId);
	}
}
