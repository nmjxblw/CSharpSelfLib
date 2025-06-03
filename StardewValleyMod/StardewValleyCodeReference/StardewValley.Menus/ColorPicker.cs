using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class ColorPicker
{
	public const int sliderChunks = 24;

	private Rectangle bounds;

	public SliderBar hueBar;

	public SliderBar valueBar;

	public SliderBar saturationBar;

	public SliderBar recentSliderBar;

	public string Name;

	public Color LastColor;

	public bool Dirty;

	public ColorPicker(string name, int x, int y)
	{
		this.Name = name;
		this.hueBar = new SliderBar(0, 0, 50);
		this.saturationBar = new SliderBar(0, 20, 50);
		this.valueBar = new SliderBar(0, 40, 50);
		this.bounds = new Rectangle(x, y, SliderBar.defaultWidth, 60);
	}

	public Color getSelectedColor()
	{
		return ColorPicker.HsvToRgb((double)this.hueBar.value / 100.0 * 360.0, (double)this.saturationBar.value / 100.0, (double)this.valueBar.value / 100.0);
	}

	public Color click(int x, int y)
	{
		if (this.bounds.Contains(x, y))
		{
			x -= this.bounds.X;
			y -= this.bounds.Y;
			if (this.hueBar.bounds.Contains(x, y))
			{
				this.hueBar.click(x, y);
				this.recentSliderBar = this.hueBar;
			}
			if (this.saturationBar.bounds.Contains(x, y))
			{
				this.recentSliderBar = this.saturationBar;
				this.saturationBar.click(x, y);
			}
			if (this.valueBar.bounds.Contains(x, y))
			{
				this.recentSliderBar = this.valueBar;
				this.valueBar.click(x, y);
			}
		}
		return this.getSelectedColor();
	}

	public void changeHue(int amount)
	{
		this.hueBar.changeValueBy(amount);
		this.recentSliderBar = this.hueBar;
	}

	public void changeSaturation(int amount)
	{
		this.saturationBar.changeValueBy(amount);
		this.recentSliderBar = this.saturationBar;
	}

	public void changeValue(int amount)
	{
		this.valueBar.changeValueBy(amount);
		this.recentSliderBar = this.valueBar;
	}

	public Color clickHeld(int x, int y)
	{
		if (this.recentSliderBar != null)
		{
			x = Math.Max(x, this.bounds.X);
			x = Math.Min(x, this.bounds.Right - 1);
			y = this.recentSliderBar.bounds.Center.Y;
			x -= this.bounds.X;
			if (this.recentSliderBar.Equals(this.hueBar))
			{
				this.hueBar.click(x, y);
			}
			if (this.recentSliderBar.Equals(this.saturationBar))
			{
				this.saturationBar.click(x, y);
			}
			if (this.recentSliderBar.Equals(this.valueBar))
			{
				this.valueBar.click(x, y);
			}
		}
		return this.getSelectedColor();
	}

	public void releaseClick()
	{
		this.hueBar.release(0, 0);
		this.saturationBar.release(0, 0);
		this.valueBar.release(0, 0);
		this.recentSliderBar = null;
	}

	public void draw(SpriteBatch b)
	{
		for (int i = 0; i < 24; i++)
		{
			Color c = ColorPicker.HsvToRgb((double)i / 24.0 * 360.0, 0.9, 0.9);
			b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X + this.bounds.Width / 24 * i, this.bounds.Y + this.hueBar.bounds.Center.Y - 2, this.hueBar.bounds.Width / 24, 4), c);
		}
		b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + (int)((float)this.hueBar.value / 100f * (float)this.hueBar.bounds.Width), this.bounds.Y + this.hueBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
		Utility.drawTextWithShadow(b, this.hueBar.value.ToString() ?? "", Game1.smallFont, new Vector2(this.bounds.X + this.bounds.Width + 8, this.bounds.Y + this.hueBar.bounds.Y), Game1.textColor);
		for (int j = 0; j < 24; j++)
		{
			Color c2 = ColorPicker.HsvToRgb((double)this.hueBar.value / 100.0 * 360.0, (double)j / 24.0, (double)this.valueBar.value / 100.0);
			b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X + this.bounds.Width / 24 * j, this.bounds.Y + this.saturationBar.bounds.Center.Y - 2, this.saturationBar.bounds.Width / 24, 4), c2);
		}
		b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + (int)((float)this.saturationBar.value / 100f * (float)this.saturationBar.bounds.Width), this.bounds.Y + this.saturationBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.87f);
		Utility.drawTextWithShadow(b, this.saturationBar.value.ToString() ?? "", Game1.smallFont, new Vector2(this.bounds.X + this.bounds.Width + 8, this.bounds.Y + this.saturationBar.bounds.Y), Game1.textColor);
		for (int k = 0; k < 24; k++)
		{
			Color c3 = ColorPicker.HsvToRgb((double)this.hueBar.value / 100.0 * 360.0, (double)this.saturationBar.value / 100.0, (double)k / 24.0);
			b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X + this.bounds.Width / 24 * k, this.bounds.Y + this.valueBar.bounds.Center.Y - 2, this.valueBar.bounds.Width / 24, 4), c3);
		}
		b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + (int)((float)this.valueBar.value / 100f * (float)this.valueBar.bounds.Width), this.bounds.Y + this.valueBar.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
		Utility.drawTextWithShadow(b, this.valueBar.value.ToString() ?? "", Game1.smallFont, new Vector2(this.bounds.X + this.bounds.Width + 8, this.bounds.Y + this.valueBar.bounds.Y), Game1.textColor);
	}

	public bool containsPoint(int x, int y)
	{
		return this.bounds.Contains(x, y);
	}

	public void setColor(Color color)
	{
		ColorPicker.RGBtoHSV((int)color.R, (int)color.G, (int)color.B, out var hue, out var sat, out var value);
		this.setHsvColor(hue, sat, value);
	}

	public void setHsvColor(float hue, float sat, float value)
	{
		if (float.IsNaN(hue))
		{
			hue = 0f;
		}
		if (float.IsNaN(sat))
		{
			sat = 0f;
		}
		if (float.IsNaN(hue))
		{
			hue = 0f;
		}
		this.hueBar.value = (int)(hue / 360f * 100f);
		this.saturationBar.value = (int)(sat * 100f);
		this.valueBar.value = (int)(value / 255f * 100f);
	}

	/// <summary>Convert RGB color values to the equivalent HSV values.</summary>
	/// <param name="r">The red color value.</param>
	/// <param name="g">The green color value.</param>
	/// <param name="b">The blue color value.</param>
	/// <param name="h">The equivalent hue value.</param>
	/// <param name="s">The equivalent saturation value.</param>
	/// <param name="v">The equivalent color value.</param>
	public static void RGBtoHSV(float r, float g, float b, out float h, out float s, out float v)
	{
		float min = Math.Min(Math.Min(r, g), b);
		float max = (v = Math.Max(Math.Max(r, g), b));
		float delta = max - min;
		if (max != 0f)
		{
			s = delta / max;
			if (r == max)
			{
				h = (g - b) / delta;
			}
			else if (g == max)
			{
				h = 2f + (b - r) / delta;
			}
			else
			{
				h = 4f + (r - g) / delta;
			}
			h *= 60f;
			if (h < 0f)
			{
				h += 360f;
			}
		}
		else
		{
			s = 0f;
			h = -1f;
		}
	}

	/// <summary>Convert HSV color values to a MonoGame color.</summary>
	/// <param name="hue">The hue value.</param>
	/// <param name="saturation">The saturation value.</param>
	/// <param name="value">The color value.</param>
	public static Color HsvToRgb(double hue, double saturation, double value)
	{
		double H = hue;
		while (H < 0.0)
		{
			H += 1.0;
			if (H < -1000000.0)
			{
				H = 0.0;
			}
		}
		while (H >= 360.0)
		{
			H -= 1.0;
		}
		double R;
		double G;
		double B;
		if (value <= 0.0)
		{
			R = (G = (B = 0.0));
		}
		else if (saturation <= 0.0)
		{
			R = (G = (B = value));
		}
		else
		{
			double num = H / 60.0;
			int i = (int)Math.Floor(num);
			double f = num - (double)i;
			double pv = value * (1.0 - saturation);
			double qv = value * (1.0 - saturation * f);
			double tv = value * (1.0 - saturation * (1.0 - f));
			switch (i)
			{
			case 0:
				R = value;
				G = tv;
				B = pv;
				break;
			case 1:
				R = qv;
				G = value;
				B = pv;
				break;
			case 2:
				R = pv;
				G = value;
				B = tv;
				break;
			case 3:
				R = pv;
				G = qv;
				B = value;
				break;
			case 4:
				R = tv;
				G = pv;
				B = value;
				break;
			case 5:
				R = value;
				G = pv;
				B = qv;
				break;
			case 6:
				R = value;
				G = tv;
				B = pv;
				break;
			case -1:
				R = value;
				G = pv;
				B = qv;
				break;
			default:
				R = (G = (B = value));
				break;
			}
		}
		return new Color(ColorPicker.Clamp((int)(R * 255.0)), ColorPicker.Clamp((int)(G * 255.0)), ColorPicker.Clamp((int)(B * 255.0)));
	}

	/// <summary>Clamp an RGB color value to the valie range (0 to 255).</summary>
	/// <param name="value">The RGB color value.</param>
	public static int Clamp(int value)
	{
		if (value < 0)
		{
			return 0;
		}
		if (value > 255)
		{
			return 255;
		}
		return value;
	}
}
