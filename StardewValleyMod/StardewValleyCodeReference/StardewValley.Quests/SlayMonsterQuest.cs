using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace StardewValley.Quests;

public class SlayMonsterQuest : Quest
{
	public string targetMessage;

	[XmlElement("monsterName")]
	public readonly NetString monsterName = new NetString();

	[XmlElement("target")]
	public readonly NetString target = new NetString();

	[XmlElement("monster")]
	public readonly NetRef<Monster> monster = new NetRef<Monster>();

	[XmlElement("numberToKill")]
	public readonly NetInt numberToKill = new NetInt();

	[XmlElement("reward")]
	public readonly NetInt reward = new NetInt();

	[XmlElement("numberKilled")]
	public readonly NetInt numberKilled = new NetInt();

	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Whether to ignore monsters killed on the farm.</summary>
	[XmlElement("ignoreFarmMonsters")]
	public readonly NetBool ignoreFarmMonsters = new NetBool(value: true);

	public SlayMonsterQuest()
	{
		base.questType.Value = 4;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.parts, "parts").AddField(this.dialogueparts, "dialogueparts").AddField(this.objective, "objective")
			.AddField(this.monsterName, "monsterName")
			.AddField(this.target, "target")
			.AddField(this.monster, "monster")
			.AddField(this.numberToKill, "numberToKill")
			.AddField(this.reward, "reward")
			.AddField(this.numberKilled, "numberKilled")
			.AddField(this.ignoreFarmMonsters, "ignoreFarmMonsters");
	}

	public void loadQuestInfo()
	{
		if (this.target.Value != null && this.monster != null)
		{
			return;
		}
		Random random = base.CreateInitializationRandom();
		for (int i = 0; i < random.Next(1, 100); i++)
		{
			random.Next();
		}
		base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
		List<string> possibleMonsters = new List<string>();
		int mineLevel = Utility.GetAllPlayerDeepestMineLevel();
		if (mineLevel < 39)
		{
			possibleMonsters.Add("Green Slime");
			if (mineLevel > 10)
			{
				possibleMonsters.Add("Rock Crab");
			}
			if (mineLevel > 30)
			{
				possibleMonsters.Add("Duggy");
			}
		}
		else if (mineLevel < 79)
		{
			possibleMonsters.Add("Frost Jelly");
			if (mineLevel > 70)
			{
				possibleMonsters.Add("Skeleton");
			}
			possibleMonsters.Add("Dust Spirit");
		}
		else
		{
			possibleMonsters.Add("Sludge");
			possibleMonsters.Add("Ghost");
			possibleMonsters.Add("Lava Crab");
			possibleMonsters.Add("Squid Kid");
		}
		int num;
		if (this.monsterName.Value != null)
		{
			num = ((this.numberToKill.Value == 0) ? 1 : 0);
			if (num == 0)
			{
				goto IL_011d;
			}
		}
		else
		{
			num = 1;
		}
		this.monsterName.Value = random.ChooseFrom(possibleMonsters);
		goto IL_011d;
		IL_011d:
		if (this.monsterName.Value == "Frost Jelly" || this.monsterName.Value == "Sludge")
		{
			this.monster.Value = new Monster("Green Slime", Vector2.Zero);
			this.monster.Value.Name = this.monsterName.Value;
		}
		else
		{
			this.monster.Value = new Monster(this.monsterName.Value, Vector2.Zero);
		}
		if (num != 0)
		{
			switch (this.monsterName.Value)
			{
			case "Green Slime":
				this.numberToKill.Value = random.Next(4, 11);
				this.numberToKill.Value = this.numberToKill.Value - this.numberToKill.Value % 2;
				this.reward.Value = this.numberToKill.Value * 60;
				break;
			case "Rock Crab":
				this.numberToKill.Value = random.Next(2, 6);
				this.reward.Value = this.numberToKill.Value * 75;
				break;
			case "Duggy":
				this.parts.Clear();
				this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13711", this.numberToKill.Value));
				this.target.Value = "Clint";
				this.numberToKill.Value = random.Next(2, 4);
				this.reward.Value = this.numberToKill.Value * 150;
				break;
			case "Frost Jelly":
				this.numberToKill.Value = random.Next(4, 11);
				this.numberToKill.Value = this.numberToKill.Value - this.numberToKill.Value % 2;
				this.reward.Value = this.numberToKill.Value * 85;
				break;
			case "Ghost":
				this.numberToKill.Value = random.Next(2, 4);
				this.reward.Value = this.numberToKill.Value * 250;
				break;
			case "Sludge":
				this.numberToKill.Value = random.Next(4, 11);
				this.numberToKill.Value = this.numberToKill.Value - this.numberToKill.Value % 2;
				this.reward.Value = this.numberToKill.Value * 125;
				break;
			case "Lava Crab":
				this.numberToKill.Value = random.Next(2, 6);
				this.reward.Value = this.numberToKill.Value * 180;
				break;
			case "Squid Kid":
				this.numberToKill.Value = random.Next(1, 3);
				this.reward.Value = this.numberToKill.Value * 350;
				break;
			case "Skeleton":
				this.numberToKill.Value = random.Next(6, 12);
				this.reward.Value = this.numberToKill.Value * 100;
				break;
			case "Dust Spirit":
				this.numberToKill.Value = random.Next(10, 21);
				this.reward.Value = this.numberToKill.Value * 60;
				break;
			default:
				this.numberToKill.Value = random.Next(3, 7);
				this.reward.Value = this.numberToKill.Value * 120;
				break;
			}
		}
		switch (this.monsterName.Value)
		{
		case "Green Slime":
		case "Frost Jelly":
		case "Sludge":
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13723", this.numberToKill.Value, this.monsterName.Value.Equals("Frost Jelly") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13725") : (this.monsterName.Value.Equals("Sludge") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13727") : new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13728"))));
			this.target.Value = "Lewis";
			this.dialogueparts.Clear();
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13730");
			if (random.NextBool())
			{
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13731");
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13732", "13733"));
				this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13734", new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13735", "13736")), new DescriptionElement("Strings\\StringsFromCSFiles:Dialogue.cs." + random.Choose<string>("795", "796", "797", "798", "799", "800", "801", "802", "803", "804", "805", "806", "807", "808", "809", "810")), new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13740", "13741", "13742"))));
			}
			else
			{
				this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13744");
			}
			break;
		case "Rock Crab":
		case "Lava Crab":
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13747", this.numberToKill.Value));
			this.target.Value = "Demetrius";
			this.dialogueparts.Clear();
			this.dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13750", this.monster.Value));
			break;
		default:
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13752", this.monster.Value, this.numberToKill.Value, new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13755", "13756", "13757"))));
			this.target.Value = "Wizard";
			this.dialogueparts.Clear();
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13760");
			break;
		}
		if (this.target.Value.Equals("Wizard") && !Utility.doesAnyFarmerHaveMail("wizardJunimoNote") && !Utility.doesAnyFarmerHaveMail("JojaMember"))
		{
			this.parts.Clear();
			this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13764", this.numberToKill.Value, this.monster.Value));
			this.target.Value = "Lewis";
			this.dialogueparts.Clear();
			this.dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13767");
		}
		this.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", this.reward.Value));
		this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", "0", this.numberToKill.Value, this.monster.Value);
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
		if (this.numberKilled.Value != 0 || !base.HasId())
		{
			if (this.numberKilled.Value < this.numberToKill.Value)
			{
				this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", this.numberKilled.Value, this.numberToKill.Value, this.monster.Value);
			}
			if (this.objective.Value != null)
			{
				base.currentObjective = this.objective.Value.loadDescriptionElement();
			}
		}
	}

	private bool isSlimeName(string s)
	{
		if (s.Contains("Slime") || s.Contains("Jelly") || s.Contains("Sludge"))
		{
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override bool OnMonsterSlain(GameLocation location, Monster monster, bool killedByBomb, bool isTameMonster, bool probe = false)
	{
		bool baseChanged = base.OnMonsterSlain(location, monster, killedByBomb, isTameMonster, probe);
		if (!base.completed.Value && (monster.Name.Contains(this.monsterName.Value) || (base.id.Value == "15" && this.isSlimeName(monster.Name))) && this.numberKilled.Value < this.numberToKill.Value)
		{
			if (!probe)
			{
				this.numberKilled.Value = Math.Min(this.numberToKill.Value, this.numberKilled.Value + 1);
				Game1.dayTimeMoneyBox.pingQuest(this);
				if (this.numberKilled.Value >= this.numberToKill.Value)
				{
					if (this.target.Value == null || this.target.Value.Equals("null"))
					{
						this.questComplete();
					}
					else
					{
						NPC actualTarget = Game1.getCharacterFromName(this.target.Value);
						this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13277", actualTarget);
						Game1.playSound("jingle1");
					}
				}
				else if (this.monster.Value == null)
				{
					if (this.monsterName.Value == "Frost Jelly" || this.monsterName.Value == "Sludge")
					{
						this.monster.Value = new Monster("Green Slime", Vector2.Zero);
						this.monster.Value.Name = this.monsterName.Value;
					}
					else
					{
						this.monster.Value = new Monster(this.monsterName.Value, Vector2.Zero);
					}
				}
			}
			return true;
		}
		return baseChanged;
	}

	public override bool OnNpcSocialized(NPC npc, bool probe = false)
	{
		bool baseChanged = base.OnNpcSocialized(npc, probe);
		if (!base.completed.Value && this.target.Value != null && this.target.Value != "null" && this.numberKilled.Value >= this.numberToKill.Value && npc.Name == this.target.Value && npc.IsVillager)
		{
			if (!probe)
			{
				this.reloadDescription();
				npc.CurrentDialogue.Push(new Dialogue(npc, null, this.targetMessage));
				base.moneyReward.Value = this.reward.Value;
				this.questComplete();
				Game1.drawDialogue(npc);
			}
			return true;
		}
		return baseChanged;
	}
}
