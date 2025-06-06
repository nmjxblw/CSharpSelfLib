using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class OptionsTextEntry : OptionsElement
{
	public const int pixelsHigh = 11;

	public TextBox textBox;

	public OptionsTextEntry(string label, int whichOption, int x = -1, int y = -1)
		: base(label, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44, whichOption)
	{
		this.textBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Black);
		this.textBox.Width = base.bounds.Width;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		this.textBox.X = slotX + base.bounds.Left - 8;
		this.textBox.Y = slotY + base.bounds.Top;
		this.textBox.Draw(b);
		base.draw(b, slotX, slotY, context);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y)
	{
		this.textBox.SelectMe();
		this.textBox.Update();
	}
}
