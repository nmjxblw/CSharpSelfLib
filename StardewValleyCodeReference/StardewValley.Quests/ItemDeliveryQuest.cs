using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Network;

namespace StardewValley.Quests;

public class ItemDeliveryQuest : Quest
{
	/// <summary>The translated NPC dialogue shown when the quest is completed.</summary>
	public string targetMessage;

	/// <summary>The internal name for the NPC who gave the quest.</summary>
	[XmlElement("target")]
	public readonly NetString target = new NetString();

	/// <summary>The qualified item ID that must be delivered.</summary>
	[XmlElement("item")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The number of items that must be delivered.</summary>
	[XmlElement("number")]
	public readonly NetInt number = new NetInt(1);

	/// <summary>The translatable text segments for the quest description shown in the quest log.</summary>
	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the <see cref="F:StardewValley.Quests.ItemDeliveryQuest.targetMessage" />.</summary>
	public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the objective shown in the quest log (like "0/5 caught").</summary>
	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Construct an instance.</summary>
	public ItemDeliveryQuest()
	{
		base.questType.Value = 3;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="target">The internal name for the NPC who gave the quest.</param>
	/// <param name="itemId">The qualified or unqualified item ID that must be delivered.</param>
	public ItemDeliveryQuest(string target, string itemId)
		: this()
	{
		this.target.Value = target;
		this.ItemId.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="target">The internal name for the NPC who gave the quest.</param>
	/// <param name="itemId">The qualified or unqualified item ID that must be delivered.</param>
	/// <param name="objective">The translatable text segments for the objective shown in the quest log (like "0/5 caught").</param>
	/// <param name="returnDialogue">The translated NPC dialogue shown when the quest is completed.</param>
	public ItemDeliveryQuest(string target, string itemId, string questTitle, string questDescription, string objective, string returnDialogue)
		: this(target, itemId)
	{
		base.questDescription = questDescription;
		base.questTitle = questTitle;
		base._loadedTitle = true;
		this.targetMessage = returnDialogue;
		this.objective = new NetDescriptionElementRef(new DescriptionElement(objective));
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.target, "target").AddField(this.ItemId, "ItemId").AddField(this.number, "number")
			.AddField(this.parts, "parts")
			.AddField(this.dialogueparts, "dialogueparts")
			.AddField(this.objective, "objective");
	}

	public List<NPC> GetValidTargetList()
	{
		Farmer[] source = Game1.getAllFarmers().ToArray();
		HashSet<string> friendshipKeys = new HashSet<string>(source.SelectMany((Farmer player) => player.friendshipData.Keys));
		HashSet<string> spouses = new HashSet<string>(source.Select((Farmer p) => p.spouse));
		List<NPC> validTargets = new List<NPC>();
		foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
		{
			CharacterData data = pair.Value;
			if (GameStateQuery.CheckConditions(data.CanSocialize) && ((data.ItemDeliveryQuests != null) ? GameStateQuery.CheckConditions(data.ItemDeliveryQuests) : (data.HomeRegion == "Town")) && friendshipKeys.Contains(pair.Key) && !spouses.Contains(pair.Key) && pair.Value.Age != NpcAge.Child)
			{
				NPC npc = Game1.getCharacterFromName(pair.Key);
				if (npc != null && !npc.IsInvisible)
				{
					validTargets.Add(npc);
				}
			}
		}
		return validTargets;
	}

	public void loadQuestInfo()
	{
		if (this.target.Value != null)
		{
			return;
		}
		Random random = base.CreateInitializationRandom();
		List<NPC> valid_targets = this.GetValidTargetList();
		NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = Game1.player.friendshipData;
		if (friendshipData == null || friendshipData.Length <= 0 || valid_targets.Count <= 0)
		{
			return;
		}
		NPC actualTarget = valid_targets[random.Next(valid_targets.Count)];
		if (actualTarget == null)
		{
			return;
		}
		this.target.Value = actualTarget.name.Value;
		if (this.target.Value.Equals("Wizard") && !Game1.player.mailReceived.Contains("wizardJunimoNote") && !Game1.player.mailReceived.Contains("JojaMember"))
		{
			this.target.Value = "Demetrius";
			actualTarget = Game1.getCharacterFromName(this.target.Value);
		}
		base.questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ItemDeliveryQuestTitle", NPC.GetDisplayName(this.target.Value));
		Item item;
		if (Game1.season != Season.Winter && random.NextDouble() < 0.15)
		{
			this.ItemId.Value = random.ChooseFrom(Utility.possibleCropsAtThisTime(Game1.season, Game1.dayOfMonth <= 7));
			this.ItemId.Value = ItemRegistry.QualifyItemId(this.ItemId.Value) ?? this.ItemId.Value;
			item = ItemRegistry.Create(this.ItemId.Value);
			if (base.dailyQuest.Value || base.moneyReward.Value == 0)
			{
				base.moneyReward.Value = this.GetGoldRewardPerItem(item);
			}
			switch (this.target.Value)
			{
			case "Demetrius":
				this.parts.Clear();
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13311", "13314"), item));
				break;
			case "Marnie":
				this.parts.Clear();
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13317", "13320"), item));
				break;
			case "Sebastian":
				this.parts.Clear();
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13324", "13327"), item));
				break;
			default:
				this.parts.Clear();
				this.parts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13299", "13300", "13301"));
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13302", "13303", "13304"), item));
				this.parts.Add(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13306", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13307", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13308"));
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
				break;
			}
		}
		else
		{
			string rawId = Utility.getRandomItemFromSeason(Game1.season, 1000, forQuest: true);
			if (!(rawId == "-5"))
			{
				if (rawId == "-6")
				{
					this.ItemId.Value = "(O)184";
				}
				else
				{
					this.ItemId.Value = ItemRegistry.QualifyItemId(rawId) ?? rawId;
				}
			}
			else
			{
				this.ItemId.Value = "(O)176";
			}
			item = ItemRegistry.Create(this.ItemId.Value);
			if (base.dailyQuest.Value || base.moneyReward.Value == 0)
			{
				base.moneyReward.Value = this.GetGoldRewardPerItem(item);
			}
			DescriptionElement[] questDescriptions = null;
			DescriptionElement[] questDescriptions2 = null;
			DescriptionElement[] questDescriptions3 = null;
			if ((item as Object)?.Type == "Cooking" && this.target.Value != "Wizard")
			{
				if (random.NextDouble() < 0.33)
				{
					DescriptionElement[] questStrings = new DescriptionElement[12]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341"),
						(!(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13349"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13350"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13351"),
						(Game1.season == Season.Winter) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13353") : ((Game1.season == Season.Summer) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13355") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13356")),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357")
					};
					this.parts.Clear();
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13333", "13334"), item, random.ChooseFrom(questStrings)));
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
				}
				else
				{
					DescriptionElement day = (Game1.dayOfMonth % 7) switch
					{
						0 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3042"), 
						1 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3043"), 
						2 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3044"), 
						3 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3045"), 
						4 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3046"), 
						5 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3047"), 
						_ => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3048"), 
					};
					questDescriptions = new DescriptionElement[5]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13360", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13364", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13367", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13370", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13373", day, item, actualTarget)
					};
					questDescriptions2 = new DescriptionElement[5]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("")
					};
					questDescriptions3 = new DescriptionElement[5]
					{
						new DescriptionElement(""),
						new DescriptionElement(""),
						new DescriptionElement(""),
						new DescriptionElement(""),
						new DescriptionElement("")
					};
				}
				this.parts.Clear();
				int rand = random.Next(questDescriptions.Length);
				this.parts.Add(questDescriptions[rand]);
				this.parts.Add(questDescriptions2[rand]);
				this.parts.Add(questDescriptions3[rand]);
				if (this.target.Value.Equals("Sebastian"))
				{
					this.parts.Clear();
					this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13378", "13381"), item));
				}
			}
			else
			{
				if (random.NextBool())
				{
					Object obj = item as Object;
					if (obj != null && obj.Edibility > 0)
					{
						questDescriptions = new DescriptionElement[1]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13383", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13385", "13386", "13387", "13388", "13389", "13390", "13391", "13392", "13393", "13394", "13395", "13396")), new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13400", item))
						};
						questDescriptions2 = new DescriptionElement[2]
						{
							new DescriptionElement(random.Choose("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13398")),
							new DescriptionElement(random.Choose("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13402"))
						};
						questDescriptions3 = new DescriptionElement[2]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget)
						};
						if (random.NextDouble() < 0.33)
						{
							DescriptionElement[] questStrings2 = new DescriptionElement[12]
							{
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341"),
								(!(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13420"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13421"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13422"),
								(Game1.season == Season.Winter) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13424") : ((Game1.season == Season.Summer) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13426") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13427")),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357")
							};
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13333", "13334"), item, random.ChooseFrom(questStrings2)));
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
						}
						else
						{
							this.parts.Clear();
							int rand2 = random.Next(questDescriptions.Length);
							this.parts.Add(questDescriptions[rand2]);
							this.parts.Add(questDescriptions2[rand2]);
							this.parts.Add(questDescriptions3[rand2]);
						}
						switch (this.target.Value)
						{
						case "Demetrius":
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13311", "13314"), item));
							break;
						case "Marnie":
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13317", "13320"), item));
							break;
						case "Harvey":
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13446", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13448", "13449", "13450", "13451", "13452", "13453", "13454", "13455", "13456", "13457", "13458", "13459"))));
							break;
						case "Gus":
							if (random.NextDouble() < 0.6)
							{
								this.parts.Clear();
								this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13462", item));
							}
							break;
						}
						goto IL_134b;
					}
				}
				if (random.NextBool())
				{
					Object obj2 = item as Object;
					if (obj2 == null || obj2.Edibility < 0)
					{
						this.parts.Clear();
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13464", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13465", "13466", "13467", "13468", "13469"))));
						this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
						if (this.target.Value.Equals("Emily"))
						{
							this.parts.Clear();
							this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13473", "13476"), item));
						}
						goto IL_134b;
					}
				}
				questDescriptions = new DescriptionElement[9]
				{
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13480", actualTarget, item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13481", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13485", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13491", "13492"), item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13497", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13500", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13502", "13503", "13504", "13505", "13506", "13507", "13508", "13509", "13510", "13511", "13512", "13513"))),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13518", actualTarget, item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13520", "13523"), item)
				};
				questDescriptions2 = new DescriptionElement[9]
				{
					new DescriptionElement(""),
					new DescriptionElement(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13482", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13483")),
					new DescriptionElement(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13487", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13488", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13489")),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13514", "13516")),
					new DescriptionElement(""),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget)
				};
				questDescriptions3 = new DescriptionElement[9]
				{
					new DescriptionElement(""),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement(""),
					new DescriptionElement(""),
					new DescriptionElement(""),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement(""),
					new DescriptionElement("")
				};
				this.parts.Clear();
				int rand3 = random.Next(questDescriptions.Length);
				this.parts.Add(questDescriptions[rand3]);
				this.parts.Add(questDescriptions2[rand3]);
				this.parts.Add(questDescriptions3[rand3]);
			}
		}
		goto IL_134b;
		IL_134b:
		this.dialogueparts.Clear();
		this.dialogueparts.Add((random.NextBool(0.3) || this.target.Value == "Evelyn") ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13526") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13527", "13528")));
		this.dialogueparts.Add(random.NextBool(0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13530", item) : (random.NextBool() ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13532") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13534", "13535", "13536"))));
		this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13538", "13539", "13540"));
		this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13542", "13543", "13544"));
		switch (this.target.Value)
		{
		case "Wizard":
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13546", "13548", "13551", "13553"), item));
			this.dialogueparts.Clear();
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13555");
			break;
		case "Haley":
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13557", "13560"), item));
			this.dialogueparts.Clear();
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13566");
			break;
		case "Sam":
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13568", "13571"), item));
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13577"));
			break;
		case "Maru":
		{
			bool rand5 = random.NextBool();
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand5 ? "13580" : "13583"), item));
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand5 ? "13585" : "13587")));
			break;
		}
		case "Abigail":
		{
			bool rand4 = random.NextBool();
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand4 ? "13590" : "13593"), item));
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand4 ? "13597" : "13599")));
			break;
		}
		case "Sebastian":
			this.dialogueparts.Clear();
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13602");
			break;
		case "Elliott":
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13604", item));
			break;
		}
		DescriptionElement lastPart = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13608", "13610", "13612"), actualTarget);
		this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", base.moneyReward.Value));
		this.parts.Add(lastPart);
		this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13614", actualTarget, item);
	}

	public override void reloadDescription()
	{
		if (base._questDescription == "")
		{
			this.loadQuestInfo();
		}
		string descriptionBuilder = "";
		string messageBuilder = "";
		if (this.parts != null && this.parts.Count != 0)
		{
			foreach (DescriptionElement a in this.parts)
			{
				descriptionBuilder += a.loadDescriptionElement();
			}
			base.questDescription = descriptionBuilder;
		}
		if (this.dialogueparts != null && this.dialogueparts.Count != 0)
		{
			foreach (DescriptionElement b in this.dialogueparts)
			{
				messageBuilder += b.loadDescriptionElement();
			}
			this.targetMessage = messageBuilder;
		}
		else if (base.HasId())
		{
			string[] fields = Quest.GetRawQuestFields(base.id.Value);
			this.targetMessage = ArgUtility.Get(fields, 9, this.targetMessage, allowBlank: false);
		}
	}

	public override void reloadObjective()
	{
		if (this.objective.Value != null)
		{
			base.currentObjective = this.objective.Value.loadDescriptionElement();
		}
	}

	/// <inheritdoc />
	public override bool OnItemOfferedToNpc(NPC npc, Item item, bool probe = false)
	{
		bool baseChanged = base.OnItemOfferedToNpc(npc, item, probe);
		if (base.completed.Value)
		{
			return false;
		}
		if (npc.IsVillager && npc.Name == this.target.Value && item.QualifiedItemId == this.ItemId.Value)
		{
			if (item.Stack >= this.number.Value)
			{
				if (!probe)
				{
					Game1.player.Items.Reduce(item, this.number.Value);
					this.reloadDescription();
					npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage));
					Game1.drawDialogue(npc);
					if (base.dailyQuest.Value)
					{
						Game1.player.changeFriendship(150, npc);
					}
					else
					{
						Game1.player.changeFriendship(255, npc);
					}
					this.questComplete();
				}
				return true;
			}
			if (!probe)
			{
				npc.CurrentDialogue.Push(Dialogue.FromTranslation(npc, "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13615", this.number.Value));
				Game1.drawDialogue(npc);
			}
		}
		return baseChanged;
	}

	/// <summary>Get the gold reward for a given item.</summary>
	/// <param name="item">The item instance.</param>
	public int GetGoldRewardPerItem(Item item)
	{
		if (item is Object obj)
		{
			return obj.Price * 3;
		}
		return (int)((float)item.salePrice() * 1.5f);
	}
}
