using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley;

public class PurchaseableKeyItem : ISalable, IHaveItemTypeId
{
	protected string _displayName = "";

	protected string _name = "";

	protected string _description = "";

	protected int _price;

	protected int _id;

	protected List<string> _tags;

	protected Action<Farmer> _onPurchase;

	/// <inheritdoc />
	public string TypeDefinitionId => "(Salable)";

	/// <inheritdoc />
	public string QualifiedItemId => this.TypeDefinitionId + "PurchaseableKeyItem." + this.id;

	/// <inheritdoc />
	public string DisplayName => this._displayName;

	public int id => this._id;

	public List<string> tags => this._tags;

	/// <inheritdoc />
	public string Name => this._name;

	public bool IsRecipe
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public int Stack
	{
		get
		{
			return 1;
		}
		set
		{
		}
	}

	public int Quality
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public PurchaseableKeyItem(string display_name, string display_description, int parent_sheet_index, Action<Farmer> on_purchase = null)
	{
		this._id = parent_sheet_index;
		this._name = display_name;
		this._displayName = display_name;
		this._description = display_description;
		this._onPurchase = on_purchase;
	}

	/// <inheritdoc />
	public string GetItemTypeId()
	{
		return this.TypeDefinitionId;
	}

	public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this._id, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
	}

	public bool ShouldDrawIcon()
	{
		return true;
	}

	public string getDescription()
	{
		return this._description;
	}

	public int maximumStackSize()
	{
		return 1;
	}

	public int addToStack(Item stack)
	{
		return 1;
	}

	public bool canStackWith(ISalable other)
	{
		return false;
	}

	/// <inheritdoc />
	public int sellToStorePrice(long specificPlayerID = -1L)
	{
		return -1;
	}

	/// <inheritdoc />
	public int salePrice(bool ignoreProfitMargins = false)
	{
		return this._price;
	}

	/// <inheritdoc />
	public bool appliesProfitMargins()
	{
		return false;
	}

	/// <inheritdoc />
	public bool actionWhenPurchased(string shopId)
	{
		this._onPurchase?.Invoke(Game1.player);
		return true;
	}

	public bool CanBuyItem(Farmer farmer)
	{
		return true;
	}

	public bool IsInfiniteStock()
	{
		return false;
	}

	public ISalable GetSalableInstance()
	{
		return this;
	}

	/// <inheritdoc />
	public void FixStackSize()
	{
	}

	/// <inheritdoc />
	public void FixQuality()
	{
	}
}
