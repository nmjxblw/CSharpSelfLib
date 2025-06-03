using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;

namespace StardewValley.Menus;

public class LevelUpMenu : IClickableMenu
{
	public const int region_okButton = 101;

	public const int region_leftProfession = 102;

	public const int region_rightProfession = 103;

	public const int basewidth = 768;

	public const int baseheight = 512;

	public bool informationUp;

	public bool isActive;

	public bool isProfessionChooser;

	public bool hasUpdatedProfessions;

	private int currentLevel;

	private int currentSkill;

	private int timerBeforeStart;

	private Color leftProfessionColor = Game1.textColor;

	private Color rightProfessionColor = Game1.textColor;

	private MouseState oldMouseState;

	public ClickableTextureComponent starIcon;

	public ClickableTextureComponent okButton;

	public ClickableComponent leftProfession;

	public ClickableComponent rightProfession;

	private List<CraftingRecipe> newCraftingRecipes = new List<CraftingRecipe>();

	private List<string> extraInfoForLevel = new List<string>();

	private List<string> leftProfessionDescription = new List<string>();

	private List<string> rightProfessionDescription = new List<string>();

	private Rectangle sourceRectForLevelIcon;

	private string title;

	private List<int> professionsToChoose = new List<int>();

	private TemporaryAnimatedSpriteList littleStars = new TemporaryAnimatedSpriteList();

	public bool hasMovedSelection;

