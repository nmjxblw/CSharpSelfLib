using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;

namespace StardewValley.Menus;

public class LanguageSelectionMenu : IClickableMenu
{
	/// <summary>A language which can be selected in this menu.</summary>
	public class LanguageEntry
	{
		/// <summary>The language code for this entry.</summary>
		public readonly LocalizedContentManager.LanguageCode LanguageCode;

		/// <summary>The data for this language in <c>Data/AdditionalLanguages</c>, if applicable.</summary>
		public readonly ModLanguage ModLanguage;

		/// <summary>The button texture to render.</summary>
		public readonly Texture2D Texture;

		/// <summary>The sprite index for the button in the <see cref="F:StardewValley.Menus.LanguageSelectionMenu.LanguageEntry.Texture" />.</summary>
		public readonly int SpriteIndex;

		/// <summary>Construct an instance.</summary>
		/// <param name="languageCode"><inheritdoc cref="F:StardewValley.Menus.LanguageSelectionMenu.LanguageEntry.LanguageCode" path="/summary" /></param>
		/// <param name="modLanguage"><inheritdoc cref="F:StardewValley.Menus.LanguageSelectionMenu.LanguageEntry.ModLanguage" path="/summary" /></param>
		/// <param name="texture"><inheritdoc cref="F:StardewValley.Menus.LanguageSelectionMenu.LanguageEntry.Texture" path="/summary" /></param>
		/// <param name="spriteIndex"><inheritdoc cref="F:StardewValley.Menus.LanguageSelectionMenu.LanguageEntry.SpriteIndex" path="/summary" /></param>
		public LanguageEntry(LocalizedContentManager.LanguageCode languageCode, ModLanguage modLanguage, Texture2D texture, int spriteIndex)
		{
			this.LanguageCode = languageCode;
			this.ModLanguage = modLanguage;
			this.Texture = texture;
			this.SpriteIndex = spriteIndex;
		}
	}

	public new static int width = 500;

	public new static int height = 728;

	protected int _currentPage;

	protected int _pageCount;

	public readonly Dictionary<string, LanguageEntry> languages;

	public readonly List<ClickableComponent> languageButtons = new List<ClickableComponent>();

	public ClickableTextureComponent nextPageButton;

	public ClickableTextureComponent previousPageButton;

	public LanguageSelectionMenu()
	{
		Texture2D texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LanguageButtons");
		this.languages = new LanguageEntry[12]
		{
			new LanguageEntry(LocalizedContentManager.LanguageCode.en, null, texture, 0),
			new LanguageEntry(LocalizedContentManager.LanguageCode.ru, null, texture, 3),
			new LanguageEntry(LocalizedContentManager.LanguageCode.zh, null, texture, 4),
			new LanguageEntry(LocalizedContentManager.LanguageCode.de, null, texture, 6),
			new LanguageEntry(LocalizedContentManager.LanguageCode.pt, null, texture, 2),
			new LanguageEntry(LocalizedContentManager.LanguageCode.fr, null, texture, 7),
			new LanguageEntry(LocalizedContentManager.LanguageCode.es, null, texture, 1),
			new LanguageEntry(LocalizedContentManager.LanguageCode.ja, null, texture, 5),
			new LanguageEntry(LocalizedContentManager.LanguageCode.ko, null, texture, 8),
			new LanguageEntry(LocalizedContentManager.LanguageCode.it, null, texture, 10),
			new LanguageEntry(LocalizedContentManager.LanguageCode.tr, null, texture, 9),
			new LanguageEntry(LocalizedContentManager.LanguageCode.hu, null, texture, 11)
		}.ToDictionary((LanguageEntry p) => p.LanguageCode.ToString());
		foreach (ModLanguage modLanguage in DataLoader.AdditionalLanguages(Game1.content))
		{
			Texture2D customTexture = Game1.temporaryContent.Load<Texture2D>(modLanguage.ButtonTexture);
			this.languages["ModLanguage_" + modLanguage.Id] = new LanguageEntry(LocalizedContentManager.LanguageCode.mod, modLanguage, customTexture, 0);
		}
		this._pageCount = (int)Math.Floor((float)(this.languages.Count - 1) / 12f) + 1;
		this.SetupButtons();
	}

