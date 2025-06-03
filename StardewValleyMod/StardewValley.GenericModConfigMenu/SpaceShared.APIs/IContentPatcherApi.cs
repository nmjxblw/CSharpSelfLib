using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace SpaceShared.APIs;

public interface IContentPatcherApi
{
	bool IsConditionsApiReady { get; }

	IManagedConditions ParseConditions(IManifest manifest, IDictionary<string, string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null);

	IManagedTokenString ParseTokenString(IManifest manifest, string rawValue, ISemanticVersion formatVersion, string[]? assumeModIds = null);

	void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>?> getValue);

	void RegisterToken(IManifest mod, string name, object token);
}
