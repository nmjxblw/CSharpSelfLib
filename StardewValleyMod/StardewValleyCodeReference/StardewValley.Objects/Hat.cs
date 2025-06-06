using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Enchantments;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class Hat : Item
{
	public enum HairDrawType
	{
		DrawFullHair,
		DrawObscuredHair,
		HideHair
	}

	public const int widthOfTileSheetSquare = 20;

	public const int heightOfTileSheetSquare = 20;

	/// <summary>The index in <c>Data/Hats</c> for the internal name field.</summary>
	public const int data_index_internalName = 0;

	/// <summary>The index in <c>Data/Hats</c> for the description field.</summary>
	public const int data_index_description = 1;

	/// <summary>The index in <c>Data/Hats</c> for the 'show full hair' field.</summary>
	public const int data_index_showFullHair = 2;

	/// <summary>The index in <c>Data/Hats</c> for the ignore-hair-offset field.</summary>
	public const int data_index_ignoreHairOffset = 3;

	/// <summary>The index in <c>Data/Hats</c> for the special tags field.</summary>
	public const int data_index_tags = 4;

	/// <summary>The index in <c>Data/Hats</c> for the display name field.</summary>
	public const int data_index_displayName = 5;

	/// <summary>The index in <c>Data/Hats</c> for the texture field.</summary>
	public const int data_index_texture = 7;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="P:StardewValley.Item.ItemId" /> instead.</summary>
	[XmlElement("which")]
	public int? obsolete_which;

	[XmlElement("skipHairDraw")]
	public bool skipHairDraw;

	[XmlElement("ignoreHairstyleOffset")]
	public readonly NetBool ignoreHairstyleOffset = new NetBool();

	[XmlElement("hairDrawType")]
	public readonly NetInt hairDrawType = new NetInt();

	[XmlElement("isPrismatic")]
	public readonly NetBool isPrismatic = new NetBool(value: false);

	[XmlIgnore]
	protected int _isMask = -1;

	[XmlElement("enchantments")]
	public List<BaseEnchantment> enchantments = new List<BaseEnchantment>();

	[XmlElement("previousEnchantments")]
	public List<string> previousEnchantments = new List<string>();

	[XmlIgnore]
	public string displayName;

	[XmlIgnore]
	public string description;

	/// <inheritdoc />
	public override string TypeDefinitionId { get; } = "(H)";

	[XmlIgnore]
	public bool isMask
	{
		get
		{
			if (this._isMask == -1)
			{
				if (this.Name.Contains("Mask"))
				{
					this._isMask = 1;
				}
				else
				{
					this._isMask = 0;
				}
				if (this.hairDrawType.Value == 2)
				{
					this._isMask = 0;
				}
			}
			return this._isMask == 1;
		}
	}

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

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = this.obsolete_which?.ToString() ?? "0";
		this.obsolete_which = null;
	}

	public Hat()
	{
	}

	public Hat(string itemId)
	{
		itemId = base.ValidateUnqualifiedItemId(itemId);
		base.ItemId = itemId;
		this.load(base.ItemId);
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.ignoreHairstyleOffset, "ignoreHairstyleOffset").AddField(this.hairDrawType, "hairDrawType").AddField(this.isPrismatic, "isPrismatic");
		base.itemId.fieldChangeVisibleEvent += delegate
		{
			this.load(base.itemId.Value);
		};
	}

	public void load(string id)
	{
		Dictionary<string, string> hatInfo = DataLoader.Hats(Game1.content);
		if (!hatInfo.TryGetValue(id, out var rawData))
		{
			id = "0";
			rawData = hatInfo[id];
		}
		string[] split = rawData.Split('/');
		this.Name = ArgUtility.Get(split, 0, null, allowBlank: false) ?? ItemRegistry.GetDataOrErrorItem("(H)" + id).InternalName;
		string showFullHair = split[2];
		if (showFullHair == "hide")
		{
			this.hairDrawType.Set(2);
		}
		else if (Convert.ToBoolean(showFullHair))
		{
			this.hairDrawType.Set(0);
		}
		else
		{
			this.hairDrawType.Set(1);
		}
		if (this.skipHairDraw)
		{
			this.skipHairDraw = false;
			this.hairDrawType.Set(0);
		}
		string[] array = ArgUtility.SplitBySpace(ArgUtility.Get(split, 4));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == "Prismatic")
			{
				this.isPrismatic.Value = true;
			}
		}
		this.ignoreHairstyleOffset.Value = Convert.ToBoolean(split[3]);
		base.Category = -95;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		scaleSize *= 0.75f;
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		int spriteIndex = itemData.SpriteIndex;
		Texture2D texture = itemData.GetTexture();
		Rectangle drawnSourceRect = new Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4, 20, 20);
		if (itemData.IsErrorItem)
		{
			drawnSourceRect = itemData.GetSourceRect();
		}
		spriteBatch.Draw(texture, location + new Vector2(32f, 32f), drawnSourceRect, this.isPrismatic.Value ? (Utility.GetPrismaticColor() * transparency) : (color * transparency), 0f, new Vector2(10f, 10f), 4f * scaleSize, SpriteEffects.None, layerDepth);
		this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	public void draw(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction, bool useAnimalTexture = false)
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		int spriteIndex = itemData.SpriteIndex;
		Texture2D texture;
		if (useAnimalTexture)
		{
			string textureName = itemData.GetTextureName();
			if (Game1.content.DoesAssetExist<Texture2D>(textureName + "_animals"))
			{
				textureName += "_animals";
			}
			texture = Game1.content.Load<Texture2D>(textureName);
		}
		else
		{
			texture = itemData.GetTexture();
		}
		switch (direction)
		{
		case 0:
			direction = 3;
			break;
		case 2:
			direction = 0;
			break;
		case 3:
			direction = 2;
			break;
		}
		Rectangle drawnSourceRect = ((!itemData.IsErrorItem) ? new Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4 + direction * 20, 20, 20) : itemData.GetSourceRect());
		spriteBatch.Draw(texture, location + new Vector2(10f, 10f), drawnSourceRect, this.isPrismatic.Value ? (Utility.GetPrismaticColor() * transparency) : (Color.White * transparency), 0f, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
	}

	public override string getDescription()
	{
		if (this.description == null)
		{
			this.loadDisplayFields();
		}
		return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override bool isPlaceable()
	{
		return false;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Hat(base.ItemId);
	}

	private bool loadDisplayFields()
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		if (this.Name != null && this.Name != "Error Item" && itemData.IsErrorItem)
		{
			foreach (KeyValuePair<string, string> kvp in DataLoader.Hats(Game1.content))
			{
				if (kvp.Value.Split('/')[0] == this.Name)
				{
					itemData = ItemRegistry.GetDataOrErrorItem(this.TypeDefinitionId + kvp.Key);
					break;
				}
			}
		}
		this.displayName = itemData.DisplayName;
		this.description = itemData.Description;
		return true;
	}
}
