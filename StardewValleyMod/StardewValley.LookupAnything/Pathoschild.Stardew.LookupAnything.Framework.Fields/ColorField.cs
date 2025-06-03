using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class ColorField : GenericField
{
	private readonly Color Color;

	private readonly int Strength;

	private readonly bool IsPrismatic;

	public ColorField(string label, Item item)
		: base(label)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (item.Name == "Prismatic Shard")
		{
			this.IsPrismatic = true;
			base.HasValue = true;
		}
		else
		{
			Color? color = TailoringMenu.GetDyeColor(item);
			if (color.HasValue)
			{
				this.Color = color.Value;
				base.HasValue = true;
			}
		}
		if (base.HasValue)
		{
			if (item.HasContextTag("dye_strong"))
			{
				this.Strength = 3;
			}
			else if (item.HasContextTag("dye_medium"))
			{
				this.Strength = 2;
			}
			else
			{
				this.Strength = 1;
			}
		}
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float textHeight = font.MeasureString("ABC").Y;
		Vector2 iconSize = default(Vector2);
		((Vector2)(ref iconSize))._002Ector(textHeight);
		float offset = 0f;
		Color color = (this.IsPrismatic ? Utility.GetPrismaticColor(0, 1f) : this.Color);
		for (int i = 0; i < this.Strength; i++)
		{
			spriteBatch.DrawSpriteWithin(CommonHelper.Pixel, new Rectangle(0, 0, 1, 1), position.X + offset, position.Y, iconSize, color);
			offset += iconSize.X + 2f;
		}
		return new Vector2(wrapWidth, iconSize.Y + 5f);
	}
}
