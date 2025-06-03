using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations;

public class Town : GameLocation
{
	private TemporaryAnimatedSprite minecartSteam;

	private bool ccRefurbished;

	private bool ccJoja;

	private bool playerCheckedBoard;

	private bool isShowingDestroyedJoja;

	private bool isShowingUpgradedPamHouse;

	private bool isShowingSpecialOrdersBoard;

	private bool showBookseller;

	private LocalizedContentManager mapLoader;

	[XmlElement("daysUntilCommunityUpgrade")]
	public readonly NetInt daysUntilCommunityUpgrade = new NetInt(0);

	private Vector2 clockCenter = new Vector2(3392f, 1056f);

	private Vector2 ccFacadePosition = new Vector2(3044f, 940f);

	private Vector2 ccFacadePositionBottom = new Vector2(3044f, 1140f);

	public static Microsoft.Xna.Framework.Rectangle minuteHandSource = new Microsoft.Xna.Framework.Rectangle(363, 395, 5, 13);

	public static Microsoft.Xna.Framework.Rectangle hourHandSource = new Microsoft.Xna.Framework.Rectangle(369, 399, 5, 9);

	public static Microsoft.Xna.Framework.Rectangle clockNub = new Microsoft.Xna.Framework.Rectangle(375, 404, 4, 4);

	public static Microsoft.Xna.Framework.Rectangle jojaFacadeTop = new Microsoft.Xna.Framework.Rectangle(424, 1275, 174, 50);

	public static Microsoft.Xna.Framework.Rectangle jojaFacadeBottom = new Microsoft.Xna.Framework.Rectangle(424, 1325, 174, 51);

