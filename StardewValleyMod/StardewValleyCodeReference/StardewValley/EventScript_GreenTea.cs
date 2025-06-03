using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley;

internal class EventScript_GreenTea : ICustomEventScript
{
	private const int Phase_intro = 0;

	private const int Phase_text1 = 1;

	private const int Phase_text2 = 2;

	private const int Phase_text3 = 3;

	private const int Phase_buddy = 4;

	private const int Phase_end = 5;

	private int width;

	private int height;

	private int topLeftX;

	private int topLeftY;

	private int phaseTimer = 5000;

	private int steamTimer = 100;

	private int cupTimer = 500;

	private int currentPhase;

	private int buddyPhase;

	private int buddyTimer;

	private int textColor;

	private string text;

	private Texture2D tempText;

	private Color bgColor;

	private Color hillColor;

	private Color lightLeafColor;

	private Color darkLeafColor;

	private Vector2 globalCenterPosition;

	private TemporaryAnimatedSprite buddy;

	public EventScript_GreenTea(Vector2 onScreenCenterPosition, Event e)
	{
		this.tempText = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
		this.width = 1920;
		this.height = 1080;
		this.topLeftX = Game1.graphics.GraphicsDevice.Viewport.Width / 2 - this.width / 2;
		this.topLeftY = Game1.graphics.GraphicsDevice.Viewport.Height / 2 - this.height / 2;
		this.bgColor = new Color(20, 104, 82);
		this.hillColor = new Color(55, 68, 53);
		this.lightLeafColor = new Color(11, 56, 39);
		this.darkLeafColor = new Color(5, 3, 4);
		this.globalCenterPosition = onScreenCenterPosition;
		e.aboveMapSprites = new TemporaryAnimatedSpriteList();
		this.addStar(new Vector2(this.topLeftX + 608, this.topLeftY + 228), e);
		this.addStar(new Vector2(this.topLeftX + 644, this.topLeftY + 364), e);
		this.addStar(new Vector2(this.topLeftX + 876, this.topLeftY + 256), e);
		this.addStar(new Vector2(this.topLeftX + 740, this.topLeftY + 452), e);
		this.addStar(new Vector2(this.topLeftX + 1052, this.topLeftY + 472), e);
		this.addStar(new Vector2(this.topLeftX + 1204, this.topLeftY + 252), e);
		this.addStar(new Vector2(this.topLeftX + 1188, this.topLeftY + 400), e);
		this.addStar(new Vector2(this.topLeftX + 736, this.topLeftY + 248), e);
		this.addStar(new Vector2(this.topLeftX + 1120, this.topLeftY + 256), e);
		this.currentPhase = 0;
		this.phaseTimer = 5000;
	}

	private void addStar(Vector2 pos, Event e)
	{
		e.aboveMapSprites.Add(new TemporaryAnimatedSprite
		{
			texture = this.tempText,
			local = true,
			position = pos,
			initialPosition = pos,
			sourceRect = new Rectangle(408, 459, 7, 7),
			scale = 4f,
			sourceRectStartingPos = new Vector2(408f, 459f),
			animationLength = 6,
			totalNumberOfLoops = 99999,
			interval = 150 + Game1.random.Next(-20, 21),
			layerDepth = 1f,
			overrideLocationDestroy = true
		});
	}

