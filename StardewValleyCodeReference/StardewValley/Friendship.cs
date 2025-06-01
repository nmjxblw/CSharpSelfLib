using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley;

public class Friendship : INetObject<NetFields>
{
	private readonly NetInt points = new NetInt();

	private readonly NetInt giftsThisWeek = new NetInt();

	private readonly NetInt giftsToday = new NetInt();

	private readonly NetRef<WorldDate> lastGiftDate = new NetRef<WorldDate>();

	private readonly NetBool talkedToToday = new NetBool();

	private readonly NetBool proposalRejected = new NetBool();

	private readonly NetRef<WorldDate> weddingDate = new NetRef<WorldDate>();

	private readonly NetRef<WorldDate> nextBirthingDate = new NetRef<WorldDate>();

	private readonly NetEnum<FriendshipStatus> status = new NetEnum<FriendshipStatus>(FriendshipStatus.Friendly);

	private readonly NetLong proposer = new NetLong();

	private readonly NetBool roommateMarriage = new NetBool(value: false);

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("Friendship");

	public int Points
	{
		get
		{
			return this.points.Value;
		}
		set
		{
			this.points.Value = value;
		}
	}

	public int GiftsThisWeek
	{
		get
		{
			return this.giftsThisWeek.Value;
		}
		set
		{
			this.giftsThisWeek.Value = value;
		}
	}

	public int GiftsToday
	{
		get
		{
			return this.giftsToday.Value;
		}
		set
		{
			this.giftsToday.Value = value;
		}
	}

	public WorldDate LastGiftDate
	{
		get
		{
			return this.lastGiftDate.Value;
		}
		set
		{
			this.lastGiftDate.Value = value;
		}
	}

	public bool TalkedToToday
	{
		get
		{
			return this.talkedToToday.Value;
		}
		set
		{
			this.talkedToToday.Value = value;
		}
	}

	public bool ProposalRejected
	{
		get
		{
			return this.proposalRejected.Value;
		}
		set
		{
			this.proposalRejected.Value = value;
		}
	}

	public WorldDate WeddingDate
	{
		get
		{
			return this.weddingDate.Value;
		}
		set
		{
			this.weddingDate.Value = value;
		}
	}

	public WorldDate NextBirthingDate
	{
		get
		{
			return this.nextBirthingDate.Value;
		}
		set
		{
			this.nextBirthingDate.Value = value;
		}
	}

	public FriendshipStatus Status
	{
		get
		{
			return this.status.Value;
		}
		set
		{
			this.status.Value = value;
		}
	}

	public long Proposer
	{
		get
		{
			return this.proposer.Value;
		}
		set
		{
			this.proposer.Value = value;
		}
	}

	public bool RoommateMarriage
	{
		get
		{
			return this.roommateMarriage.Value;
		}
		set
		{
			this.roommateMarriage.Value = value;
		}
	}

	public int DaysMarried
	{
		get
		{
			if (this.WeddingDate == null || this.WeddingDate.TotalDays > Game1.Date.TotalDays)
			{
				return 0;
			}
			return Game1.Date.TotalDays - this.WeddingDate.TotalDays;
		}
	}

	public int CountdownToWedding
	{
		get
		{
			if (this.WeddingDate == null || this.WeddingDate.TotalDays < Game1.Date.TotalDays)
			{
				return 0;
			}
			return this.WeddingDate.TotalDays - Game1.Date.TotalDays;
		}
	}

	public int DaysUntilBirthing
	{
		get
		{
			if (this.NextBirthingDate == null)
			{
				return -1;
			}
			return Math.Max(0, this.NextBirthingDate.TotalDays - Game1.Date.TotalDays);
		}
	}

	public Friendship()
	{
		this.NetFields.SetOwner(this).AddField(this.points, "points").AddField(this.giftsThisWeek, "giftsThisWeek")
			.AddField(this.giftsToday, "giftsToday")
			.AddField(this.lastGiftDate, "lastGiftDate")
			.AddField(this.talkedToToday, "talkedToToday")
			.AddField(this.proposalRejected, "proposalRejected")
			.AddField(this.weddingDate, "weddingDate")
			.AddField(this.nextBirthingDate, "nextBirthingDate")
			.AddField(this.status, "status")
			.AddField(this.proposer, "proposer")
			.AddField(this.roommateMarriage, "roommateMarriage");
	}

	public Friendship(int startingPoints)
		: this()
	{
		this.Points = startingPoints;
	}

	public void Clear()
	{
		this.points.Value = 0;
		this.giftsThisWeek.Value = 0;
		this.giftsToday.Value = 0;
		this.lastGiftDate.Value = null;
		this.talkedToToday.Value = false;
		this.proposalRejected.Value = false;
		this.roommateMarriage.Value = false;
		this.weddingDate.Value = null;
		this.nextBirthingDate.Value = null;
		this.status.Value = FriendshipStatus.Friendly;
		this.proposer.Value = 0L;
	}

	public bool IsDating()
	{
		if (this.Status != FriendshipStatus.Dating && this.Status != FriendshipStatus.Engaged)
		{
			return this.Status == FriendshipStatus.Married;
		}
		return true;
	}

	public bool IsEngaged()
	{
		return this.Status == FriendshipStatus.Engaged;
	}

	public bool IsMarried()
	{
		return this.Status == FriendshipStatus.Married;
	}

	public bool IsDivorced()
	{
		return this.Status == FriendshipStatus.Divorced;
	}

	public bool IsRoommate()
	{
		if (this.IsMarried())
		{
			return this.roommateMarriage.Value;
		}
		return false;
	}
}
