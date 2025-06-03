namespace BirbCore.APIs;

public interface IDynamicGameAssetsApi
{
	string GetDGAItemId(object item);

	object SpawnDGAItem(string fullId);
}
