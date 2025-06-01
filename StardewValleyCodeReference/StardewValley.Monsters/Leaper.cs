using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;
using xTile.Dimensions;

namespace StardewValley.Monsters;

public class Leaper : Monster
{
	public NetFloat leapDuration = new NetFloat(0.75f);

	public NetFloat leapProgress = new NetFloat(0f);

	public NetBool leaping = new NetBool(value: false);

	public NetVector2 leapStartPosition = new NetVector2();

	public NetVector2 leapEndPosition = new NetVector2();

	public float nextLeap;

	public Leaper()
	{
	}

	public Leaper(Vector2 position)
		: base("Spider", position)
	{
		base.forceOneTileWide.Value = true;
		base.IsWalkingTowardPlayer = false;
		this.nextLeap = Utility.RandomFloat(1f, 1.5f);
		base.isHardModeMonster.Value = true;
		this.reloadSprite();
	}

	public override int GetBaseDifficultyLevel()
	{
		return 1;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		this.Sprite.SpriteWidth = 32;
		this.Sprite.SpriteHeight = 32;
		this.Sprite.UpdateSourceRect();
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.leapDuration, "leapDuration").AddField(this.leapProgress, "leapProgress").AddField(this.leapStartPosition, "leapStartPosition")
			.AddField(this.leapEndPosition, "leapEndPosition")
			.AddField(this.leaping, "leaping");
		this.leapProgress.Interpolated(interpolate: true, wait: true);
		this.leaping.Interpolated(interpolate: true, wait: true);
		this.leaping.fieldChangeVisibleEvent += OnLeapingChanged;
	}

	public virtual void OnLeapingChanged(NetBool field, bool old_value, bool new_value)
	{
	}

	public override bool isInvincible()
	{
		if (this.leaping.Value)
		{
			return true;
		}
		return base.isInvincible();
	}

	public override void updateMovement(GameLocation location, GameTime time)
	{
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.localSound("monsterdead");
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.DarkRed, 10)
		{
			holdLastFrame = true,
			alphaFade = 0.01f,
			interval = 70f
		}, base.currentLocation);
	}

	protected override void sharedDeathAnimation()
	{
	}

	public override void defaultMovementBehavior(GameTime time)
	{
	}

	public override void noMovementProgressNearPlayerBehavior()
	{
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.farmerPassesThrough = true;
		base.update(time, location);
		if (this.leaping.Value)
		{
			base.yJumpGravity = 0f;
			float progress = this.leapProgress.Value;
			if (!Game1.IsMasterGame)
			{
				float total_length = (this.leapStartPosition.Value - this.leapEndPosition.Value).Length();
				progress = ((total_length != 0f) ? ((this.leapStartPosition.Value - base.Position).Length() / total_length) : 0f);
				if (progress < 0f)
				{
					progress = 0f;
				}
				if (progress > 1f)
				{
					progress = 1f;
				}
			}
			base.yJumpOffset = (int)(Math.Sin((double)progress * Math.PI) * -64.0 * 3.0);
		}
		else
		{
			base.yJumpOffset = 0;
		}
	}

	protected override void updateAnimation(GameTime time)
	{
		if (this.leaping.Value)
		{
			this.Sprite.CurrentFrame = 2;
		}
		else
		{
			this.Sprite.Animate(time, 0, 2, 500f);
		}
		this.Sprite.UpdateSourceRect();
	}

	public virtual bool IsValidLandingTile(Vector2 tile, bool check_other_characters = false)
	{
		if (base.currentLocation is MineShaft mine && !mine.isTileOnClearAndSolidGround(tile))
		{
			return false;
		}
		if (base.currentLocation.IsTileOccupiedBy(tile, ~(CollisionMask.Characters | CollisionMask.Farmers)) || !base.currentLocation.isTileOnMap(tile) || !base.currentLocation.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
		{
			return false;
		}
		Microsoft.Xna.Framework.Rectangle my_bounding_box = this.GetBoundingBox();
		if (check_other_characters && base.currentLocation != null)
		{
			foreach (NPC character in base.currentLocation.characters)
			{
				if (character != this && character.GetBoundingBox().Intersects(my_bounding_box))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		if (this.leaping.Value)
		{
			this.leapProgress.Value += (float)time.ElapsedGameTime.TotalSeconds / this.leapDuration.Value;
			if (this.leapProgress.Value >= 1f)
			{
				this.leapProgress.Value = 1f;
			}
			base.Position = new Vector2(Utility.Lerp(this.leapStartPosition.X, this.leapEndPosition.X, this.leapProgress.Value), Utility.Lerp(this.leapStartPosition.Y, this.leapEndPosition.Y, this.leapProgress.Value));
			if (this.leapProgress.Value == 1f)
			{
				this.leaping.Value = false;
				this.leapProgress.Value = 0f;
				if (!this.IsValidLandingTile(base.Tile, check_other_characters: true))
				{
					this.nextLeap = 0.1f;
				}
			}
			return;
		}
		if (this.nextLeap > 0f)
		{
			this.nextLeap -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (!(this.nextLeap <= 0f))
		{
			return;
		}
		Vector2? found_tile = null;
		Vector2 current_tile = base.Tile;
		current_tile.X = (int)current_tile.X;
		current_tile.X = (int)current_tile.X;
		if (this.withinPlayerThreshold(5) && base.Player != null)
		{
			Vector2 target_tile = base.Tile;
			if (Game1.random.NextDouble() < 0.6000000238418579)
			{
				this.nextLeap = Utility.RandomFloat(1.25f, 1.5f);
				target_tile = base.Player.Tile;
				target_tile.X = (int)Math.Round(target_tile.X);
				target_tile.Y = (int)Math.Round(target_tile.Y);
				target_tile.X += Game1.random.Next(-1, 2);
				target_tile.Y += Game1.random.Next(-1, 2);
			}
			else
			{
				this.nextLeap = Utility.RandomFloat(0.1f, 0.2f);
				target_tile.X += Game1.random.Next(-1, 2);
				target_tile.Y += Game1.random.Next(-1, 2);
			}
			if (this.IsValidLandingTile(target_tile))
			{
				found_tile = target_tile;
			}
		}
		if (!found_tile.HasValue)
		{
			for (int i = 0; i < 8; i++)
			{
				Vector2 offset = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5));
				if (!(offset == Vector2.Zero))
				{
					Vector2 tile = current_tile + offset;
					if (this.IsValidLandingTile(tile))
					{
						this.nextLeap = Utility.RandomFloat(0.6f, 1.5f);
						found_tile = tile;
						break;
					}
				}
			}
		}
		if (found_tile.HasValue)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				base.currentLocation.playSound("batFlap");
			}
			this.leapProgress.Value = 0f;
			this.leaping.Value = true;
			this.leapStartPosition.Value = base.Position;
			this.leapEndPosition.Value = found_tile.Value * 64f;
		}
		else
		{
			this.nextLeap = Utility.RandomFloat(0.25f, 0.5f);
		}
	}

	public override void shedChunks(int number, float scale)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, 64, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
	}
}
