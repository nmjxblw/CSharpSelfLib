using System;
using Netcode;

namespace StardewValley.Network;

public class NetDancePartner : INetObject<NetFields>
{
	private readonly NetFarmerRef farmer = new NetFarmerRef();

	private readonly NetString villager = new NetString();

	public Character Value
	{
		get
		{
			return this.GetCharacter();
		}
		set
		{
			this.SetCharacter(value);
		}
	}

	public NetFields NetFields { get; } = new NetFields("NetDancePartner");

	public NetDancePartner()
	{
		this.NetFields.SetOwner(this).AddField(this.farmer.NetFields, "farmer.NetFields").AddField(this.villager, "villager");
	}

	public NetDancePartner(Farmer farmer)
	{
		this.farmer.Value = farmer;
	}

	public NetDancePartner(string villagerName)
	{
		this.villager.Value = villagerName;
	}

	public Character GetCharacter()
	{
		if (this.farmer.Value != null)
		{
			return this.farmer.Value;
		}
		if (Game1.CurrentEvent != null && this.villager.Value != null)
		{
			return Game1.CurrentEvent.getActorByName(this.villager.Value);
		}
		return null;
	}

	public void SetCharacter(Character value)
	{
		if (value != null)
		{
			if (!(value is Farmer curFarmer))
			{
				if (!(value is NPC npc))
				{
					throw new ArgumentException(value.ToString());
				}
				if (!npc.IsVillager)
				{
					throw new ArgumentException(value.ToString());
				}
				this.farmer.Value = null;
				this.villager.Value = npc.Name;
			}
			else
			{
				this.farmer.Value = curFarmer;
				this.villager.Value = null;
			}
		}
		else
		{
			this.farmer.Value = null;
			this.villager.Value = null;
		}
	}

	public NPC TryGetVillager()
	{
		if (this.farmer.Value != null)
		{
			return null;
		}
		if (Game1.CurrentEvent != null && this.villager.Value != null)
		{
			return Game1.CurrentEvent.getActorByName(this.villager.Value);
		}
		return null;
	}

	public Farmer TryGetFarmer()
	{
		return this.farmer.Value;
	}

	public bool IsFarmer()
	{
		return this.TryGetFarmer() != null;
	}

	public bool IsVillager()
	{
		return this.TryGetVillager() != null;
	}

	public Gender GetGender()
	{
		if (this.IsFarmer())
		{
			return this.TryGetFarmer().Gender;
		}
		if (this.IsVillager())
		{
			return this.TryGetVillager().Gender;
		}
		return Gender.Undefined;
	}
}
