using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Quests;

public class ItemHarvestQuest : Quest
{
	/// <summary>The qualified item ID to harvest.</summary>
	[XmlElement("itemIndex")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The number of items that must be harvested.</summary>
	[XmlElement("number")]
	public readonly NetInt Number = new NetInt();

	/// <summary>Construct an instance.</summary>
	public ItemHarvestQuest()
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to harvest.</param>
	/// <param name="number">The number of items that must be harvested.</param>
	public ItemHarvestQuest(string itemId, int number = 1)
	{
		this.ItemId.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
		this.Number.Value = number;
		base.questType.Value = 9;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.ItemId, "ItemId").AddField(this.Number, "Number");
	}

	/// <inheritdoc />
	public override bool OnItemReceived(Item item, int numberAdded, bool probe = false)
	{
		bool baseChanged = base.OnItemReceived(item, numberAdded, probe);
		if (!base.completed.Value && (item.QualifiedItemId == this.ItemId.Value || (this.ItemId.Value.StartsWith('-') && item.Category.ToString() == this.ItemId.Value)))
		{
			int newNumber = this.Number.Value - numberAdded;
			bool complete = newNumber <= 0;
			if (!probe)
			{
				this.Number.Value = newNumber;
				if (complete)
				{
					this.questComplete();
				}
			}
			return true;
		}
		return baseChanged;
	}
}
