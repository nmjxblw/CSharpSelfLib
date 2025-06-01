using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

internal class ImageCreditsBlock : ICreditsBlock
{
	private ClickableTextureComponent clickableComponent;

	private int animationFrames;

	public ImageCreditsBlock(Texture2D texture, Rectangle sourceRect, int pixelZoom, int animationFrames)
	{
		this.animationFrames = animationFrames;
		this.clickableComponent = new ClickableTextureComponent(new Rectangle(0, 0, sourceRect.Width * pixelZoom, sourceRect.Height * pixelZoom), texture, sourceRect, pixelZoom);
	}

	public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
	{
		b.Draw(this.clickableComponent.texture, new Rectangle(topLeftX + widthToOccupy / 2 - this.clickableComponent.bounds.Width / 2, topLeftY, this.clickableComponent.bounds.Width, this.clickableComponent.bounds.Height), new Rectangle(this.clickableComponent.sourceRect.X + this.clickableComponent.sourceRect.Width * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / (double)(600 / this.animationFrames)), this.clickableComponent.sourceRect.Y, this.clickableComponent.sourceRect.Width, this.clickableComponent.sourceRect.Height), Color.White);
	}

	public override int getHeight(int maxWidth)
	{
		return this.clickableComponent.bounds.Height;
	}
}
