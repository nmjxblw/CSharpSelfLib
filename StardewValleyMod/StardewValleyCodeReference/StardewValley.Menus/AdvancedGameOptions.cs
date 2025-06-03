using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class AdvancedGameOptions : IClickableMenu
{
	public const int itemsPerPage = 7;

	private string hoverText = "";

	public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

	public int currentItemIndex;

	private ClickableTextureComponent upArrow;

	private ClickableTextureComponent downArrow;

	private ClickableTextureComponent scrollBar;

	public ClickableTextureComponent okButton;

	public List<Action> applySettingCallbacks = new List<Action>();

	public Dictionary<OptionsElement, string> tooltips = new Dictionary<OptionsElement, string>();

	public int ID_okButton = 10000;

	private bool scrolling;

	public List<OptionsElement> options = new List<OptionsElement>();

	private Rectangle scrollBarBounds;

	protected static int _lastSelectedIndex;

	protected static int _lastCurrentItemIndex;

	protected int _lastHoveredIndex;

	protected int _hoverDuration;

	public const int WINDOW_WIDTH = 800;

	public const int WINDOW_HEIGHT = 500;

	public bool initialMonsterSpawnAtValue;

	private int optionsSlotHeld = -1;

	public AdvancedGameOptions()
		: base(Game1.uiViewport.Width / 2 - 400, Game1.uiViewport.Height / 2 - 250, 800, 500)
	{
		this.ResetComponents();
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - 400;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - 250;
		this.ResetComponents();
	}

	private void ResetComponents()
	{
		int scrollbarX = base.xPositionOnScreen + base.width + 16;
		this.upArrow = new ClickableTextureComponent(new Rectangle(scrollbarX, base.yPositionOnScreen, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
		this.downArrow = new ClickableTextureComponent(new Rectangle(scrollbarX, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
		this.scrollBarBounds = new Rectangle
		{
			X = this.upArrow.bounds.X + 12,
			Y = this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4,
			Width = 24
		};
		this.scrollBarBounds.Height = this.downArrow.bounds.Y - 4 - this.scrollBarBounds.Y;
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.scrollBarBounds.X, this.scrollBarBounds.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		this.optionSlots.Clear();
		for (int i = 0; i < 7; i++)
		{
			this.optionSlots.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + i * ((base.height - 16) / 7), base.width - 16, base.height / 7), i.ToString() ?? "")
			{
				myID = i,
				downNeighborID = ((i < 6) ? (i + 1) : (-7777)),
				upNeighborID = ((i > 0) ? (i - 1) : (-7777)),
				fullyImmutable = true
			});
		}
		this.PopulateOptions();
		this.okButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen, base.yPositionOnScreen + base.height + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = this.ID_okButton,
			upNeighborID = -99998
		};
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.setCurrentlySnappedComponentTo(this.ID_okButton);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		base.customSnapBehavior(direction, oldRegion, oldID);
		switch (oldID)
		{
		case 6:
			if (direction != 2)
			{
				break;
			}
			if (this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shiny4");
				break;
			}
			base.currentlySnappedComponent = base.getComponentWithID(this.ID_okButton);
			if (base.currentlySnappedComponent != null)
			{
				base.currentlySnappedComponent.upNeighborID = Math.Min(this.options.Count, 7) - 1;
			}
			break;
		case 0:
			if (direction == 0)
			{
				if (this.currentItemIndex > 0)
				{
					this.upArrowPressed();
					Game1.playSound("shiny4");
				}
				else
				{
					this.snapCursorToCurrentSnappedComponent();
				}
			}
			break;
		}
	}

	public virtual void PopulateOptions()
	{
		this.options.Clear();
		this.tooltips.Clear();
		this.applySettingCallbacks.Clear();
		this.AddHeader(Game1.content.LoadString("Strings\\UI:AGO_Label"));
		this.AddDropdown(Game1.content.LoadString("Strings\\UI:AGO_CCB"), Game1.content.LoadString("Strings\\UI:AGO_CCB_Tooltip"), true, () => Game1.bundleType, delegate(Game1.BundleType val)
		{
			Game1.bundleType = val;
		}, new KeyValuePair<string, Game1.BundleType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Normal"), Game1.BundleType.Default), new KeyValuePair<string, Game1.BundleType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Remixed"), Game1.BundleType.Remixed));
		this.AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_Year1Completable"), Game1.content.LoadString("Strings\\UI:AGO_Year1Completable_Tooltip"), () => Game1.game1.GetNewGameOption<bool>("YearOneCompletable"), delegate(bool val)
		{
			Game1.game1.SetNewGameOption("YearOneCompletable", val);
		});
		this.AddDropdown(Game1.content.LoadString("Strings\\UI:AGO_MineTreasureShuffle"), Game1.content.LoadString("Strings\\UI:AGO_MineTreasureShuffle_Tooltip"), true, () => Game1.game1.GetNewGameOption<Game1.MineChestType>("MineChests"), delegate(Game1.MineChestType val)
		{
			Game1.game1.SetNewGameOption("MineChests", val);
		}, new KeyValuePair<string, Game1.MineChestType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Normal"), Game1.MineChestType.Default), new KeyValuePair<string, Game1.MineChestType>(Game1.content.LoadString("Strings\\UI:AGO_CCB_Remixed"), Game1.MineChestType.Remixed));
		this.AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_FarmMonsters"), Game1.content.LoadString("Strings\\UI:AGO_FarmMonsters_Tooltip"), delegate
		{
			bool result = Game1.spawnMonstersAtNight;
			if (Game1.game1.newGameSetupOptions.ContainsKey("SpawnMonstersAtNight"))
			{
				result = Game1.game1.GetNewGameOption<bool>("SpawnMonstersAtNight");
			}
			this.initialMonsterSpawnAtValue = result;
			return result;
		}, delegate(bool val)
		{
			if (this.initialMonsterSpawnAtValue != val)
			{
				Game1.game1.SetNewGameOption("SpawnMonstersAtNight", val);
			}
		});
		this.AddDropdown(Game1.content.LoadString("Strings\\UI:Character_Difficulty"), Game1.content.LoadString("Strings\\UI:AGO_ProfitMargin_Tooltip"), false, () => Game1.player.difficultyModifier, delegate(float val)
		{
			Game1.player.difficultyModifier = val;
		}, new KeyValuePair<string, float>(Game1.content.LoadString("Strings\\UI:Character_Normal"), 1f), new KeyValuePair<string, float>("75%", 0.75f), new KeyValuePair<string, float>("50%", 0.5f), new KeyValuePair<string, float>("25%", 0.25f));
		this.AddHeader(Game1.content.LoadString("Strings\\UI:AGO_MPOptions_Label"));
		KeyValuePair<string, int>[] startingCabinOptions = new KeyValuePair<string, int>[Game1.multiplayer.playerLimit];
		startingCabinOptions[0] = new KeyValuePair<string, int>(Game1.content.LoadString("Strings\\UI:Character_none"), 0);
		for (int i = 1; i < Game1.multiplayer.playerLimit; i++)
		{
			startingCabinOptions[i] = new KeyValuePair<string, int>(i.ToString(), i);
		}
		this.AddDropdown(Game1.content.LoadString("Strings\\UI:Character_StartingCabins"), Game1.content.LoadString("Strings\\UI:AGO_StartingCabins_Tooltip"), labelOnSeparateLine: false, () => Game1.startingCabins, delegate(int val)
		{
			Game1.startingCabins = val;
		}, startingCabinOptions);
		this.AddDropdown(Game1.content.LoadString("Strings\\UI:Character_CabinLayout"), Game1.content.LoadString("Strings\\UI:AGO_CabinLayout_Tooltip"), false, () => Game1.cabinsSeparate, delegate(bool val)
		{
			Game1.cabinsSeparate = val;
		}, new KeyValuePair<string, bool>(Game1.content.LoadString("Strings\\UI:Character_Close"), value: false), new KeyValuePair<string, bool>(Game1.content.LoadString("Strings\\UI:Character_Separate"), value: true));
		this.AddHeader(Game1.content.LoadString("Strings\\UI:AGO_OtherOptions_Label"));
		this.AddTextEntry(Game1.content.LoadString("Strings\\UI:AGO_RandomSeed"), Game1.content.LoadString("Strings\\UI:AGO_RandomSeed_Tooltip"), labelOnSeparateLine: true, () => (!Game1.startingGameSeed.HasValue) ? "" : Game1.startingGameSeed.Value.ToString(), delegate(string val)
		{
			val.Trim();
			if (string.IsNullOrEmpty(val))
			{
				Game1.startingGameSeed = null;
			}
			else
			{
				while (val.Length > 0)
				{
					if (ulong.TryParse(val, out var result))
					{
						Game1.startingGameSeed = result;
						break;
					}
					val = val.Substring(0, val.Length - 1);
				}
			}
		}, delegate(OptionsTextEntry textbox)
		{
			textbox.textBox.numbersOnly = true;
			textbox.textBox.textLimit = 9;
		});
		this.AddCheckbox(Game1.content.LoadString("Strings\\UI:AGO_LegacyRandomization"), Game1.content.LoadString("Strings\\UI:AGO_LegacyRandomization_Tooltip"), () => Game1.UseLegacyRandom, delegate(bool val)
		{
			Game1.UseLegacyRandom = val;
		});
		for (int i2 = this.options.Count; i2 < 7; i2++)
		{
			this.options.Add(new OptionsElement(""));
		}
	}

	public virtual void CloseAndApply()
	{
		foreach (Action applySettingCallback in this.applySettingCallbacks)
		{
			applySettingCallback();
		}
		this.applySettingCallbacks.Clear();
		base.exitThisMenu();
	}

	public virtual void AddHeader(string label)
	{
		this.options.Add(new OptionsElement(label));
	}

	public virtual void AddTextEntry(string label, string tooltip, bool labelOnSeparateLine, Func<string> get, Action<string> set, Action<OptionsTextEntry> configure = null)
	{
		if (labelOnSeparateLine)
		{
			OptionsElement labelElement = new OptionsElement(label)
			{
				style = OptionsElement.Style.OptionLabel
			};
			this.options.Add(labelElement);
			this.tooltips[labelElement] = tooltip;
		}
		OptionsTextEntry option_element = new OptionsTextEntry(labelOnSeparateLine ? string.Empty : label, -999);
		configure?.Invoke(option_element);
		this.tooltips[option_element] = tooltip;
		option_element.textBox.Text = get();
		this.applySettingCallbacks.Add(delegate
		{
			set(option_element.textBox.Text);
		});
		this.options.Add(option_element);
	}

	public virtual void AddDropdown<T>(string label, string tooltip, bool labelOnSeparateLine, Func<T> get, Action<T> set, params KeyValuePair<string, T>[] dropdown_options)
	{
		if (labelOnSeparateLine)
		{
			OptionsElement labelElement = new OptionsElement(label)
			{
				style = OptionsElement.Style.OptionLabel
			};
			this.options.Add(labelElement);
			this.tooltips[labelElement] = tooltip;
		}
		OptionsDropDown option_element = new OptionsDropDown(labelOnSeparateLine ? string.Empty : label, -999);
		this.tooltips[option_element] = tooltip;
		KeyValuePair<string, T>[] array = dropdown_options;
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, T> option = array[i];
			option_element.dropDownDisplayOptions.Add(option.Key);
			option_element.dropDownOptions.Add(option.Value.ToString());
		}
		option_element.RecalculateBounds();
		T selected_value = get();
		int selected_option = 0;
		for (int j = 0; j < dropdown_options.Length; j++)
		{
			KeyValuePair<string, T> dropdown_option = dropdown_options[j];
			if ((dropdown_option.Value == null && selected_value == null) || (dropdown_option.Value != null && selected_value != null && dropdown_option.Value.Equals(selected_value)))
			{
				selected_option = j;
				break;
			}
		}
		option_element.selectedOption = selected_option;
		this.applySettingCallbacks.Add(delegate
		{
			set(dropdown_options[option_element.selectedOption].Value);
		});
		this.options.Add(option_element);
	}

	public virtual void AddCheckbox(string label, string tooltip, Func<bool> get, Action<bool> set)
	{
		OptionsCheckbox option_element = new OptionsCheckbox(label, -999);
		this.tooltips[option_element] = tooltip;
		option_element.isChecked = get();
		this.applySettingCallbacks.Add(delegate
		{
			set(option_element.isChecked);
		});
		this.options.Add(option_element);
	}

	public override bool readyToClose()
	{
		return false;
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		base.currentlySnappedComponent = base.getComponentWithID(this.ID_okButton);
		this.snapCursorToCurrentSnappedComponent();
	}

	public override void applyMovementKey(int direction)
	{
		if (!this.IsDropdownActive())
		{
			base.applyMovementKey(direction);
		}
	}

	private void setScrollBarToCurrentIndex()
	{
		if (this.options.Count > 0)
		{
			this.scrollBar.bounds.Y = this.scrollBarBounds.Y + this.scrollBarBounds.Height / Math.Max(1, this.options.Count - 7) * this.currentItemIndex;
			if (this.currentItemIndex == this.options.Count - 7)
			{
				this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
			}
		}
	}

	public override void snapCursorToCurrentSnappedComponent()
	{
		if (base.currentlySnappedComponent != null && base.currentlySnappedComponent.myID < this.options.Count)
		{
			OptionsElement optionsElement = this.options[base.currentlySnappedComponent.myID + this.currentItemIndex];
			if (!(optionsElement is OptionsDropDown dropdown))
			{
				if (!(optionsElement is OptionsPlusMinusButton))
				{
					if (optionsElement is OptionsInputListener)
					{
						Game1.setMousePosition(base.currentlySnappedComponent.bounds.Right - 48, base.currentlySnappedComponent.bounds.Center.Y - 12);
					}
					else
					{
						Game1.setMousePosition(base.currentlySnappedComponent.bounds.Left + 48, base.currentlySnappedComponent.bounds.Center.Y - 12);
					}
				}
				else
				{
					Game1.setMousePosition(base.currentlySnappedComponent.bounds.Left + 64, base.currentlySnappedComponent.bounds.Center.Y + 4);
				}
			}
			else
			{
				Game1.setMousePosition(base.currentlySnappedComponent.bounds.Left + dropdown.bounds.Right - 32, base.currentlySnappedComponent.bounds.Center.Y - 4);
			}
		}
		else if (base.currentlySnappedComponent != null)
		{
			base.snapCursorToCurrentSnappedComponent();
		}
	}

	public virtual void SetScrollFromY(int y)
	{
		int y2 = this.scrollBar.bounds.Y;
		float percentage = (float)(y - this.scrollBarBounds.Y) / (float)this.scrollBarBounds.Height;
		this.currentItemIndex = (int)Utility.Lerp(t: Utility.Clamp(percentage, 0f, 1f), a: 0f, b: this.options.Count - 7);
		this.setScrollBarToCurrentIndex();
		if (y2 != this.scrollBar.bounds.Y)
		{
			Game1.playSound("shiny4");
		}
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		if (!GameMenu.forcePreventClose)
		{
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				this.SetScrollFromY(y);
			}
			else if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickHeld(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
			}
		}
	}

	public override void setCurrentlySnappedComponentTo(int id)
	{
		base.currentlySnappedComponent = base.getComponentWithID(id);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if ((this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count) || (Game1.options.snappyMenus && Game1.options.gamepadControls))
		{
			if (base.currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls && this.options.Count > this.currentItemIndex + base.currentlySnappedComponent.myID && this.currentItemIndex + base.currentlySnappedComponent.myID >= 0)
			{
				this.options[this.currentItemIndex + base.currentlySnappedComponent.myID].receiveKeyPress(key);
			}
			else if (this.options.Count > this.currentItemIndex + this.optionsSlotHeld && this.currentItemIndex + this.optionsSlotHeld >= 0)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].receiveKeyPress(key);
			}
		}
		base.receiveKeyPress(key);
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		if (!GameMenu.forcePreventClose && !this.IsDropdownActive())
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shiny4");
			}
			else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shiny4");
			}
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		if (!GameMenu.forcePreventClose)
		{
			base.releaseLeftClick(x, y);
			if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickReleased(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
			}
			this.optionsSlotHeld = -1;
			this.scrolling = false;
		}
	}

	public bool IsDropdownActive()
	{
		if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count && this.options[this.currentItemIndex + this.optionsSlotHeld] is OptionsDropDown)
		{
			return true;
		}
		return false;
	}

	private void downArrowPressed()
	{
		if (!this.IsDropdownActive())
		{
			this.downArrow.scale = this.downArrow.baseScale;
			this.currentItemIndex++;
			this.UnsubscribeFromSelectedTextbox();
			this.setScrollBarToCurrentIndex();
		}
	}

	public virtual void UnsubscribeFromSelectedTextbox()
	{
		if (Game1.keyboardDispatcher.Subscriber == null)
		{
			return;
		}
		foreach (OptionsElement option in this.options)
		{
			if (option is OptionsTextEntry entry && Game1.keyboardDispatcher.Subscriber == entry.textBox)
			{
				Game1.keyboardDispatcher.Subscriber = null;
				break;
			}
		}
	}

	public void preWindowSizeChange()
	{
		AdvancedGameOptions._lastSelectedIndex = ((this.getCurrentlySnappedComponent() != null) ? this.getCurrentlySnappedComponent().myID : (-1));
		AdvancedGameOptions._lastCurrentItemIndex = this.currentItemIndex;
	}

	public void postWindowSizeChange()
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.activeClickableMenu.setCurrentlySnappedComponentTo(AdvancedGameOptions._lastSelectedIndex);
		}
		this.currentItemIndex = AdvancedGameOptions._lastCurrentItemIndex;
		this.setScrollBarToCurrentIndex();
	}

	private void upArrowPressed()
	{
		if (!this.IsDropdownActive())
		{
			this.upArrow.scale = this.upArrow.baseScale;
			this.currentItemIndex--;
			this.UnsubscribeFromSelectedTextbox();
			this.setScrollBarToCurrentIndex();
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (GameMenu.forcePreventClose)
		{
			return;
		}
		if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
		{
			this.downArrowPressed();
			Game1.playSound("shwip");
		}
		else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
		{
			this.upArrowPressed();
			Game1.playSound("shwip");
		}
		else if (this.scrollBar.containsPoint(x, y))
		{
			this.scrolling = true;
		}
		else if (!this.downArrow.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
		{
			this.scrolling = true;
			this.leftClickHeld(x, y);
			this.releaseLeftClick(x, y);
		}
		this.currentItemIndex = Math.Max(0, Math.Min(this.options.Count - 7, this.currentItemIndex));
		if (this.okButton.containsPoint(x, y))
		{
			this.CloseAndApply();
			return;
		}
		this.UnsubscribeFromSelectedTextbox();
		for (int i = 0; i < this.optionSlots.Count; i++)
		{
			if (this.optionSlots[i].bounds.Contains(x, y) && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
			{
				this.options[this.currentItemIndex + i].receiveLeftClick(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y);
				this.optionsSlotHeld = i;
				break;
			}
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.okButton.tryHover(x, y);
		for (int i = 0; i < this.optionSlots.Count; i++)
		{
			if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
			{
				Game1.SetFreeCursorDrag();
				break;
			}
		}
		if (this.scrollBarBounds.Contains(x, y))
		{
			Game1.SetFreeCursorDrag();
		}
		if (GameMenu.forcePreventClose)
		{
			return;
		}
		this.hoverText = "";
		int hovered_index = -1;
		if (!this.IsDropdownActive())
		{
			for (int j = 0; j < this.optionSlots.Count; j++)
			{
				if (this.optionSlots[j].containsPoint(x, y) && j + this.currentItemIndex < this.options.Count && this.hoverText == "")
				{
					hovered_index = j + this.currentItemIndex;
				}
			}
		}
		if (this._lastHoveredIndex != hovered_index)
		{
			this._lastHoveredIndex = hovered_index;
			this._hoverDuration = 0;
		}
		else
		{
			this._hoverDuration += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
		}
		if (this._lastHoveredIndex >= 0 && this._hoverDuration >= 500)
		{
			OptionsElement option = this.options[this._lastHoveredIndex];
			if (this.tooltips.TryGetValue(option, out var tooltip))
			{
				this.hoverText = Game1.parseText(tooltip);
			}
		}
		this.upArrow.tryHover(x, y);
		this.downArrow.tryHover(x, y);
		this.scrollBar.tryHover(x, y);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black * 0.75f);
		Game1.DrawBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height);
		this.okButton.draw(b);
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		for (int i = 0; i < this.optionSlots.Count; i++)
		{
			if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count)
			{
				this.options[this.currentItemIndex + i].draw(b, this.optionSlots[i].bounds.X, this.optionSlots[i].bounds.Y, this);
			}
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (this.options.Count > 7)
		{
			this.upArrow.draw(b);
			this.downArrow.draw(b);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarBounds.X, this.scrollBarBounds.Y, this.scrollBarBounds.Width, this.scrollBarBounds.Height, Color.White, 4f, drawShadow: false);
			this.scrollBar.draw(b);
		}
		if (!this.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
		}
		base.drawMouse(b);
	}
}
