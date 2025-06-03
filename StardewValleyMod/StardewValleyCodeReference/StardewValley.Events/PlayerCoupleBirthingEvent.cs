using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Events;

public class PlayerCoupleBirthingEvent : BaseFarmEvent
{
	private int timer;

	private string soundName;

	private string message;

	private string babyName;

	private bool playedSound;

	private bool isMale;

	private bool getBabyName;

	private bool naming;

	private FarmHouse farmHouse;

	private long spouseID;

	private Farmer spouse;

	private bool isPlayersTurn;

	private Child child;

	public PlayerCoupleBirthingEvent()
	{
		this.spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
		Game1.otherFarmers.TryGetValue(this.spouseID, out this.spouse);
		this.farmHouse = this.chooseHome();
	}

	private bool isSuitableHome(FarmHouse home)
	{
		if (home.getChildrenCount() < 2)
		{
			return home.upgradeLevel >= 2;
		}
		return false;
	}

	private FarmHouse chooseHome()
	{
		List<Farmer> parents = new List<Farmer>
		{
			Game1.player,
			this.spouse
		};
		parents.Sort((Farmer p1, Farmer p2) => p1.UniqueMultiplayerID.CompareTo(p2.UniqueMultiplayerID));
		foreach (Farmer parent in parents)
		{
			if (Game1.getLocationFromName(parent.homeLocation.Value) is FarmHouse home && home == parent.currentLocation && this.isSuitableHome(home))
			{
				return home;
			}
		}
		foreach (Farmer item in parents)
		{
			if (Game1.getLocationFromName(item.homeLocation.Value) is FarmHouse home2 && this.isSuitableHome(home2))
			{
				return home2;
			}
		}
		return Game1.player.currentLocation as FarmHouse;
	}

	/// <inheritdoc />
	public override bool setUp()
	{
		if (this.spouse == null || this.farmHouse == null)
		{
			return true;
		}
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.Date.TotalDays);
		Game1.player.CanMove = false;
		if (this.farmHouse.getChildrenCount() == 0)
		{
			this.isMale = r.NextBool();
		}
		else
		{
			this.isMale = this.farmHouse.getChildren()[0].Gender == Gender.Female;
		}
		Friendship friendship = Game1.player.GetSpouseFriendship();
		this.isPlayersTurn = friendship.Proposer != Game1.player.UniqueMultiplayerID == (this.farmHouse.getChildrenCount() % 2 == 0);
		if (this.spouse.IsMale == Game1.player.IsMale)
		{
			this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(this.isMale));
		}
		else if (this.spouse.IsMale)
		{
			this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(this.isMale));
		}
		else
		{
			this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(this.isMale), this.spouse.Name);
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
		if (this.isPlayersTurn)
		{
			this.getBabyName = true;
			double chance = (this.spouse.hasDarkSkin() ? 0.5 : 0.0);
			chance += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
			bool isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).NextDouble() < chance;
			this.farmHouse.characters.Add(this.child = new Child("Baby", this.isMale, isDarkSkinned, Game1.player));
			this.child.Age = 0;
			this.child.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
			Game1.player.stats.checkForFullHouseAchievement(isDirectUnlock: true);
			Game1.player.GetSpouseFriendship().NextBirthingDate = null;
		}
		else
		{
			Game1.afterDialogues = delegate
			{
				this.getBabyName = true;
			};
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseNaming_" + (this.isMale ? "Male" : "Female"), this.spouse.Name));
		}
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
			if (!this.isPlayersTurn)
			{
				Game1.globalFadeToClear();
				return true;
			}
			if (!this.naming)
			{
				Game1.activeClickableMenu = new NamingMenu(returnBabyName, Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
				this.naming = true;
			}
			if (!string.IsNullOrEmpty(this.babyName) && this.babyName.Length > 0)
			{
				string newBabyName = this.babyName;
				List<NPC> all_characters = Utility.getAllCharacters();
				bool collision_found;
				do
				{
					collision_found = false;
					foreach (NPC item in all_characters)
					{
						if (item.Name == newBabyName)
						{
							newBabyName += " ";
							collision_found = true;
							break;
						}
					}
				}
				while (collision_found);
				this.child.Name = newBabyName;
				Game1.playSound("smallSelect");
				if (Game1.keyboardDispatcher != null)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
				Game1.globalFadeToClear();
				return true;
			}
		}
		return false;
	}
}
