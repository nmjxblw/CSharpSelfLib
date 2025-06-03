using SpaceShared.APIs;
using StardewModdingAPI;

namespace SpaceShared;

internal static class ModExtensions
{
	public static TInterface GetApi<TInterface>(this IModRegistry modRegistry, string uniqueId, string label, string minVersion, IMonitor monitor) where TInterface : class
	{
		IModInfo obj = modRegistry.Get(uniqueId);
		IManifest manifest = ((obj != null) ? obj.Manifest : null);
		if (manifest == null)
		{
			return null;
		}
		if (manifest.Version.IsOlderThan(minVersion))
		{
			monitor.Log($"Detected {label} {manifest.Version}, but need {minVersion} or later. Disabled integration with this mod.", (LogLevel)3);
			return null;
		}
		TInterface api = modRegistry.GetApi<TInterface>(uniqueId);
		if (api == null)
		{
			monitor.Log("Detected " + label + ", but couldn't fetch its API. Disabled integration with this mod.", (LogLevel)3);
			return null;
		}
		return api;
	}

	public static IGenericModConfigMenuApi GetGenericModConfigMenuApi(this IModRegistry modRegistry, IMonitor monitor)
	{
		return modRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", "Generic Mod Config Menu", "1.8.0", monitor);
	}
}
