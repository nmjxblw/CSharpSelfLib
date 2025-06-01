using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Monsters;

public class AngryRoger : Monster
{
	public const float rotationIncrement = (float)Math.PI / 64f;

	[XmlIgnore]
	public int wasHitCounter;

	[XmlIgnore]
	public float targetRotation;

	[XmlIgnore]
	public bool turningRight;

	[XmlIgnore]
	public int identifier = Game1.random.Next(-99999, 99999);

	[XmlIgnore]
	public new int yOffset;

	[XmlIgnore]
	public int yOffsetExtra;

	public string lightSourceId;

	public AngryRoger()
	{
		this.lightSourceId = this.GenerateLightSourceId(this.identifier);
	}

	public AngryRoger(Vector2 position)
		: base("Ghost", position)
	{
		base.Slipperiness = 8;
		base.isGlider.Value = true;
		base.HideShadow = true;
		this.lightSourceId = this.GenerateLightSourceId(this.identifier);
	}

	/// <summary>
	/// constructor for non-default ghosts
	/// </summary>
	/// <param name="position"></param>
	/// <param name="name"></param>
	public AngryRoger(Vector2 position, string name)
		: base(name, position)
	{
		base.Slipperiness = 8;
		base.isGlider.Value = true;
		base.HideShadow = true;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.name.Value);
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + this.yOffset), this.Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		base.Slipperiness = 8;
		Utility.addSprinklesToLocation(base.currentLocation, base.TilePoint.X, base.TilePoint.Y, 2, 2, 101, 50, Color.LightBlue);
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			if (base.Health <= 0)
			{
				base.deathAnimation();
			}
			base.setTrajectory(xTrajectory, yTrajectory);
		}
		this.addedSpeed = -1f;
		Utility.removeLightSource(this.lightSourceId);
		return actualDamage;
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.localSound("ghost");
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 24), 100f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f, 0.01f, 0f, (float)Math.PI / 64f));
	}

	protected override void sharedDeathAnimation()
	{
	}

	protected override void updateAnimation(GameTime time)
	{
		this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f) * (Math.PI * 2.0)) * 20.0) - this.yOffsetExtra;
		if (base.currentLocation == Game1.currentLocation)
		{
			if (Game1.currentLightSources.TryGetValue(this.lightSourceId, out var light))
			{
				light.position.Value = new Vector2(base.Position.X + 32f, base.Position.Y + 64f + (float)this.yOffset);
			}
			else
			{
				Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 5, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, Color.White * 0.7f, LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName));
			}
		}
		Point monsterPixel = base.StandingPixel;
		Point standingPixel = base.Player.StandingPixel;
		float xSlope = -(standingPixel.X - monsterPixel.X);
		float ySlope = standingPixel.Y - monsterPixel.Y;
		float t = 400f;
		xSlope /= t;
		ySlope /= t;
		if (this.wasHitCounter <= 0)
		{
			this.targetRotation = (float)Math.Atan2(0f - ySlope, xSlope) - (float)Math.PI / 2f;
			if ((double)(Math.Abs(this.targetRotation) - Math.Abs(base.rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextBool())
			{
				this.turningRight = true;
			}
			else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(base.rotation)) < Math.PI / 8.0)
			{
				this.turningRight = false;
			}
			if (this.turningRight)
			{
				base.rotation -= (float)Math.Sign(this.targetRotation - base.rotation) * ((float)Math.PI / 64f);
			}
			else
			{
				base.rotation += (float)Math.Sign(this.targetRotation - base.rotation) * ((float)Math.PI / 64f);
			}
			base.rotation %= (float)Math.PI * 2f;
			this.wasHitCounter = 0;
		}
		float maxAccel = Math.Min(4f, Math.Max(1f, 5f - t / 64f / 2f));
		xSlope = (float)Math.Cos((double)base.rotation + Math.PI / 2.0);
		ySlope = 0f - (float)Math.Sin((double)base.rotation + Math.PI / 2.0);
		base.xVelocity += (0f - xSlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		base.yVelocity += (0f - ySlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		if (Math.Abs(base.xVelocity) > Math.Abs((0f - xSlope) * 5f))
		{
			base.xVelocity -= (0f - xSlope) * maxAccel / 6f;
		}
		if (Math.Abs(base.yVelocity) > Math.Abs((0f - ySlope) * 5f))
		{
			base.yVelocity -= (0f - ySlope) * maxAccel / 6f;
		}
		base.faceGeneralDirection(base.Player.getStandingPosition());
		base.resetAnimationSpeed();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		Microsoft.Xna.Framework.Rectangle monsterBounds = this.GetBoundingBox();
		Microsoft.Xna.Framework.Rectangle playerBounds = base.Player.GetBoundingBox();
		if (!monsterBounds.Intersects(playerBounds) || !base.Player.temporarilyInvincible)
		{
			return;
		}
		Layer backLayer = base.currentLocation.map.RequireLayer("Back");
		Point playerCenter = playerBounds.Center;
		int attempts = 0;
		Vector2 attemptedPosition = new Vector2(playerCenter.X / 64 + Game1.random.Next(-12, 12), playerCenter.Y / 64 + Game1.random.Next(-12, 12));
		for (; attempts < 3; attempts++)
		{
			if (!(attemptedPosition.X >= (float)backLayer.LayerWidth) && !(attemptedPosition.Y >= (float)backLayer.LayerHeight) && !(attemptedPosition.X < 0f) && !(attemptedPosition.Y < 0f) && backLayer.Tiles[(int)attemptedPosition.X, (int)attemptedPosition.Y] != null && base.currentLocation.isTilePassable(new Location((int)attemptedPosition.X, (int)attemptedPosition.Y), Game1.viewport) && !attemptedPosition.Equals(new Vector2(playerCenter.X / 64, playerCenter.Y / 64)))
			{
				break;
			}
			attemptedPosition = new Vector2(playerCenter.X / 64 + Game1.random.Next(-12, 12), playerCenter.Y / 64 + Game1.random.Next(-12, 12));
		}
		if (attempts < 3)
		{
			base.Position = new Vector2(attemptedPosition.X * 64f, attemptedPosition.Y * 64f - 32f);
			this.Halt();
		}
	}
}
