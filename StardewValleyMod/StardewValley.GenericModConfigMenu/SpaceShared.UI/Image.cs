using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceShared.UI;

internal class Image : Element, ISingleTexture
{
	public Texture2D Texture { get; set; }

	public Rectangle? TexturePixelArea { get; set; }

	public int Scale { get; set; }

	public Action<Element> Callback { get; set; }

	public override int Width => (int)this.GetActualSize().X;

	public override int Height => (int)this.GetActualSize().Y;

	public override string HoveredSound
	{
		get
		{
			if (this.Callback == null)
			{
				return null;
			}
			return "shiny4";
		}
	}

	public Color DrawColor { get; set; } = Color.White;

	public override void Update(bool isOffScreen = false)
	{
		base.Update(isOffScreen);
		if (base.Clicked)
		{
			this.Callback?.Invoke(this);
		}
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden())
		{
			b.Draw(this.Texture, base.Position, this.TexturePixelArea, this.DrawColor, 0f, Vector2.Zero, (float)this.Scale, (SpriteEffects)0, 1f);
		}
	}

	private Vector2 GetActualSize()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (this.TexturePixelArea.HasValue)
		{
			return new Vector2((float)this.TexturePixelArea.Value.Width, (float)this.TexturePixelArea.Value.Height) * (float)this.Scale;
		}
		return new Vector2((float)this.Texture.Width, (float)this.Texture.Height) * (float)this.Scale;
	}
}
