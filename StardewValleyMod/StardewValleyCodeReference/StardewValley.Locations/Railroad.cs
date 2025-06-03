using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class Railroad : GameLocation
{
	/// <summary>The effective chance that a train will appear.</summary>
	private const double TrainChance = 0.09;

	public const int trainSoundDelay = 15000;

	[XmlIgnore]
	public readonly NetRef<Train> train = new NetRef<Train>();

	[XmlElement("hasTrainPassed")]
	private readonly NetBool hasTrainPassed = new NetBool(value: false);

	private int trainTime = -1;

	[XmlIgnore]
	public readonly NetInt trainTimer = new NetInt(0);

	public static ICue trainLoop;

	[XmlElement("witchStatueGone")]
	public readonly NetBool witchStatueGone = new NetBool(value: false);

	/// <summary>The chance for a train to appear on a given day. This is computed from <see cref="F:StardewValley.Locations.Railroad.TrainChance" />.</summary>
	/// <remarks>This should not be modified. See <see cref="F:StardewValley.Locations.Railroad.TrainChance" /> to modify the train spawn chance.</remarks>
	private static double DailyTrainChance;

	static Railroad()
	{
		double trainChance = 0.09;
		if (trainChance < 0.0001)
		{
			trainChance = 0.0001;
		}
		else if (trainChance > 0.2499)
		{
			trainChance = 0.2499;
		}
		Railroad.DailyTrainChance = (1.0 - Math.Sqrt(1.0 - 4.0 * trainChance)) * 0.5;
	}

	public Railroad()
	{
	}

	public Railroad(string map, string name)
		: base(map, name)
	{
	}

	public override void ResetForEvent(Event ev)
	{
		base.ResetForEvent(ev);
		if (ev?.id == "528052")
		{
			ev.eventPositionTileOffset.X -= 8f;
			ev.eventPositionTileOffset.Y -= 2f;
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.train, "train").AddField(this.hasTrainPassed, "hasTrainPassed").AddField(this.witchStatueGone, "witchStatueGone")
			.AddField(this.trainTimer, "trainTimer");
		this.witchStatueGone.fieldChangeEvent += delegate(NetBool field, bool oldValue, bool newValue)
		{
			if (!oldValue && newValue && base.Map != null)
			{
				DelayedAction.removeTileAfterDelay(54, 35, 2000, this, "Buildings");
				DelayedAction.removeTileAfterDelay(54, 34, 2000, this, "Front");
			}
		};
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (this.witchStatueGone.Value || Game1.MasterPlayer.mailReceived.Contains("witchStatueGone"))
		{
			base.removeTile(54, 35, "Buildings");
			base.removeTile(54, 34, "Front");
		}
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
		{
			base.removeTile(24, 34, "Buildings");
			base.removeTile(25, 34, "Buildings");
			base.removeTile(24, 35, "Buildings");
			base.removeTile(25, 35, "Buildings");
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (!base.IsWinterHere())
		{
			AmbientLocationSounds.addSound(new Vector2(15f, 56f), 0);
		}
	}

	public override void cleanupBeforePlayerExit()
	{
		base.cleanupBeforePlayerExit();
		Railroad.trainLoop?.Stop(AudioStopOptions.Immediate);
		Railroad.trainLoop = null;
	}

	public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
	{
		if (who.secretNotesSeen.Contains(16) && xLocation == 12 && yLocation == 38 && who.mailReceived.Add("SecretNote16_done"))
		{
			Game1.createObjectDebris("(O)166", xLocation, yLocation, who.UniqueMultiplayerID, this);
			return "";
		}
		return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet") == 287)
		{
			if (Game1.player.hasDarkTalisman)
			{
				Game1.player.freezePause = 7000;
				base.playSound("fireball");
				DelayedAction.playSoundAfterDelay("secret1", 2000);
				DelayedAction.removeTemporarySpriteAfterDelay(this, 9999, 2000);
				this.witchStatueGone.Value = true;
				who.mailReceived.Add("witchStatueGone");
				for (int i = 0; i < 22; i++)
				{
					DelayedAction.playSoundAfterDelay("batFlap", 2220 + 240 * i);
				}
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(576, 271, 28, 31), 60f, 3, 999, new Vector2(54f, 34f) * 64f + new Vector2(-2f, 1f) * 4f, flicker: false, flipped: false, 0.2176f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					xPeriodic = true,
					xPeriodicLoopTime = 8000f,
					xPeriodicRange = 384f,
					motion = new Vector2(-2f, 0f),
					acceleration = new Vector2(0f, -0.015f),
					pingPong = true,
					delayBeforeAnimationStart = 2000
				});
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 499, 10, 11), 50f, 7, 999, new Vector2(54f, 34f) * 64f + new Vector2(7f, 11f) * 4f, flicker: false, flipped: false, 0.2177f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					xPeriodic = true,
					xPeriodicLoopTime = 8000f,
					xPeriodicRange = 384f,
					motion = new Vector2(-2f, 0f),
					acceleration = new Vector2(0f, -0.015f),
					delayBeforeAnimationStart = 2000
				});
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 499, 10, 11), 35.715f, 7, 8, new Vector2(54f, 34f) * 64f + new Vector2(3f, 10f) * 4f, flicker: false, flipped: false, 0.2305f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 9999
				});
			}
			else
			{
				Game1.drawObjectDialogue("???");
			}
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	internal void ResetTrainForNewDay()
	{
		this.hasTrainPassed.Value = false;
		this.trainTime = -1;
		Random random = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 2);
		Random rngTomorrow = Utility.CreateRandom(Game1.stats.DaysPlayed + 1, Game1.uniqueIDForThisGame / 2);
		bool num = random.NextDouble() < Railroad.DailyTrainChance;
		bool trainTomorrow = rngTomorrow.NextDouble() < Railroad.DailyTrainChance;
		if (num && !trainTomorrow && Game1.isLocationAccessible("Railroad"))
		{
			this.trainTime = 900;
			this.trainTime -= this.trainTime % 10;
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		this.ResetTrainForNewDay();
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
	{
		if (!Game1.eventUp && this.train.Value != null && this.train.Value.getBoundingBox().Intersects(position))
		{
			return true;
		}
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
	}

	public void setTrainComing(int delay)
	{
		this.trainTimer.Value = delay;
		if (Game1.IsMasterGame)
		{
			this.PlayTrainApproach();
			Game1.multiplayer.sendServerToClientsMessage("trainApproach");
		}
	}

	public void PlayTrainApproach()
	{
		bool? flag = Game1.currentLocation?.IsOutdoors;
		if (flag.HasValue && flag == true && !Game1.isFestival() && Game1.currentLocation.InValleyContext())
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:Railroad_TrainComing"));
			Game1.playSound("distantTrain", out var whistle);
			whistle.SetVariable("Volume", 100f);
		}
	}

	public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
	{
		if (Game1.player.secretNotesSeen.Contains(GameLocation.NECKLACE_SECRET_NOTE_INDEX) && !Game1.player.hasOrWillReceiveMail(GameLocation.CAROLINES_NECKLACE_MAIL))
		{
			Game1.player.mailForTomorrow.Add(GameLocation.CAROLINES_NECKLACE_MAIL + "%&NL&%");
			Item result = ItemRegistry.Create(GameLocation.CAROLINES_NECKLACE_ITEM_QID);
			Game1.player.addQuest("128");
			Game1.player.addQuest("129");
			return result;
		}
		return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
	}

	public override bool isTileFishable(int tileX, int tileY)
	{
		if (!base.IsWinterHere())
		{
			return base.isTileFishable(tileX, tileY);
		}
		return false;
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
		if (this.train.Value != null && this.train.Value.Update(time, this) && Game1.IsMasterGame)
		{
			this.train.Value = null;
		}
		if (Game1.IsMasterGame)
		{
			if (Game1.timeOfDay == this.trainTime - this.trainTime % 10 && this.trainTimer.Value <= 0 && !Game1.isFestival() && this.train.Value == null)
			{
				this.setTrainComing(15000);
			}
			if (this.trainTimer.Value > 0)
			{
				this.trainTimer.Value -= time.ElapsedGameTime.Milliseconds;
				if (this.trainTimer.Value <= 0)
				{
					this.train.Value = new Train();
					base.playSound("trainWhistle");
				}
			}
		}
		if (this.trainTimer.Value > 0 && this.trainTimer.Value < 3500)
		{
			this.StartTrainLoopIfNeeded();
		}
		if (this.train.Value != null)
		{
			this.StartTrainLoopIfNeeded();
			ICue cue = Railroad.trainLoop;
			if (cue != null && cue.GetVariable("Volume") < 100f)
			{
				Railroad.trainLoop.SetVariable("Volume", Railroad.trainLoop.GetVariable("Volume") + 0.5f);
			}
		}
		else if (Railroad.trainLoop != null && this.trainTimer.Value <= 0)
		{
			Railroad.trainLoop.SetVariable("Volume", Railroad.trainLoop.GetVariable("Volume") - 0.15f);
			if (Railroad.trainLoop.GetVariable("Volume") <= 0f)
			{
				Railroad.trainLoop.Stop(AudioStopOptions.Immediate);
				Railroad.trainLoop = null;
			}
		}
		else if (this.trainTimer.Value > 0 && Railroad.trainLoop != null)
		{
			Railroad.trainLoop.SetVariable("Volume", Railroad.trainLoop.GetVariable("Volume") + 0.15f);
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (this.train.Value != null && !Game1.eventUp)
		{
			this.train.Value.draw(b, this);
		}
	}

	private void StartTrainLoopIfNeeded()
	{
		if (Game1.currentLocation == this && ((!(Railroad.trainLoop?.IsPlaying)) ?? true))
		{
			Game1.playSound("trainLoop", out Railroad.trainLoop);
			Railroad.trainLoop.SetVariable("Volume", 0f);
		}
	}
}
