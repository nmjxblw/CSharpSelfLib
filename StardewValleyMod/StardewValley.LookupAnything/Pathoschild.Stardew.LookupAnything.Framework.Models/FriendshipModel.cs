using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models;

internal record FriendshipModel
{
	public bool CanDate { get; }

	public bool IsDating { get; }

	public bool IsSpouse { get; }

	public bool IsHousemate { get; }

	public bool IsDivorced { get; }

	public bool HasStardrop { get; }

	public bool TalkedToday { get; }

	public int GiftsToday { get; }

	public int GiftsThisWeek { get; }

	public FriendshipStatus Status { get; }

	public int Points { get; }

	public int? StardropPoints { get; }

	public int MaxPoints { get; }

	public int PointsPerLevel { get; }

	public int FilledHearts { get; }

	public int EmptyHearts { get; }

	public int LockedHearts { get; }

	public int TotalHearts => this.FilledHearts + this.EmptyHearts + this.LockedHearts;

	public FriendshipModel(Farmer player, NPC npc, Friendship friendship, ConstantData constants)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		bool marriedOrRoommate = friendship.IsMarried();
		bool roommate = friendship.IsRoommate();
		this.CanDate = ((NetFieldBase<bool, NetBool>)(object)npc.datable).Value;
		this.IsDating = friendship.IsDating();
		this.IsSpouse = marriedOrRoommate && !roommate;
		this.IsHousemate = marriedOrRoommate && roommate;
		this.IsDivorced = friendship.IsDivorced();
		this.Status = friendship.Status;
		this.TalkedToday = friendship.TalkedToToday;
		this.GiftsToday = friendship.GiftsToday;
		this.GiftsThisWeek = friendship.GiftsThisWeek;
		this.MaxPoints = ((this.IsSpouse || this.IsHousemate) ? constants.SpouseMaxFriendship : 2500);
		this.Points = friendship.Points;
		this.PointsPerLevel = 250;
		this.FilledHearts = this.Points / 250;
		this.LockedHearts = ((this.CanDate && !this.IsDating) ? constants.DatingHearts : 0);
		this.EmptyHearts = this.MaxPoints / 250 - this.FilledHearts - this.LockedHearts;
		if (this.IsSpouse || this.IsHousemate)
		{
			this.StardropPoints = constants.SpouseFriendshipForStardrop;
			this.HasStardrop = !((NetHashSet<string>)(object)player.mailReceived).Contains("CF_Spouse");
		}
	}

	public FriendshipModel(int points, int pointsPerLevel, int maxPoints)
	{
		this.Points = points;
		this.PointsPerLevel = pointsPerLevel;
		this.MaxPoints = maxPoints;
		this.FilledHearts = this.Points / pointsPerLevel;
		this.EmptyHearts = this.MaxPoints / pointsPerLevel - this.FilledHearts;
	}

	public int GetPointsToNext()
	{
		if (this.Points < this.MaxPoints)
		{
			return this.PointsPerLevel - this.Points % this.PointsPerLevel;
		}
		if (this.StardropPoints.HasValue && this.Points < this.StardropPoints)
		{
			return this.StardropPoints.Value - this.Points;
		}
		return 0;
	}
}
