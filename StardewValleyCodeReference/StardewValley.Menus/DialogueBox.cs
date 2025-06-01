using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class DialogueBox : IClickableMenu
{
	public List<string> dialogues = new List<string>();

	public Dialogue characterDialogue;

	public Stack<string> characterDialoguesBrokenUp = new Stack<string>();

	public Response[] responses = LegacyShims.EmptyArray<Response>();

	public const int portraitBoxSize = 74;

	public const int nameTagWidth = 102;

	public const int nameTagHeight = 18;

	public const int portraitPlateWidth = 115;

	public const int nameTagSideMargin = 5;

	public const float transitionRate = 3f;

	public const int characterAdvanceDelay = 30;

	public const int safetyDelay = 750;

	public int questionFinishPauseTimer;

	protected bool _showedOptions;

	public Rectangle friendshipJewel = Rectangle.Empty;

	public List<ClickableComponent> responseCC;

	public int x;

	public int y;

	public int transitionX = -1;

	public int transitionY;

	public int transitionWidth;

	public int transitionHeight;

	public int characterAdvanceTimer;

	public int characterIndexInDialogue;

	public int safetyTimer = 750;

	public int heightForQuestions;

	public int selectedResponse = -1;

	public int newPortaitShakeTimer;

	public bool transitionInitialized;

	/// <summary>Whether to progressively type the dialogue text into the box. If false, the dialogue appears instantly instead.</summary>
	public bool showTyping = true;

	public bool transitioning = true;

	public bool transitioningBigger = true;

	public bool dialogueContinuedOnNextPage;

	public bool dialogueFinished;

	public bool isQuestion;

	public TemporaryAnimatedSprite dialogueIcon;

	public TemporaryAnimatedSprite aboveDialogueImage;

	private string hoverText = "";

	public DialogueBox(int x, int y, int width, int height)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		this.x = x;
		this.y = y;
		base.width = width;
		base.height = height;
	}

	public DialogueBox(string dialogue)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		this.dialogues.AddRange(dialogue.Split('#'));
		base.width = Math.Min(1240, SpriteText.getWidthOfString(this.dialogues[0]) + 64);
		base.height = SpriteText.getHeightOfString(this.dialogues[0], base.width - 20) + 4;
		this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height).X;
		this.y = Game1.uiViewport.Height - base.height - 64;
		this.setUpIcons();
	}

	public DialogueBox(string dialogue, Response[] responses, int width = 1200)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		this.dialogues.Add(dialogue);
		this.responses = responses;
		this.isQuestion = true;
		base.width = width;
		this.setUpQuestions();
		base.height = this.heightForQuestions;
		this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, base.height).X;
		this.y = Game1.uiViewport.Height - base.height - 64;
		this.setUpIcons();
		this.characterIndexInDialogue = dialogue.Length;
		if (responses != null)
		{
			foreach (Response response in responses)
			{
				response.responseText = Dialogue.applyGenderSwitch(Game1.player.Gender, response.responseText, altTokenOnly: true);
			}
		}
	}

	public DialogueBox(Dialogue dialogue)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		this.characterDialogue = dialogue;
		base.width = 1200;
		base.height = 384;
		this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height).X;
		this.y = Game1.uiViewport.Height - base.height - 64;
		this.friendshipJewel = new Rectangle(this.x + base.width - 64, this.y + 256, 44, 44);
		dialogue.prepareDialogueForDisplay();
		this.characterDialogue.prepareCurrentDialogueForDisplay();
		if (!this.characterDialogue.isDialogueFinished())
		{
			this.characterDialoguesBrokenUp.Push(dialogue.getCurrentDialogue());
			this.checkDialogue(dialogue);
		}
		else
		{
			this.dialogueFinished = true;
		}
		this.newPortaitShakeTimer = ((this.characterDialogue.getPortraitIndex() == 1) ? 250 : 0);
		this.setUpForGamePadMode();
	}

	public DialogueBox(List<string> dialogues)
	{
		if (Game1.options.SnappyMenus)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		this.dialogues = dialogues;
		base.width = Math.Min(1200, SpriteText.getWidthOfString(dialogues[0]) + 64);
		base.height = SpriteText.getHeightOfString(dialogues[0], base.width - 16);
		this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height).X;
		this.y = Game1.uiViewport.Height - base.height - 64;
		this.setUpIcons();
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	private void playOpeningSound()
	{
		Game1.playSound("breathin");
	}

	public void closeDialogue()
	{
		if (Game1.activeClickableMenu.Equals(this))
		{
			Game1.exitActiveMenu();
			Game1.dialogueUp = false;
			if (this.characterDialogue?.speaker != null && this.characterDialogue.speaker.CurrentDialogue.Count > 0 && this.dialogueFinished && this.characterDialogue.speaker.CurrentDialogue.Count > 0)
			{
				this.characterDialogue.speaker.CurrentDialogue.Pop();
			}
			if (Game1.messagePause)
			{
				Game1.pauseTime = 500f;
			}
			if (Game1.currentObjectDialogue.Count > 0)
			{
				Game1.currentObjectDialogue.Dequeue();
			}
			Game1.currentDialogueCharacterIndex = 0;
			if (Game1.currentObjectDialogue.Count > 0)
			{
				Game1.dialogueUp = true;
				Game1.questionChoices.Clear();
				Game1.dialogueTyping = true;
			}
			if (this.characterDialogue?.speaker != null && !this.characterDialogue.speaker.Name.Equals("Gunther") && !Game1.eventUp && !this.characterDialogue.speaker.doingEndOfRouteAnimation.Value)
			{
				this.characterDialogue.speaker.doneFacingPlayer(Game1.player);
			}
			Game1.currentSpeaker = null;
			if (!Game1.eventUp)
			{
				if (!Game1.isWarping)
				{
					Game1.player.CanMove = true;
				}
				Game1.player.movementDirections.Clear();
			}
			else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
			{
				if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
				else
				{
					Game1.player.CanMove = true;
				}
			}
			Game1.questionChoices.Clear();
		}
		if (Game1.afterDialogues != null)
		{
			Game1.afterFadeFunction afterDialogues = Game1.afterDialogues;
			Game1.afterDialogues = null;
			afterDialogues();
		}
	}

	public void finishTyping()
	{
		this.characterIndexInDialogue = this.getCurrentString().Length;
	}

	public void beginOutro()
	{
		this.transitioning = true;
		this.transitioningBigger = false;
		Game1.playSound("breathout");
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.receiveLeftClick(x, y, playSound);
	}

	private void tryOutro()
	{
		if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.Equals(this))
		{
			this.beginOutro();
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (this.transitioning)
		{
			return;
		}
		if (Game1.options.SnappyMenus && !this.isQuestion && Game1.options.doesInputListContain(Game1.options.menuButton, key))
		{
			this.receiveLeftClick(0, 0);
		}
		else if (!Game1.options.gamepadControls && Game1.options.doesInputListContain(Game1.options.actionButton, key))
		{
			this.receiveLeftClick(0, 0);
		}
		else if (this.isQuestion && !Game1.eventUp && this.characterDialogue == null)
		{
			if (this.responses != null)
			{
				Response[] array = this.responses;
				foreach (Response response in array)
				{
					if (response.hotkey == key && Game1.currentLocation.answerDialogue(response))
					{
						Game1.playSound("smallSelect");
						this.selectedResponse = -1;
						this.tryOutro();
						return;
					}
				}
				if (key == Keys.N)
				{
					array = this.responses;
					foreach (Response response2 in array)
					{
						if (response2.hotkey == Keys.Escape && Game1.currentLocation.answerDialogue(response2))
						{
							Game1.playSound("smallSelect");
							this.selectedResponse = -1;
							this.tryOutro();
							return;
						}
					}
				}
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) || key == Keys.N)
			{
				Response[] array2 = this.responses;
				if (array2 != null && array2.Length != 0 && Game1.currentLocation.answerDialogue(this.responses[this.responses.Length - 1]))
				{
					Game1.playSound("smallSelect");
				}
				this.selectedResponse = -1;
				this.tryOutro();
			}
			else if (Game1.options.SnappyMenus)
			{
				this.safetyTimer = 0;
				base.receiveKeyPress(key);
			}
			else if (key == Keys.Y)
			{
				Response[] array3 = this.responses;
				if (array3 != null && array3.Length != 0 && this.responses[0].responseKey.Equals("Yes") && Game1.currentLocation.answerDialogue(this.responses[0]))
				{
					Game1.playSound("smallSelect");
					this.selectedResponse = -1;
					this.tryOutro();
				}
			}
		}
		else if (Game1.options.SnappyMenus && this.isQuestion && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.transitioning)
		{
			return;
		}
		if (this.characterIndexInDialogue < this.getCurrentString().Length - 1)
		{
			this.characterIndexInDialogue = this.getCurrentString().Length - 1;
		}
		else
		{
			if (this.safetyTimer > 0)
			{
				return;
			}
			if (this.isQuestion)
			{
				if (this.selectedResponse == -1)
				{
					return;
				}
				this.questionFinishPauseTimer = (Game1.eventUp ? 600 : 200);
				this.transitioning = true;
				this.transitionInitialized = false;
				this.transitioningBigger = true;
				if (this.characterDialogue == null)
				{
					Game1.dialogueUp = false;
					if (Game1.eventUp && Game1.currentLocation.afterQuestion == null)
					{
						Game1.playSound("smallSelect");
						Game1.currentLocation.currentEvent.answerDialogue(Game1.currentLocation.lastQuestionKey, this.selectedResponse);
						this.selectedResponse = -1;
						this.tryOutro();
						return;
					}
					if (Game1.currentLocation.answerDialogue(this.responses[this.selectedResponse]))
					{
						Game1.playSound("smallSelect");
					}
					this.selectedResponse = -1;
					this.tryOutro();
					return;
				}
				this.characterDialoguesBrokenUp.Pop();
				this.characterDialogue.chooseResponse(this.responses[this.selectedResponse]);
				this.characterDialoguesBrokenUp.Push("");
				Game1.playSound("smallSelect");
			}
			else if (this.characterDialogue == null)
			{
				this.dialogues.RemoveAt(0);
				if (this.dialogues.Count == 0)
				{
					this.closeDialogue();
				}
				else
				{
					base.width = Math.Min(1200, SpriteText.getWidthOfString(this.dialogues[0]) + 64);
					base.height = SpriteText.getHeightOfString(this.dialogues[0], base.width - 16);
					this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height).X;
					this.y = Game1.uiViewport.Height - base.height - 64;
					base.xPositionOnScreen = x;
					base.yPositionOnScreen = y;
					this.setUpIcons();
				}
			}
			this.characterIndexInDialogue = 0;
			if (this.characterDialogue != null)
			{
				int oldPortrait = this.characterDialogue.getPortraitIndex();
				if (this.characterDialoguesBrokenUp.Count == 0)
				{
					this.beginOutro();
					return;
				}
				this.characterDialoguesBrokenUp.Pop();
				if (this.characterDialoguesBrokenUp.Count == 0)
				{
					if (!this.characterDialogue.isCurrentStringContinuedOnNextScreen)
					{
						this.beginOutro();
					}
					this.characterDialogue.exitCurrentDialogue();
				}
				if (!this.characterDialogue.isDialogueFinished() && this.characterDialogue.getCurrentDialogue().Length > 0 && this.characterDialoguesBrokenUp.Count == 0)
				{
					this.characterDialogue.prepareCurrentDialogueForDisplay();
					if (this.characterDialogue.isDialogueFinished())
					{
						this.beginOutro();
						return;
					}
					this.characterDialoguesBrokenUp.Push(this.characterDialogue.getCurrentDialogue());
				}
				this.checkDialogue(this.characterDialogue);
				if (this.characterDialogue.getPortraitIndex() != oldPortrait)
				{
					this.newPortaitShakeTimer = ((this.characterDialogue.getPortraitIndex() == 1) ? 250 : 50);
				}
			}
			if (!this.transitioning)
			{
				Game1.playSound("smallSelect");
			}
			this.setUpIcons();
			this.safetyTimer = ((!Game1.IsDedicatedHost) ? 750 : 0);
			if (this.getCurrentString() != null && this.getCurrentString().Length <= 20)
			{
				this.safetyTimer -= 200;
			}
		}
	}

	private void setUpIcons()
	{
		this.dialogueIcon = null;
		if (this.isQuestion)
		{
			this.setUpQuestionIcon();
		}
		else if (this.characterDialogue != null && (this.characterDialogue.isCurrentStringContinuedOnNextScreen || this.characterDialoguesBrokenUp.Count > 1))
		{
			this.setUpNextPageIcon();
		}
		else
		{
			List<string> list = this.dialogues;
			if (list != null && list.Count > 1)
			{
				this.setUpNextPageIcon();
			}
			else
			{
				this.setUpCloseDialogueIcon();
			}
		}
		this.setUpForGamePadMode();
		if (this.getCurrentString() != null && this.getCurrentString().Length <= 20)
		{
			this.safetyTimer -= 200;
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int mouseX, int mouseY)
	{
		this.hoverText = "";
		if (!this.transitioning && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
		{
			base.performHoverAction(mouseX, mouseY);
			if (this.isQuestion)
			{
				int oldResponse = this.selectedResponse;
				this.selectedResponse = -1;
				if (Game1.options.gamepadControls && base.currentlySnappedComponent != null)
				{
					this.selectedResponse = base.currentlySnappedComponent.myID;
				}
				int responseY = this.y - (this.heightForQuestions - base.height) + SpriteText.getHeightOfString(this.getCurrentString(), base.width - 16) + 48;
				int margin = 8;
				for (int i = 0; i < this.responses.Length; i++)
				{
					if (mouseY >= responseY - margin && mouseY < responseY + SpriteText.getHeightOfString(this.responses[i].responseText, base.width - 16) + margin)
					{
						this.selectedResponse = i;
						if (i < this.responseCC?.Count)
						{
							base.currentlySnappedComponent = this.responseCC[i];
						}
						break;
					}
					responseY += SpriteText.getHeightOfString(this.responses[i].responseText, base.width - 16) + 16;
				}
				if (this.selectedResponse != oldResponse)
				{
					Game1.playSound("Cowboy_gunshot");
				}
			}
		}
		if (this.shouldDrawFriendshipJewel() && this.friendshipJewel.Contains(mouseX, mouseY))
		{
			this.hoverText = Game1.player.getFriendshipHeartLevelForNPC(this.characterDialogue.speaker.Name) + "/" + Utility.GetMaximumHeartsForCharacter(this.characterDialogue.speaker) + "<";
		}
		if (Game1.options.SnappyMenus && base.currentlySnappedComponent != null)
		{
			this.selectedResponse = base.currentlySnappedComponent.myID;
		}
	}

	public bool shouldDrawFriendshipJewel()
	{
		if (base.width >= 642 && !Game1.eventUp && !this.isQuestion && this.isPortraitBox() && !this.friendshipJewel.Equals(Rectangle.Empty) && this.characterDialogue?.speaker != null && Game1.player.friendshipData.ContainsKey(this.characterDialogue.speaker.Name) && this.characterDialogue.speaker.Name != "Henchman")
		{
			return true;
		}
		return false;
	}

	private void setUpQuestionIcon()
	{
	}

	private void setUpCloseDialogueIcon()
	{
		Vector2 iconPosition = new Vector2(this.x + base.width - 40, this.y + base.height - 44);
		if (this.isPortraitBox())
		{
			iconPosition.X -= 492f;
		}
		this.dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(289, 342, 11, 12), 80f, 11, 999999, iconPosition, flicker: false, flipped: false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
	}

	private void setUpNextPageIcon()
	{
		Vector2 iconPosition = new Vector2(this.x + base.width - 40, this.y + base.height - 40);
		if (this.isPortraitBox())
		{
			iconPosition.X -= 492f;
		}
		this.dialogueIcon = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(232, 346, 9, 9), 90f, 6, 999999, iconPosition, flicker: false, flipped: false, 0.89f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
		{
			yPeriodic = true,
			yPeriodicLoopTime = 1500f,
			yPeriodicRange = 8f
		};
	}

	private void checkDialogue(Dialogue d)
	{
		this.isQuestion = false;
		string sub = "";
		if (this.characterDialoguesBrokenUp.Count == 1)
		{
			sub = SpriteText.getSubstringBeyondHeight(this.characterDialoguesBrokenUp.Peek(), base.width - 460 - 20, base.height - 16);
		}
		if (sub.Length > 0)
		{
			string full = this.characterDialoguesBrokenUp.Pop().Replace(Environment.NewLine, "");
			this.characterDialoguesBrokenUp.Push(sub.Trim());
			this.characterDialoguesBrokenUp.Push(full.Substring(0, full.Length - sub.Length + 1).Trim());
		}
		if (d.getCurrentDialogue().Length == 0)
		{
			this.dialogueFinished = true;
		}
		if (d.isCurrentStringContinuedOnNextScreen || this.characterDialoguesBrokenUp.Count > 1)
		{
			this.dialogueContinuedOnNextPage = true;
		}
		else if (d.getCurrentDialogue().Length == 0)
		{
			this.beginOutro();
		}
		if (d.isCurrentDialogueAQuestion())
		{
			this.responses = d.getResponseOptions();
			this.isQuestion = true;
			this.setUpQuestions();
		}
	}

	private void setUpQuestions()
	{
		int tmpwidth = base.width - 16;
		this.heightForQuestions = SpriteText.getHeightOfString(this.getCurrentString(), tmpwidth);
		Response[] array = this.responses;
		foreach (Response r in array)
		{
			this.heightForQuestions += SpriteText.getHeightOfString(r.responseText, tmpwidth) + 16;
		}
		this.heightForQuestions += 40;
	}

	public bool isPortraitBox()
	{
		if (this.characterDialogue?.speaker?.Portrait != null && this.characterDialogue.showPortrait)
		{
			return Game1.options.showPortraits;
		}
		return false;
	}

	public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
	{
		if (this.transitionInitialized)
		{
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(306, 320, 16, 16), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		}
	}

	/// <summary>Get whether the current portrait should shake.</summary>
	/// <param name="d">The dialogue being displayed.</param>
	private bool shouldPortraitShake(Dialogue d)
	{
		if (this.newPortaitShakeTimer > 0)
		{
			return true;
		}
		List<int> shakePortraits = d.speaker.GetData()?.ShakePortraits;
		if (shakePortraits != null && shakePortraits.Count > 0)
		{
			return shakePortraits.Contains(d.getPortraitIndex());
		}
		return false;
	}

	public void drawPortrait(SpriteBatch b)
	{
		NPC speaker = this.characterDialogue.speaker;
		if (!Game1.IsMasterGame && !speaker.EventActor)
		{
			GameLocation currentLocation = speaker.currentLocation;
			if (currentLocation == null || !currentLocation.IsActiveLocation())
			{
				NPC actualSpeaker = Game1.getCharacterFromName(speaker.Name);
				if (actualSpeaker != null && actualSpeaker.currentLocation.IsActiveLocation())
				{
					speaker = actualSpeaker;
				}
			}
		}
		if (base.width >= 642)
		{
			int xPositionOfPortraitArea = this.x + base.width - 448 + 4;
			int widthOfPortraitArea = this.x + base.width - xPositionOfPortraitArea;
			b.Draw(Game1.mouseCursors, new Rectangle(xPositionOfPortraitArea - 40, this.y, 36, base.height), new Rectangle(278, 324, 9, 1), Color.White);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOfPortraitArea - 40, this.y - 20), new Rectangle(278, 313, 10, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOfPortraitArea - 40, this.y + base.height), new Rectangle(278, 328, 10, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			int portraitBoxX = xPositionOfPortraitArea + 76;
			int portraitBoxY = this.y + base.height / 2 - 148 - 36;
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOfPortraitArea - 8, this.y), new Rectangle(583, 411, 115, 97), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			Texture2D portraitTexture = this.characterDialogue.overridePortrait ?? speaker.Portrait;
			Rectangle portraitSource = Game1.getSourceRectForStandardTileSheet(portraitTexture, this.characterDialogue.getPortraitIndex(), 64, 64);
			if (!portraitTexture.Bounds.Contains(portraitSource))
			{
				portraitSource = new Rectangle(0, 0, 64, 64);
			}
			int xOffset = (this.shouldPortraitShake(this.characterDialogue) ? Game1.random.Next(-1, 2) : 0);
			b.Draw(portraitTexture, new Vector2(portraitBoxX + 16 + xOffset, portraitBoxY + 24), portraitSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			SpriteText.drawStringHorizontallyCenteredAt(b, speaker.getName(), xPositionOfPortraitArea + widthOfPortraitArea / 2, portraitBoxY + 296 + 16);
			if (this.shouldDrawFriendshipJewel())
			{
				b.Draw(Game1.mouseCursors, new Vector2(this.friendshipJewel.X, this.friendshipJewel.Y), (Game1.player.getFriendshipHeartLevelForNPC(speaker.Name) >= 10) ? new Rectangle(269, 494, 11, 11) : new Rectangle(Math.Max(140, 140 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 / 250.0) * 11), Math.Max(532, 532 + Game1.player.getFriendshipHeartLevelForNPC(speaker.Name) / 2 * 11), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
		}
	}

	public string getCurrentString()
	{
		if (this.characterDialogue != null)
		{
			string s = ((this.characterDialoguesBrokenUp.Count <= 0) ? this.characterDialogue.getCurrentDialogue().Trim().Replace(Environment.NewLine, "") : this.characterDialoguesBrokenUp.Peek().Trim().Replace(Environment.NewLine, ""));
			if (!Game1.options.showPortraits)
			{
				s = this.characterDialogue.speaker.getName() + ": " + s;
			}
			return s;
		}
		if (this.dialogues.Count > 0)
		{
			return this.dialogues[0].Trim().Replace(Environment.NewLine, "");
		}
		return "";
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (Game1.options.SnappyMenus && !Game1.lastCursorMotionWasMouse)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		else
		{
			Game1.mouseCursorTransparency = 1f;
		}
		if (this.isQuestion && this.characterIndexInDialogue >= this.getCurrentString().Length - 1 && !this.transitioning)
		{
			Game1.mouseCursorTransparency = 1f;
			if (!this._showedOptions)
			{
				this._showedOptions = true;
				if (this.responses != null)
				{
					this.responseCC = new List<ClickableComponent>();
					int responseY = this.y - (this.heightForQuestions - base.height) + SpriteText.getHeightOfString(this.getCurrentString(), base.width) + 48;
					for (int i = 0; i < this.responses.Length; i++)
					{
						this.responseCC.Add(new ClickableComponent(new Rectangle(this.x + 8, responseY, base.width - 8, SpriteText.getHeightOfString(this.responses[i].responseText, base.width) + 16), "")
						{
							myID = i,
							downNeighborID = ((i < this.responses.Length - 1) ? (i + 1) : (-1)),
							upNeighborID = ((i > 0) ? (i - 1) : (-1))
						});
						responseY += SpriteText.getHeightOfString(this.responses[i].responseText, base.width) + 16;
					}
				}
				this.populateClickableComponentList();
				if (Game1.options.gamepadControls)
				{
					this.snapToDefaultClickableComponent();
					this.selectedResponse = base.currentlySnappedComponent.myID;
				}
			}
		}
		if (this.safetyTimer > 0)
		{
			this.safetyTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (!Game1.IsDedicatedHost && this.questionFinishPauseTimer > 0)
		{
			this.questionFinishPauseTimer -= time.ElapsedGameTime.Milliseconds;
			return;
		}
		if (this.transitioning)
		{
			if (!this.transitionInitialized)
			{
				this.transitionInitialized = true;
				this.transitionX = this.x + base.width / 2;
				this.transitionY = this.y + base.height / 2;
				this.transitionWidth = 0;
				this.transitionHeight = 0;
			}
			if (this.transitioningBigger)
			{
				int num = this.transitionWidth;
				this.transitionX -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
				this.transitionY -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(this.isQuestion ? this.heightForQuestions : base.height) / (float)base.width));
				this.transitionX = Math.Max(this.x, this.transitionX);
				this.transitionY = Math.Max(this.isQuestion ? (this.y + base.height - this.heightForQuestions) : this.y, this.transitionY);
				this.transitionWidth += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
				this.transitionHeight += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(this.isQuestion ? this.heightForQuestions : base.height) / (float)base.width) * 2f);
				this.transitionWidth = Math.Min(base.width, this.transitionWidth);
				this.transitionHeight = Math.Min(this.isQuestion ? this.heightForQuestions : base.height, this.transitionHeight);
				if (num == 0 && this.transitionWidth > 0)
				{
					this.playOpeningSound();
				}
				if (Game1.IsDedicatedHost || (this.transitionX == this.x && this.transitionY == (this.isQuestion ? (this.y + base.height - this.heightForQuestions) : this.y)))
				{
					this.transitioning = false;
					this.characterAdvanceTimer = 90;
					this.setUpIcons();
					this.transitionX = this.x;
					this.transitionY = this.y;
					this.transitionWidth = base.width;
					this.transitionHeight = base.height;
				}
			}
			else
			{
				this.transitionX += (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
				this.transitionY += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)base.height / (float)base.width));
				this.transitionX = Math.Min(this.x + base.width / 2, this.transitionX);
				this.transitionY = Math.Min(this.y + base.height / 2, this.transitionY);
				this.transitionWidth -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
				this.transitionHeight -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)base.height / (float)base.width) * 2f);
				this.transitionWidth = Math.Max(0, this.transitionWidth);
				this.transitionHeight = Math.Max(0, this.transitionHeight);
				if (Game1.IsDedicatedHost || (this.transitionWidth == 0 && this.transitionHeight == 0))
				{
					this.closeDialogue();
				}
			}
		}
		if (!this.transitioning && !this.showTyping && this.characterIndexInDialogue < this.getCurrentString().Length)
		{
			this.finishTyping();
		}
		if (!this.transitioning && this.characterIndexInDialogue < this.getCurrentString().Length)
		{
			this.characterAdvanceTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.characterAdvanceTimer <= 0 || Game1.IsDedicatedHost)
			{
				this.characterAdvanceTimer = 30;
				int old = this.characterIndexInDialogue;
				this.characterIndexInDialogue = Math.Min(this.characterIndexInDialogue + 1, this.getCurrentString().Length);
				if (this.characterIndexInDialogue != old && this.characterIndexInDialogue == this.getCurrentString().Length)
				{
					Game1.playSound("dialogueCharacterClose");
				}
				if (this.characterIndexInDialogue > 1 && this.characterIndexInDialogue < this.getCurrentString().Length && Game1.options.dialogueTyping)
				{
					Game1.playSound("dialogueCharacter");
				}
			}
		}
		if (!this.transitioning && this.dialogueIcon != null)
		{
			this.dialogueIcon.update(time);
		}
		if (!this.transitioning && this.newPortaitShakeTimer > 0)
		{
			this.newPortaitShakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.width = 1200;
		base.height = 384;
		this.x = (int)Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height).X;
		this.y = Game1.uiViewport.Height - base.height - 64;
		this.friendshipJewel = new Rectangle(this.x + base.width - 64, this.y + 256, 44, 44);
		this.setUpIcons();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (base.width < 16 || base.height < 16)
		{
			return;
		}
		if (this.transitioning)
		{
			this.drawBox(b, this.transitionX, this.transitionY, this.transitionWidth, this.transitionHeight);
			base.drawMouse(b);
			return;
		}
		if (this.isQuestion)
		{
			this.drawBox(b, this.x, this.y - (this.heightForQuestions - base.height), base.width, this.heightForQuestions);
			b.Draw(Game1.mouseCursors_1_6, new Vector2(this.x + base.width - 72, this.y - (this.heightForQuestions - base.height) - 88), new Rectangle(495, 461, 17, 19), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors_1_6, new Vector2(this.x + base.width - 52, this.y - (this.heightForQuestions - base.height) - 88 + 16), new Rectangle(470 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 900 / 150 * 7, 447, 7, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			SpriteText.drawString(b, this.getCurrentString(), this.x + 8, this.y + 12 - (this.heightForQuestions - base.height), this.characterIndexInDialogue, base.width - 16);
			if (this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
			{
				int responseY = this.y - (this.heightForQuestions - base.height) + SpriteText.getHeightOfString(this.getCurrentString(), base.width - 16) + 48;
				for (int i = 0; i < this.responses.Length; i++)
				{
					if (i == this.selectedResponse)
					{
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), this.x + 4, responseY - 8, base.width - 8, SpriteText.getHeightOfString(this.responses[i].responseText, base.width) + 16, Color.White, 4f, drawShadow: false);
					}
					SpriteText.drawString(b, this.responses[i].responseText, this.x + 8, responseY, 999999, base.width, 999999, (this.selectedResponse == i) ? 1f : 0.6f);
					responseY += SpriteText.getHeightOfString(this.responses[i].responseText, base.width) + 16;
				}
			}
		}
		else
		{
			this.drawBox(b, this.x, this.y, base.width, base.height);
			if (!this.isPortraitBox() && !this.isQuestion)
			{
				SpriteText.drawString(b, this.getCurrentString(), this.x + 8, this.y + 8, this.characterIndexInDialogue, base.width);
			}
		}
		if (this.isPortraitBox() && !this.isQuestion)
		{
			this.drawPortrait(b);
			if (!this.isQuestion)
			{
				SpriteText.drawString(b, this.getCurrentString(), this.x + 8, this.y + 8, this.characterIndexInDialogue, base.width - 460 - 24);
			}
		}
		if (this.dialogueIcon != null && this.characterIndexInDialogue >= this.getCurrentString().Length - 1)
		{
			this.dialogueIcon.draw(b, localPosition: true);
		}
		if (this.aboveDialogueImage != null)
		{
			this.drawBox(b, this.x + base.width / 2 - (int)((float)(this.aboveDialogueImage.sourceRect.Width / 2) * this.aboveDialogueImage.scale), this.y - 64 - 4 - (int)((float)this.aboveDialogueImage.sourceRect.Height * this.aboveDialogueImage.scale), (int)((float)this.aboveDialogueImage.sourceRect.Width * this.aboveDialogueImage.scale), (int)((float)this.aboveDialogueImage.sourceRect.Height * this.aboveDialogueImage.scale) + 8);
			Utility.drawWithShadow(b, this.aboveDialogueImage.texture, new Vector2((float)(this.x + base.width / 2) - (float)(this.aboveDialogueImage.sourceRect.Width / 2) * this.aboveDialogueImage.scale, this.y - 64 - (int)((float)this.aboveDialogueImage.sourceRect.Height * this.aboveDialogueImage.scale)), this.aboveDialogueImage.sourceRect, Color.White, 0f, Vector2.Zero, this.aboveDialogueImage.scale, flipped: false, 1f);
		}
		if (this.hoverText.Length > 0)
		{
			SpriteText.drawStringWithScrollBackground(b, this.hoverText, this.friendshipJewel.Center.X - SpriteText.getWidthOfString(this.hoverText) / 2, this.friendshipJewel.Y - 64);
		}
		base.drawMouse(b);
	}
}
