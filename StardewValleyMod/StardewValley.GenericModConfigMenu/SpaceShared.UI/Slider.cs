using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class Slider : Element
{
	protected bool Dragging;

	public int RequestWidth { get; set; }

	public Action<Element> Callback { get; set; }

	public override int Width => this.RequestWidth;

	public override int Height => 24;

	public override void Draw(SpriteBatch b)
	{
	}
}
internal class Slider<T> : Slider
{
	public T Minimum { get; set; }

	public T Maximum { get; set; }

	public T Value { get; set; }

	public T Interval { get; set; }

	public override void Update(bool isOffScreen = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		base.Update(isOffScreen);
		if (base.Clicked)
		{
			base.Dragging = true;
		}
		MouseState val;
		GamePadState gamePadState;
		GamePadButtons buttons;
		if ((int)Constants.TargetPlatform != 0)
		{
			val = Mouse.GetState();
			if ((int)((MouseState)(ref val)).LeftButton == 0)
			{
				gamePadState = Game1.input.GetGamePadState();
				buttons = ((GamePadState)(ref gamePadState)).Buttons;
				if ((int)((GamePadButtons)(ref buttons)).A == 0)
				{
					base.Dragging = false;
				}
			}
		}
		else
		{
			val = Game1.input.GetMouseState();
			if ((int)((MouseState)(ref val)).LeftButton == 0)
			{
				gamePadState = Game1.input.GetGamePadState();
				buttons = ((GamePadState)(ref gamePadState)).Buttons;
				if ((int)((GamePadButtons)(ref buttons)).A == 0)
				{
					base.Dragging = false;
				}
			}
		}
		if (base.Dragging)
		{
			float perc = ((float)Game1.getOldMouseX() - base.Position.X) / (float)this.Width;
			this.Value = Util.Adjust(this.Value, this.Interval);
			T value = this.Value;
			T value2 = ((value is int) ? Util.Clamp(this.Minimum, (T)(object)(int)(perc * (float)((int)(object)this.Maximum - (int)(object)this.Minimum) + (float)(int)(object)this.Minimum), this.Maximum) : ((!(value is float)) ? this.Value : Util.Clamp(this.Minimum, (T)(object)(perc * ((float)(object)this.Maximum - (float)(object)this.Minimum) + (float)(object)this.Minimum), this.Maximum)));
			this.Value = value2;
			base.Callback?.Invoke(this);
		}
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden())
		{
			T value = this.Value;
			float num = ((value is int) ? ((float)((int)(object)this.Value - (int)(object)this.Minimum) / (float)((int)(object)this.Maximum - (int)(object)this.Minimum)) : ((!(value is float)) ? 0f : (((float)(object)this.Value - (float)(object)this.Minimum) / ((float)(object)this.Maximum - (float)(object)this.Minimum))));
			float perc = num;
			Rectangle back = default(Rectangle);
			((Rectangle)(ref back))._002Ector((int)base.Position.X, (int)base.Position.Y, this.Width, this.Height);
			Rectangle front = default(Rectangle);
			((Rectangle)(ref front))._002Ector((int)(base.Position.X + perc * (float)(this.Width - 40)), (int)base.Position.Y, 40, this.Height);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, 4f, false, -1f);
			b.Draw(Game1.mouseCursors, new Vector2((float)front.X, (float)front.Y), (Rectangle?)new Rectangle(420, 441, 10, 6), Color.White, 0f, Vector2.Zero, 4f, (SpriteEffects)0, 0.9f);
		}
	}
}
