using System;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Pathfinding;
using xTile.Dimensions;

namespace StardewValley;

[InstanceStatics]
[XmlInclude(typeof(FarmAnimal))]
[XmlInclude(typeof(Farmer))]
[XmlInclude(typeof(NPC))]
[NotImplicitNetField]
public class Character : INetObject<NetFields>, IHaveModData
{
	public const float emoteBeginInterval = 20f;

	public const float emoteNormalInterval = 250f;

	public const int emptyCanEmote = 4;

	public const int questionMarkEmote = 8;

	public const int angryEmote = 12;

	public const int exclamationEmote = 16;

	public const int heartEmote = 20;

	public const int sleepEmote = 24;

	public const int sadEmote = 28;

	public const int happyEmote = 32;

	public const int xEmote = 36;

	public const int pauseEmote = 40;

	public const int videoGameEmote = 52;

	public const int musicNoteEmote = 56;

	public const int blushEmote = 60;

	public const int blockedIntervalBeforeEmote = 3000;

	public const int blockedIntervalBeforeSprint = 5000;

	public const double chanceForSound = 0.001;

	/// <summary>A position value that's invalid, used to force cached position info to update.</summary>
	private static Vector2 ClearPositionValue = new Vector2(-2.1474836E+09f);

	/// <summary>The backing field for <see cref="P:StardewValley.Character.StandingPixel" />.</summary>
	private Point cachedStandingPixel;

	/// <summary>The backing field for <see cref="P:StardewValley.Character.Tile" />.</summary>
	private Vector2 cachedTile;

	/// <summary>The backing field for <see cref="P:StardewValley.Character.TilePoint" />.</summary>
	private Point cachedTilePoint;

	/// <summary>The position value for which <see cref="F:StardewValley.Character.cachedStandingPixel" /> was calculated.</summary>
	private Vector2 pixelPositionForCachedStandingPixel;

	/// <summary>The position value for which <see cref="F:StardewValley.Character.cachedTile" /> was calculated.</summary>
	private Vector2 pixelPositionForCachedTile;

	/// <summary>The position value for which <see cref="F:StardewValley.Character.cachedTilePoint" /> was calculated.</summary>
	private Vector2 pixelPositionForCachedTilePoint;

	/// <summary>Whether to hide this character from the animal social menu, if it would normally be shown.</summary>
	[XmlIgnore]
	public readonly NetBool hideFromAnimalSocialMenu = new NetBool();

	[XmlIgnore]
	public readonly NetRef<AnimatedSprite> sprite = new NetRef<AnimatedSprite>();

	/// <summary>The backing field for <see cref="P:StardewValley.Character.Position" />.</summary>
	[XmlIgnore]
	public readonly NetPosition position = new NetPosition();

	[XmlIgnore]
	private readonly NetInt netSpeed = new NetInt();

	[XmlIgnore]
	private readonly NetFloat netAddedSpeed = new NetFloat();

	[XmlIgnore]
	public readonly NetDirection facingDirection = new NetDirection(2);

	[XmlIgnore]
	public int blockedInterval;

	[XmlIgnore]
	public int faceTowardFarmerTimer;

	[XmlIgnore]
	public int forceUpdateTimer;

	[XmlIgnore]
	public int movementPause;

	[XmlIgnore]
	public NetEvent1Field<int, NetInt> faceTowardFarmerEvent = new NetEvent1Field<int, NetInt>();

	[XmlIgnore]
	public readonly NetInt faceTowardFarmerRadius = new NetInt();

	[XmlIgnore]
	public readonly NetBool simpleNonVillagerNPC = new NetBool();

	[XmlIgnore]
	public int nonVillagerNPCTimesTalked;

	[XmlElement("name")]
	public readonly NetString name = new NetString();

	[XmlElement("forceOneTileWide")]
	public readonly NetBool forceOneTileWide = new NetBool(value: false);

	protected bool moveUp;

	protected bool moveRight;

	protected bool moveDown;

	protected bool moveLeft;

	protected bool freezeMotion;

	[XmlIgnore]
	private string _displayName;

	public bool isEmoting;

	public bool isCharging;

	public bool isGlowing;

	public bool coloredBorder;

	public bool flip;

	public bool drawOnTop;

	public bool faceTowardFarmer;

	public bool ignoreMovementAnimation;

	[XmlIgnore]
	public bool hasJustStartedFacingPlayer;

	[XmlElement("faceAwayFromFarmer")]
	public readonly NetBool faceAwayFromFarmer = new NetBool();

	protected int currentEmote;

	protected int currentEmoteFrame;

	protected readonly NetInt facingDirectionBeforeSpeakingToPlayer = new NetInt(-1);

	[XmlIgnore]
	public float emoteInterval;

	[XmlIgnore]
	public float xVelocity;

	[XmlIgnore]
	public float yVelocity;

	[XmlIgnore]
	public Vector2 lastClick = Vector2.Zero;

	public readonly NetFloat scale = new NetFloat(1f);

	public float glowingTransparency;

	public float glowRate;

	private bool glowUp;

	[XmlIgnore]
	public readonly NetBool swimming = new NetBool();

	[XmlIgnore]
	public bool nextEventcommandAfterEmote;

	[XmlIgnore]
	public bool farmerPassesThrough;

	[XmlIgnore]
	public NetBool netEventActor = new NetBool();

	[XmlIgnore]
	public readonly NetBool collidesWithOtherCharacters = new NetBool();

	protected bool ignoreMovementAnimations;

	[XmlIgnore]
	public int yJumpOffset;

	[XmlIgnore]
	public int ySourceRectOffset;

	[XmlIgnore]
	public float yJumpVelocity;

	[XmlIgnore]
	public float yJumpGravity = -0.5f;

	[XmlIgnore]
	public bool wasJumpWithSound;

	[XmlIgnore]
	private readonly NetFarmerRef whoToFace = new NetFarmerRef();

	[XmlIgnore]
	public Color glowingColor;

	[XmlIgnore]
	public PathFindController controller;

	private bool emoteFading;

	[XmlIgnore]
	private readonly NetBool _willDestroyObjectsUnderfoot = new NetBool(value: true);

	[XmlIgnore]
	protected readonly NetLocationRef currentLocationRef = new NetLocationRef();

