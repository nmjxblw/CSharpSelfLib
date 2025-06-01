using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations;

public class Mountain : GameLocation
{
	public const int daysBeforeLandslide = 31;

	private TemporaryAnimatedSprite minecartSteam;

	private bool bridgeRestored;

	[XmlIgnore]
	public bool treehouseBuilt;

	[XmlIgnore]
	public bool treehouseDoorDirty;

	private readonly NetBool oreBoulderPresent = new NetBool();

	private readonly NetBool railroadAreaBlocked = new NetBool(Game1.stats.DaysPlayed < 31);

	private readonly NetBool landslide = new NetBool(Game1.stats.DaysPlayed < 5);

	private Microsoft.Xna.Framework.Rectangle landSlideRect = new Microsoft.Xna.Framework.Rectangle(3200, 256, 192, 320);

	private Microsoft.Xna.Framework.Rectangle railroadBlockRect = new Microsoft.Xna.Framework.Rectangle(512, 0, 256, 320);

	private int oldTime;

	private Microsoft.Xna.Framework.Rectangle boulderSourceRect = new Microsoft.Xna.Framework.Rectangle(439, 1385, 39, 48);

	private Microsoft.Xna.Framework.Rectangle raildroadBlocksourceRect = new Microsoft.Xna.Framework.Rectangle(640, 2176, 64, 80);

	private Microsoft.Xna.Framework.Rectangle landSlideSourceRect = new Microsoft.Xna.Framework.Rectangle(646, 1218, 48, 80);

	private Vector2 boulderPosition = new Vector2(47f, 3f) * 64f - new Vector2(4f, 3f) * 4f;

	public Mountain()
	{
	}

	public Mountain(string map, string name)
		: base(map, name)
	{
		for (int i = 0; i < 10; i++)
		{
			this.quarryDayUpdate();
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.oreBoulderPresent, "oreBoulderPresent").AddField(this.railroadAreaBlocked, "railroadAreaBlocked").AddField(this.landslide, "landslide");
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (base.getTileIndexAt(tileLocation, "Buildings", "outdoors"))
		{
		case 1136:
			if (!who.mailReceived.Contains("guildMember") && !who.hasQuest("16"))
			{
				Game1.drawLetterMessage(Game1.content.LoadString("Strings\\Locations:Mountain_AdventurersGuildNote").Replace('\n', '^'));
				return true;
			}
			break;
		case 958:
		case 1080:
		case 1081:
			base.ShowMineCartMenu("Default", "Quarry");
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public void ApplyTreehouseIfNecessary()
	{
		WorldChangeEvent obj = Game1.farmEvent as WorldChangeEvent;
		if (((obj != null && obj.whichEvent.Value == 14) || Game1.MasterPlayer.mailReceived.Contains("leoMoved") || Game1.MasterPlayer.mailReceived.Contains("leoMoved%&NL&%")) && !this.treehouseBuilt)
		{
			TileSheet tilesheet = base.map.RequireTileSheet("untitled tile sheet2");
			Layer buildingsLayer = base.map.RequireLayer("Buildings");
			Layer backLayer = base.map.RequireLayer("Back");
			buildingsLayer.Tiles[16, 6] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, 197);
			buildingsLayer.Tiles[16, 7] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, 213);
			backLayer.Tiles[16, 8] = new StaticTile(backLayer, tilesheet, BlendMode.Alpha, 229);
			buildingsLayer.Tiles[16, 7].Properties["Action"] = "LockedDoorWarp 3 8 LeoTreeHouse 600 2300";
			this.treehouseBuilt = true;
			if (Game1.IsMasterGame)
			{
				base.updateDoors();
				this.treehouseDoorDirty = true;
			}
		}
	}

