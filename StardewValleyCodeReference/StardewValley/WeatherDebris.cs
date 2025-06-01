using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley;

[InstanceStatics]
public class WeatherDebris
{
	public const int pinkPetals = 0;

	public const int greenLeaves = 1;

	public const int fallLeaves = 2;

	public const int snow = 3;

	public const int animationInterval = 100;

	public const float gravity = -0.5f;

	public Vector2 position;

	public Rectangle sourceRect;

	public int which;

	public int animationIndex;

	public int animationTimer = 100;

	public int animationDirection = 1;

	public int animationIntervalOffset;

	public float dx;

	public float dy;

	public static float globalWind = -0.25f;

	private bool blowing;

	public WeatherDebris(Vector2 position, int which, float rotationVelocity, float dx, float dy)
	{
		this.position = position;
		this.which = which;
		this.dx = dx;
		this.dy = dy;
		switch (which)
		{
		case 1:
			this.sourceRect = new Rectangle(352, 1200, 16, 16);
			this.animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
			break;
		case 0:
			this.sourceRect = new Rectangle(352, 1184, 16, 16);
			this.animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
			break;
		case 2:
			this.sourceRect = new Rectangle(352, 1216, 16, 16);
			this.animationIntervalOffset = (Game1.random.Next(25) - 12) * 2;
			break;
		case 3:
			this.sourceRect = new Rectangle(391 + 4 * Game1.random.Next(5), 1236, 4, 4);
			break;
		}
	}

	public void update()
	{
		this.update(slow: false);
	}

	public void update(bool slow)
	{
		this.position.X += this.dx + (slow ? 0f : WeatherDebris.globalWind);
		this.position.Y += this.dy - (slow ? 0f : (-0.5f));
		if (this.dy < 0f && !this.blowing)
		{
			this.dy += 0.01f;
		}
		if (!Game1.fadeToBlack && Game1.fadeToBlackAlpha <= 0f)
		{
			if (this.position.X < -80f)
			{
				this.position.X = Game1.viewport.Width;
				this.position.Y = Game1.random.Next(0, Game1.viewport.Height - 64);
			}
			if (this.position.Y > (float)(Game1.viewport.Height + 16))
			{
				this.position.X = Game1.random.Next(0, Game1.viewport.Width);
				this.position.Y = -64f;
				this.dy = (float)Game1.random.Next(-15, 10) / ((!slow) ? 50f : ((Game1.random.NextDouble() < 0.1) ? 5f : 200f));
				this.dx = (float)Game1.random.Next(-10, 0) / (slow ? 200f : 50f);
			}
			else if (this.position.Y < -64f)
			{
				this.position.Y = Game1.viewport.Height;
				this.position.X = Game1.random.Next(0, Game1.viewport.Width);
			}
		}
		if (this.blowing)
		{
			this.dy -= 0.01f;
			if (Game1.random.NextDouble() < 0.006 || this.dy < -2f)
			{
				this.blowing = false;
			}
		}
		else if (!slow && Game1.random.NextDouble() < 0.001 && (Game1.IsSpring || Game1.IsSummer))
		{
			this.blowing = true;
		}
		int num = this.which;
		if ((uint)num > 3u)
		{
			return;
		}
		this.animationTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		if (this.animationTimer > 0)
		{
			return;
		}
		this.animationTimer = 100 + this.animationIntervalOffset;
		this.animationIndex += this.animationDirection;
		if (this.animationDirection == 0)
		{
			if (this.animationIndex >= 9)
			{
				this.animationDirection = -1;
			}
			else
			{
				this.animationDirection = 1;
			}
		}
		if (this.animationIndex > 10)
		{
			if (Game1.random.NextDouble() < 0.82)
			{
				this.animationIndex--;
				this.animationDirection = 0;
				this.dx += 0.1f;
				this.dy -= 0.2f;
			}
			else
			{
				this.animationIndex = 0;
			}
		}
		else if (this.animationIndex == 4 && this.animationDirection == -1)
		{
			this.animationIndex++;
			this.animationDirection = 0;
			this.dx -= 0.1f;
			this.dy -= 0.1f;
		}
		if (this.animationIndex == 7 && this.animationDirection == -1)
		{
			this.dy -= 0.2f;
		}
		if (this.which != 3)
		{
			this.sourceRect.X = 352 + this.animationIndex * 16;
		}
	}

	public void draw(SpriteBatch b)
	{
		b.Draw(Game1.mouseCursors, this.position, this.sourceRect, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1E-06f);
	}
}
