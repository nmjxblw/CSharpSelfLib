using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace SpaceShared.UI;

internal abstract class Element
{
	public Func<bool> ForceHide;

	public object UserData { get; set; }

	public Container Parent { get; internal set; }

	public Vector2 LocalPosition { get; set; }

	public Vector2 Position
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (this.Parent != null)
			{
				return this.Parent.Position + this.LocalPosition;
			}
			return this.LocalPosition;
		}
	}

	public abstract int Width { get; }

	public abstract int Height { get; }

	public Rectangle Bounds => new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);

	public bool Hover { get; private set; }

	public virtual string HoveredSound => null;

	public bool ClickGestured { get; private set; }

	public bool Clicked
	{
		get
		{
			if (this.Hover)
			{
				return this.ClickGestured;
			}
			return false;
		}
	}

	public virtual string ClickedSound => null;

	public virtual void Update(bool isOffScreen = false)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between Unknown and I4
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		bool hidden = this.IsHidden(isOffScreen);
		if (hidden)
		{
			this.Hover = false;
			this.ClickGestured = false;
			return;
		}
		int mouseX;
		int mouseY;
		if ((int)Constants.TargetPlatform == 0)
		{
			mouseX = Game1.getMouseX();
			mouseY = Game1.getMouseY();
		}
		else
		{
			mouseX = Game1.getOldMouseX();
			mouseY = Game1.getOldMouseY();
		}
		int num;
		if (!hidden && !this.GetRoot().Obscured)
		{
			Rectangle bounds = this.Bounds;
			num = (((Rectangle)(ref bounds)).Contains(mouseX, mouseY) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool newHover = (byte)num != 0;
		if (newHover && !this.Hover && this.HoveredSound != null)
		{
			Game1.playSound(this.HoveredSound, (int?)null);
		}
		this.Hover = newHover;
		MouseState mouseState = Game1.input.GetMouseState();
		this.ClickGestured = (int)((MouseState)(ref mouseState)).LeftButton == 1 && (int)((MouseState)(ref Game1.oldMouseState)).LeftButton == 0;
		int clickGestured;
		if (!this.ClickGestured)
		{
			if (Game1.options.gamepadControls)
			{
				GamePadState gamePadState = Game1.input.GetGamePadState();
				clickGestured = ((((GamePadState)(ref gamePadState)).IsButtonDown((Buttons)4096) && !((GamePadState)(ref Game1.oldPadState)).IsButtonDown((Buttons)4096)) ? 1 : 0);
			}
			else
			{
				clickGestured = 0;
			}
		}
		else
		{
			clickGestured = 1;
		}
		this.ClickGestured = (byte)clickGestured != 0;
		if (this.ClickGestured && (Dropdown.SinceDropdownWasActive > 0 || Dropdown.ActiveDropdown != null))
		{
			this.ClickGestured = false;
		}
		if (this.Clicked && this.ClickedSound != null)
		{
			Game1.playSound(this.ClickedSound, (int?)null);
		}
	}

	public abstract void Draw(SpriteBatch b);

	public RootElement GetRoot()
	{
		return this.GetRootImpl();
	}

	internal virtual RootElement GetRootImpl()
	{
		if (this.Parent == null)
		{
			throw new Exception("Element must have a parent.");
		}
		return this.Parent.GetRoot();
	}

	public bool IsHidden(bool isOffScreen = false)
	{
		if (!isOffScreen)
		{
			return this.ForceHide?.Invoke() ?? false;
		}
		return true;
	}
}