	private void restoreBridge()
	{
		LocalizedContentManager temp = Game1.content.CreateTemporary();
		Map obj = temp.Load<Map>("Maps\\Mountain-BridgeFixed");
		int xOffset = 92;
		int yOffset = 24;
		Layer curBackLayer = base.map.RequireLayer("Back");
		Layer curBuildingsLayer = base.map.RequireLayer("Buildings");
		Layer curFrontLayer = base.map.RequireLayer("Front");
		Layer fixedBackLayer = obj.RequireLayer("Back");
		Layer fixedBuildingsLayer = obj.RequireLayer("Buildings");
		Layer fixedFrontLayer = obj.RequireLayer("Front");
		TileSheet tileSheet = base.map.RequireTileSheet(0, "outdoors");
		for (int x = 0; x < fixedBackLayer.LayerWidth; x++)
		{
			for (int y = 0; y < fixedBackLayer.LayerHeight; y++)
			{
				curBackLayer.Tiles[x + xOffset, y + yOffset] = ((fixedBackLayer.Tiles[x, y] == null) ? null : new StaticTile(curBackLayer, tileSheet, BlendMode.Alpha, fixedBackLayer.Tiles[x, y].TileIndex));
				curBuildingsLayer.Tiles[x + xOffset, y + yOffset] = ((fixedBuildingsLayer.Tiles[x, y] == null) ? null : new StaticTile(curBuildingsLayer, tileSheet, BlendMode.Alpha, fixedBuildingsLayer.Tiles[x, y].TileIndex));
				curFrontLayer.Tiles[x + xOffset, y + yOffset] = ((fixedFrontLayer.Tiles[x, y] == null) ? null : new StaticTile(curFrontLayer, tileSheet, BlendMode.Alpha, fixedFrontLayer.Tiles[x, y].TileIndex));
			}
		}
		this.bridgeRestored = true;
		temp.Unload();
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		this.oreBoulderPresent.Value = !Game1.MasterPlayer.mailReceived.Contains("ccFishTank") || Game1.farmEvent != null;
		Vector2 fireTile = new Vector2(29f, 9f);
		if (!base.objects.ContainsKey(fireTile))
		{
			base.objects.Add(fireTile, new Torch("146", bigCraftable: true)
			{
				IsOn = false,
				Fragility = 2
			});
			base.objects[fireTile].checkForAction(null);
		}
		if (Game1.stats.DaysPlayed >= 5)
		{
			this.landslide.Value = false;
		}
		if (Game1.stats.DaysPlayed >= 31)
		{
			this.railroadAreaBlocked.Value = false;
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (force)
		{
			this.treehouseBuilt = false;
			this.bridgeRestored = false;
		}
		if (!this.bridgeRestored && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccCraftsRoom"))
		{
			this.restoreBridge();
		}
		WorldChangeEvent obj = Game1.farmEvent as WorldChangeEvent;
		if (obj == null || obj.whichEvent.Value != 14)
		{
			this.ApplyTreehouseIfNecessary();
		}
		if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
		{
			base.ApplyMapOverride("Mountain_Shortcuts");
			base.waterTiles[81, 37] = false;
			base.waterTiles[82, 37] = false;
			base.waterTiles[83, 37] = false;
			base.waterTiles[84, 37] = false;
			base.waterTiles[85, 37] = false;
			base.waterTiles[85, 38] = false;
			base.waterTiles[85, 39] = false;
			base.waterTiles[85, 40] = false;
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
		{
			this.minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(8072f, 656f), Color.White)
			{
				totalNumberOfLoops = 999999,
				interval = 60f,
				flipped = true
			};
		}
		Season season = base.GetSeason();
		this.boulderSourceRect = new Microsoft.Xna.Framework.Rectangle(439 + ((season == Season.Winter) ? 39 : 0), 1385, 39, 48);
		this.raildroadBlocksourceRect = new Microsoft.Xna.Framework.Rectangle(640, (season == Season.Spring) ? 2176 : 1453, 64, 80);
		base.addFrog();
		if (Game1.IsWinter)
		{
			Game1.currentLightSources.Add(new LightSource("Mountain_1", 4, new Vector2(800f, 1366f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Mountain_2", 4, new Vector2(544f, 1155f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Mountain_3", 4, new Vector2(924f, 1563f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Mountain_4", 4, new Vector2(673f, 1567f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		this.quarryDayUpdate();
		if (Game1.stats.DaysPlayed >= 31)
		{
			this.railroadAreaBlocked.Value = false;
		}
		if (Game1.stats.DaysPlayed >= 5)
		{
			this.landslide.Value = false;
			if (!Game1.player.hasOrWillReceiveMail("landslideDone"))
			{
				Game1.addMail("landslideDone", noLetter: false, sendToEveryone: true);
			}
		}
		if (Game1.IsFall && Game1.dayOfMonth == 17)
		{
			base.tryPlaceObject(new Vector2(11f, 26f), ItemRegistry.Create<Object>("(O)746"));
		}
	}

	private void quarryDayUpdate()
	{
		Microsoft.Xna.Framework.Rectangle quarryBounds = new Microsoft.Xna.Framework.Rectangle(106, 13, 22, 22);
		int numberOfAdditionsToTry = Math.Min(16, 5 + Game1.year * 2);
		for (int i = 0; i < numberOfAdditionsToTry; i++)
		{
			Vector2 position = Utility.getRandomPositionInThisRectangle(quarryBounds, Game1.random);
			if (!this.isTileOpenForQuarryStone((int)position.X, (int)position.Y))
			{
				continue;
			}
			if (Game1.random.NextDouble() < 0.06)
			{
				base.terrainFeatures.Add(position, new Tree((1 + Game1.random.Next(2)).ToString(), 1));
			}
			else if (Game1.random.NextDouble() < 0.02)
			{
				if (Game1.random.NextDouble() < 0.1)
				{
					base.objects.Add(position, new Object(46.ToString(), 1)
					{
						MinutesUntilReady = 12
					});
				}
				else
				{
					base.objects.Add(position, new Object(((Game1.random.Next(7) + 1) * 2).ToString(), 1)
					{
						MinutesUntilReady = 5
					});
				}
			}
			else if (Game1.random.NextDouble() < 0.04)
			{
				base.objects.Add(position, ItemRegistry.Create<Object>(Game1.random.NextBool(0.15) ? "(O)SeedSpot" : "(O)590"));
			}
			else if (Game1.random.NextDouble() < 0.15)
			{
				if (Game1.random.NextDouble() < 0.001)
				{
					base.objects.Add(position, new Object("765", 1)
					{
						MinutesUntilReady = 16
					});
				}
				else if (Game1.random.NextDouble() < 0.1)
				{
					base.objects.Add(position, new Object("764", 1)
					{
						MinutesUntilReady = 8
					});
				}
				else if (Game1.random.NextDouble() < 0.33)
				{
					base.objects.Add(position, new Object("290", 1)
					{
						MinutesUntilReady = 5
					});
				}
				else
				{
					base.objects.Add(position, new Object("751", 1)
					{
						MinutesUntilReady = 3
					});
				}
			}
			else if (Game1.random.NextDouble() < 0.1)
			{
				base.objects.Add(position, new Object(Game1.random.Choose("BasicCoalNode0", "BasicCoalNode1"), 1)
				{
					MinutesUntilReady = 5
				});
			}
			else
			{
				string id = Game1.random.Choose<string>("32", "38", "40", "42", "668", "670");
				base.objects.Add(position, new Object(id, 1)
				{
					MinutesUntilReady = 2
				});
			}
		}
	}

	private bool isTileOpenForQuarryStone(int tileX, int tileY)
	{
		if (this.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
		{
			return this.CanItemBePlacedHere(new Vector2(tileX, tileY), itemIsPassable: false, CollisionMask.All, CollisionMask.None);
		}
		return false;
	}

	public override void cleanupBeforePlayerExit()
	{
		base.cleanupBeforePlayerExit();
		this.minecartSteam = null;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.minecartSteam?.update(time);
		if (this.landslide.Value && (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds - 400.0) / 1600.0) % 2 != 0 && Utility.isOnScreen(new Point(this.landSlideRect.X / 64, this.landSlideRect.Y / 64), 128))
		{
			if (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 < (double)(this.oldTime % 400))
			{
				base.localSound("hammer");
			}
			this.oldTime = (int)time.TotalGameTime.TotalMilliseconds;
		}
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
	{
		if (this.landslide.Value && position.Intersects(this.landSlideRect))
		{
			return true;
		}
		if (this.railroadAreaBlocked.Value && position.Intersects(this.railroadBlockRect))
		{
			return true;
		}
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
	}

	public override bool isTilePlaceable(Vector2 tileLocation, bool itemIsPassable = false)
	{
		Point non_tile_position = Utility.Vector2ToPoint((tileLocation + new Vector2(0.5f, 0.5f)) * 64f);
		if (this.landslide.Value && this.landSlideRect.Contains(non_tile_position))
		{
			return false;
		}
		if (this.railroadAreaBlocked.Value && this.railroadBlockRect.Contains(non_tile_position))
		{
			return false;
		}
		return base.isTilePlaceable(tileLocation, itemIsPassable);
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		base.draw(spriteBatch);
		this.minecartSteam?.draw(spriteBatch);
		if (this.oreBoulderPresent.Value)
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.boulderPosition), this.boulderSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
		}
		if (this.railroadAreaBlocked.Value)
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.railroadBlockRect), this.raildroadBlocksourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0193f);
		}
		if (this.landslide.Value)
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.landSlideRect), this.landSlideSourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.0192f);
			spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2(this.landSlideRect.X + 192 - 20, this.landSlideRect.Y + 192 + 20) + new Vector2(32f, 24f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0224f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(this.landSlideRect.X + 192 - 20, this.landSlideRect.Y + 128)), new Microsoft.Xna.Framework.Rectangle(288 + (((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1600.0 % 2.0) != 0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 19) : 0), 1349, 19, 28), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0256f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(this.landSlideRect.X + 256 - 20, this.landSlideRect.Y + 128)), new Microsoft.Xna.Framework.Rectangle(335, 1410, 21, 21), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0128f);
		}
	}
}
