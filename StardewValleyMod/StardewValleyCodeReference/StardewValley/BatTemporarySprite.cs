using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley;

public class BatTemporarySprite : TemporaryAnimatedSprite
{
	public new Texture2D texture;

	private bool moveLeft;

	private int horizontalSpeed;

	private float verticalSpeed;

	public BatTemporarySprite(Vector2 position)
		: base(-666, 100f, 4, 99999, position, flicker: false, flipped: false)
	{
		this.texture = Game1.content.Load<Texture2D>("LooseSprites\\Bat");
		base.currentParentTileIndex = 0;
		if (position.X > (float)(Game1.currentLocation.Map.DisplayWidth / 2))
		{
			this.moveLeft = true;
		}
		this.horizontalSpeed = Game1.random.Next(1, 8);
		this.verticalSpeed = Game1.random.Next(3, 7);
		base.interval = 160f - ((float)this.horizontalSpeed + this.verticalSpeed) * 10f;
	}

	public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
	{
		spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, base.Position), new Rectangle(base.currentParentTileIndex * 64, 0, 64, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (base.Position.Y + 32f) / 10000f);
	}

	public override bool update(GameTime time)
	{
		base.timer += time.ElapsedGameTime.Milliseconds;
		if (base.timer > base.interval)
		{
			base.currentParentTileIndex++;
			base.timer = 0f;
			if (base.currentParentTileIndex >= base.animationLength)
			{
				base.currentNumberOfLoops++;
				base.currentParentTileIndex = 0;
			}
		}
		if (this.moveLeft)
		{
			base.position.X -= this.horizontalSpeed;
		}
		else
		{
			base.position.X += this.horizontalSpeed;
		}
		base.position.Y += this.verticalSpeed;
		this.verticalSpeed -= 0.1f;
		if (base.position.Y >= (float)Game1.currentLocation.Map.DisplayHeight || base.position.Y < 0f || base.position.X < 0f || base.position.X >= (float)Game1.currentLocation.Map.DisplayWidth)
		{
			return true;
		}
		return false;
	}
}
