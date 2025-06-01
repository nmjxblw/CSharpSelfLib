using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles;

public class SebsFrogs : TemporaryAnimatedSprite
{
	private float yOriginal;

	private bool flipJump;

	public override bool update(GameTime time)
	{
		base.update(time);
		if (!base.pingPong && base.motion.Equals(Vector2.Zero) && Game1.random.NextDouble() < 0.003)
		{
			if (Game1.random.NextDouble() < 0.4)
			{
				base.animationLength = 3;
				base.pingPong = true;
			}
			else
			{
				this.flipJump = !this.flipJump;
				this.yOriginal = base.position.Y;
				base.motion = new Vector2((!this.flipJump) ? 1 : (-1), -3f);
				base.acceleration = new Vector2(0f, 0.2f);
				base.sourceRect.X = 0;
				base.interval = Game1.random.Next(110, 150);
				base.animationLength = 5;
				base.flipped = this.flipJump;
				if (base.Parent != null && base.Parent == Game1.currentLocation && Game1.random.NextDouble() < 0.03)
				{
					Game1.playSound("croak");
				}
			}
		}
		else if (base.pingPong && Game1.random.NextDouble() < 0.02 && base.sourceRect.X == 64)
		{
			base.animationLength = 1;
			base.pingPong = false;
			base.sourceRect.X = (int)base.sourceRectStartingPos.X;
		}
		if (!base.motion.Equals(Vector2.Zero) && base.position.Y > this.yOriginal)
		{
			base.motion = Vector2.Zero;
			base.acceleration = Vector2.Zero;
			base.sourceRect.X = 64;
			base.animationLength = 1;
			base.position.Y = this.yOriginal;
		}
		return false;
	}
}
