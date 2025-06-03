using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace SpaceShared.APIs;

public interface IJsonAssetsApi
{
	event EventHandler ItemsRegistered;

	event EventHandler AddedItemsToShop;

	void LoadAssets(string path);

	void LoadAssets(string path, ITranslationHelper translations);

	List<string> GetAllObjectsFromContentPack(string cp);

	List<string> GetAllCropsFromContentPack(string cp);

	List<string> GetAllFruitTreesFromContentPack(string cp);

	List<string> GetAllBigCraftablesFromContentPack(string cp);

	List<string> GetAllHatsFromContentPack(string cp);

	List<string> GetAllWeaponsFromContentPack(string cp);

	List<string> GetAllClothingFromContentPack(string cp);

	List<string> GetAllBootsFromContentPack(string cp);
}
