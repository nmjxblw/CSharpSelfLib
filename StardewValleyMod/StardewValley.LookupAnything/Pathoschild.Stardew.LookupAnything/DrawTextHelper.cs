using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.LookupAnything;

internal static class DrawTextHelper
{
	private static string? LastLanguage;

	private static readonly HashSet<char> SoftBreakCharacters = new HashSet<char>();

	public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, string? text, Vector2 position, float wrapWidth, Color? color = null, bool bold = false, float scale = 1f)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return batch.DrawTextBlock(font, new _003C_003Ez__ReadOnlySingleElementList<IFormattedText>(new FormattedText(text, color, bold)), position, wrapWidth, scale);
	}

	public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, IEnumerable<IFormattedText?>? text, Vector2 position, float wrapWidth, float scale = 1f)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		if (text == null)
		{
			return new Vector2(0f, 0f);
		}
		DrawTextHelper.InitIfNeeded();
		float xOffset = 0f;
		float yOffset = 0f;
		float lineHeight = font.MeasureString("ABC").Y * scale;
		float spaceWidth = DrawHelper.GetSpaceWidth(font) * scale;
		float blockWidth = 0f;
		float blockHeight = lineHeight;
		DrawTextHelper.InitIfNeeded();
		Vector2 wordPosition = default(Vector2);
		foreach (IFormattedText snippet in text)
		{
			if (snippet == null || snippet.Text == null)
			{
				continue;
			}
			string[] rawWords = snippet.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (rawWords.Length != 0)
			{
				if (snippet.Text.StartsWith(" "))
				{
					rawWords[0] = " " + rawWords[0];
				}
				if (snippet.Text.EndsWith(" "))
				{
					rawWords[^1] += " ";
				}
			}
			bool isFirstOfLine = true;
			string[] array = rawWords;
			foreach (string rawWord in array)
			{
				string[] explicitLineBreaks = rawWord.Split('\n');
				if (explicitLineBreaks.Length > 1)
				{
					for (int j = 0; j < explicitLineBreaks.Length; j++)
					{
						explicitLineBreaks[j] = explicitLineBreaks[j].TrimEnd('\r');
					}
				}
				for (int k = 0; k < explicitLineBreaks.Length; k++)
				{
					if (k > 0)
					{
						xOffset = 0f;
						yOffset += lineHeight;
						blockHeight += lineHeight;
						isFirstOfLine = true;
					}
					bool isStartOfWord = true;
					foreach (string wordPart in DrawTextHelper.SplitWithinWordForLineWrapping(explicitLineBreaks[k]))
					{
						float wordWidth = font.MeasureString(wordPart).X * scale;
						float prependSpace = ((isStartOfWord && !isFirstOfLine) ? spaceWidth : 0f);
						if (wordPart == Environment.NewLine || (wordWidth + xOffset + prependSpace > wrapWidth && (int)xOffset != 0))
						{
							xOffset = 0f;
							yOffset += lineHeight;
							blockHeight += lineHeight;
							isFirstOfLine = true;
						}
						if (!(wordPart == Environment.NewLine))
						{
							((Vector2)(ref wordPosition))._002Ector(position.X + xOffset + prependSpace, position.Y + yOffset);
							if (snippet.Bold)
							{
								Utility.drawBoldText(batch, wordPart, font, wordPosition, (Color)(((_003F?)snippet.Color) ?? Color.Black), scale, -1f, 1);
							}
							else
							{
								batch.DrawString(font, wordPart, wordPosition, (Color)(((_003F?)snippet.Color) ?? Color.Black), 0f, Vector2.Zero, scale, (SpriteEffects)0, 1f);
							}
							if (xOffset + wordWidth + prependSpace > blockWidth)
							{
								blockWidth = xOffset + wordWidth + prependSpace;
							}
							xOffset += wordWidth + prependSpace;
							isFirstOfLine = false;
							isStartOfWord = false;
						}
					}
				}
			}
		}
		return new Vector2(blockWidth, blockHeight);
	}

	public static void InitIfNeeded()
	{
		string language = LocalizedContentManager.CurrentLanguageString;
		if (DrawTextHelper.LastLanguage != language)
		{
			string characters = Translation.op_Implicit(I18n.GetByKey("generic.line-wrap-on").UsePlaceholder(false));
			DrawTextHelper.SoftBreakCharacters.Clear();
			if (!string.IsNullOrEmpty(characters))
			{
				CollectionExtensions.AddRange<char>((ISet<char>)DrawTextHelper.SoftBreakCharacters, (IEnumerable<char>)characters);
			}
			DrawTextHelper.LastLanguage = language;
		}
	}

	private static IList<string> SplitWithinWordForLineWrapping(string text)
	{
		HashSet<char> splitChars = DrawTextHelper.SoftBreakCharacters;
		string newLine = Environment.NewLine;
		List<string> words = new List<string>();
		int start = 0;
		for (int i = 0; i < text.Length; i++)
		{
			char ch = text[i];
			if (ch == newLine[0] && DrawTextHelper.IsNewlineAt(text, i))
			{
				if (i > start)
				{
					words.Add(text.Substring(start, i - start));
				}
				words.Add(newLine);
				i += newLine.Length;
				start = i;
			}
			else if (splitChars.Contains(ch))
			{
				words.Add(text.Substring(start, i - start + 1));
				start = i + 1;
			}
		}
		if (start == 0)
		{
			words.Add(text);
		}
		else if (start < text.Length - 1)
		{
			words.Add(text.Substring(start));
		}
		return words;
	}

	private static bool IsNewlineAt(string text, int index)
	{
		string newline = Environment.NewLine;
		int i = index;
		int n = 0;
		while (i < text.Length && n < newline.Length)
		{
			if (text[i] != newline[n])
			{
				return false;
			}
			i++;
			n++;
		}
		return true;
	}
}
