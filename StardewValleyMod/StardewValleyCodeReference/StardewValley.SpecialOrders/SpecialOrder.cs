using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;
using Netcode.Validation;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.SpecialOrders;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Quests;
using StardewValley.SpecialOrders.Objectives;
using StardewValley.SpecialOrders.Rewards;
using StardewValley.TokenizableStrings;

namespace StardewValley.SpecialOrders;

[XmlInclude(typeof(OrderObjective))]
[XmlInclude(typeof(OrderReward))]
[NotImplicitNetField]
public class SpecialOrder : INetObject<NetFields>, IQuest
{
	[XmlIgnore]
	public Action<Farmer, Item, int> onItemShipped;

	[XmlIgnore]
	public Action<Farmer, Monster> onMonsterSlain;

	[XmlIgnore]
	public Action<Farmer, Item> onFishCaught;

	[XmlIgnore]
	public Action<Farmer, NPC, Item> onGiftGiven;

	[XmlIgnore]
	public Func<Farmer, NPC, Item, bool, int> onItemDelivered;

	[XmlIgnore]
	public Action<Farmer, Item> onItemCollected;

	[XmlIgnore]
	public Action<Farmer, int> onMineFloorReached;

	[XmlIgnore]
	public Action<Farmer, int> onJKScoreAchieved;

	[XmlIgnore]
	protected bool _objectiveRegistrationDirty;

	[XmlElement("preSelectedItems")]
	public NetStringDictionary<string, NetString> preSelectedItems = new NetStringDictionary<string, NetString>();

	[XmlElement("selectedRandomElements")]
	public NetStringDictionary<int, NetInt> selectedRandomElements = new NetStringDictionary<int, NetInt>();

	[XmlElement("objectives")]
	public NetList<OrderObjective, NetRef<OrderObjective>> objectives = new NetList<OrderObjective, NetRef<OrderObjective>>();

	[XmlElement("generationSeed")]
	public NetInt generationSeed = new NetInt();

	[XmlElement("seenParticipantsIDs")]
	public NetLongDictionary<bool, NetBool> seenParticipants = new NetLongDictionary<bool, NetBool>();

	[XmlElement("participantsIDs")]
	public NetLongDictionary<bool, NetBool> participants = new NetLongDictionary<bool, NetBool>();

	[XmlElement("unclaimedRewardsIDs")]
	public NetLongDictionary<bool, NetBool> unclaimedRewards = new NetLongDictionary<bool, NetBool>();

	[XmlElement("donatedItems")]
	public readonly NetCollection<Item> donatedItems = new NetCollection<Item>();

	[XmlElement("appliedSpecialRules")]
	public bool appliedSpecialRules;

	[XmlIgnore]
	public readonly NetMutex donateMutex = new NetMutex();

	[XmlIgnore]
	protected int _isIslandOrder = -1;

	[XmlElement("rewards")]
	public NetList<OrderReward, NetRef<OrderReward>> rewards = new NetList<OrderReward, NetRef<OrderReward>>();

	[XmlIgnore]
	protected int _moneyReward = -1;

	[XmlElement("questKey")]
	public NetString questKey = new NetString();

	[XmlElement("questName")]
	public NetString questName = new NetString("Strings\\SpecialOrders:PlaceholderName");

	[XmlElement("questDescription")]
	public NetString questDescription = new NetString("Strings\\SpecialOrders:PlaceholderDescription");

	[XmlElement("requester")]
	public NetString requester = new NetString();

	[XmlElement("orderType")]
	public NetString orderType = new NetString("");

	[XmlElement("specialRule")]
	public NetString specialRule = new NetString("");

	[XmlElement("readyForRemoval")]
	public NetBool readyForRemoval = new NetBool(value: false);

	[XmlElement("itemToRemoveOnEnd")]
	public NetString itemToRemoveOnEnd = new NetString();

	[XmlElement("mailToRemoveOnEnd")]
	public NetString mailToRemoveOnEnd = new NetString();

	[XmlIgnore]
	protected string _localizedName;

	[XmlIgnore]
	protected string _localizedDescription;

	[XmlElement("dueDate")]
	public NetInt dueDate = new NetInt();

	[XmlElement("duration")]
	public NetEnum<QuestDuration> questDuration = new NetEnum<QuestDuration>();

	[XmlIgnore]
	protected List<OrderObjective> _registeredObjectives = new List<OrderObjective>();

	[XmlIgnore]
	protected Dictionary<Item, bool> _highlightLookup;

	[XmlIgnore]
	protected SpecialOrderData _orderData;

