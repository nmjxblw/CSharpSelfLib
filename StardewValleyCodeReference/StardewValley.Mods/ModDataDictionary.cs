using Netcode;
using StardewValley.Network;

namespace StardewValley.Mods;

public class ModDataDictionary : NetStringDictionary<string, NetString>
{
	public ModDataDictionary()
	{
		base.InterpolationWait = false;
	}

	public virtual void SetFromSerialization(ModDataDictionary source)
	{
		base.Clear();
		if (source == null)
		{
			return;
		}
		foreach (string key in source.Keys)
		{
			base[key] = source[key];
		}
	}

	public ModDataDictionary GetForSerialization()
	{
		if (Game1.game1 != null && Game1.game1.IsSaving && base.Length == 0)
		{
			return null;
		}
		return this;
	}

	public void CopyFrom(ModDataDictionary dict)
	{
		base.CopyFrom(dict.Pairs);
	}
}
