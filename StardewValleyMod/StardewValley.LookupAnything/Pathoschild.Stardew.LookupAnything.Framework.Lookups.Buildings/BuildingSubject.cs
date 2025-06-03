using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
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
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FishPonds;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using xTile;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Buildings;

internal class BuildingSubject : BaseSubject
{
	private readonly Building Target;

	private readonly Rectangle SourceRectangle;

	private readonly ISubjectRegistry Codex;

	private readonly ModCollapseLargeFieldsConfig CollapseFieldsConfig;

	private readonly bool ShowInvalidRecipes;

	public BuildingSubject(ISubjectRegistry codex, GameHelper gameHelper, Building building, Rectangle sourceRectangle, ModCollapseLargeFieldsConfig collapseFieldsConfig, bool showInvalidRecipes)
		: base(gameHelper, ((NetFieldBase<string, NetString>)(object)building.buildingType).Value, null, I18n.Type_Building())
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		this.Codex = codex;
		this.Target = building;
		this.SourceRectangle = sourceRectangle;
		this.CollapseFieldsConfig = collapseFieldsConfig;
		this.ShowInvalidRecipes = showInvalidRecipes;
		BuildingData buildingData = building.GetData();
		base.Name = TokenParser.ParseText(buildingData?.Name, (Random)null, (TokenParserDelegate)null, (Farmer)null) ?? base.Name;
		base.Description = TokenParser.ParseText(buildingData?.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null) ?? base.Description;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		Building building = this.Target;
		BuildingData data = building.GetData();
		bool built = !building.isUnderConstruction(true);
		int? upgradeLevel = this.GetUpgradeLevel(building);
		IModInfo fromMod = base.GameHelper.TryGetModFromStringId(((NetFieldBase<string, NetString>)(object)building.buildingType).Value);
		if (fromMod != null)
		{
			yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(fromMod.Manifest.Name));
		}
		if (!built || ((NetFieldBase<int, NetInt>)(object)building.daysUntilUpgrade).Value > 0)
		{
			int daysLeft = (building.isUnderConstruction(true) ? ((NetFieldBase<int, NetInt>)(object)building.daysOfConstructionLeft).Value : ((NetFieldBase<int, NetInt>)(object)building.daysUntilUpgrade).Value);
			SDate readyDate = SDate.Now().AddDays(daysLeft);
			yield return new GenericField(I18n.Building_Construction(), I18n.Building_Construction_Summary(base.Stringify(readyDate)));
		}
		Farmer owner = this.GetOwner();
		if (owner != null)
		{
			yield return new LinkField(I18n.Building_Owner(), ((Character)owner).Name, () => this.Codex.GetByEntity(owner, ((Character)owner).currentLocation));
		}
		else if (building.GetIndoors() is Cabin)
		{
			yield return new GenericField(I18n.Building_Owner(), I18n.Building_Owner_None());
		}
		if (built)
		{
			Building obj = building;
			Stable stable = (Stable)(object)((obj is Stable) ? obj : null);
			if (stable != null)
			{
				Horse horse = Utility.findHorse(stable.HorseId);
				if (horse != null)
				{
					yield return new LinkField(I18n.Building_Horse(), ((Character)horse).Name, () => this.Codex.GetByEntity(horse, ((Character)horse).currentLocation));
					yield return new GenericField(I18n.Building_HorseLocation(), I18n.Building_HorseLocation_Summary(((Character)horse).currentLocation.Name, ((Character)horse).TilePoint.X, ((Character)horse).TilePoint.Y));
				}
			}
		}
		if (built)
		{
			GameLocation indoors = building.GetIndoors();
			AnimalHouse animalHouse = (AnimalHouse)(object)((indoors is AnimalHouse) ? indoors : null);
			if (animalHouse != null)
			{
				yield return new GenericField(I18n.Building_Animals(), I18n.Building_Animals_Summary(((NetList<long, NetLong>)(object)animalHouse.animalsThatLiveHere).Count, ((NetFieldBase<int, NetInt>)(object)animalHouse.animalLimit).Value));
				if (upgradeLevel >= 2 && (this.IsBarn(building) || this.IsCoop(building)))
				{
					yield return new GenericField(I18n.Building_FeedTrough(), I18n.Building_FeedTrough_Automated());
				}
				else
				{
					this.GetFeedMetrics(animalHouse, out var totalFeedSpaces, out var filledFeedSpaces);
					yield return new GenericField(I18n.Building_FeedTrough(), I18n.Building_FeedTrough_Summary(filledFeedSpaces, totalFeedSpaces));
				}
			}
		}
		if (built)
		{
			GameLocation indoors = building.GetIndoors();
			SlimeHutch slimeHutch = (SlimeHutch)(object)((indoors is SlimeHutch) ? indoors : null);
			if (slimeHutch != null)
			{
				int slimeCount = ((IEnumerable)((GameLocation)slimeHutch).characters).OfType<GreenSlime>().Count();
				yield return new GenericField(I18n.Building_Slimes(), I18n.Building_Slimes_Summary(slimeCount, 20));
				yield return new GenericField(I18n.Building_WaterTrough(), I18n.Building_WaterTrough_Summary(((IEnumerable<bool>)slimeHutch.waterSpots).Count((bool p) => p), slimeHutch.waterSpots.Count));
			}
		}
		if (built)
		{
			Checkbox[] upgradeLevelSummary = this.GetUpgradeLevelSummary(building, upgradeLevel).ToArray();
			if (upgradeLevelSummary.Any())
			{
				yield return new CheckboxListField(I18n.Building_Upgrades(), new CheckboxList(upgradeLevelSummary));
			}
		}
		if (built)
		{
			FishPond val = default(FishPond);
			ref FishPond reference = ref val;
			Building obj2 = building;
			reference = (FishPond)(object)((obj2 is FishPond) ? obj2 : null);
			if (val == null)
			{
				Building obj3 = building;
				JunimoHut hut = (JunimoHut)(object)((obj3 is JunimoHut) ? obj3 : null);
				if (hut != null)
				{
					yield return new GenericField(I18n.Building_JunimoHarvestingEnabled(), I18n.Stringify(!((NetFieldBase<bool, NetBool>)(object)hut.noHarvest).Value));
					GameHelper gameHelper = base.GameHelper;
					string label = I18n.Building_OutputReady();
					Chest outputChest = hut.GetOutputChest();
					yield return new ItemIconListField(gameHelper, label, (IEnumerable<Item?>?)((outputChest != null) ? outputChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID) : null), showStackSize: true);
				}
				else
				{
					RecipeModel[] recipes = base.GameHelper.GetRecipesForBuilding(building).ToArray();
					if (recipes.Length != 0)
					{
						ItemRecipesField field = new ItemRecipesField(base.GameHelper, this.Codex, I18n.Item_Recipes(), null, recipes, showUnknownRecipes: true, this.ShowInvalidRecipes);
						if (this.CollapseFieldsConfig.Enabled)
						{
							field.CollapseIfLengthExceeds(this.CollapseFieldsConfig.BuildingRecipes, recipes.Length);
						}
						yield return field;
						if (MachineDataHelper.TryGetBuildingChestNames(data, out ISet<string> inputChestIds, out ISet<string> outputChestIds))
						{
							IEnumerable<Item> inputItems = MachineDataHelper.GetBuildingChests(building, inputChestIds).SelectMany((Chest p) => (IEnumerable<Item>)p.GetItemsForPlayer());
							IEnumerable<Item?> outputItems = MachineDataHelper.GetBuildingChests(building, outputChestIds).SelectMany((Chest p) => (IEnumerable<Item>)p.GetItemsForPlayer());
							yield return new ItemIconListField(base.GameHelper, I18n.Building_OutputProcessing(), inputItems, showStackSize: true);
							yield return new ItemIconListField(base.GameHelper, I18n.Building_OutputReady(), outputItems, showStackSize: true);
						}
					}
				}
			}
			else if (!CommonHelper.IsItemId(((NetFieldBase<string, NetString>)(object)val.fishType).Value))
			{
				yield return new GenericField(I18n.Building_FishPond_Population(), I18n.Building_FishPond_Population_Empty());
			}
			else
			{
				Object fish = val.GetFishObject();
				((Item)fish).Stack = val.FishCount;
				FishPondData pondData = val.GetFishPondData();
				string populationStr = ((Item)fish).DisplayName + " (" + I18n.Generic_Ratio(val.FishCount, ((NetFieldBase<int, NetInt>)(object)((Building)val).maxOccupants).Value) + ")";
				if (val.FishCount < ((NetFieldBase<int, NetInt>)(object)((Building)val).maxOccupants).Value)
				{
					SDate nextSpawn = SDate.Now().AddDays(pondData.SpawnTime - ((NetFieldBase<int, NetInt>)(object)val.daysSinceSpawn).Value);
					populationStr = populationStr + Environment.NewLine + I18n.Building_FishPond_Population_NextSpawn(base.GetRelativeDateStr(nextSpawn));
				}
				yield return new ItemIconField(base.GameHelper, I18n.Building_FishPond_Population(), (Item?)(object)fish, this.Codex, populationStr);
				yield return new ItemIconField(base.GameHelper, I18n.Building_OutputReady(), ((NetFieldBase<Item, NetRef<Item>>)(object)val.output).Value, this.Codex);
				float chanceOfAnyDrop = ((pondData.BaseMinProduceChance >= pondData.BaseMaxProduceChance) ? pondData.BaseMinProduceChance : Utility.Lerp(pondData.BaseMinProduceChance, pondData.BaseMaxProduceChance, (float)((NetFieldBase<int, NetInt>)(object)((Building)val).currentOccupants).Value / 10f));
				yield return new FishPondDropsField(base.GameHelper, this.Codex, I18n.Building_FishPond_Drops(), ((NetFieldBase<int, NetInt>)(object)((Building)val).currentOccupants).Value, pondData, fish, I18n.Building_FishPond_Drops_Preface((chanceOfAnyDrop * 100f).ToString("0.##")));
				if (pondData.PopulationGates?.Any((KeyValuePair<int, List<string>> gate) => gate.Key > ((NetFieldBase<int, NetInt>)(object)val.lastUnlockedPopulationGate).Value) ?? false)
				{
					yield return new CheckboxListField(I18n.Building_FishPond_Quests(), new CheckboxList(this.GetPopulationGates(val, pondData)));
				}
			}
			if (((NetFieldBase<int, NetInt>)(object)building.hayCapacity).Value > 0)
			{
				Farm farm = Game1.getFarm();
				int hayCount = ((NetFieldBase<int, NetInt>)(object)((GameLocation)farm).piecesOfHay).Value;
				int maxHay = Math.Max(((NetFieldBase<int, NetInt>)(object)((GameLocation)farm).piecesOfHay).Value, ((GameLocation)farm).GetHayCapacity());
				yield return new GenericField(I18n.Building_StoredHay(), I18n.Building_StoredHay_Summary(hayCount, maxHay, ((NetFieldBase<int, NetInt>)(object)building.hayCapacity).Value));
			}
		}
		RecipeModel[] recipes2 = (from recipe in base.GameHelper.GetRecipes()
			where recipe.Type == RecipeType.BuildingBlueprint && recipe.MachineId == ((NetFieldBase<string, NetString>)(object)building.buildingType).Value
			select recipe).ToArray();
		if (recipes2.Length != 0)
		{
			ItemRecipesField field2 = new ItemRecipesField(base.GameHelper, this.Codex, I18n.Building_ConstructionCosts(), null, recipes2, showUnknownRecipes: true, this.ShowInvalidRecipes, showLabelForSingleGroup: false, showOutputLabels: false);
			if (this.CollapseFieldsConfig.Enabled)
			{
				field2.CollapseIfLengthExceeds(this.CollapseFieldsConfig.BuildingRecipes, recipes2.Length);
			}
			yield return field2;
		}
		yield return new GenericField(I18n.InternalId(), ((NetFieldBase<string, NetString>)(object)building.buildingType).Value);
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		Building target = this.Target;
		yield return new GenericDebugField("building type", ((NetFieldBase<string, NetString>)(object)target.buildingType).Value, null, pinned: true);
		yield return new GenericDebugField("days of construction left", ((NetFieldBase<int, NetInt>)(object)target.daysOfConstructionLeft).Value, null, pinned: true);
		yield return new GenericDebugField("indoors name", target.GetIndoorsName(), null, pinned: true);
		yield return new GenericDebugField("indoors type", ((object)target.GetIndoorsType()/*cast due to .constrained prefix*/).ToString(), null, pinned: true);
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Building target = this.Target;
		spriteBatch.Draw(target.texture.Value, position, (Rectangle?)this.SourceRectangle, target.color, 0f, Vector2.Zero, size.X / (float)this.SourceRectangle.Width, (SpriteEffects)0, 0.89f);
		return true;
	}

	private bool IsBarn(Building? building)
	{
		switch (((NetFieldBase<string, NetString>)(object)building?.buildingType).Value)
		{
		case "Barn":
		case "Big Barn":
		case "Deluxe Barn":
			return true;
		default:
			return false;
		}
	}

	private bool IsCoop(Building? building)
	{
		switch (((NetFieldBase<string, NetString>)(object)building?.buildingType).Value)
		{
		case "Coop":
		case "Big Coop":
		case "Deluxe Coop":
			return true;
		default:
			return false;
		}
	}

	private Farmer? GetOwner()
	{
		Building target = this.Target;
		Stable stable = (Stable)(object)((target is Stable) ? target : null);
		if (stable != null)
		{
			long ownerID = ((NetFieldBase<long, NetLong>)(object)((Building)stable).owner).Value;
			return Game1.GetPlayer(ownerID, false);
		}
		GameLocation indoors = this.Target.GetIndoors();
		Cabin cabin = (Cabin)(object)((indoors is Cabin) ? indoors : null);
		if (cabin != null)
		{
			return ((FarmHouse)cabin).owner;
		}
		return null;
	}

	private int? GetUpgradeLevel(Building building)
	{
		if (this.IsBarn(building) && int.TryParse(((NetFieldBase<string, NetString>)(object)building.GetIndoors()?.mapPath).Value?.Substring("Maps\\Barn".Length), out var barnUpgradeLevel))
		{
			return barnUpgradeLevel - 1;
		}
		GameLocation indoors = building.GetIndoors();
		Cabin cabin = (Cabin)(object)((indoors is Cabin) ? indoors : null);
		if (cabin != null)
		{
			return ((FarmHouse)cabin).upgradeLevel;
		}
		if (this.IsCoop(building) && int.TryParse(((NetFieldBase<string, NetString>)(object)building.GetIndoors()?.mapPath).Value?.Substring("Maps\\Coop".Length), out var coopUpgradeLevel))
		{
			return coopUpgradeLevel - 1;
		}
		return null;
	}

	private void GetFeedMetrics(AnimalHouse building, out int total, out int filled)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Map map = ((GameLocation)building).Map;
		total = 0;
		filled = 0;
		Object obj = default(Object);
		for (int x = 0; x < map.Layers[0].LayerWidth; x++)
		{
			for (int y = 0; y < map.Layers[0].LayerHeight; y++)
			{
				if (((GameLocation)building).doesTileHaveProperty(x, y, "Trough", "Back", false) != null)
				{
					total++;
					if (((GameLocation)building).objects.TryGetValue(new Vector2((float)x, (float)y), ref obj) && ((Item)obj).QualifiedItemId == "(O)178")
					{
						filled++;
					}
				}
			}
		}
	}

	private IEnumerable<Checkbox> GetUpgradeLevelSummary(Building building, int? upgradeLevel)
	{
		if (this.IsBarn(building))
		{
			yield return new Checkbox(isChecked: true, I18n.Building_Upgrades_Barn_0());
			string text = I18n.Building_Upgrades_Barn_1();
			yield return new Checkbox(upgradeLevel >= 1, text);
			text = I18n.Building_Upgrades_Barn_2();
			yield return new Checkbox(upgradeLevel >= 2, text);
		}
		else if (building.GetIndoors() is Cabin)
		{
			yield return new Checkbox(isChecked: true, I18n.Building_Upgrades_Cabin_0());
			string text = I18n.Building_Upgrades_Cabin_1();
			yield return new Checkbox(upgradeLevel >= 1, text);
			text = I18n.Building_Upgrades_Cabin_2();
			yield return new Checkbox(upgradeLevel >= 2, text);
		}
		else if (this.IsCoop(building))
		{
			yield return new Checkbox(isChecked: true, I18n.Building_Upgrades_Coop_0());
			string text = I18n.Building_Upgrades_Coop_1();
			yield return new Checkbox(upgradeLevel >= 1, text);
			text = I18n.Building_Upgrades_Coop_2();
			yield return new Checkbox(upgradeLevel >= 2, text);
		}
	}

	private IEnumerable<Checkbox> GetPopulationGates(FishPond pond, FishPondData data)
	{
		bool foundNextQuest = false;
		foreach (FishPondPopulationGateData gate in base.GameHelper.GetFishPondPopulationGates(data))
		{
			int newPopulation = gate.NewPopulation;
			if (((NetFieldBase<int, NetInt>)(object)pond.lastUnlockedPopulationGate).Value >= gate.RequiredPopulation)
			{
				yield return new Checkbox(isChecked: true, I18n.Building_FishPond_Quests_Done(newPopulation));
				continue;
			}
			string[] requiredItems = gate.RequiredItems.Select(delegate(FishPondPopulationGateQuestItemData drop)
			{
				string text = ItemRegistry.GetDataOrErrorItem(drop.ItemID).DisplayName;
				if (drop.MinCount != drop.MaxCount)
				{
					text = text + " (" + I18n.Generic_Range(drop.MinCount, drop.MaxCount) + ")";
				}
				else if (drop.MinCount > 1)
				{
					text += $" ({drop.MinCount})";
				}
				return text;
			}).ToArray();
			string result = ((requiredItems.Length > 1) ? I18n.Building_FishPond_Quests_IncompleteRandom(newPopulation, I18n.List(requiredItems)) : I18n.Building_FishPond_Quests_IncompleteOne(newPopulation, requiredItems[0]));
			if (!foundNextQuest)
			{
				foundNextQuest = true;
				int nextQuestDays = data.SpawnTime + data.SpawnTime * (((NetFieldBase<int, NetInt>)(object)((Building)pond).maxOccupants).Value - ((NetFieldBase<int, NetInt>)(object)((Building)pond).currentOccupants).Value) - ((NetFieldBase<int, NetInt>)(object)pond.daysSinceSpawn).Value;
				result = result + "; " + I18n.Building_FishPond_Quests_Available(base.GetRelativeDateStr(nextQuestDays));
			}
			yield return new Checkbox(isChecked: false, result);
		}
	}
}
