using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Network;

public class BarGraph
{
	public static double DYNAMIC_SCALE_MAX = -1.0;

	public static double DYNAMIC_SCALE_AVG = -2.0;

	private Queue<double> elements;

	private int height;

	private int width;

	private int x;

	private int y;

	private double maxValue;

	private Color barColor;

	private int elementWidth;

	private Texture2D whiteTexture;

	public BarGraph(Queue<double> elements, int x, int y, int width, int height, int elementWidth, double maxValue, Color barColor, Texture2D whiteTexture)
	{
		this.elements = elements;
		this.width = width;
		this.height = height;
		this.x = x;
		this.y = y;
		this.maxValue = maxValue;
		this.barColor = barColor;
		this.elementWidth = elementWidth;
		this.whiteTexture = whiteTexture;
	}

	public void Draw(SpriteBatch sb)
	{
		double scaleMaxValue = this.maxValue;
		if (scaleMaxValue == BarGraph.DYNAMIC_SCALE_MAX)
		{
			foreach (double element2 in this.elements)
			{
				scaleMaxValue = Math.Max(element2, scaleMaxValue);
			}
		}
		else if (scaleMaxValue == BarGraph.DYNAMIC_SCALE_AVG)
		{
			double total = 0.0;
			foreach (double element in this.elements)
			{
				total += element;
			}
			scaleMaxValue = total / (double)Math.Max(1, this.elements.Count);
		}
		sb.Draw(this.whiteTexture, new Rectangle(this.x - 1, this.y, this.width, this.height), null, Color.Black * 0.5f);
		int leftX = this.x + this.width - this.elementWidth * this.elements.Count;
		int i = 0;
		foreach (double element3 in this.elements)
		{
			int elementX = leftX + i * this.elementWidth;
			int elementY = this.y;
			int elementHeight = (int)((double)(float)element3 / scaleMaxValue * (double)this.height);
			sb.Draw(this.whiteTexture, new Rectangle(elementX, elementY + this.height - elementHeight, this.elementWidth, elementHeight), null, this.barColor);
			i++;
		}
	}
}
