using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class FishTankFurniture : StorageFurniture
{
	public enum FishTankCategories
	{
		None,
		Swim,
		Ground,
		Decoration
	}

	public const int TANK_DEPTH = 10;

	public const int FLOOR_DECORATION_OFFSET = 4;

	public const int TANK_SORT_REGION = 20;

	[XmlIgnore]
	public List<Vector4> bubbles = new List<Vector4>();

	[XmlIgnore]
	public List<TankFish> tankFish = new List<TankFish>();

	[XmlIgnore]
	public NetEvent0 refreshFishEvent = new NetEvent0();

	[XmlIgnore]
	public bool fishDirty = true;

	[XmlIgnore]
	private Texture2D _aquariumTexture;

	[XmlIgnore]
	public List<KeyValuePair<Rectangle, Vector2>?> floorDecorations = new List<KeyValuePair<Rectangle, Vector2>?>();

	[XmlIgnore]
	public List<Vector2> decorationSlots = new List<Vector2>();

	[XmlIgnore]
	public List<int> floorDecorationIndices = new List<int>();

	public NetInt generationSeed = new NetInt();

	[XmlIgnore]
	public Item localDepositedItem;

	[XmlIgnore]
	protected int _currentDecorationIndex;

	protected Dictionary<Item, TankFish> _fishLookup = new Dictionary<Item, TankFish>();

	public FishTankFurniture()
	{
		this.generationSeed.Value = Game1.random.Next();
	}

	public FishTankFurniture(string itemId, Vector2 tile, int initialRotations)
		: base(itemId, tile, initialRotations)
	{
		this.generationSeed.Value = Game1.random.Next();
	}

	public FishTankFurniture(string itemId, Vector2 tile)
		: base(itemId, tile)
	{
		this.generationSeed.Value = Game1.random.Next();
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntryOrPlacement(GameLocation environment, bool dropDown)
	{
		base.actionOnPlayerEntryOrPlacement(environment, dropDown);
		this.ResetFish();
		this.UpdateFish();
	}

	public virtual void ResetFish()
	{
		this.bubbles.Clear();
		this.tankFish.Clear();
		this._fishLookup.Clear();
		this.UpdateFish();
	}

	public Texture2D GetAquariumTexture()
	{
		if (this._aquariumTexture == null)
		{
			this._aquariumTexture = Game1.content.Load<Texture2D>("LooseSprites\\AquariumFish");
		}
		return this._aquariumTexture;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.generationSeed, "generationSeed").AddField(this.refreshFishEvent, "refreshFishEvent");
		this.refreshFishEvent.onEvent += UpdateDecorAndFish;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new FishTankFurniture(base.ItemId, base.tileLocation.Value);
	}

	public virtual int GetCapacityForCategory(FishTankCategories category)
	{
		int extra = 0;
		if (base.QualifiedItemId.Equals("(F)JungleTank"))
		{
			extra++;
		}
		switch (category)
		{
		case FishTankCategories.Swim:
			return this.getTilesWide() - 1;
		case FishTankCategories.Ground:
			return this.getTilesWide() - 1 + extra;
		case FishTankCategories.Decoration:
			if (this.getTilesWide() <= 2)
			{
				return 1;
			}
			return -1;
		default:
			return 0;
		}
	}

	public FishTankCategories GetCategoryFromItem(Item item)
	{
		Dictionary<string, string> aquarium_data = this.GetAquariumData();
		if (!this.CanBeDeposited(item))
		{
			return FishTankCategories.None;
		}
		if (item.QualifiedItemId == "(TR)FrogEgg")
		{
			return FishTankCategories.Ground;
		}
		if (aquarium_data.TryGetValue(item.ItemId, out var rawData))
		{
			switch (ArgUtility.Get(rawData.Split('/'), 1))
			{
			case "crawl":
			case "ground":
			case "front_crawl":
			case "static":
				return FishTankCategories.Ground;
			default:
				return FishTankCategories.Swim;
			}
		}
		return FishTankCategories.Decoration;
	}

	public bool HasRoomForThisItem(Item item)
	{
		if (!this.CanBeDeposited(item))
		{
			return false;
		}
		FishTankCategories category = this.GetCategoryFromItem(item);
		int capacity = this.GetCapacityForCategory(category);
		if (item is Hat)
		{
			capacity = 999;
		}
		if (capacity < 0)
		{
			foreach (Item held_item in base.heldItems)
			{
				if (held_item != null && held_item.QualifiedItemId == item.QualifiedItemId)
				{
					return false;
				}
			}
			return true;
		}
		int current_count = 0;
		foreach (Item held_item2 in base.heldItems)
		{
			if (held_item2 != null)
			{
				if (this.GetCategoryFromItem(held_item2) == category)
				{
					current_count++;
				}
				if (current_count >= capacity)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string GetShopMenuContext()
	{
		return "FishTank";
	}

	public override void ShowMenu()
	{
		this.ShowShopMenu();
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		if (justCheckingForActivity)
		{
			return true;
		}
		if (base.mutex.IsLocked())
		{
			return true;
		}
		if ((who.ActiveObject != null || who.CurrentItem is Hat || who.CurrentItem?.QualifiedItemId == "(TR)FrogEgg") && this.localDepositedItem == null && this.CanBeDeposited(who.CurrentItem))
		{
			if (!this.HasRoomForThisItem(who.CurrentItem))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishTank_Full"));
				return true;
			}
			this.localDepositedItem = who.CurrentItem.getOne();
			if (who.CurrentItem.ConsumeStack(1) == null)
			{
				who.removeItemFromInventory(who.CurrentItem);
				who.showNotCarrying();
			}
			base.mutex.RequestLock(delegate
			{
				location.playSound("dropItemInWater");
				base.heldItems.Add(this.localDepositedItem);
				this.localDepositedItem = null;
				this.refreshFishEvent.Fire();
				base.mutex.ReleaseLock();
			}, delegate
			{
				this.localDepositedItem = who.addItemToInventory(this.localDepositedItem);
				if (this.localDepositedItem != null)
				{
					Game1.createItemDebris(this.localDepositedItem, new Vector2(this.TileLocation.X + (float)this.getTilesWide() / 2f + 0.5f, this.TileLocation.Y + 0.5f) * 64f, -1, location);
				}
				this.localDepositedItem = null;
			});
			return true;
		}
		base.mutex.RequestLock(ShowMenu);
		return true;
	}

	public virtual bool CanBeDeposited(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.QualifiedItemId == "(TR)FrogEgg")
		{
			return true;
		}
		if (!(item is Hat) && !Utility.IsNormalObjectAtParentSheetIndex(item, item.ItemId))
		{
			return false;
		}
		if (item.QualifiedItemId == "(O)152" || item.QualifiedItemId == "(O)393" || item.QualifiedItemId == "(O)390" || item.QualifiedItemId == "(O)117" || item.QualifiedItemId == "(O)166" || item.QualifiedItemId == "(O)832" || item.QualifiedItemId == "(O)109" || item.QualifiedItemId == "(O)709" || item.QualifiedItemId == "(O)392" || item.QualifiedItemId == "(O)394" || item.QualifiedItemId == "(O)167" || item.QualifiedItemId == "(O)789" || item.QualifiedItemId == "(O)330" || item.QualifiedItemId == "(O)797")
		{
			return true;
		}
		if (item is Hat)
		{
			int numHatWearers = 0;
			int numHats = 0;
			foreach (TankFish item2 in this.tankFish)
			{
				if (item2.CanWearHat())
				{
					numHatWearers++;
				}
			}
			foreach (Item heldItem in base.heldItems)
			{
				if (heldItem is Hat)
				{
					numHats++;
				}
			}
			return numHats < numHatWearers;
		}
		return this.GetAquariumData().ContainsKey(item.ItemId);
	}

	public override void DayUpdate()
	{
		this.ResetFish();
		base.DayUpdate();
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = this.Location;
		if (Game1.currentLocation == environment)
		{
			if (this.fishDirty)
			{
				this.fishDirty = false;
				this.UpdateDecorAndFish();
			}
			foreach (TankFish item in this.tankFish)
			{
				item.Update(time);
			}
			for (int i = 0; i < this.bubbles.Count; i++)
			{
				Vector4 bubble = this.bubbles[i];
				bubble.W += 0.05f;
				if (bubble.W > 1f)
				{
					bubble.W = 1f;
				}
				bubble.Y += bubble.W;
				this.bubbles[i] = bubble;
				if (bubble.Y >= (float)this.GetTankBounds().Height)
				{
					this.bubbles.RemoveAt(i);
					i--;
				}
			}
		}
		base.updateWhenCurrentLocation(time);
		this.refreshFishEvent.Poll();
	}

	/// <inheritdoc />
	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		this.generationSeed.Value = Game1.random.Next();
		this.fishDirty = true;
		return base.placementAction(location, x, y, who);
	}

	public Dictionary<string, string> GetAquariumData()
	{
		return DataLoader.AquariumFish(Game1.content);
	}

	/// <inheritdoc />
	public override bool onDresserItemWithdrawn(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
	{
		bool result = base.onDresserItemWithdrawn(salable, who, countTaken, stock);
		this.refreshFishEvent.Fire();
		return result;
	}

	public virtual void UpdateFish()
	{
		List<Item> fish_items = new List<Item>();
		Dictionary<string, string> aquarium_data = this.GetAquariumData();
		foreach (Item item in base.heldItems)
		{
			if (item != null)
			{
				if (item is Object o)
				{
					o.reloadSprite();
				}
				bool forceValid = item.QualifiedItemId == "(TR)FrogEgg";
				if ((forceValid || Utility.IsNormalObjectAtParentSheetIndex(item, item.ItemId)) && (forceValid || aquarium_data.ContainsKey(item.ItemId)))
				{
					fish_items.Add(item);
				}
			}
		}
		List<Item> items_to_remove = new List<Item>();
		foreach (Item key in this._fishLookup.Keys)
		{
			if (!base.heldItems.Contains(key))
			{
				items_to_remove.Add(key);
			}
		}
		for (int i = 0; i < fish_items.Count; i++)
		{
			Item item2 = fish_items[i];
			if (!this._fishLookup.ContainsKey(item2))
			{
				TankFish fish = new TankFish(this, item2);
				this.tankFish.Add(fish);
				this._fishLookup[item2] = fish;
			}
		}
		foreach (Item removed_item in items_to_remove)
		{
			this.tankFish.Remove(this._fishLookup[removed_item]);
			base.heldItems.Remove(removed_item);
		}
	}

	public virtual void UpdateDecorAndFish()
	{
		Random r = Utility.CreateRandom(this.generationSeed.Value);
		this.UpdateFish();
		this.decorationSlots.Clear();
		for (int y = 0; y < 3; y++)
		{
			for (int x = 0; x < this.getTilesWide(); x++)
			{
				Vector2 slot_position = default(Vector2);
				if (y % 2 == 0)
				{
					if (x == this.getTilesWide() - 1)
					{
						continue;
					}
					slot_position.X = 16 + x * 16;
				}
				else
				{
					slot_position.X = 8 + x * 16;
				}
				slot_position.Y = 4f;
				slot_position.Y += 3.3333333f * (float)y;
				this.decorationSlots.Add(slot_position);
			}
		}
		this.floorDecorationIndices.Clear();
		this.floorDecorations.Clear();
		this._currentDecorationIndex = 0;
		for (int i = 0; i < this.decorationSlots.Count; i++)
		{
			this.floorDecorationIndices.Add(i);
			this.floorDecorations.Add(null);
		}
		Utility.Shuffle(r, this.floorDecorationIndices);
		Random decoration_random = Utility.CreateRandom(r.Next());
		bool add_decoration = this.GetItemCount("393") > 0;
		for (int j = 0; j < 1; j++)
		{
			if (add_decoration)
			{
				this.AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 5), 256, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
		}
		decoration_random = Utility.CreateRandom(r.Next());
		bool add_decoration2 = this.GetItemCount("152") > 0;
		for (int k = 0; k < 4; k++)
		{
			if (add_decoration2)
			{
				this.AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 3), 288, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
		}
		decoration_random = Utility.CreateRandom(r.Next());
		bool add_decoration3 = this.GetItemCount("390") > 0;
		for (int l = 0; l < 2; l++)
		{
			if (add_decoration3)
			{
				this.AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 3), 272, 16, 16));
			}
			else
			{
				this._AdvanceDecorationIndex();
			}
		}
		if (this.GetItemCount("117") > 0)
		{
			this.AddFloorDecoration(new Rectangle(48, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("166") > 0)
		{
			this.AddFloorDecoration(new Rectangle(64, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("797") > 0)
		{
			this.AddFloorDecoration(new Rectangle(80, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("832") > 0)
		{
			this.AddFloorDecoration(new Rectangle(96, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("109") > 0)
		{
			this.AddFloorDecoration(new Rectangle(112, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("709") > 0)
		{
			this.AddFloorDecoration(new Rectangle(128, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("392") > 0)
		{
			this.AddFloorDecoration(new Rectangle(144, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("394") > 0)
		{
			this.AddFloorDecoration(new Rectangle(160, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("167") > 0)
		{
			this.AddFloorDecoration(new Rectangle(176, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("789") > 0)
		{
			this.AddFloorDecoration(new Rectangle(192, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
		if (this.GetItemCount("330") > 0)
		{
			this.AddFloorDecoration(new Rectangle(208, 288, 16, 16));
		}
		else
		{
			this._AdvanceDecorationIndex();
		}
	}

	public virtual void AddFloorDecoration(Rectangle source_rect)
	{
		if (this._currentDecorationIndex != -1)
		{
			int index = this.floorDecorationIndices[this._currentDecorationIndex];
			this._AdvanceDecorationIndex();
			int center_x = (int)this.decorationSlots[index].X;
			int center_y = (int)this.decorationSlots[index].Y;
			if (center_x < source_rect.Width / 2)
			{
				center_x = source_rect.Width / 2;
			}
			if (center_x > this.GetTankBounds().Width / 4 - source_rect.Width / 2)
			{
				center_x = this.GetTankBounds().Width / 4 - source_rect.Width / 2;
			}
			KeyValuePair<Rectangle, Vector2> decoration = new KeyValuePair<Rectangle, Vector2>(source_rect, new Vector2(center_x, center_y));
			this.floorDecorations[index] = decoration;
		}
	}

	protected virtual void _AdvanceDecorationIndex()
	{
		for (int i = 0; i < this.decorationSlots.Count; i++)
		{
			this._currentDecorationIndex++;
			if (this._currentDecorationIndex >= this.decorationSlots.Count)
			{
				this._currentDecorationIndex = 0;
			}
			if (!this.floorDecorations[this.floorDecorationIndices[this._currentDecorationIndex]].HasValue)
			{
				return;
			}
		}
		this._currentDecorationIndex = 1;
	}

	public override void OnMenuClose()
	{
		this.refreshFishEvent.Fire();
		base.OnMenuClose();
	}

	public Vector2 GetFishSortRegion()
	{
		return new Vector2(this.GetBaseDrawLayer() + 1E-06f, this.GetGlassDrawLayer() - 1E-06f);
	}

	public float GetGlassDrawLayer()
	{
		return this.GetBaseDrawLayer() + 0.0001f;
	}

	public float GetBaseDrawLayer()
	{
		if (base.furniture_type.Value != 12)
		{
			return (float)(base.boundingBox.Value.Bottom - ((base.furniture_type.Value == 6 || base.furniture_type.Value == 13) ? 48 : 8)) / 10000f;
		}
		return 2E-09f;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		Vector2 shake = Vector2.Zero;
		if (base.isTemporarilyInvisible)
		{
			return;
		}
		Vector2 draw_position = base.drawPosition.Value;
		if (!Furniture.isDrawingLocationFurniture)
		{
			draw_position = new Vector2(x, y) * 64f;
			draw_position.Y -= base.sourceRect.Height * 4 - base.boundingBox.Height;
		}
		if (base.shakeTimer > 0)
		{
			shake = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
		}
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		Rectangle mainSourceRect = itemData.GetSourceRect();
		spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, draw_position + shake), new Rectangle(mainSourceRect.X + mainSourceRect.Width, mainSourceRect.Y, mainSourceRect.Width, mainSourceRect.Height), Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.GetGlassDrawLayer());
		if (Furniture.isDrawingLocationFurniture)
		{
			for (int i = 0; i < this.tankFish.Count; i++)
			{
				TankFish fish = this.tankFish[i];
				float fish_layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, fish.zPosition / 20f);
				fish_layer += 1E-07f * (float)i;
				fish.Draw(spriteBatch, alpha, fish_layer);
			}
			for (int j = 0; j < this.floorDecorations.Count; j++)
			{
				if (this.floorDecorations[j].HasValue)
				{
					KeyValuePair<Rectangle, Vector2> decoration = this.floorDecorations[j].Value;
					Vector2 decoration_position = decoration.Value;
					Rectangle decoration_source_rect = decoration.Key;
					float decoration_layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, decoration_position.Y / 20f) - 1E-06f;
					spriteBatch.Draw(this.GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)this.GetTankBounds().Left + decoration_position.X * 4f, (float)(this.GetTankBounds().Bottom - 4) - decoration_position.Y * 4f)), decoration_source_rect, Color.White * alpha, 0f, new Vector2(decoration_source_rect.Width / 2, decoration_source_rect.Height - 4), 4f, SpriteEffects.None, decoration_layer);
				}
			}
			foreach (Vector4 bubble in this.bubbles)
			{
				float layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, bubble.Z / 20f) - 1E-06f;
				spriteBatch.Draw(this.GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)this.GetTankBounds().Left + bubble.X, (float)(this.GetTankBounds().Bottom - 4) - bubble.Y - bubble.Z * 4f)), new Rectangle(0, 240, 16, 16), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * bubble.W, SpriteEffects.None, layer);
			}
		}
		base.draw(spriteBatch, x, y, alpha);
	}

	public int GetItemCount(string itemId)
	{
		int count = 0;
		foreach (Item item in base.heldItems)
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(item, itemId))
			{
				count += item.Stack;
			}
		}
		return count;
	}

	public virtual Rectangle GetTankBounds()
	{
		Rectangle rectangle = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).GetSourceRect();
		int height = rectangle.Height / 16;
		int width = rectangle.Width / 16;
		Rectangle tank_rect = new Rectangle((int)this.TileLocation.X * 64, (int)((this.TileLocation.Y - (float)this.getTilesHigh() - 1f) * 64f), width * 64, height * 64);
		tank_rect.X += 4;
		tank_rect.Width -= 8;
		if (base.QualifiedItemId == "(F)CCFishTank")
		{
			tank_rect.X += 24;
			tank_rect.Width -= 76;
		}
		tank_rect.Height -= 28;
		tank_rect.Y += 64;
		tank_rect.Height -= 64;
		return tank_rect;
	}
}
