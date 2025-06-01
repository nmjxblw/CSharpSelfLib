using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class EmoteMenu : IClickableMenu
{
	public Texture2D menuBackgroundTexture;

	public List<string> emotes;

	protected Point _mouseStartPosition;

	public bool _hasSelectedEmote;

	protected List<ClickableTextureComponent> _emoteButtons;

	protected string _selectedEmote;

	protected int _selectedIndex = -1;

	protected int _oldSelection;

	protected int _selectedTime;

	protected float _alpha;

	protected int _menuCloseGracePeriod = -1;

	protected int _age;

	public bool gamepadMode;

	protected int _expandTime = 200;

	protected int _expandedButtonRadius = 24;

	protected int _buttonRadius;

	public EmoteMenu()
	{
		this.menuBackgroundTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\EmoteMenu");
		base.width = 256;
		base.height = 256;
		base.xPositionOnScreen = (int)((float)(Game1.viewport.Width / 2) - (float)base.width / 2f);
		base.yPositionOnScreen = (int)((float)(Game1.viewport.Height / 2) - (float)base.height / 2f);
		this.emotes = new List<string>();
		foreach (string emote_string in Game1.player.GetEmoteFavorites())
		{
			this.emotes.Add(emote_string);
		}
		this._mouseStartPosition = Game1.getMousePosition(ui_scale: false);
		this._alpha = 0f;
		this._menuCloseGracePeriod = 300;
		this._CreateEmoteButtons();
		this._SnapToPlayerPosition();
	}

	protected void _CreateEmoteButtons()
	{
		this._emoteButtons = new List<ClickableTextureComponent>();
		for (int i = 0; i < this.emotes.Count; i++)
		{
			int emote_index = -1;
			for (int j = 0; j < Farmer.EMOTES.Length; j++)
			{
				if (Farmer.EMOTES[j].emoteString == this.emotes[i])
				{
					emote_index = j;
					break;
				}
			}
			ClickableTextureComponent emote_button = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), this.menuBackgroundTexture, EmoteMenu.GetEmoteNonBubbleSpriteRect(emote_index), 4f);
			this._emoteButtons.Add(emote_button);
		}
		this._RepositionButtons();
	}

	public static Rectangle GetEmoteSpriteRect(int emote_index)
	{
		if (emote_index <= 0)
		{
			return new Rectangle(48, 0, 16, 16);
		}
		return new Rectangle(emote_index % 4 * 16 + 48, emote_index / 4 * 16, 16, 16);
	}

	public static Rectangle GetEmoteNonBubbleSpriteRect(int emote_index)
	{
		return new Rectangle(emote_index % 4 * 16, emote_index / 4 * 16, 16, 16);
	}

	public override void applyMovementKey(int direction)
	{
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		Game1.emoteMenu = null;
		Game1.oldMouseState = Game1.input.GetMouseState();
		base.cleanupBeforeExit();
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		x = (int)Utility.ModifyCoordinateFromUIScale(x);
		y = (int)Utility.ModifyCoordinateFromUIScale(y);
		if (this.gamepadMode)
		{
			return;
		}
		for (int i = 0; i < this._emoteButtons.Count; i++)
		{
			if (this._emoteButtons[i].containsPoint(x, y))
			{
				this._selectedEmote = this.emotes[i];
				this._selectedIndex = i;
				if (this._selectedIndex != this._oldSelection)
				{
					this._selectedTime = 0;
				}
				return;
			}
		}
		this._selectedEmote = null;
		this._selectedIndex = -1;
	}

	protected void _RepositionButtons()
	{
		for (int i = 0; i < this._emoteButtons.Count; i++)
		{
			ClickableTextureComponent emote_button = this._emoteButtons[i];
			float radians = Utility.Lerp(0f, (float)Math.PI * 2f, (float)i / (float)this._emoteButtons.Count);
			emote_button.bounds.X = (int)((float)(base.xPositionOnScreen + base.width / 2 + (int)(Math.Cos(radians) * (double)this._buttonRadius) * 4) - (float)emote_button.bounds.Width / 2f);
			emote_button.bounds.Y = (int)((float)(base.yPositionOnScreen + base.height / 2 + (int)((0.0 - Math.Sin(radians)) * (double)this._buttonRadius) * 4) - (float)emote_button.bounds.Height / 2f);
		}
	}

	protected void _SnapToPlayerPosition()
	{
		if (Game1.player != null)
		{
			Vector2 player_position = Game1.player.getLocalPosition(Game1.viewport) + new Vector2((float)(-base.width) / 2f, (float)(-base.height) / 2f);
			base.xPositionOnScreen = (int)player_position.X + 32;
			base.yPositionOnScreen = (int)player_position.Y - 64;
			if (base.xPositionOnScreen + base.width > Game1.viewport.Width)
			{
				base.xPositionOnScreen -= base.xPositionOnScreen + base.width - Game1.viewport.Width;
			}
			if (base.xPositionOnScreen < 0)
			{
				base.xPositionOnScreen -= base.xPositionOnScreen;
			}
			if (base.yPositionOnScreen + base.height > Game1.viewport.Height)
			{
				base.yPositionOnScreen -= base.yPositionOnScreen + base.height - Game1.viewport.Height;
			}
			if (base.yPositionOnScreen < 0)
			{
				base.yPositionOnScreen -= base.yPositionOnScreen;
			}
			this._RepositionButtons();
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		this._age += time.ElapsedGameTime.Milliseconds;
		if (this._age > this._expandTime)
		{
			this._age = this._expandTime;
		}
		if (!this.gamepadMode && Game1.options.gamepadControls && (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f))
		{
			this.gamepadMode = true;
		}
		this._alpha = (float)this._age / (float)this._expandTime;
		this._buttonRadius = (int)((float)this._age / (float)this._expandTime * (float)this._expandedButtonRadius);
		this._SnapToPlayerPosition();
		Vector2 offset = default(Vector2);
		if (this.gamepadMode)
		{
			this._mouseStartPosition = Game1.getMousePosition(ui_scale: false);
			if (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f)
			{
				this._hasSelectedEmote = true;
				offset = new Vector2(Game1.input.GetGamePadState().ThumbSticks.Right.X, Game1.input.GetGamePadState().ThumbSticks.Right.Y);
				offset.Y *= -1f;
				offset.Normalize();
				float highest_dot = -1f;
				for (int i = 0; i < this._emoteButtons.Count; i++)
				{
					float dot = Vector2.Dot(value2: new Vector2((float)this._emoteButtons[i].bounds.Center.X - ((float)base.xPositionOnScreen + (float)base.width / 2f), (float)this._emoteButtons[i].bounds.Center.Y - ((float)base.yPositionOnScreen + (float)base.height / 2f)), value1: offset);
					if (dot > highest_dot)
					{
						highest_dot = dot;
						this._selectedEmote = this.emotes[i];
						this._selectedIndex = i;
					}
				}
				this._menuCloseGracePeriod = 100;
				if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) && this._selectedIndex >= 0)
				{
					Game1.activeClickableMenu = new EmoteSelector(this._selectedIndex, this.emotes[this._selectedIndex]);
					base.exitThisMenuNoSound();
					return;
				}
			}
			else
			{
				if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick) && this._menuCloseGracePeriod < 100)
				{
					this._menuCloseGracePeriod = 100;
				}
				if (this._menuCloseGracePeriod >= 0)
				{
					this._menuCloseGracePeriod -= time.ElapsedGameTime.Milliseconds;
				}
				if (this._menuCloseGracePeriod <= 0 && !Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
				{
					this.ConfirmSelection();
				}
			}
		}
		for (int j = 0; j < this._emoteButtons.Count; j++)
		{
			if (this._emoteButtons[j].scale > 4f)
			{
				this._emoteButtons[j].scale = Utility.MoveTowards(this._emoteButtons[j].scale, 4f, (float)time.ElapsedGameTime.Milliseconds / 1000f * 10f);
			}
		}
		if (this._selectedEmote != null && this._selectedIndex > -1)
		{
			this._emoteButtons[this._selectedIndex].scale = 5f;
		}
		if (this._oldSelection != this._selectedIndex)
		{
			this._oldSelection = this._selectedIndex;
			this._selectedTime = 0;
		}
		this._selectedTime += time.ElapsedGameTime.Milliseconds;
		base.update(time);
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		x = (int)Utility.ModifyCoordinateFromUIScale(x);
		y = (int)Utility.ModifyCoordinateFromUIScale(y);
		for (int i = 0; i < this._emoteButtons.Count; i++)
		{
			if (this._emoteButtons[i].containsPoint(x, y) && Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new EmoteSelector(i, this.emotes[i]);
				base.exitThisMenuNoSound();
				return;
			}
		}
		base.receiveLeftClick(x, y, playSound);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		x = (int)Utility.ModifyCoordinateFromUIScale(x);
		y = (int)Utility.ModifyCoordinateFromUIScale(y);
		this.ConfirmSelection();
		base.receiveLeftClick(x, y, playSound);
	}

	public void ConfirmSelection()
	{
		if (this._selectedEmote != null)
		{
			Game1.chatBox.textBoxEnter("/emote " + this._selectedEmote);
		}
		base.exitThisMenu(playSound: false);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		Game1.StartWorldDrawInUI(b);
		Color background_color = Color.White;
		background_color.A = (byte)Utility.Lerp(0f, 255f, this._alpha);
		foreach (ClickableTextureComponent emoteButton in this._emoteButtons)
		{
			emoteButton.draw(b, background_color, 0.86f);
		}
		if (this._selectedEmote != null)
		{
			Farmer.EmoteType[] eMOTES = Farmer.EMOTES;
			foreach (Farmer.EmoteType emote_type in eMOTES)
			{
				if (emote_type.emoteString == this._selectedEmote)
				{
					SpriteText.drawStringWithScrollCenteredAt(b, emote_type.displayName, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + base.height);
					break;
				}
			}
		}
		if (this._selectedIndex >= 0 && this._selectedTime >= 250)
		{
			Vector2 draw_position = Utility.PointToVector2(this._emoteButtons[this._selectedIndex].bounds.Center);
			draw_position.X += 16f;
			if (!this.gamepadMode)
			{
				draw_position = Utility.PointToVector2(Game1.getMousePosition(ui_scale: false)) + new Vector2(32f, 32f);
				b.Draw(this.menuBackgroundTexture, draw_position, new Rectangle(64, 0, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
			}
			else
			{
				b.Draw(Game1.controllerMaps, draw_position, Utility.controllerMapSourceRect(new Rectangle(625, 260, 28, 28)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
			}
			draw_position.X += 32f;
			b.Draw(this.menuBackgroundTexture, draw_position, new Rectangle(64, 16, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
		}
		Game1.EndWorldDrawInUI(b);
	}
}
