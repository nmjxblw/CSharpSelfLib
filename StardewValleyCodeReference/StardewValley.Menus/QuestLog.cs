using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;

namespace StardewValley.Menus;

public class QuestLog : IClickableMenu
{
	public const int questsPerPage = 6;

	public const int region_forwardButton = 101;

	public const int region_backButton = 102;

	public const int region_rewardBox = 103;

	public const int region_cancelQuestButton = 104;

	protected List<List<IQuest>> pages;

	public List<ClickableComponent> questLogButtons;

	protected int currentPage;

	protected int questPage = -1;

	public ClickableTextureComponent forwardButton;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent rewardBox;

	public ClickableTextureComponent cancelQuestButton;

	protected IQuest _shownQuest;

	protected List<string> _objectiveText;

	protected float _contentHeight;

	protected float _scissorRectHeight;

	public float scrollAmount;

	public ClickableTextureComponent upArrow;

	public ClickableTextureComponent downArrow;

	public ClickableTextureComponent scrollBar;

	protected bool scrolling;

	public Rectangle scrollBarBounds;

	private string hoverText = "";

	public QuestLog()
		: base(0, 0, 0, 0, showUpperRightCloseButton: true)
	{
		Game1.dayTimeMoneyBox.DismissQuestPing();
		Game1.playSound("bigSelect");
		this.paginateQuests();
		base.width = 832;
		base.height = 576;
		if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
		{
			base.height += 64;
		}
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
		base.xPositionOnScreen = (int)topLeft.X;
		base.yPositionOnScreen = (int)topLeft.Y + 32;
		this.questLogButtons = new List<ClickableComponent>();
		for (int i = 0; i < 6; i++)
		{
			this.questLogButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 32) / 6), base.width - 32, (base.height - 32) / 6 + 4), i.ToString() ?? "")
			{
				myID = i,
				downNeighborID = -7777,
				upNeighborID = ((i > 0) ? (i - 1) : (-1)),
				rightNeighborID = -7777,
				leftNeighborID = -7777,
				fullyImmutable = true
			});
		}
		base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 20, base.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 64, base.yPositionOnScreen + 8, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 102,
			rightNeighborID = -7777
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 64 - 48, base.yPositionOnScreen + base.height - 48, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 101
		};
		this.rewardBox = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 80, base.yPositionOnScreen + base.height - 32 - 96, 96, 96), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f, drawShadow: true)
		{
			myID = 103
		};
		this.cancelQuestButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 4, base.yPositionOnScreen + base.height + 4, 48, 48), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 4f, drawShadow: true)
		{
			myID = 104
		};
		int scrollbar_x = base.xPositionOnScreen + base.width + 16;
		this.upArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, base.yPositionOnScreen + 96, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
		this.downArrow = new ClickableTextureComponent(new Rectangle(scrollbar_x, base.yPositionOnScreen + base.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
		this.scrollBarBounds = default(Rectangle);
		this.scrollBarBounds.X = this.upArrow.bounds.X + 12;
		this.scrollBarBounds.Width = 24;
		this.scrollBarBounds.Y = this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4;
		this.scrollBarBounds.Height = this.downArrow.bounds.Y - 4 - this.scrollBarBounds.Y;
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.scrollBarBounds.X, this.scrollBarBounds.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (oldID >= 0 && oldID < 6 && this.questPage == -1)
		{
			switch (direction)
			{
			case 2:
				if (oldID < 5 && this.pages[this.currentPage].Count - 1 > oldID)
				{
					base.currentlySnappedComponent = base.getComponentWithID(oldID + 1);
				}
				break;
			case 1:
				if (this.currentPage < this.pages.Count - 1)
				{
					base.currentlySnappedComponent = base.getComponentWithID(101);
					base.currentlySnappedComponent.leftNeighborID = oldID;
				}
				break;
			case 3:
				if (this.currentPage > 0)
				{
					base.currentlySnappedComponent = base.getComponentWithID(102);
					base.currentlySnappedComponent.rightNeighborID = oldID;
				}
				break;
			}
		}
		else if (oldID == 102)
		{
			if (this.questPage != -1)
			{
				return;
			}
			base.currentlySnappedComponent = base.getComponentWithID(0);
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		switch (button)
		{
		case Buttons.RightTrigger:
			if (this.questPage == -1 && this.currentPage < this.pages.Count - 1)
			{
				this.nonQuestPageForwardButton();
			}
			break;
		case Buttons.LeftTrigger:
			if (this.questPage == -1 && this.currentPage > 0)
			{
				this.nonQuestPageBackButton();
			}
			break;
		}
	}

	/// <summary>Get the paginated list of quests which should be shown in the quest log.</summary>
	protected virtual void paginateQuests()
	{
		this.pages = new List<List<IQuest>>();
		IList<IQuest> quests = this.GetAllQuests();
		int startIndex = 0;
		while (startIndex < quests.Count)
		{
			List<IQuest> page = new List<IQuest>();
			for (int i = 0; i < 6; i++)
			{
				if (startIndex >= quests.Count)
				{
					break;
				}
				page.Add(quests[startIndex]);
				startIndex++;
			}
			this.pages.Add(page);
		}
		if (this.pages.Count == 0)
		{
			this.pages.Add(new List<IQuest>());
		}
		this.currentPage = Utility.Clamp(this.currentPage, 0, this.pages.Count - 1);
		this.questPage = -1;
	}

	/// <summary>Get the quests which should be shown in the quest log.</summary>
	protected virtual IList<IQuest> GetAllQuests()
	{
		List<IQuest> quests = new List<IQuest>();
		for (int i = Game1.player.team.specialOrders.Count - 1; i >= 0; i--)
		{
			SpecialOrder order = Game1.player.team.specialOrders[i];
			if (!order.IsHidden())
			{
				quests.Add(order);
			}
		}
		for (int i2 = Game1.player.questLog.Count - 1; i2 >= 0; i2--)
		{
			Quest quest = Game1.player.questLog[i2];
			if (quest == null || quest.destroy.Value)
			{
				Game1.player.questLog.RemoveAt(i2);
			}
			else if (!quest.IsHidden())
			{
				quests.Add(quest);
			}
		}
		return quests;
	}

	public bool NeedsScroll()
	{
		if (this._shownQuest != null && this._shownQuest.ShouldDisplayAsComplete())
		{
			return false;
		}
		if (this.questPage != -1)
		{
			return this._contentHeight > this._scissorRectHeight;
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		if (this.NeedsScroll())
		{
			float new_scroll = this.scrollAmount - (float)(Math.Sign(direction) * 64 / 2);
			if (new_scroll < 0f)
			{
				new_scroll = 0f;
			}
			if (new_scroll > this._contentHeight - this._scissorRectHeight)
			{
				new_scroll = this._contentHeight - this._scissorRectHeight;
			}
			if (this.scrollAmount != new_scroll)
			{
				this.scrollAmount = new_scroll;
				Game1.playSound("shiny4");
				this.SetScrollBarFromAmount();
			}
		}
		base.receiveScrollWheelAction(direction);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		base.performHoverAction(x, y);
		if (this.questPage == -1)
		{
			for (int i = 0; i < this.questLogButtons.Count; i++)
			{
				if (this.pages.Count > 0 && this.pages[0].Count > i && this.questLogButtons[i].containsPoint(x, y) && !this.questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					Game1.playSound("Cowboy_gunshot");
				}
			}
		}
		else if (this._shownQuest.CanBeCancelled() && this.cancelQuestButton.containsPoint(x, y))
		{
			this.hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11364");
		}
		this.forwardButton.tryHover(x, y, 0.2f);
		this.backButton.tryHover(x, y, 0.2f);
		this.cancelQuestButton.tryHover(x, y, 0.2f);
		if (this.NeedsScroll())
		{
			this.upArrow.tryHover(x, y);
			this.downArrow.tryHover(x, y);
			this.scrollBar.tryHover(x, y);
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.isAnyGamePadButtonBeingPressed() && this.questPage != -1 && Game1.options.doesInputListContain(Game1.options.menuButton, key))
		{
			this.exitQuestPage();
		}
		else
		{
			base.receiveKeyPress(key);
		}
		if (Game1.options.doesInputListContain(Game1.options.journalButton, key) && this.readyToClose())
		{
			Game1.exitActiveMenu();
			Game1.playSound("bigDeSelect");
		}
	}

	private void nonQuestPageForwardButton()
	{
		this.currentPage++;
		Game1.playSound("shwip");
		if (Game1.options.SnappyMenus && this.currentPage == this.pages.Count - 1)
		{
			base.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	private void nonQuestPageBackButton()
	{
		this.currentPage--;
		Game1.playSound("shwip");
		if (Game1.options.SnappyMenus && this.currentPage == 0)
		{
			base.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
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
		}
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		if (!GameMenu.forcePreventClose)
		{
			base.releaseLeftClick(x, y);
			this.scrolling = false;
		}
	}

	public virtual void SetScrollFromY(int y)
	{
		int y2 = this.scrollBar.bounds.Y;
		float percentage = (float)(y - this.scrollBarBounds.Y) / (float)(this.scrollBarBounds.Height - this.scrollBar.bounds.Height);
		percentage = Utility.Clamp(percentage, 0f, 1f);
		this.scrollAmount = percentage * (this._contentHeight - this._scissorRectHeight);
		this.SetScrollBarFromAmount();
		if (y2 != this.scrollBar.bounds.Y)
		{
			Game1.playSound("shiny4");
		}
	}

	public void UpArrowPressed()
	{
		this.upArrow.scale = this.upArrow.baseScale;
		this.scrollAmount -= 64f;
		if (this.scrollAmount < 0f)
		{
			this.scrollAmount = 0f;
		}
		this.SetScrollBarFromAmount();
	}

	public void DownArrowPressed()
	{
		this.downArrow.scale = this.downArrow.baseScale;
		this.scrollAmount += 64f;
		if (this.scrollAmount > this._contentHeight - this._scissorRectHeight)
		{
			this.scrollAmount = this._contentHeight - this._scissorRectHeight;
		}
		this.SetScrollBarFromAmount();
	}

	private void SetScrollBarFromAmount()
	{
		if (!this.NeedsScroll())
		{
			this.scrollAmount = 0f;
			return;
		}
		if (this.scrollAmount < 8f)
		{
			this.scrollAmount = 0f;
		}
		if (this.scrollAmount > this._contentHeight - this._scissorRectHeight - 8f)
		{
			this.scrollAmount = this._contentHeight - this._scissorRectHeight;
		}
		this.scrollBar.bounds.Y = (int)((float)this.scrollBarBounds.Y + (float)(this.scrollBarBounds.Height - this.scrollBar.bounds.Height) / Math.Max(1f, this._contentHeight - this._scissorRectHeight) * this.scrollAmount);
	}

	public override void applyMovementKey(int direction)
	{
		base.applyMovementKey(direction);
		if (this.NeedsScroll())
		{
			switch (direction)
			{
			case 0:
				this.UpArrowPressed();
				break;
			case 2:
				this.DownArrowPressed();
				break;
			}
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (Game1.activeClickableMenu == null)
		{
			return;
		}
		if (this.questPage == -1)
		{
			for (int i = 0; i < this.questLogButtons.Count; i++)
			{
				if (this.pages.Count > 0 && this.pages[this.currentPage].Count > i && this.questLogButtons[i].containsPoint(x, y))
				{
					Game1.playSound("smallSelect");
					this.questPage = i;
					this._shownQuest = this.pages[this.currentPage][i];
					this._objectiveText = this._shownQuest.GetObjectiveDescriptions();
					this._shownQuest.MarkAsViewed();
					this.scrollAmount = 0f;
					this.SetScrollBarFromAmount();
					if (Game1.options.SnappyMenus)
					{
						base.currentlySnappedComponent = base.getComponentWithID(102);
						base.currentlySnappedComponent.rightNeighborID = -7777;
						base.currentlySnappedComponent.downNeighborID = (this.HasMoneyReward() ? 103 : (this._shownQuest.CanBeCancelled() ? 104 : (-1)));
						this.snapCursorToCurrentSnappedComponent();
					}
					return;
				}
			}
			if (this.currentPage < this.pages.Count - 1 && this.forwardButton.containsPoint(x, y))
			{
				this.nonQuestPageForwardButton();
				return;
			}
			if (this.currentPage > 0 && this.backButton.containsPoint(x, y))
			{
				this.nonQuestPageBackButton();
				return;
			}
			Game1.playSound("bigDeSelect");
			base.exitThisMenu();
			return;
		}
		Quest quest = this._shownQuest as Quest;
		int yOffset = ((this._shownQuest.IsTimedQuest() && this._shownQuest.GetDaysLeft() > 0 && SpriteText.getWidthOfString(this._shownQuest.GetName()) > base.width / 2) ? (-48) : 0);
		if (this.questPage != -1 && this._shownQuest.ShouldDisplayAsComplete() && this._shownQuest.HasMoneyReward() && this.rewardBox.containsPoint(x, y + yOffset))
		{
			Game1.player.Money += this._shownQuest.GetMoneyReward();
			Game1.playSound("purchaseRepeat");
			this._shownQuest.OnMoneyRewardClaimed();
		}
		else if (this.questPage != -1 && quest != null && !quest.completed.Value && quest.canBeCancelled.Value && this.cancelQuestButton.containsPoint(x, y))
		{
			quest.accepted.Value = false;
			if (quest.dailyQuest.Value && quest.dayQuestAccepted.Value == Game1.Date.TotalDays)
			{
				Game1.player.acceptedDailyQuest.Set(newValue: false);
			}
			Game1.player.questLog.Remove(quest);
			this.pages[this.currentPage].RemoveAt(this.questPage);
			this.questPage = -1;
			Game1.playSound("trashcan");
			if (Game1.options.SnappyMenus && this.currentPage == 0)
			{
				base.currentlySnappedComponent = base.getComponentWithID(0);
				this.snapCursorToCurrentSnappedComponent();
			}
		}
		else if (!this.NeedsScroll() || this.backButton.containsPoint(x, y))
		{
			this.exitQuestPage();
		}
		if (this.NeedsScroll())
		{
			if (this.downArrow.containsPoint(x, y) && this.scrollAmount < this._contentHeight - this._scissorRectHeight)
			{
				this.DownArrowPressed();
				Game1.playSound("shwip");
			}
			else if (this.upArrow.containsPoint(x, y) && this.scrollAmount > 0f)
			{
				this.UpArrowPressed();
				Game1.playSound("shwip");
			}
			else if (this.scrollBar.containsPoint(x, y))
			{
				this.scrolling = true;
			}
			else if (this.scrollBarBounds.Contains(x, y))
			{
				this.scrolling = true;
			}
			else if (!this.downArrow.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
			{
				this.scrolling = true;
				this.leftClickHeld(x, y);
				this.releaseLeftClick(x, y);
			}
		}
	}

	public bool HasReward()
	{
		return this._shownQuest.HasReward();
	}

	public bool HasMoneyReward()
	{
		return this._shownQuest.HasMoneyReward();
	}

	public void exitQuestPage()
	{
		if (this._shownQuest.OnLeaveQuestPage())
		{
			this.pages[this.currentPage].RemoveAt(this.questPage);
		}
		this.questPage = -1;
		this.paginateQuests();
		Game1.playSound("shwip");
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.questPage != -1 && this.HasReward())
		{
			this.rewardBox.scale = this.rewardBox.baseScale + Game1.dialogueButtonScale / 20f;
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11373"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
		if (this.questPage == -1)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);
			for (int i = 0; i < this.questLogButtons.Count; i++)
			{
				if (this.pages.Count > 0 && this.pages[this.currentPage].Count > i)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), this.questLogButtons[i].bounds.X, this.questLogButtons[i].bounds.Y, this.questLogButtons[i].bounds.Width, this.questLogButtons[i].bounds.Height, this.questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
					if (this.pages[this.currentPage][i].ShouldDisplayAsNew() || this.pages[this.currentPage][i].ShouldDisplayAsComplete())
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(this.questLogButtons[i].bounds.X + 64 + 4, this.questLogButtons[i].bounds.Y + 44), new Rectangle(this.pages[this.currentPage][i].ShouldDisplayAsComplete() ? 341 : 317, 410, 23, 9), Color.White, 0f, new Vector2(11f, 4f), 4f + Game1.dialogueButtonScale * 10f / 250f, flipped: false, 0.99f);
					}
					else
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(this.questLogButtons[i].bounds.X + 32, this.questLogButtons[i].bounds.Y + 28), this.pages[this.currentPage][i].IsTimedQuest() ? new Rectangle(410, 501, 9, 9) : new Rectangle(395 + (this.pages[this.currentPage][i].IsTimedQuest() ? 3 : 0), 497, 3, 8), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
					}
					this.pages[this.currentPage][i].IsTimedQuest();
					SpriteText.drawString(b, this.pages[this.currentPage][i].GetName(), this.questLogButtons[i].bounds.X + 128 + 4, this.questLogButtons[i].bounds.Y + 20);
				}
			}
		}
		else
		{
			int titleWidth = SpriteText.getWidthOfString(this._shownQuest.GetName());
			if (titleWidth > base.width / 2)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height + (this._shownQuest.ShouldDisplayAsComplete() ? 48 : 0), Color.White, 4f);
				SpriteText.drawStringHorizontallyCenteredAt(b, this._shownQuest.GetName(), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + 32);
			}
			else
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);
				SpriteText.drawStringHorizontallyCenteredAt(b, this._shownQuest.GetName(), base.xPositionOnScreen + base.width / 2 + ((this._shownQuest.IsTimedQuest() && this._shownQuest.GetDaysLeft() > 0) ? (Math.Max(32, SpriteText.getWidthOfString(this._shownQuest.GetName()) / 3) - 32) : 0), base.yPositionOnScreen + 32);
			}
			float extraYOffset = 0f;
			if (this._shownQuest.IsTimedQuest() && this._shownQuest.GetDaysLeft() > 0)
			{
				int xOffset = 0;
				if (titleWidth > base.width / 2)
				{
					xOffset = 28;
					extraYOffset = 48f;
				}
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(base.xPositionOnScreen + xOffset + 32, (float)(base.yPositionOnScreen + 48 - 8) + extraYOffset), new Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
				Utility.drawTextWithShadow(b, Game1.parseText((this.pages[this.currentPage][this.questPage].GetDaysLeft() > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", this.pages[this.currentPage][this.questPage].GetDaysLeft()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest_FinalDay"), Game1.dialogueFont, base.width - 128), Game1.dialogueFont, new Vector2(base.xPositionOnScreen + xOffset + 80, (float)(base.yPositionOnScreen + 48 - 8) + extraYOffset), Game1.textColor);
			}
			string description = Game1.parseText(this._shownQuest.GetDescription(), Game1.dialogueFont, base.width - 128);
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			Vector2 description_size = Game1.dialogueFont.MeasureString(description);
			Rectangle scissor_rect = default(Rectangle);
			scissor_rect.X = base.xPositionOnScreen + 32;
			scissor_rect.Y = base.yPositionOnScreen + 96 + (int)extraYOffset;
			scissor_rect.Height = base.yPositionOnScreen + base.height - 32 - scissor_rect.Y;
			scissor_rect.Width = base.width - 64;
			this._scissorRectHeight = scissor_rect.Height;
			scissor_rect = Utility.ConstrainScissorRectToScreen(scissor_rect);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState
			{
				ScissorTestEnable = true
			});
			Game1.graphics.GraphicsDevice.ScissorRectangle = scissor_rect;
			Utility.drawTextWithShadow(b, description, Game1.dialogueFont, new Vector2(base.xPositionOnScreen + 64, (float)base.yPositionOnScreen - this.scrollAmount + 96f + extraYOffset), Game1.textColor);
			float yPos = (float)(base.yPositionOnScreen + 96) + description_size.Y + 32f - this.scrollAmount + extraYOffset;
			if (this._shownQuest.ShouldDisplayAsComplete())
			{
				b.End();
				b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11376"), base.xPositionOnScreen + 32 + 4, this.rewardBox.bounds.Y + 21 + 4 + (int)extraYOffset);
				this.rewardBox.draw(b, Color.White, 0.9f, 0, 0, (int)extraYOffset);
				if (this.HasMoneyReward())
				{
					b.Draw(Game1.mouseCursors, new Vector2(this.rewardBox.bounds.X + 16, (float)(this.rewardBox.bounds.Y + 16) - Game1.dialogueButtonScale / 2f + extraYOffset), new Rectangle(280, 410, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this._shownQuest.GetMoneyReward()), base.xPositionOnScreen + 448, this.rewardBox.bounds.Y + 21 + 4 + (int)extraYOffset);
				}
			}
			else
			{
				for (int j = 0; j < this._objectiveText.Count; j++)
				{
					string parsed_text = Game1.parseText(this._objectiveText[j], width: base.width - 192, whichFont: Game1.dialogueFont);
					bool num = this._shownQuest is SpecialOrder o && o.objectives[j].IsComplete();
					Color text_color = Game1.unselectedOptionColor;
					if (!num)
					{
						text_color = Color.DarkBlue;
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(base.xPositionOnScreen + 96) + 8f * Game1.dialogueButtonScale / 10f, yPos), new Rectangle(412, 495, 5, 4), Color.White, (float)Math.PI / 2f, Vector2.Zero);
					}
					Utility.drawTextWithShadow(b, parsed_text, Game1.dialogueFont, new Vector2(base.xPositionOnScreen + 128, yPos - 8f), text_color);
					yPos += Game1.dialogueFont.MeasureString(parsed_text).Y;
					if (this._shownQuest is SpecialOrder order)
					{
						OrderObjective order_objective = order.objectives[j];
						if (order_objective.GetMaxCount() > 1 && order_objective.ShouldShowProgress())
						{
							Color dark_bar_color = Color.DarkRed;
							Color bar_color = Color.Red;
							if (order_objective.GetCount() >= order_objective.GetMaxCount())
							{
								bar_color = Color.LimeGreen;
								dark_bar_color = Color.Green;
							}
							int inset = 64;
							int objective_count_draw_width = 160;
							int notches = 4;
							Rectangle bar_background_source = new Rectangle(0, 224, 47, 12);
							Rectangle bar_notch_source = new Rectangle(47, 224, 1, 12);
							int bar_horizontal_padding = 3;
							int bar_vertical_padding = 3;
							int slice_width = 5;
							string objective_count_text = order_objective.GetCount() + "/" + order_objective.GetMaxCount();
							int max_text_width = (int)Game1.dialogueFont.MeasureString(order_objective.GetMaxCount() + "/" + order_objective.GetMaxCount()).X;
							int count_text_width = (int)Game1.dialogueFont.MeasureString(objective_count_text).X;
							int text_draw_position = base.xPositionOnScreen + base.width - inset - count_text_width;
							int max_text_draw_position = base.xPositionOnScreen + base.width - inset - max_text_width;
							Utility.drawTextWithShadow(b, objective_count_text, Game1.dialogueFont, new Vector2(text_draw_position, yPos), Color.DarkBlue);
							Rectangle bar_draw_position = new Rectangle(base.xPositionOnScreen + inset, (int)yPos, base.width - inset * 2 - objective_count_draw_width, bar_background_source.Height * 4);
							if (bar_draw_position.Right > max_text_draw_position - 16)
							{
								int adjustment = bar_draw_position.Right - (max_text_draw_position - 16);
								bar_draw_position.Width -= adjustment;
							}
							b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.X, bar_draw_position.Y, slice_width * 4, bar_draw_position.Height), new Rectangle(bar_background_source.X, bar_background_source.Y, slice_width, bar_background_source.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
							b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.X + slice_width * 4, bar_draw_position.Y, bar_draw_position.Width - 2 * slice_width * 4, bar_draw_position.Height), new Rectangle(bar_background_source.X + slice_width, bar_background_source.Y, bar_background_source.Width - 2 * slice_width, bar_background_source.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
							b.Draw(Game1.mouseCursors2, new Rectangle(bar_draw_position.Right - slice_width * 4, bar_draw_position.Y, slice_width * 4, bar_draw_position.Height), new Rectangle(bar_background_source.Right - slice_width, bar_background_source.Y, slice_width, bar_background_source.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
							float quest_progress = (float)order_objective.GetCount() / (float)order_objective.GetMaxCount();
							if (order_objective.GetMaxCount() < notches)
							{
								notches = order_objective.GetMaxCount();
							}
							bar_draw_position.X += 4 * bar_horizontal_padding;
							bar_draw_position.Width -= 4 * bar_horizontal_padding * 2;
							for (int n = 1; n < notches; n++)
							{
								b.Draw(Game1.mouseCursors2, new Vector2((float)bar_draw_position.X + (float)bar_draw_position.Width * ((float)n / (float)notches), bar_draw_position.Y), bar_notch_source, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
							}
							bar_draw_position.Y += 4 * bar_vertical_padding;
							bar_draw_position.Height -= 4 * bar_vertical_padding * 2;
							Rectangle rect = new Rectangle(bar_draw_position.X, bar_draw_position.Y, (int)((float)bar_draw_position.Width * quest_progress) - 4, bar_draw_position.Height);
							b.Draw(Game1.staminaRect, rect, null, bar_color, 0f, Vector2.Zero, SpriteEffects.None, (float)rect.Y / 10000f);
							rect.X = rect.Right;
							rect.Width = 4;
							b.Draw(Game1.staminaRect, rect, null, dark_bar_color, 0f, Vector2.Zero, SpriteEffects.None, (float)rect.Y / 10000f);
							yPos += (float)((bar_background_source.Height + 4) * 4);
						}
					}
					this._contentHeight = yPos + this.scrollAmount - (float)scissor_rect.Y;
				}
				b.End();
				b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				if (this._shownQuest.CanBeCancelled())
				{
					this.cancelQuestButton.draw(b);
				}
				if (this.NeedsScroll())
				{
					if (this.scrollAmount > 0f)
					{
						b.Draw(Game1.staminaRect, new Rectangle(scissor_rect.X, scissor_rect.Top, scissor_rect.Width, 4), Color.Black * 0.15f);
					}
					if (this.scrollAmount < this._contentHeight - this._scissorRectHeight)
					{
						b.Draw(Game1.staminaRect, new Rectangle(scissor_rect.X, scissor_rect.Bottom - 4, scissor_rect.Width, 4), Color.Black * 0.15f);
					}
				}
			}
		}
		if (this.NeedsScroll())
		{
			this.upArrow.draw(b);
			this.downArrow.draw(b);
			this.scrollBar.draw(b);
		}
		if (this.currentPage < this.pages.Count - 1 && this.questPage == -1)
		{
			this.forwardButton.draw(b);
		}
		if (this.currentPage > 0 || this.questPage != -1)
		{
			this.backButton.draw(b);
		}
		base.draw(b);
		Game1.mouseCursorTransparency = 1f;
		base.drawMouse(b);
		if (this.hoverText.Length > 0)
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont);
		}
	}
}
