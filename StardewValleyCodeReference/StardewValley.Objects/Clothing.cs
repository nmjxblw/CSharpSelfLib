using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.Shirts;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects;

public class Clothing : Item
{
	public enum ClothesType
	{
		SHIRT,
		PANTS
	}

	public const int SHIRT_SHEET_WIDTH = 128;

	public const string DefaultShirtSheetName = "Characters\\Farmer\\shirts";

	public const string DefaultPantsSheetName = "Characters\\Farmer\\pants";

	public const int MinShirtId = 1000;

	[XmlElement("price")]
	public readonly NetInt price = new NetInt();

	[XmlElement("indexInTileSheet")]
	public readonly NetInt indexInTileSheet = new NetInt();

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Objects.Clothing.indexInTileSheet" /> instead.</summary>
	[XmlElement("indexInTileSheetFemale")]
	public int? obsolete_indexInTileSheetFemale;

	[XmlIgnore]
	public string description;

	[XmlIgnore]
	public string displayName;

	[XmlElement("clothesType")]
	public readonly NetEnum<ClothesType> clothesType = new NetEnum<ClothesType>();

	[XmlElement("dyeable")]
	public readonly NetBool dyeable = new NetBool(value: false);

	[XmlElement("clothesColor")]
	public readonly NetColor clothesColor = new NetColor(new Color(255, 255, 255));

	[XmlElement("isPrismatic")]
	public readonly NetBool isPrismatic = new NetBool(value: false);

	[XmlIgnore]
	protected bool _loadedData;

	/// <inheritdoc />
	public override string TypeDefinitionId
	{
		get
		{
			if (this.clothesType.Value != ClothesType.PANTS)
			{
				return "(S)";
			}
			return "(P)";
		}
	}

	public int Price
	{
		get
		{
			return this.price.Value;
		}
		set
		{
			this.price.Value = value;
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName
	{
		get
		{
			if (!this._loadedData)
			{
				this.LoadData();
			}
			return this.displayName;
		}
	}

	public Clothing()
	{
		base.Category = -100;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.price, "price").AddField(this.indexInTileSheet, "indexInTileSheet").AddField(this.clothesType, "clothesType")
			.AddField(this.dyeable, "dyeable")
			.AddField(this.clothesColor, "clothesColor")
			.AddField(this.isPrismatic, "isPrismatic");
	}

	public Clothing(string itemId)
		: this()
	{
		itemId = base.ValidateUnqualifiedItemId(itemId);
		this.Name = "Clothing";
		base.Category = -100;
		base.ItemId = itemId;
		this.LoadData(applyColor: true);
	}

	/// <summary>Apply the data from <see cref="F:StardewValley.Game1.pantsData" /> or <see cref="F:StardewValley.Game1.shirtData" /> to this item instance.</summary>
	/// <param name="applyColor">Whether to parse the tint color in field 6; else the tint is set to neutral white.</param>
	/// <param name="forceReload">Whether to reapply the latest data, even if this item was previously initialized.</param>
	public virtual void LoadData(bool applyColor = false, bool forceReload = false)
	{
		if (this._loadedData && !forceReload)
		{
			return;
		}
		base.Category = -100;
		ShirtData shirtData;
		if (Game1.pantsData.TryGetValue(base.ItemId, out var pantsData))
		{
			this.Name = pantsData.Name;
			this.price.Value = pantsData.Price;
			this.indexInTileSheet.Value = pantsData.SpriteIndex;
			this.dyeable.Value = pantsData.CanBeDyed;
			if (applyColor)
			{
				this.clothesColor.Value = Utility.StringToColor(pantsData.DefaultColor) ?? Color.White;
			}
			else if (forceReload)
			{
				this.clothesColor.Value = Color.White;
			}
			this.displayName = TokenParser.ParseText(pantsData.DisplayName);
			this.description = TokenParser.ParseText(pantsData.Description);
			this.clothesType.Value = ClothesType.PANTS;
			this.isPrismatic.Value = pantsData.IsPrismatic;
		}
		else if (Game1.shirtData.TryGetValue(base.ItemId, out shirtData))
		{
			this.Name = shirtData.Name;
			this.price.Value = shirtData.Price;
			this.indexInTileSheet.Value = shirtData.SpriteIndex;
			this.dyeable.Value = shirtData.CanBeDyed;
			if (applyColor)
			{
				this.clothesColor.Value = Utility.StringToColor(shirtData.DefaultColor) ?? Color.White;
			}
			else if (forceReload)
			{
				this.clothesColor.Value = Color.White;
			}
			this.displayName = TokenParser.ParseText(shirtData.DisplayName);
			this.description = TokenParser.ParseText(shirtData.Description);
			this.clothesType.Value = ClothesType.SHIRT;
			this.isPrismatic.Value = shirtData.IsPrismatic;
		}
		else
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			this.displayName = itemData.DisplayName;
			this.description = itemData.Description;
		}
		if (this.dyeable.Value)
		{
			this.description = this.description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Clothes_Dyeable");
		}
		this._loadedData = true;
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		return Object.GetCategoryDisplayName(-100);
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return this.price.Value;
	}

