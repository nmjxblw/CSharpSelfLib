using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace StardewValley.Menus;

public class JunimoNoteMenu : IClickableMenu
{
	public const int region_ingredientSlotModifier = 250;

	public const int region_ingredientListModifier = 1000;

	public const int region_bundleModifier = 5000;

	public const int region_areaNextButton = 101;

	public const int region_areaBackButton = 102;

	public const int region_backButton = 103;

	public const int region_purchaseButton = 104;

	public const int region_presentButton = 105;

	public const string noteTextureName = "LooseSprites\\JunimoNote";

	public Texture2D noteTexture;

	public bool specificBundlePage;

	public const int baseWidth = 320;

	public const int baseHeight = 180;

	public InventoryMenu inventory;

	public Item partialDonationItem;

	public List<Item> partialDonationComponents = new List<Item>();

	public BundleIngredientDescription? currentPartialIngredientDescription;

	public int currentPartialIngredientDescriptionIndex = -1;

	public Item heldItem;

	public Item hoveredItem;

	public static bool canClick = true;

	public int whichArea;

	public int gameMenuTabToReturnTo = -1;

	public IClickableMenu menuToReturnTo;

	public bool bundlesChanged;

	public static ScreenSwipe screenSwipe;

	public static string hoverText = "";

	public List<Bundle> bundles = new List<Bundle>();

	public static TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();

	public List<ClickableTextureComponent> ingredientSlots = new List<ClickableTextureComponent>();

	public List<ClickableTextureComponent> ingredientList = new List<ClickableTextureComponent>();

	public bool fromGameMenu;

	public bool fromThisMenu;

	public bool scrambledText;

	private bool singleBundleMenu;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent purchaseButton;

	public ClickableTextureComponent areaNextButton;

	public ClickableTextureComponent areaBackButton;

	public ClickableAnimatedComponent presentButton;

	public Action<int> onIngredientDeposit;

	public Action<JunimoNoteMenu> onBundleComplete;

	public Action<JunimoNoteMenu> onScreenSwipeFinished;

	public Bundle currentPageBundle;

	private int oldTriggerSpot;

