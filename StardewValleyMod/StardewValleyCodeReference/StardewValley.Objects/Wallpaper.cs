using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData;
using StardewValley.Locations;

namespace StardewValley.Objects;

public class Wallpaper : Object
{
	[XmlElement("sourceRect")]
	public readonly NetRectangle sourceRect = new NetRectangle();

	/// <summary>Whether this is a flooring item; else it's a wallpaper item.</summary>
	[XmlElement("isFloor")]
	public readonly NetBool isFloor = new NetBool(value: false);

	/// <summary>The <c>Data/AdditionalWallpaperFlooring</c> set which contains this flooring or wallpaper, or <c>null</c> for a pre-1.6 vanilla wallpaper.</summary>
	[XmlElement("sourceTexture")]
	public readonly NetString setId = new NetString(null);

	/// <summary>The cached data for the flooring or wallpaper set.</summary>
	protected ModWallpaperOrFlooring setData;

	private static readonly Rectangle wallpaperContainerRect = new Rectangle(39, 31, 16, 16);

	private static readonly Rectangle floorContainerRect = new Rectangle(55, 31, 16, 16);

	/// <inheritdoc />
	public override string TypeDefinitionId
	{
		get
		{
			if (!this.isFloor.Value)
			{
				return "(WP)";
			}
			return "(FL)";
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override string Name => base.name;

	public Wallpaper()
	{
	}

	public Wallpaper(int which, bool isFloor = false)
		: this()
	{
		base.ItemId = which.ToString();
		this.isFloor.Value = isFloor;
		base.ParentSheetIndex = which;
		base.name = (isFloor ? "Flooring" : "Wallpaper");
		this.sourceRect.Value = (isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
		base.price.Value = 100;
	}

	public Wallpaper(string setId, int which)
		: this()
	{
		base.ItemId = $"{setId}:{which}";
		this.setId.Value = setId;
		base.ParentSheetIndex = which;
		ModWallpaperOrFlooring setData = this.GetSetData();
		if (setData == null)
		{
			this.setId.Value = null;
		}
		this.isFloor.Value = setData?.IsFlooring ?? false;
		this.sourceRect.Value = (this.isFloor.Value ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
		if (setData != null && this.isFloor.Value)
		{
			this.sourceRect.Y = which / 8 * 32;
		}
		base.name = (this.isFloor.Value ? "Flooring" : "Wallpaper");
		base.price.Value = 100;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.sourceRect, "sourceRect").AddField(this.isFloor, "isFloor").AddField(this.setId, "setId");
	}

	/// <summary>Get the data for the flooring or wallpaper set which contains this item, if any.</summary>
	public virtual ModWallpaperOrFlooring GetSetData()
	{
		if (this.setId.Value == null)
		{
			return null;
		}
		if (this.setData != null)
		{
			return this.setData;
		}
		foreach (ModWallpaperOrFlooring entry in DataLoader.AdditionalWallpaperFlooring(Game1.content))
		{
			if (entry.Id == this.setId.Value)
			{
				this.setData = entry;
				return entry;
			}
		}
		return null;
	}

	/// <inheritdoc />
	protected override string loadDisplayName()
	{
		if (!this.isFloor.Value)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13204");
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203");
	}

	public override string getDescription()
	{
		if (!this.isFloor.Value)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13206");
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13205");
	}

	/// <inheritdoc />
	public override bool performDropDownAction(Farmer who)
	{
		return true;
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		return false;
	}

	public override bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
	{
		Vector2 nonTile = tile * 64f;
		nonTile.X += 32f;
		nonTile.Y += 32f;
		foreach (Furniture f in l.furniture)
		{
			if (f.furniture_type.Value != 12 && f.GetBoundingBox().Contains((int)nonTile.X, (int)nonTile.Y))
			{
				return false;
			}
		}
		return true;
	}

	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		if (location is DecoratableLocation decoratableLocation)
		{
			Point tile = new Point(x / 64, y / 64);
			if (this.isFloor.Value)
			{
				string floor_id = decoratableLocation.GetFloorID(tile.X, tile.Y);
				if (floor_id != null)
				{
					if (this.GetSetData() != null)
					{
						decoratableLocation.SetFloor(this.GetSetData().Id + ":" + base.parentSheetIndex.Value, floor_id);
					}
					else
					{
						decoratableLocation.SetFloor(base.parentSheetIndex.Value.ToString(), floor_id);
					}
					location.playSound("coin");
					return true;
				}
			}
			else
			{
				string wall_id = decoratableLocation.GetWallpaperID(tile.X, tile.Y);
				if (wall_id != null)
				{
					if (this.GetSetData() != null)
					{
						decoratableLocation.SetWallpaper(this.GetSetData().Id + ":" + base.parentSheetIndex.Value, wall_id);
					}
					else
					{
						decoratableLocation.SetWallpaper(base.parentSheetIndex.Value.ToString(), wall_id);
					}
					location.playSound("coin");
					return true;
				}
			}
		}
		return false;
	}

	public override bool isPlaceable()
	{
		return true;
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return base.price.Value;
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		base.drawInMenu(spriteBatch, objectPosition, 1f);
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		Texture2D wallpaperTexture;
		if (this.GetSetData() != null)
		{
			try
			{
				wallpaperTexture = Game1.content.Load<Texture2D>(this.GetSetData().Texture);
			}
			catch (Exception)
			{
				wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
			}
		}
		else
		{
			wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
		}
		if (this.isFloor.Value)
		{
			spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), Wallpaper.floorContainerRect, color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 30f), this.sourceRect.Value, color * transparency, 0f, new Vector2(14f, 13f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
		}
		else
		{
			spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), Wallpaper.wallpaperContainerRect, color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 32f), this.sourceRect.Value, color * transparency, 0f, new Vector2(8f, 14f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
		}
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		ModWallpaperOrFlooring data = this.GetSetData();
		if (data == null)
		{
			return new Wallpaper(base.parentSheetIndex.Value, this.isFloor.Value);
		}
		return new Wallpaper(data.Id, base.parentSheetIndex.Value);
	}
}
