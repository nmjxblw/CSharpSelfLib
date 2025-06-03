using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Constants;

internal static class Constant
{
	public static class MailLetters
	{
		public const string ReceivedSpouseStardrop = "CF_Spouse";

		public const string JojaMember = "JojaMember";
	}

	public static class SeasonNames
	{
		public const string Spring = "spring";

		public const string Summer = "summer";

		public const string Fall = "fall";

		public const string Winter = "winter";
	}

	public static class ItemNames
	{
		public static string Heater = "Heater";
	}

	public static class BuildingNames
	{
		public static string GoldClock = "Gold Clock";
	}

	public static class ObjectIndexes
	{
		public static int AutoGrabber = 165;
	}

	public static readonly int MaxStackSizeForPricing = 999;

	public static readonly Vector2 MaxTargetSpriteSize = new Vector2(3f, 5f);

	public static readonly Vector2 MaxBuildingTargetSpriteSize = new Vector2(10f, 10f);

	public static bool AllowBold => (int)Game1.content.GetCurrentLanguage() != 3;
}
