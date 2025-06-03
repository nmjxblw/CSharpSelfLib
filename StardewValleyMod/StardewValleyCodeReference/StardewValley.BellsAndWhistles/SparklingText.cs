using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class SparklingText
{
	public static int maxDistanceForSparkle = 32;

	private SpriteFont font;

	private Color color;

	private Color sparkleColor;

	private bool rainbow;

	private int millisecondsDuration;

	private int amplitude;

	private int period;

	private int colorCycle;

	public string text;

	private float[] individualCharacterOffsets;

	public float offsetDecay = 1f;

	public float alpha = 1f;

	public float textWidth;

	public float drawnTextWidth;

	public float layerDepth = 1f;

	private double sparkleFrequency;

	private TemporaryAnimatedSpriteList sparkles;

	private Rectangle boundingBox;

	public SparklingText(SpriteFont font, string text, Color color, Color sparkleColor, bool rainbow = false, double sparkleFrequency = 0.1, int millisecondsDuration = 2500, int amplitude = -1, int speed = 500, float depth = 1f)
	{
		if (amplitude == -1)
		{
			amplitude = 64;
		}
		SparklingText.maxDistanceForSparkle = 32;
		this.font = font;
		this.color = color;
		this.sparkleColor = sparkleColor;
		this.text = text;
		this.rainbow = rainbow;
		if (rainbow)
		{
			color = Color.Yellow;
		}
		this.sparkleFrequency = sparkleFrequency;
		this.millisecondsDuration = millisecondsDuration;
		this.individualCharacterOffsets = new float[text.Length];
		this.amplitude = amplitude;
		this.period = speed;
		this.sparkles = new TemporaryAnimatedSpriteList();
		this.boundingBox = new Rectangle(-SparklingText.maxDistanceForSparkle, -SparklingText.maxDistanceForSparkle, (int)font.MeasureString(text).X + SparklingText.maxDistanceForSparkle * 2, (int)font.MeasureString(text).Y + SparklingText.maxDistanceForSparkle * 2);
		this.textWidth = font.MeasureString(text).X;
		this.layerDepth = depth;
		int xOffset = 0;
		for (int i = 0; i < text.Length; i++)
		{
			xOffset += (int)font.MeasureString(text[i].ToString() ?? "").X;
		}
		this.drawnTextWidth = xOffset;
	}

	public bool update(GameTime time)
	{
		this.millisecondsDuration -= time.ElapsedGameTime.Milliseconds;
		this.offsetDecay -= 0.001f;
		this.amplitude = (int)((float)this.amplitude * this.offsetDecay);
		if (this.millisecondsDuration <= 500)
		{
			this.alpha = (float)this.millisecondsDuration / 500f;
		}
		for (int i = 0; i < this.individualCharacterOffsets.Length; i++)
		{
			this.individualCharacterOffsets[i] = (float)((double)(this.amplitude / 2) * Math.Sin(Math.PI * 2.0 / (double)this.period * (double)(this.millisecondsDuration - i * 100)));
		}
		if (this.millisecondsDuration > 500 && Game1.random.NextDouble() < this.sparkleFrequency)
		{
			int speed = Game1.random.Next(100, 600);
			this.sparkles.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 704, 64, 64), speed / 6, 6, 0, new Vector2(Game1.random.Next(this.boundingBox.X, this.boundingBox.Right), Game1.random.Next(this.boundingBox.Y, this.boundingBox.Bottom)), flicker: false, flipped: false, this.layerDepth, 0f, this.rainbow ? this.color : this.sparkleColor, 1f, 0f, 0f, 0f));
		}
		this.sparkles.RemoveWhere((TemporaryAnimatedSprite sparkle) => sparkle.update(time));
		if (this.rainbow)
		{
			this.incrementRainbowColors();
		}
		return this.millisecondsDuration <= 0;
	}

	private void incrementRainbowColors()
	{
		if (this.colorCycle != 0)
		{
			return;
		}
		if ((this.color.G += 4) >= byte.MaxValue)
		{
			this.colorCycle = 1;
		}
		else
		{
			if (this.colorCycle != 1)
			{
				return;
			}
			if ((this.color.R -= 4) <= 0)
			{
				this.colorCycle = 2;
			}
			else
			{
				if (this.colorCycle != 2)
				{
					return;
				}
				if ((this.color.B += 4) >= byte.MaxValue)
				{
					this.colorCycle = 3;
				}
				else
				{
					if (this.colorCycle != 3)
					{
						return;
					}
					if ((this.color.G -= 4) <= 0)
					{
						this.colorCycle = 4;
					}
					else if (this.colorCycle == 4)
					{
						if (++this.color.R >= byte.MaxValue)
						{
							this.colorCycle = 5;
						}
						else if (this.colorCycle == 5 && (this.color.B -= 4) <= 0)
						{
							this.colorCycle = 0;
						}
					}
				}
			}
		}
	}

	private static Color getRainbowColorFromIndex(int index)
	{
		return (index % 8) switch
		{
			0 => Color.Red, 
			1 => Color.Orange, 
			2 => Color.Yellow, 
			3 => Color.Chartreuse, 
			4 => Color.Green, 
			5 => Color.Cyan, 
			6 => Color.Blue, 
			7 => Color.Violet, 
			_ => Color.White, 
		};
	}

	public void draw(SpriteBatch b, Vector2 onScreenPosition)
	{
		int xOffset = 0;
		for (int i = 0; i < this.text.Length; i++)
		{
			b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset - 2, this.individualCharacterOffsets[i]), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
			b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset + 2, this.individualCharacterOffsets[i]), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.991f);
			b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset, this.individualCharacterOffsets[i] - 2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.992f);
			b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset, this.individualCharacterOffsets[i] + 2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.993f);
			b.DrawString(this.font, this.text[i].ToString() ?? "", onScreenPosition + new Vector2(xOffset, this.individualCharacterOffsets[i]), this.rainbow ? SparklingText.getRainbowColorFromIndex(i) : (this.color * this.alpha), 0f, Vector2.Zero, 1f, SpriteEffects.None, this.layerDepth);
			xOffset += (int)this.font.MeasureString(this.text[i].ToString() ?? "").X;
		}
		foreach (TemporaryAnimatedSprite sparkle in this.sparkles)
		{
			sparkle.Position += onScreenPosition;
			sparkle.draw(b, localPosition: true);
			sparkle.Position -= onScreenPosition;
		}
	}
}
