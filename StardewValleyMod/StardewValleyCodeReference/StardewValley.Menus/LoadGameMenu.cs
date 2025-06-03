using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.SaveSerialization;

namespace StardewValley.Menus;

public class LoadGameMenu : IClickableMenu, IDisposable
{
	public abstract class MenuSlot : IDisposable
	{
		public int ActivateDelay;

		protected LoadGameMenu menu;

		public MenuSlot(LoadGameMenu menu)
		{
			this.menu = menu;
		}

		public abstract void Activate();

		public abstract void Draw(SpriteBatch b, int i);

		public virtual void Dispose()
		{
		}
	}

	public class SaveFileSlot : MenuSlot
	{
		/// <summary>The farmer to load for this save slot.</summary>
		public Farmer Farmer;

		/// <summary>The slot number to display for this slot, or <c>null</c> to calculate it automatically from its position in the list.</summary>
		public int? SlotNumber;

		public double redTimer;

		public int versionComparison;

		public SaveFileSlot(LoadGameMenu menu, Farmer farmer, int? slotNumber)
			: base(menu)
		{
			base.ActivateDelay = 2150;
			this.Farmer = farmer;
			this.SlotNumber = slotNumber;
			this.versionComparison = Utility.CompareGameVersions(Game1.version, farmer.gameVersion, ignore_platform_specific: true);
		}

		public override void Activate()
		{
			SaveGame.Load(this.Farmer.slotName);
			Game1.exitActiveMenu();
		}

		protected virtual void drawSlotSaveNumber(SpriteBatch b, int i)
		{
			MenuSlot menuSlot = base.menu.MenuSlots[base.menu.currentItemIndex + i];
			ClickableComponent button = base.menu.slotButtons[i];
			string slotNumberLabel = ((menuSlot as SaveFileSlot)?.SlotNumber ?? (base.menu.currentItemIndex + i + 1)) + ".";
			SpriteText.drawString(b, slotNumberLabel, button.bounds.X + 28 + 32 - SpriteText.getWidthOfString(slotNumberLabel) / 2, button.bounds.Y + 36);
		}

		protected virtual string slotName()
		{
			return this.Farmer.Name;
		}

		public virtual float getSlotAlpha()
		{
			return 1f;
		}

		protected virtual void drawSlotName(SpriteBatch b, int i)
		{
			SpriteText.drawString(b, this.slotName(), base.menu.slotButtons[i].bounds.X + 128 + 36, base.menu.slotButtons[i].bounds.Y + 36, 999999, -1, 999999, this.getSlotAlpha());
		}

		protected virtual void drawSlotShadow(SpriteBatch b, int i)
		{
			Vector2 offset = this.portraitOffset();
			b.Draw(Game1.shadowTexture, new Vector2((float)base.menu.slotButtons[i].bounds.X + offset.X + 32f, base.menu.slotButtons[i].bounds.Y + 128 + 16), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.8f);
		}

		protected virtual Vector2 portraitOffset()
		{
			return new Vector2(92f, 20f);
		}

		protected virtual void drawSlotFarmer(SpriteBatch b, int i)
		{
			Vector2 offset = this.portraitOffset();
			FarmerRenderer.isDrawingForUI = true;
			this.Farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0, new Rectangle(0, 0, 16, 32), new Vector2((float)base.menu.slotButtons[i].bounds.X + offset.X, (float)base.menu.slotButtons[i].bounds.Y + offset.Y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, this.Farmer);
			FarmerRenderer.isDrawingForUI = false;
		}

		protected virtual void drawSlotDate(SpriteBatch b, int i)
		{
			string dateStringForSaveGame = ((!this.Farmer.dayOfMonthForSaveGame.HasValue || !this.Farmer.seasonForSaveGame.HasValue || !this.Farmer.yearForSaveGame.HasValue) ? this.Farmer.dateStringForSaveGame : Utility.getDateStringFor(this.Farmer.dayOfMonthForSaveGame.Value, this.Farmer.seasonForSaveGame.Value, this.Farmer.yearForSaveGame.Value));
			Utility.drawTextWithShadow(b, dateStringForSaveGame, Game1.dialogueFont, new Vector2(base.menu.slotButtons[i].bounds.X + 128 + 32, base.menu.slotButtons[i].bounds.Y + 64 + 40), Game1.textColor * this.getSlotAlpha());
		}

