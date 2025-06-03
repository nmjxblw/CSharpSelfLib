using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;

namespace StardewValley;

public class Stats
{
	/// <summary>The number of each monster type killed, prefixed by the monster's internal name.</summary>
	public StatsDictionary<int> specificMonstersKilled = new StatsDictionary<int>();

	/// <summary>The numeric metrics tracked by the game.</summary>
	/// <remarks>Most code should use methods like <see cref="M:StardewValley.Stats.Get(System.String)" /> or <see cref="M:StardewValley.Stats.Set(System.String,System.UInt32)" /> instead of calling this directly.</remarks>
	public StatsDictionary<uint> Values = new StatsDictionary<uint>();

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Stats.Values" /> instead.</summary>
	[XmlElement("stat_dictionary")]
	public SerializableDictionary<string, uint> obsolete_stat_dictionary;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.AverageBedtime" /> instead.</summary>
	[XmlElement("averageBedtime")]
	public uint? obsolete_averageBedtime;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.BeveragesMade" /> instead.</summary>
	[XmlElement("beveragesMade")]
	public uint? obsolete_beveragesMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CaveCarrotsFound" /> instead.</summary>
	[XmlElement("caveCarrotsFound")]
	public uint? obsolete_caveCarrotsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CheeseMade" /> instead.</summary>
	[XmlElement("cheeseMade")]
	public uint? obsolete_cheeseMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ChickenEggsLayed" /> instead.</summary>
	[XmlElement("chickenEggsLayed")]
	public uint? obsolete_chickenEggsLayed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CopperFound" /> instead.</summary>
	[XmlElement("copperFound")]
	public uint? obsolete_copperFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CowMilkProduced" /> instead.</summary>
	[XmlElement("cowMilkProduced")]
	public uint? obsolete_cowMilkProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.CropsShipped" /> instead.</summary>
	[XmlElement("cropsShipped")]
	public uint? obsolete_cropsShipped;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DaysPlayed" /> instead.</summary>
	[XmlElement("daysPlayed")]
	public uint? obsolete_daysPlayed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DiamondsFound" /> instead.</summary>
	[XmlElement("diamondsFound")]
	public uint? obsolete_diamondsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DirtHoed" /> instead.</summary>
	[XmlElement("dirtHoed")]
	public uint? obsolete_dirtHoed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.DuckEggsLayed" /> instead.</summary>
	[XmlElement("duckEggsLayed")]
	public uint? obsolete_duckEggsLayed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.FishCaught" /> instead.</summary>
	[XmlElement("fishCaught")]
	public uint? obsolete_fishCaught;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GeodesCracked" /> instead.</summary>
	[XmlElement("geodesCracked")]
	public uint? obsolete_geodesCracked;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GiftsGiven" /> instead.</summary>
	[XmlElement("giftsGiven")]
	public uint? obsolete_giftsGiven;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoatCheeseMade" /> instead.</summary>
	[XmlElement("goatCheeseMade")]
	public uint? obsolete_goatCheeseMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoatMilkProduced" /> instead.</summary>
	[XmlElement("goatMilkProduced")]
	public uint? obsolete_goatMilkProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoldFound" /> instead.</summary>
	[XmlElement("goldFound")]
	public uint? obsolete_goldFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.GoodFriends" /> instead.</summary>
	[XmlElement("goodFriends")]
	public uint? obsolete_goodFriends;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.IndividualMoneyEarned" /> instead.</summary>
	[XmlElement("individualMoneyEarned")]
	public uint? obsolete_individualMoneyEarned;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.IridiumFound" /> instead.</summary>
	[XmlElement("iridiumFound")]
	public uint? obsolete_iridiumFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.IronFound" /> instead.</summary>
	[XmlElement("ironFound")]
	public uint? obsolete_ironFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsCooked" /> instead.</summary>
	[XmlElement("itemsCooked")]
	public uint? obsolete_itemsCooked;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsCrafted" /> instead.</summary>
	[XmlElement("itemsCrafted")]
	public uint? obsolete_itemsCrafted;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsForaged" /> instead.</summary>
	[XmlElement("itemsForaged")]
	public uint? obsolete_itemsForaged;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.ItemsShipped" /> instead.</summary>
	[XmlElement("itemsShipped")]
	public uint? obsolete_itemsShipped;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.MonstersKilled" /> instead.</summary>
	[XmlElement("monstersKilled")]
	public uint? obsolete_monstersKilled;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.MysticStonesCrushed" /> instead.</summary>
	[XmlElement("mysticStonesCrushed")]
	public uint? obsolete_mysticStonesCrushed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.NotesFound" /> instead.</summary>
	[XmlElement("notesFound")]
	public uint? obsolete_notesFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.OtherPreciousGemsFound" /> instead.</summary>
	[XmlElement("otherPreciousGemsFound")]
	public uint? obsolete_otherPreciousGemsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.PiecesOfTrashRecycled" /> instead.</summary>
	[XmlElement("piecesOfTrashRecycled")]
	public uint? obsolete_piecesOfTrashRecycled;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.PreservesMade" /> instead.</summary>
	[XmlElement("preservesMade")]
	public uint? obsolete_preservesMade;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.PrismaticShardsFound" /> instead.</summary>
	[XmlElement("prismaticShardsFound")]
	public uint? obsolete_prismaticShardsFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.QuestsCompleted" /> instead.</summary>
	[XmlElement("questsCompleted")]
	public uint? obsolete_questsCompleted;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.RabbitWoolProduced" /> instead.</summary>
	[XmlElement("rabbitWoolProduced")]
	public uint? obsolete_rabbitWoolProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.RocksCrushed" /> instead.</summary>
	[XmlElement("rocksCrushed")]
	public uint? obsolete_rocksCrushed;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.SheepWoolProduced" /> instead.</summary>
	[XmlElement("sheepWoolProduced")]
	public uint? obsolete_sheepWoolProduced;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.SlimesKilled" /> instead.</summary>
	[XmlElement("slimesKilled")]
	public uint? obsolete_slimesKilled;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.StepsTaken" /> instead.</summary>
	[XmlElement("stepsTaken")]
	public uint? obsolete_stepsTaken;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.StoneGathered" /> instead.</summary>
	[XmlElement("stoneGathered")]
	public uint? obsolete_stoneGathered;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.StumpsChopped" /> instead.</summary>
	[XmlElement("stumpsChopped")]
	public uint? obsolete_stumpsChopped;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.TimesFished" /> instead.</summary>
	[XmlElement("timesFished")]
	public uint? obsolete_timesFished;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.TimesUnconscious" /> instead.</summary>
	[XmlElement("timesUnconscious")]
	public uint? obsolete_timesUnconscious;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Constants.StatKeys.TotalMoneyGifted" /> instead.</summary>
	[XmlElement("totalMoneyGifted")]
	public uint? obsolete_totalMoneyGifted;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.TrufflesFound" /> instead.</summary>
	[XmlElement("trufflesFound")]
	public uint? obsolete_trufflesFound;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.WeedsEliminated" /> instead.</summary>
	[XmlElement("weedsEliminated")]
	public uint? obsolete_weedsEliminated;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Stats.SeedsSown" /> instead.</summary>
	[XmlElement("seedsSown")]
	public uint? obsolete_seedsSown;

