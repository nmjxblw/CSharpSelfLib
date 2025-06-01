using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Projectiles;
using xTile.Dimensions;

namespace StardewValley.Minigames;

[InstanceStatics]
public class TargetGame : IMinigame
{
	public class Target
	{
		public static int width = 56;

		public static int spawnRightPosition = 960;

		public static int spawnLeftPosition = 0;

		public static int basicTarget = 0;

		public static int bonusTarget = 1;

		public static int deluxeTarget = 2;

		public static int mediumSpeed = 4;

		public static int slowSpeed = 2;

		public static int fastSpeed = 5;

		public static int nearLane = 448;

		public static int middleLane = 320;

		public static int farLane = 128;

		public static int superNearLane = 576;

		public static int behindLane = 832;

		public static int pauseFarRight = 832;

		public static int pauseRight = 704;

		public static int pauseMiddleRight = 576;

		public static int pauseMiddleLeft = 384;

		public static int pauseLeft = 256;

		public static int pauseFarLeft = 128;

		public Microsoft.Xna.Framework.Rectangle Position;

		private int targetType;

		private int countdownBeforeSpawn;

		private int xPausePosition;

		private int xPauseTime;

		private int speed;

		private bool spawned;

		private bool atPausePosition;

		private Microsoft.Xna.Framework.Rectangle sourceRect;

		public Target(int countdownBeforeSpawn, int whichLane, int type = 0, int speed = 4, bool spawnFromRight = true, int pauseAndReturn = -1, int pauseTime = -1)
		{
			this.countdownBeforeSpawn = countdownBeforeSpawn;
			this.targetType = type;
			this.speed = speed * ((!spawnFromRight) ? 1 : (-1));
			this.Position = new Microsoft.Xna.Framework.Rectangle(spawnFromRight ? Target.spawnRightPosition : Target.spawnLeftPosition, whichLane, Target.width, Target.width);
			this.xPausePosition = pauseAndReturn;
			this.xPauseTime = pauseTime;
			this.sourceRect = new Microsoft.Xna.Framework.Rectangle(289, 1184 + type * 16, 14, 14);
		}

		public bool update(GameTime time, GameLocation location)
		{
			if (this.countdownBeforeSpawn > 0)
			{
				this.countdownBeforeSpawn -= time.ElapsedGameTime.Milliseconds;
				if (this.countdownBeforeSpawn <= 0)
				{
					this.spawned = true;
				}
			}
			if (this.spawned)
			{
				if (this.atPausePosition)
				{
					this.xPauseTime -= time.ElapsedGameTime.Milliseconds;
					if (this.xPauseTime <= 0)
					{
						this.speed = -this.speed;
						this.atPausePosition = false;
						this.xPausePosition = -1;
					}
				}
				else
				{
					this.Position.X += this.speed;
					if (this.xPausePosition != -1 && Math.Abs(this.xPausePosition - this.Position.X) <= Math.Abs(this.speed))
					{
						this.atPausePosition = true;
					}
				}
				if (this.Position.X < 0 || this.Position.Right > Target.spawnRightPosition + 64)
				{
					return true;
				}
				bool projectileHit = false;
				location.projectiles.RemoveWhere(delegate(Projectile projectile)
				{
					if (projectile.getBoundingBox().Intersects(this.Position))
					{
						this.shatter(location, projectile);
						projectileHit = true;
						if (this.targetType != Target.basicTarget)
						{
							projectile.behaviorOnCollisionWithOther(location);
							return true;
						}
					}
					return false;
				});
				return projectileHit;
			}
			return false;
		}

		public void shatter(GameLocation location, Projectile stone)
		{
			int scoreToAdd = 0;
			if (this.targetType == Target.basicTarget)
			{
				Game1.playSound("breakingGlass");
				scoreToAdd++;
			}
			if (this.targetType == Target.bonusTarget)
			{
				Game1.playSound("potterySmash");
				scoreToAdd += 2;
			}
			if (this.targetType == Target.deluxeTarget)
			{
				Game1.playSound("potterySmash");
				scoreToAdd += 5;
			}
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(304, 1183 + this.targetType * 16, 16, 16), 60f, 3, 0, new Vector2(this.Position.X - 4, this.Position.Y - 4), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
			location.debris.Add(new Debris(scoreToAdd, new Vector2(this.Position.Center.X, this.Position.Center.Y), new Color(255, 130, 0), 1f, null));
			TargetGame.score += scoreToAdd;
			if (stone is BasicProjectile projectile && projectile.damageToFarmer.Value > 0)
			{
				TargetGame.successShots++;
				projectile.damageToFarmer.Value = -1;
			}
		}

