using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace StardewValley.Objects;

public class CrabPot : Object
{
	public const int lidFlapTimerInterval = 60;

	[XmlIgnore]
	public float yBob;

	[XmlElement("directionOffset")]
	public readonly NetVector2 directionOffset = new NetVector2();

	[XmlElement("bait")]
	public readonly NetRef<Object> bait = new NetRef<Object>();

	public int tileIndexToShow;

	[XmlIgnore]
	public bool lidFlapping;

	[XmlIgnore]
	public bool lidClosing;

	[XmlIgnore]
	public float lidFlapTimer;

	[XmlIgnore]
	public new float shakeTimer;

	[XmlIgnore]
	public Vector2 shake;

	[XmlIgnore]
	private int ignoreRemovalTimer;

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.directionOffset, "directionOffset").AddField(this.bait, "bait");
	}

	public CrabPot()
		: base("710", 1)
	{
		base.CanBeGrabbed = false;
		base.type.Value = "interactive";
		this.tileIndexToShow = base.ParentSheetIndex;
	}

	/// <summary>Get whether the crab pot currently needs to be filled with bait before it can catch something.</summary>
	/// <param name="player">The player checking the crab pot.</param>
	public bool NeedsBait(Farmer player)
	{
		if (this.bait.Value != null)
		{
			return false;
		}
		return !(Game1.GetPlayer(base.owner.Value) ?? player ?? Game1.player).professions.Contains(11);
	}

	public List<Vector2> getOverlayTiles()
	{
		List<Vector2> tiles = new List<Vector2>();
		if (this.Location != null)
		{
			if (this.directionOffset.Y < 0f)
			{
				this.addOverlayTilesIfNecessary((int)this.TileLocation.X, (int)base.tileLocation.Y, tiles);
			}
			this.addOverlayTilesIfNecessary((int)this.TileLocation.X, (int)base.tileLocation.Y + 1, tiles);
			if (this.directionOffset.X < 0f)
			{
				this.addOverlayTilesIfNecessary((int)this.TileLocation.X - 1, (int)base.tileLocation.Y + 1, tiles);
			}
			if (this.directionOffset.X > 0f)
			{
				this.addOverlayTilesIfNecessary((int)this.TileLocation.X + 1, (int)base.tileLocation.Y + 1, tiles);
			}
		}
		return tiles;
	}

	protected void addOverlayTilesIfNecessary(int tile_x, int tile_y, List<Vector2> tiles)
	{
		GameLocation location = this.Location;
		if (location != null && location == Game1.currentLocation && location.hasTileAt(tile_x, tile_y, "Buildings") && !location.isWaterTile(tile_x, tile_y + 1))
		{
			tiles.Add(new Vector2(tile_x, tile_y));
		}
	}

	/// <summary>Add any tiles that might overlap with this crab pot incorrectly to the <see cref="F:StardewValley.Game1.crabPotOverlayTiles" /> dictionary.</summary>
	public void addOverlayTiles()
	{
		GameLocation location = this.Location;
		if (location == null || location != Game1.currentLocation)
		{
			return;
		}
		foreach (Vector2 tile in this.getOverlayTiles())
		{
			if (!Game1.crabPotOverlayTiles.TryGetValue(tile, out var count))
			{
				count = (Game1.crabPotOverlayTiles[tile] = 0);
			}
			Game1.crabPotOverlayTiles[tile] = count + 1;
		}
	}

	/// <summary>Remove any tiles that might overlap with this crab pot incorrectly from the <see cref="F:StardewValley.Game1.crabPotOverlayTiles" /> dictionary.</summary>
	public void removeOverlayTiles()
	{
		if (this.Location == null || this.Location != Game1.currentLocation)
		{
			return;
		}
		foreach (Vector2 tile in this.getOverlayTiles())
		{
			if (Game1.crabPotOverlayTiles.TryGetValue(tile, out var count))
			{
				count--;
				if (count <= 0)
				{
					Game1.crabPotOverlayTiles.Remove(tile);
				}
				else
				{
					Game1.crabPotOverlayTiles[tile] = count;
				}
			}
		}
	}

	public static bool IsValidCrabPotLocationTile(GameLocation location, int x, int y)
	{
		if (location is Caldera || location is VolcanoDungeon || location is MineShaft)
		{
			return false;
		}
		Vector2 placement_tile = new Vector2(x, y);
		bool neighbor_check = (location.isWaterTile(x + 1, y) && location.isWaterTile(x - 1, y)) || (location.isWaterTile(x, y + 1) && location.isWaterTile(x, y - 1));
		if (location.objects.ContainsKey(placement_tile) || !neighbor_check || !location.isWaterTile((int)placement_tile.X, (int)placement_tile.Y) || location.doesTileHaveProperty((int)placement_tile.X, (int)placement_tile.Y, "Passable", "Buildings") != null)
		{
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		this.updateOffset();
		this.addOverlayTiles();
		base.actionOnPlayerEntry();
	}

	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		Vector2 placementTile = new Vector2(x / 64, y / 64);
		if (who != null)
		{
			base.owner.Value = who.UniqueMultiplayerID;
		}
		if (!CrabPot.IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
		{
			return false;
		}
		this.TileLocation = placementTile;
		location.objects.Add(base.tileLocation.Value, this);
		location.playSound("waterSlosh");
		DelayedAction.playSoundAfterDelay("slosh", 150);
		this.updateOffset();
		this.addOverlayTiles();
		return true;
	}

	public void updateOffset()
	{
		Vector2 offset = Vector2.Zero;
		if (this.checkLocation(base.tileLocation.X - 1f, base.tileLocation.Y))
		{
			offset += new Vector2(32f, 0f);
		}
		if (this.checkLocation(base.tileLocation.X + 1f, base.tileLocation.Y))
		{
			offset += new Vector2(-32f, 0f);
		}
		if (offset.X != 0f && this.checkLocation(base.tileLocation.X + (float)Math.Sign(offset.X), base.tileLocation.Y + 1f))
		{
			offset += new Vector2(0f, -42f);
		}
		if (this.checkLocation(base.tileLocation.X, base.tileLocation.Y - 1f))
		{
			offset += new Vector2(0f, 32f);
		}
		if (this.checkLocation(base.tileLocation.X, base.tileLocation.Y + 1f))
		{
			offset += new Vector2(0f, -42f);
		}
		this.directionOffset.Value = offset;
	}

	protected bool checkLocation(float tile_x, float tile_y)
	{
		GameLocation location = this.Location;
		if (!location.isWaterTile((int)tile_x, (int)tile_y) || location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Passable", "Buildings") != null)
		{
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Object(base.ItemId, 1);
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		if (dropInItem is Object { Category: -21 } dropIn && this.NeedsBait(who))
		{
			if (!probe)
			{
				if (who != null)
				{
					base.owner.Value = who.UniqueMultiplayerID;
				}
				this.bait.Value = dropIn.getOne() as Object;
				location.playSound("Ship");
				this.lidFlapping = true;
				this.lidFlapTimer = 60f;
			}
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override bool AttemptAutoLoad(IInventory inventory, Farmer who)
	{
		Object prevBait = this.bait.Value;
		if (base.AttemptAutoLoad(inventory, who) && prevBait != this.bait.Value)
		{
			inventory.ReduceId(this.bait.Value.QualifiedItemId, this.bait.Value.Stack);
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		if (this.tileIndexToShow == 714)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			Object item = base.heldObject.Value;
			if (item != null)
			{
				int numToCatch = item.Stack;
				if (Utility.CreateDaySaveRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 77, base.tileLocation.X * 777f + base.tileLocation.Y).NextDouble() < 0.25 && Game1.player.stats.Get("Book_Crabbing") != 0 && who.couldInventoryAcceptThisItem(item.QualifiedItemId, numToCatch * 2, item.Quality))
				{
					numToCatch *= 2;
				}
				item.Stack = numToCatch;
				base.heldObject.Value = null;
				if (who.IsLocalPlayer && !who.addItemToInventoryBool(item))
				{
					base.heldObject.Value = item;
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					return false;
				}
				if (DataLoader.Fish(Game1.content).TryGetValue(item.ItemId, out var rawDataStr))
				{
					string[] rawData = rawDataStr.Split('/');
					int minFishSize = ((rawData.Length <= 5) ? 1 : Convert.ToInt32(rawData[5]));
					int maxFishSize = ((rawData.Length > 5) ? Convert.ToInt32(rawData[6]) : 10);
					who.caughtFish(item.QualifiedItemId, Game1.random.Next(minFishSize, maxFishSize + 1), from_fish_pond: false, numToCatch);
				}
				who.gainExperience(1, 5);
			}
			base.readyForHarvest.Value = false;
			this.tileIndexToShow = 710;
			this.lidFlapping = true;
			this.lidFlapTimer = 60f;
			this.bait.Value = null;
			who.animateOnce(279 + who.FacingDirection);
			location.playSound("fishingRodBend");
			DelayedAction.playSoundAfterDelay("coin", 500);
			this.shake = Vector2.Zero;
			this.shakeTimer = 0f;
			this.ignoreRemovalTimer = 750;
			return true;
		}
		if (this.bait.Value == null && this.ignoreRemovalTimer <= 0)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
			{
				if (Game1.player.addItemToInventoryBool(base.getOne()))
				{
					if (who.isMoving())
					{
						Game1.haltAfterCheck = false;
					}
					Game1.playSound("coin");
					location.objects.Remove(base.tileLocation.Value);
					return true;
				}
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
		}
		return false;
	}

	public override void performRemoveAction()
	{
		this.removeOverlayTiles();
		base.performRemoveAction();
	}

	public override void DayUpdate()
	{
		GameLocation location = this.Location;
		Farmer ownedByPlayer = Game1.GetPlayer(base.owner.Value) ?? Game1.MasterPlayer;
		bool isMariner = ownedByPlayer.professions.Contains(10);
		if (this.NeedsBait(ownedByPlayer) || base.heldObject.Value != null)
		{
			return;
		}
		this.tileIndexToShow = 714;
		base.readyForHarvest.Value = true;
		Random r = Utility.CreateDaySaveRandom(base.tileLocation.X * 1000f, base.tileLocation.Y * 255f, this.directionOffset.X * 1000f + this.directionOffset.Y);
		List<string> marinerList = new List<string>();
		if (!location.TryGetFishAreaForTile(base.tileLocation.Value, out var _, out var fishArea))
		{
			fishArea = null;
		}
		double chanceForJunk = (isMariner ? 0.0 : (((double?)fishArea?.CrabPotJunkChance) ?? 0.2));
		int quantity = 1;
		int quality = 0;
		string baitTargetFish = null;
		switch (this.bait.Value?.QualifiedItemId)
		{
		case "(O)DeluxeBait":
			quality = 1;
			chanceForJunk /= 2.0;
			break;
		case "(O)774":
			chanceForJunk /= 2.0;
			if (r.NextBool(0.25))
			{
				quantity = 2;
			}
			break;
		case "(O)SpecificBait":
			if (this.bait.Value.preservedParentSheetIndex.Value != null && this.bait.Value.preserve.Value.HasValue)
			{
				baitTargetFish = this.bait.Value.preservedParentSheetIndex.Value;
				chanceForJunk /= 2.0;
			}
			break;
		}
		if (!r.NextBool(chanceForJunk))
		{
			IList<string> targetAreas = location.GetCrabPotFishForTile(base.tileLocation.Value);
			foreach (KeyValuePair<string, string> v in DataLoader.Fish(Game1.content))
			{
				if (!v.Value.Contains("trap"))
				{
					continue;
				}
				string[] rawSplit = v.Value.Split('/');
				string[] array = ArgUtility.SplitBySpace(rawSplit[4]);
				bool found = false;
				string[] array2 = array;
				foreach (string crabPotArea in array2)
				{
					foreach (string targetArea in targetAreas)
					{
						if (crabPotArea == targetArea)
						{
							found = true;
							break;
						}
					}
				}
				if (!found)
				{
					continue;
				}
				if (isMariner)
				{
					marinerList.Add(v.Key);
					continue;
				}
				double chanceForCatch = Convert.ToDouble(rawSplit[2]);
				if (baitTargetFish != null && baitTargetFish == v.Key)
				{
					chanceForCatch *= (double)((chanceForCatch < 0.1) ? 4 : ((chanceForCatch < 0.2) ? 3 : 2));
				}
				if (!(r.NextDouble() < chanceForCatch))
				{
					continue;
				}
				base.heldObject.Value = ItemRegistry.Create<Object>("(O)" + v.Key, quantity, quality);
				break;
			}
		}
		if (base.heldObject.Value == null)
		{
			if (isMariner && marinerList.Count > 0)
			{
				base.heldObject.Value = ItemRegistry.Create<Object>("(O)" + r.ChooseFrom(marinerList), quantity, quality);
			}
			else
			{
				base.heldObject.Value = ItemRegistry.Create<Object>("(O)" + r.Next(168, 173));
			}
		}
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		if (this.lidFlapping)
		{
			this.lidFlapTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.lidFlapTimer <= 0f)
			{
				this.tileIndexToShow += ((!this.lidClosing) ? 1 : (-1));
				if (this.tileIndexToShow >= 713 && !this.lidClosing)
				{
					this.lidClosing = true;
					this.tileIndexToShow--;
				}
				else if (this.tileIndexToShow <= 709 && this.lidClosing)
				{
					this.lidClosing = false;
					this.tileIndexToShow++;
					this.lidFlapping = false;
					if (this.bait.Value != null)
					{
						this.tileIndexToShow = 713;
					}
				}
				this.lidFlapTimer = 60f;
			}
		}
		if (base.readyForHarvest.Value && base.heldObject.Value != null)
		{
			this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.shakeTimer < 0f)
			{
				this.shakeTimer = Game1.random.Next(2800, 3200);
			}
		}
		if (this.shakeTimer > 2000f)
		{
			this.shake.X = Game1.random.Next(-1, 2);
		}
		else
		{
			this.shake.X = 0f;
		}
		if (this.ignoreRemovalTimer > 0)
		{
			this.ignoreRemovalTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return;
		}
		if (base.heldObject.Value != null)
		{
			this.tileIndexToShow = 714;
		}
		else if (this.tileIndexToShow == 0)
		{
			this.tileIndexToShow = base.ParentSheetIndex;
		}
		this.yBob = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + (double)(x * 64)) * 8.0 + 8.0);
		if (this.yBob <= 0.001f)
		{
			location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, this.directionOffset.Value + new Vector2(x * 64 + 4, y * 64 + 32), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f));
		}
		spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2(x * 64, y * 64 + (int)this.yBob)) + this.shake, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.tileIndexToShow, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, ((float)(y * 64) + this.directionOffset.Y + (float)(x % 4)) / 10000f);
		if (location.waterTiles != null && x < location.waterTiles.waterTiles.GetLength(0) && y < location.waterTiles.waterTiles.GetLength(1) && location.waterTiles.waterTiles[x, y].isWater)
		{
			if (location.waterTiles.waterTiles[x, y].isVisible)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2(x * 64 + 4, y * 64 + 48)) + this.shake, new Rectangle(location.waterAnimationIndex * 64, 2112 + (((x + y) % 2 != 0) ? ((!location.waterTileFlip) ? 128 : 0) : (location.waterTileFlip ? 128 : 0)), 56, 16 + (int)this.yBob), location.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, ((float)(y * 64) + this.directionOffset.Y + (float)(x % 4)) / 9999f);
			}
			else
			{
				Color water_color = new Color(135, 135, 135, 215);
				water_color = Utility.MultiplyColor(water_color, location.waterColor.Value);
				spriteBatch.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2(x * 64 + 4, y * 64 + 48)) + this.shake, null, water_color, 0f, Vector2.Zero, new Vector2(56f, 16 + (int)this.yBob), SpriteEffects.None, ((float)(y * 64) + this.directionOffset.Y + (float)(x % 4)) / 9999f);
			}
		}
		if (base.readyForHarvest.Value && base.heldObject.Value != null)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-06f + base.tileLocation.X / 10000f);
			ParsedItemData heldItemData = ItemRegistry.GetDataOrErrorItem(base.heldObject.Value.QualifiedItemId);
			spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, this.directionOffset.Value + new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), heldItemData.GetSourceRect(), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + base.tileLocation.X / 10000f);
		}
	}
}
