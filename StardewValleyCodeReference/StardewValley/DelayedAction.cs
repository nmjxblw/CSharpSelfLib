using System;
using Microsoft.Xna.Framework;

namespace StardewValley;

/// <summary>An action that will be performed after a delay.</summary>
public class DelayedAction
{
	/// <summary>The number of milliseconds until the action is invoked.</summary>
	public int timeUntilAction;

	/// <summary>An arbitrary int value saved for the action, if applicable.</summary>
	public int intData;

	/// <summary>An arbitrary float value saved for the action, if applicable.</summary>
	public float floatData;

	/// <summary>An arbitrary string value saved for the action, if applicable.</summary>
	public string stringData;

	/// <summary>An arbitrary point value saved for the action, if applicable.</summary>
	public Point pointData;

	/// <summary>An arbitrary NPC value saved for the action, if applicable.</summary>
	public NPC character;

	/// <summary>An arbitrary location value saved for the action, if applicable.</summary>
	public GameLocation location;

	/// <summary>The action to invoke.</summary>
	public Action behavior;

	/// <summary>The action to invoke after the screen is fully faded to black, if applicable.</summary>
	public Game1.afterFadeFunction afterFadeBehavior;

	/// <summary>Whether to only decrement the delay timer when there's no open menu.</summary>
	public bool waitUntilMenusGone;

	/// <summary>An arbitrary temporary animated sprite saved for the action, if applicable.</summary>
	public TemporaryAnimatedSprite temporarySpriteData;

	/// <summary>Construct an instance.</summary>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public DelayedAction(int delay)
	{
		this.timeUntilAction = delay;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	/// <param name="behavior">The action to invoke.</param>
	public DelayedAction(int delay, Action behavior)
	{
		this.timeUntilAction = delay;
		this.behavior = behavior;
	}

	/// <summary>Decrement the timer and invoke the action, if applicable in the current context.</summary>
	/// <param name="time">The current game time.</param>
	public bool update(GameTime time)
	{
		if (!this.waitUntilMenusGone || Game1.activeClickableMenu == null)
		{
			this.timeUntilAction -= time.ElapsedGameTime.Milliseconds;
			if (this.timeUntilAction <= 0)
			{
				this.behavior();
			}
		}
		return this.timeUntilAction <= 0;
	}

	/// <summary>Warp the local player to another location after a delay.</summary>
	/// <param name="targetLocation">The internal name of the target location.</param>
	/// <param name="targetTile">The target tile position.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public static void warpAfterDelay(string targetLocation, Point targetTile, int delay)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyWarp;
		action.stringData = targetLocation;
		action.pointData = targetTile;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Add a temporary animated sprite to the current location after a delay.</summary>
	/// <param name="sprite">The temporary sprite to add.</param>
	/// <param name="location">The location to which to add the sprite.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	/// <param name="waitUntilMenusGone">Whether to only decrement the delay timer when there's no open menu.</param>
	public static void addTemporarySpriteAfterDelay(TemporaryAnimatedSprite sprite, GameLocation location, int delay, bool waitUntilMenusGone = false)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyTempSprite;
		action.temporarySpriteData = sprite;
		action.location = location;
		action.waitUntilMenusGone = waitUntilMenusGone;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Play a sound after a delay, either for all player in the location (if <paramref name="location" /> is specified) else for the current player.</summary>
	/// <param name="soundName">The cue ID for the sound to play.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	/// <param name="location">The location in which to play the sound. If specified, it's played for all players in the location; otherwise it's only played for the current player.</param>
	/// <param name="position">The tile position from which to play the sound, or <c>null</c> if it should be played throughout the location.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <param name="local">Whether the sound should only be played for the local player. Ignored if <paramref name="location" /> is null.</param>
	public static void playSoundAfterDelay(string soundName, int delay, GameLocation location = null, Vector2? position = null, int pitch = -1, bool local = false)
	{
		DelayedAction action = new DelayedAction(delay);
		if (local)
		{
			action.behavior = action.ApplySoundLocal;
		}
		else
		{
			action.behavior = action.ApplySound;
		}
		action.stringData = soundName;
		action.location = location;
		action.intData = pitch;
		if (position.HasValue)
		{
			action.pointData = Utility.Vector2ToPoint(position.Value);
		}
		Game1.delayedActions.Add(action);
	}

	/// <summary>Remove a temporary animated sprite from the current location after a delay.</summary>
	/// <param name="location">The location from which to remove the sprite.</param>
	/// <param name="idOfTempSprite">The ID of the temporary sprite to remove.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public static void removeTemporarySpriteAfterDelay(GameLocation location, int idOfTempSprite, int delay)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyRemoveTemporarySprite;
		action.location = location;
		action.intData = idOfTempSprite;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Start a music track after a delay.</summary>
	/// <param name="musicName">The cue ID for the music to play.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	/// <param name="interruptable">Whether the music can be overridden by a jukebox.</param>
	public static DelayedAction playMusicAfterDelay(string musicName, int delay, bool interruptable = true)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyMusicTrack;
		action.stringData = musicName;
		action.intData = (interruptable ? 1 : 0);
		Game1.delayedActions.Add(action);
		return action;
	}