		protected virtual string slotSubName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11019", this.Farmer.farmName);
		}

		protected virtual void drawSlotSubName(SpriteBatch b, int i)
		{
			string subName = this.slotSubName();
			Utility.drawTextWithShadow(b, subName, Game1.dialogueFont, new Vector2((float)(base.menu.slotButtons[i].bounds.X + base.menu.width - 128) - Game1.dialogueFont.MeasureString(subName).X, base.menu.slotButtons[i].bounds.Y + 44), Game1.textColor * this.getSlotAlpha());
		}

		protected virtual void drawSlotMoney(SpriteBatch b, int i)
		{
			string cashText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", Utility.getNumberWithCommas(this.Farmer.Money));
			if (this.Farmer.Money == 1 && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
			{
				cashText = cashText.Substring(0, cashText.Length - 1);
			}
			int moneyWidth = (int)Game1.dialogueFont.MeasureString(cashText).X;
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(base.menu.slotButtons[i].bounds.X + base.menu.width - 192 - 100 - moneyWidth, base.menu.slotButtons[i].bounds.Y + 64 + 44), new Rectangle(193, 373, 9, 9), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			Vector2 position = new Vector2(base.menu.slotButtons[i].bounds.X + base.menu.width - 192 - 60 - moneyWidth, base.menu.slotButtons[i].bounds.Y + 64 + 44);
			if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				position.Y += 5f;
			}
			Utility.drawTextWithShadow(b, cashText, Game1.dialogueFont, position, Game1.textColor * this.getSlotAlpha());
		}

