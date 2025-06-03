using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;

namespace StardewValley.Menus;

public class MasteryTrackerMenu : IClickableMenu
{
	public const int MASTERY_EXP_PER_LEVEL = 10000;

	public const int WIDTH = 200;

	public const int HEIGHT = 80;

	public ClickableTextureComponent mainButton;

	private float pressedButtonTimer;

	private float destroyTimer;

	private List<ClickableTextureComponent> rewards = new List<ClickableTextureComponent>();

	private int which;

	private bool canClaim;

	public MasteryTrackerMenu(int whichSkill = -1)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(800, 320).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(800, 320).Y, 800, 320, showUpperRightCloseButton: true)
	{
		this.which = whichSkill;
		base.closeSound = "stone_button";
		Texture2D objects2Tex = Game1.content.Load<Texture2D>("TileSheets\\Objects_2");
		switch (whichSkill)
		{
		case 0:
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.content.Load<Texture2D>("TileSheets\\weapons"), new Rectangle(32, 128, 16, 16), 4f, drawShadow: true)
			{
				name = Game1.content.LoadString("Strings\\Weapons:IridiumScythe_Name"),
				label = Game1.content.LoadString("Strings\\Weapons:IridiumScythe_Description"),
				hoverText = "(W)66"
			});
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(32, 1152, 16, 32), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(BC)StatueOfBlessings").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("(BC)StatueOfBlessings").Description,
				myAlternateID = 1,
				hoverText = "Statue Of Blessings"
			});
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, drawShadow: true)
			{
				name = "",
				label = Game1.content.LoadString("Strings\\1_6_Strings:Farming_Mastery"),
				myAlternateID = 0
			});
			Game1.playSound("weed_cut");
			break;
		case 3:
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(64, 1152, 16, 32), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(BC)StatueOfTheDwarfKing").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("StatueOfTheDwarfKing").Description,
				myAlternateID = 1,
				hoverText = "Statue Of The Dwarf King"
			});
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(0, 1152, 16, 32), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(BC)HeavyFurnace").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("(BC)HeavyFurnace").Description,
				myAlternateID = 1,
				hoverText = "Heavy Furnace"
			});
			Game1.playSound("stoneCrack");
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, drawShadow: true)
			{
				name = "",
				label = Game1.content.LoadString("Strings\\1_6_Strings:Mining_Mastery"),
				myAlternateID = 0
			});
			break;
		case 1:
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.toolSpriteSheet, new Rectangle(272, 0, 16, 16), 4f, drawShadow: true)
			{
				name = Game1.content.LoadString("Strings\\Tools:FishingRod_AdvancedIridium_Name"),
				label = Game1.content.LoadString("Strings\\Tools:FishingRod_AdvancedIridium_Description"),
				hoverText = "(T)AdvancedIridiumRod"
			});
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, objects2Tex, new Rectangle(0, 144, 16, 16), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(O)ChallengeBait").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("(O)ChallengeBait").Description,
				myAlternateID = 1,
				hoverText = "Challenge Bait"
			});
			Game1.playSound("waterSlosh");
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, drawShadow: true)
			{
				name = "",
				label = Game1.content.LoadString("Strings\\1_6_Strings:Fishing_Mastery"),
				myAlternateID = 0
			});
			break;
		case 2:
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, objects2Tex, new Rectangle(80, 112, 16, 16), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(O)MysticTreeSeed").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("(O)MysticTreeSeed").Description,
				myAlternateID = 1,
				hoverText = "Mystic Tree Seed"
			});
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, objects2Tex, new Rectangle(112, 128, 16, 16), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(O)TreasureTotem").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("(O)TreasureTotem").Description,
				myAlternateID = 1,
				hoverText = "Treasure Totem"
			});
			Game1.playSound("axchop");
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, drawShadow: true)
			{
				name = "",
				label = Game1.content.LoadString("Strings\\1_6_Strings:Foraging_Mastery"),
				myAlternateID = 0
			});
			break;
		case 4:
			Game1.playSound("cavedrip");
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(80, 1152, 16, 32), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(BC)Anvil").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("(BC)Anvil").Description,
				myAlternateID = 1,
				hoverText = "Anvil"
			});
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.bigCraftableSpriteSheet, new Rectangle(96, 1152, 16, 32), 4f, drawShadow: true)
			{
				name = ItemRegistry.GetDataOrErrorItem("(BC)MiniForge").DisplayName,
				label = ItemRegistry.GetDataOrErrorItem("(BC)MiniForge").Description,
				myAlternateID = 1,
				hoverText = "Mini-Forge"
			});
			this.rewards.Add(new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors_1_6, new Rectangle(103, 90, 17, 16), 4f, drawShadow: true)
			{
				name = "",
				label = Game1.content.LoadString("Strings\\1_6_Strings:Trinkets_Description"),
				myAlternateID = 0
			});
			break;
		}
		float yHeight = 80f;
		for (int i = 0; i < this.rewards.Count; i++)
		{
			this.rewards[i].bounds = new Rectangle(base.xPositionOnScreen + 40, base.yPositionOnScreen + 64 + (int)yHeight, 64, 64);
			this.rewards[i].label = Game1.parseText(this.rewards[i].label, Game1.smallFont, base.width - 200);
			yHeight += Game1.smallFont.MeasureString(this.rewards[i].label).Y;
			if (i < this.rewards.Count - 1)
			{
				yHeight += (float)((this.rewards[i].sourceRect.Height > 16) ? 132 : 80);
			}
		}
		base.height += (int)yHeight;
		base.height -= 48;
		if (whichSkill != -1)
		{
			base.height -= 64;
		}
		int num = base.yPositionOnScreen;
		base.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, base.height).Y;
		int offset = num - base.yPositionOnScreen;
		foreach (ClickableTextureComponent reward in this.rewards)
		{
			reward.bounds.Y -= offset;
		}
		base.upperRightCloseButton.bounds.Y -= offset;
		int levelsNotSpent = MasteryTrackerMenu.getCurrentMasteryLevel() - (int)Game1.stats.Get("masteryLevelsSpent");
		this.canClaim = levelsNotSpent > 0;
		if (Game1.player.stats.Get(StatKeys.Mastery(whichSkill)) == 0)
		{
			this.mainButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 84, base.yPositionOnScreen + base.height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
			{
				visible = (whichSkill != -1),
				myID = 0
			};
		}
		if (whichSkill == -1)
		{
			Game1.playSound("boulderCrack");
		}
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			if (this.mainButton == null)
			{
				base.currentlySnappedComponent = base.getComponentWithID(base.upperRightCloseButton.myID);
			}
			else
			{
				base.currentlySnappedComponent = base.getComponentWithID(0);
			}
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (this.destroyTimer > 0f)
		{
			return;
		}
		if (this.mainButton != null && this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && this.canClaim)
		{
			if (this.mainButton.sourceRect.X == 0)
			{
				Game1.playSound("Cowboy_gunshot");
			}
			this.mainButton.sourceRect.X = 42;
		}
		else if (this.mainButton != null)
		{
			this.mainButton.sourceRect.X = 0;
		}
		base.performHoverAction(x, y);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!(this.destroyTimer > 0f))
		{
			if (this.mainButton != null && this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && this.canClaim)
			{
				Game1.playSound("cowboy_monsterhit");
				DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 200);
				this.pressedButtonTimer = 200f;
				this.claimReward();
			}
			base.receiveLeftClick(x, y, playSound);
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		base.receiveRightClick(x, y, playSound);
		base.exitThisMenu();
	}

	private void claimReward()
	{
		List<Item> toDrop = new List<Item>();
		foreach (ClickableTextureComponent c in this.rewards)
		{
			if (c.myAlternateID == 1)
			{
				Game1.player.craftingRecipes.TryAdd(c.hoverText, 0);
				continue;
			}
			string hoverText = c.hoverText;
			if (hoverText != null && hoverText.Length > 0)
			{
				Item i = ItemRegistry.Create(c.hoverText);
				if (!Game1.player.addItemToInventoryBool(i))
				{
					toDrop.Add(i);
				}
			}
		}
		foreach (Item item in toDrop)
		{
			Game1.createItemDebris(item, Game1.player.getStandingPosition(), 2);
		}
		Game1.player.stats.Increment(StatKeys.Mastery(this.which), 1);
		if (this.which == 4)
		{
			Game1.player.stats.Set("trinketSlots", 1);
		}
		Game1.stats.Increment("masteryLevelsSpent");
		Game1.currentLocation.removeTemporarySpritesWithID(8765 + this.which);
		MasteryTrackerMenu.addSkillFlairPlaque(this.which);
		Game1.stats.Get("MasteryExp");
		if (MasteryTrackerMenu.getCurrentMasteryLevel() - (int)Game1.stats.Get("masteryLevelsSpent") <= 0)
		{
			Game1.currentLocation.removeTemporarySpritesWithID(8765);
			Game1.currentLocation.removeTemporarySpritesWithID(8766);
			Game1.currentLocation.removeTemporarySpritesWithID(8767);
			Game1.currentLocation.removeTemporarySpritesWithID(8768);
			Game1.currentLocation.removeTemporarySpritesWithID(8769);
		}
		if (MasteryTrackerMenu.hasCompletedAllMasteryPlaques())
		{
			DelayedAction.functionAfterDelay(delegate
			{
				MasteryTrackerMenu.addSpiritCandles();
			}, 500);
			Game1.player.freezePause = 2000;
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.changeMusicTrack("grandpas_theme");
			}, 2000);
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryCompleteToast"));
				Game1.playSound("newArtifact");
			}, 4000);
		}
	}

	public static void addSpiritCandles(bool instant = false)
	{
		MasteryTrackerMenu.addCandle(58, 67, (!instant) ? 500 : 0);
		MasteryTrackerMenu.addCandle(88, 51, (!instant) ? 700 : 0);
		MasteryTrackerMenu.addCandle(120, 51, (!instant) ? 900 : 0);
		MasteryTrackerMenu.addCandle(152, 51, (!instant) ? 1100 : 0);
		MasteryTrackerMenu.addCandle(183, 67, (!instant) ? 1300 : 0);
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(483, 0, 29, 27), new Vector2(61f, 82f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 99999f,
			totalNumberOfLoops = 99999,
			animationLength = 1,
			lightId = "MasteryTrackerMenu_GrandpaHat",
			id = 6666,
			lightRadius = 1f,
			scale = 4f,
			layerDepth = 0.0449f,
			delayBeforeAnimationStart = ((!instant) ? 250 : 0)
		});
		Game1.currentLocation.removeTile(10, 9, "Buildings");
		if (!instant)
		{
			Utility.addSprinklesToLocation(Game1.currentLocation, 10, 9, 1, 1, 300, 100, Color.White);
			Utility.addSprinklesToLocation(Game1.currentLocation, 4, 6, 1, 2, 300, 50, Color.White);
		}
	}

	private static void addCandle(int x, int y, int delay)
	{
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(536, 1945, 8, 8), new Vector2(x, y) * 4f + new Vector2(-3f, -6f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 50f + (float)Game1.random.Next(15),
			totalNumberOfLoops = 99999,
			animationLength = 7,
			lightId = $"{"MasteryTrackerMenu"}_SpiritCandle_{x}_{y}",
			id = 6666,
			lightRadius = 1f,
			scale = 3f,
			layerDepth = 0.038500004f,
			delayBeforeAnimationStart = delay,
			startSound = ((delay > 0) ? "fireball" : null),
			drawAboveAlwaysFront = true
		});
	}

	public static void addSkillFlairPlaque(int which)
	{
		switch (which)
		{
		case 4:
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(0, 59, 9, 21), new Vector2(53f, 75f) * 4f, flipped: false, 0f, Color.White)
			{
				animationLength = 1,
				interval = 9999f,
				totalNumberOfLoops = 999999,
				scale = 4f
			});
			break;
		case 2:
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(10, 59, 10, 21), new Vector2(82f, 61f) * 4f, flipped: false, 0f, Color.White)
			{
				animationLength = 1,
				interval = 9999f,
				totalNumberOfLoops = 999999,
				scale = 4f
			});
			break;
		case 0:
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(21, 59, 15, 21), new Vector2(113f, 61f) * 4f, flipped: false, 0f, Color.White)
			{
				animationLength = 1,
				interval = 9999f,
				totalNumberOfLoops = 999999,
				scale = 4f
			});
			break;
		case 1:
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(37, 59, 16, 21), new Vector2(143f, 63f) * 4f, flipped: false, 0f, Color.White)
			{
				animationLength = 1,
				interval = 9999f,
				totalNumberOfLoops = 999999,
				scale = 4f
			});
			break;
		case 3:
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(54, 59, 16, 21), new Vector2(175f, 75f) * 4f, flipped: false, 0f, Color.White)
			{
				animationLength = 1,
				interval = 9999f,
				totalNumberOfLoops = 999999,
				scale = 4f
			});
			break;
		}
	}

	public static bool hasCompletedAllMasteryPlaques()
	{
		if (Game1.player.stats.Get(StatKeys.Mastery(0)) != 0 && Game1.player.stats.Get(StatKeys.Mastery(1)) != 0 && Game1.player.stats.Get(StatKeys.Mastery(2)) != 0 && Game1.player.stats.Get(StatKeys.Mastery(3)) != 0)
		{
			return Game1.player.stats.Get(StatKeys.Mastery(4)) != 0;
		}
		return false;
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (this.destroyTimer > 0f)
		{
			this.destroyTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this.destroyTimer <= 0f)
			{
				Game1.activeClickableMenu = null;
				Game1.playSound("discoverMineral");
			}
		}
		if (this.pressedButtonTimer > 0f)
		{
			this.pressedButtonTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			this.mainButton.sourceRect.X = 84;
			if (this.pressedButtonTimer <= 0f)
			{
				this.destroyTimer = 100f;
			}
		}
		base.update(time);
	}

	public static int getMasteryExpNeededForLevel(int level)
	{
		return level switch
		{
			0 => 0, 
			1 => 10000, 
			2 => 25000, 
			3 => 45000, 
			4 => 70000, 
			5 => 100000, 
			_ => int.MaxValue, 
		};
	}

	public static int getCurrentMasteryLevel()
	{
		int masteryExp = (int)Game1.stats.Get("MasteryExp");
		int level = 0;
		for (int i = 1; i <= 5; i++)
		{
			if (masteryExp >= MasteryTrackerMenu.getMasteryExpNeededForLevel(i))
			{
				level++;
			}
		}
		return level;
	}

	public static void drawBar(SpriteBatch b, Vector2 topLeftSpot, float widthScale = 1f)
	{
		int masteryExp = (int)Game1.stats.Get("MasteryExp");
		int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
		float currentProgressXP = masteryExp - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved);
		float expNeededToReachNextLevel = MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved + 1) - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved);
		int barWidth = (int)(576f * currentProgressXP / expNeededToReachNextLevel * widthScale);
		if (levelsAchieved >= 5)
		{
			barWidth = (int)(576f * widthScale);
		}
		if (levelsAchieved >= 5 || barWidth > 0)
		{
			Color light = new Color(60, 180, 80);
			Color med = new Color(0, 113, 62);
			Color medDark = new Color(0, 80, 50);
			Color dark = new Color(0, 60, 30);
			if (levelsAchieved >= 5 && widthScale == 1f)
			{
				light = new Color(220, 220, 220);
				med = new Color(140, 140, 140);
				medDark = new Color(80, 80, 80);
				dark = med;
			}
			if (widthScale != 1f)
			{
				dark = medDark;
			}
			b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 144, barWidth, 32), med);
			b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 148, 4, 28), medDark);
			if (barWidth > 8)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 172, barWidth - 8, 4), medDark);
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 116, (int)topLeftSpot.Y + 144, barWidth - 4, 4), light);
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 104 + barWidth, (int)topLeftSpot.Y + 144, 4, 28), light);
				b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 108 + barWidth, (int)topLeftSpot.Y + 144, 4, 32), dark);
			}
		}
		if (levelsAchieved < 5)
		{
			string s = masteryExp - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved) + "/" + (MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved + 1) - MasteryTrackerMenu.getMasteryExpNeededForLevel(levelsAchieved));
			b.DrawString(Game1.smallFont, s, new Vector2((float)((int)topLeftSpot.X + 112) + 288f * widthScale - Game1.smallFont.MeasureString(s).X / 2f, (float)(int)topLeftSpot.Y + 146f), Color.White * 0.75f);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(1, 85, 21, 21), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);
		b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2(6f, 7f) * 4f, new Rectangle(0, 144, 23, 23), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2(24f, base.height - 24), new Rectangle(0, 144, 23, 23), Color.White, -(float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2(base.width - 24, 28f), new Rectangle(0, 144, 23, 23), Color.White, -4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		b.Draw(Game1.mouseCursors_1_6, base.Position + new Vector2(base.width - 24, base.height - 24), new Rectangle(0, 144, 23, 23), Color.White, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		Game1.stats.Get("MasteryExp");
		int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
		int levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");
		if (this.which == -1)
		{
			SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\1_6_Strings:FinalPath"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + 48, 9999, -1, 9999, 1f, 0.88f, junimoText: false, Color.Black);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(0, 107, 15, 15), base.xPositionOnScreen + 100, base.yPositionOnScreen + 128, 600, 64, Color.White, 4f);
			MasteryTrackerMenu.drawBar(b, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen));
			for (int i = 0; i < 5; i++)
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(base.xPositionOnScreen + base.width / 2) - 110f + (float)(i * 11 * 4), base.yPositionOnScreen + 220), new Rectangle((i >= levelsAchieved - levelsNotSpent && i < levelsAchieved) ? (43 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600 / 100 * 10) : ((levelsAchieved > i) ? 33 : 23), 89, 10, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
		}
		else
		{
			SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\1_6_Strings:" + this.which + "_Mastery"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + 48, 9999, -1, 9999, 1f, 0.88f, junimoText: false, Color.Black);
			float yMeasure = Game1.smallFont.MeasureString("I").Y;
			foreach (ClickableTextureComponent c in this.rewards)
			{
				if (Game1.smallFont.MeasureString(c.label).Y < yMeasure * 2f)
				{
					Utility.drawWithShadow(b, c.texture, c.getVector2() + new Vector2(0f, -16f), c.sourceRect, Color.White, 0f, Vector2.Zero, 4f);
				}
				else
				{
					Utility.drawWithShadow(b, c.texture, c.getVector2(), c.sourceRect, Color.White, 0f, Vector2.Zero, 4f);
				}
				if (c.name != "")
				{
					Utility.drawTextWithColoredShadow(b, c.name, Game1.dialogueFont, c.getVector2() + new Vector2(104f, 0f), Color.Black, Color.Black * 0.2f);
				}
				Utility.drawTextWithColoredShadow(b, c.label, Game1.smallFont, c.getVector2() + new Vector2(104f, (!(c.name == "")) ? 48 : 0), Color.Black, Color.Black * 0.2f);
				if (c.myAlternateID == 1)
				{
					b.Draw(Game1.objectSpriteSheet, c.getVector2() + new Vector2(32f, 32 + ((c.sourceRect.Height > 16) ? 64 : 0)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
				}
			}
			if (this.mainButton != null)
			{
				this.mainButton?.draw(b, (levelsNotSpent > 0) ? Color.White : (Color.White * 0.5f), 0.88f);
				string s = Game1.content.LoadString("Strings\\1_6_Strings:Claim");
				Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont, this.mainButton.getVector2() + new Vector2((float)(this.mainButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f, 6f + (float)((this.mainButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * ((levelsNotSpent > 0) ? 1f : 0.5f), Color.Black * 0.2f, 1f, 0.9f);
			}
		}
		base.draw(b);
		base.drawMouse(b);
	}
}
