using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Network;

namespace StardewValley.Monsters;

public class Serpent : Monster
{
	public const float rotationIncrement = (float)Math.PI / 64f;

	[XmlIgnore]
	public int wasHitCounter;

	[XmlIgnore]
	public float targetRotation;

	[XmlIgnore]
	public bool turningRight;

	[XmlIgnore]
	public readonly NetFarmerRef killer = new NetFarmerRef().Delayed(interpolationWait: false);

	public List<Vector3> segments = new List<Vector3>();

	public NetInt segmentCount = new NetInt(0);

	public Serpent()
	{
	}

	public Serpent(Vector2 position)
		: base("Serpent", position)
	{
		this.InitializeAttributes();
	}

	public Serpent(Vector2 position, string name)
		: base(name, position)
	{
		this.InitializeAttributes();
		if (name == "Royal Serpent")
		{
			this.segmentCount.Value = Game1.random.Next(3, 7);
			if (Game1.random.NextDouble() < 0.1)
			{
				this.segmentCount.Value = Game1.random.Next(5, 10);
			}
			else if (Game1.random.NextDouble() < 0.01)
			{
				this.segmentCount.Value *= 3;
			}
			this.reloadSprite();
			base.MaxHealth += this.segmentCount.Value * 50;
			base.Health = base.MaxHealth;
		}
	}

	public virtual void InitializeAttributes()
	{
		base.Slipperiness = 24 + Game1.random.Next(10);
		this.Halt();
		base.IsWalkingTowardPlayer = false;
		this.Sprite.SpriteWidth = 32;
		this.Sprite.SpriteHeight = 32;
		base.Scale = 0.75f;
		base.HideShadow = true;
	}

	public bool IsRoyalSerpent()
	{
		return this.segmentCount.Value > 1;
	}

