using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles;

[InstanceStatics]
public class AmbientLocationSounds
{
	public const int sound_babblingBrook = 0;

	public const int sound_cracklingFire = 1;

	public const int sound_engine = 2;

	public const int sound_cricket = 3;

	public const int sound_waterfall = 4;

	public const int sound_waterfall_big = 5;

	public const int numberOfSounds = 6;

	public const float doNotPlay = 9999999f;

	private static Dictionary<Vector2, int> sounds = new Dictionary<Vector2, int>();

	private static int updateTimer = 100;

	private static int farthestSoundDistance = 1024;

	private static float[] shortestDistanceForCue;

	private static ICue babblingBrook;

	private static ICue cracklingFire;

	private static ICue engine;

	private static ICue cricket;

	private static ICue waterfall;

	private static ICue waterfallBig;

	private static float volumeOverrideForLocChange;

	public static void InitShared()
	{
		if (AmbientLocationSounds.babblingBrook == null)
		{
			Game1.playSound("babblingBrook", out AmbientLocationSounds.babblingBrook);
			AmbientLocationSounds.babblingBrook.Pause();
		}
		if (AmbientLocationSounds.cracklingFire == null)
		{
			Game1.playSound("cracklingFire", out AmbientLocationSounds.cracklingFire);
			AmbientLocationSounds.cracklingFire.Pause();
		}
		if (AmbientLocationSounds.engine == null)
		{
			Game1.playSound("heavyEngine", out AmbientLocationSounds.engine);
			AmbientLocationSounds.engine.Pause();
		}
		if (AmbientLocationSounds.cricket == null)
		{
			Game1.playSound("cricketsAmbient", out AmbientLocationSounds.cricket);
			AmbientLocationSounds.cricket.Pause();
		}
		if (AmbientLocationSounds.waterfall == null)
		{
			Game1.playSound("waterfall", out AmbientLocationSounds.waterfall);
			AmbientLocationSounds.waterfall.Pause();
		}
		if (AmbientLocationSounds.waterfallBig == null)
		{
			Game1.playSound("waterfall_big", out AmbientLocationSounds.waterfallBig);
			AmbientLocationSounds.waterfallBig.Pause();
		}
		AmbientLocationSounds.shortestDistanceForCue = new float[6];
	}

