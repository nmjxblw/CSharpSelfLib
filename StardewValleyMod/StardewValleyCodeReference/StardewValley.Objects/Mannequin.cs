using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Delegates;
using StardewValley.GameData;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.Objects;

/// <summary>A mannequin which can be placed in the world and used to store and display clothing.</summary>
public class Mannequin : Object
{
	protected string _description;

	protected MannequinData _data;

	public string displayNameOverride;

	public readonly NetMutex changeMutex = new NetMutex();

	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	public readonly NetRef<Clothing> shirt = new NetRef<Clothing>();

	public readonly NetRef<Clothing> pants = new NetRef<Clothing>();

	public readonly NetRef<Boots> boots = new NetRef<Boots>();

	public readonly NetDirection facing = new NetDirection();

	public readonly NetBool swappedWithFarmerTonight = new NetBool();

	private Farmer renderCache;

	internal int eyeTimer;

	public override string TypeDefinitionId { get; } = "(M)";

	public Mannequin()
	{
	}

	public Mannequin(string itemId)
		: this()
	{
		base.ItemId = itemId;
		base.name = itemId;
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
		base.ParentSheetIndex = data.SpriteIndex;
		base.bigCraftable.Value = true;
		base.canBeSetDown.Value = true;
		base.setIndoors.Value = true;
		base.setOutdoors.Value = true;
		base.Type = "interactive";
		this.facing.Value = 2;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.changeMutex.NetFields, "changeMutex.NetFields").AddField(this.hat, "hat").AddField(this.shirt, "shirt")
			.AddField(this.pants, "pants")
			.AddField(this.boots, "boots")
			.AddField(this.facing, "facing")
			.AddField(this.swappedWithFarmerTonight, "swappedWithFarmerTonight");
		this.hat.fieldChangeVisibleEvent += OnMannequinUpdated;
		this.shirt.fieldChangeVisibleEvent += OnMannequinUpdated;
		this.pants.fieldChangeVisibleEvent += OnMannequinUpdated;
		this.boots.fieldChangeVisibleEvent += OnMannequinUpdated;
	}

	private void OnMannequinUpdated<TNetField, TValue>(TNetField field, TValue oldValue, TValue newValue)
	{
		this.renderCache = null;
	}

	protected internal MannequinData GetMannequinData()
	{
		if (this._data == null)
		{
			this._data = DataLoader.Mannequins(Game1.content).GetValueOrDefault(base.ItemId);
		}
		return this._data;
	}

	protected override string loadDisplayName()
	{
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.ItemId);
		if (this.displayNameOverride == null)
		{
			return data.DisplayName;
		}
		return this.displayNameOverride;
	}

	public override string getDescription()
	{
		if (this._description == null)
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.ItemId);
			this._description = Game1.parseText(TokenParser.ParseText(data.Description), Game1.smallFont, this.getDescriptionWidth());
		}
		return this._description;
	}

	public override bool isPlaceable()
	{
		return true;
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
	{
		if (base.ForEachItem(handler, getPath) && ForEachItemHelper.ApplyToField(this.hat, handler, getPath) && ForEachItemHelper.ApplyToField(this.shirt, handler, getPath) && ForEachItemHelper.ApplyToField(this.pants, handler, getPath))
		{
			return ForEachItemHelper.ApplyToField(this.boots, handler, getPath);
		}
		return false;
	}

	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		Vector2 placementTile = new Vector2(x / 64, y / 64);
		Mannequin toPlace = base.getOne() as Mannequin;
		location.Objects.Add(placementTile, toPlace);
		location.playSound("woodyStep");
		return true;
	}

	private void emitGhost()
	{
		this.Location.temporarySprites.Add(new TemporaryAnimatedSprite(this.GetMannequinData().Texture, new Rectangle((!(Game1.random.NextDouble() < 0.5)) ? 64 : 0, 64, 16, 32), this.TileLocation * 64f + new Vector2(0f, -1f) * 64f, flipped: false, 0.004f, Color.White)
		{
			scale = 4f,
			layerDepth = 1f,
			motion = new Vector2(7 + Game1.random.Next(-1, 6), -8 + Game1.random.Next(-1, 5)),
			acceleration = new Vector2(-0.4f + (float)Game1.random.Next(10) / 100f, 0f),
			animationLength = 4,
			totalNumberOfLoops = 99,
			interval = 80f,
			scaleChangeChange = 0.01f
		});
		this.Location.playSound("cursed_mannequin");
	}

	/// <inheritdoc />
	public override bool minutesElapsed(int minutes)
	{
		if (Game1.random.NextDouble() < 0.001 && this.GetMannequinData().Cursed)
		{
			if (Game1.timeOfDay > Game1.getTrulyDarkTime(this.Location) && Game1.random.NextDouble() < 0.1)
			{
				this.emitGhost();
			}
			else if (Game1.random.NextDouble() < 0.66)
			{
				if (Game1.random.NextDouble() < 0.5)
				{
					foreach (Farmer f in this.Location.farmers)
					{
						this.facing.Value = Utility.GetOppositeFacingDirection(Utility.getDirectionFromChange(this.TileLocation, f.Tile));
						this.renderCache = null;
					}
				}
				else
				{
					this.eyeTimer = 2500;
				}
			}
			else
			{
				this.Location.playSound("cursed_mannequin");
				base.shakeTimer = Game1.random.Next(500, 4000);
			}
		}
		return base.minutesElapsed(minutes);
	}

	public override void actionOnPlayerEntry()
	{
		if (Game1.random.NextDouble() < 0.001 && this.GetMannequinData().Cursed)
		{
			base.shakeTimer = Game1.random.Next(500, 1000);
		}
		base.actionOnPlayerEntry();
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		if (Game1.IsMasterGame && this.GetMannequinData().Cursed && this.Location != null && (this.Location is FarmHouse || this.Location is IslandFarmHouse || this.Location is Shed))
		{
			if (Game1.random.NextDouble() < 0.05)
			{
				Vector2 oldTile = this.TileLocation;
				Utility.spawnObjectAround(this.TileLocation, this, this.Location, playSound: false, delegate
				{
					if (!this.TileLocation.Equals(oldTile))
					{
						this.Location.objects.Remove(oldTile);
					}
				});
			}
			else if (this.swappedWithFarmerTonight.Value)
			{
				this.swappedWithFarmerTonight.Value = false;
			}
			else
			{
				if (Game1.random.NextDouble() < 0.005)
				{
					if (this.Location.farmers.Count <= 0)
					{
						return;
					}
					using FarmerCollection.Enumerator enumerator = this.Location.farmers.GetEnumerator();
					if (enumerator.MoveNext())
					{
						Farmer who = enumerator.Current;
						Vector2 oldTile2 = this.TileLocation;
						Vector2 bedTile = who.mostRecentBed / 64f;
						bedTile.X = (int)bedTile.X;
						bedTile.Y = (int)bedTile.Y;
						if (Utility.spawnObjectAround(bedTile, this, this.Location, playSound: false, delegate
						{
							if (!this.TileLocation.Equals(oldTile2))
							{
								this.Location.objects.Remove(oldTile2);
							}
						}))
						{
							this.facing.Value = Utility.GetOppositeFacingDirection(Utility.getDirectionFromChange(this.TileLocation, who.Tile));
							this.renderCache = null;
							this.eyeTimer = 2000;
						}
					}
					return;
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					DecoratableLocation dec_location = this.Location as DecoratableLocation;
					string floorID = dec_location.GetFloorID((int)this.TileLocation.X, (int)this.TileLocation.Y);
					string wallpaperID = null;
					for (int y = (int)this.TileLocation.Y; y > 0; y--)
					{
						wallpaperID = dec_location.GetWallpaperID((int)this.TileLocation.X, y);
						if (wallpaperID != null)
						{
							break;
						}
					}
					if (floorID != null)
					{
						dec_location.SetFloor("MoreFloors:6", floorID);
					}
					if (wallpaperID != null)
					{
						dec_location.SetWallpaper("MoreWalls:21", wallpaperID);
					}
					base.shakeTimer = 10000;
				}
				else
				{
					if (!(Game1.random.NextDouble() < 0.02))
					{
						return;
					}
					DecoratableLocation dec_location2 = this.Location as DecoratableLocation;
					if (Game1.random.NextDouble() < 0.33)
					{
						for (int i = 0; i < 30; i++)
						{
							int xPos = Game1.random.Next(2, this.Location.Map.Layers[0].LayerWidth - 2);
							for (int y2 = 1; y2 < this.Location.Map.Layers[0].LayerHeight; y2++)
							{
								Vector2 spot = new Vector2(xPos, y2);
								if (this.Location.isTileLocationOpen(spot) && this.Location.isTilePlaceable(spot) && !dec_location2.isTileOnWall(xPos, y2) && !this.Location.IsTileOccupiedBy(spot))
								{
									this.facing.Value = 2;
									this.renderCache = null;
									this.Location.objects.Remove(this.TileLocation);
									this.TileLocation = spot;
									this.Location.objects.Add(this.TileLocation, this);
									return;
								}
							}
						}
						return;
					}
					int xStartingPoint;
					int xEndingPoint;
					int xDirection;
					if (Game1.random.NextDouble() < 0.5)
					{
						xStartingPoint = 1;
						xEndingPoint = this.Location.Map.Layers[0].LayerWidth - 1;
						xDirection = 1;
					}
					else
					{
						xStartingPoint = this.Location.Map.Layers[0].LayerWidth - 1;
						xEndingPoint = 1;
						xDirection = -1;
					}
					for (int i2 = 0; i2 < 30; i2++)
					{
						int yPos = Game1.random.Next(2, this.Location.Map.Layers[0].LayerHeight - 2);
						for (int x = xStartingPoint; x != xEndingPoint; x += xDirection)
						{
							Vector2 spot2 = new Vector2(x, yPos);
							if (this.Location.isTileLocationOpen(spot2) && this.Location.isTilePlaceable(spot2) && !dec_location2.isTileOnWall(x, yPos) && !this.Location.IsTileOccupiedBy(spot2))
							{
								this.facing.Value = ((xDirection == 1) ? 1 : 3);
								this.renderCache = null;
								this.Location.objects.Remove(this.TileLocation);
								this.TileLocation = spot2;
								this.Location.objects.Add(this.TileLocation, this);
								return;
							}
						}
					}
				}
			}
		}
		else if (Game1.IsMasterGame && this.Location is SeedShop && this.TileLocation.X > 33f && this.TileLocation.Y > 14f)
		{
			if (base.ItemId.Equals("CursedMannequinMale"))
			{
				base.ItemId = "MannequinMale";
			}
			else if (base.ItemId.Equals("CursedMannequinFemale"))
			{
				base.ItemId = "MannequinFemale";
			}
			base.ResetParentSheetIndex();
			this.renderCache = null;
			this._data = null;
		}
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		base.updateWhenCurrentLocation(time);
		this.changeMutex.Update(this.Location);
		if (this.eyeTimer > 0)
		{
			this.eyeTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
	}

	public override bool performToolAction(Tool t)
	{
		if (t == null)
		{
			return false;
		}
		if (!(t is MeleeWeapon) && t.isHeavyHitter())
		{
			if (this.hat.Value != null || this.shirt.Value != null || this.pants.Value != null || this.boots.Value != null)
			{
				if (this.hat.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.hat.Value));
					this.hat.Value = null;
				}
				else if (this.shirt.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.shirt.Value));
					this.shirt.Value = null;
				}
				else if (this.pants.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.pants.Value));
					this.pants.Value = null;
				}
				else if (this.boots.Value != null)
				{
					this.DropItem(Utility.PerformSpecialItemGrabReplacement(this.boots.Value));
					this.boots.Value = null;
				}
				this.Location.playSound("hammer");
				base.shakeTimer = 100;
				return false;
			}
			this.Location.objects.Remove(this.TileLocation);
			this.Location.playSound("hammer");
			this.DropItem(new Mannequin(base.ItemId));
			return false;
		}
		return false;
	}

	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (who.CurrentItem is Hat || who.CurrentItem is Clothing || who.CurrentItem is Boots)
		{
			return false;
		}
		if (justCheckingForActivity)
		{
			return true;
		}
		if (this.hat.Value == null && this.shirt.Value == null && this.pants.Value == null && this.boots.Value == null)
		{
			this.facing.Value = (this.facing.Value + 1) % 4;
			this.renderCache = null;
			Game1.playSound("shwip");
		}
		else
		{
			this.changeMutex.RequestLock(delegate
			{
				this.hat.Value = who.Equip(this.hat.Value, who.hat);
				this.shirt.Value = who.Equip(this.shirt.Value, who.shirtItem);
				this.pants.Value = who.Equip(this.pants.Value, who.pantsItem);
				this.boots.Value = who.Equip(this.boots.Value, who.boots);
				this.changeMutex.ReleaseLock();
			});
			Game1.playSound("coin");
		}
		if (this.GetMannequinData().Cursed && Game1.random.NextDouble() < 0.001)
		{
			this.emitGhost();
		}
		return true;
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		if (!(dropInItem is Hat newHat))
		{
			if (!(dropInItem is Clothing newClothing))
			{
				if (!(dropInItem is Boots newBoots))
				{
					return false;
				}
				if (!probe)
				{
					this.DropItem(this.boots.Value);
					this.boots.Value = (Boots)newBoots.getOne();
				}
			}
			else if (!probe)
			{
				if (newClothing.clothesType.Value == Clothing.ClothesType.SHIRT)
				{
					this.DropItem(this.shirt.Value);
					this.shirt.Value = (Clothing)newClothing.getOne();
				}
				else
				{
					this.DropItem(this.pants.Value);
					this.pants.Value = (Clothing)newClothing.getOne();
				}
			}
		}
		else if (!probe)
		{
			this.DropItem(this.hat.Value);
			this.hat.Value = (Hat)newHat.getOne();
		}
		if (!probe)
		{
			Game1.playSound("dirtyHit");
		}
		return true;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		base.draw(spriteBatch, x, y, alpha);
		if (this.eyeTimer > 0 && this.facing.Value != 0)
		{
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1.1E-05f;
			Vector2 pos = Game1.GlobalToLocal(new Vector2(x, y) * 64f + new Vector2(20f, -40f));
			switch (this.facing.Value)
			{
			case 1:
				pos.X += 12f;
				break;
			case 3:
				pos.X += 4f;
				break;
			}
			if (this.facing.Value != 2)
			{
				pos.Y -= 4f;
			}
			spriteBatch.Draw(Game1.mouseCursors_1_6, pos, new Rectangle(377 + 5 * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1620.0 / 60.0), 330, 5 + ((this.facing.Value != 2) ? (-3) : 0), 3), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);
		}
		float drawLayer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
		Farmer fakeFarmer = this.GetFarmerForRendering();
		fakeFarmer.position.Value = new Vector2(x * 64, y * 64 - 4 + (this.GetMannequinData().DisplaysClothingAsMale ? 20 : 16));
		if (base.shakeTimer > 0)
		{
			fakeFarmer.position.Value += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
		}
		fakeFarmer.FarmerRenderer.draw(spriteBatch, fakeFarmer.FarmerSprite, fakeFarmer.FarmerSprite.SourceRect, fakeFarmer.getLocalPosition(Game1.viewport), new Vector2(0f, fakeFarmer.GetBoundingBox().Height), drawLayer + 0.0001f, Color.White, 0f, fakeFarmer);
		FarmerRenderer.FarmerSpriteLayers armLayer = FarmerRenderer.FarmerSpriteLayers.Arms;
		if (fakeFarmer.facingDirection.Value == 0)
		{
			armLayer = FarmerRenderer.FarmerSpriteLayers.ArmsUp;
		}
		if (fakeFarmer.FarmerSprite.CurrentAnimationFrame.armOffset > 0)
		{
			Rectangle sourceRect = fakeFarmer.FarmerSprite.SourceRect;
			sourceRect.Offset(-288 + fakeFarmer.FarmerSprite.CurrentAnimationFrame.armOffset * 16, 0);
			spriteBatch.Draw(fakeFarmer.FarmerRenderer.baseTexture, fakeFarmer.getLocalPosition(Game1.viewport) + new Vector2(0f, fakeFarmer.GetBoundingBox().Height) + fakeFarmer.FarmerRenderer.positionOffset + fakeFarmer.armOffset, sourceRect, Color.White, 0f, new Vector2(0f, fakeFarmer.GetBoundingBox().Height), 4f * base.scale, fakeFarmer.FarmerSprite.CurrentAnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(drawLayer + 0.0001f, armLayer));
		}
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Mannequin(base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Mannequin from)
		{
			this.hat.Value = from.hat.Value?.getOne() as Hat;
			this.shirt.Value = from.shirt.Value?.getOne() as Clothing;
			this.pants.Value = from.pants.Value?.getOne() as Clothing;
			this.boots.Value = from.boots.Value?.getOne() as Boots;
		}
	}

	private void DropItem(Item item)
	{
		if (item != null)
		{
			Vector2 position = new Vector2((this.TileLocation.X + 0.5f) * 64f, (this.TileLocation.Y + 0.5f) * 64f);
			this.Location.debris.Add(new Debris(item, position));
		}
	}

	private Farmer GetFarmerForRendering()
	{
		this.renderCache = this.renderCache ?? CreateInstance();
		return this.renderCache;
		Farmer CreateInstance()
		{
			MannequinData data = this.GetMannequinData();
			Farmer farmer = new Farmer();
			farmer.changeGender(data.DisplaysClothingAsMale);
			farmer.faceDirection(this.facing.Value);
			farmer.changeHairColor(Color.Transparent);
			farmer.skin.Set(farmer.FarmerRenderer.recolorSkin(-12345));
			farmer.hat.Value = this.hat.Value;
			farmer.shirtItem.Value = this.shirt.Value;
			if (this.shirt.Value != null)
			{
				farmer.changeShirt("-1");
			}
			farmer.pantsItem.Value = this.pants.Value;
			if (this.pants.Value != null)
			{
				farmer.changePantStyle("-1");
			}
			farmer.boots.Value = this.boots.Value;
			if (this.boots.Value != null)
			{
				farmer.changeShoeColor(this.boots.Value.GetBootsColorString());
			}
			farmer.FarmerRenderer.textureName.Value = data.FarmerTexture;
			farmer.FarmerSprite.PauseForSingleAnimation = true;
			farmer.currentEyes = 0;
			return farmer;
		}
	}
}
