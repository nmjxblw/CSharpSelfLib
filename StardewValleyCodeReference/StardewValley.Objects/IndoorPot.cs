using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Objects;

public class IndoorPot : Object
{
	[XmlElement("hoeDirt")]
	public readonly NetRef<HoeDirt> hoeDirt = new NetRef<HoeDirt>();

	[XmlElement("bush")]
	public readonly NetRef<Bush> bush = new NetRef<Bush>();

	[XmlIgnore]
	public readonly NetBool bushLoadDirty = new NetBool(value: true);

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	/// <inheritdoc />
	[XmlIgnore]
	public override GameLocation Location
	{
		get
		{
			return base.Location;
		}
		set
		{
			if (this.hoeDirt.Value != null)
			{
				this.hoeDirt.Value.Location = value;
				this.hoeDirt.Value.Pot = this;
			}
			if (this.bush.Value != null)
			{
				this.bush.Value.Location = value;
			}
			base.Location = value;
		}
	}

	/// <inheritdoc />
	public override Vector2 TileLocation
	{
		get
		{
			return base.TileLocation;
		}
		set
		{
			if (this.hoeDirt.Value != null)
			{
				this.hoeDirt.Value.Tile = value;
			}
			if (this.bush.Value != null)
			{
				this.bush.Value.Tile = value;
			}
			base.TileLocation = value;
		}
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.hoeDirt, "hoeDirt").AddField(this.bush, "bush").AddField(this.bushLoadDirty, "bushLoadDirty");
		this.bush.fieldChangeEvent += delegate(NetRef<Bush> field, Bush value, Bush newValue)
		{
			if (newValue != null)
			{
				newValue.Location = this.Location;
				newValue.inPot.Value = true;
			}
		};
	}

	public IndoorPot()
	{
	}

	public IndoorPot(Vector2 tileLocation)
		: base(tileLocation, "62")
	{
		GameLocation location = (this.Location = Game1.currentLocation);
		this.hoeDirt.Value = new HoeDirt(0, location);
		if (location.IsRainingHere() && location.isOutdoors.Value)
		{
			this.Water();
		}
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		this.hoeDirt.Value.dayUpdate();
		base.showNextIndex.Value = this.hoeDirt.Value.isWatered();
		GameLocation location = this.Location;
		if (location.isOutdoors.Value && location.IsRainingHere())
		{
			this.Water();
		}
		if (base.heldObject.Value != null)
		{
			base.readyForHarvest.Value = true;
		}
		this.bush.Value?.dayUpdate();
	}

	/// <summary>Water the dirt in this garden pot.</summary>
	public void Water()
	{
		this.hoeDirt.Value.state.Value = 1;
		base.showNextIndex.Value = true;
	}

	/// <summary>Get whether an item type can be planted in indoor pots, regardless of whether the pot has room currently.</summary>
	/// <param name="item">The item to check.</param>
	public bool IsPlantableItem(Item item)
	{
		if (item.HasTypeObject())
		{
			string qualifiedItemId = item.QualifiedItemId;
			if (qualifiedItemId == "(O)499" || qualifiedItemId == "(O)805")
			{
				return false;
			}
			if (item.Category == -19)
			{
				return true;
			}
			string cropItemId = Crop.ResolveSeedId(item.ItemId, this.Location);
			if (Game1.cropData.ContainsKey(cropItemId))
			{
				return true;
			}
			if (item is Object obj && obj.IsTeaSapling())
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		if (who != null && dropInItem != null && this.bush.Value == null)
		{
			if (this.hoeDirt.Value.canPlantThisSeedHere(dropInItem.ItemId, dropInItem.Category == -19))
			{
				if (dropInItem.QualifiedItemId == "(O)805")
				{
					if (!probe)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					}
					return false;
				}
				if (!probe)
				{
					return this.hoeDirt.Value.plant(dropInItem.ItemId, who, dropInItem.Category == -19);
				}
				return true;
			}
			if (this.hoeDirt.Value.crop == null && dropInItem.QualifiedItemId == "(O)251")
			{
				if (!probe)
				{
					NetRef<Bush> netRef = this.bush;
					Bush obj = new Bush(base.tileLocation.Value, 3, this.Location);
					obj.inPot.Value = true;
					netRef.Value = obj;
					if (!this.Location.IsOutdoors)
					{
						this.bush.Value.loadSprite();
						Game1.playSound("coin");
					}
				}
				return true;
			}
		}
		return false;
	}

	public override bool performToolAction(Tool t)
	{
		if (t != null)
		{
			this.hoeDirt.Value.performToolAction(t, -1, base.tileLocation.Value);
			if (this.bush.Value != null)
			{
				if (this.bush.Value.performToolAction(t, -1, base.tileLocation.Value))
				{
					this.bush.Value = null;
				}
				return false;
			}
		}
		if (this.hoeDirt.Value.isWatered())
		{
			this.Water();
		}
		return base.performToolAction(t);
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (who != null)
		{
			if (justCheckingForActivity)
			{
				if (!this.hoeDirt.Value.readyForHarvest() && base.heldObject.Value == null)
				{
					if (this.bush.Value != null)
					{
						return this.bush.Value.inBloom();
					}
					return false;
				}
				return true;
			}
			if (who.isMoving())
			{
				Game1.haltAfterCheck = false;
			}
			if (base.heldObject.Value != null)
			{
				Object obj = base.heldObject.Value;
				int originalQuality = obj.Quality;
				obj.Quality = this.Location.GetHarvestSpawnedObjectQuality(who, obj.isForage(), this.TileLocation);
				bool num = who.addItemToInventoryBool(obj);
				if (num)
				{
					base.heldObject.Value = null;
					base.readyForHarvest.Value = false;
					Game1.playSound("coin");
					this.Location.OnHarvestedForage(who, obj);
					return num;
				}
				base.heldObject.Value.Quality = originalQuality;
				return num;
			}
			bool b = this.hoeDirt.Value.performUseAction(base.tileLocation.Value);
			if (b)
			{
				return b;
			}
			Crop crop = this.hoeDirt.Value.crop;
			if (crop != null && crop.currentPhase.Value > 0 && this.hoeDirt.Value.getMaxShake() == 0f)
			{
				this.hoeDirt.Value.shake((float)Math.PI / 32f, (float)Math.PI / 50f, Game1.random.NextBool());
				DelayedAction.playSoundAfterDelay("leafrustle", Game1.random.Next(100));
			}
			this.bush.Value?.performUseAction(base.tileLocation.Value);
		}
		return false;
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		base.actionOnPlayerEntry();
		this.hoeDirt.Value?.performPlayerEntryAction();
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		base.updateWhenCurrentLocation(time);
		if (this.Location != null)
		{
			this.hoeDirt.Value.tickUpdate(time);
			this.bush.Value?.tickUpdate(time);
			if (this.bushLoadDirty.Value)
			{
				this.bush.Value?.loadSprite();
				this.bushLoadDirty.Value = false;
			}
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		Vector2 scaleFactor = this.getScale();
		scaleFactor *= 4f;
		Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
		Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(itemData.GetTexture(), destination, itemData.GetSourceRect(base.showNextIndex.Value ? 1 : 0), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f);
		if (this.hoeDirt.Value.HasFertilizer())
		{
			Rectangle fertilizer_rect = this.hoeDirt.Value.GetFertilizerSourceRect();
			fertilizer_rect.Width = 13;
			fertilizer_rect.Height = 13;
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileLocation.X * 64f + 4f, base.tileLocation.Y * 64f - 12f)), fertilizer_rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.tileLocation.Y + 0.65f) * 64f / 10000f + (float)x * 1E-05f);
		}
		this.hoeDirt.Value.crop?.drawWithOffset(spriteBatch, base.tileLocation.Value, (this.hoeDirt.Value.isWatered() && this.hoeDirt.Value.crop.currentPhase.Value == 0 && !this.hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, this.hoeDirt.Value.getShakeRotation(), new Vector2(32f, 8f));
		base.heldObject.Value?.draw(spriteBatch, x * 64, y * 64 - 48, (base.tileLocation.Y + 0.66f) * 64f / 10000f + (float)x * 1E-05f, 1f);
		this.bush.Value?.draw(spriteBatch, -24f);
	}
}
