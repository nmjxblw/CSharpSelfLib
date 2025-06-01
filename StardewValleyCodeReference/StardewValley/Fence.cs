using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Fences;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;

namespace StardewValley;

public class Fence : Object
{
	public const int debrisPieces = 4;

	public static int fencePieceWidth = 16;

	public static int fencePieceHeight = 32;

	public const int gateClosedPosition = 0;

	public const int gateOpenedPosition = 88;

	public const int sourceRectForSoloGate = 17;

	public const int globalHealthMultiplier = 2;

	public const int N = 1000;

	public const int E = 100;

	public const int S = 500;

	public const int W = 10;

	/// <summary>The unqualified item ID for a wood fence.</summary>
	public const string woodFenceId = "322";

	/// <summary>The unqualified item ID for a stone fence.</summary>
	public const string stoneFenceId = "323";

	/// <summary>The unqualified item ID for an iron fence.</summary>
	public const string ironFenceId = "324";

	/// <summary>The unqualified item ID for a hardwood fence.</summary>
	public const string hardwoodFenceId = "298";

	/// <summary>The unqualified item ID for a fence gate.</summary>
	public const string gateId = "325";

	[XmlIgnore]
	public Lazy<Texture2D> fenceTexture;

	public static Dictionary<int, int> fenceDrawGuide;

	[XmlElement("health")]
	public new readonly NetFloat health = new NetFloat();

	[XmlElement("maxHealth")]
	public readonly NetFloat maxHealth = new NetFloat();

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Item.ItemId" /> instead.</summary>
	[XmlElement("whichType")]
	public int? obsolete_whichType;

	[XmlElement("gatePosition")]
	public readonly NetInt gatePosition = new NetInt();

	public int gateMotion;

	[XmlElement("isGate")]
	public readonly NetBool isGate = new NetBool();

	[XmlIgnore]
	public readonly NetBool repairQueued = new NetBool();

	protected static Dictionary<string, FenceData> _FenceLookup;

