using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class Scrollbar : Element
{
	private bool DragScroll;

	public int RequestHeight { get; set; }

	public int Rows { get; set; }

	public int FrameSize { get; set; }

	public int TopRow { get; private set; }

	public int MaxTopRow => Math.Max(0, this.Rows - this.FrameSize);

	public float ScrollPercent
	{
		get
		{
			if (this.MaxTopRow <= 0)
			{
				return 0f;
			}
			return (float)this.TopRow / (float)this.MaxTopRow;
		}
	}

	public override int Width => 24;

	public override int Height => this.RequestHeight;

	public void ScrollBy(int amount)
	{
		int row = Util.Clamp(0, this.TopRow + amount, this.MaxTopRow);
		if (row != this.TopRow)
		{
			Game1.playSound("shwip", (int?)null);
			this.TopRow = row;
		}
	}

	public void ScrollTo(int row)
	{
		if (this.TopRow != row)
		{
			Game1.playSound("shiny4", (int?)null);
			this.TopRow = Util.Clamp(0, row, this.MaxTopRow);
		}
	}

	public override void Update(bool isOffScreen = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.Update(isOffScreen);
		if (base.Clicked)
		{
			this.DragScroll = true;
		}
		MouseState val;
		if ((int)Constants.TargetPlatform != 0)
		{
			if (this.DragScroll)
			{
				val = Mouse.GetState();
				if ((int)((MouseState)(ref val)).LeftButton == 0)
				{
					this.DragScroll = false;
				}
			}
		}
		else if (this.DragScroll)
		{
			val = Game1.input.GetMouseState();
			if ((int)((MouseState)(ref val)).LeftButton == 0)
			{
				this.DragScroll = false;
			}
		}
		if (this.DragScroll)
		{
			int my = Game1.getMouseY();
			int relY = (int)((float)my - base.Position.Y - 20f);
			this.ScrollTo((int)Math.Round((float)relY / (float)(this.Height - 40) * (float)this.MaxTopRow));
		}
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden() && this.MaxTopRow != 0)
		{
			Rectangle back = default(Rectangle);
			((Rectangle)(ref back))._002Ector((int)base.Position.X, (int)base.Position.Y, this.Width, this.Height);
			Vector2 front = default(Vector2);
			((Vector2)(ref front))._002Ector((float)back.X, (float)back.Y + (float)(this.Height - 40) * this.ScrollPercent);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, 4f, false, -1f);
			b.Draw(Game1.mouseCursors, front, (Rectangle?)new Rectangle(435, 463, 6, 12), Color.White, 0f, default(Vector2), 4f, (SpriteEffects)0, 0.77f);
		}
	}
}
