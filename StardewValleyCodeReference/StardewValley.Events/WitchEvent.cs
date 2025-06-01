using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events;

public class WitchEvent : BaseFarmEvent
{
	public string lightSourceId;

	private Vector2 witchPosition;

	private Building targetBuilding;

	private Farm f;

	private Random r;

	private int witchFrame;

	private int witchAnimationTimer;

	private int animationLoopsDone;

	private int timerSinceFade;

	private bool animateLeft;

	private bool terminate;

	public bool goldenWitch;

	/// <inheritdoc />
	public override bool setUp()
	{
		this.lightSourceId = this.GenerateLightSourceId();
		this.f = Game1.getFarm();
		this.r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		foreach (Building b in this.f.buildings)
		{
			if (!(b.buildingType.Value == "Big Coop") && !(b.buildingType.Value == "Deluxe Coop"))
			{
				continue;
			}
			AnimalHouse animalHouse = (AnimalHouse)b.GetIndoors();
			if (!animalHouse.isFull() && animalHouse.objects.Length < 50 && this.r.NextDouble() < 0.8)
			{
				this.targetBuilding = b;
				if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && this.r.NextDouble() < 0.6)
				{
					this.goldenWitch = true;
				}
			}
		}
		if (this.targetBuilding == null)
		{
			foreach (Building b2 in this.f.buildings)
			{
				if (b2.buildingType.Value == "Slime Hutch")
				{
					GameLocation indoors = b2.GetIndoors();
					if (indoors.characters.Count > 0 && this.r.NextBool() && indoors.numberOfObjectsOfType("83", bigCraftable: true) == 0)
					{
						this.targetBuilding = b2;
					}
				}
			}
		}
		if (this.targetBuilding == null)
		{
			return true;
		}
		Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, this.witchPosition, 2f, Color.Black, LightSource.LightContext.None, 0L));
		Game1.currentLocation = this.f;
		this.f.resetForPlayerEntry();
		Game1.fadeClear();
		Game1.nonWarpFade = true;
		Game1.timeOfDay = 2400;
		Game1.ambientLight = new Color(200, 190, 40);
		Game1.displayHUD = false;
		Game1.freezeControls = true;
		Game1.viewportFreeze = true;
		Game1.displayFarmer = false;
		Game1.viewport.X = Math.Max(0, Math.Min(this.f.map.DisplayWidth - Game1.viewport.Width, this.targetBuilding.tileX.Value * 64 - Game1.viewport.Width / 2));
		Game1.viewport.Y = Math.Max(0, Math.Min(this.f.map.DisplayHeight - Game1.viewport.Height, (this.targetBuilding.tileY.Value - 3) * 64 - Game1.viewport.Height / 2));
		this.witchPosition = new Vector2(Game1.viewport.X + Game1.viewport.Width + 128, this.targetBuilding.tileY.Value * 64 - 64);
		Game1.changeMusicTrack("nightTime");
		DelayedAction.playSoundAfterDelay(this.goldenWitch ? "yoba" : "cacklingWitch", 3200);
		return false;
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		if (this.terminate)
		{
			return true;
		}
		Game1.UpdateGameClock(time);
		this.f.UpdateWhenCurrentLocation(time);
		this.f.updateEvenIfFarmerIsntHere(time);
		Game1.UpdateOther(time);
		Utility.repositionLightSource(this.lightSourceId, this.witchPosition + new Vector2(32f, 32f));
		if (this.animationLoopsDone < 1)
		{
			this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
		}
		if (this.witchPosition.X > (float)(this.targetBuilding.tileX.Value * 64 + 96))
		{
			if (this.timerSinceFade < 2000)
			{
				return false;
			}
			this.witchPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.4f;
			this.witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
		}
		else if (this.animationLoopsDone < 4)
		{
			this.witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
			this.witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
			if (this.witchAnimationTimer > 2000)
			{
				this.witchAnimationTimer = 0;
				if (!this.animateLeft)
				{
					this.witchFrame++;
					if (this.witchFrame == 1)
					{
						this.animateLeft = true;
						for (int i = 0; i < 75; i++)
						{
							this.f.temporarySprites.Add(new TemporaryAnimatedSprite(10, this.witchPosition + new Vector2(8f, 80f), this.goldenWitch ? (this.r.NextBool() ? Color.Gold : new Color(255, 150, 0)) : (this.r.NextBool() ? Color.Lime : Color.DarkViolet))
							{
								motion = new Vector2((float)this.r.Next(-100, 100) / 100f, 1.5f),
								alphaFade = 0.015f,
								delayBeforeAnimationStart = i * 30,
								layerDepth = 1f
							});
						}
						Game1.playSound(this.goldenWitch ? "discoverMineral" : "debuffSpell");
					}
				}
				else
				{
					this.witchFrame--;
					this.animationLoopsDone = 4;
					DelayedAction.playSoundAfterDelay(this.goldenWitch ? "yoba" : "cacklingWitch", 2500);
				}
			}
		}
		else
		{
			this.witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
			this.witchFrame = 0;
			if (this.witchAnimationTimer > 1000 && this.witchPosition.X > -999999f)
			{
				this.witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 256.0) * 2f;
				this.witchPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.4f;
			}
			if (this.witchPosition.X < (float)(Game1.viewport.X - 128) || float.IsNaN(this.witchPosition.X))
			{
				if (!Game1.fadeToBlack && this.witchPosition.X != -999999f)
				{
					Game1.globalFadeToBlack(afterLastFade);
					Game1.changeMusicTrack("none");
					this.timerSinceFade = 0;
					this.witchPosition.X = -999999f;
				}
				this.timerSinceFade += time.ElapsedGameTime.Milliseconds;
			}
		}
		return false;
	}

	public void afterLastFade()
	{
		this.terminate = true;
		Game1.globalFadeToClear();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (this.goldenWitch)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, this.witchPosition), new Rectangle(215, 262 + this.witchFrame * 29, 34, 29), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.witchPosition), new Rectangle(277, 1886 + this.witchFrame * 29, 34, 29), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}
	}

	/// <inheritdoc />
	public override void makeChangesToLocation()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		GameLocation indoors = this.targetBuilding.GetIndoors();
		if (this.targetBuilding.buildingType.Value == "Slime Hutch")
		{
			foreach (NPC character in indoors.characters)
			{
				if (character is GreenSlime slime)
				{
					slime.color.Value = new Color(40 + this.r.Next(10), 40 + this.r.Next(10), 40 + this.r.Next(10));
				}
			}
			return;
		}
		for (int tries = 0; tries < 200; tries++)
		{
			Vector2 v = new Vector2(this.r.Next(2, indoors.Map.Layers[0].LayerWidth - 2), this.r.Next(2, indoors.Map.Layers[0].LayerHeight - 2));
			if ((indoors.CanItemBePlacedHere(v) || (indoors.terrainFeatures.TryGetValue(v, out var terrainFeature) && terrainFeature is Flooring)) && !indoors.objects.ContainsKey(v))
			{
				Object egg = ItemRegistry.Create<Object>(this.goldenWitch ? "(O)928" : "(O)305");
				egg.CanBeSetDown = false;
				egg.IsSpawnedObject = true;
				indoors.objects.Add(v, egg);
				break;
			}
		}
	}
}
