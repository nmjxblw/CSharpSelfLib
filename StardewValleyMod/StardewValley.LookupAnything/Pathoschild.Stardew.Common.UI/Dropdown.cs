using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.UI;

internal class Dropdown<TItem> : ClickableComponent
{
	private readonly SpriteFont Font;

	private readonly DropdownList<TItem> List;

	private readonly int BorderWidth = CommonSprites.Tab.TopLeft.Width * 2 * 4;

	private readonly int? MaxLabelWidth;

	private bool IsExpandedImpl;

	private string? DisplayLabel;

	private bool IsAndroid => (int)Constants.TargetPlatform == 0;

	public bool IsExpanded
	{
		get
		{
			return this.IsExpandedImpl;
		}
		set
		{
			this.IsExpandedImpl = value;
			base.downNeighborID = (value ? this.List.TopComponentId : this.DefaultDownNeighborId);
		}
	}

	public TItem Selected => this.List.SelectedValue;

	public int DefaultDownNeighborId { get; set; } = -99999;

	public Dropdown(int x, int y, SpriteFont font, TItem? selectedItem, TItem[] items, Func<TItem, string> getLabel, int? maxLabelWidth = null)
		: base(Rectangle.Empty, (selectedItem != null) ? getLabel(selectedItem) : string.Empty)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		this.Font = font;
		this.List = new DropdownList<TItem>(selectedItem, items, getLabel, x, y, font);
		base.bounds.X = x;
		base.bounds.Y = y;
		this.MaxLabelWidth = maxLabelWidth;
		this.ReinitializeComponents();
		this.OnValueSelected();
	}

	public override bool containsPoint(int x, int y)
	{
		if (!((ClickableComponent)this).containsPoint(x, y))
		{
			if (this.IsExpanded)
			{
				return ((ClickableComponent)this.List).containsPoint(x, y);
			}
			return false;
		}
		return true;
	}

	public bool TryClick(int x, int y)
	{
		bool itemClicked;
		bool dropdownToggled;
		return this.TryClick(x, y, out itemClicked, out dropdownToggled);
	}

	public bool TryClick(int x, int y, out bool itemClicked, out bool dropdownToggled)
	{
		itemClicked = false;
		dropdownToggled = false;
		if (this.IsExpanded && this.List.TryClick(x, y, out itemClicked))
		{
			if (itemClicked)
			{
				this.IsExpanded = false;
				dropdownToggled = true;
				this.OnValueSelected();
			}
			return true;
		}
		if (((Rectangle)(ref base.bounds)).Contains(x, y) || this.IsExpanded)
		{
			this.IsExpanded = !this.IsExpanded;
			dropdownToggled = true;
			return true;
		}
		return false;
	}

	public bool TrySelect(TItem value)
	{
		return this.List.TrySelect(value);
	}

	public void ReceiveScrollWheelAction(int direction)
	{
		if (this.IsExpanded)
		{
			this.List.ReceiveScrollWheelAction(direction);
		}
	}

	public void Draw(SpriteBatch sprites, float opacity = 1f)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		CommonHelper.DrawTab(sprites, base.bounds.X, base.bounds.Y, base.bounds.Width, this.List.MaxLabelHeight, out var textPos, 0, 1f, forIcon: false, this.IsAndroid);
		sprites.DrawString(this.Font, this.DisplayLabel, textPos, Color.Black * opacity);
		if (this.IsExpanded)
		{
			this.List.Draw(sprites, opacity);
		}
	}

	public void ReinitializeComponents()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.bounds.Height = (int)this.Font.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Y - 10 + this.BorderWidth;
		base.bounds.Width = this.List.MaxLabelWidth + this.BorderWidth;
		if (base.bounds.Width > this.MaxLabelWidth)
		{
			base.bounds.Width = this.MaxLabelWidth.Value;
		}
		((ClickableComponent)this.List).bounds.X = base.bounds.X;
		((ClickableComponent)this.List).bounds.Y = ((Rectangle)(ref base.bounds)).Bottom;
		this.List.ReinitializeComponents();
		this.ReinitializeControllerFlow();
	}

	public void ReinitializeControllerFlow()
	{
		this.List.ReinitializeControllerFlow();
		this.IsExpanded = this.IsExpanded;
	}

	public IEnumerable<ClickableComponent> GetChildComponents()
	{
		return this.List.GetChildComponents();
	}

	private void OnValueSelected()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		string displayLabel = this.List.SelectedLabel;
		if (this.MaxLabelWidth.HasValue && this.Font.MeasureString(displayLabel).X > (float?)this.MaxLabelWidth)
		{
			float ellipsisWidth = this.Font.MeasureString("...").X;
			int maxWidth = this.MaxLabelWidth.Value - (int)ellipsisWidth;
			bool truncated = false;
			while (displayLabel.Length > 10 && this.Font.MeasureString(displayLabel).X > (float)maxWidth)
			{
				string text = displayLabel;
				displayLabel = text.Substring(0, text.Length - 1);
				truncated = true;
			}
			if (truncated)
			{
				displayLabel += "...";
			}
		}
		this.DisplayLabel = displayLabel;
	}
}
