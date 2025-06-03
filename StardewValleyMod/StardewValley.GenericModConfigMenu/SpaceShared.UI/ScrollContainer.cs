using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class ScrollContainer : StaticContainer
{
	private int ContentHeight;

	public int lastScroll;

	public Scrollbar Scrollbar { get; }

	public override int Width => (int)base.Size.X;

	public override int Height => (int)base.Size.Y;

	public ScrollContainer()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateChildren = false;
		this.Scrollbar = new Scrollbar
		{
			LocalPosition = new Vector2(0f, 0f)
		};
		base.AddChild(this.Scrollbar);
	}

	public override void OnChildrenChanged()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		int topPx = 0;
		Element[] children = base.Children;
		foreach (Element child in children)
		{
			if (child != this.Scrollbar)
			{
				topPx = Math.Max(topPx, child.Bounds.Y + child.Bounds.Height);
			}
		}
		if (topPx != this.ContentHeight)
		{
			this.ContentHeight = topPx;
			this.Scrollbar.Rows = this.PxToRow(this.ContentHeight);
		}
		this.UpdateScrollbar();
	}

	public override void Update(bool isOffScreen = false)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.Update(isOffScreen);
		if (base.IsHidden(isOffScreen))
		{
			return;
		}
		if (this.lastScroll != this.Scrollbar.TopRow)
		{
			float diff = this.lastScroll * 50 - this.Scrollbar.TopRow * 50;
			this.lastScroll = this.Scrollbar.TopRow;
			Element[] children = base.Children;
			foreach (Element child in children)
			{
				if (child != this.Scrollbar)
				{
					child.LocalPosition = new Vector2(child.LocalPosition.X, child.LocalPosition.Y + diff);
				}
			}
		}
		Element[] children2 = base.Children;
		foreach (Element child2 in children2)
		{
			if (child2 != this.Scrollbar)
			{
				bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(child2);
				if (!isChildOffScreen || child2 is Label)
				{
					child2.Update(isChildOffScreen);
				}
			}
		}
		this.Scrollbar.Update();
	}

	public void ForceUpdateEvenHidden(bool isOffScreen = false)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (this.lastScroll != this.Scrollbar.TopRow)
		{
			float diff = this.lastScroll * 50 - this.Scrollbar.TopRow * 50;
			this.lastScroll = this.Scrollbar.TopRow;
			Element[] children = base.Children;
			foreach (Element child in children)
			{
				if (child != this.Scrollbar)
				{
					child.LocalPosition = new Vector2(child.LocalPosition.X, child.LocalPosition.Y + diff);
				}
			}
		}
		Element[] children2 = base.Children;
		foreach (Element child2 in children2)
		{
			if (child2 != this.Scrollbar)
			{
				bool isChildOffScreen = isOffScreen || this.IsElementOffScreen(child2);
				if (!isChildOffScreen || child2 is Label)
				{
					child2.Update(isChildOffScreen);
				}
			}
		}
		this.Scrollbar.Update(isOffScreen);
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsHidden())
		{
			return;
		}
		if (base.OutlineColor.HasValue)
		{
			IClickableMenu.drawTextureBox(b, (int)base.Position.X - 12, (int)base.Position.Y - 12, this.Width + 24, this.Height + 24, base.OutlineColor.Value);
		}
		Rectangle backgroundArea = default(Rectangle);
		((Rectangle)(ref backgroundArea))._002Ector((int)base.Position.X - 32, (int)base.Position.Y - 32, (int)base.Size.X + 64, (int)base.Size.Y + 64);
		int contentPadding = 12;
		Rectangle contentArea = default(Rectangle);
		((Rectangle)(ref contentArea))._002Ector(backgroundArea.X + 20 + contentPadding, backgroundArea.Y + 20 + contentPadding, backgroundArea.Width - 40 - contentPadding * 2, backgroundArea.Height - 40 - contentPadding * 2);
		Element renderLast = null;
		this.InScissorRectangle(b, contentArea, delegate(SpriteBatch contentBatch)
		{
			Element[] children = base.Children;
			foreach (Element element in children)
			{
				if (element != this.Scrollbar && !this.IsElementOffScreen(element))
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
			return element.Position.Y > base.Position.Y + base.Size.Y;
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
		this.Scrollbar.LocalPosition = new Vector2(base.Size.X + 48f, this.Scrollbar.LocalPosition.Y);
		this.Scrollbar.RequestHeight = (int)base.Size.Y;
		this.Scrollbar.Rows = this.PxToRow(this.ContentHeight);
		this.Scrollbar.FrameSize = (int)(base.Size.Y / 50f);
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
		return (px + 50 - 1) / 50;
	}
}