	public override bool TakesDamageFromHitbox(Rectangle area_of_effect)
	{
		if (base.TakesDamageFromHitbox(area_of_effect))
		{
			return true;
		}
		if (this.IsRoyalSerpent())
		{
			Rectangle bounds = this.GetBoundingBox();
			Vector2 offset = new Vector2((float)bounds.X - base.Position.X, (float)bounds.Y - base.Position.Y);
			foreach (Vector3 segment in this.segments)
			{
				bounds.X = (int)(segment.X + offset.X);
				bounds.Y = (int)(segment.Y + offset.Y);
				if (bounds.Intersects(area_of_effect))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool OverlapsFarmerForDamage(Farmer who)
	{
		if (base.OverlapsFarmerForDamage(who))
		{
			return true;
		}
		if (this.IsRoyalSerpent())
		{
			Rectangle monsterBounds = this.GetBoundingBox();
			Rectangle playerBounds = who.GetBoundingBox();
			Vector2 offset = new Vector2((float)monsterBounds.X - base.Position.X, (float)monsterBounds.Y - base.Position.Y);
			foreach (Vector3 segment in this.segments)
			{
				monsterBounds.X = (int)(segment.X + offset.X);
				monsterBounds.Y = (int)(segment.Y + offset.Y);
				if (monsterBounds.Intersects(playerBounds))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.killer.NetFields, "killer.NetFields").AddField(this.segmentCount, "segmentCount");
		this.segmentCount.fieldChangeVisibleEvent += delegate(NetInt field, int old_value, int new_value)
		{
			if (new_value > 0)
			{
				this.reloadSprite();
			}
		};
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		if (this.IsRoyalSerpent())
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Royal Serpent");
			base.Scale = 1f;
		}
		else
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Serpent");
			base.Scale = 0.75f;
		}
		this.Sprite.SpriteWidth = 32;
		this.Sprite.SpriteHeight = 32;
		base.HideShadow = true;
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
			this.wasHitCounter = 500;
			base.currentLocation.playSound("serpentHit");
			if (base.Health <= 0)
			{
				this.killer.Value = who;
				base.deathAnimation();
			}
		}
		this.addedSpeed = Game1.random.Next(-1, 1);
		return actualDamage;
	}

	protected override void sharedDeathAnimation()
	{
	}

	protected override void localDeathAnimation()
	{
		if (this.killer.Value == null)
		{
			return;
		}
		Rectangle bb = this.GetBoundingBox();
		bb.Inflate(-bb.Width / 2 + 1, -bb.Height / 2 + 1);
		Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(bb.Center, 4f, this.killer.Value);
		int xTrajectory = -(int)velocityTowardPlayer.X;
		int yTrajectory = -(int)velocityTowardPlayer.Y;
		if (this.IsRoyalSerpent())
		{
			base.currentLocation.localSound("serpentDie");
			for (int i = -1; i < this.segments.Count; i++)
			{
				Vector2 segment_position;
				Rectangle source_rect;
				float current_rotation;
				float color_fade;
				if (i == -1)
				{
					segment_position = base.Position;
					source_rect = new Rectangle(0, 64, 32, 32);
					current_rotation = base.rotation;
					color_fade = 0f;
				}
				else
				{
					if (this.segments.Count <= 0 || i >= this.segments.Count)
					{
						break;
					}
					color_fade = (float)(i + 1) / (float)this.segments.Count;
					segment_position = new Vector2(this.segments[i].X, this.segments[i].Y);
					bb.X = (int)(segment_position.X - (float)(bb.Width / 2));
					bb.Y = (int)(segment_position.Y - (float)(bb.Height / 2));
					source_rect = new Rectangle(32, 64, 32, 32);
					if (i == this.segments.Count - 1)
					{
						source_rect = new Rectangle(64, 64, 32, 32);
					}
					current_rotation = this.segments[i].Z;
				}
				TemporaryAnimatedSprite current_sprite = new TemporaryAnimatedSprite(color: new Color
				{
					R = (byte)Utility.Lerp(255f, 255f, color_fade),
					G = (byte)Utility.Lerp(0f, 166f, color_fade),
					B = (byte)Utility.Lerp(0f, 0f, color_fade),
					A = byte.MaxValue
				}, textureName: this.Sprite.textureName.Value, sourceRect: source_rect, animationInterval: 800f, animationLength: 1, numberOfLoops: 0, position: segment_position, flicker: false, flipped: false, layerDepth: 0.9f, alphaFade: 0.001f, scale: 4f * base.scale.Value, scaleChange: 0.01f, rotation: current_rotation + (float)Math.PI, rotationChange: (float)((double)Game1.random.Next(3, 5) * Math.PI / 64.0))
				{
					motion = new Vector2(xTrajectory, yTrajectory),
					layerDepth = 1f
				};
				current_sprite.alphaFade = 0.025f;
				base.currentLocation.temporarySprites.Add(current_sprite);
				current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 50,
					motion = new Vector2(xTrajectory, yTrajectory),
					layerDepth = 1f
				};
				if (i == -1)
				{
					current_sprite.startSound = "cowboy_monsterhit";
				}
				base.currentLocation.temporarySprites.Add(current_sprite);
				current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 100,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.8f,
					layerDepth = 1f
				};
				if (i == -1)
				{
					current_sprite.startSound = "cowboy_monsterhit";
				}
				base.currentLocation.temporarySprites.Add(current_sprite);
				current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10)
				{
					delayBeforeAnimationStart = 150,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.6f,
					layerDepth = 1f
				};
				if (i == -1)
				{
					current_sprite.startSound = "cowboy_monsterhit";
				}
				base.currentLocation.temporarySprites.Add(current_sprite);
				current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center), Color.LightGreen * 0.6f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 200,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.4f,
					layerDepth = 1f
				};
				if (i == -1)
				{
					current_sprite.startSound = "cowboy_monsterhit";
				}
				base.currentLocation.temporarySprites.Add(current_sprite);
				current_sprite = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10)
				{
					delayBeforeAnimationStart = 250,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.2f,
					layerDepth = 1f
				};
				if (i == -1)
				{
					current_sprite.startSound = "cowboy_monsterhit";
				}
				base.currentLocation.temporarySprites.Add(current_sprite);
			}
		}
		else
		{
			Vector2 standingPixel = Utility.PointToVector2(base.StandingPixel);
			base.currentLocation.localSound("serpentDie");
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Rectangle(0, 64, 32, 32), 200f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f * base.scale.Value, 0.01f, base.rotation + (float)Math.PI, (float)((double)Game1.random.Next(3, 5) * Math.PI / 64.0))
			{
				motion = new Vector2(xTrajectory, yTrajectory),
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 50,
				startSound = "cowboy_monsterhit",
				motion = new Vector2(xTrajectory, yTrajectory),
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 100,
				startSound = "cowboy_monsterhit",
				motion = new Vector2(xTrajectory, yTrajectory) * 0.8f,
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10)
			{
				delayBeforeAnimationStart = 150,
				startSound = "cowboy_monsterhit",
				motion = new Vector2(xTrajectory, yTrajectory) * 0.6f,
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel, Color.LightGreen * 0.6f, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 200,
				startSound = "cowboy_monsterhit",
				motion = new Vector2(xTrajectory, yTrajectory) * 0.4f,
				layerDepth = 1f
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, standingPixel + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10)
			{
				delayBeforeAnimationStart = 250,
				startSound = "cowboy_monsterhit",
				motion = new Vector2(xTrajectory, yTrajectory) * 0.2f,
				layerDepth = 1f
			});
		}
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> items = new List<Item>();
		if (Game1.random.NextDouble() < 0.002)
		{
			items.Add(ItemRegistry.Create("(O)485"));
		}
		return items;
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		Vector2 last_position = base.Position;
		bool is_royal = this.IsRoyalSerpent();
		int standingY = base.StandingPixel.Y;
		for (int i = -1; i < this.segmentCount.Value; i++)
		{
			float sort_offset = (float)(i + 1) * -0.25f / 10000f;
			float max_offset = (float)this.segmentCount.Value * -0.25f / 10000f - 5E-05f;
			if ((float)(standingY - 1) / 10000f + max_offset < 0f)
			{
				sort_offset += 0f - ((float)(standingY - 1) / 10000f + max_offset);
			}
			Rectangle draw_rect = this.Sprite.SourceRect;
			Vector2 shadow_position = base.Position;
			Vector2 draw_position;
			float current_rotation;
			if (i == -1)
			{
				if (is_royal)
				{
					draw_rect = new Rectangle(0, 0, 32, 32);
				}
				draw_position = base.Position;
				current_rotation = base.rotation;
			}
			else
			{
				if (i >= this.segments.Count)
				{
					break;
				}
				Vector3 pos = this.segments[i];
				draw_position = new Vector2(pos.X, pos.Y);
				draw_rect = new Rectangle(32, 0, 32, 32);
				if (i == this.segments.Count - 1)
				{
					draw_rect = new Rectangle(64, 0, 32, 32);
				}
				current_rotation = pos.Z;
				shadow_position = (last_position + draw_position) / 2f;
			}
			if (Utility.isOnScreen(draw_position, 128))
			{
				Vector2 local_draw_position = Game1.GlobalToLocal(Game1.viewport, draw_position) + base.drawOffset + new Vector2(0f, base.yJumpOffset);
				Vector2 local_shadow_position = Game1.GlobalToLocal(Game1.viewport, shadow_position) + base.drawOffset + new Vector2(0f, base.yJumpOffset);
				int boundsHeight = this.GetBoundingBox().Height;
				b.Draw(Game1.shadowTexture, local_shadow_position + new Vector2(64f, boundsHeight), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(standingY - 1) / 10000f + sort_offset);
				b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, boundsHeight / 2), draw_rect, Color.White, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + sort_offset)));
				if (base.isGlowing)
				{
					b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, boundsHeight / 2), draw_rect, base.glowingColor * base.glowingTransparency, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + 0.0001f + sort_offset)));
				}
				if (is_royal)
				{
					sort_offset += -5E-05f;
					current_rotation = 0f;
					draw_rect = new Rectangle(96, 0, 32, 32);
					local_draw_position = Game1.GlobalToLocal(Game1.viewport, last_position) + base.drawOffset + new Vector2(0f, base.yJumpOffset);
					if (i > 0)
					{
						b.Draw(Game1.shadowTexture, local_draw_position + new Vector2(64f, boundsHeight), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(standingY - 1) / 10000f + sort_offset);
					}
					b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, boundsHeight / 2), draw_rect, Color.White, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + sort_offset)));
					if (base.isGlowing)
					{
						b.Draw(this.Sprite.Texture, local_draw_position + new Vector2(64f, boundsHeight / 2), draw_rect, base.glowingColor * base.glowingTransparency, current_rotation, new Vector2(16f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f + 0.0001f + sort_offset)));
					}
				}
			}
			last_position = draw_position;
		}
	}

	public override Rectangle GetBoundingBox()
	{
		Vector2 position = base.Position;
		return new Rectangle((int)position.X + 8, (int)position.Y, this.Sprite.SpriteWidth * 4 * 3 / 4, 96);
	}

	protected override void updateAnimation(GameTime time)
	{
		if (this.IsRoyalSerpent())
		{
			if (this.segments.Count < this.segmentCount.Value)
			{
				for (int i = 0; i < this.segmentCount.Value; i++)
				{
					Vector2 position = base.Position;
					this.segments.Add(new Vector3(position.X, position.Y, 0f));
				}
			}
			Vector2 last_position = base.Position;
			for (int j = 0; j < this.segments.Count; j++)
			{
				Vector2 current_position = new Vector2(this.segments[j].X, this.segments[j].Y);
				Vector2 offset = current_position - last_position;
				int segment_length = 64;
				int num = (int)offset.Length();
				offset.Normalize();
				if (num > segment_length)
				{
					current_position = offset * segment_length + last_position;
				}
				double angle = Math.Atan2(offset.Y, offset.X) - Math.PI / 2.0;
				this.segments[j] = new Vector3(current_position.X, current_position.Y, (float)angle);
				last_position = current_position;
			}
		}
		base.updateAnimation(time);
		if (this.wasHitCounter >= 0)
		{
			this.wasHitCounter -= time.ElapsedGameTime.Milliseconds;
		}
		if (!this.IsRoyalSerpent())
		{
			this.Sprite.Animate(time, 0, 9, 40f);
		}
		if (this.withinPlayerThreshold() && base.invincibleCountdown <= 0)
		{
			Point monsterPixel = base.StandingPixel;
			Point standingPixel = base.Player.StandingPixel;
			float xSlope = -(standingPixel.X - monsterPixel.X);
			float ySlope = standingPixel.Y - monsterPixel.Y;
			float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
			if (t < 64f)
			{
				base.xVelocity = Math.Max(-7f, Math.Min(7f, base.xVelocity * 1.1f));
				base.yVelocity = Math.Max(-7f, Math.Min(7f, base.yVelocity * 1.1f));
			}
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
				this.wasHitCounter = 5 + Game1.random.Next(-1, 2);
			}
			float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
			xSlope = (float)Math.Cos((double)base.rotation + Math.PI / 2.0);
			ySlope = 0f - (float)Math.Sin((double)base.rotation + Math.PI / 2.0);
			base.xVelocity += (0f - xSlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			base.yVelocity += (0f - ySlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			if (Math.Abs(base.xVelocity) > Math.Abs((0f - xSlope) * 7f))
			{
				base.xVelocity -= (0f - xSlope) * maxAccel / 6f;
			}
			if (Math.Abs(base.yVelocity) > Math.Abs((0f - ySlope) * 7f))
			{
				base.yVelocity -= (0f - ySlope) * maxAccel / 6f;
			}
		}
		base.resetAnimationSpeed();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		if (double.IsNaN(base.xVelocity) || double.IsNaN(base.yVelocity))
		{
			base.Health = -500;
		}
		if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
		{
			base.Health = -500;
		}
		if (this.withinPlayerThreshold() && base.invincibleCountdown <= 0)
		{
			this.faceDirection(2);
		}
	}
}
