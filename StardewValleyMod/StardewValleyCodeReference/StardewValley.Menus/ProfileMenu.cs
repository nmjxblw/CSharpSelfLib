using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Menus;

public class ProfileMenu : IClickableMenu
{
	public class ProfileItemCategory
	{
		public string categoryName;

		public int[] validCategories;

		public ProfileItemCategory(string name, int[] valid_categories)
		{
			this.categoryName = name;
			this.validCategories = valid_categories;
		}
	}

	public const int region_characterSelectors = 500;

	public const int region_categorySelector = 501;

	public const int region_itemButtons = 502;

	public const int region_backButton = 101;

	public const int region_forwardButton = 102;

	public const int region_upArrow = 105;

	public const int region_downArrow = 106;

	public const int letterWidth = 320;

	public const int letterHeight = 180;

	public Texture2D letterTexture;

	protected string hoverText = "";

	protected List<ProfileItem> _profileItems;

	public Item hoveredItem;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public ClickableTextureComponent nextCharacterButton;

	public ClickableTextureComponent previousCharacterButton;

	protected Rectangle characterSpriteBox;

	protected int _currentCategory;

	protected AnimatedSprite _animatedSprite;

	protected float _directionChangeTimer;

	protected float _hiddenEmoteTimer = -1f;

	protected int _currentDirection;

	protected int _hideTooltipTime;

	protected SocialPage _socialPage;

	protected string _status = "";

	protected string _printedName = "";

	protected Vector2 _characterEntrancePosition = new Vector2(0f, 0f);

	public ClickableTextureComponent upArrow;

	public ClickableTextureComponent downArrow;

	protected ClickableTextureComponent scrollBar;

	protected Rectangle scrollBarRunner;

	public List<ClickableComponent> clickableProfileItems;

	/// <summary>The current character being shown in the menu.</summary>
	public SocialPage.SocialEntry Current;

	/// <summary>The social entries for characters that can be viewed in the profile menu.</summary>
	public readonly List<SocialPage.SocialEntry> SocialEntries = new List<SocialPage.SocialEntry>();

	protected Vector2 _characterNamePosition;

	protected Vector2 _heartDisplayPosition;

	protected Vector2 _birthdayHeadingDisplayPosition;

	protected Vector2 _birthdayDisplayPosition;

	protected Vector2 _statusHeadingDisplayPosition;

	protected Vector2 _statusDisplayPosition;

	protected Vector2 _giftLogHeadingDisplayPosition;

	protected Vector2 _giftLogCategoryDisplayPosition;

	protected Vector2 _errorMessagePosition;

	protected Vector2 _characterSpriteDrawPosition;

	protected Rectangle _characterStatusDisplayBox;

	protected List<ClickableTextureComponent> _clickableTextureComponents;

	public Rectangle _itemDisplayRect;

	protected int scrollPosition;

	protected int scrollStep = 36;

	protected int scrollSize;

	public static ProfileItemCategory[] itemCategories = new ProfileItemCategory[10]
	{
		new ProfileItemCategory("Profile_Gift_Category_LikedGifts", null),
		new ProfileItemCategory("Profile_Gift_Category_FruitsAndVegetables", new int[2] { -75, -79 }),
		new ProfileItemCategory("Profile_Gift_Category_AnimalProduce", new int[4] { -6, -5, -14, -18 }),
		new ProfileItemCategory("Profile_Gift_Category_ArtisanItems", new int[1] { -26 }),
		new ProfileItemCategory("Profile_Gift_Category_CookedItems", new int[1] { -7 }),
		new ProfileItemCategory("Profile_Gift_Category_ForagedItems", new int[4] { -80, -81, -23, -17 }),
		new ProfileItemCategory("Profile_Gift_Category_Fish", new int[1] { -4 }),
		new ProfileItemCategory("Profile_Gift_Category_Ingredients", new int[2] { -27, -25 }),
		new ProfileItemCategory("Profile_Gift_Category_MineralsAndGems", new int[3] { -15, -12, -2 }),
		new ProfileItemCategory("Profile_Gift_Category_Misc", null)
	};

	protected Dictionary<int, List<Item>> _sortedItems;

	public bool scrolling;

	private int _characterSpriteRandomInt;

