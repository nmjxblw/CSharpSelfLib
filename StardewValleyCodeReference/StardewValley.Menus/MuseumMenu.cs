using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Menus;

public class MuseumMenu : MenuWithInventory
{
	public const int startingState = 0;

	public const int placingInMuseumState = 1;

	public const int exitingState = 2;

	public int fadeTimer;

	public int state;

	public int menuPositionOffset;

	public bool fadeIntoBlack;

	public bool menuMovingDown;

	public float blackFadeAlpha;

	public SparklingText sparkleText;

	public Vector2 globalLocationOfSparklingArtifact;

	/// <summary>The museum for which the menu was opened.</summary>
	public LibraryMuseum Museum;

	private bool holdingMuseumPiece;

	public bool reOrganizing;

	public MuseumMenu(InventoryMenu.highlightThisItem highlighterMethod)
		: base(highlighterMethod, okButton: true)
	{
		this.fadeTimer = 800;
		this.fadeIntoBlack = true;
		base.movePosition(0, Game1.uiViewport.Height - base.yPositionOnScreen - base.height);
		Game1.player.forceCanMove();
		this.Museum = (Game1.currentLocation as LibraryMuseum) ?? throw new InvalidOperationException("The museum donation menu must be used from within the museum.");
		if (Game1.options.SnappyMenus)
		{
			if (base.okButton != null)
			{
				base.okButton.myID = 106;
			}
			this.populateClickableComponentList();
			base.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}
		Game1.displayHUD = false;
	}

