using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Menus;

public class AnimalQueryMenu : IClickableMenu
{
	public const int region_okButton = 101;

	public const int region_love = 102;

	public const int region_sellButton = 103;

	public const int region_moveHomeButton = 104;

	public const int region_noButton = 105;

	public const int region_allowReproductionButton = 106;

	public const int region_loveHover = 109;

	public const int region_textBoxCC = 110;

	public new static int width = 384;

	public new static int height = 512;

	public FarmAnimal animal;

	public TextBox textBox;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent love;

	public ClickableTextureComponent sellButton;

	public ClickableTextureComponent moveHomeButton;

	public ClickableTextureComponent yesButton;

	public ClickableTextureComponent noButton;

	public ClickableTextureComponent allowReproductionButton;

	public ClickableComponent loveHover;

	public ClickableComponent textBoxCC;

	public double loveLevel;

	public bool confirmingSell;

	public bool movingAnimal;

	public string hoverText = "";

	public string parentName;

	public AnimalQueryMenu(FarmAnimal animal)
		: base(Game1.uiViewport.Width / 2 - AnimalQueryMenu.width / 2, Game1.uiViewport.Height / 2 - AnimalQueryMenu.height / 2, AnimalQueryMenu.width, AnimalQueryMenu.height)
	{
		Game1.player.Halt();
		Game1.player.faceGeneralDirection(animal.Position, 0, opposite: false, useTileCalculations: false);
		AnimalQueryMenu.width = 384;
		if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
		{
			AnimalQueryMenu.width += 32;
		}
		AnimalQueryMenu.height = 512;
		this.animal = animal;
		this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
		this.textBox.X = Game1.uiViewport.Width / 2 - 128 - 12;
		this.textBox.Y = base.yPositionOnScreen - 4 + 128;
		this.textBox.Width = 256;
		this.textBox.Height = 192;
		this.textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(this.textBox.X, this.textBox.Y, this.textBox.Width, 64), "")
		{
			myID = 110,
			downNeighborID = 104
		};
		this.textBox.Text = animal.displayName;
		Game1.keyboardDispatcher.Subscriber = this.textBox;
		this.textBox.Selected = false;
		if (animal.parentId.Value != -1)
		{
			FarmAnimal parent = Utility.getAnimal(animal.parentId.Value);
			if (parent != null)
			{
				this.parentName = parent.displayName;
			}
		}
		animal.makeSound();
		this.okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + AnimalQueryMenu.width + 4, base.yPositionOnScreen + AnimalQueryMenu.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101,
			upNeighborID = -99998
		};
		this.sellButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + AnimalQueryMenu.width + 4, base.yPositionOnScreen + AnimalQueryMenu.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, 384, 16, 16), 4f)
		{
			myID = 103,
			downNeighborID = -99998,
			upNeighborID = 104
		};
		this.moveHomeButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + AnimalQueryMenu.width + 4, base.yPositionOnScreen + AnimalQueryMenu.height - 256 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(16, 384, 16, 16), 4f)
		{
			myID = 104,
			downNeighborID = 103,
			upNeighborID = 110
		};
		if (!animal.isBaby() && animal.CanHavePregnancy())
		{
			this.allowReproductionButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + AnimalQueryMenu.width + 16, base.yPositionOnScreen + AnimalQueryMenu.height - 128 - IClickableMenu.borderWidth + 8, 36, 36), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(animal.allowReproduction.Value ? 128 : 137, 393, 9, 9), 4f)
			{
				myID = 106,
				downNeighborID = 101,
				upNeighborID = 103
			};
		}
		this.love = new ClickableTextureComponent(Math.Round((double)animal.friendshipTowardFarmer.Value, 0) / 10.0 + "<", new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32 + 16, base.yPositionOnScreen - 32 + IClickableMenu.spaceToClearTopBorder + 256 - 32, AnimalQueryMenu.width - 128, 64), null, "Friendship", Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(172, 512, 16, 16), 4f)
		{
			myID = 102
		};
		this.loveHover = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 192 - 32, AnimalQueryMenu.width, 64), "Friendship")
		{
			myID = 109
		};
		if (animal.homeInterior == null)
		{
			Utility.fixAllAnimals();
		}
		this.loveLevel = (float)animal.friendshipTowardFarmer.Value / 1000f;
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override bool shouldClampGamePadCursor()
	{
		return this.movingAnimal;
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(101);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.globalFade)
		{
			return;
		}
		if (Game1.options.menuButton.Contains(new InputButton(key)) && (this.textBox == null || !this.textBox.Selected))
		{
			Game1.playSound("smallSelect");
			if (this.readyToClose())
			{
				Game1.exitActiveMenu();
				if (this.textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(this.textBox.Text))
				{
					this.animal.displayName = this.textBox.Text;
					this.animal.Name = this.textBox.Text;
				}
			}
			else if (this.movingAnimal)
			{
				Game1.globalFadeToBlack(prepareForReturnFromPlacement);
			}
		}
		else if (Game1.options.SnappyMenus && (!Game1.options.menuButton.Contains(new InputButton(key)) || this.textBox == null || !this.textBox.Selected))
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.movingAnimal)
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

	public void finishedPlacingAnimal()
	{
		Game1.exitActiveMenu();
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.currentLocation = Game1.player.currentLocation;
		Game1.currentLocation.resetForPlayerEntry();
		Game1.globalFadeToClear();
		Game1.displayHUD = true;
		Game1.viewportFreeze = false;
		Game1.displayFarmer = true;
		Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_HomeChanged")));
		Game1.player.viewingLocation.Value = null;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (Game1.globalFade)
		{
			return;
		}
		if (this.movingAnimal)
		{
			if (this.okButton != null && this.okButton.containsPoint(x, y))
			{
				Game1.globalFadeToBlack(prepareForReturnFromPlacement);
				Game1.playSound("smallSelect");
			}
			Vector2 clickTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
			Farm f = Game1.getFarm();
			Building selection = f.getBuildingAt(clickTile);
			if (selection == null)
			{
				return;
			}
			if (this.animal.CanLiveIn(selection))
			{
				AnimalHouse selectedHome = (AnimalHouse)selection.GetIndoors();
				if (selectedHome.isFull())
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_BuildingFull"));
					return;
				}
				if (selection.Equals(this.animal.home))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_AlreadyHome"));
					return;
				}
				AnimalHouse oldHome = (AnimalHouse)this.animal.homeInterior;
				if (oldHome.animals.Remove(this.animal.myID.Value) || f.animals.Remove(this.animal.myID.Value))
				{
					oldHome.animalsThatLiveHere.Remove(this.animal.myID.Value);
					selectedHome.adoptAnimal(this.animal);
				}
				this.animal.makeSound();
				Game1.globalFadeToBlack(finishedPlacingAnimal);
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_CantLiveThere", this.animal.shortDisplayType()));
			}
			return;
		}
		if (this.confirmingSell)
		{
			if (this.yesButton.containsPoint(x, y))
			{
				Game1.player.Money += this.animal.getSellPrice();
				((AnimalHouse)this.animal.homeInterior).animalsThatLiveHere.Remove(this.animal.myID.Value);
				this.animal.health.Value = -1;
				if (this.animal.foundGrass != null && FarmAnimal.reservedGrass.Contains(this.animal.foundGrass))
				{
					FarmAnimal.reservedGrass.Remove(this.animal.foundGrass);
				}
				int numClouds = this.animal.Sprite.getWidth() / 2;
				for (int i = 0; i < numClouds; i++)
				{
					int nonRedness = Game1.random.Next(25, 200);
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, this.animal.Position + new Vector2(Game1.random.Next(-32, this.animal.Sprite.getWidth() * 3), Game1.random.Next(-32, this.animal.GetBoundingBox().Height * 3)), new Color(255 - nonRedness, 255, 255 - nonRedness), 8, flipped: false, Game1.random.NextBool() ? 50 : Game1.random.Next(30, 200), 0, 64, -1f, 64, (!Game1.random.NextBool()) ? Game1.random.Next(0, 600) : 0)
					{
						scale = (float)Game1.random.Next(2, 5) * 0.25f,
						alpha = (float)Game1.random.Next(2, 5) * 0.25f,
						motion = new Vector2(0f, (float)(0.0 - Game1.random.NextDouble()))
					});
				}
				Game1.playSound("newRecipe");
				Game1.playSound("money");
				Game1.exitActiveMenu();
			}
			else if (this.noButton.containsPoint(x, y))
			{
				this.confirmingSell = false;
				Game1.playSound("smallSelect");
				if (Game1.options.SnappyMenus)
				{
					base.currentlySnappedComponent = base.getComponentWithID(103);
					this.snapCursorToCurrentSnappedComponent();
				}
			}
			return;
		}
		if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
		{
			Game1.exitActiveMenu();
			if (this.textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(this.textBox.Text))
			{
				this.animal.displayName = this.textBox.Text;
				this.animal.Name = this.textBox.Text;
			}
			Game1.playSound("smallSelect");
		}
		if (this.sellButton.containsPoint(x, y))
		{
			this.confirmingSell = true;
			this.yesButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.Width / 2 - 64 - 4, Game1.uiViewport.Height / 2 - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 111,
				rightNeighborID = 105
			};
			this.noButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(Game1.uiViewport.Width / 2 + 4, Game1.uiViewport.Height / 2 - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 105,
				leftNeighborID = 111
			};
			Game1.playSound("smallSelect");
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				base.currentlySnappedComponent = this.noButton;
				this.snapCursorToCurrentSnappedComponent();
			}
			return;
		}
		if (this.moveHomeButton.containsPoint(x, y))
		{
			Game1.playSound("smallSelect");
			Game1.globalFadeToBlack(prepareForAnimalPlacement);
		}
		if (this.allowReproductionButton != null && this.allowReproductionButton.containsPoint(x, y))
		{
			Game1.playSound("drumkit6");
			this.animal.allowReproduction.Value = !this.animal.allowReproduction.Value;
			if (this.animal.allowReproduction.Value)
			{
				this.allowReproductionButton.sourceRect.X = 128;
			}
			else
			{
				this.allowReproductionButton.sourceRect.X = 137;
			}
		}
		this.textBox.Update();
	}

	/// <inheritdoc />
	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return this.movingAnimal;
	}

	public void prepareForAnimalPlacement()
	{
		this.movingAnimal = true;
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.currentLocation = Game1.getFarm();
		Game1.player.viewingLocation.Value = Game1.currentLocation.NameOrUniqueName;
		Game1.globalFadeToClear();
		this.okButton.bounds.X = Game1.uiViewport.Width - 128;
		this.okButton.bounds.Y = Game1.uiViewport.Height - 128;
		Game1.displayHUD = false;
		Game1.viewportFreeze = true;
		Game1.viewport.Location = new Location(3136, 320);
		Game1.panScreen(0, 0);
		Game1.currentLocation.resetForPlayerEntry();
		Game1.displayFarmer = false;
	}

	public void prepareForReturnFromPlacement()
	{
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.currentLocation = Game1.player.currentLocation;
		Game1.currentLocation.resetForPlayerEntry();
		Game1.globalFadeToClear();
		this.okButton.bounds.X = base.xPositionOnScreen + AnimalQueryMenu.width + 4;
		this.okButton.bounds.Y = base.yPositionOnScreen + AnimalQueryMenu.height - 64 - IClickableMenu.borderWidth;
		Game1.displayHUD = true;
		Game1.viewportFreeze = false;
		Game1.displayFarmer = true;
		this.movingAnimal = false;
		Game1.player.viewingLocation.Value = null;
	}

	public override bool readyToClose()
	{
		this.textBox.Selected = false;
		if (base.readyToClose() && !this.movingAnimal)
		{
			return !Game1.globalFade;
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (Game1.globalFade)
		{
			return;
		}
		if (this.readyToClose())
		{
			Game1.exitActiveMenu();
			if (this.textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(this.textBox.Text))
			{
				this.animal.displayName = this.textBox.Text;
				this.animal.Name = this.textBox.Text;
			}
			Game1.playSound("smallSelect");
		}
		else if (this.movingAnimal)
		{
			Game1.globalFadeToBlack(prepareForReturnFromPlacement);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		if (this.movingAnimal)
		{
			Vector2 clickTile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
			Farm f = Game1.getFarm();
			foreach (Building building in f.buildings)
			{
				building.color = Color.White;
			}
			Building selection = f.getBuildingAt(clickTile);
			if (selection != null)
			{
				if (this.animal.CanLiveIn(selection) && !((AnimalHouse)selection.GetIndoors()).isFull() && !selection.Equals(this.animal.home))
				{
					selection.color = Color.LightGreen * 0.8f;
				}
				else
				{
					selection.color = Color.Red * 0.8f;
				}
			}
		}
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
		if (this.sellButton != null)
		{
			if (this.sellButton.containsPoint(x, y))
			{
				this.sellButton.scale = Math.Min(4.1f, this.sellButton.scale + 0.05f);
				this.hoverText = Game1.content.LoadString("Strings\\UI:AnimalQuery_Sell", this.animal.getSellPrice());
			}
			else
			{
				this.sellButton.scale = Math.Max(4f, this.sellButton.scale - 0.05f);
			}
		}
		if (this.moveHomeButton != null)
		{
			if (this.moveHomeButton.containsPoint(x, y))
			{
				this.moveHomeButton.scale = Math.Min(4.1f, this.moveHomeButton.scale + 0.05f);
				this.hoverText = Game1.content.LoadString("Strings\\UI:AnimalQuery_Move");
			}
			else
			{
				this.moveHomeButton.scale = Math.Max(4f, this.moveHomeButton.scale - 0.05f);
			}
		}
		if (this.allowReproductionButton != null)
		{
			if (this.allowReproductionButton.containsPoint(x, y))
			{
				this.allowReproductionButton.scale = Math.Min(4.1f, this.allowReproductionButton.scale + 0.05f);
				this.hoverText = Game1.content.LoadString("Strings\\UI:AnimalQuery_AllowReproduction");
			}
			else
			{
				this.allowReproductionButton.scale = Math.Max(4f, this.allowReproductionButton.scale - 0.05f);
			}
		}
		if (this.yesButton != null)
		{
			if (this.yesButton.containsPoint(x, y))
			{
				this.yesButton.scale = Math.Min(1.1f, this.yesButton.scale + 0.05f);
			}
			else
			{
				this.yesButton.scale = Math.Max(1f, this.yesButton.scale - 0.05f);
			}
		}
		if (this.noButton != null)
		{
			if (this.noButton.containsPoint(x, y))
			{
				this.noButton.scale = Math.Min(1.1f, this.noButton.scale + 0.05f);
			}
			else
			{
				this.noButton.scale = Math.Max(1f, this.noButton.scale - 0.05f);
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!this.movingAnimal && !Game1.globalFade)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen + 128, AnimalQueryMenu.width, AnimalQueryMenu.height - 128, speaker: false, drawOnlyBox: true);
			this.textBox.Draw(b);
			int age = (this.animal.GetDaysOwned() + 1) / 28 + 1;
			string ageText = ((age <= 1) ? Game1.content.LoadString("Strings\\UI:AnimalQuery_Age1") : Game1.content.LoadString("Strings\\UI:AnimalQuery_AgeN", age));
			if (this.animal.isBaby())
			{
				ageText += Game1.content.LoadString("Strings\\UI:AnimalQuery_AgeBaby");
			}
			Utility.drawTextWithShadow(b, ageText, Game1.smallFont, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128), Game1.textColor);
			int yOffset = 0;
			if (this.parentName != null)
			{
				yOffset = 21;
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AnimalQuery_Parent", this.parentName), Game1.smallFont, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, 32 + base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128), Game1.textColor);
			}
			int halfHeart = (int)((this.loveLevel * 1000.0 % 200.0 >= 100.0) ? (this.loveLevel * 1000.0 / 200.0) : (-100.0));
			for (int i = 0; i < 5; i++)
			{
				b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 96 + 32 * i, yOffset + base.yPositionOnScreen - 32 + 320), new Microsoft.Xna.Framework.Rectangle(211 + ((this.loveLevel * 1000.0 <= (double)((i + 1) * 195)) ? 7 : 0), 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				if (halfHeart == i)
				{
					b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 96 + 32 * i, yOffset + base.yPositionOnScreen - 32 + 320), new Microsoft.Xna.Framework.Rectangle(211, 428, 4, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.891f);
				}
			}
			Utility.drawTextWithShadow(b, Game1.parseText(this.animal.getMoodMessage(), Game1.smallFont, AnimalQueryMenu.width - IClickableMenu.spaceToClearSideBorder * 2 - 64), Game1.smallFont, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, yOffset + base.yPositionOnScreen + 384 - 64 + 4), Game1.textColor);
			this.okButton.draw(b);
			this.sellButton.draw(b);
			this.moveHomeButton.draw(b);
			this.allowReproductionButton?.draw(b);
			if (this.animal != null && this.animal.hasEatenAnimalCracker.Value && Game1.objectSpriteSheet_2 != null)
			{
				Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2((float)(base.xPositionOnScreen + AnimalQueryMenu.width) - 105.6f, (float)base.yPositionOnScreen + 224f), new Microsoft.Xna.Framework.Rectangle(16, 240, 16, 16), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.89f);
			}
			if (this.confirmingSell)
			{
				if (!Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				Game1.drawDialogueBox(Game1.uiViewport.Width / 2 - 160, Game1.uiViewport.Height / 2 - 192, 320, 256, speaker: false, drawOnlyBox: true);
				string confirmText = Game1.content.LoadString("Strings\\UI:AnimalQuery_ConfirmSell");
				b.DrawString(Game1.dialogueFont, confirmText, new Vector2((float)(Game1.uiViewport.Width / 2) - Game1.dialogueFont.MeasureString(confirmText).X / 2f, Game1.uiViewport.Height / 2 - 96 + 8), Game1.textColor);
				this.yesButton.draw(b);
				this.noButton.draw(b);
			}
			else
			{
				string text = this.hoverText;
				if (text != null && text.Length > 0)
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
				}
			}
		}
		else if (!Game1.globalFade)
		{
			string s = Game1.content.LoadString("Strings\\UI:AnimalQuery_ChooseBuilding", this.animal.displayHouse, this.animal.displayType);
			Game1.drawDialogueBox(32, -64, (int)Game1.dialogueFont.MeasureString(s).X + IClickableMenu.borderWidth * 2 + 16, 128 + IClickableMenu.borderWidth * 2, speaker: false, drawOnlyBox: true);
			b.DrawString(Game1.dialogueFont, s, new Vector2(32 + IClickableMenu.spaceToClearSideBorder * 2 + 8, 44f), Game1.textColor);
			this.okButton.draw(b);
		}
		base.drawMouse(b);
	}
}