	protected FenceData _data;

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.health, "health").AddField(this.maxHealth, "maxHealth").AddField(this.gatePosition, "gatePosition")
			.AddField(this.isGate, "isGate")
			.AddField(this.repairQueued, "repairQueued");
		base.itemId.fieldChangeVisibleEvent += delegate
		{
			this.OnIdChanged();
		};
		this.isGate.fieldChangeVisibleEvent += delegate
		{
			this.OnIdChanged();
		};
	}

	public Fence(Vector2 tileLocation, string itemId, bool isGate)
		: base(itemId, 1)
	{
		if (Fence.fenceDrawGuide == null)
		{
			Fence.populateFenceDrawGuide();
		}
		base.Type = "Crafting";
		this.isGate.Value = isGate;
		this.TileLocation = tileLocation;
		base.canBeSetDown.Value = true;
		base.canBeGrabbed.Value = true;
		base.price.Value = 1;
		this.ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
		if (isGate)
		{
			this.health.Value *= 2f;
		}
		this.OnIdChanged();
	}

	public Fence()
		: this(Vector2.Zero, "322", isGate: false)
	{
	}

	public virtual void ResetHealth(float amount_adjustment)
	{
		float base_health = this.GetData()?.Health ?? 100;
		if (this.isGate.Value)
		{
			amount_adjustment = 0f;
		}
		this.health.Value = base_health + amount_adjustment;
		this.health.Value *= 2f;
		this.maxHealth.Value = this.health.Value;
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		switch (this.obsolete_whichType ?? 1)
		{
		case 2:
			base.ItemId = "323";
			break;
		case 3:
			base.ItemId = "324";
			break;
		case 4:
			base.ItemId = "325";
			break;
		case 5:
			base.ItemId = "298";
			break;
		default:
			base.ItemId = "322";
			break;
		}
		this.obsolete_whichType = null;
	}

	/// <summary>Reset the fence data and texture when the item ID changes (e.g. when the save is being loaded).</summary>
	protected virtual void OnIdChanged()
	{
		if (this.fenceTexture == null || this.fenceTexture.IsValueCreated)
		{
			this.fenceTexture = new Lazy<Texture2D>(loadFenceTexture);
		}
		this._data = null;
	}

	public virtual void repair()
	{
		this.ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
	}

	public static void populateFenceDrawGuide()
	{
		Fence.fenceDrawGuide = new Dictionary<int, int>
		{
			[0] = 5,
			[10] = 9,
			[100] = 10,
			[1000] = 3,
			[500] = 5,
			[1010] = 8,
			[1100] = 6,
			[1500] = 3,
			[600] = 0,
			[510] = 2,
			[110] = 7,
			[1600] = 0,
			[1610] = 4,
			[1510] = 2,
			[1110] = 7,
			[610] = 4
		};
	}

	public virtual void PerformRepairIfNecessary()
	{
		if (Game1.IsMasterGame && this.repairQueued.Value)
		{
			this.ResetHealth(this.GetRepairHealthAdjustment());
			this.repairQueued.Value = false;
		}
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		this.PerformRepairIfNecessary();
		int gatePosition = this.gatePosition.Get();
		gatePosition += this.gateMotion;
		if (gatePosition == 88)
		{
			int drawSum = this.getDrawSum();
			if (drawSum != 110 && drawSum != 1500 && drawSum != 1000 && drawSum != 500 && drawSum != 100 && drawSum != 10)
			{
				this.toggleGate(Game1.player, open: false);
			}
		}
		this.gatePosition.Set(gatePosition);
		if (gatePosition >= 88 || gatePosition <= 0)
		{
			this.gateMotion = 0;
		}
		base.heldObject.Get()?.updateWhenCurrentLocation(time);
	}

	public static Dictionary<string, FenceData> GetFenceLookup()
	{
		if (Fence._FenceLookup == null)
		{
			Fence._LoadFenceData();
		}
		return Fence._FenceLookup;
	}

	/// <summary>Get the fence's data from <c>Data/Fences</c>, if found.</summary>
	public FenceData GetData()
	{
		if (this._data == null)
		{
			Fence.TryGetData(base.ItemId, out this._data);
		}
		return this._data;
	}

	/// <summary>Try to get a fence's data from <c>Data/Fences</c>.</summary>
	/// <param name="itemId">The fence's unqualified item ID (i.e. the key in <c>Data/Fences</c>).</param>
	/// <param name="data">The fence data, if found.</param>
	/// <returns>Returns whether the fence data was found.</returns>
	public static bool TryGetData(string itemId, out FenceData data)
	{
		if (itemId == null)
		{
			data = null;
			return false;
		}
		return Fence.GetFenceLookup().TryGetValue(itemId, out data);
	}

	protected static void _LoadFenceData()
	{
		Fence._FenceLookup = DataLoader.Fences(Game1.content);
	}

	public int getDrawSum()
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return 0;
		}
		int drawSum = 0;
		Vector2 surroundingLocations = base.tileLocation.Value;
		surroundingLocations.X += 1f;
		if (location.objects.TryGetValue(surroundingLocations, out var rightObj) && rightObj is Fence rightFence && rightFence.countsForDrawing(base.ItemId))
		{
			drawSum += 100;
		}
		surroundingLocations.X -= 2f;
		if (location.objects.TryGetValue(surroundingLocations, out var leftObj) && leftObj is Fence leftFence && leftFence.countsForDrawing(base.ItemId))
		{
			drawSum += 10;
		}
		surroundingLocations.X += 1f;
		surroundingLocations.Y += 1f;
		if (location.objects.TryGetValue(surroundingLocations, out var downObj) && downObj is Fence downFence && downFence.countsForDrawing(base.ItemId))
		{
			drawSum += 500;
		}
		surroundingLocations.Y -= 2f;
		if (location.objects.TryGetValue(surroundingLocations, out var upObj) && upObj is Fence upFence && upFence.countsForDrawing(base.ItemId))
		{
			drawSum += 1000;
		}
		return drawSum;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		if (!justCheckingForActivity && who != null)
		{
			Point playerTile = who.TilePoint;
			if (location.objects.TryGetValue(new Vector2(playerTile.X, playerTile.Y - 1), out var upObj) && location.objects.TryGetValue(new Vector2(playerTile.X, playerTile.Y + 1), out var downObj) && location.objects.TryGetValue(new Vector2(playerTile.X + 1, playerTile.Y), out var rightObj) && location.objects.TryGetValue(new Vector2(playerTile.X - 1, playerTile.Y), out var leftObj) && !upObj.isPassable() && !downObj.isPassable() && !leftObj.isPassable() && !rightObj.isPassable())
			{
				this.performToolAction(null);
			}
		}
		if (this.health.Value <= 1f)
		{
			return false;
		}
		if (this.isGate.Value)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (this.isGate.Value)
			{
				this.toggleGate(who, this.gatePosition.Value == 0);
			}
			return true;
		}
		if (justCheckingForActivity)
		{
			return false;
		}
		foreach (Vector2 v in Utility.getAdjacentTileLocations(base.tileLocation.Value))
		{
			if (location.objects.TryGetValue(v, out var obj) && obj is Fence fence && fence.isGate.Value)
			{
				fence.checkForAction(who);
				return true;
			}
		}
		return this.health.Value <= 0f;
	}

	public virtual void toggleGate(bool open, bool is_toggling_counterpart = false, Farmer who = null)
	{
		if (this.health.Value <= 1f)
		{
			return;
		}
		GameLocation location = this.Location;
		if (location == null)
		{
			return;
		}
		int drawSum = this.getDrawSum();
		if (drawSum == 110 || drawSum == 1500 || drawSum == 1000 || drawSum == 500 || drawSum == 100 || drawSum == 10)
		{
			who?.TemporaryPassableTiles.Add(new Rectangle((int)base.tileLocation.X * 64, (int)base.tileLocation.Y * 64, 64, 64));
			if (open)
			{
				this.gatePosition.Value = 88;
			}
			else
			{
				this.gatePosition.Value = 0;
			}
			if (!is_toggling_counterpart)
			{
				location?.playSound("doorClose");
			}
		}
		else
		{
			who?.TemporaryPassableTiles.Add(new Rectangle((int)base.tileLocation.X * 64, (int)base.tileLocation.Y * 64, 64, 64));
			this.gatePosition.Value = 0;
		}
		if (is_toggling_counterpart)
		{
			return;
		}
		switch (drawSum)
		{
		case 100:
		{
			Vector2 neighborTile2 = base.tileLocation.Value + new Vector2(-1f, 0f);
			if (location.objects.TryGetValue(neighborTile2, out var neighbor2) && neighbor2 is Fence fence2 && fence2.isGate.Value && fence2.getDrawSum() == 10)
			{
				fence2.toggleGate(this.gatePosition.Value != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		case 10:
		{
			Vector2 neighborTile4 = base.tileLocation.Value + new Vector2(1f, 0f);
			if (location.objects.TryGetValue(neighborTile4, out var neighbor4) && neighbor4 is Fence fence4 && fence4.isGate.Value && fence4.getDrawSum() == 100)
			{
				fence4.toggleGate(this.gatePosition.Value != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		case 1000:
		{
			Vector2 neighborTile3 = base.tileLocation.Value + new Vector2(0f, 1f);
			if (location.objects.TryGetValue(neighborTile3, out var neighbor3) && neighbor3 is Fence fence3 && fence3.isGate.Value && fence3.getDrawSum() == 500)
			{
				fence3.toggleGate(this.gatePosition.Value != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		case 500:
		{
			Vector2 neighborTile = base.tileLocation.Value + new Vector2(0f, -1f);
			if (location.objects.TryGetValue(neighborTile, out var neighbor) && neighbor is Fence fence && fence.isGate.Value && fence.getDrawSum() == 1000)
			{
				fence.toggleGate(this.gatePosition.Value != 0, is_toggling_counterpart: true, who);
			}
			break;
		}
		}
	}

	public void toggleGate(Farmer who, bool open, bool is_toggling_counterpart = false)
	{
		this.toggleGate(open, is_toggling_counterpart, who);
	}

	public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
	{
		location.debris.Add(new Debris(base.ItemId, origin, destination));
	}

	public override bool performToolAction(Tool t)
	{
		GameLocation location = this.Location;
		if (base.heldObject.Value != null && t != null && !(t is MeleeWeapon) && t.isHeavyHitter())
		{
			Object value = base.heldObject.Value;
			base.heldObject.Value.performRemoveAction();
			base.heldObject.Value = null;
			Game1.createItemDebris(value.getOne(), this.TileLocation * 64f, -1);
			base.playNearbySoundAll("axchop");
		}
		else if (this.isGate.Value && (t is Axe || t is Pickaxe))
		{
			base.playNearbySoundAll("axchop");
			Game1.createObjectDebris("(O)325", (int)base.tileLocation.X, (int)base.tileLocation.Y, Game1.player.UniqueMultiplayerID, location);
			location.objects.Remove(base.tileLocation.Value);
			Game1.createRadialDebris(location, 12, (int)base.tileLocation.X, (int)base.tileLocation.Y, 6, resource: false);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(base.tileLocation.X * 64f, base.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
		}
		else if (!this.isGate.Value && this.IsValidRemovalTool(t))
		{
			FenceData data = this.GetData();
			string sound = data?.RemovalSound ?? data?.PlacementSound ?? "hammer";
			int removalDebrisType = data?.RemovalDebrisType ?? 14;
			base.playNearbySoundAll(sound);
			location.objects.Remove(base.tileLocation.Value);
			for (int i = 0; i < 4; i++)
			{
				location.temporarySprites.Add(new CosmeticDebris(this.fenceTexture.Value, new Vector2(base.tileLocation.X * 64f + 32f, base.tileLocation.Y * 64f + 32f), (float)Game1.random.Next(-5, 5) / 100f, (float)Game1.random.Next(-64, 64) / 30f, (float)Game1.random.Next(-800, -100) / 100f, (int)((base.tileLocation.Y + 1f) * 64f), new Rectangle(32 + Game1.random.Next(2) * 16 / 2, 96 + Game1.random.Next(2) * 16 / 2, 8, 8), Color.White, Game1.soundBank.GetCue("shiny4"), null, 0, 200));
			}
			Game1.createRadialDebris(location, removalDebrisType, (int)base.tileLocation.X, (int)base.tileLocation.Y, 6, resource: false);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(base.tileLocation.X * 64f, base.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextBool(), 50f));
			if (this.maxHealth.Value - this.health.Value < 0.5f)
			{
				location.debris.Add(new Debris(new Object(base.ItemId, 1), base.tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
		}
		return false;
	}

	/// <summary>Get whether a tool can be used to break this fence.</summary>
	/// <param name="tool">The tool instance to check.</param>
	public virtual bool IsValidRemovalTool(Tool tool)
	{
		if (tool == null)
		{
			return !this.isGate.Value;
		}
		FenceData data = this.GetData();
		List<string> removalToolIds = data?.RemovalToolIds;
		List<string> removalToolTypes = data?.RemovalToolTypes;
		bool allowAnyTool = true;
		if (removalToolIds != null && removalToolIds.Count > 0)
		{
			allowAnyTool = false;
			string toolName = tool.Name;
			foreach (string requiredName in removalToolIds)
			{
				if (toolName == requiredName)
				{
					return true;
				}
			}
		}
		if (removalToolTypes != null && removalToolTypes.Count > 0)
		{
			allowAnyTool = false;
			string toolType = tool.GetType().FullName;
			foreach (string requiredType in removalToolTypes)
			{
				if (toolType == requiredType)
				{
					return true;
				}
			}
		}
		return allowAnyTool;
	}

	/// <inheritdoc />
	public override bool minutesElapsed(int minutes)
	{
		if (!Game1.IsMasterGame)
		{
			return false;
		}
		this.PerformRepairIfNecessary();
		if (!Game1.IsBuildingConstructed("Gold Clock") || Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
		{
			this.health.Value -= (float)minutes / 1440f;
			if (this.health.Value <= -1f && (Game1.timeOfDay <= 610 || Game1.timeOfDay > 1800))
			{
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		base.actionOnPlayerEntry();
		if (base.heldObject.Value != null)
		{
			base.heldObject.Value.TileLocation = base.tileLocation.Value;
			base.heldObject.Value.Location = this.Location;
			base.heldObject.Value.actionOnPlayerEntry();
			base.heldObject.Value.isOn.Value = true;
			base.heldObject.Value.initializeLightSource(base.tileLocation.Value);
		}
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		GameLocation location = this.Location;
		if (location == null)
		{
			return false;
		}
		if (dropInItem.HasTypeObject() && dropInItem.ItemId == "325")
		{
			if (probe)
			{
				return false;
			}
			if (!this.isGate.Value)
			{
				int drawSum = this.getDrawSum();
				if (drawSum == 1500 || drawSum == 110 || drawSum == 1000 || drawSum == 10 || drawSum == 100 || drawSum == 500)
				{
					Vector2 neighbor = default(Vector2);
					switch (drawSum)
					{
					case 10:
						neighbor = base.tileLocation.Value + new Vector2(1f, 0f);
						if (location.objects.GetValueOrDefault(neighbor) is Fence rightFence && rightFence.isGate.Value)
						{
							int neighborSum2 = rightFence.getDrawSum();
							if (neighborSum2 != 100 && neighborSum2 != 110)
							{
								return false;
							}
						}
						break;
					case 100:
						neighbor = base.tileLocation.Value + new Vector2(-1f, 0f);
						if (location.objects.GetValueOrDefault(neighbor) is Fence leftFence && leftFence.isGate.Value)
						{
							int neighborSum4 = leftFence.getDrawSum();
							if (neighborSum4 != 10 && neighborSum4 != 110)
							{
								return false;
							}
						}
						break;
					case 1000:
						neighbor = base.tileLocation.Value + new Vector2(0f, 1f);
						if (location.objects.GetValueOrDefault(neighbor) is Fence downFence && downFence.isGate.Value)
						{
							int neighborSum3 = downFence.getDrawSum();
							if (neighborSum3 != 500 && neighborSum3 != 1500)
							{
								return false;
							}
						}
						break;
					case 500:
						neighbor = base.tileLocation.Value + new Vector2(0f, -1f);
						if (location.objects.GetValueOrDefault(neighbor) is Fence upFence && upFence.isGate.Value)
						{
							int neighborSum = upFence.getDrawSum();
							if (neighborSum != 1000 && neighborSum != 1500)
							{
								return false;
							}
						}
						break;
					}
					Vector2[] array = new Vector2[4]
					{
						base.tileLocation.Value + new Vector2(1f, 0f),
						base.tileLocation.Value + new Vector2(-1f, 0f),
						base.tileLocation.Value + new Vector2(0f, -1f),
						base.tileLocation.Value + new Vector2(0f, 1f)
					};
					foreach (Vector2 adjacent_tile in array)
					{
						if (!(adjacent_tile == neighbor) && location.objects.TryGetValue(adjacent_tile, out var adjacent) && adjacent is Fence fence && fence.isGate.Value && fence.Type == base.Type)
						{
							return false;
						}
					}
					if (base.heldObject.Value != null)
					{
						Object value = base.heldObject.Value;
						base.heldObject.Value.performRemoveAction();
						base.heldObject.Value = null;
						Game1.createItemDebris(value.getOne(), this.TileLocation * 64f, -1);
					}
					this.isGate.Value = true;
					if (Fence.TryGetData("325", out var gateData))
					{
						location.playSound(gateData.PlacementSound);
					}
					return true;
				}
			}
		}
		else if (dropInItem.QualifiedItemId == "(O)93" && base.heldObject.Value == null && !this.isGate.Value)
		{
			if (!probe)
			{
				base.heldObject.Value = new Torch();
				location.playSound("axe");
				base.heldObject.Value.Location = this.Location;
				base.heldObject.Value.initializeLightSource(base.tileLocation.Value);
			}
			return true;
		}
		if (this.health.Value <= 1f && !this.repairQueued.Value && this.CanRepairWithThisItem(dropInItem))
		{
			if (!probe)
			{
				string repair_sound = this.GetRepairSound();
				if (!string.IsNullOrEmpty(repair_sound))
				{
					location.playSound(repair_sound);
				}
				this.repairQueued.Value = true;
			}
			return true;
		}
		return base.performObjectDropInAction(dropInItem, probe, who, returnFalseIfItemConsumed);
	}

	public virtual float GetRepairHealthAdjustment()
	{
		FenceData data = this.GetData();
		if (data == null)
		{
			return 0f;
		}
		return Utility.RandomFloat(data.RepairHealthAdjustmentMinimum, data.RepairHealthAdjustmentMaximum);
	}

	public virtual string GetRepairSound()
	{
		return this.GetData()?.PlacementSound ?? "";
	}

	public virtual bool CanRepairWithThisItem(Item item)
	{
		if (this.health.Value > 1f)
		{
			return false;
		}
		if (item == null)
		{
			return false;
		}
		return item.QualifiedItemId == base.QualifiedItemId;
	}

	/// <inheritdoc />
	public override bool performDropDownAction(Farmer who)
	{
		return false;
	}

	public virtual Texture2D loadFenceTexture()
	{
		if (base.ItemId == "325")
		{
			this.isGate.Value = true;
		}
		FenceData data = this.GetData();
		if (data == null)
		{
			return ItemRegistry.RequireTypeDefinition(this.TypeDefinitionId).GetErrorTexture();
		}
		return Game1.content.Load<Texture2D>(data.Texture);
	}

	public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		spriteBatch.Draw(this.fenceTexture.Value, objectPosition - new Vector2(0f, 64f), new Rectangle(5 * Fence.fencePieceWidth % this.fenceTexture.Value.Bounds.Width, 5 * Fence.fencePieceWidth / this.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)(f.StandingPixel.Y + 1) / 10000f);
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scale, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		location.Y -= 64f * scale;
		int drawSum = this.getDrawSum();
		int sourceRectPosition = Fence.fenceDrawGuide[drawSum];
		if (this.isGate.Value)
		{
			switch (drawSum)
			{
			case 110:
				spriteBatch.Draw(this.fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle(0, 512, 88, 24), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				return;
			case 1500:
				spriteBatch.Draw(this.fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle(112, 512, 16, 64), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				return;
			}
		}
		spriteBatch.Draw(this.fenceTexture.Value, location + new Vector2(32f, 32f) * scale, Game1.getArbitrarySourceRect(this.fenceTexture.Value, 64, 128, sourceRectPosition), color * transparency, 0f, new Vector2(32f, 32f) * scale, scale, SpriteEffects.None, layerDepth);
	}

	public bool countsForDrawing(string otherItemId)
	{
		if ((this.health.Value > 1f || this.repairQueued.Value) && !this.isGate.Value)
		{
			if (!(otherItemId == base.ItemId))
			{
				return otherItemId == "325";
			}
			return true;
		}
		return false;
	}

	public override bool isPassable()
	{
		if (this.isGate.Value)
		{
			return this.gatePosition.Value >= 88;
		}
		return false;
	}

	public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
	{
		int sourceRectPosition = 1;
		FenceData data = this.GetData();
		if (data == null)
		{
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition(this.TypeDefinitionId);
			b.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(base.tileLocation.X * 64f, base.tileLocation.Y * 64f)), itemType.GetErrorSourceRect(), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);
			return;
		}
		if (this.health.Value > 1f || this.repairQueued.Value)
		{
			int drawSum = this.getDrawSum();
			sourceRectPosition = Fence.fenceDrawGuide[drawSum];
			if (this.isGate.Value)
			{
				Vector2 offset = new Vector2(0f, 0f);
				switch (drawSum)
				{
				case 10:
					b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 192, 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
					return;
				case 100:
					b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 240, 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
					return;
				case 1000:
					b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 288, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
					return;
				case 500:
					b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 320, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
					return;
				case 110:
					b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 - 16, y * 64 - 64)), new Rectangle((this.gatePosition.Value == 88) ? 24 : 0, 128, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
					return;
				case 1500:
					b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle((this.gatePosition.Value == 88) ? 16 : 0, 160, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
					b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(x * 64 + 20, y * 64 - 64 + 44)), new Rectangle((this.gatePosition.Value == 88) ? 16 : 0, 176, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
					return;
				}
				sourceRectPosition = 17;
			}
			else if (base.heldObject.Value != null)
			{
				Vector2 offset2 = Vector2.Zero;
				offset2 += data.HeldObjectDrawOffset;
				switch (drawSum)
				{
				case 10:
					offset2.X = data.RightEndHeldObjectDrawX;
					break;
				case 100:
					offset2.X = data.LeftEndHeldObjectDrawX;
					break;
				}
				offset2 *= 4f;
				base.heldObject.Value.draw(b, x * 64 + (int)offset2.X, y * 64 + (int)offset2.Y, (float)(y * 64 + 64) / 10000f, 1f);
			}
		}
		b.Draw(this.fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64)), new Rectangle(sourceRectPosition * Fence.fencePieceWidth % this.fenceTexture.Value.Bounds.Width, sourceRectPosition * Fence.fencePieceWidth / this.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);
	}
}
