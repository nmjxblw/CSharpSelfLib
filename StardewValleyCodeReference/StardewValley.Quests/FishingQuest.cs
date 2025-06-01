using System;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests;

public class FishingQuest : Quest
{
	/// <summary>The internal name for the NPC who gave the quest.</summary>
	[XmlElement("target")]
	public readonly NetString target = new NetString();

	/// <summary>The translated text for the NPC dialogue shown when the quest is completed.</summary>
	public string targetMessage;

	/// <summary>The number of fish which must be caught.</summary>
	[XmlElement("numberToFish")]
	public readonly NetInt numberToFish = new NetInt();

	/// <summary>The gold reward for finishing the quest.</summary>
	[XmlElement("reward")]
	public readonly NetInt reward = new NetInt();

	/// <summary>The number of fish caught so far.</summary>
	[XmlElement("numberFished")]
	public readonly NetInt numberFished = new NetInt();

	/// <summary>The qualified item ID for the fish to catch.</summary>
	[XmlElement("whichFish")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The translatable text segments for the quest description in the quest log.</summary>
	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the NPC dialogue shown when the quest is completed.</summary>
	public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the objective shown in the quest log (like "0/5 caught").</summary>
	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Construct an instance.</summary>
	public FishingQuest()
	{
		base.questType.Value = 7;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="itemId">The qualified item ID for the fish to catch.</param>
	/// <param name="numberToFish">The number of fish which must be caught.</param>
	/// <param name="target">The internal name for the NPC who gave the quest.</param>
	/// <param name="returnDialogue">The translated text for the NPC dialogue shown when the quest is completed.</param>
	public FishingQuest(string itemId, int numberToFish, string target, string questTitle, string questDescription, string returnDialogue)
		: this()
	{
		this.ItemId.Value = ItemRegistry.QualifyItemId(itemId);
		this.numberToFish.Value = numberToFish;
		this.target.Value = target;
		base.questDescription = questDescription;
		base.questTitle = questTitle;
		base._loadedTitle = true;
		this.targetMessage = returnDialogue;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.parts, "parts").AddField(this.dialogueparts, "dialogueparts").AddField(this.objective, "objective")
			.AddField(this.target, "target")
			.AddField(this.numberToFish, "numberToFish")
			.AddField(this.reward, "reward")
			.AddField(this.numberFished, "numberFished")
			.AddField(this.ItemId, "ItemId");
	}

	public void loadQuestInfo()
	{
		if (this.target.Value != null && this.ItemId.Value != null)
		{
			return;
		}
		Random random = base.CreateInitializationRandom();
		base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
		if (random.NextBool())
		{
			switch (Game1.season)
			{
			case Season.Spring:
				this.ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)142", "(O)143", "(O)145", "(O)147");
				break;
			case Season.Summer:
				this.ItemId.Value = random.Choose<string>("(O)130", "(O)136", "(O)138", "(O)142", "(O)144", "(O)145", "(O)146", "(O)149", "(O)150");
				break;
			case Season.Fall:
				this.ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)139", "(O)142", "(O)143", "(O)150");
				break;
			case Season.Winter:
				this.ItemId.Value = random.Choose<string>("(O)130", "(O)131", "(O)136", "(O)141", "(O)144", "(O)146", "(O)147", "(O)150", "(O)151");
				break;
			}
			Item fish = ItemRegistry.Create(this.ItemId.Value);
			bool isOctopus = this.ItemId.Value == "(O)149";
			this.numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, this.GetGoldRewardPerItem(fish))) + Game1.player.FishingLevel / 5;
			this.reward.Value = this.numberToFish.Value * this.GetGoldRewardPerItem(fish);
			this.target.Value = "Demetrius";
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13228", fish, this.numberToFish.Value));
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13231", fish, random.Choose(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13233"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13234"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13235"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13236", fish))));
			this.objective.Value = (isOctopus ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", 0, this.numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, this.numberToFish.Value, fish));
		}
		else
		{
			switch (Game1.season)
			{
			case Season.Spring:
				this.ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)142", "(O)143", "(O)145", "(O)147", "(O)702");
				break;
			case Season.Summer:
				this.ItemId.Value = random.Choose<string>("(O)128", "(O)130", "(O)136", "(O)138", "(O)142", "(O)144", "(O)145", "(O)146", "(O)149", "(O)150", "(O)702");
				break;
			case Season.Fall:
				this.ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)139", "(O)142", "(O)143", "(O)150", "(O)699", "(O)702", "(O)705");
				break;
			case Season.Winter:
				this.ItemId.Value = random.Choose<string>("(O)130", "(O)131", "(O)136", "(O)141", "(O)143", "(O)144", "(O)146", "(O)147", "(O)151", "(O)699", "(O)702", "(O)705");
				break;
			}
			this.target.Value = "Willy";
			Item fish2 = ItemRegistry.Create(this.ItemId.Value);
			bool isSquid = this.ItemId.Value == "(O)151";
			this.numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, this.GetGoldRewardPerItem(fish2))) + Game1.player.FishingLevel / 5;
			this.reward.Value = this.numberToFish.Value * this.GetGoldRewardPerItem(fish2);
			this.parts.Clear();
			this.parts.Add(isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", this.reward.Value, this.numberToFish.Value, new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13253")) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", this.reward.Value, this.numberToFish.Value, fish2));
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13256", fish2));
			this.dialogueparts.Add(random.Choose(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13258"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13259"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13260", new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs." + random.Choose<string>("13261", "13262", "13263", "13264", "13265", "13266"))), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13267")));
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13268"));
			this.objective.Value = (isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", 0, this.numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, this.numberToFish.Value, fish2));
		}
		this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", this.reward.Value));
		this.parts.Add("Strings\\StringsFromCSFiles:FishingQuest.cs.13275");
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
		this.targetMessage = messageBuilder;
	}

	public override void reloadObjective()
	{
		bool isOctopus = this.ItemId.Value == "(O)149";
		bool isSquid = this.ItemId.Value == "(O)151";
		if (this.numberFished.Value < this.numberToFish.Value)
		{
			this.objective.Value = (isOctopus ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", this.numberFished.Value, this.numberToFish.Value) : (isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", this.numberFished.Value, this.numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", this.numberFished.Value, this.numberToFish.Value, ItemRegistry.Create(this.ItemId.Value))));
		}
		if (this.objective.Value != null)
		{
			base.currentObjective = this.objective.Value.loadDescriptionElement();
		}
	}

	/// <inheritdoc />
	public override bool OnFishCaught(string fishId, int numberCaught, int size, bool probe = false)
	{
		bool baseChanged = base.OnFishCaught(fishId, numberCaught, size, probe);
		this.loadQuestInfo();
		if (fishId == this.ItemId.Value && this.numberFished.Value < this.numberToFish.Value)
		{
			if (!probe)
			{
				this.numberFished.Value = Math.Min(this.numberToFish.Value, this.numberFished.Value + numberCaught);
				Game1.dayTimeMoneyBox.pingQuest(this);
				if (this.numberFished.Value >= this.numberToFish.Value)
				{
					if (this.target.Value == null)
					{
						this.target.Value = "Willy";
					}
					NPC actualTarget = Game1.getCharacterFromName(this.target.Value);
					this.objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", actualTarget);
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
		if (this.numberFished.Value >= this.numberToFish.Value && this.target.Value != null && npc.Name == this.target.Value && npc.IsVillager && !base.completed.Value)
		{
			if (!probe)
			{
				npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage));
				base.moneyReward.Value = this.reward.Value;
				this.questComplete();
				Game1.drawDialogue(npc);
			}
			return true;
		}
		return baseChanged;
	}

	/// <summary>Get the gold reward for a given item.</summary>
	/// <param name="item">The item instance.</param>
	private int GetGoldRewardPerItem(Item item)
	{
		if (item is Object obj)
		{
			return obj.Price;
		}
		return (int)((float)item.salePrice() * 1.5f);
	}
}
