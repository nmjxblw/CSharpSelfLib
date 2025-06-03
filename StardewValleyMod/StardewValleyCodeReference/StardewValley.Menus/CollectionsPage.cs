using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace StardewValley.Menus;

public class CollectionsPage : IClickableMenu
{
	public const int region_sideTabShipped = 7001;

	public const int region_sideTabFish = 7002;

	public const int region_sideTabArtifacts = 7003;

	public const int region_sideTabMinerals = 7004;

	public const int region_sideTabCooking = 7005;

	public const int region_sideTabAchivements = 7006;

	public const int region_sideTabSecretNotes = 7007;

	public const int region_sideTabLetters = 7008;

	public const int region_forwardButton = 707;

	public const int region_backButton = 706;

	public static int widthToMoveActiveTab = 8;

	public const int organicsTab = 0;

	public const int fishTab = 1;

	public const int archaeologyTab = 2;

	public const int mineralsTab = 3;

	public const int cookingTab = 4;

	public const int achievementsTab = 5;

	public const int secretNotesTab = 6;

	public const int lettersTab = 7;

	public const int distanceFromMenuBottomBeforeNewPage = 128;

	private string hoverText = "";

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public Dictionary<int, ClickableTextureComponent> sideTabs = new Dictionary<int, ClickableTextureComponent>();

	public int currentTab;

	public int currentPage;

	public int secretNoteImage = -1;

	public Dictionary<int, List<List<ClickableTextureComponent>>> collections = new Dictionary<int, List<List<ClickableTextureComponent>>>();

	public Dictionary<int, string> secretNotesData;

	public Texture2D secretNoteImageTexture;

	public LetterViewerMenu letterviewerSubMenu;

	private Item hoverItem;

	private CraftingRecipe hoverCraftingRecipe;

	private int value;

