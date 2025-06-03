using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Companions;

public class Companion : INetObject<NetFields>
{
	public readonly NetInt direction = new NetInt();

	protected readonly NetPosition _position = new NetPosition();

	protected readonly NetFarmerRef _owner = new NetFarmerRef();

	public readonly NetInt whichVariant = new NetInt();

	public float lerp = -1f;

	public Vector2 startPosition;

	public Vector2 endPosition;

	public float height;

	public float gravity;

	public NetEvent1Field<float, NetFloat> hopEvent = new NetEvent1Field<float, NetFloat>();

	public NetFields NetFields { get; } = new NetFields("Companion");

	public Farmer Owner
	{
		get
		{
			return this._owner.Value;
		}
		set
		{
			this._owner.Value = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			return this._position.Value;
		}
		set
		{
			this._position.Value = value;
		}
	}

	public Vector2 OwnerPosition => Utility.PointToVector2(this.Owner.GetBoundingBox().Center);

	public bool IsLocal => this.Owner.IsLocalPlayer;

	public Companion()
	{
		this.InitNetFields();
		this.direction.Value = 1;
	}

	public virtual void InitializeCompanion(Farmer farmer)
	{
		this._owner.Value = farmer;
		this._position.Value = farmer.Position;
	}

	public virtual void CleanupCompanion()
	{
		this._owner.Value = null;
	}

	public virtual void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this._owner.NetFields, "_owner.NetFields").AddField(this._position.NetFields, "_position.NetFields")
			.AddField(this.hopEvent, "hopEvent")
			.AddField(this.direction, "direction")
			.AddField(this.whichVariant, "whichVariant");
		this.hopEvent.onEvent += Hop;
	}

	public virtual void Hop(float amount)
	{
		this.height = 0f;
		this.gravity = amount;
	}

	public virtual void Update(GameTime time, GameLocation location)
	{
		if (this.IsLocal)
		{
			if (this.lerp < 0f)
			{
				if ((this.OwnerPosition - this.Position).Length() > 768f)
				{
					Utility.addRainbowStarExplosion(location, this.Position + new Vector2(0f, 0f - this.height), 1);
					this.Position = this.Owner.Position;
					this.lerp = -1f;
				}
				if ((this.OwnerPosition - this.Position).Length() > 80f)
				{
					this.startPosition = this.Position;
					float radius = 0.33f;
					this.endPosition = this.OwnerPosition + new Vector2(Utility.RandomFloat(-64f, 64f) * radius, Utility.RandomFloat(-64f, 64f) * radius);
					if (location.isCollidingPosition(new Rectangle((int)this.endPosition.X - 8, (int)this.endPosition.Y - 8, 16, 16), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: true, projectile: false, ignoreCharacterRequirement: true))
					{
						this.endPosition = this.OwnerPosition;
					}
					this.lerp = 0f;
					this.hopEvent.Fire(1f);
					if (Math.Abs(this.OwnerPosition.X - this.Position.X) > 8f)
					{
						if (this.OwnerPosition.X > this.Position.X)
						{
							this.direction.Value = 1;
						}
						else
						{
							this.direction.Value = 3;
						}
					}
				}
			}
			if (this.lerp >= 0f)
			{
				this.lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
				if (this.lerp > 1f)
				{
					this.lerp = 1f;
				}
				float x = Utility.Lerp(this.startPosition.X, this.endPosition.X, this.lerp);
				float y = Utility.Lerp(this.startPosition.Y, this.endPosition.Y, this.lerp);
				this.Position = new Vector2(x, y);
				if (this.lerp == 1f)
				{
					this.lerp = -1f;
				}
			}
		}
		this.hopEvent.Poll();
		if (this.gravity != 0f || this.height != 0f)
		{
			this.height += this.gravity;
			this.gravity -= (float)time.ElapsedGameTime.TotalSeconds * 6f;
			if (this.height <= 0f)
			{
				this.height = 0f;
				this.gravity = 0f;
			}
		}
	}

	public virtual void Draw(SpriteBatch b)
	{
	}

	public virtual void OnOwnerWarp()
	{
		this._position.Value = this._owner.Value.Position;
	}
}
