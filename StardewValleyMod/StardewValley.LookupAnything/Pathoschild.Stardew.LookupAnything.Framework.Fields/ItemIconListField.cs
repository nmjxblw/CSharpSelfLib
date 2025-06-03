using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class ItemIconListField : GenericField
{
	private readonly Tuple<Item, SpriteInfo?>[] Items;

	private readonly Func<Item, string?>? FormatItemName;

	private readonly bool ShowStackSize;

	public ItemIconListField(GameHelper gameHelper, string label, IEnumerable<Item?>? items, bool showStackSize, Func<Item, string?>? formatItemName = null)
		: base(label, items != null)
	{
		this.Items = (from item in items?.WhereNotNull()
			select Tuple.Create<Item, SpriteInfo>(item, gameHelper.GetSprite(item))).ToArray() ?? Array.Empty<Tuple<Item, SpriteInfo>>();
		base.HasValue = this.Items.Any();
		this.ShowStackSize = showStackSize;
		this.FormatItemName = formatItemName;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		float textHeight = font.MeasureString("ABC").Y;
		Vector2 iconSize = default(Vector2);
		((Vector2)(ref iconSize))._002Ector(textHeight);
		int topOffset = 0;
		Tuple<Item, SpriteInfo>[] items = this.Items;
		for (int i = 0; i < items.Length; i++)
		{
			var (item, sprite) = items[i];
			spriteBatch.DrawSpriteWithin(sprite, position.X, position.Y + (float)topOffset, iconSize);
			if (this.ShowStackSize && item.Stack > 1)
			{
				float scale = 2f;
				Vector2 sizePos = position + new Vector2(iconSize.X - (float)Utility.getWidthOfTinyDigitString(item.Stack, scale), iconSize.Y + (float)topOffset - 6f * scale);
				Utility.drawTinyDigits(item.Stack, spriteBatch, sizePos, scale, 1f, Color.White);
			}
			string displayText = this.FormatItemName?.Invoke(item) ?? item.DisplayName;
			Vector2 textSize = spriteBatch.DrawTextBlock(font, displayText, position + new Vector2(iconSize.X + 5f, (float)topOffset), wrapWidth);
			topOffset += (int)Math.Max(iconSize.Y, textSize.Y) + 5;
		}
		return new Vector2(wrapWidth, (float)(topOffset + 5));
	}
}
