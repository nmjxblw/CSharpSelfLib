namespace BirbCore.APIs;

public interface IMargo
{
	public interface IModConfig
	{
		bool EnableProfessions { get; }
	}

	void RegisterCustomSkillForPrestige(string id);

	IModConfig GetConfig();
}
