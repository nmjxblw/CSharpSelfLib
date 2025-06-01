using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

[InstanceStatics]
public class Phone : Object
{
	/// <summary>The methods which handle incoming phone calls.</summary>
	public static List<IPhoneHandler> PhoneHandlers = new List<IPhoneHandler>
	{
		new DefaultPhoneHandler()
	};

	/// <summary>While the phone is ringing, how long each ring sound should last in milliseconds.</summary>
	public const int RING_DURATION = 600;

	/// <summary>While the phone is ringing, the delay between each ring sound in milliseconds.</summary>
	public const int RING_CYCLE_TIME = 1800;

	public static Random r;

	protected static bool _phoneSoundPlaying = false;

	public static int ringingTimer;

	public static string whichPhoneCall = null;

	public static long lastRunTick = -1L;

	public static long lastMinutesElapsedTick = -1L;

	public static int intervalsToRing = 0;

	/// <inheritdoc />
	public override string TypeDefinitionId => "(BC)";

	public Phone()
	{
	}

	public Phone(Vector2 position)
		: base(position, "214")
	{
		this.Name = "Telephone";
		base.type.Value = "Crafting";
		base.bigCraftable.Value = true;
		base.canBeSetDown.Value = true;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		string callId = Phone.whichPhoneCall;
		Phone.StopRinging();
		if (callId == null)
		{
			Game1.game1.ShowTelephoneMenu();
		}
		else if (!Phone.HandleIncomingCall(callId))
		{
			Phone.HangUp();
		}
		return true;
	}

	/// <summary>Handle an incoming phone call when the player interacts with the phone, if applicable.</summary>
	/// <param name="callId">The unique ID for the incoming call.</param>
	/// <remarks>For custom calls, add a new handler to <see cref="F:StardewValley.Objects.Phone.PhoneHandlers" /> instead.</remarks>
	public static bool HandleIncomingCall(string callId)
	{
		Action showDialogue = Phone.GetIncomingCallAction(callId);
		if (showDialogue == null)
		{
			return false;
		}
		Game1.playSound("openBox");
		Game1.player.freezePause = 500;
		DelayedAction.functionAfterDelay(showDialogue, 500);
		if (!Game1.player.callsReceived.TryGetValue(callId, out var count))
		{
			count = 0;
		}
		Game1.player.callsReceived[callId] = count + 1;
		return true;
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		if (this.Location != Game1.currentLocation)
		{
			return;
		}
		if (Game1.ticks != Phone.lastRunTick)
		{
			if (Game1.eventUp)
			{
				return;
			}
			Phone.lastRunTick = Game1.ticks;
			if (Phone.whichPhoneCall != null && Game1.shouldTimePass())
			{
				if (Phone.ringingTimer == 0)
				{
					Game1.playSound("phone");
					Phone._phoneSoundPlaying = true;
				}
				Phone.ringingTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
				if (Phone.ringingTimer >= 1800)
				{
					Phone.ringingTimer = 0;
					Phone._phoneSoundPlaying = false;
				}
			}
		}
		base.updateWhenCurrentLocation(time);
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		Phone.r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		Phone._phoneSoundPlaying = false;
		Phone.ringingTimer = 0;
		Phone.whichPhoneCall = null;
		Phone.intervalsToRing = 0;
	}

	/// <inheritdoc />
	public override bool minutesElapsed(int minutes)
	{
		if (!Game1.IsMasterGame)
		{
			return false;
		}
		if (Phone.lastMinutesElapsedTick != Game1.ticks)
		{
			Phone.lastMinutesElapsedTick = Game1.ticks;
			if (Phone.intervalsToRing == 0)
			{
				if (Phone.r == null)
				{
					Phone.r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
				}
				foreach (IPhoneHandler phoneHandler in Phone.PhoneHandlers)
				{
					string callId = phoneHandler.CheckForIncomingCall(Phone.r);
					if (callId != null)
					{
						Phone.intervalsToRing = 3;
						Game1.player.team.ringPhoneEvent.Fire(callId);
						break;
					}
				}
			}
			else
			{
				Phone.intervalsToRing--;
				if (Phone.intervalsToRing <= 0)
				{
					Game1.player.team.ringPhoneEvent.Fire(null);
				}
			}
		}
		return base.minutesElapsed(minutes);
	}

	/// <summary>Get whether the phone is currently ringing.</summary>
	public static bool IsRinging()
	{
		return Phone._phoneSoundPlaying;
	}

	/// <summary>Start ringing the phone for an incoming call.</summary>
	/// <param name="callId">The unique ID for the incoming call.</param>
	public static void Ring(string callId)
	{
		if (string.IsNullOrWhiteSpace(callId))
		{
			Phone.StopRinging();
		}
		else if (Phone.GetIncomingCallAction(callId) != null)
		{
			Phone.whichPhoneCall = callId;
			Phone.ringingTimer = 0;
			Phone._phoneSoundPlaying = false;
		}
	}

	/// <summary>Stop ringing the phone and discard the incoming call, if any.</summary>
	public static void StopRinging()
	{
		Phone.whichPhoneCall = null;
		Phone.ringingTimer = 0;
		Phone.intervalsToRing = 0;
		if (Phone.IsRinging())
		{
			Game1.soundBank.GetCue("phone").Stop(AudioStopOptions.Immediate);
			Phone._phoneSoundPlaying = false;
		}
	}

	/// <summary>Hang up the phone.</summary>
	public static void HangUp()
	{
		Phone.StopRinging();
		Game1.currentLocation.playSound("openBox");
	}

	/// <summary>Get the action to call when the player answers the phone, if the call ID is valid.</summary>
	/// <param name="callId">The unique ID for the incoming call.</param>
	public static Action GetIncomingCallAction(string callId)
	{
		foreach (IPhoneHandler phoneHandler in Phone.PhoneHandlers)
		{
			if (phoneHandler.TryHandleIncomingCall(callId, out var showDialogue))
			{
				return showDialogue;
			}
		}
		return null;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (!base.isTemporarilyInvisible)
		{
			base.draw(spriteBatch, x, y, alpha);
			bool ringing = Phone.ringingTimer > 0 && Phone.ringingTimer < 600;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destination = new Rectangle((int)position.X + ((ringing || base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)position.Y + ((ringing || base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64, 128);
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f;
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), destination, itemData.GetSourceRect(1, base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
		}
	}
}
