using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class ChatTextBox : TextBox
{
	public IClickableMenu parentMenu;

	public List<ChatSnippet> finalText = new List<ChatSnippet>();

	public float currentWidth;

	public ChatTextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor)
		: base(textBoxTexture, caretTexture, font, textColor)
	{
	}

	public void reset()
	{
		this.currentWidth = 0f;
		this.finalText.Clear();
	}

	public void setText(string text)
	{
		this.reset();
		this.RecieveTextInput(text);
	}

	public override void RecieveTextInput(string text)
	{
		if (this.finalText.Count == 0)
		{
			this.finalText.Add(new ChatSnippet("", LocalizedContentManager.CurrentLanguageCode));
		}
		if (!(this.currentWidth + ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(text).X >= (float)(base.Width - 16)))
		{
			ChatSnippet lastSnippet = this.finalText.Last();
			if (lastSnippet.message != null)
			{
				lastSnippet.message += text;
			}
			else
			{
				this.finalText.Add(new ChatSnippet(text, LocalizedContentManager.CurrentLanguageCode));
			}
			this.finalText.Last().message = Utility.FilterDirtyWordsIfStrictPlatform(this.finalText.Last().message);
			this.updateWidth();
		}
	}

	public override void RecieveTextInput(char inputChar)
	{
		this.RecieveTextInput(inputChar.ToString() ?? "");
	}

	public override void RecieveCommandInput(char command)
	{
		if (base.Selected && command == '\b')
		{
			this.backspace();
		}
		else
		{
			base.RecieveCommandInput(command);
		}
	}

	public void backspace()
	{
		if (this.finalText.Count > 0)
		{
			ChatSnippet lastSnippet = this.finalText.Last();
			if (lastSnippet.message != null)
			{
				if (lastSnippet.message.Length > 1)
				{
					lastSnippet.message = lastSnippet.message.Remove(lastSnippet.message.Length - 1);
				}
				else
				{
					this.finalText.RemoveAt(this.finalText.Count - 1);
				}
			}
			else if (lastSnippet.emojiIndex != -1)
			{
				this.finalText.RemoveAt(this.finalText.Count - 1);
			}
		}
		this.updateWidth();
	}

	public void receiveEmoji(int emoji)
	{
		if (!(this.currentWidth + 40f > (float)(base.Width - 16)))
		{
			this.finalText.Add(new ChatSnippet(emoji));
			this.updateWidth();
		}
	}

	public void updateWidth()
	{
		this.currentWidth = 0f;
		foreach (ChatSnippet cs in this.finalText)
		{
			if (cs.message != null)
			{
				cs.myLength = ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(cs.message).X;
			}
			this.currentWidth += cs.myLength;
		}
	}

	public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
	{
		bool num = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
		if (base._textBoxTexture != null)
		{
			spriteBatch.Draw(base._textBoxTexture, new Rectangle(base.X, base.Y, 16, base.Height), new Rectangle(0, 0, 16, base.Height), Color.White);
			spriteBatch.Draw(base._textBoxTexture, new Rectangle(base.X + 16, base.Y, base.Width - 32, base.Height), new Rectangle(16, 0, 4, base.Height), Color.White);
			spriteBatch.Draw(base._textBoxTexture, new Rectangle(base.X + base.Width - 16, base.Y, 16, base.Height), new Rectangle(base._textBoxTexture.Bounds.Width - 16, 0, 16, base.Height), Color.White);
		}
		else
		{
			Game1.drawDialogueBox(base.X - 32, base.Y - 112 + 10, base.Width + 80, base.Height, speaker: false, drawOnlyBox: true);
		}
		if (num && base.Selected)
		{
			spriteBatch.Draw(Game1.staminaRect, new Rectangle(base.X + 16 + (int)this.currentWidth - 2, base.Y + 8, 4, 32), base._textColor);
		}
		float xPositionSoFar = 0f;
		for (int i = 0; i < this.finalText.Count; i++)
		{
			if (this.finalText[i].emojiIndex != -1)
			{
				spriteBatch.Draw(ChatBox.emojiTexture, new Vector2((float)base.X + xPositionSoFar + 12f, base.Y + 12), new Rectangle(this.finalText[i].emojiIndex * 9 % ChatBox.emojiTexture.Width, this.finalText[i].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
			else if (this.finalText[i].message != null)
			{
				spriteBatch.DrawString(ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode), this.finalText[i].message, new Vector2((float)base.X + xPositionSoFar + 12f, base.Y + 12), ChatMessage.getColorFromName(Game1.player.defaultChatColor), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
			}
			xPositionSoFar += this.finalText[i].myLength;
		}
	}
}
