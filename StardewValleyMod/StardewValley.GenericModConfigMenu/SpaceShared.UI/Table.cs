using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class Table : Container
{
	private readonly List<Element[]> Rows = new List<Element[]>();

	private Vector2 SizeImpl;

	private const int RowPadding = 16;

	private int RowHeightImpl;

	private bool FixedRowHeight;

	private int ContentHeight;

	public Vector2 Size
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return this.SizeImpl;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			this.SizeImpl = new Vector2(value.X, (float)((int)value.Y / this.RowHeight * this.RowHeight));
			this.UpdateScrollbar();
		}
	}

	public int RowHeight
	{
		get
		{
			return this.RowHeightImpl;
		}
		set
		{
			this.RowHeightImpl = value + 16;
			this.UpdateScrollbar();
		}
	}

	public int RowCount => this.Rows.Count;

	public Scrollbar Scrollbar { get; }

	public override int Width => (int)this.Size.X;

	public override int Height => (int)this.Size.Y;

	public Table(bool fixedRowHeight = true)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		this.FixedRowHeight = fixedRowHeight;
		base.UpdateChildren = false;
		this.Scrollbar = new Scrollbar
		{
			LocalPosition = new Vector2(0f, 0f)
		};
		base.AddChild(this.Scrollbar);
	}

	public void AddRow(Element[] elements)
	{
		this.Rows.Add(elements);
		int maxElementHeight = 0;
		foreach (Element child in elements)
		{
			base.AddChild(child);
			maxElementHeight = Math.Max(maxElementHeight, child.Height);
		}
		this.ContentHeight += (this.FixedRowHeight ? this.RowHeight : (maxElementHeight + 16));
		this.UpdateScrollbar();
	}

	public override void Update(bool isOffScreen = false)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		base.Update(isOffScreen);
		if (base.IsHidden(isOffScreen))
		{
			return;
		}
		int topPx = 0;
		foreach (Element[] row in this.Rows)
		{
			int maxElementHeight = 0;
			Element[] array = row;
			foreach (Element element in array)
			{
				element.LocalPosition = new Vector2(element.LocalPosition.X, (float)(topPx - this.Scrollbar.TopRow * this.RowHeight));
				bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(element);
				if (!isChildOffScreen || element is Label)
				{
					element.Update(isChildOffScreen);
				}
				maxElementHeight = Math.Max(maxElementHeight, element.Height);
			}
			topPx += (this.FixedRowHeight ? this.RowHeight : (maxElementHeight + 16));
		}
		if (topPx != this.ContentHeight)
		{
			this.ContentHeight = topPx;
			this.Scrollbar.Rows = this.PxToRow(this.ContentHeight);
		}
		this.Scrollbar.Update();
	}

	public void ForceUpdateEvenHidden(bool isOffScreen = false)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		int topPx = 0;
		foreach (Element[] row in this.Rows)
		{
			int maxElementHeight = 0;
			Element[] array = row;
			foreach (Element element in array)
			{
				element.LocalPosition = new Vector2(element.LocalPosition.X, (float)topPx - this.Scrollbar.ScrollPercent * (float)this.Rows.Count * (float)this.RowHeight);
				bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(element);
				element.Update(isChildOffScreen);
				maxElementHeight = Math.Max(maxElementHeight, element.Height);
			}
			topPx += (this.FixedRowHeight ? this.RowHeight : (maxElementHeight + 16));
		}
		this.ContentHeight = topPx;
		this.Scrollbar.Update(isOffScreen);
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsHidden())
		{
			return;
		}
		Rectangle backgroundArea = default(Rectangle);
		((Rectangle)(ref backgroundArea))._002Ector((int)base.Position.X - 32, (int)base.Position.Y - 32, (int)this.Size.X + 64, (int)this.Size.Y + 64);
		int contentPadding = 12;
		Rectangle contentArea = default(Rectangle);
		((Rectangle)(ref contentArea))._002Ector(backgroundArea.X + contentPadding, backgroundArea.Y + contentPadding, backgroundArea.Width - contentPadding * 2, backgroundArea.Height - contentPadding * 2);
		IClickableMenu.drawTextureBox(b, backgroundArea.X, backgroundArea.Y, backgroundArea.Width, backgroundArea.Height, Color.White);
		b.Draw(Game1.menuTexture, contentArea, (Rectangle?)new Rectangle(64, 128, 64, 64), Color.White);
		Element renderLast = null;
		this.InScissorRectangle(b, contentArea, delegate(SpriteBatch contentBatch)
		{
			foreach (Element[] current in this.Rows)
			{
				Element[] array = current;
				foreach (Element element in array)
				{
					if (!this.IsElementOffScreen(element))
					{
						if (element == base.RenderLast)
						{
							renderLast = element;
						}
						else
						{
							element.Draw(contentBatch);
						}
					}
				}
			}
		});
		renderLast?.Draw(b);
		this.Scrollbar.Draw(b);
	}

	private bool IsElementOffScreen(Element element)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!(element.Position.Y + (float)element.Height < base.Position.Y))
		{
			return element.Position.Y > base.Position.Y + this.Size.Y;
		}
		return true;
	}

	private void UpdateScrollbar()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		this.Scrollbar.LocalPosition = new Vector2(this.Size.X + 48f, this.Scrollbar.LocalPosition.Y);
		this.Scrollbar.RequestHeight = (int)this.Size.Y;
		this.Scrollbar.Rows = this.PxToRow(this.ContentHeight);
		this.Scrollbar.FrameSize = (int)(this.Size.Y / (float)this.RowHeight);
	}

	private void InScissorRectangle(SpriteBatch spriteBatch, Rectangle area, Action<SpriteBatch> draw)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		spriteBatch.End();
		SpriteBatch contentBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
		try
		{
			GraphicsDevice device = Game1.graphics.GraphicsDevice;
			Rectangle prevScissorRectangle = device.ScissorRectangle;
			try
			{
				device.ScissorRectangle = area;
				contentBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, Utility.ScissorEnabled, (Effect)null, (Matrix?)null);
				draw(contentBatch);
				contentBatch.End();
			}
			finally
			{
				device.ScissorRectangle = prevScissorRectangle;
			}
			spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null, (Effect)null, (Matrix?)null);
		}
		finally
		{
			((IDisposable)contentBatch)?.Dispose();
		}
	}

	private int PxToRow(int px)
	{
		return (px + this.RowHeight - 1) / this.RowHeight;
	}
}
