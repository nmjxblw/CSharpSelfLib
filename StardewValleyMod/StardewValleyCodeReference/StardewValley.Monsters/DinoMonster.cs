using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

[XmlInclude(typeof(BreathProjectile))]
public class DinoMonster : Monster
{
	public enum AttackState
	{
		None,
		Fireball,
		Charge
	}

	/// <summary>Lightweight version of projectile for pooling.</summary>
	public class BreathProjectile : INetObject<NetFields>
	{
		public readonly NetBool active = new NetBool();

		public readonly NetVector2 position = new NetVector2();

		public readonly NetVector2 startPosition = new NetVector2();

		public readonly NetVector2 velocity = new NetVector2();

		public float rotation;

		public float alpha;

		public NetFields NetFields { get; } = new NetFields("BreathProjectile");

		public BreathProjectile()
		{
			this.NetFields.SetOwner(this).AddField(this.active, "active").AddField(this.position, "position")
				.AddField(this.startPosition, "startPosition")
				.AddField(this.velocity, "velocity");
			this.active.InterpolationEnabled = (this.active.InterpolationWait = false);
			this.position.InterpolationEnabled = (this.position.InterpolationWait = false);
			this.startPosition.InterpolationEnabled = (this.startPosition.InterpolationWait = false);
			this.velocity.InterpolationEnabled = (this.velocity.InterpolationWait = false);
		}

		public Rectangle GetBoundingBox()
		{
			Vector2 pos = this.position.Value;
			int damageSize = 29;
			float currentScale = 1f;
			damageSize = (int)((float)damageSize * currentScale);
			return new Rectangle((int)pos.X + 32 - damageSize / 2, (int)pos.Y + 32 - damageSize / 2, damageSize, damageSize);
		}

		public Rectangle GetSourceRect()
		{
			return Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 10, 16, 16);
		}

		public void ExplosionAnimation(GameLocation location)
		{
			Rectangle sourceRect = this.GetSourceRect();
			sourceRect.X += 4;
			sourceRect.Y += 4;
			sourceRect.Width = 8;
			sourceRect.Height = 8;
			Game1.createRadialDebris_MoreNatural(location, "TileSheets\\Projectiles", sourceRect, 1, (int)this.position.X + 32, (int)this.position.Y + 32, 6, (int)(this.position.Y / 64f) + 1);
		}