	private Microsoft.Xna.Framework.Rectangle originalSourceRect;

	protected int emoteYOffset;

	public static readonly Vector2[] AdjacentTilesOffsets = new Vector2[4]
	{
		new Vector2(1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2(0f, -1f),
		new Vector2(0f, 1f)
	};

	[XmlIgnore]
	public Vector2 drawOffset = Vector2.Zero;

	[XmlIgnore]
	public bool shouldShadowBeOffset;

	/// <summary>The character's gender identity.</summary>
	public virtual Gender Gender { get; set; } = Gender.Undefined;

	[XmlIgnore]
	public int speed
	{
		get
		{
			return this.netSpeed.Value;
		}
		set
		{
			this.netSpeed.Value = value;
		}
	}

	[XmlIgnore]
	public virtual float addedSpeed
	{
		get
		{
			return this.netAddedSpeed.Value;
		}
		set
		{
			this.netAddedSpeed.Value = value;
		}
	}

	[XmlIgnore]
	public virtual string displayName
	{
		get
		{
			return this._displayName ?? (this._displayName = this.translateName());
		}
		set
		{
			this._displayName = value;
		}
	}

	[XmlIgnore]
	public virtual bool EventActor
	{
		get
		{
			return this.netEventActor.Value;
		}
		set
		{
			this.netEventActor.Value = value;
		}
	}

	public bool willDestroyObjectsUnderfoot
	{
		get
		{
			return this._willDestroyObjectsUnderfoot.Value;
		}
		set
		{
			this._willDestroyObjectsUnderfoot.Value = value;
		}
	}

	/// <summary>The character's pixel coordinates within their current location, ignoring their bounding box, relative to the top-left corner of the map.</summary>
	/// <remarks>See also <see cref="P:StardewValley.Character.StandingPixel" /> for the pixel coordinates at the center of their bounding box, and <see cref="P:StardewValley.Character.Tile" /> and <see cref="P:StardewValley.Character.TilePoint" /> for the tile coordinates.</remarks>
	public Vector2 Position
	{
		get
		{
			return this.position.Value;
		}
		set
		{
			if (this.position.Value != value)
			{
				this.position.Set(value);
			}
		}
	}

	/// <summary>The pixel coordinates at the center of this character's bounding box, relative to the top-left corner of the map.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Character.getStandingPosition" /> for a vector version, <see cref="P:StardewValley.Character.Tile" /> and <see cref="P:StardewValley.Character.TilePoint" /> for the tile coordinates, or <see cref="P:StardewValley.Character.Position" /> for the raw pixel position.</remarks>
	public Point StandingPixel
	{
		get
		{
			if (this.position.X != this.pixelPositionForCachedStandingPixel.X || this.position.Y != this.pixelPositionForCachedStandingPixel.Y)
			{
				this.cachedStandingPixel = this.GetBoundingBox().Center;
				this.pixelPositionForCachedStandingPixel = this.position.Value;
			}
			return this.cachedStandingPixel;
		}
	}

	/// <summary>The character's tile position within their current location.</summary>
	/// <remarks>See also <see cref="P:StardewValley.Character.TilePoint" /> for a point version, <see cref="P:StardewValley.Character.StandingPixel" /> the pixel coordinates at the center of their bounding box, or <see cref="P:StardewValley.Character.Position" /> for the raw pixel position.</remarks>
	public Vector2 Tile
	{
		get
		{
			if (this.position.X != this.pixelPositionForCachedTile.X || this.position.Y != this.pixelPositionForCachedTile.Y)
			{
				Point pixel = this.StandingPixel;
				this.cachedTile = new Vector2(pixel.X / 64, pixel.Y / 64);
				this.pixelPositionForCachedTile = this.position.Value;
			}
			return this.cachedTile;
		}
	}

	/// <summary>The character's tile position within their current location as a <see cref="T:Microsoft.Xna.Framework.Point" />.</summary>
	/// <remarks>See also <see cref="P:StardewValley.Character.Tile" /> for a vector version, <see cref="P:StardewValley.Character.StandingPixel" /> the pixel coordinates at the center of their bounding box, or <see cref="P:StardewValley.Character.Position" /> for the raw pixel position.</remarks>
	public Point TilePoint
	{
		get
		{
			if (this.position.X != this.pixelPositionForCachedTilePoint.X || this.position.Y != this.pixelPositionForCachedTilePoint.Y)
			{
				Vector2 tile = this.Tile;
				this.cachedTilePoint = new Point((int)tile.X, (int)tile.Y);
				this.pixelPositionForCachedTilePoint = this.position.Value;
			}
			return this.cachedTilePoint;
		}
	}

	public int Speed
	{
		get
		{
			return this.speed;
		}
		set
		{
			this.speed = value;
		}
	}

	public virtual int FacingDirection
	{
		get
		{
			return this.facingDirection.Value;
		}
		set
		{
			this.facingDirection.Set(value);
		}
	}

	[XmlIgnore]
	public string Name
	{
		get
		{
			return this.name.Value;
		}
		set
		{
			this.name.Set(value);
		}
	}

	[XmlIgnore]
	public bool SimpleNonVillagerNPC
	{
		get
		{
			return this.simpleNonVillagerNPC.Value;
		}
		set
		{
			this.simpleNonVillagerNPC.Set(value);
		}
	}

	[XmlIgnore]
	public virtual AnimatedSprite Sprite
	{
		get
		{
			return this.sprite.Value;
		}
		set
		{
			this.sprite.Value = value;
		}
	}

	public bool IsEmoting
	{
		get
		{
			return this.isEmoting;
		}
		set
		{
			this.isEmoting = value;
		}
	}

	public int CurrentEmote
	{
		get
		{
			return this.currentEmote;
		}
		set
		{
			this.currentEmote = value;
		}
	}

	public int CurrentEmoteIndex => this.currentEmoteFrame;

	/// <summary>Whether this is a monster NPC type, regardless of whether they're present in <c>Data/Monsters</c>.</summary>
	public virtual bool IsMonster => false;

	/// <summary>Whether this is a villager NPC type, regardless of whether they're present in <c>Data/Characters</c>.</summary>
	[XmlIgnore]
	public virtual bool IsVillager => false;

	public float Scale
	{
		get
		{
			return this.scale.Value;
		}
		set
		{
			this.scale.Value = value;
		}
	}

	[XmlIgnore]
	public GameLocation currentLocation
	{
		get
		{
			return this.currentLocationRef.Value;
		}
		set
		{
			this.currentLocationRef.Value = value;
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

	public NetFields NetFields { get; }

	public Character()
	{
		this.NetFields = new NetFields(NetFields.GetNameForInstance(this));
		this.initNetFields();
	}

	protected virtual void initNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.sprite, "sprite").AddField(this.position.NetFields, "position.NetFields")
			.AddField(this.facingDirection, "facingDirection")
			.AddField(this.netSpeed, "netSpeed")
			.AddField(this.netAddedSpeed, "netAddedSpeed")
			.AddField(this.name, "name")
			.AddField(this.scale, "scale")
			.AddField(this.currentLocationRef.NetFields, "currentLocationRef.NetFields")
			.AddField(this.swimming, "swimming")
			.AddField(this.collidesWithOtherCharacters, "collidesWithOtherCharacters")
			.AddField(this.facingDirectionBeforeSpeakingToPlayer, "facingDirectionBeforeSpeakingToPlayer")
			.AddField(this.faceTowardFarmerRadius, "faceTowardFarmerRadius")
			.AddField(this.faceAwayFromFarmer, "faceAwayFromFarmer")
			.AddField(this.whoToFace.NetFields, "whoToFace.NetFields")
			.AddField(this.faceTowardFarmerEvent, "faceTowardFarmerEvent")
			.AddField(this._willDestroyObjectsUnderfoot, "_willDestroyObjectsUnderfoot")
			.AddField(this.forceOneTileWide, "forceOneTileWide")
			.AddField(this.simpleNonVillagerNPC, "simpleNonVillagerNPC")
			.AddField(this.hideFromAnimalSocialMenu, "hideFromAnimalSocialMenu")
			.AddField(this.netEventActor, "netEventActor")
			.AddField(this.modData, "modData");
		this.facingDirection.Position = this.position;
		this.faceTowardFarmerEvent.onEvent += performFaceTowardFarmerEvent;
		this.sprite.fieldChangeEvent += delegate(NetRef<AnimatedSprite> field, AnimatedSprite value, AnimatedSprite newValue)
		{
			newValue?.SetOwner(this);
			this.ClearCachedPosition();
		};
	}

	public Character(AnimatedSprite sprite, Vector2 position, int speed, string name)
		: this()
	{
		this.Sprite = sprite;
		this.Position = position;
		this.speed = speed;
		this.Name = name;
		if (sprite != null)
		{
			this.originalSourceRect = sprite.SourceRect;
		}
	}

	protected virtual string translateName()
	{
		return this.name.Value;
	}

	/// <summary>Forget the cached bounding box values so they're recalculated on the next request.</summary>
	internal void ClearCachedPosition()
	{
		this.pixelPositionForCachedStandingPixel = Character.ClearPositionValue;
		this.pixelPositionForCachedTile = Character.ClearPositionValue;
		this.pixelPositionForCachedTilePoint = Character.ClearPositionValue;
	}

	/// <summary>Reset the cached display name, so <see cref="M:StardewValley.Character.translateName" /> is called again next time it's requested.</summary>
	protected void resetCachedDisplayName()
	{
		this._displayName = null;
	}

	public virtual void SetMovingUp(bool b)
	{
		this.moveUp = b;
		if (!b)
		{
			this.Halt();
		}
	}

	public virtual void SetMovingRight(bool b)
	{
		this.moveRight = b;
		if (!b)
		{
			this.Halt();
		}
	}

	public virtual void SetMovingDown(bool b)
	{
		this.moveDown = b;
		if (!b)
		{
			this.Halt();
		}
	}

	public virtual void SetMovingLeft(bool b)
	{
		this.moveLeft = b;
		if (!b)
		{
			this.Halt();
		}
	}

	public void setMovingInFacingDirection()
	{
		switch (this.FacingDirection)
		{
		case 0:
			this.SetMovingUp(b: true);
			break;
		case 1:
			this.SetMovingRight(b: true);
			break;
		case 2:
			this.SetMovingDown(b: true);
			break;
		case 3:
			this.SetMovingLeft(b: true);
			break;
		}
	}

	public int getFacingDirection()
	{
		if (this.Sprite.currentFrame < 4)
		{
			return 2;
		}
		if (this.Sprite.currentFrame < 8)
		{
			return 1;
		}
		if (this.Sprite.currentFrame < 12)
		{
			return 0;
		}
		return 3;
	}

	public void setTrajectory(int xVelocity, int yVelocity)
	{
		this.setTrajectory(new Vector2(xVelocity, yVelocity));
	}

	public virtual void setTrajectory(Vector2 trajectory)
	{
		this.xVelocity = trajectory.X;
		this.yVelocity = trajectory.Y;
	}

	public virtual void Halt()
	{
		this.moveUp = false;
		this.moveDown = false;
		this.moveRight = false;
		this.moveLeft = false;
		this.Sprite.StopAnimation();
	}

	public void extendSourceRect(int horizontal, int vertical, bool ignoreSourceRectUpdates = true)
	{
		this.Sprite.sourceRect.Inflate(Math.Abs(horizontal) / 2, Math.Abs(vertical) / 2);
		this.Sprite.sourceRect.Offset(horizontal / 2, vertical / 2);
		_ = this.originalSourceRect;
		if (this.Sprite.SourceRect.Equals(this.originalSourceRect))
		{
			this.Sprite.ignoreSourceRectUpdates = false;
		}
		else
		{
			this.Sprite.ignoreSourceRectUpdates = ignoreSourceRectUpdates;
		}
	}

	public virtual bool collideWith(Object o)
	{
		return true;
	}

	public virtual void faceDirection(int direction)
	{
		if (!this.SimpleNonVillagerNPC)
		{
			if (direction != -3)
			{
				this.FacingDirection = direction;
				this.Sprite?.faceDirection(direction);
				this.faceTowardFarmer = false;
			}
			else
			{
				this.faceTowardFarmer = true;
			}
		}
	}

	public int getDirection()
	{
		if (this.moveUp)
		{
			return 0;
		}
		if (this.moveRight)
		{
			return 1;
		}
		if (this.moveDown)
		{
			return 2;
		}
		if (this.moveLeft)
		{
			return 3;
		}
		if (this.IsRemoteMoving())
		{
			return this.FacingDirection;
		}
		return -1;
	}

	public bool IsRemoteMoving()
	{
		if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
		{
			if (!this.position.moving.Value)
			{
				return this.position.Field.IsInterpolating();
			}
			return true;
		}
		return this.position.Field.IsInterpolating();
	}

	public void tryToMoveInDirection(int direction, bool isFarmer, int damagesFarmer, bool glider)
	{
		if (!this.currentLocation.isCollidingPosition(this.nextPosition(direction), Game1.viewport, isFarmer, damagesFarmer, glider, this))
		{
			switch (direction)
			{
			case 0:
				this.position.Y -= (float)this.speed + this.addedSpeed;
				break;
			case 1:
				this.position.X += (float)this.speed + this.addedSpeed;
				break;
			case 2:
				this.position.Y += (float)this.speed + this.addedSpeed;
				break;
			case 3:
				this.position.X -= (float)this.speed + this.addedSpeed;
				break;
			}
		}
	}

	public virtual Vector2 GetShadowOffset()
	{
		if (this.shouldShadowBeOffset)
		{
			return this.drawOffset;
		}
		return Vector2.Zero;
	}

	public virtual bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		if (this.controller == null)
		{
			return !this.IsMonster;
		}
		return false;
	}