	public static void update(GameTime time)
	{
		if (AmbientLocationSounds.sounds.Count == 0)
		{
			return;
		}
		if (AmbientLocationSounds.volumeOverrideForLocChange < 1f)
		{
			AmbientLocationSounds.volumeOverrideForLocChange += (float)time.ElapsedGameTime.Milliseconds * 0.0003f;
		}
		AmbientLocationSounds.updateTimer -= time.ElapsedGameTime.Milliseconds;
		if (AmbientLocationSounds.updateTimer > 0)
		{
			return;
		}
		for (int i = 0; i < AmbientLocationSounds.shortestDistanceForCue.Length; i++)
		{
			AmbientLocationSounds.shortestDistanceForCue[i] = 9999999f;
		}
		Vector2 farmerPosition = Game1.player.getStandingPosition();
		foreach (KeyValuePair<Vector2, int> pair in AmbientLocationSounds.sounds)
		{
			float distance = Vector2.Distance(pair.Key, farmerPosition);
			if (AmbientLocationSounds.shortestDistanceForCue[pair.Value] > distance)
			{
				AmbientLocationSounds.shortestDistanceForCue[pair.Value] = distance;
			}
		}
		if (AmbientLocationSounds.volumeOverrideForLocChange >= 0f)
		{
			for (int j = 0; j < AmbientLocationSounds.shortestDistanceForCue.Length; j++)
			{
				if (AmbientLocationSounds.shortestDistanceForCue[j] <= (float)AmbientLocationSounds.farthestSoundDistance * 1.5f)
				{
					float volume = Math.Min(AmbientLocationSounds.volumeOverrideForLocChange, Math.Min(1f, 1f - AmbientLocationSounds.shortestDistanceForCue[j] / ((float)AmbientLocationSounds.farthestSoundDistance * 1.5f)));
					volume = (float)Math.Pow(volume, 5.0);
					switch (j)
					{
					case 0:
						if (AmbientLocationSounds.babblingBrook != null)
						{
							AmbientLocationSounds.babblingBrook.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							AmbientLocationSounds.babblingBrook.Resume();
						}
						break;
					case 1:
						if (AmbientLocationSounds.cracklingFire != null)
						{
							AmbientLocationSounds.cracklingFire.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							AmbientLocationSounds.cracklingFire.Resume();
						}
						break;
					case 2:
						if (AmbientLocationSounds.engine != null)
						{
							AmbientLocationSounds.engine.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							AmbientLocationSounds.engine.Resume();
						}
						break;
					case 3:
						if (AmbientLocationSounds.cricket != null)
						{
							AmbientLocationSounds.cricket.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							AmbientLocationSounds.cricket.Resume();
						}
						break;
					case 4:
						if (AmbientLocationSounds.waterfall != null)
						{
							AmbientLocationSounds.waterfall.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							AmbientLocationSounds.waterfall.Resume();
						}
						break;
					case 5:
						if (AmbientLocationSounds.waterfallBig != null)
						{
							AmbientLocationSounds.waterfallBig.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							AmbientLocationSounds.waterfallBig.Resume();
						}
						break;
					}
				}
				else
				{
					switch (j)
					{
					case 0:
						AmbientLocationSounds.babblingBrook?.Pause();
						break;
					case 1:
						AmbientLocationSounds.cracklingFire?.Pause();
						break;
					case 2:
						AmbientLocationSounds.engine?.Pause();
						break;
					case 3:
						AmbientLocationSounds.cricket?.Pause();
						break;
					case 4:
						AmbientLocationSounds.waterfall?.Pause();
						break;
					case 5:
						AmbientLocationSounds.waterfallBig?.Pause();
						break;
					}
				}
			}
		}
		AmbientLocationSounds.updateTimer = 100;
	}

	public static void changeSpecificVariable(string variableName, float value, int whichSound)
	{
		if (whichSound == 2)
		{
			AmbientLocationSounds.engine?.SetVariable(variableName, value);
		}
	}

	public static void addSound(Vector2 tileLocation, int whichSound)
	{
		AmbientLocationSounds.sounds.TryAdd(tileLocation * 64f, whichSound);
	}

	public static void removeSound(Vector2 tileLocation)
	{
		if (AmbientLocationSounds.sounds.TryGetValue(tileLocation * 64f, out var sound))
		{
			switch (sound)
			{
			case 0:
				AmbientLocationSounds.babblingBrook?.Pause();
				break;
			case 1:
				AmbientLocationSounds.cracklingFire?.Pause();
				break;
			case 2:
				AmbientLocationSounds.engine?.Pause();
				break;
			case 3:
				AmbientLocationSounds.cricket?.Pause();
				break;
			case 4:
				AmbientLocationSounds.waterfall?.Pause();
				break;
			case 5:
				AmbientLocationSounds.waterfallBig?.Pause();
				break;
			}
			AmbientLocationSounds.sounds.Remove(tileLocation * 64f);
		}
	}

	public static void onLocationLeave()
	{
		AmbientLocationSounds.sounds.Clear();
		AmbientLocationSounds.volumeOverrideForLocChange = -0.5f;
		AmbientLocationSounds.babblingBrook?.Pause();
		AmbientLocationSounds.cracklingFire?.Pause();
		if (AmbientLocationSounds.engine != null)
		{
			AmbientLocationSounds.engine.SetVariable("Frequency", 100f);
			AmbientLocationSounds.engine.Pause();
		}
		AmbientLocationSounds.cricket?.Pause();
		AmbientLocationSounds.waterfall?.Pause();
		AmbientLocationSounds.waterfallBig?.Pause();
	}
}
