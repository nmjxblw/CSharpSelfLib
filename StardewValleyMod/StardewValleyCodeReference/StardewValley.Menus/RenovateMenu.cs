using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Menus;

public class RenovateMenu : IClickableMenu
{
	public const int region_okButton = 101;

	public const int region_randomButton = 103;

	public static int menuHeight = 320;

	public static int menuWidth = 448;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent hovered;

	private bool freeze;

	protected HouseRenovation _renovation;

	protected string _oldLocation;

	protected Point _oldPosition;

	protected int _selectedIndex = -1;

	protected int _animatingIndex = -1;

	protected int _buildAnimationTimer;

	protected int _buildAnimationCount;

	public RenovateMenu(HouseRenovation renovation)
		: base(Game1.uiViewport.Width / 2 - RenovateMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - RenovateMenu.menuHeight - IClickableMenu.borderWidth * 2) / 4, RenovateMenu.menuWidth + IClickableMenu.borderWidth * 2, RenovateMenu.menuHeight + IClickableMenu.borderWidth)
	{
		base.height += 64;
		this.okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 101,
			upNeighborID = 103,
			leftNeighborID = 103
		};
		this._renovation = renovation;
		RenovateMenu.menuHeight = 320;
		RenovateMenu.menuWidth = 448;
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
		this.SetupForRenovationPlacement();
	}

	public override bool shouldClampGamePadCursor()
	{
		return true;
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public void SetupForReturn()
	{
		this.freeze = true;
		LocationRequest locationRequest = Game1.getLocationRequest(this._oldLocation);
		locationRequest.OnWarp += delegate
		{
			Game1.player.viewingLocation.Value = null;
			Game1.displayHUD = true;
			Game1.displayFarmer = true;
			this.freeze = false;
			Game1.viewportFreeze = false;
			this.FinalizeReturn();
		};
		Game1.warpFarmer(locationRequest, this._oldPosition.X, this._oldPosition.Y, Game1.player.FacingDirection);
	}

	public void FinalizeReturn()
	{
		base.exitThisMenu(playSound: false);
		Game1.player.forceCanMove();
		this.freeze = false;
	}

	public void SetupForRenovationPlacement()
	{
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.displayFarmer = false;
		this._oldLocation = Game1.currentLocation.NameOrUniqueName;
		this._oldPosition = Game1.player.TilePoint;
		Game1.currentLocation = this._renovation.location;
		Game1.player.viewingLocation.Value = this._renovation.location.NameOrUniqueName;
		Game1.currentLocation.resetForPlayerEntry();
		Game1.globalFadeToClear();
		this.freeze = false;
		this.okButton.bounds.X = Game1.uiViewport.Width - 128;
		this.okButton.bounds.Y = Game1.uiViewport.Height - 128;
		Game1.displayHUD = false;
		Game1.viewportFreeze = true;
		Vector2 center = default(Vector2);
		int count = 0;
		foreach (List<Microsoft.Xna.Framework.Rectangle> renovationBound in this._renovation.renovationBounds)
		{
			foreach (Microsoft.Xna.Framework.Rectangle rectangle in renovationBound)
			{
				center.X += rectangle.Center.X;
				center.Y += rectangle.Center.Y;
				count++;
			}
		}
		if (count > 0)
		{
			center.X = (int)Math.Round(center.X / (float)count);
			center.Y = (int)Math.Round(center.Y / (float)count);
		}
		Game1.viewport.Location = new Location((int)((center.X + 0.5f) * 64f) - Game1.viewport.Width / 2, (int)((center.Y + 0.5f) * 64f) - Game1.viewport.Height / 2);
		Game1.panScreen(0, 0);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (Game1.globalFade || this.freeze)
		{
			return;
		}
		if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
		{
			this.SetupForReturn();
			Game1.playSound("smallSelect");
			return;
		}
		Vector2 clickTile = new Vector2((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f);
		for (int i = 0; i < this._renovation.renovationBounds.Count; i++)
		{
			foreach (Microsoft.Xna.Framework.Rectangle item in this._renovation.renovationBounds[i])
			{
				if (item.Contains((int)clickTile.X, (int)clickTile.Y))
				{
					this.CompleteRenovation(i);
					return;
				}
			}
		}
	}

	public virtual void AnimateRenovation()
	{
		if (this._buildAnimationTimer == 0)
		{
			return;
		}
		this._buildAnimationTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
		if (this._buildAnimationTimer > 0)
		{
			return;
		}
		if (this._buildAnimationCount > 0)
		{
			this._buildAnimationCount--;
			if (this._renovation.animationType == HouseRenovation.AnimationType.Destroy)
			{
				this._buildAnimationTimer = 50;
				for (int i = 0; i < 5; i++)
				{
					Microsoft.Xna.Framework.Rectangle rectangle = Game1.random.ChooseFrom(this._renovation.renovationBounds[this._animatingIndex]);
					int x = (int)Utility.RandomFloat((rectangle.Left - 1) * 64, 64 * rectangle.Right);
					int y = (int)Utility.RandomFloat((rectangle.Top - 1) * 64, 64 * rectangle.Bottom);
					this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x, y), flicker: false, Game1.random.NextBool()));
					this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x, y), flicker: false, Game1.random.NextBool()));
					this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(x, y), flipped: false, 0f, Color.White)
					{
						interval = 30f,
						totalNumberOfLoops = 99999,
						animationLength = 4,
						scale = 4f,
						alphaFade = 0.01f
					});
				}
			}
			else
			{
				this._buildAnimationTimer = 500;
				Game1.playSound("axe");
				for (int j = 0; j < 20; j++)
				{
					Microsoft.Xna.Framework.Rectangle rectangle2 = Game1.random.ChooseFrom(this._renovation.renovationBounds[this._animatingIndex]);
					int x2 = (int)Utility.RandomFloat((rectangle2.Left - 1) * 64, 64 * rectangle2.Right);
					int y2 = (int)Utility.RandomFloat((rectangle2.Top - 1) * 64, 64 * rectangle2.Bottom);
					this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90) - 64, 6, 1, new Vector2(x2, y2), flicker: false, Game1.random.NextBool()));
					this._renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90) - 64, 6, 1, new Vector2(x2, y2), flicker: false, Game1.random.NextBool()));
				}
			}
		}
		else
		{
			this._buildAnimationTimer = 0;
			this.SetupForReturn();
		}
	}

	public virtual void CompleteRenovation(int selected_index)
	{
		if (this._renovation.validate != null && !this._renovation.validate(this._renovation, selected_index))
		{
			return;
		}
		if (Game1.player.Money < this._renovation.Price)
		{
			Game1.playSound("cancel");
			return;
		}
		bool isRefund = this._renovation.Price < 0;
		if (!isRefund || Game1.player.mailReceived.Contains("FirstPurchase_" + this._renovation.RoomId))
		{
			if (isRefund)
			{
				Game1.player._money -= this._renovation.Price;
			}
			else
			{
				Game1.player.Money -= this._renovation.Price;
				Game1.player.mailReceived.Add("FirstPurchase_" + this._renovation.RoomId);
			}
		}
		this.freeze = true;
		if (this._renovation.animationType == HouseRenovation.AnimationType.Destroy)
		{
			Game1.playSound("explosion");
			this._buildAnimationCount = 10;
		}
		else
		{
			this._buildAnimationCount = 3;
		}
		this._buildAnimationTimer = -1;
		this._animatingIndex = this._selectedIndex;
		if (this._renovation.onRenovation != null)
		{
			this._renovation.onRenovation(this._renovation, selected_index);
			Game1.player.renovateEvent.Fire(this._renovation.location.NameOrUniqueName);
		}
		this.AnimateRenovation();
	}

	/// <inheritdoc />
	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return true;
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if (button == Buttons.B && !Game1.globalFade)
		{
			this.SetupForReturn();
			Game1.playSound("smallSelect");
		}
	}

	public override bool readyToClose()
	{
		if (this.freeze)
		{
			return false;
		}
		return base.readyToClose();
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.globalFade || this.freeze)
		{
			return;
		}
		if (!Game1.globalFade)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
			{
				this.SetupForReturn();
			}
			else if (!Game1.options.SnappyMenus && !this.freeze)
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
		else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.globalFade)
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
		this.AnimateRenovation();
		int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
		int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
		if (!this.freeze)
		{
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
		}
		Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
		foreach (Keys key in pressedKeys)
		{
			this.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hovered = null;
		if (Game1.globalFade || this.freeze)
		{
			return;
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
		Vector2 clickTile = new Vector2((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f);
		this._selectedIndex = -1;
		for (int i = 0; i < this._renovation.renovationBounds.Count; i++)
		{
			foreach (Microsoft.Xna.Framework.Rectangle item in this._renovation.renovationBounds[i])
			{
				if (item.Contains((int)clickTile.X, (int)clickTile.Y))
				{
					this._selectedIndex = i;
					break;
				}
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.globalFade && !this.freeze)
		{
			Game1.StartWorldDrawInUI(b);
			for (int i = 0; i < this._renovation.renovationBounds.Count; i++)
			{
				foreach (Microsoft.Xna.Framework.Rectangle rectangle in this._renovation.renovationBounds[i])
				{
					for (int x = rectangle.Left; x < rectangle.Right; x++)
					{
						for (int y = rectangle.Top; y < rectangle.Bottom; y++)
						{
							int index = 0;
							if (i == this._selectedIndex)
							{
								index = 1;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f), new Microsoft.Xna.Framework.Rectangle(194 + index * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
				}
			}
			Game1.EndWorldDrawInUI(b);
		}
		if (!Game1.globalFade && !this.freeze)
		{
			string s = this._renovation.placementText;
			SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16);
		}
		if (!Game1.globalFade && !this.freeze && this.okButton != null)
		{
			this.okButton.draw(b);
		}
		Game1.mouseCursorTransparency = 1f;
		base.drawMouse(b);
	}
}