		public void draw(SpriteBatch b)
		{
			if (this.spawned)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.Position.X, this.Position.Bottom + 32)), Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.Position), this.sourceRect, Color.White);
			}
		}
	}

	private GameLocation location;

	private int timerToStart = 1000;

	private int gameEndTimer = 61000;

	private int showResultsTimer = -1;

	private bool gameDone;

	private bool exit;

	public static int score;

	public static int shotsFired;

	public static int successShots;

	public static int accuracy = -1;

	public static int starTokensWon;

	public List<Target> targets;

	private float modifierBonus;

	public TargetGame()
	{
		TargetGame.score = 0;
		TargetGame.successShots = 0;
		TargetGame.shotsFired = 0;
		this.location = new GameLocation("Maps\\TargetGame", "tent");
		Tool slingshot = ItemRegistry.Create<Tool>("(W)32");
		slingshot.attachments[0] = ItemRegistry.Create<Object>("(O)390", 999);
		Game1.player.TemporaryItem = slingshot;
		Game1.player.CurrentToolIndex = 0;
		Game1.globalFadeToClear(null, 0.01f);
		this.location.Map.LoadTileSheets(Game1.mapDisplayDevice);
		Game1.player.Position = new Vector2(8f, 13f) * 64f;
		this.changeScreenSize();
		this.gameEndTimer = 50000;
		this.targets = new List<Target>();
		this.addTargets();
	}

	public bool overrideFreeMouseMovement()
	{
		return false;
	}

	public bool tick(GameTime time)
	{
		this.location.UpdateWhenCurrentLocation(time);
		this.location.wasUpdated = false;
		this.location.updateEvenIfFarmerIsntHere(time);
		Game1.player.Stamina = Game1.player.MaxStamina;
		Game1.player.Update(time, this.location);
		if ((Game1.oldKBState.GetPressedKeys().Length == 0 || (Game1.oldKBState.GetPressedKeys().Length == 1 && Game1.options.doesInputListContain(Game1.options.runButton, Game1.oldKBState.GetPressedKeys()[0])) || !Game1.player.movedDuringLastTick()) && !Game1.player.UsingTool)
		{
			Game1.player.Halt();
		}
		if (this.timerToStart > 0)
		{
			this.timerToStart -= time.ElapsedGameTime.Milliseconds;
			if (this.timerToStart <= 0)
			{
				Game1.playSound("whistle");
				Game1.changeMusicTrack("tickTock", track_interruptable: false, MusicContext.MiniGame);
			}
		}
		else if (this.showResultsTimer >= 0)
		{
			int num = this.showResultsTimer;
			this.showResultsTimer -= time.ElapsedGameTime.Milliseconds;
			if (num > 16000 && this.showResultsTimer <= 16000)
			{
				Game1.playSound("smallSelect");
			}
			if (num > 14000 && this.showResultsTimer <= 14000)
			{
				Game1.playSound("smallSelect");
				TargetGame.accuracy = (int)Math.Max(0.0, Math.Round((float)TargetGame.successShots / (float)(TargetGame.shotsFired - 1), 2) * 100.0);
			}
			if (num > 11000 && this.showResultsTimer <= 11000)
			{
				if (TargetGame.accuracy >= 75)
				{
					Game1.playSound("newArtifact");
					float modifier = 1.5f;
					if (TargetGame.accuracy >= 85)
					{
						modifier = 2f;
					}
					if (TargetGame.accuracy >= 90)
					{
						modifier = 2.5f;
					}
					if (TargetGame.accuracy >= 95)
					{
						modifier = 3f;
					}
					if (TargetGame.accuracy >= 100)
					{
						modifier = 4f;
					}
					TargetGame.score = (int)((float)TargetGame.score * modifier);
					this.modifierBonus = modifier;
				}
				else
				{
					Game1.playSound("smallSelect");
				}
			}
			if (num > 9000 && this.showResultsTimer <= 9000)
			{
				if (TargetGame.score >= 40)
				{
					Game1.playSound("reward");
					TargetGame.starTokensWon = (int)((float)((TargetGame.score * 2 - 30) / 10) * 2.5f);
					TargetGame.starTokensWon *= 2;
					if (TargetGame.starTokensWon > 280)
					{
						TargetGame.starTokensWon = 500;
					}
					Game1.player.festivalScore += TargetGame.starTokensWon;
				}
				else
				{
					Game1.playSound("fishEscape");
				}
			}
			if (this.showResultsTimer <= 0)
			{
				Game1.globalFadeToClear();
				Game1.player.Position = ((Game1.year % 2 == 0) ? (new Vector2(24f, 70f) * 64f) : (new Vector2(24f, 63f) * 64f));
				return true;
			}
		}
		else if (!this.gameDone)
		{
			this.gameEndTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.gameEndTimer <= 0)
			{
				Game1.playSound("whistle");
				this.gameEndTimer = 1000;
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.canMove = false;
				this.gameDone = true;
			}
			this.targets.RemoveAll((Target target) => target.update(time, this.location));
		}
		else if (this.gameDone && this.gameEndTimer > 0)
		{
			this.gameEndTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.gameEndTimer <= 0)
			{
				Game1.globalFadeToBlack(gameDoneAfterFade, 0.01f);
				Game1.player.forceCanMove();
			}
		}
		return this.exit;
	}

	public void gameDoneAfterFade()
	{
		this.showResultsTimer = 16100;
		Game1.player.canMove = false;
		Game1.player.freezePause = 16100;
		Game1.player.Position = ((Game1.year % 2 == 0) ? (new Vector2(24f, 70f) * 64f) : (new Vector2(24f, 63f) * 64f));
		Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(Game1.player.TilePoint.X * 64, Game1.player.TilePoint.Y * 64, 64, 64));
		Game1.player.faceDirection(2);
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.showResultsTimer < 0)
		{
			Game1.pressUseToolButton();
		}
		else if (this.showResultsTimer > 16000)
		{
			this.showResultsTimer = 16001;
		}
		else if (this.showResultsTimer > 14000)
		{
			this.showResultsTimer = 14001;
		}
		else if (this.showResultsTimer > 11000)
		{
			this.showResultsTimer = 11001;
		}
		else if (this.showResultsTimer > 9000)
		{
			this.showResultsTimer = 9001;
		}
		else if (this.showResultsTimer < 9000 && this.showResultsTimer > 1000)
		{
			this.showResultsTimer = 1500;
			Game1.player.freezePause = 1500;
			Game1.playSound("smallSelect");
		}
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public void releaseLeftClick(int x, int y)
	{
		int projectileCount = this.location.projectiles.Count;
		if (this.showResultsTimer < 0 && Game1.player.CurrentTool != null && Game1.player.UsingTool && Game1.player.CurrentTool.onRelease(this.location, x, y, Game1.player))
		{
			Game1.player.usingSlingshot = false;
			Game1.player.canReleaseTool = true;
			Game1.player.UsingTool = false;
			Game1.player.CanMove = true;
			if (this.location.projectiles.Count > projectileCount)
			{
				TargetGame.shotsFired++;
			}
		}
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveKeyPress(Keys k)
	{
		if (Game1.options.doesInputListContain(Game1.options.menuButton, k))
		{
			Game1.playSound("fishEscape");
			this.showResultsTimer = 1;
		}
		if (this.showResultsTimer > 0 || this.gameEndTimer > 0)
		{
			Game1.player.Halt();
			return;
		}
		if (Game1.player.movementDirections.Count < 2 && !Game1.player.UsingTool && this.timerToStart <= 0)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
			{
				Game1.player.setMoving(1);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, k))
			{
				Game1.player.setMoving(2);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
			{
				Game1.player.setMoving(4);
			}
			if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, k))
			{
				Game1.player.setMoving(8);
			}
		}
		if (Game1.options.doesInputListContain(Game1.options.runButton, k))
		{
			Game1.player.setRunning(isRunning: true);
		}
	}

	public void receiveKeyRelease(Keys k)
	{
		if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
		{
			Game1.player.setMoving(33);
		}
		if (Game1.options.doesInputListContain(Game1.options.moveRightButton, k))
		{
			Game1.player.setMoving(34);
		}
		if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
		{
			Game1.player.setMoving(36);
		}
		if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, k))
		{
			Game1.player.setMoving(40);
		}
		if (Game1.options.doesInputListContain(Game1.options.runButton, k))
		{
			Game1.player.setRunning(isRunning: false);
		}
	}

	public void draw(SpriteBatch b)
	{
		if (this.showResultsTimer < 0)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			Game1.mapDisplayDevice.BeginScene(b);
			this.location.Map.RequireLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f - ((Game1.player.running || Game1.player.UsingTool) ? ((float)Math.Abs(FarmerRenderer.featureYOffsetPerFrame[Game1.player.FarmerSprite.CurrentFrame]) * 0.8f) : 0f), SpriteEffects.None, Math.Max(0f, (float)Game1.player.StandingPixel.Y / 10000f + 0.00011f) - 1E-07f);
			this.location.Map.RequireLayer("Buildings").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
			Game1.mapDisplayDevice.EndScene();
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			this.location.draw(b);
			Game1.player.draw(b);
			foreach (Target target in this.targets)
			{
				target.draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			Game1.mapDisplayDevice.BeginScene(b);
			this.location.Map.RequireLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, wrapAround: false, 4);
			Game1.mapDisplayDevice.EndScene();
			this.location.drawAboveAlwaysFrontLayer(b);
			Game1.player.CurrentTool.draw(b);
			Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", TargetGame.score), Color.Black, Color.White, new Vector2(32f, 32f));
			Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", this.gameEndTimer / 1000), Color.Black, Color.White, new Vector2(32f, 64f));
			if (TargetGame.shotsFired > 1)
			{
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:TargetGame.cs.12154", (int)(Math.Round((float)TargetGame.successShots / (float)(TargetGame.shotsFired - 1), 2) * 100.0)), Color.Black, Color.White, new Vector2(32f, 96f));
			}
			b.End();
			return;
		}
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		Vector2 position = new Vector2(Game1.viewport.Width / 2 - 128, Game1.viewport.Height / 2 - 64);
		if (this.showResultsTimer <= 16000)
		{
			Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", TargetGame.score), Game1.textColor, (this.showResultsTimer <= 11000 && this.modifierBonus > 1f) ? Color.Lime : Color.White, position);
		}
		if (this.showResultsTimer <= 14000)
		{
			position.Y += 48f;
			Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:TargetGame.cs.12157", TargetGame.accuracy, TargetGame.successShots, TargetGame.shotsFired), Game1.textColor, Color.White, position);
		}
		if (this.showResultsTimer <= 11000)
		{
			position.Y += 48f;
			if (this.modifierBonus > 1f)
			{
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:TargetGame.cs.12161", this.modifierBonus), Game1.textColor, Color.Yellow, position);
			}
			else
			{
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:TargetGame.cs.12163"), Game1.textColor, Color.Red, position);
			}
		}
		if (this.showResultsTimer <= 9000)
		{
			position.Y += 64f;
			if (TargetGame.starTokensWon > 0)
			{
				float fade = Math.Min(1f, (float)(this.showResultsTimer - 2000) / 4000f);
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", TargetGame.starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", TargetGame.starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", TargetGame.starTokensWon), Game1.textColor * 0.2f * fade, Color.SkyBlue * 0.3f * fade, position + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) * 4f * 2f, 0f, 1f, 1f);
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12013", TargetGame.starTokensWon), Game1.textColor, Color.SkyBlue, position, 0f, 1f, 1f);
			}
			else
			{
				Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.12021"), Game1.textColor, Color.Red, position);
			}
		}
		if (this.showResultsTimer <= 1000)
		{
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * (1f - (float)this.showResultsTimer / 1000f));
		}
		b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
		b.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(72f, 29f), 0f, 1f, 1f, tiny: false);
		b.End();
	}

	public static void startMe()
	{
		Game1.currentMinigame = new TargetGame();
		Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);
	}

	public void changeScreenSize()
	{
		Game1.viewport.X = this.location.Map.Layers[0].LayerWidth * 64 / 2 - Game1.viewport.Width / 2;
		Game1.viewport.Y = this.location.Map.Layers[0].LayerHeight * 64 / 2 - Game1.viewport.Height / 2;
	}

	public void unload()
	{
		Game1.player.TemporaryItem = null;
		Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);
		Game1.player.forceCanMove();
		Game1.stopMusicTrack(MusicContext.MiniGame);
	}

	public void addTargets()
	{
		this.addRowOfTargetsOnLane(0, Target.middleLane, 1500, 5, Target.mediumSpeed, spawnFromRight: false);
		this.addRowOfTargetsOnLane(4000, Target.nearLane, 1000, 5, Target.mediumSpeed);
		this.addRowOfTargetsOnLane(8000, Target.farLane, 2000, 5, Target.mediumSpeed, spawnFromRight: false, Target.bonusTarget);
		this.addTwinPausers(8000, Target.superNearLane, Target.pauseMiddleLeft, Target.fastSpeed, 2000, Target.bonusTarget);
		this.addTwinPausers(15000, Target.superNearLane, Target.pauseFarLeft, Target.mediumSpeed, 4000, Target.bonusTarget);
		this.addRowOfTargetsOnLane(18000, Target.middleLane, 1500, 5, Target.mediumSpeed, spawnFromRight: false);
		this.addRowOfTargetsOnLane(21000, Target.nearLane, 1000, 5, Target.mediumSpeed);
		this.addTwinPausers(25000, Target.behindLane, Target.pauseFarLeft, Target.fastSpeed, 1500, Target.deluxeTarget);
		this.addRowOfTargetsOnLane(27000, Target.superNearLane, 500, 8, Target.slowSpeed);
		this.addRowOfTargetsOnLane(28000, Target.nearLane, 500, 8, Target.slowSpeed);
		this.addRowOfTargetsOnLane(29000, Target.middleLane, 500, 8, Target.slowSpeed);
		this.addRowOfTargetsOnLane(30000, Target.farLane, 500, 8, Target.slowSpeed);
		this.addTwinPausers(36000, Target.behindLane, Target.pauseFarLeft, Target.fastSpeed, 2000, Target.deluxeTarget);
		this.addRowOfTargetsOnLane(41000, Target.middleLane, 1500, 5, Target.mediumSpeed, spawnFromRight: false);
		this.addRowOfTargetsOnLane(42000, Target.nearLane, 1000, 5, Target.mediumSpeed);
		this.addRowOfTargetsOnLane(43000, Target.farLane, 1000, 4, Target.mediumSpeed, spawnFromRight: false);
	}

	private void addTwinPausers(int initialDelay, int whichLane, int pauseArea, int speed, int pauseTime, int targetType)
	{
		int otherPauseArea = -1;
		bool firstIsSpawnLeft = false;
		if (pauseArea == Target.pauseFarLeft)
		{
			otherPauseArea = Target.pauseFarRight;
			firstIsSpawnLeft = true;
		}
		if (pauseArea == Target.pauseLeft)
		{
			otherPauseArea = Target.pauseRight;
			firstIsSpawnLeft = true;
		}
		if (pauseArea == Target.pauseMiddleLeft)
		{
			otherPauseArea = Target.pauseMiddleRight;
			firstIsSpawnLeft = true;
		}
		if (pauseArea == Target.pauseMiddleRight)
		{
			otherPauseArea = Target.pauseMiddleLeft;
		}
		if (pauseArea == Target.pauseRight)
		{
			otherPauseArea = Target.pauseLeft;
		}
		if (pauseArea == Target.pauseFarRight)
		{
			otherPauseArea = Target.pauseFarLeft;
		}
		this.targets.Add(new Target(initialDelay, whichLane, targetType, speed, !firstIsSpawnLeft, pauseArea, pauseTime));
		this.targets.Add(new Target(initialDelay, whichLane, targetType, speed, firstIsSpawnLeft, otherPauseArea, pauseTime));
	}

	private void addRowOfTargetsOnLane(int initialDelayBeforeStarting, int whichLane, int delayBetween, int numberOfTargets, int speed, bool spawnFromRight = true, int targetType = 0)
	{
		for (int i = 0; i < numberOfTargets; i++)
		{
			this.targets.Add(new Target(initialDelayBeforeStarting + i * delayBetween, whichLane, targetType, speed, spawnFromRight));
		}
	}

	public void receiveEventPoke(int data)
	{
	}

	public string minigameId()
	{
		return "TargetGame";
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool forceQuit()
	{
		return false;
	}
}
