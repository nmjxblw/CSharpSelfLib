using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Minigames;

public class MaruComet : IMinigame
{
	private const int telescopeCircleWidth = 143;

	private const int flybyRepeater = 200;

	private const float flybySpeed = 0.8f;

	private LocalizedContentManager content;

	private Vector2 centerOfScreen;

	private Vector2 cometColorOrigin;

	private Texture2D cometTexture;

	private List<Vector2> flybys = new List<Vector2>();

	private List<Vector2> flybysClose = new List<Vector2>();

	private List<Vector2> flybysFar = new List<Vector2>();

	private string currentString = "";

	private int zoom;

	private int flybyTimer;

	private int totalTimer;

	private int currentStringCharacter;

	private int characterAdvanceTimer;

	private float fade = 1f;

	public MaruComet()
	{
		this.zoom = 4;
		this.content = Game1.content.CreateTemporary();
		this.cometTexture = this.content.Load<Texture2D>("Minigames\\MaruComet");
		this.changeScreenSize();
	}

	public void changeScreenSize()
	{
		float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
		this.centerOfScreen = pixel_zoom_adjustment * new Vector2(Game1.game1.localMultiplayerWindow.Width / 2, Game1.game1.localMultiplayerWindow.Height / 2);
		this.centerOfScreen.X = (int)this.centerOfScreen.X;
		this.centerOfScreen.Y = (int)this.centerOfScreen.Y;
		this.cometColorOrigin = this.centerOfScreen + pixel_zoom_adjustment * new Vector2(-71 * this.zoom, 71 * this.zoom);
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool tick(GameTime time)
	{
		this.flybyTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.fade > 0f)
		{
			this.fade -= (float)time.ElapsedGameTime.Milliseconds * 0.001f;
		}
		if (this.flybyTimer <= 0)
		{
			this.flybyTimer = 200;
			bool bottom = Game1.random.NextBool();
			this.flybys.Add(new Vector2(bottom ? Game1.random.Next(143 * this.zoom) : (-8 * this.zoom), bottom ? (8 * this.zoom) : (-Game1.random.Next(143 * this.zoom))));
			this.flybysClose.Add(new Vector2(bottom ? Game1.random.Next(143 * this.zoom) : (-8 * this.zoom), bottom ? (8 * this.zoom) : (-Game1.random.Next(143 * this.zoom))));
			this.flybysFar.Add(new Vector2(bottom ? Game1.random.Next(143 * this.zoom) : (-8 * this.zoom), bottom ? (8 * this.zoom) : (-Game1.random.Next(143 * this.zoom))));
		}
		for (int i = this.flybys.Count - 1; i >= 0; i--)
		{
			this.flybys[i] = new Vector2(this.flybys[i].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds, this.flybys[i].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds);
			if (this.cometColorOrigin.Y + this.flybys[i].Y < this.centerOfScreen.Y - (float)(143 * this.zoom / 2))
			{
				this.flybys.RemoveAt(i);
			}
		}
		for (int i2 = this.flybysClose.Count - 1; i2 >= 0; i2--)
		{
			this.flybysClose[i2] = new Vector2(this.flybysClose[i2].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds * 1.5f, this.flybysClose[i2].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds * 1.5f);
			if (this.cometColorOrigin.Y + this.flybysClose[i2].Y < this.centerOfScreen.Y - (float)(143 * this.zoom / 2))
			{
				this.flybysClose.RemoveAt(i2);
			}
		}
		for (int i3 = this.flybysFar.Count - 1; i3 >= 0; i3--)
		{
			this.flybysFar[i3] = new Vector2(this.flybysFar[i3].X + 0.8f * (float)time.ElapsedGameTime.Milliseconds * 0.5f, this.flybysFar[i3].Y - 0.8f * (float)time.ElapsedGameTime.Milliseconds * 0.5f);
			if (this.cometColorOrigin.Y + this.flybysFar[i3].Y < this.centerOfScreen.Y - (float)(143 * this.zoom / 2))
			{
				this.flybysFar.RemoveAt(i3);
			}
		}
		this.totalTimer += time.ElapsedGameTime.Milliseconds;
		if (this.totalTimer >= 28000)
		{
			if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet5")))
			{
				this.currentStringCharacter = 0;
				this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet5");
			}
		}
		else if (this.totalTimer >= 25000)
		{
			if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet4")))
			{
				this.currentStringCharacter = 0;
				this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet4");
			}
		}
		else if (this.totalTimer >= 20000)
		{
			if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet3")))
			{
				this.currentStringCharacter = 0;
				this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet3");
			}
		}
		else if (this.totalTimer >= 16000)
		{
			if (!this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet2")))
			{
				this.currentStringCharacter = 0;
				this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet2");
			}
		}
		else if (this.totalTimer >= 10000 && !this.currentString.Equals(Game1.content.LoadString("Strings\\Events:Maru_comet1")))
		{
			this.currentStringCharacter = 0;
			this.currentString = Game1.content.LoadString("Strings\\Events:Maru_comet1");
		}
		this.characterAdvanceTimer += time.ElapsedGameTime.Milliseconds;
		if (this.characterAdvanceTimer > 30)
		{
			this.currentStringCharacter++;
			this.characterAdvanceTimer = 0;
		}
		if (this.totalTimer >= 35000)
		{
			this.fade += (float)time.ElapsedGameTime.Milliseconds * 0.002f;
			if (this.fade >= 1f)
			{
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
				return true;
			}
		}
		return false;
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap);
		b.Draw(this.cometTexture, this.cometColorOrigin + new Vector2((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0), -(int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0)), new Rectangle(247, 0, 265, 240), Color.White, 0f, new Vector2(265f, 0f), this.zoom, SpriteEffects.None, 0.1f);
		b.Draw(this.cometTexture, this.cometColorOrigin + new Vector2((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0) + 808, -(int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2.0 % 808.0) - 808), new Rectangle(247, 0, 265, 240), Color.White, 0f, new Vector2(265f, 0f), this.zoom, SpriteEffects.None, 0.1f);
		b.Draw(this.cometTexture, this.centerOfScreen + new Vector2(-71f, -71f) * this.zoom, new Rectangle((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 300.0 / 100.0) * 143, 240, 143, 143), Color.White, 0f, Vector2.Zero, this.zoom, SpriteEffects.None, 0.2f);
		foreach (Vector2 v in this.flybys)
		{
			b.Draw(this.cometTexture, this.cometColorOrigin + v, new Rectangle(0, 0, 8, 8), Color.White * 0.4f, 0f, Vector2.Zero, this.zoom, SpriteEffects.None, 0.24f);
		}
		foreach (Vector2 v2 in this.flybysClose)
		{
			b.Draw(this.cometTexture, this.cometColorOrigin + v2, new Rectangle(0, 0, 8, 8), Color.White * 0.4f, 0f, Vector2.Zero, this.zoom + 1, SpriteEffects.None, 0.24f);
		}
		foreach (Vector2 v3 in this.flybysFar)
		{
			b.Draw(this.cometTexture, this.cometColorOrigin + v3, new Rectangle(0, 0, 8, 8), Color.White * 0.4f, 0f, Vector2.Zero, this.zoom - 1, SpriteEffects.None, 0.24f);
		}
		b.Draw(this.cometTexture, this.centerOfScreen + new Vector2(-71f, -71f) * this.zoom, new Rectangle(0, 97, 143, 143), Color.White, 0f, Vector2.Zero, this.zoom, SpriteEffects.None, 0.3f);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, (int)this.centerOfScreen.X - 71 * this.zoom, Game1.graphics.GraphicsDevice.Viewport.Height), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, (int)this.centerOfScreen.Y - 71 * this.zoom), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
		b.Draw(Game1.staminaRect, new Rectangle((int)this.centerOfScreen.X + 71 * this.zoom, 0, Game1.graphics.GraphicsDevice.Viewport.Width - ((int)this.centerOfScreen.X + 71 * this.zoom), Game1.graphics.GraphicsDevice.Viewport.Height), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
		b.Draw(Game1.staminaRect, new Rectangle((int)this.centerOfScreen.X - 71 * this.zoom, (int)this.centerOfScreen.Y + 71 * this.zoom, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height - ((int)this.centerOfScreen.Y + 71 * this.zoom)), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
		float height = SpriteText.getHeightOfString(this.currentString, Game1.game1.localMultiplayerWindow.Width);
		float text_draw_y = (int)this.centerOfScreen.Y + 79 * this.zoom;
		if (text_draw_y + height > (float)Game1.viewport.Height)
		{
			text_draw_y += (float)Game1.viewport.Height - (text_draw_y + height);
		}
		SpriteText.drawStringHorizontallyCenteredAt(b, this.currentString, (int)this.centerOfScreen.X, (int)text_draw_y, this.currentStringCharacter, -1, 99999, 1f, 0.99f, junimoText: false, SpriteText.color_Purple, Game1.game1.localMultiplayerWindow.Width);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Game1.staminaRect.Bounds, Color.Black * this.fade, 0f, Vector2.Zero, SpriteEffects.None, 1f);
		b.End();
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public string minigameId()
	{
		return null;
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public void receiveEventPoke(int data)
	{
	}

	public void receiveKeyPress(Keys k)
	{
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
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

	public void unload()
	{
		this.content.Unload();
	}

	public bool forceQuit()
	{
		return false;
	}
}