		public void Update(GameTime time, GameLocation location, DinoMonster parent)
		{
			if (!this.active.Value)
			{
				return;
			}
			this.position.Value += this.velocity.Value;
			if (!Game1.IsMasterGame)
			{
				this.position.MarkClean();
				this.position.ResetNewestReceivedChangeVersion();
			}
			float dist = Vector2.Distance(this.position.Value, this.startPosition.Value);
			if (dist > 128f)
			{
				this.alpha = (256f - dist) / 128f;
			}
			else
			{
				this.alpha = 1f;
			}
			if (dist > 256f)
			{
				this.active.Value = false;
				return;
			}
			Rectangle boundingBox = this.GetBoundingBox();
			if (Game1.player.currentLocation == location && Game1.player.CanBeDamaged() && boundingBox.Intersects(Game1.player.GetBoundingBox()))
			{
				Game1.player.takeDamage(25, overrideParry: false, null);
				this.ExplosionAnimation(location);
				this.active.Value = false;
				return;
			}
			foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(boundingBox))
			{
				if (location.terrainFeatures.TryGetValue(tile, out var feature) && !feature.isPassable())
				{
					this.ExplosionAnimation(location);
					this.active.Value = false;
					return;
				}
			}
			if (!location.isTileOnMap(this.position.Value / 64f) || location.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: false, 0, glider: true, parent, pathfinding: false, projectile: true))
			{
				this.ExplosionAnimation(location);
				this.active.Value = false;
			}
		}

		public void Draw(SpriteBatch b)
		{
			if (this.active.Value)
			{
				float currentScale = 4f;
				Texture2D texture = Projectile.projectileSheet;
				Rectangle sourceRect = this.GetSourceRect();
				Vector2 pixelPosition = this.position.Value;
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), sourceRect, Color.White * this.alpha, this.rotation, new Vector2(8f, 8f), currentScale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f);
			}
		}
	}

	public int timeUntilNextAttack;

	public readonly NetBool firing = new NetBool(value: false);

	public NetInt attackState = new NetInt();

	public int nextFireTime;

	public int totalFireTime;

	public int nextChangeDirectionTime;

	public int nextWanderTime;

	public bool wanderState;

	public readonly NetObjectArray<BreathProjectile> projectiles = new NetObjectArray<BreathProjectile>(15);

	public int lastProjectileSlot;

	public DinoMonster()
	{
	}

	public DinoMonster(Vector2 position)
		: base("Pepper Rex", position)
	{
		this.Sprite.SpriteWidth = 32;
		this.Sprite.SpriteHeight = 32;
		this.Sprite.UpdateSourceRect();
		this.timeUntilNextAttack = 2000;
		this.nextChangeDirectionTime = Game1.random.Next(1000, 3000);
		this.nextWanderTime = Game1.random.Next(1000, 2000);
		for (int i = 0; i < this.projectiles.Count; i++)
		{
			this.projectiles[i] = new BreathProjectile();
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.attackState, "attackState").AddField(this.firing, "firing").AddField(this.projectiles, "projectiles");
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		this.Sprite.SpriteWidth = 32;
		this.Sprite.SpriteHeight = 32;
		this.Sprite.UpdateSourceRect();
	}

	public override void draw(SpriteBatch b)
	{
		if (base.Health > 0 && !base.IsInvisible && Utility.isOnScreen(base.Position, 128))
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + base.yJumpOffset), this.Sprite.SourceRect, Color.White, base.rotation, new Vector2(16f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			if (base.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + base.yJumpOffset), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, base.scale.Value), base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.001f)));
			}
		}
		foreach (BreathProjectile projectile in this.projectiles)
		{
			if (Utility.isOnScreen(projectile.position.Value, 64))
			{
				projectile.Draw(b);
			}
		}
	}

	public override Rectangle GetBoundingBox()
	{
		if (base.Health <= 0)
		{
			return new Rectangle(-100, -100, 0, 0);
		}
		Vector2 position = base.Position;
		return new Rectangle((int)position.X + 8, (int)position.Y, this.Sprite.SpriteWidth * 4 * 3 / 4, 64);
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extra_items = new List<Item>();
		if (Game1.random.NextDouble() < 0.10000000149011612)
		{
			extra_items.Add(ItemRegistry.Create("(O)107"));
		}
		else
		{
			Item[] non_egg_items = new Item[3]
			{
				ItemRegistry.Create("(O)580"),
				ItemRegistry.Create("(O)583"),
				ItemRegistry.Create("(O)584")
			};
			extra_items.Add(Game1.random.ChooseFrom(non_egg_items));
		}
		return extra_items;
	}

	public override bool ShouldMonsterBeRemoved()
	{
		foreach (BreathProjectile projectile in this.projectiles)
		{
			if (projectile.active.Value)
			{
				return false;
			}
		}
		return base.ShouldMonsterBeRemoved();
	}

	protected override void sharedDeathAnimation()
	{
		base.currentLocation.playSound("skeletonDie");
		base.currentLocation.playSound("grunt");
		Rectangle bounds = this.GetBoundingBox();
		for (int i = 0; i < 16; i++)
		{
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp(bounds.Left, bounds.Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp(bounds.Bottom, bounds.Top, (float)Game1.random.NextDouble()), 1, base.TilePoint.Y, Color.White, 4f);
		}
	}

	protected override void localDeathAnimation()
	{
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10)
		{
			holdLastFrame = true,
			alphaFade = 0.01f,
			interval = 70f
		}, base.currentLocation, 8, 96);
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (base.Health > 0)
		{
			base.update(time, location);
		}
		foreach (BreathProjectile projectile in this.projectiles)
		{
			projectile.Update(time, location, this);
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (this.attackState.Value == 1)
		{
			base.IsWalkingTowardPlayer = false;
			this.Halt();
		}
		else if (this.withinPlayerThreshold())
		{
			base.IsWalkingTowardPlayer = true;
		}
		else
		{
			base.IsWalkingTowardPlayer = false;
			this.nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
			this.nextWanderTime -= time.ElapsedGameTime.Milliseconds;
			if (this.nextChangeDirectionTime < 0)
			{
				this.nextChangeDirectionTime = Game1.random.Next(500, 1000);
				base.facingDirection.Value = (base.facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
			}
			if (this.nextWanderTime < 0)
			{
				if (this.wanderState)
				{
					this.nextWanderTime = Game1.random.Next(1000, 2000);
				}
				else
				{
					this.nextWanderTime = Game1.random.Next(1000, 3000);
				}
				this.wanderState = !this.wanderState;
			}
			if (this.wanderState)
			{
				base.moveLeft = (base.moveUp = (base.moveRight = (base.moveDown = false)));
				base.tryToMoveInDirection(base.facingDirection.Value, isFarmer: false, base.DamageToFarmer, base.isGlider.Value);
			}
		}
		this.timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
		if (this.attackState.Value == 0 && this.withinPlayerThreshold(2))
		{
			this.firing.Set(newValue: false);
			if (this.timeUntilNextAttack < 0)
			{
				this.timeUntilNextAttack = 0;
				this.attackState.Set(1);
				this.nextFireTime = 500;
				this.totalFireTime = 3000;
				base.currentLocation.playSound("croak");
			}
		}
		else
		{
			if (this.totalFireTime <= 0)
			{
				return;
			}
			if (!this.firing.Value)
			{
				Farmer player = base.Player;
				if (player != null)
				{
					base.faceGeneralDirection(player.Position);
				}
			}
			this.totalFireTime -= time.ElapsedGameTime.Milliseconds;
			if (this.nextFireTime > 0)
			{
				this.nextFireTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextFireTime <= 0)
				{
					if (!this.firing.Value)
					{
						this.firing.Set(newValue: true);
						base.currentLocation.playSound("furnace");
					}
					float fire_angle = 0f;
					Point standingPixel = base.StandingPixel;
					Vector2 shot_origin = new Vector2((float)standingPixel.X - 32f, (float)standingPixel.Y - 32f);
					switch (base.facingDirection.Value)
					{
					case 0:
						base.yVelocity = -1f;
						shot_origin.Y -= 64f;
						fire_angle = 90f;
						break;
					case 1:
						base.xVelocity = -1f;
						shot_origin.X += 64f;
						fire_angle = 0f;
						break;
					case 3:
						base.xVelocity = 1f;
						shot_origin.X -= 64f;
						fire_angle = 180f;
						break;
					case 2:
						base.yVelocity = 1f;
						fire_angle = 270f;
						break;
					}
					fire_angle += (float)Math.Sin((double)((float)this.totalFireTime / 1000f * 180f) * Math.PI / 180.0) * 25f;
					Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * Math.PI / 180.0), 0f - (float)Math.Sin((double)fire_angle * Math.PI / 180.0));
					shot_velocity *= 10f;
					BreathProjectile projectile = this.projectiles[this.lastProjectileSlot];
					projectile.active.Value = true;
					NetVector2 netVector = projectile.position;
					Vector2 value = (projectile.startPosition.Value = shot_origin);
					netVector.Value = value;
					projectile.velocity.Value = shot_velocity;
					this.lastProjectileSlot = (this.lastProjectileSlot + 1) % this.projectiles.Count;
					this.nextFireTime = 70;
				}
			}
			if (this.totalFireTime <= 0)
			{
				this.totalFireTime = 0;
				this.nextFireTime = 0;
				this.attackState.Set(0);
				this.timeUntilNextAttack = Game1.random.Next(1000, 2000);
			}
		}
	}

	protected override void updateAnimation(GameTime time)
	{
		int direction_offset = 0;
		switch (this.FacingDirection)
		{
		case 2:
			direction_offset = 0;
			break;
		case 1:
			direction_offset = 4;
			break;
		case 0:
			direction_offset = 8;
			break;
		case 3:
			direction_offset = 12;
			break;
		}
		if (this.attackState.Value == 1)
		{
			if (this.firing.Value)
			{
				this.Sprite.CurrentFrame = 16 + direction_offset;
			}
			else
			{
				this.Sprite.CurrentFrame = 17 + direction_offset;
			}
			return;
		}
		if (this.isMoving() || this.wanderState)
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.Sprite.AnimateUp(time);
				break;
			case 3:
				this.Sprite.AnimateLeft(time);
				break;
			case 1:
				this.Sprite.AnimateRight(time);
				break;
			case 2:
				this.Sprite.AnimateDown(time);
				break;
			}
			return;
		}
		switch (this.FacingDirection)
		{
		case 0:
			this.Sprite.AnimateUp(time);
			break;
		case 3:
			this.Sprite.AnimateLeft(time);
			break;
		case 1:
			this.Sprite.AnimateRight(time);
			break;
		case 2:
			this.Sprite.AnimateDown(time);
			break;
		}
		this.Sprite.StopAnimation();
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		int direction_offset = 0;
		switch (this.FacingDirection)
		{
		case 2:
			direction_offset = 0;
			break;
		case 1:
			direction_offset = 4;
			break;
		case 0:
			direction_offset = 8;
			break;
		case 3:
			direction_offset = 12;
			break;
		}
		if (this.attackState.Value == 1)
		{
			if (this.firing.Value)
			{
				this.Sprite.CurrentFrame = 16 + direction_offset;
			}
			else
			{
				this.Sprite.CurrentFrame = 17 + direction_offset;
			}
		}
		else if (this.isMoving())
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.Sprite.AnimateUp(time);
				break;
			case 3:
				this.Sprite.AnimateLeft(time);
				break;
			case 1:
				this.Sprite.AnimateRight(time);
				break;
			case 2:
				this.Sprite.AnimateDown(time);
				break;
			}
		}
		else
		{
			this.Sprite.StopAnimation();
		}
	}
}
