using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class EmojiMenu : IClickableMenu
{
	public const int EMOJI_SIZE = 9;

	private Texture2D chatBoxTexture;

	private Texture2D emojiTexture;

	private ChatBox chatBox;

	private List<ClickableComponent> emojiSelectionButtons = new List<ClickableComponent>();

	private int pageStartIndex;

	private ClickableComponent upArrow;

	private ClickableComponent downArrow;

	private ClickableComponent sendArrow;

	public static int totalEmojis;

	public static int totalVisibleEmojis;

	public EmojiMenu(ChatBox chatBox, Texture2D emojiTexture, Texture2D chatBoxTexture)
	{
		this.chatBox = chatBox;
		this.chatBoxTexture = chatBoxTexture;
		this.emojiTexture = emojiTexture;
		base.width = 300;
		base.height = 248;
		for (int y = 0; y < 5; y++)
		{
			for (int x = 0; x < 6; x++)
			{
				this.emojiSelectionButtons.Add(new ClickableComponent(new Rectangle(16 + x * 10 * 4, 16 + y * 10 * 4, 36, 36), (x + y * 6).ToString() ?? ""));
			}
		}
		this.upArrow = new ClickableComponent(new Rectangle(256, 16, 32, 20), "");
		this.downArrow = new ClickableComponent(new Rectangle(256, 156, 32, 20), "");
		this.sendArrow = new ClickableComponent(new Rectangle(256, 188, 32, 32), "");
		EmojiMenu.totalEmojis = 197;
		EmojiMenu.totalVisibleEmojis = 196;
	}

	public void leftClick(int x, int y, ChatBox cb)
	{
		if (!this.isWithinBounds(x, y))
		{
			return;
		}
		int relativeX = x - base.xPositionOnScreen;
		int relativeY = y - base.yPositionOnScreen;
		if (this.upArrow.containsPoint(relativeX, relativeY))
		{
			this.upArrowPressed();
		}
		else if (this.downArrow.containsPoint(relativeX, relativeY))
		{
			this.downArrowPressed();
		}
		else if (this.sendArrow.containsPoint(relativeX, relativeY) && cb.chatBox.currentWidth > 0f)
		{
			cb.textBoxEnter(cb.chatBox);
			this.sendArrow.scale = 0.5f;
			Game1.playSound("shwip");
		}
		foreach (ClickableComponent c in this.emojiSelectionButtons)
		{
			if (c.containsPoint(relativeX, relativeY))
			{
				int index = this.pageStartIndex + int.Parse(c.name);
				cb.chatBox.receiveEmoji(index);
				Game1.playSound("coin");
				break;
			}
		}
	}

	private void upArrowPressed(int amountToScroll = 30)
	{
		if (this.pageStartIndex != 0)
		{
			Game1.playSound("Cowboy_Footstep");
		}
		this.pageStartIndex = Math.Max(0, this.pageStartIndex - amountToScroll);
		this.upArrow.scale = 0.75f;
	}

	private void downArrowPressed(int amountToScroll = 30)
	{
		if (this.pageStartIndex != EmojiMenu.totalVisibleEmojis - 30)
		{
			Game1.playSound("Cowboy_Footstep");
		}
		this.pageStartIndex = Math.Min(EmojiMenu.totalVisibleEmojis - 30, this.pageStartIndex + amountToScroll);
		this.downArrow.scale = 0.75f;
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		if (direction < 0)
		{
			this.downArrowPressed(6);
		}
		else if (direction > 0)
		{
			this.upArrowPressed(6);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(this.chatBoxTexture, new Rectangle(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height), new Rectangle(0, 56, 300, 244), Color.White);
		for (int i = 0; i < this.emojiSelectionButtons.Count; i++)
		{
			b.Draw(this.emojiTexture, new Vector2(this.emojiSelectionButtons[i].bounds.X + base.xPositionOnScreen, this.emojiSelectionButtons[i].bounds.Y + base.yPositionOnScreen), new Rectangle((this.pageStartIndex + i) * 9 % this.emojiTexture.Width, (this.pageStartIndex + i) * 9 / this.emojiTexture.Width * 9, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		}
		if (this.upArrow.scale < 1f)
		{
			this.upArrow.scale += 0.05f;
		}
		if (this.downArrow.scale < 1f)
		{
			this.downArrow.scale += 0.05f;
		}
		if (this.sendArrow.scale < 1f)
		{
			this.sendArrow.scale += 0.05f;
		}
		b.Draw(this.chatBoxTexture, new Vector2(this.upArrow.bounds.X + base.xPositionOnScreen + 16, this.upArrow.bounds.Y + base.yPositionOnScreen + 10), new Rectangle(156, 300, 32, 20), Color.White * ((this.pageStartIndex == 0) ? 0.25f : 1f), 0f, new Vector2(16f, 10f), this.upArrow.scale, SpriteEffects.None, 0.9f);
		b.Draw(this.chatBoxTexture, new Vector2(this.downArrow.bounds.X + base.xPositionOnScreen + 16, this.downArrow.bounds.Y + base.yPositionOnScreen + 10), new Rectangle(192, 300, 32, 20), Color.White * ((this.pageStartIndex == EmojiMenu.totalVisibleEmojis - 30) ? 0.25f : 1f), 0f, new Vector2(16f, 10f), this.downArrow.scale, SpriteEffects.None, 0.9f);
		b.Draw(this.chatBoxTexture, new Vector2(this.sendArrow.bounds.X + base.xPositionOnScreen + 16, this.sendArrow.bounds.Y + base.yPositionOnScreen + 10), new Rectangle(116, 304, 28, 28), Color.White * ((this.chatBox.chatBox.currentWidth > 0f) ? 1f : 0.4f), 0f, new Vector2(14f, 16f), this.sendArrow.scale, SpriteEffects.None, 0.9f);
	}
}
