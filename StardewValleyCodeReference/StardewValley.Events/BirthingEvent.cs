using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewValley.Events;

public class BirthingEvent : BaseFarmEvent
{
	private int timer;

	private string soundName;

	private string message;

	private string babyName;

	private bool playedSound;

	private bool isMale;

	private bool getBabyName;

	private bool naming;

	/// <inheritdoc />
	public override bool setUp()
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		NPC spouse = Game1.RequireCharacter(Game1.player.spouse);
		Game1.player.CanMove = false;
		if (Game1.player.getNumberOfChildren() == 0)
		{
			this.isMale = r.NextBool();
		}
		else
		{
			this.isMale = Game1.player.getChildren()[0].Gender == Gender.Female;
		}
		if (spouse.isAdoptionSpouse())
		{
			this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(this.isMale));
		}
		else if (spouse.Gender == Gender.Male)
		{
			this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(this.isMale));
		}
		else
		{
			this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(this.isMale), spouse.displayName);
		}
		return false;
	}

	public void returnBabyName(string name)
	{
		this.babyName = name;
		Game1.exitActiveMenu();
	}

	public void afterMessage()
	{
		this.getBabyName = true;
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		Game1.player.CanMove = false;
		this.timer += time.ElapsedGameTime.Milliseconds;
		Game1.fadeToBlackAlpha = 1f;
		if (this.timer > 1500 && !this.playedSound && !this.getBabyName)
		{
			if (!string.IsNullOrEmpty(this.soundName))
			{
				Game1.playSound(this.soundName);
				this.playedSound = true;
			}
			if (!this.playedSound && this.message != null && !Game1.dialogueUp && Game1.activeClickableMenu == null)
			{
				Game1.drawObjectDialogue(this.message);
				Game1.afterDialogues = afterMessage;
			}
		}
		else if (this.getBabyName)
		{
			if (!this.naming)
			{
				Game1.activeClickableMenu = new NamingMenu(returnBabyName, Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
				this.naming = true;
			}
			if (!string.IsNullOrEmpty(this.babyName) && this.babyName.Length > 0)
			{
				NPC spouse = Game1.player.getSpouse();
				double chance = (spouse.hasDarkSkin() ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
				bool isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).NextBool(chance);
				string newBabyName = this.babyName;
				List<NPC> all_characters = Utility.getAllCharacters();
				bool collision_found;
				do
				{
					collision_found = false;
					if (Game1.characterData.ContainsKey(newBabyName))
					{
						newBabyName += " ";
						collision_found = true;
						continue;
					}
					foreach (NPC item in all_characters)
					{
						if (item.Name == newBabyName)
						{
							newBabyName += " ";
							collision_found = true;
						}
					}
				}
				while (collision_found);
				Child baby = new Child(newBabyName, this.isMale, isDarkSkinned, Game1.player);
				baby.Age = 0;
				baby.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
				Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
				Game1.stats.checkForFullHouseAchievement(isDirectUnlock: true);
				Game1.playSound("smallSelect");
				spouse.daysAfterLastBirth = 5;
				Game1.player.GetSpouseFriendship().NextBirthingDate = null;
				if (Game1.player.getChildrenCount() == 2)
				{
					spouse.shouldSayMarriageDialogue.Value = true;
					spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + Game1.random.Next(1, 3), true));
				}
				else if (spouse.isAdoptionSpouse())
				{
					spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, this.babyName));
				}
				else
				{
					spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, this.babyName));
				}
				Game1.morningQueue.Enqueue(delegate
				{
					string text = Game1.getCharacterFromName(Game1.player.spouse)?.GetTokenizedDisplayName() ?? Game1.player.spouse;
					Game1.multiplayer.globalChatInfoMessage("Baby", Lexicon.capitalize(Game1.player.Name), text, Lexicon.getTokenizedGenderedChildTerm(this.isMale), Lexicon.getTokenizedPronoun(this.isMale), baby.displayName);
				});
				if (Game1.keyboardDispatcher != null)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
				Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).GetPlayerBedSpot()) * 64f;
				Game1.globalFadeToClear();
				return true;
			}
		}
		return false;
	}
}
