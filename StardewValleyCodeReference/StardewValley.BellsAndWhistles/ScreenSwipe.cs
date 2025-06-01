using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.BellsAndWhistles;

public class ScreenSwipe
{
	public const int swipe_bundleComplete = 0;

	public const int swipe_raccoon = 1;

	public const int borderPixelWidth = 7;

	private Rectangle bgSource;

	private Rectangle flairSource;

	private Rectangle messageSource;

	private Rectangle movingFlairSource;

	private Rectangle bgDest;

	private int yPosition;

	private int durationAfterSwipe;

	private int originalBGSourceXLimit;

	private List<Vector2> flairPositions = new List<Vector2>();

	private Vector2 messagePosition;

	private Vector2 movingFlairPosition;

	private Vector2 movingFlairMotion;

	private float swipeVelocity;

	private Texture2D texture;

	private int width;

	private int height;

	private int ViewportWidth => Game1.uiViewport.Width;

	private int ViewportHeight => Game1.uiViewport.Height;

	public ScreenSwipe(int which, float swipeVelocity = -1f, int durationAfterSwipe = -1, int w = -1, int h = -1)
	{
		Game1.playSound("throw");
		if (swipeVelocity == -1f)
		{
			swipeVelocity = 5f;
		}
		if (durationAfterSwipe == -1)
		{
			durationAfterSwipe = 2700;
		}
		this.swipeVelocity = swipeVelocity;
		this.durationAfterSwipe = durationAfterSwipe;
		Vector2 screenCenter = new Vector2(this.ViewportWidth / 2, this.ViewportHeight / 2);
		if (which == 0)
		{
			this.messageSource = new Rectangle(128, 1367, 150, 14);
		}
		switch (which)
		{
		case 0:
			this.texture = Game1.mouseCursors;
			this.bgSource = new Rectangle(128, 1296, 1, 71);
			this.flairSource = new Rectangle(144, 1303, 144, 58);
			this.movingFlairSource = new Rectangle(643, 768, 8, 13);
			this.originalBGSourceXLimit = this.bgSource.X + this.bgSource.Width;
			this.yPosition = (int)screenCenter.Y - this.bgSource.Height * 4 / 2;
			this.messagePosition = new Vector2(screenCenter.X - (float)(this.messageSource.Width * 4 / 2), screenCenter.Y - (float)(this.messageSource.Height * 4 / 2));
			this.flairPositions.Add(new Vector2(this.messagePosition.X - (float)(this.flairSource.Width * 4) - 64f, this.yPosition + 28));
			this.flairPositions.Add(new Vector2(this.messagePosition.X + (float)(this.messageSource.Width * 4) + 64f, this.yPosition + 28));
			this.movingFlairPosition = new Vector2(this.messagePosition.X + (float)(this.messageSource.Width * 4) + 192f, screenCenter.Y + 32f);
			this.movingFlairMotion = new Vector2(0f, -0.5f);
			break;
		case 1:
			this.texture = Game1.mouseCursors_1_6;
			this.bgSource = new Rectangle(0, 361, 1, 71);
			this.flairSource = new Rectangle(1, 361, 159, 71);
			this.movingFlairSource = new Rectangle(161, 412, 17, 16);
			this.originalBGSourceXLimit = this.bgSource.X + this.bgSource.Width;
			this.yPosition = (int)screenCenter.Y - this.bgSource.Height * 4 / 2;
			this.messagePosition = new Vector2(screenCenter.X - (float)(this.messageSource.Width * 4 / 2), screenCenter.Y - (float)(this.messageSource.Height * 4 / 2));
			this.flairPositions.Add(new Vector2(this.messagePosition.X - (float)(this.flairSource.Width * 4 / 2), this.yPosition));
			this.movingFlairPosition = new Vector2(this.messagePosition.X + (float)(this.messageSource.Width * 4) + 192f, screenCenter.Y + 32f);
			this.movingFlairMotion = new Vector2(0f, -0.5f);
			break;
		}
		this.bgDest = new Rectangle(0, this.yPosition, this.bgSource.Width * 4, this.bgSource.Height * 4);
	}

	public bool update(GameTime time)
	{
		if (this.durationAfterSwipe > 0 && this.bgDest.Width <= this.ViewportWidth)
		{
			this.bgDest.Width += (int)((double)this.swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
			if (this.bgDest.Width > this.ViewportWidth)
			{
				Game1.playSound("newRecord");
			}
		}
		else if (this.durationAfterSwipe <= 0)
		{
			this.bgDest.X += (int)((double)this.swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
			for (int i = 0; i < this.flairPositions.Count; i++)
			{
				if ((float)this.bgDest.X > this.flairPositions[i].X)
				{
					this.flairPositions[i] = new Vector2(this.bgDest.X, this.flairPositions[i].Y);
				}
			}
			if ((float)this.bgDest.X > this.messagePosition.X)
			{
				this.messagePosition = new Vector2(this.bgDest.X, this.messagePosition.Y);
			}
			if ((float)this.bgDest.X > this.movingFlairPosition.X)
			{
				this.movingFlairPosition = new Vector2(this.bgDest.X, this.movingFlairPosition.Y);
			}
		}
		if (this.bgDest.Width > this.ViewportWidth && this.durationAfterSwipe > 0)
		{
			if (Game1.oldMouseState.LeftButton == ButtonState.Pressed)
			{
				this.durationAfterSwipe = 0;
			}
			this.durationAfterSwipe -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this.durationAfterSwipe <= 0)
			{
				Game1.playSound("tinyWhip");
			}
		}
		this.movingFlairPosition += this.movingFlairMotion;
		return this.bgDest.X > this.ViewportWidth;
	}

	public Rectangle getAdjustedSourceRect(Rectangle sourceRect, float xStartPosition)
	{
		if (xStartPosition > (float)this.bgDest.Width || xStartPosition + (float)(sourceRect.Width * 4) < (float)this.bgDest.X)
		{
			return Rectangle.Empty;
		}
		Math.Min(sourceRect.X + sourceRect.Width, Math.Max(sourceRect.X, (float)sourceRect.X + ((float)this.bgDest.Width - xStartPosition) / 4f));
		return new Rectangle(sourceRect.X, sourceRect.Y, (int)Math.Min(sourceRect.Width, ((float)this.bgDest.Width - xStartPosition) / 4f), sourceRect.Height);
	}

	public void draw(SpriteBatch b)
	{
		b.Draw(this.texture, this.bgDest, this.bgSource, Color.White);
		foreach (Vector2 v in this.flairPositions)
		{
			Rectangle r = this.getAdjustedSourceRect(this.flairSource, v.X);
			_ = r.Right;
			_ = this.originalBGSourceXLimit;
			b.Draw(this.texture, v, r, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
		b.Draw(this.texture, this.movingFlairPosition, this.getAdjustedSourceRect(this.movingFlairSource, this.movingFlairPosition.X), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		b.Draw(this.texture, this.messagePosition, this.getAdjustedSourceRect(this.messageSource, this.messagePosition.X), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
	}
}
