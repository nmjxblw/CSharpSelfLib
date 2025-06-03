using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData;
using StardewValley.GameData.Pets;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

internal class CharacterSubject : BaseSubject
{
	private readonly SubjectType TargetType;

	private readonly NPC Target;

	private readonly ISubjectRegistry Codex;

	private readonly bool ShowUnknownGiftTastes;

	private readonly bool HighlightUnrevealedGiftTastes;

	private readonly ModGiftTasteConfig ShowGiftTastes;

	private readonly ModCollapseLargeFieldsConfig CollapseFieldsConfig;

	private readonly bool EnableTargetRedirection;

	private readonly bool ShowUnownedGifts;

	private readonly bool IsGourmand;

	private readonly bool IsHauntedSkull;

	private readonly bool IsMagmaSprite;

	private readonly bool DisablePortraits;

	public CharacterSubject(ISubjectRegistry codex, GameHelper gameHelper, NPC npc, SubjectType type, Metadata metadata, bool showUnknownGiftTastes, bool highlightUnrevealedGiftTastes, ModGiftTasteConfig showGiftTastes, ModCollapseLargeFieldsConfig collapseFieldsConfig, bool enableTargetRedirection, bool showUnownedGifts)
		: base(gameHelper)
	{
		this.Codex = codex;
		this.ShowUnknownGiftTastes = showUnknownGiftTastes;
		this.HighlightUnrevealedGiftTastes = highlightUnrevealedGiftTastes;
		this.ShowGiftTastes = showGiftTastes;
		this.CollapseFieldsConfig = collapseFieldsConfig;
		this.EnableTargetRedirection = enableTargetRedirection;
		this.ShowUnownedGifts = showUnownedGifts;
		this.Target = npc;
		this.TargetType = type;
		CharacterData overrides = metadata.GetCharacter(npc, type);
		base.Initialize(npc.getName(), ((object)overrides != null && overrides.DescriptionKey != null) ? Translation.op_Implicit(I18n.GetByKey(overrides.DescriptionKey)) : null, CharacterSubject.GetTypeName((Character)(object)npc, type));
		Bat bat = (Bat)(object)((npc is Bat) ? npc : null);
		if (bat != null)
		{
			this.IsHauntedSkull = ((NetFieldBase<bool, NetBool>)(object)bat.hauntedSkull).Value;
			this.IsMagmaSprite = ((NetFieldBase<bool, NetBool>)(object)bat.magmaSprite).Value;
		}
		else
		{
			this.IsGourmand = type == SubjectType.Villager && ((Character)npc).Name == "Gourmand" && ((Character)npc).currentLocation.Name == "IslandFarmCave";
		}
		this.DisablePortraits = CharacterSubject.ShouldDisablePortraits(npc, this.IsGourmand);
	}

