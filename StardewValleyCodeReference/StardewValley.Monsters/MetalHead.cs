using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;

namespace StardewValley.Monsters;

public class MetalHead : Monster
{
	[XmlElement("c")]
	public readonly NetColor c = new NetColor();

	public MetalHead()
	{
	}

	public MetalHead(Vector2 tileLocation, MineShaft mine)
		: this(tileLocation, mine.getMineArea())
	{
	}

	public MetalHead(string name, Vector2 tileLocation)
		: base(name, tileLocation)
	{
		this.Sprite.SpriteHeight = 16;
		this.Sprite.UpdateSourceRect();
		this.c.Value = Color.White;
		base.IsWalkingTowardPlayer = true;
	}

	public MetalHead(Vector2 tileLocation, int mineArea)
		: base("Metal Head", tileLocation)
	{
		this.Sprite.SpriteHeight = 16;
		this.Sprite.UpdateSourceRect();
		this.c.Value = Color.White;
		base.IsWalkingTowardPlayer = true;
		switch (mineArea)
		{
		case 0:
			this.c.Value = Color.White;
			break;
		case 40:
			this.c.Value = Color.Turquoise;
			base.Health *= 2;
			break;
		case 80:
			this.c.Value = Color.White;
			base.Health *= 3;
			break;
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.c, "c");
		base.position.Field.AxisAlignedMovement = true;
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "clank");
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position, Color.DarkGray, 10, flipped: false, 70f));
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position + new Vector2(-32f, 0f), Color.DarkGray, 10, flipped: false, 70f)
		{
			delayBeforeAnimationStart = 300
		});
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position + new Vector2(32f, 0f), Color.DarkGray, 10, flipped: false, 70f)
		{
			delayBeforeAnimationStart = 600
		});
		base.currentLocation.localSound("monsterdead");
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.MediumPurple, 10)
		{
			holdLastFrame = true,
			alphaFade = 0.01f,
			interval = 70f
		}, base.currentLocation);
		base.localDeathAnimation();
	}

	public override void draw(SpriteBatch b)
	{
		if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 42f + base.yOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3.5f + base.scale.Value + base.yOffset / 30f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 48 + base.yJumpOffset), this.Sprite.SourceRect, this.c.Value, base.rotation, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		}
	}

	public override void shedChunks(int number, float scale)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, this.Sprite.getHeight() * 4, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, scale * 4f);
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extraItems = new List<Item>();
		if ((Game1.stats.getMonstersKilled(base.name.Value) + (int)Game1.uniqueIDForThisGame) % 100 == 0)
		{
			extraItems.Add(ItemRegistry.Create("(H)51"));
		}
		return extraItems;
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		if (this.isMoving())
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
