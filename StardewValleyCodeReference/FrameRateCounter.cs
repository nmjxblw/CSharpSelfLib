using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

public class FrameRateCounter : DrawableGameComponent
{
	private LocalizedContentManager content;

	private SpriteBatch spriteBatch;

	private int frameRate;

	private int frameCounter;

	private TimeSpan elapsedTime = TimeSpan.Zero;

	public FrameRateCounter(Game game)
		: base(game)
	{
		this.content = new LocalizedContentManager(game.Services, base.Game.Content.RootDirectory);
	}

	protected override void LoadContent()
	{
		this.spriteBatch = new SpriteBatch(base.GraphicsDevice);
	}

	protected override void UnloadContent()
	{
		this.content.Unload();
	}

	public override void Update(GameTime gameTime)
	{
		this.elapsedTime += gameTime.ElapsedGameTime;
		if (this.elapsedTime > TimeSpan.FromSeconds(1.0))
		{
			this.elapsedTime -= TimeSpan.FromSeconds(1.0);
			this.frameRate = this.frameCounter;
			this.frameCounter = 0;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		this.frameCounter++;
		string fps = $"fps: {this.frameRate}";
		this.spriteBatch.Begin();
		this.spriteBatch.DrawString(Game1.dialogueFont, fps, new Vector2(33f, 33f), Color.Black);
		this.spriteBatch.DrawString(Game1.dialogueFont, fps, new Vector2(32f, 32f), Color.White);
		this.spriteBatch.End();
	}
}
