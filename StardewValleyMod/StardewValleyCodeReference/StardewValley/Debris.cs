using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley;

public class Debris : INetObject<NetFields>
{
	public enum DebrisType
	{
		/// <summary>The small 'chunks' that appear when hitting a tree with wood.</summary>
		CHUNKS = 0,
		LETTERS = 1,
		ARCHAEOLOGY = 3,
		OBJECT = 4,
		/// <summary>Sprites broken up into square chunks (i.e. the crumbs when you eat).</summary>
		SPRITECHUNKS = 5,
		RESOURCE = 6,
		NUMBERS = 7
	}

	public const int copperDebris = 0;

	public const int ironDebris = 2;

	public const int coalDebris = 4;

	public const int goldDebris = 6;

	public const int coinsDebris = 8;

	public const int iridiumDebris = 10;

	public const int woodDebris = 12;

	public const int stoneDebris = 14;

	public const int bigStoneDebris = 32;

	public const int bigWoodDebris = 34;

	public const int timesToBounce = 2;

	public const float gravity = 0.4f;

	public const float timeToWaitBeforeRemoval = 600f;

	public const int marginForChunkPickup = 64;

	public const int white = 10000;

	public const int green = 100001;

	public const int blue = 100002;

	public const int red = 100003;

	public const int yellow = 100004;

	public const int black = 100005;

	public const int charcoal = 100007;

	public const int gray = 100006;

	private float relativeXPosition;

	private readonly NetObjectShrinkList<Chunk> chunks = new NetObjectShrinkList<Chunk>();

	public readonly NetInt chunkType = new NetInt();

	public readonly NetInt sizeOfSourceRectSquares = new NetInt(8);

	private readonly NetInt netItemQuality = new NetInt();

	private readonly NetInt netChunkFinalYLevel = new NetInt();

	private readonly NetInt netChunkFinalYTarget = new NetInt();

	public float timeSinceDoneBouncing;

	public readonly NetFloat scale = new NetFloat(1f).Interpolated(interpolate: true, wait: true);

	protected NetBool _chunksMoveTowardsPlayer = new NetBool(value: false).Interpolated(interpolate: false, wait: false);

	public readonly NetLong DroppedByPlayerID = new NetLong().Interpolated(interpolate: false, wait: false);

	private bool movingUp;

	public readonly NetBool floppingFish = new NetBool();

	public bool isFishable;

	public bool movingFinalYLevel;

	public readonly NetEnum<DebrisType> debrisType = new NetEnum<DebrisType>(DebrisType.CHUNKS);

	/// <summary>Whether the debris is in water and will sink soon.</summary>
	public readonly NetBool isSinking = new NetBool(value: false);

	public readonly NetString debrisMessage = new NetString("");

	public readonly NetColor nonSpriteChunkColor = new NetColor(Color.White);

	public readonly NetColor chunksColor = new NetColor();

	private float animationTimer;

	private int timeBeforeReturnToDroppingPlayer = 1200;

	public readonly NetString spriteChunkSheetName = new NetString();

	private Texture2D _spriteChunkSheet;

	public readonly NetString itemId = new NetString();

	private readonly NetRef<Item> netItem = new NetRef<Item>();

	public Character toHover;

	public readonly NetFarmerRef player = new NetFarmerRef();

	public int itemQuality
	{
		get
		{
			return this.netItemQuality.Value;
		}
		set
		{
			this.netItemQuality.Value = value;
		}
	}

	public int chunkFinalYLevel
	{
		get
		{
			return this.netChunkFinalYLevel.Value;
		}
		set
		{
			this.netChunkFinalYLevel.Value = value;
		}
	}

	public int chunkFinalYTarget
	{
		get
		{
			return this.netChunkFinalYTarget.Value;
		}
		set
		{
			this.netChunkFinalYTarget.Value = value;
		}
	}

	public bool chunksMoveTowardPlayer
	{
		get
		{
			return this._chunksMoveTowardsPlayer.Value;
		}
		set
		{
			this._chunksMoveTowardsPlayer.Value = value;
		}
	}

