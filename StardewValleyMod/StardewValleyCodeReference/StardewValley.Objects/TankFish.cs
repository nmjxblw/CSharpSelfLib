using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects.Trinkets;

namespace StardewValley.Objects;

public class TankFish
{
	public enum FishType
	{
		Normal,
		Eel,
		Cephalopod,
		Float,
		Ground,
		Crawl,
		Hop,
		Static
	}

	/// <summary>The field index in <c>Data/AquariumFish</c> for the sprite index.</summary>
	public const int field_spriteIndex = 0;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the type.</summary>
	public const int field_type = 1;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the idle animations.</summary>
	public const int field_idleAnimations = 2;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the dart start animation frames.</summary>
	public const int field_dartStartFrames = 3;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the dart hold animation frames.</summary>
	public const int field_dartHoldFrames = 4;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the dart end animation frames.</summary>
	public const int field_dartEndFrames = 5;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the texture, if set.</summary>
	public const int field_texture = 6;

	/// <summary>The field index in <c>Data/AquariumFish</c> for the pixel offset from the upper-left corner of sprite that the hat sits on, if set.</summary>
	public const int field_hatOffset = 7;

	protected FishTankFurniture _tank;

	public Vector2 position;

	public float zPosition;

	public bool facingLeft;

	public Vector2 velocity = Vector2.Zero;

	protected Texture2D _texture;

	public float nextSwim;

	public string fishItemId = "";

	public int fishIndex;

	public int currentFrame;

	public Point? hatPosition;

	public int frogVariant;

	public int numberOfDarts;

	public FishType fishType;

	public float minimumVelocity;

	public float fishScale = 1f;

	public List<int> currentAnimation;

	public List<int> idleAnimation;

	public List<int> dartStartAnimation;

	public List<int> dartHoldAnimation;

	public List<int> dartEndAnimation;

	public int currentAnimationFrame;

	public float currentFrameTime;

	public float nextBubble;

	public bool isErrorFish;

