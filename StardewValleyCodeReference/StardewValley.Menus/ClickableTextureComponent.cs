using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class ClickableTextureComponent : ClickableComponent
{
	public Texture2D texture;

	public Rectangle sourceRect;

	public Rectangle startingSourceRect;

	public float baseScale;

	public string hoverText = "";

	public bool drawLabel = true;

	public bool drawShadow;

	public bool drawLabelWithShadow;

	public ClickableTextureComponent(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false)
		: base(bounds, name, label)
	{
		this.texture = texture;
		if (sourceRect.Equals(Rectangle.Empty) && texture != null)
		{
			this.sourceRect = texture.Bounds;
		}
		else
		{
			this.sourceRect = sourceRect;
		}
		base.scale = scale;
		this.baseScale = scale;
		this.hoverText = hoverText;
		this.drawShadow = drawShadow;
		base.label = label;
		this.startingSourceRect = sourceRect;
	}

	public ClickableTextureComponent(Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false)
		: this("", bounds, "", "", texture, sourceRect, scale, drawShadow)
	{
	}

	public Vector2 getVector2()
	{
		return new Vector2(base.bounds.X, base.bounds.Y);
	}

	public void setPosition(Vector2 position)
	{
		this.setPosition((int)position.X, (int)position.Y);
	}

	public void setPosition(int x, int y)
	{
		base.bounds.X = x;
		base.bounds.Y = y;
	}

	public virtual void tryHover(int x, int y, float maxScaleIncrease = 0.1f)
	{
		if (base.bounds.Contains(x, y))
		{
			base.scale = Math.Min(base.scale + 0.04f, this.baseScale + maxScaleIncrease);
			Game1.SetFreeCursorDrag();
		}
		else
		{
			base.scale = Math.Max(base.scale - 0.04f, this.baseScale);
		}
	}

	public virtual void draw(SpriteBatch b)
	{
		if (base.visible)
		{
			this.draw(b, Color.White, 0.86f + (float)base.bounds.Y / 20000f);
		}
	}

	public virtual void draw(SpriteBatch b, Color c, float layerDepth, int frameOffset = 0, int xOffset = 0, int yOffset = 0)
	{
		if (!base.visible)
		{
			return;
		}
		if (this.texture != null)
		{
			Rectangle r = this.sourceRect;
			if (frameOffset != 0)
			{
				r = new Rectangle(this.sourceRect.X + this.sourceRect.Width * frameOffset, this.sourceRect.Y, this.sourceRect.Width, this.sourceRect.Height);
			}
			if (this.drawShadow)
			{
				Utility.drawWithShadow(b, this.texture, new Vector2((float)(base.bounds.X + xOffset) + (float)(this.sourceRect.Width / 2) * this.baseScale, (float)(base.bounds.Y + yOffset) + (float)(this.sourceRect.Height / 2) * this.baseScale), r, c, 0f, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), base.scale, flipped: false, layerDepth);
			}
			else
			{
				b.Draw(this.texture, new Vector2((float)(base.bounds.X + xOffset) + (float)(this.sourceRect.Width / 2) * this.baseScale, (float)(base.bounds.Y + yOffset) + (float)(this.sourceRect.Height / 2) * this.baseScale), r, c, 0f, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), base.scale, SpriteEffects.None, layerDepth);
			}
		}
		if (this.drawLabel && !string.IsNullOrEmpty(base.label))
		{
			if (this.drawLabelWithShadow)
			{
				Utility.drawTextWithShadow(b, base.label, Game1.smallFont, new Vector2(base.bounds.X + xOffset + base.bounds.Width, (float)(base.bounds.Y + yOffset) + ((float)(base.bounds.Height / 2) - Game1.smallFont.MeasureString(base.label).Y / 2f)), Game1.textColor);
			}
			else
			{
				b.DrawString(Game1.smallFont, base.label, new Vector2(base.bounds.X + xOffset + base.bounds.Width, (float)(base.bounds.Y + yOffset) + ((float)(base.bounds.Height / 2) - Game1.smallFont.MeasureString(base.label).Y / 2f)), Game1.textColor);
			}
		}
	}

	public virtual void drawItem(SpriteBatch b, int xOffset = 0, int yOffset = 0, float alpha = 1f)
	{
		if (base.item != null && base.visible)
		{
			base.item.drawInMenu(b, new Vector2(base.bounds.X + xOffset, base.bounds.Y + yOffset), base.scale / 4f, alpha, 0.9f);
		}
	}
}