	public Texture2D spriteChunkSheet
	{
		get
		{
			if (this._spriteChunkSheet == null && this.spriteChunkSheetName.Value != null)
			{
				this._spriteChunkSheet = Game1.content.Load<Texture2D>(this.spriteChunkSheetName.Value);
			}
			return this._spriteChunkSheet;
		}
	}

	public Item item
	{
		get
		{
			return this.netItem.Value;
		}
		set
		{
			this.netItem.Value = value;
		}
	}

	public NetFields NetFields { get; } = new NetFields("Debris");

	public NetObjectShrinkList<Chunk> Chunks => this.chunks;

	public Debris()
	{
		this.InitNetFields();
	}

	public virtual void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.chunks, "chunks").AddField(this.chunkType, "chunkType")
			.AddField(this.sizeOfSourceRectSquares, "sizeOfSourceRectSquares")
			.AddField(this.netItemQuality, "netItemQuality")
			.AddField(this.netChunkFinalYLevel, "netChunkFinalYLevel")
			.AddField(this.netChunkFinalYTarget, "netChunkFinalYTarget")
			.AddField(this.scale, "scale")
			.AddField(this.floppingFish, "floppingFish")
			.AddField(this.debrisType, "debrisType")
			.AddField(this.isSinking, "isSinking")
			.AddField(this.debrisMessage, "debrisMessage")
			.AddField(this.nonSpriteChunkColor, "nonSpriteChunkColor")
			.AddField(this.chunksColor, "chunksColor")
			.AddField(this.spriteChunkSheetName, "spriteChunkSheetName")
			.AddField(this.netItem, "netItem")
			.AddField(this.player.NetFields, "player.NetFields")
			.AddField(this.DroppedByPlayerID, "DroppedByPlayerID")
			.AddField(this._chunksMoveTowardsPlayer, "_chunksMoveTowardsPlayer")
			.AddField(this.itemId, "itemId");
		this.player.Delayed(interpolationWait: false);
	}

	/// <summary>Construct an instance for resource/item debris.</summary>
	public Debris(int debris_type, Vector2 debrisOrigin, Vector2 playerPosition)
		: this(debris_type, 1, debrisOrigin, playerPosition)
	{
	}

	/// <summary>Construct an instance for resource/item type debris.</summary>
	public Debris(int resource_type, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f)
		: this()
	{
		this.InitializeResource(resource_type);
		this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, velocityMultiplyer);
	}

	/// <summary>Construct an instance for cosmetic "chunks".</summary>
	public Debris(int debrisType, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, Color? color = null)
		: this()
	{
		this.debrisType.Value = DebrisType.CHUNKS;
		this.chunkType.Value = debrisType;
		this.chunksColor.Value = color ?? Debris.getColorForDebris(debrisType);
		this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition);
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(string item_id, Vector2 debrisOrigin, Vector2 playerPosition)
		: this(item_id, 1, debrisOrigin, playerPosition)
	{
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(string item_id, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f)
		: this()
	{
		this.InitializeItem(item_id);
		this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition, velocityMultiplyer);
	}

	public virtual void InitializeItem(string item_id)
	{
		if (this.debrisType.Value == DebrisType.CHUNKS)
		{
			this.debrisType.Value = DebrisType.OBJECT;
		}
		this.itemId.Value = item_id;
		ParsedItemData data = ItemRegistry.GetData(this.itemId.Value);
		if (this.item != null)
		{
			return;
		}
		if (data.HasTypeObject())
		{
			this.floppingFish.Value = data.Category == -4 && data.InternalName != "Mussel";
			this.isFishable = data.ObjectType == "Fish";
			if (data.ObjectType == "Arch")
			{
				this.debrisType.Value = DebrisType.ARCHAEOLOGY;
			}
		}
		else
		{
			this.item = ItemRegistry.Create(this.itemId.Value);
		}
	}

	public virtual void InitializeResource(int item_id)
	{
		this.debrisType.Value = DebrisType.OBJECT;
		switch (item_id)
		{
		case 0:
		case 378:
			this.itemId.Value = "(O)378";
			this.debrisType.Value = DebrisType.RESOURCE;
			break;
		case 2:
		case 380:
			this.itemId.Value = "(O)380";
			this.debrisType.Value = DebrisType.RESOURCE;
			break;
		case 6:
		case 384:
			this.itemId.Value = "(O)384";
			this.debrisType.Value = DebrisType.RESOURCE;
			break;
		case 10:
		case 386:
			this.itemId.Value = "(O)386";
			this.debrisType.Value = DebrisType.RESOURCE;
			break;
		case 12:
		case 388:
			this.itemId.Value = "(O)388";
			this.debrisType.Value = DebrisType.RESOURCE;
			break;
		case 14:
		case 390:
			this.itemId.Value = "(O)390";
			this.debrisType.Value = DebrisType.RESOURCE;
			break;
		case 4:
		case 382:
			this.itemId.Value = "(O)382";
			this.debrisType.Value = DebrisType.RESOURCE;
			break;
		default:
			this.itemId.Value = "(O)" + item_id;
			break;
		}
		if (this.itemId.Value != null)
		{
			this.InitializeItem(this.itemId.Value);
		}
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(Item item, Vector2 debrisOrigin)
		: this()
	{
		this.item = item;
		item.resetState();
		this.InitializeItem(item.QualifiedItemId);
		this.InitializeChunks(1, debrisOrigin, Utility.PointToVector2(Game1.player.StandingPixel));
	}

	/// <summary>Construct an instance for floating items.</summary>
	public Debris(Item item, Vector2 debrisOrigin, Vector2 targetLocation)
		: this()
	{
		this.item = item;
		item.resetState();
		this.InitializeItem(item.QualifiedItemId);
		this.InitializeChunks(1, debrisOrigin, targetLocation);
	}

	/// <summary>Construct an instance for numbers.</summary>
	public Debris(int number, Vector2 debrisOrigin, Color messageColor, float scale, Character toHover)
		: this()
	{
		this.chunkType.Value = number;
		this.debrisType.Value = DebrisType.NUMBERS;
		this.nonSpriteChunkColor.Value = messageColor;
		this.InitializeChunks(1, debrisOrigin, Game1.player.Position);
		this.chunks[0].scale = scale;
		this.toHover = toHover;
		this.chunks[0].xVelocity.Value = Game1.random.Next(-1, 2);
		this.updateHoverPosition(this.chunks[0]);
	}

	/// <summary>Construct an instance for letters.</summary>
	public Debris(string message, int numberOfChunks, Vector2 debrisOrigin, Color messageColor, float scale, float rotation)
		: this()
	{
		this.debrisType.Value = DebrisType.LETTERS;
		this.debrisMessage.Value = message;
		this.nonSpriteChunkColor.Value = messageColor;
		this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position);
		this.chunks[0].rotation = rotation;
		this.chunks[0].scale = scale;
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, int numberOfChunks, Vector2 debrisOrigin)
		: this()
	{
		this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position);
		this.debrisType.Value = DebrisType.SPRITECHUNKS;
		this.spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < this.chunks.Count; i++)
		{
			Chunk chunk = this.chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(0, 56);
			chunk.ySpriteSheet.Value = Game1.random.Next(0, 88);
			chunk.scale = 1f;
		}
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin)
		: this()
	{
		this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position);
		this.debrisType.Value = DebrisType.SPRITECHUNKS;
		this.spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < this.chunks.Count; i++)
		{
			Chunk chunk = this.chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
			chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
			chunk.scale = 1f;
		}
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, int sizeOfSourceRectSquares)
		: this()
	{
		this.InitializeChunks(numberOfChunks, debrisOrigin, Game1.player.Position, 0.6f);
		this.sizeOfSourceRectSquares.Value = sizeOfSourceRectSquares;
		this.debrisType.Value = DebrisType.SPRITECHUNKS;
		this.spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < this.chunks.Count; i++)
		{
			Chunk chunk = this.chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.X;
			chunk.ySpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.Y;
			chunk.rotationVelocity = (Game1.random.NextBool() ? ((float)(Math.PI / (double)Game1.random.Next(-32, -16))) : ((float)(Math.PI / (double)Game1.random.Next(16, 32))));
			chunk.xVelocity.Value *= 1.2f;
			chunk.yVelocity.Value *= 1.2f;
			chunk.scale = 4f;
		}
	}

	/// <summary>Construct an instance for sprite chunks.</summary>
	public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel)
		: this()
	{
		this.InitializeChunks(numberOfChunks, debrisOrigin, playerPosition);
		this.debrisType.Value = DebrisType.SPRITECHUNKS;
		this.spriteChunkSheetName.Value = spriteSheet;
		for (int i = 0; i < this.chunks.Count; i++)
		{
			Chunk chunk = this.chunks[i];
			chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
			chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
			chunk.scale = 1f;
		}
		this.chunkFinalYLevel = groundLevel;
	}

	public virtual bool isEssentialItem()
	{
		if (this.itemId.Value == "(O)73" || this.item?.QualifiedItemId == "(O)73")
		{
			return true;
		}
		if (this.item != null && !this.item.canBeTrashed())
		{
			return true;
		}
		return false;
	}

	public virtual bool collect(Farmer farmer, Chunk chunk = null)
	{
		if (this.debrisType.Value == DebrisType.ARCHAEOLOGY)
		{
			Game1.farmerFindsArtifact(this.itemId.Value);
		}
		else if (this.item != null)
		{
			Item tmpItem = this.item;
			this.item = null;
			if (!farmer.addItemToInventoryBool(tmpItem))
			{
				this.item = tmpItem;
				return false;
			}
		}
		else if ((this.debrisType.Value != DebrisType.CHUNKS || this.chunkType.Value != 8) && !farmer.addItemToInventoryBool(ItemRegistry.Create(this.itemId.Value, 1, this.itemQuality)))
		{
			return false;
		}
		return true;
	}

	public static Color getColorForDebris(int type)
	{
		return type switch
		{
			12 => new Color(170, 106, 46), 
			100006 => Color.Gray, 
			100001 => Color.LightGreen, 
			100003 => Color.Red, 
			100004 => Color.Yellow, 
			100005 => Color.Black, 
			100007 => Color.DimGray, 
			100002 => Color.LightBlue, 
			_ => Color.White, 
		};
	}

	/// <summary>Initialize the chunks, called from all constructors.</summary>
	public void InitializeChunks(int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer = 1f)
	{
		if (this.itemId.Value != null || this.chunkType.Value != -1)
		{
			playerPosition -= (playerPosition - debrisOrigin) * 2f;
		}
		int minXVelocity;
		int maxXVelocity;
		int minYVelocity;
		int maxYVelocity;
		if (playerPosition.Y >= debrisOrigin.Y - 32f && playerPosition.Y <= debrisOrigin.Y + 32f)
		{
			this.chunkFinalYLevel = (int)debrisOrigin.Y - 32;
			minYVelocity = 250;
			maxYVelocity = 300;
			if (playerPosition.X < debrisOrigin.X)
			{
				minXVelocity = 20;
				maxXVelocity = 110;
			}
			else
			{
				minXVelocity = -110;
				maxXVelocity = -20;
			}
		}
		else if (playerPosition.Y < debrisOrigin.Y - 32f)
		{
			this.chunkFinalYLevel = (int)debrisOrigin.Y + (int)(32f * velocityMultiplyer);
			minYVelocity = 180;
			maxYVelocity = 230;
			minXVelocity = -50;
			maxXVelocity = 50;
		}
		else
		{
			this.movingFinalYLevel = true;
			this.chunkFinalYLevel = (int)debrisOrigin.Y - 1;
			this.chunkFinalYTarget = (int)debrisOrigin.Y - (int)(96f * velocityMultiplyer);
			this.movingUp = true;
			minYVelocity = 350;
			maxYVelocity = 400;
			minXVelocity = -50;
			maxXVelocity = 50;
		}
		debrisOrigin.X -= 32f;
		debrisOrigin.Y -= 32f;
		minXVelocity = (int)((float)minXVelocity * velocityMultiplyer);
		maxXVelocity = (int)((float)maxXVelocity * velocityMultiplyer);
		minYVelocity = (int)((float)minYVelocity * velocityMultiplyer);
		maxYVelocity = (int)((float)maxYVelocity * velocityMultiplyer);
		for (int i = 0; i < numberOfChunks; i++)
		{
			this.chunks.Add(new Chunk(debrisOrigin, (float)Game1.recentMultiplayerRandom.Next(minXVelocity, maxXVelocity) / 40f, (float)Game1.recentMultiplayerRandom.Next(minYVelocity, maxYVelocity) / 40f, Game1.recentMultiplayerRandom.Next(0, 2)));
		}
	}

	private Vector2 approximatePosition()
	{
		Vector2 total = default(Vector2);
		foreach (Chunk chunk in this.Chunks)
		{
			total += chunk.position.Value;
		}
		return total / this.Chunks.Count;
	}

	private bool playerInRange(Vector2 position, Farmer farmer)
	{
		if (this.isEssentialItem())
		{
			return true;
		}
		int applied_magnetic_radius = farmer.GetAppliedMagneticRadius();
		Point playerPixel = farmer.StandingPixel;
		if (Math.Abs(position.X + 32f - (float)playerPixel.X) <= (float)applied_magnetic_radius)
		{
			return Math.Abs(position.Y + 32f - (float)playerPixel.Y) <= (float)applied_magnetic_radius;
		}
		return false;
	}

	private Farmer findBestPlayer(GameLocation location)
	{
		if (location?.IsTemporary ?? false)
		{
			return Game1.player;
		}
		Vector2 position = this.approximatePosition();
		float bestDistance = float.MaxValue;
		Farmer bestFarmer = null;
		foreach (Farmer farmer in location.farmers)
		{
			if ((farmer.UniqueMultiplayerID != this.DroppedByPlayerID.Value || bestFarmer == null) && this.playerInRange(position, farmer))
			{
				float distance = (farmer.Position - position).LengthSquared();
				if (distance < bestDistance || (bestFarmer != null && bestFarmer.UniqueMultiplayerID == this.DroppedByPlayerID.Value))
				{
					bestFarmer = farmer;
					bestDistance = distance;
				}
			}
		}
		return bestFarmer;
	}

	public bool shouldControlThis(GameLocation location)
	{
		if (!Game1.IsMasterGame)
		{
			return location?.IsTemporary ?? false;
		}
		return true;
	}

	public bool updateChunks(GameTime time, GameLocation location)
	{
		if (this.chunks.Count == 0)
		{
			return true;
		}
		this.timeSinceDoneBouncing += time.ElapsedGameTime.Milliseconds;
		if (this.timeSinceDoneBouncing >= (this.floppingFish.Value ? 2500f : ((this.debrisType.Value == DebrisType.SPRITECHUNKS || this.debrisType.Value == DebrisType.NUMBERS) ? 1800f : 600f)))
		{
			switch (this.debrisType.Value)
			{
			case DebrisType.LETTERS:
			case DebrisType.SPRITECHUNKS:
			case DebrisType.NUMBERS:
				return true;
			case DebrisType.CHUNKS:
				if (this.chunkType.Value != 8)
				{
					return true;
				}
				this.chunksMoveTowardPlayer = true;
				break;
			case DebrisType.ARCHAEOLOGY:
			case DebrisType.OBJECT:
			case DebrisType.RESOURCE:
				this.chunksMoveTowardPlayer = true;
				break;
			}
			this.timeSinceDoneBouncing = 0f;
		}
		if (!location.farmers.Any() && !location.IsTemporary)
		{
			return false;
		}
		Vector2 position = this.approximatePosition();
		Farmer farmer = this.player.Value;
		if (this.isEssentialItem() && this.shouldControlThis(location) && farmer == null)
		{
			farmer = this.findBestPlayer(location);
		}
		if (this.chunksMoveTowardPlayer)
		{
			if (this.timeBeforeReturnToDroppingPlayer > 0)
			{
				this.timeBeforeReturnToDroppingPlayer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (!this.isEssentialItem())
			{
				if (this.player.Value != null && this.player.Value == Game1.player && !this.playerInRange(position, this.player.Value))
				{
					this.player.Value = null;
					farmer = null;
				}
				if (this.shouldControlThis(location))
				{
					if (this.player.Value != null && this.player.Value.currentLocation != location)
					{
						this.player.Value = null;
						farmer = null;
					}
					if (farmer == null)
					{
						farmer = this.findBestPlayer(location);
					}
				}
				if (farmer != null && this.timeBeforeReturnToDroppingPlayer > 0 && farmer.UniqueMultiplayerID == this.DroppedByPlayerID.Value)
				{
					farmer = null;
				}
			}
		}
		bool anyCouldMove = false;
		for (int i = this.chunks.Count - 1; i >= 0; i--)
		{
			Chunk chunk = this.chunks[i];
			chunk.position.UpdateExtrapolation(chunk.getSpeed());
			if (chunk.alpha > 0.1f && (this.debrisType.Value == DebrisType.SPRITECHUNKS || this.debrisType.Value == DebrisType.NUMBERS) && this.timeSinceDoneBouncing > 600f)
			{
				chunk.alpha = (1800f - this.timeSinceDoneBouncing) / 1000f;
			}
			if (chunk.position.X < -128f || chunk.position.Y < -64f || chunk.position.X >= (float)(location.map.DisplayWidth + 64) || chunk.position.Y >= (float)(location.map.DisplayHeight + 64))
			{
				this.chunks.RemoveAt(i);
			}
			else
			{
				if (this.item?.QualifiedItemId == "(O)GoldCoin")
				{
					this.animationTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
					if (this.animationTimer > 700f)
					{
						this.animationTimer = 0f;
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), 100f, 6, 1, Utility.getRandomPositionInThisRectangle(new Rectangle((int)chunk.position.X + 32 - 4, (int)chunk.position.Y + 32 - 4, 32, 28), Game1.random), flicker: false, flipped: false, ((float)(this.chunkFinalYLevel + 64 + 8) + (chunk.position.X + 1f) / 10000f) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
					}
				}
				bool canMoveTowardPlayer = farmer != null;
				if (canMoveTowardPlayer)
				{
					switch (this.debrisType.Value)
					{
					case DebrisType.ARCHAEOLOGY:
					case DebrisType.OBJECT:
						if (this.item != null)
						{
							canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(this.item);
							break;
						}
						canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(this.itemId.Value, 1, this.itemQuality);
						if (this.itemId.Value == "(O)102" && farmer.hasMenuOpen.Value)
						{
							canMoveTowardPlayer = false;
						}
						break;
					case DebrisType.RESOURCE:
						canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(this.itemId.Value, 1);
						break;
					default:
						canMoveTowardPlayer = true;
						break;
					}
					anyCouldMove = anyCouldMove || canMoveTowardPlayer;
					if (canMoveTowardPlayer && this.shouldControlThis(location))
					{
						this.player.Value = farmer;
					}
				}
				if ((this.chunksMoveTowardPlayer || this.isFishable) && canMoveTowardPlayer && this.player.Value != null)
				{
					if (this.player.Value.IsLocalPlayer)
					{
						if (chunk.position.X < this.player.Value.Position.X - 12f)
						{
							chunk.xVelocity.Value = Math.Min(chunk.xVelocity.Value + 0.8f, 8f);
						}
						else if (chunk.position.X > this.player.Value.Position.X + 12f)
						{
							chunk.xVelocity.Value = Math.Max(chunk.xVelocity.Value - 0.8f, -8f);
						}
						int playerStandingY = this.player.Value.StandingPixel.Y;
						if (chunk.position.Y + 32f < (float)(playerStandingY - 12))
						{
							chunk.yVelocity.Value = Math.Max(chunk.yVelocity.Value - 0.8f, -8f);
						}
						else if (chunk.position.Y + 32f > (float)(playerStandingY + 12))
						{
							chunk.yVelocity.Value = Math.Min(chunk.yVelocity.Value + 0.8f, 8f);
						}
						chunk.position.X += chunk.xVelocity.Value;
						chunk.position.Y -= chunk.yVelocity.Value;
						Point playerPixel = this.player.Value.StandingPixel;
						if (Math.Abs(chunk.position.X + 32f - (float)playerPixel.X) <= 64f && Math.Abs(chunk.position.Y + 32f - (float)playerPixel.Y) <= 64f)
						{
							Item old = this.item;
							if (this.collect(this.player.Value, chunk))
							{
								if (Game1.debrisSoundInterval <= 0f)
								{
									Game1.debrisSoundInterval = 10f;
									if (old?.QualifiedItemId != "(O)73" && this.itemId.Value != "(O)73")
									{
										location.localSound("coin");
									}
								}
								this.chunks.RemoveAt(i);
							}
						}
					}
				}
				else
				{
					if (this.debrisType.Value == DebrisType.NUMBERS)
					{
						this.updateHoverPosition(chunk);
					}
					chunk.position.X += chunk.xVelocity.Value;
					chunk.position.Y -= chunk.yVelocity.Value;
					if (this.movingFinalYLevel)
					{
						this.chunkFinalYLevel -= (int)Math.Ceiling(chunk.yVelocity.Value / 2f);
						if (this.chunkFinalYLevel <= this.chunkFinalYTarget)
						{
							this.chunkFinalYLevel = this.chunkFinalYTarget;
							this.movingFinalYLevel = false;
						}
					}
					if (chunk.bounces <= (this.floppingFish.Value ? 65 : 2))
					{
						if (this.debrisType.Value == DebrisType.SPRITECHUNKS)
						{
							chunk.yVelocity.Value -= 0.25f;
						}
						else
						{
							chunk.yVelocity.Value -= 0.4f;
						}
					}
					bool destroyThisChunk = false;
					if (chunk.position.Y >= (float)this.chunkFinalYLevel && chunk.hasPassedRestingLineOnce.Value)
					{
						Vector2 chunkTile = new Vector2((int)((chunk.position.X + 32f) / 64f), (int)((chunk.position.Y + 32f) / 64f));
						bool stillBouncing = chunk.bounces <= (this.floppingFish.Value ? 65 : 2);
						if (stillBouncing)
						{
							Point tile_point = new Point((int)chunk.position.X / 64, this.chunkFinalYLevel / 64);
							if (Game1.currentLocation is IslandNorth && (this.debrisType.Value == DebrisType.ARCHAEOLOGY || this.debrisType.Value == DebrisType.OBJECT || this.debrisType.Value == DebrisType.RESOURCE || this.debrisType.Value == DebrisType.CHUNKS) && Game1.currentLocation.isTileOnMap(tile_point.X, tile_point.Y) && !Game1.currentLocation.hasTileAt(tile_point, "Back"))
							{
								this.chunkFinalYLevel += 48;
							}
							chunk.bounces++;
							if (this.floppingFish.Value)
							{
								chunk.yVelocity.Value = Math.Abs(chunk.yVelocity.Value) * ((this.movingUp && chunk.bounces < 2) ? 0.6f : 0.9f);
								chunk.xVelocity.Value = (float)Game1.random.Next(-250, 250) / 100f;
							}
							else
							{
								chunk.yVelocity.Value = Math.Abs(chunk.yVelocity.Value * 2f / 3f);
								chunk.rotationVelocity = (Game1.random.NextBool() ? (chunk.rotationVelocity / 2f) : ((0f - chunk.rotationVelocity) * 2f / 3f));
								chunk.xVelocity.Value -= chunk.xVelocity.Value / 2f;
							}
							if (this.debrisType.Value != DebrisType.LETTERS && this.debrisType.Value != DebrisType.SPRITECHUNKS && this.debrisType.Value != DebrisType.NUMBERS && location.doesTileSinkDebris((int)chunkTile.X, (int)chunkTile.Y, this.debrisType.Value))
							{
								destroyThisChunk = location.sinkDebris(this, chunkTile, chunk.position.Value);
								if (this.isSinking.Value)
								{
									chunk.xVelocity.Value = 0f;
									chunk.yVelocity.Value = 0f;
								}
							}
							else if (this.debrisType.Value != DebrisType.LETTERS && this.debrisType.Value != DebrisType.NUMBERS && this.debrisType.Value != DebrisType.SPRITECHUNKS && (this.debrisType.Value != DebrisType.CHUNKS || this.chunkType.Value == 8) && this.shouldControlThis(location))
							{
								location.playSound("shiny4");
							}
						}
						if (this.isSinking.Value)
						{
							if (!stillBouncing)
							{
								chunk.bob = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.25 + (double)(position.X / 32f)) * 4f;
							}
							chunk.sinkTimer.Value -= time.ElapsedGameTime.Milliseconds;
							if (chunk.sinkTimer.Value <= 0)
							{
								destroyThisChunk = location.sinkDebris(this, chunkTile, chunk.position.Value);
							}
						}
					}
					int tile_x = (int)((chunk.position.X + 32f) / 64f);
					int tile_y = (int)((chunk.position.Y + 32f) / 64f);
					if ((!chunk.hitWall && location.Map.RequireLayer("Buildings").Tiles[tile_x, tile_y] != null && location.doesTileHaveProperty(tile_x, tile_y, "Passable", "Buildings") == null) || location.Map.RequireLayer("Back").Tiles[tile_x, tile_y] == null)
					{
						chunk.xVelocity.Value = 0f - chunk.xVelocity.Value;
						chunk.hitWall = true;
					}
					if (chunk.position.Y < (float)this.chunkFinalYLevel)
					{
						chunk.hasPassedRestingLineOnce.Value = true;
					}
					if (chunk.bounces > (this.floppingFish.Value ? 65 : 2))
					{
						chunk.yVelocity.Value = 0f;
						chunk.xVelocity.Value = 0f;
						chunk.rotationVelocity = 0f;
					}
					chunk.rotation += chunk.rotationVelocity;
					if (destroyThisChunk)
					{
						this.chunks.RemoveAt(i);
					}
				}
			}
		}
		if (!anyCouldMove && this.shouldControlThis(location))
		{
			this.player.Value = null;
		}
		if (this.chunks.Count == 0)
		{
			return true;
		}
		return false;
	}

	public void updateHoverPosition(Chunk chunk)
	{
		if (this.toHover != null)
		{
			this.relativeXPosition += chunk.xVelocity.Value;
			chunk.position.X = this.toHover.Position.X + 32f + this.relativeXPosition;
			chunk.scale = Math.Min(2f, Math.Max(1f, 0.9f + Math.Abs(chunk.position.Y - (float)this.chunkFinalYLevel) / 128f));
			this.chunkFinalYLevel = this.toHover.StandingPixel.Y + 8;
			if (this.timeSinceDoneBouncing > 250f)
			{
				chunk.alpha = Math.Max(0f, chunk.alpha - 0.033f);
			}
			if (!(this.toHover is Farmer) && !this.nonSpriteChunkColor.Equals(Color.Yellow) && !this.nonSpriteChunkColor.Equals(Color.Green))
			{
				this.nonSpriteChunkColor.R = (byte)Math.Max(Math.Min(255, 200 + this.chunkType.Value), Math.Min(Math.Min(255, 220 + this.chunkType.Value), 400.0 * Math.Sin((double)this.timeSinceDoneBouncing / (Math.PI * 256.0) + Math.PI / 12.0)));
				this.nonSpriteChunkColor.G = (byte)Math.Max(150 - this.chunkType.Value, Math.Min(255 - this.chunkType.Value, (this.nonSpriteChunkColor.R > 220) ? (300.0 * Math.Sin((double)this.timeSinceDoneBouncing / (Math.PI * 256.0) + Math.PI / 12.0)) : 0.0));
				this.nonSpriteChunkColor.B = (byte)Math.Max(0, Math.Min(255, (this.nonSpriteChunkColor.G > 200) ? (this.nonSpriteChunkColor.G - 20) : 0));
			}
		}
	}

	public static string getNameOfDebrisTypeFromIntId(int id)
	{
		switch (id)
		{
		case 0:
		case 1:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.621");
		case 2:
		case 3:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.622");
		case 4:
		case 5:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.623");
		case 6:
		case 7:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.624");
		case 8:
		case 9:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.625");
		case 10:
		case 11:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.626");
		case 12:
		case 13:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.627");
		case 14:
		case 15:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.628");
		case 28:
		case 29:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.629");
		case 30:
		case 31:
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.630");
		default:
			return "???";
		}
	}
}
