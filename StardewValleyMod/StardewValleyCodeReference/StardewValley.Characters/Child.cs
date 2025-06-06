using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using xTile.Dimensions;

namespace StardewValley.Characters;

public class Child : NPC
{
	public const int newborn = 0;

	public const int baby = 1;

	public const int crawler = 2;

	public const int toddler = 3;

	[XmlElement("daysOld")]
	public readonly NetInt daysOld = new NetInt(0);

	[XmlElement("idOfParent")]
	public NetLong idOfParent = new NetLong(0L);

	[XmlElement("darkSkinned")]
	public readonly NetBool darkSkinned = new NetBool(value: false);

	private readonly NetEvent1Field<int, NetInt> setStateEvent = new NetEvent1Field<int, NetInt>();

	[XmlElement("hat")]
	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	private int previousState;

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public Child()
	{
	}

	public Child(string name, bool isMale, bool isDarkSkinned, Farmer parent)
	{
		base.Age = 2;
		this.Gender = ((!isMale) ? Gender.Female : Gender.Male);
		this.darkSkinned.Value = isDarkSkinned;
		this.reloadSprite();
		base.Name = name;
		this.displayName = name;
		base.DefaultMap = "FarmHouse";
		base.HideShadow = true;
		base.speed = 1;
		this.idOfParent.Value = parent.UniqueMultiplayerID;
		base.Breather = false;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.setStateEvent, "setStateEvent").AddField(this.darkSkinned, "darkSkinned").AddField(this.daysOld, "daysOld")
			.AddField(this.idOfParent, "idOfParent")
			.AddField(this.mutex.NetFields, "mutex.NetFields")
			.AddField(this.hat, "hat");
		base.age.fieldChangeVisibleEvent += delegate
		{
			this.reloadSprite();
		};
		this.setStateEvent.onEvent += doSetState;
		base.name.FilterStringEvent += Utility.FilterDirtyWords;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		if (Game1.IsMasterGame && !onlyAppearance)
		{
			Farmer parent = Game1.GetPlayer(this.idOfParent.Value);
			if (this.idOfParent.Value == 0L || parent == null)
			{
				long parent_unique_id = Game1.MasterPlayer.UniqueMultiplayerID;
				if (base.currentLocation is FarmHouse)
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						if (Utility.getHomeOfFarmer(farmer) == base.currentLocation)
						{
							parent_unique_id = farmer.UniqueMultiplayerID;
							break;
						}
					}
				}
				this.idOfParent.Value = parent_unique_id;
			}
		}
		if (this.Sprite == null)
		{
			this.Sprite = new AnimatedSprite("Characters\\Baby" + (this.darkSkinned.Value ? "_dark" : ""), 0, 22, 16);
		}
		if (base.Age >= 3)
		{
			this.Sprite.textureName.Value = "Characters\\Toddler" + ((this.Gender == Gender.Male) ? "" : "_girl") + (this.darkSkinned.Value ? "_dark" : "");
			this.Sprite.SpriteWidth = 16;
			this.Sprite.SpriteHeight = 32;
			this.Sprite.currentFrame = 0;
			base.HideShadow = false;
		}
		else
		{
			this.Sprite.textureName.Value = "Characters\\Baby" + (this.darkSkinned.Value ? "_dark" : "");
			this.Sprite.SpriteWidth = 22;
			this.Sprite.SpriteHeight = ((base.Age == 1) ? 32 : 16);
			this.Sprite.currentFrame = 0;
			switch (base.Age)
			{
			case 1:
				this.Sprite.currentFrame = 4;
				break;
			case 2:
				this.Sprite.currentFrame = 32;
				break;
			}
			base.HideShadow = true;
		}
		this.Sprite.UpdateSourceRect();
		base.Breather = false;
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (this.Sprite?.Texture == null)
		{
			this.reloadSprite(onlyAppearance: true);
		}
	}

	protected override void updateSlaveAnimation(GameTime time)
	{
		if (base.Age >= 2 && (this.Sprite.currentFrame > 7 || this.Sprite.SpriteHeight != 16))
		{
			base.updateSlaveAnimation(time);
		}
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (Game1.eventUp && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
		{
			base.MovePosition(time, viewport, currentLocation);
			return;
		}
		if (!Game1.IsMasterGame)
		{
			base.moveLeft = base.IsRemoteMoving() && this.FacingDirection == 3;
			base.moveRight = base.IsRemoteMoving() && this.FacingDirection == 1;
			base.moveUp = base.IsRemoteMoving() && this.FacingDirection == 0;
			base.moveDown = base.IsRemoteMoving() && this.FacingDirection == 2;
		}
		if (base.moveUp)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, 0, glider: false, this) || base.isCharging)
			{
				if (Game1.IsMasterGame)
				{
					base.position.Y -= (float)base.speed + this.addedSpeed;
				}
				if (base.Age == 3)
				{
					this.Sprite.AnimateUp(time);
					this.FacingDirection = 0;
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
			{
				base.moveUp = false;
				this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
				this.Sprite.CurrentAnimation = null;
				if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
				{
					this.setCrawlerInNewDirection();
				}
			}
		}
		else if (base.moveRight)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, 0, glider: false, this) || base.isCharging)
			{
				if (Game1.IsMasterGame)
				{
					base.position.X += (float)base.speed + this.addedSpeed;
				}
				if (base.Age == 3)
				{
					this.Sprite.AnimateRight(time);
					this.FacingDirection = 1;
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
			{
				base.moveRight = false;
				this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
				this.Sprite.CurrentAnimation = null;
				if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
				{
					this.setCrawlerInNewDirection();
				}
			}
		}
		else if (base.moveDown)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, 0, glider: false, this) || base.isCharging)
			{
				if (Game1.IsMasterGame)
				{
					base.position.Y += (float)base.speed + this.addedSpeed;
				}
				if (base.Age == 3)
				{
					this.Sprite.AnimateDown(time);
					this.FacingDirection = 2;
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
			{
				base.moveDown = false;
				this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
				this.Sprite.CurrentAnimation = null;
				if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
				{
					this.setCrawlerInNewDirection();
				}
			}
		}
		else if (base.moveLeft)
		{
			if (currentLocation == null || !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, 0, glider: false, this) || base.isCharging)
			{
				if (Game1.IsMasterGame)
				{
					base.position.X -= (float)base.speed + this.addedSpeed;
				}
				if (base.Age == 3)
				{
					this.Sprite.AnimateLeft(time);
					this.FacingDirection = 3;
				}
			}
			else if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
			{
				base.moveLeft = false;
				this.Sprite.currentFrame = ((this.Sprite.CurrentAnimation != null) ? this.Sprite.CurrentAnimation[0].frame : this.Sprite.currentFrame);
				this.Sprite.CurrentAnimation = null;
				if (Game1.IsMasterGame && base.Age == 2 && Game1.timeOfDay < 1800)
				{
					this.setCrawlerInNewDirection();
				}
			}
		}
		if (base.blockedInterval >= 3000 && (float)base.blockedInterval <= 3750f && !Game1.eventUp)
		{
			base.doEmote(Game1.random.Choose(8, 40));
			base.blockedInterval = 3750;
		}
		else if (base.blockedInterval >= 5000)
		{
			base.speed = 1;
			base.isCharging = true;
			base.blockedInterval = 0;
		}
	}

	public override bool canPassThroughActionTiles()
	{
		return false;
	}

	public override void resetForNewDay(int dayOfMonth)
	{
		base.resetForNewDay(dayOfMonth);
		if (base.currentLocation is FarmHouse farmhouse && farmhouse.GetChildBed(this.GetChildIndex()) == null)
		{
			base.sleptInBed.Value = false;
		}
	}

	protected override string translateName()
	{
		return base.name.Value.TrimEnd();
	}

	public override void reloadData()
	{
	}

	public override void dayUpdate(int dayOfMonth)
	{
		base.UpdateInvisibilityOnNewDay();
		this.resetForNewDay(dayOfMonth);
		this.mutex.ReleaseLock();
		base.moveUp = false;
		base.moveDown = false;
		base.moveLeft = false;
		base.moveRight = false;
		int parent_unique_id = (int)Game1.MasterPlayer.UniqueMultiplayerID;
		if (Game1.currentLocation is FarmHouse { HasOwner: not false } farmhouse)
		{
			parent_unique_id = (int)farmhouse.OwnerId;
		}
		Random r = Utility.CreateDaySaveRandom(parent_unique_id * 2);
		this.daysOld.Value = this.daysOld.Value + 1;
		if (this.daysOld.Value >= 55)
		{
			base.Age = 3;
			base.speed = 4;
		}
		else if (this.daysOld.Value >= 27)
		{
			base.Age = 2;
		}
		else if (this.daysOld.Value >= 13)
		{
			base.Age = 1;
		}
		if (base.age.Value == 0 || base.age.Value == 1)
		{
			base.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
		}
		if (base.Age == 2)
		{
			base.speed = 1;
			Point p = (base.currentLocation as FarmHouse).getRandomOpenPointInHouse(r, 1, 200);
			if (!p.Equals(Point.Zero))
			{
				base.setTilePosition(p);
			}
			else
			{
				base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -24f);
			}
			this.Sprite.CurrentAnimation = null;
		}
		if (base.Age == 3)
		{
			Point p2 = (base.currentLocation as FarmHouse).getRandomOpenPointInHouse(r, 1, 200);
			if (!p2.Equals(Point.Zero))
			{
				base.setTilePosition(p2);
			}
			else
			{
				p2 = (base.currentLocation as FarmHouse).GetChildBedSpot(this.GetChildIndex());
				if (!p2.Equals(Point.Zero))
				{
					base.setTilePosition(p2);
				}
			}
			this.Sprite.CurrentAnimation = null;
		}
		this.reloadSprite();
		if (base.Age == 2)
		{
			this.setCrawlerInNewDirection();
		}
	}

	public bool isInCrib()
	{
		Point tile = base.TilePoint;
		if (tile.X >= 30 && tile.X <= 32 && tile.Y >= 13)
		{
			return tile.Y <= 14;
		}
		return false;
	}

	/// <inheritdoc />
	public override bool hasDarkSkin()
	{
		return this.darkSkinned.Value;
	}

	public void toss(Farmer who)
	{
		if (base.IsInvisible || Game1.timeOfDay >= 1800 || this.Sprite.SpriteHeight <= 16)
		{
			return;
		}
		if (who == Game1.player)
		{
			this.mutex.RequestLock(delegate
			{
				this.performToss(who);
			});
		}
		else
		{
			this.performToss(who);
		}
	}

	public void performToss(Farmer who)
	{
		who.forceTimePass = true;
		who.faceDirection(2);
		who.FarmerSprite.PauseForSingleAnimation = false;
		base.Position = who.Position + new Vector2(-16f, -96f);
		who.stats?.Increment("timesTossedBaby", 1);
		if (Game1.random.NextDouble() < 0.01 && who.stats.Get("timesTossedBaby") > 3)
		{
			base.yJumpVelocity = 30f;
			who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(57, 2500, secondaryArm: false, flip: false, doneTossing, behaviorAtEndOfFrame: true)
			});
			who.freezePause = 2500;
			Game1.playSound("crit");
		}
		else
		{
			who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(57, 1500, secondaryArm: false, flip: false, doneTossing, behaviorAtEndOfFrame: true)
			});
			who.freezePause = 1500;
			base.yJumpVelocity = Game1.random.Next(12, 19);
			Game1.playSound("dwop");
		}
		base.yJumpOffset = -1;
		who.CanMove = false;
		base.drawOnTop = true;
		this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
		{
			new FarmerSprite.AnimationFrame(4, 100),
			new FarmerSprite.AnimationFrame(5, 100),
			new FarmerSprite.AnimationFrame(6, 100),
			new FarmerSprite.AnimationFrame(7, 100)
		});
	}

	public void doneTossing(Farmer who)
	{
		who.forceTimePass = false;
		this.resetForPlayerEntry(who.currentLocation);
		who.CanMove = true;
		who.forceCanMove();
		who.faceDirection(0);
		base.drawOnTop = false;
		base.doEmote(20);
		if (!who.friendshipData.ContainsKey(base.Name))
		{
			who.friendshipData.Add(base.Name, new Friendship(250));
		}
		who.talkToFriend(this);
		Game1.playSound("tinyWhip");
		if (this.mutex.IsLockHeld())
		{
			this.mutex.ReleaseLock();
		}
	}

	/// <inheritdoc />
	public override Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
	{
		return base.Age switch
		{
			0 => new Microsoft.Xna.Framework.Rectangle(0, 0, 22, 16), 
			1 => new Microsoft.Xna.Framework.Rectangle(0, 42, 22, 24), 
			2 => new Microsoft.Xna.Framework.Rectangle(0, 112, 22, 16), 
			3 => new Microsoft.Xna.Framework.Rectangle(0, 4, 16, 24), 
			_ => Microsoft.Xna.Framework.Rectangle.Empty, 
		};
	}

	private void setState(int state)
	{
		this.setStateEvent.Fire(state);
	}

	private void doSetState(int state)
	{
		switch (state)
		{
		case 0:
			base.SetMovingOnlyUp();
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(32, 160),
				new FarmerSprite.AnimationFrame(33, 160),
				new FarmerSprite.AnimationFrame(34, 160),
				new FarmerSprite.AnimationFrame(35, 160)
			});
			break;
		case 1:
			base.SetMovingOnlyRight();
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(28, 160),
				new FarmerSprite.AnimationFrame(29, 160),
				new FarmerSprite.AnimationFrame(30, 160),
				new FarmerSprite.AnimationFrame(31, 160)
			});
			break;
		case 2:
			base.SetMovingOnlyDown();
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(24, 160),
				new FarmerSprite.AnimationFrame(25, 160),
				new FarmerSprite.AnimationFrame(26, 160),
				new FarmerSprite.AnimationFrame(27, 160)
			});
			break;
		case 3:
			base.SetMovingOnlyLeft();
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(36, 160),
				new FarmerSprite.AnimationFrame(37, 160),
				new FarmerSprite.AnimationFrame(38, 160),
				new FarmerSprite.AnimationFrame(39, 160)
			});
			break;
		case 4:
			this.Halt();
			this.Sprite.SpriteHeight = 16;
			this.Sprite.setCurrentAnimation(this.getRandomCrawlerAnimation(0));
			break;
		case 5:
			this.Halt();
			this.Sprite.SpriteHeight = 16;
			this.Sprite.setCurrentAnimation(this.getRandomCrawlerAnimation(1));
			break;
		}
	}

	private void setCrawlerInNewDirection()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		base.speed = 1;
		int state = Game1.random.Next(6);
		if (Game1.timeOfDay >= 1800 && this.isInCrib())
		{
			this.Sprite.currentFrame = 7;
			this.Sprite.UpdateSourceRect();
			return;
		}
		if (this.previousState >= 4 && Game1.random.NextDouble() < 0.6)
		{
			state = this.previousState;
		}
		if (state < 4)
		{
			while (state == this.previousState)
			{
				state = Game1.random.Next(6);
			}
		}
		else if (this.previousState >= 4)
		{
			state = this.previousState;
		}
		if (this.isInCrib())
		{
			state = Game1.random.Next(4, 6);
		}
		this.setState(state);
		this.previousState = state;
	}

	public override bool hasSpecialCollisionRules()
	{
		return true;
	}

	public override bool isColliding(GameLocation l, Vector2 tile)
	{
		if (!l.isTilePlaceable(tile))
		{
			return true;
		}
		return false;
	}

	public void tenMinuteUpdate()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (base.Age == 2)
		{
			this.setCrawlerInNewDirection();
		}
		else if (Game1.timeOfDay % 100 == 0 && base.Age == 3 && Game1.timeOfDay < 1900)
		{
			base.IsWalkingInSquare = false;
			this.Halt();
			FarmHouse farmHouse = base.currentLocation as FarmHouse;
			if (farmHouse.characters.Contains(this))
			{
				base.controller = new PathFindController(this, farmHouse, farmHouse.getRandomOpenPointInHouse(Game1.random, 1), -1, toddlerReachedDestination);
				if (base.controller.pathToEndPoint == null || !farmHouse.isTileOnMap(base.controller.pathToEndPoint.Last()))
				{
					base.controller = null;
				}
			}
		}
		else
		{
			if (base.Age != 3 || Game1.timeOfDay != 1900)
			{
				return;
			}
			base.IsWalkingInSquare = false;
			this.Halt();
			FarmHouse farmHouse2 = base.currentLocation as FarmHouse;
			if (!farmHouse2.characters.Contains(this))
			{
				return;
			}
			int child_index = this.GetChildIndex();
			BedFurniture bed = farmHouse2.GetChildBed(child_index);
			Point bed_point = farmHouse2.GetChildBedSpot(child_index);
			if (!bed_point.Equals(Point.Zero))
			{
				base.controller = new PathFindController(this, farmHouse2, bed_point, -1, toddlerReachedDestination);
				if (base.controller.pathToEndPoint == null || !farmHouse2.isTileOnMap(base.controller.pathToEndPoint.Last()))
				{
					base.controller = null;
				}
				else
				{
					bed?.ReserveForNPC();
				}
			}
		}
	}

	public virtual int GetChildIndex()
	{
		Farmer parent = Game1.GetPlayer(this.idOfParent.Value);
		if (parent != null)
		{
			List<Child> children = parent.getChildren();
			children.Sort((Child a, Child b) => a.daysOld.Value.CompareTo(b.daysOld.Value));
			children.Reverse();
			return children.IndexOf(this);
		}
		return (int)this.Gender;
	}

	public void toddlerReachedDestination(Character c, GameLocation l)
	{
		if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 2)
		{
			List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(17, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(18, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(19, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(18, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(17, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 1000, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(17, 100, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(18, 100, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(19, 100, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(18, 300, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(17, 100, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 2000, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(17, 180, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(16, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(0, 800, 0, secondaryArm: false, flip: false)
			};
			this.Sprite.setCurrentAnimation(animation);
		}
		else if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 1)
		{
			List<FarmerSprite.AnimationFrame> animation2 = new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(20, 120, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(21, 70, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(22, 70, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(23, 70, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(22, 999999, 0, secondaryArm: false, flip: false)
			};
			this.Sprite.setCurrentAnimation(animation2);
		}
		else if (Game1.random.NextDouble() < 0.8 && c.FacingDirection == 3)
		{
			List<FarmerSprite.AnimationFrame> animation3 = new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(20, 120, 0, secondaryArm: false, flip: true),
				new FarmerSprite.AnimationFrame(21, 70, 0, secondaryArm: false, flip: true),
				new FarmerSprite.AnimationFrame(22, 70, 0, secondaryArm: false, flip: true),
				new FarmerSprite.AnimationFrame(23, 70, 0, secondaryArm: false, flip: true),
				new FarmerSprite.AnimationFrame(22, 999999, 0, secondaryArm: false, flip: true)
			};
			this.Sprite.setCurrentAnimation(animation3);
		}
		else if (c.FacingDirection == 0)
		{
			base.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64);
			base.squareMovementFacingPreference = -1;
			base.walkInSquare(4, 4, 2000);
		}
	}

	public override bool canTalk()
	{
		if (Game1.player.friendshipData.TryGetValue(base.Name, out var friendship))
		{
			return !friendship.TalkedToToday;
		}
		return false;
	}

	public override bool checkAction(Farmer who, GameLocation l)
	{
		if (base.IsInvisible)
		{
			return false;
		}
		if (!who.friendshipData.ContainsKey(base.Name))
		{
			who.friendshipData.Add(base.Name, new Friendship(250));
		}
		if (base.Age >= 2 && !who.hasTalkedToFriendToday(base.Name))
		{
			who.talkToFriend(this);
			base.doEmote(20, nextEventCommand: false);
			if (base.Age == 3)
			{
				base.faceTowardFarmerForPeriod(4000, 3, faceAway: false, who);
			}
			return true;
		}
		if (Game1.CurrentEvent != null)
		{
			return false;
		}
		if (base.Age >= 3 && who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat)
		{
			if (this.hat.Value != null)
			{
				Game1.createItemDebris(this.hat.Value, base.Position, this.FacingDirection);
				this.hat.Value = null;
			}
			else
			{
				Hat hatItem = who.Items[who.CurrentToolIndex] as Hat;
				who.Items[who.CurrentToolIndex] = null;
				this.hat.Value = hatItem;
				Game1.playSound("dirtyHit");
			}
		}
		return false;
	}

	private List<FarmerSprite.AnimationFrame> getRandomCrawlerAnimation(int which = -1)
	{
		List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
		double d = Game1.random.NextDouble();
		if (which == 0 || d < 0.5)
		{
			animation.Add(new FarmerSprite.AnimationFrame(40, 500, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(43, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(40, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(43, 1900, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(42, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(40, 500, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(40, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(41, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(40, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(40, 1500, 0, secondaryArm: false, flip: false));
		}
		else if (which == 1 || d >= 0.5)
		{
			animation.Add(new FarmerSprite.AnimationFrame(44, 1500, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(45, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(46, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(45, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(46, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(45, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(44, 200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(46, 200, 0, secondaryArm: false, flip: false));
		}
		return animation;
	}

	private List<FarmerSprite.AnimationFrame> getRandomNewbornAnimation()
	{
		List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
		if (Game1.random.NextBool())
		{
			animation.Add(new FarmerSprite.AnimationFrame(0, 400, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(1, 400, 0, secondaryArm: false, flip: false));
		}
		else
		{
			animation.Add(new FarmerSprite.AnimationFrame(1, 3400, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(2, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(3, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(4, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(5, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(6, 4400, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(5, 3400, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(4, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(3, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(2, 100, 0, secondaryArm: false, flip: false));
		}
		return animation;
	}

	private List<FarmerSprite.AnimationFrame> getRandomBabyAnimation()
	{
		List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
		if (Game1.random.NextBool())
		{
			animation.Add(new FarmerSprite.AnimationFrame(4, 120, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(5, 120, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(6, 120, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(7, 120, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(4, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(5, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(6, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(7, 100, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(4, 150, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(5, 150, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(6, 150, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(7, 150, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(4, 2000, 0, secondaryArm: false, flip: false));
			if (Game1.random.NextBool())
			{
				animation.Add(new FarmerSprite.AnimationFrame(8, 1950, 0, secondaryArm: false, flip: false));
				animation.Add(new FarmerSprite.AnimationFrame(9, 1200, 0, secondaryArm: false, flip: false));
				animation.Add(new FarmerSprite.AnimationFrame(10, 180, 0, secondaryArm: false, flip: false));
				animation.Add(new FarmerSprite.AnimationFrame(11, 1500, 0, secondaryArm: false, flip: false));
				animation.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, secondaryArm: false, flip: false));
			}
		}
		else
		{
			animation.Add(new FarmerSprite.AnimationFrame(8, 250, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(9, 250, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(10, 250, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(11, 250, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(8, 1950, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(9, 1200, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(10, 180, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(11, 1500, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(9, 150, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(10, 150, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(11, 150, 0, secondaryArm: false, flip: false));
			animation.Add(new FarmerSprite.AnimationFrame(8, 1500, 0, secondaryArm: false, flip: false));
		}
		return animation;
	}

	public override void update(GameTime time, GameLocation location)
	{
		this.setStateEvent.Poll();
		this.mutex.Update(location);
		base.update(time, location);
		if (base.Age >= 2 && (Game1.IsMasterGame || base.Age < 3))
		{
			this.MovePosition(time, Game1.viewport, location);
		}
		if (base.yJumpVelocity > 18f)
		{
			Utility.addSmokePuff(location, base.Position + new Vector2(32f, base.yJumpOffset), 0, base.yJumpVelocity / 8f, 0.01f, 0.75f, 0.01f);
		}
	}

	public void resetForPlayerEntry(GameLocation l)
	{
		switch (base.Age)
		{
		case 0:
			base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -24f);
			if (Game1.timeOfDay >= 1800 && this.Sprite != null)
			{
				this.Sprite.StopAnimation();
				this.Sprite.currentFrame = Game1.random.Next(7);
			}
			else
			{
				this.Sprite?.setCurrentAnimation(this.getRandomNewbornAnimation());
			}
			break;
		case 1:
			base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -12f);
			if (Game1.timeOfDay >= 1800 && this.Sprite != null)
			{
				this.Sprite.StopAnimation();
				this.Sprite.SpriteHeight = 16;
				this.Sprite.currentFrame = Game1.random.Next(7);
			}
			else if (this.Sprite != null)
			{
				this.Sprite.SpriteHeight = 32;
				this.Sprite.setCurrentAnimation(this.getRandomBabyAnimation());
			}
			break;
		case 2:
			if (this.Sprite != null)
			{
				this.Sprite.SpriteHeight = 16;
			}
			if (Game1.timeOfDay >= 1800)
			{
				base.Position = new Vector2(31f, 14f) * 64f + new Vector2(0f, -24f);
				if (this.Sprite != null)
				{
					this.Sprite.StopAnimation();
					this.Sprite.SpriteHeight = 16;
					this.Sprite.currentFrame = 7;
				}
			}
			break;
		}
		if (this.Sprite != null)
		{
			this.Sprite.loop = true;
		}
		if (base.drawOnTop && !this.mutex.IsLocked())
		{
			base.drawOnTop = false;
		}
		this.Sprite.UpdateSourceRect();
	}

	public override void draw(SpriteBatch b, float alpha = 1f)
	{
		Microsoft.Xna.Framework.Rectangle cached_source_rect = this.Sprite.SourceRect;
		int cached_sprite_height = this.Sprite.SpriteHeight;
		int cached_y_offset = base.yJumpOffset;
		if (!base.IsInvisible && this.hat.Value != null && this.hat.Value.hairDrawType.Value != 0)
		{
			Microsoft.Xna.Framework.Rectangle source_rect = this.Sprite.SourceRect;
			int new_height = 17;
			switch (this.Sprite.CurrentFrame)
			{
			case 0:
				new_height = 17;
				break;
			case 1:
				new_height = 18;
				break;
			case 2:
				new_height = 17;
				break;
			case 3:
				new_height = 16;
				break;
			case 4:
				new_height = 17;
				break;
			case 5:
				new_height = 18;
				break;
			case 6:
				new_height = 17;
				break;
			case 7:
				new_height = 16;
				break;
			case 8:
				new_height = 17;
				break;
			case 9:
				new_height = 18;
				break;
			case 10:
				new_height = 17;
				break;
			case 11:
				new_height = 16;
				break;
			case 12:
				new_height = 17;
				break;
			case 13:
				new_height = 16;
				break;
			case 14:
				new_height = 17;
				break;
			case 15:
				new_height = 18;
				break;
			case 16:
				new_height = 17;
				break;
			case 17:
				new_height = 17;
				break;
			case 18:
				new_height = 16;
				break;
			case 19:
				new_height = 16;
				break;
			case 20:
				new_height = 17;
				break;
			case 21:
				new_height = 16;
				break;
			case 22:
				new_height = 15;
				break;
			case 23:
				new_height = 14;
				break;
			}
			int height_difference = cached_source_rect.Height - new_height;
			source_rect.Y += cached_source_rect.Height - new_height;
			source_rect.Height = new_height;
			this.Sprite.SourceRect = source_rect;
			this.Sprite.SpriteHeight = new_height;
			base.yJumpOffset = height_difference;
		}
		base.draw(b, 1f);
		this.Sprite.SpriteHeight = cached_sprite_height;
		this.Sprite.SourceRect = cached_source_rect;
		base.yJumpOffset = cached_y_offset;
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (base.IsInvisible)
		{
			return;
		}
		if (base.IsEmoting && !Game1.eventUp)
		{
			Vector2 emotePosition = base.getLocalPosition(Game1.viewport);
			emotePosition.Y -= 32 + this.Sprite.SpriteHeight * 4 - ((base.Age == 1 || base.Age == 3) ? 64 : 0);
			emotePosition.X += ((base.Age == 1) ? 8 : 0);
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)base.StandingPixel.Y / 10000f);
		}
		bool draw_hat = true;
		if (this.hat.Value == null)
		{
			return;
		}
		Vector2 hatOffset = Vector2.Zero;
		hatOffset *= 4f;
		if (hatOffset.X <= -100f)
		{
			return;
		}
		float horse_draw_layer = (float)base.StandingPixel.Y / 10000f;
		hatOffset.X = -36f;
		hatOffset.Y = -12f;
		if (draw_hat)
		{
			horse_draw_layer += 1E-07f;
			int direction = 2;
			bool flipped = base.sprite.Value.CurrentAnimation != null && base.sprite.Value.CurrentAnimation[base.sprite.Value.currentAnimationIndex].flip;
			switch (this.Sprite.CurrentFrame)
			{
			case 1:
				hatOffset.Y -= 4f;
				direction = 2;
				break;
			case 3:
				hatOffset.Y += 4f;
				direction = 2;
				break;
			case 4:
				direction = 1;
				break;
			case 5:
				hatOffset.Y -= 4f;
				direction = 1;
				break;
			case 6:
				direction = 1;
				break;
			case 7:
				hatOffset.Y += 4f;
				direction = 1;
				break;
			case 20:
				direction = 1;
				break;
			case 21:
				hatOffset.Y += 4f;
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += (flipped ? 1 : (-1)) * 4;
				break;
			case 22:
				hatOffset.Y += 8f;
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += (flipped ? 2 : (-2)) * 4;
				break;
			case 23:
				hatOffset.Y += 12f;
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += (flipped ? 2 : (-2)) * 4;
				break;
			case 8:
				direction = 0;
				break;
			case 9:
				hatOffset.Y -= 4f;
				direction = 0;
				break;
			case 10:
				direction = 0;
				break;
			case 11:
				hatOffset.Y += 4f;
				direction = 0;
				break;
			case 12:
				direction = 3;
				break;
			case 13:
				hatOffset.Y += 4f;
				direction = 3;
				break;
			case 14:
				direction = 3;
				break;
			case 15:
				hatOffset.Y -= 4f;
				direction = 3;
				break;
			case 18:
			case 19:
				hatOffset.Y += 4f;
				direction = 2;
				break;
			}
			if (base.shakeTimer > 0)
			{
				hatOffset += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f), 1.3333334f, 1f, horse_draw_layer, direction);
		}
	}

	public override void behaviorOnLocalFarmerLocationEntry(GameLocation location)
	{
		this.reloadSprite();
	}
}
