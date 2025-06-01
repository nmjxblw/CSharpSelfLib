using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures;

public class Bush : LargeTerrainFeature
{
	public const float shakeRate = (float)Math.PI / 200f;

	public const float shakeDecayRate = 0.0030679617f;

	public const int smallBush = 0;

	public const int mediumBush = 1;

	public const int largeBush = 2;

	public const int greenTeaBush = 3;

	public const int walnutBush = 4;

	public const int daysToMatureGreenTeaBush = 20;

	/// <summary>The type of bush, usually matching a constant like <see cref="F:StardewValley.TerrainFeatures.Bush.smallBush" />.</summary>
	[XmlElement("size")]
	public readonly NetInt size = new NetInt();

	[XmlElement("datePlanted")]
	public readonly NetInt datePlanted = new NetInt();

	[XmlElement("tileSheetOffset")]
	public readonly NetInt tileSheetOffset = new NetInt();

	public float health;

	[XmlElement("flipped")]
	public readonly NetBool flipped = new NetBool();

	/// <summary>Whether this is a cosmetic bush which produces no berries.</summary>
	[XmlElement("townBush")]
	public readonly NetBool townBush = new NetBool();

	/// <summary>Whether this bush is planted in a garden pot.</summary>
	public readonly NetBool inPot = new NetBool();

	[XmlElement("drawShadow")]
	public readonly NetBool drawShadow = new NetBool(value: true);

	private bool shakeLeft;

	private float shakeRotation;

	private float maxShake;

	[XmlIgnore]
	public float shakeTimer;

	[XmlIgnore]
	public readonly NetRectangle sourceRect = new NetRectangle();

	[XmlIgnore]
	public NetMutex uniqueSpawnMutex = new NetMutex();

