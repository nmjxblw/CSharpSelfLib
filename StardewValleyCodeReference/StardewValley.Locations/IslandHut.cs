using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandHut : IslandLocation
{
	public NetBool treeNutObtained = new NetBool();

	[XmlIgnore]
	public NetEvent0 hitTreeEvent = new NetEvent0();

	[XmlIgnore]
	public NetEvent0 parrotBoyEvent = new NetEvent0();

	[XmlIgnore]
	public bool treeHitLocal;

	[XmlElement("firstParrotDone")]
	public readonly NetBool firstParrotDone = new NetBool();

	[XmlIgnore]
	public List<string> hintDialogues = new List<string>();

	[XmlElement("hintForToday")]
	public NetString hintForToday = new NetString(null);

	[XmlIgnore]
	public float hintShowTime = -1f;

	[XmlIgnore]
	public float hintShakeTime = -1f;

	public override void draw(SpriteBatch b)
	{
		if (this.treeHitLocal)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(10f, 7f) * 64f), new Microsoft.Xna.Framework.Rectangle(16, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}
		base.draw(b);
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (ArgUtility.Get(action, 0) == "Parrot")
		{
			this.ShowNutHint();
			return true;
		}
		return base.performAction(action, who, tileLocation);
	}

	public virtual int ShowNutHint()
	{
		List<KeyValuePair<string, int>> valid_hints = new List<KeyValuePair<string, int>>();
		int missing = 0;
		int north_nuts = 0;
		if (this.MissingTheseNuts(ref north_nuts, "Bush_IslandNorth_13_33", "Bush_IslandNorth_5_30"))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_VolcanoLava", 0));
		}
		bool archaeology_unlocked = Game1.MasterPlayer.hasOrWillReceiveMail("Island_UpgradeBridge");
		int buried_nuts = 0;
		if (this.MissingTheseNuts(ref buried_nuts, "Buried_IslandNorth_19_39") && archaeology_unlocked)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_BuriedArch", 0));
		}
		this.MissingTheseNuts(ref north_nuts, "Bush_IslandNorth_4_42");
		this.MissingTheseNuts(ref north_nuts, "Bush_IslandNorth_45_38", "Bush_IslandNorth_47_40");
		bool tent_nut_missing = false;
		if (this.MissingTheseNuts(ref missing, "IslandLeftPlantRestored", "IslandRightPlantRestored", "IslandBatRestored", "IslandFrogRestored"))
		{
			tent_nut_missing = true;
		}
		if (this.MissingTheseNuts(ref missing, "IslandCenterSkeletonRestored"))
		{
			missing += 5;
			tent_nut_missing = true;
		}
		if (this.MissingTheseNuts(ref missing, "IslandSnakeRestored"))
		{
			missing += 2;
			tent_nut_missing = true;
		}
		if (tent_nut_missing && Utility.doesAnyFarmerHaveOrWillReceiveMail("islandNorthCaveOpened"))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_Arch", 0));
		}
		if (this.MissingTheseNuts(ref buried_nuts, "Buried_IslandNorth_19_13", "Buried_IslandNorth_57_79", "Buried_IslandNorth_54_21", "Buried_IslandNorth_42_77", "Buried_IslandNorth_62_54", "Buried_IslandNorth_26_81"))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_NorthBuried", buried_nuts));
		}
		this.MissingTheseNuts(ref north_nuts, "Bush_IslandNorth_20_26", "Bush_IslandNorth_9_84");
		this.MissingTheseNuts(ref north_nuts, "Bush_IslandNorth_56_27");
		this.MissingTheseNuts(ref north_nuts, "Bush_IslandSouth_31_5");
		north_nuts += buried_nuts;
		if (north_nuts > 0)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_NorthHidden", north_nuts));
		}
		missing += north_nuts;
		if (this.MissingTheseNuts(ref missing, "TreeNut"))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_HutTree", 0));
		}
		bool west_unlocked = Game1.MasterPlayer.hasOrWillReceiveMail("Island_Turtle");
		int west_nuts = 0;
		if (this.MissingTheseNuts(ref west_nuts, "IslandWestCavePuzzle"))
		{
			west_nuts += 2;
		}
		this.MissingTheseNuts(ref west_nuts, "SandDuggy");
		if (this.MissingLimitedNutDrops(ref west_nuts, "TigerSlimeNut") && west_unlocked)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_TigerSlime", 0));
		}
		int buried_nuts2 = 0;
		if (this.MissingTheseNuts(ref buried_nuts2, "Buried_IslandWest_21_81", "Buried_IslandWest_62_76", "Buried_IslandWest_39_24", "Buried_IslandWest_88_14", "Buried_IslandWest_43_74", "Buried_IslandWest_30_75"))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_WestBuried", buried_nuts2));
		}
		west_nuts += buried_nuts2;
		int mussel_stone = 0;
		if (this.MissingLimitedNutDrops(ref mussel_stone, "MusselStone", 5) && west_unlocked)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_MusselStone", mussel_stone));
		}
		missing += mussel_stone;
		bool farm_unlocked = Game1.MasterPlayer.hasOrWillReceiveMail("Island_UpgradeHouse");
		int farming_nuts = 0;
		if (this.MissingLimitedNutDrops(ref farming_nuts, "IslandFarming", 5) && farm_unlocked)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_IslandFarming", farming_nuts));
		}
		this.MissingTheseNuts(ref west_nuts, "Bush_IslandWest_104_3", "Bush_IslandWest_31_24", "Bush_IslandWest_38_56", "Bush_IslandWest_75_29", "Bush_IslandWest_64_30");
		this.MissingTheseNuts(ref west_nuts, "Bush_IslandWest_54_18", "Bush_IslandWest_25_30", "Bush_IslandWest_15_3");
		missing += farming_nuts;
		missing += west_nuts;
		if (west_nuts > 0 && west_unlocked)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_WestHidden", west_nuts));
		}
		int fishing_nuts = 0;
		if (this.MissingLimitedNutDrops(ref fishing_nuts, "IslandFishing", 5))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_IslandFishing", fishing_nuts));
		}
		missing += fishing_nuts;
		int chest_nuts = 0;
		this.MissingLimitedNutDrops(ref chest_nuts, "VolcanoNormalChest");
		this.MissingLimitedNutDrops(ref chest_nuts, "VolcanoRareChest");
		if (chest_nuts > 0)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_VolcanoTreasure", chest_nuts));
		}
		missing += chest_nuts;
		int barrel_nuts = 0;
		if (this.MissingLimitedNutDrops(ref barrel_nuts, "VolcanoBarrel", 5))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_VolcanoBarrel", barrel_nuts));
		}
		missing += barrel_nuts;
		int mining_nuts = 0;
		if (this.MissingLimitedNutDrops(ref mining_nuts, "VolcanoMining", 5))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_VolcanoMining", mining_nuts));
		}
		missing += mining_nuts;
		int monster_nuts = 0;
		if (this.MissingLimitedNutDrops(ref monster_nuts, "VolcanoMonsterDrop", 5))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_VolcanoMonsters", monster_nuts));
		}
		missing += monster_nuts;
		int journal_treasures = 0;
		this.MissingLimitedNutDrops(ref journal_treasures, "Island_N_BuriedTreasureNut");
		this.MissingLimitedNutDrops(ref journal_treasures, "Island_W_BuriedTreasureNut");
		this.MissingLimitedNutDrops(ref journal_treasures, "Island_W_BuriedTreasureNut2");
		if (this.MissingTheseNuts(ref journal_treasures, "Mermaid"))
		{
			journal_treasures += 4;
		}
		this.MissingTheseNuts(ref journal_treasures, "TreeNutShot");
		if (journal_treasures > 0 && Utility.HasAnyPlayerSeenSecretNote(GameLocation.JOURNAL_INDEX + 1))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_Journal", journal_treasures));
		}
		missing += journal_treasures;
		bool resort_unlocked = Game1.MasterPlayer.hasOrWillReceiveMail("Island_Resort");
		int buried_resort_nuts = 0;
		if (this.MissingTheseNuts(ref buried_resort_nuts, "Buried_IslandSouthEastCave_36_26", "Buried_IslandSouthEast_25_17") && resort_unlocked)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_SouthEastBuried", buried_resort_nuts));
		}
		missing += buried_resort_nuts;
		if (this.MissingTheseNuts(ref missing, "StardropPool") && resort_unlocked)
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_StardropPool", 0));
		}
		if (this.MissingTheseNuts(ref missing, "Bush_Caldera_28_36", "Bush_Caldera_9_34"))
		{
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_Caldera", 0));
		}
		this.MissingTheseNuts(ref missing, "Bush_CaptainRoom_2_4");
		if (this.MissingTheseNuts(ref missing, "BananaShrine"))
		{
			missing += 2;
		}
		this.MissingTheseNuts(ref missing, "Bush_IslandEast_17_37");
		this.MissingLimitedNutDrops(ref missing, "Darts", 3);
		int gourmand_missing = 0;
		if (this.MissingTheseNuts(ref gourmand_missing, "IslandGourmand1", "IslandGourmand2", "IslandGourmand3"))
		{
			if (Utility.doesAnyFarmerHaveOrWillReceiveMail("talkedToGourmand"))
			{
				valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_Gourmand", 0));
			}
			gourmand_missing *= 5;
		}
		missing += gourmand_missing;
		if (this.MissingTheseNuts(ref missing, "IslandShrinePuzzle"))
		{
			missing += 4;
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_IslandShrine", 0));
		}
		this.MissingTheseNuts(ref missing, "Bush_IslandShrine_23_34");
		if (!Game1.netWorldState.Value.GoldenCoconutCracked)
		{
			missing++;
			valid_hints.Add(new KeyValuePair<string, int>("Strings\\Locations:NutHint_GoldenCoconut", 0));
		}
		if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
		{
			missing += 5;
		}
		KeyValuePair<string, int>? valid_hint = null;
		if (this.hintForToday.Value == null)
		{
			Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.Date.TotalDays * 642.0);
			if (valid_hints.Count > 0)
			{
				valid_hint = valid_hints[r.Next(valid_hints.Count)];
				this.hintForToday.Value = valid_hint.Value.Key;
			}
		}
		else
		{
			foreach (KeyValuePair<string, int> hint in valid_hints)
			{
				if (hint.Key == this.hintForToday.Value)
				{
					valid_hint = hint;
					break;
				}
			}
		}
		this.hintShowTime = 1.5f;
		this.hintShakeTime = 0.5f;
		this.hintDialogues.Clear();
		this.Squawk();
		if (valid_hint.HasValue)
		{
			this.hintDialogues.Add(Game1.content.LoadString("Strings\\Locations:NutHint_Squawk"));
			this.hintDialogues.Add(Game1.content.LoadString(valid_hint.Value.Key, valid_hint.Value.Value));
			this.hintDialogues.Add(Game1.content.LoadString("Strings\\Locations:NutHint_Squawk"));
		}
		else
		{
			this.hintDialogues.Add(Game1.content.LoadString("Strings\\Locations:NutHint_Squawk"));
		}
		return missing;
	}

	public virtual void Squawk()
	{
		if (base.parrotUpgradePerches.Count > 0)
		{
			base.parrotUpgradePerches[0].ShowInsufficientNuts();
		}
	}

	protected virtual bool MissingLimitedNutDrops(ref int running_total, string key, int count = 1)
	{
		count -= Math.Max(Game1.player.team.GetDroppedLimitedNutCount(key), 0);
		running_total += count;
		return count > 0;
	}

	protected virtual bool MissingTheseNuts(ref int running_total, params string[] keys)
	{
		int missing_nuts = 0;
		foreach (string key in keys)
		{
			if (!Game1.player.team.collectedNutTracker.Contains(key))
			{
				missing_nuts++;
			}
		}
		running_total += missing_nuts;
		return missing_nuts > 0;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.hitTreeEvent.Poll();
		this.parrotBoyEvent.Poll();
		if (this.hintDialogues.Count <= 0)
		{
			return;
		}
		this.hintShowTime -= (float)time.ElapsedGameTime.TotalSeconds;
		this.hintShakeTime -= (float)time.ElapsedGameTime.TotalSeconds;
		if (!(this.hintShowTime <= 0f))
		{
			return;
		}
		this.hintDialogues.RemoveAt(0);
		if (this.hintDialogues.Count > 0)
		{
			if (this.hintDialogues.Count == 2)
			{
				this.hintShowTime = 3f;
			}
			else
			{
				this.hintShowTime = 1.5f;
			}
			this.hintShakeTime = 0.5f;
			this.Squawk();
		}
		else
		{
			this.hintShowTime = -1f;
		}
	}

	public IslandHut()
	{
	}

	public IslandHut(string map, string name)
		: base(map, name)
	{
		base.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(7, 6), new Microsoft.Xna.Framework.Rectangle(-1000, -1000, 1, 1), 1, delegate
		{
			Game1.addMailForTomorrow("Island_FirstParrot", noLetter: true, sendToEveryone: true);
			this.firstParrotDone.Value = true;
			this.parrotBoyEvent.Fire();
		}, () => this.firstParrotDone.Value, "Hut"));
	}

	public override bool performToolAction(Tool t, int tileX, int tileY)
	{
		if (tileX == 10 && tileY == 8 && (t is Pickaxe || t is Axe) && !this.treeHitLocal)
		{
			this.hitTreeEvent.Fire();
		}
		return base.performToolAction(t, tileX, tileY);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		this.hintForToday.Value = null;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.treeNutObtained, "treeNutObtained").AddField(this.hitTreeEvent.NetFields, "hitTreeEvent.NetFields").AddField(this.firstParrotDone, "firstParrotDone")
			.AddField(this.parrotBoyEvent.NetFields, "parrotBoyEvent.NetFields")
			.AddField(this.hintForToday, "hintForToday");
		this.hitTreeEvent.onEvent += SpitTreeNut;
		this.parrotBoyEvent.onEvent += ParrotBoyEvent_onEvent;
	}

	private void ParrotBoyEvent_onEvent()
	{
		if (Game1.player.currentLocation.Equals(this) && !Game1.IsFading())
		{
			Game1.addMailForTomorrow("sawParrotBoyIntro", noLetter: true);
			Game1.globalFadeToBlack(delegate
			{
				this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandHut_Event_ParrotBoyIntro")));
			});
		}
		else if (Game1.locationRequest?.Location?.NameOrUniqueName == base.NameOrUniqueName && !Game1.warpingForForcedRemoteEvent)
		{
			Game1.addMailForTomorrow("sawParrotBoyIntro", noLetter: true);
			this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandHut_Event_ParrotBoyIntro")));
		}
	}

	public virtual void SpitTreeNut()
	{
		if (this.treeHitLocal)
		{
			return;
		}
		this.treeHitLocal = true;
		if (Game1.currentLocation == this)
		{
			Game1.playSound("boulderBreak");
			DelayedAction.playSoundAfterDelay("croak", 300);
			DelayedAction.playSoundAfterDelay("slimeHit", 1250);
			DelayedAction.playSoundAfterDelay("coin", 1250);
		}
		TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(5, new Vector2(10f, 5f) * 64f, Color.White);
		sprite.motion = new Vector2(0f, -1.5f);
		sprite.interval = 25f;
		sprite.delayBeforeAnimationStart = 1250;
		base.temporarySprites.Add(sprite);
		sprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(32, 192, 16, 32), 1250f, 1, 1, new Vector2(10f, 7f) * 64f, flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f);
		sprite.shakeIntensity = 1f;
		base.temporarySprites.Add(sprite);
		sprite = new TemporaryAnimatedSprite(46, new Vector2(10f, 5f) * 64f, Color.White);
		sprite.motion = new Vector2(0f, -3f);
		sprite.interval = 25f;
		sprite.delayBeforeAnimationStart = 1250;
		base.temporarySprites.Add(sprite);
		for (int i = 0; i < 5; i++)
		{
			sprite = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(352, 1200, 16, 16), 50f, 11, 3, new Vector2(10f, 5f) * 64f, flicker: false, flipped: false, 0.1f, 0.01f, Color.White, 4f, 0f, 0f, 0f);
			sprite.motion.X = Utility.RandomFloat(-3f, 3f);
			sprite.motion.Y = Utility.RandomFloat(-1f, -3f);
			sprite.acceleration.Y = 0.05f;
			sprite.delayBeforeAnimationStart = 1250;
			base.temporarySprites.Add(sprite);
		}
		if (Game1.IsMasterGame && !this.treeNutObtained.Value)
		{
			Game1.player.team.MarkCollectedNut("TreeNut");
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(10.5f, 7f) * 64f, 0, this, 0);
			}, 1250);
			this.treeNutObtained.Value = true;
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is IslandHut location)
		{
			this.treeNutObtained.Value = location.treeNutObtained.Value;
			this.firstParrotDone.Value = location.firstParrotDone.Value;
			this.hintForToday.Value = location.hintForToday.Value;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (this.hintDialogues.Count > 0)
		{
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(7.25f, 3f) * 64f);
			if (this.hintShakeTime > 0f)
			{
				position.X += Utility.RandomFloat(-1f, 1f);
				position.Y += Utility.RandomFloat(-1f, 1f);
			}
			SpriteText.drawStringWithScrollCenteredAt(b, this.hintDialogues[0], (int)position.X, (int)position.Y, "", Math.Min(1f, this.hintShowTime * 2f), null, 1, 1f);
		}
		base.drawAboveAlwaysFrontLayer(b);
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		this.hintDialogues.Clear();
		this.hintShowTime = -1f;
		this.treeHitLocal = this.treeNutObtained.Value;
		if (Game1.netWorldState.Value.GoldenWalnutsFound < 10)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(5.15f, 2.25f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 777,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 9999f,
				animationLength = 1,
				layerDepth = 0.95f,
				drawAboveAlwaysFront = true
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\ParrotBoy", new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 32), new Vector2(5f, 0.5f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 777,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 9999f,
				animationLength = 1,
				layerDepth = 1f,
				drawAboveAlwaysFront = true
			});
		}
		if (this.firstParrotDone.Value && !Game1.MasterPlayer.hasOrWillReceiveMail("addedParrotBoy") && !Game1.player.hasOrWillReceiveMail("sawParrotBoyIntro"))
		{
			this.ParrotBoyEvent_onEvent();
		}
	}
}