	public override bool shouldClampGamePadCursor()
	{
		return true;
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (this.fadeTimer > 0)
		{
			return;
		}
		if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.menuButton) && this.readyToClose())
		{
			this.state = 2;
			this.fadeTimer = 500;
			this.fadeIntoBlack = true;
		}
		else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.menuButton) && !this.holdingMuseumPiece && this.menuMovingDown)
		{
			if (base.heldItem != null)
			{
				Game1.playSound("bigDeSelect");
				Utility.CollectOrDrop(base.heldItem);
				base.heldItem = null;
			}
			this.ReturnToDonatableItems();
		}
		else if (Game1.options.SnappyMenus && base.heldItem == null && !this.reOrganizing)
		{
			base.receiveKeyPress(key);
		}
		if (!Game1.options.SnappyMenus)
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
		else
		{
			if (base.heldItem == null && !this.reOrganizing)
			{
				return;
			}
			LibraryMuseum museum = this.Museum;
			Vector2 newCursorPositionTile = new Vector2((int)((Utility.ModifyCoordinateFromUIScale(Game1.getMouseX()) + (float)Game1.viewport.X) / 64f), (int)((Utility.ModifyCoordinateFromUIScale(Game1.getMouseY()) + (float)Game1.viewport.Y) / 64f));
			if (!museum.isTileSuitableForMuseumPiece((int)newCursorPositionTile.X, (int)newCursorPositionTile.Y) && (!this.reOrganizing || !LibraryMuseum.HasDonatedArtifactAt(newCursorPositionTile)))
			{
				newCursorPositionTile = museum.getFreeDonationSpot();
				Game1.setMousePosition((int)Utility.ModifyCoordinateForUIScale(newCursorPositionTile.X * 64f - (float)Game1.viewport.X + 32f), (int)Utility.ModifyCoordinateForUIScale(newCursorPositionTile.Y * 64f - (float)Game1.viewport.Y + 32f));
				return;
			}
			if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton))
			{
				newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 0, 21, !this.reOrganizing);
			}
			else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton))
			{
				newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 1, 21, !this.reOrganizing);
			}
			else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton))
			{
				newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 2, 21, !this.reOrganizing);
			}
			else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton))
			{
				newCursorPositionTile = museum.findMuseumPieceLocationInDirection(newCursorPositionTile, 3, 21, !this.reOrganizing);
			}
			if (!Game1.viewport.Contains(new Location((int)(newCursorPositionTile.X * 64f + 32f), Game1.viewport.Y + 1)))
			{
				Game1.panScreen((int)(newCursorPositionTile.X * 64f - (float)Game1.viewport.X), 0);
			}
			else if (!Game1.viewport.Contains(new Location(Game1.viewport.X + 1, (int)(newCursorPositionTile.Y * 64f + 32f))))
			{
				Game1.panScreen(0, (int)(newCursorPositionTile.Y * 64f - (float)Game1.viewport.Y));
			}
			Game1.setMousePosition((int)Utility.ModifyCoordinateForUIScale((int)newCursorPositionTile.X * 64 - Game1.viewport.X + 32), (int)Utility.ModifyCoordinateForUIScale((int)newCursorPositionTile.Y * 64 - Game1.viewport.Y + 32));
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		if ((button == Buttons.DPadUp || button == Buttons.LeftThumbstickUp) && !this.menuMovingDown && Game1.options.SnappyMenus && base.currentlySnappedComponent != null && base.currentlySnappedComponent.myID < 12)
		{
			this.reOrganizing = true;
			this.menuMovingDown = true;
			this.receiveKeyPress(Game1.options.moveUpButton[0].key);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.fadeTimer > 0)
		{
			return;
		}
		if (base.okButton != null && base.okButton.containsPoint(x, y) && this.readyToClose())
		{
			if (this.fadeTimer <= 0)
			{
				Game1.playSound("bigDeSelect");
			}
			this.state = 2;
			this.fadeTimer = 800;
			this.fadeIntoBlack = true;
			return;
		}
		Item oldItem = base.heldItem;
		if (!this.holdingMuseumPiece)
		{
			if (base.heldItem == null)
			{
				int inventoryIndex = base.inventory.getInventoryPositionOfClick(x, y);
				Item inventoryItem = ((inventoryIndex >= 0 && inventoryIndex < base.inventory.actualInventory.Count) ? base.inventory.actualInventory[inventoryIndex] : null);
				if (inventoryItem != null && base.inventory.highlightMethod(inventoryItem))
				{
					base.heldItem = inventoryItem.getOne();
					base.inventory.actualInventory[inventoryIndex] = inventoryItem.ConsumeStack(1);
				}
			}
			else
			{
				base.heldItem = base.inventory.leftClick(x, y, base.heldItem);
			}
		}
		if (oldItem == null && base.heldItem != null && Game1.isAnyGamePadButtonBeingPressed())
		{
			this.receiveGamePadButton(Buttons.DPadUp);
		}
		if (oldItem != null && base.heldItem != null && (y < Game1.viewport.Height - (base.height - (IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192)) || this.menuMovingDown))
		{
			Item item = base.heldItem;
			LibraryMuseum museum = this.Museum;
			int mapXTile = (int)(Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64;
			int mapYTile = (int)(Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64;
			if (museum.isTileSuitableForMuseumPiece(mapXTile, mapYTile) && museum.isItemSuitableForDonation(item))
			{
				int rewardsCount = museum.getRewardsForPlayer(Game1.player).Count;
				museum.museumPieces.Add(new Vector2(mapXTile, mapYTile), item.ItemId);
				Game1.playSound("stoneStep");
				if (museum.getRewardsForPlayer(Game1.player).Count > rewardsCount && !this.holdingMuseumPiece)
				{
					this.sparkleText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:NewReward"), Color.MediumSpringGreen, Color.White);
					Game1.playSound("reward");
					this.globalLocationOfSparklingArtifact = new Vector2((float)(mapXTile * 64 + 32) - this.sparkleText.textWidth / 2f, mapYTile * 64 - 48);
				}
				else
				{
					Game1.playSound("newArtifact");
				}
				Game1.player.completeQuest("24");
				base.heldItem = item.ConsumeStack(1);
				int pieces = museum.museumPieces.Length;
				if (!this.holdingMuseumPiece)
				{
					Game1.stats.checkForArchaeologyAchievements();
					if (pieces == LibraryMuseum.totalArtifacts)
					{
						Game1.multiplayer.globalChatInfoMessage("MuseumComplete", Game1.player.farmName.Value);
					}
					else if (pieces == 40)
					{
						Game1.multiplayer.globalChatInfoMessage("Museum40", Game1.player.farmName.Value);
					}
					else
					{
						Game1.multiplayer.globalChatInfoMessage("donation", Game1.player.name.Value, TokenStringBuilder.ItemNameFor(item));
					}
				}
				this.ReturnToDonatableItems();
			}
		}
		else if (base.heldItem == null && !base.inventory.isWithinBounds(x, y))
		{
			int mapXTile2 = (int)(Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64;
			int mapYTile2 = (int)(Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64;
			Vector2 v = new Vector2(mapXTile2, mapYTile2);
			LibraryMuseum location = this.Museum;
			if (location.museumPieces.TryGetValue(v, out var itemId))
			{
				base.heldItem = ItemRegistry.Create(itemId, 1, 0, allowNull: true);
				location.museumPieces.Remove(v);
				if (base.heldItem != null)
				{
					this.holdingMuseumPiece = !LibraryMuseum.HasDonatedArtifact(base.heldItem.QualifiedItemId);
				}
			}
		}
		if (base.heldItem != null && oldItem == null)
		{
			this.menuMovingDown = true;
			this.reOrganizing = false;
		}
	}

	public virtual void ReturnToDonatableItems()
	{
		this.menuMovingDown = false;
		this.holdingMuseumPiece = false;
		this.reOrganizing = false;
		if (Game1.options.SnappyMenus)
		{
			base.movePosition(0, -this.menuPositionOffset);
			this.menuPositionOffset = 0;
			base.snapCursorToCurrentSnappedComponent();
		}
	}

	public override void emergencyShutDown()
	{
		if (base.heldItem != null && this.holdingMuseumPiece)
		{
			Vector2 tile = this.Museum.getFreeDonationSpot();
			if (this.Museum.museumPieces.TryAdd(tile, base.heldItem.ItemId))
			{
				base.heldItem = null;
				this.holdingMuseumPiece = false;
			}
		}
		base.emergencyShutDown();
	}

	public override bool readyToClose()
	{
		if (!this.holdingMuseumPiece && base.heldItem == null)
		{
			return !this.menuMovingDown;
		}
		return false;
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		if (base.heldItem != null)
		{
			base.heldItem = Game1.player.addItemToInventory(base.heldItem);
			if (base.heldItem != null)
			{
				Game1.createItemDebris(base.heldItem, Game1.player.Position, -1);
				base.heldItem = null;
			}
		}
		Game1.displayHUD = true;
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		Item oldItem = base.heldItem;
		if (this.fadeTimer <= 0)
		{
			base.receiveRightClick(x, y);
		}
		if (base.heldItem != null && oldItem == null)
		{
			this.menuMovingDown = true;
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.sparkleText != null && this.sparkleText.update(time))
		{
			this.sparkleText = null;
		}
		if (this.fadeTimer > 0)
		{
			this.fadeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.fadeIntoBlack)
			{
				this.blackFadeAlpha = 0f + (1500f - (float)this.fadeTimer) / 1500f;
			}
			else
			{
				this.blackFadeAlpha = 1f - (1500f - (float)this.fadeTimer) / 1500f;
			}
			if (this.fadeTimer <= 0)
			{
				switch (this.state)
				{
				case 0:
					this.state = 1;
					Game1.viewportFreeze = true;
					Game1.viewport.Location = new Location(1152, 128);
					Game1.clampViewportToGameMap();
					this.fadeTimer = 800;
					this.fadeIntoBlack = false;
					break;
				case 2:
					Game1.viewportFreeze = false;
					this.fadeIntoBlack = false;
					this.fadeTimer = 800;
					this.state = 3;
					break;
				case 3:
					base.exitThisMenuNoSound();
					break;
				}
			}
		}
		if (this.menuMovingDown && this.menuPositionOffset < base.height / 3)
		{
			this.menuPositionOffset += 8;
			base.movePosition(0, 8);
		}
		else if (!this.menuMovingDown && this.menuPositionOffset > 0)
		{
			this.menuPositionOffset -= 8;
			base.movePosition(0, -8);
		}
		int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
		int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
		if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseX - Game1.viewport.X < 64) || Game1.input.GetGamePadState().ThumbSticks.Right.X < 0f)
		{
			Game1.panScreen(-4, 0);
			if (Game1.input.GetGamePadState().ThumbSticks.Right.X < 0f)
			{
				this.snapCursorToCurrentMuseumSpot();
			}
		}
		else if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64) || Game1.input.GetGamePadState().ThumbSticks.Right.X > 0f)
		{
			Game1.panScreen(4, 0);
			if (Game1.input.GetGamePadState().ThumbSticks.Right.X > 0f)
			{
				this.snapCursorToCurrentMuseumSpot();
			}
		}
		if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseY - Game1.viewport.Y < 64) || Game1.input.GetGamePadState().ThumbSticks.Right.Y > 0f)
		{
			Game1.panScreen(0, -4);
			if (Game1.input.GetGamePadState().ThumbSticks.Right.Y > 0f)
			{
				this.snapCursorToCurrentMuseumSpot();
			}
		}
		else if ((!Game1.options.SnappyMenus && Game1.lastCursorMotionWasMouse && mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64) || Game1.input.GetGamePadState().ThumbSticks.Right.Y < 0f)
		{
			Game1.panScreen(0, 4);
			if (Game1.input.GetGamePadState().ThumbSticks.Right.Y < 0f)
			{
				this.snapCursorToCurrentMuseumSpot();
			}
		}
		Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
		foreach (Keys key in pressedKeys)
		{
			this.receiveKeyPress(key);
		}
	}

	private void snapCursorToCurrentMuseumSpot()
	{
		if (this.menuMovingDown)
		{
			Vector2 newCursorPositionTile = new Vector2((Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64, (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64);
			Game1.setMousePosition((int)newCursorPositionTile.X * 64 - Game1.viewport.X + 32, (int)newCursorPositionTile.Y * 64 - Game1.viewport.Y + 32, ui_scale: false);
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		base.movePosition(0, Game1.viewport.Height - base.yPositionOnScreen - base.height);
		Game1.player.forceCanMove();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if ((this.fadeTimer <= 0 || !this.fadeIntoBlack) && this.state != 3)
		{
			if (base.heldItem != null)
			{
				Game1.StartWorldDrawInUI(b);
				for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 2; y++)
				{
					for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; x++)
					{
						if (this.Museum.isTileSuitableForMuseumPiece(x, y))
						{
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29), Color.LightGreen);
						}
					}
				}
				Game1.EndWorldDrawInUI(b);
			}
			if (!this.holdingMuseumPiece)
			{
				base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
			}
			if (!base.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, base.hoverText, Game1.smallFont);
			}
			base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
			base.drawMouse(b);
			this.sparkleText?.draw(b, Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.viewport, this.globalLocationOfSparklingArtifact)));
		}
		b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * this.blackFadeAlpha);
	}
}