	/// <summary>Whether platform achievements can be unlocked retroactively overnight or when loading the save.</summary>
	/// <remarks>Certification requirements on some platforms prohibit us from unlocking trophies without the player doing something. On those platforms, we instead unlock missed achievements when the player performs a relevant action.</remarks>
	public static bool AllowRetroactiveAchievements => Program.sdk.RetroactiveAchievementsAllowed();

	[XmlIgnore]
	public uint AverageBedtime
	{
		get
		{
			return this.Get("averageBedtime");
		}
		set
		{
			uint prevAverage = this.Get("averageBedtime");
			uint daysPlayed = this.Get("daysPlayed");
			this.Set("averageBedtime", (prevAverage * (daysPlayed - 1) + value) / Math.Max(1u, daysPlayed));
		}
	}

	[XmlIgnore]
	public uint DaysPlayed
	{
		get
		{
			return this.Get("daysPlayed");
		}
		set
		{
			this.Set("daysPlayed", value);
		}
	}

	[XmlIgnore]
	public uint IndividualMoneyEarned
	{
		get
		{
			return this.Get("individualMoneyEarned");
		}
		set
		{
			uint previousEarned = this.Get("individualMoneyEarned");
			this.Set("individualMoneyEarned", value);
			if (previousEarned < 1000000 && value >= 1000000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned1mil_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
			else if (previousEarned < 100000 && value >= 100000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned100k_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
			else if (previousEarned < 10000 && value >= 10000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned10k_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
			else if (previousEarned < 1000 && value >= 1000)
			{
				Game1.multiplayer.globalChatInfoMessage("SoloEarned1k_" + (Game1.player.IsMale ? "Male" : "Female"), Game1.player.Name);
			}
		}
	}

	[XmlIgnore]
	public uint ItemsCooked
	{
		get
		{
			return this.Get("itemsCooked");
		}
		set
		{
			this.Set("itemsCooked", value);
		}
	}

	[XmlIgnore]
	public uint ItemsCrafted
	{
		get
		{
			return this.Get("itemsCrafted");
		}
		set
		{
			this.Set("itemsCrafted", value);
			this.checkForCraftingAchievements();
		}
	}

	[XmlIgnore]
	public uint ItemsForaged
	{
		get
		{
			return this.Get("itemsForaged");
		}
		set
		{
			this.Set("itemsForaged", value);
		}
	}

	[XmlIgnore]
	public uint ItemsShipped
	{
		get
		{
			return this.Get("itemsShipped");
		}
		set
		{
			this.Set("itemsShipped", value);
		}
	}

	[XmlIgnore]
	public uint NotesFound
	{
		get
		{
			return this.Get("notesFound");
		}
		set
		{
			this.Set("notesFound", value);
		}
	}

	[XmlIgnore]
	public uint StepsTaken
	{
		get
		{
			return this.Get("stepsTaken");
		}
		set
		{
			this.Set("stepsTaken", value);
		}
	}

	[XmlIgnore]
	public uint StumpsChopped
	{
		get
		{
			return this.Get("stumpsChopped");
		}
		set
		{
			this.Set("stumpsChopped", value);
		}
	}

	[XmlIgnore]
	public uint TimesUnconscious
	{
		get
		{
			return this.Get("timesUnconscious");
		}
		set
		{
			this.Set("timesUnconscious", value);
		}
	}

	[XmlIgnore]
	public uint BeveragesMade
	{
		get
		{
			return this.Get("beveragesMade");
		}
		set
		{
			this.Set("beveragesMade", value);
		}
	}

	[XmlIgnore]
	public uint CheeseMade
	{
		get
		{
			return this.Get("cheeseMade");
		}
		set
		{
			this.Set("cheeseMade", value);
		}
	}

	[XmlIgnore]
	public uint ChickenEggsLayed
	{
		get
		{
			return this.Get("chickenEggsLayed");
		}
		set
		{
			this.Set("chickenEggsLayed", value);
		}
	}

	[XmlIgnore]
	public uint CowMilkProduced
	{
		get
		{
			return this.Get("cowMilkProduced");
		}
		set
		{
			this.Set("cowMilkProduced", value);
		}
	}

	[XmlIgnore]
	public uint CropsShipped
	{
		get
		{
			return this.Get("cropsShipped");
		}
		set
		{
			this.Set("cropsShipped", value);
		}
	}

	[XmlIgnore]
	public uint DirtHoed
	{
		get
		{
			return this.Get("dirtHoed");
		}
		set
		{
			this.Set("dirtHoed", value);
		}
	}

	[XmlIgnore]
	public uint DuckEggsLayed
	{
		get
		{
			return this.Get("duckEggsLayed");
		}
		set
		{
			this.Set("duckEggsLayed", value);
		}
	}

	[XmlIgnore]
	public uint GoatCheeseMade
	{
		get
		{
			return this.Get("goatCheeseMade");
		}
		set
		{
			this.Set("goatCheeseMade", value);
		}
	}

	[XmlIgnore]
	public uint GoatMilkProduced
	{
		get
		{
			return this.Get("goatMilkProduced");
		}
		set
		{
			this.Set("goatMilkProduced", value);
		}
	}

	[XmlIgnore]
	public uint PiecesOfTrashRecycled
	{
		get
		{
			return this.Get("piecesOfTrashRecycled");
		}
		set
		{
			this.Set("piecesOfTrashRecycled", value);
		}
	}

	[XmlIgnore]
	public uint PreservesMade
	{
		get
		{
			return this.Get("preservesMade");
		}
		set
		{
			this.Set("preservesMade", value);
		}
	}

	[XmlIgnore]
	public uint RabbitWoolProduced
	{
		get
		{
			return this.Get("rabbitWoolProduced");
		}
		set
		{
			this.Set("rabbitWoolProduced", value);
		}
	}

	[XmlIgnore]
	public uint SeedsSown
	{
		get
		{
			return this.Get("seedsSown");
		}
		set
		{
			this.Set("seedsSown", value);
		}
	}

	[XmlIgnore]
	public uint SheepWoolProduced
	{
		get
		{
			return this.Get("sheepWoolProduced");
		}
		set
		{
			this.Set("sheepWoolProduced", value);
		}
	}

	[XmlIgnore]
	public uint TrufflesFound
	{
		get
		{
			return this.Get("trufflesFound");
		}
		set
		{
			this.Set("trufflesFound", value);
		}
	}

	[XmlIgnore]
	public uint WeedsEliminated
	{
		get
		{
			return this.Get("weedsEliminated");
		}
		set
		{
			this.Set("weedsEliminated", value);
		}
	}

	[XmlIgnore]
	public uint MonstersKilled
	{
		get
		{
			return this.Get("monstersKilled");
		}
		set
		{
			this.Set("monstersKilled", value);
		}
	}

	[XmlIgnore]
	public uint SlimesKilled
	{
		get
		{
			return this.Get("slimesKilled");
		}
		set
		{
			this.Set("slimesKilled", value);
		}
	}

	[XmlIgnore]
	public uint FishCaught
	{
		get
		{
			return this.Get("fishCaught");
		}
		set
		{
			this.Set("fishCaught", value);
		}
	}

	[XmlIgnore]
	public uint TimesFished
	{
		get
		{
			return this.Get("timesFished");
		}
		set
		{
			this.Set("timesFished", value);
		}
	}

	[XmlIgnore]
	public uint CaveCarrotsFound
	{
		get
		{
			return this.Get("caveCarrotsFound");
		}
		set
		{
			this.Set("caveCarrotsFound", value);
		}
	}

	[XmlIgnore]
	public uint CopperFound
	{
		get
		{
			return this.Get("copperFound");
		}
		set
		{
			this.Set("copperFound", value);
		}
	}

	[XmlIgnore]
	public uint DiamondsFound
	{
		get
		{
			return this.Get("diamondsFound");
		}
		set
		{
			this.Set("diamondsFound", value);
		}
	}

	[XmlIgnore]
	public uint GeodesCracked
	{
		get
		{
			return this.Get("geodesCracked");
		}
		set
		{
			this.Set("geodesCracked", value);
		}
	}

	[XmlIgnore]
	public uint GoldFound
	{
		get
		{
			return this.Get("goldFound");
		}
		set
		{
			this.Set("goldFound", value);
		}
	}

	[XmlIgnore]
	public uint IridiumFound
	{
		get
		{
			return this.Get("iridiumFound");
		}
		set
		{
			this.Set("iridiumFound", value);
		}
	}

	[XmlIgnore]
	public uint IronFound
	{
		get
		{
			return this.Get("ironFound");
		}
		set
		{
			this.Set("ironFound", value);
		}
	}

	[XmlIgnore]
	public uint MysticStonesCrushed
	{
		get
		{
			return this.Get("mysticStonesCrushed");
		}
		set
		{
			this.Set("mysticStonesCrushed", value);
		}
	}

	[XmlIgnore]
	public uint OtherPreciousGemsFound
	{
		get
		{
			return this.Get("otherPreciousGemsFound");
		}
		set
		{
			this.Set("otherPreciousGemsFound", value);
		}
	}

	[XmlIgnore]
	public uint PrismaticShardsFound
	{
		get
		{
			return this.Get("prismaticShardsFound");
		}
		set
		{
			this.Set("prismaticShardsFound", value);
		}
	}

	[XmlIgnore]
	public uint RocksCrushed
	{
		get
		{
			return this.Get("rocksCrushed");
		}
		set
		{
			this.Set("rocksCrushed", value);
		}
	}

	[XmlIgnore]
	public uint StoneGathered
	{
		get
		{
			return this.Get("stoneGathered");
		}
		set
		{
			this.Set("stoneGathered", value);
		}
	}

	[XmlIgnore]
	public uint GiftsGiven
	{
		get
		{
			return this.Get("giftsGiven");
		}
		set
		{
			this.Set("giftsGiven", value);
		}
	}

	[XmlIgnore]
	public uint GoodFriends
	{
		get
		{
			return this.Get("goodFriends");
		}
		set
		{
			this.Set("goodFriends", value);
		}
	}

	[XmlIgnore]
	public uint QuestsCompleted
	{
		get
		{
			return this.Get("questsCompleted");
		}
		set
		{
			this.Set("questsCompleted", value);
			this.checkForQuestAchievements();
		}
	}

	/// <summary>Get the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	public uint Get(string key)
	{
		if (!this.Values.TryGetValue(key, out var value))
		{
			return 0u;
		}
		return value;
	}

	/// <summary>Set the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="value">The new value to set.</param>
	public void Set(string key, uint value)
	{
		if (value != 0)
		{
			this.Values[key] = value;
		}
		else
		{
			this.Values.Remove(key);
		}
	}

	/// <summary>Set the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="value">The new value to set.</param>
	/// <remarks>The minimum stat value is zero. Setting a negative value is equivalent to setting zero.</remarks>
	public void Set(string key, int value)
	{
		if (value <= 0)
		{
			this.Set(key, 0u);
		}
		else
		{
			this.Set(key, (uint)value);
		}
	}

	/// <summary>Decrease the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="amount">The amount by which to decrease the stat.</param>
	/// <remarks>The minimum stat value is zero. Decrementing past zero is equivalent to setting zero.</remarks>
	public uint Decrement(string key, uint amount = 1u)
	{
		uint newValue = this.Get(key);
		newValue = ((amount < newValue) ? (newValue - amount) : 0u);
		this.Set(key, newValue);
		return newValue;
	}

	/// <summary>Increase the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="amount">The amount by which to increase the stat.</param>
	/// <returns>Returns the new stat value.</returns>
	public uint Increment(string key, uint amount = 1u)
	{
		uint newValue = this.Get(key) + amount;
		this.Set(key, newValue);
		return newValue;
	}

	/// <summary>Increase the value of a tracked stat.</summary>
	/// <param name="key">The unique stat key, usually matching a <see cref="T:StardewValley.Constants.StatKeys" /> field.</param>
	/// <param name="amount">The amount by which to increase the stat. If this is set to a negative value, the stat will be decremented instead (up to a minimum of zero).</param>
	/// <returns>Returns the new stat value.</returns>
	public uint Increment(string key, int amount)
	{
		if (amount >= 0)
		{
			return this.Increment(key, (uint)amount);
		}
		return this.Decrement(key, (uint)(-amount));
	}

	/// <summary>Update the stats when a monster is killed.</summary>
	/// <param name="name">The monster's internal name.</param>
	public void monsterKilled(string name)
	{
		if (AdventureGuild.willThisKillCompleteAMonsterSlayerQuest(name))
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Stats.cs.5129"));
			Game1.multiplayer.globalChatInfoMessage("MonsterSlayer" + Game1.random.Next(4), Game1.player.Name, TokenStringBuilder.MonsterName(name));
		}
		this.specificMonstersKilled[name] = this.getMonstersKilled(name) + 1;
		this.checkForMonsterSlayerAchievement(isDirectUnlock: true);
	}

	/// <summary>Get the number of a given monster type that the player has killed.</summary>
	/// <param name="name">The monster's internal name.</param>
	public int getMonstersKilled(string name)
	{
		return this.specificMonstersKilled.GetValueOrDefault(name);
	}

	public void onMoneyGifted(uint amount)
	{
		uint previousMoney = this.Get("totalMoneyGifted");
		uint totalMoneyGifted = this.Increment("totalMoneyGifted", amount);
		if (previousMoney <= 1000000 && totalMoneyGifted > 1000000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted1mil", Game1.player.Name);
		}
		else if (previousMoney <= 100000 && totalMoneyGifted > 100000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted100k", Game1.player.Name);
		}
		else if (previousMoney <= 10000 && totalMoneyGifted > 10000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted10k", Game1.player.Name);
		}
		else if (previousMoney <= 1000 && totalMoneyGifted > 1000)
		{
			Game1.multiplayer.globalChatInfoMessage("Gifted1k", Game1.player.Name);
		}
	}

	public void takeStep()
	{
		switch (this.Increment("stepsTaken"))
		{
		case 10000u:
			Game1.multiplayer.globalChatInfoMessage("Walked10k", Game1.player.Name);
			break;
		case 100000u:
			Game1.multiplayer.globalChatInfoMessage("Walked100k", Game1.player.Name);
			break;
		case 1000000u:
			Game1.multiplayer.globalChatInfoMessage("Walked1m", Game1.player.Name);
			break;
		case 10000000u:
			Game1.multiplayer.globalChatInfoMessage("Walked10m", Game1.player.Name);
			break;
		}
	}

	/// <summary>Unlock the 'Well Read' achievement if its criteria has been met.</summary>
	public void checkForBooksReadAchievement()
	{
		if (Game1.player.stats.Get("Book_Trash") != 0 && Game1.player.stats.Get("Book_Crabbing") != 0 && Game1.player.stats.Get("Book_Bombs") != 0 && Game1.player.stats.Get("Book_Roe") != 0 && Game1.player.stats.Get("Book_WildSeeds") != 0 && Game1.player.stats.Get("Book_Woodcutting") != 0 && Game1.player.stats.Get("Book_Defense") != 0 && Game1.player.stats.Get("Book_Friendship") != 0 && Game1.player.stats.Get("Book_Void") != 0 && Game1.player.stats.Get("Book_Speed") != 0 && Game1.player.stats.Get("Book_Marlon") != 0 && Game1.player.stats.Get("Book_PriceCatalogue") != 0 && Game1.player.stats.Get("Book_Diamonds") != 0 && Game1.player.stats.Get("Book_Mystery") != 0 && Game1.player.stats.Get("Book_AnimalCatalogue") != 0 && Game1.player.stats.Get("Book_Speed2") != 0 && Game1.player.stats.Get("Book_Artifact") != 0 && Game1.player.stats.Get("Book_Horse") != 0 && Game1.player.stats.Get("Book_Grass") != 0)
		{
			Game1.getAchievement(35);
		}
	}

	/// <summary>Unlock the cooking-related achievements if their criteria have been met.</summary>
	public void checkForCookingAchievements()
	{
		Dictionary<string, string> recipes = CraftingRecipe.cookingRecipes;
		int numberOfRecipesCooked = 0;
		int numberOfMealsMade = 0;
		foreach (KeyValuePair<string, string> v in recipes)
		{
			if (Game1.player.cookingRecipes.ContainsKey(v.Key))
			{
				string recipe = ArgUtility.SplitBySpaceAndGet(v.Value.Split('/')[2], 0);
				if (Game1.player.recipesCooked.TryGetValue(recipe, out var timesCooked))
				{
					numberOfMealsMade += timesCooked;
					numberOfRecipesCooked++;
				}
			}
		}
		this.Set("itemsCooked", numberOfMealsMade);
		if (numberOfRecipesCooked >= recipes.Count)
		{
			Game1.getAchievement(17);
		}
		if (numberOfRecipesCooked >= 25)
		{
			Game1.getAchievement(16);
		}
		if (numberOfRecipesCooked >= 10)
		{
			Game1.getAchievement(15);
		}
	}

	/// <summary>Unlock the crafting-related achievements if their criteria have been met.</summary>
	public void checkForCraftingAchievements()
	{
		Dictionary<string, string> recipes = CraftingRecipe.craftingRecipes;
		int numberOfRecipesMade = 0;
		int numberOfItemsCrafted = 0;
		foreach (string s in recipes.Keys)
		{
			if (!(s == "Wedding Ring") && Game1.player.craftingRecipes.TryGetValue(s, out var timesCrafted))
			{
				numberOfItemsCrafted += timesCrafted;
				if (Game1.player.craftingRecipes[s] > 0)
				{
					numberOfRecipesMade++;
				}
			}
		}
		this.Set("itemsCrafted", numberOfItemsCrafted);
		if (numberOfRecipesMade >= recipes.Count - 1)
		{
			Game1.getAchievement(22);
		}
		if (numberOfRecipesMade >= 30)
		{
			Game1.getAchievement(21);
		}
		if (numberOfRecipesMade >= 15)
		{
			Game1.getAchievement(20);
		}
	}

	/// <summary>Unlock the shipping-related achievements if their criteria have been met.</summary>
	public void checkForShippingAchievements()
	{
		bool allPolycultureCropsShipped = true;
		bool anyMonocultureCropShipped = false;
		foreach (CropData data in Game1.cropData.Values)
		{
			if (data.CountForPolyculture)
			{
				allPolycultureCropsShipped = allPolycultureCropsShipped && DidFarmerShip(data.HarvestItemId, 15);
			}
			if (data.CountForMonoculture)
			{
				anyMonocultureCropShipped = anyMonocultureCropShipped || DidFarmerShip(data.HarvestItemId, 300);
			}
		}
		if (allPolycultureCropsShipped)
		{
			Game1.getAchievement(31);
		}
		if (anyMonocultureCropShipped)
		{
			Game1.getAchievement(32);
		}
		if (Utility.hasFarmerShippedAllItems())
		{
			Game1.getAchievement(34);
		}
		static bool DidFarmerShip(string itemId, int number)
		{
			return Game1.player.basicShipped.GetValueOrDefault(itemId, 0) >= number;
		}
	}

	/// <summary>Unlock the fishing-related achievements if their criteria have been met.</summary>
	public void checkForFishingAchievements()
	{
		int numberOfFishCaught = 0;
		int differentKindsOfFishCaught = 0;
		int totalKindsOfFish = 0;
		foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
		{
			if (itemData.ObjectType == "Fish" && !(itemData.RawData is ObjectData { ExcludeFromFishingCollection: not false }))
			{
				totalKindsOfFish++;
				if (Game1.player.fishCaught.TryGetValue(itemData.QualifiedItemId, out var data))
				{
					numberOfFishCaught += data[0];
					differentKindsOfFishCaught++;
				}
			}
		}
		this.Set("fishCaught", numberOfFishCaught);
		if (numberOfFishCaught >= 100)
		{
			Game1.getAchievement(27);
		}
		if (differentKindsOfFishCaught >= totalKindsOfFish)
		{
			Game1.getAchievement(26);
			if (!Game1.player.hasOrWillReceiveMail("CF_Fish"))
			{
				Game1.addMailForTomorrow("CF_Fish");
			}
		}
		if (differentKindsOfFishCaught >= 24)
		{
			Game1.getAchievement(25);
		}
		if (differentKindsOfFishCaught >= 10)
		{
			Game1.getAchievement(24);
		}
	}

	/// <summary>Unlock the artifact donation-related achievements if their criteria have been met.</summary>
	public void checkForArchaeologyAchievements()
	{
		int length = Game1.netWorldState.Value.MuseumPieces.Length;
		if (length >= LibraryMuseum.totalArtifacts)
		{
			Game1.getAchievement(5);
		}
		if (length >= 40)
		{
			Game1.getAchievement(28);
		}
	}

	/// <summary>Unlock achievements related to the items held by the player.</summary>
	public void checkForHeldItemAchievements()
	{
		if (Game1.player.Items.ContainsId("(W)62") || Game1.player.Items.ContainsId("(W)63") || Game1.player.Items.ContainsId("(W)64"))
		{
			Game1.getAchievement(42);
		}
	}

	/// <summary>Unlock the money-related achievements if their criteria have been met.</summary>
	public void checkForMoneyAchievements()
	{
		if (Game1.player.totalMoneyEarned >= 10000000)
		{
			Game1.getAchievement(4);
		}
		if (Game1.player.totalMoneyEarned >= 1000000)
		{
			Game1.getAchievement(3);
		}
		if (Game1.player.totalMoneyEarned >= 250000)
		{
			Game1.getAchievement(2);
		}
		if (Game1.player.totalMoneyEarned >= 50000)
		{
			Game1.getAchievement(1);
		}
		if (Game1.player.totalMoneyEarned >= 15000)
		{
			Game1.getAchievement(0);
		}
	}

	/// <summary>Unlock the farmhouse upgrade-related achievements if their criteria have been met.</summary>
	public void checkForBuildingUpgradeAchievements()
	{
		if (Game1.player.HouseUpgradeLevel >= 2)
		{
			Game1.getAchievement(19);
		}
		if (Game1.player.HouseUpgradeLevel >= 1)
		{
			Game1.getAchievement(18);
		}
	}

	/// <summary>Unlock the quest-related achievements if their criteria have been met.</summary>
	public void checkForQuestAchievements()
	{
		if (this.QuestsCompleted >= 40)
		{
			Game1.getAchievement(30);
			Game1.addMailForTomorrow("quest35");
		}
		if (this.QuestsCompleted >= 10)
		{
			Game1.getAchievement(29);
			Game1.addMailForTomorrow("quest10");
		}
	}

	/// <summary>Unlock the friendship-related achievements if their criteria have been met.</summary>
	public void checkForFriendshipAchievements()
	{
		uint numberOf5Level = 0u;
		uint numberOf8Level = 0u;
		uint numberOf10Level = 0u;
		foreach (Friendship value in Game1.player.friendshipData.Values)
		{
			if (value.Points >= 2500)
			{
				numberOf10Level++;
			}
			if (value.Points >= 2000)
			{
				numberOf8Level++;
			}
			if (value.Points >= 1250)
			{
				numberOf5Level++;
			}
		}
		this.GoodFriends = numberOf8Level;
		if (numberOf5Level >= 20)
		{
			Game1.getAchievement(13);
		}
		if (numberOf5Level >= 10)
		{
			Game1.getAchievement(12);
		}
		if (numberOf5Level >= 4)
		{
			Game1.getAchievement(11);
		}
		if (numberOf5Level >= 1)
		{
			Game1.getAchievement(6);
		}
		if (numberOf10Level >= 8)
		{
			Game1.getAchievement(9);
		}
		if (numberOf10Level >= 1)
		{
			Game1.getAchievement(7);
		}
		foreach (KeyValuePair<string, string> pair in CraftingRecipe.cookingRecipes)
		{
			string recipeKey = pair.Key;
			string[] getConditions = ArgUtility.SplitBySpace(ArgUtility.Get(pair.Value.Split('/'), 3));
			if (!(ArgUtility.Get(getConditions, 0) != "f"))
			{
				string npcName = ArgUtility.Get(getConditions, 1);
				int minHearts = ArgUtility.GetInt(getConditions, 2);
				if (npcName != null && Game1.player.friendshipData.TryGetValue(npcName, out var friendship) && friendship.Points >= minHearts * 250 && !Game1.player.cookingRecipes.ContainsKey(recipeKey) && !Game1.player.hasOrWillReceiveMail(npcName + "Cooking"))
				{
					Game1.addMailForTomorrow(npcName + "Cooking");
				}
			}
		}
		foreach (KeyValuePair<string, string> pair2 in CraftingRecipe.craftingRecipes)
		{
			string recipeKey2 = pair2.Key;
			string[] getConditions2 = ArgUtility.SplitBySpace(ArgUtility.Get(pair2.Value.Split('/'), 4));
			if (!(ArgUtility.Get(getConditions2, 0) != "f"))
			{
				string npcName2 = ArgUtility.Get(getConditions2, 1);
				int minHearts2 = ArgUtility.GetInt(getConditions2, 2);
				if (npcName2 != null && Game1.player.friendshipData.TryGetValue(npcName2, out var friendship2) && friendship2.Points >= minHearts2 * 250 && !Game1.player.craftingRecipes.ContainsKey(recipeKey2) && !Game1.player.hasOrWillReceiveMail(npcName2 + "Crafting"))
				{
					Game1.addMailForTomorrow(npcName2 + "Crafting");
				}
			}
		}
	}

	/// <summary>Unlock the achievements for completing the community center or Joja path if their criteria have been met.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	public void checkForCommunityCenterOrJojaAchievements(bool isDirectUnlock)
	{
		if (this.CanUnlockPlatformAchievements(isDirectUnlock))
		{
			if (Game1.player.eventsSeen.Contains("191393"))
			{
				Game1.getSteamAchievement("Achievement_LocalLegend");
			}
			if (Game1.player.eventsSeen.Contains("502261"))
			{
				Game1.getSteamAchievement("Achievement_Joja");
			}
		}
	}

	/// <summary>Unlock the mini-game-related achievements if their criteria have been met.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	public void checkForMiniGameAchievements(bool isDirectUnlock)
	{
		if (this.CanUnlockPlatformAchievements(isDirectUnlock))
		{
			if (Game1.player.stats.Get("completedPrairieKing") != 0)
			{
				Game1.getSteamAchievement("Achievement_PrairieKing");
			}
			if (Game1.player.stats.Get("completedPrairieKingWithoutDying") != 0)
			{
				Game1.getSteamAchievement("Achievement_FectorsChallenge");
			}
		}
	}

	/// <summary>Unlock the 'Full House' achievement if the player is married with two children.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	public void checkForFullHouseAchievement(bool isDirectUnlock)
	{
		if (this.CanUnlockPlatformAchievements(isDirectUnlock) && Game1.player.isMarriedOrRoommates() && Game1.player.getChildrenCount() >= 2)
		{
			Game1.getSteamAchievement("Achievement_FullHouse");
		}
	}

	/// <summary>Unlock the 'The Bottom' achievement if the player has reached the bottom of the mines.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	/// <param name="assumeDeepestLevel">Unlock the achievement regardless of the <see cref="P:StardewValley.Farmer.deepestMineLevel" />.</param>
	public void checkForMineAchievement(bool isDirectUnlock, bool assumeDeepestLevel = false)
	{
		if (this.CanUnlockPlatformAchievements(isDirectUnlock) && (assumeDeepestLevel || Game1.player.deepestMineLevel >= 120))
		{
			Game1.getSteamAchievement("Achievement_TheBottom");
		}
	}

	/// <summary>Unlock the 'Protector of the Valley' achievement if the player has completed all monster slayer goals.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	public void checkForMonsterSlayerAchievement(bool isDirectUnlock)
	{
		if (this.CanUnlockPlatformAchievements(isDirectUnlock) && AdventureGuild.areAllMonsterSlayerQuestsComplete())
		{
			Game1.player.hasCompletedAllMonsterSlayerQuests.Value = true;
			Game1.getSteamAchievement("Achievement_KeeperOfTheMysticRings");
		}
	}

	/// <summary>Unlock the skill-related achievements if their criteria have been met.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	public void checkForSkillAchievements(bool isDirectUnlock)
	{
		if (!this.CanUnlockPlatformAchievements(isDirectUnlock))
		{
			return;
		}
		NetInt[] obj = new NetInt[5]
		{
			Game1.player.farmingLevel,
			Game1.player.miningLevel,
			Game1.player.fishingLevel,
			Game1.player.foragingLevel,
			Game1.player.combatLevel
		};
		bool anyMaxed = false;
		bool allMaxed = true;
		NetInt[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Value >= 10)
			{
				anyMaxed = true;
			}
			else
			{
				allMaxed = false;
			}
		}
		if (anyMaxed)
		{
			Game1.getSteamAchievement("Achievement_SingularTalent");
			if (allMaxed)
			{
				Game1.getSteamAchievement("Achievement_MasterOfTheFiveWays");
			}
		}
	}

	/// <summary>Unlock the 'Mystery Of The Stardrops' achievement if the player has found all stardrops.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	public void checkForStardropAchievement(bool isDirectUnlock)
	{
		if (this.CanUnlockPlatformAchievements(isDirectUnlock) && Utility.foundAllStardrops())
		{
			Game1.getSteamAchievement("Achievement_Stardrop");
		}
	}

	public bool isSharedAchievement(int which)
	{
		if ((uint)which <= 5u || which == 28)
		{
			return true;
		}
		return false;
	}

	/// <summary>Unlock all achievements whose criteria have been met.</summary>
	public void checkForAchievements()
	{
		this.checkForBooksReadAchievement();
		this.checkForCookingAchievements();
		this.checkForCraftingAchievements();
		this.checkForShippingAchievements();
		this.checkForFishingAchievements();
		this.checkForArchaeologyAchievements();
		this.checkForHeldItemAchievements();
		this.checkForMoneyAchievements();
		this.checkForBuildingUpgradeAchievements();
		this.checkForQuestAchievements();
		this.checkForFriendshipAchievements();
		this.checkForCommunityCenterOrJojaAchievements(isDirectUnlock: false);
		this.checkForMiniGameAchievements(isDirectUnlock: false);
		this.checkForFullHouseAchievement(isDirectUnlock: false);
		this.checkForMineAchievement(isDirectUnlock: false);
		this.checkForMonsterSlayerAchievement(isDirectUnlock: false);
		this.checkForSkillAchievements(isDirectUnlock: false);
		this.checkForStardropAchievement(isDirectUnlock: false);
	}

	/// <summary>Get whether platform achievements can be unlocked now based on platform restrictions.</summary>
	/// <param name="isDirectUnlock">Whether we're unlocking the achievements at the point where they normally trigger (i.e. not retroactively).</param>
	/// <remarks>See remarks on <see cref="P:StardewValley.Stats.AllowRetroactiveAchievements" />.</remarks>
	public bool CanUnlockPlatformAchievements(bool isDirectUnlock)
	{
		return Stats.AllowRetroactiveAchievements || isDirectUnlock;
	}
}
