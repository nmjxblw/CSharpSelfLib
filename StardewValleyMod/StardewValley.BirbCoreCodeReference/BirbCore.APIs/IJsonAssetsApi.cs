namespace BirbCore.APIs;

public interface IJsonAssetsApi
{
	void LoadAssets(string path);

	int GetObjectId(string name);

	int GetBigCraftableId(string name);

	int GetHatId(string name);

	int GetWeaponId(string name);

	int GetClothingId(string name);
}
