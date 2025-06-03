using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>The base parsed metadata for an item.</summary>
public class ParsedItemData : IHaveItemTypeId
{
	/// <summary>Whether the <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.Texture" /> has been loaded, regardless of whether the load was successful.</summary>
	private bool LoadedTexture;

	/// <summary>The texture containing the sprites to render for this item.</summary>
	private Texture2D Texture;

	/// <summary>The pixel area for the default sprite within the <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.Texture" />.</summary>
	private Rectangle DefaultSourceRect;

	/// <summary>The item type which defines this item.</summary>
	public readonly IItemDataDefinition ItemType;

	/// <summary>The item's unqualified ID within the <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.ItemType" />.</summary>
	public readonly string ItemId;

	/// <summary>The item's qualified ID.</summary>
	public readonly string QualifiedItemId;

	/// <summary>The item's index within the sprite sheet.</summary>
	public readonly int SpriteIndex;

	/// <summary>The asset name for the sprite sheet to use when drawing the item to the screen.</summary>
	public readonly string TextureName;

	/// <summary>The internal (non-localized) item name.</summary>
	public readonly string InternalName;

	/// <summary>The localized item name.</summary>
	public readonly string DisplayName;

	/// <summary>The localized item description.</summary>
	public readonly string Description;

	/// <summary>The object category ID.</summary>
	public readonly int Category;

	/// <summary>The object type.</summary>
	/// <remarks>This is the in-game type like <see cref="P:StardewValley.Object.Type" />, not the item type definition.</remarks>
	public readonly string ObjectType;

	/// <summary>The raw data fields from the underlying data asset if applicable, else <c>null</c>.</summary>
	public readonly object RawData;

	/// <summary>Whether this is a broken Error Item instance.</summary>
	public readonly bool IsErrorItem;

	/// <summary>Whether to exclude this item from shops when selecting random items to sell, including catalogues.</summary>
	public readonly bool ExcludeFromRandomSale;

	/// <summary>Construct an instance.</summary>
	/// <param name="itemType">The item type which defines this item.</param>
	/// <param name="itemId">The item's unqualified ID within the <paramref name="itemType" />.&gt;</param>
	/// <param name="spriteIndex">The item's index within the sprite sheet.</param>
	/// <param name="textureName">The asset name for the sprite sheet to use when drawing the item to the screen.</param>
	/// <param name="internalName">The internal (non-localized) item name.</param>
	/// <param name="displayName">The localized item name.</param>
	/// <param name="description">The localized item description.</param>
	/// <param name="category">The object category ID.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="rawData">The raw data fields from the underlying data asset if applicable, else <c>null</c>.</param>
	/// <param name="isErrorItem">Whether this is a broken Error Item instance.</param>
	/// <param name="excludeFromRandomSale">Whether to exclude this item from shops when selecting random items to sell, including catalogues.</param>
	public ParsedItemData(IItemDataDefinition itemType, string itemId, int spriteIndex, string textureName, string internalName, string displayName, string description, int category, string objectType, object rawData, bool isErrorItem = false, bool excludeFromRandomSale = false)
	{
		string qualifiedItemId = itemType.Identifier + itemId;
		if (string.IsNullOrWhiteSpace(internalName))
		{
			internalName = qualifiedItemId;
		}
		if (string.IsNullOrWhiteSpace(displayName))
		{
			displayName = ItemRegistry.GetUnnamedItemName(qualifiedItemId);
		}
		this.ItemType = itemType;
		this.ItemId = itemId;
		this.QualifiedItemId = qualifiedItemId;
		this.SpriteIndex = spriteIndex;
		this.TextureName = textureName;
		this.InternalName = internalName;
		this.DisplayName = displayName;
		this.Description = description;
		this.Category = category;
		this.ObjectType = objectType;
		this.RawData = rawData;
		this.IsErrorItem = isErrorItem;
		this.ExcludeFromRandomSale = excludeFromRandomSale;
		if (this.IsErrorItem)
		{
			this.LoadedTexture = true;
		}
	}

	/// <inheritdoc />
	public string GetItemTypeId()
	{
		return this.ItemType.Identifier;
	}

	/// <summary>Get the texture to render for this item.</summary>
	public virtual Texture2D GetTexture()
	{
		if (!this.IsErrorItem)
		{
			this.LoadTextureIfNeeded();
			Texture2D texture = this.Texture;
			if (texture != null)
			{
				return texture;
			}
		}
		return this.ItemType.GetErrorTexture();
	}

	/// <summary>Get the texture name to render for this item.</summary>
	public virtual string GetTextureName()
	{
		if (!this.IsErrorItem)
		{
			this.LoadTextureIfNeeded();
			string textureName = this.TextureName;
			if (this.Texture != null && textureName != null)
			{
				return textureName;
			}
		}
		return this.ItemType.GetErrorTextureName();
	}

	/// <summary>Get the pixel rectangle to render for the item's sprite within the texture returned by <see cref="M:StardewValley.ItemTypeDefinitions.ParsedItemData.GetTexture" /> or <see cref="M:StardewValley.ItemTypeDefinitions.ParsedItemData.GetTextureName" />.</summary>
	/// <param name="offset">An index offset to apply to the sprite index.</param>
	/// <param name="spriteIndex">The sprite index to render, or <c>null</c> to use the parsed <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.SpriteIndex" />.</param>
	public virtual Rectangle GetSourceRect(int offset = 0, int? spriteIndex = null)
	{
		if (!this.IsErrorItem)
		{
			this.LoadTextureIfNeeded();
			if (this.Texture != null)
			{
				if (offset != 0 || (spriteIndex.HasValue && spriteIndex != this.SpriteIndex))
				{
					return this.ItemType.GetSourceRect(this, this.Texture, (spriteIndex ?? this.SpriteIndex) + offset);
				}
				return this.DefaultSourceRect;
			}
		}
		return this.ItemType.GetErrorSourceRect();
	}

	/// <summary>Get whether the item specifies an object category.</summary>
	public virtual bool HasCategory()
	{
		return this.Category < -1;
	}

	/// <summary>Load the texture data if it's not already loaded.</summary>
	protected virtual void LoadTextureIfNeeded()
	{
		if (!this.LoadedTexture)
		{
			if (this.IsErrorItem)
			{
				this.Texture = null;
				this.DefaultSourceRect = Rectangle.Empty;
				this.LoadedTexture = true;
			}
			else
			{
				this.Texture = this.TryLoadTexture();
				this.DefaultSourceRect = ((this.Texture == null) ? Rectangle.Empty : this.ItemType.GetSourceRect(this, this.Texture, this.SpriteIndex));
				this.LoadedTexture = true;
			}
		}
	}

	/// <summary>Load the texture instance.</summary>
	protected virtual Texture2D TryLoadTexture()
	{
		string textureName = this.TextureName;
		try
		{
			if (!Game1.content.DoesAssetExist<Texture2D>(textureName))
			{
				Game1.log.Error($"Failed loading texture {textureName} for item {this.QualifiedItemId}: asset doesn't exist.");
				return null;
			}
			return Game1.content.Load<Texture2D>(textureName);
		}
		catch (Exception exception)
		{
			Game1.log.Error($"Failed loading texture {textureName} for item {this.QualifiedItemId}.", exception);
			return null;
		}
	}
}
