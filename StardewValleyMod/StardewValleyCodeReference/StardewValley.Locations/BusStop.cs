using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class BusStop : GameLocation
{
	public const int busDefaultXTile = 21;

	public const int busDefaultYTile = 6;

	private TemporaryAnimatedSprite minecartSteam;

	private TemporaryAnimatedSprite busDoor;

	[XmlIgnore]
	public Vector2 busPosition;

	[XmlIgnore]
	public Vector2 busMotion;

	[XmlIgnore]
	public bool drivingOff;

	[XmlIgnore]
	public bool drivingBack;

	[XmlIgnore]
	public bool leaving;

	private int forceWarpTimer;

	private Microsoft.Xna.Framework.Rectangle busSource = new Microsoft.Xna.Framework.Rectangle(288, 1247, 128, 64);

	private Microsoft.Xna.Framework.Rectangle pamSource = new Microsoft.Xna.Framework.Rectangle(384, 1311, 15, 19);

	private Vector2 pamOffset = new Vector2(0f, 29f);

	/// <summary>The gold price to buy a ticket on the bus.</summary>
	[XmlIgnore]
	public int TicketPrice { get; set; } = 500;

	public BusStop()
	{
	}

	public BusStop(string mapPath, string name)
		: base(mapPath, name)
	{
		this.busPosition = new Vector2(21f, 6f) * 64f;
	}

	/// <inheritdoc />
	public override bool IgnoreTouchActions()
	{
		if (!base.IgnoreTouchActions() && !this.drivingBack)
		{
			return this.drivingOff;
		}
		return true;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (base.getTileIndexAt(tileLocation, "Buildings", "outdoors"))
		{
		case 958:
		case 1080:
		case 1081:
			base.ShowMineCartMenu("Default", "Bus");
			return true;
		case 1057:
			if (Game1.MasterPlayer.mailReceived.Contains("ccVault"))
			{
				if (!Game1.player.isRidingHorse() || Game1.player.mount == null)
				{
					string displayPrice = Utility.getNumberWithCommas(this.TicketPrice);
					if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.es)
					{
						base.createQuestionDialogueWithCustomWidth(Game1.content.LoadString("Strings\\Locations:BusStop_BuyTicketToDesert", displayPrice), base.createYesNoResponses(), "Bus");
					}
					else
					{
						base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_BuyTicketToDesert", displayPrice), base.createYesNoResponses(), "Bus");
					}
					break;
				}
				Game1.player.mount.checkAction(Game1.player, this);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_DesertOutOfService"));
			}
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	private void playerReachedBusDoor(Character c, GameLocation l)
	{
		this.forceWarpTimer = 0;
		Game1.player.position.X = -10000f;
		Game1.changeMusicTrack("silence");
		this.busDriveOff();
		base.playSound("stoneStep");
		if (Game1.player.mount != null)
		{
			Game1.player.mount.farmerPassesThrough = false;
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		if (Game1.netWorldState.Value.canDriveYourselfToday.Value && Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.canDriveYourselfToday.Value = false;
		}
		Object possibleSign = base.getObjectAtTile(25, 10);
		if (possibleSign != null && possibleSign.SpecialVariable == 987659)
		{
			base.objects.Remove(new Vector2(25f, 10f));
		}
	}

	public override bool answerDialogue(Response answer)
	{
		if (base.lastQuestionKey != null && base.afterQuestion == null && ArgUtility.SplitBySpaceAndGet(base.lastQuestionKey, 0) + "_" + answer.responseKey == "Bus_Yes")
		{
			NPC pam = Game1.getCharacterFromName("Pam");
			if (!Game1.netWorldState.Value.canDriveYourselfToday.Value && (!base.characters.Contains(pam) || pam.TilePoint.X != 21 || pam.TilePoint.Y != 10))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NoDriver"));
			}
			else if (Game1.player.Money < this.TicketPrice)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= this.TicketPrice;
				Game1.freezeControls = true;
				Game1.viewportFreeze = true;
				this.forceWarpTimer = 8000;
				Game1.player.controller = new PathFindController(Game1.player, this, new Point(22, 9), 0, playerReachedBusDoor);
				Game1.player.setRunning(isRunning: true);
				if (Game1.player.mount != null)
				{
					Game1.player.mount.farmerPassesThrough = true;
				}
				Desert.warpedToDesert = false;
			}
			return true;
		}
		return base.answerDialogue(answer);
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		this.leaving = false;
		if (Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
		{
			this.minecartSteam = new TemporaryAnimatedSprite(27, new Vector2(1032f, 144f), Color.White)
			{
				totalNumberOfLoops = 999999,
				interval = 60f,
				flipped = true
			};
		}
		if (Game1.getFarm().grandpaScore.Value == 0 && Game1.year >= 3)
		{
			Game1.player.eventsSeen.Remove("558292");
		}
		bool arrived_from_other_location_context = false;
		GameLocation previous_location = Game1.getLocationFromName(Game1.player.previousLocationName);
		if (previous_location != null && previous_location.GetLocationContext() != this.GetLocationContext())
		{
			arrived_from_other_location_context = true;
		}
		if (Game1.player.TilePoint.Y > 16 || Game1.eventUp || Game1.player.TilePoint.X <= 10 || Game1.player.isRidingHorse() || !arrived_from_other_location_context)
		{
			this.drivingOff = false;
			this.drivingBack = false;
			this.busMotion = Vector2.Zero;
			this.busPosition = new Vector2(21f, 6f) * 64f;
			this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 6,
				holdLastFrame = true,
				layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
		}
		else
		{
			Game1.changeMusicTrack("silence");
			this.busPosition = new Vector2(21f, 6f) * 64f;
			this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(368, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = 999999f,
				animationLength = 1,
				holdLastFrame = true,
				layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
				scale = 4f
			};
			Game1.displayFarmer = false;
			this.busDriveBack();
		}
		if (Game1.player.TilePoint.Y > 16 && Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken") && Game1.isDarkOut(this) && Game1.random.NextDouble() < 0.01)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Microsoft.Xna.Framework.Rectangle(448, 546, 16, 25), new Vector2(12f, 6.5f) * 64f, flipped: true, 0f, Color.White)
			{
				scale = 4f,
				motion = new Vector2(-3f, 0f),
				animationLength = 4,
				interval = 80f,
				totalNumberOfLoops = 200,
				layerDepth = 0.0448f,
				delayBeforeAnimationStart = Game1.random.Next(1500)
			});
		}
	}

	public override void cleanupBeforePlayerExit()
	{
		base.cleanupBeforePlayerExit();
		if (base.farmers.Count <= 1)
		{
			this.minecartSteam = null;
			this.busDoor = null;
		}
	}

	public void busDriveOff()
	{
		this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 999999f,
			animationLength = 6,
			holdLastFrame = true,
			layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
			scale = 4f
		};
		this.busDoor.timer = 0f;
		this.busDoor.interval = 70f;
		this.busDoor.endFunction = busStartMovingOff;
		base.localSound("trashcanlid");
		this.drivingBack = false;
		this.busDoor.paused = false;
	}

	public void busDriveBack()
	{
		this.busPosition.X = base.map.RequireLayer("Back").DisplayWidth;
		this.busDoor.Position = this.busPosition + new Vector2(16f, 26f) * 4f;
		this.drivingBack = true;
		this.drivingOff = false;
		base.localSound("busDriveOff");
		this.busMotion = new Vector2(-12f, 0f);
		Game1.freezeControls = true;
	}

	private void busStartMovingOff(int extraInfo)
	{
		Game1.globalFadeToBlack(delegate
		{
			Game1.globalFadeToClear();
			base.localSound("batFlap");
			this.drivingOff = true;
			base.localSound("busDriveOff");
			Game1.changeMusicTrack("silence");
		});
	}

	private void doorOpenAfterReturn(int extraInfo)
	{
		this.busDoor = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), this.busPosition + new Vector2(16f, 26f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 999999f,
			animationLength = 6,
			holdLastFrame = true,
			layerDepth = (this.busPosition.Y + 192f) / 10000f + 1E-05f,
			scale = 4f
		};
		Game1.player.Position = new Vector2(22f, 10f) * 64f;
		base.lastTouchActionLocation = Game1.player.Tile;
		Game1.displayFarmer = true;
		Game1.player.forceCanMove();
		Game1.player.faceDirection(2);
		Game1.changeMusicTrack("none", track_interruptable: true);
		GameLocation.HandleMusicChange(null, this);
	}

	private void busLeftToDesert()
	{
		Game1.viewportFreeze = true;
		Game1.warpFarmer("Desert", 16, 24, flip: true);
		Game1.globalFade = false;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		if (this.drivingBack || this.drivingOff)
		{
			if (Game1.player.currentLocation == this)
			{
				Game1.player.CanMove = false;
			}
			else
			{
				this.drivingBack = false;
				this.drivingOff = false;
			}
		}
		if (this.forceWarpTimer > 0)
		{
			this.forceWarpTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.forceWarpTimer <= 0)
			{
				this.playerReachedBusDoor(Game1.player, this);
			}
		}
		this.minecartSteam?.update(time);
		if (this.drivingOff && !this.leaving)
		{
			this.busMotion.X -= 0.075f;
			if (this.busPosition.X + 512f < 10f)
			{
				this.leaving = true;
				this.busLeftToDesert();
			}
		}
		if (this.drivingBack && this.busMotion != Vector2.Zero)
		{
			Game1.player.Position = this.busPosition;
			if (this.busPosition.X - 1344f < 512f)
			{
				this.busMotion.X = Math.Min(-1f, this.busMotion.X * 0.98f);
			}
			if (Math.Abs(this.busPosition.X - 1344f) <= Math.Abs(this.busMotion.X * 1.5f))
			{
				this.busPosition.X = 1344f;
				this.busMotion = Vector2.Zero;
				Game1.globalFadeToBlack(delegate
				{
					this.drivingBack = false;
					this.busDoor.Position = this.busPosition + new Vector2(16f, 26f) * 4f;
					this.busDoor.pingPong = true;
					this.busDoor.interval = 70f;
					this.busDoor.currentParentTileIndex = 5;
					this.busDoor.endFunction = doorOpenAfterReturn;
					base.localSound("trashcanlid");
					if (!string.IsNullOrEmpty(Game1.player.horseName.Value))
					{
						for (int i = 0; i < base.characters.Count; i++)
						{
							if (base.characters[i] is Horse horse && horse.getOwner() == Game1.player)
							{
								if (string.IsNullOrEmpty(base.characters[i].Name))
								{
									Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:BusStop_ReturnToHorse2", base.characters[i].displayName));
								}
								else
								{
									Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:BusStop_ReturnToHorse" + (Game1.random.Next(2) + 1), base.characters[i].displayName));
								}
								break;
							}
						}
					}
					Game1.globalFadeToClear();
				});
			}
		}
		if (!this.busMotion.Equals(Vector2.Zero))
		{
			this.busPosition += this.busMotion;
			if (this.busDoor != null)
			{
				this.busDoor.Position += this.busMotion;
			}
		}
		this.busDoor?.update(time);
	}

	public override bool shouldHideCharacters()
	{
		if (!this.drivingOff)
		{
			return this.drivingBack;
		}
		return true;
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		base.draw(spriteBatch);
		this.minecartSteam?.draw(spriteBatch);
		spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.busPosition.X, (int)this.busPosition.Y)), this.busSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f) / 10000f);
		this.busDoor?.draw(spriteBatch);
		if ((Game1.netWorldState.Value.canDriveYourselfToday.Value && (this.drivingOff || this.drivingBack)) || (this.drivingBack && Desert.warpedToDesert))
		{
			Game1.player.faceDirection(3);
			Game1.player.blinkTimer = -1000;
			Game1.player.FarmerRenderer.draw(spriteBatch, new FarmerSprite.AnimationFrame(117, 99999, 0, secondaryArm: false, flip: true), 117, new Microsoft.Xna.Framework.Rectangle(48, 608, 16, 32), Game1.GlobalToLocal(new Vector2((int)(this.busPosition.X + 4f), (int)(this.busPosition.Y - 8f)) + this.pamOffset * 4f), Vector2.Zero, (this.busPosition.Y + 192f + 4f) / 10000f, Color.White, 0f, 1f, Game1.player);
			spriteBatch.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.busPosition.X, (int)this.busPosition.Y - 40) + this.pamOffset * 4f), new Microsoft.Xna.Framework.Rectangle(0, 0, 21, 41), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f + 8f) / 10000f);
		}
		else if (this.drivingOff || this.drivingBack)
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)this.busPosition.X, (int)this.busPosition.Y) + this.pamOffset * 4f), this.pamSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.busPosition.Y + 192f + 4f) / 10000f);
		}
	}
}
