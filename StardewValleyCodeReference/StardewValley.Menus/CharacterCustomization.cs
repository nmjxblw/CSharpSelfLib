using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shirts;
using StardewValley.Minigames;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class CharacterCustomization : IClickableMenu
{
	public enum Source
	{
		NewGame,
		NewFarmhand,
		Wizard,
		HostNewFarm,
		Dresser,
		ClothesDye,
		DyePots
	}

	public const int region_okbutton = 505;

	public const int region_skipIntroButton = 506;

	public const int region_randomButton = 507;

	public const int region_male = 508;

	public const int region_female = 509;

	public const int region_dog = 510;

	public const int region_cat = 511;

	public const int region_shirtLeft = 512;

	public const int region_shirtRight = 513;

	public const int region_hairLeft = 514;

	public const int region_hairRight = 515;

	public const int region_accLeft = 516;

	public const int region_accRight = 517;

	public const int region_skinLeft = 518;

	public const int region_skinRight = 519;

	public const int region_directionLeft = 520;

	public const int region_directionRight = 521;

	public const int region_cabinsLeft = 621;

	public const int region_cabinsRight = 622;

	public const int region_cabinsClose = 623;

	public const int region_cabinsSeparate = 624;

	public const int region_coopHelp = 625;

	public const int region_coopHelpOK = 626;

	public const int region_difficultyLeft = 627;

	public const int region_difficultyRight = 628;

	public const int region_petLeft = 627;

	public const int region_petRight = 628;

	public const int region_pantsLeft = 629;

	public const int region_pantsRight = 630;

	public const int region_walletsLeft = 631;

	public const int region_walletsRight = 632;

	public const int region_coopHelpRight = 633;

	public const int region_coopHelpLeft = 634;

	public const int region_coopHelpButtons = 635;

	public const int region_advancedOptions = 636;

	public const int region_colorPicker1 = 522;

	public const int region_colorPicker2 = 523;

	public const int region_colorPicker3 = 524;

	public const int region_colorPicker4 = 525;

	public const int region_colorPicker5 = 526;

	public const int region_colorPicker6 = 527;

	public const int region_colorPicker7 = 528;

	public const int region_colorPicker8 = 529;

	public const int region_colorPicker9 = 530;

	public const int region_farmSelection1 = 531;

	public const int region_farmSelection2 = 532;

	public const int region_farmSelection3 = 533;

	public const int region_farmSelection4 = 534;

	public const int region_farmSelection5 = 535;

	public const int region_farmSelection6 = 545;

	public const int region_farmSelection7 = 546;

	public const int region_farmSelection8 = 547;

	public const int region_farmSelection9 = 548;

	public const int region_farmSelection10 = 549;

	public const int region_farmSelection11 = 550;

	public const int region_farmSelection12 = 551;

	public const int region_farmSelectionLeft = 647;

	public const int region_farmSelectionRight = 648;

	public const int region_nameBox = 536;

	public const int region_farmNameBox = 537;

	public const int region_favThingBox = 538;

	public const int colorPickerTimerDelay = 100;

	public const int widthOfMultiplayerArea = 256;

	private int colorPickerTimer;

	public ColorPicker pantsColorPicker;

	public ColorPicker hairColorPicker;

	public ColorPicker eyeColorPicker;

	public List<ClickableComponent> labels = new List<ClickableComponent>();

	public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();

	public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

	public List<ClickableComponent> genderButtons = new List<ClickableComponent>();

	public List<ClickableTextureComponent> farmTypeButtons = new List<ClickableTextureComponent>();

	public ClickableTextureComponent farmTypeNextPageButton;

	public ClickableTextureComponent farmTypePreviousPageButton;

	private List<string> farmTypeButtonNames = new List<string>();

	private List<string> farmTypeHoverText = new List<string>();

	private List<KeyValuePair<Texture2D, Rectangle>> farmTypeIcons = new List<KeyValuePair<Texture2D, Rectangle>>();

	protected int _currentFarmPage;

	protected int _farmPages;

	public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

	public List<ClickableTextureComponent> cabinLayoutButtons = new List<ClickableTextureComponent>();

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent skipIntroButton;

	public ClickableTextureComponent randomButton;

	public ClickableTextureComponent coopHelpButton;

	public ClickableTextureComponent coopHelpOkButton;

	public ClickableTextureComponent coopHelpRightButton;

	public ClickableTextureComponent coopHelpLeftButton;

	public ClickableTextureComponent advancedOptionsButton;

	private TextBox nameBox;

	private TextBox farmnameBox;

	private TextBox favThingBox;

	private bool skipIntro;

	public bool isModifyingExistingPet;

	public bool showingCoopHelp;

	public int coopHelpScreen;

	public Source source;

	private Vector2 helpStringSize;

	private string hoverText;

	private string hoverTitle;

	private string coopHelpString;

	private string noneString;

	private string normalDiffString;

	private string toughDiffString;

	private string hardDiffString;

	private string superDiffString;

	private string sharedWalletString;

	private string separateWalletString;

	public ClickableComponent nameBoxCC;

	public ClickableComponent farmnameBoxCC;

	public ClickableComponent favThingBoxCC;

	public ClickableComponent backButton;

	private ClickableComponent nameLabel;

	private ClickableComponent farmLabel;

	private ClickableComponent favoriteLabel;

	private ClickableComponent shirtLabel;

	private ClickableComponent skinLabel;

	private ClickableComponent hairLabel;

	private ClickableComponent accLabel;

	private ClickableComponent pantsStyleLabel;

	private ClickableComponent startingCabinsLabel;

	private ClickableComponent cabinLayoutLabel;

	private ClickableComponent separateWalletLabel;

	private ClickableComponent difficultyModifierLabel;

	private ColorPicker _sliderOpTarget;

	private Action _sliderAction;

	private readonly Action _recolorEyesAction;

	private readonly Action _recolorPantsAction;

	private readonly Action _recolorHairAction;

	protected Clothing _itemToDye;

	protected bool _shouldShowBackButton = true;

	protected bool _isDyeMenu;

	protected Farmer _displayFarmer;

	public Rectangle portraitBox;

	public Rectangle? petPortraitBox;

	public string oldName = "";

	private bool _multiplayerServer;

	private float advancedCCHighlightTimer;

	protected List<KeyValuePair<string, string>> _petTypesAndBreeds;

	private ColorPicker lastHeldColorPicker;

	private int timesRandom;

	public CharacterCustomization(Clothing item)
		: this(Source.ClothesDye)
	{
		this._itemToDye = item;
		this.ResetComponents();
		if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
		{
			Game1.spawnMonstersAtNight = false;
		}
		this._recolorPantsAction = delegate
		{
			this.DyeItem(this.pantsColorPicker.getSelectedColor());
		};
		switch (this._itemToDye.clothesType.Value)
		{
		case Clothing.ClothesType.SHIRT:
			this._displayFarmer.Equip(this._itemToDye, this._displayFarmer.shirtItem);
			break;
		case Clothing.ClothesType.PANTS:
			this._displayFarmer.Equip(this._itemToDye, this._displayFarmer.pantsItem);
			break;
		}
		this._displayFarmer.UpdateClothing();
	}

	public void DyeItem(Color color)
	{
		if (this._itemToDye != null)
		{
			this._itemToDye.Dye(color, 1f);
			this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
		}
	}

	public CharacterCustomization(Source source, bool multiplayerServer = false)
		: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 648 + IClickableMenu.borderWidth * 2 + 64)
	{
		if (source == Source.NewGame || source == Source.HostNewFarm)
		{
			Game1.player.difficultyModifier = 1f;
			Game1.player.team.useSeparateWallets.Value = false;
			Game1.startingCabins = ((source == Source.HostNewFarm) ? 1 : 0);
		}
		this.LoadFarmTypeData();
		this.oldName = Game1.player.Name;
		this._multiplayerServer = multiplayerServer;
		int items_to_dye = 0;
		if (source == Source.ClothesDye || source == Source.DyePots)
		{
			this._isDyeMenu = true;
			switch (source)
			{
			case Source.ClothesDye:
				items_to_dye = 1;
				break;
			case Source.DyePots:
				if (Game1.player.CanDyePants())
				{
					items_to_dye++;
				}
				if (Game1.player.CanDyeShirt())
				{
					items_to_dye++;
				}
				break;
			}
			base.height = 308 + IClickableMenu.borderWidth * 2 + 64 + 72 * items_to_dye - 4;
			base.xPositionOnScreen = Game1.uiViewport.Width / 2 - base.width / 2;
			base.yPositionOnScreen = Game1.uiViewport.Height / 2 - base.height / 2 - 64;
		}
		this.source = source;
		this.ResetComponents();
		this._recolorEyesAction = delegate
		{
			Game1.player.changeEyeColor(this.eyeColorPicker.getSelectedColor());
		};
		this._recolorPantsAction = delegate
		{
			Game1.player.changePantsColor(this.pantsColorPicker.getSelectedColor());
		};
		this._recolorHairAction = delegate
		{
			Game1.player.changeHairColor(this.hairColorPicker.getSelectedColor());
		};
		if (source == Source.DyePots)
		{
			this._recolorHairAction = delegate
			{
				if (Game1.player.CanDyeShirt())
				{
					Game1.player.shirtItem.Value.clothesColor.Value = this.hairColorPicker.getSelectedColor();
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
			};
			this._recolorPantsAction = delegate
			{
				if (Game1.player.CanDyePants())
				{
					Game1.player.pantsItem.Value.clothesColor.Value = this.pantsColorPicker.getSelectedColor();
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
			};
			this.favThingBoxCC.visible = false;
			this.nameBoxCC.visible = false;
			this.farmnameBoxCC.visible = false;
			this.favoriteLabel.visible = false;
			this.nameLabel.visible = false;
			this.farmLabel.visible = false;
		}
		this._displayFarmer = this.GetOrCreateDisplayFarmer();
	}

	public Farmer GetOrCreateDisplayFarmer()
	{
		if (this._displayFarmer == null)
		{
			if (this.source == Source.ClothesDye || this.source == Source.DyePots)
			{
				this._displayFarmer = Game1.player.CreateFakeEventFarmer();
			}
			else
			{
				this._displayFarmer = Game1.player;
			}
			if (this.source == Source.NewFarmhand)
			{
				if (this._displayFarmer.pants.Value == null)
				{
					this._displayFarmer.pants.Value = this._displayFarmer.GetPantsId();
				}
				if (this._displayFarmer.shirt.Value == null)
				{
					this._displayFarmer.shirt.Value = this._displayFarmer.GetShirtId();
				}
			}
			this._displayFarmer.faceDirection(2);
			this._displayFarmer.FarmerSprite.StopAnimation();
		}
		return this._displayFarmer;
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		this.ResetComponents();
	}

	public void showAdvancedCharacterCreationHighlight()
	{
		this.advancedCCHighlightTimer = 4000f;
	}

	private void ResetComponents()
	{
		if (this._isDyeMenu)
		{
			base.xPositionOnScreen = Game1.uiViewport.Width / 2 - base.width / 2;
			base.yPositionOnScreen = Game1.uiViewport.Height / 2 - base.height / 2 - 64;
		}
		else
		{
			base.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			base.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
		}
		this.colorPickerCCs.Clear();
		if (this.source == Source.ClothesDye && this._itemToDye == null)
		{
			return;
		}
		bool creatingNewSave = this.source == Source.NewGame || this.source == Source.HostNewFarm;
		bool allow_clothing_changes = this.source != Source.Wizard && this.source != Source.ClothesDye && this.source != Source.DyePots;
		bool allow_accessory_changes = this.source != Source.ClothesDye && this.source != Source.DyePots;
		this.labels.Clear();
		this.genderButtons.Clear();
		this.cabinLayoutButtons.Clear();
		this.leftSelectionButtons.Clear();
		this.rightSelectionButtons.Clear();
		this.farmTypeButtons.Clear();
		if (creatingNewSave)
		{
			this.advancedOptionsButton = new ClickableTextureComponent("Advanced", new Rectangle(base.xPositionOnScreen - 80, base.yPositionOnScreen + base.height - 80 - 16, 80, 80), null, null, Game1.mouseCursors2, new Rectangle(154, 154, 20, 20), 4f)
			{
				myID = 636,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
		else
		{
			this.advancedOptionsButton = null;
		}
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 505,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		this.backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
		{
			myID = 81114,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		this.nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
		{
			X = base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
			Y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16,
			Text = Game1.player.Name
		};
		this.nameBoxCC = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 192, 48), "")
		{
			myID = 536,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		int textBoxLabelsXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-4) : 0);
		this.labels.Add(this.nameLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Name")));
		this.farmnameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
		{
			X = base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
			Y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64,
			Text = Game1.MasterPlayer.farmName.Value
		};
		this.farmnameBoxCC = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 192, 48), "")
		{
			myID = 537,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		int farmLabelXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? (-16) : 0);
		this.labels.Add(this.farmLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + textBoxLabelsXOffset * 3 + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4 + farmLabelXOffset, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Farm")));
		int favThingBoxXoffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 48 : 0);
		this.favThingBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
		{
			X = base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256 + favThingBoxXoffset,
			Y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128,
			Text = Game1.player.favoriteThing.Value
		};
		this.favThingBoxCC = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 192, 48), "")
		{
			myID = 538,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		this.labels.Add(this.favoriteLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 1, 1), Game1.content.LoadString("Strings\\UI:Character_FavoriteThing")));
		this.randomButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + 64 + 56, 40, 40), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f)
		{
			myID = 507,
			upNeighborID = -99998,
			leftNeighborImmutable = true,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		if (this.source == Source.DyePots || this.source == Source.ClothesDye)
		{
			this.randomButton.visible = false;
		}
		this.portraitBox = new Rectangle(base.xPositionOnScreen + 64 + 42 - 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);
		if (this._isDyeMenu)
		{
			this.portraitBox.X = base.xPositionOnScreen + (base.width - this.portraitBox.Width) / 2;
			this.randomButton.bounds.X = this.portraitBox.X - 56;
		}
		int yOffset = 128;
		this.leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.X - 32, this.portraitBox.Y + 144, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
		{
			myID = 520,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			leftNeighborImmutable = true,
			rightNeighborID = -99998,
			downNeighborID = -99998
		});
		this.rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.Right - 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
		{
			myID = 521,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		});
		int leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-20) : 0);
		this.isModifyingExistingPet = false;
		if (creatingNewSave)
		{
			this.petPortraitBox = new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 448 - 16 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 60 : 0), base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 - 16, 64, 64);
			this.labels.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8 + textBoxLabelsXOffset, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8 + 192, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Animal")));
		}
		if (creatingNewSave || this.source == Source.NewFarmhand || this.source == Source.Wizard)
		{
			this.genderButtons.Add(new ClickableTextureComponent("Male", new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), 4f)
			{
				myID = 508,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.genderButtons.Add(new ClickableTextureComponent("Female", new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 64 + 24, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f)
			{
				myID = 509,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			if (this.source == Source.Wizard)
			{
				List<ClickableComponent> list = this.genderButtons;
				if (list != null && list.Count > 0)
				{
					int start_x = base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 320 + 16;
					int start_y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64 + 48;
					for (int i = 0; i < this.genderButtons.Count; i++)
					{
						this.genderButtons[i].bounds.X = start_x + 80 * i;
						this.genderButtons[i].bounds.Y = start_y;
					}
				}
			}
			yOffset = 256;
			if (this.source == Source.Wizard)
			{
				yOffset = 192;
			}
			leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? (-20) : 0);
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + leftSelectionXOffset, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 518,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.skinLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + 64 + 8 + leftSelectionXOffset / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Skin")));
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 519,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		if (creatingNewSave)
		{
			this.RefreshFarmTypeButtons();
		}
		if (this.source == Source.HostNewFarm)
		{
			this.labels.Add(this.startingCabinsLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen - 21 - 128, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 84, 1, 1), Game1.content.LoadString("Strings\\UI:Character_StartingCabins")));
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 621,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 622,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.cabinLayoutLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen - 128 - (int)(Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_CabinLayout")).X / 2f), base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 120 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_CabinLayout")));
			this.cabinLayoutButtons.Add(new ClickableTextureComponent("Close", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Close"), Game1.mouseCursors, new Rectangle(208, 192, 16, 16), 4f)
			{
				myID = 623,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.cabinLayoutButtons.Add(new ClickableTextureComponent("Separate", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Separate"), Game1.mouseCursors, new Rectangle(224, 192, 16, 16), 4f)
			{
				myID = 624,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.difficultyModifierLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen - 21 - 128, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 56, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Difficulty")));
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 627,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 628,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int walletY = base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 320 + 100;
			this.labels.Add(this.separateWalletLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen - 21 - 128, walletY - 24, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Wallets")));
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 631,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 632,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.coopHelpButton = new ClickableTextureComponent("CoopHelp", new Rectangle(base.xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 448 + 40, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_CoopHelp"), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 4f)
			{
				myID = 625,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.coopHelpOkButton = new ClickableTextureComponent("CoopHelpOK", new Rectangle(base.xPositionOnScreen - 256 - 12, base.yPositionOnScreen + base.height - 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 626,
				region = 635,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.noneString = Game1.content.LoadString("Strings\\UI:Character_none");
			this.normalDiffString = Game1.content.LoadString("Strings\\UI:Character_Normal");
			this.toughDiffString = Game1.content.LoadString("Strings\\UI:Character_Tough");
			this.hardDiffString = Game1.content.LoadString("Strings\\UI:Character_Hard");
			this.superDiffString = Game1.content.LoadString("Strings\\UI:Character_Super");
			this.separateWalletString = Game1.content.LoadString("Strings\\UI:Character_SeparateWallet");
			this.sharedWalletString = Game1.content.LoadString("Strings\\UI:Character_SharedWallet");
			this.coopHelpRightButton = new ClickableTextureComponent("CoopHelpRight", new Rectangle(base.xPositionOnScreen + base.width, base.yPositionOnScreen + base.height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 633,
				region = 635,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.coopHelpLeftButton = new ClickableTextureComponent("CoopHelpLeft", new Rectangle(base.xPositionOnScreen, base.yPositionOnScreen + base.height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 634,
				region = 635,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
		Point top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
		int label_position = base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;
		if (this._isDyeMenu)
		{
			label_position = base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
		}
		if (creatingNewSave || this.source == Source.NewFarmhand || this.source == Source.Wizard)
		{
			this.labels.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));
			this.eyeColorPicker = new ColorPicker("Eyes", top.X, top.Y);
			this.eyeColorPicker.setColor(Game1.player.newEyeColor.Value);
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 522,
				downNeighborID = -99998,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 523,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 524,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			yOffset += 68;
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + leftSelectionXOffset, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 514,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.hairLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair")));
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 515,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
		if (creatingNewSave || this.source == Source.NewFarmhand || this.source == Source.Wizard)
		{
			this.labels.Add(new ClickableComponent(new Rectangle(label_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_HairColor")));
			this.hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
			this.hairColorPicker.setColor(Game1.player.hairstyleColor.Value);
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 525,
				downNeighborID = -99998,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 526,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 527,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
		}
		if (this.source == Source.DyePots)
		{
			yOffset += 68;
			if (Game1.player.CanDyeShirt())
			{
				top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				top.X = base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
				this.labels.Add(new ClickableComponent(new Rectangle(label_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_ShirtColor")));
				this.hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
				this.hairColorPicker.setColor(Game1.player.GetShirtColor());
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 525,
					downNeighborID = -99998,
					upNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 526,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 527,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				yOffset += 64;
			}
			if (Game1.player.CanDyePants())
			{
				top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				top.X = base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
				int pantsColorLabelYOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? (-16) : 0);
				this.labels.Add(new ClickableComponent(new Rectangle(label_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16 + pantsColorLabelYOffset, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
				this.pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
				this.pantsColorPicker.setColor(Game1.player.GetPantsColor());
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 528,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 529,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 530,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
			}
		}
		else if (allow_clothing_changes)
		{
			yOffset += 68;
			int shirtArrowsExtraWidth = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 8 : 0);
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - shirtArrowsExtraWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 512,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.shirtLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Shirt")));
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + shirtArrowsExtraWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 513,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int pantsColorLabelYOffset2 = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? (-16) : 0);
			this.labels.Add(new ClickableComponent(new Rectangle(label_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16 + pantsColorLabelYOffset2, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
			top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
			this.pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
			this.pantsColorPicker.setColor(Game1.player.GetPantsColor());
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 528,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 529,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 530,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
		}
		else if (this.source == Source.ClothesDye)
		{
			yOffset += 60;
			top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
			top.X = base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
			this.labels.Add(new ClickableComponent(new Rectangle(label_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_DyeColor")));
			this.pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
			this.pantsColorPicker.setColor(this._itemToDye.clothesColor.Value);
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 528,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 529,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 530,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
		}
		this.skipIntroButton = new ClickableTextureComponent("Skip Intro", new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 - 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 80, 36, 36), null, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4f)
		{
			myID = 506,
			upNeighborID = 530,
			leftNeighborID = 517,
			rightNeighborID = 505
		};
		this.skipIntroButton.sourceRect.X = (this.skipIntro ? 236 : 227);
		if (allow_clothing_changes)
		{
			yOffset += 68;
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 629,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.pantsStyleLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Pants")));
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 517,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		yOffset += 68;
		if (allow_accessory_changes)
		{
			int accessoryArrowsExtraWidth = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 32 : 0);
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - accessoryArrowsExtraWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 516,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.accLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Accessory")));
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + accessoryArrowsExtraWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 517,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		if (Game1.gameMode == 3)
		{
			_ = Game1.locations;
		}
		if (this.petPortraitBox.HasValue)
		{
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(this.petPortraitBox.Value.Left - 64, this.petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 511,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(this.petPortraitBox.Value.Left + 64, this.petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 510,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			List<ClickableComponent> list2 = this.colorPickerCCs;
			if (list2 != null && list2.Count > 0)
			{
				this.colorPickerCCs[0].upNeighborID = 511;
				this.colorPickerCCs[0].upNeighborImmutable = true;
			}
		}
		this._shouldShowBackButton = true;
		if (this.source == Source.Dresser || this.source == Source.Wizard || this.source == Source.ClothesDye)
		{
			this._shouldShowBackButton = false;
		}
		if (this.source == Source.Dresser || this.source == Source.Wizard || this._isDyeMenu)
		{
			this.nameBoxCC.visible = false;
			this.farmnameBoxCC.visible = false;
			this.favThingBoxCC.visible = false;
			this.farmLabel.visible = false;
			this.nameLabel.visible = false;
			this.favoriteLabel.visible = false;
		}
		if (this.source == Source.Wizard)
		{
			this.nameLabel.visible = true;
			this.nameBoxCC.visible = true;
			this.favThingBoxCC.visible = true;
			this.favoriteLabel.visible = true;
			this.favThingBoxCC.bounds.Y = this.farmnameBoxCC.bounds.Y;
			this.favoriteLabel.bounds.Y = this.farmLabel.bounds.Y;
			this.favThingBox.Y = this.farmnameBox.Y;
		}
		this.skipIntroButton.visible = creatingNewSave;
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public virtual void LoadFarmTypeData()
	{
		List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
		this.farmTypeButtonNames.Add("Standard");
		this.farmTypeButtonNames.Add("Riverland");
		this.farmTypeButtonNames.Add("Forest");
		this.farmTypeButtonNames.Add("Hills");
		this.farmTypeButtonNames.Add("Wilderness");
		this.farmTypeButtonNames.Add("Four Corners");
		this.farmTypeButtonNames.Add("Beach");
		this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmStandard"));
		this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmFishing"));
		this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmForaging"));
		this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmMining"));
		this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmCombat"));
		this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmFourCorners"));
		this.farmTypeHoverText.Add(this.GetFarmTypeTooltip("Strings\\UI:Character_FarmBeach"));
		this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(0, 324, 22, 20)));
		this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(22, 324, 22, 20)));
		this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(44, 324, 22, 20)));
		this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(66, 324, 22, 20)));
		this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(88, 324, 22, 20)));
		this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(0, 345, 22, 20)));
		this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(22, 345, 22, 20)));
		if (farm_types != null)
		{
			foreach (ModFarmType farm_type in farm_types)
			{
				this.farmTypeButtonNames.Add("ModFarm_" + farm_type.Id);
				this.farmTypeHoverText.Add(this.GetFarmTypeTooltip(farm_type.TooltipStringPath));
				if (farm_type.IconTexture != null)
				{
					Texture2D texture = Game1.content.Load<Texture2D>(farm_type.IconTexture);
					this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(texture, new Rectangle(0, 0, 22, 20)));
				}
				else
				{
					this.farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(1, 324, 22, 20)));
				}
			}
		}
		this._farmPages = 1;
		if (farm_types != null)
		{
			this._farmPages = (int)Math.Floor((float)(this.farmTypeButtonNames.Count - 1) / 12f) + 1;
		}
	}

	public virtual void RefreshFarmTypeButtons()
	{
		this.farmTypeButtons.Clear();
		Point baseFarmButton = new Point(base.xPositionOnScreen + base.width + 4 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth);
		int index = this._currentFarmPage * 12;
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 88, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 531,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = 537
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 176, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 532,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 264, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 533,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 352, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 534,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 440, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 535,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 528, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 545,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 88, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 546,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 176, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 547,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 264, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 548,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 352, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 549,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 440, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 550,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < this.farmTypeButtonNames.Count)
		{
			this.farmTypeButtons.Add(new ClickableTextureComponent(this.farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 528, 88, 80), null, this.farmTypeHoverText[index], this.farmTypeIcons[index].Key, this.farmTypeIcons[index].Value, 4f)
			{
				myID = 551,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		this.farmTypePreviousPageButton = null;
		this.farmTypeNextPageButton = null;
		if (this._currentFarmPage > 0)
		{
			this.farmTypePreviousPageButton = new ClickableTextureComponent("", new Rectangle(baseFarmButton.X - 64 + 16, baseFarmButton.Y + 352 + 12, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 647,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
		if (this._currentFarmPage < this._farmPages - 1)
		{
			this.farmTypeNextPageButton = new ClickableTextureComponent("", new Rectangle(baseFarmButton.X + 172, baseFarmButton.Y + 352 + 12, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 647,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.showingCoopHelp)
		{
			base.currentlySnappedComponent = base.getComponentWithID(626);
		}
		else
		{
			base.currentlySnappedComponent = base.getComponentWithID(521);
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void gamePadButtonHeld(Buttons b)
	{
		base.gamePadButtonHeld(b);
		if (base.currentlySnappedComponent == null)
		{
			return;
		}
		switch (b)
		{
		case Buttons.DPadRight:
		case Buttons.LeftThumbstickRight:
			switch (base.currentlySnappedComponent.myID)
			{
			case 522:
				this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
				this.eyeColorPicker.changeHue(1);
				this.eyeColorPicker.Dirty = true;
				this._sliderOpTarget = this.eyeColorPicker;
				this._sliderAction = this._recolorEyesAction;
				break;
			case 523:
				this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
				this.eyeColorPicker.changeSaturation(1);
				this.eyeColorPicker.Dirty = true;
				this._sliderOpTarget = this.eyeColorPicker;
				this._sliderAction = this._recolorEyesAction;
				break;
			case 524:
				this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
				this.eyeColorPicker.changeValue(1);
				this.eyeColorPicker.Dirty = true;
				this._sliderOpTarget = this.eyeColorPicker;
				this._sliderAction = this._recolorEyesAction;
				break;
			case 525:
				this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
				this.hairColorPicker.changeHue(1);
				this.hairColorPicker.Dirty = true;
				this._sliderOpTarget = this.hairColorPicker;
				this._sliderAction = this._recolorHairAction;
				break;
			case 526:
				this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
				this.hairColorPicker.changeSaturation(1);
				this.hairColorPicker.Dirty = true;
				this._sliderOpTarget = this.hairColorPicker;
				this._sliderAction = this._recolorHairAction;
				break;
			case 527:
				this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
				this.hairColorPicker.changeValue(1);
				this.hairColorPicker.Dirty = true;
				this._sliderOpTarget = this.hairColorPicker;
				this._sliderAction = this._recolorHairAction;
				break;
			case 528:
				this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
				this.pantsColorPicker.changeHue(1);
				this.pantsColorPicker.Dirty = true;
				this._sliderOpTarget = this.pantsColorPicker;
				this._sliderAction = this._recolorPantsAction;
				break;
			case 529:
				this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
				this.pantsColorPicker.changeSaturation(1);
				this.pantsColorPicker.Dirty = true;
				this._sliderOpTarget = this.pantsColorPicker;
				this._sliderAction = this._recolorPantsAction;
				break;
			case 530:
				this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
				this.pantsColorPicker.changeValue(1);
				this.pantsColorPicker.Dirty = true;
				this._sliderOpTarget = this.pantsColorPicker;
				this._sliderAction = this._recolorPantsAction;
				break;
			}
			break;
		case Buttons.DPadLeft:
		case Buttons.LeftThumbstickLeft:
			switch (base.currentlySnappedComponent.myID)
			{
			case 522:
				this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
				this.eyeColorPicker.changeHue(-1);
				this.eyeColorPicker.Dirty = true;
				this._sliderOpTarget = this.eyeColorPicker;
				this._sliderAction = this._recolorEyesAction;
				break;
			case 523:
				this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
				this.eyeColorPicker.changeSaturation(-1);
				this.eyeColorPicker.Dirty = true;
				this._sliderOpTarget = this.eyeColorPicker;
				this._sliderAction = this._recolorEyesAction;
				break;
			case 524:
				this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
				this.eyeColorPicker.changeValue(-1);
				this.eyeColorPicker.Dirty = true;
				this._sliderOpTarget = this.eyeColorPicker;
				this._sliderAction = this._recolorEyesAction;
				break;
			case 525:
				this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
				this.hairColorPicker.changeHue(-1);
				this.hairColorPicker.Dirty = true;
				this._sliderOpTarget = this.hairColorPicker;
				this._sliderAction = this._recolorHairAction;
				break;
			case 526:
				this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
				this.hairColorPicker.changeSaturation(-1);
				this.hairColorPicker.Dirty = true;
				this._sliderOpTarget = this.hairColorPicker;
				this._sliderAction = this._recolorHairAction;
				break;
			case 527:
				this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
				this.hairColorPicker.changeValue(-1);
				this.hairColorPicker.Dirty = true;
				this._sliderOpTarget = this.hairColorPicker;
				this._sliderAction = this._recolorHairAction;
				break;
			case 528:
				this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
				this.pantsColorPicker.changeHue(-1);
				this.pantsColorPicker.Dirty = true;
				this._sliderOpTarget = this.pantsColorPicker;
				this._sliderAction = this._recolorPantsAction;
				break;
			case 529:
				this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
				this.pantsColorPicker.changeSaturation(-1);
				this.pantsColorPicker.Dirty = true;
				this._sliderOpTarget = this.pantsColorPicker;
				this._sliderAction = this._recolorPantsAction;
				break;
			case 530:
				this.pantsColorPicker.LastColor = this.pantsColorPicker.getSelectedColor();
				this.pantsColorPicker.changeValue(-1);
				this.pantsColorPicker.Dirty = true;
				this._sliderOpTarget = this.pantsColorPicker;
				this._sliderAction = this._recolorPantsAction;
				break;
			}
			break;
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if (base.currentlySnappedComponent == null)
		{
			return;
		}
		switch (button)
		{
		case Buttons.RightTrigger:
		{
			int myID = base.currentlySnappedComponent.myID;
			if ((uint)(myID - 512) <= 9u)
			{
				this.selectionClick(base.currentlySnappedComponent.name, 1);
			}
			break;
		}
		case Buttons.LeftTrigger:
		{
			int myID = base.currentlySnappedComponent.myID;
			if ((uint)(myID - 512) <= 9u)
			{
				this.selectionClick(base.currentlySnappedComponent.name, -1);
			}
			break;
		}
		case Buttons.B:
			if (this.showingCoopHelp)
			{
				this.receiveLeftClick(this.coopHelpOkButton.bounds.Center.X, this.coopHelpOkButton.bounds.Center.Y);
			}
			break;
		}
	}

	private void optionButtonClick(string name)
	{
		if (name.StartsWith("ModFarm_"))
		{
			if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
			{
				List<ModFarmType> list = DataLoader.AdditionalFarms(Game1.content);
				string farmId = name.Substring("ModFarm_".Length);
				foreach (ModFarmType farmType in list)
				{
					if (farmType.Id == farmId)
					{
						Game1.whichFarm = 7;
						Game1.whichModFarm = farmType;
						Game1.spawnMonstersAtNight = farmType.SpawnMonstersByDefault;
						break;
					}
				}
			}
		}
		else
		{
			switch (name)
			{
			case "Standard":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.whichFarm = 0;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Riverland":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.whichFarm = 1;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Forest":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.whichFarm = 2;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Hills":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.whichFarm = 3;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Wilderness":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.whichFarm = 4;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = true;
				}
				break;
			case "Four Corners":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.whichFarm = 5;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Beach":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.whichFarm = 6;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Male":
				Game1.player.changeGender(male: true);
				if (this.source != Source.Wizard)
				{
					Game1.player.changeHairStyle(0);
				}
				break;
			case "Close":
				Game1.cabinsSeparate = false;
				break;
			case "Separate":
				Game1.cabinsSeparate = true;
				break;
			case "Female":
				Game1.player.changeGender(male: false);
				if (this.source != Source.Wizard)
				{
					Game1.player.changeHairStyle(16);
				}
				break;
			case "Cat":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.player.whichPetType = "Cat";
				}
				break;
			case "Dog":
				if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
				{
					Game1.player.whichPetType = "Dog";
				}
				break;
			case "OK":
			{
				if (!this.canLeaveMenu())
				{
					return;
				}
				if (this._itemToDye != null)
				{
					if (!Game1.player.IsEquippedItem(this._itemToDye))
					{
						Utility.CollectOrDrop(this._itemToDye);
					}
					this._itemToDye = null;
				}
				if (this.source == Source.ClothesDye)
				{
					Game1.exitActiveMenu();
					break;
				}
				Game1.player.Name = Utility.FilterDirtyWordsIfStrictPlatform(this.nameBox.Text.Trim());
				Game1.player.displayName = Game1.player.Name;
				Game1.player.favoriteThing.Value = Utility.FilterDirtyWordsIfStrictPlatform(this.favThingBox.Text.Trim());
				Game1.player.isCustomized.Value = true;
				Game1.player.ConvertClothingOverrideToClothesItems();
				if (this.source == Source.HostNewFarm)
				{
					Game1.multiplayerMode = 2;
				}
				try
				{
					if (Game1.player.Name != this.oldName)
					{
						int start = Game1.player.Name.IndexOf("[");
						int end = Game1.player.Name.IndexOf("]");
						if (start >= 0 && end > start)
						{
							string itemName = ItemRegistry.GetData(Game1.player.Name.Substring(start + 1, end - start - 1))?.DisplayName;
							if (itemName != null)
							{
								switch (Game1.random.Next(5))
								{
								case 0:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg1"), new Color(104, 214, 255));
									break;
								case 1:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg2", Lexicon.makePlural(itemName)), new Color(100, 50, 255));
									break;
								case 2:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg3", Lexicon.makePlural(itemName)), new Color(0, 220, 40));
									break;
								case 3:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg4"), new Color(0, 220, 40));
									DelayedAction.functionAfterDelay(delegate
									{
										Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg5"), new Color(104, 214, 255));
									}, 12000);
									break;
								case 4:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg6", Lexicon.getProperArticleForWord(itemName), itemName), new Color(100, 120, 255));
									break;
								}
							}
						}
					}
				}
				catch
				{
				}
				string changed_pet_name = null;
				if (this.petPortraitBox.HasValue && Game1.IsMasterGame && Game1.gameMode == 3 && Game1.locations != null)
				{
					Pet pet = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
					if (pet != null && this.petHasChanges(pet))
					{
						pet.petType.Value = Game1.player.whichPetType;
						pet.whichBreed.Value = Game1.player.whichPetBreed;
						changed_pet_name = pet.getName();
					}
				}
				if (Game1.activeClickableMenu is TitleMenu titleMenu)
				{
					titleMenu.createdNewCharacter(this.skipIntro);
					break;
				}
				Game1.exitActiveMenu();
				if (Game1.currentMinigame is Intro intro)
				{
					intro.doneCreatingCharacter();
					break;
				}
				switch (this.source)
				{
				case Source.Wizard:
					if (changed_pet_name != null)
					{
						Game1.multiplayer.globalChatInfoMessage("Makeover_Pet", Game1.player.Name, changed_pet_name);
					}
					else
					{
						Game1.multiplayer.globalChatInfoMessage("Makeover", Game1.player.Name);
					}
					Game1.flashAlpha = 1f;
					Game1.playSound("yoba");
					break;
				case Source.ClothesDye:
					Game1.playSound("yoba");
					break;
				}
				break;
			}
			}
		}
		Game1.playSound("coin");
	}

	public bool petHasChanges(Pet pet)
	{
		if (Game1.player.whichPetType != pet.petType.Value)
		{
			return true;
		}
		if (Game1.player.whichPetBreed != pet.whichBreed.Value)
		{
			return true;
		}
		return false;
	}

	/// <summary>Load the tooltip translation for a farm type in the expected format.</summary>
	/// <param name="translationKey">The translation key to load.</param>
	/// <remarks>This returns a tooltip string in the form <c>name_description</c>.</remarks>
	protected virtual string GetFarmTypeTooltip(string translationKey)
	{
		string text = Game1.content.LoadString(translationKey);
		string[] parts = text.Split('_', 2);
		if (parts.Length == 1 || parts[1].Length == 0)
		{
			text = parts[0] + "_ ";
		}
		return text;
	}

	protected List<KeyValuePair<string, string>> GetPetTypesAndBreeds()
	{
		if (this._petTypesAndBreeds == null)
		{
			this._petTypesAndBreeds = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, PetData> pair in Game1.petData)
			{
				if (this.isModifyingExistingPet && Game1.player.whichPetType != pair.Key)
				{
					continue;
				}
				foreach (PetBreed breed in pair.Value.Breeds)
				{
					if (breed.CanBeChosenAtStart)
					{
						this._petTypesAndBreeds.Add(new KeyValuePair<string, string>(pair.Key, breed.Id));
					}
				}
			}
		}
		return this._petTypesAndBreeds;
	}

	private void selectionClick(string name, int change)
	{
		if (name == null)
		{
			return;
		}
		switch (name.Length)
		{
		case 4:
			switch (name[0])
			{
			case 'S':
				if (name == "Skin")
				{
					Game1.player.changeSkinColor(Game1.player.skin.Value + change);
					Game1.playSound("skeletonStep");
				}
				break;
			case 'H':
				if (name == "Hair")
				{
					List<int> all_hairs = Farmer.GetAllHairstyleIndices();
					int current_index = all_hairs.IndexOf(Game1.player.hair.Value);
					current_index += change;
					if (current_index >= all_hairs.Count)
					{
						current_index = 0;
					}
					else if (current_index < 0)
					{
						current_index = all_hairs.Count - 1;
					}
					Game1.player.changeHairStyle(all_hairs[current_index]);
					Game1.playSound("grassyStep");
				}
				break;
			}
			break;
		case 3:
			switch (name[0])
			{
			case 'A':
				if (name == "Acc")
				{
					Game1.player.changeAccessory(Game1.player.accessory.Value + change);
					Game1.playSound("purchase");
				}
				break;
			case 'P':
				if (name == "Pet")
				{
					List<KeyValuePair<string, string>> pets = this.GetPetTypesAndBreeds();
					int index = pets.IndexOf(new KeyValuePair<string, string>(Game1.player.whichPetType, Game1.player.whichPetBreed));
					index = ((index != -1) ? (index + change) : 0);
					if (index < 0)
					{
						index = pets.Count - 1;
					}
					else if (index >= pets.Count)
					{
						index = 0;
					}
					KeyValuePair<string, string> selectedPetType = pets[index];
					Game1.player.whichPetType = selectedPetType.Key;
					Game1.player.whichPetBreed = selectedPetType.Value;
					Game1.playSound("coin");
				}
				break;
			}
			break;
		case 5:
			if (name == "Shirt")
			{
				Game1.player.rotateShirt(change, this.GetValidShirtIds());
				Game1.playSound("coin");
			}
			break;
		case 11:
			if (name == "Pants Style")
			{
				Game1.player.rotatePantStyle(change, this.GetValidPantsIds());
				Game1.playSound("coin");
			}
			break;
		case 9:
			if (name == "Direction")
			{
				this._displayFarmer.faceDirection((this._displayFarmer.FacingDirection - change + 4) % 4);
				this._displayFarmer.FarmerSprite.StopAnimation();
				this._displayFarmer.completelyStopAnimatingOrDoingAction();
				Game1.playSound("pickUpItem");
			}
			break;
		case 6:
			if (name == "Cabins")
			{
				if ((Game1.startingCabins != 0 || change >= 0) && (Game1.startingCabins != Game1.multiplayer.playerLimit - 1 || change <= 0))
				{
					Game1.playSound("axchop");
				}
				Game1.startingCabins += change;
				Game1.startingCabins = Math.Max(0, Math.Min(Game1.multiplayer.playerLimit - 1, Game1.startingCabins));
			}
			break;
		case 10:
			if (name == "Difficulty")
			{
				if (Game1.player.difficultyModifier < 1f && change < 0)
				{
					Game1.playSound("breathout");
					Game1.player.difficultyModifier += 0.25f;
				}
				else if (Game1.player.difficultyModifier > 0.25f && change > 0)
				{
					Game1.playSound("batFlap");
					Game1.player.difficultyModifier -= 0.25f;
				}
			}
			break;
		case 7:
			if (name == "Wallets")
			{
				if (Game1.player.team.useSeparateWallets.Value)
				{
					Game1.playSound("coin");
					Game1.player.team.useSeparateWallets.Value = false;
				}
				else
				{
					Game1.playSound("coin");
					Game1.player.team.useSeparateWallets.Value = true;
				}
			}
			break;
		case 8:
			break;
		}
	}

	public void ShowAdvancedOptions()
	{
		base.AddDependency();
		(TitleMenu.subMenu = new AdvancedGameOptions()).exitFunction = delegate
		{
			TitleMenu.subMenu = this;
			base.RemoveDependency();
			this.ResetComponents();
			this.populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				this.setCurrentlySnappedComponentTo(636);
				this.snapCursorToCurrentSnappedComponent();
			}
		};
	}

	public override bool readyToClose()
	{
		if (this.showingCoopHelp)
		{
			return false;
		}
		if (Game1.lastCursorMotionWasMouse)
		{
			foreach (ClickableTextureComponent farmTypeButton in this.farmTypeButtons)
			{
				if (farmTypeButton.containsPoint(Game1.getMouseX(ui_scale: true), Game1.getMouseY(ui_scale: true)))
				{
					return false;
				}
			}
		}
		return base.readyToClose();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.showingCoopHelp)
		{
			if (this.coopHelpOkButton != null && this.coopHelpOkButton.containsPoint(x, y))
			{
				this.showingCoopHelp = false;
				Game1.playSound("bigDeSelect");
				if (Game1.options.SnappyMenus)
				{
					base.currentlySnappedComponent = this.coopHelpButton;
					this.snapCursorToCurrentSnappedComponent();
				}
			}
			if (this.coopHelpScreen == 0 && this.coopHelpRightButton != null && this.coopHelpRightButton.containsPoint(x, y))
			{
				this.coopHelpScreen++;
				this.coopHelpString = Game1.parseText(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString2").Replace("^", Environment.NewLine), Game1.dialogueFont, base.width + 384 - IClickableMenu.borderWidth * 2);
				Game1.playSound("shwip");
			}
			if (this.coopHelpScreen == 1 && this.coopHelpLeftButton != null && this.coopHelpLeftButton.containsPoint(x, y))
			{
				this.coopHelpScreen--;
				string rawText = string.Format(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.multiplayer.playerLimit - 1);
				this.coopHelpString = Game1.parseText(rawText, Game1.dialogueFont, base.width + 384 - IClickableMenu.borderWidth * 2);
				Game1.playSound("shwip");
			}
			return;
		}
		if (this.genderButtons.Count > 0)
		{
			foreach (ClickableComponent c in this.genderButtons)
			{
				if (c.containsPoint(x, y))
				{
					this.optionButtonClick(c.name);
					c.scale -= 0.5f;
					c.scale = Math.Max(3.5f, c.scale);
				}
			}
		}
		if (this.farmTypeNextPageButton != null && this.farmTypeNextPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this._currentFarmPage++;
			this.RefreshFarmTypeButtons();
		}
		else if (this.farmTypePreviousPageButton != null && this.farmTypePreviousPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this._currentFarmPage--;
			this.RefreshFarmTypeButtons();
		}
		else if (this.farmTypeButtons.Count > 0)
		{
			foreach (ClickableTextureComponent c2 in this.farmTypeButtons)
			{
				if (c2.containsPoint(x, y) && !c2.name.Contains("Gray"))
				{
					this.optionButtonClick(c2.name);
					c2.scale -= 0.5f;
					c2.scale = Math.Max(3.5f, c2.scale);
				}
			}
		}
		if (this.cabinLayoutButtons.Count > 0)
		{
			foreach (ClickableTextureComponent c3 in this.cabinLayoutButtons)
			{
				if (Game1.startingCabins > 0 && c3.containsPoint(x, y))
				{
					this.optionButtonClick(c3.name);
					c3.scale -= 0.5f;
					c3.scale = Math.Max(3.5f, c3.scale);
				}
			}
		}
		if (this.leftSelectionButtons.Count > 0)
		{
			foreach (ClickableComponent c4 in this.leftSelectionButtons)
			{
				if (c4.containsPoint(x, y))
				{
					this.selectionClick(c4.name, -1);
					if (c4.scale != 0f)
					{
						c4.scale -= 0.25f;
						c4.scale = Math.Max(0.75f, c4.scale);
					}
				}
			}
		}
		if (this.rightSelectionButtons.Count > 0)
		{
			foreach (ClickableComponent c5 in this.rightSelectionButtons)
			{
				if (c5.containsPoint(x, y))
				{
					this.selectionClick(c5.name, 1);
					if (c5.scale != 0f)
					{
						c5.scale -= 0.25f;
						c5.scale = Math.Max(0.75f, c5.scale);
					}
				}
			}
		}
		if (this.okButton.containsPoint(x, y) && this.canLeaveMenu())
		{
			this.optionButtonClick(this.okButton.name);
			this.okButton.scale -= 0.25f;
			this.okButton.scale = Math.Max(0.75f, this.okButton.scale);
		}
		if (this.hairColorPicker != null && this.hairColorPicker.containsPoint(x, y))
		{
			Color color = this.hairColorPicker.click(x, y);
			if (this.source == Source.DyePots)
			{
				if (Game1.player.CanDyeShirt())
				{
					Game1.player.shirtItem.Value.clothesColor.Value = color;
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
			}
			else
			{
				Game1.player.changeHairColor(color);
			}
			this.lastHeldColorPicker = this.hairColorPicker;
		}
		else if (this.pantsColorPicker != null && this.pantsColorPicker.containsPoint(x, y))
		{
			Color color2 = this.pantsColorPicker.click(x, y);
			switch (this.source)
			{
			case Source.DyePots:
				if (Game1.player.CanDyePants())
				{
					Game1.player.pantsItem.Value.clothesColor.Value = color2;
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
				break;
			case Source.ClothesDye:
				this.DyeItem(color2);
				break;
			default:
				Game1.player.changePantsColor(color2);
				break;
			}
			this.lastHeldColorPicker = this.pantsColorPicker;
		}
		else if (this.eyeColorPicker != null && this.eyeColorPicker.containsPoint(x, y))
		{
			Game1.player.changeEyeColor(this.eyeColorPicker.click(x, y));
			this.lastHeldColorPicker = this.eyeColorPicker;
		}
		if (this.source != Source.Dresser && this.source != Source.ClothesDye && this.source != Source.DyePots)
		{
			this.nameBox.Update();
			if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
			{
				this.farmnameBox.Update();
			}
			else
			{
				this.farmnameBox.Text = Game1.MasterPlayer.farmName.Value;
			}
			this.favThingBox.Update();
			if ((this.source == Source.NewGame || this.source == Source.HostNewFarm) && this.skipIntroButton.containsPoint(x, y))
			{
				Game1.playSound("drumkit6");
				this.skipIntroButton.sourceRect.X = ((this.skipIntroButton.sourceRect.X == 227) ? 236 : 227);
				this.skipIntro = !this.skipIntro;
			}
		}
		if (this.coopHelpButton != null && this.coopHelpButton.containsPoint(x, y))
		{
			if (Game1.options.SnappyMenus)
			{
				base.currentlySnappedComponent = this.coopHelpOkButton;
				this.snapCursorToCurrentSnappedComponent();
			}
			Game1.playSound("bigSelect");
			this.showingCoopHelp = true;
			this.coopHelpScreen = 0;
			string rawText2 = string.Format(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.multiplayer.playerLimit - 1);
			this.coopHelpString = Game1.parseText(rawText2, Game1.dialogueFont, base.width + 384 - IClickableMenu.borderWidth * 2);
			this.helpStringSize = Game1.dialogueFont.MeasureString(this.coopHelpString);
			this.coopHelpRightButton.bounds.Y = base.yPositionOnScreen + (int)this.helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
			this.coopHelpRightButton.bounds.X = base.xPositionOnScreen + (int)this.helpStringSize.X - IClickableMenu.borderWidth * 5;
			this.coopHelpLeftButton.bounds.Y = base.yPositionOnScreen + (int)this.helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
			this.coopHelpLeftButton.bounds.X = base.xPositionOnScreen - IClickableMenu.borderWidth * 4;
		}
		if (this.advancedOptionsButton != null && this.advancedOptionsButton.containsPoint(x, y))
		{
			Game1.playSound("drumkit6");
			this.ShowAdvancedOptions();
		}
		if (!this.randomButton.containsPoint(x, y))
		{
			return;
		}
		string sound = "drumkit6";
		if (this.timesRandom > 0)
		{
			switch (Game1.random.Next(15))
			{
			case 0:
				sound = "drumkit1";
				break;
			case 1:
				sound = "dirtyHit";
				break;
			case 2:
				sound = "axchop";
				break;
			case 3:
				sound = "hoeHit";
				break;
			case 4:
				sound = "fishSlap";
				break;
			case 5:
				sound = "drumkit6";
				break;
			case 6:
				sound = "drumkit5";
				break;
			case 7:
				sound = "drumkit6";
				break;
			case 8:
				sound = "junimoMeep1";
				break;
			case 9:
				sound = "coin";
				break;
			case 10:
				sound = "axe";
				break;
			case 11:
				sound = "hammer";
				break;
			case 12:
				sound = "drumkit2";
				break;
			case 13:
				sound = "drumkit4";
				break;
			case 14:
				sound = "drumkit3";
				break;
			}
		}
		Game1.playSound(sound);
		this.timesRandom++;
		if (this.accLabel != null && this.accLabel.visible)
		{
			if (Game1.random.NextDouble() < 0.33)
			{
				if (Game1.player.IsMale)
				{
					if (Game1.random.NextDouble() < 0.33)
					{
						if (Game1.random.NextDouble() < 0.8)
						{
							Game1.player.changeAccessory(Game1.random.Next(7));
						}
						else
						{
							Game1.player.changeAccessory(Game1.random.Next(19, 21));
						}
					}
					else if (Game1.random.NextDouble() < 0.33)
					{
						Game1.player.changeAccessory(Game1.random.Choose<int>(25, 14, 17, 10, 9));
					}
					else if (Game1.random.NextDouble() < 0.1)
					{
						Game1.player.changeAccessory(Game1.random.Next(19));
					}
				}
				else if (Game1.random.NextDouble() < 0.33)
				{
					Game1.player.changeAccessory(Game1.random.Next(6, 19));
				}
				else if (Game1.random.NextDouble() < 0.5)
				{
					Game1.player.changeAccessory(Game1.random.Choose(23, 27, 28));
				}
				else
				{
					Game1.player.changeAccessory(Game1.random.Choose<int>(25, 14, 17, 10, 9));
				}
			}
			else
			{
				Game1.player.changeAccessory(-1);
			}
		}
		if (this.skinLabel != null && this.skinLabel.visible)
		{
			Game1.player.changeSkinColor(Game1.random.Next(6));
			if (Game1.random.NextDouble() < 0.15)
			{
				Game1.player.changeSkinColor(Game1.random.Next(24));
			}
		}
		if (this.hairLabel != null && this.hairLabel.visible)
		{
			if (Game1.player.IsMale)
			{
				Game1.player.changeHairStyle(Game1.random.NextBool() ? Game1.random.Next(16) : Game1.random.Next(108, 118));
			}
			else
			{
				Game1.player.changeHairStyle(Game1.random.Next(16, 41));
			}
			Color hairColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			if (Game1.random.NextBool())
			{
				hairColor.R /= 2;
				hairColor.G /= 2;
				hairColor.B /= 2;
			}
			if (Game1.random.NextBool())
			{
				hairColor.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				hairColor.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				hairColor.B = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				if (hairColor.B > hairColor.R)
				{
					hairColor.B = (byte)Math.Max(0, hairColor.B - 50);
				}
				if (hairColor.B > hairColor.G)
				{
					hairColor.B = (byte)Math.Max(0, hairColor.B - 50);
				}
				if (hairColor.G > hairColor.R)
				{
					hairColor.G = (byte)Math.Max(0, hairColor.R - 50);
				}
				hairColor.R = (byte)Math.Min(255, hairColor.R + 50);
				hairColor.G = (byte)Math.Min(255, hairColor.G + 50);
			}
			else if (Game1.random.NextDouble() < 0.33)
			{
				hairColor = new Color(Game1.random.Next(80, 130), Game1.random.Next(35, 70), 0);
			}
			if (hairColor.R < 100 && hairColor.G < 100 && hairColor.B < 100 && Game1.random.NextDouble() < 0.8)
			{
				hairColor = Utility.getBlendedColor(hairColor, Color.Tan);
			}
			if (Game1.player.hasDarkSkin() && Game1.random.NextDouble() < 0.5)
			{
				hairColor = new Color(Game1.random.Next(50, 100), Game1.random.Next(25, 40), 0);
			}
			Game1.player.changeHairColor(hairColor);
			this.hairColorPicker.setColor(hairColor);
		}
		if (this.shirtLabel != null && this.shirtLabel.visible)
		{
			string shirtSelection = "";
			Utility.TryGetRandomExcept(this.GetValidShirtIds(), Game1.player.IsMale ? new HashSet<string>
			{
				"1056", "1057", "1070", "1046", "1040", "1060", "1090", "1051", "1082", "1107",
				"1080", "1083", "1092", "1072", "1076", "1041"
			} : new HashSet<string>(), Game1.random, out shirtSelection);
			Game1.player.changeShirt(shirtSelection);
		}
		if (this.pantsStyleLabel != null && this.pantsStyleLabel.visible)
		{
			Color pantsColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			if (Game1.random.NextBool())
			{
				pantsColor.R /= 2;
				pantsColor.G /= 2;
				pantsColor.B /= 2;
			}
			if (Game1.random.NextBool())
			{
				pantsColor.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				pantsColor.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				pantsColor.B = (byte)Game1.random.Next(15, 50);
			}
			switch (Game1.player.GetShirtIndex())
			{
			case 50:
				pantsColor = new Color(226, 133, 160);
				break;
			case 0:
			case 7:
			case 71:
				pantsColor = new Color(34, 29, 173);
				break;
			case 68:
			case 88:
				pantsColor = new Color(119, 215, 130);
				break;
			case 67:
			case 72:
				pantsColor = new Color(108, 134, 224);
				break;
			case 79:
			case 99:
			case 103:
				pantsColor = new Color(55, 55, 60);
				break;
			}
			Game1.player.changePantsColor(pantsColor);
			this.pantsColorPicker.setColor(Game1.player.GetPantsColor());
		}
		if (this.eyeColorPicker != null)
		{
			Color eyeColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			eyeColor.R /= 2;
			eyeColor.G /= 2;
			eyeColor.B /= 2;
			if (Game1.random.NextBool())
			{
				eyeColor.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				eyeColor.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				eyeColor.B = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				if (eyeColor.B > eyeColor.R)
				{
					eyeColor.B = (byte)Math.Max(0, eyeColor.B - 50);
				}
				if (eyeColor.B > eyeColor.G)
				{
					eyeColor.B = (byte)Math.Max(0, eyeColor.B - 50);
				}
				if (eyeColor.G > eyeColor.R)
				{
					eyeColor.G = (byte)Math.Max(0, eyeColor.R - 50);
				}
			}
			Game1.player.changeEyeColor(eyeColor);
			this.eyeColorPicker.setColor(Game1.player.newEyeColor.Value);
		}
		this.randomButton.scale = 3.5f;
	}

	/// <summary>Get the shirts or pants which can be selected on the character customization screen.</summary>
	/// <typeparam name="TData">The clothing data.</typeparam>
	/// <param name="equippedId">The unqualified item ID for the item equipped by the player.</param>
	/// <param name="data">The data to search.</param>
	/// <param name="canChooseDuringCharacterCustomization">Get whether a clothing item should be visible on the character customization screen.</param>
	public List<string> GetValidClothingIds<TData>(string equippedId, IDictionary<string, TData> data, Func<TData, bool> canChooseDuringCharacterCustomization)
	{
		List<string> validIds = new List<string>();
		foreach (KeyValuePair<string, TData> pair in data)
		{
			if (pair.Key == equippedId || canChooseDuringCharacterCustomization(pair.Value))
			{
				validIds.Add(pair.Key);
			}
		}
		return validIds;
	}

	/// <summary>Get the pants which can be selected on the character customization screen.</summary>
	public List<string> GetValidPantsIds()
	{
		return this.GetValidClothingIds(Game1.player.pants.Value, Game1.pantsData, (PantsData data) => data.CanChooseDuringCharacterCustomization);
	}

	/// <summary>Get the shirts which can be selected on the character customization screen.</summary>
	public List<string> GetValidShirtIds()
	{
		return this.GetValidClothingIds(Game1.player.shirt.Value, Game1.shirtData, (ShirtData data) => data.CanChooseDuringCharacterCustomization);
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		this.colorPickerTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		if (this.colorPickerTimer > 0)
		{
			return;
		}
		if (this.lastHeldColorPicker != null && !Game1.options.SnappyMenus)
		{
			if (this.lastHeldColorPicker.Equals(this.hairColorPicker))
			{
				Color color = this.hairColorPicker.clickHeld(x, y);
				if (this.source == Source.DyePots)
				{
					if (Game1.player.CanDyeShirt())
					{
						Game1.player.shirtItem.Value.clothesColor.Value = color;
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				}
				else
				{
					Game1.player.changeHairColor(color);
				}
			}
			if (this.lastHeldColorPicker.Equals(this.pantsColorPicker))
			{
				Color color2 = this.pantsColorPicker.clickHeld(x, y);
				switch (this.source)
				{
				case Source.DyePots:
					if (Game1.player.CanDyePants())
					{
						Game1.player.pantsItem.Value.clothesColor.Value = color2;
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						this._displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
					break;
				case Source.ClothesDye:
					this.DyeItem(color2);
					break;
				default:
					Game1.player.changePantsColor(color2);
					break;
				}
			}
			if (this.lastHeldColorPicker.Equals(this.eyeColorPicker))
			{
				Game1.player.changeEyeColor(this.eyeColorPicker.clickHeld(x, y));
			}
		}
		this.colorPickerTimer = 100;
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		this.hairColorPicker?.releaseClick();
		this.pantsColorPicker?.releaseClick();
		this.eyeColorPicker?.releaseClick();
		this.lastHeldColorPicker = null;
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (key == Keys.Tab)
		{
			switch (this.source)
			{
			case Source.NewGame:
			case Source.HostNewFarm:
				if (this.nameBox.Selected)
				{
					this.farmnameBox.SelectMe();
					this.nameBox.Selected = false;
				}
				else if (this.farmnameBox.Selected)
				{
					this.farmnameBox.Selected = false;
					this.favThingBox.SelectMe();
				}
				else
				{
					this.favThingBox.Selected = false;
					this.nameBox.SelectMe();
				}
				break;
			case Source.NewFarmhand:
				if (this.nameBox.Selected)
				{
					this.favThingBox.SelectMe();
					this.nameBox.Selected = false;
				}
				else
				{
					this.favThingBox.Selected = false;
					this.nameBox.SelectMe();
				}
				break;
			}
		}
		if (Game1.options.SnappyMenus && !Game1.options.doesInputListContain(Game1.options.menuButton, key) && Game1.GetKeyboardState().GetPressedKeys().Length == 0)
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		this.hoverTitle = "";
		foreach (ClickableTextureComponent c in this.leftSelectionButtons)
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
			}
			if (c.name.Equals("Cabins") && Game1.startingCabins == 0)
			{
				c.scale = 0f;
			}
		}
		foreach (ClickableTextureComponent c2 in this.rightSelectionButtons)
		{
			if (c2.containsPoint(x, y))
			{
				c2.scale = Math.Min(c2.scale + 0.02f, c2.baseScale + 0.1f);
			}
			else
			{
				c2.scale = Math.Max(c2.scale - 0.02f, c2.baseScale);
			}
			if (c2.name.Equals("Cabins") && Game1.startingCabins == Game1.multiplayer.playerLimit - 1)
			{
				c2.scale = 0f;
			}
		}
		if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
		{
			foreach (ClickableTextureComponent c3 in this.farmTypeButtons)
			{
				if (c3.containsPoint(x, y) && !c3.name.Contains("Gray"))
				{
					c3.scale = Math.Min(c3.scale + 0.02f, c3.baseScale + 0.1f);
					this.hoverTitle = c3.hoverText.Split('_')[0];
					this.hoverText = c3.hoverText.Split('_')[1];
					continue;
				}
				c3.scale = Math.Max(c3.scale - 0.02f, c3.baseScale);
				if (c3.name.Contains("Gray") && c3.containsPoint(x, y))
				{
					this.hoverText = "Reach level 10 " + Game1.content.LoadString("Strings\\UI:Character_" + c3.name.Split('_')[1]) + " to unlock.";
				}
			}
		}
		foreach (ClickableTextureComponent c4 in this.genderButtons)
		{
			if (c4.containsPoint(x, y))
			{
				c4.scale = Math.Min(c4.scale + 0.05f, c4.baseScale + 0.5f);
			}
			else
			{
				c4.scale = Math.Max(c4.scale - 0.05f, c4.baseScale);
			}
		}
		if (this.source == Source.NewGame || this.source == Source.HostNewFarm)
		{
			foreach (ClickableTextureComponent c5 in this.cabinLayoutButtons)
			{
				if (Game1.startingCabins > 0 && c5.containsPoint(x, y))
				{
					c5.scale = Math.Min(c5.scale + 0.05f, c5.baseScale + 0.5f);
					this.hoverText = c5.hoverText;
				}
				else
				{
					c5.scale = Math.Max(c5.scale - 0.05f, c5.baseScale);
				}
			}
		}
		if (this.okButton.containsPoint(x, y) && this.canLeaveMenu())
		{
			this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.1f);
		}
		else
		{
			this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
		}
		if (this.coopHelpButton != null)
		{
			if (this.coopHelpButton.containsPoint(x, y))
			{
				this.coopHelpButton.scale = Math.Min(this.coopHelpButton.scale + 0.05f, this.coopHelpButton.baseScale + 0.5f);
				this.hoverText = this.coopHelpButton.hoverText;
			}
			else
			{
				this.coopHelpButton.scale = Math.Max(this.coopHelpButton.scale - 0.05f, this.coopHelpButton.baseScale);
			}
		}
		if (this.coopHelpOkButton != null)
		{
			if (this.coopHelpOkButton.containsPoint(x, y))
			{
				this.coopHelpOkButton.scale = Math.Min(this.coopHelpOkButton.scale + 0.025f, this.coopHelpOkButton.baseScale + 0.2f);
			}
			else
			{
				this.coopHelpOkButton.scale = Math.Max(this.coopHelpOkButton.scale - 0.025f, this.coopHelpOkButton.baseScale);
			}
		}
		if (this.coopHelpRightButton != null)
		{
			if (this.coopHelpRightButton.containsPoint(x, y))
			{
				this.coopHelpRightButton.scale = Math.Min(this.coopHelpRightButton.scale + 0.025f, this.coopHelpRightButton.baseScale + 0.2f);
			}
			else
			{
				this.coopHelpRightButton.scale = Math.Max(this.coopHelpRightButton.scale - 0.025f, this.coopHelpRightButton.baseScale);
			}
		}
		if (this.coopHelpLeftButton != null)
		{
			if (this.coopHelpLeftButton.containsPoint(x, y))
			{
				this.coopHelpLeftButton.scale = Math.Min(this.coopHelpLeftButton.scale + 0.025f, this.coopHelpLeftButton.baseScale + 0.2f);
			}
			else
			{
				this.coopHelpLeftButton.scale = Math.Max(this.coopHelpLeftButton.scale - 0.025f, this.coopHelpLeftButton.baseScale);
			}
		}
		this.advancedOptionsButton?.tryHover(x, y);
		this.farmTypeNextPageButton?.tryHover(x, y);
		this.farmTypePreviousPageButton?.tryHover(x, y);
		this.randomButton.tryHover(x, y, 0.25f);
		this.randomButton.tryHover(x, y, 0.25f);
		if ((this.hairColorPicker != null && this.hairColorPicker.containsPoint(x, y)) || (this.pantsColorPicker != null && this.pantsColorPicker.containsPoint(x, y)) || (this.eyeColorPicker != null && this.eyeColorPicker.containsPoint(x, y)))
		{
			Game1.SetFreeCursorDrag();
		}
		this.nameBox.Hover(x, y);
		this.farmnameBox.Hover(x, y);
		this.favThingBox.Hover(x, y);
		this.skipIntroButton.tryHover(x, y);
	}

	public bool canLeaveMenu()
	{
		if (this.source != Source.ClothesDye && this.source != Source.DyePots)
		{
			if (Game1.player.Name.Length > 0 && Game1.player.farmName.Length > 0)
			{
				return Game1.player.favoriteThing.Length > 0;
			}
			return false;
		}
		return true;
	}

	private string getNameOfDifficulty()
	{
		if (Game1.player.difficultyModifier < 0.5f)
		{
			return this.superDiffString;
		}
		if (Game1.player.difficultyModifier < 0.75f)
		{
			return this.hardDiffString;
		}
		if (Game1.player.difficultyModifier < 1f)
		{
			return this.toughDiffString;
		}
		return this.normalDiffString;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (this.showingCoopHelp)
		{
			IClickableMenu.drawTextureBox(b, base.xPositionOnScreen - 192, base.yPositionOnScreen + 64, (int)this.helpStringSize.X + IClickableMenu.borderWidth * 2, (int)this.helpStringSize.Y + IClickableMenu.borderWidth * 2, Color.White);
			Utility.drawTextWithShadow(b, this.coopHelpString, Game1.dialogueFont, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth - 192, base.yPositionOnScreen + IClickableMenu.borderWidth + 64), Game1.textColor);
			this.coopHelpOkButton?.draw(b, Color.White, 0.95f);
			this.coopHelpRightButton?.draw(b, Color.White, 0.95f);
			this.coopHelpLeftButton?.draw(b, Color.White, 0.95f);
			base.drawMouse(b);
			return;
		}
		Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
		if (this.source == Source.HostNewFarm)
		{
			IClickableMenu.drawTextureBox(b, base.xPositionOnScreen - 256 + 4 - ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 25 : 0), base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 68, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 320 : 256, 512, Color.White);
			foreach (ClickableTextureComponent c in this.cabinLayoutButtons)
			{
				c.draw(b, Color.White * ((Game1.startingCabins > 0) ? 1f : 0.5f), 0.9f);
				if (Game1.startingCabins > 0 && ((c.name.Equals("Close") && !Game1.cabinsSeparate) || (c.name.Equals("Separate") && Game1.cabinsSeparate)))
				{
					b.Draw(Game1.mouseCursors, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
				}
			}
		}
		b.Draw(Game1.daybg, new Vector2(this.portraitBox.X, this.portraitBox.Y), Color.White);
		foreach (ClickableTextureComponent c2 in this.genderButtons)
		{
			if (c2.visible)
			{
				c2.draw(b);
				if ((c2.name.Equals("Male") && Game1.player.IsMale) || (c2.name.Equals("Female") && !Game1.player.IsMale))
				{
					b.Draw(Game1.mouseCursors, c2.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
				}
			}
		}
		if (this.nameBoxCC.visible)
		{
			Game1.player.Name = this.nameBox.Text;
		}
		if (this.favThingBoxCC.visible)
		{
			Game1.player.favoriteThing.Value = this.favThingBox.Text;
		}
		if (this.farmnameBoxCC.visible)
		{
			Game1.player.farmName.Value = this.farmnameBox.Text;
		}
		if (this.source == Source.NewFarmhand)
		{
			Game1.player.farmName.Value = Game1.MasterPlayer.farmName.Value;
		}
		foreach (ClickableTextureComponent leftSelectionButton in this.leftSelectionButtons)
		{
			leftSelectionButton.draw(b);
		}
		foreach (ClickableComponent c3 in this.labels)
		{
			if (!c3.visible)
			{
				continue;
			}
			string sub = "";
			float offset = 0f;
			float subYOffset = 0f;
			Color color = Game1.textColor;
			if (c3 == this.nameLabel)
			{
				string name = Game1.player.Name;
				color = ((name != null && name.Length < 1) ? Color.Red : Game1.textColor);
			}
			else if (c3 == this.farmLabel)
			{
				color = ((Game1.player.farmName.Value != null && Game1.player.farmName.Length < 1) ? Color.Red : Game1.textColor);
			}
			else if (c3 == this.favoriteLabel)
			{
				color = ((Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Length < 1) ? Color.Red : Game1.textColor);
			}
			else if (c3 == this.shirtLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				sub = Game1.player.GetShirtIndex().ToString();
				if (int.TryParse(sub, out var id))
				{
					sub = (id + 1).ToString();
				}
			}
			else if (c3 == this.skinLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				sub = (Game1.player.skin.Value + 1).ToString() ?? "";
			}
			else if (c3 == this.hairLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				if (!c3.name.Contains("Color"))
				{
					sub = (Farmer.GetAllHairstyleIndices().IndexOf(Game1.player.hair.Value) + 1).ToString() ?? "";
				}
			}
			else if (c3 == this.accLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				sub = (Game1.player.accessory.Value + 2).ToString() ?? "";
			}
			else if (c3 == this.pantsStyleLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				sub = Game1.player.GetPantsIndex().ToString();
				if (int.TryParse(sub, out var id2))
				{
					sub = (id2 + 1).ToString();
				}
			}
			else if (c3 == this.startingCabinsLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				sub = ((Game1.startingCabins == 0 && this.noneString != null) ? this.noneString : (Game1.startingCabins.ToString() ?? ""));
				subYOffset = 4f;
			}
			else if (c3 == this.difficultyModifierLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				subYOffset = 4f;
				sub = this.getNameOfDifficulty();
			}
			else if (c3 == this.separateWalletLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
				subYOffset = 4f;
				sub = (Game1.player.team.useSeparateWallets.Value ? this.separateWalletString : this.sharedWalletString);
			}
			else
			{
				color = Game1.textColor;
			}
			Utility.drawTextWithShadow(b, c3.name, Game1.smallFont, new Vector2((float)c3.bounds.X + offset, c3.bounds.Y), color);
			if (sub.Length > 0)
			{
				Utility.drawTextWithShadow(b, sub, Game1.smallFont, new Vector2((float)(c3.bounds.X + 21) - Game1.smallFont.MeasureString(sub).X / 2f, (float)(c3.bounds.Y + 32) + subYOffset), color);
			}
		}
		foreach (ClickableTextureComponent rightSelectionButton in this.rightSelectionButtons)
		{
			rightSelectionButton.draw(b);
		}
		if (this.farmTypeButtons.Count > 0)
		{
			IClickableMenu.drawTextureBox(b, this.farmTypeButtons[0].bounds.X - 16, this.farmTypeButtons[0].bounds.Y - 20, 220, 564, Color.White);
			for (int i = 0; i < this.farmTypeButtons.Count; i++)
			{
				this.farmTypeButtons[i].draw(b, this.farmTypeButtons[i].name.Contains("Gray") ? (Color.Black * 0.5f) : Color.White, 0.88f);
				if (this.farmTypeButtons[i].name.Contains("Gray"))
				{
					b.Draw(Game1.mouseCursors, new Vector2(this.farmTypeButtons[i].bounds.Center.X - 12, this.farmTypeButtons[i].bounds.Center.Y - 8), new Rectangle(107, 442, 7, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				}
				bool farm_is_selected = false;
				int index = i + this._currentFarmPage * 6;
				if (Game1.whichFarm == 7)
				{
					if ("ModFarm_" + Game1.whichModFarm.Id == this.farmTypeButtonNames[index])
					{
						farm_is_selected = true;
					}
				}
				else if (Game1.whichFarm == index)
				{
					farm_is_selected = true;
				}
				if (farm_is_selected)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.farmTypeButtons[i].bounds.X, this.farmTypeButtons[i].bounds.Y - 4, this.farmTypeButtons[i].bounds.Width, this.farmTypeButtons[i].bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
			}
			this.farmTypeNextPageButton?.draw(b);
			this.farmTypePreviousPageButton?.draw(b);
		}
		if (this.petPortraitBox.HasValue && Pet.TryGetData(Game1.MasterPlayer.whichPetType, out var petData))
		{
			Texture2D texture = null;
			Rectangle sourceRect = Rectangle.Empty;
			foreach (PetBreed breed in petData.Breeds)
			{
				if (breed.Id == Game1.MasterPlayer.whichPetBreed)
				{
					texture = Game1.content.Load<Texture2D>(breed.IconTexture);
					sourceRect = breed.IconSourceRect;
					break;
				}
			}
			if (texture != null)
			{
				b.Draw(texture, this.petPortraitBox.Value, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.89f);
			}
		}
		this.advancedOptionsButton?.draw(b);
		if (this.canLeaveMenu())
		{
			this.okButton.draw(b, Color.White, 0.75f);
		}
		else
		{
			this.okButton.draw(b, Color.White, 0.75f);
			this.okButton.draw(b, Color.Black * 0.5f, 0.751f);
		}
		this.coopHelpButton?.draw(b, Color.White, 0.75f);
		this.hairColorPicker?.draw(b);
		this.pantsColorPicker?.draw(b);
		this.eyeColorPicker?.draw(b);
		if (this.source != Source.Dresser && this.source != Source.DyePots && this.source != Source.ClothesDye)
		{
			this.nameBox.Draw(b);
			this.favThingBox.Draw(b);
		}
		if (this.farmnameBoxCC.visible)
		{
			this.farmnameBox.Draw(b);
			Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_FarmNameSuffix"), Game1.smallFont, new Vector2(this.farmnameBox.X + this.farmnameBox.Width + 8, this.farmnameBox.Y + 12), Game1.textColor);
		}
		if (this.skipIntroButton != null && this.skipIntroButton.visible)
		{
			this.skipIntroButton.draw(b);
			Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.smallFont, new Vector2(this.skipIntroButton.bounds.X + this.skipIntroButton.bounds.Width + 8, this.skipIntroButton.bounds.Y + 8), Game1.textColor);
		}
		if (this.advancedCCHighlightTimer > 0f)
		{
			b.Draw(Game1.mouseCursors, this.advancedOptionsButton.getVector2() + new Vector2(4f, 84f), new Rectangle(128 + ((this.advancedCCHighlightTimer % 500f < 250f) ? 16 : 0), 208, 16, 16), Color.White * Math.Min(1f, this.advancedCCHighlightTimer / 500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
		}
		this.randomButton.draw(b);
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		this._displayFarmer.FarmerRenderer.draw(b, this._displayFarmer.FarmerSprite.CurrentAnimationFrame, this._displayFarmer.FarmerSprite.CurrentFrame, this._displayFarmer.FarmerSprite.SourceRect, new Vector2(this.portraitBox.Center.X - 32, this.portraitBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, this._displayFarmer);
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		string text = this.hoverTitle;
		if (text != null && text.Length > 0)
		{
			int width = Math.Max((int)Game1.dialogueFont.MeasureString(this.hoverTitle).X, 256);
			IClickableMenu.drawHoverText(b, Game1.parseText(this.hoverText, Game1.smallFont, width), Game1.smallFont, 0, 0, -1, this.hoverTitle);
		}
		base.drawMouse(b);
	}

	public override void emergencyShutDown()
	{
		if (this._itemToDye != null)
		{
			if (!Game1.player.IsEquippedItem(this._itemToDye))
			{
				Utility.CollectOrDrop(this._itemToDye);
			}
			this._itemToDye = null;
		}
		base.emergencyShutDown();
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (a.region != b.region)
		{
			return false;
		}
		if (this.advancedOptionsButton != null && this.backButton != null && a == this.advancedOptionsButton && b == this.backButton)
		{
			return false;
		}
		if (this.source == Source.Wizard)
		{
			if (a == this.favThingBoxCC && b.myID >= 522 && b.myID <= 530)
			{
				return false;
			}
			if (b == this.favThingBoxCC && a.myID >= 522 && a.myID <= 530)
			{
				return false;
			}
		}
		if (this.source == Source.Wizard)
		{
			if (a.name == "Direction" && b.name == "Pet")
			{
				return false;
			}
			if (b.name == "Direction" && a.name == "Pet")
			{
				return false;
			}
		}
		if (this.randomButton != null)
		{
			switch (direction)
			{
			case 3:
				if (b == this.randomButton && a.name == "Direction")
				{
					return false;
				}
				break;
			default:
				if (a == this.randomButton && b.name != "Direction")
				{
					return false;
				}
				if (b == this.randomButton && a.name != "Direction")
				{
					return false;
				}
				break;
			case 0:
				break;
			}
			if (a.myID == 622 && direction == 1 && (b == this.nameBoxCC || b == this.favThingBoxCC || b == this.farmnameBoxCC))
			{
				return false;
			}
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.showingCoopHelp)
		{
			this.backButton.visible = false;
			switch (this.coopHelpScreen)
			{
			case 0:
				this.coopHelpRightButton.visible = true;
				this.coopHelpLeftButton.visible = false;
				break;
			case 1:
				this.coopHelpRightButton.visible = false;
				this.coopHelpLeftButton.visible = true;
				break;
			}
		}
		else
		{
			this.backButton.visible = this._shouldShowBackButton;
		}
		if (this._sliderOpTarget != null)
		{
			Color col = this._sliderOpTarget.getSelectedColor();
			if (this._sliderOpTarget.Dirty && this._sliderOpTarget.LastColor == col)
			{
				this._sliderAction();
				this._sliderOpTarget.LastColor = this._sliderOpTarget.getSelectedColor();
				this._sliderOpTarget.Dirty = false;
				this._sliderOpTarget = null;
			}
			else
			{
				this._sliderOpTarget.LastColor = col;
			}
		}
		if (this.advancedCCHighlightTimer > 0f)
		{
			this.advancedCCHighlightTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
	}
}
