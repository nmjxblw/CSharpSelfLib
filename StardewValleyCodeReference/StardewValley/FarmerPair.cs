using System;

namespace StardewValley;

public struct FarmerPair
{
	public long Farmer1;

	public long Farmer2;

	public static FarmerPair MakePair(long f1, long f2)
	{
		return new FarmerPair
		{
			Farmer1 = Math.Min(f1, f2),
			Farmer2 = Math.Max(f1, f2)
		};
	}

	public bool Contains(long f)
	{
		if (this.Farmer1 != f)
		{
			return this.Farmer2 == f;
		}
		return true;
	}

	public long GetOther(long f)
	{
		if (this.Farmer1 == f)
		{
			return this.Farmer2;
		}
		return this.Farmer1;
	}

	public bool Equals(FarmerPair other)
	{
		if (this.Farmer1 == other.Farmer1)
		{
			return this.Farmer2 == other.Farmer2;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is FarmerPair pair)
		{
			return this.Equals(pair);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.Farmer1.GetHashCode() ^ (this.Farmer2.GetHashCode() << 16);
	}
}
