using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Network;

namespace StardewValley.Monsters;

public class HotHead : MetalHead
{
	[XmlIgnore]
	public NetFarmerRef lastAttacker = new NetFarmerRef();

	[XmlIgnore]
	public NetFloat timeUntilExplode = new NetFloat(-1f);

	[XmlIgnore]
	public NetBool angry = new NetBool();

	public HotHead()
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.lastAttacker.NetFields, "lastAttacker.NetFields").AddField(this.angry, "angry").AddField(this.timeUntilExplode, "timeUntilExplode");
	}

	public HotHead(Vector2 position)
		: base("Hot Head", position)
	{
		base.Slipperiness *= 2;
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		this.lastAttacker.Value = who;
		int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		if (this.timeUntilExplode.Value == -1f && base.Health < 25)
		{
			base.currentLocation.netAudio.StartPlaying("fuse");
			this.timeUntilExplode.Value = 2.4f;
			base.Speed = 5;
			this.angry.Value = true;
		}
		return result;
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (Game1.IsMasterGame && this.timeUntilExplode.Value > 0f)
		{
			this.timeUntilExplode.Value -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.timeUntilExplode.Value <= 0f)
			{
				base.currentLocation.netAudio.StopPlaying("fuse");
				this.timeUntilExplode.Value = 0f;
				this.DropBomb();
				base.Health = -9999;
				return;
			}
		}
		base.behaviorAtGameTick(time);
	}

	public virtual void DropBomb()
	{
		base.currentLocation.netAudio.StopPlaying("fuse");
		if (this.lastAttacker.Value != null)
		{
			Farmer who = this.lastAttacker.Value;
			int idNum = Game1.random.Next();
			base.currentLocation.playSound("thudStep");
			Vector2 placementTile = base.Tile;
			float y = base.Position.Y;
			float bomb_life = 2.4f;
			if (this.timeUntilExplode.Value >= 0f)
			{
				bomb_life = this.timeUntilExplode.Value;
				base.currentLocation.netAudio.StartPlaying("fuse");
			}
			int loops = Math.Max(1, (int)(bomb_life * 1000f / 100f));
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("Characters\\Monsters\\Hot Head", new Rectangle(16, 64, 16, 16), 25f, 3, loops, placementTile * 64f, flicker: false, Game1.random.NextBool())
			{
				shakeIntensity = 0.5f,
				shakeIntensityChange = 0.002f,
				extraInfoForEndBehavior = idNum,
				endFunction = base.currentLocation.removeTemporarySpritesWithID,
				bombRadius = 2,
				bombDamage = base.DamageToFarmer,
				Parent = base.currentLocation,
				scale = 4f,
				owner = who
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (y + 7f) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
			{
				id = idNum
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (y + 7f) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
			{
				delayBeforeAnimationStart = 100,
				id = idNum
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (y + 7f) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
			{
				delayBeforeAnimationStart = 200,
				id = idNum
			});
		}
	}

	protected override void sharedDeathAnimation()
	{
		base.sharedDeathAnimation();
		this.DropBomb();
	}

	public override void draw(SpriteBatch b)
	{
		if (this.angry.Value)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				Rectangle source_rect = this.Sprite.SourceRect;
				source_rect.Y += 80;
				int standingY = base.StandingPixel.Y;
				b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 42f + base.yOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3.5f + base.scale.Value + base.yOffset / 30f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 48 + base.yJumpOffset), source_rect, base.c.Value, base.rotation, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			}
		}
		else
		{
			base.draw(b);
		}
	}
}
