using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class ButtonTutorialMenu : IClickableMenu
{
	public const int move_run_check = 0;

	public const int useTool_menu = 1;

	public const float movementSpeed = 0.2f;

	public new const int width = 42;

	public new const int height = 109;

	private int timerToclose = 15000;

	private int which;

	private static int current;

	private int myID;

	public ButtonTutorialMenu(int which)
		: base(-168, Game1.uiViewport.Height / 2 - 218, 168, 436)
	{
		this.which = which;
		ButtonTutorialMenu.current++;
		this.myID = ButtonTutorialMenu.current;
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.myID != ButtonTutorialMenu.current)
		{
			base.destroy = true;
		}
		if (base.xPositionOnScreen < 0 && this.timerToclose > 0)
		{
			base.xPositionOnScreen += (int)((float)time.ElapsedGameTime.Milliseconds * 0.2f);
			if (base.xPositionOnScreen >= 0)
			{
				base.xPositionOnScreen = 0;
			}
			return;
		}
		this.timerToclose -= time.ElapsedGameTime.Milliseconds;
		if (this.timerToclose <= 0)
		{
			if (base.xPositionOnScreen >= -232)
			{
				base.xPositionOnScreen -= (int)((float)time.ElapsedGameTime.Milliseconds * 0.2f);
			}
			else
			{
				base.destroy = true;
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!base.destroy)
		{
			if (!Game1.options.gamepadControls)
			{
				b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), new Rectangle(275 + this.which * 42, 0, 42, 109), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.82f);
			}
			else
			{
				b.Draw(Game1.controllerMaps, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), Utility.controllerMapSourceRect(new Rectangle(512 + this.which * 42 * 2, 0, 84, 218)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.82f);
			}
		}
	}
}
