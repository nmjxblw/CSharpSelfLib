using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events;

public class FairyEvent : BaseFarmEvent
{
	public string lightSourceId;

	private Vector2 fairyPosition;

	private Vector2 targetCrop;

	private Farm f;

	private int fairyFrame;

	private int fairyAnimationTimer;

	private int animationLoopsDone;

	private int timerSinceFade;

	private bool animateLeft;

	private bool terminate;

	/// <inheritdoc />
	public override bool setUp()
	{
		this.lightSourceId = this.GenerateLightSourceId();
		this.f = Game1.getFarm();
		if (this.f.IsRainingHere())
		{
			return true;
		}
		this.targetCrop = this.ChooseCrop();
		if (this.targetCrop == Vector2.Zero)
		{
			return true;
		}
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, this.fairyPosition, 1f, Color.Black, LightSource.LightContext.None, 0L));
		Game1.currentLocation = this.f;
		this.f.resetForPlayerEntry();
		Game1.fadeClear();
		Game1.nonWarpFade = true;
		Game1.timeOfDay = 2400;
		Game1.displayHUD = false;
		Game1.freezeControls = true;
		Game1.viewportFreeze = true;
		Game1.displayFarmer = false;
		Game1.viewport.X = Math.Max(0, Math.Min(this.f.map.DisplayWidth - Game1.viewport.Width, (int)this.targetCrop.X * 64 - Game1.viewport.Width / 2));
		Game1.viewport.Y = Math.Max(0, Math.Min(this.f.map.DisplayHeight - Game1.viewport.Height, (int)this.targetCrop.Y * 64 - Game1.viewport.Height / 2));
		this.fairyPosition = new Vector2(Game1.viewport.X + Game1.viewport.Width + 128, this.targetCrop.Y * 64f - 64f);
		Game1.changeMusicTrack("nightTime");
		return false;
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		if (this.terminate)
		{
			return true;
		}
		Game1.UpdateGameClock(time);
		this.f.UpdateWhenCurrentLocation(time);
		this.f.updateEvenIfFarmerIsntHere(time);
		Game1.UpdateOther(time);
		Utility.repositionLightSource(this.lightSourceId, this.fairyPosition + new Vector2(32f, 32f));
		if (this.animationLoopsDone < 1)
		{
			this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
		}
		if (this.fairyPosition.X > this.targetCrop.X * 64f + 32f)
		{
			if (this.timerSinceFade < 2000)
			{
				return false;
			}
			this.fairyPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.1f;
			this.fairyPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
			int num = this.fairyFrame;
			if (time.TotalGameTime.Milliseconds % 500 > 250)
			{
				this.fairyFrame = 1;
			}
			else
			{
				this.fairyFrame = 0;
			}
			if (num != this.fairyFrame && this.fairyFrame == 1)
			{
				Game1.playSound("batFlap");
				this.f.temporarySprites.Add(new TemporaryAnimatedSprite(11, this.fairyPosition + new Vector2(32f, 0f), Color.Purple));
			}
			if (this.fairyPosition.X <= this.targetCrop.X * 64f + 32f)
			{
				this.fairyFrame = 1;
			}
		}
		else if (this.animationLoopsDone < 4)
		{
			this.fairyAnimationTimer += time.ElapsedGameTime.Milliseconds;
			if (this.fairyAnimationTimer > 250)
			{
				this.fairyAnimationTimer = 0;
				if (!this.animateLeft)
				{
					this.fairyFrame++;
					if (this.fairyFrame == 3)
					{
						this.animateLeft = true;
						this.f.temporarySprites.Add(new TemporaryAnimatedSprite(10, this.fairyPosition + new Vector2(-16f, 64f), Color.LightPink));
						Game1.playSound("yoba");
						if (this.f.terrainFeatures.TryGetValue(this.targetCrop, out var terrainFeature) && terrainFeature is HoeDirt dirt)
						{
							dirt.crop.currentPhase.Value = Math.Min(dirt.crop.currentPhase.Value + 1, dirt.crop.phaseDays.Count - 1);
						}
					}
				}
				else
				{
					this.fairyFrame--;
					if (this.fairyFrame == 1)
					{
						this.animateLeft = false;
						this.animationLoopsDone++;
						if (this.animationLoopsDone >= 4)
						{
							for (int i = 0; i < 10; i++)
							{
								DelayedAction.playSoundAfterDelay("batFlap", 4000 + 500 * i);
							}
						}
					}
				}
			}
		}
		else
		{
			this.fairyAnimationTimer += time.ElapsedGameTime.Milliseconds;
			if (time.TotalGameTime.Milliseconds % 500 > 250)
			{
				this.fairyFrame = 1;
			}
			else
			{
				this.fairyFrame = 0;
			}
			if (this.fairyAnimationTimer > 2000 && this.fairyPosition.Y > -999999f)
			{
				this.fairyPosition.X += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 256.0) * 2f;
				this.fairyPosition.Y -= (float)time.ElapsedGameTime.Milliseconds * 0.2f;
			}
			if (this.fairyPosition.Y < (float)(Game1.viewport.Y - 128) || float.IsNaN(this.fairyPosition.Y))
			{
				if (!Game1.fadeToBlack && this.fairyPosition.Y != -999999f)
				{
					Game1.globalFadeToBlack(afterLastFade);
					Game1.changeMusicTrack("none");
					this.timerSinceFade = 0;
					this.fairyPosition.Y = -999999f;
				}
				this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
			}
		}
		return false;
	}

	public void afterLastFade()
	{
		this.terminate = true;
		Game1.globalFadeToClear();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.fairyPosition), new Rectangle(16 + this.fairyFrame * 16, 592, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
	}

	/// <inheritdoc />
	public override void makeChangesToLocation()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		for (int x = (int)this.targetCrop.X - 2; (float)x <= this.targetCrop.X + 2f; x++)
		{
			for (int y = (int)this.targetCrop.Y - 2; (float)y <= this.targetCrop.Y + 2f; y++)
			{
				Vector2 v = new Vector2(x, y);
				if (this.f.terrainFeatures.TryGetValue(v, out var terrainFeature) && terrainFeature is HoeDirt { crop: not null } dirt)
				{
					dirt.crop.growCompletely();
				}
			}
		}
	}

	/// <summary>Choose a random valid crop to target.</summary>
	protected Vector2 ChooseCrop()
	{
		Vector2[] validCropPositions = (from p in this.f.terrainFeatures.Pairs
			where p.Value is HoeDirt { crop: not null } hoeDirt && !hoeDirt.crop.dead.Value && !hoeDirt.crop.isWildSeedCrop() && hoeDirt.crop.currentPhase.Value < hoeDirt.crop.phaseDays.Count - 1
			orderby p.Key.X, p.Key.Y
			select p.Key).ToArray();
		return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).ChooseFrom(validCropPositions);
	}
}