	public void draw(SpriteBatch b)
	{
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 208, this.topLeftY + 8, this.width - 416, this.height - 16), Game1.staminaRect.Bounds, this.bgColor, 0f, Vector2.Zero, SpriteEffects.None, 0.05f);
		for (int i = 0; i < 5; i++)
		{
			b.Draw(this.tempText, new Vector2(this.topLeftX + 208 + i * 71 * 4, this.topLeftY + this.height / 2), new Rectangle(386, 472, 71, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		}
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 208, this.topLeftY + this.height / 2 + 60, this.width - 416, this.height / 2 - 76), Game1.staminaRect.Bounds, this.hillColor, 0f, Vector2.Zero, SpriteEffects.None, 0.15f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(276f, 110f) * 4f, new Rectangle(0, 315, 72, 69), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1525f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(196f, 144f) * 4f, new Rectangle(145, 440, 129, 72), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.155f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(200f, 152f) * 4f, new Rectangle(336 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800.0) / 200 * 44, 493, 44, 19), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.156f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(215f, 170f) * 4f, new Rectangle(278, 482, 19, 30), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.159f);
		this.buddy?.draw(b);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 208, this.topLeftY + 8, 296, 1064), Game1.staminaRect.Bounds, this.lightLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.16f);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + this.width - 504, this.topLeftY + 8, 296, 1064), Game1.staminaRect.Bounds, this.lightLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.16f);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 504, this.topLeftY + 900, 936, 180), Game1.staminaRect.Bounds, this.lightLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.165f);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 504, this.topLeftY + 8, 936, 180), Game1.staminaRect.Bounds, this.lightLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.165f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(124f, 213f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(154f, 205f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(200f, 213f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(244f, 209f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(290f, 205f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(325f, 213f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(148f, 27f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(142f, 40f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(148f, 70f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(138f, 102f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(148f, 150f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(135f, 186f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width, this.topLeftY) + new Vector2(-148f, 67f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width, this.topLeftY) + new Vector2(-142f, 80f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width, this.topLeftY) + new Vector2(-148f, 110f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width, this.topLeftY) + new Vector2(-138f, 142f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width, this.topLeftY) + new Vector2(-148f, 190f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width, this.topLeftY) + new Vector2(-135f, 226f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(164f, 62f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(214f, 55f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(240f, 59f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(274f, 55f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(320f, 57f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(365f, 62f) * 4f, new Rectangle(462, 470, 50, 22), this.lightLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 208, this.topLeftY + 8, 140, 1064), Game1.staminaRect.Bounds, this.darkLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.17f);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + this.width - 340, this.topLeftY + 8, 132, 1064), Game1.staminaRect.Bounds, this.darkLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.17f);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 340, this.topLeftY + 1020, 1240, 60), Game1.staminaRect.Bounds, this.darkLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.175f);
		b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX + 340, this.topLeftY + 8, 1240, 60), Game1.staminaRect.Bounds, this.darkLeafColor, 0f, Vector2.Zero, SpriteEffects.None, 0.175f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(94f, 213f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(124f, 213f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(153f, 207f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(200f, 214f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(244f, 209f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(290f, 205f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(325f, 213f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY + 112) + new Vector2(350f, 213f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(148f, 0f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(148f, 27f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(142f, 40f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(148f, 70f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(138f, 102f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(148f, 150f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(135f, 186f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX - 160, this.topLeftY) + new Vector2(148f, 220f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-148f, 57f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-148f, 67f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-142f, 80f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-148f, 110f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-138f, 142f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-148f, 190f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-135f, 226f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX + this.width + 164, this.topLeftY) + new Vector2(-148f, 260f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(124f, 62f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(164f, 62f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(214f, 55f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(240f, 59f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(274f, 54f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(320f, 58f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(365f, 62f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY - 112) + new Vector2(394f, 62f) * 4f, new Rectangle(462, 470, 50, 22), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(111f, 228f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(159f, 214f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(226f, 232f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(294f, 218f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(358f, 221f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(128f, 156f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(108f, 200f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(130f, 78f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(117f, 33f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(184f, 44f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(228f, 42f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(311f, 38f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(123f, 39f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(353f, 101f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(366f, 140f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(352f, 183f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(352f, 50f) * 4f, new Rectangle(79, 354, 41, 27), this.darkLeafColor, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.21f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(121f, 16f) * 4f, new Rectangle(129, 353, 12, 46), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(106f, 93f) * 4f, new Rectangle(129, 353, 12, 46), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(361f, 153f) * 4f, new Rectangle(129, 353, 12, 46), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(341f, 22f) * 4f, new Rectangle(129, 353, 12, 46), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
		b.Draw(this.tempText, new Vector2(this.topLeftX, this.topLeftY) + new Vector2(326f, 0f) * 4f, new Rectangle(129, 353, 12, 46), this.darkLeafColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.22f);
	}

	public void drawAboveAlwaysFront(SpriteBatch b)
	{
		if (this.currentPhase == 5)
		{
			b.Draw(Game1.staminaRect, new Rectangle(this.topLeftX, this.topLeftY, this.width, this.height), Game1.staminaRect.Bounds, this.darkLeafColor * (1f - (float)Math.Min(2000, this.phaseTimer) / 2000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
		}
	}

	public bool update(GameTime time, Event e)
	{
		this.phaseTimer -= time.ElapsedGameTime.Milliseconds;
		this.steamTimer -= time.ElapsedGameTime.Milliseconds;
		this.cupTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.steamTimer <= 0)
		{
			if (e.aboveMapSprites == null)
			{
				e.aboveMapSprites = new TemporaryAnimatedSpriteList();
			}
			int randomX = Game1.random.Next(-48, 64);
			e.aboveMapSprites.Add(new TemporaryAnimatedSprite
			{
				texture = this.tempText,
				local = true,
				position = new Vector2(this.topLeftX + this.width / 2, this.topLeftY + this.height / 2) + new Vector2(-64 + randomX, 64f),
				initialPosition = new Vector2(this.topLeftX + this.width / 2, this.topLeftY + this.height / 2) + new Vector2(-64 + randomX, 64f),
				motion = new Vector2(-0.1f, -1f),
				alphaFade = -0.01f,
				alphaFadeFade = -0.0001f,
				alpha = 0.1f,
				rotationChange = Utility.Lerp(-0.01f, 0.01f, (float)Game1.random.NextDouble()),
				sourceRect = new Rectangle(472, 450, 16, 14),
				scale = 4f,
				sourceRectStartingPos = new Vector2(472f, 450f),
				animationLength = 1,
				totalNumberOfLoops = 1,
				interval = 50000f,
				layerDepth = 1f,
				overrideLocationDestroy = true
			});
			this.steamTimer = 100;
		}
		if (this.phaseTimer <= 0)
		{
			this.currentPhase++;
			this.phaseTimer = 99999;
			switch (this.currentPhase)
			{
			case 1:
				this.text = Game1.content.LoadString("Strings\\Locations:Caroline_Tea_Event1");
				this.textColor = 6;
				break;
			case 2:
				this.text = Game1.content.LoadString("Strings\\Locations:Caroline_Tea_Event2");
				this.textColor = 6;
				break;
			case 3:
				this.text = Game1.content.LoadString("Strings\\Locations:Caroline_Tea_Event3");
				this.textColor = 6;
				break;
			case 4:
				this.buddy = new TemporaryAnimatedSprite
				{
					texture = this.tempText,
					local = true,
					position = new Vector2(this.topLeftX, this.topLeftY) + new Vector2(213f, 170f) * 4f,
					initialPosition = new Vector2(this.topLeftX, this.topLeftY) + new Vector2(219f, 170f) * 4f,
					motion = new Vector2(0f, -9f),
					acceleration = new Vector2(0f, 0.2f),
					sourceRect = new Rectangle(0, 242, 27, 32),
					scale = 4f,
					sourceRectStartingPos = new Vector2(0f, 242f),
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 950000f,
					layerDepth = 0.158f,
					overrideLocationDestroy = true
				};
				this.setBuddyFrame(7);
				Game1.playSound("pullItemFromWater");
				this.buddyPhase = 0;
				break;
			case 5:
				this.phaseTimer = 3000;
				break;
			default:
				this.phaseTimer = 5000;
				break;
			}
		}
		if (this.buddy != null)
		{
			float y = this.buddy.motion.Y;
			this.buddy.update(time);
			if (y <= 0f && this.buddy.motion.Y > 0f)
			{
				this.buddy.layerDepth = 0.161f;
			}
			if (this.buddy.motion.Y > 0f && this.buddy.position.Y >= (float)(this.topLeftY + 608))
			{
				this.buddy.motion.Y = 0f;
				this.buddy.acceleration.Y = 0f;
				this.buddy.position.Y = this.topLeftY + 608;
				this.setBuddyFrame(0);
				Game1.playSound("coin");
				this.buddyPhase = 1;
				this.buddyTimer = 2500;
			}
			if (this.buddyTimer >= 0)
			{
				this.buddyTimer -= time.ElapsedGameTime.Milliseconds;
			}
			switch (this.buddyPhase)
			{
			case 1:
				this.setBuddyFrame(this.buddyTimer % 1000 / 500);
				if (this.buddyTimer <= 0)
				{
					this.buddyPhase = 2;
					this.buddyTimer = 1500;
					this.setBuddyFrame(5);
					Game1.playSound("dwop");
					e.aboveMapSprites.Add(new TemporaryAnimatedSprite
					{
						texture = this.tempText,
						local = true,
						position = this.buddy.position + new Vector2(-7f, -7f) * 4f,
						initialPosition = this.buddy.position + new Vector2(-7f, -7f) * 4f,
						sourceRect = new Rectangle(0, 384, 16, 16),
						scale = 4f,
						sourceRectStartingPos = new Vector2(0f, 384f),
						animationLength = 8,
						totalNumberOfLoops = 4,
						interval = 100f,
						layerDepth = 1f,
						id = 777,
						overrideLocationDestroy = true
					});
				}
				break;
			case 2:
				if (this.buddyTimer <= 0)
				{
					this.setBuddyFrame(6);
					this.buddyPhase = 3;
					Game1.playSound("sipTea");
					this.buddyTimer = 1000;
					e.aboveMapSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.id == 777);
				}
				break;
			case 3:
				if (this.buddyTimer <= 0)
				{
					this.setBuddyFrame(8);
					Game1.playSound("gulp");
					this.buddyPhase = 4;
					this.buddyTimer = 1500;
				}
				break;
			case 4:
				if (this.buddyTimer < 1000)
				{
					this.setBuddyFrame(9);
				}
				if (this.buddyTimer <= 0)
				{
					this.buddyPhase = 5;
					this.buddyTimer = 2400;
					Game1.playSound("dustMeep");
					DelayedAction.playSoundAfterDelay("dustMeep", 400);
					DelayedAction.playSoundAfterDelay("dustMeep", 800);
					DelayedAction.playSoundAfterDelay("dustMeep", 1200);
				}
				break;
			case 5:
				if (this.buddyTimer > 1000)
				{
					this.setBuddyFrame(2 + this.buddyTimer % 400 / 200);
				}
				else
				{
					this.setBuddyFrame(4);
				}
				if (this.buddyTimer <= 0)
				{
					this.buddyTimer = 2000;
					this.buddyPhase = 6;
					for (int i = 0; i < 30; i++)
					{
						Vector2 randomPositionOffset = Utility.getRandomPositionInThisRectangle(new Rectangle(-8, -8, 27, 32), Game1.random) * 4f;
						float xMotion = Utility.Lerp(-2f, 2f, (float)Game1.random.NextDouble());
						e.aboveMapSprites.Add(new TemporaryAnimatedSprite
						{
							texture = this.tempText,
							local = true,
							position = this.buddy.position + randomPositionOffset,
							initialPosition = this.buddy.position + randomPositionOffset,
							motion = new Vector2(xMotion, -0.5f),
							alphaFade = -0.0125f,
							alphaFadeFade = -0.0002f,
							alpha = 0.25f,
							rotationChange = Utility.Lerp(-0.01f, 0.01f, (float)Game1.random.NextDouble()),
							sourceRect = new Rectangle(472, 450, 16, 14),
							scale = 4f,
							sourceRectStartingPos = new Vector2(472f, 450f),
							animationLength = 1,
							totalNumberOfLoops = 1,
							interval = 50000f,
							layerDepth = 1f,
							overrideLocationDestroy = true
						});
					}
					this.buddy = null;
					this.phaseTimer = 1;
					Game1.playSound("fireball");
				}
				break;
			case 6:
				if (this.buddyTimer <= 0)
				{
					this.phaseTimer = 1;
				}
				break;
			}
			Game1.InvalidateOldMouseMovement();
		}
		if (this.text != null)
		{
			e.int_useMeForAnything2 = this.textColor;
			e.float_useMeForAnything += time.ElapsedGameTime.Milliseconds;
			if (e.float_useMeForAnything > 80f)
			{
				if (e.int_useMeForAnything >= this.text.Length)
				{
					if (e.float_useMeForAnything >= 2500f)
					{
						e.int_useMeForAnything = 0;
						e.float_useMeForAnything = 0f;
						e.spriteTextToDraw = "";
						this.text = null;
						this.phaseTimer = 1;
					}
				}
				else
				{
					e.int_useMeForAnything++;
					e.float_useMeForAnything = 0f;
				}
			}
			e.spriteTextToDraw = this.text;
		}
		if (this.currentPhase == 5 && this.phaseTimer <= 20)
		{
			e.aboveMapSprites.Clear();
			return true;
		}
		return false;
	}

	private void setBuddyFrame(int frame)
	{
		if (this.buddy != null)
		{
			this.buddy.sourceRect.X = frame % 5 * 27;
			this.buddy.sourceRect.Y = 242 + frame / 5 * 32;
			this.buddy.sourceRectStartingPos = new Vector2(this.buddy.sourceRect.X, this.buddy.sourceRect.Y);
		}
	}
}
