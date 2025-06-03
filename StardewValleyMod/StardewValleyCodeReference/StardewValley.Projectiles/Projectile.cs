using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace StardewValley.Projectiles;

public abstract class Projectile : INetObject<NetFields>, IHaveModData
{
	public const int travelTimeBeforeCollisionPossible = 100;

	public const int goblinsCurseIndex = 0;

	public const int flameBallIndex = 1;

	public const int fearBolt = 2;

	public const int shadowBall = 3;

	public const int bone = 4;

	public const int throwingKnife = 5;

	public const int snowBall = 6;

	public const int shamanBolt = 7;

	public const int frostBall = 8;

	public const int frozenBolt = 9;

	public const int fireball = 10;

	public const int slash = 11;

	public const int arrowBolt = 12;

	public const int launchedSlime = 13;

	public const int magicArrow = 14;

	public const int iceOrb = 15;

	public const string projectileSheetName = "TileSheets\\Projectiles";

	public const int timePerTailUpdate = 50;

	public readonly NetInt boundingBoxWidth = new NetInt(21);

	public static Texture2D projectileSheet;

	protected float startingAlpha = 1f;

	/// <summary>The index of the sprite to draw in <see cref="F:StardewValley.Projectiles.Projectile.projectileSheetName" />. Ignored if <see cref="F:StardewValley.Projectiles.Projectile.itemId" /> is set.</summary>
	[XmlIgnore]
	public readonly NetInt currentTileSheetIndex = new NetInt();

	/// <summary>The qualified item ID for the item to draw. If set, this overrides <see cref="F:StardewValley.Projectiles.Projectile.currentTileSheetIndex" />.</summary>
	[XmlIgnore]
	public readonly NetString itemId = new NetString();

	/// <summary>The projectile's pixel position in the world.</summary>
	[XmlIgnore]
	public readonly NetPosition position = new NetPosition();

	/// <summary>The length of the tail which trails behind the main projectile.</summary>
	[XmlIgnore]
	public readonly NetInt tailLength = new NetInt();

	[XmlIgnore]
	public int tailCounter = 50;

	/// <summary>The sound to play when the projectile bounces off a wall.</summary>
	public readonly NetString bounceSound = new NetString();

	/// <summary>The number of times the projectile can bounce off walls before being destroyed.</summary>
	[XmlIgnore]
	public readonly NetInt bouncesLeft = new NetInt();

	/// <summary>The number of times the projectile can pierce through an enemy before being destroyed.</summary>
	public readonly NetInt piercesLeft = new NetInt(1);

	public int travelTime;

	protected float? _rotation;

	[XmlIgnore]
	public float hostTimeUntilAttackable = -1f;

	public readonly NetFloat startingRotation = new NetFloat();

	/// <summary>The rotation velocity.</summary>
	[XmlIgnore]
	public readonly NetFloat rotationVelocity = new NetFloat();

	public readonly NetFloat alpha = new NetFloat(1f);

	public readonly NetFloat alphaChange = new NetFloat(0f);

	/// <summary>The speed at which the projectile moves along the X axis.</summary>
	[XmlIgnore]
	public readonly NetFloat xVelocity = new NetFloat();

	/// <summary>The speed at which the projectile moves along the Y axis.</summary>
	[XmlIgnore]
	public readonly NetFloat yVelocity = new NetFloat();

	public readonly NetVector2 acceleration = new NetVector2();

	public readonly NetFloat maxVelocity = new NetFloat(-1f);

	public readonly NetColor color = new NetColor(Color.White);

	[XmlIgnore]
	public Queue<Vector2> tail = new Queue<Vector2>();

	public readonly NetInt maxTravelDistance = new NetInt(-1);

	public float travelDistance;

	public readonly NetInt projectileID = new NetInt(-1);

	public readonly NetInt uniqueID = new NetInt(-1);

	public NetFloat height = new NetFloat(0f);

	/// <summary>Whether the projectile damage monsters (true) or players (false).</summary>
	[XmlIgnore]
	public readonly NetBool damagesMonsters = new NetBool();

	[XmlIgnore]
	public readonly NetCharacterRef theOneWhoFiredMe = new NetCharacterRef();

	public readonly NetBool ignoreTravelGracePeriod = new NetBool(value: false);

	public readonly NetBool ignoreLocationCollision = new NetBool();

	public readonly NetBool ignoreObjectCollisions = new NetBool();

	public readonly NetBool ignoreMeleeAttacks = new NetBool(value: false);

