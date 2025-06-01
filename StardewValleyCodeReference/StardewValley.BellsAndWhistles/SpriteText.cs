using System;
using System.Collections.Generic;
using System.Linq;
using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace StardewValley.BellsAndWhistles;

public class SpriteText
{
	public enum ScrollTextAlignment
	{
		Left,
		Center,
		Right
	}

	public const int scrollStyle_scroll = 0;

	public const int scrollStyle_speechBubble = 1;

	public const int scrollStyle_darkMetal = 2;

	public const int scrollStyle_blueMetal = 3;

	public const int maxCharacter = 999999;

	public const int maxHeight = 999999;

	public const int characterWidth = 8;

	public const int characterHeight = 16;

	public const int horizontalSpaceBetweenCharacters = 0;

	public const int verticalSpaceBetweenCharacters = 2;

	public const char newLine = '^';

	public static float fontPixelZoom = 3f;

	public static float shadowAlpha = 0.15f;

	public static Dictionary<char, FontChar> characterMap;

	public static FontFile FontFile = null;

	public static List<Texture2D> fontPages = null;

	public static Texture2D spriteTexture;

	public static Texture2D coloredTexture;

	public const int color_index_Default = -1;

	public const int color_index_Black = 0;

	public const int color_index_Blue = 1;

	public const int color_index_Red = 2;

	public const int color_index_Purple = 3;

	public const int color_index_White = 4;

	public const int color_index_Orange = 5;

	public const int color_index_Green = 6;

	public const int color_index_Cyan = 7;

	public const int color_index_Gray = 8;

	public const int color_index_JojaBlue = 9;

	public static bool forceEnglishFont = false;

