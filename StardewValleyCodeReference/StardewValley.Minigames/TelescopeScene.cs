using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Minigames;

public class TelescopeScene : IMinigame
{
	public LocalizedContentManager temporaryContent;

	public Texture2D background;

	public Texture2D trees;

	public float yOffset;

	public GameLocation walkSpace;

	public TelescopeScene(NPC Maru)
	{
		this.temporaryContent = Game1.content.CreateTemporary();
		this.background = this.temporaryContent.Load<Texture2D>("LooseSprites\\nightSceneMaru");
		this.trees = this.temporaryContent.Load<Texture2D>("LooseSprites\\nightSceneMaruTrees");
		this.walkSpace = new GameLocation(null, "walkSpace");
		this.walkSpace.map = new Map();
		this.walkSpace.map.AddLayer(new Layer("Back", this.walkSpace.map, new Size(30, 1), new Size(64)));
		Game1.currentLocation = this.walkSpace;
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public bool tick(GameTime time)
	{
		return false;
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public void releaseLeftClick(int x, int y)
	{
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveKeyPress(Keys k)
	{
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		b.Draw(this.background, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - this.background.Bounds.Width / 2 * 4, -(this.background.Bounds.Height * 4) + Game1.graphics.GraphicsDevice.Viewport.Height), this.background.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
		b.Draw(this.trees, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - this.trees.Bounds.Width / 2 * 4, -(this.trees.Bounds.Height * 4) + Game1.graphics.GraphicsDevice.Viewport.Height), this.trees.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		b.End();
	}

	public void changeScreenSize()
	{
	}

	public void unload()
	{
		this.temporaryContent.Unload();
	}

	public void receiveEventPoke(int data)
	{
		throw new NotImplementedException();
	}

	public string minigameId()
	{
		return null;
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool forceQuit()
	{
		return false;
	}
}
