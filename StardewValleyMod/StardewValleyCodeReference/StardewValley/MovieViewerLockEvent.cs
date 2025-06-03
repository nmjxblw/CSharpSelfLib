using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley;

public class MovieViewerLockEvent : NetEventArg
{
	public List<long> uids;

	public int movieStartTime;

	public MovieViewerLockEvent()
	{
		this.uids = new List<long>();
		this.movieStartTime = 0;
	}

	public MovieViewerLockEvent(List<Farmer> present_farmers, int movie_start_time)
	{
		this.movieStartTime = movie_start_time;
		this.uids = new List<long>();
		foreach (Farmer farmer in present_farmers)
		{
			this.uids.Add(farmer.UniqueMultiplayerID);
		}
	}

	public void Read(BinaryReader reader)
	{
		this.uids.Clear();
		this.movieStartTime = reader.ReadInt32();
		int capacity = reader.ReadInt32();
		for (int i = 0; i < capacity; i++)
		{
			this.uids.Add(reader.ReadInt64());
		}
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(this.movieStartTime);
		writer.Write(this.uids.Count);
		for (int i = 0; i < this.uids.Count; i++)
		{
			writer.Write(this.uids[i]);
		}
	}
}