	[XmlElement("questState")]
	public NetEnum<SpecialOrderStatus> questState = new NetEnum<SpecialOrderStatus>(SpecialOrderStatus.InProgress);

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("SpecialOrder");

	public SpecialOrder()
	{
		this.InitializeNetFields();
	}

	public virtual void SetDuration(QuestDuration duration)
	{
		this.questDuration.Value = duration;
		WorldDate date = new WorldDate();
		switch (duration)
		{
		case QuestDuration.Week:
			date = new WorldDate(Game1.year, Game1.season, (Game1.dayOfMonth - 1) / 7 * 7);
			date.TotalDays++;
			date.TotalDays += 7;
			break;
		case QuestDuration.TwoWeeks:
			date = new WorldDate(Game1.year, Game1.season, (Game1.dayOfMonth - 1) / 7 * 7);
			date.TotalDays++;
			date.TotalDays += 14;
			break;
		case QuestDuration.Month:
			date = new WorldDate(Game1.year, Game1.season, 0);
			date.TotalDays++;
			date.TotalDays += 28;
			break;
		case QuestDuration.OneDay:
			date = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
			date.TotalDays++;
			break;
		case QuestDuration.TwoDays:
			date = WorldDate.Now();
			date.TotalDays += 2;
			break;
		case QuestDuration.ThreeDays:
			date = WorldDate.Now();
			date.TotalDays += 3;
			break;
		}
		this.dueDate.Value = date.TotalDays;
	}

	public virtual void OnFail()
	{
		foreach (OrderObjective objective in this.objectives)
		{
			objective.OnFail();
		}
		for (int i = 0; i < this.donatedItems.Count; i++)
		{
			Item item = this.donatedItems[i];
			this.donatedItems[i] = null;
			if (item != null)
			{
				Game1.player.team.returnedDonations.Add(item);
				Game1.player.team.newLostAndFoundItems.Value = true;
			}
		}
		if (Game1.IsMasterGame)
		{
			this.HostHandleQuestEnd();
		}
		this.questState.Value = SpecialOrderStatus.Failed;
		this._RemoveSpecialRuleIfNecessary();
	}

	public virtual int GetCompleteObjectivesCount()
	{
		int count = 0;
		foreach (OrderObjective objective in this.objectives)
		{
			if (objective.IsComplete())
			{
				count++;
			}
		}
		return count;
	}

	public virtual void ConfirmCompleteDonations()
	{
		foreach (OrderObjective objective in this.objectives)
		{
			if (objective is DonateObjective donateObjective)
			{
				donateObjective.Confirm();
			}
		}
	}

	public virtual void UpdateDonationCounts()
	{
		this._highlightLookup = null;
		int old_completed_objectives_count = 0;
		int new_completed_objectives_count = 0;
		foreach (OrderObjective objective in this.objectives)
		{
			if (!(objective is DonateObjective donate_objective))
			{
				continue;
			}
			int count = 0;
			if (donate_objective.GetCount() >= donate_objective.GetMaxCount())
			{
				old_completed_objectives_count++;
			}
			foreach (Item item in this.donatedItems)
			{
				if (donate_objective.IsValidItem(item))
				{
					count += item.Stack;
				}
			}
			donate_objective.SetCount(count);
			if (donate_objective.GetCount() >= donate_objective.GetMaxCount())
			{
				new_completed_objectives_count++;
			}
		}
		if (new_completed_objectives_count > old_completed_objectives_count)
		{
			Game1.playSound("newArtifact");
		}
	}

	public bool HighlightAcceptableItems(Item item)
	{
		if (this._highlightLookup != null && this._highlightLookup.TryGetValue(item, out var acceptable))
		{
			return acceptable;
		}
		if (this._highlightLookup == null)
		{
			this._highlightLookup = new Dictionary<Item, bool>();
		}
		foreach (OrderObjective objective in this.objectives)
		{
			if (objective is DonateObjective donate_objective && donate_objective.GetAcceptCount(item, 1) > 0)
			{
				this._highlightLookup[item] = true;
				return true;
			}
		}
		this._highlightLookup[item] = false;
		return false;
	}

	public virtual int GetAcceptCount(Item item)
	{
		int total_accepted_count = 0;
		int total_stacks = item.Stack;
		foreach (OrderObjective objective in this.objectives)
		{
			if (objective is DonateObjective donate_objective)
			{
				int accepted_count = donate_objective.GetAcceptCount(item, total_stacks);
				total_stacks -= accepted_count;
				total_accepted_count += accepted_count;
			}
		}
		return total_accepted_count;
	}

