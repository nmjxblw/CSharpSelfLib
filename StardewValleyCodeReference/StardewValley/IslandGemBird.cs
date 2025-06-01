using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley;

public class IslandGemBird : INetObject<NetFields>
{
	public enum GemBirdType
	{
		Emerald,
		Aquamarine,
		Ruby,
		Amethyst,
		Topaz,
		MAX
	}

	[XmlIgnore]
	public Texture2D texture;

	[XmlElement("position")]
	public NetVector2 position = new NetVector2();

	[XmlIgnore]
	protected float _destroyTimer;

	[XmlElement("height")]
	public NetFloat height = new NetFloat();

	[XmlIgnore]
	public int[] idleAnimation = new int[1];

	[XmlIgnore]
	public int[] lookBackAnimation = new int[17]
	{
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1
	};

	[XmlIgnore]
	public int[] scratchAnimation = new int[19]
	{
		0, 1, 2, 3, 2, 3, 2, 3, 2, 3,
		2, 3, 2, 3, 2, 3, 2, 3, 2
	};

	[XmlIgnore]
	public int[] flyAnimation = new int[11]
	{
		4, 5, 6, 7, 7, 6, 6, 5, 5, 4,
		4
	};

	[XmlIgnore]
	public int[] currentAnimation;

	[XmlIgnore]
	public float frameTimer;

	[XmlIgnore]
	public int currentFrameIndex;

	[XmlIgnore]
	public float idleAnimationTime;

	[XmlElement("alpha")]
	public NetFloat alpha = new NetFloat(1f);

	[XmlElement("flying")]
	public NetBool flying = new NetBool();

	[XmlElement("color")]
	public NetColor color = new NetColor();

	[XmlElement("itemIndex")]
	public NetString itemIndex = new NetString("0");

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("IslandGemBird");

	public IslandGemBird()
	{
		this.texture = Game1.content.Load<Texture2D>("LooseSprites\\GemBird");
		this.InitNetFields();
	}

	public IslandGemBird(Vector2 tile_position, GemBirdType bird_type)
		: this()
	{
		this.position.Value = (tile_position + new Vector2(0.5f, 0.5f)) * 64f;
		this.color.Value = IslandGemBird.GetColor(bird_type);
		this.itemIndex.Value = IslandGemBird.GetItemIndex(bird_type);
	}

	public static Color GetColor(GemBirdType bird_type)
	{
		return bird_type switch
		{
			GemBirdType.Emerald => new Color(67, 255, 83), 
			GemBirdType.Aquamarine => new Color(74, 243, 255), 
			GemBirdType.Ruby => new Color(255, 38, 38), 
			GemBirdType.Amethyst => new Color(255, 67, 251), 
			GemBirdType.Topaz => new Color(255, 156, 33), 
			_ => Color.White, 
		};
	}

	public static string GetItemIndex(GemBirdType bird_type)
	{
		return bird_type switch
		{
			GemBirdType.Emerald => "60", 
			GemBirdType.Aquamarine => "62", 
			GemBirdType.Ruby => "64", 
			GemBirdType.Amethyst => "66", 
			GemBirdType.Topaz => "68", 
			_ => "0", 
		};
	}