	public TankFish(FishTankFurniture tank, Item item)
	{
		this._tank = tank;
		this.fishItemId = item.ItemId;
		if (!this._tank.GetAquariumData().TryGetValue(item.ItemId, out var rawAquariumData))
		{
			rawAquariumData = "0/float";
			this.isErrorFish = true;
		}
		string[] aquarium_fish_split = rawAquariumData.Split('/');
		string rawTexture = ArgUtility.Get(aquarium_fish_split, 6, null, allowBlank: false);
		if (rawTexture != null)
		{
			try
			{
				this._texture = Game1.content.Load<Texture2D>(rawTexture);
			}
			catch (Exception)
			{
				this.isErrorFish = true;
			}
		}
		if (this._texture == null)
		{
			this._texture = this._tank.GetAquariumTexture();
		}
		string rawHatOffset = ArgUtility.Get(aquarium_fish_split, 7, null, allowBlank: false);
		if (rawHatOffset != null)
		{
			try
			{
				string[] point_split = ArgUtility.SplitBySpace(rawHatOffset);
				this.hatPosition = new Point(int.Parse(point_split[0]), int.Parse(point_split[1]));
			}
			catch (Exception)
			{
				this.hatPosition = null;
			}
		}
		this.fishIndex = int.Parse(aquarium_fish_split[0]);
		this.currentFrame = this.fishIndex;
		this.zPosition = Utility.RandomFloat(4f, 10f);
		this.fishScale = 0.75f;
		if (DataLoader.Fish(Game1.content).TryGetValue(item.ItemId, out var fish_data))
		{
			string[] fish_split = fish_data.Split('/');
			if (!(fish_split[1] == "trap"))
			{
				this.minimumVelocity = Utility.RandomFloat(0.25f, 0.35f);
				if (fish_split[2] == "smooth")
				{
					this.minimumVelocity = Utility.RandomFloat(0.5f, 0.6f);
				}
				if (fish_split[2] == "dart")
				{
					this.minimumVelocity = 0f;
				}
			}
		}
		switch (ArgUtility.Get(aquarium_fish_split, 1))
		{
		case "eel":
			this.fishType = FishType.Eel;
			this.minimumVelocity = Utility.Clamp(this.fishScale, 0.3f, 0.4f);
			break;
		case "cephalopod":
			this.fishType = FishType.Cephalopod;
			this.minimumVelocity = 0f;
			break;
		case "ground":
			this.fishType = FishType.Ground;
			this.zPosition = 4f;
			this.minimumVelocity = 0f;
			break;
		case "static":
			this.fishType = FishType.Static;
			break;
		case "crawl":
			this.fishType = FishType.Crawl;
			this.minimumVelocity = 0f;
			break;
		case "front_crawl":
			this.fishType = FishType.Crawl;
			this.zPosition = 3f;
			this.minimumVelocity = 0f;
			break;
		case "float":
			this.fishType = FishType.Float;
			break;
		}
		string rawIdleAnimation = ArgUtility.Get(aquarium_fish_split, 2, null, allowBlank: false);
		if (rawIdleAnimation != null)
		{
			string[] array = ArgUtility.SplitBySpace(rawIdleAnimation);
			this.idleAnimation = new List<int>();
			string[] array2 = array;
			foreach (string frame in array2)
			{
				this.idleAnimation.Add(int.Parse(frame));
			}
			this.SetAnimation(this.idleAnimation);
		}
		string rawDartStartFrames = ArgUtility.Get(aquarium_fish_split, 3, null, allowBlank: false);
		if (rawDartStartFrames != null)
		{
			string[] array3 = ArgUtility.SplitBySpace(rawDartStartFrames);
			this.dartStartAnimation = new List<int>();
			string[] array2 = array3;
			foreach (string frame2 in array2)
			{
				this.dartStartAnimation.Add(int.Parse(frame2));
			}
		}
		string rawDartHoldFrames = ArgUtility.Get(aquarium_fish_split, 4, null, allowBlank: false);
		if (rawDartHoldFrames != null)
		{
			string[] array4 = ArgUtility.SplitBySpace(rawDartHoldFrames);
			this.dartHoldAnimation = new List<int>();
			string[] array2 = array4;
			foreach (string frame3 in array2)
			{
				this.dartHoldAnimation.Add(int.Parse(frame3));
			}
		}
		string rawDartEndFrames = ArgUtility.Get(aquarium_fish_split, 5, null, allowBlank: false);
		if (rawDartEndFrames != null)
		{
			string[] array5 = ArgUtility.SplitBySpace(rawDartEndFrames);
			this.dartEndAnimation = new List<int>();
			string[] array2 = array5;
			foreach (string frame4 in array2)
			{
				this.dartEndAnimation.Add(int.Parse(frame4));
			}
		}
		Rectangle tank_bounds_local = this._tank.GetTankBounds();
		tank_bounds_local.X = 0;
		tank_bounds_local.Y = 0;
		this.position = Vector2.Zero;
		this.position = Utility.getRandomPositionInThisRectangle(tank_bounds_local, Game1.random);
		this.nextSwim = Utility.RandomFloat(0.1f, 10f);
		this.nextBubble = Utility.RandomFloat(0.1f, 10f);
		this.facingLeft = Game1.random.Next(2) == 1;
		if (this.facingLeft)
		{
			this.velocity = new Vector2(-1f, 0f);
		}
		else
		{
			this.velocity = new Vector2(1f, 0f);
		}
		this.velocity *= this.minimumVelocity;
		if (item.QualifiedItemId == "(TR)FrogEgg")
		{
			this.fishType = FishType.Hop;
			this._texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
			this.frogVariant = ((item as Trinket).GetEffect() as CompanionTrinketEffect).Variant;
			this.isErrorFish = false;
		}
		if (this.fishType == FishType.Ground || this.fishType == FishType.Crawl || this.fishType == FishType.Hop || this.fishType == FishType.Static)
		{
			this.position.Y = 0f;
		}
		this.ConstrainToTank();
	}

