using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.Menus;

public class BobberBar : IClickableMenu
{
	public const int timePerFishSizeReduction = 800;

	public const int bobberTrackHeight = 548;

	public const int bobberBarTrackHeight = 568;

	public const int xOffsetToBobberTrack = 64;

	public const int yOffsetToBobberTrack = 12;

	public const int mixed = 0;

	public const int dart = 1;

	public const int smooth = 2;

	public const int sink = 3;

	public const int floater = 4;

	public const int CHALLENGE_BAIT_MAX_FISHES = 3;

	public bool handledFishResult;

	public float difficulty;

	public int motionType;

	public string whichFish;

	/// <summary>A modifier that only affects the "damage" for not having the fish in the bobber bar.</summary>
	public float distanceFromCatchPenaltyModifier = 1f;

	/// <summary>The mail flag to set for the current player when the current <see cref="F:StardewValley.Menus.BobberBar.whichFish" /> is successfully caught.</summary>
	public string setFlagOnCatch;

	public float bobberPosition = 548f;

	public float bobberSpeed;

	public float bobberAcceleration;

	public float bobberTargetPosition;

	public float scale;

	public float everythingShakeTimer;

	public float floaterSinkerAcceleration;

	public float treasurePosition;

	public float treasureCatchLevel;

	public float treasureAppearTimer;

	public float treasureScale;

	public bool bobberInBar;

	public bool buttonPressed;

	public bool flipBubble;

	public bool fadeIn;

	public bool fadeOut;

	public bool treasure;

	public bool treasureCaught;

	public bool perfect;

	public bool bossFish;

	public bool beginnersRod;

	public bool fromFishPond;

	public bool goldenTreasure;

	public int bobberBarHeight;

	public int fishSize;

	public int fishQuality;

	public int minFishSize;

	public int maxFishSize;

	public int fishSizeReductionTimer;

	public int challengeBaitFishes = -1;

	public List<string> bobbers;

	public Vector2 barShake;

	public Vector2 fishShake;

	public Vector2 everythingShake;

	public Vector2 treasureShake;

	public float reelRotation;

	private SparklingText sparkleText;

	public float bobberBarPos;

	public float bobberBarSpeed;

	public float distanceFromCatching = 0.3f;

	public static ICue reelSound;

	public static ICue unReelSound;

	private Item fishObject;

