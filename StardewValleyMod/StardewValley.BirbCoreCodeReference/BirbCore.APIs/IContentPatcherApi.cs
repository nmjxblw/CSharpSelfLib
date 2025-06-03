using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace BirbCore.APIs;

public interface IContentPatcherApi
{
	bool IsConditionsApiReady { get; }

	void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>?> getValue);

	void RegisterToken(IManifest mod, string name, object token);
}