	private void SetupButtons()
	{
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)((float)LanguageSelectionMenu.width * 2.5f), LanguageSelectionMenu.height);
		this.languageButtons.Clear();
		int buttonWidth = LanguageSelectionMenu.width - 128;
		int buttonHeight = 83;
		int minIndex = 12 * this._currentPage;
		int maxIndex = minIndex + 11;
		int index = 0;
		int row = 0;
		int column = 0;
		foreach (KeyValuePair<string, LanguageEntry> pair in this.languages)
		{
			if (index < minIndex)
			{
				index++;
				continue;
			}
			if (index > maxIndex)
			{
				break;
			}
			this.languageButtons.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64 + column * 6 * 64, (int)topLeft.Y + LanguageSelectionMenu.height - 30 - buttonHeight * (6 - row) - 16, buttonWidth, buttonHeight), pair.Key, null)
			{
				myID = index - minIndex,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
			column++;
			if (column > 2)
			{
				row++;
				column = 0;
			}
		}
		this.previousPageButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + 4, (int)topLeft.Y + LanguageSelectionMenu.height / 2 - 25, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 554,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			visible = (this._currentPage > 0)
		};
		this.nextPageButton = new ClickableTextureComponent(new Rectangle((int)(topLeft.X + (float)LanguageSelectionMenu.width * 2.5f) - 32, (int)topLeft.Y + LanguageSelectionMenu.height / 2 - 25, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 555,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			visible = (this._currentPage < this._pageCount - 1)
		};
		if (Game1.options.SnappyMenus)
		{
			int id = base.currentlySnappedComponent?.myID ?? 0;
			this.populateClickableComponentList();
			base.currentlySnappedComponent = base.getComponentWithID(id);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (this.nextPageButton.visible && this.nextPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this._currentPage++;
			this.SetupButtons();
			return;
		}
		if (this.previousPageButton.visible && this.previousPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this._currentPage--;
			this.SetupButtons();
			return;
		}
		foreach (ClickableComponent component in this.languageButtons)
		{
			if (!component.containsPoint(x, y))
			{
				continue;
			}
			Game1.playSound("select");
			LanguageEntry entry = this.languages.GetValueOrDefault(component.name);
			if (entry == null)
			{
				Game1.log.Error("Received click on unknown language button '" + component.name + "'.");
				continue;
			}
			if (Game1.options.SnappyMenus)
			{
				Game1.activeClickableMenu.setCurrentlySnappedComponentTo(81118);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
			this.ApplyLanguage(entry);
			base.exitThisMenu();
			break;
		}
	}

	public virtual void ApplyLanguage(LanguageEntry entry)
	{
		if (entry.ModLanguage != null)
		{
			LocalizedContentManager.SetModLanguage(entry.ModLanguage);
		}
		else
		{
			LocalizedContentManager.CurrentLanguageCode = entry.LanguageCode;
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		foreach (ClickableComponent component in this.languageButtons)
		{
			if (component.containsPoint(x, y))
			{
				if (component.label == null)
				{
					Game1.playSound("Cowboy_Footstep");
					component.label = "hovered";
				}
			}
			else
			{
				component.label = null;
			}
		}
		this.previousPageButton.tryHover(x, y);
		this.nextPageButton.tryHover(x, y);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)((float)LanguageSelectionMenu.width * 2.5f), LanguageSelectionMenu.height);
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeft.X + 32, (int)topLeft.Y + 156, (int)((float)LanguageSelectionMenu.width * 2.55f) - 64, LanguageSelectionMenu.height / 2 + 25, Color.White, 4f);
		foreach (ClickableComponent c in this.languageButtons)
		{
			LanguageEntry entry = this.languages.GetValueOrDefault(c.name);
			if (entry != null)
			{
				int buttonSourceY = ((entry.SpriteIndex <= 6) ? (entry.SpriteIndex * 78) : ((entry.SpriteIndex - 7) * 78));
				buttonSourceY += ((c.label != null) ? 39 : 0);
				int buttonSourceX = ((entry.SpriteIndex > 6) ? 174 : 0);
				b.Draw(entry.Texture, c.bounds, new Rectangle(buttonSourceX, buttonSourceY, 174, 40), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0f);
			}
		}
		this.previousPageButton.draw(b);
		this.nextPageButton.draw(b);
		if (Game1.activeClickableMenu == this)
		{
			base.drawMouse(b);
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		this.SetupButtons();
	}
}
