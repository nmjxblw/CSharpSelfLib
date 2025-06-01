using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

internal class TextCreditsBlock : ICreditsBlock
{
	private string text;

	private Color color;

	private bool renderNameInEnglish;

	public TextCreditsBlock(string rawtext)
	{
		string[] split = rawtext.Split(']');
		if (split.Length > 1)
		{
			this.text = split[1];
			this.color = SpriteText.getColorFromIndex(Convert.ToInt32(split[0].Substring(1)));
		}
		else
		{
			this.text = split[0];
			this.color = SpriteText.color_White;
		}
		if (SpriteText.IsMissingCharacters(rawtext))
		{
			this.renderNameInEnglish = true;
		}
	}

	public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
	{
		if (this.renderNameInEnglish)
		{
			int parenthesis_index = this.text.IndexOf('(');
			if (parenthesis_index != -1 && parenthesis_index > 0)
			{
				string name = this.text.Substring(0, parenthesis_index);
				string parenthesis_text = this.text.Substring(parenthesis_index);
				SpriteText.forceEnglishFont = true;
				int width_of_text = (int)((float)SpriteText.getWidthOfString(name) / SpriteText.FontPixelZoom * 3f);
				SpriteText.drawString(b, name, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", this.color);
				SpriteText.forceEnglishFont = false;
				SpriteText.drawString(b, parenthesis_text, topLeftX + width_of_text, topLeftY, 999999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", this.color);
			}
			else
			{
				SpriteText.forceEnglishFont = true;
				SpriteText.drawString(b, this.text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", this.color);
				SpriteText.forceEnglishFont = false;
			}
		}
		else
		{
			SpriteText.drawString(b, this.text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", this.color);
		}
	}

	public override int getHeight(int maxWidth)
	{
		if (!(this.text == ""))
		{
			return SpriteText.getHeightOfString(this.text, maxWidth);
		}
		return 64;
	}
}
