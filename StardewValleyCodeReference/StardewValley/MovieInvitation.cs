using Netcode;
using StardewValley.Network;

namespace StardewValley;

public class MovieInvitation : INetObject<NetFields>
{
	private NetFarmerRef _farmer = new NetFarmerRef();

	protected NetString _invitedNPCName = new NetString();

	protected NetBool _fulfilled = new NetBool(value: false);

	public NetFields NetFields { get; } = new NetFields("MovieInvitation");

	public Farmer farmer
	{
		get
		{
			return this._farmer.Value;
		}
		set
		{
			this._farmer.Value = value;
		}
	}

	public NPC invitedNPC
	{
		get
		{
			return Game1.getCharacterFromName(this._invitedNPCName.Value);
		}
		set
		{
			if (value == null)
			{
				this._invitedNPCName.Set(null);
			}
			else
			{
				this._invitedNPCName.Set(value.name.Value);
			}
		}
	}

	public bool fulfilled
	{
		get
		{
			return this._fulfilled.Value;
		}
		set
		{
			this._fulfilled.Set(value);
		}
	}

	public MovieInvitation()
	{
		this.NetFields.SetOwner(this).AddField(this._farmer.NetFields, "_farmer.NetFields").AddField(this._invitedNPCName, "_invitedNPCName")
			.AddField(this._fulfilled, "_fulfilled");
	}
}
