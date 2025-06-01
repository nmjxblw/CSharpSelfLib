using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Events;

public class QiPlaneEvent : BaseFarmEvent
{
	private Vector2 qiPlanePos;

	private List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

	private float boxDropTimer;

	private float textTimer;

	private float finalFadeTimer;

	private string str;

	public QiPlaneEvent()
	{
		this.qiPlanePos = new Vector2(-400f, Game1.graphics.GraphicsDevice.Viewport.Height / 4);
		this.boxDropTimer = 2000f;
		this.str = Game1.content.LoadString("Strings\\1_6_Strings:MysteryBoxAnnounce");
		Game1.changeMusicTrack("nightTime");
		DelayedAction.playSoundAfterDelay("planeflyby", 1000);
		Game1.player.mailReceived.Add("sawQiPlane");
	}

	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), new Rectangle(0, 0, 1, 1), new Color(24, 34, 84));
		b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, (int)((float)Game1.graphics.GraphicsDevice.Viewport.Height * 0.7f)), new Rectangle(639, 858, 1, 184), Color.LightBlue);
		b.Draw(Game1.mouseCursors, new Vector2(1f, 1f), new Rectangle(0, 1453, 639, 191), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
		b.Draw(Game1.mouseCursors, new Vector2(2564f, 1f), new Rectangle(0, 1453, 639, 191), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
		b.Draw(Game1.mouseCursors, new Vector2(-50f, -10f) * 4f + new Vector2(0f, Game1.graphics.GraphicsDevice.Viewport.Height - 596), new Rectangle(0, 885, 639, 149), Color.DarkCyan, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		b.Draw(Game1.mouseCursors, new Vector2(-50f, -10f) * 4f + new Vector2(2556f, Game1.graphics.GraphicsDevice.Viewport.Height - 596), new Rectangle(0, 885, 639, 149), Color.DarkCyan, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.graphics.GraphicsDevice.Viewport.Height - 596), new Rectangle(0, 885, 639, 149), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
		b.Draw(Game1.mouseCursors, new Vector2(2556f, Game1.graphics.GraphicsDevice.Viewport.Height - 596), new Rectangle(0, 885, 639, 149), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
		foreach (TemporaryAnimatedSprite tempSprite in this.tempSprites)
		{
			tempSprite.draw(b, localPosition: true);
		}
		b.Draw(Game1.mouseCursors_1_6, this.qiPlanePos, new Rectangle(113, 204, 79, 43), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.82f);
		b.Draw(Game1.mouseCursors_1_6, this.qiPlanePos + new Vector2(79f, 0f) * 4f, new Rectangle(192 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 90.0 / 30.0) * 4, 204, 4, 44), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.82f);
		if (this.qiPlanePos.X > (float)(Game1.graphics.GraphicsDevice.Viewport.Width - 480))
		{
			float oldTime = this.textTimer;
			this.textTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			if (this.textTimer % 100f < oldTime % 100f && (int)(this.textTimer / 100f) < this.str.Length)
			{
				Game1.playSound("dialogueCharacter");
			}
			if ((int)(this.textTimer / 100f) > this.str.Length + 27)
			{
				this.finalFadeTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			}
			b.Draw(Game1.staminaRect, new Rectangle(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - SpriteText.getWidthOfString(this.str) / 2 - 18, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 56, SpriteText.getWidthOfString(this.str) + 20, 60), Color.Black * 0.4f);
			SpriteText.drawStringHorizontallyCenteredAt(b, this.str, Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 50, (int)(this.textTimer / 100f), -1, 999999, 1f, 0.9f, junimoText: false, Color.White);
		}
		b.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * (this.finalFadeTimer / 3000f));
		base.draw(b);
	}

	public override void drawAboveEverything(SpriteBatch b)
	{
		base.drawAboveEverything(b);
	}

	public override bool setUp()
	{
		return base.setUp();
	}

	public override bool tickUpdate(GameTime time)
	{
		if (Game1.GetKeyboardState().IsKeyDown(Keys.Escape))
		{
			this.qiPlanePos.X = Game1.graphics.GraphicsDevice.Viewport.Width + 1000;
			this.textTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds * 2f;
			if ((int)(this.textTimer / 100f) > this.str.Length + 27)
			{
				this.finalFadeTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds * 2f;
			}
		}
		this.boxDropTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		if (this.boxDropTimer <= 0f && this.qiPlanePos.X < (float)Game1.graphics.GraphicsDevice.Viewport.Width)
		{
			this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(112, 166, 14, 35), 50f, 10, 1, this.qiPlanePos + new Vector2(52f, -4f) * 4f, flicker: false, flipped: false)
			{
				holdLastFrame = true,
				motion = new Vector2(-1f, Game1.random.Next(3, 5)),
				accelerationChange = new Vector2(0f, -0.001f + (float)(Game1.random.NextDouble() - 0.5) / 1000f),
				acceleration = new Vector2(0f, 0.05f),
				scale = 4f
			});
			this.boxDropTimer = Game1.random.Next(150, 500);
			DelayedAction.playSoundAfterDelay("parachute", 300);
		}
		for (int i = this.tempSprites.Count - 1; i >= 0; i--)
		{
			this.tempSprites[i].update(time);
			if (this.tempSprites[i].motion.Y < 1f)
			{
				this.tempSprites[i].motion.Y = 1f;
			}
			if (this.tempSprites[i].position.Y > (float)(Game1.graphics.GraphicsDevice.Viewport.Height + 500))
			{
				this.tempSprites[i].alphaFade = 0.01f;
			}
			if (this.tempSprites[i].alpha <= 0f)
			{
				this.tempSprites.RemoveAt(i);
			}
		}
		this.qiPlanePos.X += (float)(time.ElapsedGameTime.TotalMilliseconds * 0.25);
		return this.finalFadeTimer > 4000f;
	}
}
