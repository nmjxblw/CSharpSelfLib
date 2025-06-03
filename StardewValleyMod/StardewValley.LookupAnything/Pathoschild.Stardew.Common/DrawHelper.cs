using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.Common;

internal static class DrawHelper
{
	public static float GetSpaceWidth(SpriteFont font)
	{
		return CommonHelper.GetSpaceWidth(font);
	}

	public static void DrawSprite(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Vector2 errorSize, Color? color = null, float scale = 1f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			spriteBatch.Draw(sheet, new Vector2(x, y), (Rectangle?)sprite, (Color)(((_003F?)color) ?? Color.White), 0f, Vector2.Zero, scale, (SpriteEffects)0, 0f);
		}
		catch
		{
			Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, (int)errorSize.X, (int)errorSize.Y), 0f);
		}
	}

	public static void DrawSprite(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Point errorSize, Color? color = null, float scale = 1f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			spriteBatch.Draw(sheet, new Vector2(x, y), (Rectangle?)sprite, (Color)(((_003F?)color) ?? Color.White), 0f, Vector2.Zero, scale, (SpriteEffects)0, 0f);
		}
		catch
		{
			Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, errorSize.X, errorSize.Y), 0f);
		}
	}

	public static void DrawSpriteWithin(this SpriteBatch spriteBatch, SpriteInfo? sprite, float x, float y, Vector2 size, Color? color = null)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			sprite?.Draw(spriteBatch, (int)x, (int)y, size, color);
		}
		catch
		{
			Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), 0f);
		}
	}

	public static void DrawSpriteWithin(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Vector2 size, Color? color = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		float largestDimension = Math.Max(sprite.Width, sprite.Height);
		float scale = size.X / largestDimension;
		float leftOffset = Math.Max((size.X - (float)sprite.Width * scale) / 2f, 0f);
		float topOffset = Math.Max((size.Y - (float)sprite.Height * scale) / 2f, 0f);
		spriteBatch.DrawSprite(sheet, sprite, x + leftOffset, y + topOffset, size, (Color)(((_003F?)color) ?? Color.White), scale);
	}

	public static void DrawLine(this SpriteBatch batch, float x, float y, Vector2 size, Color? color = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		batch.Draw(CommonHelper.Pixel, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), (Color)(((_003F?)color) ?? Color.White));
	}
}