	protected void applyVelocity(GameLocation currentLocation)
	{
		Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
		nextPosition.X += (int)this.xVelocity;
		nextPosition.Y -= (int)this.yVelocity;
		if (currentLocation == null || !currentLocation.isCollidingPosition(nextPosition, Game1.viewport, isFarmer: false, 0, glider: false, this))
		{
			this.position.X += this.xVelocity;
			this.position.Y -= this.yVelocity;
		}
		this.xVelocity = (int)(this.xVelocity - this.xVelocity / 2f);
		this.yVelocity = (int)(this.yVelocity - this.yVelocity / 2f);
	}

	public virtual void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (this is FarmAnimal)
		{
			this.willDestroyObjectsUnderfoot = false;
		}
		int num;
		if (this.willDestroyObjectsUnderfoot)
		{
			PathFindController pathFindController = this.controller;
			num = ((pathFindController == null || !pathFindController.nonDestructivePathing) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool shouldDestroyUnderfootObjects = (byte)num != 0;
		if (this.xVelocity != 0f || this.yVelocity != 0f)
		{
			this.applyVelocity(currentLocation);
		}
		else if (this.moveUp)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, 0, glider: false, this) || this.isCharging)
			{
				this.position.Y -= (float)this.speed + this.addedSpeed;
				if (!this.ignoreMovementAnimation)
				{
					this.Sprite.AnimateUp(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
					this.faceDirection(0);
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !shouldDestroyUnderfootObjects)
			{
				this.Halt();
			}
			else if (shouldDestroyUnderfootObjects)
			{
				if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(0), showDestroyedObject: true))
				{
					this.doEmote(12);
					this.position.Y -= (float)this.speed + this.addedSpeed;
				}
				else
				{
					this.blockedInterval += time.ElapsedGameTime.Milliseconds;
				}
			}
		}
		else if (this.moveRight)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, 0, glider: false, this) || this.isCharging)
			{
				this.position.X += (float)this.speed + this.addedSpeed;
				if (!this.ignoreMovementAnimation)
				{
					this.Sprite.AnimateRight(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
					this.faceDirection(1);
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !shouldDestroyUnderfootObjects)
			{
				this.Halt();
			}
			else if (shouldDestroyUnderfootObjects)
			{
				if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(1), showDestroyedObject: true))
				{
					this.doEmote(12);
					this.position.X += (float)this.speed + this.addedSpeed;
				}
				else
				{
					this.blockedInterval += time.ElapsedGameTime.Milliseconds;
				}
			}
		}
		else if (this.moveDown)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, 0, glider: false, this) || this.isCharging)
			{
				this.position.Y += (float)this.speed + this.addedSpeed;
				if (!this.ignoreMovementAnimation)
				{
					this.Sprite.AnimateDown(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
					this.faceDirection(2);
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !shouldDestroyUnderfootObjects)
			{
				this.Halt();
			}
			else if (shouldDestroyUnderfootObjects)
			{
				if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(2), showDestroyedObject: true))
				{
					this.doEmote(12);
					this.position.Y += (float)this.speed + this.addedSpeed;
				}
				else
				{
					this.blockedInterval += time.ElapsedGameTime.Milliseconds;
				}
			}
		}
		else if (this.moveLeft)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, 0, glider: false, this) || this.isCharging)
			{
				this.position.X -= (float)this.speed + this.addedSpeed;
				if (!this.ignoreMovementAnimation)
				{
					this.Sprite.AnimateLeft(time, (this.speed - 2 + (int)this.addedSpeed) * -25, Utility.isOnScreen(this.TilePoint, 1, currentLocation) ? "Cowboy_Footstep" : "");
					this.faceDirection(3);
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !shouldDestroyUnderfootObjects)
			{
				this.Halt();
			}
			else if (shouldDestroyUnderfootObjects)
			{
				if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(3), showDestroyedObject: true))
				{
					this.doEmote(12);
					this.position.X -= (float)this.speed + this.addedSpeed;
				}
				else
				{
					this.blockedInterval += time.ElapsedGameTime.Milliseconds;
				}
			}
		}
		else
		{
			this.Sprite.animateOnce(time);
		}
		if (shouldDestroyUnderfootObjects && currentLocation != null && this.isMoving())
		{
			currentLocation.characterTrampleTile(this.Tile);
		}
		if (this.blockedInterval >= 3000 && (float)this.blockedInterval <= 3750f && !Game1.eventUp)
		{
			this.doEmote(Game1.random.Choose(8, 40));
			this.blockedInterval = 3750;
		}
		else if (this.blockedInterval >= 5000)
		{
			this.speed = 4;
			this.isCharging = true;
			this.blockedInterval = 0;
		}
	}

	public virtual bool canPassThroughActionTiles()
	{
		return false;
	}

	public virtual Microsoft.Xna.Framework.Rectangle nextPosition(int direction)
	{
		Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
		switch (direction)
		{
		case 0:
			nextPosition.Y -= this.speed + (int)this.addedSpeed;
			break;
		case 1:
			nextPosition.X += this.speed + (int)this.addedSpeed;
			break;
		case 2:
			nextPosition.Y += this.speed + (int)this.addedSpeed;
			break;
		case 3:
			nextPosition.X -= this.speed + (int)this.addedSpeed;
			break;
		}
		return nextPosition;
	}

	public Location nextPositionPoint()
	{
		Location nextPositionTile = default(Location);
		Point standingPixel = this.StandingPixel;
		switch (this.getDirection())
		{
		case 0:
			nextPositionTile = new Location(standingPixel.X, standingPixel.Y - 64);
			break;
		case 1:
			nextPositionTile = new Location(standingPixel.X + 64, standingPixel.Y);
			break;
		case 2:
			nextPositionTile = new Location(standingPixel.X, standingPixel.Y + 64);
			break;
		case 3:
			nextPositionTile = new Location(standingPixel.X - 64, standingPixel.Y);
			break;
		}
		return nextPositionTile;
	}

	public int getHorizontalMovement()
	{
		if (!this.moveRight)
		{
			if (!this.moveLeft)
			{
				return 0;
			}
			return -this.speed - (int)this.addedSpeed;
		}
		return this.speed + (int)this.addedSpeed;
	}

	public int getVerticalMovement()
	{
		if (!this.moveDown)
		{
			if (!this.moveUp)
			{
				return 0;
			}
			return -this.speed - (int)this.addedSpeed;
		}
		return this.speed + (int)this.addedSpeed;
	}

	public Vector2 nextPositionVector2()
	{
		Point standingPixel = this.StandingPixel;
		return new Vector2(standingPixel.X + this.getHorizontalMovement(), standingPixel.Y + this.getVerticalMovement());
	}

	public Location nextPositionTile()
	{
		Location nextPositionTile = this.nextPositionPoint();
		nextPositionTile.X /= 64;
		nextPositionTile.Y /= 64;
		return nextPositionTile;
	}

	public virtual void doEmote(int whichEmote, bool playSound, bool nextEventCommand = true)
	{
		if (!this.isEmoting && (!Game1.eventUp || this is Farmer || (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.actors.Contains(this))))
		{
			this.emoteYOffset = 0;
			this.isEmoting = true;
			this.currentEmote = whichEmote;
			this.currentEmoteFrame = 0;
			this.emoteInterval = 0f;
			this.nextEventcommandAfterEmote = nextEventCommand;
		}
	}

	public void doEmote(int whichEmote, bool nextEventCommand = true)
	{
		this.doEmote(whichEmote, playSound: true, nextEventCommand);
	}

	public void doEmote(int whichEmote, int emoteYOffset)
	{
		this.doEmote(whichEmote, playSound: true, nextEventCommand: false);
		this.emoteYOffset = emoteYOffset;
	}

	public void updateEmote(GameTime time)
	{
		if (!this.isEmoting)
		{
			return;
		}
		this.emoteInterval += time.ElapsedGameTime.Milliseconds;
		if (this.emoteFading && this.emoteInterval > 20f)
		{
			this.emoteInterval = 0f;
			this.currentEmoteFrame--;
			if (this.currentEmoteFrame < 0)
			{
				this.emoteFading = false;
				this.isEmoting = false;
				if (this.nextEventcommandAfterEmote && Game1.currentLocation.currentEvent != null && (Game1.currentLocation.currentEvent.actors.Contains(this) || Game1.currentLocation.currentEvent.farmerActors.Contains(this) || this.Name.Equals(Game1.player.Name)))
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
			}
		}
		else if (!this.emoteFading && this.emoteInterval > 20f && this.currentEmoteFrame <= 3)
		{
			this.emoteInterval = 0f;
			this.currentEmoteFrame++;
			if (this.currentEmoteFrame == 4)
			{
				this.currentEmoteFrame = this.currentEmote;
			}
		}
		else if (!this.emoteFading && this.emoteInterval > 250f)
		{
			this.emoteInterval = 0f;
			this.currentEmoteFrame++;
			if (this.currentEmoteFrame >= this.currentEmote + 4)
			{
				this.emoteFading = true;
				this.currentEmoteFrame = 3;
			}
		}
	}

	/// <summary>Play a sound for the current player only if they're near this player.</summary>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> to keep it as-is.</param>
	/// <param name="context">The source which triggered the sound.</param>
	public void playNearbySoundLocal(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
	{
		if (this.currentLocation == null)
		{
			Farmer obj = this as Farmer;
			if (obj == null || !obj.IsLocalPlayer)
			{
				return;
			}
		}
		Game1.sounds.PlayLocal(audioName, this.currentLocation, this.Tile, pitch, context, out var _);
	}

	/// <summary>Play a sound for each nearby online player.</summary>
	/// <param name="audioName">The sound ID to play.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> to keep it as-is.</param>
	/// <param name="context">The source which triggered the sound.</param>
	public void playNearbySoundAll(string audioName, int? pitch = null, SoundContext context = SoundContext.Default)
	{
		if (this.currentLocation == null)
		{
			Farmer obj = this as Farmer;
			if (obj != null && obj.IsLocalPlayer)
			{
				Game1.sounds.PlayLocal(audioName, null, null, pitch, context, out var _);
			}
		}
		else
		{
			Game1.sounds.PlayAll(audioName, this.currentLocation, this.Tile, pitch, context);
		}
	}

	public Vector2 GetGrabTile()
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
		return this.FacingDirection switch
		{
			0 => new Vector2((boundingBox.X + boundingBox.Width / 2) / 64, (boundingBox.Y - 5) / 64), 
			1 => new Vector2((boundingBox.X + boundingBox.Width + 5) / 64, (boundingBox.Y + boundingBox.Height / 2) / 64), 
			2 => new Vector2((boundingBox.X + boundingBox.Width / 2) / 64, (boundingBox.Y + boundingBox.Height + 5) / 64), 
			3 => new Vector2((boundingBox.X - 5) / 64, (boundingBox.Y + boundingBox.Height / 2) / 64), 
			_ => this.getStandingPosition(), 
		};
	}

	public Vector2 GetDropLocation()
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
		return this.FacingDirection switch
		{
			0 => new Vector2(boundingBox.X + 16, boundingBox.Y - 64), 
			1 => new Vector2(boundingBox.X + boundingBox.Width + 64, boundingBox.Y + 16), 
			2 => new Vector2(boundingBox.X + 16, boundingBox.Y + boundingBox.Height + 64), 
			3 => new Vector2(boundingBox.X - 64, boundingBox.Y + 16), 
			_ => this.getStandingPosition(), 
		};
	}

	public virtual Vector2 GetToolLocation(Vector2 target_position, bool ignoreClick = false)
	{
		int direction = this.FacingDirection;
		if ((Game1.player.CurrentTool == null || !Game1.player.CurrentTool.CanUseOnStandingTile()) && (int)(target_position.X / 64f) == Game1.player.TilePoint.X && (int)(target_position.Y / 64f) == Game1.player.TilePoint.Y)
		{
			Microsoft.Xna.Framework.Rectangle bb = this.GetBoundingBox();
			switch (this.FacingDirection)
			{
			case 0:
				return new Vector2(bb.X + bb.Width / 2, bb.Y - 64);
			case 1:
				return new Vector2(bb.X + bb.Width + 64, bb.Y + bb.Height / 2);
			case 2:
				return new Vector2(bb.X + bb.Width / 2, bb.Y + bb.Height + 64);
			case 3:
				return new Vector2(bb.X - 64, bb.Y + bb.Height / 2);
			}
		}
		if (!ignoreClick && !target_position.Equals(Vector2.Zero) && this.Name.Equals(Game1.player.Name))
		{
			bool allow_clicking_on_same_tile = false;
			if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.CanUseOnStandingTile())
			{
				allow_clicking_on_same_tile = true;
			}
			if (Utility.withinRadiusOfPlayer((int)target_position.X, (int)target_position.Y, 1, Game1.player))
			{
				direction = Game1.player.getGeneralDirectionTowards(new Vector2((int)target_position.X, (int)target_position.Y));
				if (allow_clicking_on_same_tile)
				{
					return target_position;
				}
				Point playerPixel = Game1.player.StandingPixel;
				if (Math.Abs(target_position.X - (float)playerPixel.X) >= 32f || Math.Abs(target_position.Y - (float)playerPixel.Y) >= 32f)
				{
					return target_position;
				}
			}
		}
		Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
		return direction switch
		{
			0 => new Vector2(boundingBox.X + boundingBox.Width / 2, boundingBox.Y - 48), 
			1 => new Vector2(boundingBox.X + boundingBox.Width + 48, boundingBox.Y + boundingBox.Height / 2), 
			2 => new Vector2(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height + 48), 
			3 => new Vector2(boundingBox.X - 48, boundingBox.Y + boundingBox.Height / 2), 
			_ => this.getStandingPosition(), 
		};
	}

	public virtual Vector2 GetToolLocation(bool ignoreClick = false)
	{
		if (!Game1.wasMouseVisibleThisFrame || Game1.isAnyGamePadButtonBeingHeld())
		{
			ignoreClick = true;
		}
		return this.GetToolLocation(this.lastClick, ignoreClick);
	}

	public int getGeneralDirectionTowards(Vector2 target, int yBias = 0, bool opposite = false, bool useTileCalculations = true)
	{
		int multiplier = ((!opposite) ? 1 : (-1));
		Point playerPixel = this.StandingPixel;
		int xDif;
		int yDif;
		if (useTileCalculations)
		{
			Point playerTile = this.TilePoint;
			xDif = ((int)(target.X / 64f) - playerTile.X) * multiplier;
			yDif = ((int)(target.Y / 64f) - playerTile.Y) * multiplier;
			if (xDif == 0 && yDif == 0)
			{
				Vector2 vector = new Vector2(((float)(int)(target.X / 64f) + 0.5f) * 64f, ((float)(int)(target.Y / 64f) + 0.5f) * 64f);
				xDif = (int)(vector.X - (float)playerPixel.X) * multiplier;
				yDif = (int)(vector.Y - (float)playerPixel.Y) * multiplier;
				yBias *= 64;
			}
		}
		else
		{
			xDif = (int)(target.X - (float)playerPixel.X) * multiplier;
			yDif = (int)(target.Y - (float)playerPixel.Y) * multiplier;
		}
		if (xDif > Math.Abs(yDif) + yBias)
		{
			return 1;
		}
		if (Math.Abs(xDif) > Math.Abs(yDif) + yBias)
		{
			return 3;
		}
		if (yDif > 0 || ((float)playerPixel.Y - target.Y) * (float)multiplier < 0f)
		{
			return 2;
		}
		return 0;
	}

	public void faceGeneralDirection(Vector2 target, int yBias, bool opposite, bool useTileCalculations)
	{
		this.faceDirection(this.getGeneralDirectionTowards(target, yBias, opposite, useTileCalculations));
	}

	public void faceGeneralDirection(Vector2 target, int yBias = 0, bool opposite = false)
	{
		this.faceGeneralDirection(target, yBias, opposite, useTileCalculations: true);
	}

	public virtual void draw(SpriteBatch b)
	{
		this.draw(b, 1f);
	}

	public virtual void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
	}

	public virtual void draw(SpriteBatch b, float alpha = 1f)
	{
		Vector2 draw_position = this.Position;
		this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, draw_position), (float)this.StandingPixel.Y / 10000f);
		if (this.IsEmoting)
		{
			Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
			emotePosition.Y -= 96f;
			emotePosition.Y += this.emoteYOffset;
			emotePosition.X += (float)(this.Sprite.SourceRect.Width * 4) / 2f - 32f;
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.StandingPixel.Y / 10000f);
		}
	}

	public virtual void draw(SpriteBatch b, int ySourceRectOffset, float alpha = 1f)
	{
		Microsoft.Xna.Framework.Rectangle box = this.GetBoundingBox();
		this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.Position) + new Vector2(this.GetSpriteWidthForPositioning() * 4 / 2, box.Height / 2), (float)box.Center.Y / 10000f, 0, ySourceRectOffset, Color.White, flip: false, 4f, 0f, characterSourceRectOffset: true);
		if (this.IsEmoting)
		{
			Vector2 emotePosition = this.getLocalPosition(Game1.viewport);
			emotePosition.Y -= 96f;
			emotePosition.Y += this.emoteYOffset;
			emotePosition.X += (float)(this.Sprite.SourceRect.Width * 4) / 2f - 32f;
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(this.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, this.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)this.StandingPixel.Y / 10000f);
		}
	}

	public int GetSpriteWidthForPositioning()
	{
		if (this.forceOneTileWide.Value)
		{
			return 16;
		}
		return this.Sprite.SpriteWidth;
	}

	public virtual Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		if (this.Sprite == null)
		{
			return Microsoft.Xna.Framework.Rectangle.Empty;
		}
		Vector2 position = this.Position;
		int width = this.GetSpriteWidthForPositioning() * 4 * 3 / 4;
		return new Microsoft.Xna.Framework.Rectangle((int)position.X + 8, (int)position.Y + 16, width, 32);
	}

	public void stopWithoutChangingFrame()
	{
		this.moveDown = false;
		this.moveLeft = false;
		this.moveRight = false;
		this.moveUp = false;
	}

	public virtual void collisionWithFarmerBehavior()
	{
	}

	/// <summary>Get the pixel coordinates at the center of this character's bounding box as a vector, relative to the top-left corner of the map.</summary>
	/// <remarks>See <see cref="P:StardewValley.Character.StandingPixel" /> for a point version.</remarks>
	public Vector2 getStandingPosition()
	{
		Point pixel = this.StandingPixel;
		return new Vector2(pixel.X, pixel.Y);
	}

	public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
	{
		Vector2 position = this.Position;
		return new Vector2(position.X - (float)viewport.X, position.Y - (float)viewport.Y + (float)this.yJumpOffset) + this.drawOffset;
	}

	public virtual bool isMoving()
	{
		if (!this.moveUp && !this.moveDown && !this.moveRight && !this.moveLeft)
		{
			return this.position.Field.IsInterpolating();
		}
		return true;
	}

	public void setTileLocation(Vector2 tileLocation)
	{
		float standingX = (tileLocation.X + 0.5f) * 64f;
		float standingY = (tileLocation.Y + 0.5f) * 64f;
		Vector2 pos = this.Position;
		Microsoft.Xna.Framework.Rectangle box = this.GetBoundingBox();
		pos.X += standingX - (float)box.Center.X;
		pos.Y += standingY - (float)box.Center.Y;
		this.Position = pos;
	}

	public void startGlowing(Color glowingColor, bool border, float glowRate)
	{
		if (!this.glowingColor.Equals(glowingColor))
		{
			this.isGlowing = true;
			this.coloredBorder = border;
			this.glowingColor = glowingColor;
			this.glowUp = true;
			this.glowRate = glowRate;
			this.glowingTransparency = 0f;
		}
	}

	public void stopGlowing()
	{
		this.isGlowing = false;
		this.glowingColor = Color.White;
	}

	public virtual void jumpWithoutSound(float velocity = 8f)
	{
		this.yJumpVelocity = velocity;
		this.yJumpOffset = -1;
		this.yJumpGravity = -0.5f;
	}

	public virtual void jump()
	{
		this.yJumpVelocity = 8f;
		this.yJumpOffset = -1;
		this.yJumpGravity = -0.5f;
		this.wasJumpWithSound = true;
		this.currentLocation?.localSound("dwop");
	}

	public virtual void jump(float jumpVelocity)
	{
		this.yJumpVelocity = jumpVelocity;
		this.yJumpOffset = -1;
		this.yJumpGravity = -0.5f;
		this.wasJumpWithSound = true;
		this.currentLocation?.localSound("dwop");
	}

	public void faceTowardFarmerForPeriod(int milliseconds, int radius, bool faceAway, Farmer who)
	{
		if (!this.SimpleNonVillagerNPC && ((this.Sprite != null && this.Sprite.CurrentAnimation == null) || this.isMoving()))
		{
			if (this.isMoving())
			{
				milliseconds /= 2;
			}
			this.faceTowardFarmerEvent.Fire(milliseconds);
			this.faceTowardFarmerEvent.Poll();
			if (this.facingDirectionBeforeSpeakingToPlayer.Value == -1)
			{
				this.facingDirectionBeforeSpeakingToPlayer.Value = this.FacingDirection;
			}
			this.faceTowardFarmerRadius.Value = radius;
			this.faceAwayFromFarmer.Value = faceAway;
			this.whoToFace.Value = who;
			this.hasJustStartedFacingPlayer = true;
		}
	}

	protected void performFaceTowardFarmerEvent(int milliseconds)
	{
		if ((this.Sprite != null && this.Sprite.CurrentAnimation == null) || this.isMoving())
		{
			this.Halt();
			this.faceTowardFarmerTimer = milliseconds;
			this.movementPause = milliseconds;
		}
	}

	public virtual void update(GameTime time, GameLocation location)
	{
		this.position.UpdateExtrapolation((float)this.speed + this.addedSpeed);
		this.update(time, location, 0L, move: true);
	}

	/// <summary>Update when the location containing this character is removed from the game (e.g. a mine level that was unloaded).</summary>
	public virtual void OnLocationRemoved()
	{
	}

	public virtual void checkForFootstep()
	{
		Game1.currentLocation.playTerrainSound(this.Tile, this);
	}

	public virtual void update(GameTime time, GameLocation location, long id, bool move)
	{
		this.position.UpdateExtrapolation((float)this.speed + this.addedSpeed);
		this.currentLocation = location;
		this.faceTowardFarmerEvent.Poll();
		if (this.yJumpOffset != 0)
		{
			this.yJumpVelocity += this.yJumpGravity;
			this.yJumpOffset -= (int)this.yJumpVelocity;
			if (this.yJumpOffset >= 0)
			{
				this.yJumpOffset = 0;
				this.yJumpVelocity = 0f;
				if (!this.IsMonster && (location == null || location.Equals(Game1.currentLocation)) && this.wasJumpWithSound)
				{
					this.checkForFootstep();
				}
			}
		}
		if (this.forceUpdateTimer > 0)
		{
			this.forceUpdateTimer -= time.ElapsedGameTime.Milliseconds;
		}
		this.updateGlow();
		this.updateEmote(time);
		this.updateFaceTowardsFarmer(time, location);
		bool is_event_controlled_character = false;
		if (location.currentEvent != null)
		{
			if (location.IsTemporary)
			{
				is_event_controlled_character = true;
			}
			else if (location.currentEvent.actors.Contains(this))
			{
				is_event_controlled_character = true;
			}
		}
		if (Game1.IsMasterGame || is_event_controlled_character)
		{
			if (this.controller == null && move && !this.freezeMotion)
			{
				this.updateMovement(location, time);
			}
			if (this.controller != null && !this.freezeMotion && this.controller.update(time))
			{
				this.controller = null;
			}
		}
		else
		{
			this.updateSlaveAnimation(time);
		}
		this.hasJustStartedFacingPlayer = false;
	}

	public virtual void updateFaceTowardsFarmer(GameTime time, GameLocation location)
	{
		if (this.faceTowardFarmerTimer > 0)
		{
			this.faceTowardFarmerTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.whoToFace.Value != null)
			{
				Vector2 tile = this.Tile;
				if (!this.faceTowardFarmer && this.faceTowardFarmerTimer > 0 && Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, this.faceTowardFarmerRadius.Value, this.whoToFace.Value))
				{
					this.faceTowardFarmer = true;
				}
				else if (!Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, this.faceTowardFarmerRadius.Value, this.whoToFace.Value) || this.faceTowardFarmerTimer <= 0)
				{
					this.faceDirection(this.facingDirectionBeforeSpeakingToPlayer.Value);
					if (this.faceTowardFarmerTimer <= 0)
					{
						this.facingDirectionBeforeSpeakingToPlayer.Value = -1;
						this.faceTowardFarmer = false;
						this.faceAwayFromFarmer.Value = false;
						this.faceTowardFarmerTimer = 0;
					}
				}
			}
		}
		if ((Game1.IsMasterGame || location.currentEvent != null) && this.faceTowardFarmer && this.whoToFace.Value != null)
		{
			this.faceGeneralDirection(this.whoToFace.Value.getStandingPosition(), 0, opposite: false, useTileCalculations: true);
			if (this.faceAwayFromFarmer.Value)
			{
				this.faceDirection((this.FacingDirection + 2) % 4);
			}
		}
		this.hasJustStartedFacingPlayer = false;
	}

	public virtual bool hasSpecialCollisionRules()
	{
		return false;
	}

	/// <summary>
	///
	/// make sure that you also override hasSpecialCollisionRules() in any class that overrides isColliding().
	/// Otherwise isColliding() will never be called.
	/// dumb I kno
	/// </summary>
	/// <param name="l"></param>
	/// <param name="tile"></param>
	/// <returns></returns>
	public virtual bool isColliding(GameLocation l, Vector2 tile)
	{
		return false;
	}

	public virtual void animateInFacingDirection(GameTime time)
	{
		switch (this.FacingDirection)
		{
		case 0:
			this.Sprite.AnimateUp(time);
			break;
		case 1:
			this.Sprite.AnimateRight(time);
			break;
		case 2:
			this.Sprite.AnimateDown(time);
			break;
		case 3:
			this.Sprite.AnimateLeft(time);
			break;
		}
	}

	public virtual void updateMovement(GameLocation location, GameTime time)
	{
	}

	protected virtual void updateSlaveAnimation(GameTime time)
	{
		if (this.Sprite.CurrentAnimation != null)
		{
			this.Sprite.animateOnce(time);
		}
		else if (!this.SimpleNonVillagerNPC)
		{
			this.faceDirection(this.FacingDirection);
			if (this.isMoving())
			{
				this.animateInFacingDirection(time);
			}
			else
			{
				this.Sprite.StopAnimation();
			}
		}
	}

	public void updateGlow()
	{
		if (!this.isGlowing)
		{
			return;
		}
		if (this.glowUp)
		{
			this.glowingTransparency += this.glowRate;
			if (this.glowingTransparency >= 1f)
			{
				this.glowingTransparency = 1f;
				this.glowUp = false;
			}
		}
		else
		{
			this.glowingTransparency -= this.glowRate;
			if (this.glowingTransparency <= 0f)
			{
				this.glowingTransparency = 0f;
				this.glowUp = true;
			}
		}
	}

	public void convertEventMotionCommandToMovement(Vector2 command)
	{
		if (command.X < 0f)
		{
			this.SetMovingLeft(b: true);
		}
		else if (command.X > 0f)
		{
			this.SetMovingRight(b: true);
		}
		else if (command.Y < 0f)
		{
			this.SetMovingUp(b: true);
		}
		else if (command.Y > 0f)
		{
			this.SetMovingDown(b: true);
		}
	}

	/// <summary>Draw the shadow under this character.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	public virtual void DrawShadow(SpriteBatch b)
	{
		int offsetX = this.GetSpriteWidthForPositioning() * 4 / 2;
		int offsetY = this.GetBoundingBox().Height;
		float shadowScale = Math.Max(0f, 4f + (float)this.yJumpOffset / 40f) * this.scale.Value;
		if (!this.IsMonster)
		{
			offsetY = ((Game1.CurrentEvent == null || this.Sprite.SpriteHeight > 16) ? (offsetY + 12) : (offsetY + -4));
		}
		if (this.IsVillager && NPC.TryGetData(this.Name, out var data) && data.Shadow != null)
		{
			CharacterShadowData shadow = data.Shadow;
			if (!shadow.Visible)
			{
				return;
			}
			offsetX += shadow.Offset.X;
			offsetY += shadow.Offset.Y;
			shadowScale = Math.Max(0f, shadowScale * shadow.Scale);
		}
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.GetShadowOffset() + this.Position + new Vector2(offsetX, offsetY)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), shadowScale, SpriteEffects.None, Math.Max(0f, (float)this.StandingPixel.Y / 10000f) - 1E-06f);
	}
}