	public LevelUpMenu()
		: base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
	{
		Game1.player.team.endOfNightStatus.UpdateState("level");
		base.width = 768;
		base.height = 512;
		this.okButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101
		};
		this.RepositionOkButton();
	}

	public LevelUpMenu(int skill, int level)
		: base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
	{
		Game1.player.team.endOfNightStatus.UpdateState("level");
		this.timerBeforeStart = 250;
		this.isActive = true;
		base.width = 960;
		base.height = 512;
		this.okButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101
		};
		this.informationUp = true;
		this.isProfessionChooser = (level == 5 || level == 10) && skill != 5;
		this.currentLevel = level;
		this.currentSkill = skill;
		this.title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", level, Farmer.getSkillDisplayNameFromIndex(skill));
		this.extraInfoForLevel = this.getExtraInfoForLevel(skill, level);
		Game1.player.completelyStopAnimatingOrDoingAction();
		Game1.player.stats.checkForSkillAchievements(isDirectUnlock: true);
		Game1.player.AddMissedMailAndRecipes();
		switch (skill)
		{
		case 0:
			this.sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
			break;
		case 1:
			this.sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
			break;
		case 3:
			this.sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
			break;
		case 2:
			this.sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
			break;
		case 4:
			this.sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
			break;
		case 5:
			this.sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
			break;
		}
		int newHeight = 0;
		foreach (KeyValuePair<string, string> v in CraftingRecipe.craftingRecipes)
		{
			if (CraftingRecipe.TryParseLevelRequirement(v.Key, v.Value, isCooking: false, out var reqSkillNumber, out var minLevel) && reqSkillNumber == skill && minLevel == level)
			{
				CraftingRecipe recipe = new CraftingRecipe(v.Key, isCookingRecipe: false);
				this.newCraftingRecipes.Add(recipe);
				newHeight += (recipe.bigCraftable ? 128 : 64);
			}
		}
		foreach (KeyValuePair<string, string> v2 in CraftingRecipe.cookingRecipes)
		{
			if (CraftingRecipe.TryParseLevelRequirement(v2.Key, v2.Value, isCooking: true, out var reqSkillNumber2, out var minLevel2) && reqSkillNumber2 == skill && minLevel2 == level)
			{
				CraftingRecipe recipe2 = new CraftingRecipe(v2.Key, isCookingRecipe: true);
				this.newCraftingRecipes.Add(recipe2);
				newHeight += (recipe2.bigCraftable ? 128 : 64);
			}
		}
		base.height = newHeight + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
		Game1.player.freezePause = 100;
		this.gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
		if (this.isProfessionChooser)
		{
			this.leftProfession = new ClickableComponent(new Rectangle(base.xPositionOnScreen, base.yPositionOnScreen + 128, base.width / 2, base.height), "")
			{
				myID = 102,
				rightNeighborID = 103
			};
			this.rightProfession = new ClickableComponent(new Rectangle(base.width / 2 + base.xPositionOnScreen, base.yPositionOnScreen + 128, base.width / 2, base.height), "")
			{
				myID = 103,
				leftNeighborID = 102
			};
		}
		this.populateClickableComponentList();
	}

	public bool CanReceiveInput()
	{
		if (!this.informationUp)
		{
			return false;
		}
		if (this.timerBeforeStart > 0)
		{
			return false;
		}
		return true;
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.isProfessionChooser)
		{
			base.currentlySnappedComponent = base.getComponentWithID(103);
			Game1.setMousePosition(base.xPositionOnScreen + base.width + 64, base.yPositionOnScreen + base.height + 64);
		}
		else
		{
			base.currentlySnappedComponent = base.getComponentWithID(101);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	public override void applyMovementKey(int direction)
	{
		if (this.CanReceiveInput())
		{
			if (direction == 3 || direction == 1)
			{
				this.hasMovedSelection = true;
			}
			base.applyMovementKey(direction);
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - base.width / 2;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - base.height / 2;
		this.RepositionOkButton();
	}

	public virtual void RepositionOkButton()
	{
		this.okButton.bounds = new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth, 64, 64);
		if (this.okButton.bounds.Right > Game1.uiViewport.Width)
		{
			this.okButton.bounds.X = Game1.uiViewport.Width - 64;
		}
		if (this.okButton.bounds.Bottom > Game1.uiViewport.Height)
		{
			this.okButton.bounds.Y = Game1.uiViewport.Height - 64;
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	public List<string> getExtraInfoForLevel(int whichSkill, int whichLevel)
	{
		List<string> extraInfo = new List<string>();
		switch (whichSkill)
		{
		case 0:
			extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming1"));
			extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming2"));
			break;
		case 3:
			extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Mining"));
			break;
		case 1:
			extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Fishing"));
			break;
		case 2:
			extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging1"));
			switch (whichLevel)
			{
			case 1:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging2"));
				break;
			case 4:
			case 8:
				extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging3"));
				break;
			}
			break;
		case 4:
			extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Combat"));
			break;
		case 5:
			extraInfo.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Luck"));
			break;
		}
		return extraInfo;
	}

	private static void addProfessionDescriptions(List<string> descriptions, string professionName)
	{
		descriptions.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + professionName));
		descriptions.AddRange(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionDescription_" + professionName).Split('\n'));
	}

	private static string getProfessionName(int whichProfession)
	{
		return whichProfession switch
		{
			0 => "Rancher", 
			1 => "Tiller", 
			2 => "Coopmaster", 
			3 => "Shepherd", 
			4 => "Artisan", 
			5 => "Agriculturist", 
			6 => "Fisher", 
			7 => "Trapper", 
			8 => "Angler", 
			9 => "Pirate", 
			10 => "Mariner", 
			11 => "Luremaster", 
			12 => "Forester", 
			13 => "Gatherer", 
			14 => "Lumberjack", 
			15 => "Tapper", 
			16 => "Botanist", 
			17 => "Tracker", 
			18 => "Miner", 
			19 => "Geologist", 
			20 => "Blacksmith", 
			21 => "Prospector", 
			22 => "Excavator", 
			23 => "Gemologist", 
			24 => "Fighter", 
			25 => "Scout", 
			26 => "Brute", 
			27 => "Defender", 
			28 => "Acrobat", 
			_ => "Desperado", 
		};
	}

	public static List<string> getProfessionDescription(int whichProfession)
	{
		List<string> list = new List<string>();
		LevelUpMenu.addProfessionDescriptions(list, LevelUpMenu.getProfessionName(whichProfession));
		return list;
	}

	public static string getProfessionTitleFromNumber(int whichProfession)
	{
		return Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + LevelUpMenu.getProfessionName(whichProfession));
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if ((button == Buttons.Start || button == Buttons.B) && !this.isProfessionChooser && this.isActive)
		{
			this.okButtonClicked();
		}
	}

	public static void AddMissedProfessionChoices(Farmer farmer)
	{
		int[] skills = new int[5] { 0, 1, 2, 3, 4 };
		foreach (int skill in skills)
		{
			if (farmer.GetUnmodifiedSkillLevel(skill) >= 5 && !farmer.newLevels.Contains(new Point(skill, 5)) && farmer.getProfessionForSkill(skill, 5) == -1)
			{
				farmer.newLevels.Add(new Point(skill, 5));
			}
			if (farmer.GetUnmodifiedSkillLevel(skill) >= 10 && !farmer.newLevels.Contains(new Point(skill, 10)) && farmer.getProfessionForSkill(skill, 10) == -1)
			{
				farmer.newLevels.Add(new Point(skill, 10));
			}
		}
	}

	public static void removeImmediateProfessionPerk(int whichProfession)
	{
		switch (whichProfession)
		{
		case 24:
			Game1.player.maxHealth -= 15;
			break;
		case 27:
			Game1.player.maxHealth -= 25;
			break;
		}
		if (Game1.player.health > Game1.player.maxHealth)
		{
			Game1.player.health = Game1.player.maxHealth;
		}
	}

	public void getImmediateProfessionPerk(int whichProfession)
	{
		switch (whichProfession)
		{
		case 24:
			Game1.player.maxHealth += 15;
			break;
		case 27:
			Game1.player.maxHealth += 25;
			break;
		}
		Game1.player.health = Game1.player.maxHealth;
		Game1.player.stamina = Game1.player.MaxStamina;
	}

	public static void RevalidateHealth(Farmer farmer)
	{
		int expected_max_health = 100;
		if (farmer.mailReceived.Contains("qiCave"))
		{
			expected_max_health += 25;
		}
		for (int i = 1; i <= farmer.GetUnmodifiedSkillLevel(4); i++)
		{
			if (!farmer.newLevels.Contains(new Point(4, i)) && i != 5 && i != 10)
			{
				expected_max_health += 5;
			}
		}
		if (farmer.professions.Contains(24))
		{
			expected_max_health += 15;
		}
		if (farmer.professions.Contains(27))
		{
			expected_max_health += 25;
		}
		if (farmer.maxHealth < expected_max_health)
		{
			Game1.log.Verbose("Fixing max health of: " + farmer.Name + " was " + farmer.maxHealth + " (expected: " + expected_max_health + ")");
			int difference = expected_max_health - farmer.maxHealth;
			farmer.maxHealth = expected_max_health;
			farmer.health += difference;
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (!this.isActive)
		{
			base.exitThisMenu();
			return;
		}
		if (this.isProfessionChooser && !this.hasUpdatedProfessions)
		{
			if (this.currentLevel == 5)
			{
				this.professionsToChoose.Add(this.currentSkill * 6);
				this.professionsToChoose.Add(this.currentSkill * 6 + 1);
			}
			else if (Game1.player.professions.Contains(this.currentSkill * 6))
			{
				this.professionsToChoose.Add(this.currentSkill * 6 + 2);
				this.professionsToChoose.Add(this.currentSkill * 6 + 3);
			}
			else
			{
				this.professionsToChoose.Add(this.currentSkill * 6 + 4);
				this.professionsToChoose.Add(this.currentSkill * 6 + 5);
			}
			this.leftProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
			this.rightProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
			this.hasUpdatedProfessions = true;
		}
		this.littleStars.RemoveWhere((TemporaryAnimatedSprite star) => star.update(time));
		if (Game1.random.NextDouble() < 0.03)
		{
			Vector2 position = new Vector2(0f, Game1.random.Next(base.yPositionOnScreen - 128, base.yPositionOnScreen - 4) / 20 * 4 * 5 + 32);
			if (Game1.random.NextBool())
			{
				position.X = Game1.random.Next(base.xPositionOnScreen + base.width / 2 - 228, base.xPositionOnScreen + base.width / 2 - 132);
			}
			else
			{
				position.X = Game1.random.Next(base.xPositionOnScreen + base.width / 2 + 116, base.xPositionOnScreen + base.width - 160);
			}
			if (position.Y < (float)(base.yPositionOnScreen - 64 - 8))
			{
				position.X = Game1.random.Next(base.xPositionOnScreen + base.width / 2 - 116, base.xPositionOnScreen + base.width / 2 + 116);
			}
			position.X = position.X / 20f * 4f * 5f;
			this.littleStars.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(364, 79, 5, 5), 80f, 7, 1, position, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				local = true
			});
		}
		if (this.timerBeforeStart > 0)
		{
			this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
			if (this.timerBeforeStart <= 0 && Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
			return;
		}
		if (this.isActive && this.isProfessionChooser)
		{
			this.leftProfessionColor = Game1.textColor;
			this.rightProfessionColor = Game1.textColor;
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.player.freezePause = 100;
			if (Game1.getMouseY() > base.yPositionOnScreen + 192 && Game1.getMouseY() < base.yPositionOnScreen + base.height)
			{
				if (Game1.getMouseX() > base.xPositionOnScreen && Game1.getMouseX() < base.xPositionOnScreen + base.width / 2)
				{
					this.leftProfessionColor = Color.Green;
					if (((Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
					{
						Game1.player.professions.Add(this.professionsToChoose[0]);
						this.getImmediateProfessionPerk(this.professionsToChoose[0]);
						this.isActive = false;
						this.informationUp = false;
						this.isProfessionChooser = false;
						this.RemoveLevelFromLevelList();
					}
				}
				else if (Game1.getMouseX() > base.xPositionOnScreen + base.width / 2 && Game1.getMouseX() < base.xPositionOnScreen + base.width)
				{
					this.rightProfessionColor = Color.Green;
					if (((Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
					{
						Game1.player.professions.Add(this.professionsToChoose[1]);
						this.getImmediateProfessionPerk(this.professionsToChoose[1]);
						this.isActive = false;
						this.informationUp = false;
						this.isProfessionChooser = false;
						this.RemoveLevelFromLevelList();
					}
				}
			}
			base.height = 512;
		}
		this.oldMouseState = Game1.input.GetMouseState();
		if (this.isActive && !this.informationUp && this.starIcon != null)
		{
			if (this.starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				this.starIcon.sourceRect.X = 294;
			}
			else
			{
				this.starIcon.sourceRect.X = 310;
			}
		}
		if (this.isActive && this.starIcon != null && !this.informationUp && (this.oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && this.starIcon.containsPoint(this.oldMouseState.X, this.oldMouseState.Y))
		{
			this.newCraftingRecipes.Clear();
			this.extraInfoForLevel.Clear();
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.playSound("bigSelect");
			this.informationUp = true;
			this.isProfessionChooser = false;
			Point newLevel = Game1.player.newLevels[0];
			this.currentLevel = newLevel.Y;
			this.currentSkill = newLevel.X;
			this.title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", this.currentLevel, Farmer.getSkillDisplayNameFromIndex(this.currentSkill));
			this.extraInfoForLevel = this.getExtraInfoForLevel(this.currentSkill, this.currentLevel);
			switch (this.currentSkill)
			{
			case 0:
				this.sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
				break;
			case 1:
				this.sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
				break;
			case 3:
				this.sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
				break;
			case 2:
				this.sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
				break;
			case 4:
				this.sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
				break;
			case 5:
				this.sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
				break;
			}
			if ((this.currentLevel == 5 || this.currentLevel == 10) && this.currentSkill != 5)
			{
				this.professionsToChoose.Clear();
				this.isProfessionChooser = true;
				if (this.currentLevel == 5)
				{
					this.professionsToChoose.Add(this.currentSkill * 6);
					this.professionsToChoose.Add(this.currentSkill * 6 + 1);
				}
				else if (Game1.player.professions.Contains(this.currentSkill * 6))
				{
					this.professionsToChoose.Add(this.currentSkill * 6 + 2);
					this.professionsToChoose.Add(this.currentSkill * 6 + 3);
				}
				else
				{
					this.professionsToChoose.Add(this.currentSkill * 6 + 4);
					this.professionsToChoose.Add(this.currentSkill * 6 + 5);
				}
				this.leftProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
				this.rightProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
			}
			int newHeight = 0;
			foreach (KeyValuePair<string, string> v in CraftingRecipe.craftingRecipes)
			{
				string conditions = ArgUtility.Get(v.Value.Split('/'), 4, "");
				if (conditions.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && conditions.Contains(this.currentLevel.ToString() ?? ""))
				{
					CraftingRecipe recipe = new CraftingRecipe(v.Key, isCookingRecipe: false);
					this.newCraftingRecipes.Add(recipe);
					Game1.player.craftingRecipes.TryAdd(v.Key, 0);
					newHeight += (recipe.bigCraftable ? 128 : 64);
				}
			}
			foreach (KeyValuePair<string, string> v2 in CraftingRecipe.cookingRecipes)
			{
				string conditions2 = ArgUtility.Get(v2.Value.Split('/'), 3, "");
				if (conditions2.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && conditions2.Contains(this.currentLevel.ToString() ?? ""))
				{
					CraftingRecipe recipe2 = new CraftingRecipe(v2.Key, isCookingRecipe: true);
					this.newCraftingRecipes.Add(recipe2);
					if (!Game1.player.cookingRecipes.ContainsKey(v2.Key))
					{
						Game1.player.cookingRecipes.Add(v2.Key, 0);
					}
					newHeight += (recipe2.bigCraftable ? 128 : 64);
				}
			}
			base.height = newHeight + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
			Game1.player.freezePause = 100;
		}
		if (!this.isActive || !this.informationUp)
		{
			return;
		}
		Game1.player.completelyStopAnimatingOrDoingAction();
		if (this.okButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !this.isProfessionChooser)
		{
			this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
			if ((this.oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
			{
				this.okButtonClicked();
			}
		}
		else
		{
			this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
		}
		Game1.player.freezePause = 100;
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		if (this.isActive)
		{
			this.okButtonClicked();
		}
	}

	public void okButtonClicked()
	{
		this.getLevelPerk(this.currentSkill, this.currentLevel);
		this.RemoveLevelFromLevelList();
		this.isActive = false;
		this.informationUp = false;
	}

	public virtual void RemoveLevelFromLevelList()
	{
		Game1.player.newLevels.RemoveWhere((Point level) => level.X == this.currentSkill && level.Y == this.currentLevel);
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if ((!Game1.options.doesInputListContain(Game1.options.cancelButton, key) && !Game1.options.doesInputListContain(Game1.options.menuButton, key)) || !this.isProfessionChooser)
		{
			base.receiveKeyPress(key);
		}
	}

	public void getLevelPerk(int skill, int level)
	{
		switch (skill)
		{
		case 4:
			Game1.player.maxHealth += 5;
			break;
		case 1:
			switch (level)
			{
			case 2:
				if (!Game1.player.hasOrWillReceiveMail("fishing2"))
				{
					Game1.addMailForTomorrow("fishing2");
				}
				break;
			case 6:
				if (!Game1.player.hasOrWillReceiveMail("fishing6"))
				{
					Game1.addMailForTomorrow("fishing6");
				}
				break;
			}
			break;
		}
		Game1.player.health = Game1.player.maxHealth;
		Game1.player.Stamina = Game1.player.maxStamina.Value;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (this.timerBeforeStart > 0)
		{
			return;
		}
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		foreach (TemporaryAnimatedSprite littleStar in this.littleStars)
		{
			littleStar.draw(b);
		}
		b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + base.width / 2 - 116, base.yPositionOnScreen - 32 + 12), new Rectangle(363, 87, 58, 22), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		if (!this.informationUp && this.isActive && this.starIcon != null)
		{
			this.starIcon.draw(b);
		}
		else
		{
			if (!this.informationUp)
			{
				return;
			}
			if (this.isProfessionChooser)
			{
				if (this.professionsToChoose.Count == 0)
				{
					return;
				}
				Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
				base.drawHorizontalPartition(b, base.yPositionOnScreen + 192);
				base.drawVerticalIntersectingPartition(b, base.xPositionOnScreen + base.width / 2 - 32, base.yPositionOnScreen + 192);
				Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
				b.DrawString(Game1.dialogueFont, this.title, new Vector2((float)(base.xPositionOnScreen + base.width / 2) - Game1.dialogueFont.MeasureString(this.title).X / 2f, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
				Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
				string chooseProfession = Game1.content.LoadString("Strings\\UI:LevelUp_ChooseProfession");
				b.DrawString(Game1.smallFont, chooseProfession, new Vector2((float)(base.xPositionOnScreen + base.width / 2) - Game1.smallFont.MeasureString(chooseProfession).X / 2f, base.yPositionOnScreen + 64 + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
				b.DrawString(Game1.dialogueFont, this.leftProfessionDescription[0], new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), this.leftProfessionColor);
				b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width / 2 - 112, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(this.professionsToChoose[0] % 6 * 16, 624 + this.professionsToChoose[0] / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				for (int i = 1; i < this.leftProfessionDescription.Count; i++)
				{
					b.DrawString(Game1.smallFont, Game1.parseText(this.leftProfessionDescription[i], Game1.smallFont, base.width / 2 - 64), new Vector2(-4 + base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (i + 1)), this.leftProfessionColor);
				}
				b.DrawString(Game1.dialogueFont, this.rightProfessionDescription[0], new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width / 2, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), this.rightProfessionColor);
				b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width - 128, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(this.professionsToChoose[1] % 6 * 16, 624 + this.professionsToChoose[1] / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				for (int j = 1; j < this.rightProfessionDescription.Count; j++)
				{
					b.DrawString(Game1.smallFont, Game1.parseText(this.rightProfessionDescription[j], Game1.smallFont, base.width / 2 - 48), new Vector2(-4 + base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width / 2, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (j + 1)), this.rightProfessionColor);
				}
			}
			else
			{
				Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
				Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
				b.DrawString(Game1.dialogueFont, this.title, new Vector2((float)(base.xPositionOnScreen + base.width / 2) - Game1.dialogueFont.MeasureString(this.title).X / 2f, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
				Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
				int y = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 80;
				foreach (string s in this.extraInfoForLevel)
				{
					b.DrawString(Game1.smallFont, s, new Vector2((float)(base.xPositionOnScreen + base.width / 2) - Game1.smallFont.MeasureString(s).X / 2f, y), Game1.textColor);
					y += 48;
				}
				foreach (CraftingRecipe s2 in this.newCraftingRecipes)
				{
					string cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_" + (s2.isCookingRecipe ? "cooking" : "crafting"));
					string message = Game1.content.LoadString("Strings\\UI:LevelUp_NewRecipe", cookingOrCrafting, s2.DisplayName);
					b.DrawString(Game1.smallFont, message, new Vector2((float)(base.xPositionOnScreen + base.width / 2) - Game1.smallFont.MeasureString(message).X / 2f - 64f, y + (s2.bigCraftable ? 38 : 12)), Game1.textColor);
					s2.drawMenuView(b, (int)((float)(base.xPositionOnScreen + base.width / 2) + Game1.smallFont.MeasureString(message).X / 2f - 48f), y - 16);
					y += (s2.bigCraftable ? 128 : 64) + 8;
				}
				this.okButton.draw(b);
			}
			if (!Game1.options.SnappyMenus || !this.isProfessionChooser || this.hasMovedSelection)
			{
				Game1.mouseCursorTransparency = 1f;
				base.drawMouse(b);
			}
		}
	}
}
