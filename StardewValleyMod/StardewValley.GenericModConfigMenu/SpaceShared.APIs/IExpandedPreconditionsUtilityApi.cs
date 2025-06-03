namespace SpaceShared.APIs;

public interface IExpandedPreconditionsUtilityApi
{
	void Initialize(bool verbose, string uniqueId);

	bool CheckConditions(string[] conditions);

	bool CheckConditions(string conditions);
}