	public readonly NetBool ignoreCharacterCollisions = new NetBool(value: false);

	public bool destroyMe;

	public readonly NetFloat startingScale = new NetFloat(1f);

	protected float? _localScale;

	public readonly NetFloat scaleGrow = new NetFloat(0f);

	public NetBool light = new NetBool();

	public bool hasLit;

	[XmlIgnore]
	public string lightSourceId;

	protected float rotation
	{
		get
		{
			if (!this._rotation.HasValue)
			{
				this._rotation = this.startingRotation.Value;
			}
			return this._rotation.Value;
		}
		set
		{
			this._rotation = value;
		}
	}

	public bool IgnoreLocationCollision
	{
		get
		{
			return this.ignoreLocationCollision.Value;
		}
		set
		{
			this.ignoreLocationCollision.Value = value;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();

	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return this.modData.GetForSerialization();
		}
		set
		{
			this.modData.SetFromSerialization(value);
		}
	}

	public NetFields NetFields { get; } = new NetFields("Projectile");

	[XmlIgnore]
	public virtual float localScale
	{
		get
		{
			if (!this._localScale.HasValue)
			{
				this._localScale = this.startingScale.Value;
			}
			return this._localScale.Value;
		}
		set
		{
			this._localScale = value;
		}
	}

	/// <summary>Construct an empty instance.</summary>
	public Projectile()
	{
		this.InitNetFields();
		this.uniqueID.Value = Game1.random.Next();
	}

