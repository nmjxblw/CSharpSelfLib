using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Monsters;

namespace StardewValley.Quests;

[XmlInclude(typeof(CraftingQuest))]
[XmlInclude(typeof(DescriptionElement))]
[XmlInclude(typeof(FishingQuest))]
[XmlInclude(typeof(GoSomewhereQuest))]
[XmlInclude(typeof(HaveBuildingQuest))]
[XmlInclude(typeof(ItemDeliveryQuest))]
[XmlInclude(typeof(ItemHarvestQuest))]
[XmlInclude(typeof(LostItemQuest))]
[XmlInclude(typeof(ResourceCollectionQuest))]
[XmlInclude(typeof(SecretLostItemQuest))]
[XmlInclude(typeof(SlayMonsterQuest))]
[XmlInclude(typeof(SocializeQuest))]
public class Quest : INetObject<NetFields>, IQuest, IHaveModData
{
	public const int type_basic = 1;

	public const int type_crafting = 2;

	public const int type_itemDelivery = 3;

	public const int type_monster = 4;

	public const int type_socialize = 5;

	public const int type_location = 6;

	public const int type_fishing = 7;

	public const int type_building = 8;

	public const int type_harvest = 9;

	public const int type_resource = 10;

	public const int type_weeding = 11;

	public string _currentObjective = "";

	public string _questDescription = "";

	public string _questTitle = "";

	[XmlElement("rewardDescription")]
	public readonly NetString rewardDescription = new NetString();

	[XmlElement("accepted")]
	public readonly NetBool accepted = new NetBool();

	[XmlElement("completed")]
	public readonly NetBool completed = new NetBool();

	[XmlElement("dailyQuest")]
	public readonly NetBool dailyQuest = new NetBool();

	[XmlElement("showNew")]
	public readonly NetBool showNew = new NetBool();

	[XmlElement("canBeCancelled")]
	public readonly NetBool canBeCancelled = new NetBool();

	[XmlElement("destroy")]
	public readonly NetBool destroy = new NetBool();

	[XmlElement("id")]
	public readonly NetString id = new NetString();

	[XmlElement("moneyReward")]
	public readonly NetInt moneyReward = new NetInt();

	[XmlElement("questType")]
	public readonly NetInt questType = new NetInt();

	[XmlElement("daysLeft")]
	public readonly NetInt daysLeft = new NetInt();

	[XmlElement("dayQuestAccepted")]
	public readonly NetInt dayQuestAccepted = new NetInt(-1);

	[XmlArrayItem("int")]
	public readonly NetStringList nextQuests = new NetStringList();

	/// <summary>Obsolete since 1.6.9. This is only kept to preserve data from old save files; use more specific fields like <see cref="F:StardewValley.Quests.HaveBuildingQuest.buildingType" /> instead.</summary>
	[XmlElement("completionString")]
	public string obsolete_completionString;

	private bool _loadedDescription;

