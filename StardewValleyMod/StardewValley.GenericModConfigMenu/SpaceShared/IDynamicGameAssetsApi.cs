using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace SpaceShared;

public interface IDynamicGameAssetsApi
{
	string GetDGAItemId(object item);

	object SpawnDGAItem(string fullId, Color? color);

	object SpawnDGAItem(string fullId);

	void AddEmbeddedPack(IManifest manifest, string dir);
}