	public void SetAnimation(List<int> frames)
	{
		if (this.fishType != FishType.Hop && this.currentAnimation != frames)
		{
			this.currentAnimation = frames;
			this.currentAnimationFrame = 0;
			this.currentFrameTime = 0f;
			List<int> list = this.currentAnimation;
			if (list != null && list.Count > 0)
			{
				this.currentFrame = frames[0];
			}
		}
	}

	public virtual void Draw(SpriteBatch b, float alpha, float draw_layer)
	{
		SpriteEffects sprite_effects = SpriteEffects.None;
		int draw_offset = -12;
		int slice_size = 8;
		if (this.fishType == FishType.Eel)
		{
			slice_size = 4;
		}
		int slice_offset = slice_size;
		if (this.facingLeft)
		{
			sprite_effects = SpriteEffects.FlipHorizontally;
			slice_offset *= -1;
			draw_offset = -draw_offset - slice_size;
		}
		float bob = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.25 + (double)(this.position.X / 32f)) * 2f;
		if (this.fishType == FishType.Crawl || this.fishType == FishType.Ground || this.fishType == FishType.Static)
		{
			bob = 0f;
		}
		float scale = this.GetScale();
		int cols = this._texture.Width / 24;
		int sprite_sheet_x = this.currentFrame % cols * 24;
		int sprite_sheet_y = this.currentFrame / cols * 48;
		int wiggle_start_pixels = 10;
		float wiggle_amount = 1f;
		if (this.fishType == FishType.Eel)
		{
			wiggle_start_pixels = 20;
			bob *= 0f;
		}
		float hatOffsetY = -12f;
		float angle = 0f;
		if (this.isErrorFish)
		{
			angle = 0f;
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(F)");
			b.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), itemType.GetErrorSourceRect(), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
		}
		else
		{
			switch (this.fishType)
			{
			case FishType.Ground:
			case FishType.Crawl:
			case FishType.Static:
				angle = 0f;
				b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
				break;
			case FishType.Hop:
			{
				int frame = 0;
				if (this.position.Y > 0f)
				{
					frame = ((!((double)this.velocity.Y > 0.2)) ? 3 : (((double)this.velocity.Y > 0.3) ? 1 : 2));
				}
				else if (this.nextSwim <= 3f)
				{
					frame = ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 >= 200.0) ? 5 : 6);
				}
				Rectangle rect = new Rectangle(frame * 16, 16 + this.frogVariant * 16, 16, 16);
				Color c = Color.White;
				if (this.frogVariant == 7)
				{
					c = Utility.GetPrismaticColor();
				}
				b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(16f, -8f)), rect, c * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
				break;
			}
			case FishType.Cephalopod:
			case FishType.Float:
				angle = Utility.Clamp(this.velocity.X, -0.5f, 0.5f);
				b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
				break;
			default:
			{
				for (int slice = 0; slice < 24 / slice_size; slice++)
				{
					float multiplier = (float)(slice * slice_size) / (float)wiggle_start_pixels;
					multiplier = 1f - multiplier;
					float velocity_multiplier = this.velocity.Length() / 1f;
					float time_multiplier = 1f;
					float position_multiplier = 0f;
					velocity_multiplier = Utility.Clamp(velocity_multiplier, 0.2f, 1f);
					multiplier = Utility.Clamp(multiplier, 0f, 1f);
					if (this.fishType == FishType.Eel)
					{
						multiplier = 1f;
						velocity_multiplier = 1f;
						time_multiplier = 0.1f;
						position_multiplier = 4f;
					}
					if (this.facingLeft)
					{
						position_multiplier *= -1f;
					}
					float yOffset = (float)(Math.Sin((double)(slice * 20) + Game1.currentGameTime.TotalGameTime.TotalSeconds * 25.0 * (double)time_multiplier + (double)(position_multiplier * this.position.X / 16f)) * (double)wiggle_amount * (double)multiplier * (double)velocity_multiplier);
					if (slice == 24 / slice_size - 1)
					{
						hatOffsetY = -12f + yOffset;
					}
					b.Draw(this._texture, Game1.GlobalToLocal(this.GetWorldPosition() + new Vector2(draw_offset + slice * slice_offset, bob + yOffset) * 4f * scale), new Rectangle(sprite_sheet_x + slice * slice_size, sprite_sheet_y, slice_size, 24), Color.White * alpha, 0f, new Vector2(0f, 12f), 4f * scale, sprite_effects, draw_layer);
				}
				break;
			}
			}
		}
		float hatOffsetX = (this.facingLeft ? 12 : (-12));
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2(this.GetWorldPosition().X, (float)this._tank.GetTankBounds().Bottom - this.zPosition * 4f)), null, Color.White * alpha * 0.75f, 0f, new Vector2(Game1.shadowTexture.Width / 2, Game1.shadowTexture.Height / 2), new Vector2(4f * scale, 1f), SpriteEffects.None, this._tank.GetFishSortRegion().X - 1E-07f);
		int hatsDrawn = 0;
		foreach (TankFish fish in this._tank.tankFish)
		{
			if (fish == this)
			{
				break;
			}
			if (fish.CanWearHat())
			{
				hatsDrawn++;
			}
		}
		if (!this.CanWearHat())
		{
			return;
		}
		int hatsSoFar = 0;
		foreach (Item heldItem in this._tank.heldItems)
		{
			if (!(heldItem is Hat hat))
			{
				continue;
			}
			if (hatsSoFar == hatsDrawn)
			{
				Vector2 hatPlacementOffset = new Vector2(this.hatPosition.Value.X, this.hatPosition.Value.Y);
				if (this.facingLeft)
				{
					hatPlacementOffset.X *= -1f;
				}
				Vector2 hatOffset = new Vector2(hatOffsetX, hatOffsetY) + hatPlacementOffset;
				if (angle != 0f)
				{
					float cos = (float)Math.Cos(angle);
					float sin = (float)Math.Sin(angle);
					hatOffset.X = hatOffset.X * cos - hatOffset.Y * sin;
					hatOffset.Y = hatOffset.X * sin + hatOffset.Y * cos;
				}
				hatOffset *= 4f * scale;
				Vector2 pos = Game1.GlobalToLocal(this.GetWorldPosition() + hatOffset);
				pos.Y += bob;
				int direction = ((this.fishType == FishType.Cephalopod || this.fishType == FishType.Static) ? 2 : ((!this.facingLeft) ? 1 : 3));
				pos -= new Vector2(10f, 10f);
				pos += new Vector2(3f, 3f) * scale * 3f;
				pos -= new Vector2(10f, 10f) * scale * 3f;
				hat.draw(b, pos, scale, 1f, draw_layer + 1E-08f, direction);
				hatsDrawn++;
				break;
			}
			hatsSoFar++;
		}
	}

	[MemberNotNullWhen(true, "hatPosition")]
	public bool CanWearHat()
	{
		return this.hatPosition.HasValue;
	}

	public Vector2 GetWorldPosition()
	{
		return new Vector2((float)this._tank.GetTankBounds().X + this.position.X, (float)this._tank.GetTankBounds().Bottom - this.position.Y - this.zPosition * 4f);
	}

	public void ConstrainToTank()
	{
		Rectangle tank_bounds = this._tank.GetTankBounds();
		Rectangle bounds = this.GetBounds();
		tank_bounds.X = 0;
		tank_bounds.Y = 0;
		if (bounds.X < tank_bounds.X)
		{
			this.position.X += tank_bounds.X - bounds.X;
			bounds = this.GetBounds();
		}
		if (bounds.Y < tank_bounds.Y)
		{
			this.position.Y -= tank_bounds.Y - bounds.Y;
			bounds = this.GetBounds();
		}
		if (bounds.Right > tank_bounds.Right)
		{
			this.position.X += tank_bounds.Right - bounds.Right;
			bounds = this.GetBounds();
		}
		if (this.fishType == FishType.Crawl || this.fishType == FishType.Ground || this.fishType == FishType.Static || this.fishType == FishType.Hop)
		{
			if (this.position.Y > (float)tank_bounds.Bottom)
			{
				this.position.Y -= (float)tank_bounds.Bottom - this.position.Y;
			}
		}
		else if (bounds.Bottom > tank_bounds.Bottom)
		{
			this.position.Y -= tank_bounds.Bottom - bounds.Bottom;
		}
	}

	public virtual float GetScale()
	{
		return this.fishScale;
	}

	public Rectangle GetBounds()
	{
		Vector2 dimensions = new Vector2(24f, 18f);
		dimensions *= 4f * this.GetScale();
		if (this.fishType == FishType.Crawl || this.fishType == FishType.Ground || this.fishType == FishType.Static || this.fishType == FishType.Hop)
		{
			return new Rectangle((int)(this.position.X - dimensions.X / 2f), (int)((float)this._tank.GetTankBounds().Height - this.position.Y - dimensions.Y), (int)dimensions.X, (int)dimensions.Y);
		}
		return new Rectangle((int)(this.position.X - dimensions.X / 2f), (int)((float)this._tank.GetTankBounds().Height - this.position.Y - dimensions.Y / 2f), (int)dimensions.X, (int)dimensions.Y);
	}

	public virtual void Update(GameTime time)
	{
		List<int> list = this.currentAnimation;
		if (list != null && list.Count > 0)
		{
			this.currentFrameTime += (float)time.ElapsedGameTime.TotalSeconds;
			float seconds_per_frame = 0.125f;
			if (this.currentFrameTime > seconds_per_frame)
			{
				this.currentAnimationFrame += (int)(this.currentFrameTime / seconds_per_frame);
				this.currentFrameTime %= seconds_per_frame;
				if (this.currentAnimationFrame >= this.currentAnimation.Count)
				{
					if (this.currentAnimation == this.idleAnimation)
					{
						this.currentAnimationFrame %= this.currentAnimation.Count;
						this.currentFrame = this.currentAnimation[this.currentAnimationFrame];
					}
					else if (this.currentAnimation == this.dartStartAnimation)
					{
						if (this.dartHoldAnimation != null)
						{
							this.SetAnimation(this.dartHoldAnimation);
						}
						else
						{
							this.SetAnimation(this.idleAnimation);
						}
					}
					else if (this.currentAnimation == this.dartHoldAnimation)
					{
						this.currentAnimationFrame %= this.currentAnimation.Count;
						this.currentFrame = this.currentAnimation[this.currentAnimationFrame];
					}
					else if (this.currentAnimation == this.dartEndAnimation)
					{
						this.SetAnimation(this.idleAnimation);
					}
				}
				else
				{
					this.currentFrame = this.currentAnimation[this.currentAnimationFrame];
				}
			}
		}
		if (this.fishType != FishType.Static)
		{
			Rectangle local_tank_bounds = this._tank.GetTankBounds();
			local_tank_bounds.X = 0;
			local_tank_bounds.Y = 0;
			float velocity_x = this.velocity.X;
			if (this.fishType == FishType.Crawl)
			{
				velocity_x = Utility.Clamp(velocity_x, -0.5f, 0.5f);
			}
			this.position.X += velocity_x;
			Rectangle bounds = this.GetBounds();
			if (bounds.Left < local_tank_bounds.Left || bounds.Right > local_tank_bounds.Right)
			{
				this.ConstrainToTank();
				bounds = this.GetBounds();
				this.velocity.X *= -1f;
				this.facingLeft = !this.facingLeft;
			}
			this.position.Y += this.velocity.Y;
			bounds = this.GetBounds();
			if (bounds.Top < local_tank_bounds.Top || bounds.Bottom > local_tank_bounds.Bottom)
			{
				this.ConstrainToTank();
				this.velocity.Y *= 0f;
			}
			float move_magnitude = this.velocity.Length();
			if (move_magnitude > this.minimumVelocity)
			{
				float deceleration = 0.015f;
				if (this.fishType == FishType.Crawl || this.fishType == FishType.Ground || this.fishType == FishType.Hop)
				{
					deceleration = 0.03f;
				}
				move_magnitude = Utility.Lerp(move_magnitude, this.minimumVelocity, deceleration);
				if (move_magnitude < 0.0001f)
				{
					move_magnitude = 0f;
				}
				this.velocity.Normalize();
				this.velocity *= move_magnitude;
				if (this.currentAnimation == this.dartHoldAnimation && move_magnitude <= this.minimumVelocity + 0.5f)
				{
					List<int> list2 = this.dartEndAnimation;
					if (list2 != null && list2.Count > 0)
					{
						this.SetAnimation(this.dartEndAnimation);
					}
					else
					{
						List<int> list3 = this.idleAnimation;
						if (list3 != null && list3.Count > 0)
						{
							this.SetAnimation(this.idleAnimation);
						}
					}
				}
			}
			this.nextSwim -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.nextSwim <= 0f)
			{
				if (this.numberOfDarts == 0)
				{
					this.numberOfDarts = Game1.random.Next(1, 4);
					this.nextSwim = Utility.RandomFloat(6f, 12f);
					switch (this.fishType)
					{
					case FishType.Cephalopod:
						this.nextSwim = Utility.RandomFloat(2f, 5f);
						break;
					case FishType.Hop:
						this.numberOfDarts = 0;
						break;
					}
					if (Game1.random.NextDouble() < 0.30000001192092896)
					{
						this.facingLeft = !this.facingLeft;
					}
				}
				else
				{
					this.nextSwim = Utility.RandomFloat(0.1f, 0.5f);
					this.numberOfDarts--;
					if (Game1.random.NextDouble() < 0.05000000074505806)
					{
						this.facingLeft = !this.facingLeft;
					}
				}
				List<int> list4 = this.dartStartAnimation;
				if (list4 != null && list4.Count > 0)
				{
					this.SetAnimation(this.dartStartAnimation);
				}
				else
				{
					List<int> list5 = this.dartHoldAnimation;
					if (list5 != null && list5.Count > 0)
					{
						this.SetAnimation(this.dartHoldAnimation);
					}
				}
				this.velocity.X = 1.5f;
				if (this._tank.getTilesWide() <= 2)
				{
					this.velocity.X *= 0.5f;
				}
				if (this.facingLeft)
				{
					this.velocity.X *= -1f;
				}
				switch (this.fishType)
				{
				case FishType.Cephalopod:
					this.velocity.Y = Utility.RandomFloat(0.5f, 0.75f);
					break;
				case FishType.Ground:
					this.velocity.X *= 0.5f;
					this.velocity.Y = Utility.RandomFloat(0.5f, 0.25f);
					break;
				case FishType.Hop:
					this.velocity.Y = Utility.RandomFloat(0.35f, 0.65f);
					break;
				default:
					this.velocity.Y = Utility.RandomFloat(-0.5f, 0.5f);
					break;
				}
				if (this.fishType == FishType.Crawl)
				{
					this.velocity.Y = 0f;
				}
			}
		}
		if (this.fishType == FishType.Cephalopod || this.fishType == FishType.Ground || this.fishType == FishType.Crawl || this.fishType == FishType.Static || this.fishType == FishType.Hop)
		{
			float fall_speed = 0.2f;
			if (this.fishType == FishType.Static)
			{
				fall_speed = 0.6f;
			}
			if (this.position.Y > 0f)
			{
				this.position.Y -= fall_speed;
			}
		}
		this.nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
		if (this.nextBubble <= 0f)
		{
			this.nextBubble = Utility.RandomFloat(1f, 10f);
			float x_offset = 0f;
			if (this.fishType == FishType.Ground || this.fishType == FishType.Normal || this.fishType == FishType.Eel)
			{
				x_offset = 32f;
			}
			if (this.facingLeft)
			{
				x_offset *= -1f;
			}
			x_offset *= this.fishScale;
			this._tank.bubbles.Add(new Vector4(this.position.X + x_offset, this.position.Y + this.zPosition, this.zPosition, 0.25f));
		}
		this.ConstrainToTank();
	}
}
