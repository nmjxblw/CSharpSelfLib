using System;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests;

public class ResourceCollectionQuest : Quest
{
	/// <summary>The internal name for the NPC who gave the quest.</summary>
	[XmlElement("target")]
	public readonly NetString target = new NetString();

	/// <summary>The translated NPC dialogue shown when the quest is completed.</summary>
	[XmlElement("targetMessage")]
	public readonly NetString targetMessage = new NetString();

	/// <summary>The number of items collected so far.</summary>
	[XmlElement("numberCollected")]
	public readonly NetInt numberCollected = new NetInt();

	/// <summary>The number of items which must be collected.</summary>
	[XmlElement("number")]
	public readonly NetInt number = new NetInt();

	/// <summary>The gold reward for finishing the quest.</summary>
	[XmlElement("reward")]
	public readonly NetInt reward = new NetInt();

	/// <summary>The qualified item ID that must be collected.</summary>
	[XmlElement("resource")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The translatable text segments for the quest description shown in the quest log.</summary>
	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the <see cref="F:StardewValley.Quests.ResourceCollectionQuest.targetMessage" />.</summary>
	public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the objective shown in the quest log (like "0/5 caught").</summary>
	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Construct an instance.</summary>
	public ResourceCollectionQuest()
	{
		base.questType.Value = 10;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.parts, "parts").AddField(this.dialogueparts, "dialogueparts").AddField(this.objective, "objective")
			.AddField(this.target, "target")
			.AddField(this.targetMessage, "targetMessage")
			.AddField(this.numberCollected, "numberCollected")
			.AddField(this.number, "number")
			.AddField(this.reward, "reward")
			.AddField(this.ItemId, "ItemId");
	}

