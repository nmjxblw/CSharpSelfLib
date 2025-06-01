namespace StardewValley.SDKs;

public interface SDKHelper
{
	/// <summary>
	/// This property needs to be initialized to the correct value before Initialize(), so probably within EarlyInitialize().
	/// </summary>
	bool IsEnterButtonAssignmentFlipped { get; }

	/// <summary>
	/// This property needs to be initialized to the correct value before Initialize(), so probably within EarlyInitialize().
	/// </summary>
	bool IsJapaneseRegionRelease { get; }

	string Name { get; }

	SDKNetHelper Networking { get; }

	bool ConnectionFinished { get; }

	int ConnectionProgress { get; }

	bool HasOverlay { get; }

	void EarlyInitialize();

	void Initialize();

	void Update();

	void Shutdown();

	void DebugInfo();

	/// <summary>Get whether platform achievements can be unlocked retroactively overnight or when loading the save.</summary>
	/// <remarks>Certification requirements on some platforms prohibit us from unlocking trophies without the player doing something. On those platforms, we instead unlock missed achievements when the player performs a relevant action.</remarks>
	bool RetroactiveAchievementsAllowed();

	void GetAchievement(string achieve);

	void ResetAchievements();

	string FilterDirtyWords(string words);
}
