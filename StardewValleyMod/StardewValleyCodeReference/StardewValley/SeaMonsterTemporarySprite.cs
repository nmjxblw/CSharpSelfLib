using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley;

public class SeaMonsterTemporarySprite : TemporaryAnimatedSprite
{
	public new Texture2D texture;

	public SeaMonsterTemporarySprite(float animationInterval, int animationLength, int numberOfLoops, Vector2 position)
		: base(-666, animationInterval, animationLength, numberOfLoops, position, flicker: false, flipped: false)
	{
		this.texture = Game1.content.Load<Texture2D>("LooseSprites\\SeaMonster");
		Game1.playSound("pullItemFromWater");
		base.currentParentTileIndex = 0;
	}

	public override void draw(SpriteBatch spriteBatch, bool localPosition = false, int xOffset = 0, int yOffset = 0, float extraAlpha = 1f)
	{
		spriteBatch.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, base.Position), new Rectangle(base.currentParentTileIndex * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 32f) / 10000f);
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
				base.currentParentTileIndex = 2;
			}
		}
		if (base.currentNumberOfLoops >= base.totalNumberOfLoops)
		{
			base.position.Y += 2f;
			if (base.position.Y >= (float)Game1.currentLocation.Map.DisplayHeight)
			{
				return true;
			}
		}
		return false;
	}
}
