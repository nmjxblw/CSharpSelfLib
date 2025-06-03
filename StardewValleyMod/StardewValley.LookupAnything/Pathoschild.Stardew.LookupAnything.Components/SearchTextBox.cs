using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Components;

internal class SearchTextBox : IDisposable
{
	private readonly TextBox Textbox;

	private string LastText = string.Empty;

	private Rectangle BoundsImpl;

	public Rectangle Bounds
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return this.BoundsImpl;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			this.BoundsImpl = value;
			this.Textbox.X = value.X;
			this.Textbox.Y = value.Y;
			this.Textbox.Width = value.Width;
			this.Textbox.Height = value.Height;
		}
	}

	public event EventHandler<string>? OnChanged;

	public SearchTextBox(SpriteFont font, Color textColor)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		this.Textbox = new TextBox(Sprites.Textbox.Sheet, (Texture2D)null, font, textColor);
		this.Bounds = new Rectangle(this.Textbox.X, this.Textbox.Y, this.Textbox.Width, this.Textbox.Height);
	}

	public void Select()
	{
		this.Textbox.Selected = true;
	}

	public void Draw(SpriteBatch batch)
	{
		this.NotifyChange();
		this.Textbox.Draw(batch, true);
	}

	private void NotifyChange()
	{
		if (this.Textbox.Text != this.LastText)
		{
			this.OnChanged?.Invoke(this, this.Textbox.Text);
			this.LastText = this.Textbox.Text;
		}
	}

	public void Dispose()
	{
		this.OnChanged = null;
		this.Textbox.Selected = false;
	}
}