	public BobberBar(string whichFish, float fishSize, bool treasure, List<string> bobbers, string setFlagOnCatch, bool isBossFish, string baitID = "", bool goldenTreasure = false)
		: base(0, 0, 96, 636)
	{
		this.fishObject = ItemRegistry.Create(whichFish);
		this.bobbers = bobbers;
		this.setFlagOnCatch = setFlagOnCatch;
		this.handledFishResult = false;
		this.treasure = treasure;
		this.goldenTreasure = goldenTreasure;
		this.treasureAppearTimer = Game1.random.Next(1000, 3000);
		this.fadeIn = true;
		this.scale = 0f;
		this.whichFish = whichFish;
		Dictionary<string, string> dictionary = DataLoader.Fish(Game1.content);
		this.beginnersRod = Game1.player.CurrentTool is FishingRod && Game1.player.CurrentTool.upgradeLevel.Value == 1;
		this.bobberBarHeight = 96 + Game1.player.FishingLevel * 8;
		if (Game1.player.FishingLevel < 5 && this.beginnersRod)
		{
			this.bobberBarHeight += 40 - Game1.player.FishingLevel * 8;
		}
		this.bossFish = isBossFish;
		if (dictionary.TryGetValue(whichFish, out var rawData))
		{
			string[] fields = rawData.Split('/');
			this.difficulty = Convert.ToInt32(fields[1]);
			switch (fields[2].ToLower())
			{
			case "mixed":
				this.motionType = 0;
				break;
			case "dart":
				this.motionType = 1;
				break;
			case "smooth":
				this.motionType = 2;
				break;
			case "floater":
				this.motionType = 4;
				break;
			case "sinker":
				this.motionType = 3;
				break;
			}
			this.minFishSize = Convert.ToInt32(fields[3]);
			this.maxFishSize = Convert.ToInt32(fields[4]);
			this.fishSize = (int)((float)this.minFishSize + (float)(this.maxFishSize - this.minFishSize) * fishSize);
			this.fishSize++;
			this.perfect = true;
			this.fishQuality = ((!((double)fishSize < 0.33)) ? (((double)fishSize < 0.66) ? 1 : 2) : 0);
			this.fishSizeReductionTimer = 800;
			for (int i = 0; i < Utility.getStringCountInList(bobbers, "(O)877"); i++)
			{
				this.fishQuality++;
				if (this.fishQuality > 2)
				{
					this.fishQuality = 4;
				}
			}
			if (this.beginnersRod)
			{
				this.fishQuality = 0;
				fishSize = this.minFishSize;
			}
			if (Game1.player.stats.Get("blessingOfWaters") != 0)
			{
				if (this.difficulty > 20f)
				{
					if (isBossFish)
					{
						this.difficulty *= 0.75f;
					}
					else
					{
						this.difficulty /= 2f;
					}
				}
				this.distanceFromCatchPenaltyModifier = 0.5f;
				Game1.player.stats.Decrement("blessingOfWaters");
				if (Game1.player.stats.Get("blessingOfWaters") == 0)
				{
					Game1.player.buffs.Remove("statue_of_blessings_3");
				}
			}
		}
		NetStringIntArrayDictionary fishCaught = Game1.player.fishCaught;
		if (fishCaught != null && fishCaught.Length == 0)
		{
			this.distanceFromCatching = 0.1f;
			if (this.difficulty < 50f)
			{
				this.difficulty = 50f;
			}
		}
		this.Reposition();
		this.bobberBarHeight += Utility.getStringCountInList(bobbers, "(O)695") * 24;
		if (baitID == "(O)DeluxeBait")
		{
			this.bobberBarHeight += 12;
		}
		this.bobberBarPos = 568 - this.bobberBarHeight;
		this.bobberPosition = 508f;
		this.bobberTargetPosition = (100f - this.difficulty) / 100f * 548f;
		if (baitID == "(O)ChallengeBait")
		{
			this.challengeBaitFishes = 3;
		}
		Game1.setRichPresence("fishing", Game1.currentLocation.Name);
	}

