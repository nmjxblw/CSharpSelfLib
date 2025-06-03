using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace StardewValley.Characters;

public class Junimo : NPC
{
	private readonly NetFloat alpha = new NetFloat(1f);

	private readonly NetFloat alphaChange = new NetFloat();

	public readonly NetInt whichArea = new NetInt();

	public readonly NetBool friendly = new NetBool();

	public readonly NetBool holdingStar = new NetBool();

	public readonly NetBool holdingBundle = new NetBool();

	public readonly NetBool temporaryJunimo = new NetBool();

	public readonly NetBool stayPut = new NetBool();

	private readonly NetVector2 motion = new NetVector2(Vector2.Zero);

	private new readonly NetRectangle nextPosition = new NetRectangle();

	private readonly NetColor color = new NetColor();

	private readonly NetColor bundleColor = new NetColor();

	private readonly NetBool sayingGoodbye = new NetBool();

	private readonly NetEvent0 setReturnToJunimoHutToFetchStarControllerEvent = new NetEvent0();

	private readonly NetEvent0 setBringBundleBackToHutControllerEvent = new NetEvent0();

	private readonly NetEvent0 setJunimoReachedHutToFetchStarControllerEvent = new NetEvent0();

	private readonly NetEvent0 starDoneSpinningEvent = new NetEvent0();

	private readonly NetEvent0 returnToJunimoHutToFetchFinalStarEvent = new NetEvent0();

	private int farmerCloseCheckTimer = 100;

	private static int soundTimer;

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public Junimo()
	{
		base.forceUpdateTimer = 9999;
	}

