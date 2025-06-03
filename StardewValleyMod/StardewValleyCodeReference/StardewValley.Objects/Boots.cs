using System;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class Boots : Item
{
	[XmlElement("defenseBonus")]
	public readonly NetInt defenseBonus = new NetInt();

	[XmlElement("immunityBonus")]
	public readonly NetInt immunityBonus = new NetInt();

	[XmlElement("indexInTileSheet")]
	public readonly NetInt indexInTileSheet = new NetInt();

	[XmlElement("price")]
	public readonly NetInt price = new NetInt();

	[XmlElement("indexInColorSheet")]
	public readonly NetInt indexInColorSheet = new NetInt();

	[XmlElement("appliedBootSheetIndex")]
	public readonly NetString appliedBootSheetIndex = new NetString();

	/// <summary>The cached value for <see cref="P:StardewValley.Objects.Boots.DisplayName" />.</summary>
	[XmlIgnore]
	public string displayName;

	[XmlIgnore]
	public string description;

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(B)";

	/// <inheritdoc />
	[XmlIgnore]
	public override string DisplayName
	{
		get
		{
			if (this.displayName == null)
			{
				this.loadDisplayFields();
			}
			return this.displayName;
		}
	}

	public Boots()
	{
		base.Category = -97;
	}

	public Boots(string itemId)
		: this()
	{
		itemId = base.ValidateUnqualifiedItemId(itemId);
		base.ItemId = itemId;
		this.reloadData();
		base.Category = -97;
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = this.indexInTileSheet.Value.ToString();
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.defenseBonus, "defenseBonus").AddField(this.immunityBonus, "immunityBonus").AddField(this.indexInTileSheet, "indexInTileSheet")
			.AddField(this.price, "price")
			.AddField(this.indexInColorSheet, "indexInColorSheet")
			.AddField(this.appliedBootSheetIndex, "appliedBootSheetIndex");
	}

	public virtual void reloadData()
	{
		ParsedItemData parsedData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		string[] data = DataLoader.Boots(Game1.content)[base.ItemId].Split('/');
		this.Name = ArgUtility.Get(data, 0, null, allowBlank: false) ?? parsedData.InternalName;
		this.price.Value = Convert.ToInt32(data[2]);
		this.defenseBonus.Value = Convert.ToInt32(data[3]);
		this.immunityBonus.Value = Convert.ToInt32(data[4]);
		this.indexInColorSheet.Value = Convert.ToInt32(data[5]);
		this.indexInTileSheet.Value = parsedData.SpriteIndex;
	}

	public void applyStats(Boots applied_boots)
	{
		this.reloadData();
		if (this.defenseBonus.Value == applied_boots.defenseBonus.Value && this.immunityBonus.Value == applied_boots.immunityBonus.Value)
		{
			this.appliedBootSheetIndex.Value = null;
		}
		else
		{
			this.appliedBootSheetIndex.Value = applied_boots.getStatsIndex();
		}
		this.defenseBonus.Value = applied_boots.defenseBonus.Value;
		this.immunityBonus.Value = applied_boots.immunityBonus.Value;
		this.price.Value = applied_boots.price.Value;
		this.loadDisplayFields();
	}

	public virtual string getStatsIndex()
	{
		return this.appliedBootSheetIndex.Value ?? base.ItemId;
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return this.defenseBonus.Value * 100 + this.immunityBonus.Value * 100;
	}

	/// <inheritdoc />
	public override void onEquip(Farmer who)
	{
		base.onEquip(who);
		who.changeShoeColor(this.GetBootsColorString());
	}

	/// <inheritdoc />
	public override void onUnequip(Farmer who)
	{
		base.onUnequip(who);
		who.changeShoeColor("12");
	}

	public override void AddEquipmentEffects(BuffEffects effects)
	{
		base.AddEquipmentEffects(effects);
		effects.Defense.Value += this.defenseBonus.Value;
		effects.Immunity.Value += this.immunityBonus.Value;
	}

	public string GetBootsColorString()
	{
		if (DataLoader.Boots(Game1.content).TryGetValue(base.ItemId, out var rawData))
		{
			string[] split = rawData.Split('/');
			if (split.Length > 7 && split[7] != "")
			{
				return split[7] + ":" + this.indexInColorSheet.Value;
			}
		}
		return this.indexInColorSheet.Value.ToString();
	}

	public int getNumberOfDescriptionCategories()
	{
		if (this.immunityBonus.Value > 0 && this.defenseBonus.Value > 0)
		{
			return 2;
		}
		return 1;
	}

	public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
	{
		Utility.drawTextWithShadow(spriteBatch, Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
		y += (int)font.MeasureString(Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth())).Y;
		if (this.defenseBonus.Value > 0)
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", this.defenseBonus), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
		if (this.immunityBonus.Value > 0)
		{
			Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", this.immunityBonus), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
			y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
		}
	}

	public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
	{
		int maxStat = 9999;
		Point dimensions = new Point(0, startingHeight);
		dimensions.Y -= (int)font.MeasureString(descriptionText).Y;
		dimensions.Y += (int)((float)(this.getNumberOfDescriptionCategories() * 4 * 12) + font.MeasureString(Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth())).Y);
		dimensions.X = (int)Math.Max(minWidth, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", maxStat)).X + (float)horizontalBuffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", maxStat)).X + (float)horizontalBuffer));
		return dimensions;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		spriteBatch.Draw(data.GetTexture(), location + new Vector2(32f, 32f) * scaleSize, data.GetSourceRect(), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	/// <inheritdoc />
	public override string getCategoryName()
	{
		return Object.GetCategoryDisplayName(-97);
	}

	public override string getDescription()
	{
		if (this.description == null)
		{
			this.loadDisplayFields();
		}
		return Game1.parseText(this.description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Boots.cs.12500", this.immunityBonus.Value + this.defenseBonus.Value), Game1.smallFont, this.getDescriptionWidth());
	}

	public override bool isPlaceable()
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Boots(base.ItemId);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is Boots fromBoots)
		{
			this.appliedBootSheetIndex.Value = fromBoots.appliedBootSheetIndex.Value;
			this.indexInColorSheet.Value = fromBoots.indexInColorSheet.Value;
			this.defenseBonus.Value = fromBoots.defenseBonus.Value;
			this.immunityBonus.Value = fromBoots.immunityBonus.Value;
			this.loadDisplayFields();
		}
	}

	protected virtual bool loadDisplayFields()
	{
		if (DataLoader.Boots(Game1.content).TryGetValue(base.ItemId, out var rawData))
		{
			string[] data = rawData.Split('/');
			this.displayName = this.Name;
			if (data.Length > 6)
			{
				this.displayName = data[6];
			}
			if (this.appliedBootSheetIndex.Value != null)
			{
				this.displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CustomizedBootItemName", this.DisplayName);
			}
			this.description = data[1];
			return true;
		}
		return false;
	}
}
