using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Tools;

public class Pickaxe : Tool
{
	public const int hitMargin = 8;

	public const int BoulderStrength = 4;

	private int boulderTileX;

	private int boulderTileY;

	private int hitsToBoulder;

	public NetInt additionalPower = new NetInt(0);

	public Pickaxe()
		: base("Pickaxe", 0, 105, 131, stackable: false)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.additionalPower, "additionalPower");
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		switch (base.UpgradeLevel)
		{
		case 0:
			base.ItemId = "Pickaxe";
			break;
		case 1:
			base.ItemId = "CopperPickaxe";
			break;
		case 2:
			base.ItemId = "SteelPickaxe";
			break;
		case 3:
			base.ItemId = "GoldPickaxe";
			break;
		case 4:
			base.ItemId = "IridiumPickaxe";
			break;
		default:
			base.ItemId = "Pickaxe";
			break;
		}
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Pickaxe();
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Pickaxe fromPickaxe)
		{
			this.additionalPower.Value = fromPickaxe.additionalPower.Value;
		}
	}

	public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
	{
		this.Update(who.FacingDirection, 0, who);
		who.EndUsingTool();
		return true;
	}

	public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
	{
		base.DoFunction(location, x, y, power, who);
		power = who.toolPower.Value;
		if (!base.isEfficient.Value)
		{
			who.Stamina -= (float)(2 * (power + 1)) - (float)who.MiningLevel * 0.1f;
		}
		Utility.clampToTile(new Vector2(x, y));
		int tileX = x / 64;
		int tileY = y / 64;
		Vector2 tile = new Vector2(tileX, tileY);
		if (location.performToolAction(this, tileX, tileY))
		{
			return;
		}
		location.Objects.TryGetValue(tile, out var o);
		if (o == null)
		{
			if (who.FacingDirection == 0 || who.FacingDirection == 2)
			{
				tileX = (x - 8) / 64;
				location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
				if (o == null)
				{
					tileX = (x + 8) / 64;
					location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
				}
			}
			else
			{
				tileY = (y + 8) / 64;
				location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
				if (o == null)
				{
					tileY = (y - 8) / 64;
					location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
				}
			}
			x = tileX * 64;
			y = tileY * 64;
			if (location.terrainFeatures.TryGetValue(tile, out var terrainFeature) && terrainFeature.performToolAction(this, 0, tile))
			{
				location.terrainFeatures.Remove(tile);
			}
		}
		tile = new Vector2(tileX, tileY);
		if (o != null)
		{
			if (o.IsBreakableStone())
			{
				if (base.PlayUseSounds)
				{
					location.playSound("hammer", tile);
				}
				if (o.MinutesUntilReady > 0)
				{
					int damage = Math.Max(1, base.upgradeLevel.Value + 1) + this.additionalPower.Value;
					o.minutesUntilReady.Value -= damage;
					o.shakeTimer = 200;
					if (o.MinutesUntilReady > 0)
					{
						Game1.createRadialDebris(Game1.currentLocation, 14, tileX, tileY, Game1.random.Next(2, 5), resource: false);
						return;
					}
				}
				TemporaryAnimatedSprite sprite = ((ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).TextureName == "Maps\\springobjects" && o.ParentSheetIndex < 200 && !Game1.objectData.ContainsKey((o.ParentSheetIndex + 1).ToString()) && o.QualifiedItemId != "(O)25") ? new TemporaryAnimatedSprite(o.ParentSheetIndex + 1, 300f, 1, 2, new Vector2(x - x % 64, y - y % 64), flicker: true, o.flipped.Value)
				{
					alphaFade = 0.01f
				} : new TemporaryAnimatedSprite(47, new Vector2(tileX * 64, tileY * 64), Color.Gray, 10, flipped: false, 80f));
				Game1.multiplayer.broadcastSprites(location, sprite);
				Game1.createRadialDebris(location, 14, tileX, tileY, Game1.random.Next(2, 5), resource: false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(46, new Vector2(tileX * 64, tileY * 64), Color.White, 10, flipped: false, 80f)
				{
					motion = new Vector2(0f, -0.6f),
					acceleration = new Vector2(0f, 0.002f),
					alphaFade = 0.015f
				});
				location.OnStoneDestroyed(o.ItemId, tileX, tileY, base.getLastFarmerToUse());
				if (who != null && who.stats.Get("Book_Diamonds") != 0 && Game1.random.NextDouble() < 0.0066)
				{
					Game1.createObjectDebris("(O)72", tileX, tileY, who.UniqueMultiplayerID, location);
					if (who.professions.Contains(19) && Game1.random.NextBool())
					{
						Game1.createObjectDebris("(O)72", tileX, tileY, who.UniqueMultiplayerID, location);
					}
				}
				if (o.MinutesUntilReady <= 0)
				{
					o.performRemoveAction();
					location.Objects.Remove(new Vector2(tileX, tileY));
					if (base.PlayUseSounds)
					{
						location.playSound("stoneCrack", tile);
					}
					Game1.stats.RocksCrushed++;
				}
			}
			else if (o.Name.Contains("Boulder"))
			{
				if (base.PlayUseSounds)
				{
					location.playSound("hammer", tile);
				}
				if (base.UpgradeLevel < 2)
				{
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14194")));
					return;
				}
				if (tileX == this.boulderTileX && tileY == this.boulderTileY)
				{
					this.hitsToBoulder += power + 1;
					o.shakeTimer = 190;
				}
				else
				{
					this.hitsToBoulder = 0;
					this.boulderTileX = tileX;
					this.boulderTileY = tileY;
				}
				if (this.hitsToBoulder >= 4)
				{
					location.removeObject(tile, showDestroyedObject: false);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X - 32f, 64f * (tile.Y - 1f)), Color.Gray, 8, Game1.random.NextBool(), 50f)
					{
						delayBeforeAnimationStart = 0
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X + 32f, 64f * (tile.Y - 1f)), Color.Gray, 8, Game1.random.NextBool(), 50f)
					{
						delayBeforeAnimationStart = 200
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X, 64f * (tile.Y - 1f) - 32f), Color.Gray, 8, Game1.random.NextBool(), 50f)
					{
						delayBeforeAnimationStart = 400
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X, 64f * tile.Y - 32f), Color.Gray, 8, Game1.random.NextBool(), 50f)
					{
						delayBeforeAnimationStart = 600
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X, 64f * tile.Y), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, 128));
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X + 32f, 64f * tile.Y), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, 128)
					{
						delayBeforeAnimationStart = 250
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X - 32f, 64f * tile.Y), Color.White, 8, Game1.random.NextBool(), 50f, 0, -1, -1f, 128)
					{
						delayBeforeAnimationStart = 500
					});
					if (base.PlayUseSounds)
					{
						location.playSound("boulderBreak", tile);
					}
				}
			}
			else if (o.performToolAction(this))
			{
				o.performRemoveAction();
				if (o.Type == "Crafting" && o.fragility.Value != 2)
				{
					Game1.currentLocation.debris.Add(new Debris(o.QualifiedItemId, who.GetToolLocation(), Utility.PointToVector2(who.StandingPixel)));
				}
				Game1.currentLocation.Objects.Remove(tile);
			}
		}
		else
		{
			if (base.PlayUseSounds)
			{
				location.playSound("woodyHit", tile);
			}
			if (location.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileX * 64, tileY * 64), Color.White, 8, flipped: false, 80f)
				{
					alphaFade = 0.015f
				});
			}
		}
	}
}
