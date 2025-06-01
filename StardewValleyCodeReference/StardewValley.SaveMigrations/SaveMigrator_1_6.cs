using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Tools;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.SaveMigrations;

/// <summary>Migrates existing save files for compatibility with Stardew Valley 1.6.</summary>
public class SaveMigrator_1_6 : ISaveMigrator
{
	/// <summary>The pre-1.6 structure of <see cref="T:StardewValley.Quests.DescriptionElement" />.</summary>
	public class LegacyDescriptionElement
	{
		/// <summary>The translation key for the text to render.</summary>
		public string xmlKey;

		/// <summary>The values to substitute for placeholders like <c>{0}</c> in the translation text.</summary>
		public List<object> param;
	}

	/// <inheritdoc />
	public Version GameVersion { get; } = new Version(1, 5);

	/// <inheritdoc />
	public bool ApplySaveFix(SaveFixes saveFix)
	{
		switch (saveFix)
		{
		case SaveFixes.MigrateBuildingsToData:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building is JunimoHut { obsolete_output: not null } junimoHut)
				{
					junimoHut.GetOutputChest().Items.AddRange(junimoHut.obsolete_output.Items);
					junimoHut.obsolete_output = null;
				}
				if (building.isUnderConstruction(ignoreUpgrades: false))
				{
					Game1.netWorldState.Value.MarkUnderConstruction("Robin", building);
					if (building.daysUntilUpgrade.Value > 0 && string.IsNullOrWhiteSpace(building.upgradeName.Value))
					{
						building.upgradeName.Value = SaveMigrator_1_6.InferBuildingUpgradingTo(building.buildingType.Value);
					}
				}
				return true;
			});
			return true;
		case SaveFixes.ModularizeFarmhouse:
			Game1.getFarm().AddDefaultBuildings();
			return true;
		case SaveFixes.ModularizePets:
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				allFarmer.whichPetType = ((allFarmer.obsolete_catPerson ?? false) ? "Cat" : "Dog");
				allFarmer.obsolete_catPerson = null;
			}
			Utility.ForEachLocation(delegate(GameLocation gameLocation)
			{
				for (int num2 = gameLocation.characters.Count - 1; num2 >= 0; num2--)
				{
					if (gameLocation.characters[num2] is Pet pet3)
					{
						string text = null;
						if (pet3.GetType() == typeof(Cat))
						{
							text = "Cat";
						}
						else if (pet3.GetType() == typeof(Dog))
						{
							text = "Dog";
						}
						if (text != null)
						{
							Pet pet4 = new Pet((int)(pet3.Position.X / 64f), (int)(pet3.Position.X / 64f), pet3.whichBreed.Value, text)
							{
								Name = pet3.Name,
								displayName = pet3.displayName
							};
							if (pet3.currentLocation != null)
							{
								pet4.currentLocation = pet3.currentLocation;
							}
							pet4.friendshipTowardFarmer.Value = pet3.friendshipTowardFarmer.Value;
							pet4.grantedFriendshipForPet.Value = pet3.grantedFriendshipForPet.Value;
							pet4.lastPetDay.Clear();
							pet4.lastPetDay.CopyFrom(pet3.lastPetDay.Pairs);
							pet4.isSleepingOnFarmerBed.Value = pet3.isSleepingOnFarmerBed.Value;
							pet4.modData.CopyFrom(pet3.modData);
							gameLocation.characters[num2] = pet4;
						}
					}
				}
				return true;
			});
			Farm farm2 = Game1.getFarm();
			farm2.AddDefaultBuilding("Pet Bowl", farm2.GetStarterPetBowlLocation());
			PetBowl bowl2 = farm2.getBuildingByType("Pet Bowl") as PetBowl;
			Pet pet2 = Game1.player.getPet();
			if (bowl2 != null && pet2 != null)
			{
				bowl2.AssignPet(pet2);
				pet2.setAtFarmPosition();
			}
			return true;
		}
		case SaveFixes.AddNpcRemovalFlags:
		{
			GameLocation location = Game1.getLocationFromName("WitchSwamp");
			if (location != null && location.getCharacterFromName("Henchman") == null)
			{
				Game1.addMail("henchmanGone", noLetter: true, sendToEveryone: true);
			}
			location = Game1.getLocationFromName("SandyHouse");
			if (location != null && location.getCharacterFromName("Bouncer") == null)
			{
				Game1.addMail("bouncerGone", noLetter: true, sendToEveryone: true);
			}
			return true;
		}
		case SaveFixes.MigrateFarmhands:
			return true;
		case SaveFixes.MigrateLitterItemData:
			Utility.ForEachItem(delegate(Item item3)
			{
				switch (item3.QualifiedItemId)
				{
				case "(O)2":
				case "(O)4":
				case "(O)6":
				case "(O)8":
				case "(O)10":
				case "(O)12":
				case "(O)14":
				case "(O)25":
				case "(O)75":
				case "(O)76":
				case "(O)77":
				case "(O)95":
				case "(O)290":
				case "(O)751":
				case "(O)764":
				case "(O)765":
				case "(O)816":
				case "(O)817":
				case "(O)818":
				case "(O)819":
				case "(O)843":
				case "(O)844":
				case "(O)849":
				case "(O)850":
				case "(O)32":
				case "(O)34":
				case "(O)36":
				case "(O)38":
				case "(O)40":
				case "(O)42":
				case "(O)44":
				case "(O)46":
				case "(O)48":
				case "(O)50":
				case "(O)52":
				case "(O)54":
				case "(O)56":
				case "(O)58":
				case "(O)343":
				case "(O)450":
				case "(O)668":
				case "(O)670":
				case "(O)760":
				case "(O)762":
				case "(O)845":
				case "(O)846":
				case "(O)847":
				case "(O)294":
				case "(O)295":
				case "(O)0":
				case "(O)313":
				case "(O)314":
				case "(O)315":
				case "(O)316":
				case "(O)317":
				case "(O)318":
				case "(O)319":
				case "(O)320":
				case "(O)321":
				case "(O)452":
				case "(O)674":
				case "(O)675":
				case "(O)676":
				case "(O)677":
				case "(O)678":
				case "(O)679":
				case "(O)750":
				case "(O)784":
				case "(O)785":
				case "(O)786":
				case "(O)792":
				case "(O)793":
				case "(O)794":
				case "(O)882":
				case "(O)883":
				case "(O)884":
					item3.Category = -999;
					if (item3 is Object obj5)
					{
						obj5.Type = "Litter";
					}
					break;
				case "(O)372":
					item3.Category = -4;
					if (item3 is Object obj4)
					{
						obj4.Type = "Fish";
					}
					break;
				}
				return true;
			});
			return true;
		case SaveFixes.MigrateHoneyItems:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (!(item3 is Object obj4) || obj4.QualifiedItemId != "(O)340")
				{
					return true;
				}
				obj4.preserve.Value = Object.PreserveType.Honey;
				if (obj4.preservedParentSheetIndex.Value == null || obj4.preservedParentSheetIndex.Value == "0")
				{
					string text = obj4.obsolete_honeyType;
					if (string.IsNullOrWhiteSpace(text) && obj4.name.EndsWith(" Honey"))
					{
						text = obj4.name.Substring(0, obj4.name.Length - " Honey".Length).Replace(" ", "");
					}
					switch (text)
					{
					case "Poppy":
						obj4.preservedParentSheetIndex.Value = "376";
						break;
					case "Tulip":
						obj4.preservedParentSheetIndex.Value = "591";
						break;
					case "SummerSpangle":
						obj4.preservedParentSheetIndex.Value = "593";
						break;
					case "FairyRose":
						obj4.preservedParentSheetIndex.Value = "595";
						break;
					case "BlueJazz":
						obj4.preservedParentSheetIndex.Value = "597";
						break;
					default:
						obj4.Name = "Wild Honey";
						obj4.preservedParentSheetIndex.Value = null;
						break;
					}
				}
				if (obj4.Name == "Honey" && obj4.preservedParentSheetIndex.Value == "-1")
				{
					obj4.Name = "Wild Honey";
				}
				obj4.obsolete_honeyType = null;
				return true;
			});
			return true;
		case SaveFixes.MigrateMachineLastOutputRule:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (item3 is Object machine)
				{
					SaveMigrator_1_6.InferMachineInputOutputFields(machine);
				}
				return true;
			});
			return true;
		case SaveFixes.StandardizeBundleFields:
			return true;
		case SaveFixes.MigrateAdventurerGoalFlags:
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["Gil_Slime Charmer Ring"] = "Gil_Slimes";
			dictionary["Gil_Slime Charmer Ring"] = "Gil_Slimes";
			dictionary["Gil_Savage Ring"] = "Gil_Shadows";
			dictionary["Gil_Vampire Ring"] = "Gil_Bats";
			dictionary["Gil_Skeleton Mask"] = "Gil_Skeletons";
			dictionary["Gil_Insect Head"] = "Gil_Insects";
			dictionary["Gil_Hard Hat"] = "Gil_Duggy";
			dictionary["Gil_Burglar's Ring"] = "Gil_DustSpirits";
			dictionary["Gil_Crabshell Ring"] = "Gil_Crabs";
			dictionary["Gil_Arcane Hat"] = "Gil_Mummies";
			dictionary["Gil_Knight's Helmet"] = "Gil_Dinos";
			dictionary["Gil_Napalm Ring"] = "Gil_Serpents";
			dictionary["Gil_Telephone"] = "Gil_FlameSpirits";
			Dictionary<string, string> map = dictionary;
			foreach (Farmer player4 in Game1.getAllFarmers())
			{
				NetStringHashSet[] array = new NetStringHashSet[2] { player4.mailReceived, player4.mailForTomorrow };
				foreach (NetStringHashSet mail in array)
				{
					foreach (KeyValuePair<string, string> pair in map)
					{
						if (mail.Remove(pair.Key))
						{
							mail.Add(pair.Value);
						}
					}
				}
				IList<string> mailbox = Game1.mailbox;
				for (int i3 = 0; i3 < mailbox.Count; i3++)
				{
					if (map.TryGetValue(mailbox[i3], out var newFlag))
					{
						mailbox[i3] = newFlag;
					}
				}
			}
			return true;
		}
		case SaveFixes.SetCropSeedId:
		{
			Dictionary<string, string> seedsByHarvestId = new Dictionary<string, string>();
			foreach (KeyValuePair<string, CropData> pair2 in Game1.cropData)
			{
				string seedId = pair2.Key;
				string harvestId = pair2.Value.HarvestItemId;
				if (harvestId != null)
				{
					seedsByHarvestId.TryAdd(harvestId, seedId);
				}
			}
			Utility.ForEachCrop(delegate(Crop crop)
			{
				if (crop.netSeedIndex.Value == "-1")
				{
					crop.netSeedIndex.Value = null;
				}
				if (!string.IsNullOrWhiteSpace(crop.netSeedIndex.Value))
				{
					return true;
				}
				if (crop.isWildSeedCrop() || crop.forageCrop.Value)
				{
					return true;
				}
				if (crop.indexOfHarvest.Value != null && seedsByHarvestId.TryGetValue(crop.indexOfHarvest.Value, out var value))
				{
					crop.netSeedIndex.Value = value;
				}
				return true;
			});
			return true;
		}
		case SaveFixes.FixMineBoulderCollisions:
		{
			Mine mine = Game1.RequireLocation<Mine>("Mine");
			Vector2 tile2 = mine.GetBoulderPosition();
			if (mine.objects.TryGetValue(tile2, out var boulder) && boulder.QualifiedItemId == "(BC)78" && boulder.TileLocation == Vector2.Zero)
			{
				boulder.TileLocation = tile2;
			}
			return true;
		}
		case SaveFixes.MigratePetAndPetBowlIds:
		{
			Pet pet = Game1.player.getPet();
			if (pet != null)
			{
				pet.petId.Value = Guid.NewGuid();
				PetBowl bowl = (PetBowl)Game1.getFarm().getBuildingByType("Pet Bowl");
				if (bowl != null)
				{
					bowl.AssignPet(pet);
					pet.setAtFarmPosition();
				}
			}
			return true;
		}
		case SaveFixes.MigrateHousePaint:
		{
			Farm farm3 = Game1.getFarm();
			if (farm3.housePaintColor.Value != null)
			{
				farm3.GetMainFarmHouse().netBuildingPaintColor.Value.CopyFrom(farm3.housePaintColor.Value);
				farm3.housePaintColor.Value = null;
			}
			return true;
		}
		case SaveFixes.MigrateItemIds:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (!(item3 is Boots boots))
				{
					if (!(item3 is MeleeWeapon meleeWeapon))
					{
						if (!(item3 is Fence fence))
						{
							if (!(item3 is Slingshot slingshot))
							{
								if (item3 is Torch && item3.itemId.Value != item3.ParentSheetIndex.ToString())
								{
									item3.itemId.Value = null;
								}
							}
							else
							{
								slingshot.ItemId = null;
							}
						}
						else if (fence.obsolete_whichType.HasValue)
						{
							item3.itemId.Value = null;
						}
					}
					else
					{
						meleeWeapon.appearance.Value = ((!string.IsNullOrWhiteSpace(meleeWeapon.appearance.Value) && meleeWeapon.appearance.Value != "-1") ? ItemRegistry.ManuallyQualifyItemId(meleeWeapon.appearance.Value, "(W)") : null);
					}
				}
				else if (boots.appliedBootSheetIndex.Value == "-1")
				{
					boots.appliedBootSheetIndex.Value = null;
				}
				_ = item3.ItemId;
				return true;
			});
			foreach (Farmer player6 in Game1.getAllFarmers())
			{
				NetStringIntArrayDictionary fishCaught = player6.fishCaught;
				if (fishCaught != null)
				{
					KeyValuePair<string, int[]>[] array3 = fishCaught.Pairs.ToArray();
					for (int num = 0; num < array3.Length; num++)
					{
						KeyValuePair<string, int[]> pair7 = array3[num];
						fishCaught.Remove(pair7.Key);
						fishCaught[ItemRegistry.ManuallyQualifyItemId(pair7.Key, "(O)")] = pair7.Value;
					}
				}
				if (player6.toolBeingUpgraded.Value != null)
				{
					switch (player6.toolBeingUpgraded.Value.InitialParentTileIndex)
					{
					case 13:
						player6.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)CopperTrashCan");
						break;
					case 14:
						player6.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)SteelTrashCan");
						break;
					case 15:
						player6.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)GoldTrashCan");
						break;
					case 16:
						player6.toolBeingUpgraded.Value = ItemRegistry.Create<Tool>("(T)IridiumTrashCan");
						break;
					}
				}
				if (!(player6.obsolete_isMale ?? player6.IsMale))
				{
					NetRef<Clothing>[] array4 = new NetRef<Clothing>[2] { player6.shirtItem, player6.pantsItem };
					foreach (NetRef<Clothing> field in array4)
					{
						Clothing clothing = field.Value;
						if (clothing == null)
						{
							continue;
						}
						if (clothing.obsolete_indexInTileSheetFemale > -1)
						{
							int variantId = clothing.obsolete_indexInTileSheetFemale.Value;
							if (clothing.HasTypeId("(S)"))
							{
								variantId += 1000;
							}
							ItemMetadata variantData = ItemRegistry.GetMetadata(clothing.TypeDefinitionId + variantId);
							if (variantData.Exists())
							{
								Clothing newClothing = (Clothing)variantData.CreateItemOrErrorItem();
								newClothing.clothesColor.Value = clothing.clothesColor.Value;
								newClothing.modData.CopyFrom(clothing.modData);
								field.Value = newClothing;
							}
						}
						clothing.obsolete_indexInTileSheetFemale = null;
					}
				}
				foreach (Quest rawQuest in player6.questLog)
				{
					if (!(rawQuest is CraftingQuest quest2))
					{
						if (!(rawQuest is FishingQuest quest3))
						{
							if (!(rawQuest is ItemDeliveryQuest quest4))
							{
								if (!(rawQuest is ItemHarvestQuest quest5))
								{
									if (!(rawQuest is LostItemQuest quest6))
									{
										if (!(rawQuest is ResourceCollectionQuest quest7))
										{
											if (rawQuest is SecretLostItemQuest quest8)
											{
												quest8.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest8.ItemId.Value, "(O)");
											}
										}
										else
										{
											quest7.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest7.ItemId.Value, "(O)");
										}
									}
									else
									{
										quest6.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest6.ItemId.Value, "(O)");
									}
								}
								else
								{
									quest5.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest5.ItemId.Value, "(O)");
								}
							}
							else
							{
								quest4.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest4.ItemId.Value, "(O)");
								if (quest4.dailyQuest.Value)
								{
									quest4.moneyReward.Value = quest4.GetGoldRewardPerItem(ItemRegistry.Create(quest4.ItemId.Value));
								}
							}
						}
						else
						{
							quest3.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest3.ItemId.Value, "(O)");
						}
					}
					else
					{
						quest2.ItemId.Value = ItemRegistry.ManuallyQualifyItemId(quest2.ItemId.Value, (quest2.obsolete_isBigCraftable == true) ? "(BC)" : "(O)");
						quest2.obsolete_isBigCraftable = null;
					}
				}
			}
			foreach (SpecialOrder order in Game1.player.team.specialOrders)
			{
				if (order.itemToRemoveOnEnd.Value == "-1")
				{
					order.itemToRemoveOnEnd.Value = null;
				}
			}
			Utility.ForEachLocation(delegate(GameLocation gameLocation)
			{
				if (gameLocation is IslandShrine islandShrine)
				{
					islandShrine.AddMissingPedestals();
				}
				foreach (KeyValuePair<Vector2, Object> pair9 in gameLocation.objects.Pairs)
				{
					if (pair9.Value is Fence fence && fence.obsolete_whichType.HasValue)
					{
						fence.ItemId = null;
					}
				}
				foreach (TerrainFeature value3 in gameLocation.terrainFeatures.Values)
				{
					if (value3 is FruitTree fruitTree)
					{
						if (fruitTree.obsolete_treeType != null)
						{
							switch (fruitTree.obsolete_treeType)
							{
							case "0":
								fruitTree.treeId.Value = "628";
								break;
							case "1":
								fruitTree.treeId.Value = "629";
								break;
							case "2":
								fruitTree.treeId.Value = "630";
								break;
							case "3":
								fruitTree.treeId.Value = "631";
								break;
							case "4":
								fruitTree.treeId.Value = "632";
								break;
							case "5":
								fruitTree.treeId.Value = "633";
								break;
							case "7":
								fruitTree.treeId.Value = "69";
								break;
							case "8":
								fruitTree.treeId.Value = "835";
								break;
							default:
								fruitTree.treeId.Value = fruitTree.obsolete_treeType;
								break;
							}
							fruitTree.obsolete_treeType = null;
						}
						if (fruitTree.obsolete_fruitsOnTree.HasValue)
						{
							bool isGreenhouse = fruitTree.Location.IsGreenhouse;
							try
							{
								fruitTree.Location.IsGreenhouse = true;
								for (int j = 0; j < fruitTree.obsolete_fruitsOnTree; j++)
								{
									fruitTree.TryAddFruit();
								}
							}
							finally
							{
								fruitTree.Location.IsGreenhouse = isGreenhouse;
							}
							fruitTree.obsolete_fruitsOnTree = null;
						}
					}
				}
				foreach (Building building in gameLocation.buildings)
				{
					if (building is FishPond fishPond && fishPond.fishType.Value == "-1")
					{
						fishPond.fishType.Value = null;
					}
				}
				foreach (FarmAnimal current3 in gameLocation.animals.Values)
				{
					if (current3.currentProduce.Value == "-1")
					{
						current3.currentProduce.Value = null;
						current3.ReloadTextureIfNeeded();
					}
				}
				return true;
			});
			return true;
		case SaveFixes.MigrateShedFloorWallIds:
			Utility.ForEachLocation(delegate(GameLocation gameLocation)
			{
				if (gameLocation is Shed shed)
				{
					if (shed.appliedFloor.TryGetValue("Floor_0", out var value))
					{
						shed.appliedFloor.Remove("Floor_0");
						shed.appliedFloor["Floor"] = value;
					}
					if (shed.appliedWallpaper.TryGetValue("Wall_0", out var value2))
					{
						shed.appliedWallpaper.Remove("Wall_0");
						shed.appliedWallpaper["Wall"] = value2;
					}
				}
				return true;
			});
			return true;
		case SaveFixes.RemoveMeatFromAnimalBundle:
		{
			if (Game1.netWorldState.Value.BundleData.TryGetValue("Pantry/4", out var rawData) && rawData.StartsWith("Animal/"))
			{
				string[] fields = rawData.Split('/');
				List<string> ingredients = ArgUtility.SplitBySpace(ArgUtility.Get(rawData.Split('/'), 2)).ToList();
				for (int i5 = 0; i5 < ingredients.Count; i5 += 3)
				{
					string id = ingredients[i5];
					switch (id)
					{
					case "639":
					case "640":
					case "641":
					case "642":
					case "643":
						if (ItemRegistry.ResolveMetadata("(O)" + id) == null)
						{
							ingredients.RemoveRange(i5, Math.Min(3, ingredients.Count - 1));
							i5 -= 3;
						}
						break;
					}
				}
				fields[2] = string.Join(" ", ingredients);
				Game1.netWorldState.Value.BundleData["Pantry/4"] = string.Join("/", fields);
				if (Game1.netWorldState.Value.Bundles.TryGetValue(4, out var values) && values.Length > ingredients.Count)
				{
					Array.Resize(ref values, ingredients.Count);
					Game1.netWorldState.Value.Bundles.Remove(4);
					Game1.netWorldState.Value.Bundles.Add(4, values);
				}
			}
			return true;
		}
		case SaveFixes.RemoveMasteryRoomFoliage:
		{
			GameLocation forest = Game1.getLocationFromName("Forest");
			if (forest != null)
			{
				forest.largeTerrainFeatures.RemoveWhere((LargeTerrainFeature feature) => feature.Tile == new Vector2(100f, 74f) || feature.Tile == new Vector2(101f, 76f));
				if (forest.terrainFeatures.GetValueOrDefault(new Vector2(98f, 75f)) is Tree t2 && t2.tapped.Value && forest.objects.TryGetValue(new Vector2(98f, 75f), out var o))
				{
					if (o.readyForHarvest.Value && o.heldObject != null)
					{
						Game1.player.team.returnedDonations.Add(o.heldObject.Value);
					}
					Game1.player.team.returnedDonations.Add(o);
					Game1.player.team.newLostAndFoundItems.Value = true;
				}
				forest.terrainFeatures.Remove(new Vector2(98f, 75f));
			}
			return true;
		}
		case SaveFixes.AddTownTrees:
		{
			GameLocation town = Game1.getLocationFromName("Town");
			Layer pathsLayer = town.map?.GetLayer("Paths");
			if (pathsLayer == null)
			{
				return false;
			}
			for (int x = 0; x < town.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < town.map.Layers[0].LayerHeight; y++)
				{
					Tile t = pathsLayer.Tiles[x, y];
					if (t == null)
					{
						continue;
					}
					Vector2 tile = new Vector2(x, y);
					if (town.TryGetTreeIdForTile(t, out var treeId, out var growthStageOnLoad, out var _, out var isFruitTree) && town.GetFurnitureAt(tile) == null && !town.terrainFeatures.ContainsKey(tile) && !town.objects.ContainsKey(tile))
					{
						if (isFruitTree)
						{
							town.terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnLoad ?? 4));
						}
						else
						{
							town.terrainFeatures.Add(tile, new Tree(treeId, growthStageOnLoad ?? 5));
						}
					}
				}
			}
			return true;
		}
		case SaveFixes.MapAdjustments_1_6:
		{
			Game1.getLocationFromName("BusStop").shiftContents(10, 0);
			List<Point> obj = new List<Point>
			{
				new Point(78, 17),
				new Point(79, 17),
				new Point(79, 18),
				new Point(80, 17),
				new Point(80, 18),
				new Point(80, 19),
				new Point(81, 16),
				new Point(81, 17),
				new Point(81, 18),
				new Point(81, 19),
				new Point(82, 15),
				new Point(82, 16),
				new Point(82, 17),
				new Point(82, 18),
				new Point(83, 13),
				new Point(83, 14),
				new Point(83, 15),
				new Point(83, 16),
				new Point(83, 17),
				new Point(84, 13),
				new Point(84, 14),
				new Point(84, 15),
				new Point(84, 16),
				new Point(84, 17),
				new Point(84, 18),
				new Point(85, 13),
				new Point(85, 14),
				new Point(85, 15),
				new Point(85, 16),
				new Point(85, 17),
				new Point(85, 18),
				new Point(86, 14),
				new Point(86, 15),
				new Point(86, 16),
				new Point(86, 17),
				new Point(86, 18),
				new Point(87, 14),
				new Point(87, 15),
				new Point(87, 16),
				new Point(87, 17),
				new Point(87, 18),
				new Point(87, 19),
				new Point(88, 13),
				new Point(88, 14),
				new Point(88, 15),
				new Point(88, 16),
				new Point(88, 17),
				new Point(88, 18),
				new Point(88, 19),
				new Point(89, 13),
				new Point(89, 14),
				new Point(89, 15),
				new Point(89, 16),
				new Point(89, 17),
				new Point(79, 21),
				new Point(79, 22),
				new Point(79, 23),
				new Point(79, 24),
				new Point(79, 25),
				new Point(76, 16),
				new Point(75, 16),
				new Point(74, 16)
			};
			GameLocation mountain2 = Game1.getLocationFromName("Mountain");
			foreach (Point p in obj)
			{
				mountain2.cleanUpTileForMapOverride(p);
			}
			mountain2.terrainFeatures.Remove(new Vector2(79f, 20f));
			mountain2.terrainFeatures.Remove(new Vector2(79f, 19f));
			mountain2.terrainFeatures.Remove(new Vector2(79f, 16f));
			mountain2.terrainFeatures.Remove(new Vector2(80f, 20f));
			mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(82, 11));
			mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(86, 13));
			mountain2.largeTerrainFeatures.Remove(mountain2.getLargeTerrainFeatureAt(85, 16));
			mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(81f, 9f), 1, mountain2));
			mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(84f, 18f), 2, mountain2));
			mountain2.largeTerrainFeatures.Add(new Bush(new Vector2(87f, 19f), 1, mountain2));
			List<Point> obj2 = new List<Point>
			{
				new Point(92, 10),
				new Point(93, 10),
				new Point(94, 10),
				new Point(93, 13),
				new Point(95, 13),
				new Point(92, 5),
				new Point(92, 6),
				new Point(97, 9),
				new Point(91, 10),
				new Point(91, 9),
				new Point(91, 8),
				new Point(93, 11),
				new Point(94, 11),
				new Point(95, 11)
			};
			GameLocation town2 = Game1.getLocationFromName("Town");
			foreach (Point p2 in obj2)
			{
				town2.cleanUpTileForMapOverride(p2);
			}
			town2.loadPathsLayerObjectsInArea(103, 16, 16, 27);
			town2.loadPathsLayerObjectsInArea(120, 57, 7, 12);
			town2.largeTerrainFeatures.Remove(town2.getLargeTerrainFeatureAt(105, 42));
			town2.largeTerrainFeatures.Remove(town2.getLargeTerrainFeatureAt(108, 42));
			List<Point> obj3 = new List<Point>
			{
				new Point(63, 77),
				new Point(63, 78),
				new Point(63, 79),
				new Point(63, 80),
				new Point(46, 26),
				new Point(46, 27),
				new Point(46, 28),
				new Point(46, 29)
			};
			GameLocation forest2 = Game1.getLocationFromName("Forest");
			foreach (Point p3 in obj3)
			{
				forest2.cleanUpTileForMapOverride(p3);
			}
			forest2.largeTerrainFeatures.Add(new Bush(new Vector2(54f, 8f), 0, forest2));
			forest2.largeTerrainFeatures.Add(new Bush(new Vector2(58f, 8f), 0, forest2));
			return true;
		}
		case SaveFixes.MigrateWalletItems:
		{
			Farmer player7 = Game1.MasterPlayer;
			player7.hasRustyKey = player7.hasRustyKey || (player7.obsolete_hasRustyKey ?? false);
			player7.hasSkullKey = player7.hasSkullKey || (player7.obsolete_hasSkullKey ?? false);
			player7.canUnderstandDwarves = player7.canUnderstandDwarves || (player7.obsolete_canUnderstandDwarves ?? false);
			player7.obsolete_hasRustyKey = null;
			player7.obsolete_hasSkullKey = null;
			player7.obsolete_canUnderstandDwarves = null;
			foreach (Farmer player8 in Game1.getAllFarmers())
			{
				player8.hasClubCard = player8.hasClubCard || (player8.obsolete_hasClubCard ?? false);
				player8.hasDarkTalisman = player8.hasDarkTalisman || (player8.obsolete_hasDarkTalisman ?? false);
				player8.hasMagicInk = player8.hasMagicInk || (player8.obsolete_hasMagicInk ?? false);
				player8.hasMagnifyingGlass = player8.hasMagnifyingGlass || (player8.obsolete_hasMagnifyingGlass ?? false);
				player8.hasSpecialCharm = player8.hasSpecialCharm || (player8.obsolete_hasSpecialCharm ?? false);
				player8.HasTownKey = player8.HasTownKey || (player8.obsolete_hasTownKey ?? false);
				player8.hasUnlockedSkullDoor = player8.hasUnlockedSkullDoor || (player8.obsolete_hasUnlockedSkullDoor ?? false);
				player8.obsolete_hasClubCard = null;
				player8.obsolete_hasDarkTalisman = null;
				player8.obsolete_hasMagicInk = null;
				player8.obsolete_hasMagnifyingGlass = null;
				player8.obsolete_hasSpecialCharm = null;
				player8.obsolete_hasTownKey = null;
				player8.obsolete_hasUnlockedSkullDoor = null;
				player8.obsolete_daysMarried = null;
			}
			return true;
		}
		case SaveFixes.MigrateResourceClumps:
			Utility.ForEachLocation(delegate(GameLocation gameLocation)
			{
				if (!(gameLocation is Forest forest3))
				{
					if (gameLocation is Woods woods)
					{
						woods.DayUpdate(Game1.dayOfMonth);
					}
				}
				else if (forest3.obsolete_log != null)
				{
					forest3.resourceClumps.Add(forest3.obsolete_log);
					forest3.obsolete_log = null;
				}
				return true;
			}, includeInteriors: false);
			return true;
		case SaveFixes.MigrateFishingRodAttachmentSlots:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (item3 is FishingRod fishingRod)
				{
					ToolData toolData = fishingRod.GetToolData();
					if (toolData == null || toolData.AttachmentSlots < 0 || fishingRod.AttachmentSlotsCount <= toolData.AttachmentSlots)
					{
						return true;
					}
					INetSerializable parent = fishingRod.attachments.Parent;
					fishingRod.attachments.Parent = null;
					try
					{
						int num2 = fishingRod.AttachmentSlotsCount - 1;
						while (fishingRod.AttachmentSlotsCount > toolData.AttachmentSlots && num2 >= 0)
						{
							if (fishingRod.attachments.Count <= num2)
							{
								fishingRod.AttachmentSlotsCount--;
							}
							else if (fishingRod.attachments[num2] == null)
							{
								fishingRod.AttachmentSlotsCount--;
							}
							num2--;
						}
					}
					finally
					{
						fishingRod.attachments.Parent = parent;
					}
				}
				return true;
			});
			return true;
		case SaveFixes.MoveSlimeHutches:
		{
			Farm farm = Game1.getFarm();
			for (int i4 = farm.buildings.Count - 1; i4 >= 0; i4--)
			{
				if (farm.buildings[i4].buildingType.Value == "Slime Hutch")
				{
					farm.buildings[i4].tileX.Value += 2;
					farm.buildings[i4].tileY.Value += 2;
					farm.buildings[i4].ReloadBuildingData();
					farm.buildings[i4].updateInteriorWarps();
				}
			}
			return true;
		}
		case SaveFixes.AddLocationsVisited:
			foreach (Farmer who in Game1.getAllFarmers())
			{
				NetStringHashSet visited = who.locationsVisited;
				Farmer mainPlayer = Game1.MasterPlayer;
				visited.AddRange(new string[30]
				{
					"Farm", "FarmHouse", "FarmCave", "Cellar", "Town", "JoshHouse", "HaleyHouse", "SamHouse", "Blacksmith", "ManorHouse",
					"SeedShop", "Saloon", "Trailer", "Hospital", "HarveyRoom", "ArchaeologyHouse", "JojaMart", "Beach", "ElliottHouse", "FishShop",
					"Mountain", "ScienceHouse", "SebastianRoom", "Tent", "Forest", "AnimalShop", "LeahHouse", "Backwoods", "BusStop", "Tunnel"
				});
				if (mainPlayer.mailReceived.Contains("ccPantry"))
				{
					visited.Add("Greenhouse");
				}
				if (Game1.isLocationAccessible("CommunityCenter"))
				{
					visited.Add("CommunityCenter");
				}
				if (who.eventsSeen.Contains("100162"))
				{
					visited.Add("Mine");
				}
				if (mainPlayer.mailReceived.Contains("ccVault"))
				{
					visited.AddRange(new string[2] { "Desert", "SkullCave" });
				}
				if (who.eventsSeen.Contains("67"))
				{
					visited.Add("SandyHouse");
				}
				if (mainPlayer.mailReceived.Contains("bouncerGone"))
				{
					visited.Add("Club");
				}
				if (Game1.isLocationAccessible("Railroad"))
				{
					visited.AddRange(new string[4]
					{
						"Railroad",
						"BathHouse_Entry",
						who.IsMale ? "BathHouse_MensLocker" : "BathHouse_WomensLocker",
						"BathHouse_Pool"
					});
				}
				if (mainPlayer.mailReceived.Contains("Farm_Eternal"))
				{
					visited.Add("Summit");
				}
				if (mainPlayer.mailReceived.Contains("witchStatueGone"))
				{
					visited.AddRange(new string[2] { "WitchSwamp", "WitchWarpCave" });
				}
				if (mainPlayer.mailReceived.Contains("henchmanGone"))
				{
					visited.Add("WitchHut");
				}
				if (who.mailReceived.Contains("beenToWoods"))
				{
					visited.Add("Woods");
				}
				if (Forest.isWizardHouseUnlocked())
				{
					visited.Add("WizardHouse");
					if (who.getFriendshipHeartLevelForNPC("Wizard") >= 4)
					{
						visited.Add("WizardHouseBasement");
					}
				}
				if (who.mailReceived.Add("guildMember"))
				{
					visited.Add("AdventureGuild");
				}
				if (who.mailReceived.Contains("OpenedSewer"))
				{
					visited.Add("Sewer");
				}
				if (who.mailReceived.Contains("krobusUnseal"))
				{
					visited.Add("BugLand");
				}
				if (mainPlayer.mailReceived.Contains("abandonedJojaMartAccessible"))
				{
					visited.Add("AbandonedJojaMart");
				}
				if (mainPlayer.mailReceived.Contains("ccMovieTheater"))
				{
					visited.Add("MovieTheater");
				}
				if (mainPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					visited.Add("Trailer_Big");
				}
				if (who.getFriendshipHeartLevelForNPC("Caroline") >= 2)
				{
					visited.Add("Sunroom");
				}
				if (Game1.year > 1 || (Game1.season == Season.Winter && Game1.dayOfMonth >= 15))
				{
					visited.AddRange(new string[3] { "BeachNightMarket", "MermaidHouse", "Submarine" });
				}
				if (who.mailReceived.Contains("willyBackRoomInvitation"))
				{
					visited.Add("BoatTunnel");
				}
				if (who.mailReceived.Contains("Visited_Island"))
				{
					visited.AddRange(new string[4] { "IslandSouth", "IslandEast", "IslandHut", "IslandShrine" });
					if (mainPlayer.mailReceived.Contains("Island_FirstParrot"))
					{
						visited.AddRange(new string[2] { "IslandNorth", "IslandFieldOffice" });
					}
					if (mainPlayer.mailReceived.Contains("islandNorthCaveOpened"))
					{
						visited.Add("IslandNorthCave1");
					}
					if (mainPlayer.mailReceived.Contains("reachedCaldera"))
					{
						visited.Add("Caldera");
					}
					if (mainPlayer.mailReceived.Contains("Island_Turtle"))
					{
						visited.AddRange(new string[2] { "IslandWest", "IslandWestCave1" });
					}
					if (mainPlayer.mailReceived.Contains("Island_UpgradeHouse"))
					{
						visited.AddRange(new string[2] { "IslandFarmHouse", "IslandFarmCave" });
					}
					if (mainPlayer.team.collectedNutTracker.Contains("Bush_CaptainRoom_2_4"))
					{
						visited.Add("CaptainRoom");
					}
					if (IslandWest.IsQiWalnutRoomDoorUnlocked(out var _))
					{
						visited.Add("QiNutRoom");
					}
					if (mainPlayer.mailReceived.Contains("Island_Resort"))
					{
						visited.AddRange(new string[2] { "IslandSouthEast", "IslandSouthEastCave" });
					}
				}
				if (mainPlayer.mailReceived.Contains("leoMoved"))
				{
					visited.Add("LeoTreeHouse");
				}
			}
			return true;
		case SaveFixes.MarkStarterGiftBoxes:
			Utility.ForEachLocation(delegate(GameLocation gameLocation)
			{
				if (gameLocation is FarmHouse)
				{
					foreach (Object value4 in gameLocation.objects.Values)
					{
						if (value4 is Chest chest && chest.giftbox.Value && !chest.playerChest.Value)
						{
							chest.giftboxIsStarterGift.Value = true;
						}
					}
				}
				return true;
			});
			return true;
		case SaveFixes.MigrateMailEventsToTriggerActions:
		{
			Dictionary<string, string> migrateFromEvents = new Dictionary<string, string>
			{
				["2346097"] = "Mail_Abigail_8heart",
				["2346096"] = "Mail_Penny_10heart",
				["2346095"] = "Mail_Elliott_8heart",
				["2346094"] = "Mail_Elliott_10heart",
				["3333094"] = "Mail_Pierre_ExtendedHours",
				["2346093"] = "Mail_Harvey_10heart",
				["2346092"] = "Mail_Sam_10heart",
				["2346091"] = "Mail_Alex_10heart",
				["68"] = "Mail_Mom_5K",
				["69"] = "Mail_Mom_15K",
				["70"] = "Mail_Mom_32K",
				["71"] = "Mail_Mom_120K",
				["72"] = "Mail_Dad_5K",
				["73"] = "Mail_Dad_15K",
				["74"] = "Mail_Dad_32K",
				["75"] = "Mail_Dad_120K",
				["76"] = "Mail_Tribune_UpAndComing",
				["706"] = "Mail_Pierre_Fertilizers",
				["707"] = "Mail_Pierre_FertilizersHighQuality",
				["909"] = "Mail_Robin_Woodchipper",
				["3872126"] = "Mail_Willy_BackRoomUnlocked"
			};
			Dictionary<string, string> duplicateFromEvents = new Dictionary<string, string>
			{
				["2111194"] = "Mail_Emily_8heart",
				["2111294"] = "Mail_Emily_10heart",
				["3912126"] = "Mail_Elliott_Tour1",
				["3912127"] = "Mail_Elliott_Tour2",
				["3912128"] = "Mail_Elliott_Tour3",
				["3912129"] = "Mail_Elliott_Tour4",
				["3912130"] = "Mail_Elliott_Tour5",
				["3912131"] = "Mail_Elliott_Tour6"
			};
			foreach (Farmer allFarmer2 in Game1.getAllFarmers())
			{
				NetStringHashSet events = allFarmer2.eventsSeen;
				NetStringHashSet actions = allFarmer2.triggerActionsRun;
				foreach (KeyValuePair<string, string> pair5 in migrateFromEvents)
				{
					if (events.Remove(pair5.Key))
					{
						actions.Add(pair5.Value);
					}
				}
				foreach (KeyValuePair<string, string> pair6 in duplicateFromEvents)
				{
					if (events.Contains(pair6.Key))
					{
						actions.Add(pair6.Value);
					}
				}
			}
			return true;
		}
		case SaveFixes.ShiftFarmHouseFurnitureForExpansion:
			Utility.ForEachLocation(delegate(GameLocation gameLocation)
			{
				FarmHouse house2 = gameLocation as FarmHouse;
				if (house2 != null && house2.upgradeLevel >= 2)
				{
					house2.shiftContents(15, 10, delegate(Vector2 vector2, object entity)
					{
						if (entity is BedFurniture)
						{
							int xTile = (int)vector2.X;
							int yTile = (int)vector2.Y;
							if (house2.doesTileHaveProperty(xTile, yTile, "DefaultBedPosition", "Back") == null)
							{
								return house2.doesTileHaveProperty(xTile, yTile, "DefaultChildBedPosition", "Back") == null;
							}
							return false;
						}
						if (entity is Furniture { QualifiedItemId: "(F)1792" })
						{
							Vector2 vector3 = vector2 - Utility.PointToVector2(house2.getFireplacePoint());
							if (!(Math.Abs(vector3.X) > 1E-05f))
							{
								return Math.Abs(vector3.Y) > 1E-05f;
							}
							return true;
						}
						return true;
					});
					foreach (NPC current3 in house2.characters)
					{
						if (!current3.TilePoint.Equals(house2.getKitchenStandingSpot()))
						{
							current3.Position += new Vector2(15f, 10f) * 64f;
						}
						if (house2.hasTileAt(current3.TilePoint, "Buildings") || !house2.hasTileAt(current3.TilePoint, "Back"))
						{
							Vector2 vector = Utility.recursiveFindOpenTileForCharacter(current3, house2, Utility.PointToVector2(house2.getKitchenStandingSpot()), 99, allowOffMap: false);
							if (vector != Vector2.Zero)
							{
								current3.setTileLocation(vector);
							}
							else
							{
								current3.setTileLocation(Utility.PointToVector2(house2.getKitchenStandingSpot()));
							}
						}
					}
				}
				return true;
			});
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.currentLocation is FarmHouse { upgradeLevel: >=2 })
				{
					f.Position += new Vector2(15f, 10f) * 64f;
				}
			}
			return true;
		case SaveFixes.MigratePreservesTo16:
		{
			ObjectDataDefinition objTypeDefinition = ItemRegistry.GetObjectTypeDefinition();
			Utility.ForEachItemContext(HandleItem);
			return true;
		}
		case SaveFixes.MigrateQuestDataTo16:
		{
			Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(LegacyDescriptionElement), new Type[3]
			{
				typeof(DescriptionElement),
				typeof(Character),
				typeof(Item)
			}));
			foreach (Farmer allFarmer3 in Game1.getAllFarmers())
			{
				foreach (Quest quest9 in allFarmer3.questLog)
				{
					FieldInfo[] fields2 = quest9.GetType().GetFields();
					foreach (FieldInfo field2 in fields2)
					{
						if (field2.FieldType == typeof(NetDescriptionElementList))
						{
							NetDescriptionElementList fieldValue = (NetDescriptionElementList)field2.GetValue(quest9);
							if (fieldValue == null)
							{
								continue;
							}
							foreach (DescriptionElement entry in fieldValue)
							{
								SaveMigrator_1_6.MigrateLegacyDescriptionElement(serializer, entry);
							}
						}
						else if (field2.FieldType == typeof(NetDescriptionElementRef))
						{
							SaveMigrator_1_6.MigrateLegacyDescriptionElement(serializer, ((NetDescriptionElementRef)field2.GetValue(quest9))?.Value);
						}
					}
				}
			}
			return true;
		}
		case SaveFixes.SetBushesInPots:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (item3 is IndoorPot indoorPot && indoorPot.bush.Value != null)
				{
					indoorPot.bush.Value.inPot.Value = true;
				}
				return true;
			});
			return true;
		case SaveFixes.FixItemsNotMarkedAsInInventory:
			foreach (Farmer farmer2 in Game1.getAllFarmers())
			{
				foreach (Item equippedItem in farmer2.GetEquippedItems())
				{
					equippedItem.HasBeenInInventory = true;
				}
				foreach (Item item2 in farmer2.Items)
				{
					if (item2 != null)
					{
						item2.HasBeenInInventory = true;
					}
				}
			}
			return true;
		case SaveFixes.BetaFixesFor16:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (item3 is Boots || item3 is Clothing || item3 is Hat)
				{
					item3.FixStackSize();
				}
				return true;
			});
			return true;
		case SaveFixes.FixBasicWines:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (item3.ParentSheetIndex == 348 && item3.QualifiedItemId.Equals("(O)348"))
				{
					item3.ParentSheetIndex = 123;
				}
				return true;
			});
			return true;
		case SaveFixes.ResetForges_1_6:
			SaveMigrator_1_5.ResetForges();
			return true;
		case SaveFixes.RestoreAncientSeedRecipe_1_6:
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.mailReceived.Contains("museumCollectedRewardO_499_1"))
				{
					farmer.craftingRecipes.TryAdd("Ancient Seeds", 0);
				}
			}
			return true;
		case SaveFixes.FixInstancedInterior:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building.GetIndoorsType() == IndoorsType.Instanced)
				{
					GameLocation indoors = building.GetIndoors();
					if (indoors.uniqueName.Value == null)
					{
						indoors.uniqueName.Value = (building.GetData()?.IndoorMap ?? indoors.Name) + GuidHelper.NewGuid();
					}
					if (indoors is AnimalHouse animalHouse)
					{
						animalHouse.animalsThatLiveHere.RemoveWhere((long id2) => Utility.getAnimal(id2)?.home != building);
					}
				}
				return true;
			});
			return true;
		case SaveFixes.FixNonInstancedInterior:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building.GetIndoorsType() == IndoorsType.Global)
				{
					building.GetIndoors().uniqueName.Value = null;
				}
				return true;
			});
			return true;
		case SaveFixes.PopulateConstructedBuildings:
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (!string.IsNullOrWhiteSpace(building.buildingType.Value))
				{
					if (!building.isUnderConstruction(ignoreUpgrades: false))
					{
						Game1.player.team.constructedBuildings.Add(building.buildingType.Value);
					}
					BuildingData data = building.GetData();
					while (!string.IsNullOrWhiteSpace(data?.BuildingToUpgrade))
					{
						Game1.player.team.constructedBuildings.Add(data.BuildingToUpgrade);
						Building.TryGetData(data.BuildingToUpgrade, out data);
					}
				}
				return true;
			}, ignoreUnderConstruction: false);
			return true;
		case SaveFixes.FixRacoonQuestCompletion:
			if (NetWorldState.checkAnywhereForWorldStateID("forestStumpFixed"))
			{
				Game1.player.removeQuest("134");
				foreach (Farmer offlineFarmhand in Game1.getOfflineFarmhands())
				{
					offlineFarmhand.removeQuest("134");
				}
			}
			return true;
		case SaveFixes.RestoreDwarvish:
			if (Game1.player.hasOrWillReceiveMail("museumCollectedRewardO_326_1"))
			{
				Game1.player.canUnderstandDwarves = true;
			}
			return true;
		case SaveFixes.FixTubOFlowers:
			Utility.ForEachItem(delegate(Item item3)
			{
				if (item3.QualifiedItemId == "(BC)109")
				{
					item3.ItemId = "108";
					item3.ResetParentSheetIndex();
					if (item3 is Object obj4 && (obj4.Location?.IsOutdoors ?? false))
					{
						Season season = obj4.Location.GetSeason();
						if (season == Season.Winter || season == Season.Fall)
						{
							item3.ParentSheetIndex = 109;
						}
					}
				}
				return true;
			});
			return true;
		case SaveFixes.MigrateStatFields:
			foreach (Farmer player5 in Game1.getAllFarmers())
			{
				Stats stats = player5.stats;
				SerializableDictionary<string, uint> obsolete_stat_dictionary = stats.obsolete_stat_dictionary;
				if (obsolete_stat_dictionary != null && obsolete_stat_dictionary.Count > 0)
				{
					foreach (KeyValuePair<string, uint> pair3 in stats.obsolete_stat_dictionary)
					{
						stats.Values[pair3.Key] = (stats.Values.TryGetValue(pair3.Key, out var prevValue) ? (prevValue + pair3.Value) : pair3.Value);
					}
					stats.obsolete_stat_dictionary = null;
				}
				if (stats.Values.TryGetValue("walnutsFound", out var walnutsFound))
				{
					Game1.netWorldState.Value.GoldenWalnutsFound += (int)walnutsFound;
					stats.Values.Remove("walnutsFound");
				}
				KeyValuePair<string, uint>[] array2 = stats.Values.ToArray();
				for (int num = 0; num < array2.Length; num++)
				{
					KeyValuePair<string, uint> pair4 = array2[num];
					if (pair4.Value == 0)
					{
						stats.Values.Remove(pair4.Key);
					}
				}
				if (stats.AverageBedtime == 0)
				{
					stats.Set("averageBedtime", stats.obsolete_averageBedtime.GetValueOrDefault());
				}
				stats.obsolete_averageBedtime = null;
				stats.obsolete_beveragesMade = MergeStats("beveragesMade", stats.obsolete_beveragesMade);
				stats.obsolete_caveCarrotsFound = MergeStats("caveCarrotsFound", stats.obsolete_caveCarrotsFound);
				stats.obsolete_cheeseMade = MergeStats("cheeseMade", stats.obsolete_cheeseMade);
				stats.obsolete_chickenEggsLayed = MergeStats("chickenEggsLayed", stats.obsolete_chickenEggsLayed);
				stats.obsolete_copperFound = MergeStats("copperFound", stats.obsolete_copperFound);
				stats.obsolete_cowMilkProduced = MergeStats("cowMilkProduced", stats.obsolete_cowMilkProduced);
				stats.obsolete_cropsShipped = MergeStats("cropsShipped", stats.obsolete_cropsShipped);
				stats.obsolete_daysPlayed = MergeStats("daysPlayed", stats.obsolete_daysPlayed);
				stats.obsolete_diamondsFound = MergeStats("diamondsFound", stats.obsolete_diamondsFound);
				stats.obsolete_dirtHoed = MergeStats("dirtHoed", stats.obsolete_dirtHoed);
				stats.obsolete_duckEggsLayed = MergeStats("duckEggsLayed", stats.obsolete_duckEggsLayed);
				stats.obsolete_fishCaught = MergeStats("fishCaught", stats.obsolete_fishCaught);
				stats.obsolete_geodesCracked = MergeStats("geodesCracked", stats.obsolete_geodesCracked);
				stats.obsolete_giftsGiven = MergeStats("giftsGiven", stats.obsolete_giftsGiven);
				stats.obsolete_goatCheeseMade = MergeStats("goatCheeseMade", stats.obsolete_goatCheeseMade);
				stats.obsolete_goatMilkProduced = MergeStats("goatMilkProduced", stats.obsolete_goatMilkProduced);
				stats.obsolete_goldFound = MergeStats("goldFound", stats.obsolete_goldFound);
				stats.obsolete_goodFriends = MergeStats("goodFriends", stats.obsolete_goodFriends);
				stats.obsolete_individualMoneyEarned = MergeStats("individualMoneyEarned", stats.obsolete_individualMoneyEarned);
				stats.obsolete_iridiumFound = MergeStats("iridiumFound", stats.obsolete_iridiumFound);
				stats.obsolete_ironFound = MergeStats("ironFound", stats.obsolete_ironFound);
				stats.obsolete_itemsCooked = MergeStats("itemsCooked", stats.obsolete_itemsCooked);
				stats.obsolete_itemsCrafted = MergeStats("itemsCrafted", stats.obsolete_itemsCrafted);
				stats.obsolete_itemsForaged = MergeStats("itemsForaged", stats.obsolete_itemsForaged);
				stats.obsolete_itemsShipped = MergeStats("itemsShipped", stats.obsolete_itemsShipped);
				stats.obsolete_monstersKilled = MergeStats("monstersKilled", stats.obsolete_monstersKilled);
				stats.obsolete_mysticStonesCrushed = MergeStats("mysticStonesCrushed", stats.obsolete_mysticStonesCrushed);
				stats.obsolete_notesFound = MergeStats("notesFound", stats.obsolete_notesFound);
				stats.obsolete_otherPreciousGemsFound = MergeStats("otherPreciousGemsFound", stats.obsolete_otherPreciousGemsFound);
				stats.obsolete_piecesOfTrashRecycled = MergeStats("piecesOfTrashRecycled", stats.obsolete_piecesOfTrashRecycled);
				stats.obsolete_preservesMade = MergeStats("preservesMade", stats.obsolete_preservesMade);
				stats.obsolete_prismaticShardsFound = MergeStats("prismaticShardsFound", stats.obsolete_prismaticShardsFound);
				stats.obsolete_questsCompleted = MergeStats("questsCompleted", stats.obsolete_questsCompleted);
				stats.obsolete_rabbitWoolProduced = MergeStats("rabbitWoolProduced", stats.obsolete_rabbitWoolProduced);
				stats.obsolete_rocksCrushed = MergeStats("rocksCrushed", stats.obsolete_rocksCrushed);
				stats.obsolete_sheepWoolProduced = MergeStats("sheepWoolProduced", stats.obsolete_sheepWoolProduced);
				stats.obsolete_slimesKilled = MergeStats("slimesKilled", stats.obsolete_slimesKilled);
				stats.obsolete_stepsTaken = MergeStats("stepsTaken", stats.obsolete_stepsTaken);
				stats.obsolete_stoneGathered = MergeStats("stoneGathered", stats.obsolete_stoneGathered);
				stats.obsolete_stumpsChopped = MergeStats("stumpsChopped", stats.obsolete_stumpsChopped);
				stats.obsolete_timesFished = MergeStats("timesFished", stats.obsolete_timesFished);
				stats.obsolete_timesUnconscious = MergeStats("timesUnconscious", stats.obsolete_timesUnconscious);
				stats.obsolete_totalMoneyGifted = MergeStats("totalMoneyGifted", stats.obsolete_totalMoneyGifted);
				stats.obsolete_trufflesFound = MergeStats("trufflesFound", stats.obsolete_trufflesFound);
				stats.obsolete_weedsEliminated = MergeStats("weedsEliminated", stats.obsolete_weedsEliminated);
				stats.obsolete_seedsSown = MergeStats("seedsSown", stats.obsolete_seedsSown);
				uint? MergeStats(string newKey, uint? oldValue)
				{
					stats.Increment(newKey, oldValue.GetValueOrDefault());
					return null;
				}
			}
			return true;
		case SaveFixes.MakeWildSeedsDeterministic:
			Utility.ForEachCrop(delegate(Crop crop)
			{
				if (crop.isWildSeedCrop())
				{
					crop.replaceWithObjectOnFullGrown.Value = crop.getRandomWildCropForSeason(onlyDeterministic: true);
				}
				return true;
			});
			return true;
		case SaveFixes.FixTranslatedInternalNames:
			Utility.ForEachItem(delegate(Item item3)
			{
				switch (item3.QualifiedItemId)
				{
				case "(H)15":
				case "(H)17":
				case "(H)18":
				case "(H)23":
				case "(H)28":
				case "(H)35":
				case "(H)41":
				case "(H)50":
				case "(H)51":
				case "(H)82":
				case "(H)90":
				case "(O)804":
				case "(H)AbigailsBow":
				case "(H)GilsHat":
				case "(H)GovernorsHat":
					if (item3.Name.Contains('’'))
					{
						item3.Name = ItemRegistry.GetData(item3.QualifiedItemId)?.InternalName ?? item3.Name;
					}
					break;
				case "(H)GoldPanHat":
					if (item3.Name == "Steel Pan")
					{
						item3.Name = ItemRegistry.GetData(item3.QualifiedItemId)?.InternalName ?? item3.Name;
					}
					break;
				}
				return true;
			});
			return true;
		case SaveFixes.ConvertBuildingQuests:
			foreach (Farmer player3 in Game1.getAllFarmers())
			{
				for (int i2 = 0; i2 < player3.questLog.Count; i2++)
				{
					Quest quest = player3.questLog[i2];
					if (quest.questType.Value == 8)
					{
						player3.questLog[i2] = new HaveBuildingQuest(quest.obsolete_completionString);
					}
				}
			}
			return true;
		case SaveFixes.AddJunimoKartAndPrairieKingStats:
			foreach (Farmer player2 in Game1.getAllFarmers())
			{
				if (player2.hasOrWillReceiveMail("JunimoKart"))
				{
					player2.stats.Increment("completedJunimoKart", 1);
				}
				if (player2.hasOrWillReceiveMail("Beat_PK"))
				{
					player2.stats.Increment("completedPrairieKing", 1);
				}
			}
			return true;
		case SaveFixes.FixEmptyLostAndFoundItemStacks:
			foreach (Item item in Game1.player.team.returnedDonations)
			{
				if (item != null && item.Stack < 1)
				{
					item.Stack = 1;
				}
			}
			return true;
		case SaveFixes.FixDuplicateMissedMail:
		{
			HashSet<string> mailboxSet = new HashSet<string>();
			List<int> indicesToRemove = new List<int>();
			foreach (Farmer player in Game1.getAllFarmers())
			{
				mailboxSet.Clear();
				indicesToRemove.Clear();
				for (int i = 0; i < player.mailbox.Count; i++)
				{
					string mailKey = player.mailbox[i];
					if (!mailboxSet.Add(mailKey))
					{
						switch (mailKey)
						{
						case "robinKitchenLetter":
						case "marnieAutoGrabber":
						case "JunimoKart":
						case "Beat_PK":
							indicesToRemove.Add(i);
							break;
						}
					}
				}
				indicesToRemove.Reverse();
				foreach (int indexToRemove in indicesToRemove)
				{
					player.mailbox.RemoveAt(indexToRemove);
				}
			}
			return true;
		}
		default:
			return false;
		}
	}

	/// <summary>Convert individually implemented buildings that were saved before Stardew Valley 1.6 to the new Data/BuildingsData format.</summary>
	/// <param name="location">The location whose buildings to convert.</param>
	public static void ConvertBuildingsToData(GameLocation location)
	{
		for (int i = location.buildings.Count - 1; i >= 0; i--)
		{
			Building building = location.buildings[i];
			GameLocation indoors = building.GetIndoors();
			if (indoors != null)
			{
				SaveMigrator_1_6.ConvertBuildingsToData(indoors);
			}
			switch (building.buildingType.Value)
			{
			case "Log Cabin":
			case "Plank Cabin":
			case "Stone Cabin":
				building.skinId.Value = building.buildingType.Value;
				building.buildingType.Value = "Cabin";
				building.ReloadBuildingData();
				building.updateInteriorWarps();
				break;
			}
			string expectedType = building.GetData()?.BuildingType;
			if (expectedType != null && expectedType != building.GetType().FullName)
			{
				Building newBuilding = Building.CreateInstanceFromId(building.buildingType.Value, new Vector2(building.tileX.Value, building.tileY.Value));
				if (newBuilding != null)
				{
					newBuilding.indoors.Value = building.indoors.Value;
					newBuilding.buildingType.Value = building.buildingType.Value;
					newBuilding.tileX.Value = building.tileX.Value;
					newBuilding.tileY.Value = building.tileY.Value;
					location.buildings.RemoveAt(i);
					location.buildings.Add(newBuilding);
					SaveMigrator_1_6.TransferValuesToDataBuilding(building, newBuilding);
				}
			}
		}
	}

	/// <summary>Copy values from an older pre-1.6 building to a new data-driven <see cref="T:StardewValley.Buildings.Building" /> instance.</summary>
	/// <param name="oldBuilding">The pre-1.6 building instance.</param>
	/// <param name="newBuilding">The new data-driven building instance that will replace <paramref name="oldBuilding" />.</param>
	public static void TransferValuesToDataBuilding(Building oldBuilding, Building newBuilding)
	{
		newBuilding.animalDoorOpen.Value = oldBuilding.animalDoorOpen.Value;
		newBuilding.animalDoorOpenAmount.Value = oldBuilding.animalDoorOpenAmount.Value;
		newBuilding.netBuildingPaintColor.Value.CopyFrom(oldBuilding.netBuildingPaintColor.Value);
		newBuilding.modData.CopyFrom(oldBuilding.modData.Pairs);
		if (oldBuilding is Mill oldMill)
		{
			oldMill.TransferValuesToNewBuilding(newBuilding);
		}
	}

	/// <summary>Migrate all farmhands from Cabin.deprecatedFarmhand into NetWorldState.</summary>
	/// <param name="locations">The locations to scan for cabins.</param>
	public static void MigrateFarmhands(List<GameLocation> locations)
	{
		foreach (GameLocation location in locations)
		{
			foreach (Building building in location.buildings)
			{
				if (building.GetIndoors() is Cabin { obsolete_farmhand: var farmhand } cabin)
				{
					cabin.obsolete_farmhand = null;
					Game1.netWorldState.Value.farmhandData[farmhand.UniqueMultiplayerID] = farmhand;
					cabin.farmhandReference.Value = farmhand;
				}
			}
		}
	}

	/// <summary>Migrate saved bundle data from Stardew Valley 1.5.6 or earlier to the new format.</summary>
	/// <param name="bundleData">The raw bundle data to standardize.</param>
	public static void StandardizeBundleFields(Dictionary<string, string> bundleData)
	{
		string[] array = bundleData.Keys.ToArray();
		foreach (string key in array)
		{
			string[] fields = bundleData[key].Split('/');
			if (fields.Length < 7)
			{
				Array.Resize(ref fields, 7);
				fields[6] = fields[0];
				bundleData[key] = string.Join("/", fields);
			}
		}
	}

	/// <summary>For a building with an upgrade started before 1.6, get the building type it should be upgraded to if possible.</summary>
	/// <param name="fromBuildingType">The building type before the upgrade finishes.</param>
	public static string InferBuildingUpgradingTo(string fromBuildingType)
	{
		switch (fromBuildingType)
		{
		case "Coop":
			return "Big Coop";
		case "Big Coop":
			return "Deluxe Coop";
		case "Barn":
			return "Big Barn";
		case "Big Barn":
			return "Deluxe Barn";
		case "Shed":
			return "Big Shed";
		default:
			foreach (KeyValuePair<string, BuildingData> pair in Game1.buildingData)
			{
				if (pair.Value.BuildingToUpgrade == fromBuildingType)
				{
					return pair.Key;
				}
			}
			return null;
		}
	}

	/// <summary>For a machine which contains output produced before 1.6, set the <see cref="F:StardewValley.Object.lastInputItem" /> and <see cref="F:StardewValley.Object.lastOutputRuleId" /> values when possible. This ensures that some machine logic works as expected (e.g. crystalariums resuming on collect).</summary>
	/// <param name="machine">The machine which produced output.</param>
	/// <remarks>This is heuristic, and some fields may not be set if it's not possible to retroactively infer them.</remarks>
	public static void InferMachineInputOutputFields(Object machine)
	{
		Object output = machine.heldObject.Value;
		string outputItemId = output?.QualifiedItemId;
		if (outputItemId == null)
		{
			return;
		}
		NetRef<Item> inputItem = machine.lastInputItem;
		NetString outputRule = machine.lastOutputRuleId;
		string qualifiedItemId = machine.QualifiedItemId;
		if (qualifiedItemId == null)
		{
			return;
		}
		switch (qualifiedItemId.Length)
		{
		case 6:
			switch (qualifiedItemId[5])
			{
			default:
				return;
			case '0':
				break;
			case '7':
				if (qualifiedItemId == "(BC)17" && outputItemId == "(O)428")
				{
					outputRule.Value = "Default";
					inputItem.Value = ItemRegistry.Create("(O)440");
				}
				return;
			case '3':
			{
				if (!(qualifiedItemId == "(BC)13") || outputItemId == null)
				{
					return;
				}
				int length = outputItemId.Length;
				if (length != 6)
				{
					return;
				}
				switch (outputItemId[5])
				{
				case '4':
					if (outputItemId == "(O)334")
					{
						outputRule.Value = "Default_CopperOre";
						inputItem.Value = ItemRegistry.Create("(O)378", 5);
					}
					break;
				case '5':
					if (outputItemId == "(O)335")
					{
						outputRule.Value = "Default_IronOre";
						inputItem.Value = ItemRegistry.Create("(O)380", 5);
					}
					break;
				case '6':
					if (outputItemId == "(O)336")
					{
						outputRule.Value = "Default_GoldOre";
						inputItem.Value = ItemRegistry.Create("(O)384", 5);
					}
					break;
				case '7':
					if (!(outputItemId == "(O)337"))
					{
						if (outputItemId == "(O)277")
						{
							outputRule.Value = "Default_Bouquet";
							inputItem.Value = ItemRegistry.Create("(O)458");
						}
					}
					else
					{
						outputRule.Value = "Default_IridiumOre";
						inputItem.Value = ItemRegistry.Create("(O)386", 5);
					}
					break;
				case '8':
					if (outputItemId == "(O)338")
					{
						if (output.Stack > 1)
						{
							outputRule.Value = "Default_FireQuartz";
							inputItem.Value = ItemRegistry.Create("(O)82");
						}
						else
						{
							outputRule.Value = "Default_Quartz";
							inputItem.Value = ItemRegistry.Create("(O)80");
						}
					}
					break;
				case '0':
					if (outputItemId == "(O)910")
					{
						outputRule.Value = "Default_RadioactiveOre";
						inputItem.Value = ItemRegistry.Create("(O)909", 5);
					}
					break;
				case '1':
				case '2':
				case '3':
					break;
				}
				return;
			}
			case '2':
			{
				if (!(qualifiedItemId == "(BC)12"))
				{
					return;
				}
				switch (outputItemId)
				{
				case "(O)346":
					outputRule.Value = "Default_Wheat";
					inputItem.Value = ItemRegistry.Create("(O)262");
					return;
				case "(O)303":
					outputRule.Value = "Default_Hops";
					inputItem.Value = ItemRegistry.Create("(O)304");
					return;
				case "(O)614":
					outputRule.Value = "Default_TeaLeaves";
					inputItem.Value = ItemRegistry.Create("(O)815");
					return;
				case "(O)395":
					outputRule.Value = "Default_CoffeeBeans";
					inputItem.Value = ItemRegistry.Create("(O)433", 5);
					return;
				case "(O)340":
					outputRule.Value = "Default_Honey";
					inputItem.Value = ItemRegistry.Create("(O)459", 5);
					return;
				}
				Object.PreserveType? value = output.preserve.Value;
				if (value.HasValue)
				{
					switch (value.GetValueOrDefault())
					{
					case Object.PreserveType.Juice:
						outputRule.Value = "Default_Juice";
						inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
						break;
					case Object.PreserveType.Wine:
						outputRule.Value = "Default_Wine";
						inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
						break;
					}
				}
				return;
			}
			case '5':
				if (!(qualifiedItemId == "(BC)15"))
				{
					if (qualifiedItemId == "(BC)25")
					{
						outputRule.Value = "Default";
						if (outputItemId != "(O)499" && output.HasTypeObject() && Game1.cropData.TryGetValue(output.ItemId, out var cropData) && cropData.HarvestItemId != null)
						{
							inputItem.Value = ItemRegistry.Create(cropData.HarvestItemId, 1, 0, allowNull: true);
						}
					}
					return;
				}
				switch (outputItemId)
				{
				case "(O)445":
					outputRule.Value = "Default_SturgeonRoe";
					inputItem.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(ItemRegistry.Create<Object>("(O)698"));
					break;
				case "(O)447":
					outputRule.Value = "Default_Roe";
					inputItem.Value = ItemRegistry.GetObjectTypeDefinition().CreateFlavoredRoe(ItemRegistry.Create<Object>(output.preservedParentSheetIndex.Value));
					break;
				case "(O)342":
					outputRule.Value = "Default_Pickled";
					inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
					break;
				case "(O)344":
					outputRule.Value = "Default_Jelly";
					inputItem.Value = ItemRegistry.Create(output.preservedParentSheetIndex.Value, 1, 0, allowNull: true);
					break;
				}
				return;
			case '6':
				if (!(qualifiedItemId == "(BC)16"))
				{
					return;
				}
				if (!(outputItemId == "(O)426"))
				{
					if (outputItemId == "(O)424")
					{
						if (output.Quality == 0)
						{
							outputRule.Value = "Default_Milk";
							inputItem.Value = ItemRegistry.Create("(O)184");
						}
						else
						{
							outputRule.Value = "Default_LargeMilk";
							inputItem.Value = ItemRegistry.Create("(O)186");
						}
					}
				}
				else if (output.Quality == 0)
				{
					outputRule.Value = "Default_GoatMilk";
					inputItem.Value = ItemRegistry.Create("(O)436");
				}
				else
				{
					outputRule.Value = "Default_LargeGoatMilk";
					inputItem.Value = ItemRegistry.Create("(O)438");
				}
				return;
			case '4':
				if (!(qualifiedItemId == "(BC)24"))
				{
					return;
				}
				switch (outputItemId)
				{
				case "(O)306":
					switch (output.Stack)
					{
					case 10:
						outputRule.Value = "Default_OstrichEgg";
						inputItem.Value = ItemRegistry.Create("(O)289", 1, output.Quality);
						break;
					case 3:
						outputRule.Value = "Default_GoldenEgg";
						inputItem.Value = ItemRegistry.Create("(O)928");
						break;
					default:
						if (output.Quality == 2)
						{
							outputRule.Value = "Default_LargeEgg";
							inputItem.Value = ItemRegistry.Create("(O)174");
						}
						else
						{
							outputRule.Value = "Default_Egg";
							inputItem.Value = ItemRegistry.Create("(O)176");
						}
						break;
					}
					break;
				case "(O)307":
					outputRule.Value = "Default_DuckEgg";
					inputItem.Value = ItemRegistry.Create("(O)442");
					break;
				case "(O)308":
					outputRule.Value = "Default_VoidEgg";
					inputItem.Value = ItemRegistry.Create("(O)305");
					break;
				case "(O)807":
					outputRule.Value = "Default_DinosaurEgg";
					inputItem.Value = ItemRegistry.Create("(O)107");
					break;
				}
				return;
			case '9':
				if (qualifiedItemId == "(BC)19" && !(outputItemId == "(O)247") && outputItemId == "(O)432")
				{
					outputRule.Value = "Default_Truffle";
					inputItem.Value = ItemRegistry.Create("(O)430");
				}
				return;
			case '1':
				if (qualifiedItemId == "(BC)21")
				{
					outputRule.Value = "Default";
					inputItem.Value = output.getOne();
				}
				return;
			case '8':
				return;
			}
			switch (qualifiedItemId)
			{
			default:
				return;
			case "(BC)90":
				switch (outputItemId)
				{
				case "(O)466":
				case "(O)465":
				case "(O)369":
				case "(O)805":
					outputRule.Value = "Default";
					break;
				}
				return;
			case "(BC)20":
				if (outputItemId == null)
				{
					return;
				}
				switch (outputItemId.Length)
				{
				case 6:
				{
					char c = outputItemId[4];
					if ((uint)c <= 51u)
					{
						switch (c)
						{
						default:
							return;
						case '3':
							_ = outputItemId == "(O)338";
							return;
						case '2':
							break;
						}
						if (!(outputItemId == "(O)428"))
						{
							return;
						}
						break;
					}
					switch (c)
					{
					default:
						return;
					case '8':
						switch (outputItemId)
						{
						default:
							return;
						case "(O)382":
						case "(O)380":
							break;
						case "(O)388":
							outputRule.Value = "Default_Driftwood";
							inputItem.Value = ItemRegistry.Create("(O)169");
							return;
						}
						break;
					case '9':
						if (!(outputItemId == "(O)390"))
						{
							return;
						}
						break;
					}
					outputRule.Value = "Default_Trash";
					inputItem.Value = ItemRegistry.Create("(O)168");
					return;
				}
				case 5:
					if (!(outputItemId == "(O)93"))
					{
						return;
					}
					break;
				default:
					return;
				}
				outputRule.Value = "Default_SoggyNewspaper";
				inputItem.Value = ItemRegistry.Create("(O)172");
				return;
			case "(BC)10":
				break;
			}
			goto IL_0c29;
		case 7:
			switch (qualifiedItemId[5])
			{
			default:
				return;
			case '6':
				switch (qualifiedItemId)
				{
				default:
					_ = qualifiedItemId == "(BC)264";
					return;
				case "(BC)163":
					switch (outputItemId)
					{
					case "(O)424":
						outputRule.Value = "Cheese";
						break;
					case "(O)426":
						outputRule.Value = "GoatCheese";
						break;
					case "(O)348":
						outputRule.Value = "Wine";
						break;
					case "(O)459":
						outputRule.Value = "Mead";
						break;
					case "(O)303":
						outputRule.Value = "PaleAle";
						break;
					case "(O)346":
						outputRule.Value = "Beer";
						break;
					}
					if (outputRule.Value != null)
					{
						inputItem.Value = output.getOne();
						inputItem.Value.Quality = 0;
					}
					return;
				case "(BC)265":
					outputRule.Value = "Default";
					return;
				case "(BC)160":
					break;
				}
				break;
			case '1':
				switch (qualifiedItemId)
				{
				default:
					return;
				case "(BC)114":
					if (outputItemId == "(O)382")
					{
						outputRule.Value = "Default";
						inputItem.Value = ItemRegistry.Create("(O)388", 10);
					}
					return;
				case "(BC)117":
					break;
				case "(BC)211":
					return;
				}
				break;
			case '0':
				if (!(qualifiedItemId == "(BC)101"))
				{
					_ = qualifiedItemId == "(BC)105";
					return;
				}
				goto IL_0b7a;
			case '5':
				switch (qualifiedItemId)
				{
				default:
					return;
				case "(BC)254":
				case "(BC)156":
					break;
				case "(BC)158":
					outputRule.Value = "Default";
					inputItem.Value = ItemRegistry.Create("(O)766", 100);
					return;
				case "(BC)154":
					goto end_IL_00a5;
				}
				goto IL_0b7a;
			case '8':
				if (!(qualifiedItemId == "(BC)182"))
				{
					if (!(qualifiedItemId == "(BC)280"))
					{
						return;
					}
					break;
				}
				outputRule.Value = "Default";
				return;
			case '4':
				if (!(qualifiedItemId == "(BC)246"))
				{
					return;
				}
				break;
			case '3':
				if (!(qualifiedItemId == "(BC)231"))
				{
					return;
				}
				break;
			case '2':
				if (!(qualifiedItemId == "(BC)127") && !(qualifiedItemId == "(BC)128"))
				{
					return;
				}
				break;
			case '7':
				return;
				IL_0b7a:
				outputRule.Value = "Default";
				inputItem.Value = output.getOne();
				return;
				end_IL_00a5:
				break;
			}
			goto IL_0c29;
		case 5:
			{
				if (!(qualifiedItemId == "(BC)9"))
				{
					break;
				}
				goto IL_0c29;
			}
			IL_0c29:
			outputRule.Value = "Default";
			break;
		}
	}

	/// <summary>Migrate a pre-1.6 quest to the new format.</summary>
	/// <param name="serializer">The XML serializer with which to serialize/deserialize <see cref="T:StardewValley.Quests.DescriptionElement" /> and <see cref="T:StardewValley.SaveMigrations.SaveMigrator_1_6.LegacyDescriptionElement" /> values.</param>
	/// <param name="element">The description element to migrate.</param>
	/// <remarks>
	///   This updates quest data for two changes in 1.6:
	///
	///   <list type="bullet">
	///     <item><description>
	///       The way <see cref="F:StardewValley.Quests.DescriptionElement.substitutions" /> values are stored in the save XML changed from this:
	///
	///       <code>
	///         &lt;objective&gt;
	///           &lt;xmlKey&gt;Strings\StringsFromCSFiles:SocializeQuest.cs.13802&lt;/xmlKey&gt;
	///           &lt;param&gt;
	///             &lt;anyType xsi:type="xsd:int"&gt;4&lt;/anyType&gt;
	///             &lt;anyType xsi:type="xsd:int"&gt;28&lt;/anyType&gt;
	///           &lt;/param&gt;
	///         &lt;/objective&gt;
	///       </code>
	///
	///      To this:
	///
	///       <code>
	///         &lt;objective&gt;
	///           &lt;xmlKey&gt;Strings\StringsFromCSFiles:SocializeQuest.cs.13802&lt;/xmlKey&gt;
	///           &lt;param xsi:type="xsd:int"&gt;4&lt;/param&gt;
	///           &lt;param xsi:type="xsd:int"&gt;28&lt;/param&gt;
	///         &lt;/objective&gt;
	///       </code>
	///
	///       If the given description element is affected, this method re-deserializes the data into the correct format.
	///   </description></item>
	///
	///   <item><description>Some translation keys were merged to fix gender issues.</description></item>
	///   </list>
	/// </remarks>
	public static void MigrateLegacyDescriptionElement(Lazy<XmlSerializer> serializer, DescriptionElement element)
	{
		if (element == null)
		{
			return;
		}
		List<object> substitutions = element.substitutions;
		if (substitutions != null && substitutions.Count == 1 && element.substitutions[0] is XmlNode[] nodes)
		{
			StringBuilder xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?><LegacyDescriptionElement xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><param>");
			XmlNode[] array = nodes;
			foreach (XmlNode node in array)
			{
				xml.Append(node.OuterXml);
			}
			xml.Append("</param></LegacyDescriptionElement>");
			LegacyDescriptionElement data;
			using (StringReader stringReader = new StringReader(xml.ToString()))
			{
				using XmlReader xmlReader = new XmlTextReader(stringReader);
				data = (LegacyDescriptionElement)serializer.Value.Deserialize(xmlReader);
			}
			if (data != null)
			{
				element.substitutions = data.param;
			}
		}
		switch (element.translationKey)
		{
		case "Strings\\StringsFromCSFiles:FishingQuest.cs.13251":
			element.translationKey = "Strings\\StringsFromCSFiles:FishingQuest.cs.13248";
			break;
		case "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13563":
			element.translationKey = "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13560";
			break;
		case "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13574":
			element.translationKey = "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13571";
			break;
		}
		List<object> substitutions2 = element.substitutions;
		if (substitutions2 == null || substitutions2.Count <= 0)
		{
			return;
		}
		foreach (object substitution in element.substitutions)
		{
			if (substitution is DescriptionElement childElement)
			{
				SaveMigrator_1_6.MigrateLegacyDescriptionElement(serializer, childElement);
			}
		}
	}
}
