using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;

namespace StardewValley.Menus;

public class BuildingPaintMenu : IClickableMenu
{
	/// <summary>The data model for a paint region.</summary>
	public class RegionData
	{
		/// <summary>The unique region ID within the building's paint regions.</summary>
		public string Id { get; }

		/// <summary>The localized display name.</summary>
		public string DisplayName { get; }

		/// <summary>The minimum brightness allowed.</summary>
		public int MinBrightness { get; }

		/// <summary>The maximum brightness allowed.</summary>
		public int MaxBrightness { get; }

		/// <summary>Construct an instance.</summary>
		/// <param name="id">The unique region ID within the building's paint regions.</param>
		/// <param name="displayName">The localized display name.</param>
		/// <param name="minBrightness">The minimum brightness allowed.</param>
		/// <param name="maxBrightness">The maximum brightness allowed.</param>
		public RegionData(string id, string displayName, int minBrightness, int maxBrightness)
		{
			this.Id = id;
			this.DisplayName = displayName;
			this.MinBrightness = minBrightness;
			this.MaxBrightness = maxBrightness;
		}
	}

	public class ColorSliderPanel
	{
		public BuildingPaintMenu buildingPaintMenu;

		public int regionIndex;

		public string regionId = "Paint Region Name";

		public Rectangle rectangle;

		public Vector2 colorDrawPosition;

		public List<KeyValuePair<string, List<int>>> colors = new List<KeyValuePair<string, List<int>>>();

		public int selectedColor;

		public BuildingColorSlider hueSlider;

		public BuildingColorSlider saturationSlider;

		public BuildingColorSlider lightnessSlider;

		public int minimumBrightness = -100;

		public int maximumBrightness = 100;

		public ColorSliderPanel(BuildingPaintMenu menu, int region_index, string regionId, int min_brightness = -100, int max_brightness = 100)
		{
			this.regionIndex = region_index;
			this.buildingPaintMenu = menu;
			this.regionId = regionId;
			this.minimumBrightness = min_brightness;
			this.maximumBrightness = max_brightness;
		}

		public virtual int GetHeight()
		{
			return this.rectangle.Height;
		}

