using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class OptionsInputListener : OptionsElement
{
	public List<string> buttonNames = new List<string>();

	private string listenerMessage;

	private bool listening;

	private Rectangle setbuttonBounds;

	public static Rectangle setButtonSource = new Rectangle(294, 428, 21, 11);

	public OptionsInputListener(string label, int whichOption, int slotWidth, int x = -1, int y = -1)
		: base(label, x, y, slotWidth - x, 44, whichOption)
	{
		this.setbuttonBounds = new Rectangle(slotWidth - 112, y + 12, 84, 44);
		if (whichOption != -1)
		{
			Game1.options.setInputListenerToProperValue(this);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y)
	{
		if (base.greyedOut || this.listening || !this.setbuttonBounds.Contains(x, y))
		{
			return;
		}
		if (base.whichOption == -1)
		{
			Game1.options.setControlsToDefault();
			if (!(Game1.activeClickableMenu is GameMenu gameMenu) || !(gameMenu.GetCurrentPage() is OptionsPage optionsPage))
			{
				return;
			}
			{
				foreach (OptionsElement option in optionsPage.options)
				{
					if (option is OptionsInputListener listener)
					{
						Game1.options.setInputListenerToProperValue(listener);
					}
				}
				return;
			}
		}
		this.listening = true;
		Game1.playSound("breathin");
		GameMenu.forcePreventClose = true;
		this.listenerMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsElement.cs.11225");
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (!base.greyedOut && this.listening)
		{
			if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.GetCurrentPage() is OptionsPage optionsPage)
			{
				optionsPage.lastRebindTick = Game1.ticks;
			}
			if (key == Keys.Escape)
			{
				Game1.playSound("bigDeSelect");
				this.listening = false;
				GameMenu.forcePreventClose = false;
			}
			else if (!Game1.options.isKeyInUse(key) || new InputButton(key).ToString().Equals(this.buttonNames[0]))
			{
				Game1.options.changeInputListenerValue(base.whichOption, key);
				this.buttonNames[0] = new InputButton(key).ToString();
				Game1.playSound("coin");
				this.listening = false;
				GameMenu.forcePreventClose = false;
			}
			else
			{
				this.listenerMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsElement.cs.11228");
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		if (this.buttonNames.Count > 0 || base.whichOption == -1)
		{
			if (base.whichOption == -1)
			{
				Utility.drawTextWithShadow(b, base.label, Game1.dialogueFont, new Vector2(base.bounds.X + slotX, base.bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
			}
			else
			{
				Utility.drawTextWithShadow(b, base.label + ": " + this.buttonNames.Last() + ((this.buttonNames.Count > 1) ? (", " + this.buttonNames[0]) : ""), Game1.dialogueFont, new Vector2(base.bounds.X + slotX, base.bounds.Y + slotY), Game1.textColor, 1f, 0.15f);
			}
		}
		Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(this.setbuttonBounds.X + slotX, this.setbuttonBounds.Y + slotY), OptionsInputListener.setButtonSource, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.15f);
		if (this.listening)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(0, 0, 1, 1), Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0.999f);
			b.DrawString(Game1.dialogueFont, this.listenerMessage, Utility.getTopLeftPositionForCenteringOnScreen(192, 64), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999f);
		}
	}
}