	public static GemBirdType GetBirdTypeForLocation(string location)
	{
		List<string> island_locations = new List<string> { "IslandNorth", "IslandSouth", "IslandEast", "IslandWest" };
		if (!island_locations.Contains(location))
		{
			return GemBirdType.Aquamarine;
		}
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame);
		List<GemBirdType> types = new List<GemBirdType>();
		for (int i = 0; i < 5; i++)
		{
			types.Add((GemBirdType)i);
		}
		Utility.Shuffle(r, types);
		return types[island_locations.IndexOf(location)];
	}

	public void Draw(SpriteBatch b)
	{
		if (this.currentAnimation != null)
		{
			int frame = this.currentAnimation[Math.Min(this.currentFrameIndex, this.currentAnimation.Length - 1)];
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.position.Value + new Vector2(0f, 0f - this.height.Value)), new Rectangle(frame * 32, 0, 32, 32), Color.White * this.alpha.Value, 0f, new Vector2(16f, 32f), 4f, SpriteEffects.None, (this.position.Value.Y - 1f) / 10000f);
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.position.Value + new Vector2(0f, 0f - this.height.Value)), new Rectangle(frame * 32, 32, 32, 32), this.color.Value * this.alpha.Value, 0f, new Vector2(16f, 32f), 4f, SpriteEffects.None, this.position.Value.Y / 10000f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position.Value), Game1.shadowTexture.Bounds, Color.White * this.alpha.Value, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (this.position.Y - 2f) / 10000f);
		}
	}

	public void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.position, "position").AddField(this.flying, "flying")
			.AddField(this.height, "height")
			.AddField(this.color, "color")
			.AddField(this.alpha, "alpha")
			.AddField(this.itemIndex, "itemIndex");
		this.position.Interpolated(interpolate: true, wait: true);
		this.height.Interpolated(interpolate: true, wait: true);
		this.alpha.Interpolated(interpolate: true, wait: true);
	}

	public bool Update(GameTime time, GameLocation location)
	{
		if (this.currentAnimation == null)
		{
			this.currentAnimation = this.idleAnimation;
		}
		this.frameTimer += (float)time.ElapsedGameTime.TotalSeconds;
		float frame_time = 0.15f;
		if (this.flying.Value)
		{
			frame_time = 0.05f;
		}
		if (this.frameTimer >= frame_time)
		{
			this.frameTimer = 0f;
			this.currentFrameIndex++;
			if (this.currentFrameIndex >= this.currentAnimation.Length)
			{
				this.currentFrameIndex = 0;
				if (this.currentAnimation == this.flyAnimation && location == Game1.currentLocation && Utility.isOnScreen(this.position.Value + new Vector2(0f, 0f - this.height.Value), 64))
				{
					Game1.playSound("batFlap");
				}
				if (this.currentAnimation == this.lookBackAnimation || this.currentAnimation == this.scratchAnimation)
				{
					this.currentAnimation = this.idleAnimation;
				}
			}
		}
		if (this.flying.Value)
		{
			this.currentAnimation = this.flyAnimation;
			if (Game1.IsMasterGame)
			{
				this.height.Value += 4f;
				this.position.X -= 3f;
				if (this.alpha.Value > 0f && this.height.Value >= 300f)
				{
					this.alpha.Value -= 0.01f;
					if (this.alpha.Value < 0f)
					{
						this.alpha.Value = 0f;
					}
				}
			}
		}
		else
		{
			if (this.currentAnimation == this.idleAnimation)
			{
				this.idleAnimationTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (this.idleAnimationTime <= 0f)
			{
				this.currentFrameIndex = 0;
				if (Game1.random.NextDouble() < 0.75)
				{
					this.currentAnimation = this.lookBackAnimation;
				}
				else
				{
					this.currentAnimation = this.scratchAnimation;
				}
				this.idleAnimationTime = Utility.RandomFloat(1f, 3f);
			}
		}
		if (Game1.IsMasterGame && !this.flying.Value)
		{
			foreach (Farmer farmer in location.farmers)
			{
				Vector2 offset = farmer.Position - this.position.Value;
				if (Math.Abs(offset.X) <= 128f && Math.Abs(offset.Y) <= 128f)
				{
					this.flying.Value = true;
					location.playSound("parrot");
					Game1.createObjectDebris(this.itemIndex.Value, (int)(this.position.X / 64f), (int)(this.position.Y / 64f), location);
				}
			}
		}
		if (this.alpha.Value <= 0f)
		{
			if (this._destroyTimer == 0f)
			{
				this._destroyTimer = 3f;
			}
			else if (this._destroyTimer >= 0f)
			{
				this._destroyTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._destroyTimer <= 0f)
				{
					return true;
				}
			}
		}
		return false;
	}
}
