using System;
using System.Diagnostics.CodeAnalysis;
using Netcode;
using StardewValley;
using StardewValley.Mods;
using StardewValley.Network;

namespace Pathoschild.Stardew.Common;

internal static class ModDataDictionaryExtensions
{
	[return: NotNullIfNotNull("defaultValue")]
	public static T? ReadField<T>(this ModDataDictionary data, string key, Func<string, T> parse, T? defaultValue = default(T?))
	{
		string rawValue = default(string);
		if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data).TryGetValue(key, ref rawValue))
		{
			try
			{
				return parse(rawValue);
			}
			catch
			{
			}
		}
		return defaultValue;
	}

	[return: NotNullIfNotNull("defaultValue")]
	public static string? ReadField(this ModDataDictionary data, string key, string? defaultValue = null)
	{
		string rawValue = default(string);
		if (!((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data).TryGetValue(key, ref rawValue))
		{
			return defaultValue;
		}
		return rawValue;
	}

	public static ModDataDictionary WriteField(this ModDataDictionary data, string key, string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data).Remove(key);
		}
		else
		{
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data)[key] = value;
		}
		return data;
	}
}
