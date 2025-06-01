using System.Xml.Serialization;
using Netcode;

namespace StardewValley;

public class Warp : INetObject<NetFields>
{
	[XmlElement("x")]
	private readonly NetInt x = new NetInt();

	[XmlElement("y")]
	private readonly NetInt y = new NetInt();

	[XmlElement("targetX")]
	private readonly NetInt targetX = new NetInt();

	[XmlElement("targetY")]
	private readonly NetInt targetY = new NetInt();

	[XmlElement("flipFarmer")]
	public readonly NetBool flipFarmer = new NetBool();

	[XmlElement("targetName")]
	private readonly NetString targetName = new NetString();

	[XmlElement("npcOnly")]
	public readonly NetBool npcOnly = new NetBool();

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("Warp");

	public int X => this.x.Value;

	public int Y => this.y.Value;

	public int TargetX
	{
		get
		{
			return this.targetX.Value;
		}
		set
		{
			this.targetX.Value = value;
		}
	}

	public int TargetY
	{
		get
		{
			return this.targetY.Value;
		}
		set
		{
			this.targetY.Value = value;
		}
	}

	public string TargetName
	{
		get
		{
			return this.targetName.Value;
		}
		set
		{
			this.targetName.Value = value;
		}
	}

	public Warp()
	{
		this.NetFields.SetOwner(this).AddField(this.x, "this.x").AddField(this.y, "this.y")
			.AddField(this.targetX, "this.targetX")
			.AddField(this.targetY, "this.targetY")
			.AddField(this.targetName, "this.targetName")
			.AddField(this.flipFarmer, "this.flipFarmer")
			.AddField(this.npcOnly, "this.npcOnly");
	}

	public Warp(int x, int y, string targetName, int targetX, int targetY, bool flipFarmer, bool npcOnly = false)
		: this()
	{
		this.x.Value = x;
		this.y.Value = y;
		this.targetX.Value = targetX;
		this.targetY.Value = targetY;
		this.targetName.Value = targetName;
		this.flipFarmer.Value = flipFarmer;
		this.npcOnly.Value = npcOnly;
	}
}