		public virtual Rectangle Reposition(Rectangle start_rect)
		{
			this.buildingPaintMenu.sliderHandles.Clear();
			this.rectangle.X = start_rect.X;
			this.rectangle.Y = start_rect.Y;
			this.rectangle.Width = start_rect.Width;
			this.rectangle.Height = 0;
			this.lightnessSlider = null;
			this.hueSlider = null;
			this.saturationSlider = null;
			this.colorDrawPosition = new Vector2(start_rect.X + start_rect.Width - 64, start_rect.Y);
			this.hueSlider = new BuildingColorSlider(this.buildingPaintMenu, 106, new Rectangle(this.rectangle.Left, this.rectangle.Bottom, this.rectangle.Width - 100, 12), 0, 360, delegate
			{
				switch (this.regionIndex)
				{
				case 0:
					this.buildingPaintMenu.colorTarget.Color1Default.Value = false;
					break;
				case 1:
					this.buildingPaintMenu.colorTarget.Color2Default.Value = false;
					break;
				default:
					this.buildingPaintMenu.colorTarget.Color3Default.Value = false;
					break;
				}
				this.ApplyColors();
			});
			BuildingColorSlider buildingColorSlider = this.hueSlider;
			buildingColorSlider.getDrawColor = (Func<float, Color>)Delegate.Combine(buildingColorSlider.getDrawColor, (Func<float, Color>)((float val) => this.GetColorForValues(val, 100f)));
			switch (this.regionIndex)
			{
			case 0:
				this.hueSlider.SetValue(this.buildingPaintMenu.colorTarget.Color1Hue.Value, skip_value_set: true);
				break;
			case 1:
				this.hueSlider.SetValue(this.buildingPaintMenu.colorTarget.Color2Hue.Value, skip_value_set: true);
				break;
			default:
				this.hueSlider.SetValue(this.buildingPaintMenu.colorTarget.Color3Hue.Value, skip_value_set: true);
				break;
			}
			this.rectangle.Height += 24;
			this.saturationSlider = new BuildingColorSlider(this.buildingPaintMenu, 107, new Rectangle(this.rectangle.Left, this.rectangle.Bottom, this.rectangle.Width - 100, 12), 0, 75, delegate
			{
				switch (this.regionIndex)
				{
				case 0:
					this.buildingPaintMenu.colorTarget.Color1Default.Value = false;
					break;
				case 1:
					this.buildingPaintMenu.colorTarget.Color2Default.Value = false;
					break;
				default:
					this.buildingPaintMenu.colorTarget.Color3Default.Value = false;
					break;
				}
				this.ApplyColors();
			});
			BuildingColorSlider buildingColorSlider2 = this.saturationSlider;
			buildingColorSlider2.getDrawColor = (Func<float, Color>)Delegate.Combine(buildingColorSlider2.getDrawColor, (Func<float, Color>)((float val) => this.GetColorForValues(this.hueSlider.GetValue(), val)));
			switch (this.regionIndex)
			{
			case 0:
				this.saturationSlider.SetValue(this.buildingPaintMenu.colorTarget.Color1Saturation.Value, skip_value_set: true);
				break;
			case 1:
				this.saturationSlider.SetValue(this.buildingPaintMenu.colorTarget.Color2Saturation.Value, skip_value_set: true);
				break;
			default:
				this.saturationSlider.SetValue(this.buildingPaintMenu.colorTarget.Color3Saturation.Value, skip_value_set: true);
				break;
			}
			this.rectangle.Height += 24;
			this.lightnessSlider = new BuildingColorSlider(this.buildingPaintMenu, 108, new Rectangle(this.rectangle.Left, this.rectangle.Bottom, this.rectangle.Width - 100, 12), this.minimumBrightness, this.maximumBrightness, delegate
			{
				switch (this.regionIndex)
				{
				case 0:
					this.buildingPaintMenu.colorTarget.Color1Default.Value = false;
					break;
				case 1:
					this.buildingPaintMenu.colorTarget.Color2Default.Value = false;
					break;
				default:
					this.buildingPaintMenu.colorTarget.Color3Default.Value = false;
					break;
				}
				this.ApplyColors();
			});
			BuildingColorSlider buildingColorSlider3 = this.lightnessSlider;
			buildingColorSlider3.getDrawColor = (Func<float, Color>)Delegate.Combine(buildingColorSlider3.getDrawColor, (Func<float, Color>)((float val) => this.GetColorForValues(this.hueSlider.GetValue(), this.saturationSlider.GetValue(), val)));
			switch (this.regionIndex)
			{
			case 0:
				this.lightnessSlider.SetValue(this.buildingPaintMenu.colorTarget.Color1Lightness.Value, skip_value_set: true);
				break;
			case 1:
				this.lightnessSlider.SetValue(this.buildingPaintMenu.colorTarget.Color2Lightness.Value, skip_value_set: true);
				break;
			default:
				this.lightnessSlider.SetValue(this.buildingPaintMenu.colorTarget.Color3Lightness.Value, skip_value_set: true);
				break;
			}
			this.rectangle.Height += 24;
			if ((this.regionIndex == 0 && this.buildingPaintMenu.colorTarget.Color1Default.Value) || (this.regionIndex == 1 && this.buildingPaintMenu.colorTarget.Color2Default.Value) || (this.regionIndex == 2 && this.buildingPaintMenu.colorTarget.Color3Default.Value))
			{
				this.hueSlider.SetValue(this.hueSlider.min, skip_value_set: true);
				this.saturationSlider.SetValue(this.saturationSlider.max, skip_value_set: true);
				this.lightnessSlider.SetValue((this.lightnessSlider.min + this.lightnessSlider.max) / 2, skip_value_set: true);
			}
			this.buildingPaintMenu.sliderHandles.Add(this.hueSlider.handle);
			this.buildingPaintMenu.sliderHandles.Add(this.saturationSlider.handle);
			this.buildingPaintMenu.sliderHandles.Add(this.lightnessSlider.handle);
			this.hueSlider.handle.upNeighborID = 104;
			this.hueSlider.handle.downNeighborID = 107;
			this.saturationSlider.handle.downNeighborID = 108;
			this.saturationSlider.handle.upNeighborID = 106;
			this.lightnessSlider.handle.upNeighborID = 107;
			this.rectangle.Height += 32;
			start_rect.Y += this.rectangle.Height;
			return start_rect;
		}