	public static float FontPixelZoom => SpriteText.fontPixelZoom + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? ((Game1.options.dialogueFontScale - 1f) / (Game1.options.useChineseSmoothFont ? 4f : 2f)) : 0f);

	public static Color color_Default
	{
		get
		{
			if (!LocalizedContentManager.CurrentLanguageLatin && (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ru || Game1.options.useAlternateFont))
			{
				return new Color(86, 22, 12);
			}
			return Color.White;
		}
	}

	public static Color color_Black { get; } = Color.Black;

	public static Color color_Blue { get; } = Color.SkyBlue;

	public static Color color_Red { get; } = Color.Red;

	public static Color color_Purple { get; } = new Color(110, 43, 255);

	public static Color color_White { get; } = Color.White;

	public static Color color_Orange { get; } = Color.OrangeRed;

	public static Color color_Green { get; } = Color.LimeGreen;

	public static Color color_Cyan { get; } = Color.Cyan;

	public static Color color_Gray { get; } = new Color(60, 60, 60);

	public static Color color_JojaBlue { get; } = new Color(52, 50, 122);

	public static void drawStringHorizontallyCenteredAt(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, Color? color = null, int maxWidth = 99999)
	{
		SpriteText.drawString(b, s, x - SpriteText.getWidthOfString(s, maxWidth) / 2, y, characterPosition, width, height, alpha, layerDepth, junimoText, -1, "", color);
	}

	public static int getWidthOfString(string s, int widthConstraint = 999999)
	{
		SpriteText.setUpCharacterMap();
		int width = 0;
		int maxWidth = 0;
		for (int i = 0; i < s.Length; i++)
		{
			if (SpriteText.isUsingNonSpriteSheetFont() && !SpriteText.forceEnglishFont)
			{
				if (SpriteText.characterMap.TryGetValue(s[i], out var c))
				{
					width += c.XAdvance;
				}
				maxWidth = Math.Max(width, maxWidth);
				if (s[i] == '^' || (float)width * SpriteText.FontPixelZoom > (float)widthConstraint)
				{
					width = 0;
				}
				continue;
			}
			width += 8 + SpriteText.getWidthOffsetForChar(s[i]);
			if (i > 0)
			{
				width += SpriteText.getWidthOffsetForChar(s[Math.Max(0, i - 1)]);
			}
			maxWidth = Math.Max(width, maxWidth);
			float pos = SpriteText.positionOfNextSpace(s, i, (int)((float)width * SpriteText.FontPixelZoom), 0);
			if (s[i] == '^' || (float)width * SpriteText.FontPixelZoom >= (float)widthConstraint || pos >= (float)widthConstraint)
			{
				width = 0;
			}
		}
		return (int)((float)maxWidth * SpriteText.FontPixelZoom);
	}

	public static bool IsMissingCharacters(string text)
	{
		SpriteText.setUpCharacterMap();
		if (!LocalizedContentManager.CurrentLanguageLatin && !SpriteText.forceEnglishFont)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (!SpriteText.characterMap.ContainsKey(text[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static int getHeightOfString(string s, int widthConstraint = 999999)
	{
		if (s.Length == 0)
		{
			return 0;
		}
		Vector2 position = default(Vector2);
		int accumulatedHorizontalSpaceBetweenCharacters = 0;
		s = s.Replace(Environment.NewLine, "");
		SpriteText.setUpCharacterMap();
		if (SpriteText.isUsingNonSpriteSheetFont() && !SpriteText.forceEnglishFont)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '^')
				{
					position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
					position.X = 0f;
					continue;
				}
				if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= widthConstraint)
				{
					position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					position.X = 0f;
				}
				if (SpriteText.characterMap.TryGetValue(s[i], out var c))
				{
					position.X += (float)c.XAdvance * SpriteText.FontPixelZoom;
				}
			}
			return (int)(position.Y + (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom);
		}
		for (int j = 0; j < s.Length; j++)
		{
			if (s[j] == '^')
			{
				position.Y += 18f * SpriteText.FontPixelZoom;
				position.X = 0f;
				accumulatedHorizontalSpaceBetweenCharacters = 0;
				continue;
			}
			if (SpriteText.positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= widthConstraint)
			{
				position.Y += 18f * SpriteText.FontPixelZoom;
				accumulatedHorizontalSpaceBetweenCharacters = 0;
				position.X = 0f;
			}
			position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)SpriteText.getWidthOffsetForChar(s[j]) * SpriteText.FontPixelZoom;
			if (j > 0)
			{
				position.X += (float)SpriteText.getWidthOffsetForChar(s[j - 1]) * SpriteText.FontPixelZoom;
			}
			accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
		}
		return (int)(position.Y + 16f * SpriteText.FontPixelZoom);
	}

	public static Color getColorFromIndex(int index)
	{
		return index switch
		{
			1 => SpriteText.color_Blue, 
			2 => SpriteText.color_Red, 
			3 => SpriteText.color_Purple, 
			-1 => SpriteText.color_Default, 
			4 => SpriteText.color_White, 
			5 => SpriteText.color_Orange, 
			6 => SpriteText.color_Green, 
			7 => SpriteText.color_Cyan, 
			8 => SpriteText.color_Gray, 
			9 => SpriteText.color_JojaBlue, 
			_ => Color.Black, 
		};
	}

	public static string getSubstringBeyondHeight(string s, int width, int height)
	{
		Vector2 position = default(Vector2);
		int accumulatedHorizontalSpaceBetweenCharacters = 0;
		s = s.Replace(Environment.NewLine, "");
		SpriteText.setUpCharacterMap();
		if (SpriteText.isUsingNonSpriteSheetFont())
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '^')
				{
					position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					continue;
				}
				if (SpriteText.characterMap.TryGetValue(s[i], out var c))
				{
					if (i > 0)
					{
						position.X += (float)c.XAdvance * SpriteText.FontPixelZoom;
					}
					if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
					{
						position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = 0f;
					}
				}
				if (position.Y >= (float)height - (float)SpriteText.FontFile.Common.LineHeight * SpriteText.FontPixelZoom * 2f)
				{
					return s.Substring(SpriteText.getLastSpace(s, i));
				}
			}
			return "";
		}
		for (int j = 0; j < s.Length; j++)
		{
			if (s[j] == '^')
			{
				position.Y += 18f * SpriteText.FontPixelZoom;
				position.X = 0f;
				accumulatedHorizontalSpaceBetweenCharacters = 0;
				continue;
			}
			if (j > 0)
			{
				position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(SpriteText.getWidthOffsetForChar(s[j]) + SpriteText.getWidthOffsetForChar(s[j - 1])) * SpriteText.FontPixelZoom;
			}
			accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
			if (SpriteText.positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
			{
				position.Y += 18f * SpriteText.FontPixelZoom;
				accumulatedHorizontalSpaceBetweenCharacters = 0;
				position.X = 0f;
			}
			if (position.Y >= (float)height - 16f * SpriteText.FontPixelZoom * 2f)
			{
				return s.Substring(SpriteText.getLastSpace(s, j));
			}
		}
		return "";
	}

	public static int getIndexOfSubstringBeyondHeight(string s, int width, int height)
	{
		Vector2 position = default(Vector2);
		int accumulatedHorizontalSpaceBetweenCharacters = 0;
		s = s.Replace(Environment.NewLine, "");
		SpriteText.setUpCharacterMap();
		if (!LocalizedContentManager.CurrentLanguageLatin)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '^')
				{
					position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
					position.X = 0f;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					continue;
				}
				if (SpriteText.characterMap.TryGetValue(s[i], out var c))
				{
					if (i > 0)
					{
						position.X += (float)c.XAdvance * SpriteText.FontPixelZoom;
					}
					if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
					{
						position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
						accumulatedHorizontalSpaceBetweenCharacters = 0;
						position.X = 0f;
					}
				}
				if (position.Y >= (float)height - (float)SpriteText.FontFile.Common.LineHeight * SpriteText.FontPixelZoom * 2f)
				{
					return i - 1;
				}
			}
			return s.Length - 1;
		}
		for (int j = 0; j < s.Length; j++)
		{
			if (s[j] == '^')
			{
				position.Y += 18f * SpriteText.FontPixelZoom;
				position.X = 0f;
				accumulatedHorizontalSpaceBetweenCharacters = 0;
				continue;
			}
			if (j > 0)
			{
				position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(SpriteText.getWidthOffsetForChar(s[j]) + SpriteText.getWidthOffsetForChar(s[j - 1])) * SpriteText.FontPixelZoom;
			}
			accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
			if (SpriteText.positionOfNextSpace(s, j, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= width)
			{
				position.Y += 18f * SpriteText.FontPixelZoom;
				accumulatedHorizontalSpaceBetweenCharacters = 0;
				position.X = 0f;
			}
			if (position.Y >= (float)height - 16f * SpriteText.FontPixelZoom)
			{
				return j - 1;
			}
		}
		return s.Length - 1;
	}

	public static List<string> getStringBrokenIntoSectionsOfHeight(string s, int width, int height)
	{
		List<string> brokenUp = new List<string>();
		while (s.Length > 0)
		{
			string tmp = SpriteText.getStringPreviousToThisHeightCutoff(s, width, height);
			if (tmp.Length <= 0)
			{
				break;
			}
			brokenUp.Add(tmp);
			s = s.Substring(brokenUp.Last().Length);
		}
		return brokenUp;
	}

	public static string getStringPreviousToThisHeightCutoff(string s, int width, int height)
	{
		return s.Substring(0, SpriteText.getIndexOfSubstringBeyondHeight(s, width, height) + 1);
	}

	private static int getLastSpace(string s, int startIndex)
	{
		if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
		{
			return startIndex;
		}
		for (int i = startIndex; i >= 0; i--)
		{
			if (s[i] == ' ')
			{
				return i;
			}
		}
		return startIndex;
	}

	public static int getWidthOffsetForChar(char c)
	{
		switch (c)
		{
		case ',':
		case '.':
			return -2;
		case '!':
		case 'j':
		case 'l':
		case '¡':
			return -1;
		case 'i':
		case 'ì':
		case 'í':
		case 'î':
		case 'ï':
		case 'ı':
			return -1;
		case '^':
			return -8;
		case '$':
			return 1;
		case 'ş':
			return -1;
		default:
			return 0;
		}
	}

	public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, int width, float alpha = 1f, Color? color = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
	{
		SpriteText.drawString(b, s, x - width / 2, y, 999999, width, 999999, alpha, layerDepth, junimoText, scrollType, "", color, ScrollTextAlignment.Center);
	}

	public static void drawSmallTextBubble(SpriteBatch b, string s, Vector2 positionOfBottomCenter, int maxWidth = -1, float layerDepth = -1f, bool drawPointerOnTop = false)
	{
		if (maxWidth != -1)
		{
			s = Game1.parseText(s, Game1.smallFont, maxWidth - 16);
		}
		s = s.Trim();
		Vector2 size = Game1.smallFont.MeasureString(s);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(241, 503, 9, 9), (int)(positionOfBottomCenter.X - size.X / 2f - 4f), (int)(positionOfBottomCenter.Y - size.Y), (int)size.X + 16, (int)size.Y + 12, Color.White, 4f, drawShadow: false, layerDepth);
		if (drawPointerOnTop)
		{
			b.Draw(Game1.mouseCursors_1_6, positionOfBottomCenter + new Vector2(-4f, -3f) * 4f + new Vector2(size.X / 2f, 0f - size.Y), new Rectangle(251, 506, 5, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, layerDepth + 1E-05f);
		}
		else
		{
			b.Draw(Game1.mouseCursors_1_6, positionOfBottomCenter + new Vector2(-2.5f, 1f) * 4f, new Rectangle(251, 506, 5, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 1E-05f);
		}
		Utility.drawTextWithShadow(b, s, Game1.smallFont, positionOfBottomCenter - size + new Vector2(4f + size.X / 2f, 8f), Game1.textColor, 1f, layerDepth + 2E-05f, -1, -1, 0.5f);
	}

	public static void drawStringWithScrollCenteredAt(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, Color? color = null, int scrollType = 0, float layerDepth = 0.88f, bool junimoText = false)
	{
		SpriteText.drawString(b, s, x - SpriteText.getWidthOfString((placeHolderWidthText.Length > 0) ? placeHolderWidthText : s) / 2, y, 999999, -1, 999999, alpha, layerDepth, junimoText, scrollType, placeHolderWidthText, color, ScrollTextAlignment.Center);
	}

	public static void drawStringWithScrollBackground(SpriteBatch b, string s, int x, int y, string placeHolderWidthText = "", float alpha = 1f, Color? color = null, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
	{
		SpriteText.drawString(b, s, x, y, 999999, -1, 999999, alpha, 0.88f, junimoText: false, 0, placeHolderWidthText, color, scroll_text_alignment);
	}

	private static FontFile loadFont(string assetName)
	{
		return FontLoader.Parse(Game1.content.Load<XmlSource>(assetName).Source);
	}

	private static void setUpCharacterMap()
	{
		if (!LocalizedContentManager.CurrentLanguageLatin && SpriteText.characterMap == null)
		{
			LocalizedContentManager.OnLanguageChange += OnLanguageChange;
			SpriteText.LoadFontData(LocalizedContentManager.CurrentLanguageCode);
		}
	}

	public static void drawString(SpriteBatch b, string s, int x, int y, int characterPosition = 999999, int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f, bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "", Color? color = null, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
	{
		SpriteText.setUpCharacterMap();
		bool isCustomColor = color.HasValue;
		color = color ?? SpriteText.color_Default;
		bool widthSpecified = width != -1;
		if (!widthSpecified)
		{
			width = Game1.graphics.GraphicsDevice.Viewport.Width - x;
			if (drawBGScroll == 1)
			{
				width = SpriteText.getWidthOfString(s) * 2;
			}
		}
		if (SpriteText.FontPixelZoom < 4f && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.zh)
		{
			y += (int)((4f - SpriteText.FontPixelZoom) * 4f);
		}
		Vector2 position = new Vector2(x, y);
		int accumulatedHorizontalSpaceBetweenCharacters = 0;
		if (drawBGScroll != 1)
		{
			if (position.X + (float)width > (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 4))
			{
				position.X = Game1.graphics.GraphicsDevice.Viewport.Width - width - 4;
			}
			if (position.X < 0f)
			{
				position.X = 0f;
			}
		}
		switch (drawBGScroll)
		{
		case 0:
		case 2:
		case 3:
		{
			int scroll_width = SpriteText.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
			if (widthSpecified)
			{
				scroll_width = width;
			}
			switch (drawBGScroll)
			{
			case 0:
				b.Draw(Game1.mouseCursors, position + new Vector2(-12f, -3f) * 4f, new Rectangle(325, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(337, 318, 1, 18), Color.White * alpha, 0f, Vector2.Zero, new Vector2(scroll_width, 4f), SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2(scroll_width, -12f), new Rectangle(338, 318, 12, 18), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				break;
			case 2:
				b.Draw(Game1.mouseCursors, position + new Vector2(-3f, -3f) * 4f, new Rectangle(327, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(330, 281, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(scroll_width + 4, 4f), SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors, position + new Vector2(scroll_width + 4, -12f), new Rectangle(333, 281, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				break;
			case 3:
				b.Draw(Game1.mouseCursors_1_6, position + new Vector2(-3f, -3f) * 4f, new Rectangle(86, 145, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors_1_6, position + new Vector2(0f, -3f) * 4f, new Rectangle(89, 145, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(scroll_width + 4, 4f), SpriteEffects.None, layerDepth - 0.001f);
				b.Draw(Game1.mouseCursors_1_6, position + new Vector2(scroll_width + 4, -12f), new Rectangle(92, 145, 3, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
				break;
			}
			switch (scroll_text_alignment)
			{
			case ScrollTextAlignment.Center:
				x += (scroll_width - SpriteText.getWidthOfString(s)) / 2;
				position.X = x;
				break;
			case ScrollTextAlignment.Right:
				x += scroll_width - SpriteText.getWidthOfString(s);
				position.X = x;
				break;
			}
			position.Y += (4f - SpriteText.FontPixelZoom) * 4f;
			break;
		}
		case 1:
		{
			int text_width = SpriteText.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s);
			Vector2 speech_position = position;
			if (Game1.currentLocation?.map?.Layers[0] != null)
			{
				int left_edge = -Game1.viewport.X + 28;
				int right_edge = -Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 28;
				if (position.X < (float)left_edge)
				{
					position.X = left_edge;
				}
				if (position.X + (float)text_width > (float)right_edge)
				{
					position.X = right_edge - text_width;
				}
				speech_position.X += text_width / 2;
				if (speech_position.X < position.X)
				{
					position.X += speech_position.X - position.X;
				}
				if (speech_position.X > position.X + (float)text_width - 24f)
				{
					position.X += speech_position.X - (position.X + (float)text_width - 24f);
				}
				speech_position.X = Utility.Clamp(speech_position.X, position.X, position.X + (float)text_width - 24f);
			}
			b.Draw(Game1.mouseCursors, position + new Vector2(-7f, -3f) * 4f, new Rectangle(324, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
			b.Draw(Game1.mouseCursors, position + new Vector2(0f, -3f) * 4f, new Rectangle(331, 299, 1, 17), Color.White * alpha, 0f, Vector2.Zero, new Vector2(SpriteText.getWidthOfString((placeHolderScrollWidthText.Length > 0) ? placeHolderScrollWidthText : s), 4f), SpriteEffects.None, layerDepth - 0.001f);
			b.Draw(Game1.mouseCursors, position + new Vector2(text_width, -12f), new Rectangle(332, 299, 7, 17), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.001f);
			b.Draw(Game1.mouseCursors, speech_position + new Vector2(0f, 52f), new Rectangle(341, 308, 6, 5), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth - 0.0001f);
			x = (int)position.X;
			if (placeHolderScrollWidthText.Length > 0)
			{
				x += SpriteText.getWidthOfString(placeHolderScrollWidthText) / 2 - SpriteText.getWidthOfString(s) / 2;
				position.X = x;
			}
			position.Y += (4f - SpriteText.FontPixelZoom) * 4f;
			break;
		}
		}
		if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
		{
			position.Y -= 8f;
		}
		if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
		{
			if (drawBGScroll != -1)
			{
				float factor = 3.5f;
				if (Game1.options.useChineseSmoothFont)
				{
					position.Y -= 2f;
					factor = 3.8f;
				}
				else
				{
					position.Y += 4f;
				}
				position.Y -= (SpriteText.FontPixelZoom - 0.75f) * 4f * factor;
			}
			else
			{
				position.Y += 4f;
			}
		}
		s = s.Replace(Environment.NewLine, "");
		if (!junimoText && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage.FontApplyYOffset)))
		{
			position.Y -= (4f - SpriteText.FontPixelZoom) * 4f;
		}
		s = s.Replace('♡', '<');
		for (int i = 0; i < Math.Min(s.Length, characterPosition); i++)
		{
			if (LocalizedContentManager.CurrentLanguageLatin || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru && !Game1.options.useAlternateFont) || SpriteText.IsSpecialCharacter(s[i]) || junimoText || SpriteText.forceEnglishFont)
			{
				float tempzoom = SpriteText.fontPixelZoom;
				if (SpriteText.IsSpecialCharacter(s[i]) || junimoText || SpriteText.forceEnglishFont)
				{
					SpriteText.fontPixelZoom = 3f;
				}
				if (s[i] == '^')
				{
					position.Y += 18f * SpriteText.FontPixelZoom;
					position.X = x;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					SpriteText.fontPixelZoom = tempzoom;
					continue;
				}
				accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
				bool upper = char.IsUpper(s[i]) || s[i] == 'ß';
				Vector2 spriteFontOffset = new Vector2(0f, -1 + ((!junimoText && upper) ? (-3) : 0));
				if (s[i] == 'Ç')
				{
					spriteFontOffset.Y += 2f;
				}
				if (SpriteText.positionOfNextSpace(s, i, (int)position.X - x, accumulatedHorizontalSpaceBetweenCharacters) >= width)
				{
					position.Y += 18f * SpriteText.FontPixelZoom;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					position.X = x;
					if (s[i] == ' ')
					{
						SpriteText.fontPixelZoom = tempzoom;
						continue;
					}
				}
				Rectangle srcRect = SpriteText.getSourceRectForChar(s[i], junimoText);
				b.Draw(isCustomColor ? SpriteText.coloredTexture : SpriteText.spriteTexture, position + spriteFontOffset * SpriteText.FontPixelZoom, srcRect, ((SpriteText.IsSpecialCharacter(s[i]) || junimoText) ? Color.White : color.Value) * alpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
				if (i < s.Length - 1)
				{
					position.X += 8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)SpriteText.getWidthOffsetForChar(s[i + 1]) * SpriteText.FontPixelZoom;
				}
				if (s[i] != '^')
				{
					position.X += (float)SpriteText.getWidthOffsetForChar(s[i]) * SpriteText.FontPixelZoom;
				}
				SpriteText.fontPixelZoom = tempzoom;
				continue;
			}
			if (s[i] == '^')
			{
				position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
				position.X = x;
				accumulatedHorizontalSpaceBetweenCharacters = 0;
				continue;
			}
			if (i > 0 && SpriteText.IsSpecialCharacter(s[i - 1]))
			{
				position.X += 24f;
			}
			if (SpriteText.characterMap.TryGetValue(s[i], out var fc))
			{
				Rectangle sourcerect = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
				Texture2D _texture = SpriteText.fontPages[fc.Page];
				if (SpriteText.positionOfNextSpace(s, i, (int)position.X, accumulatedHorizontalSpaceBetweenCharacters) >= x + width - 4)
				{
					position.Y += (float)(SpriteText.FontFile.Common.LineHeight + 2) * SpriteText.FontPixelZoom;
					accumulatedHorizontalSpaceBetweenCharacters = 0;
					position.X = x;
				}
				Vector2 position2 = new Vector2(position.X + (float)fc.XOffset * SpriteText.FontPixelZoom, position.Y + (float)fc.YOffset * SpriteText.FontPixelZoom);
				if (drawBGScroll != -1 && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
				{
					position2.Y -= 8f;
				}
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
				{
					Vector2 offset = new Vector2(-1f, 1f) * SpriteText.FontPixelZoom;
					b.Draw(_texture, position2 + offset, sourcerect, color.Value * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
					b.Draw(_texture, position2 + new Vector2(0f, offset.Y), sourcerect, color.Value * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
					b.Draw(_texture, position2 + new Vector2(offset.X, 0f), sourcerect, color.Value * alpha * SpriteText.shadowAlpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
				}
				b.Draw(_texture, position2, sourcerect, color.Value * alpha, 0f, Vector2.Zero, SpriteText.FontPixelZoom, SpriteEffects.None, layerDepth);
				position.X += (float)fc.XAdvance * SpriteText.FontPixelZoom;
			}
		}
	}

	private static bool IsSpecialCharacter(char c)
	{
		if (!c.Equals('<') && !c.Equals('=') && !c.Equals('>') && !c.Equals('@') && !c.Equals('$') && !c.Equals('`'))
		{
			return c.Equals('+');
		}
		return true;
	}

	private static void OnLanguageChange(LocalizedContentManager.LanguageCode code)
	{
		SpriteText.LoadFontData(code);
	}

	public static void LoadFontData(LocalizedContentManager.LanguageCode code)
	{
		if (SpriteText.characterMap != null)
		{
			SpriteText.characterMap.Clear();
		}
		else
		{
			SpriteText.characterMap = new Dictionary<char, FontChar>();
		}
		if (SpriteText.fontPages != null)
		{
			SpriteText.fontPages.Clear();
		}
		else
		{
			SpriteText.fontPages = new List<Texture2D>();
		}
		string pathBase = "Fonts\\";
		switch (code)
		{
		case LocalizedContentManager.LanguageCode.ja:
			SpriteText.FontFile = SpriteText.loadFont(pathBase + "Japanese");
			SpriteText.fontPixelZoom = 1.75f;
			break;
		case LocalizedContentManager.LanguageCode.zh:
			if (Game1.options.useChineseSmoothFont)
			{
				pathBase += "Chinese_round\\";
				SpriteText.fontPixelZoom = 1f;
			}
			else
			{
				SpriteText.fontPixelZoom = 1.5f;
			}
			SpriteText.FontFile = SpriteText.loadFont(pathBase + "Chinese");
			break;
		case LocalizedContentManager.LanguageCode.ru:
			SpriteText.FontFile = SpriteText.loadFont(pathBase + "Russian");
			SpriteText.fontPixelZoom = 3f;
			break;
		case LocalizedContentManager.LanguageCode.th:
			SpriteText.FontFile = SpriteText.loadFont(pathBase + "Thai");
			SpriteText.fontPixelZoom = 1.5f;
			break;
		case LocalizedContentManager.LanguageCode.ko:
			SpriteText.FontFile = SpriteText.loadFont(pathBase + "Korean");
			SpriteText.fontPixelZoom = 1.5f;
			break;
		case LocalizedContentManager.LanguageCode.mod:
			SpriteText.FontFile = SpriteText.loadFont(LocalizedContentManager.CurrentModLanguage.FontFile);
			SpriteText.fontPixelZoom = LocalizedContentManager.CurrentModLanguage.FontPixelZoom;
			break;
		default:
			SpriteText.FontFile = null;
			SpriteText.fontPixelZoom = 3f;
			break;
		}
		if (SpriteText.FontFile == null)
		{
			return;
		}
		foreach (FontChar fontCharacter in SpriteText.FontFile.Chars)
		{
			char c = (char)fontCharacter.ID;
			SpriteText.characterMap.Add(c, fontCharacter);
		}
		foreach (FontPage fontPage in SpriteText.FontFile.Pages)
		{
			SpriteText.fontPages.Add(Game1.content.Load<Texture2D>(pathBase + fontPage.File));
		}
	}

	public static int positionOfNextSpace(string s, int index, int currentXPosition, int accumulatedHorizontalSpaceBetweenCharacters)
	{
		SpriteText.setUpCharacterMap();
		LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
		if (currentLanguageCode == LocalizedContentManager.LanguageCode.ja || currentLanguageCode == LocalizedContentManager.LanguageCode.zh || currentLanguageCode == LocalizedContentManager.LanguageCode.th)
		{
			float result = currentXPosition;
			string value = Game1.asianSpacingRegex.Match(s, index).Value;
			foreach (char c in value)
			{
				if (SpriteText.characterMap.TryGetValue(c, out var fc))
				{
					result += (float)fc.XAdvance * SpriteText.FontPixelZoom;
				}
			}
			return (int)result;
		}
		for (int j = index; j < s.Length; j++)
		{
			if (SpriteText.isUsingNonSpriteSheetFont())
			{
				if (s[j] == ' ' || s[j] == '^')
				{
					return currentXPosition;
				}
				currentXPosition = ((!SpriteText.characterMap.TryGetValue(s[j], out var fc2)) ? (currentXPosition + (int)((float)SpriteText.FontFile.Common.LineHeight * SpriteText.FontPixelZoom)) : (currentXPosition + (int)((float)fc2.XAdvance * SpriteText.FontPixelZoom)));
				continue;
			}
			if (s[j] == ' ' || s[j] == '^')
			{
				return currentXPosition;
			}
			currentXPosition += (int)(8f * SpriteText.FontPixelZoom + (float)accumulatedHorizontalSpaceBetweenCharacters + (float)(SpriteText.getWidthOffsetForChar(s[j]) + SpriteText.getWidthOffsetForChar(s[Math.Max(0, j - 1)])) * SpriteText.FontPixelZoom);
			accumulatedHorizontalSpaceBetweenCharacters = (int)(0f * SpriteText.FontPixelZoom);
		}
		return currentXPosition;
	}

	private static bool isUsingNonSpriteSheetFont()
	{
		if (!LocalizedContentManager.CurrentLanguageLatin)
		{
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
			{
				return Game1.options.useAlternateFont;
			}
			return true;
		}
		return false;
	}

	private static Rectangle getSourceRectForChar(char c, bool junimoText)
	{
		int i = c - 32;
		switch (c)
		{
		case 'Œ':
			i = 96;
			break;
		case 'œ':
			i = 97;
			break;
		case 'Ğ':
			i = 102;
			break;
		case 'ğ':
			i = 103;
			break;
		case 'İ':
			i = 98;
			break;
		case 'ı':
			i = 99;
			break;
		case 'Ş':
			i = 100;
			break;
		case 'ş':
			i = 101;
			break;
		case '’':
			i = 104;
			break;
		case 'Ő':
			i = 105;
			break;
		case 'ő':
			i = 106;
			break;
		case 'Ű':
			i = 107;
			break;
		case 'ű':
			i = 108;
			break;
		case 'ё':
			i = 560;
			break;
		case 'ґ':
			i = 561;
			break;
		case 'є':
			i = 562;
			break;
		case 'і':
			i = 563;
			break;
		case 'ї':
			i = 564;
			break;
		case 'ў':
			i = 565;
			break;
		case 'Ё':
			i = 512;
			break;
		case '–':
			i = 464;
			break;
		case '—':
			i = 465;
			break;
		case '№':
			i = 466;
			break;
		case 'Ґ':
			i = 513;
			break;
		case 'Є':
			i = 514;
			break;
		case 'І':
			i = 515;
			break;
		case 'Ї':
			i = 516;
			break;
		case 'Ў':
			i = 517;
			break;
		case 'Ą':
			i = 576;
			break;
		case 'ą':
			i = 578;
			break;
		case 'Ć':
			i = 579;
			break;
		case 'ć':
			i = 580;
			break;
		case 'Ę':
			i = 581;
			break;
		case 'ę':
			i = 582;
			break;
		case 'Ł':
			i = 583;
			break;
		case 'ł':
			i = 584;
			break;
		case 'Ń':
			i = 585;
			break;
		case 'ń':
			i = 586;
			break;
		case 'Ź':
			i = 587;
			break;
		case 'ź':
			i = 588;
			break;
		case 'Ż':
			i = 589;
			break;
		case 'ż':
			i = 590;
			break;
		case 'Ś':
			i = 574;
			break;
		case 'ś':
			i = 575;
			break;
		default:
			if (i >= 1008 && i < 1040)
			{
				i -= 528;
			}
			else if (i >= 1040 && i < 1072)
			{
				i -= 512;
			}
			break;
		}
		return new Rectangle(i * 8 % SpriteText.spriteTexture.Width, i * 8 / SpriteText.spriteTexture.Width * 16 + (junimoText ? 224 : 0), 8, 16);
	}
}
