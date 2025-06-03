using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using xTile.Dimensions;

namespace GenericModConfigMenu.Framework.Overlays;

internal class KeybindOverlay
{
	private const int BoxWith = 650;

	private const int ContentPadding = 30;

	private const int KeybindListIndent = 40;

	private const string OkAction = "OK";

	private const string ClearAction = "Clear";

	private const string AddAction = "Add";

	private const string RemoveAction = "Remove";

	private readonly List<Keybind> Keybinds;

	private readonly bool OnlyAllowSingleButton;

	private readonly string Name;

	private readonly Action<Keybind[]> OnSaved;

	private readonly List<ClickableComponent> Labels = new List<ClickableComponent>();

	private readonly List<ClickableTextureComponent> Buttons = new List<ClickableTextureComponent>();

	private Rectangle Bounds;

	private bool ShouldResetLayout = true;

	private bool ButtonsChanged;

	private KeybindEdit KeybindEdit;

	public bool IsFinished { get; private set; }

	public KeybindOverlay(Keybind[] keybinds, bool onlyAllowSingleButton, string name, Action<Keybind[]> onSaved)
	{
		List<Keybind> list = new List<Keybind>(keybinds.Length);
		list.AddRange(keybinds);
		this.Keybinds = list;
		this.OnlyAllowSingleButton = onlyAllowSingleButton;
		this.Name = name;
		this.OnSaved = onSaved;
		if (onlyAllowSingleButton || keybinds.Length == 0)
		{
			this.StartEditingKeybind(0);
		}
	}

	public void OnButtonsChanged(ButtonsChangedEventArgs e)
	{
		if (e.Released.Contains((SButton)27))
		{
			Game1.playSound("bigDeSelect", (int?)null);
			this.Exit(save: false);
		}
		else if (this.KeybindEdit != null)
		{
			if (this.KeybindEdit.Add(e.Pressed))
			{
				this.ButtonsChanged = true;
			}
			if (this.KeybindEdit.Any() && e.Released.Any(this.KeybindEdit.IsValidKey))
			{
				this.FinishEditingKeybind();
			}
		}
	}

	public void OnWindowResized()
	{
		this.ShouldResetLayout = true;
	}