		protected virtual void drawSlotTimer(SpriteBatch b, int i)
		{
			Utility.drawWithShadow(position: new Vector2(base.menu.slotButtons[i].bounds.X + base.menu.width - 192 - 44, base.menu.slotButtons[i].bounds.Y + 64 + 36), b: b, texture: Game1.mouseCursors, sourceRect: new Rectangle(595, 1748, 9, 11), color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: 4f, flipped: false, layerDepth: 1f);
			Vector2 position = new Vector2(base.menu.slotButtons[i].bounds.X + base.menu.width - 192 - 4, base.menu.slotButtons[i].bounds.Y + 64 + 44);
			if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				position.Y += 5f;
			}
			Utility.drawTextWithShadow(b, Utility.getHoursMinutesStringFromMilliseconds(this.Farmer.millisecondsPlayed), Game1.dialogueFont, position, Game1.textColor * this.getSlotAlpha());
		}

		public virtual void drawVersionMismatchSlot(SpriteBatch b, int i)
		{
			SpriteText.drawString(b, this.slotName(), base.menu.slotButtons[i].bounds.X + 128, base.menu.slotButtons[i].bounds.Y + 36);
			string farm_name = this.slotSubName();
			Utility.drawTextWithShadow(b, farm_name, Game1.dialogueFont, new Vector2((float)(base.menu.slotButtons[i].bounds.X + base.menu.width - 128) - Game1.dialogueFont.MeasureString(farm_name).X, base.menu.slotButtons[i].bounds.Y + 44), Game1.textColor);
			string game_version = this.Farmer.gameVersion;
			if (game_version == "-1")
			{
				game_version = "<1.4";
			}
			string mismatch_text = Game1.content.LoadString("Strings\\UI:VersionMismatch", game_version);
			Color text_color = Game1.textColor;
			if (Game1.currentGameTime.TotalGameTime.TotalSeconds < this.redTimer && (int)((this.redTimer - Game1.currentGameTime.TotalGameTime.TotalSeconds) / 0.25) % 2 == 1)
			{
				text_color = Color.Red;
			}
			Utility.drawTextWithShadow(b, mismatch_text, Game1.dialogueFont, new Vector2(base.menu.slotButtons[i].bounds.X + 128, base.menu.slotButtons[i].bounds.Y + 64 + 40), text_color);
		}

		public override void Draw(SpriteBatch b, int i)
		{
			this.drawSlotSaveNumber(b, i);
			if (this.versionComparison < 0)
			{
				this.drawVersionMismatchSlot(b, i);
				return;
			}
			this.drawSlotName(b, i);
			this.drawSlotShadow(b, i);
			this.drawSlotFarmer(b, i);
			this.drawSlotDate(b, i);
			this.drawSlotSubName(b, i);
			this.drawSlotMoney(b, i);
			this.drawSlotTimer(b, i);
		}

		public new void Dispose()
		{
			this.Farmer.unload();
		}
	}

	protected const int CenterOffset = 0;

	public const int region_upArrow = 800;

	public const int region_downArrow = 801;

	public const int region_okDelete = 802;

	public const int region_cancelDelete = 803;

	public const int region_slots = 900;

	public const int region_deleteButtons = 901;

	public const int region_navigationButtons = 902;

	public const int region_deleteConfirmations = 903;

	public const int itemsPerPage = 4;

	public List<ClickableComponent> slotButtons = new List<ClickableComponent>();

	public List<ClickableTextureComponent> deleteButtons = new List<ClickableTextureComponent>();

	public int currentItemIndex;

	public int timerToLoad;

	public int selected = -1;

	public int selectedForDelete = -1;

	public ClickableTextureComponent upArrow;

	public ClickableTextureComponent downArrow;

	public ClickableTextureComponent scrollBar;

	public ClickableTextureComponent okDeleteButton;

	public ClickableTextureComponent cancelDeleteButton;

	public ClickableComponent backButton;

	public bool scrolling;

	public bool deleteConfirmationScreen;

	protected List<MenuSlot> menuSlots = new List<MenuSlot>();

	private Rectangle scrollBarRunner;

	protected string hoverText = "";

	public bool loading;

	public bool drawn;

	public bool deleting;

	private int _updatesSinceLastDeleteConfirmScreen;

	private Task<List<Farmer>> _initTask;

	private Task _deleteTask;

	private bool disposedValue;

	public virtual List<MenuSlot> MenuSlots
	{
		get
		{
			return this.menuSlots;
		}
		set
		{
			this.menuSlots = value;
		}
	}

	public bool IsDoingTask()
	{
		if (this._initTask == null && this._deleteTask == null && !this.loading)
		{
			return this.deleting;
		}
		return true;
	}

	public override bool readyToClose()
	{
		if (!this.IsDoingTask())
		{
			return this._updatesSinceLastDeleteConfirmScreen > 1;
		}
		return false;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="filter">A search filter to apply to the displayed list of saves, if any.</param>
	public LoadGameMenu(string filter = null)
		: base(Game1.uiViewport.Width / 2 - (1100 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1100 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2)
	{
		this.backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
		{
			myID = 81114,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		this.upArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
		{
			myID = 800,
			downNeighborID = 801,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 902
		};
		this.downArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
		{
			myID = 801,
			upNeighborID = 800,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = -99998,
			region = 902
		};
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 64 - this.upArrow.bounds.Height - 28);
		this.okDeleteButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10992"), new Rectangle((int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X - 64, (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 802,
			rightNeighborID = 803,
			region = 903
		};
		this.cancelDeleteButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993"), new Rectangle((int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X + 64, (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 803,
			leftNeighborID = 802,
			region = 903
		};
		for (int i = 0; i < 4; i++)
		{
			this.slotButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * (base.height / 4), base.width - 32, base.height / 4 + 4), i.ToString() ?? "")
			{
				myID = i,
				region = 900,
				downNeighborID = ((i < 3) ? (-99998) : (-7777)),
				upNeighborID = ((i > 0) ? (-99998) : (-7777)),
				rightNeighborID = -99998,
				fullyImmutable = true
			});
			if (this.hasDeleteButtons())
			{
				this.deleteButtons.Add(new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width - 64 - 4, base.yPositionOnScreen + 32 + 4 + i * (base.height / 4), 48, 48), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10994"), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3f)
				{
					myID = i + 100,
					region = 901,
					leftNeighborID = -99998,
					downNeighborImmutable = true,
					downNeighborID = -99998,
					upNeighborImmutable = true,
					upNeighborID = ((i > 0) ? (-99998) : (-1)),
					rightNeighborID = -99998
				});
			}
		}
		this.startListPopulation(filter);
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
		this.UpdateButtons();
	}

	protected virtual bool hasDeleteButtons()
	{
		return true;
	}

	/// <summary>Asynchronously populate the list of saves.</summary>
	/// <param name="filter">A search filter to apply to the displayed list of saves, if any.</param>
	protected virtual void startListPopulation(string filter)
	{
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			this.addSaveFiles(LoadGameMenu.FindSaveGames(filter));
			this.saveFileScanComplete();
			return;
		}
		this._initTask = new Task<List<Farmer>>(delegate
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			return LoadGameMenu.FindSaveGames(filter);
		});
		Game1.hooks.StartTask(this._initTask, "Find Save Games");
	}

	public virtual void UpdateButtons()
	{
		for (int i = 0; i < this.slotButtons.Count; i++)
		{
			ClickableTextureComponent delete_button = null;
			if (this.hasDeleteButtons() && i >= 0 && i < this.deleteButtons.Count)
			{
				delete_button = this.deleteButtons[i];
			}
			if (this.currentItemIndex + i < this.MenuSlots.Count)
			{
				this.slotButtons[i].visible = true;
				if (delete_button != null)
				{
					delete_button.visible = true;
				}
			}
			else
			{
				this.slotButtons[i].visible = false;
				if (delete_button != null)
				{
					delete_button.visible = false;
				}
			}
		}
	}

	protected virtual void addSaveFiles(List<Farmer> files)
	{
		int startNumbersAt = this.MenuSlots.Count + 1;
		for (int i = 0; i < files.Count; i++)
		{
			Farmer file = files[i];
			if (file != null)
			{
				this.MenuSlots.Add(new SaveFileSlot(this, file, startNumbersAt + i));
			}
		}
		this.UpdateButtons();
	}

	/// <summary>Get the save games to show in the menu.</summary>
	/// <param name="filter">A search filter to apply to the displayed list of saves, if any.</param>
	/// <returns>Returns a list of farmers for which to add save slots. If a <paramref name="filter" /> is given, filtered-out slots will be null in the list.</returns>
	private static List<Farmer> FindSaveGames(string filter)
	{
		List<Farmer> results = new List<Farmer>();
		string pathToDirectory = Program.GetSavesFolder();
		if (Directory.Exists(pathToDirectory))
		{
			foreach (string s in Directory.EnumerateDirectories(pathToDirectory).ToList())
			{
				string saveName = s.Split(Path.DirectorySeparatorChar).Last();
				string pathToFile = Path.Combine(pathToDirectory, s, "SaveGameInfo");
				string pathToSave = Path.Combine(pathToDirectory, s, saveName);
				if (!File.Exists(pathToSave) && !File.Exists(pathToSave + "_old") && !File.Exists(pathToSave + "_STARDEWVALLEYSAVETMP"))
				{
					continue;
				}
				Farmer f = null;
				try
				{
					Exception error;
					object obj = TryReadSaveInfo(null, out error);
					if (obj == null)
					{
						obj = TryReadSaveInfo("_old", out var loadError) ?? TryReadSaveInfo("_STARDEWVALLEYSAVETMP", out loadError);
						if (obj == null)
						{
							obj = TryReadSaveData(null, out loadError) ?? TryReadSaveData("_old", out loadError) ?? TryReadSaveData("_STARDEWVALLEYSAVETMP", out loadError);
						}
					}
					f = (Farmer)obj;
					if (f == null)
					{
						Game1.log.Error("Exception occurred trying to access file '" + pathToFile + "'", error);
						continue;
					}
					SaveGame.loadDataToFarmer(f);
					f.slotName = saveName;
					results.Add(f);
				}
				catch (Exception exception)
				{
					Game1.log.Error("Exception occurred trying to access file '" + pathToFile + "'", exception);
					f?.unload();
				}
				Farmer TryReadSaveData(string suffix, out Exception loadError2)
				{
					return TryReadFile<SaveGame>(pathToSave + suffix, out loadError2, SaveSerializer.Deserialize<SaveGame>)?.player;
				}
				Farmer TryReadSaveInfo(string suffix, out Exception loadError2)
				{
					return TryReadFile<Farmer>(pathToFile + suffix, out loadError2, SaveSerializer.Deserialize<Farmer>);
				}
			}
		}
		results.Sort();
		if (!string.IsNullOrWhiteSpace(filter))
		{
			for (int i = 0; i < results.Count; i++)
			{
				Farmer farmer = results[i];
				string name = farmer.Name;
				if (name != null && name.IndexOfIgnoreCase(filter) == -1)
				{
					string value = farmer.farmName.Value;
					if (value != null && value.IndexOfIgnoreCase(filter) == -1)
					{
						results[i] = null;
					}
				}
			}
		}
		return results;
		static TData TryReadFile<TData>(string path, out Exception reference, Func<Stream, TData> load)
		{
			try
			{
				using FileStream stream = File.OpenRead(path);
				reference = null;
				return load(stream);
			}
			catch (Exception ex)
			{
				reference = ex;
				return default(TData);
			}
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		if (button == Buttons.B && this.deleteConfirmationScreen)
		{
			this.deleteConfirmationScreen = false;
			this.selectedForDelete = -1;
			Game1.playSound("smallSelect");
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				base.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.deleteConfirmationScreen)
		{
			base.currentlySnappedComponent = base.getComponentWithID(803);
		}
		else
		{
			base.currentlySnappedComponent = base.getComponentWithID(0);
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		switch (direction)
		{
		case 2:
			if (this.currentItemIndex < Math.Max(0, this.MenuSlots.Count - 4))
			{
				this.downArrowPressed();
				base.currentlySnappedComponent = base.getComponentWithID(3);
				this.snapCursorToCurrentSnappedComponent();
			}
			break;
		case 0:
			if (this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				base.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
			break;
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.xPositionOnScreen = (newBounds.Width - base.width) / 2;
		base.yPositionOnScreen = (newBounds.Height - (base.height + 32)) / 2;
		this.backButton.bounds.X = Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2;
		this.backButton.bounds.Y = Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom;
		this.upArrow.bounds.X = base.xPositionOnScreen + base.width + 16;
		this.upArrow.bounds.Y = base.yPositionOnScreen + 16;
		this.downArrow.bounds.X = base.xPositionOnScreen + base.width + 16;
		this.downArrow.bounds.Y = base.yPositionOnScreen + base.height - 64;
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 64 - this.upArrow.bounds.Height - 28);
		this.okDeleteButton.bounds.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X - 64;
		this.okDeleteButton.bounds.Y = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128;
		this.cancelDeleteButton.bounds.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X + 64;
		this.cancelDeleteButton.bounds.Y = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128;
		for (int i = 0; i < this.slotButtons.Count; i++)
		{
			this.slotButtons[i].bounds.X = base.xPositionOnScreen + 16;
			this.slotButtons[i].bounds.Y = base.yPositionOnScreen + 16 + i * (base.height / 4);
		}
		for (int j = 0; j < this.deleteButtons.Count; j++)
		{
			this.deleteButtons[j].bounds.X = base.xPositionOnScreen + base.width - 64 - 4;
			this.deleteButtons[j].bounds.Y = base.yPositionOnScreen + 32 + 4 + j * (base.height / 4);
		}
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			int id = ((base.currentlySnappedComponent != null) ? base.currentlySnappedComponent.myID : 81114);
			this.populateClickableComponentList();
			base.currentlySnappedComponent = base.getComponentWithID(id);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		base.performHoverAction(x, y);
		if (this.deleteConfirmationScreen)
		{
			this.okDeleteButton.tryHover(x, y);
			this.cancelDeleteButton.tryHover(x, y);
			if (this.okDeleteButton.containsPoint(x, y))
			{
				this.hoverText = "";
			}
			else if (this.cancelDeleteButton.containsPoint(x, y))
			{
				this.hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993");
			}
			return;
		}
		this.upArrow.tryHover(x, y);
		this.downArrow.tryHover(x, y);
		this.scrollBar.tryHover(x, y);
		foreach (ClickableTextureComponent deleteButton in this.deleteButtons)
		{
			deleteButton.tryHover(x, y, 0.2f);
			if (deleteButton.containsPoint(x, y))
			{
				this.hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10994");
				return;
			}
		}
		if (this.scrolling)
		{
			return;
		}
		for (int i = 0; i < this.slotButtons.Count; i++)
		{
			if (this.currentItemIndex + i < this.MenuSlots.Count && this.slotButtons[i].containsPoint(x, y))
			{
				if (this.slotButtons[i].scale == 1f)
				{
					Game1.playSound("Cowboy_gunshot");
				}
				this.slotButtons[i].scale = Math.Min(this.slotButtons[i].scale + 0.03f, 1.1f);
			}
			else
			{
				this.slotButtons[i].scale = Math.Max(1f, this.slotButtons[i].scale - 0.03f);
			}
		}
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		base.leftClickHeld(x, y);
		if (this.scrolling)
		{
			int y2 = this.scrollBar.bounds.Y;
			this.scrollBar.bounds.Y = Math.Min(base.yPositionOnScreen + base.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, base.yPositionOnScreen + this.upArrow.bounds.Height + 20));
			float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
			this.currentItemIndex = Math.Min(this.MenuSlots.Count - 4, Math.Max(0, (int)((float)this.MenuSlots.Count * percentage)));
			this.setScrollBarToCurrentIndex();
			if (y2 != this.scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
			}
		}
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		this.scrolling = false;
	}

	protected void setScrollBarToCurrentIndex()
	{
		if (this.MenuSlots.Count > 0)
		{
			this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.MenuSlots.Count - 4 + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + 4;
			if (this.currentItemIndex == this.MenuSlots.Count - 4)
			{
				this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
			}
		}
		this.UpdateButtons();
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		if (direction > 0 && this.currentItemIndex > 0)
		{
			this.upArrowPressed();
		}
		else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.MenuSlots.Count - 4))
		{
			this.downArrowPressed();
		}
	}

	private void downArrowPressed()
	{
		this.downArrow.scale = this.downArrow.baseScale;
		this.currentItemIndex++;
		Game1.playSound("shwip");
		this.setScrollBarToCurrentIndex();
	}

	private void upArrowPressed()
	{
		this.upArrow.scale = this.upArrow.baseScale;
		this.currentItemIndex--;
		Game1.playSound("shwip");
		this.setScrollBarToCurrentIndex();
	}

	private void deleteFile(int which)
	{
		if (!(this.MenuSlots[which] is SaveFileSlot slot))
		{
			return;
		}
		string filenameNoTmpString = slot.Farmer.slotName;
		string saveFolderPath = Path.Combine(Program.GetSavesFolder(), filenameNoTmpString);
		if (Directory.Exists(saveFolderPath))
		{
			Directory.Delete(saveFolderPath, recursive: true);
		}
		for (int i = 0; i < 50; i++)
		{
			if (!Directory.Exists(saveFolderPath))
			{
				break;
			}
			Thread.Sleep(100);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.timerToLoad > 0 || this.loading || this.deleting)
		{
			return;
		}
		if (this.deleteConfirmationScreen)
		{
			if (this.cancelDeleteButton.containsPoint(x, y))
			{
				this.deleteConfirmationScreen = false;
				this.selectedForDelete = -1;
				Game1.playSound("smallSelect");
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					base.currentlySnappedComponent = base.getComponentWithID(0);
					this.snapCursorToCurrentSnappedComponent();
				}
			}
			else
			{
				if (!this.okDeleteButton.containsPoint(x, y))
				{
					return;
				}
				this.deleting = true;
				if (LocalMultiplayer.IsLocalMultiplayer())
				{
					this.deleteFile(this.selectedForDelete);
					this.deleting = false;
				}
				else
				{
					this._deleteTask = new Task(delegate
					{
						Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
						this.deleteFile(this.selectedForDelete);
					});
					Game1.hooks.StartTask(this._deleteTask, "Farm_Delete");
				}
				this.deleteConfirmationScreen = false;
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					base.currentlySnappedComponent = base.getComponentWithID(0);
					this.snapCursorToCurrentSnappedComponent();
				}
				Game1.playSound("trashcan");
			}
			return;
		}
		base.receiveLeftClick(x, y, playSound);
		if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.MenuSlots.Count - 4))
		{
			this.downArrowPressed();
		}
		else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
		{
			this.upArrowPressed();
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
		if (this.selected == -1)
		{
			for (int i = 0; i < this.deleteButtons.Count; i++)
			{
				if (this.deleteButtons[i].containsPoint(x, y) && i < this.MenuSlots.Count && !this.deleteConfirmationScreen)
				{
					this.deleteConfirmationScreen = true;
					Game1.playSound("drumkit6");
					this.selectedForDelete = this.currentItemIndex + i;
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						base.currentlySnappedComponent = base.getComponentWithID(803);
						this.snapCursorToCurrentSnappedComponent();
					}
					return;
				}
			}
		}
		if (!this.deleteConfirmationScreen)
		{
			for (int i2 = 0; i2 < this.slotButtons.Count; i2++)
			{
				if (!this.slotButtons[i2].containsPoint(x, y) || i2 >= this.MenuSlots.Count)
				{
					continue;
				}
				if (this.MenuSlots[this.currentItemIndex + i2] is SaveFileSlot { versionComparison: <0 } menu_save_slot)
				{
					menu_save_slot.redTimer = Game1.currentGameTime.TotalGameTime.TotalSeconds + 1.0;
					Game1.playSound("cancel");
					continue;
				}
				Game1.playSound("select");
				this.timerToLoad = this.MenuSlots[this.currentItemIndex + i2].ActivateDelay;
				if (this.timerToLoad > 0)
				{
					this.loading = true;
					this.selected = this.currentItemIndex + i2;
				}
				else
				{
					this.MenuSlots[this.currentItemIndex + i2].Activate();
				}
				return;
			}
		}
		this.currentItemIndex = Math.Max(0, Math.Min(this.MenuSlots.Count - 4, this.currentItemIndex));
	}

	protected virtual void saveFileScanComplete()
	{
		Game1.game1.ResetGameStateOnTitleScreen();
	}

	protected virtual bool checkListPopulation()
	{
		if (!this.deleteConfirmationScreen)
		{
			this._updatesSinceLastDeleteConfirmScreen++;
		}
		else
		{
			this._updatesSinceLastDeleteConfirmScreen = 0;
		}
		if (this._initTask != null)
		{
			if (this._initTask.IsCanceled || this._initTask.IsCompleted || this._initTask.IsFaulted)
			{
				if (this._initTask.IsCompleted)
				{
					this.addSaveFiles(this._initTask.Result);
					this.saveFileScanComplete();
				}
				this._initTask = null;
			}
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.checkListPopulation())
		{
			return;
		}
		if (this._deleteTask != null)
		{
			if (this._deleteTask.IsCanceled || this._deleteTask.IsCompleted || this._deleteTask.IsFaulted)
			{
				if (!this._deleteTask.IsCompleted)
				{
					this.selectedForDelete = -1;
				}
				this._deleteTask = null;
				this.deleting = false;
			}
			return;
		}
		if (this.selectedForDelete != -1 && !this.deleteConfirmationScreen && !this.deleting && this.MenuSlots[this.selectedForDelete] is SaveFileSlot slot)
		{
			slot.Farmer.unload();
			this.MenuSlots.RemoveAt(this.selectedForDelete);
			this.selectedForDelete = -1;
			this.slotButtons.Clear();
			this.deleteButtons.Clear();
			for (int i = 0; i < 4; i++)
			{
				this.slotButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * (base.height / 4), base.width - 32, base.height / 4 + 4), i.ToString() ?? ""));
				if (this.hasDeleteButtons())
				{
					this.deleteButtons.Add(new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width - 64 - 4, base.yPositionOnScreen + 32 + 4 + i * (base.height / 4), 48, 48), "", "Delete File", Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3f));
				}
			}
			if (this.MenuSlots.Count <= 4)
			{
				this.currentItemIndex = 0;
				this.setScrollBarToCurrentIndex();
			}
		}
		if (this.timerToLoad <= 0)
		{
			return;
		}
		this.timerToLoad -= time.ElapsedGameTime.Milliseconds;
		if (this.timerToLoad <= 0)
		{
			if (this.MenuSlots.Count > this.selected)
			{
				this.MenuSlots[this.selected].Activate();
			}
			else
			{
				Game1.ExitToTitle();
			}
		}
	}

	protected virtual string getStatusText()
	{
		if (this._initTask != null)
		{
			return Game1.content.LoadString("Strings\\UI:LoadGameMenu_LookingForSavedGames");
		}
		if (this.deleting)
		{
			return Game1.content.LoadString("Strings\\UI:LoadGameMenu_Deleting");
		}
		if (this.MenuSlots.Count == 0)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11022");
		}
		return null;
	}

	protected virtual void drawExtra(SpriteBatch b)
	{
	}

	protected virtual void drawSlotBackground(SpriteBatch b, int i, MenuSlot slot)
	{
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.slotButtons[i].bounds.X, this.slotButtons[i].bounds.Y, this.slotButtons[i].bounds.Width, this.slotButtons[i].bounds.Height, ((this.currentItemIndex + i != this.selected || this.timerToLoad % 150 <= 75 || this.timerToLoad <= 1000) && (this.selected != -1 || !(this.slotButtons[i].scale > 1f) || this.scrolling || this.deleteConfirmationScreen)) ? Color.White : ((this.deleteButtons.Count > i && this.deleteButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) ? Color.White : Color.Wheat), 4f, drawShadow: false);
	}

	protected virtual void drawBefore(SpriteBatch b)
	{
	}

	protected virtual void drawStatusText(SpriteBatch b)
	{
		string text = this.getStatusText();
		if (text != null)
		{
			SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		this.drawBefore(b);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height + 32, Color.White, 4f);
		if (this.selectedForDelete == -1 || !this.deleting || this.deleteConfirmationScreen)
		{
			for (int i = 0; i < this.slotButtons.Count; i++)
			{
				if (this.currentItemIndex + i < this.MenuSlots.Count)
				{
					this.drawSlotBackground(b, i, this.MenuSlots[this.currentItemIndex + i]);
					this.MenuSlots[this.currentItemIndex + i].Draw(b, i);
					if (this.deleteButtons.Count > i)
					{
						this.deleteButtons[i].draw(b, Color.White * 0.75f, 1f);
					}
				}
			}
		}
		this.drawStatusText(b);
		this.upArrow.draw(b);
		this.downArrow.draw(b);
		if (this.MenuSlots.Count > 4)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
			this.scrollBar.draw(b);
		}
		if (this.deleteConfirmationScreen && this.MenuSlots[this.selectedForDelete] is SaveFileSlot slot)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.75f);
			string toDisplay = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11023", slot.Farmer.Name);
			int middlePosX = this.okDeleteButton.bounds.X + (this.cancelDeleteButton.bounds.X - this.okDeleteButton.bounds.X) / 2 + this.okDeleteButton.bounds.Width / 2;
			SpriteText.drawString(b, toDisplay, middlePosX - SpriteText.getWidthOfString(toDisplay) / 2, (int)Utility.getTopLeftPositionForCenteringOnScreen(192, 64).Y, 9999, -1, 9999, 1f, 1f, junimoText: false, -1, "", SpriteText.color_White);
			this.okDeleteButton.draw(b);
			this.cancelDeleteButton.draw(b);
		}
		base.draw(b);
		if (this.hoverText.Length > 0)
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont);
		}
		this.drawExtra(b);
		if (this.selected != -1 && this.timerToLoad < 1000)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * (1f - (float)this.timerToLoad / 1000f));
		}
		if (Game1.activeClickableMenu == this && (!Game1.options.SnappyMenus || base.currentlySnappedComponent != null) && !this.IsDoingTask())
		{
			base.drawMouse(b, ignore_transparency: false, this.loading ? 1 : (-1));
		}
		this.drawn = true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (this.disposedValue)
		{
			return;
		}
		if (disposing)
		{
			if (this.MenuSlots != null)
			{
				foreach (MenuSlot menuSlot in this.MenuSlots)
				{
					menuSlot.Dispose();
				}
				this.MenuSlots.Clear();
				this.MenuSlots = null;
			}
			this._initTask = null;
			this._deleteTask = null;
		}
		this.disposedValue = true;
	}

	~LoadGameMenu()
	{
		this.Dispose(disposing: false);
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (a.region == 901 && b.region != 901 && direction == 2 && b.myID != 81114)
		{
			return true;
		}
		if (a.region == 901 && direction == 3 && b.region != 900)
		{
			return false;
		}
		if (direction == 1 && a.region == 900 && this.hasDeleteButtons() && b.region != 901)
		{
			return false;
		}
		if (a.region != 903 && b.region == 903)
		{
			return false;
		}
		if ((direction == 0 || direction == 2) && a.myID == 81114 && b.region == 902)
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
	{
		return false;
	}

	[Conditional("LOG_FS_IO")]
	private static void LogFsio(string format, params object[] args)
	{
		Game1.log.Verbose(string.Format(format, args));
	}
}
