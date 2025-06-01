using System.Collections.Generic;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on specific game types.</summary>
public static class GameExtensions
{
	/// <summary>Add a light source to the dictionary.</summary>
	/// <param name="dictionary">The dictionary of light sources to update.</param>
	/// <param name="lightSource">The light source to add.</param>
	public static void Add(this IDictionary<string, LightSource> dictionary, LightSource lightSource)
	{
		if (lightSource != null)
		{
			if (string.IsNullOrWhiteSpace(lightSource.Id))
			{
				lightSource.Id = $"LightSource_TempId_{Game1.random.Next()}";
				Game1.log.Warn("Light source has no ID; assigning ID '" + lightSource.Id + "'.");
			}
			dictionary[lightSource.Id] = lightSource;
		}
	}

	/// <summary>Add or overwrite a light source to the dictionary.</summary>
	/// <param name="dictionary">The dictionary of light sources to update.</param>
	/// <param name="lightSource">The light source to add.</param>
	public static void AddLight(this NetStringDictionary<LightSource, NetRef<LightSource>> dictionary, LightSource lightSource)
	{
		if (lightSource != null)
		{
			if (string.IsNullOrWhiteSpace(lightSource.Id))
			{
				lightSource.Id = $"LightSource_TempId_{Game1.random.Next()}";
				Game1.log.Warn("Light source has no ID; assigning ID '" + lightSource.Id + "'.");
			}
			dictionary[lightSource.Id] = lightSource;
		}
	}
}
