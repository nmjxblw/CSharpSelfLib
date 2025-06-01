using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
using xTile.Dimensions;

namespace StardewValley.Menus;

public class PurchaseAnimalsMenu : IClickableMenu
{
	public const int region_okButton = 101;

	public const int region_doneNamingButton = 102;

	public const int region_randomButton = 103;

	public const int region_namingBox = 104;

	public const int region_upArrow = 105;

	public const int region_downArrow = 106;

	public static int menuHeight = 320;

	public static int menuWidth = 384;

	public int clickedAnimalButton = -1;

	public List<ClickableTextureComponent> animalsToPurchase = new List<ClickableTextureComponent>();

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent doneNamingButton;

	public ClickableTextureComponent randomButton;

	public ClickableTextureComponent upArrow;

	public ClickableTextureComponent downArrow;

	public ClickableTextureComponent hovered;

	public ClickableComponent textBoxCC;

	/// <summary>Whether the menu is currently showing the target location (regardless of whether it's the farm), so the player can choose a building to put animals in.</summary>
	public bool onFarm;

	public bool namingAnimal;

	public bool freeze;

	public FarmAnimal animalBeingPurchased;

	public TextBox textBox;

	public TextBoxEvent textBoxEvent;

	public Building newAnimalHome;

	public int priceOfAnimal;

	public bool readOnly;

	/// <summary>The index of the row shown at the top of the shop menu.</summary>
	public int currentScroll;

	/// <summary>The number of shop rows that are off-screen.</summary>
	public int scrollRows;

	/// <summary>The location in which to construct or manage buildings.</summary>
	public GameLocation TargetLocation;

