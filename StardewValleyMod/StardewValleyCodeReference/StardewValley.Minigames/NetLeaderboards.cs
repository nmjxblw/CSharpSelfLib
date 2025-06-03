using System.Collections.Generic;
using Netcode;

namespace StardewValley.Minigames;

public class NetLeaderboards : INetObject<NetFields>
{
	public NetObjectList<NetLeaderboardsEntry> entries = new NetObjectList<NetLeaderboardsEntry>();

	public NetInt maxEntries = new NetInt(10);

	public NetFields NetFields { get; } = new NetFields("NetLeaderboards");

	public void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.entries, "entries").AddField(this.maxEntries, "maxEntries");
	}

	public NetLeaderboards()
	{
		this.InitNetFields();
	}

	public void AddScore(string name, int score)
	{
		List<NetLeaderboardsEntry> temp_entries = new List<NetLeaderboardsEntry>(this.entries);
		temp_entries.Add(new NetLeaderboardsEntry(name, score));
		temp_entries.Sort((NetLeaderboardsEntry a, NetLeaderboardsEntry b) => a.score.Value.CompareTo(b.score.Value));
		temp_entries.Reverse();
		while (temp_entries.Count > this.maxEntries.Value)
		{
			temp_entries.RemoveAt(temp_entries.Count - 1);
		}
		this.entries.Set(temp_entries);
	}

	public List<KeyValuePair<string, int>> GetScores()
	{
		List<KeyValuePair<string, int>> scores = new List<KeyValuePair<string, int>>();
		foreach (NetLeaderboardsEntry entry in this.entries)
		{
			scores.Add(new KeyValuePair<string, int>(entry.name.Value, entry.score.Value));
		}
		scores.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => a.Value.CompareTo(b.Value));
		scores.Reverse();
		return scores;
	}

	public void LoadScores(List<KeyValuePair<string, int>> scores)
	{
		this.entries.Clear();
		foreach (KeyValuePair<string, int> score in scores)
		{
			this.AddScore(score.Key, score.Value);
		}
	}
}