	public Junimo(Vector2 position, int whichArea, bool temporary = false)
		: base(new AnimatedSprite("Characters\\Junimo", 0, 16, 16), position, 2, "Junimo")
	{
		this.whichArea.Value = whichArea;
		try
		{
			this.friendly.Value = Game1.RequireLocation<CommunityCenter>("CommunityCenter").areasComplete[whichArea];
		}
		catch (Exception)
		{
			this.friendly.Value = true;
		}
		if (whichArea == 6)
		{
			this.friendly.Value = false;
		}
		this.temporaryJunimo.Value = temporary;
		this.nextPosition.Value = this.GetBoundingBox();
		base.Breather = false;
		base.speed = 3;
		base.forceUpdateTimer = 9999;
		base.collidesWithOtherCharacters.Value = true;
		base.farmerPassesThrough = true;
		base.Scale = 0.75f;
		if (this.temporaryJunimo.Value)
		{
			if (Game1.random.NextDouble() < 0.01)
			{
				switch (Game1.random.Next(8))
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
				case 7:
					this.color.Value = Color.Salmon;
					break;
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					this.color.Value = Color.White;
				}
			}
			else
			{
				switch (Game1.random.Next(8))
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
				case 7:
					this.color.Value = Color.Turquoise;
					break;
				}
			}
		}
		else
		{
			switch (whichArea)
			{
			case -1:
			case 0:
				this.color.Value = Color.LimeGreen;
				break;
			case 1:
				this.color.Value = Color.Orange;
				break;
			case 2:
				this.color.Value = Color.Turquoise;
				break;
			case 3:
				this.color.Value = Color.Tan;
				break;
			case 4:
				this.color.Value = Color.Gold;
				break;
			case 5:
				this.color.Value = Color.BlanchedAlmond;
				break;
			case 6:
				this.color.Value = new Color(160, 20, 220);
				break;
			}
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.alpha, "alpha").AddField(this.alphaChange, "alphaChange").AddField(this.whichArea, "whichArea")
			.AddField(this.friendly, "friendly")
			.AddField(this.holdingStar, "holdingStar")
			.AddField(this.holdingBundle, "holdingBundle")
			.AddField(this.temporaryJunimo, "temporaryJunimo")
			.AddField(this.stayPut, "stayPut")
			.AddField(this.motion, "motion")
			.AddField(this.nextPosition, "nextPosition")
			.AddField(this.color, "color")
			.AddField(this.bundleColor, "bundleColor")
			.AddField(this.sayingGoodbye, "sayingGoodbye")
			.AddField(this.setReturnToJunimoHutToFetchStarControllerEvent, "setReturnToJunimoHutToFetchStarControllerEvent")
			.AddField(this.setBringBundleBackToHutControllerEvent, "setBringBundleBackToHutControllerEvent")
			.AddField(this.setJunimoReachedHutToFetchStarControllerEvent, "setJunimoReachedHutToFetchStarControllerEvent")
			.AddField(this.starDoneSpinningEvent, "starDoneSpinningEvent")
			.AddField(this.returnToJunimoHutToFetchFinalStarEvent, "returnToJunimoHutToFetchFinalStarEvent");
		this.setReturnToJunimoHutToFetchStarControllerEvent.onEvent += setReturnToJunimoHutToFetchStarController;
		this.setBringBundleBackToHutControllerEvent.onEvent += setBringBundleBackToHutController;
		this.setJunimoReachedHutToFetchStarControllerEvent.onEvent += setJunimoReachedHutToFetchStarController;
		this.starDoneSpinningEvent.onEvent += performStartDoneSpinning;
		this.returnToJunimoHutToFetchFinalStarEvent.onEvent += returnToJunimoHutToFetchFinalStar;
		base.position.Field.AxisAlignedMovement = false;
	}

	public override bool canPassThroughActionTiles()
	{
		return false;
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public override bool canTalk()
	{
		return false;
	}

	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
	}

	public void fadeAway()
	{
		base.collidesWithOtherCharacters.Value = false;
		this.alphaChange.Value = (this.stayPut.Value ? (-0.005f) : (-0.015f));
	}

	public void setAlpha(float a)
	{
		this.alpha.Value = a;
	}

	public void fadeBack()
	{
		this.alpha.Value = 0f;
		this.alphaChange.Value = 0.02f;
		base.IsInvisible = false;
	}

	public void setMoving(int xSpeed, int ySpeed)
	{
		this.motion.X = xSpeed;
		this.motion.Y = ySpeed;
	}

	public void setMoving(Vector2 motion)
	{
		this.motion.Value = motion;
	}

	public override void Halt()
	{
		base.Halt();
		this.motion.Value = Vector2.Zero;
	}

	public void returnToJunimoHut(GameLocation location)
	{
		base.currentLocation = location;
		this.jump();
		base.collidesWithOtherCharacters.Value = false;
		base.controller = new PathFindController(this, location, new Point(25, 10), 0, junimoReachedHut);
		location.playSound("junimoMeep1");
	}

	public void stayStill()
	{
		this.stayPut.Value = true;
		this.motion.Value = Vector2.Zero;
	}

	public void allowToMoveAgain()
	{
		this.stayPut.Value = false;
	}

	private void returnToJunimoHutToFetchFinalStar()
	{
		if (base.currentLocation == Game1.currentLocation)
		{
			Game1.globalFadeToBlack(finalCutscene, 0.005f);
			Game1.freezeControls = true;
			Game1.flashAlpha = 1f;
		}
	}

	public void returnToJunimoHutToFetchStar(GameLocation location)
	{
		base.currentLocation = location;
		this.friendly.Value = true;
		CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		if (communityCenter.areAllAreasComplete())
		{
			this.returnToJunimoHutToFetchFinalStarEvent.Fire();
			base.collidesWithOtherCharacters.Value = false;
			base.farmerPassesThrough = false;
			this.stayStill();
			this.faceDirection(0);
			Game1.player.mailReceived.Add("ccIsComplete");
			if (Game1.currentLocation.Equals(communityCenter))
			{
				communityCenter.addStarToPlaque();
			}
		}
		else
		{
			DelayedAction.textAboveHeadAfterDelay(Game1.random.NextBool() ? Game1.content.LoadString("Strings\\Characters:JunimoTextAboveHead1") : Game1.content.LoadString("Strings\\Characters:JunimoTextAboveHead2"), this, Game1.random.Next(3000, 6000));
			this.setReturnToJunimoHutToFetchStarControllerEvent.Fire();
			location.playSound("junimoMeep1");
			base.collidesWithOtherCharacters.Value = false;
			base.farmerPassesThrough = false;
			this.holdingBundle.Value = true;
			base.speed = 3;
		}
	}

	private void setReturnToJunimoHutToFetchStarController()
	{
		if (Game1.IsMasterGame)
		{
			base.controller = new PathFindController(this, base.currentLocation, new Point(25, 10), 0, junimoReachedHutToFetchStar);
		}
	}

	private void finalCutscene()
	{
		base.collidesWithOtherCharacters.Value = false;
		base.farmerPassesThrough = false;
		Game1.RequireLocation<CommunityCenter>("CommunityCenter").prepareForJunimoDance();
		Game1.player.Position = new Vector2(29f, 11f) * 64f;
		Game1.player.completelyStopAnimatingOrDoingAction();
		Game1.player.faceDirection(3);
		Point playerPixel = Game1.player.StandingPixel;
		Game1.UpdateViewPort(overrideFreeze: true, playerPixel);
		Game1.viewport.X = playerPixel.X - Game1.viewport.Width / 2;
		Game1.viewport.Y = playerPixel.Y - Game1.viewport.Height / 2;
		Game1.viewportTarget = Vector2.Zero;
		Game1.viewportCenter = playerPixel;
		Game1.moveViewportTo(new Vector2(32.5f, 6f) * 64f, 2f, 999999);
		Game1.globalFadeToClear(goodbyeDance, 0.005f);
		Game1.pauseTime = 1000f;
		Game1.freezeControls = true;
	}

	public void bringBundleBackToHut(Color bundleColor, GameLocation location)
	{
		base.currentLocation = location;
		if (this.holdingBundle.Value)
		{
			return;
		}
		base.Position = Utility.getRandomAdjacentOpenTile(Game1.player.Tile, location) * 64f;
		int iter = 0;
		while (location.isCollidingPosition(this.GetBoundingBox(), Game1.viewport, this) && iter < 5)
		{
			base.Position = Utility.getRandomAdjacentOpenTile(Game1.player.Tile, location) * 64f;
			iter++;
		}
		if (iter < 5)
		{
			if (Game1.random.NextDouble() < 0.25)
			{
				DelayedAction.textAboveHeadAfterDelay(Game1.random.NextBool() ? Game1.content.LoadString("Strings\\Characters:JunimoThankYou1") : Game1.content.LoadString("Strings\\Characters:JunimoThankYou2"), this, Game1.random.Next(3000, 6000));
			}
			this.fadeBack();
			this.bundleColor.Value = bundleColor;
			this.setBringBundleBackToHutControllerEvent.Fire();
			base.collidesWithOtherCharacters.Value = false;
			base.farmerPassesThrough = false;
			this.holdingBundle.Value = true;
			base.speed = 1;
		}
	}

	private void setBringBundleBackToHutController()
	{
		if (Game1.IsMasterGame)
		{
			base.controller = new PathFindController(this, base.currentLocation, new Point(25, 10), 0, junimoReachedHutToReturnBundle);
		}
	}

	private void junimoReachedHutToReturnBundle(Character c, GameLocation l)
	{
		base.currentLocation = l;
		this.holdingBundle.Value = false;
		base.collidesWithOtherCharacters.Value = true;
		base.farmerPassesThrough = true;
		l.playSound("Ship");
	}

	private void junimoReachedHutToFetchStar(Character c, GameLocation l)
	{
		base.currentLocation = l;
		this.holdingStar.Value = true;
		this.holdingBundle.Value = false;
		base.speed = 1;
		base.collidesWithOtherCharacters.Value = false;
		base.farmerPassesThrough = false;
		this.setJunimoReachedHutToFetchStarControllerEvent.Fire();
		l.playSound("dwop");
		base.farmerPassesThrough = false;
	}

	private void setJunimoReachedHutToFetchStarController()
	{
		if (Game1.IsMasterGame)
		{
			base.controller = new PathFindController(this, base.currentLocation, new Point(32, 9), 2, placeStar);
		}
	}

	private void placeStar(Character c, GameLocation l)
	{
		base.currentLocation = l;
		base.collidesWithOtherCharacters.Value = false;
		base.farmerPassesThrough = true;
		this.holdingStar.Value = false;
		l.playSound("tinyWhip");
		this.friendly.Value = true;
		base.speed = 3;
		Game1.multiplayer.broadcastSprites(l, new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Rectangle(0, 109, 16, 19), 40f, 8, 10, base.Position + new Vector2(0f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f * base.scale.Value, 0f, 0f, 0f)
		{
			endFunction = starDoneSpinning,
			motion = new Vector2(0.22f, -2f),
			acceleration = new Vector2(0f, 0.01f),
			id = 777
		});
	}

	public void sayGoodbye()
	{
		this.sayingGoodbye.Value = true;
		base.farmerPassesThrough = true;
	}

	private void goodbyeDance()
	{
		Game1.player.faceDirection(3);
		Game1.RequireLocation<CommunityCenter>("CommunityCenter").junimoGoodbyeDance();
	}

	private void starDoneSpinning(int extraInfo)
	{
		this.starDoneSpinningEvent.Fire();
		(base.currentLocation as CommunityCenter).addStarToPlaque();
	}

	private void performStartDoneSpinning()
	{
		if (Game1.currentLocation is CommunityCenter)
		{
			Game1.playSound("yoba");
			Game1.flashAlpha = 1f;
			Game1.playSound("yoba");
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (base.textAboveHeadTimer > 0 && base.textAboveHead != null)
		{
			Point standingPixel = base.StandingPixel;
			Vector2 local = Game1.GlobalToLocal(new Vector2(standingPixel.X, (float)standingPixel.Y - 128f + (float)base.yJumpOffset));
			if (base.textAboveHeadStyle == 0)
			{
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			SpriteText.drawStringWithScrollCenteredAt(b, base.textAboveHead, (int)local.X, (int)local.Y, "", base.textAboveHeadAlpha, base.textAboveHeadColor, 1, (float)(base.TilePoint.Y * 64) / 10000f + 0.001f + (float)base.TilePoint.X / 10000f, !this.sayingGoodbye.Value);
		}
	}

	public void junimoReachedHut(Character c, GameLocation l)
	{
		base.currentLocation = l;
		this.fadeAway();
		base.controller = null;
		this.motion.X = 0f;
		this.motion.Y = -1f;
	}

	protected override void updateSlaveAnimation(GameTime time)
	{
		if (this.sayingGoodbye.Value || this.temporaryJunimo.Value)
		{
			return;
		}
		if (this.holdingStar.Value || this.holdingBundle.Value)
		{
			this.Sprite.Animate(time, 44, 4, 200f);
		}
		else if (base.position.IsInterpolating())
		{
			switch (this.FacingDirection)
			{
			case 1:
				base.flip = false;
				this.Sprite.Animate(time, 16, 8, 50f);
				break;
			case 3:
				base.flip = true;
				this.Sprite.Animate(time, 16, 8, 50f);
				break;
			case 0:
				this.Sprite.Animate(time, 32, 8, 50f);
				break;
			default:
				this.Sprite.Animate(time, 0, 8, 50f);
				break;
			}
		}
		else
		{
			this.Sprite.Animate(time, 8, 4, 100f);
		}
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.currentLocation = location;
		this.setReturnToJunimoHutToFetchStarControllerEvent.Poll();
		this.setBringBundleBackToHutControllerEvent.Poll();
		this.setJunimoReachedHutToFetchStarControllerEvent.Poll();
		this.starDoneSpinningEvent.Poll();
		this.returnToJunimoHutToFetchFinalStarEvent.Poll();
		base.update(time, location);
		base.forceUpdateTimer = 99999;
		if (this.sayingGoodbye.Value)
		{
			base.flip = false;
			if (this.whichArea.Value % 2 == 0)
			{
				this.Sprite.Animate(time, 16, 8, 50f);
			}
			else
			{
				this.Sprite.Animate(time, 28, 4, 80f);
			}
			if (!base.IsInvisible && Game1.random.NextDouble() < 0.009999999776482582 && base.yJumpOffset == 0)
			{
				this.jump();
				if (Game1.random.NextDouble() < 0.15 && Game1.player.Tile.X == 29f && Game1.player.Tile.Y == 11f)
				{
					base.showTextAboveHead(Game1.random.NextBool() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Junimo.cs.6625") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Junimo.cs.6626"));
				}
			}
			this.alpha.Value += this.alphaChange.Value;
			if (this.alpha.Value > 1f)
			{
				this.alpha.Value = 1f;
				this.alphaChange.Value = 0f;
			}
			if (this.alpha.Value < 0f)
			{
				this.alpha.Value = 0f;
				base.IsInvisible = true;
				base.HideShadow = true;
			}
		}
		else if (this.temporaryJunimo.Value)
		{
			this.Sprite.Animate(time, 12, 4, 100f);
			if (Game1.random.NextDouble() < 0.001)
			{
				this.jumpWithoutSound();
				location.localSound("junimoMeep1");
			}
		}
		else
		{
			if (this.EventActor)
			{
				return;
			}
			this.alpha.Value += this.alphaChange.Value;
			if (this.alpha.Value > 1f)
			{
				this.alpha.Value = 1f;
				base.HideShadow = false;
			}
			else if (this.alpha.Value < 0f)
			{
				this.alpha.Value = 0f;
				base.IsInvisible = true;
				base.HideShadow = true;
			}
			Junimo.soundTimer--;
			this.farmerCloseCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.sayingGoodbye.Value || this.temporaryJunimo.Value || !Game1.IsMasterGame)
			{
				return;
			}
			if (!base.IsInvisible && this.farmerCloseCheckTimer <= 0 && base.controller == null && this.alpha.Value >= 1f && !this.stayPut.Value && Game1.IsMasterGame)
			{
				this.farmerCloseCheckTimer = 100;
				if (this.holdingStar.Value)
				{
					this.setJunimoReachedHutToFetchStarController();
				}
				else
				{
					Farmer f = Utility.isThereAFarmerWithinDistance(base.Tile, 5, base.currentLocation);
					if (f != null)
					{
						if (this.friendly.Value && Vector2.Distance(base.Position, f.Position) > (float)(base.speed * 4))
						{
							if (this.motion.Equals(Vector2.Zero) && Junimo.soundTimer <= 0)
							{
								this.jump();
								location.localSound("junimoMeep1");
								Junimo.soundTimer = 400;
							}
							if (Game1.random.NextDouble() < 0.007)
							{
								this.jumpWithoutSound(Game1.random.Next(6, 9));
							}
							this.setMoving(Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), base.speed, f));
						}
						else if (!this.friendly.Value)
						{
							this.fadeAway();
							Vector2 v = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), f);
							v.Normalize();
							v.Y *= -1f;
							this.setMoving(v * base.speed);
						}
						else if (this.alpha.Value >= 1f)
						{
							this.motion.Value = Vector2.Zero;
						}
					}
					else if (this.alpha.Value >= 1f)
					{
						this.motion.Value = Vector2.Zero;
					}
				}
			}
			if (!base.IsInvisible && base.controller == null)
			{
				this.nextPosition.Value = this.GetBoundingBox();
				this.nextPosition.X += (int)this.motion.X;
				bool sparkle = false;
				if (!location.isCollidingPosition(this.nextPosition.Value, Game1.viewport, this))
				{
					base.position.X += (int)this.motion.X;
					sparkle = true;
				}
				this.nextPosition.X -= (int)this.motion.X;
				this.nextPosition.Y += (int)this.motion.Y;
				if (!location.isCollidingPosition(this.nextPosition.Value, Game1.viewport, this))
				{
					base.position.Y += (int)this.motion.Y;
					sparkle = true;
				}
				if (!this.motion.Equals(Vector2.Zero) && sparkle && Game1.random.NextDouble() < 0.005)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.Choose(10, 11), base.Position, this.color.Value)
					{
						motion = this.motion.Value / 4f,
						alphaFade = 0.01f,
						layerDepth = 0.8f,
						scale = 0.75f,
						alpha = 0.75f
					});
				}
			}
			if (base.controller != null || !this.motion.Equals(Vector2.Zero))
			{
				if (this.holdingStar.Value || this.holdingBundle.Value)
				{
					this.Sprite.Animate(time, 44, 4, 200f);
				}
				else if (base.moveRight || (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X > 0f))
				{
					base.flip = false;
					this.Sprite.Animate(time, 16, 8, 50f);
				}
				else if (base.moveLeft || (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X < 0f))
				{
					this.Sprite.Animate(time, 16, 8, 50f);
					base.flip = true;
				}
				else if (base.moveUp || (Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y < 0f))
				{
					this.Sprite.Animate(time, 32, 8, 50f);
				}
				else
				{
					this.Sprite.Animate(time, 0, 8, 50f);
				}
			}
			else
			{
				this.Sprite.Animate(time, 8, 4, 100f);
			}
		}
	}

	public override void draw(SpriteBatch b, float alpha = 1f)
	{
		if (!base.IsInvisible)
		{
			this.Sprite.UpdateSourceRect();
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)this.Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow(this.Sprite.SpriteHeight / 16, 2.0) + (float)base.yJumpOffset - 8f) + ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), this.Sprite.SourceRect, this.color.Value * this.alpha.Value, base.rotation, new Vector2(this.Sprite.SpriteWidth * 4 / 2, (float)(this.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
			if (this.holdingStar.Value)
			{
				b.Draw(this.Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(8f, -64f * base.scale.Value + 4f + (float)base.yJumpOffset)), new Rectangle(0, 109, 16, 19), Color.White * this.alpha.Value, 0f, Vector2.Zero, 4f * base.scale.Value, SpriteEffects.None, base.Position.Y / 10000f + 0.0001f);
			}
			else if (this.holdingBundle.Value)
			{
				b.Draw(this.Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(8f, -64f * base.scale.Value + 20f + (float)base.yJumpOffset)), new Rectangle(0, 96, 16, 13), this.bundleColor.Value * this.alpha.Value, 0f, Vector2.Zero, 4f * base.scale.Value, SpriteEffects.None, base.Position.Y / 10000f + 0.0001f);
			}
		}
	}

	/// <inheritdoc />
	public override void DrawShadow(SpriteBatch b)
	{
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2((float)(this.Sprite.SpriteWidth * 4) / 2f, 44f)), Game1.shadowTexture.Bounds, this.color.Value * this.alpha.Value, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)base.yJumpOffset / 40f) * base.scale.Value, SpriteEffects.None, Math.Max(0f, (float)base.StandingPixel.Y / 10000f) - 1E-06f);
	}
}
