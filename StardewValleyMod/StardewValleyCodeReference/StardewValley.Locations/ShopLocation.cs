using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Locations;

public class ShopLocation : GameLocation
{
	public const int maxItemsToSellFromPlayer = 11;

	public readonly NetObjectList<Item> itemsFromPlayerToSell = new NetObjectList<Item>();

	public readonly NetObjectList<Item> itemsToStartSellingTomorrow = new NetObjectList<Item>();

	public ShopLocation()
	{
	}

	public ShopLocation(string map, string name)
		: base(map, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.itemsFromPlayerToSell, "itemsFromPlayerToSell").AddField(this.itemsToStartSellingTomorrow, "itemsToStartSellingTomorrow");
	}

	/// <summary>Get a dialogue for an NPC when the player purchases an item from the shop, if they have any.</summary>
	/// <param name="i">The item that was purchased.</param>
	/// <param name="n">The NPC for which to get a dialogue.</param>
	/// <returns>Returns a dialogue to use, or <c>null</c> to skip this NPC.</returns>
	public virtual Dialogue getPurchasedItemDialogueForNPC(Object i, NPC n)
	{
		Dialogue response = null;
		string[] split = Game1.content.LoadString("Strings\\Lexicon:GenericPlayerTerm").Split('^');
		string genderName = split[0];
		if (split.Length > 1 && !Game1.player.IsMale)
		{
			genderName = split[1];
		}
		string whatToCallPlayer = ((Game1.random.NextDouble() < (double)(Game1.player.getFriendshipLevelForNPC(n.Name) / 1250)) ? Game1.player.Name : genderName);
		if (n.Age != 0)
		{
			whatToCallPlayer = Game1.player.Name;
		}
		string particle = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? Lexicon.getProperArticleForWord(i.name) : "");
		if ((i.Category == -4 || i.Category == -75 || i.Category == -79) && Game1.random.NextBool())
		{
			particle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SeedShop.cs.9701");
		}
		int whichDialogue = Game1.random.Next(5);
		if (n.Manners == 2)
		{
			whichDialogue = 2;
		}
		switch (whichDialogue)
		{
		case 0:
			response = ((!(Game1.random.NextDouble() < (double)i.quality.Value * 0.5 + 0.2)) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityLow", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomNegativeFoodAdjective(n)) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_1_QualityHigh", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomDeliciousAdjective(n)));
			break;
		case 1:
			response = ((i.quality.Value != 0) ? ((!n.Name.Equals("Jodi")) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh", whatToCallPlayer, particle, i.DisplayName) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi", whatToCallPlayer, particle, i.DisplayName)) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_2_QualityLow", whatToCallPlayer, particle, i.DisplayName));
			break;
		case 2:
			if (n.Manners == 2)
			{
				response = ((i.quality.Value == 2) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n)) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomNegativeFoodAdjective(n), Lexicon.getRandomNegativeItemSlanderNoun()));
			}
			else
			{
				Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_3_NonRude", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2);
			}
			break;
		case 3:
			response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_4", whatToCallPlayer, particle, i.DisplayName);
			break;
		case 4:
			switch (i.Category)
			{
			case -79:
			case -75:
				response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_5_VegetableOrFruit", whatToCallPlayer, particle, i.DisplayName);
				break;
			case -7:
			{
				string adjective = Lexicon.getRandomPositiveAdjectiveForEventOrPerson(n);
				response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_5_Cooking", whatToCallPlayer, particle, i.DisplayName, Lexicon.getProperArticleForWord(adjective), adjective);
				break;
			}
			default:
				response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_5_Foraged", whatToCallPlayer, particle, i.DisplayName);
				break;
			}
			break;
		}
		if (n.Age == 1 && Game1.random.NextDouble() < 0.6)
		{
			response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Teen", whatToCallPlayer, particle, i.DisplayName);
		}
		switch (n.Name)
		{
		case "Abigail":
			response = ((i.quality.Value != 0) ? Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Abigail_QualityHigh", whatToCallPlayer, particle, i.DisplayName) : Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Abigail_QualityLow", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomNegativeItemSlanderNoun()));
			break;
		case "Caroline":
		{
			string key2 = ((i.quality.Value == 0) ? "Data\\ExtraDialogue:PurchasedItem_Caroline_QualityLow" : "Data\\ExtraDialogue:PurchasedItem_Caroline_QualityHigh");
			response = Dialogue.FromTranslation(n, key2, whatToCallPlayer, particle, i.DisplayName);
			break;
		}
		case "Pierre":
		{
			string key = ((i.quality.Value == 0) ? "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow" : "Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh");
			response = Dialogue.FromTranslation(n, key, whatToCallPlayer, particle, i.DisplayName);
			break;
		}
		case "Haley":
			response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Haley", whatToCallPlayer, particle, i.DisplayName);
			break;
		case "Elliott":
			response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Elliott", whatToCallPlayer, particle, i.DisplayName);
			break;
		case "Alex":
			response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Alex", whatToCallPlayer, particle, i.DisplayName);
			break;
		case "Leah":
			response = Dialogue.FromTranslation(n, "Data\\ExtraDialogue:PurchasedItem_Leah", whatToCallPlayer, particle, i.DisplayName);
			break;
		}
		return response;
	}

	public override void DayUpdate(int dayOfMonth)
	{
		this.itemsToStartSellingTomorrow.RemoveWhere((Item p) => p == null);
		this.itemsFromPlayerToSell.RemoveWhere((Item p) => p == null);
		for (int i = this.itemsToStartSellingTomorrow.Count - 1; i >= 0; i--)
		{
			Item tomorrowItem = this.itemsToStartSellingTomorrow[i];
			if (this.itemsFromPlayerToSell.Count < 11)
			{
				bool stacked = false;
				foreach (Item item in this.itemsFromPlayerToSell)
				{
					if (item.Name == tomorrowItem.Name && item.Quality == tomorrowItem.Quality)
					{
						item.Stack += tomorrowItem.Stack;
						stacked = true;
						break;
					}
				}
				this.itemsToStartSellingTomorrow.RemoveAt(i);
				if (!stacked)
				{
					this.itemsFromPlayerToSell.Add(tomorrowItem);
				}
			}
		}
		base.DayUpdate(dayOfMonth);
	}
}
