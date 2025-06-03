using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace GenericModConfigMenu.Framework;

internal class ModConfigManager
{
	private readonly Dictionary<string, ModConfig> Configs = new Dictionary<string, ModConfig>(StringComparer.OrdinalIgnoreCase);

	public ModConfig Get(IManifest manifest, bool assert)
	{
		this.AssertManifest(manifest);
		lock (this.Configs)
		{
			if (this.Configs.TryGetValue(manifest.UniqueID, out var value))
			{
				return value;
			}
			if (!assert)
			{
				return null;
			}
			throw new KeyNotFoundException("The '" + manifest.Name + "' mod hasn't registered a config menu.");
		}
	}

	public IEnumerable<ModConfig> GetAll()
	{
		lock (this.Configs)
		{
			return this.Configs.Values;
		}
	}

	public void Set(IManifest manifest, ModConfig config)
	{
		lock (this.Configs)
		{
			this.AssertManifest(manifest);
			this.Configs[manifest.UniqueID] = config ?? throw new ArgumentNullException("config");
		}
	}

	public void Remove(IManifest manifest)
	{
		lock (this.Configs)
		{
			this.AssertManifest(manifest);
			if (this.Configs.ContainsKey(manifest.UniqueID))
			{
				this.Configs.Remove(manifest.UniqueID);
			}
		}
	}

	private void AssertManifest(IManifest manifest)
	{
		if (manifest == null)
		{
			throw new ArgumentNullException("manifest");
		}
		if (string.IsNullOrWhiteSpace(manifest.UniqueID))
		{
			throw new ArgumentException("The '" + manifest.Name + "' mod manifest doesn't have a unique ID value.", "manifest");
		}
		if (string.IsNullOrWhiteSpace(manifest.Name))
		{
			throw new ArgumentException("The '" + manifest.UniqueID + "' mod manifest doesn't have a name value.", "manifest");
		}
	}
}