	public virtual void Dye(Color color, float strength = 0.5f)
	{
		if (this.dyeable.Value)
		{
			Color current_color = this.clothesColor.Value;
			this.clothesColor.Value = new Color(Utility.MoveTowards((float)(int)current_color.R / 255f, (float)(int)color.R / 255f, strength), Utility.MoveTowards((float)(int)current_color.G / 255f, (float)(int)color.G / 255f, strength), Utility.MoveTowards((float)(int)current_color.B / 255f, (float)(int)color.B / 255f, strength), Utility.MoveTowards((float)(int)current_color.A / 255f, (float)(int)color.A / 255f, strength));
		}
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		Color clothes_color = this.clothesColor.Value;
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		Texture2D texture = itemData.GetTexture();
		Rectangle spriteSourceRect = itemData.GetSourceRect();
		Rectangle dyeMaskSourceRect = Rectangle.Empty;
		if (!itemData.IsErrorItem)
		{
			if (this.clothesType.Value == ClothesType.SHIRT)
			{
				dyeMaskSourceRect = new Rectangle(spriteSourceRect.X + texture.Width / 2, spriteSourceRect.Y, spriteSourceRect.Width, spriteSourceRect.Height);
			}
			if (this.isPrismatic.Value)
			{
				clothes_color = Utility.GetPrismaticColor();
			}
		}
		switch (this.clothesType.Value)
		{
		case ClothesType.SHIRT:
		{
			float dye_portion_layer_offset = 1E-07f;
			if (layerDepth >= 1f - dye_portion_layer_offset)
			{
				layerDepth = 1f - dye_portion_layer_offset;
			}
			Vector2 origin = new Vector2(4f, 4f);
			if (itemData.IsErrorItem)
			{
				origin.X = spriteSourceRect.Width / 2;
				origin.Y = spriteSourceRect.Height / 2;
			}
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f), spriteSourceRect, color * transparency, 0f, origin, scaleSize * 4f, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f), dyeMaskSourceRect, Utility.MultiplyColor(clothes_color, color) * transparency, 0f, origin, scaleSize * 4f, SpriteEffects.None, layerDepth + dye_portion_layer_offset);
			break;
		}
		case ClothesType.PANTS:
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f), spriteSourceRect, Utility.MultiplyColor(clothes_color, color) * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
			break;
		}
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override string getDescription()
	{
		if (!this._loadedData)
		{
			this.LoadData();
		}
		return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
	}

	public override bool isPlaceable()
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Clothing(base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Clothing fromClothing)
		{
			this.clothesColor.Value = fromClothing.clothesColor.Value;
		}
	}
}
