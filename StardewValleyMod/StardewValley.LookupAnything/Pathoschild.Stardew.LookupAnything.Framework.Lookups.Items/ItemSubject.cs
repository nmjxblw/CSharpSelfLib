using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Movies;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;

internal class ItemSubject : BaseSubject
{
	public record RecipeData(RecipeType Type, string DisplayName, int TimesCrafted, bool IsKnown);

	private readonly Item Target;

	private readonly Crop? FromCrop;

	private readonly HoeDirt? FromDirt;

	private readonly Crop? SeedForCrop;

	private readonly ObjectContext Context;

	private readonly bool KnownQuality;

	private readonly GameLocation? Location;

	public bool ShowUncaughtFishSpawnRules;

	private readonly bool ShowUnknownGiftTastes;

	private readonly bool HighlightUnrevealedGiftTastes;

	private readonly ModGiftTasteConfig ShowGiftTastes;

	private readonly bool ShowUnknownRecipes;

	private readonly bool ShowInvalidRecipes;

	private readonly ModCollapseLargeFieldsConfig CollapseFieldsConfig;

	private readonly ISubjectRegistry Codex;

	private readonly Func<Crop, ObjectContext, HoeDirt?, ISubject> GetCropSubject;

	public ItemSubject(ISubjectRegistry codex, GameHelper gameHelper, bool showUncaughtFishSpawnRules, bool showUnknownGiftTastes, bool highlightUnrevealedGiftTastes, ModGiftTasteConfig showGiftTastes, bool showUnknownRecipes, bool showInvalidRecipes, ModCollapseLargeFieldsConfig collapseFieldsConfig, Item item, ObjectContext context, bool knownQuality, GameLocation? location, Func<Crop, ObjectContext, HoeDirt?, ISubject> getCropSubject, Crop? fromCrop = null, HoeDirt? fromDirt = null)
		: base(gameHelper)
	{
		this.Codex = codex;
		this.ShowUncaughtFishSpawnRules = showUncaughtFishSpawnRules;
		this.ShowUnknownGiftTastes = showUnknownGiftTastes;
		this.HighlightUnrevealedGiftTastes = highlightUnrevealedGiftTastes;
		this.ShowGiftTastes = showGiftTastes;
		this.ShowUnknownRecipes = showUnknownRecipes;
		this.ShowInvalidRecipes = showInvalidRecipes;
		this.CollapseFieldsConfig = collapseFieldsConfig;
		this.Target = item;
		this.FromCrop = fromCrop ?? ((fromDirt != null) ? fromDirt.crop : null);
		this.FromDirt = fromDirt;
		this.Context = context;
		this.Location = location;
		this.KnownQuality = knownQuality;
		this.GetCropSubject = getCropSubject;
		this.SeedForCrop = ((item.QualifiedItemId != "(O)433" || this.FromCrop == null) ? this.TryGetCropForSeed(item, location) : null);
		base.Initialize(this.Target.DisplayName, this.GetDescription(this.Target), this.GetTypeValue(this.Target));
	}

