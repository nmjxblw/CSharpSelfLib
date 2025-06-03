using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class ChatMessage
{
	public List<ChatSnippet> message = new List<ChatSnippet>();

	public int timeLeftToDisplay;

	public int verticalSize;

	public float alpha = 1f;

	public Color color;

	public LocalizedContentManager.LanguageCode language;

	public void parseMessageForEmoji(string messagePlaintext)
	{
		if (messagePlaintext == null)
		{
			return;
		}
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < messagePlaintext.Length; i++)
		{
			if (messagePlaintext[i] == '[')
			{
				if (sb.Length > 0)
				{
					this.breakNewLines(sb);
				}
				sb.Clear();
				int tag_close_index = messagePlaintext.IndexOf(']', i);
				int next_open_index = -1;
				if (i + 1 < messagePlaintext.Length)
				{
					next_open_index = messagePlaintext.IndexOf('[', i + 1);
				}
				if (tag_close_index != -1 && (next_open_index == -1 || next_open_index > tag_close_index))
				{
					string sub = messagePlaintext.Substring(i + 1, tag_close_index - i - 1);
					if (int.TryParse(sub, out var emojiIndex))
					{
						if (emojiIndex < EmojiMenu.totalEmojis)
						{
							this.message.Add(new ChatSnippet(emojiIndex));
						}
					}
					else
					{
						switch (sub)
						{
						case "gray":
						case "jade":
						case "pink":
						case "plum":
						case "aqua":
						case "blue":
						case "jungle":
						case "yellow":
						case "orange":
						case "purple":
						case "salmon":
						case "green":
						case "peach":
						case "brown":
						case "cream":
						case "red":
						case "yellowgreen":
							if (this.color.Equals(Color.White))
							{
								this.color = ChatMessage.getColorFromName(sub);
							}
							break;
						default:
							sb.Append("[");
							sb.Append(sub);
							sb.Append("]");
							break;
						}
					}
					i = tag_close_index;
				}
				else
				{
					sb.Append("[");
				}
			}
			else
			{
				sb.Append(messagePlaintext[i]);
			}
		}
		if (sb.Length > 0)
		{
			this.breakNewLines(sb);
		}
	}

	public static Color getColorFromName(string name)
	{
		return name switch
		{
			"aqua" => Color.MediumTurquoise, 
			"jungle" => Color.SeaGreen, 
			"red" => new Color(220, 20, 20), 
			"blue" => Color.DodgerBlue, 
			"jade" => new Color(50, 230, 150), 
			"green" => new Color(0, 180, 10), 
			"yellowgreen" => new Color(182, 214, 0), 
			"pink" => Color.HotPink, 
			"yellow" => new Color(240, 200, 0), 
			"orange" => new Color(255, 100, 0), 
			"purple" => new Color(138, 43, 250), 
			"gray" => Color.Gray, 
			"cream" => new Color(255, 255, 180), 
			"peach" => new Color(255, 180, 120), 
			"brown" => new Color(160, 80, 30), 
			"salmon" => Color.Salmon, 
			"plum" => new Color(190, 0, 190), 
			_ => Color.White, 
		};
	}

	private void breakNewLines(StringBuilder sb)
	{
		string[] split = sb.ToString().Split(Environment.NewLine);
		for (int i = 0; i < split.Length; i++)
		{
			this.message.Add(new ChatSnippet(split[i], this.language));
			if (i != split.Length - 1)
			{
				this.message.Add(new ChatSnippet(Environment.NewLine, this.language));
			}
		}
	}

	public static string makeMessagePlaintext(List<ChatSnippet> message, bool include_color_information)
	{
		StringBuilder sb = new StringBuilder();
		foreach (ChatSnippet cs in message)
		{
			if (cs.message != null)
			{
				sb.Append(cs.message);
			}
			else if (cs.emojiIndex != -1)
			{
				sb.Append("[" + cs.emojiIndex + "]");
			}
		}
		if (include_color_information && Game1.player.defaultChatColor != null && !ChatMessage.getColorFromName(Game1.player.defaultChatColor).Equals(Color.White))
		{
			sb.Append(" [");
			sb.Append(Game1.player.defaultChatColor);
			sb.Append("]");
		}
		return sb.ToString();
	}

	public void draw(SpriteBatch b, int x, int y)
	{
		float xPositionSoFar = 0f;
		float yPositionSoFar = 0f;
		for (int i = 0; i < this.message.Count; i++)
		{
			if (this.message[i].emojiIndex != -1)
			{
				b.Draw(ChatBox.emojiTexture, new Vector2((float)x + xPositionSoFar + 1f, (float)y + yPositionSoFar - 4f), new Rectangle(this.message[i].emojiIndex * 9 % ChatBox.emojiTexture.Width, this.message[i].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			}
			else if (this.message[i].message != null)
			{
				if (this.message[i].message.Equals(Environment.NewLine))
				{
					xPositionSoFar = 0f;
					yPositionSoFar += ChatBox.messageFont(this.language).MeasureString("(").Y;
				}
				else
				{
					b.DrawString(ChatBox.messageFont(this.language), this.message[i].message, new Vector2((float)x + xPositionSoFar, (float)y + yPositionSoFar), this.color * this.alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				}
			}
			xPositionSoFar += this.message[i].myLength;
			if (xPositionSoFar >= 888f)
			{
				xPositionSoFar = 0f;
				yPositionSoFar += ChatBox.messageFont(this.language).MeasureString("(").Y;
				if (this.message.Count > i + 1 && this.message[i + 1].message != null && this.message[i + 1].message.Equals(Environment.NewLine))
				{
					i++;
				}
			}
		}
	}
}
