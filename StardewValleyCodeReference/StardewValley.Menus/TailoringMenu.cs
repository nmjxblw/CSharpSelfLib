using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class TailoringMenu : MenuWithInventory
{
	protected enum CraftState
	{
		MissingIngredients,
		Valid,
		InvalidRecipe,
		NotDyeable
	}

	/// <summary>The slots which can currently accept an item.</summary>
	public class TailorHighlight
	{
		/// <summary>Whether the item can be placed in the left slot.</summary>
		public readonly bool LeftSlot;

		/// <summary>Whether the item can be placed in the right slot.</summary>
		public readonly bool RightSlot;

		/// <summary>Whether the item can be placed in an equipment slot.</summary>
		public readonly bool EquipmentSlot;

		/// <summary>Whether the item can be placed in any of these slots.</summary>
		public readonly bool AnySlot;

		/// <summary>Construct an instance.</summary>
		public TailorHighlight()
		{
		}

		/// <summary>Construct an instance.</summary>
		/// <param name="leftSlot"><inheritdoc cref="F:StardewValley.Menus.TailoringMenu.TailorHighlight.LeftSlot" path="/summary" /></param>
		/// <param name="rightSlot"><inheritdoc cref="F:StardewValley.Menus.TailoringMenu.TailorHighlight.RightSlot" path="/summary" /></param>
		/// <param name="equipmentSlot"><inheritdoc cref="F:StardewValley.Menus.TailoringMenu.TailorHighlight.EquipmentSlot" path="/summary" /></param>
		public TailorHighlight(bool leftSlot, bool rightSlot, bool equipmentSlot)
		{
			this.LeftSlot = leftSlot;
			this.RightSlot = rightSlot;
			this.EquipmentSlot = equipmentSlot;
			this.AnySlot = leftSlot || rightSlot || equipmentSlot;
		}
	}

	protected int _timeUntilCraft;

	public const int region_leftIngredient = 998;

	public const int region_rightIngredient = 997;

	public const int region_startButton = 996;

	public const int region_resultItem = 995;

	public ClickableTextureComponent needleSprite;

	public ClickableTextureComponent presserSprite;

	public ClickableTextureComponent craftResultDisplay;

	public Vector2 needlePosition;

	public Vector2 presserPosition;

	public Vector2 leftIngredientStartSpot;

	public Vector2 leftIngredientEndSpot;

	protected float _rightItemOffset;

	public ClickableTextureComponent leftIngredientSpot;

	public ClickableTextureComponent rightIngredientSpot;

	public ClickableTextureComponent blankLeftIngredientSpot;

	public ClickableTextureComponent blankRightIngredientSpot;

	public ClickableTextureComponent startTailoringButton;

	public const int region_shirt = 108;

	public const int region_pants = 109;

	public const int region_hat = 101;

	public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

	public const int CRAFT_TIME = 1500;

	public Texture2D tailoringTextures;

	public List<TailorItemRecipe> _tailoringRecipes;

	private ICue _sewingSound;

	/// <summary>The slots in which each item can be placed.</summary>
	private readonly Dictionary<Item, TailorHighlight> ItemHighlightCache = new Dictionary<Item, TailorHighlight>();

	protected bool _shouldPrismaticDye;

	protected bool _isDyeCraft;

	protected bool _isMultipleResultCraft;

	protected string displayedDescription = "";

	protected CraftState _craftState;

	public Vector2 questionMarkOffset;

	public TailoringMenu()
		: base(null, okButton: true, trashCan: true, 12, 132)
	{
		Game1.playSound("bigSelect");
		if (base.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
		{
			base.movePosition(0, -IClickableMenu.spaceToClearTopBorder);
		}
		base.inventory.highlightMethod = HighlightItems;
		this.tailoringTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\tailoring");
		this._tailoringRecipes = DataLoader.TailoringRecipes(Game1.temporaryContent);
		this._CreateButtons();
		if (base.trashCan != null)
		{
			base.trashCan.myID = 106;
		}
		if (base.okButton != null)
		{
			base.okButton.leftNeighborID = 11;
		}
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
		this._ValidateCraft();
	}

	protected void _CreateButtons()
	{
		this.leftIngredientSpot = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 192, 96, 96), this.tailoringTextures, new Rectangle(0, 156, 24, 24), 4f)
		{
			myID = 998,
			downNeighborID = -99998,
			leftNeighborID = 109,
			rightNeighborID = 996,
			upNeighborID = 997,
			item = ((this.leftIngredientSpot != null) ? this.leftIngredientSpot.item : null)
		};
		this.leftIngredientStartSpot = new Vector2(this.leftIngredientSpot.bounds.X, this.leftIngredientSpot.bounds.Y);
		this.leftIngredientEndSpot = this.leftIngredientStartSpot + new Vector2(256f, 0f);
		this.needleSprite = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 116, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), this.tailoringTextures, new Rectangle(64, 80, 16, 32), 4f);
		this.presserSprite = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 116, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), this.tailoringTextures, new Rectangle(48, 80, 16, 32), 4f);
		this.needlePosition = new Vector2(this.needleSprite.bounds.X, this.needleSprite.bounds.Y);
		this.presserPosition = new Vector2(this.presserSprite.bounds.X, this.presserSprite.bounds.Y);
		this.rightIngredientSpot = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 400, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8, 96, 96), this.tailoringTextures, new Rectangle(0, 180, 24, 24), 4f)
		{
			myID = 997,
			downNeighborID = 996,
			leftNeighborID = 998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			item = ((this.rightIngredientSpot != null) ? this.rightIngredientSpot.item : null),
			fullyImmutable = true
		};
		this.blankRightIngredientSpot = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 400, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8, 96, 96), this.tailoringTextures, new Rectangle(0, 128, 24, 24), 4f);
		this.blankLeftIngredientSpot = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 192, 96, 96), this.tailoringTextures, new Rectangle(0, 128, 24, 24), 4f);
		this.startTailoringButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 448, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 128, 96, 96), this.tailoringTextures, new Rectangle(24, 80, 24, 24), 4f)
		{
			myID = 996,
			downNeighborID = -99998,
			leftNeighborID = 998,
			rightNeighborID = 995,
			upNeighborID = 997,
			item = ((this.startTailoringButton != null) ? this.startTailoringButton.item : null),
			fullyImmutable = true
		};
		List<ClickableComponent> list = base.inventory.inventory;
		if (list != null && list.Count >= 12)
		{
			for (int i = 0; i < 12; i++)
			{
				if (base.inventory.inventory[i] != null)
				{
					base.inventory.inventory[i].upNeighborID = -99998;
				}
			}
		}
		this.equipmentIcons = new List<ClickableComponent>
		{
			new ClickableComponent(new Rectangle(0, 0, 64, 64), "Hat")
			{
				myID = 101,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborID = -99998
			},
			new ClickableComponent(new Rectangle(0, 0, 64, 64), "Shirt")
			{
				myID = 108,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			},
			new ClickableComponent(new Rectangle(0, 0, 64, 64), "Pants")
			{
				myID = 109,
				upNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998
			}
		};
		for (int j = 0; j < this.equipmentIcons.Count; j++)
		{
			this.equipmentIcons[j].bounds.X = base.xPositionOnScreen - 64 + 9;
			this.equipmentIcons[j].bounds.Y = base.yPositionOnScreen + 192 + j * 64;
		}
		this.craftResultDisplay = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64), this.tailoringTextures, new Rectangle(0, 208, 16, 16), 4f)
		{
			myID = 995,
			downNeighborID = -99998,
			leftNeighborID = 996,
			upNeighborID = 997,
			item = this.craftResultDisplay?.item
		};
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public bool IsBusy()
	{
		return this._timeUntilCraft > 0;
	}

	public override bool readyToClose()
	{
		if (base.readyToClose() && base.heldItem == null)
		{
			return !this.IsBusy();
		}
		return false;
	}

	/// <summary>Get whether an item can be put into one of the empty tailoring slots, or (if all tailoring slots are empty) swapped with an equipment slot.</summary>
	/// <param name="i">The item to check.</param>
	public bool HighlightItems(Item i)
	{
		if (i == null)
		{
			return false;
		}
		if (!this.ItemHighlightCache.ContainsKey(i))
		{
			this.BuildHighlightCache();
		}
		return this.ItemHighlightCache[i].AnySlot;
	}

	public void BuildHighlightCache()
	{
		this.ItemHighlightCache.Clear();
		List<Item> obj = new List<Item>(base.inventory.actualInventory)
		{
			Game1.player.pantsItem.Value,
			Game1.player.shirtItem.Value,
			Game1.player.hat.Value
		};
		Item leftItem = this.leftIngredientSpot.item;
		Item rightItem = this.rightIngredientSpot.item;
		bool leftFree = leftItem == null;
		bool rightFree = rightItem == null;
		foreach (Item item in obj)
		{
			if (item == null)
			{
				continue;
			}
			if ((!leftFree && !rightFree) || !this.IsValidCraftIngredient(item))
			{
				this.ItemHighlightCache[item] = new TailorHighlight();
				continue;
			}
			if (!this.IsValidCraftIngredient(item))
			{
				this.ItemHighlightCache[item] = new TailorHighlight(leftSlot: false, rightSlot: false, item is Hat || item is Clothing);
				continue;
			}
			if (leftFree != rightFree)
			{
				this.ItemHighlightCache[item] = new TailorHighlight(leftFree && this.IsValidCraft(item, rightItem), rightFree && this.IsValidCraft(leftItem, item), equipmentSlot: false);
				continue;
			}
			bool validForLeft = false;
			bool validForRight = false;
			if (item is Boots)
			{
				validForLeft = true;
				validForRight = true;
			}
			else if (item is Clothing clothing && clothing.dyeable.Value)
			{
				validForLeft = true;
			}
			else if (item.HasContextTag("color_prismatic") || TailoringMenu.GetDyeColor(item).HasValue)
			{
				validForRight = true;
			}
			foreach (TailorItemRecipe recipe in this._tailoringRecipes)
			{
				if (validForLeft && validForRight)
				{
					break;
				}
				validForLeft = validForLeft || this.HasRequiredTags(item, recipe.FirstItemTags);
				validForRight = validForRight || this.HasRequiredTags(item, recipe.SecondItemTags);
			}
			this.ItemHighlightCache[item] = new TailorHighlight(validForLeft, validForRight, item is Hat || item is Clothing);
		}
	}

	private void _leftIngredientSpotClicked()
	{
		if (base.heldItem == null || !((!(this.ItemHighlightCache.GetValueOrDefault(base.heldItem)?.LeftSlot)) ?? false))
		{
			Item old_item = this.leftIngredientSpot.item;
			if (base.heldItem == null || this.IsValidCraftIngredient(base.heldItem))
			{
				Game1.playSound("stoneStep");
				this.leftIngredientSpot.item = base.heldItem;
				base.heldItem = old_item;
				this.ItemHighlightCache.Clear();
				this._ValidateCraft();
			}
		}
	}

	public bool IsValidCraftIngredient(Item item)
	{
		if (!item.HasContextTag("item_lucky_purple_shorts"))
		{
			return item.canBeTrashed();
		}
		return true;
	}

	private void _rightIngredientSpotClicked()
	{
		if (base.heldItem == null || !((!(this.ItemHighlightCache.GetValueOrDefault(base.heldItem)?.RightSlot)) ?? false))
		{
			Item old_item = this.rightIngredientSpot.item;
			if (base.heldItem == null || this.IsValidCraftIngredient(base.heldItem))
			{
				Game1.playSound("stoneStep");
				this.rightIngredientSpot.item = base.heldItem;
				base.heldItem = old_item;
				this.ItemHighlightCache.Clear();
				this._ValidateCraft();
			}
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (key == Keys.Delete)
		{
			if (base.heldItem?.canBeTrashed() ?? false)
			{
				Utility.trashItem(base.heldItem);
				base.heldItem = null;
			}
		}
		else
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		Item oldHeldItem = base.heldItem;
		bool num = Game1.player.IsEquippedItem(oldHeldItem);
		base.receiveLeftClick(x, y);
		if (num && base.heldItem != oldHeldItem)
		{
			if (oldHeldItem == Game1.player.hat.Value)
			{
				Game1.player.Equip(null, Game1.player.hat);
				this.ItemHighlightCache.Clear();
			}
			else if (oldHeldItem == Game1.player.shirtItem.Value)
			{
				Game1.player.Equip(null, Game1.player.shirtItem);
				this.ItemHighlightCache.Clear();
			}
			else if (oldHeldItem == Game1.player.pantsItem.Value)
			{
				Game1.player.Equip(null, Game1.player.pantsItem);
				this.ItemHighlightCache.Clear();
			}
		}
		foreach (ClickableComponent c in this.equipmentIcons)
		{
			if (!c.containsPoint(x, y))
			{
				continue;
			}
			switch (c.name)
			{
			case "Hat":
			{
				Item item_to_place3 = Utility.PerformSpecialItemPlaceReplacement(base.heldItem);
				if (base.heldItem == null)
				{
					if (this.HighlightItems(Game1.player.hat.Value))
					{
						base.heldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.hat.Value);
						Game1.playSound("dwop");
						if (!(base.heldItem is Hat))
						{
							Game1.player.Equip(null, Game1.player.hat);
						}
						this.ItemHighlightCache.Clear();
						this._ValidateCraft();
					}
				}
				else if (item_to_place3 is Hat hat)
				{
					Item old_item3 = Game1.player.hat.Value;
					old_item3 = Utility.PerformSpecialItemGrabReplacement(old_item3);
					if (old_item3 == base.heldItem)
					{
						old_item3 = null;
					}
					Game1.player.Equip(hat, Game1.player.hat);
					base.heldItem = old_item3;
					Game1.playSound("grassyStep");
					this.ItemHighlightCache.Clear();
					this._ValidateCraft();
				}
				break;
			}
			case "Shirt":
			{
				Item item_to_place2 = Utility.PerformSpecialItemPlaceReplacement(base.heldItem);
				if (base.heldItem == null)
				{
					if (this.HighlightItems(Game1.player.shirtItem.Value))
					{
						base.heldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.shirtItem.Value);
						Game1.playSound("dwop");
						if (!(base.heldItem is Clothing))
						{
							Game1.player.Equip(null, Game1.player.shirtItem);
						}
						this.ItemHighlightCache.Clear();
						this._ValidateCraft();
					}
				}
				else if (item_to_place2 is Clothing shirt && shirt.clothesType.Value == Clothing.ClothesType.SHIRT)
				{
					Item old_item2 = Game1.player.shirtItem.Value;
					old_item2 = Utility.PerformSpecialItemGrabReplacement(old_item2);
					if (old_item2 == base.heldItem)
					{
						old_item2 = null;
					}
					Game1.player.Equip(shirt, Game1.player.shirtItem);
					base.heldItem = old_item2;
					Game1.playSound("sandyStep");
					this.ItemHighlightCache.Clear();
					this._ValidateCraft();
				}
				break;
			}
			case "Pants":
			{
				Item item_to_place = Utility.PerformSpecialItemPlaceReplacement(base.heldItem);
				if (base.heldItem == null)
				{
					if (this.HighlightItems(Game1.player.pantsItem.Value))
					{
						base.heldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.pantsItem.Value);
						if (!(base.heldItem is Clothing))
						{
							Game1.player.Equip(null, Game1.player.pantsItem);
						}
						Game1.playSound("dwop");
						this.ItemHighlightCache.Clear();
						this._ValidateCraft();
					}
				}
				else if (item_to_place is Clothing pants && pants.clothesType.Value == Clothing.ClothesType.PANTS)
				{
					Item old_item = Game1.player.pantsItem.Value;
					old_item = Utility.PerformSpecialItemGrabReplacement(old_item);
					if (old_item == base.heldItem)
					{
						old_item = null;
					}
					Game1.player.Equip(pants, Game1.player.pantsItem);
					base.heldItem = old_item;
					Game1.playSound("sandyStep");
					this.ItemHighlightCache.Clear();
					this._ValidateCraft();
				}
				break;
			}
			}
			return;
		}
		if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && oldHeldItem != base.heldItem && base.heldItem != null)
		{
			if (base.heldItem.QualifiedItemId == "(O)428" || (base.heldItem is Clothing clothing && clothing.dyeable.Value))
			{
				this._leftIngredientSpotClicked();
			}
			else
			{
				this._rightIngredientSpotClicked();
			}
		}
		if (this.IsBusy())
		{
			return;
		}
		if (this.leftIngredientSpot.containsPoint(x, y))
		{
			this._leftIngredientSpotClicked();
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && base.heldItem != null)
			{
				if (Game1.player.IsEquippedItem(base.heldItem))
				{
					base.heldItem = null;
				}
				else
				{
					base.heldItem = base.inventory.tryToAddItem(base.heldItem, "");
				}
			}
		}
		else if (this.rightIngredientSpot.containsPoint(x, y))
		{
			this._rightIngredientSpotClicked();
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && base.heldItem != null)
			{
				if (Game1.player.IsEquippedItem(base.heldItem))
				{
					base.heldItem = null;
				}
				else
				{
					base.heldItem = base.inventory.tryToAddItem(base.heldItem, "");
				}
			}
		}
		else if (this.startTailoringButton.containsPoint(x, y))
		{
			if (base.heldItem == null)
			{
				bool fail = false;
				if (!this.CanFitCraftedItem())
				{
					Game1.playSound("cancel");
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					this._timeUntilCraft = 0;
					fail = true;
				}
				if (!fail && this.IsValidCraft(this.leftIngredientSpot.item, this.rightIngredientSpot.item))
				{
					Game1.playSound("bigSelect");
					Game1.playSound("sewing_loop", out this._sewingSound);
					this.startTailoringButton.scale = this.startTailoringButton.baseScale;
					this._timeUntilCraft = 1500;
					this._UpdateDescriptionText();
				}
				else
				{
					Game1.playSound("sell");
				}
			}
			else
			{
				Game1.playSound("sell");
			}
		}
		if (base.heldItem == null || this.isWithinBounds(x, y) || !base.heldItem.canBeTrashed())
		{
			return;
		}
		if (Game1.player.IsEquippedItem(base.heldItem))
		{
			if (base.heldItem == Game1.player.hat.Value)
			{
				Game1.player.Equip(null, Game1.player.hat);
			}
			else if (base.heldItem == Game1.player.shirtItem.Value)
			{
				Game1.player.Equip(null, Game1.player.shirtItem);
			}
			else if (base.heldItem == Game1.player.pantsItem.Value)
			{
				Game1.player.Equip(null, Game1.player.pantsItem);
			}
		}
		Game1.playSound("throwDownITem");
		Game1.createItemDebris(base.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
		base.heldItem = null;
	}

	protected void _ValidateCraft()
	{
		Item left_item = this.leftIngredientSpot.item;
		Item right_item = this.rightIngredientSpot.item;
		if (left_item == null || right_item == null)
		{
			this._craftState = CraftState.MissingIngredients;
		}
		else if (left_item is Clothing clothing && !clothing.dyeable.Value)
		{
			this._craftState = CraftState.NotDyeable;
		}
		else if (this.IsValidCraft(left_item, right_item))
		{
			this._craftState = CraftState.Valid;
			bool should_prismatic_dye = this._shouldPrismaticDye;
			Item left_item_clone = left_item.getOne();
			if (this.IsMultipleResultCraft(left_item, right_item))
			{
				this._isMultipleResultCraft = true;
			}
			else
			{
				this._isMultipleResultCraft = false;
			}
			this.craftResultDisplay.item = this.CraftItem(left_item_clone, right_item.getOne());
			if (this.craftResultDisplay.item == left_item_clone)
			{
				this._isDyeCraft = true;
			}
			else
			{
				this._isDyeCraft = false;
			}
			this._shouldPrismaticDye = should_prismatic_dye;
		}
		else
		{
			this._craftState = CraftState.InvalidRecipe;
		}
		this._UpdateDescriptionText();
	}

	protected void _UpdateDescriptionText()
	{
		if (this.IsBusy())
		{
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_Busy");
			return;
		}
		switch (this._craftState)
		{
		case CraftState.NotDyeable:
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_NotDyeable");
			break;
		case CraftState.MissingIngredients:
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_MissingIngredients");
			break;
		case CraftState.Valid:
			this.displayedDescription = ((!this.CanFitCraftedItem()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588") : Game1.content.LoadString("Strings\\UI:Tailor_Valid"));
			break;
		case CraftState.InvalidRecipe:
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_InvalidRecipe");
			break;
		default:
			this.displayedDescription = "";
			break;
		}
	}

	public static Color? GetDyeColor(Item dye_object)
	{
		if (dye_object == null)
		{
			return null;
		}
		if (dye_object.QualifiedItemId == "(O)74")
		{
			return Color.White;
		}
		if (dye_object is ColoredObject coloredObject)
		{
			return coloredObject.color.Value;
		}
		return ItemContextTagManager.GetColorFromTags(dye_object);
	}

	public bool DyeItems(Clothing clothing, Item dye_object, float dye_strength_override = -1f)
	{
		if (dye_object.QualifiedItemId == "(O)74")
		{
			clothing.Dye(Color.White, 1f);
			clothing.isPrismatic.Set(newValue: true);
			return true;
		}
		Color? dye_color = TailoringMenu.GetDyeColor(dye_object);
		if (dye_color.HasValue)
		{
			float dye_strength = 0.25f;
			if (dye_object.HasContextTag("dye_medium"))
			{
				dye_strength = 0.5f;
			}
			if (dye_object.HasContextTag("dye_strong"))
			{
				dye_strength = 1f;
			}
			if (dye_strength_override >= 0f)
			{
				dye_strength = dye_strength_override;
			}
			clothing.Dye(dye_color.Value, dye_strength);
			if (clothing == Game1.player.shirtItem.Value || clothing == Game1.player.pantsItem.Value)
			{
				Game1.player.FarmerRenderer.MarkSpriteDirty();
			}
			return true;
		}
		return false;
	}

	/// <summary>Get the recipe which accepts the given items.</summary>
	/// <param name="leftItem">The item in the left slot (usually cloth).</param>
	/// <param name="rightItem">The item in the right slot.</param>
	/// <returns>Returns the matching recipe, or <c>null</c> if none was found.</returns>
	public TailorItemRecipe GetRecipeForItems(Item leftItem, Item rightItem)
	{
		if (leftItem != null && rightItem != null)
		{
			foreach (TailorItemRecipe recipe in this._tailoringRecipes)
			{
				if (this.HasRequiredTags(leftItem, recipe.FirstItemTags) && this.HasRequiredTags(rightItem, recipe.SecondItemTags))
				{
					return recipe;
				}
			}
		}
		return null;
	}

	/// <summary>Get whether an item matches every given tag.</summary>
	/// <param name="item">The item to check.</param>
	/// <param name="requiredTags">The context tags which must all match the item.</param>
	private bool HasRequiredTags(Item item, List<string> requiredTags)
	{
		if (item != null && requiredTags != null && requiredTags.Count > 0)
		{
			foreach (string tag in requiredTags)
			{
				if (!item.HasContextTag(tag))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool IsValidCraft(Item left_item, Item right_item)
	{
		if (left_item == null || right_item == null)
		{
			return false;
		}
		if (left_item is Boots && right_item is Boots)
		{
			return true;
		}
		if (left_item is Clothing clothing && clothing.dyeable.Value)
		{
			if (right_item.HasContextTag("color_prismatic"))
			{
				return true;
			}
			if (TailoringMenu.GetDyeColor(right_item).HasValue)
			{
				return true;
			}
		}
		return this.GetRecipeForItems(left_item, right_item) != null;
	}

	public bool IsMultipleResultCraft(Item left_item, Item right_item)
	{
		TailorItemRecipe recipeForItems = this.GetRecipeForItems(left_item, right_item);
		if (recipeForItems == null)
		{
			return false;
		}
		return recipeForItems.CraftedItemIds?.Count > 0;
	}

	public Item CraftItem(Item left_item, Item right_item)
	{
		if (left_item == null || right_item == null)
		{
			return null;
		}
		if (left_item is Boots leftBoots && right_item is Boots rightBoots)
		{
			leftBoots.applyStats(rightBoots);
			return leftBoots;
		}
		if (left_item is Clothing leftClothing && leftClothing.dyeable.Value)
		{
			if (right_item.HasContextTag("color_prismatic"))
			{
				this._shouldPrismaticDye = true;
				return leftClothing;
			}
			if (this.DyeItems(leftClothing, right_item))
			{
				return leftClothing;
			}
		}
		TailorItemRecipe recipe = this.GetRecipeForItems(left_item, right_item);
		if (recipe != null)
		{
			string crafted_item_id;
			if (recipe.CraftedItemIdFeminine != null && !Game1.player.IsMale)
			{
				crafted_item_id = recipe.CraftedItemIdFeminine;
			}
			else
			{
				List<string> craftedItemIds = recipe.CraftedItemIds;
				crafted_item_id = ((craftedItemIds == null || craftedItemIds.Count <= 0) ? recipe.CraftedItemId : Game1.random.ChooseFrom(recipe.CraftedItemIds));
			}
			crafted_item_id = TailoringMenu.ConvertLegacyItemId(crafted_item_id);
			Item item = ItemRegistry.Create(crafted_item_id);
			if (item is Clothing craftedClothing)
			{
				this.DyeItems(craftedClothing, right_item, 1f);
			}
			if (item is Object craftedObj && ((left_item is Object leftObj && leftObj.questItem.Value) || (right_item is Object rightObj && rightObj.questItem.Value)))
			{
				craftedObj.questItem.Value = true;
			}
			return item;
		}
		return null;
	}

	/// <summary>Get an item ID for a legacy output from Stardew Valley 1.5.5 and earlier.</summary>
	/// <param name="id">The legacy item ID.</param>
	public static string ConvertLegacyItemId(string id)
	{
		if (!int.TryParse(id, out var legacyId))
		{
			return id;
		}
		if (legacyId < 0)
		{
			return "(O)" + -legacyId;
		}
		if (legacyId >= 2000 && legacyId < 3000)
		{
			return "(H)" + (legacyId - 2000);
		}
		if (legacyId >= 1000)
		{
			return "(S)" + legacyId;
		}
		return "(P)" + legacyId;
	}

	public void SpendRightItem()
	{
		this.rightIngredientSpot.item = this.rightIngredientSpot.item?.ConsumeStack(1);
	}

	public void SpendLeftItem()
	{
		this.leftIngredientSpot.item = this.leftIngredientSpot.item?.ConsumeStack(1);
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (!this.IsBusy())
		{
			base.receiveRightClick(x, y);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (this.IsBusy())
		{
			return;
		}
		base.hoveredItem = null;
		base.performHoverAction(x, y);
		base.hoverText = "";
		for (int i = 0; i < this.equipmentIcons.Count; i++)
		{
			if (this.equipmentIcons[i].containsPoint(x, y))
			{
				switch (this.equipmentIcons[i].name)
				{
				case "Shirt":
					base.hoveredItem = Game1.player.shirtItem.Value;
					break;
				case "Hat":
					base.hoveredItem = Game1.player.hat.Value;
					break;
				case "Pants":
					base.hoveredItem = Game1.player.pantsItem.Value;
					break;
				}
			}
		}
		if (this.craftResultDisplay.visible && this.craftResultDisplay.containsPoint(x, y) && this.craftResultDisplay.item != null)
		{
			if (this._isDyeCraft || Game1.player.HasTailoredThisItem(this.craftResultDisplay.item))
			{
				base.hoveredItem = this.craftResultDisplay.item;
			}
			else
			{
				base.hoverText = Game1.content.LoadString("Strings\\UI:Tailor_MakeResultUnknown");
			}
		}
		if (this.leftIngredientSpot.containsPoint(x, y))
		{
			if (this.leftIngredientSpot.item != null)
			{
				base.hoveredItem = this.leftIngredientSpot.item;
			}
			else
			{
				base.hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Feed");
			}
		}
		if (this.rightIngredientSpot.containsPoint(x, y) && this.rightIngredientSpot.item == null)
		{
			base.hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Spool");
		}
		this.rightIngredientSpot.tryHover(x, y);
		this.leftIngredientSpot.tryHover(x, y);
		if (this._craftState == CraftState.Valid && this.CanFitCraftedItem())
		{
			this.startTailoringButton.tryHover(x, y, 0.33f);
		}
		else
		{
			this.startTailoringButton.tryHover(-999, -999);
		}
	}

	public bool CanFitCraftedItem()
	{
		if (this.craftResultDisplay.item != null && !Utility.canItemBeAddedToThisInventoryList(this.craftResultDisplay.item, base.inventory.actualInventory))
		{
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		int yPositionForInventory = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
		base.inventory = new InventoryMenu(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, playerInventory: false, null, base.inventory.highlightMethod);
		this._CreateButtons();
	}

	public override void emergencyShutDown()
	{
		this._OnCloseMenu();
		base.emergencyShutDown();
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		base.descriptionText = this.displayedDescription;
		this.questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
		this.questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
		bool can_fit_crafted_item = this.CanFitCraftedItem();
		this.startTailoringButton.sourceRect.Y = ((this._craftState == CraftState.Valid && can_fit_crafted_item) ? 104 : 80);
		this.craftResultDisplay.visible = this._craftState == CraftState.Valid && !this.IsBusy() && can_fit_crafted_item;
		if (this._timeUntilCraft > 0)
		{
			this.startTailoringButton.tryHover(this.startTailoringButton.bounds.Center.X, this.startTailoringButton.bounds.Center.Y, 0.33f);
			this.leftIngredientSpot.bounds.X = (int)Utility.Lerp(this.leftIngredientEndSpot.X, this.leftIngredientStartSpot.X, (float)this._timeUntilCraft / 1500f);
			this.leftIngredientSpot.bounds.Y = (int)Utility.Lerp(this.leftIngredientEndSpot.Y, this.leftIngredientStartSpot.Y, (float)this._timeUntilCraft / 1500f);
			this._timeUntilCraft -= time.ElapsedGameTime.Milliseconds;
			this.needleSprite.bounds.Location = new Point((int)this.needlePosition.X, (int)(this.needlePosition.Y - 2f * ((float)this._timeUntilCraft % 25f) / 25f * 4f));
			this.presserSprite.bounds.Location = new Point((int)this.presserPosition.X, (int)(this.presserPosition.Y - 1f * ((float)this._timeUntilCraft % 50f) / 50f * 4f));
			this._rightItemOffset = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 2.0 * Math.PI / 180.0) * 2f;
			if (this._timeUntilCraft > 0)
			{
				return;
			}
			TailorItemRecipe recipe = this.GetRecipeForItems(this.leftIngredientSpot.item, this.rightIngredientSpot.item);
			this._shouldPrismaticDye = false;
			Item crafted_item = this.CraftItem(this.leftIngredientSpot.item, this.rightIngredientSpot.item);
			if (this._sewingSound != null && this._sewingSound.IsPlaying)
			{
				this._sewingSound.Stop(AudioStopOptions.Immediate);
			}
			if (!Utility.canItemBeAddedToThisInventoryList(crafted_item, base.inventory.actualInventory))
			{
				Game1.playSound("cancel");
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
				this._timeUntilCraft = 0;
				return;
			}
			if (this.leftIngredientSpot.item == crafted_item)
			{
				this.leftIngredientSpot.item = null;
			}
			else
			{
				this.SpendLeftItem();
			}
			if ((recipe == null || recipe.SpendRightItem) && (this.readyToClose() || !this._shouldPrismaticDye))
			{
				this.SpendRightItem();
			}
			if (recipe != null)
			{
				Game1.player.MarkItemAsTailored(crafted_item);
			}
			Game1.playSound("coin");
			base.heldItem = crafted_item;
			this._timeUntilCraft = 0;
			this._ValidateCraft();
			if (this._shouldPrismaticDye)
			{
				Item old_held_item = base.heldItem;
				base.heldItem = null;
				if (this.readyToClose())
				{
					base.exitThisMenuNoSound();
					Game1.activeClickableMenu = new CharacterCustomization(crafted_item as Clothing);
					return;
				}
				base.heldItem = old_held_item;
			}
		}
		this._rightItemOffset = 0f;
		this.leftIngredientSpot.bounds.X = (int)this.leftIngredientStartSpot.X;
		this.leftIngredientSpot.bounds.Y = (int)this.leftIngredientStartSpot.Y;
		this.needleSprite.bounds.Location = new Point((int)this.needlePosition.X, (int)this.needlePosition.Y);
		this.presserSprite.bounds.Location = new Point((int)this.presserPosition.X, (int)this.presserPosition.Y);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		b.Draw(this.tailoringTextures, new Vector2((float)base.xPositionOnScreen + 96f, base.yPositionOnScreen - 64), new Rectangle(101, 80, 41, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.87f);
		b.Draw(this.tailoringTextures, new Vector2((float)base.xPositionOnScreen + 352f, base.yPositionOnScreen - 64), new Rectangle(101, 80, 41, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		b.Draw(this.tailoringTextures, new Vector2((float)base.xPositionOnScreen + 608f, base.yPositionOnScreen - 64), new Rectangle(101, 80, 41, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		b.Draw(this.tailoringTextures, new Vector2((float)base.xPositionOnScreen + 256f, base.yPositionOnScreen), new Rectangle(79, 97, 22, 20), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		b.Draw(this.tailoringTextures, new Vector2((float)base.xPositionOnScreen + 512f, base.yPositionOnScreen), new Rectangle(79, 97, 22, 20), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		b.Draw(this.tailoringTextures, new Vector2((float)base.xPositionOnScreen + 32f, base.yPositionOnScreen + 44), new Rectangle(81, 81, 16, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		b.Draw(this.tailoringTextures, new Vector2((float)base.xPositionOnScreen + 768f, base.yPositionOnScreen + 44), new Rectangle(81, 81, 16, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		Game1.DrawBox(base.xPositionOnScreen - 64, base.yPositionOnScreen + 128, 128, 265, new Color(50, 160, 255));
		Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float)(base.xPositionOnScreen - 64) + 9.6f, base.yPositionOnScreen + 128), 0.87f, 4f, 2, Game1.player);
		base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, 50, 160, 255);
		b.Draw(this.tailoringTextures, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder), new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		this.startTailoringButton.draw(b, Color.White, 0.96f);
		this.startTailoringButton.drawItem(b, 16, 16);
		this.presserSprite.draw(b, Color.White, 0.99f);
		this.needleSprite.draw(b, Color.White, 0.97f);
		Point random_shaking = new Point(0, 0);
		if (!this.IsBusy())
		{
			Color color = ((base.heldItem == null || (this.ItemHighlightCache.GetValueOrDefault(base.heldItem)?.LeftSlot ?? false)) ? Color.White : (Color.White * 0.5f));
			if (this.leftIngredientSpot.item != null)
			{
				this.blankLeftIngredientSpot.draw(b, color, 0.87f);
			}
			else
			{
				this.leftIngredientSpot.draw(b, color, 0.87f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200);
			}
		}
		else
		{
			random_shaking.X = Game1.random.Next(-1, 2);
			random_shaking.Y = Game1.random.Next(-1, 2);
		}
		this.leftIngredientSpot.drawItem(b, (4 + random_shaking.X) * 4, (4 + random_shaking.Y) * 4);
		if (this.craftResultDisplay.visible)
		{
			string make_result_text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
			Utility.drawTextWithColoredShadow(position: new Vector2((float)this.craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)this.craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(make_result_text).Y), b: b, text: make_result_text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
			this.craftResultDisplay.draw(b);
			if (this.craftResultDisplay.item != null)
			{
				if (this._isMultipleResultCraft)
				{
					Rectangle question_mark_bounds = this.craftResultDisplay.bounds;
					question_mark_bounds.X += 6;
					question_mark_bounds.Y -= 8 + (int)this.questionMarkOffset.Y;
					b.Draw(this.tailoringTextures, question_mark_bounds, new Rectangle(112, 208, 16, 16), Color.White);
				}
				else if (this._isDyeCraft || Game1.player.HasTailoredThisItem(this.craftResultDisplay.item))
				{
					this.craftResultDisplay.drawItem(b);
				}
				else
				{
					Item item = this.craftResultDisplay.item;
					if (!(item is Hat))
					{
						if (!(item is Clothing clothing))
						{
							if (item is Object { QualifiedItemId: "(O)71" })
							{
								b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle(64, 208, 16, 16), Color.White);
							}
						}
						else
						{
							switch (clothing.clothesType.Value)
							{
							case Clothing.ClothesType.PANTS:
								b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle(64, 208, 16, 16), Color.White);
								break;
							case Clothing.ClothesType.SHIRT:
								b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle(80, 208, 16, 16), Color.White);
								break;
							}
						}
					}
					else
					{
						b.Draw(this.tailoringTextures, this.craftResultDisplay.bounds, new Rectangle(96, 208, 16, 16), Color.White);
					}
					Rectangle question_mark_bounds2 = this.craftResultDisplay.bounds;
					question_mark_bounds2.X += 24;
					question_mark_bounds2.Y += 12 + (int)this.questionMarkOffset.Y;
					b.Draw(this.tailoringTextures, question_mark_bounds2, new Rectangle(112, 208, 16, 16), Color.White);
				}
			}
		}
		foreach (ClickableComponent c in this.equipmentIcons)
		{
			float num2;
			float num;
			float transparency;
			float transparency2;
			switch (c.name)
			{
			case "Hat":
				if (Game1.player.hat.Value != null)
				{
					b.Draw(this.tailoringTextures, c.bounds, new Rectangle(0, 208, 16, 16), Color.White);
					float transparency3 = ((!this.HighlightItems(Game1.player.hat.Value) || Game1.player.hat.Value == base.heldItem || (base.heldItem != null && !(base.heldItem is Hat))) ? 0.5f : 1f);
					Game1.player.hat.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency3, 0.866f, StackDrawType.Hide);
				}
				else
				{
					b.Draw(this.tailoringTextures, c.bounds, new Rectangle(48, 208, 16, 16), Color.White);
				}
				break;
			case "Shirt":
				if (Game1.player.shirtItem.Value != null)
				{
					b.Draw(this.tailoringTextures, c.bounds, new Rectangle(0, 208, 16, 16), Color.White);
					if (!this.HighlightItems(Game1.player.shirtItem.Value) || Game1.player.shirtItem.Value == base.heldItem)
					{
						goto IL_09ee;
					}
					if (base.heldItem != null)
					{
						Clothing obj2 = base.heldItem as Clothing;
						if (obj2 == null || obj2.clothesType.Value != Clothing.ClothesType.SHIRT)
						{
							goto IL_09ee;
						}
					}
					num2 = 1f;
					goto IL_09f3;
				}
				b.Draw(this.tailoringTextures, c.bounds, new Rectangle(32, 208, 16, 16), Color.White);
				break;
			case "Pants":
				{
					if (Game1.player.pantsItem.Value != null)
					{
						b.Draw(this.tailoringTextures, c.bounds, new Rectangle(0, 208, 16, 16), Color.White);
						if (!this.HighlightItems(Game1.player.pantsItem.Value) || Game1.player.pantsItem.Value == base.heldItem)
						{
							goto IL_0b0f;
						}
						if (base.heldItem != null)
						{
							Clothing obj = base.heldItem as Clothing;
							if (obj == null || obj.clothesType.Value != Clothing.ClothesType.PANTS)
							{
								goto IL_0b0f;
							}
						}
						num = 1f;
						goto IL_0b14;
					}
					b.Draw(this.tailoringTextures, c.bounds, new Rectangle(16, 208, 16, 16), Color.White);
					break;
				}
				IL_0b0f:
				num = 0.5f;
				goto IL_0b14;
				IL_0b14:
				transparency = num;
				Game1.player.pantsItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency, 0.866f);
				break;
				IL_09f3:
				transparency2 = num2;
				Game1.player.shirtItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency2, 0.866f);
				break;
				IL_09ee:
				num2 = 0.5f;
				goto IL_09f3;
			}
		}
		if (!this.IsBusy())
		{
			Color color2 = ((base.heldItem == null || (this.ItemHighlightCache.GetValueOrDefault(base.heldItem)?.RightSlot ?? false)) ? Color.White : (Color.White * 0.5f));
			if (this.rightIngredientSpot.item != null)
			{
				this.blankRightIngredientSpot.draw(b, color2, 0.87f);
			}
			else
			{
				this.rightIngredientSpot.draw(b, color2, 0.87f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200);
			}
		}
		this.rightIngredientSpot.drawItem(b, 16, (4 + (int)this._rightItemOffset) * 4);
		if (!base.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, base.hoverText, Game1.smallFont, (base.heldItem != null) ? 32 : 0, (base.heldItem != null) ? 32 : 0);
		}
		else if (base.hoveredItem != null)
		{
			IClickableMenu.drawToolTip(b, base.hoveredItem.getDescription(), base.hoveredItem.DisplayName, base.hoveredItem, base.heldItem != null);
		}
		base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
		if (!Game1.options.hardwareCursor)
		{
			base.drawMouse(b);
		}
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		this._OnCloseMenu();
	}

	protected void _OnCloseMenu()
	{
		if (!Game1.player.IsEquippedItem(base.heldItem))
		{
			Utility.CollectOrDrop(base.heldItem);
		}
		if (!Game1.player.IsEquippedItem(this.leftIngredientSpot.item))
		{
			Utility.CollectOrDrop(this.leftIngredientSpot.item);
		}
		if (!Game1.player.IsEquippedItem(this.rightIngredientSpot.item))
		{
			Utility.CollectOrDrop(this.rightIngredientSpot.item);
		}
		if (!Game1.player.IsEquippedItem(this.startTailoringButton.item))
		{
			Utility.CollectOrDrop(this.startTailoringButton.item);
		}
		base.heldItem = null;
		this.leftIngredientSpot.item = null;
		this.rightIngredientSpot.item = null;
		this.startTailoringButton.item = null;
	}
}
