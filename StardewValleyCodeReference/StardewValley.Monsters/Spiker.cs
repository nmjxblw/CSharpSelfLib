using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Monsters;

public class Spiker : Monster
{
	[XmlIgnore]
	public int targetDirection;

	[XmlIgnore]
	public NetBool moving = new NetBool(value: false);

	protected bool _localMoving;

	[XmlIgnore]
	public float nextMoveCheck;

	public Spiker()
	{
	}

	public Spiker(Vector2 position, int direction)
		: base("Spiker", position)
	{
		this.Sprite.SpriteWidth = 16;
		this.Sprite.SpriteHeight = 16;
		this.Sprite.UpdateSourceRect();
		this.targetDirection = direction;
		base.speed = 14;
		base.ignoreMovementAnimations = true;
		base.onCollision = collide;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.moving, "moving");
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		if (this.moving.Value == this._localMoving)
		{
			return;
		}
		this._localMoving = this.moving.Value;
		if (this._localMoving)
		{
			if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
			{
				Game1.playSound("parry");
			}
		}
		else if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
		{
			Game1.playSound("hammer");
		}
	}

	public override void draw(SpriteBatch b)
	{
		this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position), (float)base.StandingPixel.Y / 10000f);
	}

	private void collide(GameLocation location)
	{
		Rectangle bb = this.nextPosition(this.FacingDirection);
		foreach (Farmer farmer in location.farmers)
		{
			if (farmer.GetBoundingBox().Intersects(bb))
			{
				return;
			}
		}
		if (this.moving.Value)
		{
			this.moving.Value = false;
			this.targetDirection = (this.targetDirection + 2) % 4;
			this.nextMoveCheck = 0.75f;
		}
	}

	public override void updateMovement(GameLocation location, GameTime time)
	{
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		return -1;
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (this.nextMoveCheck > 0f)
		{
			this.nextMoveCheck -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (this.nextMoveCheck <= 0f)
		{
			this.nextMoveCheck = 0.25f;
			foreach (Farmer farmer in base.currentLocation.farmers)
			{
				if ((this.targetDirection == 0 || this.targetDirection == 2) && Math.Abs(farmer.TilePoint.X - base.TilePoint.X) <= 1)
				{
					if (this.targetDirection == 0 && farmer.Position.Y < base.Position.Y)
					{
						this.moving.Value = true;
						break;
					}
					if (this.targetDirection == 2 && farmer.Position.Y > base.Position.Y)
					{
						this.moving.Value = true;
						break;
					}
				}
				if ((this.targetDirection == 3 || this.targetDirection == 1) && Math.Abs(farmer.TilePoint.Y - base.TilePoint.Y) <= 1)
				{
					if (this.targetDirection == 3 && farmer.Position.X < base.Position.X)
					{
						this.moving.Value = true;
						break;
					}
					if (this.targetDirection == 1 && farmer.Position.X > base.Position.X)
					{
						this.moving.Value = true;
						break;
					}
				}
			}
		}
		base.moveUp = false;
		base.moveDown = false;
		base.moveLeft = false;
		base.moveRight = false;
		if (this.moving.Value)
		{
			switch (this.targetDirection)
			{
			case 0:
				base.moveUp = true;
				break;
			case 2:
				base.moveDown = true;
				break;
			case 3:
				base.moveLeft = true;
				break;
			case 1:
				base.moveRight = true;
				break;
			}
			this.MovePosition(time, Game1.viewport, base.currentLocation);
		}
		this.faceDirection(2);
	}
}
