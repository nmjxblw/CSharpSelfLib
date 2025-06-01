using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Util;

public class BoundingBoxGroup
{
	private List<Rectangle> rectangles = new List<Rectangle>();

	public bool Intersects(Rectangle rect)
	{
		foreach (Rectangle rectangle2 in this.rectangles)
		{
			if (rectangle2.Intersects(rect))
			{
				return true;
			}
		}
		return false;
	}

	public bool Contains(int x, int y)
	{
		foreach (Rectangle rectangle2 in this.rectangles)
		{
			if (rectangle2.Contains(x, y))
			{
				return true;
			}
		}
		return false;
	}

	public void Add(Rectangle rect)
	{
		if (!this.rectangles.Contains(rect))
		{
			this.rectangles.Add(rect);
		}
	}

	public void ClearNonIntersecting(Rectangle rect)
	{
		this.rectangles.RemoveAll((Rectangle r) => !r.Intersects(rect));
	}

	public void Clear()
	{
		this.rectangles.Clear();
	}

	public void Draw(SpriteBatch b)
	{
		foreach (Rectangle r in this.rectangles)
		{
			r.Offset(-Game1.viewport.X, -Game1.viewport.Y);
			b.Draw(Game1.fadeToBlackRect, r, Color.Green * 0.5f);
		}
	}

	public bool IsEmpty()
	{
		return this.rectangles.Count == 0;
	}
}
