using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace Pathoschild.Stardew.Common.UI;

internal class DropdownList<TValue> : ClickableComponent
{
	private class DropListOption : ClickableComponent
	{
		public int Index { get; }

		public TValue Value { get; }

		public int LabelWidth { get; }

		public int LabelHeight { get; }

		public DropListOption(Rectangle bounds, int index, string label, TValue value, SpriteFont font)
			: base(bounds, index.ToString(), label)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			this.Index = index;
			this.Value = value;
			Vector2 labelSize = font.MeasureString(label);
			this.LabelWidth = (int)labelSize.X;
			this.LabelHeight = (int)labelSize.Y;
		}
	}

	private const int DropdownPadding = 5;

	private DropListOption SelectedOption;

	private readonly DropListOption[] Options;

	private int FirstVisibleIndex;

	private int MaxItems;

	private readonly SpriteFont Font;

	private ClickableTextureComponent UpArrow;

	private ClickableTextureComponent DownArrow;

	private int LastVisibleIndex => this.FirstVisibleIndex + this.MaxItems - 1;

	private int MaxFirstVisibleIndex => this.Options.Length - this.MaxItems;

	private bool CanScrollUp => this.FirstVisibleIndex > 0;

	private bool CanScrollDown => this.FirstVisibleIndex < this.MaxFirstVisibleIndex;

	public TValue SelectedValue => this.SelectedOption.Value;

	public string SelectedLabel => ((ClickableComponent)this.SelectedOption).label;

	public int MaxLabelHeight { get; }

	public int MaxLabelWidth { get; private set; }

	public int TopComponentId => ((ClickableComponent)this.Options.First((DropListOption p) => ((ClickableComponent)p).visible)).myID;

	public DropdownList(TValue? selectedValue, TValue[] items, Func<TValue, string> getLabel, int x, int y, SpriteFont font)
		: base(default(Rectangle), "DropdownList")
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		this.Options = items.Select((TValue item, int index) => new DropListOption(Rectangle.Empty, index, getLabel(item), item, font)).ToArray();
		this.Font = font;
		this.MaxLabelHeight = this.Options.Max((DropListOption p) => p.LabelHeight);
		int selectedIndex = Array.IndexOf<TValue>(items, selectedValue);
		this.SelectedOption = ((selectedIndex >= 0) ? this.Options[selectedIndex] : this.Options.First());
		base.bounds.X = x;
		base.bounds.Y = y;
		this.ReinitializeComponents();
	}

	public void ReceiveScrollWheelAction(int direction)
	{
		this.Scroll((direction <= 0) ? 1 : (-1));
	}

	public bool TryClick(int x, int y, out bool itemClicked)
	{
		DropListOption option = this.Options.FirstOrDefault((DropListOption p) => ((ClickableComponent)p).visible && ((ClickableComponent)p).containsPoint(x, y));
		if (option != null)
		{
			this.SelectedOption = option;
			itemClicked = true;
			return true;
		}
		itemClicked = false;
		if (((ClickableComponent)this.UpArrow).containsPoint(x, y))
		{
			this.Scroll(-1);
			return true;
		}
		if (((ClickableComponent)this.DownArrow).containsPoint(x, y))
		{
			this.Scroll(1);
			return true;
		}
		return false;
	}

	public bool TrySelect(TValue value)
	{
		DropListOption entry = this.Options.FirstOrDefault(delegate(DropListOption p)
		{
			if (p.Value != null || value != null)
			{
				TValue value2 = p.Value;
				if (value2 == null)
				{
					return false;
				}
				object obj = value;
				return value2.Equals(obj);
			}
			return true;
		});
		if (entry == null)
		{
			return false;
		}
		this.SelectedOption = entry;
		return true;
	}

	public override bool containsPoint(int x, int y)
	{
		if (!((ClickableComponent)this).containsPoint(x, y) && !((ClickableComponent)this.UpArrow).containsPoint(x, y))
		{
			return ((ClickableComponent)this.DownArrow).containsPoint(x, y);
		}
		return true;
	}

	public void Draw(SpriteBatch sprites, float opacity = 1f)
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		DropListOption[] options = this.Options;
		Vector2 position = default(Vector2);
		foreach (DropListOption option in options)
		{
			if (((ClickableComponent)option).visible)
			{
				if (((ClickableComponent)option).containsPoint(Game1.getMouseX(), Game1.getMouseY()))
				{
					sprites.Draw(CommonSprites.DropDown.Sheet, ((ClickableComponent)option).bounds, (Rectangle?)CommonSprites.DropDown.HoverBackground, Color.White * opacity);
				}
				else if (option.Index == this.SelectedOption.Index)
				{
					sprites.Draw(CommonSprites.DropDown.Sheet, ((ClickableComponent)option).bounds, (Rectangle?)CommonSprites.DropDown.ActiveBackground, Color.White * opacity);
				}
				else
				{
					sprites.Draw(CommonSprites.DropDown.Sheet, ((ClickableComponent)option).bounds, (Rectangle?)CommonSprites.DropDown.InactiveBackground, Color.White * opacity);
				}
				((Vector2)(ref position))._002Ector((float)(((ClickableComponent)option).bounds.X + 5), (float)(((ClickableComponent)option).bounds.Y + 4));
				sprites.DrawString(this.Font, ((ClickableComponent)option).label, position, Color.Black * opacity);
			}
		}
		if (this.CanScrollUp)
		{
			this.UpArrow.draw(sprites, Color.White * opacity, 1f, 0, 0, 0);
		}
		if (this.CanScrollDown)
		{
			this.DownArrow.draw(sprites, Color.White * opacity, 1f, 0, 0, 0);
		}
	}

	[MemberNotNull(new string[] { "UpArrow", "DownArrow" })]
	public void ReinitializeComponents()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		int x = base.bounds.X;
		int y = base.bounds.Y;
		int num = (this.MaxLabelWidth = Math.Max(this.Options.Max((DropListOption p) => p.LabelWidth), 128) + 10);
		int itemWidth = num;
		int itemHeight = this.MaxLabelHeight;
		this.MaxItems = Math.Min((((Rectangle)(ref Game1.uiViewport)).Height - y) / itemHeight, this.Options.Length);
		this.FirstVisibleIndex = this.GetValidFirstItem(this.FirstVisibleIndex, this.MaxFirstVisibleIndex);
		base.bounds.Width = itemWidth;
		base.bounds.Height = itemHeight * this.MaxItems;
		int itemY = y;
		DropListOption[] options = this.Options;
		foreach (DropListOption option in options)
		{
			((ClickableComponent)option).visible = option.Index >= this.FirstVisibleIndex && option.Index <= this.LastVisibleIndex;
			if (((ClickableComponent)option).visible)
			{
				((ClickableComponent)option).bounds = new Rectangle(x, itemY, itemWidth, itemHeight);
				itemY += itemHeight;
			}
		}
		Rectangle upSource = CommonSprites.Icons.UpArrow;
		Rectangle downSource = CommonSprites.Icons.DownArrow;
		this.UpArrow = new ClickableTextureComponent("up-arrow", new Rectangle(x - upSource.Width, y, upSource.Width, upSource.Height), "", "", CommonSprites.Icons.Sheet, upSource, 1f, false);
		this.DownArrow = new ClickableTextureComponent("down-arrow", new Rectangle(x - downSource.Width, y + base.bounds.Height - downSource.Height, downSource.Width, downSource.Height), "", "", CommonSprites.Icons.Sheet, downSource, 1f, false);
		this.ReinitializeControllerFlow();
	}

	public void ReinitializeControllerFlow()
	{
		int firstIndex = this.FirstVisibleIndex;
		int lastIndex = this.LastVisibleIndex;
		int initialId = 1100000;
		DropListOption[] options = this.Options;
		foreach (DropListOption option in options)
		{
			int index = option.Index;
			int id = (((ClickableComponent)option).myID = initialId + index);
			((ClickableComponent)option).upNeighborID = ((index > firstIndex) ? (id - 1) : (-99999));
			((ClickableComponent)option).downNeighborID = ((index < lastIndex) ? (id + 1) : (-1));
		}
	}

	public IEnumerable<ClickableComponent> GetChildComponents()
	{
		return (IEnumerable<ClickableComponent>)(object)this.Options;
	}

	private void Scroll(int amount)
	{
		int firstItem = this.GetValidFirstItem(this.FirstVisibleIndex + amount, this.MaxFirstVisibleIndex);
		if (firstItem != this.FirstVisibleIndex)
		{
			this.FirstVisibleIndex = firstItem;
			this.ReinitializeComponents();
		}
	}

	private int GetValidFirstItem(int value, int maxIndex)
	{
		return Math.Max(Math.Min(value, maxIndex), 0);
	}
}
