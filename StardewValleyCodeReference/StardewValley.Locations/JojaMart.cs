using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class JojaMart : GameLocation
{
	public const int JojaMembershipPrice = 5000;

	public static NPC Morris;

	private Texture2D communityDevelopmentTexture;

	public JojaMart()
	{
	}

	public JojaMart(string map, string name)
		: base(map, name)
	{
	}

	private bool signUpForJoja(int response)
	{
		if (response == 0)
		{
			base.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:JojaMart_SignUp")), base.createYesNoResponses(), "JojaSignUp");
			return true;
		}
		Game1.dialogueUp = false;
		Game1.player.forceCanMove();
		base.localSound("smallSelect");
		Game1.currentSpeaker = null;
		Game1.dialogueTyping = false;
		return true;
	}

	public override bool answerDialogue(Response answer)
	{
		if (base.lastQuestionKey != null && base.afterQuestion == null && ArgUtility.SplitBySpaceAndGet(base.lastQuestionKey, 0) + "_" + answer.responseKey == "JojaSignUp_Yes")
		{
			if (Game1.player.Money >= 5000)
			{
				Game1.player.Money -= 5000;
				Game1.addMailForTomorrow("JojaMember", noLetter: true, sendToEveryone: true);
				Game1.player.removeQuest("26");
				JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_PlayerSignedUp");
				Game1.drawDialogue(JojaMart.Morris);
			}
			else if (Game1.player.Money < 5000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
			}
			return true;
		}
		return base.answerDialogue(answer);
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings") == "JoinJoja")
		{
			JojaMart.Morris.CurrentDialogue.Clear();
			if (Game1.player.mailForTomorrow.Contains("JojaMember%&NL&%"))
			{
				JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_ComeBackLater");
				Game1.drawDialogue(JojaMart.Morris);
			}
			else if (!Game1.player.mailReceived.Contains("JojaMember"))
			{
				if (Game1.player.mailReceived.Add("JojaGreeting"))
				{
					JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_Greeting");
					Game1.drawDialogue(JojaMart.Morris);
				}
				else if (Game1.stats.DaysPlayed < 0)
				{
					string greeting = ((Game1.dayOfMonth % 7 == 0 || Game1.dayOfMonth % 7 == 6) ? "Data\\ExtraDialogue:Morris_WeekendGreeting" : "Data\\ExtraDialogue:Morris_FirstGreeting");
					JojaMart.Morris.setNewDialogue(greeting);
					Game1.drawDialogue(JojaMart.Morris);
				}
				else
				{
					string greeting2 = ((Game1.dayOfMonth % 7 == 0 || Game1.dayOfMonth % 7 == 6) ? "Data\\ExtraDialogue:Morris_WeekendGreeting" : "Data\\ExtraDialogue:Morris_FirstGreeting");
					if (Game1.IsMasterGame)
					{
						if (!Game1.player.eventsSeen.Contains("611439"))
						{
							JojaMart.Morris.setNewDialogue(greeting2);
							Game1.drawDialogue(JojaMart.Morris);
						}
						else if (Game1.player.mailReceived.Contains("ccIsComplete"))
						{
							JojaMart.Morris.setNewDialogue(greeting2 + "_CommunityCenterComplete");
							Game1.drawDialogue(JojaMart.Morris);
						}
						else
						{
							JojaMart.Morris.setNewDialogue(Dialogue.FromTranslation(JojaMart.Morris, greeting2 + "_MembershipAvailable", 5000));
							JojaMart.Morris.CurrentDialogue.Peek().answerQuestionBehavior = signUpForJoja;
							Game1.drawDialogue(JojaMart.Morris);
						}
					}
					else
					{
						JojaMart.Morris.setNewDialogue(greeting2 + "_SecondPlayer");
						Game1.drawDialogue(JojaMart.Morris);
					}
				}
			}
			else
			{
				if (Game1.player.eventsSeen.Contains("502261") && !Game1.player.hasOrWillReceiveMail("ccMovieTheater"))
				{
					JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_BuyMovieTheater");
					JojaMart.Morris.CurrentDialogue.Peek().answerQuestionBehavior = buyMovieTheater;
				}
				else if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%"))
				{
					JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_StillProcessingOrder");
				}
				else if (Game1.player.eventsSeen.Contains("502261"))
				{
					JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_NoMoreCD");
				}
				else
				{
					JojaMart.Morris.setNewDialogue(Game1.player.IsMale ? "Data\\ExtraDialogue:Morris_CommunityDevelopmentForm_PlayerMale" : "Data\\ExtraDialogue:Morris_CommunityDevelopmentForm_PlayerFemale");
					JojaMart.Morris.CurrentDialogue.Peek().answerQuestionBehavior = viewJojaNote;
				}
				Game1.drawDialogue(JojaMart.Morris);
			}
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	private bool buyMovieTheater(int response)
	{
		if (response == 0)
		{
			if (Game1.player.Money >= 500000)
			{
				Game1.player.Money -= 500000;
				Game1.addMailForTomorrow("ccMovieTheater", noLetter: true, sendToEveryone: true);
				Game1.addMailForTomorrow("ccMovieTheaterJoja", noLetter: true, sendToEveryone: true);
				if (Game1.player.team.theaterBuildDate.Value < 0)
				{
					Game1.player.team.theaterBuildDate.Set(Game1.Date.TotalDays + 1);
				}
				JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_TheaterBought");
				Game1.drawDialogue(JojaMart.Morris);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
			}
		}
		return true;
	}

	private bool viewJojaNote(int response)
	{
		if (response == 0)
		{
			Game1.activeClickableMenu = new JojaCDMenu(this.communityDevelopmentTexture);
			Game1.player.activeDialogueEvents.TryAdd("joja_Begin", 7);
		}
		Game1.dialogueUp = false;
		Game1.player.forceCanMove();
		base.localSound("smallSelect");
		Game1.currentSpeaker = null;
		Game1.dialogueTyping = false;
		return true;
	}

	protected override void resetLocalState()
	{
		this.communityDevelopmentTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JojaCDForm");
		JojaMart.Morris = new NPC(null, Vector2.Zero, "JojaMart", 2, "Morris", datable: false, Game1.temporaryContent.Load<Texture2D>("Portraits\\Morris"));
		base.resetLocalState();
	}
}
