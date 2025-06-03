using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace StardewValley.BellsAndWhistles;

public class MoneyDial
{
	public const int digitHeight = 8;

	public int numDigits;

	public int currentValue;

	public int previousTargetValue;

	public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

	private int speed;

	private int soundTimer;

	private int moneyMadeAccumulator;

	private int moneyShineTimer;

	private bool playSounds = true;

	public Action<int> onPlaySound;

	/// <summary>Whether to shake the money display in <see cref="F:StardewValley.Game1.dayTimeMoneyBox" /> when the money amount changes.</summary>
	public bool ShouldShakeMainMoneyBox = true;

	public MoneyDial(int numDigits, bool playSound = true)
	{
		this.numDigits = numDigits;
		this.playSounds = playSound;
		this.currentValue = 0;
		if (Game1.player != null)
		{
			this.currentValue = Game1.player.Money;
		}
		this.onPlaySound = playDefaultSound;
	}

	public void playDefaultSound(int direction)
	{
		if (direction > 0)
		{
			Game1.playSound("moneyDial");
		}
	}

	public void draw(SpriteBatch b, Vector2 position, int target)
	{
		if (this.previousTargetValue != target)
		{
			this.speed = (target - this.currentValue) / 100;
			this.previousTargetValue = target;
			this.soundTimer = Math.Max(6, 100 / (Math.Abs(this.speed) + 1));
		}
		if (this.moneyShineTimer > 0 && this.currentValue == target)
		{
			this.moneyShineTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
		if (this.moneyMadeAccumulator > 0)
		{
			this.moneyMadeAccumulator -= (Math.Abs(this.speed / 2) + 1) * ((this.animations.Count > 0) ? 1 : 100);
			if (this.moneyMadeAccumulator <= 0)
			{
				this.moneyShineTimer = this.numDigits * 60;
			}
		}
		if (this.ShouldShakeMainMoneyBox && this.moneyMadeAccumulator > 2000)
		{
			Game1.dayTimeMoneyBox.moneyShakeTimer = 100;
		}
		if (this.currentValue != target)
		{
			this.currentValue += this.speed + ((this.currentValue < target) ? 1 : (-1));
			if (this.currentValue < target)
			{
				this.moneyMadeAccumulator += Math.Abs(this.speed);
			}
			this.soundTimer--;
			if (Math.Abs(target - this.currentValue) <= this.speed + 1 || (this.speed != 0 && Math.Sign(target - this.currentValue) != Math.Sign(this.speed)))
			{
				this.currentValue = target;
			}
			if (this.soundTimer <= 0)
			{
				if (this.playSounds)
				{
					this.onPlaySound?.Invoke(Math.Sign(target - this.currentValue));
				}
				this.soundTimer = Math.Max(6, 100 / (Math.Abs(this.speed) + 1));
				if (Game1.random.NextDouble() < 0.4)
				{
					if (target > this.currentValue)
					{
						this.animations.Add(TemporaryAnimatedSprite.GetTemporaryAnimatedSprite(Game1.random.Next(10, 12), position + new Vector2(Game1.random.Next(30, 190), Game1.random.Next(-32, 48)), Color.Gold));
					}
					else if (target < this.currentValue)
					{
						TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(356, 449, 1, 1), 999999f, 1, 44, position + new Vector2(Game1.random.Next(160), Game1.random.Next(-32, 32)), flicker: false, flipped: false, 1f, 0.01f, Color.White, Game1.random.Next(1, 3) * 4, -0.001f, 0f, 0f);
						sprite.motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(-30, -5) / 10f);
						sprite.acceleration = new Vector2(0f, 0.25f);
						this.animations.Add(sprite);
					}
				}
			}
		}
		for (int i = this.animations.Count - 1; i >= 0; i--)
		{
			if (this.animations[i].update(Game1.currentGameTime))
			{
				this.animations.RemoveAt(i);
			}
			else
			{
				this.animations[i].draw(b, localPosition: true);
			}
		}
		int xPosition = 0;
		int digitStrip = (int)Math.Pow(10.0, this.numDigits - 1);
		bool significant = false;
		for (int j = 0; j < this.numDigits; j++)
		{
			int currentDigit = this.currentValue / digitStrip % 10;
			if (currentDigit > 0 || j == this.numDigits - 1)
			{
				significant = true;
			}
			if (significant)
			{
				b.Draw(Game1.mouseCursors, position + new Vector2(xPosition, (Game1.activeClickableMenu is ShippingMenu && this.currentValue >= 1000000) ? ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.53096771240234 + (double)j) * (float)(this.currentValue / 1000000)) : 0f), new Rectangle(286, 502 - currentDigit * 8, 5, 8), Color.Maroon, 0f, Vector2.Zero, 4f + ((this.moneyShineTimer / 60 == this.numDigits - j) ? 0.3f : 0f), SpriteEffects.None, 1f);
			}
			xPosition += 24;
			digitStrip /= 10;
		}
	}
}
