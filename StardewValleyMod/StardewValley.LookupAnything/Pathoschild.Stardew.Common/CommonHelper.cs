using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common;

internal static class CommonHelper
{
	private static readonly Lazy<Texture2D> LazyPixel = new Lazy<Texture2D>((Func<Texture2D>)delegate
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
		val.SetData<Color>((Color[])(object)new Color[1] { Color.White });
		return val;
	});

	public const int ButtonBorderWidth = 16;

	public static readonly Vector2 ScrollEdgeSize = new Vector2((float)(CommonSprites.Scroll.TopLeft.Width * 4), (float)(CommonSprites.Scroll.TopLeft.Height * 4));

	public static Texture2D Pixel => CommonHelper.LazyPixel.Value;

	public static IEnumerable<TValue> GetEnumValues<TValue>() where TValue : struct
	{
		return Enum.GetValues(typeof(TValue)).Cast<TValue>();
	}

	public static IEnumerable<GameLocation> GetLocations(bool includeTempLevels = false)
	{
		IEnumerable<GameLocation> locations = Game1.locations.Concat(from location in Game1.locations
			from indoors in location.GetInstancedBuildingInteriors()
			select indoors);
		if (includeTempLevels)
		{
			locations = locations.Concat((IEnumerable<GameLocation>)MineShaft.activeMines).Concat((IEnumerable<GameLocation>)VolcanoDungeon.activeLevels);
		}
		return locations;
	}

	public static Vector2 GetPlayerTile(Farmer? player)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 position = ((player != null) ? ((Character)player).Position : Vector2.Zero);
		return new Vector2((float)(int)(position.X / 64f), (float)(int)(position.Y / 64f));
	}

	public static bool IsItemId(string itemId, bool allowZero = true)
	{
		if (!string.IsNullOrWhiteSpace(itemId))
		{
			if (int.TryParse(itemId, out var id))
			{
				return id >= ((!allowZero) ? 1 : 0);
			}
			return true;
		}
		return false;
	}

	public static string FormatTime(int time)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		string timeStr = Game1.getTimeOfDayString(time);
		LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
		if (currentLanguageCode - 9 <= 1)
		{
			LocalizedContentManager content = Game1.content;
			bool flag = ((time < 1200 || time >= 2400) ? true : false);
			string amOrPm = content.LoadString(flag ? "Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370" : "Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371");
			timeStr += (((int)LocalizedContentManager.CurrentLanguageCode == 9) ? amOrPm : (" " + amOrPm));
		}
		return timeStr;
	}

	public static float GetSpaceWidth(SpriteFont font)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return font.MeasureString("A B").X - font.MeasureString("AB").X;
	}

	public static string GetFormattedPercentageNumber(float chance)
	{
		float percent = chance * 100f;
		if (!(percent <= 0f))
		{
			if (!(percent >= 100f))
			{
				if (!(percent < 1f))
				{
					if (percent < 1.95f)
					{
						return percent.ToString("#.#");
					}
					return percent.ToString("#");
				}
				for (int precision = 3; precision < 28; precision++)
				{
					decimal result = Math.Round((decimal)percent, precision);
					if (result > 0m)
					{
						return result.ToString("0.".PadRight(precision + 2, '#'));
					}
				}
				return percent.ToString(CultureInfo.InvariantCulture);
			}
			return "100";
		}
		return "0";
	}

	public static IModInfo? TryGetModFromStringId(IModRegistry modRegistry, string? id, bool allowModOnlyId = false)
	{
		if (id == null)
		{
			return null;
		}
		IModInfo mod = null;
		if (allowModOnlyId)
		{
			mod = modRegistry.Get(id);
			if (mod != null)
			{
				return mod;
			}
		}
		string[] parts = id.Split('_');
		if (parts.Length == 1)
		{
			return null;
		}
		string modId = parts[0];
		int itemIdIndex = parts.Length - 1;
		for (int i = 0; i < itemIdIndex; i++)
		{
			if (i != 0)
			{
				modId = modId + "_" + parts[i];
			}
			mod = modRegistry.Get(modId) ?? mod;
		}
		return mod;
	}

	public static void Draw(this SpriteBatch batch, Texture2D sheet, Rectangle sprite, int x, int y, int width, int height, Color? color = null)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		batch.Draw(sheet, new Rectangle(x, y, width, height), (Rectangle?)sprite, (Color)(((_003F?)color) ?? Color.White));
	}

	public static Vector2 DrawHoverBox(SpriteBatch spriteBatch, string label, in Vector2 position, float wrapWidth)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		Vector2 labelSize = spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(20f), wrapWidth, (Color?)null);
		IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)position.X, (int)position.Y, (int)labelSize.X + 27 + 20, (int)labelSize.Y + 27, Color.White, 1f, true, -1f);
		spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(20f), wrapWidth, (Color?)null);
		return labelSize + new Vector2(27f);
	}

	public static void DrawTab(SpriteBatch spriteBatch, int x, int y, int innerWidth, int innerHeight, out Vector2 innerDrawPosition, int align = 0, float alpha = 1f, bool forIcon = false, bool drawShadow = true)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		int outerWidth = innerWidth + 32;
		int outerHeight = innerHeight + 21;
		int offsetX = align switch
		{
			1 => -outerWidth / 2, 
			2 => -outerWidth, 
			_ => 0, 
		};
		int iconOffsetX = (forIcon ? (-4) : 0);
		int iconOffsetY = (forIcon ? (-8) : 0);
		innerDrawPosition = new Vector2((float)(x + 16 + offsetX + iconOffsetX), (float)(y + 16 + iconOffsetY));
		IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x + offsetX, y, outerWidth, outerHeight + 4, Color.White * alpha, 1f, drawShadow, -1f);
	}

	public static void DrawButton(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 0)
	{
		CommonHelper.DrawContentBox(spriteBatch, CommonSprites.Button.Sheet, in CommonSprites.Button.Background, in CommonSprites.Button.Top, in CommonSprites.Button.Right, in CommonSprites.Button.Bottom, in CommonSprites.Button.Left, in CommonSprites.Button.TopLeft, in CommonSprites.Button.TopRight, in CommonSprites.Button.BottomRight, in CommonSprites.Button.BottomLeft, in position, in contentSize, out contentPos, out bounds, padding);
	}

	public static void DrawScroll(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 5)
	{
		CommonHelper.DrawContentBox(spriteBatch, CommonSprites.Scroll.Sheet, in CommonSprites.Scroll.Background, in CommonSprites.Scroll.Top, in CommonSprites.Scroll.Right, in CommonSprites.Scroll.Bottom, in CommonSprites.Scroll.Left, in CommonSprites.Scroll.TopLeft, in CommonSprites.Scroll.TopRight, in CommonSprites.Scroll.BottomRight, in CommonSprites.Scroll.BottomLeft, in position, in contentSize, out contentPos, out bounds, padding);
	}

	public static void DrawContentBox(SpriteBatch spriteBatch, Texture2D texture, in Rectangle background, in Rectangle top, in Rectangle right, in Rectangle bottom, in Rectangle left, in Rectangle topLeft, in Rectangle topRight, in Rectangle bottomRight, in Rectangle bottomLeft, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		CommonHelper.GetContentBoxDimensions(topLeft, contentSize, padding, out var innerWidth, out var innerHeight, out var outerWidth, out var outerHeight, out var cornerWidth, out var cornerHeight);
		int x = (int)position.X;
		int y = (int)position.Y;
		spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y + cornerHeight, innerWidth, innerHeight), (Rectangle?)background, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y, innerWidth, cornerHeight), (Rectangle?)top, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x + cornerWidth, y + cornerHeight + innerHeight, innerWidth, cornerHeight), (Rectangle?)bottom, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x, y + cornerHeight, cornerWidth, innerHeight), (Rectangle?)left, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight, cornerWidth, innerHeight), (Rectangle?)right, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x, y, cornerWidth, cornerHeight), (Rectangle?)topLeft, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), (Rectangle?)bottomLeft, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y, cornerWidth, cornerHeight), (Rectangle?)topRight, Color.White);
		spriteBatch.Draw(texture, new Rectangle(x + cornerWidth + innerWidth, y + cornerHeight + innerHeight, cornerWidth, cornerHeight), (Rectangle?)bottomRight, Color.White);
		contentPos = new Vector2((float)(x + cornerWidth + padding), (float)(y + cornerHeight + padding));
		bounds = new Rectangle(x, y, outerWidth, outerHeight);
	}

	public static void ShowInfoMessage(string message, int? duration = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		Game1.addHUDMessage(new HUDMessage(message, 3)
		{
			noIcon = true,
			timeLeft = (((float?)duration) ?? 3500f)
		});
	}

	public static void ShowErrorMessage(string message)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		Game1.addHUDMessage(new HUDMessage(message, 3));
	}

	public static void GetScrollDimensions(Vector2 contentSize, int padding, out int innerWidth, out int innerHeight, out int labelOuterWidth, out int outerHeight, out int borderWidth, out int borderHeight)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		CommonHelper.GetContentBoxDimensions(CommonSprites.Scroll.TopLeft, contentSize, padding, out innerWidth, out innerHeight, out labelOuterWidth, out outerHeight, out borderWidth, out borderHeight);
	}

	public static void GetContentBoxDimensions(Rectangle topLeft, Vector2 contentSize, int padding, out int innerWidth, out int innerHeight, out int outerWidth, out int outerHeight, out int borderWidth, out int borderHeight)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		borderWidth = topLeft.Width * 4;
		borderHeight = topLeft.Height * 4;
		innerWidth = (int)(contentSize.X + (float)(padding * 2));
		innerHeight = (int)(contentSize.Y + (float)(padding * 2));
		outerWidth = innerWidth + borderWidth * 2;
		outerHeight = innerHeight + borderHeight * 2;
	}

	public static void DrawLine(this SpriteBatch batch, float x, float y, in Vector2 size, in Color? color = null)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		batch.Draw(CommonHelper.Pixel, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), (Color)(((_003F?)color) ?? Color.White));
	}

	public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, string? text, in Vector2 position, float wrapWidth, in Color? color = null, bool bold = false, float scale = 1f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		if (text == null)
		{
			return new Vector2(0f, 0f);
		}
		List<string> words = new List<string>();
		string[] array = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		foreach (string word in array)
		{
			string wordPart = word;
			int newlineIndex;
			while ((newlineIndex = wordPart.IndexOf(Environment.NewLine, StringComparison.Ordinal)) >= 0)
			{
				if (newlineIndex == 0)
				{
					words.Add(Environment.NewLine);
					wordPart = wordPart.Substring(Environment.NewLine.Length);
				}
				else if (newlineIndex > 0)
				{
					words.Add(wordPart.Substring(0, newlineIndex));
					words.Add(Environment.NewLine);
					wordPart = wordPart.Substring(newlineIndex + Environment.NewLine.Length);
				}
			}
			if (wordPart.Length > 0)
			{
				words.Add(wordPart);
			}
		}
		float xOffset = 0f;
		float yOffset = 0f;
		float lineHeight = font.MeasureString("ABC").Y * scale;
		float spaceWidth = CommonHelper.GetSpaceWidth(font) * scale;
		float blockWidth = 0f;
		float blockHeight = lineHeight;
		Vector2 wordPosition = default(Vector2);
		foreach (string word2 in words)
		{
			float wordWidth = font.MeasureString(word2).X * scale;
			if (word2 == Environment.NewLine || (wordWidth + xOffset > wrapWidth && (int)xOffset != 0))
			{
				xOffset = 0f;
				yOffset += lineHeight;
				blockHeight += lineHeight;
			}
			if (!(word2 == Environment.NewLine))
			{
				((Vector2)(ref wordPosition))._002Ector(position.X + xOffset, position.Y + yOffset);
				if (bold)
				{
					Utility.drawBoldText(batch, word2, font, wordPosition, (Color)(((_003F?)color) ?? Color.Black), scale, -1f, 1);
				}
				else
				{
					batch.DrawString(font, word2, wordPosition, (Color)(((_003F?)color) ?? Color.Black), 0f, Vector2.Zero, scale, (SpriteEffects)0, 1f);
				}
				if (xOffset + wordWidth > blockWidth)
				{
					blockWidth = xOffset + wordWidth;
				}
				xOffset += wordWidth + spaceWidth;
			}
		}
		return new Vector2(blockWidth, blockHeight);
	}

	public static void InterceptErrors(this IMonitor monitor, string verb, Action action, Action<Exception>? onError = null)
	{
		monitor.InterceptErrors(verb, null, action, onError);
	}

	public static void InterceptErrors(this IMonitor monitor, string verb, string? detailedVerb, Action action, Action<Exception>? onError = null)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			monitor.InterceptError(ex, verb, detailedVerb);
			onError?.Invoke(ex);
		}
	}

	public static void InterceptError(this IMonitor monitor, Exception ex, string verb, string? detailedVerb = null)
	{
		if (detailedVerb == null)
		{
			detailedVerb = verb;
		}
		monitor.Log($"Something went wrong {detailedVerb}:\n{ex}", (LogLevel)4);
		CommonHelper.ShowErrorMessage("Huh. Something went wrong " + verb + ". The error log has the technical details.");
	}

	public static void RemoveObsoleteFiles(IMod mod, params string[] relativePaths)
	{
		string basePath = mod.Helper.DirectoryPath;
		foreach (string relativePath in relativePaths)
		{
			string fullPath = Path.Combine(basePath, relativePath);
			if (File.Exists(fullPath))
			{
				try
				{
					File.Delete(fullPath);
					mod.Monitor.Log("Removed obsolete file '" + relativePath + "'.", (LogLevel)0);
				}
				catch (Exception value)
				{
					mod.Monitor.Log($"Failed deleting obsolete file '{relativePath}':\n{value}", (LogLevel)0);
				}
			}
		}
	}

	public static string GetFileHash(string absolutePath)
	{
		using FileStream stream = File.OpenRead(absolutePath);
		using MD5 md5 = MD5.Create();
		byte[] hash = md5.ComputeHash(stream);
		return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
	}
}
