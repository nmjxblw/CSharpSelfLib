using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Characters;

public class Horse : NPC
{
	private readonly NetGuid horseId = new NetGuid();

	private readonly NetFarmerRef netRider = new NetFarmerRef();

	public readonly NetLong ownerId = new NetLong();

	[XmlIgnore]
	public readonly NetBool mounting = new NetBool();

	[XmlIgnore]
	public readonly NetBool dismounting = new NetBool();

	private Vector2 dismountTile;

	private int ridingAnimationDirection;

	private bool roomForHorseAtDismountTile;

	[XmlElement("hat")]
	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	public readonly NetMutex mutex = new NetMutex();

	[XmlIgnore]
	public Action<string> onFootstepAction;

	[XmlIgnore]
	public bool ateCarrotToday;

	private bool squeezingThroughGate;

	private int munchingCarrotTimer;

	public Guid HorseId
	{
		get
		{
			return this.horseId.Value;
		}
		set
		{
			this.horseId.Value = value;
		}
	}

	[XmlIgnore]
	public Farmer rider
	{
		get
		{
			return this.netRider.Value;
		}
		set
		{
			this.netRider.Value = value;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public Horse()
	{
		base.willDestroyObjectsUnderfoot = false;
		base.HideShadow = true;
		base.drawOffset = new Vector2(-16f, 0f);
		this.onFootstepAction = PerformDefaultHorseFootstep;
		this.ChooseAppearance();
		this.faceDirection(3);
		base.Breather = false;
	}

	public Horse(Guid horseId, int xTile, int yTile)
		: this()
	{
		base.Name = "";
		this.displayName = base.Name;
		base.Position = new Vector2(xTile, yTile) * 64f;
		base.currentLocation = Game1.currentLocation;
		this.HorseId = horseId;
	}

	public override void reloadData()
	{
	}

	protected override string translateName()
	{
		return base.name.Value.Trim();
	}

	public override bool canTalk()
	{
		return false;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.horseId, "horseId").AddField(this.netRider.NetFields, "netRider.NetFields").AddField(this.mounting, "mounting")
			.AddField(this.dismounting, "dismounting")
			.AddField(this.hat, "hat")
			.AddField(this.mutex.NetFields, "mutex.NetFields")
			.AddField(this.ownerId, "ownerId");
		base.position.Field.AxisAlignedMovement = false;
		base.facingDirection.fieldChangeEvent += delegate
		{
			base.ClearCachedPosition();
		};
	}

	public Farmer getOwner()
	{
		if (this.ownerId.Value == 0L)
		{
			return null;
		}
		return Game1.GetPlayer(this.ownerId.Value);
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (this.Sprite == null)
		{
			this.Sprite = new AnimatedSprite("Animals\\horse", 0, 32, 32);
			this.Sprite.textureUsesFlippedRightForLeft = true;
			this.Sprite.loop = true;
		}
	}

	public override void dayUpdate(int dayOfMonth)
	{
		this.ateCarrotToday = false;
		this.faceDirection(3);
	}

	public override Rectangle GetBoundingBox()
	{
		Rectangle r = base.GetBoundingBox();
		if (this.squeezingThroughGate && (this.FacingDirection == 0 || this.FacingDirection == 2))
		{
			r.Inflate(-36, 0);
		}
		return r;
	}

	public override bool canPassThroughActionTiles()
	{
		return false;
	}

	public void squeezeForGate()
	{
		if (!this.squeezingThroughGate)
		{
			this.squeezingThroughGate = true;
			base.ClearCachedPosition();
		}
		this.rider?.TemporaryPassableTiles.Add(this.GetBoundingBox());
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.currentLocation = location;
		this.mutex.Update(location);
		if (this.squeezingThroughGate)
		{
			this.squeezingThroughGate = false;
			base.ClearCachedPosition();
		}
		base.faceTowardFarmer = false;
		base.faceTowardFarmerTimer = -1;
		this.Sprite.loop = this.rider != null && !this.rider.hidden.Value;
		if (this.rider != null && this.rider.hidden.Value)
		{
			return;
		}
		if (this.munchingCarrotTimer > 0)
		{
			this.munchingCarrotTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this.munchingCarrotTimer <= 0)
			{
				this.mutex.ReleaseLock();
			}
			base.update(time, location);
			return;
		}
		if (this.rider != null && this.rider.isAnimatingMount)
		{
			this.rider.showNotCarrying();
		}
		if (this.mounting.Value)
		{
			if (this.rider == null || !this.rider.IsLocalPlayer)
			{
				return;
			}
			if (this.rider.mount != null)
			{
				this.mounting.Value = false;
				this.rider.isAnimatingMount = false;
				this.rider = null;
				this.Halt();
				base.farmerPassesThrough = false;
				return;
			}
			Rectangle horseBounds = this.GetBoundingBox();
			int anchorX = horseBounds.X + 16;
			if (this.rider.Position.X < (float)(anchorX - 4))
			{
				this.rider.position.X += 4f;
			}
			else if (this.rider.Position.X > (float)(anchorX + 4))
			{
				this.rider.position.X -= 4f;
			}
			int riderStandingY = this.rider.StandingPixel.Y;
			if (riderStandingY < horseBounds.Y - 4)
			{
				this.rider.position.Y += 4f;
			}
			else if (riderStandingY > horseBounds.Y + 4)
			{
				this.rider.position.Y -= 4f;
			}
			if (this.rider.yJumpOffset >= -8 && this.rider.yJumpVelocity <= 0f)
			{
				this.Halt();
				this.Sprite.loop = true;
				base.currentLocation.characters.Remove(this);
				this.rider.mount = this;
				this.rider.freezePause = -1;
				this.mounting.Value = false;
				this.rider.isAnimatingMount = false;
				this.rider.canMove = true;
				if (this.FacingDirection == 1)
				{
					this.rider.xOffset += 8f;
				}
			}
		}
		else if (this.dismounting.Value)
		{
			if (this.rider == null || !this.rider.IsLocalPlayer)
			{
				this.Halt();
				return;
			}
			if (this.rider.isAnimatingMount)
			{
				this.rider.faceDirection(this.FacingDirection);
			}
			Vector2 targetPosition = new Vector2(this.dismountTile.X * 64f + 32f - (float)(this.rider.GetBoundingBox().Width / 2), this.dismountTile.Y * 64f + 4f);
			if (Math.Abs(this.rider.Position.X - targetPosition.X) > 4f)
			{
				if (this.rider.Position.X < targetPosition.X)
				{
					this.rider.position.X += Math.Min(4f, targetPosition.X - this.rider.Position.X);
				}
				else if (this.rider.Position.X > targetPosition.X)
				{
					this.rider.position.X += Math.Max(-4f, targetPosition.X - this.rider.Position.X);
				}
			}
			if (Math.Abs(this.rider.Position.Y - targetPosition.Y) > 4f)
			{
				if (this.rider.Position.Y < targetPosition.Y)
				{
					this.rider.position.Y += Math.Min(4f, targetPosition.Y - this.rider.Position.Y);
				}
				else if (this.rider.Position.Y > targetPosition.Y)
				{
					this.rider.position.Y += Math.Max(-4f, targetPosition.Y - this.rider.Position.Y);
				}
			}
			if (this.rider.yJumpOffset >= 0 && this.rider.yJumpVelocity <= 0f)
			{
				this.rider.position.Y += 8f;
				this.rider.position.X = targetPosition.X;
				int tries = 0;
				while (this.rider.currentLocation.isCollidingPosition(this.rider.GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, this.rider) && tries < 6)
				{
					tries++;
					this.rider.position.Y -= 4f;
				}
				if (tries == 6)
				{
					this.rider.Position = base.Position;
					this.dismounting.Value = false;
					this.rider.isAnimatingMount = false;
					this.rider.freezePause = -1;
					this.rider.canMove = true;
					return;
				}
				this.dismount();
			}
		}
		else if (this.rider == null && this.FacingDirection != 2 && this.Sprite.CurrentAnimation == null && Game1.random.NextDouble() < 0.002)
		{
			this.Sprite.loop = false;
			switch (this.FacingDirection)
			{
			case 0:
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(25, Game1.random.Next(250, 750)),
					new FarmerSprite.AnimationFrame(14, 10)
				});
				break;
			case 1:
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(21, 100),
					new FarmerSprite.AnimationFrame(22, 100),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(24, 400),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(24, 400),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(24, 400),
					new FarmerSprite.AnimationFrame(23, 400),
					new FarmerSprite.AnimationFrame(22, 100),
					new FarmerSprite.AnimationFrame(21, 100)
				});
				break;
			case 3:
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
					new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true)
				});
				break;
			}
		}
		else if (this.rider != null)
		{
			if (this.FacingDirection != this.rider.FacingDirection || this.ridingAnimationDirection != this.FacingDirection)
			{
				this.Sprite.StopAnimation();
				this.faceDirection(this.rider.FacingDirection);
			}
			bool num = (this.rider.movementDirections.Any() && this.rider.CanMove) || this.rider.position.Field.IsInterpolating();
			this.SyncPositionToRider();
			if (!num)
			{
				this.Sprite.StopAnimation();
				this.faceDirection(this.rider.FacingDirection);
			}
			else if (this.Sprite.CurrentAnimation == null)
			{
				switch (this.FacingDirection)
				{
				case 1:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(8, 70),
						new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(12, 70),
						new FarmerSprite.AnimationFrame(13, 70)
					});
					break;
				case 3:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(8, 70, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: true, OnMountFootstep),
						new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: true, OnMountFootstep),
						new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: true, OnMountFootstep),
						new FarmerSprite.AnimationFrame(12, 70, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(13, 70, secondaryArm: false, flip: true)
					});
					break;
				case 0:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(15, 70),
						new FarmerSprite.AnimationFrame(16, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(17, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(18, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(19, 70),
						new FarmerSprite.AnimationFrame(20, 70)
					});
					break;
				case 2:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(1, 70),
						new FarmerSprite.AnimationFrame(2, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(3, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(4, 70, secondaryArm: false, flip: false, OnMountFootstep),
						new FarmerSprite.AnimationFrame(5, 70),
						new FarmerSprite.AnimationFrame(6, 70)
					});
					break;
				}
				this.ridingAnimationDirection = this.FacingDirection;
			}
		}
		if (this.FacingDirection == 3)
		{
			base.drawOffset = Vector2.Zero;
		}
		else
		{
			base.drawOffset = new Vector2(-16f, 0f);
		}
		base.flip = this.FacingDirection == 3;
		base.update(time, location);
	}

	/// <inheritdoc />
	public override void OnLocationRemoved()
	{
		base.OnLocationRemoved();
		if (Game1.IsMasterGame && this.TryFindStable(out var location, out var stable))
		{
			Game1.warpCharacter(this, location, Utility.PointToVector2(stable.GetDefaultHorseTile()));
		}
	}

	public virtual void OnMountFootstep(Farmer who)
	{
		if (this.onFootstepAction != null && this.rider != null)
		{
			string step_type = this.rider.currentLocation.doesTileHaveProperty(this.rider.TilePoint.X, this.rider.TilePoint.Y, "Type", "Back");
			this.onFootstepAction(step_type);
		}
	}

	public virtual void PerformDefaultHorseFootstep(string step_type)
	{
		if (this.rider == null)
		{
			return;
		}
		if (!(step_type == "Stone"))
		{
			if (step_type == "Wood")
			{
				if (this.rider.ShouldHandleAnimationSound())
				{
					this.rider.currentLocation.localSound("woodyStep", base.Tile);
				}
				if (this.rider == Game1.player)
				{
					Rumble.rumble(0.1f, 50f);
				}
			}
			else
			{
				if (this.rider.ShouldHandleAnimationSound())
				{
					this.rider.currentLocation.localSound("thudStep", base.Tile);
				}
				if (this.rider == Game1.player)
				{
					Rumble.rumble(0.3f, 50f);
				}
			}
		}
		else
		{
			if (this.rider.ShouldHandleAnimationSound())
			{
				this.rider.currentLocation.localSound("stoneStep", base.Tile);
			}
			if (this.rider == Game1.player)
			{
				Rumble.rumble(0.1f, 50f);
			}
		}
	}

	public void dismount(bool from_demolish = false)
	{
		this.mutex.ReleaseLock();
		this.rider.mount = null;
		if (base.currentLocation != null)
		{
			if (!from_demolish && this.TryFindStable() != null && !base.currentLocation.characters.Any((NPC c) => c is Horse horse && horse.HorseId == this.HorseId))
			{
				base.currentLocation.characters.Add(this);
			}
			this.SyncPositionToRider();
			this.rider.TemporaryPassableTiles.Add(new Rectangle((int)this.dismountTile.X * 64, (int)this.dismountTile.Y * 64, 64, 64));
			this.rider.freezePause = -1;
			this.dismounting.Value = false;
			this.rider.isAnimatingMount = false;
			this.rider.canMove = true;
			this.rider.forceCanMove();
			this.rider.xOffset = 0f;
			this.rider = null;
			this.Halt();
			base.farmerPassesThrough = false;
		}
	}

	/// <summary>Find the stable which this horse calls home, if it exists.</summary>
	public Stable TryFindStable()
	{
		if (!this.TryFindStable(out var _, out var stable))
		{
			return null;
		}
		return stable;
	}

	/// <summary>Get the home location and stable for this horse, if available.</summary>
	/// <param name="location">The location containing its home stable, if found.</param>
	/// <param name="stable">Its home stable, if found.</param>
	public bool TryFindStable(out GameLocation location, out Stable stable)
	{
		GameLocation foundLocation = null;
		Stable foundStable = null;
		Utility.ForEachLocation(delegate(GameLocation curLocation)
		{
			foreach (Building building in curLocation.buildings)
			{
				if (building is Stable stable2 && stable2.HorseId == this.HorseId && !stable2.isUnderConstruction())
				{
					foundLocation = curLocation;
					foundStable = stable2;
					if (curLocation.IsActiveLocation())
					{
						return false;
					}
				}
			}
			return true;
		});
		location = foundLocation;
		stable = foundStable;
		return stable != null;
	}

	public void nameHorse(string name)
	{
		if (name.Length <= 0)
		{
			return;
		}
		Game1.multiplayer.globalChatInfoMessage("HorseNamed", Game1.player.Name, name);
		Utility.ForEachVillager(delegate(NPC n)
		{
			if (n.Name == name)
			{
				name += " ";
			}
			return true;
		});
		base.Name = name;
		this.displayName = name;
		if (Game1.player.horseName.Value == null)
		{
			Game1.player.horseName.Value = name;
		}
		Game1.exitActiveMenu();
		Game1.playSound("newArtifact");
		if (this.mutex.IsLockHeld())
		{
			this.mutex.ReleaseLock();
		}
	}

	public override bool checkAction(Farmer who, GameLocation l)
	{
		if (who != null && !who.canMove)
		{
			return false;
		}
		if (this.munchingCarrotTimer > 0)
		{
			return false;
		}
		if (this.rider == null)
		{
			this.mutex.RequestLock(delegate
			{
				if (who.mount != null || this.rider != null || who.FarmerSprite.PauseForSingleAnimation || base.currentLocation != who.currentLocation)
				{
					this.mutex.ReleaseLock();
				}
				else
				{
					Stable stable = this.TryFindStable();
					if (stable != null)
					{
						if ((this.getOwner() == Game1.player || (this.getOwner() == null && (string.IsNullOrEmpty(Game1.player.horseName.Value) || Utility.findHorseForPlayer(Game1.player.UniqueMultiplayerID) == null))) && base.Name.Length <= 0)
						{
							stable.owner.Value = who.UniqueMultiplayerID;
							stable.updateHorseOwnership();
							Utility.ForEachBuilding(delegate(Stable curStable)
							{
								if (curStable.owner.Value == who.UniqueMultiplayerID && curStable != stable)
								{
									stable.owner.Value = 0L;
									stable.updateHorseOwnership();
								}
								return true;
							});
							if (string.IsNullOrEmpty(Game1.player.horseName.Value))
							{
								Game1.activeClickableMenu = new NamingMenu(nameHorse, Game1.content.LoadString("Strings\\Characters:NameYourHorse"), Game1.content.LoadString("Strings\\Characters:DefaultHorseName"));
								return;
							}
						}
						else
						{
							if (who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is Hat value)
							{
								if (this.hat.Value != null)
								{
									Game1.createItemDebris(this.hat.Value, base.Position, this.FacingDirection);
									this.hat.Value = null;
								}
								else
								{
									who.Items[who.CurrentToolIndex] = null;
									this.hat.Value = value;
									Game1.playSound("dirtyHit");
								}
								this.mutex.ReleaseLock();
								return;
							}
							if (!this.ateCarrotToday && who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is Object { QualifiedItemId: "(O)Carrot" })
							{
								this.Sprite.StopAnimation();
								this.Sprite.faceDirection(this.FacingDirection);
								Game1.playSound("eat");
								DelayedAction.playSoundAfterDelay("eat", 600);
								DelayedAction.playSoundAfterDelay("eat", 1200);
								this.munchingCarrotTimer = 1500;
								base.doEmote(20, 32);
								who.reduceActiveItemByOne();
								this.ateCarrotToday = true;
								return;
							}
						}
					}
					this.rider = who;
					this.rider.freezePause = 5000;
					this.rider.synchronizedJump(6f);
					this.rider.Halt();
					if (this.rider.Position.X < base.Position.X)
					{
						this.rider.faceDirection(1);
					}
					l.playSound("dwop");
					this.mounting.Value = true;
					this.rider.isAnimatingMount = true;
					this.rider.completelyStopAnimatingOrDoingAction();
					this.rider.faceGeneralDirection(Utility.PointToVector2(base.StandingPixel), 0, opposite: false, useTileCalculations: false);
				}
			});
			return true;
		}
		this.dismounting.Value = true;
		this.rider.isAnimatingMount = true;
		base.farmerPassesThrough = false;
		this.rider.TemporaryPassableTiles.Clear();
		Vector2 position = Utility.recursiveFindOpenTileForCharacter(this.rider, this.rider.currentLocation, base.Tile, 8);
		base.Position = new Vector2(position.X * 64f + 32f - (float)(this.GetBoundingBox().Width / 2), position.Y * 64f + 4f);
		this.roomForHorseAtDismountTile = !base.currentLocation.isCollidingPosition(this.GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, this);
		base.Position = this.rider.Position;
		this.dismounting.Value = false;
		this.rider.isAnimatingMount = false;
		this.Halt();
		if (!position.Equals(Vector2.Zero) && Vector2.Distance(position, base.Tile) < 2f)
		{
			this.rider.synchronizedJump(6f);
			l.playSound("dwop");
			this.rider.freezePause = 5000;
			this.rider.Halt();
			this.rider.xOffset = 0f;
			this.dismounting.Value = true;
			this.rider.isAnimatingMount = true;
			this.dismountTile = position;
		}
		else
		{
			this.dismount();
		}
		return true;
	}

	public void SyncPositionToRider()
	{
		if (this.rider != null && (!this.dismounting.Value || this.roomForHorseAtDismountTile))
		{
			base.Position = this.rider.Position;
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.flip = this.FacingDirection == 3;
		this.Sprite.UpdateSourceRect();
		base.draw(b);
		if (this.FacingDirection == 2 && this.rider != null)
		{
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(48f, -24f - this.rider.yOffset), new Rectangle(160, 96, 9, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f);
		}
		bool draw_hat = true;
		Vector2 hatOffset;
		string itemId;
		if (this.hat.Value != null)
		{
			hatOffset = Vector2.Zero;
			itemId = this.hat.Value.ItemId;
			if (itemId != null)
			{
				int length = itemId.Length;
				if (length != 1)
				{
					if (length == 2)
					{
						switch (itemId[1])
						{
						case '4':
							if (itemId == "14" && this.FacingDirection == 0)
							{
								hatOffset.X = -100f;
							}
							goto IL_033c;
						case '0':
							if (itemId == "10")
							{
								hatOffset.Y += 3f;
								if (this.FacingDirection == 0)
								{
									draw_hat = false;
								}
							}
							goto IL_033c;
						case '2':
							break;
						case '1':
							goto IL_0195;
						case '9':
							goto IL_01ba;
						case '6':
							if (itemId == "26")
							{
								goto IL_02f4;
							}
							if (itemId == "56")
							{
								goto IL_0332;
							}
							goto IL_033c;
						case '7':
							if (itemId == "67")
							{
								goto IL_0332;
							}
							goto IL_033c;
						default:
							goto IL_033c;
							IL_0332:
							if (this.FacingDirection == 0)
							{
								draw_hat = false;
							}
							goto IL_033c;
						}
						if (itemId == "32")
						{
							goto IL_0278;
						}
					}
				}
				else
				{
					char c = itemId[0];
					if (c != '6')
					{
						if (c == '9')
						{
							goto IL_0278;
						}
					}
					else
					{
						hatOffset.Y += 2f;
						if (this.FacingDirection == 2)
						{
							hatOffset.Y -= 1f;
						}
					}
				}
			}
			goto IL_033c;
		}
		if (this.munchingCarrotTimer > 0)
		{
			switch (this.FacingDirection)
			{
			case 2:
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(24f, -24f), new Rectangle(170 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 112, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f + 1E-07f);
				break;
			case 1:
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(80f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f + 1E-07f);
				break;
			case 3:
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(-16f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (base.Position.Y + 64f) / 10000f + 1E-07f);
				break;
			}
		}
		return;
		IL_02b6:
		if (this.FacingDirection == 3 || this.FacingDirection == 1)
		{
			if (base.flip)
			{
				hatOffset.X += 2f;
			}
			else
			{
				hatOffset.X -= 2f;
			}
		}
		goto IL_033c;
		IL_02f4:
		if (this.FacingDirection == 3 || this.FacingDirection == 1)
		{
			if (base.flip)
			{
				hatOffset.X += 1f;
			}
			else
			{
				hatOffset.X -= 1f;
			}
		}
		goto IL_033c;
		IL_01ba:
		if (itemId == "39")
		{
			goto IL_02b6;
		}
		goto IL_033c;
		IL_033c:
		hatOffset *= 4f;
		if (base.shakeTimer > 0)
		{
			hatOffset += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
		}
		if (hatOffset.X <= -100f)
		{
			return;
		}
		float horse_draw_layer = (float)base.StandingPixel.Y / 10000f;
		if (this.rider != null)
		{
			if (this.FacingDirection == 2)
			{
				horse_draw_layer = (base.position.Y + 64f + 1f) / 10000f;
			}
			else if (this.FacingDirection != 0)
			{
				horse_draw_layer = (base.position.Y + 48f - 1f) / 10000f;
			}
		}
		if (this.munchingCarrotTimer > 0)
		{
			switch (this.FacingDirection)
			{
			case 2:
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(24f, -24f), new Rectangle(170 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 112, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, horse_draw_layer + 1E-07f);
				break;
			case 1:
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(80f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, horse_draw_layer + 1E-07f);
				break;
			case 3:
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(-16f, -56f), new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, horse_draw_layer + 1E-07f);
				break;
			}
		}
		if (!draw_hat)
		{
			return;
		}
		horse_draw_layer += 2E-07f;
		switch (this.Sprite.CurrentFrame)
		{
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
			this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f - ((this.rider != null) ? this.rider.yOffset : 0f))), 1.3333334f, 1f, horse_draw_layer, 2);
			break;
		case 7:
		case 11:
			if (base.flip)
			{
				this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -74f), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -74f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 8:
			if (base.flip)
			{
				this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -74f), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -74f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 9:
			if (base.flip)
			{
				this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -70f), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -70f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 10:
			if (base.flip)
			{
				this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -70f), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -70f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 12:
			if (base.flip)
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -78f)), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -78f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 13:
			if (base.flip)
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -78f)), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -78f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 21:
			if (base.flip)
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -66f)), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -66f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 22:
			if (base.flip)
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -54f)), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -54f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 23:
			if (base.flip)
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 24:
			if (base.flip)
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.3333334f, 1f, horse_draw_layer, 3);
			}
			else
			{
				this.hat.Value.draw(b, Utility.snapDrawPosition(base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.3333334f, 1f, horse_draw_layer, 1);
			}
			break;
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 25:
			this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(28f, -106f - ((this.rider != null) ? this.rider.yOffset : 0f)), 1.3333334f, 1f, horse_draw_layer, 0);
			break;
		}
		return;
		IL_0195:
		if (!(itemId == "31"))
		{
			if (itemId == "11")
			{
				goto IL_02b6;
			}
		}
		else
		{
			hatOffset.Y += 1f;
		}
		goto IL_033c;
		IL_0278:
		if (this.FacingDirection == 0 || this.FacingDirection == 2)
		{
			hatOffset.Y += 1f;
		}
		goto IL_033c;
	}
}