	public void OnLeftClick(int x, int y)
	{
		if (this.ShouldResetLayout)
		{
			return;
		}
		foreach (ClickableTextureComponent button in this.Buttons)
		{
			if (((ClickableComponent)button).containsPoint(x, y))
			{
				this.PerformButtonAction(((ClickableComponent)button).name);
				return;
			}
		}
		if (!((Rectangle)(ref this.Bounds)).Contains(x, y))
		{
			this.PerformButtonAction("OK");
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (this.ShouldResetLayout || this.ButtonsChanged)
		{
			this.ResetLayout();
		}
		spriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, ((Rectangle)(ref Game1.uiViewport)).Width, ((Rectangle)(ref Game1.uiViewport)).Height), new Color(0, 0, 0, 192));
		IClickableMenu.drawTextureBox(spriteBatch, this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height, Color.White);
		foreach (ClickableComponent label in this.Labels)
		{
			spriteBatch.DrawString(Game1.dialogueFont, label.label, new Vector2((float)label.bounds.X, (float)label.bounds.Y), Game1.textColor);
		}
		foreach (ClickableTextureComponent button in this.Buttons)
		{
			button.draw(spriteBatch);
		}
	}

	private void ResetLayout()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Expected O, but got Unknown
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Expected O, but got Unknown
		this.ButtonsChanged = false;
		this.ShouldResetLayout = false;
		this.Labels.Clear();
		this.Buttons.Clear();
		int topOffset = 30;
		string newLine = Environment.NewLine;
		topOffset += this.AddCenteredLabel(I18n.Config_RebindKey_Title(this.Name) + newLine, 0, 0, topOffset, 650).Height;
		bool isEditing = this.KeybindEdit != null;
		if (isEditing)
		{
			string text = ((!this.KeybindEdit.Any()) ? (this.OnlyAllowSingleButton ? I18n.Config_RebindKey_SimpleInstructions() : I18n.Config_RebindKey_ComboInstructions()) : this.KeybindEdit.ToString());
			topOffset += this.AddCenteredLabel(text + newLine, 0, 0, topOffset, 650).Height;
		}
		else if (this.OnlyAllowSingleButton)
		{
			string text2 = I18n.Config_RebindKey_SimpleInstructions();
			topOffset += this.AddCenteredLabel(text2 + newLine, 0, 0, topOffset, 650).Height;
		}
		if (!this.OnlyAllowSingleButton && !isEditing)
		{
			string heading = I18n.Config_RebindKey_KeybindList();
			Vector2 headingSize = Game1.dialogueFont.MeasureString(heading);
			this.Labels.Add(new ClickableComponent(new Rectangle(30, topOffset, (int)headingSize.X, (int)headingSize.Y), string.Empty, heading));
			topOffset += (int)headingSize.Y;
			Rectangle bounds = default(Rectangle);
			for (int i = 0; i < this.Keybinds.Count; i++)
			{
				string text3 = ((object)this.Keybinds[i]).ToString();
				Vector2 size = Game1.dialogueFont.MeasureString(text3);
				((Rectangle)(ref bounds))._002Ector(118, topOffset, (int)size.X, (int)size.Y);
				if (bounds.Height < 64)
				{
					bounds.Y += (64 - bounds.Height) / 2;
				}
				this.Labels.Add(new ClickableComponent(bounds, string.Empty, text3));
				this.Buttons.Add(new ClickableTextureComponent($"{"Remove"} {i}", new Rectangle(70, topOffset, 44, 44), (string)null, (string)null, Game1.mouseCursors, new Rectangle(338, 494, 11, 11), 4f, false));
				topOffset += Math.Max(bounds.Height, 44);
			}
			ClickableTextureComponent appendButton = new ClickableTextureComponent("Add", new Rectangle(70, topOffset, 40, 44), (string)null, (string)null, Game1.mouseCursors, new Rectangle(402, 361, 10, 11), 4f, false);
			this.Buttons.Add(appendButton);
			topOffset += ((ClickableComponent)appendButton).bounds.Height;
		}
		int height = topOffset + 30;
		Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(650, height, 0, 0);
		int x = Math.Max(0, (int)pos.X);
		int y = Math.Max(0, (int)pos.Y);
		this.Bounds = new Rectangle(x, y, 650, height);
		foreach (ClickableComponent label in this.Labels)
		{
			label.bounds.X += x;
			label.bounds.Y += y;
		}
		foreach (ClickableTextureComponent button in this.Buttons)
		{
			((ClickableComponent)button).bounds.X += x;
			((ClickableComponent)button).bounds.Y += y;
		}
		this.Buttons.AddRange(new _003C_003Ez__ReadOnlyArray<ClickableTextureComponent>((ClickableTextureComponent[])(object)new ClickableTextureComponent[2]
		{
			new ClickableTextureComponent("OK", new Rectangle(x + 650 - 64 - 64, y + height, 64, 64), (string)null, (string)null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false),
			new ClickableTextureComponent("Clear", new Rectangle(x + 650 - 64, y + height, 64, 64), (string)null, (string)null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
		}));
	}

	private Rectangle AddCenteredLabel(string text, int contentX, int contentY, int topOffset, int contentWidth)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		text = Game1.parseText(text, Game1.dialogueFont, contentWidth - 30 - 30);
		Vector2 size = Game1.dialogueFont.MeasureString(text);
		Rectangle bounds = default(Rectangle);
		((Rectangle)(ref bounds))._002Ector((int)((float)contentX + (float)contentWidth / 2f - size.X / 2f), contentY + topOffset, (int)size.X, (int)size.Y);
		ClickableComponent label = new ClickableComponent(bounds, "", text);
		this.Labels.Add(label);
		return label.bounds;
	}

	private void PerformButtonAction(string action)
	{
		switch (action)
		{
		case "Clear":
			Game1.playSound("coin", (int?)null);
			this.Keybinds.Clear();
			this.Exit();
			return;
		case "OK":
			Game1.playSound("bigDeSelect", (int?)null);
			this.Exit();
			return;
		case "Add":
			this.StartEditingKeybind(this.Keybinds.Count);
			return;
		}
		if (action.StartsWith("Remove"))
		{
			int keyBindIndex = ArgUtility.GetInt(action.Split(' '), 1, 0);
			this.Keybinds.RemoveAt(keyBindIndex);
			this.ShouldResetLayout = true;
		}
	}

	private void StartEditingKeybind(int index)
	{
		this.KeybindEdit = new KeybindEdit(index, this.OnlyAllowSingleButton);
		this.ShouldResetLayout = true;
	}

	private void FinishEditingKeybind()
	{
		KeybindEdit edit = this.KeybindEdit;
		if (edit != null)
		{
			Keybind keybind = edit.ToKeybind();
			if (this.OnlyAllowSingleButton)
			{
				this.Keybinds.Clear();
				this.Keybinds.Add(keybind);
			}
			else if (edit.Index >= this.Keybinds.Count)
			{
				this.Keybinds.Add(keybind);
			}
			else
			{
				this.Keybinds[edit.Index] = keybind;
			}
			if (this.Keybinds.Count > 1)
			{
				HashSet<string> seen = new HashSet<string>();
				CollectionExtensions.RemoveWhere<Keybind>((IList<Keybind>)this.Keybinds, (Predicate<Keybind>)((Keybind bind) => !seen.Add(string.Join(" + ", bind.Buttons.OrderBy((SButton button) => button)))));
			}
		}
		this.KeybindEdit = null;
		this.ShouldResetLayout = true;
		if (this.OnlyAllowSingleButton)
		{
			this.Exit();
		}
	}

	private void Exit(bool save = true)
	{
		if (save)
		{
			this.OnSaved(this.Keybinds.ToArray());
		}
		this.IsFinished = true;
		this.KeybindEdit = null;
	}
}
