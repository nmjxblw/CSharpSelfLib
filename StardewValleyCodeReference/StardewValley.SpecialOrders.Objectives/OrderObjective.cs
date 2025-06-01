using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives;

[XmlInclude(typeof(CollectObjective))]
[XmlInclude(typeof(DeliverObjective))]
[XmlInclude(typeof(DonateObjective))]
[XmlInclude(typeof(FishObjective))]
[XmlInclude(typeof(GiftObjective))]
[XmlInclude(typeof(JKScoreObjective))]
[XmlInclude(typeof(ReachMineFloorObjective))]
[XmlInclude(typeof(ShipObjective))]
[XmlInclude(typeof(SlayObjective))]
public class OrderObjective : INetObject<NetFields>
{
	[XmlIgnore]
	protected SpecialOrder _order;

	[XmlElement("currentCount")]
	public NetIntDelta currentCount = new NetIntDelta();

	[XmlElement("maxCount")]
	public NetInt maxCount = new NetInt(0);

	[XmlElement("description")]
	public NetString description = new NetString();

	[XmlIgnore]
	protected bool _complete;

	[XmlIgnore]
	protected bool _registered;

	[XmlElement("failOnCompletion")]
	public NetBool failOnCompletion = new NetBool(value: false);

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("OrderObjective");

	public OrderObjective()
	{
		this.InitializeNetFields();
	}

	public virtual void OnFail()
	{
	}

	public virtual void InitializeNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.currentCount, "currentCount").AddField(this.maxCount, "maxCount")
			.AddField(this.failOnCompletion, "failOnCompletion")
			.AddField(this.description, "description");
		this.currentCount.fieldChangeVisibleEvent += OnCurrentCountChanged;
	}

	protected void OnCurrentCountChanged(NetIntDelta field, int oldValue, int newValue)
	{
		if (!Utility.ShouldIgnoreValueChangeCallback())
		{
			this.CheckCompletion();
		}
	}

	public void Register(SpecialOrder new_order)
	{
		this._registered = true;
		this._order = new_order;
		this._Register();
		this.CheckCompletion(play_sound: false);
	}

	protected virtual void _Register()
	{
	}

	public virtual void Unregister()
	{
		this._registered = false;
		this._Unregister();
		this._order = null;
	}

	protected virtual void _Unregister()
	{
	}

	public virtual bool ShouldShowProgress()
	{
		return true;
	}

	public int GetCount()
	{
		return this.currentCount.Value;
	}

	public virtual void IncrementCount(int amount)
	{
		int new_value = this.GetCount() + amount;
		if (new_value < 0)
		{
			new_value = 0;
		}
		if (new_value > this.GetMaxCount())
		{
			new_value = this.GetMaxCount();
		}
		this.SetCount(new_value);
	}

	public virtual void SetCount(int new_count)
	{
		if (new_count > this.GetMaxCount())
		{
			new_count = this.GetMaxCount();
		}
		if (new_count != this.GetCount())
		{
			this.currentCount.Value = new_count;
		}
	}

	public int GetMaxCount()
	{
		return this.maxCount.Value;
	}

	public virtual void OnCompletion()
	{
	}

	public virtual void CheckCompletion(bool play_sound = true)
	{
		if (!this._registered)
		{
			return;
		}
		bool was_just_completed = false;
		if (this.GetCount() >= this.GetMaxCount() && this.CanComplete())
		{
			if (!this._complete)
			{
				was_just_completed = true;
				this.OnCompletion();
			}
			this._complete = true;
		}
		else if (this.CanUncomplete() && this._complete)
		{
			this._complete = false;
		}
		if (this._order != null)
		{
			this._order.CheckCompletion();
			if (was_just_completed && this._order.questState.Value != SpecialOrderStatus.Complete && play_sound)
			{
				Game1.playSound("jingle1");
			}
		}
	}

	public virtual bool IsComplete()
	{
		return this._complete;
	}

	public virtual bool CanUncomplete()
	{
		return false;
	}

	public virtual bool CanComplete()
	{
		return true;
	}

	public virtual string GetDescription()
	{
		return this.description.Value;
	}

	public virtual void Load(SpecialOrder order, Dictionary<string, string> data)
	{
	}
}
