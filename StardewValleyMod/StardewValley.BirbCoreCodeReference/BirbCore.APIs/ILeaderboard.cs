using System.Collections.Generic;

namespace BirbCore.APIs;

public interface ILeaderboard
{
	bool UploadScore(string stat, int score);

	List<Dictionary<string, string>> GetTopN(string stat, int count);

	List<Dictionary<string, string>> GetLocalTopN(string stat, int count);

	int GetRank(string stat);

	int GetLocalRank(string stat);

	Dictionary<string, string> GetPersonalBest(string stat);

	bool RefreshCache(string stat);
}