	public void loadQuestInfo()
	{
		if (this.target.Value != null || Game1.gameMode == 6)
		{
			return;
		}
		Random random = base.CreateInitializationRandom();
		base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");
		int randomResource = random.Next(6) * 2;
		for (int i = 0; i < random.Next(1, 100); i++)
		{
			random.Next();
		}
		int highest_mining_level = 0;
		int highest_foraging_level = 0;
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			highest_mining_level = Math.Max(highest_mining_level, farmer.MiningLevel);
		}
		foreach (Farmer farmer2 in Game1.getAllFarmers())
		{
			highest_foraging_level = Math.Max(highest_foraging_level, farmer2.ForagingLevel);
		}
		switch (randomResource)
		{
		case 0:
			this.ItemId.Value = "(O)378";
			this.number.Value = 20 + highest_mining_level * 2 + random.Next(-2, 4) * 2;
			this.reward.Value = this.number.Value * 10;
			this.number.Value = this.number.Value - this.number.Value % 5;
			this.target.Value = "Clint";
			break;
		case 2:
			this.ItemId.Value = "(O)380";
			this.number.Value = 15 + highest_mining_level + random.Next(-1, 3) * 2;
			this.reward.Value = this.number.Value * 15;
			this.number.Value = (int)((float)this.number.Value * 0.75f);
			this.number.Value = this.number.Value - this.number.Value % 5;
			this.target.Value = "Clint";
			break;
		case 4:
			this.ItemId.Value = "(O)382";
			this.number.Value = 10 + highest_mining_level + random.Next(-1, 3) * 2;
			this.reward.Value = this.number.Value * 25;
			this.number.Value = (int)((float)this.number.Value * 0.75f);
			this.number.Value = this.number.Value - this.number.Value % 5;
			this.target.Value = "Clint";
			break;
		case 6:
			this.ItemId.Value = ((Utility.GetAllPlayerDeepestMineLevel() > 40) ? "(O)384" : "(O)378");
			this.number.Value = 8 + highest_mining_level / 2 + random.Next(-1, 1) * 2;
			this.reward.Value = this.number.Value * 30;
			this.number.Value = (int)((float)this.number.Value * 0.75f);
			this.number.Value = this.number.Value - this.number.Value % 2;
			this.target.Value = "Clint";
			break;
		case 8:
			this.ItemId.Value = "(O)388";
			this.number.Value = 25 + highest_foraging_level + random.Next(-3, 3) * 2;
			this.number.Value = this.number.Value - this.number.Value % 5;
			this.reward.Value = this.number.Value * 8;
			this.target.Value = "Robin";
			break;
		default:
			this.ItemId.Value = "(O)390";
			this.number.Value = 25 + highest_mining_level + random.Next(-3, 3) * 2;
			this.number.Value = this.number.Value - this.number.Value % 5;
			this.reward.Value = this.number.Value * 8;
			this.target.Value = "Robin";
			break;
		}
		if (this.target.Value == null)
		{
			return;
		}
		Item item = ItemRegistry.Create(this.ItemId.Value);
		if (this.ItemId.Value != "(O)388" && this.ItemId.Value != "(O)390")
		{
			this.parts.Clear();
			int rand = random.Next(4);
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13647", this.number.Value, item, new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + (new string[4] { "13649", "13650", "13651", "13652" })[rand])));
			if (rand == 3)
			{
				this.dialogueparts.Clear();
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13655");
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13656", "13657", "13658"));
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13659");
			}
			else
			{
				this.dialogueparts.Clear();
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13662");
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13656", "13657", "13658"));
				this.dialogueparts.Add(random.NextBool() ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13667", new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13668", "13669", "13670"))) : new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13672"));
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13673");
			}
		}
		else
		{
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13674", this.number.Value, item));
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13677", (this.ItemId.Value == "(O)388") ? new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13678") : new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13679")));
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs." + random.Choose("13681", "13682", "13683"));
		}
		this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", this.reward.Value));
		this.parts.Add(this.target.Value.Equals("Clint") ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13688" : "");
		this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", "0", this.number.Value, item);
	}

	public override void reloadDescription()
	{
		if (base._questDescription == "")
		{
			this.loadQuestInfo();
		}
		if (this.parts.Count == 0 || this.parts == null || this.dialogueparts.Count == 0 || this.dialogueparts == null)
		{
			return;
		}
		string descriptionBuilder = "";
		string messageBuilder = "";
		foreach (DescriptionElement a in this.parts)
		{
			descriptionBuilder += a.loadDescriptionElement();
		}
		foreach (DescriptionElement b in this.dialogueparts)
		{
			messageBuilder += b.loadDescriptionElement();
		}
		base.questDescription = descriptionBuilder;
		this.targetMessage.Value = messageBuilder;
	}

	public override void reloadObjective()
	{
		if (this.numberCollected.Value < this.number.Value)
		{
			Item item = ItemRegistry.Create(this.ItemId.Value);
			this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", this.numberCollected.Value, this.number.Value, item);
		}
		if (this.objective.Value != null)
		{
			base.currentObjective = this.objective.Value.loadDescriptionElement();
		}
	}

	/// <inheritdoc />
	public override bool OnItemReceived(Item item, int numberAdded, bool probe = false)
	{
		bool baseChanged = base.OnItemReceived(item, numberAdded, probe);
		if (!base.completed.Value && item?.QualifiedItemId == this.ItemId.Value && numberAdded != -1 && this.numberCollected.Value < this.number.Value)
		{
			if (!probe)
			{
				this.numberCollected.Value = Math.Min(this.number.Value, this.numberCollected.Value + numberAdded);
				Game1.dayTimeMoneyBox.pingQuest(this);
				if (this.numberCollected.Value >= this.number.Value)
				{
					NPC actualTarget = Game1.getCharacterFromName(this.target.Value);
					this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13277", actualTarget);
					Game1.playSound("jingle1");
				}
			}
			return true;
		}
		return baseChanged;
	}

	/// <inheritdoc />
	public override bool OnNpcSocialized(NPC npc, bool probe = false)
	{
		bool baseChanged = base.OnNpcSocialized(npc, probe);
		if (!base.completed.Value && npc.IsVillager && npc.Name == this.target.Value && this.numberCollected.Value >= this.number.Value)
		{
			if (!probe)
			{
				npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage.Value));
				base.moneyReward.Value = this.reward.Value;
				this.questComplete();
				Game1.drawDialogue(npc);
			}
			return true;
		}
		return baseChanged;
	}
}
