using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley.Buildings;

public class JunimoHut : Building
{
	public int cropHarvestRadius = 8;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="M:StardewValley.Buildings.JunimoHut.GetOutputChest" /> instead.</summary>
	[XmlElement("output")]
	public Chest obsolete_output;

	[XmlElement("noHarvest")]
	public readonly NetBool noHarvest = new NetBool();

	[XmlElement("wasLit")]
	public readonly NetBool wasLit = new NetBool(value: false);

	private int junimoSendOutTimer;

	[XmlIgnore]
	public List<JunimoHarvester> myJunimos = new List<JunimoHarvester>();

	[XmlIgnore]
	public Point lastKnownCropLocation = Point.Zero;

	public NetInt raisinDays = new NetInt();

	[XmlElement("shouldSendOutJunimos")]
	public NetBool shouldSendOutJunimos = new NetBool(value: false);

	private Rectangle lightInteriorRect = new Rectangle(195, 0, 18, 17);

	private Rectangle bagRect = new Rectangle(208, 51, 15, 13);

	public JunimoHut(Vector2 tileLocation)
		: base("Junimo Hut", tileLocation)
	{
	}

	public JunimoHut()
		: this(Vector2.Zero)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.noHarvest, "noHarvest").AddField(this.wasLit, "wasLit").AddField(this.shouldSendOutJunimos, "shouldSendOutJunimos")
			.AddField(this.raisinDays, "raisinDays");
		this.wasLit.fieldChangeVisibleEvent += delegate
		{
			this.updateLightState();
		};
	}

	public override Rectangle getRectForAnimalDoor(BuildingData data)
	{
		return new Rectangle((1 + base.tileX.Value) * 64, (base.tileY.Value + 1) * 64, 64, 64);
	}

	public override Rectangle? getSourceRectForMenu()
	{
		return new Rectangle(Game1.GetSeasonIndexForLocation(base.GetParentLocation()) * 48, 0, 48, 64);
	}

	public Chest GetOutputChest()
	{
		return base.GetBuildingChest("Output");
	}

	public override void dayUpdate(int dayOfMonth)
	{
		base.dayUpdate(dayOfMonth);
		this.myJunimos.Clear();
		this.wasLit.Value = false;
		this.shouldSendOutJunimos.Value = true;
		if (this.raisinDays.Value > 0 && !Game1.IsWinter)
		{
			this.raisinDays.Value--;
		}
		if (this.raisinDays.Value == 0 && !Game1.IsWinter)
		{
			Chest output = this.GetOutputChest();
			if (output.Items.CountId("(O)Raisins") > 0)
			{
				this.raisinDays.Value += 7;
				output.Items.ReduceId("(O)Raisins", 1);
			}
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.isActive() && f.currentLocation != null && (f.currentLocation is FarmHouse || f.currentLocation.isStructure.Value))
			{
				this.shouldSendOutJunimos.Value = false;
			}
		}
	}

	public void sendOutJunimos()
	{
		this.junimoSendOutTimer = 1000;
	}

	/// <inheritdoc />
	public override void performActionOnConstruction(GameLocation location, Farmer who)
	{
		base.performActionOnConstruction(location, who);
		this.sendOutJunimos();
	}

	public override void resetLocalState()
	{
		base.resetLocalState();
		this.updateLightState();
	}

	public void updateLightState()
	{
		if (!base.IsInCurrentLocation())
		{
			return;
		}
		string lightSourceId = $"{"JunimoHut"}_{base.tileX}_{base.tileY}";
		if (this.wasLit.Value)
		{
			if (Utility.getLightSource(lightSourceId) == null)
			{
				Game1.currentLightSources.Add(new LightSource(lightSourceId, 4, new Vector2(base.tileX.Value + 1, base.tileY.Value + 1) * 64f + new Vector2(32f, 32f), 0.5f, LightSource.LightContext.None, 0L, base.parentLocationName.Value));
			}
			AmbientLocationSounds.addSound(new Vector2(base.tileX.Value + 1, base.tileY.Value + 1), 1);
		}
		else
		{
			Utility.removeLightSource(lightSourceId);
			AmbientLocationSounds.removeSound(new Vector2(base.tileX.Value + 1, base.tileY.Value + 1));
		}
	}

	public int getUnusedJunimoNumber()
	{
		for (int i = 0; i < 3; i++)
		{
			if (i >= this.myJunimos.Count)
			{
				return i;
			}
			bool found = false;
			foreach (JunimoHarvester myJunimo in this.myJunimos)
			{
				if (myJunimo.whichJunimoFromThisHut == i)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				return i;
			}
		}
		return 2;
	}

	public override void updateWhenFarmNotCurrentLocation(GameTime time)
	{
		base.updateWhenFarmNotCurrentLocation(time);
		GameLocation location = base.GetParentLocation();
		Chest output = this.GetOutputChest();
		if (output?.mutex != null)
		{
			output.mutex.Update(location);
			if (output.mutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				output.mutex.ReleaseLock();
			}
		}
		if (!Game1.IsMasterGame || this.junimoSendOutTimer <= 0 || !this.shouldSendOutJunimos.Value)
		{
			return;
		}
		this.junimoSendOutTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.junimoSendOutTimer > 0 || this.myJunimos.Count >= 3 || location.IsWinterHere() || location.IsRainingHere() || !this.areThereMatureCropsWithinRadius() || (!(location.NameOrUniqueName != "Farm") && Game1.farmEvent != null))
		{
			return;
		}
		int junimoNumber = this.getUnusedJunimoNumber();
		bool isPrismatic = false;
		Color? gemColor = this.getGemColor(ref isPrismatic);
		JunimoHarvester j = new JunimoHarvester(location, new Vector2(base.tileX.Value + 1, base.tileY.Value + 1) * 64f + new Vector2(0f, 32f), this, junimoNumber, gemColor);
		j.isPrismatic.Value = isPrismatic;
		location.characters.Add(j);
		this.myJunimos.Add(j);
		this.junimoSendOutTimer = 1000;
		if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2(base.tileX.Value + 1, base.tileY.Value + 1)), 64, location))
		{
			try
			{
				location.playSound("junimoMeep1");
			}
			catch (Exception)
			{
			}
		}
	}

	public override void Update(GameTime time)
	{
		if (!this.shouldSendOutJunimos.Value)
		{
			this.shouldSendOutJunimos.Value = true;
		}
		base.Update(time);
	}

	private Color? getGemColor(ref bool isPrismatic)
	{
		List<Color> gemColors = new List<Color>();
		foreach (Item item in this.GetOutputChest().Items)
		{
			if (item != null && (item.Category == -12 || item.Category == -2))
			{
				Color? gemColor = TailoringMenu.GetDyeColor(item);
				if (item.QualifiedItemId == "(O)74")
				{
					isPrismatic = true;
				}
				if (gemColor.HasValue)
				{
					gemColors.Add(gemColor.Value);
				}
			}
		}
		if (gemColors.Count > 0)
		{
			return gemColors[Game1.random.Next(gemColors.Count)];
		}
		return null;
	}

	public bool areThereMatureCropsWithinRadius()
	{
		GameLocation location = base.GetParentLocation();
		for (int x = base.tileX.Value + 1 - this.cropHarvestRadius; x < base.tileX.Value + 2 + this.cropHarvestRadius; x++)
		{
			for (int y = base.tileY.Value - this.cropHarvestRadius + 1; y < base.tileY.Value + 2 + this.cropHarvestRadius; y++)
			{
				if (location.terrainFeatures.TryGetValue(new Vector2(x, y), out var terrainFeature))
				{
					if (location.isCropAtTile(x, y) && ((HoeDirt)terrainFeature).readyForHarvest())
					{
						this.lastKnownCropLocation = new Point(x, y);
						return true;
					}
					if (terrainFeature is Bush bush && bush.readyForHarvest())
					{
						this.lastKnownCropLocation = new Point(x, y);
						return true;
					}
				}
			}
		}
		this.lastKnownCropLocation = Point.Zero;
		return false;
	}

	public override void performTenMinuteAction(int timeElapsed)
	{
		base.performTenMinuteAction(timeElapsed);
		GameLocation location = base.GetParentLocation();
		if (this.myJunimos.Count > 0)
		{
			for (int i = this.myJunimos.Count - 1; i >= 0; i--)
			{
				if (!location.characters.Contains(this.myJunimos[i]))
				{
					this.myJunimos.RemoveAt(i);
				}
				else
				{
					this.myJunimos[i].pokeToHarvest();
				}
			}
		}
		if (this.myJunimos.Count < 3 && Game1.timeOfDay < 1900)
		{
			this.junimoSendOutTimer = 1;
		}
		if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400)
		{
			if (!location.IsWinterHere() && Game1.random.NextDouble() < 0.2)
			{
				this.wasLit.Value = true;
			}
		}
		else if (Game1.timeOfDay == 2400 && !location.IsWinterHere())
		{
			this.wasLit.Value = false;
		}
	}

	public override bool doAction(Vector2 tileLocation, Farmer who)
	{
		if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
		{
			return false;
		}
		if (base.occupiesTile(tileLocation))
		{
			Chest output = this.GetOutputChest();
			if (output.Items.Count > 36)
			{
				output.clearNulls();
			}
			output.mutex.RequestLock(delegate
			{
				Game1.activeClickableMenu = new ItemGrabMenu(output.Items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, output.grabItemFromInventory, null, output.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, 1, this);
			});
			return true;
		}
		return base.doAction(tileLocation, who);
	}

	public override void drawInMenu(SpriteBatch b, int x, int y)
	{
		this.drawShadow(b, x, y);
		b.Draw(base.texture.Value, new Vector2(x, y), new Rectangle(0, 0, 48, 64), base.color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
	}

	public override void draw(SpriteBatch b)
	{
		if (base.isMoving)
		{
			return;
		}
		if (base.daysOfConstructionLeft.Value > 0)
		{
			this.drawInConstruction(b);
			return;
		}
		this.drawShadow(b);
		Rectangle sourceRect = this.getSourceRectForMenu() ?? this.getSourceRect();
		b.Draw(base.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileX.Value * 64, base.tileY.Value * 64 + base.tilesHigh.Value * 64)), sourceRect, base.color * base.alpha, 0f, new Vector2(0f, base.texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)((base.tileY.Value + base.tilesHigh.Value - 1) * 64) / 10000f);
		if (this.raisinDays.Value > 0 && !Game1.IsWinter)
		{
			b.Draw(base.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileX.Value * 64 + 12, base.tileY.Value * 64 + base.tilesHigh.Value * 64 + 20)), new Rectangle(246, 46, 10, 18), base.color * base.alpha, 0f, new Vector2(0f, 18f), 4f, SpriteEffects.None, (float)((base.tileY.Value + base.tilesHigh.Value - 1) * 64 + 2) / 10000f);
		}
		bool containsOutput = false;
		Chest output = this.GetOutputChest();
		if (output != null)
		{
			foreach (Item item in output.Items)
			{
				if (item != null && item.Category != -12 && item.Category != -2)
				{
					containsOutput = true;
					break;
				}
			}
		}
		if (containsOutput)
		{
			b.Draw(base.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileX.Value * 64 + 128 + 12, base.tileY.Value * 64 + base.tilesHigh.Value * 64 - 32)), this.bagRect, base.color * base.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((base.tileY.Value + base.tilesHigh.Value - 1) * 64 + 1) / 10000f);
		}
		if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && this.wasLit.Value && !base.GetParentLocation().IsWinterHere())
		{
			b.Draw(base.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileX.Value * 64 + 64, base.tileY.Value * 64 + base.tilesHigh.Value * 64 - 64)), this.lightInteriorRect, base.color * base.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((base.tileY.Value + base.tilesHigh.Value - 1) * 64 + 1) / 10000f);
		}
	}
}
