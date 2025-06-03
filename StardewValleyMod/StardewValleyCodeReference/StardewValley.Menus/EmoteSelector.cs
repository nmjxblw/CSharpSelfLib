using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class EmoteSelector : IClickableMenu
{
	public Rectangle scrollView;

	public List<ClickableTextureComponent> emoteButtons;

	public ClickableTextureComponent okButton;

	public float scrollY;

	public int emoteIndex;

	protected ClickableTextureComponent _selectedEmote;

	protected ClickableTextureComponent _hoveredEmote;

	protected Texture2D emoteTexture;

	public EmoteSelector(int emote_index, string selected_emote = "")
		: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
	{
		this.emoteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\EmoteMenu");
		Game1.playSound("shwip");
		this.emoteIndex = emote_index;
		Game1.player.faceDirection(2);
		Game1.player.FarmerSprite.StopAnimation();
		this.emoteButtons = new List<ClickableTextureComponent>();
		base.currentlySnappedComponent = null;
		for (int i = 0; i < Farmer.EMOTES.Length; i++)
		{
			Farmer.EmoteType emote_type = Farmer.EMOTES[i];
			if (!emote_type.hidden || Game1.player.performedEmotes.ContainsKey(emote_type.emoteString))
			{
				ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 80, 68), this.emoteTexture, EmoteMenu.GetEmoteNonBubbleSpriteRect(i), 4f, drawShadow: true)
				{
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					upNeighborID = -99998,
					downNeighborID = -99998,
					myID = i
				};
				component.label = emote_type.displayName;
				component.name = emote_type.emoteString;
				component.drawLabelWithShadow = true;
				component.hoverText = ((emote_type.animationFrames != null) ? "animated" : "");
				this.emoteButtons.Add(component);
				if (base.currentlySnappedComponent == null)
				{
					base.currentlySnappedComponent = component;
				}
				if (selected_emote != "" && selected_emote == component.name)
				{
					base.currentlySnappedComponent = component;
					this._selectedEmote = component;
				}
			}
		}
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998,
			myID = 1000,
			drawShadow = true
		};
		this.RepositionElements();
		this.populateClickableComponentList();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
		this.RepositionElements();
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		ClickableTextureComponent oldHovered = this._hoveredEmote;
		this._hoveredEmote = null;
		this.okButton.tryHover(x, y);
		foreach (ClickableTextureComponent component in this.emoteButtons)
		{
			int component_width = component.bounds.Width;
			component.bounds.Width = this.scrollView.Width / 3;
			component.tryHover(x, y);
			if (component != this._selectedEmote && component.bounds.Contains(x, y) && this.scrollView.Contains(x, y))
			{
				this._hoveredEmote = component;
			}
			component.bounds.Width = component_width;
		}
		if (this._hoveredEmote != null && this._hoveredEmote != oldHovered)
		{
			Game1.playSound("shiny4");
		}
	}

	private void RepositionElements()
	{
		this.scrollView = new Rectangle(base.xPositionOnScreen + 64, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 4, base.width - 128, base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder - 64 + 8);
		this.RepositionScrollElements();
	}

	public void RepositionScrollElements()
	{
		int y_offset = (int)this.scrollY + 4;
		if (this.scrollY > 0f)
		{
			this.scrollY = 0f;
		}
		int x_offset = 8;
		foreach (ClickableTextureComponent component in this.emoteButtons)
		{
			component.bounds.X = this.scrollView.X + x_offset;
			component.bounds.Y = this.scrollView.Y + y_offset;
			if (component.bounds.Bottom > this.scrollView.Bottom)
			{
				y_offset = 4;
				x_offset += this.scrollView.Width / 3;
				component.bounds.X = this.scrollView.X + x_offset;
				component.bounds.Y = this.scrollView.Y + y_offset;
			}
			y_offset += component.bounds.Height;
			if (this.scrollView.Intersects(component.bounds))
			{
				component.visible = true;
			}
			else
			{
				component.visible = false;
			}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		foreach (ClickableTextureComponent component in this.emoteButtons)
		{
			int component_width = component.bounds.Width;
			component.bounds.Width = this.scrollView.Width / 3;
			if (component.bounds.Contains(x, y) && this.scrollView.Contains(x, y))
			{
				component.bounds.Width = component_width;
				if (this.emoteIndex < Game1.player.GetEmoteFavorites().Count)
				{
					Game1.player.GetEmoteFavorites()[this.emoteIndex] = component.name;
				}
				base.exitThisMenu(playSound: false);
				Game1.playSound("drumkit6");
				if (!Game1.options.gamepadControls)
				{
					Game1.emoteMenu = new EmoteMenu();
				}
				return;
			}
			component.bounds.Width = component_width;
		}
		if (this.okButton.containsPoint(x, y))
		{
			base.exitThisMenu();
		}
	}

	public bool canLeaveMenu()
	{
		return true;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), base.xPositionOnScreen - 128 - 8, base.yPositionOnScreen + 128 - 8, 192, 164, Color.White, 1f, drawShadow: false);
		Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
		foreach (ClickableTextureComponent component in this.emoteButtons)
		{
			if (component == base.currentlySnappedComponent && Game1.options.gamepadControls && component != this._selectedEmote && component == this._hoveredEmote)
			{
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(64, 320, 60, 60), component.bounds.X + 64 + 8, component.bounds.Y + 8, this.scrollView.Width / 3 - 64 - 16, component.bounds.Height - 16, Color.White, 1f, drawShadow: false);
				Utility.drawWithShadow(b, this.emoteTexture, component.getVector2() - new Vector2(4f, 4f), new Rectangle(83, 0, 18, 18), Color.White, 0f, Vector2.Zero, 4f);
			}
			component.draw(b, Color.White * ((component == this._selectedEmote) ? 0.4f : 1f), 0.87f);
			if (component != this._selectedEmote && component.hoverText != "" && Game1.currentGameTime.TotalGameTime.Milliseconds % 500 < 250)
			{
				b.Draw(component.texture, component.getVector2(), new Rectangle(component.sourceRect.X + 80, component.sourceRect.Y, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
		}
		if (this._selectedEmote != null)
		{
			for (int i = 0; i < 8; i++)
			{
				float radians = Utility.Lerp(0f, (float)Math.PI * 2f, (float)i / 8f);
				Vector2 pos = Vector2.Zero;
				pos.X = (int)((float)(base.xPositionOnScreen - 64 + (int)(Math.Cos(radians) * 12.0) * 4) - 3.5f);
				pos.Y = (int)((float)(base.yPositionOnScreen + 192 + (int)((0.0 - Math.Sin(radians)) * 12.0) * 4) - 3.5f);
				Utility.drawWithShadow(b, this.emoteTexture, pos, new Rectangle(64 + ((i == this.emoteIndex) ? 8 : 0), 48, 8, 8), Color.White, 0f, Vector2.Zero);
			}
		}
		this.okButton.draw(b);
		base.drawMouse(b);
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		Game1.player.noMovementPause = Math.Max(Game1.player.noMovementPause, 200);
	}
}