	/// <summary>Construct an instance.</summary>
	/// <param name="stock">The animals available to purchase.</param>
	/// <param name="targetLocation">The location for which to purchase animals, or <c>null</c> for the farm.</param>
	public PurchaseAnimalsMenu(List<Object> stock, GameLocation targetLocation = null)
		: base(Game1.uiViewport.Width / 2 - PurchaseAnimalsMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - PurchaseAnimalsMenu.menuHeight - IClickableMenu.borderWidth * 2) / 4, PurchaseAnimalsMenu.menuWidth + IClickableMenu.borderWidth * 2 + ((PurchaseAnimalsMenu.GetOffScreenRows(stock.Count) > 0) ? 44 : 0), PurchaseAnimalsMenu.menuHeight + IClickableMenu.borderWidth)
	{
		base.height += 64;
		this.TargetLocation = targetLocation ?? Game1.getFarm();
		for (int i = 0; i < stock.Count; i++)
		{
			Texture2D texture;
			Microsoft.Xna.Framework.Rectangle sourceRect;
			if (Game1.farmAnimalData.TryGetValue(stock[i].Name, out var animalData) && animalData.ShopTexture != null)
			{
				texture = Game1.content.Load<Texture2D>(animalData.ShopTexture);
				sourceRect = animalData.ShopSourceRect;
			}
			else if (i >= 9)
			{
				texture = Game1.mouseCursors2;
				sourceRect = new Microsoft.Xna.Framework.Rectangle(128 + i % 3 * 16 * 2, i / 3 * 16, 32, 16);
			}
			else
			{
				texture = Game1.mouseCursors;
				sourceRect = new Microsoft.Xna.Framework.Rectangle(i % 3 * 16 * 2, 448 + i / 3 * 16, 32, 16);
			}
			ClickableTextureComponent animalButton = new ClickableTextureComponent(stock[i].salePrice().ToString() ?? "", new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + i % 3 * 64 * 2, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + i / 3 * 85, 128, 64), null, stock[i].Name, texture, sourceRect, 4f, stock[i].Type == null)
			{
				item = stock[i],
				myID = i,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			this.animalsToPurchase.Add(animalButton);
		}
		this.scrollRows = PurchaseAnimalsMenu.GetOffScreenRows(this.animalsToPurchase.Count);
		if (this.scrollRows < 0)
		{
			this.scrollRows = 0;
		}
		this.RepositionAnimalButtons();
		this.okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 101,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		this.randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + base.width + 51 + 64, Game1.uiViewport.Height / 2, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f)
		{
			myID = 103,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		PurchaseAnimalsMenu.menuHeight = 320;
		PurchaseAnimalsMenu.menuWidth = 384;
		this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
		this.textBox.X = Game1.uiViewport.Width / 2 - 192;
		this.textBox.Y = Game1.uiViewport.Height / 2;
		this.textBox.Width = 256;
		this.textBox.Height = 192;
		this.textBoxEvent = textBoxEnter;
		this.textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X, this.textBox.Y, 192, 48), "")
		{
			myID = 104,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		this.randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X + this.textBox.Width + 64 + 48 - 8, Game1.uiViewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f)
		{
			myID = 103,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		this.doneNamingButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X + this.textBox.Width + 32 + 4, Game1.uiViewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 102,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		int arrowsX = base.xPositionOnScreen + base.width - 64 - 24;
		this.upArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(arrowsX, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16, 44, 48), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(421, 459, 11, 12), 4f)
		{
			myID = 105,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		this.downArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(arrowsX, base.yPositionOnScreen + base.height - 64 - 24, 44, 48), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(421, 472, 11, 12), 4f)
		{
			myID = 106,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		this.doneNamingButton.visible = false;
		this.randomButton.visible = false;
		this.textBoxCC.visible = false;
		if (this.scrollRows <= 0)
		{
			this.upArrow.visible = false;
			this.downArrow.visible = false;
		}
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	/// <summary>Get the number of shop rows that are off-screen.</summary>
	/// <param name="animalsToPurchase">The number of animals available to purchase.</param>
	public static int GetOffScreenRows(int animalsToPurchase)
	{
		return (animalsToPurchase - 1) / 3 + 1 - 3;
	}

	public override bool shouldClampGamePadCursor()
	{
		return this.onFarm;
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public void textBoxEnter(TextBox sender)
	{
		if (!this.namingAnimal)
		{
			return;
		}
		if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is PurchaseAnimalsMenu))
		{
			this.textBox.OnEnterPressed -= this.textBoxEvent;
		}
		else if (sender.Text.Length >= 1)
		{
			if (Utility.areThereAnyOtherAnimalsWithThisName(sender.Text))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11308"));
				return;
			}
			this.textBox.OnEnterPressed -= this.textBoxEvent;
			this.animalBeingPurchased.Name = sender.Text;
			this.animalBeingPurchased.displayName = sender.Text;
			((AnimalHouse)this.newAnimalHome.GetIndoors()).adoptAnimal(this.animalBeingPurchased);
			this.newAnimalHome = null;
			this.namingAnimal = false;
			Game1.player.Money -= this.priceOfAnimal;
			this.setUpForReturnAfterPurchasingAnimal();
		}
	}

	public void setUpForReturnAfterPurchasingAnimal()
	{
		LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
		locationRequest.OnWarp += delegate
		{
			this.onFarm = false;
			Game1.player.viewingLocation.Value = null;
			this.okButton.bounds.X = base.xPositionOnScreen + base.width + 4;
			Game1.displayHUD = true;
			Game1.displayFarmer = true;
			this.freeze = false;
			this.textBox.OnEnterPressed -= this.textBoxEvent;
			this.textBox.Selected = false;
			Game1.viewportFreeze = false;
			this.marnieAnimalPurchaseMessage();
		};
		Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
	}

	public void marnieAnimalPurchaseMessage()
	{
		base.exitThisMenu();
		Game1.player.forceCanMove();
		this.freeze = false;
		Game1.DrawDialogue(Game1.getCharacterFromName("Marnie"), this.animalBeingPurchased.isMale() ? "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311" : "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", this.animalBeingPurchased.displayName);
	}

	public void setUpForAnimalPlacement()
	{
		this.upArrow.visible = false;
		this.downArrow.visible = false;
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.displayFarmer = false;
		Game1.currentLocation = this.TargetLocation;
		Game1.player.viewingLocation.Value = this.TargetLocation.NameOrUniqueName;
		Game1.currentLocation.resetForPlayerEntry();
		Game1.globalFadeToClear();
		this.onFarm = true;
		this.freeze = false;
		this.okButton.bounds.X = Game1.uiViewport.Width - 128;
		this.okButton.bounds.Y = Game1.uiViewport.Height - 128;
		Game1.displayHUD = false;
		Game1.viewportFreeze = true;
		Game1.viewport.Location = new Location(3136, 320);
		Building suggestedBuilding = this.GetSuggestedBuilding(this.animalBeingPurchased);
		if (suggestedBuilding != null)
		{
			Game1.viewport.Location = this.GetTopLeftPixelToCenterBuilding(suggestedBuilding);
		}
		Game1.panScreen(0, 0);
	}

	public void setUpForReturnToShopMenu()
	{
		this.freeze = false;
		if (this.scrollRows > 0)
		{
			this.upArrow.visible = true;
			this.downArrow.visible = true;
		}
		this.doneNamingButton.visible = false;
		this.randomButton.visible = false;
		Game1.displayFarmer = true;
		LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
		locationRequest.OnWarp += delegate
		{
			this.onFarm = false;
			Game1.player.viewingLocation.Value = null;
			this.okButton.bounds.X = base.xPositionOnScreen + base.width + 4;
			this.okButton.bounds.Y = base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth;
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			this.namingAnimal = false;
			this.textBox.OnEnterPressed -= this.textBoxEvent;
			this.textBox.Selected = false;
			if (Game1.options.SnappyMenus)
			{
				this.setCurrentlySnappedComponentTo(this.clickedAnimalButton);
				this.snapCursorToCurrentSnappedComponent();
			}
		};
		Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
	}

	public virtual void Scroll(int offset)
	{
		this.currentScroll += offset;
		if (this.currentScroll < 0)
		{
			this.currentScroll = 0;
		}
		if (this.currentScroll > this.scrollRows)
		{
			this.currentScroll = this.scrollRows;
		}
		this.RepositionAnimalButtons();
	}

	public virtual void RepositionAnimalButtons()
	{
		foreach (ClickableTextureComponent item in this.animalsToPurchase)
		{
			item.visible = false;
		}
		for (int y = 0; y < 3; y++)
		{
			for (int x = 0; x < 3; x++)
			{
				int index = (y + this.currentScroll) * 3 + x;
				if (index >= this.animalsToPurchase.Count || index < 0)
				{
					break;
				}
				ClickableTextureComponent clickableTextureComponent = this.animalsToPurchase[index];
				clickableTextureComponent.bounds.X = base.xPositionOnScreen + IClickableMenu.borderWidth + x * 64 * 2;
				clickableTextureComponent.bounds.Y = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + y * 85;
				clickableTextureComponent.visible = true;
			}
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (Game1.IsFading() || this.freeze)
		{
			return;
		}
		if (this.upArrow.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this.Scroll(-1);
		}
		else if (this.downArrow.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this.Scroll(1);
		}
		if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
		{
			if (this.onFarm)
			{
				this.setUpForReturnToShopMenu();
				Game1.playSound("smallSelect");
			}
			else
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
			}
		}
		if (this.onFarm)
		{
			Vector2 clickTile = new Vector2((int)((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f), (int)((Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f));
			Building selection = this.TargetLocation.getBuildingAt(clickTile);
			if (!this.namingAnimal && selection?.GetIndoors() is AnimalHouse animalHouse && !selection.isUnderConstruction())
			{
				if (this.animalBeingPurchased.CanLiveIn(selection))
				{
					if (animalHouse.isFull())
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11321"));
					}
					else
					{
						this.namingAnimal = true;
						this.doneNamingButton.visible = true;
						this.randomButton.visible = true;
						this.textBoxCC.visible = true;
						this.newAnimalHome = selection;
						FarmAnimalData data = this.animalBeingPurchased.GetAnimalData();
						if (data != null)
						{
							if (data.BabySound != null)
							{
								Game1.playSound(data.BabySound, 1200 + Game1.random.Next(-200, 201));
							}
							else if (data.Sound != null)
							{
								Game1.playSound(data.Sound, 1200 + Game1.random.Next(-200, 201));
							}
						}
						this.textBox.OnEnterPressed += this.textBoxEvent;
						this.textBox.Text = this.animalBeingPurchased.displayName;
						Game1.keyboardDispatcher.Subscriber = this.textBox;
						if (Game1.options.SnappyMenus)
						{
							base.currentlySnappedComponent = base.getComponentWithID(104);
							this.snapCursorToCurrentSnappedComponent();
						}
					}
				}
				else
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", this.animalBeingPurchased.displayType));
				}
			}
			if (this.namingAnimal)
			{
				if (this.doneNamingButton.containsPoint(x, y))
				{
					this.textBoxEnter(this.textBox);
					Game1.playSound("smallSelect");
				}
				else if (this.namingAnimal && this.randomButton.containsPoint(x, y))
				{
					this.animalBeingPurchased.Name = Dialogue.randomName();
					this.animalBeingPurchased.displayName = this.animalBeingPurchased.Name;
					this.textBox.Text = this.animalBeingPurchased.displayName;
					this.randomButton.scale = this.randomButton.baseScale;
					Game1.playSound("drumkit6");
				}
				this.textBox.Update();
			}
			return;
		}
		foreach (ClickableTextureComponent c in this.animalsToPurchase)
		{
			if (this.readOnly || !c.containsPoint(x, y) || (c.item as Object).Type != null)
			{
				continue;
			}
			int price = c.item.salePrice();
			if (Game1.player.Money >= price)
			{
				this.clickedAnimalButton = c.myID;
				Game1.globalFadeToBlack(setUpForAnimalPlacement);
				Game1.playSound("smallSelect");
				this.onFarm = true;
				string animalType = c.hoverText;
				if (Game1.farmAnimalData.TryGetValue(animalType, out var animalData) && animalData.AlternatePurchaseTypes != null)
				{
					foreach (AlternatePurchaseAnimals alternateAnimal in animalData.AlternatePurchaseTypes)
					{
						if (GameStateQuery.CheckConditions(alternateAnimal.Condition))
						{
							animalType = Game1.random.ChooseFrom(alternateAnimal.AnimalIds);
							break;
						}
					}
				}
				this.animalBeingPurchased = new FarmAnimal(animalType, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
				this.priceOfAnimal = price;
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"), 3));
			}
		}
	}

	/// <inheritdoc />
	public override bool overrideSnappyMenuCursorMovementBan()
	{
		if (this.onFarm)
		{
			return !this.namingAnimal;
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if (button == Buttons.B && !Game1.globalFade && this.onFarm && this.namingAnimal)
		{
			this.setUpForReturnToShopMenu();
			Game1.playSound("smallSelect");
		}
	}

	public override void gamePadButtonHeld(Buttons b)
	{
		base.gamePadButtonHeld(b);
		if (((uint)(b - 1) <= 1u || b == Buttons.DPadLeft || b == Buttons.DPadRight) && this.onFarm && !this.namingAnimal)
		{
			GamePadState gamePadState = Game1.input.GetGamePadState();
			MouseState mouseState = Game1.input.GetMouseState();
			int speed = 12 + ((gamePadState.IsButtonDown(Buttons.RightTrigger) || gamePadState.IsButtonDown(Buttons.RightShoulder)) ? 8 : 0);
			Game1.setMousePositionRaw(mouseState.X + b switch
			{
				Buttons.DPadLeft => -speed, 
				Buttons.DPadRight => speed, 
				_ => 0, 
			}, mouseState.Y + b switch
			{
				Buttons.DPadUp => -speed, 
				Buttons.DPadDown => speed, 
				_ => 0, 
			});
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.globalFade || this.freeze)
		{
			return;
		}
		if (!Game1.globalFade && this.onFarm)
		{
			if (!this.namingAnimal)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose() && !Game1.IsFading())
				{
					this.setUpForReturnToShopMenu();
				}
				else if (!Game1.options.SnappyMenus)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
					{
						Game1.panScreen(0, 4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						Game1.panScreen(4, 0);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
					{
						Game1.panScreen(0, -4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						Game1.panScreen(-4, 0);
					}
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				if (!this.textBox.Selected && Game1.options.doesInputListContain(Game1.options.menuButton, key))
				{
					this.setUpForReturnToShopMenu();
					Game1.playSound("smallSelect");
				}
				else if (!this.textBox.Selected || !Game1.options.doesInputListContain(Game1.options.menuButton, key))
				{
					base.receiveKeyPress(key);
				}
			}
		}
		else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.IsFading())
		{
			if (this.readyToClose())
			{
				Game1.player.forceCanMove();
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
			}
		}
		else if (Game1.options.SnappyMenus)
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (!this.onFarm)
		{
			this.upArrow.visible = this.currentScroll > 0;
			this.downArrow.visible = this.currentScroll < this.scrollRows;
		}
		else if (!this.namingAnimal)
		{
			int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
			int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
			if (mouseX - Game1.viewport.X < 64)
			{
				Game1.panScreen(-8, 0);
			}
			else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64)
			{
				Game1.panScreen(8, 0);
			}
			if (mouseY - Game1.viewport.Y < 64)
			{
				Game1.panScreen(0, -8);
			}
			else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
			{
				Game1.panScreen(0, 8);
			}
			Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
			foreach (Keys key in pressedKeys)
			{
				this.receiveKeyPress(key);
			}
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hovered = null;
		if (Game1.IsFading() || this.freeze)
		{
			return;
		}
		this.upArrow.tryHover(x, y);
		this.downArrow.tryHover(x, y);
		if (this.okButton != null)
		{
			if (this.okButton.containsPoint(x, y))
			{
				this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
			}
			else
			{
				this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
			}
		}
		if (this.onFarm)
		{
			if (!this.namingAnimal)
			{
				Vector2 clickTile = new Vector2((int)((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f), (int)((Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f));
				GameLocation f = this.TargetLocation;
				foreach (Building building in f.buildings)
				{
					building.color = Color.White;
				}
				Building selection = f.getBuildingAt(clickTile);
				if (selection?.GetIndoors() is AnimalHouse animalHouse)
				{
					if (this.animalBeingPurchased.CanLiveIn(selection) && !animalHouse.isFull())
					{
						selection.color = Color.LightGreen * 0.8f;
					}
					else
					{
						selection.color = Color.Red * 0.8f;
					}
				}
			}
			if (this.doneNamingButton != null)
			{
				if (this.doneNamingButton.containsPoint(x, y))
				{
					this.doneNamingButton.scale = Math.Min(1.1f, this.doneNamingButton.scale + 0.05f);
				}
				else
				{
					this.doneNamingButton.scale = Math.Max(1f, this.doneNamingButton.scale - 0.05f);
				}
			}
			this.randomButton.tryHover(x, y, 0.5f);
			return;
		}
		foreach (ClickableTextureComponent c in this.animalsToPurchase)
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.05f, 4.1f);
				this.hovered = c;
			}
			else
			{
				c.scale = Math.Max(4f, c.scale - 0.025f);
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!this.onFarm && !Game1.dialogueUp && !Game1.IsFading())
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11354"), base.xPositionOnScreen + 96, base.yPositionOnScreen);
			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
			Game1.dayTimeMoneyBox.drawMoneyBox(b);
			this.upArrow.draw(b);
			this.downArrow.draw(b);
			foreach (ClickableTextureComponent c in this.animalsToPurchase)
			{
				c.draw(b, ((c.item as Object).Type != null) ? (Color.Black * 0.4f) : Color.White, 0.87f);
			}
		}
		else if (!Game1.IsFading() && this.onFarm)
		{
			string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355", this.animalBeingPurchased.displayHouse, this.animalBeingPurchased.displayType);
			SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16);
			if (this.namingAnimal)
			{
				if (!Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				Game1.drawDialogueBox(Game1.uiViewport.Width / 2 - 256, Game1.uiViewport.Height / 2 - 192 - 32, 512, 192, speaker: false, drawOnlyBox: true);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11357"), Game1.dialogueFont, new Vector2(Game1.uiViewport.Width / 2 - 256 + 32 + 8, Game1.uiViewport.Height / 2 - 128 + 8), Game1.textColor);
				this.textBox.Draw(b);
				this.doneNamingButton.draw(b);
				this.randomButton.draw(b);
			}
		}
		if (!Game1.IsFading() && this.okButton != null)
		{
			this.okButton.draw(b);
		}
		if (this.hovered != null)
		{
			if ((this.hovered.item as Object).Type != null)
			{
				IClickableMenu.drawHoverText(b, Game1.parseText((this.hovered.item as Object).Type, Game1.dialogueFont, 320), Game1.dialogueFont);
			}
			else
			{
				string displayName = FarmAnimal.GetDisplayName(this.hovered.hoverText, forShop: true);
				SpriteText.drawStringWithScrollBackground(b, displayName, base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 64, base.yPositionOnScreen + base.height + -32 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "Truffle Pig");
				SpriteText.drawStringWithScrollBackground(b, "$" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.hovered.item.salePrice()), base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128, base.yPositionOnScreen + base.height + 64 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "$99999999g", (Game1.player.Money >= this.hovered.item.salePrice()) ? 1f : 0.5f);
				string description = FarmAnimal.GetShopDescription(this.hovered.hoverText);
				IClickableMenu.drawHoverText(b, Game1.parseText(description, Game1.smallFont, 320), Game1.smallFont, 0, 0, this.hovered.item.salePrice(), displayName);
			}
		}
		Game1.mouseCursorTransparency = (Game1.IsFading() ? 0f : 1f);
		base.drawMouse(b);
	}

	/// <summary>Get a suggested building to preselect when opening the menu.</summary>
	/// <param name="animal">The farm animal being placed.</param>
	/// <returns>Returns a building which has room for the animal, else a building which could accept the animal if it wasn't full, else null.</returns>
	public Building GetSuggestedBuilding(FarmAnimal animal)
	{
		Building bestBuilding = null;
		foreach (Building building in this.TargetLocation.buildings)
		{
			if (this.animalBeingPurchased.CanLiveIn(building))
			{
				bestBuilding = building;
				if (building.GetIndoors() is AnimalHouse animalHouse && !animalHouse.isFull())
				{
					return bestBuilding;
				}
			}
		}
		return bestBuilding;
	}

	/// <summary>Get the pixel position relative to the top-left corner of the map at which to set the viewpoint so a given building is centered on screen.</summary>
	/// <param name="building">The building to center on screen.</param>
	public Location GetTopLeftPixelToCenterBuilding(Building building)
	{
		Vector2 screenPosition = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, building.tilesWide.Value * 64, building.tilesHigh.Value * 64);
		int x = building.tileX.Value * 64 - (int)screenPosition.X;
		int yOrigin = building.tileY.Value * 64 - (int)screenPosition.Y;
		return new Location(x, yOrigin);
	}
}