	public override IEnumerable<ICustomField> GetData()
	{
		NPC npc = this.Target;
		IModInfo fromMod = base.GameHelper.TryGetModFromStringId(((Character)npc).Name);
		if (fromMod != null)
		{
			yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(fromMod.Manifest.Name));
		}
		IEnumerable<ICustomField> enumerable;
		switch (this.TargetType)
		{
		case SubjectType.Monster:
			enumerable = this.GetDataForMonster((Monster)npc);
			break;
		case SubjectType.Pet:
			enumerable = this.GetDataForPet((Pet)npc);
			break;
		case SubjectType.Villager:
		{
			NPC val = npc;
			Child child = (Child)(object)((val is Child) ? val : null);
			IEnumerable<ICustomField> enumerable2;
			if (child == null)
			{
				TrashBear trashBear = (TrashBear)(object)((val is TrashBear) ? val : null);
				enumerable2 = ((trashBear == null) ? ((!this.IsGourmand) ? this.GetDataForVillager(npc) : this.GetDataForGourmand()) : this.GetDataForTrashBear(trashBear));
			}
			else
			{
				enumerable2 = this.GetDataForChild(child);
			}
			enumerable = enumerable2;
			break;
		}
		default:
			enumerable = Array.Empty<ICustomField>();
			break;
		}
		IEnumerable<ICustomField> fields = enumerable;
		foreach (ICustomField item in fields)
		{
			yield return item;
		}
		switch (this.TargetType)
		{
		case SubjectType.Horse:
			yield return new GenericField(I18n.InternalId(), ((Horse)npc).HorseId.ToString());
			break;
		case SubjectType.Pet:
			yield return new GenericField(I18n.InternalId(), ((NetFieldBase<string, NetString>)(object)((Pet)npc).petType).Value);
			break;
		default:
			yield return new GenericField(I18n.InternalId(), ((Character)npc).Name);
			break;
		}
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		NPC target = this.Target;
		Pet pet = (Pet)(object)((target is Pet) ? target : null);
		yield return new GenericDebugField("facing direction", base.Stringify((FacingDirection)((Character)target).FacingDirection), null, pinned: true);
		yield return new GenericDebugField("walking towards player", base.Stringify(target.IsWalkingTowardPlayer), null, pinned: true);
		if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).ContainsKey(((Character)target).Name))
		{
			FriendshipModel friendship = base.GameHelper.GetFriendshipForVillager(Game1.player, target, ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData)[((Character)target).Name]);
			yield return new GenericDebugField("friendship", $"{friendship.Points} (max {friendship.MaxPoints})", null, pinned: true);
		}
		if (pet != null)
		{
			yield return new GenericDebugField("friendship", $"{pet.friendshipTowardFarmer} of {1000})", null, pinned: true);
		}
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		NPC npc = this.Target;
		if (this.IsHauntedSkull || this.IsMagmaSprite)
		{
			Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(((Character)npc).Sprite.Texture, 4, 16, 16);
			spriteBatch.Draw(((Character)npc).Sprite.Texture, position, (Rectangle?)sourceRect, Color.White, 0f, Vector2.Zero, new Vector2(size.X / 16f), (SpriteEffects)0, 1f);
			return true;
		}
		if (((Character)npc).IsVillager && !this.DisablePortraits)
		{
			DrawHelper.DrawSprite(spriteBatch, npc.Portrait, new Rectangle(0, 0, 64, 64), position.X, position.Y, new Point(64, 64), (Color?)Color.White, size.X / 64f);
			return true;
		}
		((Character)npc).Sprite.draw(spriteBatch, position, 1f, 0, 0, Color.White, false, size.X / (float)((Character)npc).Sprite.getWidth(), 0f, false);
		return true;
	}

	private IEnumerable<ICustomField> GetDataForChild(Child child)
	{
		yield return new GenericField(I18n.Npc_Birthday(), this.GetChildBirthdayString(child));
		ChildAge stage = (ChildAge)((NPC)child).Age;
		int daysOld = ((NetFieldBase<int, NetInt>)(object)child.daysOld).Value;
		int daysToNext = this.GetDaysToNextChildGrowth(stage, daysOld);
		bool isGrown = daysToNext == -1;
		int daysAtNext = daysOld + ((!isGrown) ? daysToNext : 0);
		string ageDesc = (isGrown ? I18n.Npc_Child_Age_DescriptionGrown(I18n.For(stage)) : I18n.Npc_Child_Age_DescriptionPartial(I18n.For(stage), daysToNext, I18n.For(stage + 1)));
		yield return new PercentageBarField(I18n.Npc_Child_Age(), ((NetFieldBase<int, NetInt>)(object)child.daysOld).Value, daysAtNext, Color.Green, Color.Gray, ageDesc);
		if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).ContainsKey(((Character)child).Name))
		{
			FriendshipModel friendship = base.GameHelper.GetFriendshipForVillager(Game1.player, (NPC)(object)child, ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData)[((Character)child).Name]);
			yield return new CharacterFriendshipField(I18n.Npc_Friendship(), friendship);
			yield return new GenericField(I18n.Npc_TalkedToday(), base.Stringify(((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData)[((Character)child).Name].TalkedToToday));
		}
	}

	private IEnumerable<ICustomField> GetDataForGourmand()
	{
		IslandFarmCave cave = (IslandFarmCave)Game1.getLocationFromName("IslandFarmCave");
		if (cave == null)
		{
			yield break;
		}
		int questsDone = ((NetFieldBase<int, NetInt>)(object)cave.gourmandRequestsFulfilled).Value;
		int maxQuests = IslandFarmCave.TOTAL_GOURMAND_REQUESTS;
		if (questsDone <= maxQuests)
		{
			List<Checkbox> checkboxes = new List<Checkbox>();
			for (int i = 0; i < maxQuests; i++)
			{
				string wantedKey = cave.IndexForRequest(i);
				if (CommonHelper.IsItemId(wantedKey))
				{
					string displayName = ItemRegistry.GetDataOrErrorItem(wantedKey).DisplayName;
					checkboxes.Add(new Checkbox(questsDone > i, displayName));
				}
			}
			if (checkboxes.Any())
			{
				yield return new CheckboxListField(I18n.TrashBearOrGourmand_ItemWanted(), new CheckboxList(checkboxes));
			}
		}
		yield return new GenericField(I18n.TrashBearOrGourmand_QuestProgress(), I18n.Generic_Ratio(questsDone, maxQuests));
	}

	private IEnumerable<ICustomField> GetDataForMonster(Monster monster)
	{
		bool canRerollDrops = Game1.player.isWearingRing("526");
		yield return new GenericField(I18n.Monster_Invincible(), I18n.Generic_Seconds(monster.invincibleCountdown), monster.isInvincible());
		yield return new PercentageBarField(I18n.Monster_Health(), monster.Health, monster.MaxHealth, Color.Green, Color.Gray, I18n.Generic_PercentRatio((int)Math.Round((float)monster.Health / ((float)monster.MaxHealth * 1f) * 100f), monster.Health, monster.MaxHealth));
		yield return new ItemDropListField(base.GameHelper, this.Codex, I18n.Monster_Drops(), this.GetMonsterDrops(monster), sort: true, fadeNonGuaranteed: true, !canRerollDrops, I18n.Monster_Drops_Nothing());
		yield return new GenericField(I18n.Monster_Experience(), base.Stringify(monster.ExperienceGained));
		yield return new GenericField(I18n.Monster_Defense(), base.Stringify(((NetFieldBase<int, NetInt>)(object)monster.resilience).Value));
		yield return new GenericField(I18n.Monster_Attack(), base.Stringify(monster.DamageToFarmer));
		foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
		{
			if (questData.Targets?.Contains(((Character)monster).Name) ?? false)
			{
				int kills = ((IEnumerable<string>)questData.Targets).Sum((Func<string, int>)Game1.stats.getMonstersKilled);
				string goalName = TokenParser.ParseText(questData.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				string text = I18n.Monster_AdventureGuild_EradicationGoal(goalName, kills, questData.Count);
				Checkbox checkbox = new Checkbox(kills >= questData.Count, text);
				yield return new CheckboxListField(I18n.Monster_AdventureGuild(), new CheckboxList(new Checkbox[1] { checkbox }));
			}
		}
	}

	private IEnumerable<ICustomField> GetDataForPet(Pet pet)
	{
		yield return new CharacterFriendshipField(I18n.Pet_Love(), base.GameHelper.GetFriendshipForPet(Game1.player, pet));
		int? lastDayPetted = this.GetLastDayPetted(pet, Game1.player.UniqueMultiplayerID);
		yield return new GenericField(I18n.Pet_PettedToday(), (lastDayPetted == Game1.Date.TotalDays) ? I18n.Pet_LastPetted_Yes() : base.Stringify(false));
		if (!lastDayPetted.HasValue)
		{
			yield return new GenericField(I18n.Pet_LastPetted(), I18n.Pet_LastPetted_Never());
		}
		else if (lastDayPetted != Game1.Date.TotalDays)
		{
			int daysSincePetted = Game1.Date.TotalDays - lastDayPetted.Value;
			yield return new GenericField(I18n.Pet_LastPetted(), (daysSincePetted == 1) ? I18n.Generic_Yesterday() : I18n.Pet_LastPetted_DaysAgo(daysSincePetted));
		}
		PetBowl bowl = pet.GetPetBowl();
		if (bowl != null)
		{
			yield return new GenericField(I18n.Pet_WaterBowl(), ((NetFieldBase<bool, NetBool>)(object)bowl.watered).Value ? I18n.Pet_WaterBowl_Filled() : I18n.Pet_WaterBowl_Empty());
		}
	}

	private IEnumerable<ICustomField> GetDataForTrashBear(TrashBear trashBear)
	{
		int questsDone = 0;
		if (NetWorldState.checkAnywhereForWorldStateID("trashBear1"))
		{
			questsDone = 1;
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBear2"))
		{
			questsDone = 2;
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBear3"))
		{
			questsDone = 3;
		}
		if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
		{
			questsDone = 4;
		}
		if (questsDone < 4)
		{
			trashBear.updateItemWanted();
			yield return new ItemIconField(base.GameHelper, I18n.TrashBearOrGourmand_ItemWanted(), ItemRegistry.Create(trashBear.itemWantedIndex, 1, 0, false), this.Codex);
		}
		yield return new GenericField(I18n.TrashBearOrGourmand_QuestProgress(), I18n.Generic_Ratio(questsDone, 4));
	}

	private IEnumerable<ICustomField> GetDataForVillager(NPC npc)
	{
		if (this.EnableTargetRedirection && npc != null && ((Character)npc).Name == "AbigailMine")
		{
			GameLocation currentLocation = ((Character)npc).currentLocation;
			if (currentLocation != null && currentLocation.Name == "UndergroundMine20")
			{
				npc = Game1.getCharacterFromName("Abigail", true, false) ?? npc;
			}
		}
		if (!base.GameHelper.IsSocialVillager(npc))
		{
			yield break;
		}
		if (base.GameHelper.TryGetDate(npc.Birthday_Day, npc.Birthday_Season, out SDate birthday))
		{
			yield return new GenericField(I18n.Npc_Birthday(), I18n.Stringify(birthday));
		}
		if (((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).ContainsKey(((Character)npc).Name))
		{
			FriendshipModel friendship = base.GameHelper.GetFriendshipForVillager(Game1.player, npc, ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData)[((Character)npc).Name]);
			yield return new GenericField(I18n.Npc_CanRomance(), friendship.IsSpouse ? I18n.Npc_CanRomance_Married() : (friendship.IsHousemate ? I18n.Npc_CanRomance_Housemate() : base.Stringify(friendship.CanDate)));
			yield return new CharacterFriendshipField(I18n.Npc_Friendship(), friendship);
			yield return new GenericField(I18n.Npc_TalkedToday(), base.Stringify(friendship.TalkedToday));
			yield return new GenericField(I18n.Npc_GiftedToday(), base.Stringify(friendship.GiftsToday > 0));
			if (friendship.IsSpouse || friendship.IsHousemate)
			{
				yield return new GenericField(friendship.IsSpouse ? I18n.Npc_KissedToday() : I18n.Npc_HuggedToday(), base.Stringify(((NetFieldBase<bool, NetBool>)(object)npc.hasBeenKissedToday).Value));
			}
			if ((object)friendship != null && !friendship.IsSpouse && !friendship.IsHousemate)
			{
				yield return new GenericField(I18n.Npc_GiftedThisWeek(), I18n.Generic_Ratio(friendship.GiftsThisWeek, 2));
			}
		}
		else
		{
			yield return new GenericField(I18n.Npc_Friendship(), I18n.Npc_Friendship_NotMet());
		}
		IDictionary<GiftTaste, GiftTasteModel[]> giftTastes = this.GetGiftTastes(npc);
		IDictionary<string, bool> ownedItems = CharacterGiftTastesField.GetOwnedItemsCache(base.GameHelper);
		if (this.ShowGiftTastes.Loved)
		{
			yield return this.GetGiftTasteField(I18n.Npc_LovesGifts(), giftTastes, ownedItems, GiftTaste.Love);
		}
		if (this.ShowGiftTastes.Liked)
		{
			yield return this.GetGiftTasteField(I18n.Npc_LikesGifts(), giftTastes, ownedItems, GiftTaste.Like);
		}
		if (this.ShowGiftTastes.Neutral)
		{
			yield return this.GetGiftTasteField(I18n.Npc_NeutralGifts(), giftTastes, ownedItems, GiftTaste.Neutral);
		}
		if (this.ShowGiftTastes.Disliked)
		{
			yield return this.GetGiftTasteField(I18n.Npc_DislikesGifts(), giftTastes, ownedItems, GiftTaste.Dislike);
		}
		if (this.ShowGiftTastes.Hated)
		{
			yield return this.GetGiftTasteField(I18n.Npc_HatesGifts(), giftTastes, ownedItems, GiftTaste.Hate);
		}
		yield return new ScheduleField(npc, base.GameHelper);
	}

	private ICustomField GetGiftTasteField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, IDictionary<string, bool> ownedItemsCache, GiftTaste taste)
	{
		CharacterGiftTastesField field = new CharacterGiftTastesField(label, giftTastes, taste, this.ShowUnknownGiftTastes, this.HighlightUnrevealedGiftTastes, !this.ShowUnownedGifts, ownedItemsCache);
		if (this.CollapseFieldsConfig.Enabled)
		{
			field.CollapseIfLengthExceeds(this.CollapseFieldsConfig.NpcGiftTastes, field.TotalItems);
		}
		return field;
	}

	private static string GetTypeName(Character npc, SubjectType type)
	{
		switch (type)
		{
		case SubjectType.Villager:
			return I18n.Type_Villager();
		case SubjectType.Horse:
			return GameI18n.GetString("Strings\\StringsFromCSFiles:StrengthGame.cs.11665");
		case SubjectType.Monster:
			return I18n.Type_Monster();
		case SubjectType.Pet:
		{
			Pet pet = (Pet)(object)((npc is Pet) ? npc : null);
			PetData petData = default(PetData);
			if (pet != null && Pet.TryGetData(((NetFieldBase<string, NetString>)(object)pet.petType).Value, ref petData))
			{
				string typeName = TokenParser.ParseText(petData.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null) ?? ((NetFieldBase<string, NetString>)(object)pet.petType).Value;
				if (typeName.Length > 1)
				{
					typeName = char.ToUpperInvariant(typeName[0]) + typeName.Substring(1);
				}
				return typeName;
			}
			break;
		}
		}
		return ((object)npc).GetType().Name;
	}

	private static bool ShouldDisablePortraits(NPC npc, bool isGourmand)
	{
		bool flag = Game1.CurrentEvent?.id == "festival_fall27";
		bool flag2 = flag;
		if (flag2)
		{
			bool flag3;
			switch (((Character)npc).Name)
			{
			case "Mummy":
			case "Stone Golem":
			case "Wilderness Golem":
				flag3 = true;
				break;
			default:
				flag3 = false;
				break;
			}
			flag2 = flag3;
		}
		if (flag2)
		{
			return true;
		}
		if (isGourmand)
		{
			return true;
		}
		try
		{
			return npc.Portrait == null;
		}
		catch
		{
			return true;
		}
	}

	private IDictionary<GiftTaste, GiftTasteModel[]> GetGiftTastes(NPC npc)
	{
		return (from entry in base.GameHelper.GetGiftTastes(npc)
			group entry by entry.Taste).ToDictionary((IGrouping<GiftTaste, GiftTasteModel> tasteGroup) => tasteGroup.Key, (IGrouping<GiftTaste, GiftTasteModel> tasteGroup) => tasteGroup.ToArray());
	}

	private string GetChildBirthdayString(Child child)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		int daysOld = ((NetFieldBase<int, NetInt>)(object)child.daysOld).Value;
		try
		{
			return SDate.Now().AddDays(-daysOld).ToLocaleString(true);
		}
		catch (ArithmeticException)
		{
			return new SDate(Game1.dayOfMonth, Game1.season, 100000000).AddDays(-daysOld).ToLocaleString(false);
		}
	}

	private int GetDaysToNextChildGrowth(ChildAge stage, int daysOld)
	{
		return stage switch
		{
			ChildAge.Newborn => 13 - daysOld, 
			ChildAge.Baby => 27 - daysOld, 
			ChildAge.Crawler => 55 - daysOld, 
			_ => -1, 
		};
	}

	private int? GetLastDayPetted(Pet pet, long playerID)
	{
		int lastDay = default(int);
		if (!((NetDictionary<long, int, NetInt, SerializableDictionary<long, int>, NetLongDictionary<int, NetInt>>)(object)pet.lastPetDay).TryGetValue(playerID, ref lastDay))
		{
			return null;
		}
		return lastDay;
	}

	private IEnumerable<ItemDropData> GetMonsterDrops(Monster monster)
	{
		ItemDropData[] possibleDrops = base.GameHelper.GetMonsterData().FirstOrDefault((MonsterData p) => p.Name == ((Character)monster).Name)?.Drops;
		if (this.IsHauntedSkull && possibleDrops == null)
		{
			possibleDrops = base.GameHelper.GetMonsterData().FirstOrDefault((MonsterData p) => p.Name == "Lava Bat")?.Drops;
		}
		if (possibleDrops == null)
		{
			possibleDrops = Array.Empty<ItemDropData>();
		}
		IDictionary<string, List<ItemDropData>> dropsLeft = (from p in ((IEnumerable<string>)monster.objectsToDrop).Select(GetActualDrop)
			group p by p.ItemId).ToDictionary((IGrouping<string, ItemDropData> group) => group.Key, (IGrouping<string, ItemDropData> group) => group.ToList());
		foreach (ItemDropData drop in possibleDrops.OrderByDescending((ItemDropData p) => p.Probability))
		{
			List<ItemDropData> actualDrops;
			bool isGuaranteed = dropsLeft.TryGetValue(drop.ItemId, out actualDrops) && actualDrops.Any();
			if (isGuaranteed)
			{
				ItemDropData[] matches = actualDrops.Where((ItemDropData p) => p.MinDrop >= drop.MinDrop && p.MaxDrop <= drop.MaxDrop).ToArray();
				ItemDropData bestMatch = matches.FirstOrDefault((ItemDropData p) => p.MinDrop == drop.MinDrop && p.MaxDrop == drop.MaxDrop) ?? matches.FirstOrDefault();
				if ((object)bestMatch != null)
				{
					actualDrops.Remove(bestMatch);
				}
			}
			yield return new ItemDropData(drop.ItemId, 1, drop.MaxDrop, isGuaranteed ? 1f : drop.Probability);
		}
		foreach (KeyValuePair<string, List<ItemDropData>> item in dropsLeft.Where<KeyValuePair<string, List<ItemDropData>>>((KeyValuePair<string, List<ItemDropData>> p) => p.Value.Any()))
		{
			foreach (ItemDropData item2 in item.Value)
			{
				yield return item2;
			}
		}
	}

	private ItemDropData GetActualDrop(string id)
	{
		int minDrop = 1;
		int maxDrop = 1;
		if (int.TryParse(id, out var numericId) && numericId < 0)
		{
			id = (-numericId).ToString();
			maxDrop = 3;
		}
		string text;
		if (id != null)
		{
			int length = id.Length;
			if (length == 1)
			{
				switch (id[0])
				{
				case '0':
					break;
				case '2':
					goto IL_00de;
				case '4':
					goto IL_00ef;
				case '6':
					goto IL_0100;
				default:
					goto IL_0144;
				}
				text = 378.ToString();
				goto IL_0147;
			}
			if (length == 2)
			{
				switch (id[1])
				{
				case '0':
					break;
				case '2':
					goto IL_00ac;
				case '4':
					goto IL_00be;
				default:
					goto IL_0144;
				}
				if (id == "10")
				{
					text = 386.ToString();
					goto IL_0147;
				}
			}
		}
		goto IL_0144;
		IL_0100:
		text = 384.ToString();
		goto IL_0147;
		IL_0144:
		text = id;
		goto IL_0147;
		IL_00ef:
		text = 382.ToString();
		goto IL_0147;
		IL_00ac:
		if (!(id == "12"))
		{
			goto IL_0144;
		}
		text = 388.ToString();
		goto IL_0147;
		IL_00be:
		if (!(id == "14"))
		{
			goto IL_0144;
		}
		text = 390.ToString();
		goto IL_0147;
		IL_0147:
		id = text;
		return new ItemDropData(id, minDrop, maxDrop, 1f);
		IL_00de:
		text = 380.ToString();
		goto IL_0147;
	}
}