	public static Lazy<Texture2D> texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>("TileSheets\\bushes"));

	public static Rectangle shadowSourceRect = new Rectangle(663, 1011, 41, 30);

	private float yDrawOffset;

	public Bush()
		: base(needsTick: true)
	{
	}

	public Bush(Vector2 tileLocation, int size, GameLocation location, int datePlantedOverride = -1)
		: this()
	{
		this.Tile = tileLocation;
		this.size.Value = size;
		this.Location = location;
		this.townBush.Value = location is Town && (size == 0 || size == 1 || size == 2) && tileLocation.X % 5f != 0f;
		if (location.map.RequireLayer("Front").Tiles[(int)tileLocation.X, (int)tileLocation.Y] != null)
		{
			this.drawShadow.Value = false;
		}
		this.datePlanted.Value = ((datePlantedOverride == -1) ? ((int)Game1.stats.DaysPlayed) : datePlantedOverride);
		switch (size)
		{
		case 3:
			this.drawShadow.Value = false;
			break;
		case 4:
			this.tileSheetOffset.Value = 1;
			break;
		}
		GameLocation old_location = this.Location;
		this.Location = location;
		this.loadSprite();
		this.Location = old_location;
		this.flipped.Value = Game1.random.NextBool();
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.size, "size").AddField(this.tileSheetOffset, "tileSheetOffset").AddField(this.flipped, "flipped")
			.AddField(this.townBush, "townBush")
			.AddField(this.drawShadow, "drawShadow")
			.AddField(this.sourceRect, "sourceRect")
			.AddField(this.datePlanted, "datePlanted")
			.AddField(this.inPot, "inPot")
			.AddField(this.uniqueSpawnMutex.NetFields, "uniqueSpawnMutex.NetFields");
	}

	public int getAge()
	{
		return (int)Game1.stats.DaysPlayed - this.datePlanted.Value;
	}

	public void setUpSourceRect()
	{
		Season season = this.GetCosmeticSeason();
		int seasonNumber = (int)season;
		switch (this.size.Value)
		{
		case 0:
			this.sourceRect.Value = new Rectangle(seasonNumber * 16 * 2 + this.tileSheetOffset.Value * 16, 224, 16, 32);
			break;
		case 1:
		{
			if (this.townBush.Value)
			{
				this.sourceRect.Value = new Rectangle(seasonNumber * 16 * 2, 96, 32, 32);
				break;
			}
			int xOffset = seasonNumber * 16 * 4 + this.tileSheetOffset.Value * 16 * 2;
			this.sourceRect.Value = new Rectangle(xOffset % Bush.texture.Value.Bounds.Width, xOffset / Bush.texture.Value.Bounds.Width * 3 * 16, 32, 48);
			break;
		}
		case 2:
			if (this.townBush.Value && (season == Season.Spring || season == Season.Summer))
			{
				this.sourceRect.Value = new Rectangle(48, 176, 48, 48);
				break;
			}
			switch (season)
			{
			case Season.Spring:
			case Season.Summer:
				this.sourceRect.Value = new Rectangle(0, 128, 48, 48);
				break;
			case Season.Fall:
				this.sourceRect.Value = new Rectangle(48, 128, 48, 48);
				break;
			case Season.Winter:
				this.sourceRect.Value = new Rectangle(0, 176, 48, 48);
				break;
			}
			break;
		case 3:
		{
			int age = this.getAge();
			switch (season)
			{
			case Season.Spring:
				this.sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 256, 16, 32);
				break;
			case Season.Summer:
				this.sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 256, 16, 32);
				break;
			case Season.Fall:
				this.sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 288, 16, 32);
				break;
			case Season.Winter:
				this.sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + this.tileSheetOffset.Value * 16, 288, 16, 32);
				break;
			}
			break;
		}
		case 4:
			this.sourceRect.Value = new Rectangle(this.tileSheetOffset.Value * 32, 320, 32, 32);
			break;
		}
	}

	/// <summary>Get whether this bush has berries or fruit to harvest.</summary>
	public bool readyForHarvest()
	{
		return this.tileSheetOffset.Value == 1;
	}

	/// <summary>Get the season for which to show a bush sprite (which isn't necessarily the season for which it produces fruit or leaves).</summary>
	public virtual Season GetCosmeticSeason()
	{
		if (this.size.Value == 1)
		{
			return this.Location.GetSeason();
		}
		if (!this.IsSheltered())
		{
			return this.Location.GetSeason();
		}
		return Season.Spring;
	}

	/// <summary>Whether this bush is in a greenhouse or indoor pot.</summary>
	public bool IsSheltered()
	{
		if (this.Location == null || !this.Location.SeedsIgnoreSeasonsHere())
		{
			if (this.inPot.Value)
			{
				return !this.Location.IsOutdoors;
			}
			return false;
		}
		return true;
	}

	/// <summary>Get whether this bush is in season to produce items, regardless of whether it has any currently.</summary>
	public bool inBloom()
	{
		if (this.size.Value == 4)
		{
			return this.readyForHarvest();
		}
		Season season = this.Location?.GetSeason() ?? Game1.season;
		int dayOfMonth = Game1.dayOfMonth;
		if (this.size.Value == 3)
		{
			bool inBloom = this.getAge() >= 20 && dayOfMonth >= 22 && (season != Season.Winter || this.IsSheltered());
			if (inBloom && this.Location != null && this.Location.IsFarm)
			{
				foreach (Farmer allFarmer in Game1.getAllFarmers())
				{
					allFarmer.autoGenerateActiveDialogueEvent("cropMatured_815");
				}
			}
			return inBloom;
		}
		switch (season)
		{
		case Season.Spring:
			if (dayOfMonth > 14)
			{
				return dayOfMonth < 19;
			}
			return false;
		case Season.Fall:
			if (dayOfMonth > 7)
			{
				return dayOfMonth < 12;
			}
			return false;
		default:
			return false;
		}
	}

	public override bool isActionable()
	{
		return true;
	}

	public override void loadSprite()
	{
		Vector2 tilePosition = this.Tile;
		Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, tilePosition.X, (double)tilePosition.Y * 777.0);
		double extra = ((r.NextDouble() < 0.5) ? 0.0 : ((double)r.Next(6) / 100.0));
		if (this.size.Value != 4)
		{
			if (this.size.Value == 1 && !this.readyForHarvest() && r.NextDouble() < 0.2 + extra && this.inBloom())
			{
				this.tileSheetOffset.Value = 1;
			}
			else if (Game1.GetSeasonForLocation(this.Location) != Season.Summer && !this.inBloom())
			{
				this.tileSheetOffset.Value = 0;
			}
		}
		if (this.size.Value == 3)
		{
			this.tileSheetOffset.Value = (this.inBloom() ? 1 : 0);
		}
		this.setUpSourceRect();
	}

	public override Rectangle getBoundingBox()
	{
		Vector2 tileLocation = this.Tile;
		switch (this.size.Value)
		{
		case 0:
		case 3:
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		case 1:
		case 4:
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 128, 64);
		case 2:
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 192, 64);
		default:
			return Rectangle.Empty;
		}
	}

	public override Rectangle getRenderBounds()
	{
		Vector2 tileLocation = this.Tile;
		switch (this.size.Value)
		{
		case 0:
		case 3:
			return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 1f) * 64, 64, 160);
		case 1:
		case 4:
			return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 128, 256);
		case 2:
			return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 192, 256);
		default:
			return Rectangle.Empty;
		}
	}

	public override bool performUseAction(Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		base.NeedsUpdate = true;
		if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
		{
			this.shakeTimer = 0f;
		}
		if (this.shakeTimer <= 0f)
		{
			Season season = location.GetSeason();
			if (this.maxShake == 0f && (this.size.Value != 3 || season != Season.Winter || this.IsSheltered()))
			{
				location.localSound("leafrustle");
			}
			GameLocation old_location = this.Location;
			this.Location = location;
			this.shake(tileLocation, doEvenIfStillShaking: false);
			this.Location = old_location;
			this.shakeTimer = 500f;
		}
		return true;
	}

	public override bool tickUpdate(GameTime time)
	{
		if (this.shakeTimer > 0f)
		{
			this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.size.Value == 4)
		{
			this.uniqueSpawnMutex.Update(this.Location);
		}
		if (this.maxShake > 0f)
		{
			if (this.shakeLeft)
			{
				this.shakeRotation -= (float)Math.PI / 200f;
				if (this.shakeRotation <= 0f - this.maxShake)
				{
					this.shakeLeft = false;
				}
			}
			else
			{
				this.shakeRotation += (float)Math.PI / 200f;
				if (this.shakeRotation >= this.maxShake)
				{
					this.shakeLeft = true;
				}
			}
			this.maxShake = Math.Max(0f, this.maxShake - 0.0030679617f);
		}
		if (this.shakeTimer <= 0f && this.size.Value != 4 && this.maxShake <= 0f)
		{
			base.NeedsUpdate = false;
		}
		return false;
	}

	public void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
	{
		if (!(this.maxShake == 0f || doEvenIfStillShaking))
		{
			return;
		}
		this.shakeLeft = Game1.player.Tile.X > tileLocation.X || (Game1.player.Tile.X == tileLocation.X && Game1.random.NextBool());
		this.maxShake = (float)Math.PI / 128f;
		base.NeedsUpdate = true;
		if (!this.townBush.Value && this.readyForHarvest() && this.inBloom())
		{
			string shakeOff = this.GetShakeOffItem();
			if (shakeOff == null)
			{
				return;
			}
			this.tileSheetOffset.Value = 0;
			this.setUpSourceRect();
			switch (this.size.Value)
			{
			case 4:
				this.uniqueSpawnMutex.RequestLock(delegate
				{
					Game1.player.team.MarkCollectedNut("Bush_" + this.Location.Name + "_" + tileLocation.X + "_" + tileLocation.Y);
					Game1.createItemDebris(ItemRegistry.Create(shakeOff), new Vector2(this.getBoundingBox().Center.X, this.getBoundingBox().Bottom - 2), 0, this.Location, this.getBoundingBox().Bottom);
				});
				break;
			case 3:
				Game1.createObjectDebris(shakeOff, (int)tileLocation.X, (int)tileLocation.Y);
				break;
			default:
			{
				int number = Utility.CreateRandom(tileLocation.X, (double)tileLocation.Y * 5000.0, Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).Next(1, 2) + Game1.player.ForagingLevel / 4;
				for (int i = 0; i < number; i++)
				{
					Item item = ItemRegistry.Create(shakeOff);
					if (Game1.player.professions.Contains(16))
					{
						item.Quality = 4;
					}
					Game1.createItemDebris(item, Utility.PointToVector2(this.getBoundingBox().Center), Game1.random.Next(1, 4));
				}
				Game1.player.gainExperience(2, number);
				break;
			}
			}
			if (this.size.Value != 3)
			{
				DelayedAction.playSoundAfterDelay("leafrustle", 100);
			}
		}
		else if (tileLocation.X == 20f && tileLocation.Y == 8f && Game1.dayOfMonth == 28 && Game1.timeOfDay == 1200 && !Game1.player.mailReceived.Contains("junimoPlush"))
		{
			Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(F)1733"), junimoPlushCallback);
		}
		else if (Game1.currentLocation is Town town)
		{
			if (tileLocation.X == 28f && tileLocation.Y == 14f && Game1.player.eventsSeen.Contains("520702") && !Game1.player.hasMagnifyingGlass)
			{
				town.initiateMagnifyingGlassGet();
			}
			else if (tileLocation.X == 47f && tileLocation.Y == 100f && Game1.player.secretNotesSeen.Contains(21) && Game1.timeOfDay == 2440 && Game1.player.mailReceived.Add("secretNote21_done"))
			{
				town.initiateMarnieLewisBush();
			}
		}
	}

	/// <summary>Get the qualified or unqualified item ID to produce when the bush is shaken, assuming it's in bloom.</summary>
	public string GetShakeOffItem()
	{
		return this.size.Value switch
		{
			3 => "(O)815", 
			4 => "(O)73", 
			_ => this.Location.GetSeason() switch
			{
				Season.Spring => "(O)296", 
				Season.Fall => "(O)410", 
				_ => null, 
			}, 
		};
	}

	public void junimoPlushCallback(Item item, Farmer who)
	{
		if (item?.QualifiedItemId == "(F)1733")
		{
			who?.mailReceived.Add("junimoPlush");
		}
	}

	/// <inheritdoc />
	public override bool isPassable(Character c = null)
	{
		return c is JunimoHarvester;
	}

	public override void dayUpdate()
	{
		GameLocation environment = this.Location;
		base.NeedsUpdate = true;
		Season season = environment.GetSeason();
		if (this.size.Value != 4)
		{
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, this.Tile.X, (double)this.Tile.Y * 777.0);
			double extra = ((r.NextDouble() < 0.5) ? 0.0 : ((double)r.Next(6) / 100.0));
			if (this.size.Value == 1 && !this.readyForHarvest() && r.NextDouble() < 0.2 + extra && this.inBloom())
			{
				this.tileSheetOffset.Value = 1;
			}
			else if (season != Season.Summer && !this.inBloom())
			{
				this.tileSheetOffset.Value = 0;
			}
			if (this.size.Value == 3)
			{
				this.tileSheetOffset.Value = (this.inBloom() ? 1 : 0);
			}
			this.setUpSourceRect();
			Vector2 tileLocation = this.Tile;
			if (tileLocation.X != 6f || tileLocation.Y != 7f || !(environment.Name == "Sunroom"))
			{
				this.health = 0f;
			}
		}
	}

	/// <inheritdoc />
	public override bool seasonUpdate(bool onLoad)
	{
		if (this.size.Value == 4)
		{
			return false;
		}
		if (!Game1.IsMultiplayer || Game1.IsServer)
		{
			Season season = this.Location.GetSeason();
			this.tileSheetOffset.Value = ((this.size.Value == 1 && season == Season.Summer && Game1.random.NextBool()) ? 1 : 0);
			this.loadSprite();
		}
		return false;
	}

	public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		base.NeedsUpdate = true;
		if (this.size.Value == 4)
		{
			return false;
		}
		if (explosion > 0)
		{
			this.shake(tileLocation, doEvenIfStillShaking: true);
			return false;
		}
		if (this.size.Value == 3 && t is MeleeWeapon { ItemId: "66" })
		{
			this.shake(tileLocation, doEvenIfStillShaking: true);
		}
		else if (t is Axe axe && this.isDestroyable())
		{
			location.playSound("leafrustle", tileLocation);
			this.shake(tileLocation, doEvenIfStillShaking: true);
			if (axe.upgradeLevel.Value >= 1 || this.size.Value == 3)
			{
				this.health -= ((this.size.Value == 3) ? 0.5f : ((float)axe.upgradeLevel.Value / 5f));
				if (this.health <= -1f)
				{
					location.playSound("treethud", tileLocation);
					DelayedAction.playSoundAfterDelay("leafrustle", 100, location, tileLocation);
					Color c = Color.Green;
					Season season = location.GetSeason();
					if (!this.IsSheltered())
					{
						switch (season)
						{
						case Season.Spring:
							c = Color.Green;
							break;
						case Season.Summer:
							c = Color.ForestGreen;
							break;
						case Season.Fall:
							c = Color.IndianRed;
							break;
						case Season.Winter:
							c = Color.Cyan;
							break;
						}
					}
					if (location.Name == "Sunroom")
					{
						foreach (NPC character in location.characters)
						{
							character.jump();
							character.doEmote(12);
						}
					}
					for (int i = 0; i <= this.getEffectiveSize(); i++)
					{
						for (int j = 0; j < 12; j++)
						{
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1200 + (season.Equals("fall") ? 16 : (season.Equals("winter") ? (-16) : 0)), 16, 16), Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(0f, Game1.random.Next(64)), flipped: false, 0.01f, c)
							{
								motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -Game1.random.Next(5, 7)),
								acceleration = new Vector2(0f, (float)Game1.random.Next(13, 17) / 100f),
								accelerationChange = new Vector2(0f, -0.001f),
								scale = 4f,
								layerDepth = (tileLocation.Y + 1f) * 64f / 10000f,
								animationLength = 11,
								totalNumberOfLoops = 99,
								interval = Game1.random.Next(20, 90),
								delayBeforeAnimationStart = (i + 1) * j * 20
							});
							if (j % 6 == 0)
							{
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(32f, Game1.random.Next(32, 64)), c));
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, Utility.getRandomPositionInThisRectangle(this.getBoundingBox(), Game1.random) - new Vector2(32f, Game1.random.Next(32, 64)), Color.White));
							}
						}
					}
					if (this.size.Value == 3)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)251"), tileLocation * 64f, 2, location);
					}
					return true;
				}
				location.playSound("axchop", tileLocation);
			}
		}
		return false;
	}

	public bool isDestroyable()
	{
		if (this.size.Value == 3)
		{
			return true;
		}
		if (this.Location is Farm)
		{
			Vector2 tile = this.Tile;
			switch (Game1.whichFarm)
			{
			case 2:
				if (tile.X == 13f && tile.Y == 35f)
				{
					return true;
				}
				if (tile.X == 37f && tile.Y == 9f)
				{
					return true;
				}
				return new Rectangle(43, 11, 34, 50).Contains((int)tile.X, (int)tile.Y);
			case 1:
				return new Rectangle(32, 11, 11, 25).Contains((int)tile.X, (int)tile.Y);
			case 3:
				return new Rectangle(24, 56, 10, 8).Contains((int)tile.X, (int)tile.Y);
			case 6:
				return new Rectangle(20, 44, 36, 44).Contains((int)tile.X, (int)tile.Y);
			}
		}
		return false;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
	{
		layerDepth += positionOnScreen.X / 100000f;
		spriteBatch.Draw(Bush.texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Rectangle(32, 96, 16, 32), Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
	}

	public override void performPlayerEntryAction()
	{
		base.performPlayerEntryAction();
		Season season = this.Location.GetSeason();
		if (season != Season.Winter && !this.Location.IsRainingHere() && Game1.isDarkOut(this.Location) && Game1.random.NextBool((season == Season.Summer) ? 0.08 : 0.04))
		{
			AmbientLocationSounds.addSound(this.Tile, 3);
		}
		NetRectangle netRectangle = this.sourceRect;
		if ((object)netRectangle != null && netRectangle.X < 0)
		{
			this.setUpSourceRect();
		}
	}

	private int getEffectiveSize()
	{
		return this.size.Value switch
		{
			3 => 0, 
			4 => 1, 
			_ => this.size.Value, 
		};
	}

	public void draw(SpriteBatch spriteBatch, float yDrawOffset)
	{
		this.yDrawOffset = yDrawOffset;
		this.draw(spriteBatch);
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		Vector2 tileLocation = this.Tile;
		if (this.drawShadow.Value)
		{
			if (this.getEffectiveSize() > 0)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((tileLocation.X + ((this.getEffectiveSize() == 1) ? 0.5f : 1f)) * 64f - 51f, tileLocation.Y * 64f - 16f + this.yDrawOffset)), Bush.shadowSourceRect, Color.White, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
			}
			else
			{
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f - 4f + this.yDrawOffset)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-06f);
			}
		}
		spriteBatch.Draw(Bush.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + (float)((this.getEffectiveSize() + 1) * 64 / 2), (tileLocation.Y + 1f) * 64f - (float)((this.getEffectiveSize() > 0 && (!this.townBush.Value || this.getEffectiveSize() != 1) && this.size.Value != 4) ? 64 : 0) + this.yDrawOffset)), this.sourceRect.Value, Color.White, this.shakeRotation, new Vector2((this.getEffectiveSize() + 1) * 16 / 2, 32f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.getBoundingBox().Center.Y + 48) / 10000f - tileLocation.X / 1000000f);
	}
}
