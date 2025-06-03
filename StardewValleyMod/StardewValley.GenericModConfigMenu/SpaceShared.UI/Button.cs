using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class Button : Element, ISingleTexture
{
	private float Scale = 1f;

	public Texture2D Texture { get; set; }

	public Rectangle IdleTextureRect { get; set; }

	public Rectangle HoverTextureRect { get; set; }

	public Action<Element> Callback { get; set; }

	public override int Width => this.IdleTextureRect.Width;

	public override int Height => this.IdleTextureRect.Height;

	public override string HoveredSound => "Cowboy_Footstep";

	public Button()
	{
	}

	public Button(Texture2D tex)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		this.Texture = tex;
		this.IdleTextureRect = new Rectangle(0, 0, tex.Width / 2, tex.Height);
		this.HoverTextureRect = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);
	}

	public override void Update(bool isOffScreen = false)
	{
		base.Update(isOffScreen);
		this.Scale = (base.Hover ? Math.Min(this.Scale + 0.013f, 1.083f) : Math.Max(this.Scale - 0.013f, 1f));
		if (base.Clicked)
		{
			this.Callback?.Invoke(this);
		}
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden())
		{
			Rectangle texRect = (base.Hover ? this.HoverTextureRect : this.IdleTextureRect);
			Vector2 origin = default(Vector2);
			((Vector2)(ref origin))._002Ector((float)texRect.Width / 2f, (float)texRect.Height / 2f);
			b.Draw(this.Texture, base.Position + origin, (Rectangle?)texRect, Color.White, 0f, origin, this.Scale, (SpriteEffects)0, 0f);
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			if (activeClickableMenu != null)
			{
				activeClickableMenu.drawMouse(b, false, -1);
			}
		}
	}
}
