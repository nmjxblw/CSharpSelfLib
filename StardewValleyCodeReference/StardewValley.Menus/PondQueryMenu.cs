using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Menus;

public class PondQueryMenu : IClickableMenu
{
	public const int region_okButton = 101;

	public const int region_emptyButton = 103;

	public const int region_noButton = 105;

	public const int region_nettingButton = 106;

	public new static int width = 384;

	public new static int height = 512;

	public const int unresolved_needs_extra_height = 116;

	protected FishPond _pond;

	protected Object _fishItem;

	protected string _statusText = "";

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent emptyButton;

	public ClickableTextureComponent yesButton;

	public ClickableTextureComponent noButton;

	public ClickableTextureComponent changeNettingButton;

	private bool confirmingEmpty;

	protected Rectangle _confirmationBoxRectangle;

	protected string _confirmationText;

	protected float _age;

	private string hoverText = "";

	public PondQueryMenu(FishPond fish_pond)
		: base(Game1.uiViewport.Width / 2 - PondQueryMenu.width / 2, Game1.uiViewport.Height / 2 - PondQueryMenu.height / 2, PondQueryMenu.width, PondQueryMenu.height)
	{
		Game1.player.Halt();
		PondQueryMenu.width = 384;
		PondQueryMenu.height = 512;
		this._pond = fish_pond;
		this._fishItem = new Object(this._pond.fishType.Value, 1);
		this.okButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + PondQueryMenu.width + 4, base.yPositionOnScreen + PondQueryMenu.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101,
			upNeighborID = -99998
		};
		this.emptyButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + PondQueryMenu.width + 4, base.yPositionOnScreen + PondQueryMenu.height - 256 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Rectangle(32, 384, 16, 16), 4f)
		{
			myID = 103,
			downNeighborID = -99998
		};
		this.changeNettingButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + PondQueryMenu.width + 4, base.yPositionOnScreen + PondQueryMenu.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, new Rectangle(48, 384, 16, 16), 4f)
		{
			myID = 106,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
		this.UpdateState();
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.measureTotalHeight() / 2;
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(101);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.globalFade)
		{
			return;
		}
		if (Game1.options.menuButton.Contains(new InputButton(key)))
		{
			Game1.playSound("smallSelect");
			if (this.readyToClose())
			{
				Game1.exitActiveMenu();
			}
		}
		else if (Game1.options.SnappyMenus && !Game1.options.menuButton.Contains(new InputButton(key)))
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		this._age += (float)time.ElapsedGameTime.TotalSeconds;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (Game1.globalFade)
		{
			return;
		}
		if (this.confirmingEmpty)
		{
			if (this.yesButton.containsPoint(x, y))
			{
				Game1.playSound("fishSlap");
				this._pond.ClearPond();
				base.exitThisMenu();
			}
			else if (this.noButton.containsPoint(x, y))
			{
				this.confirmingEmpty = false;
				Game1.playSound("smallSelect");
				if (Game1.options.SnappyMenus)
				{
					base.currentlySnappedComponent = base.getComponentWithID(103);
					this.snapCursorToCurrentSnappedComponent();
				}
			}
			return;
		}
		if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
		{
			Game1.exitActiveMenu();
			Game1.playSound("smallSelect");
		}
		if (this.changeNettingButton.containsPoint(x, y))
		{
			Game1.playSound("drumkit6");
			this._pond.nettingStyle.Value++;
			this._pond.nettingStyle.Value %= 4;
		}
		else if (this.emptyButton.containsPoint(x, y))
		{
			this._confirmationBoxRectangle = new Rectangle(0, 0, 400, 100);
			this._confirmationBoxRectangle.X = Game1.uiViewport.Width / 2 - this._confirmationBoxRectangle.Width / 2;
			this._confirmationText = Game1.content.LoadString("Strings\\UI:PondQuery_ConfirmEmpty");
			this._confirmationText = Game1.parseText(this._confirmationText, Game1.smallFont, this._confirmationBoxRectangle.Width);
			Vector2 text_size = Game1.smallFont.MeasureString(this._confirmationText);
			this._confirmationBoxRectangle.Height = (int)text_size.Y;
			this._confirmationBoxRectangle.Y = Game1.uiViewport.Height / 2 - this._confirmationBoxRectangle.Height / 2;
			this.confirmingEmpty = true;
			this.yesButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 - 64 - 4, this._confirmationBoxRectangle.Bottom + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 111,
				rightNeighborID = 105
			};
			this.noButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 + 4, this._confirmationBoxRectangle.Bottom + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 105,
				leftNeighborID = 111
			};
			Game1.playSound("smallSelect");
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				base.currentlySnappedComponent = this.noButton;
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public override bool readyToClose()
	{
		if (base.readyToClose())
		{
			return !Game1.globalFade;
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (!Game1.globalFade && this.readyToClose())
		{
			Game1.exitActiveMenu();
			Game1.playSound("smallSelect");
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		if (this.okButton != null)
		{
			if (this.okButton.containsPoint(x, y))
			{
				this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
			}
			else
			{
				this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
			}
		}
		if (this.emptyButton != null)
		{
			if (this.emptyButton.containsPoint(x, y))
			{
				this.emptyButton.scale = Math.Min(4.1f, this.emptyButton.scale + 0.05f);
				this.hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_EmptyPond", 10);
			}
			else
			{
				this.emptyButton.scale = Math.Max(4f, this.emptyButton.scale - 0.05f);
			}
		}
		if (this.changeNettingButton != null)
		{
			if (this.changeNettingButton.containsPoint(x, y))
			{
				this.changeNettingButton.scale = Math.Min(4.1f, this.changeNettingButton.scale + 0.05f);
				this.hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_ChangeNetting", 10);
			}
			else
			{
				this.changeNettingButton.scale = Math.Max(4f, this.emptyButton.scale - 0.05f);
			}
		}
		if (this.yesButton != null)
		{
			if (this.yesButton.containsPoint(x, y))
			{
				this.yesButton.scale = Math.Min(1.1f, this.yesButton.scale + 0.05f);
			}
			else
			{
				this.yesButton.scale = Math.Max(1f, this.yesButton.scale - 0.05f);
			}
		}
		if (this.noButton != null)
		{
			if (this.noButton.containsPoint(x, y))
			{
				this.noButton.scale = Math.Min(1.1f, this.noButton.scale + 0.05f);
			}
			else
			{
				this.noButton.scale = Math.Max(1f, this.noButton.scale - 0.05f);
			}
		}
	}

	public static string GetFishTalkSuffix(Object fishItem)
	{
		HashSet<string> tags = fishItem.GetContextTags();
		if (tags.Contains("fish_talk_rude"))
		{
			return "_Rude";
		}
		if (tags.Contains("fish_talk_stiff"))
		{
			return "_Stiff";
		}
		if (tags.Contains("fish_talk_demanding"))
		{
			return "_Demanding";
		}
		foreach (string tag in tags)
		{
			if (!tag.StartsWithIgnoreCase("fish_talk_"))
			{
				continue;
			}
			char[] array = tag.Substring("fish_talk".Length).ToCharArray();
			bool capitalizeNext = false;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == '_')
				{
					capitalizeNext = true;
				}
				else if (capitalizeNext)
				{
					array[j] = char.ToUpper(array[j]);
					capitalizeNext = false;
				}
			}
			return new string(array);
		}
		if (tags.Contains("fish_carnivorous"))
		{
			return "_Carnivore";
		}
		return "";
	}

	public static string getCompletedRequestString(FishPond pond, Object fishItem, Random r)
	{
		if (fishItem != null)
		{
			string talk_suffix = PondQueryMenu.GetFishTalkSuffix(fishItem);
			if (talk_suffix != "")
			{
				return Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete" + talk_suffix + r.Next(3), pond.neededItem.Value.DisplayName));
			}
		}
		return Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete" + r.Next(7), pond.neededItem.Value.DisplayName);
	}

	public void UpdateState()
	{
		Random r = Utility.CreateDaySaveRandom(this._pond.seedOffset.Value);
		if (this._pond.currentOccupants.Value <= 0)
		{
			this._statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusNoFish");
			return;
		}
		if (this._pond.neededItem.Value != null)
		{
			if (this._pond.hasCompletedRequest.Value)
			{
				this._statusText = PondQueryMenu.getCompletedRequestString(this._pond, this._fishItem, r);
				return;
			}
			if (this._pond.HasUnresolvedNeeds())
			{
				string item_count_string = this._pond.neededItemCount.Value.ToString() ?? "";
				if (this._pond.neededItemCount.Value <= 1)
				{
					item_count_string = Lexicon.getProperArticleForWord(this._pond.neededItem.Value.DisplayName);
					if (item_count_string == "")
					{
						item_count_string = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestOneCount");
					}
				}
				if (this._fishItem != null)
				{
					if (this._fishItem.HasContextTag("fish_talk_rude"))
					{
						this._statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending_Rude" + r.Next(3) + "_" + (Game1.player.IsMale ? "Male" : "Female"), Lexicon.makePlural(this._pond.neededItem.Value.DisplayName, this._pond.neededItemCount.Value == 1), item_count_string, this._pond.neededItem.Value.DisplayName));
						return;
					}
					string talk_suffix = PondQueryMenu.GetFishTalkSuffix(this._fishItem);
					if (talk_suffix != "")
					{
						this._statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending" + talk_suffix + r.Next(3), Lexicon.makePlural(this._pond.neededItem.Value.DisplayName, this._pond.neededItemCount.Value == 1), item_count_string, this._pond.neededItem.Value.DisplayName));
						return;
					}
				}
				this._statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending" + r.Next(7), Lexicon.makePlural(this._pond.neededItem.Value.DisplayName, this._pond.neededItemCount.Value == 1), item_count_string, this._pond.neededItem.Value.DisplayName));
				return;
			}
		}
		if (this._fishItem != null && (this._fishItem.QualifiedItemId == "(O)397" || this._fishItem.QualifiedItemId == "(O)393"))
		{
			this._statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk_Coral", this._fishItem.DisplayName);
		}
		else
		{
			this._statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk" + r.Next(7));
		}
	}

	private int measureTotalHeight()
	{
		return 644 + this.measureExtraTextHeight(this.getDisplayedText());
	}

	private int measureExtraTextHeight(string displayed_text)
	{
		return Math.Max(0, (int)Game1.smallFont.MeasureString(displayed_text).Y - 90) + 4;
	}

	private string getDisplayedText()
	{
		return Game1.parseText(this._statusText, Game1.smallFont, PondQueryMenu.width - IClickableMenu.spaceToClearSideBorder * 2 - 64);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.globalFade)
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			bool has_unresolved_needs = this._pond.neededItem.Value != null && this._pond.HasUnresolvedNeeds() && !this._pond.hasCompletedRequest.Value;
			string pond_name_text = Game1.content.LoadString("Strings\\UI:PondQuery_Name", this._fishItem.DisplayName);
			Vector2 text_size = Game1.smallFont.MeasureString(pond_name_text);
			Game1.DrawBox((int)((float)(Game1.uiViewport.Width / 2) - (text_size.X + 64f) * 0.5f), base.yPositionOnScreen - 4 + 128, (int)(text_size.X + 64f), 64);
			Utility.drawTextWithShadow(b, pond_name_text, Game1.smallFont, new Vector2((float)(Game1.uiViewport.Width / 2) - text_size.X * 0.5f, (float)(base.yPositionOnScreen - 4) + 160f - text_size.Y * 0.5f), Color.Black);
			string displayed_text = this.getDisplayedText();
			int extraHeight = 0;
			if (has_unresolved_needs)
			{
				extraHeight += 116;
			}
			int extraTextHeight = this.measureExtraTextHeight(displayed_text);
			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen + 128, PondQueryMenu.width, PondQueryMenu.height - 128 + extraHeight + extraTextHeight, speaker: false, drawOnlyBox: true);
			string population_text = Game1.content.LoadString("Strings\\UI:PondQuery_Population", this._pond.FishCount.ToString() ?? "", this._pond.maxOccupants);
			text_size = Game1.smallFont.MeasureString(population_text);
			Utility.drawTextWithShadow(b, population_text, Game1.smallFont, new Vector2(this._pond.goldenAnimalCracker.Value ? ((float)(base.xPositionOnScreen + IClickableMenu.borderWidth + 4)) : ((float)(base.xPositionOnScreen + PondQueryMenu.width / 2) - text_size.X * 0.5f), base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128), Game1.textColor);
			int slots_to_draw = this._pond.maxOccupants.Value;
			float slot_spacing = 13f;
			int x = 0;
			int y = 0;
			for (int i = 0; i < slots_to_draw; i++)
			{
				float y_offset = (float)Math.Sin(this._age * 1f + (float)x * 0.75f + (float)y * 0.25f) * 2f;
				if (i < this._pond.FishCount)
				{
					this._fishItem.drawInMenu(b, new Vector2((float)(base.xPositionOnScreen + PondQueryMenu.width / 2) - slot_spacing * (float)Math.Min(slots_to_draw, 5) * 4f * 0.5f + slot_spacing * 4f * (float)x - 12f, (float)(base.yPositionOnScreen + (int)(y_offset * 4f)) + (float)(y * 4) * slot_spacing + 275.2f), 0.75f, 1f, 0f, StackDrawType.Hide, Color.White, drawShadow: false);
				}
				else
				{
					this._fishItem.drawInMenu(b, new Vector2((float)(base.xPositionOnScreen + PondQueryMenu.width / 2) - slot_spacing * (float)Math.Min(slots_to_draw, 5) * 4f * 0.5f + slot_spacing * 4f * (float)x - 12f, (float)(base.yPositionOnScreen + (int)(y_offset * 4f)) + (float)(y * 4) * slot_spacing + 275.2f), 0.75f, 0.35f, 0f, StackDrawType.Hide, Color.Black, drawShadow: false);
				}
				x++;
				if (x == 5)
				{
					x = 0;
					y++;
				}
			}
			text_size = Game1.smallFont.MeasureString(displayed_text);
			Utility.drawTextWithShadow(b, displayed_text, Game1.smallFont, new Vector2((float)(base.xPositionOnScreen + PondQueryMenu.width / 2) - text_size.X * 0.5f, (float)(base.yPositionOnScreen + PondQueryMenu.height + extraTextHeight - (has_unresolved_needs ? 32 : 48)) - text_size.Y), Game1.textColor);
			if (has_unresolved_needs)
			{
				base.drawHorizontalPartition(b, (int)((float)(base.yPositionOnScreen + PondQueryMenu.height + extraTextHeight) - 48f));
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(base.xPositionOnScreen + 60) + 8f * Game1.dialogueButtonScale / 10f, base.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 28), new Rectangle(412, 495, 5, 4), Color.White, (float)Math.PI / 2f, Vector2.Zero);
				string bring_text = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequest_Bring");
				text_size = Game1.smallFont.MeasureString(bring_text);
				int left_x = base.xPositionOnScreen + 88;
				float text_x = left_x;
				float icon_x = text_x + text_size.X + 4f;
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
				{
					icon_x = left_x - 8;
					text_x = left_x + 76;
				}
				Utility.drawTextWithShadow(b, bring_text, Game1.smallFont, new Vector2(text_x, base.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 24), Game1.textColor);
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this._pond.neededItem.Value.QualifiedItemId);
				Texture2D texture = dataOrErrorItem.GetTexture();
				Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
				b.Draw(texture, new Vector2(icon_x, base.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 4), sourceRect, Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(texture, new Vector2(icon_x + 4f, base.yPositionOnScreen + PondQueryMenu.height + extraTextHeight), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (this._pond.neededItemCount.Value > 1)
				{
					Utility.drawTinyDigits(this._pond.neededItemCount.Value, b, new Vector2(icon_x + 48f, base.yPositionOnScreen + PondQueryMenu.height + extraTextHeight + 48), 3f, 1f, Color.White);
				}
			}
			if (this._pond.goldenAnimalCracker.Value && Game1.objectSpriteSheet_2 != null)
			{
				Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2((float)(base.xPositionOnScreen + PondQueryMenu.width) - 105.6f, (float)base.yPositionOnScreen + 224f), new Rectangle(16, 240, 16, 16), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.89f);
			}
			this.okButton.draw(b);
			this.emptyButton.draw(b);
			this.changeNettingButton.draw(b);
			if (this.confirmingEmpty)
			{
				if (!Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				int padding = 16;
				this._confirmationBoxRectangle.Width += padding;
				this._confirmationBoxRectangle.Height += padding;
				this._confirmationBoxRectangle.X -= padding / 2;
				this._confirmationBoxRectangle.Y -= padding / 2;
				Game1.DrawBox(this._confirmationBoxRectangle.X, this._confirmationBoxRectangle.Y, this._confirmationBoxRectangle.Width, this._confirmationBoxRectangle.Height);
				this._confirmationBoxRectangle.Width -= padding;
				this._confirmationBoxRectangle.Height -= padding;
				this._confirmationBoxRectangle.X += padding / 2;
				this._confirmationBoxRectangle.Y += padding / 2;
				b.DrawString(Game1.smallFont, this._confirmationText, new Vector2(this._confirmationBoxRectangle.X, this._confirmationBoxRectangle.Y), Game1.textColor);
				this.yesButton.draw(b);
				this.noButton.draw(b);
			}
			else
			{
				string text = this.hoverText;
				if (text != null && text.Length > 0)
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
				}
			}
		}
		base.drawMouse(b);
	}
}
