using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network.NetEvents;

/// <summary>A request to drop a nut from a limited pool of nuts.</summary>
public class NutDropRequest : NetEventArg
{
	/// <summary>The key for the limited pool of nut drops.</summary>
	public string Key { get; private set; }

	/// <summary>The name of the location where the nut will be dropped.</summary>
	public string LocationName { get; private set; }

	/// <summary>The tile coordinate where the nut will be dropped in <see cref="P:StardewValley.Network.NetEvents.NutDropRequest.LocationName" />.</summary>
	public Point Tile { get; private set; }

	/// <summary>The max amount of nuts that should be dropped from the pool specified by <see cref="P:StardewValley.Network.NetEvents.NutDropRequest.Key" />.</summary>
	public int Limit { get; private set; } = 1;

	/// <summary>The number of nuts that should be dropped.</summary>
	public int RewardAmount { get; private set; } = 1;

	/// <summary>Constructs an instance.</summary>
	public NutDropRequest()
	{
	}

	/// <summary>Constructs an instance.</summary>
	/// <param name="key">The key for the limited pool of nut drops.</param>
	/// <param name="locationName">The name of the location where the nut will be dropped.</param>
	/// <param name="tile">The tile coordinate where we will drop the nut in <paramref name="locationName" />.</param>
	/// <param name="limit">The max amount of nuts that should be dropped from the pool specified by <paramref name="key" />.</param>
	/// <param name="rewardAmount">The number of nuts that should be dropped.</param>
	public NutDropRequest(string key, string locationName, Point tile, int limit, int rewardAmount)
	{
		this.Key = key;
		this.LocationName = locationName ?? "null";
		this.Tile = tile;
		this.Limit = limit;
		this.RewardAmount = rewardAmount;
	}

	/// <summary>Reads the nut drop request data from binary.</summary>
	public void Read(BinaryReader reader)
	{
		this.Key = reader.ReadString();
		this.LocationName = reader.ReadString();
		this.Tile = new Point(reader.ReadInt32(), reader.ReadInt32());
		this.Limit = reader.ReadInt32();
		this.RewardAmount = reader.ReadInt32();
	}

	/// <summary>Writes the nut drop request data to binary.</summary>
	public void Write(BinaryWriter writer)
	{
		writer.Write(this.Key);
		writer.Write(this.LocationName);
		writer.Write(this.Tile.X);
		writer.Write(this.Tile.Y);
		writer.Write(this.Limit);
		writer.Write(this.RewardAmount);
	}
}
