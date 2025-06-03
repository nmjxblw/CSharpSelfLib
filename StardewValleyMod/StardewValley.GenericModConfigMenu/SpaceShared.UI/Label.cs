using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace SpaceShared.UI;

internal class Label : Element
{
	public bool Bold { get; set; }

	public float NonBoldScale { get; set; } = 1f;

	public bool NonBoldShadow { get; set; } = true;

	public Color IdleTextColor { get; set; } = Game1.textColor;

	public Color HoverTextColor { get; set; } = Game1.unselectedOptionColor;

	public SpriteFont Font { get; set; } = Game1.dialogueFont;

	public float Scale
	{
		get
		{
			if (!this.Bold)
			{
				return this.NonBoldScale;
			}
			return 1f;
		}
	}

	public string String { get; set; }

	public Action<Element> Callback { get; set; }

	public override int Width => (int)this.Measure().X;

	public override int Height => (int)this.Measure().Y;

	public override string HoveredSound
	{
		get
		{
			if (this.Callback == null)
			{
				return null;
			}
			return "shiny4";
		}
	}

	public override void Update(bool isOffScreen = false)
	{
		base.Update(isOffScreen);
		if (base.Clicked)
		{
			this.Callback?.Invoke(this);
		}
	}

	public Vector2 Measure()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return Label.MeasureString(this.String, this.Bold, this.Bold ? 1f : this.NonBoldScale, this.Font);
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsHidden())
		{
			return;
		}
		bool altColor = base.Hover && this.Callback != null;
		if (this.Bold)
		{
			SpriteText.drawString(b, this.String, (int)base.Position.X, (int)base.Position.Y, 999999, -1, 999999, 1f, 1f, false, -1, "", altColor ? new Color?(SpriteText.color_Gray) : ((Color?)null), (ScrollTextAlignment)0);
			return;
		}
		Color col = (altColor ? this.HoverTextColor : this.IdleTextColor);
		if (((Color)(ref col)).A > 0)
		{
			if (this.NonBoldShadow)
			{
				Utility.drawTextWithShadow(b, this.String, this.Font, base.Position, col, this.NonBoldScale, -1f, -1, -1, 1f, 3);
			}
			else
			{
				b.DrawString(this.Font, this.String, base.Position, col, 0f, Vector2.Zero, this.NonBoldScale, (SpriteEffects)0, 1f);
			}
		}
	}

	public static Vector2 MeasureString(string text, bool bold = false, float scale = 1f, SpriteFont font = null)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (bold)
		{
			return new Vector2((float)SpriteText.getWidthOfString(text, 999999) * scale, (float)SpriteText.getHeightOfString(text, 999999) * scale);
		}
		return (font ?? Game1.dialogueFont).MeasureString(text) * scale;
	}
}