	protected bool _loadedTitle;

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();

	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return this.modData.GetForSerialization();
		}
		set
		{
			this.modData.SetFromSerialization(value);
		}
	}

	public NetFields NetFields { get; }

	public string questTitle
	{
		get
		{
			if (!this._loadedTitle)
			{
				switch (this.questType.Value)
				{
				case 3:
					if (this is ItemDeliveryQuest deliveryQuest && deliveryQuest.target.Value != null)
					{
						this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ItemDeliveryQuestTitle", NPC.GetDisplayName(deliveryQuest.target.Value));
					}
					else
					{
						this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13285");
					}
					break;
				case 4:
					if (this is SlayMonsterQuest slayQuest && slayQuest.monsterName.Value != null)
					{
						this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:MonsterQuestTitle", Monster.GetDisplayName(slayQuest.monsterName.Value));
					}
					else
					{
						this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
					}
					break;
				case 5:
					this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");
					break;
				case 7:
					if (this is FishingQuest fishQuest && fishQuest.ItemId.Value != null)
					{
						string fishName = "???";
						ParsedItemData data2 = ItemRegistry.GetDataOrErrorItem(fishQuest.ItemId.Value);
						if (!data2.IsErrorItem)
						{
							fishName = data2.DisplayName;
						}
						this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:FishingQuestTitle", fishName);
					}
					else
					{
						this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
					}
					break;
				case 10:
					if (this is ResourceCollectionQuest collectQuest && collectQuest.ItemId.Value != null)
					{
						string resourceName = "???";
						ParsedItemData data = ItemRegistry.GetDataOrErrorItem(collectQuest.ItemId.Value);
						if (!data.IsErrorItem)
						{
							resourceName = data.DisplayName;
						}
						this._questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ResourceQuestTitle", resourceName);
					}
					else
					{
						this._questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13640");
					}
					break;
				}
				string[] fields = Quest.GetRawQuestFields(this.id.Value);
				this._questTitle = ArgUtility.Get(fields, 1, this._questTitle);
				this._loadedTitle = true;
			}
			if (this._questTitle == null)
			{
				this._questTitle = "";
			}
			return this._questTitle;
		}
		set
		{
			this._questTitle = value;
		}
	}

	[XmlIgnore]
	public string questDescription
	{
		get
		{
			if (!this._loadedDescription)
			{
				this.reloadDescription();
				string[] fields = Quest.GetRawQuestFields(this.id.Value);
				this._questDescription = ArgUtility.Get(fields, 2, this._questDescription);
				this._loadedDescription = true;
			}
			if (this._questDescription == null)
			{
				this._questDescription = "";
			}
			return this._questDescription;
		}
		set
		{
			this._questDescription = value;
		}
	}

	[XmlIgnore]
	public string currentObjective
	{
		get
		{
			string[] fields = Quest.GetRawQuestFields(this.id.Value);
			this._currentObjective = ArgUtility.Get(fields, 3, this._currentObjective, allowBlank: false);
			this.reloadObjective();
			if (this._currentObjective == null)
			{
				this._currentObjective = "";
			}
			return this._currentObjective;
		}
		set
		{
			this._currentObjective = value;
		}
	}

	public Quest()
	{
		this.NetFields = new NetFields(NetFields.GetNameForInstance(this));
		this.initNetFields();
	}

	/// <summary>Register all net fields and their events.</summary>
	protected virtual void initNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.rewardDescription, "rewardDescription").AddField(this.accepted, "accepted")
			.AddField(this.completed, "completed")
			.AddField(this.dailyQuest, "dailyQuest")
			.AddField(this.showNew, "showNew")
			.AddField(this.canBeCancelled, "canBeCancelled")
			.AddField(this.destroy, "destroy")
			.AddField(this.id, "id")
			.AddField(this.moneyReward, "moneyReward")
			.AddField(this.questType, "questType")
			.AddField(this.daysLeft, "daysLeft")
			.AddField(this.nextQuests, "nextQuests")
			.AddField(this.dayQuestAccepted, "dayQuestAccepted")
			.AddField(this.modData, "modData");
	}

	public static string[] GetRawQuestFields(string id)
	{
		if (id == null)
		{
			return null;
		}
		Dictionary<string, string> questData = DataLoader.Quests(Game1.content);
		if (questData == null || !questData.TryGetValue(id, out var rawData))
		{
			return null;
		}
		return rawData.Split('/');
	}

	public static Quest getQuestFromId(string id)
	{
		string[] fields = Quest.GetRawQuestFields(id);
		if (fields == null)
		{
			return null;
		}
		if (!ArgUtility.TryGet(fields, 0, out var questType, out var error, allowBlank: false, "string questType") || !ArgUtility.TryGet(fields, 1, out var title, out error, allowBlank: false, "string title") || !ArgUtility.TryGet(fields, 2, out var description, out error, allowBlank: false, "string description") || !ArgUtility.TryGetOptional(fields, 3, out var objective, out error, null, allowBlank: false, "string objective") || !ArgUtility.TryGetOptional(fields, 5, out var rawNextQuests, out error, null, allowBlank: false, "string rawNextQuests") || !ArgUtility.TryGetInt(fields, 6, out var moneyReward, out error, "int moneyReward") || !ArgUtility.TryGetOptional(fields, 7, out var rewardDescription, out error, null, allowBlank: false, "string rewardDescription") || !ArgUtility.TryGetOptionalBool(fields, 8, out var canBeCancelled, out error, defaultValue: false, "bool canBeCancelled"))
		{
			return Quest.LogParseError(id, error);
		}
		string[] nextQuests = ArgUtility.SplitBySpace(rawNextQuests);
		Quest q;
		switch (questType)
		{
		case "Crafting":
		{
			if (!Quest.TryParseConditions(fields, out var conditions4, out error))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions4, 0, out var itemId2, out error, allowBlank: false, "string itemId"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			bool? isBigCraftable = null;
			if (ArgUtility.HasIndex(conditions4, 1))
			{
				if (!ArgUtility.TryGetOptionalBool(conditions4, 1, out var isBigCraftableValue, out error, defaultValue: false, "bool isBigCraftableValue"))
				{
					return Quest.LogConditionsParseError(id, error);
				}
				isBigCraftable = isBigCraftableValue;
			}
			if (!ItemRegistry.IsQualifiedItemId(itemId2))
			{
				itemId2 = ((!isBigCraftable.HasValue) ? (ItemRegistry.QualifyItemId(itemId2) ?? itemId2) : (isBigCraftable.Value ? ("(BC)" + itemId2) : ("(O)" + itemId2)));
			}
			q = new CraftingQuest(itemId2);
			q.questType.Value = 2;
			break;
		}
		case "Location":
		{
			if (!Quest.TryParseConditions(fields, out var conditions3, out error))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions3, 0, out var locationName, out error, allowBlank: false, "string locationName"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			q = new GoSomewhereQuest(locationName);
			q.questType.Value = 6;
			break;
		}
		case "Building":
		{
			if (!Quest.TryParseConditions(fields, out var conditions6, out error))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions6, 0, out var buildingType, out error, allowBlank: false, "string buildingType"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			q = new HaveBuildingQuest(buildingType);
			break;
		}
		case "ItemDelivery":
		{
			if (!Quest.TryParseConditions(fields, out var conditions5, out error) || !ArgUtility.TryGet(fields, 9, out var targetMessage, out error, allowBlank: false, "string targetMessage"))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions5, 0, out var npcName2, out error, allowBlank: false, "string npcName") || !ArgUtility.TryGet(conditions5, 1, out var itemId3, out error, allowBlank: false, "string itemId") || !ArgUtility.TryGetOptionalInt(conditions5, 2, out var numberRequired, out error, 1, "int numberRequired"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			ItemDeliveryQuest itemDeliveryQuest = new ItemDeliveryQuest(npcName2, itemId3);
			itemDeliveryQuest.targetMessage = targetMessage;
			itemDeliveryQuest.number.Value = numberRequired;
			itemDeliveryQuest.questType.Value = 3;
			q = itemDeliveryQuest;
			break;
		}
		case "Monster":
		{
			if (!Quest.TryParseConditions(fields, out var conditions2, out error))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions2, 0, out var monsterName, out error, allowBlank: false, "string monsterName") || !ArgUtility.TryGetInt(conditions2, 1, out var numberToKill, out error, "int numberToKill") || !ArgUtility.TryGetOptional(conditions2, 2, out var targetNpc, out error, null, allowBlank: true, "string targetNpc") || !ArgUtility.TryGetOptionalBool(conditions2, 3, out var ignoreFarmMonsters, out error, defaultValue: true, "bool ignoreFarmMonsters"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			SlayMonsterQuest slayQuest = new SlayMonsterQuest();
			slayQuest.loadQuestInfo();
			slayQuest.monster.Value.Name = monsterName.Replace('_', ' ');
			slayQuest.monsterName.Value = slayQuest.monster.Value.Name;
			slayQuest.numberToKill.Value = numberToKill;
			slayQuest.ignoreFarmMonsters.Value = ignoreFarmMonsters;
			slayQuest.target.Value = targetNpc ?? "null";
			slayQuest.questType.Value = 4;
			q = slayQuest;
			break;
		}
		case "Basic":
			q = new Quest();
			q.questType.Value = 1;
			break;
		case "Social":
		{
			SocializeQuest socializeQuest = new SocializeQuest();
			socializeQuest.loadQuestInfo();
			q = socializeQuest;
			break;
		}
		case "ItemHarvest":
		{
			if (!Quest.TryParseConditions(fields, out var conditions8, out error))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions8, 0, out var itemId5, out error, allowBlank: false, "string itemId") || !ArgUtility.TryGetOptionalInt(conditions8, 1, out var numberRequired2, out error, 1, "int numberRequired"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			q = new ItemHarvestQuest(itemId5, numberRequired2);
			break;
		}
		case "LostItem":
		{
			if (!Quest.TryParseConditions(fields, out var conditions7, out error))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions7, 0, out var npcName3, out error, allowBlank: false, "string npcName") || !ArgUtility.TryGet(conditions7, 1, out var itemId4, out error, allowBlank: false, "string itemId") || !ArgUtility.TryGet(conditions7, 2, out var locationOfItem, out error, allowBlank: false, "string locationOfItem") || !ArgUtility.TryGetInt(conditions7, 3, out var tileX, out error, "int tileX") || !ArgUtility.TryGetInt(conditions7, 4, out var tileY, out error, "int tileY"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			q = new LostItemQuest(npcName3, locationOfItem, itemId4, tileX, tileY);
			break;
		}
		case "SecretLostItem":
		{
			if (!Quest.TryParseConditions(fields, out var conditions, out error))
			{
				return Quest.LogParseError(id, error);
			}
			if (!ArgUtility.TryGet(conditions, 0, out var npcName, out error, allowBlank: false, "string npcName") || !ArgUtility.TryGet(conditions, 1, out var itemId, out error, allowBlank: false, "string itemId") || !ArgUtility.TryGetInt(conditions, 2, out var friendshipReward, out error, "int friendshipReward") || !ArgUtility.TryGetOptional(conditions, 3, out var exclusiveQuestId, out error, null, allowBlank: false, "string exclusiveQuestId"))
			{
				return Quest.LogConditionsParseError(id, error);
			}
			q = new SecretLostItemQuest(npcName, itemId, friendshipReward, exclusiveQuestId);
			break;
		}
		default:
			return Quest.LogParseError(id, "quest type '" + questType + "' doesn't match a known type.");
		}
		q.id.Value = id;
		q.questTitle = title;
		q.questDescription = description;
		q.currentObjective = objective;
		string[] array = nextQuests;
		for (int i = 0; i < array.Length; i++)
		{
			string nextQuest = array[i];
			if (nextQuest.StartsWith('h'))
			{
				if (!Game1.IsMasterGame)
				{
					continue;
				}
				nextQuest = nextQuest.Substring(1);
			}
			q.nextQuests.Add(nextQuest);
		}
		q.showNew.Value = true;
		q.moneyReward.Value = moneyReward;
		q.rewardDescription.Value = ((moneyReward == -1) ? null : rewardDescription);
		q.canBeCancelled.Value = canBeCancelled;
		return q;
	}

	public virtual void reloadObjective()
	{
	}

	public virtual void reloadDescription()
	{
	}

	public virtual void accept()
	{
		this.accepted.Value = true;
	}

	/// <summary>Handle a building type existing in the save. This is called for each constructed building type which exists in the save.</summary>
	/// <param name="buildingType">The building type.</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed).</returns>
	public virtual bool OnBuildingExists(string buildingType, bool probe = false)
	{
		return false;
	}

	/// <summary>Handle the local player catching a fish.</summary>
	/// <param name="fishId">The qualified item ID of the caught fish.</param>
	/// <param name="numberCaught">The number of fish caught.</param>
	/// <param name="size">The fish size in inches.</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed).</returns>
	public virtual bool OnFishCaught(string fishId, int numberCaught, int size, bool probe = false)
	{
		return false;
	}

	/// <summary>Handle the local player catching a fish.</summary>
	/// <param name="item">The item that was received.</param>
	/// <param name="numberAdded">The number of items added to the player's inventory.</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed).</returns>
	public virtual bool OnItemReceived(Item item, int numberAdded, bool probe = false)
	{
		return false;
	}

	/// <summary>Handle the local player slaying a monster.</summary>
	/// <param name="location">The location containing the monster.</param>
	/// <param name="monster">The monster that was slain.</param>
	/// <param name="killedByBomb">Whether the monster was killed by a bomb placed by the player.</param>
	/// <param name="isTameMonster">Whether the slain monster was tame (e.g. a slime in a slime hutch).</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed).</returns>
	public virtual bool OnMonsterSlain(GameLocation location, Monster monster, bool killedByBomb, bool isTameMonster, bool probe = false)
	{
		return false;
	}

	/// <summary>Handle the local player talking to an NPC.</summary>
	/// <param name="npc">The NPC they talked to.</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed).</returns>
	public virtual bool OnNpcSocialized(NPC npc, bool probe = false)
	{
		return false;
	}

	/// <summary>Handle the local player crafting an item.</summary>
	/// <param name="recipe">The recipe that was crafted.</param>
	/// <param name="item">The item produced by the recipe.</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed).</returns>
	public virtual bool OnRecipeCrafted(CraftingRecipe recipe, Item item, bool probe = false)
	{
		return false;
	}

	/// <summary>Handle the local player arriving in a location.</summary>
	/// <param name="location">The recipe that was crafted.</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed).</returns>
	public virtual bool OnWarped(GameLocation location, bool probe = false)
	{
		return false;
	}

	/// <summary>Handle the local player offering an item to an NPC.</summary>
	/// <param name="npc">The NPC who would receive the item.</param>
	/// <param name="item">The item being offered.</param>
	/// <param name="probe">Whether to return whether the quest would advance, without actually changing the quest state.</param>
	/// <remarks>The item isn't deducted automatically from the player's inventory. If the quest should consume the item, it can do so within this method (e.g. via <see cref="M:StardewValley.Inventories.IInventory.Reduce(StardewValley.Item,System.Int32,System.Boolean)" />).</remarks>
	/// <returns>Returns whether the quest state changed (e.g. closer to completion or completed). Returning true prevents further processing (e.g. gifting the item to the NPC).</returns>
	public virtual bool OnItemOfferedToNpc(NPC npc, Item item, bool probe = false)
	{
		return false;
	}

	public bool hasReward()
	{
		if (this.moneyReward.Value <= 0)
		{
			string value = this.rewardDescription.Value;
			if (value == null)
			{
				return false;
			}
			return value.Length > 2;
		}
		return true;
	}

	public virtual bool isSecretQuest()
	{
		return false;
	}

	public virtual void questComplete()
	{
		if (this.completed.Value)
		{
			return;
		}
		if (this.dailyQuest.Value)
		{
			Game1.stats.Increment("BillboardQuestsDone");
			if (!Game1.player.mailReceived.Contains("completedFirstBillboardQuest"))
			{
				Game1.player.mailReceived.Add("completedFirstBillboardQuest");
			}
			if (Game1.stats.Get("BillboardQuestsDone") % 3 == 0)
			{
				if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)PrizeTicket")))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)PrizeTicket"), Game1.player.getStandingPosition(), 2);
				}
				if (Game1.stats.Get("BillboardQuestsDone") >= 6 && !Game1.player.mailReceived.Contains("gotFirstBillboardPrizeTicket"))
				{
					Game1.player.mailReceived.Add("gotFirstBillboardPrizeTicket");
				}
			}
		}
		if (this.dailyQuest.Value || this.questType.Value == 7)
		{
			Game1.stats.QuestsCompleted++;
		}
		this.completed.Value = true;
		Game1.player.currentLocation?.customQuestCompleteBehavior(this.id.Value);
		if (this.nextQuests.Count > 0)
		{
			foreach (string i in this.nextQuests)
			{
				if (this.IsValidId(i))
				{
					Game1.player.addQuest(i);
				}
			}
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
		}
		if (this.moneyReward.Value <= 0 && (this.rewardDescription.Value == null || this.rewardDescription.Value.Length <= 2))
		{
			Game1.player.questLog.Remove(this);
		}
		else
		{
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
		}
		Game1.playSound("questcomplete");
		if (this.id.Value == "126")
		{
			Game1.player.mailReceived.Add("emilyFiber");
			Game1.player.activeDialogueEvents["emilyFiber"] = 2;
		}
		Game1.dayTimeMoneyBox.questsDirty = true;
		Game1.player.autoGenerateActiveDialogueEvent("questComplete_" + this.id.Value);
	}

	public string GetName()
	{
		return this.questTitle;
	}

	public string GetDescription()
	{
		return this.questDescription;
	}

	public bool IsHidden()
	{
		return this.isSecretQuest();
	}

	public List<string> GetObjectiveDescriptions()
	{
		return new List<string> { this.currentObjective };
	}

	public bool CanBeCancelled()
	{
		return this.canBeCancelled.Value;
	}

	public bool HasReward()
	{
		if (!this.HasMoneyReward())
		{
			string value = this.rewardDescription.Value;
			if (value == null)
			{
				return false;
			}
			return value.Length > 2;
		}
		return true;
	}

	public bool HasMoneyReward()
	{
		if (this.completed.Value)
		{
			return this.moneyReward.Value > 0;
		}
		return false;
	}

	public void MarkAsViewed()
	{
		this.showNew.Value = false;
	}

	public bool ShouldDisplayAsNew()
	{
		return this.showNew.Value;
	}

	public bool ShouldDisplayAsComplete()
	{
		if (this.completed.Value)
		{
			return !this.IsHidden();
		}
		return false;
	}

	public bool IsTimedQuest()
	{
		if (!this.dailyQuest.Value)
		{
			return this.GetDaysLeft() > 0;
		}
		return true;
	}

	public int GetDaysLeft()
	{
		return this.daysLeft.Value;
	}

	public int GetMoneyReward()
	{
		return this.moneyReward.Value;
	}

	public void OnMoneyRewardClaimed()
	{
		this.moneyReward.Value = 0;
		this.destroy.Value = true;
	}

	public bool OnLeaveQuestPage()
	{
		if (this.completed.Value && this.moneyReward.Value <= 0)
		{
			this.destroy.Value = true;
		}
		if (this.destroy.Value)
		{
			Game1.player.questLog.Remove(this);
			return true;
		}
		return false;
	}

	/// <summary>Get whether the <see cref="F:StardewValley.Quests.Quest.id" /> is set to a valid value.</summary>
	protected bool HasId()
	{
		return this.IsValidId(this.id.Value);
	}

	/// <summary>Get whether the given quest ID is valid.</summary>
	/// <param name="id">The quest ID to check.</param>
	protected bool IsValidId(string id)
	{
		switch (id)
		{
		case "7":
			return Game1.GetFarmTypeID() != "MeadowlandsFarm";
		case null:
		case "-1":
		case "0":
			return false;
		default:
			return true;
		}
	}

	/// <summary>Create an RNG instance intended to initialize the quest fields.</summary>
	protected Random CreateInitializationRandom()
	{
		return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
	}

	/// <summary>Get the split quest conditions from raw quest fields, if it's found and valid.</summary>
	/// <param name="questFields">The raw quest fields.</param>
	/// <param name="conditions">The parsed conditions.</param>
	/// <param name="error">The error message indicating why parsing failed.</param>
	/// <param name="allowBlank">Whether to match the argument even if it's null or whitespace. If false, it will be treated as invalid in that case.</param>
	/// <returns>Returns whether the conditions field was found and valid.</returns>
	protected static bool TryParseConditions(string[] questFields, out string[] conditions, out string error, bool allowBlank = false)
	{
		if (!ArgUtility.TryGet(questFields, 4, out var rawConditions, out error, allowBlank, "string rawConditions"))
		{
			conditions = null;
			return false;
		}
		conditions = ArgUtility.SplitBySpace(rawConditions);
		error = null;
		return true;
	}

	/// <summary>Log an error message indicating that the quest data couldn't be parsed.</summary>
	/// <param name="id">The quest ID being parsed.</param>
	/// <param name="error">The error message indicating why parsing failed.</param>
	/// <returns>Returns a null quest for convenience.</returns>
	protected static Quest LogParseError(string id, string error)
	{
		Game1.log.Error("Failed to parse data for quest '" + id + "': " + error);
		return null;
	}

	/// <summary>Log an error message indicating that the conditions field in the quest data couldn't be parsed.</summary>
	/// <param name="id">The quest ID being parsed.</param>
	/// <param name="error">The error message indicating why parsing failed.</param>
	/// <returns>Returns a null quest for convenience.</returns>
	protected static Quest LogConditionsParseError(string id, string error)
	{
		Game1.log.Error("Failed to parse for quest '" + id + "': conditions field (index 4) is invalid: " + error);
		return null;
	}
}