	/// <summary>Initialize the collection of fields to sync in multiplayer.</summary>
	protected virtual void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.currentTileSheetIndex, "currentTileSheetIndex").AddField(this.position.NetFields, "position.NetFields")
			.AddField(this.tailLength, "tailLength")
			.AddField(this.bouncesLeft, "bouncesLeft")
			.AddField(this.bounceSound, "bounceSound")
			.AddField(this.rotationVelocity, "rotationVelocity")
			.AddField(this.startingRotation, "startingRotation")
			.AddField(this.xVelocity, "xVelocity")
			.AddField(this.yVelocity, "yVelocity")
			.AddField(this.damagesMonsters, "damagesMonsters")
			.AddField(this.theOneWhoFiredMe.NetFields, "theOneWhoFiredMe.NetFields")
			.AddField(this.ignoreLocationCollision, "ignoreLocationCollision")
			.AddField(this.maxTravelDistance, "maxTravelDistance")
			.AddField(this.ignoreTravelGracePeriod, "ignoreTravelGracePeriod")
			.AddField(this.ignoreMeleeAttacks, "ignoreMeleeAttacks")
			.AddField(this.height, "height")
			.AddField(this.startingScale, "startingScale")
			.AddField(this.scaleGrow, "scaleGrow")
			.AddField(this.color, "color")
			.AddField(this.light, "light")
			.AddField(this.itemId, "itemId")
			.AddField(this.projectileID, "projectileID")
			.AddField(this.ignoreObjectCollisions, "ignoreObjectCollisions")
			.AddField(this.acceleration, "acceleration")
			.AddField(this.maxVelocity, "maxVelocity")
			.AddField(this.alpha, "alpha")
			.AddField(this.alphaChange, "alphaChange")
			.AddField(this.boundingBoxWidth, "boundingBoxWidth")
			.AddField(this.ignoreCharacterCollisions, "ignoreCharacterCollisions")
			.AddField(this.uniqueID, "uniqueID")
			.AddField(this.modData, "modData");
	}

	/// <summary>Handle the projectile hitting an obstacle.</summary>
	/// <param name="location">The location containing the projectile.</param>
	/// <param name="target">The target player or monster that was hit, if applicable.</param>
	/// <param name="terrainFeature">The terrain feature that was hit, if applicable.</param>
	private void behaviorOnCollision(GameLocation location, Character target, TerrainFeature terrainFeature)
	{
		bool successfulHit = true;
		if (!(target is Farmer player))
		{
			if (target is NPC npc)
			{
				if (!npc.IsInvisible)
				{
					this.behaviorOnCollisionWithMonster(npc, location);
				}
				else
				{
					successfulHit = false;
				}
			}
			else if (terrainFeature != null)
			{
				this.behaviorOnCollisionWithTerrainFeature(terrainFeature, terrainFeature.Tile, location);
			}
			else
			{
				this.behaviorOnCollisionWithOther(location);
			}
		}
		else
		{
			this.behaviorOnCollisionWithPlayer(location, player);
		}
		if (successfulHit && this.piercesLeft.Value <= 0 && this.hasLit)
		{
			LightSource lightSource = Utility.getLightSource(this.lightSourceId);
			if (lightSource != null)
			{
				lightSource.fadeOut.Value = 3;
			}
		}
	}

	public abstract void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player);

	public abstract void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location);

	public abstract void behaviorOnCollisionWithOther(GameLocation location);

	public abstract void behaviorOnCollisionWithMonster(NPC n, GameLocation location);

	public virtual bool update(GameTime time, GameLocation location)
	{
		if (Game1.isTimePaused)
		{
			return false;
		}
		if (Game1.IsMasterGame && this.hostTimeUntilAttackable > 0f)
		{
			this.hostTimeUntilAttackable -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.hostTimeUntilAttackable <= 0f)
			{
				this.ignoreMeleeAttacks.Value = false;
				this.hostTimeUntilAttackable = -1f;
			}
		}
		if (this.light.Value)
		{
			if (!this.hasLit)
			{
				this.hasLit = true;
				this.lightSourceId = $"{base.GetType().Name}_{Game1.random.Next()}";
				if (location.Equals(Game1.currentLocation))
				{
					Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, this.position.Value + new Vector2(32f, 32f), 1f, new Color(Utility.getOppositeColor(this.color.Value).ToVector4() * this.alpha.Value), LightSource.LightContext.None, 0L, location.NameOrUniqueName));
				}
			}
			else
			{
				LightSource l = Utility.getLightSource(this.lightSourceId);
				if (l != null)
				{
					l.color.A = (byte)(255f * this.alpha.Value);
				}
				Utility.repositionLightSource(this.lightSourceId, this.position.Value + new Vector2(32f, 32f));
			}
		}
		this.alpha.Value += this.alphaChange.Value;
		this.alpha.Value = Utility.Clamp(this.alpha.Value, 0f, 1f);
		this.rotation += this.rotationVelocity.Value;
		this.travelTime += time.ElapsedGameTime.Milliseconds;
		if (this.scaleGrow.Value != 0f)
		{
			this.localScale += this.scaleGrow.Value;
		}
		Vector2 old_position = this.position.Value;
		this.updatePosition(time);
		this.updateTail(time);
		this.travelDistance += (old_position - this.position.Value).Length();
		if (this.maxTravelDistance.Value >= 0)
		{
			if (this.travelDistance > (float)(this.maxTravelDistance.Value - 128))
			{
				this.alpha.Value = ((float)this.maxTravelDistance.Value - this.travelDistance) / 128f;
			}
			if (this.travelDistance >= (float)this.maxTravelDistance.Value)
			{
				if (this.hasLit)
				{
					Utility.removeLightSource(this.lightSourceId);
				}
				return true;
			}
		}
		if ((this.travelTime > 100 || this.ignoreTravelGracePeriod.Value) && this.isColliding(location, out var target, out var terrainFeature) && this.ShouldApplyCollisionLocally(location))
		{
			if (this.bouncesLeft.Value <= 0 || target != null)
			{
				this.behaviorOnCollision(location, target, terrainFeature);
				return this.piercesLeft.Value <= 0;
			}
			this.bouncesLeft.Value--;
			bool[] array = Utility.horizontalOrVerticalCollisionDirections(this.getBoundingBox(), this.theOneWhoFiredMe.Get(location), projectile: true);
			if (array[0])
			{
				this.xVelocity.Value = 0f - this.xVelocity.Value;
			}
			if (array[1])
			{
				this.yVelocity.Value = 0f - this.yVelocity.Value;
			}
			if (!string.IsNullOrEmpty(this.bounceSound.Value))
			{
				location?.playSound(this.bounceSound.Value);
			}
		}
		return false;
	}

	/// <summary>Get whether this projectile's <see cref="M:StardewValley.Projectiles.Projectile.behaviorOnCollision(StardewValley.GameLocation,StardewValley.Character,StardewValley.TerrainFeatures.TerrainFeature)" /> should be called for the local player.</summary>
	/// <param name="location">The location containing the projectile.</param>
	protected virtual bool ShouldApplyCollisionLocally(GameLocation location)
	{
		if (this.theOneWhoFiredMe.Get(location) is Farmer firedBy && firedBy != Game1.player)
		{
			if (Game1.IsMasterGame)
			{
				return firedBy.currentLocation != location;
			}
			return false;
		}
		return true;
	}

	protected virtual void updateTail(GameTime time)
	{
		this.tailCounter -= time.ElapsedGameTime.Milliseconds;
		if (this.tailCounter <= 0)
		{
			this.tailCounter = 50;
			this.tail.Enqueue(this.position.Value);
			if (this.tail.Count > this.tailLength.Value)
			{
				this.tail.Dequeue();
			}
		}
	}

	/// <summary>Get whether the projectile is colliding with a wall or target.</summary>
	/// <param name="location">The location containing the projectile.</param>
	/// <param name="target">The target that was hit, if applicable.</param>
	/// <param name="terrainFeature">The terrain feature that was hit, if applicable.</param>
	public virtual bool isColliding(GameLocation location, out Character target, out TerrainFeature terrainFeature)
	{
		target = null;
		terrainFeature = null;
		Rectangle boundingBox = this.getBoundingBox();
		if (!this.ignoreCharacterCollisions.Value)
		{
			if (this.damagesMonsters.Value)
			{
				Character npc = location.doesPositionCollideWithCharacter(boundingBox);
				if (npc != null)
				{
					if (npc is NPC && (npc as NPC).IsInvisible)
					{
						return false;
					}
					target = npc;
					return true;
				}
			}
			else if (Game1.player.currentLocation == location && Game1.player.GetBoundingBox().Intersects(boundingBox))
			{
				target = Game1.player;
				return true;
			}
		}
		foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(boundingBox))
		{
			if (location.terrainFeatures.TryGetValue(tile, out var feature) && !feature.isPassable())
			{
				terrainFeature = feature;
				return true;
			}
		}
		if (!location.isTileOnMap(this.position.Value / 64f) || (!this.ignoreLocationCollision.Value && location.isCollidingPosition(boundingBox, Game1.viewport, isFarmer: false, 0, glider: true, this.theOneWhoFiredMe.Get(location), pathfinding: false, projectile: true)))
		{
			return true;
		}
		return false;
	}

	public abstract void updatePosition(GameTime time);

	public virtual Rectangle getBoundingBox()
	{
		Vector2 pos = this.position.Value;
		int damageSize = this.boundingBoxWidth.Value + (this.damagesMonsters.Value ? 8 : 0);
		float current_scale = this.localScale;
		damageSize = (int)((float)damageSize * current_scale);
		return new Rectangle((int)pos.X + 32 - damageSize / 2, (int)pos.Y + 32 - damageSize / 2, damageSize, damageSize);
	}

	public virtual void draw(SpriteBatch b)
	{
		float current_scale = 4f * this.localScale;
		Texture2D texture = this.GetTexture();
		Rectangle sourceRect = this.GetSourceRect();
		Vector2 pixelPosition = this.position.Value;
		b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(0f, 0f - this.height.Value) + new Vector2(32f, 32f)), sourceRect, this.color.Value * this.alpha.Value, this.rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f);
		if (this.height.Value > 0f)
		{
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), Game1.shadowTexture.Bounds, Color.White * this.alpha.Value * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, (pixelPosition.Y - 1f) / 10000f);
		}
		float tailAlpha = this.alpha.Value;
		for (int i = this.tail.Count - 1; i >= 0; i--)
		{
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == this.tail.Count - 1) ? pixelPosition : this.tail.ElementAt(i + 1), this.tail.ElementAt(i), (float)this.tailCounter / 50f) + new Vector2(0f, 0f - this.height.Value) + new Vector2(32f, 32f)), sourceRect, this.color.Value * tailAlpha, this.rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (pixelPosition.Y - (float)(this.tail.Count - i) + 96f) / 10000f);
			tailAlpha -= 1f / (float)this.tail.Count;
			current_scale = 0.8f * (float)(4 - 4 / (i + 4));
		}
	}

	/// <summary>Get the texture to draw for the projectile.</summary>
	public Texture2D GetTexture()
	{
		if (this.itemId.Value == null)
		{
			return Projectile.projectileSheet;
		}
		return ItemRegistry.GetDataOrErrorItem(this.itemId.Value).GetTexture();
	}

	/// <summary>Get the source rectangle to draw for the projectile.</summary>
	public Rectangle GetSourceRect()
	{
		if (this.itemId.Value == null)
		{
			return Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, this.currentTileSheetIndex.Value, 16, 16);
		}
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(this.itemId.Value);
		switch (this.itemId.Value)
		{
		case "(O)388":
		case "(O)378":
		case "(O)390":
		case "(O)380":
		case "(O)384":
		case "(O)382":
		case "(O)386":
			return data.GetSourceRect(1);
		default:
			return data.GetSourceRect();
		}
	}
}
