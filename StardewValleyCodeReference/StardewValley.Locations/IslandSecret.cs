using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Monsters;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandSecret : IslandLocation
{
	[XmlIgnore]
	public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

	[XmlElement("addedSlimesToday")]
	private readonly NetBool addedSlimesToday = new NetBool();

	public IslandSecret()
	{
	}

	public IslandSecret(string map, string name)
		: base(map, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.addedSlimesToday, "addedSlimesToday");
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (this.addedSlimesToday.Value)
		{
			return;
		}
		this.addedSlimesToday.Value = true;
		Random rand = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 12.0);
		Microsoft.Xna.Framework.Rectangle spawnArea = new Microsoft.Xna.Framework.Rectangle(13, 15, 7, 6);
		for (int tries = 5; tries > 0; tries--)
		{
			Vector2 tile = Utility.getRandomPositionInThisRectangle(spawnArea, rand);
			if (this.CanItemBePlacedHere(tile))
			{
				GreenSlime m = new GreenSlime(tile * 64f, 9999899);
				base.characters.Add(m);
			}
		}
		if (rand.NextBool() && this.CanItemBePlacedHere(new Vector2(17f, 18f)))
		{
			base.objects.Add(new Vector2(17f, 18f), ItemRegistry.Create<Object>("(BC)56"));
		}
		GreenSlime slime = new GreenSlime(new Vector2(42f, 34f) * 64f);
		slime.makeTigerSlime();
		base.characters.Add(slime);
		slime = new GreenSlime(new Vector2(38f, 33f) * 64f);
		slime.makeTigerSlime();
		base.characters.Add(slime);
	}

	public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
	{
		if (xLocation == 82 && yLocation == 83 && who.secretNotesSeen.Contains(1002))
		{
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("Island_Secret_BuriedTreasureNut"))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(xLocation, yLocation) * 64f, 1);
				Game1.addMailForTomorrow("Island_Secret_BuriedTreasureNut", noLetter: true, sendToEveryone: true);
			}
			if (!Game1.player.hasOrWillReceiveMail("Island_Secret_BuriedTreasure"))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)166"), new Vector2(xLocation, yLocation) * 64f, 1);
				Game1.addMailForTomorrow("Island_Secret_BuriedTreasure", noLetter: true);
			}
		}
		return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		this.suspensionBridges.Clear();
		this.suspensionBridges.Add(new SuspensionBridge(46, 44));
		this.suspensionBridges.Add(new SuspensionBridge(47, 34));
		NPC n = base.getCharacterFromName("Birdie");
		if (n != null)
		{
			if (n.Sprite.SourceRect.Width < 32)
			{
				n.extendSourceRect(16, 0);
			}
			n.Sprite.SpriteWidth = 32;
			n.Sprite.ignoreSourceRectUpdates = false;
			n.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(8, 1000, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(9, 1000, 0, secondaryArm: false, flip: false)
			});
			n.Sprite.loop = true;
			n.HideShadow = true;
			n.IsInvisible = base.IsRainingHere();
		}
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		NPC birdie = base.getCharacterFromName("Birdie");
		if (birdie != null && !birdie.IsInvisible && birdie.Tile == new Vector2(tileLocation.X, tileLocation.Y))
		{
			if (who.mailReceived.Add("birdieQuestBegun"))
			{
				Game1.globalFadeToBlack(delegate
				{
					this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieIntro")));
				});
			}
			else if (!who.mailReceived.Contains("birdieQuestFinished") && who.ActiveObject?.QualifiedItemId == "(O)870")
			{
				Game1.globalFadeToBlack(delegate
				{
					this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieFinished")));
					who.ActiveObject = null;
				});
				who.mailReceived.Add("birdieQuestFinished");
			}
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.characters.RemoveWhere((NPC npc) => npc is Monster);
		this.addedSlimesToday.Value = false;
		base.DayUpdate(dayOfMonth);
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (ArgUtility.Get(action, 0) == "BananaShrine")
		{
			if (who.CurrentItem?.QualifiedItemId == "(O)91" && base.getTemporarySpriteByID(777) == null)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(304, 48, 16, 16), new Vector2(tileLocation.X, tileLocation.Y - 1) * 64f, flipped: false, 0f, Color.White)
				{
					id = 888,
					scale = 4f,
					layerDepth = ((float)tileLocation.Y + 1.2f) * 64f / 10000f
				});
				base.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32), 400f, 2, 999, new Vector2(15.5f, 20f) * 64f, flicker: false, flipped: false, 0.128f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 777,
					yStopCoordinate = 1561,
					motion = new Vector2(0f, 2f),
					reachedStopCoordinate = gorillaReachedShrine,
					delayBeforeAnimationStart = 1000
				});
				base.playSound("coin");
				DelayedAction.playSoundAfterDelay("grassyStep", 1400);
				DelayedAction.playSoundAfterDelay("grassyStep", 1800);
				DelayedAction.playSoundAfterDelay("grassyStep", 2200);
				DelayedAction.playSoundAfterDelay("grassyStep", 2600);
				DelayedAction.playSoundAfterDelay("grassyStep", 3000);
				who.reduceActiveItemByOne();
				Game1.changeMusicTrack("none");
				DelayedAction.playSoundAfterDelay("gorilla_intro", 2000);
			}
			return true;
		}
		return base.performAction(action, who, tileLocation);
	}

	private void gorillaReachedShrine(int extra)
	{
		TemporaryAnimatedSprite temporarySpriteByID = base.getTemporarySpriteByID(777);
		temporarySpriteByID.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
		temporarySpriteByID.sourceRectStartingPos = Utility.PointToVector2(temporarySpriteByID.sourceRect.Location);
		temporarySpriteByID.currentNumberOfLoops = 0;
		temporarySpriteByID.totalNumberOfLoops = 1;
		temporarySpriteByID.interval = 1000f;
		temporarySpriteByID.timer = 0f;
		temporarySpriteByID.motion = Vector2.Zero;
		temporarySpriteByID.animationLength = 1;
		temporarySpriteByID.endFunction = gorillaGrabBanana;
	}

	private void gorillaGrabBanana(int extra)
	{
		TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
		base.removeTemporarySpritesWithID(888);
		base.playSound("slimeHit");
		gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 352, 32, 32);
		gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
		gorilla.currentNumberOfLoops = 0;
		gorilla.totalNumberOfLoops = 1;
		gorilla.interval = 1000f;
		gorilla.timer = 0f;
		gorilla.animationLength = 1;
		gorilla.endFunction = gorillaEatBanana;
		base.temporarySprites.Add(gorilla);
	}

	private void gorillaEatBanana(int extra)
	{
		TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
		gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(128, 352, 32, 32);
		gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
		gorilla.currentNumberOfLoops = 0;
		gorilla.totalNumberOfLoops = 5;
		gorilla.interval = 300f;
		gorilla.timer = 0f;
		gorilla.animationLength = 2;
		gorilla.endFunction = gorillaAfterEat;
		base.playSound("eat");
		DelayedAction.playSoundAfterDelay("eat", 600);
		DelayedAction.playSoundAfterDelay("eat", 1200);
		DelayedAction.playSoundAfterDelay("eat", 1800);
		DelayedAction.playSoundAfterDelay("eat", 2400);
		base.temporarySprites.Add(gorilla);
	}

	private void gorillaAfterEat(int extra)
	{
		TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
		gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
		gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
		gorilla.currentNumberOfLoops = 0;
		gorilla.totalNumberOfLoops = 1;
		gorilla.interval = 1000f;
		gorilla.timer = 0f;
		gorilla.motion = Vector2.Zero;
		gorilla.animationLength = 1;
		gorilla.endFunction = gorillaSpawnNut;
		gorilla.shakeIntensity = 1f;
		gorilla.shakeIntensityChange = -0.01f;
		base.temporarySprites.Add(gorilla);
	}

	private void gorillaSpawnNut(int extra)
	{
		TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
		gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
		gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
		gorilla.currentNumberOfLoops = 0;
		gorilla.totalNumberOfLoops = 1;
		gorilla.interval = 1000f;
		gorilla.shakeIntensity = 2f;
		gorilla.shakeIntensityChange = -0.01f;
		base.playSound("grunt");
		Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(16.5f, 25f) * 64f, 0, this, 1280);
		gorilla.timer = 0f;
		gorilla.motion = Vector2.Zero;
		gorilla.animationLength = 1;
		gorilla.endFunction = gorillaReturn;
		base.temporarySprites.Add(gorilla);
	}

	private void gorillaReturn(int extra)
	{
		TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
		gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32);
		gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
		gorilla.currentNumberOfLoops = 0;
		gorilla.totalNumberOfLoops = 6;
		gorilla.interval = 200f;
		gorilla.timer = 0f;
		gorilla.motion = new Vector2(0f, -3f);
		gorilla.animationLength = 2;
		gorilla.yStopCoordinate = 1280;
		gorilla.reachedStopCoordinate = delegate
		{
			base.removeTemporarySpritesWithID(777);
		};
		base.temporarySprites.Add(gorilla);
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playMorningSong();
		}, 3000);
	}

	public override void SetBuriedNutLocations()
	{
		base.buriedNutPoints.Add(new Point(23, 47));
		base.buriedNutPoints.Add(new Point(61, 21));
		base.SetBuriedNutLocations();
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		foreach (SuspensionBridge suspensionBridge in this.suspensionBridges)
		{
			suspensionBridge.Update(time);
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		foreach (SuspensionBridge suspensionBridge in this.suspensionBridges)
		{
			suspensionBridge.Draw(b);
		}
	}

	public override bool IsLocationSpecificPlacementRestriction(Vector2 tileLocation)
	{
		foreach (SuspensionBridge suspensionBridge in this.suspensionBridges)
		{
			if (suspensionBridge.CheckPlacementPrevention(tileLocation))
			{
				return true;
			}
		}
		return base.IsLocationSpecificPlacementRestriction(tileLocation);
	}
}