	public ProfileMenu(SocialPage.SocialEntry subject, List<SocialPage.SocialEntry> allSocialEntries)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		this._printedName = "";
		this._characterEntrancePosition = new Vector2(0f, 4f);
		foreach (SocialPage.SocialEntry entry in allSocialEntries)
		{
			if (entry.Character is NPC && entry.IsMet)
			{
				this.SocialEntries.Add(entry);
			}
		}
		this._profileItems = new List<ProfileItem>();
		this.clickableProfileItems = new List<ClickableComponent>();
		this.UpdateButtons();
		this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		this._SetCharacter(subject);
	}

	protected void _SetCharacter(SocialPage.SocialEntry entry)
	{
		this.Current = entry;
		this._sortedItems = new Dictionary<int, List<Item>>();
		if (this.Current.Character is NPC npcData)
		{
			CharacterData data = npcData.GetData();
			string assetName = "Characters/" + npcData.getTextureName();
			try
			{
				this._animatedSprite = new AnimatedSprite(assetName, 0, data?.Size.X ?? 16, data?.Size.Y ?? 32);
			}
			catch (Exception exception)
			{
				Game1.log.Error($"Profile menu couldn't load sprite '{assetName}' for NPC '{npcData.Name}', defaulting to their current sprite.", exception);
				this._animatedSprite = npcData.Sprite.Clone();
				this._animatedSprite.tempSpriteHeight = -1;
				this._animatedSprite.SpriteWidth = data?.Size.X ?? this._animatedSprite.SpriteWidth;
				this._animatedSprite.SpriteHeight = data?.Size.Y ?? this._animatedSprite.SpriteHeight;
			}
			this._animatedSprite.faceDirection(2);
			foreach (ParsedItemData itemData in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (!Game1.player.hasGiftTasteBeenRevealed(npcData, itemData.ItemId))
				{
					continue;
				}
				Object item = ItemRegistry.Create<Object>(itemData.QualifiedItemId);
				if (item.IsBreakableStone())
				{
					continue;
				}
				for (int i = 0; i < ProfileMenu.itemCategories.Length; i++)
				{
					string categoryName = ProfileMenu.itemCategories[i].categoryName;
					if (!(categoryName == "Profile_Gift_Category_LikedGifts"))
					{
						if (categoryName == "Profile_Gift_Category_Misc")
						{
							bool isAccountedFor = false;
							ProfileItemCategory[] array = ProfileMenu.itemCategories;
							foreach (ProfileItemCategory category in array)
							{
								if (category.validCategories != null && category.validCategories.Contains(item.Category))
								{
									isAccountedFor = true;
									break;
								}
							}
							if (!isAccountedFor)
							{
								if (!this._sortedItems.TryGetValue(i, out var categoryItems))
								{
									categoryItems = (this._sortedItems[i] = new List<Item>());
								}
								categoryItems.Add(item);
							}
						}
						else if (ProfileMenu.itemCategories[i].validCategories.Contains(item.Category))
						{
							if (!this._sortedItems.TryGetValue(i, out var categoryItems2))
							{
								categoryItems2 = (this._sortedItems[i] = new List<Item>());
							}
							categoryItems2.Add(item);
						}
						continue;
					}
					int giftTaste = npcData.getGiftTasteForThisItem(item);
					if (giftTaste == 2 || giftTaste == 0)
					{
						if (!this._sortedItems.TryGetValue(i, out var categoryItems3))
						{
							categoryItems3 = (this._sortedItems[i] = new List<Item>());
						}
						categoryItems3.Add(item);
					}
				}
			}
			Gender gender = this.Current.Gender;
			bool isDatable = this.Current.IsDatable;
			bool housemate = this.Current.IsRoommateForCurrentPlayer();
			this._status = "";
			if (isDatable || housemate)
			{
				string text = (housemate ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Male")) : (this.Current.IsMarriedToCurrentPlayer() ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Wife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Husband")) : (this.Current.IsMarriedToAnyone() ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_FemaleNpc") : Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_MaleNpc")) : ((!Game1.player.isMarriedOrRoommates() && this.Current.IsDatingCurrentPlayer()) ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Girlfriend") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Boyfriend")) : ((!this.Current.IsDivorcedFromCurrentPlayer()) ? ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Male")) : ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExWife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExHusband")))))));
				text = Game1.parseText(text, Game1.smallFont, base.width);
				string status = text.Replace("(", "").Replace(")", "").Replace("（", "")
					.Replace("）", "");
				status = Utility.capitalizeFirstLetter(status);
				this._status = status;
			}
			this._UpdateList();
		}
		this._directionChangeTimer = 2000f;
		this._currentDirection = 2;
		this._hiddenEmoteTimer = -1f;
	}

	public void ChangeCharacter(int offset)
	{
		int index = this.SocialEntries.IndexOf(this.Current);
		if (index == -1)
		{
			if (this.SocialEntries.Count > 0)
			{
				this._SetCharacter(this.SocialEntries[0]);
			}
			return;
		}
		for (index += offset; index < 0; index += this.SocialEntries.Count)
		{
		}
		while (index >= this.SocialEntries.Count)
		{
			index -= this.SocialEntries.Count;
		}
		this._SetCharacter(this.SocialEntries[index]);
		Game1.playSound("smallSelect");
		this._printedName = "";
		this._characterEntrancePosition = new Vector2(Math.Sign(offset) * -4, 0f);
		if (Game1.options.SnappyMenus && (base.currentlySnappedComponent == null || !base.currentlySnappedComponent.visible))
		{
			this.snapToDefaultClickableComponent();
		}
	}

	protected void _UpdateList()
	{
		for (int i = 0; i < this._profileItems.Count; i++)
		{
			this._profileItems[i].Unload();
		}
		this._profileItems.Clear();
		if (!(this.Current.Character is NPC npc))
		{
			return;
		}
		List<Item> loved_items = new List<Item>();
		List<Item> liked_items = new List<Item>();
		List<Item> neutral_items = new List<Item>();
		List<Item> disliked_items = new List<Item>();
		List<Item> hated_items = new List<Item>();
		if (this._sortedItems.TryGetValue(this._currentCategory, out var categoryItems))
		{
			foreach (Item item in categoryItems)
			{
				switch (npc.getGiftTasteForThisItem(item))
				{
				case 0:
					loved_items.Add(item);
					break;
				case 2:
					liked_items.Add(item);
					break;
				case 8:
					neutral_items.Add(item);
					break;
				case 4:
					disliked_items.Add(item);
					break;
				case 6:
					hated_items.Add(item);
					break;
				}
			}
		}
		PI_ItemList item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Loved"), loved_items);
		this._profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Liked"), liked_items);
		this._profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Neutral"), neutral_items);
		this._profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Disliked"), disliked_items);
		this._profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Hated"), hated_items);
		this._profileItems.Add(item_display);
		this.SetupLayout();
		this.populateClickableComponentList();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls && (base.currentlySnappedComponent == null || !base.allClickableComponents.Contains(base.currentlySnappedComponent)))
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (direction == 2 && a.region == 501 && b.region == 500)
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.clickableProfileItems.Count > 0)
		{
			base.currentlySnappedComponent = this.clickableProfileItems[0];
		}
		else
		{
			base.currentlySnappedComponent = this.backButton;
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	public void UpdateButtons()
	{
		this._clickableTextureComponents = new List<ClickableTextureComponent>();
		this.upArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
		{
			myID = 105,
			upNeighborID = 102,
			upNeighborImmutable = true,
			downNeighborID = 106,
			downNeighborImmutable = true,
			leftNeighborID = -99998,
			leftNeighborImmutable = true
		};
		this.downArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
		{
			myID = 106,
			upNeighborID = 105,
			upNeighborImmutable = true,
			leftNeighborID = -99998,
			leftNeighborImmutable = true
		};
		this.scrollBar = new ClickableTextureComponent(new Rectangle(0, 0, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 32, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			name = "Back Button",
			upNeighborID = -99998,
			downNeighborID = -99998,
			downNeighborImmutable = true,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 501
		};
		this._clickableTextureComponents.Add(this.backButton);
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			name = "Forward Button",
			upNeighborID = -99998,
			downNeighborID = -99998,
			downNeighborImmutable = true,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 501
		};
		this._clickableTextureComponents.Add(this.forwardButton);
		this.previousCharacterButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 32, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 0,
			name = "Previous Char",
			upNeighborID = -99998,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 500
		};
		this._clickableTextureComponents.Add(this.previousCharacterButton);
		this.nextCharacterButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 0,
			name = "Next Char",
			upNeighborID = -99998,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 500
		};
		this._clickableTextureComponents.Add(this.nextCharacterButton);
		this._clickableTextureComponents.Add(this.upArrow);
		this._clickableTextureComponents.Add(this.downArrow);
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		if (direction > 0)
		{
			this.Scroll(-this.scrollStep);
		}
		else if (direction < 0)
		{
			this.Scroll(this.scrollStep);
		}
	}

	public void ChangePage(int offset)
	{
		this.scrollPosition = 0;
		this._currentCategory += offset;
		while (this._currentCategory < 0)
		{
			this._currentCategory += ProfileMenu.itemCategories.Length;
		}
		while (this._currentCategory >= ProfileMenu.itemCategories.Length)
		{
			this._currentCategory -= ProfileMenu.itemCategories.Length;
		}
		Game1.playSound("shwip");
		this._UpdateList();
		if (Game1.options.SnappyMenus && (base.currentlySnappedComponent == null || !base.currentlySnappedComponent.visible))
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X;
		base.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y;
		this.UpdateButtons();
		this.SetupLayout();
		base.initializeUpperRightCloseButton();
		this.populateClickableComponentList();
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		switch (button)
		{
		case Buttons.LeftTrigger:
			this.ChangePage(-1);
			break;
		case Buttons.RightTrigger:
			this.ChangePage(1);
			break;
		case Buttons.RightShoulder:
			this.ChangeCharacter(1);
			break;
		case Buttons.LeftShoulder:
			this.ChangeCharacter(-1);
			break;
		case Buttons.Back:
			this.PlayHiddenEmote();
			break;
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (key != Keys.None)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
			{
				base.exitThisMenu();
			}
			else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !this.overrideSnappyMenuCursorMovementBan())
			{
				base.applyMovementKey(key);
			}
		}
	}

	public override void applyMovementKey(int direction)
	{
		base.applyMovementKey(direction);
		this.ConstrainSelectionToView();
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		this.scrolling = false;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.scrollBar.containsPoint(x, y))
		{
			this.scrolling = true;
		}
		else if (this.scrollBarRunner.Contains(x, y))
		{
			this.scrolling = true;
			this.leftClickHeld(x, y);
			this.releaseLeftClick(x, y);
		}
		if (base.upperRightCloseButton != null && this.readyToClose() && base.upperRightCloseButton.containsPoint(x, y))
		{
			base.exitThisMenu();
		}
		else
		{
			if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
			{
				return;
			}
			if (this.backButton.containsPoint(x, y))
			{
				this.ChangePage(-1);
				return;
			}
			if (this.forwardButton.containsPoint(x, y))
			{
				this.ChangePage(1);
				return;
			}
			if (this.previousCharacterButton.containsPoint(x, y))
			{
				this.ChangeCharacter(-1);
				return;
			}
			if (this.nextCharacterButton.containsPoint(x, y))
			{
				this.ChangeCharacter(1);
				return;
			}
			if (this.downArrow.containsPoint(x, y))
			{
				this.Scroll(this.scrollStep);
			}
			if (this.upArrow.containsPoint(x, y))
			{
				this.Scroll(-this.scrollStep);
			}
			if (this.characterSpriteBox.Contains(x, y))
			{
				this.PlayHiddenEmote();
			}
		}
	}

	public void PlayHiddenEmote()
	{
		if (this.Current.HeartLevel >= 4)
		{
			this._currentDirection = 2;
			this._characterSpriteRandomInt = Game1.random.Next(4);
			CharacterData data = this.Current.Data;
			Game1.playSound(data?.HiddenProfileEmoteSound ?? "drumkit6");
			this._hiddenEmoteTimer = ((data != null && data.HiddenProfileEmoteDuration >= 0) ? ((float)data.HiddenProfileEmoteDuration) : 4000f);
		}
		else
		{
			this._currentDirection = 2;
			this._directionChangeTimer = 5000f;
			Game1.playSound("Cowboy_Footstep");
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.hoveredItem = null;
		if (this._itemDisplayRect.Contains(x, y))
		{
			foreach (ProfileItem profileItem in this._profileItems)
			{
				profileItem.performHover(x, y);
			}
		}
		this.upArrow.tryHover(x, y);
		this.downArrow.tryHover(x, y);
		this.backButton.tryHover(x, y, 0.6f);
		this.forwardButton.tryHover(x, y, 0.6f);
		this.nextCharacterButton.tryHover(x, y, 0.6f);
		this.previousCharacterButton.tryHover(x, y, 0.6f);
	}

	public void ConstrainSelectionToView()
	{
		if (!Game1.options.snappyMenus)
		{
			return;
		}
		ClickableComponent clickableComponent = base.currentlySnappedComponent;
		if (clickableComponent != null && clickableComponent.region == 502 && !this._itemDisplayRect.Contains(base.currentlySnappedComponent.bounds))
		{
			if (base.currentlySnappedComponent.bounds.Bottom > this._itemDisplayRect.Bottom)
			{
				int scroll = (int)Math.Ceiling(((double)base.currentlySnappedComponent.bounds.Bottom - (double)this._itemDisplayRect.Bottom) / (double)this.scrollStep) * this.scrollStep;
				this.Scroll(scroll);
			}
			else if (base.currentlySnappedComponent.bounds.Top < this._itemDisplayRect.Top)
			{
				int scroll2 = (int)Math.Floor(((double)base.currentlySnappedComponent.bounds.Top - (double)this._itemDisplayRect.Top) / (double)this.scrollStep) * this.scrollStep;
				this.Scroll(scroll2);
			}
		}
		if (this.scrollPosition <= this.scrollStep)
		{
			this.scrollPosition = 0;
			this.UpdateScroll();
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.Current.DisplayName != null && this._printedName.Length < this.Current.DisplayName.Length)
		{
			this._printedName += this.Current.DisplayName[this._printedName.Length];
		}
		if (this._hideTooltipTime > 0)
		{
			this._hideTooltipTime -= time.ElapsedGameTime.Milliseconds;
			if (this._hideTooltipTime < 0)
			{
				this._hideTooltipTime = 0;
			}
		}
		if (this._characterEntrancePosition.X != 0f)
		{
			this._characterEntrancePosition.X -= (float)Math.Sign(this._characterEntrancePosition.X) * 0.25f;
		}
		if (this._characterEntrancePosition.Y != 0f)
		{
			this._characterEntrancePosition.Y -= (float)Math.Sign(this._characterEntrancePosition.Y) * 0.25f;
		}
		if (this._animatedSprite == null)
		{
			return;
		}
		if (this._hiddenEmoteTimer > 0f)
		{
			this._hiddenEmoteTimer -= time.ElapsedGameTime.Milliseconds;
			if (this._hiddenEmoteTimer <= 0f)
			{
				this._hiddenEmoteTimer = -1f;
				this._currentDirection = 2;
				this._directionChangeTimer = 2000f;
				if (this.Current.InternalName == "Leo")
				{
					this.Current.Character.Sprite.AnimateDown(time);
				}
			}
		}
		else if (this._directionChangeTimer > 0f)
		{
			this._directionChangeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this._directionChangeTimer <= 0f)
			{
				this._directionChangeTimer = 2000f;
				this._currentDirection = (this._currentDirection + 1) % 4;
			}
		}
		if (this._characterEntrancePosition != Vector2.Zero)
		{
			if (this._characterEntrancePosition.X < 0f)
			{
				this._animatedSprite.AnimateRight(time, 2);
			}
			else if (this._characterEntrancePosition.X > 0f)
			{
				this._animatedSprite.AnimateLeft(time, 2);
			}
			else if (this._characterEntrancePosition.Y > 0f)
			{
				this._animatedSprite.AnimateUp(time, 2);
			}
			else if (this._characterEntrancePosition.Y < 0f)
			{
				this._animatedSprite.AnimateDown(time, 2);
			}
			return;
		}
		if (this._hiddenEmoteTimer > 0f)
		{
			CharacterData data = this.Current.Data;
			if (data != null && data.HiddenProfileEmoteStartFrame >= 0)
			{
				int startFrame = ((this.Current.InternalName == "Emily" && data.HiddenProfileEmoteStartFrame == 16) ? (data.HiddenProfileEmoteStartFrame + this._characterSpriteRandomInt * 2) : data.HiddenProfileEmoteStartFrame);
				this._animatedSprite.Animate(time, startFrame, data.HiddenProfileEmoteFrameCount, data.HiddenProfileEmoteFrameDuration);
			}
			else
			{
				this._animatedSprite.AnimateDown(time, 2);
			}
			return;
		}
		switch (this._currentDirection)
		{
		case 0:
			this._animatedSprite.AnimateUp(time, 2);
			break;
		case 2:
			this._animatedSprite.AnimateDown(time, 2);
			break;
		case 3:
			this._animatedSprite.AnimateLeft(time, 2);
			break;
		case 1:
			this._animatedSprite.AnimateRight(time, 2);
			break;
		}
	}

	public void SetupLayout()
	{
		int x = base.xPositionOnScreen + 64 - 12;
		int y = base.yPositionOnScreen + IClickableMenu.borderWidth;
		Rectangle left_pane_rectangle = new Rectangle(x, y, 400, 720 - IClickableMenu.borderWidth * 2);
		Rectangle content_rectangle = new Rectangle(x, y, 1204, 720 - IClickableMenu.borderWidth * 2);
		content_rectangle.X += left_pane_rectangle.Width;
		content_rectangle.Width -= left_pane_rectangle.Width;
		this._characterStatusDisplayBox = new Rectangle(left_pane_rectangle.X, left_pane_rectangle.Y, left_pane_rectangle.Width, left_pane_rectangle.Height);
		left_pane_rectangle.Y += 32;
		left_pane_rectangle.Height -= 32;
		this._characterSpriteDrawPosition = new Vector2(left_pane_rectangle.X + (left_pane_rectangle.Width - Game1.nightbg.Width) / 2, left_pane_rectangle.Y);
		this.characterSpriteBox = new Rectangle(base.xPositionOnScreen + 64 - 12 + (400 - Game1.nightbg.Width) / 2, base.yPositionOnScreen + IClickableMenu.borderWidth, Game1.nightbg.Width, Game1.nightbg.Height);
		this.previousCharacterButton.bounds.X = (int)this._characterSpriteDrawPosition.X - 64 - this.previousCharacterButton.bounds.Width / 2;
		this.previousCharacterButton.bounds.Y = (int)this._characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - this.previousCharacterButton.bounds.Height / 2;
		this.nextCharacterButton.bounds.X = (int)this._characterSpriteDrawPosition.X + Game1.nightbg.Width + 64 - this.nextCharacterButton.bounds.Width / 2;
		this.nextCharacterButton.bounds.Y = (int)this._characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - this.nextCharacterButton.bounds.Height / 2;
		left_pane_rectangle.Y += Game1.daybg.Height + 32;
		left_pane_rectangle.Height -= Game1.daybg.Height + 32;
		this._characterNamePosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
		left_pane_rectangle.Y += 96;
		left_pane_rectangle.Height -= 96;
		this._heartDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
		if (this.Current.Character is NPC npc)
		{
			left_pane_rectangle.Y += 56;
			left_pane_rectangle.Height -= 48;
			this._birthdayHeadingDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
			if (npc.birthday_Season.Value != null && Utility.getSeasonNumber(npc.birthday_Season.Value) >= 0)
			{
				left_pane_rectangle.Y += 48;
				left_pane_rectangle.Height -= 48;
				this._birthdayDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
				left_pane_rectangle.Y += 64;
				left_pane_rectangle.Height -= 64;
			}
			if (this._status != "")
			{
				this._statusHeadingDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
				left_pane_rectangle.Y += 48;
				left_pane_rectangle.Height -= 48;
				this._statusDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
				left_pane_rectangle.Y += 64;
				left_pane_rectangle.Height -= 64;
			}
		}
		content_rectangle.Height -= 96;
		content_rectangle.Y -= 8;
		this._giftLogHeadingDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
		content_rectangle.Y += 80;
		content_rectangle.Height -= 70;
		this.backButton.bounds.X = content_rectangle.Left + 64 - this.forwardButton.bounds.Width / 2;
		this.backButton.bounds.Y = content_rectangle.Top;
		this.forwardButton.bounds.X = content_rectangle.Right - 64 - this.forwardButton.bounds.Width / 2;
		this.forwardButton.bounds.Y = content_rectangle.Top;
		content_rectangle.Width -= 250;
		content_rectangle.X += 125;
		this._giftLogCategoryDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
		content_rectangle.Y += 64;
		content_rectangle.Y += 32;
		content_rectangle.Height -= 32;
		this._itemDisplayRect = content_rectangle;
		int scroll_inset = 64;
		this.scrollBarRunner = new Rectangle(content_rectangle.Right + 48, content_rectangle.Top + scroll_inset, this.scrollBar.bounds.Width, content_rectangle.Height - scroll_inset * 2);
		this.downArrow.bounds.Y = this.scrollBarRunner.Bottom + 16;
		this.downArrow.bounds.X = this.scrollBarRunner.Center.X - this.downArrow.bounds.Width / 2;
		this.upArrow.bounds.Y = this.scrollBarRunner.Top - 16 - this.upArrow.bounds.Height;
		this.upArrow.bounds.X = this.scrollBarRunner.Center.X - this.upArrow.bounds.Width / 2;
		float draw_y = 0f;
		if (this._profileItems.Count > 0)
		{
			int drawn_index = 0;
			for (int i = 0; i < this._profileItems.Count; i++)
			{
				ProfileItem profile_item = this._profileItems[i];
				if (profile_item.ShouldDraw())
				{
					draw_y = profile_item.HandleLayout(draw_y, this._itemDisplayRect, drawn_index);
					drawn_index++;
				}
			}
		}
		this.scrollSize = (int)draw_y - this._itemDisplayRect.Height;
		if (this.NeedsScrollBar())
		{
			this.upArrow.visible = true;
			this.downArrow.visible = true;
		}
		else
		{
			this.upArrow.visible = false;
			this.downArrow.visible = false;
		}
		this.UpdateScroll();
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		if (GameMenu.forcePreventClose)
		{
			return;
		}
		base.leftClickHeld(x, y);
		if (this.scrolling)
		{
			int num = this.scrollPosition;
			this.scrollPosition = (int)Math.Round((float)(y - this.scrollBarRunner.Top) / (float)this.scrollBarRunner.Height * (float)this.scrollSize / (float)this.scrollStep) * this.scrollStep;
			this.UpdateScroll();
			if (num != this.scrollPosition)
			{
				Game1.playSound("shiny4");
			}
		}
	}

	public bool NeedsScrollBar()
	{
		return this.scrollSize > 0;
	}

	public void Scroll(int offset)
	{
		if (this.NeedsScrollBar())
		{
			int num = this.scrollPosition;
			this.scrollPosition += offset;
			this.UpdateScroll();
			if (num != this.scrollPosition)
			{
				Game1.playSound("shwip");
			}
		}
	}

	public virtual void UpdateScroll()
	{
		this.scrollPosition = Utility.Clamp(this.scrollPosition, 0, this.scrollSize);
		float draw_y = this._itemDisplayRect.Top - this.scrollPosition;
		this._errorMessagePosition = new Vector2(this._itemDisplayRect.Center.X, this._itemDisplayRect.Center.Y);
		if (this._profileItems.Count > 0)
		{
			int drawn_index = 0;
			for (int i = 0; i < this._profileItems.Count; i++)
			{
				ProfileItem profile_item = this._profileItems[i];
				if (profile_item.ShouldDraw())
				{
					draw_y = profile_item.HandleLayout(draw_y, this._itemDisplayRect, drawn_index);
					drawn_index++;
				}
			}
		}
		if (this.scrollSize > 0)
		{
			this.scrollBar.bounds.X = this.scrollBarRunner.Center.X - this.scrollBar.bounds.Width / 2;
			this.scrollBar.bounds.Y = (int)Utility.Lerp(this.scrollBarRunner.Top, this.scrollBarRunner.Bottom - this.scrollBar.bounds.Height, (float)this.scrollPosition / (float)this.scrollSize);
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
		}
		b.Draw(this.letterTexture, new Vector2(base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + base.height / 2), new Rectangle(0, 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f, SpriteEffects.None, 0.86f);
		Game1.DrawBox(this._characterStatusDisplayBox.X, this._characterStatusDisplayBox.Y, this._characterStatusDisplayBox.Width, this._characterStatusDisplayBox.Height);
		b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, this._characterSpriteDrawPosition, Color.White);
		Vector2 portraitPosition = new Vector2(this._characterSpriteDrawPosition.X + (float)((Game1.daybg.Width - this._animatedSprite.SpriteWidth * 4) / 2), this._characterSpriteDrawPosition.Y + 32f + (float)((32 - this._animatedSprite.SpriteHeight) * 4));
		NPC npc = this.Current.Character as NPC;
		if (npc != null)
		{
			this._animatedSprite.draw(b, portraitPosition, 0.8f);
			bool isCurrentSpouse = this.Current.IsMarriedToCurrentPlayer();
			int drawn_hearts = Math.Max(10, Utility.GetMaximumHeartsForCharacter(npc));
			float heart_draw_start_x = this._heartDisplayPosition.X - (float)(Math.Min(10, drawn_hearts) * 32 / 2);
			float heart_draw_offset_y = ((drawn_hearts > 10) ? (-16f) : 0f);
			for (int hearts = 0; hearts < drawn_hearts; hearts++)
			{
				this.drawNPCSlotHeart(b, heart_draw_start_x, heart_draw_offset_y, this.Current, hearts, this.Current.IsDatingCurrentPlayer(), isCurrentSpouse);
			}
		}
		if (this._printedName.Length < this.Current.DisplayName.Length)
		{
			SpriteText.drawStringWithScrollCenteredAt(b, "", (int)this._characterNamePosition.X, (int)this._characterNamePosition.Y, this._printedName);
		}
		else
		{
			SpriteText.drawStringWithScrollCenteredAt(b, this.Current.DisplayName, (int)this._characterNamePosition.X, (int)this._characterNamePosition.Y);
		}
		if (npc != null && npc.birthday_Season.Value != null)
		{
			int season_number = Utility.getSeasonNumber(npc.birthday_Season.Value);
			if (season_number >= 0)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Birthday"), (int)this._birthdayHeadingDisplayPosition.X, (int)this._birthdayHeadingDisplayPosition.Y);
				string birthday = Game1.content.LoadString("Strings\\UI:BirthdayOrder", npc.Birthday_Day, Utility.getSeasonNameFromNumber(season_number));
				b.DrawString(Game1.dialogueFont, birthday, new Vector2((0f - Game1.dialogueFont.MeasureString(birthday).X) / 2f + this._birthdayDisplayPosition.X, this._birthdayDisplayPosition.Y), Game1.textColor);
			}
			if (this._status != "")
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Status"), (int)this._statusHeadingDisplayPosition.X, (int)this._statusHeadingDisplayPosition.Y);
				b.DrawString(Game1.dialogueFont, this._status, new Vector2((0f - Game1.dialogueFont.MeasureString(this._status).X) / 2f + this._statusDisplayPosition.X, this._statusDisplayPosition.Y), Game1.textColor);
			}
		}
		SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_GiftLog"), (int)this._giftLogHeadingDisplayPosition.X, (int)this._giftLogHeadingDisplayPosition.Y);
		SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:" + ProfileMenu.itemCategories[this._currentCategory].categoryName, this.Current.DisplayName), (int)this._giftLogCategoryDisplayPosition.X, (int)this._giftLogCategoryDisplayPosition.Y);
		bool drew_items = false;
		b.End();
		Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = this._itemDisplayRect;
		if (this._profileItems.Count > 0)
		{
			for (int i = 0; i < this._profileItems.Count; i++)
			{
				ProfileItem profile_item = this._profileItems[i];
				if (profile_item.ShouldDraw())
				{
					drew_items = true;
					profile_item.Draw(b);
				}
			}
		}
		b.End();
		b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (this.NeedsScrollBar())
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
			this.scrollBar.draw(b);
		}
		if (!drew_items)
		{
			string error_string = Game1.content.LoadString("Strings\\UI:Profile_GiftLog_NoGiftsGiven");
			b.DrawString(Game1.smallFont, error_string, new Vector2((0f - Game1.smallFont.MeasureString(error_string).X) / 2f + this._errorMessagePosition.X, this._errorMessagePosition.Y), Game1.textColor);
		}
		foreach (ClickableTextureComponent clickableTextureComponent in this._clickableTextureComponents)
		{
			clickableTextureComponent.draw(b);
		}
		base.draw(b);
		base.drawMouse(b, ignore_transparency: true);
		if (this.hoveredItem == null)
		{
			return;
		}
		bool draw_tooltip = true;
		if (Game1.options.snappyMenus && Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse && this._hideTooltipTime > 0)
		{
			draw_tooltip = false;
		}
		if (draw_tooltip)
		{
			string name = this.hoveredItem.DisplayName;
			string description = this.hoveredItem.getDescription();
			if (description.Contains("{0}") || this.hoveredItem.ItemId == "DriedMushrooms")
			{
				name = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoveredItem.ItemId + "_CollectionsTabName") ?? name;
				description = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoveredItem.ItemId + "_CollectionsTabDescription") ?? description;
			}
			IClickableMenu.drawToolTip(b, description, name, this.hoveredItem);
		}
	}

	/// <summary>Draw the heart sprite for an NPC's entry in the social page.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="heartDrawStartX">The left X position at which to draw the first heart.</param>
	/// <param name="heartDrawStartY">The top Y position at which to draw hearts.</param>
	/// <param name="entry">The NPC's cached social data.</param>
	/// <param name="hearts">The current heart index being drawn (starting at 0 for the first heart).</param>
	/// <param name="isDating">Whether the player is currently dating this NPC.</param>
	/// <param name="isCurrentSpouse">Whether the player is currently married to this NPC.</param>
	private void drawNPCSlotHeart(SpriteBatch b, float heartDrawStartX, float heartDrawStartY, SocialPage.SocialEntry entry, int hearts, bool isDating, bool isCurrentSpouse)
	{
		bool isLockedHeart = entry.IsDatable && !isDating && !isCurrentSpouse && hearts >= 8;
		int heartX = ((hearts < entry.HeartLevel || isLockedHeart) ? 211 : 218);
		Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);
		if (hearts < 10)
		{
			b.Draw(Game1.mouseCursors, new Vector2(heartDrawStartX + (float)(hearts * 32), this._heartDisplayPosition.Y + heartDrawStartY), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, new Vector2(heartDrawStartX + (float)((hearts - 10) * 32), this._heartDisplayPosition.Y + heartDrawStartY + 32f), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.receiveLeftClick(x, y, playSound);
	}

	public void RegisterClickable(ClickableComponent clickable)
	{
		this.clickableProfileItems.Add(clickable);
	}

	public void UnregisterClickable(ClickableComponent clickable)
	{
		this.clickableProfileItems.Remove(clickable);
	}
}