	public static bool CheckTags(string tag_list)
	{
		if (tag_list == null)
		{
			return true;
		}
		string[] tags = tag_list.Split(',');
		for (int i = 0; i < tags.Length; i++)
		{
			tags[i] = tags[i].Trim();
		}
		string[] array = tags;
		for (int j = 0; j < array.Length; j++)
		{
			string current_tag = array[j];
			if (current_tag.Length != 0)
			{
				bool match = true;
				if (current_tag.StartsWith('!'))
				{
					match = false;
					current_tag = current_tag.Substring(1);
				}
				if (SpecialOrder.CheckTag(current_tag) != match)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool CheckTag(string tag)
	{
		if (tag == "NOT_IMPLEMENTED")
		{
			return false;
		}
		if (tag.StartsWith("dropbox_"))
		{
			string value = tag.Substring("dropbox_".Length);
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				if (specialOrder.UsesDropBox(value))
				{
					return true;
				}
			}
		}
		if (tag.StartsWith("rule_"))
		{
			string value2 = tag.Substring("rule_".Length);
			if (Game1.player.team.SpecialOrderRuleActive(value2))
			{
				return true;
			}
		}
		if (tag.StartsWith("completed_"))
		{
			string value3 = tag.Substring("completed_".Length);
			if (Game1.player.team.completedSpecialOrders.Contains(value3))
			{
				return true;
			}
		}
		if (tag.StartsWith("season_"))
		{
			string value4 = tag.Substring("season_".Length);
			if (Game1.currentSeason == value4)
			{
				return true;
			}
		}
		else if (tag.StartsWith("mail_"))
		{
			string value5 = tag.Substring("mail_".Length);
			if (Game1.MasterPlayer.hasOrWillReceiveMail(value5))
			{
				return true;
			}
		}
		else if (tag.StartsWith("event_"))
		{
			string value6 = tag.Substring("event_".Length);
			if (Game1.MasterPlayer.eventsSeen.Contains(value6))
			{
				return true;
			}
		}
		else
		{
			if (tag == "island")
			{
				if (Utility.doesAnyFarmerHaveOrWillReceiveMail("seenBoatJourney"))
				{
					return true;
				}
				return false;
			}
			if (tag.StartsWith("knows_"))
			{
				string value7 = tag.Substring("knows_".Length);
				foreach (Farmer allFarmer in Game1.getAllFarmers())
				{
					if (allFarmer.friendshipData.ContainsKey(value7))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsIslandOrder()
	{
		if (this._isIslandOrder == -1 && DataLoader.SpecialOrders(Game1.content).TryGetValue(this.questKey.Value, out var data))
		{
			string requiredTags = data.RequiredTags;
			this._isIslandOrder = ((requiredTags != null && requiredTags.Contains("island")) ? (this._isIslandOrder = 1) : (this._isIslandOrder = 0));
		}
		return this._isIslandOrder == 1;
	}

	public static bool IsSpecialOrdersBoardUnlocked()
	{
		return Game1.stats.DaysPlayed >= 58;
	}

	public static void RemoveAllSpecialOrders(string orderType)
	{
		Game1.player.team.availableSpecialOrders.RemoveWhere((SpecialOrder order) => order.orderType.Value == orderType);
		Game1.player.team.acceptedSpecialOrderTypes.Remove(orderType);
	}

	public static void UpdateAvailableSpecialOrders(string orderType, bool forceRefresh)
	{
		foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
		{
			if ((order.questDuration.Value == QuestDuration.TwoDays || order.questDuration.Value == QuestDuration.ThreeDays) && !Game1.player.team.acceptedSpecialOrderTypes.Contains(order.orderType.Value))
			{
				order.SetDuration(order.questDuration.Value);
			}
		}
		if (!forceRefresh)
		{
			foreach (SpecialOrder availableSpecialOrder in Game1.player.team.availableSpecialOrders)
			{
				if (availableSpecialOrder.orderType.Value == orderType)
				{
					return;
				}
			}
		}
		SpecialOrder.RemoveAllSpecialOrders(orderType);
		List<string> keyQueue = new List<string>();
		foreach (KeyValuePair<string, SpecialOrderData> pair in DataLoader.SpecialOrders(Game1.content))
		{
			if (pair.Value.OrderType == orderType && SpecialOrder.CanStartOrderNow(pair.Key, pair.Value))
			{
				keyQueue.Add(pair.Key);
			}
		}
		List<string> keysIncludingCompleted = new List<string>(keyQueue);
		if (orderType == "")
		{
			keyQueue.RemoveAll((string id) => Game1.player.team.completedSpecialOrders.Contains(id));
		}
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.stats.DaysPlayed * 1.3);
		for (int i = 0; i < 2; i++)
		{
			if (keyQueue.Count == 0)
			{
				if (keysIncludingCompleted.Count == 0)
				{
					break;
				}
				keyQueue = new List<string>(keysIncludingCompleted);
			}
			string key = r.ChooseFrom(keyQueue);
			Game1.player.team.availableSpecialOrders.Add(SpecialOrder.GetSpecialOrder(key, r.Next()));
			keyQueue.Remove(key);
			keysIncludingCompleted.Remove(key);
		}
	}

	/// <summary>Get whether a special order is eligible to be started now by the player.</summary>
	/// <param name="orderId">The order ID in <c>Data/SpecialOrders</c>.</param>
	/// <param name="order">The special order data.</param>
	public static bool CanStartOrderNow(string orderId, SpecialOrderData order)
	{
		if (!order.Repeatable && Game1.MasterPlayer.team.completedSpecialOrders.Contains(orderId))
		{
			return false;
		}
		if (Game1.dayOfMonth >= 16 && order.Duration == QuestDuration.Month)
		{
			return false;
		}
		if (!SpecialOrder.CheckTags(order.RequiredTags))
		{
			return false;
		}
		if (!GameStateQuery.CheckConditions(order.Condition))
		{
			return false;
		}
		foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
		{
			if (specialOrder.questKey.Value == orderId)
			{
				return false;
			}
		}
		return true;
	}

	public static SpecialOrder GetSpecialOrder(string key, int? generation_seed)
	{
		try
		{
			if (!generation_seed.HasValue)
			{
				generation_seed = Game1.random.Next();
			}
			if (DataLoader.SpecialOrders(Game1.content).TryGetValue(key, out var data))
			{
				Random r = Utility.CreateRandom(generation_seed.Value);
				SpecialOrder order = new SpecialOrder();
				order.generationSeed.Value = generation_seed.Value;
				order._orderData = data;
				order.questKey.Value = key;
				order.questName.Value = data.Name;
				order.requester.Value = data.Requester;
				order.orderType.Value = data.OrderType.Trim();
				order.specialRule.Value = data.SpecialRule.Trim();
				if (data.ItemToRemoveOnEnd != null)
				{
					order.itemToRemoveOnEnd.Value = data.ItemToRemoveOnEnd;
				}
				if (data.MailToRemoveOnEnd != null)
				{
					order.mailToRemoveOnEnd.Value = data.MailToRemoveOnEnd;
				}
				order.selectedRandomElements.Clear();
				if (data.RandomizedElements != null)
				{
					foreach (RandomizedElement randomized_element in data.RandomizedElements)
					{
						List<int> valid_indices = new List<int>();
						for (int i = 0; i < randomized_element.Values.Count; i++)
						{
							if (SpecialOrder.CheckTags(randomized_element.Values[i].RequiredTags))
							{
								valid_indices.Add(i);
							}
						}
						int selected_index = r.ChooseFrom(valid_indices);
						order.selectedRandomElements[randomized_element.Name] = selected_index;
						string value = randomized_element.Values[selected_index].Value;
						if (!value.StartsWith("PICK_ITEM"))
						{
							continue;
						}
						value = value.Substring("PICK_ITEM".Length);
						string[] array = value.Split(',');
						List<string> valid_item_ids = new List<string>();
						string[] array2 = array;
						for (int j = 0; j < array2.Length; j++)
						{
							string valid_item_name = array2[j].Trim();
							if (valid_item_name.Length != 0)
							{
								ParsedItemData parsedData = ItemRegistry.GetData(valid_item_name);
								if (parsedData != null)
								{
									valid_item_ids.Add(parsedData.QualifiedItemId);
									continue;
								}
								Item item = Utility.fuzzyItemSearch(valid_item_name);
								valid_item_ids.Add(item.QualifiedItemId);
							}
						}
						order.preSelectedItems[randomized_element.Name] = r.ChooseFrom(valid_item_ids);
					}
				}
				order.SetDuration(data.Duration);
				order.questDescription.Value = data.Text;
				string objectivesNamespace = typeof(OrderObjective).Namespace;
				string rewardsNamespace = typeof(OrderReward).Namespace;
				foreach (SpecialOrderObjectiveData objective_data in data.Objectives)
				{
					Type objective_type = Type.GetType(objectivesNamespace + "." + objective_data.Type.Trim() + "Objective");
					if (!(objective_type == null) && objective_type.IsSubclassOf(typeof(OrderObjective)))
					{
						OrderObjective objective = (OrderObjective)Activator.CreateInstance(objective_type);
						if (objective != null)
						{
							objective.description.Value = objective_data.Text;
							objective.maxCount.Value = int.Parse(order.Parse(objective_data.RequiredCount));
							objective.Load(order, objective_data.Data);
							order.objectives.Add(objective);
						}
					}
				}
				foreach (SpecialOrderRewardData reward_data in data.Rewards)
				{
					Type reward_type = Type.GetType(rewardsNamespace + "." + reward_data.Type.Trim() + "Reward");
					if (!(reward_type == null) && reward_type.IsSubclassOf(typeof(OrderReward)))
					{
						OrderReward reward = (OrderReward)Activator.CreateInstance(reward_type);
						if (reward != null)
						{
							reward.Load(order, reward_data.Data);
							order.rewards.Add(reward);
						}
					}
				}
				return order;
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error("Failed loading special order '" + key + "'.", exception);
		}
		return null;
	}

	public static string MakeLocalizationReplacements(string data)
	{
		data = data.Trim();
		int open_index;
		do
		{
			open_index = data.LastIndexOf('[');
			if (open_index >= 0)
			{
				int close_index = data.IndexOf(']', open_index);
				if (close_index == -1)
				{
					return data;
				}
				string inner = data.Substring(open_index + 1, close_index - open_index - 1);
				string value = Game1.content.LoadString("Strings\\SpecialOrderStrings:" + inner);
				data = data.Remove(open_index, close_index - open_index + 1);
				data = data.Insert(open_index, value);
			}
		}
		while (open_index >= 0);
		return data;
	}

	public virtual string Parse(string data)
	{
		data = data.Trim();
		this.GetData();
		data = SpecialOrder.MakeLocalizationReplacements(data);
		int open_index;
		do
		{
			open_index = data.LastIndexOf('{');
			if (open_index < 0)
			{
				continue;
			}
			int close_index = data.IndexOf('}', open_index);
			if (close_index == -1)
			{
				return data;
			}
			string inner = data.Substring(open_index + 1, close_index - open_index - 1);
			string value = inner;
			string key = inner;
			string subkey = null;
			if (inner.Contains(':'))
			{
				string[] split = inner.Split(':');
				key = split[0];
				if (split.Length > 1)
				{
					subkey = split[1];
				}
			}
			if (this._orderData.RandomizedElements != null)
			{
				int index;
				if (this.preSelectedItems.TryGetValue(key, out var itemId))
				{
					Item requested_item = ItemRegistry.Create(itemId);
					switch (subkey)
					{
					case "Text":
						value = requested_item.DisplayName;
						break;
					case "TextPlural":
						value = Lexicon.makePlural(requested_item.DisplayName);
						break;
					case "TextPluralCapitalized":
						value = Utility.capitalizeFirstLetter(Lexicon.makePlural(requested_item.DisplayName));
						break;
					case "Tags":
					{
						string alternate_id = "id_" + Utility.getStandardDescriptionFromItem(requested_item, 0, '_');
						alternate_id = alternate_id.Substring(0, alternate_id.Length - 2).ToLower();
						value = alternate_id;
						break;
					}
					case "Price":
						value = ((requested_item is Object obj) ? (obj.sellToStorePrice(-1L).ToString() ?? "") : "1");
						break;
					}
				}
				else if (this.selectedRandomElements.TryGetValue(key, out index))
				{
					foreach (RandomizedElement randomized_element in this._orderData.RandomizedElements)
					{
						if (randomized_element.Name == key)
						{
							value = SpecialOrder.MakeLocalizationReplacements(randomized_element.Values[index].Value);
							break;
						}
					}
				}
			}
			if (subkey != null)
			{
				string[] split2 = value.Split('|');
				for (int i = 0; i < split2.Length; i += 2)
				{
					if (i + 1 <= split2.Length && split2[i] == subkey)
					{
						value = split2[i + 1];
						break;
					}
				}
			}
			data = data.Remove(open_index, close_index - open_index + 1);
			data = data.Insert(open_index, value);
		}
		while (open_index >= 0);
		return data;
	}

	/// <summary>Get the special order's data from <c>Data/SpecialOrders</c>, if found.</summary>
	public virtual SpecialOrderData GetData()
	{
		if (this._orderData == null)
		{
			SpecialOrder.TryGetData(this.questKey.Value, out this._orderData);
		}
		return this._orderData;
	}

	/// <summary>Try to get a special order's data from <c>Data/SpecialOrders</c>.</summary>
	/// <param name="id">The special order ID (i.e. the key in <c>Data/SpecialOrders</c>).</param>
	/// <param name="data">The special order data, if found.</param>
	/// <returns>Returns whether the special order data was found.</returns>
	public static bool TryGetData(string id, out SpecialOrderData data)
	{
		if (id == null)
		{
			data = null;
			return false;
		}
		return DataLoader.SpecialOrders(Game1.content).TryGetValue(id, out data);
	}

	public virtual void InitializeNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.questName, "questName").AddField(this.questDescription, "questDescription")
			.AddField(this.dueDate, "dueDate")
			.AddField(this.objectives, "objectives")
			.AddField(this.rewards, "rewards")
			.AddField(this.questState, "questState")
			.AddField(this.donatedItems, "donatedItems")
			.AddField(this.questKey, "questKey")
			.AddField(this.requester, "requester")
			.AddField(this.generationSeed, "generationSeed")
			.AddField(this.selectedRandomElements, "selectedRandomElements")
			.AddField(this.preSelectedItems, "preSelectedItems")
			.AddField(this.orderType, "orderType")
			.AddField(this.specialRule, "specialRule")
			.AddField(this.participants, "participants")
			.AddField(this.seenParticipants, "seenParticipants")
			.AddField(this.unclaimedRewards, "unclaimedRewards")
			.AddField(this.donateMutex.NetFields, "donateMutex.NetFields")
			.AddField(this.itemToRemoveOnEnd, "itemToRemoveOnEnd")
			.AddField(this.mailToRemoveOnEnd, "mailToRemoveOnEnd")
			.AddField(this.questDuration, "questDuration")
			.AddField(this.readyForRemoval, "readyForRemoval");
		this.objectives.OnArrayReplaced += delegate
		{
			this._objectiveRegistrationDirty = true;
		};
		this.objectives.OnElementChanged += delegate
		{
			this._objectiveRegistrationDirty = true;
		};
	}

	protected virtual void _UpdateObjectiveRegistration()
	{
		for (int i = 0; i < this._registeredObjectives.Count; i++)
		{
			OrderObjective objective = this._registeredObjectives[i];
			if (!this.objectives.Contains(objective))
			{
				objective.Unregister();
			}
		}
		foreach (OrderObjective objective2 in this.objectives)
		{
			if (!this._registeredObjectives.Contains(objective2))
			{
				objective2.Register(this);
				this._registeredObjectives.Add(objective2);
			}
		}
	}

	public bool UsesDropBox(string box_id)
	{
		if (this.questState.Value != SpecialOrderStatus.InProgress)
		{
			return false;
		}
		foreach (OrderObjective objective in this.objectives)
		{
			if (objective is DonateObjective donateObjective && donateObjective.dropBox.Value == box_id)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMinimumDropBoxCapacity(string box_id)
	{
		int minimum_capacity = 9;
		foreach (OrderObjective objective in this.objectives)
		{
			if (objective is DonateObjective donateObjective && donateObjective.dropBox.Value == box_id && donateObjective.minimumCapacity.Value > 0)
			{
				minimum_capacity = Math.Max(minimum_capacity, donateObjective.minimumCapacity.Value);
			}
		}
		return minimum_capacity;
	}

	public virtual void Update()
	{
		this._AddSpecialRulesIfNecessary();
		if (this._objectiveRegistrationDirty)
		{
			this._objectiveRegistrationDirty = false;
			this._UpdateObjectiveRegistration();
		}
		if (!this.readyForRemoval.Value)
		{
			switch (this.questState.Value)
			{
			case SpecialOrderStatus.InProgress:
				this.participants.TryAdd(Game1.player.UniqueMultiplayerID, value: true);
				break;
			case SpecialOrderStatus.Complete:
				if (this.unclaimedRewards.Remove(Game1.player.UniqueMultiplayerID))
				{
					Game1.stats.QuestsCompleted++;
					Game1.playSound("questcomplete");
					Game1.dayTimeMoneyBox.questsDirty = true;
					if (this.orderType.Value == "" && !this.questKey.Value.Contains("QiChallenge") && !this.questKey.Value.Contains("DesertFestival"))
					{
						Game1.player.stats.Increment("specialOrderPrizeTickets");
					}
					foreach (OrderReward reward in this.rewards)
					{
						reward.Grant();
					}
				}
				if (this.participants.ContainsKey(Game1.player.UniqueMultiplayerID) && this.GetMoneyReward() <= 0)
				{
					this.RemoveFromParticipants();
				}
				break;
			}
		}
		this.donateMutex.Update(Game1.getOnlineFarmers());
		if (this.donateMutex.IsLockHeld() && Game1.activeClickableMenu == null)
		{
			this.donateMutex.ReleaseLock();
		}
		if (Game1.activeClickableMenu == null)
		{
			this._highlightLookup = null;
		}
		if (Game1.IsMasterGame && this.questState.Value != SpecialOrderStatus.InProgress)
		{
			this.MarkForRemovalIfEmpty();
			if (this.readyForRemoval.Value)
			{
				this._RemoveSpecialRuleIfNecessary();
				Game1.player.team.specialOrders.Remove(this);
			}
		}
	}

	public virtual void RemoveFromParticipants()
	{
		this.participants.Remove(Game1.player.UniqueMultiplayerID);
		this.MarkForRemovalIfEmpty();
	}

	public virtual void MarkForRemovalIfEmpty()
	{
		if (this.participants.Length == 0)
		{
			this.readyForRemoval.Value = true;
		}
	}

	public virtual void HostHandleQuestEnd()
	{
		if (Game1.IsMasterGame)
		{
			if (this.itemToRemoveOnEnd.Value != null && !Game1.player.team.itemsToRemoveOvernight.Contains(this.itemToRemoveOnEnd.Value))
			{
				Game1.player.team.itemsToRemoveOvernight.Add(this.itemToRemoveOnEnd.Value);
			}
			if (this.mailToRemoveOnEnd.Value != null && !Game1.player.team.mailToRemoveOvernight.Contains(this.mailToRemoveOnEnd.Value))
			{
				Game1.player.team.mailToRemoveOvernight.Add(this.mailToRemoveOnEnd.Value);
			}
		}
	}

	protected void _AddSpecialRulesIfNecessary()
	{
		if (!Game1.IsMasterGame || this.appliedSpecialRules || this.questState.Value != SpecialOrderStatus.InProgress)
		{
			return;
		}
		this.appliedSpecialRules = true;
		string[] array = this.specialRule.Value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string formatted_rule = array[i].Trim();
			if (!Game1.player.team.SpecialOrderRuleActive(formatted_rule, this))
			{
				this.AddSpecialRule(formatted_rule);
				if (Game1.player.team.specialRulesRemovedToday.Contains(formatted_rule))
				{
					Game1.player.team.specialRulesRemovedToday.Remove(formatted_rule);
				}
			}
		}
	}

	protected void _RemoveSpecialRuleIfNecessary()
	{
		if (!Game1.IsMasterGame || !this.appliedSpecialRules)
		{
			return;
		}
		this.appliedSpecialRules = false;
		string[] array = this.specialRule.Value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string formatted_rule = array[i].Trim();
			if (!Game1.player.team.SpecialOrderRuleActive(formatted_rule, this))
			{
				this.RemoveSpecialRule(formatted_rule);
				if (!Game1.player.team.specialRulesRemovedToday.Contains(formatted_rule))
				{
					Game1.player.team.specialRulesRemovedToday.Add(formatted_rule);
				}
			}
		}
	}

	public virtual void AddSpecialRule(string rule)
	{
		if (!(rule == "MINE_HARD"))
		{
			if (rule == "SC_HARD")
			{
				Game1.netWorldState.Value.SkullCavesDifficulty++;
				Game1.player.team.kickOutOfMinesEvent.Fire(121);
			}
		}
		else
		{
			Game1.netWorldState.Value.MinesDifficulty++;
			Game1.player.team.kickOutOfMinesEvent.Fire(120);
			Game1.netWorldState.Value.LowestMineLevelForOrder = 0;
		}
	}

	public static void RemoveSpecialRuleAtEndOfDay(string rule)
	{
		switch (rule)
		{
		case "MINE_HARD":
			if (Game1.netWorldState.Value.MinesDifficulty > 0)
			{
				Game1.netWorldState.Value.MinesDifficulty--;
			}
			Game1.netWorldState.Value.LowestMineLevelForOrder = -1;
			break;
		case "SC_HARD":
			if (Game1.netWorldState.Value.SkullCavesDifficulty > 0)
			{
				Game1.netWorldState.Value.SkullCavesDifficulty--;
			}
			break;
		case "QI_COOKING":
			Utility.ForEachItem(delegate(Item item)
			{
				if (item is Object obj && obj.orderData.Value == "QI_COOKING")
				{
					obj.orderData.Value = null;
					obj.MarkContextTagsDirty();
				}
				return true;
			});
			break;
		}
	}

	public virtual void RemoveSpecialRule(string rule)
	{
		if (rule == "QI_BEANS")
		{
			Game1.player.team.itemsToRemoveOvernight.Add("890");
			Game1.player.team.itemsToRemoveOvernight.Add("889");
		}
	}

	public virtual bool HasMoneyReward()
	{
		if (this.questState.Value == SpecialOrderStatus.Complete && this.GetMoneyReward() > 0)
		{
			return this.participants.ContainsKey(Game1.player.UniqueMultiplayerID);
		}
		return false;
	}

	public virtual void Fail()
	{
	}

	public virtual void AddObjective(OrderObjective objective)
	{
		this.objectives.Add(objective);
	}

	public void CheckCompletion()
	{
		if (this.questState.Value != SpecialOrderStatus.InProgress)
		{
			return;
		}
		foreach (OrderObjective objective in this.objectives)
		{
			if (objective.failOnCompletion.Value && objective.IsComplete())
			{
				this.OnFail();
				return;
			}
		}
		foreach (OrderObjective objective2 in this.objectives)
		{
			if (!objective2.failOnCompletion.Value && !objective2.IsComplete())
			{
				return;
			}
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		foreach (long farmer_id in this.participants.Keys)
		{
			this.unclaimedRewards.TryAdd(farmer_id, value: true);
		}
		Game1.multiplayer.globalChatInfoMessage("CompletedSpecialOrder", TokenStringBuilder.SpecialOrderName(this.questKey.Value));
		this.HostHandleQuestEnd();
		Game1.player.team.completedSpecialOrders.Add(this.questKey.Value);
		this.questState.Value = SpecialOrderStatus.Complete;
		this._RemoveSpecialRuleIfNecessary();
	}

	public override string ToString()
	{
		string temp = "";
		foreach (OrderObjective objective in this.objectives)
		{
			temp += objective.description.Value;
			if (objective.GetMaxCount() > 1)
			{
				temp = temp + " (" + objective.GetCount() + "/" + objective.GetMaxCount() + ")";
			}
			temp += "\n";
		}
		return temp.Trim();
	}

	public string GetName()
	{
		if (this._localizedName == null)
		{
			this._localizedName = SpecialOrder.MakeLocalizationReplacements(this.questName.Value);
		}
		return this._localizedName;
	}

	public string GetDescription()
	{
		if (this._localizedDescription == null)
		{
			this._localizedDescription = this.Parse(this.questDescription.Value).Trim();
		}
		return this._localizedDescription;
	}

	public List<string> GetObjectiveDescriptions()
	{
		List<string> objective_descriptions = new List<string>();
		foreach (OrderObjective objective in this.objectives)
		{
			objective_descriptions.Add(this.Parse(objective.GetDescription()));
		}
		return objective_descriptions;
	}

	public bool CanBeCancelled()
	{
		return false;
	}

	public void MarkAsViewed()
	{
		this.seenParticipants.TryAdd(Game1.player.UniqueMultiplayerID, value: true);
	}

	public bool IsHidden()
	{
		return !this.participants.ContainsKey(Game1.player.UniqueMultiplayerID);
	}

	public bool ShouldDisplayAsNew()
	{
		return !this.seenParticipants.ContainsKey(Game1.player.UniqueMultiplayerID);
	}

	public bool HasReward()
	{
		return this.HasMoneyReward();
	}

	public int GetMoneyReward()
	{
		if (this._moneyReward == -1)
		{
			this._moneyReward = 0;
			foreach (OrderReward reward in this.rewards)
			{
				if (reward is MoneyReward moneyReward)
				{
					this._moneyReward += moneyReward.GetRewardMoneyAmount();
				}
			}
		}
		return this._moneyReward;
	}

	public bool ShouldDisplayAsComplete()
	{
		return this.questState.Value != SpecialOrderStatus.InProgress;
	}

	public bool IsTimedQuest()
	{
		return true;
	}

	public int GetDaysLeft()
	{
		if (this.questState.Value != SpecialOrderStatus.InProgress)
		{
			return 0;
		}
		return this.dueDate.Value - Game1.Date.TotalDays;
	}

	public void OnMoneyRewardClaimed()
	{
		this.participants.Remove(Game1.player.UniqueMultiplayerID);
		this.MarkForRemovalIfEmpty();
	}

	public bool OnLeaveQuestPage()
	{
		if (!this.participants.ContainsKey(Game1.player.UniqueMultiplayerID))
		{
			this.MarkForRemovalIfEmpty();
			return true;
		}
		return false;
	}
}
