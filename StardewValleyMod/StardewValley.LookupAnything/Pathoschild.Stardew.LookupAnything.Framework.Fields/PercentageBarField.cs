using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Components;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class PercentageBarField : GenericField
{
	protected readonly int CurrentValue;

	protected readonly int MaxValue;

	protected readonly string? Text;

	protected readonly Color FilledColor;

	protected readonly Color EmptyColor;

	public PercentageBarField(string label, int currentValue, int maxValue, Color filledColor, Color emptyColor, string? text)
		: base(label, hasValue: true)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		this.CurrentValue = currentValue;
		this.MaxValue = maxValue;
		this.FilledColor = filledColor;
		this.EmptyColor = emptyColor;
		this.Text = text;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Vector2 barSize = this.DrawBar(spriteBatch, position, (float)this.CurrentValue / ((float)this.MaxValue * 1f), this.FilledColor, this.EmptyColor, wrapWidth);
		Vector2 textSize = ((!string.IsNullOrWhiteSpace(this.Text)) ? spriteBatch.DrawTextBlock(font, this.Text, new Vector2(position.X + barSize.X + 3f, position.Y), wrapWidth) : Vector2.Zero);
		return new Vector2(barSize.X + 3f + textSize.X, Math.Max(barSize.Y, textSize.Y));
	}

	protected Vector2 DrawBar(SpriteBatch spriteBatch, Vector2 position, float ratio, Color filledColor, Color emptyColor, float maxWidth = 100f)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		int barHeight = 22;
		ratio = Math.Min(1f, ratio);
		float width = Math.Min(100f, maxWidth);
		float filledWidth = width * ratio;
		float emptyWidth = width - filledWidth;
		if (filledWidth > 0f)
		{
			spriteBatch.Draw(Sprites.Pixel, new Rectangle((int)position.X, (int)position.Y, (int)filledWidth, barHeight), filledColor);
		}
		if (emptyWidth > 0f)
		{
			spriteBatch.Draw(Sprites.Pixel, new Rectangle((int)(position.X + filledWidth), (int)position.Y, (int)emptyWidth, barHeight), emptyColor);
		}
		return new Vector2(width, (float)barHeight);
	}
}
