using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley;

[InstanceStatics]
public static class Rumble
{
	private static float rumbleStrength;

	private static float rumbleTimerMax;

	private static float rumbleTimerCurrent;

	private static float rumbleDuringFade;

	private static float maxRumbleDuringFade;

	private static bool isRumbling;

	private static bool fade;

	public static void update(float milliseconds)
	{
		float rumble_amount = 0f;
		if (Rumble.isRumbling)
		{
			rumble_amount = Rumble.rumbleStrength;
			Rumble.rumbleTimerCurrent += milliseconds;
			if (Rumble.rumbleTimerCurrent > Rumble.rumbleTimerMax)
			{
				rumble_amount = 0f;
			}
			else if (Rumble.fade)
			{
				if (Rumble.rumbleTimerCurrent > Rumble.rumbleTimerMax - 1000f)
				{
					Rumble.rumbleDuringFade = Utility.Lerp(Rumble.maxRumbleDuringFade, 0f, (Rumble.rumbleTimerCurrent - (Rumble.rumbleTimerMax - 1000f)) / 1000f);
				}
				rumble_amount = Rumble.rumbleDuringFade;
			}
		}
		if (rumble_amount <= 0f)
		{
			rumble_amount = 0f;
			Rumble.isRumbling = false;
		}
		if ((double)rumble_amount > 1.0)
		{
			rumble_amount = 1f;
		}
		if (!Game1.options.gamepadControls || !Game1.options.rumble)
		{
			rumble_amount = 0f;
		}
		if (Game1.playerOneIndex != (PlayerIndex)(-1))
		{
			GamePad.SetVibration(Game1.playerOneIndex, rumble_amount, rumble_amount);
		}
	}

	public static void stopRumbling()
	{
		Rumble.rumbleStrength = 0f;
		Rumble.isRumbling = false;
	}

	public static void rumble(float leftPower, float rightPower, float milliseconds)
	{
		Rumble.rumble(leftPower, milliseconds);
	}

	public static void rumble(float power, float milliseconds)
	{
		if (!Rumble.isRumbling && Game1.options.gamepadControls && Game1.options.rumble)
		{
			Rumble.fade = false;
			Rumble.rumbleTimerCurrent = 0f;
			Rumble.rumbleTimerMax = milliseconds;
			Rumble.isRumbling = true;
			Rumble.rumbleStrength = power;
		}
	}

	public static void rumbleAndFade(float power, float milliseconds)
	{
		if (!Rumble.isRumbling && Game1.options.gamepadControls && Game1.options.rumble)
		{
			Rumble.rumbleTimerCurrent = 0f;
			Rumble.rumbleTimerMax = milliseconds;
			Rumble.isRumbling = true;
			Rumble.fade = true;
			Rumble.rumbleDuringFade = power;
			Rumble.maxRumbleDuringFade = power;
			Rumble.rumbleStrength = power;
		}
	}
}
