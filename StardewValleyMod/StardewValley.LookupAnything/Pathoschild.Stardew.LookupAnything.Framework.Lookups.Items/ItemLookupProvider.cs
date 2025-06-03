using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Items;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;

internal class ItemLookupProvider : BaseLookupProvider
{
	private readonly ItemRepository ItemRepository = new ItemRepository();

	private readonly Func<ModConfig> Config;

	private readonly ISubjectRegistry Codex;

	public ItemLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, ISubjectRegistry codex)
		: base(reflection, gameHelper)
	{
		this.Config = config;
		this.Codex = codex;
	}

	public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector2 key;
		foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
		{
			pair.Deconstruct(out key, out var value);
			Vector2 tile = key;
			Object obj = value;
			if ((!(location is IslandShrine) || !(obj is ItemPedestal)) && base.GameHelper.CouldSpriteOccludeTile(tile, lookupTile))
			{
				yield return new ObjectTarget(base.GameHelper, obj, tile, () => this.BuildSubject((Item)(object)obj, ObjectContext.World, location, knownQuality: false));
			}
		}
		foreach (Furniture furniture in location.furniture)
		{
			Vector2 entityTile = ((Object)furniture).TileLocation;
			if (base.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
			{
				yield return new ObjectTarget(base.GameHelper, (Object)(object)furniture, entityTile, () => this.BuildSubject((Item)(object)furniture, ObjectContext.Inventory, location));
			}
		}
		Enumerator<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>> enumerator3 = ((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)location.terrainFeatures).Pairs.GetEnumerator();
		TerrainFeature value2;
		try
		{
			while (enumerator3.MoveNext())
			{
				enumerator3.Current.Deconstruct(out key, out value2);
				Vector2 tile2 = key;
				TerrainFeature feature = value2;
				HoeDirt dirt = (HoeDirt)(object)((feature is HoeDirt) ? feature : null);
				if (dirt != null && dirt.crop != null && base.GameHelper.CouldSpriteOccludeTile(tile2, lookupTile))
				{
					yield return new CropTarget(base.GameHelper, dirt, tile2, () => this.BuildSubject(dirt.crop, ObjectContext.World, dirt));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator3/*cast due to .constrained prefix*/).Dispose();
		}
		enumerator3 = ((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)location.terrainFeatures).Pairs.GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				enumerator3.Current.Deconstruct(out key, out value2);
				Vector2 entityTile2 = key;
				TerrainFeature feature2 = value2;
				if (!base.GameHelper.CouldSpriteOccludeTile(entityTile2, lookupTile))
				{
					continue;
				}
				Flooring flooring = (Flooring)(object)((feature2 is Flooring) ? feature2 : null);
				if (flooring == null)
				{
					continue;
				}
				FloorPathData data = flooring.GetData();
				if (data != null)
				{
					Item item = ItemRegistry.Create(data.ItemId, 1, 0, false);
					yield return new FlooringTarget(base.GameHelper, flooring, entityTile2, () => this.BuildSubject(item, ObjectContext.World, location));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator3/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public override ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY)
	{
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		IClickableMenu targetMenu = base.GameHelper.GetGameMenuPage(menu) ?? menu;
		IClickableMenu val = targetMenu;
		MenuWithInventory inventoryMenu = (MenuWithInventory)(object)((val is MenuWithInventory) ? val : null);
		if (inventoryMenu == null)
		{
			InventoryPage inventory = (InventoryPage)(object)((val is InventoryPage) ? val : null);
			if (inventory == null)
			{
				ShopMenu shopMenu = (ShopMenu)(object)((val is ShopMenu) ? val : null);
				if (shopMenu != null)
				{
					ISalable entry = shopMenu.hoveredItem;
					Item item = (Item)(object)((entry is Item) ? entry : null);
					if (item != null)
					{
						return this.BuildSubject(item, ObjectContext.Inventory, null);
					}
					MovieConcession snack = (MovieConcession)(object)((entry is MovieConcession) ? entry : null);
					if (snack != null)
					{
						return new MovieSnackSubject(base.GameHelper, snack);
					}
					return this.GetSubject((IClickableMenu)(object)shopMenu.inventory, cursorX, cursorY);
				}
				Toolbar toolbar = (Toolbar)(object)((val is Toolbar) ? val : null);
				if (toolbar == null)
				{
					InventoryMenu inventory2 = (InventoryMenu)(object)((val is InventoryMenu) ? val : null);
					if (inventory2 == null)
					{
						CollectionsPage collectionsTab = (CollectionsPage)(object)((val is CollectionsPage) ? val : null);
						if (collectionsTab == null)
						{
							CraftingPage crafting = (CraftingPage)(object)((val is CraftingPage) ? val : null);
							if (crafting == null)
							{
								ProfileMenu profileMenu = (ProfileMenu)(object)((val is ProfileMenu) ? val : null);
								if (profileMenu == null)
								{
									JunimoNoteMenu bundleMenu = (JunimoNoteMenu)(object)((val is JunimoNoteMenu) ? val : null);
									if (bundleMenu == null)
									{
										goto IL_0640;
									}
									Item item2 = bundleMenu.hoveredItem;
									if (item2 != null)
									{
										return this.BuildSubject(item2, ObjectContext.Inventory, null);
									}
									for (int i = 0; i < bundleMenu.ingredientList.Count; i++)
									{
										if (((ClickableComponent)bundleMenu.ingredientList[i]).containsPoint(cursorX, cursorY))
										{
											Bundle bundle = bundleMenu.currentPageBundle;
											BundleIngredientDescription ingredient = bundle.ingredients[i];
											Item item3 = ItemRegistry.Create(ingredient.id, ingredient.stack, 0, false);
											item3.Quality = ingredient.quality;
											return this.BuildSubject(item3, ObjectContext.Inventory, null);
										}
									}
									foreach (ClickableTextureComponent slot in bundleMenu.ingredientSlots)
									{
										if (((ClickableComponent)slot).item != null && ((ClickableComponent)slot).containsPoint(cursorX, cursorY))
										{
											return this.BuildSubject(((ClickableComponent)slot).item, ObjectContext.Inventory, null);
										}
									}
								}
								else
								{
									Item item4 = profileMenu.hoveredItem;
									if (item4 != null)
									{
										return this.BuildSubject(item4, ObjectContext.Inventory, null);
									}
								}
							}
							else
							{
								Item item5 = crafting.hoverItem;
								if (item5 != null)
								{
									return this.BuildSubject(item5, ObjectContext.Inventory, null);
								}
								CraftingRecipe recipe = crafting.hoverRecipe;
								if (recipe != null)
								{
									return this.BuildSubject(recipe.createItem(), ObjectContext.Inventory, null);
								}
								if (crafting.pagesOfCraftingRecipes.TryGetIndex(crafting.currentCraftingPage, out Dictionary<ClickableTextureComponent, CraftingRecipe> page))
								{
									foreach (var (sprite, recipe2) in page)
									{
										if (((ClickableComponent)sprite).containsPoint(cursorX, cursorY))
										{
											Item item6 = ((recipe2 != null) ? recipe2.createItem() : null);
											if (item6 != null)
											{
												return this.BuildSubject(recipe2.createItem(), ObjectContext.Inventory, null);
											}
											break;
										}
									}
								}
							}
						}
						else
						{
							int currentTab = collectionsTab.currentTab;
							if ((uint)(currentTab - 5) > 2u)
							{
								int currentPage = collectionsTab.currentPage;
								foreach (ClickableTextureComponent component in collectionsTab.collections[currentTab][currentPage])
								{
									if (((ClickableComponent)component).containsPoint(cursorX, cursorY))
									{
										string itemID = ((ClickableComponent)component).name.Split(' ')[0];
										Item item7 = ItemRegistry.Create(itemID, 1, 0, false);
										return this.BuildSubject(item7, ObjectContext.Inventory, null, knownQuality: false);
									}
								}
							}
						}
					}
					else
					{
						foreach (ClickableComponent slot2 in inventory2.inventory)
						{
							if (slot2.containsPoint(cursorX, cursorY))
							{
								if (int.TryParse(slot2.name, out var index) && inventory2.actualInventory.TryGetIndex(index, out Item item8) && item8 != null)
								{
									return this.BuildSubject(item8, ObjectContext.Inventory, null);
								}
								break;
							}
						}
					}
				}
				else
				{
					ClickableComponent hoveredSlot = ((IEnumerable<ClickableComponent>)toolbar.buttons).FirstOrDefault((Func<ClickableComponent, bool>)((ClickableComponent val4) => val4.containsPoint(cursorX, cursorY)));
					if (hoveredSlot == null)
					{
						return null;
					}
					int index2 = toolbar.buttons.IndexOf(hoveredSlot);
					if (index2 < 0 || index2 > Game1.player.Items.Count - 1)
					{
						return null;
					}
					Item item9 = Game1.player.Items[index2];
					if (item9 != null)
					{
						return this.BuildSubject(item9, ObjectContext.Inventory, null);
					}
				}
			}
			else
			{
				Item item10 = Game1.player.CursorSlotItem ?? inventory.hoveredItem;
				if (item10 != null)
				{
					return this.BuildSubject(item10, ObjectContext.Inventory, null);
				}
			}
		}
		else if ((menu is FieldOfficeMenu || menu is TailoringMenu) ? true : false)
		{
			TailoringMenu tailoringMenu = (TailoringMenu)(object)((val is TailoringMenu) ? val : null);
			if (tailoringMenu != null)
			{
				ClickableTextureComponent[] array = (ClickableTextureComponent[])(object)new ClickableTextureComponent[3] { tailoringMenu.leftIngredientSpot, tailoringMenu.rightIngredientSpot, tailoringMenu.craftResultDisplay };
				foreach (ClickableTextureComponent slot3 in array)
				{
					if (((ClickableComponent)slot3).containsPoint(cursorX, cursorY) && ((ClickableComponent)slot3).item != null)
					{
						return this.BuildSubject(((ClickableComponent)slot3).item, ObjectContext.Inventory, null);
					}
				}
				return this.GetSubject((IClickableMenu)(object)((MenuWithInventory)tailoringMenu).inventory, cursorX, cursorY);
			}
			FieldOfficeMenu fieldOfficeMenu = (FieldOfficeMenu)(object)((val is FieldOfficeMenu) ? val : null);
			if (fieldOfficeMenu == null)
			{
				goto IL_0640;
			}
			ClickableComponent slot4 = ((IEnumerable<ClickableComponent>)fieldOfficeMenu.pieceHolders).FirstOrDefault((Func<ClickableComponent, bool>)((ClickableComponent p) => p.containsPoint(cursorX, cursorY)));
			if (slot4 != null)
			{
				if (slot4.item != null)
				{
					return this.BuildSubject(slot4.item, ObjectContext.Inventory, null, knownQuality: false);
				}
				if (CommonHelper.IsItemId(slot4.label))
				{
					return this.BuildSubject(ItemRegistry.Create(slot4.label, 1, 0, false), ObjectContext.Inventory, null, knownQuality: false);
				}
			}
		}
		else
		{
			Item item11 = Game1.player.CursorSlotItem ?? inventoryMenu.heldItem ?? inventoryMenu.hoveredItem;
			if (item11 != null)
			{
				return this.BuildSubject(item11, ObjectContext.Inventory, null);
			}
		}
		goto IL_0692;
		IL_0692:
		return null;
		IL_0640:
		Item item12 = base.Reflection.GetField<Item>((object)targetMenu, "hoveredItem", false)?.GetValue() ?? base.Reflection.GetField<Item>((object)targetMenu, "HoveredItem", false)?.GetValue();
		if (item12 != null)
		{
			return this.BuildSubject(item12, ObjectContext.Inventory, null);
		}
		goto IL_0692;
	}

	public override IEnumerable<ISubject> GetSearchSubjects()
	{
		foreach (SearchableItem item in this.ItemRepository.GetAll())
		{
			yield return this.BuildSubject(item.Item, ObjectContext.World, null, knownQuality: false);
		}
	}

	public override ISubject? GetSubjectFor(object entity, GameLocation? location)
	{
		Item item = (Item)((entity is Item) ? entity : null);
		if (item == null)
		{
			return null;
		}
		return this.BuildSubject(item, ObjectContext.Any, location, knownQuality: false);
	}

	private ISubject BuildSubject(Item target, ObjectContext context, GameLocation? location, bool knownQuality = true)
	{
		ModConfig config = this.Config();
		return new ItemSubject(this.Codex, base.GameHelper, config.ShowUncaughtFishSpawnRules, config.ShowUnknownGiftTastes, showUnknownRecipes: config.ShowUnknownRecipes, showInvalidRecipes: config.ShowInvalidRecipes, highlightUnrevealedGiftTastes: config.HighlightUnrevealedGiftTastes, showGiftTastes: config.ShowGiftTastes, collapseFieldsConfig: config.CollapseLargeFields, item: target, context: context, knownQuality: knownQuality, location: location, getCropSubject: BuildSubject);
	}

	private ISubject BuildSubject(Crop target, ObjectContext context, HoeDirt? dirt)
	{
		string indexOfHarvest = ((NetFieldBase<string, NetString>)(object)target.indexOfHarvest).Value;
		if (!CommonHelper.IsItemId(indexOfHarvest, allowZero: false) && ((NetFieldBase<bool, NetBool>)(object)target.forageCrop).Value)
		{
			if (((NetFieldBase<string, NetString>)(object)target.whichForageCrop).Value == 2.ToString())
			{
				indexOfHarvest = "829";
			}
			else if (((NetFieldBase<string, NetString>)(object)target.whichForageCrop).Value == 1.ToString())
			{
				indexOfHarvest = "399";
			}
		}
		ModConfig config = this.Config();
		return new ItemSubject(this.Codex, base.GameHelper, config.ShowUncaughtFishSpawnRules, config.ShowUnknownGiftTastes, showUnknownRecipes: config.ShowUnknownRecipes, showInvalidRecipes: config.ShowInvalidRecipes, highlightUnrevealedGiftTastes: config.HighlightUnrevealedGiftTastes, showGiftTastes: config.ShowGiftTastes, collapseFieldsConfig: config.CollapseLargeFields, item: ItemRegistry.Create(indexOfHarvest, 1, 0, false), context: context, knownQuality: false, location: (dirt != null) ? ((TerrainFeature)dirt).Location : null, getCropSubject: BuildSubject, fromCrop: null, fromDirt: dirt);
	}
}
