using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class Butterfly : Critter
{
	public const float maxSpeed = 3f;

	private int flapTimer;

	private int flapSpeed = 50;

	private Vector2 motion;

	private float motionMultiplier = 1f;

	private float prismaticCaptureTimer = -1f;

	private float prismaticSprinkleTimer;

	private bool summerButterfly;

	public bool stayInbounds;

	public bool isPrismatic;

	public bool isLit;

	private string lightId;

	public Butterfly(GameLocation location, Vector2 position, bool islandButterfly = false, bool forceSummerButterfly = false, int baseFrameOverride = -1, bool prismatic = false)
	{
		base.position = position * 64f;
		base.startingPosition = base.position;
		this.isPrismatic = prismatic;
		if (location.IsWinterHere())
		{
			base.baseFrame = 397;
			this.isLit = true;
		}
		else if (location.IsSpringHere() && !forceSummerButterfly)
		{
			base.baseFrame = (Game1.random.NextBool() ? (Game1.random.Next(3) * 3 + 160) : (Game1.random.Next(3) * 3 + 180));
		}
		else
		{
			base.baseFrame = (Game1.random.NextBool() ? (Game1.random.Next(3) * 4 + 128) : (Game1.random.Next(3) * 4 + 148));
			this.summerButterfly = true;
			if (Game1.random.NextDouble() < 0.05)
			{
				base.baseFrame = Game1.random.Next(2) * 4 + 169;
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				base.baseFrame = Game1.random.Next(2) * 4 + 480;
			}
		}
		if (islandButterfly)
		{
			base.baseFrame = Game1.random.Next(4) * 4 + 364;
			this.summerButterfly = true;
		}
		if (baseFrameOverride != -1)
		{
			base.baseFrame = baseFrameOverride;
			this.summerButterfly = false;
			this.isLit = false;
		}
		this.motion = new Vector2((float)(Game1.random.NextDouble() + 0.25) * 3f * (float)Game1.random.Choose(-1, 1) / 2f, (float)(Game1.random.NextDouble() + 0.5) * 3f * (float)Game1.random.Choose(-1, 1) / 2f);
		this.flapSpeed = Game1.random.Next(45, 80);
		base.sprite = new AnimatedSprite(Critter.critterTexture, base.baseFrame, 16, 16);
		base.sprite.loop = false;
		base.startingPosition = position;
		if (this.isLit)
		{
			this.lightId = this.GenerateLightSourceId(Game1.random.Next());
			Game1.currentLightSources.Add(new LightSource(this.lightId, 10, position + new Vector2(-30.72f, -93.44f), 0.66f, Color.Black * 0.75f, LightSource.LightContext.None, 0L, location.NameOrUniqueName));
		}
	}

	public void doneWithFlap(Farmer who)
	{
		this.flapTimer = 200 + Game1.random.Next(-5, 6);
	}

	public Butterfly setStayInbounds(bool stayInbounds)
	{
		this.stayInbounds = stayInbounds;
		return this;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		this.flapTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.flapTimer <= 0 && base.sprite.CurrentAnimation == null)
		{
			this.motionMultiplier = 1f;
			this.motion.X += (float)Game1.random.Next(-80, 81) / 100f;
			this.motion.Y = (float)(Game1.random.NextDouble() + 0.25) * -3f / 2f;
			if (Math.Abs(this.motion.X) > 1.5f)
			{
				this.motion.X = 3f * (float)Math.Sign(this.motion.X) / 2f;
			}
			if (Math.Abs(this.motion.Y) > 3f)
			{
				this.motion.Y = 3f * (float)Math.Sign(this.motion.Y);
			}
			if (this.stayInbounds)
			{
				if (base.position.X < 128f)
				{
					this.motion.X = 0.8f;
				}
				if (base.position.Y < 192f)
				{
					this.motion.Y /= 2f;
					this.flapTimer = 1000;
				}
				if (base.position.X > (float)(environment.map.DisplayWidth - 128))
				{
					this.motion.X = -0.8f;
				}
				if (base.position.Y > (float)(environment.map.DisplayHeight - 128))
				{
					this.motion.Y = -1f;
					this.flapTimer = 100;
				}
			}
			if (this.summerButterfly)
			{
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(base.baseFrame + 1, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame + 2, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame + 3, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame + 2, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame + 1, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame, this.flapSpeed, secondaryArm: false, flip: false, doneWithFlap)
				});
			}
			else
			{
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(base.baseFrame + 1, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame + 2, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame + 1, this.flapSpeed),
					new FarmerSprite.AnimationFrame(base.baseFrame, this.flapSpeed, secondaryArm: false, flip: false, doneWithFlap)
				});
			}
			if (this.isPrismatic && this.prismaticCaptureTimer < 0f)
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, base.position + new Vector2(-48 + Game1.random.Next(-32, 32), -96 + Game1.random.Next(-32, 32)), flicker: false, flipped: false, Math.Max(0f, (base.position.Y + 64f - 24f) / 10000f) + base.position.X / 64f * 1E-05f, 0f, Utility.GetPrismaticColor(Game1.random.Next(7), 10f), 4f, 0f, 0f, 0f)
				{
					drawAboveAlwaysFront = true
				}, environment);
			}
		}
		if (this.prismaticCaptureTimer > 0f)
		{
			this.motion = Game1.player.position.Value + new Vector2(64f, -32f) - base.position;
			this.motion *= 0.1f;
			this.prismaticCaptureTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			base.position += this.motion;
			base.position += new Vector2((float)Math.Cos(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0) * (this.prismaticCaptureTimer / 150f), (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0) * (this.prismaticCaptureTimer / 150f));
			this.prismaticSprinkleTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			if (this.prismaticSprinkleTimer <= 0f)
			{
				environment.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, base.position + new Vector2(-48f, -96f), flicker: false, flipped: false, Math.Max(0f, (base.position.Y + 64f - 24f) / 10000f) + base.position.X / 64f * 1E-05f, 0f, Utility.GetPrismaticColor(Game1.random.Next(7), 10f), 4f, 0f, 0f, 0f)
				{
					drawAboveAlwaysFront = true
				});
				this.prismaticSprinkleTimer = 80f;
			}
			if (this.prismaticCaptureTimer <= 0f)
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, base.position + new Vector2(-48f, -96f), flicker: false, flipped: false, Math.Max(0f, (base.position.Y + 64f - 24f) / 10000f) + base.position.X / 64f * 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					drawAboveAlwaysFront = true
				}, environment, 16);
				Game1.playSound("yoba");
				Game1.player.buffs.Remove("statue_of_blessings_6");
				if (Utility.CreateDaySaveRandom(Game1.player.UniqueMultiplayerID % 10000).NextDouble() < 0.05000000074505806 + Game1.player.DailyLuck)
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)74"), base.position + new Vector2(-48f, -96f), 2, environment, (int)Game1.player.position.Y);
				}
				Game1.player.Money += Math.Max(100, Math.Min(50000, (int)((float)Game1.player.totalMoneyEarned * 0.005f)));
				return true;
			}
		}
		else
		{
			base.position += this.motion * this.motionMultiplier;
			this.motion.Y += 0.005f * (float)time.ElapsedGameTime.Milliseconds;
			this.motionMultiplier -= 0.0005f * (float)time.ElapsedGameTime.Milliseconds;
			if (this.motionMultiplier <= 0f)
			{
				this.motionMultiplier = 0f;
			}
		}
		if (this.isPrismatic && this.prismaticCaptureTimer < 0f && Utility.distance(base.position.X, Game1.player.position.X, base.position.Y, Game1.player.position.Y) < 128f)
		{
			this.prismaticCaptureTimer = 2000f;
		}
		if (this.isLit)
		{
			Utility.repositionLightSource(this.lightId, base.position + new Vector2(-30.72f, -93.44f));
		}
		return base.update(time, environment);
	}

	public override void draw(SpriteBatch b)
	{
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		base.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.position + new Vector2(-64f, -128f + base.yJumpOffset + base.yOffset)), base.position.Y / 10000f, 0, 0, this.isPrismatic ? Utility.GetPrismaticColor(0, 10f) : Color.White, base.flip, 4f);
	}
}