	public JunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false)
		: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
	{
		CommunityCenter cc = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		if (fromGameMenu && !fromThisMenu)
		{
			for (int i = 0; i < cc.areasComplete.Count; i++)
			{
				if (cc.shouldNoteAppearInArea(i) && !cc.areasComplete[i])
				{
					area = i;
					this.whichArea = area;
					break;
				}
			}
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
			{
				area = 6;
			}
		}
		this.setUpMenu(area, cc.bundlesDict());
		Game1.player.forceCanMove();
		this.areaNextButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 128, base.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			visible = false,
			myID = 101,
			leftNeighborID = 102,
			leftNeighborImmutable = true,
			downNeighborID = -99998
		};
		this.areaBackButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 64, base.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			visible = false,
			myID = 102,
			rightNeighborID = 101,
			rightNeighborImmutable = true,
			downNeighborID = -99998
		};
		int area_count = 6;
		for (int j = 0; j < area_count; j++)
		{
			if (j != area && cc.shouldNoteAppearInArea(j))
			{
				this.areaNextButton.visible = true;
				this.areaBackButton.visible = true;
				break;
			}
		}
		this.fromGameMenu = fromGameMenu;
		this.fromThisMenu = fromThisMenu;
		foreach (Bundle bundle in this.bundles)
		{
			bundle.depositsAllowed = false;
		}
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public JunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
		: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
	{
		this.setUpMenu(whichArea, bundlesComplete);
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public JunimoNoteMenu(Bundle b, string noteTexturePath)
		: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
	{
		this.singleBundleMenu = true;
		this.whichArea = -1;
		this.noteTexture = Game1.temporaryContent.Load<Texture2D>(noteTexturePath);
		JunimoNoteMenu.tempSprites.Clear();
		this.inventory = new InventoryMenu(base.xPositionOnScreen + 128, base.yPositionOnScreen + 140, playerInventory: true, null, HighlightObjects, 36, 6, 8, 8, drawSlots: false)
		{
			capacity = 36
		};
		for (int i = 0; i < this.inventory.inventory.Count; i++)
		{
			if (i >= this.inventory.actualInventory.Count)
			{
				this.inventory.inventory[i].visible = false;
			}
		}
		foreach (ClickableComponent item in this.inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
		{
			item.downNeighborID = -99998;
		}
		foreach (ClickableComponent item2 in this.inventory.GetBorder(InventoryMenu.BorderSide.Right))
		{
			item2.rightNeighborID = -99998;
		}
		this.inventory.dropItemInvisibleButton.visible = false;
		JunimoNoteMenu.canClick = true;
		this.setUpBundleSpecificPage(b);
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.specificBundlePage)
		{
			base.currentlySnappedComponent = base.getComponentWithID(0);
		}
		else
		{
			base.currentlySnappedComponent = base.getComponentWithID(5000);
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
	{
		return !this.specificBundlePage;
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") || oldID - 5000 < 0 || oldID - 5000 >= 10 || base.currentlySnappedComponent == null)
		{
			return;
		}
		int lowestScoreBundle = -1;
		int lowestScore = 999999;
		Point startingPosition = base.currentlySnappedComponent.bounds.Center;
		for (int i = 0; i < this.bundles.Count; i++)
		{
			if (this.bundles[i].myID == oldID)
			{
				continue;
			}
			int score = 999999;
			Point bundlePosition = this.bundles[i].bounds.Center;
			switch (direction)
			{
			case 3:
				if (bundlePosition.X < startingPosition.X)
				{
					score = startingPosition.X - bundlePosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
				}
				break;
			case 0:
				if (bundlePosition.Y < startingPosition.Y)
				{
					score = startingPosition.Y - bundlePosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
				}
				break;
			case 1:
				if (bundlePosition.X > startingPosition.X)
				{
					score = bundlePosition.X - startingPosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
				}
				break;
			case 2:
				if (bundlePosition.Y > startingPosition.Y)
				{
					score = bundlePosition.Y - startingPosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
				}
				break;
			}
			if (score < 10000 && score < lowestScore)
			{
				lowestScore = score;
				lowestScoreBundle = i;
			}
		}
		if (lowestScoreBundle != -1)
		{
			base.currentlySnappedComponent = base.getComponentWithID(lowestScoreBundle + 5000);
			this.snapCursorToCurrentSnappedComponent();
			return;
		}
		switch (direction)
		{
		case 2:
			if (this.presentButton != null)
			{
				base.currentlySnappedComponent = this.presentButton;
				this.snapCursorToCurrentSnappedComponent();
				this.presentButton.upNeighborID = oldID;
			}
			break;
		case 3:
			if (this.areaBackButton != null && this.areaBackButton.visible)
			{
				base.currentlySnappedComponent = this.areaBackButton;
				this.snapCursorToCurrentSnappedComponent();
				this.areaBackButton.rightNeighborID = oldID;
			}
			break;
		case 1:
			if (this.areaNextButton != null && this.areaNextButton.visible)
			{
				base.currentlySnappedComponent = this.areaNextButton;
				this.snapCursorToCurrentSnappedComponent();
				this.areaNextButton.leftNeighborID = oldID;
			}
			break;
		}
	}

	public void setUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
	{
		this.noteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
		if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
		{
			Game1.player.removeQuest("26");
			Game1.player.mailReceived.Add("seenJunimoNote");
		}
		if (!Game1.player.hasOrWillReceiveMail("wizardJunimoNote"))
		{
			Game1.addMailForTomorrow("wizardJunimoNote");
		}
		if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
		{
			Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
		}
		this.scrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
		JunimoNoteMenu.tempSprites.Clear();
		this.whichArea = whichArea;
		this.inventory = new InventoryMenu(base.xPositionOnScreen + 128, base.yPositionOnScreen + 140, playerInventory: true, null, HighlightObjects, 36, 6, 8, 8, drawSlots: false)
		{
			capacity = 36
		};
		for (int i = 0; i < this.inventory.inventory.Count; i++)
		{
			if (i >= this.inventory.actualInventory.Count)
			{
				this.inventory.inventory[i].visible = false;
			}
		}
		foreach (ClickableComponent item in this.inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
		{
			item.downNeighborID = -99998;
		}
		foreach (ClickableComponent item2 in this.inventory.GetBorder(InventoryMenu.BorderSide.Right))
		{
			item2.rightNeighborID = -99998;
		}
		this.inventory.dropItemInvisibleButton.visible = false;
		Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
		string areaName = CommunityCenter.getAreaNameFromNumber(whichArea);
		int bundlesAdded = 0;
		foreach (string k in bundlesInfo.Keys)
		{
			if (k.Contains(areaName))
			{
				int bundleIndex = Convert.ToInt32(k.Split('/')[1]);
				this.bundles.Add(new Bundle(bundleIndex, bundlesInfo[k], bundlesComplete[bundleIndex], this.getBundleLocationFromNumber(bundlesAdded), "LooseSprites\\JunimoNote", this)
				{
					myID = bundlesAdded + 5000,
					rightNeighborID = -7777,
					leftNeighborID = -7777,
					upNeighborID = -7777,
					downNeighborID = -7777,
					fullyImmutable = true
				});
				bundlesAdded++;
			}
		}
		this.backButton = new ClickableTextureComponent("Back", new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
		{
			myID = 103
		};
		this.checkForRewards();
		JunimoNoteMenu.canClick = true;
		Game1.playSound("shwip");
		bool isOneIncomplete = false;
		foreach (Bundle b in this.bundles)
		{
			if (!b.complete && !b.Equals(this.currentPageBundle))
			{
				isOneIncomplete = true;
				break;
			}
		}
		if (!isOneIncomplete)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			communityCenter.markAreaAsComplete(whichArea);
			base.exitFunction = restoreAreaOnExit;
			communityCenter.areaCompleteReward(whichArea);
		}
	}

	public virtual bool HighlightObjects(Item item)
	{
		if (this.currentPageBundle != null)
		{
			if (this.partialDonationItem != null && this.currentPartialIngredientDescriptionIndex >= 0)
			{
				return this.currentPageBundle.IsValidItemForThisIngredientDescription(item, this.currentPageBundle.ingredients[this.currentPartialIngredientDescriptionIndex]);
			}
			foreach (BundleIngredientDescription ingredient in this.currentPageBundle.ingredients)
			{
				if (this.currentPageBundle.IsValidItemForThisIngredientDescription(item, ingredient))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool readyToClose()
	{
		if (!this.specificBundlePage || this.singleBundleMenu)
		{
			return this.isReadyToCloseMenuOrBundle();
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!JunimoNoteMenu.canClick)
		{
			return;
		}
		base.receiveLeftClick(x, y, playSound);
		if (this.scrambledText)
		{
			return;
		}
		if (this.specificBundlePage)
		{
			if (!this.currentPageBundle.complete && this.currentPageBundle.completionTimer <= 0)
			{
				this.heldItem = this.inventory.leftClick(x, y, this.heldItem);
			}
			if (this.backButton != null && this.backButton.containsPoint(x, y) && this.heldItem == null)
			{
				this.closeBundlePage();
			}
			if (this.partialDonationItem != null)
			{
				if (this.heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					for (int i = 0; i < this.ingredientSlots.Count; i++)
					{
						if (this.ingredientSlots[i].item == this.partialDonationItem)
						{
							this.HandlePartialDonation(this.heldItem, this.ingredientSlots[i]);
						}
					}
				}
				else
				{
					for (int j = 0; j < this.ingredientSlots.Count; j++)
					{
						if (this.ingredientSlots[j].containsPoint(x, y) && this.ingredientSlots[j].item == this.partialDonationItem)
						{
							if (this.heldItem != null)
							{
								this.HandlePartialDonation(this.heldItem, this.ingredientSlots[j]);
								return;
							}
							bool return_to_inventory = Game1.oldKBState.IsKeyDown(Keys.LeftShift);
							this.ReturnPartialDonations(!return_to_inventory);
							return;
						}
					}
				}
			}
			else if (this.heldItem != null)
			{
				if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					for (int k = 0; k < this.ingredientSlots.Count; k++)
					{
						if (this.currentPageBundle.canAcceptThisItem(this.heldItem, this.ingredientSlots[k]))
						{
							if (this.ingredientSlots[k].item == null)
							{
								this.heldItem = this.currentPageBundle.tryToDepositThisItem(this.heldItem, this.ingredientSlots[k], "LooseSprites\\JunimoNote", this);
								this.checkIfBundleIsComplete();
								return;
							}
						}
						else if (this.ingredientSlots[k].item == null)
						{
							this.HandlePartialDonation(this.heldItem, this.ingredientSlots[k]);
						}
					}
				}
				for (int l = 0; l < this.ingredientSlots.Count; l++)
				{
					if (this.ingredientSlots[l].containsPoint(x, y))
					{
						if (this.currentPageBundle.canAcceptThisItem(this.heldItem, this.ingredientSlots[l]))
						{
							this.heldItem = this.currentPageBundle.tryToDepositThisItem(this.heldItem, this.ingredientSlots[l], "LooseSprites\\JunimoNote", this);
							this.checkIfBundleIsComplete();
						}
						else if (this.ingredientSlots[l].item == null)
						{
							this.HandlePartialDonation(this.heldItem, this.ingredientSlots[l]);
						}
					}
				}
			}
			if (this.purchaseButton != null && this.purchaseButton.containsPoint(x, y))
			{
				int moneyRequired = this.currentPageBundle.ingredients.Last().stack;
				if (Game1.player.Money >= moneyRequired)
				{
					Game1.player.Money -= moneyRequired;
					Game1.playSound("select");
					this.currentPageBundle.completionAnimation(this);
					if (this.purchaseButton != null)
					{
						this.purchaseButton.scale = this.purchaseButton.baseScale * 0.75f;
					}
					CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
					communityCenter.bundleRewards[this.currentPageBundle.bundleIndex] = true;
					communityCenter.bundles.FieldDict[this.currentPageBundle.bundleIndex][0] = true;
					this.checkForRewards();
					bool isOneIncomplete = false;
					foreach (Bundle b in this.bundles)
					{
						if (!b.complete && !b.Equals(this.currentPageBundle))
						{
							isOneIncomplete = true;
							break;
						}
					}
					if (!isOneIncomplete)
					{
						communityCenter.markAreaAsComplete(this.whichArea);
						base.exitFunction = restoreAreaOnExit;
						communityCenter.areaCompleteReward(this.whichArea);
					}
					else
					{
						communityCenter.getJunimoForArea(this.whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(this.currentPageBundle.bundleColor), Game1.RequireLocation("CommunityCenter"));
					}
					Game1.multiplayer.globalChatInfoMessage("Bundle");
				}
				else
				{
					Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
				}
			}
			if (base.upperRightCloseButton != null && this.isReadyToCloseMenuOrBundle() && base.upperRightCloseButton.containsPoint(x, y))
			{
				this.closeBundlePage();
				return;
			}
		}
		else
		{
			foreach (Bundle b2 in this.bundles)
			{
				if (b2.canBeClicked() && b2.containsPoint(x, y))
				{
					this.setUpBundleSpecificPage(b2);
					Game1.playSound("shwip");
					return;
				}
			}
			if (this.presentButton != null && this.presentButton.containsPoint(x, y) && !this.fromGameMenu && !this.fromThisMenu)
			{
				this.openRewardsMenu();
			}
			if (this.fromGameMenu)
			{
				if (this.areaNextButton.containsPoint(x, y))
				{
					this.SwapPage(1);
				}
				else if (this.areaBackButton.containsPoint(x, y))
				{
					this.SwapPage(-1);
				}
			}
		}
		if (this.heldItem != null && !this.isWithinBounds(x, y) && this.heldItem.canBeTrashed())
		{
			Game1.playSound("throwDownITem");
			Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			this.heldItem = null;
		}
	}

	public virtual void ReturnPartialDonation(Item item, bool play_sound = true)
	{
		List<Item> affected_items = new List<Item>();
		Item remainder = Game1.player.addItemToInventory(item, affected_items);
		foreach (Item affected_item in affected_items)
		{
			this.inventory.ShakeItem(affected_item);
		}
		if (remainder != null)
		{
			Utility.CollectOrDrop(remainder);
			this.inventory.ShakeItem(remainder);
		}
		if (play_sound)
		{
			Game1.playSound("coin");
		}
	}

	public virtual void ReturnPartialDonations(bool to_hand = true)
	{
		if (this.partialDonationComponents.Count > 0)
		{
			bool play_sound = true;
			foreach (Item item in this.partialDonationComponents)
			{
				if (this.heldItem == null && to_hand)
				{
					Game1.playSound("dwop");
					this.heldItem = item;
				}
				else
				{
					this.ReturnPartialDonation(item, play_sound);
					play_sound = false;
				}
			}
		}
		this.ResetPartialDonation();
	}

	public virtual void ResetPartialDonation()
	{
		this.partialDonationComponents.Clear();
		this.currentPartialIngredientDescription = null;
		this.currentPartialIngredientDescriptionIndex = -1;
		foreach (ClickableTextureComponent slot in this.ingredientSlots)
		{
			if (slot.item == this.partialDonationItem)
			{
				slot.item = null;
			}
		}
		this.partialDonationItem = null;
	}

	public virtual bool CanBePartiallyOrFullyDonated(Item item)
	{
		if (this.currentPageBundle == null)
		{
			return false;
		}
		int index = this.currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
		if (index < 0)
		{
			return false;
		}
		BundleIngredientDescription description = this.currentPageBundle.ingredients[index];
		int count = 0;
		if (this.currentPageBundle.IsValidItemForThisIngredientDescription(item, description))
		{
			count += item.Stack;
		}
		foreach (Item inventory_item in Game1.player.Items)
		{
			if (this.currentPageBundle.IsValidItemForThisIngredientDescription(inventory_item, description))
			{
				count += inventory_item.Stack;
			}
		}
		if (index == this.currentPartialIngredientDescriptionIndex && this.partialDonationItem != null)
		{
			count += this.partialDonationItem.Stack;
		}
		return count >= description.stack;
	}

	public virtual void HandlePartialDonation(Item item, ClickableTextureComponent slot)
	{
		if ((this.currentPageBundle != null && !this.currentPageBundle.depositsAllowed) || (this.partialDonationItem != null && slot.item != this.partialDonationItem) || !this.CanBePartiallyOrFullyDonated(item))
		{
			return;
		}
		if (!this.currentPartialIngredientDescription.HasValue)
		{
			this.currentPartialIngredientDescriptionIndex = this.currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
			if (this.currentPartialIngredientDescriptionIndex != -1)
			{
				this.currentPartialIngredientDescription = this.currentPageBundle.ingredients[this.currentPartialIngredientDescriptionIndex];
			}
		}
		if (!this.currentPartialIngredientDescription.HasValue || !this.currentPageBundle.IsValidItemForThisIngredientDescription(item, this.currentPartialIngredientDescription.Value))
		{
			return;
		}
		bool playSound = true;
		bool isHeldItem = item == this.heldItem;
		int amountToDonate;
		if (slot.item == null)
		{
			Game1.playSound("sell");
			playSound = false;
			this.partialDonationItem = item.getOne();
			amountToDonate = Math.Min(this.currentPartialIngredientDescription.Value.stack, item.Stack);
			this.partialDonationItem.Stack = amountToDonate;
			item = item.ConsumeStack(amountToDonate);
			this.partialDonationItem.Quality = this.currentPartialIngredientDescription.Value.quality;
			slot.item = this.partialDonationItem;
			slot.sourceRect.X = 512;
			slot.sourceRect.Y = 244;
		}
		else
		{
			amountToDonate = Math.Min(this.currentPartialIngredientDescription.Value.stack - this.partialDonationItem.Stack, item.Stack);
			this.partialDonationItem.Stack += amountToDonate;
			item = item.ConsumeStack(amountToDonate);
		}
		if (amountToDonate > 0)
		{
			Item donatedItem = this.heldItem.getOne();
			donatedItem.Stack = amountToDonate;
			foreach (Item contributed_item in this.partialDonationComponents)
			{
				if (contributed_item.canStackWith(this.heldItem))
				{
					donatedItem.Stack = contributed_item.addToStack(donatedItem);
				}
			}
			if (donatedItem.Stack > 0)
			{
				this.partialDonationComponents.Add(donatedItem);
			}
			this.partialDonationComponents.Sort((Item a, Item b) => b.Stack.CompareTo(a.Stack));
		}
		if (isHeldItem && item == null)
		{
			this.heldItem = null;
		}
		if (this.partialDonationItem.Stack >= this.currentPartialIngredientDescription.Value.stack)
		{
			slot.item = null;
			this.partialDonationItem = this.currentPageBundle.tryToDepositThisItem(this.partialDonationItem, slot, "LooseSprites\\JunimoNote", this);
			Item item2 = this.partialDonationItem;
			if (item2 != null && item2.Stack > 0)
			{
				this.ReturnPartialDonation(this.partialDonationItem);
			}
			this.partialDonationItem = null;
			this.ResetPartialDonation();
			this.checkIfBundleIsComplete();
		}
		else if (amountToDonate > 0 && playSound)
		{
			Game1.playSound("sell");
		}
	}

	public bool isReadyToCloseMenuOrBundle()
	{
		if (this.specificBundlePage)
		{
			Bundle bundle = this.currentPageBundle;
			if (bundle != null && bundle.completionTimer > 0)
			{
				return false;
			}
		}
		if (this.heldItem != null)
		{
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if (this.specificBundlePage)
		{
			switch (button)
			{
			case Buttons.RightTrigger:
			{
				ClickableComponent clickableComponent2 = base.currentlySnappedComponent;
				if (clickableComponent2 == null || clickableComponent2.myID >= 50)
				{
					break;
				}
				this.oldTriggerSpot = base.currentlySnappedComponent.myID;
				int id = 250;
				foreach (ClickableTextureComponent c in this.ingredientSlots)
				{
					if (c.item == null)
					{
						id = c.myID;
						break;
					}
				}
				this.setCurrentlySnappedComponentTo(id);
				this.snapCursorToCurrentSnappedComponent();
				break;
			}
			case Buttons.LeftTrigger:
			{
				ClickableComponent clickableComponent = base.currentlySnappedComponent;
				if (clickableComponent != null && clickableComponent.myID >= 250)
				{
					this.setCurrentlySnappedComponentTo(this.oldTriggerSpot);
					this.snapCursorToCurrentSnappedComponent();
				}
				break;
			}
			}
		}
		else if (this.fromGameMenu)
		{
			switch (button)
			{
			case Buttons.RightTrigger:
				this.SwapPage(1);
				break;
			case Buttons.LeftTrigger:
				this.SwapPage(-1);
				break;
			}
		}
	}

	public void SwapPage(int direction)
	{
		if ((direction > 0 && !this.areaNextButton.visible) || (direction < 0 && !this.areaBackButton.visible))
		{
			return;
		}
		CommunityCenter cc = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		int area = this.whichArea;
		int area_count = 6;
		for (int i = 0; i < area_count; i++)
		{
			area += direction;
			if (area < 0)
			{
				area += area_count;
			}
			if (area >= area_count)
			{
				area -= area_count;
			}
			if (cc.shouldNoteAppearInArea(area))
			{
				int selected_id = -1;
				if (base.currentlySnappedComponent != null && (base.currentlySnappedComponent.myID >= 5000 || base.currentlySnappedComponent.myID == 101 || base.currentlySnappedComponent.myID == 102))
				{
					selected_id = base.currentlySnappedComponent.myID;
				}
				JunimoNoteMenu new_menu = (JunimoNoteMenu)(Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, area, fromThisMenu: true)
				{
					gameMenuTabToReturnTo = this.gameMenuTabToReturnTo
				});
				if (selected_id >= 0)
				{
					new_menu.currentlySnappedComponent = new_menu.getComponentWithID(base.currentlySnappedComponent.myID);
					new_menu.snapCursorToCurrentSnappedComponent();
				}
				if (new_menu.getComponentWithID(this.areaNextButton.leftNeighborID) != null)
				{
					new_menu.areaNextButton.leftNeighborID = this.areaNextButton.leftNeighborID;
				}
				else
				{
					new_menu.areaNextButton.leftNeighborID = new_menu.areaBackButton.myID;
				}
				new_menu.areaNextButton.rightNeighborID = this.areaNextButton.rightNeighborID;
				new_menu.areaNextButton.upNeighborID = this.areaNextButton.upNeighborID;
				new_menu.areaNextButton.downNeighborID = this.areaNextButton.downNeighborID;
				if (new_menu.getComponentWithID(this.areaBackButton.rightNeighborID) != null)
				{
					new_menu.areaBackButton.leftNeighborID = this.areaBackButton.leftNeighborID;
				}
				else
				{
					new_menu.areaBackButton.leftNeighborID = new_menu.areaNextButton.myID;
				}
				new_menu.areaBackButton.rightNeighborID = this.areaBackButton.rightNeighborID;
				new_menu.areaBackButton.upNeighborID = this.areaBackButton.upNeighborID;
				new_menu.areaBackButton.downNeighborID = this.areaBackButton.downNeighborID;
				break;
			}
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (this.gameMenuTabToReturnTo != -1)
		{
			base.closeSound = "shwip";
		}
		base.receiveKeyPress(key);
		if (key == Keys.Delete && this.heldItem != null && this.heldItem.canBeTrashed())
		{
			Utility.trashItem(this.heldItem);
			this.heldItem = null;
		}
		if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.isReadyToCloseMenuOrBundle())
		{
			if (this.singleBundleMenu)
			{
				base.exitThisMenu(this.gameMenuTabToReturnTo == -1);
			}
			this.closeBundlePage();
		}
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		if (this.gameMenuTabToReturnTo != -1)
		{
			Game1.activeClickableMenu = new GameMenu(this.gameMenuTabToReturnTo, -1, playOpeningSound: false);
		}
		else if (this.menuToReturnTo != null)
		{
			Game1.activeClickableMenu = this.menuToReturnTo;
		}
	}

	private void closeBundlePage()
	{
		if (this.partialDonationItem != null)
		{
			this.ReturnPartialDonations(to_hand: false);
		}
		else if (this.specificBundlePage)
		{
			this.hoveredItem = null;
			this.inventory.descriptionText = "";
			if (this.heldItem == null)
			{
				this.takeDownBundleSpecificPage();
				Game1.playSound("shwip");
			}
			else
			{
				this.heldItem = this.inventory.tryToAddItem(this.heldItem);
			}
		}
	}

	private void reOpenThisMenu()
	{
		bool num = this.specificBundlePage;
		JunimoNoteMenu newMenu = ((!this.fromGameMenu && !this.fromThisMenu) ? new JunimoNoteMenu(this.whichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict())
		{
			gameMenuTabToReturnTo = this.gameMenuTabToReturnTo,
			menuToReturnTo = this.menuToReturnTo
		} : new JunimoNoteMenu(this.fromGameMenu, this.whichArea, this.fromThisMenu)
		{
			gameMenuTabToReturnTo = this.gameMenuTabToReturnTo,
			menuToReturnTo = this.menuToReturnTo
		});
		if (num)
		{
			foreach (Bundle bundle in newMenu.bundles)
			{
				if (bundle.bundleIndex == this.currentPageBundle.bundleIndex)
				{
					newMenu.setUpBundleSpecificPage(bundle);
					break;
				}
			}
		}
		Game1.activeClickableMenu = newMenu;
	}

	private void updateIngredientSlots()
	{
		int slotNumber = 0;
		foreach (BundleIngredientDescription ingredient in this.currentPageBundle.ingredients)
		{
			if (ingredient.completed && slotNumber < this.ingredientSlots.Count)
			{
				string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
				if (ingredient.preservesId != null)
				{
					this.ingredientSlots[slotNumber].item = Utility.CreateFlavoredItem(id, ingredient.preservesId, ingredient.quality, ingredient.stack);
				}
				else
				{
					this.ingredientSlots[slotNumber].item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality);
				}
				this.currentPageBundle.ingredientDepositAnimation(this.ingredientSlots[slotNumber], "LooseSprites\\JunimoNote", skipAnimation: true);
				slotNumber++;
			}
		}
	}

	/// <summary>Get the qualified item ID to draw in the bundle UI for an ingredient.</summary>
	/// <param name="ingredient">The ingredient to represent.</param>
	public static string GetRepresentativeItemId(BundleIngredientDescription ingredient)
	{
		if (ingredient.category.HasValue)
		{
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (data.Category == ingredient.category)
				{
					return data.QualifiedItemId;
				}
			}
			return "0";
		}
		return ingredient.id;
	}

	public static void GetBundleRewards(int area, List<Item> rewards)
	{
		CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
		foreach (string k in bundlesInfo.Keys)
		{
			if (k.Contains(CommunityCenter.getAreaNameFromNumber(area)))
			{
				int bundleIndex = Convert.ToInt32(k.Split('/')[1]);
				if (communityCenter.bundleRewards[bundleIndex])
				{
					Item i = Utility.getItemFromStandardTextDescription(bundlesInfo[k].Split('/')[1], Game1.player);
					i.SpecialVariable = bundleIndex;
					rewards.Add(i);
				}
			}
		}
	}

	private void openRewardsMenu()
	{
		Game1.playSound("smallSelect");
		List<Item> rewards = new List<Item>();
		JunimoNoteMenu.GetBundleRewards(this.whichArea, rewards);
		Game1.activeClickableMenu = new ItemGrabMenu(rewards, reverseGrab: false, showReceivingMenu: true, null, null, null, rewardGrabbed, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
		Game1.activeClickableMenu.exitFunction = ((base.exitFunction != null) ? base.exitFunction : new onExit(reOpenThisMenu));
	}

	private void rewardGrabbed(Item item, Farmer who)
	{
		Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[item.SpecialVariable] = false;
	}

	private void checkIfBundleIsComplete()
	{
		this.ReturnPartialDonations();
		if (!this.specificBundlePage || this.currentPageBundle == null)
		{
			return;
		}
		int numberOfFilledSlots = 0;
		foreach (ClickableTextureComponent c in this.ingredientSlots)
		{
			if (c.item != null && c.item != this.partialDonationItem)
			{
				numberOfFilledSlots++;
			}
		}
		if (numberOfFilledSlots < this.currentPageBundle.numberOfIngredientSlots)
		{
			return;
		}
		if (this.heldItem != null)
		{
			Game1.player.addItemToInventory(this.heldItem);
			this.heldItem = null;
		}
		if (!this.singleBundleMenu)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			for (int i = 0; i < communityCenter.bundles[this.currentPageBundle.bundleIndex].Length; i++)
			{
				communityCenter.bundles.FieldDict[this.currentPageBundle.bundleIndex][i] = true;
			}
			communityCenter.checkForNewJunimoNotes();
			JunimoNoteMenu.screenSwipe = new ScreenSwipe(0, -1f, -1, base.width, base.height);
			this.currentPageBundle.completionAnimation(this, playSound: true, 400);
			JunimoNoteMenu.canClick = false;
			communityCenter.bundleRewards[this.currentPageBundle.bundleIndex] = true;
			Game1.multiplayer.globalChatInfoMessage("Bundle");
			bool isOneIncomplete = false;
			foreach (Bundle b in this.bundles)
			{
				if (!b.complete && !b.Equals(this.currentPageBundle))
				{
					isOneIncomplete = true;
					break;
				}
			}
			if (!isOneIncomplete)
			{
				if (this.whichArea == 6)
				{
					base.exitFunction = restoreaAreaOnExit_AbandonedJojaMart;
				}
				else
				{
					communityCenter.markAreaAsComplete(this.whichArea);
					base.exitFunction = restoreAreaOnExit;
					communityCenter.areaCompleteReward(this.whichArea);
				}
			}
			else
			{
				communityCenter.getJunimoForArea(this.whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(this.currentPageBundle.bundleColor), communityCenter);
			}
			this.checkForRewards();
		}
		else if (this.onBundleComplete != null)
		{
			this.onBundleComplete(this);
		}
	}

	private void restoreaAreaOnExit_AbandonedJojaMart()
	{
		Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart").restoreAreaCutscene();
	}

	private void restoreAreaOnExit()
	{
		if (!this.fromGameMenu)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").restoreAreaCutscene(this.whichArea);
		}
	}

	public void checkForRewards()
	{
		Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
		foreach (string k in bundlesInfo.Keys)
		{
			if (k.Contains(CommunityCenter.getAreaNameFromNumber(this.whichArea)) && bundlesInfo[k].Split('/')[1].Length > 1)
			{
				int bundleIndex = Convert.ToInt32(k.Split('/')[1]);
				if (Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[bundleIndex])
				{
					this.presentButton = new ClickableAnimatedComponent(new Rectangle(base.xPositionOnScreen + 592, base.yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true));
					break;
				}
			}
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (!JunimoNoteMenu.canClick)
		{
			return;
		}
		if (this.specificBundlePage)
		{
			this.heldItem = this.inventory.rightClick(x, y, this.heldItem);
			if (this.partialDonationItem != null)
			{
				for (int i = 0; i < this.ingredientSlots.Count; i++)
				{
					if (!this.ingredientSlots[i].containsPoint(x, y) || this.ingredientSlots[i].item != this.partialDonationItem)
					{
						continue;
					}
					if (this.partialDonationComponents.Count <= 0)
					{
						break;
					}
					Item item = this.partialDonationComponents[0].getOne();
					bool valid = false;
					if (this.heldItem == null)
					{
						this.heldItem = item;
						Game1.playSound("dwop");
						valid = true;
					}
					else if (this.heldItem.canStackWith(item))
					{
						this.heldItem.addToStack(item);
						Game1.playSound("dwop");
						valid = true;
					}
					if (!valid)
					{
						break;
					}
					if (this.partialDonationComponents[0].ConsumeStack(1) == null)
					{
						this.partialDonationComponents.RemoveAt(0);
					}
					if (this.partialDonationItem != null)
					{
						int count = 0;
						foreach (Item contributedItem in this.partialDonationComponents)
						{
							count += contributedItem.Stack;
						}
						this.partialDonationItem.Stack = count;
					}
					if (this.partialDonationComponents.Count == 0)
					{
						this.ResetPartialDonation();
					}
					break;
				}
			}
		}
		if (!this.specificBundlePage && this.isReadyToCloseMenuOrBundle())
		{
			base.exitThisMenu(this.gameMenuTabToReturnTo == -1);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (this.specificBundlePage && this.currentPageBundle != null && this.currentPageBundle.completionTimer <= 0 && this.isReadyToCloseMenuOrBundle() && this.currentPageBundle.complete)
		{
			this.takeDownBundleSpecificPage();
		}
		foreach (Bundle bundle in this.bundles)
		{
			bundle.update(time);
		}
		JunimoNoteMenu.tempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		this.presentButton?.update(time);
		if (JunimoNoteMenu.screenSwipe != null)
		{
			JunimoNoteMenu.canClick = false;
			if (JunimoNoteMenu.screenSwipe.update(time))
			{
				JunimoNoteMenu.screenSwipe = null;
				JunimoNoteMenu.canClick = true;
				this.onScreenSwipeFinished?.Invoke(this);
			}
		}
		if (this.bundlesChanged && this.fromGameMenu)
		{
			this.reOpenThisMenu();
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		if (this.scrambledText)
		{
			return;
		}
		JunimoNoteMenu.hoverText = "";
		if (this.specificBundlePage)
		{
			this.backButton?.tryHover(x, y);
			if (!this.currentPageBundle.complete && this.currentPageBundle.completionTimer <= 0)
			{
				this.hoveredItem = this.inventory.hover(x, y, this.heldItem);
			}
			else
			{
				this.hoveredItem = null;
			}
			foreach (ClickableTextureComponent c in this.ingredientList)
			{
				if (c.bounds.Contains(x, y))
				{
					JunimoNoteMenu.hoverText = c.hoverText;
					break;
				}
			}
			if (this.heldItem != null)
			{
				foreach (ClickableTextureComponent c2 in this.ingredientSlots)
				{
					if (c2.bounds.Contains(x, y) && this.CanBePartiallyOrFullyDonated(this.heldItem) && (this.partialDonationItem == null || c2.item == this.partialDonationItem))
					{
						c2.sourceRect.X = 530;
						c2.sourceRect.Y = 262;
					}
					else
					{
						c2.sourceRect.X = 512;
						c2.sourceRect.Y = 244;
					}
				}
			}
			this.purchaseButton?.tryHover(x, y);
			return;
		}
		if (this.presentButton != null)
		{
			JunimoNoteMenu.hoverText = this.presentButton.tryHover(x, y);
		}
		foreach (Bundle bundle in this.bundles)
		{
			bundle.tryHoverAction(x, y);
		}
		if (this.fromGameMenu)
		{
			this.areaNextButton.tryHover(x, y);
			this.areaBackButton.tryHover(x, y);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (Game1.options.showMenuBackground)
		{
			base.drawBackground(b);
		}
		else if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		}
		if (!this.specificBundlePage)
		{
			b.Draw(this.noteTexture, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			SpriteText.drawStringHorizontallyCenteredAt(b, this.scrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(this.whichArea) : CommunityCenter.getAreaDisplayNameFromNumber(this.whichArea), base.xPositionOnScreen + base.width / 2 + 16, base.yPositionOnScreen + 12, 999999, -1, 99999, 0.88f, 0.88f, this.scrambledText);
			if (this.scrambledText)
			{
				SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), base.xPositionOnScreen + 96, base.yPositionOnScreen + 96, 999999, base.width - 192, 99999, 0.88f, 0.88f, junimoText: true);
				base.draw(b);
				if (!Game1.options.SnappyMenus && JunimoNoteMenu.canClick)
				{
					base.drawMouse(b);
				}
				return;
			}
			foreach (Bundle bundle in this.bundles)
			{
				bundle.draw(b);
			}
			this.presentButton?.draw(b);
			foreach (TemporaryAnimatedSprite tempSprite in JunimoNoteMenu.tempSprites)
			{
				tempSprite.draw(b, localPosition: true);
			}
			if (this.fromGameMenu)
			{
				if (this.areaNextButton.visible)
				{
					this.areaNextButton.draw(b);
				}
				if (this.areaBackButton.visible)
				{
					this.areaBackButton.draw(b);
				}
			}
		}
		else
		{
			b.Draw(this.noteTexture, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), new Rectangle(320, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			if (this.currentPageBundle != null)
			{
				int bundle_index = this.currentPageBundle.bundleIndex;
				Texture2D bundle_texture = this.noteTexture;
				int y_offset = 180;
				if (this.currentPageBundle.bundleTextureIndexOverride >= 0)
				{
					bundle_index = this.currentPageBundle.bundleTextureIndexOverride;
				}
				if (this.currentPageBundle.bundleTextureOverride != null)
				{
					bundle_texture = this.currentPageBundle.bundleTextureOverride;
					y_offset = 0;
				}
				b.Draw(bundle_texture, new Vector2(base.xPositionOnScreen + 872, base.yPositionOnScreen + 88), new Rectangle(bundle_index * 16 * 2 % bundle_texture.Width, y_offset + 32 * (bundle_index * 16 * 2 / bundle_texture.Width), 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
				if (this.currentPageBundle.label != null)
				{
					float textX = Game1.dialogueFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label)).X;
					b.Draw(this.noteTexture, new Vector2(base.xPositionOnScreen + 936 - (int)textX / 2 - 16, base.yPositionOnScreen + 228), new Rectangle(517, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					b.Draw(this.noteTexture, new Rectangle(base.xPositionOnScreen + 936 - (int)textX / 2, base.yPositionOnScreen + 228, (int)textX, 68), new Rectangle(520, 266, 1, 17), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
					b.Draw(this.noteTexture, new Vector2(base.xPositionOnScreen + 936 + (int)textX / 2, base.yPositionOnScreen + 228), new Rectangle(524, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, base.yPositionOnScreen + 236) + new Vector2(2f, 2f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, base.yPositionOnScreen + 236) + new Vector2(0f, 2f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, base.yPositionOnScreen + 236) + new Vector2(2f, 0f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", this.currentPageBundle.label), new Vector2((float)(base.xPositionOnScreen + 936) - textX / 2f, base.yPositionOnScreen + 236), Game1.textColor * 0.9f);
				}
			}
			if (this.backButton != null)
			{
				this.backButton.draw(b);
			}
			if (this.purchaseButton != null)
			{
				this.purchaseButton.draw(b);
				Game1.dayTimeMoneyBox.drawMoneyBox(b);
			}
			float completed_slot_alpha = 1f;
			if (this.partialDonationItem != null)
			{
				completed_slot_alpha = 0.25f;
			}
			foreach (TemporaryAnimatedSprite tempSprite2 in JunimoNoteMenu.tempSprites)
			{
				tempSprite2.draw(b, localPosition: true, 0, 0, completed_slot_alpha);
			}
			foreach (ClickableTextureComponent c in this.ingredientSlots)
			{
				float alpha_mult = 1f;
				if (this.partialDonationItem != null && c.item != this.partialDonationItem)
				{
					alpha_mult = 0.25f;
				}
				if (c.item == null || (this.partialDonationItem != null && c.item == this.partialDonationItem))
				{
					c.draw(b, (this.fromGameMenu ? (Color.LightGray * 0.5f) : Color.White) * alpha_mult, 0.89f);
				}
				c.drawItem(b, 4, 4, alpha_mult);
			}
			for (int i = 0; i < this.ingredientList.Count; i++)
			{
				float alpha_mult2 = 1f;
				if (this.currentPartialIngredientDescriptionIndex >= 0 && this.currentPartialIngredientDescriptionIndex != i)
				{
					alpha_mult2 = 0.25f;
				}
				ClickableTextureComponent c2 = this.ingredientList[i];
				bool completed = false;
				if (i < this.currentPageBundle?.ingredients?.Count && this.currentPageBundle.ingredients[i].completed)
				{
					completed = true;
				}
				if (!completed)
				{
					b.Draw(Game1.shadowTexture, new Vector2(c2.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, c2.bounds.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White * alpha_mult2, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				}
				if (c2.item != null && c2.visible)
				{
					c2.item.drawInMenu(b, new Vector2(c2.bounds.X, c2.bounds.Y), c2.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * (completed ? 0.25f : alpha_mult2), drawShadow: false);
				}
			}
			this.inventory.draw(b);
		}
		if (this.getRewardNameForArea(this.whichArea) != "")
		{
			SpriteText.drawStringWithScrollCenteredAt(b, this.getRewardNameForArea(this.whichArea), base.xPositionOnScreen + base.width / 2, Math.Min(base.yPositionOnScreen + base.height + 20, Game1.uiViewport.Height - 64 - 8));
		}
		base.draw(b);
		Game1.mouseCursorTransparency = 1f;
		if (JunimoNoteMenu.canClick)
		{
			base.drawMouse(b);
		}
		this.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
		if (this.inventory.descriptionText.Length > 0)
		{
			if (this.hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem);
			}
		}
		else
		{
			IClickableMenu.drawHoverText(b, (!this.singleBundleMenu && !Game1.player.hasOrWillReceiveMail("canReadJunimoText") && JunimoNoteMenu.hoverText.Length > 0) ? "???" : JunimoNoteMenu.hoverText, Game1.dialogueFont);
		}
		JunimoNoteMenu.screenSwipe?.draw(b);
	}

	public string getRewardNameForArea(int whichArea)
	{
		return whichArea switch
		{
			-1 => "", 
			3 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBoiler"), 
			5 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBulletin"), 
			1 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardCrafts"), 
			0 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardPantry"), 
			4 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardVault"), 
			2 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardFishTank"), 
			_ => "???", 
		};
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		JunimoNoteMenu.tempSprites.Clear();
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - 360;
		this.backButton = new ClickableTextureComponent("Back", new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
		if (this.fromGameMenu)
		{
			this.areaNextButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 128, base.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				visible = false
			};
			this.areaBackButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 64, base.yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				visible = false
			};
		}
		this.inventory = new InventoryMenu(base.xPositionOnScreen + 128, base.yPositionOnScreen + 140, playerInventory: true, null, HighlightObjects, Game1.player.maxItems.Value, 6, 8, 8, drawSlots: false);
		for (int i = 0; i < this.inventory.inventory.Count; i++)
		{
			if (i >= this.inventory.actualInventory.Count)
			{
				this.inventory.inventory[i].visible = false;
			}
		}
		for (int j = 0; j < this.bundles.Count; j++)
		{
			Point p = this.getBundleLocationFromNumber(j);
			this.bundles[j].bounds.X = p.X;
			this.bundles[j].bounds.Y = p.Y;
			this.bundles[j].sprite.position = new Vector2(p.X, p.Y);
		}
		if (!this.specificBundlePage)
		{
			return;
		}
		int numberOfIngredientSlots = this.currentPageBundle.numberOfIngredientSlots;
		List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
		this.addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
		this.ingredientSlots.Clear();
		for (int k = 0; k < ingredientSlotRectangles.Count; k++)
		{
			this.ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[k], this.noteTexture, new Rectangle(512, 244, 18, 18), 4f));
		}
		List<Rectangle> ingredientListRectangles = new List<Rectangle>();
		this.ingredientList.Clear();
		this.addRectangleRowsToList(ingredientListRectangles, this.currentPageBundle.ingredients.Count, 932, 364);
		for (int l = 0; l < ingredientListRectangles.Count; l++)
		{
			BundleIngredientDescription ingredient = this.currentPageBundle.ingredients[l];
			ItemMetadata metadata = ItemRegistry.GetMetadata(ingredient.id);
			if (metadata?.TypeIdentifier == "(O)")
			{
				ParsedItemData parsedOrErrorData = metadata.GetParsedOrErrorData();
				Texture2D texture = parsedOrErrorData.GetTexture();
				Rectangle sourceRect = parsedOrErrorData.GetSourceRect();
				Item item = ((ingredient.preservesId != null) ? Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack) : ItemRegistry.Create(ingredient.id, ingredient.stack, ingredient.quality));
				this.ingredientList.Add(new ClickableTextureComponent("", ingredientListRectangles[l], "", item.DisplayName, texture, sourceRect, 4f)
				{
					myID = l + 1000,
					item = item,
					upNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998
				});
			}
		}
		this.updateIngredientSlots();
	}

	private void setUpBundleSpecificPage(Bundle b)
	{
		JunimoNoteMenu.tempSprites.Clear();
		this.currentPageBundle = b;
		this.specificBundlePage = true;
		if (this.whichArea == 4)
		{
			if (!this.fromGameMenu)
			{
				this.purchaseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 800, base.yPositionOnScreen + 504, 260, 72), this.noteTexture, new Rectangle(517, 286, 65, 20), 4f)
				{
					myID = 797,
					leftNeighborID = 103
				};
				if (Game1.options.SnappyMenus)
				{
					base.currentlySnappedComponent = this.purchaseButton;
					this.snapCursorToCurrentSnappedComponent();
				}
			}
			return;
		}
		int numberOfIngredientSlots = b.numberOfIngredientSlots;
		List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
		this.addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
		for (int i = 0; i < ingredientSlotRectangles.Count; i++)
		{
			this.ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[i], this.noteTexture, new Rectangle(512, 244, 18, 18), 4f)
			{
				myID = i + 250,
				upNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		List<Rectangle> ingredientListRectangles = new List<Rectangle>();
		this.addRectangleRowsToList(ingredientListRectangles, b.ingredients.Count, 932, 364);
		for (int j = 0; j < ingredientListRectangles.Count; j++)
		{
			BundleIngredientDescription ingredient = b.ingredients[j];
			string id = JunimoNoteMenu.GetRepresentativeItemId(ingredient);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
			if (itemData.HasTypeObject())
			{
				string displayName = ingredient.category switch
				{
					-2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569"), 
					-75 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570"), 
					-4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571"), 
					-5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572"), 
					-6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573"), 
					_ => itemData.DisplayName, 
				};
				Item item;
				if (ingredient.preservesId != null)
				{
					item = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
					displayName = item.DisplayName;
				}
				else
				{
					item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality);
				}
				Texture2D texture = itemData.GetTexture();
				Rectangle sourceRect = itemData.GetSourceRect();
				this.ingredientList.Add(new ClickableTextureComponent("ingredient_list_slot", ingredientListRectangles[j], "", displayName, texture, sourceRect, 4f)
				{
					myID = j + 1000,
					item = item,
					upNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998
				});
			}
		}
		this.updateIngredientSlots();
		if (!Game1.options.SnappyMenus)
		{
			return;
		}
		this.populateClickableComponentList();
		if (this.inventory?.inventory != null)
		{
			for (int k = 0; k < this.inventory.inventory.Count; k++)
			{
				if (this.inventory.inventory[k] != null)
				{
					if (this.inventory.inventory[k].downNeighborID == 101)
					{
						this.inventory.inventory[k].downNeighborID = -1;
					}
					if (this.inventory.inventory[k].leftNeighborID == -1)
					{
						this.inventory.inventory[k].leftNeighborID = 103;
					}
					if (this.inventory.inventory[k].upNeighborID >= 1000)
					{
						this.inventory.inventory[k].upNeighborID = 103;
					}
				}
			}
		}
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (this.currentPartialIngredientDescriptionIndex >= 0)
		{
			if (this.ingredientSlots.Contains(b) && b.item != this.partialDonationItem)
			{
				return false;
			}
			if (this.ingredientList.Contains(b) && this.ingredientList.IndexOf(b as ClickableTextureComponent) != this.currentPartialIngredientDescriptionIndex)
			{
				return false;
			}
		}
		return (a.myID >= 5000 || a.myID == 101 || a.myID == 102) == (b.myID >= 5000 || b.myID == 101 || b.myID == 102);
	}

	private void addRectangleRowsToList(List<Rectangle> toAddTo, int numberOfItems, int centerX, int centerY)
	{
		switch (numberOfItems)
		{
		case 1:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 1, 72, 72, 12));
			break;
		case 2:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 2, 72, 72, 12));
			break;
		case 3:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 3, 72, 72, 12));
			break;
		case 4:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY, 4, 72, 72, 12));
			break;
		case 5:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
			break;
		case 6:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
			break;
		case 7:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
			break;
		case 8:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
			break;
		case 9:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
			break;
		case 10:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
			break;
		case 11:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
			break;
		case 12:
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
			toAddTo.AddRange(this.createRowOfBoxesCenteredAt(base.xPositionOnScreen + centerX, base.yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
			break;
		}
	}

	private List<Rectangle> createRowOfBoxesCenteredAt(int xStart, int yStart, int numBoxes, int boxWidth, int boxHeight, int horizontalGap)
	{
		List<Rectangle> rectangles = new List<Rectangle>();
		int actualXStart = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
		int actualYStart = yStart - boxHeight / 2;
		for (int i = 0; i < numBoxes; i++)
		{
			rectangles.Add(new Rectangle(actualXStart + i * (boxWidth + horizontalGap), actualYStart, boxWidth, boxHeight));
		}
		return rectangles;
	}

	public void takeDownBundleSpecificPage()
	{
		if (!this.isReadyToCloseMenuOrBundle())
		{
			return;
		}
		this.ReturnPartialDonations(to_hand: false);
		this.hoveredItem = null;
		if (!this.specificBundlePage)
		{
			return;
		}
		this.specificBundlePage = false;
		this.ingredientSlots.Clear();
		this.ingredientList.Clear();
		JunimoNoteMenu.tempSprites.Clear();
		this.purchaseButton = null;
		if (Game1.options.SnappyMenus)
		{
			if (this.currentPageBundle != null)
			{
				base.currentlySnappedComponent = this.currentPageBundle;
				this.snapCursorToCurrentSnappedComponent();
			}
			else
			{
				this.snapToDefaultClickableComponent();
			}
		}
	}

	private Point getBundleLocationFromNumber(int whichBundle)
	{
		Point location = new Point(base.xPositionOnScreen, base.yPositionOnScreen);
		switch (whichBundle)
		{
		case 0:
			location.X += 592;
			location.Y += 136;
			break;
		case 1:
			location.X += 392;
			location.Y += 384;
			break;
		case 2:
			location.X += 784;
			location.Y += 388;
			break;
		case 5:
			location.X += 588;
			location.Y += 276;
			break;
		case 6:
			location.X += 588;
			location.Y += 380;
			break;
		case 3:
			location.X += 304;
			location.Y += 252;
			break;
		case 4:
			location.X += 892;
			location.Y += 252;
			break;
		case 7:
			location.X += 440;
			location.Y += 164;
			break;
		case 8:
			location.X += 776;
			location.Y += 164;
			break;
		}
		return location;
	}
}