	public virtual void Reposition()
	{
		switch (Game1.player.FacingDirection)
		{
		case 1:
			base.xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
			base.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
			break;
		case 3:
			base.xPositionOnScreen = (int)Game1.player.Position.X + 128;
			base.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
			this.flipBubble = true;
			break;
		case 0:
			base.xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
			base.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
			break;
		case 2:
			base.xPositionOnScreen = (int)Game1.player.Position.X - 64 - 132;
			base.yPositionOnScreen = (int)Game1.player.Position.Y - 274;
			break;
		}
		base.xPositionOnScreen -= Game1.viewport.X;
		base.yPositionOnScreen -= Game1.viewport.Y + 64;
		if (base.xPositionOnScreen + 96 > Game1.viewport.Width)
		{
			base.xPositionOnScreen = Game1.viewport.Width - 96;
		}
		else if (base.xPositionOnScreen < 0)
		{
			base.xPositionOnScreen = 0;
		}
		if (base.yPositionOnScreen < 0)
		{
			base.yPositionOnScreen = 0;
		}
		else if (base.yPositionOnScreen + 636 > Game1.viewport.Height)
		{
			base.yPositionOnScreen = Game1.viewport.Height - 636;
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		this.Reposition();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
	}

	private static int SafeNext(Random random, int minValue, int maxValue)
	{
		if (minValue >= maxValue)
		{
			return maxValue;
		}
		return random.Next(minValue, maxValue);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		this.Reposition();
		if (this.sparkleText != null && this.sparkleText.update(time))
		{
			this.sparkleText = null;
		}
		if (this.everythingShakeTimer > 0f)
		{
			this.everythingShakeTimer -= time.ElapsedGameTime.Milliseconds;
			this.everythingShake = new Vector2((float)Game1.random.Next(-10, 11) / 10f, (float)Game1.random.Next(-10, 11) / 10f);
			if (this.everythingShakeTimer <= 0f)
			{
				this.everythingShake = Vector2.Zero;
			}
		}
		if (this.fadeIn)
		{
			this.scale += 0.05f;
			if (this.scale >= 1f)
			{
				this.scale = 1f;
				this.fadeIn = false;
			}
		}
		else if (this.fadeOut)
		{
			if (this.everythingShakeTimer > 0f || this.sparkleText != null)
			{
				return;
			}
			this.scale -= 0.05f;
			if (this.scale <= 0f)
			{
				this.scale = 0f;
				this.fadeOut = false;
				FishingRod rod = Game1.player.CurrentTool as FishingRod;
				string baitId = rod?.GetBait()?.QualifiedItemId;
				int numCaught = ((this.bossFish || !(baitId == "(O)774") || !(Game1.random.NextDouble() < 0.25 + Game1.player.DailyLuck / 2.0)) ? 1 : 2);
				if (this.challengeBaitFishes > 0)
				{
					numCaught = this.challengeBaitFishes;
				}
				if (this.distanceFromCatching > 0.9f && rod != null)
				{
					rod.pullFishFromWater(this.whichFish, this.fishSize, this.fishQuality, (int)this.difficulty, this.treasureCaught, this.perfect, this.fromFishPond, this.setFlagOnCatch, this.bossFish, numCaught);
				}
				else
				{
					Game1.player.completelyStopAnimatingOrDoingAction();
					rod?.doneFishing(Game1.player, consumeBaitAndTackle: true);
				}
				Game1.exitActiveMenu();
				Game1.setRichPresence("location", Game1.currentLocation.Name);
			}
		}
		else
		{
			if (Game1.random.NextDouble() < (double)(this.difficulty * (float)((this.motionType != 2) ? 1 : 20) / 4000f) && (this.motionType != 2 || this.bobberTargetPosition == -1f))
			{
				float spaceBelow = 548f - this.bobberPosition;
				float spaceAbove = this.bobberPosition;
				float percent = Math.Min(99f, this.difficulty + (float)Game1.random.Next(10, 45)) / 100f;
				this.bobberTargetPosition = this.bobberPosition + (float)Game1.random.Next((int)Math.Min(0f - spaceAbove, spaceBelow), (int)spaceBelow) * percent;
			}
			switch (this.motionType)
			{
			case 4:
				this.floaterSinkerAcceleration = Math.Max(this.floaterSinkerAcceleration - 0.01f, -1.5f);
				break;
			case 3:
				this.floaterSinkerAcceleration = Math.Min(this.floaterSinkerAcceleration + 0.01f, 1.5f);
				break;
			}
			if (Math.Abs(this.bobberPosition - this.bobberTargetPosition) > 3f && this.bobberTargetPosition != -1f)
			{
				this.bobberAcceleration = (this.bobberTargetPosition - this.bobberPosition) / ((float)Game1.random.Next(10, 30) + (100f - Math.Min(100f, this.difficulty)));
				this.bobberSpeed += (this.bobberAcceleration - this.bobberSpeed) / 5f;
			}
			else if (this.motionType != 2 && Game1.random.NextDouble() < (double)(this.difficulty / 2000f))
			{
				this.bobberTargetPosition = this.bobberPosition + (float)(Game1.random.NextBool() ? Game1.random.Next(-100, -51) : Game1.random.Next(50, 101));
			}
			else
			{
				this.bobberTargetPosition = -1f;
			}
			if (this.motionType == 1 && Game1.random.NextDouble() < (double)(this.difficulty / 1000f))
			{
				this.bobberTargetPosition = this.bobberPosition + (float)(Game1.random.NextBool() ? BobberBar.SafeNext(Game1.random, -100 - (int)this.difficulty * 2, -51) : BobberBar.SafeNext(Game1.random, 50, 101 + (int)this.difficulty * 2));
			}
			this.bobberTargetPosition = Math.Max(-1f, Math.Min(this.bobberTargetPosition, 548f));
			this.bobberPosition += this.bobberSpeed + this.floaterSinkerAcceleration;
			if (this.bobberPosition > 532f)
			{
				this.bobberPosition = 532f;
			}
			else if (this.bobberPosition < 0f)
			{
				this.bobberPosition = 0f;
			}
			this.bobberInBar = this.bobberPosition + 12f <= this.bobberBarPos - 32f + (float)this.bobberBarHeight && this.bobberPosition - 16f >= this.bobberBarPos - 32f;
			if (this.bobberPosition >= (float)(548 - this.bobberBarHeight) && this.bobberBarPos >= (float)(568 - this.bobberBarHeight - 4))
			{
				this.bobberInBar = true;
			}
			bool num = this.buttonPressed;
			this.buttonPressed = Game1.oldMouseState.LeftButton == ButtonState.Pressed || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton) || (Game1.options.gamepadControls && (Game1.oldPadState.IsButtonDown(Buttons.X) || Game1.oldPadState.IsButtonDown(Buttons.A)));
			if (!num && this.buttonPressed)
			{
				Game1.playSound("fishingRodBend");
			}
			float gravity = (this.buttonPressed ? (-0.25f) : 0.25f);
			if (this.buttonPressed && gravity < 0f && (this.bobberBarPos == 0f || this.bobberBarPos == (float)(568 - this.bobberBarHeight)))
			{
				this.bobberBarSpeed = 0f;
			}
			if (this.bobberInBar)
			{
				gravity *= (this.bobbers.Contains("(O)691") ? 0.3f : 0.6f);
				if (this.bobbers.Contains("(O)691"))
				{
					for (int i = 0; i < Utility.getStringCountInList(this.bobbers, "(O)691"); i++)
					{
						if (this.bobberPosition + 16f < this.bobberBarPos + (float)(this.bobberBarHeight / 2))
						{
							this.bobberBarSpeed -= ((i > 0) ? 0.05f : 0.2f);
						}
						else
						{
							this.bobberBarSpeed += ((i > 0) ? 0.05f : 0.2f);
						}
						if (i > 0)
						{
							gravity *= 0.9f;
						}
					}
				}
			}
			float oldPos = this.bobberBarPos;
			this.bobberBarSpeed += gravity;
			this.bobberBarPos += this.bobberBarSpeed;
			if (this.bobberBarPos + (float)this.bobberBarHeight > 568f)
			{
				this.bobberBarPos = 568 - this.bobberBarHeight;
				this.bobberBarSpeed = (0f - this.bobberBarSpeed) * 2f / 3f * (this.bobbers.Contains("(O)692") ? ((float)Utility.getStringCountInList(this.bobbers, "(O)692") * 0.1f) : 1f);
				if (oldPos + (float)this.bobberBarHeight < 568f)
				{
					Game1.playSound("shiny4");
				}
			}
			else if (this.bobberBarPos < 0f)
			{
				this.bobberBarPos = 0f;
				this.bobberBarSpeed = (0f - this.bobberBarSpeed) * 2f / 3f;
				if (oldPos > 0f)
				{
					Game1.playSound("shiny4");
				}
			}
			bool treasureInBar = false;
			if (this.treasure)
			{
				float oldTreasureAppearTimer = this.treasureAppearTimer;
				this.treasureAppearTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.treasureAppearTimer <= 0f)
				{
					if (this.treasureScale < 1f && !this.treasureCaught)
					{
						if (oldTreasureAppearTimer > 0f)
						{
							if (this.bobberBarPos > 274f)
							{
								this.treasurePosition = Game1.random.Next(8, (int)this.bobberBarPos - 20);
							}
							else
							{
								int min = Math.Min(528, (int)this.bobberBarPos + this.bobberBarHeight);
								int max = 500;
								this.treasurePosition = ((min > max) ? (max - 1) : Game1.random.Next(min, max));
							}
							Game1.playSound("dwop");
						}
						this.treasureScale = Math.Min(1f, this.treasureScale + 0.1f);
					}
					treasureInBar = this.treasurePosition + 12f <= this.bobberBarPos - 32f + (float)this.bobberBarHeight && this.treasurePosition - 16f >= this.bobberBarPos - 32f;
					if (treasureInBar && !this.treasureCaught)
					{
						this.treasureCatchLevel += 0.0135f;
						this.treasureShake = new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3));
						if (this.treasureCatchLevel >= 1f)
						{
							Game1.playSound("newArtifact");
							this.treasureCaught = true;
						}
					}
					else if (this.treasureCaught)
					{
						this.treasureScale = Math.Max(0f, this.treasureScale - 0.1f);
					}
					else
					{
						this.treasureShake = Vector2.Zero;
						this.treasureCatchLevel = Math.Max(0f, this.treasureCatchLevel - 0.01f);
					}
				}
			}
			if (this.bobberInBar)
			{
				this.distanceFromCatching += 0.002f;
				this.reelRotation += (float)Math.PI / 8f;
				this.fishShake.X = (float)Game1.random.Next(-10, 11) / 10f;
				this.fishShake.Y = (float)Game1.random.Next(-10, 11) / 10f;
				this.barShake = Vector2.Zero;
				Rumble.rumble(0.1f, 1000f);
				BobberBar.unReelSound?.Stop(AudioStopOptions.Immediate);
				if (BobberBar.reelSound == null || BobberBar.reelSound.IsStopped || BobberBar.reelSound.IsStopping || !BobberBar.reelSound.IsPlaying)
				{
					Game1.playSound("fastReel", out BobberBar.reelSound);
				}
			}
			else if (!treasureInBar || this.treasureCaught || !this.bobbers.Contains("(O)693"))
			{
				if (!this.fishShake.Equals(Vector2.Zero))
				{
					Game1.playSound("tinyWhip");
					this.perfect = false;
					Rumble.stopRumbling();
					if (this.challengeBaitFishes > 0)
					{
						this.challengeBaitFishes--;
						if (this.challengeBaitFishes <= 0)
						{
							this.distanceFromCatching = 0f;
						}
					}
				}
				this.fishSizeReductionTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.fishSizeReductionTimer <= 0)
				{
					this.fishSize = Math.Max(this.minFishSize, this.fishSize - 1);
					this.fishSizeReductionTimer = 800;
				}
				if ((Game1.player.fishCaught != null && Game1.player.fishCaught.Length != 0) || Game1.currentMinigame != null)
				{
					if (this.bobbers.Contains("(O)694"))
					{
						float reduction = 0.003f;
						float amount = 0.001f;
						for (int j = 0; j < Utility.getStringCountInList(this.bobbers, "(O)694"); j++)
						{
							reduction -= amount;
							amount /= 2f;
						}
						reduction = Math.Max(0.001f, reduction);
						this.distanceFromCatching -= reduction * this.distanceFromCatchPenaltyModifier;
					}
					else
					{
						this.distanceFromCatching -= (this.beginnersRod ? 0.002f : 0.003f) * this.distanceFromCatchPenaltyModifier;
					}
				}
				float distanceAway = Math.Abs(this.bobberPosition - (this.bobberBarPos + (float)(this.bobberBarHeight / 2)));
				this.reelRotation -= (float)Math.PI / Math.Max(10f, 200f - distanceAway);
				this.barShake.X = (float)Game1.random.Next(-10, 11) / 10f;
				this.barShake.Y = (float)Game1.random.Next(-10, 11) / 10f;
				this.fishShake = Vector2.Zero;
				BobberBar.reelSound?.Stop(AudioStopOptions.Immediate);
				if (BobberBar.unReelSound == null || BobberBar.unReelSound.IsStopped)
				{
					Game1.playSound("slowReel", 600, out BobberBar.unReelSound);
				}
			}
			this.distanceFromCatching = Math.Max(0f, Math.Min(1f, this.distanceFromCatching));
			if (Game1.player.CurrentTool != null)
			{
				Game1.player.CurrentTool.tickUpdate(time, Game1.player);
			}
			if (this.distanceFromCatching <= 0f)
			{
				this.fadeOut = true;
				this.everythingShakeTimer = 500f;
				Game1.playSound("fishEscape");
				this.handledFishResult = true;
				BobberBar.unReelSound?.Stop(AudioStopOptions.Immediate);
				BobberBar.reelSound?.Stop(AudioStopOptions.Immediate);
			}
			else if (this.distanceFromCatching >= 1f)
			{
				this.everythingShakeTimer = 500f;
				Game1.playSound("jingle1");
				this.fadeOut = true;
				this.handledFishResult = true;
				BobberBar.unReelSound?.Stop(AudioStopOptions.Immediate);
				BobberBar.reelSound?.Stop(AudioStopOptions.Immediate);
				if (this.perfect)
				{
					this.sparkleText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, rainbow: false, 0.1, 1500);
					if (Game1.isFestival())
					{
						Game1.CurrentEvent.perfectFishing();
					}
				}
				else if (this.fishSize == this.maxFishSize)
				{
					this.fishSize--;
				}
			}
		}
		if (this.bobberPosition < 0f)
		{
			this.bobberPosition = 0f;
		}
		if (this.bobberPosition > 548f)
		{
			this.bobberPosition = 548f;
		}
	}

	public override bool readyToClose()
	{
		return false;
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		BobberBar.unReelSound?.Stop(AudioStopOptions.Immediate);
		BobberBar.reelSound?.Stop(AudioStopOptions.Immediate);
		if (!this.handledFishResult)
		{
			Game1.playSound("fishEscape");
		}
		this.fadeOut = true;
		this.everythingShakeTimer = 500f;
		this.distanceFromCatching = -1f;
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.menuButton.Contains(new InputButton(key)))
		{
			this.emergencyShutDown();
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		Game1.StartWorldDrawInUI(b);
		b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen - (this.flipBubble ? 44 : 20) + 104, base.yPositionOnScreen - 16 + 314) + this.everythingShake, new Rectangle(652, 1685, 52, 157), Color.White * 0.6f * this.scale, 0f, new Vector2(26f, 78.5f) * this.scale, 4f * this.scale, this.flipBubble ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.001f);
		b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 70, base.yPositionOnScreen + 296) + this.everythingShake, new Rectangle(644, 1999, 38, 150), Color.White * this.scale, 0f, new Vector2(18.5f, 74f) * this.scale, 4f * this.scale, SpriteEffects.None, 0.01f);
		if (this.scale == 1f)
		{
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 64, base.yPositionOnScreen + 12 + (int)this.bobberBarPos) + this.barShake + this.everythingShake, new Rectangle(682, 2078, 9, 2), this.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 64, base.yPositionOnScreen + 12 + (int)this.bobberBarPos + 8) + this.barShake + this.everythingShake, new Rectangle(682, 2081, 9, 1), this.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, new Vector2(4f, this.bobberBarHeight - 16), SpriteEffects.None, 0.89f);
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 64, base.yPositionOnScreen + 12 + (int)this.bobberBarPos + this.bobberBarHeight - 8) + this.barShake + this.everythingShake, new Rectangle(682, 2085, 9, 2), this.bobberInBar ? Color.White : (Color.White * 0.25f * ((float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0), 2) + 2f)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
			b.Draw(Game1.staminaRect, new Rectangle(base.xPositionOnScreen + 124, base.yPositionOnScreen + 4 + (int)(580f * (1f - this.distanceFromCatching)), 16, (int)(580f * this.distanceFromCatching)), Utility.getRedToGreenLerpColor(this.distanceFromCatching));
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 18, base.yPositionOnScreen + 514) + this.everythingShake, new Rectangle(257, 1990, 5, 10), Color.White, this.reelRotation, new Vector2(2f, 10f), 4f, SpriteEffects.None, 0.9f);
			if (this.goldenTreasure)
			{
				b.Draw(Game1.mouseCursors_1_6, new Vector2(base.xPositionOnScreen + 64 + 18, (float)(base.yPositionOnScreen + 12 + 24) + this.treasurePosition) + this.treasureShake + this.everythingShake, new Rectangle(256, 51, 20, 24), Color.White, 0f, new Vector2(10f, 10f), 2f * this.treasureScale, SpriteEffects.None, 0.85f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 64 + 18, (float)(base.yPositionOnScreen + 12 + 24) + this.treasurePosition) + this.treasureShake + this.everythingShake, new Rectangle(638, 1865, 20, 24), Color.White, 0f, new Vector2(10f, 10f), 2f * this.treasureScale, SpriteEffects.None, 0.85f);
			}
			if (this.treasureCatchLevel > 0f && !this.treasureCaught)
			{
				b.Draw(Game1.staminaRect, new Rectangle(base.xPositionOnScreen + 64, base.yPositionOnScreen + 12 + (int)this.treasurePosition, 40, 8), Color.DimGray * 0.5f);
				b.Draw(Game1.staminaRect, new Rectangle(base.xPositionOnScreen + 64, base.yPositionOnScreen + 12 + (int)this.treasurePosition, (int)(this.treasureCatchLevel * 40f), 8), Color.Orange);
			}
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 64 + 18, (float)(base.yPositionOnScreen + 12 + 24) + this.bobberPosition) + this.fishShake + this.everythingShake, new Rectangle(614 + (this.bossFish ? 20 : 0), 1840, 20, 20), Color.White, 0f, new Vector2(10f, 10f), 2f, SpriteEffects.None, 0.88f);
			this.sparkleText?.draw(b, new Vector2(base.xPositionOnScreen - 16, base.yPositionOnScreen - 64));
			if (this.bobbers.Contains("(O)SonarBobber"))
			{
				int xPosition = (((float)base.xPositionOnScreen > (float)Game1.viewport.Width * 0.75f) ? (base.xPositionOnScreen - 80) : (base.xPositionOnScreen + 216));
				bool flip = xPosition < base.xPositionOnScreen;
				b.Draw(Game1.mouseCursors_1_6, new Vector2(xPosition - 12, base.yPositionOnScreen + 40) + this.everythingShake, new Rectangle(227, 6, 29, 24), Color.White, 0f, new Vector2(10f, 10f), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.88f);
				this.fishObject.drawInMenu(b, new Vector2(xPosition, base.yPositionOnScreen) + new Vector2(flip ? (-8) : (-4), 4f) * 4f + this.everythingShake, 1f);
			}
			if (this.challengeBaitFishes > -1)
			{
				int xPosition2 = (((float)base.xPositionOnScreen > (float)Game1.viewport.Width * 0.75f) ? (base.xPositionOnScreen - 80) : (base.xPositionOnScreen + 216));
				int yPos = (this.bobbers.Contains("(O)SonarBobber") ? (base.yPositionOnScreen + 136) : (base.yPositionOnScreen + 40));
				Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2((float)(xPosition2 - 24) + this.everythingShake.X, (float)(yPos - 16) + this.everythingShake.Y), new Rectangle(240, 31, 15, 38), Color.White, 0f, Vector2.Zero, 4f);
				for (int y = 0; y < 3; y++)
				{
					if (y < this.challengeBaitFishes)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2(xPosition2 - 12, (float)yPos + (float)(y * 20) * 2f) + this.everythingShake, new Rectangle(236, 205, 19, 19), Color.White, 0f, new Vector2(0f, 0f), 2f, flipped: false, 0.88f);
					}
					else
					{
						b.Draw(Game1.mouseCursors_1_6, new Vector2(xPosition2 - 12, (float)yPos + (float)(y * 20) * 2f) + this.everythingShake, new Rectangle(217, 205, 19, 19), Color.White, 0f, new Vector2(0f, 0f), 2f, SpriteEffects.None, 0.88f);
					}
				}
			}
		}
		NetStringIntArrayDictionary fishCaught = Game1.player.fishCaught;
		if (fishCaught != null && fishCaught.Length == 0)
		{
			Vector2 pos = new Vector2(base.xPositionOnScreen + (this.flipBubble ? (base.width + 64 + 8) : (-200)), base.yPositionOnScreen + 192);
			if (!Game1.options.gamepadControls)
			{
				b.Draw(Game1.mouseCursors, pos, new Rectangle(644, 1330, 48, 69), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
			else
			{
				b.Draw(Game1.controllerMaps, pos, Utility.controllerMapSourceRect(new Rectangle(681, 0, 96, 138)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
		}
		Game1.EndWorldDrawInUI(b);
	}
}
