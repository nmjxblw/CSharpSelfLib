using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.GameData.Shops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class DesertFestival : Desert
{
	public enum RaceState
	{
		PreRace,
		StartingLine,
		Ready,
		Set,
		Go,
		AnnounceWinner,
		AnnounceWinner2,
		AnnounceWinner3,
		AnnounceWinner4,
		RaceEnd,
		RacesOver
	}

	public const int CALICO_STATUE_GHOST_INVASION = 0;

	public const int CALICO_STATUE_SERPENT_INVASION = 1;

	public const int CALICO_STATUE_SKELETON_INVASION = 2;

	public const int CALICO_STATUE_BAT_INVASION = 3;

	public const int CALICO_STATUE_ASSASSIN_BUGS = 4;

	public const int CALICO_STATUE_THIN_SHELLS = 5;

	public const int CALICO_STATUE_MEAGER_MEALS = 6;

	public const int CALICO_STATUE_MONSTER_SURGE = 7;

	public const int CALICO_STATUE_SHARP_TEETH = 8;

	public const int CALICO_STATUE_MUMMY_CURSE = 9;

	public const int CALICO_STATUE_SPEED_BOOST = 10;

	public const int CALICO_STATUE_REFRESH = 11;

	public const int CALICO_STATUE_50_EGG_TREASURE = 12;

	public const int CALICO_STATUE_NO_EFFECT = 13;

	public const int CALICO_STATUE_TOOTH_FILE = 14;

	public const int CALICO_STATUE_25_EGG_TREASURE = 15;

	public const int CALICO_STATUE_10_EGG_TREASURE = 16;

	public const int CALICO_STATUE_100_EGG_TREASURE = 17;

	public static readonly int[] CalicoStatueInvasionIds = new int[4] { 3, 0, 1, 2 };

	public const int NUM_SCHOLAR_QUESTIONS = 4;

	public const string FISHING_QUEST_ID = "98765";

	protected RandomizedPlantFurniture _cactusGuyRevealItem;

	protected float _cactusGuyRevealTimer = -1f;

	protected float _cactusShakeTimer = -1f;

	protected int _currentlyShownCactusID;

	protected NetEvent1Field<int, NetInt> _revealCactusEvent = new NetEvent1Field<int, NetInt>();

	protected NetEvent1Field<int, NetInt> _hideCactusEvent = new NetEvent1Field<int, NetInt>();

	protected MoneyDial eggMoneyDial;

	[XmlIgnore]
	public NetList<Racer, NetRef<Racer>> netRacers = new NetList<Racer, NetRef<Racer>>();

	[XmlIgnore]
	protected List<Racer> _localRacers = new List<Racer>();

	[XmlIgnore]
	protected float festivalChimneyTimer;

	[XmlIgnore]
	public List<int> finishedRacers = new List<int>();

	[XmlIgnore]
	public int racerCount = 3;

	[XmlIgnore]
	public int totalRacers = 5;

	[XmlIgnore]
	public NetEvent1Field<string, NetString> announceRaceEvent = new NetEvent1Field<string, NetString>();

	[XmlIgnore]
	public NetEnum<RaceState> currentRaceState = new NetEnum<RaceState>(RaceState.PreRace);

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> sabotages = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> raceGuesses = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> nextRaceGuesses = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetLongDictionary<bool, NetBool> specialRewardsCollected = new NetLongDictionary<bool, NetBool>();

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> rewardsToCollect = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetInt lastRaceWinner = new NetInt();

	[XmlIgnore]
	protected float _raceStateTimer;

	protected string _raceText;

	protected float _raceTextTimer;

	protected bool _raceTextShake;

	protected int _localSabotageText = -1;

	protected int _currentScholarQuestion = -1;

	protected int _cookIngredient = -1;

	protected int _cookSauce = -1;

	public Vector3[][] raceTrack = new Vector3[16][]
	{
		new Vector3[2]
		{
			new Vector3(41f, 39f, 0f),
			new Vector3(42f, 39f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(41f, 29f, 0f),
			new Vector3(42f, 28f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(6f, 29f, 0f),
			new Vector3(5f, 28f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(6f, 35f, 0f),
			new Vector3(5f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(10f, 35f, 2f),
			new Vector3(10f, 36f, 2f)
		},
		new Vector3[2]
		{
			new Vector3(12.5f, 35f, 0f),
			new Vector3(12.5f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(17.5f, 35f, 1f),
			new Vector3(17.5f, 36f, 1f)
		},
		new Vector3[2]
		{
			new Vector3(23.5f, 35f, 0f),
			new Vector3(23.5f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(28.5f, 35f, 1f),
			new Vector3(28.5f, 36f, 1f)
		},
		new Vector3[2]
		{
			new Vector3(31f, 35f, 0f),
			new Vector3(31f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 35f, 0f),
			new Vector3(31f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 38f, 3f),
			new Vector3(31f, 38f, 3f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 43f, 0f),
			new Vector3(31f, 43f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 46f, 0f),
			new Vector3(31f, 47f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(41f, 46f, 0f),
			new Vector3(42f, 47f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(41f, 39f, 0f),
			new Vector3(42f, 39f, 0f)
		}
	};

	private bool checkedMineExplanation;

	public DesertFestival()
	{
		base.forceLoadPathLayerLights = true;
	}

	public DesertFestival(string mapPath, string name)
		: base(mapPath, name)
	{
		base.forceLoadPathLayerLights = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this._revealCactusEvent, "_revealCactusEvent").AddField(this._hideCactusEvent, "_hideCactusEvent").AddField(this.netRacers, "netRacers")
			.AddField(this.announceRaceEvent, "announceRaceEvent")
			.AddField(this.sabotages, "sabotages")
			.AddField(this.raceGuesses, "raceGuesses")
			.AddField(this.rewardsToCollect, "rewardsToCollect")
			.AddField(this.specialRewardsCollected, "specialRewardsCollected")
			.AddField(this.nextRaceGuesses, "nextRaceGuesses")
			.AddField(this.lastRaceWinner, "lastRaceWinner")
			.AddField(this.currentRaceState, "currentRaceState");
		this._revealCactusEvent.onEvent += CactusGuyRevealCactus;
		this._hideCactusEvent.onEvent += CactusGuyHideCactus;
		this.announceRaceEvent.onEvent += AnnounceRace;
	}

	public static void SetupMerchantSchedule(NPC character, int shop_index)
	{
		StringBuilder schedule = new StringBuilder();
		if (shop_index == 0)
		{
			schedule.Append("/a1130 Desert 15 40 2");
		}
		else
		{
			schedule.Append("/a1140 Desert 26 40 2");
		}
		schedule.Append("/2400 bed");
		schedule.Remove(0, 1);
		GameLocation defaultMap = Game1.getLocationFromName(character.DefaultMap);
		if (defaultMap != null)
		{
			Game1.warpCharacter(character, defaultMap, new Vector2((int)(character.DefaultPosition.X / 64f), (int)(character.DefaultPosition.Y / 64f)));
		}
		character.islandScheduleName.Value = "festival_vendor";
		character.TryLoadSchedule("desertFestival", schedule.ToString());
		character.performSpecialScheduleChanges();
	}

	public override void OnCamel()
	{
		Game1.playSound("camel");
		this.ShowCamelAnimation();
		Game1.player.faceDirection(0);
		Game1.haltAfterCheck = false;
	}

	public override void ShowCamelAnimation()
	{
		base.temporarySprites.Add(new TemporaryAnimatedSprite
		{
			texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
			sourceRect = new Microsoft.Xna.Framework.Rectangle(273, 524, 65, 49),
			sourceRectStartingPos = new Vector2(273f, 524f),
			animationLength = 1,
			totalNumberOfLoops = 1,
			interval = 300f,
			scale = 4f,
			position = new Vector2(536f, 340f) * 4f,
			layerDepth = 0.1332f,
			id = 999
		});
	}

	public override void checkForMusic(GameTime time)
	{
		Game1.changeMusicTrack(this.GetFestivalMusic(), track_interruptable: true);
	}

	public virtual string GetFestivalMusic()
	{
		if (Utility.IsPassiveFestivalOpen("DesertFestival"))
		{
			return "event2";
		}
		return "summer_day_ambient";
	}

	public override string GetLocationSpecificMusic()
	{
		return this.GetFestivalMusic();
	}

	public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
	{
		Random r = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation);
		Game1.createMultipleObjectDebris("CalicoEgg", xLocation, yLocation, r.Next(3, 7), who.UniqueMultiplayerID, this);
		base.digUpArtifactSpot(xLocation, yLocation, who);
	}

	public virtual void CollectRacePrizes()
	{
		List<Item> rewards = new List<Item>();
		if (this.specialRewardsCollected.TryGetValue(Game1.player.UniqueMultiplayerID, out var collectedSpecialReward) && !collectedSpecialReward)
		{
			this.specialRewardsCollected[Game1.player.UniqueMultiplayerID] = true;
			rewards.Add(ItemRegistry.Create("CalicoEgg", 100));
		}
		for (int i = 0; i < this.rewardsToCollect[Game1.player.UniqueMultiplayerID]; i++)
		{
			rewards.Add(ItemRegistry.Create("CalicoEgg", 20));
		}
		this.rewardsToCollect[Game1.player.UniqueMultiplayerID] = 0;
		Game1.activeClickableMenu = new ItemGrabMenu(rewards, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, this);
	}

	public override void performTouchAction(string full_action_string, Vector2 player_standing_position)
	{
		if (Game1.eventUp)
		{
			return;
		}
		if (full_action_string.Split(' ')[0] == "DesertMakeover")
		{
			if (Game1.player.controller != null)
			{
				return;
			}
			bool fail = false;
			string failMessageKey = null;
			NPC stylist = this.GetStylist();
			if (!fail && stylist == null)
			{
				stylist = null;
				failMessageKey = "Strings\\1_6_Strings:MakeOver_NoStylist";
				fail = true;
			}
			if (!fail && Game1.player.activeDialogueEvents.ContainsKey("DesertMakeover"))
			{
				failMessageKey = "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_AlreadyStyled";
				fail = true;
			}
			int required_space = 0;
			if (Game1.player.hat.Value != null)
			{
				required_space++;
			}
			if (Game1.player.shirtItem.Value != null)
			{
				required_space++;
			}
			if (Game1.player.pantsItem.Value != null)
			{
				required_space++;
			}
			if (!fail && Game1.player.freeSpotsInInventory() < required_space)
			{
				failMessageKey = "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_InventoryFull";
				fail = true;
			}
			if (fail)
			{
				Game1.freezeControls = true;
				Game1.displayHUD = false;
				int end_direction = 2;
				if (stylist != null)
				{
					end_direction = 3;
				}
				Game1.player.controller = new PathFindController(Game1.player, this, new Point(26, 52), end_direction, delegate
				{
					Game1.freezeControls = false;
					Game1.displayHUD = true;
					if (stylist != null)
					{
						stylist.faceTowardFarmerForPeriod(1000, 2, faceAway: false, Game1.player);
						if (failMessageKey != null)
						{
							Game1.DrawDialogue(stylist, failMessageKey);
						}
					}
					else if (failMessageKey != null)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString(failMessageKey));
					}
				});
			}
			else
			{
				Game1.player.activeDialogueEvents["DesertMakeover"] = 0;
				Game1.freezeControls = true;
				Game1.displayHUD = false;
				Game1.player.controller = new PathFindController(Game1.player, this, new Point(27, 50), 0);
				Game1.globalFadeToBlack(delegate
				{
					Game1.freezeControls = false;
					Game1.forceSnapOnNextViewportUpdate = true;
					Event obj = new Event(this.GetMakeoverEvent());
					obj.onEventFinished = (Action)Delegate.Combine(obj.onEventFinished, new Action(ReceiveMakeOver));
					this.startEvent(obj);
					Game1.globalFadeToClear();
				});
			}
		}
		else
		{
			base.performTouchAction(full_action_string, player_standing_position);
		}
	}

	public virtual string GetMakeoverEvent()
	{
		NPC stylist = this.GetStylist();
		Random r = Utility.CreateDaySaveRandom(Game1.year);
		StringBuilder sb = new StringBuilder();
		sb.Append("continue/26 51/farmer 27 50 2 ");
		foreach (NPC npc in base.characters)
		{
			if (!(npc.Name == stylist.Name) && !(npc.Name == "Sandy"))
			{
				sb.Append(npc.Name + " " + npc.Tile.X + " " + npc.Tile.Y + " " + npc.FacingDirection + " ");
			}
		}
		if (stylist.Name == "Emily")
		{
			sb.Append("Emily 25 52 2 Sandy 22 52 2/skippable/pause 1200/speak Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_1"));
			sb.Append("\"/pause 100/");
			switch (r.Next(0, 3))
			{
			case 0:
				sb.Append("animate Emily false true 200 39 39/");
				break;
			case 1:
				sb.Append("animate Emily false true 300 16 17 18 19 20 21 22 23/");
				break;
			case 2:
				sb.Append("animate Emily false true 300 31 48 49/");
				break;
			}
			sb.Append("pause 1000/faceDirection Sandy 1 true/pause 2000/textAboveHead Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_2"));
			sb.Append("\"/pause 3000/stopAnimation Emily 2/playSound dwop/shake Emily 100/jump Emily 4/pause 300/speak Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_3"));
			sb.Append("\"/pause 100/advancedMove Emily false 1 0 0 -1 0 -1 0 -1 1 100/pause 100/");
			sb.Append("advancedMove Sandy false 1 0 1 0 1 0 1 0 2 100/pause 3000/playSound openChest/pause 1000/");
			List<string> reactions = new List<string>
			{
				string.Format("playSound dustMeep/pause 300/playSound dustMeep/pause 300/playSound dustMeep/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction1")),
				string.Format("playSound rooster/playSound dwop/shake Sandy 400/jump Sandy 4/pause 500/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction2")),
				string.Format("playSound slimeHit/pause 300/playSound slimeHit/pause 600/playSound slimedead/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction3")),
				string.Format("textAboveHead Emily \"{0}\"/playSound trashcanlid/pause 1000/playSound trashcan/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction4")),
				string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound cast/pause 500/playSound axe/pause 200/playSound ow/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction5")),
				string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound eat/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction6")),
				string.Format("textAboveHead Emily \"{0}\"/playSound scissors/pause 300/playSound scissors/pause 300/playSound scissors/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction7")),
				string.Format("textAboveHead Emily \"{0}\"/pause 500/playSound trashbear/pause 300/playSound trashbear/pause 300/playSound trashbear/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction8")),
				string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound fishingRodBend/pause 500/playSound fishingRodBend/pause 1000/playSound fishingRodBend/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction9"))
			};
			Utility.Shuffle(r, reactions);
			for (int i = 0; i < 3; i++)
			{
				sb.Append("pause 500/");
				sb.Append(reactions[i]);
				sb.Append("pause 1500/");
			}
			sb.Append("pause 500/playSound money/textAboveHead Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_4"));
			sb.Append("\"/playSound dwop/shake Sandy 400/jump Sandy 4/pause 750/advancedMove Sandy false -1 0 -1 0 -1 0 -1 0 1 100/pause 2000/advancedMove Emily false 0 1 0 1 0 1 2 100/pause 2000/speak Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_5"));
		}
		else
		{
			sb.Append("Sandy 22 52 2/skippable/pause 2000/textAboveHead Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_1"));
			sb.Append("\"/");
			sb.Append("pause 1000/playSound dwop/shake Sandy 400/jump Sandy 4/textAboveHead Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_2"));
			sb.Append("\"/");
			sb.Append("pause 200/advancedMove Sandy false 1 0 1 0 1 0 1 0 4 100/");
			sb.Append("pause 2500/speak Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_3"));
			sb.Append("\"/");
			sb.Append("pause 500/advancedMove Sandy false 0 -1 0 -1 0 -1/pause 3000/playSound openChest/pause 1000/");
			sb.Append(string.Format("textAboveHead Sandy \"{0}\"/pause 1000/playSound fishingRodBend/pause 500/playSound fishingRodBend/pause 1000/playSound fishingRodBend/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_4")));
			sb.Append("pause 1500/");
			sb.Append("pause 500/playSound money/textAboveHead Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_5"));
			sb.Append("\"/pause 200/advancedMove Sandy false 0 1 0 1 0 1 2 100/pause 2000/speak Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_6"));
		}
		sb.Append("\"/pause 500/end");
		return sb.ToString();
	}

	private void ReceiveMakeOver()
	{
		this.ReceiveMakeOver(-1);
	}

	public virtual void ReceiveMakeOver(int randomSeedOverride = -1)
	{
		Random r = ((randomSeedOverride == -1) ? Utility.CreateDaySaveRandom(Game1.year) : Utility.CreateRandom(randomSeedOverride));
		if (randomSeedOverride == -1 && r.NextDouble() < 0.75)
		{
			r = Utility.CreateDaySaveRandom(Game1.year, (int)Game1.player.uniqueMultiplayerID.Value);
		}
		List<MakeoverOutfit> makeoverOutfits = DataLoader.MakeoverOutfits(Game1.content);
		if (makeoverOutfits == null)
		{
			return;
		}
		List<MakeoverOutfit> valid_outfits = new List<MakeoverOutfit>(makeoverOutfits);
		for (int i = 0; i < valid_outfits.Count; i++)
		{
			MakeoverOutfit outfit = valid_outfits[i];
			if (outfit.Gender.HasValue && outfit.Gender.Value != Game1.player.Gender)
			{
				valid_outfits.RemoveAt(i);
				i--;
				continue;
			}
			bool match = false;
			foreach (MakeoverItem outfitPart in outfit.OutfitParts)
			{
				if (outfitPart.MatchesGender(Game1.player.Gender))
				{
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(outfitPart.ItemId);
					match = Game1.player.hat.Value?.QualifiedItemId == itemData.QualifiedItemId || Game1.player.shirtItem.Value?.QualifiedItemId == itemData.QualifiedItemId;
					if (match)
					{
						break;
					}
				}
			}
			if (match)
			{
				valid_outfits.RemoveAt(i);
				i--;
			}
		}
		Farmer player = Game1.player;
		foreach (Item item2 in new List<Item>
		{
			player.Equip(null, player.shirtItem),
			player.Equip(null, player.pantsItem),
			player.Equip(null, player.hat)
		})
		{
			Item clothes = Utility.PerformSpecialItemGrabReplacement(item2);
			if (clothes != null && player.addItemToInventory(clothes) != null)
			{
				player.team.returnedDonations.Add(clothes);
				player.team.newLostAndFoundItems.Value = true;
			}
		}
		MakeoverOutfit selectedOutfit = r.ChooseFrom(valid_outfits);
		Random togaRandom = Utility.CreateDaySaveRandom();
		if (Utility.GetDayOfPassiveFestival("DesertFestival") == 2 && togaRandom.NextDouble() < 0.03)
		{
			selectedOutfit = new MakeoverOutfit
			{
				OutfitParts = new List<MakeoverItem>
				{
					new MakeoverItem
					{
						ItemId = "(H)LaurelWreathCrown"
					},
					new MakeoverItem
					{
						ItemId = "(P)3",
						Color = "247 245 205"
					},
					new MakeoverItem
					{
						ItemId = "(S)1199"
					}
				}
			};
		}
		if (selectedOutfit?.OutfitParts == null)
		{
			return;
		}
		bool appliedHat = false;
		bool appliedShirt = false;
		bool appliedPants = false;
		foreach (MakeoverItem part in selectedOutfit.OutfitParts)
		{
			if (!part.MatchesGender(Game1.player.Gender))
			{
				continue;
			}
			Item item = ItemRegistry.Create(part.ItemId);
			if (!(item is Hat hat))
			{
				if (!(item is Clothing clothing))
				{
					continue;
				}
				Color? color = Utility.StringToColor(part.Color);
				if (color.HasValue)
				{
					clothing.clothesColor.Value = color.Value;
				}
				switch (clothing.clothesType.Value)
				{
				case Clothing.ClothesType.PANTS:
					if (!appliedPants)
					{
						player.Equip(clothing, player.pantsItem);
						appliedPants = true;
					}
					break;
				case Clothing.ClothesType.SHIRT:
					if (!appliedShirt)
					{
						player.Equip(clothing, player.shirtItem);
						appliedShirt = true;
					}
					break;
				}
			}
			else if (!appliedHat)
			{
				player.Equip(hat, player.hat);
				appliedHat = true;
			}
		}
	}

	public virtual void AfterMakeOver()
	{
		Game1.player.canOnlyWalk = false;
		Game1.freezeControls = false;
		Game1.displayHUD = true;
		NPC stylist = this.GetStylist();
		if (stylist != null)
		{
			Game1.DrawDialogue(stylist, "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_Done");
			stylist.faceTowardFarmerForPeriod(1000, 2, faceAway: false, Game1.player);
		}
	}

	public NPC GetStylist()
	{
		NPC stylist = base.getCharacterFromName("Emily");
		if (stylist != null && stylist.TilePoint == new Point(25, 52))
		{
			return stylist;
		}
		stylist = base.getCharacterFromName("Sandy");
		if (stylist != null && stylist.TilePoint == new Point(22, 52))
		{
			NPC emily = base.getCharacterFromName("Emily");
			if (emily != null && emily.islandScheduleName.Value == "festival_vendor")
			{
				return stylist;
			}
		}
		return null;
	}

	public static void addCalicoStatueSpeedBuff()
	{
		BuffEffects speedBuff = new BuffEffects();
		speedBuff.Speed.Value = 1f;
		Game1.player.applyBuff(new Buff("CalicoStatueSpeed", "Calico Statue", Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue"), 300000, Game1.buffsIcons, 9, speedBuff, false, Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Name_10")));
	}

	public override bool performAction(string action, Farmer who, Location tile_location)
	{
		string festival_id = "DesertFestival";
		DataLoader.Shops(Game1.content);
		switch (action)
		{
		case "DesertFestivalMineExplanation":
			Game1.player.mailReceived.Add("Checked_DF_Mine_Explanation");
			this.checkedMineExplanation = true;
			Game1.multipleDialogues(new string[3]
			{
				Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation"),
				Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation_2"),
				Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation_3")
			});
			break;
		case "DesertFishingBoard":
			if (Game1.Date != who.lastDesertFestivalFishingQuest.Value)
			{
				List<Response> responses = new List<Response>
				{
					new Response("Yes", Game1.content.LoadString("Strings\\1_6_Strings:Accept")),
					new Response("No", Game1.content.LoadString("Strings\\1_6_Strings:Decline"))
				};
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Willy_DesertFishing" + Utility.GetDayOfPassiveFestival("DesertFestival")), responses.ToArray(), "Fishing_Quest");
			}
			break;
		case "DesertVendor":
		{
			Game1.player.faceDirection(0);
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				return false;
			}
			Microsoft.Xna.Framework.Rectangle shop_tile_rect = new Microsoft.Xna.Framework.Rectangle(tile_location.X, tile_location.Y - 1, 1, 1);
			foreach (NPC npc in base.characters)
			{
				if (shop_tile_rect.Contains(npc.TilePoint) && Utility.TryOpenShopMenu(festival_id + "_" + npc.Name, npc.Name))
				{
					return true;
				}
			}
			break;
		}
		case "DesertCactusMan":
			Game1.player.faceDirection(0);
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Closed"));
			}
			else if (Game1.player.isInventoryFull())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Full"));
			}
			else if (!Game1.player.mailReceived.Contains(this.GetCactusMail()))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Intro_" + Game1.random.Next(1, 4)));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Question"), base.createYesNoResponses(), "CactusMan");
				});
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Collected"));
			}
			break;
		case "DesertEggShop":
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:EggShop_Closed"));
			}
			else
			{
				Utility.TryOpenShopMenu("DesertFestival_EggShop", "Vendor");
			}
			break;
		case "DesertRacerMan":
		{
			Game1.player.faceGeneralDirection(new Vector2((float)tile_location.X + 0.5f, (float)tile_location.Y + 0.5f) * 64f);
			int toCollect;
			int guessedRacer2;
			if (this.specialRewardsCollected.TryGetValue(Game1.player.UniqueMultiplayerID, out var collectedSpecialReward) && !collectedSpecialReward)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Collect_Prize_Special"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(CollectRacePrizes));
			}
			else if (this.rewardsToCollect.TryGetValue(Game1.player.UniqueMultiplayerID, out toCollect) && toCollect > 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Collect_Prize"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(CollectRacePrizes));
			}
			else if (!Utility.IsPassiveFestivalOpen(festival_id) && Game1.timeOfDay < 1000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Closed"));
			}
			else if (this.currentRaceState.Value >= RaceState.Go && this.currentRaceState.Value < RaceState.AnnounceWinner4)
			{
				if (this.raceGuesses.TryGetValue(Game1.player.UniqueMultiplayerID, out var guessedRacer) && this.currentRaceState.Value == RaceState.Go)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Already_Made", Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + guessedRacer)));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Ongoing"));
				}
			}
			else if (!this.CanMakeAnotherRaceGuess())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Ended"));
			}
			else if (this.nextRaceGuesses.TryGetValue(Game1.player.UniqueMultiplayerID, out guessedRacer2))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Already_Made", Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + guessedRacer2)));
			}
			else
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Question"), base.createYesNoResponses(), "Race");
			}
			return true;
		}
		case "DesertShadyGuy":
			Game1.player.faceDirection(0);
			if (!Utility.IsPassiveFestivalOpen(festival_id) && Game1.timeOfDay < 1000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Closed"));
			}
			if (this.currentRaceState.Value >= RaceState.Go && this.currentRaceState.Value < RaceState.AnnounceWinner4)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Ongoing"));
			}
			else if (!this.CanMakeAnotherRaceGuess())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Ended"));
			}
			else if (this.sabotages.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				this.ShowSabotagedRaceText();
			}
			else if (!Game1.player.mailReceived.Contains("Desert_Festival_Shady_Guy"))
			{
				Game1.player.mailReceived.Add("Desert_Festival_Shady_Guy");
				Game1.multipleDialogues(new string[3]
				{
					Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro"),
					Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro_2"),
					Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro_3")
				});
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy"), base.createYesNoResponses(), "Shady_Guy");
				});
			}
			else
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_2nd"), base.createYesNoResponses(), "Shady_Guy");
			}
			return true;
		case "DesertGil":
			if (Game1.Date == who.lastGotPrizeFromGil.Value)
			{
				if (Utility.GetDayOfPassiveFestival("DesertFestival") == 3)
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_NextYear");
				}
				else
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_ComeBack");
				}
			}
			else if (Game1.player.team.highestCalicoEggRatingToday.Value == 0)
			{
				Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_NoRating");
			}
			else
			{
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Gil_SubmitRating", Game1.player.team.highestCalicoEggRatingToday.Value + 1), base.createYesNoResponses(), "Gil_EggRating");
			}
			return true;
		case "DesertMarlon":
		{
			if (!Game1.player.mailReceived.Contains("Desert_Festival_Marlon"))
			{
				Game1.player.mailReceived.Add("Desert_Festival_Marlon");
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Intro");
				break;
			}
			bool order_chosen = false;
			bool order_complete = false;
			if (Game1.player.team.acceptedSpecialOrderTypes.Contains("DesertFestivalMarlon"))
			{
				order_complete = true;
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.orderType.Value == "DesertFestivalMarlon")
					{
						order_chosen = true;
						if (order.questState.Value == SpecialOrderStatus.InProgress || order.questState.Value == SpecialOrderStatus.Failed)
						{
							order_complete = false;
						}
						break;
					}
				}
			}
			if (order_complete)
			{
				if (Utility.GetDayOfPassiveFestival("DesertFestival") < 3)
				{
					Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Challenge_Finished");
					return true;
				}
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Challenge_Finished_LastDay");
				return true;
			}
			if (order_chosen)
			{
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Challenge_Chosen");
			}
			else
			{
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_" + Game1.random.Next(1, 5));
			}
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				Game1.activeClickableMenu = new SpecialOrdersBoard("DesertFestivalMarlon");
			});
			return true;
		}
		case "DesertScholar":
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Closed"));
				return true;
			}
			if (Game1.player.mailReceived.Contains(this.GetScholarMail()))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_DoneThisYear"));
				return true;
			}
			if (this._currentScholarQuestion == -2)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Failed"));
				return true;
			}
			base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Intro"), base.createYesNoResponses(), "DesertScholar");
			break;
		case "DesertFood":
			Game1.player.faceDirection(0);
			base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro"), base.createYesNoResponses(), "Cook_Intro");
			break;
		}
		return base.performAction(action, who, tile_location);
	}

	public string GetCactusMail()
	{
		return "Y" + Game1.year + "_Cactus";
	}

	public string GetScholarMail()
	{
		return "Y" + Game1.year + "_Scholar";
	}

	public virtual Response[] GetRacerResponses()
	{
		List<Response> responses = new List<Response>();
		foreach (Racer racer in this.netRacers)
		{
			responses.Add(new Response(racer.racerIndex.ToString(), Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + racer.racerIndex.Value)));
		}
		responses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
		return responses.ToArray();
	}

	public virtual void ShowSabotagedRaceText()
	{
		if (this.sabotages.TryGetValue(Game1.player.UniqueMultiplayerID, out var sabotagedRacer))
		{
			if (this._localSabotageText == -1)
			{
				this._localSabotageText = Game1.random.Next(1, 4);
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Selected_" + this._localSabotageText, Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + sabotagedRacer)));
		}
	}

	private void generateNextScholarQuestion()
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame);
		int whichQuestion = r.Next(3);
		whichQuestion += Game1.year;
		whichQuestion %= 3;
		string questionKey = "Scholar_Question_" + this._currentScholarQuestion + "_" + whichQuestion;
		string optionsKey = "Scholar_Question_" + this._currentScholarQuestion + "_" + whichQuestion + "_Options";
		string answersKey = "Scholar_Question_" + this._currentScholarQuestion + "_" + whichQuestion + "_Answers";
		string[] options = null;
		int optionIndex = 0;
		try
		{
			options = Game1.content.LoadString("Strings\\1_6_Strings:" + optionsKey).Split(',');
			optionIndex = r.Next(options.Length);
		}
		catch (Exception)
		{
		}
		string[] answers = Game1.content.LoadString("Strings\\1_6_Strings:" + answersKey).Split(',');
		string question = ((options != null) ? Game1.content.LoadString("Strings\\1_6_Strings:" + questionKey, options[optionIndex]) : Game1.content.LoadString("Strings\\1_6_Strings:" + questionKey));
		List<Response> choices = new List<Response>();
		if (this._currentScholarQuestion == 2 && whichQuestion == 1)
		{
			choices.Add(new Response("Correct", Game1.stats.StepsTaken.ToString() ?? ""));
			choices.Add(new Response("Wrong", (Game1.stats.StepsTaken * 2).ToString() ?? ""));
			choices.Add(new Response("Wrong", (Game1.stats.StepsTaken / 2).ToString() ?? ""));
		}
		else
		{
			choices.Add(new Response("Correct", answers[optionIndex]));
			int index;
			for (index = optionIndex; index == optionIndex; index = r.Next(answers.Length))
			{
			}
			choices.Add(new Response("Wrong", answers[index]));
			int index2 = optionIndex;
			while (index2 == optionIndex || index2 == index)
			{
				index2 = r.Next(answers.Length);
			}
			choices.Add(new Response("Wrong", answers[index2]));
		}
		Utility.Shuffle(r, choices);
		base.createQuestionDialogue(question, choices.ToArray(), "DesertScholar_Answer_");
		this._currentScholarQuestion++;
	}

	public override void customQuestCompleteBehavior(string questId)
	{
		if (questId == "98765")
		{
			switch (Utility.GetDayOfPassiveFestival("DesertFestival"))
			{
			case 1:
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 25));
				break;
			case 2:
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 50));
				break;
			case 3:
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 30));
				break;
			}
		}
		base.customQuestCompleteBehavior(questId);
	}

	public override bool answerDialogueAction(string question_and_answer, string[] question_params)
	{
		switch (question_and_answer)
		{
		case null:
			return false;
		case "WarperQuestion_Yes":
			if (Game1.player.Money < 250)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 250;
				Game1.player.CanMove = true;
				ItemRegistry.Create<Object>("(O)688").performUseAction(this);
				Game1.player.freezePause = 5000;
			}
			return true;
		case "Fishing_Quest_Yes":
		{
			Quest q = null;
			q = ((Utility.GetDayOfPassiveFestival("DesertFestival") != 3) ? ((Quest)new FishingQuest((Utility.GetDayOfPassiveFestival("DesertFestival") == 1) ? "164" : "165", (Utility.GetDayOfPassiveFestival("DesertFestival") != 1) ? 1 : 3, "Willy", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge"), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Description_" + Utility.GetDayOfPassiveFestival("DesertFestival")), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Return_" + Utility.GetDayOfPassiveFestival("DesertFestival")))) : ((Quest)new ItemDeliveryQuest("Willy", "GoldenBobber", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge"), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Description_" + Utility.GetDayOfPassiveFestival("DesertFestival")), "Strings\\1_6_Strings:Willy_GoldenBobber", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Return_" + Utility.GetDayOfPassiveFestival("DesertFestival")))));
			q.daysLeft.Value = 1;
			q.id.Value = "98765";
			Game1.player.questLog.Add(q);
			Game1.player.lastDesertFestivalFishingQuest.Value = Game1.Date;
			return true;
		}
		case "Gil_EggRating_Yes":
			Game1.player.lastGotPrizeFromGil.Value = Game1.Date;
			Game1.player.freezePause = 1400;
			DelayedAction.playSoundAfterDelay("coin", 500);
			DelayedAction.functionAfterDelay(delegate
			{
				int num = Game1.player.team.highestCalicoEggRatingToday.Value + 1;
				int eggPrize = 0;
				Item extraPrize = null;
				if (num >= 1000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Gil_Rating_1000"));
				}
				else if (num >= 55)
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_Rating_50", num);
					eggPrize = 500;
					extraPrize = new Object("279", 1);
				}
				else if (num >= 25)
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_Rating_25", num);
					eggPrize = 200;
					if (!Game1.player.mailReceived.Contains("DF_Gil_Hat"))
					{
						extraPrize = new Hat("GilsHat");
						Game1.player.mailReceived.Add("DF_Gil_Hat");
					}
					else
					{
						extraPrize = new Object("253", 5);
					}
				}
				else if (num >= 20)
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_Rating_20to24", num);
					eggPrize = 100;
					extraPrize = new Object("253", 5);
				}
				else if (num >= 15)
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_Rating_15to19", num);
					eggPrize = 50;
					extraPrize = new Object("253", 3);
				}
				else if (num >= 10)
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_Rating_10to14", num);
					eggPrize = 25;
					extraPrize = new Object("253", 1);
				}
				else if (num >= 5)
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_Rating_5to9", num);
					eggPrize = 10;
					extraPrize = new Object("395", 1);
				}
				else
				{
					Game1.DrawDialogue(Game1.RequireLocation<AdventureGuild>("AdventureGuild").Gil, "Strings\\1_6_Strings:Gil_Rating_1to4", num);
					eggPrize = 1;
					extraPrize = new Object("243", 1);
				}
				Game1.afterDialogues = delegate
				{
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object("CalicoEgg", eggPrize));
					if (extraPrize != null)
					{
						Game1.afterDialogues = delegate
						{
							Game1.player.addItemByMenuIfNecessary(extraPrize);
						};
					}
				};
			}, 1000);
			break;
		case "Race_Yes":
			base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess"), this.GetRacerResponses(), "Race_Guess_");
			return true;
		case "Shady_Guy_Yes":
			if (Game1.player.Items.CountId("CalicoEgg") >= 1)
			{
				Game1.player.Items.ReduceId("CalicoEgg", 1);
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Question"), this.GetRacerResponses(), "Shady_Guy_Sabotage_");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_NoEgg"));
			}
			break;
		case "CactusMan_Yes":
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Intro"));
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				if (Game1.player.isInventoryFull())
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Full"));
				}
				else
				{
					int seed = Utility.CreateRandomSeed(Game1.player.UniqueMultiplayerID, Game1.year);
					Game1.player.freezePause = 4000;
					DelayedAction.functionAfterDelay(delegate
					{
						this._revealCactusEvent.Fire(seed);
					}, 1000);
					DelayedAction.functionAfterDelay(delegate
					{
						Random random = Utility.CreateRandom(seed);
						random.Next();
						random.Next();
						random.Next();
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_" + random.Next(1, 6)));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							RandomizedPlantFurniture item = new RandomizedPlantFurniture("FreeCactus", Vector2.Zero, seed);
							if (Game1.player.addItemToInventoryBool(item))
							{
								Game1.playSound("coin");
								Game1.player.mailReceived.Add(this.GetCactusMail());
							}
							this._hideCactusEvent.Fire(seed);
							Game1.player.freezePause = 100;
						});
					}, 3000);
				}
			});
			return true;
		case "CactusMan_No":
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_No"));
			return true;
		}
		if (question_and_answer.StartsWith("Race_Guess_"))
		{
			string s = question_and_answer.Substring("Race_Guess_".Length + 1);
			int guessed_racer = -1;
			if (int.TryParse(s, out guessed_racer))
			{
				if (this.currentRaceState.Value >= RaceState.Go && this.currentRaceState.Value < RaceState.AnnounceWinner4)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Late_Guess"));
					return true;
				}
				string racerNameKey = "Strings\\1_6_Strings:Racer_" + guessed_racer;
				string racer_name = Game1.content.LoadString(racerNameKey);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Made", racer_name));
				Game1.multiplayer.globalChatInfoMessage("GuessRacer_" + Game1.random.Next(1, 11), Game1.player.Name, TokenStringBuilder.LocalizedText(racerNameKey));
				this.nextRaceGuesses[Game1.player.UniqueMultiplayerID] = guessed_racer;
			}
			return true;
		}
		if (question_and_answer.StartsWith("Shady_Guy_Sabotage_"))
		{
			string s2 = question_and_answer.Substring("Shady_Guy_Sabotage_".Length + 1);
			int sabotaged_racer = -1;
			if (int.TryParse(s2, out sabotaged_racer))
			{
				if (this.currentRaceState.Value >= RaceState.Go && this.currentRaceState.Value < RaceState.AnnounceWinner4)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Late"));
					return true;
				}
				if (!this.sabotages.Any() && Game1.random.NextDouble() < 0.25)
				{
					Game1.multiplayer.globalChatInfoMessage("RaceSabotage_" + Game1.random.Next(1, 6));
				}
				this.sabotages[Game1.player.UniqueMultiplayerID] = sabotaged_racer;
				this._localSabotageText = -1;
				this.ShowSabotagedRaceText();
			}
			return true;
		}
		if (question_and_answer.StartsWith("DesertScholar"))
		{
			if (question_and_answer == "DesertScholar_Yes")
			{
				this._currentScholarQuestion++;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Intro2"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					this.generateNextScholarQuestion();
				});
			}
			else if (question_and_answer.StartsWith("DesertScholar_Answer_"))
			{
				if (question_and_answer == "DesertScholar_Answer__Wrong")
				{
					Game1.playSound("cancel");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Wrong"));
					this._currentScholarQuestion = -2;
				}
				else if (question_and_answer == "DesertScholar_Answer__Correct")
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Correct"));
					Game1.playSound("give_gift");
					if (this._currentScholarQuestion == 4)
					{
						Game1.player.mailReceived.Add(this.GetScholarMail());
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Win"));
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 50));
								Game1.playSound("coin");
							});
						});
					}
					else
					{
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							this.generateNextScholarQuestion();
						});
					}
				}
			}
		}
		if (question_and_answer.StartsWith("Cook"))
		{
			if (question_and_answer.EndsWith("No"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_No"));
			}
			else if (question_and_answer.StartsWith("Cook_ChoseSauce"))
			{
				Game1.playSound("smallSelect");
				this._cookSauce = Convert.ToInt32(question_and_answer[question_and_answer.Length - 1].ToString() ?? "");
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_ChoseSauce", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Sauce" + this._cookSauce)));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					base.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\desert_festival_tilesheet", new Microsoft.Xna.Framework.Rectangle(320, 280, 29, 24), new Vector2(480f, 1372f), flipped: false, 0f, Color.White)
					{
						id = 1001,
						animationLength = 2,
						interval = 200f,
						totalNumberOfLoops = 9999,
						scale = 4f,
						layerDepth = 0.1343f
					});
					base.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\desert_festival_tilesheet", new Microsoft.Xna.Framework.Rectangle(378, 280, 29, 24), new Vector2(480f, 1372f), flipped: false, 0f, Color.White)
					{
						id = 1002,
						animationLength = 4,
						interval = 100f,
						totalNumberOfLoops = 4,
						delayBeforeAnimationStart = 400,
						scale = 4f,
						layerDepth = 0.1344f
					});
					DelayedAction.playSoundAfterDelay("hammer", 800, this);
					DelayedAction.playSoundAfterDelay("hammer", 1200, this);
					DelayedAction.playSoundAfterDelay("hammer", 1600, this);
					DelayedAction.playSoundAfterDelay("hammer", 2000, this);
					DelayedAction.playSoundAfterDelay("furnace", 2500, this);
					for (int j = 0; j < 12; j++)
					{
						base.temporarySprites.Add(new TemporaryAnimatedSprite(30, new Vector2(460.8f + (float)Game1.random.Next(-10, 10), 1388 + Game1.random.Next(-10, 10)), Color.White, 4, flipped: false, 100f, 2)
						{
							delayBeforeAnimationStart = 2700 + j * 80,
							motion = new Vector2(-1f + (float)Game1.random.Next(-5, 5) / 10f, -1f + (float)Game1.random.Next(-5, 5) / 10f),
							drawAboveAlwaysFront = true
						});
						base.temporarySprites.Add(new TemporaryAnimatedSprite(30, new Vector2(544f + (float)Game1.random.Next(-10, 10), 1388 + Game1.random.Next(-10, 10)), Color.White, 4, flipped: false, 100f, 2)
						{
							delayBeforeAnimationStart = 2700 + j * 80,
							motion = new Vector2(1f + (float)Game1.random.Next(-5, 5) / 10f, -1f + (float)Game1.random.Next(-5, 5) / 10f),
							drawAboveAlwaysFront = true
						});
						if (j % 2 == 0)
						{
							base.temporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\Animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), new Vector2(505.6f + (float)Game1.random.Next(-16, 16), 1344f), Game1.random.NextDouble() < 0.5, 0f, Color.Gray)
							{
								delayBeforeAnimationStart = 2700 + j * 80,
								motion = new Vector2(0f, -0.25f),
								animationLength = 8,
								interval = 70f,
								drawAboveAlwaysFront = true
							});
						}
					}
					Game1.player.freezePause = 4805;
					DelayedAction.functionAfterDelay(delegate
					{
						base.removeTemporarySpritesWithID(1001);
						base.removeTemporarySpritesWithID(1002);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Done", Game1.content.LoadString("Strings\\1_6_Strings:Cook_DishNames_" + this._cookIngredient + "_" + this._cookSauce)));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							Object food = new Object();
							food.edibility.Value = Game1.player.maxHealth;
							string text = "Strings\\1_6_Strings:Cook_DishNames_" + this._cookIngredient + "_" + this._cookSauce;
							food.name = Game1.content.LoadString(text);
							food.displayNameFormat = "[LocalizedText " + text + "]";
							BuffEffects effects = new BuffEffects();
							switch (this._cookIngredient)
							{
							case 0:
								effects.Defense.Value = 3f;
								break;
							case 1:
								effects.MiningLevel.Value = 3f;
								break;
							case 2:
								effects.LuckLevel.Value = 3f;
								break;
							case 3:
								effects.Attack.Value = 3f;
								break;
							case 4:
								effects.FishingLevel.Value = 3f;
								break;
							}
							switch (this._cookSauce)
							{
							case 0:
								effects.Defense.Value = 1f;
								break;
							case 1:
								effects.MiningLevel.Value = 1f;
								break;
							case 2:
								effects.LuckLevel.Value = 1f;
								break;
							case 3:
								effects.Attack.Value = 1f;
								break;
							case 4:
								effects.Speed.Value = 1f;
								break;
							}
							food.customBuff = () => new Buff("DesertFestival", food.Name, food.Name, 600 * Game1.realMilliSecondsPerGameMinute, null, -1, effects);
							int sourceIndex = this._cookIngredient * 4 + this._cookSauce + ((this._cookSauce > this._cookIngredient) ? (-1) : 0);
							Game1.player.tempFoodItemTextureName.Value = "TileSheets\\Objects_2";
							Game1.player.tempFoodItemSourceRect.Value = Utility.getSourceRectWithinRectangularRegion(0, 32, 128, sourceIndex, 16, 16);
							Game1.player.faceDirection(2);
							Game1.player.eatObject(food);
						});
					}, 4800);
				});
			}
			else if (question_and_answer.StartsWith("Cook_PickedIngredient"))
			{
				Game1.playSound("smallSelect");
				this._cookIngredient = Convert.ToInt32(question_and_answer[question_and_answer.Length - 1].ToString() ?? "");
				List<Response> sauces = new List<Response>();
				for (int i = 0; i < 5; i++)
				{
					if (i != this._cookIngredient || this._cookIngredient == 4)
					{
						sauces.Add(new Response(i.ToString() ?? "", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Sauce" + i)));
					}
				}
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_ChoseIngredient", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Ingredient" + this._cookIngredient)), sauces.ToArray(), "Cook_ChoseSauce");
			}
			else if (!(question_and_answer == "Cook_Intro_Yes"))
			{
				if (question_and_answer == "Cook_Intro2_Yes")
				{
					Game1.playSound("smallSelect");
					Response[] ingredients = new Response[5];
					for (int i2 = 0; i2 < 5; i2++)
					{
						ingredients[i2] = new Response(i2.ToString() ?? "", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Ingredient" + i2));
					}
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes3"), ingredients, "Cook_PickedIngredient");
				}
			}
			else
			{
				Game1.playSound("smallSelect");
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					Game1.playSound("smallSelect");
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes2"), base.createYesNoResponses(), "Cook_Intro2");
				});
			}
		}
		return base.answerDialogueAction(question_and_answer, question_params);
	}

	public void CactusGuyHideCactus(int seed)
	{
		if (this._currentlyShownCactusID == seed)
		{
			this._cactusGuyRevealItem = null;
			this._cactusGuyRevealTimer = -1f;
			this._cactusShakeTimer = -1f;
			this._currentlyShownCactusID = -1;
		}
	}

	public void CactusGuyRevealCactus(int seed)
	{
		RandomizedPlantFurniture cactus = new RandomizedPlantFurniture("FreeCactus", Vector2.Zero, seed);
		this._currentlyShownCactusID = seed;
		this._cactusGuyRevealItem = cactus.getOne() as RandomizedPlantFurniture;
		this._cactusGuyRevealTimer = 0f;
		this._cactusShakeTimer = -1f;
		Random random = Utility.CreateRandom(seed);
		random.Next();
		random.Next();
		List<string> sounds = new List<string> { "pig", "Duck", "dog_bark", "cat", "camel" };
		Game1.playSound("throwDownITem");
		DelayedAction.playSoundAfterDelay("thudStep", 500);
		DelayedAction.playSoundAfterDelay("thudStep", 750);
		DelayedAction.playSoundAfterDelay(random.ChooseFrom(sounds), 1000);
		DelayedAction.functionAfterDelay(delegate
		{
			this._cactusShakeTimer = 0.25f;
		}, 1000);
	}

	public bool CanMakeAnotherRaceGuess()
	{
		if (Game1.timeOfDay >= 2200 && this.currentRaceState.Value >= RaceState.Go)
		{
			return false;
		}
		return true;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (this._cactusShakeTimer > 0f)
		{
			this._cactusShakeTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this._cactusShakeTimer <= 0f)
			{
				this._cactusShakeTimer = -1f;
			}
		}
		if (this._raceTextTimer > 0f)
		{
			this._raceTextTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this._raceTextTimer < 0f)
			{
				this._raceTextTimer = 0f;
			}
		}
		if (this._cactusGuyRevealTimer >= 0f && this._cactusGuyRevealTimer < 1f)
		{
			this._cactusGuyRevealTimer += (float)time.ElapsedGameTime.TotalSeconds / 0.75f;
			if (this._cactusGuyRevealTimer >= 1f)
			{
				this._cactusGuyRevealTimer = 1f;
			}
		}
		this._revealCactusEvent.Poll();
		this._hideCactusEvent.Poll();
		this.announceRaceEvent.Poll();
		if (Game1.shouldTimePass())
		{
			if (Game1.IsMasterGame)
			{
				if (this._raceStateTimer >= 0f)
				{
					this._raceStateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this._raceStateTimer <= 0f)
					{
						this._raceStateTimer = 0f;
						switch (this.currentRaceState.Value)
						{
						case RaceState.StartingLine:
							this.announceRaceEvent.Fire("Race_Ready");
							this._raceStateTimer = 3f;
							this.currentRaceState.Value = RaceState.Ready;
							break;
						case RaceState.Ready:
							this.currentRaceState.Value = RaceState.Set;
							this.announceRaceEvent.Fire("Race_Set");
							this._raceStateTimer = 3f;
							break;
						case RaceState.Set:
							this.currentRaceState.Value = RaceState.Go;
							this.announceRaceEvent.Fire("Race_Go");
							this.raceGuesses.Clear();
							foreach (KeyValuePair<long, int> kvp in this.nextRaceGuesses.Pairs)
							{
								this.raceGuesses[kvp.Key] = kvp.Value;
							}
							this.nextRaceGuesses.Clear();
							foreach (Racer racer in this.netRacers)
							{
								racer.sabotages.Value = 0;
								foreach (int value in this.sabotages.Values)
								{
									if (value == racer.racerIndex.Value)
									{
										racer.sabotages.Value++;
									}
								}
								racer.ResetMoveSpeed();
							}
							this.sabotages.Clear();
							this._raceStateTimer = 3f;
							break;
						case RaceState.AnnounceWinner:
						case RaceState.AnnounceWinner2:
						case RaceState.AnnounceWinner3:
						case RaceState.AnnounceWinner4:
							this._raceStateTimer = 2f;
							switch (this.currentRaceState.Value)
							{
							case RaceState.AnnounceWinner:
								this.announceRaceEvent.Fire("Race_Comment_" + Game1.random.Next(1, 5));
								this._raceStateTimer = 4f;
								break;
							case RaceState.AnnounceWinner2:
								this.announceRaceEvent.Fire("Race_Winner");
								this._raceStateTimer = 2f;
								break;
							case RaceState.AnnounceWinner3:
								this.announceRaceEvent.Fire("Racer_" + this.lastRaceWinner.Value);
								this._raceStateTimer = 4f;
								break;
							case RaceState.AnnounceWinner4:
								this.announceRaceEvent.Fire("RESULT");
								this._raceStateTimer = 2f;
								this.finishedRacers.Clear();
								break;
							}
							this.currentRaceState.Value++;
							break;
						case RaceState.RaceEnd:
							if (!this.CanMakeAnotherRaceGuess())
							{
								if (Utility.GetDayOfPassiveFestival("DesertFestival") < 3)
								{
									this.announceRaceEvent.Fire("Race_Close");
								}
								else
								{
									this.announceRaceEvent.Fire("Race_Close_LastDay");
								}
								this.currentRaceState.Value = RaceState.RacesOver;
							}
							else
							{
								this.currentRaceState.Value = RaceState.PreRace;
							}
							break;
						}
					}
				}
				if (this.currentRaceState.Value == RaceState.Go)
				{
					if (this.finishedRacers.Count >= this.racerCount)
					{
						this.currentRaceState.Value = RaceState.AnnounceWinner;
						this._raceStateTimer = 2f;
					}
					else
					{
						foreach (Racer netRacer in this.netRacers)
						{
							netRacer.UpdateRaceProgress(this);
						}
					}
				}
			}
			foreach (Racer netRacer2 in this.netRacers)
			{
				netRacer2.Update(this);
			}
		}
		this.festivalChimneyTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.festivalChimneyTimer <= 0f)
		{
			this.AddSmokePuff(new Vector2(7.25f, 16.25f) * 64f);
			this.AddSmokePuff(new Vector2(28.25f, 6f) * 64f);
			this.festivalChimneyTimer = 500f;
		}
		if (Game1.isStartingToGetDarkOut(this) && Game1.outdoorLight.R > 160)
		{
			Game1.outdoorLight.R = 160;
			Game1.outdoorLight.G = 160;
			Game1.outdoorLight.B = 0;
		}
		base.UpdateWhenCurrentLocation(time);
	}

	public void OnRaceWon(int winner)
	{
		this.lastRaceWinner.Value = winner;
		if (this.raceGuesses.FieldDict.Count <= 0)
		{
			return;
		}
		List<string> winning_farmers = new List<string>();
		foreach (KeyValuePair<long, int> kvp in this.raceGuesses.Pairs)
		{
			if (kvp.Value != winner)
			{
				continue;
			}
			if (winner == 3 && !this.specialRewardsCollected.ContainsKey(kvp.Key))
			{
				this.specialRewardsCollected[kvp.Key] = false;
				continue;
			}
			if (!this.rewardsToCollect.ContainsKey(kvp.Key))
			{
				this.rewardsToCollect[kvp.Key] = 0;
			}
			this.rewardsToCollect[kvp.Key]++;
			Farmer winner_farmer = Game1.GetPlayer(kvp.Key);
			if (winner_farmer != null)
			{
				winning_farmers.Add(winner_farmer.Name);
			}
		}
		string tokenizedWinnerName = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:Racer_" + winner);
		switch (winning_farmers.Count)
		{
		case 0:
			Game1.multiplayer.globalChatInfoMessage("RaceWinners_Zero", tokenizedWinnerName);
			return;
		case 1:
			Game1.multiplayer.globalChatInfoMessage("RaceWinners_One", tokenizedWinnerName, winning_farmers[0]);
			return;
		case 2:
			Game1.multiplayer.globalChatInfoMessage("RaceWinners_Two", tokenizedWinnerName, winning_farmers[0], winning_farmers[1]);
			return;
		}
		Game1.multiplayer.globalChatInfoMessage("RaceWinners_Many", tokenizedWinnerName);
		for (int i = 0; i < winning_farmers.Count; i++)
		{
			if (i < winning_farmers.Count - 1)
			{
				Game1.multiplayer.globalChatInfoMessage("RaceWinners_List", winning_farmers[i]);
			}
			else
			{
				Game1.multiplayer.globalChatInfoMessage("RaceWinners_Final", winning_farmers[i]);
			}
		}
	}

	public void AddSmokePuff(Vector2 v)
	{
		base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, flipped: false, 0.002f, Color.Gray)
		{
			alpha = 0.75f,
			motion = new Vector2(0f, -0.5f),
			acceleration = new Vector2(0.002f, 0f),
			interval = 99999f,
			layerDepth = 1f,
			scale = 2f,
			scaleChange = 0.02f,
			drawAboveAlwaysFront = true,
			rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
		});
	}

	public static void CleanupFestival()
	{
		Game1.player.team.itemsToRemoveOvernight.Add("CalicoEgg");
		SpecialOrder.RemoveAllSpecialOrders("DesertFestivalMarlon");
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		if (this._cactusGuyRevealTimer > 0f && this._cactusGuyRevealItem != null)
		{
			Vector2 start = new Vector2(29f, 66.5f) * 64f;
			Vector2 end = new Vector2(27.5f, 66.5f) * 64f;
			float height = 0f;
			float bounce_point = 0.6f;
			height = ((!(this._cactusGuyRevealTimer < bounce_point)) ? ((float)Math.Sin((double)((this._cactusGuyRevealTimer - bounce_point) / (1f - bounce_point)) * Math.PI) * 8f * 4f) : ((float)Math.Sin((double)(this._cactusGuyRevealTimer / bounce_point) * Math.PI) * 16f * 4f));
			Vector2 position = new Vector2(Utility.Lerp(start.X, end.X, this._cactusGuyRevealTimer), Utility.Lerp(start.Y, end.Y, this._cactusGuyRevealTimer));
			float sort_y = position.Y;
			if (this._cactusShakeTimer > 0f)
			{
				position.X += Game1.random.Next(-1, 2);
				position.Y += Game1.random.Next(-1, 2);
			}
			this._cactusGuyRevealItem.DrawFurniture(spriteBatch, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, 0f - height)), 1f, new Vector2(8f, 16f), 4f, sort_y / 10000f);
			spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position), null, Color.White * 0.75f, 0f, new Vector2(Game1.shadowTexture.Width / 2, Game1.shadowTexture.Height / 2), new Vector2(4f, 4f), SpriteEffects.None, sort_y / 10000f - 1E-07f);
		}
		foreach (Racer racer in this._localRacers)
		{
			if (!racer.drawAboveMap.Value)
			{
				racer.Draw(spriteBatch);
			}
		}
		if (Game1.Date != Game1.player.lastDesertFestivalFishingQuest.Value)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(984f, 842f + yOffset)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 16f), SpriteEffects.None, 1f);
		}
		if (!this.checkedMineExplanation)
		{
			float yOffset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(609.6f, 320f + yOffset2)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset2 / 16f), SpriteEffects.None, 1f);
		}
		if (Game1.timeOfDay < 1000)
		{
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(45f, 14f) * 64f + new Vector2(7f, 9f) * 4f), new Microsoft.Xna.Framework.Rectangle(239, 317, 16, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.096f);
		}
		base.draw(spriteBatch);
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (base.getTileIndexAt(tileLocation, "Buildings", "desert-festival"))
		{
		case 796:
		case 797:
			Utility.TryOpenShopMenu("Traveler", this);
			return true;
		case 792:
		case 793:
			base.playSound("pig");
			return true;
		case 1073:
			base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), base.createYesNoResponses(), "WarperQuestion");
			return true;
		default:
			return base.checkAction(tileLocation, viewport, who);
		}
	}

	public override void drawOverlays(SpriteBatch b)
	{
		SpecialCurrencyDisplay.Draw(b, new Vector2(16f, 0f), this.eggMoneyDial, Game1.player.Items.CountId("CalicoEgg"), Game1.mouseCursors_1_6, new Microsoft.Xna.Framework.Rectangle(0, 21, 0, 0));
		base.drawOverlays(b);
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch sb)
	{
		base.drawAboveAlwaysFrontLayer(sb);
		this._localRacers.Sort((Racer a, Racer b) => a.position.Y.CompareTo(b.position.Y));
		foreach (Racer racer in this._localRacers)
		{
			if (racer.drawAboveMap.Value)
			{
				racer.Draw(sb);
			}
		}
		if (this._raceTextTimer > 0f && this._raceText != null)
		{
			Vector2 local = Game1.GlobalToLocal(new Vector2(44.5f, 39.5f) * 64f);
			if (this._raceTextShake)
			{
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			float alpha = Utility.Clamp(this._raceTextTimer / 0.25f, 0f, 1f);
			SpriteText.drawStringWithScrollCenteredAt(sb, this._raceText, (int)local.X, (int)local.Y - 192, "", alpha, null, 1, local.Y / 10000f + 0.001f);
		}
	}

	public Vector3 GetTrackPosition(int track_index, float horizontal_position)
	{
		Vector2 inner_edge = new Vector2(this.raceTrack[track_index][0].X + 0.5f, this.raceTrack[track_index][0].Y + 0.5f);
		Vector2 outer_edge = new Vector2(this.raceTrack[track_index][1].X + 0.5f, this.raceTrack[track_index][1].Y + 0.5f);
		_ = inner_edge == outer_edge;
		Vector2 delta = outer_edge - inner_edge;
		delta.Normalize();
		inner_edge *= 64f;
		outer_edge *= 64f;
		inner_edge -= delta * 64f / 4f;
		outer_edge += delta * 64f / 4f;
		return new Vector3(Utility.Lerp(inner_edge.X, outer_edge.X, horizontal_position), Utility.Lerp(inner_edge.Y, outer_edge.Y, horizontal_position), this.raceTrack[track_index][0].Z);
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		string festival_id = "DesertFestival";
		base.performTenMinuteUpdate(timeOfDay);
		if (Game1.IsMasterGame && Utility.IsPassiveFestivalOpen(festival_id) && timeOfDay % 200 == 0 && timeOfDay < 2400 && this.currentRaceState.Value == RaceState.PreRace)
		{
			this.announceRaceEvent.Fire("Race_Begin");
			this.currentRaceState.Value = RaceState.StartingLine;
			if (this.nextRaceGuesses.FieldDict.Count > 0)
			{
				Game1.multiplayer.globalChatInfoMessage("RaceStarting");
			}
			this._raceStateTimer = 5f;
		}
	}

	public virtual void AnnounceRace(string text)
	{
		this._raceTextShake = false;
		this._raceTextTimer = 2f;
		if (text == "Race_Go" || text == "Race_Finish" || text.StartsWith("Racer_"))
		{
			this._raceTextShake = true;
		}
		if (text.StartsWith("Race_Close"))
		{
			this._raceTextTimer = 4f;
		}
		if (text == "RESULT")
		{
			this._raceTextTimer = 4f;
			if (this.raceGuesses.TryGetValue(Game1.player.UniqueMultiplayerID, out var guessedRacer))
			{
				if (this.lastRaceWinner.Value == guessedRacer)
				{
					this._raceText = Game1.content.LoadString("Strings\\1_6_Strings:Race_Win");
				}
				else
				{
					this._raceText = Game1.content.LoadString("Strings\\1_6_Strings:Race_Lose");
				}
			}
		}
		else
		{
			this._raceText = Game1.content.LoadString("Strings\\1_6_Strings:" + text);
			if (text.StartsWith("Racer_"))
			{
				this._raceText += "!";
			}
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		Game1.player.team.calicoEggSkullCavernRating.Value = 0;
		Game1.player.team.highestCalicoEggRatingToday.Value = 0;
		Game1.player.team.calicoStatueEffects.Clear();
		MineShaft.totalCalicoStatuesActivatedToday = 0;
		this.finishedRacers.Clear();
		this.lastRaceWinner.Value = -1;
		this.rewardsToCollect.Clear();
		this.specialRewardsCollected.Clear();
		this.raceGuesses.Clear();
		this.nextRaceGuesses.Clear();
		this.sabotages.Clear();
		this.currentRaceState.Value = RaceState.PreRace;
		this._raceStateTimer = 0f;
		this._currentScholarQuestion = -1;
	}

	public override void cleanupBeforePlayerExit()
	{
		this._localRacers.Clear();
		this._cactusGuyRevealTimer = -1f;
		this._cactusGuyRevealItem = null;
		base.cleanupBeforePlayerExit();
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.player.mailReceived.Contains("Checked_DF_Mine_Explanation"))
		{
			this.checkedMineExplanation = true;
		}
		this._localRacers.Clear();
		this._localRacers.AddRange(this.netRacers);
		if (base.critters == null)
		{
			base.critters = new List<Critter>();
		}
		for (int i = 0; i < 8; i++)
		{
			base.critters.Add(new Butterfly(this, base.getRandomTile(), islandButterfly: false, forceSummerButterfly: true));
		}
		this.eggMoneyDial = new MoneyDial(4, playSound: false);
		this.eggMoneyDial.currentValue = Game1.player.Items.CountId("CalicoEgg");
	}

	public static void SetupFestivalDay()
	{
		string festival_id = "DesertFestival";
		int day_number = Utility.GetDayOfPassiveFestival(festival_id);
		Dictionary<string, ShopData> store_data_sheet = DataLoader.Shops(Game1.content);
		List<NPC> characters = Utility.getAllVillagers();
		characters.RemoveAll((NPC nPC) => !store_data_sheet.ContainsKey(festival_id + "_" + nPC.Name) || (nPC.Name == "Leo" && !Game1.MasterPlayer.mailReceived.Contains("leoMoved")) || nPC.getMasterScheduleRawData().ContainsKey(festival_id + "_" + day_number));
		Random r = Utility.CreateDaySaveRandom();
		for (int i = 0; i < day_number - 1; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				NPC character = r.ChooseFrom(characters);
				characters.Remove(character);
				if (characters.Count == 0)
				{
					break;
				}
			}
		}
		if (characters.Count > 0)
		{
			NPC character2 = r.ChooseFrom(characters);
			characters.Remove(character2);
			DesertFestival.SetupMerchantSchedule(character2, 0);
		}
		if (characters.Count > 0)
		{
			NPC character3 = r.ChooseFrom(characters);
			characters.Remove(character3);
			DesertFestival.SetupMerchantSchedule(character3, 1);
		}
		if (Game1.getLocationFromName("DesertFestival") is DesertFestival festival_location)
		{
			festival_location.netRacers.Clear();
			List<int> racers = new List<int>();
			for (int i2 = 0; i2 < festival_location.totalRacers; i2++)
			{
				racers.Add(i2);
			}
			for (int i3 = 0; i3 < festival_location.racerCount; i3++)
			{
				int racer_index = r.ChooseFrom(racers);
				racers.Remove(racer_index);
				Racer racer = new Racer(racer_index);
				racer.position.Value = new Vector2(44.5f, 37.5f - (float)i3) * 64f;
				racer.segmentStart = racer.position.Value;
				racer.segmentEnd = racer.position.Value;
				festival_location.netRacers.Add(racer);
			}
		}
		SpecialOrder.UpdateAvailableSpecialOrders("DesertFestivalMarlon", forceRefresh: true);
	}
}
