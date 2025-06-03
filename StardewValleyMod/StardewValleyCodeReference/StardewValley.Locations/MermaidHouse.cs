using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class MermaidHouse : GameLocation
{
	private Texture2D mermaidSprites;

	private float showTimer;

	private float curtainMovement;

	private float curtainOpenPercent;

	private float blackBGAlpha;

	private float bigMermaidAlpha;

	private float oldStopWatchTime;

	private float finalLeftMermaidAlpha;

	private float finalRightMermaidAlpha;

	private float finalBigMermaidAlpha;

	private float fairyTimer;

	private int[] mermaidFrames;

	private Stopwatch stopWatch;

	private List<Vector2> bubbles;

	private TemporaryAnimatedSpriteList sparkles;

	private TemporaryAnimatedSpriteList alwaysFrontTempSprites;

	private List<int> lastFiveClamTones;

	private Farmer pearlRecipient;

	public MermaidHouse()
	{
	}

	public MermaidHouse(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		this.mermaidSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
		Game1.ambientLight = Color.White;
		Game1.changeMusicTrack("none");
		this.finalLeftMermaidAlpha = 0f;
		this.finalRightMermaidAlpha = 0f;
		this.finalBigMermaidAlpha = 0f;
		this.blackBGAlpha = 0f;
		this.bigMermaidAlpha = 0f;
		this.oldStopWatchTime = 0f;
		this.showTimer = 0f;
		this.curtainMovement = 0f;
		this.curtainOpenPercent = 0f;
		this.fairyTimer = 0f;
		this.stopWatch = new Stopwatch();
		this.bubbles = new List<Vector2>();
		this.sparkles = new TemporaryAnimatedSpriteList();
		this.alwaysFrontTempSprites = new TemporaryAnimatedSpriteList();
		this.lastFiveClamTones = new List<int>();
		this.pearlRecipient = null;
		this.mermaidFrames = new int[93]
		{
			1, 0, 2, 0, 1, 0, 2, 0, 3, 3,
			3, 4, 3, 3, 3, 4, 3, 3, 3, 4,
			3, 3, 3, 4, 3, 3, 3, 4, 3, 3,
			4, 4, 3, 3, 3, 3, 0, 0, 0, 0,
			3, 3, 3, 4, 3, 3, 3, 4, 3, 3,
			3, 4, 3, 3, 3, 4, 3, 3, 3, 4,
			3, 3, 4, 4, 3, 3, 3, 3, 0, 0,
			0, 0, 3, 3, 3, 3, 4, 4, 4, 4,
			3, 3, 3, 3, 0, 0, 5, 6, 5, 6,
			7, 8, 8
		};
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (base.getTileIndexAt(tileLocation, "Buildings", "mermaid_tiles"))
		{
		case 56:
			this.playClamTone(0, who);
			return true;
		case 57:
			this.playClamTone(1, who);
			return true;
		case 58:
			this.playClamTone(2, who);
			return true;
		case 59:
			this.playClamTone(3, who);
			return true;
		case 60:
			this.playClamTone(4, who);
			return true;
		default:
			return base.checkAction(tileLocation, viewport, who);
		}
	}

	public void playClamTone(int which)
	{
		this.playClamTone(which, null);
	}

	public void playClamTone(int which, Farmer who)
	{
		if (!(this.oldStopWatchTime < 68000f))
		{
			int pitch = 1200;
			switch (which)
			{
			case 0:
				pitch = 300;
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.HotPink,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(35f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 1:
				pitch = 600;
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Orange,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(51f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 2:
				pitch = 800;
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Yellow,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(67f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 3:
				pitch = 1000;
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Cyan,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(83f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			case 4:
				pitch = 1200;
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					color = Color.Lime,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(125, 126, 11, 12),
					scale = 4f,
					position = new Vector2(99f, 98f) * 4f,
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.03f,
					layerDepth = 0.0001f
				});
				break;
			}
			Game1.playSound("clam_tone", pitch);
			this.lastFiveClamTones.Add(which);
			if (this.lastFiveClamTones.Count > 5)
			{
				this.lastFiveClamTones.RemoveAt(0);
			}
			if (this.lastFiveClamTones.Count == 5 && this.lastFiveClamTones[0] == 0 && this.lastFiveClamTones[1] == 4 && this.lastFiveClamTones[2] == 3 && this.lastFiveClamTones[3] == 1 && this.lastFiveClamTones[4] == 2 && who != null && !who.mailReceived.Contains("gotPearl"))
			{
				who.freezePause = 4500;
				this.fairyTimer = 3500f;
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 885,
					texture = this.mermaidSprites,
					endFunction = playClamTone,
					extraInfoForEndBehavior = 0
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 1270,
					texture = this.mermaidSprites,
					endFunction = playClamTone,
					extraInfoForEndBehavior = 4
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 1655,
					texture = this.mermaidSprites,
					endFunction = playClamTone,
					extraInfoForEndBehavior = 3
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 2040,
					texture = this.mermaidSprites,
					endFunction = playClamTone,
					extraInfoForEndBehavior = 1
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					interval = 1f,
					delayBeforeAnimationStart = 2425,
					texture = this.mermaidSprites,
					endFunction = playClamTone,
					extraInfoForEndBehavior = 2
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 885,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(28f, 49f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 121
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 1270,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(108f, 49f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 117
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 1655,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(88f, 39f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 113
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 2040,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(48f, 39f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 19
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					delayBeforeAnimationStart = 2425,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(2, 127, 19, 18),
					sourceRectStartingPos = new Vector2(2f, 127f),
					scale = 4f,
					position = new Vector2(68f, 29f) * 4f,
					interval = 96f,
					animationLength = 4,
					totalNumberOfLoops = 15
				});
				this.pearlRecipient = who;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		foreach (TemporaryAnimatedSprite sparkle in this.sparkles)
		{
			sparkle.draw(b, localPosition: true);
		}
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(58f, 54f) * 4f), new Microsoft.Xna.Framework.Rectangle(this.mermaidFrames[Math.Min((int)((float)this.stopWatch.ElapsedMilliseconds / 769.2308f), this.mermaidFrames.Length - 1)] * 28, 80, 28, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(27f, 29f) * 4f + new Vector2((float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f) * 4f * 4f, (float)Math.Cos((float)this.stopWatch.ElapsedMilliseconds / 1000f) * 4f * 4f)), new Microsoft.Xna.Framework.Rectangle(2 + (int)(this.showTimer % 400f / 100f) * 19, 127, 19, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(97f, 29f) * 4f + new Vector2((float)Math.Cos((float)this.stopWatch.ElapsedMilliseconds / 1000f + 0.1f) * 4f * 4f, (float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f + 0.1f) * 4f * 4f)), new Microsoft.Xna.Framework.Rectangle(2 + (int)(this.showTimer % 400f / 100f) * 19, 127, 19, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(16f, 16f) * 4f), new Microsoft.Xna.Framework.Rectangle((int)(144f + 57f * this.curtainOpenPercent), 119, (int)(57f * (1f - this.curtainOpenPercent)), 81), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(73f + 57f * this.curtainOpenPercent, 16f) * 4f), new Microsoft.Xna.Framework.Rectangle(200, 119, (int)(57f * (1f - this.curtainOpenPercent)), 81), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		base.drawAboveAlwaysFrontLayer(b);
		b.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * this.blackBGAlpha);
		int spacing = Game1.graphics.GraphicsDevice.Viewport.Bounds.Height / 4;
		for (int i = -448; i < Game1.graphics.GraphicsDevice.Viewport.Width + 448; i += 448)
		{
			b.Draw(this.mermaidSprites, new Vector2(i - (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f), spacing - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.Lime * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) - (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)spacing - (float)spacing / 4f + (float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f) * 64f), new Microsoft.Xna.Framework.Rectangle(177, 0, 16, 16), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, new Vector2(i + (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f), spacing * 2 - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.Cyan * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) + (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 2) - (float)spacing / 4f + (float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f + 4f) * 64f), new Microsoft.Xna.Framework.Rectangle(161, 0, 16, 16), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
			b.Draw(this.mermaidSprites, new Vector2(i - (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f), spacing * 3 - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.Orange * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) - (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 3) - (float)spacing / 4f + (float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f + 3f) * 64f), new Microsoft.Xna.Framework.Rectangle(129, 0, 16, 16), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, new Vector2(i + (int)((float)this.stopWatch.ElapsedMilliseconds / 6f % 448f), spacing * 4 - spacing * 3 / 4), new Microsoft.Xna.Framework.Rectangle(144, 32, 112, 48), Color.HotPink * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.mermaidSprites, new Vector2((float)(i + 112) + (float)this.stopWatch.ElapsedMilliseconds / 6f % 448f, (float)(spacing * 4) - (float)spacing / 4f + (float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f + 2f) * 64f), new Microsoft.Xna.Framework.Rectangle(145, 0, 16, 16), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
		}
		b.Draw(this.mermaidSprites, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X - 112) + (float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f) * 64f * 2f, (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y - 140) + (float)Math.Cos((double)((float)this.stopWatch.ElapsedMilliseconds / 1000f * 2f) + Math.PI / 2.0) * 64f), new Microsoft.Xna.Framework.Rectangle((int)(57 * (this.stopWatch.ElapsedMilliseconds % 1538 / 769)), 0, 57, 70), Color.White * this.bigMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		foreach (TemporaryAnimatedSprite alwaysFrontTempSprite in this.alwaysFrontTempSprites)
		{
			alwaysFrontTempSprite.draw(b, localPosition: true);
		}
		foreach (Vector2 v in this.bubbles)
		{
			b.Draw(this.mermaidSprites, v + new Vector2((float)Math.Sin((float)this.stopWatch.ElapsedMilliseconds / 1000f * 4f + v.X) * 4f * 6f, 0f), new Microsoft.Xna.Framework.Rectangle(132, 20, 8, 8), Color.White * this.blackBGAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		}
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-20f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-20f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Orange * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-30f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-30f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Cyan * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-40f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(-40f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Lime * this.finalLeftMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0011f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(150f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(150f, 50f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Orange * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(160f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(160f, 90f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Cyan * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(170f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(192, 0, 16, 32), Color.White * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.001f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(170f, 130f) * 4f), new Microsoft.Xna.Framework.Rectangle(208, 0, 16, 32), Color.Lime * this.finalRightMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.0011f);
		b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(43f, 180f) * 4f), new Microsoft.Xna.Framework.Rectangle((int)(57 * (this.stopWatch.ElapsedMilliseconds % 1538 / 769)), 0, 57, 70), Color.White * this.finalBigMermaidAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		ICue main_player_music = Game1.currentSong;
		if (!Game1.game1.IsMainInstance)
		{
			main_player_music = GameRunner.instance.gameInstances[0].instanceCurrentSong;
		}
		base.UpdateWhenCurrentLocation(time);
		if (this.stopWatch == null)
		{
			return;
		}
		if (!Game1.shouldTimePass())
		{
			if (this.stopWatch != null && this.stopWatch.IsRunning)
			{
				this.stopWatch.Stop();
			}
			if (main_player_music?.Name == "mermaidSong" && !main_player_music.IsPaused && main_player_music.IsPlaying)
			{
				main_player_music.Pause();
			}
		}
		else
		{
			if (this.stopWatch != null && !this.stopWatch.IsRunning && main_player_music?.Name == "mermaidSong" && main_player_music.IsPaused)
			{
				this.stopWatch.Start();
			}
			if (main_player_music?.Name == "mermaidSong" && main_player_music.IsPaused)
			{
				main_player_music.Resume();
			}
		}
		if (Game1.shouldTimePass())
		{
			float num = this.showTimer;
			this.showTimer += time.ElapsedGameTime.Milliseconds;
			if (((main_player_music?.Name == "mermaidSong" && main_player_music.IsPlaying) || (Game1.options.musicVolumeLevel <= 0f && Game1.options.ambientVolumeLevel <= 0f)) && !this.stopWatch.IsRunning)
			{
				this.stopWatch.Start();
			}
			if (this.curtainMovement != 0f)
			{
				this.curtainOpenPercent = Math.Max(0f, Math.Min(1f, this.curtainOpenPercent + this.curtainMovement * (float)time.ElapsedGameTime.Milliseconds));
			}
			if (num < 3000f && this.showTimer >= 3000f)
			{
				Game1.changeMusicTrack("mermaidSong");
			}
			Stopwatch stopwatch = this.stopWatch;
			if (stopwatch != null && stopwatch.ElapsedMilliseconds > 0 && this.stopWatch.ElapsedMilliseconds < 1000)
			{
				this.curtainMovement = 0.0004f;
			}
			this.sparkles.RemoveWhere((TemporaryAnimatedSprite sparkle) => sparkle.update(time));
			this.alwaysFrontTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			if (this.stopWatch.ElapsedMilliseconds >= 30000 && this.stopWatch.ElapsedMilliseconds < 50000 && (this.blackBGAlpha < 1f || this.bigMermaidAlpha < 1f))
			{
				this.blackBGAlpha += 0.01f;
				this.bigMermaidAlpha += 0.01f;
			}
			if (this.stopWatch.ElapsedMilliseconds > 27692 && this.stopWatch.ElapsedMilliseconds < 55385)
			{
				if (this.oldStopWatchTime % 769f > (float)(this.stopWatch.ElapsedMilliseconds % 769))
				{
					this.bubbles.Add(new Vector2(Game1.random.Next((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel) - 64), (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel));
				}
				for (int i = 0; i < this.bubbles.Count; i++)
				{
					this.bubbles[i] = new Vector2(this.bubbles[i].X, this.bubbles[i].Y - 0.1f * (float)time.ElapsedGameTime.Milliseconds);
				}
			}
			if (this.oldStopWatchTime < 36923f && this.stopWatch.ElapsedMilliseconds >= 36923)
			{
				this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 32f,
					motion = new Vector2(0f, -4f),
					sourceRectStartingPos = new Vector2(67f, 189f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
					totalNumberOfLoops = 100,
					animationLength = 3,
					pingPong = true,
					interval = 192f,
					delayBeforeAnimationStart = 0,
					initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
					position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			if (this.oldStopWatchTime < 40000f && this.stopWatch.ElapsedMilliseconds >= 40000)
			{
				this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 32f,
					motion = new Vector2(0f, -4f),
					sourceRectStartingPos = new Vector2(67f, 189f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
					totalNumberOfLoops = 100,
					animationLength = 3,
					pingPong = true,
					interval = 192f,
					delayBeforeAnimationStart = 0,
					initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width * 3f / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
					position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel * 3f / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			if (this.oldStopWatchTime < 43077f && this.stopWatch.ElapsedMilliseconds >= 43077)
			{
				this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 32f,
					motion = new Vector2(0f, -4f),
					sourceRectStartingPos = new Vector2(67f, 189f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
					totalNumberOfLoops = 100,
					animationLength = 3,
					pingPong = true,
					interval = 192f,
					delayBeforeAnimationStart = 0,
					initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
					position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			if (this.oldStopWatchTime < 46154f && this.stopWatch.ElapsedMilliseconds >= 46154)
			{
				this.alwaysFrontTempSprites.Add(new TemporaryAnimatedSprite
				{
					texture = this.mermaidSprites,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 32f,
					motion = new Vector2(0f, -4f),
					sourceRectStartingPos = new Vector2(67f, 189f),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(67, 189, 24, 53),
					totalNumberOfLoops = 100,
					animationLength = 3,
					pingPong = true,
					interval = 192f,
					delayBeforeAnimationStart = 0,
					initialPosition = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width * 3f / 4f, Game1.graphics.GraphicsDevice.Viewport.Height - 1),
					position = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel * 3f / 4f, (float)Game1.graphics.GraphicsDevice.Viewport.Height / Game1.options.zoomLevel - 1f),
					scale = 4f,
					layerDepth = 1f
				});
			}
			if (this.stopWatch.ElapsedMilliseconds >= 52308 && (this.blackBGAlpha > 0f || this.bigMermaidAlpha > 0f))
			{
				this.blackBGAlpha -= 0.01f;
				this.bigMermaidAlpha -= 0.01f;
			}
			if (this.stopWatch.ElapsedMilliseconds >= 58462 && this.stopWatch.ElapsedMilliseconds < 60000 && this.finalLeftMermaidAlpha < 1f)
			{
				this.finalLeftMermaidAlpha += 0.01f;
			}
			if (this.stopWatch.ElapsedMilliseconds >= 60000 && this.stopWatch.ElapsedMilliseconds < 62000 && this.finalRightMermaidAlpha < 1f)
			{
				this.finalRightMermaidAlpha += 0.01f;
			}
			if (this.stopWatch.ElapsedMilliseconds >= 61538 && this.stopWatch.ElapsedMilliseconds < 63538 && this.finalBigMermaidAlpha < 1f)
			{
				this.finalBigMermaidAlpha += 0.01f;
			}
			if (this.stopWatch.ElapsedMilliseconds >= 64615 && (this.finalBigMermaidAlpha < 1f || this.finalRightMermaidAlpha < 1f || this.finalLeftMermaidAlpha < 1f))
			{
				this.finalBigMermaidAlpha -= 0.01f;
				this.finalRightMermaidAlpha -= 0.01f;
				this.finalLeftMermaidAlpha -= 0.01f;
			}
			if (this.oldStopWatchTime < 64808f && this.stopWatch.ElapsedMilliseconds >= 64808)
			{
				for (int i2 = 0; i2 < 200; i2++)
				{
					this.sparkles.Add(new TemporaryAnimatedSprite
					{
						texture = this.mermaidSprites,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 146, 16, 13),
						animationLength = 9,
						interval = 100f,
						delayBeforeAnimationStart = i2 * 10,
						position = Utility.getRandomPositionOnScreenNotOnMap(),
						scale = 4f
					});
				}
				Utility.addSprinklesToLocation(this, 5, 5, 9, 5, 2000, 100, Color.White);
			}
			if (this.oldStopWatchTime < 67500f && this.stopWatch.ElapsedMilliseconds >= 67500)
			{
				this.curtainMovement = -0.0003f;
			}
			this.oldStopWatchTime = this.stopWatch.ElapsedMilliseconds;
		}
		if (!(this.fairyTimer > 0f))
		{
			return;
		}
		this.fairyTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.fairyTimer < 200f)
		{
			Farmer farmer = this.pearlRecipient;
			if (farmer != null && farmer.FacingDirection == 0)
			{
				this.pearlRecipient.faceDirection(1);
			}
		}
		if (this.fairyTimer < 100f && this.pearlRecipient != null)
		{
			this.pearlRecipient.faceDirection(2);
		}
		if (!(this.fairyTimer <= 0f) || this.pearlRecipient == null)
		{
			return;
		}
		foreach (TemporaryAnimatedSprite temporarySprite in base.temporarySprites)
		{
			temporarySprite.alphaFade = 0.01f;
		}
		this.pearlRecipient.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(O)797"));
		this.pearlRecipient.mailReceived.Add("gotPearl");
	}
}