	public override IEnumerable<ICustomField> GetData()
	{
		Item item = this.Target;
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
		Item obj = item;
		Object obj2 = (Object)(object)((obj is Object) ? obj : null);
		bool isCrop = this.FromCrop != null;
		bool isSeed = this.SeedForCrop != null;
		bool isDeadCrop = ((NetFieldBase<bool, NetBool>)(object)this.FromCrop?.dead).Value ?? false;
		bool canSell = item.canBeShipped() || base.Metadata.Shops.Any((ShopData shop) => shop.BuysCategories.Contains(item.Category));
		bool isMovieTicket = item.QualifiedItemId == "(O)809";
		bool showInventoryFields = obj2 == null || !obj2.IsBreakableStone();
		ItemData objData = base.Metadata.GetObject(item, this.Context);
		if (objData != null)
		{
			base.Name = ((objData.NameKey != null) ? Translation.op_Implicit(I18n.GetByKey(objData.NameKey)) : base.Name);
			base.Description = ((objData.DescriptionKey != null) ? Translation.op_Implicit(I18n.GetByKey(objData.DescriptionKey)) : base.Description);
			base.Type = ((objData.TypeKey != null) ? Translation.op_Implicit(I18n.GetByKey(objData.TypeKey)) : base.Type);
			showInventoryFields = objData.ShowInventoryFields ?? showInventoryFields;
		}
		IModInfo fromMod = base.GameHelper.TryGetModFromStringId(item.ItemId);
		if (fromMod != null)
		{
			yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(fromMod.Manifest.Name));
		}
		string flavorId = ((obj2 != null) ? obj2.GetPreservedItemId() : null);
		if (flavorId != null)
		{
			Item preservedItem = ItemRegistry.Create(flavorId, 1, 0, false);
			yield return new LinkField(I18n.Item_Flavor(), preservedItem.DisplayName, () => this.Codex.GetByEntity(preservedItem, null));
		}
		if (isDeadCrop)
		{
			yield return new GenericField(I18n.Crop_Summary(), I18n.Crop_Summary_Dead());
			yield break;
		}
		foreach (ICustomField cropField in this.GetCropFields(this.FromDirt, this.FromCrop ?? this.SeedForCrop, obj2, isSeed))
		{
			yield return cropField;
		}
		IndoorPot pot = default(IndoorPot);
		ref IndoorPot reference = ref pot;
		Item obj3 = item;
		reference = (IndoorPot)(object)((obj3 is IndoorPot) ? obj3 : null);
		if (pot != null)
		{
			Crop potCrop = ((NetFieldBase<HoeDirt, NetRef<HoeDirt>>)(object)pot.hoeDirt).Value.crop;
			Bush potBush = ((NetFieldBase<Bush, NetRef<Bush>>)(object)pot.bush).Value;
			if (potCrop != null)
			{
				string dropName = ItemRegistry.GetDataOrErrorItem(((NetFieldBase<string, NetString>)(object)potCrop.indexOfHarvest).Value).DisplayName;
				yield return new LinkField(I18n.Item_Contents(), dropName, () => this.GetCropSubject(potCrop, ObjectContext.World, ((NetFieldBase<HoeDirt, NetRef<HoeDirt>>)(object)pot.hoeDirt).Value));
			}
			if (potBush != null)
			{
				ISubject subject = this.Codex.GetByEntity(potBush, this.Location ?? ((TerrainFeature)potBush).Location);
				if (subject != null)
				{
					yield return new LinkField(I18n.Item_Contents(), subject.Name, () => subject);
				}
			}
		}
		foreach (ICustomField machineOutputField in this.GetMachineOutputFields(obj2))
		{
			yield return machineOutputField;
		}
		if (((obj2 != null) ? ((Item)obj2).Name : null) == "Flute Block")
		{
			yield return new GenericField(I18n.Item_MusicBlock_Pitch(), I18n.Generic_Ratio(((NetFieldBase<string, NetString>)(object)obj2.preservedParentSheetIndex).Value, 2300));
		}
		else if (((obj2 != null) ? ((Item)obj2).Name : null) == "Drum Block")
		{
			yield return new GenericField(I18n.Item_MusicBlock_DrumType(), I18n.Generic_Ratio(((NetFieldBase<string, NetString>)(object)obj2.preservedParentSheetIndex).Value, 6));
		}
		if (showInventoryFields)
		{
			foreach (ICustomField neededForField in this.GetNeededForFields(obj2))
			{
				yield return neededForField;
			}
			if (canSell && !isCrop)
			{
				string saleValueSummary = GenericField.GetSaleValueString(this.GetSaleValue(item, this.KnownQuality), item.Stack);
				yield return new GenericField(I18n.Item_SellsFor(), saleValueSummary);
				List<string> buyers = new List<string>();
				if (item.canBeShipped())
				{
					buyers.Add(I18n.Item_SellsTo_ShippingBox());
				}
				buyers.AddRange(from shop in base.Metadata.Shops
					where shop.BuysCategories.Contains(item.Category)
					let name = ((object)I18n.GetByKey(shop.DisplayKey)).ToString()
					orderby name
					select name);
				yield return new GenericField(I18n.Item_SellsTo(), I18n.List(buyers));
			}
			Item obj4 = item;
			Clothing clothing = (Clothing)(object)((obj4 is Clothing) ? obj4 : null);
			if (clothing != null)
			{
				yield return new GenericField(I18n.Item_CanBeDyed(), base.Stringify(((NetFieldBase<bool, NetBool>)(object)clothing.dyeable).Value));
			}
			if (!isMovieTicket)
			{
				IDictionary<GiftTaste, GiftTasteModel[]> giftTastes = this.GetGiftTastes(item);
				if (this.ShowGiftTastes.Loved)
				{
					yield return new ItemGiftTastesField(I18n.Item_LovesThis(), giftTastes, GiftTaste.Love, this.ShowUnknownGiftTastes, this.HighlightUnrevealedGiftTastes);
				}
				if (this.ShowGiftTastes.Liked)
				{
					yield return new ItemGiftTastesField(I18n.Item_LikesThis(), giftTastes, GiftTaste.Like, this.ShowUnknownGiftTastes, this.HighlightUnrevealedGiftTastes);
				}
				if (this.ShowGiftTastes.Neutral)
				{
					yield return new ItemGiftTastesField(I18n.Item_NeutralAboutThis(), giftTastes, GiftTaste.Neutral, this.ShowUnknownGiftTastes, this.HighlightUnrevealedGiftTastes);
				}
				if (this.ShowGiftTastes.Disliked)
				{
					yield return new ItemGiftTastesField(I18n.Item_DislikesThis(), giftTastes, GiftTaste.Dislike, this.ShowUnknownGiftTastes, this.HighlightUnrevealedGiftTastes);
				}
				if (this.ShowGiftTastes.Hated)
				{
					yield return new ItemGiftTastesField(I18n.Item_HatesThis(), giftTastes, GiftTaste.Hate, this.ShowUnknownGiftTastes, this.HighlightUnrevealedGiftTastes);
				}
			}
		}
		Item obj5 = item;
		MeleeWeapon weapon = (MeleeWeapon)(object)((obj5 is MeleeWeapon) ? obj5 : null);
		if (weapon != null && !((Tool)weapon).isScythe())
		{
			int accuracy = ((NetFieldBase<int, NetInt>)(object)weapon.addedPrecision).Value;
			float critChance = ((NetFieldBase<float, NetFloat>)(object)weapon.critChance).Value;
			float critMultiplier = ((NetFieldBase<float, NetFloat>)(object)weapon.critMultiplier).Value;
			int damageMin = ((NetFieldBase<int, NetInt>)(object)weapon.minDamage).Value;
			int damageMax = ((NetFieldBase<int, NetInt>)(object)weapon.maxDamage).Value;
			int defense = ((NetFieldBase<int, NetInt>)(object)weapon.addedDefense).Value;
			float knockback = ((NetFieldBase<float, NetFloat>)(object)weapon.knockback).Value;
			int speed = ((NetFieldBase<int, NetInt>)(object)weapon.speed).Value;
			int reach = ((NetFieldBase<int, NetInt>)(object)weapon.addedAreaOfEffect).Value;
			int shownKnockback = (int)Math.Ceiling(Math.Abs(knockback - weapon.defaultKnockBackForThisType(((NetFieldBase<int, NetInt>)(object)weapon.type).Value)) * 10f);
			int shownSpeed = (speed - ((((NetFieldBase<int, NetInt>)(object)weapon.type).Value == 2) ? (-8) : 0)) / 2;
			yield return new GenericField(I18n.Item_MeleeWeapon_Damage(), (damageMin != damageMax) ? I18n.Generic_Range(damageMin, damageMax) : damageMin.ToString());
			yield return new GenericField(I18n.Item_MeleeWeapon_CriticalChance(), I18n.Generic_Percent(critChance * 100f));
			yield return new GenericField(I18n.Item_MeleeWeapon_CriticalDamage(), I18n.Item_MeleeWeapon_CriticalDamage_Label(critMultiplier));
			yield return new GenericField(I18n.Item_MeleeWeapon_Defense(), (defense == 0) ? "0" : I18n.Item_MeleeWeapon_Defense_Label(AddSign(((NetFieldBase<int, NetInt>)(object)weapon.addedDefense).Value)));
			if (speed == 0)
			{
				yield return new GenericField(I18n.Item_MeleeWeapon_Speed(), "0");
			}
			else
			{
				string speedLabel = I18n.Item_MeleeWeapon_Speed_Summary(AddSign(speed), AddSign(-speed * 40));
				if (speed != shownSpeed)
				{
					string shownSpeed2 = AddSign(shownSpeed);
					object actualSpeed = speedLabel;
					speedLabel = I18n.Item_MeleeWeapon_Speed_ShownVsActual(shownSpeed2, Environment.NewLine, actualSpeed);
				}
				yield return new GenericField(I18n.Item_MeleeWeapon_Speed(), speedLabel);
			}
			yield return new GenericField(I18n.Item_MeleeWeapon_Knockback(), (knockback > 1f) ? I18n.Item_MeleeWeapon_Knockback_Label(AddSign(shownKnockback), knockback) : "0");
			yield return new GenericField(I18n.Item_MeleeWeapon_Reach(), (reach > 0) ? I18n.Item_MeleeWeapon_Reach_Label(AddSign(reach)) : "0");
			yield return new GenericField(I18n.Item_MeleeWeapon_Accuracy(), AddSign(accuracy));
		}
		if (showInventoryFields)
		{
			IEnumerable<RecipeModel> first = base.GameHelper.GetRecipesForIngredient(item).Concat(base.GameHelper.GetRecipesForOutput(item));
			GameHelper gameHelper = base.GameHelper;
			Item obj6 = item;
			RecipeModel[] recipes = first.Concat(gameHelper.GetRecipesForMachine((Object?)(object)((obj6 is Object) ? obj6 : null))).ToArray();
			if (recipes.Length != 0)
			{
				ItemRecipesField field = new ItemRecipesField(base.GameHelper, this.Codex, I18n.Item_Recipes(), item, recipes, this.ShowUnknownRecipes, this.ShowInvalidRecipes);
				if (this.CollapseFieldsConfig.Enabled)
				{
					field.CollapseIfLengthExceeds(this.CollapseFieldsConfig.ItemRecipes, recipes.Length);
				}
				yield return field;
			}
		}
		yield return new FishSpawnRulesField(base.GameHelper, I18n.Item_FishSpawnRules(), itemData, this.ShowUncaughtFishSpawnRules);
		bool flag = ItemExtensions.HasTypeObject((IHaveItemTypeId)(object)item);
		bool flag2 = flag;
		if (flag2)
		{
			bool flag3 = item.Category == -4;
			bool flag4 = flag3;
			if (!flag4)
			{
				string qualifiedItemId = item.QualifiedItemId;
				bool flag5 = ((qualifiedItemId == "(O)393" || qualifiedItemId == "(O)397") ? true : false);
				flag4 = flag5;
			}
			flag2 = flag4;
		}
		if (flag2)
		{
			FishPondData fishPondData = FishPond.GetRawData(item.ItemId);
			if (fishPondData != null)
			{
				int minChanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, 0.1f) * 100f);
				int maxChanceOfAnyDrop = (int)Math.Round(Utility.Lerp(0.15f, 0.95f, 1f) * 100f);
				string preface = I18n.Building_FishPond_Drops_Preface(I18n.Generic_Range(minChanceOfAnyDrop, maxChanceOfAnyDrop));
				yield return new FishPondDropsField(base.GameHelper, this.Codex, I18n.Item_FishPondDrops(), -1, fishPondData, obj2, preface);
			}
		}
		Item obj7 = item;
		Fence fence = (Fence)(object)((obj7 is Fence) ? obj7 : null);
		if (fence != null)
		{
			string healthLabel = I18n.Item_FenceHealth();
			if (((GameLocation)Game1.getFarm()).isBuildingConstructed(Constant.BuildingNames.GoldClock))
			{
				yield return new GenericField(healthLabel, I18n.Item_FenceHealth_GoldClock());
			}
			else
			{
				float maxHealth = (((NetFieldBase<bool, NetBool>)(object)fence.isGate).Value ? (((NetFieldBase<float, NetFloat>)(object)fence.maxHealth).Value * 2f) : ((NetFieldBase<float, NetFloat>)(object)fence.maxHealth).Value);
				float health = ((NetFieldBase<float, NetFloat>)(object)fence.health).Value / maxHealth;
				double daysLeft = Math.Round(((NetFieldBase<float, NetFloat>)(object)fence.health).Value * base.Constants.FenceDecayRate / 60f / 24f);
				double percent = Math.Round(health * 100f);
				yield return new PercentageBarField(healthLabel, (int)((NetFieldBase<float, NetFloat>)(object)fence.health).Value, (int)maxHealth, Color.Green, Color.Red, I18n.Item_FenceHealth_Summary((int)percent, (int)daysLeft));
			}
		}
		if (isMovieTicket)
		{
			MovieData movie = MovieTheater.GetMovieForDate(Game1.Date);
			if (movie == null)
			{
				yield return new GenericField(I18n.Item_MovieTicket_MovieThisWeek(), I18n.Item_MovieTicket_MovieThisWeek_None());
			}
			else
			{
				yield return new GenericField(I18n.Item_MovieTicket_MovieThisWeek(), new _003C_003Ez__ReadOnlyArray<IFormattedText>(new IFormattedText[3]
				{
					new FormattedText(TokenParser.ParseText(movie.Title, (Random)null, (TokenParserDelegate)null, (Farmer)null), null, bold: true),
					new FormattedText(Environment.NewLine),
					new FormattedText(TokenParser.ParseText(movie.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null))
				}));
				IDictionary<GiftTaste, string[]> tastes = (from entry in base.GameHelper.GetMovieTastes()
					group entry by entry.Value ?? ((GiftTaste)(-1))).ToDictionary((IGrouping<GiftTaste, KeyValuePair<NPC, GiftTaste?>> group) => group.Key, (IGrouping<GiftTaste, KeyValuePair<NPC, GiftTaste?>> group) => (from p in @group
					select ((Character)p.Key).displayName into p
					orderby p
					select p).ToArray());
				yield return new MovieTastesField(I18n.Item_MovieTicket_LovesMovie(), tastes, GiftTaste.Love);
				yield return new MovieTastesField(I18n.Item_MovieTicket_LikesMovie(), tastes, GiftTaste.Like);
				yield return new MovieTastesField(I18n.Item_MovieTicket_DislikesMovie(), tastes, GiftTaste.Dislike);
				yield return new MovieTastesField(I18n.Item_MovieTicket_RejectsMovie(), tastes, (GiftTaste)(-1));
			}
		}
		if (showInventoryFields)
		{
			yield return new ColorField(I18n.Item_ProducesDye(), item);
		}
		if (showInventoryFields && !isCrop)
		{
			yield return new GenericField(I18n.Item_NumberOwned(), this.GetNumberOwnedText(item));
			RecipeModel[] recipes2 = (from recipe in base.GameHelper.GetRecipes()
				where recipe.OutputQualifiedItemId == this.Target.QualifiedItemId
				select recipe).ToArray();
			if (recipes2.Any())
			{
				string label = ((recipes2.First().Type == RecipeType.Cooking) ? I18n.Item_NumberCooked() : I18n.Item_NumberCrafted());
				int timesCrafted = recipes2.Sum((RecipeModel recipe) => recipe.GetTimesCrafted(Game1.player));
				if (timesCrafted >= 0)
				{
					yield return new GenericField(label, I18n.Item_NumberCrafted_Summary(timesCrafted));
				}
			}
		}
		bool flag6 = isSeed && item.ItemId != ((NetFieldBase<string, NetString>)(object)this.SeedForCrop.indexOfHarvest).Value;
		bool flag7 = flag6;
		if (flag7)
		{
			bool flag5;
			switch (item.ItemId)
			{
			case "495":
			case "496":
			case "497":
				flag5 = true;
				break;
			default:
				flag5 = false;
				break;
			}
			flag7 = !flag5;
		}
		if (flag7 && item.ItemId != "770")
		{
			string dropName2 = ItemRegistry.GetDataOrErrorItem(((NetFieldBase<string, NetString>)(object)this.SeedForCrop.indexOfHarvest).Value).DisplayName;
			yield return new LinkField(I18n.Item_SeeAlso(), dropName2, () => this.GetCropSubject(this.SeedForCrop, ObjectContext.Inventory, null));
		}
		yield return new GenericField(I18n.InternalId(), I18n.Item_InternalId_Summary(item.ItemId, item.QualifiedItemId));
		static string AddSign(float value)
		{
			return ((value > 0f) ? "+" : "") + value;
		}
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		Item target = this.Target;
		Object obj = (Object)(object)((target is Object) ? target : null);
		Crop crop = this.FromCrop ?? this.SeedForCrop;
		yield return new GenericDebugField("item ID", target.QualifiedItemId, null, pinned: true);
		yield return new GenericDebugField("sprite index", target.ParentSheetIndex, null, pinned: true);
		yield return new GenericDebugField("category", $"{target.Category} ({target.getCategoryName()})", null, pinned: true);
		if (obj != null)
		{
			yield return new GenericDebugField("edibility", obj.Edibility, null, pinned: true);
			yield return new GenericDebugField("item type", obj.Type, null, pinned: true);
		}
		if (crop != null)
		{
			yield return new GenericDebugField("crop fully grown", base.Stringify(((NetFieldBase<bool, NetBool>)(object)crop.fullyGrown).Value), null, pinned: true);
			yield return new GenericDebugField("crop phase", $"{crop.currentPhase} (day {crop.dayOfCurrentPhase} in phase)", null, pinned: true);
		}
		yield return new GenericDebugField("context tags", I18n.List(target.GetContextTags().OrderBy<string, string>((string p) => p, new HumanSortComparer())), null, pinned: true);
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
		if (crop == null)
		{
			yield break;
		}
		foreach (IDebugField field in base.GetDebugFieldsFrom(crop))
		{
			yield return new GenericDebugField("crop::" + field.Label, field.Value, field.HasValue, field.IsPinned);
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		this.Target.drawInMenu(spriteBatch, position, 1f, 1f, 1f, (StackDrawType)0, Color.White, false);
		return true;
	}

	private string? GetDescription(Item item)
	{
		try
		{
			_ = item.DisplayName;
			MeleeWeapon weapon = (MeleeWeapon)(object)((item is MeleeWeapon) ? item : null);
			return (weapon != null && !((Tool)weapon).isScythe()) ? ((Tool)weapon).Description : item.getDescription();
		}
		catch (KeyNotFoundException)
		{
			return null;
		}
	}

	private string GetTypeValue(Item item)
	{
		string categoryName = item.getCategoryName();
		if (string.IsNullOrWhiteSpace(categoryName))
		{
			return I18n.Type_Other();
		}
		return categoryName;
	}

	private Crop? TryGetCropForSeed(Item seed, GameLocation? location)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!ItemExtensions.HasTypeId((IHaveItemTypeId)(object)seed, "(O)"))
		{
			return null;
		}
		try
		{
			CropData val = default(CropData);
			return (!Crop.TryGetData(seed.ItemId, ref val)) ? ((Crop)null) : new Crop(seed.ItemId, 0, 0, (GameLocation)(((object)location) ?? ((object)Game1.getFarm())));
		}
		catch
		{
			return null;
		}
	}

	private IEnumerable<ICustomField> GetCropFields(HoeDirt? dirt, Crop? crop, Object? producedItem, bool isSeed)
	{
		CropDataParser data = new CropDataParser(crop, !isSeed);
		if (data.CropData == null || crop == null)
		{
			yield break;
		}
		bool isForage = CommonHelper.IsItemId(((NetFieldBase<string, NetString>)(object)crop.whichForageCrop).Value, allowZero: false) && ((NetFieldBase<bool, NetBool>)(object)crop.fullyGrown).Value;
		if (!isSeed && !isForage)
		{
			SDate nextHarvest = data.GetNextHarvest();
			yield return new GenericField(value: data.CanHarvestNow ? I18n.Generic_Now() : ((Game1.currentLocation.SeedsIgnoreSeasonsHere() || data.Seasons.Contains(nextHarvest.Season)) ? (base.Stringify(nextHarvest) + " (" + base.GetRelativeDateStr(nextHarvest) + ")") : I18n.Crop_Harvest_TooLate(base.Stringify(nextHarvest))), label: I18n.Crop_Harvest());
		}
		if (!isForage)
		{
			List<string> summary = new List<string>();
			if (!((NetFieldBase<bool, NetBool>)(object)crop.forageCrop).Value)
			{
				summary.Add(data.HasMultipleHarvests ? I18n.Crop_Summary_HarvestMulti(data.DaysToFirstHarvest, data.DaysToSubsequentHarvest) : I18n.Crop_Summary_HarvestOnce(data.DaysToFirstHarvest));
			}
			summary.Add(I18n.Crop_Summary_Seasons(I18n.List(I18n.GetSeasonNames(data.Seasons))));
			if (data.CropData != null)
			{
				int minStack = data.CropData.HarvestMinStack;
				int maxStack = data.CropData.HarvestMaxStack;
				double extraHarvestChance = data.CropData.ExtraHarvestChance;
				if (minStack != maxStack)
				{
					summary.Add(I18n.Crop_Summary_DropsXToY(minStack, maxStack, (int)Math.Round(extraHarvestChance * 100.0, 2)));
				}
				else if (minStack > 1)
				{
					summary.Add(I18n.Crop_Summary_DropsX(minStack));
				}
			}
			else
			{
				summary.Add(I18n.Crop_Summary_DropsX(1));
			}
			if (((NetFieldBase<bool, NetBool>)(object)crop.forageCrop).Value)
			{
				summary.Add(I18n.Crop_Summary_ForagingXp(3));
			}
			else
			{
				int price = ((producedItem != null) ? producedItem.Price : 0);
				int experience = (int)Math.Round((float)(16.0 * Math.Log(0.018 * (double)price + 1.0, Math.E)));
				summary.Add(I18n.Crop_Summary_FarmingXp(experience));
			}
			Item drop = data.GetSampleDrop();
			summary.Add(I18n.Crop_Summary_SellsFor(GenericField.GetSaleValueString(this.GetSaleValue(drop, qualityIsKnown: false), 1)));
			yield return new GenericField(I18n.Crop_Summary(), "-" + string.Join(Environment.NewLine + "-", summary));
		}
		if (dirt != null && !isForage)
		{
			yield return new GenericField(I18n.Crop_Watered(), base.Stringify(((NetFieldBase<int, NetInt>)(object)dirt.state).Value == 1));
			string[] appliedFertilizers = (from p in this.GetAppliedFertilizers(dirt).Select(GameI18n.GetObjectName).Distinct()
					.DefaultIfEmpty(base.Stringify(false))
				orderby p
				select p).ToArray();
			yield return new GenericField(I18n.Crop_Fertilized(), I18n.List(appliedFertilizers));
		}
	}

	private IEnumerable<string> GetAppliedFertilizers(HoeDirt dirt)
	{
		if (base.GameHelper.MultiFertilizer.IsLoaded)
		{
			return base.GameHelper.MultiFertilizer.GetAppliedFertilizers(dirt);
		}
		if (ItemRegistry.QualifyItemId(((NetFieldBase<string, NetString>)(object)dirt.fertilizer).Value) != null)
		{
			return new _003C_003Ez__ReadOnlySingleElementList<string>(((NetFieldBase<string, NetString>)(object)dirt.fertilizer).Value);
		}
		return Array.Empty<string>();
	}

	private IEnumerable<ICustomField> GetMachineOutputFields(Object? machine)
	{
		if (machine == null)
		{
			yield break;
		}
		Object heldObj = ((NetFieldBase<Object, NetRef<Object>>)(object)machine.heldObject).Value;
		int minutesLeft = machine.MinutesUntilReady;
		Cask cask = (Cask)(object)((machine is Cask) ? machine : null);
		if (cask != null)
		{
			if (heldObj == null)
			{
				yield break;
			}
			ItemQuality curQuality = (ItemQuality)((Item)heldObj).Quality;
			float effectiveAge = (float)base.Constants.CaskAgeSchedule.Values.Max() - ((NetFieldBase<float, NetFloat>)(object)cask.daysToMature).Value;
			var schedule = (from _003C_003Eh__TransparentIdentifier0 in base.Constants.CaskAgeSchedule.Select(delegate(KeyValuePair<ItemQuality, int> entry)
				{
					KeyValuePair<ItemQuality, int> keyValuePair = entry;
					return new
					{
						entry = entry,
						quality = keyValuePair.Key
					};
				})
				let baseDays = entry.Value
				where (float)baseDays > effectiveAge
				orderby baseDays
				let daysLeft = (int)Math.Ceiling(((float)baseDays - effectiveAge) / ((NetFieldBase<float, NetFloat>)(object)cask.agingRate).Value)
				select new
				{
					Quality = quality,
					DaysLeft = daysLeft,
					HarvestDate = SDate.Now().AddDays(daysLeft)
				}).ToArray();
			yield return new ItemIconField(base.GameHelper, I18n.Item_Contents(), (Item?)(object)heldObj, this.Codex);
			if (minutesLeft <= 0 || !schedule.Any())
			{
				yield return new GenericField(I18n.Item_CaskSchedule(), I18n.Item_CaskSchedule_Now(I18n.For(curQuality)));
				yield break;
			}
			string scheduleStr = string.Join(Environment.NewLine, from entry in schedule
				let str = I18n.GetPlural(entry.DaysLeft, I18n.Item_CaskSchedule_Tomorrow(I18n.For(entry.Quality)), I18n.Item_CaskSchedule_InXDays(I18n.For(entry.Quality), entry.DaysLeft, base.Stringify(entry.HarvestDate)))
				select "-" + str);
			yield return new GenericField(I18n.Item_CaskSchedule(), I18n.Item_CaskSchedule_NowPartial(I18n.For(curQuality)) + Environment.NewLine + scheduleStr);
			yield break;
		}
		CrabPot pot = (CrabPot)(object)((machine is CrabPot) ? machine : null);
		if (pot != null)
		{
			if (heldObj == null)
			{
				if (((NetFieldBase<Object, NetRef<Object>>)(object)pot.bait).Value != null)
				{
					yield return new ItemIconField(base.GameHelper, I18n.Item_CrabpotBait(), (Item?)(object)((NetFieldBase<Object, NetRef<Object>>)(object)pot.bait).Value, this.Codex);
				}
				else if (((NetHashSet<int>)(object)Game1.player.professions).Contains(11))
				{
					yield return new GenericField(I18n.Item_CrabpotBait(), I18n.Item_CrabpotBaitNotNeeded());
				}
				else
				{
					yield return new GenericField(I18n.Item_CrabpotBait(), I18n.Item_CrabpotBaitNeeded());
				}
			}
			if (heldObj != null)
			{
				string summary = I18n.Item_Contents_Ready(((Item)heldObj).DisplayName);
				yield return new ItemIconField(base.GameHelper, I18n.Item_Contents(), (Item?)(object)heldObj, this.Codex, summary);
			}
		}
		else if (machine is Furniture)
		{
			if (heldObj != null)
			{
				string summary2 = I18n.Item_Contents_Placed(((Item)heldObj).DisplayName);
				yield return new ItemIconField(base.GameHelper, I18n.Item_Contents(), (Item?)(object)heldObj, this.Codex, summary2);
			}
		}
		else if (((Item)machine).QualifiedItemId == $"{"(BC)"}{Constant.ObjectIndexes.AutoGrabber}")
		{
			Chest output = (Chest)(object)((heldObj is Chest) ? heldObj : null);
			string readyText = I18n.Stringify(output != null && ((IEnumerable<Item>)output.GetItemsForPlayer(Game1.player.UniqueMultiplayerID)).Any());
			yield return new GenericField(I18n.Item_Contents(), readyText);
		}
		else if (heldObj != null)
		{
			string summary3 = ((minutesLeft <= 0) ? I18n.Item_Contents_Ready(((Item)heldObj).DisplayName) : I18n.Item_Contents_Partial(((Item)heldObj).DisplayName, base.Stringify(TimeSpan.FromMinutes(minutesLeft))));
			yield return new ItemIconField(base.GameHelper, I18n.Item_Contents(), (Item?)(object)heldObj, this.Codex, summary3);
		}
	}

	private IEnumerable<ICustomField> GetNeededForFields(Object? obj)
	{
		if (obj == null || ((Item)obj).TypeDefinitionId != "(O)")
		{
			yield break;
		}
		List<string> neededFor = new List<string>();
		string[] missingBundles = (from bundle in this.GetUnfinishedBundles(obj)
			orderby bundle.Area, bundle.DisplayName
			let countNeeded = this.GetIngredientCountNeeded(bundle, obj)
			select (countNeeded > 1) ? $"{this.GetTranslatedBundleArea(bundle)}: {bundle.DisplayName} x {countNeeded}" : (this.GetTranslatedBundleArea(bundle) + ": " + bundle.DisplayName)).ToArray();
		if (missingBundles.Any())
		{
			neededFor.Add(I18n.Item_NeededFor_CommunityCenter(I18n.List(missingBundles)));
		}
		CropData cropData = (CropData)((this.FromCrop != null) ? ((object)this.FromCrop.GetData()) : ((object)Pathoschild.Stardew.LookupAnything.GameHelper.GetCropDataByHarvestItem(((Item)obj).ItemId)));
		if (cropData != null)
		{
			if (cropData.CountForPolyculture && !((NetHashSet<int>)(object)Game1.player.achievements).Contains(31))
			{
				int needed = base.Constants.PolycultureCount - base.GameHelper.GetShipped(((Item)obj).ItemId);
				if (needed > 0)
				{
					neededFor.Add(I18n.Item_NeededFor_Polyculture(needed));
				}
			}
			if (cropData.CountForMonoculture && !((NetHashSet<int>)(object)Game1.player.achievements).Contains(32))
			{
				int needed2 = base.Constants.MonocultureCount - base.GameHelper.GetShipped(((Item)obj).ItemId);
				if (needed2 > 0)
				{
					neededFor.Add(I18n.Item_NeededFor_Monoculture(needed2));
				}
			}
		}
		if (base.GameHelper.GetFullShipmentAchievementItems().Any<KeyValuePair<string, bool>>((KeyValuePair<string, bool> p) => p.Key == ((Item)obj).QualifiedItemId && !p.Value))
		{
			neededFor.Add(I18n.Item_NeededFor_FullShipment());
		}
		if (obj.needsToBeDonated())
		{
			neededFor.Add(I18n.Item_NeededFor_FullCollection());
		}
		RecipeData[] missingRecipes = (from recipe in base.GameHelper.GetRecipesForIngredient(this.Target)
			let item = recipe.TryCreateItem(this.Target)
			let timesCrafted = recipe.GetTimesCrafted(Game1.player)
			where item != null && timesCrafted <= 0
			orderby item.DisplayName
			select new RecipeData(recipe.Type, item.DisplayName, timesCrafted, recipe.IsKnown())).ToArray();
		RecipeData[] missingCookingRecipes = missingRecipes.Where((RecipeData recipe) => recipe.Type == RecipeType.Cooking).ToArray();
		if (missingCookingRecipes.Length != 0)
		{
			string missingRecipesText = this.GetNeededForRecipeText(missingCookingRecipes);
			neededFor.Add(I18n.Item_NeededFor_GourmetChef(missingRecipesText));
		}
		RecipeData[] missingCraftingRecipes = missingRecipes.Where((RecipeData recipe) => recipe.Type == RecipeType.Crafting).ToArray();
		if (missingCraftingRecipes.Length != 0)
		{
			string missingRecipesText2 = this.GetNeededForRecipeText(missingCraftingRecipes);
			neededFor.Add(I18n.Item_NeededFor_CraftMaster(missingRecipesText2));
		}
		string[] quests = (from p in base.GameHelper.GetQuestsWhichNeedItem(obj)
			select p.DisplayText into p
			orderby p
			select p).ToArray();
		if (quests.Any())
		{
			neededFor.Add(I18n.Item_NeededFor_Quests(I18n.List(quests)));
		}
		if (neededFor.Any())
		{
			yield return new GenericField(I18n.Item_NeededFor(), I18n.List(neededFor));
		}
	}

	private string GetNeededForRecipeText(RecipeData[] missingRecipes)
	{
		if (this.ShowUnknownRecipes)
		{
			string[] recipeNames = missingRecipes.Select((RecipeData recipe) => recipe.DisplayName).ToArray();
			return I18n.List(recipeNames);
		}
		RecipeData[] knownMissingRecipes = missingRecipes.Where((RecipeData recipe) => recipe.IsKnown).ToArray();
		int unknownMissingRecipeCount = missingRecipes.Length - knownMissingRecipes.Length;
		if (knownMissingRecipes.Any())
		{
			string[] knownRecipeNames = knownMissingRecipes.Select((RecipeData recipe) => recipe.DisplayName).ToArray();
			return I18n.List(knownRecipeNames.Append(I18n.Item_UnknownRecipes(unknownMissingRecipeCount)));
		}
		return I18n.Item_UnknownRecipes(unknownMissingRecipeCount);
	}

	private IEnumerable<BundleModel> GetUnfinishedBundles(Object item)
	{
		if (Game1.player.hasOrWillReceiveMail("JojaMember"))
		{
			yield break;
		}
		bool value = ((NetFieldBase<bool, NetBool>)(object)item.bigCraftable).Value;
		bool flag = value;
		if (!flag)
		{
			bool flag2 = ((item is Cask || item is Fence || item is Furniture || item is IndoorPot || item is Sign || item is Torch || item is Wallpaper) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			yield break;
		}
		CommunityCenter communityCenter = Game1.locations.OfType<CommunityCenter>().First();
		if (communityCenter.areAllAreasComplete() && !IsBundleOpen(36))
		{
			yield break;
		}
		foreach (BundleModel bundle in base.GameHelper.GetBundleData())
		{
			if (IsBundleOpen(bundle.ID) && this.GetIngredientsFromBundle(bundle, item).Any((BundleIngredientModel p) => this.IsIngredientNeeded(bundle, p)))
			{
				yield return bundle;
			}
		}
		bool IsBundleOpen(int id)
		{
			try
			{
				return !communityCenter.isBundleComplete(id);
			}
			catch
			{
				return false;
			}
		}
	}

	private string GetNumberOwnedText(Item item)
	{
		int baseCount = base.GameHelper.CountOwnedItems(item, flavorSpecific: false);
		int flavoredCount = ((item is Object) ? base.GameHelper.CountOwnedItems(item, flavorSpecific: true) : baseCount);
		if (baseCount != flavoredCount)
		{
			ParsedItemData baseData = ItemRegistry.GetData(item.QualifiedItemId);
			if (baseData != null)
			{
				object name = item.Name;
				return I18n.Item_NumberOwnedFlavored_Summary(flavoredCount, name, baseName: baseData.DisplayName, baseCount: baseCount);
			}
		}
		return I18n.Item_NumberOwned_Summary(flavoredCount);
	}

	private string GetTranslatedBundleArea(BundleModel bundle)
	{
		return bundle.Area switch
		{
			"Pantry" => I18n.BundleArea_Pantry(), 
			"Crafts Room" => I18n.BundleArea_CraftsRoom(), 
			"Fish Tank" => I18n.BundleArea_FishTank(), 
			"Boiler Room" => I18n.BundleArea_BoilerRoom(), 
			"Vault" => I18n.BundleArea_Vault(), 
			"Bulletin Board" => I18n.BundleArea_BulletinBoard(), 
			"Abandoned Joja Mart" => I18n.BundleArea_AbandonedJojaMart(), 
			_ => bundle.Area, 
		};
	}

	private IDictionary<ItemQuality, int> GetSaleValue(Item item, bool qualityIsKnown)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		Item one = item.getOne();
		Object obj = (Object)(object)((one is Object) ? one : null);
		if (obj == null || !base.GameHelper.CanHaveQuality(item) || qualityIsKnown)
		{
			ItemQuality quality = ((qualityIsKnown && obj != null) ? ((ItemQuality)((Item)obj).Quality) : ItemQuality.Normal);
			return new Dictionary<ItemQuality, int> { [quality] = this.GetRawSalePrice(item) };
		}
		string[] iridiumItems = base.Constants.ItemsWithIridiumQuality;
		Dictionary<ItemQuality, int> prices = new Dictionary<ItemQuality, int>();
		Object sample = (Object)item.getOne();
		foreach (ItemQuality quality2 in CommonHelper.GetEnumValues<ItemQuality>())
		{
			if (quality2 != ItemQuality.Iridium || iridiumItems.Contains(item.QualifiedItemId) || iridiumItems.Contains(item.Category.ToString()))
			{
				((Item)sample).Quality = (int)quality2;
				prices[quality2] = this.GetRawSalePrice((Item)(object)sample);
			}
		}
		return prices;
	}

	private int GetRawSalePrice(Item item)
	{
		Object obj = (Object)(object)((item is Object) ? item : null);
		int price = ((obj != null) ? ((Item)obj).sellToStorePrice(-1L) : (item.salePrice(false) / 2));
		if (price <= 0)
		{
			return 0;
		}
		return price;
	}

	private IDictionary<GiftTaste, GiftTasteModel[]> GetGiftTastes(Item item)
	{
		return (from p in base.GameHelper.GetGiftTastes(item)
			group p by p.Taste).ToDictionary((IGrouping<GiftTaste, GiftTasteModel> p) => p.Key, (IGrouping<GiftTaste, GiftTasteModel> p) => p.Distinct().ToArray());
	}

	private IEnumerable<BundleIngredientModel> GetIngredientsFromBundle(BundleModel bundle, Object item)
	{
		return bundle.Ingredients.Where(delegate(BundleIngredientModel required)
		{
			string itemId = required.ItemId;
			if ((itemId == null || itemId == "-1") ? true : false)
			{
				return false;
			}
			if (!ItemRegistry.HasItemId((Item)(object)item, required.ItemId) && required.ItemId != ((Item)item).Category.ToString())
			{
				return false;
			}
			return (((Item)item).Quality >= (int)required.Quality) ? true : false;
		});
	}

	private bool IsIngredientNeeded(BundleModel bundle, BundleIngredientModel ingredient)
	{
		CommunityCenter communityCenter = Game1.locations.OfType<CommunityCenter>().First();
		bool[] items = default(bool[]);
		if (!((NetDictionary<int, bool[], NetArray<bool, NetBool>, SerializableDictionary<int, bool[]>, NetBundles>)(object)communityCenter.bundles).TryGetValue(bundle.ID, ref items) || ingredient.Index >= items.Length)
		{
			return true;
		}
		return !items[ingredient.Index];
	}

	private int GetIngredientCountNeeded(BundleModel bundle, Object item)
	{
		return (from p in this.GetIngredientsFromBundle(bundle, item)
			where this.IsIngredientNeeded(bundle, p)
			select p).Sum((BundleIngredientModel p) => p.Stack);
	}
}