	public CollectionsPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		this.sideTabs.Add(0, new ClickableTextureComponent(0.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48 + CollectionsPage.widthToMoveActiveTab, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Shipped"), Game1.mouseCursors, new Rectangle(640, 80, 16, 16), 4f)
		{
			myID = 7001,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		this.collections.Add(0, new List<List<ClickableTextureComponent>>());
		this.sideTabs.Add(1, new ClickableTextureComponent(1.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Fish"), Game1.mouseCursors, new Rectangle(640, 64, 16, 16), 4f)
		{
			myID = 7002,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		this.collections.Add(1, new List<List<ClickableTextureComponent>>());
		this.sideTabs.Add(2, new ClickableTextureComponent(2.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Artifacts"), Game1.mouseCursors, new Rectangle(656, 64, 16, 16), 4f)
		{
			myID = 7003,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		this.collections.Add(2, new List<List<ClickableTextureComponent>>());
		this.sideTabs.Add(3, new ClickableTextureComponent(3.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Minerals"), Game1.mouseCursors, new Rectangle(672, 64, 16, 16), 4f)
		{
			myID = 7004,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		this.collections.Add(3, new List<List<ClickableTextureComponent>>());
		this.sideTabs.Add(4, new ClickableTextureComponent(4.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Cooking"), Game1.mouseCursors, new Rectangle(688, 64, 16, 16), 4f)
		{
			myID = 7005,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		this.collections.Add(4, new List<List<ClickableTextureComponent>>());
		this.sideTabs.Add(5, new ClickableTextureComponent(5.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Achievements"), Game1.mouseCursors, new Rectangle(656, 80, 16, 16), 4f)
		{
			myID = 7006,
			upNeighborID = 7005,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		this.collections.Add(5, new List<List<ClickableTextureComponent>>());
		this.sideTabs.Add(7, new ClickableTextureComponent(7.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Letters"), Game1.mouseCursors, new Rectangle(688, 80, 16, 16), 4f)
		{
			myID = 7008,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		this.collections.Add(7, new List<List<ClickableTextureComponent>>());
		if (Game1.player.secretNotesSeen.Count > 0)
		{
			this.sideTabs.Add(6, new ClickableTextureComponent(6.ToString() ?? "", new Rectangle(base.xPositionOnScreen - 48, base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_SecretNotes"), Game1.mouseCursors, new Rectangle(672, 80, 16, 16), 4f)
			{
				myID = 7007,
				upNeighborID = -99998,
				rightNeighborID = 0
			});
			this.collections.Add(6, new List<List<ClickableTextureComponent>>());
		}
		this.sideTabs[0].upNeighborID = -1;
		this.sideTabs[0].upNeighborImmutable = true;
		int last_tab = 0;
		int last_y = 0;
		foreach (int key in this.sideTabs.Keys)
		{
			if (this.sideTabs[key].bounds.Y > last_y)
			{
				last_y = this.sideTabs[key].bounds.Y;
				last_tab = key;
			}
		}
		this.sideTabs[last_tab].downNeighborID = -1;
		this.sideTabs[last_tab].downNeighborImmutable = true;
		CollectionsPage.widthToMoveActiveTab = 8;
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 706,
			rightNeighborID = -7777
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + width - 32 - 60, base.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 707,
			leftNeighborID = -7777
		};
		int[] widthUsed = new int[8];
		int baseX = base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
		int baseY = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
		int collectionWidth = 10;
		List<ParsedItemData> dataEntries = new List<ParsedItemData>(from entry in ItemRegistry.GetObjectTypeDefinition().GetAllData()
			orderby entry.TextureName, entry.SpriteIndex
			select entry);
		List<ParsedItemData> wineAndFriends = new List<ParsedItemData>();
		for (int i = dataEntries.Count - 1; i >= 0; i--)
		{
			string s = dataEntries[i].InternalName;
			if (s.Equals("Wine") || s.Equals("Pickles") || s.Equals("Jelly") || s.Equals("Juice"))
			{
				wineAndFriends.Add(dataEntries[i]);
				dataEntries.RemoveAt(i);
			}
			if (wineAndFriends.Count == 4)
			{
				break;
			}
		}
		wineAndFriends.Sort((ParsedItemData a, ParsedItemData b) => a.InternalName.CompareTo(b.InternalName));
		dataEntries.Insert(278, wineAndFriends[2]);
		dataEntries.Insert(279, wineAndFriends[0]);
		dataEntries.Insert(283, wineAndFriends[3]);
		dataEntries.Insert(284, wineAndFriends[1]);
		foreach (ParsedItemData data in dataEntries)
		{
			string id = data.ItemId;
			string type = data.ObjectType;
			bool farmerHas = false;
			bool farmerHasButNotMade = false;
			int whichCollection;
			switch (type)
			{
			case "Arch":
				whichCollection = 2;
				farmerHas = LibraryMuseum.HasDonatedArtifact(id);
				break;
			case "Fish":
				if (data.RawData is ObjectData { ExcludeFromFishingCollection: not false })
				{
					continue;
				}
				whichCollection = 1;
				farmerHas = Game1.player.fishCaught.ContainsKey(data.QualifiedItemId);
				break;
			default:
				if (data.Category != -2)
				{
					if (type == "Cooking" || data.Category == -7)
					{
						whichCollection = 4;
						string last_minute_1_5_hack_name = data.InternalName;
						switch (last_minute_1_5_hack_name)
						{
						case "Cheese Cauli.":
							last_minute_1_5_hack_name = "Cheese Cauliflower";
							break;
						case "Cheese Cauliflower":
							last_minute_1_5_hack_name = "Cheese Cauli.";
							break;
						case "Vegetable Medley":
							last_minute_1_5_hack_name = "Vegetable Stew";
							break;
						case "Cookie":
							last_minute_1_5_hack_name = "Cookies";
							break;
						case "Eggplant Parmesan":
							last_minute_1_5_hack_name = "Eggplant Parm.";
							break;
						case "Cranberry Sauce":
							last_minute_1_5_hack_name = "Cran. Sauce";
							break;
						case "Dish O' The Sea":
							last_minute_1_5_hack_name = "Dish o' The Sea";
							break;
						}
						if (Game1.player.recipesCooked.ContainsKey(id))
						{
							farmerHas = true;
						}
						else if (Game1.player.cookingRecipes.ContainsKey(last_minute_1_5_hack_name))
						{
							farmerHasButNotMade = true;
						}
						switch (id)
						{
						case "217":
						case "772":
						case "773":
						case "279":
						case "873":
							continue;
						}
					}
					else
					{
						if (!Object.isPotentialBasicShipped(id, data.Category, data.ObjectType))
						{
							continue;
						}
						whichCollection = 0;
						farmerHas = Game1.player.basicShipped.ContainsKey(id);
					}
					break;
				}
				goto case "Minerals";
			case "Minerals":
				whichCollection = 3;
				farmerHas = LibraryMuseum.HasDonatedArtifact(id);
				break;
			}
			int xPos = baseX + widthUsed[whichCollection] % collectionWidth * 68;
			int yPos = baseY + widthUsed[whichCollection] / collectionWidth * 68;
			if (yPos > base.yPositionOnScreen + height - 128)
			{
				this.collections[whichCollection].Add(new List<ClickableTextureComponent>());
				widthUsed[whichCollection] = 0;
				xPos = baseX;
				yPos = baseY;
			}
			if (this.collections[whichCollection].Count == 0)
			{
				this.collections[whichCollection].Add(new List<ClickableTextureComponent>());
			}
			List<ClickableTextureComponent> list = this.collections[whichCollection].Last();
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
			list.Add(new ClickableTextureComponent(id + " " + farmerHas + " " + farmerHasButNotMade, new Rectangle(xPos, yPos, 64, 64), null, "", itemData.GetTexture(), itemData.GetSourceRect(), 4f, farmerHas)
			{
				myID = list.Count,
				rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
				leftNeighborID = ((list.Count % collectionWidth == 0) ? 7001 : (list.Count - 1)),
				downNeighborID = ((yPos + 68 > base.yPositionOnScreen + height - 128) ? (-7777) : (list.Count + collectionWidth)),
				upNeighborID = ((list.Count < collectionWidth) ? 12347 : (list.Count - collectionWidth)),
				fullyImmutable = true
			});
			widthUsed[whichCollection]++;
		}
		if (this.collections[5].Count == 0)
		{
			this.collections[5].Add(new List<ClickableTextureComponent>());
		}
		foreach (KeyValuePair<int, string> kvp in Game1.achievements)
		{
			bool farmerHas2 = Game1.player.achievements.Contains(kvp.Key);
			string[] split = kvp.Value.Split('^');
			if (farmerHas2 || (split[2].Equals("true") && (split[3].Equals("-1") || this.farmerHasAchievements(split[3]))))
			{
				int xPos2 = baseX + widthUsed[5] % collectionWidth * 68;
				int yPos2 = baseY + widthUsed[5] / collectionWidth * 68;
				this.collections[5][0].Add(new ClickableTextureComponent(kvp.Key + " " + farmerHas2, new Rectangle(xPos2, yPos2, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f));
				widthUsed[5]++;
			}
			else
			{
				int xPos3 = baseX + widthUsed[5] % collectionWidth * 68;
				int yPos3 = baseY + widthUsed[5] / collectionWidth * 68;
				this.collections[5][0].Add(new ClickableTextureComponent("??? false", new Rectangle(xPos3, yPos3, 64, 64), null, "???", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f));
				widthUsed[5]++;
			}
		}
		if (Game1.player.secretNotesSeen.Count > 0)
		{
			if (this.collections[6].Count == 0)
			{
				this.collections[6].Add(new List<ClickableTextureComponent>());
			}
			this.secretNotesData = DataLoader.SecretNotes(Game1.content);
			this.secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
			bool show_journals = Game1.player.secretNotesSeen.Contains(GameLocation.JOURNAL_INDEX + 1);
			foreach (int i2 in this.secretNotesData.Keys)
			{
				if (i2 >= GameLocation.JOURNAL_INDEX)
				{
					if (!show_journals)
					{
						continue;
					}
				}
				else if (!Game1.player.hasMagnifyingGlass)
				{
					continue;
				}
				int xPos4 = baseX + widthUsed[6] % collectionWidth * 68;
				int yPos4 = baseY + widthUsed[6] / collectionWidth * 68;
				if (i2 >= GameLocation.JOURNAL_INDEX)
				{
					this.collections[6][0].Add(new ClickableTextureComponent(i2 + " " + Game1.player.secretNotesSeen.Contains(i2), new Rectangle(xPos4, yPos4, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 842, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(i2)));
				}
				else
				{
					this.collections[6][0].Add(new ClickableTextureComponent(i2 + " " + Game1.player.secretNotesSeen.Contains(i2), new Rectangle(xPos4, yPos4, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 79, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(i2)));
				}
				widthUsed[6]++;
			}
		}
		if (this.collections[7].Count == 0)
		{
			this.collections[7].Add(new List<ClickableTextureComponent>());
		}
		List<ClickableTextureComponent> letters = this.collections[7].Last();
		Dictionary<string, string> mail = DataLoader.Mail(Game1.content);
		foreach (string s2 in Game1.player.mailReceived)
		{
			if (mail.TryGetValue(s2, out var rawText))
			{
				int xPos5 = baseX + widthUsed[7] % collectionWidth * 68;
				int yPos5 = baseY + widthUsed[7] / collectionWidth * 68;
				string[] split2 = rawText.Split("[#]");
				if (yPos5 > base.yPositionOnScreen + height - 128)
				{
					this.collections[7].Add(new List<ClickableTextureComponent>());
					widthUsed[7] = 0;
					xPos5 = baseX;
					yPos5 = baseY;
					letters = this.collections[7].Last();
				}
				letters.Add(new ClickableTextureComponent(s2 + " true " + ((split2.Length > 1) ? split2[1] : "???"), new Rectangle(xPos5, yPos5, 64, 64), null, "", Game1.mouseCursors, new Rectangle(190, 423, 14, 11), 4f, drawShadow: true)
				{
					myID = letters.Count,
					rightNeighborID = (((letters.Count + 1) % collectionWidth == 0) ? (-1) : (letters.Count + 1)),
					leftNeighborID = ((letters.Count % collectionWidth == 0) ? 7008 : (letters.Count - 1)),
					downNeighborID = ((yPos5 + 68 > base.yPositionOnScreen + height - 128) ? (-7777) : (letters.Count + collectionWidth)),
					upNeighborID = ((letters.Count < collectionWidth) ? 12347 : (letters.Count - collectionWidth)),
					fullyImmutable = true
				});
				widthUsed[7]++;
			}
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		base.customSnapBehavior(direction, oldRegion, oldID);
		switch (direction)
		{
		case 2:
			if (this.currentPage > 0)
			{
				base.currentlySnappedComponent = base.getComponentWithID(706);
			}
			else if (this.currentPage == 0 && this.collections[this.currentTab].Count > 1)
			{
				base.currentlySnappedComponent = base.getComponentWithID(707);
			}
			this.backButton.upNeighborID = oldID;
			this.forwardButton.upNeighborID = oldID;
			break;
		case 3:
			if (oldID == 707 && this.currentPage > 0)
			{
				base.currentlySnappedComponent = base.getComponentWithID(706);
			}
			break;
		case 1:
			if (oldID == 706 && this.collections[this.currentTab].Count > this.currentPage + 1)
			{
				base.currentlySnappedComponent = base.getComponentWithID(707);
			}
			break;
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <summary>Restore the page state when it's recreated for a window resize.</summary>
	/// <param name="oldPage">The previous page instance before it was recreated.</param>
	public void postWindowSizeChange(IClickableMenu oldPage)
	{
		if (oldPage is CollectionsPage oldCollectionsPage)
		{
			this.sideTabs[this.currentTab].bounds.X -= CollectionsPage.widthToMoveActiveTab;
			this.currentTab = oldCollectionsPage.currentTab;
			this.currentPage = oldCollectionsPage.currentPage;
			this.sideTabs[this.currentTab].bounds.X += CollectionsPage.widthToMoveActiveTab;
		}
	}

	private bool farmerHasAchievements(string listOfAchievementNumbers)
	{
		string[] array = ArgUtility.SplitBySpace(listOfAchievementNumbers);
		foreach (string s in array)
		{
			if (!Game1.player.achievements.Contains(Convert.ToInt32(s)))
			{
				return false;
			}
		}
		return true;
	}

	public override bool readyToClose()
	{
		if (this.letterviewerSubMenu != null)
		{
			return false;
		}
		return base.readyToClose();
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.letterviewerSubMenu == null)
		{
			return;
		}
		this.letterviewerSubMenu.update(time);
		if (this.letterviewerSubMenu.destroy)
		{
			this.letterviewerSubMenu = null;
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		this.letterviewerSubMenu?.receiveKeyPress(key);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.letterviewerSubMenu != null)
		{
			this.letterviewerSubMenu.receiveLeftClick(x, y);
			return;
		}
		foreach (KeyValuePair<int, ClickableTextureComponent> v in this.sideTabs)
		{
			if (v.Value.containsPoint(x, y) && this.currentTab != v.Key)
			{
				Game1.playSound("smallSelect");
				this.sideTabs[this.currentTab].bounds.X -= CollectionsPage.widthToMoveActiveTab;
				this.currentTab = Convert.ToInt32(v.Value.name);
				this.currentPage = 0;
				v.Value.bounds.X += CollectionsPage.widthToMoveActiveTab;
			}
		}
		if (this.currentPage > 0 && this.backButton.containsPoint(x, y))
		{
			this.currentPage--;
			Game1.playSound("shwip");
			this.backButton.scale = this.backButton.baseScale;
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && this.currentPage == 0)
			{
				base.currentlySnappedComponent = this.forwardButton;
				Game1.setMousePosition(base.currentlySnappedComponent.bounds.Center);
			}
		}
		if (this.currentPage < this.collections[this.currentTab].Count - 1 && this.forwardButton.containsPoint(x, y))
		{
			this.currentPage++;
			Game1.playSound("shwip");
			this.forwardButton.scale = this.forwardButton.baseScale;
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && this.currentPage == this.collections[this.currentTab].Count - 1)
			{
				base.currentlySnappedComponent = this.backButton;
				Game1.setMousePosition(base.currentlySnappedComponent.bounds.Center);
			}
		}
		switch (this.currentTab)
		{
		case 7:
		{
			Dictionary<string, string> mail = DataLoader.Mail(Game1.content);
			{
				foreach (ClickableTextureComponent c2 in this.collections[this.currentTab][this.currentPage])
				{
					if (c2.containsPoint(x, y))
					{
						string id = ArgUtility.SplitBySpaceAndGet(c2.name, 0);
						this.letterviewerSubMenu = new LetterViewerMenu(mail[id], id, fromCollection: true);
					}
				}
				break;
			}
		}
		case 6:
		{
			foreach (ClickableTextureComponent c in this.collections[this.currentTab][this.currentPage])
			{
				if (c.containsPoint(x, y))
				{
					string[] split = ArgUtility.SplitBySpace(c.name);
					if (split[1] == "True" && int.TryParse(split[0], out var index))
					{
						this.letterviewerSubMenu = new LetterViewerMenu(index);
						this.letterviewerSubMenu.isFromCollection = true;
						break;
					}
				}
			}
			break;
		}
		}
	}

	public override bool shouldDrawCloseButton()
	{
		return this.letterviewerSubMenu == null;
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.letterviewerSubMenu?.receiveRightClick(x, y);
	}

	public override void applyMovementKey(int direction)
	{
		if (this.letterviewerSubMenu != null)
		{
			this.letterviewerSubMenu.applyMovementKey(direction);
		}
		else
		{
			base.applyMovementKey(direction);
		}
	}

	/// <inheritdoc />
	public override void gamePadButtonHeld(Buttons b)
	{
		if (this.letterviewerSubMenu != null)
		{
			this.letterviewerSubMenu.gamePadButtonHeld(b);
		}
		else
		{
			base.gamePadButtonHeld(b);
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		if (this.letterviewerSubMenu != null)
		{
			this.letterviewerSubMenu.receiveGamePadButton(button);
		}
		else
		{
			base.receiveGamePadButton(button);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		this.value = -1;
		this.secretNoteImage = -1;
		if (this.letterviewerSubMenu != null)
		{
			this.letterviewerSubMenu.performHoverAction(x, y);
			return;
		}
		foreach (ClickableTextureComponent c in this.sideTabs.Values)
		{
			if (c.containsPoint(x, y))
			{
				this.hoverText = c.hoverText;
				return;
			}
		}
		bool hoveredAny = false;
		foreach (ClickableTextureComponent c2 in this.collections[this.currentTab][this.currentPage])
		{
			if (c2.containsPoint(x, y, 2))
			{
				c2.scale = Math.Min(c2.scale + 0.02f, c2.baseScale + 0.1f);
				string[] data_split = ArgUtility.SplitBySpace(c2.name);
				if (this.currentTab == 5 || (data_split.Length > 1 && Convert.ToBoolean(data_split[1])) || (data_split.Length > 2 && Convert.ToBoolean(data_split[2])))
				{
					if (this.currentTab == 7)
					{
						this.hoverText = Game1.parseText(c2.name.Substring(c2.name.IndexOf(' ', c2.name.IndexOf(' ') + 1) + 1), Game1.smallFont, 256);
					}
					else
					{
						this.hoverText = this.createDescription(data_split[0]);
					}
				}
				else
				{
					if (this.hoverText != "???")
					{
						this.hoverItem = null;
					}
					this.hoverText = "???";
				}
				hoveredAny = true;
			}
			else
			{
				c2.scale = Math.Max(c2.scale - 0.02f, c2.baseScale);
			}
		}
		if (!hoveredAny)
		{
			this.hoverItem = null;
		}
		this.forwardButton.tryHover(x, y, 0.5f);
		this.backButton.tryHover(x, y, 0.5f);
	}

	public string createDescription(string id)
	{
		string description = "";
		switch (this.currentTab)
		{
		case 5:
		{
			if (id == "???")
			{
				return "???";
			}
			int index = int.Parse(id);
			string[] split = Game1.achievements[index].Split('^');
			description = description + split[0] + Environment.NewLine + Environment.NewLine;
			description += split[1];
			break;
		}
		case 6:
		{
			if (this.secretNotesData == null)
			{
				break;
			}
			int index2 = int.Parse(id);
			description = ((index2 >= GameLocation.JOURNAL_INDEX) ? (description + Game1.content.LoadString("Strings\\Locations:Journal_Name") + " #" + (index2 - GameLocation.JOURNAL_INDEX)) : (description + Game1.content.LoadString("Strings\\Locations:Secret_Note_Name") + " #" + index2));
			if (this.secretNotesData[index2][0] == '!')
			{
				this.secretNoteImage = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(this.secretNotesData[index2], 1));
				break;
			}
			string letter_text = Game1.parseText(Utility.ParseGiftReveals(this.secretNotesData[index2]).TrimStart(' ', '^').Replace("^", Environment.NewLine)
				.Replace("@", Game1.player.name.Value), Game1.smallFont, 512);
			string[] split2 = letter_text.Split(Environment.NewLine);
			int max_lines = 15;
			if (split2.Length > max_lines)
			{
				string[] new_split = new string[max_lines];
				for (int i = 0; i < max_lines; i++)
				{
					new_split[i] = split2[i];
				}
				letter_text = string.Join(Environment.NewLine, new_split).Trim() + Environment.NewLine + "(...)";
			}
			description = description + Environment.NewLine + Environment.NewLine + letter_text;
			break;
		}
		default:
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(id);
			string displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + data.ItemId + "_CollectionsTabName") ?? data.DisplayName;
			string dataDescription = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + data.ItemId + "_CollectionsTabDescription") ?? data.Description;
			description = description + displayName + Environment.NewLine + Environment.NewLine + Game1.parseText(dataDescription, Game1.smallFont, 256) + Environment.NewLine + Environment.NewLine;
			switch (data.ObjectType)
			{
			case "Arch":
			{
				description += (Game1.player.archaeologyFound.TryGetValue(id, out var fields2) ? Game1.content.LoadString("Strings\\UI:Collections_Description_ArtifactsFound", fields2[0]) : "");
				break;
			}
			case "Cooking":
			{
				description += (Game1.player.recipesCooked.TryGetValue(id, out var timesCooked) ? Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", timesCooked) : "");
				if (this.hoverItem == null || this.hoverItem.ItemId != id)
				{
					this.hoverItem = new Object(id, 1);
					string last_minute_1_5_hack_name = this.hoverItem.Name;
					switch (last_minute_1_5_hack_name)
					{
					case "Cheese Cauli.":
						last_minute_1_5_hack_name = "Cheese Cauliflower";
						break;
					case "Cheese Cauliflower":
						last_minute_1_5_hack_name = "Cheese Cauli.";
						break;
					case "Vegetable Medley":
						last_minute_1_5_hack_name = "Vegetable Stew";
						break;
					case "Cookie":
						last_minute_1_5_hack_name = "Cookies";
						break;
					case "Eggplant Parmesan":
						last_minute_1_5_hack_name = "Eggplant Parm.";
						break;
					case "Cranberry Sauce":
						last_minute_1_5_hack_name = "Cran. Sauce";
						break;
					case "Dish O' The Sea":
						last_minute_1_5_hack_name = "Dish o' The Sea";
						break;
					}
					this.hoverCraftingRecipe = new CraftingRecipe(last_minute_1_5_hack_name, isCookingRecipe: true);
				}
				break;
			}
			case "Fish":
			{
				if (Game1.player.fishCaught.TryGetValue("(O)" + id, out var fields))
				{
					description += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", fields[0]);
					if (fields[1] > 0)
					{
						description = description + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en) ? Math.Round((double)fields[1] * 2.54) : ((double)fields[1])));
					}
				}
				else
				{
					description += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", 0);
				}
				break;
			}
			default:
			{
				description = ((!(data.ObjectType == "Minerals") && data.Category != -2) ? (description + Game1.content.LoadString("Strings\\UI:Collections_Description_NumberShipped", Game1.player.basicShipped.TryGetValue(id, out var timesFound) ? timesFound : 0)) : (description + Game1.content.LoadString("Strings\\UI:Collections_Description_MineralsFound", Game1.player.mineralsFound.TryGetValue(id, out var timesFound2) ? timesFound2 : 0)));
				break;
			}
			}
			this.value = ObjectDataDefinition.GetRawPrice(data);
			break;
		}
		}
		return description;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		foreach (ClickableTextureComponent value in this.sideTabs.Values)
		{
			value.draw(b);
		}
		if (this.currentPage > 0)
		{
			this.backButton.draw(b);
		}
		if (this.currentPage < this.collections[this.currentTab].Count - 1)
		{
			this.forwardButton.draw(b);
		}
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (ClickableTextureComponent c in this.collections[this.currentTab][this.currentPage])
		{
			string[] nameParts = ArgUtility.SplitBySpace(c.name);
			bool drawColor = Convert.ToBoolean(nameParts[1]);
			bool drawColorFaded = (this.currentTab == 4 && Convert.ToBoolean(nameParts[2])) || (this.currentTab == 5 && !drawColor && c.hoverText != "???");
			c.draw(b, drawColorFaded ? (Color.DimGray * 0.4f) : (drawColor ? Color.White : (Color.Black * 0.2f)), 0.86f);
			if (this.currentTab == 5 && drawColor)
			{
				int startPos = Utility.CreateRandom(Convert.ToInt32(nameParts[0])).Next(12);
				b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X + 16 + 16, c.bounds.Y + 20 + 16), new Rectangle(256 + startPos % 6 * 64 / 2, 128 + startPos / 6 * 64 / 2, 32, 32), Color.White, 0f, new Vector2(16f, 16f), c.scale, SpriteEffects.None, 0.88f);
			}
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (this.hoverItem != null)
		{
			string desc = this.hoverItem.getDescription();
			string name = this.hoverItem.DisplayName;
			if (desc.Contains("{0}"))
			{
				string replaced_desc = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoverItem.Name + "_CollectionsTabDescription");
				if (replaced_desc != null)
				{
					desc = replaced_desc;
				}
				string replaced_name = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + this.hoverItem.Name + "_CollectionsTabName");
				if (replaced_name != null)
				{
					name = replaced_name;
				}
			}
			IClickableMenu.drawToolTip(b, desc, name, this.hoverItem, heldItem: false, -1, 0, null, -1, this.hoverCraftingRecipe);
		}
		else if (!this.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, this.value);
			if (this.secretNoteImage != -1)
			{
				IClickableMenu.drawTextureBox(b, Game1.getOldMouseX(), Game1.getOldMouseY() + 64 + 32, 288, 288, Color.White);
				b.Draw(this.secretNoteImageTexture, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 64 + 32 + 16), new Rectangle(this.secretNoteImage * 64 % this.secretNoteImageTexture.Width, this.secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
			}
		}
		this.letterviewerSubMenu?.draw(b);
	}
}