	/// <summary>Show text in a speech bubble over an NPC's head after a delay.</summary>
	/// <param name="text">The literal text to display.</param>
	/// <param name="who">The NPC over which to show the text.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public static void textAboveHeadAfterDelay(string text, NPC who, int delay)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyTextAboveHead;
		action.stringData = text;
		action.character = who;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Disable a glowing effect applied to the current player after a delay.</summary>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public static void stopFarmerGlowing(int delay)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyStopGlowing;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Show a generic dialogue message without an NPC speaker after a delay.</summary>
	/// <param name="dialogue">The dialogue text to show.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public static void showDialogueAfterDelay(string dialogue, int delay)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyDialogue;
		action.stringData = dialogue;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Show a screen flash after a delay.</summary>
	/// <param name="intensity">The intensity of the flash, as a value between 0 (transparent) and 1 (fully opaque).</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	/// <param name="sound">The cue ID for the sound to play.</param>
	public static void screenFlashAfterDelay(float intensity, int delay, string sound = null)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyScreenFlash;
		action.stringData = sound;
		action.floatData = intensity;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Remove a tile from a location's map after a delay.</summary>
	/// <param name="x">The tile's X tile position.</param>
	/// <param name="y">The tile's Y tile position.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	/// <param name="location">The location from whose map to remove the tile.</param>
	/// <param name="whichLayer">The map layer ID from which to remove the tile.</param>
	public static void removeTileAfterDelay(int x, int y, int delay, GameLocation location, string whichLayer)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyRemoveMapTile;
		action.pointData = new Point(x, y);
		action.location = location;
		action.stringData = whichLayer;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Fade the screen to black after a delay.</summary>
	/// <param name="behaviorAfterFade">The action to invoke after the screen is fully faded to black.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public static void fadeAfterDelay(Game1.afterFadeFunction behaviorAfterFade, int delay)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = action.ApplyFade;
		action.afterFadeBehavior = behaviorAfterFade;
		Game1.delayedActions.Add(action);
	}

	/// <summary>Invoke a callback after a delay.</summary>
	/// <param name="func">The action to invoke.</param>
	/// <param name="delay">The number of milliseconds until the action is invoked.</param>
	public static DelayedAction functionAfterDelay(Action func, int delay)
	{
		DelayedAction action = new DelayedAction(delay);
		action.behavior = func;
		Game1.delayedActions.Add(action);
		return action;
	}

	/// <summary>Apply the delayed screen fade to black.</summary>
	private void ApplyFade()
	{
		Game1.globalFadeToBlack(this.afterFadeBehavior);
	}

	/// <summary>Apply the delayed text over an NPC's head.</summary>
	private void ApplyTextAboveHead()
	{
		string text = this.stringData;
		if (text != null)
		{
			this.character?.showTextAboveHead(text);
		}
	}

	/// <summary>Apply the delayed temporary animated sprite addition.</summary>
	private void ApplyTempSprite()
	{
		if (this.temporarySpriteData != null)
		{
			this.location?.TemporarySprites.Add(this.temporarySpriteData);
		}
	}

	/// <summary>Apply the delayed player glow disable.</summary>
	private void ApplyStopGlowing()
	{
		Game1.player.stopGlowing();
		Game1.player.stopJittering();
		Game1.screenGlowHold = false;
		if (Game1.isFestival() && Game1.IsFall)
		{
			Game1.changeMusicTrack("fallFest");
		}
	}

	/// <summary>Apply the delayed dialogue without NPC.</summary>
	private void ApplyDialogue()
	{
		Game1.drawObjectDialogue(this.stringData);
	}

	/// <summary>Apply the delayed warp action.</summary>
	private void ApplyWarp()
	{
		string targetLocation = this.stringData;
		Point targetTile = this.pointData;
		if (targetLocation != null)
		{
			Game1.warpFarmer(targetLocation, targetTile.X, targetTile.Y, flip: false);
		}
	}

	/// <summary>Apply the delayed map tile removal.</summary>
	private void ApplyRemoveMapTile()
	{
		string layerId = this.stringData;
		Point tile = this.pointData;
		if (layerId != null)
		{
			this.location?.removeTile(tile.X, tile.Y, layerId);
		}
	}

	/// <summary>Apply the delayed temporary animated sprite removal.</summary>
	private void ApplyRemoveTemporarySprite()
	{
		int spriteId = this.intData;
		this.location?.removeTemporarySpritesWithID(spriteId);
	}

	/// <summary>Helper to apply the delayed sound action.</summary>
	private void ApplySoundHelper(bool local)
	{
		string soundId = this.stringData;
		int? pitch = ((this.intData > -1) ? new int?(this.intData) : ((int?)null));
		Vector2? position = ((this.pointData != Point.Zero) ? new Vector2?(Utility.PointToVector2(this.pointData)) : ((Vector2?)null));
		if (soundId != null)
		{
			if (this.location == null)
			{
				Game1.playSound(soundId, pitch);
			}
			else if (local)
			{
				this.location.localSound(soundId, position, pitch);
			}
			else
			{
				this.location.playSound(soundId, position, pitch);
			}
		}
	}

	/// <summary>Apply the delayed sound action.</summary>
	private void ApplySound()
	{
		this.ApplySoundHelper(local: false);
	}

	/// <summary>Apply the delayed sound action for the local player.</summary>
	private void ApplySoundLocal()
	{
		this.ApplySoundHelper(local: true);
	}

	/// <summary>Apply the delayed music action.</summary>
	private void ApplyMusicTrack()
	{
		string cueId = this.stringData;
		bool interruptable = this.intData > 0;
		if (cueId != null)
		{
			Game1.changeMusicTrack(cueId, interruptable);
		}
	}

	/// <summary>Apply the delayed screen flash.</summary>
	private void ApplyScreenFlash()
	{
		float flashAlpha = this.floatData;
		string soundId = this.stringData;
		if (!string.IsNullOrEmpty(soundId))
		{
			Game1.playSound(soundId);
		}
		Game1.flashAlpha = flashAlpha;
	}
}
