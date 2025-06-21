using System;
using System.Xml.Serialization;
using StardewValley;

namespace Firearm.Framework.Patches;

public class SaveSerializerPatches
{
	private static readonly XmlSerializer ItemSerializer = new XmlSerializer(typeof(Item), new Type[1] { typeof(Firearm) });

	internal static bool GetSerializer(ref XmlSerializer __result, Type type)
	{
		if (type != typeof(Item))
		{
			return true;
		}
		__result = ItemSerializer;
		return false;
	}
}
