using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;

namespace StardewValley.Characters;

public class JunimoHarvester : NPC
{
	protected float alpha = 1f;

	protected float alphaChange;

	protected Vector2 motion = Vector2.Zero;

	protected new Rectangle nextPosition;

	protected readonly NetColor color = new NetColor();

	protected bool destroy;

	protected Item lastItemHarvested;

	public int whichJunimoFromThisHut;

	protected int harvestTimer;

	public readonly NetBool isPrismatic = new NetBool(value: false);

	protected readonly NetGuid netHome = new NetGuid();

	protected readonly NetEvent1Field<int, NetInt> netAnimationEvent = new NetEvent1Field<int, NetInt>();

	public Guid HomeId
	{
		get
		{
			return this.netHome.Value;
		}
		set
		{
			this.netHome.Value = value;
		}
	}

	[XmlIgnore]
	public JunimoHut home
	{
		get
		{
			if (!base.currentLocation.buildings.TryGetValue(this.netHome.Value, out var building))
			{
				return null;
			}
			return building as JunimoHut;
		}
		set
		{
			this.netHome.Value = base.currentLocation.buildings.GuidOf(value);
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public JunimoHarvester()
	{
	}

	public JunimoHarvester(GameLocation location, Vector2 position, JunimoHut hut, int whichJunimoNumberFromThisHut, Color? c)
		: base(new AnimatedSprite("Characters\\Junimo", 0, 16, 16), position, 2, "Junimo")
	{
		base.currentLocation = location;
		this.home = hut;
		this.whichJunimoFromThisHut = whichJunimoNumberFromThisHut;
		if (!c.HasValue)
		{
			this.pickColor();
		}
		else
		{
			this.color.Value = c.Value;
		}
		this.nextPosition = this.GetBoundingBox();
		base.Breather = false;
		base.speed = 3;
		base.forceUpdateTimer = 9999;
		base.collidesWithOtherCharacters.Value = true;
		base.ignoreMovementAnimation = true;
		base.farmerPassesThrough = true;
		base.Scale = 0.75f;
		base.willDestroyObjectsUnderfoot = false;
		Vector2 tileToPathfindTo = Vector2.Zero;
		switch (whichJunimoNumberFromThisHut)
		{
		case 0:
			tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2(hut.tileX.Value + 1, hut.tileY.Value + hut.tilesHigh.Value + 1), 30);
			break;
		case 1:
			tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2(hut.tileX.Value - 1, hut.tileY.Value), 30);
			break;
		case 2:
			tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2(hut.tileX.Value + hut.tilesWide.Value, hut.tileY.Value), 30);
			break;
		}
		if (tileToPathfindTo != Vector2.Zero)
		{
			base.controller = new PathFindController(this, base.currentLocation, Utility.Vector2ToPoint(tileToPathfindTo), -1, reachFirstDestinationFromHut, 100);
		}
		if (base.controller?.pathToEndPoint == null && Game1.IsMasterGame)
		{
			this.pathfindToRandomSpotAroundHut();
			if (base.controller?.pathToEndPoint == null)
			{
				this.destroy = true;
			}
		}
		base.collidesWithOtherCharacters.Value = false;
	}

	protected virtual void pickColor()
	{
		JunimoHut hut = this.home;
		if (hut == null)
		{
			this.color.Value = Color.White;
			return;
		}
		Random r = Utility.CreateRandom(hut.tileX.Value, (double)hut.tileY.Value * 777.0, this.whichJunimoFromThisHut);
		if (r.NextBool(0.25))
		{
			if (r.NextBool(0.01))
			{
				this.color.Value = Color.White;
				return;
			}
			switch (r.Next(8))
			{
			case 0:
				this.color.Value = Color.Red;
				break;
			case 1:
				this.color.Value = Color.Goldenrod;
				break;
			case 2:
				this.color.Value = Color.Yellow;
				break;
			case 3:
				this.color.Value = Color.Lime;
				break;
			case 4:
				this.color.Value = new Color(0, 255, 180);
				break;
			case 5:
				this.color.Value = new Color(0, 100, 255);
				break;
			case 6:
				this.color.Value = Color.MediumPurple;
				break;
			default:
				this.color.Value = Color.Salmon;
				break;
			}
		}
		else
		{
			switch (r.Next(8))
			{
			case 0:
				this.color.Value = Color.LimeGreen;
				break;
			case 1:
				this.color.Value = Color.Orange;
				break;
			case 2:
				this.color.Value = Color.LightGreen;
				break;
			case 3:
				this.color.Value = Color.Tan;
				break;
			case 4:
				this.color.Value = Color.GreenYellow;
				break;
			case 5:
				this.color.Value = Color.LawnGreen;
				break;
			case 6:
				this.color.Value = Color.PaleGreen;
				break;
			default:
				this.color.Value = Color.Turquoise;
				break;
			}
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.color, "color").AddField(this.netHome.NetFields, "netHome.NetFields").AddField(this.netAnimationEvent, "netAnimationEvent")
			.AddField(this.isPrismatic, "isPrismatic");
		this.netAnimationEvent.onEvent += doAnimationEvent;
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (this.Sprite == null)
		{
			this.Sprite = new AnimatedSprite(content ?? Game1.content, "Characters\\Junimo");
		}
	}

	protected virtual void doAnimationEvent(int animId)
	{
		switch (animId)
		{
		case 0:
			this.Sprite.CurrentAnimation = null;
			break;
		case 2:
			this.Sprite.currentFrame = 0;
			break;
		case 3:
			this.Sprite.currentFrame = 1;
			break;
		case 4:
			this.Sprite.currentFrame = 2;
			break;
		case 5:
			this.Sprite.currentFrame = 44;
			break;
		case 6:
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(12, 200),
				new FarmerSprite.AnimationFrame(13, 200),
				new FarmerSprite.AnimationFrame(14, 200),
				new FarmerSprite.AnimationFrame(15, 200)
			});
			break;
		case 7:
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(44, 200),
				new FarmerSprite.AnimationFrame(45, 200),
				new FarmerSprite.AnimationFrame(46, 200),
				new FarmerSprite.AnimationFrame(47, 200)
			});
			break;
		case 8:
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(28, 100),
				new FarmerSprite.AnimationFrame(29, 100),
				new FarmerSprite.AnimationFrame(30, 100),
				new FarmerSprite.AnimationFrame(31, 100)
			});
			break;
		case 1:
			break;
		}
	}

	public virtual void reachFirstDestinationFromHut(Character c, GameLocation l)
	{
		this.tryToHarvestHere();
	}

	public virtual void tryToHarvestHere()
	{
		if (base.currentLocation != null)
		{
			if (this.isHarvestable())
			{
				this.harvestTimer = 2000;
			}
			else
			{
				this.pokeToHarvest();
			}
		}
	}

	public virtual void pokeToHarvest()
	{
		JunimoHut hut = this.home;
		if (hut != null)
		{
			if (!hut.isTilePassable(base.Tile) && Game1.IsMasterGame)
			{
				this.destroy = true;
			}
			else if (this.harvestTimer <= 0 && Game1.random.NextDouble() < 0.7)
			{
				this.pathfindToNewCrop();
			}
		}
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public void setMoving(int xSpeed, int ySpeed)
	{
		this.motion.X = xSpeed;
		this.motion.Y = ySpeed;
	}

	public void setMoving(Vector2 motion)
	{
		this.motion = motion;
	}

	public override void Halt()
	{
		base.Halt();
		this.motion = Vector2.Zero;
	}

	public override bool canTalk()
	{
		return false;
	}

	public void junimoReachedHut(Character c, GameLocation l)
	{
		base.controller = null;
		this.motion.X = 0f;
		this.motion.Y = -1f;
		this.destroy = true;
	}

	public virtual bool foundCropEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
	{
		if (location.terrainFeatures.TryGetValue(new Vector2(currentNode.x, currentNode.y), out var terrainFeature))
		{
			if (location.isCropAtTile(currentNode.x, currentNode.y) && (terrainFeature as HoeDirt).readyForHarvest())
			{
				return true;
			}
			if (terrainFeature is Bush bush && bush.readyForHarvest())
			{
				return true;
			}
		}
		return false;
	}

	public virtual void pathfindToNewCrop()
	{
		JunimoHut hut = this.home;
		if (hut == null)
		{
			return;
		}
		if (Game1.timeOfDay > 1900)
		{
			if (base.controller == null)
			{
				this.returnToJunimoHut(base.currentLocation);
			}
			return;
		}
		if (Game1.random.NextDouble() < 0.035 || hut.noHarvest.Value)
		{
			this.pathfindToRandomSpotAroundHut();
			return;
		}
		base.controller = new PathFindController(this, base.currentLocation, foundCropEndFunction, -1, reachFirstDestinationFromHut, 100, Point.Zero);
		Point? endpoint = base.controller.pathToEndPoint?.Last();
		if (!endpoint.HasValue || Math.Abs(endpoint.Value.X - (hut.tileX.Value + 1)) > hut.cropHarvestRadius || Math.Abs(endpoint.Value.Y - (hut.tileY.Value + 1)) > hut.cropHarvestRadius)
		{
			if (Game1.random.NextBool() && !hut.lastKnownCropLocation.Equals(Point.Zero))
			{
				base.controller = new PathFindController(this, base.currentLocation, hut.lastKnownCropLocation, -1, reachFirstDestinationFromHut, 100);
			}
			else if (Game1.random.NextDouble() < 0.25)
			{
				this.netAnimationEvent.Fire(0);
				this.returnToJunimoHut(base.currentLocation);
			}
			else
			{
				this.pathfindToRandomSpotAroundHut();
			}
		}
		else
		{
			this.netAnimationEvent.Fire(0);
		}
	}

	public virtual void returnToJunimoHut(GameLocation location)
	{
		if (Utility.isOnScreen(Utility.Vector2ToPoint(base.position.Value / 64f), 64, base.currentLocation))
		{
			this.jump();
		}
		base.collidesWithOtherCharacters.Value = false;
		if (Game1.IsMasterGame)
		{
			JunimoHut hut = this.home;
			if (hut == null)
			{
				return;
			}
			base.controller = new PathFindController(this, location, new Point(hut.tileX.Value + 1, hut.tileY.Value + 1), 0, junimoReachedHut);
			if (base.controller.pathToEndPoint == null || base.controller.pathToEndPoint.Count == 0 || location.isCollidingPosition(this.nextPosition, Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				this.destroy = true;
			}
		}
		if (Utility.isOnScreen(Utility.Vector2ToPoint(base.position.Value / 64f), 64, base.currentLocation))
		{
			location.playSound("junimoMeep1");
		}
	}

	public override void faceDirection(int direction)
	{
	}

	protected override void updateSlaveAnimation(GameTime time)
	{
	}

	protected virtual bool isHarvestable()
	{
		if (base.currentLocation.terrainFeatures.TryGetValue(base.Tile, out var terrainFeature))
		{
			if (terrainFeature is HoeDirt dirt)
			{
				return dirt.readyForHarvest();
			}
			if (terrainFeature is Bush bush)
			{
				return bush.readyForHarvest();
			}
		}
		return false;
	}

	public override void update(GameTime time, GameLocation location)
	{
		this.netAnimationEvent.Poll();
		base.update(time, location);
		if (this.isPrismatic.Value)
		{
			this.color.Value = Utility.GetPrismaticColor(this.whichJunimoFromThisHut);
		}
		base.forceUpdateTimer = 99999;
		if (this.EventActor)
		{
			return;
		}
		if (this.destroy)
		{
			this.alphaChange = -0.05f;
		}
		this.alpha += this.alphaChange;
		if (this.alpha > 1f)
		{
			this.alpha = 1f;
		}
		else if (this.alpha < 0f)
		{
			this.alpha = 0f;
			if (this.destroy && Game1.IsMasterGame)
			{
				location.characters.Remove(this);
				this.home?.myJunimos.Remove(this);
			}
		}
		if (Game1.IsMasterGame)
		{
			if (this.harvestTimer > 0)
			{
				int oldTimer = this.harvestTimer;
				this.harvestTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.harvestTimer > 1800)
				{
					this.netAnimationEvent.Fire(2);
				}
				else if (this.harvestTimer > 1600)
				{
					this.netAnimationEvent.Fire(3);
				}
				else if (this.harvestTimer > 1000)
				{
					this.netAnimationEvent.Fire(4);
					base.shake(50);
				}
				else if (oldTimer >= 1000 && this.harvestTimer < 1000)
				{
					this.netAnimationEvent.Fire(2);
					JunimoHut hut = this.home;
					if (base.currentLocation != null && hut != null && !hut.noHarvest.Value && this.isHarvestable())
					{
						this.netAnimationEvent.Fire(5);
						this.lastItemHarvested = null;
						TerrainFeature terrainFeature = base.currentLocation.terrainFeatures[base.Tile];
						if (!(terrainFeature is Bush bush))
						{
							if (terrainFeature is HoeDirt dirt && dirt.crop.harvest(base.TilePoint.X, base.TilePoint.Y, dirt, this))
							{
								dirt.destroyCrop(base.currentLocation.farmers.Any());
							}
						}
						else if (bush.readyForHarvest())
						{
							this.tryToAddItemToHut(ItemRegistry.Create("(O)815"));
							bush.tileSheetOffset.Value = 0;
							bush.setUpSourceRect();
							if (Utility.isOnScreen(base.TilePoint, 64, base.currentLocation))
							{
								bush.performUseAction(base.Tile);
							}
							if (Utility.isOnScreen(base.TilePoint, 64, base.currentLocation))
							{
								DelayedAction.playSoundAfterDelay("coin", 260, base.currentLocation);
							}
						}
						if (this.lastItemHarvested != null)
						{
							bool gotDouble = false;
							if (this.home.raisinDays.Value > 0 && Game1.random.NextDouble() < 0.2)
							{
								gotDouble = true;
								Item i = this.lastItemHarvested.getOne();
								i.Quality = this.lastItemHarvested.Quality;
								this.tryToAddItemToHut(i);
							}
							if (base.currentLocation.farmers.Any())
							{
								ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this.lastItemHarvested.QualifiedItemId);
								float mainDrawLayer = (float)base.StandingPixel.Y / 10000f + 0.01f;
								if (gotDouble)
								{
									for (int j = 0; j < 2; j++)
									{
										Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(), 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer, 0.02f, Color.White, 4f, -0.01f, 0f, 0f)
										{
											motion = new Vector2((float)((j != 0) ? 1 : (-1)) * 0.5f, -0.25f),
											delayBeforeAnimationStart = 200
										});
										if (this.lastItemHarvested is ColoredObject coloredObj2)
										{
											Rectangle colored_source_rect = ItemRegistry.GetDataOrErrorItem(this.lastItemHarvested.QualifiedItemId).GetSourceRect(1);
											Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, colored_source_rect, 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer + 0.005f, 0.02f, coloredObj2.color.Value, 4f, -0.01f, 0f, 0f)
											{
												motion = new Vector2((float)((j != 0) ? 1 : (-1)) * 0.5f, -0.25f),
												delayBeforeAnimationStart = 200
											});
										}
									}
								}
								else
								{
									Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(), 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer, 0.02f, Color.White, 4f, -0.01f, 0f, 0f)
									{
										motion = new Vector2(0.08f, -0.25f)
									});
									if (this.lastItemHarvested is ColoredObject coloredObj3)
									{
										Rectangle colored_source_rect2 = ItemRegistry.GetDataOrErrorItem(this.lastItemHarvested.QualifiedItemId).GetSourceRect(1);
										Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(itemData.TextureName, colored_source_rect2, 1000f, 1, 0, base.Position + new Vector2(0f, -40f), flicker: false, flipped: false, mainDrawLayer + 0.005f, 0.02f, coloredObj3.color.Value, 4f, -0.01f, 0f, 0f)
										{
											motion = new Vector2(0.08f, -0.25f)
										});
									}
								}
							}
						}
					}
				}
				else if (this.harvestTimer <= 0)
				{
					this.pokeToHarvest();
				}
			}
			else if (this.alpha > 0f && base.controller == null)
			{
				if ((this.addedSpeed > 0f || base.speed > 3 || base.isCharging) && Game1.IsMasterGame)
				{
					this.destroy = true;
				}
				this.nextPosition = this.GetBoundingBox();
				this.nextPosition.X += (int)this.motion.X;
				bool sparkle = false;
				if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this))
				{
					base.position.X += (int)this.motion.X;
					sparkle = true;
				}
				this.nextPosition.X -= (int)this.motion.X;
				this.nextPosition.Y += (int)this.motion.Y;
				if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this))
				{
					base.position.Y += (int)this.motion.Y;
					sparkle = true;
				}
				if (!this.motion.Equals(Vector2.Zero) && sparkle && Game1.random.NextDouble() < 0.005)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(Game1.random.Choose(10, 11), base.Position, this.color.Value)
					{
						motion = this.motion / 4f,
						alphaFade = 0.01f,
						layerDepth = 0.8f,
						scale = 0.75f,
						alpha = 0.75f
					});
				}
				if (Game1.random.NextDouble() < 0.002)
				{
					switch (Game1.random.Next(6))
					{
					case 0:
						this.netAnimationEvent.Fire(6);
						break;
					case 1:
						this.netAnimationEvent.Fire(7);
						break;
					case 2:
						this.netAnimationEvent.Fire(0);
						break;
					case 3:
						this.jumpWithoutSound();
						base.yJumpVelocity /= 2f;
						this.netAnimationEvent.Fire(0);
						break;
					case 4:
					{
						JunimoHut hut2 = this.home;
						if (hut2 != null && !hut2.noHarvest.Value)
						{
							this.pathfindToNewCrop();
						}
						break;
					}
					case 5:
						this.netAnimationEvent.Fire(8);
						break;
					}
				}
			}
		}
		bool moveRight = base.moveRight;
		bool moveLeft = base.moveLeft;
		bool moveUp = base.moveUp;
		bool moveDown = base.moveDown;
		if (Game1.IsMasterGame)
		{
			if (base.controller == null && this.motion.Equals(Vector2.Zero))
			{
				return;
			}
			moveRight |= Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X > 0f;
			moveLeft |= Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X < 0f;
			moveUp |= Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y < 0f;
			moveDown |= Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y > 0f;
		}
		else
		{
			moveLeft = base.IsRemoteMoving() && this.FacingDirection == 3;
			moveRight = base.IsRemoteMoving() && this.FacingDirection == 1;
			moveUp = base.IsRemoteMoving() && this.FacingDirection == 0;
			moveDown = base.IsRemoteMoving() && this.FacingDirection == 2;
			if (!moveRight && !moveLeft && !moveUp && !moveDown)
			{
				return;
			}
		}
		this.Sprite.CurrentAnimation = null;
		if (moveRight)
		{
			base.flip = false;
			if (this.Sprite.Animate(time, 16, 8, 50f))
			{
				this.Sprite.currentFrame = 16;
			}
		}
		else if (moveLeft)
		{
			if (this.Sprite.Animate(time, 16, 8, 50f))
			{
				this.Sprite.currentFrame = 16;
			}
			base.flip = true;
		}
		else if (moveUp)
		{
			if (this.Sprite.Animate(time, 32, 8, 50f))
			{
				this.Sprite.currentFrame = 32;
			}
		}
		else if (moveDown)
		{
			this.Sprite.Animate(time, 0, 8, 50f);
		}
	}

	public virtual void pathfindToRandomSpotAroundHut()
	{
		JunimoHut hut = this.home;
		if (hut != null)
		{
			base.controller = new PathFindController(endPoint: Utility.Vector2ToPoint(new Vector2(hut.tileX.Value + 1 + Game1.random.Next(-hut.cropHarvestRadius, hut.cropHarvestRadius + 1), hut.tileY.Value + 1 + Game1.random.Next(-hut.cropHarvestRadius, hut.cropHarvestRadius + 1))), c: this, location: base.currentLocation, finalFacingDirection: -1, endBehaviorFunction: reachFirstDestinationFromHut, limit: 100);
		}
	}

	public virtual void tryToAddItemToHut(Item i)
	{
		this.lastItemHarvested = i;
		Item result = this.home?.GetOutputChest().addItem(i);
		if (result != null)
		{
			for (int j = 0; j < result.Stack; j++)
			{
				Game1.createItemDebris(i.getOne(), base.Position, -1, base.currentLocation);
			}
		}
	}

	public override void draw(SpriteBatch b, float alpha = 1f)
	{
		if (this.alpha > 0f)
		{
			float mainDrawLayer = (float)(base.StandingPixel.Y + 2) / 10000f;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)this.Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow(this.Sprite.SpriteHeight / 16, 2.0) + (float)base.yJumpOffset - 8f) + ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), this.Sprite.SourceRect, this.color.Value * this.alpha, base.rotation, new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)(this.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : mainDrawLayer));
			if (!base.swimming.Value)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2((float)(this.Sprite.SpriteWidth * 4) / 2f, 44f)), Game1.shadowTexture.Bounds, this.color.Value * this.alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)base.yJumpOffset / 40f) * base.scale.Value, SpriteEffects.None, Math.Max(0f, mainDrawLayer) - 1E-06f);
			}
		}
	}
}