	public static Microsoft.Xna.Framework.Rectangle jojaFacadeWinterOverlay = new Microsoft.Xna.Framework.Rectangle(66, 1678, 174, 25);

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.daysUntilCommunityUpgrade, "daysUntilCommunityUpgrade");
	}

	public Town()
	{
	}

	public Town(string map, string name)
		: base(map, name)
	{
	}

	protected override LocalizedContentManager getMapLoader()
	{
		if (this.mapLoader == null)
		{
			this.mapLoader = Game1.game1.xTileContent.CreateTemporary();
		}
		return this.mapLoader;
	}

	public override void UpdateMapSeats()
	{
		base.UpdateMapSeats();
		if (Game1.IsMasterGame)
		{
			base.mapSeats.RemoveWhere((MapSeat seat) => seat.tilePosition.Value.X == 24f && seat.tilePosition.Value.Y == 13f && seat.seatType.Value == "swings");
		}
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		if (!Game1.isStartingToGetDarkOut(this))
		{
			this.addClintMachineGraphics();
		}
		else
		{
			AmbientLocationSounds.removeSound(new Vector2(100f, 79f));
		}
	}

	public void checkedBoard()
	{
		this.playerCheckedBoard = true;
	}

	private void addClintMachineGraphics()
	{
		TemporaryAnimatedSprite s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(302, 1946, 15, 16), Game1.realMilliSecondsPerGameTenMinutes - Game1.gameTimeInterval, 1, 1, new Vector2(100f, 79f) * 64f + new Vector2(9f, 6f) * 4f, flicker: false, flipped: false, 0.5188f, 0f, Color.White, 4f, 0f, 0f, 0f);
		s.shakeIntensity = 1f;
		base.temporarySprites.Add(s);
		for (int i = 0; i < 10; i++)
		{
			Utility.addSmokePuff(this, new Vector2(101f, 78f) * 64f + new Vector2(4f, 4f) * 4f, i * ((Game1.realMilliSecondsPerGameTenMinutes - Game1.gameTimeInterval) / 16));
		}
		Microsoft.Xna.Framework.Rectangle pipeSourceRect = (base.IsFallHere() ? new Microsoft.Xna.Framework.Rectangle(304, 256, 5, 18) : new Microsoft.Xna.Framework.Rectangle(643, 1305, 5, 18));
		for (int j = 0; j < Game1.random.Next(1, 4); j++)
		{
			for (int k = 0; k < 16; k++)
			{
				s = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", pipeSourceRect, 50f, 4, 1, new Vector2(100f, 78f) * 64f + new Vector2(-5 - k * 4, 0f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f);
				s.delayBeforeAnimationStart = j * 1500 + 100 * k;
				base.temporarySprites.Add(s);
			}
			Utility.addSmokePuff(this, new Vector2(100f, 78f) * 64f + new Vector2(-70f, -6f) * 4f, j * 1500 + 1600);
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		this.showBookseller = false;
		if (Game1.dayOfMonth == 2 && Game1.IsSpring && !Game1.MasterPlayer.mailReceived.Contains("JojaMember") && this.CanItemBePlacedHere(new Vector2(57f, 16f)))
		{
			base.objects.Add(new Vector2(57f, 16f), ItemRegistry.Create<Object>("(BC)55"));
		}
		if (this.daysUntilCommunityUpgrade.Value > 0)
		{
			this.daysUntilCommunityUpgrade.Value--;
			if (this.daysUntilCommunityUpgrade.Value <= 0)
			{
				if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "pamHouseUpgrade", MailType.Received, add: true);
					Game1.player.changeFriendship(1000, Game1.getCharacterFromName("Pam"));
				}
				else
				{
					Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "communityUpgradeShortcuts", MailType.Received, add: true);
				}
			}
		}
		if (Game1.IsFall && Game1.dayOfMonth == 17)
		{
			base.tryPlaceObject(new Vector2(9f, 86f), ItemRegistry.Create<Object>("(O)746"));
			base.tryPlaceObject(new Vector2(21f, 89f), ItemRegistry.Create<Object>("(O)746"));
			base.tryPlaceObject(new Vector2(70f, 69f), ItemRegistry.Create<Object>("(O)746"));
			base.tryPlaceObject(new Vector2(63f, 63f), ItemRegistry.Create<Object>("(O)746"));
			if (this.ccRefurbished && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				base.tryPlaceObject(new Vector2(50f, 21f), ItemRegistry.Create<Object>("(O)746"));
				base.tryPlaceObject(new Vector2(55f, 21f), ItemRegistry.Create<Object>("(O)746"));
			}
			if (!this.isObjectAtTile(41, 85))
			{
				base.furniture.Add(new Furniture("1369", new Vector2(41f, 85f))
				{
					CanBeGrabbed = false
				});
			}
			if (!this.isObjectAtTile(48, 86))
			{
				base.furniture.Add(new Furniture("1369", new Vector2(48f, 86f))
				{
					CanBeGrabbed = false
				});
			}
			if (!this.isObjectAtTile(43, 89))
			{
				base.furniture.Add(new Furniture("1369", new Vector2(43f, 89f))
				{
					CanBeGrabbed = false
				});
			}
			if (!this.isObjectAtTile(52, 86))
			{
				base.furniture.Add(new Furniture("1369", new Vector2(52f, 86f))
				{
					CanBeGrabbed = false
				});
			}
		}
		if (Game1.IsWinter && Game1.dayOfMonth == 1)
		{
			if (!base.objects.ContainsKey(new Vector2(41f, 85f)))
			{
				this.removeEverythingFromThisTile(41, 85);
			}
			if (!base.objects.ContainsKey(new Vector2(48f, 86f)))
			{
				this.removeEverythingFromThisTile(48, 86);
			}
			if (!base.objects.ContainsKey(new Vector2(43f, 89f)))
			{
				this.removeEverythingFromThisTile(43, 89);
			}
			if (!base.objects.ContainsKey(new Vector2(52f, 86f)))
			{
				this.removeEverythingFromThisTile(52, 86);
			}
			this.removeObjectAtTileWithName(9, 86, "Rotten Plant");
			this.removeObjectAtTileWithName(21, 89, "Rotten Plant");
			this.removeObjectAtTileWithName(70, 96, "Rotten Plant");
			this.removeObjectAtTileWithName(63, 63, "Rotten Plant");
			this.removeObjectAtTileWithName(50, 21, "Rotten Plant");
			this.removeObjectAtTileWithName(55, 21, "Rotten Plant");
		}
	}

	public bool removeObjectAtTileWithName(int x, int y, string name)
	{
		Vector2 v = new Vector2(x, y);
		if (base.objects.TryGetValue(v, out var tileObj) && tileObj.Name == name)
		{
			base.objects.Remove(v);
			return true;
		}
		return false;
	}

	public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
	{
		if (who.secretNotesSeen.Contains(17) && xLocation == 98 && yLocation == 5 && who.mailReceived.Add("SecretNote17_done"))
		{
			Game1.createObjectDebris("(O)126", xLocation, yLocation, who.UniqueMultiplayerID, this);
			return "";
		}
		return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
	}

	/// <inheritdoc />
	public override bool CanPlantTreesHere(string itemId, int tileX, int tileY, out string deniedMessage)
	{
		if (this.doesTileHavePropertyNoNull(tileX, tileY, "Type", "Back") != "Dirt")
		{
			deniedMessage = null;
			return false;
		}
		return base.CheckItemPlantRules(itemId, isGardenPot: false, defaultAllowed: false, out deniedMessage);
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		Layer buildingLayer = base.map.RequireLayer("Buildings");
		if (who.mount == null)
		{
			switch (buildingLayer.GetTileIndexAt(tileLocation, "Town"))
			{
			case 620:
				if (Utility.HasAnyPlayerSeenEvent("191393"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_SeedShopSign").Replace('\n', '^'));
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_SeedShopSign").Split('\n')[0] + "^" + Game1.content.LoadString("Strings\\Locations:SeedShop_LockedWed"));
				}
				return true;
			case 1935:
			case 2270:
				if (Game1.player.secretNotesSeen.Contains(20) && !Game1.player.mailReceived.Contains("SecretNote20_done"))
				{
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Town_SpecialCharmQuestion"), base.createYesNoResponses(), "specialCharmQuestion");
				}
				break;
			case 1913:
			case 1914:
			case 1945:
			case 1946:
				if (this.isShowingDestroyedJoja)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_JojaSign_Destroyed"));
					return true;
				}
				break;
			case 2000:
			case 2001:
			case 2032:
			case 2033:
				if (this.isShowingDestroyedJoja)
				{
					Rumble.rumble(0.15f, 200f);
					Game1.player.completelyStopAnimatingOrDoingAction();
					base.playSound("stairsdown", Game1.player.Tile);
					Game1.warpFarmer("AbandonedJojaMart", 9, 13, flip: false);
					return true;
				}
				break;
			case 599:
				if (Game1.player.secretNotesSeen.Contains(19) && Game1.player.mailReceived.Add("SecretNote19_done"))
				{
					DelayedAction.playSoundAfterDelay("newArtifact", 250);
					Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)164"));
				}
				break;
			case 1080:
			case 1081:
			{
				if (Game1.player.mount != null)
				{
					return true;
				}
				bool? flag = base.currentEvent?.isFestival;
				if (flag.HasValue && flag == true && base.currentEvent.checkAction(tileLocation, viewport, who))
				{
					return true;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_PickupTruck"));
				return true;
			}
			}
			int tileIndexAt = buildingLayer.GetTileIndexAt(tileLocation, "Landscape");
			if ((tileIndexAt == 958 || (uint)(tileIndexAt - 1080) <= 1u) && !Game1.isFestival())
			{
				base.ShowMineCartMenu("Default", "Town");
				return true;
			}
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public void crackOpenAbandonedJojaMartDoor()
	{
		base.setMapTile(95, 49, 2000, "Buildings", "Town");
		base.setMapTile(96, 49, 2001, "Buildings", "Town");
		base.setMapTile(95, 50, 2032, "Buildings", "Town");
		base.setMapTile(96, 50, 2033, "Buildings", "Town");
	}

	private void refurbishCommunityCenter()
	{
		if (this.ccRefurbished)
		{
			return;
		}
		this.ccRefurbished = true;
		if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
		{
			this.ccJoja = true;
		}
		if (base._appliedMapOverrides != null)
		{
			if (base._appliedMapOverrides.Contains("ccRefurbished"))
			{
				return;
			}
			base._appliedMapOverrides.Add("ccRefurbished");
		}
		Microsoft.Xna.Framework.Rectangle ccBounds = new Microsoft.Xna.Framework.Rectangle(47, 11, 11, 9);
		Layer backLayer = base.map.RequireLayer("Back");
		Layer buildingsLayer = base.map.RequireLayer("Buildings");
		Layer frontLayer = base.map.RequireLayer("Front");
		Layer alwaysFrontLayer = base.map.RequireLayer("AlwaysFront");
		for (int x = ccBounds.X; x <= ccBounds.Right; x++)
		{
			for (int y = ccBounds.Y; y <= ccBounds.Bottom; y++)
			{
				if (backLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					backLayer.Tiles[x, y].TileIndex += 12;
				}
				if (buildingsLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					buildingsLayer.Tiles[x, y].TileIndex += 12;
				}
				if (frontLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					frontLayer.Tiles[x, y].TileIndex += 12;
				}
				if (alwaysFrontLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					alwaysFrontLayer.Tiles[x, y].TileIndex += 12;
				}
			}
		}
	}

	private void showDestroyedJoja()
	{
		if (this.isShowingDestroyedJoja)
		{
			return;
		}
		this.isShowingDestroyedJoja = true;
		if (base._appliedMapOverrides != null && base._appliedMapOverrides.Contains("isShowingDestroyedJoja"))
		{
			return;
		}
		base._appliedMapOverrides.Add("isShowingDestroyedJoja");
		Microsoft.Xna.Framework.Rectangle jojaBounds = new Microsoft.Xna.Framework.Rectangle(90, 42, 11, 9);
		Layer backLayer = base.map.RequireLayer("Back");
		Layer buildingsLayer = base.map.RequireLayer("Buildings");
		Layer frontLayer = base.map.RequireLayer("Front");
		Layer alwaysFrontLayer = base.map.RequireLayer("AlwaysFront");
		for (int x = jojaBounds.X; x <= jojaBounds.Right; x++)
		{
			for (int y = jojaBounds.Y; y <= jojaBounds.Bottom; y++)
			{
				int num;
				if (x <= jojaBounds.X + 6)
				{
					num = ((y < jojaBounds.Y + 9) ? 1 : 0);
					if (num == 0)
					{
						goto IL_00fc;
					}
				}
				else
				{
					num = 1;
				}
				if (backLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					backLayer.Tiles[x, y].TileIndex += 20;
				}
				goto IL_00fc;
				IL_00fc:
				if (num != 0 && buildingsLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					buildingsLayer.Tiles[x, y].TileIndex += 20;
				}
				if (num != 0 && ((x != 93 && y != 50) || (x != 94 && y != 50)) && frontLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					frontLayer.Tiles[x, y].TileIndex += 20;
				}
				if (num != 0 && alwaysFrontLayer.GetTileIndexAt(x, y, "Town") > 1200)
				{
					alwaysFrontLayer.Tiles[x, y].TileIndex += 20;
				}
			}
		}
	}

	public override bool isTileFishable(int tileX, int tileY)
	{
		if ((base.GetSeason() != Season.Winter && tileY == 26 && (tileX == 25 || tileX == 26 || tileX == 27)) || (tileX == 25 && tileY == 25) || (tileX == 27 && tileY == 25))
		{
			return true;
		}
		return base.isTileFishable(tileX, tileY);
	}

	public void showImprovedPamHouse()
	{
		if (this.isShowingUpgradedPamHouse)
		{
			return;
		}
		this.isShowingUpgradedPamHouse = true;
		if (base._appliedMapOverrides != null)
		{
			if (base._appliedMapOverrides.Contains("isShowingUpgradedPamHouse"))
			{
				return;
			}
			base._appliedMapOverrides.Add("isShowingUpgradedPamHouse");
		}
		Microsoft.Xna.Framework.Rectangle buildingsLayerBounds = new Microsoft.Xna.Framework.Rectangle(69, 66, 8, 3);
		Microsoft.Xna.Framework.Rectangle alwaysFrontLayerBounds = new Microsoft.Xna.Framework.Rectangle(69, 60, 8, 6);
		Layer buildingsLayer = base.map.RequireLayer("Buildings");
		Layer frontLayer = base.map.RequireLayer("Front");
		Layer alwaysFrontLayer = base.map.RequireLayer("AlwaysFront");
		foreach (Point point in buildingsLayerBounds.GetPoints())
		{
			int x = point.X;
			int y = point.Y;
			if (buildingsLayer.Tiles[x, y] != null)
			{
				buildingsLayer.Tiles[x, y].TileIndex += 842;
				if (buildingsLayer.GetTileIndexAt(x, y, "Town") == 1568)
				{
					buildingsLayer.Tiles[x, y].TileIndex = 1562;
				}
			}
			if (frontLayer.Tiles[x, y] != null && y < buildingsLayerBounds.Bottom - 1)
			{
				frontLayer.Tiles[x, y].TileIndex += 842;
			}
		}
		foreach (Point point2 in alwaysFrontLayerBounds.GetPoints())
		{
			int x2 = point2.X;
			int y2 = point2.Y;
			if (alwaysFrontLayer.Tiles[x2, y2] == null)
			{
				alwaysFrontLayer.Tiles[x2, y2] = new StaticTile(alwaysFrontLayer, base.map.RequireTileSheet("Town"), BlendMode.Alpha, 1336 + (x2 - alwaysFrontLayerBounds.X) + (y2 - alwaysFrontLayerBounds.Y) * 32);
			}
		}
		if (!Game1.eventUp)
		{
			base.removeTile(63, 68, "Buildings");
			base.removeTile(62, 72, "Buildings");
			base.removeTile(74, 71, "Buildings");
		}
	}

	public static Point GetTheaterTileOffset()
	{
		if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja"))
		{
			return new Point(-43, -31);
		}
		return new Point(0, 0);
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (force)
		{
			this.isShowingSpecialOrdersBoard = false;
			this.isShowingUpgradedPamHouse = false;
			this.isShowingDestroyedJoja = false;
			this.ccRefurbished = false;
		}
		if (Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter())
		{
			this.refurbishCommunityCenter();
		}
		if (!this.isShowingSpecialOrdersBoard && SpecialOrder.IsSpecialOrdersBoardUnlocked())
		{
			this.isShowingSpecialOrdersBoard = true;
			LargeTerrainFeature bush;
			do
			{
				bush = base.getLargeTerrainFeatureAt(61, 93);
				if (bush != null)
				{
					base.largeTerrainFeatures.Remove(bush);
				}
			}
			while (bush != null);
			base.setMapTile(61, 93, 2045, "Buildings", "Town", "SpecialOrders");
			base.setMapTile(62, 93, 2046, "Buildings", "Town", "SpecialOrders");
			base.setMapTile(63, 93, 2047, "Buildings", "Town", "SpecialOrders");
			base.setMapTile(61, 92, 2013, "Front", "Town");
			base.setMapTile(62, 92, 2014, "Front", "Town");
			base.setMapTile(63, 92, 2015, "Front", "Town");
			base.cleanUpTileForMapOverride(new Point(60, 93));
			base.setMapTile(60, 93, 2034, "Buildings", "Town", "SpecialOrdersPrizeTickets");
			base.setMapTile(60, 92, 2002, "Front", "Town");
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone") && base.currentEvent?.id != "777111")
		{
			if (!Game1.eventUp || base.mapPath.Value == null || !base.mapPath.Value.Contains("Town-Fair"))
			{
				base.ApplyMapOverride("Town-TrashGone", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(57, 68, 17, 5));
			}
			base.ApplyMapOverride("Town-DogHouse", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(51, 65, 5, 6));
			base.removeTile(121, 57, "Buildings");
			base.removeTile(119, 59, "Buildings");
			base.removeTile(124, 56, "Buildings");
			base.removeTile(126, 59, "Buildings");
			base.removeTile(127, 60, "Buildings");
			base.removeTile(125, 61, "Buildings");
			base.removeTile(126, 62, "Buildings");
			base.removeTile(119, 64, "Buildings");
			base.removeTile(120, 52, "Buildings");
		}
		if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
		{
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja"))
			{
				Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(46, 11, 15, 17);
				bool isHalloweenAlternate = Game1.IsFall && Game1.dayOfMonth == 27 && Game1.year % 2 == 0 && base.loadedMapPath.Contains("Halloween");
				if (isHalloweenAlternate)
				{
					base._appliedMapOverrides.Remove("Town-TheaterCC");
				}
				base.ApplyMapOverride("Town-TheaterCC" + (isHalloweenAlternate ? "-Halloween2" : ""), rect, rect);
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle rect2 = new Microsoft.Xna.Framework.Rectangle(84, 41, 27, 15);
				base.ApplyMapOverride("Town-Theater", rect2, rect2);
			}
		}
		else if (Utility.HasAnyPlayerSeenEvent("191393"))
		{
			this.showDestroyedJoja();
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible"))
			{
				this.crackOpenAbandonedJojaMartDoor();
			}
		}
		if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
		{
			this.showImprovedPamHouse();
		}
		if (Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
		{
			this.showTownCommunityUpgradeShortcuts();
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccBoilerRoom"))
		{
			this.minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(6856f, 5008f), Color.White)
			{
				totalNumberOfLoops = 999999,
				interval = 60f,
				flipped = true
			};
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone") && base.currentEvent?.id != "777111" && !base.IsRainingHere() && Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).NextDouble() < 0.2)
		{
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(348, 1916, 12, 20), 999f, 1, 999999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 2f) * 4f, flicker: false, flipped: false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				id = 1
			});
		}
		if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
		{
			if (Game1.player.team.theaterBuildDate.Value < 0)
			{
				Game1.player.team.theaterBuildDate.Value = Game1.Date.TotalDays;
			}
			Point offset = Town.GetTheaterTileOffset();
			MovieTheater.AddMoviePoster(this, (91 + offset.X) * 64 + 32, (48 + offset.Y) * 64 + 64);
			MovieTheater.AddMoviePoster(this, (93 + offset.X) * 64 + 24, (48 + offset.Y) * 64 + 64, isUpcoming: true);
			Vector2 vector_offset = new Vector2(offset.X, offset.Y);
			Game1.currentLightSources.Add(new LightSource("Town_Theater1", 4, (new Vector2(91f, 46f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater2", 4, (new Vector2(96f, 47f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater3", 4, (new Vector2(100f, 47f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater4", 4, (new Vector2(96f, 45f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater5", 4, (new Vector2(100f, 45f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater6", 4, (new Vector2(97f, 43f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater7", 4, (new Vector2(99f, 43f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater8", 4, (new Vector2(98f, 49f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater9", 4, (new Vector2(92f, 49f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater10", 4, (new Vector2(94f, 49f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater11", 4, (new Vector2(98f, 51f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater12", 4, (new Vector2(92f, 51f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Theater13", 4, (new Vector2(94f, 51f) + vector_offset) * 64f, 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
		}
		bool num = base.IsWinterHere();
		if (!num)
		{
			AmbientLocationSounds.addSound(new Vector2(26f, 26f), 0);
			AmbientLocationSounds.addSound(new Vector2(26f, 28f), 0);
		}
		if (!Game1.isStartingToGetDarkOut(this))
		{
			AmbientLocationSounds.addSound(new Vector2(100f, 79f), 2);
			this.addClintMachineGraphics();
		}
		if (Game1.stats.DaysPlayed > 3)
		{
			Random asdfTime = Utility.CreateDaySaveRandom(15.0);
			int delayBeforeStart = Utility.CalculateMinutesBetweenTimes(endTime: Utility.ModifyTime(1920, asdfTime.Next(390)), startTime: Game1.timeOfDay) * Game1.realMilliSecondsPerGameMinute;
			if (delayBeforeStart > 0)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\asldkfjsquaskutanfsldk", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 48), new Vector2(7788f, 5837f), flipped: false, 0f, Color.White)
				{
					animationLength = 8,
					totalNumberOfLoops = 99,
					interval = 100f,
					motion = new Vector2(5f, 0f),
					scale = 5.5f,
					delayBeforeAnimationStart = delayBeforeStart
				});
			}
		}
		if (Game1.player.mailReceived.Contains("checkedBulletinOnce"))
		{
			this.playerCheckedBoard = true;
		}
		if (num && Game1.player.eventsSeen.Contains("520702") && !Game1.player.hasMagnifyingGlass)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(14.5f, 52.75f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(13.5f, 53f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(15.5f, 53f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(16f, 52.25f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(17f, 52f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(17f, 51f) * 64f + new Vector2(8f, 0f) * 4f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(18f, 51f) * 64f + new Vector2(5f, -7f) * 4f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(18f, 50f) * 64f + new Vector2(12f, -2f) * 4f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21.75f, 39.5f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21f, 39f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21.75f, 38.25f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(22.5f, 37.5f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(21.75f, 36.75f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(23f, 36f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(22.25f, 35.25f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(23.5f, 34.6f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(23.5f, 33.6f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(24.25f, 32.6f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(26.75f, 26.75f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(27.5f, 26f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30f, 23f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(31f, 22f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30.5f, 21f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(31f, 20f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30f, 19f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(29f, 18f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(29.1f, 17f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30f, 17.7f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(31.5f, 18.2f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(30.5f, 16.8f) * 64f, flicker: false, flipped: false, 1E-06f, 0f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, 0f, 0f));
		}
		if (Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken") && Game1.isDarkOut(this) && Game1.random.NextDouble() < 0.01)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(3f, 59f) * 64f, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				motion = new Vector2(3f, 0f),
				animationLength = 4,
				interval = 80f,
				totalNumberOfLoops = 200,
				layerDepth = 0.384f,
				xStopCoordinate = 384
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(58f, 108f) * 64f, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				motion = new Vector2(3f, 0f),
				animationLength = 4,
				interval = 80f,
				totalNumberOfLoops = 200,
				layerDepth = 0.384f,
				xStopCoordinate = 4800
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(20f, 92.5f) * 64f, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				motion = new Vector2(3f, 0f),
				animationLength = 4,
				interval = 80f,
				totalNumberOfLoops = 200,
				layerDepth = 0.384f,
				xStopCoordinate = 1664,
				delayBeforeAnimationStart = 1000
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(75f, 1f) * 64f, flipped: true, 0f, Color.White)
			{
				scale = 4f,
				motion = new Vector2(-4f, 0f),
				animationLength = 4,
				interval = 60f,
				totalNumberOfLoops = 200,
				layerDepth = 0.0064f,
				xStopCoordinate = 4352
			});
		}
		Game1.currentLightSources.Add(new LightSource("Town_Saloon", 4, new Vector2(2803f, 4418f), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
		if (num)
		{
			Game1.currentLightSources.Add(new LightSource("Town_AlexHouse_1", 4, new Vector2(3544f, 4005f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_AlexHouse_2", 4, new Vector2(3680f, 3832f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_AlexHouse_3", 4, new Vector2(3877f, 4007f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_PennyHouse", 4, new Vector2(4836f, 4320f), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_SeedShop_1", 4, new Vector2(2514f, 3538f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Plaza_1", 4, new Vector2(2205f, 4950f), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Plaza_2", 4, new Vector2(2205f, 4755f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_SeedShop_2", 4, new Vector2(2981f, 3497f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Hospital_1", 4, new Vector2(2332f, 3192f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_ManorHouse_1", 4, new Vector2(3675f, 5437f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_ManorHouse_2", 4, new Vector2(3853f, 5445f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_HaleyHouse_1", 4, new Vector2(1558f, 5520f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_HaleyHouse_2", 4, new Vector2(1557f, 5613f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_HaleyHouse_3", 4, new Vector2(1307f, 5593f), 0.25f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_SamHouse_1", 4, new Vector2(815f, 5383f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_SamHouse_2", 4, new Vector2(560f, 5384f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_SamHouse_3", 4, new Vector2(671f, 5216f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			if (this.ccRefurbished && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
			{
				Game1.currentLightSources.Add(new LightSource("Town_JojaMart_1", 4, new Vector2(3153f, 1171f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("Town_JojaMart_2", 4, new Vector2(3630f, 1170f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
				Game1.currentLightSources.Add(new LightSource("Town_JojaMart_3", 4, new Vector2(3389f, 1053f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			}
			Action<int, int> obj = delegate(int x, int y)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(256, 432, 64, 80), new Vector2(x, y) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 2.1474836E+09f,
					drawAboveAlwaysFront = true,
					scale = 4f
				});
				Game1.currentLightSources.Add(new LightSource($"Town_WinterTree_{x}_{y}", 9, new Vector2(x, y) * 64f + new Vector2(128f, 160f), 1f, Color.Black * 0.66f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			};
			obj(32, 83);
			obj(42, 96);
			obj(50, 88);
			obj(16, 66);
			obj(29, 49);
			obj(63, 57);
			obj(56, 46);
			obj(5, 58);
			obj(65, 10);
		}
		if (Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
		{
			this.showBookseller = true;
			Game1.currentLightSources.Add(new LightSource("Town_Bookseller_1", 4, new Vector2(7202f, 1634f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			Game1.currentLightSources.Add(new LightSource("Town_Bookseller_2", 4, new Vector2(7032f, 1634f), 0.5f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
		}
		base.addOneTimeGiftBox(ItemRegistry.Create("(O)Book_Trash"), 123, 58, 3);
		base.addOneTimeGiftBox(ItemRegistry.Create("(O)PrizeTicket"), 114, 17);
	}

	private void showTownCommunityUpgradeShortcuts()
	{
		base.removeTile(90, 2, "Buildings");
		base.removeTile(90, 1, "Front");
		base.removeTile(90, 1, "Buildings");
		base.removeTile(90, 0, "Buildings");
		base.setMapTile(89, 1, 360, "Front", "Landscape");
		base.setMapTile(89, 2, 385, "Buildings", "Landscape");
		base.setMapTile(89, 1, 436, "Buildings", "Landscape");
		base.setMapTile(89, 0, 411, "Buildings", "Landscape");
		base.removeTile(98, 4, "Buildings");
		base.removeTile(98, 3, "Buildings");
		base.removeTile(98, 2, "Buildings");
		base.removeTile(98, 1, "Buildings");
		base.removeTile(98, 0, "Buildings");
		base.setMapTile(98, 4, 12, "Back", "v16_landscape2");
		base.setMapTile(98, 3, 509, "Back", "Landscape");
		base.setMapTile(98, 2, 217, "Back", "Landscape");
		base.setMapTile(97, 3, 1683, "Buildings", "Landscape");
		base.setMapTile(97, 3, 509, "Back", "Landscape");
		base.setMapTile(97, 2, 1658, "Buildings", "Landscape");
		base.setMapTile(97, 2, 217, "Back", "Landscape");
		base.setMapTile(98, 2, 1659, "AlwaysFront", "Landscape");
		base.removeTile(92, 104, "Buildings");
		base.removeTile(93, 104, "Buildings");
		base.removeTile(94, 104, "Buildings");
		base.removeTile(92, 105, "Buildings");
		base.removeTile(93, 105, "Buildings");
		base.removeTile(94, 105, "Buildings");
		base.removeTile(93, 106, "Buildings");
		base.removeTile(94, 106, "Buildings");
		base.removeTile(92, 103, "Front");
		base.removeTile(93, 103, "Front");
		base.removeTile(94, 103, "Front");
	}

	public override void cleanupBeforePlayerExit()
	{
		base.cleanupBeforePlayerExit();
		this.minecartSteam = null;
		if (this.mapLoader != null)
		{
			this.mapLoader.Dispose();
			this.mapLoader = null;
		}
	}

	public void initiateMarnieLewisBush()
	{
		Game1.player.freezePause = 3000;
		base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Marnie", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32), new Vector2(48f, 98f) * 64f, flipped: false, 0f, Color.White)
		{
			scale = 4f,
			animationLength = 4,
			interval = 200f,
			totalNumberOfLoops = 99999,
			motion = new Vector2(-3f, -12f),
			acceleration = new Vector2(0f, 0.4f),
			xStopCoordinate = 2880,
			yStopCoordinate = 6336,
			layerDepth = 0.64f,
			reachedStopCoordinate = marnie_landed,
			id = 888
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Lewis", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32), new Vector2(48f, 98f) * 64f, flipped: false, 0f, Color.White)
		{
			scale = 4f,
			animationLength = 4,
			interval = 200f,
			totalNumberOfLoops = 99999,
			motion = new Vector2(3f, -12f),
			acceleration = new Vector2(0f, 0.4f),
			xStopCoordinate = 3264,
			yStopCoordinate = 6336,
			layerDepth = 0.64f,
			id = 777
		});
		Game1.playSound("dwop");
	}

	private void marnie_landed(int extra)
	{
		Game1.player.freezePause = 2000;
		TemporaryAnimatedSprite t = base.getTemporarySpriteByID(777);
		if (t != null)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Lewis", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 32), t.position, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 60f,
				totalNumberOfLoops = 50,
				layerDepth = 0.64f,
				id = 0,
				motion = new Vector2(8f, 0f)
			});
		}
		t = base.getTemporarySpriteByID(888);
		if (t != null)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Marnie", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 32), t.position, flipped: true, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 60f,
				totalNumberOfLoops = 50,
				layerDepth = 0.64f,
				id = 1,
				motion = new Vector2(-8f, 0f)
			});
		}
		base.removeTemporarySpritesWithID(777);
		base.removeTemporarySpritesWithID(888);
		for (int i = 0; i < 3200; i += 200)
		{
			DelayedAction.playSoundAfterDelay("grassyStep", 100 + i);
		}
	}

	public void initiateMagnifyingGlassGet()
	{
		Game1.player.freezePause = 3000;
		if (Game1.player.TilePoint.X >= 31)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), new Vector2(29f, 13f) * 64f, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 200f,
				totalNumberOfLoops = 99999,
				motion = new Vector2(3f, -12f),
				acceleration = new Vector2(0f, 0.4f),
				xStopCoordinate = 2048,
				yStopCoordinate = 960,
				layerDepth = 1f,
				reachedStopCoordinate = mgThief_landed,
				id = 777
			});
		}
		else
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), new Vector2(29f, 13f) * 64f, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 200f,
				totalNumberOfLoops = 99999,
				motion = new Vector2(2f, -12f),
				acceleration = new Vector2(0f, 0.4f),
				xStopCoordinate = 1984,
				yStopCoordinate = 832,
				layerDepth = 0.0896f,
				reachedStopCoordinate = mgThief_landed,
				id = 777
			});
		}
		Game1.playSound("dwop");
	}

	private void mgThief_landed(int extra)
	{
		TemporaryAnimatedSprite mg = base.getTemporarySpriteByID(777);
		if (mg != null)
		{
			mg.animationLength = 1;
			mg.shakeIntensity = 1f;
			mg.interval = 1500f;
			mg.timer = 0f;
			mg.totalNumberOfLoops = 1;
			mg.currentNumberOfLoops = 0;
			mg.endFunction = mgThief_speech;
			Game1.playSound("snowyStep");
		}
	}

	private void mgThief_speech(int extra)
	{
		Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_mgThiefMessage"));
		Game1.afterDialogues = mgThief_afterSpeech;
		TemporaryAnimatedSprite mg = base.getTemporarySpriteByID(777);
		if (mg != null)
		{
			mg.animationLength = 4;
			mg.shakeIntensity = 0f;
			mg.interval = 200f;
			mg.timer = 0f;
			mg.totalNumberOfLoops = 9999;
			mg.currentNumberOfLoops = 0;
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), mg.position, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 200f,
				totalNumberOfLoops = 99999,
				layerDepth = 0.0896f,
				id = 777
			});
		}
	}

	private void mgThief_afterSpeech()
	{
		Game1.player.holdUpItemThenMessage(new SpecialItem(5));
		Game1.afterDialogues = mgThief_afterGlass;
		Game1.player.hasMagnifyingGlass = true;
		Game1.player.removeQuest("31");
	}

	private void mgThief_afterGlass()
	{
		Game1.player.freezePause = 1500;
		TemporaryAnimatedSprite mg = base.getTemporarySpriteByID(777);
		if (mg != null)
		{
			mg.animationLength = 1;
			mg.shakeIntensity = 1f;
			mg.interval = 500f;
			mg.timer = 0f;
			mg.totalNumberOfLoops = 1;
			mg.currentNumberOfLoops = 0;
			mg.endFunction = mg_disappear;
		}
	}

	private void mg_disappear(int extra)
	{
		Game1.player.freezePause = 1000;
		TemporaryAnimatedSprite t = base.getTemporarySpriteByID(777);
		if (t != null)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Krobus", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24), t.position, flipped: false, 0f, Color.White)
			{
				scale = 4f,
				animationLength = 4,
				interval = 60f,
				totalNumberOfLoops = 50,
				layerDepth = 0.0896f,
				id = 777,
				motion = new Vector2(0f, 8f)
			});
			for (int i = 0; i < 3200; i += 200)
			{
				DelayedAction.playSoundAfterDelay("snowyStep", 100 + i);
			}
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.minecartSteam?.update(time);
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		base.draw(spriteBatch);
		this.minecartSteam?.draw(spriteBatch);
		if (this.ccJoja && !base._appliedMapOverrides.Contains("Town-TheaterCC") && !base._appliedMapOverrides.Contains("Town-TheaterCC-Halloween2"))
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.ccFacadePositionBottom), Town.jojaFacadeBottom, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.128f);
		}
		if (!this.playerCheckedBoard)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2616f, 3472f + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.98f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2656f, 3512f + yOffset)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 1f);
		}
		if (Game1.CanAcceptDailyQuest())
		{
			float yOffset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(2692f, 3528f + yOffset2)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset2 / 16f), SpriteEffects.None, 1f);
		}
		if (SpecialOrder.IsSpecialOrdersBoardUnlocked() && !Game1.player.team.acceptedSpecialOrderTypes.Contains("") && !Game1.eventUp)
		{
			float yOffset3 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3997.6f, 5908.8f + yOffset3)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset3 / 8f), SpriteEffects.None, 1f);
		}
		if (Game1.player.stats.Get("specialOrderPrizeTickets") != 0 && !Game1.isFestival())
		{
			float yOffset4 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3832f, 5840f + yOffset4)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.98f);
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2(3872f, 5880f + yOffset4)), new Microsoft.Xna.Framework.Rectangle(240, 240, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
		}
		if (this.showBookseller)
		{
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(52f, 50f) * 64f + new Vector2(6f, 1f) * 4f), new Microsoft.Xna.Framework.Rectangle(258, 335, 26, 29), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.32f);
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(106f, 22f) * 64f + new Vector2(1f, 1f) * 4f), new Microsoft.Xna.Framework.Rectangle(0, 433, 110, 79), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1728f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(1832f, 425f) * 4f), new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), Color.White, 0f, new Vector2(42f, 160f), 4f, SpriteEffects.None, 0.1728f);
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(106f, 22f) * 64f + new Vector2(90f, 14f) * 4f), new Microsoft.Xna.Framework.Rectangle(89, 446, 44, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.17216f);
			if (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 7000.0 < 200.0)
			{
				spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(106f, 22f) * 64f + new Vector2(54f, 41f) * 4f), new Microsoft.Xna.Framework.Rectangle(110, 488, 17, 24), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.17344001f);
			}
			if (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 15000.0 < 1200.0)
			{
				spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(106f, 22f) * 64f + new Vector2(54f, 61f) * 4f), new Microsoft.Xna.Framework.Rectangle(127 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400 / 100 * 17, 508, 17, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.17408f);
			}
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(106f, 22f) * 64f + new Vector2(107f, 21f) * 4f), new Microsoft.Xna.Framework.Rectangle(110 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0) / 100 * 10, 474, 10, 7), Color.White, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.1728f);
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(106f, 22f) * 64f + new Vector2(115f, 21f) * 4f), new Microsoft.Xna.Framework.Rectangle(110 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + 400.0) % 1000.0) / 100 * 10, 467, 10, 7), Color.White, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.1728f);
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(106f, 22f) * 64f + new Vector2(123f, 21f) * 4f), new Microsoft.Xna.Framework.Rectangle(110 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + 200.0) % 1000.0) / 100 * 10, 481, 10, 7), Color.White, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.1728f);
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (this.ccJoja)
		{
			if (!base._appliedMapOverrides.Contains("Town-TheaterCC") && !base._appliedMapOverrides.Contains("Town-TheaterCC-Halloween2"))
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.ccFacadePosition), Town.jojaFacadeTop, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.128f);
				if (base.IsWinterHere())
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.ccFacadePosition), Town.jojaFacadeWinterOverlay, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1281f);
				}
			}
		}
		else if (this.ccRefurbished)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.clockCenter), Town.hourHandSource, Color.White, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1200) / 1200f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes / 23f)), new Vector2(2.5f, 8f), 4f, SpriteEffects.None, 0.98f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.clockCenter), Town.minuteHandSource, Color.White, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1000 % 100 % 60) / 60f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 1.02f)), new Vector2(2.5f, 12f), 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.clockCenter), Town.clockNub, Color.White, 0f, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
		}
		base.drawAboveAlwaysFrontLayer(b);
	}
}
