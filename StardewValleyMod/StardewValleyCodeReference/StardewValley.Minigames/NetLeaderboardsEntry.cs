using Netcode;

namespace StardewValley.Minigames;

public class NetLeaderboardsEntry : INetObject<NetFields>
{
	public readonly NetString name = new NetString("");

	public readonly NetInt score = new NetInt(0);

	public NetFields NetFields { get; } = new NetFields("NetLeaderboardsEntry");

	public void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.name, "name").AddField(this.score, "score");
	}

	public NetLeaderboardsEntry()
	{
		this.InitNetFields();
	}

	public NetLeaderboardsEntry(string new_name, int new_score)
	{
		this.InitNetFields();
		this.name.Value = new_name;
		this.score.Value = new_score;
	}
}
