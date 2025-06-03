using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Triggers;

namespace StardewValley.Menus;

public class LetterViewerMenu : IClickableMenu
{
	public const int region_backButton = 101;

	public const int region_forwardButton = 102;

	public const int region_acceptQuestButton = 103;

	public const int region_itemGrabButton = 104;

	public const int letterWidth = 320;

	public const int letterHeight = 180;

	public Texture2D letterTexture;

	public Texture2D secretNoteImageTexture;

	public int moneyIncluded;

	public int secretNoteImage = -1;

	public int whichBG;

	/// <summary>The ID of the quest attached to the letter being viewed, if any.</summary>
	public string questID;

	/// <summary>The ID of the special order attached to the letter being viewed, if any.</summary>
	public string specialOrderId;

	/// <summary>The translated name of the recipe learned from this letter, if any.</summary>
	public string learnedRecipe = "";

	public string cookingOrCrafting = "";

	public string mailTitle;

	public List<string> mailMessage = new List<string>();

	public int page;

	public readonly List<ClickableComponent> itemsToGrab = new List<ClickableComponent>();

	public float scale;

	public bool isMail;

	public bool isFromCollection;

	public new bool destroy;

	public Color? customTextColor;

	public bool usingCustomBackground;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public ClickableComponent acceptQuestButton;

	public const float scaleChange = 0.003f;

	/// <summary>Whether the letter has an attached quest or special order which the player can accept.</summary>
	public bool HasQuestOrSpecialOrder
	{
		get
		{
			if (this.questID == null)
			{
				return this.specialOrderId != null;
			}
			return true;
		}
	}

