using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.Logging;

namespace StardewValley.Menus;

public class ChatBox : IClickableMenu
{
	public const int chatMessage = 0;

	public const int errorMessage = 1;

	public const int userNotificationMessage = 2;

	public const int privateMessage = 3;

	public const int defaultMaxMessages = 10;

	public const int timeToDisplayMessages = 600;

	public const int chatboxWidth = 896;

	public const int chatboxHeight = 56;

	public const int region_chatBox = 101;

	public const int region_emojiButton = 102;

	public ChatTextBox chatBox;

	public ClickableComponent chatBoxCC;

	/// <summary>A logger which copies messages to the chat box, used when entering commands through the chat.</summary>
	private readonly IGameLogger CheatCommandChatLogger;

	public List<ChatMessage> messages = new List<ChatMessage>();

	private KeyboardState oldKBState;

	private List<string> cheatHistory = new List<string>();

	private int cheatHistoryPosition = -1;

	public int maxMessages = 10;

	public static Texture2D emojiTexture;

	public ClickableTextureComponent emojiMenuIcon;

	public EmojiMenu emojiMenu;

	public bool choosingEmoji;

	private long lastReceivedPrivateMessagePlayerId;

	public ChatBox()
	{
		this.CheatCommandChatLogger = new CheatCommandChatLogger(this);
		Texture2D chatboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\chatBox");
		this.chatBox = new ChatTextBox(chatboxTexture, null, Game1.smallFont, Color.White);
		this.chatBox.OnEnterPressed += textBoxEnter;
		this.chatBox.TitleText = "Chat";
		this.chatBoxCC = new ClickableComponent(new Rectangle(this.chatBox.X, this.chatBox.Y, this.chatBox.Width, this.chatBox.Height), "")
		{
			myID = 101
		};
		Game1.keyboardDispatcher.Subscriber = this.chatBox;
		ChatBox.emojiTexture = Game1.content.Load<Texture2D>("LooseSprites\\emojis");
		this.emojiMenuIcon = new ClickableTextureComponent(new Rectangle(0, 0, 40, 36), ChatBox.emojiTexture, new Rectangle(0, 0, 9, 9), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		this.emojiMenu = new EmojiMenu(this, ChatBox.emojiTexture, chatboxTexture);
		this.chatBoxCC.rightNeighborID = 102;
		this.updatePosition();
		this.chatBox.Selected = false;
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(101);
		this.snapCursorToCurrentSnappedComponent();
	}

	private void updatePosition()
	{
		this.chatBox.Width = 896;
		this.chatBox.Height = 56;
		base.width = this.chatBox.Width;
		base.height = this.chatBox.Height;
		base.xPositionOnScreen = 0;
		base.yPositionOnScreen = Game1.uiViewport.Height - this.chatBox.Height;
		Utility.makeSafe(ref base.xPositionOnScreen, ref base.yPositionOnScreen, this.chatBox.Width, this.chatBox.Height);
		this.chatBox.X = base.xPositionOnScreen;
		this.chatBox.Y = base.yPositionOnScreen;
		this.chatBoxCC.bounds = new Rectangle(this.chatBox.X, this.chatBox.Y, this.chatBox.Width, this.chatBox.Height);
		this.emojiMenuIcon.bounds.Y = this.chatBox.Y + 8;
		this.emojiMenuIcon.bounds.X = this.chatBox.Width - this.emojiMenuIcon.bounds.Width - 8;
		if (this.emojiMenu != null)
		{
			this.emojiMenu.xPositionOnScreen = this.emojiMenuIcon.bounds.Center.X - 146;
			this.emojiMenu.yPositionOnScreen = this.emojiMenuIcon.bounds.Y - 248;
		}
	}

	public virtual void textBoxEnter(string text_to_send)
	{
		if (text_to_send.Length < 1)
		{
			return;
		}
		if (text_to_send[0] == '/')
		{
			string text = ArgUtility.SplitBySpaceAndGet(text_to_send, 0);
			if (text != null && text.Length > 1)
			{
				this.runCommand(text_to_send.Substring(1));
				return;
			}
		}
		text_to_send = Program.sdk.FilterDirtyWords(text_to_send);
		Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, text_to_send, Multiplayer.AllPlayers);
		this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, text_to_send);
	}

	public virtual void textBoxEnter(TextBox sender)
	{
		bool include_color_information;
		if (sender is ChatTextBox box)
		{
			if (box.finalText.Count > 0)
			{
				include_color_information = true;
				string message = box.finalText[0].message;
				if (message != null && message.StartsWith('/'))
				{
					string text = ArgUtility.SplitBySpaceAndGet(box.finalText[0].message, 0);
					if (text != null && text.Length > 1)
					{
						include_color_information = false;
					}
				}
				if (box.finalText.Count != 1)
				{
					goto IL_00c8;
				}
				if (box.finalText[0].message != null || box.finalText[0].emojiIndex != -1)
				{
					string message2 = box.finalText[0].message;
					if (message2 == null || message2.Trim().Length != 0)
					{
						goto IL_00c8;
					}
				}
			}
			goto IL_00dc;
		}
		goto IL_00e9;
		IL_00e9:
		sender.Text = "";
		this.clickAway();
		return;
		IL_00dc:
		box.reset();
		this.cheatHistoryPosition = -1;
		goto IL_00e9;
		IL_00c8:
		string textToSend = ChatMessage.makeMessagePlaintext(box.finalText, include_color_information);
		this.textBoxEnter(textToSend);
		goto IL_00dc;
	}

	public virtual void addInfoMessage(string message)
	{
		this.receiveChatMessage(0L, 2, LocalizedContentManager.CurrentLanguageCode, message);
	}

	public virtual void globalInfoMessage(string messageKey, params string[] args)
	{
		if (Game1.IsMultiplayer)
		{
			Game1.multiplayer.globalChatInfoMessage(messageKey, args);
		}
		else
		{
			this.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_" + messageKey, args));
		}
	}

	public virtual void addErrorMessage(string message)
	{
		this.receiveChatMessage(0L, 1, LocalizedContentManager.CurrentLanguageCode, message);
	}

	public virtual void listPlayers(bool otherPlayersOnly = false, bool onlineOnly = true)
	{
		this.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ListOnlinePlayers"));
		IEnumerable<Farmer> enumerable;
		if (!onlineOnly)
		{
			enumerable = Game1.getAllFarmers();
		}
		else
		{
			IEnumerable<Farmer> onlineFarmers = Game1.getOnlineFarmers();
			enumerable = onlineFarmers;
		}
		foreach (Farmer f in enumerable)
		{
			if (!otherPlayersOnly || f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
			{
				this.addInfoMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_ListOnlinePlayersEntry", ChatBox.formattedUserNameLong(f)));
			}
		}
	}

	protected virtual void runCommand(string commandText)
	{
		if (!ChatCommands.TryHandle(ArgUtility.SplitBySpace(commandText), this) && (ChatCommands.AllowCheats || Game1.isRunningMacro))
		{
			this.cheat(commandText);
		}
	}

	public virtual void cheat(string command, bool isDebug = false)
	{
		string fullCommand = (isDebug ? "debug " : "") + command;
		Game1.debugOutput = null;
		this.addInfoMessage("/" + fullCommand);
		if (!Game1.isRunningMacro)
		{
			this.cheatHistory.Insert(0, "/" + fullCommand);
		}
		if (Game1.game1.parseDebugInput(command, this.CheatCommandChatLogger))
		{
			if (!string.IsNullOrEmpty(Game1.debugOutput))
			{
				this.addInfoMessage(Game1.debugOutput);
			}
		}
		else if (!string.IsNullOrEmpty(Game1.debugOutput))
		{
			this.addErrorMessage(Game1.debugOutput);
		}
		else
		{
			this.addErrorMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ChatBox.cs.10261") + " " + ArgUtility.SplitBySpaceAndGet(command, 0));
		}
	}

	public void replyPrivateMessage(string[] command)
	{
		if (!Game1.IsMultiplayer)
		{
			return;
		}
		Farmer lastPlayer;
		if (this.lastReceivedPrivateMessagePlayerId == 0L)
		{
			this.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Reply_NoMessageFound"));
		}
		else if (!Game1.otherFarmers.TryGetValue(this.lastReceivedPrivateMessagePlayerId, out lastPlayer) || !lastPlayer.isActive())
		{
			this.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Reply_Failed"));
		}
		else
		{
			if (command.Length <= 1)
			{
				return;
			}
			string message = "";
			for (int i = 1; i < command.Length; i++)
			{
				message += command[i];
				if (i < command.Length - 1)
				{
					message += " ";
				}
			}
			message = Program.sdk.FilterDirtyWords(message);
			Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message, this.lastReceivedPrivateMessagePlayerId);
			this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message);
		}
	}

	public Farmer findMatchingFarmer(string[] command, ref int matchingIndex, bool allowMatchingByUserName = false, bool onlineOnly = true)
	{
		Farmer matchingFarmer = null;
		IEnumerable<Farmer> enumerable;
		if (!onlineOnly)
		{
			enumerable = Game1.getAllFarmers();
		}
		else
		{
			IEnumerable<Farmer> values = Game1.otherFarmers.Values;
			enumerable = values;
		}
		foreach (Farmer farmer in enumerable)
		{
			string[] farmerNameSplit = ArgUtility.SplitBySpace(farmer.displayName);
			bool isMatch = true;
			int i;
			for (i = 0; i < farmerNameSplit.Length; i++)
			{
				if (command.Length > i + 1)
				{
					if (!command[i + 1].EqualsIgnoreCase(farmerNameSplit[i]))
					{
						isMatch = false;
						break;
					}
					continue;
				}
				isMatch = false;
				break;
			}
			if (isMatch)
			{
				matchingFarmer = farmer;
				matchingIndex = i;
				break;
			}
			if (!allowMatchingByUserName)
			{
				continue;
			}
			isMatch = true;
			string[] userNameSplit = ArgUtility.SplitBySpace(Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID));
			if (userNameSplit.Length == 0)
			{
				continue;
			}
			for (i = 0; i < userNameSplit.Length; i++)
			{
				if (command.Length > i + 1)
				{
					if (!command[i + 1].EqualsIgnoreCase(userNameSplit[i]))
					{
						isMatch = false;
						break;
					}
					continue;
				}
				isMatch = false;
				break;
			}
			if (isMatch)
			{
				matchingFarmer = farmer;
				matchingIndex = i;
				break;
			}
		}
		return matchingFarmer;
	}

	public void sendPrivateMessage(string[] command)
	{
		if (!Game1.IsMultiplayer)
		{
			return;
		}
		int matchingIndex = 0;
		Farmer matchingFarmer = this.findMatchingFarmer(command, ref matchingIndex);
		if (matchingFarmer == null)
		{
			this.addErrorMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NoSuchOnlinePlayer"));
			return;
		}
		string message = "";
		for (int i = matchingIndex + 1; i < command.Length; i++)
		{
			message += command[i];
			if (i < command.Length - 1)
			{
				message += " ";
			}
		}
		message = Program.sdk.FilterDirtyWords(message);
		Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message, matchingFarmer.UniqueMultiplayerID);
		this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message);
	}

	public bool isActive()
	{
		return this.chatBox.Selected;
	}

	public void activate()
	{
		this.chatBox.Selected = true;
		this.setText("");
	}

	public override void clickAway()
	{
		base.clickAway();
		if (!this.choosingEmoji || !this.emojiMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Escape))
		{
			bool selected = this.chatBox.Selected;
			this.chatBox.Selected = false;
			this.choosingEmoji = false;
			this.setText("");
			this.cheatHistoryPosition = -1;
			if (selected)
			{
				Game1.oldKBState = Game1.GetKeyboardState();
			}
		}
	}

	public override bool isWithinBounds(int x, int y)
	{
		if (x - base.xPositionOnScreen >= base.width || x - base.xPositionOnScreen < 0 || y - base.yPositionOnScreen >= base.height || y - base.yPositionOnScreen < -this.getOldMessagesBoxHeight())
		{
			if (this.choosingEmoji)
			{
				return this.emojiMenu.isWithinBounds(x, y);
			}
			return false;
		}
		return true;
	}

	public virtual void setText(string text)
	{
		this.chatBox.setText(text);
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		switch (key)
		{
		case Keys.Up:
			if (this.cheatHistoryPosition < this.cheatHistory.Count - 1)
			{
				this.cheatHistoryPosition++;
				string cheat2 = this.cheatHistory[this.cheatHistoryPosition];
				this.chatBox.setText(cheat2);
			}
			break;
		case Keys.Down:
			if (this.cheatHistoryPosition > 0)
			{
				this.cheatHistoryPosition--;
				string cheat = this.cheatHistory[this.cheatHistoryPosition];
				this.chatBox.setText(cheat);
			}
			break;
		}
		if (!Game1.options.doesInputListContain(Game1.options.moveUpButton, key) && !Game1.options.doesInputListContain(Game1.options.moveRightButton, key) && !Game1.options.doesInputListContain(Game1.options.moveDownButton, key) && !Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
		{
			base.receiveKeyPress(key);
		}
	}

	public override bool readyToClose()
	{
		return false;
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
	}

	public bool isHoveringOverClickable(int x, int y)
	{
		if (this.emojiMenuIcon.containsPoint(x, y) || (this.choosingEmoji && this.emojiMenu.isWithinBounds(x, y)))
		{
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!this.chatBox.Selected)
		{
			return;
		}
		if (this.emojiMenuIcon.containsPoint(x, y))
		{
			this.choosingEmoji = !this.choosingEmoji;
			Game1.playSound("shwip");
			this.emojiMenuIcon.scale = 4f;
			return;
		}
		if (this.choosingEmoji && this.emojiMenu.isWithinBounds(x, y))
		{
			this.emojiMenu.leftClick(x, y, this);
			return;
		}
		this.chatBox.Update();
		if (this.choosingEmoji)
		{
			this.choosingEmoji = false;
			this.emojiMenuIcon.scale = 4f;
		}
		if (this.isWithinBounds(x, y))
		{
			this.chatBox.Selected = true;
		}
	}

	public static string formattedUserName(Farmer farmer)
	{
		string name = farmer.Name;
		if (string.IsNullOrWhiteSpace(name))
		{
			name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
		}
		return Program.sdk.FilterDirtyWords(name);
	}

	public static string formattedUserNameLong(Farmer farmer)
	{
		string name = ChatBox.formattedUserName(farmer);
		string userName = Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID);
		if (string.IsNullOrWhiteSpace(userName))
		{
			return name;
		}
		return Game1.content.LoadString("Strings\\UI:Chat_PlayerName", name, userName);
	}

	public string formatMessage(long sourceFarmer, int chatKind, string message)
	{
		string userName = Game1.content.LoadString("Strings\\UI:Chat_UnknownUserName");
		Farmer farmer;
		if (sourceFarmer == Game1.player.UniqueMultiplayerID)
		{
			farmer = Game1.player;
		}
		else if (!Game1.otherFarmers.TryGetValue(sourceFarmer, out farmer))
		{
			farmer = null;
		}
		if (farmer != null)
		{
			userName = ChatBox.formattedUserName(farmer);
		}
		return chatKind switch
		{
			0 => Game1.content.LoadString("Strings\\UI:Chat_ChatMessageFormat", userName, message), 
			2 => Game1.content.LoadString("Strings\\UI:Chat_UserNotificationMessageFormat", message), 
			3 => Game1.content.LoadString("Strings\\UI:Chat_PrivateMessageFormat", userName, message), 
			_ => Game1.content.LoadString("Strings\\UI:Chat_ErrorMessageFormat", message), 
		};
	}

	public virtual Color messageColor(int chatKind)
	{
		return chatKind switch
		{
			0 => this.chatBox.TextColor, 
			3 => Color.DarkCyan, 
			2 => Color.Yellow, 
			_ => Color.Red, 
		};
	}

	public virtual void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
	{
		string text = this.formatMessage(sourceFarmer, chatKind, message);
		ChatMessage c = new ChatMessage();
		string s = Game1.parseText(text, this.chatBox.Font, this.chatBox.Width - 16);
		c.timeLeftToDisplay = 600;
		c.verticalSize = (int)this.chatBox.Font.MeasureString(s).Y + 4;
		c.color = this.messageColor(chatKind);
		c.language = language;
		c.parseMessageForEmoji(s);
		this.messages.Add(c);
		if (this.messages.Count > this.maxMessages)
		{
			this.messages.RemoveAt(0);
		}
		if (chatKind == 3 && sourceFarmer != Game1.player.UniqueMultiplayerID)
		{
			this.lastReceivedPrivateMessagePlayerId = sourceFarmer;
		}
	}

	public virtual void addMessage(string message, Color color)
	{
		ChatMessage c = new ChatMessage();
		string s = Game1.parseText(message, this.chatBox.Font, this.chatBox.Width - 8);
		c.timeLeftToDisplay = 600;
		c.verticalSize = (int)this.chatBox.Font.MeasureString(s).Y + 4;
		c.color = color;
		c.language = LocalizedContentManager.CurrentLanguageCode;
		c.parseMessageForEmoji(s);
		this.messages.Add(c);
		if (this.messages.Count > this.maxMessages)
		{
			this.messages.RemoveAt(0);
		}
	}

	/// <summary>Add a "ConcernedApe: Nice try..." Easter egg message to the chat box.</summary>
	public void addNiceTryEasterEggMessage()
	{
		this.addMessage(Game1.content.LoadString("Strings\\UI:ChatCommands_Error_NiceTry"), new Color(104, 214, 255));
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.emojiMenuIcon.tryHover(x, y, 1f);
		this.emojiMenuIcon.tryHover(x, y, 1f);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		KeyboardState keyState = Game1.input.GetKeyboardState();
		Keys[] pressedKeys = keyState.GetPressedKeys();
		foreach (Keys key in pressedKeys)
		{
			if (!this.oldKBState.IsKeyDown(key))
			{
				this.receiveKeyPress(key);
			}
		}
		this.oldKBState = keyState;
		for (int j = 0; j < this.messages.Count; j++)
		{
			if (this.messages[j].timeLeftToDisplay > 0)
			{
				this.messages[j].timeLeftToDisplay--;
			}
			if (this.messages[j].timeLeftToDisplay < 75)
			{
				this.messages[j].alpha = (float)this.messages[j].timeLeftToDisplay / 75f;
			}
		}
		if (this.chatBox.Selected)
		{
			foreach (ChatMessage message in this.messages)
			{
				message.alpha = 1f;
			}
		}
		this.emojiMenuIcon.tryHover(0, 0, 1f);
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		if (this.choosingEmoji)
		{
			this.emojiMenu.receiveScrollWheelAction(direction);
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		this.updatePosition();
	}

	public static SpriteFont messageFont(LocalizedContentManager.LanguageCode language)
	{
		return Game1.content.Load<SpriteFont>("Fonts\\SmallFont", language);
	}

	public int getOldMessagesBoxHeight()
	{
		int heightSoFar = 20;
		for (int i = this.messages.Count - 1; i >= 0; i--)
		{
			ChatMessage message = this.messages[i];
			if (this.chatBox.Selected || message.alpha > 0.01f)
			{
				heightSoFar += message.verticalSize;
			}
		}
		return heightSoFar;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		int heightSoFar = 0;
		bool drawBG = false;
		for (int i = this.messages.Count - 1; i >= 0; i--)
		{
			ChatMessage message = this.messages[i];
			if (this.chatBox.Selected || message.alpha > 0.01f)
			{
				heightSoFar += message.verticalSize;
				drawBG = true;
			}
		}
		if (drawBG)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), base.xPositionOnScreen, base.yPositionOnScreen - heightSoFar - 20 + ((!this.chatBox.Selected) ? this.chatBox.Height : 0), this.chatBox.Width, heightSoFar + 20, Color.White, 4f, drawShadow: false);
		}
		heightSoFar = 0;
		for (int i2 = this.messages.Count - 1; i2 >= 0; i2--)
		{
			ChatMessage message2 = this.messages[i2];
			heightSoFar += message2.verticalSize;
			message2.draw(b, base.xPositionOnScreen + 12, base.yPositionOnScreen - heightSoFar - 8 + ((!this.chatBox.Selected) ? this.chatBox.Height : 0));
		}
		if (this.chatBox.Selected)
		{
			this.chatBox.Draw(b, drawShadow: false);
			this.emojiMenuIcon.draw(b, Color.White, 0.99f);
			if (this.choosingEmoji)
			{
				this.emojiMenu.draw(b);
			}
			if (this.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) && !Game1.options.hardwareCursor)
			{
				Game1.mouseCursor = (Game1.options.gamepadControls ? Game1.cursor_gamepad_pointer : Game1.cursor_default);
			}
		}
	}
}
