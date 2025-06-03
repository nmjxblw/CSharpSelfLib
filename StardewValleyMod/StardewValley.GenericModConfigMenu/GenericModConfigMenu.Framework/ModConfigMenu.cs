using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace GenericModConfigMenu.Framework;

internal class ModConfigMenu : IClickableMenu
{
	private RootElement Ui;

	private readonly Table Table;

	private readonly int ScrollSpeed;

	private readonly Action<IManifest, int> OpenModMenu;

	private List<Label> LabelsWithTooltips = new List<Label>();

	private int scrollCounter;

	private bool InGame => Context.IsWorldReady;

	public int ScrollRow
	{
		get
		{
			return this.Table.Scrollbar.TopRow;
		}
		set
		{
			this.Table.Scrollbar.ScrollTo(value);
		}
	}

	public ModConfigMenu(int scrollSpeed, Action<IManifest, int> openModMenu, Action<int> openKeybindsMenu, ModConfigManager configs, Texture2D keybindsTexture, int? scrollTo = null)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		ModConfigMenu modConfigMenu = this;
		this.ScrollSpeed = scrollSpeed;
		this.OpenModMenu = openModMenu;
		this.Ui = new RootElement();
		this.Table = new Table
		{
			RowHeight = 50,
			LocalPosition = new Vector2((float)((((Rectangle)(ref Game1.uiViewport)).Width - 800) / 2), 64f),
			Size = new Vector2(800f, (float)(((Rectangle)(ref Game1.uiViewport)).Height - 128))
		};
		Label heading = new Label
		{
			String = I18n.List_EditableHeading(),
			Bold = true
		};
		heading.LocalPosition = new Vector2((800f - heading.Measure().X) / 2f, heading.LocalPosition.Y);
		this.Table.AddRow(new Element[1] { heading });
		ModConfig[] editable = (from modConfig in configs.GetAll()
			where modConfig.AnyEditableInGame || !modConfigMenu.InGame
			orderby modConfig.ModName
			select modConfig).ToArray();
		ModConfig[] array = editable;
		foreach (ModConfig entry in array)
		{
			Label label = new Label
			{
				String = entry.ModName,
				UserData = entry.ModManifest.Description,
				Callback = delegate
				{
					modConfigMenu.ChangeToModPage(entry.ModManifest);
				}
			};
			this.Table.AddRow(new Element[1] { label });
			this.LabelsWithTooltips.Add(label);
		}
		ModConfig[] notEditable = (from modConfig in configs.GetAll()
			where !modConfig.AnyEditableInGame && modConfigMenu.InGame
			orderby modConfig.ModName
			select modConfig).ToArray();
		if (notEditable.Any())
		{
			Label heading2 = new Label
			{
				String = I18n.List_NotEditableHeading(),
				Bold = true
			};
			this.Table.AddRow(Array.Empty<Element>());
			this.Table.AddRow(new Element[1] { heading2 });
			ModConfig[] array2 = notEditable;
			foreach (ModConfig entry2 in array2)
			{
				Label label2 = new Label
				{
					String = entry2.ModName,
					UserData = entry2.ModManifest.Description,
					IdleTextColor = Color.Black * 0.4f,
					HoverTextColor = Color.Black * 0.4f
				};
				this.Table.AddRow(new Element[1] { label2 });
				this.LabelsWithTooltips.Add(label2);
			}
		}
		this.Ui.AddChild(this.Table);
		Button button = new Button(keybindsTexture)
		{
			LocalPosition = this.Table.LocalPosition - new Vector2((float)(keybindsTexture.Width / 2 + 32), 0f),
			Callback = delegate
			{
				openKeybindsMenu(modConfigMenu.ScrollRow);
			}
		};
		this.Ui.AddChild(button);
		if ((int)Constants.TargetPlatform == 0)
		{
			((IClickableMenu)this).initializeUpperRightCloseButton();
		}
		else
		{
			base.upperRightCloseButton = null;
		}
		if (scrollTo.HasValue)
		{
			this.ScrollRow = scrollTo.Value;
		}
		if (!this.InGame)
		{
			((Mod)Mod.instance).Helper.Reflection.GetField<bool>((object)Game1.activeClickableMenu, "titleInPosition", true).SetValue(false);
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		ClickableTextureComponent upperRightCloseButton = base.upperRightCloseButton;
		if (upperRightCloseButton != null && ((ClickableComponent)upperRightCloseButton).containsPoint(x, y) && ((IClickableMenu)this).readyToClose())
		{
			if (playSound)
			{
				Game1.playSound("bigDeSelect", (int?)null);
			}
			Mod.ActiveConfigMenu = null;
		}
	}