	public LetterViewerMenu(string text)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		Game1.playSound("shwip");
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 32, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		text = this.ApplyCustomFormatting(text);
		this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(text, base.width - 64, base.height - 128);
		this.forwardButton.visible = this.page < this.mailMessage.Count - 1;
		this.backButton.visible = this.page > 0;
		this.OnPageChange();
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public LetterViewerMenu(int secretNoteIndex)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		Game1.playSound("shwip");
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 32, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		string data = DataLoader.SecretNotes(Game1.content)[secretNoteIndex];
		if (data[0] == '!')
		{
			this.secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
			this.secretNoteImage = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(data, 1));
		}
		else
		{
			this.whichBG = ((secretNoteIndex <= 1000) ? 1 : 0);
			string note_text = this.ApplyCustomFormatting(Utility.ParseGiftReveals(data.Replace("@", Game1.player.name.Value)));
			this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(note_text, base.width - 64, base.height - 128);
		}
		this.OnPageChange();
		this.forwardButton.visible = this.page < this.mailMessage.Count - 1;
		this.backButton.visible = this.page > 0;
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public virtual void OnPageChange()
	{
		this.forwardButton.visible = this.page < this.mailMessage.Count - 1;
		this.backButton.visible = this.page > 0;
		foreach (ClickableComponent item in this.itemsToGrab)
		{
			item.visible = this.ShouldShowInteractable();
		}
		if (this.acceptQuestButton != null)
		{
			this.acceptQuestButton.visible = this.ShouldShowInteractable();
		}
		if (Game1.options.SnappyMenus && (base.currentlySnappedComponent == null || !base.currentlySnappedComponent.visible))
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public LetterViewerMenu(string mail, string mailTitle, bool fromCollection = false)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		this.isFromCollection = fromCollection;
		this.mailTitle = mailTitle;
		this.isMail = true;
		Game1.playSound("shwip");
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 32, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		this.acceptQuestButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 128, base.yPositionOnScreen + base.height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
		{
			myID = 103,
			rightNeighborID = 102,
			leftNeighborID = 101
		};
		this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		if (mailTitle.Equals("winter_5_2") || mailTitle.Equals("winter_12_1") || mailTitle.ContainsIgnoreCase("wizard"))
		{
			this.whichBG = 2;
		}
		else if (mailTitle.Equals("Sandy"))
		{
			this.whichBG = 1;
		}
		else if (mailTitle.Contains("Krobus"))
		{
			this.whichBG = 3;
		}
		else if (mailTitle.Contains("passedOut1") || mailTitle.Equals("landslideDone") || mailTitle.Equals("FizzIntro"))
		{
			this.whichBG = 4;
		}
		try
		{
			mail = mail.Split("[#]")[0];
			mail = mail.Replace("@", Game1.player.Name);
			mail = Dialogue.applyGenderSwitch(Game1.player.Gender, mail, altTokenOnly: true);
			mail = this.ApplyCustomFormatting(mail);
			mail = this.HandleActionCommand(mail);
			mail = this.HandleItemCommand(mail);
			bool hideSecretSanta = fromCollection && (Game1.season != Season.Winter || Game1.dayOfMonth < 18 || Game1.dayOfMonth > 25);
			mail = mail.Replace("%secretsanta", hideSecretSanta ? "???" : Utility.GetRandomWinterStarParticipant().displayName);
			if (mailTitle.Equals("winter_18") && !fromCollection)
			{
				Game1.player.mailReceived.Add("sawSecretSanta" + Game1.year);
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error("Letter '" + this.mailTitle + "' couldn't be parsed.", exception);
			mail = "...";
		}
		if (mailTitle == "ccBulletinThankYou" && !Game1.player.hasOrWillReceiveMail("ccBulletinThankYouReceived"))
		{
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (!n.datable.Value)
				{
					Game1.player.changeFriendship(500, n);
				}
				return true;
			});
			Game1.addMailForTomorrow("ccBulletinThankYouReceived", noLetter: true);
		}
		int page_height = base.height - 128;
		if (this.HasInteractable())
		{
			page_height = base.height - 128 - 32;
		}
		this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(mail, base.width - 64, page_height);
		if (this.mailMessage.Count == 0)
		{
			this.mailMessage.Add("[" + mailTitle + "]");
		}
		this.forwardButton.visible = this.page < this.mailMessage.Count - 1;
		this.backButton.visible = this.page > 0;
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
			if (this.mailMessage.Count <= 1)
			{
				this.backButton.myID = -100;
				this.forwardButton.myID = -100;
			}
		}
	}

	/// <summary>Handle the <c>%action</c> command in the mail text, if present. This runs the action(s) and return the mail text with the commands stripped.</summary>
	/// <param name="mail">The mail text to parse.</param>
	public string HandleActionCommand(string mail)
	{
		int searchFromIndex = 0;
		while (true)
		{
			int startItemIndex = mail.IndexOf("%action", searchFromIndex, StringComparison.InvariantCulture);
			if (startItemIndex < 0)
			{
				break;
			}
			int endItemIndex = mail.IndexOf("%%", startItemIndex, StringComparison.InvariantCulture);
			if (endItemIndex < 0)
			{
				break;
			}
			string substring = mail.Substring(startItemIndex, endItemIndex + 2 - startItemIndex);
			mail = mail.Substring(0, startItemIndex) + mail.Substring(startItemIndex + substring.Length);
			string action = substring.Substring("%action".Length, substring.Length - "%action".Length - "%%".Length);
			searchFromIndex = startItemIndex;
			if (!this.isFromCollection && !TriggerActionManager.TryRunAction(action, out var error, out var ex))
			{
				Game1.log.Error($"Letter '{this.mailTitle}' has invalid action command '{action}': {error}", ex);
			}
		}
		return mail;
	}

	/// <summary>Handle the <c>%item</c> command in the mail text, if present. This adds the matching item to the letter and return the mail text with the command stripped.</summary>
	/// <param name="mail">The mail text to parse.</param>
	public string HandleItemCommand(string mail)
	{
		int searchFromIndex = 0;
		while (true)
		{
			int startItemIndex = mail.IndexOf("%item", searchFromIndex, StringComparison.InvariantCulture);
			if (startItemIndex < 0)
			{
				break;
			}
			int endItemIndex = mail.IndexOf("%%", startItemIndex, StringComparison.InvariantCulture);
			if (endItemIndex < 0)
			{
				break;
			}
			string substring = mail.Substring(startItemIndex, endItemIndex + 2 - startItemIndex);
			mail = mail.Substring(0, startItemIndex) + mail.Substring(startItemIndex + substring.Length);
			string[] typeAndArgs = ArgUtility.SplitBySpace(substring.Substring("%item".Length, substring.Length - "%item".Length - "%%".Length), 2);
			string type = typeAndArgs[0];
			string[] args = ((typeAndArgs.Length > 1) ? ArgUtility.SplitBySpace(typeAndArgs[1]) : LegacyShims.EmptyArray<string>());
			searchFromIndex = startItemIndex;
			if (this.isFromCollection)
			{
				continue;
			}
			switch (type.ToLower())
			{
			case "id":
			{
				string id;
				int count;
				if (args.Length == 1)
				{
					id = args[0];
					count = 1;
				}
				else
				{
					int index = Game1.random.Next(args.Length);
					index -= index % 2;
					id = args[index];
					count = int.Parse(args[index + 1]);
				}
				Item item = ItemRegistry.Create(id, count);
				this.itemsToGrab.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height - 32 - 96, 96, 96), item)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				this.backButton.rightNeighborID = 104;
				this.forwardButton.leftNeighborID = 104;
				break;
			}
			case "object":
			{
				int which = Game1.random.Next(args.Length);
				which -= which % 2;
				Item o = ItemRegistry.Create(args[which], int.Parse(args[which + 1]));
				this.itemsToGrab.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height - 32 - 96, 96, 96), o)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				this.backButton.rightNeighborID = 104;
				this.forwardButton.leftNeighborID = 104;
				break;
			}
			case "tools":
			{
				string[] array = args;
				foreach (string arg in array)
				{
					Item tool = null;
					switch (arg)
					{
					case "Axe":
					case "Hoe":
					case "Pickaxe":
						tool = ItemRegistry.Create("(T)" + arg);
						break;
					case "Can":
						tool = ItemRegistry.Create("(T)WateringCan");
						break;
					case "Scythe":
						tool = ItemRegistry.Create("(W)47");
						break;
					}
					if (tool != null)
					{
						this.itemsToGrab.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height - 32 - 96, 96, 96), tool));
					}
				}
				break;
			}
			case "bigobject":
			{
				string id2 = Game1.random.ChooseFrom(args);
				Item o2 = ItemRegistry.Create("(BC)" + id2);
				this.itemsToGrab.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height - 32 - 96, 96, 96), o2)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				this.backButton.rightNeighborID = 104;
				this.forwardButton.leftNeighborID = 104;
				break;
			}
			case "furniture":
			{
				string id3 = Game1.random.ChooseFrom(args);
				Item o3 = ItemRegistry.Create("(F)" + id3);
				this.itemsToGrab.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height - 32 - 96, 96, 96), o3)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				this.backButton.rightNeighborID = 104;
				this.forwardButton.leftNeighborID = 104;
				break;
			}
			case "money":
			{
				int moneyToAdd = ((args.Length > 1) ? Game1.random.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1])) : Convert.ToInt32(args[0]));
				moneyToAdd -= moneyToAdd % 10;
				Game1.player.Money += moneyToAdd;
				this.moneyIncluded = moneyToAdd;
				break;
			}
			case "conversationtopic":
			{
				string topic = args[0];
				int numDays = Convert.ToInt32(args[1]);
				Game1.player.activeDialogueEvents[topic] = numDays;
				if (topic.Equals("ElliottGone3"))
				{
					Utility.getHomeOfFarmer(Game1.player).fridge.Value.addItem(ItemRegistry.Create("(O)732"));
				}
				break;
			}
			case "cookingrecipe":
			{
				Dictionary<string, string> cookingRecipes = CraftingRecipe.cookingRecipes;
				string recipeKey = string.Join(" ", args);
				if (string.IsNullOrWhiteSpace(recipeKey))
				{
					int lowest_required_heart_level = 1000;
					foreach (string s in cookingRecipes.Keys)
					{
						string[] getConditions = ArgUtility.SplitBySpace(ArgUtility.Get(cookingRecipes[s].Split('/'), 3));
						string conditionKey = ArgUtility.Get(getConditions, 0);
						string npcName = ArgUtility.Get(getConditions, 1);
						if (conditionKey == "f" && npcName == this.mailTitle.Replace("Cooking", "") && !Game1.player.cookingRecipes.ContainsKey(s))
						{
							int required_heart_level = Convert.ToInt32(getConditions[2]);
							if (required_heart_level <= lowest_required_heart_level)
							{
								lowest_required_heart_level = required_heart_level;
								recipeKey = s;
							}
						}
					}
				}
				if (!string.IsNullOrWhiteSpace(recipeKey))
				{
					if (cookingRecipes.ContainsKey(recipeKey))
					{
						Game1.player.cookingRecipes.TryAdd(recipeKey, 0);
						this.learnedRecipe = new CraftingRecipe(recipeKey, isCookingRecipe: true).DisplayName;
						this.cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_cooking");
						break;
					}
					Game1.log.Warn($"Letter '{this.mailTitle}' has unknown cooking recipe '{recipeKey}'.");
				}
				break;
			}
			case "craftingrecipe":
			{
				Dictionary<string, string> craftingRecipes = CraftingRecipe.craftingRecipes;
				if (craftingRecipes.ContainsKey(args[0]))
				{
					this.learnedRecipe = args[0];
				}
				else
				{
					string fallbackKey = args[0].Replace('_', ' ');
					if (!craftingRecipes.ContainsKey(fallbackKey))
					{
						Game1.log.Warn($"Letter '{this.mailTitle}' has unknown crafting recipe '{args[0]}'{((args[0] != fallbackKey) ? (" or '" + fallbackKey + "'") : "")}.");
						break;
					}
					this.learnedRecipe = fallbackKey;
				}
				Game1.player.craftingRecipes.TryAdd(this.learnedRecipe, 0);
				this.learnedRecipe = new CraftingRecipe(this.learnedRecipe, isCookingRecipe: false).DisplayName;
				this.cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_crafting");
				break;
			}
			case "itemrecovery":
				if (Game1.player.recoveredItem != null)
				{
					Item item2 = Game1.player.recoveredItem;
					Game1.player.recoveredItem = null;
					this.itemsToGrab.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height - 32 - 96, 96, 96), item2)
					{
						myID = 104,
						leftNeighborID = 101,
						rightNeighborID = 102
					});
					this.backButton.rightNeighborID = 104;
					this.forwardButton.leftNeighborID = 104;
				}
				break;
			case "quest":
				this.questID = args[0];
				if (args.Length > 1)
				{
					if (!Game1.player.mailReceived.Contains("NOQUEST_" + this.questID))
					{
						Game1.player.addQuest(this.questID);
					}
					this.questID = null;
				}
				this.backButton.rightNeighborID = 103;
				this.forwardButton.leftNeighborID = 103;
				break;
			case "specialorder":
			{
				this.specialOrderId = args[0];
				if (ArgUtility.TryGetBool(args, 1, out var addImmediately, out var _, "bool addImmediately") && addImmediately)
				{
					if (!Game1.player.mailReceived.Contains("NOSPECIALORDER_" + this.specialOrderId))
					{
						Game1.player.team.AddSpecialOrder(this.specialOrderId);
					}
					this.specialOrderId = null;
				}
				this.backButton.rightNeighborID = 103;
				this.forwardButton.leftNeighborID = 103;
				break;
			}
			}
		}
		return mail;
	}

	public virtual string ApplyCustomFormatting(string text)
	{
		text = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
		for (int index = text.IndexOf("["); index >= 0; index = text.IndexOf("[", index + 1))
		{
			int end_index = text.IndexOf("]", index);
			if (end_index >= 0)
			{
				bool valid_tag = false;
				try
				{
					string[] split = ArgUtility.SplitBySpace(text.Substring(index + 1, end_index - index - 1));
					string text2 = split[0];
					if (!(text2 == "letterbg"))
					{
						if (text2 == "textcolor")
						{
							string color_string = split[1].ToLower();
							string[] color_lookup = new string[10] { "black", "blue", "red", "purple", "white", "orange", "green", "cyan", "gray", "jojablue" };
							this.customTextColor = null;
							for (int i = 0; i < color_lookup.Length; i++)
							{
								if (color_string == color_lookup[i])
								{
									this.customTextColor = SpriteText.getColorFromIndex(i);
									break;
								}
							}
							valid_tag = true;
						}
					}
					else
					{
						switch (split.Length)
						{
						case 2:
							this.whichBG = int.Parse(split[1]);
							break;
						case 3:
							this.usingCustomBackground = true;
							this.letterTexture = Game1.temporaryContent.Load<Texture2D>(split[1]);
							this.whichBG = int.Parse(split[2]);
							break;
						}
						valid_tag = true;
					}
				}
				catch (Exception)
				{
				}
				if (valid_tag)
				{
					text = text.Remove(index, end_index - index + 1);
					index--;
				}
			}
		}
		return text;
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.HasQuestOrSpecialOrder && this.ShouldShowInteractable())
		{
			base.currentlySnappedComponent = base.getComponentWithID(103);
		}
		else if (this.itemsToGrab.Count > 0 && this.ShouldShowInteractable())
		{
			base.currentlySnappedComponent = base.getComponentWithID(104);
		}
		else if (base.currentlySnappedComponent == null || (base.currentlySnappedComponent != this.backButton && base.currentlySnappedComponent != this.forwardButton))
		{
			base.currentlySnappedComponent = this.forwardButton;
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X;
		base.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y;
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 32, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		this.acceptQuestButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width / 2 - 128, base.yPositionOnScreen + base.height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
		{
			myID = 103,
			rightNeighborID = 102,
			leftNeighborID = 101
		};
		foreach (ClickableComponent item in this.itemsToGrab)
		{
			item.bounds = new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height - 32 - 96, 96, 96);
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (key != Keys.None)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
			{
				base.exitThisMenu(this.ShouldPlayExitSound());
			}
			else
			{
				base.receiveKeyPress(key);
			}
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		switch (button)
		{
		case Buttons.B:
			if (this.isFromCollection)
			{
				base.exitThisMenu(playSound: false);
			}
			break;
		case Buttons.LeftTrigger:
			if (this.page > 0)
			{
				this.page--;
				Game1.playSound("shwip");
				this.OnPageChange();
			}
			break;
		case Buttons.RightTrigger:
			if (this.page < this.mailMessage.Count - 1)
			{
				this.page++;
				Game1.playSound("shwip");
				this.OnPageChange();
			}
			break;
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.scale < 1f)
		{
			return;
		}
		if (base.upperRightCloseButton != null && this.readyToClose() && base.upperRightCloseButton.containsPoint(x, y))
		{
			if (playSound)
			{
				Game1.playSound("bigDeSelect");
			}
			if (!this.isFromCollection)
			{
				base.exitThisMenu(this.ShouldPlayExitSound());
			}
			else
			{
				this.destroy = true;
			}
		}
		if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
		{
			this.unload();
			return;
		}
		if (this.ShouldShowInteractable())
		{
			for (int i = 0; i < this.itemsToGrab.Count; i++)
			{
				ClickableComponent c = this.itemsToGrab[i];
				if (c.containsPoint(x, y) && c.item != null)
				{
					Game1.playSound("coin");
					Game1.player.addItemByMenuIfNecessary(c.item);
					c.item = null;
					if (this.itemsToGrab.Count > 1)
					{
						this.itemsToGrab.RemoveAt(i);
					}
					return;
				}
			}
		}
		if (this.backButton.containsPoint(x, y) && this.page > 0)
		{
			this.page--;
			Game1.playSound("shwip");
			this.OnPageChange();
		}
		else if (this.forwardButton.containsPoint(x, y) && this.page < this.mailMessage.Count - 1)
		{
			this.page++;
			Game1.playSound("shwip");
			this.OnPageChange();
		}
		else if (this.ShouldShowInteractable() && this.acceptQuestButton != null && this.acceptQuestButton.containsPoint(x, y))
		{
			this.AcceptQuest();
		}
		else if (this.isWithinBounds(x, y))
		{
			if (this.page < this.mailMessage.Count - 1)
			{
				this.page++;
				Game1.playSound("shwip");
				this.OnPageChange();
			}
			else if (!this.isMail)
			{
				base.exitThisMenuNoSound();
				Game1.playSound("shwip");
			}
			else if (this.isFromCollection)
			{
				this.destroy = true;
			}
		}
		else if (!this.itemsLeftToGrab())
		{
			if (!this.isFromCollection)
			{
				base.exitThisMenuNoSound();
				Game1.playSound("shwip");
			}
			else
			{
				this.destroy = true;
			}
		}
	}

	public virtual bool ShouldPlayExitSound()
	{
		if (this.HasQuestOrSpecialOrder)
		{
			return false;
		}
		if (this.isFromCollection)
		{
			return false;
		}
		return true;
	}

	public bool itemsLeftToGrab()
	{
		foreach (ClickableComponent item in this.itemsToGrab)
		{
			if (item.item != null)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Add the attached quest or special order to the player.</summary>
	public void AcceptQuest()
	{
		if (this.questID != null)
		{
			Game1.player.addQuest(this.questID);
			if (this.questID == "20")
			{
				MineShaft.CheckForQiChallengeCompletion();
			}
			this.questID = null;
			Game1.playSound("newArtifact");
		}
		else if (this.specialOrderId != null)
		{
			Game1.player.team.AddSpecialOrder(this.specialOrderId);
			this.specialOrderId = null;
			Game1.playSound("newArtifact");
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		if (this.ShouldShowInteractable())
		{
			foreach (ClickableComponent c in this.itemsToGrab)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.03f, 1.1f);
				}
				else
				{
					c.scale = Math.Max(1f, c.scale - 0.03f);
				}
			}
		}
		this.backButton.tryHover(x, y, 0.6f);
		this.forwardButton.tryHover(x, y, 0.6f);
		if (this.ShouldShowInteractable() && this.HasQuestOrSpecialOrder)
		{
			float oldScale = this.acceptQuestButton.scale;
			this.acceptQuestButton.scale = (this.acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
			if (this.acceptQuestButton.scale > oldScale)
			{
				Game1.playSound("Cowboy_gunshot");
			}
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		this.forwardButton.visible = this.page < this.mailMessage.Count - 1;
		this.backButton.visible = this.page > 0;
		if (this.scale < 1f)
		{
			this.scale += (float)time.ElapsedGameTime.Milliseconds * 0.003f;
			if (this.scale >= 1f)
			{
				this.scale = 1f;
			}
		}
		if (this.page < this.mailMessage.Count - 1 && !this.forwardButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
		{
			this.forwardButton.scale = 4f + (float)Math.Sin((double)(float)time.TotalGameTime.Milliseconds / (Math.PI * 64.0)) / 1.5f;
		}
	}

	public virtual Color? getTextColor()
	{
		if (this.customTextColor.HasValue)
		{
			return this.customTextColor.Value;
		}
		if (this.usingCustomBackground)
		{
			return null;
		}
		return this.whichBG switch
		{
			1 => SpriteText.color_Gray, 
			2 => SpriteText.color_Cyan, 
			3 => SpriteText.color_White, 
			4 => SpriteText.color_JojaBlue, 
			_ => null, 
		};
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
		}
		b.Draw(this.letterTexture, new Vector2(base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + base.height / 2), new Rectangle(this.whichBG % 4 * 320, (this.whichBG >= 4) ? (204 + (this.whichBG / 4 - 1) * 180) : 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f * this.scale, SpriteEffects.None, 0.86f);
		if (this.scale == 1f)
		{
			if (this.secretNoteImage != -1)
			{
				b.Draw(this.secretNoteImageTexture, new Vector2(base.xPositionOnScreen + base.width / 2 - 128 - 4, base.yPositionOnScreen + base.height / 2 - 128 + 8), new Rectangle(this.secretNoteImage * 64 % this.secretNoteImageTexture.Width, this.secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64), Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				b.Draw(this.secretNoteImageTexture, new Vector2(base.xPositionOnScreen + base.width / 2 - 128, base.yPositionOnScreen + base.height / 2 - 128), new Rectangle(this.secretNoteImage * 64 % this.secretNoteImageTexture.Width, this.secretNoteImage * 64 / this.secretNoteImageTexture.Width * 64, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				b.Draw(this.secretNoteImageTexture, new Vector2(base.xPositionOnScreen + base.width / 2 - 40, base.yPositionOnScreen + base.height / 2 - 192), new Rectangle(193, 65, 14, 21), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.867f);
			}
			else
			{
				SpriteText.drawString(b, this.mailMessage[this.page], base.xPositionOnScreen + 32, base.yPositionOnScreen + 32, 999999, base.width - 64, 999999, 0.75f, 0.865f, junimoText: false, -1, "", this.getTextColor());
			}
			if (this.ShouldShowInteractable())
			{
				using (List<ClickableComponent>.Enumerator enumerator = this.itemsToGrab.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						ClickableComponent c = enumerator.Current;
						b.Draw(this.letterTexture, c.bounds, new Rectangle(this.whichBG * 24, 180, 24, 24), Color.White);
						c.item?.drawInMenu(b, new Vector2(c.bounds.X + 16, c.bounds.Y + 16), c.scale);
					}
				}
				if (this.moneyIncluded > 0)
				{
					string moneyText = Game1.content.LoadString("Strings\\UI:LetterViewer_MoneyIncluded", this.moneyIncluded);
					SpriteText.drawString(b, moneyText, base.xPositionOnScreen + base.width / 2 - SpriteText.getWidthOfString(moneyText) / 2, base.yPositionOnScreen + base.height - 96, 999999, -1, 9999, 0.75f, 0.865f, junimoText: false, -1, "", this.getTextColor());
				}
				else
				{
					string text = this.learnedRecipe;
					if (text != null && text.Length > 0)
					{
						string recipeText = Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipe", this.cookingOrCrafting);
						SpriteText.drawStringHorizontallyCenteredAt(b, recipeText, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + base.height - 32 - SpriteText.getHeightOfString(recipeText) * 2, 999999, -1, 9999, 0.65f, 0.865f, junimoText: false, this.getTextColor());
						SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipeName", this.learnedRecipe), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + base.height - 32 - SpriteText.getHeightOfString("t"), 999999, -1, 9999, 0.9f, 0.865f, junimoText: false, this.getTextColor());
					}
				}
			}
			base.draw(b);
			this.forwardButton.draw(b);
			this.backButton.draw(b);
			if (this.ShouldShowInteractable() && this.HasQuestOrSpecialOrder)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptQuestButton.bounds.X, this.acceptQuestButton.bounds.Y, this.acceptQuestButton.bounds.Width, this.acceptQuestButton.bounds.Height, (this.acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * this.acceptQuestButton.scale);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(this.acceptQuestButton.bounds.X + 12, this.acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
			}
		}
		if ((!Game1.options.SnappyMenus || !(this.scale < 1f)) && (!Game1.options.SnappyMenus || this.forwardButton.visible || this.backButton.visible || this.HasQuestOrSpecialOrder || this.itemsLeftToGrab()))
		{
			base.drawMouse(b);
		}
	}

	public virtual bool ShouldShowInteractable()
	{
		if (!this.HasInteractable())
		{
			return false;
		}
		return this.page == this.mailMessage.Count - 1;
	}

	public virtual bool HasInteractable()
	{
		if (this.isFromCollection)
		{
			return false;
		}
		if (this.HasQuestOrSpecialOrder)
		{
			return true;
		}
		if (this.moneyIncluded > 0)
		{
			return true;
		}
		if (this.itemsToGrab.Count > 0)
		{
			return true;
		}
		string text = this.learnedRecipe;
		if (text != null && text.Length > 0)
		{
			return true;
		}
		return false;
	}

	public void unload()
	{
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		if (this.HasQuestOrSpecialOrder)
		{
			this.AcceptQuest();
		}
		if (this.itemsLeftToGrab())
		{
			List<Item> items = new List<Item>();
			foreach (ClickableComponent c in this.itemsToGrab)
			{
				if (c.item != null)
				{
					items.Add(c.item);
				}
			}
			this.itemsToGrab.Clear();
			if (items.Count > 0)
			{
				Game1.playSound("coin");
				Game1.player.addItemsByMenuIfNecessary(items);
			}
		}
		if (this.isFromCollection)
		{
			this.destroy = true;
			Game1.oldKBState = Game1.GetKeyboardState();
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldPadState = Game1.input.GetGamePadState();
		}
		base.cleanupBeforeExit();
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (this.isFromCollection)
		{
			this.destroy = true;
		}
		else
		{
			this.receiveLeftClick(x, y, playSound);
		}
	}
}
