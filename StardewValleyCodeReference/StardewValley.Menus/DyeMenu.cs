using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class DyeMenu : MenuWithInventory
{
	protected int _timeUntilCraft;

	public List<ClickableTextureComponent> dyePots;

	public ClickableTextureComponent dyeButton;

	public const int DYE_POT_ID_OFFSET = 5000;

	public Texture2D dyeTexture;

	protected Dictionary<Item, int> _highlightDictionary;

	protected List<Vector2> _slotDrawPositions;

	protected int _hoveredPotIndex = -1;

	protected int[] _dyeDropAnimationFrames;

	public const int MILLISECONDS_PER_DROP_FRAME = 50;

	public const int TOTAL_DROP_FRAMES = 10;

	public string[][] validPotColors = new string[6][]
	{
		new string[4] { "color_red", "color_salmon", "color_dark_red", "color_pink" },
		new string[5] { "color_orange", "color_dark_orange", "color_dark_brown", "color_brown", "color_copper" },
		new string[4] { "color_yellow", "color_dark_yellow", "color_gold", "color_sand" },
		new string[5] { "color_green", "color_dark_green", "color_lime", "color_yellow_green", "color_jade" },
		new string[6] { "color_blue", "color_dark_blue", "color_dark_cyan", "color_light_cyan", "color_cyan", "color_aquamarine" },
		new string[6] { "color_purple", "color_dark_purple", "color_dark_pink", "color_pale_violet_red", "color_poppyseed", "color_iridium" }
	};

	protected string displayedDescription = "";

	public List<ClickableTextureComponent> dyedClothesDisplays;

	protected Vector2 _dyedClothesDisplayPosition;

	public DyeMenu()
		: base(null, okButton: true, trashCan: true, 12, 132)
	{
		if (base.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
		{
			base.movePosition(0, -IClickableMenu.spaceToClearTopBorder);
		}
		Game1.playSound("bigSelect");
		base.inventory.highlightMethod = HighlightItems;
		this.dyeTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\dye_bench");
		this.dyedClothesDisplays = new List<ClickableTextureComponent>();
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
		this.GenerateHighlightDictionary();
		this._UpdateDescriptionText();
	}

	protected void _CreateButtons()
	{
		this._slotDrawPositions = base.inventory.GetSlotDrawPositions();
		Dictionary<int, Item> old_items = new Dictionary<int, Item>();
		if (this.dyePots != null)
		{
			for (int i = 0; i < this.dyePots.Count; i++)
			{
				old_items[i] = this.dyePots[i].item;
			}
		}
		this.dyePots = new List<ClickableTextureComponent>();
		for (int j = 0; j < this.validPotColors.Length; j++)
		{
			ClickableTextureComponent dye_pot = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4 + 68 + 18 * j * 4, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 132, 64, 64), this.dyeTexture, new Rectangle(32 + 16 * j, 80, 16, 16), 4f)
			{
				myID = j + 5000,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				item = old_items.GetValueOrDefault(j)
			};
			this.dyePots.Add(dye_pot);
		}
		this._dyeDropAnimationFrames = new int[this.dyePots.Count];
		for (int k = 0; k < this._dyeDropAnimationFrames.Length; k++)
		{
			this._dyeDropAnimationFrames[k] = -1;
		}
		this.dyeButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 448, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 200, 96, 96), this.dyeTexture, new Rectangle(0, 80, 24, 24), 4f)
		{
			myID = 1000,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			item = ((this.dyeButton != null) ? this.dyeButton.item : null)
		};
		List<ClickableComponent> list = base.inventory.inventory;
		if (list != null && list.Count >= 12)
		{
			for (int l = 0; l < 12; l++)
			{
				if (base.inventory.inventory[l] != null)
				{
					base.inventory.inventory[l].upNeighborID = -99998;
				}
			}
		}
		this.dyedClothesDisplays.Clear();
		this._dyedClothesDisplayPosition = new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 692, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232);
		Vector2 dyed_items_position = this._dyedClothesDisplayPosition;
		int drawn_items_count = 0;
		if (Game1.player.CanDyeShirt())
		{
			drawn_items_count++;
		}
		if (Game1.player.CanDyePants())
		{
			drawn_items_count++;
		}
		dyed_items_position.X -= drawn_items_count * 64 / 2;
		if (Game1.player.CanDyeShirt())
		{
			ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle((int)dyed_items_position.X, (int)dyed_items_position.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f);
			component.item = Game1.player.shirtItem.Value;
			dyed_items_position.X += 64f;
			this.dyedClothesDisplays.Add(component);
		}
		if (Game1.player.CanDyePants())
		{
			ClickableTextureComponent component2 = new ClickableTextureComponent(new Rectangle((int)dyed_items_position.X, (int)dyed_items_position.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f);
			component2.item = Game1.player.pantsItem.Value;
			dyed_items_position.X += 64f;
			this.dyedClothesDisplays.Add(component2);
		}
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

	public bool HighlightItems(Item i)
	{
		if (i == null)
		{
			return false;
		}
		if (i != null && !i.canBeTrashed())
		{
			return false;
		}
		if (this._highlightDictionary == null)
		{
			this.GenerateHighlightDictionary();
		}
		if (!this._highlightDictionary.ContainsKey(i))
		{
			this._highlightDictionary = null;
			this.GenerateHighlightDictionary();
		}
		if (this._hoveredPotIndex >= 0)
		{
			return this._hoveredPotIndex == this._highlightDictionary[i];
		}
		if (this._highlightDictionary[i] >= 0)
		{
			return this.dyePots[this._highlightDictionary[i]].item == null;
		}
		return false;
	}

	public void GenerateHighlightDictionary()
	{
		this._highlightDictionary = new Dictionary<Item, int>();
		foreach (Item item in new List<Item>(base.inventory.actualInventory))
		{
			if (item != null)
			{
				this._highlightDictionary[item] = this.GetPotIndex(item);
			}
		}
	}

	private void _DyePotClicked(ClickableTextureComponent dyePot)
	{
		Item oldItem = dyePot.item;
		int index = this.dyePots.IndexOf(dyePot);
		if (index < 0)
		{
			return;
		}
		if (base.heldItem == null || (base.heldItem.canBeTrashed() && this.GetPotIndex(base.heldItem) == index))
		{
			bool removeHeldItem = false;
			if (dyePot.item != null && base.heldItem != null && dyePot.item.canStackWith(base.heldItem))
			{
				base.heldItem.Stack++;
				dyePot.item = null;
				Game1.playSound("quickSlosh");
				return;
			}
			dyePot.item = base.heldItem?.getOne();
			if (base.heldItem != null && base.heldItem.ConsumeStack(1) == null)
			{
				removeHeldItem = true;
			}
			if (base.heldItem != null && removeHeldItem)
			{
				base.heldItem = oldItem;
			}
			else if (base.heldItem != null && oldItem != null)
			{
				Item i = Game1.player.addItemToInventory(base.heldItem);
				if (i != null)
				{
					Game1.createItemDebris(i, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				}
				base.heldItem = oldItem;
			}
			else if (oldItem != null)
			{
				base.heldItem = oldItem;
			}
			else if (base.heldItem != null && oldItem == null && Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				Game1.player.addItemToInventory(base.heldItem);
				base.heldItem = null;
			}
			if (oldItem != dyePot.item)
			{
				this._dyeDropAnimationFrames[index] = 0;
				Game1.playSound("quickSlosh");
				int count = 0;
				for (int j = 0; j < this.dyePots.Count; j++)
				{
					if (this.dyePots[j].item != null)
					{
						count++;
					}
				}
				if (count >= this.dyePots.Count)
				{
					DelayedAction.playSoundAfterDelay("newArtifact", 200);
				}
			}
			this._highlightDictionary = null;
			this.GenerateHighlightDictionary();
		}
		this._UpdateDescriptionText();
	}

	public Color GetColorForPot(int index)
	{
		return index switch
		{
			0 => new Color(220, 0, 0), 
			1 => new Color(255, 128, 0), 
			2 => new Color(255, 230, 0), 
			3 => new Color(10, 143, 0), 
			4 => new Color(46, 105, 203), 
			5 => new Color(115, 41, 181), 
			_ => Color.Black, 
		};
	}

	public int GetPotIndex(Item item)
	{
		for (int i = 0; i < this.validPotColors.Length; i++)
		{
			for (int j = 0; j < this.validPotColors[i].Length; j++)
			{
				if (item is ColoredObject colorObject && colorObject.preservedParentSheetIndex.Value != null && ItemContextTagManager.DoAnyTagsMatch(new List<string> { this.validPotColors[i][j] }, ItemContextTagManager.GetBaseContextTags(colorObject.preservedParentSheetIndex.Value)))
				{
					return i;
				}
				if (item.HasContextTag(this.validPotColors[i][j]))
				{
					return i;
				}
			}
		}
		return -1;
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (key == Keys.Delete)
		{
			if (base.heldItem != null && base.heldItem.canBeTrashed())
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
		base.receiveLeftClick(x, y, base.heldItem != null || !Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift));
		if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && oldHeldItem != base.heldItem && base.heldItem != null)
		{
			foreach (ClickableTextureComponent pot in this.dyePots)
			{
				if (pot.item == null)
				{
					this._DyePotClicked(pot);
				}
				if (base.heldItem == null)
				{
					return;
				}
			}
		}
		if (this.IsBusy())
		{
			return;
		}
		bool wasHeldItem = base.heldItem != null;
		foreach (ClickableTextureComponent pot2 in this.dyePots)
		{
			if (pot2.containsPoint(x, y))
			{
				this._DyePotClicked(pot2);
				if (!wasHeldItem && base.heldItem != null && Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
				{
					base.heldItem = Game1.player.addItemToInventory(base.heldItem);
				}
				return;
			}
		}
		if (this.dyeButton.containsPoint(x, y))
		{
			if (base.heldItem == null && this.CanDye())
			{
				Game1.playSound("glug");
				foreach (ClickableTextureComponent dyePot in this.dyePots)
				{
					if (dyePot.item != null)
					{
						dyePot.item = dyePot.item.ConsumeStack(1);
					}
				}
				Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots);
				this._UpdateDescriptionText();
			}
			else
			{
				Game1.playSound("sell");
			}
		}
		if (base.heldItem != null && !this.isWithinBounds(x, y) && base.heldItem.canBeTrashed())
		{
			Game1.playSound("throwDownITem");
			Game1.createItemDebris(base.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			base.heldItem = null;
		}
	}

	public bool CanDye()
	{
		for (int i = 0; i < this.dyePots.Count; i++)
		{
			if (this.dyePots[i].item == null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsWearingDyeable()
	{
		if (!Game1.player.CanDyeShirt())
		{
			return Game1.player.CanDyePants();
		}
		return true;
	}

	protected void _UpdateDescriptionText()
	{
		if (!DyeMenu.IsWearingDyeable())
		{
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable");
		}
		else if (this.CanDye())
		{
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_CanDye");
		}
		else
		{
			this.displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_Help");
		}
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
		if (x <= this.dyePots[0].bounds.X || x >= this.dyePots.Last().bounds.Right || y <= this.dyePots[0].bounds.Y || y >= this.dyePots[0].bounds.Bottom)
		{
			this._hoveredPotIndex = -1;
		}
		if (this.IsBusy())
		{
			return;
		}
		base.hoveredItem = null;
		base.performHoverAction(x, y);
		base.hoverText = "";
		foreach (ClickableTextureComponent component in this.dyedClothesDisplays)
		{
			if (component.containsPoint(x, y))
			{
				base.hoveredItem = component.item;
			}
		}
		for (int i = 0; i < this.dyePots.Count; i++)
		{
			if (this.dyePots[i].containsPoint(x, y))
			{
				this.dyePots[i].tryHover(x, y, 0f);
				this._hoveredPotIndex = i;
			}
		}
		if (this.CanDye())
		{
			this.dyeButton.tryHover(x, y, 0.2f);
		}
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
		if (this.CanDye())
		{
			this.dyeButton.sourceRect.Y = 180;
			this.dyeButton.sourceRect.X = (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 24;
		}
		else
		{
			this.dyeButton.sourceRect.Y = 80;
			this.dyeButton.sourceRect.X = 0;
		}
		for (int i = 0; i < this.dyePots.Count; i++)
		{
			if (this._dyeDropAnimationFrames[i] >= 0)
			{
				this._dyeDropAnimationFrames[i] += time.ElapsedGameTime.Milliseconds;
				if (this._dyeDropAnimationFrames[i] >= 500)
				{
					this._dyeDropAnimationFrames[i] = -1;
				}
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, 50, 160, 255);
		b.Draw(this.dyeTexture, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder), new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		for (int i = 0; i < this._slotDrawPositions.Count; i++)
		{
			if (i < base.inventory.actualInventory.Count && base.inventory.actualInventory[i] != null && this._highlightDictionary.TryGetValue(base.inventory.actualInventory[i], out var index) && index >= 0)
			{
				Color color = this.GetColorForPot(index);
				if (this._hoveredPotIndex == -1 && this.HighlightItems(base.inventory.actualInventory[i]))
				{
					b.Draw(this.dyeTexture, this._slotDrawPositions[i], new Rectangle(32, 96, 32, 32), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				}
			}
		}
		this.dyeButton.draw(b, Color.White * (this.CanDye() ? 1f : 0.55f), 0.96f);
		this.dyeButton.drawItem(b, 16, 16);
		string make_result_text = Game1.content.LoadString("Strings\\UI:DyePot_WillDye");
		Vector2 dyed_items_position = this._dyedClothesDisplayPosition;
		Utility.drawTextWithColoredShadow(position: new Vector2(dyed_items_position.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)(int)dyed_items_position.Y - Game1.smallFont.MeasureString(make_result_text).Y), b: b, text: make_result_text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
		foreach (ClickableTextureComponent dyedClothesDisplay in this.dyedClothesDisplays)
		{
			dyedClothesDisplay.drawItem(b);
		}
		for (int j = 0; j < this.dyePots.Count; j++)
		{
			this.dyePots[j].drawItem(b, 0, -16);
			if (this._dyeDropAnimationFrames[j] >= 0)
			{
				Color color2 = this.GetColorForPot(j);
				b.Draw(this.dyeTexture, new Vector2(this.dyePots[j].bounds.X, this.dyePots[j].bounds.Y - 12), new Rectangle(this._dyeDropAnimationFrames[j] / 50 * 16, 128, 16, 16), color2, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			}
			this.dyePots[j].draw(b);
		}
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
		Utility.CollectOrDrop(base.heldItem);
		for (int i = 0; i < this.dyePots.Count; i++)
		{
			if (this.dyePots[i].item != null)
			{
				Utility.CollectOrDrop(this.dyePots[i].item);
			}
		}
		base.heldItem = null;
		this.dyeButton.item = null;
	}
}