	public override void receiveScrollWheelAction(int direction)
	{
		this.Table.Scrollbar.ScrollBy(direction / -this.ScrollSpeed);
	}

	public override void update(GameTime time)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		((IClickableMenu)this).update(time);
		this.Ui.Update();
		GamePadState gamePadState = Game1.input.GetGamePadState();
		GamePadThumbSticks thumbSticks = ((GamePadState)(ref gamePadState)).ThumbSticks;
		if (((GamePadThumbSticks)(ref thumbSticks)).Right.Y != 0f)
		{
			if (++this.scrollCounter == 5)
			{
				this.scrollCounter = 0;
				Scrollbar scrollbar = this.Table.Scrollbar;
				gamePadState = Game1.input.GetGamePadState();
				thumbSticks = ((GamePadState)(ref gamePadState)).ThumbSticks;
				scrollbar.ScrollBy(Math.Sign(((GamePadThumbSticks)(ref thumbSticks)).Right.Y) * 120 / -this.ScrollSpeed);
			}
		}
		else
		{
			this.scrollCounter = 0;
		}
	}

	public override void draw(SpriteBatch b)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		((IClickableMenu)this).draw(b);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, ((Rectangle)(ref Game1.uiViewport)).Width, ((Rectangle)(ref Game1.uiViewport)).Height), new Color(0, 0, 0, 192));
		this.Ui.Draw(b);
		ClickableTextureComponent upperRightCloseButton = base.upperRightCloseButton;
		if (upperRightCloseButton != null)
		{
			upperRightCloseButton.draw(b);
		}
		if (this.InGame)
		{
			((IClickableMenu)this).drawMouse(b, false, -1);
		}
		if ((int)Constants.TargetPlatform == 0 || ((IClickableMenu)this).GetChildMenu() != null)
		{
			return;
		}
		foreach (Label label in this.LabelsWithTooltips)
		{
			if (label.Hover && label.UserData != null)
			{
				string text = (string)label.UserData;
				if (text != null && !text.Contains("\n"))
				{
					text = Game1.parseText(text, Game1.smallFont, 800);
				}
				string title = label.String;
				if (title != null && !title.Contains("\n"))
				{
					title = Game1.parseText(title, Game1.dialogueFont, 800);
				}
				IClickableMenu.drawToolTip(b, text, title, (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
			}
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		RootElement oldUi = this.Ui;
		this.Ui = new RootElement();
		Vector2 newSize = default(Vector2);
		((Vector2)(ref newSize))._002Ector(800f, (float)(((Rectangle)(ref Game1.uiViewport)).Height - 128));
		this.Table.LocalPosition = new Vector2((float)((((Rectangle)(ref Game1.uiViewport)).Width - 800) / 2), 64f);
		Element[] children = this.Table.Children;
		foreach (Element opt in children)
		{
			opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);
		}
		this.Table.Size = newSize;
		this.Table.Scrollbar.Update();
		this.Ui.AddChild(this.Table);
		Element b = oldUi.Children.First((Element e) => e is Button);
		oldUi.RemoveChild(b);
		this.Ui.AddChild(b);
	}

	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return true;
	}

	private void ChangeToModPage(IManifest modManifest)
	{
		Log.Trace("Changing to mod config page for mod " + modManifest.UniqueID);
		Game1.playSound("bigSelect", (int?)null);
		this.OpenModMenu(modManifest, this.ScrollRow);
	}
}