		public virtual void ApplyColors()
		{
			switch (this.regionIndex)
			{
			case 0:
				this.buildingPaintMenu.colorTarget.Color1Hue.Value = this.hueSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color1Saturation.Value = this.saturationSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color1Lightness.Value = this.lightnessSlider.GetValue();
				break;
			case 1:
				this.buildingPaintMenu.colorTarget.Color2Hue.Value = this.hueSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color2Saturation.Value = this.saturationSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color2Lightness.Value = this.lightnessSlider.GetValue();
				break;
			default:
				this.buildingPaintMenu.colorTarget.Color3Hue.Value = this.hueSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color3Saturation.Value = this.saturationSlider.GetValue();
				this.buildingPaintMenu.colorTarget.Color3Lightness.Value = this.lightnessSlider.GetValue();
				break;
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			if ((this.regionIndex != 0 || !this.buildingPaintMenu.colorTarget.Color1Default.Value) && (this.regionIndex != 1 || !this.buildingPaintMenu.colorTarget.Color2Default.Value) && (this.regionIndex != 2 || !this.buildingPaintMenu.colorTarget.Color3Default.Value))
			{
				Color drawn_color = this.GetColorForValues(this.hueSlider.GetValue(), this.saturationSlider.GetValue(), this.lightnessSlider.GetValue());
				b.Draw(Game1.staminaRect, new Rectangle((int)this.colorDrawPosition.X - 4, (int)this.colorDrawPosition.Y - 4, 72, 72), null, Game1.textColor, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				b.Draw(Game1.staminaRect, new Rectangle((int)this.colorDrawPosition.X, (int)this.colorDrawPosition.Y, 64, 64), null, drawn_color, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			}
			this.hueSlider?.Draw(b);
			this.saturationSlider?.Draw(b);
			this.lightnessSlider?.Draw(b);
		}

		public Color GetColorForValues(float hue_slider, float saturation_slider)
		{
			Utility.HSLtoRGB(hue_slider, saturation_slider / 100f, 0.5, out var red, out var green, out var blue);
			return new Color((byte)red, green, blue);
		}

		public Color GetColorForValues(float hue_slider, float saturation_slider, float lightness_slider)
		{
			Utility.HSLtoRGB(hue_slider, saturation_slider / 100f, Utility.Lerp(0.25f, 0.5f, (lightness_slider - (float)this.lightnessSlider.min) / (float)(this.lightnessSlider.max - this.lightnessSlider.min)), out var red, out var green, out var blue);
			return new Color((byte)red, green, blue);
		}

		public virtual bool ApplyMovementKey(int direction)
		{
			if (direction == 3 || direction == 1)
			{
				if (this.saturationSlider.handle == this.buildingPaintMenu.currentlySnappedComponent)
				{
					this.saturationSlider.ApplyMovementKey(direction);
					return true;
				}
				if (this.hueSlider.handle == this.buildingPaintMenu.currentlySnappedComponent)
				{
					this.hueSlider.ApplyMovementKey(direction);
					return true;
				}
				if (this.lightnessSlider.handle == this.buildingPaintMenu.currentlySnappedComponent)
				{
					this.lightnessSlider.ApplyMovementKey(direction);
					return true;
				}
			}
			return false;
		}

		public virtual void PerformHoverAction(int x, int y)
		{
		}

		public virtual bool ReceiveLeftClick(int x, int y, bool play_sound = true)
		{
			this.hueSlider?.ReceiveLeftClick(x, y);
			this.saturationSlider?.ReceiveLeftClick(x, y);
			this.lightnessSlider?.ReceiveLeftClick(x, y);
			return false;
		}
	}

	public class BuildingColorSlider
	{
		public ClickableTextureComponent handle;

		public BuildingPaintMenu buildingPaintMenu;

		public Rectangle bounds;

		protected float _sliderPosition;

		public int min;

		public int max;

		public Action<int> onValueSet;

		public Func<float, Color> getDrawColor;

		protected int _displayedValue;

		public BuildingColorSlider(BuildingPaintMenu bpm, int handle_id, Rectangle bounds, int min, int max, Action<int> on_value_set = null)
		{
			this.handle = new ClickableTextureComponent(new Rectangle(0, 0, 4, 5), Game1.mouseCursors, new Rectangle(72, 256, 16, 20), 1f);
			this.handle.myID = handle_id;
			this.handle.upNeighborID = -99998;
			this.handle.upNeighborImmutable = true;
			this.handle.downNeighborID = -99998;
			this.handle.downNeighborImmutable = true;
			this.handle.leftNeighborImmutable = true;
			this.handle.rightNeighborImmutable = true;
			this.buildingPaintMenu = bpm;
			this.bounds = bounds;
			this.min = min;
			this.max = max;
			this.onValueSet = on_value_set;
		}

		public virtual void ApplyMovementKey(int direction)
		{
			int amount = Math.Max((this.max - this.min) / 50, 1);
			if (direction == 3)
			{
				this.SetValue(this._displayedValue - amount);
			}
			else
			{
				this.SetValue(this._displayedValue + amount);
			}
			if (this.buildingPaintMenu.currentlySnappedComponent == this.handle && Game1.options.SnappyMenus)
			{
				this.buildingPaintMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		public virtual void ReceiveLeftClick(int x, int y)
		{
			if (this.bounds.Contains(x, y))
			{
				this.buildingPaintMenu.activeSlider = this;
				this.SetValueFromPosition(x, y);
			}
		}

		public virtual void SetValueFromPosition(int x, int y)
		{
			if (this.bounds.Width != 0 && this.min != this.max)
			{
				float new_value = x - this.bounds.Left;
				new_value /= (float)this.bounds.Width;
				if (new_value < 0f)
				{
					new_value = 0f;
				}
				if (new_value > 1f)
				{
					new_value = 1f;
				}
				int steps = this.max - this.min;
				new_value /= (float)steps;
				new_value *= (float)steps;
				if (this._sliderPosition != new_value)
				{
					this._sliderPosition = new_value;
					this.SetValue(this.min + (int)(this._sliderPosition * (float)steps));
				}
			}
		}

		public void SetValue(int value, bool skip_value_set = false)
		{
			if (value > this.max)
			{
				value = this.max;
			}
			if (value < this.min)
			{
				value = this.min;
			}
			this._sliderPosition = (float)(value - this.min) / (float)(this.max - this.min);
			this.handle.bounds.X = (int)Utility.Lerp(this.bounds.Left, this.bounds.Right, this._sliderPosition) - this.handle.bounds.Width / 2 * 4;
			this.handle.bounds.Y = this.bounds.Top - 4;
			if (this._displayedValue != value)
			{
				this._displayedValue = value;
				if (!skip_value_set)
				{
					this.onValueSet?.Invoke(value);
				}
			}
		}

		public int GetValue()
		{
			return this._displayedValue;
		}

		public virtual void Draw(SpriteBatch b)
		{
			int divisions = 20;
			for (int i = 0; i < divisions; i++)
			{
				Rectangle section_bounds = new Rectangle((int)((float)this.bounds.X + (float)this.bounds.Width / (float)divisions * (float)i), this.bounds.Y, (int)Math.Ceiling((float)this.bounds.Width / (float)divisions), this.bounds.Height);
				Color drawn_color = Color.Black;
				if (this.getDrawColor != null)
				{
					drawn_color = this.getDrawColor(Utility.Lerp(this.min, this.max, (float)i / (float)divisions));
				}
				b.Draw(Game1.staminaRect, section_bounds, drawn_color);
			}
			this.handle.draw(b);
		}

		public virtual void Update(int x, int y)
		{
			this.SetValueFromPosition(x, y);
		}
	}

	public const int region_colorButtons = 1000;

	public const int region_okButton = 101;

	public const int region_nextRegion = 102;

	public const int region_prevRegion = 103;

	public const int region_copyColor = 104;

	public const int region_defaultColor = 105;

	public const int region_hueSlider = 106;

	public const int region_saturationSlider = 107;

	public const int region_lightnessSlider = 108;

	public const int region_appearanceButton = 109;

	public static int WINDOW_WIDTH = 1024;

	public static int WINDOW_HEIGHT = 576;

	public Rectangle previewPane;

	public Rectangle colorPane;

	public BuildingColorSlider activeSlider;

	public ClickableTextureComponent appearanceButton;

	public ClickableTextureComponent okButton;

	public static List<Vector3> savedColors = null;

	public List<Color> buttonColors = new List<Color>();

	public ColorSliderPanel colorSliderPanel;

	private string hoverText = "";

	public Building building;

	public string buildingType = "";

	public BuildingPaintColor colorTarget;

	protected Dictionary<string, string> _paintData;

	public int currentPaintRegion;

	/// <summary>The paint regions for the building.</summary>
	public List<RegionData> regions;

	public ClickableTextureComponent nextRegionButton;

	public ClickableTextureComponent previousRegionButton;

	public ClickableTextureComponent copyColorButton;

	public ClickableTextureComponent defaultColorButton;

	public List<ClickableTextureComponent> savedColorButtons = new List<ClickableTextureComponent>();

	public List<ClickableComponent> sliderHandles = new List<ClickableComponent>();

	public BuildingPaintMenu(Building target_building)
		: base(Game1.uiViewport.Width / 2 - BuildingPaintMenu.WINDOW_WIDTH / 2, Game1.uiViewport.Height / 2 - BuildingPaintMenu.WINDOW_HEIGHT / 2, BuildingPaintMenu.WINDOW_WIDTH, BuildingPaintMenu.WINDOW_HEIGHT)
	{
		this.InitializeSavedColors();
		this._paintData = DataLoader.PaintData(Game1.content);
		Game1.player.Halt();
		this.building = target_building;
		this.colorTarget = target_building.netBuildingPaintColor.Value;
		this.buildingType = this.building.buildingType.Value;
		this.SetRegion(0);
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public virtual void InitializeSavedColors()
	{
		if (BuildingPaintMenu.savedColors == null)
		{
			BuildingPaintMenu.savedColors = new List<Vector3>();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(101);
		this.snapCursorToCurrentSnappedComponent();
	}

	public override void applyMovementKey(int direction)
	{
		if (!this.colorSliderPanel.ApplyMovementKey(direction))
		{
			base.applyMovementKey(direction);
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		switch (button)
		{
		case Buttons.RightTrigger:
			Game1.playSound("shwip");
			this.SetRegion((this.currentPaintRegion + 1 + this.regions.Count) % this.regions.Count);
			break;
		case Buttons.LeftTrigger:
			Game1.playSound("shwip");
			this.SetRegion((this.currentPaintRegion - 1 + this.regions.Count) % this.regions.Count);
			break;
		}
		base.receiveGamePadButton(button);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		this.activeSlider?.Update(Game1.getMouseX(), Game1.getMouseY());
		base.update(time);
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		this.activeSlider = null;
		base.releaseLeftClick(x, y);
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		for (int i = 0; i < this.savedColorButtons.Count; i++)
		{
			if (this.savedColorButtons[i].containsPoint(x, y))
			{
				BuildingPaintMenu.savedColors.RemoveAt(i);
				this.RepositionElements();
				Game1.playSound("coin");
				return;
			}
		}
		base.receiveRightClick(x, y, playSound);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.colorSliderPanel.ReceiveLeftClick(x, y, playSound))
		{
			return;
		}
		if (this.defaultColorButton.containsPoint(x, y))
		{
			switch (this.currentPaintRegion)
			{
			case 0:
				this.colorTarget.Color1Default.Value = true;
				break;
			case 1:
				this.colorTarget.Color2Default.Value = true;
				break;
			default:
				this.colorTarget.Color3Default.Value = true;
				break;
			}
			Game1.playSound("coin");
			this.RepositionElements();
			return;
		}
		for (int i = 0; i < this.savedColorButtons.Count; i++)
		{
			if (this.savedColorButtons[i].containsPoint(x, y))
			{
				this.colorSliderPanel.hueSlider.SetValue((int)BuildingPaintMenu.savedColors[i].X);
				this.colorSliderPanel.saturationSlider.SetValue((int)BuildingPaintMenu.savedColors[i].Y);
				this.colorSliderPanel.lightnessSlider.SetValue((int)Utility.Lerp(this.colorSliderPanel.lightnessSlider.min, this.colorSliderPanel.lightnessSlider.max, BuildingPaintMenu.savedColors[i].Z));
				Game1.playSound("coin");
				return;
			}
		}
		if (this.copyColorButton.containsPoint(x, y))
		{
			if (this.SaveColor())
			{
				Game1.playSound("coin");
				this.RepositionElements();
			}
			else
			{
				Game1.playSound("cancel");
			}
		}
		else if (this.okButton.containsPoint(x, y))
		{
			base.exitThisMenu(playSound);
		}
		else if (this.appearanceButton.containsPoint(x, y))
		{
			Game1.playSound("smallSelect");
			BuildingSkinMenu skinMenu = new BuildingSkinMenu(this.building);
			skinMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(skinMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
			{
				if (this.building.CanBePainted())
				{
					BuildingPaintMenu buildingPaintMenu = new BuildingPaintMenu(this.building);
					IClickableMenu clickableMenu = Game1.activeClickableMenu;
					IClickableMenu clickableMenu2 = null;
					while (clickableMenu.GetChildMenu() != null)
					{
						clickableMenu2 = clickableMenu;
						clickableMenu = clickableMenu.GetChildMenu();
						if (clickableMenu is BuildingPaintMenu)
						{
							break;
						}
					}
					if (clickableMenu2 == null)
					{
						Game1.activeClickableMenu = buildingPaintMenu;
					}
					else
					{
						clickableMenu2.SetChildMenu(buildingPaintMenu);
					}
					if (Game1.options.SnappyMenus)
					{
						buildingPaintMenu.setCurrentlySnappedComponentTo(109);
						buildingPaintMenu.snapCursorToCurrentSnappedComponent();
					}
				}
				else
				{
					base.exitThisMenuNoSound();
				}
			});
			base.SetChildMenu(skinMenu);
		}
		else if (this.previousRegionButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this.SetRegion((this.currentPaintRegion - 1 + this.regions.Count) % this.regions.Count);
		}
		else if (this.nextRegionButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this.SetRegion((this.currentPaintRegion + 1) % this.regions.Count);
		}
		else
		{
			base.receiveLeftClick(x, y, playSound);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		this.okButton.tryHover(x, y);
		this.previousRegionButton.tryHover(x, y);
		this.nextRegionButton.tryHover(x, y);
		this.copyColorButton.tryHover(x, y);
		this.defaultColorButton.tryHover(x, y);
		this.appearanceButton.tryHover(x, y);
		if (this.appearanceButton.containsPoint(x, y))
		{
			this.hoverText = this.appearanceButton.name;
		}
		foreach (ClickableTextureComponent savedColorButton in this.savedColorButtons)
		{
			savedColorButton.tryHover(x, y);
		}
		this.colorSliderPanel.PerformHoverAction(x, y);
	}

	public virtual void RepositionElements()
	{
		this.previewPane.X = base.xPositionOnScreen;
		this.previewPane.Y = base.yPositionOnScreen;
		this.previewPane.Width = 512;
		this.previewPane.Height = 576;
		this.colorPane.Width = 448;
		this.colorPane.X = base.xPositionOnScreen + base.width - this.colorPane.Width;
		this.colorPane.Y = base.yPositionOnScreen;
		this.colorPane.Height = 576;
		Rectangle panel_rectangle = this.colorPane;
		panel_rectangle.Inflate(-32, -32);
		this.previousRegionButton = new ClickableTextureComponent(new Rectangle(panel_rectangle.Left, panel_rectangle.Top, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
		{
			myID = 103,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 105,
			upNeighborID = -99998,
			fullyImmutable = true
		};
		this.nextRegionButton = new ClickableTextureComponent(new Rectangle(panel_rectangle.Right - 64, panel_rectangle.Top, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
		{
			myID = 102,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 105,
			upNeighborID = -99998,
			fullyImmutable = true
		};
		panel_rectangle.Y += 64;
		panel_rectangle.Height = 0;
		int color_x = panel_rectangle.Left;
		this.defaultColorButton = new ClickableTextureComponent(new Rectangle(color_x, panel_rectangle.Bottom, 64, 64), Game1.mouseCursors2, new Rectangle(80, 144, 16, 16), 4f)
		{
			region = 1000,
			myID = 105,
			upNeighborID = -99998,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			fullyImmutable = true
		};
		color_x += 80;
		this.savedColorButtons.Clear();
		this.buttonColors.Clear();
		for (int i = 0; i < BuildingPaintMenu.savedColors.Count; i++)
		{
			if (color_x + 64 > panel_rectangle.X + panel_rectangle.Width)
			{
				color_x = panel_rectangle.X;
				panel_rectangle.Y += 72;
			}
			ClickableTextureComponent color_button = new ClickableTextureComponent(new Rectangle(color_x, panel_rectangle.Bottom, 64, 64), Game1.mouseCursors2, new Rectangle(96, 144, 16, 16), 4f)
			{
				region = 1000,
				myID = i,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				fullyImmutable = true
			};
			color_x += 80;
			this.savedColorButtons.Add(color_button);
			Vector3 saved_color = BuildingPaintMenu.savedColors[i];
			Utility.HSLtoRGB(saved_color.X, saved_color.Y / 100f, Utility.Lerp(0.25f, 0.5f, saved_color.Z), out var r, out var g, out var b);
			this.buttonColors.Add(new Color((byte)r, (byte)g, (byte)b));
		}
		if (color_x + 64 > panel_rectangle.X + panel_rectangle.Width)
		{
			color_x = panel_rectangle.X;
			panel_rectangle.Y += 72;
		}
		this.copyColorButton = new ClickableTextureComponent(new Rectangle(color_x, panel_rectangle.Bottom, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f)
		{
			region = 1000,
			myID = 104,
			upNeighborID = -99998,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			fullyImmutable = true
		};
		panel_rectangle.Y += 80;
		panel_rectangle = this.colorSliderPanel.Reposition(panel_rectangle);
		panel_rectangle.Y += 64;
		this.okButton = new ClickableTextureComponent(new Rectangle(this.colorPane.Right - 64 - 16, this.colorPane.Bottom - 64 - 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101,
			upNeighborID = 108,
			leftNeighborID = 109
		};
		this.appearanceButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_ChangeAppearance"), new Rectangle(this.previewPane.Right - 64 - 16, this.colorPane.Bottom - 64 - 16, 64, 64), null, null, Game1.mouseCursors2, new Rectangle(96, 208, 16, 16), 4f)
		{
			myID = 109,
			upNeighborID = 108,
			rightNeighborID = 101,
			visible = this.building.CanBeReskinned()
		};
		this.populateClickableComponentList();
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (a.region == 1000 && b.region != 1000)
		{
			switch (direction)
			{
			case 1:
			case 3:
				return false;
			case 2:
				if (b.myID != 106)
				{
					return false;
				}
				break;
			}
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public virtual bool SaveColor()
	{
		if ((this.currentPaintRegion == 0 && this.colorTarget.Color1Default.Value) || (this.currentPaintRegion == 1 && this.colorTarget.Color2Default.Value) || (this.currentPaintRegion == 2 && this.colorTarget.Color3Default.Value))
		{
			return false;
		}
		Vector3 saved_color = new Vector3(this.colorSliderPanel.hueSlider.GetValue(), this.colorSliderPanel.saturationSlider.GetValue(), (float)(this.colorSliderPanel.lightnessSlider.GetValue() - this.colorSliderPanel.lightnessSlider.min) / (float)(this.colorSliderPanel.lightnessSlider.max - this.colorSliderPanel.lightnessSlider.min));
		if (BuildingPaintMenu.savedColors.Count >= 8)
		{
			BuildingPaintMenu.savedColors.RemoveAt(0);
		}
		BuildingPaintMenu.savedColors.Add(saved_color);
		return true;
	}

	public virtual void SetRegion(int new_region)
	{
		if (this.regions == null)
		{
			this.LoadRegionData();
		}
		if (new_region < this.regions.Count && new_region >= 0)
		{
			this.currentPaintRegion = new_region;
			RegionData region = this.regions[new_region];
			this.colorSliderPanel = new ColorSliderPanel(this, new_region, region.Id, region.MinBrightness, region.MaxBrightness);
		}
		this.RepositionElements();
	}

	public virtual void LoadRegionData()
	{
		if (this.regions != null)
		{
			return;
		}
		this.regions = new List<RegionData>();
		string lookupName = this.building.GetPaintDataKey(this._paintData);
		string rawData;
		string data = ((lookupName != null && this._paintData.TryGetValue(lookupName, out rawData)) ? rawData.Replace("\n", "").Replace("\t", "") : null);
		if (data == null)
		{
			return;
		}
		string[] data_split = data.Split('/');
		for (int i = 0; i < data_split.Length / 2; i++)
		{
			if (data_split[i].Trim() == "")
			{
				continue;
			}
			string regionId = data_split[i * 2];
			string[] brightness_split = ArgUtility.SplitBySpace(data_split[i * 2 + 1]);
			int min_brightness = -100;
			int max_brightness = 100;
			if (brightness_split.Length >= 2)
			{
				try
				{
					min_brightness = int.Parse(brightness_split[0]);
					max_brightness = int.Parse(brightness_split[1]);
				}
				catch (Exception)
				{
				}
			}
			string region_name = Game1.content.LoadStringReturnNullIfNotFound("Strings/Buildings:Paint_Region_" + regionId) ?? regionId;
			this.regions.Add(new RegionData(regionId, region_name, min_brightness, max_brightness));
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		Game1.DrawBox(this.previewPane.X, this.previewPane.Y, this.previewPane.Width, this.previewPane.Height);
		Rectangle rectangle = this.previewPane;
		rectangle.Inflate(0, 0);
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = rectangle;
		Vector2 building_draw_center = new Vector2(this.previewPane.X + this.previewPane.Width / 2, this.previewPane.Y + this.previewPane.Height / 2 - 16);
		Rectangle sourceRect = this.building.getSourceRectForMenu() ?? this.building.getSourceRect();
		this.building.drawInMenu(b, (int)building_draw_center.X - (int)((float)this.building.tilesWide.Value / 2f * 64f), (int)building_draw_center.Y - sourceRect.Height * 4 / 2);
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		Game1.DrawBox(this.colorPane.X, this.colorPane.Y, this.colorPane.Width, this.colorPane.Height);
		RegionData region = this.regions[this.currentPaintRegion];
		int text_height = SpriteText.getHeightOfString(region.DisplayName);
		SpriteText.drawStringHorizontallyCenteredAt(b, region.DisplayName, this.colorPane.X + this.colorPane.Width / 2, this.nextRegionButton.bounds.Center.Y - text_height / 2);
		this.okButton.draw(b);
		this.appearanceButton.draw(b);
		this.colorSliderPanel.Draw(b);
		this.nextRegionButton.draw(b);
		this.previousRegionButton.draw(b);
		this.copyColorButton.draw(b);
		this.defaultColorButton.draw(b);
		for (int i = 0; i < this.savedColorButtons.Count; i++)
		{
			this.savedColorButtons[i].draw(b, this.buttonColors[i], 1f);
		}
		if (base.GetChildMenu() == null)
		{
			base.drawMouse(b);
			string text = this.hoverText;
			if (text != null && text.Length > 0)
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont);
			}
		}
	}
}
